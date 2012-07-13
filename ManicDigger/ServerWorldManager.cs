using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ManicDigger;
using Vector3iG = ManicDigger.Vector3i;
using Vector3iC = ManicDigger.Vector3i;
using PointG = System.Drawing.Point;
using GameModeFortress;
using System.Diagnostics;

namespace ManicDiggerServer
{
    public partial class Server
    {
        //The main function for loading, unloadnig and sending chunks to players.
        private void NotifyMap()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            foreach (var k in clients)
            {
                if (k.Value.state == ClientStateOnServer.Connecting)
                {
                    continue;
                }
                var chunksAround = new List<Vector3i>(PlayerAreaChunks(k.Key));
                Vector3i playerpos = PlayerBlockPosition(k.Value);
                //a) if player is loading, then first generate all (LoadingGenerating), and then send all (LoadingSending)
                //b) if player is playing, then load 1, send 1.
                if (k.Value.state == ClientStateOnServer.LoadingGenerating)
                {
                    //load
                    for (int i = 0; i < chunksAround.Count; i++)
                    {
                        Vector3i v = chunksAround[i];
                        LoadChunk(v);
                        if (k.Value.state == ClientStateOnServer.LoadingGenerating)
                        {
                            //var a = PlayerArea(k.Key);
                            if (i % 10 == 0)
                            {
                                SendLevelProgress(k.Key, (int)(((float)i / chunksAround.Count) * 100), "Generating world...");
                            }
                        }
                        if (s.ElapsedMilliseconds > 10)
                        {
                            return;
                        }
                    }
                    k.Value.state = ClientStateOnServer.LoadingSending;
                }
                else if (k.Value.state == ClientStateOnServer.LoadingSending)
                {
                    //send
                    for (int i = 0; i < chunksAround.Count; i++)
                    {
                        Vector3i v = chunksAround[i];

                        if (!k.Value.chunksseen.ContainsKey(v))
                        {
                            SendChunk(k.Key, v);
                            SendLevelProgress(k.Key, (int)(((float)k.Value.maploadingsentchunks++ / chunksAround.Count) * 100), "Downloading map...");
                            if (s.ElapsedMilliseconds > 10)
                            {
                                return;
                            }
                        }
                    }
                    //Finished map loading for a connecting player.
                    bool sent_all_in_range = (k.Value.maploadingsentchunks == chunksAround.Count);
                    if (sent_all_in_range)
                    {
                        DontSpawnPlayerInWater(k.Key);
                        SendLevelFinalize(k.Key);
                        clients[k.Key].state = ClientStateOnServer.Playing;
                    }
                }
                else //b)
                {
                    chunksAround.AddRange(ChunksAroundPlayer(playerpos));
                    //chunksAround.Sort((a, b) => DistanceSquared(a, playerpos).CompareTo(DistanceSquared(b, playerpos)));
                    for (int i = 0; i < chunksAround.Count; i++)
                    {
                        Vector3i v = chunksAround[i];
                        //load
                        LoadChunk(v);
                        //send
                        if (!k.Value.chunksseen.ContainsKey(v))
                        {
                            SendChunk(k.Key, v);
                        }
                        if (s.ElapsedMilliseconds > 10)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void DontSpawnPlayerInWater(int clientId)
        {
            Vector3i pos1 = PlayerBlockPosition(clients[clientId]);
            if (IsPlayerPositionDry(pos1.x, pos1.y, pos1.z))
            {
                return;
            }
            
            //find shore
            //bonus +10 because player shouldn't be spawned too close to shore.
            bool bonusset = false;
            int bonus = -1;
            for (int i = 0; i < playerareasize / 4 - 5; i++)
            {
                Vector3i pos = PlayerBlockPosition(clients[clientId]);
                if (IsPlayerPositionDry(pos.x, pos.y, pos.z))
                {
                    if (!bonusset)
                    {
                        bonus = 10;
                        bonusset = true;
                    }
                }
                if (bonusset && bonus-- < 0)
                {
                    break;
                }
                clients[clientId].PositionMul32GlX += 32;
                int newblockheight = MapUtil.blockheight(d_Map, 0,
                    clients[clientId].PositionMul32GlX / 32,
                    clients[clientId].PositionMul32GlZ / 32);
                clients[clientId].PositionMul32GlY = newblockheight * 32 + 16;
            }
            foreach (var k in clients)
            {
                int clientId2 = clientId;
                if (k.Key == clientId)
                {
                    clientId2 = 255;
                }
                SendPlayerTeleport(k.Key, (byte)clientId2, k.Value.PositionMul32GlX, k.Value.PositionMul32GlY,
                    k.Value.PositionMul32GlZ, (byte)k.Value.positionheading, (byte)k.Value.positionpitch);
            }
        }

        bool IsPlayerPositionDry(int x, int y, int z)
        {
            for (int i = 0; i < 4; i++)
            {
                if (MapUtil.IsValidPos(d_Map, x, y, z - i))
                {
                    int blockUnderPlayer = d_Map.GetBlock(x, y, z - i);
                    if (d_Data.IsFluid[blockUnderPlayer])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        void SendChunk(int clientid, Vector3i v)
        {
            Client c = clients[clientid];
            byte[] chunk = d_Map.GetChunk(v.x, v.y, v.z);
            c.chunksseen[v] = (int)simulationcurrentframe;
            //sent++;
            byte[] compressedchunk;
            if (MapUtil.IsSolidChunk(chunk) && chunk[0] == 0)
            {
                //don't send empty chunk.
                compressedchunk = null;
            }
            else
            {
                compressedchunk = CompressChunkNetwork(chunk);
                //todo!
                //commented because it was being sent too early, before full column was generated.
                //if (!c.heightmapchunksseen.ContainsKey(new Vector2i(v.x, v.y)))
                {
                    byte[] heightmapchunk = d_Map.GetHeightmapChunk(v.x, v.y);
                    byte[] compressedHeightmapChunk = d_NetworkCompression.Compress(heightmapchunk);
                    PacketServerHeightmapChunk p1 = new PacketServerHeightmapChunk()
                    {
                        X = v.x,
                        Y = v.y,
                        SizeX = chunksize,
                        SizeY = chunksize,
                        CompressedHeightmap = compressedHeightmapChunk,
                    };
                    SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.HeightmapChunk, HeightmapChunk = p1 }));
                    c.heightmapchunksseen[new Vector2i(v.x, v.y)] = (int)simulationcurrentframe;
                }
            }
            PacketServerChunk p = new PacketServerChunk()
            {
                X = v.x,
                Y = v.y,
                Z = v.z,
                SizeX = chunksize,
                SizeY = chunksize,
                SizeZ = chunksize,
                CompressedChunk = compressedchunk,
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.Chunk, Chunk = p }));
        }

        int playerareasize = 256;
        int centerareasize = 128;

        PointG PlayerArea(int playerId)
        {
            return MapUtil.PlayerArea(playerareasize, centerareasize, PlayerBlockPosition(clients[playerId]));
        }

        IEnumerable<Vector3iG> PlayerAreaChunks(int playerId)
        {
            PointG p = PlayerArea(playerId);
            for (int x = 0; x < playerareasize / chunksize; x++)
            {
                for (int y = 0; y < playerareasize / chunksize; y++)
                {
                    for (int z = 0; z < d_Map.MapSizeZ / chunksize; z++)
                    {
                        var v = new Vector3i(p.X + x * chunksize, p.Y + y * chunksize, z * chunksize);
                        if (MapUtil.IsValidPos(d_Map, v.x, v.y, v.z))
                        {
                            yield return v;
                        }
                    }
                }
            }
        }
        // Interfaces to manipulate server's map.
        public void SetBlock(int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                SetBlockAndNotify(x, y, z, blocktype);
            }
        }
        public int GetBlock(int x, int y, int z)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                return d_Map.GetBlock(x, y, z);
            }
            return 0;
        }
        public int GetHeight(int x, int y)
        {
            return MapUtil.blockheight(d_Map, 0, x, y);
        }
        public void SetChunk(int x, int y, int z, byte[] data)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                x = x / chunksize;
                y = y / chunksize;
                z = z / chunksize;
                Chunk c = d_Map.chunks[x,y,z];
                if (c == null)
                {
                    c = new Chunk();
                }
                c.data = data;
                c.DirtyForSaving = true;
                d_Map.chunks[x,y,z] = c;
                // update related chunk at clients
                foreach (var k in clients)
                {
                    k.Value.chunksseen.Clear();
                }
            }
        }
        public byte[] GetChunk(int x, int y, int z)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                x = x / chunksize;
                y = y / chunksize;
                z = z / chunksize;
                return d_Map.chunks[x,y,z].data;
            }
            return null;
        }
        public void DeleteChunk(int x, int y, int z)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                x = x / chunksize;
                y = y / chunksize;
                z = z / chunksize;
                ChunkDb.DeleteChunk(d_ChunkDb, x, y, z);
                d_Map.chunks[x,y,z] = null;
                // update related chunk at clients
                foreach (var k in clients)
                {
                    k.Value.chunksseen.Clear();
                }
            }
        }
        public void DeleteChunks(List<Vector3i> chunkPositions)
        {
            List<Xyz> chunks = new List<Xyz>();
            foreach (Vector3i pos in chunkPositions)
            {
                if (MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z))
                {
                    int x = pos.x / chunksize;
                    int y = pos.y / chunksize;
                    int z = pos.z / chunksize;
                    d_Map.chunks[x,y,z] = null;
                    chunks.Add(new Xyz(){X = x, Y = y, Z = z});
                }
            }
            if (chunks.Count != 0)
            {
                ChunkDb.DeleteChunks(d_ChunkDb, chunks);
                // force to update chunks at clients
                foreach (var k in clients)
                {
                    k.Value.chunksseen.Clear();
                }
            }
        }
        public int[] GetMapSize()
        {
            return new int[] {d_Map.MapSizeX, d_Map.MapSizeY, d_Map.MapSizeZ};
        }
    }
}

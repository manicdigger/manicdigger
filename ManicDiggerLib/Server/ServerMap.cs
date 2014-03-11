#region Using Statements
using System;
using System.IO;
using GameModeFortress;
using ManicDigger;
using ProtoBuf;
using System.Collections.Generic;
using Jint.Delegates;
#endregion

namespace ManicDiggerServer
{
    [ProtoContract()]
    public class Monster
    {
        [ProtoMember(1, IsRequired = false)]
        public int Id;
        [ProtoMember(2, IsRequired = false)]
        public int MonsterType;
        [ProtoMember(3, IsRequired = false)]
        public int X;
        [ProtoMember(4, IsRequired = false)]
        public int Y;
        [ProtoMember(5, IsRequired = false)]
        public int Z;
        public int Health;
        public Vector3i WalkDirection;
        public float WalkProgress = 0;
    }
    [ProtoContract()]
    public class Chunk
    {
        [ProtoMember(1, IsRequired = false)]
        public byte[] dataOld;
        [ProtoMember(6, IsRequired = false)]
        public ushort[] data;
        [ProtoMember(2, IsRequired = false)]
        public long LastUpdate;
        [ProtoMember(3, IsRequired = false)]
        public bool IsPopulated;
        [ProtoMember(4, IsRequired = false)]
        public int LastChange;
        public bool DirtyForSaving;
        [ProtoMember(5, IsRequired = false)]
        public List<Monster> Monsters = new List<Monster>();
    }
    public class ServerMap : IMapStorage
    {
        public Server server;
        public IChunkDb d_ChunkDb;
        public ICurrentTime d_CurrentTime;
        public Chunk[, ,] chunks;
        public GameData d_Data;
        public bool wasChunkGenerated;
        #region IMapStorage Members
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        public int GetBlock(int x, int y, int z)
        {
            ushort[] chunk = GetChunk(x, y, z);
            return chunk[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)];
        }
        public void SetBlock(int x, int y, int z, int tileType)
        {
            ushort[] chunk = GetChunk(x, y, z);
            chunk[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (byte)tileType;
            chunks[x / chunksize, y / chunksize, z / chunksize].LastChange = d_CurrentTime.SimulationCurrentFrame;
            chunks[x / chunksize, y / chunksize, z / chunksize].DirtyForSaving = true;
            UpdateColumnHeight(x, y);
        }
        private void UpdateColumnHeight(int x, int y)
        {
            //todo faster
            int height = MapSizeZ - 1;
            for (int i = MapSizeZ - 1; i >= 0; i--)
            {
                height = i;
                if (MapUtil.IsValidPos(this, x, y, i) && !Server.IsTransparentForLight(server.BlockTypes[GetBlock(x, y, i)]))
                {
                    break;
                }
            }
            d_Heightmap.SetBlock(x, y, height);
        }
        public void SetBlockNotMakingDirty(int x, int y, int z, int tileType)
        {
            ushort[] chunk = GetChunk(x, y, z);
            chunk[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (byte)tileType;
            chunks[x / chunksize, y / chunksize, z / chunksize].DirtyForSaving = true;
            UpdateColumnHeight(x, y);
        }
        public float WaterLevel { get; set; }
        public void UseMap(byte[, ,] map)
        {
        }
        #endregion
        public void LoadChunk(int cx, int cy, int cz)
        {
            Chunk chunk = chunks[cx, cy, cz];
            if (chunk == null)
            {
                GetChunk(cx * chunksize, cy * chunksize, cz * chunksize);
            }
        }
        public ushort[] GetChunk(int x, int y, int z)
        {
            x = x / chunksize;
            y = y / chunksize;
            z = z / chunksize;
            Chunk chunk = chunks[x, y, z];
            if (chunk == null)
            {
                wasChunkGenerated = true;
                byte[] serializedChunk = ChunkDb.GetChunk(d_ChunkDb, x, y, z);
                if (serializedChunk != null)
                {
                    chunks[x, y, z] = DeserializeChunk(serializedChunk);
                    //todo get heightmap from disk
                    UpdateChunkHeight(x, y, z);
                    return chunks[x, y, z].data;
                }

                // get chunk
                ushort[] newchunk = new ushort[chunksize * chunksize * chunksize];
                for (int i = 0; i < server.modEventHandlers.getchunk.Count; i++)
                {
                    server.modEventHandlers.getchunk[i](x, y, z, newchunk);
                }
                chunks[x, y, z] = new Chunk() { data = newchunk };
                chunks[x, y, z].DirtyForSaving = true;
                UpdateChunkHeight(x, y, z);
                return chunks[x, y, z].data;
            }
            return chunk.data;
        }
        private void UpdateChunkHeight(int x, int y, int z)
        {
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    //UpdateColumnHeight(x * chunksize + xx, y * chunksize + yy);
                    
                    int inChunkHeight = GetColumnHeightInChunk(chunks[x, y, z].data, xx, yy);
                    if (inChunkHeight != 0)//not empty column
                    {
                        int oldHeight = d_Heightmap.GetBlock(x * chunksize + xx, y * chunksize + yy);
                        d_Heightmap.SetBlock(x * chunksize + xx, y * chunksize + yy, Math.Max(oldHeight, inChunkHeight + z * chunksize));
                    }
                }
            }
        }
        private int GetColumnHeightInChunk(ushort[] chunk, int xx, int yy)
        {
            int height = chunksize - 1;
            for (int i = chunksize - 1; i >= 0; i--)
            {
                height = i;
                if (!Server.IsTransparentForLight(server.BlockTypes[chunk[MapUtil.Index3d(xx, yy, i, chunksize, chunksize)]]))
                {
                    break;
                }
            }
            return height;
        }
        private Chunk DeserializeChunk(byte[] serializedChunk)
        {
            Chunk c = Serializer.Deserialize<Chunk>(new MemoryStream(serializedChunk));
            //convert savegame to new format
            if (c.dataOld != null)
            {
                c.data = new ushort[chunksize * chunksize * chunksize];
                for (int i = 0; i < c.dataOld.Length; i++)
                {
                    c.data[i] = c.dataOld[i];
                }
                c.dataOld = null;
            }
            return c;
        }
        public int chunksize = 16;
        public void Reset(int sizex, int sizey, int sizez)
        {
            MapSizeX = sizex;
            MapSizeY = sizey;
            MapSizeZ = sizez;
            chunks = new Chunk[sizex / chunksize, sizey / chunksize, sizez / chunksize];
            d_Heightmap.Restart();
        }
        #region IMapStorage Members
        public void SetChunk(int x, int y, int z, ushort[, ,] chunk)
        {
            int chunksizex = chunk.GetUpperBound(0) + 1;
            int chunksizey = chunk.GetUpperBound(1) + 1;
            int chunksizez = chunk.GetUpperBound(2) + 1;
            if (chunksizex % chunksize != 0) { throw new ArgumentException(); }
            if (chunksizey % chunksize != 0) { throw new ArgumentException(); }
            if (chunksizez % chunksize != 0) { throw new ArgumentException(); }
            ushort[, ,][] localchunks = new ushort[chunksizex / chunksize, chunksizey / chunksize, chunksizez / chunksize][];
            for (int cx = 0; cx < chunksizex / chunksize; cx++)
            {
                for (int cy = 0; cy < chunksizey / chunksize; cy++)
                {
                    for (int cz = 0; cz < chunksizex / chunksize; cz++)
                    {
                        localchunks[cx, cy, cz] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
                        FillChunk(localchunks[cx, cy, cz], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk);
                    }
                }
            }
            for (int xxx = 0; xxx < chunksizex; xxx += chunksize)
            {
                for (int yyy = 0; yyy < chunksizex; yyy += chunksize)
                {
                    for (int zzz = 0; zzz < chunksizex; zzz += chunksize)
                    {
                        chunks[(x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize].LastChange = d_CurrentTime.SimulationCurrentFrame;
                    }
                }
            }
        }
        private void FillChunk(ushort[] destination, int destinationchunksize,
            int sourcex, int sourcey, int sourcez, ushort[, ,] source)
        {
            for (int x = 0; x < destinationchunksize; x++)
            {
                for (int y = 0; y < destinationchunksize; y++)
                {
                    for (int z = 0; z < destinationchunksize; z++)
                    {
                        //if (x + sourcex < source.GetUpperBound(0) + 1
                        //    && y + sourcey < source.GetUpperBound(1) + 1
                        //    && z + sourcez < source.GetUpperBound(2) + 1)
                        {
                            destination[MapUtil.Index3d(x, y, z, destinationchunksize, destinationchunksize)]
                                = source[x + sourcex, y + sourcey, z + sourcez];
                        }
                    }
                }
            }
        }
        #endregion
        public InfiniteMapChunked2dServer d_Heightmap;
        public unsafe ushort[] GetHeightmapChunk(int x, int y)
        {
            //todo don't copy
            ushort[] chunk2d = d_Heightmap.GetChunk(x, y);
            ushort[] chunk = new ushort[chunksize * chunksize];
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    chunk[MapUtil.Index2d(xx, yy, chunksize)] = chunk2d[MapUtil.Index2d(xx, yy, chunksize)];
                }
            }
            //todo ushort[]
            return chunk;
        }
    }
}

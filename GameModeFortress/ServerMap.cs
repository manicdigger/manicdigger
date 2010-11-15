using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using GameModeFortress;
using ProtoBuf;
using System.IO;

namespace ManicDiggerServer
{
    [ProtoContract()]
    public class Chunk
    {
        [ProtoMember(1, IsRequired = false)]
        public byte[] data;
        [ProtoMember(2, IsRequired = false)]
        public long LastUpdate;
        [ProtoMember(3, IsRequired = false)]
        public bool IsPopulated;
        [ProtoMember(4, IsRequired = false)]
        public int LastChange;
    }
    public class ServerMap : IMapStorage
    {
        [Inject]
        public IChunkDb chunkdb;
        [Inject]
        public IWorldGenerator generator;
        [Inject]
        public ICurrentTime currenttime;
        public Chunk[, ,] chunks;
        #region IMapStorage Members
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        public int GetBlock(int x, int y, int z)
        {
            byte[] chunk = GetChunk(x, y, z);
            return chunk[MapUtil.Index(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)];
        }
        public void SetBlock(int x, int y, int z, int tileType)
        {
            byte[] chunk = GetChunk(x, y, z);
            chunk[MapUtil.Index(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (byte)tileType;
            chunks[x / chunksize, y / chunksize, z / chunksize].LastChange = currenttime.SimulationCurrentFrame;
        }
        public void SetBlockNotMakingDirty(int x, int y, int z, int tileType)
        {
            byte[] chunk = GetChunk(x, y, z);
            chunk[MapUtil.Index(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (byte)tileType;
        }
        public float WaterLevel { get; set; }
        public void Dispose()
        {
        }
        public void UseMap(byte[, ,] map)
        {
        }
        #endregion
        public byte[] GetChunk(int x, int y, int z)
        {
            x = x / chunksize;
            y = y / chunksize;
            z = z / chunksize;
            Chunk chunk = chunks[x, y, z];
            if (chunk == null)
            {
                byte[] serializedChunk = ChunkDb.GetChunk(chunkdb, x, y, z);
                if (serializedChunk != null)
                {
                    chunks[x, y, z] = DeserializeChunk(serializedChunk);
                    return chunks[x, y, z].data;
                }
                //byte[, ,] newchunk = new byte[chunksize, chunksize, chunksize];
                byte[, ,] newchunk = generator.GetChunk(x, y, z, chunksize);
                if (newchunk != null)
                {
                    chunks[x, y, z] = new Chunk() { data = MapUtil.ToFlatMap(newchunk) };
                }
                else
                {
                    chunks[x, y, z] = new Chunk() { data = new byte[chunksize * chunksize * chunksize] };
                }
                return chunks[x, y, z].data;
            }
            return chunk.data;
        }
        private Chunk DeserializeChunk(byte[] serializedChunk)
        {
            return Serializer.Deserialize<Chunk>(new MemoryStream(serializedChunk));
        }
        public int chunksize = 16;
        public void Reset(int sizex, int sizey, int sizez)
        {
            MapSizeX = sizex;
            MapSizeY = sizey;
            MapSizeZ = sizez;
            chunks = new Chunk[sizex / chunksize, sizey / chunksize, sizez / chunksize];
        }
        #region IMapStorage Members
        public void SetChunk(int x, int y, int z, byte[, ,] chunk)
        {
            int chunksizex = chunk.GetUpperBound(0) + 1;
            int chunksizey = chunk.GetUpperBound(1) + 1;
            int chunksizez = chunk.GetUpperBound(2) + 1;
            if (chunksizex % chunksize != 0) { throw new ArgumentException(); }
            if (chunksizey % chunksize != 0) { throw new ArgumentException(); }
            if (chunksizez % chunksize != 0) { throw new ArgumentException(); }
            byte[, ,][] localchunks = new byte[chunksizex / chunksize, chunksizey / chunksize, chunksizez / chunksize][];
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
                        chunks[(x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize].LastChange = currenttime.SimulationCurrentFrame;
                    }
                }
            }
        }
        private void FillChunk(byte[] destination, int destinationchunksize,
            int sourcex, int sourcey, int sourcez, byte[, ,] source)
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
                            destination[MapUtil.Index(x, y, z, destinationchunksize, destinationchunksize)]
                                = source[x + sourcex, y + sourcey, z + sourcez];
                        }
                    }
                }
            }
        }
        #endregion
    }
}

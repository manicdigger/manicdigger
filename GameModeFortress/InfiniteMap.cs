using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace GameModeFortress
{
    public class InfiniteMapChunked : IMapStorage, IIsChunkReady
    {
        public class Chunk
        {
            public byte[] data;
            public byte[] compressed;
            public long LastUpdate;
        }
        [Inject]
        public IWorldGenerator generator { get; set; }
        public Chunk[, ,] chunks;
        bool[, ,] chunksreceived;
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
            if (chunk.compressed != null)
            {
                chunk.data = GzipCompression.Decompress(chunk.compressed);
                chunk.compressed = null;
            }
            return chunk.data;
        }
        public int chunksize = 16;
        public void Reset(int sizex, int sizey, int sizez)
        {
            MapSizeX = sizex;
            MapSizeY = sizey;
            MapSizeZ = sizez;
            chunks = new Chunk[sizex / chunksize, sizey / chunksize, sizez / chunksize];
            chunksreceived = new bool[sizex / chunksize, sizey / chunksize, sizez / chunksize];
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
            for (int xxx = 0; xxx < chunksizex; xxx += chunksize)
            {
                for (int yyy = 0; yyy < chunksizex; yyy += chunksize)
                {
                    for (int zzz = 0; zzz < chunksizex; zzz += chunksize)
                    {
                        //if (!chunksreceived[(x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize])
                        {
                            chunksreceived[(x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize] = true;
                        }
                    }
                }
            }
            byte[, ,][] localchunks = new byte[chunksizex / chunksize, chunksizey / chunksize, chunksizez / chunksize][];
            for (int cx = 0; cx < chunksizex / chunksize; cx++)
            {
                for (int cy = 0; cy < chunksizey / chunksize; cy++)
                {
                    for (int cz = 0; cz < chunksizex / chunksize; cz++)
                    {
                        localchunks[cx, cy, cz] = GetChunk(x + cx*chunksize, y + cy*chunksize, z + cz*chunksize);
                        FillChunk(localchunks[cx, cy, cz], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk);
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
        #region IIsChunkReady Members
        public bool IsChunkReady(int x, int y, int z)
        {
            return chunksreceived[x / chunksize, y / chunksize, z / chunksize];
        }
        #endregion
    }
}

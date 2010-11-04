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
            public bool IsPopulated;
        }
        [Inject]
        public IWorldGenerator generator { get; set; }
        public Chunk[, ,] chunks;
        bool[, ,] notchunksdirty;
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
            notchunksdirty[x / chunksize, y / chunksize, z / chunksize] = true;
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
            SetAllChunksDirty();
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
                            notchunksdirty[(x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize] = false;
                            SetChunksAroundDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize);
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
                        localchunks[cx, cy, cz] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
                        FillChunk(localchunks[cx, cy, cz], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk);
                    }
                }
            }
        }
        private void SetChunksAroundDirty(int x, int y, int z)
        {
            if (IsValidChunkPosition(x, y, z)) { SetChunkDirty(x - 1, y, z, true); }
            if (IsValidChunkPosition(x - 1, y, z)) { SetChunkDirty(x - 1, y, z, true); }
            if (IsValidChunkPosition(x + 1, y, z)) { SetChunkDirty(x + 1, y, z, true); }
            if (IsValidChunkPosition(x, y - 1, z)) { SetChunkDirty(x, y - 1, z, true); }
            if (IsValidChunkPosition(x, y + 1, z)) { SetChunkDirty(x, y + 1, z, true); }
            if (IsValidChunkPosition(x, y, z - 1)) { SetChunkDirty(x, y, z - 1, true); }
            if (IsValidChunkPosition(x, y, z + 1)) { SetChunkDirty(x, y, z + 1, true); }
        }
        private bool IsValidChunkPosition(int xx, int yy, int zz)
        {
            return xx >= 0 && yy >= 0 && zz >= 0
                && xx < MapSizeX / chunksize
                && yy < MapSizeY / chunksize
                && zz < MapSizeZ / chunksize;
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
            return !notchunksdirty[x / chunksize, y / chunksize, z / chunksize];
        }
        #endregion
        #region IIsChunkReady Members
        public bool IsChunkDirty(int x, int y, int z)
        {
            return !notchunksdirty[x, y, z];
        }
        public void SetChunkDirty(int x, int y, int z, bool dirty)
        {
            notchunksdirty[x, y, z] = !dirty;
        }
        #endregion
        #region IIsChunkReady Members
        public void SetAllChunksDirty()
        {
            notchunksdirty = new bool[MapSizeX / chunksize, MapSizeY / chunksize, MapSizeZ / chunksize];
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace GameModeFortress
{
    public class InfiniteMapChunked : IMapStorage, IMapStoragePortion
    {
        public class Chunk
        {
            public byte[] data;
            public int LastUpdate;
            public bool IsPopulated;
            public int LastChange;
        }
        //[Inject]
        //public IWorldGenerator generator;
        [Inject]
        public IIsChunkDirty d_IsChunkReady;
        public Chunk[] chunks;
        #region IMapStorage Members
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        public int GetBlock(int x, int y, int z)
        {
            int cx = x / chunksize;
            int cy = y / chunksize;
            int cz = z / chunksize;
            Chunk chunk = chunks[MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize)];
            if (chunk == null)
            {
                return 0;
            }
            return chunk.data[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)];
        }
        public void SetBlock(int x, int y, int z, int tileType)
        {
            byte[] chunk = GetChunk(x, y, z);
            chunk[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (byte)tileType;
            SetChunkDirty(x / chunksize, y / chunksize, z / chunksize, true);
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
            int mapsizexchunks = MapSizeX / chunksize;
            int mapsizeychunks = MapSizeY / chunksize;
            Chunk chunk = chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
            if (chunk == null)
            {
                //byte[, ,] newchunk = new byte[chunksize, chunksize, chunksize];
                //byte[, ,] newchunk = generator.GetChunk(x, y, z, chunksize);
                //if (newchunk != null)
                //{
                //    chunks[x, y, z] = new Chunk() { data = MapUtil.ToFlatMap(newchunk) };
                //}
                //else
                {
                    chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)] = new Chunk() { data = new byte[chunksize * chunksize * chunksize] };
                }
                return chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)].data;
            }
            return chunk.data;
        }
        public int chunksize = 16;
        public void Reset(int sizex, int sizey, int sizez)
        {
            MapSizeX = sizex;
            MapSizeY = sizey;
            MapSizeZ = sizez;
            chunks = new Chunk[(sizex / chunksize) * (sizey / chunksize) * (sizez / chunksize)];
            SetAllChunksNotDirty();
        }
        #region IMapStorage Members
        public void SetMapPortion(int x, int y, int z, byte[, ,] chunk)
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
                        SetChunkDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize, true);
                        SetChunksAroundDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize);
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
                            destination[MapUtil.Index3d(x, y, z, destinationchunksize, destinationchunksize)]
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
            return IsChunkDirty(x, y, z);
        }
        #endregion
        #region IIsChunkReady Members
        public bool IsChunkDirty(int x, int y, int z)
        {
            return d_IsChunkReady.IsChunkDirty(x, y, z);
        }
        public void SetChunkDirty(int x, int y, int z, bool dirty)
        {
            d_IsChunkReady.SetChunkDirty(x, y, z, dirty);
        }
        #endregion
        public void SetAllChunksNotDirty()
        {
            d_IsChunkReady.SetAllChunksNotDirty();
        }
        public void GetMapPortion(byte[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
        {
            Array.Clear(outPortion, 0, outPortion.Length);

            int chunksizebits = (int)Math.Log(chunksize, 2);

            for (int xx = 0; xx < portionsizex; xx++)
            {
                for (int yy = 0; yy < portionsizey; yy++)
                {
                    for (int zz = 0; zz < portionsizez; zz++)
                    {
                        //Find chunk.
                        int cx = (x + xx) >> chunksizebits;
                        int cy = (y + yy) >> chunksizebits;
                        int cz = (z + zz) >> chunksizebits;
                        int cpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
                        if (cpos < 0 || cpos >= ((MapSizeX / chunksize) * (MapSizeY / chunksize) * (MapSizeZ / chunksize)))
                        {
                            continue;
                        }
                        Chunk chunk = chunks[cpos];
                        if (chunk == null || chunk.data == null)
                        {
                            continue;
                        }
                        int pos = MapUtil.Index3d((x + xx) % chunksize, (y + yy) % chunksize, (z + zz) % chunksize, chunksize, chunksize);
                        int block = chunk.data[pos];
                        outPortion[MapUtil.Index3d(xx, yy, zz, portionsizex, portionsizey)] = (byte)block;
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ManicDigger.Renderers;

namespace ManicDigger
{
    public interface IShadows : IShadowsGetLight
    {
        void OnLocalBuild(int x, int y, int z);
        void OnSetBlock(int x, int y, int z);
        void ResetShadows();
        int GetLight(int x, int y, int z);
        void OnGetTerrainBlock(int x, int y, int z);
        void OnMakeChunk(int chunkx, int chunky, int chunkz);
        int sunlight { get; set; }
        void OnSetChunk(int x, int y, int z);
    }
    public interface IShadowsGetLight
    {
        int GetLight(int x, int y, int z);
        int? MaybeGetLight(int x, int y, int z);
        int maxlight { get; }
    }
    public class InfiniteMapChunked2d
    {
        [Inject]
        public IMapStorage d_Map;
        public int chunksize = 16;
        byte[,][,] chunks;
        public int GetBlock(int x, int y)
        {
            byte[,] chunk = GetChunk(x, y);
            return chunk[x % chunksize, y % chunksize];
        }
        public byte[,] GetChunk(int x, int y)
        {
            byte[,] chunk = null;
            int kx = x / chunksize;
            int ky = y / chunksize;
            if (chunks[kx, ky] == null)
            {
                chunk = new byte[chunksize, chunksize];
                chunks[kx, ky] = chunk;
            }
            chunk = chunks[kx, ky];
            return chunk;
        }
        public void SetBlock(int x, int y, int blocktype)
        {
            GetChunk(x, y)[x % chunksize, y % chunksize] = (byte)blocktype;
        }
        public void Restart()
        {
            chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
        }
        public void ClearChunk(int x, int y)
        {
            int px = x / chunksize;
            int py = y / chunksize;
            chunks[px, py] = null;
        }
    }
    public class InfiniteMapChunkedSimple
    {
        [Inject]
        public IMapStorage d_Map;
        int chunksize = 16;
        byte[, ,][, ,] chunks;
        public int GetBlock(int x, int y, int z)
        {
            byte[, ,] chunk = GetChunk(x, y, z);
            return chunk[x % chunksize, y % chunksize, z % chunksize];
        }
        public byte[, ,] GetChunk(int x, int y, int z)
        {
            int cx = x / chunksize;
            int cy = y / chunksize;
            int cz = z / chunksize;
            byte[, ,] chunk = chunks[cx, cy, cz];
            if (chunk == null)
            {
                chunk = new byte[chunksize, chunksize, chunksize];
                chunks[cx, cy, cz] = chunk;
            }
            return chunk;
        }
        public void SetBlock(int x, int y, int z, int blocktype)
        {
            GetChunk(x, y, z)[x % chunksize, y % chunksize, z % chunksize] = (byte)blocktype;
        }
        public void ClearChunk(int x, int y, int z)
        {
            if (!MapUtil.IsValidPos(d_Map, x, y, z))
            {
                return;
            }
            int cx = x / chunksize;
            int cy = y / chunksize;
            int cz = z / chunksize;
            chunks[cx, cy, cz] = new byte[chunksize, chunksize, chunksize];
        }
        public void Restart()
        {
            chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize, d_Map.MapSizeZ / chunksize][, ,];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ManicDigger.Renderers;
using System.Runtime.InteropServices;

namespace ManicDigger
{
    public class Shadows3x3x3Simple : IShadows3x3x3
    {
        public override void Start()
        {
        }

        const int shadowlight = 10;
        const int maxlight = 15;

        public override void Update(byte[] outputChunkLight, int[][] inputMapChunks, int[][] inputHeightmapChunks, int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight)
        {
            this.inputHeightmapChunks = inputHeightmapChunks;

            for (int i = 0; i < outputChunkLight.Length; i++)
            {
                outputChunkLight[i] = (byte)shadowlight;
            }
            int zplus = 18 * 18;
            for (int xx = 0; xx < 18; xx++)
            {
                for (int yy = 0; yy < 18; yy++)
                {
                    int height = GetLightHeight(16 - 1 + xx, 16 - 1 + yy) - 16;
                    int h = height - baseheight;
                    if (h < 0) { h = 0; }
                    if (h > 18) { continue; }
                    int pos = MapUtil.Index3d(xx, yy, h, 18, 18);
                    /*
                    for (int zz = 0; zz < h; zz++)
                    {
                        worklight[pos] = (byte)minlight;
                        pos += zplus;
                    }
                    */
                    for (int zz = h; zz < 18; zz++)
                    {
                        //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                        outputChunkLight[pos] = (byte)maxlight;
                        pos += zplus;
                    }
                }
            }
        }
        int[][] inputHeightmapChunks;
        int chunksize = 16;
        int GetLightHeight(int xx, int yy)
        {
            int[] chunk = inputHeightmapChunks[MapUtil.Index2d(xx / chunksize, yy / chunksize, 3)];
            if (chunk == null)
            {
                //throw new Exception();
                //return 64;
                return 0;
            }
            return chunk[MapUtil.Index2d(xx % chunksize, yy % chunksize, chunksize)];
        }
    }
    public interface IShadowsGetLight
    {
        int GetLight(int x, int y, int z);
        int? MaybeGetLight(int x, int y, int z);
        int maxlight { get; }
    }
    public class InfiniteMapChunked2dServer
    {
        [Inject]
        public IMapStorage d_Map;
        public int chunksize = 16;
        public ushort[][] chunks;
        public unsafe int GetBlock(int x, int y)
        {
            ushort[] chunk = GetChunk(x, y);
            return chunk[MapUtil.Index2d(x % chunksize, y % chunksize, chunksize)];
        }
        public ushort[] GetChunk(int x, int y)
        {
            ushort[] chunk = null;
            int kx = x / chunksize;
            int ky = y / chunksize;
            if (chunks[MapUtil.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] == null)
            {
                chunk = new ushort[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
                for (int i = 0; i < chunksize * chunksize; i++)
                {
                    chunk[i] = 0;
                }
                chunks[MapUtil.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] = chunk;
            }
            chunk = chunks[MapUtil.Index2d(kx, ky, d_Map.MapSizeX / chunksize)];
            return chunk;
        }
        public unsafe void SetBlock(int x, int y, int blocktype)
        {
            GetChunk(x, y)[MapUtil.Index2d(x % chunksize, y % chunksize, chunksize)] = (byte)blocktype;
        }
        public unsafe void Restart()
        {
            //chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
            int n = (d_Map.MapSizeX / chunksize) * (d_Map.MapSizeY / chunksize);
            chunks = new ushort[n][];//(byte**)Marshal.AllocHGlobal(n * sizeof(IntPtr));
            for (int i = 0; i < n; i++)
            {
                chunks[i] = null;
            }
        }
        public unsafe void ClearChunk(int x, int y)
        {
            int px = x / chunksize;
            int py = y / chunksize;
            chunks[MapUtil.Index2d(px, py, d_Map.MapSizeX / chunksize)] = null;
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

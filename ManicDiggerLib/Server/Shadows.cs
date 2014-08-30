using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ManicDigger.Renderers;
using System.Runtime.InteropServices;

public class InfiniteMapChunked2dServer
{
    public IMapStorage2 d_Map;
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
        if (chunks[MapUtil.Index2d(kx, ky, d_Map.GetMapSizeX() / chunksize)] == null)
        {
            chunk = new ushort[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
            for (int i = 0; i < chunksize * chunksize; i++)
            {
                chunk[i] = 0;
            }
            chunks[MapUtil.Index2d(kx, ky, d_Map.GetMapSizeX() / chunksize)] = chunk;
        }
        chunk = chunks[MapUtil.Index2d(kx, ky, d_Map.GetMapSizeX() / chunksize)];
        return chunk;
    }
    public unsafe void SetBlock(int x, int y, int blocktype)
    {
        GetChunk(x, y)[MapUtil.Index2d(x % chunksize, y % chunksize, chunksize)] = (byte)blocktype;
    }
    public unsafe void Restart()
    {
        //chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
        int n = (d_Map.GetMapSizeX() / chunksize) * (d_Map.GetMapSizeY() / chunksize);
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
        chunks[MapUtil.Index2d(px, py, d_Map.GetMapSizeX() / chunksize)] = null;
    }
}

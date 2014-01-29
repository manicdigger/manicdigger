using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ManicDigger.Renderers;
using System.Runtime.InteropServices;

namespace ManicDigger
{
    public interface IShadows3x3x3
    {
        void Start();
        unsafe void Update(byte[] outputChunkLight,
            int[][] inputMapChunks, int[][] inputHeightmapChunks,
            int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight);
    }
    public class Shadows3x3x3 : IShadows3x3x3
    {
        public Shadows3x3x3()
        {
            workportionArr = new int[16 * 16 * 16 * 3 * 3 * 3];
            worklightArr = new byte[16 * 16 * 16 * 3 * 3 * 3];
            q.Initialize(1024);
            lighttoflood.Initialize(1024);
        }
        public void Start()
        {
        }
        public unsafe void Update(byte[] outputChunkLight,
            int[][] inputMapChunks, int[][] inputHeightmapChunks,
            int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight)
        {
            this.outputChunkLight = outputChunkLight;
            this.inputMapChunks = inputMapChunks;
            this.inputHeightmapChunks = inputHeightmapChunks;
            this.dataLightRadius = dataLightRadius;
            this.dataTransparent = dataTransparent;
            this.sunlight = currentSunlight;
            this.baseheight = baseheight;

            lighttoflood.Clear();
            Array.Clear(workportionArr, 0, workportionArr.Length);
            GetPortion();
            Array.Clear(worklightArr, 0, worklightArr.Length);
            ApplySunlight();
            ApplyLightEmitting();
            FloodSunlight();
            FloodLight();
            SetPortion();
        }

        byte[] outputChunkLight;
        int[][] inputMapChunks;
        int[][] inputHeightmapChunks;
        int[] dataLightRadius;
        bool[] dataTransparent;
        int sunlight;
        int baseheight;
        int[] workportionArr;
        byte[] worklightArr;
        
        int minlight = 0;
        const int chunksize = 16;
        //copies 3x3x3 16*16*16 input chunks to one 48*48*48 temporary portion.
        private unsafe void GetPortion()
        {
            for (int xx = 0; xx < 3; xx++)
            {
                for (int yy = 0; yy < 3; yy++)
                {
                    for (int zz = 0; zz < 3; zz++)
                    {
                        //if (IsValidChunkPos(x / chunksize + xx, y / chunksize + yy, z / chunksize + zz))
                        int[] chunk = inputMapChunks[MapUtil.Index3d(xx, yy, zz, 3, 3)];
                        if(chunk!=null)
                        {
                            CopyChunk(workportionArr,
                                chunk,
                                xx * chunksize, yy * chunksize, zz * chunksize,
                                chunksize * 3, chunksize * 3, chunksize * 3);
                        }
                    }
                }
            }
        }

        private unsafe void CopyChunk(int[] portionArr, int[] chunkArr, int x, int y, int z,
            int portionsizex, int portionsizey, int portionsizez)
        {
            fixed (int* portion = portionArr)
            fixed (int* chunk = chunkArr)
            {
                int* portionInt = (int*)portion;
                int* chunkInt = (int*)chunk;
                for (int zz = 0; zz < 16; zz++)
                {
                    for (int yy = 0; yy < 16; yy++)
                    {
                        int pos = MapUtil.Index3d(0, yy, zz, 16, 16);
                        int pos2 = MapUtil.Index3d(x + 0, y + yy, z + zz, portionsizex, portionsizey);

                        //pos /= 2;
                        //pos2 /= 2;
                        //portionInt[pos2 + 0] = chunkInt[pos + 0];
                        //portionInt[pos2 + 1] = chunkInt[pos + 1];
                        //portionInt[pos2 + 2] = chunkInt[pos + 2];
                        //portionInt[pos2 + 3] = chunkInt[pos + 3];
                        //portionInt[pos2 + 4] = chunkInt[pos + 4];
                        //portionInt[pos2 + 5] = chunkInt[pos + 5];
                        //portionInt[pos2 + 6] = chunkInt[pos + 6];
                        //portionInt[pos2 + 7] = chunkInt[pos + 7];

                        
                        for (int xx = 0; xx < 16; xx++)
                        {
                            int orig = chunk[pos];
                            portionArr[pos2] = (byte)orig;
                            pos++;
                            pos2++;
                        }
                    }
                }
            }
        }

        private unsafe void ApplySunlight()
        {
            const int portionsize = 16 * 3;
            int[] radius = dataLightRadius;
            const int zplus = portionsize * portionsize;
            fixed (byte* worklight = worklightArr)
            {
                for (int xx = 0; xx < portionsize; xx++)
                {
                    for (int yy = 0; yy < portionsize; yy++)
                    {
                        int height = GetLightHeight(xx, yy);
                        int h = height - baseheight;
                        //h = MyMath.Clamp(h, 0, portionsize);
                        if (h < 0) { h = 0; }
                        if (h > portionsize) { continue; }
                        int pos = MapUtil.Index3d(xx, yy, h, portionsize, portionsize);
                        /*
                        for (int zz = 0; zz < h; zz++)
                        {
                            worklight[pos] = (byte)minlight;
                            pos += zplus;
                        }
                        */
                        for (int zz = h; zz < portionsize; zz++)
                        {
                            //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                            worklight[pos] = (byte)sunlight;
                            pos += zplus;
                        }
                    }
                }
            }
        }

        private int GetLightHeight(int xx, int yy)
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

        private unsafe void ApplyLightEmitting()
        {
            int[] radiusArr = dataLightRadius;
            const int portionsize = 16 * 3;
            const int portionsize3 = portionsize * portionsize * portionsize;
            fixed (int* workportion = workportionArr)
            fixed (byte* worklight = worklightArr)
            fixed (int* radius = radiusArr)
            {
                int* portionInt = (int*)workportion;
                int portionsize3div2 = portionsize3 / 2;
                for (int pos1 = 0; pos1 < portionsize3div2; pos1++)
                //for (int pos = 0; pos < portionsize3; pos++)
                //for (int xx = 0; xx < portionsize; xx++)
                {
                    //for (int yy = 0; yy < portionsize; yy++)
                    {
                        // for (int zz = 0; zz < portionsize; zz++)
                        //if (portionInt[pos1] == 0)
                        //{
                        //    continue;
                        //}
                        for (int pos = pos1 * 2; pos < pos1 * 2 + 2; pos++)
                        {
                            if (workportion[pos] >= 10) //optimization
                            {
                                if (radius[workportion[pos]] != 0) //optimization
                                {
                                    //var pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                                    if (radius[workportion[pos]] > worklight[pos])
                                    {
                                        var p = MapUtil.Pos(pos, portionsize, portionsize);
                                        int xx = p.x;
                                        int yy = p.y;
                                        int zz = p.z;
                                        int l = radius[workportion[pos]];
                                        if (xx > 1 && yy > 1 && zz > 1
                                            && xx < portionsize - 1 && yy < portionsize - 1 && zz < portionsize - 1)
                                        {
                                            lighttoflood.Push(new Vector3i(xx, yy, zz));
                                        }
                                        worklight[pos] = (byte)Math.Max(l, worklight[pos]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void FloodSunlight()
        {
            int portionsize = 16 * 3;
            int portionsize3 = MyMath.Pow3(portionsize);
            //int startx = x;
            //int starty = y;
            //int startz = z;
            int[] radiusArr = dataLightRadius;
            bool[] transparentArr = dataTransparent;
            fixed (int* workportion = workportionArr)
            fixed (byte* worklight = worklightArr)
            fixed (int* radius = radiusArr)
            fixed (bool* transparent = transparentArr)
            {
                for (int pos = 0; pos < portionsize3 - portionsize; pos++)
                //for (int zz = 1; zz < portionsize - 1; zz++)
                {
                    //for (int xx = 1; xx < portionsize - 1; xx++)
                    {
                        //for (int yy = 1; yy < portionsize - 1; yy++)
                        {
                            //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                            if (!transparent[workportion[pos]])
                            {
                                continue;
                            }
                            int curlight = worklight[pos];
                            if ((worklight[pos + 1] != curlight && transparent[workportion[pos + 1]])
                                || (worklight[pos + portionsize] != curlight && transparent[workportion[pos + portionsize]]))
                            {
                                var p = MapUtil.Pos(pos, portionsize, portionsize);
                                int xx = p.x;
                                int yy = p.y;
                                int zz = p.z;
                                if (xx > 1 && yy > 1 && zz > 1
                                    && xx < portionsize - 1 && yy < portionsize - 1 && zz < portionsize - 1)
                                {
                                    lighttoflood.Push(new Vector3i(xx, yy, zz));
                                    lighttoflood.Push(new Vector3i(xx + 1, yy, zz));
                                    lighttoflood.Push(new Vector3i(xx, yy + 1, zz));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FloodLight()
        {
            while (lighttoflood.Count > 0)
            {
                var k = lighttoflood.Pop();
                FloodLight(workportionArr, worklightArr, k.x, k.y, k.z);
            }
        }

        FastQueue<int> q = new FastQueue<int>();
        public void FloodLight(int[] portion, byte[] light, int startx, int starty, int startz)
        {
            const int portionsize = 16 * 3;
            int pos = MapUtil.Index3d(startx, starty, startz, portionsize, portionsize);
            if (light[pos] == minlight)
            {
                return;
            }
            int lightradius = dataLightRadius[portion[pos]];
            if (lightradius != 0)
            {
                light[pos] = (byte)lightradius;
            }
            if (light[pos + 1] == light[pos]
                && light[pos - 1] == light[pos]
                && light[pos + portionsize] == light[pos]
                && light[pos - portionsize] == light[pos]
                && light[pos + portionsize * portionsize] == light[pos]
                && light[pos - portionsize * portionsize] == light[pos])
            {
                return;
            }

            q.Clear();
            //Vector3i start = new Vector3i();
            //start.x = startx;
            //start.y = starty;
            //start.z = startz;
            int start = MapUtil.Index3d(startx, starty, startz, portionsize, portionsize);
            q.Push(start);
            for (; ; )
            {
                if (q.Count == 0)
                {
                    break;
                }
                int vpos = q.Pop();
                //var v = q.Pop();
                //int vpos = MapUtil.Index3d(v.x, v.y, v.z, portionsize, portionsize);
                int vlight = light[vpos];
                if (vlight == minlight)
                {
                    continue;
                }
                int vblock = portion[vpos];
                if (!dataTransparent[vblock]
                    && dataLightRadius[vblock] == 0)
                {
                    continue;
                }
                for (int i = 0; i < blocksnear.Length; i++)
                {
                    int n = vpos + blocksnear[i];
                    
                    //int nx = v.x + blocksnear[i].x;
                    //int ny = v.y + blocksnear[i].y;
                    //int nz = v.z + blocksnear[i].z;
                    //if (!IsValidPos(nx, ny, nz))
                    //{
                    //    continue;
                    //}
                    
                    //int n = MapUtil.Index3d(nx, ny, nz, portionsize, portionsize);
                    if (n < 0 || n >= light.Length) { continue; }
                    if (light[n] < vlight - 1)
                    {
                        light[n] = (byte)(vlight - 1);
                        //q.Push(new Vector3i(nx, ny, nz));
                        q.Push(n);
                    }
                }
            }
        }

        //int chunksizeportion { get { return chunksize * 3; } }
        const int chunksizeportion = 16 * 3;

        private bool IsValidPos(int vx, int vy, int vz)
        {
            return vx >= 0 && vy >= 0 && vz >= 0
                && vx < chunksizeportion && vy < chunksizeportion && vz < chunksizeportion;
        }
        
        int[] blocksnear = new int[] { -1, +1, -chunksizeportion, +chunksizeportion,
            -chunksizeportion * chunksizeportion, +chunksizeportion * chunksizeportion };
        /*
        Vector3i[] blocksnear = new Vector3i[6]
        {
            new Vector3i(-1, 0, 0),
            new Vector3i(1, 0, 0),
            new Vector3i(0, -1, 0),
            new Vector3i(0, 1, 0),
            new Vector3i(0, 0, -1),
            new Vector3i(0, 0, 1),
        };
        */

        private void SetPortion()
        {
            //int mapsizex = d_Map.MapSizeX;
            //int mapsizey = d_Map.MapSizeY;
            //int mapsizez = d_Map.MapSizeZ;
            for (int xx = -1; xx < chunksize + 1; xx++)
            {
                for (int yy = -1; yy < chunksize + 1; yy++)
                {
                    for (int zz = -1; zz < chunksize + 1; zz++)
                    {
                        //if (IsValidPos(x + chunksize + xx, y + chunksize + yy, z + chunksize + zz,
                        //    mapsizex, mapsizey, mapsizez))
                        {
                            //d_Light.SetBlock(x + chunksize + xx, y + chunksize + yy, z + chunksize + zz,
                            //    light[MapUtil.Index3d(xx + chunksize, yy + chunksize, zz + chunksize,
                            //    chunksize * 3, chunksize * 3)]);
                            outputChunkLight[MapUtil.Index3d(xx + 1, yy + 1, zz + 1, 18, 18)] =
                                worklightArr[MapUtil.Index3d(xx + chunksize, yy + chunksize, zz + chunksize,
                                chunksize * 3, chunksize * 3)];
                        }
                    }
                }
            }
        }

        public static bool IsValidPos(int x, int y, int z, int MapSizeX, int MapSizeY, int MapSizeZ)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= MapSizeX || y >= MapSizeY || z >= MapSizeZ)
            {
                return false;
            }
            return true;
        }

        FastStack<Vector3i> lighttoflood = new FastStack<Vector3i>();
    }
    public class Shadows3x3x3Simple : IShadows3x3x3
    {
        public void Start()
        {
        }

        const int shadowlight = 10;
        const int maxlight = 15;

        public unsafe void Update(byte[] outputChunkLight, int[][] inputMapChunks, int[][] inputHeightmapChunks, int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight)
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
    public class InfiniteMapChunked2d
    {
        [Inject]
        public IMapStorage d_Map;
        public int chunksize = 16;
        public int[][] chunks;
        public unsafe int GetBlock(int x, int y)
        {
            int[] chunk = GetChunk(x, y);
            return chunk[MapUtil.Index2d(x % chunksize, y % chunksize, chunksize)];
        }
        public int[] GetChunk(int x, int y)
        {
            int[] chunk = null;
            int kx = x / chunksize;
            int ky = y / chunksize;
            if (chunks[MapUtil.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] == null)
            {
                chunk = new int[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
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
            chunks = new int[n][];//(byte**)Marshal.AllocHGlobal(n * sizeof(IntPtr));
            for (int i = 0; i < n; i++)
            {
                chunks[i] = null;
            }
        }
        public unsafe void ClearChunk(int x, int y)
        {
            int px = x / chunksize;
            int py = y / chunksize;
            chunks[MapUtil.Index2d(px, py, d_Map.MapSizeX/chunksize)] = null;
        }
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ManicDigger.Renderers;

namespace ManicDigger
{
    public interface IShadows
    {
        void OnLocalBuild(int x, int y, int z);
        void OnSetBlock(int x, int y, int z);
        void ResetShadows();
        int GetLight(int x, int y, int z);
        int maxlight { get; }
        void OnGetTerrainBlock(int x, int y, int z);
        int? MaybeGetLight(int x, int y, int z);
        void OnMakeChunk(int chunkx, int chunky, int chunkz);
        int sunlight { get; set; }
        void OnSetChunk(int x, int y, int z);
    }
    //Top block in each column is at light level 16.
    //All blocks below are at light level 11.
    public class ShadowsSimple : IShadows
    {
        [Inject]
        public IGameDataLight d_Data;
        [Inject]
        public IMapStorage d_Map;
        [Inject]
        public IIsChunkDirty d_IsChunkDirty;
        [Inject]
        public InfiniteMapChunked2d d_Heightmap;
        public int chunksize = 16;
        public int defaultshadow = 11;
        #region IShadows Members
        public void OnLocalBuild(int x, int y, int z)
        {
        }
        public void OnSetBlock(int x, int y, int z)
        {
            int oldheight = d_Heightmap.GetBlock(x, y);
            UpdateColumnHeight(x, y);
            //update shadows in all chunks below
            int newheight = d_Heightmap.GetBlock(x, y);
            int min = Math.Min(oldheight, newheight);
            int max = Math.Max(oldheight, newheight);
            for (int i = min; i < max; i++)
            {
                if (i / chunksize != z / chunksize)
                {
                    d_IsChunkDirty.SetChunkDirty(x / chunksize, y / chunksize, i / chunksize, true);
                }
            }
        }
        private void UpdateColumnHeight(int x, int y)
        {
            //todo faster
            int height = d_Map.MapSizeZ - 1;
            for (int i = d_Map.MapSizeZ - 1; i >= 0; i--)
            {
                height = i;
                if (!d_Data.IsTransparentForLight[d_Map.GetBlock(x, y, i)])
                {
                    break;
                }
            }
            d_Heightmap.SetBlock(x, y, height);
        }
        public void ResetShadows()
        {
        }
        public int GetLight(int x, int y, int z)
        {
            return IsShadow(x, y, z) ? defaultshadow : maxlight;
        }
        public int maxlight
        {
            get { return 16; }
        }
        public void OnGetTerrainBlock(int x, int y, int z)
        {
        }
        #endregion
        private bool IsShadow(int x, int y, int z)
        {
            int height = d_Heightmap.GetBlock(x, y);
            return z < height;
        }
        #region IShadows Members
        public int? MaybeGetLight(int x, int y, int z)
        {
            return IsShadow(x, y, z) ? defaultshadow : maxlight;
        }
        #endregion
        #region IShadows Members
        public void OnMakeChunk(int chunkx, int chunky, int chunkz)
        {
        }
        #endregion
        #region IShadows Members
        int sunlight_ = 16;
        public int sunlight { get { return sunlight_; } set { sunlight_ = value; } }
        #endregion
        #region IShadows Members
        public void OnSetChunk(int x, int y, int z)
        {
        }
        #endregion
    }
    //Before drawing a chunk (16x16x16) on screen,
    //it floods light from 3x3x3 chunks around it.
    //Todo: Optimize.
    public class Shadows : IShadows
    {
        [Inject]
        public IMapStorage d_Map;
        [Inject]
        public IMapStoragePortion d_MapPortion;
        [Inject]
        public IIsChunkDirty d_DirtyChunks;
        [Inject]
        public IGameDataLight d_Data;
        [Inject]
        public InfiniteMapChunked2d d_Heightmap;
        [Inject]
        public InfiniteMapChunkedSimple d_Light;
        
        public int chunksize = 16;
        int minlight = 0;
        public int maxlight { get { return 16; } }
        int sunlight_ = 16;
        public int sunlight { get { return sunlight_; } set { sunlight_ = value; } }

        public Shadows()
        {
        }

        public void Start()
        {
            portion = new byte[chunksize * chunksize * chunksize * 3 * 3 * 3];
            light = new byte[chunksize * chunksize * chunksize * 3 * 3 * 3];
            q.Initialize(1024);
            lighttoflood.Initialize(1024);
        }

        public void OnLocalBuild(int x, int y, int z)
        {
        }

        public void OnSetBlock(int x, int y, int z)
        {
            int oldheight = d_Heightmap.GetBlock(x, y);
            UpdateColumnHeight(x, y);
            //update shadows in all chunks below
            int newheight = d_Heightmap.GetBlock(x, y);
            int min = Math.Min(oldheight, newheight);
            int max = Math.Max(oldheight, newheight);
            for (int i = min; i < max; i++)
            {
                if (i / chunksize != z / chunksize)
                {
                    d_DirtyChunks.SetChunkDirty(x / chunksize, y / chunksize, i / chunksize, true);
                }
            }
            //Todo: too many redraws. Optimize.
            //Now placing a single block updates 27 chunks,
            //and each of those chunk updates calculates light from 27 chunks.
            //So placing a block is often 729x slower than it should be.
            for (int xx = 0; xx < 3; xx++)
            {
                for (int yy = 0; yy < 3; yy++)
                {
                    for (int zz = 0; zz < 3; zz++)
                    {
                        d_DirtyChunks.SetChunkDirty(x / chunksize + xx - 1, y / chunksize + yy - 1, z / chunksize + zz - 1, true);
                    }
                }
            }
        }

        private void UpdateColumnHeight(int x, int y)
        {
            //todo faster
            int height = d_Map.MapSizeZ - 1;
            for (int i = d_Map.MapSizeZ - 1; i >= 0; i--)
            {
                height = i;
                if (!d_Data.IsTransparentForLight[d_Map.GetBlock(x, y, i)])
                {
                    break;
                }
            }
            d_Heightmap.SetBlock(x, y, height);
        }

        public void ResetShadows()
        {
            Array.Clear(portion, 0, portion.Length);
            Array.Clear(light, 0, portion.Length);
        }

        public int GetLight(int x, int y, int z)
        {
            return d_Light.GetBlock(x, y, z);
        }

        public void OnGetTerrainBlock(int x, int y, int z)
        {
        }

        public int? MaybeGetLight(int x, int y, int z)
        {
            int l = 0;
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                l = d_Light.GetBlock(x, y, z); // returns 0 when unknown
            }
            if (l == 0)
            {
                if (z >= GetLightHeight(x, y))
                {
                    return sunlight_;
                }
                else
                {
                    return minlight;
                }
            }
            else
            {
                return l - 1;
            }
        }

        byte[] portion;
        byte[] light;

        public void OnMakeChunk(int chunkx, int chunky, int chunkz)
        {
            int x = (chunkx - 1) * chunksize;
            int y = (chunky - 1) * chunksize;
            int z = (chunkz - 1) * chunksize;
            
            GetPortion(d_MapPortion, chunksize, portion, x, y, z);
            lighttoflood.Clear();
            ApplySunlight(portion, light, x, y, z);
            ApplyLightEmitting(portion, light, x, y, z);
            FloodSunlight(portion, light, x, y, z);
            FloodLight();
            SetPortion(x, y, z);
        }

        private void FloodLight()
        {
            while (lighttoflood.Count > 0)
            {
                var k = lighttoflood.Pop();
                FloodLight(portion, light, k.x, k.y, k.z);
            }
        }

        FastQueue<Vector3i> q = new FastQueue<Vector3i>();

        public void FloodLight(byte[] portion, byte[] light, int startx, int starty, int startz)
        {
            const int portionsize = 16 * 3;
            int pos = MapUtil.Index3d(startx, starty, startz, portionsize, portionsize);
            if (light[pos] == minlight)
            {
                return;
            }
            int lightradius = d_Data.LightRadius[portion[pos]];
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
            Vector3i start = new Vector3i();
            start.x = startx;
            start.y = starty;
            start.z = startz;
            q.Push(start);
            for (; ; )
            {
                if (q.Count == 0)
                {
                    break;
                }
                Vector3i v = q.Pop();
                int vpos = MapUtil.Index3d(v.x, v.y, v.z, portionsize, portionsize);
                int vlight = light[vpos];
                if (vlight == minlight)
                {
                    continue;
                }
                int vblock = portion[vpos];
                if (!d_Data.IsTransparentForLight[vblock]
                    && d_Data.LightRadius[vblock] == 0)
                {
                    continue;
                }
                for (int i = 0; i < blocksnear.Length; i++)
                {
                    int nx = v.x + blocksnear[i].x;
                    int ny = v.y + blocksnear[i].y;
                    int nz = v.z + blocksnear[i].z;
                    if (!IsValidPos(nx, ny, nz))
                    {
                        continue;
                    }
                    int npos = MapUtil.Index3d(nx, ny, nz, portionsize, portionsize);
                    if (light[npos] < vlight - 1)
                    {
                        light[npos] = (byte)(vlight - 1);
                        q.Push(new Vector3i(nx, ny, nz));
                    }
                }
            }
        }

        int chunksizeportion { get { return chunksize * 3; } }

        private bool IsValidPos(int vx, int vy, int vz)
        {
            return vx >= 0 && vy >= 0 && vz >= 0
                && vx < chunksizeportion && vy < chunksizeportion && vz < chunksizeportion;
        }

        Vector3i[] blocksnear = new Vector3i[6]
        {
            new Vector3i(-1, 0, 0),
            new Vector3i(1, 0, 0),
            new Vector3i(0, -1, 0),
            new Vector3i(0, 1, 0),
            new Vector3i(0, 0, -1),
            new Vector3i(0, 0, 1),
        };

        private void SetPortion(int x, int y, int z)
        {
            int mapsizex = d_Map.MapSizeX;
            int mapsizey = d_Map.MapSizeY;
            int mapsizez = d_Map.MapSizeZ;
            for (int xx = -1; xx < chunksize + 1; xx++)
            {
                for (int yy = -1; yy < chunksize + 1; yy++)
                {
                    for (int zz = -1; zz < chunksize + 1; zz++)
                    {
                        if (IsValidPos(x + chunksize + xx, y + chunksize + yy, z + chunksize + zz,
                            mapsizex, mapsizey, mapsizez))
                        {
                            d_Light.SetBlock(x + chunksize + xx, y + chunksize + yy, z + chunksize + zz,
                                light[MapUtil.Index3d(xx + chunksize, yy + chunksize, zz + chunksize,
                                chunksize * 3, chunksize * 3)]);
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

        public void OnSetChunk(int x, int y, int z)
        {
        }

        private void GetPortion(IMapStoragePortion m, int chunksize, byte[] portion, int x, int y, int z)
        {
            for (int xx = 0; xx < 3; xx++)
            {
                for (int yy = 0; yy < 3; yy++)
                {
                    for (int zz = 0; zz < 3; zz++)
                    {
                        if (IsValidChunkPos(x / chunksize + xx, y / chunksize + yy, z / chunksize + zz))
                        {
                            CopyChunk(portion,
                                m.GetChunk(x + xx * chunksize, y + yy * chunksize, z + zz * chunksize),
                                xx * chunksize, yy * chunksize, zz * chunksize,
                                chunksize * 3, chunksize * 3, chunksize * 3);
                        }
                    }
                }
            }
        }

        bool IsValidChunkPos(int cx, int cy, int cz)
        {
            return cx >= 0 && cy >= 0 && cz >= 0
                && cx < d_Map.MapSizeX / chunksize
                && cy < d_Map.MapSizeY / chunksize
                && cz < d_Map.MapSizeZ / chunksize;
        }

        private unsafe void CopyChunk(byte[] portionArr, byte[] chunkArr, int x, int y, int z,
            int portionsizex, int portionsizey, int portionsizez)
        {
            fixed (byte* portion = portionArr)
            fixed (byte* chunk = chunkArr)
            for (int zz = 0; zz < 16; zz++)
            {
                for (int yy = 0; yy < 16; yy++)
                {
                    int pos = MapUtil.Index3d(0, yy, zz, 16, 16);
                    int pos2 = MapUtil.Index3d(x + 0, y + yy, z + zz, portionsizex, portionsizey);
                    for (int xx = 0; xx < 16; xx++)
                    {
                        byte orig = chunk[pos];
                        portion[pos2] = (byte)orig;
                        pos++;
                        pos2++;
                    }
                }
            }
        }

        FastStack<Vector3i> lighttoflood = new FastStack<Vector3i>();
        
        public unsafe void ApplySunlight(byte[] portionarr, byte[] lightarr, int x, int y, int z)
        {
            int portionsize = 16 * 3;
            int[] radius = d_Data.LightRadius;
            int zplus = portionsize * portionsize;
            fixed (byte* light = lightarr)
            for (int xx = 0; xx < portionsize; xx++)
            {
                for (int yy = 0; yy < portionsize; yy++)
                {
                    int height = GetLightHeight(x + xx, y + yy);
                    int h = height - z;
                    h = MyMath.Clamp(h, 0, portionsize);
                    int pos = MapUtil.Index3d(xx, yy, 0, portionsize, portionsize);
                    for (int zz = 0; zz < h; zz++)
                    {
                        light[pos] = (byte)minlight;
                        pos += zplus;
                    }
                    for (int zz = h; zz < portionsize; zz++)
                    {
                        //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                        light[pos] = (byte)sunlight_;
                        pos += zplus;
                    }
                }
            }
            
        }

        private unsafe void ApplyLightEmitting(byte[] portionArr, byte[] lightArr, int x, int y, int z)
        {
            int[] radiusArr = d_Data.LightRadius;
            int portionsize = 16 * 3;
            int portionsize3 = MyMath.Pow3(portionsize);
            fixed (byte* portion = portionArr)
            fixed (byte* light = lightArr)
            fixed (int* radius = radiusArr)
            for (int pos = 0; pos < portionsize3; pos++)
            //for (int xx = 0; xx < portionsize; xx++)
            {
                //for (int yy = 0; yy < portionsize; yy++)
                {
                   // for (int zz = 0; zz < portionsize; zz++)
                    if(portion[pos]!=0) //optimization
                    if(radius[portion[pos]]!=0) //optimization
                    {
                        //var pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                        if (radius[portion[pos]] > light[pos])
                        {
                            var p = MapUtil.Pos(pos, portionsize, portionsize);
                            int xx = p.x;
                            int yy = p.y;
                            int zz = p.z;
                            int l = d_Data.LightRadius[portion[pos]];
                            if (xx > 1 && yy > 1 && zz > 1
                                && xx < portionsize - 1 && yy < portionsize - 1 && zz < portionsize - 1)
                            {
                                lighttoflood.Push(new Vector3i(xx, yy, zz));
                            }
                            light[pos] = (byte)Math.Max(l, light[pos]);
                        }
                    }
                }
            }
        }

        public unsafe void FloodSunlight(byte[] portionArr, byte[] lightArr, int x, int y, int z)
        {
            int portionsize = 16 * 3;
            int portionsize3 = MyMath.Pow3(portionsize);
            int startx = x;
            int starty = y;
            int startz = z;
            int[] radiusArr = d_Data.LightRadius;
            bool[] transparentArr = d_Data.IsTransparentForLight;
            fixed (byte* portion = portionArr)
            fixed (byte* light = lightArr)
            fixed (int* radius = radiusArr)
            fixed (bool* transparent = transparentArr)
            for (int pos = 0; pos < portionsize3 - portionsize; pos++)
            //for (int zz = 1; zz < portionsize - 1; zz++)
            {
                //for (int xx = 1; xx < portionsize - 1; xx++)
                {
                    //for (int yy = 1; yy < portionsize - 1; yy++)
                    {
                        //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                        if (!transparent[portion[pos]])
                        {
                            continue;
                        }
                        int curlight = light[pos];
                        if ((light[pos + 1] != curlight && transparent[portion[pos + 1]])
                            || (light[pos + portionsize] != curlight && transparent[portion[pos + portionsize]]))
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

        int GetLightHeight(int x, int y)
        {
            return d_Heightmap.GetBlock(x, y);
        }
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

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
    public class ShadowsSimple : IShadows
    {
        [Inject]
        public IGameData data;
        [Inject]
        public IMapStorage map;
        [Inject]
        public IIsChunkReady ischunkdirty;
        [Inject]
        public InfiniteMapChunked2d heightmap;
        public int chunksize = 16;
        int defaultshadow = 11;
        #region IShadows Members
        public void OnLocalBuild(int x, int y, int z)
        {
        }
        public void OnSetBlock(int x, int y, int z)
        {
            int oldheight = heightmap.GetBlock(x, y);
            UpdateColumnHeight(x, y);
            //update shadows in all chunks below
            int newheight = heightmap.GetBlock(x, y);
            int min = Math.Min(oldheight, newheight);
            int max = Math.Max(oldheight, newheight);
            for (int i = min; i < max; i++)
            {
                if (i / chunksize != z / chunksize)
                {
                    ischunkdirty.SetChunkDirty(x / chunksize, y / chunksize, i / chunksize, true);
                }
            }
        }
        private void UpdateColumnHeight(int x, int y)
        {
            //todo faster
            int height = map.MapSizeZ - 1;
            for (int i = map.MapSizeZ - 1; i >= 0; i--)
            {
                height = i;
                if (!data.GrassGrowsUnder(map.GetBlock(x, y, i)))
                {
                    break;
                }
            }
            heightmap.SetBlock(x, y, height);
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
            int height = heightmap.GetBlock(x, y);
            return z < height;
        }
        #region IShadows Members
        public int? MaybeGetLight(int x, int y, int z)
        {
            return null;
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
    public class Shadows : IShadows
    {
        [Inject]
        public IMapStorage map;
        [Inject]
        public IGameData data;
        [Inject]
        public ITerrainRenderer terrain;
        [Inject]
        public ILocalPlayerPosition localplayerposition;
        [Inject]
        public Config3d config3d;
        [Inject]
        public IIsChunkReady ischunkready;
        [Inject]
        public InfiniteMapChunked2d heightmap;
        [Inject]
        public InfiniteMapChunkedSimple light;
        
        public int chunksize = 16;
        int minlight = 0;
        public int maxlight { get { return 16; } }
        int sunlight_ = 16;
        public int sunlight { get { return sunlight_; } set { sunlight_ = value; } }

        public Shadows()
        {
            q.Initialize(1024);
        }

        Queue<Vector3i> shadowstoupdate = new Queue<Vector3i>();
        public void OnLocalBuild(int x, int y, int z)
        {
            lock (lighttoupdate_lock)
            {
                //UpdateShadows(x, y, z);
                shadowstoupdate.Enqueue(new Vector3i(x, y, z));
            }
        }
        public void OnSetBlock(int x, int y, int z)
        {
            shadowstoupdate.Enqueue(new Vector3i(x, y, z));
        }
        private void UpdateShadows(int x, int y, int z)
        {
            if ((localplayerposition.LocalPlayerPosition
                - new OpenTK.Vector3(x, z, y)).Length
                > terrain.DrawDistance * 1.5f)
            {
                //do not update shadow info outside of fog range
                // - clear shadow information there.
                for (int zz = 0; zz < map.MapSizeZ; zz += chunksize)
                {
                    light.ClearChunk(x, y, zz);
                    light.ClearChunk(x - chunksize, y, zz);
                    light.ClearChunk(x + chunksize, y, zz);
                    light.ClearChunk(x, y - chunksize, zz);
                    light.ClearChunk(x, y + chunksize, zz);
                }
                return;
            }

            lighttoupdate.Clear();
            UpdateSunlight(x, y, z);
            List<Vector3i> near = new List<Vector3i>();
            near.Add(new Vector3i(x, y, z));
            foreach (var n in BlocksNear(x, y, z))
            {
                if (MapUtil.IsValidPos(map, n.x, n.y, n.z))
                {
                    near.Add(n);
                }
            }
            if (near.Count > 0)
            {
                DefloodLight(near);
            }
            SetChangedChunksDirty();
        }
        private void SetChangedChunksDirty()
        {
            foreach (var k in lighttoupdate)
            {
                Vector3i kpos = MapUtil.FromMapPos(k.Key);
                if (MapUtil.IsValidPos(map, kpos.x, kpos.y, kpos.z))
                {
                    terrain.UpdateTile(kpos.x, kpos.y, kpos.z);
                    ischunkready.SetChunkDirty(kpos.x / chunksize, kpos.y / chunksize, kpos.z / chunksize, true);
                }
            }
        }
        Dictionary<ulong, bool> lighttoupdate = new Dictionary<ulong, bool>();
        object lighttoupdate_lock = new object();
        public bool loaded = true;
        public void ResetShadows()
        {
            light.Restart();
            chunklighted = new bool[map.MapSizeX / chunksize, map.MapSizeY / chunksize, map.MapSizeZ / chunksize];
            UpdateHeightCache();
            loaded = true;
        }
        private void UpdateLight()
        {
            //light = new InfiniteMapChunkedSimple() { map = map };
            light.Restart();
            UpdateHeightCache();
        }
        int LightGetBlock(int x, int y, int z)
        {
            retry:
            int block = light.GetBlock(x, y, z);
            if (block == 0)//unknown
            {
                UpdateStartSunlight(x, y, z);
                goto retry;
            }
            return block - 1;
        }
        private void UpdateStartSunlight(int x, int y, int z)
        {
            int height = GetLightHeight(x,y);
            int light;
            if (z >= height)
            {
                light = sunlight;
            }
            else
            {
                light = minlight;
            }
            LightSetBlock(x, y, z, (byte)light);
        }
        private void UpdateStartSunlightChunk(int x, int y, int z)
        {
            int startx = (x / chunksize) * chunksize;
            int starty = (y / chunksize) * chunksize;
            int startz = (z / chunksize) * chunksize;
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    int height = GetLightHeight(startx + xx, starty + yy);
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int light;
                        if (startz + zz >= height)
                        {
                            light = sunlight;
                        }
                        else
                        {
                            light = minlight;
                        }
                        LightSetBlock(startx + xx, starty + yy, startz + zz, (byte)light);
                    }
                }
            }
            chunklighted[x / chunksize, y / chunksize, z / chunksize] = true;
        }
        void LightSetBlock(int x, int y, int z, int block)
        {
            light.SetBlock(x, y, z, block + 1);
        }
        private void UpdateHeightCache()
        {
            /*
            if (lightheight != null)
            {
                lightheight.Clear();
            }
            */
        }
        private int GetRealLightHeightAt(int x, int y)
        {
            Point p = new Point(x, y);
            int height = map.MapSizeZ - 1;
            for (int z = map.MapSizeZ - 1; z >= 0; z--)
            {
                if (data.GrassGrowsUnder(map.GetBlock(x, y, z)))
                {
                    height--;
                }
                else
                {
                    break;
                }
            }
            return height;
        }
        int GetLightHeight(int x, int y)
        {
            return heightmap.GetBlock(x, y);
        }
        void UpdateLightHeightmapAt(int x, int y)
        {
            //todo faster
            int height = map.MapSizeZ - 1;
            for (int i = map.MapSizeZ - 1; i >= 0; i--)
            {
                height = i;
                if (!data.GrassGrowsUnder(map.GetBlock(x, y, i)))
                {
                    break;
                }
            }
            heightmap.SetBlock(x, y, height);
            /*lightheight.SetBlock(x, y, GetRealLightHeightAt(x, y) + 1);*/
        }
        void UpdateSunlight(int x, int y, int z)
        {
            /* if (lightheight == null) { ResetShadows(); } */
            int oldheight = GetLightHeight(x, y);
            if (oldheight < 0) { oldheight = 0; }
            if (oldheight >= map.MapSizeZ) { oldheight = map.MapSizeZ - 1; }
            UpdateLightHeightmapAt(x, y);
            int newheight = GetLightHeight(x, y);
            if (newheight < 0) { newheight = 0; } //fixes crash
            if (newheight < oldheight)
            {
                //make white
                for (int i = newheight; i <= oldheight; i++)
                {
                    SetLight(x, y, i, sunlight);
                    currentlightchunk = null;
                    FloodLight(x, y, i);
                }
            }
            if (newheight > oldheight)
            {
                List<Vector3i> deflood = new List<Vector3i>();
                //make black
                for (int i = oldheight; i <= newheight; i++)
                {
                    SetLight(x, y, i, minlight);
                    //DefloodLight(x, i);
                    foreach (var n in BlocksNear(x, y, i))
                    {
                        if (MapUtil.IsValidPos(map, n.x, n.y, n.z))
                        {
                            deflood.Add(n);
                        }
                    }
                }
                if (deflood.Count != 0)
                {
                    DefloodLight(deflood);
                }
            }
        }
        void SetLight(int x, int y, int z, int value)
        {
            LightSetBlock(x, y, z, (byte)value);
            if (lighttoupdate != null)
            {
                lighttoupdate[MapUtil.ToMapPos((x / 16) * 16 + 5, (y / 16) * 16 + 5, (z / 16) * 16 + 5)] = true;

                for (int i = 0; i < blocksnear.Length; i++)
                {
                    int vx = x + blocksnear[i].x;
                    int vy = y + blocksnear[i].y;
                    int vz = z + blocksnear[i].z;
                    if (vx / 16 != x / 16
                        || vy / 16 != y / 16
                        || vz / 16 != z / 16)
                    {
                        lighttoupdate[MapUtil.ToMapPos((vx / 16) * 16 + 5, (vy / 16) * 16 + 5, (vz / 16) * 16 + 5)] = true;
                    }
                }
            }
        }
        bool IsSunlighted(int x, int y, int z)
        {
            return z > heightmap.GetBlock(x, y);
        }
        private void DefloodLight(IEnumerable<Vector3i> start)
        {
            Queue<Vector3i> q = new Queue<Vector3i>();
            Vector3i ss = new Vector3i();
            foreach (var s in start)
            {
                q.Enqueue(s);
                ss = s;
            }
            Dictionary<Vector3i, bool> reflood = new Dictionary<Vector3i, bool>();
            int searched = 1;
            for (; ; )
            {
                if (q.Count == 0)
                {
                    break;
                }
                Vector3i v = q.Dequeue();
                searched++;
                if (distancesquare(v, new Vector3i(ss.x, ss.y, ss.z)) > maxlight * 2 * maxlight * 2)
                {
                    continue;
                }
                int vblock = map.GetBlock(v.x, v.y, v.z);
                if (!data.GrassGrowsUnder(vblock)
                    && data.GetLightRadius(vblock) == 0)
                {
                    continue;
                }
                int vlight = LightGetBlock(v.x, v.y, v.z);
                if (vlight == maxlight
                    || data.GetLightRadius(vblock) != 0)
                {
                    reflood[v] = true;
                    continue;
                }
                if (vlight == minlight)
                {
                    continue;
                }
                SetLight(v.x, v.y, v.z, minlight);
                foreach (var n in BlocksNear(v.x, v.y, v.z))
                {
                    if (!MapUtil.IsValidPos(map, n.x, n.y, n.z))
                    {
                        continue;
                    }
                    if (LightGetBlock(n.x, n.y, n.z) < vlight)
                    {
                        q.Enqueue(n);
                    }
                    else
                    {
                        reflood[n] = true;
                    }
                }
            }
            //Console.WriteLine("reflood: {0}, searched: {1}", reflood.Keys.Count, searched);
            foreach (var p in reflood.Keys)
            {
                currentlightchunk = null;
                FloodLight(p.x, p.y, p.z);
            }
        }
        private int distancesquare(Vector3i a, Vector3i b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            int dz = a.z - b.z;
            return dx * dx + dy * dy + dz * dz;
        }
        FastStack<Vector3i> q = new FastStack<Vector3i>();
        byte[, ,] currentlightchunk;
        int startx;
        int starty;
        int startz;
        private void FloodLight(int x, int y, int z)
        {
            if (light == null)
            {
                UpdateLight();
            }
            int lightradius = data.GetLightRadius(map.GetBlock(x, y, z));
            if (lightradius != 0)
            {
                LightSetBlock(x, y, z, (byte)(lightradius));
            }
            q.Clear();
            q.Push(new Vector3i(x, y, z));
            for (; ; )
            {
                if (q.Count == 0)
                {
                    break;
                }
                Vector3i v = q.Pop();
                if (distancesquare(v, new Vector3i(x, y, z)) > maxlight * maxlight)
                {
                    continue;
                }
                int vlight = LightGetBlockFast(v.x, v.y, v.z);
                if (vlight == minlight)
                {
                    continue;
                }
                int vblock = map.GetBlock(v.x, v.y, v.z);
                if (!data.GrassGrowsUnder(vblock)
                    && data.GetLightRadius(vblock) == 0)
                {
                    continue;
                }
                for (int i = 0; i < blocksnear.Length; i++)
                {
                    int nx = v.x + blocksnear[i].x;
                    int ny = v.y + blocksnear[i].y;
                    int nz = v.z + blocksnear[i].z;
                    if (!MapUtil.IsValidPos(map, nx, ny, nz))
                    {
                        continue;
                    }
                    if (LightGetBlockFast(nx, ny, nz) < vlight - 1)
                    {
                        //this is for reflooding sunlight.
                        if (IsSunlighted(nx, ny, nz)
                            && (LightGetBlock(nx, ny, nz) < sunlight)
                            && (vlight - 1 <= sunlight))
                        {
                            LightSetBlock(nx, ny, nz, sunlight);
                        }
                        else
                        {
                            SetLight(nx, ny, nz, (byte)(vlight - 1));
                        }
                        q.Push(new Vector3i(nx, ny, nz));
                    }
                }
            }
        }
        int LightGetBlockFast(int x, int y, int z)
        {
            /*
            if (currentlightchunk != null && InSameChunk(x, y, z, startx, starty, startz))
            {
                int block = currentlightchunk[x % chunksize, y % chunksize, z % chunksize];
                if (block == 0)//unknown
                {
                    return LightGetBlock(x, y, z);
                }
                return block - 1;
            }
            */
            return LightGetBlock(x, y, z);
        }
        bool InSameChunk(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            return x1 / chunksize == x2 / chunksize
                && y1 / chunksize == y2 / chunksize
                && z1 / chunksize == z2 / chunksize;
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
        private IEnumerable<Vector3i> BlocksNear(int x, int y, int z)
        {
            yield return new Vector3i(x - 1, y, z);
            yield return new Vector3i(x + 1, y, z);
            yield return new Vector3i(x, y - 1, z);
            yield return new Vector3i(x, y + 1, z);
            yield return new Vector3i(x, y, z - 1);
            yield return new Vector3i(x, y, z + 1);
        }
        private IEnumerable<Vector3i> BlocksNearWith(int x, int y, int z)
        {
            yield return new Vector3i(x, y, z);
            yield return new Vector3i(x - 1, y, z);
            yield return new Vector3i(x + 1, y, z);
            yield return new Vector3i(x, y - 1, z);
            yield return new Vector3i(x, y + 1, z);
            yield return new Vector3i(x, y, z - 1);
            yield return new Vector3i(x, y, z + 1);
        }
        //int solidmax = 0;
        //int solidmin = 0;
        //int solidunknown = 0;
        //int notsolid = 0;
        private void FloodLightChunk(int x, int y, int z)
        {
            this.currentlightchunk = light.GetChunk(x, y, z);
            this.startx = x;
            this.starty = y;
            this.startz = z;
            if (IsSolidChunk(currentlightchunk, (byte)(sunlight + 1)))
            {
                //solidmax++;
                return;
            }
            //if (IsSolidChunk(currentlightchunk, (byte)(minlight + 1)))
            //{
            //    //solidmin++;
            //    return;
            //}
            //if (IsSolidChunk(currentlightchunk, 0)) { solidunknown++; }
            //else { notsolid++; }
        //    Dictionary<Vector3i, bool> lighttoupdatecopy = lighttoupdate;
    //        lighttoupdate = null;
            for (int xx = 0 - 1; xx < chunksize + 1; xx++)
            {
                for (int yy = 0 - 1; yy < chunksize + 1; yy++)
                {
                    for (int zz = 0 - 1; zz < chunksize + 1; zz++)
                    {
                        if (MapUtil.IsValidPos(map, x + xx, y + yy, z + zz))
                            FloodLight(x + xx, y + yy, z + zz);
                    }
                }
            }
       //     lighttoupdate = lighttoupdatecopy;
        }
        bool IsSolidChunk(byte[, ,] chunk, byte value)
        {
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        if (currentlightchunk[xx, yy, zz] != value)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        bool IsValidChunkPos(int cx, int cy, int cz)
        {
            return cx >= 0 && cy >= 0 && cz >= 0
                && cx < map.MapSizeX / chunksize
                && cy < map.MapSizeY / chunksize
                && cz < map.MapSizeZ / chunksize;
        }
        bool[, ,] chunklighted;
        public int GetLight(int x, int y, int z)
        {
            if (loaded)
            {
                while (shadowstoupdate.Count > 0)
                {
                    Vector3i p = shadowstoupdate.Dequeue();
                    UpdateShadows(p.x, p.y, p.z);
                }
            }
            if (light == null)
            {
                UpdateLight();
            }
            return LightGetBlock(x, y, z);
        }
        public void OnGetTerrainBlock(int x, int y, int z)
        {
        }
        #region IShadows Members
        public int? MaybeGetLight(int x, int y, int z)
        {
            if (light == null)
            {
                UpdateLight();
            }
            int l = light.GetBlock(x, y, z);
            if (l == 0)
            {
                return null;
            }
            else
            {
                return l - 1;
            }
        }
        #endregion
        #region IShadows Members
        public void OnMakeChunk(int chunkx, int chunky, int chunkz)
        {
            if (chunklighted == null)
            {
                chunklighted = new bool[map.MapSizeX / chunksize, map.MapSizeY / chunksize, map.MapSizeZ / chunksize];
            }
            if (!chunklighted[chunkx, chunky, chunkz])
            FloodLightChunk(chunkx * chunksize, chunky * chunksize, chunkz * chunksize);
            chunklighted[chunkx, chunky, chunkz] = true;
            lighttoupdate.Remove(MapUtil.ToMapPos(chunkx * chunksize + 5, chunky * chunksize + 5, chunkz * chunksize + 5));
            SetChangedChunksDirty();
            lighttoupdate.Clear();
        }
        #endregion
        #region IShadows Members
        public void OnSetChunk(int x, int y, int z)
        {
            chunklighted[x / chunksize, y / chunksize, z / chunksize] = false;
            light.ClearChunk(x, y, z);
        }
        #endregion
    }
    public class InfiniteMapChunked2d
    {
        [Inject]
        public IMapStorage map;
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
            chunks = new byte[map.MapSizeX / chunksize, map.MapSizeY / chunksize][,];
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
        public IMapStorage map;
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
            if (!MapUtil.IsValidPos(map, x, y, z))
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
            chunks = new byte[map.MapSizeX / chunksize, map.MapSizeY / chunksize, map.MapSizeZ / chunksize][, ,];
        }
    }
}

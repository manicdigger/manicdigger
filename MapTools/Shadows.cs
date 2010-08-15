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
    }
    public class ShadowsSimple : IShadows
    {
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IMapStorage map { get; set; }
        int defaultshadow = 11;
        #region IShadows Members
        public void OnLocalBuild(int x, int y, int z)
        {
        }
        public void OnSetBlock(int x, int y, int z)
        {
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
            for (int i = 1; i < 10; i++)
            {
                if (MapUtil.IsValidPos(map, x, y, z + i) && !data.GrassGrowsUnder(map.GetBlock(x, y, z + i)))
                {
                    return true;
                }
            }
            return false;
        }
        #region IShadows Members
        public int? MaybeGetLight(int x, int y, int z)
        {
            return null;
        }
        #endregion
    }
    public class Shadows : IShadows
    {
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public ITerrainRenderer terrain { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        [Inject]
        public Config3d config3d { get; set; }

        const int chunksize = 16;
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
                lightheight.ClearChunk(x, y);
                lightheight.ClearChunk(x - chunksize, y);
                lightheight.ClearChunk(x + chunksize, y);
                lightheight.ClearChunk(x, y - chunksize);
                lightheight.ClearChunk(x, y - chunksize);
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
            foreach (var k in lighttoupdate)
            {
                terrain.UpdateTile(k.Key.x, k.Key.y, k.Key.z);
            }
        }
        Dictionary<Vector3i, bool> lighttoupdate = new Dictionary<Vector3i, bool>();
        object lighttoupdate_lock = new object();
        public bool loaded = true;
        public void ResetShadows()
        {
            light = null;
            lightheight = null;
            lightheight = new InfiniteHeightCache();
            chunklighted = null;
            UpdateHeightCache();
            loaded = true;
        }
        private void UpdateLight()
        {
            light = new InfiniteMapCache();
            UpdateHeightCache();
        }
        private void UpdateStartSunlight(int x, int y)
        {
            int height = GetLightHeight(x, y);
            for (int z = 0; z < map.MapSizeZ; z++)
            {
                if (z >= height)
                {
                    LightSetBlock(x, y, z, (byte)maxlight);
                }
                else
                {
                    LightSetBlock(x, y, z, (byte)minlight);
                }
            }
        }
        int LightGetBlock(int x, int y, int z)
        {
            int block = light.GetBlock(x, y, z);
            if (block == 0)//unknown
            {
                UpdateStartSunlightChunk(x, y);
            }
            return block - 1;
        }
        private void UpdateStartSunlightChunk(int x, int y)
        {
            int startx = (x / chunksize) * chunksize;
            int starty = (y / chunksize) * chunksize;
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    UpdateStartSunlight(startx + xx, starty + yy);
                }
            }
        }
        void LightSetBlock(int x, int y, int z, int block)
        {
            light.SetBlock(x, y, z, block + 1);
        }
        private void UpdateHeightCache()
        {
            if (lightheight != null)
            {
                lightheight.Clear();
            }
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
        //Dictionary<Point, int> lightheight = new Dictionary<Point, int>();
        InfiniteHeightCache lightheight = new InfiniteHeightCache();
        int GetLightHeight(int x, int y)
        {
            var p = new Point(x, y);
            var height = lightheight.GetBlock(x, y);
            if (height == 0)
            {
                UpdateLightHeightmapAt(x, y);
                height = lightheight.GetBlock(x, y);
            }
            return height - 1;
        }
        void UpdateLightHeightmapAt(int x, int y)
        {
            lightheight.SetBlock(x, y, GetRealLightHeightAt(x, y) + 1);
        }
        InfiniteMapCache light = new InfiniteMapCache();
        int minlight = 0;
        public int maxlight { get { return 16; } }
        void UpdateSunlight(int x, int y, int z)
        {
            if (lightheight == null) { ResetShadows(); }
            int oldheight = GetLightHeight(x, y);
            if (oldheight < 0) { oldheight = 0; }
            if (oldheight >= map.MapSizeZ) { oldheight = map.MapSizeZ - 1; }
            UpdateLightHeightmapAt(x, y);
            int newheight = GetLightHeight(x, y);
            if (newheight < oldheight)
            {
                //make white
                for (int i = newheight; i <= oldheight; i++)
                {
                    SetLight(x, y, i, maxlight);
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
                lighttoupdate[new Vector3i((x / 16) * 16 + 5, (y / 16) * 16 + 5, (z / 16) * 16 + 5)] = true;

                foreach (Vector3i v in BlocksNear(x, y, z))
                {
                    if (v.x / 16 != x / 16
                        || v.y / 16 != y / 16
                        || v.z / 16 != z / 16)
                    {
                        lighttoupdate[new Vector3i((v.x / 16) * 16 + 5, (v.y / 16) * 16 + 5, (v.z / 16) * 16 + 5)] = true;
                    }
                }
            }
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
                    if (LightGetBlock(n.x, n.y, n.z) <= vlight)
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
        Queue<Vector3i> q = new Queue<Vector3i>();
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
            q.Enqueue(new Vector3i(x, y, z));
            for (; ; )
            {
                if (q.Count == 0)
                {
                    break;
                }
                Vector3i v = q.Dequeue();
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
                foreach (var n in BlocksNear(v.x, v.y, v.z))
                {
                    if (!MapUtil.IsValidPos(map, n.x, n.y, n.z))
                    {
                        continue;
                    }
                    if (LightGetBlockFast(n.x, n.y, n.z) < vlight - 1)
                    {
                        SetLight(n.x, n.y, n.z, (byte)(vlight - 1));
                        q.Enqueue(n);
                    }
                }
            }
        }
        int LightGetBlockFast(int x, int y, int z)
        {
            if (currentlightchunk != null && InSameChunk(x, y, z, startx, starty, startz))
            {
                int block = currentlightchunk[x % chunksize, y % chunksize, z % chunksize];
                if (block == 0)//unknown
                {
                    UpdateStartSunlightChunk(x, y);
                }
                return block - 1;
            }
            return LightGetBlock(x, y, z);
        }
        bool InSameChunk(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            return x1 / chunksize == x2 / chunksize
                && y1 / chunksize == y2 / chunksize
                && z1 / chunksize == z2 / chunksize;
        }
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
            if (IsSolidChunk(currentlightchunk, (byte)(maxlight + 1)))
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
            Dictionary<Vector3i, bool> lighttoupdatecopy = lighttoupdate;
            lighttoupdate = null;
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        FloodLight(x + xx, y + yy, z + zz);
                    }
                }
            }
            lighttoupdate = lighttoupdatecopy;
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
            if (chunklighted == null)
            {
                chunklighted = new bool[map.MapSizeX / chunksize, map.MapSizeY / chunksize, map.MapSizeZ / chunksize];
            }

            //Commented out: no need to flood light from chunks around, 
            //because TerrainDrawer calls this function for blocks in all 9 chunks around
            //when drawing single terrain chunk to draw its boundaries anyway.

            //foreach (var k in BlocksNear(x / chunksize, y / chunksize, z / chunksize))
            var k = new Vector3i(x / chunksize, y / chunksize, z / chunksize);
            {
                if (!IsValidChunkPos(k.x, k.y, k.z))
                {
                    //continue;
                }
                if (!chunklighted[k.x, k.y, k.z])
                {
                    lock (lighttoupdate_lock)
                    {
                        FloodLightChunk(k.x * chunksize, k.y * chunksize, k.z * chunksize);
                        chunklighted[k.x, k.y, k.z] = true;
                    }
                }
            }
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
    }
    class InfiniteHeightCache
    {
        int chunksize = 16;
        Dictionary<Point, byte[,]> gencache = new Dictionary<Point, byte[,]>();
        public int GetBlock(int x, int y)
        {
            byte[,] chunk = GetChunk(x, y);
            return chunk[x % chunksize, y % chunksize];
        }
        public byte[,] GetChunk(int x, int y)
        {
            byte[,] chunk = null;
            var k = new Point(x / chunksize, y / chunksize);
            if (!gencache.TryGetValue(k, out chunk))
            {
                chunk = new byte[chunksize, chunksize];
                gencache[k] = chunk;
            }
            return chunk;
        }
        public void SetBlock(int x, int y, int blocktype)
        {
            GetChunk(x, y)[x % chunksize, y % chunksize] = (byte)blocktype;
        }
        public void Clear()
        {
            gencache = new Dictionary<Point, byte[,]>();
        }
        public void ClearChunk(int x, int y)
        {
            int px = (x / chunksize) * chunksize;
            int py = (y / chunksize) * chunksize;
            gencache.Remove(new Point(px, py));
        }
    }
    class InfiniteMapCache
    {
        int chunksize = 16;
        Dictionary<ulong, byte[, ,]> gencache = new Dictionary<ulong, byte[, ,]>();
        public int GetBlock(int x, int y, int z)
        {
            byte[, ,] chunk = GetChunk(x, y, z);
            return chunk[x % chunksize, y % chunksize, z % chunksize];
        }
        public byte[, ,] GetChunk(int x, int y, int z)
        {
            byte[, ,] chunk = null;
            var k = MapUtil.ToMapPos(x / chunksize, y / chunksize, z / chunksize);
            if (!gencache.TryGetValue(k, out chunk))
            {
                chunk = new byte[chunksize, chunksize, chunksize];
                gencache[k] = chunk;
            }
            return chunk;
        }
        public void SetBlock(int x, int y, int z, int blocktype)
        {
            GetChunk(x, y, z)[x % chunksize, y % chunksize, z % chunksize] = (byte)blocktype;
        }
        public void ClearChunk(int x, int y, int z)
        {
            int px = (x / chunksize) * chunksize;
            int py = (y / chunksize) * chunksize;
            int pz = (z / chunksize) * chunksize;
            gencache.Remove(MapUtil.ToMapPos(px, py, pz));
        }
    }
}

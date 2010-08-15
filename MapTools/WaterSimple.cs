using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace ManicDigger
{
    //todo water and sponges may not work when map is saved during flooding and then loaded.
    public class WaterSimple
    {
        [Inject]
        public IGameData data { get; set; }
        public void Update()
        {
            if ((DateTime.Now - lastflood).TotalSeconds > 1)
            {
                lastflood = DateTime.Now;
                var curtoflood = new List<Vector3>(toflood.Keys);
                foreach (var v in curtoflood)
                {
                    Flood(v);
                    toflood.Remove(v);
                }
            }
        }
        int spongerange = 2;
        bool IsSpongeNear(int x, int y, int z)
        {
            for (int xx = x - spongerange; xx <= x + spongerange; xx++)
            {
                for (int yy = y - spongerange; yy <= y + spongerange; yy++)
                {
                    for (int zz = z - spongerange; zz <= z + spongerange; zz++)
                    {
                        if (MapUtil.IsValidPos(map, xx, yy, zz) && map.GetBlock(xx, yy, zz) == data.TileIdSponge)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void BlockChange(IMapStorage map, int x, int y, int z)
        {
            this.flooded = new Dictionary<Vector3, Vector3>();
            this.map = map;
            //sponge just built.
            if (MapUtil.IsValidPos(map, x, y, z) && map.GetBlock(x, y, z) == data.TileIdSponge)
            {
                for (int xx = x - spongerange; xx <= x + spongerange; xx++)
                {
                    for (int yy = y - spongerange; yy <= y + spongerange; yy++)
                    {
                        for (int zz = z - spongerange; zz <= z + spongerange; zz++)
                        {
                            if (MapUtil.IsValidPos(map, xx, yy, zz) && data.IsWaterTile(map.GetBlock(xx, yy, zz)))
                            {
                                tosetempty.Add(new Vector3(xx, yy, zz));
                            }
                        }
                    }
                }
            }
            //maybe sponge destroyed. todo faster test.
            for (int xx = x - spongerange; xx <= x + spongerange; xx++)
            {
                for (int yy = y - spongerange; yy <= y + spongerange; yy++)
                {
                    for (int zz = z - spongerange; zz <= z + spongerange; zz++)
                    {
                        if (MapUtil.IsValidPos(map, xx, yy, zz) && map.GetBlock(xx, yy, zz) == data.TileIdEmpty)
                        {
                            BlockChangeFlood(map, xx, yy, zz);
                        }
                    }
                }
            }
            BlockChangeFlood(map, x, y, z);
            var v = new Vector3(x, y, z);
            tosetwater.Sort((a, b) => (v - a).Length.CompareTo((v - b).Length));
        }
        void BlockChangeFlood(IMapStorage map, int x, int y, int z)
        {
            //water here
            if (MapUtil.IsValidPos(map, x, y, z)
                && data.IsWaterTile(map.GetBlock(x, y, z)))
            {
                Flood(new Vector3(x, y, z));
                return;
            }
            //water around
            foreach (var vv in BlocksAround(new Vector3(x, y, z)))
            {
                if (MapUtil.IsValidPos(map, (int)vv.X, (int)vv.Y, (int)vv.Z) &&
                    data.IsWaterTile(map.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z)))
                {
                    Flood(vv);
                    return;
                }
            }
        }
        IMapStorage map;
        Dictionary<Vector3, Vector3> flooded = new Dictionary<Vector3, Vector3>();
        public List<Vector3> tosetwater = new List<Vector3>();
        public List<Vector3> tosetempty = new List<Vector3>();
        Dictionary<Vector3, Vector3> toflood = new Dictionary<Vector3, Vector3>();
        DateTime lastflood;
        private void Flood(Vector3 v)
        {
            if (!MapUtil.IsValidPos(map, (int)v.X, (int)v.Y, (int)v.Z))
            {
                return;
            }
            if (flooded.ContainsKey(v))
            {
                return;
            }
            flooded.Add(v, v);
            foreach (Vector3 vv in BlocksAround(v))
            {
                if (!MapUtil.IsValidPos(map, (int)vv.X, (int)vv.Y, (int)vv.Z))
                {
                    continue;
                }
                var type = map.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z);
                if (type == data.TileIdEmpty && (!IsSpongeNear((int)vv.X, (int)vv.Y, (int)vv.Z)))
                {
                    tosetwater.Add(vv);
                    toflood[vv] = vv;
                }
            }
        }
        IEnumerable<Vector3> BlocksAround(Vector3 pos)
        {
            yield return pos + new Vector3(-1, 0, 0);
            yield return pos + new Vector3(1, 0, 0);
            yield return pos + new Vector3(0, -1, 0);
            yield return pos + new Vector3(0, 1, 0);
            yield return pos + new Vector3(0, 0, -1);
            //yield return pos + new Vector3(0, 0, 1); //water does not flow up.
        }
    }
}

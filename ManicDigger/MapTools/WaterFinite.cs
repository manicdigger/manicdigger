using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace ManicDigger.MapTools
{
    public class WaterFinite
    {
        [Inject]
        public IGameData data;
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
                            if (MapUtil.IsValidPos(map, xx, yy, zz) && IsWater(map.GetBlock(xx, yy, zz)))
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
            tosetwater.Sort((a, b) => (v - a.pos).Length.CompareTo((v - b.pos).Length));
        }
        void BlockChangeFlood(IMapStorage map, int x, int y, int z)
        {
            //water here
            if (MapUtil.IsValidPos(map, x, y, z)
                && IsWater(map.GetBlock(x, y, z)))
            {
                Flood(new Vector3(x, y, z));
                return;
            }
            //water around
            foreach (var vv in BlocksAround(new Vector3(x, y, z)))
            {
                if (MapUtil.IsValidPos(map, (int)vv.X, (int)vv.Y, (int)vv.Z) &&
                    IsWater(map.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z)))
                {
                    Flood(vv);
                    return;
                }
            }
        }
        IMapStorage map;
        Dictionary<Vector3, Vector3> flooded = new Dictionary<Vector3, Vector3>();
        public struct ToSet
        {
            public Vector3 pos;
            public int level;
        }
        public List<ToSet> tosetwater = new List<ToSet>();
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
            int level = GetWaterLevel((byte)map.GetBlock((int)v.X, (int)v.Y, (int)v.Z));
            flooded.Add(v, v);

            foreach (Vector3 vv in BlocksAround(v))
            {
                if (!MapUtil.IsValidPos(map, (int)vv.X, (int)vv.Y, (int)vv.Z))
                {
                    continue;
                }
                var type = map.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z);
                if (!MapUtil.IsValidPos(map, (int)vv.X, (int)vv.Y, (int)vv.Z - 1))
                {
                    continue;
                }
                int under = map.GetBlock((int)v.X, (int)v.Y, (int)v.Z - 1);
                int nextlevel;
                //is ground under, then can flow to sides
                if (under != 0 && !IsWater(under))
                {
                    nextlevel = level - 1;
                    if (nextlevel < 0)
                    {
                        continue;
                    }
                }
                //is no ground under, then can only flow down.
                else
                {
                    if (vv.Z == v.Z)
                    {
                        continue;
                    }
                    nextlevel = waterLevelsCount - 1;
                }
                if (GetWaterLevel(type) > nextlevel)
                {
                    //already higher water level
                    continue;
                }
                if (type == data.TileIdEmpty && (!IsSpongeNear((int)vv.X, (int)vv.Y, (int)vv.Z)))
                {
                    tosetwater.Add(new ToSet() { pos = vv, level = nextlevel });
                    toflood[vv] = vv;
                }
            }
        }
        int waterLevelsCount = 8;
        int PartialWaterBlock = 118;
        bool IsWater(int tt)
        {
            return (tt >= PartialWaterBlock && tt < PartialWaterBlock + waterLevelsCount)
            || data.IsWater[tt];
        }
        private int GetWaterLevel(int tt)
        {
            if (tt >= PartialWaterBlock && tt < PartialWaterBlock + waterLevelsCount)
            {
                return tt - PartialWaterBlock;
            }
            if (data.IsWater[tt])
            {
                return waterLevelsCount;
            }
            if (tt == 0)
            {
                return -1;
            }
            return -1;
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

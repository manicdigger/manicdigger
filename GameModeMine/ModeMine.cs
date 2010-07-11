using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Collisions;
using OpenTK;

namespace ManicDigger
{
    public class GameMinecraft : IGameMode, IMapStorage, IClients, ITerrainInfo
    {
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public IViewport3d viewport { get; set; }
        [Inject]
        public INetworkClient network { get; set; }
        [Inject]
        public IGameData data { get; set; }
        public void OnPick(Vector3 blockposnew, Vector3 blockposold, Vector3 pos3d, bool right)
        {
            var mode = right ? BlockSetMode.Create : BlockSetMode.Destroy;
            if (IsAnyPlayerInPos(blockposnew))
            {
                return;
            }
            int activematerial = (byte)viewport.MaterialSlots[viewport.activematerial];
            network.SendSetBlock(blockposnew, mode, activematerial);
            if (mode == BlockSetMode.Destroy)
            {
                activematerial = data.TileIdEmpty;
            }
            //speculative
            int x = (int)blockposnew.X;
            int y = (int)blockposnew.Y;
            int z = (int)blockposnew.Z;
            map.Map[x, y, z] = (byte)activematerial;

            terrain.UpdateTile(x, y, z);
            if (ENABLE_SHADOWS)
            {
                lock (lighttoupdate_lock)
                {
                    UpdateShadows(x, y, z);
                }
            }
        }
        private void UpdateShadows(int x, int y, int z)
        {
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
        Dictionary<Vector3i, Vector3i> lighttoupdate = new Dictionary<Vector3i, Vector3i>();
        object lighttoupdate_lock = new object();
        public void SendSetBlock(Vector3 vector3, BlockSetMode blockSetMode, int p)
        {
            network.SendSetBlock(vector3, blockSetMode, p);
        }
        private bool IsAnyPlayerInPos(Vector3 blockpos)
        {
            foreach (var k in players)
            {
                Vector3 playerpos = k.Value.Position;
                if (IsPlayerInPos(playerpos, blockpos))
                {
                    return true;
                }
            }
            return IsPlayerInPos(viewport.LocalPlayerPosition, blockpos);
        }
        private bool IsPlayerInPos(Vector3 playerpos, Vector3 blockpos)
        {
            if (Math.Floor(playerpos.X) == blockpos.X
                &&
                (Math.Floor(playerpos.Y + 0.5f) == blockpos.Z
                 || Math.Floor(playerpos.Y + 1 + 0.5f) == blockpos.Z)
                && Math.Floor(playerpos.Z) == blockpos.Y)
            {
                return true;
            }
            return false;
        }
        public void OnNewFrame(double dt)
        {
        }
        public IEnumerable<ICharacterToDraw> Characters
        {
            get { yield break; }
        }
        Vector3 playerpositionspawn = new Vector3(15.5f, 64, 15.5f);
        public Vector3 PlayerPositionSpawn { get { return playerpositionspawn; } set { playerpositionspawn = value; } }
        public MapStorage map = new MapStorage();
        public Vector3 PlayerOrientationSpawn { get { return new Vector3((float)Math.PI, 0, 0); } }
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int,Player> Players { get { return players; } set { players = value; } }
        public GameMinecraft()
        {
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
        }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, int tileType)
        {
            map.Map[x, y, z] = (byte)tileType;
            if (ENABLE_SHADOWS)
            {
                if (loaded)
                {
                    UpdateShadows(x, y, z);
                }
            }
        }
        #endregion
        public bool ENABLE_SHADOWS = false;
        //float waterlevel = 32;
        #region IMapStorage Members
        //public float WaterLevel { get { return waterlevel; } set { waterlevel = value; } }
        public float WaterLevel { get { return MapSizeZ / 2; } set { } }
        #endregion
        #region IMapStorage Members
        public byte[, ,] Map { get { return map.Map; } set { map.Map = value; } }
        public int MapSizeX { get { return map.MapSizeX; } set { map.MapSizeX = value; } }
        public int MapSizeY { get { return map.MapSizeY; } set { map.MapSizeY = value; } }
        public int MapSizeZ { get { return map.MapSizeZ; } set { map.MapSizeZ = value; } }
        #endregion
        #region IMapStorage Members
        public void Dispose()
        {
        }
        #endregion
        #region IGameMode Members
        public void OnNewMap()
        {
            int x = map.MapSizeX / 2;
            int y = map.MapSizeY / 2;
            playerpositionspawn = new Vector3(x + 0.5f, MapUtil.blockheight(map, data.TileIdEmpty, x, y), y + 0.5f);
        }
        #endregion
        #region IMapStorage Members
        public int GetBlock(int x, int y, int z)
        {
            return Map[x, y, z];
        }
        #endregion
        #region IMapStorage Members
        public void UseMap(byte[, ,] map)
        {
            this.map.Map = map;
            MapSizeX = map.GetUpperBound(0) + 1;
            MapSizeY = map.GetUpperBound(1) + 1;
            MapSizeZ = map.GetUpperBound(2) + 1;
            ResetShadows();
            loaded = true;
        }
        private void ResetShadows()
        {
            if (ENABLE_SHADOWS)
            {
                light = null;
                lightheight = null;
                chunklighted = null;
                UpdateHeightCache();
            }
        }
        #endregion
        MapManipulator mapmanipulator = new MapManipulator();
        #region IGameMode Members
        public byte[] SaveState()
        {
            return mapmanipulator.SaveMap(map);
        }
        public void LoadState(byte[] savegame)
        {
            mapmanipulator.LoadMap(map, savegame);
            ResetShadows();
            loaded = true;
        }
        bool loaded = false;
        private void UpdateLight()
        {
            light = new byte[MapSizeX, MapSizeY, MapSizeZ];
            UpdateHeightCache();
            for (int x = 0; x < MapSizeX; x++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    for (int z = 0; z < MapSizeZ; z++)
                    {
                        if (z >= lightheight[x, y])
                        {
                            light[x, y, z] = (byte)maxlight;
                        }
                        else
                        {
                            light[x, y, z] = (byte)minlight;
                        }
                    }
                }
            }
        }
        private void UpdateHeightCache()
        {
            if (lightheight == null)
            {
                lightheight = new int[MapSizeX, MapSizeY];
            }
            for (int x = 0; x < MapSizeX; x++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    UpdateLightHeightmapAt(x, y);
                }
            }
        }
        private void UpdateLightHeightmapAt(int x, int y)
        {
            lightheight[x, y] = MapSizeZ - 1;
            for (int z = MapSizeZ - 1; z >= 0; z--)
            {
                if (data.GrassGrowsUnder(map.GetBlock(x, y, z)))
                {
                    lightheight[x, y]--;
                }
                else
                {
                    break;
                }
            }
        }
        int[,] lightheight;
        byte[, ,] light;
        int minlight = 1;
        int maxlight = 10;
        int defaultshadow = 7;
        void UpdateSunlight(int x, int y, int z)
        {
            int oldheight = lightheight[x, y];
            if (oldheight < 0) { oldheight = 0; }
            if (oldheight >= map.MapSizeZ) { oldheight = map.MapSizeZ - 1; }
            UpdateLightHeightmapAt(x, y);
            int newheight = lightheight[x, y];
            if (newheight < oldheight)
            {
                //make white
                for (int i = newheight; i <= oldheight; i++)
                {
                    SetLight(x, y, i, maxlight);
                    FloodLight(x, y, i);
                }
            }
            if (newheight > oldheight)
            {
                //make black
                for (int i = oldheight; i <= newheight; i++)
                {
                    SetLight(x, y, i, minlight);
                    //DefloodLight(x, i);

                    List<Vector3i> deflood = new List<Vector3i>();
                    foreach (var n in BlocksNear(x, y, i))
                    {
                        if (MapUtil.IsValidPos(map, n.x, n.y, n.z))
                        {
                            deflood.Add(n);
                        }
                    }
                    if (deflood.Count != 0)
                    {
                        DefloodLight(deflood);
                    }
                }
            }
        }
        void SetLight(int x,int y,int z, int value)
        {
            light[x, y, z] = (byte)value;
            lighttoupdate[new Vector3i((x / 16) * 16 + 5, (y / 16) * 16 + 5, (z / 16) * 16 + 5)] = new Vector3i();
            foreach(Vector3i v in BlocksNear(x,y,z))
            {
                if (v.x / 16 != x / 16
                    || v.y / 16 != y / 16
                    || v.z / 16 != z / 16)
                {
                    lighttoupdate[new Vector3i((v.x / 16) * 16 + 5, (v.y / 16) * 16 + 5, (v.z / 16) * 16 + 5)] = new Vector3i();
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
            for (; ; )
            {
                if (q.Count == 0)
                {
                    break;
                }
                Vector3i v = q.Dequeue();
                if (distancesquare(v, new Vector3i(ss.x, ss.y, ss.z)) > maxlight * maxlight)
                {
                    continue;
                }
                if (!data.GrassGrowsUnder(map.GetBlock(v.x, v.y, v.z))
                    && !IsLightEmitting(map.GetBlock(v.x, v.y, v.z)))
                {
                    continue;
                }
                if (light[v.x, v.y, v.z] == minlight)
                {
                    continue;
                }
                if (light[v.x, v.y, v.z] == maxlight)
                {
                    reflood[v] = true;
                    continue;
                }
                SetLight(v.x, v.y, v.z, minlight);
                foreach (var n in BlocksNear(v.x, v.y, v.z))
                {
                    if (!MapUtil.IsValidPos(map, n.x, n.y, n.z))
                    {
                        continue;
                    }
                    if (light[n.x, n.y, n.z] > light[v.x, v.y, v.z])
                    {
                        q.Enqueue(n);
                    }
                }
            }
            foreach (var p in reflood.Keys)
            {
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
        private void FloodLight(int x, int y, int z)
        {
            if (light == null)
            {
                UpdateLight();
            }
            if (IsLightEmitting(GetBlock(x, y, z)))
            {
                light[x, y, z] = (byte)(maxlight);
            }
            Queue<Vector3i> q = new Queue<Vector3i>();
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
                if (light[v.x, v.y, v.z] == minlight)
                {
                    continue;
                }
                if (!data.GrassGrowsUnder(map.GetBlock(v.x, v.y, v.z))
                    && !IsLightEmitting(map.GetBlock(v.x,v.y,v.z)))
                {
                    continue;
                }
                foreach (var n in BlocksNear(v.x, v.y, v.z))
                {
                    if (!MapUtil.IsValidPos(map, n.x, n.y, n.z))
                    {
                        continue;
                    }
                    if (light[n.x, n.y, n.z] < light[v.x, v.y, v.z] - 1)
                    {
                        SetLight(n.x, n.y, n.z, (byte)(light[v.x, v.y, v.z] - 1));
                        q.Enqueue(n);
                    }
                }
            }
        }
        private bool IsLightEmitting(int blocktype)
        {
            return blocktype == (int)TileTypeMinecraft.GoldBlock
                || blocktype == (int)TileTypeMinecraft.Torch
                || blocktype == (int)TileTypeMinecraft.Lava
                || blocktype == (int)TileTypeMinecraft.StationaryLava
                || blocktype == (int)TileTypeMinecraft.InfiniteLavaSource;
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
        #endregion
        #region IGameMode Members
        public IEnumerable<IModelToDraw> Models
        {
            get { yield break; }
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryAmount(int blocktype)
        {
            return 0;
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryMax { get { return 0; } }
        #endregion
        #region IGameMode Members
        public double SIMULATIONLAG_SECONDS { get; set; }
        #endregion
        const int chunksize = 16;
        #region ITerrainInfo Members
        public int GetTerrainBlock(int x, int y, int z)
        {
            if (ENABLE_SHADOWS)
            {
                if (chunklighted == null)
                {
                    chunklighted = new bool[MapSizeX / chunksize, MapSizeY / chunksize, MapSizeZ / chunksize];
                }
                foreach (var k in BlocksNear(x / chunksize, y / chunksize, z / chunksize))
                {
                    if (!IsValidChunkPos(k.x, k.y, k.z))
                    {
                        continue;
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
            return GetBlock(x, y, z);
        }
        private void FloodLightChunk(int x, int y, int z)
        {
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
        }
        bool IsValidChunkPos(int cx, int cy, int cz)
        {
            return cx >= 0 && cy >= 0 && cz >= 0
                && cx < MapSizeX / chunksize
                && cy < MapSizeY / chunksize
                && cz < MapSizeZ / chunksize;
        }
        bool[, ,] chunklighted;
        public System.Drawing.Color GetTerrainBlockColor(int x, int y, int z)
        {
            return System.Drawing.Color.White;
        }
        public int GetLight(int x, int y, int z)
        {
            if (ENABLE_SHADOWS)
            {
                if (light == null)
                {
                    UpdateLight();
                }
                return light[x, y, z];
            }
            else
            {
                return IsShadow(x, y, z) ? defaultshadow : maxlight;
            }
        }
        public float LightMaxValue()
        {
            return maxlight;
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
    }
    public class TileTypeData
    {
        public bool Buildable;
        public int TextureTop;
        public int TextureSide;
        public int TextureBottom;
        public int AllTextures
        {
            set
            {
                TextureTop = value;
                TextureSide = value;
                TextureBottom = value;
            }
        }
    }
    public class GameDataTilesMinecraft : IGameData
    {
        public GameDataTilesMinecraft()
        {
            MakeData();
        }
        public byte TileIdEmpty
        {
            get { return (byte)TileTypeMinecraft.Empty; }
        }
        public byte TileIdGrass
        {
            get { return (byte)TileTypeMinecraft.Grass; }
        }
        public byte TileIdDirt
        {
            get { return (byte)TileTypeMinecraft.Dirt; }
        }
        public int GetTileTextureId(int tileType, TileSide side)
        {
            if (side == TileSide.Top) { return data[tileType].TextureTop; }
            if (side == TileSide.Bottom) { return data[tileType].TextureBottom; }
            return data[tileType].TextureSide;
        }
        public int[] DefaultMaterialSlots
        {
            get
            {
                var slots = new List<int>();
                slots.Add((int)TileTypeMinecraft.Dirt);
                slots.Add((int)TileTypeMinecraft.Stone);
                slots.Add((int)TileTypeMinecraft.Cobblestone);
                slots.Add((int)TileTypeMinecraft.Wood);
                slots.Add((int)TileTypeMinecraft.Sand);
                slots.Add((int)TileTypeMinecraft.Gravel);
                slots.Add((int)TileTypeMinecraft.Leaves);
                slots.Add((int)TileTypeMinecraft.Stair);
                slots.Add((int)TileTypeMinecraft.Glass);
                slots.Add((int)TileTypeMinecraft.Sponge);
                return slots.ToArray();
            }
        }
        public byte GoldTileId
        {
            get { return (int)TileTypeMinecraft.GoldOre; }
        }
        #region IGameData Members
        public int TileIdStone
        {
            get { return (int)TileTypeMinecraft.Stone; }
        }
        public int TileIdWater
        {
            get { return (int)TileTypeMinecraft.StationaryWater; }
        }
        public int TileIdSand
        {
            get { return (int)TileTypeMinecraft.Sand; }
        }
        public bool IsWaterTile(int tiletype)
        {
            return tiletype == (int)TileTypeMinecraft.Water
                || tiletype == (int)TileTypeMinecraft.StationaryWater
                || tiletype == (int)TileTypeMinecraft.InfiniteWaterSource
                || tiletype == (int)TileTypeMinecraft.Lava
                || tiletype == (int)TileTypeMinecraft.StationaryLava
                || tiletype == (int)TileTypeMinecraft.InfiniteLavaSource;
        }
        #endregion
        #region IGameData Members
        public bool IsBuildableTile(int tiletype)
        {
            if (!IsValidTileType(tiletype)) { throw new ArgumentException(); }
            //if (tiletype == 41) { return false; }//?
            //if (tiletype == 29) { return false; }//?
            return data[tiletype].Buildable;
        }
        #endregion
        public bool IsValidTileType(int tiletype)
        {
            return data[tiletype] != null;
        }
        void MakeData()
        {
            data[(int)TileTypeMinecraft.Stone] = new TileTypeData() { Buildable = true, AllTextures = 1 };
            data[(int)TileTypeMinecraft.Grass] = new TileTypeData()
            {
                Buildable = false,
                TextureBottom = 2,
                TextureSide = 3,
                TextureTop = 0,
            };
            data[(int)TileTypeMinecraft.Dirt] = new TileTypeData() { Buildable = true, AllTextures = 2 };
            data[(int)TileTypeMinecraft.Cobblestone] = new TileTypeData() { Buildable = true, AllTextures = (1 * 16) + 0 };
            data[(int)TileTypeMinecraft.Wood] = new TileTypeData() { Buildable = true, AllTextures = 4 };
            data[(int)TileTypeMinecraft.Sapling] = new TileTypeData() { Buildable = true, AllTextures = 15 }; //special
            data[(int)TileTypeMinecraft.Adminium] = new TileTypeData() { Buildable = true, AllTextures = (1 * 16) + 1 };
            data[(int)TileTypeMinecraft.Water] = new TileTypeData() { Buildable = false, AllTextures = 14 };
            data[(int)TileTypeMinecraft.StationaryWater] = new TileTypeData() { Buildable = false, AllTextures = 14 };
            data[(int)TileTypeMinecraft.Lava] = new TileTypeData() { Buildable = false, AllTextures = (1 * 16) + 14 };
            data[(int)TileTypeMinecraft.StationaryLava] = new TileTypeData() { Buildable = false, AllTextures = (1 * 16) + 14 };
            data[(int)TileTypeMinecraft.Sand] = new TileTypeData() { Buildable = true, AllTextures = (1 * 16) + 2 };
            data[(int)TileTypeMinecraft.Gravel] = new TileTypeData() { Buildable = true, AllTextures = (1 * 16) + 3 };
            data[(int)TileTypeMinecraft.GoldOre] = new TileTypeData() { Buildable = false, AllTextures = (2 * 16) + 0 };
            data[(int)TileTypeMinecraft.IronOre] = new TileTypeData() { Buildable = false, AllTextures = (2 * 16) + 1 };
            data[(int)TileTypeMinecraft.CoalOre] = new TileTypeData() { Buildable = false, AllTextures = (2 * 16) + 2 };
            data[(int)TileTypeMinecraft.TreeTrunk] = new TileTypeData()
            {
                Buildable = true,
                TextureTop = (1 * 16) + 5,
                TextureBottom = (1 * 16) + 5,
                TextureSide = (1 * 16) + 4,
            };
            data[(int)TileTypeMinecraft.Leaves] = new TileTypeData() { Buildable = true, AllTextures = (1 * 16) + 6 };
            data[(int)TileTypeMinecraft.Sponge] = new TileTypeData() { Buildable = true, AllTextures = (3 * 16) + 0 };
            data[(int)TileTypeMinecraft.Glass] = new TileTypeData() { Buildable = true, AllTextures = (3 * 16) + 1 };
            data[(int)TileTypeMinecraft.RedCloth] = new TileTypeData() { Buildable = true, AllTextures = 64 };
            data[(int)TileTypeMinecraft.RedCloth + 1] = new TileTypeData() { Buildable = true, AllTextures = 64 + 1 };
            data[(int)TileTypeMinecraft.RedCloth + 2] = new TileTypeData() { Buildable = true, AllTextures = 64 + 2 };
            data[(int)TileTypeMinecraft.RedCloth + 3] = new TileTypeData() { Buildable = true, AllTextures = 64 + 3 };
            data[(int)TileTypeMinecraft.RedCloth + 4] = new TileTypeData() { Buildable = true, AllTextures = 64 + 4 };
            data[(int)TileTypeMinecraft.RedCloth + 5] = new TileTypeData() { Buildable = true, AllTextures = 64 + 5 };
            data[(int)TileTypeMinecraft.RedCloth + 6] = new TileTypeData() { Buildable = true, AllTextures = 64 + 6 };
            data[(int)TileTypeMinecraft.RedCloth + 7] = new TileTypeData() { Buildable = true, AllTextures = 64 + 7 };
            data[(int)TileTypeMinecraft.RedCloth + 8] = new TileTypeData() { Buildable = true, AllTextures = 64 + 8 };
            data[(int)TileTypeMinecraft.RedCloth + 9] = new TileTypeData() { Buildable = true, AllTextures = 64 + 9 };
            data[(int)TileTypeMinecraft.RedCloth + 10] = new TileTypeData() { Buildable = true, AllTextures = 64 + 10 };
            data[(int)TileTypeMinecraft.RedCloth + 11] = new TileTypeData() { Buildable = true, AllTextures = 64 + 11 };
            data[(int)TileTypeMinecraft.RedCloth + 12] = new TileTypeData() { Buildable = true, AllTextures = 64 + 12 };
            data[(int)TileTypeMinecraft.RedCloth + 13] = new TileTypeData() { Buildable = true, AllTextures = 64 + 13 };
            data[(int)TileTypeMinecraft.RedCloth + 14] = new TileTypeData() { Buildable = true, AllTextures = 64 + 14 };
            data[(int)TileTypeMinecraft.RedCloth + 15] = new TileTypeData() { Buildable = true, AllTextures = 64 + 15 };//36
            data[(int)TileTypeMinecraft.YellowFlowerDecorations] = new TileTypeData() { Buildable = true, AllTextures = 13 };
            data[(int)TileTypeMinecraft.RedRoseDecorations] = new TileTypeData() { Buildable = true, AllTextures = 12 };
            data[(int)TileTypeMinecraft.RedMushroom] = new TileTypeData() { Buildable = true, AllTextures = 28 };
            data[(int)TileTypeMinecraft.BrownMushroom] = new TileTypeData() { Buildable = true, AllTextures = 29 };
            data[(int)TileTypeMinecraft.Lava] = new TileTypeData() { Buildable = false, AllTextures = 30 };
            data[(int)TileTypeMinecraft.GoldBlock] = new TileTypeData() { Buildable = true, AllTextures = 24 };
            data[(int)TileTypeMinecraft.IronBlock] = new TileTypeData() { Buildable = true, AllTextures = 23 };
            data[(int)TileTypeMinecraft.DoubleStair] = new TileTypeData() { Buildable = true, AllTextures = (0 * 16) + 5, TextureTop = 6 };//43 todo
            data[(int)TileTypeMinecraft.Stair] = new TileTypeData() { Buildable = true, AllTextures = 6 };//44
            data[(int)TileTypeMinecraft.TNT] = new TileTypeData()
            {
                Buildable = true,
                TextureSide = (0 * 16) + 8,
                TextureTop = (0 * 16) + 9,
                TextureBottom = (0 * 16) + 10,
            };//45
            data[(int)TileTypeMinecraft.Brick] = new TileTypeData() { Buildable = true, AllTextures = (6 * 16) + 7 };//46
            data[(int)TileTypeMinecraft.Bookcase] = new TileTypeData()
            {
                Buildable = true,
                TextureSide = (2 * 16) + 3,
                TextureBottom = (0 * 16) + 4,
                TextureTop = (0 * 16) + 4,
            };//47
            data[(int)TileTypeMinecraft.MossyCobblestone] = new TileTypeData() { Buildable = true, AllTextures = (2 * 16) + 4 };//48
            data[(int)TileTypeMinecraft.Obsidian] = new TileTypeData() { Buildable = true, AllTextures = (2 * 16) + 5 };//49
            //torch todo
            //fire todo
            data[(int)TileTypeMinecraft.InfiniteWaterSource] = new TileTypeData() { Buildable = false, AllTextures = 14 };//52
            data[(int)TileTypeMinecraft.InfiniteLavaSource] = new TileTypeData() { Buildable = false, AllTextures = 30 };//53
            data[(int)TileTypeMinecraft.Chest] = new TileTypeData() { Buildable = false, AllTextures = 4 };//54
            //gear todo
            //diamond todo
            //diamond block todo
            //crafting table todo
            //crops todo
            //soil todo
            //furnace todo
            //burning furnace todo
        }
        TileTypeData[] data = new TileTypeData[256];
        /*
            if (blockUp == 0 || blockUp == 8 || blockUp == 9 ||
                blockUp == 10 || blockUp == 11 || blockUp == 18 ||
                blockUp == 44 || blockUp == 6 || blockUp == 37 ||
                blockUp == 38 || blockUp == 39 || blockUp == 40 ||
                blockLeft == 0 || blockLeft == 8 || blockLeft == 9 ||
                blockLeft == 10 || blockLeft == 11 || blockLeft == 18 ||
                blockLeft == 44 || blockLeft == 6 || blockLeft == 37 ||
                blockLeft == 38 || blockLeft == 39 || blockLeft == 40 ||
                blockRight == 0 || blockRight == 8 || blockRight == 9 ||
                blockRight == 10 || blockRight == 11 || blockRight == 18 ||
                blockRight == 44 || blockRight == 6 || blockRight == 37 ||
                blockRight == 38 || blockRight == 39 || blockRight == 40)
                Blend(block);
        */
        public bool IsTransparentTile(int tileType)
        {
            return
                tileType == (byte)TileTypeMinecraft.Empty
                || tileType == (byte)TileTypeMinecraft.Sapling
                || tileType == (byte)TileTypeMinecraft.Crops
                //|| tileType == (byte)TileTypeMinecraft.Water
                //|| tileType == (byte)TileTypeMinecraft.StationaryWater
                //|| tileType == (byte)TileTypeMinecraft.Lava
                //|| tileType == (byte)TileTypeMinecraft.StationaryLava
                || tileType == (byte)TileTypeMinecraft.YellowFlowerDecorations
                || tileType == (byte)TileTypeMinecraft.RedRoseDecorations
                || tileType == (byte)TileTypeMinecraft.Leaves
                || tileType == (byte)TileTypeMinecraft.Glass
                || tileType == (byte)TileTypeMinecraft.RedMushroom
                || tileType == (byte)TileTypeMinecraft.BrownMushroom
                || tileType == (byte)TileTypeMinecraft.Stair // transparent on side
                || tileType == (byte)TileTypeMinecraft.InfiniteWaterSource;
        }
        public int PlayerBuildableMaterialType(int tt)
        {
            TileTypeMinecraft t = (TileTypeMinecraft)tt;
            if (t == TileTypeMinecraft.Grass)
            {
                return (int)TileTypeMinecraft.Dirt;
            }
            if (t == TileTypeMinecraft.Water || t == TileTypeMinecraft.Lava) //...
            {
                return (int)TileTypeMinecraft.Dirt;
            }
            return (int)t;
        }
        #region IGameData Members
        public bool IsBlockFlower(int tiletype)
        {
            return tiletype == (int)TileTypeMinecraft.YellowFlowerDecorations
                || tiletype == (int)TileTypeMinecraft.RedRoseDecorations
                || tiletype == (int)TileTypeMinecraft.Sapling
                || tiletype == (int)TileTypeMinecraft.RedMushroom
                || tiletype == (int)TileTypeMinecraft.BrownMushroom;
        }
        #endregion
        #region IGameData Members
        public RailDirectionFlags GetRail(int tiletype)
        {
            return RailDirectionFlags.None;
        }
        #endregion
        #region IGameData Members
        public int TileIdSingleStairs
        {
            get { return (int)TileTypeMinecraft.Stair; }
        }
        #endregion
        #region IGameData Members
        public int TileIdSponge
        {
            get { return (int)TileTypeMinecraft.Sponge; }
        }
        #endregion
        #region IGameData Members
        public int GetTileTextureIdForInventory(int tileType)
        {
            return GetTileTextureId(tileType, TileSide.Front);
        }
        #endregion
        #region IGameData Members
        public string BlockName(int blocktype)
        {
            return Enum.GetName(typeof(TileTypeMinecraft), blocktype);
        }
        #endregion
        #region IGameData Members
        public bool IsEmptyForPhysics(int blocktype)
        {
            return blocktype == (int)TileTypeMinecraft.BrownMushroom
                || blocktype == (int)TileTypeMinecraft.Crops
                || blocktype == (int)TileTypeMinecraft.RedMushroom
                || blocktype == (int)TileTypeMinecraft.RedRoseDecorations
                || blocktype == (int)TileTypeMinecraft.Sapling
                || blocktype == (int)TileTypeMinecraft.YellowFlowerDecorations;
        }
        #endregion
        #region IGameData Members
        public float BlockWalkSpeed(int blocktype)
        {
            if (blocktype == (int)TileTypeMinecraft.Cobblestone)
            {
                return 1.5f;
            }
            return 1;
        }
        #endregion
        #region IGameData Members
        public bool GrassGrowsUnder(int blocktype)
        {
            return blocktype == TileIdEmpty
                || IsBlockFlower(blocktype)
                || blocktype == (int)TileTypeMinecraft.Leaves
                || blocktype == (int)TileTypeMinecraft.Glass
                || IsWaterTile(blocktype);
        }
        #endregion
        #region IGameData Members
        public bool IsSlipperyWalk(int blocktype)
        {
            return false;
        }
        #endregion
        string[] soundwalk = { "walk1.wav", "walk2.wav", "walk3.wav", "walk4.wav" };
        #region IGameData Members
        public string[] WalkSound(int blocktype)
        {
            if (blocktype == (int)TileTypeMinecraft.Empty)
            {
                return new string[0];
            }
            return soundwalk;
        }
        #endregion
    }
    //http://www.minecraftwiki.net/wiki/Blocks,Items_%26_Data_values
    public enum TileTypeMinecraft : byte
    {
        Empty = 0,
        Stone,
        Grass,
        Dirt,
        Cobblestone,
        Wood,
        Sapling,
        Adminium,
        Water,
        StationaryWater,
        Lava,
        StationaryLava,
        Sand,
        Gravel,
        GoldOre,
        IronOre,
        CoalOre,
        TreeTrunk,
        Leaves,
        Sponge,
        Glass,
        RedCloth,
        OrangeCloth,
        YellowCloth,
        LightGreenCloth,
        GreenCloth,
        AquaGreenCloth,
        CyanCloth,
        BlueCloth,
        PurpleCloth,
        IndigoCloth,
        VioletCloth,
        //dec  hex  Block type  ,
        MagentaCloth,
        PinkCloth,
        BlackCloth,
        GrayCloth,
        WhiteCloth,
        YellowFlowerDecorations,
        RedRoseDecorations,
        RedMushroom,
        BrownMushroom,
        GoldBlock,
        IronBlock,
        DoubleStair,
        Stair,
        Brick,
        TNT,
        Bookcase,
        MossyCobblestone,
        Obsidian,
        Torch,
        FireBlock,
        InfiniteWaterSource,
        InfiniteLavaSource,
        Chest,
        Gear,
        DiamondPre,
        DiamondBlock,
        CraftingTable,
        Crops,
        Soil,
        Furnace,
        BurningFurnace,
    }
}

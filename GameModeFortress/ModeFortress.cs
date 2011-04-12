using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using OpenTK;
using System.IO;
using System.Drawing;
using ManicDigger.Renderers;

namespace GameModeFortress
{
    public class CraftingTableTool
    {
        [Inject]
        public IMapStorage map;
        public List<int> GetOnTable(List<Vector3i> table)
        {
            List<int> ontable = new List<int>();
            foreach (var v in table)
            {
                int t = map.GetBlock(v.x, v.y, v.z + 1);
                ontable.Add(t);
            }
            return ontable;
        }
        public int maxcraftingtablesize = 2000;
        public List<Vector3i> GetTable(Vector3i pos)
        {
            List<Vector3i> l = new List<Vector3i>();
            Queue<Vector3i> todo = new Queue<Vector3i>();
            todo.Enqueue(pos);
            for (; ; )
            {
                if (todo.Count == 0 || l.Count >= maxcraftingtablesize)
                {
                    break;
                }
                var p = todo.Dequeue();
                if (l.Contains(p))
                {
                    continue;
                }
                l.Add(p);
                var a = new Vector3i(p.x + 1, p.y, p.z);
                if (map.GetBlock(a.x, a.y, a.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(a);
                }
                var b = new Vector3i(p.x - 1, p.y, p.z);
                if (map.GetBlock(b.x, b.y, b.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(b);
                }
                var c = new Vector3i(p.x, p.y + 1, p.z);
                if (map.GetBlock(c.x, c.y, c.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(c);
                }
                var d = new Vector3i(p.x, p.y - 1, p.z);
                if (map.GetBlock(d.x, d.y, d.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(d);
                }
            }
            return l;
        }
    }
    public interface ICurrentTime
    {
        int SimulationCurrentFrame { get; }
    }
    public class CurrentTimeDummy : ICurrentTime
    {
        #region ICurrentTime Members
        public int SimulationCurrentFrame { get { return 0; } }
        #endregion
    }
    public partial class GameFortress : IGameMode, IMapStorage, IClients, ITerrainInfo, INetworkPacketReceived, ICurrentSeason, IMapStorageLight
    {
        [Inject]
        public ITerrainRenderer terrain;
        [Inject]
        public ITerrainTextures terrainTextures;
        [Inject]
        public IViewport3d viewport;
        [Inject]
        public INetworkClientFortress network;
        [Inject]
        public IGameData data;
        [Inject]
        public IShadows shadows;
        [Inject]
        public InfiniteMapChunked map;
        public CraftingRecipes craftingrecipes = new CraftingRecipes();
        [Inject]
        public CraftingTableTool craftingtabletool;
        [Inject]
        public ManicDigger.Renderers.SunMoonRenderer sunmoonrenderer;
        public void OnPick(Vector3 blockpos, Vector3 blockposold, Vector3 pos3d, bool right)
        {
            float xfract = pos3d.X - (float)Math.Floor(pos3d.X);
            float zfract = pos3d.Z - (float)Math.Floor(pos3d.Z);
            int activematerial = (byte)viewport.MaterialSlots[viewport.activematerial];
            int railstart = GameDataManicDigger.railstart;
            if (activematerial == railstart + (int)RailDirectionFlags.TwoHorizontalVertical
                || activematerial == railstart + (int)RailDirectionFlags.Corners)
            {
                RailDirection dirnew;
                if (activematerial == railstart + (int)RailDirectionFlags.TwoHorizontalVertical)
                {
                    dirnew = PickHorizontalVertical(xfract, zfract);
                }
                else
                {
                    dirnew = PickCorners(xfract, zfract);
                }
                RailDirectionFlags dir = data.Rail[GetTerrainBlock((int)blockposold.X, (int)blockposold.Y, (int)blockposold.Z)];
                if (dir != RailDirectionFlags.None)
                {
                    blockpos = blockposold;
                }
                activematerial = railstart + (int)(dir | DirectionUtils.ToRailDirectionFlags(dirnew));
                //Console.WriteLine(blockposold);
                //Console.WriteLine(xfract + ":" + zfract + ":" + activematerial + ":" + dirnew);
            }
            int x = (short)blockpos.X;
            int y = (short)blockpos.Y;
            int z = (short)blockpos.Z;
            var mode = right ? BlockSetMode.Create : BlockSetMode.Destroy;
            {
                if (IsAnyPlayerInPos(blockpos))
                {
                    return;
                }
                Vector3i v = new Vector3i(x, y, z);
                Vector3i? oldfillstart = fillstart;
                Vector3i? oldfillend = fillend;
                if (mode == BlockSetMode.Create)
                {
                    if (activematerial == (int)TileTypeManicDigger.Cuboid)
                    {
                        ClearFillArea();

                        if (fillstart != null)
                        {
                            Vector3i f = fillstart.Value;
                            if (!IsFillBlock(map.GetBlock(f.x, f.y, f.z)))
                            {
                                fillarea[f] = map.GetBlock(f.x, f.y, f.z);
                            }
                            map.SetBlock(f.x, f.y, f.z, (int)TileTypeManicDigger.FillStart);


                            FillFill(v, fillstart.Value);
                        }
                        if (!IsFillBlock(map.GetBlock(v.x, v.y, v.z)))
                        {
                            fillarea[v] = map.GetBlock(v.x, v.y, v.z);
                        }
                        map.SetBlock(v.x, v.y, v.z, (int)TileTypeManicDigger.Cuboid);
                        fillend = v;
                        terrain.UpdateTile(v.x, v.y, v.z);
                        return;
                    }
                    if (activematerial == (int)TileTypeManicDigger.FillStart)
                    {
                        ClearFillArea();
                        if (!IsFillBlock(map.GetBlock(v.x, v.y, v.z)))
                        {
                            fillarea[v] = map.GetBlock(v.x, v.y, v.z);
                        }
                        map.SetBlock(v.x, v.y, v.z, (int)TileTypeManicDigger.FillStart);
                        fillstart = v;
                        fillend = null;
                        terrain.UpdateTile(v.x, v.y, v.z);
                        return;
                    }
                    if (fillarea.ContainsKey(v))// && fillarea[v])
                    {
                        foreach (var p in fillarea)
                        {
                            //if (p.Value)
                            {
                                if (activematerial == (int)TileTypeManicDigger.FillArea)
                                {
                                    SendSetBlock(new Vector3(p.Key.x, p.Key.y, p.Key.z), BlockSetMode.Destroy, activematerial);
                                }
                                else
                                {
                                    SendSetBlock(new Vector3(p.Key.x, p.Key.y, p.Key.z), BlockSetMode.Create, activematerial);
                                }
                            }
                        }
                        ClearFillArea();
                        fillstart = null;
                        fillend = null;
                        return;
                    }
                }
                else
                {
                    //delete fill start
                    if (fillstart != null && fillstart == v)
                    {
                        ClearFillArea();
                        fillstart = null;
                        fillend = null;
                        return;
                    }
                    //delete fill end
                    if (fillend != null && fillend == v)
                    {
                        ClearFillArea();
                        fillend = null;
                        return;
                    }
                }
                if (mode == BlockSetMode.Create && activematerial == (int)TileTypeManicDigger.Minecart)
                {
                    /*
                    CommandRailVehicleBuild cmd2 = new CommandRailVehicleBuild();
                    cmd2.x = (short)x;
                    cmd2.y = (short)y;
                    cmd2.z = (short)z;
                    TrySendCommand(MakeCommand(CommandId.RailVehicleBuild, cmd2));
                    */
                    return;
                }
                //if (TrySendCommand(MakeCommand(CommandId.Build, cmd)))
                SendSetBlockAndUpdateSpeculative(activematerial, x, y, z, mode);
            }
        }
        private void SendSetBlockAndUpdateSpeculative(int activematerial, int x, int y, int z, BlockSetMode mode)
        {
            network.SendSetBlock(new Vector3(x, y, z), mode, activematerial);
            if (mode == BlockSetMode.Destroy)
            {
                activematerial = SpecialBlockId.Empty;
            }
            speculative[new Vector3i(x, y, z)] = new Speculative() { blocktype = map.GetBlock(x, y, z), time = DateTime.UtcNow };
            map.SetBlock(x, y, z, activematerial);
            terrain.UpdateTile(x, y, z);
            shadows.OnLocalBuild(x, y, z);
        }
        private void ClearFillArea()
        {
            foreach (var k in fillarea)
            {
                var vv = k.Key;
                map.SetBlock(vv.x, vv.y, vv.z, k.Value);
                terrain.UpdateTile(vv.x, vv.y, vv.z);
            }
            fillarea.Clear();
        }
        //value is original block.
        Dictionary<Vector3i, int> fillarea = new Dictionary<Vector3i, int>();
        Vector3i? fillstart;
        Vector3i? fillend;
        int MAXFILL = 200;
        private void FillFill(Vector3i a, Vector3i b)
        {
            int startx = Math.Min(a.x, b.x);
            int endx = Math.Max(a.x, b.x);
            int starty = Math.Min(a.y, b.y);
            int endy = Math.Max(a.y, b.y);
            int startz = Math.Min(a.z, b.z);
            int endz = Math.Max(a.z, b.z);
            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    for (int z = startz; z <= endz; z++)
                    {
                        if (fillarea.Count > MAXFILL)
                        {
                            ClearFillArea();
                            return;
                        }
                        if(!IsFillBlock(map.GetBlock(x,y,z)))
                        {
                            fillarea[new Vector3i(x, y, z)] = map.GetBlock(x, y, z);
                            map.SetBlock(x, y, z, (int)TileTypeManicDigger.FillArea);
                            terrain.UpdateTile(x, y, z);
                        }
                    }
                }
            }
        }
        bool IsFillBlock(int blocktype)
        {
            return blocktype == (int)TileTypeManicDigger.FillArea
                || blocktype == (int)TileTypeManicDigger.FillStart
                || blocktype == (int)TileTypeManicDigger.Cuboid;
        }
        RailDirection PickHorizontalVertical(float xfract, float yfract)
        {
            float x = xfract;
            float y = yfract;
            if (y >= x && y >= (1 - x))
            {
                return RailDirection.Vertical;
            }
            if (y < x && y < (1 - x))
            {
                return RailDirection.Vertical;
            }
            return RailDirection.Horizontal;
        }
        private RailDirection PickCorners(float xfract, float zfract)
        {
            if (xfract < 0.5f && zfract < 0.5f)
            {
                return RailDirection.UpLeft;
            }
            if (xfract >= 0.5f && zfract < 0.5f)
            {
                return RailDirection.UpRight;
            }
            if (xfract < 0.5f && zfract >= 0.5f)
            {
                return RailDirection.DownLeft;
            }
            return RailDirection.DownRight;
        }
        struct Speculative
        {
            public DateTime time;
            public int blocktype;
        }
        Dictionary<Vector3i, Speculative> speculative = new Dictionary<Vector3i, Speculative>();
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
            foreach (var k in new Dictionary<Vector3i, Speculative>(speculative))
            {
                if ((DateTime.UtcNow - k.Value.time).TotalSeconds > 2)
                {
                    speculative.Remove(k.Key);
                    terrain.UpdateTile(k.Key.x, k.Key.y, k.Key.z);
                }
            }
            if (KeyPressed(viewport.GetKey(OpenTK.Input.Key.C)))
            {
                if (viewport.PickCubePos != new Vector3(-1, -1, -1))
                {
                    Vector3i pos = new Vector3i((int)viewport.PickCubePos.X, (int)viewport.PickCubePos.Z, (int)viewport.PickCubePos.Y);
                    if (map.GetBlock(pos.x, pos.y, pos.z)
                        == (int)TileTypeManicDigger.CraftingTable)
                    {
                        //draw crafting recipes list.
                        viewport.CraftingRecipesStart(craftingrecipes.craftingrecipes, craftingtabletool.GetOnTable(craftingtabletool.GetTable(pos)),
                        (recipe) => { CraftingRecipeSelected(pos, recipe); });
                    }
                }
            }
            if (KeyPressed(viewport.GetKey(OpenTK.Input.Key.U)) || KeyPressed(viewport.GetKey(OpenTK.Input.Key.L)))
            {
                if (viewport.PickCubePos != new Vector3(-1, -1, -1))
                {
                    Vector3i pos = new Vector3i((int)viewport.PickCubePos.X,
                        (int)viewport.PickCubePos.Z,
                        (int)viewport.PickCubePos.Y);
                    {
                        DoCommandDumpOrLoad(pos.x,pos.y,pos.z,KeyPressed(viewport.GetKey(OpenTK.Input.Key.U)),
                            viewport.MaterialSlots[viewport.activematerial]);
                    }
                }
            }
            viewport.FiniteInventory = FiniteInventory;
            viewport.ENABLE_FINITEINVENTORY = this.ENABLE_FINITEINVENTORY;
            RailOnNewFrame((float)dt);
        }
        private bool DoCommandDumpOrLoad(int x,int y,int z, bool dump, int blocktype)
        {
            if (!ENABLE_FINITEINVENTORY)
            {
                return false;
            }
            bool execute = true;
            Dictionary<int, int> inventory = FiniteInventory;
            int dumpcount = 0;
            if (inventory.ContainsKey(blocktype))
            {
                dumpcount = inventory[blocktype];
            }
            if (dumpcount > 50) { dumpcount = 50; }
            Vector3i pos = new Vector3i(x, y, z);
            if (execute)
            {
                if (map.GetBlock(pos.x, pos.y, pos.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    List<Vector3i> table = craftingtabletool.GetTable(pos);
                    if (dump)
                    {
                        int dumped = 0;
                        foreach (Vector3i v in table)
                        {
                            if (dumped >= table.Count / 2 || dumped >= dumpcount)
                            {
                                break;
                            }
                            if (GetBlock(v.x, v.y, v.z + 1) == SpecialBlockId.Empty)
                            {
                                SendSetBlockAndUpdateSpeculative(blocktype, v.x, v.y, v.z + 1, BlockSetMode.Create);
                                dumped++;
                            }
                        }
                    }
                    else
                    {
                        foreach (Vector3i v in table)
                        {
                            if (TotalAmount(inventory) + 1 > FiniteInventoryMax)
                            {
                                break;
                            }
                            int b = GetBlock(v.x, v.y, v.z + 1);
                            if (b != SpecialBlockId.Empty)
                            {
                                SendSetBlockAndUpdateSpeculative(0, v.x, v.y, v.z + 1, BlockSetMode.Destroy);
                            }
                        }
                    }
                    return true;
                }
                if (dump)
                {
                    for (int i = 0; i < dumpcount; i++)
                    {
                        //find empty position that is nearest to dump place AND has a block under.
                        Vector3i? nearpos = FindDumpPlace(pos);
                        if (nearpos == null)
                        {
                            break;
                        }
                        SendSetBlockAndUpdateSpeculative(blocktype, nearpos.Value.x, nearpos.Value.y, nearpos.Value.z, BlockSetMode.Create);
                    }
                }
            }
            return true;
        }
        private Vector3i? FindDumpPlace(Vector3i pos)
        {
            List<Vector3i> l = new List<Vector3i>();
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    for (int z = 0; z < 10; z++)
                    {
                        int xx = pos.x + x - 10 / 2;
                        int yy = pos.y + y - 10 / 2;
                        int zz = pos.z + z - 10 / 2;
                        if (!MapUtil.IsValidPos(map, xx, yy, zz))
                        {
                            continue;
                        }
                        if (GetBlock(xx, yy, zz) == SpecialBlockId.Empty
                            && GetBlock(xx, yy, zz - 1) != SpecialBlockId.Empty)
                        {
                            bool playernear = false;
                            if (players != null)
                            {
                                foreach (var player in players)
                                {
                                    if ((player.Value.Position - new Vector3(xx, zz, yy)).Length < 3)
                                    {
                                        playernear = true;
                                    }
                                }
                            }
                            if (!playernear)
                            {
                                l.Add(new Vector3i(xx, yy, zz));
                            }
                        }
                    }
                }
            }
            l.Sort((a, b) => Length(Minus(a, pos)).CompareTo(Length(Minus(b, pos))));
            if (l.Count > 0)
            {
                return l[0];
            }
            return null;
        }
        private Vector3i Minus(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        int Length(Vector3i v)
        {
            return (int)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }
        int TotalAmount(Dictionary<int, int> inventory)
        {
            int sum = 0;
            foreach (var k in inventory)
            {
                sum += k.Value;
            }
            return sum;
        }
        void CraftingRecipeSelected(Vector3i pos, int? recipe)
        {
            if (recipe == null)
            {
                return;
            }
            PacketClientCraft cmd = new PacketClientCraft();
            cmd.X = (short)pos.x;
            cmd.Y = (short)pos.y;
            cmd.Z = (short)pos.z;
            cmd.RecipeId = (short)recipe.Value;
            network.SendPacketClient(new PacketClient() { PacketId = ClientPacketId.Craft, Craft = cmd });
        }
        private bool KeyPressed(OpenTK.Input.Key key)
        {
            return viewport.keypressed != null && viewport.keypressed.Key == key;
        }
        private bool KeyDepressed(OpenTK.Input.Key key)
        {
            return viewport.keydepressed != null && viewport.keydepressed.Key == key;
        }
        Dictionary<int, int> FiniteInventory = new Dictionary<int, int>();
        public IEnumerable<ICharacterToDraw> Characters
        {
            get { yield break; }
        }
        Vector3 playerpositionspawn = new Vector3(15.5f, 64, 15.5f);
        public Vector3 PlayerPositionSpawn { get { return playerpositionspawn; } set { playerpositionspawn = value; } }
        public Vector3 PlayerOrientationSpawn { get { return new Vector3((float)Math.PI, 0, 0); } }
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int, Player> Players { get { return players; } set { players = value; } }
        public GameFortress()
        {
            /*
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
            */
        }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, int tileType)
        {
            map.SetBlock(x, y, z, (byte)tileType);
            shadows.OnSetBlock(x, y, z);
        }
        #endregion
        #region IMapStorage Members
        public void SetChunk(int x, int y, int z, byte[, ,] chunk)
        {
            map.SetMapPortion(x, y, z, chunk);
        }
        #endregion
        #region IMapStorage Members
        public byte[, ,] Map { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public int MapSizeX { get { return map.MapSizeX; } set { map.MapSizeX = value; } }
        public int MapSizeY { get { return map.MapSizeY; } set { map.MapSizeY = value; } }
        public int MapSizeZ { get { return map.MapSizeZ; } set { map.MapSizeZ = value; } }
        #endregion
        #region IGameMode Members
        public void OnNewMap()
        {
            int x = map.MapSizeX / 2;
            int y = map.MapSizeY / 2;
            playerpositionspawn = new Vector3(x + 0.5f, MapUtil.blockheight(map, SpecialBlockId.Empty, x, y), y + 0.5f);
        }
        #endregion
        #region IMapStorage Members
        public int GetBlock(int x, int y, int z)
        {
            return map.GetBlock(x, y, z);
        }
        #endregion
        #region IMapStorage Members
        public void UseMap(byte[, ,] map)
        {
            /*
            this.map.Map = map;
            MapSizeX = map.GetUpperBound(0) + 1;
            MapSizeY = map.GetUpperBound(1) + 1;
            MapSizeZ = map.GetUpperBound(2) + 1;
            shadows.ResetShadows();
            */
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryAmount(int blocktype)
        {
            if (!FiniteInventory.ContainsKey(blocktype))
            {
                return 0;
            }
            return FiniteInventory[blocktype];
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryMax { get; set; }
        #endregion
        #region IGameMode Members
        public double SIMULATIONLAG_SECONDS { get; set; }
        #endregion
        #region ITerrainInfo Members
        public int GetTerrainBlock(int x, int y, int z)
        {
            shadows.OnGetTerrainBlock(x, y, z);
            return GetBlock(x, y, z);
        }
        FastColor white = new FastColor(Color.White);
        public FastColor GetTerrainBlockColor(int x, int y, int z)
        {
            return white;
        }
        public int GetLight(int x, int y, int z)
        {
            return shadows.GetLight(x, y, z);
        }
        public float LightMaxValue()
        {
            return shadows.maxlight;
        }
        #endregion
        #region IGameMode Members
        public void ModelClick(int selectedmodelid)
        {
        }
        #endregion
        #region IGameWorld Members
        public string GameInfo
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region IGameWorldRun Members
        public void Tick()
        {
        }
        public void DoCommand(byte[] command, int player_id)
        {
        }
        public int GetStateHash()
        {
            return 0;
        }
        #endregion
        public IMapStorage mapforphysics { get { return this; } }
        public bool ENABLE_FINITEINVENTORY { get; set; }
        #region INetworkPacketReceived Members
        public bool NetworkPacketReceived(PacketServer packet)
        {
            switch (packet.PacketId)
            {
                case ServerPacketId.FiniteInventory:
                    {
                        FiniteInventory = packet.FiniteInventory.BlockTypeAmount;
                        ENABLE_FINITEINVENTORY = packet.FiniteInventory.IsFinite;
                        FiniteInventoryMax = packet.FiniteInventory.Max;
                    }
                    return true;
                case ServerPacketId.Season:
                    {
                        if (packet.Season.Season != CurrentSeason)
                        {
                            CurrentSeason = packet.Season.Season;
                            data.Update();
                            if (CurrentSeason == 0 || CurrentSeason == 3)
                            {
                                terrain.UpdateAllTiles();
                            }
                        }
                        if (packet.Season.Hour == int.MinValue)
                        {
                            //why this happens?
                            //server never sents this value but it gets received here anyway.
                            //default was 12 but it caused a 1-hour long day at hour 23.
                            //fix: changed default value to int.MinValue to at least detect this.
                            if (!received_sun)
                            {
                                packet.Season.Hour = 6;
                                received_sun = true;
                            }
                            else
                            {
                                packet.Season.Hour = sunmoonrenderer.Hour;
                            }
                        }
                        int sunlight;
                        if (packet.Season.Hour >= 6 && packet.Season.Hour < 18)
                        {
                            sunlight = shadows.maxlight;
                            viewport.SkySphereNight = false;
                        }
                        else
                        {
                            sunlight = packet.Season.Moon == 0 ? 0 : 1;
                            viewport.SkySphereNight = true;
                        }
                        sunmoonrenderer.day_length_in_seconds = 60 * 60 * 24 / packet.Season.DayNightCycleSpeedup;
                        sunmoonrenderer.Hour = packet.Season.Hour;

                        if (shadows.sunlight != sunlight)
                        {
                            shadows.sunlight = sunlight;
                            shadows.ResetShadows();
                            terrain.UpdateAllTiles();
                        }
                    }
                    return true;
                case ServerPacketId.BlobInitialize:
                    {
                        blobdownload = new MemoryStream();
                        blobdownloadhash = ByteArrayToString(packet.BlobInitialize.hash);
                        ((NetworkClientFortress)network).ReceivedMapLength = 0; //todo
                    }
                    return true;
                case ServerPacketId.BlobPart:
                    {
                        BinaryWriter bw = new BinaryWriter(blobdownload);
                        bw.Write(packet.BlobPart.data);
                        ((NetworkClientFortress)network).ReceivedMapLength += packet.BlobPart.data.Length; //todo
                    }
                    return true;
                case ServerPacketId.BlobFinalize:
                    {
                        blobs[blobdownloadhash] = blobdownload.ToArray();
                        blobdownload = null;
                        if (ENABLE_PER_SERVER_TEXTURES || viewport.Options.UseServerTextures)
                        {
                            if (blobdownloadhash == serverterraintexture)
                            {
                                using (Bitmap bmp = new Bitmap(new MemoryStream(blobs[blobdownloadhash])))
                                {
                                    terrainTextures.UseTerrainTextureAtlas2d(bmp);
                                }
                            }
                        }
                    }
                    return true;
                case ServerPacketId.ServerIdentification:
                    {
                        serverterraintexture = ByteArrayToString(packet.Identification.TerrainTextureMd5);
                    }
                    return true;
                default:
                    return false;
            }
        }
        bool received_sun = false;
        public bool ENABLE_PER_SERVER_TEXTURES = false;
        string serverterraintexture;
        #endregion
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        string blobdownloadhash;
        MemoryStream blobdownload;
        Dictionary<string, byte[]> blobs = new Dictionary<string, byte[]>();
        #region ICurrentSeason Members
        public int CurrentSeason { get; set; }
        #endregion
        #region ITerrainInfo Members
        public byte[] GetChunk(int x, int y, int z)
        {
            return map.GetChunk(x, y, z);
        }
        #endregion
        public void Reset(int sizex, int sizey, int sizez)
        {
            map.Reset(sizex, sizey, sizez);
        }
    }
}

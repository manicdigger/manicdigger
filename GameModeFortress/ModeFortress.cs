using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Collisions;
using ManicDigger;
using OpenTK;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Security;
using OpenTK.Graphics.OpenGL;
using System.Xml.XPath;

namespace GameModeFortress
{
    public class WorldGeneratorSandbox : IWorldGenerator
    {
        static Sandboxer sandboxer = new Sandboxer();
        static object l = new object();
        public void Compile(string s)
        {
            lock (l)
            {
                if (sandboxer != null)
                {
                    sandboxer.Dispose();
                }
                sandboxer = new Sandboxer();
                sandboxer.Main1(s);
            }
        }
        #region IWorldGenerator Members
        public byte[] GetBlocks(int[] pos)
        {
            throw new NotImplementedException();
        }
        public byte[, ,] GetChunk(int x, int y, int z, int chunksize)
        {
            lock (l)
            {
                return (byte[, ,])sandboxer.Call("WorldGenerator", "GetChunk", new object[] { x, y, z, chunksize });
            }
        }
        #endregion
    }
    public interface IWorldGenerator
    {
        byte[] GetBlocks(int[] pos);
        byte[, ,] GetChunk(int x, int y, int z, int chunksize);
    }
    public class InfiniteMap : IMapStorage
    {
        public IWorldGenerator gen { get; set; }
        int mapsizex = 10 * 1000;
        int mapsizey = 10 * 1000;
        int mapsizez = 128;
        #region IMapStorage Members
        public int MapSizeX { get { return mapsizex; } set { mapsizex = value; } }
        public int MapSizeY { get { return mapsizey; } set { mapsizey = value; } }
        public int MapSizeZ { get { return mapsizez; } set { mapsizez = value; } }
        public Dictionary<ulong, byte> blocks = new Dictionary<ulong, byte>();
        public void Restart()
        {
            gencache = new Dictionary<ulong, byte[, ,]>();
        }
        Dictionary<ulong, byte[, ,]> gencache = new Dictionary<ulong, byte[, ,]>();
        public int GetBlock(int x, int y, int z)
        {
            if (blocks.ContainsKey(MapUtil.ToMapPos(x,y,z)))
            {
                return blocks[MapUtil.ToMapPos(x, y, z)];
            }
            else
            {
                byte[, ,] chunk = null;
                var k = MapUtil.ToMapPos(x / 16, y / 16, z / 16);
                if (!gencache.TryGetValue(k, out chunk))
                {
                    chunk = gen.GetChunk(x / 16, y / 16, z / 16, 16);
                    if (gencache.Count > 64 * 64 * 4)
                    {
                        Restart();
                    }
                    gencache[k] = chunk;
                }
                return chunk[x % 16, y % 16, z % 16];
            }
        }
        float waterlevel = 32;
        public float WaterLevel { get { return waterlevel; } set { waterlevel = value; } }
        public void Dispose()
        {
        }
        #endregion
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, int tileType)
        {
            blocks[MapUtil.ToMapPos(x,y,z)] = (byte)tileType;
        }
        public void UseMap(byte[, ,] map)
        {
        }
        #endregion
        public byte[] SaveBlocks()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            NetworkHelper.WriteInt32(bw, blocks.Count);
            foreach (var b in blocks)
            {
                Vector3i v = MapUtil.FromMapPos(b.Key);
                NetworkHelper.WriteInt32(bw, v.x);
                NetworkHelper.WriteInt32(bw, v.y);
                NetworkHelper.WriteInt32(bw, v.z);
                NetworkHelper.WriteInt16(bw, b.Value);
            }
            return ms.ToArray();
        }
        public void LoadBlocks(byte[] blocksdata)
        {
            MemoryStream ms = new MemoryStream(blocksdata);
            BinaryReader br = new BinaryReader(ms);
            int count = NetworkHelper.ReadInt32(br);
            blocks.Clear();
            for (int i = 0; i < count; i++)
            {
                int x = NetworkHelper.ReadInt32(br);
                int y = NetworkHelper.ReadInt32(br);
                int z = NetworkHelper.ReadInt32(br);
                int type = NetworkHelper.ReadInt16(br);
                blocks.Add(MapUtil.ToMapPos(x, y, z), (byte)type);
            }
        }
    }
    public class GameFortress : IGameMode, IGameWorld, IMapStorage, IClients, ITerrainInfo, IGameWorldTodo
    {
        [Inject]
        public WorldGeneratorSandbox worldgeneratorsandbox { get; set; }
        [Inject]
        public InfiniteMap map { get; set; }
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public IViewport3d viewport { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public INetworkClient network { get; set; }
        [Inject]
        public IAudio audio { get; set; }
        public IMapStorage mapforphysics;
        class MapForPhysics : IMapStorage
        {
            public GameFortress game;
            #region IMapStorage Members
            public int MapSizeX { get { return game.MapSizeX; } set { game.MapSizeX = value; } }
            public int MapSizeY { get { return game.MapSizeX; } set { game.MapSizeY = value; } }
            public int MapSizeZ { get { return game.MapSizeZ; } set { game.MapSizeZ = value; } }
            public int GetBlock(int x, int y, int z)
            {
                return game.GetBlockForPhysics(x,y,z);
            }
            public void SetBlock(int x, int y, int z, int tileType)
            {
                game.SetBlock(x, y, z, tileType);
            }
            public float WaterLevel { get { return game.WaterLevel; } set { throw new Exception(); } }
            public void Dispose()
            {
            }
            #endregion
            #region IMapStorage Members
            public void UseMap(byte[, ,] map)
            {
                //this.Map = map;
            }
            #endregion
        }
        public GameFortress()
        {
            /*
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
            */
            mapforphysics = new MapForPhysics() { game = this };
            MakeRecipes();
        }
        float railheight = 0.3f;
        private float minecartheight { get { return 1.5f - 1; } }
        public struct TileEnterData
        {
            public Vector3 BlockPosition;
            public TileEnterDirection EnterDirection;
        }
        public VehicleDirection12Flags PossibleRails(TileEnterData enter)
        {
            Vector3 new_position = enter.BlockPosition;
            VehicleDirection12Flags possible_rails = VehicleDirection12Flags.None;
            if (MapUtil.IsValidPos(map, (int)enter.BlockPosition.X, (int)enter.BlockPosition.Y, (int)enter.BlockPosition.Z))
            {
                RailDirectionFlags newpositionrail = data.GetRail(
                    map.GetBlock((int)enter.BlockPosition.X, (int)enter.BlockPosition.Y, (int)enter.BlockPosition.Z));
                List<VehicleDirection12> all_possible_rails = new List<VehicleDirection12>();
                foreach (var z in DirectionUtils.PossibleNewRails(enter.EnterDirection))
                {
                    if ((newpositionrail & DirectionUtils.ToRailDirectionFlags(DirectionUtils.ToRailDirection(z)))
                        != RailDirectionFlags.None)
                    {
                        all_possible_rails.Add(z);
                    }
                }
                possible_rails = DirectionUtils.ToVehicleDirection12Flags(all_possible_rails);
            }
            return possible_rails;
        }
        public static Vector3 NextTile(VehicleDirection12 direction, Vector3 currentTile)
        {
            return NextTile(DirectionUtils.ResultExit(direction), currentTile);
        }
        public static Vector3 NextTile(TileExitDirection direction, Vector3 currentTile)
        {
            switch (direction)
            {
                case TileExitDirection.Left:
                    return new Vector3(currentTile.X - 1, currentTile.Y, currentTile.Z);
                case TileExitDirection.Right:
                    return new Vector3(currentTile.X + 1, currentTile.Y, currentTile.Z);
                case TileExitDirection.Up:
                    return new Vector3(currentTile.X, currentTile.Y - 1, currentTile.Z);
                case TileExitDirection.Down:
                    return new Vector3(currentTile.X, currentTile.Y + 1, currentTile.Z);
                default:
                    throw new ArgumentException("direction");
            }
        }
        bool railriding
        {
            get
            {
                return railridingall.ContainsKey(viewport.LocalPlayerName)
                    && railridingall[viewport.LocalPlayerName] != -1;
            }
        }
        Dictionary<string, int> railridingall = new Dictionary<string, int>();
        static VehicleDirection12? BestNewDirection(VehicleDirection12Flags dir, bool turnleft, bool turnright)
        {
            if (turnright)
            {
                if ((dir & VehicleDirection12Flags.DownRightRight) != 0)
                {
                    return VehicleDirection12.DownRightRight;
                }
                if ((dir & VehicleDirection12Flags.UpRightUp) != 0)
                {
                    return VehicleDirection12.UpRightUp;
                }
                if ((dir & VehicleDirection12Flags.UpLeftLeft) != 0)
                {
                    return VehicleDirection12.UpLeftLeft;
                }
                if ((dir & VehicleDirection12Flags.DownLeftDown) != 0)
                {
                    return VehicleDirection12.DownLeftDown;
                }
            }
            if (turnleft)
            {
                if ((dir & VehicleDirection12Flags.DownRightDown) != 0)
                {
                    return VehicleDirection12.DownRightDown;
                }
                if ((dir & VehicleDirection12Flags.UpRightRight) != 0)
                {
                    return VehicleDirection12.UpRightRight;
                }
                if ((dir & VehicleDirection12Flags.UpLeftUp) != 0)
                {
                    return VehicleDirection12.UpLeftUp;
                }
                if ((dir & VehicleDirection12Flags.DownLeftLeft) != 0)
                {
                    return VehicleDirection12.DownLeftLeft;
                }
            }
            foreach (var v in DirectionUtils.ToVehicleDirection12s(dir))
            {
                return v;
            }
            return null;
        }
        RailMapUtil railmaputil;
        enum UpDown
        {
            None,
            Up,
            Down,
        }
        UpDown GetUpDownMove(Vector3 railblock, TileEnterDirection dir)
        {
            if (!MapUtil.IsValidPos(map, (int)railblock.X, (int)railblock.Y, (int)railblock.Z))
            {
                return UpDown.None;
            }
            //going up
            RailSlope slope = RailMapUtil().GetRailSlope((int)railblock.X, (int)railblock.Y, (int)railblock.Z);
            if (slope == RailSlope.TwoDownRaised && dir == TileEnterDirection.Up)
            {
                return UpDown.Up;
            }
            if (slope == RailSlope.TwoUpRaised && dir == TileEnterDirection.Down)
            {
                return UpDown.Up;
            }
            if (slope == RailSlope.TwoLeftRaised && dir == TileEnterDirection.Right)
            {
                return UpDown.Up;
            }
            if (slope == RailSlope.TwoRightRaised && dir == TileEnterDirection.Left)
            {
                return UpDown.Up;
            }
            //going down
            if (slope == RailSlope.TwoDownRaised && dir == TileEnterDirection.Down)
            {
                return UpDown.Down;
            }
            if (slope == RailSlope.TwoUpRaised && dir == TileEnterDirection.Up)
            {
                return UpDown.Down;
            }
            if (slope == RailSlope.TwoLeftRaised && dir == TileEnterDirection.Left)
            {
                return UpDown.Down;
            }
            if (slope == RailSlope.TwoRightRaised && dir == TileEnterDirection.Right)
            {
                return UpDown.Down;
            }
            return UpDown.None;
        }
        Dictionary<int, int> StartFiniteInventory()
        {
            Dictionary<int, int> d = new Dictionary<int, int>();
            d[(int)TileTypeManicDigger.CraftingTable] = 6;
            d[(int)TileTypeManicDigger.Crops1] = 1;
            return d;
        }
        //RailVehicle localrailvehicle = new RailVehicle();
        RailVehicle localrailvehicle { get { return vehicles[railridingall[viewport.LocalPlayerName]]; } }
        double simulationaccumulator;
        int simulationcurrentframe;
        int SIMULATIONLAG { get { return (int)(0.3 / SIMULATION_STEP_LENGTH); } }
        public void OnNewFrame(double dt)
        {
            foreach (var k in new Dictionary<Vector3i, Speculative>(speculative))
            {
                if ((DateTime.Now - k.Value.time).TotalSeconds > 2)
                {
                    speculative.Remove(k.Key);
                    terrain.UpdateTile(k.Key.x, k.Key.y, k.Key.z);
                }
            }
            viewport.PerformanceInfo["frame"] = "Frame: " + simulationcurrentframe.ToString();
            simulationaccumulator += dt;
            double simulationdt = SIMULATION_STEP_LENGTH;
            while (simulationaccumulator > simulationdt)
            {
                if (simulationcurrentframe >= simulationallowedkeyframe - SIMULATIONLAG)
                {
                    simulationaccumulator = 0;
                    break;
                }
                simulationaccumulator -= simulationdt;
                Tick();
                //hash check
                if (simulationhashchecktodo.Count > 0)
                {
                    HashCheckTodo todo = simulationhashchecktodo.Peek();
                    if (todo.frame == simulationcurrentframe)
                    {
                        if (GetStateHash() != todo.hash)
                        {
                            Console.WriteLine("Desync.");
                            throw new Exception("Desync.");
                        }
                        simulationhashchecktodo.Dequeue();
                    }
                }
                //commands
                while(simulationcmdtodo.Count > 0)
                {
                    CommandTodo todo = simulationcmdtodo.Peek();
                    if (todo.frame > simulationcurrentframe)
                    {
                        break;
                    }
                    if (todo.frame == simulationcurrentframe)
                    {
                        try
                        {
                            DoCommand(todo.cmd, todo.playerid);
                        }
                        catch
                        {
                            //should not happen, server should check this.
                            Console.WriteLine("Invalid command.");
                        }
                        simulationcmdtodo.Dequeue();
                    }
                    if (todo.frame < simulationcurrentframe)
                    {
                        throw new Exception("Past command.");
                    }
                }
            }

            viewport.FiniteInventory = GetPlayerInventory(viewport.LocalPlayerName);
            bool turnright = viewport.keyboardstate[OpenTK.Input.Key.D];
            bool turnleft = viewport.keyboardstate[OpenTK.Input.Key.A];
            viewport.LocalPlayerAnimationHint.leanleft = railriding && turnleft;
            viewport.LocalPlayerAnimationHint.leanright = railriding && turnright;
            RailSound();
            //UpdateRailVehicle(dt, turnright, turnleft);
            if (railriding && (KeyPressed(OpenTK.Input.Key.W) || KeyPressed(OpenTK.Input.Key.S)
                || KeyPressed(OpenTK.Input.Key.A) || KeyPressed(OpenTK.Input.Key.D)
                || KeyPressed(OpenTK.Input.Key.Q)
                || KeyDepressed(OpenTK.Input.Key.W) || KeyDepressed(OpenTK.Input.Key.S)
                || KeyDepressed(OpenTK.Input.Key.A) || KeyDepressed(OpenTK.Input.Key.D)))
            {
                CommandRailVehicleControl cmd = new CommandRailVehicleControl();
                if (KeyPressed(OpenTK.Input.Key.W)) { cmd.controlaction = ControlAction.AcceleratePlus; }
                if (KeyPressed(OpenTK.Input.Key.S)) { cmd.controlaction = ControlAction.AccelerateMinus; }
                if (KeyPressed(OpenTK.Input.Key.A)) { cmd.controlaction = ControlAction.TurnLeft; }
                if (KeyPressed(OpenTK.Input.Key.D)) { cmd.controlaction = ControlAction.TurnRight; }
                if (KeyPressed(OpenTK.Input.Key.Q)) { cmd.controlaction = ControlAction.Reverse; }

                if (KeyDepressed(OpenTK.Input.Key.W)) { cmd.controlaction = ControlAction.AccelerateNone; }
                if (KeyDepressed(OpenTK.Input.Key.S)) { cmd.controlaction = ControlAction.AccelerateNone; }
                if (KeyDepressed(OpenTK.Input.Key.A)) { cmd.controlaction = ControlAction.TurnNone; }
                if (KeyDepressed(OpenTK.Input.Key.D)) { cmd.controlaction = ControlAction.TurnNone; }
                TrySendCommand(MakeCommand(CommandId.RailVehicleControl, cmd));
            }
            if (railriding)
            {
                viewport.ENABLE_FREEMOVE = true;
                viewport.ENABLE_MOVE = false;
                viewport.LocalPlayerPosition = minecartdrawer.CurrentRailPos(
                    localrailvehicle.currentrailblock,
                    localrailvehicle.currentdirection,
                    (float)localrailvehicle.currentrailblockprogressMul1000 / 1000f);
            }
            viewport.LocalPlayerAnimationHint.InVehicle = railriding;
            viewport.LocalPlayerAnimationHint.DrawFix = railriding ? new Vector3(0, -0.7f, 0) : new Vector3();
            foreach (var k in players)
            {
                if (k.Value.Name == viewport.LocalPlayerName)
                {
                    continue;
                }
                bool r = railridingall.ContainsKey(k.Value.Name) && railridingall[k.Value.Name] != -1;
                if (r)
                {
                    var veh = vehicles[railridingall[k.Value.Name]];
                    k.Value.Position = minecartdrawer.CurrentRailPos(
                        veh.currentrailblock,
                        veh.currentdirection,
                        (float)veh.currentrailblockprogressMul1000 / 1000f);

                }
                network.EnablePlayerUpdatePosition[k.Key] = !r;
                k.Value.AnimationHint.InVehicle = r;
                k.Value.AnimationHint.DrawFix = r ? new Vector3(0, +0.8f, 0) : new Vector3();
            }
            /*
            if (targetspeed < 0)
            {
                targetspeed = 0;
            }
            if (currentvehiclespeed < targetspeed)
            {
                currentvehiclespeed += 10f * (float)dt;
            }
            if (currentvehiclespeed > targetspeed)
            {
                currentvehiclespeed -= 10f * (float)dt;
            }
            */
            if (KeyPressed(OpenTK.Input.Key.V)) { Console.WriteLine("V"); }
            if (KeyPressed(OpenTK.Input.Key.V) && !railriding)
            {
                //todo command
                CommandRailVehicleEnterLeave cmd = new CommandRailVehicleEnterLeave();
                cmd.enter = true;
                cmd.vehicleid = -1;
                for (int i = 0; i < vehicles.Count; i++)
                {
                    if (vehicles[i] == null)
                    {
                        continue;
                    }
                    var currentrailblock = new Vector3((int)viewport.LocalPlayerPosition.X,
                           (int)viewport.LocalPlayerPosition.Z, (int)viewport.LocalPlayerPosition.Y - 1);
                    if ((vehicles[i].currentrailblock - currentrailblock).Length <= 2)
                    {
                        cmd.vehicleid = i;
                    }
                }
                if (cmd.vehicleid != -1)
                {
                    TrySendCommand(MakeCommand(CommandId.RailVehicleEnterLeave, cmd));
                }
            }
            else if (KeyPressed(OpenTK.Input.Key.V) && railriding)
            {
                CommandRailVehicleEnterLeave cmd = new CommandRailVehicleEnterLeave();
                cmd.enter = false;
                TrySendCommand(MakeCommand(CommandId.RailVehicleEnterLeave, cmd));
            }
            if (KeyPressed(OpenTK.Input.Key.C))
            {
                if (viewport.PickCubePos != new Vector3(-1, -1, -1))
                {
                    Vector3i pos=new Vector3i((int)viewport.PickCubePos.X, (int)viewport.PickCubePos.Z, (int)viewport.PickCubePos.Y);
                    if (map.GetBlock(pos.x,pos.y,pos.z)
                        == (int)TileTypeManicDigger.CraftingTable)
                    {
                        //draw crafting recipes list.
                        viewport.GuiStateCraft(craftingrecipes, GetOnTable(GetTable(pos)),
                        (recipe) => { CraftingRecipeSelected(pos, recipe); });
                    }
                }
            }
            if (KeyPressed(OpenTK.Input.Key.U) || KeyPressed(OpenTK.Input.Key.L))
            {
                if (viewport.PickCubePos != new Vector3(-1, -1, -1))
                {
                    Vector3i pos = new Vector3i((int)viewport.PickCubePos.X, (int)viewport.PickCubePos.Z, (int)viewport.PickCubePos.Y);
                    {
                        CommandDumpOrLoad cmd = new CommandDumpOrLoad();
                        cmd.x = (short)pos.x;
                        cmd.y = (short)pos.y;
                        cmd.z = (short)pos.z;
                        cmd.blocktype = (short)viewport.MaterialSlots[viewport.activematerial];
                        cmd.dump = KeyPressed(OpenTK.Input.Key.U);
                        network.SendCommand(MakeCommand(CommandId.DumpOrLoad, cmd));
                    }
                }
            }
        }
        private bool KeyPressed(OpenTK.Input.Key key)
        {
            return viewport.keypressed != null && viewport.keypressed.Key == key;
        }
        private bool KeyDepressed(OpenTK.Input.Key key)
        {
            return viewport.keydepressed != null && viewport.keydepressed.Key == key;
        }
        private void UpdateRailVehicle(double dt, int vehicleid)
        {
            RailVehicle veh = vehicles[vehicleid];
            veh.currentrailblockprogressMul1000 += (int)(veh.currentvehiclespeedMul1000 * (float)dt);
            if ((float)veh.currentrailblockprogressMul1000 / 1000 >= 1)
            {
                veh.lastdirection = veh.currentdirection;
                veh.currentrailblockprogressMul1000 = 0;
                var newenter = new TileEnterData();
                newenter.BlockPosition = NextTile(veh.currentdirection, veh.currentrailblock);
                //slope
                if (GetUpDownMove(veh.currentrailblock,
                    DirectionUtils.ResultEnter(DirectionUtils.ResultExit(veh.currentdirection))) == UpDown.Up)
                {
                    newenter.BlockPosition.Z++;
                }
                if (GetUpDownMove(newenter.BlockPosition + new Vector3(0, 0, -1),
                    DirectionUtils.ResultEnter(DirectionUtils.ResultExit(veh.currentdirection))) == UpDown.Down)
                {
                    newenter.BlockPosition.Z--;
                }

                newenter.EnterDirection = DirectionUtils.ResultEnter(DirectionUtils.ResultExit(veh.currentdirection));
                var newdir = BestNewDirection(PossibleRails(newenter), veh.turnleft, veh.turnright);
                if (newdir == null)
                {
                    //end of rail
                    veh.currentdirection = DirectionUtils.Reverse(veh.currentdirection);
                }
                else
                {
                    veh.currentdirection = newdir.Value;
                    veh.currentrailblock = newenter.BlockPosition;
                }
                /*
                var updown = GetUpDownMove(newenter.BlockPosition, newenter.EnterDirection);
                if (updown == UpDown.Up)
                {
                    currentvehiclespeed -= 0.5f;
                }
                if (updown == UpDown.Down)
                {
                    currentvehiclespeed += 0.5f;
                }
                */
            }
            if (veh.accelerate)//if (viewport.keyboardstate[OpenTK.Input.Key.W])
            {
                veh.currentvehiclespeedMul1000 += (int)(1f * (float)dt * 1000);
            }
            if (veh.decelerate)//if (viewport.keyboardstate[OpenTK.Input.Key.S])
            {
                veh.currentvehiclespeedMul1000 -= (int)(5f * (float)dt * 1000);
            }
            if (veh.currentvehiclespeedMul1000 > veh.maxvehiclespeedMul1000)
            {
                veh.currentvehiclespeedMul1000 = (int)veh.maxvehiclespeedMul1000;
            }
            if (veh.currentvehiclespeedMul1000 < 0)
            {
                veh.currentvehiclespeedMul1000 = 0;
            }
        }
        void CraftingRecipeSelected(Vector3i pos, int? recipe)
        {
            if (recipe == null)
            {
                return;
            }
            CommandCraft cmd = new CommandCraft();
            cmd.x = (short)pos.x;
            cmd.y = (short)pos.y;
            cmd.z = (short)pos.z;
            cmd.recipeid = (short)recipe.Value;
            network.SendCommand(MakeCommand(CommandId.Craft, cmd));
        }
        private List<Vector3i> GetTable(Vector3i pos)
        {
            int maxcraftingtablesize = 200;
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
        DateTime lastrailsoundtime;
        int lastrailsound;
        private void RailSound()
        {
            if (!railriding)
            {
                audio.PlayAudioLoop("railnoise.wav", false);
                return;
            }
            float railsoundpersecond = localrailvehicle.currentvehiclespeedMul1000 / 1000f;
            if (railsoundpersecond > 10)
            {
                railsoundpersecond = 10;
            }
            audio.PlayAudioLoop("railnoise.wav", railriding && railsoundpersecond > 0.1f);
            if ((DateTime.Now - lastrailsoundtime).TotalSeconds > 1 / railsoundpersecond)
            {
                audio.Play("rail" + (lastrailsound + 1) + ".wav");
                lastrailsoundtime = DateTime.Now;
                lastrailsound++;
                if (lastrailsound >= 4)
                {
                    lastrailsound = 0;
                }
            }
        }
        private float WalkCharacterHeight = 1.5f;
        private RailMapUtil RailMapUtil()
        {
            if (railmaputil == null)
            {
                railmaputil = new RailMapUtil() { data = data, mapstorage = this };
            }
            return railmaputil;
        }
        float targetspeed = 0;
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
        public void OnPick(OpenTK.Vector3 blockpos,Vector3 blockposold, OpenTK.Vector3 pos3d, bool right)
        {
            float xfract = pos3d.X - (float)Math.Floor(pos3d.X);
            float zfract = pos3d.Z - (float)Math.Floor(pos3d.Z);
            int activematerial = (byte)viewport.MaterialSlots[viewport.activematerial];
            int railstart = GameDataTilesManicDigger.railstart;
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
                RailDirectionFlags dir = data.GetRail(GetTerrainBlock((int)blockposold.X, (int)blockposold.Y, (int)blockposold.Z));
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
                var cmd = new CommandBuild()
                {
                    x = (short)blockpos.X,
                    y = (short)blockpos.Y,
                    z = (short)blockpos.Z,
                    mode = mode,
                    tiletype = (byte)activematerial,
                };
                if (TrySendCommand(MakeCommand(CommandId.Build, cmd)))
                {
                    if (mode == BlockSetMode.Destroy)
                    {
                        activematerial = data.TileIdEmpty;
                    }
                    //speculative
                    if (cmd.tiletype != (int)TileTypeManicDigger.Minecart)
                    {
                        speculative[new Vector3i(x, y, z)] = new Speculative() { blocktype = activematerial, time = DateTime.Now };
                        terrain.UpdateTile(x, y, z);
                    }
                }
            }
        }
        //there is no local player on server
        int? localplayerid
        {
            get
            {
                foreach (var k in players)
                {
                    if (k.Value.Name == viewport.LocalPlayerName)
                    {
                        return k.Key;
                    }
                }
                return null;
            }
        }
        bool TrySendCommand(byte[] cmd)
        {
            if (DoCommand(cmd, localplayerid.Value, false))
            {
                network.SendCommand(cmd);
                return true;
            }
            return false;
        }
        struct Speculative
        {
            public DateTime time;
            public int blocktype;
        }
        Dictionary<Vector3i, Speculative> speculative = new Dictionary<Vector3i, Speculative>();
        byte[] MakeCommand(CommandId cmdid, IStreamizable cmd)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)cmdid);
            cmd.ToStream(ms);
            return ms.ToArray();
        }
        public void SendSetBlock(OpenTK.Vector3 vector3, BlockSetMode blockSetMode, int type)
        {
        }
        public struct BuildOrder
        {
            public int playerid;
            public Vector3 position;
            public BlockSetMode mode;
            public int tiletype;
        }
        class Character : ICharacterToDraw
        {
            public Vector3 pos3d;
            //public List<Vector3> orders = new List<Vector3>();
            public int currentOrderId = 0;
            public int cargoAmount = 0;
            public Vector3 dir3d;
            public bool moves;
            public Vector3 Pos3d { get { return pos3d; } }
            public Vector3 Dir3d { get { return dir3d; } }
            public bool Moves { get { return moves; } }
            public Vector3? currentorder;
            //public CharacterState state;
            public float buildprogress;
            public List<Vector3> path;
        }
        List<Character> characters = new List<Character>();
        [Inject]
        public CharacterPhysics physics { get; set; }
        float basecharactersmovespeed { get { return ManicDiggerGameWindow.basemovespeed / 3; } }
        [Inject]
        public Pathfinder3d pathfinder{get;set;}
        private void RemoveOrder(Vector3 vv)
        {
            var o = orders[vv];
            terrain.UpdateTile((int)vv.X, (int)vv.Y, (int)vv.Z);
            orders.Remove(vv);
        }
        Vector3 ToMap(Vector3 v)
        {
            return new Vector3(v.X,v.Z,v.Y);
        }
        Vector3 To3d(Vector3 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }
        private Vector3? ClosestBuildOrder(Vector3 v)
        {
            List<BuildOrder> o = new List<BuildOrder>(orders.Values);
            o.Sort((a, b) => (To3d(a.position) - v).Length.CompareTo((To3d(b.position) - v).Length));
            if (orders.Count == 0)
            {
                return null;
            }
            return o[0].position;
        }
        //IGameWorld
        //List<BuildOrder> orders = new List<BuildOrder>();
        Dictionary<Vector3, BuildOrder> orders = new Dictionary<Vector3, BuildOrder>();
        public byte[] SaveState()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            b.AppendLine("<ManicDiggerSave>");
            b.AppendLine(XmlTool.X("FormatVersion", "200"));
            b.AppendLine("<MapSize>");
            b.AppendLine(XmlTool.X("X", "" + map.MapSizeX));
            b.AppendLine(XmlTool.X("Y", "" + map.MapSizeY));
            b.AppendLine(XmlTool.X("Z", "" + map.MapSizeZ));
            b.AppendLine("</MapSize>");
            b.AppendLine("<RailVehicles>");
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i] == null)
                {
                    continue;
                }
                b.AppendLine("<RailVehicle>");
                b.AppendLine(XmlTool.X("Id", ((int)i).ToString()));
                b.AppendLine("<Position>");
                b.AppendLine(XmlTool.X("X", ((int)vehicles[i].currentrailblock.X).ToString()));
                b.AppendLine(XmlTool.X("Y", ((int)vehicles[i].currentrailblock.Y).ToString()));
                b.AppendLine(XmlTool.X("Z", ((int)vehicles[i].currentrailblock.Z).ToString()));
                b.AppendLine("</Position>");
                b.AppendLine(XmlTool.X("Direction", ((int)vehicles[i].currentdirection).ToString()));
                b.AppendLine(XmlTool.X("LastDirection", ((int)vehicles[i].lastdirection).ToString()));
                b.AppendLine(XmlTool.X("Progress", ((int)vehicles[i].currentrailblockprogressMul1000).ToString()));
                b.AppendLine(XmlTool.X("Speed", ((int)vehicles[i].currentvehiclespeedMul1000).ToString()));
                b.AppendLine(XmlTool.X("TurnLeft", vehicles[i].turnleft.ToString()));
                b.AppendLine(XmlTool.X("TurnRight", vehicles[i].turnright.ToString()));
                b.AppendLine(XmlTool.X("Accelerate", vehicles[i].accelerate.ToString()));
                b.AppendLine(XmlTool.X("Decelerate", vehicles[i].decelerate.ToString()));
                b.AppendLine("</RailVehicle>");
            }
            b.AppendLine("</RailVehicles>");
            b.AppendLine("<FiniteInventory>");
            foreach (var k in PlayersFiniteInventory)
            {
                b.AppendLine("<PlayerFiniteInventory>");
                b.AppendLine(XmlTool.X("Name", SecurityElement.Escape(k.Key)));
                b.AppendLine("<Blocks>");
                foreach (var kk in k.Value)
                {
                    b.AppendLine("<Block>");
                    b.AppendLine(XmlTool.X("Type", kk.Key.ToString()));
                    b.AppendLine(XmlTool.X("Amount", kk.Value.ToString()));
                    b.AppendLine("</Block>");
                }
                b.AppendLine("</Blocks>");
                b.AppendLine("</PlayerFiniteInventory>");
            }
            b.AppendLine("</FiniteInventory>");
            b.AppendLine(XmlTool.X("InfiniteWorldGenerator", SecurityElement.Escape(generator)));
            byte[] mapdata = map.SaveBlocks();
            b.AppendLine(XmlTool.X("InfiniteMapData", Convert.ToBase64String(mapdata)));
            b.AppendLine("</ManicDiggerSave>");
            return GzipCompression.Compress(Encoding.UTF8.GetBytes(b.ToString()));
        }
        public string generator;
        public void LoadState(byte[] savegame, int simulationstartframe)
        {
            LoadState(savegame);
            simulationcurrentframe = simulationstartframe;
            simulationaccumulator = 0;
            simulationallowedkeyframe = 0;
            simulationcmdtodo = new Queue<CommandTodo>();
            simulationhashchecktodo = new Queue<HashCheckTodo>();
        }
        public void LoadState(byte[] savegame)
        {
            using (Stream s = new MemoryStream(GzipCompression.Decompress(savegame)))
            {
                StreamReader sr = new StreamReader(s);
                XmlDocument d = new XmlDocument();
                d.Load(sr);
                int format = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerSave/FormatVersion"));
                if (format != 200)
                {
                    throw new Exception("Invalid map format");
                }
                map.MapSizeX = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerSave/MapSize/X"));
                map.MapSizeY = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerSave/MapSize/Y"));
                map.MapSizeZ = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerSave/MapSize/Z"));
                foreach (XPathNavigator k in d.CreateNavigator()
                    .Select("/ManicDiggerSave/RailVehicles/RailVehicle"))
                {
                    int id = int.Parse(k.SelectSingleNode("Id").Value);
                    int x = int.Parse(k.SelectSingleNode("Position").SelectSingleNode("X").Value);
                    int y = int.Parse(k.SelectSingleNode("Position").SelectSingleNode("Y").Value);
                    int z = int.Parse(k.SelectSingleNode("Position").SelectSingleNode("Z").Value);
                    int direction = int.Parse(k.SelectSingleNode("Direction").Value);
                    int lastdirection = int.Parse(k.SelectSingleNode("LastDirection").Value);
                    int progressMul1000 = int.Parse(k.SelectSingleNode("Progress").Value);
                    int speedMul1000 = int.Parse(k.SelectSingleNode("Speed").Value);
                    bool turnleft = bool.Parse(k.SelectSingleNode("TurnLeft").Value);
                    bool turnright = bool.Parse(k.SelectSingleNode("TurnRight").Value);
                    bool accelerate = bool.Parse(k.SelectSingleNode("Accelerate").Value);
                    bool decelerate = bool.Parse(k.SelectSingleNode("Decelerate").Value);
                    while (vehicles.Count <= id)
                    {
                        vehicles.Add(null);
                    }
                    RailVehicle v = new RailVehicle();
                    v.currentrailblock = new Vector3(x, y, z);
                    v.currentdirection = (VehicleDirection12)direction;
                    v.lastdirection = (VehicleDirection12)lastdirection;
                    v.currentrailblockprogressMul1000 = progressMul1000;
                    v.currentvehiclespeedMul1000 = speedMul1000;
                    v.turnleft = turnleft;
                    v.turnright = turnright;
                    v.accelerate = accelerate;
                    v.decelerate = decelerate;
                    vehicles[id] = v;
                }
                foreach (XPathNavigator k in d.CreateNavigator()
                    .Select("/ManicDiggerSave/FiniteInventory/PlayerFiniteInventory"))
                {
                    string name = k.SelectSingleNode("Name").Value;
                    Dictionary<int, int> blocks = new Dictionary<int, int>();
                    foreach (XPathNavigator kk in k.SelectSingleNode("Blocks").Select("Block"))
                    {
                        int type = int.Parse(kk.SelectSingleNode("Type").Value);
                        int amount = int.Parse(kk.SelectSingleNode("Amount").Value);
                        blocks[type] = amount;
                    }
                    PlayersFiniteInventory[name] = blocks;
                }
                var ss = XmlTool.XmlVal(d, "/ManicDiggerSave/InfiniteWorldGenerator");
                if (ss != null && ss != "")
                {
                    generator = ss;
                }
                else
                {
                    //plain map?
                }
                worldgeneratorsandbox.Compile(generator);
                byte[] mapdata = Convert.FromBase64String(XmlTool.XmlVal(d, "/ManicDiggerSave/InfiniteMapData"));
                map.Restart();
                map.LoadBlocks(mapdata);
            }
        }
        public string GameInfo
        {
            get { return ""; }
        }
        public float SIMULATION_STEP_LENGTH = 1f / 64f;
        public void Tick()
        {
            simulationcurrentframe++;
            //float dt = 1.0f / 75;
            float dt = SIMULATION_STEP_LENGTH;
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i] == null)
                {
                    continue;
                }
                UpdateRailVehicle(dt, i);
            }
            if (simulationcurrentframe % (int)((10 * 60) / SIMULATION_STEP_LENGTH) == 0)
            {
                Dictionary<Vector3i, int> blockstoset = new Dictionary<Vector3i, int>();
                foreach (var k in map.blocks)
                {
                    if (k.Value == (int)TileTypeManicDigger.DirtForFarming)
                    {
                        var pos = MapUtil.FromMapPos(k.Key);
                        if (MapUtil.IsValidPos(map, pos.x, pos.y, pos.z + 1))
                        {
                            int blockabove = map.GetBlock(pos.x, pos.y, pos.z + 1);
                            if (blockabove == (int)TileTypeManicDigger.Crops1) { blockabove = (int)TileTypeManicDigger.Crops2; }
                            else if (blockabove == (int)TileTypeManicDigger.Crops2) { blockabove = (int)TileTypeManicDigger.Crops3; }
                            else if (blockabove == (int)TileTypeManicDigger.Crops3) { blockabove = (int)TileTypeManicDigger.Crops4; }
                            else { continue; }
                            blockstoset[new Vector3i(pos.x, pos.y, pos.z + 1)] = blockabove;
                        }
                    }
                }
                foreach (var k in blockstoset)
                {
                    map.SetBlock(k.Key.x, k.Key.y, k.Key.z, k.Value);
                    terrain.UpdateTile(k.Key.x, k.Key.y, k.Key.z);
                }
            }
        }
        Dictionary<string, Dictionary<int, int>> PlayersFiniteInventory = new Dictionary<string, Dictionary<int, int>>();
        public bool ENABLE_FINITEINVENTORY = true;
        Dictionary<int, int> GetPlayerInventory(string playername)
        {
            if (!PlayersFiniteInventory.ContainsKey(playername))
            {
                PlayersFiniteInventory[playername] = StartFiniteInventory();
            }
            return PlayersFiniteInventory[playername];
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
        int finiteinventorymax = 100;
        public int FiniteInventoryMax { get { return finiteinventorymax; } }
        public void DoCommand(byte[] command, int player_id)
        {
            DoCommand(command, player_id, true);
        }
        public bool DoCommand(byte[] command, int player_id, bool execute)
        {
            MemoryStream ms = new MemoryStream(command);
            BinaryReader br = new BinaryReader(ms);
            CommandId commandid = (CommandId)br.ReadByte();
            switch (commandid)
            {
                case CommandId.Build:
                    {
                        var cmd = new CommandBuild();
                        cmd.FromStream(ms);
                        Vector3 v = new Vector3(cmd.x, cmd.y, cmd.z);
                        bool placeminecartblock = false;
                        if (ENABLE_FINITEINVENTORY)
                        {
                            Dictionary<int, int> inventory = GetPlayerInventory(players[player_id].Name);
                            if (cmd.mode == BlockSetMode.Create)
                            {
                                int oldblock = map.GetBlock(cmd.x, cmd.y, cmd.z);
                                if (cmd.tiletype == (int)TileTypeManicDigger.Minecart)
                                {
                                    if (!GameDataTilesManicDigger.IsRailTile(oldblock))
                                    {
                                        return false;
                                    }
                                    if (!(inventory.ContainsKey((int)TileTypeManicDigger.Minecart)
                                        && inventory[(int)TileTypeManicDigger.Minecart] > 0))
                                    {
                                        return false;
                                    }
                                    RailVehicle veh = new RailVehicle();
                                    veh.currentrailblock = new Vector3(cmd.x, cmd.y, cmd.z);
                                    {
                                        var railunderplayer = data.GetRail(oldblock);
                                        veh.currentvehiclespeedMul1000 = 0;
                                        if ((railunderplayer & RailDirectionFlags.Horizontal) != 0)
                                        {
                                            veh.currentdirection = VehicleDirection12.HorizontalRight;
                                        }
                                        else if ((railunderplayer & RailDirectionFlags.Vertical) != 0)
                                        {
                                            veh.currentdirection = VehicleDirection12.VerticalUp;
                                        }
                                        else if ((railunderplayer & RailDirectionFlags.UpLeft) != 0)
                                        {
                                            veh.currentdirection = VehicleDirection12.UpLeftUp;
                                        }
                                        else if ((railunderplayer & RailDirectionFlags.UpRight) != 0)
                                        {
                                            veh.currentdirection = VehicleDirection12.UpRightUp;
                                        }
                                        else if ((railunderplayer & RailDirectionFlags.DownLeft) != 0)
                                        {
                                            veh.currentdirection = VehicleDirection12.DownLeftLeft;
                                        }
                                        else if ((railunderplayer & RailDirectionFlags.DownRight) != 0)
                                        {
                                            veh.currentdirection = VehicleDirection12.DownRightRight;
                                        }
                                        else
                                        {
                                            //ExitVehicle();
                                            return false;
                                        }
                                        veh.lastdirection = veh.currentdirection;
                                    }
                                    if (execute)
                                    {
                                        int vid = newrailvehicleid();
                                        vehicles[vid] = veh;
                                        inventory[(int)TileTypeManicDigger.Minecart]--;
                                    }
                                    return true;
                                }
                                int blockstoput = 1;
                                if (GameDataTilesManicDigger.IsRailTile(cmd.tiletype))
                                {
                                    if (!(oldblock == data.TileIdEmpty
                                        || GameDataTilesManicDigger.IsRailTile(oldblock)))
                                    {
                                        return false;
                                    }
                                    //count how many rails will be created
                                    int oldrailcount = 0;
                                    if (GameDataTilesManicDigger.IsRailTile(oldblock))
                                    {
                                        oldrailcount = MyLinq.Count(
                                            DirectionUtils.ToRailDirections(
                                            (RailDirectionFlags)(oldblock - GameDataTilesManicDigger.railstart)));
                                    }
                                    int newrailcount = MyLinq.Count(
                                        DirectionUtils.ToRailDirections(
                                        (RailDirectionFlags)(cmd.tiletype - GameDataTilesManicDigger.railstart)));
                                    blockstoput = newrailcount - oldrailcount;
                                    //check if player has that many rails
                                    int inventoryrail = GetEquivalentCount(inventory, cmd.tiletype);
                                    if (blockstoput > inventoryrail)
                                    {
                                        return false;
                                    }
                                    if (execute)
                                    {
                                        RemoveEquivalent(inventory, cmd.tiletype, blockstoput);
                                    }
                                }
                                else
                                {
                                    if (oldblock != data.TileIdEmpty)
                                    {
                                        return false;
                                    }
                                    //check if player has such block
                                    int hasblock = -1; //which equivalent block it has exactly?
                                    foreach (var k in inventory)
                                    {
                                        if (EquivalentBlock(k.Key, cmd.tiletype)
                                            && k.Value > 0)
                                        {
                                            hasblock = k.Key;
                                        }
                                    }
                                    if (hasblock == -1)
                                    {
                                        return false;
                                    }
                                    if (execute)
                                    {
                                        inventory[hasblock]--;
                                    }
                                }
                            }
                            else
                            {
                                //add to inventory
                                int blocktype = map.GetBlock(cmd.x, cmd.y, cmd.z);
                                blocktype = data.PlayerBuildableMaterialType(blocktype);
                                if ((!data.IsValidTileType(blocktype))
                                    || blocktype == data.TileIdEmpty)
                                {
                                    return false;
                                }
                                int blockstopick = 1;
                                if (GameDataTilesManicDigger.IsRailTile(blocktype))
                                {
                                    blockstopick = MyLinq.Count(
                                        DirectionUtils.ToRailDirections(
                                        (RailDirectionFlags)(blocktype - GameDataTilesManicDigger.railstart)));
                                }
                                if (TotalAmount(inventory) + blockstopick > FiniteInventoryMax)
                                {
                                    return false;
                                }
                                if (execute)
                                {
                                    //when removing rail under minecart, make minecart block in place of removed rail.
                                    if (GameDataTilesManicDigger.IsRailTile(blocktype))
                                    {
                                        for (int i = 0; i < vehicles.Count; i++)
                                        {
                                            if (vehicles[i].currentrailblock == new Vector3(cmd.x, cmd.y, cmd.z))
                                            {
                                                vehicles[i] = null;
                                                placeminecartblock = true;
                                                //stop railriding
                                                foreach (var k in players)
                                                {
                                                    if (railridingall.ContainsKey(k.Value.Name)
                                                        && railridingall[k.Value.Name] == i)
                                                    {
                                                        RailRidingLeave(k.Key);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (!inventory.ContainsKey(blocktype))
                                    {
                                        inventory[blocktype] = 0;
                                    }
                                    inventory[blocktype] += blockstopick;
                                }
                            }
                        }
                        else
                        {
                        }
                        if (execute)
                        {
                            int tiletype = cmd.mode == BlockSetMode.Create ?
                                (byte)cmd.tiletype : data.TileIdEmpty;
                            if (placeminecartblock)
                            {
                                tiletype = (int)TileTypeManicDigger.Minecart;
                            }
                            map.SetBlock(cmd.x, cmd.y, cmd.z, tiletype);
                            terrain.UpdateTile(cmd.x, cmd.y, cmd.z);
                        }
                        return true;
                    }
                    break;
                case CommandId.RailVehicleEnterLeave:
                    {
                        var cmd = new CommandRailVehicleEnterLeave();
                        cmd.FromStream(ms);
                        //check if vehicle id is valid
                        if (execute)
                        {
                            if (cmd.enter)
                            {
                                railridingall[players[player_id].Name] = cmd.vehicleid;
                                if (localplayerid != null && player_id == localplayerid)
                                {
                                    viewport.CharacterHeight = minecartheight;
                                }
                            }
                            else
                            {
                                RailVehicle veh = vehicles[railridingall[players[player_id].Name]];
                                veh.currentvehiclespeedMul1000 = 0;
                                RailRidingLeave(player_id);
                            }
                        }
                        return true;
                    }
                    break;
                case CommandId.Craft:
                    {
                        var cmd = new CommandCraft();
                        cmd.FromStream(ms);
                        if (map.GetBlock(cmd.x, cmd.y, cmd.z) != (int)TileTypeManicDigger.CraftingTable)
                        {
                            return false;
                        }
                        if (cmd.recipeid < 0 || cmd.recipeid >= craftingrecipes.Count)
                        {
                            return false;
                        }
                        List<Vector3i> table = GetTable(new Vector3i(cmd.x, cmd.y, cmd.z));
                        List<int> ontable = GetOnTable(table);
                        List<int> outputtoadd = new List<int>();
                        //for (int i = 0; i < craftingrecipes.Count; i++)
                        int i = cmd.recipeid;
                        {
                            //try apply recipe. if success then try until fail.
                            for (; ; )
                            {
                                //check if ingredients available
                                foreach (Ingredient ingredient in craftingrecipes[i].ingredients)
                                {
                                    if (ontable.FindAll(v => v == ingredient.Type).Count < ingredient.Amount)
                                    {
                                        goto nextrecipe;
                                    }
                                }
                                //remove ingredients
                                foreach (Ingredient ingredient in craftingrecipes[i].ingredients)
                                {
                                    for (int ii = 0; ii < ingredient.Amount; ii++)
                                    {
                                        //replace on table
                                        ReplaceOne(ontable, ingredient.Type, (int)TileTypeMinecraft.Empty);
                                    }
                                }
                                //add output
                                for (int z = 0; z < craftingrecipes[i].output.Amount; z++)
                                {
                                    outputtoadd.Add(craftingrecipes[i].output.Type);
                                }
                            }
                        nextrecipe:
                            ;
                        }
                        foreach (var v in outputtoadd)
                        {
                            ReplaceOne(ontable, (int)TileTypeMinecraft.Empty, v);
                        }
                        int zz = 0;
                        if (execute)
                        {
                            foreach (var v in table)
                            {
                                map.SetBlock(v.x, v.y, v.z + 1, ontable[zz]);
                                terrain.UpdateTile(v.x, v.y, v.z + 1);
                                zz++;
                            }
                        }
                        return true;
                    }
                    break;
                case CommandId.DumpOrLoad:
                    {
                        var cmd = new CommandDumpOrLoad();
                        cmd.FromStream(ms);
                        Dictionary<int, int> inventory = GetPlayerInventory(players[player_id].Name);
                        int dumpcount = 0;
                        if (inventory.ContainsKey(cmd.blocktype))
                        {
                            dumpcount = inventory[cmd.blocktype];
                        }
                        Vector3i pos = new Vector3i(cmd.x, cmd.y, cmd.z);
                        if(execute)
                        {
                            if (map.GetBlock(pos.x, pos.y, pos.z) == (int)TileTypeManicDigger.CraftingTable)
                            {
                                List<Vector3i> table = GetTable(pos);
                                if (cmd.dump)
                                {
                                    int dumped = 0;
                                    foreach (Vector3i v in table)
                                    {
                                        if (dumped >= table.Count / 2 || dumped >= dumpcount)
                                        {
                                            break;
                                        }
                                        if (map.GetBlock(v.x, v.y, v.z + 1) == data.TileIdEmpty)
                                        {
                                            map.SetBlock(v.x, v.y, v.z + 1, cmd.blocktype);
                                            inventory[cmd.blocktype]--;
                                            terrain.UpdateTile(v.x, v.y, v.z + 1);
                                            dumped++;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Vector3i v in table)
                                    {
                                        if (TotalAmount(inventory) + 1 > finiteinventorymax)
                                        {
                                            break;
                                        }
                                        int b = map.GetBlock(v.x, v.y, v.z + 1);
                                        if (b != data.TileIdEmpty)
                                        {
                                            map.SetBlock(v.x, v.y, v.z + 1, data.TileIdEmpty);
                                            inventory[b]++;
                                            terrain.UpdateTile(v.x, v.y, v.z + 1);
                                        }
                                    }
                                }
                                return true;
                            }
                            if (cmd.dump)
                            {
                                for (int i = 0; i < dumpcount; i++)
                                {
                                    //find empty position that is nearest to dump place AND has a block under.
                                    Vector3i? nearpos = FindDumpPlace(pos);
                                    if (nearpos == null)
                                    {
                                        break;
                                    }
                                    map.SetBlock(nearpos.Value.x, nearpos.Value.y, nearpos.Value.z, cmd.blocktype);
                                    inventory[cmd.blocktype]--;
                                    terrain.UpdateTile(nearpos.Value.x, nearpos.Value.y, nearpos.Value.z);
                                }
                            }
                        }
                        return true;
                    }
                    break;
                case CommandId.RailVehicleControl:
                    {
                        CommandRailVehicleControl cmd = new CommandRailVehicleControl();
                        cmd.FromStream(ms);
                        int vehicleid = railridingall[players[player_id].Name];
                        if (vehicleid == -1)
                        {
                            return false;
                        }
                        var veh = vehicles[vehicleid];
                        if (execute)
                        {
                            switch (cmd.controlaction)
                            {
                                case ControlAction.AccelerateMinus:
                                    veh.accelerate = false;
                                    veh.decelerate = true;
                                    break;
                                case ControlAction.AccelerateNone:
                                    veh.accelerate = false;
                                    veh.decelerate = false;
                                    break;
                                case ControlAction.AcceleratePlus:
                                    veh.accelerate = true;
                                    veh.decelerate = false;
                                    break;
                                case ControlAction.Reverse:
                                    veh.currentdirection = DirectionUtils.Reverse(veh.currentdirection);
                                    veh.currentrailblockprogressMul1000 = 1000 - veh.currentrailblockprogressMul1000;
                                    veh.lastdirection = veh.currentdirection;
                                    //currentvehiclespeed = 0;
                                    break;
                                case ControlAction.TurnLeft:
                                    veh.turnleft = true;
                                    break;
                                case ControlAction.TurnRight:
                                    veh.turnright = true;
                                    break;
                                case ControlAction.TurnNone:
                                    veh.turnleft = false;
                                    veh.turnright = false;
                                    break;
                            }
                        }
                        return true;
                    }
                    break;
                default:
                    throw new Exception();
            }
        }
        private void RailRidingLeave(int player_id)
        {
            railridingall[players[player_id].Name] = -1;
            if (localplayerid != null && player_id == localplayerid)
            {
                viewport.CharacterHeight = WalkCharacterHeight;
                viewport.ENABLE_FREEMOVE = false;
                viewport.ENABLE_MOVE = true;
                viewport.LocalPlayerPosition += new Vector3(0, 0.7f, 0);
            }
        }
        private void RemoveEquivalent(Dictionary<int, int> inventory, byte blocktype, int count)
        {
            int removed = 0;
            for (int i = 0; i < count; i++)
            {
                foreach (var k in new Dictionary<int, int>(inventory))
                {
                    if (EquivalentBlock(k.Key, blocktype)
                        && k.Value > 0)
                    {
                        inventory[k.Key]--;
                        removed++;
                        goto removenext;
                    }
                }
            removenext:
                ;
            }
            if (removed != count)
            {
                throw new Exception();
            }
        }
        private int GetEquivalentCount(Dictionary<int, int> inventory, byte blocktype)
        {
            int count = 0;
            foreach (var k in inventory)
            {
                if (EquivalentBlock(k.Key, blocktype))
                {
                    count += k.Value;
                }
            }
            return count;
        }
        bool EquivalentBlock(int blocktypea, int blocktypeb)
        {
            if (GameDataTilesManicDigger.IsRailTile(blocktypea) && GameDataTilesManicDigger.IsRailTile(blocktypeb))
            {
                return true;
            }
            return blocktypea == blocktypeb;
        }
        private List<int> GetOnTable(List<Vector3i> table)
        {
            List<int> ontable = new List<int>();
            foreach (var v in table)
            {
                int t = map.GetBlock(v.x, v.y, v.z + 1);
                ontable.Add(t);
            }
            return ontable;
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
                        if (map.GetBlock(xx, yy, zz) == data.TileIdEmpty && map.GetBlock(xx, yy, zz - 1) != data.TileIdEmpty)
                        {
                            l.Add(new Vector3i(xx, yy, zz));
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
        private void ReplaceOne<T>(List<T> l, T from, T to)
        {
            for (int ii = 0; ii < l.Count; ii++)
            {
                if (l[ii].Equals(from))
                {
                    l[ii] = to;
                    break;
                }
            }
        }
        List<CraftingRecipe> craftingrecipes = new List<CraftingRecipe>();
        void MakeRecipes()
        {
            craftingrecipes = new List<CraftingRecipe>();
            MakeRecipe(TileTypeMinecraft.Stone, 2, TileTypeMinecraft.Cobblestone, 1);
            MakeRecipe(TileTypeMinecraft.Cobblestone, 2, TileTypeMinecraft.Stone, 1);
            MakeRecipe(TileTypeMinecraft.TreeTrunk, 1, TileTypeMinecraft.Wood, 2);
            MakeRecipe(TileTypeMinecraft.Stone, 4, TileTypeMinecraft.Brick, 1);
            MakeRecipe(TileTypeMinecraft.GoldOre, 1, TileTypeMinecraft.CoalOre, 1, TileTypeMinecraft.GoldBlock, 1);
            MakeRecipe(TileTypeMinecraft.IronOre, 1, TileTypeMinecraft.CoalOre, 1, TileTypeMinecraft.IronBlock, 1);
            MakeRecipe(TileTypeMinecraft.Wood, 1, TileTypeMinecraft.IronBlock, 1, GameDataTilesManicDigger.railstart + (int)RailDirectionFlags.TwoHorizontalVertical, 2);
            MakeRecipe(TileTypeMinecraft.Wood, 1, TileTypeMinecraft.IronBlock, 1, GameDataTilesManicDigger.railstart + (int)RailDirectionFlags.Corners, 2);
            MakeRecipe(TileTypeMinecraft.Wood, 3, TileTypeManicDigger.CraftingTable, 1);
            MakeRecipe(TileTypeMinecraft.Stone, 2, TileTypeMinecraft.Stair, 1);
            MakeRecipe(TileTypeMinecraft.Stone, 2, TileTypeMinecraft.DoubleStair, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.TNT, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.Adminium, 1);
            MakeRecipe(TileTypeMinecraft.Stone, 1, TileTypeMinecraft.Dirt, 1, TileTypeMinecraft.Grass, 1);
            MakeRecipe(TileTypeMinecraft.Sand, 2, TileTypeMinecraft.Glass, 1);
            MakeRecipe(TileTypeMinecraft.Leaves, 10, TileTypeMinecraft.RedRoseDecorations, 1);
            MakeRecipe(TileTypeMinecraft.Leaves, 10, TileTypeMinecraft.YellowFlowerDecorations, 1);
            MakeRecipe(TileTypeMinecraft.Leaves, 10, TileTypeMinecraft.Sapling, 1);
            MakeRecipe(TileTypeMinecraft.Dirt, 10, TileTypeMinecraft.RedMushroom, 1);
            MakeRecipe(TileTypeMinecraft.Dirt, 10, TileTypeMinecraft.BrownMushroom, 1);
            MakeRecipe(TileTypeMinecraft.Wood, 2, TileTypeMinecraft.Bookcase, 1);
            MakeRecipe(TileTypeMinecraft.Cobblestone, 1, TileTypeMinecraft.MossyCobblestone, 1);
            MakeRecipe(TileTypeMinecraft.MossyCobblestone, 1, TileTypeMinecraft.Cobblestone, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.Sponge, 1);

            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.RedCloth, 1);
            MakeRecipe(TileTypeMinecraft.RedCloth, 1, TileTypeMinecraft.OrangeCloth, 1);
            MakeRecipe(TileTypeMinecraft.OrangeCloth, 1, TileTypeMinecraft.YellowCloth, 1);
            MakeRecipe(TileTypeMinecraft.YellowCloth, 1, TileTypeMinecraft.LightGreenCloth, 1);
            MakeRecipe(TileTypeMinecraft.LightGreenCloth, 1, TileTypeMinecraft.GreenCloth, 1);
            MakeRecipe(TileTypeMinecraft.GreenCloth, 1, TileTypeMinecraft.AquaGreenCloth, 1);
            MakeRecipe(TileTypeMinecraft.AquaGreenCloth, 1, TileTypeMinecraft.CyanCloth, 1);
            MakeRecipe(TileTypeMinecraft.CyanCloth, 1, TileTypeMinecraft.BlueCloth, 1);
            MakeRecipe(TileTypeMinecraft.BlueCloth, 1, TileTypeMinecraft.PurpleCloth, 1);
            MakeRecipe(TileTypeMinecraft.PurpleCloth, 1, TileTypeMinecraft.IndigoCloth, 1);
            MakeRecipe(TileTypeMinecraft.IndigoCloth, 1, TileTypeMinecraft.VioletCloth, 1);
            MakeRecipe(TileTypeMinecraft.VioletCloth, 1, TileTypeMinecraft.MagentaCloth, 1);
            MakeRecipe(TileTypeMinecraft.MagentaCloth, 1, TileTypeMinecraft.PinkCloth, 1);
            MakeRecipe(TileTypeMinecraft.PinkCloth, 1, TileTypeMinecraft.BlackCloth, 1);
            MakeRecipe(TileTypeMinecraft.BlackCloth, 1, TileTypeMinecraft.GrayCloth, 1);
            MakeRecipe(TileTypeMinecraft.GrayCloth, 1, TileTypeMinecraft.WhiteCloth, 1);
            MakeRecipe(TileTypeMinecraft.WhiteCloth, 1, TileTypeMinecraft.RedCloth, 1);

            MakeRecipe(TileTypeMinecraft.Brick, 2, TileTypeManicDigger.Roof, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeManicDigger.ChemicalGreen, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeManicDigger.Camouflage, 1);
            MakeRecipe(TileTypeMinecraft.Dirt, 2, TileTypeManicDigger.DirtForFarming, 1);
            MakeRecipe(TileTypeManicDigger.Crops4, 1, TileTypeManicDigger.Crops1, 2);
            MakeRecipe(TileTypeMinecraft.IronBlock, 1, TileTypeMinecraft.CoalOre, 1, TileTypeManicDigger.BrushedMetal, 1);
            MakeRecipe(TileTypeManicDigger.BrushedMetal, 5, TileTypeManicDigger.Minecart, 1);
        }
        void MakeRecipe(params object[] r)
        {
            var recipe = new CraftingRecipe();
            for (int i = 0; i < r.Length - 2; i += 2)
            {
                recipe.ingredients.Add(new Ingredient() { Type = Convert.ToInt32(r[i]), Amount = Convert.ToInt32(r[i + 1]) });
            }
            recipe.output = new Ingredient() { Type = Convert.ToInt32(r[r.Length - 2]), Amount = Convert.ToInt32(r[r.Length - 1]) };
            craftingrecipes.Add(recipe);
        }
        //Dictionary<int, RailVehicle> vehicles = new Dictionary<int, RailVehicle>();
        List<RailVehicle> vehicles = new List<RailVehicle>();
        int newrailvehicleid()
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i] == null)
                {
                    return i;
                }
            }
            vehicles.Add(null);
            return vehicles.Count - 1;
        }
        public int GetStateHash()
        {
            return 0;
        }
        public IEnumerable<ICharacterToDraw> Characters
        {
            get
            {
                foreach (Character c in characters)
                {
                    yield return c;
                }
            }
        }
        Vector3 playerpositionspawn = new Vector3(15.5f, 64, 15.5f);
        public Vector3 PlayerPositionSpawn { get { return playerpositionspawn; } set { playerpositionspawn = value; } }

        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int, Player> Players { get { return players; } set { players = value; } }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, int tileType)
        {
            //map.Map[x, y, z] = (byte)tileType;
            map.SetBlock(x, y, z, tileType);
        }
        #endregion
        //float waterlevel = 32;
        #region IMapStorage Members
        //public float WaterLevel { get { return waterlevel; } set { waterlevel = value; } }
        public float WaterLevel { get { return MapSizeZ / 2; } set { } }
        #endregion
        #region IMapStorage Members
        public int MapSizeX { get { return map.MapSizeX; } set { map.MapSizeX = value; } }
        public int MapSizeY { get { return map.MapSizeY; } set { map.MapSizeY = value; } }
        public int MapSizeZ { get { return map.MapSizeZ; } set { map.MapSizeZ = value; } }
        #endregion
        #region IMapStorage Members
        public void Dispose()
        {
        }
        #endregion
        #region ITerrainInfo Members
        public int GetTerrainBlock(int x, int y, int z)
        {
            if (speculative.ContainsKey(new Vector3i(x, y, z)))
            {
                return speculative[new Vector3i(x, y, z)].blocktype;
            }
            return map.GetBlock(x, y, z);
        }
        public System.Drawing.Color GetTerrainBlockColor(int x, int y, int z)
        {
            return Color.White;
        }
        #endregion
        #region IGameMode Members
        public void OnNewMap()
        {
            int x = map.MapSizeX / 2;
            int y = map.MapSizeY / 2;
            playerpositionspawn = new Vector3(x + 0.5f, MapUtil.blockheight(map, data.TileIdEmpty, x, y), y + 0.5f);
            viewport.LocalPlayerPosition = PlayerPositionSpawn;
            viewport.LocalPlayerOrientation = PlayerOrientationSpawn;
        }
        #endregion
        #region IGameMode Members
        public Vector3 PlayerOrientationSpawn
        {
            get { return new Vector3((float)Math.PI, 0, 0); }
        }
        #endregion
        #region IMapStorage Members
        public int GetBlock(int x, int y, int z)
        {
            return GetTerrainBlock(x, y, z);
        }
        #endregion
        public int GetBlockForPhysics(int x,int y,int z)
        {
            if (speculative.ContainsKey(new Vector3i(x, y, z)))
            {
                return speculative[new Vector3i(x, y, z)].blocktype;
            }
            return map.GetBlock(x,y,z);
        }
        #region IMapStorage Members
        public void UseMap(byte[, ,] map)
        {
            this.map.UseMap(map);
        }
        #endregion
        [Inject]
        public MinecartDrawer minecartdrawer { get; set; }
        #region IGameMode Members
        public IEnumerable<IModelToDraw> Models
        {
            get
            {
                /*
                if (railriding)
                {
                    var m = new MinecartToDraw();
                    m.drawer = minecartdrawer;
                    m.position = viewport.LocalPlayerPosition;
                    m.direction = localrailvehicle.currentdirection;
                    m.lastdirection = localrailvehicle.lastdirection;
                    m.progress = localrailvehicle.currentrailblockprogress;
                    yield return m;
                }
                */
                for (int i = 0; i < vehicles.Count; i++)
                {
                    if (vehicles[i] == null)
                    {
                        continue;
                    }
                    var v = vehicles[i];
                    var m = new MinecartToDraw();
                    m.drawer = minecartdrawer;
                    m.currentrailblock = v.currentrailblock;
                    m.direction = v.currentdirection;
                    m.lastdirection = v.lastdirection;
                    m.progress = v.currentrailblockprogressMul1000 / 1000f;
                    yield return m;
                }
            }
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryAmount(int blocktype)
        {
            var FiniteInventory = viewport.FiniteInventory;
            int amount = 0;
            foreach (var k in FiniteInventory)
            {
                if (EquivalentBlock(k.Key, blocktype))
                {
                    amount += k.Value;
                }
            }
            return amount;
        }
        #endregion
        int simulationallowedkeyframe = 0;
        #region IGameWorldTodo Members
        public void KeyFrame(int allowedframe, int hash)
        {
            simulationallowedkeyframe = allowedframe;
            simulationhashchecktodo.Enqueue(new HashCheckTodo() { frame = simulationallowedkeyframe, hash = hash });
        }
        struct CommandTodo
        {
            public byte[] cmd;
            public int playerid;
            public int frame;
        }
        struct HashCheckTodo
        {
            public int frame;
            public int hash;
        }
        Queue<HashCheckTodo> simulationhashchecktodo = new Queue<HashCheckTodo>();
        Queue<CommandTodo> simulationcmdtodo = new Queue<CommandTodo>();
        public void EnqueueCommand(int playerid, int frame, byte[] cmd)
        {
            simulationcmdtodo.Enqueue(new CommandTodo() { playerid = playerid, frame = frame, cmd = cmd });
        }
        #endregion
    }
    public class RailVehicle
    {
        public int currentvehiclespeedMul1000;
        public Vector3 currentrailblock;
        public int currentrailblockprogressMul1000 = 0;
        public VehicleDirection12 currentdirection;
        public VehicleDirection12 lastdirection;
        //public bool railriding = true;

        public bool turnleft;
        public bool turnright;
        public bool accelerate = false;
        public bool decelerate;
        public int maxvehiclespeedMul1000 = 15 * 1000;
    }
    public class MinecartToDraw : IModelToDraw
    {
        public Vector3 currentrailblock;
        public Vector3 position;
        public VehicleDirection12 direction;
        public VehicleDirection12 lastdirection;
        public double progress;
        public MinecartDrawer drawer;
        #region IModelToDraw Members
        public void Draw()
        {
            drawer.Draw(currentrailblock, position, direction, lastdirection, progress);
        }
        #endregion
    }
    public class MinecartDrawer
    {
        public float railheight = 0.3f;
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IThe3d the3d { get; set; }
        [Inject]
        public RailMapUtil railmaputil { get; set; }
        int minecarttexture = -1;
        #region IModelToDraw Members
        public void Draw(Vector3 currentrailblock, Vector3 position, VehicleDirection12 dir, VehicleDirection12 lastdir, double progress)
        {
            if (minecarttexture == -1)
            {
                minecarttexture = the3d.LoadTexture(getfile.GetFile("minecart.png"));
            }
            GL.PushMatrix();
            Vector3 glpos;
            if (position != new Vector3())
            {
                glpos = position;
            }
            else
            {
                glpos = CurrentRailPos(currentrailblock, dir, (float)progress);
            }
            glpos += new Vector3(0, -0.7f, 0);
            GL.Translate(glpos);
            double currot = vehiclerotation(dir);
            double lastrot = vehiclerotation(lastdir);
            //double rot = lastrot + (currot - lastrot) * progress;
            double rot = AngleInterpolation.InterpolateAngle360(lastrot, currot, progress);
            GL.Rotate(-rot - 90, 0, 1, 0);
            var c = new CharacterDrawerBlock();
            var cc = c.MakeCoords(8, 8, 8, 0, 0);
            CharacterDrawerBlock.MakeTextureCoords(cc, 32, 16);
            c.DrawCube(new Vector3(-0.5f, -0.3f, -0.5f), new Vector3(1, 1, 1), minecarttexture, cc);
            GL.PopMatrix();
        }
        #endregion
        public Vector3 CurrentRailPos(Vector3 currentrailblock, VehicleDirection12 currentdirection, float currentrailblockprogress)
        {
            var slope = railmaputil.GetRailSlope((int)currentrailblock.X,
                (int)currentrailblock.Y, (int)currentrailblock.Z);
            Vector3 a = currentrailblock;
            float x_correction = 0;
            float y_correction = 0;
            float z_correction = 0;
            switch (currentdirection)
            {
                case VehicleDirection12.HorizontalRight:
                    x_correction += currentrailblockprogress;
                    y_correction += 0.5f;
                    if (slope == RailSlope.TwoRightRaised)
                        z_correction += currentrailblockprogress;
                    if (slope == RailSlope.TwoLeftRaised)
                        z_correction += 1 - currentrailblockprogress;
                    break;
                case VehicleDirection12.HorizontalLeft:
                    x_correction += 1.0f - currentrailblockprogress;
                    y_correction += 0.5f;
                    if (slope == RailSlope.TwoRightRaised)
                        z_correction += 1 - currentrailblockprogress;
                    if (slope == RailSlope.TwoLeftRaised)
                        z_correction += currentrailblockprogress;
                    break;
                case VehicleDirection12.VerticalDown:
                    x_correction += 0.5f;
                    y_correction += currentrailblockprogress;
                    if (slope == RailSlope.TwoDownRaised)
                        z_correction += currentrailblockprogress;
                    if (slope == RailSlope.TwoUpRaised)
                        z_correction += 1 - currentrailblockprogress;
                    break;
                case VehicleDirection12.VerticalUp:
                    x_correction += 0.5f;
                    y_correction += 1.0f - currentrailblockprogress;
                    if (slope == RailSlope.TwoDownRaised)
                        z_correction += 1 - currentrailblockprogress;
                    if (slope == RailSlope.TwoUpRaised)
                        z_correction += currentrailblockprogress;
                    break;
                case VehicleDirection12.UpLeftLeft:
                    x_correction += 0.5f * (1.0f - currentrailblockprogress);
                    y_correction += 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.UpLeftUp:
                    x_correction += 0.5f * currentrailblockprogress;
                    y_correction += 0.5f - 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.UpRightRight:
                    x_correction += 0.5f + 0.5f * currentrailblockprogress;
                    y_correction += 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.UpRightUp:
                    x_correction += 1.0f - 0.5f * currentrailblockprogress;
                    y_correction += 0.5f - 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.DownLeftLeft:
                    x_correction += 0.5f * (1 - currentrailblockprogress);
                    y_correction += 1.0f - 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.DownLeftDown:
                    x_correction += 0.5f * currentrailblockprogress;
                    y_correction += 0.5f + 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.DownRightRight:
                    x_correction += 0.5f + 0.5f * currentrailblockprogress;
                    y_correction += 1.0f - 0.5f * currentrailblockprogress;
                    break;
                case VehicleDirection12.DownRightDown:
                    x_correction += 1.0f - 0.5f * currentrailblockprogress;
                    y_correction += 0.5f + 0.5f * currentrailblockprogress;
                    break;
            }
            //+1 because player can't be inside rail block (picking wouldn't work)
            return new Vector3(a.X + x_correction, a.Z + railheight + 1 + z_correction, a.Y + y_correction);
        }
        double vehiclerotation(VehicleDirection12 dir)
        {
            switch (dir)
            {
                case VehicleDirection12.VerticalUp:
                    return 0;
                case VehicleDirection12.DownRightRight:
                case VehicleDirection12.UpLeftUp:
                    return 45;
                case VehicleDirection12.HorizontalRight:
                    return 90;
                case VehicleDirection12.UpRightRight:
                case VehicleDirection12.DownLeftDown:
                    return 90 + 45;
                case VehicleDirection12.VerticalDown:
                    return 180;
                case VehicleDirection12.UpLeftLeft:
                case VehicleDirection12.DownRightDown:
                    return 180 + 45;
                case VehicleDirection12.HorizontalLeft:
                    return 180 + 90;
                case VehicleDirection12.UpRightUp:
                case VehicleDirection12.DownLeftLeft:
                    return 180 + 90 + 45;
                default:
                    throw new Exception();
            }
        }
    }
    public enum CommandId
    {
        Build,
        Craft,
        DumpOrLoad,
        RailVehicleEnterLeave,
        RailVehicleControl,
    }
    public interface IStreamizable
    {
        void ToStream(Stream s);
        void FromStream(Stream s);
    }
    public class CommandBuild : IStreamizable
    {
        public short x;
        public short y;
        public short z;
        public BlockSetMode mode;
        public byte tiletype;
        public void ToStream(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((short)x);
            bw.Write((short)y);
            bw.Write((short)z);
            bw.Write((byte)mode);
            bw.Write((byte)tiletype);
        }
        public void FromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            x = br.ReadInt16();
            y = br.ReadInt16();
            z = br.ReadInt16();
            mode = (BlockSetMode)br.ReadByte();
            tiletype = br.ReadByte();
        }
    }
    public class CommandRailVehicleEnterLeave : IStreamizable
    {
        public int vehicleid;
        public bool enter;
        #region IStreamizable Members
        public void ToStream(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((int)vehicleid);
            bw.Write((bool)enter);
        }
        public void FromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            vehicleid = br.ReadInt32();
            enter = br.ReadBoolean();
        }
        #endregion
    }
    public enum ControlAction
    {
        TurnLeft,
        TurnRight,
        TurnNone,
        AcceleratePlus,
        AccelerateMinus,
        AccelerateNone,
        Reverse,
    }
    public class CommandRailVehicleControl : IStreamizable
    {
        public ControlAction controlaction;
        #region IStreamizable Members
        public void ToStream(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((byte)controlaction);
        }
        public void FromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            controlaction = (ControlAction)br.ReadByte();
        }
        #endregion
    }
    public class CommandCraft : IStreamizable
    {
        public short x;
        public short y;
        public short z;
        public short recipeid;
        #region IStreamizable Members
        public void ToStream(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((short)x);
            bw.Write((short)y);
            bw.Write((short)z);
            bw.Write((short)recipeid);
        }
        public void FromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            x = br.ReadInt16();
            y = br.ReadInt16();
            z = br.ReadInt16();
            recipeid = br.ReadInt16();
        }
        #endregion
    }
    public class CommandDumpOrLoad : IStreamizable
    {
        public short x;
        public short y;
        public short z;
        public short blocktype;
        public bool dump;
        #region IStreamizable Members
        public void ToStream(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((short)x);
            bw.Write((short)y);
            bw.Write((short)z);
            bw.Write((short)blocktype);
            bw.Write((bool)dump);
        }
        public void FromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            x = br.ReadInt16();
            y = br.ReadInt16();
            z = br.ReadInt16();
            blocktype = br.ReadInt16();
            dump = br.ReadBoolean();
        }
        #endregion
    }
    public class GameDataTilesManicDigger : IGameData
    {
        public GameDataTilesMinecraft data = new GameDataTilesMinecraft();
        public GameDataTilesManicDigger()
        {
            datanew[(int)TileTypeManicDigger.BrushedMetal] = new TileTypeData() { Buildable=true, AllTextures = (5 * 16) + 0 };
            datanew[(int)TileTypeManicDigger.ChemicalGreen] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 1 };
            datanew[(int)TileTypeManicDigger.Salt] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 2 };
            datanew[(int)TileTypeManicDigger.Roof] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 3 };
            datanew[(int)TileTypeManicDigger.Camouflage] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 4 };
            datanew[(int)TileTypeManicDigger.DirtForFarming] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 5 };
            datanew[(int)TileTypeManicDigger.Apples] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 6 };
            datanew[(int)TileTypeManicDigger.Hay] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 7 };
            datanew[(int)TileTypeManicDigger.Crops1] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 8 };
            datanew[(int)TileTypeManicDigger.Crops2] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 9 };
            datanew[(int)TileTypeManicDigger.Crops3] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 10 };
            datanew[(int)TileTypeManicDigger.Crops4] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 11 };
            datanew[(int)TileTypeManicDigger.CraftingTable] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 0 };
            datanew[(int)TileTypeManicDigger.Minecart] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 1, TextureTop = (7 * 16) + 2 };
        }
        #region IGameData Members
        public int GetTileTextureId(int tileType, TileSide side)
        {
            if (datanew[tileType] != null)
            {
                if (side == TileSide.Top)
                {
                    return datanew[tileType].TextureTop;
                }
                if (side == TileSide.Bottom)
                {
                    return datanew[tileType].TextureBottom;
                }
                return datanew[tileType].TextureSide;
            }
            if (IsRailTile(tileType))
            {
                //return 1;
                if (side == TileSide.Top)
                {
                    return tileType;
                }
                else
                {
                    return data.GetTileTextureId((int)TileTypeMinecraft.Cobblestone, TileSide.Top);
                }
            }
            return data.GetTileTextureId(tileType, side);
        }
        public byte TileIdEmpty
        {
            get { return data.TileIdEmpty; }
        }
        public byte TileIdGrass
        {
            get { return data.TileIdGrass; }
        }
        public byte TileIdDirt
        {
            get { return data.TileIdDirt; }
        }
        public int[] DefaultMaterialSlots
        {
            get
            {
                var slots = new List<int>();
                slots.Add((int)TileTypeMinecraft.Dirt);
                slots.Add((int)TileTypeMinecraft.Brick);
                slots.Add(railstart + (int)RailDirectionFlags.TwoHorizontalVertical);
                slots.Add(railstart + (int)RailDirectionFlags.Corners);
                slots.Add((int)TileTypeMinecraft.Gravel);
                slots.Add((int)TileTypeMinecraft.Cobblestone);
                slots.Add((int)TileTypeMinecraft.Wood);
                slots.Add((int)TileTypeMinecraft.Leaves);
                slots.Add(railstart + (int)RailDirectionFlags.Horizontal);
                slots.Add(railstart + (int)RailDirectionFlags.Vertical);
                return slots.ToArray();
            }
        }
        public byte GoldTileId
        {
            get { return data.GoldTileId; }
        }
        public int TileIdStone
        {
            get { return data.TileIdStone; }
        }
        public int TileIdWater
        {
            get { return data.TileIdWater; }
        }
        public int TileIdSand
        {
            get { return data.TileIdSand; }
        }
        public bool IsWaterTile(int tiletype)
        {
            return data.IsWaterTile(tiletype);
        }
        public bool IsBuildableTile(int tiletype)
        {
            //temporary
            if (tiletype == railstart + (int)RailDirectionFlags.TwoHorizontalVertical) { return true; }
            if (tiletype == railstart + (int)(RailDirectionFlags.UpLeft | RailDirectionFlags.UpRight |
                RailDirectionFlags.DownLeft | RailDirectionFlags.DownRight)) { return true; }
            if (IsRailTile(tiletype)) { return false; }
            if (datanew[tiletype] != null) { return true; }
            return data.IsValidTileType(tiletype)
                && tiletype != (int)TileTypeMinecraft.Water
                && tiletype != (int)TileTypeMinecraft.InfiniteWaterSource
                && tiletype != (int)TileTypeMinecraft.Lava
                && tiletype != (int)TileTypeMinecraft.InfiniteLavaSource;
            //return data.IsValidTileType(tiletype) && !data.IsWaterTile(tiletype) && tiletype != (int)TileTypeMinecraft.Lava
            //    && tiletype != (int)TileTypeMinecraft.InfiniteLavaSource && tiletype != (int)TileTypeMinecraft.StationaryLava;
            //----
            if (datanew[tiletype] != null) { return datanew[tiletype].Buildable; }
            if (tiletype == railstart + (int)RailDirectionFlags.TwoHorizontalVertical) { return true; }
            if (tiletype == railstart + (int)(RailDirectionFlags.UpLeft|RailDirectionFlags.UpRight|
                RailDirectionFlags.DownLeft|RailDirectionFlags.DownRight)) { return true; }
            if (IsRailTile(tiletype)) { return false; }
            return data.IsBuildableTile(tiletype);
        }
        public bool IsValidTileType(int tiletype)
        {
            if (datanew[tiletype] != null) { return true; }
            if (IsRailTile(tiletype)) { return true; }
            return data.IsValidTileType(tiletype);
        }
        public bool IsTransparentTile(int tiletype)
        {
            if (tiletype == (int)TileTypeManicDigger.Crops1) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops2) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops3) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops4) { return true; }
            if (IsRailTile(tiletype)) { return true; }
            return data.IsTransparentTile(tiletype);
        }
        public int PlayerBuildableMaterialType(int p)
        {
            return data.PlayerBuildableMaterialType(p);
        }
        public bool IsBlockFlower(int tiletype)
        {
            if (tiletype == (int)TileTypeManicDigger.Crops1) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops2) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops3) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops4) { return true; }
            return data.IsBlockFlower(tiletype);
        }
        #endregion
        #region IGameData Members
        public RailDirectionFlags GetRail(int tiletype)
        {
            if (IsRailTile(tiletype))
            {
                return (RailDirectionFlags)(tiletype - railstart);
            }
            return RailDirectionFlags.None;
        }
        public static bool IsRailTile(int tiletype)
        {
            return tiletype >= railstart && tiletype < railstart + 64;
        }
        #endregion
        public static int railstart = (11 * 16);
        #region IGameData Members
        public int TileIdSingleStairs
        {
            get { return data.TileIdSingleStairs; }
        }
        #endregion
        #region IGameData Members
        public int TileIdSponge
        {
            get { return data.TileIdSponge; }
        }
        #endregion
        #region IGameData Members
        public int GetTileTextureIdForInventory(int tileType)
        {
            if (IsRailTile(tileType))
            {
                return GetTileTextureId(tileType, TileSide.Top);
            }
            if (datanew[tileType] != null)
            {
                return datanew[tileType].TextureSide;
            }
            return data.GetTileTextureIdForInventory(tileType);
        }
        #endregion
        TileTypeData[] datanew = new TileTypeData[256];
        #region IGameData Members
        public string BlockName(int blocktype)
        {
            if (data.IsValidTileType(blocktype))
            {
                return data.BlockName(blocktype);
            }
            if (IsRailTile(blocktype))
            {
                return "Rail";
            }
            return Enum.GetName(typeof(TileTypeManicDigger), blocktype);
        }
        #endregion
        #region IGameData Members
        public bool IsEmptyForPhysics(int blocktype)
        {
            return data.IsEmptyForPhysics(blocktype)
                || blocktype == (int)TileTypeManicDigger.Crops1
                || blocktype == (int)TileTypeManicDigger.Crops2
                || blocktype == (int)TileTypeManicDigger.Crops3
                || blocktype == (int)TileTypeManicDigger.Crops4;
        }
        #endregion
        #region IGameData Members
        public float BlockWalkSpeed(int blocktype)
        {
            return data.BlockWalkSpeed(blocktype);
        }
        #endregion
    }
    public enum TileTypeManicDigger
    {
        BrushedMetal = 100,
        ChemicalGreen,
        Salt,
        Roof,
        Camouflage,
        DirtForFarming,
        Apples,
        Hay,
        Crops1,
        Crops2,
        Crops3,
        Crops4,
        CraftingTable,
        Minecart,
    }
}

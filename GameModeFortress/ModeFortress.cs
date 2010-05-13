using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Collisions;
using ManicDigger;
using OpenTK;
using System.IO;
using System.Drawing;

namespace GameModeFortress
{
    public class GameFortress : IGameMode, IGameWorld, IMapStorage, IClients, ITerrainInfo
    {
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public ITicks ticks { get; set; }
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
            public byte[, ,] Map { get { return game.Map; } set { game.Map = value; } }
            public int MapSizeX { get { return game.MapSizeX; } set { game.MapSizeX = value; } }
            public int MapSizeY { get { return game.MapSizeX; } set { game.MapSizeY = value; } }
            public int MapSizeZ { get { return game.MapSizeZ; } set { game.MapSizeZ = value; } }
            public int GetBlock(int x, int y, int z)
            {
                return game.GetBlockForPhysics(x,y,z);
            }
            public void SetBlock(int x, int y, int z, byte tileType)
            {
                game.SetBlock(x, y, z, tileType);
            }
            public float WaterLevel { get { return game.WaterLevel; } set { throw new Exception(); } }
            public void Dispose()
            {
            }
            #endregion
        }
        public GameFortress()
        {
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
            mapforphysics = new MapForPhysics() { game = this };
        }
        float currentvehiclespeed;
        Vector3 currentrailblock;
        float currentrailblockprogress = 0;
        VehicleDirection12 currentdirection;
        Vector3 CurrentRailPos()
        {
            var slope = RailMapUtil().GetRailSlope((int)currentrailblock.X,
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
        bool railriding = false;
        bool wasqpressed = false;
        bool wasvpressed = false;
        VehicleDirection12? BestNewDirection(VehicleDirection12Flags dir, bool turnleft, bool turnright)
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
        public void OnNewFrame(double dt)
        {
            Tick();
            bool turnright = viewport.keyboardstate[OpenTK.Input.Key.D];
            bool turnleft = viewport.keyboardstate[OpenTK.Input.Key.A];
            RailSound();
            if (railriding)
            {
                viewport.ENABLE_FREEMOVE = true;
                viewport.ENABLE_MOVE = false;
                viewport.LocalPlayerPosition = CurrentRailPos();
                currentrailblockprogress += currentvehiclespeed * (float)dt;
                if (currentrailblockprogress >= 1)
                {
                    currentrailblockprogress = 0;
                    var newenter = new TileEnterData();
                    newenter.BlockPosition = NextTile(currentdirection, currentrailblock);
                    //slope
                    if (GetUpDownMove(currentrailblock,
                        DirectionUtils.ResultEnter(DirectionUtils.ResultExit(currentdirection))) == UpDown.Up)
                    {
                        newenter.BlockPosition.Z++;
                    }
                    if (GetUpDownMove(newenter.BlockPosition + new Vector3(0, 0, -1),
                        DirectionUtils.ResultEnter(DirectionUtils.ResultExit(currentdirection))) == UpDown.Down)
                    {
                        newenter.BlockPosition.Z--;
                    }

                    newenter.EnterDirection = DirectionUtils.ResultEnter(DirectionUtils.ResultExit(currentdirection));
                    var newdir = BestNewDirection(PossibleRails(newenter), turnleft, turnright);
                    if (newdir == null)
                    {
                        //end of rail
                        currentdirection = DirectionUtils.Reverse(currentdirection);
                    }
                    else
                    {
                        currentdirection = newdir.Value;
                        currentrailblock = newenter.BlockPosition;
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
            }
            if (viewport.keyboardstate[OpenTK.Input.Key.W])
            {
                currentvehiclespeed += 1f * (float)dt;
            }
            if (viewport.keyboardstate[OpenTK.Input.Key.S])
            {
                currentvehiclespeed -= 5f * (float)dt;
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
            if (currentvehiclespeed < 0)
            {
                currentvehiclespeed = 0;
            }
            //todo fix
            //if (viewport.keypressed != null && viewport.keypressed.Key == OpenTK.Input.Key.Q)            
            if(!wasqpressed && viewport.keyboardstate[OpenTK.Input.Key.Q])
            {
                Reverse();
            }

            if (!wasvpressed && viewport.keyboardstate[OpenTK.Input.Key.V] && !railriding)
            {
                currentrailblock = new Vector3((int)viewport.LocalPlayerPosition.X,
                    (int)viewport.LocalPlayerPosition.Z, (int)viewport.LocalPlayerPosition.Y - 1);
                if (!MapUtil.IsValidPos(map, (int)currentrailblock.X, (int)currentrailblock.Y, (int)currentrailblock.Z))
                {
                    ExitVehicle();
                }
                else
                {
                    var railunderplayer = data.GetRail(
                        map.GetBlock((int)currentrailblock.X, (int)currentrailblock.Y, (int)currentrailblock.Z));
                    railriding = true;
                    viewport.CharacterHeight = minecartheight;
                    currentvehiclespeed = 0;
                    if (railunderplayer == RailDirectionFlags.Horizontal)
                    {
                        currentdirection = VehicleDirection12.HorizontalRight;
                    }
                    else if (railunderplayer == RailDirectionFlags.Vertical)
                    {
                        currentdirection = VehicleDirection12.VerticalUp;
                    }
                    else
                    {
                        ExitVehicle();
                    }
                }
            }
            else if (!wasvpressed && viewport.keyboardstate[OpenTK.Input.Key.V] && railriding)
            {
                ExitVehicle();
            }
            wasqpressed = viewport.keyboardstate[OpenTK.Input.Key.Q];
            wasvpressed = viewport.keyboardstate[OpenTK.Input.Key.V];
        }
        private void ExitVehicle()
        {
            viewport.CharacterHeight = WalkCharacterHeight;
            railriding = false;
            viewport.ENABLE_FREEMOVE = false;
            viewport.ENABLE_MOVE = true;
        }
        DateTime lastrailsoundtime;
        int lastrailsound;
        private void RailSound()
        {
            float railsoundpersecond = currentvehiclespeed;
            if (railsoundpersecond > 10)
            {
                railsoundpersecond = 10;
            }
            audio.PlayAudioLoop("railnoise.wav", railriding && railsoundpersecond > 0.1f);
            if (!railriding)
            {
                return;
            }
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
        private void Reverse()
        {
            currentdirection = DirectionUtils.Reverse(currentdirection);
            currentrailblockprogress = 1 - currentrailblockprogress;
            //currentvehiclespeed = 0;
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
                //ticks.DoCommand(MakeCommand(CommandId.Build, cmd)); ???
                network.SendSetBlock(blockpos, mode, activematerial);
                if (mode == BlockSetMode.Destroy)
                {
                    activematerial = data.TileIdEmpty;
                }
                //speculative
                map.Map[x, y, z] = (byte)activematerial;
                terrain.UpdateTile(x, y, z);
            }
        }
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
            public CharacterState state;
            public float buildprogress;
            public List<Vector3> path;
        }
        enum CharacterState
        {
            Walking,
            Building,
        }
        List<Character> characters = new List<Character>();
        [Inject]
        public CharacterPhysics physics { get; set; }
        float basecharactersmovespeed { get { return ManicDiggerGameWindow.basemovespeed / 3; } }
        [Inject]
        public Pathfinder3d pathfinder{get;set;}
        void UpdateCharacters(float dt)
        {
            return;
            if (characters.Count == 0)
            {
                var v0 = new Character();
                characters.Add(v0);
                //v0.orders = new List<Vector3>();
                //v0.orders.Add(new Vector3(0, 32, 0));
                //v0.orders.Add(new Vector3(16, 32, 0));
                //v0.currentorder = new Vector3(0, 32, 0);
                v0.pos3d = PlayerPositionSpawn;
            }
            //clear wrong orders
            for (int i = 0; i < characters.Count; i++)
            {
                var v0 = characters[i];
                if (v0.currentorder != null && !orders.ContainsKey(v0.currentorder.Value))
                {
                    v0.currentorder = null;
                    v0.state = CharacterState.Walking;
                    v0.buildprogress = 0;
                }
            }
            var lcharacters = new List<Character>(characters);
            for (int i = 0; i < lcharacters.Count; i++)
            {
                var v0 = lcharacters[i];
                if (v0.currentorder == null)
                {
                    Vector3? destination = ClosestBuildOrder(v0.pos3d);
                    if (destination == null)
                    {
                        continue;
                    }
                    v0.currentorder = destination;
                }
                if (v0.state == CharacterState.Walking)
                {
                    Vector3 curorder = To3d(v0.currentorder.Value);//v0.orders[v0.currentOrderId]
                    var dir = (curorder - v0.pos3d);
                    dir.Normalize();
                    var newpos = v0.pos3d + Vector3.Multiply(dir, dt * basecharactersmovespeed);
                    //newpos = physics.WallSlide(v0.pos3d, newpos);
                    var delta = newpos - v0.pos3d;
                    //if (delta.Length < dt * 0.1 * ManicDiggerGameWindow.basemovespeed)
                    if ((newpos - curorder).Length < 0.2)
                    {
                        v0.moves = false;
                        v0.state = CharacterState.Building;
                    }
                    else
                    {
                        v0.moves = true;
                        v0.dir3d = newpos - v0.pos3d;
                    }
                    v0.pos3d = newpos;
                }
                else if (v0.state == CharacterState.Building)
                {
                    v0.buildprogress += dt;
                    if (v0.buildprogress > 2)
                    {
                        var vv = v0.currentorder.Value;
                        DoOrder(vv);
                    }
                }
                else
                {
                    throw new Exception();
                }
                /*
                if ((v0.pos3d - v0.orders[v0.currentOrderId]).Length < 0.5f)
                {
                    v0.progress = 0;
                    v0.currentOrderId++;
                    if (v0.currentOrderId > 1)
                    {
                        v0.currentOrderId = 0;
                    }
                }
                */
            }
        }
        private void DoOrder(Vector3 vv)
        {
            var o = orders[vv];
            map.Map[(int)vv.X, (int)vv.Y, (int)vv.Z] = o.mode == BlockSetMode.Create ? (byte)o.tiletype : data.TileIdEmpty;
            terrain.UpdateTile((int)vv.X, (int)vv.Y, (int)vv.Z);
            orders.Remove(vv);
        }
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
            throw new NotImplementedException();
        }
        public void LoadState(byte[] savegame)
        {
            throw new NotImplementedException();
        }
        public string GameInfo
        {
            get { return ""; }
        }
        public void Tick()
        {
            float dt = 1.0f / 75;
            UpdateCharacters(dt);
        }
        public void DoCommand(byte[] command, int player_id)
        {
            MemoryStream ms = new MemoryStream(command);
            BinaryReader br = new BinaryReader(ms);
            CommandId commandid = (CommandId)br.ReadByte();
            switch (commandid)
            {
                case CommandId.Build:
                    var cmd = new CommandBuild();
                    cmd.FromStream(ms);
                    Vector3 v = new Vector3(cmd.x, cmd.y, cmd.z);
                    if (ENABLE_BUILD_ORDERS)
                    {
                        //cancelling orders
                        if (map.GetBlock(cmd.x, cmd.y, cmd.z) == data.TileIdEmpty
                            && cmd.mode == BlockSetMode.Destroy)
                        {
                            RemoveOrder(v);
                            break;
                        }
                        if (map.GetBlock(cmd.x, cmd.y, cmd.z) == cmd.tiletype
                             && cmd.mode == BlockSetMode.Create)
                        {
                            RemoveOrder(v);
                            break;
                        }
                        orders[v] = new BuildOrder()
                        {
                            playerid = player_id,
                            position = v,
                            mode = cmd.mode,
                            tiletype = cmd.tiletype,
                        };
                    }
                    else
                    {
                        map.Map[cmd.x, cmd.y, cmd.z] = cmd.mode == BlockSetMode.Create ?
                            (byte)cmd.tiletype : data.TileIdEmpty;
                    }
                    terrain.UpdateTile(cmd.x, cmd.y, cmd.z);
                    break;
                default:
                    throw new Exception();
            }
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
        public Vector3 PlayerPositionSpawn { get { return playerpositionspawn; } }

        public MapStorage map = new MapStorage();
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int,Player> Players { get { return players; } set { players = value; } }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, byte tileType)
        {
            map.Map[x, y, z] = tileType;
        }
        #endregion
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
        bool ENABLE_BUILD_ORDERS = false;
        #region ITerrainInfo Members
        public int GetTerrainBlock(int x, int y, int z)
        {
            if (ENABLE_BUILD_ORDERS)
            {
                var v = new Vector3(x, y, z);
                if (orders.ContainsKey(v))
                {
                    return orders[v].tiletype;
                }
            }
            return Map[x, y, z];
        }
        public System.Drawing.Color GetTerrainBlockColor(int x, int y, int z)
        {
            var v = new Vector3(x, y, z);
            if (orders.ContainsKey(v))
            {
                Color c = orders[v].mode == BlockSetMode.Create ?
                    Color.FromArgb(100, 100, 255) : Color.FromArgb(255, 100, 100);
                return c;
            }
            return Color.White;
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
        //Needed for walking on and picking the build order blocks.
        internal int GetBlockForPhysics(int x,int y,int z)
        {
            if (ENABLE_BUILD_ORDERS)
            {
                var v = new Vector3(x, y, z);
                if (orders.ContainsKey(v))
                {
                    return orders[v].mode == BlockSetMode.Create ? orders[v].tiletype : data.TileIdEmpty;
                }
            }
            return map.Map[x,y,z];
        }
    }
    public enum CommandId
    {
        Build,
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
    public class GameDataTilesManicDigger : IGameData
    {
        public GameDataTilesMinecraft data = new GameDataTilesMinecraft();
        #region IGameData Members
        public int GetTileTextureId(int tileType, TileSide side)
        {
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
            if (tiletype == railstart + (int)RailDirectionFlags.TwoHorizontalVertical) { return true; }
            if (tiletype == railstart + (int)(RailDirectionFlags.UpLeft|RailDirectionFlags.UpRight|
                RailDirectionFlags.DownLeft|RailDirectionFlags.DownRight)) { return true; }
            if (IsRailTile(tiletype)) { return false; }
            return data.IsBuildableTile(tiletype);
        }
        public bool IsValidTileType(int tiletype)
        {
            if (IsRailTile(tiletype)) { return true; }
            return data.IsValidTileType(tiletype);
        }
        public bool IsTransparentTile(int tiletype)
        {
            if (IsRailTile(tiletype)) { return true; }
            return data.IsTransparentTile(tiletype);
        }
        public int PlayerBuildableMaterialType(int p)
        {
            return data.PlayerBuildableMaterialType(p);
        }
        public bool IsBlockFlower(int tiletype)
        {
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
    }
}

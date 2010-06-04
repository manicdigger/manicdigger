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
    public class WorldGenerator : IWorldGenerator
    {
        IGameData data = new GameDataTilesMinecraft();
        public WorldGenerator()
        {
        }
        int waterlevel = 20;
        byte[,] heightcache;
        #region IWorldGenerator Members
        public byte[, ,] GetChunk(int x, int y, int z, int chunksize)
        {
            heightcache = new byte[chunksize, chunksize];
            x = x * chunksize;
            y = y * chunksize;
            z = z * chunksize;
            byte[, ,] chunk = new byte[chunksize, chunksize, chunksize];
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    heightcache[xx, yy] = GetHeight(x + xx, y + yy);
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        chunk[xx, yy, zz] = (byte)GetBlockInside(x + xx, y + yy, z + zz, heightcache[xx, yy]);
                    }
                }
            }
            return chunk;
        }
        public int GetBlock(int x, int y, int z)
        {
            return GetBlockInside(x, y, z, GetHeight(x, y));
        }
        int GetBlockInside(int x, int y, int z, int height)
        {
            Vector2i v = new Vector2i(x, y);
            if (z > waterlevel)
            {
                if (z > height) { return data.TileIdEmpty; }
                if (z == height) { return data.TileIdGrass; }
                return data.TileIdDirt;
            }
            else
            {
                if (z > height) { return data.TileIdWater; }
                if (z == height) { return data.TileIdSand; }
                return data.TileIdDirt;
            }
        }
        private byte GetHeight(int x, int y)
        {
            //double p = 0.2 + ((findnoise2(x / 100.0, y / 100.0) + 1.0) / 2) * 0.3;
            double p = 0.5;
            double zoom = 150;
            double getnoise = 0;
            int octaves = 6;
            for (int a = 0; a < octaves - 1; a++)//This loops trough the octaves.
            {
                double frequency = Math.Pow(2, a);//This increases the frequency with every loop of the octave.
                double amplitude = Math.Pow(p, a);//This decreases the amplitude with every loop of the octave.
                getnoise += noise(((double)x) * frequency / zoom, ((double)y) / zoom * frequency) * amplitude;//This uses our perlin noise functions. It calculates all our zoom and frequency and amplitude
            }
            double maxheight = 64;
            int height = (int)(((getnoise + 1) / 2.0) * (maxheight - 5)) + 3;//(int)((getnoise * 128.0) + 128.0);
            if (height > maxheight - 1) { height = (int)maxheight - 1; }
            if (height < 2) { height = 2; }
            return (byte)height;
        }
        #endregion
        double findnoise2(double x, double y)
        {
            int n = (int)x + (int)y * 57;
            n = (n << 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return 1.0 - ((double)nn / 1073741824.0);
        }
        double interpolate(double a, double b, double x)
        {
            double ft = x * 3.1415927;
            double f = (1.0 - Math.Cos(ft)) * 0.5;
            return a * (1.0 - f) + b * f;
        }
        double noise(double x, double y)
        {
            double floorx = (double)((int)x);//This is kinda a cheap way to floor a double integer.
            double floory = (double)((int)y);
            double s, t, u, v;//Integer declaration
            s = findnoise2(floorx, floory);
            t = findnoise2(floorx + 1, floory);
            u = findnoise2(floorx, floory + 1);//Get the surrounding pixels to calculate the transition.
            v = findnoise2(floorx + 1, floory + 1);
            double int1 = interpolate(s, t, x - floorx);//Interpolate between the values.
            double int2 = interpolate(u, v, x - floorx);//Here we use x-floorx, to get 1st dimension. Don't mind the x-floorx thingie, it's part of the cosine formula.
            return interpolate(int1, int2, y - floory);//Here we use y-floory, to get the 2nd dimension.
        }
        #region IWorldGenerator Members
        public byte[] GetBlocks(int[] pos)
        {
            byte[] blocks = new byte[pos.Length / 3];
            for (int i = 0; i < pos.Length / 3; i += 3)
            {
                int x = i;
                int y = i + 1;
                int z = i + 2;
                blocks[i / 3] = (byte)GetBlock(x, y, z);
            }
            return blocks;
        }
        #endregion
    }
    public class InfiniteMap : IMapStorage
    {
        public IWorldGenerator gen { get; set; }
        #region IMapStorage Members
        public int MapSizeX { get { return 10 * 1000; } set { } }
        public int MapSizeY { get { return 10 * 1000; } set { } }
        public int MapSizeZ { get { return 64; } set { } }
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
    public class GameFortress : IGameMode, IGameWorld, IMapStorage, IClients, ITerrainInfo
    {
        [Inject]
        public WorldGeneratorSandbox worldgeneratorsandbox { get; set; }
        [Inject]
        public InfiniteMap map { get; set; }
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
        }
        float currentvehiclespeed;
        Vector3 currentrailblock;
        float currentrailblockprogress = 0;
        VehicleDirection12 currentdirection;
        VehicleDirection12 lastdirection;
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
            viewport.LocalPlayerAnimationHint.InVehicle = railriding;
            viewport.LocalPlayerAnimationHint.DrawFix = railriding ? new Vector3(0, -0.7f, 0) : new Vector3();

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
                    lastdirection = currentdirection;
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
                    if ((railunderplayer & RailDirectionFlags.Horizontal) != 0)
                    {
                        currentdirection = VehicleDirection12.HorizontalRight;
                    }
                    else if ((railunderplayer & RailDirectionFlags.Vertical) != 0)
                    {
                        currentdirection = VehicleDirection12.VerticalUp;
                    }
                    else if ((railunderplayer & RailDirectionFlags.UpLeft) != 0)
                    {
                        currentdirection = VehicleDirection12.UpLeftUp;
                    }
                    else if ((railunderplayer & RailDirectionFlags.UpRight) != 0)
                    {
                        currentdirection = VehicleDirection12.UpRightUp;
                    }
                    else if ((railunderplayer & RailDirectionFlags.DownLeft) != 0)
                    {
                        currentdirection = VehicleDirection12.DownLeftLeft;
                    }
                    else if ((railunderplayer & RailDirectionFlags.DownRight) != 0)
                    {
                        currentdirection = VehicleDirection12.DownRightRight;
                    }
                    else
                    {
                        ExitVehicle();
                    }
                    lastdirection = currentdirection;
                }
            }
            else if (!wasvpressed && viewport.keyboardstate[OpenTK.Input.Key.V] && railriding)
            {
                ExitVehicle();
                viewport.LocalPlayerPosition += new Vector3(0, 0.7f, 0);
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
            lastdirection = currentdirection;
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
                //ticks.DoCommand(MakeCommand(CommandId.Build, cmd));
                network.SendCommand(MakeCommand(CommandId.Build, cmd));
                //network.SendSetBlock(blockpos, mode, activematerial);
                if (mode == BlockSetMode.Destroy)
                {
                    activematerial = data.TileIdEmpty;
                }
                //speculative
                map.SetBlock(x, y, z, (byte)activematerial);
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
            b.AppendLine(XmlTool.X("InfiniteWorldGenerator", SecurityElement.Escape(generator)));
            byte[] mapdata = map.SaveBlocks();
            b.AppendLine(XmlTool.X("InfiniteMapData", Convert.ToBase64String(mapdata)));
            b.AppendLine("</ManicDiggerSave>");
            return GzipCompression.Compress(Encoding.UTF8.GetBytes(b.ToString()));
        }
        public string generator;
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
        public void Tick()
        {
            float dt = 1.0f / 75;
        }
        public void DoCommand(byte[] command, int player_id)
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
                        map.SetBlock(cmd.x, cmd.y, cmd.z, cmd.mode == BlockSetMode.Create ?
                            (byte)cmd.tiletype : data.TileIdEmpty);
                        terrain.UpdateTile(cmd.x, cmd.y, cmd.z);
                    }
                    break;
                case CommandId.EnterLeaveRailVehicle:
                    {
                        var cmd = new CommandEnterLeaveRailVehicle();
                        cmd.FromStream(ms);
                    }
                    break;
                default:
                    throw new Exception();
            }
        }
        class RailVehicle
        {
        }
        Dictionary<int, RailVehicle> vehicles = new Dictionary<int, RailVehicle>();
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
        //Needed for walking on and picking the build order blocks.
        internal int GetBlockForPhysics(int x,int y,int z)
        {
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
                if (railriding)
                {
                    var m = new Minecart();
                    m.drawer = minecartdrawer;
                    m.position = viewport.LocalPlayerPosition;
                    m.direction = currentdirection;
                    m.lastdirection = lastdirection;
                    m.progress = currentrailblockprogress;
                    yield return m;
                }
            }
        }
        #endregion
    }
    public class Minecart : IModelToDraw
    {
        public Vector3 position;
        public VehicleDirection12 direction;
        public VehicleDirection12 lastdirection;
        public double progress;
        public MinecartDrawer drawer;
        #region IModelToDraw Members
        public void Draw()
        {
            drawer.Draw(position, direction, lastdirection, progress);
        }
        #endregion
    }
    public class MinecartDrawer
    {
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IThe3d the3d { get; set; }
        int minecarttexture = -1;
        #region IModelToDraw Members
        public void Draw(Vector3 position, VehicleDirection12 dir, VehicleDirection12 lastdir, double progress)
        {
            if (minecarttexture == -1)
            {
                minecarttexture = the3d.LoadTexture(getfile.GetFile("minecart.png"));
            }
            GL.PushMatrix();
            GL.Translate(position + new Vector3(0, -0.7f, 0));
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
        EnterLeaveRailVehicle,
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
    public class CommandEnterLeaveRailVehicle : IStreamizable
    {
        public short x;
        public short y;
        public short z;
        public bool enter;
        #region IStreamizable Members
        public void ToStream(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((short)x);
            bw.Write((short)y);
            bw.Write((short)z);
            bw.Write((bool)enter);
        }
        public void FromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            x = br.ReadInt16();
            y = br.ReadInt16();
            z = br.ReadInt16();
            enter = br.ReadBoolean();
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
    }
}

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
        public GameFortress()
        {
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
        }
        public void OnNewFrame(double dt)
        {
            Tick();
        }
        public void OnPick(OpenTK.Vector3 vector3, bool right)
        {
            var cmd = new CommandBuild()
            {
                x = (short)vector3.X,
                y = (short)vector3.Y,
                z = (short)vector3.Z,
                mode = right ? BlockSetMode.Create : BlockSetMode.Destroy,
                tiletype = (byte)viewport.MaterialSlots[viewport.activematerial],
            };
            ticks.DoCommand(MakeCommand(CommandId.Build, cmd));
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
        }
        enum CharacterState
        {
            Walking,
            Building,
        }
        List<Character> characters = new List<Character>();
        [Inject]
        public CharacterPhysics physics { get; set; }
        void UpdateCharacters(float dt)
        {
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
            for (int i = 0; i < characters.Count; i++)
            {
                var v0 = characters[i];
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
                    var newpos = v0.pos3d + Vector3.Multiply(dir, dt * ManicDiggerGameWindow.basemovespeed);
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
                        var o = orders[v0.currentorder.Value];
                        map.Map[(int)vv.X, (int)vv.Y, (int)vv.Z] = o.mode == BlockSetMode.Create ? (byte)o.tiletype : data.TileIdEmpty;
                        terrain.UpdateTile((int)vv.X, (int)vv.Y, (int)vv.Z);
                        orders.Remove(v0.currentorder.Value);
                        v0.currentorder = null;
                        v0.state = CharacterState.Walking;
                        v0.buildprogress = 0;
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
                    orders[v] = new BuildOrder()
                    {
                        playerid = player_id,
                        position = v,
                        mode = cmd.mode,
                        tiletype = cmd.tiletype,
                    };
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
        #region ITerrainInfo Members
        public int GetBlock(int x, int y, int z)
        {
            var v = new Vector3(x, y, z);
            if (orders.ContainsKey(v))
            {
                return orders[v].tiletype;
            }
            return Map[x, y, z];
        }
        public System.Drawing.Color GetBlockColor(int x, int y, int z)
        {
            var v = new Vector3(x, y, z);
            if (orders.ContainsKey(v))
            {
                Color c = orders[v].mode == BlockSetMode.Create ? Color.Blue : Color.Red;
                return c;
            }
            return Color.White;
        }
        #endregion
        #region IGameMode Members
        public void OnNewMap()
        {
        }
        #endregion
        #region IGameMode Members
        public Vector3 PlayerOrientationSpawn
        {
            get { return new Vector3((float)Math.PI, 0, 0); }
        }
        #endregion
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
        public int GetTileTextureId(int tileType, TileSide side)
        {
            TileTypesManicDigger tt = (TileTypesManicDigger)tileType;
            if (tt == TileTypesManicDigger.Grass)
            {
                if (side == TileSide.Top) { return 0; }
                if (side == TileSide.Bottom) { return 1; }
                return 2;
            }
            if (tt == TileTypesManicDigger.Wall) { return 2; }
            if (tt == TileTypesManicDigger.Dirt) { return 3; }
            if (tt == TileTypesManicDigger.Gold) { return 4; }
            //if ((int)tt < 3) { return (int)tt - 1; }
            return 255;
        }
        public byte TileIdEmpty { get { return (int)TileTypesManicDigger.Empty; } }
        public byte TileIdGrass { get { return (int)TileTypesManicDigger.Grass; } }
        public byte TileIdDirt { get { return (int)TileTypesManicDigger.Dirt; } }
        public int[] DefaultMaterialSlots
        {
            get
            {
                int[] m = new int[10];
                for (int i = 0; i < 10; i++)
                {
                    m[i] = (i + 1);
                }
                return m;
            }
        }
        public byte GoldTileId { get { return (int)TileTypesManicDigger.Gold; } }
        #region IGameData Members
        public int TileIdStone
        {
            get { return TileIdDirt; }//todo
        }
        public int TileIdWater
        {
            get { return (int)TileTypesManicDigger.Water; }//todo
        }
        public int TileIdSand
        {
            get { return TileIdDirt; }//todo
        }
        public bool IsWaterTile(int tiletype)
        {
            return tiletype == TileIdWater;
        }
        #endregion
        #region IGameData Members
        public bool IsBuildableTile(int tiletype)
        {
            return tiletype != TileIdWater;
        }
        #endregion
        #region IGameData Members
        public bool IsValidTileType(int tiletype)
        {
            if (tiletype == 0) { return false; }
            return tiletype < (int)TileTypesManicDigger.Count;
        }
        #endregion
        public bool IsTransparentTile(int tileType)
        {
            return false;
        }
        public int PlayerBuildableMaterialType(int p)
        {
            return p;
        }
        #region IGameData Members
        public bool IsBlockFlower(int tiletype)
        {
            return false;
        }
        #endregion
    }
    public enum TileTypesManicDigger
    {
        Empty,
        Grass,
        Floor,
        Wall,
        Dirt,
        Gold,
        Water,
        Count,
    }
}

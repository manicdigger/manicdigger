using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Collisions;
using ManicDigger;
using OpenTK;
using System.IO;

namespace GameModeFortress
{
    public class GameFortress : IGameMode, IGameWorld
    {
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public ITicks ticks { get; set; }
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
                mode = right ? BlockSetMode.Create : BlockSetMode.Destroy
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
            public List<Vector3> orders = new List<Vector3>();
            public float progress;
            public int currentOrderId = 0;
            public int cargoAmount = 0;
            public Vector3 dir3d;
            public bool moves;
            public Vector3 Pos3d { get { return pos3d; } }
            public Vector3 Dir3d { get { return dir3d; } }
            public bool Moves { get { return moves; } }
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
                v0.orders = new List<Vector3>();
                v0.orders.Add(new Vector3(0, 32, 0));
                v0.orders.Add(new Vector3(16, 32, 0));
                v0.pos3d = PlayerPositionSpawn;
            }
            for (int i = 0; i < characters.Count; i++)
            {
                var v0 = characters[i];
                var dir = (v0.orders[v0.currentOrderId] - v0.pos3d);
                dir.Normalize();
                var newpos = v0.pos3d + Vector3.Multiply(dir, dt * ManicDiggerGameWindow.basemovespeed);
                //Console.Write(v0.pos3d);
                newpos = physics.WallSlide(v0.pos3d, newpos);
                var delta = newpos - v0.pos3d;
                if (delta.Length < dt * 0.1 * ManicDiggerGameWindow.basemovespeed)
                {
                    v0.moves = false;
                }
                else
                {
                    v0.moves = true;
                    v0.dir3d = newpos - v0.pos3d;
                }
                v0.pos3d = newpos;
                //v0.progress += dt * 0.1f;
                //if (v0.progress >= 1)
                if ((v0.pos3d - v0.orders[v0.currentOrderId]).Length < 0.5f)
                {
                    v0.progress = 0;
                    v0.currentOrderId++;
                    if (v0.currentOrderId > 1)
                    {
                        v0.currentOrderId = 0;
                    }
                }
                int nextorderid = (v0.currentOrderId + 1) % v0.orders.Count;
                {
                    //v0.pos3d = v0.orders[v0.currentOrderId]
                    //    + Vector3.Multiply(v0.orders[nextorderid] - v0.orders[v0.currentOrderId], v0.progress);
                }
            }
        }
        //IGameWorld
        List<BuildOrder> orders = new List<BuildOrder>();
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
                    orders.Add(new BuildOrder()
                    {
                        playerid = player_id,
                        position = new Vector3(cmd.x, cmd.y, cmd.z),
                        mode = cmd.mode,
                        tiletype = cmd.tiletype,
                    }
                        );
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
        public bool IsTransparentTile(byte tileType)
        {
            return false;
        }
        public int PlayerBuildableMaterialType(int p)
        {
            return p;
        }
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

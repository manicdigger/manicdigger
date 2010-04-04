#region --- Using directives ---

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Media;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using System.Diagnostics;
using ManicDigger.Collisions;
using DependencyInjection;
using ManicDigger;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;

#endregion

namespace ManicDigger
{
    public interface IGameExit
    {
        bool exit { get; }
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
    public interface IGui
    {
        void DrawMap();
        void AddChatline(string s);
    }
    public class MapGeneratorPlain : IMapGenerator
    {
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public IGameData data { get; set; }
        public void GenerateMap()
        {
            for (int x = 0; x < map.MapSizeX; x++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    for (int z = 0; z < map.MapSizeZ; z++)
                    {
                        map.Map[x, y, z] = data.TileIdEmpty;
                    }
                }
            }
            for (int x = 0; x < map.MapSizeX; x++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    for (int z = 0; z < map.MapSizeZ / 2 - 1; z++)
                    {
                        map.Map[x, y, z] = data.TileIdDirt;
                    }
                    map.Map[x, y, map.MapSizeZ / 2 - 1] = data.TileIdGrass;
                }
            }
            for (int x = 0; x < 100; x++)
            {
                map.Map[x, 1, 0] = 1;
            }
            Random rnd = new Random();
            for (int i = 0; i < map.MapSizeX * map.MapSizeY * map.MapSizeZ * 0.005f; i++)
            {
                int x = rnd.Next(map.MapSizeX);
                int y = rnd.Next(map.MapSizeY);
                int z = rnd.Next(map.MapSizeZ);
                if (map.Map[x, y, z] == data.TileIdDirt)
                {
                    map.Map[x, y, z] = data.GoldTileId;
                }
            }
            //debug
            map.Map[10, 10, map.MapSizeZ / 2] = data.GoldTileId;
        }
    }
    public interface IMapStorage
    {
        byte[, ,] Map { get; set; }
        int MapSizeX { get; set; }
        int MapSizeY { get; set; }
        int MapSizeZ { get; set; }
        void LoadMapArray(Stream ms);
        void SetBlock(int x, int y, int z, byte tileType);
        float WaterLevel { get; set; }
    }
    public class Player
    {
        public Vector3 Position;
    }
    //zawiera wszystko co się niszczy przy wczytaniu z dysku/internetu nowej gry.
    public class ClientGame : IMapStorage, IPlayers
    {
        [Inject]
        public IGui gui { get; set; }
        [Inject]
        public IMapGenerator mapgenerator { get; set; }
        [Inject]
        public CharacterPhysics p { get; set; }
        byte[, ,] map;
        public object mapupdate = new object();
        public byte[, ,] Map { get { return map; } set { map = value; } }
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int,Player> Players { get { return players; } set { players = value; } }
        public ClientGame()
        {
            map = new byte[256, 256, 64];
            MapSizeX = 256;
            MapSizeY = 256;
            MapSizeZ = 64;
        }
        public void GeneratePlainMap()
        {
            mapgenerator.GenerateMap();
        }
        public const string XmlSaveExtension = ".mdxs.gz";
        public const string MinecraftMapSaveExtension = ".dat";
        public void LoadMap(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine(filename + " not found.");
            }
            if (filename.EndsWith(MinecraftMapSaveExtension))
            {
                //minecraft map
                LoadMapMinecraft(filename);
                return;
            }
            using (Stream s = new MemoryStream(GzipCompression.Decompress(File.ReadAllBytes(filename))))
            //using (FileStream s = File.OpenRead(filename))
            {
                /*
                BinaryReader br = new BinaryReader(s);
                int formatversion = br.ReadInt32();
                MapSizeZ = br.ReadInt32();
                MapSizeX = br.ReadInt32();
                MapSizeY = br.ReadInt32();
                */
                StreamReader sr = new StreamReader(s);
                XDocument d = XDocument.Load(sr);
                XElement save = d.Element("ManicDiggerSave");
                int format = int.Parse(save.Element("FormatVersion").Value);
                this.MapSizeX = int.Parse(save.Element("MapSize").Element("X").Value);
                this.MapSizeY = int.Parse(save.Element("MapSize").Element("Y").Value);
                this.MapSizeZ = int.Parse(save.Element("MapSize").Element("Z").Value);
                byte[] mapdata = Convert.FromBase64String(save.Element("MapData").Value);
                LoadMapArray(new MemoryStream(mapdata));
            }
        }
        public class XmlTool
        {
            public static string XmlVal(XmlDocument d, string path)
            {
                XPathNavigator navigator = d.CreateNavigator();
                XPathNodeIterator iterator = navigator.Select(path);
                foreach (XPathNavigator n in iterator)
                {
                    return n.Value;
                }
                return null;
            }
            public static IEnumerable<string> XmlVals(XmlDocument d, string path)
            {
                XPathNavigator navigator = d.CreateNavigator();
                XPathNodeIterator iterator = navigator.Select(path);
                foreach (XPathNavigator n in iterator)
                {
                    yield return n.Value;
                }
            }
        }
        public void LoadMapArray(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            for (int z = 0; z < MapSizeZ; z++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    for (int x = 0; x < MapSizeX; x++)
                    {
                        map[x, y, z] = br.ReadByte();
                    }
                }
            }
            gui.DrawMap();
            Console.WriteLine("Game loaded successfully.");
        }
        public string defaultminesave = "default" + XmlSaveExtension;
        public void SaveMap(string filename)
        {
            //using (FileStream s = File.OpenWrite("default.minesave"))
            /*
            MemoryStream s = new MemoryStream();
            {
                BinaryWriter bw = new BinaryWriter(s);
                bw.Write((int)0);//format version
                bw.Write((int)MapSizeZ);
                bw.Write((int)MapSizeX);
                bw.Write((int)MapSizeY);
                for (int z = 0; z < MapSizeZ; z++)
                {
                    for (int y = 0; y < MapSizeY; y++)
                    {
                        for (int x = 0; x < MapSizeX; x++)
                        {
                            bw.Write((byte)map[x, y, z]);
                        }
                    }
                }
            }
            File.WriteAllBytes("default.minesave", GzipCompression.Compress(s.ToArray()));
            */
            File.WriteAllBytes(filename, GzipCompression.Compress(SaveXml()));
            Console.WriteLine("Game saved successfully.");
        }
        static public string Base64Encode(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        static public string Base64Decode(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }
        byte[] SaveXml()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            b.AppendLine("<ManicDiggerSave>");
            b.AppendLine(X("FormatVersion", "1"));
            b.AppendLine("<MapSize>");
            b.AppendLine(X("X", "" + MapSizeX));
            b.AppendLine(X("Y", "" + MapSizeY));
            b.AppendLine(X("Z", "" + MapSizeZ));
            b.AppendLine("</MapSize>");
            MemoryStream mapdata = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mapdata);
            for (int z = 0; z < MapSizeZ; z++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    for (int x = 0; x < MapSizeX; x++)
                    {
                        bw.Write((byte)map[x, y, z]);
                    }
                }
            }
            b.AppendLine(X("MapData", Convert.ToBase64String(mapdata.ToArray())));
            //b.AppendLine(X("DefaultSpawn", ));
            b.AppendLine("</ManicDiggerSave>");
            /*
            <ManicDiggerSave>
          <SaveFormat>1</SaveFormat>
          <MapSize>
            <X>256</X>
            <Y>256</Y>
            <Z>64</Z>
          </MapSize>
          <MapData>BASE64</MapData>
          <Players>
            <Player>
              <Id>0</Id>
              <SpawnPoint>
                <X>5</X>
                <Y>5</Y>
                <Z>40</Z>
              </SpawnPoint>
            </Player>
          </Players>
        </ManicDiggerSave>
        */
            return Encoding.UTF8.GetBytes(b.ToString());
        }
        string X(string name, string value)
        {
            return string.Format("<{0}>{1}</{0}>", name, value);
        }
        [Inject]
        public IGetFilePath getfile { get; set; }
        public void LoadMapMinecraft(string filename)
        {
            byte[] serialized = GzipCompression.Decompress(new FileInfo(getfile.GetFile(filename)));
            fCraft.MapLoaderDAT maploaderdat = new fCraft.MapLoaderDAT();
            fCraft.IFMap mymap = new MyFCraftMap() { map = this };
            maploaderdat.log = new fCraft.FLogDummy();
            maploaderdat.Load(filename, mymap);
        }
        private bool IsText(byte[] serialized, int start, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (serialized[start + i] != bytes[i])
                {
                    return false;
                }
            }
            return true;
        }
        void ResizeMap(int newsizex, int newsizey, int newsizez)
        {
            byte[, ,] newmap = new byte[newsizex, newsizey, newsizez];

            int oldsizex = MapSizeX;
            int oldsizey = MapSizeY;
            int oldsizez = MapSizeZ;

            /*
            int movex = newsizex / 2 - (oldsizex) / 2;
            int movey = newsizey / 2 - (oldsizey) / 2;
            int movez = newsizez / 2 - (oldsizez) / 2;
            for (int x = 0; x < oldsizex; x++)
                for (int y = 0; y < oldsizey; y++)
                    for (int z = 0; z < oldsizez; z++)
                    {
                        //newmap[x+newsizex/4,y+newsizey/4,z+newsizez/4]
                        newmap[x + movex, y + movey, z + movez] = map[x, y, z];
                    }
            */
            CloneMap(map, newmap, new Vector3(0, 0, 0), new Vector3(256, 256, 64));
            CloneMap(map, newmap, new Vector3(256, 256, 0), new Vector3(256, 256, 64));
            CloneMap(map, newmap, new Vector3(0, 256, 0), new Vector3(256, 256, 64));
            CloneMap(map, newmap, new Vector3(256, 0, 0), new Vector3(256, 256, 64));
            map = newmap;

            MapSizeX = newsizex;
            MapSizeY = newsizey;
            MapSizeZ = newsizez;

            //DrawMap();
        }
        void CloneMap(byte[, ,] a, byte[, ,] b, Vector3 newpos, Vector3 oldsize)
        {
            for (int x = 0; x < oldsize.X; x++)
                for (int y = 0; y < oldsize.Y; y++)
                    for (int z = 0; z < oldsize.Z; z++)
                    {
                        b[x + (int)newpos.X, y + (int)newpos.Y, z + (int)newpos.Z] = a[x, y, z];
                    }
        }
        public void Dispose()
        {
        }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, byte tileType)
        {
            map[x, y, z] = tileType;
        }
        #endregion
        //float waterlevel = 32;
        #region IMapStorage Members
        //public float WaterLevel { get { return waterlevel; } set { waterlevel = value; } }
        public float WaterLevel { get { return MapSizeZ / 2; } set { } }
        #endregion
    }
    public interface IGameData
    {
        int GetTileTextureId(int tileType, TileSide side);
        byte TileIdEmpty { get; }
        byte TileIdGrass { get; }
        byte TileIdDirt { get; }
        int[] DefaultMaterialSlots { get; }
        byte GoldTileId { get; }
        int TileIdStone { get; }
        int TileIdWater { get; }
        int TileIdSand { get; }
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
        public byte TileIdEmpty { get{return (int)TileTypesManicDigger.Empty; }}
        public byte TileIdGrass { get{return (int)TileTypesManicDigger.Grass; }}
        public byte TileIdDirt { get{return (int)TileTypesManicDigger.Dirt; }}
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
            get{return TileIdDirt;}//todo
        }
        public int TileIdWater
        {
            get{return TileIdDirt;}//todo
        }
        public int TileIdSand
        {
           get{ return TileIdDirt;}//todo
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
    }
    public interface IMapGenerator
    {
        void GenerateMap();
    }
    public class GameDataTilesMinecraft : IGameData
    {
        public GameDataTilesMinecraft()
        {
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
            TileTypeMinecraft tt = (TileTypeMinecraft)tileType;
            if (tt == TileTypeMinecraft.Stone) { return 1; }
            if (tt == TileTypeMinecraft.Grass)
            {
                if (side == TileSide.Top) { return 0; }
                if (side == TileSide.Bottom) { return 2; }
                return 3;
            }
            if (tt == TileTypeMinecraft.Dirt) { return 2; }
            if (tt == TileTypeMinecraft.Cobblestone) { return (1 * 16) + 0; }
            if (tt == TileTypeMinecraft.Wood) { return 4; }
            if (tt == TileTypeMinecraft.Sapling) { return 15; }//special
            if (tt == TileTypeMinecraft.Adminium) { return (1 * 16) + 1; }
            if (tt == TileTypeMinecraft.Water) { return 14; }
            if (tt == TileTypeMinecraft.StationaryWater) { return 14; }
            if (tt == TileTypeMinecraft.Lava) { return (1 * 16) + 15; }
            if (tt == TileTypeMinecraft.StationaryLava) { return (1 * 16) + 15; }
            if (tt == TileTypeMinecraft.Sand) { return (1 * 16) + 2; }
            if (tt == TileTypeMinecraft.Gravel) { return (1 * 16) + 3; }
            if (tt == TileTypeMinecraft.GoldOre) { return (2 * 16) + 0; }
            if (tt == TileTypeMinecraft.IronOre) { return (2 * 16) + 1; }
            if (tt == TileTypeMinecraft.CoalOre) { return (2 * 16) + 2; }
            if (tt == TileTypeMinecraft.TreeTrunk)
            {
                if (side == TileSide.Top || side == TileSide.Bottom) { return (1 * 16) + 5; }
                return (1 * 16) + 4;
            }
            if (tt == TileTypeMinecraft.Leaves) { return (1 * 16) + 6; }
            if (tt == TileTypeMinecraft.Sponge) { return (3 * 16) + 0; }
            if (tt == TileTypeMinecraft.Glass) { return (3 * 16) + 1; }
            if (tt == TileTypeMinecraft.RedCloth) { return 64; }
            if (tt == TileTypeMinecraft.RedCloth + 1) { return 65; }
            if (tt == TileTypeMinecraft.RedCloth + 2) { return 66; }
            if (tt == TileTypeMinecraft.RedCloth + 3) { return 67; }
            if (tt == TileTypeMinecraft.RedCloth + 4) { return 68; }
            if (tt == TileTypeMinecraft.RedCloth + 5) { return 69; }
            if (tt == TileTypeMinecraft.RedCloth + 6) { return 70; }
            if (tt == TileTypeMinecraft.RedCloth + 7) { return 71; }
            if (tt == TileTypeMinecraft.RedCloth + 8) { return 72; }
            if (tt == TileTypeMinecraft.RedCloth + 9) { return 73; }
            if (tt == TileTypeMinecraft.RedCloth + 10) { return 74; }
            if (tt == TileTypeMinecraft.RedCloth + 11) { return 75; }
            if (tt == TileTypeMinecraft.RedCloth + 12) { return 76; }
            if (tt == TileTypeMinecraft.RedCloth + 13) { return 77; }
            if (tt == TileTypeMinecraft.RedCloth + 14) { return 78; }
            if (tt == TileTypeMinecraft.RedCloth + 15) { return 79; }//36
            if (tt == TileTypeMinecraft.YellowFlowerDecorations) { return 13; }
            if (tt == TileTypeMinecraft.RedRoseDecorations) { return 12; }
            if (tt == TileTypeMinecraft.RedMushroom) { return 28; }
            if (tt == TileTypeMinecraft.BrownMushroom) { return 29; }
            if (tt == TileTypeMinecraft.Lava) { return 30; }
            if (tt == TileTypeMinecraft.GoldBlock) { return 24; }
            if (tt == TileTypeMinecraft.IronBlock) { return 23; }
            if (tt == TileTypeMinecraft.DoubleStair) { return (0 * 16) + 5; }//43 todo
            if (tt == TileTypeMinecraft.Stair) { return 6; }//44
            if (tt == TileTypeMinecraft.TNT) { return (0 * 16) + 8; }//45
            if (tt == TileTypeMinecraft.Brick) { return (6 * 16) + 7; }//46
            if (tt == TileTypeMinecraft.Bookcase) { return (2 * 16) + 3; }//47
            if (tt == TileTypeMinecraft.MossyCobblestone) { return (2 * 16) + 4; }//48
            if (tt == TileTypeMinecraft.Obsidian) { return (2 * 16) + 5; }//49
            //torch todo
            //fire todo
            if (tt == TileTypeMinecraft.InfiniteWaterSource) { return 14; }//52
            if (tt == TileTypeMinecraft.InfiniteLavaSource) { return 30; }//53
            if (tt == TileTypeMinecraft.Chest) { return 4; }//54
            //gear todo
            //diamond todo
            //diamond block todo
            //crafting table todo
            //crops todo
            //soil todo
            //furnace todo
            //burning furnace todo
            return (int)tt;
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
            get { return (int)TileTypeMinecraft.Water; }
        }
        public int TileIdSand
        {
            get { return (int)TileTypeMinecraft.Sand; }
        }
        #endregion
    }
    public class CharacterPhysics
    {
        [Inject]
        public IMapStorage clientgame { get; set; }
        void Update()
        {
        }
        bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            if (z >= clientgame.MapSizeZ)
            {
                return true;
            }
            bool ENABLE_FREEMOVE = false;
            if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            if (x >= clientgame.MapSizeX || y >= clientgame.MapSizeY)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            return clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Empty;
        }
        float walldistance = 0.2f;
        public const float characterheight = 1.5f;
        public void Move(CharacterPhysicsState state, double dt)
        {
        }
        public Vector3 WallSlide(Vector3 oldposition, Vector3 newposition)
        {
            //Math.Floor() is needed because casting negative values to integer is not floor.
            Vector3 playerposition = newposition;
            //left
            {
                var qnewposition = newposition + new Vector3(0, 0, walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Z - oldposition.Z > 0)
                {
                    if (!newempty)
                    {
                        playerposition.Z = oldposition.Z;
                    }
                }
            }
            //front
            {
                var qnewposition = newposition + new Vector3(walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.X - oldposition.X > 0)
                {
                    if (!newempty)
                    {
                        playerposition.X = oldposition.X;
                    }
                }
            }
            //top
            {
                var qnewposition = newposition + new Vector3(0, -walldistance, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Y - oldposition.Y < 0)
                {
                    if (!newempty)
                    {
                        playerposition.Y = oldposition.Y;
                    }
                }
            }
            //right
            {
                var qnewposition = newposition + new Vector3(0, 0, -walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Z - oldposition.Z < 0)
                {
                    if (!newempty)
                    {
                        playerposition.Z = oldposition.Z;
                    }
                }
            }
            //back
            {
                var qnewposition = newposition + new Vector3(-walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.X - oldposition.X < 0)
                {
                    if (!newempty)
                    {
                        playerposition.X = oldposition.X;
                    }
                }
            }
            //bottom
            {
                var qnewposition = newposition + new Vector3(0, +walldistance + characterheight, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Y - oldposition.Y > 0)
                {
                    if (!newempty)
                    {
                        playerposition.Y = oldposition.Y;
                    }
                }
            }
            return playerposition;
        }
    }
    public interface IInternetGameFactory
    {
        void NewInternetGame();
        IClientNetwork GetNetwork();
        ClientGame GetClientGame();
        ITerrainDrawer GetTerrain();
    }
    public class CharacterPhysicsState
    {
        public float movedz = 0;
        public Vector3 playerposition = new Vector3(15.5f, 40, 15.5f);
        public Vector3 playerorientation = new Vector3((float)Math.PI, 0, 0);
    }
    public class PlayMp3 : IDisposable
    {
        private string _command;
        private bool isOpen;
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        bool closed = false;
        public void Close()
        {
            try
            {
                _command = "close MediaFile";
                mciSendString(_command, null, 0, IntPtr.Zero);
                isOpen = false;
            }
            catch
            {
                Console.WriteLine("winmm.dll problem");
            }
        }
        public void Open(string sFileName)
        {
            if (!File.Exists(sFileName))
            {
                Console.WriteLine("Music file not found: " + sFileName);
            }
            try
            {
                _command = "open \"" + sFileName + "\" type mpegvideo alias MediaFile";
                mciSendString(_command, null, 0, IntPtr.Zero);
                isOpen = true;
            }
            catch
            {
                Console.WriteLine("winmm.dll problem");
            }
        }
        public void Play(bool loop)
        {
            try
            {
                if (isOpen)
                {
                    _command = "play MediaFile";
                    if (loop)
                        _command += " REPEAT";
                    mciSendString(_command, null, 0, IntPtr.Zero);
                }
            }
            catch
            {
                Console.WriteLine("winmm.dll problem");
            }
        }
        #region IDisposable Members
        public void Dispose()
        {
            if (!closed)
            {
                Close();
                closed = true;
            }
        }
        #endregion
    }
    public class Viewport
    {
    }
    public interface IMap
    {
        //void LoadMap(byte[, ,] map);
        void SetTileAndUpdate(Vector3 pos, byte type);
    }
    public interface ILocalPlayerPosition
    {
        Vector3 LocalPlayerPosition { get; set; }
        Vector3 LocalPlayerOrientation { get; set; }
    }
    public interface IPlayers
    {
        IDictionary<int, Player> Players { get; set; }
    }
    public enum BlockSetMode
    {
        Create,
        Destroy,
    }
}
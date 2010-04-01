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
    public static class GzipCompression
    {
        public static byte[] Compress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (GZipStream compress = new GZipStream(output, CompressionMode.Compress))
            {
                byte[] buffer = new byte[4096];
                int numRead;
                while ((numRead = input.Read(buffer, 0, buffer.Length)) != 0)
                {
                    compress.Write(buffer, 0, numRead);
                }
            }
            return output.ToArray();
        }
        public static byte[] Decompress(byte[] fi)
        {
            MemoryStream ms = new MemoryStream();
            // Get the stream of the source file.
            using (MemoryStream inFile = new MemoryStream(fi))
            {
                // Get original file extension, for example "doc" from report.doc.gz.
                //string curFile = fi.FullName;
                //string origName = curFile.Remove(curFile.Length - fi.Extension.Length);

                //Create the decompressed file.
                //using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        //Copy the decompression stream into the output file.
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, numRead);
                        }
                        //Console.WriteLine("Decompressed: {0}", fi.Name);
                    }
                }
            }
            return ms.ToArray();
        }
        public static byte[] Decompress(FileInfo fi)
        {
            MemoryStream ms = new MemoryStream();
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length - fi.Extension.Length);

                //Create the decompressed file.
                //using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        //Copy the decompression stream into the output file.
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, numRead);
                        }
                        //Console.WriteLine("Decompressed: {0}", fi.Name);
                    }
                }
            }
            return ms.ToArray();
        }
    }
    public interface IAudio
    {
        void Play(string filename);
    }
    public class AudioDummy : IAudio
    {
        public void Play(string filename)
        {
        }
    }
    public interface IGameExit
    {
        bool exit { get; }
    }
    public class AudioOpenAl : IAudio
    {
        [Inject]
        public IGameExit gameexit { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        public AudioOpenAl()
        {
            try
            {
                IList<string> x = AudioContext.AvailableDevices;//only with this line an exception can be catched.
                context = new AudioContext();
            }
            catch (Exception e)
            {
                string oalinst = "oalinst.exe";
                if (File.Exists(oalinst))
                {
                    try
                    {
                        Process.Start(oalinst, "/s");
                    }
                    catch
                    {
                    }
                }
                Console.WriteLine(e);
            }
        }
        AudioContext context;
        /*
        static byte[] LoadOgg(Stream stream, out int channels, out int bits, out int rate)
        {
            byte[] bytes;
            Jarnbjo.Ogg.OggPage.Create(
            return bytes;
        }
        */
        // Loads a wave/riff audio file.
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }
        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
        class X
        {
            public X(string filename, IGameExit gameexit)
            {
                this.filename = filename;
                this.gameexit = gameexit;
            }
            IGameExit gameexit;
            public string filename;
            public void Play()
            {
                if (started)
                {
                    shouldplay = true;
                    return;
                }
                started = true;
                new Thread(play).Start();
            }
            //bool resume = true;
            bool started = false;
            //static Dictionary<string, int> audiofiles = new Dictionary<string, int>();
            void play()
            {
                try
                {
                    DoPlay();
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            private void DoPlay()
            {
                //if(!audiofiles.ContainsKey(filename))
                {

                }
                int source = AL.GenSource();
                int state;
                //using ()
                {
                    //Trace.WriteLine("Testing WaveReader({0}).ReadToEnd()", filename);

                    int buffer = AL.GenBuffer();

                    int channels, bits_per_sample, sample_rate;
                    byte[] sound_data = LoadWave(File.Open(filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
                    AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);
                    //audiofiles[filename]=buffer;

                    AL.Source(source, ALSourcei.Buffer, buffer);
                    AL.SourcePlay(source);

                    // Query the source to find out when it stops playing.
                    for (; ; )
                    {
                        AL.GetSource(source, ALGetSourcei.SourceState, out state);
                        if ((!loop) && (ALSourceState)state != ALSourceState.Playing)
                        {
                            break;
                        }
                        if (gameexit.exit)
                        {
                            break;
                        }
                        if (loop)
                        {
                            if (state == (int)ALSourceState.Playing && (!shouldplay))
                            {
                                AL.SourcePause(source);
                            }
                            if (state != (int)ALSourceState.Playing && (shouldplay))
                            {
                                AL.SourcePlay(source);
                            }
                        }
                        /*
                        if (stop)
                        {
                            AL.SourcePause(source);
                            resume = false;
                        }
                        if (resume)
                        {
                            AL.SourcePlay(source);
                            resume = false;
                        }
                        */
                        Thread.Sleep(1);
                    }
                    AL.SourceStop(source);
                    AL.DeleteSource(source);
                    AL.DeleteBuffer(buffer);
                }
            }
            public bool loop = false;
            //bool stop;
            //public void Stop()
            //{
            //    stop = true;
            //}
            public bool shouldplay;
        }
        public void Play(string filename)
        {
            if (context == null)
            {
                return;
            }
            new X(getfile.GetFile(filename), gameexit).Play();
        }
        Dictionary<string, X> soundsplaying = new Dictionary<string, X>();
        public void PlayAudioLoop(string filename, bool play)
        {
            if (context == null)
            {
                return;
            }
            filename = getfile.GetFile(filename);
            //todo: resume playing.
            if (play)
            {
                if (!soundsplaying.ContainsKey(filename))
                {
                    var x = new X(filename, gameexit);
                    x.loop = true;
                    soundsplaying[filename] = x;
                }
                soundsplaying[filename].Play();
            }
            else
            {
                if (soundsplaying.ContainsKey(filename))
                {
                    soundsplaying[filename].shouldplay = false;
                    //soundsplaying.Remove(filename);
                }
            }
        }
    }
    public interface IGetFilePath
    {
        string GetFile(string p);
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
    public class GetFilePathDummy : IGetFilePath
    {
        #region IGetFilePath Members
        public string GetFile(string p)
        {
            return p;
        }
        #endregion
    }
    public class GetFilePath : IGetFilePath
    {
        public GetFilePath()
        {
        }
        public string DataPath;
        public string GetFile(string filename)
        {
            if (!Directory.Exists("data"))
            {
                throw new Exception("data not found");
            }
            string a = Path.Combine(Path.Combine("data", DataPath), filename);
            string b = Path.Combine("data", filename);
            string c = filename;
            if (File.Exists(a))
            {
                return a;
            }
            if (File.Exists(b))
            {
                return b;
            }
            if (File.Exists(c))
            {
                return c;
            }
            throw new Exception(filename + " not found.");
        }
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
                        map.Map[x, y, z] = data.TileIdEmpty();
                    }
                }
            }
            for (int x = 0; x < map.MapSizeX; x++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    for (int z = 0; z < map.MapSizeZ / 2 - 1; z++)
                    {
                        map.Map[x, y, z] = data.TileIdDirt();
                    }
                    map.Map[x, y, map.MapSizeZ / 2 - 1] = data.TileIdGrass();
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
                if (map.Map[x, y, z] == data.TileIdDirt())
                {
                    map.Map[x, y, z] = data.GoldTileId();
                }
            }
            //debug
            map.Map[10, 10, map.MapSizeZ / 2] = data.GoldTileId();
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
    }
    //zawiera wszystko co się niszczy przy wczytaniu z dysku/internetu nowej gry.
    public class ClientGame : IMapStorage
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
    }
    public interface IGameData
    {
        int GetTileTextureId(int tileType, TileSide side);
        byte TileIdEmpty();
        byte TileIdGrass();
        byte TileIdDirt();
        int[] DefaultMaterialSlots { get; }
        byte GoldTileId();
        int TileIdStone();
        int TileIdWater();
        int TileIdSand();
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
        public byte TileIdEmpty() { return (int)TileTypesManicDigger.Empty; }
        public byte TileIdGrass() { return (int)TileTypesManicDigger.Grass; }
        public byte TileIdDirt() { return (int)TileTypesManicDigger.Dirt; }
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
        public byte GoldTileId() { return (int)TileTypesManicDigger.Gold; }
        #region IGameData Members
        public int TileIdStone()
        {
            return TileIdDirt();//todo
        }
        public int TileIdWater()
        {
            return TileIdDirt();//todo
        }
        public int TileIdSand()
        {
            return TileIdDirt();//todo
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
        public byte TileIdEmpty()
        {
            return (byte)TileTypeMinecraft.Empty;
        }
        public byte TileIdGrass()
        {
            return (byte)TileTypeMinecraft.Grass;
        }
        public byte TileIdDirt()
        {
            return (byte)TileTypeMinecraft.Dirt;
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
            if (tt == TileTypeMinecraft.Cobblestone) { return (1 * 16) + 0; }
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
            //...
            //43
            if (tt == TileTypeMinecraft.DoubleStair) { return (0 * 16) + 5; }
            if (tt == TileTypeMinecraft.Brick) { return (6 * 16) + 7; }
            if (tt == TileTypeMinecraft.Wood) { return 4; }
            if (tt == TileTypeMinecraft.Dirt) { return 2; }
            if (tt == TileTypeMinecraft.Stair) { return 5; }
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
        public byte GoldTileId()
        {
            return (int)TileTypeMinecraft.GoldOre;
        }
        #region IGameData Members
        public int TileIdStone()
        {
            return (int)TileTypeMinecraft.Stone;
        }
        public int TileIdWater()
        {
            return (int)TileTypeMinecraft.Water;
        }
        public int TileIdSand()
        {
            return (int)TileTypeMinecraft.Sand;
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
            //Vector3 oldposition = playerposition;
            Vector3 playerposition = newposition;
            //left
            {
                var qnewposition = newposition + new Vector3(0, 0, walldistance);
                bool newempty = IsTileEmptyForPhysics((int)qnewposition.X, (int)qnewposition.Z, (int)qnewposition.Y);
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
                bool newempty = IsTileEmptyForPhysics((int)qnewposition.X, (int)qnewposition.Z, (int)qnewposition.Y);
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
                bool newempty = IsTileEmptyForPhysics((int)qnewposition.X, (int)qnewposition.Z, (int)qnewposition.Y);
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
                bool newempty = IsTileEmptyForPhysics((int)qnewposition.X, (int)qnewposition.Z, (int)qnewposition.Y);
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
                bool newempty = IsTileEmptyForPhysics((int)qnewposition.X, (int)qnewposition.Z, (int)qnewposition.Y);
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
                bool newempty = IsTileEmptyForPhysics((int)qnewposition.X, (int)qnewposition.Z, (int)qnewposition.Y);
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
    public class ManicDiggerGameWindow : GameWindow, IGameExit, ILocalPlayerPosition, IGui, IMap
    {
        [Inject]
        public ClientGame clientgame { get; set; }
        [Inject]
        public IClientNetwork network { get; set; }
        [Inject]
        public IAudio audio { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public LoginClientMinecraft login { get; set; }

        bool ENABLE_BACKFACECULLING = true;
        bool ENABLE_TRANSPARENCY = true;
        bool ENABLE_MIPMAPS = true;
        bool ENABLE_VSYNC = false;

        const float rotation_speed = 180.0f * 0.05f;
        //float angle;

        struct Vbo
        {
            public int VboID, EboID, NumElements;
            public Box3D box;
        }
        //List<Vbo> vbo = new List<Vbo>();
        Dictionary<Vector3, Vbo> vbo = new Dictionary<Vector3, Vbo>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VertexPositionTexture
        {
            public Vector3 Position;
            public float u;
            public float v;

            public VertexPositionTexture(float x, float y, float z, float u, float v)
            {
                Position = new Vector3(x, y, z);
                this.u = u;
                this.v = v;
            }

            static uint ToRgba(Color color)
            {
                return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
            }
        }

        VertexPositionTexture[] CubeVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture( 0.0f,  1.0f,  0.0f, 0, 0),
            new VertexPositionTexture( 0.0f,  1.0f,  1.0f, 0, 1),
            new VertexPositionTexture( 1.0f,  1.0f,  0.0f, 1, 0),
            new VertexPositionTexture( 1.0f,  1.0f,  1.0f, 1, 1),
        };

        short[] CubeElements = new short[]
        {
            0, 1, 2, 2, 3, 0, // front face
            3, 2, 6, 6, 7, 3, // top face
            7, 6, 5, 5, 4, 7, // back face
            4, 0, 3, 3, 7, 4, // left face
            0, 1, 5, 5, 4, 0, // bottom face
            1, 5, 6, 6, 2, 1, // right face
        };
        const bool ENABLE_FULLSCREEN = false;
        public ManicDiggerGameWindow()
            : base(800, 600, GraphicsMode.Default, "",
                ENABLE_FULLSCREEN ? GameWindowFlags.Fullscreen : GameWindowFlags.Default) { }
        int LoadTexture(string filename)
        {
            Bitmap bmp = new Bitmap(filename);
            return LoadTexture(bmp);
        }
        //http://www.opentk.com/doc/graphics/textures/loading
        int LoadTexture(Bitmap bmp)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            if (!ENABLE_MIPMAPS)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); //DOES NOT WORK ON ATI GRAPHIC CARDS
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
                //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                //#if(DEBUG)
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                //#else
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearClipmapLinearSgix);
                //#endif

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            GL.Enable(EnableCap.DepthTest);
            if (ENABLE_TRANSPARENCY)
            {
                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
            }
            /*
            if (ENABLE_TRANSPARENCY)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new Color4(0, 0, 0, byte.MaxValue));
            }
            */
            return id;
        }
        PlayMp3 mp3 = new PlayMp3();
        protected override void OnFocusedChanged(EventArgs e)
        {
            if (guistate == GuiState.Normal)
            { GuiActionGoToEscapeMenu(); }
            else if (guistate == GuiState.MainMenu || guistate == GuiState.EscapeMenu)
            { }
            else { throw new Exception(); }
            base.OnFocusedChanged(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mp3.Open("data\\Tenebrous Brothers Carnival - Act One.mp3");
            mp3.Play(true);
            guistate = GuiState.MainMenu;
            FreeMouse = true;

            string version = GL.GetString(StringName.Version);
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major <= 1 && minor < 5)
            {
                //System.Windows.Forms.MessageBox.Show("You need at least OpenGL 1.5 to run this example. Aborting.", "VBOs not supported",
                //System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                this.Exit();
            }
            if (!ENABLE_VSYNC)
            {
                TargetRenderFrequency = 0;
            }
            GL.ClearColor(System.Drawing.Color.MidnightBlue);
            /*
            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogMode, 1);
            GL.Fog(FogParameter.FogStart, viewdistance);
            GL.Fog(FogParameter.FogEnd, 1);
            */
            //GL.Frustum(double.MinValue, double.MaxValue, double.MinValue, double.MaxValue, 1, 1000);
            //clientgame.GeneratePlainMap();
            //clientgame.LoadMapMinecraft();
            clientgame.LoadMap("menu" + ClientGame.XmlSaveExtension);
            ENABLE_FREEMOVE = true;
            player.playerposition = new Vector3(4.691565f, 45.2253f, 2.52523f);
            player.playerorientation = new Vector3(3.897586f, 2.385999f, 0f);
            DrawMap();
            Console.WriteLine("Hardware buffers: " + toupdate.Count);
            GL.Enable(EnableCap.Texture2D);
            terrainTexture = LoadTexture(getfile.GetFile("terrain.png"));
            Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            if (ENABLE_BACKFACECULLING)
            {
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
            }
            Keyboard.KeyRepeat = true;
            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            MaterialSlots = data.DefaultMaterialSlots;
            new Thread(bgworker).Start();
        }
        protected override void OnClosed(EventArgs e)
        {
            exit = true;
            base.OnClosed(e);
        }
        string[] soundwalk = { "walk1.wav", "walk2.wav", "walk3.wav", "walk4.wav" };
        string soundbuild = "build.wav";
        string sounddestruct = "destruct.wav";
        string soundclone = "clone.wav";
        bool exitbgworker = false;
        //ISoundPlayer soundplayer = new SoundPlayerDummy();
        /// <summary>
        /// Background thread generating vertices and indices.
        /// Actual vbo loading must be done in the main thread (it is fast).
        /// </summary>
        void bgworker()
        {
            for (; ; )
            {
                if (exit || exitbgworker)
                {
                    return;
                }
                Vector3? pp = null;
                lock (toupdate)
                {
                    if (toupdate.Count > 0)
                    {
                        pp = toupdate.Dequeue();
                    }
                }
                if (pp != null)
                {
                    Vector3 p = pp.Value;
                    //lock (clientgame.mapupdate)//does not work, clientgame can get replaced
                    {
                        try
                        {
                            VerticesIndicesToLoad q = MakeChunk((int)p.X * buffersize, (int)p.Y * buffersize, (int)p.Z * buffersize, buffersize);

                            if (q != null)
                            {
                                lock (vbotoload)
                                {
                                    vbotoload.Enqueue(q);
                                }
                            }
                        }
                        catch { }
                    }
                }
                Thread.Sleep(0);
            }
        }
        Queue<Vector3> toupdate = new Queue<Vector3>();
        public void DrawMap()
        {
            lock (toupdate)
            {
                toupdate.Clear();
            }
            lock (vbotoload)
            {
                vbotoload.Clear();
            }
            foreach (var v in vbo)
            {
                var a = v.Value.VboID;
                var b = v.Value.EboID;
                GL.DeleteBuffers(1, ref a);
                GL.DeleteBuffers(1, ref b);
            }
            vbo.Clear();
            for (int i = 0; i < 1; i++)
                for (int x = 0; x < clientgame.MapSizeX / buffersize; x++)
                    for (int y = 0; y < clientgame.MapSizeY / buffersize; y++)
                        for (int z = 0; z < clientgame.MapSizeZ / buffersize; z++)//bbb mapsizez / buffersize
                            //DrawUpdateChunk(x, y, z);
                            lock (toupdate)
                            {
                                toupdate.Enqueue(new Vector3(x, y, z));
                            }
        }
        int buffersize = 32;//32,45
        public void UpdateTileSet(Vector3 pos, byte type)
        {
            //            frametickmainthreadtodo.Add(() =>
            //        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int z = (int)pos.Z;
            clientgame.Map[x, y, z] = type;
            UpdateTile(x, y, z);
            //          });
        }
        private void UpdateTile(int x, int y, int z)
        {
            Vector3 bufferpos = new Vector3(x / buffersize, y / buffersize, z / buffersize);
            lock (toupdate)
            {
                //if we are on a chunk boundary, then update near chunks too.
                if (x % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(-1, 0, 0));
                }
                if (x % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(1, 0, 0));
                }
                if (y % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, -1, 0));
                }
                if (y % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 1, 0));
                }
                if (z % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 0, -1));
                }
                if (z % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 0, 1));
                }
                toupdate.Enqueue(bufferpos);///bbb z / buffersize
            }
        }
        void ClientCommand(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("/"))
            {
                string cmd = ss[0].Substring(1);
                string arguments;
                if (s.IndexOf(" ") == -1)
                { arguments = ""; }
                else
                { arguments = s.Substring(s.IndexOf(" ")); }
                arguments = arguments.Trim();
                if (cmd == "server" || cmd == "connect")
                {
                    string server = arguments;
                    DownloadInternetGame(username, pass, server);
                    return;
                }
                else if (cmd == "nick" || cmd == "user" || cmd == "username")
                {
                    username = arguments;
                }
                else if (cmd == "pass" || cmd == "password")
                {
                    pass = arguments;
                }
                else if (cmd == "load")
                {
                    if (arguments == "")
                    {
                        AddChatline("error: missing arg1 - savename");
                    }
                    try
                    {
                        string filename = arguments;
                        //if no extension given, then add default
                        if (filename.IndexOf(".") == -1)
                        {
                            filename += ClientGame.XmlSaveExtension;
                        }
                        clientgame.LoadMap(filename );
                    }
                    catch (Exception e) { AddChatline(new StringReader(e.ToString()).ReadLine()); }
                }
                else if (cmd == "save")
                {
                    if (arguments == "")
                    {
                        AddChatline("error: missing arg1 - savename");
                        return;
                    }
                    try
                    {
                        clientgame.SaveMap(arguments + ClientGame.XmlSaveExtension);
                    }
                    catch (Exception e) { AddChatline(new StringReader(e.ToString()).ReadLine()); }
                }
                else if (cmd == "fps")
                {
                    ENABLE_DRAWFPS = (arguments == "" || arguments == "1" || arguments == "on");
                }
                else
                {
                    network.SendChat(GuiTypingBuffer);
                }
            }
            else
            {
                network.SendChat(GuiTypingBuffer);
            }
        }
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (guistate == GuiState.Normal)
            {
                if (e.Key == OpenTK.Input.Key.Escape)
                {
                    GuiActionGoToEscapeMenu();
                }
            }
            else if (guistate == GuiState.EscapeMenu)
            {
                int menuelements = 3;
                if (e.Key==OpenTK.Input.Key.Escape)
                {
                    EscapeMenuBackToGame();
                }
                if (e.Key == OpenTK.Input.Key.Up)
                {
                    menustate.selected--;
                    menustate.selected = Math.Max(0, menustate.selected);
                }
                if (e.Key == OpenTK.Input.Key.Down)
                {
                    menustate.selected++;
                    menustate.selected = Math.Min(menuelements - 1, menustate.selected);
                }
                if (e.Key == OpenTK.Input.Key.Enter)
                {
                    EscapeMenuAction();
                }
                return;
            }
            else if (guistate == GuiState.MainMenu)
            {
                int menuelements = 3;
                if (e.Key == OpenTK.Input.Key.Escape)
                {
                    exit = true;
                    Exit();
                }
                if (e.Key == OpenTK.Input.Key.Up)
                {
                    menustate.selected--;
                    menustate.selected = Math.Max(0, menustate.selected);
                }
                if (e.Key == OpenTK.Input.Key.Down)
                {
                    menustate.selected++;
                    menustate.selected = Math.Min(menuelements - 1, menustate.selected);
                }
                if (e.Key == OpenTK.Input.Key.Enter)
                {
                    MainMenuAction();
                }
                return;
            }
            else throw new Exception();
            if (Keyboard[OpenTK.Input.Key.Escape])
            {
                guistate = GuiState.EscapeMenu;
                menustate = new MenuState();
                FreeMouse = true;
            }
            if (e.Key == OpenTK.Input.Key.Enter)
            {
                if (GuiTyping == TypingState.Typing)
                {
                    //GuiTyping = TypingState.Ready;
                    //?
                    //if (GuiTyping == TypingState.Ready)
                    {
                        ClientCommand(GuiTypingBuffer);
                        GuiTypingBuffer = "";
                        GuiTyping = TypingState.None;
                    }
                }
                else if (GuiTyping == TypingState.None)
                {
                    GuiTyping = TypingState.Typing;
                }
                else if (GuiTyping == TypingState.Ready)
                {
                    Console.WriteLine("Keyboard_KeyDown ready");
                }
                return;
            }
            if (GuiTyping == TypingState.Typing)
            {
                var key = e.Key;
                string c = "";
                if (key == OpenTK.Input.Key.BackSpace)
                {
                    if (GuiTypingBuffer.Length > 0)
                    {
                        GuiTypingBuffer = GuiTypingBuffer.Substring(0, GuiTypingBuffer.Length - 1);
                    }
                    return;
                }
                if (Keyboard[OpenTK.Input.Key.ControlLeft] || Keyboard[OpenTK.Input.Key.ControlRight])
                {
                    if (key == OpenTK.Input.Key.V)
                    {
                        if (Clipboard.ContainsText())
                        {
                            GuiTypingBuffer += Clipboard.GetText();
                        }
                        return;
                    }
                }
                if (key == OpenTK.Input.Key.Q) { c += "q"; }
                if (key == OpenTK.Input.Key.W) { c += "w"; }
                if (key == OpenTK.Input.Key.E) { c += "e"; }
                if (key == OpenTK.Input.Key.R) { c += "r"; }
                if (key == OpenTK.Input.Key.T) { c += "t"; }
                if (key == OpenTK.Input.Key.Y) { c += "y"; }
                if (key == OpenTK.Input.Key.U) { c += "u"; }
                if (key == OpenTK.Input.Key.I) { c += "i"; }
                if (key == OpenTK.Input.Key.O) { c += "o"; }
                if (key == OpenTK.Input.Key.P) { c += "p"; }

                if (key == OpenTK.Input.Key.A) { c += "a"; }
                if (key == OpenTK.Input.Key.S) { c += "s"; }
                if (key == OpenTK.Input.Key.D) { c += "d"; }
                if (key == OpenTK.Input.Key.F) { c += "f"; }
                if (key == OpenTK.Input.Key.G) { c += "g"; }
                if (key == OpenTK.Input.Key.H) { c += "h"; }
                if (key == OpenTK.Input.Key.J) { c += "j"; }
                if (key == OpenTK.Input.Key.K) { c += "k"; }
                if (key == OpenTK.Input.Key.L) { c += "l"; }

                if (key == OpenTK.Input.Key.Z) { c += "z"; }
                if (key == OpenTK.Input.Key.X) { c += "x"; }
                if (key == OpenTK.Input.Key.C) { c += "c"; }
                if (key == OpenTK.Input.Key.V) { c += "v"; }
                if (key == OpenTK.Input.Key.B) { c += "b"; }
                if (key == OpenTK.Input.Key.N) { c += "n"; }
                if (key == OpenTK.Input.Key.M) { c += "m"; }

                if (key == OpenTK.Input.Key.Comma) { c += ","; }
                if (key == OpenTK.Input.Key.Period) { c += "."; }
                if (key == OpenTK.Input.Key.Number0) { c += "0"; }
                if (key == OpenTK.Input.Key.Number1) { c += "1"; }
                if (key == OpenTK.Input.Key.Number2) { c += "2"; }
                if (key == OpenTK.Input.Key.Number3) { c += "3"; }
                if (key == OpenTK.Input.Key.Number4) { c += "4"; }
                if (key == OpenTK.Input.Key.Number5) { c += "5"; }
                if (key == OpenTK.Input.Key.Number6) { c += "6"; }
                if (key == OpenTK.Input.Key.Number7) { c += "7"; }
                if (key == OpenTK.Input.Key.Number8) { c += "8"; }
                if (key == OpenTK.Input.Key.Number9) { c += "9"; }
                if (key == OpenTK.Input.Key.BackSlash) { c += "\\"; }
                if (key == OpenTK.Input.Key.Slash) { c += "/"; }
                if (key == OpenTK.Input.Key.Plus) { c += "+"; }
                if (key == OpenTK.Input.Key.Minus) { c += "-"; }
                if (key == OpenTK.Input.Key.Space) { c += " "; }
                if (Keyboard[OpenTK.Input.Key.ShiftLeft] || Keyboard[OpenTK.Input.Key.ShiftRight])
                {
                    c = c.ToUpper();
                    if (c == "1") { c = "!"; }
                    if (c == "2") { c = "@"; }
                    if (c == "3") { c = "#"; }
                    if (c == "4") { c = "$"; }
                    if (c == "5") { c = "%"; }
                    if (c == "6") { c = "^"; }
                    if (c == "7") { c = "&"; }
                    if (c == "8") { c = "*"; }
                    if (c == "9") { c = "("; }
                    if (c == "0") { c = ")"; }
                    if (c == "-") { c = "_"; }
                    if (c == "=") { c = "+"; }
                }
                GuiTypingBuffer += c;
                return;
            }
            if (e.Key == OpenTK.Input.Key.F1)
            {
                movespeed = basemovespeed * 1;
            }
            if (e.Key == OpenTK.Input.Key.F2)
            {
                movespeed = basemovespeed * 10;
            }
            if (e.Key == OpenTK.Input.Key.F7)
            {
                GuiActionLoadGame();
            }
            if (e.Key == OpenTK.Input.Key.F5)
            {
                clientgame.SaveMap(clientgame.defaultminesave);
            }
            if (e.Key == OpenTK.Input.Key.F8)
            {
                GuiActionGenerateNewMap();
            }
            if (e.Key == OpenTK.Input.Key.F9)
            {
                DownloadInternetGame(username, pass, testgameurl);
            }
            if (e.Key == OpenTK.Input.Key.M)
            {
                FreeMouse = !FreeMouse;
                mouse_delta = new Point(0, 0);
                if (!FreeMouse)
                {
                    freemousejustdisabled = true;
                }
            }
            if (e.Key == OpenTK.Input.Key.F)
            {
                ENABLE_FREEMOVE = !ENABLE_FREEMOVE;
            }
            if (e.Key == OpenTK.Input.Key.N)
            {
                ENABLE_NOCLIP = !ENABLE_NOCLIP;
            }
            if (e.Key == OpenTK.Input.Key.R)
            {
                player.playerposition = playerpositionspawn;
                player.movedz = 0;
            }
            if (e.Key == OpenTK.Input.Key.Number1) { activematerial = 0; }
            if (e.Key == OpenTK.Input.Key.Number2) { activematerial = 1; }
            if (e.Key == OpenTK.Input.Key.Number3) { activematerial = 2; }
            if (e.Key == OpenTK.Input.Key.Number4) { activematerial = 3; }
            if (e.Key == OpenTK.Input.Key.Number5) { activematerial = 4; }
            if (e.Key == OpenTK.Input.Key.Number6) { activematerial = 5; }
            if (e.Key == OpenTK.Input.Key.Number7) { activematerial = 6; }
            if (e.Key == OpenTK.Input.Key.Number8) { activematerial = 7; }
            if (e.Key == OpenTK.Input.Key.Number9) { activematerial = 8; }
            if (e.Key == OpenTK.Input.Key.Number0) { activematerial = 9; }
        }
        private void GuiActionGoToEscapeMenu()
        {
            guistate = GuiState.EscapeMenu;
            menustate = new MenuState();
            FreeMouse = true;
        }
        private void GuiActionLoadGame()
        {
            clientgame.LoadMap(clientgame.defaultminesave);
        }
        private void EscapeMenuBackToGame()
        {
            guistate = GuiState.Normal;
            FreeMouse = false;
            ENABLE_FREEMOVE = false;
            freemousejustdisabled = true;
        }
        private void GuiActionGenerateNewMap()
        {
            clientgame.GeneratePlainMap();
            player.playerposition = playerpositionspawn;
            DrawMap();
        }
        bool freemousejustdisabled;
        enum TypingState { None, Typing, Ready };
        TypingState GuiTyping = TypingState.None;
        string GuiTypingBuffer = "";
        IClientNetwork newnetwork;
        ClientGame newclientgame;

        string username = "gamer1";
        string pass = "12345";
        string testgameurl
        {
            get
            {
                return File.ReadAllText("defaultserver.cfg");
            }
        }
        [Inject]
        public IInternetGameFactory internetgamefactory { get; set; }
        private void DownloadInternetGame(string qusername, string qpass, string qgameurl)
        {
            var oldclientgame = clientgame;
            var oldnetwork = network;
            internetgamefactory.NewInternetGame();
            newclientgame = internetgamefactory.GetClientGame();
            newnetwork = internetgamefactory.GetNetwork();

            oldclientgame.Dispose();
            newnetwork.MapLoaded += new EventHandler<MapLoadedEventArgs>(network_MapLoaded);

            oldnetwork.Dispose();

            new MethodInvoker(() =>
            {
                LoginData logindata = login.Login(qusername, qpass, qgameurl);
                frametickmainthreadtodo.Add(
                    () =>
                    {
                        newnetwork.Connect(logindata.serveraddress, logindata.port, username, logindata.mppass);
                    }
                );
            }).BeginInvoke(null, null);
        }
        List<MethodInvoker> frametickmainthreadtodo = new List<MethodInvoker>();
        void network_MapLoaded(object sender, MapLoadedEventArgs e)
        {
            //frametickmainthreadtodo.Add(
            //() =>
            {
                this.network = newnetwork;
                this.clientgame = newclientgame;
                newnetwork = null; newclientgame = null;
                var ee = (MapLoadedEventArgs)e;
                lock (clientgame.mapupdate)
                {
                    clientgame.Map = ee.map;
                    clientgame.MapSizeX = ee.map.GetUpperBound(0) + 1;
                    clientgame.MapSizeY = ee.map.GetUpperBound(1) + 1;
                    clientgame.MapSizeZ = ee.map.GetUpperBound(2) + 1;
                    Console.WriteLine("Game loaded successfully.");
                    DrawMap();
                }
            }
            //);
        }
        void maploaded()
        {
        }
        int[] MaterialSlots;
        //warning! buffer zone!
        RectangleF TextureCoords(int textureId, int texturesPacked)
        {
            float bufferRatio = 0.0f;//0.1
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPacked * (int)(textureId / texturesPacked)) + ((bufferRatio) * (1.0f / texturesPacked));
            r.X = (1.0f / texturesPacked * (textureId % texturesPacked)) + ((bufferRatio) * (1.0f / texturesPacked));
            r.Width = (1f - 2f * bufferRatio) * 1.0f / texturesPacked;
            r.Height = (1f - 2f * bufferRatio) * 1.0f / texturesPacked;
            return r;
        }
        bool IsTileEmptyForDrawing(int x, int y, int z)
        {
            if (!IsValidPos(x, y, z))
            {
                return true;
            }
            return clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Empty;
        }
        bool IsTileEmptyForDrawingOrTransparent(int x, int y, int z)
        {
            if (!ENABLE_TRANSPARENCY)
            {
                return IsTileEmptyForDrawing(x, y, z);
            }
            if (!IsValidPos(x, y, z))
            {
                return true;
            }
            return clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Empty
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Water
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Glass
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.InfiniteWaterSource
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Leaves;
        }
        int texturesPacked = 16;//16x16
        bool DONOTDRAWEDGES = true;
        VerticesIndicesToLoad MakeChunk(int startx, int starty, int startz, int size)
        {
            List<ushort> myelements = new List<ushort>();
            List<VertexPositionTexture> myvertices = new List<VertexPositionTexture>();
            for (int x = startx; x < startx + size; x++)
                for (int y = starty; y < starty + size; y++)
                    for (int z = startz; z < startz + size; z++)//bbb startz+size
                    {
                        if (IsTileEmptyForDrawing(x, y, z)) { continue; }
                        bool drawtop = IsTileEmptyForDrawingOrTransparent(x, y, z + 1);
                        bool drawbottom = IsTileEmptyForDrawingOrTransparent(x, y, z - 1);
                        bool drawfront = IsTileEmptyForDrawingOrTransparent(x - 1, y, z);
                        bool drawback = IsTileEmptyForDrawingOrTransparent(x + 1, y, z);
                        bool drawleft = IsTileEmptyForDrawingOrTransparent(x, y - 1, z);
                        bool drawright = IsTileEmptyForDrawingOrTransparent(x, y + 1, z);
                        if (DONOTDRAWEDGES)
                        {
                            //if the game is fillrate limited, then this makes it much faster.
                            //(39fps vs vsync 75fps)
                            //bbb.
                            if (z == 0) { drawbottom = false; }
                            if (x == 0) { drawfront = false; }
                            if (x == 256 - 1) { drawback = false; }
                            if (y == 0) { drawleft = false; }
                            if (y == 256 - 1) { drawright = false; }
                        }
                        //top
                        if (drawtop)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Top);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 0.0f, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 1.0f, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 0.0f, texrec.Right, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 1.0f, texrec.Right, texrec.Bottom));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //bottom - same as top, but z is 1 less.
                        if (drawbottom)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Bottom);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 0.0f, texrec.Right, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 1.0f, texrec.Right, texrec.Bottom));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //front
                        if (drawfront)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Front);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //back - same as front, but x is 1 greater.
                        if (drawback)
                        {//todo fix tcoords
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Back);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        if (drawleft)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Left);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //right - same as left, but y is 1 greater.
                        if (drawright)
                        {//todo fix tcoords
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Right);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                    }
            if (myelements.Count == 0)
            {
                //yield break;
                return null;
            }
            if (myvertices.Count > ushort.MaxValue)
            {
                //throw new Exception();//aaa
            }
            var a = myelements.ToArray();
            var b = myvertices.ToArray();
            //return LoadVBO(b, a);
            return new VerticesIndicesToLoad()
            {
                position = new Vector3(startx / size, starty / size, startz / size),
                indices = a,
                vertices = b
            };
        }
        int terrainTexture;
        bool ENABLE_ZFAR = false;
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
            Set3dProjection();
        }
        private void Set3dProjection()
        {
            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, znear, zfar);
            //Matrix4 perpective = Matrix4.CreateOrthographic(800 * 0.10f, 600 * 0.10f, 0.0001f, zfar);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }
        float znear = 0.01f;
        float zfar { get { return ENABLE_ZFAR ? viewdistance * 3f / 4 : 99999; } }
        //int z = 0;
        Vector3 up = new Vector3(0f, 1f, 0f);
        Point mouse_current, mouse_previous;
        Point mouse_delta;
        bool FreeMouse = false;
        void UpdateMousePosition()
        {
            mouse_current = System.Windows.Forms.Cursor.Position;
            if (freemousejustdisabled)
            {
                mouse_previous = mouse_current;
                freemousejustdisabled = false;
            }
            if (FreeMouse)
            {
                System.Windows.Forms.Cursor.Hide();
                mouse_current.Offset(-X, -Y);
                mouse_current.Offset(0, -20);
                //System.Windows.Forms.Cursor.Show();
                return;
            }
            int centerx = Bounds.Left + (Bounds.Width / 2);
            int centery = Bounds.Top + (Bounds.Height / 2);
                        
            mouse_delta = new Point(mouse_current.X - mouse_previous.X,
                mouse_current.Y - mouse_previous.Y);
            mouse_previous = mouse_current;

            if ((Math.Abs(System.Windows.Forms.Cursor.Position.X - centerx) > 100)
                || (Math.Abs(System.Windows.Forms.Cursor.Position.Y - centery) > 100))
            {
                System.Windows.Forms.Cursor.Position =
                    new Point(centerx, centery);
                mouse_previous = new Point(centerx, centery);
            }
        }
        Vector3 playerpositionspawn = new Vector3(15.5f, 64, 15.5f);
        public Vector3 toVectorInFixedSystem1(float dx, float dy, float dz)
        {
            //Don't calculate for nothing ...
            if (dx == 0.0f & dy == 0.0f && dz == 0.0f)
                return new Vector3();

            //Convert to Radian : 360° = 2PI
            double xRot = player.playerorientation.X;//Math.toRadians(orientation.X);
            double yRot = player.playerorientation.Y;//Math.toRadians(orientation.Y);

            //Calculate the formula
            float x = (float)(dx * Math.Cos(yRot) + dy * Math.Sin(xRot) * Math.Sin(yRot) - dz * Math.Cos(xRot) * Math.Sin(yRot));
            float y = (float)(+dy * Math.Cos(xRot) + dz * Math.Sin(xRot));
            float z = (float)(dx * Math.Sin(yRot) - dy * Math.Sin(xRot) * Math.Cos(yRot) + dz * Math.Cos(xRot) * Math.Cos(yRot));

            //Return the vector expressed in the global axis system
            return new Vector3(x, y, z);
        }
        float rotationspeed = 0.15f;
        float movespeed = basemovespeed;
        float fallspeed { get { return movespeed / 10; } }
        const float basemovespeed = 5f;
        DateTime lastbuild = new DateTime();
        class VerticesIndicesToLoad
        {
            public VertexPositionTexture[] vertices;
            public ushort[] indices;
            public Vector3 position;
        }
        Queue<VerticesIndicesToLoad> vbotoload = new Queue<VerticesIndicesToLoad>();
        public bool exit { get; set; }
        float walksoundtimer = 0;
        int lastwalksound = 0;
        float stepsoundduration = 0.4f;
        void UpdateWalkSound(double dt)
        {
            walksoundtimer += (float)dt;
            if (walksoundtimer >= stepsoundduration || dt == -1)
            {
                walksoundtimer = 0;
                lastwalksound++;
                if (lastwalksound >= soundwalk.Length)
                {
                    lastwalksound = 0;
                }
                if (rnd.Next(100) > 30)
                {
                    lastwalksound = rnd.Next(soundwalk.Length);
                }
                audio.Play(soundwalk[lastwalksound]);
            }
        }
        bool IsInLeft(Vector3 player_yy, Vector3 tile_yy)
        {
            return (int)player_yy.X == (int)tile_yy.X && (int)player_yy.Z == (int)tile_yy.Z;
        }
        //float fix = 0.5f;

        float jumpacceleration = 0;
        bool ENABLE_FREEMOVE = false;
        bool ENABLE_NOCLIP = false;
        float gravity = 0.3f;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            //UpdateFrame(e);
        }
        public enum LockY
        {
            True,
            False,
        }
        CharacterPhysicsState player = new CharacterPhysicsState();
        void FrameTick(FrameEventArgs e)
        {
            lock (frametickmainthreadtodo)
            {
                for (int i = 0; i < frametickmainthreadtodo.Count; i++)
                {
                    frametickmainthreadtodo[i].Invoke();
                }
                frametickmainthreadtodo.Clear();
            }
            UpdateCharacters((float)e.Time);
            network.Process();
            if (newnetwork != null)
            {
                newnetwork.Process();
            }
            UpdateMousePosition();
            int movedx = 0;
            int movedy = 0;
            if (guistate == GuiState.Normal)
            {
                if (GuiTyping == TypingState.None)
                {
                    if (Keyboard[OpenTK.Input.Key.W]) { movedy += 1; }
                    if (Keyboard[OpenTK.Input.Key.S]) { movedy += -1; }
                    if (Keyboard[OpenTK.Input.Key.A]) { movedx += -1; }
                    if (Keyboard[OpenTK.Input.Key.D]) { movedx += 1; }
                }
                if (ENABLE_FREEMOVE)
                {
                    if (GuiTyping == TypingState.None && Keyboard[OpenTK.Input.Key.Space])
                    {
                        player.playerposition.Y += movespeed * (float)e.Time;
                    }
                }
            }
            else if (guistate == GuiState.EscapeMenu)
            {
            }
            else if (guistate == GuiState.MainMenu)
            {
            }
            else throw new Exception();

            if (!ENABLE_FREEMOVE)
            {
                player.movedz += -gravity;//gravity
            }
            Vector3 newposition = player.playerposition + toVectorInFixedSystem1
                (movedx * movespeed * (float)e.Time,
                0,
                movedy * movespeed * (float)e.Time);
            if (!ENABLE_FREEMOVE)
            {
                newposition.Y = player.playerposition.Y;
                //fast move when looking at the ground.
                var diff = newposition - player.playerposition;
                if (diff.Length > 0)
                {
                    diff.Normalize();
                }
                newposition = player.playerposition + diff * (float)e.Time * movespeed;
            }
            newposition.Y += player.movedz * (float)e.Time;
            Vector3 previousposition = player.playerposition;
            if (!ENABLE_NOCLIP)
            {
                player.playerposition = clientgame.p.WallSlide(player.playerposition, newposition);
            }
            else
            {
                player.playerposition = newposition;
            }
            bool isplayeronground;
            if (!ENABLE_FREEMOVE)
            {
                isplayeronground = player.playerposition.Y == previousposition.Y;
                {
                    if (GuiTyping == TypingState.None && Keyboard[OpenTK.Input.Key.Space] && isplayeronground && jumpacceleration <= 0)
                    {
                        jumpacceleration = 2.1f * gravity;
                        UpdateWalkSound(-1);
                    }
                    if (jumpacceleration < 0)
                    {
                        jumpacceleration = 0;
                        player.movedz = 0;
                    }
                    if (jumpacceleration > 0)
                    {
                        jumpacceleration -= (float)e.Time * 2.5f;
                    }
                    player.movedz += jumpacceleration * 2;
                    if (isplayeronground)
                    {
                        player.movedz = Math.Max(0, player.movedz);
                    }
                }
            }
            else
            {
                isplayeronground = true;
            }
            if (isplayeronground && movedx != 0 || movedy != 0)
            {
                UpdateWalkSound(e.Time);
            }
            if (!FreeMouse)
            {
                UpdateMouseViewportControl(e);
            }
        }
        class MenuState
        {
            public int selected = 0;
        }
        MenuState menustate = new MenuState();
        private void UpdateMouseViewportControl(FrameEventArgs e)
        {
            player.playerorientation.Y += (float)mouse_delta.X * rotationspeed * (float)e.Time;
            player.playerorientation.X += (float)mouse_delta.Y * rotationspeed * (float)e.Time;
            player.playerorientation.X = Clamp(player.playerorientation.X, (float)Math.PI / 2 + 0.001f, (float)(Math.PI / 2 + Math.PI - 0.001f));
            if (iii++ % 2 == 0) UpdatePicking();
        }
        int iii = 0;
        bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            if (z >= clientgame.MapSizeZ)
            {
                return true;
            }
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
        float PICK_DISTANCE = 3.5f;
        private void UpdatePicking()
        {
            float unit_x = 0;
            float unit_y = 0;
            int NEAR = 1;
            int FOV = 600;
            float ASPECT = 640f / 480;
            float near_height = NEAR * (float)(Math.Tan(FOV * Math.PI / 360.0));
            Vector3 ray = new Vector3(unit_x * near_height * ASPECT, unit_y * near_height, 1);//, 0);
            Vector3 ray_start_point = new Vector3(0.0f, 0.0f, 0.0f);//, 1.0f);
            Matrix4 the_modelview;
            //Read the current modelview matrix into the array the_modelview

            GL.GetFloat(GetPName.ModelviewMatrix, out the_modelview);
            the_modelview.Invert();
            //the_modelview = new Matrix4();
            ray = Vector3.Transform(ray, the_modelview);
            ray_start_point = Vector3.Transform(ray_start_point, the_modelview);

            var pick = new Line3D();
            var raydir = -(ray - ray_start_point);
            raydir.Normalize();
            raydir = Vector3.Multiply(raydir, 100);
            pick.Start = ray + Vector3.Multiply(raydir, 0.01f); //do not pick behind
            pick.End = ray + raydir;
            var s = new TileOctreeSearcher();
            s.StartBox = new Box3D(0, 0, 0, 256);
            List<TilePosSide> pick2 = new List<TilePosSide>(s.LineIntersection(IsTileEmptyForPhysics, pick));
            pick2.Sort((a, b) => { return (a.pos - player.playerposition).Length.CompareTo((b.pos - player.playerposition).Length); });

            bool left = Mouse[OpenTK.Input.MouseButton.Left];//destruct
            bool middle = Mouse[OpenTK.Input.MouseButton.Middle];//clone material as active
            bool right = Mouse[OpenTK.Input.MouseButton.Right];//build
            TilePosSide pick0;
            if (pick2.Count > 0 && (pick2[0].pos - player.playerposition).Length <= PICK_DISTANCE
                && IsTileEmptyForPhysics((int)ToMapPos(player.playerposition).X,
                (int)ToMapPos(player.playerposition).Y, (int)ToMapPos(player.playerposition).Z))
            {
                pickcubepos = From3dPos(pick2[0]);
                pickcubepos = new Vector3((int)pickcubepos.X, (int)pickcubepos.Y, (int)pickcubepos.Z);
                pick0 = pick2[0];
            }
            else
            {
                pickcubepos = new Vector3(-1, -1, -1);
                pick0.pos = new Vector3(-1, -1, -1);
                pick0.side = TileSide.Front;
            }
            if ((DateTime.Now - lastbuild).TotalSeconds >= BuildDelay)
            {
                if (left && !fastclicking)
                {
                    //todo animation
                    fastclicking = false;
                }
                if (left || right || middle)
                {
                    lastbuild = DateTime.Now;
                }
                if (pick2.Count > 0)
                {
                    if (middle)
                    {
                        var newtile = From3dPos(pick0);
                        if (IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            int clonesource = clientgame.Map[(int)newtile.X, (int)newtile.Z, (int)newtile.Y];
                            clonesource = (int)PlayerBuildableMaterialType((TileTypeMinecraft)clonesource);
                            for (int i = 0; i < MaterialSlots.Length; i++)
                            {
                                if ((int)MaterialSlots[i] == clonesource)
                                {
                                    activematerial = i;
                                    goto done;
                                }
                            }
                            MaterialSlots[activematerial] = clonesource;
                        done:
                            audio.Play(soundclone);
                        }
                    }
                    if (left || right)
                    {
                        TilePosSide tile = pick0;
                        Console.Write(tile.pos + ":" + Enum.GetName(typeof(TileSide), tile.side));
                        Vector3 newtile = right ? tile.Translated() : From3dPos(tile);
                        Console.WriteLine(". newtile:" + newtile);
                        if (IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            if (pick0.pos != new Vector3(-1, -1, -1))
                            {
                                audio.Play(left ? sounddestruct : soundbuild);
                            }
                            if (!right)
                            {
                                StartParticleEffect(newtile);//must be before deletion - gets ground type.
                            }
                            network.SendSetBlock(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
                                right ? BlockSetMode.Create : BlockSetMode.Destroy, (byte)MaterialSlots[activematerial]);
                        }
                    }
                }
            }
            fastclicking = false;
            if (!(left || right || middle))
            {
                lastbuild = new DateTime();
                fastclicking = true;
            }
        }
        private TileTypeMinecraft PlayerBuildableMaterialType(TileTypeMinecraft t)
        {
            if (t == TileTypeMinecraft.Grass)
            {
                return TileTypeMinecraft.Dirt;
            }
            if (t == TileTypeMinecraft.Water || t == TileTypeMinecraft.Lava) //...
            {
                return TileTypeMinecraft.Dirt;
            }
            return t;
        }
        float BuildDelay = 0.95f * (1 / basemovespeed);
        Vector3 ToMapPos(Vector3 a)
        {
            return new Vector3((int)a.X, (int)a.Z, (int)a.Y);
        }
        bool fastclicking = false;
        Vector3 pickcubepos;
        float viewdistance = 256;
        //double currentTime = 0;
        double accumulator = 0;
        double t = 0;
        //Vector3 oldplayerposition;
        float characterheight { get { return CharacterPhysics.characterheight; } }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Application.DoEvents();
            //Console.WriteLine("pos:" + player.playerposition);
            //Console.WriteLine("orientation:" + player.playerorientation);

            var deltaTime = e.Time;

            accumulator += deltaTime;
            double dt = 1d / 75;

            while (accumulator >= dt)
            {
                FrameTick(new FrameEventArgs(dt));
                t += dt;
                accumulator -= dt;
            }

            //const float alpha = accumulator / dt;
            //Vector3 currentPlayerPosition = currentState * alpha + previousState * (1.0f - alpha);
            UpdateTitleFps(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);

            GL.MatrixMode(MatrixMode.Modelview);
            Vector3 forward = toVectorInFixedSystem1(0, 0, 1);
            Matrix4 camera = Matrix4.LookAt(player.playerposition + new Vector3(0, characterheight, 0),
                player.playerposition + new Vector3(0, characterheight, 0) + forward, up);
            GL.LoadMatrix(ref camera);
            chunkupdateframecounter += vboupdatesperframe;
            while (chunkupdateframecounter >= 1)
            {
                chunkupdateframecounter -= 1;
                VerticesIndicesToLoad v = null;
                lock (vbotoload)
                {
                    if (vbotoload.Count > 0)
                    {
                        v = vbotoload.Dequeue();
                    }
                }
                if (v != null)
                {
                    Vbo vbo1 = LoadVBO(v.vertices, v.indices);
                    foreach (var vv in v.vertices)
                    {
                        vbo1.box.AddPoint(vv.Position.X, vv.Position.Y, vv.Position.Z);
                    }
                    vbo[v.position] = vbo1;
                    //DrawUpdateChunk(((int)v.X), ((int)v.Y), ((int)v.Z));
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
            var z = new List<Vbo>(VisibleVbo());
            z.Sort(f);
            foreach (var k in z)
            {
                Draw(k);
            }
            DrawImmediateParticleEffects(e.Time);
            DrawCubeLines(pickcubepos);

            DrawVehicles();

            Draw2d();

            //OnResize(new EventArgs());
            SwapBuffers();
        }
        class Chatline
        {
            public string text;
            public DateTime time;
        }
        List<Chatline> chatlines = new List<Chatline>();
        class Character
        {
            public Vector3 pos3d;
            public List<Vector3> orders = new List<Vector3>();
            public float progress;
            public int currentOrderId = 0;
            public int cargoAmount = 0;
        }
        Dictionary<string, int> textures = new Dictionary<string, int>();
        Character v0;
        void UpdateCharacters(float dt)
        {
            if (v0 == null)
            {
                v0 = new Character();
                v0.orders = new List<Vector3>();
                v0.orders.Add(new Vector3(0, 32, 0));
                v0.orders.Add(new Vector3(16, 32, 0));
                v0.pos3d = playerpositionspawn;
            }
            var dir = (v0.orders[v0.currentOrderId] - v0.pos3d);
            dir.Normalize();
            var newpos = v0.pos3d + Vector3.Multiply(dir, dt * basemovespeed);
            //Console.Write(v0.pos3d);
            v0.pos3d = clientgame.p.WallSlide(v0.pos3d, newpos);
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
        void DrawVehicles()
        {
            if (v0 != null)
                DrawCube(v0.pos3d);
        }
        int f(Vbo a, Vbo b)
        {
            var aa = (a.box.Center() - player.playerposition).Length;
            var bb = (b.box.Center() - player.playerposition).Length;
            return aa.CompareTo(bb);
        }
        void EscapeMenuAction()
        {
            if (menustate.selected == 0)
            {
                //GuiActionGenerateNewMap();
                EscapeMenuBackToGame();
            }
            else if (menustate.selected == 1)
            {
                GuiActionSaveGame();
                EscapeMenuBackToGame();
            }
            else if (menustate.selected == 2)
            {
                exit = true;
                this.Exit();
            }
            else throw new Exception();
        }
        private void GuiActionSaveGame()
        {
            clientgame.SaveMap(clientgame.defaultminesave);
        }
        void MainMenuAction()
        {
            if (menustate.selected == 0)
            {
                GuiActionGenerateNewMap();
                mp3.Close();
                mp3 = new PlayMp3();
                mp3.Open("data\\Atlantean Twilight.mp3");
                mp3.Play(true);
                EscapeMenuBackToGame();
            }
            else if (menustate.selected == 1)
            {
                if (SaveGameExists())
                {
                    GuiActionLoadGame();
                    EscapeMenuBackToGame();
                    mp3.Close();
                }
            }
            else if (menustate.selected == 2)
            {
                exit = true;
                this.Exit();
            }
            else throw new Exception();
        }
        void DrawEscapeMenu()
        {
            string newgame = "Return to game";
            string save = "Save";
            string exitstr = "Exit";
            int starty = 200;
            int textheight = 50;
            int fontsize = 20;
            if (guistate == GuiState.EscapeMenu)
            {
                Draw2dText(newgame, xcenter(TextSize(newgame, fontsize).Width), starty, fontsize, menustate.selected == 0 ? Color.Red : Color.White);
                Draw2dText(save, xcenter(TextSize(save, fontsize).Width), starty + textheight * 1, 20, menustate.selected == 1 ? Color.Red : Color.White);
                Draw2dText(exitstr, xcenter(TextSize(exitstr, fontsize).Width), starty + textheight * 2, 20, menustate.selected == 2 ? Color.Red : Color.White);
                DrawMouseCursor();
            }
        }
        bool SaveGameExists()
        {
            return File.Exists(clientgame.defaultminesave);
        }
        bool? savegameexists;
        void DrawMainMenu()
        {
            string newgame = "New single-player game";
            string loadgame = "Load game";
            string exitstr = "Exit";
            int fontsize = 20;
            int starty = 300;
            int textheight = 50;
            if (savegameexists == null) { savegameexists = SaveGameExists(); }
            if (guistate == GuiState.MainMenu)
            {
                Draw2dBitmapFile("manicdigger.png", xcenter(565), 50, 565, 119);
                Draw2dText(newgame, xcenter(TextSize(newgame, fontsize).Width), starty, fontsize, menustate.selected == 0 ? Color.Red : Color.White);
                Draw2dText(loadgame, xcenter(TextSize(loadgame, fontsize).Width), starty + textheight * 1, fontsize,
                    savegameexists.Value ?
                    (menustate.selected == 1 ? Color.Red : Color.White)
                    : (menustate.selected == 1 ? Color.Red : Color.Gray));
                Draw2dText(exitstr, xcenter(TextSize(exitstr, fontsize).Width), starty + textheight * 2, 20, menustate.selected == 2 ? Color.Red : Color.White);
                DrawMouseCursor();
            }
        }
        GuiState guistate;
        enum GuiState
        {
            Normal,
            EscapeMenu,
            MainMenu,
        }
        private void DrawMouseCursor()
        {
            Draw2dBitmapFile("gui\\mousecursor.png", mouse_current.X, mouse_current.Y, 30, 30);
        }
        int chatfontsize = 12;
        private void Draw2d()
        {
            OrthoMode();
            if (guistate == GuiState.Normal)
            {
                float targetwidth = Width / 20;
                float targetheight = Height / 20;
                Draw2dBitmapFile("target.png", Width / 2 - targetwidth / 2, Height / 2 - targetheight / 2, targetwidth, targetheight);

                DrawMaterialSelector();
                DrawChatLines();
                if (GuiTyping == TypingState.Typing)
                {
                    Draw2dText(GuiTypingBuffer + "_", 50, Height - 100, chatfontsize, Color.White);
                }
                if (Keyboard[OpenTK.Input.Key.Tab])
                {
                    var l = new List<string>(network.ConnectedPlayers());
                    for (int i = 0; i < l.Count; i++)
                    {
                        Draw2dText(l[i], 200 + 200 * (i / 8), 200 + 30 * i, chatfontsize, Color.White);
                    }
                }
            }
            else if (guistate == GuiState.EscapeMenu)
                DrawEscapeMenu();
            else if (guistate == GuiState.MainMenu)
                DrawMainMenu();
            else throw new Exception();
            if (ENABLE_DRAWFPS)
            {
                Draw2dText(fpstext, 20f, 20f, 14,Color.White);
            }
            PerspectiveMode();
        }
        private void DrawMaterialSelector()
        {
            int singlesize = 40;
            for (int i = 0; i < 10; i++)
            {
                Draw2dTexture(terrainTexture, xcenter(singlesize * 10) + i * singlesize, Height - 100, singlesize, singlesize,
                    data.GetTileTextureId((int)MaterialSlots[i], TileSide.Top));
                if (i == activematerial)
                {
                    Draw2dBitmapFile("gui\\activematerial.png", xcenter(singlesize * 10) + i * singlesize, Height - 100, singlesize, singlesize);
                }
            }
        }
        private int xcenter(float width)
        {
            return (int)(Width / 2 - width / 2);
        }
        private int ycenter(float height)
        {
            return (int)(Height / 2 - height / 2);
        }
        int ChatScreenExpireTimeSeconds = 20;
        private void DrawChatLines()
        {
            /*
            if (chatlines.Count>0 && (DateTime.Now - chatlines[0].time).TotalSeconds > 10)
            {
                chatlines.RemoveAt(0);
            }
            */
            List<Chatline> chatlines2 = new List<Chatline>();
            foreach (Chatline c in chatlines)
            {
                if ((DateTime.Now - c.time).TotalSeconds < ChatScreenExpireTimeSeconds)
                {
                    chatlines2.Add(c);
                }
            }
            for (int i = 0; i < chatlines2.Count; i++)
            {
                Draw2dText(chatlines2[i].text, 20, 50f + i * 25f, chatfontsize, Color.White);
            }
        }
        SizeF TextSize(string text, float fontsize)
        {
            var font = new Font("Verdana", fontsize);
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            SizeF size = g.MeasureString(text, font);
            return size;
        }
        void Draw2dText(string text, float x, float y, float fontsize, Color? color)
        {
            if (color == null) { color = Color.White; }
            var font = new Font("Verdana", fontsize);
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            SizeF size = g.MeasureString(text, font);
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }
            bmp = new Bitmap((int)size.Width, (int)size.Height);
            g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, size.Width, size.Height);
            g.DrawString(text, font, new SolidBrush(color.Value), 0, 0);
            int texture = LoadTexture(bmp);
            Draw2dTexture(texture, x, y, size.Width, size.Height, null);
            GL.DeleteTexture(texture);
        }
        bool ENABLE_DRAWFPS = false;
        void Draw2dBitmapFile(string filename, float x1, float y1, float width, float height)
        {
            if (!textures.ContainsKey(filename))
            {
                textures[filename] = LoadTexture(getfile.GetFile(filename));
            }
            Draw2dTexture(textures[filename], x1, y1, width, height, null);
        }
        void Draw2dTexture(int textureid, float x1, float y1, float width, float height, int? inAtlasId)
        {
            RectangleF rect;
            if (inAtlasId == null)
            {
                rect = new RectangleF(0, 0, 1, 1);
            }
            else
            {
                rect = TextureCoords(inAtlasId.Value, texturesPacked);
            }
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, textureid);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Begin(BeginMode.Quads);
            float x2 = x1 + width;
            float y2 = y1 + height;
            GL.TexCoord2(rect.Right, rect.Bottom); GL.Vertex2(x2, y2);
            GL.TexCoord2(rect.Right, rect.Top); GL.Vertex2(x2, y1);
            GL.TexCoord2(rect.Left, rect.Top); GL.Vertex2(x1, y1);
            GL.TexCoord2(rect.Left, rect.Bottom); GL.Vertex2(x1, y2);
            /*
            GL.TexCoord2(1, 1); GL.Vertex2(x2, y2);
            GL.TexCoord2(1, 0); GL.Vertex2(x2, y1);
            GL.TexCoord2(0, 0); GL.Vertex2(x1, y1);
            GL.TexCoord2(0, 1); GL.Vertex2(x1, y2);
            */
            GL.End();
            GL.Enable(EnableCap.DepthTest);
        }
        void OrthoMode()
        {
            //GL.Disable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
        }
        // Set Up A Perspective View
        void PerspectiveMode()
        {
            // Enter into our projection matrix mode
            GL.MatrixMode(MatrixMode.Projection);
            // Pop off the last matrix pushed on when in projection mode (Get rid of ortho mode)
            GL.PopMatrix();
            // Go back to our model view matrix like normal
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            //GL.LoadIdentity();
            //GL.Enable(EnableCap.DepthTest);
        }
        //int targettexture = -1;
        IEnumerable<TileSide> AllTileSides
        {
            get
            {
                yield return TileSide.Front;
                yield return TileSide.Back;
                yield return TileSide.Left;
                yield return TileSide.Right;
                yield return TileSide.Top;
                yield return TileSide.Bottom;
            }
        }
        float chunkupdateframecounter = 0;
        float vboupdatesperframe = 0.5f;
        private void DrawImmediateParticleEffects(double deltaTime)
        {
            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
            foreach (ParticleEffect p in new List<ParticleEffect>(particleEffects))
            {
                foreach (Particle pp in p.particles)
                {
                    GL.Begin(BeginMode.Triangles);
                    RectangleF texrec = TextureCoords(p.textureid, texturesPacked);
                    GL.TexCoord2(texrec.Left, texrec.Top);
                    GL.Vertex3(pp.position);
                    GL.TexCoord2(texrec.Right, texrec.Top);
                    GL.Vertex3(pp.position + Vector3.Multiply(pp.direction, new Vector3(0, particlesize, particlesize)));
                    GL.TexCoord2(texrec.Right, texrec.Bottom);
                    GL.Vertex3(pp.position + Vector3.Multiply(pp.direction, new Vector3(particlesize, 0, particlesize)));
                    Vector3 delta = pp.direction;
                    delta = Vector3.Multiply(delta, (float)deltaTime * particlespeed);
                    pp.direction.Y -= (float)deltaTime * particlegravity;
                    pp.position += delta;
                    GL.End();
                }
                if ((DateTime.Now - p.start) >= particletime)
                {
                    particleEffects.Remove(p);
                }
            }
        }
        float particlesize = 0.6f;
        float particlespeed = 5;
        float particlegravity = 2f;
        int particlecount = 20;
        TimeSpan particletime = TimeSpan.FromSeconds(5);
        int maxparticleeffects = 50;
        List<ParticleEffect> particleEffects = new List<ParticleEffect>();
        class ParticleEffect
        {
            public Vector3 center;
            public DateTime start;
            public List<Particle> particles = new List<Particle>();
            public int textureid;
        }
        class Particle
        {
            public Vector3 position;
            public Vector3 direction;
        }
        Random rnd = new Random();
        private void StartParticleEffect(Vector3 v)
        {
            if (particleEffects.Count >= maxparticleeffects)
            {
                return;
            }
            ParticleEffect p = new ParticleEffect();
            p.center = v + new Vector3(0.5f, 0.5f, 0.5f);
            p.start = DateTime.Now;
            p.textureid = data.GetTileTextureId(clientgame.Map[(int)v.X, (int)v.Z, (int)v.Y], TileSide.Top);
            for (int i = 0; i < particlecount; i++)
            {
                Particle pp = new Particle();
                pp.position = p.center;
                pp.direction = new Vector3((float)rnd.NextDouble() - 0.5f,
                    (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
                pp.direction.Normalize();
                p.particles.Add(pp);
            }
            particleEffects.Add(p);
        }
        private bool IsValidPos(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= clientgame.MapSizeX || y >= clientgame.MapSizeY || z >= clientgame.MapSizeZ)
            {
                return false;
            }
            return true;
        }
        private Vector3 From3dPos(TilePosSide v)
        {
            if (v.side == TileSide.Back) { return v.pos + new Vector3(-1, 0, 0); }
            if (v.side == TileSide.Right) { return v.pos + new Vector3(0, 0, -1); }
            if (v.side == TileSide.Top) { return v.pos + new Vector3(0, -1, 0); }
            return v.pos;
        }
        int activematerial = 0;
        void DrawCube(Vector3 pos)
        {
            float size = 0.5f;
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.Purple);
            //GL.Color3(Color.Silver);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.Honeydew);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);

            //GL.Color3(Color.Moccasin);

            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.IndianRed);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);

            //GL.Color3(Color.PaleVioletRed);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.ForestGreen);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);

            GL.Color3(Color.Transparent);

            GL.End();
        }
        private void DrawCubeLines(Vector3 posx)
        {
            //Vector3 pos = new Vector3((int)posx.X, (int)posx.Y, (int)posx.Z);
            Vector3 pos = posx;
            pos += new Vector3(0.5f, 0.5f, 0.5f);
            GL.LineWidth(150);
            float size = 0.51f;
            GL.Begin(BeginMode.LineStrip);
            GL.Color3(Color.Red);
            //GL.Color3(Color.Silver);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.Honeydew);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);

            //GL.Color3(Color.Moccasin);

            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.IndianRed);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);

            //GL.Color3(Color.PaleVioletRed);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.ForestGreen);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + 1.0f * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -1.0f * size, pos.Z + 1.0f * size);

            GL.Color3(Color.White);//Color.Transparent);

            GL.End();
        }
        public static T Clamp<T>(T value, T min, T max)
             where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }
        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
        }
        DateTime lasttitleupdate;
        int fpscount = 0;
        string fpstext = "";
        private void UpdateTitleFps(FrameEventArgs e)
        {
            string title = "";
            fpscount++;
            TimeSpan elapsed = (DateTime.Now - lasttitleupdate);
            if (elapsed.TotalSeconds >= 1)
            {
                lasttitleupdate = DateTime.Now;
                title += "FPS: " + (int)((float)fpscount / elapsed.TotalSeconds);
                //z = 100;
                fpscount = 0;
                int totaltriangles = 0;
                foreach (var k in VisibleVbo())
                {
                    totaltriangles += k.NumElements / 3;
                }
                title += ", triangles: " + totaltriangles;
                //Title = title;
                Title = applicationname;
                fpstext = title;
            }
        }
        string applicationname = "Manic Digger";
        bool ENABLE_VISIBILITY_CULLING = false;
        private IEnumerable<Vbo> VisibleVbo()
        {
            foreach (var k in vbo)
            {
                if (!ENABLE_VISIBILITY_CULLING || (k.Value.box.Center() - player.playerposition).Length < viewdistance)
                {
                    yield return k.Value;
                }
            }
        }
        int strideofvertices = -1;
        int StrideOfVertices
        {
            get
            {
                if (strideofvertices == -1) strideofvertices = BlittableValueType.StrideOf(CubeVertices);
                return strideofvertices;
            }
        }
        Vbo LoadVBO<TVertex>(TVertex[] vertices, ushort[] elements) where TVertex : struct
        {
            Vbo handle = new Vbo();
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out handle.VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * StrideOfVertices), vertices,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * StrideOfVertices != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            GL.GenBuffers(1, out handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(ushort)), elements,//aaa sizeof(short)
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length * sizeof(ushort) != size)//aaa ushort
                throw new ApplicationException("Element data not uploaded correctly");

            handle.NumElements = elements.Length;
            return handle;
        }

        void Draw(Vbo handle)
        {
            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).

            //GL.EnableClientState(EnableCap.ColorArray);
            GL.EnableClientState(EnableCap.TextureCoordArray);
            GL.EnableClientState(EnableCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);

            GL.VertexPointer(3, VertexPointerType.Float, StrideOfVertices, new IntPtr(0));
            //GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVertices), new IntPtr(12));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, StrideOfVertices, new IntPtr(12));

            GL.DrawElements(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);//aaa
        }
        #region ILocalPlayerPosition Members
        public Vector3 LocalPlayerPosition { get { return player.playerposition; } set { player.playerposition = value; } }
        public Vector3 LocalPlayerOrientation { get { return player.playerorientation; } set { player.playerorientation = value; } }
        #endregion
        public void AddChatline(string s)
        {
            chatlines.Add(new Chatline() { text = s, time = DateTime.Now });
        }
    }
    public class ManicDiggerProgram2
    {
        public void Start()
        {
            var data = new GameDataTilesManicDigger();
            var audio = new AudioOpenAl();
            var network = new ClientNetworkDummy();
            var clientgame = new ClientGame();
            var w = new ManicDiggerGameWindow();
            var getfile = new GetFilePath() { DataPath = "mine" };
            w.clientgame = clientgame;
            w.network = network;
            w.audio = audio;
            w.data = data;
            w.getfile = getfile;
            var p = new CharacterPhysics();
            clientgame.p = p;
            p.clientgame = clientgame;
            network.gui = w;
            network.map1 = w;
            var mapgenerator = new MapGeneratorPlain();
            mapgenerator.data = data;
            mapgenerator.map = clientgame;
            clientgame.mapgenerator = mapgenerator;
            clientgame.gui = w;
            clientgame.getfile = getfile;
            audio.getfile = getfile;
            audio.gameexit = w;
            w.Run();
        }
    }
    public class ManicDiggerProgram : IInternetGameFactory
    {
        static KernelAndBinder b;
        public void MainModule(KernelAndBinder k)
        {
            k.Bind<IGameExit, ManicDiggerGameWindow>();
            k.Bind<IGui, ManicDiggerGameWindow>();
            k.Bind<IMapGenerator, MapGeneratorPlain>();
            k.Bind<IMapStorage, ClientGame>();
            k.Bind<IMap, ManicDiggerGameWindow>();
            k.Bind<IAudio, AudioOpenAl>();
            k.Bind<IClientNetwork, ClientNetworkDummy>();
            k.BindInstance<IInternetGameFactory>(this);
        }
        public void MinecraftModule(KernelAndBinder k)
        {
            k.BindInstance<IGetFilePath>(new GetFilePath() { DataPath = "minecraft" });
            k.Bind<IGameData, GameDataTilesMinecraft>();
        }
        public void ManicDiggerModule(KernelAndBinder k)
        {
            k.BindInstance<IGetFilePath>(new GetFilePath() { DataPath = "manicdigger" });
            k.Bind<IGameData, GameDataTilesManicDigger>();
        }
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            KernelAndBinder b = new KernelAndBinder();
            MainModule(b);
            b.BindInstance<IGui>(window);
            b.BindInstance<ManicDiggerGameWindow>(window);
            b.Bind<IClientNetwork, ClientNetworkMinecraft>();
            clientgame = b.Get<ClientGame>();
            network = b.Get<IClientNetwork>();
        }
        ManicDiggerGameWindow window;
        ClientGame clientgame;
        IClientNetwork network;
        public IClientNetwork GetNetwork()
        {
            return network;
        }
        public ClientGame GetClientGame()
        {
            return clientgame;
        }
        #endregion
        [STAThread]
        public static void Main(string[] args)
        {
            //new ManicDiggerProgram2().Start();
            new ManicDiggerProgram().Start(args);
        }
        private void Start(string[] args)
        {
            b = new KernelAndBinder();
            bool digger = args.Length < 1; if (Debugger.IsAttached) digger = false;
            MainModule(b);
            if (!digger)
            {
                MinecraftModule(b);
            }
            else
            {
                ManicDiggerModule(b);
            }
            using (var w = b.Get<ManicDiggerGameWindow>())
            {
                this.window = w;
                w.Run(0, 0);
            }
        }
    }
    public interface IMap
    {
        //void LoadMap(byte[, ,] map);
        void UpdateTileSet(Vector3 pos, byte type);
    }
    public interface ILocalPlayerPosition
    {
        Vector3 LocalPlayerPosition { get; set; }
        Vector3 LocalPlayerOrientation { get; set; }
    }
    public enum BlockSetMode
    {
        Create,
        Destroy,
    }
    public interface IClientNetwork
    {
        void Dispose();
        void Connect(string serverAddress, int port, string username, string auth);
        void Process();
        void SendSetBlock(Vector3 position, BlockSetMode mode, byte type);
        event EventHandler<MapLoadedEventArgs> MapLoaded;
        void SendChat(string s);
        IEnumerable<string> ConnectedPlayers();
    }
    public class ClientNetworkDummy : IClientNetwork
    {
        public void Dispose()
        {
        }
        public void Connect(string serverAddress, int port, string username, string auth)
        {
        }
        public void Process()
        {
        }
        [Inject]
        public IMap map1 { get; set; }
        public void SendSetBlock(Vector3 position, BlockSetMode mode, byte type)
        {
            if (mode == BlockSetMode.Destroy)
            {
                type = (byte)TileTypeMinecraft.Empty;
            }
            map1.UpdateTileSet(position, type);
        }
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        [Inject]
        public IGui gui { get; set; }
        public void SendChat(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("/"))
            {
                string cmd = ss[0].Substring(1);
                string arguments;
                if (s.IndexOf(" ") == -1)
                { arguments = ""; }
                else
                { arguments = s.Substring(s.IndexOf(" ")); }
                arguments = arguments.Trim();
                if (cmd == "generate")
                {
                    DoGenerate(arguments, false);
                    gui.DrawMap();
                }
            }
            gui.AddChatline(s);
        }
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public fCraft.MapGenerator gen { get; set; }
        void DoGenerate(string mode, bool hollow)
        {
            switch (mode)
            {
                case "flatgrass":
                    bool reportedProgress = false;
                    playerMessage("Generating flatgrass map...");
                    for (int i = 0; i < map.MapSizeX; i++)
                    {
                        for (int j = 0; j < map.MapSizeY; j++)
                        {
                            for (int k = 1; k < map.MapSizeZ / 2 - 1; k++)
                            {
                                if (!hollow) map.SetBlock(i, j, k, data.TileIdDirt());
                            }
                            map.SetBlock(i, j, map.MapSizeZ / 2 - 1, data.TileIdGrass());
                        }
                        if (i > map.MapSizeX / 2 && !reportedProgress)
                        {
                            reportedProgress = true;
                            playerMessage("Map generation: 50%");
                        }
                    }

                    //map.MakeFloodBarrier();

                    //if (map.Save(filename))
                    //{
                    //    player.Message("Map generation: Done.");
                    //}
                    //else
                    //{
                    //    player.Message(Color.Red, "An error occured while generating the map.");
                    //}
                    break;

                case "empty":
                    playerMessage("Generating empty map...");
                    //map.MakeFloodBarrier();

                    //if (map.Save(filename))
                    //{
                    //    player.Message("Map generation: Done.");
                    //}
                    //else
                    //{
                    //    player.Message(Color.Red, "An error occured while generating the map.");
                    //}

                    break;

                case "hills":
                    playerMessage("Generating terrain...");
                    gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              1, 1, 0.5, 0.45, 0, 0.5, hollow));
                    break;

                case "mountains":
                    playerMessage("Generating terrain...");
                    gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              4, 1, 0.5, 0.45, 0.1, 0.5, hollow));
                    break;

                case "lake":
                    playerMessage("Generating terrain...");
                    gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              1, 0.6, 0.9, 0.45, -0.35, 0.55, hollow));
                    break;

                case "island":
                    playerMessage("Generating terrain...");
                    gen.GenerateMap(new fCraft.MapGeneratorParameters(1, 0.6, 1, 0.45, 0.3, 0.35, hollow));
                    break;

                default:
                    playerMessage("Unknown map generation mode: " + mode);
                    break;
            }
        }
        private void playerMessage(string p)
        {
            gui.AddChatline(p);
        }
        public IEnumerable<string> ConnectedPlayers()
        {
            yield return "[local player]";
        }
    }
    public class MapLoadedEventArgs : EventArgs
    {
        public byte[, ,] map;
    }
    public class ClientNetworkMinecraft : IClientNetwork
    {
        [Inject]
        public IMap map { get; set; }
        //public void Connect(LoginData login, string username)
        public void Connect(string serverAddress, int port, string username, string auth)
        {
            main = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);

            iep = new IPEndPoint(IPAddress.Any, port);
            main.Connect(serverAddress, port);
            byte[] n = CreateLoginPacket(username, auth);
            main.Send(n);
        }
        private static byte[] CreateLoginPacket(string username, string verificationKey)
        {
            MemoryStream n = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(n);
            bw.Write((byte)0);//Packet ID 
            bw.Write((byte)0x07);//Protocol version
            bw.Write(StringToBytes(username));//Username
            bw.Write(StringToBytes(verificationKey));//Verification key
            bw.Write((byte)0);//Unused
            return n.ToArray();
        }
        IPEndPoint iep;
        Socket main;
        public void SendPacket(byte[] packet)
        {
            int sent = main.Send(packet);
            if (sent != packet.Length)
            {
                throw new Exception();
            }
        }
        public void Disconnect()
        {
            main.Disconnect(false);
        }
        [Inject]
        public ILocalPlayerPosition position { get; set; }
        DateTime lastpositionsent;
        public void SendSetBlock(Vector3 position, BlockSetMode mode, byte type)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)ClientPacketId.SetBlock);
            WriteInt16(bw, (short)(position.X));//-4
            WriteInt16(bw, (short)(position.Z));
            WriteInt16(bw, (short)position.Y);
            bw.Write((byte)(mode == BlockSetMode.Create ? 1 : 0));
            bw.Write((byte)type);
            SendPacket(ms.ToArray());
        }
        public void SendChat(string s)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)ClientPacketId.Message);
            bw.Write((byte)255);//unused
            WriteString64(bw, s);
            SendPacket(ms.ToArray());
        }
        public void Process()
        {
            if (main == null)
            {
                return;
            }
            for (; ; )
            {
                if (!main.Poll(0, SelectMode.SelectRead))
                {
                    break;
                }
                byte[] data = new byte[1024];
                int recv;
                try
                {
                    recv = main.Receive(data);
                }
                catch
                {
                    recv = 0;
                }
                if (recv == 0)
                {
                    //disconnected
                    return;
                }
                for (int i = 0; i < recv; i++)
                {
                    received.Add(data[i]);
                }
                for (; ; )
                {
                    if (received.Count < 4)
                    {
                        break;
                    }
                    byte[] packet = new byte[received.Count];
                    int bytesRead;
                    bytesRead = TryReadPacket();
                    if (bytesRead > 0)
                    {
                        received.RemoveRange(0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (spawned && ((DateTime.Now - lastpositionsent).TotalSeconds > 0.1))
            {
                lastpositionsent = DateTime.Now;
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write((byte)ClientPacketId.PositionandOrientation);
                bw.Write((byte)255);//player id, self
                WriteInt16(bw, (short)((position.LocalPlayerPosition.X) * 32));//gfd1
                WriteInt16(bw, (short)((position.LocalPlayerPosition.Y + CharacterPhysics.characterheight) * 32));
                WriteInt16(bw, (short)(position.LocalPlayerPosition.Z * 32));
                bw.Write((byte)(((position.LocalPlayerOrientation.Y % 2 * Math.PI) / (2 * Math.PI)) * 256));//heading todo
                bw.Write((byte)(position.LocalPlayerOrientation.X / 256));//pitch todo
                //Console.WriteLine("sent " + position.LocalPlayerPosition);
                SendPacket(ms.ToArray());
            }
        }
        bool spawned = false;
        private int TryReadPacket()
        {
            BinaryReader br = new BinaryReader(new MemoryStream(received.ToArray()));
            if (received.Count == 0)
            {
                return 0;
            }
            var packetId = (ServerPacketId)br.ReadByte();
            int totalread = 1;
            Console.WriteLine(Enum.GetName(typeof(ServerPacketId), packetId));
            if (packetId == ServerPacketId.ServerIdentification)
            {
                totalread += 1 + 64 + 64 + 1; if (received.Count < totalread) { return 0; }
                ServerPlayerIdentification p = new ServerPlayerIdentification();
                p.ProtocolVersion = br.ReadByte();
                if (p.ProtocolVersion != 7)
                {
                    throw new Exception();
                }
                p.ServerName = ReadString64(br);
                p.ServerMotd = ReadString64(br);
                p.UserType = br.ReadByte();
                //connected = true;
            }
            else if (packetId == ServerPacketId.Ping)
            {
            }
            else if (packetId == ServerPacketId.LevelInitialize)
            {
                receivedMapStream = new MemoryStream();
            }
            else if (packetId == ServerPacketId.LevelDataChunk)
            {
                totalread += 2 + 1024 + 1; if (received.Count < totalread) { return 0; }
                int chunkLength = ReadInt16(br);
                byte[] chunkData = br.ReadBytes(1024);
                BinaryWriter bw1 = new BinaryWriter(receivedMapStream);
                byte[] chunkDataWithoutPadding = new byte[chunkLength];
                for (int i = 0; i < chunkLength; i++)
                {
                    chunkDataWithoutPadding[i] = chunkData[i];
                }
                bw1.Write(chunkDataWithoutPadding);
                MapLoadingPercentComplete = br.ReadByte();
                Console.WriteLine(MapLoadingPercentComplete);
            }
            else if (packetId == ServerPacketId.LevelFinalize)
            {
                totalread += 2 + 2 + 2; if (received.Count < totalread) { return 0; }
                mapreceivedsizex = ReadInt16(br);
                mapreceivedsizez = ReadInt16(br);
                mapreceivedsizey = ReadInt16(br);
                //ReceivedMapEventArgs a = new ReceivedMapEventArgs();
                //a.data = receivedMap.ToArray();
                //a.sizex = mapreceivedsizex;
                //a.sizey = mapreceivedsizey;
                //a.sizez = mapreceivedsizez;
                //if (MapReceived != null)
                //{
                //MapReceived.Invoke(this, a);
                //}
                //mapreceived = true;
                receivedMapStream.Seek(0, SeekOrigin.Begin);
                MemoryStream decompressed = new MemoryStream(GzipCompression.Decompress(receivedMapStream.ToArray()));
                //File.WriteAllBytes("c:\\map.dat", decompressed.ToArray());
                if (decompressed.Length != mapreceivedsizex * mapreceivedsizey * mapreceivedsizez +
                    (decompressed.Length % 1024))
                {
                    //throw new Exception();
                    Console.WriteLine("warning: invalid map data size");
                }
                byte[, ,] receivedmap = new byte[mapreceivedsizex, mapreceivedsizey, mapreceivedsizez];
                {
                    BinaryReader br2 = new BinaryReader(decompressed);
                    int wtf = br2.ReadByte();
                    int wtf2 = br2.ReadByte();
                    int wtf3 = br2.ReadByte();
                    int wtf4 = br2.ReadByte();
                    for (int z = 0; z < mapreceivedsizez; z++)
                    {
                        for (int y = 0; y < mapreceivedsizey; y++)
                        {
                            for (int x = 0; x < mapreceivedsizex; x++)
                            {
                                //receivedmap[(x + mapreceivedsizex - 4) % mapreceivedsizex, y, z] = br2.ReadByte();
                                receivedmap[x, y, z] = br2.ReadByte();
                                //todo fix wtf, map is broken, x rotation=4.
                            }
                        }
                    }
                }
                //map.LoadMap(receivedmap);
                if (MapLoaded != null)
                {
                    MapLoaded.Invoke(this, new MapLoadedEventArgs() { map = receivedmap });
                }
            }
            else if (packetId == ServerPacketId.SetBlock)
            {
                totalread += 2 + 2 + 2 + 1; if (received.Count < totalread) { return 0; }
                int x = ReadInt16(br);
                int z = ReadInt16(br);
                int y = ReadInt16(br);
                byte type = br.ReadByte();
                map.UpdateTileSet(new Vector3(x, y, z), type);
            }
            else if (packetId == ServerPacketId.SpawnPlayer)
            {
                totalread += 1 + 64 + 2 + 2 + 2 + 1 + 1; if (received.Count < totalread) { return 0; }
                byte playerid = br.ReadByte();
                string playername = ReadString64(br);
                float x = ReadInt16(br) / 32;// +4; //gfd1
                float y = ReadInt16(br) / 32;
                float z = ReadInt16(br) / 32;
                //SendSetBlock(new Vector3(x, y - 1, z), BlockSetMode.Destroy, 0);
                byte heading = br.ReadByte();
                byte pitch = br.ReadByte();
                if (playerid == 255)
                {
                    position.LocalPlayerPosition = new Vector3(x, y, z) + new Vector3(0.5f, 0, 0.5f);
                }
                //chatlines.AddChatline(string.Format("{0} joins.", playername));
                connectedplayers.Add(new ConnectedPlayer() { name = playername, id = playerid });
                spawned = true;
            }
            else if (packetId == ServerPacketId.PlayerTeleport)
            {
                totalread += 1 + (2 + 2 + 2) + 1 + 1; if (received.Count < totalread) { return 0; }
            }
            else if (packetId == ServerPacketId.PositionandOrientationUpdate)
            {
                totalread += 1 + (2 + 2 + 2) + 1 + 1 + ((-3)); if (received.Count < totalread) { return 0; }
            }
            else if (packetId == ServerPacketId.PositionUpdate)
            {
                totalread += 1 + 1 + 1 + 1; if (received.Count < totalread) { return 0; }
            }
            else if (packetId == ServerPacketId.OrientationUpdate)
            {
                totalread += 1 + 1 + 1; if (received.Count < totalread) { return 0; }
            }
            else if (packetId == ServerPacketId.DespawnPlayer)
            {
                totalread += 1; if (received.Count < totalread) { return 0; }
                byte playerid = br.ReadByte();
                for (int i = 0; i < connectedplayers.Count; i++)
                {
                    if (connectedplayers[i].id == playerid)
                    {
                        connectedplayers.RemoveAt(i);
                    }
                }
            }
            else if (packetId == ServerPacketId.Message)
            {
                totalread += 1 + 64; if (received.Count < totalread) { return 0; }
                byte unused = br.ReadByte();
                string message = ReadString64(br);
                chatlines.AddChatline(message);
            }
            else if (packetId == ServerPacketId.DisconnectPlayer)
            {
                totalread += 64; if (received.Count < totalread) { return 0; }
                string disconnectReason = ReadString64(br);
                throw new Exception(disconnectReason);
            }
            else
            {
                throw new Exception();
            }
            return totalread;
        }
        [Inject]
        public IGui chatlines { get; set; }
        List<byte> received = new List<byte>();
        public void Dispose()
        {
            if (main != null)
            {
                //main.DisconnectAsync(new SocketAsyncEventArgs());
                main.Disconnect(false);
                main = null;
            }
            //throw new NotImplementedException();
        }
        enum ClientPacketId
        {
            PlayerIdentification = 0,
            SetBlock = 5,
            PositionandOrientation = 8,
            Message = 0x0d,
        }
        enum ServerPacketId
        {
            ServerIdentification = 0,
            Ping = 1,
            LevelInitialize = 2,
            LevelDataChunk = 3,
            LevelFinalize = 4,
            SetBlock = 6,
            SpawnPlayer = 7,
            PlayerTeleport = 8,
            PositionandOrientationUpdate = 9,
            PositionUpdate = 10,
            OrientationUpdate = 11,
            DespawnPlayer = 12,
            Message = 13,
            DisconnectPlayer = 14,
        }
        private static byte[] StringToBytes(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            byte[] bb = new byte[64];
            for (int i = 0; i < bb.Length; i++)
            {
                bb[i] = 32; //' '
            }
            for (int i = 0; i < b.Length; i++)
            {
                bb[i] = b[i];
            }
            return bb;
        }
        private static string BytesToString(byte[] s)
        {
            string b = Encoding.ASCII.GetString(s).Trim();
            return b;
        }
        public int mapreceivedsizex;
        public int mapreceivedsizey;
        public int mapreceivedsizez;
        int ReadInt16(BinaryReader br)
        {
            byte[] array = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToInt16(array, 0);
        }
        void WriteInt16(BinaryWriter bw, short v)
        {
            byte[] array = BitConverter.GetBytes((short)v);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            bw.Write(array);
        }
        int MapLoadingPercentComplete;
        public MemoryStream receivedMapStream;
        static string ReadString64(BinaryReader br)
        {
            return BytesToString(br.ReadBytes(64));
        }
        static void WriteString64(BinaryWriter bw, string s)
        {
            bw.Write(StringToBytes(s));
        }
        struct ServerPlayerIdentification
        {
            public byte ProtocolVersion;
            public string ServerName;
            public string ServerMotd;
            public byte UserType;
        }
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        class ConnectedPlayer
        {
            public int id;
            public string name;
        }
        List<ConnectedPlayer> connectedplayers = new List<ConnectedPlayer>();
        public IEnumerable<string> ConnectedPlayers()
        {
            foreach (ConnectedPlayer p in connectedplayers)
            {
                yield return p.name;
            }
        }
    }
    public class LoginData
    {
        public string serveraddress;
        public int port;
        public string mppass;
    }
    public class LoginClientMinecraft
    {
        public LoginData Login(string username, string password, string gameurl)
        {
            //Three Steps

            //Step 1.
            //---
            //Go to http://www.minecraft.net/login.jsp and GET, you will receive JSESSIONID cookie.
            //---
            string loginurl = "http://www.minecraft.net/login.jsp";
            string data11 = string.Format("username={0}&password={1}", username, password);
            string sessionidcookie;
            string sessionid;
            {
                using (WebClient c = new WebClient())
                {
                    string html = c.DownloadString(loginurl);
                    sessionidcookie = c.ResponseHeaders[HttpResponseHeader.SetCookie];
                    sessionid = sessionidcookie.Substring(0, sessionidcookie.IndexOf(";"));
                    sessionid = sessionid.Substring(sessionid.IndexOf("=") + 1);
                }
            }
            //Step 2.
            //---
            //Go to http://www.minecraft.net/login.jsp and POST "username={0}&password={1}" using JSESSIONID cookie.
            //You will receive logged in cookie ("_uid").
            //Because of multipart http page, HttpWebRequest has some trouble receiving cookies in step 2,
            //so it is easier to just use raw TcpClient for this.
            //---
            List<string> loggedincookie = new List<string>();
            {
                using (TcpClient step2Client = new TcpClient("minecraft.net", 80))
                {
                    var stream = step2Client.GetStream();
                    StreamWriter sw = new StreamWriter(stream);

                    sw.WriteLine("POST /login.jsp HTTP/1.0");
                    sw.WriteLine("Host: www.minecraft.net");
                    sw.WriteLine("Content-Type: application/x-www-form-urlencoded");
                    sw.WriteLine("Set-Cookie: " + sessionidcookie);
                    sw.WriteLine("Content-Length: " + data11.Length);
                    sw.WriteLine("");
                    sw.WriteLine(data11);

                    sw.Flush();
                    StreamReader sr = new StreamReader(stream);
                    for (; ; )
                    {
                        var s = sr.ReadLine();
                        if (s == null)
                        {
                            break;
                        }
                        if (s.Contains("Set-Cookie"))
                        {
                            loggedincookie.Add(s);
                        }
                    }
                }
            }
            for (int i = 0; i < loggedincookie.Count; i++)
            {
                loggedincookie[i] = loggedincookie[i].Replace("Set-", "");
            }
            //Step 3.
            //---
            //Go to game url and GET using JSESSIONID cookie and _uid cookie.
            //Parse the page to find server, port, mpass strings.
            //---
            WebRequest step3Request = (HttpWebRequest)HttpWebRequest.Create(gameurl);
            foreach (string cookie in loggedincookie)
            {
                step3Request.Headers.Add(cookie);
            }
            using (var s4 = step3Request.GetResponse().GetResponseStream())
            {
                string html = new StreamReader(s4).ReadToEnd();
                string serveraddress = ReadValue(html.Substring(html.IndexOf("\"server\""), 40));
                string port = ReadValue(html.Substring(html.IndexOf("\"port\""), 40));
                string mppass = ReadValue(html.Substring(html.IndexOf("\"mppass\""), 80));
                return new LoginData() { serveraddress = serveraddress, port = int.Parse(port), mppass = mppass };
            }
        }
        private static string ReadValue(string s)
        {
            string start = "value=\"";
            string end = "\"";
            string ss = s.Substring(s.IndexOf(start) + start.Length);
            ss = ss.Substring(0, ss.IndexOf(end));
            return ss;
        }
    }
}
namespace ManicDigger.Collisions
{
    public interface ITriangleContainer
    {
    }
    public struct Line3D
    {
        public Vector3 Start;
        public Vector3 End;
    }
    public struct Triangle3D
    {
        public Vector3 PointA;
        public Vector3 PointB;
        public Vector3 PointC;
    }
    public struct Box3D
    {
        public Box3D(float x, float y, float z, float size)
        {
            this.MinEdge = new Vector3(x, y, z);
            this.MaxEdge = new Vector3(x + size, y + size, z + size);
        }
        public Vector3 MinEdge;
        public Vector3 MaxEdge;
        //public Vector3 MaxEdge { get { return new Vector3(MinEdge.X + size, MinEdge.Y + size, MinEdge.Z + size); } }
        //float size;
        public float LengthX { get { return MaxEdge.X - MinEdge.X; } }
        public float LengthY { get { return MaxEdge.Y - MinEdge.Y; } }
        public float LengthZ { get { return MaxEdge.Z - MinEdge.Z; } }
        public void AddPoint(float x, float y, float z)
        {
            //if is empty
            if (MinEdge == new Vector3(0, 0, 0) && MaxEdge == new Vector3(0, 0, 0))
            {
                MinEdge = new Vector3(x, y, z);
                MaxEdge = new Vector3(x, y, z);
            }
            MinEdge.X = Math.Min(MinEdge.X, x);
            MinEdge.Y = Math.Min(MinEdge.Y, y);
            MinEdge.Z = Math.Min(MinEdge.Z, z);
            MaxEdge.X = Math.Max(MaxEdge.X, x);
            MaxEdge.Y = Math.Max(MaxEdge.Y, y);
            MaxEdge.Z = Math.Max(MaxEdge.Z, z);
        }
        public Vector3 Center()
        {
            return (MinEdge + MaxEdge) / 2;
        }
    }
    public interface ITriangleSearcher
    {
        int AddTriangle(Triangle3D triangle);
        void DeleteTriangle(int triangle_id);
        IEnumerable<Vector3> LineIntersection(Line3D line);
    }
    public class TileOctreeSearcher
    {
        public Box3D StartBox;
        IEnumerable<Box3D> Search(Predicate<Box3D> query)
        {
            if (StartBox.LengthX == 0)
            {
                throw new Exception();
            }
            return SearchPrivate(query, StartBox);
        }
        IEnumerable<Box3D> SearchPrivate(Predicate<Box3D> query, Box3D box)
        {
            if (box.LengthX == 1)
            {
                yield return box;
                yield break;
            }
            foreach (Box3D child in Children(box))
            {
                if (query(child))
                {
                    foreach (Box3D n in SearchPrivate(query, child))
                    {
                        yield return n;
                    }
                }
            }
        }
        IEnumerable<Box3D> Children(Box3D box)
        {
            float x = box.MinEdge.X;
            float y = box.MinEdge.Y;
            float z = box.MinEdge.Z;
            float size = box.LengthX / 2;
            yield return new Box3D(x, y, z, size);
            yield return new Box3D(x + size, y, z, size);
            yield return new Box3D(x, y, z + size, size);
            yield return new Box3D(x + size, y, z + size, size);

            yield return new Box3D(x, y + size, z, size);
            yield return new Box3D(x + size, y + size, z, size);
            yield return new Box3D(x, y + size, z + size, size);
            yield return new Box3D(x + size, y + size, z + size, size);
        }
        public delegate bool IsTileEmpty(int x, int y, int z);
        public IEnumerable<TilePosSide> LineIntersection(IsTileEmpty isEmpty, Line3D line)
        {
            Vector3 hit = new Vector3();
            foreach (var node in
                //quadtree.Search(v=>true))
                 Search(v => Intersection.CheckLineBox(v, line, out hit)))
            {
                /*
                foreach (var obj in node.ObjectsHere)
                {
                    Vector3 intersection;
                    Triangle3D t = triangles[obj.ObjectId];
                    bool intersects = Intersection.RayTriangle(line, t, out intersection) != 0;
                    if (intersects)
                    {
                        yield return intersection;
                    }
                }
                */
                if (!isEmpty((int)node.MinEdge.X, (int)node.MinEdge.Z, (int)node.MinEdge.Y))
                {
                    var hit2 = Intersection.CheckLineBoxExact(line, node);
                    yield return hit2;
                }
            }
        }
    }
    public enum TileSide
    {
        Top,
        Bottom,
        Front,
        Back,
        Left,
        Right,
    }
    public struct TilePosSide
    {
        public TilePosSide(int x, int y, int z, TileSide side)
        {
            this.pos = new Vector3(x, y, z);
            this.side = side;
        }
        public Vector3 pos;
        public TileSide side;
        public Vector3 Translated()
        {
            if (side == TileSide.Top) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Bottom) { return pos + new Vector3(0, -1, 0); }
            if (side == TileSide.Front) { return pos + new Vector3(-1, 0, 0); }
            if (side == TileSide.Back) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Left) { return pos + new Vector3(0, 0, -1); }
            if (side == TileSide.Right) { return pos + new Vector3(0, 0, 0); }
            throw new Exception();
        }
    }
    public static class Intersection
    {
        //http://www.3dkingdoms.com/weekly/weekly.php?a=3
        static bool GetIntersection(float fDst1, float fDst2, Vector3 P1, Vector3 P2, out Vector3 Hit)
        {
            Hit = new Vector3();
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));
            return true;
        }
        static bool InBox(Vector3 Hit, Vector3 B1, Vector3 B2, int Axis)
        {
            if (Axis == 1 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            if (Axis == 2 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.X > B1.X && Hit.X < B2.X) return true;
            if (Axis == 3 && Hit.X > B1.X && Hit.X < B2.X && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            return false;
        }
        // returns true if line (L1, L2) intersects with the box (B1, B2)
        // returns intersection point in Hit
        public static bool CheckLineBox(Vector3 B1, Vector3 B2, Vector3 L1, Vector3 L2, out Vector3 Hit)
        {
            Hit = new Vector3();
            if (L2.X < B1.X && L1.X < B1.X) return false;
            if (L2.X > B2.X && L1.X > B2.X) return false;
            if (L2.Y < B1.Y && L1.Y < B1.Y) return false;
            if (L2.Y > B2.Y && L1.Y > B2.Y) return false;
            if (L2.Z < B1.Z && L1.Z < B1.Z) return false;
            if (L2.Z > B2.Z && L1.Z > B2.Z) return false;
            if (L1.X > B1.X && L1.X < B2.X &&
                L1.Y > B1.Y && L1.Y < B2.Y &&
                L1.Z > B1.Z && L1.Z < B2.Z)
            {
                Hit = L1;
                return true;
            }
            if ((GetIntersection(L1.X - B1.X, L2.X - B1.X, L1, L2, out Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B1.Y, L2.Y - B1.Y, L1, L2, out Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B1.Z, L2.Z - B1.Z, L1, L2, out Hit) && InBox(Hit, B1, B2, 3))
              || (GetIntersection(L1.X - B2.X, L2.X - B2.X, L1, L2, out Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B2.Y, L2.Y - B2.Y, L1, L2, out Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B2.Z, L2.Z - B2.Z, L1, L2, out Hit) && InBox(Hit, B1, B2, 3)))
                return true;
            return false;
        }
        /// <summary>
        /// Warning: randomly returns incorrect hit position (back side of box).
        /// </summary>
        /// <param name="box"></param>
        /// <param name="line"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool CheckLineBox(Box3D box, Line3D line, out Vector3 hit)
        {
            return CheckLineBox(box.MinEdge, box.MaxEdge, line.Start, line.End, out hit);
        }
        // Copyright 2001, softSurfer (www.softsurfer.com)
        // This code may be freely used and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        // Assume that classes are already given for the objects:
        //    Point and Vector with
        //        coordinates {float x, y, z;}
        //        operators for:
        //            == to test equality
        //            != to test inequality
        //            (Vector)0 = (0,0,0)         (null vector)
        //            Point  = Point ± Vector
        //            Vector = Point - Point
        //            Vector = Scalar * Vector    (scalar product)
        //            Vector = Vector * Vector    (cross product)
        //    Line and Ray and Segment with defining points {Point P0, P1;}
        //        (a Line is infinite, Rays and Segments start at P0)
        //        (a Ray extends beyond P1, but a Segment ends at P1)
        //    Plane with a point and a normal {Point V0; Vector n;}
        //    Triangle with defining vertices {Point V0, V1, V2;}
        //    Polyline and Polygon with n vertices {int n; Point *V;}
        //        (a Polygon has V[n]=V[0])
        //===================================================================

        static float SMALL_NUM = 0.00000001f; // anything that avoids division overflow
        // dot product (3D) which allows vector operations in arguments
        static float dot(Vector3 u, Vector3 v) { return (u).X * (v).X + (u).Y * (v).Y + (u).Z * (v).Z; }

        // intersect_RayTriangle(): intersect a ray with a 3D triangle
        //    Input:  a ray R, and a triangle T
        //    Output: *I = intersection point (when it exists)
        //    Return: -1 = triangle is degenerate (a segment or point)
        //             0 = disjoint (no intersect)
        //             1 = intersect in unique point I1
        //             2 = are in the same plane
        public static int
        RayTriangle(Line3D R, Triangle3D T, out Vector3 I)
        {
            Vector3 u, v, n;             // triangle vectors
            Vector3 dir, w0, w;          // ray vectors
            float r, a, b;             // params to calc ray-plane intersect

            I = new Vector3();

            // get triangle edge vectors and plane normal
            u = T.PointB - T.PointA;
            v = T.PointC - T.PointA;
            //n = u.CrossProduct(v);             // cross product
            Vector3.Cross(ref u, ref v, out n);
            //if (n == (Vector3D)0)            // triangle is degenerate
            //    return -1;                 // do not deal with this case

            dir = R.End - R.Start;             // ray direction vector
            w0 = R.Start - T.PointA;
            a = -dot(n, w0);
            b = dot(n, dir);
            if (Math.Abs(b) < SMALL_NUM)
            {     // ray is parallel to triangle plane
                if (a == 0)                // ray lies in triangle plane
                    return 2;
                else return 0;             // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                   // ray goes away from triangle
                return 0;                  // => no intersect
            // for a segment, also test if (r > 1.0) => no intersect

            I = R.Start + r * dir;           // intersect point of ray and plane

            // is I inside T?
            float uu, uv, vv, wu, wv, D;
            uu = dot(u, u);
            uv = dot(u, v);
            vv = dot(v, v);
            w = I - T.PointA;
            wu = dot(w, u);
            wv = dot(w, v);
            D = uv * uv - uu * vv;

            // get and test parametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)        // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            return 1;                      // I is in T
        }
        public static TilePosSide CheckLineBoxExact(Line3D line, Box3D box)
        {
            if (PointInBox(line.Start, box)) { return new TilePosSide() { pos = line.Start }; }
            Vector3 big = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 closest = big;
            TileSide side = TileSide.Top;
            foreach (Triangle3DAndSide t in BoxTrianglesAndSides(box.MinEdge, box.MaxEdge))
            {
                Vector3 i;
                if (RayTriangle(line, t.t, out i) != 0)
                {
                    if ((line.Start - i).Length < (line.Start - closest).Length)
                    {
                        closest = i;
                        side = t.side;
                    }
                }
            }
            if (closest == big) { throw new Exception(); }
            return new TilePosSide() { pos = closest, side = side };
            //if (PointInBox(line.End, box)) { return new TilePosSide() { pos = line.End }; }
            throw new Exception();
        }
        public class Triangle3DAndSide
        {
            public Triangle3D t;
            public TileSide side;
        }
        public static IEnumerable<Triangle3DAndSide> BoxTrianglesAndSides(Vector3 a, Vector3 b)
        {
            TileSide side = TileSide.Top;
            TileSide sidei = TileSide.Top;
            int ii = 0;
            foreach (Triangle3D t in BoxTriangles(a, b))
            {
                side = sidei;
                ii++;
                if (ii % 2 == 0)
                {
                    if (sidei == TileSide.Top) { sidei = TileSide.Bottom; }
                    else if (sidei == TileSide.Bottom) { sidei = TileSide.Front; }
                    else if (sidei == TileSide.Front) { sidei = TileSide.Back; }
                    else if (sidei == TileSide.Back) { sidei = TileSide.Left; }
                    else if (sidei == TileSide.Left) { sidei = TileSide.Right; }
                    else if (sidei == TileSide.Right) { sidei = TileSide.Top; }
                }
                yield return new Triangle3DAndSide() { t = t, side = side };
            }
        }
        public static IEnumerable<Triangle3D> BoxTriangles(Vector3 a, Vector3 b)
        {
            float x = a.X;
            float z = a.Y;
            float y = a.Z;
            float sx = b.X - a.X;
            float sz = b.Y - a.Y;
            float sy = b.Z - a.Z;
            List<short> myelements = new List<short>();
            List<Vector3> myvertices = new List<Vector3>();
            //top
            //if (drawtop)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 1.0f * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 1.0f * sz, y + 1.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 1.0f * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 1.0f * sz, y + 1.0f * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            //if (drawbottom)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 0 * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 0 * sz, y + 1.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 0 * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 0 * sz, y + 1.0f * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //front
            //if (drawfront)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 1 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            //if (drawback)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 1 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //if (drawleft)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 0 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            //if (drawright)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 1 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            for (int i = 0; i < myelements.Count / 3; i++)
            {
                Triangle3D t = new Triangle3D();
                t.PointA = myvertices[myelements[i * 3 + 0]];
                t.PointB = myvertices[myelements[i * 3 + 1]];
                t.PointC = myvertices[myelements[i * 3 + 2]];
                yield return t;
            }
        }
        private static bool PointInBox(Vector3 v, Box3D node)
        {
            return v.X >= node.MinEdge.X && v.Y >= node.MinEdge.Y && v.Z >= node.MinEdge.Z
                && v.X <= node.MaxEdge.X && v.Y <= node.MaxEdge.Y && v.Z <= node.MaxEdge.Z;
        }
        private static Vector3 Interpolate(Vector3 a, Vector3 b, float f)
        {
            float x = a.X + (b.X - a.X) * f;
            float y = a.Y + (b.Y - a.Y) * f;
            float z = a.Z + (b.Z - a.Z) * f;
            return new Vector3(x, y, z);
        }
    }
}
class MyFCraftMap : fCraft.IFMap
{
    [Inject]
    public IMapStorage map { get; set; }
    #region IFMap Members
    public int MapSizeX
    {
        get
        {
            return map.MapSizeX;
        }
        set
        {
            map.MapSizeX = value;
        }
    }
    public int MapSizeY
    {
        get
        {
            return map.MapSizeY;
        }
        set
        {
            map.MapSizeY = value;
        }
    }
    public int MapSizeZ
    {
        get
        {
            return map.MapSizeZ;
        }
        set
        {
            map.MapSizeZ = value;
        }
    }
    public int SpawnX
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }
    public int SpawnY
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }
    public int SpawnZ
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }
    public void LoadMapArray(byte[] data, int offset)
    {
        var ms = new MemoryStream(data);
        ms.Seek(offset, SeekOrigin.Begin);
        map.LoadMapArray(ms);
    }
    public bool ValidateBlockTypes()
    {
        return true;
    }
    public void SetBlock(int x, int y, int z, int tileType)
    {
        map.Map[x, y, z] = (byte)tileType;
    }
    #endregion
}
public class GetRandomDummy : fCraft.IGetRandom
{
    #region IGetRandom Members
    Random rnd1 = new Random();
    public Random rnd
    {
        get { return rnd1; }
    }
    #endregion
}
//http://fcraft.svn.sourceforge.net/viewvc/fcraft/fCraft/fCraft/World/MapLoaderDat.cs?revision=99
//http://fcraft.svn.sourceforge.net/viewvc/fcraft/fCraft/fCraft/World/MapGenerator.cs?revision=85
//author: Fragmer, license: MIT
namespace fCraft
{
    public interface IFLogger
    {
        void Log(string s);
        void Log(string s, FLogType type);
        void Log(string message, FLogType type, params object[] values);
    }
    public interface IFMap
    {
        int MapSizeX { get; set; }
        int MapSizeY { get; set; }
        int MapSizeZ { get; set; }
        int SpawnX { get; set; }
        int SpawnY { get; set; }
        int SpawnZ { get; set; }
        void LoadMapArray(byte[] data, int offset);
        bool ValidateBlockTypes();
        void SetBlock(int x, int y, int z, int tileType);
    }
    public enum FLogType
    {
        SystemActivity,
        Error,
        Debug,
    }
    public class FLogDummy : IFLogger
    {
        public void Log(string message, FLogType type, params object[] values)
        {
            Log(String.Format(message, values), type);
        }
        #region IFLogger Members
        public void Log(string s)
        {
        }
        public void Log(string s, FLogType type)
        {
        }
        #endregion
    }
    public class MapLoaderDAT
    {
        [Inject]
        public IFLogger log { get; set; }
        public void Load(string fileName, IFMap map)
        {
            log.Log("Converting {0}...", FLogType.SystemActivity, fileName);
            byte[] temp = new byte[8];
            //Map map = new Map(world);
            byte[] data;
            int length;
            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    stream.Seek(-4, SeekOrigin.End);
                    stream.Read(temp, 0, sizeof(int));
                    stream.Seek(0, SeekOrigin.Begin);
                    length = BitConverter.ToInt32(temp, 0);
                    data = new byte[length];
                    using (GZipStream reader = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        reader.Read(data, 0, length);
                    }
                }

                //if( data[0] == 0xBE && data[1] == 0xEE && data[2] == 0xEF ) {
                for (int i = 0; i < length - 1; i++)
                {
                    if (data[i] == 0xAC && data[i + 1] == 0xED)
                    {

                        // bypassing the header crap
                        int pointer = i + 6;
                        Array.Copy(data, pointer, temp, 0, sizeof(short));
                        pointer += IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                        pointer += 13;

                        int headerEnd = 0;
                        // find the end of serialization listing
                        for (headerEnd = pointer; headerEnd < data.Length - 1; headerEnd++)
                        {
                            if (data[headerEnd] == 0x78 && data[headerEnd + 1] == 0x70)
                            {
                                headerEnd += 2;
                                break;
                            }
                        }

                        // start parsing serialization listing
                        int offset = 0;
                        while (pointer < headerEnd)
                        {
                            if (data[pointer] == 'Z') offset++;
                            else if (data[pointer] == 'I' || data[pointer] == 'F') offset += 4;
                            else if (data[pointer] == 'J') offset += 8;

                            pointer += 1;
                            Array.Copy(data, pointer, temp, 0, sizeof(short));
                            short skip = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                            pointer += 2;

                            // look for relevant variables
                            Array.Copy(data, headerEnd + offset - 4, temp, 0, sizeof(int));
                            if (MemCmp(data, pointer, "width"))
                            {
                                map.MapSizeX = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                            }
                            else if (MemCmp(data, pointer, "depth"))
                            {
                                map.MapSizeZ = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                            }
                            else if (MemCmp(data, pointer, "height"))
                            {
                                map.MapSizeY = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                            }
                            else if (MemCmp(data, pointer, "xSpawn"))
                            {
                                map.SpawnX = (short)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0)) * 32 + 16);
                            }
                            else if (MemCmp(data, pointer, "ySpawn"))
                            {
                                map.SpawnZ = (short)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0)) * 32 + 16);
                            }
                            else if (MemCmp(data, pointer, "zSpawn"))
                            {
                                map.SpawnY = (short)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0)) * 32 + 16);
                            }

                            pointer += skip;
                        }

                        // find the start of the block array
                        bool foundBlockArray = false;
                        offset = Array.IndexOf<byte>(data, 0x00, headerEnd);
                        while (offset != -1 && offset < data.Length - 2)
                        {
                            if (data[offset] == 0x00 && data[offset + 1] == 0x78 && data[offset + 2] == 0x70)
                            {
                                foundBlockArray = true;
                                pointer = offset + 7;
                            }
                            offset = Array.IndexOf<byte>(data, 0x00, offset + 1);
                        }

                        // copy the block array... or fail
                        if (foundBlockArray)
                        {
                            map.LoadMapArray(data, pointer);
                            if (!map.ValidateBlockTypes())
                            {
                                throw new Exception("Map validation failed: unknown block types found. Either parsing has done wrong, or this is an incompatible format.");
                            }
                        }
                        else
                        {
                            throw new Exception("Could not locate block array.");
                        }
                        break;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                log.Log("Conversion failed: {0}", FLogType.Error, ex.Message);
                log.Log(ex.StackTrace, FLogType.Debug);
            }
            log.Log("Conversion completed succesfully succesful.", FLogType.SystemActivity, fileName);
        }

        static bool MemCmp(byte[] data, int offset, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (offset + i >= data.Length || data[offset + i] != value[i]) return false;
            }
            return true;
        }
    }
    public interface IGetRandom
    {
        Random rnd { get; }
    }
    public class MapGeneratorParameters
    {
        public MapGeneratorParameters(double _roughness, double _smoothingOver, double _smoothingUnder, double _water, double _midpoint, double _sides, bool _hollow)
        {
            roughness = _roughness;
            smoothingOver = _smoothingOver;
            smoothingUnder = _smoothingUnder;
            midpoint = _midpoint;
            sides = _sides;
            water = _water;
            hollow = _hollow;
        }
        public double roughness, gBigSize, smoothingOver, smoothingUnder, water, midpoint, sides;
        public bool hollow;
    }
    public class MapGenerator
    {
        [Inject]
        public IGetRandom rand { get; set; }
        [Inject]
        public IFMap map { get; set; }
        [Inject]
        public IFLogger log { get; set; }
        [Inject]
        public IGameData data { get; set; }
        MapGeneratorParameters parameters;
        public void GenerateMap(MapGeneratorParameters parameters)
        {
            this.parameters = parameters;
            double[,] heightmap = GenerateHeightmap(map.MapSizeX, map.MapSizeY);
            Feedback("Filling...");
            for (int x = 0; x < map.MapSizeX; x++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    double level = heightmap[x, y];
                    if (level > parameters.water)
                    {
                        level = (level - parameters.water) * parameters.smoothingOver + parameters.water;
                        map.SetBlock(x, y, (int)(level * map.MapSizeZ), data.TileIdGrass());
                        if (!parameters.hollow)
                        {
                            for (int i = (int)(level * map.MapSizeZ) - 1; i > 0; i--)
                            {
                                if ((int)(level * map.MapSizeZ) - i < 5)
                                {
                                    map.SetBlock(x, y, i, data.TileIdDirt());
                                }
                                else
                                {
                                    map.SetBlock(x, y, i, data.TileIdStone());
                                }
                            }
                        }
                    }
                    else
                    {
                        level = (level - parameters.water) * parameters.smoothingUnder + parameters.water;
                        map.SetBlock(x, y, (int)(parameters.water * map.MapSizeZ), data.TileIdWater());
                        if (!parameters.hollow)
                        {
                            for (int i = (int)(parameters.water * map.MapSizeZ) - 1; i >= (int)(level * map.MapSizeZ); i--)
                            {
                                map.SetBlock(x, y, i, data.TileIdWater());
                            }
                        }
                        map.SetBlock(x, y, (int)(level * map.MapSizeZ), data.TileIdSand());
                        if (!parameters.hollow)
                        {
                            for (int i = (int)(level * map.MapSizeZ) - 1; i > 0; i--)
                            {
                                map.SetBlock(x, y, i, data.TileIdStone());
                            }
                        }
                    }
                }
            }
            //map.MakeFloodBarrier();
            //map.Save(filename);
            Feedback("Done.");
        }
        void Feedback(string message)
        {
            //player.Message("Map generation: " + message);
            log.Log("Map generation: " + message, FLogType.SystemActivity);
        }
        double[,] GenerateHeightmap(int iWidth, int iHeight)
        {
            double c1, c2, c3, c4;
            double[,] points = new double[iWidth + 1, iHeight + 1];

            //Assign the four corners of the intial grid random color values
            //These will end up being the colors of the four corners
            c1 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            c2 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            c3 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            c4 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            parameters.gBigSize = iWidth + iHeight;
            DivideGrid(ref points, 0, 0, iWidth, iHeight, c1, c2, c3, c4, true);
            return points;
        }
        public void DivideGrid(ref double[,] points, double x, double y, int width, int height, double c1, double c2, double c3, double c4, bool isTop)
        {
            double Edge1, Edge2, Edge3, Edge4, Middle;

            int newWidth = width / 2;
            int newHeight = height / 2;

            if (width > 1 || height > 1)
            {
                if (isTop)
                {
                    Middle = ((c1 + c2 + c3 + c4) / 4) + parameters.midpoint;	//Randomly displace the midpoint!
                }
                else
                {
                    Middle = ((c1 + c2 + c3 + c4) / 4) + Displace(newWidth + newHeight);	//Randomly displace the midpoint!
                }
                Edge1 = ((c1 + c2) / 2);	//Calculate the edges by averaging the two corners of each edge.
                Edge2 = ((c2 + c3) / 2);
                Edge3 = ((c3 + c4) / 2);
                Edge4 = ((c4 + c1) / 2);//
                //Make sure that the midpoint doesn't accidentally "randomly displaced" past the boundaries!
                Middle = Rectify(Middle);
                Edge1 = Rectify(Edge1);
                Edge2 = Rectify(Edge2);
                Edge3 = Rectify(Edge3);
                Edge4 = Rectify(Edge4);
                //Do the operation over again for each of the four new grids.
                DivideGrid(ref points, x, y, newWidth, newHeight, c1, Edge1, Middle, Edge4, false);
                DivideGrid(ref points, x + newWidth, y, width - newWidth, newHeight, Edge1, c2, Edge2, Middle, false);
                if (isTop) Feedback("Heightmap: 50%");
                DivideGrid(ref points, x + newWidth, y + newHeight, width - newWidth, height - newHeight, Middle, Edge2, c3, Edge3, false);
                DivideGrid(ref points, x, y + newHeight, newWidth, height - newHeight, Edge4, Middle, Edge3, c4, false);
                if (isTop) Feedback("Heightmap: 100%");
            }
            else
            {
                //This is the "base case," where each grid piece is less than the size of a pixel.
                //The four corners of the grid piece will be averaged and drawn as a single pixel.
                double c = (c1 + c2 + c3 + c4) / 4;

                points[(int)(x), (int)(y)] = c;
                if (width == 2)
                {
                    points[(int)(x + 1), (int)(y)] = c;
                }
                if (height == 2)
                {
                    points[(int)(x), (int)(y + 1)] = c;
                }
                if ((width == 2) && (height == 2))
                {
                    points[(int)(x + 1), (int)(y + 1)] = c;
                }
            }
        }
        double Rectify(double iNum)
        {
            if (iNum < 0)
            {
                iNum = 0;
            }
            else if (iNum > 1.0)
            {
                iNum = 1.0;
            }
            return iNum;
        }
        double Displace(double SmallSize)
        {
            double Max = SmallSize / parameters.gBigSize * parameters.roughness;
            return (rand.rnd.NextDouble() - 0.5) * Max;
        }
    }
}
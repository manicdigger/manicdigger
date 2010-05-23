using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;

namespace GameModeFortress
{
    public class NetworkClientDummyInfinite : INetworkClient
    {
        #region INetworkClient Members
        public void Dispose()
        {
        }
        public void Connect(string serverAddress, int port, string username, string auth)
        {
        }
        public void Process()
        {
        }
        public void SendSetBlock(OpenTK.Vector3 position, BlockSetMode mode, int type)
        {
        }
        public event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        public void SendChat(string s)
        {
        }
        public IEnumerable<string> ConnectedPlayers()
        {
            yield return "[Local player]";
        }
        public void SendPosition(OpenTK.Vector3 position, OpenTK.Vector3 orientation)
        {
        }
        #endregion
        #region INetworkClient Members
        public void SendCommand(byte[] cmd)
        {
        }
        #endregion
    }
    public class ManicDiggerProgram2 : IInternetGameFactory
    {
        public string GameUrl = null;
        ManicDiggerGameWindow w;
        AudioOpenAl audio;
        public void Start()
        {
            w = new ManicDiggerGameWindow();
            audio = new AudioOpenAl();
            w.audio = audio;
            MakeGame(true);
            w.GameUrl = GameUrl;
            if (User != null)
            {
                w.username = User;
            }
            w.Run();
        }
        private void MakeGame(bool singleplayer)
        {
            var gamedata = new GameDataTilesManicDigger();

            INetworkClient network;
            if (singleplayer)
            {
                network = new NetworkClientDummyInfinite();
            }
            else
            {
                network = new NetworkClientMinecraft();
            }
            var clientgame = new GameFortress();
            var mapstorage = clientgame;
            var getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var config3d = new Config3d();
            var mapManipulator = new MapManipulator();
            var terrainDrawer = new TerrainDrawer3d();
            var the3d = w;
            var exit = w;
            var localplayerposition = w;
            var worldfeatures = new WorldFeaturesDrawerDummy();
            var physics = new CharacterPhysics();
            var mapgenerator = new MapGeneratorPlain();
            var internetgamefactory = this;
            if (singleplayer)
            {
                var n = (NetworkClientDummyInfinite)network;
                /*
                n.player = localplayerposition;
                n.Gui = w;
                n.Map1 = w;
                n.Map = mapstorage;
                n.Data = gamedata;
                n.Gen = new fCraft.MapGenerator();
                n.Gen.data = gamedata;
                n.Gen.log = new fCraft.FLogDummy();
                n.Gen.map = new MyFCraftMap() { data = gamedata, map = mapstorage, mapManipulator = mapManipulator };
                n.Gen.rand = new GetRandomDummy();
                n.DEFAULTMAP = "mountains";
                */
            }
            else
            {
                var n = (NetworkClientMinecraft)network;
                n.Map = w;
                n.Clients = clientgame;
                n.Chatlines = w;
                n.Position = localplayerposition;
                n.ENABLE_FORTRESS = true;
                n.gameworld = clientgame;
            }
            terrainDrawer.the3d = the3d;
            terrainDrawer.getfile = getfile;
            terrainDrawer.config3d = config3d;
            terrainDrawer.mapstorage = clientgame;
            terrainDrawer.data = gamedata;
            terrainDrawer.exit = exit;
            terrainDrawer.localplayerposition = localplayerposition;
            terrainDrawer.worldfeatures = worldfeatures;
            terrainDrawer.OnCrash += (a, b) => { CrashReporter.Crash(b.exception); };
            //worldfeatures.getfile = getfile;
            //worldfeatures.localplayerposition = localplayerposition;
            //worldfeatures.mapstorage = mapstorage;
            //worldfeatures.the3d = the3d;
            mapManipulator.getfile = getfile;
            mapManipulator.mapgenerator = mapgenerator;
            w.map = clientgame.mapforphysics;
            w.physics = physics;
            w.clients = clientgame;
            w.network = network;
            w.data = gamedata;
            w.getfile = getfile;
            w.config3d = config3d;
            w.mapManipulator = mapManipulator;
            w.terrain = terrainDrawer;
            w.PickDistance = 4.5f;
            clientgame.physics = physics;
            clientgame.ticks = new TicksDummy() { game = clientgame };
            clientgame.terrain = terrainDrawer;
            clientgame.viewport = w;
            clientgame.data = gamedata;
            clientgame.network = network;
            clientgame.audio = audio;
            var gen = new WorldGeneratorSandbox();
            clientgame.generator = File.ReadAllText("WorldGenerator.cs");
            gen.Compile(clientgame.generator);
            clientgame.map = new InfiniteMap() { gen = gen };
            clientgame.worldgeneratorsandbox = gen;
            w.game = clientgame;
            w.login = new LoginClientMinecraft();
            w.internetgamefactory = internetgamefactory;
            physics.map = clientgame.mapforphysics;
            physics.data = gamedata;
            clientgame.physics = physics;
            mapgenerator.data = gamedata;
            audio.getfile = getfile;
            audio.gameexit = w;
        }
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            MakeGame(false);
        }
        #endregion
        public string User;
    }
    public class ManicDiggerProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new CrashReporter().Start(Start, args);
        }
        private static void Start(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                string appPath = Path.GetDirectoryName(Application.ExecutablePath);
                System.Environment.CurrentDirectory = appPath;
            }
            var p = new ManicDiggerProgram2();
            if (args.Length > 0)
            {
                if (args[0].EndsWith(".mdlink", StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlDocument d = new XmlDocument();
                    d.Load(args[0]);
                    string mode = XmlTool.XmlVal(d, "/ManicDiggerLink/GameMode");
                    if (mode != "Fortress")
                    {
                        throw new Exception("Invalid game mode: " + mode);
                    }
                    p.GameUrl = XmlTool.XmlVal(d, "/ManicDiggerLink/Ip");
                    p.User = XmlTool.XmlVal(d, "/ManicDiggerLink/User");
                }
            }
            p.Start();
        }
    }
}

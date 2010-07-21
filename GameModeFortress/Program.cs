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
        double starttime = gettime();
        static double gettime()
        {
            return (double)DateTime.Now.Ticks / (10 * 1000 * 1000);
        }
        int simulationcurrentframe;
        double oldtime;
        double accumulator;
        double SIMULATION_STEP_LENGTH = 1.0 / 64;
        public void Process()
        {
            double currenttime = gettime() - starttime;
            double deltaTime = currenttime - oldtime;
            accumulator += deltaTime;
            double dt = SIMULATION_STEP_LENGTH;
            while (accumulator > dt)
            {
                simulationcurrentframe++;
                gameworld.Tick();
                accumulator -= dt;
            }
            oldtime = currenttime;
            //players.Players[0].Position = localplayerposition.LocalPlayerPosition;
            //players.Players[0].Pitch = NetworkClientMinecraft.PitchByte(localplayerposition.LocalPlayerOrientation);
            //players.Players[0].Heading = NetworkClientMinecraft.HeadingByte(localplayerposition.LocalPlayerOrientation);
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
        public IGameWorld gameworld;
        public IClients players;
        public ILocalPlayerPosition localplayerposition;
        #region INetworkClient Members
        public void SendCommand(byte[] cmd)
        {
            gameworld.DoCommand(cmd, 0);
        }
        #endregion
        Dictionary<int, bool> enablePlayerUpdatePosition = new Dictionary<int, bool>();
        #region INetworkClient Members
        public Dictionary<int, bool> EnablePlayerUpdatePosition { get { return enablePlayerUpdatePosition; } set { enablePlayerUpdatePosition = value; } }
        #endregion
        #region INetworkClient Members
        public string ServerName
        {
            get { return "ServerName"; }
        }
        public string ServerMotd
        {
            get { return "ServerMotd"; }
        }
        #endregion
    }
    public class ManicDiggerProgram2 : IInternetGameFactory
    {
        public string GameUrl = null;
        public string User;
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
            var clientgame = new GameFortress();
            gamedata.CurrentSeason = clientgame;
            INetworkClient network;
            if (singleplayer)
            {
                network = new NetworkClientDummyInfinite() { gameworld = clientgame };
                clientgame.Players[0] = new Player() { Name = "gamer1" };
            }
            else
            {
                network = new NetworkClientMinecraft();
            }            
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
                n.players = clientgame;
                n.localplayerposition = localplayerposition;
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
            w.weapon = new WeaponDrawer() { info = new WeaponBlockInfo() { data = gamedata, terrain = terrainDrawer, viewport = w } };
            var playerdrawer = new CharacterDrawerMonsterCode();
            playerdrawer.Load(new List<string>(File.ReadAllLines(getfile.GetFile("player.mdc"))));
            w.characterdrawer = playerdrawer;
            w.particleEffectBlockBreak = new ParticleEffectBlockBreak() { data = gamedata, map = clientgame, terrain = terrainDrawer };
            w.ENABLE_FINITEINVENTORY = true;
            clientgame.physics = physics;
            clientgame.terrain = terrainDrawer;
            clientgame.viewport = w;
            clientgame.data = gamedata;
            clientgame.network = network;
            clientgame.audio = audio;
            clientgame.zombiedrawer = new CharacterDrawerMonsterCode() { };//zombie = true };
            clientgame.getfile = getfile;
            clientgame.the3d = w;
            var gen = new WorldGeneratorSandbox();
            clientgame.generator = File.ReadAllText("WorldGenerator.cs");
            int seed = new Random().Next();
            gen.Compile(clientgame.generator, seed);
            clientgame.Seed = seed;
            clientgame.map = new InfiniteMap() { gen = gen };
            clientgame.worldgeneratorsandbox = gen;
            clientgame.minecartdrawer = new MinecartDrawer() { the3d = the3d, getfile = getfile,
                railmaputil = new RailMapUtil() { data = gamedata, mapstorage = clientgame } };
            w.game = clientgame;
            w.login = new LoginClientDummy();
            w.internetgamefactory = internetgamefactory;
            w.skinserver = "http://fragmer.net/md/skins/";
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
                    int port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerLink/Port"));
                    p.GameUrl += ":" + port;
                    p.User = XmlTool.XmlVal(d, "/ManicDiggerLink/User");
                }
            }
            p.Start();
        }
    }
}

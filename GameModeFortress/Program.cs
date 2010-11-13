using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using ManicDigger.Network;
using ManicDigger.Renderers;
using System.Threading;

namespace GameModeFortress
{
    public class NetworkClientDummyInfinite : INetworkClientFortress
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
        #region IIsChunkReady Members
        public bool IsChunkReady(int chunkx, int chunky, int chunkz)
        {
            return true;
        }
        #endregion
        #region INetworkClientFortress Members
        public void SendPacketClient(PacketClient packetClient)
        {
        }
        #endregion
    }
    public class ManicDiggerProgram2 : IInternetGameFactory, ICurrentShadows
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
            ManicDiggerProgram.exit = w;
            w.Run();
        }
        private void MakeGame(bool singleplayer)
        {
            var gamedata = new GameDataTilesManicDigger();
            var clientgame = new GameFortress();
            ICurrentSeason currentseason = clientgame;
            gamedata.CurrentSeason = currentseason;
            INetworkClientFortress network;
            if (singleplayer)
            {
                network = new NetworkClientDummyInfinite() { gameworld = clientgame };
                clientgame.Players[0] = new Player() { Name = "gamer1" };
            }
            else
            {
                network = new NetworkClientFortress();
            }            
            var mapstorage = clientgame;
            var getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var config3d = new Config3d();
            var mapManipulator = new MapManipulator();
            var terrainDrawer = new TerrainRenderer();
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
                n.players = clientgame;
                n.localplayerposition = localplayerposition;
            }
            else
            {
                var n = (NetworkClientFortress)network;
                n.Map = w;
                n.Clients = clientgame;
                n.Chatlines = w;
                n.Position = localplayerposition;
                n.ENABLE_FORTRESS = true;
                n.NetworkPacketReceived = clientgame;
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
            var blockdrawertorch = new BlockDrawerTorch();
            blockdrawertorch.terraindrawer = terrainDrawer;
            blockdrawertorch.data = gamedata;
            var terrainChunkDrawer = new TerrainChunkRenderer();
            terrainChunkDrawer.config3d = config3d;
            terrainChunkDrawer.data = gamedata;
            terrainChunkDrawer.mapstorage = clientgame;
            terrainDrawer.terrainchunkdrawer = terrainChunkDrawer;
            var frustumculling = new FrustumCulling() { the3d = the3d };
            terrainDrawer.batcher = new MeshBatcher() { frustumculling = frustumculling };
            terrainDrawer.frustumculling = frustumculling;
            w.RenderFrame += (a, b) => { frustumculling.CalcFrustumEquations(); };
            terrainChunkDrawer.blockdrawertorch = blockdrawertorch;
            terrainChunkDrawer.terrainrenderer = terrainDrawer;
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
            weapon = new WeaponBlockInfo() { data = gamedata, terrain = terrainDrawer, viewport = w, map = clientgame, shadows = shadowssimple };
            w.weapon = new WeaponRenderer() { info = weapon, blockdrawertorch = blockdrawertorch, playerpos = w };
            var playerdrawer = new CharacterRendererMonsterCode();
            playerdrawer.Load(new List<string>(File.ReadAllLines(getfile.GetFile("player.mdc"))));
            w.characterdrawer = playerdrawer;
            w.particleEffectBlockBreak = new ParticleEffectBlockBreak() { data = gamedata, map = clientgame, terrain = terrainDrawer };
            w.ENABLE_FINITEINVENTORY = false;
            clientgame.terrain = terrainDrawer;
            clientgame.viewport = w;
            clientgame.data = gamedata;
            clientgame.network = network;
            clientgame.craftingtabletool = new CraftingTableTool() { map = mapstorage };
            InfiniteMapChunked map = new InfiniteMapChunked() { generator = new WorldGeneratorDummy() };
            map.Reset(10 * 1000, 10 * 1000, 128);
            clientgame.map = map;
            terrainDrawer.ischunkready = map;
            w.game = clientgame;
            w.login = new LoginClientDummy();
            w.internetgamefactory = internetgamefactory;
            PlayerSkinDownloader playerskindownloader = new PlayerSkinDownloader();
            playerskindownloader.exit = w;
            playerskindownloader.the3d = the3d;
            playerskindownloader.skinserver = "http://fragmer.net/md/skins/";
            w.playerskindownloader = playerskindownloader;
            w.fpshistorygraphrenderer = new FpsHistoryGraphRenderer() { draw = w, viewportsize = w };
            physics.map = clientgame.mapforphysics;
            physics.data = gamedata;
            mapgenerator.data = gamedata;
            audio.getfile = getfile;
            audio.gameexit = w;
            this.clientgame = clientgame;
            this.map = map;
            w.currentshadows = this;
            shadowsfull = new Shadows() { data = gamedata, map = clientgame, terrain = terrainDrawer,
                localplayerposition = localplayerposition, config3d = config3d, ischunkready = map };
            shadowssimple = new ShadowsSimple() { data = gamedata, map = clientgame };
            if (fullshadows)
            {
                UseShadowsFull();
            }
            else
            {
                UseShadowsSimple();
            }
            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    w, audio, gamedata, clientgame, network, mapstorage, getfile,
                    config3d, mapManipulator, terrainDrawer, the3d, exit,
                    localplayerposition, worldfeatures, physics, mapgenerator,
                    internetgamefactory, blockdrawertorch, playerdrawer,
                    map, w.login, shadowsfull, shadowssimple, terrainChunkDrawer);
            }
        }
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            MakeGame(false);
        }
        #endregion
        GameFortress clientgame;
        InfiniteMapChunked map;
        ShadowsSimple shadowssimple;
        Shadows shadowsfull;
        WeaponBlockInfo weapon;
        public bool fullshadows = true;
        void UseShadowsSimple()
        {
            clientgame.shadows = shadowssimple;
            //map.shadows = clientgame.shadows;
            weapon.shadows = clientgame.shadows;
        }
        void UseShadowsFull()
        {
            clientgame.shadows = shadowsfull;
            //map.shadows = clientgame.shadows;
            weapon.shadows = clientgame.shadows;
        }
        #region ICurrentShadows Members
        public bool ShadowsFull
        {
            get
            {
                return fullshadows;
            }
            set
            {
                if (value && !fullshadows)
                {
                    UseShadowsFull();
                }
                if (!value && fullshadows)
                {
                    UseShadowsSimple();
                }
                fullshadows = value;
            }
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
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            if (!Debugger.IsAttached)
            {
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
            else
            {
                new Thread(ServerThread).Start();
                p.GameUrl = "127.0.0.1:25570";
                p.User = "Local";
            }
            p.Start();
        }
        public static IGameExit exit;
        static void ServerThread()
        {
            Server s = new Server();
            var map = new GameModeFortress.InfiniteMapChunked();
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            s.chunksize = 32;
            map.Reset(10000, 10000, 128);
            s.map = map;
            s.generator = generator;
            s.data = new GameDataTilesManicDigger();
            s.craftingtabletool = new CraftingTableTool() { map = map };
            s.LocalConnectionsOnly = true;
            s.getfile = new GetFilePath(new[] { "mine", "minecraft" });
            s.Start();
            for (; ; )
            {
                s.Process();
                Thread.Sleep(1);
                if (exit != null && exit.exit) { return; }
            }
        }
    }
}

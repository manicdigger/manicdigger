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
using ManicDiggerServer;

namespace GameModeFortress
{
    public class ManicDiggerProgram2 : IInternetGameFactory, ICurrentShadows
    {
        public string GameUrl = null;
        public string User;
        ManicDiggerGameWindow w;
        AudioOpenAl audio;
        bool IsSinglePlayer { get { return GameUrl.StartsWith("127.0.0.1"); } }
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
            var network = new NetworkClientFortress();
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
            ICompression compression = IsSinglePlayer ? (ICompression)new CompressionGzip() : new CompressionGzip();
            network.Map = w;
            network.Clients = clientgame;
            network.Chatlines = w;
            network.Position = localplayerposition;
            network.ENABLE_FORTRESS = true;
            network.NetworkPacketReceived = clientgame;
            network.compression = compression;
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
            w.BeforeRenderFrame += (a, b) => { frustumculling.CalcFrustumEquations(); };
            terrainChunkDrawer.blockdrawertorch = blockdrawertorch;
            terrainChunkDrawer.terrainrenderer = terrainDrawer;
            mapManipulator.getfile = getfile;
            mapManipulator.mapgenerator = mapgenerator;
            mapManipulator.compression = compression;
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
            clientgame.audio = audio;
            clientgame.railmaputil = new RailMapUtil() { data = gamedata, mapstorage = clientgame };
            clientgame.minecartrenderer = new MinecartRenderer() { getfile = getfile, the3d = the3d };
            InfiniteMapChunked map = new InfiniteMapChunked() { generator = new WorldGeneratorDummy() };
            var dirtychunks = new DirtyChunks() { mapstorage = map };
            terrainDrawer.ischunkready = dirtychunks;
            map.ischunkready = dirtychunks;
            map.Reset(10 * 1000, 10 * 1000, 128);
            dirtychunks.Start();
            dirtychunks.frustum = frustumculling;
            clientgame.map = map;
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
            bool IsMono = Type.GetType("Mono.Runtime") != null;
            terrainDrawer.textureatlasconverter = new TextureAtlasConverter();
            if (IsMono)
            {
                terrainDrawer.textureatlasconverter.fastbitmapfactory = () => { return new FastBitmapDummy(); };
            }
            else
            {
                terrainDrawer.textureatlasconverter.fastbitmapfactory = () => { return new FastBitmap(); };
            }
            var heightmap = new InfiniteMapChunked2d() { map = map };            
            heightmap.Restart();
            network.heightmap = heightmap;
            var light = new InfiniteMapChunkedSimple() { map = map };
            light.Restart();
            shadowsfull = new Shadows()
            {
                data = gamedata,
                map = clientgame,
                terrain = terrainDrawer,
                localplayerposition = localplayerposition,
                config3d = config3d,
                ischunkready = dirtychunks,
                heightmap = heightmap,
                light = light,
            };
            shadowssimple = new ShadowsSimple()
            {
                data = gamedata,
                map = clientgame,
                ischunkdirty = dirtychunks,
                heightmap = heightmap
            };
            this.terrainchunkrenderer = terrainChunkDrawer;
            this.network = network;
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
        TerrainChunkRenderer terrainchunkrenderer;
        public bool fullshadows = false;
        NetworkClientFortress network;
        void UseShadowsSimple()
        {
            if (clientgame.shadows != null)
            {
                shadowssimple.sunlight = clientgame.shadows.sunlight;
            }
            clientgame.shadows = shadowssimple;
            //map.shadows = clientgame.shadows;
            weapon.shadows = clientgame.shadows;
            terrainchunkrenderer.shadows = clientgame.shadows;
            network.shadows = clientgame.shadows;
        }
        void UseShadowsFull()
        {
            if (clientgame.shadows != null)
            {
                shadowsfull.sunlight = clientgame.shadows.sunlight;
            }
            clientgame.shadows = shadowsfull;
            //map.shadows = clientgame.shadows;
            weapon.shadows = clientgame.shadows;
            terrainchunkrenderer.shadows = clientgame.shadows;
            network.shadows = clientgame.shadows;
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
                new Thread(ServerThreadStart).Start();
                p.GameUrl = "127.0.0.1:25570";
                p.User = "Local";
            }
            p.Start();
        }
        public static IGameExit exit;
        static void ServerThreadStart()
        {
            try
            {
                ServerThread();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        static void ServerThread()
        {
            Server server = new Server();
            var map = new ManicDiggerServer.ServerMap();
            map.currenttime = server;
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            server.chunksize = 32;
            map.heightmap = new InfiniteMapChunked2d() { chunksize = server.chunksize, map = map };
            map.Reset(10000, 10000, 128);
            server.map = map;
            server.generator = generator;
            server.data = new GameDataTilesManicDigger();
            server.craftingtabletool = new CraftingTableTool() { map = map };
            server.LocalConnectionsOnly = true;
            server.getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var networkcompression = new CompressionGzip();
            var diskcompression = new CompressionGzip();
            var chunkdb = new ChunkDbCompressed() { chunkdb = new ChunkDbSqlite(), compression = diskcompression };
            server.chunkdb = chunkdb;
            map.chunkdb = chunkdb;
            server.networkcompression = networkcompression;
            map.data = server.data;
            server.Start();
            for (; ; )
            {
                server.Process();
                Thread.Sleep(1);
                if (exit != null && exit.exit) { return; }
            }
        }
    }
}

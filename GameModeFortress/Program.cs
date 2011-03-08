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
using ManicDigger.MapTools;
using GameMenu;
using System.Net;
using ManicDigger.Menu;

namespace GameModeFortress
{
    public class ManicDiggerProgram2 : IInternetGameFactory, ICurrentShadows, IResetMap, Game
    {
        public string GameUrl = null;
        public string User = "gamer";
        ManicDiggerGameWindow w;
        AudioOpenAl audio;
        bool IsSinglePlayer { get { return GameUrl.StartsWith("127.0.0.1"); } }
        public void Start()
        {
            ManicDiggerProgram.exit = exit;
            StartMenu();
            //if (exit.exit) { return; }
            //StartGame();
        }
        private void StartGame()
        {
            w = new ManicDiggerGameWindow();
            //maingamewindow = new MainGameWindow(w);  //done in StartMenu()
            maingamewindow.mywindow = w;

            w.mainwindow = maingamewindow;
            w.exit = exit;
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
        GetFilePath getfile = new GetFilePath(new[] { "mine", "minecraft" });
        private void StartMenu()
        {
            var ww = new GameMenu.MenuWindow();
            maingamewindow = new MainGameWindow(ww);
            ww.mainwindow = maingamewindow;

            ww.the3d = new The3d();
            ww.the3d.getfile = getfile;
            ww.the3d.config3d = new Config3d();
            ww.the3d.terrain = new TerrainDrawerDummy();
            ww.the3d.textdrawer = new ManicDigger.TextRenderer();
            var game = this;
            ww.game = game;
            ww.textrenderer = new ManicDigger.TextRenderer();
            ww.exit = exit;
            
            ww.formMainMenu = new FormMainMenu();
            ww.formMainMenu.menu = ww;
            ww.formMainMenu.Initialize();
            ww.formJoinMultiplayer = new FormJoinMultiplayer();
            ww.formJoinMultiplayer.menu = ww;
            ww.formJoinMultiplayer.game = game;
            ww.formJoinMultiplayer.Initialize();
            ww.formLogin = new FormLogin();
            ww.formLogin.menu = ww;
            ww.formLogin.game = game;
            ww.formLogin.Initialize();
            ww.formSelectWorld = new FormSelectWorld();
            ww.formSelectWorld.menu = ww;
            ww.formSelectWorld.game = game;
            ww.formSelectWorld.Initialize();
            ww.formWorldOptions = new FormWorldOptions();
            ww.formWorldOptions.menu = ww;
            ww.formWorldOptions.game = game;
            ww.formWorldOptions.Initialize();
            ww.formMessageBox = new FormMessageBox();
            ww.formMessageBox.menu = ww;
            ww.formMessageBox.game = game;
            ww.formStartServer = new FormStartServer();
            ww.formStartServer.menu = ww;
            ww.formStartServer.game = game;
            ww.formStartServer.Initialize();
            ww.formGameOptions = new FormGameOptions();
            ww.formGameOptions.menu = ww;
            ww.formGameOptions.game = game;
            ww.formGameOptions.Initialize();
            ww.formConnectToIp = new FormConnectToIp();
            ww.formConnectToIp.menu = ww;
            ww.formConnectToIp.game = game;
            ww.formConnectToIp.Initialize();

            maingamewindow.Run();
        }
        MainGameWindow maingamewindow;
        IGameExit exit = new GameExitDummy();
        private void MakeGame(bool singleplayer)
        {
            var gamedata = new GameDataTilesManicDigger();
            var clientgame = new GameFortress();
            ICurrentSeason currentseason = clientgame;
            gamedata.CurrentSeason = currentseason;
            var network = new NetworkClientFortress();
            var mapstorage = clientgame;
            var getfile = this.getfile;
            var config3d = new Config3d();
            var mapManipulator = new MapManipulator();
            var terrainDrawer = new TerrainRenderer();
            var the3d = new The3d();
            the3d.getfile = getfile;
            the3d.config3d = config3d;
            w.the3d = the3d;
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
            network.resetmap = this;
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
            var textrenderer = new ManicDigger.TextRenderer();
            w.textrenderer = textrenderer;
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
            InfiniteMapChunked map = new InfiniteMapChunked();// { generator = new WorldGeneratorDummy() };
            this.dirtychunks = new DirtyChunks() { mapstorage = map };
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
            playerskindownloader.exit = exit;
            playerskindownloader.the3d = the3d;
            playerskindownloader.skinserver = "http://fragmer.net/md/skins/";
            w.playerskindownloader = playerskindownloader;
            w.fpshistorygraphrenderer = new FpsHistoryGraphRenderer() { draw = the3d, viewportsize = w };
            physics.map = clientgame.mapforphysics;
            physics.data = gamedata;
            mapgenerator.data = gamedata;
            audio.getfile = getfile;
            audio.gameexit = exit;
            this.clientgame = clientgame;
            this.map = map;
            the3d.terrain = terrainDrawer;
            the3d.textdrawer = textrenderer;
            w.currentshadows = this;
            var sunmoonrenderer = new SunMoonRenderer() { draw2d = the3d, player = w, getfile = getfile, the3d = the3d };
            w.sunmoonrenderer = sunmoonrenderer;
            clientgame.sunmoonrenderer = sunmoonrenderer;
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
            this.heightmap = new InfiniteMapChunked2d() { map = map };
            heightmap.Restart();
            network.heightmap = heightmap;
            this.light = new InfiniteMapChunkedSimple() { map = map };
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
        InfiniteMapChunked2d heightmap;
        InfiniteMapChunkedSimple light;
        DirtyChunks dirtychunks;
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
            var shadows = shadowssimple;
            clientgame.shadows = shadows;
            //map.shadows = shadows;
            weapon.shadows = shadows;
            terrainchunkrenderer.shadows = shadows;
            network.shadows = shadows;
            w.shadows = shadows;
        }
        void UseShadowsFull()
        {
            if (clientgame.shadows != null)
            {
                shadowsfull.sunlight = clientgame.shadows.sunlight;
            }
            var shadows = shadowsfull;
            clientgame.shadows = shadows;
            //map.shadows = shadows;
            weapon.shadows = shadows;
            terrainchunkrenderer.shadows = shadows;
            network.shadows = shadows;
            w.shadows = shadows;
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
        public void Reset(int sizex, int sizey, int sizez)
        {
            map.Reset(sizex, sizey, sizez);
            light.Restart();
            heightmap.Restart();
            dirtychunks.Start();
        }
        public GameMenu.ServerInfo[] GetServers()
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false; // fixes lighthttpd 417 error in future connections
                WebClient c = new WebClient();
                string xml = c.DownloadString(ServerListAddress);
                XmlDocument d = new XmlDocument();
                d.LoadXml(xml);
                string[] allHash = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Hash")).ToArray();
                string[] allName = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Name")).ToArray();
                string[] allMotd = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/MOTD")).ToArray();
                string[] allPort = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Port")).ToArray();
                string[] allIp = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/IP")).ToArray();
                string[] allVersion = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Version")).ToArray();
                string[] allUsers = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Users")).ToArray();
                string[] allMax = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Max")).ToArray();
                string[] allGameMode = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/GameMode")).ToArray();
                string[] allPlayers = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Players")).ToArray();
                List<GameMenu.ServerInfo> l = new List<GameMenu.ServerInfo>();
                for (int i = 0; i < allHash.Length; i++)
                {
                    GameMenu.ServerInfo info = new GameMenu.ServerInfo();
                    info.Hash = allHash[i];
                    info.Name = allName[i];
                    info.Motd = allMotd[i];
                    info.Port = int.Parse(allPort[i]);
                    info.Ip = allIp[i];
                    info.Version = allVersion[i];
                    info.Users = int.Parse(allUsers[i]);
                    info.Max = int.Parse(allMax[i]);
                    info.GameMode = allGameMode[i];
                    info.Players = allPlayers[i];
                    l.Add(info);
                }
                return l.ToArray();
            }
            catch
            {
                return null;
            }
        }
        public string ServerListAddress = "http://fragmer.net/md/xml.php";
        public string[] GetWorlds()
        {
            //Todo: replace fixed slots with ability to load any files, for easy
            //copying of saves. Identify world names by filename instead of slot.txt.
            string[] w = new string[0];
            if (File.Exists(slotspath))
            {
                w = File.ReadAllLines(slotspath);
            }
            string[] worlds = new string[8];
            for (int i = 0; i < w.Length; i++)
            {
                worlds[i] = w[i];
            }
            return worlds;
        }
        public void LoginGuest(string guestlogin)
        {
            this.LoginName = guestlogin;
            IsLoggedIn = false;
        }
        public bool LoginAccount(string login, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            var data = new MdLogin().Login(login, "" + password, "a");
            if (!data.PasswordCorrect)
            {
                return false;
            }
            IsLoggedIn = true;
            this.LoginName = login;
            return true;
        }
        public void CreateAccountLogin(string createlogin, string createpassword)
        {
            this.User = createlogin;
        }
        public void StartSinglePlayer(int worldId)
        {
            GameUrl = "127.0.0.1:25570";
            User = "Local";
            string name = "default";
            if (worldId != 0)
            {
                name += worldId;
            }
            ManicDiggerProgram.SaveFilenameWithoutExtension = name;
            new Thread(ManicDiggerProgram.ServerThreadStart).Start();
            StartGame();
        }
        public void StartAndJoinLocalServer(int worldId)
        {
            //todo login and config
            GameUrl = "127.0.0.1:25565";
            User = "Local";
            string name = "default";
            if (worldId != 0)
            {
                name += worldId;
            }
            ManicDiggerProgram.SaveFilenameWithoutExtension = name;
            ManicDiggerProgram.Public = true;
            new Thread(ManicDiggerProgram.ServerThreadStart).Start();
            StartGame();
        }
        public void JoinMultiplayer(string ip, int port)
        {
            GameUrl = ip + ":" + port;
            StartGame();
        }
        string slotsdirpath = Path.Combine(GameStorePath.GetStorePath(), "Saves");
        string slotspath = Path.Combine(Path.Combine(GameStorePath.GetStorePath(), "Saves"), "slots.txt");
        public void SetWorldOptions(int worldId, string name)
        {
            string[] worlds = GetWorlds();
            worlds[worldId] = name;
            File.WriteAllLines(slotspath, worlds);
        }
        public bool IsLoggedIn { get; set; }
        public string LoginName { get { return User; } set { User = value; } }
        public void DeleteWorld(int worldId)
        {
            string name = "default";
            if (worldId != 0)
            {
                name += worldId;
            }
            name += MapManipulator.BinSaveExtension;
            File.Delete(Path.Combine(slotsdirpath, name));
            SetWorldOptions(worldId, "");
        }
    }
    public interface IResetMap
    {
        void Reset(int sizex, int sizey, int sizez);
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
                //new Thread(ServerThreadStart).Start();
                //p.GameUrl = "127.0.0.1:25570";
                //p.User = "Local";
            }
            p.Start();
        }
        public static IGameExit exit;
        public static void ServerThreadStart()
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
        public static string SaveFilenameWithoutExtension = "default";
        public static bool Public = false;
        static void ServerThread()
        {
            Server server = new Server();
            server.LoadConfig();
            var map = new ManicDiggerServer.ServerMap();
            map.currenttime = server;
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            server.chunksize = 32;
            map.heightmap = new InfiniteMapChunked2d() { chunksize = server.chunksize, map = map };
            map.Reset(server.cfgmapsizex, server.cfgmapsizey, server.cfgmapsizez);
            server.map = map;
            server.generator = generator;
            server.data = new GameDataTilesManicDigger();
            server.craftingtabletool = new CraftingTableTool() { map = map };
            server.LocalConnectionsOnly = !Public;
            server.getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var networkcompression = new CompressionGzip();
            var diskcompression = new CompressionGzip();
            var chunkdb = new ChunkDbCompressed() { chunkdb = new ChunkDbSqlite(), compression = diskcompression };
            server.chunkdb = chunkdb;
            map.chunkdb = chunkdb;
            server.networkcompression = networkcompression;
            map.data = server.data;
            server.water = new WaterFinite() { data = server.data };
            server.SaveFilenameWithoutExtension = SaveFilenameWithoutExtension;
            server.Start();
            if ((Public) && (server.cfgpublic))
            {
                new Thread((a) => { for (; ; ) { server.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            }
            for (; ; )
            {
                server.Process();
                Thread.Sleep(1);
                if (exit != null && exit.exit) { return; }
            }
        }
    }
}

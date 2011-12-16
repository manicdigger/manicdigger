#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using GameMenu;
using ManicDigger;
using ManicDigger.MapTools;
using ManicDigger.MapTools.Generators;
using ManicDigger.Menu;
using ManicDigger.Network;
using ManicDigger.Renderers;
using ManicDiggerServer;
using System.Text;
using ManicDigger.Hud;
using System.Net.Sockets;
#endregion

namespace GameModeFortress
{
    public class ManicDiggerProgram2 : ICurrentShadows, IResetMap, Game
    {
        ManicDiggerGameWindow w;
        AudioOpenAl audio = new AudioOpenAl();
        //bool IsSinglePlayer { get { return GameUrl.StartsWith("127.0.0.1"); } }
        public void Start()
        {
            string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
            getfile = new GetFileStream(datapaths);
            ManicDiggerProgram.exit = exit;
            if (connectdata.Ip == null)
            {
                StartMenu();
            }
            else
            {
                StartGame();
            }
            //if (exit.exit) { return; }
            //StartGame();
        }
        private void StartGame()
        {
            w = new ManicDiggerGameWindow();
            if (maingamewindow == null)
            {
                //wasn't done in StartMenu().
                maingamewindow = new MainGameWindow(w);
            }
            maingamewindow.mywindow = w;

            w.d_MainWindow = maingamewindow;
            w.d_Exit = exit;
            w.d_Audio = audio;
            MakeGame();
            w.connectdata = connectdata;
            w.Run();
        }
        public ConnectData connectdata = new ConnectData();
        bool IsSinglePlayer { get { return connectdata.Port == 25570; } }
        GetFileStream getfile;
        LoginDataFile logindatafile = new LoginDataFile();
        private void StartMenu()
        {
            var ww = new GameMenu.MenuWindow();
            maingamewindow = new MainGameWindow(ww);
            ww.d_MainWindow = maingamewindow;

            ww.d_The3d = new The3d();
            ww.d_The3d.d_GetFile = getfile;
            ww.d_The3d.d_Config3d = new Config3d();
            ww.d_The3d.d_Terrain = new TerrainTextures();
            ww.d_The3d.d_TextRenderer = new ManicDigger.Renderers.TextRenderer();
            var game = this;
            ww.d_Game = game;
            ww.d_TextRenderer = new ManicDigger.Renderers.TextRenderer();
            ww.d_Exit = exit;
            ww.d_Audio = audio;
            ww.d_GetFile = getfile;

            audio.d_GetFile = getfile;
            audio.d_GameExit = exit;

            ww.d_FormMainMenu = new FormMainMenu();
            ww.d_FormMainMenu.menu = ww;
            ww.d_FormMainMenu.Initialize();
            ww.d_FormJoinMultiplayer = new FormJoinMultiplayer();
            ww.d_FormJoinMultiplayer.menu = ww;
            ww.d_FormJoinMultiplayer.game = game;
            ww.d_FormJoinMultiplayer.Initialize();
            ww.d_FormLogin = new FormLogin();
            ww.d_FormLogin.menu = ww;
            ww.d_FormLogin.game = game;
            ww.d_FormLogin.logindatafile = logindatafile;
            ww.d_FormLogin.Initialize();
            ww.d_FormSelectWorld = new FormSelectWorld();
            ww.d_FormSelectWorld.menu = ww;
            ww.d_FormSelectWorld.game = game;
            ww.d_FormSelectWorld.Initialize();
            ww.d_FormWorldOptions = new FormWorldOptions();
            ww.d_FormWorldOptions.menu = ww;
            ww.d_FormWorldOptions.game = game;
            ww.d_FormWorldOptions.Initialize();
            ww.d_FormMessageBox = new FormMessageBox();
            ww.d_FormMessageBox.menu = ww;
            ww.d_FormMessageBox.game = game;
            ww.d_FormStartServer = new FormStartServer();
            ww.d_FormStartServer.menu = ww;
            ww.d_FormStartServer.game = game;
            ww.d_FormStartServer.Initialize();
            ww.d_FormGameOptions = new FormGameOptions();
            ww.d_FormGameOptions.menu = ww;
            ww.d_FormGameOptions.game = game;
            ww.d_FormGameOptions.Initialize();
            ww.d_FormConnectToIp = new FormConnectToIp();
            ww.d_FormConnectToIp.menu = ww;
            ww.d_FormConnectToIp.game = game;
            ww.d_FormConnectToIp.Initialize();

            maingamewindow.Run();
        }
        MainGameWindow maingamewindow;
        IGameExit exit = new GameExitDummy();
        private void MakeGame()
        {
            var getfile = this.getfile;
            var gamedata = new GameDataCsv();
            var clientgame = w;
            ICurrentSeason currentseason = clientgame;
            gamedata.CurrentSeason = currentseason;
            var network = w;
            var mapstorage = clientgame;
            var config3d = new Config3d();
            var mapManipulator = new MapManipulator();
            var the3d = new The3d();
            the3d.d_GetFile = getfile;
            the3d.d_Config3d = config3d;
            the3d.d_ViewportSize = w;
            w.d_The3d = the3d;
            var localplayerposition = w;
            var physics = new CharacterPhysics();
            var internetgamefactory = this;
            ICompression compression = new CompressionGzip(); //IsSinglePlayer ? (ICompression)new CompressionGzip() : new CompressionGzip();
            network.d_Compression = compression;
            network.d_ResetMap = this;
            if (IsSinglePlayer)
            {
                var socket = new SocketDummy() { network = this.dummyNetwork };
                network.main = socket;
            }
            else
            {
                network.main = new SocketNet(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            }
            var terrainTextures = new TerrainTextures();
            terrainTextures.d_GetFile = getfile;
            terrainTextures.d_The3d = the3d;
            bool IsMono = Type.GetType("Mono.Runtime") != null;
            terrainTextures.d_TextureAtlasConverter = new TextureAtlasConverter();
            if (IsMono)
            {
                terrainTextures.d_TextureAtlasConverter.d_FastBitmapFactory = () => { return new FastBitmapDummy(); };
            }
            else
            {
                terrainTextures.d_TextureAtlasConverter.d_FastBitmapFactory = () => { return new FastBitmap(); };
            }
            terrainTextures.Start();
            w.d_TerrainTextures = terrainTextures;
            var blockrenderertorch = new BlockRendererTorch();
            blockrenderertorch.d_TerainRenderer = terrainTextures;
            blockrenderertorch.d_Data = gamedata;
            //InfiniteMapChunked map = new InfiniteMapChunked();// { generator = new WorldGeneratorDummy() };
            var map = w;
            var terrainchunktesselator = new TerrainChunkTesselator();
            terrainchunktesselator.d_Config3d = config3d;
            terrainchunktesselator.d_Data = gamedata;
            terrainchunktesselator.d_MapStorage = clientgame;
            terrainchunktesselator.d_MapStoragePortion = map;
            terrainchunktesselator.d_MapStorageLight = clientgame;
            w.d_TerrainChunkTesselator = terrainchunktesselator;
            var frustumculling = new FrustumCulling() { d_GetCameraMatrix = the3d };
            w.d_Batcher = new MeshBatcher() { d_FrustumCulling = frustumculling };
            w.d_FrustumCulling = frustumculling;
            w.BeforeRenderFrame += (a, b) => { frustumculling.CalcFrustumEquations(); };
            terrainchunktesselator.d_BlockRendererTorch = blockrenderertorch;
            terrainchunktesselator.d_TerrainTextures = terrainTextures;
            //w.d_Map = clientgame.mapforphysics;
            w.d_Physics = physics;
            w.d_Clients = clientgame;
            w.d_Data = gamedata;
            w.d_DataMonsters = new GameDataMonsters(getfile);
            w.d_GetFile = getfile;
            w.d_Config3d = config3d;
            w.d_MapManipulator = mapManipulator;
            w.d_Terrain = w;
            w.PickDistance = 4.5f;
            var skysphere = new SkySphere();
            skysphere.d_MeshBatcher = new MeshBatcher() { d_FrustumCulling = new FrustumCullingDummy() };
            skysphere.d_LocalPlayerPosition = localplayerposition;
            skysphere.d_The3d = the3d;
            w.skysphere = skysphere;
            var textrenderer = new ManicDigger.Renderers.TextRenderer();
            w.d_TextRenderer = textrenderer;
            Inventory inventory = Inventory.Create();
            weapon = new WeaponBlockInfo() { d_Data = gamedata, d_Terrain = terrainTextures, d_Viewport = w, d_Map = clientgame, d_Shadows = w, d_Inventory = inventory, d_LocalPlayerPosition = w };
            w.d_Weapon = new WeaponRenderer() { d_Info = weapon, d_BlockRendererTorch = blockrenderertorch, d_LocalPlayerPosition = w };
            var playerrenderer = new CharacterRendererMonsterCode();
            playerrenderer.Load(new List<string>(MyStream.ReadAllLines(getfile.GetFile("player.mdc"))));
            w.d_CharacterRenderer = playerrenderer;
            particle = new ParticleEffectBlockBreak() { d_Data = gamedata, d_Map = clientgame, d_Terrain = terrainTextures };
            w.particleEffectBlockBreak = particle;
            w.ENABLE_FINITEINVENTORY = false;
            w.d_Shadows = w;
            clientgame.d_Terrain = w;
            clientgame.d_Data = gamedata;
            clientgame.d_CraftingTableTool = new CraftingTableTool() { d_Map = mapstorage };
            clientgame.d_Audio = audio;
            clientgame.d_RailMapUtil = new RailMapUtil() { d_Data = gamedata, d_MapStorage = clientgame };
            clientgame.d_MinecartRenderer = new MinecartRenderer() { d_GetFile = getfile, d_The3d = the3d };
            clientgame.d_TerrainTextures = terrainTextures;
            clientgame.d_GetFile = getfile;
            var craftingrecipes = new CraftingRecipes();
            craftingrecipes.data = gamedata;
            w.d_CraftingRecipes = craftingrecipes;
            network.d_CraftingRecipes = craftingrecipes;
            w.Reset(10 * 1000, 10 * 1000, 128);
            clientgame.d_Map = map;
            PlayerSkinDownloader playerskindownloader = new PlayerSkinDownloader();
            playerskindownloader.d_Exit = exit;
            playerskindownloader.d_The3d = the3d;
            playerskindownloader.skinserver = "http://fragmer.net/md/skins/";
            w.playerskindownloader = playerskindownloader;
            w.d_FpsHistoryGraphRenderer = new HudFpsHistoryGraphRenderer() { d_Draw = the3d, d_ViewportSize = w };
            w.d_Screenshot = new Screenshot() { d_GameWindow = maingamewindow };
            w.d_FrustumCulling = frustumculling;
            physics.d_Map = clientgame.mapforphysics;
            physics.d_Data = gamedata;
            audio.d_GetFile = getfile;
            audio.d_GameExit = exit;
            this.map = map;
            the3d.d_Terrain = terrainTextures;
            the3d.d_TextRenderer = textrenderer;
            w.d_CurrentShadows = this;
            var sunmoonrenderer = new SunMoonRenderer() { d_Draw2d = the3d, d_LocalPlayerPosition = w, d_GetFile = getfile, d_The3d = the3d };
            w.d_SunMoonRenderer = sunmoonrenderer;
            clientgame.d_SunMoonRenderer = sunmoonrenderer;
            this.heightmap = new InfiniteMapChunked2d() { d_Map = map };
            heightmap.Restart();
            network.d_Heightmap = heightmap;
            this.light = new InfiniteMapChunkedSimple() { d_Map = map };
            light.Restart();
            this.terrainchunktesselator = terrainchunktesselator;
            w.d_TerrainChunkTesselator = terrainchunktesselator;
            terrainchunktesselator.d_Shadows = w;
            if (fullshadows)
            {
                UseShadowsFull();
            }
            else
            {
                UseShadowsSimple();
            }
            w.d_HudChat = new ManicDigger.Gui.HudChat() { d_Draw2d = the3d, d_ViewportSize = w };

            var dataItems = new GameDataItemsBlocks() { d_Data = gamedata };
            var inventoryController = clientgame;
            var inventoryUtil = new InventoryUtil();
            var hudInventory = new HudInventory();
            hudInventory.dataItems = dataItems;
            hudInventory.inventory = inventory;
            hudInventory.inventoryUtil = inventoryUtil;
            hudInventory.controller = inventoryController;
            hudInventory.viewport_size = w;
            hudInventory.mouse_current = w;
            hudInventory.the3d = the3d;
            hudInventory.getfile = getfile;
            hudInventory.ActiveMaterial = w;
            hudInventory.viewport3d = w;
            w.d_Inventory = inventory;
            w.d_InventoryController = inventoryController;
            w.d_InventoryUtil = inventoryUtil;
            inventoryUtil.d_Inventory = inventory;
            inventoryUtil.d_Items = dataItems;

            clientgame.d_Inventory = inventory;
            w.d_HudInventory = hudInventory;
            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    w, audio, gamedata, clientgame, network, mapstorage, getfile,
                    config3d, mapManipulator, w, the3d, exit,
                    localplayerposition, physics,
                    internetgamefactory, blockrenderertorch, playerrenderer,
                    map, terrainchunktesselator);
            }
        }
        SocketDummyNetwork dummyNetwork = new SocketDummyNetwork();
        InfiniteMapChunked2d heightmap;
        InfiniteMapChunkedSimple light;
        ManicDiggerGameWindow map;
        //ShadowsSimple shadowssimple;
        //Shadows shadowsfull;
        WeaponBlockInfo weapon;
        TerrainChunkTesselator terrainchunktesselator;
        public bool fullshadows = true;
        ParticleEffectBlockBreak particle;
        void UseShadowsSimple()
        {
            /*
            if (w.d_Shadows != null)
            {
                shadowssimple.sunlight = w.d_Shadows.sunlight;
            }
            var shadows = shadowssimple;
            //map.shadows = shadows;
            weapon.d_Shadows = shadows;
            terrainchunktesselator.d_Shadows = shadows;
            w.d_Shadows = shadows;
            particle.d_Shadows = shadows;
            */
        }
        void UseShadowsFull()
        {
            /*
            if (w.d_Shadows != null)
            {
                shadowsfull.sunlight = w.d_Shadows.sunlight;
            }
            var shadows = shadowsfull;
            //map.shadows = shadows;
            weapon.d_Shadows = shadows;
            terrainchunktesselator.d_Shadows = shadows;
            w.d_Shadows = shadows;
            particle.d_Shadows = shadows;
            */
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
            //dirtychunks.Start();
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
        ILoginClient loginclient = new LoginClientManicDigger();
        public bool LoginAccount(string login, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            try
            {
                var servers = GetServers(); //workaround for login.php problem. see comment in LoginClient.
                var data = loginclient.Login(login, "" + password, servers[0].Hash);
                if (!data.PasswordCorrect)
                {
                    return false;
                }
                IsLoggedIn = true;
                this.LoginName = login;
                this.LoginPassword = password;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void StartSinglePlayer(int worldId)
        {
            connectdata.Ip = "127.0.0.1";
            connectdata.Port = 25570;
            connectdata.Username = "Local";
            string name = "default";
            if (worldId != 0)
            {
                name += worldId;
            }
            ServerProgram.SaveFilenameWithoutExtension = name;
            ServerProgram.Public = false;
            var socket = new SocketDummy(dummyNetwork);
            ServerProgram.Socket = socket;
            new Thread(ManicDiggerProgram.ServerThreadStart).Start();
            StartGame();
        }
        public void StartAndJoinLocalServer(int worldId)
        {
            //todo login and config
            connectdata.Ip = "127.0.0.1";
            connectdata.Port = 25565;
            connectdata.Username = "Local";
            string name = "default";
            if (worldId != 0)
            {
                name += worldId;
            }
            ServerProgram.SaveFilenameWithoutExtension = name;
            ServerProgram.Public = true;
            new Thread(ManicDiggerProgram.ServerThreadStart).Start();
            StartGame();
        }
        public void JoinMultiplayer(string ip, int port)
        {
            connectdata.Ip = ip;
            connectdata.Port = port;
            StartGame();
        }
        public void JoinMultiplayer(string hash)
        {
            var l = loginclient.Login(LoginName, LoginPassword, hash);
            connectdata.Ip = l.ServerAddress;
            connectdata.Port = l.Port;
            connectdata.Username = LoginName;
            connectdata.Auth = l.AuthCode;
            StartGame();
        }
        string slotsdirpath = Path.Combine(GameStorePath.GetStorePath(), "Saves");
        string slotspath = Path.Combine(Path.Combine(GameStorePath.GetStorePath(), "Saves"), "slots.txt");
        public void SetWorldOptions(int worldId, string name)
        {
            string[] worlds = GetWorlds();
            worlds[worldId] = name;
            Directory.CreateDirectory(slotsdirpath);
            File.WriteAllLines(slotspath, worlds);
        }
        public bool IsLoggedIn { get; set; }
        public string LoginName { get { return connectdata.Username; } set { connectdata.Username = value; } }
        public string LoginPassword { get; set; }
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
        public void LoadLogin()
        {
            logindatafile.Load();
            if (logindatafile.Password != "")
            {
                LoginAccount(logindatafile.LoginName, logindatafile.Password);
            }
            else if (logindatafile.LoginName != "")
            {
                LoginGuest(logindatafile.LoginName);
            }
            else
            {
                LoginGuest("gamer");
            }
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
            new CrashReporter().Start(delegate { Start(args); });
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
                    p.connectdata.Ip = XmlTool.XmlVal(d, "/ManicDiggerLink/Ip");
                    int port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerLink/Port"));
                    p.connectdata.Port = port;
                    p.connectdata.Username = XmlTool.XmlVal(d, "/ManicDiggerLink/User");
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
                ServerProgram server = new ServerProgram();
                server.d_Exit = exit;
                server.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}

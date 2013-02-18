using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using ManicDigger.Hud;
using ManicDigger.Renderers;
using OpenTK;
using ManicDigger.Gui;
using System.Drawing;
using ManicDigger.Collisions;
using ManicDigger.Network;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using GameModeFortress;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using ProtoBuf;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ManicDigger
{
    //This is the main game class.
    public partial class ManicDiggerGameWindow : IMyGameWindow, ILocalPlayerPosition, IMap,
        IAddChatLine, IWaterLevel, IMouseCurrent, IActiveMaterial, ICurrentSeason,
        IClients, IViewportSize, IViewport3dSelectedBlock,
        IMapStorage, IInventoryController, IMapStorageLight,
        IMapStoragePortion, IShadows, ICurrentShadows, IResetMap, ITerrainTextures
    {
        public void Start()
        {
            d_Audio = new AudioOpenAl();
            string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
            var getfile = new GetFileStream(datapaths);
            var w = this;
            var gamedata = new GameData();
            gamedata.Start();
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
            //network.d_ResetMap = this;
            var terrainTextures = this;
            terrainTextures.d_GetFile = getfile;
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
            terrainTextures.StartTerrainTextures();
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
            w.PickDistance = 4.5f;
            var skysphere = new SkySphere();
            skysphere.d_MeshBatcher = new MeshBatcher() { d_FrustumCulling = new FrustumCullingDummy() };
            skysphere.d_LocalPlayerPosition = localplayerposition;
            skysphere.d_The3d = the3d;
            w.skysphere = skysphere;
            var textrenderer = new ManicDigger.Renderers.TextRenderer();
            w.d_TextRenderer = textrenderer;
            Inventory inventory = Inventory.Create();
            var weapon = new WeaponBlockInfo() { d_Data = gamedata, d_Terrain = terrainTextures, d_Viewport = w, d_Map = clientgame, d_Shadows = w, d_Inventory = inventory, d_LocalPlayerPosition = w };
            w.d_Weapon = new WeaponRenderer() { d_Info = weapon, d_BlockRendererTorch = blockrenderertorch, d_LocalPlayerPosition = w };
            var playerrenderer = new CharacterRendererMonsterCode();
            playerrenderer.Load(new List<string>(MyStream.ReadAllLines(getfile.GetFile("player.txt"))));
            w.d_CharacterRenderer = playerrenderer;
            var particle = new ParticleEffectBlockBreak() { d_Data = gamedata, d_Map = clientgame, d_Terrain = terrainTextures };
            w.particleEffectBlockBreak = particle;
            w.ENABLE_FINITEINVENTORY = false;
            w.d_Shadows = w;
            clientgame.d_Data = gamedata;
            clientgame.d_CraftingTableTool = new CraftingTableTool() { d_Map = mapstorage, d_Data = gamedata };
            clientgame.d_RailMapUtil = new RailMapUtil() { d_Data = gamedata, d_MapStorage = clientgame };
            clientgame.d_MinecartRenderer = new MinecartRenderer() { d_GetFile = getfile, d_The3d = the3d };
            clientgame.d_TerrainTextures = terrainTextures;
            clientgame.d_GetFile = getfile;
            w.Reset(10 * 1000, 10 * 1000, 128);
            clientgame.d_Map = map;
            PlayerSkinDownloader playerskindownloader = new PlayerSkinDownloader();
            playerskindownloader.d_Exit = d_Exit;
            playerskindownloader.d_The3d = the3d;
            playerskindownloader.skinserver = "http://manicdigger.sourceforge.net/play/skins/";
            w.playerskindownloader = playerskindownloader;
            w.d_FpsHistoryGraphRenderer = new HudFpsHistoryGraphRenderer() { d_Draw = the3d, d_ViewportSize = w };
            w.d_Screenshot = new Screenshot() { d_GameWindow = d_GlWindow };
            w.d_FrustumCulling = frustumculling;
            physics.d_Map = clientgame.mapforphysics;
            physics.d_Data = gamedata;
            d_Audio.d_GetFile = getfile;
            d_Audio.d_GameExit = d_Exit;
            the3d.d_Terrain = terrainTextures;
            the3d.d_TextRenderer = textrenderer;
            //w.d_CurrentShadows = this;
            var sunmoonrenderer = new SunMoonRenderer() { d_Draw2d = the3d, d_LocalPlayerPosition = w, d_GetFile = getfile, d_The3d = the3d };
            w.d_SunMoonRenderer = sunmoonrenderer;
            clientgame.d_SunMoonRenderer = sunmoonrenderer;
            this.d_Heightmap = new InfiniteMapChunked2d() { d_Map = map };
            d_Heightmap.Restart();
            network.d_Heightmap = d_Heightmap;
            //this.light = new InfiniteMapChunkedSimple() { d_Map = map };
            //light.Restart();
            w.d_TerrainChunkTesselator = terrainchunktesselator;
            terrainchunktesselator.d_Shadows = w;
            /*
            if (fullshadows)
            {
                UseShadowsFull();
            }
            else
            {
                UseShadowsSimple();
            }
            */
            w.d_HudChat = new ManicDigger.Gui.HudChat() { d_Draw2d = the3d, d_ViewportSize = w };
            w.d_HudTextEditor = new HudTextEditor() { d_ViewportSize = w };
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
            hudInventory.terraintextures = d_TerrainTextures;
            w.d_Inventory = inventory;
            w.d_InventoryController = inventoryController;
            w.d_InventoryUtil = inventoryUtil;
            inventoryUtil.d_Inventory = inventory;
            inventoryUtil.d_Items = dataItems;

            clientgame.d_Inventory = inventory;
            w.d_HudInventory = hudInventory;
            w.d_CurrentShadows = this;
            w.d_ResetMap = this;
            d_The3d.currentfov = GetCurrentFov;
            crashreporter.OnCrash += new EventHandler(crashreporter_OnCrash);
            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    w, d_Audio, gamedata, clientgame, network, mapstorage, getfile,
                    config3d, mapManipulator, w, the3d, d_Exit,
                    localplayerposition, physics,
                    internetgamefactory, blockrenderertorch, playerrenderer,
                    map, terrainchunktesselator);
            }
        }

        void crashreporter_OnCrash(object sender, EventArgs e)
        {
            try
            {
                SendLeave(LeaveReason.Crash);
            }
            catch
            {
            }
        }

        void SendLeave(LeaveReason reason)
        {
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.Leave, Leave = new PacketClientLeave() { Reason = reason} }));
        }

        [Inject]
        public GlWindow d_GlWindow;
        [Inject]
        public The3d d_The3d;
        [Inject]
        public ManicDigger.Renderers.TextRenderer d_TextRenderer;
        [Inject]
        public ManicDiggerGameWindow d_Map;
        [Inject]
        public IClients d_Clients;
        [Inject]
        public CharacterPhysics d_Physics;

        [Inject]
        public AudioOpenAl d_Audio;
        [Inject]
        public GetFileStream d_GetFile;
        [Inject]
        public GameData d_Data;
        [Inject]
        public Config3d d_Config3d;
        [Inject]
        public WeaponRenderer d_Weapon;
        [Inject]
        public ICharacterRenderer d_CharacterRenderer;
        [Inject]
        public ICurrentShadows d_CurrentShadows;
        [Inject]
        public HudFpsHistoryGraphRenderer d_FpsHistoryGraphRenderer;
        [Inject]
        public MapManipulator d_MapManipulator;
        [Inject]
        public SunMoonRenderer d_SunMoonRenderer;
        [Inject]
        public IGameExit d_Exit;
        [Inject]
        public ITerrainTextures d_TerrainTextures;
        [Inject]
        public HudChat d_HudChat;
        [Inject]
        public HudTextEditor d_HudTextEditor;
        [Inject]
        public HudInventory d_HudInventory;
        [Inject]
        public Inventory d_Inventory;
        [Inject]
        public IInventoryController d_InventoryController;
        [Inject]
        public InventoryUtil d_InventoryUtil;
        [Inject]
        public IScreenshot d_Screenshot;
        [Inject]
        public CraftingTableTool d_CraftingTableTool;
        [Inject]
        public INetClient main;
        [Inject]
        public InfiniteMapChunked2d d_Heightmap;
        [Inject]
        public IResetMap d_ResetMap;
        [Inject]
        public ICompression d_Compression;
        [Inject]
        public IFrustumCulling d_FrustumCulling;
        [Inject]
        public MeshBatcher d_Batcher;
        [Inject]
        public TerrainChunkTesselator d_TerrainChunkTesselator;
        public IShadows d_Shadows;
        public CraftingRecipe[] d_CraftingRecipes;

        public bool SkySphereNight { get; set; }

        public bool IsMono = Type.GetType("Mono.Runtime") != null;
        public bool IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;

        const float rotation_speed = 180.0f * 0.05f;
        //float angle;

        public ServerInformation ServerInfo = new ServerInformation();
        public bool AllowFreemove = true;

        public void SetTileAndUpdate(Vector3 pos, int type)
        {
            //            frametickmainthreadtodo.Add(() =>
            //        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int z = (int)pos.Z;
            d_Map.SetBlock(x, y, z, type);
            RedrawBlock(x, y, z);
            //          });
        }
        public int LoadTexture(string filename)
        {
            d_The3d.d_Config3d = d_Config3d;
            return d_The3d.LoadTexture(d_GetFile.GetFile(filename));
        }
        public int LoadTexture(Bitmap bmp)
        {
            d_The3d.d_Config3d = d_Config3d;
            return d_The3d.LoadTexture(bmp);
        }
        public void OnFocusedChanged(EventArgs e)
        {
            if (guistate == GuiState.Normal)
            { EscapeMenuStart(); }
            else if (guistate == GuiState.EscapeMenu)
            { }
            else if (guistate == GuiState.Inventory)
            { }
            else if (guistate == GuiState.MapLoading)
            { }
            else if (guistate == GuiState.CraftingRecipes)
            { }
            else { throw new Exception(); }
            //..base.OnFocusedChanged(e);
        }

        public void OnLoad(EventArgs e)
        {
            if (resolutions == null)
            {
                resolutions = new List<DisplayResolution>();
                foreach (var r in DisplayDevice.Default.AvailableResolutions)
                {
                    if (r.Width < 800 || r.Height < 600 || r.BitsPerPixel < 16)
                    {
                        continue;
                    }
                    resolutions.Add(r);
                }
            }
            try
            {
                GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
            }
            catch
            {
                maxTextureSize = 1024;
            }
            if (maxTextureSize < 1024)
            {
                maxTextureSize = 1024;
            }
            //Start();
            //Connect();
            MapLoadingStart();

            string version = GL.GetString(StringName.Version);
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major <= 1 && minor < 5)
            {
                System.Windows.Forms.MessageBox.Show("You need at least OpenGL 1.5 to run this example. Aborting.", "VBOs not supported",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                d_GlWindow.Exit();
            }
            if (!d_Config3d.ENABLE_VSYNC)
            {
                d_GlWindow.TargetRenderFrequency = 0;
            }
            GL.ClearColor(Color.Black);
            d_GlWindow.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            d_GlWindow.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            d_GlWindow.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            d_GlWindow.Mouse.WheelChanged += new EventHandler<OpenTK.Input.MouseWheelEventArgs>(Mouse_WheelChanged);
            if (d_Config3d.ENABLE_BACKFACECULLING)
            {
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
            }
            d_GlWindow.Keyboard.KeyRepeat = true;
            d_GlWindow.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(ManicDiggerGameWindow_KeyPress);
            d_GlWindow.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            d_GlWindow.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);

            GL.Enable(EnableCap.Lighting);
            //SetAmbientLight(terraincolor);
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
            GL.ShadeModel(ShadingModel.Smooth);
            if (!IsMac)
            {
                System.Windows.Forms.Cursor.Hide();
            }
            else
            {
                d_GlWindow.CursorVisible = false;
            }
        }
        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!d_GlWindow.Focused)
            {
                return;
            }
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                mouseleftdeclick = true;
            }
            if (e.Button == OpenTK.Input.MouseButton.Right)
            {
                mouserightdeclick = true;
            }
            if (guistate == GuiState.Normal)
            {
                UpdatePicking();
            }
            if (guistate == GuiState.Inventory)
            {
                d_HudInventory.Mouse_ButtonUp(sender, e);
            }
        }
        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!d_GlWindow.Focused)
            {
                return;
            }
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                mouseleftclick = true;
            }
            if (e.Button == OpenTK.Input.MouseButton.Right)
            {
                mouserightclick = true;
            }
            if (guistate == GuiState.Normal)
            {
                UpdatePicking();
            }
            if (guistate == GuiState.Inventory)
            {
                d_HudInventory.Mouse_ButtonDown(sender, e);
            }
        }
        void ManicDiggerGameWindow_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            if (KeyIsEqualChar(OpenTK.Input.Key.T, e.KeyChar) && GuiTyping == TypingState.None)
            {
                GuiTyping = TypingState.Typing;
                d_HudChat.GuiTypingBuffer = "";
                d_HudChat.IsTeamchat = false;
                return;
            }
            if (KeyIsEqualChar(OpenTK.Input.Key.Y, e.KeyChar) && GuiTyping == TypingState.None)
            {
                GuiTyping = TypingState.Typing;
                d_HudChat.GuiTypingBuffer = "";
                d_HudChat.IsTeamchat = true;
                return;
            }
            if (GuiTyping == TypingState.Typing)
            {
                char c = e.KeyChar;
                if ((char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)
                    || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
                    && c != '\r' && c != '\t')
                {
                    d_HudChat.GuiTypingBuffer += e.KeyChar;
                }
                if (c == '\t' && d_HudChat.GuiTypingBuffer.Trim() != "")
                {
                    foreach (var k in players)
                    {
                        if (k.Value.Type != PlayerType.Player)
                        {
                            continue;
                        }
                        if (k.Value.Name.StartsWith(d_HudChat.GuiTypingBuffer, StringComparison.InvariantCultureIgnoreCase))
                        {
                            d_HudChat.GuiTypingBuffer = k.Value.Name + ": ";
                            break;
                        }
                    }
                }
            }
            foreach (var d in dialogs)
            {
                foreach (var w in d.Value.Widgets)
                {
                    if (("abcdefghijklmnopqrstuvwxyz1234567890\t " + (char)27).Contains("" + w.ClickKey))
                    {
                        if (e.KeyChar == w.ClickKey)
                        {
                            SendPacketClient(new PacketClient() { PacketId = ClientPacketId.DialogClick, DialogClick = new PacketClientDialogClick() { WidgetId = w.Id } });
                            return;
                        }
                    }
                }
            }
            if (guistate == GuiState.EditText)
            {
                d_HudTextEditor.HandleKeyPress(sender, e);
            }
        }
        float overheadcameradistance = 10;
        float tppcameradistance = 3;
        void Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            if (keyboardstate[GetKey(OpenTK.Input.Key.LControl)])
            {
                if (cameratype == CameraType.Overhead)
                {
                    overheadcameradistance -= e.DeltaPrecise;
                    if (overheadcameradistance < TPP_CAMERA_DISTANCE_MIN) { overheadcameradistance = TPP_CAMERA_DISTANCE_MIN; }
                    if (overheadcameradistance > TPP_CAMERA_DISTANCE_MAX) { overheadcameradistance = TPP_CAMERA_DISTANCE_MAX; }
                }
                if (cameratype == CameraType.Tpp)
                {
                    tppcameradistance -= e.DeltaPrecise;
                    if (tppcameradistance < TPP_CAMERA_DISTANCE_MIN) { tppcameradistance = TPP_CAMERA_DISTANCE_MIN; }
                    if (tppcameradistance > TPP_CAMERA_DISTANCE_MAX) { tppcameradistance = TPP_CAMERA_DISTANCE_MAX; }
                }
            }
        }
        public int TPP_CAMERA_DISTANCE_MIN = 1;
        public int TPP_CAMERA_DISTANCE_MAX = 10;
        private static void SetAmbientLight(Color c)
        {
            float mult = 1f;
            float[] global_ambient = new float[] { (float)c.R / 255f * mult, (float)c.G / 255f * mult, (float)c.B / 255f * mult, 1f };
            GL.LightModel(LightModelParameter.LightModelAmbient, global_ambient);
        }
        public void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SendLeave(LeaveReason.Leave);
        }
        public void OnClosed(EventArgs e)
        {
            d_Exit.exit = true;
            //..base.OnClosed(e);
        }
        void ClientCommand(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("."))
            {
                string strFreemoveNotAllowed = Language.FreemoveNotAllowed;
                try
                {
                    string cmd = ss[0].Substring(1);
                    string arguments;
                    if (s.IndexOf(" ") == -1)
                    { arguments = ""; }
                    else
                    { arguments = s.Substring(s.IndexOf(" ")); }
                    arguments = arguments.Trim();
                    if (cmd == "fps")
                    {
                        ENABLE_DRAWFPS = BoolCommandArgument(arguments) || arguments.Trim() == "2";
                        ENABLE_DRAWFPSHISTORY = arguments.Trim() == "2";
                    }
                    if (cmd == "pos")
                    {
                        ENABLE_DRAWPOSITION = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "fog")
                    {
                        int foglevel;
                        foglevel = int.Parse(arguments);
                        //if (foglevel <= 16)
                        //{
                        //    terrain.DrawDistance = (int)Math.Pow(2, foglevel);
                        //}
                        //else
                        {
                            int foglevel2 = foglevel;
                            if (foglevel2 > 1024)
                            {
                                foglevel2 = 1024;
                            }
                            if (foglevel2 % 2 == 0)
                            {
                                foglevel2--;
                            }
                            d_Config3d.viewdistance = foglevel2;
                            //terrain.UpdateAllTiles();
                        }
                        OnResize(new EventArgs());
                    }
                    else if (cmd == "noclip")
                    {
                        ENABLE_NOCLIP = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "freemove")
                    {
                        if (this.AllowFreemove)
                        {
                            ENABLE_FREEMOVE = BoolCommandArgument(arguments);
                        }
                        else
                        {
                            Log(strFreemoveNotAllowed);
                            return;
                        }
                    }
                    else if (cmd == "fov")
                    {
                        int arg = int.Parse(arguments);
                        int minfov = 1;
                        int maxfov = 179;
                        if (!issingleplayer)
                        {
                            minfov = 60;
                        }
                        if (arg < minfov || arg > maxfov)
                        {
                            throw new Exception(string.Format("Valid field of view: {0}-{1}", minfov, maxfov));
                        }
                        float fov = (float)(2 * Math.PI * ((float)arg / 360));
                        d_The3d.fov = fov;
                        OnResize(new EventArgs());
                    }
                    else if (cmd == "clients")
                    {
                        Log ("Clients:");
                        foreach (var k in d_Clients.Players)
                        {
                            Log (string.Format("{0} {1}", k.Key, k.Value.Name));
                        }
                    }
                    else if (cmd == "movespeed")
                    {
                        try
                        {
                            if (this.AllowFreemove)
                            {
                                if (float.Parse(arguments) <= 500)
                                {
                                    movespeed = basemovespeed * float.Parse(arguments);
                                    AddChatline("Movespeed: " + arguments + "x");
                                }
                                else
                                {
                                    AddChatline("Entered movespeed to high! max. 500x");
                                }
                            }
                            else
                            {
                                Log(strFreemoveNotAllowed);
                                return;
                            }
                        }
                        catch
                        {
                            AddChatline("Invalid value!");
                            AddChatline("USE: .movespeed [movespeed]");
                        }
                    }
                    else if (cmd == "testmodel")
                    {
                        ENABLE_DRAW_TEST_CHARACTER = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "simulationlag")
                    {
                        SIMULATIONLAG_SECONDS = double.Parse(arguments);
                    }
                    else if (cmd == "gui")
                    {
                        ENABLE_DRAW2D = BoolCommandArgument(arguments);
                    }
                    else
                    {
                        string chatline = d_HudChat.GuiTypingBuffer.Substring(0, Math.Min(d_HudChat.GuiTypingBuffer.Length, 256));
                        SendChat(chatline);
                    }
                }
                catch (Exception e) { AddChatline(new StringReader(e.Message).ReadLine()); }
            }
            else
            {
                string chatline = d_HudChat.GuiTypingBuffer.Substring(0, Math.Min(d_HudChat.GuiTypingBuffer.Length, 4096));
                SendChat(chatline);

            }
        }
        private static bool BoolCommandArgument(string arguments)
        {
            arguments = arguments.Trim();
            return (arguments == "" || arguments == "1" || arguments == "on" || arguments == "yes");
        }

        private float GetCurrentFov()
        {
            if (IronSights)
            {
                Item item = d_Inventory.RightHand[ActiveMaterial];
                if (item != null && item.ItemClass == ItemClass.Block)
                {
                    if (blocktypes[item.BlockId].IronSightsFov != 0)
                    {
                        return d_The3d.fov * blocktypes[item.BlockId].IronSightsFov;
                    }
                }
            }
            return d_The3d.fov;
        }

        OpenTK.Input.KeyboardKeyEventArgs keyevent;
        OpenTK.Input.KeyboardKeyEventArgs keyeventup;
        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == GetKey(OpenTK.Input.Key.ShiftLeft) || e.Key == GetKey(OpenTK.Input.Key.ShiftRight))
                IsShiftPressed = false;
            if (GuiTyping == TypingState.None)
            {
                keyeventup = e;
            }
            if (guistate == GuiState.EditText)
            {
                d_HudTextEditor.HandleKeyUp(sender, e);
            }
        }
        bool IsShiftPressed = false;
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == GetKey(OpenTK.Input.Key.ShiftLeft) || e.Key == GetKey(OpenTK.Input.Key.ShiftRight))
                IsShiftPressed = true;
            if (e.Key == GetKey(OpenTK.Input.Key.F11))
            {
                if (d_GlWindow.WindowState == WindowState.Fullscreen)
                {
                    d_GlWindow.WindowState = WindowState.Normal;
                    RestoreResolution();
                    SaveOptions();
                }
                else
                {
                    d_GlWindow.WindowState = WindowState.Fullscreen;
                    UseResolution();
                    SaveOptions();
                }
            }
            if (GuiTyping == TypingState.None)
            {
                keyevent = e;
            }
            if (guistate == GuiState.Normal)
            {
                if (Keyboard[GetKey(OpenTK.Input.Key.Escape)])
                {
                    foreach (var k in new Dictionary<string, Dialog>(dialogs))
                    {
                        if (k.Value.IsModal)
                        {
                            dialogs.Remove(k.Key);
                            return;
                        }
                    }
                    guistate = GuiState.EscapeMenu;
                    menustate = new MenuState();
                    FreeMouse = true;
                }
                if (e.Key == GetKey(OpenTK.Input.Key.Number7) && IsShiftPressed && GuiTyping == TypingState.None) // don't need to hit enter for typing commands starting with slash
                {
                    GuiTyping = TypingState.Typing;
                    d_HudChat.IsTyping = true;
                    d_HudChat.GuiTypingBuffer = "";
                    d_HudChat.IsTeamchat = false;
                    return;
                }
                if (e.Key == GetKey(OpenTK.Input.Key.PageUp) && GuiTyping == TypingState.Typing)
                {
                    d_HudChat.ChatPageScroll++;
                }
                if (e.Key == GetKey(OpenTK.Input.Key.PageDown) && GuiTyping == TypingState.Typing)
                {
                    d_HudChat.ChatPageScroll--;
                }
                d_HudChat.ChatPageScroll = MyMath.Clamp(d_HudChat.ChatPageScroll, 0, d_HudChat.ChatLines.Count / d_HudChat.ChatLinesMaxToDraw);
                if (e.Key == GetKey(OpenTK.Input.Key.Enter) || e.Key == GetKey(OpenTK.Input.Key.KeypadEnter))
                {
                    if (GuiTyping == TypingState.Typing)
                    {
                        typinglog.Add(d_HudChat.GuiTypingBuffer);
                        typinglogpos = typinglog.Count;
                        ClientCommand(d_HudChat.GuiTypingBuffer);

                        d_HudChat.GuiTypingBuffer = "";
                        d_HudChat.IsTyping = false;

                        GuiTyping = TypingState.None;
                    }
                    else if (GuiTyping == TypingState.None)
                    {
                        GuiTyping = TypingState.Typing;
                        d_HudChat.IsTyping = true;
                        d_HudChat.GuiTypingBuffer = "";
                        d_HudChat.IsTeamchat = false;
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
                    if (key == GetKey(OpenTK.Input.Key.BackSpace))
                    {
                        if (d_HudChat.GuiTypingBuffer.Length > 0)
                        {
                            d_HudChat.GuiTypingBuffer = d_HudChat.GuiTypingBuffer.Substring(0, d_HudChat.GuiTypingBuffer.Length - 1);
                        }
                        return;
                    }
                    if (Keyboard[GetKey(OpenTK.Input.Key.ControlLeft)] || Keyboard[GetKey(OpenTK.Input.Key.ControlRight)])
                    {
                        if (key == GetKey(OpenTK.Input.Key.V))
                        {
                            if (Clipboard.ContainsText())
                            {
                                d_HudChat.GuiTypingBuffer += Clipboard.GetText();
                            }
                            return;
                        }
                    }
                    if (key == GetKey(OpenTK.Input.Key.Up))
                    {
                        typinglogpos--;
                        if (typinglogpos < 0) { typinglogpos = 0; }
                        if (typinglogpos >= 0 && typinglogpos < typinglog.Count)
                        {
                            d_HudChat.GuiTypingBuffer = typinglog[typinglogpos];
                        }
                    }
                    if (key == GetKey(OpenTK.Input.Key.Down))
                    {
                        typinglogpos++;
                        if (typinglogpos > typinglog.Count) { typinglogpos = typinglog.Count; }
                        if (typinglogpos >= 0 && typinglogpos < typinglog.Count)
                        {
                            d_HudChat.GuiTypingBuffer = typinglog[typinglogpos];
                        }
                        if (typinglogpos == typinglog.Count)
                        {
                            d_HudChat.GuiTypingBuffer = "";
                        }
                    }
                    return;
                }

                string strFreemoveNotAllowed = "You are not allowed to enable freemove.";

                if (e.Key == GetKey(OpenTK.Input.Key.F1))
                {
                    if (!this.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    movespeed = basemovespeed * 1;
                    Log("Move speed: 1x.");
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F2))
                {
                    if (!this.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    movespeed = basemovespeed * 10;
                    Log(string.Format(Language.MoveSpeed, 10.ToString()));
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F3))
                {
                    if (!this.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    player.movedz = 0;
                    if (!ENABLE_FREEMOVE)
                    {
                        ENABLE_FREEMOVE = true;
                        Log(Language.MoveFree);
                    }
                    else if (ENABLE_FREEMOVE && (!ENABLE_NOCLIP))
                    {
                        ENABLE_NOCLIP = true;
                        Log(Language.MoveFreeNoclip);
                    }
                    else if (ENABLE_FREEMOVE && ENABLE_NOCLIP)
                    {
                        ENABLE_FREEMOVE = false;
                        ENABLE_NOCLIP = false;
                        Log(Language.MoveNormal);
                    }
                }
                if (e.Key == GetKey(OpenTK.Input.Key.I))
                {
                    drawblockinfo = !drawblockinfo;
                }
                    performanceinfo["height"]="height:"+d_Heightmap.GetBlock((int)player.playerposition.X, (int)player.playerposition.Z);
                if (e.Key == GetKey(OpenTK.Input.Key.F5))
                {
                    if (cameratype == CameraType.Fpp)
                    {
                        cameratype = CameraType.Tpp;
                        ENABLE_TPP_VIEW = true;
                    }
                    else if (cameratype == CameraType.Tpp)
                    {
                        cameratype = CameraType.Overhead;
                        overheadcamera = true;
                        FreeMouse = true;
                        ENABLE_TPP_VIEW = true;
                        playerdestination = player.playerposition;
                    }
                    else if (cameratype == CameraType.Overhead)
                    {
                        cameratype = CameraType.Fpp;
                        FreeMouse = false;
                        ENABLE_TPP_VIEW = false;
                        overheadcamera = false;
                    }
                    else throw new Exception();
                }
                if (e.Key == GetKey(OpenTK.Input.Key.Plus) || e.Key == GetKey(OpenTK.Input.Key.KeypadPlus))
                {
                    if (cameratype == CameraType.Overhead)
                    {
                        overheadcameradistance -= 1;
                    }
                    else if (cameratype == CameraType.Tpp)
                    {
                        tppcameradistance -= 1;
                    }
                }
                if (e.Key == GetKey(OpenTK.Input.Key.Minus) || e.Key == GetKey(OpenTK.Input.Key.KeypadMinus))
                {
                    if (cameratype == CameraType.Overhead)
                    {
                        overheadcameradistance += 1;
                    }
                    else if (cameratype == CameraType.Tpp)
                    {
                        tppcameradistance += 1;
                    }
                }
                if (overheadcameradistance < TPP_CAMERA_DISTANCE_MIN) { overheadcameradistance = TPP_CAMERA_DISTANCE_MIN; }
                if (overheadcameradistance > TPP_CAMERA_DISTANCE_MAX) { overheadcameradistance = TPP_CAMERA_DISTANCE_MAX; }

                if (tppcameradistance < TPP_CAMERA_DISTANCE_MIN) { tppcameradistance = TPP_CAMERA_DISTANCE_MIN; }
                if (tppcameradistance > TPP_CAMERA_DISTANCE_MAX) { tppcameradistance = TPP_CAMERA_DISTANCE_MAX; }

                if (e.Key == GetKey(OpenTK.Input.Key.F6))
                {
                    RedrawAllBlocks();
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F7))
                {
                    if (ENABLE_DRAWFPSHISTORY)
                    {
                        ENABLE_DRAWFPS = ENABLE_DRAWFPSHISTORY = false;
                    }
                    else
                    {
                        ENABLE_DRAWFPS = ENABLE_DRAWFPSHISTORY = true;
                    }
                }
                if (e.Key == OpenTK.Input.Key.F8)
                {
                    ToggleVsync();
                    if (ENABLE_LAG == 0) { Log(Language.FrameRateVsync); }
                    if (ENABLE_LAG == 1) { Log(Language.FrameRateUnlimited); }
                    if (ENABLE_LAG == 2) { Log(Language.FrameRateLagSimulation); }
                }
                if (e.Key == OpenTK.Input.Key.F9)
                {
                    guistate = GuiState.EditText;
                    FreeMouse = true;
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F12))
                {
                    d_Screenshot.SaveScreenshot();
                    screenshotflash = 5;
                }
                if (e.Key == GetKey(OpenTK.Input.Key.Tab))
                {
                    SendPacketClient(new PacketClient()
                    {
                        PacketId = ClientPacketId.SpecialKey,
                        SpecialKey = new PacketClientSpecialKey() { key = ManicDigger.SpecialKey.TabPlayerList },
                    });
                }
                if (e.Key == GetKey(OpenTK.Input.Key.E))
                {
                    if (currentAttackedBlock != null)
                    {
                        Vector3 pos = new Vector3(currentAttackedBlock.Value.x, currentAttackedBlock.Value.y, currentAttackedBlock.Value.z);
                        int blocktype = d_Map.GetBlock(currentAttackedBlock.Value.x, currentAttackedBlock.Value.y, currentAttackedBlock.Value.z);
                        if (IsUsableBlock(blocktype))
                        {
                            if (d_Data.IsRailTile(blocktype))
                            {
                                player.playerposition.X = pos.X + .5f;
                                player.playerposition.Y = pos.Z + 1;
                                player.playerposition.Z = pos.Y + .5f;
                                ENABLE_FREEMOVE = false;
                            }
                            else
                            {
                                SendSetBlock(pos, BlockSetMode.Use, 0, ActiveMaterial);
                            }
                        }
                    }
                }
                if (e.Key == GetKey(OpenTK.Input.Key.R))
                {
                    Item item = d_Inventory.RightHand[ActiveMaterial];
                    if (item != null && item.ItemClass == ItemClass.Block
                        && blocktypes[item.BlockId].IsPistol
                        && reloadstart.Ticks == 0)
                    {
                        int sound = rnd.Next(blocktypes[item.BlockId].Sounds.Reload.Length);
                        d_Audio.Play(blocktypes[item.BlockId].Sounds.Reload[sound] + ".ogg");
                        reloadstart = DateTime.UtcNow;
                        reloadblock = item.BlockId;
                        SendPacketClient(new PacketClient() { PacketId = ClientPacketId.Reload, Reload = new PacketClientReload() });
                    }
                }
                if (e.Key == GetKey(OpenTK.Input.Key.O))
                {
                    Respawn();
                }
                if (e.Key == GetKey(OpenTK.Input.Key.L))
                {
                    SendPacketClient(new PacketClient()
                    {
                        PacketId = ClientPacketId.SpecialKey,
                        SpecialKey = new PacketClientSpecialKey() { key = ManicDigger.SpecialKey.SelectTeam },
                    });
                }
                if (e.Key == GetKey(OpenTK.Input.Key.P))
                {
                    SendPacketClient(new PacketClient()
                    {
                        PacketId = ClientPacketId.SpecialKey,
                        SpecialKey = new PacketClientSpecialKey() { key = ManicDigger.SpecialKey.SetSpawn },
                    });
                    PlayerPositionSpawn = player.playerposition;
                    player.playerposition = new Vector3((int)player.playerposition.X + 0.5f, player.playerposition.Y, (int)player.playerposition.Z + 0.5f);
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F))
                {
                    ToggleFog();
                    Log(string.Format(Language.FogDistance, d_Config3d.viewdistance));
                    OnResize(new EventArgs());
                }
                if (e.Key == GetKey(OpenTK.Input.Key.B))
                {
                    guistate = GuiState.Inventory;
                    menustate = new MenuState();
                    FreeMouse = true;
                }
                HandleMaterialKeys(e);
                if (e.Key == GetKey(OpenTK.Input.Key.Escape))
                {
                    EscapeMenuStart();
                }
            }
            else if (guistate == GuiState.EscapeMenu)
            {
                EscapeMenuKeyDown(e);
                return;
            }
            else if (guistate == GuiState.Inventory)
            {
                if (e.Key == GetKey(OpenTK.Input.Key.B)
                    || e.Key == GetKey(OpenTK.Input.Key.Escape))
                {
                    GuiStateBackToGame();
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F12))
                {
                    d_Screenshot.SaveScreenshot();
                    screenshotflash = 5;
                }
                return;
            }
            else if (guistate == GuiState.ModalDialog)
            {
                if (e.Key == GetKey(OpenTK.Input.Key.B)
                    || e.Key == GetKey(OpenTK.Input.Key.Escape))
                {
                    GuiStateBackToGame();
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F12))
                {
                    d_Screenshot.SaveScreenshot();
                    screenshotflash = 5;
                }
            }
            else if (guistate == GuiState.MapLoading)
            {
            }
            else if (guistate == GuiState.CraftingRecipes)
            {
                if (e.Key == GetKey(OpenTK.Input.Key.Escape))
                {
                    GuiStateBackToGame();
                }
            }
            else if (guistate == GuiState.EditText)
            {
                if (e.Key == GetKey(OpenTK.Input.Key.Escape))
                {
                    GuiStateBackToGame();
                }
                d_HudTextEditor.HandleKeyDown(sender, e);
            }
            else throw new Exception();
        }

        public void SetCamera(CameraType type)
        {
            if (type == CameraType.Fpp)
            {
                cameratype = CameraType.Fpp;
                FreeMouse = false;
                ENABLE_TPP_VIEW = false;
                overheadcamera = false;
            }
            else if (type == CameraType.Tpp)
            {
                cameratype = CameraType.Tpp;
                ENABLE_TPP_VIEW = true;
            }
            else
            {
                cameratype = CameraType.Overhead;
                overheadcamera = true;
                FreeMouse = true;
                ENABLE_TPP_VIEW = true;
                playerdestination = player.playerposition;
            }
        }

        private void ToggleVsync()
        {
            ENABLE_LAG++;
            ENABLE_LAG = ENABLE_LAG % 3;
            UseVsync();
        }

        private void UseVsync()
        {
            d_GlWindow.VSync = (ENABLE_LAG == 1) ? VSyncMode.Off : VSyncMode.On;
        }
        int maxdrawdistance; 
        private void ToggleFog()
        {
            List<int> drawDistances = new List<int>();
            drawDistances.Add(32);
            if (maxdrawdistance >= 64) { drawDistances.Add(64); }
            if (maxdrawdistance >= 128) { drawDistances.Add(128); }
            if (maxdrawdistance >= 256) { drawDistances.Add(256); }
            if (maxdrawdistance >= 512) { drawDistances.Add(512); }
            for (int i = 0; i < drawDistances.Count; i++)
            {
                if (d_Config3d.viewdistance == drawDistances[i])
                {
                    d_Config3d.viewdistance = drawDistances[(i + 1) % drawDistances.Count];
                    goto end;
                }
            }
            d_Config3d.viewdistance = drawDistances[0];
        end:
            StartTerrain();
        }
        public enum CameraType
        {
            Fpp,
            Tpp,
            Overhead,
        }
        CameraType cameratype = CameraType.Fpp;
        public void Log(string p)
        {
            AddChatline(p);
        }
        public void HandleMaterialKeys(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == GetKey(OpenTK.Input.Key.Number1)) { ActiveMaterial = 0; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number2)) { ActiveMaterial = 1; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number3)) { ActiveMaterial = 2; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number4)) { ActiveMaterial = 3; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number5)) { ActiveMaterial = 4; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number6)) { ActiveMaterial = 5; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number7)) { ActiveMaterial = 6; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number8)) { ActiveMaterial = 7; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number9)) { ActiveMaterial = 8; }
            if (e.Key == GetKey(OpenTK.Input.Key.Number0)) { ActiveMaterial = 9; }
        }
        List<string> typinglog = new List<string>();
        int typinglogpos = 0;
        public void GuiStateBackToGame()
        {
            guistate = GuiState.Normal;
            FreeMouse = false;
            freemousejustdisabled = true;
        }
        bool freemousejustdisabled;
        enum TypingState { None, Typing, Ready };
        TypingState GuiTyping = TypingState.None;

        public ConnectData connectdata;
        public CrashReporter crashreporter;
        public bool issingleplayer;
        public bool StartedSinglePlayerServer;
        private void Connect()
        {
            LoadOptions();
            MapLoaded += new EventHandler<MapLoadedEventArgs>(network_MapLoaded);
            MapLoadingProgress += new EventHandler<MapLoadingProgressEventArgs>(newnetwork_MapLoadingProgress);

            while (issingleplayer && !StartedSinglePlayerServer)
            {
                Thread.Sleep(1);
            }

            if (string.IsNullOrEmpty(connectdata.ServerPassword))
            {
                Connect(connectdata.Ip, connectdata.Port, connectdata.Username, connectdata.Auth);
            }
            else
            {
                Connect(connectdata.Ip, connectdata.Port, connectdata.Username, connectdata.Auth, connectdata.ServerPassword);
            }
            MapLoadingStart();
        }
        void newnetwork_MapLoadingProgress(object sender, MapLoadingProgressEventArgs e)
        {
            this.maploadingprogress = e;
        }
        void network_MapLoaded(object sender, MapLoadedEventArgs e)
        {
            StartTerrain();
            materialSlots = d_Data.DefaultMaterialSlots;
            GuiStateBackToGame();
            OnNewMap();
        }
        //[Obsolete]
        int[] materialSlots;
        //[Obsolete]
        public int[] MaterialSlots
        {
            get
            {
                //return materialSlots;
                int[] m = new int[10];
                for (int i = 0; i < 10; i++)
                {
                    Item item = d_Inventory.RightHand[i];
                    m[i] = d_Data.BlockIdDirt;
                    if (item != null && item.ItemClass == ItemClass.Block)
                    {
                        m[i] = d_Inventory.RightHand[i].BlockId;
                    }
                }
                return m;
            }
            set
            {
                materialSlots = value;
            }
        }
        public void OnResize(EventArgs e)
        {
            //.mainwindow.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
            d_The3d.Set3dProjection();
        }
        Vector3 up = new Vector3(0f, 1f, 0f);
        Point mouse_current, mouse_previous;
        PointF mouse_delta;
        bool freemouse;
        bool FreeMouse
        {
            get
            {
                if (overheadcamera)
                {
                    return true;
                }
                return freemouse;
            }
            set
            {
                if (IsMac)
                {
                    d_GlWindow.CursorVisible = value;
                    System.Windows.Forms.Cursor.Hide();
                }
                freemouse = value;
            }
        }
        void UpdateMouseButtons()
        {
            if (!d_GlWindow.Focused)
            {
                return;
            }
            mouseleftclick = (!wasmouseleft) && Mouse[OpenTK.Input.MouseButton.Left];
            mouserightclick = (!wasmouseright) && Mouse[OpenTK.Input.MouseButton.Right];
            mouseleftdeclick = wasmouseleft && (!Mouse[OpenTK.Input.MouseButton.Left]);
            mouserightdeclick = wasmouseright && (!Mouse[OpenTK.Input.MouseButton.Right]);
            wasmouseleft = Mouse[OpenTK.Input.MouseButton.Left];
            wasmouseright = Mouse[OpenTK.Input.MouseButton.Right];
        }
        void UpdateMousePosition()
        {
            mouse_current = System.Windows.Forms.Cursor.Position;
            if (FreeMouse)
            {
                mouse_current.Offset(-d_GlWindow.X, -d_GlWindow.Y);
                mouse_current.Offset(0, -System.Windows.Forms.SystemInformation.CaptionHeight);
            }
            if (!d_GlWindow.Focused)
            {
                return;
            }
            if (freemousejustdisabled)
            {
                mouse_previous = mouse_current;
                freemousejustdisabled = false;
            }
            if (!FreeMouse)
            {
                //There are two versions:

                //a) System.Windows.Forms.Cursor and GameWindow.CursorVisible = true.
                //It works by centering global mouse cursor every frame.
                //*Windows:
                //   *OK.
                //   *On a few YouTube videos mouse cursor is not hiding properly.
                //    That could be just a problem with video recording software.
                //*Ubuntu: Broken, mouse cursor doesn't hide.
                //*Mac: Broken, mouse doesn't move at all.

                //b) OpenTk.Input.Mouse and GameWindow.CursorVisible = false.
                //Uses raw mouse coordinates, movement is not accelerated.
                //*Windows:
                //  *OK.
                //  *Worse than a), because this doesn't use system-wide mouse acceleration.
                //*Ubuntu: Broken, crashes with "libxi" library missing.
                //*Mac: OK.

                if (!IsMac)
                {
                    //a)
                    int centerx = d_GlWindow.Bounds.Left + (d_GlWindow.Bounds.Width / 2);
                    int centery = d_GlWindow.Bounds.Top + (d_GlWindow.Bounds.Height / 2);

                    mouse_delta = new Point(mouse_current.X - mouse_previous.X,
                        mouse_current.Y - mouse_previous.Y);

                    System.Windows.Forms.Cursor.Position =
                        new Point(centerx, centery);
                    mouse_previous = new Point(centerx, centery);
                }
                else
                {
                    //b)
                    var state = OpenTK.Input.Mouse.GetState();
                    float dx = state.X - mouse_previous_state.X;
                    float dy = state.Y - mouse_previous_state.Y;
                    mouse_previous_state = state;
                    //These are raw coordinates, so need to apply acceleration manually.
                    float dx2 = (dx * Math.Abs(dx) * MouseAcceleration1);
                    float dy2 = (dy * Math.Abs(dy) * MouseAcceleration1);
                    dx2 += dx * MouseAcceleration2;
                    dy2 += dy * MouseAcceleration2;
                    mouse_delta = new PointF(dx2, dy2);
                }
            }
        }
        public float MouseAcceleration1 = 0.12f;
        public float MouseAcceleration2 = 0.7f;
        OpenTK.Input.MouseState mouse_previous_state;
        float rotationspeed = 0.15f;
        float movespeed = basemovespeed;
        float fallspeed { get { return movespeed / 10; } }
        public const float basemovespeed = 5f;
        DateTime lastbuild = new DateTime();
        float walksoundtimer = 0;
        int lastwalksound = 0;
        float stepsoundduration = 0.4f;
        void UpdateWalkSound(double dt)
        {
            if (dt == -1)
            {
                dt = stepsoundduration / 2;
            }
            walksoundtimer += (float)dt;
            string[] soundwalk = soundwalkcurrent();
            if (soundwalk.Length == 0)
            {
                return;
            }
            if (walksoundtimer >= stepsoundduration)
            {
                walksoundtimer = 0;
                lastwalksound++;
                if (lastwalksound >= soundwalk.Length)
                {
                    lastwalksound = 0;
                }
                if (rnd.Next(100) < 40)
                {
                    lastwalksound = rnd.Next(soundwalk.Length);
                }
                d_Audio.Play(soundwalk[lastwalksound]);
            }
        }
        string[] soundwalkcurrent()
        {
            int? b = BlockUnderPlayer();
            if (b != null)
            {
                return d_Data.WalkSound[b.Value];
            }
            return d_Data.WalkSound[0];
        }
        bool IsInLeft(Vector3 player_yy, Vector3 tile_yy)
        {
            return (int)player_yy.X == (int)tile_yy.X && (int)player_yy.Z == (int)tile_yy.Z;
        }
        bool enable_freemove = false;
        public bool ENABLE_FREEMOVE
        {
            get { return enable_freemove; }
            set { enable_freemove = value; }
        }
        bool enable_move = true;
        public bool ENABLE_MOVE { get { return enable_move; } set { enable_move = value; } }
        bool ENABLE_NOCLIP = false;
        public void OnUpdateFrame(FrameEventArgs e)
        {
            //..base.OnUpdateFrame(e);
            //UpdateFrame(e);
        }
        public enum LockY
        {
            True,
            False,
        }
        CharacterPhysicsState player = new CharacterPhysicsState();
        //DateTime lasttodo;
        bool mouseleftclick = false;
        bool mouseleftdeclick = false;
        bool wasmouseleft = false;
        bool mouserightclick = false;
        bool mouserightdeclick = false;
        bool wasmouseright = false;
        public float PlayerPushDistance = 2f;
        void FrameTick(FrameEventArgs e)
        {
            //if ((DateTime.Now - lasttodo).TotalSeconds > BuildDelay && todo.Count > 0)
            //UpdateTerrain();
            OnNewFrame(e.Time);
            UpdateMousePosition();
            if (guistate == GuiState.Normal)
            {
                UpdateMouseViewportControl(e);
            }
            NetworkProcess();
            if (guistate == GuiState.MapLoading) { return; }

            bool angleup = false;
            bool angledown = false;
            float overheadcameraanglemovearea = 0.05f;
            float overheadcameraspeed = 3;
            if (guistate == GuiState.Normal && d_GlWindow.Focused && cameratype == CameraType.Overhead)
            {
                if (mouse_current.X > Width - Width * overheadcameraanglemovearea)
                {
                    overheadcameraK.TurnLeft((float)e.Time * overheadcameraspeed);
                }
                if (mouse_current.X < Width * overheadcameraanglemovearea)
                {
                    overheadcameraK.TurnRight((float)e.Time * overheadcameraspeed);
                }
                if (mouse_current.Y < Height * overheadcameraanglemovearea)
                {
                    angledown = true;
                }
                if (mouse_current.Y > Height - Height * overheadcameraanglemovearea)
                {
                    angleup = true;
                }
            }
            bool wantsjump = GuiTyping == TypingState.None && Keyboard[GetKey(OpenTK.Input.Key.Space)];
            int movedx = 0;
            int movedy = 0;
            bool moveup = false;
            bool movedown = false;
            if (guistate == GuiState.Normal)
            {
                if (GuiTyping == TypingState.None)
                {
                    if (overheadcamera)
                    {
                        CameraMove m = new CameraMove();
                        if (Keyboard[GetKey(OpenTK.Input.Key.A)]) { overheadcameraK.TurnRight((float)e.Time * overheadcameraspeed); }
                        if (Keyboard[GetKey(OpenTK.Input.Key.D)]) { overheadcameraK.TurnLeft((float)e.Time * overheadcameraspeed); }
                        if (Keyboard[GetKey(OpenTK.Input.Key.W)]) { angleup = true; }
                        if (Keyboard[GetKey(OpenTK.Input.Key.S)]) { angledown = true; }
                        overheadcameraK.Center = player.playerposition;
                        m.Distance = overheadcameradistance;
                        m.AngleUp = angleup;
                        m.AngleDown = angledown;
                        overheadcameraK.Move(m, (float)e.Time);
                        if ((player.playerposition - playerdestination).Length >= 1f)
                        {
                            movedy += 1;
                            if (d_Physics.reachedwall)
                            {
                                wantsjump = true;
                            }
                            //player orientation
                            Vector3 q = playerdestination - player.playerposition;
                            float angle = VectorAngleGet(q);
                            player.playerorientation.Y = (float)Math.PI / 2 + angle;
                            player.playerorientation.X = (float)Math.PI;
                        }
                    }
                    else if (ENABLE_MOVE)
                    {
                        if (Keyboard[GetKey(OpenTK.Input.Key.W)]) { movedy += 1; }
                        if (Keyboard[GetKey(OpenTK.Input.Key.S)]) { movedy += -1; }
                        if (Keyboard[GetKey(OpenTK.Input.Key.A)]) { movedx += -1; }
                        if (Keyboard[GetKey(OpenTK.Input.Key.D)]) { movedx += 1; }
                    }
                }
                if (ENABLE_FREEMOVE || Swimming)
                {
                    if (GuiTyping == TypingState.None && Keyboard[GetKey(OpenTK.Input.Key.Space)])
                    {
                        moveup = true;
                    }
                    if (GuiTyping == TypingState.None && Keyboard[GetKey(OpenTK.Input.Key.ControlLeft)])
                    {
                        movedown = true;
                    }
                }
            }
            else if (guistate == GuiState.EscapeMenu)
            {
            }
            else if (guistate == GuiState.Inventory)
            {
            }
            else if (guistate == GuiState.MapLoading)
            {
                //todo back to game when escape key pressed.
            }
            else if (guistate == GuiState.CraftingRecipes)
            {
            }
            else if (guistate == GuiState.EditText) { }
            else if (guistate == GuiState.ModalDialog)
            {
            }
            else throw new Exception();
            float movespeednow = MoveSpeedNow();
            Acceleration acceleration = new Acceleration();
            int? blockunderplayer = BlockUnderPlayer();
            {
                //slippery walk on ice and when swimming
                if ((blockunderplayer != null && d_Data.IsSlipperyWalk[blockunderplayer.Value]) || Swimming)
                {
                    acceleration = new Acceleration()
                    {
                        acceleration1 = 0.99f,
                        acceleration2 = 0.2f,
                        acceleration3 = 70,
                    };
                }
            }
            float jumpstartacceleration = 13.333f * d_Physics.gravity;
            if (blockunderplayer != null && blockunderplayer == d_Data.BlockIdTrampoline
                && (!player.isplayeronground))
            {
                wantsjump = true;
                jumpstartacceleration = 20.666f * d_Physics.gravity;
            }
            //no aircontrol
            if (!player.isplayeronground)
            {
                acceleration = new Acceleration()
                {
                    acceleration1 = 0.99f,
                    acceleration2 = 0.2f,
                    acceleration3 = 70f,
                };
            }
            Vector3 push = new Vector3();
            foreach (var k in d_Clients.Players)
            {
                if ((k.Value.Position == null) ||
                    (k.Key == this.LocalPlayerId) ||
                    (k.Value.Position == LocalPlayerPosition)
                     || (float.IsNaN(LocalPlayerPosition.X)))
                {
                    continue;
                }
                if ((k.Value.Position.Value - LocalPlayerPosition).Length < PlayerPushDistance)
                {
                    Vector3 diff = LocalPlayerPosition - k.Value.Position.Value;
                    push += diff;
                }
            }
            foreach (var k in new List<Explosion>(explosions))
            {
                Vector3 kpos = new Vector3(k.explosion.X, k.explosion.Z, k.explosion.Y);
                if (k.explosion.IsRelativeToPlayerPosition)
                {
                    kpos += LocalPlayerPosition;
                }
                if ((kpos - LocalPlayerPosition).Length < k.explosion.Range)
                {
                    Vector3 diff = LocalPlayerPosition - kpos;
                    push += diff;
                }
                if ((DateTime.UtcNow - k.date) > TimeSpan.FromSeconds(k.explosion.Time))
                {
                    explosions.Remove(k);
                }
            }
            var move = new CharacterPhysics.MoveInfo()
            {
                movedx = movedx,
                movedy = movedy,
                acceleration = acceleration,
                ENABLE_FREEMOVE = ENABLE_FREEMOVE,
                ENABLE_NOCLIP = ENABLE_NOCLIP,
                jumpstartacceleration = jumpstartacceleration,
                movespeednow = movespeednow,
                moveup = moveup,
                movedown = movedown,
                Swimming = Swimming,
                wantsjump = wantsjump,
            };
            bool soundnow;
            if (FollowId == null)
            {
                d_Physics.Move(player, move, e.Time, out soundnow, push, Players[LocalPlayerId].ModelHeight);
                if (soundnow)
                {
                    UpdateWalkSound(-1);
                }
                if (player.isplayeronground && movedx != 0 || movedy != 0)
                {
                    UpdateWalkSound(e.Time);
                }
                UpdateBlockDamageToPlayer();
                UpdateFallDamageToPlayer();
            }
            else
            {
                if (FollowId == LocalPlayerId)
                {
                    move.movedx = 0;
                    move.movedy = 0;
                    move.wantsjump = false;
                    d_Physics.Move(player, move, e.Time, out soundnow, push, players[LocalPlayerId].ModelHeight);
                }
            }
            if (guistate == GuiState.CraftingRecipes)
            {
                CraftingMouse();
            }
            if (guistate == GuiState.EscapeMenu)
            {
                //EscapeMenuMouse();
            }
            if (guistate == GuiState.Normal)
            {
                UpdatePicking();
            }
            //must be here because frametick can be called more than once per render frame.
            keyevent = null;
            keyeventup = null;

            Vector3 orientation = new Vector3((float)Math.Sin(LocalPlayerOrientation.Y), 0, -(float)Math.Cos(LocalPlayerOrientation.Y));
            d_Audio.UpdateListener(LocalPlayerPosition + new Vector3(0, CharacterEyesHeight, 0), orientation);
            Item activeitem = d_Inventory.RightHand[ActiveMaterial];
            int activeblock = 0;
            if (activeitem != null) { activeblock = activeitem.BlockId; }
            if (activeblock != PreviousActiveMaterialBlock)
            {
                SendPacketClient(new PacketClient()
                {
                    PacketId = ClientPacketId.ActiveMaterialSlot,
                    ActiveMaterialSlot = new PacketClientActiveMaterialSlot() { ActiveMaterialSlot = ActiveMaterial }
                });
            }
            PreviousActiveMaterialBlock = activeblock;
            playervelocity = LocalPlayerPosition - lastplayerposition;
            playervelocity *= 75;
            lastplayerposition = LocalPlayerPosition;
            if (reloadstart.Ticks != 0
                && (DateTime.UtcNow - reloadstart).TotalSeconds > blocktypes[reloadblock].ReloadDelay)
            {
                if (TotalAmmo.ContainsKey(reloadblock))
                {
                    int loaded = TotalAmmo[reloadblock];
                    loaded = Math.Min(blocktypes[reloadblock].AmmoMagazine, loaded);
                    LoadedAmmo[reloadblock] = loaded;
                    reloadstart = new DateTime(0);
                    reloadblock = -1;
                }
            }
            foreach (Projectile p in new List<Projectile>(projectiles))
            {
                UpdateGrenade(p, (float)e.Time);
            }
        }
        public class Projectile
        {
            public Vector3 position;
            public Vector3 velocity;
            public DateTime start;
            public int block;
            public float explodesafter;
        }
        int reloadblock;
        DateTime reloadstart;
        Vector3 lastplayerposition;
        Vector3 playervelocity;
        int PreviousActiveMaterialBlock;
        //bool test;
        private void UpdateFallDamageToPlayer()
        {
            //fallspeed 4 is 10 blocks high
            //fallspeed 5.5 is 20 blocks high
            float fallspeed = player.movedz / (-basemovespeed);
            /*
            test = false;
            if (fallspeed > 5.5f)
            {
                test = true;
            }
            */
            Vector3i pos = GetPlayerEyesBlock();
            if ((MapUtil.blockheight(d_Map, 0, pos.x, pos.y) < pos.z - 8)
                || fallspeed > 3)
            {
                d_Audio.PlayAudioLoop("fallloop.wav", fallspeed > 2, true);
            }
            else
            {
                d_Audio.PlayAudioLoop("fallloop.wav", false, true);
            }

            //fall damage

            if (MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z - 3))
            {
                int blockBelow = d_Map.GetBlock(pos.x, pos.y, pos.z - 3);
                if ((blockBelow != 0) && (!d_Data.IsWater[blockBelow]))
                {
                    float severity = 0;
                    if (fallspeed < 4) { return; }
                    else if (fallspeed < 4.5) { severity = 0.3f; }
                    else if (fallspeed < 5.0) { severity = 0.5f; }
                    else if (fallspeed < 5.5) { severity = 0.6f; }
                    else if (fallspeed < 6.0) { severity = 0.8f; }
                    else { severity = 1f; }
                    if ((DateTime.UtcNow - lastfalldamagetime).TotalSeconds < 1)
                    {
                        return;
                    }
                    lastfalldamagetime = DateTime.UtcNow;
                    ApplyDamageToPlayer((int)(severity * PlayerStats.MaxHealth));
                }
            }
        }
        DateTime lastfalldamagetime;

        Vector3i GetPlayerEyesBlock()
        {
            var p = LocalPlayerPosition;
            p += new Vector3(0, Players[LocalPlayerId].EyeHeight, 0);
            return new Vector3i((int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y));
        }

        //TODO server side, damage parameter in Blocks.csv
        private void UpdateBlockDamageToPlayer()
        {
            var p = LocalPlayerPosition;
            p += new Vector3(0, Players[LocalPlayerId].EyeHeight, 0);
            int block1 = 0;
            int block2 = 0;
            if (MapUtil.IsValidPos(d_Map, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y)))
            {
                block1 = d_Map.GetBlock((int)p.X, (int)p.Z, (int)p.Y);
            }
            if (MapUtil.IsValidPos(d_Map, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y) - 1))
            {
                block2 = d_Map.GetBlock((int)p.X, (int)p.Z, (int)p.Y - 1);
            }

            //TODO d_Data.DamageToPlayer.
            //TODO swimming in water too long.
            if (block1 == d_Data.BlockIdLava
                || block1 == d_Data.BlockIdStationaryLava
                || block2 == d_Data.BlockIdLava
                || block2 == d_Data.BlockIdStationaryLava)
            {
                BlockDamageToPlayerTimer.Update(ApplyBlockDamageToPlayer);
            }
        }

        public int BlockDamageToPlayer = 2;
        public const int BlockDamageToPlayerEvery = 1;

        void ApplyBlockDamageToPlayer()
        {
            ApplyDamageToPlayer(BlockDamageToPlayer);
        }

        void ApplyDamageToPlayer(int damage)
        {
            PlayerStats.CurrentHealth -= damage;
            if (PlayerStats.CurrentHealth <= 0)
            {
                d_Audio.Play("death.wav");
                Respawn();
            }
            else
            {
                d_Audio.Play(rnd.Next() % 2 == 0 ? "grunt1.wav" : "grunt2.wav");
            }
            SendPacketClient(new PacketClient()
            {
                PacketId = ClientPacketId.Health,
                Health = new PacketClientHealth() { CurrentHealth = PlayerStats.CurrentHealth },
            });
        }

        private void Respawn()
        {
            SendPacketClient(new PacketClient()
            {
                PacketId = ClientPacketId.SpecialKey,
                SpecialKey = new PacketClientSpecialKey() { key = ManicDigger.SpecialKey.Respawn },
            });
            player.movedz = 0;
        }

        Timer BlockDamageToPlayerTimer = new Timer() { INTERVAL = BlockDamageToPlayerEvery, MaxDeltaTime = BlockDamageToPlayerEvery * 2 };

        public OpenTK.Input.Key GetKey(OpenTK.Input.Key key)
        {
            if (options.Keys.ContainsKey((int)key))
            {
                return (OpenTK.Input.Key)options.Keys[(int)key];
            }
            return key;
        }
        public bool KeyIsEqualChar(OpenTK.Input.Key key1, char key2)
        {
            // TODO: Any better solution? http://www.opentk.com/node/1202
            if (options.Keys.ContainsKey((int)key1))
            {
                return ((OpenTK.Input.Key)options.Keys[(int)key1]).ToString().Equals(key2.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }
            return  key1.ToString().Equals(key2.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        private float VectorAngleGet(Vector3 q)
        {
            return (float)(Math.Acos(q.X / q.Length) * Math.Sign(q.Z));
        }
        private float MoveSpeedNow()
        {
            float movespeednow = movespeed;
            {
                //walk faster on cobblestone
                int? blockunderplayer = BlockUnderPlayer();
                if (blockunderplayer != null)
                {
                    movespeednow *= d_Data.WalkSpeed[blockunderplayer.Value];
                }
            }
            if (Keyboard[GetKey(OpenTK.Input.Key.ShiftLeft)])
            {
                //enable_acceleration = false;
                movespeednow *= 0.2f;
            }
            Item item = d_Inventory.RightHand[ActiveMaterial];
            if (item != null && item.ItemClass == ItemClass.Block)
            {
                movespeednow *= blocktypes[item.BlockId].WalkSpeedWhenUsed;
                if (IronSights)
                {
                    movespeednow *= blocktypes[item.BlockId].IronSightsMoveSpeed;
                }
            }
            return movespeednow;
        }
        bool IronSights;
        int? BlockUnderPlayer()
        {
            if (!MapUtil.IsValidPos(d_Map, (int)player.playerposition.X,
                 (int)player.playerposition.Z, (int)player.playerposition.Y - 1))
            {
                return null;
            }
            int blockunderplayer = d_Map.GetBlock((int)player.playerposition.X,
                (int)player.playerposition.Z, (int)player.playerposition.Y - 1);
            return blockunderplayer;
        }
        Vector3 playerdestination;
        class MenuState
        {
            public int selected = 0;
        }
        MenuState menustate = new MenuState();
        private void UpdateMouseViewportControl(FrameEventArgs e)
        {
            if (!overheadcamera)
            {
                player.playerorientation.Y += (float)mouse_delta.X * rotationspeed * (float)e.Time;
                player.playerorientation.X += (float)mouse_delta.Y * rotationspeed * (float)e.Time;
                player.playerorientation.X = MyMath.Clamp(player.playerorientation.X, (float)Math.PI / 2 + 0.015f, (float)(Math.PI / 2 + Math.PI - 0.015f));
            }
        }
        bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            if (z >= d_Map.MapSizeZ)
            {
                return true;
            }
            if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            if (x >= d_Map.MapSizeX || y >= d_Map.MapSizeY)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            return d_Map.GetBlock(x, y, z) == SpecialBlockId.Empty
                || d_Map.GetBlock(x, y, z) == 117
                || d_Data.IsWater[d_Map.GetBlock(x, y, z)];
        }
        bool IsTileEmptyForPhysicsClose(int x, int y, int z)
        {
            return IsTileEmptyForPhysics(x, y, z)
                || (MapUtil.IsValidPos(d_Map, x, y, z) && d_Data.DrawType1[d_Map.GetBlock(x, y, z)] == DrawType.HalfHeight)
                || (MapUtil.IsValidPos(d_Map, x, y, z) && d_Data.IsEmptyForPhysics[d_Map.GetBlock(x, y, z)]);
        }
        public float PICK_DISTANCE = 3.5f;
        public float PickDistance { get { return PICK_DISTANCE; } set { PICK_DISTANCE = value; } }
        bool leftpressedpicking = false;
        public int SelectedModelId { get { return selectedmodelid; } set { selectedmodelid = value; } }
        int selectedmodelid = -1;
        bool IsUsableBlock(int blocktype)
        {
            return d_Data.IsRailTile(blocktype) || blocktypes[blocktype].IsUsable;
        }
        bool IsWearingWeapon()
        {
            return d_Inventory.RightHand[ActiveMaterial] != null;
        }
        int pistolcycle;
        class Sprite
        {
            public Vector3 position;
            public DateTime time;
            public TimeSpan timespan;
            public string image;
            public int size = 40;
            public int animationcount;
        }
        DateTime lastironsightschange;
        List<Sprite> sprites = new List<Sprite>();
        DateTime grenadecookingstart;
        float grenadetime = 3;
        private void UpdatePicking()
        {
            if (FollowId != null)
            {
                SelectedBlockPosition = new Vector3(-1, -1, -1);
                return;
            }
            int bulletsshot = 0;
            bool IsNextShot = false;
        NextBullet:
            bool left = Mouse[OpenTK.Input.MouseButton.Left];//destruct
            bool middle = Mouse[OpenTK.Input.MouseButton.Middle];//clone material as active
            bool right = Mouse[OpenTK.Input.MouseButton.Right];//build

            if (!leftpressedpicking)
            {
                if (mouseleftclick)
                {
                    leftpressedpicking = true;
                }
                else
                {
                    left = false;
                }
            }
            else
            {
                if (mouseleftdeclick)
                {
                    leftpressedpicking = false;
                    left = false;
                }
            }
            if (!left)
            {
                currentAttackedBlock = null;
            }

            float pick_distance = PICK_DISTANCE;
            if (cameratype == CameraType.Tpp) { pick_distance = tppcameradistance * 2; }
            if (cameratype == CameraType.Overhead) { pick_distance = overheadcameradistance; }

            Item item = d_Inventory.RightHand[ActiveMaterial];
            bool ispistol = (item != null && blocktypes[item.BlockId].IsPistol);
            bool ispistolshoot = ispistol && left;
            bool isgrenade = ispistol && blocktypes[item.BlockId].PistolType == PistolType.Grenade;
            if (ispistol && isgrenade)
            {
                ispistolshoot = mouseleftdeclick;
            }
            //grenade cooking
            if (mouseleftclick)
            {
                grenadecookingstart = DateTime.UtcNow;
                if (ispistol && isgrenade)
                {
                    if (blocktypes[item.BlockId].Sounds.Shoot.Length > 0)
                    {
                        d_Audio.Play(blocktypes[item.BlockId].Sounds.Shoot[0] + ".ogg");
                    }
                }
            }
            float wait = (float)(DateTime.UtcNow - grenadecookingstart).TotalSeconds;
            if (isgrenade && left)
            {
                if (wait >= grenadetime && isgrenade && grenadecookingstart != new DateTime())
                {
                    ispistolshoot = true;
                    mouseleftdeclick = true;
                }
                else
                {
                    return;
                }
            }
            else
            {
                grenadecookingstart = new DateTime();
            }

            if (ispistol && mouserightclick && (DateTime.UtcNow - lastironsightschange).TotalSeconds >= 0.5)
            {
                IronSights = !IronSights;
                lastironsightschange = DateTime.UtcNow;
            }

            float unit_x = 0;
            float unit_y = 0;
            int NEAR = 1;
            int FOV = (int)GetCurrentFov() * 10; // 600
            float ASPECT = 640f / 480;
            float near_height = NEAR * (float)(Math.Tan(FOV * Math.PI / 360.0));
            Vector3 ray = new Vector3(unit_x * near_height * ASPECT, unit_y * near_height, 1);//, 0);

            Vector3 ray_start_point = new Vector3(0, 0, 0);
            PointF aim = GetAim();
            if (overheadcamera || aim.X != 0 || aim.Y != 0)
            {
                float mx = 0;
                float my = 0;
                if (overheadcamera)
                {
                    mx = (float)mouse_current.X / Width - 0.5f;
                    my = (float)mouse_current.Y / Height - 0.5f;
                }
                else if (ispistolshoot && (aim.X != 0 || aim.Y != 0))
                {
                    mx += aim.X / Width;
                    my += aim.Y / Height;
                }
                //ray_start_point = new Vector3(mx * 1.4f, -my * 1.1f, 0.0f);
                ray_start_point = new Vector3(mx * 3f, -my * 2.2f, -1.0f);
            }

            //Matrix4 the_modelview;
            //Read the current modelview matrix into the array the_modelview
            //GL.GetFloat(GetPName.ModelviewMatrix, out the_modelview);
            if (d_The3d.ModelViewMatrix.Equals(new Matrix4())) { return; }
            Matrix4 theModelView = d_The3d.ModelViewMatrix;
            theModelView.Invert();
            //the_modelview = new Matrix4();
            ray = Vector3.Transform(ray, theModelView);
            ray_start_point = Vector3.Transform(ray_start_point, theModelView);
            Line3D pick = new Line3D();
            Vector3 raydir = -(ray - ray_start_point);
            raydir.Normalize();
            pick.Start = ray + Vector3.Multiply(raydir, 1f); //do not pick behind
            pick.End = ray + Vector3.Multiply(raydir, pick_distance * ((ispistolshoot) ? 100 : 2));

            //pick models
            selectedmodelid = -1;
            foreach (var m in Models)
            {
                Vector3 closestmodelpos = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
                foreach (var t in m.TrianglesForPicking)
                {
                    Vector3 intersection;
                    if (Collisions.Intersection.RayTriangle(pick, t, out intersection) == 1)
                    {
                        if ((pick.Start - intersection).Length > pick_distance)
                        {
                            continue;
                        }
                        if ((pick.Start - intersection).Length < (pick.Start - closestmodelpos).Length)
                        {
                            closestmodelpos = intersection;
                            selectedmodelid = m.Id;
                        }
                    }
                }
            }
            if (selectedmodelid != -1)
            {
                SelectedBlockPosition = new Vector3(-1, -1, -1);
                if (mouseleftclick)
                {
                    ModelClick(selectedmodelid);
                }
                mouseleftclick = false;
                leftpressedpicking = false;
                return;
            }

            if (left)
            {
                d_Weapon.SetAttack(true, false);
            }
            else if (right)
            {
                d_Weapon.SetAttack(true, true);
            }

            //if (iii++ % 2 == 0)
            {
                //To improve speed, update picking only every second frame.
                //return;
            }

            //pick terrain
            var s = new BlockOctreeSearcher();
            s.StartBox = new Box3D(0, 0, 0, BitTools.NextPowerOfTwo((uint)Math.Max(d_Map.MapSizeX, Math.Max(d_Map.MapSizeY, d_Map.MapSizeZ))));
            List<BlockPosSide> pick2 = new List<BlockPosSide>(s.LineIntersection(IsTileEmptyForPhysics, getblockheight, pick));
            pick2.Sort((a, b) => { return (a.pos - ray_start_point).Length.CompareTo((b.pos - ray_start_point).Length); });

            if (overheadcamera && pick2.Count > 0 && left)
            {
                //if not picked any object, and mouse button is pressed, then walk to destination.
                playerdestination = pick2[0].pos;
            }
            bool pickdistanceok = pick2.Count > 0 && (pick2[0].pos - (player.playerposition)).Length <= pick_distance;
            bool playertileempty = IsTileEmptyForPhysics(
                        (int)ToMapPos(player.playerposition).X,
                        (int)ToMapPos(player.playerposition).Y,
                        (int)ToMapPos(player.playerposition).Z);
            bool playertileemptyclose = IsTileEmptyForPhysicsClose(
                        (int)ToMapPos(player.playerposition).X,
                        (int)ToMapPos(player.playerposition).Y,
                        (int)ToMapPos(player.playerposition).Z);
            BlockPosSide pick0;
            if (pick2.Count > 0 &&
                ((pickdistanceok && (playertileempty || (playertileemptyclose)))
                || overheadcamera)
                )
            {
                SelectedBlockPosition = pick2[0].Current();
                SelectedBlockPosition = new Vector3((int)SelectedBlockPosition.X, (int)SelectedBlockPosition.Y, (int)SelectedBlockPosition.Z);
                pick0 = pick2[0];
            }
            else
            {
                SelectedBlockPosition = new Vector3(-1, -1, -1);
                pick0.pos = new Vector3(-1, -1, -1);
                pick0.side = TileSide.Front;
            }
            if (FreeMouse)
            {
                if (pick2.Count > 0)
                {
                    OnPick(pick0);
                }
                return;
            }
            var ntile = pick0.Current();
            if (IsUsableBlock(d_Map.GetBlock((int)ntile.X, (int)ntile.Z, (int)ntile.Y)))
            {
                currentAttackedBlock = new Vector3i((int)ntile.X, (int)ntile.Z, (int)ntile.Y);
            }
            if ((DateTime.Now - lastbuild).TotalSeconds >= BuildDelay || IsNextShot)
            {
                if (left && d_Inventory.RightHand[ActiveMaterial] == null)
                {
                    PacketClientHealth p = new PacketClientHealth { CurrentHealth = (int)(2 + rnd.NextDouble() * 4) };
                    SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.MonsterHit, Health = p }));
                }
                if (left && !fastclicking)
                {
                    //todo animation
                    fastclicking = false;
                }
                if ((left || right || middle) && (!isgrenade))
                {
                    lastbuild = DateTime.Now;
                }
                if (isgrenade && mouseleftdeclick)
                {
                    lastbuild = DateTime.Now;
                }
                if (reloadstart.Ticks != 0)
                {
                    goto end;
                }
                if (ispistolshoot)
                {
                    if ((!(LoadedAmmo.ContainsKey(item.BlockId) && LoadedAmmo[item.BlockId] > 0))
                        || (!(TotalAmmo.ContainsKey(item.BlockId) && TotalAmmo[item.BlockId] > 0)))
                    {
                        d_Audio.Play("Dry Fire Gun-SoundBible.com-2053652037.ogg");
                        goto end;
                    }
                }
                if(ispistolshoot)
                {
                    Vector3 to = pick.End;
                    if (pick2.Count > 0)
                    {
                        to = pick2[0].pos;
                    }

                    PacketClientShot shot = new PacketClientShot();
                    shot.FromX = pick.Start.X;
                    shot.FromY = pick.Start.Y;
                    shot.FromZ = pick.Start.Z;
                    shot.ToX = to.X;
                    shot.ToY = to.Y;
                    shot.ToZ = to.Z;
                    shot.HitPlayer = -1;

                    foreach (var k in d_Clients.Players)
                    {
                        if (k.Value.Position == null)
                        {
                            continue;
                        }
                        Vector3 feetpos = new Vector3((float)k.Value.Position.Value.X, (float)k.Value.Position.Value.Y, (float)k.Value.Position.Value.Z);
                        //var p = PlayerPositionSpawn;
                        ManicDigger.Collisions.Box3D bodybox = new ManicDigger.Collisions.Box3D();
                        float headsize = (k.Value.ModelHeight - k.Value.EyeHeight) * 2; //0.4f;
                        float h = k.Value.ModelHeight - headsize;
                        float r = 0.35f;

                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + 0, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + 0, feetpos.Z + r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + 0, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + 0, feetpos.Z + r);

                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z + r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z + r);

                        ManicDigger.Collisions.Box3D headbox = new ManicDigger.Collisions.Box3D();

                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z + r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z + r);

                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h + headsize, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h + headsize, feetpos.Z + r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h + headsize, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h + headsize, feetpos.Z + r);

                        BlockPosSide? p;
                        Vector3 localeyepos = LocalPlayerPosition + new Vector3(0, players[LocalPlayerId].ModelHeight, 0);
                        if ((p = ManicDigger.Collisions.Intersection.CheckLineBoxExact(pick, headbox)) != null)
                        {
                            //do not allow to shoot through terrain
                            if (pick2.Count == 0 || ((pick2[0].pos - localeyepos).Length > (p.Value.pos - localeyepos).Length))
                            {
                                if (!isgrenade)
                                {
                                    sprites.Add(new Sprite() { position = p.Value.pos, time = DateTime.UtcNow, timespan = TimeSpan.FromSeconds(0.2), image = "blood.png" });
                                }
                                shot.HitPlayer = k.Key;
                                shot.HitHead = true;
                            }
                        }
                        else if ((p = ManicDigger.Collisions.Intersection.CheckLineBoxExact(pick, bodybox)) != null)
                        {
                            //do not allow to shoot through terrain
                            if (pick2.Count == 0 || ((pick2[0].pos - localeyepos).Length > (p.Value.pos - localeyepos).Length))
                            {
                                if (!isgrenade)
                                {
                                    sprites.Add(new Sprite() { position = p.Value.pos, time = DateTime.UtcNow, timespan = TimeSpan.FromSeconds(0.2), image = "blood.png" });
                                }
                                shot.HitPlayer = k.Key;
                                shot.HitHead = false;
                            }
                        }
                    }
                    shot.WeaponBlock = item.BlockId;
                    LoadedAmmo[item.BlockId] = LoadedAmmo[item.BlockId] - 1;
                    TotalAmmo[item.BlockId] = TotalAmmo[item.BlockId] - 1;
                    float projectilespeed = blocktypes[item.BlockId].ProjectileSpeed;
                    if (projectilespeed == 0)
                    {
                        bullets.Add(new Bullet() { from = pick.Start, to = to, speed = 150 });
                    }
                    else
                    {
                        Vector3 v = to - pick.Start;
                        v.Normalize();
                        v *= projectilespeed;
                        shot.ExplodesAfter = grenadetime - wait;
                        projectiles.Add(new Projectile() { position = pick.Start, velocity = v, start = DateTime.UtcNow, block = item.BlockId, explodesafter = grenadetime - wait });
                    }
                    SendPacketClient(new PacketClient() { PacketId = ClientPacketId.Shot, Shot = shot });

                    if (blocktypes[item.BlockId].Sounds.ShootEnd.Length > 0)
                    {
                        pistolcycle = rnd.Next(blocktypes[item.BlockId].Sounds.ShootEnd.Length);
                        d_Audio.Play(blocktypes[item.BlockId].Sounds.ShootEnd[pistolcycle] + ".ogg");
                    }

                    bulletsshot++;
                    if (bulletsshot < blocktypes[item.BlockId].BulletsPerShot)
                    {
                        IsNextShot = true;
                        goto NextBullet;
                    }
                    
                    //recoil
                    player.playerorientation.X -= (float)rnd.NextDouble() * CurrentRecoil;
                    player.playerorientation.Y += (float)rnd.NextDouble() * CurrentRecoil * 2 - CurrentRecoil;

                    goto end;
                }
                if (ispistol && right)
                {
                    goto end;
                }
                if (pick2.Count > 0)
                {
                    if (middle)
                    {
                        var newtile = pick0.Current();
                        if (MapUtil.IsValidPos(d_Map, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            int clonesource = d_Map.GetBlock((int)newtile.X, (int)newtile.Z, (int)newtile.Y);
                            int clonesource2 = (int)d_Data.WhenPlayerPlacesGetsConvertedTo[(int)clonesource];
                            //find this block in another right hand.
                            for (int i = 0; i < 10; i++)
                            {
                                if (d_Inventory.RightHand[i] != null
                                    && d_Inventory.RightHand[i].ItemClass == ItemClass.Block
                                    && (int)d_Inventory.RightHand[i].BlockId == clonesource2)
                                {
                                    ActiveMaterial = i;
                                    goto done;
                                }
                            }
                            int? freehand = d_InventoryUtil.FreeHand(ActiveMaterial);
                            //find this block in inventory.
                            foreach (var k in d_Inventory.Items)
                            {
                                if (k.Value.ItemClass == ItemClass.Block
                                    && k.Value.BlockId == clonesource2)
                                {
                                    //free hand
                                    if (freehand != null)
                                    {
                                        d_InventoryController.WearItem(
                                            InventoryPosition.MainArea(new Point(k.Key.X, k.Key.Y)),
                                            InventoryPosition.MaterialSelector(freehand.Value));
                                        goto done;
                                    }
                                    //try to replace current slot
                                    if (d_Inventory.RightHand[ActiveMaterial] != null
                                        && d_Inventory.RightHand[ActiveMaterial].ItemClass == ItemClass.Block)
                                    {
                                        d_InventoryController.MoveToInventory(
                                            InventoryPosition.MaterialSelector(ActiveMaterial));
                                        d_InventoryController.WearItem(
                                            InventoryPosition.MainArea(new Point(k.Key.X, k.Key.Y)),
                                            InventoryPosition.MaterialSelector(ActiveMaterial));
                                    }
                                }
                            }
                        done:
                            string[] sound = d_Data.CloneSound[clonesource];
                            if (sound != null && sound.Length > 0)
                            {
                                d_Audio.Play(sound[0]); //todo sound cycle
                            }
                        }
                    }
                    if (left || right)
                    {
                        BlockPosSide tile = pick0;
                        Console.Write(tile.pos + ":" + Enum.GetName(typeof(TileSide), tile.side));
                        Vector3 newtile = right ? tile.Translated() : tile.Current();
                        if (MapUtil.IsValidPos(d_Map, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            Console.WriteLine(". newtile:" + newtile + " type: " + d_Map.GetBlock((int)newtile.X, (int)newtile.Z, (int)newtile.Y));
                            if (pick0.pos != new Vector3(-1, -1, -1))
                            {
                                int blocktype;
                                if (left) { blocktype = d_Map.GetBlock((int)newtile.X, (int)newtile.Z, (int)newtile.Y); }
                                else { blocktype = BlockInHand() ?? 1; }
                                if (left && blocktype == d_Data.BlockIdAdminium) { goto end; }
                                string[] sound = left ? d_Data.BreakSound[blocktype] : d_Data.BuildSound[blocktype];
                                if (sound != null && sound.Length > 0)
                                {
                                    d_Audio.Play(sound[0]); //todo sound cycle
                                }
                            }
                            //normal attack
                            if (!right)
                            {
                                //attack
                                var pos = new Vector3i((int)newtile.X, (int)newtile.Z, (int)newtile.Y);
                                currentAttackedBlock = new Vector3i(pos.x, pos.y, pos.z);
                                if (!blockhealth.ContainsKey(pos))
                                {
                                    blockhealth[pos] = GetCurrentBlockHealth(pos.x, pos.y, pos.z);
                                }
                                blockhealth[pos] -= WeaponAttackStrength();
                                float health = GetCurrentBlockHealth(pos.x, pos.y, pos.z);
                                if (health <= 0)
                                {
                                    if (currentAttackedBlock != null)
                                    {
                                        blockhealth.Remove(currentAttackedBlock.Value);
                                    }
                                    currentAttackedBlock = null;
                                    goto broken;
                                }
                                goto end;
                            }
                            if (!right)
                            {
                                particleEffectBlockBreak.StartParticleEffect(newtile);//must be before deletion - gets ground type.
                            }
                            if (!MapUtil.IsValidPos(d_Map, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                            {
                                throw new Exception();
                            }
                        broken:
                            OnPick(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
                                new Vector3((int)tile.Current().X, (int)tile.Current().Z, (int)tile.Current().Y), tile.pos,
                                right);
                            //network.SendSetBlock(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
                            //    right ? BlockSetMode.Create : BlockSetMode.Destroy, (byte)MaterialSlots[activematerial]);
                        }
                    }
                }
            }
        end:
            fastclicking = false;
            if ((!(left || right || middle)) && (!ispistol))
            {
                lastbuild = new DateTime();
                fastclicking = true;
            }
        }
        float WeaponAttackStrength()
        {
            return (float)NextDouble(2, 4);
        }
        double NextDouble(double min, double max)
        {
            return rnd.NextDouble() * (max - min) + min;
        }
        float GetCurrentBlockHealth(int x, int y, int z)
        {
            if (blockhealth.ContainsKey(new Vector3i(x, y, z)))
            {
                return blockhealth[new Vector3i(x, y, z)];
            }
            int blocktype = d_Map.GetBlock(x, y, z);
            return d_Data.Strength[blocktype];
        }
        Dictionary<Vector3i, float> blockhealth = new Dictionary<Vector3i, float>();
        Vector3i? currentAttackedBlock;

        public PacketServerPlayerStats PlayerStats;

        public void DrawPlayerHealth()
        {
            if (PlayerStats != null)
            {
                float progress = (float)PlayerStats.CurrentHealth / PlayerStats.MaxHealth;
                Point size = new Point(30, 140);
                Point pos = new Point((int)(0.06f * Width), Height - 50);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), pos.X, pos.Y - size.Y, size.X, size.Y, null, Color.Black);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), pos.X, pos.Y - (progress * size.Y), size.X, (progress) * size.Y, null, Color.Red);
            }
            //if (test) { d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), 50, 50, 200, 200, null, Color.Red); }
        }

        void DrawEnemyHealthBlock()
        {
            if (currentAttackedBlock != null)
            {
                int x = currentAttackedBlock.Value.x;
                int y = currentAttackedBlock.Value.y;
                int z = currentAttackedBlock.Value.z;
                int blocktype = d_Map.GetBlock(x, y, z);
                float health = GetCurrentBlockHealth(x, y, z);
                float progress = health / d_Data.Strength[blocktype];
                if (IsUsableBlock(blocktype))
                {
                    DrawEnemyHealthUseInfo(d_Data.Name[blocktype], progress, true);
                }
                DrawEnemyHealthCommon(d_Data.Name[blocktype], progress);
            }
        }

        private int compassid = -1;
        private int needleid = -1;
        private float compassangle = 0;
        private float compassvertex = 1;

        bool CompassInActiveMaterials()
        {
            for (int i = 0; i < 10; i++)
            {
                if (MaterialSlots[i] == d_Data.BlockIdCompass)
                {
                    return true;
                }
            }
            return false;
        }

        void DrawCompass()
        {
            if (!CompassInActiveMaterials()) return;
            if (compassid == -1)
            {
                compassid = d_The3d.LoadTexture(d_GetFile.GetFile(Path.Combine("gui", "compass.png")));
                needleid = d_The3d.LoadTexture(d_GetFile.GetFile(Path.Combine("gui", "compassneedle.png")));
            }
            float size = 175;
            float posX = Width - 100;
            float posY = 100;
            float playerorientation = -(float)((player.playerorientation.Y / (2 * Math.PI)) * 360);

            compassvertex += (playerorientation - compassangle) / 50;
            compassvertex *= .90f;
            compassangle += compassvertex;

            Draw2dData[] todraw = new Draw2dData[1];
            todraw[0].x1 = posX - size / 2;
            todraw[0].y1 = posY - size / 2;
            todraw[0].width = size;
            todraw[0].height = size;
            todraw[0].inAtlasId = null;
            todraw[0].color = new FastColor(Color.White);

            d_The3d.Draw2dTexture(compassid, posX - size / 2, posY - size / 2, size, size, null);
            d_The3d.Draw2dTextures(todraw, needleid, compassangle);
        }

        void DrawEnemyHealthCommon(string name, float progress)
        {
            DrawEnemyHealthUseInfo(name, 1, false);
        }

        void DrawEnemyHealthUseInfo(string name, float progress, bool useInfo)
        {
            int y = useInfo ? 55 : 35;
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), xcenter(300), 40, 300, y, null, Color.Black);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), xcenter(300), 40, 300 * progress, y, null, Color.Red);
            d_The3d.Draw2dText(name, xcenter(d_The3d.TextSize(name, 14).Width), 40, 14, null);
            if (useInfo)
            {
                name = string.Format(Language.PressToUse, "E");
                d_The3d.Draw2dText(name, xcenter(d_The3d.TextSize(name, 10).Width), 70, 10, null);
            }
        }

        public const float RailHeight = 0.3f;

        float getblockheight(int x, int y, int z)
        {
            if (!MapUtil.IsValidPos(d_Map, x, y, z))
            {
                return 1;
            }
            if (d_Data.Rail[d_Map.GetBlock(x, y, z)] != RailDirectionFlags.None)
            {
                return RailHeight;
            }
            if (d_Data.DrawType1[d_Map.GetBlock(x, y, z)] == DrawType.HalfHeight)
            {
                return 0.5f;
            }
            return 1;
        }
        private void OnPick(BlockPosSide pick0)
        {
            //playerdestination = pick0.pos;
        }
        float BuildDelay
        {
            get
            {
                float default_ = 0.95f * (1 / basemovespeed);
                Item item = d_Inventory.RightHand[ActiveMaterial];
                if (item == null || item.ItemClass != ItemClass.Block)
                {
                    return default_;
                }
                float delay = blocktypes[item.BlockId].Delay;
                if (delay == 0)
                {
                    return default_;
                }
                return delay;
            }
        }
        Vector3 ToMapPos(Vector3 a)
        {
            return new Vector3((int)a.X, (int)a.Z, (int)a.Y);
        }
        bool fastclicking = false;
        public Vector3 SelectedBlockPosition;
        //double currentTime = 0;
        double accumulator = 0;
        double t = 0;
        //Vector3 oldplayerposition;
        public float CharacterEyesHeight { get { return Players[LocalPlayerId].EyeHeight; } set { Players[LocalPlayerId].EyeHeight = value; } }
        public float CharacterModelHeight { get { return Players[LocalPlayerId].ModelHeight; } }
        public Color clearcolor = Color.FromArgb(171, 202, 228);
        public Stopwatch framestopwatch;
        public void OnRenderFrame(FrameEventArgs e)
        {
            framestopwatch = new Stopwatch();
            framestopwatch.Start();
            UpdateTerrain();
            GL.ClearColor(guistate == GuiState.MapLoading ? Color.Black : clearcolor);

            //Sleep is required in Mono for running the terrain background thread.
            if (IsMono)
            {
                Application.DoEvents();
                Thread.Sleep(0);
            }
            var deltaTime = e.Time;

            accumulator += deltaTime;
            double dt = 1d / 75;

            while (accumulator >= dt)
            {
                FrameTick(new FrameEventArgs(dt));
                t += dt;
                accumulator -= dt;
            }
            
            if (guistate == GuiState.MapLoading) { goto draw2d; }

            if (ENABLE_LAG == 2) { Thread.SpinWait(20 * 1000 * 1000); }
            //..base.OnRenderFrame(e);


            if (d_HudInventory.IsMouseOverCells() && guistate == GuiState.Inventory)
            {
                int delta = Mouse.WheelDelta;
                if (delta > 0)
                {
                    d_HudInventory.ScrollUp();
                }
                if (delta < 0)
                {
                    d_HudInventory.ScrollDown();
                }
            }
            else if (!keyboardstate[GetKey(OpenTK.Input.Key.LControl)])
            {
                ActiveMaterial -= Mouse.WheelDelta;
                ActiveMaterial = ActiveMaterial % 10;
                while (ActiveMaterial < 0)
                {
                    ActiveMaterial += 10;
                }
            }
            SetAmbientLight(terraincolor);
            //const float alpha = accumulator / dt;
            //Vector3 currentPlayerPosition = currentState * alpha + previousState * (1.0f - alpha);
            UpdateTitleFps(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindTexture(TextureTarget.Texture2D, d_TerrainTextures.terrainTexture);

            GL.MatrixMode(MatrixMode.Modelview);

            Matrix4 camera;
            if (overheadcamera)
            {
                camera = OverheadCamera();
            }
            else
            {
                camera = FppCamera();
            }
            GL.LoadMatrix(ref camera);
            d_The3d.ModelViewMatrix = camera;

            if (BeforeRenderFrame != null) { BeforeRenderFrame(this, new EventArgs()); }

            bool drawgame = guistate != GuiState.MapLoading;
            if (drawgame)
            {
                GL.Disable(EnableCap.Fog);
                DrawSkySphere();
                if (d_Config3d.viewdistance < 128)
                {
                    SetFog();
                }
                DrawPlayers((float)e.Time);
                d_SunMoonRenderer.Draw((float)e.Time);
                DrawTerrain();
                DrawPlayerNames();
                particleEffectBlockBreak.DrawImmediateParticleEffects(e.Time);
                if (ENABLE_DRAW2D)
                {
                    DrawLinesAroundSelectedCube(SelectedBlockPosition);
                }

                DrawCharacters((float)e.Time);
                foreach(Sprite b in new List<Sprite>(sprites))
                {
                    GL.MatrixMode(MatrixMode.Modelview);
                    Vector3 pos = b.position;
                    GL.PushMatrix();
                    GL.Translate(pos.X, pos.Y, pos.Z);
                    GL.Rotate(-LocalPlayerOrientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
                    GL.Rotate(-LocalPlayerOrientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
                    GL.Scale(0.02, 0.02, 0.02);
                    GL.Translate(-b.size / 2, -b.size / 2, 0);
                    //d_Draw2d.Draw2dTexture(night ? moontexture : suntexture, 0, 0, ImageSize, ImageSize, null, Color.White);
                    int? n = null;
                    if (b.animationcount > 0)
                    {
                        n = (int)((DateTime.UtcNow - b.time).TotalSeconds / b.timespan.TotalSeconds
                            * (b.animationcount * b.animationcount - 1));
                    }
                    d_The3d.Draw2dTexture(GetTexture(b.image), 0, 0, b.size, b.size, n, b.animationcount, Color.White, true);
                    GL.PopMatrix();
                    if ((DateTime.UtcNow - b.time) > b.timespan) { sprites.Remove(b); }
                }
                foreach (Bullet b in new List<Bullet>(bullets))
                {
                    if (b.progress < 1f)
                    {
                        b.progress = 1f;
                    }
                    Vector3 pos = b.from;
                    Vector3 dir = (b.to - b.from);
                    float length = dir.Length;
                    dir.Normalize();
                    pos += dir * (b.progress + b.speed * (float)dt);
                    b.progress += b.speed * (float)dt;
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.PushMatrix();
                    GL.Translate(pos.X, pos.Y, pos.Z);
                    GL.Rotate(-LocalPlayerOrientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
                    GL.Rotate(-LocalPlayerOrientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
                    GL.Scale(0.02, 0.02, 0.02);
                    int ImageSize = 4;
                    GL.Translate(-ImageSize / 2, -ImageSize / 2, 0);
                    d_The3d.Draw2dTexture(GetTexture("Sponge.png"), 0, 0, ImageSize, ImageSize, null, Color.White, true);
                    GL.PopMatrix();
                    if (b.progress > length) { bullets.Remove(b); }
                }
                foreach (Projectile b in new List<Projectile>(projectiles))
                {
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.PushMatrix();
                    GL.Translate(b.position.X, b.position.Y, b.position.Z);
                    GL.Rotate(-LocalPlayerOrientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
                    GL.Rotate(-LocalPlayerOrientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
                    GL.Scale(0.02, 0.02, 0.02);
                    int ImageSize = 14;
                    GL.Translate(-ImageSize / 2, -ImageSize / 2, 0);
                    d_The3d.Draw2dTexture(GetTexture("ChemicalGreen.png"), 0, 0, ImageSize, ImageSize, null, Color.White, true);
                    GL.PopMatrix();
                }
                if (ENABLE_DRAW_TEST_CHARACTER)
                {
                    d_CharacterRenderer.DrawCharacter(a, PlayerPositionSpawn, 0, 0, true, (float)dt, GetPlayerTexture(this.LocalPlayerId), new AnimationHint());
                }
                foreach (IModelToDraw m in Models)
                {
                    if (m.Id == selectedmodelid)
                    {
                        //GL.Color3(Color.Red);
                    }
                    m.Draw((float)e.Time);
                    //GL.Color3(Color.White);
                    /*
                    GL.Begin(BeginMode.Triangles);
                    foreach (var tri in m.TrianglesForPicking)
                    {
                        GL.Vertex3(tri.PointA);
                        GL.Vertex3(tri.PointB);
                        GL.Vertex3(tri.PointC);
                    }
                    GL.End();
                    */
                }
                if ((!ENABLE_TPP_VIEW) && ENABLE_DRAW2D)
                {
                    Item item = d_Inventory.RightHand[ActiveMaterial];
                    string img = null;
                    if (item != null)
                    {
                        img = blocktypes[item.BlockId].handimage;
                        if (IronSights)
                        {
                            img = blocktypes[item.BlockId].IronSightsImage;
                        }
                    }
                    if (img == null)
                    {
                        d_Weapon.DrawWeapon((float)e.Time);
                    }
                    else
                    {
                        d_The3d.OrthoMode(Width, Height);
                        d_The3d.Draw2dBitmapFile(img, Width / 2, Height - 512, 512, 512);
                        d_The3d.PerspectiveMode();
                    }
                }
            }
        draw2d:
            SetAmbientLight(Color.White);
            Draw2d();

            //OnResize(new EventArgs());
            d_GlWindow.SwapBuffers();
            mouseleftclick = mouserightclick = false;
            mouseleftdeclick = mouserightdeclick = false;
            if (!startedconnecting) { startedconnecting = true; Connect(); }
        }

        private int GetTexture(string s)
        {
            if (!textures.ContainsKey(s))
            {
                textures[s] = d_The3d.LoadTexture(d_GetFile.GetFile(s));
            }
            return textures[s];
        }

        float projectilegravity = 20f;
        private void UpdateGrenade(Projectile b, float dt)
        {
            Vector3 oldpos = b.position;
            Vector3 newpos = b.position + b.velocity * (float)dt;
            b.velocity.Y += -projectilegravity * (float)dt;
            b.position = GrenadeBounce(oldpos, newpos, ref b.velocity, dt);
            if ((DateTime.UtcNow - b.start).TotalSeconds > b.explodesafter)
            {
                projectiles.Remove(b);
                d_Audio.Play("grenadeexplosion.ogg", b.position);

                sprites.Add(new Sprite() { time = DateTime.UtcNow, image = "ani5.jpg", position = b.position + new Vector3(0, 1, 0), timespan = TimeSpan.FromSeconds(1), size = 200, animationcount = 4 });

                PacketServerExplosion explosion = new PacketServerExplosion();
                explosion.X = b.position.X;
                explosion.Y = b.position.Z;
                explosion.Z = b.position.Y;
                explosion.Range = blocktypes[b.block].ExplosionRange;
                explosion.IsRelativeToPlayerPosition = false;
                explosion.Time = blocktypes[b.block].ExplosionTime;
                explosions.Add(new Explosion() { date = DateTime.UtcNow, explosion = explosion });
                float dist = (LocalPlayerPosition - b.position).Length;
                float dmg = (1 - dist / blocktypes[b.block].ExplosionRange) * blocktypes[b.block].DamageBody;
                if ((int)dmg > 0)
                {
                    ApplyDamageToPlayer((int)dmg);
                }
            }
        }
        float bouncespeedmultiply = 0.5f;
        public Vector3 GrenadeBounce(Vector3 oldposition, Vector3 newposition, ref Vector3 velocity, float dt)
        {
            bool ismoving = velocity.Length > 100f * dt;
            float modelheight = walldistance;
            oldposition.Y += walldistance;
            newposition.Y += walldistance;

            //Math.Floor() is needed because casting negative values to integer is not floor.
            Vector3i oldpositioni = new Vector3i((int)Math.Floor(oldposition.X),
                (int)Math.Floor(oldposition.Z),
                (int)Math.Floor(oldposition.Y));
            Vector3 playerposition = newposition;
            //left
            {
                var qnewposition = newposition + new Vector3(0, 0, walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.Z - oldposition.Z > 0)
                {
                    if (!newempty)
                    {
                        velocity.Z = -velocity.Z;
                        velocity *= bouncespeedmultiply;
                        if (ismoving)
                        {
                            d_Audio.Play("grenadebounce.ogg", newposition);
                        }
                        //playerposition.Z = oldposition.Z - newposition.Z;
                    }
                }
            }
            //front
            {
                var qnewposition = newposition + new Vector3(walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.X - oldposition.X > 0)
                {
                    if (!newempty)
                    {
                        velocity.X = -velocity.X;
                        velocity *= bouncespeedmultiply;
                        if (ismoving)
                        {
                            d_Audio.Play("grenadebounce.ogg", newposition);
                        }
                        //playerposition.X = oldposition.X - newposition.X;
                    }
                }
            }
            //top
            {
                var qnewposition = newposition + new Vector3(0, -walldistance, 0);
                int x = (int)Math.Floor(qnewposition.X);
                int y = (int)Math.Floor(qnewposition.Z);
                int z = (int)Math.Floor(qnewposition.Y);
                float a = walldistance;
                bool newfull = (!IsTileEmptyForPhysics(x, y, z))
                    || (qnewposition.X - Math.Floor(qnewposition.X) <= a && (!IsTileEmptyForPhysics(x - 1, y, z)) && (IsTileEmptyForPhysics(x - 1, y, z + 1)))
                    || (qnewposition.X - Math.Floor(qnewposition.X) >= (1 - a) && (!IsTileEmptyForPhysics(x + 1, y, z)) && (IsTileEmptyForPhysics(x + 1, y, z + 1)))
                    || (qnewposition.Z - Math.Floor(qnewposition.Z) <= a && (!IsTileEmptyForPhysics(x, y - 1, z)) && (IsTileEmptyForPhysics(x, y - 1, z + 1)))
                    || (qnewposition.Z - Math.Floor(qnewposition.Z) >= (1 - a) && (!IsTileEmptyForPhysics(x, y + 1, z)) && (IsTileEmptyForPhysics(x, y + 1, z + 1)));
                if (newposition.Y - oldposition.Y < 0)
                {
                    if (newfull)
                    {
                        velocity.Y = -velocity.Y;
                        velocity *= bouncespeedmultiply;
                        if (ismoving)
                        {
                            d_Audio.Play("grenadebounce.ogg", newposition);
                        }
                        //playerposition.Y = oldposition.Y - newposition.Y;
                    }
                }
            }
            //right
            {
                var qnewposition = newposition + new Vector3(0, 0, -walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.Z - oldposition.Z < 0)
                {
                    if (!newempty)
                    {
                        velocity.Z = -velocity.Z;
                        velocity *= bouncespeedmultiply;
                        if (ismoving)
                        {
                            d_Audio.Play("grenadebounce.ogg", newposition);
                        }
                        //playerposition.Z = oldposition.Z - newposition.Z;
                    }
                }
            }
            //back
            {
                var qnewposition = newposition + new Vector3(-walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.X - oldposition.X < 0)
                {
                    if (!newempty)
                    {
                        velocity.X = -velocity.X;
                        velocity *= bouncespeedmultiply;
                        if (ismoving)
                        {
                            d_Audio.Play("grenadebounce.ogg", newposition);
                        }
                        //playerposition.X = oldposition.X - newposition.X;
                    }
                }
            }
            //bottom
            {
                var qnewposition = newposition + new Vector3(0, modelheight, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Y - oldposition.Y > 0)
                {
                    if (!newempty)
                    {
                        velocity.Y = -velocity.Y;
                        velocity *= bouncespeedmultiply;
                        if (ismoving)
                        {
                            d_Audio.Play("grenadebounce.ogg", newposition);
                        }
                        //playerposition.Y = oldposition.Y - newposition.Y;
                    }
                }
            }
            //ok:
            playerposition.Y -= walldistance;
            return playerposition;
        }
        float walldistance = 0.3f;

        Dictionary<string, int> textures = new Dictionary<string, int>();
        bool startedconnecting;
        private void SetFog()
        {
            float density = 0.3f;
            //float[] fogColor = new[] { 1f, 1f, 1f, 1.0f };
            float[] fogColor;
            if (SkySphereNight && (!shadowssimple))//d_Shadows.GetType() != typeof(ShadowsSimple))
            {
                fogColor = new[] { 0f, 0f, 0f, 1.0f };
            }
            else
            {
                fogColor = new[] { (float)clearcolor.R / 256, (float)clearcolor.G / 256, (float)clearcolor.B / 256, (float)clearcolor.A / 256 };
            }
            GL.Enable(EnableCap.Fog);
            GL.Hint(HintTarget.FogHint, HintMode.Nicest);
            GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
            GL.Fog(FogParameter.FogColor, fogColor);
            GL.Fog(FogParameter.FogDensity, density);
            float fogsize = 10;
            if (d_Config3d.viewdistance <= 64)
            {
                fogsize = 5;
            }
            float fogstart = d_Config3d.viewdistance - fogsize;
            GL.Fog(FogParameter.FogStart, fogstart);
            GL.Fog(FogParameter.FogEnd, fogstart + fogsize);
        }
        public event EventHandler BeforeRenderFrame;
        bool ENABLE_DRAW2D = true;
        int screenshotflash;
        int playertexturedefault = -1;
        Dictionary<string, int> playertextures = new Dictionary<string, int>();
        Dictionary<int, int> monstertextures = new Dictionary<int, int>();
        public string playertexturedefaultfilename = "mineplayer.png";
        Dictionary<string, int> diskplayertextures = new Dictionary<string, int>();
        private int GetPlayerTexture(int playerid)
        {
            if (playertexturedefault == -1)
            {
                playertexturedefault = LoadTexture(playertexturedefaultfilename);
            }
            Player player = this.players[playerid];
            if (player.Type == PlayerType.Monster)
            {
                if (!monstertextures.ContainsKey(player.MonsterType))
                {
                    string skinfile = d_DataMonsters.MonsterSkin[this.players[playerid].MonsterType];
                    using (Bitmap bmp = new Bitmap(d_GetFile.GetFile(skinfile)))
                    {
                        monstertextures[player.MonsterType] = d_The3d.LoadTexture(bmp);
                    }
                }
                return monstertextures[player.MonsterType];
            }
            if (!string.IsNullOrEmpty(player.Texture))
            {
                if (!diskplayertextures.ContainsKey(player.Texture))
                {
                    try
                    {
                        diskplayertextures[player.Texture] = d_The3d.LoadTexture(d_GetFile.GetFile(player.Texture));
                    }
                    catch
                    {
                        diskplayertextures[player.Texture] = playertexturedefault; // invalid
                    }
                }
                return diskplayertextures[player.Texture];
            }
            List<string> players = new List<string>();
            foreach (var k in d_Clients.Players)
            {
                if (!k.Value.Name.Equals("Local", StringComparison.InvariantCultureIgnoreCase))
                {
                    players.Add(k.Value.Name);
                }
            }
            playerskindownloader.Update(players.ToArray(), playertextures, playertexturedefault);
            string playername;
            if (playerid == this.LocalPlayerId)
            {
                playername = connectdata.Username;
            }
            else
            {
                playername = d_Clients.Players[playerid].Name;
            }
            if (playername == null)
            {
                playername = "";
            }
            if (playertextures.ContainsKey(playername))
            {
                return playertextures[playername];
            }
            return playertexturedefault;
        }
        public PlayerSkinDownloader playerskindownloader { get; set; }
        public bool ENABLE_TPP_VIEW = false;
        AnimationState a = new AnimationState();
        public bool ENABLE_DRAW_TEST_CHARACTER = false;
        int skyspheretexture = -1;
        int skyspherenighttexture = -1;
        public SkySphere skysphere = new SkySphere();
        private void DrawSkySphere()
        {
            if (skyspheretexture == -1)
            {
                skyspheretexture = LoadTexture("skysphere.png");
                skyspherenighttexture = LoadTexture("skyspherenight.png");
            }
            int texture = SkySphereNight ? skyspherenighttexture : skyspheretexture;
            if (shadowssimple) //d_Shadows.GetType() == typeof(ShadowsSimple))
            {
                texture = skyspheretexture;
            }
            skysphere.SkyTexture = texture;
            skysphere.Draw();
            return;
        }
        NetworkInterpolation interpolation = new NetworkInterpolation();
        Dictionary<int, PlayerDrawInfo> playerdrawinfo = new Dictionary<int, PlayerDrawInfo>();
        class PlayerDrawInfo
        {
            public AnimationState anim = new AnimationState();
            public NetworkInterpolation interpolation = new NetworkInterpolation();
            public Vector3 lastrealpos;
            public Vector3 lastcurpos;
            public byte lastrealheading;
            public byte lastrealpitch;
        }
        class PlayerInterpolationState
        {
            public Vector3 position;
            public byte heading;
            public byte pitch;
        }
        class PlayerInterpolate : IInterpolation
        {
            public object Interpolate(object a, object b, float progress)
            {
                PlayerInterpolationState aa = a as PlayerInterpolationState;
                PlayerInterpolationState bb = b as PlayerInterpolationState;
                PlayerInterpolationState cc = new PlayerInterpolationState();
                cc.position = aa.position + (bb.position - aa.position) * progress;
                cc.heading = (byte)AngleInterpolation.InterpolateAngle256(aa.heading, bb.heading, progress);
                cc.pitch = (byte)AngleInterpolation.InterpolateAngle256(aa.pitch, bb.pitch, progress);
                return cc;
            }
        }
        double totaltime;
        double lastdrawplayers;
        private void DrawPlayers(float dt)
        {
            totaltime += dt;
            foreach (var k in d_Clients.Players)
            {
                if (k.Key == this.LocalPlayerId)
                {
                    continue;
                }
                if (k.Value.Position == null)
                {
                    continue;
                }
                if (!playerdrawinfo.ContainsKey(k.Key))
                {
                    playerdrawinfo[k.Key] = new PlayerDrawInfo();
                    NetworkInterpolation n = new NetworkInterpolation();
                    n.req = new PlayerInterpolate();
                    n.DELAY = 0.5f;
                    n.EXTRAPOLATE = true;
                    n.EXTRAPOLATION_TIME = 0.3f;
                    playerdrawinfo[k.Key].interpolation = n;
                }
                playerdrawinfo[k.Key].interpolation.DELAY = (float)Math.Max(0.05, ServerInfo.ServerPing.RoundtripTime.TotalSeconds);
                PlayerDrawInfo info = playerdrawinfo[k.Key];
                Vector3 realpos = k.Value.Position.Value;
                bool redraw = false;
                if (totaltime - lastdrawplayers >= 0.1)
                {
                    redraw = true;
                    lastdrawplayers = totaltime;
                }
                if (realpos != info.lastrealpos
                    || k.Value.Heading != info.lastrealheading
                    || k.Value.Pitch != info.lastrealpitch
                    || redraw)
                {
                    info.interpolation.AddNetworkPacket(
                        new PlayerInterpolationState()
                        {
                            position = realpos,
                            heading = k.Value.Heading,
                            pitch = k.Value.Pitch,
                        },
                        totaltime);
                }
                var curstate = ((PlayerInterpolationState)info.interpolation.InterpolatedState(totaltime));
                if (curstate == null)
                {
                    curstate = new PlayerInterpolationState();
                }
                //do not interpolate player position if player is controlled by game world
                if (EnablePlayerUpdatePosition.ContainsKey(k.Key) && !EnablePlayerUpdatePosition[k.Key])
                {
                    curstate.position = k.Value.Position.Value;
                }
                Vector3 curpos = curstate.position;
                bool moves = curpos != info.lastcurpos;
                info.lastcurpos = curpos;
                info.lastrealpos = realpos;
                info.lastrealheading = k.Value.Heading;
                info.lastrealpitch = k.Value.Pitch;
                if (!d_FrustumCulling.SphereInFrustum(curpos.X, curpos.Y, curpos.Z, 3))
                {
                    continue;
                }
                if (!IsChunkRendered((int)curpos.X / chunksize, (int)curpos.Z / chunksize, (int)curpos.Y / chunksize))
                {
                    continue;
                }
                float shadow = (float)d_Shadows.MaybeGetLight((int)curpos.X, (int)curpos.Z, (int)curpos.Y) / d_Shadows.maxlight;
                GL.Color3(shadow, shadow, shadow);
                Vector3 FeetPos = curpos;
                var animHint = d_Clients.Players[k.Key].AnimationHint;
                if (k.Value.Type == PlayerType.Player)
                {
                    var r = GetCharacterRenderer(k.Value.Model);
                    r.SetAnimation("walk");
                    r.DrawCharacter(info.anim, FeetPos, (byte)(-curstate.heading - 256 / 4), curstate.pitch, moves, dt, GetPlayerTexture(k.Key), animHint);
                    //DrawCharacter(info.anim, FeetPos,
                    //    curstate.heading, curstate.pitch, moves, dt, GetPlayerTexture(k.Key), animHint);
                }
                else
                {
                    var r = MonsterRenderers[d_DataMonsters.MonsterCode[k.Value.MonsterType]];
                    r.SetAnimation("walk");
                    //curpos += new Vector3(0, -CharacterPhysics.walldistance, 0); //todos
                    r.DrawCharacter(info.anim, curpos,
                        (byte)(-curstate.heading - 256 / 4), curstate.pitch,
                        moves, dt, GetPlayerTexture(k.Key), animHint);
                }
                GL.Color3(1f, 1f, 1f);
            }
            if (ENABLE_TPP_VIEW)
            {
                float shadow = (float)d_Shadows.MaybeGetLight(
                    (int)LocalPlayerPosition.X,
                    (int)LocalPlayerPosition.Z,
                    (int)LocalPlayerPosition.Y)
                    / d_Shadows.maxlight;
                GL.Color3(shadow, shadow, shadow);
                var r = GetCharacterRenderer(d_Clients.Players[LocalPlayerId].Model);
                r.SetAnimation("walk");
                r.DrawCharacter
                    (localplayeranim, LocalPlayerPosition,
                    (byte)(-NetworkHelper.HeadingByte(LocalPlayerOrientation) - 256 / 4),
                    NetworkHelper.PitchByte(LocalPlayerOrientation),
                    lastlocalplayerpos != LocalPlayerPosition, dt, GetPlayerTexture(this.LocalPlayerId), localplayeranimationhint);
                lastlocalplayerpos = LocalPlayerPosition;
                GL.Color3(1f, 1f, 1f);
            }
        }
        ICharacterRenderer GetCharacterRenderer(string modelfilename)
        {
            if (!MonsterRenderers.ContainsKey(modelfilename))
            {
                try
                {
                    string[] lines = MyStream.ReadAllLines(d_GetFile.GetFile(modelfilename));
                    var renderer = new CharacterRendererMonsterCode();
                    renderer.Load(new List<string>(lines));
                    MonsterRenderers[modelfilename] = renderer;
                }
                catch
                {
                    MonsterRenderers[modelfilename] = GetCharacterRenderer("player.txt"); // todo invalid.txt
                }
            }
            return MonsterRenderers[modelfilename];
        }
        Vector3 lastlocalplayerpos;
        AnimationState localplayeranim = new AnimationState();
        bool overheadcamera = false;
        Kamera overheadcameraK = new Kamera();
        Matrix4 FppCamera()
        {
            Vector3 forward = VectorTool.ToVectorInFixedSystem(0, 0, 1, player.playerorientation.X, player.playerorientation.Y);
            Vector3 cameraEye;
            Vector3 cameraTarget;
            Vector3 playerEye = player.playerposition + new Vector3(0, CharacterEyesHeight, 0);
            if (!ENABLE_TPP_VIEW)
            {
                cameraEye = playerEye;
                cameraTarget = playerEye + forward;
            }
            else
            {
                cameraEye = playerEye + Vector3.Multiply(forward, -tppcameradistance);
                cameraTarget = playerEye;
                float currentTppcameradistance = tppcameradistance;
                LimitThirdPersonCameraToWalls(ref cameraEye, cameraTarget, ref currentTppcameradistance);
            }
            return Matrix4.LookAt(cameraEye, cameraTarget, up);
        }
        Matrix4 OverheadCamera()
        {
            Vector3 cameraEye = overheadcameraK.Position;
            Vector3 cameraTarget = overheadcameraK.Center + new Vector3(0, CharacterEyesHeight, 0);
            float currentOverheadcameradistance = overheadcameradistance;
            LimitThirdPersonCameraToWalls(ref cameraEye, cameraTarget, ref currentOverheadcameradistance);
            return Matrix4.LookAt(cameraEye, cameraTarget, up);
        }
        //Don't allow to look through walls.
        private void LimitThirdPersonCameraToWalls(ref Vector3 eye, Vector3 target, ref float curtppcameradistance)
        {
            var ray_start_point = target;
            var raytarget = eye;

            var pick = new Line3D();
            var raydir = (raytarget - ray_start_point);
            raydir.Normalize();
            raydir = Vector3.Multiply(raydir, tppcameradistance + 1);
            pick.Start = ray_start_point;
            pick.End = ray_start_point + raydir;

            //pick terrain
            var s = new BlockOctreeSearcher();
            s.StartBox = new Box3D(0, 0, 0, BitTools.NextPowerOfTwo((uint)Math.Max(d_Map.MapSizeX, Math.Max(d_Map.MapSizeY, d_Map.MapSizeZ))));
            List<BlockPosSide> pick2 = new List<BlockPosSide>(s.LineIntersection(IsTileEmptyForPhysics, getblockheight, pick));
            pick2.Sort((a, b) => { return (a.pos - ray_start_point).Length.CompareTo((b.pos - ray_start_point).Length); });
            if (pick2.Count > 0)
            {
                var pickdistance = (pick2[0].pos - target).Length;
                curtppcameradistance = Math.Min(pickdistance - 1, curtppcameradistance);
                if (curtppcameradistance < 0.3f) { curtppcameradistance = 0.3f; }
            }

            Vector3 cameraDirection = target - eye;
            raydir.Normalize();
            eye = target + Vector3.Multiply(raydir, curtppcameradistance);
        }
        AnimationState v0anim = new AnimationState();
        void DrawCharacters(float dt)
        {
            foreach (ICharacterToDraw v0 in Characters)
            {
                v0.Draw(dt);
            }
        }
        Dictionary<string, ICharacterRenderer> MonsterRenderers = new Dictionary<string, ICharacterRenderer>();
        GuiState guistate;
        enum GuiState
        {
            Normal,
            EscapeMenu,
            Inventory,
            MapLoading,
            CraftingRecipes,
            EditText,
            ModalDialog,
        }
        private void DrawMouseCursor()
        {
            d_The3d.Draw2dBitmapFile(Path.Combine("gui", "mousecursor.png"), mouse_current.X, mouse_current.Y, 32, 32);
        }
        Size? aimsize;
        private void Draw2d()
        {
            d_The3d.OrthoMode(Width, Height);
            switch (guistate)
            {
                case GuiState.Normal:
                    {
                        if (!ENABLE_DRAW2D)
                        {
                            if (GuiTyping == TypingState.Typing)
                            {
                                d_HudChat.DrawChatLines(true);
                                d_HudChat.DrawTypingBuffer();
                            }
                            d_The3d.PerspectiveMode();
                            return;
                        }
                        if (cameratype != CameraType.Overhead)
                        {
                            DrawAim();
                        }
                        DrawMaterialSelector();
                        DrawPlayerHealth();
                        DrawEnemyHealthBlock();
                        DrawCompass();
                        d_HudChat.DrawChatLines(GuiTyping == TypingState.Typing);
                        if (GuiTyping == TypingState.Typing)
                        {
                            d_HudChat.DrawTypingBuffer();
                        }
                        DrawAmmo();
                        DrawDialogs();
                    }
                    break;
                case GuiState.EscapeMenu:
                    {
                        DrawDialogs();
                        EscapeMenuDraw();
                    }
                    break;
                case GuiState.Inventory:
                    {
                        DrawDialogs();
                        //d_The3d.ResizeGraphics(Width, Height);
                        //d_The3d.OrthoMode(d_HudInventory.ConstWidth, d_HudInventory.ConstHeight);
                        d_HudInventory.Draw();
                        //d_The3d.PerspectiveMode();
                    }
                    break;
                case GuiState.MapLoading:
                    {
                        MapLoadingDraw();
                    }
                    break;
                case GuiState.CraftingRecipes:
                    {
                        DrawCraftingRecipes();
                    }
                    break;
                case GuiState.EditText:
                    {
                        DrawDialogs();
                        d_HudTextEditor.Render();
                    }
                    break;
                case GuiState.ModalDialog:
                    {
                        DrawDialogs();
                    }
                    break;
                default:
                    throw new Exception();
            }
            //d_The3d.OrthoMode(Width, Height);
            if (ENABLE_DRAWFPS)
            {
                d_The3d.Draw2dText(fpstext, 20f, 20f, d_HudChat.ChatFontSize, Color.White);
            }
            if (ENABLE_DRAWFPSHISTORY)
            {
                d_FpsHistoryGraphRenderer.DrawFpsHistoryGraph();
            }
            if (ENABLE_DRAWPOSITION)
            {
                string postext = "X: " + Math.Floor(player.playerposition.X) + "; Y: " + Math.Floor(player.playerposition.Z) + "; Z: " + Math.Floor(player.playerposition.Y);
                d_The3d.Draw2dText(postext, 100f, 460f, d_HudChat.ChatFontSize, Color.White);
            }
            if (drawblockinfo)
            {
                DrawBlockInfo();
            }
            if (FreeMouse)
            {
                DrawMouseCursor();
            }
            if (screenshotflash > 0)
            {
                DrawScreenshotFlash();
                screenshotflash--;
            }
            double lagSeconds = (DateTime.UtcNow - LastReceived).TotalSeconds;
            if (lagSeconds >= DISCONNECTED_ICON_AFTER_SECONDS && lagSeconds < 60 * 60 * 24)
            {
                d_The3d.Draw2dBitmapFile("disconnected.png", Width - 100, 50, 50, 50);
                d_The3d.Draw2dText(((int)lagSeconds).ToString(), Width - 100, 50 + 50 + 10, 12, Color.White);
            }
            d_The3d.PerspectiveMode();
        }

        private void DrawAmmo()
        {
            Item item = d_Inventory.RightHand[ActiveMaterial];
            if (item != null && item.ItemClass == ItemClass.Block)
            {
                if (blocktypes[item.BlockId].IsPistol)
                {
                    int loaded = 0;
                    if (LoadedAmmo.ContainsKey(item.BlockId))
                    {
                        loaded = LoadedAmmo[item.BlockId];
                    }
                    int total = 0;
                    if (TotalAmmo.ContainsKey(item.BlockId))
                    {
                        total = TotalAmmo[item.BlockId];
                    }
                    string s = string.Format("{0}/{1}", loaded, total - loaded);
                    d_The3d.Draw2dText(s, Width - d_The3d.TextSize(s, 18).Width - 50,
                        Height - d_The3d.TextSize(s, 18).Height - 50, 18, loaded == 0 ? Color.Red : Color.White);
                }
            }
        }

        bool ammostarted;
        public Dictionary<int, int> TotalAmmo = new Dictionary<int, int>();
        public Dictionary<int, int> LoadedAmmo = new Dictionary<int, int>();

        private void DrawDialogs()
        {
            foreach (var d in dialogs.Values)
            {
                int x = Width / 2 - d.Width / 2;
                int y = Height / 2 - d.Height / 2;
                foreach (var w in d.Widgets)
                {
                    if (w.Text != null)
                    {
                        w.Text = w.Text.Replace("!SERVER_IP!", ServerInfo.connectdata.Ip);
                        w.Text = w.Text.Replace("!SERVER_PORT!", ServerInfo.connectdata.Port.ToString());
                        if (w.Font != null)
                        {
                            Font font = new Font(ValidFont(w.Font.FamilyName), w.Font.Size, (FontStyle)w.Font.FontStyle);
                            d_The3d.Draw2dText(w.Text, font, w.X + x, w.Y + y, Color.FromArgb(w.Color));
                        }
                        else
                        {
                            d_The3d.Draw2dText(w.Text, w.X + x, w.Y + y, 12, Color.FromArgb(w.Color));
                        }
                    }
                    if (w.Image == "Solid")
                    {
                        d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), w.X + x, w.Y + y, w.Width, w.Height, null, Color.FromArgb(w.Color));
                    }
                    else if (w.Image != null)
                    {
                        d_The3d.Draw2dBitmapFile(w.Image + ".png", w.X + x, w.Y + y, w.Width, w.Height);
                    }
                }
            }
        }
        
        string[] AllowedFonts = new string[] { "Verdana" };

        private string ValidFont(string family)
        {
            foreach (string s in AllowedFonts)
            {
                if (s.Equals(family, StringComparison.InvariantCultureIgnoreCase))
                {
                    return family;
                }
            }
            return AllowedFonts[0];
        }

        public int DISCONNECTED_ICON_AFTER_SECONDS = 10;
        private void DrawScreenshotFlash()
        {
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), 0, 0, Width, Height, null, Color.White);
            string screenshottext = "Screenshot";
            d_The3d.Draw2dText(screenshottext, xcenter(d_The3d.TextSize(screenshottext, 50).Width),
                ycenter(d_The3d.TextSize(screenshottext, 50).Height), 50, Color.White);
        }
        private void DrawBlockInfo()
        {
            int x = (int)SelectedBlockPosition.X;
            int y = (int)SelectedBlockPosition.Z;
            int z = (int)SelectedBlockPosition.Y;
            //string info = "None";
            if (!MapUtil.IsValidPos(d_Map, x, y, z))
            {
                return;
            }
            int blocktype = d_Map.GetBlock(x, y, z);
            if (!d_Data.IsValid[blocktype])
            {
                return;
            }
            currentAttackedBlock = new Vector3i(x, y, z);
            DrawEnemyHealthBlock();
            /*
            int blocktype = d_Map.GetBlock(x, y, z);
            if (d_Data.IsValid[blocktype])
            {
                info = d_Data.Name[blocktype];
            }
            d_The3d.Draw2dText(info, Width * 0.5f - d_The3d.TextSize(info, 18f).Width / 2, 30f, 18f, Color.White);
            */
        }
        private void DrawAim()
        {
            if (aimsize == null)
            {
                using (var targetbmp = new Bitmap(d_GetFile.GetFile("target.png")))
                {
                    aimsize = targetbmp.Size;
                }
            }
            float aimwidth = aimsize.Value.Width;
            float aimheight = aimsize.Value.Height;

            if (CurrentAimRadius > 1)
            {
                float fov = d_The3d.fov;
                if (d_The3d.currentfov != null)
                {
                    fov = d_The3d.currentfov();
                }
                Circle3i(Width / 2, Height / 2, CurrentAimRadius * d_The3d.fov / fov);
            }
            d_The3d.Draw2dBitmapFile("target.png", Width / 2 - aimwidth / 2, Height / 2 - aimheight / 2, aimwidth, aimheight);
        }
        PointF GetAim()
        {
            if (CurrentAimRadius <= 1)
            {
                return new PointF(0, 0);
            }
        retry:
            float x = (float)(rnd.NextDouble() - 0.5f) * CurrentAimRadius * 2;
            float y = (float)(rnd.NextDouble() - 0.5f) * CurrentAimRadius * 2;
            float dist1 = (float)Math.Sqrt(x * x + y * y);
            if (dist1 > CurrentAimRadius)
            {
                goto retry;
            }
            return new PointF(x, y);
        }
        float CurrentAimRadius
        {
            get
            {
                Item item = d_Inventory.RightHand[ActiveMaterial];
                if (item == null || item.ItemClass != ItemClass.Block)
                {
                    return 0;
                }
                float radius = ((float)blocktypes[item.BlockId].AimRadius / 800) * Width;
                if (IronSights)
                {
                    radius = ((float)blocktypes[item.BlockId].IronSightsAimRadius / 800) * Width;
                }
                return radius + RadiusWhenMoving * radius * (Math.Min(playervelocity.Length / movespeed, 1));
            }
        }
        float RadiusWhenMoving = 0.3f;
        float CurrentRecoil
        {
            get
            {
                Item item = d_Inventory.RightHand[ActiveMaterial];
                if (item == null || item.ItemClass != ItemClass.Block)
                {
                    return 0;
                }
                return (float)blocktypes[item.BlockId].Recoil;
            }
        }
        void Circle3i(float x, float y, float radius)
        {
            float angle;
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Texture2D);
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.LineWidth(1.0f);
            GL.Begin(BeginMode.LineLoop);
            int n = 32;
            for (int i = 0; i < n; i++)
            {
                angle = (float)(i * 2 * Math.PI / n);
                GL.Vertex2(x + (Math.Cos(angle) * radius), y + (Math.Sin(angle) * radius));
            }
            GL.End();
            GL.Enable(EnableCap.Texture2D);
            GL.PopMatrix();
        }
        private void DrawPlayerNames()
        {
            foreach (KeyValuePair<int, Player> k in d_Clients.Players)
            {
                if ((k.Value.Position == null) ||
                    (k.Key == this.LocalPlayerId) || (k.Value.Name == "")
                    || (!playerdrawinfo.ContainsKey(k.Key))
                    || (playerdrawinfo[k.Key].interpolation == null))
                {
                    continue;
                }
                //todo if picking
                if (((LocalPlayerPosition - k.Value.Position.Value).Length < 20)
                    || Keyboard[GetKey(OpenTK.Input.Key.AltLeft)] || Keyboard[GetKey(OpenTK.Input.Key.AltRight)])
                {
                    string name = k.Value.Name;
                    var ppos = playerdrawinfo[k.Key].interpolation.InterpolatedState(totaltime);
                    if (ppos != null)
                    {
                        Vector3 pos = ((PlayerInterpolationState)ppos).position;
                        float shadow = (float)d_Shadows.MaybeGetLight((int)pos.X, (int)pos.Z, (int)pos.Y) / d_Shadows.maxlight;
                        //do not interpolate player position if player is controlled by game world
                        if (EnablePlayerUpdatePosition.ContainsKey(k.Key) && !EnablePlayerUpdatePosition[k.Key])
                        {
                            pos = k.Value.Position.Value;
                        }
                        GL.PushMatrix();
                        GL.Translate(pos.X, pos.Y + CharacterModelHeight + 0.8f, pos.Z);
                        if (k.Value.Type == PlayerType.Monster)
                        {
                            GL.Translate(0, 1f, 0);
                        }
                        GL.Rotate(-player.playerorientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
                        GL.Rotate(-player.playerorientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
                        GL.Scale(0.02, 0.02, 0.02);

                        //Color c = Color.FromArgb((int)(shadow * 255), (int)(shadow * 255), (int)(shadow * 255));
                        //Todo: Can't change text color because text has outline anyway.
                        if (k.Value.Type == PlayerType.Monster)
                        {
                            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), -26, -11, 52, 12, null, Color.FromArgb(0, Color.Black));
                            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), -25, -10, 50 * (k.Value.Health / 20f), 10, null, Color.FromArgb(0, Color.Red));
                        }
                        d_The3d.Draw2dText(name, -d_The3d.TextSize(name, 14).Width / 2, 0, 14, Color.White, true);
                        //                        GL.Translate(0, 1, 0);
                        GL.PopMatrix();
                    }
                }
            }
        }
        bool drawblockinfo = false;
        void CraftingMouse()
        {
            if (okrecipes == null)
            {
                return;
            }
            int menustartx = xcenter(600);
            int menustarty = ycenter(okrecipes.Count * 80);
            if (mouse_current.Y >= menustarty && mouse_current.Y < menustarty + okrecipes.Count * 80)
            {
                craftingselectedrecipe = (mouse_current.Y - menustarty) / 80;
            }
            else
            {
                //craftingselectedrecipe = -1;
            }
            if (mouseleftclick)
            {
                if (okrecipes.Count != 0)
                {
                    craftingrecipeselected(okrecipes[craftingselectedrecipe]);
                }
                mouseleftclick = false;
                GuiStateBackToGame();
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
        int ENABLE_LAG = 0;
        bool ENABLE_DRAWFPS = false;
        bool ENABLE_DRAWFPSHISTORY = false;
        bool ENABLE_DRAWPOSITION = false;

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
        public ParticleEffectBlockBreak particleEffectBlockBreak = new ParticleEffectBlockBreak();
        Random rnd = new Random();
        public int ActiveMaterial { get; set; }
        private void DrawLinesAroundSelectedCube(Vector3 posx)
        {
            float pickcubeheight = 1;
            if (posx != new Vector3(-1, -1, -1))
            {
                pickcubeheight = getblockheight((int)posx.X, (int)posx.Z, (int)posx.Y);
            }
            Vector3 pos = posx;
            pos += new Vector3(0.5f, pickcubeheight * 0.5f, 0.5f);
            GL.LineWidth(2);
            float size = 0.51f;
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.White);
            DrawLineLoop(new[]{
                new Vector3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size),
            });
            DrawLineLoop(new[]{
                new Vector3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size),
            });
            DrawLineLoop(new[]{
                new Vector3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size),
            });
            DrawLineLoop(new[]{
                new Vector3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size),
            });
            DrawLineLoop(new[]{
                new Vector3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size),
            });
            DrawLineLoop(new[]{
                new Vector3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size),
                new Vector3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size),
            });

            GL.Color3(Color.White);
            GL.End();
        }
        void DrawLineLoop(Vector3[] points)
        {
            GL.Vertex3(points[0]);
            GL.Vertex3(points[1]);
            GL.Vertex3(points[1]);
            GL.Vertex3(points[2]);
            GL.Vertex3(points[2]);
            GL.Vertex3(points[3]);
            GL.Vertex3(points[3]);
            GL.Vertex3(points[0]);
        }
        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
        }
        DateTime lasttitleupdate;
        int fpscount = 0;
        string fpstext = "";
        float longestframedt = 0;
        Dictionary<string, string> performanceinfo = new Dictionary<string, string>();
        public Dictionary<string, string> PerformanceInfo { get { return performanceinfo; } }
        int lastchunkupdates;
        private void UpdateTitleFps(FrameEventArgs e)
        {
            fpscount++;
            longestframedt = (float)Math.Max(longestframedt, e.Time);
            TimeSpan elapsed = (DateTime.Now - lasttitleupdate);
            d_FpsHistoryGraphRenderer.Update((float)e.Time);
            if (elapsed.TotalSeconds >= 1)
            {
                string fpstext1 = "";
                lasttitleupdate = DateTime.Now;
                fpstext1 += "FPS: " + (int)((float)fpscount / elapsed.TotalSeconds);
                fpstext1 += string.Format(" (min: {0})", (int)(1f / longestframedt));
                longestframedt = 0;
                fpscount = 0;
                performanceinfo["fps"] = fpstext1;
                performanceinfo["triangles"] = string.Format(Language.Triangles, TrianglesCount());
                int chunkupdates = ChunkUpdates;
                performanceinfo["chunk updates"] = string.Format(Language.ChunkUpdates, (chunkupdates - lastchunkupdates));
                lastchunkupdates = ChunkUpdates;

                string s = "";
                List<string> l = new List<string>(performanceinfo.Values);
                int perline = 2;
                for (int i = 0; i < l.Count; i++)
                {
                    s += l[i];
                    if ((i % perline == 0) && (i != l.Count - 1))
                    {
                        s += ", ";
                    }
                    if (i % perline != 0)
                    {
                        s += Environment.NewLine;
                    }
                }
                fpstext = s;
            }
            if (!titleset)
            {
                d_GlWindow.Title = applicationname;
                titleset = true;
            }
        }
        bool titleset = false;
        string applicationname = Language.GameName;
        #region ILocalPlayerPosition Members
        public Vector3 LocalPlayerPosition
        {
            get
            {
                if (FollowId != null)
                {
                    if (FollowId == LocalPlayerId)
                    {
                        return player.playerposition;
                    }
                    var curstate = ((PlayerInterpolationState)playerdrawinfo[FollowId.Value].interpolation.InterpolatedState(totaltime));
                    return curstate.position;
                }
                return player.playerposition;
            }
            set
            {
                if (FollowId != null)
                {
                    return;
                }
                player.playerposition = value;
            }
        }
        public Vector3 LocalPlayerOrientation
        {
            get
            {
                if (FollowId != null)
                {
                    if (FollowId == LocalPlayerId)
                    {
                        return player.playerorientation;
                    }
                    var curstate = ((PlayerInterpolationState)playerdrawinfo[FollowId.Value].interpolation.InterpolatedState(totaltime));
                    return HeadingPitchToOrientation(curstate.heading, curstate.pitch);
                }
                return player.playerorientation;
            }
            set
            {
                player.playerorientation = value;
            }
        }
        #endregion
        public void AddChatline(string s)
        {
            d_HudChat.AddChatline(s);
        }
        #region ILocalPlayerPosition Members
        public bool Swimming
        {
            get
            {
                var p = LocalPlayerPosition;
                p += new Vector3(0, Players[LocalPlayerId].EyeHeight, 0);
                if (!MapUtil.IsValidPos(d_Map, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y)))
                {
                    return p.Y < WaterLevel;
                }
                return d_Data.WalkableType1[d_Map.GetBlock((int)p.X, (int)p.Z, (int)p.Y)] == WalkableType.Fluid;
            }
        }
        public bool WaterSwimming
        {
            get
            {
                var p = LocalPlayerPosition;
                p += new Vector3(0, Players[LocalPlayerId].EyeHeight, 0);
                if (!MapUtil.IsValidPos(d_Map, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y)))
                {
                    return p.Y < WaterLevel;
                }
                return d_Data.IsWater[d_Map.GetBlock((int)p.X, (int)p.Z, (int)p.Y)];
            }
        }
        #endregion
        public float WaterLevel { get { return d_Map.MapSizeZ / 2; } set { } }
        Color terraincolor { get { return WaterSwimming ? Color.FromArgb(255, 78, 95, 140) : Color.White; } }
        #region IKeyboard Members
        public OpenTK.Input.KeyboardDevice keyboardstate
        {
            get { return Keyboard; }
        }
        #endregion
        #region IKeyboard Members
        public OpenTK.Input.KeyboardKeyEventArgs keypressed
        {
            get { return keyevent; }
        }
        public OpenTK.Input.KeyboardKeyEventArgs keydepressed
        {
            get { return keyeventup; }
        }
        #endregion
        #region IMap Members
        IMapStorage IMap.Map { get { return d_Map; } }
        #endregion
        AnimationHint localplayeranimationhint = new AnimationHint();
        #region IViewport3d Members
        public AnimationHint LocalPlayerAnimationHint
        {
            get { return localplayeranimationhint; }
            set { localplayeranimationhint = value; }
        }
        #endregion
        #region IViewport3d Members
        public Vector3 PickCubePos { get { return SelectedBlockPosition; } }
        #endregion
        #region IViewport3d Members
        public string LocalPlayerName { get { return connectdata.Username; } }
        #endregion
        public Options Options { get { return options; } set { options = value; } }
        public int Height { get { return d_GlWindow.Height; } }
        public int Width { get { return d_GlWindow.Width; } }
        public OpenTK.Input.KeyboardDevice Keyboard { get { return d_GlWindow.Keyboard; } }
        public OpenTK.Input.MouseDevice Mouse { get { return d_GlWindow.Mouse; } }
        public void Run()
        {
            d_GlWindow.Run();
        }
        public void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (guistate == GuiState.Inventory)
            {
                d_HudInventory.OnKeyPress(e);
            }
        }
        public void DrawMaterialSelector()
        {
            //d_The3d.ResizeGraphics(Width, Height);
            //d_The3d.OrthoMode(d_HudInventory.ConstWidth, d_HudInventory.ConstHeight);
            d_HudInventory.DrawMaterialSelector();
            //d_The3d.PerspectiveMode();
        }

        public Point MouseCurrent
        {
            get { return mouse_current; }
        }

        public Vector3i SelectedBlock()
        {
            Vector3 pos = SelectedBlockPosition;
            if (pos == new Vector3(-1, -1, -1))
            {
                pos = player.playerposition;
            }
            return new Vector3i((int)pos.X, (int)pos.Z, (int)pos.Y);
        }
        private void OnPickUseWithTool(Vector3 pos)
        {
            SendSetBlock(new Vector3(pos.X, pos.Y, pos.Z), BlockSetMode.UseWithTool, d_Inventory.RightHand[ActiveMaterial].BlockId, ActiveMaterial);
        }
        public void OnPick(Vector3 blockpos, Vector3 blockposold, Vector3 pos3d, bool right)
        {
            float xfract = pos3d.X - (float)Math.Floor(pos3d.X);
            float zfract = pos3d.Z - (float)Math.Floor(pos3d.Z);
            int activematerial = (ushort)MaterialSlots[ActiveMaterial];
            int railstart = d_Data.BlockIdRailstart;
            if (activematerial == railstart + (int)RailDirectionFlags.TwoHorizontalVertical
                || activematerial == railstart + (int)RailDirectionFlags.Corners)
            {
                RailDirection dirnew;
                if (activematerial == railstart + (int)RailDirectionFlags.TwoHorizontalVertical)
                {
                    dirnew = PickHorizontalVertical(xfract, zfract);
                }
                else
                {
                    dirnew = PickCorners(xfract, zfract);
                }
                RailDirectionFlags dir = d_Data.Rail[GetTerrainBlock((int)blockposold.X, (int)blockposold.Y, (int)blockposold.Z)];
                if (dir != RailDirectionFlags.None)
                {
                    blockpos = blockposold;
                }
                activematerial = railstart + (int)(dir | DirectionUtils.ToRailDirectionFlags(dirnew));
                //Console.WriteLine(blockposold);
                //Console.WriteLine(xfract + ":" + zfract + ":" + activematerial + ":" + dirnew);
            }
            int x = (short)blockpos.X;
            int y = (short)blockpos.Y;
            int z = (short)blockpos.Z;
            var mode = right ? BlockSetMode.Create : BlockSetMode.Destroy;
            {
                if (IsAnyPlayerInPos(blockpos) || activematerial == 151)
                {
                    return;
                }
                Vector3i v = new Vector3i(x, y, z);
                Vector3i? oldfillstart = fillstart;
                Vector3i? oldfillend = fillend;
                if (mode == BlockSetMode.Create)
                {
                    if (blocktypes[activematerial].IsTool)
                    {
                        OnPickUseWithTool(blockpos);
                        return;
                    }
                    /*
                    if (GameDataManicDigger.IsDoorTile(activematerial))
                    {
                        if (z + 1 == d_Map.MapSizeZ || z == 0) return;
                    }
                    */
                    if (activematerial == d_Data.BlockIdCuboid)
                    {
                        ClearFillArea();

                        if (fillstart != null)
                        {
                            Vector3i f = fillstart.Value;
                            if (!IsFillBlock(d_Map.GetBlock(f.x, f.y, f.z)))
                            {
                                fillarea[f] = d_Map.GetBlock(f.x, f.y, f.z);
                            }
                            SetBlock(f.x, f.y, f.z, d_Data.BlockIdFillStart);


                            FillFill(v, fillstart.Value);
                        }
                        if (!IsFillBlock(d_Map.GetBlock(v.x, v.y, v.z)))
                        {
                            fillarea[v] = d_Map.GetBlock(v.x, v.y, v.z);
                        }
                        SetBlock(v.x, v.y, v.z, d_Data.BlockIdCuboid);
                        fillend = v;
                        RedrawBlock(v.x, v.y, v.z);
                        return;
                    }
                    if (activematerial == d_Data.BlockIdFillStart)
                    {
                        ClearFillArea();
                        if (!IsFillBlock(d_Map.GetBlock(v.x, v.y, v.z)))
                        {
                            fillarea[v] = d_Map.GetBlock(v.x, v.y, v.z);
                        }
                        SetBlock(v.x, v.y, v.z, d_Data.BlockIdFillStart);
                        fillstart = v;
                        fillend = null;
                        RedrawBlock(v.x, v.y, v.z);
                        return;
                    }
                    if (fillarea.ContainsKey(v))// && fillarea[v])
                    {
                        SendFillArea(fillstart.Value, fillend.Value, activematerial);
                        ClearFillArea();
                        fillstart = null;
                        fillend = null;
                        return;
                    }
                }
                else
                {
                    if (blocktypes[activematerial].IsTool)
                    {
                        OnPickUseWithTool(blockpos);
                        return;
                    }
                    //delete fill start
                    if (fillstart != null && fillstart == v)
                    {
                        ClearFillArea();
                        fillstart = null;
                        fillend = null;
                        return;
                    }
                    //delete fill end
                    if (fillend != null && fillend == v)
                    {
                        ClearFillArea();
                        fillend = null;
                        return;
                    }
                }
                if (mode == BlockSetMode.Create && activematerial == d_Data.BlockIdMinecart)
                {
                    /*
                    CommandRailVehicleBuild cmd2 = new CommandRailVehicleBuild();
                    cmd2.x = (short)x;
                    cmd2.y = (short)y;
                    cmd2.z = (short)z;
                    TrySendCommand(MakeCommand(CommandId.RailVehicleBuild, cmd2));
                    */
                    return;
                }
                //if (TrySendCommand(MakeCommand(CommandId.Build, cmd)))
                SendSetBlockAndUpdateSpeculative(activematerial, x, y, z, mode);
            }
        }
        int? BlockInHand()
        {
            Item item = d_Inventory.RightHand[ActiveMaterial];
            if (item != null && item.ItemClass == ItemClass.Block)
            {
                return item.BlockId;
            }
            return null;
        }
        private void SendSetBlockAndUpdateSpeculative(int material, int x, int y, int z, BlockSetMode mode)
        {
            SendSetBlock(new Vector3(x, y, z), mode, material, ActiveMaterial);

            Item item = d_Inventory.RightHand[ActiveMaterial];
            if (item != null && item.ItemClass == ItemClass.Block)
            {
                //int blockid = d_Inventory.RightHand[d_Viewport.ActiveMaterial].BlockId;
                int blockid = material;
                if (mode == BlockSetMode.Destroy)
                {
                    blockid = SpecialBlockId.Empty;
                }
                speculative[new Vector3i(x, y, z)] = new Speculative() { blocktype = d_Map.GetBlock(x, y, z), time = DateTime.UtcNow };
                SetBlock(x, y, z, blockid);
                RedrawBlock(x, y, z);
                OnLocalBuild(x, y, z);
            }
            else
            {
                //TODO
            }
        }
        private void ClearFillArea()
        {
            foreach (var k in fillarea)
            {
                var vv = k.Key;
                SetBlock(vv.x, vv.y, vv.z, k.Value);
                RedrawBlock(vv.x, vv.y, vv.z);
            }
            fillarea.Clear();
        }
        //value is original block.
        Dictionary<Vector3i, int> fillarea = new Dictionary<Vector3i, int>();
        Vector3i? fillstart;
        Vector3i? fillend;
        private int fillAreaLimit = 200;

        private void FillFill(Vector3i a, Vector3i b)
        {
            int startx = Math.Min(a.x, b.x);
            int endx = Math.Max(a.x, b.x);
            int starty = Math.Min(a.y, b.y);
            int endy = Math.Max(a.y, b.y);
            int startz = Math.Min(a.z, b.z);
            int endz = Math.Max(a.z, b.z);
            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    for (int z = startz; z <= endz; z++)
                    {
                        if (fillarea.Count > fillAreaLimit)
                        {
                            ClearFillArea();
                            return;
                        }
                        if (!IsFillBlock(d_Map.GetBlock(x, y, z)))
                        {
                            fillarea[new Vector3i(x, y, z)] = d_Map.GetBlock(x, y, z);
                            SetBlock(x, y, z, d_Data.BlockIdFillArea);
                            RedrawBlock(x, y, z);
                        }
                    }
                }
            }
        }
        bool IsFillBlock(int blocktype)
        {
            return blocktype == d_Data.BlockIdFillArea
                || blocktype == d_Data.BlockIdFillStart
                || blocktype == d_Data.BlockIdCuboid;
        }
        RailDirection PickHorizontalVertical(float xfract, float yfract)
        {
            float x = xfract;
            float y = yfract;
            if (y >= x && y >= (1 - x))
            {
                return RailDirection.Vertical;
            }
            if (y < x && y < (1 - x))
            {
                return RailDirection.Vertical;
            }
            return RailDirection.Horizontal;
        }
        private RailDirection PickCorners(float xfract, float zfract)
        {
            if (xfract < 0.5f && zfract < 0.5f)
            {
                return RailDirection.UpLeft;
            }
            if (xfract >= 0.5f && zfract < 0.5f)
            {
                return RailDirection.UpRight;
            }
            if (xfract < 0.5f && zfract >= 0.5f)
            {
                return RailDirection.DownLeft;
            }
            return RailDirection.DownRight;
        }
        struct Speculative
        {
            public DateTime time;
            public int blocktype;
        }
        Dictionary<Vector3i, Speculative> speculative = new Dictionary<Vector3i, Speculative>();
        public void SendSetBlock(Vector3 vector3, BlockSetMode blockSetMode, int p)
        {
            SendSetBlock(vector3, blockSetMode, p, ActiveMaterial);
        }
        private bool IsAnyPlayerInPos(Vector3 blockpos)
        {
            foreach (var k in players)
            {
                if (k.Value.Position != null)
                {
                    Vector3 playerpos = k.Value.Position.Value;
                    if (IsPlayerInPos(playerpos, blockpos))
                    {
                        return true;
                    }
                }
            }
            return IsPlayerInPos(LocalPlayerPosition, blockpos);
        }
        private bool IsPlayerInPos(Vector3 playerpos, Vector3 blockpos)
        {
            if (Math.Floor(playerpos.X) == blockpos.X
                &&
                (Math.Floor(playerpos.Y + 0.5f) == blockpos.Z
                 || Math.Floor(playerpos.Y + 1 + 0.5f) == blockpos.Z)
                && Math.Floor(playerpos.Z) == blockpos.Y)
            {
                return true;
            }
            return false;
        }
        public void OnNewFrame(double dt)
        {
            foreach (var k in new Dictionary<Vector3i, Speculative>(speculative))
            {
                if ((DateTime.UtcNow - k.Value.time).TotalSeconds > 2)
                {
                    speculative.Remove(k.Key);
                    RedrawBlock(k.Key.x, k.Key.y, k.Key.z);
                }
            }
            if (KeyPressed(GetKey(OpenTK.Input.Key.C)))
            {
                if (PickCubePos != new Vector3(-1, -1, -1))
                {
                    Vector3i pos = new Vector3i((int)PickCubePos.X, (int)PickCubePos.Z, (int)PickCubePos.Y);
                    if (d_Map.GetBlock(pos.x, pos.y, pos.z)
                        == d_Data.BlockIdCraftingTable)
                    {
                        //draw crafting recipes list.
                        CraftingRecipesStart(d_CraftingRecipes, d_CraftingTableTool.GetOnTable(d_CraftingTableTool.GetTable(pos)),
                        (recipe) => { CraftingRecipeSelected(pos, recipe); });
                    }
                }
            }
            ENABLE_FINITEINVENTORY = this.ENABLE_FINITEINVENTORY;
            RailOnNewFrame((float)dt);
        }
        void CraftingRecipeSelected(Vector3i pos, int? recipe)
        {
            if (recipe == null)
            {
                return;
            }
            PacketClientCraft cmd = new PacketClientCraft();
            cmd.X = (short)pos.x;
            cmd.Y = (short)pos.y;
            cmd.Z = (short)pos.z;
            cmd.RecipeId = (short)recipe.Value;
            SendPacketClient(new PacketClient() { PacketId = ClientPacketId.Craft, Craft = cmd });
        }
        private bool KeyPressed(OpenTK.Input.Key key)
        {
            return keypressed != null && keypressed.Key == key;
        }
        private bool KeyDepressed(OpenTK.Input.Key key)
        {
            return keydepressed != null && keydepressed.Key == key;
        }
        Dictionary<int, int> finiteinventory = new Dictionary<int, int>();
        public IEnumerable<ICharacterToDraw> Characters
        {
            get { yield break; }
        }
        private Vector3 playerpositionspawn = new Vector3(15.5f, 64, 15.5f);
        public Vector3 PlayerPositionSpawn { get { return playerpositionspawn; } set { playerpositionspawn = value; } }
        public Vector3 PlayerOrientationSpawn { get { return new Vector3((float)Math.PI, 0, 0); } }
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int, Player> Players { get { return players; } set { players = value; } }

        #region IMapStorage Members
        public void SetChunk(int x, int y, int z, ushort[, ,] chunk)
        {
            d_Map.SetMapPortion(x, y, z, chunk);
        }
        #endregion
        #region IGameMode Members
        public void OnNewMap()
        {
            /*
            int x = d_Map.MapSizeX / 2;
            int y = d_Map.MapSizeY / 2;
            playerpositionspawn = new Vector3(x + 0.5f, MapUtil.blockheight(d_Map, SpecialBlockId.Empty, x, y), y + 0.5f);
            */
            playerpositionspawn = LocalPlayerPosition;
        }
        #endregion
        #region IMapStorage Members
        public void UseMap(byte[, ,] map)
        {
            /*
            this.map.Map = map;
            MapSizeX = map.GetUpperBound(0) + 1;
            MapSizeY = map.GetUpperBound(1) + 1;
            MapSizeZ = map.GetUpperBound(2) + 1;
            shadows.ResetShadows();
            */
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryMax { get; set; }
        #endregion
        #region IGameMode Members
        public double SIMULATIONLAG_SECONDS { get; set; }
        #endregion
        #region ITerrainInfo Members
        public int GetTerrainBlock(int x, int y, int z)
        {
            d_Shadows.OnGetTerrainBlock(x, y, z);
            return GetBlock(x, y, z);
        }
        FastColor white = new FastColor(Color.White);
        public FastColor GetTerrainBlockColor(int x, int y, int z)
        {
            return white;
        }
        public int GetLight(int x, int y, int z)
        {
            //return d_Shadows.GetLight(x, y, z);
            return 15;
        }
        public float LightMaxValue()
        {
            return d_Shadows.maxlight;
        }
        #endregion
        #region IGameMode Members
        public void ModelClick(int selectedmodelid)
        {
        }
        #endregion
        public IMapStorage mapforphysics { get { return this; } }
        public bool ENABLE_FINITEINVENTORY { get; set; }
        BlockType[] blocktypes;
        public int HourDetail = 4;
        public int[] NightLevels;
        public bool ENABLE_PER_SERVER_TEXTURES = false;
        string serverterraintexture;
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        string blobdownloadhash;
        string blobdownloadname;
        MemoryStream blobdownload;
        #region ICurrentSeason Members
        public int CurrentSeason { get; set; }
        #endregion

        public void InventoryClick(InventoryPosition pos)
        {
            PacketClientInventoryAction p = new PacketClientInventoryAction();
            p.A = pos;
            p.Action = InventoryActionType.Click;
            SendPacketClient(new PacketClient() { PacketId = ClientPacketId.InventoryAction, InventoryAction = p });
        }

        public void WearItem(InventoryPosition from, InventoryPosition to)
        {
            PacketClientInventoryAction p = new PacketClientInventoryAction();
            p.A = from;
            p.B = to;
            p.Action = InventoryActionType.WearItem;
            SendPacketClient(new PacketClient() { PacketId = ClientPacketId.InventoryAction, InventoryAction = p });
        }

        public void MoveToInventory(InventoryPosition from)
        {
            PacketClientInventoryAction p = new PacketClientInventoryAction();
            p.A = from;
            p.Action = InventoryActionType.MoveToInventory;
            SendPacketClient(new PacketClient() { PacketId = ClientPacketId.InventoryAction, InventoryAction = p });
        }

        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        public bool ENABLE_FORTRESS = true;
        public void Connect(string serverAddress, int port, string username, string auth)
        {
            iep = new IPEndPoint(IPAddress.Any, port);
            main.Start();
            main.Connect(serverAddress, port);
            this.username = username;
            this.auth = auth;
            byte[] n = CreateLoginPacket(username, auth);
            var msg = main.CreateMessage();
            msg.Write(n);
            main.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
        }
        public void Connect(string serverAddress, int port, string username, string auth, string serverPassword)
        {
            iep = new IPEndPoint(IPAddress.Any, port);
            main.Connect(serverAddress, port);
            this.username = username;
            this.auth = auth;
            byte[] n = CreateLoginPacket(username, auth, serverPassword);
            var msg = main.CreateMessage();
            msg.Write(n);
            main.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
        }
        string username;
        string auth;
        private byte[] CreateLoginPacket(string username, string verificationKey)
        {
            PacketClientIdentification p = new PacketClientIdentification()
            {
                Username = username,
                MdProtocolVersion = GameVersion.Version,
                VerificationKey = verificationKey
            };
            return Serialize(new PacketClient() { PacketId = ClientPacketId.PlayerIdentification, Identification = p });
        }
        private byte[] CreateLoginPacket(string username, string verificationKey, string serverPassword)
        {
            PacketClientIdentification p = new PacketClientIdentification()
            {
                Username = username,
                MdProtocolVersion = GameVersion.Version,
                VerificationKey = verificationKey,
                ServerPassword = serverPassword
            };
            return Serialize(new PacketClient() { PacketId = ClientPacketId.PlayerIdentification, Identification = p });
        }
        IPEndPoint iep;
        public void SendPacket(byte[] packet)
        {
            try
            {
                var msg = main.CreateMessage();
                msg.Write(packet);
                main.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
            }
            catch
            {
                Console.WriteLine("SendPacket error");
            }
        }
        void EmptyCallback(IAsyncResult result)
        {
        }
        DateTime lastpositionsent;
        public void SendSetBlock(Vector3 position, BlockSetMode mode, int type, int materialslot)
        {
            PacketClientSetBlock p = new PacketClientSetBlock()
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Z = (int)position.Z,
                Mode = mode,
                BlockType = type,
                MaterialSlot = materialslot,
            };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.SetBlock, SetBlock = p }));
        }
        public void SendFillArea(Vector3i start, Vector3i end, int blockType)
        {
            PacketClientFillArea p = new PacketClientFillArea()
            {
                X1 = start.x,
                Y1 = start.y,
                Z1 = start.z,
                X2 = end.x,
                Y2 = end.y,
                Z2 = end.z,
                BlockType = blockType,
                MaterialSlot = ActiveMaterial
            };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.FillArea, FillArea = p }));
        }
        public void SendPacketClient(PacketClient packet)
        {
            SendPacket(Serialize(packet));
        }
        public void SendChat(string s)
        {
            PacketClientMessage p = new PacketClientMessage() { Message = s, IsTeamchat = d_HudChat.IsTeamchat };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.Message, Message = p }));
        }
        private void SendPingReply()
        {
            PacketClientPingReply p = new PacketClientPingReply()
            {
            };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.PingReply, PingReply = p }));
        }
        private byte[] Serialize(PacketClient p)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, p);
            return ms.ToArray();
        }
        /// <summary>
        /// This function should be called in program main loop.
        /// It exits immediately.
        /// </summary>
        public void NetworkProcess()
        {
            currentTime = DateTime.UtcNow;
            stopwatch.Reset();
            stopwatch.Start();
            if (main == null)
            {
                return;
            }
            INetIncomingMessage msg;
            while ((msg = main.ReadMessage()) != null)
            {
                TryReadPacket(msg.ReadBytes(msg.LengthBytes));
            }
            if (spawned && ((DateTime.UtcNow - lastpositionsent).TotalSeconds > 0.1))
            {
                lastpositionsent = DateTime.UtcNow;
                SendPosition(LocalPlayerPosition, LocalPlayerOrientation);
            }
            DateTime now = DateTime.UtcNow;
            foreach (var k in players)
            {
                if ((now - k.Value.LastUpdate).TotalSeconds > 2)
                {
                    playerdrawinfo.Remove(k.Key);
                    k.Value.Position = null;
                }
            }
        }
        public int mapreceivedsizex;
        public int mapreceivedsizey;
        public int mapreceivedsizez;
        Vector3 lastsentposition;
        public void SendPosition(Vector3 position, Vector3 orientation)
        {
            PacketClientPositionAndOrientation p = new PacketClientPositionAndOrientation()
            {
                PlayerId = this.LocalPlayerId,//self
                X = (int)((position.X) * 32),
                Y = (int)((position.Y) * 32),
                Z = (int)(position.Z * 32),
                Heading = HeadingByte(orientation),
                Pitch = PitchByte(orientation),
            };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.PositionandOrientation, PositionAndOrientation = p }));
            lastsentposition = position;
        }
        public static Vector3 HeadingPitchToOrientation(byte heading, byte pitch)
        {
            float x = ((float)heading / 256) * 2 * (float)Math.PI;
            float y = (((float)pitch / 256) * 2 * (float)Math.PI) - (float)Math.PI;
            return new Vector3() { X = x, Y = y };
        }
        public static byte HeadingByte(Vector3 orientation)
        {
            return (byte)((((orientation.Y) % (2 * Math.PI)) / (2 * Math.PI)) * 256);
        }
        public static byte PitchByte(Vector3 orientation)
        {
            double xx = (orientation.X + Math.PI) % (2 * Math.PI);
            xx = xx / (2 * Math.PI);
            return (byte)(xx * 256);
        }
        bool spawned = false;

        private int LocalPlayerId = -1;

        Stopwatch stopwatch = new Stopwatch();
        public int maxMiliseconds = 3;
        private void TryReadPacket(byte[] data)
        {
            PacketServer packet = Serializer.Deserialize<PacketServer>(new MemoryStream(data));
            if (Debugger.IsAttached
                && packet.PacketId != ServerPacketId.PositionUpdate
                && packet.PacketId != ServerPacketId.OrientationUpdate
                && packet.PacketId != ServerPacketId.PlayerPositionAndOrientation
                && packet.PacketId != ServerPacketId.ExtendedPacketTick
                && packet.PacketId != ServerPacketId.Chunk
                && packet.PacketId != ServerPacketId.Ping)
            {
                //Console.WriteLine("read packet: " + Enum.GetName(typeof(MinecraftServerPacketId), packet.PacketId));
            }
            switch (packet.PacketId)
            {
                case ServerPacketId.ServerIdentification:
                    {
                        string invalidversionstr = Language.InvalidVersionConnectAnyway;
                        {
                            string servergameversion = packet.Identification.MdProtocolVersion;
                            if (servergameversion != GameVersion.Version)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    System.Windows.Forms.Cursor.Show();
                                    System.Threading.Thread.Sleep(100);
                                    Application.DoEvents();
                                }
                                string q = string.Format(invalidversionstr, GameVersion.Version, servergameversion);
                                var result = System.Windows.Forms.MessageBox.Show(q, "Invalid version", System.Windows.Forms.MessageBoxButtons.OKCancel);
                                if (result == System.Windows.Forms.DialogResult.Cancel)
                                {
                                    throw new Exception(q);
                                }
                                for (int i = 0; i < 10; i++)
                                {
                                    System.Windows.Forms.Cursor.Hide();
                                    System.Threading.Thread.Sleep(100);
                                    Application.DoEvents();
                                }
                            }
                        }
                        this.LocalPlayerId = packet.Identification.AssignedClientId;
                        this.ServerInfo.connectdata = this.connectdata;
                        this.ServerInfo.ServerName = packet.Identification.ServerName;
                        this.ServerInfo.ServerMotd = packet.Identification.ServerMotd;
                        this.d_TerrainChunkTesselator.ENABLE_TEXTURE_TILING = packet.Identification.RenderHint == (int)RenderHint.Fast;
                        ChatLog("---Connected---");
                        List<byte[]> needed = new List<byte[]>();
                        foreach (byte[] b in packet.Identification.UsedBlobsMd5)
                        {
                            if (!IsBlob(b)) { needed.Add(b); }
                        }
                        SendRequestBlob(needed);
                        if (packet.Identification.MapSizeX != d_Map.MapSizeX
                            || packet.Identification.MapSizeY != d_Map.MapSizeY
                            || packet.Identification.MapSizeZ != d_Map.MapSizeZ)
                        {
                            d_ResetMap.Reset(packet.Identification.MapSizeX,
                                packet.Identification.MapSizeY,
                                packet.Identification.MapSizeZ);
                        }
                        serverterraintexture = ByteArrayToString(packet.Identification.TerrainTextureMd5);
                        shadowssimple = packet.Identification.DisableShadows == 1 ? true : false;
                        maxdrawdistance = packet.Identification.PlayerAreaSize / 2;
                        if (maxdrawdistance == 0)
                        {
                            maxdrawdistance = 128;
                        }
                    }
                    break;
                case ServerPacketId.Ping:
                    {
                        this.SendPingReply();
                        this.ServerInfo.ServerPing.Send();
                    }
                    break;
                case ServerPacketId.PlayerPing:
                    {
                        foreach (var k in this.ServerInfo.Players)
                        {
                            if (k.id == packet.PlayerPing.ClientId)
                            {
                                if (k.id == this.LocalPlayerId)
                                {
                                    this.ServerInfo.ServerPing.Receive();
                                }
                                k.ping = packet.PlayerPing.Ping;
                                break;
                            }
                        }
                    }
                    break;
                case ServerPacketId.LevelInitialize:
                    {
                        ReceivedMapLength = 0;
                        InvokeMapLoadingProgress(0, 0, Language.Connecting);
                    }
                    break;
                case ServerPacketId.LevelDataChunk:
                    {
                        MapLoadingPercentComplete = packet.LevelDataChunk.PercentComplete;
                        MapLoadingStatus = packet.LevelDataChunk.Status;
                        InvokeMapLoadingProgress(MapLoadingPercentComplete, (int)ReceivedMapLength, MapLoadingStatus);
                    }
                    break;
                case ServerPacketId.LevelFinalize:
                    {
                        //d_Data.Load(MyStream.ReadAllLines(d_GetFile.GetFile("blocks.csv")),
                        //    MyStream.ReadAllLines(d_GetFile.GetFile("defaultmaterialslots.csv")),
                        //    MyStream.ReadAllLines(d_GetFile.GetFile("lightlevels.csv")));
                        //d_CraftingRecipes.Load(MyStream.ReadAllLines(d_GetFile.GetFile("craftingrecipes.csv")));

                        if (MapLoaded != null)
                        {
                            MapLoaded.Invoke(this, new MapLoadedEventArgs() { });
                        }
                        loadedtime = DateTime.Now;
                    }
                    break;
                case ServerPacketId.SetBlock:
                    {
                        int x = packet.SetBlock.X;
                        int y = packet.SetBlock.Y;
                        int z = packet.SetBlock.Z;
                        int type = packet.SetBlock.BlockType;
                        try { SetTileAndUpdate(new Vector3(x, y, z), type); }
                        catch { Console.WriteLine("Cannot update tile!"); }
                    }
                    break;
                case ServerPacketId.FillArea:
                    {
                        Vector3i a = new Vector3i(packet.FillArea.X1, packet.FillArea.Y1, packet.FillArea.Z1);
                        Vector3i b = new Vector3i(packet.FillArea.X2, packet.FillArea.Y2, packet.FillArea.Z2);

                        int startx = Math.Min(a.x, b.x);
                        int endx = Math.Max(a.x, b.x);
                        int starty = Math.Min(a.y, b.y);
                        int endy = Math.Max(a.y, b.y);
                        int startz = Math.Min(a.z, b.z);
                        int endz = Math.Max(a.z, b.z);

                        int blockCount = packet.FillArea.BlockCount;

                        Jint.Delegates.Action fillArea = delegate
                        {
                            for (int x = startx; x <= endx; ++x)
                            {
                                for (int y = starty; y <= endy; ++y)
                                {
                                    for (int z = startz; z <= endz; ++z)
                                    {
                                        // if creative mode is off and player run out of blocks
                                        if (blockCount == 0)
                                        {
                                            return;
                                        }
                                        try
                                        {
                                            SetTileAndUpdate(new Vector3(x, y, z), packet.FillArea.BlockType);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Cannot update tile!");
                                        }
                                        blockCount--;
                                    }
                                }
                            }
                        };
                        fillArea();
                    }
                    break;
                case ServerPacketId.FillAreaLimit:
                    {
                        this.fillAreaLimit = packet.FillAreaLimit.Limit;
                        if (this.fillAreaLimit > 100000)
                        {
                            this.fillAreaLimit = 100000;
                        }
                    }
                    break;
                case ServerPacketId.Freemove:
                    {
                        this.AllowFreemove = packet.Freemove.IsEnabled;
                        if (!this.AllowFreemove)
                        {
                            ENABLE_FREEMOVE = false;
                            ENABLE_NOCLIP = false;
                            movespeed = basemovespeed;
                            Log(Language.MoveNormal);
                        }
                    }
                    break;
                case ServerPacketId.PlayerSpawnPosition:
                    {
                        int x = packet.PlayerSpawnPosition.X;
                        int y = packet.PlayerSpawnPosition.Y;
                        int z = packet.PlayerSpawnPosition.Z;
                        this.PlayerPositionSpawn = new Vector3(x, z, y);
                        Log(string.Format(Language.SpawnPositionSetTo, x + "," + y + "," + z));
                    }
                    break;
                case ServerPacketId.SpawnPlayer:
                    {
                        int playerid = packet.SpawnPlayer.PlayerId;
                        string playername = packet.SpawnPlayer.PlayerName;
                        bool isnewplayer = true;
                        foreach(ConnectedPlayer p in ServerInfo.Players)
                        {
                            if (p.id == playerid)
                            {
                                isnewplayer = false;
                                p.name = playername;
                            }
                        }
                        if (isnewplayer)
                        {
                            this.ServerInfo.Players.Add(new ConnectedPlayer() { name = playername, id = playerid, ping = -1 });
                        }
                        d_Clients.Players[playerid] = new Player();
                        d_Clients.Players[playerid].Name = playername;
                        d_Clients.Players[playerid].Model = packet.SpawnPlayer.Model;
                        d_Clients.Players[playerid].Texture = packet.SpawnPlayer.Texture;
                        d_Clients.Players[playerid].EyeHeight = packet.SpawnPlayer.EyeHeight;
                        d_Clients.Players[playerid].ModelHeight = packet.SpawnPlayer.ModelHeight;
                        ReadAndUpdatePlayerPosition(packet.SpawnPlayer.PositionAndOrientation, playerid);
                        if (playerid == this.LocalPlayerId)
                        {
                            spawned = true;
                        }
                    }
                    break;
                case ServerPacketId.PlayerPositionAndOrientation:
                    {
                        int playerid = packet.PositionAndOrientation.PlayerId;
                        ReadAndUpdatePlayerPosition(packet.PositionAndOrientation.PositionAndOrientation, playerid);
                    }
                    break;
                case ServerPacketId.Monster:
                    {
                        if (packet.Monster.Monsters == null)
                        {
                            break;
                        }
                        Dictionary<int, int> updatedMonsters = new Dictionary<int, int>();
                        foreach (var k in packet.Monster.Monsters)
                        {
                            int id = k.Id + MonsterIdFirst;
                            if (!players.ContainsKey(id))
                            {
                                players[id] = new Player()
                                {
                                    Name = d_DataMonsters.MonsterName[k.MonsterType],
                                };
                            }
                            ReadAndUpdatePlayerPosition(k.PositionAndOrientation, id);
                            players[id].Type = PlayerType.Monster;
                            players[id].Health = k.Health;
                            players[id].MonsterType = k.MonsterType;
                            updatedMonsters[id] = 1;
                        }
                        //remove all old monsters that were not sent by server now.
                        foreach (int id in new List<int>(players.Keys))
                        {
                            if (id >= MonsterIdFirst)
                            {
                                if (!updatedMonsters.ContainsKey(id))
                                {
                                    players.Remove(id);
                                }
                            }
                        }
                    }
                    break;
                case ServerPacketId.DespawnPlayer:
                    {
                        int playerid = packet.DespawnPlayer.PlayerId;
                        for (int i = 0; i < this.ServerInfo.Players.Count; i++)
                        {
                            if (this.ServerInfo.Players[i].id == playerid)
                            {
                                this.ServerInfo.Players.RemoveAt(i);
                            }
                        }
                        d_Clients.Players.Remove(playerid);
                    }
                    break;
                case ServerPacketId.Message:
                    {
                        AddChatline(packet.Message.Message);
                        ChatLog(packet.Message.Message);
                    }
                    break;
                case ServerPacketId.DisconnectPlayer:
                    {
                        throw new Exception(packet.DisconnectPlayer.DisconnectReason);
                    }
                case ServerPacketId.ChunkPart:
                    BinaryWriter bw1 = new BinaryWriter(CurrentChunk);
                    bw1.Write((byte[])packet.ChunkPart.CompressedChunkPart);
                    break;
                case ServerPacketId.Chunk:
                    {
                        var p = packet.Chunk;
                        if (CurrentChunk.Length != 0)
                        {
                            byte[] decompressedchunk = d_Compression.Decompress(CurrentChunk.ToArray());
                            ushort[, ,] receivedchunk = new ushort[p.SizeX, p.SizeY, p.SizeZ];
                            {
                                BinaryReader br2 = new BinaryReader(new MemoryStream(decompressedchunk));
                                for (int zz = 0; zz < p.SizeZ; zz++)
                                {
                                    for (int yy = 0; yy < p.SizeY; yy++)
                                    {
                                        for (int xx = 0; xx < p.SizeX; xx++)
                                        {
                                            receivedchunk[xx, yy, zz] = br2.ReadUInt16();
                                        }
                                    }
                                }
                            }

                            d_Map.SetMapPortion(p.X, p.Y, p.Z, receivedchunk);
                            for (int xx = 0; xx < 2; xx++)
                            {
                                for (int yy = 0; yy < 2; yy++)
                                {
                                    for (int zz = 0; zz < 2; zz++)
                                    {
                                        d_Shadows.OnSetChunk(p.X + 16 * xx, p.Y + 16 * yy, p.Z + 16 * zz);//todo
                                    }
                                }
                            }
                        }
                        ReceivedMapLength += data.Length;// lengthPrefixLength + packetLength;
                        CurrentChunk = new MemoryStream();
                    }
                    break;
                case ServerPacketId.HeightmapChunk:
                    {
                        var p = packet.HeightmapChunk;
                        byte[] decompressedchunk = d_Compression.Decompress(p.CompressedHeightmap);
                        ushort[] decompressedchunk1 = Misc.ByteArrayToUshortArray(decompressedchunk);
                        for (int xx = 0; xx < p.SizeX; xx++)
                        {
                            for (int yy = 0; yy < p.SizeY; yy++)
                            {
                                int height = decompressedchunk1[MapUtil.Index2d(xx, yy, p.SizeX)];
                                d_Heightmap.SetBlock(p.X + xx, p.Y + yy, height);
                            }
                        }
                    }
                    break;
                case ServerPacketId.PlayerStats:
                    {
                        var p = packet.PlayerStats;
                        this.PlayerStats = p;
                    }
                    break;
                case ServerPacketId.FiniteInventory:
                    {
                        //check for null so it's possible to connect
                        //to old versions of game (before 2011-05-05)
                        if (packet.Inventory.Inventory != null)
                        {
                            d_Inventory.CopyFrom(packet.Inventory.Inventory);
                        }
                        /*
                        FiniteInventory = packet.FiniteInventory.BlockTypeAmount;
                        ENABLE_FINITEINVENTORY = packet.FiniteInventory.IsFinite;
                        FiniteInventoryMax = packet.FiniteInventory.Max;
                        */
                    }
                    break;
                case ServerPacketId.Season:
                    {
                        packet.Season.Hour -= 1;
                        if (packet.Season.Hour < 0)
                        {
                            //shouldn't happen
                            packet.Season.Hour = 12 * HourDetail;
                        }
                        if (NightLevels == null)
                        {
                            string[] l = MyStream.ReadAllLines(d_GetFile.GetFile("sunlevels.csv"));
                            NightLevels = new int[24 * HourDetail];
                            for (int i = 0; i < 24 * HourDetail; i++)
                            {
                                string s = l[i];
                                if (s.Contains(";")) { s = s.Substring(0, s.IndexOf(";")); }
                                if (s.Contains(",")) { s = s.Substring(0, s.IndexOf(",")); }
                                s = s.Trim();
                                NightLevels[i] = int.Parse(s);
                            }
                        }
                        int sunlight = NightLevels[packet.Season.Hour];
                        SkySphereNight = sunlight < 8;
                        d_SunMoonRenderer.day_length_in_seconds = 60 * 60 * 24 / packet.Season.DayNightCycleSpeedup;
                        int hour = packet.Season.Hour / HourDetail;
                        if (d_SunMoonRenderer.Hour != hour)
                        {
                            d_SunMoonRenderer.Hour = hour;
                        }

                        if (d_Shadows.sunlight != sunlight)
                        {
                            d_Shadows.sunlight = sunlight;
                            d_Shadows.ResetShadows();
                            RedrawAllBlocks();
                        }
                    }
                    break;
                case ServerPacketId.BlobInitialize:
                    {
                        blobdownload = new MemoryStream();
                        blobdownloadhash = ByteArrayToString(packet.BlobInitialize.hash);
                        blobdownloadname = packet.BlobInitialize.name;
                        ReceivedMapLength = 0; //todo
                    }
                    break;
                case ServerPacketId.BlobPart:
                    {
                        BinaryWriter bw = new BinaryWriter(blobdownload);
                        bw.Write(packet.BlobPart.data);
                        ReceivedMapLength += packet.BlobPart.data.Length; //todo
                    }
                    break;
                case ServerPacketId.BlobFinalize:
                    {
                        byte[] downloaded = blobdownload.ToArray();
                        /*
                        if (ENABLE_PER_SERVER_TEXTURES || Options.UseServerTextures)
                        {
                            if (blobdownloadhash == serverterraintexture)
                            {
                                using (Bitmap bmp = new Bitmap(new MemoryStream(downloaded)))
                                {
                                    d_TerrainTextures.UseTerrainTextureAtlas2d(bmp);
                                }
                            }
                        }
                        */
                        if (blobdownloadname != null) // old servers
                        {
                            d_GetFile.SetFile(blobdownloadname, downloaded);
                        }
                        blobdownload = null;
                    }
                    break;
                case ServerPacketId.Sound:
                    {
                        PlaySoundAt(packet.Sound.Name, packet.Sound.X, packet.Sound.Y, packet.Sound.Z);
                    }
                    break;
                case ServerPacketId.RemoveMonsters:
                    {
                        foreach (int id in new List<int>(players.Keys))
                        {
                            if (id >= MonsterIdFirst)
                            {
                                players.Remove(id);
                            }
                        }
                    }
                    break;
                case ServerPacketId.BlockType:
                    NewBlockTypes[packet.BlockType.Id] = packet.BlockType.blocktype;
                    break;
                case ServerPacketId.BlockTypes:
                    this.blocktypes = NewBlockTypes;
                    NewBlockTypes = new BlockType[blocktypes.Length];

                    Dictionary<string, int> textureInAtlasIds = new Dictionary<string, int>();
                    int lastTextureId = 0;
                    for (int i = 0; i < blocktypes.Length; i++)
                    {
                        string[] to_load = new string[]
                        {
                            blocktypes[i].TextureIdLeft,
                            blocktypes[i].TextureIdRight,
                            blocktypes[i].TextureIdFront,
                            blocktypes[i].TextureIdBack,
                            blocktypes[i].TextureIdTop,
                            blocktypes[i].TextureIdBottom,
                            blocktypes[i].TextureIdForInventory,
                        };
                        for (int k = 0; k < to_load.Length; k++)
                        {
                            if (!textureInAtlasIds.ContainsKey(to_load[k]))
                            {
                                textureInAtlasIds[to_load[k]] = lastTextureId++;
                            }
                        }
                    }
                    d_Data.UseBlockTypes(blocktypes, textureInAtlasIds);
                    UseTerrainTextures(textureInAtlasIds);
                    d_Weapon.redraw = true;
                    RedrawAllBlocks();
                    break;
                case ServerPacketId.SunLevels:
                    NightLevels = packet.SunLevels.sunlevels;
                    break;
                case ServerPacketId.LightLevels:
                    Array.Copy(packet.LightLevels.lightlevels, d_Data.LightLevels, packet.LightLevels.lightlevels.Length);
                    break;
                case ServerPacketId.CraftingRecipes:
                    d_CraftingRecipes = packet.CraftingRecipes.CraftingRecipes;
                    break;
                case ServerPacketId.Dialog:
                    var d = packet.Dialog;
                    if (d.Dialog == null)
                    {
                        if (dialogs.ContainsKey(d.DialogId) && dialogs[d.DialogId].IsModal)
                        {
                            GuiStateBackToGame();
                        }
                        dialogs.Remove(d.DialogId);
                        if (dialogs.Count == 0)
                        {
                            FreeMouse = false;
                        }
                    }
                    else
                    {
                        dialogs[d.DialogId] = d.Dialog;
                        if (d.Dialog.IsModal)
                        {
                            guistate = GuiState.ModalDialog;
                            FreeMouse = true;
                        }
                    }
                    break;
                case ServerPacketId.Follow:
                    int? oldFollowId = FollowId;
                    Follow = packet.Follow.Client;
                    if (packet.Follow.Tpp)
                    {
                        SetCamera(CameraType.Overhead);
                        player.playerorientation.X = (float)Math.PI;
                        GuiStateBackToGame();
                    }
                    else
                    {
                        SetCamera(CameraType.Fpp);
                    }
                    break;
                case ServerPacketId.Bullet:
                    Bullet bullet = new Bullet();
                    bullet.from = new Vector3(packet.Bullet.FromX, packet.Bullet.FromY, packet.Bullet.FromZ);
                    bullet.to = new Vector3(packet.Bullet.ToX, packet.Bullet.ToY, packet.Bullet.ToZ);
                    bullet.speed = packet.Bullet.Speed;
                    bullets.Add(bullet);
                    break;
                case ServerPacketId.Ammo:
                    if (!ammostarted)
                    {
                        ammostarted = true;
                        foreach (var k in packet.Ammo.TotalAmmo)
                        {
                            LoadedAmmo[k.Key] = Math.Min(k.Value, blocktypes[k.Key].AmmoMagazine);
                        }
                    }
                    TotalAmmo = packet.Ammo.TotalAmmo;
                    break;
                case ServerPacketId.Explosion:
                    explosions.Add(new Explosion() { date = DateTime.UtcNow, explosion = packet.Explosion });
                    break;
                case ServerPacketId.Projectile:
                    Projectile projectile = new Projectile();
                    projectile.position = new Vector3(packet.Projectile.FromX, packet.Projectile.FromY, packet.Projectile.FromZ);
                    projectile.velocity = new Vector3(packet.Projectile.VelocityX, packet.Projectile.VelocityY, packet.Projectile.VelocityZ);
                    projectile.start = DateTime.UtcNow;
                    projectile.block = packet.Projectile.BlockId;
                    projectile.explodesafter = packet.Projectile.ExplodesAfter;
                    projectiles.Add(projectile);
                    break;
                default:
                    break;
            }
            LastReceived = currentTime;
            //return lengthPrefixLength + packetLength;
        }
        public class Explosion
        {
            public DateTime date;
            public PacketServerExplosion explosion;
        }
        List<Explosion> explosions = new List<Explosion>();
        MemoryStream CurrentChunk = new MemoryStream();
        BlockType[] NewBlockTypes = new BlockType[GlobalVar.MAX_BLOCKTYPES];
        public class Bullet
        {
            public Vector3 from;
            public Vector3 to;
            public float speed;
            public float progress;
        }
        List<Bullet> bullets = new List<Bullet>();
        List<Projectile> projectiles = new List<Projectile>();
        string Follow = null;
        int? FollowId
        {
            get
            {
                foreach (var k in Players)
                {
                    if (k.Value.Name.Equals(Follow))
                    {
                        return k.Key;
                    }
                }
                return null;
            }
        }
        private void PlaySoundAt(string name, float x, float y, float z)
        {
            if (x == 0 && y == 0 && z == 0)
            {
                d_Audio.Play(name);
            }
            else
            {
                Vector3 player = LocalPlayerPosition + new Vector3(0, CharacterEyesHeight, 0);
                d_Audio.Play(name, new Vector3(x, z, y));
            }
        }

        Dictionary<string, Dialog> dialogs = new Dictionary<string, Dialog>();
        public GameDataMonsters d_DataMonsters;
        public int MonsterIdFirst = 1000;
        DateTime currentTime;
        private void SendRequestBlob(List<byte[]> needed)
        {
            PacketClientRequestBlob p = new PacketClientRequestBlob() { RequestBlobMd5 = needed };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.RequestBlob, RequestBlob = p }));
        }
        bool IsBlob(byte[] hash)
        {
            return false;
            //return File.Exists(Path.Combine(gamepathblobs, BytesToHex(hash)));
        }
        byte[] GetBlob(byte[] hash)
        {
            return null;
        }
        string BytesToHex(byte[] ba)
        {
            StringBuilder sb = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
        public int ReceivedMapLength = 0;
        DateTime loadedtime;
        private void InvokeMapLoadingProgress(int progressPercent, int progressBytes, string status)
        {
            if (MapLoadingProgress != null)
            {
                MapLoadingProgress(this, new MapLoadingProgressEventArgs()
                {
                    ProgressPercent = progressPercent,
                    ProgressBytes = progressBytes,
                    ProgressStatus = status,
                });
            }
        }
        public bool ENABLE_CHATLOG = true;
        public string gamepathlogs = Path.Combine(GameStorePath.GetStorePath(), "Logs");
        public string gamepathblobs = Path.Combine(GameStorePath.GetStorePath(), "Blobs");
        private void ChatLog(string p)
        {
            if (!ENABLE_CHATLOG)
            {
                return;
            }
            if (!Directory.Exists(gamepathlogs))
            {
                Directory.CreateDirectory(gamepathlogs);
            }
            string filename = Path.Combine(gamepathlogs, MakeValidFileName(this.ServerInfo.ServerName) + ".txt");
            try
            {
                File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
            }
            catch
            {
                Console.WriteLine(Language.CannotWriteChatLog, filename);
            }
        }
        private static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }
        private void UpdatePositionDiff(byte playerid, Vector3 v)
        {
            if (playerid == this.LocalPlayerId)
            {
                LocalPlayerPosition += v;
                spawned = true;
            }
            else
            {
                if (!d_Clients.Players.ContainsKey(playerid))
                {
                    d_Clients.Players[playerid] = new Player();
                    d_Clients.Players[playerid].Name = "invalid";
                    //throw new Exception();
                    InvalidPlayerWarning(playerid);
                }
                d_Clients.Players[playerid].Position += v;
            }
        }
        private static void InvalidPlayerWarning(int playerid)
        {
            Console.WriteLine(string.Format("Position update of nonexistent player {0}.",playerid));
        }
        private void ReadAndUpdatePlayerPosition(PositionAndOrientation positionAndOrientation, int playerid)
        {
            float x = (float)((double)positionAndOrientation.X / 32);
            float y = (float)((double)positionAndOrientation.Y / 32);
            float z = (float)((double)positionAndOrientation.Z / 32);
            byte heading = positionAndOrientation.Heading;
            byte pitch = positionAndOrientation.Pitch;
            Vector3 realpos = new Vector3(x, y, z);
            if (playerid == this.LocalPlayerId)
            {
                if (!enablePlayerUpdatePosition.ContainsKey(playerid) || enablePlayerUpdatePosition[playerid])
                {
                    LocalPlayerPosition = realpos;
                }
                spawned = true;
            }
            else
            {
                if (!d_Clients.Players.ContainsKey(playerid))
                {
                    d_Clients.Players[playerid] = new Player();
                    d_Clients.Players[playerid].Name = "invalid";
                    InvalidPlayerWarning(playerid);
                }
                if (!enablePlayerUpdatePosition.ContainsKey(playerid) || enablePlayerUpdatePosition[playerid])
                {
                    d_Clients.Players[playerid].Position = realpos;
                }
                d_Clients.Players[playerid].Heading = heading;
                d_Clients.Players[playerid].Pitch = pitch;
                d_Clients.Players[playerid].LastUpdate = DateTime.UtcNow;
            }
        }
        List<byte> received = new List<byte>();
        public void Dispose()
        {
            if (main != null)
            {
                //main.Disconnect(false);
                main = null;
            }
        }
        int MapLoadingPercentComplete;
        string MapLoadingStatus;

        #region IClientNetwork Members
        public event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        #endregion
        Dictionary<int, bool> enablePlayerUpdatePosition = new Dictionary<int, bool>();
        #region INetworkClient Members
        public Dictionary<int, bool> EnablePlayerUpdatePosition { get { return enablePlayerUpdatePosition; } set { enablePlayerUpdatePosition = value; } }
        #endregion
        public DateTime LastReceived { get; set; }
        //[Inject]
        //public IIsChunkDirty d_IsChunkReady;
        public Chunk[] chunks;
        #region IMapStorage Members
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        public unsafe int GetBlock(int x, int y, int z)
        {
            if (!MapUtil.IsValidPos(d_Map, x, y, z))
            {
                return 0;
            }

            int cx = x / chunksize;
            int cy = y / chunksize;
            int cz = z / chunksize;
            int chunkpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
            if (chunks[chunkpos] == null)
            {
                return 0;
            }
            else
            {
                return chunks[chunkpos].data[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)];
            }
        }

        public unsafe void SetBlock(int x, int y, int z, int tileType)
        {
            ushort[] chunk = GetChunk(x, y, z);
            chunk[MapUtil.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (ushort)tileType;
            SetChunkDirty(x / chunksize, y / chunksize, z / chunksize, true);
            d_Shadows.OnSetBlock(x, y, z);
            ShadowsOnSetBlock(x, y, z);
            lastplacedblock = new Vector3i(x, y, z);
        }
        Vector3i? lastplacedblock=null;
        public void ShadowsOnSetBlock(int x, int y, int z)
        {
            int oldheight = d_Heightmap.GetBlock(x, y);
            UpdateColumnHeight(x, y);
            //update shadows in all chunks below
            int newheight = d_Heightmap.GetBlock(x, y);
            int min = Math.Min(oldheight, newheight);
            int max = Math.Max(oldheight, newheight);
            for (int i = min; i < max; i++)
            {
                if (i / chunksize != z / chunksize)
                {
                    SetChunkDirty(x / chunksize, y / chunksize, i / chunksize, true);
                }
            }
            //Todo: too many redraws. Optimize.
            //Now placing a single block updates 27 chunks,
            //and each of those chunk updates calculates light from 27 chunks.
            //So placing a block is often 729x slower than it should be.
            for (int xx = 0; xx < 3; xx++)
            {
                for (int yy = 0; yy < 3; yy++)
                {
                    for (int zz = 0; zz < 3; zz++)
                    {
                        int cx = x / chunksize + xx - 1;
                        int cy = y / chunksize + yy - 1;
                        int cz = z / chunksize + zz - 1;
                        if (MapUtil.IsValidChunkPos(this, cx, cy, cz, chunksize))
                        {
                            SetChunkDirty(cx, cy, cz, true);
                        }
                    }
                }
            }
        }

        private void UpdateColumnHeight(int x, int y)
        {
            //todo faster
            int height = d_Map.MapSizeZ - 1;
            for (int i = d_Map.MapSizeZ - 1; i >= 0; i--)
            {
                height = i;
                if (!d_Data.IsTransparentForLight[d_Map.GetBlock(x, y, i)])
                {
                    break;
                }
            }
            d_Heightmap.SetBlock(x, y, height);
        }


        #endregion
        public ushort[] GetChunk(int x, int y, int z)
        {
            x = x / chunksize;
            y = y / chunksize;
            z = z / chunksize;
            int mapsizexchunks = MapSizeX / chunksize;
            int mapsizeychunks = MapSizeY / chunksize;
            Chunk chunk = chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
            if (chunk == null)
            {
                //byte[, ,] newchunk = new byte[chunksize, chunksize, chunksize];
                //byte[, ,] newchunk = generator.GetChunk(x, y, z, chunksize);
                //if (newchunk != null)
                //{
                //    chunks[x, y, z] = new Chunk() { data = MapUtil.ToFlatMap(newchunk) };
                //}
                //else
                {
                    chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)] = new Chunk() { data = new ushort[chunksize * chunksize * chunksize] };
                }
                return chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)].data;
            }
            return chunk.data;
        }
        public int chunksize = 16;
        public void Reset(int sizex, int sizey, int sizez)
        {
            MapSizeX = sizex;
            MapSizeY = sizey;
            MapSizeZ = sizez;
            chunks = new Chunk[(sizex / chunksize) * (sizey / chunksize) * (sizez / chunksize)];
            SetAllChunksNotDirty();
        }
        #region IMapStorage Members
        public unsafe void SetMapPortion(int x, int y, int z, ushort[, ,] chunk)
        {
            int chunksizex = chunk.GetUpperBound(0) + 1;
            int chunksizey = chunk.GetUpperBound(1) + 1;
            int chunksizez = chunk.GetUpperBound(2) + 1;
            if (chunksizex % chunksize != 0) { throw new ArgumentException(); }
            if (chunksizey % chunksize != 0) { throw new ArgumentException(); }
            if (chunksizez % chunksize != 0) { throw new ArgumentException(); }
            ushort[, ,][] localchunks = new ushort[chunksizex / chunksize, chunksizey / chunksize, chunksizez / chunksize][];
            for (int cx = 0; cx < chunksizex / chunksize; cx++)
            {
                for (int cy = 0; cy < chunksizey / chunksize; cy++)
                {
                    for (int cz = 0; cz < chunksizex / chunksize; cz++)
                    {
                        localchunks[cx, cy, cz] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
                        FillChunk(localchunks[cx, cy, cz], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk);
                    }
                }
            }
            for (int xxx = 0; xxx < chunksizex; xxx += chunksize)
            {
                for (int yyy = 0; yyy < chunksizex; yyy += chunksize)
                {
                    for (int zzz = 0; zzz < chunksizex; zzz += chunksize)
                    {
                        SetChunkDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize, true);
                        SetChunksAroundDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize);
                    }
                }
            }
        }
        private void SetChunksAroundDirty(int cx, int cy, int cz)
        {
            if (IsValidChunkPosition(cx, cy, cz)) { SetChunkDirty(cx - 1, cy, cz, true); }
            if (IsValidChunkPosition(cx - 1, cy, cz)) { SetChunkDirty(cx - 1, cy, cz, true); }
            if (IsValidChunkPosition(cx + 1, cy, cz)) { SetChunkDirty(cx + 1, cy, cz, true); }
            if (IsValidChunkPosition(cx, cy - 1, cz)) { SetChunkDirty(cx, cy - 1, cz, true); }
            if (IsValidChunkPosition(cx, cy + 1, cz)) { SetChunkDirty(cx, cy + 1, cz, true); }
            if (IsValidChunkPosition(cx, cy, cz - 1)) { SetChunkDirty(cx, cy, cz - 1, true); }
            if (IsValidChunkPosition(cx, cy, cz + 1)) { SetChunkDirty(cx, cy, cz + 1, true); }
        }
        private bool IsValidChunkPosition(int xx, int yy, int zz)
        {
            return xx >= 0 && yy >= 0 && zz >= 0
                && xx < MapSizeX / chunksize
                && yy < MapSizeY / chunksize
                && zz < MapSizeZ / chunksize;
        }
        private unsafe void FillChunk(ushort[] destination, int destinationchunksize,
            int sourcex, int sourcey, int sourcez, ushort[, ,] source)
        {
            for (int x = 0; x < destinationchunksize; x++)
            {
                for (int y = 0; y < destinationchunksize; y++)
                {
                    for (int z = 0; z < destinationchunksize; z++)
                    {
                        //if (x + sourcex < source.GetUpperBound(0) + 1
                        //    && y + sourcey < source.GetUpperBound(1) + 1
                        //    && z + sourcez < source.GetUpperBound(2) + 1)
                        {
                            destination[MapUtil.Index3d(x, y, z, destinationchunksize, destinationchunksize)]
                                = source[x + sourcex, y + sourcey, z + sourcez];
                        }
                    }
                }
            }
        }
        #endregion
        #region IIsChunkReady Members
        public bool IsChunkReady(int x, int y, int z)
        {
            return IsChunkDirty(x, y, z);
        }
        #endregion
        #region IIsChunkReady Members
        public bool IsChunkDirty(int x, int y, int z)
        {
            //return d_IsChunkReady.IsChunkDirty(x, y, z);
            return true;
        }
        //public void SetChunkDirty(int x, int y, int z, bool dirty)
        //{
        //d_IsChunkReady.SetChunkDirty(x, y, z, dirty);
        //}
        #endregion
        public void SetAllChunksNotDirty()
        {
            //d_IsChunkReady.SetAllChunksNotDirty();
        }
        public unsafe void GetMapPortion(ushort[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
        {
            Array.Clear(outPortion, 0, outPortion.Length);

            int chunksizebits = (int)Math.Log(chunksize, 2);
            int mapchunksx = MapSizeX / chunksize;
            int mapchunksy = MapSizeY / chunksize;
            int mapchunksz = MapSizeZ / chunksize;
            int mapsizechunks = mapchunksx * mapchunksy * mapchunksz;

            for (int xx = 0; xx < portionsizex; xx++)
            {
                for (int yy = 0; yy < portionsizey; yy++)
                {
                    for (int zz = 0; zz < portionsizez; zz++)
                    {
                        //Find chunk.
                        int cx = (x + xx) >> chunksizebits;
                        int cy = (y + yy) >> chunksizebits;
                        int cz = (z + zz) >> chunksizebits;
                        //int cpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
                        int cpos = (cz * mapchunksy + cy) * mapchunksx + cx;
                        //if (cpos < 0 || cpos >= ((MapSizeX / chunksize) * (MapSizeY / chunksize) * (MapSizeZ / chunksize)))
                        if (cpos < 0 || cpos >= mapsizechunks)
                        {
                            continue;
                        }
                        Chunk chunk = chunks[cpos];
                        if (chunk == null || chunk.data == null)
                        {
                            continue;
                        }
                        //int pos = MapUtil.Index3d((x + xx) % chunksize, (y + yy) % chunksize, (z + zz) % chunksize, chunksize, chunksize);
                        int chunkGlobalX = cx << chunksizebits;
                        int chunkGlobalY = cy << chunksizebits;
                        int chunkGlobalZ = cz << chunksizebits;

                        int inChunkX = (x + xx) - chunkGlobalX;
                        int inChunkY = (y + yy) - chunkGlobalY;
                        int inChunkZ = (z + zz) - chunkGlobalZ;

                        //int pos = MapUtil.Index3d(inChunkX, inChunkY, inChunkZ, chunksize, chunksize);
                        int pos = (((inChunkZ << chunksizebits) + inChunkY) << chunksizebits) + inChunkX;

                        int block = chunk.data[pos];
                        //outPortion[MapUtil.Index3d(xx, yy, zz, portionsizex, portionsizey)] = (byte)block;
                        outPortion[(zz * portionsizey + yy) * portionsizex + xx] = (ushort)block;
                    }
                }
            }
        }
        public static int DistanceSquared(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int dx = x1 - x2;
            int dy = y1 - y2;
            int dz = z1 - z2;
            return dx * dx + dy * dy + dz * dz;
        }

        [Inject]
        public TextureAtlasConverter d_TextureAtlasConverter;
        public int texturesPacked { get { return GlobalVar.MAX_BLOCKTYPES_SQRT; } } //16x16
        public int terrainTexture { get; set; }
        public void StartTerrainTextures()
        {
            GL.Enable(EnableCap.Texture2D);
            /*
            using (var atlas2d = new Bitmap(d_GetFile.GetFile("terrain.png")))
            {
                UseTerrainTextureAtlas2d(atlas2d);
            }
            */
        }
        public void UseTerrainTextures(Dictionary<string, int> textureIds)
        {
            //todo bigger than 32x32
            int tilesize = 32;
            Bitmap atlas2d = new Bitmap(tilesize * atlas2dtiles, tilesize * atlas2dtiles);
            IFastBitmap atlas2dFast;
            if (IsMono) { atlas2dFast = new FastBitmapDummy(); } else { atlas2dFast = new FastBitmap(); }
            atlas2dFast.bmp = atlas2d;
            atlas2dFast.Lock();
            foreach (var k in textureIds)
            {
                using (Bitmap bmp = new Bitmap(d_GetFile.GetFile(k.Key + ".png")))
                {
                    IFastBitmap bmpFast;
                    if (IsMono) { bmpFast = new FastBitmapDummy(); } else { bmpFast = new FastBitmap(); }
                    bmpFast.bmp = bmp;
                    bmpFast.Lock();
                    int x = k.Value % texturesPacked;
                    int y = k.Value / texturesPacked;
                    for (int xx = 0; xx < tilesize; xx++)
                    {
                        for (int yy = 0; yy < tilesize; yy++)
                        {
                            int c = bmpFast.GetPixel(xx, yy);
                            atlas2dFast.SetPixel(x * tilesize + xx, y * tilesize + yy, c);
                        }
                    }
                    bmpFast.Unlock();
                }
            }
            atlas2dFast.Unlock();
            UseTerrainTextureAtlas2d(atlas2d);
        }
        public void UseTerrainTextureAtlas2d(Bitmap atlas2d)
        {
            terrainTexture = d_The3d.LoadTexture(atlas2d);
            List<int> terrainTextures1d = new List<int>();
            {
                terrainTexturesPerAtlas = atlas1dheight / (atlas2d.Width / atlas2dtiles);
                List<Bitmap> atlases1d = d_TextureAtlasConverter.Atlas2dInto1d(atlas2d, atlas2dtiles, atlas1dheight);
                foreach (Bitmap bmp in atlases1d)
                {
                    terrainTextures1d.Add(d_The3d.LoadTexture(bmp));
                    bmp.Dispose();
                }
            }
            this.terrainTextures1d = terrainTextures1d.ToArray();
        }
        int maxTextureSize; // detected at runtime
        public int atlas1dheight { get { return maxTextureSize; } }
        public int atlas2dtiles = GlobalVar.MAX_BLOCKTYPES_SQRT; // 16x16
        public int[] terrainTextures1d { get; set; }
        public int terrainTexturesPerAtlas { get; set; }
    }
    [StructLayout(LayoutKind.Sequential)]
    public class Chunk
    {
        public ushort[] data;
        public int LastUpdate;
        public bool IsPopulated;
        public int LastChange;
    }
    public interface IResetMap
    {
        void Reset(int sizex, int sizey, int sizez);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using ManicDigger.Renderers;
using OpenTK;
using System.Drawing;
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
using ManicDigger.ClientNative;

namespace ManicDigger
{
    //This is the main game class.
    public partial class ManicDiggerGameWindow : IMyGameWindow,
        IMapStorage,
        ICurrentShadows
    {
        public ManicDiggerGameWindow()
        {
            one = 1;
            game = new Game();
            game.language.platform = new GamePlatformNative();
            game.language.LoadTranslations();
            mvMatrix.Push(Mat4.Create());
            pMatrix.Push(Mat4.Create());
            PerformanceInfo = new DictionaryStringString();
            AudioEnabled = true;
            OverheadCamera_cameraEye = new Vector3Ref();
            playerPositionSpawnX = 15 + one / 2;
            playerPositionSpawnY = 64;
            playerPositionSpawnZ = 15 + one / 2;
        }
        float one;
        public void Start()
        {
            game.platform = new GamePlatformNative() { window = d_GlWindow };
            string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
            var getfile = new GetFileStream(datapaths);
            var w = this;
            var gamedata = new GameData();
            gamedata.Start();
            var clientgame = w;
            var network = w;
            var mapstorage = clientgame;
            var config3d = new Config3d();
            var localplayerposition = w;
            var physics = new CharacterPhysicsCi();
            var internetgamefactory = this;
            //network.d_ResetMap = this;
            var terrainTextures = new ITerrainTextures();
            terrainTextures.game = game;
            bool IsMono = Type.GetType("Mono.Runtime") != null;
            d_TextureAtlasConverter = new TextureAtlasConverter();
            w.game.d_TerrainTextures = terrainTextures;
            var blockrenderertorch = new BlockRendererTorch();
            blockrenderertorch.d_TerainRenderer = terrainTextures;
            blockrenderertorch.d_Data = gamedata;
            //InfiniteMapChunked map = new InfiniteMapChunked();// { generator = new WorldGeneratorDummy() };
            var map = w;
            var terrainchunktesselator = new TerrainChunkTesselatorCi();
            w.d_TerrainChunkTesselator = terrainchunktesselator;
            var frustumculling = new FrustumCulling() { d_GetCameraMatrix = game.CameraMatrix, platform = game.platform };
            w.d_Batcher = new MeshBatcher() { d_FrustumCulling = frustumculling, game = game };
            w.d_FrustumCulling = frustumculling;
            //w.d_Map = clientgame.mapforphysics;
            w.d_Physics = physics;
            w.d_Data = gamedata;
            w.d_DataMonsters = new GameDataMonsters();
            w.d_GetFile = getfile;
            w.d_Config3d = config3d;
            w.PICK_DISTANCE = 4.5f;
            var skysphere = new SkySphere();
            skysphere.game = game;
            skysphere.d_MeshBatcher = new MeshBatcher() { d_FrustumCulling = new FrustumCullingDummy(), game = game };
            w.skysphere = skysphere;
            Packet_Inventory inventory = new Packet_Inventory();
            w.d_Weapon = new WeaponRenderer() { d_BlockRendererTorch = blockrenderertorch, game = game };
            var playerrenderer = new CharacterRendererMonsterCode();
            playerrenderer.game = this.game;
            var particle = new ParticleEffectBlockBreak();
            w.particleEffectBlockBreak = particle;
            w.d_Shadows = w;
            clientgame.d_Data = gamedata;
            clientgame.d_CraftingTableTool = new CraftingTableTool() { d_Map = mapstorage, d_Data = gamedata };
            clientgame.d_RailMapUtil = new RailMapUtil() { game = game };
            clientgame.d_MinecartRenderer = new MinecartRenderer() { game = game };
            clientgame.game.d_TerrainTextures = terrainTextures;
            clientgame.d_GetFile = getfile;
            w.Reset(10 * 1000, 10 * 1000, 128);
            clientgame.d_Map = game;
            if (!issingleplayer)
            {
                try
                {
                    if (skinserver == null)
                    {
                        WebClient c = new WebClient();
                        skinserver = c.DownloadString("http://manicdigger.sourceforge.net/skinserver.txt");
                    }
                }
                catch
                {
                    skinserver = "";
                }
            }
            w.d_FrustumCulling = frustumculling;
            //w.d_CurrentShadows = this;
            var sunmoonrenderer = new SunMoonRenderer() { game = game };
            w.d_SunMoonRenderer = sunmoonrenderer;
            clientgame.d_SunMoonRenderer = sunmoonrenderer;
            this.d_Heightmap = new InfiniteMapChunked2d() { d_Map = game };
            d_Heightmap.Restart();
            network.d_Heightmap = d_Heightmap;
            //this.light = new InfiniteMapChunkedSimple() { d_Map = map };
            //light.Restart();
            w.d_TerrainChunkTesselator = terrainchunktesselator;
            terrainchunktesselator.game = game;
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
            terrainRenderer = new TerrainRenderer();
            terrainRenderer.game = game;
            w.d_HudChat = new HudChat() { game = this.game };
            var dataItems = new GameDataItemsClient() { game = game };
            var inventoryController = ClientInventoryController.Create(game);
            var inventoryUtil = new InventoryUtilClient();
            var hudInventory = new HudInventory();
            hudInventory.game = game;
            hudInventory.dataItems = dataItems;
            hudInventory.inventoryUtil = inventoryUtil;
            hudInventory.controller = inventoryController;
            w.d_Inventory = inventory;
            w.d_InventoryController = inventoryController;
            w.d_InventoryUtil = inventoryUtil;
            inventoryUtil.d_Inventory = inventory;
            inventoryUtil.d_Items = dataItems;

            d_Physics.game = game;
            clientgame.d_Inventory = inventory;
            w.d_HudInventory = hudInventory;
            w.d_CurrentShadows = this;
            crashreporter.OnCrash += new EventHandler(crashreporter_OnCrash);

            clientmods = new ClientMod[128];
            clientmodsCount = 0;
            modmanager.game = game;
            AddMod(new ModAutoCamera());
            AddMod(new ModFpsHistoryGraph());
            applicationname = language.GameName();
            s = new BlockOctreeSearcher();
            s.platform = game.platform;
            escapeMenu.game = this;
        }
        void AddMod(ClientMod mod)
        {
            clientmods[clientmodsCount++] = mod;
            mod.Start(modmanager);
        }

        internal Game game;

        void crashreporter_OnCrash(object sender, EventArgs e)
        {
            try
            {
                SendLeave(Packet_LeaveReasonEnum.Crash);
            }
            catch
            {
            }
        }

        public GlWindow d_GlWindow;
        public Game d_Map;

        public GetFileStream d_GetFile;
        public ICurrentShadows d_CurrentShadows;
        public IGameExit d_Exit;
        public IInventoryController d_InventoryController;
        public CraftingTableTool d_CraftingTableTool;
        public ManicDiggerGameWindow d_Shadows;

        public bool IsMono = Type.GetType("Mono.Runtime") != null;
        public bool IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;

        public GuiStateEscapeMenu escapeMenu = new GuiStateEscapeMenu();
        public void OnFocusedChanged(EventArgs e)
        {
            if (guistate == GuiState.Normal)
            { escapeMenu.EscapeMenuStart(); }
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
        public List<DisplayResolution> resolutions;
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
            int maxTextureSize_;
            try
            {
                GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize_);
            }
            catch
            {
                maxTextureSize_ = 1024;
            }
            if (maxTextureSize < 1024)
            {
                maxTextureSize_ = 1024;
            }
            maxTextureSize = maxTextureSize_;
            //Start();
            //Connect();
            MapLoadingStart();

            if (!d_Config3d.ENABLE_VSYNC)
            {
                d_GlWindow.TargetRenderFrequency = 0;
            }
            GL.ClearColor(Color.Black);
            d_GlWindow.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            d_GlWindow.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
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
                d_HudInventory.Mouse_ButtonUp(GetMouseEventArgs(e));
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
                d_HudInventory.Mouse_ButtonDown(GetMouseEventArgs(e));
            }
            InvalidVersionAllow();
        }

        MouseEventArgs GetMouseEventArgs(OpenTK.Input.MouseButtonEventArgs e)
        {
            MouseEventArgs args = new MouseEventArgs();
            args.SetX(e.X);
            args.SetY(e.Y);
            int button = 0;
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                button = MouseButtonEnum.Left;
            }
            if (e.Button == OpenTK.Input.MouseButton.Middle)
            {
                button = MouseButtonEnum.Middle;
            }
            if (e.Button == OpenTK.Input.MouseButton.Right)
            {
                button = MouseButtonEnum.Right;
            }
            args.SetButton(button);
            return args;
        }

        void ManicDiggerGameWindow_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            game.KeyPress((int)e.KeyChar);
        }

        void Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            game.MouseWheelChanged(e.DeltaPrecise);
        }
        public void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SendLeave(Packet_LeaveReasonEnum.Leave);
        }
        public void OnClosed(EventArgs e)
        {
            d_Exit.exit = true;
            //..base.OnClosed(e);
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            int eKey = (int)e.Key;
            KeyUp(eKey);
        }
        
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            int eKey = (int)e.Key;
            KeyDown(eKey);
        }
        void KeyDown(int eKey)
        {
            keyboardState[eKey] = true;
            for (int i = 0; i < clientmodsCount; i++)
            {
                KeyEventArgs args_ = new KeyEventArgs();
                args_.SetKeyCode(eKey);
                clientmods[i].OnKeyDown(args_);
            }
            InvalidVersionAllow();
            if (eKey == GetKey(GlKeys.F6))
            {
                float lagSeconds = one * (game.platform.TimeMillisecondsFromStart() - LastReceivedMilliseconds) / 1000;
                if ((lagSeconds >= DISCONNECTED_ICON_AFTER_SECONDS) || guistate == GuiState.MapLoading)
                {
                    Reconnect();
                }
            }
            if (eKey == GetKey(GlKeys.ShiftLeft) || eKey == GetKey(GlKeys.ShiftRight))
            {
                IsShiftPressed = true;
            }
            if (eKey == GetKey(GlKeys.F11))
            {
                if (d_GlWindow.WindowState == WindowState.Fullscreen)
                {
                    d_GlWindow.WindowState = WindowState.Normal;
                    escapeMenu.RestoreResolution();
                    escapeMenu.SaveOptions();
                }
                else
                {
                    d_GlWindow.WindowState = WindowState.Fullscreen;
                    escapeMenu.UseResolution();
                    escapeMenu.SaveOptions();
                }
            }
            if (eKey == (GetKey(GlKeys.C)) && GuiTyping == TypingState.None)
            {
                if (!(SelectedBlockPositionX == -1 && SelectedBlockPositionY == -1 && SelectedBlockPositionZ == -1))
                {
                    int posx = SelectedBlockPositionX;
                    int posy = SelectedBlockPositionZ;
                    int posz = SelectedBlockPositionY;
                    if (d_Map.GetBlock(posx, posy, posz) == d_Data.BlockIdCraftingTable())
                    {
                        //draw crafting recipes list.
                        CraftingRecipesStart(d_CraftingRecipes, d_CraftingTableTool.GetOnTable(d_CraftingTableTool.GetTable(posx, posy, posz)),
                        (recipe) => { CraftingRecipeSelected(posx, posy, posz, recipe); });
                    }
                }
            }
            if (guistate == GuiState.Normal)
            {
                if (keyboardState[GetKey(GlKeys.Escape)])
                {
                    for (int i = 0; i < dialogsCount; i++)
                    {
                        if (dialogs[i] == null)
                        {
                            continue;
                        }
                        VisibleDialog d = dialogs[i];
                        if (d.value.IsModal != 0)
                        {
                            dialogs[i] = null;
                            return;
                        }
                    }
                    guistate = GuiState.EscapeMenu;
                    menustate = new MenuState();
                    FreeMouse = true;
                }
                if (eKey == GetKey(GlKeys.Number7) && IsShiftPressed && GuiTyping == TypingState.None) // don't need to hit enter for typing commands starting with slash
                {
                    GuiTyping = TypingState.Typing;
                    d_HudChat.IsTyping = true;
                    d_HudChat.GuiTypingBuffer = "";
                    d_HudChat.IsTeamchat = false;
                    return;
                }
                if (eKey == GetKey(GlKeys.PageUp) && GuiTyping == TypingState.Typing)
                {
                    d_HudChat.ChatPageScroll++;
                }
                if (eKey == GetKey(GlKeys.PageDown) && GuiTyping == TypingState.Typing)
                {
                    d_HudChat.ChatPageScroll--;
                }
                d_HudChat.ChatPageScroll = Game.ClampInt(d_HudChat.ChatPageScroll, 0, d_HudChat.ChatLinesCount / d_HudChat.ChatLinesMaxToDraw);
                if (eKey == GetKey(GlKeys.Enter) || eKey == GetKey(GlKeys.KeypadEnter))
                {
                    if (GuiTyping == TypingState.Typing)
                    {
                        typinglog[typinglogCount++] = d_HudChat.GuiTypingBuffer;
                        typinglogpos = typinglogCount;
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
                    int key = eKey;
                    if (key == GetKey(GlKeys.BackSpace))
                    {
                        if (d_HudChat.GuiTypingBuffer.Length > 0)
                        {
                            d_HudChat.GuiTypingBuffer = d_HudChat.GuiTypingBuffer.Substring(0, d_HudChat.GuiTypingBuffer.Length - 1);
                        }
                        return;
                    }
                    if (keyboardState[GetKey(GlKeys.ControlLeft)] || keyboardState[GetKey(GlKeys.ControlRight)])
                    {
                        if (key == GetKey(GlKeys.V))
                        {
                            if (Clipboard.ContainsText())
                            {
                                d_HudChat.GuiTypingBuffer += Clipboard.GetText();
                            }
                            return;
                        }
                    }
                    if (key == GetKey(GlKeys.Up))
                    {
                        typinglogpos--;
                        if (typinglogpos < 0) { typinglogpos = 0; }
                        if (typinglogpos >= 0 && typinglogpos < typinglogCount)
                        {
                            d_HudChat.GuiTypingBuffer = typinglog[typinglogpos];
                        }
                    }
                    if (key == GetKey(GlKeys.Down))
                    {
                        typinglogpos++;
                        if (typinglogpos > typinglogCount) { typinglogpos = typinglogCount; }
                        if (typinglogpos >= 0 && typinglogpos < typinglogCount)
                        {
                            d_HudChat.GuiTypingBuffer = typinglog[typinglogpos];
                        }
                        if (typinglogpos == typinglogCount)
                        {
                            d_HudChat.GuiTypingBuffer = "";
                        }
                    }
                    return;
                }

                string strFreemoveNotAllowed = "You are not allowed to enable freemove.";

                if (eKey == GetKey(GlKeys.F1))
                {
                    if (!this.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    movespeed = basemovespeed * 1;
                    Log("Move speed: 1x.");
                }
                if (eKey == GetKey(GlKeys.F2))
                {
                    if (!this.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    movespeed = basemovespeed * 10;
                    Log(string.Format(language.MoveSpeed(), 10.ToString()));
                }
                if (eKey == GetKey(GlKeys.F3))
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
                        Log(language.MoveFree());
                    }
                    else if (ENABLE_FREEMOVE && (!ENABLE_NOCLIP))
                    {
                        ENABLE_NOCLIP = true;
                        Log(language.MoveFreeNoclip());
                    }
                    else if (ENABLE_FREEMOVE && ENABLE_NOCLIP)
                    {
                        ENABLE_FREEMOVE = false;
                        ENABLE_NOCLIP = false;
                        Log(language.MoveNormal());
                    }
                }
                if (eKey == GetKey(GlKeys.I))
                {
                    drawblockinfo = !drawblockinfo;
                }
                PerformanceInfo.Set("height", "height:" + d_Heightmap.GetBlock(platform.FloatToInt(player.playerposition.X), platform.FloatToInt(player.playerposition.Z)));
                if (eKey == GetKey(GlKeys.F5))
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
                        playerdestination = Vector3Ref.Create(player.playerposition.X, player.playerposition.Y, player.playerposition.Z);
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
                if (eKey == GetKey(GlKeys.Plus) || eKey == GetKey(GlKeys.KeypadPlus))
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
                if (eKey == GetKey(GlKeys.Minus) || eKey == GetKey(GlKeys.KeypadMinus))
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

                if (eKey == GetKey(GlKeys.F6))
                {
                    RedrawAllBlocks();
                }
                if (eKey == GlKeys.F8)
                {
                    ToggleVsync();
                    if (ENABLE_LAG == 0) { Log(language.FrameRateVsync()); }
                    if (ENABLE_LAG == 1) { Log(language.FrameRateUnlimited()); }
                    if (ENABLE_LAG == 2) { Log(language.FrameRateLagSimulation()); }
                }
                if (eKey == GetKey(GlKeys.F12))
                {
                    game.platform.SaveScreenshot();
                    screenshotflash = 5;
                }
                if (eKey == GetKey(GlKeys.Tab))
                {
                    Packet_Client p = new Packet_Client();
                    p.Id = Packet_ClientIdEnum.SpecialKey;
                    p.SpecialKey_ = new Packet_ClientSpecialKey();
                    p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.TabPlayerList;
                    SendPacketClient(p);
                }
                if (eKey == GetKey(GlKeys.E))
                {
                    if (currentAttackedBlock != null)
                    {
                        int posX = currentAttackedBlock.X;
                        int posY = currentAttackedBlock.Y;
                        int posZ = currentAttackedBlock.Z;
                        int blocktype = d_Map.GetBlock(currentAttackedBlock.X, currentAttackedBlock.Y, currentAttackedBlock.Z);
                        if (IsUsableBlock(blocktype))
                        {
                            if (d_Data.IsRailTile(blocktype))
                            {
                                player.playerposition.X = posX + (one / 2);
                                player.playerposition.Y = posZ + 1;
                                player.playerposition.Z = posY + (one / 2);
                                ENABLE_FREEMOVE = false;
                            }
                            else
                            {
                                SendSetBlock(posX, posY, posZ, Packet_BlockSetModeEnum.Use, 0, ActiveMaterial);
                            }
                        }
                    }
                }
                if (eKey == GetKey(GlKeys.R))
                {
                    Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
                    if (item != null && item.ItemClass == Packet_ItemClassEnum.Block
                        && game.blocktypes[item.BlockId].IsPistol
                        && reloadstartMilliseconds == 0)
                    {
                        int sound = rnd.Next(game.blocktypes[item.BlockId].Sounds.Reload.Length);
                        AudioPlay(game.blocktypes[item.BlockId].Sounds.Reload[sound] + ".ogg");
                        reloadstartMilliseconds = game.platform.TimeMillisecondsFromStart();
                        reloadblock = item.BlockId;
                        Packet_Client p = new Packet_Client();
                        p.Id = Packet_ClientIdEnum.Reload;
                        p.Reload = new Packet_ClientReload();
                        SendPacketClient(p);
                    }
                }
                if (eKey == GetKey(GlKeys.O))
                {
                    Respawn();
                }
                if (eKey == GetKey(GlKeys.L))
                {
                    Packet_Client p = new Packet_Client();
                    {
                        p.Id = Packet_ClientIdEnum.SpecialKey;
                        p.SpecialKey_ = new Packet_ClientSpecialKey();
                        p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.SelectTeam;
                    }
                    SendPacketClient(p);
                }
                if (eKey == GetKey(GlKeys.P))
                {
                    Packet_Client p = new Packet_Client();
                    {
                        p.Id = Packet_ClientIdEnum.SpecialKey;
                        p.SpecialKey_ = new Packet_ClientSpecialKey();
                        p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.SetSpawn;
                    }
                    SendPacketClient(p);

                    playerPositionSpawnX = player.playerposition.X;
                    playerPositionSpawnY = player.playerposition.Y;
                    playerPositionSpawnZ = player.playerposition.Z;

                    player.playerposition.X = platform.FloatToInt(player.playerposition.X) + one / 2;
                    //player.playerposition.Y = player.playerposition.Y;
                    player.playerposition.Z = platform.FloatToInt(player.playerposition.Z) + one / 2;
                }
                if (eKey == GetKey(GlKeys.F))
                {
                    ToggleFog();
                    Log(platform.StringFormat(language.FogDistance(), platform.IntToString(platform.FloatToInt(d_Config3d.viewdistance))));
                    OnResize();
                }
                if (eKey == GetKey(GlKeys.B))
                {
                    guistate = GuiState.Inventory;
                    menustate = new MenuState();
                    FreeMouse = true;
                }
                HandleMaterialKeys(eKey);
                if (eKey == GetKey(GlKeys.Escape))
                {
                    escapeMenu.EscapeMenuStart();
                }
            }
            else if (guistate == GuiState.EscapeMenu)
            {
                escapeMenu.EscapeMenuKeyDown(eKey);
                return;
            }
            else if (guistate == GuiState.Inventory)
            {
                if (eKey == GetKey(GlKeys.B)
                    || eKey == GetKey(GlKeys.Escape))
                {
                    GuiStateBackToGame();
                }
                if (eKey == GetKey(GlKeys.F12))
                {
                    game.platform.SaveScreenshot();
                    screenshotflash = 5;
                }
                return;
            }
            else if (guistate == GuiState.ModalDialog)
            {
                if (eKey == GetKey(GlKeys.B)
                    || eKey == GetKey(GlKeys.Escape))
                {
                    GuiStateBackToGame();
                }
                if (eKey == GetKey(GlKeys.F12))
                {
                    game.platform.SaveScreenshot();
                    screenshotflash = 5;
                }
            }
            else if (guistate == GuiState.MapLoading)
            {
            }
            else if (guistate == GuiState.CraftingRecipes)
            {
                if (eKey == GetKey(GlKeys.Escape))
                {
                    GuiStateBackToGame();
                }
            }
            else
            {
                platform.ThrowException("");
            }
        }

        public Vector3 ToVector3(Vector3Ref vector3Ref)
        {
            return new Vector3(vector3Ref.X, vector3Ref.Y, vector3Ref.Z);
        }

        public CrashReporter crashreporter;
        private void Connect()
        {
            escapeMenu.LoadOptions();

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
                Connect_(connectdata.Ip, connectdata.Port, connectdata.Username, connectdata.Auth, connectdata.ServerPassword);
            }
            MapLoadingStart();
        }
        //[Obsolete]
        
        //[Obsolete]
        public int[] MaterialSlots
        {
            get
            {
                return game.MaterialSlots();
            }
            set
            {
                materialSlots = value;
            }
        }

        Point mouse_previous;

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
            mouseCurrentX = System.Windows.Forms.Cursor.Position.X;
            mouseCurrentY = System.Windows.Forms.Cursor.Position.Y;
            if (FreeMouse)
            {
                mouseCurrentX = mouseCurrentX - d_GlWindow.X;
                mouseCurrentY = mouseCurrentY - d_GlWindow.Y;
                
                mouseCurrentY = mouseCurrentY - System.Windows.Forms.SystemInformation.CaptionHeight;
            }
            if (!d_GlWindow.Focused)
            {
                return;
            }
            if (freemousejustdisabled)
            {
                mouse_previous.X = mouseCurrentX;
                mouse_previous.Y = mouseCurrentY;
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

                    mouseDeltaX = mouseCurrentX - mouse_previous.X;
                    mouseDeltaY = mouseCurrentY - mouse_previous.Y;

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
                    mouseDeltaX = dx2;
                    mouseDeltaY = dy2;
                }
            }
        }
        public float MouseAcceleration1 = 0.12f;
        public float MouseAcceleration2 = 0.7f;
        OpenTK.Input.MouseState mouse_previous_state;
        int lastbuildMilliseconds;
        bool enable_move = true;
        public void OnUpdateFrame(FrameEventArgs e)
        {
        }
        //DateTime lasttodo;
        void FrameTick(FrameEventArgs e)
        {
            float dt = (float)e.Time;
            //if ((DateTime.Now - lasttodo).TotalSeconds > BuildDelay && todo.Count > 0)
            //UpdateTerrain();
            OnNewFrame(dt);
            RailOnNewFrame(dt);
            UpdateMousePosition();
            if (guistate == GuiState.Normal && game.enableCameraControl)
            {
                UpdateMouseViewportControl(dt);
            }
            NetworkProcess();
            if (guistate == GuiState.MapLoading) { return; }
            SetPlayers();
            bool angleup = false;
            bool angledown = false;
            float overheadcameraanglemovearea = one * 5 / 100;
            float overheadcameraspeed = 3;
            if (guistate == GuiState.Normal && d_GlWindow.Focused && cameratype == CameraType.Overhead)
            {
                if (mouseCurrentX > Width() - Width() * overheadcameraanglemovearea)
                {
                    overheadcameraK.TurnLeft(dt * overheadcameraspeed);
                }
                if (mouseCurrentX < Width() * overheadcameraanglemovearea)
                {
                    overheadcameraK.TurnRight(dt * overheadcameraspeed);
                }
                if (mouseCurrentY < Height() * overheadcameraanglemovearea)
                {
                    angledown = true;
                }
                if (mouseCurrentY > Height() - Height() * overheadcameraanglemovearea)
                {
                    angleup = true;
                }
            }
            bool wantsjump = GuiTyping == TypingState.None && keyboardState[GetKey(GlKeys.Space)];
            bool shiftkeydown = keyboardState[GetKey(GlKeys.ShiftLeft)];
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
                        if (keyboardState[GetKey(GlKeys.A)]) { overheadcameraK.TurnRight(dt * overheadcameraspeed); }
                        if (keyboardState[GetKey(GlKeys.D)]) { overheadcameraK.TurnLeft(dt * overheadcameraspeed); }
                        if (keyboardState[GetKey(GlKeys.W)]) { angleup = true; }
                        if (keyboardState[GetKey(GlKeys.S)]) { angledown = true; }
                        overheadcameraK.Center.X = player.playerposition.X;
                        overheadcameraK.Center.Y = player.playerposition.Y;
                        overheadcameraK.Center.Z = player.playerposition.Z;
                        m.Distance = overheadcameradistance;
                        m.AngleUp = angleup;
                        m.AngleDown = angledown;
                        overheadcameraK.Move(m, dt);
                        if ((ToVector3(player.playerposition) - ToVector3(playerdestination)).Length >= 1f)
                        {
                            movedy += 1;
                            if (d_Physics.reachedwall)
                            {
                                wantsjump = true;
                            }
                            //player orientation
                            Vector3 q = ToVector3(playerdestination) - ToVector3(player.playerposition);
                            float angle = VectorAngleGet(q.X, q.Y, q.Z);
                            player.playerorientation.Y = Game.GetPi() / 2 + angle;
                            player.playerorientation.X = Game.GetPi();
                        }
                    }
                    else if (enable_move)
                    {
                        if (keyboardState[GetKey(GlKeys.W)]) { movedy += 1; }
                        if (keyboardState[GetKey(GlKeys.S)]) { movedy += -1; }
                        if (keyboardState[GetKey(GlKeys.A)]) { movedx += -1; localplayeranimationhint.leanleft = true; localstance = 1; }
                        else { localplayeranimationhint.leanleft = false; }
                        if (keyboardState[GetKey(GlKeys.D)]) { movedx += 1; localplayeranimationhint.leanright = true; localstance = 2; }
                        else { localplayeranimationhint.leanright = false; }
                        if (!localplayeranimationhint.leanleft && !localplayeranimationhint.leanright) { localstance = 0; }
                    }
                }
                if (ENABLE_FREEMOVE || Swimming())
                {
                    if (GuiTyping == TypingState.None && keyboardState[GetKey(GlKeys.Space)])
                    {
                        moveup = true;
                    }
                    if (GuiTyping == TypingState.None && keyboardState[GetKey(GlKeys.ControlLeft)])
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
            else if (guistate == GuiState.ModalDialog)
            {
            }
            else
            {
                platform.ThrowException("");
            }
            float movespeednow = MoveSpeedNow();
            Acceleration acceleration = new Acceleration();
            IntRef blockunderplayer = BlockUnderPlayer();
            {
                //slippery walk on ice and when swimming
                if ((blockunderplayer != null && d_Data.IsSlipperyWalk()[blockunderplayer.value]) || Swimming())
                {
                    acceleration = new Acceleration();
                    {
                        acceleration.acceleration1 = one * 99 / 100;
                        acceleration.acceleration2 = one * 2 / 10;
                        acceleration.acceleration3 = 70;
                    }
                }
            }
            float jumpstartacceleration = (13 + one * 333 / 1000) * d_Physics.gravity;
            if (blockunderplayer != null && blockunderplayer.value == d_Data.BlockIdTrampoline()
                && (!player.isplayeronground) && !shiftkeydown)
            {
                wantsjump = true;
                jumpstartacceleration = (20 + one * 666 / 1000) * d_Physics.gravity;
            }
            //no aircontrol
            if (!player.isplayeronground)
            {
                acceleration = new Acceleration();
                {
                    acceleration.acceleration1 = one * 99 / 100;
                    acceleration.acceleration2 = one * 2 / 10;
                    acceleration.acceleration3 = 70;
                };
            }
            float pushX = 0;
            float pushY = 0;
            float pushZ = 0;
            Vector3Ref push = new Vector3Ref();
            GetEntitiesPush(push);
            pushX += push.X;
            pushY += push.Y;
            pushZ += push.Z;
            EntityExpire(dt);
            MoveInfo move = new MoveInfo();
            {
                move.movedx = movedx;
                move.movedy = movedy;
                move.acceleration = acceleration;
                move.ENABLE_FREEMOVE = ENABLE_FREEMOVE;
                move.ENABLE_NOCLIP = ENABLE_NOCLIP;
                move.jumpstartacceleration = jumpstartacceleration;
                move.movespeednow = movespeednow;
                move.moveup = moveup;
                move.movedown = movedown;
                move.Swimming = Swimming();
                move.wantsjump = wantsjump;
                move.shiftkeydown = shiftkeydown;
            }
            BoolRef soundnow = new BoolRef();
            if (FollowId() == null)
            {
                d_Physics.Move(player, move, dt, soundnow, Vector3Ref.Create(pushX, pushY, pushZ), entities[LocalPlayerId].player.ModelHeight);
                if (soundnow.value)
                {
                    UpdateWalkSound(-1);
                }
                if (player.isplayeronground && movedx != 0 || movedy != 0)
                {
                    UpdateWalkSound(dt);
                }
                UpdateBlockDamageToPlayer(dt);
                UpdateFallDamageToPlayer();
            }
            else
            {
                if (FollowId().value == LocalPlayerId)
                {
                    move.movedx = 0;
                    move.movedy = 0;
                    move.wantsjump = false;
                    d_Physics.Move(player, move, dt, soundnow, Vector3Ref.Create(pushX, pushY, pushZ),  entities[LocalPlayerId].player.ModelHeight);
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

            float orientationX = platform.MathSin(LocalPlayerOrientation.Y);
            float orientationY = 0;
            float orientationZ = -platform.MathCos(LocalPlayerOrientation.Y);
            game.platform.AudioUpdateListener(EyesPosX(), EyesPosY(), EyesPosZ(), orientationX, orientationY, orientationZ);

            Packet_Item activeitem = d_Inventory.RightHand[ActiveMaterial];
            int activeblock = 0;
            if (activeitem != null) { activeblock = activeitem.BlockId; }
            if (activeblock != PreviousActiveMaterialBlock)
            {
                Packet_Client p = new Packet_Client();
                {
                    p.Id = Packet_ClientIdEnum.ActiveMaterialSlot;
                    p.ActiveMaterialSlot = new Packet_ClientActiveMaterialSlot();
                    p.ActiveMaterialSlot.ActiveMaterialSlot = ActiveMaterial;
                }
                SendPacketClient(p);
            }
            PreviousActiveMaterialBlock = activeblock;
            playervelocity.X = LocalPlayerPosition.X - lastplayerposition.X;
            playervelocity.Y = LocalPlayerPosition.Y - lastplayerposition.Y;
            playervelocity.Z = LocalPlayerPosition.Z - lastplayerposition.Z;
            playervelocity.X *= 75;
            playervelocity.Y *= 75;
            playervelocity.Z *= 75;
            lastplayerposition = LocalPlayerPosition;
            if (reloadstartMilliseconds != 0
                && (one * (game.platform.TimeMillisecondsFromStart() - reloadstartMilliseconds) / 1000)
                > DeserializeFloat(game.blocktypes[reloadblock].ReloadDelayFloat))
            {
                {
                    int loaded = TotalAmmo[reloadblock];
                    loaded = Game.MinInt(game.blocktypes[reloadblock].AmmoMagazine, loaded);
                    LoadedAmmo[reloadblock] = loaded;
                    reloadstartMilliseconds = 0;
                    reloadblock = -1;
                }
            }
            for (int i = 0; i < entitiesCount; i++)
            {
                Entity entity = entities[i];
                if (entity == null) { continue; }
                if (entity.grenade == null) { continue; }
                UpdateGrenade(i, dt);
            }
        }

        Vector3 lastplayerposition;

        private void UpdatePicking()
        {
            if (FollowId() != null)
            {
                SelectedBlockPositionX = 0 - 1;
                SelectedBlockPositionY = 0 - 1;
                SelectedBlockPositionZ = 0 - 1;
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

            Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
            bool ispistol = (item != null && game.blocktypes[item.BlockId].IsPistol);
            bool ispistolshoot = ispistol && left;
            bool isgrenade = ispistol && game.blocktypes[item.BlockId].PistolType == Packet_PistolTypeEnum.Grenade;
            if (ispistol && isgrenade)
            {
                ispistolshoot = mouseleftdeclick;
            }
            //grenade cooking
            if (mouseleftclick)
            {
                grenadecookingstartMilliseconds = game.platform.TimeMillisecondsFromStart();
                if (ispistol && isgrenade)
                {
                    if (game.blocktypes[item.BlockId].Sounds.Shoot.Length > 0)
                    {
                        AudioPlay(game.blocktypes[item.BlockId].Sounds.Shoot[0] + ".ogg");
                    }
                }
            }
            float wait = ((float)(game.platform.TimeMillisecondsFromStart() - grenadecookingstartMilliseconds) / 1000);
            if (isgrenade && left)
            {
                if (wait >= grenadetime && isgrenade && grenadecookingstartMilliseconds != 0)
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
                grenadecookingstartMilliseconds = 0;
            }

            if (ispistol && mouserightclick && (game.platform.TimeMillisecondsFromStart() - lastironsightschangeMilliseconds) >= 500)
            {
                IronSights = !IronSights;
                lastironsightschangeMilliseconds = game.platform.TimeMillisecondsFromStart();
            }

            float unit_x = 0;
            float unit_y = 0;
            int NEAR = 1;
            int FOV = platform.FloatToInt(currentfov() * 10); // 600
            float ASPECT = one * 640 / 480;
            float near_height = NEAR * one * (platform.MathTan(FOV * Game.GetPi() / 360));
            Vector3 ray = new Vector3(unit_x * near_height * ASPECT, unit_y * near_height, 1);//, 0);

            Vector3 ray_start_point = new Vector3(0, 0, 0);
            PointFloatRef aim = GetAim();
            if (overheadcamera || aim.X != 0 || aim.Y != 0)
            {
                float mx = 0;
                float my = 0;
                if (overheadcamera)
                {
                    mx = one * mouseCurrentX / Width() - (one / 2);
                    my = one * mouseCurrentY / Height() - (one / 2);
                }
                else if (ispistolshoot && (aim.X != 0 || aim.Y != 0))
                {
                    mx += aim.X / Width();
                    my += aim.Y / Height();
                }
                //ray_start_point = new Vector3(mx * 1.4f, -my * 1.1f, 0.0f);
                ray_start_point = new Vector3(mx * 3f, -my * 2.2f, -1.0f);
            }

            //Matrix4 the_modelview;
            //Read the current modelview matrix into the array the_modelview
            //GL.GetFloat(GetPName.ModelviewMatrix, out the_modelview);
            
            Matrix4 theModelView = ToMatrix4(mvMatrix.Peek());
            if (theModelView.M11 == float.NaN || theModelView.Equals(new Matrix4())) { return; }
            //Matrix4 theModelView = d_The3d.ModelViewMatrix;
            theModelView.Invert();
            //the_modelview = new Matrix4();
            ray = Vector3.Transform(ray, theModelView);
            ray_start_point = Vector3.Transform(ray_start_point, theModelView);
            Line3D pick = new Line3D();
            Vector3 raydir = -(ray - ray_start_point);
            raydir.Normalize();
            pick.Start = Vector3ToFloatArray(ray + Vector3.Multiply(raydir, 1f)); //do not pick behind
            pick.End = Vector3ToFloatArray( ray + Vector3.Multiply(raydir, pick_distance * ((ispistolshoot) ? 100 : 2)));

            //pick models
            selectedmodelid = -1;
            //Intersection intersection1 = new Intersection();
            //foreach (var m in Models)
            //{
            //    Vector3 closestmodelpos = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
            //    foreach (var t in m.TrianglesForPicking)
            //    {
            //        float[] intersection_ = new float[3];
            //        if (intersection1.RayTriangle(pick, t, intersection_) == 1)
            //        {
            //            Vector3 intersection = FloatArrayToVector3(intersection_);
            //            if ((FloatArrayToVector3(pick.Start) - intersection).Length > pick_distance)
            //            {
            //                continue;
            //            }
            //            if ((FloatArrayToVector3(pick.Start) - intersection).Length < (FloatArrayToVector3(pick.Start) - closestmodelpos).Length)
            //            {
            //                closestmodelpos = intersection;
            //                selectedmodelid = m.Id;
            //            }
            //        }
            //    }
            //}
            if (selectedmodelid != -1)
            {
                SelectedBlockPositionX = -1;
                SelectedBlockPositionY = -1;
                SelectedBlockPositionZ = -1;
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

            //pick terrain
            s.StartBox = Box3D.Create(0, 0, 0, BitTools.NextPowerOfTwo(Math.Max(d_Map.MapSizeX, Math.Max(d_Map.MapSizeY, d_Map.MapSizeZ))));
            IntRef pick2count = new IntRef();
            List<BlockPosSide> pick2 = new List<BlockPosSide>(s.LineIntersection(IsBlockEmpty_.Create(game), GetBlockHeight_.Create(game), pick, pick2count));
            
            pick2.Sort((a, b) => { return (FloatArrayToVector3(a.blockPos) - ray_start_point).Length.CompareTo((FloatArrayToVector3(b.blockPos) - ray_start_point).Length); });

            if (overheadcamera && pick2.Count > 0 && left)
            {
                //if not picked any object, and mouse button is pressed, then walk to destination.
                playerdestination = Vector3Ref.Create(pick2[0].blockPos[0], pick2[0].blockPos[1], pick2[0].blockPos[2]);
            }
            bool pickdistanceok = pick2.Count > 0 && (FloatArrayToVector3(pick2[0].blockPos) - ToVector3(player.playerposition)).Length <= pick_distance;
            bool playertileempty = IsTileEmptyForPhysics(
                        (int)(player.playerposition.X),
                        (int)(player.playerposition.Z),
                        (int)(player.playerposition.Y + (one / 2)));
            bool playertileemptyclose = IsTileEmptyForPhysicsClose(
                        (int)(player.playerposition.X),
                        (int)(player.playerposition.Z),
                        (int)(player.playerposition.Y + (one / 2)));
            BlockPosSide pick0 = new BlockPosSide();
            if (pick2.Count > 0 &&
                ((pickdistanceok && (playertileempty || (playertileemptyclose)))
                || overheadcamera)
                )
            {
                SelectedBlockPositionX = game.platform.FloatToInt(pick2[0].Current()[0]);
                SelectedBlockPositionY = game.platform.FloatToInt(pick2[0].Current()[1]);
                SelectedBlockPositionZ = game.platform.FloatToInt(pick2[0].Current()[2]);
                pick0 = pick2[0];
            }
            else
            {
                SelectedBlockPositionX = -1;
                SelectedBlockPositionY = -1;
                SelectedBlockPositionZ = -1;
                pick0.blockPos = new float[3];
                pick0.blockPos[0] = -1;
                pick0.blockPos[1] = -1;
                pick0.blockPos[2] = -1;
            }
            if (FreeMouse)
            {
                if (pick2.Count > 0)
                {
                    OnPick(pick0);
                }
                return;
            }
            var ntile = FloatArrayToVector3(pick0.Current());
            if (IsUsableBlock(d_Map.GetBlock((int)ntile.X, (int)ntile.Z, (int)ntile.Y)))
            {
                currentAttackedBlock = Vector3IntRef.Create((int)ntile.X, (int)ntile.Z, (int)ntile.Y);
            }
            if ((one * (game.platform.TimeMillisecondsFromStart() - lastbuildMilliseconds) / 1000) >= BuildDelay
                || IsNextShot)
            {
                if (left && d_Inventory.RightHand[ActiveMaterial] == null)
                {
                    Packet_ClientHealth p = new Packet_ClientHealth { CurrentHealth = (int)(2 + rnd.NextDouble() * 4) };
                    byte[] packet = Serialize(new Packet_Client() { Id = Packet_ClientIdEnum.MonsterHit, Health = p }, packetLen);
                    SendPacket(packet, packetLen.value);
                }
                if (left && !fastclicking)
                {
                    //todo animation
                    fastclicking = false;
                }
                if ((left || right || middle) && (!isgrenade))
                {
                    lastbuildMilliseconds = game.platform.TimeMillisecondsFromStart();
                }
                if (isgrenade && mouseleftdeclick)
                {
                    lastbuildMilliseconds = game.platform.TimeMillisecondsFromStart();
                }
                if (reloadstartMilliseconds != 0)
                {
                    goto end;
                }
                if (ispistolshoot)
                {
                    if ((!(LoadedAmmo[item.BlockId] > 0))
                        || (!(TotalAmmo[item.BlockId] > 0)))
                    {
                        AudioPlay("Dry Fire Gun-SoundBible.com-2053652037.ogg");
                        goto end;
                    }
                }
                if (ispistolshoot)
                {
                    Vector3 to = FloatArrayToVector3(pick.End);
                    if (pick2.Count > 0)
                    {
                        to = FloatArrayToVector3(pick2[0].blockPos);
                    }

                    Packet_ClientShot shot = new Packet_ClientShot();
                    shot.FromX = SerializeFloat(FloatArrayToVector3(pick.Start).X);
                    shot.FromY = SerializeFloat(FloatArrayToVector3(pick.Start).Y);
                    shot.FromZ = SerializeFloat(FloatArrayToVector3(pick.Start).Z);
                    shot.ToX = SerializeFloat(to.X);
                    shot.ToY = SerializeFloat(to.Y);
                    shot.ToZ = SerializeFloat(to.Z);
                    shot.HitPlayer = -1;

                    for(int i = 0; i < entitiesCount; i++)
                    {
                        if (entities[i] == null)
                        {
                            continue;
                        }
                        if (entities[i].player == null)
                        {
                            continue;
                        }
                        Player p_ = entities[i].player;
                        if (!p_.PositionLoaded)
                        {
                            continue;
                        }
                        Vector3 feetpos = new Vector3(p_.PositionX, p_.PositionY, p_.PositionZ);
                        //var p = PlayerPositionSpawn;
                        Box3D bodybox = new Box3D();
                        float headsize = (p_.ModelHeight - p_.EyeHeight) * 2; //0.4f;
                        float h = p_.ModelHeight - headsize;
                        float r = 0.35f;

                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + 0, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + 0, feetpos.Z + r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + 0, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + 0, feetpos.Z + r);

                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z + r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z - r);
                        bodybox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z + r);

                        Box3D headbox = new Box3D();

                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z + r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z + r);

                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h + headsize, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X - r, feetpos.Y + h + headsize, feetpos.Z + r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h + headsize, feetpos.Z - r);
                        headbox.AddPoint(feetpos.X + r, feetpos.Y + h + headsize, feetpos.Z + r);

                        float[] p;
                        Vector3 localeyepos = LocalPlayerPosition + new Vector3(0, entities[LocalPlayerId].player.ModelHeight, 0);
                        if ((p = Intersection.CheckLineBoxExact(pick, headbox)) != null)
                        {
                            //do not allow to shoot through terrain
                            if (pick2.Count == 0 || ((FloatArrayToVector3(pick2[0].blockPos) - localeyepos).Length > (FloatArrayToVector3(p) - localeyepos).Length))
                            {
                                if (!isgrenade)
                                {
                                    Entity entity = new Entity();
                                    Sprite sprite = new Sprite();
                                    sprite.positionX = p[0];
                                    sprite.positionY = p[1];
                                    sprite.positionZ = p[2];
                                    sprite.image = "blood.png";
                                    entity.sprite = sprite;
                                    entity.expires = Expires.Create(one * 2 / 10);
                                    EntityAddLocal(entity);
                                }
                                shot.HitPlayer = i;
                                shot.IsHitHead = 1;
                            }
                        }
                        else if ((p = Intersection.CheckLineBoxExact(pick, bodybox)) != null)
                        {
                            //do not allow to shoot through terrain
                            if (pick2.Count == 0 || ((FloatArrayToVector3(pick2[0].blockPos) - localeyepos).Length > (FloatArrayToVector3(p) - localeyepos).Length))
                            {
                                if (!isgrenade)
                                {
                                    Entity entity = new Entity();
                                    Sprite sprite = new Sprite();
                                    sprite.positionX = p[0];
                                    sprite.positionY = p[1];
                                    sprite.positionZ = p[2];
                                    sprite.image = "blood.png";
                                    entity.sprite = sprite;
                                    entity.expires = Expires.Create(one * 2 / 10);
                                    EntityAddLocal(entity);
                                }
                                shot.HitPlayer = i;
                                shot.IsHitHead = 0;
                            }
                        }
                    }
                    shot.WeaponBlock = item.BlockId;
                    LoadedAmmo[item.BlockId] = LoadedAmmo[item.BlockId] - 1;
                    TotalAmmo[item.BlockId] = TotalAmmo[item.BlockId] - 1;
                    float projectilespeed = DeserializeFloat(game.blocktypes[item.BlockId].ProjectileSpeedFloat);
                    if (projectilespeed == 0)
                    {
                        {
                            Entity entity = CreateBulletEntity(
                              pick.Start[0], pick.Start[1], pick.Start[2],
                              to.X, to.Y, to.Z, 150);
                            EntityAddLocal(entity);
                        }
                    }
                    else
                    {
                        Vector3 v = to - FloatArrayToVector3(pick.Start);
                        v.Normalize();
                        v *= projectilespeed;
                        shot.ExplodesAfter = SerializeFloat(grenadetime - wait);

                        {
                            Entity grenadeEntity = new Entity();

                            Sprite sprite = new Sprite();
                            sprite.image = "ChemicalGreen.png";
                            sprite.size = 14;
                            sprite.animationcount = 0;
                            sprite.positionX = pick.Start[0];
                            sprite.positionY = pick.Start[1];
                            sprite.positionZ = pick.Start[2];
                            grenadeEntity.sprite = sprite;
                            
                            Grenade_ projectile = new Grenade_();
                            projectile.velocityX = v.X;
                            projectile.velocityY = v.Y;
                            projectile.velocityZ = v.Z;
                            projectile.block = item.BlockId;

                            grenadeEntity.expires = Expires.Create(grenadetime - wait);
                            
                            grenadeEntity.grenade = projectile;
                            EntityAddLocal(grenadeEntity);
                        }
                    }
                    SendPacketClient(new Packet_Client() { Id = Packet_ClientIdEnum.Shot, Shot = shot });

                    if (game.blocktypes[item.BlockId].Sounds.ShootEnd.Length > 0)
                    {
                        pistolcycle = rnd.Next(game.blocktypes[item.BlockId].Sounds.ShootEnd.Length);
                        AudioPlay(game.blocktypes[item.BlockId].Sounds.ShootEnd[pistolcycle] + ".ogg");
                    }

                    bulletsshot++;
                    if (bulletsshot < DeserializeFloat(game.blocktypes[item.BlockId].BulletsPerShotFloat))
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
                        var newtile = FloatArrayToVector3(pick0.Current());
                        if (d_Map.IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            int clonesource = d_Map.GetBlock((int)newtile.X, (int)newtile.Z, (int)newtile.Y);
                            int clonesource2 = (int)d_Data.WhenPlayerPlacesGetsConvertedTo()[(int)clonesource];
                            //find this block in another right hand.
                            for (int i = 0; i < 10; i++)
                            {
                                if (d_Inventory.RightHand[i] != null
                                    && d_Inventory.RightHand[i].ItemClass == Packet_ItemClassEnum.Block
                                    && (int)d_Inventory.RightHand[i].BlockId == clonesource2)
                                {
                                    ActiveMaterial = i;
                                    goto done;
                                }
                            }
                            IntRef freehand = d_InventoryUtil.FreeHand(ActiveMaterial);
                            //find this block in inventory.
                            foreach (var k in d_Inventory.Items)
                            {
                                if (k == null)
                                {
                                    continue;
                                }
                                if (k.Value_.ItemClass == Packet_ItemClassEnum.Block
                                    && k.Value_.BlockId == clonesource2)
                                {
                                    //free hand
                                    if (freehand != null)
                                    {
                                        d_InventoryController.WearItem(
                                            InventoryPositionMainArea(k.X, k.Y),
                                            InventoryPositionMaterialSelector(freehand.value));
                                        goto done;
                                    }
                                    //try to replace current slot
                                    if (d_Inventory.RightHand[ActiveMaterial] != null
                                        && d_Inventory.RightHand[ActiveMaterial].ItemClass == Packet_ItemClassEnum.Block)
                                    {
                                        d_InventoryController.MoveToInventory(
                                            InventoryPositionMaterialSelector(ActiveMaterial));
                                        d_InventoryController.WearItem(
                                            InventoryPositionMainArea(k.X, k.Y),
                                            InventoryPositionMaterialSelector(ActiveMaterial));
                                    }
                                }
                            }
                        done:
                            string[] sound = d_Data.CloneSound()[clonesource];
                            if (sound != null && sound.Length > 0)
                            {
                                AudioPlay(sound[0]); //todo sound cycle
                            }
                        }
                    }
                    if (left || right)
                    {
                        BlockPosSide tile = pick0;
                        Vector3 newtile = right ? FloatArrayToVector3(tile.Translated()) : FloatArrayToVector3(tile.Current());
                        if (d_Map.IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            Console.WriteLine(". newtile:" + newtile + " type: " + d_Map.GetBlock((int)newtile.X, (int)newtile.Z, (int)newtile.Y));
                            if (FloatArrayToVector3(pick0.blockPos) != new Vector3(-1, -1, -1))
                            {
                                int blocktype;
                                if (left) { blocktype = d_Map.GetBlock((int)newtile.X, (int)newtile.Z, (int)newtile.Y); }
                                else { blocktype = ((BlockInHand() == null) ? 1 : BlockInHand().value); }
                                if (left && blocktype == d_Data.BlockIdAdminium()) { goto end; }
                                string[] sound = left ? d_Data.BreakSound()[blocktype] : d_Data.BuildSound()[blocktype];
                                if (sound != null && sound.Length > 0)
                                {
                                    AudioPlay(sound[0]); //todo sound cycle
                                }
                            }
                            //normal attack
                            if (!right)
                            {
                                //attack
                                var pos = new Vector3i((int)newtile.X, (int)newtile.Z, (int)newtile.Y);
                                currentAttackedBlock = Vector3IntRef.Create(pos.x, pos.y, pos.z);
                                if (!blockHealth.ContainsKey(pos.x, pos.y, pos.z))
                                {
                                    blockHealth.Set(pos.x, pos.y, pos.z, GetCurrentBlockHealth(pos.x, pos.y, pos.z));
                                }
                                blockHealth.Set(pos.x, pos.y, pos.z, blockHealth.Get(pos.x, pos.y, pos.z) - WeaponAttackStrength());
                                float health = GetCurrentBlockHealth(pos.x, pos.y, pos.z);
                                if (health <= 0)
                                {
                                    if (currentAttackedBlock != null)
                                    {
                                        blockHealth.Remove(pos.x, pos.y, pos.z);
                                    }
                                    currentAttackedBlock = null;
                                    goto broken;
                                }
                                goto end;
                            }
                            if (!right)
                            {
                                particleEffectBlockBreak.StartParticleEffect(newtile.X, newtile.Y, newtile.Z);//must be before deletion - gets ground type.
                            }
                            if (!d_Map.IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                            {
                                throw new Exception();
                            }
                        broken:
                            OnPick(platform.FloatToInt(newtile.X), platform.FloatToInt(newtile.Z), platform.FloatToInt(newtile.Y),
                                (int)tile.Current()[0], (int)tile.Current()[2], (int)tile.Current()[1],
                                tile.collisionPos,
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
                lastbuildMilliseconds = 0;
                fastclicking = true;
            }
        }

        Vector3 FloatArrayToVector3(float[] v)
        {
            return new Vector3(v[0], v[1], v[2]);
        }

        float[] Vector3ToFloatArray(Vector3 v)
        {
            return Vec3.FromValues(v.X, v.Y, v.Z);
        }

        Matrix4 ToMatrix4(float[] mvMatrix)
        {
            return new Matrix4(
                mvMatrix[0],
                mvMatrix[1],
                mvMatrix[2],
                mvMatrix[3],
                mvMatrix[4],
                mvMatrix[5],
                mvMatrix[6],
                mvMatrix[7],
                mvMatrix[8],
                mvMatrix[9],
                mvMatrix[10],
                mvMatrix[11],
                mvMatrix[12],
                mvMatrix[13],
                mvMatrix[14],
                mvMatrix[15]);
        }

        public const float RailHeight = 0.3f;

        private void OnPick(BlockPosSide pick0)
        {
            //playerdestination = pick0.pos;
        }
        bool fastclicking = false;

        double accumulator = 0;
        double t = 0;

        public void OnRenderFrame(FrameEventArgs e)
        {
            terrainRenderer.UpdateTerrain();
            if (guistate == GuiState.MapLoading)
            {
                platform.GlClearColorRgbaf(0, 0, 0, 1);
            }
            else
            {
                platform.GlClearColorRgbaf(one * Game.clearcolorR / 255, one * Game.clearcolorG / 255, one * Game.clearcolorB / 255, one * Game.clearcolorA / 255);
            }

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
            LoadPlayerTextures();

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
            else if (!keyboardState[GetKey(GlKeys.LControl)])
            {
                ActiveMaterial -= Mouse.WheelDelta;
                ActiveMaterial = ActiveMaterial % 10;
                while (ActiveMaterial < 0)
                {
                    ActiveMaterial += 10;
                }
            }
            SetAmbientLight(terraincolor());
            //const float alpha = accumulator / dt;
            //Vector3 currentPlayerPosition = currentState * alpha + previousState * (1.0f - alpha);
            UpdateTitleFps(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindTexture(TextureTarget.Texture2D, game.d_TerrainTextures.terrainTexture());

            GLMatrixModeModelView();

            float[] camera;
            if (overheadcamera)
            {
                camera = OverheadCamera();
            }
            else
            {
                camera = FppCamera();
            }
            GLLoadMatrix(camera);
            game.CameraMatrix.lastmvmatrix = camera;

            d_FrustumCulling.CalcFrustumEquations();

            bool drawgame = guistate != GuiState.MapLoading;
            if (drawgame)
            {
                GL.Disable(EnableCap.Fog);
                DrawSkySphere();
                if (d_Config3d.viewdistance < 512)
                {
                    SetFog();
                }
                d_SunMoonRenderer.Draw((float)e.Time);

                InterpolatePositions((float)e.Time);
                DrawPlayers((float)e.Time);
                terrainRenderer.DrawTerrain();
                DrawPlayerNames();
                particleEffectBlockBreak.Draw((float)e.Time);
                if (ENABLE_DRAW2D)
                {
                    game.DrawLinesAroundSelectedBlock(SelectedBlockPositionX,
                        SelectedBlockPositionY, SelectedBlockPositionZ);
                }
                DrawSprites();
                UpdateBullets((float)e.Time);
                foreach (IModelToDraw m in Models)
                {
                    if (m.Id() == selectedmodelid)
                    {
                        //GL.Color3(Color.Red);
                    }
                    m.Draw((float)e.Time);
                    //GL.Color3(Color.White);
                    
                    //GL.Begin(BeginMode.Triangles);
                    //foreach (var tri in m.TrianglesForPicking)
                    //{
                    //    GL.Vertex3(tri.PointA);
                    //    GL.Vertex3(tri.PointB);
                    //    GL.Vertex3(tri.PointC);
                    //}
                    //GL.End();
                    
                }
                if ((!ENABLE_TPP_VIEW) && ENABLE_DRAW2D)
                {
                    Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
                    string img = null;
                    if (item != null)
                    {
                        img = game.blocktypes[item.BlockId].Handimage;
                        if (IronSights)
                        {
                            img = game.blocktypes[item.BlockId].IronSightsImage;
                        }
                    }
                    if (img == null)
                    {
                        d_Weapon.DrawWeapon((float)e.Time);
                    }
                    else
                    {
                        OrthoMode(Width(), Height());
                        Draw2dBitmapFile(img, Width() / 2, Height() - 512, 512, 512);
                        PerspectiveMode();
                    }
                }
            }
        draw2d:
            SetAmbientLight(Game.ColorFromArgb(255, 255, 255, 255));
            Draw2d();

            for (int i = 0; i < clientmodsCount; i++)
            {
                NewFrameEventArgs args_ = new NewFrameEventArgs();
                args_.SetDt((float)e.Time);
                clientmods[i].OnNewFrame(args_);
            }

            //OnResize(new EventArgs());
            d_GlWindow.SwapBuffers();
            mouseleftclick = mouserightclick = false;
            mouseleftdeclick = mouserightdeclick = false;
            if (!startedconnecting) { startedconnecting = true; Connect(); }
        }

        bool startedconnecting;

        private void Draw2d()
        {
            OrthoMode(Width(), Height());
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
                            PerspectiveMode();
                            return;
                        }
                        if (cameratype != CameraType.Overhead)
                        {
                            DrawAim();
                        }
                        d_HudInventory.DrawMaterialSelector();
                        DrawPlayerHealth();
                        DrawPlayerOxygen();
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
                        if (!ENABLE_DRAW2D)
                        {
                            PerspectiveMode();
                            return;
                        }
                        d_HudChat.DrawChatLines(GuiTyping == TypingState.Typing);
                        DrawDialogs();
                        escapeMenu.EscapeMenuDraw();
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
                case GuiState.ModalDialog:
                    {
                        DrawDialogs();
                    }
                    break;
                default:
                    throw new Exception();
            }
            //d_The3d.OrthoMode(Width, Height);
            if (ENABLE_DRAWPOSITION)
            {
                float heading = one * HeadingByte(LocalPlayerOrientation.X, LocalPlayerOrientation.Y, LocalPlayerOrientation.Z);
                float pitch = one * PitchByte(LocalPlayerOrientation.X, LocalPlayerOrientation.Y, LocalPlayerOrientation.Z);
                string postext = "X: " + MathFloor(player.playerposition.X)
                	+ ",\tY: " + MathFloor(player.playerposition.Z)
                	+ ",\tZ: " + MathFloor(player.playerposition.Y)
                	+ "\nHeading: " + MathFloor(heading)
                	+ "\nPitch: " + MathFloor(pitch);
                FontCi font = new FontCi();
                font.family = "Arial";
                font.size = d_HudChat.ChatFontSize;
                Draw2dText(postext, font, 100, 460, null, false);
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
            float lagSeconds = one * (game.platform.TimeMillisecondsFromStart() - LastReceivedMilliseconds) / 1000;
            if ((lagSeconds >= DISCONNECTED_ICON_AFTER_SECONDS && lagSeconds < 60 * 60 * 24)
                && invalidVersionDrawMessage == null)
            {
                Draw2dBitmapFile("disconnected.png", Width() - 100, 50, 50, 50);
                FontCi font = new FontCi();
                font.family = "Arial";
                font.size = 12;
                Draw2dText(platform.IntToString(platform.FloatToInt(lagSeconds)), font, Width() - 100, 50 + 50 + 10, null, false);
                Draw2dText("Press F6 to reconnect", font, Width() / 2 - 200 / 2, 50, null, false);
            }
            PerspectiveMode();
        }

        public int DISCONNECTED_ICON_AFTER_SECONDS = 10;

        bool drawblockinfo = false;
        void CraftingMouse()
        {
            if (okrecipes == null)
            {
                return;
            }
            int menustartx = xcenter(600);
            int menustarty = ycenter(okrecipes.Count * 80);
            if (mouseCurrentY >= menustarty && mouseCurrentY < menustarty + okrecipes.Count * 80)
            {
                craftingselectedrecipe = (mouseCurrentY - menustarty) / 80;
            }
            else
            {
                //craftingselectedrecipe = -1;
            }
            if (mouseleftclick)
            {
                if (okrecipes.Count != 0)
                {
                    craftingrecipeselected(IntRef.Create(okrecipes[craftingselectedrecipe]));
                }
                mouseleftclick = false;
                GuiStateBackToGame();
            }
        }
        
        Random rnd = new Random();
        void UpdateTitleFps(FrameEventArgs e)
        {
            float elapsed = one * (game.platform.TimeMillisecondsFromStart() - lasttitleupdateMilliseconds) / 1000;
            if (elapsed >= 1)
            {
                lasttitleupdateMilliseconds = game.platform.TimeMillisecondsFromStart();
                int chunkupdates = terrainRenderer.ChunkUpdates();
                PerformanceInfo.Set("chunk updates", string.Format(language.ChunkUpdates(), (chunkupdates - lastchunkupdates)));
                lastchunkupdates = terrainRenderer.ChunkUpdates();
                PerformanceInfo.Set("triangles", string.Format(language.Triangles(), terrainRenderer.TrianglesCount()));
            }
            if (!titleset)
            {
                d_GlWindow.Title = applicationname;
                titleset = true;
            }
        }
        bool titleset = false;
        string applicationname;
        #region ILocalPlayerPosition Members
        public Vector3 LocalPlayerPosition
        {
            get
            {
                if (FollowId() != null)
                {
                    if (FollowId().value == LocalPlayerId)
                    {
                        return ToVector3(player.playerposition);
                    }
                    Player p = entities[FollowId().value].player;
                    return new Vector3(p.PositionX, p.PositionY, p.PositionZ);
                }
                return ToVector3(player.playerposition);
            }
            set
            {
                if (FollowId() != null)
                {
                    return;
                }
                player.playerposition.X = value.X;
                player.playerposition.Y = value.Y;
                player.playerposition.Z = value.Z;
            }
        }
        public Vector3 LocalPlayerOrientation
        {
            get
            {
                if (FollowId() != null)
                {
                    if (FollowId().value == LocalPlayerId)
                    {
                        return new Vector3(
                            player.playerorientation.X,
                            player.playerorientation.Y,
                            player.playerorientation.Z);
                    }
                    Player p = entities[FollowId().value].player;
                    return new Vector3(HeadingToOrientationX(p.Heading), PitchToOrientationY(p.Pitch), 0);
                }
                return new Vector3(
                    player.playerorientation.X,
                    player.playerorientation.Y,
                    player.playerorientation.Z);
            }
            set
            {
                player.playerorientation.X = value.X;
                player.playerorientation.Y = value.Y;
                player.playerorientation.Z = value.Z;
            }
        }
        #endregion
        
        #region IViewport3d Members
        public Vector3 PickCubePos { get { return new Vector3(SelectedBlockPositionX, SelectedBlockPositionY, SelectedBlockPositionZ); } }
        #endregion
        public OpenTK.Input.MouseDevice Mouse { get { return d_GlWindow.Mouse; } }
        public void Run()
        {
            d_GlWindow.Run();
        }
        public void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (guistate == GuiState.Inventory)
            {
                d_HudInventory.OnKeyPress(e.KeyChar);
            }
        }
        public void ModelClick(int selectedmodelid)
        {
        }

        IntRef packetLen = new IntRef();

        public void Dispose()
        {
            if (main != null)
            {
                //main.Disconnect(false);
                main = null;
            }
        }
 
        public bool ShadowsFull { get { return false; } set { } }
        public FontType Font;
    }
}


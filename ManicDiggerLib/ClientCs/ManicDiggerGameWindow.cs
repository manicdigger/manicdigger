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
    public partial class ManicDiggerGameWindow : IMyGameWindow
    {
        public ManicDiggerGameWindow()
        {
            game = new Game();
        }
        
        public void Start()
        {
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

            game.Start();
            escapeMenu.game = game;
        }

        public Game game;

        public GlWindow d_GlWindow;
        public Game d_Map;

        public ManicDiggerGameWindow d_Shadows;

        public bool IsMono = Type.GetType("Mono.Runtime") != null;
        public bool IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;

        
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
        
        public void OnLoad(EventArgs e)
        {
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
            if (!platform.Focused())
            {
                return;
            }
            game.MouseUp(GetMouseEventArgs(e));
        }
        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!platform.Focused())
            {
                return;
            }
            game.MouseDown(GetMouseEventArgs(e));
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
            BoolRef keyHandled = new BoolRef();
            game.KeyDown(eKey, keyHandled);
        }

        public CrashReporter crashreporter;
        private void Connect()
        {
            escapeMenu.LoadOptions();

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
        
        public void OnUpdateFrame(FrameEventArgs e)
        {
        }
        //DateTime lasttodo;
        void FrameTick(FrameEventArgs e)
        {
            float dt = (float)e.Time;
            UpdateMousePosition();
            game.FrameTick(dt);
        }

        double accumulator;
        double t;

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
            UpdateTitleFps((float)e.Time);

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
                DrawMinecarts((float)e.Time);
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
                        if (lasthandimage != img)
                        {
                            lasthandimage = img;
                            byte[] file = game.GetFile(img);
                            BitmapCi bmp = platform.BitmapCreateFromPng(file, platform.ByteArrayLength(file));
                            if (bmp != null)
                            {
                                game.handTexture = game.platform.LoadTextureFromBitmap(bmp);
                                platform.BitmapDelete(bmp);
                            }
                        }
                        Draw2dTexture(game.handTexture, Width() / 2, Height() - 512, 512, 512, null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
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
            if ((!issingleplayer)
                || (issingleplayer && StartedSinglePlayerServer))
            {
                if (!startedconnecting)
                {
                    startedconnecting = true;
                    Connect();
                }
            }
        }
        string lasthandimage;
        bool startedconnecting;
        
        Random rnd = new Random();        
        public OpenTK.Input.MouseDevice Mouse { get { return d_GlWindow.Mouse; } }
        public void Run()
        {
            d_GlWindow.Run();
        }
        public void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            game.OnKeyPress(e.KeyChar);
        }

        public void Dispose()
        {
            if (main != null)
            {
                //main.Disconnect(false);
                main = null;
            }
        }
 
        public bool ShadowsFull { get { return false; } set { } }
    }
}


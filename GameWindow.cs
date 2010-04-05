using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using DependencyInjection;
using ManicDigger.Collisions;
using System.Runtime.InteropServices;
using System.Drawing;
using OpenTK.Graphics;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;

namespace ManicDigger
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTexture
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
    public class VerticesIndicesToLoad
    {
        public VertexPositionTexture[] vertices;
        public ushort[] indices;
        public Vector3 position;
    }
    public class Config3d
    {
        public bool ENABLE_BACKFACECULLING = true;
        public bool ENABLE_TRANSPARENCY = true;
        public bool ENABLE_MIPMAPS = true;
        public bool ENABLE_VSYNC = false;
        public bool ENABLE_VISIBILITY_CULLING = false;
        public float viewdistance = 256;
    }
    public interface IThe3d
    {
        int LoadTexture(string filename);
    }
    public class CameraMove
    {
        public bool TurnLeft;
        public bool TurnRight;
        public bool DistanceUp;
        public bool DistanceDown;
        public bool AngleUp;
        public bool AngleDown;
        public int MoveX;
        public int MoveY;
        public float Distance;
    }
    public interface IKamera
    {
        void Move(CameraMove move, float p);
        Vector3 Position { get; }
    }
    public class Kamera : IKamera
    {
        public Vector3 Position
        {
            get
            {
                float cx = (float)(Math.Cos(tt * .5) * FlatDistance + Center.X);
                float cy = (float)(Math.Sin(tt * .5) * FlatDistance + Center.Z);
                return new Vector3(cx, Center.Y + CameraHeightFromCenter, cy);
            }
        }
        float distance = 5;
        public float Distance
        {
            get { return distance; }
            set
            {
                distance = value;
                if (distance < MinimumDistance)
                {
                    distance = MinimumDistance;
                }
            }
        }
        public float Angle = 45;
        public float MinimumDistance = 2f;
        float CameraHeightFromCenter
        {
            //get { return (float)Math.Tan(Angle * Math.PI/180) * Distance; }
            get { return (float)Math.Sin(Angle * Math.PI / 180) * Distance; }
        }
        float FlatDistance
        {
            get { return (float)Math.Cos(Angle * Math.PI / 180) * Distance; }
        }
        public Vector3 Center { get; set; }
        float tt = 0;
        public void TurnLeft(float p)
        {
            tt += p;
        }
        public void TurnRight(float p)
        {
            tt -= p;
        }
        public void Move(CameraMove camera_move, float p)
        {
            p *= 2;
            p *= 2;
            if (camera_move.TurnLeft)
            {
                TurnLeft(p);
            }
            if (camera_move.TurnRight)
            {
                TurnRight(p);
            }
            if (camera_move.DistanceUp)
            {
                Distance += p;
            }
            if (camera_move.DistanceDown)
            {
                Distance -= p;
            }
            if (camera_move.AngleUp)
            {
                Angle += p * 10;
            }
            if (camera_move.AngleDown)
            {
                Angle -= p * 10;
            }
            Distance = camera_move.Distance;
            if (MaximumAngle < MinimumAngle) { throw new Exception(); }
            if (Angle > MaximumAngle) { Angle = MaximumAngle; }
            if (Angle < MinimumAngle) { Angle = MinimumAngle; }
        }
        public int MaximumAngle = 89;
        public int MinimumAngle = 0;
    }
    public class ManicDiggerGameWindow : GameWindow, IGameExit, ILocalPlayerPosition, IMap, IThe3d, IGui
    {
        [Inject]
        public ClientGame clientgame { get; set; }
        [Inject]
        public IClientNetwork network { get; set; }
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public IInternetGameFactory internetgamefactory { get; set; }

        [Inject]
        public IAudio audio { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public LoginClientMinecraft login { get; set; }
        [Inject]
        public Config3d config3d { get; set; }

        const float rotation_speed = 180.0f * 0.05f;
        //float angle;

        public void DrawMap()
        {
            terrain.UpdateAllTiles();
        }
        public void SetTileAndUpdate(Vector3 pos, byte type)
        {
            //            frametickmainthreadtodo.Add(() =>
            //        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int z = (int)pos.Z;
            clientgame.Map[x, y, z] = type;
            terrain.UpdateTile(x, y, z);
            //          });
        }
        const bool ENABLE_FULLSCREEN = false;
        public ManicDiggerGameWindow()
            : base(800, 600, GraphicsMode.Default, "",
                ENABLE_FULLSCREEN ? GameWindowFlags.Fullscreen : GameWindowFlags.Default) { }
        public int LoadTexture(string filename)
        {
            Bitmap bmp = new Bitmap(filename);
            return LoadTexture(bmp);
        }
        //http://www.opentk.com/doc/graphics/textures/loading
        public int LoadTexture(Bitmap bmp)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            if (!config3d.ENABLE_MIPMAPS)
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
            if (config3d.ENABLE_TRANSPARENCY)
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
        [Inject]
        public MapManipulator mapManipulator { get; set; }
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
            if (!config3d.ENABLE_VSYNC)
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
            mapManipulator.LoadMap(clientgame,"menu" + MapManipulator.XmlSaveExtension);
            ENABLE_FREEMOVE = true;
            player.playerposition = new Vector3(4.691565f, 45.2253f, 2.52523f);
            player.playerorientation = new Vector3(3.897586f, 2.385999f, 0f);
            DrawMap();
            Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            if (config3d.ENABLE_BACKFACECULLING)
            {
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
            }
            Keyboard.KeyRepeat = true;
            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            MaterialSlots = data.DefaultMaterialSlots;
            terrain.Start();
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
        //ISoundPlayer soundplayer = new SoundPlayerDummy();
        void ClientCommand(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("."))
            {
                try
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
                        string filename = arguments;
                        //if no extension given, then add default
                        if (filename.IndexOf(".") == -1)
                        {
                            filename += MapManipulator.XmlSaveExtension;
                        }
                        mapManipulator.LoadMap(clientgame, filename);
                        terrain.UpdateAllTiles();
                    }
                    else if (cmd == "save")
                    {
                        if (arguments == "")
                        {
                            AddChatline("error: missing arg1 - savename");
                            return;
                        }
                        mapManipulator.SaveMap(clientgame, arguments + MapManipulator.XmlSaveExtension);

                    }
                    else if (cmd == "fps")
                    {
                        ENABLE_DRAWFPS = (arguments == "" || arguments == "1" || arguments == "on");
                    }
                    else if (cmd == "uploadmap")
                    {
                        //load map from disk
                        MapStorage m = new MapStorage();
                        mapManipulator.LoadMap(m, arguments);
                        //add build commands to queue
                        for (int z = m.MapSizeZ - 1; z >= 0; z--)
                        {
                            for (int x = 0; x < m.MapSizeX; x++)
                            {
                                for (int y = 0; y < m.MapSizeY; y++)
                                {
                                    if (!MapUtil.IsValidPos(clientgame, x, y, z))
                                    {
                                        continue;
                                    }
                                    byte oldtile = clientgame.Map[x, y, z];
                                    byte newtile = m.Map[x, y, z];
                                    if (data.IsWaterTile(newtile) || data.IsWaterTile(newtile))
                                    {
                                        continue;
                                    }
                                    if (oldtile != newtile)
                                    {
                                        int xx = x;
                                        int yy = y;
                                        int zz = z;
                                        todo.Enqueue(() =>
                                            {
                                                player.playerposition = new Vector3(xx, zz + 1, yy);
                                                if (oldtile == data.TileIdEmpty)
                                                {
                                                    network.SendSetBlock(new Vector3(xx, yy, zz), BlockSetMode.Destroy, newtile);
                                                }
                                                if (newtile != data.TileIdEmpty)
                                                {
                                                    network.SendSetBlock(new Vector3(xx, yy, zz), BlockSetMode.Create, newtile);
                                                }
                                            });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        network.SendChat(GuiTypingBuffer);
                    }
                }
                catch (Exception e) { AddChatline(new StringReader(e.ToString()).ReadLine()); }
            }
            else
            {
                network.SendChat(GuiTypingBuffer);
            }
        }
        Queue<MethodInvoker> todo = new Queue<MethodInvoker>();
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
                if (e.Key == OpenTK.Input.Key.Escape)
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
                        typinglog.Add(GuiTypingBuffer);
                        typinglogpos = typinglog.Count;
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
                if (key == OpenTK.Input.Key.Up)
                {
                    typinglogpos--;
                    if (typinglogpos < 0) { typinglogpos = 0; }
                    if (typinglogpos >= 0 && typinglogpos < typinglog.Count)
                    {
                        GuiTypingBuffer = typinglog[typinglogpos];
                    }
                }
                if (key == OpenTK.Input.Key.Down)
                {
                    typinglogpos++;
                    if (typinglogpos > typinglog.Count) { typinglogpos = typinglog.Count; }
                    if (typinglogpos >= 0 && typinglogpos < typinglog.Count)
                    {
                        GuiTypingBuffer = typinglog[typinglogpos];
                    }
                    if (typinglogpos == typinglog.Count)
                    {
                        GuiTypingBuffer = "";
                    }
                }
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
                mapManipulator.SaveMap(clientgame, mapManipulator.defaultminesave);
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
        List<string> typinglog = new List<string>();
        int typinglogpos = 0;
        private void GuiActionGoToEscapeMenu()
        {
            guistate = GuiState.EscapeMenu;
            menustate = new MenuState();
            FreeMouse = true;
        }
        private void GuiActionLoadGame()
        {
            mapManipulator.LoadMap(clientgame, mapManipulator.defaultminesave);
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
            mapManipulator.GeneratePlainMap(clientgame);
            player.playerposition = playerpositionspawn;
            DrawMap();
        }
        bool freemousejustdisabled;
        enum TypingState { None, Typing, Ready };
        TypingState GuiTyping = TypingState.None;
        string GuiTypingBuffer = "";
        IClientNetwork newnetwork;
        ITerrainDrawer newterrain;
        ClientGame newclientgame;

        string username = "gamer1";
        string pass = "12345";
        string testgameurl
        {
            get
            {
                return File.ReadAllText("defaultserver.txt");
            }
        }
        private void DownloadInternetGame(string qusername, string qpass, string qgameurl)
        {
            var oldclientgame = clientgame;
            var oldnetwork = network;
            internetgamefactory.NewInternetGame();
            newclientgame = internetgamefactory.GetClientGame();
            newnetwork = internetgamefactory.GetNetwork();
            newterrain = internetgamefactory.GetTerrain();
            newterrain.Start();

            oldclientgame.Dispose();
            newnetwork.MapLoaded += new EventHandler<MapLoadedEventArgs>(network_MapLoaded);

            oldnetwork.Dispose();

            new MethodInvoker(() =>
            {
                //game url can be
                //a) minecraft.net url
                //if (qgameurl.Contains("minecraft.net"))
                //{
                //}
                //b) just hash
                //c) ip:port (server must have disabled authorization checking).
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
                this.terrain = newterrain;
                newnetwork = null; newclientgame = null; newterrain = null;
                var ee = (MapLoadedEventArgs)e;
                //lock (clientgame.mapupdate)
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
        float zfar { get { return ENABLE_ZFAR ? config3d.viewdistance * 3f / 4 : 99999; } }
        //int z = 0;
        Vector3 up = new Vector3(0f, 1f, 0f);
        Point mouse_current, mouse_previous;
        Point mouse_delta;
        bool freemouse;
        bool FreeMouse { get { if (overheadcamera) { return true; } return freemouse; } set { freemouse = value; } }
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
        DateTime lasttodo;
        void FrameTick(FrameEventArgs e)
        {
            if ((DateTime.Now - lasttodo).TotalSeconds > BuildDelay && todo.Count > 0)
            {
                lasttodo = DateTime.Now;
                var task = todo.Dequeue();
                task();
            }
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
                    if (overheadcamera)
                    {
                        CameraMove m = new CameraMove();
                        if (Keyboard[OpenTK.Input.Key.Q]) { overheadcameraK.TurnRight((float)e.Time * 5); }
                        if (Keyboard[OpenTK.Input.Key.E]) { overheadcameraK.TurnLeft((float)e.Time * 5); }
                        overheadcameraK.Center = player.playerposition;
                        m.Distance = -Mouse.WheelPrecise;
                        overheadcameraK.Move(m, (float)e.Time);
                        if ((player.playerposition - playerdestination).Length >= 0.5f)
                        {
                            movedy += 1;
                            //player orientation
                            //player.playerorientation.Y=
                            Vector3 q = playerdestination - player.playerposition;
                            q.Y = player.playerposition.Y;
                            player.playerorientation.Y = (float)Math.PI + Vector3.CalculateAngle(new Vector3(1, 0, 0), q);
                        }
                    }
                    else
                    {
                        if (Keyboard[OpenTK.Input.Key.W]) { movedy += 1; }
                        if (Keyboard[OpenTK.Input.Key.S]) { movedy += -1; }
                        if (Keyboard[OpenTK.Input.Key.A]) { movedx += -1; }
                        if (Keyboard[OpenTK.Input.Key.D]) { movedx += 1; }
                    }
                }
                if (ENABLE_FREEMOVE || Swimming)
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

            if (!(ENABLE_FREEMOVE || Swimming))
            {
                player.movedz += -gravity;//gravity
            }
            Vector3 newposition = player.playerposition + toVectorInFixedSystem1
                (movedx * movespeed * (float)e.Time,
                0,
                movedy * movespeed * (float)e.Time);
            if (!(ENABLE_FREEMOVE))
            {
                if (!Swimming)
                {
                    newposition.Y = player.playerposition.Y;
                }
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
                clientgame.p.swimmingtop = Keyboard[OpenTK.Input.Key.Space];
                player.playerposition = clientgame.p.WallSlide(player.playerposition, newposition);
            }
            else
            {
                player.playerposition = newposition;
            }
            bool isplayeronground;
            if (!(ENABLE_FREEMOVE || Swimming))
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
                }
            }
            else
            {
                isplayeronground = true;
            }
            if (isplayeronground)
            {
                player.movedz = Math.Max(0, player.movedz);
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
                player.playerorientation.X = Clamp(player.playerorientation.X, (float)Math.PI / 2 + 0.001f, (float)(Math.PI / 2 + Math.PI - 0.001f));
            }
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
            return clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Empty
                || data.IsWaterTile(clientgame.Map[x, y, z]);
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
            s.StartBox = new Box3D(0, 0, 0, NextPowerOfTwo((uint)Math.Max(clientgame.MapSizeX, Math.Max(clientgame.MapSizeY, clientgame.MapSizeZ))));
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
            if (FreeMouse)
            {
                if (pick2.Count > 0)
                {
                    OnPick(pick0);
                }
                return;
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
                        if (MapUtil.IsValidPos(clientgame, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
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
                        if (MapUtil.IsValidPos(clientgame, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            Console.WriteLine(". newtile:" + newtile + " type: " + clientgame.Map[(int)newtile.X, (int)newtile.Z, (int)newtile.Y]);
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
        private uint NextPowerOfTwo(uint x)
        {
            x--;
            x |= x >> 1;  // handle  2 bit numbers
            x |= x >> 2;  // handle  4 bit numbers
            x |= x >> 4;  // handle  8 bit numbers
            x |= x >> 8;  // handle 16 bit numbers
            x |= x >> 16; // handle 32 bit numbers
            x++;
            return x;
        }
        private void OnPick(TilePosSide pick0)
        {
            playerdestination = pick0.pos;
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

            GL.BindTexture(TextureTarget.Texture2D, terrain.terrainTexture);

            GL.MatrixMode(MatrixMode.Modelview);

            Matrix4 camera;
            if (overheadcamera)
            {
                camera = OverheadCamera();
            }
            else
                camera = FppCamera();
            GL.LoadMatrix(ref camera);
            terrain.Draw();
            DrawImmediateParticleEffects(e.Time);
            DrawCubeLines(pickcubepos);

            DrawVehicles();
            DrawPlayers((float)e.Time);
            Draw2d();

            //OnResize(new EventArgs());
            SwapBuffers();
        }
        public class PlayerInterpolated
        {
            public Vector3 LastRealPosition;
            public DateTime LastRealPositionTime;
            public Vector3 InterpolatedPosition;
            public Vector3 Direction;
        }
        public Dictionary<int, PlayerInterpolated> PlayerPositionsInterpolated = new Dictionary<int, PlayerInterpolated>();
        private void DrawPlayers(float dt)
        {
            foreach (var k in clientgame.Players)
            {
                if (!PlayerPositionsInterpolated.ContainsKey(k.Key))
                {
                    PlayerPositionsInterpolated[k.Key] = new PlayerInterpolated();
                }
                var realposition = k.Value.Position;
                var pi = PlayerPositionsInterpolated[k.Key];
                if (realposition != pi.LastRealPosition)
                {
                    pi.Direction = Vector3.Multiply(k.Value.Position - pi.LastRealPosition,
                        (float)(DateTime.Now - pi.LastRealPositionTime).TotalSeconds);
                    pi.LastRealPosition = realposition;
                    pi.LastRealPositionTime = DateTime.Now;
                }
                var curpos = pi.LastRealPosition + pi.Direction * (float)(DateTime.Now - pi.LastRealPositionTime).TotalSeconds;
                DrawCube(realposition);//curpos
            }
        }
        bool overheadcamera = false;
        Kamera overheadcameraK = new Kamera();
        Matrix4 FppCamera()
        {
            Vector3 forward = toVectorInFixedSystem1(0, 0, 1);
            return Matrix4.LookAt(player.playerposition + new Vector3(0, characterheight, 0),
                player.playerposition + new Vector3(0, characterheight, 0) + forward, up);
        }
        //Vector3 overheadCameraPosition = new Vector3(5, 32 + 20, 5);
        //Vector3 overheadCameraDestination = new Vector3(5, 32, 0);
        Matrix4 OverheadCamera()
        {
            //return Matrix4.LookAt(overheadCameraPosition, overheadCameraDestination, up);
            return Matrix4.LookAt(overheadcameraK.Position, overheadcameraK.Center, up);
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
            mapManipulator.SaveMap(clientgame, mapManipulator.defaultminesave);
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
                //DrawMouseCursor();
            }
        }
        bool SaveGameExists()
        {
            return File.Exists(mapManipulator.defaultminesave);
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
                //DrawMouseCursor();
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
                Draw2dText(fpstext, 20f, 20f, 14, Color.White);
            }
            if (FreeMouse)
            {
                DrawMouseCursor();
            }
            PerspectiveMode();
        }
        private void DrawMaterialSelector()
        {
            int singlesize = 40;
            for (int i = 0; i < 10; i++)
            {
                Draw2dTexture(terrain.terrainTexture, xcenter(singlesize * 10) + i * singlesize, Height - 100, singlesize, singlesize,
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
                rect = TextureAtlas.TextureCoords(inAtlasId.Value, terrain.texturesPacked);
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
        private void DrawImmediateParticleEffects(double deltaTime)
        {
            GL.BindTexture(TextureTarget.Texture2D, terrain.terrainTexture);
            foreach (ParticleEffect p in new List<ParticleEffect>(particleEffects))
            {
                foreach (Particle pp in p.particles)
                {
                    GL.Begin(BeginMode.Triangles);
                    RectangleF texrec = TextureAtlas.TextureCoords(p.textureid, terrain.texturesPacked);
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
                int totaltriangles = terrain.TrianglesCount();
                title += ", triangles: " + totaltriangles;
                //Title = title;
                Title = applicationname;
                fpstext = title;
            }
        }
        string applicationname = "Manic Digger";
        #region ILocalPlayerPosition Members
        public Vector3 LocalPlayerPosition { get { return player.playerposition; } set { player.playerposition = value; } }
        public Vector3 LocalPlayerOrientation { get { return player.playerorientation; } set { player.playerorientation = value; } }
        #endregion
        public void AddChatline(string s)
        {
            chatlines.Add(new Chatline() { text = s, time = DateTime.Now });
        }
        #region ILocalPlayerPosition Members
        public bool Swimming
        {
            get
            {
                var p = LocalPlayerPosition;
                p += new Vector3(0, CharacterPhysics.characterheight, 0);
                if (!MapUtil.IsValidPos(clientgame, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y)))
                {
                    return p.Y < clientgame.WaterLevel;
                }
                return data.IsWaterTile(clientgame.Map[(int)p.X, (int)p.Z, (int)p.Y]);
            }
        }
        #endregion
    }
}

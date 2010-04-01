using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ManicDiggerGameWindow : GameWindow, IGameExit, ILocalPlayerPosition, IGui, IMap
    {
        [Inject]
        public ClientGame clientgame { get; set; }
        [Inject]
        public IClientNetwork network { get; set; }
        [Inject]
        public IAudio audio { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public LoginClientMinecraft login { get; set; }

        bool ENABLE_BACKFACECULLING = true;
        bool ENABLE_TRANSPARENCY = true;
        bool ENABLE_MIPMAPS = true;
        bool ENABLE_VSYNC = false;

        const float rotation_speed = 180.0f * 0.05f;
        //float angle;

        struct Vbo
        {
            public int VboID, EboID, NumElements;
            public Box3D box;
        }
        //List<Vbo> vbo = new List<Vbo>();
        Dictionary<Vector3, ICollection<Vbo>> vbo = new Dictionary<Vector3, ICollection<Vbo>>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VertexPositionTexture
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

        VertexPositionTexture[] CubeVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture( 0.0f,  1.0f,  0.0f, 0, 0),
            new VertexPositionTexture( 0.0f,  1.0f,  1.0f, 0, 1),
            new VertexPositionTexture( 1.0f,  1.0f,  0.0f, 1, 0),
            new VertexPositionTexture( 1.0f,  1.0f,  1.0f, 1, 1),
        };

        short[] CubeElements = new short[]
        {
            0, 1, 2, 2, 3, 0, // front face
            3, 2, 6, 6, 7, 3, // top face
            7, 6, 5, 5, 4, 7, // back face
            4, 0, 3, 3, 7, 4, // left face
            0, 1, 5, 5, 4, 0, // bottom face
            1, 5, 6, 6, 2, 1, // right face
        };
        const bool ENABLE_FULLSCREEN = false;
        public ManicDiggerGameWindow()
            : base(800, 600, GraphicsMode.Default, "",
                ENABLE_FULLSCREEN ? GameWindowFlags.Fullscreen : GameWindowFlags.Default) { }
        int LoadTexture(string filename)
        {
            Bitmap bmp = new Bitmap(filename);
            return LoadTexture(bmp);
        }
        //http://www.opentk.com/doc/graphics/textures/loading
        int LoadTexture(Bitmap bmp)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            if (!ENABLE_MIPMAPS)
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
            if (ENABLE_TRANSPARENCY)
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
            if (!ENABLE_VSYNC)
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
            clientgame.LoadMap("menu" + ClientGame.XmlSaveExtension);
            ENABLE_FREEMOVE = true;
            player.playerposition = new Vector3(4.691565f, 45.2253f, 2.52523f);
            player.playerorientation = new Vector3(3.897586f, 2.385999f, 0f);
            DrawMap();
            GL.Enable(EnableCap.Texture2D);
            terrainTexture = LoadTexture(getfile.GetFile("terrain.png"));
            Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            if (ENABLE_BACKFACECULLING)
            {
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
            }
            Keyboard.KeyRepeat = true;
            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            MaterialSlots = data.DefaultMaterialSlots;
            new Thread(bgworker).Start();
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
        bool exitbgworker = false;
        //ISoundPlayer soundplayer = new SoundPlayerDummy();
        /// <summary>
        /// Background thread generating vertices and indices.
        /// Actual vbo loading must be done in the main thread (it is fast).
        /// </summary>
        void bgworker()
        {
            for (; ; )
            {
                if (exit || exitbgworker)
                {
                    return;
                }
                Vector3? pp = null;
                lock (toupdate)
                {
                    if (toupdate.Count > 0)
                    {
                        pp = toupdate.Dequeue();
                    }
                }
                if (pp != null)
                {
                    Vector3 p = pp.Value;
                    //lock (clientgame.mapupdate)//does not work, clientgame can get replaced
                    {
                        //try
                        {
                            IEnumerable<VerticesIndicesToLoad> q = MakeChunk((int)p.X * buffersize, (int)p.Y * buffersize, (int)p.Z * buffersize, buffersize);
                            List<Vector3> toremove = new List<Vector3>();
                            if (q != null)
                            {
                                lock (vbotoload)
                                {
                                    //foreach (var qq in q)
                                    {
                                        vbotoload.Enqueue(new List<VerticesIndicesToLoad>(q.ToArray()));
                                    }
                                }
                            }
                        }
                        //catch
                        //{ }
                    }
                }
                Thread.Sleep(0);
            }
        }
        Queue<Vector3> toupdate = new Queue<Vector3>();
        public void DrawMap()
        {
            lock (toupdate)
            {
                toupdate.Clear();
            }
            lock (vbotoload)
            {
                vbotoload.Clear();
            }
            foreach (var v in vbo)
            {
                foreach (var vv in v.Value)
                {
                    var a = vv.VboID;
                    var b = vv.EboID;
                    GL.DeleteBuffers(1, ref a);
                    GL.DeleteBuffers(1, ref b);
                }
            }
            vbo.Clear();
            for (int i = 0; i < 1; i++)
                for (int x = 0; x < clientgame.MapSizeX / buffersize; x++)
                    for (int y = 0; y < clientgame.MapSizeY / buffersize; y++)
                        for (int z = 0; z < clientgame.MapSizeZ / buffersize; z++)//bbb mapsizez / buffersize
                            //DrawUpdateChunk(x, y, z);
                            lock (toupdate)
                            {
                                toupdate.Enqueue(new Vector3(x, y, z));
                            }
        }
        int buffersize = 32; //32,45
        public void UpdateTileSet(Vector3 pos, byte type)
        {
            //            frametickmainthreadtodo.Add(() =>
            //        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int z = (int)pos.Z;
            clientgame.Map[x, y, z] = type;
            UpdateTile(x, y, z);
            //          });
        }
        private void UpdateTile(int x, int y, int z)
        {
            Vector3 bufferpos = new Vector3(x / buffersize, y / buffersize, z / buffersize);
            lock (toupdate)
            {
                //if we are on a chunk boundary, then update near chunks too.
                if (x % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(-1, 0, 0));
                }
                if (x % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(1, 0, 0));
                }
                if (y % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, -1, 0));
                }
                if (y % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 1, 0));
                }
                if (z % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 0, -1));
                }
                if (z % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 0, 1));
                }
                toupdate.Enqueue(bufferpos);///bbb z / buffersize
            }
        }
        void ClientCommand(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("/"))
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
                    try
                    {
                        string filename = arguments;
                        //if no extension given, then add default
                        if (filename.IndexOf(".") == -1)
                        {
                            filename += ClientGame.XmlSaveExtension;
                        }
                        clientgame.LoadMap(filename);
                    }
                    catch (Exception e) { AddChatline(new StringReader(e.ToString()).ReadLine()); }
                }
                else if (cmd == "save")
                {
                    if (arguments == "")
                    {
                        AddChatline("error: missing arg1 - savename");
                        return;
                    }
                    try
                    {
                        clientgame.SaveMap(arguments + ClientGame.XmlSaveExtension);
                    }
                    catch (Exception e) { AddChatline(new StringReader(e.ToString()).ReadLine()); }
                }
                else if (cmd == "fps")
                {
                    ENABLE_DRAWFPS = (arguments == "" || arguments == "1" || arguments == "on");
                }
                else
                {
                    network.SendChat(GuiTypingBuffer);
                }
            }
            else
            {
                network.SendChat(GuiTypingBuffer);
            }
        }
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
                clientgame.SaveMap(clientgame.defaultminesave);
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
            clientgame.LoadMap(clientgame.defaultminesave);
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
            clientgame.GeneratePlainMap();
            player.playerposition = playerpositionspawn;
            DrawMap();
        }
        bool freemousejustdisabled;
        enum TypingState { None, Typing, Ready };
        TypingState GuiTyping = TypingState.None;
        string GuiTypingBuffer = "";
        IClientNetwork newnetwork;
        ClientGame newclientgame;

        string username = "gamer1";
        string pass = "12345";
        string testgameurl
        {
            get
            {
                return File.ReadAllText("defaultserver.cfg");
            }
        }
        [Inject]
        public IInternetGameFactory internetgamefactory { get; set; }
        private void DownloadInternetGame(string qusername, string qpass, string qgameurl)
        {
            var oldclientgame = clientgame;
            var oldnetwork = network;
            internetgamefactory.NewInternetGame();
            newclientgame = internetgamefactory.GetClientGame();
            newnetwork = internetgamefactory.GetNetwork();

            oldclientgame.Dispose();
            newnetwork.MapLoaded += new EventHandler<MapLoadedEventArgs>(network_MapLoaded);

            oldnetwork.Dispose();

            new MethodInvoker(() =>
            {
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
                newnetwork = null; newclientgame = null;
                var ee = (MapLoadedEventArgs)e;
                lock (clientgame.mapupdate)
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
        //warning! buffer zone!
        RectangleF TextureCoords(int textureId, int texturesPacked)
        {
            float bufferRatio = 0.0f;//0.1
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPacked * (int)(textureId / texturesPacked)) + ((bufferRatio) * (1.0f / texturesPacked));
            r.X = (1.0f / texturesPacked * (textureId % texturesPacked)) + ((bufferRatio) * (1.0f / texturesPacked));
            r.Width = (1f - 2f * bufferRatio) * 1.0f / texturesPacked;
            r.Height = (1f - 2f * bufferRatio) * 1.0f / texturesPacked;
            return r;
        }
        bool IsTileEmptyForDrawing(int x, int y, int z)
        {
            if (!IsValidPos(x, y, z))
            {
                return true;
            }
            return clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Empty;
        }
        bool IsTileEmptyForDrawingOrTransparent(int x, int y, int z, int adjacenttiletype)
        {
            if (!ENABLE_TRANSPARENCY)
            {
                return IsTileEmptyForDrawing(x, y, z);
            }
            if (!IsValidPos(x, y, z))
            {
                return true;
            }
            return clientgame.Map[x, y, z] == data.TileIdEmpty
                || (clientgame.Map[x, y, z] == data.TileIdWater
                 && !(adjacenttiletype == data.TileIdWater))
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Glass
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.InfiniteWaterSource
                || clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Leaves;
        }
        int texturesPacked = 16;//16x16
        bool DONOTDRAWEDGES = true;
        List<VerticesIndicesToLoad> MakeChunk(int startx, int starty, int startz, int size)
        {
            List<VerticesIndicesToLoad> list = new List<VerticesIndicesToLoad>();
            List<ushort> myelements = new List<ushort>();
            List<VertexPositionTexture> myvertices = new List<VertexPositionTexture>();
            for (int x = startx; x < startx + size; x++)
                for (int y = starty; y < starty + size; y++)
                    for (int z = startz; z < startz + size; z++)//bbb startz+size
                    {
                        //if (x == 0 && z == 31 & y == 128)
                        {
                        }
                        if (IsTileEmptyForDrawing(x, y, z)) { continue; }
                        var tt = clientgame.Map[x, y, z];
                        bool drawtop = IsTileEmptyForDrawingOrTransparent(x, y, z + 1, tt);
                        bool drawbottom = IsTileEmptyForDrawingOrTransparent(x, y, z - 1, tt);
                        bool drawfront = IsTileEmptyForDrawingOrTransparent(x - 1, y, z, tt);
                        bool drawback = IsTileEmptyForDrawingOrTransparent(x + 1, y, z, tt);
                        bool drawleft = IsTileEmptyForDrawingOrTransparent(x, y - 1, z, tt);
                        bool drawright = IsTileEmptyForDrawingOrTransparent(x, y + 1, z, tt);
                        if (x == 0)
                        {
                            if (tt == data.TileIdWater) { Console.WriteLine(new Vector3(x, y, z)); }
                        }
                        if (DONOTDRAWEDGES)
                        {
                            //if the game is fillrate limited, then this makes it much faster.
                            //(39fps vs vsync 75fps)
                            //bbb.
                            if (z == 0) { drawbottom = false; }
                            if (x == 0) { drawfront = false; }
                            if (x == 256 - 1) { drawback = false; }
                            if (y == 0) { drawleft = false; }
                            if (y == 256 - 1) { drawright = false; }
                        }
                        //top
                        if (drawtop)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Top);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 0.0f, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 1.0f, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 0.0f, texrec.Right, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 1.0f, texrec.Right, texrec.Bottom));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //bottom - same as top, but z is 1 less.
                        if (drawbottom)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Bottom);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 0.0f, texrec.Right, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 1.0f, texrec.Right, texrec.Bottom));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //front
                        if (drawfront)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Front);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //back - same as front, but x is 1 greater.
                        if (drawback)
                        {//todo fix tcoords
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Back);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        if (drawleft)
                        {
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Left);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //right - same as left, but y is 1 greater.
                        if (drawright)
                        {//todo fix tcoords
                            int sidetexture = data.GetTileTextureId(clientgame.Map[x, y, z], TileSide.Right);
                            RectangleF texrec = TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        if (myvertices.Count > ushort.MaxValue)
                        {
                            var aa = myelements.ToArray();
                            var bb = myvertices.ToArray();
                            list.Add(new VerticesIndicesToLoad()
                            {
                                position = new Vector3(startx / size, starty / size, startz / size),
                                indices = aa,
                                vertices = bb,
                            });
                            myelements = new List<ushort>();
                            myvertices = new List<VertexPositionTexture>();
                        }
                    }
            if (myelements.Count != 0)
            {
                var a = myelements.ToArray();
                var b = myvertices.ToArray();
                list.Add(new VerticesIndicesToLoad()
                {
                    position = new Vector3(startx / size, starty / size, startz / size),
                    indices = a,
                    vertices = b,
                });
            }
            return list;
        }
        int terrainTexture;
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
        float zfar { get { return ENABLE_ZFAR ? viewdistance * 3f / 4 : 99999; } }
        //int z = 0;
        Vector3 up = new Vector3(0f, 1f, 0f);
        Point mouse_current, mouse_previous;
        Point mouse_delta;
        bool FreeMouse = false;
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
        class VerticesIndicesToLoad
        {
            public VertexPositionTexture[] vertices;
            public ushort[] indices;
            public Vector3 position;
        }
        Queue<List<VerticesIndicesToLoad>> vbotoload = new Queue<List<VerticesIndicesToLoad>>();
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
        void FrameTick(FrameEventArgs e)
        {
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
                    if (Keyboard[OpenTK.Input.Key.W]) { movedy += 1; }
                    if (Keyboard[OpenTK.Input.Key.S]) { movedy += -1; }
                    if (Keyboard[OpenTK.Input.Key.A]) { movedx += -1; }
                    if (Keyboard[OpenTK.Input.Key.D]) { movedx += 1; }
                }
                if (ENABLE_FREEMOVE)
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

            if (!ENABLE_FREEMOVE)
            {
                player.movedz += -gravity;//gravity
            }
            Vector3 newposition = player.playerposition + toVectorInFixedSystem1
                (movedx * movespeed * (float)e.Time,
                0,
                movedy * movespeed * (float)e.Time);
            if (!ENABLE_FREEMOVE)
            {
                newposition.Y = player.playerposition.Y;
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
                player.playerposition = clientgame.p.WallSlide(player.playerposition, newposition);
            }
            else
            {
                player.playerposition = newposition;
            }
            bool isplayeronground;
            if (!ENABLE_FREEMOVE)
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
                    if (isplayeronground)
                    {
                        player.movedz = Math.Max(0, player.movedz);
                    }
                }
            }
            else
            {
                isplayeronground = true;
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
        class MenuState
        {
            public int selected = 0;
        }
        MenuState menustate = new MenuState();
        private void UpdateMouseViewportControl(FrameEventArgs e)
        {
            player.playerorientation.Y += (float)mouse_delta.X * rotationspeed * (float)e.Time;
            player.playerorientation.X += (float)mouse_delta.Y * rotationspeed * (float)e.Time;
            player.playerorientation.X = Clamp(player.playerorientation.X, (float)Math.PI / 2 + 0.001f, (float)(Math.PI / 2 + Math.PI - 0.001f));
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
            return clientgame.Map[x, y, z] == (byte)TileTypeMinecraft.Empty;
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
            s.StartBox = new Box3D(0, 0, 0, 256);
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
                        if (IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
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
                        if (IsValidPos((int)newtile.X, (int)newtile.Z, (int)newtile.Y))
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
        float viewdistance = 256;
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

            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);

            GL.MatrixMode(MatrixMode.Modelview);
            Vector3 forward = toVectorInFixedSystem1(0, 0, 1);
            Matrix4 camera = Matrix4.LookAt(player.playerposition + new Vector3(0, characterheight, 0),
                player.playerposition + new Vector3(0, characterheight, 0) + forward, up);
            GL.LoadMatrix(ref camera);
            chunkupdateframecounter += vboupdatesperframe;
            while (chunkupdateframecounter >= 1)
            {
                chunkupdateframecounter -= 1;
                IEnumerable<VerticesIndicesToLoad> v = null;
                lock (vbotoload)
                {
                    if (vbotoload.Count > 0)
                    {
                        v = vbotoload.Dequeue();
                    }
                }
                if (v != null && v.Any())
                {
                    List<Vbo> vbolist = new List<Vbo>();
                    foreach (var vv in v)
                    {
                        var vbo1 = LoadVBO(vv.vertices, vv.indices);
                        foreach (var vvv in vv.vertices)
                        {
                            vbo1.box.AddPoint(vvv.Position.X, vvv.Position.Y, vvv.Position.Z);
                        }
                        vbolist.Add(vbo1);
                    }
                    if (!vbo.ContainsKey(v.First().position))
                    {
                        vbo[v.First().position] = new List<Vbo>();
                    }
                    //delete old vbo
                    vbo[v.First().position] = vbolist;
                    //DrawUpdateChunk(((int)v.X), ((int)v.Y), ((int)v.Z));
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
            var z = new List<Vbo>(VisibleVbo());
            if (z.Count != lastvisiblevbo && vbotoload.Count == 0)
            {
                Console.WriteLine("Hardware buffers: " + z.Count);
                lastvisiblevbo = z.Count;
            }
            z.Sort(f);
            foreach (var k in z)
            {
                Draw(k);
            }
            DrawImmediateParticleEffects(e.Time);
            DrawCubeLines(pickcubepos);

            DrawVehicles();

            Draw2d();

            //OnResize(new EventArgs());
            SwapBuffers();
        }
        private void DeleteVbo(Vbo pp)
        {
        }
        int lastvisiblevbo = 0;
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
        int f(Vbo a, Vbo b)
        {
            var aa = (a.box.Center() - player.playerposition).Length;
            var bb = (b.box.Center() - player.playerposition).Length;
            return aa.CompareTo(bb);
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
            clientgame.SaveMap(clientgame.defaultminesave);
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
                DrawMouseCursor();
            }
        }
        bool SaveGameExists()
        {
            return File.Exists(clientgame.defaultminesave);
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
                DrawMouseCursor();
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
            PerspectiveMode();
        }
        private void DrawMaterialSelector()
        {
            int singlesize = 40;
            for (int i = 0; i < 10; i++)
            {
                Draw2dTexture(terrainTexture, xcenter(singlesize * 10) + i * singlesize, Height - 100, singlesize, singlesize,
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
                rect = TextureCoords(inAtlasId.Value, texturesPacked);
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
        float chunkupdateframecounter = 0;
        float vboupdatesperframe = 0.5f;
        private void DrawImmediateParticleEffects(double deltaTime)
        {
            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
            foreach (ParticleEffect p in new List<ParticleEffect>(particleEffects))
            {
                foreach (Particle pp in p.particles)
                {
                    GL.Begin(BeginMode.Triangles);
                    RectangleF texrec = TextureCoords(p.textureid, texturesPacked);
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
        private bool IsValidPos(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= clientgame.MapSizeX || y >= clientgame.MapSizeY || z >= clientgame.MapSizeZ)
            {
                return false;
            }
            return true;
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
                int totaltriangles = 0;
                foreach (var k in VisibleVbo())
                {
                    totaltriangles += k.NumElements / 3;
                }
                title += ", triangles: " + totaltriangles;
                //Title = title;
                Title = applicationname;
                fpstext = title;
            }
        }
        string applicationname = "Manic Digger";
        bool ENABLE_VISIBILITY_CULLING = false;
        private IEnumerable<Vbo> VisibleVbo()
        {
            foreach (var k in vbo)
            {
                foreach (var kk in k.Value)
                {
                    if (!ENABLE_VISIBILITY_CULLING || (kk.box.Center() - player.playerposition).Length < viewdistance)
                    {
                        yield return kk;
                    }
                }
            }
        }
        int strideofvertices = -1;
        int StrideOfVertices
        {
            get
            {
                if (strideofvertices == -1) strideofvertices = BlittableValueType.StrideOf(CubeVertices);
                return strideofvertices;
            }
        }
        Vbo LoadVBO<TVertex>(TVertex[] vertices, ushort[] elements) where TVertex : struct
        {
            Vbo handle = new Vbo();
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out handle.VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * StrideOfVertices), vertices,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * StrideOfVertices != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            GL.GenBuffers(1, out handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(ushort)), elements,//aaa sizeof(short)
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length * sizeof(ushort) != size)//aaa ushort
                throw new ApplicationException("Element data not uploaded correctly");

            handle.NumElements = elements.Length;
            return handle;
        }

        void Draw(Vbo handle)
        {
            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).

            //GL.EnableClientState(EnableCap.ColorArray);
            GL.EnableClientState(EnableCap.TextureCoordArray);
            GL.EnableClientState(EnableCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);

            GL.VertexPointer(3, VertexPointerType.Float, StrideOfVertices, new IntPtr(0));
            //GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVertices), new IntPtr(12));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, StrideOfVertices, new IntPtr(12));

            GL.DrawElements(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);//aaa
        }
        #region ILocalPlayerPosition Members
        public Vector3 LocalPlayerPosition { get { return player.playerposition; } set { player.playerposition = value; } }
        public Vector3 LocalPlayerOrientation { get { return player.playerorientation; } set { player.playerorientation = value; } }
        #endregion
        public void AddChatline(string s)
        {
            chatlines.Add(new Chatline() { text = s, time = DateTime.Now });
        }
    }
}

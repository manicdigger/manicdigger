using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
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
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public VertexPositionTexture(float x, float y, float z, float u, float v)
        {
            Position = new Vector3(x, y, z);
            this.u = u;
            this.v = v;
            r = byte.MaxValue;
            g = byte.MaxValue;
            b = byte.MaxValue;
            a = byte.MaxValue;
        }
        public VertexPositionTexture(float x, float y, float z, float u, float v, Color c)
        {
            Position = new Vector3(x, y, z);
            this.u = u;
            this.v = v;
            r = c.R;
            g = c.G;
            b = c.B;
            a = c.A;
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
        public bool transparent;
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
    public class The3dDummy : IThe3d
    {
        public int TextureId;
        #region IThe3d Members
        public int LoadTexture(string filename)
        {
            return TextureId;
        }
        public int LoadTerrainTexture(string filename)
        {
            return TextureId;
        }
        #endregion
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
    public class KameraDummy :IKamera
    {
        #region IKamera Members
        public void Move(CameraMove move, float p)
        {
        }
        public Vector3 Position { get; set; }
        #endregion
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
    public interface IInterpolation
    {
        object Interpolate(object a, object b, float progress);
    }
    public interface INetworkInterpolation
    {
        void AddNetworkPacket(object c, double time);
        object InterpolatedState(double time);
    }
    class NetworkInterpolation : INetworkInterpolation
    {
        struct Packet
        {
            public double timestamp;
            public object content;
        }
        public IInterpolation req { get; set; }
        public bool EXTRAPOLATE = false;
        public float DELAY = 0.2f;
        public float FRAMESIZE = 0.1f;
        List<Packet> received = new List<Packet>();
        public void AddNetworkPacket(object c, double time)
        {
            Packet p = new Packet();
            p.content = c;
            p.timestamp = time;
            received.Add(p);
            if (received.Count > 100)
            {
                received.RemoveRange(0, received.Count - 100);
            }
        }
        public object InterpolatedState(double time)
        {
            double curtime = time;
            double interpolationtime = curtime - DELAY;
            int p1;
            int p2;
            if (received.Count == 0)
            {
                return null;
            }
            object result;
            if (received.Count > 0 && interpolationtime < received[0].timestamp)
            {
                p1 = 0;
                p2 = 0;
            }
            //extrapolate
            else if (EXTRAPOLATE && (received.Count >= 2)
                && time > received[received.Count - 1].timestamp)
            {
                p1 = received.Count - 2;
                p2 = received.Count - 1;
            }
            else
            {
                //interpolate between packets with time a) time-delay, b) curtime-delay+framesize
                p1 = 0;
                for (int i = 0; i < received.Count; i++)
                {
                    if (received[i].timestamp <= interpolationtime)
                    {
                        p1 = i;
                    }
                }
                p2 = p1;
                for (int i = p1; i < received.Count; i++)
                {
                    if (received[i].timestamp <= interpolationtime + FRAMESIZE)
                    {
                        p2 = i;
                    }
                }
            }
            if (p1 == p2)
            {
                result = received[p1].content;
            }
            else
            {
                result = req.Interpolate(received[p1].content, received[p2].content,
                    (float)((interpolationtime - received[p1].timestamp)
                    / (received[p2].timestamp - received[p1].timestamp)));
            }
            return result;
        }
    }
    public enum Direction4
    {
        Left,
        Right,
        Up,
        Down,
    }
    public class The3d : IThe3d
    {
        public int LoadTexture(string filename)
        {
            throw new NotImplementedException();
        }
    }
    public interface IKeyboard
    {
        OpenTK.Input.KeyboardDevice keyboardstate { get; }
        OpenTK.Input.KeyboardKeyEventArgs keypressed { get; }
    }
    public interface IViewport3d : ILocalPlayerPosition, IKeyboard
    {
        int[] MaterialSlots { get; set; }
        int activematerial { get; set; }
        bool ENABLE_FREEMOVE { get; set; }
        bool ENABLE_MOVE { get; set; }
    }
    public interface IGameMode
    {
        void OnPick(Vector3 blockposnew, Vector3 blockposold, Vector3 pos3d, bool right);
        void SendSetBlock(Vector3 vector3, BlockSetMode blockSetMode, int type);
        void OnNewFrame(double dt);
        IEnumerable<ICharacterToDraw> Characters { get; }
        Vector3 PlayerPositionSpawn { get; }
        Vector3 PlayerOrientationSpawn { get; }
        void OnNewMap();
    }
    public interface ICharacterToDraw
    {
        Vector3 Pos3d { get; }
        Vector3 Dir3d { get; }
        bool Moves { get; }
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Requires OpenTK.
    /// </remarks>
    public class ManicDiggerGameWindow : GameWindow, IGameExit, ILocalPlayerPosition, IMap, IThe3d, IGui, IViewport3d
    {
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public IGameMode game { get; set; }
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public IClients clients { get; set; }
        [Inject]
        public CharacterPhysics physics { get; set; } 
        [Inject]
        public INetworkClient network { get; set; }
        [Inject]
        public IInternetGameFactory internetgamefactory { get; set; }

        [Inject]
        public IAudio audio { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public ILoginClient login { get; set; }
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
            map.Map[x, y, z] = type;
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
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }

            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            GL.Enable(EnableCap.DepthTest);
            /*
            if (config3d.ENABLE_TRANSPARENCY)
            {
                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
            }
            */
            
            if (config3d.ENABLE_TRANSPARENCY)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new Color4(0, 0, 0, byte.MaxValue));
            }
            
            return id;
        }
        PlayMp3 mp3 = new PlayMp3();
        protected override void OnFocusedChanged(EventArgs e)
        {
            if (guistate == GuiState.Normal)
            { GuiStateEscapeMenu(); }
            else if (guistate == GuiState.MainMenu || guistate == GuiState.EscapeMenu)
            { }
            else if (guistate == GuiState.Inventory)
            { }
            else if (guistate == GuiState.MapLoading)
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
            if (GameUrl == null)
            {
                guistate = GuiState.MainMenu;
                FreeMouse = true;
                mapManipulator.LoadMap(map, getfile.GetFile("menu" + MapManipulator.XmlSaveExtension));
                ENABLE_FREEMOVE = true;
                player.playerposition = new Vector3(4.691565f, 45.2253f, 2.52523f);
                player.playerorientation = new Vector3(3.897586f, 2.385999f, 0f);
                DrawMap();
                terrain.Start();
            }
            else
            {
                ClientCommand(".server " + GameUrl);
            }
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
            materialSlots = data.DefaultMaterialSlots;
            GL.Enable(EnableCap.Lighting);
            SetAmbientLight(terraincolor);
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
            GL.ShadeModel(ShadingModel.Smooth);
        }
        private static void SetAmbientLight(Color c)
        {
            float mult = 1f;
            float[] global_ambient = new float[]
            { (float)c.R / 255f * mult, (float)c.G / 255f * mult, (float)c.B / 255f * mult, 1f };
            GL.LightModel(LightModelParameter.LightModelAmbient, global_ambient);
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
                        if (server.Length == 32)
                        {
                            server = "http://www.minecraft.net/play.jsp?server=" + server;
                        }
                        ConnectToInternetGame(username, pass, server);
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
                        mapManipulator.LoadMap(map, filename);
                        terrain.UpdateAllTiles();
                    }
                    else if (cmd == "save")
                    {
                        if (arguments == "")
                        {
                            AddChatline("error: missing arg1 - savename");
                            return;
                        }
                        mapManipulator.SaveMap(map, arguments + MapManipulator.XmlSaveExtension);

                    }
                    else if (cmd == "fps")
                    {
                        ENABLE_DRAWFPS = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "uploadmap" || cmd == "uploadfeature")
                    {
                        //load map from disk
                        MapStorage m = new MapStorage();
                        mapManipulator.LoadMap(m, arguments);
                        //add build commands to queue
                        new Thread(() => { UploadMap(cmd == "uploadfeature", m); }).Start();
                    }
                    else if (cmd == "savefeature")
                    {
                        string[] ss1 = arguments.Split(new[] { ' ' });
                        int size = int.Parse(ss1[0]);
                        string filename = ss1[1];
                        MapStorage m = new MapStorage();
                        m.Map = new byte[size, size, size];
                        m.MapSizeX = size;
                        m.MapSizeY = size;
                        m.MapSizeZ = size;
                        for (int x = 0; x < size; x++)
                        {
                            for (int y = 0; y < size; y++)
                            {
                                for (int z = 0; z < size; z++)
                                {
                                    int xx = (int)player.playerposition.X + 1 + x;
                                    int yy = (int)player.playerposition.Z + 1 + y;
                                    int zz = (int)player.playerposition.Y + z;
                                    if (MapUtil.IsValidPos(map, xx, yy, zz)
                                        && MapUtil.IsValidPos(m, x, y, z))
                                    {
                                        m.Map[x, y, z] = map.Map[xx, yy, zz];
                                    }
                                }
                            }
                        }
                        if (!filename.Contains("."))
                        {
                            filename += MapManipulator.XmlSaveExtension;
                        }
                        mapManipulator.SaveMap(m, filename);
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
                            terrain.DrawDistance = foglevel2;
                            terrain.UpdateAllTiles();
                        }
                    }
                    else if (cmd == "noclip")
                    {
                        ENABLE_NOCLIP = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "freemove")
                    {
                        ENABLE_FREEMOVE = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "fov")
                    {
                        int arg=int.Parse(arguments);
                        int minfov = 1;
                        int maxfov = 179;
                        if (arg < minfov || arg > maxfov)
                        {
                            throw new Exception(string.Format("Valid field of view: {0}-{1}", minfov, maxfov));
                        }
                        float fov = (float)(2 * Math.PI * ((float)arg / 360));
                        this.fov = fov;
                        OnResize(new EventArgs());
                    }
                    else
                    {
                        network.SendChat(GuiTypingBuffer);
                    }
                }
                catch (Exception e) { AddChatline(new StringReader(e.Message).ReadLine()); }
            }
            else
            {
                network.SendChat(GuiTypingBuffer);
            }
        }
        private static bool BoolCommandArgument(string arguments)
        {
            return (arguments == "" || arguments == "1" || arguments == "on");
        }
        private void UploadMap(bool uploadfeature, MapStorage m)
        {
            ENABLE_NOCLIP = true;
            ENABLE_FREEMOVE = true;
            Vector3 playerpos = player.playerposition;
            for (int z = m.MapSizeZ - 1; z >= 0; z--)
            {
                for (int x = 0; x < m.MapSizeX; x++)
                {
                    for (int y = 0; y < m.MapSizeY; y++)
                    {
                        if (exit)
                        {
                            return;
                        }
                        int destx = x;
                        int desty = y;
                        int destz = z;
                        if (uploadfeature)
                        {
                            destx += (int)playerpos.X;
                            desty += (int)playerpos.Z;
                            destz += (int)playerpos.Y;
                        }
                        if (!MapUtil.IsValidPos(map, destx, desty, destz))
                        {
                            continue;
                        }
                        byte oldtile = map.Map[destx, desty, destz];
                        byte newtile = m.Map[x, y, z];
                        if (!(data.IsBuildableTile(oldtile) && data.IsBuildableTile(newtile)))
                        {
                            continue;
                        }
                        if (oldtile != newtile)
                        {
                            int xx = destx;
                            int yy = desty;
                            int zz = destz;
                            var newposition = new Vector3(xx + 0.5f, zz + 1 + 0.2f, yy + 0.5f);
                            Thread.Sleep(TimeSpan.FromSeconds(BuildDelay));
                            todo.Enqueue(() => { network.SendPosition(newposition, player.playerorientation); });
                            player.playerposition = newposition;
                            if (oldtile != data.TileIdEmpty)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(BuildDelay));
                                todo.Enqueue(() => ChangeTile(oldtile, newtile, xx, yy, zz));
                            }
                            if (newtile != data.TileIdEmpty)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(BuildDelay));
                                todo.Enqueue(() => ChangeTile2(oldtile, newtile, xx, yy, zz));
                            }
                        }
                    }
                }
            }
        }
        private void ChangeTile(byte oldtile, byte newtile, int xx, int yy, int zz)
        {
            Console.WriteLine(map.Map[xx, yy, zz]);
            var newposition = new Vector3(xx + 0.5f, zz + 1 + 0.2f, yy + 0.5f);
            game.SendSetBlock(new Vector3(xx, yy, zz), BlockSetMode.Destroy, 0);
        }
        private void ChangeTile2(byte oldtile, byte newtile, int xx, int yy, int zz)
        {
            var newposition = new Vector3(xx + 0.5f, zz + 1 + 0.2f, yy + 0.5f);
            game.SendSetBlock(new Vector3(xx, yy, zz), BlockSetMode.Create, newtile);
        }
        Queue<MethodInvoker> todo = new Queue<MethodInvoker>();
        OpenTK.Input.KeyboardKeyEventArgs keyevent;
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            keyevent = e;
            if (guistate == GuiState.Normal)
            {
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
                    mapManipulator.SaveMap(map, mapManipulator.defaultminesave);
                }
                /*
                if (e.Key == OpenTK.Input.Key.F8)
                {
                    GuiActionGenerateNewMap();
                }
                */
                if (e.Key == OpenTK.Input.Key.F9)
                {
                    ConnectToInternetGame(username, pass, testgameurl);
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
                if (e.Key == OpenTK.Input.Key.F3)
                {
                    ENABLE_FREEMOVE = !ENABLE_FREEMOVE;
                    if (ENABLE_FREEMOVE) { Console.WriteLine("Freemove enabled."); }
                    else { Console.WriteLine("Freemove disabled."); }
                }
                if (e.Key == OpenTK.Input.Key.F4)
                {
                    ENABLE_NOCLIP = !ENABLE_NOCLIP;
                    if (ENABLE_NOCLIP) { Console.WriteLine("Noclip enabled."); }
                    else { Console.WriteLine("Noclip disabled."); }
                }
                if (e.Key == OpenTK.Input.Key.P)
                {
                    player.playerposition = game.PlayerPositionSpawn;
                    player.movedz = 0;
                }
                if (e.Key == OpenTK.Input.Key.B)
                {
                    guistate = GuiState.Inventory;
                    menustate = new MenuState();
                    FreeMouse = true;
                }
                HandleMaterialKeys(e);
                if (e.Key == OpenTK.Input.Key.Escape)
                {
                    GuiStateEscapeMenu();
                }
            }
            else if (guistate == GuiState.EscapeMenu)
            {
                int menuelements = 3;
                if (e.Key == OpenTK.Input.Key.Escape)
                {
                    GuiStateBackToGame();
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
            else if (guistate == GuiState.Inventory)
            {
                if (e.Key == OpenTK.Input.Key.Escape)
                {
                    GuiStateBackToGame();
                }
                Direction4? dir = null;
                if (e.Key == OpenTK.Input.Key.Left) { dir = Direction4.Left; }
                if (e.Key == OpenTK.Input.Key.Right) { dir = Direction4.Right; }
                if (e.Key == OpenTK.Input.Key.Up) { dir = Direction4.Up; }
                if (e.Key == OpenTK.Input.Key.Down) { dir = Direction4.Down; }
                if (dir != null)
                {
                    InventorySelectionMove(dir.Value);
                }
                if (e.Key == OpenTK.Input.Key.Enter)
                {
                    materialSlots[activematerial] = InventoryGetSelected();
                    GuiStateBackToGame();
                }
                HandleMaterialKeys(e);
                return;
            }
            else if (guistate == GuiState.MapLoading)
            {
            }
            else throw new Exception();
        }
        private void HandleMaterialKeys(OpenTK.Input.KeyboardKeyEventArgs e)
        {
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
        private void GuiStateEscapeMenu()
        {
            guistate = GuiState.EscapeMenu;
            menustate = new MenuState();
            FreeMouse = true;
            EscapeMenuWasFreemove = ENABLE_FREEMOVE;
        }
        private void GuiActionLoadGame()
        {
            mapManipulator.LoadMap(map, mapManipulator.defaultminesave);
        }
        bool EscapeMenuWasFreemove;
        private void GuiStateBackToGame()
        {
            guistate = GuiState.Normal;
            FreeMouse = false;
            ENABLE_FREEMOVE = EscapeMenuWasFreemove;
            freemousejustdisabled = true;
        }
        private void GuiActionGenerateNewMap()
        {
            //mapManipulator.GeneratePlainMap(map);
            network.Connect("", 0, "", "");
            game.OnNewMap();
            player.playerposition = game.PlayerPositionSpawn;
            player.playerorientation = game.PlayerOrientationSpawn;
            DrawMap();
        }
        bool freemousejustdisabled;
        enum TypingState { None, Typing, Ready };
        TypingState GuiTyping = TypingState.None;
        string GuiTypingBuffer = "";
        INetworkClient newnetwork;
        ITerrainDrawer newterrain;

        public string username = "gamer1";
        string pass = "12345";
        string testgameurl
        {
            get
            {
                return File.ReadAllText("defaultserver.txt");
            }
        }
        private void ConnectToInternetGame(string qusername, string qpass, string qgameurl)
        {
            var oldclientgame = map;
            var oldnetwork = network;
            var oldterrain = terrain;
            internetgamefactory.NewInternetGame();
            //.newclientgame = internetgamefactory.GetClientGame();
            //.newnetwork = internetgamefactory.GetNetwork();
            //.newterrain = internetgamefactory.GetTerrain();
            //.newterrain.Start();
            var newnetwork = network;
            var newterrain = terrain;
            if (oldterrain is IDisposable) { ((IDisposable)oldterrain).Dispose(); }
            newterrain.Start();

            oldclientgame.Dispose();
            newnetwork.MapLoaded += new EventHandler<MapLoadedEventArgs>(network_MapLoaded);
            newnetwork.MapLoadingProgress += new EventHandler<MapLoadingProgressEventArgs>(newnetwork_MapLoadingProgress);

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
                LoginData logindata = new LoginData();
                System.Net.IPAddress server2 = null;
                if (System.Net.IPAddress.TryParse(qgameurl, out server2))
                {
                    logindata.serveraddress = server2.ToString();
                    logindata.port = 25565;
                    logindata.mppass = "";
                }
                else
                {
                    logindata = login.Login(qusername, qpass, qgameurl);
                }
                frametickmainthreadtodo.Add(
                    () =>
                    {
                        newnetwork.Connect(logindata.serveraddress, logindata.port, username, logindata.mppass);
                    }
                );
            }).BeginInvoke(null, null);
            GuiStateSetMapLoading();
        }
        void newnetwork_MapLoadingProgress(object sender, MapLoadingProgressEventArgs e)
        {
            this.maploadingprogress = e.ProgressPercent;
        }
        private void GuiStateSetMapLoading()
        {
            guistate = GuiState.MapLoading;
            freemouse = true;
            maploadingprogress = 0;
        }
        List<MethodInvoker> frametickmainthreadtodo = new List<MethodInvoker>();
        void network_MapLoaded(object sender, MapLoadedEventArgs e)
        {
            GuiStateBackToGame();
            //frametickmainthreadtodo.Add(
            //() =>
            {
                //.this.network = newnetwork;
                //.this.clientgame = newclientgame;
                //.this.terrain = newterrain;
                //.newnetwork = null; newclientgame = null; newterrain = null;
                var ee = (MapLoadedEventArgs)e;
                //lock (clientgame.mapupdate)
                {
                    map.Map = ee.map;
                    map.MapSizeX = ee.map.GetUpperBound(0) + 1;
                    map.MapSizeY = ee.map.GetUpperBound(1) + 1;
                    map.MapSizeZ = ee.map.GetUpperBound(2) + 1;
                    Console.WriteLine("Game loaded successfully.");
                    DrawMap();
                }
            }
            //);
        }
        void maploaded()
        {
        }
        int[] materialSlots;
        public int[] MaterialSlots { get { return materialSlots; } set { materialSlots = value; } }
        bool ENABLE_ZFAR = false;
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
            Set3dProjection();
        }
        public float fov = MathHelper.PiOver3;
        private void Set3dProjection()
        {
            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(fov, aspect_ratio, znear, zfar);
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
        public Vector3 toVectorInFixedSystem1(float dx, float dy, float dz, double orientationx,double orientationy)
        {
            //Don't calculate for nothing ...
            if (dx == 0.0f & dy == 0.0f && dz == 0.0f)
                return new Vector3();

            //Convert to Radian : 360° = 2PI
            double xRot = orientationx;//Math.toRadians(orientation.X);
            double yRot = orientationy;//Math.toRadians(orientation.Y);

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
        public const float basemovespeed = 5f;
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
        public bool ENABLE_FREEMOVE { get; set; }
        bool enable_move = true;
        public bool ENABLE_MOVE { get { return enable_move; } set { enable_move = value; } }
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
            //if ((DateTime.Now - lasttodo).TotalSeconds > BuildDelay && todo.Count > 0)
            game.OnNewFrame(e.Time);
            while (todo.Count > 0)
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
                    else if(ENABLE_MOVE)
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
            else if (guistate == GuiState.Inventory)
            {
            }
            else if (guistate == GuiState.MapLoading)
            {
                //todo back to game when escape key pressed.
            }
            else throw new Exception();

            if (!(ENABLE_FREEMOVE || Swimming))
            {
                player.movedz += -gravity;//gravity
            }
            Vector3 newposition = player.playerposition + toVectorInFixedSystem1
                (movedx * movespeed * (float)e.Time,
                0,
                movedy * movespeed * (float)e.Time, player.playerorientation.X, player.playerorientation.Y);
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
                physics.swimmingtop = Keyboard[OpenTK.Input.Key.Space];
                player.playerposition = physics.WallSlide(player.playerposition, newposition);
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
            if (iii++ % 2 == 0)
                UpdatePicking();
        }
        int iii = 0;
        bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            if (z >= map.MapSizeZ)
            {
                return true;
            }
            if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            if (x >= map.MapSizeX || y >= map.MapSizeY)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            return map.GetBlock(x, y, z) == data.TileIdEmpty
                || data.IsWaterTile(map.GetBlock(x, y, z));
        }
        float PICK_DISTANCE = 3.5f;
        Matrix4 the_modelview;
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
            //Matrix4 the_modelview;
            //Read the current modelview matrix into the array the_modelview
            //GL.GetFloat(GetPName.ModelviewMatrix, out the_modelview);
            if (the_modelview.Equals(new Matrix4())) { return; }
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
            var s = new BlockOctreeSearcher();
            s.StartBox = new Box3D(0, 0, 0, NextPowerOfTwo((uint)Math.Max(map.MapSizeX, Math.Max(map.MapSizeY, map.MapSizeZ))));
            List<BlockPosSide> pick2 = new List<BlockPosSide>(s.LineIntersection(IsTileEmptyForPhysics, getblockheight, pick));
            pick2.Sort((a, b) => { return (a.pos - player.playerposition).Length.CompareTo((b.pos - player.playerposition).Length); });

            bool left = Mouse[OpenTK.Input.MouseButton.Left];//destruct
            bool middle = Mouse[OpenTK.Input.MouseButton.Middle];//clone material as active
            bool right = Mouse[OpenTK.Input.MouseButton.Right];//build
            BlockPosSide pick0;
            if (pick2.Count > 0 && (pick2[0].pos - (player.playerposition + new Vector3(0, CharacterHeight, 0))).Length <= PICK_DISTANCE
                && IsTileEmptyForPhysics((int)ToMapPos(player.playerposition).X,
                (int)ToMapPos(player.playerposition).Y, (int)ToMapPos(player.playerposition).Z))
            {
                pickcubepos = pick2[0].Current();
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
                        var newtile = pick0.Current();
                        if (MapUtil.IsValidPos(map, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            int clonesource = map.Map[(int)newtile.X, (int)newtile.Z, (int)newtile.Y];
                            clonesource = (int)data.PlayerBuildableMaterialType((int)clonesource);
                            for (int i = 0; i < materialSlots.Length; i++)
                            {
                                if ((int)materialSlots[i] == clonesource)
                                {
                                    activematerial = i;
                                    goto done;
                                }
                            }
                            materialSlots[activematerial] = clonesource;
                        done:
                            audio.Play(soundclone);
                        }
                    }
                    if (left || right)
                    {
                        BlockPosSide tile = pick0;
                        Console.Write(tile.pos + ":" + Enum.GetName(typeof(TileSide), tile.side));
                        Vector3 newtile = right ? tile.Translated() : tile.Current();
                        if (MapUtil.IsValidPos(map, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                        {
                            Console.WriteLine(". newtile:" + newtile + " type: " + map.Map[(int)newtile.X, (int)newtile.Z, (int)newtile.Y]);
                            if (pick0.pos != new Vector3(-1, -1, -1))
                            {
                                audio.Play(left ? sounddestruct : soundbuild);
                            }
                            if (!right)
                            {
                                StartParticleEffect(newtile);//must be before deletion - gets ground type.
                            }
                            if (!MapUtil.IsValidPos(map,(int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                            {
                                throw new Exception();
                            }
                            game.OnPick(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
                                new Vector3((int)tile.Current().X, (int)tile.Current().Z, (int)tile.Current().Y), tile.pos,
                                right);
                            //network.SendSetBlock(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
                            //    right ? BlockSetMode.Create : BlockSetMode.Destroy, (byte)MaterialSlots[activematerial]);
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
        public const float RailHeight = 0.3f;
        float getblockheight(int x, int y, int z)
        {
            if (data.GetRail(map.GetBlock(x, y, z)) != RailDirectionFlags.None)
            {
                return RailHeight;
            }
            if (map.GetBlock(x, y, z) == data.TileIdSingleStairs)
            {
                return 0.5f;
            }
            return 1;
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
        private void OnPick(BlockPosSide pick0)
        {
            playerdestination = pick0.pos;
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
        public float CharacterHeight { get { return CharacterPhysics.characterheight; } set { CharacterPhysics.characterheight = value; } }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Application.DoEvents();
            //Sleep is required in Mono for running the terrain background thread.
            Thread.Sleep(0);
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
            activematerial += Mouse.WheelDelta;
            activematerial = activematerial % 10;
            while (activematerial < 0)
            {
                activematerial += 10;
            }
            SetAmbientLight(terraincolor);
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
            the_modelview = camera;
            bool drawgame = guistate != GuiState.MapLoading;
            if (drawgame)
            {
                DrawSkybox();
                terrain.Draw();
                
                DrawImmediateParticleEffects(e.Time);
                DrawCubeLines(pickcubepos);

                DrawVehicles((float)e.Time);
                DrawPlayers((float)e.Time);
                DrawWeapon();
            }
            SetAmbientLight(Color.White);
            Draw2d();

            //OnResize(new EventArgs());
            SwapBuffers();
            keyevent = null;
        }
        int[] _skybox;
        private void DrawSkybox()
        {
            //?
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP);
            if (_skybox == null)
            {
                _skybox = new int[6];
                _skybox[0] = LoadTexture(getfile.GetFile("skybox_fr.jpg"));
                _skybox[1] = LoadTexture(getfile.GetFile("skybox_lf.jpg"));
                _skybox[2] = LoadTexture(getfile.GetFile("skybox_bk.jpg"));
                _skybox[3] = LoadTexture(getfile.GetFile("skybox_rt.jpg"));
                _skybox[4] = LoadTexture(getfile.GetFile("skybox_up.jpg"));
                _skybox[5] = LoadTexture(getfile.GetFile("skybox_dn.jpg"));
            }
            int size = 1 * 10000;
            // Store the current matrix
            GL.PushMatrix();

            // Reset and transform the matrix.
            //GL.LoadIdentity();
            //gluLookAt(
            //    0, 0, 0,
            //    camera->x(), camera->y(), camera->z(),
            //    0, 1, 0);

            // Enable/Disable features
            GL.PushAttrib(AttribMask.EnableBit);
            GL.Enable(EnableCap.Texture2D);
            //GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);

            // Just in case we set all vertices to white.
            //GL.Color4(1, 1, 1, 1);
            GL.Translate(player.playerposition);
            GL.Scale(size, size, size);

            // Render the front quad
            GL.BindTexture(TextureTarget.Texture2D, _skybox[0]);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.TexCoord2(0, 1); GL.Vertex3(0.5f, 0.5f, -0.5f);
            GL.End();

            // Render the left quad
            GL.BindTexture(TextureTarget.Texture2D, _skybox[1]);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(0.5f, 0.5f, -0.5f);
            GL.TexCoord2(0, 1); GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.End();

            // Render the back quad
            GL.BindTexture(TextureTarget.Texture2D, _skybox[2]);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.TexCoord2(0, 1); GL.Vertex3(-0.5f, 0.5f, 0.5f);

            GL.End();

            // Render the right quad
            GL.BindTexture(TextureTarget.Texture2D, _skybox[3]);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(-0.5f, 0.5f, 0.5f);
            GL.TexCoord2(0, 1); GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.End();

            // Render the top quad
            GL.BindTexture(TextureTarget.Texture2D, _skybox[4]);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.TexCoord2(0, 0); GL.Vertex3(-0.5f, 0.5f, 0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(0.5f, 0.5f, -0.5f);
            GL.End();

            // Render the bottom quad
            GL.BindTexture(TextureTarget.Texture2D, _skybox[5]);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.TexCoord2(0, 1); GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.End();

            // Restore enable bits and matrix
            GL.PopAttrib();
            GL.PopMatrix();
        }
        void DrawWeapon()
        {
            GL.BindTexture(TextureTarget.Texture2D, terrain.terrainTexture);
            List<ushort> myelements = new List<ushort>();
            List<VertexPositionTexture> myvertices = new List<VertexPositionTexture>();
            int x = 0;
            int y = 0;
            int z = 0;
            int tiletype = materialSlots[activematerial];
            //top
            //if (drawtop)
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Top);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, terrain.texturesPacked);
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
            //if (drawbottom)
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Bottom);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, terrain.texturesPacked);
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
            ////front
            //if (drawfront)
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Front);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, terrain.texturesPacked);
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
            //if (drawback)
            {//todo fix tcoords
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Back);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, terrain.texturesPacked);
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
            //if (drawleft)
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Left);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, terrain.texturesPacked);
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
            //if (drawright)
            {//todo fix tcoords
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Right);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, terrain.texturesPacked);
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
            for (int i = 0; i < myvertices.Count; i++)
            {
                var v = myvertices[i];
                //v.Position += new Vector3(-0.5f, 0, -0.5f);
                //v.Position += new Vector3(2, 2, 2);
                /*
                Matrix4 m2;
                Matrix4.CreateRotationY(0.9f, out m2);
                v.Position = Vector3.TransformVector(v.Position, m2);

                Matrix4 m3;
                Matrix4.CreateRotationX(0.3f, out m3);
                v.Position = Vector3.TransformVector(v.Position, m3);
                */

                //Matrix4 m;
                //Matrix4.CreateRotationY(-player.playerorientation.Y, out m);
                //v.Position = Vector3.TransformPosition(v.Position, m);

                ////Matrix4.CreateRotationX(player.playerorientation.X, out m);
                ////v.Position = Vector3.TransformPosition(v.Position, m);
                
                //v.Position += new Vector3(0, -0.2f, 0);
                //v.Position += player.playerposition;
                //v.Position += toVectorInFixedSystem1(0.7f, 0, 1, player.playerorientation.X, player.playerorientation.Y);
                myvertices[i] = v;
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Translate(0, -1.5f + zzzposx, -1.5f + zzzposy);
            //GL.Scale(2, 2, 2);
            GL.Rotate(30 + (zzzx), new Vector3(1, 0, 0));//zzz += 0.01f
            GL.Rotate(60 +zzzy, new Vector3(0, 1, 0));
            //GL.Rotate(0-(zzz+=0.05f), new Vector3(0, 1, 0));
            //GL.Translate(0, -2, 0);

            /*
            if (Keyboard[OpenTK.Input.Key.Left]) zzzx += -0.1f;
            if (Keyboard[OpenTK.Input.Key.Right]) zzzx += 0.1f;
            if (Keyboard[OpenTK.Input.Key.Up]) zzzy += 0.1f;
            if (Keyboard[OpenTK.Input.Key.Down]) zzzy += -0.1f;
            if (Keyboard[OpenTK.Input.Key.Keypad4]) zzzposx += -0.1f;
            if (Keyboard[OpenTK.Input.Key.Keypad6]) zzzposx += 0.1f;
            if (Keyboard[OpenTK.Input.Key.Keypad8]) zzzposy += 0.1f;
            if (Keyboard[OpenTK.Input.Key.Keypad2]) zzzposy += -0.1f;
            */
            GL.Begin(BeginMode.Triangles);
            GL.BindTexture(TextureTarget.Texture2D, terrain.terrainTexture);
            GL.Enable(EnableCap.Texture2D);
            for (int i = 0; i < myelements.Count; i++)
            {
                GL.TexCoord2(myvertices[myelements[i]].u, myvertices[myelements[i]].v);
                GL.Vertex3(myvertices[myelements[i]].Position);
            }
            GL.End();
            GL.PopMatrix();
            //Console.WriteLine("({0}||{1}):({2}||{3})", zzzx, zzzy, zzzposx, zzzposy);
            //(-19,00004||-13,70002):(-0,2000001||-1,3)
        }
        float zzzx = -19;
        float zzzy = -13.7f;
        float zzzposx = -0.2f;
        float zzzposy = -1.3f;
        float attackprogress = 0;
        NetworkInterpolation interpolation = new NetworkInterpolation();
        /*
        public class PlayerInterpolated
        {
            public Vector3 LastRealPosition;
            public DateTime LastRealPositionTime;
            public Vector3 InterpolatedPosition;
            public Vector3 Direction;
        }
        */
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
                cc.heading = (byte)Interpolate360(aa.heading, bb.heading, progress);
                cc.pitch = (byte)((float)aa.pitch + ((float)bb.pitch - (float)aa.pitch) * progress);
                return cc;
            }
            int Interpolate360(int a, int b, float progress)
            {
                if (progress != 0 && b != a)
                {
                    int diff = NormalizeAngle(b - a);
                    if (diff >= CircleHalf)
                    {
                        diff -= CircleFull;
                    }
                    a += (int)(progress * diff);
                }
                return NormalizeAngle(a);
            }
            int CircleHalf = 256 / 2;
            int CircleFull = 256;
            private int NormalizeAngle(int v)
            {
                return v % 256;
            }
        }
        double totaltime;
        private void DrawPlayers(float dt)
        {
            totaltime += dt;
            foreach (var k in clients.Players)
            {
                if (!playerdrawinfo.ContainsKey(k.Key))
                {
                    playerdrawinfo[k.Key] = new PlayerDrawInfo();
                    NetworkInterpolation n = new NetworkInterpolation();
                    n.req = new PlayerInterpolate();
                    n.FRAMESIZE = 0.1f;
                    n.DELAY = 0.4f;
                    playerdrawinfo[k.Key].interpolation = n;
                }
                PlayerDrawInfo info = playerdrawinfo[k.Key];
                Vector3 realpos = k.Value.Position;
                if (realpos != info.lastrealpos
                    || k.Value.Heading != info.lastrealheading
                    || k.Value.Pitch != info.lastrealpitch)
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
                Vector3 curpos = curstate.position;
                bool moves = curpos != info.lastcurpos;
                DrawCharacter(info.anim, curpos + new Vector3(0, -0.25f, 0), curstate.heading, curstate.pitch, moves, dt);
                info.lastcurpos = curpos;
                info.lastrealpos = realpos;
                info.lastrealheading = k.Value.Heading;
                info.lastrealpitch = k.Value.Pitch;
            }
        }
        bool overheadcamera = false;
        Kamera overheadcameraK = new Kamera();
        Matrix4 FppCamera()
        {
            Vector3 forward = toVectorInFixedSystem1(0, 0, 1, player.playerorientation.X, player.playerorientation.Y);
            return Matrix4.LookAt(player.playerposition + new Vector3(0, CharacterHeight, 0),
                player.playerposition + new Vector3(0, CharacterHeight, 0) + forward, up);
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
        Dictionary<string, int> textures = new Dictionary<string, int>();
        class OpentkGl : Md2Engine.IOpenGl
        {
            #region IOpenGl Members
            public void GlBegin(Md2Engine.BeginMode mode)
            {
                if (mode == Md2Engine.BeginMode.Triangles)
                {
                    GL.Begin(BeginMode.Triangles);
                }
                else if (mode == Md2Engine.BeginMode.TriangleFan)
                {
                    GL.Begin(BeginMode.TriangleFan);
                }
                else if (mode == Md2Engine.BeginMode.TriangleStrip)
                {
                    GL.Begin(BeginMode.TriangleStrip);
                }
                else
                {
                    throw new Exception();
                }
            }
            public void GlEnd()
            {
                GL.End();
            }
            public void GlFrontFace(Md2Engine.FrontFace mode)
            {
                if (mode == Md2Engine.FrontFace.Cw)
                {
                    GL.FrontFace(FrontFaceDirection.Cw);
                }
                else if (mode == Md2Engine.FrontFace.Ccw)
                {
                    GL.FrontFace(FrontFaceDirection.Ccw);
                }
                else
                {
                    throw new Exception();
                }
            }
            public void GlNormal3f(float x, float y, float z)
            {
                GL.Normal3(x, y, z);
            }
            public void GlTexCoord2f(float x, float y)
            {
                GL.TexCoord2(x, y);
            }
            public void GlVertex3f(float x, float y, float z)
            {
                GL.Vertex3(x, y, z);
            }
            #endregion
        }
        Md2Engine.GlRenderer md2renderer = new Md2Engine.GlRenderer() { gl=new OpentkGl()};
        Md2Engine.Mesh m1;
        int m1texture;
        class AnimationState
        {
            public float interp;
            public int frame;
        }
        int Animate(AnimationState anim, int start, int end, int frame, float dt)//update the animation parameters
        {
            anim.interp += dt * 5;

            if (anim.interp > 1.0f)
            {
                anim.interp = 0.0f;
                frame++;
            }

            if ((frame < start) || (frame >= end))
                frame = start;

            return frame;
        }
        AnimationState v0anim = new AnimationState();
        void DrawVehicles(float dt)
        {
            if (m1 == null)
            {
                m1 = new Md2Engine.Mesh();
                m1.loadMD2(getfile.GetFile("player.md2"));
                m1texture = LoadTexture(getfile.GetFile("player.png"));
            }
            //if (v0 != null)
            foreach (ICharacterToDraw v0 in game.Characters)
            {
                DrawCharacter(v0anim, v0.Pos3d + new Vector3(0, 0.9f, 0), v0.Dir3d, v0.Moves, dt);
                //DrawCube(v0.pos3d);
            }
        }
        private void DrawCharacter(AnimationState animstate, Vector3 pos, Vector3 dir, bool moves, float dt)
        {
            DrawCharacter(animstate, pos,
                (byte)(((Vector3.CalculateAngle(new Vector3(1, 0, 0), dir) + 90) / (2 * (float)Math.PI)) * 256), 0, moves, dt);
        }
        private void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt)
        {
            string anim = moves ? "run" : "stand";
            Md2Engine.Range tmp = m1.animationPool[m1.findAnim(anim)];
            animstate.frame = Animate(animstate, tmp.getStart(), tmp.getEnd(), animstate.frame, dt);
            GL.PushMatrix();
            GL.Translate(pos);
            GL.Rotate(-90, 1, 0, 0);
            GL.Rotate((-((float)heading / 256)) * 360 + 90, 0, 0, 1);
            GL.Scale(0.05f, 0.05f, 0.05f);
            GL.BindTexture(TextureTarget.Texture2D, m1texture);
            md2renderer.RenderFrameImmediateInterpolated(animstate.frame, animstate.interp, m1, true, tmp.getStart(), tmp.getEnd());
            GL.PopMatrix();
            GL.FrontFace(FrontFaceDirection.Ccw);
        }
        void EscapeMenuAction()
        {
            if (menustate.selected == 0)
            {
                //GuiActionGenerateNewMap();
                GuiStateBackToGame();
            }
            else if (menustate.selected == 1)
            {
                GuiActionSaveGame();
                GuiStateBackToGame();
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
            mapManipulator.SaveMap(map, mapManipulator.defaultminesave);
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
                GuiStateBackToGame();
            }
            else if (menustate.selected == 1)
            {
                if (SaveGameExists())
                {
                    GuiActionLoadGame();
                    GuiStateBackToGame();
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
            Inventory,
            MapLoading,
        }
        private void DrawMouseCursor()
        {
            Draw2dBitmapFile("gui\\mousecursor.png", mouse_current.X, mouse_current.Y, 20, 20);
        }
        int chatfontsize = 12;
        Size? aimsize;
        private void Draw2d()
        {
            OrthoMode();
            if (guistate == GuiState.Normal)
            {
                if (aimsize == null)
                {
                    using (var targetbmp = new Bitmap(getfile.GetFile("target.png")))
                    {
                        aimsize = targetbmp.Size;
                    }
                }
                float aimwidth = aimsize.Value.Width;
                float aimheight = aimsize.Value.Height;

                Draw2dBitmapFile("target.png", Width / 2 - aimwidth / 2, Height / 2 - aimheight / 2, aimwidth, aimheight);

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
            { DrawEscapeMenu(); }
            else if (guistate == GuiState.MainMenu)
            { DrawMainMenu(); }
            else if (guistate == GuiState.Inventory)
            { DrawInventory(); }
            else if (guistate == GuiState.MapLoading)
            { DrawMapLoading(); }
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
        int maploadingprogress;
        private void DrawMapLoading()
        {
            string connecting="Connecting...";
            string progress = string.Format("{0}%", maploadingprogress);
            Draw2dText("Connecting...", xcenter(TextSize(connecting, 14).Width), Height / 2 - 50, 14, Color.White);
            Draw2dText(progress, xcenter(TextSize(progress, 14).Width), Height / 2 - 20, 14, Color.White);
        }
        int inventoryselectedx;
        int inventoryselectedy;
        void InventorySelectionMove(Direction4 dir)
        {
            if (dir == Direction4.Left) { inventoryselectedx--; }
            if (dir == Direction4.Right) { inventoryselectedx++; }
            if (dir == Direction4.Up) { inventoryselectedy--; }
            if (dir == Direction4.Down) { inventoryselectedy++; }
            inventoryselectedx = Clamp(inventoryselectedx, 0, inventorysize - 1);
            inventoryselectedy = Clamp(inventoryselectedy, 0, inventorysize - 1);
        }
        int inventorysize;
        int InventoryGetSelected()
        {
            return Buildable[inventoryselectedx + (inventoryselectedy * inventorysize)];
        }
        List<int> Buildable
        {
            get
            {
                List<int> buildable = new List<int>();
                for (int i = 0; i < 256; i++)
                {
                    if (data.IsValidTileType((byte)i) && data.IsBuildableTile((byte)i))
                    {
                        buildable.Add(i);
                    }
                }
                return buildable;
            }
        }
        void DrawInventory()
        {
            List<int> buildable = Buildable;
            inventorysize = (int)Math.Ceiling(Math.Sqrt(buildable.Count));
            int singlesize = 40;
            int x = 0;
            int y = 0;
            for (int ii = 0; ii < buildable.Count; ii++)
            {
                Draw2dTexture(terrain.terrainTexture, xcenter(singlesize * inventorysize) + x * singlesize,
                    ycenter(singlesize * inventorysize) + y * singlesize, singlesize, singlesize,
                    data.GetTileTextureId(buildable[ii], TileSide.Front));
                if (x == inventoryselectedx && y == inventoryselectedy)
                {
                    Draw2dBitmapFile("gui\\activematerial.png",
                        xcenter(singlesize * inventorysize) + x * singlesize,
                        ycenter(singlesize * inventorysize) + y * singlesize, singlesize, singlesize);
                }
                x++;
                if (x >= inventorysize)
                {
                    x = 0;
                    y++;
                }
            }
            DrawMaterialSelector();
        }
        private void DrawMaterialSelector()
        {
            int singlesize = 40;
            for (int i = 0; i < 10; i++)
            {
                Draw2dTexture(terrain.terrainTexture, xcenter(singlesize * 10) + i * singlesize, Height - 100, singlesize, singlesize,
                    data.GetTileTextureId((int)materialSlots[i], TileSide.Front));
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
        struct Text
        {
            public string text;
            public float fontsize;
            public Color color;
            public override int GetHashCode()
            {
                return ("" + text.GetHashCode() + fontsize.GetHashCode() + color.GetHashCode()).GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is Text)
                {
                    Text other = (Text)obj;
                    return other.text.Equals(this.text)
                        && other.fontsize.Equals(this.fontsize)
                        && other.color.Equals(this.color);
                }
                return base.Equals(obj);
            }
        }
        class CachedTexture
        {
            public int textureId;
            public SizeF size;
            public DateTime lastuse;
        }
        Dictionary<Text, CachedTexture> cachedTextTextures = new Dictionary<Text, CachedTexture>();
        CachedTexture MakeTextTexture(Text t)
        {
            var font = new Font("Verdana", t.fontsize);
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            SizeF size = g.MeasureString(t.text, font);
            if (size.Width == 0 || size.Height == 0)
            {
                return null;
            }
            bmp = new Bitmap((int)size.Width, (int)size.Height);
            g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, size.Width, size.Height);
            g.DrawString(t.text, font, new SolidBrush(t.color), 0, 0);
            int texture = LoadTexture(bmp);
            return new CachedTexture() { textureId = texture, size = size };
        }
        void DeleteUnusedCachedTextures()
        {
            foreach (var k in new List<Text>( cachedTextTextures.Keys))
            {
                var ct = cachedTextTextures[k];
                if ((DateTime.Now - ct.lastuse).TotalSeconds > 1)
                {
                    GL.DeleteTexture(ct.textureId);
                    cachedTextTextures.Remove(k);
                }
            }
        }
        void Draw2dText(string text, float x, float y, float fontsize, Color? color)
        {
            if (text.Trim() == "")
            {
                return;
            }
            if (color == null) { color = Color.White; }
            var t = new Text();
            t.text = text;
            t.color = color.Value;
            t.fontsize = fontsize;
            CachedTexture ct;
            if (!cachedTextTextures.ContainsKey(t))
            {
                ct = MakeTextTexture(t);
                if (ct == null)
                {
                    return;
                }
                cachedTextTextures.Add(t, ct);
            }
            ct = cachedTextTextures[t];
            ct.lastuse = DateTime.Now;
            Draw2dTexture(ct.textureId, x, y, ct.size.Width, ct.size.Height, null);
            DeleteUnusedCachedTextures();
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
            if (!MapUtil.IsValidPos(map, (int)v.X, (int)v.Z, (int)v.Y))
            {
                return;
            }
            int tiletype = map.Map[(int)v.X, (int)v.Z, (int)v.Y];
            if (!data.IsValidTileType(tiletype))
            {
                return;
            }
            p.textureid = data.GetTileTextureId(tiletype, TileSide.Top);
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
        /*
        private Vector3 From3dPos(BlockPosSide v)
        {
            if (v.side == TileSide.Back) { return v.pos + new Vector3(-1, 0, 0); }
            if (v.side == TileSide.Right) { return v.pos + new Vector3(0, 0, -1); }
            if (v.side == TileSide.Top) { return v.pos + new Vector3(0, -1, 0); }
            return v.pos;
        }
        */
        public int activematerial { get; set; }
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
            float pickcubeheight = 1;
            if (posx != new Vector3(-1, -1, -1))
            {
                pickcubeheight = getblockheight((int)posx.X, (int)posx.Z, (int)posx.Y);
            }
            //Vector3 pos = new Vector3((int)posx.X, (int)posx.Y, (int)posx.Z);
            Vector3 pos = posx;
            pos += new Vector3(0.5f, pickcubeheight * 0.5f, 0.5f);
            GL.LineWidth(2);
            float size = 0.51f;
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.LineStrip);
            GL.Color3(Color.White);
            //GL.Color3(Color.Silver);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.Honeydew);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size);

            //GL.Color3(Color.Moccasin);

            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.IndianRed);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size);

            //GL.Color3(Color.PaleVioletRed);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + -1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size);

            //GL.Color3(Color.ForestGreen);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + -1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + pickcubeheight * size, pos.Z + 1.0f * size);
            GL.Vertex3(pos.X + 1.0f * size, pos.Y + -pickcubeheight * size, pos.Z + 1.0f * size);

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
        float longestframedt = 0;
        private void UpdateTitleFps(FrameEventArgs e)
        {
            string title = "";
            fpscount++;
            longestframedt = (float)Math.Max(longestframedt, e.Time);
            TimeSpan elapsed = (DateTime.Now - lasttitleupdate);
            if (elapsed.TotalSeconds >= 1)
            {
                lasttitleupdate = DateTime.Now;
                title += "FPS: " + (int)((float)fpscount / elapsed.TotalSeconds);
                title += string.Format(" (min: {0})", (int)(1f / longestframedt));
                longestframedt = 0;
                //z = 100;
                fpscount = 0;
                int totaltriangles = terrain.TrianglesCount();
                title += ", triangles: " + totaltriangles;
                //Title = title;
                fpstext = title;
            }
            if (!titleset)
            {
                Title = applicationname;
                titleset = true;
            }
        }
        bool titleset = false;
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
                if (!MapUtil.IsValidPos(map, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y)))
                {
                    return p.Y < map.WaterLevel;
                }
                return data.IsWaterTile(map.GetBlock((int)p.X, (int)p.Z, (int)p.Y));
            }
        }
        #endregion
        public string GameUrl;
        Color terraincolor { get { return Swimming ? Color.FromArgb(255, 78, 95, 140) : Color.White; } }
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
        #endregion
    }
}
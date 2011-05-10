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
using System.Net;
using System.Drawing.Drawing2D;
using ManicDigger.Network;
using ManicDigger.Renderers;
using ManicDigger.Gui;
using ManicDigger.Hud;

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
        public VertexPositionTexture(float x, float y, float z, float u, float v, FastColor c)
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
        public int verticesCount;
        public ushort[] indices;
        public int indicesCount;
        public Vector3 position;
        public bool transparent;
        public int texture;
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
    public interface IGetCameraMatrix
    {
        Matrix4 ModelViewMatrix { get; }
        Matrix4 ProjectionMatrix { get; }
    }
    public interface IThe3d
    {
        int LoadTexture(Stream file);
        int LoadTexture(Bitmap bmp);
        void Set3dProjection(float zfar);
        void Set3dProjection();
    }
    public class The3dDummy : IThe3d
    {
        public int TextureId;
        #region IThe3d Members
        public int LoadTexture(Stream file)
        {
            return TextureId;
        }
        public int LoadTerrainTexture(string filename)
        {
            return TextureId;
        }
        #endregion
        #region IThe3d Members
        public int LoadTexture(Bitmap bmp)
        {
            return TextureId;
        }
        #endregion
        #region IThe3d Members
        public Matrix4 ModelViewMatrix
        {
            get { return new Matrix4(); }
        }
        public Matrix4 ProjectionMatrix
        {
            get { return new Matrix4(); }
        }
        #endregion
        public void Set3dProjection(float zfar)
        {
        }
        public void Set3dProjection()
        {
        }
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
    public class KameraDummy : IKamera
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
        public float tt = 0;
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
    public enum Direction4
    {
        Left,
        Right,
        Up,
        Down,
    }
    public interface IKeyboard
    {
        OpenTK.Input.KeyboardDevice keyboardstate { get; }
        OpenTK.Input.KeyboardKeyEventArgs keypressed { get; }
        OpenTK.Input.KeyboardKeyEventArgs keydepressed { get; }
    }
    public interface IViewportSize
    {
        int Width { get; }
        int Height { get; }
    }
    public interface IViewport3d : ILocalPlayerPosition, IKeyboard, IViewportSize, IViewport3dSelectedBlock
    {
        [Obsolete]
        int[] MaterialSlots { get; set; }
        int ActiveMaterial { get; set; }
        Dictionary<int, int> FiniteInventory { get; set; }
        bool ENABLE_FREEMOVE { get; set; }
        bool ENABLE_MOVE { get; set; }
        void Log(string s);
        Dictionary<string, string> PerformanceInfo { get; }
        AnimationHint LocalPlayerAnimationHint { get; set; }
        Vector3 PickCubePos { get; }
        string LocalPlayerName { get; }
        void CraftingRecipesStart(List<CraftingRecipe> recipes, List<int> blocks, Action<int?> craftingRecipeSelected);
        int SelectedModelId { get; }
        bool ENABLE_FINITEINVENTORY { get; set; }
        bool SkySphereNight { get; set; }
        OpenTK.Input.Key GetKey(OpenTK.Input.Key key);
        Options Options { get; set; }
    }
    public class AnimationHint
    {
        public bool InVehicle;
        public Vector3 DrawFix;
        public bool leanleft;
        public bool leanright;
    }
    public class ViewportDummy : IViewport3d
    {
        #region IViewport3d Members
        public int[] MaterialSlots { get; set; }
        public int ActiveMaterial { get; set; }
        public bool ENABLE_FREEMOVE { get; set; }
        public bool ENABLE_MOVE { get; set; }
        public void Log(string s)
        {
        }
        #endregion
        #region ILocalPlayerPosition Members
        public Vector3 LocalPlayerPosition { get; set; }
        public Vector3 LocalPlayerOrientation { get; set; }
        public bool Swimming { get { return false; } }
        public float CharacterHeight { get; set; }
        #endregion
        #region IKeyboard Members
        public OpenTK.Input.KeyboardDevice keyboardstate
        {
            get { throw new NotImplementedException(); }
        }
        public OpenTK.Input.KeyboardKeyEventArgs keypressed
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region IViewport3d Members
        public Dictionary<string, string> PerformanceInfo { get; set; }
        #endregion
        #region IViewport3d Members
        public AnimationHint LocalPlayerAnimationHint { get; set; }
        #endregion
        #region IViewport3d Members
        public Vector3 PickCubePos { get; set; }
        #endregion
        #region IViewport3d Members
        public Dictionary<int, int> FiniteInventory { get; set; }
        #endregion
        #region IViewport3d Members
        public string LocalPlayerName { get; set; }
        #endregion
        #region IViewport3d Members
        public void CraftingRecipesStart(List<CraftingRecipe> recipes, List<int> blocks, Action<int?> craftingRecipeSelected)
        {
        }
        #endregion
        #region IKeyboard Members
        public OpenTK.Input.KeyboardKeyEventArgs keydepressed
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region IViewport3d Members
        public int SelectedModelId
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region IViewport3d Members
        public bool ENABLE_FINITEINVENTORY { get; set; }
        #endregion
        #region IViewportSize Members
        public int Width { get { return 1; } }
        public int Height { get { return 1; } }
        #endregion
        #region IViewport3d Members
        public bool SkySphereNight { get; set; }
        #endregion
        public OpenTK.Input.Key GetKey(OpenTK.Input.Key key)
        {
            return key;
        }
        public Options Options { get; set; }
        public Vector3i SelectedBlock()
        {
            return new Vector3i();
        }
    }
    public interface IModelToDraw
    {
        void Draw(float dt);
        IEnumerable<Triangle3D> TrianglesForPicking { get; }
        int Id { get; }
    }
    public interface IGameMode
    {
        void OnPick(Vector3 blockposnew, Vector3 blockposold, Vector3 pos3d, bool right);
        void SendSetBlock(Vector3 vector3, BlockSetMode blockSetMode, int type);
        void OnNewFrame(double dt);
        IEnumerable<ICharacterToDraw> Characters { get; }
        IEnumerable<IModelToDraw> Models { get; }
        Vector3 PlayerPositionSpawn { get; set; }
        Vector3 PlayerOrientationSpawn { get; }
        void OnNewMap();
        int FiniteInventoryAmount(int blocktype);
        int FiniteInventoryMax { get; }
        double SIMULATIONLAG_SECONDS { get; set; }
        void ModelClick(int selectedmodelid);
    }
    public interface ICharacterToDraw : IModelToDraw
    {
        /*
        Vector3 Pos3d { get; }
        Vector3 Dir3d { get; }
        bool Moves { get; }
        */
    }
    public class AnimationState
    {
        public float interp;
        public int frame;
        public object data;
        public float slowdownTimer;
    }
    public interface ICurrentShadows
    {
        bool ShadowsFull { get; set; }
    }
    public struct Draw2dData
    {
        public float x1;
        public float y1;
        public float width;
        public float height;
        public int? inAtlasId;
        public FastColor color;
    }
    public interface IMyGameWindow
    {
        void OnLoad(EventArgs e);
        void OnFocusedChanged(EventArgs e);
        void OnClosed(EventArgs e);
        void OnResize(EventArgs e);
        void OnUpdateFrame(FrameEventArgs e);
        void OnRenderFrame(FrameEventArgs e);
        void OnKeyPress(OpenTK.KeyPressEventArgs e);
    }
    public class MainGameWindow : GameWindow
    {
        public IMyGameWindow mywindow;
        const bool ENABLE_FULLSCREEN = false;
        public MainGameWindow(IMyGameWindow mywindow)
            : base(800, 600, GraphicsMode.Default, "",
                ENABLE_FULLSCREEN ? GameWindowFlags.Fullscreen : GameWindowFlags.Default) { this.mywindow = mywindow; }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mywindow.OnLoad(e);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            mywindow.OnResize(e);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            mywindow.OnUpdateFrame(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            mywindow.OnRenderFrame(e);
        }
        protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            mywindow.OnKeyPress(e);
        }
    }
    public class ConnectData
    {
        public string Username;
        public string Ip;
        public int Port;
        public string Auth;
    }
    public partial class ManicDiggerGameWindow : IMyGameWindow, ILocalPlayerPosition, IMap,
        IAddChatLine, IViewport3d, IWaterLevel, IMouseCurrent, IActiveMaterial
    {
        [Inject]
        public MainGameWindow d_MainWindow;
        [Inject]
        public The3d d_The3d;
        [Inject]
        public ManicDigger.Renderers.TextRenderer d_TextRenderer;
        [Inject]
        public ITerrainRenderer d_Terrain;
        [Inject]
        public IGameMode d_Game;
        [Inject]
        public IMapStorage d_Map;
        [Inject]
        public IClients d_Clients;
        [Inject]
        public CharacterPhysics d_Physics;
        [Inject]
        public INetworkClient d_Network;

        [Inject]
        public IAudio d_Audio;
        [Inject]
        public IGetFileStream d_GetFile;
        [Inject]
        public IGameData d_Data;
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
        public IShadows d_Shadows;
        [Inject]
        public IGameExit d_Exit;
        [Inject]
        public ITerrainTextures d_TerrainTextures;
        [Inject]
        public HudChat d_HudChat;
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

        public bool SkySphereNight { get; set; }

        public bool IsMono = Type.GetType("Mono.Runtime") != null;
        public bool IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;

        const float rotation_speed = 180.0f * 0.05f;
        //float angle;

        public void SetTileAndUpdate(Vector3 pos, int type)
        {
            //            frametickmainthreadtodo.Add(() =>
            //        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int z = (int)pos.Z;
            d_Map.SetBlock(x, y, z, type);
            d_Terrain.UpdateTile(x, y, z);
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
            Connect();

            string version = GL.GetString(StringName.Version);
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major <= 1 && minor < 5)
            {
                System.Windows.Forms.MessageBox.Show("You need at least OpenGL 1.5 to run this example. Aborting.", "VBOs not supported",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                d_MainWindow.Exit();
            }
            if (!d_Config3d.ENABLE_VSYNC)
            {
                d_MainWindow.TargetRenderFrequency = 0;
            }
            GL.ClearColor(Color.Black);
            d_MainWindow.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            d_MainWindow.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            d_MainWindow.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            d_MainWindow.Mouse.WheelChanged += new EventHandler<OpenTK.Input.MouseWheelEventArgs>(Mouse_WheelChanged);
            if (d_Config3d.ENABLE_BACKFACECULLING)
            {
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
            }
            d_MainWindow.Keyboard.KeyRepeat = true;
            d_MainWindow.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(ManicDiggerGameWindow_KeyPress);
            d_MainWindow.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            d_MainWindow.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);

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
                d_MainWindow.CursorVisible = false;
            }
        }
        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!d_MainWindow.Focused)
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
        }
        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!d_MainWindow.Focused)
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
            if ((e.KeyChar == 't' || e.KeyChar == 'T') && GuiTyping == TypingState.None)
            {
                GuiTyping = TypingState.Typing;
                d_HudChat.GuiTypingBuffer = "";
                return;
            }
            if (GuiTyping == TypingState.Typing)
            {
                char c = e.KeyChar;
                if ((char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)
                    || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
                    && c != '\r')
                {
                    d_HudChat.GuiTypingBuffer += e.KeyChar;
                }
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
                        ENABLE_FREEMOVE = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "fov")
                    {
                        int arg = int.Parse(arguments);
                        int minfov = 1;
                        int maxfov = 179;
                        if (arg < minfov || arg > maxfov)
                        {
                            throw new Exception(string.Format("Valid field of view: {0}-{1}", minfov, maxfov));
                        }
                        float fov = (float)(2 * Math.PI * ((float)arg / 360));
                        d_The3d.fov = fov;
                        OnResize(new EventArgs());
                    }
                    else if (cmd == "tp" || cmd == "teleport")
                    {
                        string arg = arguments;
                        bool tp = false;
                        foreach (var k in d_Clients.Players)
                        {
                            if (k.Value.Name.Equals(arg, StringComparison.InvariantCultureIgnoreCase))
                            {
                                player.playerposition = k.Value.Position;
                                tp = true;
                            }
                        }
                        if (!tp)
                        {
                            Log(string.Format("No such player: {0}.", arg));
                        }
                    }
                    else if (cmd == "testmodel")
                    {
                        ENABLE_DRAW_TEST_CHARACTER = BoolCommandArgument(arguments);
                    }
                    else if (cmd == "simulationlag")
                    {
                        d_Game.SIMULATIONLAG_SECONDS = double.Parse(arguments);
                    }
                    else if (cmd == "gui")
                    {
                        ENABLE_DRAW2D = BoolCommandArgument(arguments);
                    }
                    else
                    {
                        string chatline = d_HudChat.GuiTypingBuffer.Substring(0, Math.Min(d_HudChat.GuiTypingBuffer.Length, 64));
                        d_Network.SendChat(chatline);
                    }
                }
                catch (Exception e) { AddChatline(new StringReader(e.Message).ReadLine()); }
            }
            else
            {
                string chatline = d_HudChat.GuiTypingBuffer.Substring(0, Math.Min(d_HudChat.GuiTypingBuffer.Length, 64));
                d_Network.SendChat(chatline);
            }
        }
        private static bool BoolCommandArgument(string arguments)
        {
            arguments = arguments.Trim();
            return (arguments == "" || arguments == "1" || arguments == "on" || arguments == "yes");
        }
        OpenTK.Input.KeyboardKeyEventArgs keyevent;
        OpenTK.Input.KeyboardKeyEventArgs keyeventup;
        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (GuiTyping == TypingState.None)
            {
                keyeventup = e;
            }
        }
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == GetKey(OpenTK.Input.Key.F11))
            {
                if (d_MainWindow.WindowState == WindowState.Fullscreen)
                {
                    d_MainWindow.WindowState = WindowState.Normal;
                }
                else
                {
                    d_MainWindow.WindowState = WindowState.Fullscreen;
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
                    guistate = GuiState.EscapeMenu;
                    menustate = new MenuState();
                    FreeMouse = true;
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
                string strFreemoveNotAllowed = "Freemove is not allowed on this server.";
                if (e.Key == GetKey(OpenTK.Input.Key.F1))
                {
                    if (!d_Network.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    movespeed = basemovespeed * 1;
                    Log("Move speed: 1x.");
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F2))
                {
                    if (!d_Network.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    movespeed = basemovespeed * 10;
                    Log("Move speed: 10x.");
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F3))
                {
                    if (!d_Network.AllowFreemove)
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    player.movedz = 0;
                    if (!ENABLE_FREEMOVE)
                    {
                        ENABLE_FREEMOVE = true;
                        Log("Move: Free.");
                    }
                    else if (ENABLE_FREEMOVE && (!ENABLE_NOCLIP))
                    {
                        ENABLE_NOCLIP = true;
                        Log("Move: Free, Noclip.");
                    }
                    else if (ENABLE_FREEMOVE && ENABLE_NOCLIP)
                    {
                        ENABLE_FREEMOVE = false;
                        ENABLE_NOCLIP = false;
                        Log("Move: Normal.");
                    }
                }
                if (e.Key == GetKey(OpenTK.Input.Key.I))
                {
                    drawblockinfo = !drawblockinfo;
                }
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
                if (e.Key == GetKey(OpenTK.Input.Key.F6))
                {
                    d_Terrain.UpdateAllTiles();
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
                    ENABLE_LAG++;
                    ENABLE_LAG = ENABLE_LAG % 3;
                    d_MainWindow.VSync = (ENABLE_LAG == 1) ? VSyncMode.Off : VSyncMode.On;
                    if (ENABLE_LAG == 0) { Log("Frame rate: vsync."); }
                    if (ENABLE_LAG == 1) { Log("Frame rate: unlimited."); }
                    if (ENABLE_LAG == 2) { Log("Frame rate: lag simulation."); }
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F12))
                {
                    d_Screenshot.SaveScreenshot();
                    screenshotflash = 5;
                }
                if (e.Key == GetKey(OpenTK.Input.Key.R))
                {
                    player.playerposition = d_Game.PlayerPositionSpawn;
                    player.movedz = 0;
                    Log("Respawn.");
                }
                if (e.Key == GetKey(OpenTK.Input.Key.P))
                {
                    d_Game.PlayerPositionSpawn = player.playerposition;
                    player.playerposition = new Vector3((int)player.playerposition.X + 0.5f, player.playerposition.Y, (int)player.playerposition.Z + 0.5f);
                    Log("Spawn position set.");
                }
                if (e.Key == GetKey(OpenTK.Input.Key.F))
                {
                    ToggleFog();
                    Log("Fog distance: " + d_Config3d.viewdistance);
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
                return;
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
            else throw new Exception();
        }
        public int[] drawDistances = { 32, 64, 128, 256, 512 };
        private void ToggleFog()
        {
            for (int i = 0; i < drawDistances.Length; i++)
            {
                if (d_Config3d.viewdistance == drawDistances[i])
                {
                    d_Config3d.viewdistance = drawDistances[(i + 1) % drawDistances.Length];
                    return;
                }
            }
            d_Config3d.viewdistance = drawDistances[0];
        }
        enum CameraType
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
        private void Connect()
        {
            LoadOptions();
            d_Terrain.Start();
            d_Network.MapLoaded += new EventHandler<MapLoadedEventArgs>(network_MapLoaded);
            d_Network.MapLoadingProgress += new EventHandler<MapLoadingProgressEventArgs>(newnetwork_MapLoadingProgress);
            d_Network.Connect(connectdata.Ip, connectdata.Port, connectdata.Username, connectdata.Auth);
            MapLoadingStart();
        }
        void newnetwork_MapLoadingProgress(object sender, MapLoadingProgressEventArgs e)
        {
            this.maploadingprogress = e;
        }
        void network_MapLoaded(object sender, MapLoadedEventArgs e)
        {
            materialSlots = d_Data.DefaultMaterialSlots;
            GuiStateBackToGame();
            d_Game.OnNewMap();
            d_Terrain.UpdateAllTiles();
        }
        [Obsolete]
        int[] materialSlots;
        [Obsolete]
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
        [Obsolete]
        Dictionary<int, int> finiteinventory = new Dictionary<int, int>();
        [Obsolete]
        public Dictionary<int, int> FiniteInventory { get { return finiteinventory; } set { finiteinventory = value; } }
        public bool enable_finiteinventory = false;
        public bool ENABLE_FINITEINVENTORY { get { return enable_finiteinventory; } set { enable_finiteinventory = value; } }
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
                    d_MainWindow.CursorVisible = value;
                    System.Windows.Forms.Cursor.Hide();
                }
                freemouse = value;
            }
        }
        void UpdateMouseButtons()
        {
            if (!d_MainWindow.Focused)
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
                mouse_current.Offset(-d_MainWindow.X, -d_MainWindow.Y);
                mouse_current.Offset(0, -System.Windows.Forms.SystemInformation.CaptionHeight);
            }
            if (!d_MainWindow.Focused)
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
                    int centerx = d_MainWindow.Bounds.Left + (d_MainWindow.Bounds.Width / 2);
                    int centery = d_MainWindow.Bounds.Top + (d_MainWindow.Bounds.Height / 2);

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
        DateTime lasttodo;
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
            d_Game.OnNewFrame(e.Time);
            UpdateMousePosition();
            if (guistate == GuiState.Normal)
            {
                UpdateMouseViewportControl(e);
            }
            d_Network.Process();
            if (guistate == GuiState.MapLoading) { return; }

            bool angleup = false;
            bool angledown = false;
            float overheadcameraanglemovearea = 0.05f;
            float overheadcameraspeed = 3;
            if (guistate == GuiState.Normal && d_MainWindow.Focused && cameratype == CameraType.Overhead)
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
            float jumpstartacceleration = 2.1f * d_Physics.gravity;
            if (blockunderplayer != null && blockunderplayer == d_Data.BlockIdTrampoline
                && (!player.isplayeronground))
            {
                wantsjump = true;
                jumpstartacceleration = 5f * d_Physics.gravity;
            }
            Vector3 push = new Vector3();
            foreach (var k in d_Clients.Players)
            {
                if ((k.Key == 255) ||
                    (k.Value.Position == LocalPlayerPosition)
                     || (float.IsNaN(LocalPlayerPosition.X)))
                {
                    continue;
                }
                if ((k.Value.Position - LocalPlayerPosition).Length < PlayerPushDistance)
                {
                    Vector3 diff = LocalPlayerPosition - k.Value.Position;
                    push += diff;
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
                Swimming = Swimming,
                wantsjump = wantsjump,
            };
            bool soundnow;
            d_Physics.Move(player, move, e.Time, out soundnow, push);
            if (soundnow)
            {
                UpdateWalkSound(-1);
            }
            if (player.isplayeronground && movedx != 0 || movedy != 0)
            {
                UpdateWalkSound(e.Time);
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
        }
        public OpenTK.Input.Key GetKey(OpenTK.Input.Key key)
        {
            if (options.Keys.ContainsKey((int)key))
            {
                return (OpenTK.Input.Key)options.Keys[(int)key];
            }
            return key;
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
            return movespeednow;
        }
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
                || d_Data.IsWater[d_Map.GetBlock(x, y, z)];
        }
        bool IsTileEmptyForPhysicsClose(int x, int y, int z)
        {
            return IsTileEmptyForPhysics(x, y, z)
                || (MapUtil.IsValidPos(d_Map, x, y, z) && d_Map.GetBlock(x, y, z) == d_Data.BlockIdSingleStairs)
                || (MapUtil.IsValidPos(d_Map, x, y, z) && d_Data.IsEmptyForPhysics[d_Map.GetBlock(x, y, z)]);
        }
        public float PICK_DISTANCE = 3.5f;
        public float PickDistance { get { return PICK_DISTANCE; } set { PICK_DISTANCE = value; } }
        bool leftpressedpicking = false;
        public int SelectedModelId { get { return selectedmodelid; } set { selectedmodelid = value; } }
        int selectedmodelid = -1;
        private void UpdatePicking()
        {
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

            float pick_distance = PICK_DISTANCE;
            if (cameratype == CameraType.Tpp) { pick_distance = tppcameradistance * 2; }
            if (cameratype == CameraType.Overhead) { pick_distance = overheadcameradistance; }

            float unit_x = 0;
            float unit_y = 0;
            int NEAR = 1;
            int FOV = 600;
            float ASPECT = 640f / 480;
            float near_height = NEAR * (float)(Math.Tan(FOV * Math.PI / 360.0));
            Vector3 ray = new Vector3(unit_x * near_height * ASPECT, unit_y * near_height, 1);//, 0);

            Vector3 ray_start_point = new Vector3(0, 0, 0);
            if (overheadcamera)
            {
                float mx = (float)mouse_current.X / Width - 0.5f;
                float my = (float)mouse_current.Y / Height - 0.5f;
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
            pick.End = ray + Vector3.Multiply(raydir, pick_distance * 2);

            //pick models
            selectedmodelid = -1;
            foreach (var m in d_Game.Models)
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
                pickcubepos = new Vector3(-1, -1, -1);
                if (mouseleftclick)
                {
                    d_Game.ModelClick(selectedmodelid);
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
                            int? freehand = d_InventoryUtil.FreeHand();
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
                                            InventoryPosition.MainArea(k.Key.ToPoint()),
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
                                            InventoryPosition.MainArea(k.Key.ToPoint()),
                                            InventoryPosition.MaterialSelector(ActiveMaterial));
                                    }
                                }
                            }
                        done:
                            d_Audio.Play(d_Data.CloneSound[clonesource][0]); //todo sound cycle
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
                                else { blocktype = materialSlots[ActiveMaterial]; }
                                if (left && blocktype == d_Data.BlockIdAdminium) { goto end; }
                                string[] sound = left ? d_Data.BreakSound[blocktype] : d_Data.BuildSound[blocktype];
                                if (sound != null && sound.Length > 0)
                                {
                                    d_Audio.Play(sound[0]); //todo sound cycle
                                }
                            }
                            if (!right)
                            {
                                particleEffectBlockBreak.StartParticleEffect(newtile);//must be before deletion - gets ground type.
                            }
                            if (!MapUtil.IsValidPos(d_Map, (int)newtile.X, (int)newtile.Z, (int)newtile.Y))
                            {
                                throw new Exception();
                            }
                            d_Game.OnPick(new Vector3((int)newtile.X, (int)newtile.Z, (int)newtile.Y),
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
            if (!(left || right || middle))
            {
                lastbuild = new DateTime();
                fastclicking = true;
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
            if (d_Map.GetBlock(x, y, z) == d_Data.BlockIdSingleStairs)
            {
                return 0.5f;
            }
            return 1;
        }
        private void OnPick(BlockPosSide pick0)
        {
            //playerdestination = pick0.pos;
        }
        float BuildDelay = 0.95f * (1 / basemovespeed);
        Vector3 ToMapPos(Vector3 a)
        {
            return new Vector3((int)a.X, (int)a.Z, (int)a.Y);
        }
        bool fastclicking = false;
        public Vector3 pickcubepos;
        //double currentTime = 0;
        double accumulator = 0;
        double t = 0;
        //Vector3 oldplayerposition;
        public float CharacterHeight { get { return CharacterPhysics.characterheight; } set { CharacterPhysics.characterheight = value; } }
        public Color clearcolor = Color.FromArgb(171, 202, 228);
        public void OnRenderFrame(FrameEventArgs e)
        {
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
            if (d_Config3d.viewdistance < 256)
            {
                SetFog();
            }
            else
            {
                GL.Disable(EnableCap.Fog);
            }

            if (!keyboardstate[GetKey(OpenTK.Input.Key.LControl)])
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
                DrawSkySphere();
                DrawPlayers((float)e.Time);
                d_SunMoonRenderer.Draw((float)e.Time);
                d_Terrain.Draw();
                DrawPlayerNames();
                particleEffectBlockBreak.DrawImmediateParticleEffects(e.Time);
                if (ENABLE_DRAW2D)
                {
                    DrawLinesAroundSelectedCube(pickcubepos);
                }

                DrawCharacters((float)e.Time);
                if (ENABLE_DRAW_TEST_CHARACTER)
                {
                    d_CharacterRenderer.DrawCharacter(a, d_Game.PlayerPositionSpawn, 0, 0, true, (float)dt, GetPlayerTexture(255), new AnimationHint());
                }
                foreach (IModelToDraw m in d_Game.Models)
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
                    d_Weapon.DrawWeapon((float)e.Time);
                }
            }
        draw2d:
            SetAmbientLight(Color.White);
            Draw2d();

            //OnResize(new EventArgs());
            d_MainWindow.SwapBuffers();
            mouseleftclick = mouserightclick = false;
            mouseleftdeclick = mouserightdeclick = false;
        }
        private void SetFog()
        {
            float density = 0.3f;
            //float[] fogColor = new[] { 1f, 1f, 1f, 1.0f };
            float[] fogColor;
            if (SkySphereNight && d_Shadows.GetType() != typeof(ShadowsSimple))
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
        public string playertexturedefaultfilename = "mineplayer.png";
        private int GetPlayerTexture(int playerid)
        {
            if (playertexturedefault == -1)
            {
                playertexturedefault = LoadTexture(playertexturedefaultfilename);
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
            if (playerid == 255)
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
            if (d_Shadows.GetType() == typeof(ShadowsSimple))
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
        private void DrawPlayers(float dt)
        {
            totaltime += dt;
            foreach (var k in d_Clients.Players)
            {
                if (k.Key == 255)
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
                //do not interpolate player position if player is controlled by game world
                if (d_Network.EnablePlayerUpdatePosition.ContainsKey(k.Key) && !d_Network.EnablePlayerUpdatePosition[k.Key])
                {
                    curstate.position = k.Value.Position;
                }
                Vector3 curpos = curstate.position;
                bool moves = curpos != info.lastcurpos;
                float shadow = (float)d_Shadows.MaybeGetLight((int)curpos.X, (int)curpos.Z, (int)curpos.Y) / d_Shadows.maxlight;
                GL.Color3(shadow, shadow, shadow);
                DrawCharacter(info.anim, curpos + new Vector3(0, -CharacterPhysics.characterheight, 0)
                    + new Vector3(0, -CharacterPhysics.walldistance, 0),
                    curstate.heading, curstate.pitch, moves, dt, GetPlayerTexture(k.Key),
                    d_Clients.Players[k.Key].AnimationHint);
                info.lastcurpos = curpos;
                info.lastrealpos = realpos;
                info.lastrealheading = k.Value.Heading;
                info.lastrealpitch = k.Value.Pitch;
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
                DrawCharacter(localplayeranim, LocalPlayerPosition + new Vector3(0, -CharacterPhysics.walldistance, 0),
                    NetworkHelper.HeadingByte(LocalPlayerOrientation),
                    NetworkHelper.PitchByte(LocalPlayerOrientation),
                    lastlocalplayerpos != LocalPlayerPosition, dt, GetPlayerTexture(255), localplayeranimationhint);
                lastlocalplayerpos = LocalPlayerPosition;
                GL.Color3(1f, 1f, 1f);
            }
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
            Vector3 playerEye = player.playerposition + new Vector3(0, CharacterHeight, 0);
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
            Vector3 cameraTarget = overheadcameraK.Center + new Vector3(0, CharacterHeight, 0);
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
            foreach (ICharacterToDraw v0 in d_Game.Characters)
            {
                v0.Draw(dt);
            }
        }
        private void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt, int playertexture, AnimationHint animationhint)
        {
            d_CharacterRenderer.SetAnimation("walk");
            d_CharacterRenderer.DrawCharacter(animstate, pos, (byte)(-heading - 256 / 4), pitch, moves, dt, playertexture, animationhint);
        }
        GuiState guistate;
        enum GuiState
        {
            Normal,
            EscapeMenu,
            Inventory,
            MapLoading,
            CraftingRecipes,
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
                        d_HudChat.DrawChatLines(GuiTyping == TypingState.Typing);
                        if (GuiTyping == TypingState.Typing)
                        {
                            d_HudChat.DrawTypingBuffer();
                        }
                        if (Keyboard[GetKey(OpenTK.Input.Key.Tab)])
                        {
                            DrawConnectedPlayersList();
                        }
                    }
                    break;
                case GuiState.EscapeMenu:
                    {
                        EscapeMenuDraw();
                    }
                    break;
                case GuiState.Inventory:
                    {
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
            double lagSeconds = (DateTime.UtcNow - d_Network.LastReceived).TotalSeconds;
            if (lagSeconds >= DISCONNECTED_ICON_AFTER_SECONDS && lagSeconds < 60 * 60 * 24)
            {
                d_The3d.Draw2dBitmapFile("disconnected.png", Width - 100, 50, 50, 50);
                d_The3d.Draw2dText(((int)lagSeconds).ToString(), Width - 100, 50 + 50 + 10, 12, Color.White);
            }
            d_The3d.PerspectiveMode();
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
            int x = (int)pickcubepos.X;
            int y = (int)pickcubepos.Z;
            int z = (int)pickcubepos.Y;
            string info = "None";
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                var blocktype = d_Map.GetBlock(x, y, z);
                if (d_Data.IsValid[blocktype])
                {
                    info = d_Data.Name[blocktype];
                }
            }
            d_The3d.Draw2dText(info, Width * 0.5f - d_The3d.TextSize(info, 18f).Width / 2, 30f, 18f, Color.White);
        }
        private void DrawConnectedPlayersList()
        {
            List<string> l = new List<string>(d_Network.ConnectedPlayers());
            for (int i = 0; i < l.Count; i++)
            {
                d_The3d.Draw2dText(l[i], 200 + 200 * (i / 8), 200 + 30 * i, d_HudChat.ChatFontSize, Color.White);
            }
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

            d_The3d.Draw2dBitmapFile("target.png", Width / 2 - aimwidth / 2, Height / 2 - aimheight / 2, aimwidth, aimheight);
        }
        private void DrawPlayerNames()
        {
            foreach (KeyValuePair<int, Player> k in d_Clients.Players)
            {
                if (k.Key == 255 || k.Value.Name == ""
                    || (!playerdrawinfo.ContainsKey(k.Key))
                    || playerdrawinfo[k.Key].interpolation == null)
                {
                    continue;
                }
                //todo if picking
                if (((LocalPlayerPosition - k.Value.Position).Length < 20)
                    || Keyboard[GetKey(OpenTK.Input.Key.AltLeft)] || Keyboard[GetKey(OpenTK.Input.Key.AltRight)])
                {
                    string name = k.Value.Name;
                    var ppos = playerdrawinfo[k.Key].interpolation.InterpolatedState(totaltime);
                    if (ppos != null)
                    {
                        Vector3 pos = ((PlayerInterpolationState)ppos).position;
                        float shadow = (float)d_Shadows.MaybeGetLight((int)pos.X, (int)pos.Z, (int)pos.Y) / d_Shadows.maxlight;
                        //do not interpolate player position if player is controlled by game world
                        if (d_Network.EnablePlayerUpdatePosition.ContainsKey(k.Key) && !d_Network.EnablePlayerUpdatePosition[k.Key])
                        {
                            pos = k.Value.Position;
                        }
                        GL.PushMatrix();
                        GL.Translate(pos.X, pos.Y + 1f, pos.Z);
                        GL.Rotate(-player.playerorientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
                        GL.Rotate(-player.playerorientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
                        GL.Scale(0.02, 0.02, 0.02);
                        GL.Translate(-d_The3d.TextSize(name, 14).Width / 2, 0, 0);
                        //Color c = Color.FromArgb((int)(shadow * 255), (int)(shadow * 255), (int)(shadow * 255));
                        //Todo: Can't change text color because text has outline anyway.
                        d_The3d.Draw2dText(name, 0, 0, 14, Color.White);
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
                performanceinfo["triangles"] = "Triangles: " + d_Terrain.TrianglesCount();
                int chunkupdates = d_Terrain.ChunkUpdates;
                performanceinfo["chunk updates"] = "Chunk updates: " + (chunkupdates - lastchunkupdates);
                lastchunkupdates = d_Terrain.ChunkUpdates;

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
                d_MainWindow.Title = applicationname;
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
            d_HudChat.AddChatline(s);
        }
        #region ILocalPlayerPosition Members
        public bool Swimming
        {
            get
            {
                var p = LocalPlayerPosition;
                p += new Vector3(0, CharacterPhysics.characterheight, 0);
                if (!MapUtil.IsValidPos(d_Map, (int)Math.Floor(p.X), (int)Math.Floor(p.Z), (int)Math.Floor(p.Y)))
                {
                    return p.Y < WaterLevel;
                }
                return d_Data.IsWater[d_Map.GetBlock((int)p.X, (int)p.Z, (int)p.Y)];
            }
        }
        #endregion
        public float WaterLevel { get { return d_Map.MapSizeZ / 2; } set { } }
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
        public Vector3 PickCubePos { get { return pickcubepos; } }
        #endregion
        #region IViewport3d Members
        public string LocalPlayerName { get { return connectdata.Username; } }
        #endregion
        #region IMap Members
        public void UpdateAllTiles()
        {
            d_Terrain.UpdateAllTiles();
        }
        #endregion
        public Options Options { get { return options; } set { options = value; } }
        public int Height { get { return d_MainWindow.Height; } }
        public int Width { get { return d_MainWindow.Width; } }
        public OpenTK.Input.KeyboardDevice Keyboard { get { return d_MainWindow.Keyboard; } }
        public OpenTK.Input.MouseDevice Mouse { get { return d_MainWindow.Mouse; } }
        public void Run()
        {
            d_MainWindow.Run();
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
            Vector3 pos = pickcubepos;
            if (pos == new Vector3(-1, -1, -1))
            {
                pos = player.playerposition;
            }
            return new Vector3i((int)pos.X, (int)pos.Z, (int)pos.Y);
        }
    }
}
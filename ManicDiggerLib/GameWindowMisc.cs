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
    public class AnimationHint
    {
        public bool InVehicle;
        public Vector3 DrawFix;
        public bool leanleft;
        public bool leanright;
    }
    public interface IModelToDraw
    {
        void Draw(float dt);
        IEnumerable<Triangle3D> TrianglesForPicking { get; }
        int Id { get; }
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
    //OpenTK.GameWindow can't be destroyed and recreated during program lifetime,
    //because it would be very noticeable (new window in Windows, 5-10 seconds).
    //So there is just one MainGameWindow (never deleted) that delegates
    //its tasks to IMyGameWindow which can be replaced at runtime.
    public class GlWindow : GameWindow
    {
        public IMyGameWindow mywindow;
        const bool ENABLE_FULLSCREEN = false;
        public GlWindow(IMyGameWindow mywindow)
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
        public string ServerPassword;
        public bool IsServePasswordProtected;
        public static ConnectData FromUri(MyUri uri)
        {
            ConnectData c = new ConnectData();
            c = new ConnectData();
            c.Ip = uri.Ip;
            c.Port = 25565;
            c.Username = "gamer";
            if (uri.Port != -1)
            {
                c.Port = uri.Port;
            }
            if (uri.Get.ContainsKey("user"))
            {
                c.Username = uri.Get["user"];
            }
            if (uri.Get.ContainsKey("auth"))
            {
                c.Auth = uri.Get["auth"];
            }
            if (uri.Get.ContainsKey("serverPassword"))
            {
                c.IsServePasswordProtected = Misc.ReadBool(uri.Get["serverPassword"]);
            }
            return c;
        }
    }
    public class ConnectedPlayer
    {
        public int id;
        public string name;
        public int ping; // in ms
    }
    public class ServerInformation
    {
        public string ServerName;
        public string ServerMotd;
        public ConnectData connectdata;
        public List<ConnectedPlayer> Players;
        public TimeSpan ServerPing;
        public bool AllowFreemove;

        public ServerInformation()
        {
            this.ServerName = "";
            this.ServerMotd = "";
            this.connectdata = new ConnectData();
            this.Players = new List<ConnectedPlayer>();
            this.AllowFreemove = false;
        }
    }
}
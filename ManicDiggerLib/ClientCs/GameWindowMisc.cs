using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
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
        static uint ToRgba(Color color)
        {
            return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
        }
    }

    public interface IThe3d
    {
        int LoadTexture(Bitmap bmp);
    }
    public interface IModelToDraw
    {
        void Draw(float dt);
        int Id { get; }
    }
    public interface ICurrentShadows
    {
        bool ShadowsFull { get; set; }
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
        void OnClosing(System.ComponentModel.CancelEventArgs e);
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
            : base(1280, 720, GraphicsMode.Default, "",
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
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            mywindow.OnClosing(e);
        }
    }
}

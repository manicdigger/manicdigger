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
using ManicDigger.Renderers;

namespace ManicDigger
{
    public interface IThe3d
    {
        int LoadTexture(Bitmap bmp);
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
        void OnResize();
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
            mywindow.OnResize();
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

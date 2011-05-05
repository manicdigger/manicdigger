using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using ManicDigger;
using OpenTK.Graphics.OpenGL;

namespace ManicDigger.Tests
{
    public class WindowTests : GameWindow, IViewportSize
    {
        public WindowTests(GraphicsMode mode)
            : base(800, 600, mode)
        {
            VSync = VSyncMode.On;
            WindowState = WindowState.Normal;
            MainMenu();
        }

        private void MainMenu()
        {
            currentTest = new ListOfTests() { window = this, viewportsize = this };
            currentTest.OnLoad(null);
        }

        public IMyGameWindow currentTest;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            currentTest.OnResize(e);
        }
        double esctime = 0;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (Keyboard[Key.Escape])
            {
                MainMenu();
                esctime += e.Time;
                if (esctime > 0.5) { Exit(); }
            }
            else
            {
                esctime = 0;
            }
            currentTest.OnUpdateFrame(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            currentTest.OnKeyPress(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            currentTest.OnRenderFrame(e);
            SwapBuffers();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            GraphicsMode mode = new GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 24, 0, 2, new OpenTK.Graphics.ColorFormat(32));
            using (WindowTests game = new WindowTests(mode))
            {
                game.Run(60.0);
            }
        }
    }
    public abstract class MyGameWindow : IMyGameWindow
    {
        public virtual void OnClosed(EventArgs e) { }
        public virtual void OnFocusedChanged(EventArgs e) { }
        public virtual void OnKeyPress(KeyPressEventArgs e) { }
        public virtual void OnLoad(EventArgs e) { }
        public virtual void OnRenderFrame(FrameEventArgs e) { }
        public virtual void OnResize(EventArgs e) { }
        public virtual void OnUpdateFrame(FrameEventArgs e) { }
    }
}

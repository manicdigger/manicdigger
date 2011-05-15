using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using ManicDigger.Renderers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.IO;

namespace ManicDigger.Tests
{
    public class ListOfTests : MyGameWindow
    {
        public WindowTests window;
        public IViewportSize viewportsize;

        public override void OnLoad(EventArgs e)
        {
            string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
            var getfile = new GetFileStream(datapaths);
            
            the3d.d_Config3d = new Config3d();
            the3d.d_GetFile = getfile;
            the3d.d_Terrain = new TerrainTextures();
            the3d.d_TextRenderer = textrenderer;
            the3d.d_ViewportSize = viewportsize;

            AddTest(new TestFont() { window = window, viewportsize = window, getfile = getfile }, "Font");
            AddTest(new TestAudio() { getfile = getfile }, "Audio");
            AddTest(new TestInventory() { window = window, viewportsize = window, getfile = getfile }, "Inventory");
            AddTest(new TestHeartbeat() {  }, "Server heartbeat");
        }
        The3d the3d = new The3d();
        TextRenderer textrenderer = new TextRenderer();

        void AddTest(IMyGameWindow window, string name)
        {
            tests.Add(new Test() { window = window, name = name });
        }
        struct Test
        {
            public IMyGameWindow window;
            public string name;
        }
        List<Test> tests = new List<Test>();

        public override void OnRenderFrame(FrameEventArgs e)
        {
            Point mouse_current = System.Windows.Forms.Cursor.Position;
            mouse_current.Offset(-window.X, -window.Y);
            mouse_current.Offset(0, -20);

            OrthoMode();
            int selectedTest = (mouse_current.Y - 100) / 40;
            selectedTest = MyMath.Clamp(selectedTest, 0, tests.Count - 1);
            for (int i = 0; i < tests.Count; i++)
            {
                the3d.Draw2dText(i.ToString() + ". " + tests[i].name, 100, 100 + i * 40, 14, i == selectedTest ? Color.Red : Color.White);
            }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                StartTest(selectedTest);
            }
        }
        private void StartTest(int selectedTest)
        {
            window.currentTest = tests[selectedTest].window;
            window.currentTest.OnLoad(null);
        }
        public override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            the3d.Set3dProjection();
        }
        void OrthoMode()
        {
            //GL.Disable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, window.Width, window.Height, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
        }
    }
}

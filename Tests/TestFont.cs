using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using OpenTK;
using ManicDigger.Renderers;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace ManicDigger.Tests
{
    public class TestFont : MyGameWindow
    {
        public GameWindow window;
        public IGetFileStream getfile;
        public IViewportSize viewportsize;
        public override void OnLoad(EventArgs e)
        {
            the3d.d_Config3d = new Config3d();
            the3d.d_GetFile = getfile;
            the3d.d_Terrain = new TerrainTextures();
            the3d.d_TextRenderer = new TextRenderer();
            the3d.d_ViewportSize = viewportsize;
        }
        The3d the3d = new The3d();
        Color[] colors = new Color[]
		{
			Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet,
		};
        string[] colorCodes = new string[]
		{
			"&0","&1","&2","&3","&4","&5","&6","&7",
			"&7","&9","&a","&b","&c","&d","&e","&f",
		};
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            GL.ClearColor(Color.Green);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            the3d.OrthoMode(window.Width, window.Height);
            for (int i = 0; i < 7; i++)
            {
                the3d.Draw2dText("Hello!", 50, 100 + i * 50, 8 + i * 6, colors[i]);
            }
            for (int i = 0; i < 16; i++)
            {
                the3d.Draw2dText(colorCodes[i] + "Color" + "&f & " + colorCodes[i][1], 300, 100 + i * 30, 14, null);
            }
        }
        public override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            the3d.Set3dProjection();
        }
    }
}

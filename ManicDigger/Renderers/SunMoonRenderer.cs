using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;

namespace ManicDigger.Renderers
{
    public class SunMoonRenderer
    {
        [Inject]
        public IDraw2d draw2d { get; set; }
        [Inject]
        public ILocalPlayerPosition player { get; set; }
        [Inject]
        public IThe3d the3d { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        int hour = 6;
        public int Hour
        {
            get
            {
                return hour;
            }
            set
            {
                hour = value;
                t = (hour - 6) / 24f * 2 * Math.PI;
            }
        }
        double t = 0;
        int suntexture = -1;
        int moontexture = -1;
        public int ImageSize = 96;
        public float day_length_in_seconds = 30;
        public void Draw(float dt)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            if (suntexture == -1)
            {
                suntexture = the3d.LoadTexture(getfile.GetFile("sun.png"));
                moontexture = the3d.LoadTexture(getfile.GetFile("moon.png"));
            }            
            t += dt * 2 * Math.PI / day_length_in_seconds;
            bool night = (t + 2 * Math.PI) % (2 * Math.PI) > Math.PI;
            double tt = t;
            if (night)
            {
                tt = -t;
                //tt -= Math.PI;
            }
            Vector3 pos = new Vector3((float)Math.Cos(tt) * 20, (float)Math.Sin(tt) * 20, (float)Math.Sin(t) * 20);
            pos += player.LocalPlayerPosition;
            GL.PushMatrix();
            GL.Translate(pos.X, pos.Y, pos.Z);
            GL.Rotate(-player.LocalPlayerOrientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
            GL.Rotate(-player.LocalPlayerOrientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
            GL.Scale(0.02, 0.02, 0.02);
            //GL.Translate(-ImageSize / 2, -ImageSize / 2, 0);
            draw2d.Draw2dTexture(night ? moontexture : suntexture, 0, 0, ImageSize, ImageSize, null, Color.White);
            GL.PopMatrix();
        }
    }
}

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
        public ManicDiggerGameWindow game;
        [Inject]
        public IDraw2d d_Draw2d { get; set; }
        [Inject]
        public ILocalPlayerPosition d_LocalPlayerPosition { get; set; }
        [Inject]
        public IThe3d d_The3d { get; set; }
        [Inject]
        public IGetFileStream d_GetFile { get; set; }
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
                suntexture = game.LoadTexture(d_GetFile.GetFile("sun.png"));
                moontexture = game.LoadTexture(d_GetFile.GetFile("moon.png"));
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
            pos += d_LocalPlayerPosition.LocalPlayerPosition;
            GL.PushMatrix();
            GL.Translate(pos.X, pos.Y, pos.Z);
            GL.Rotate(-d_LocalPlayerPosition.LocalPlayerOrientation.Y * 360 / (2 * Math.PI), 0.0f, 1.0f, 0.0f);
            GL.Rotate(-d_LocalPlayerPosition.LocalPlayerOrientation.X * 360 / (2 * Math.PI), 1.0f, 0.0f, 0.0f);
            GL.Scale(0.02, 0.02, 0.02);
            //GL.Translate(-ImageSize / 2, -ImageSize / 2, 0);
            game.Draw2dTexture(night ? moontexture : suntexture, 0, 0, ImageSize, ImageSize, null, Color.White);
            GL.PopMatrix();
        }
    }
}

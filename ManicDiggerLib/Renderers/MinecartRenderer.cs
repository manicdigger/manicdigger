using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ManicDigger.Renderers
{
    public class Minecart : IModelToDraw
    {
        public Vector3 position;
        public VehicleDirection12 direction;
        public VehicleDirection12 lastdirection;
        public double progress;
        public MinecartRenderer renderer;
        #region IModelToDraw Members
        public void Draw(float dt)
        {
            renderer.Draw(position, direction, lastdirection, progress);
        }
        public IEnumerable<ManicDigger.Collisions.Triangle3D> TrianglesForPicking
        {
            get { yield break; }
        }
        public int Id
        {
            get { return 0; }
        }
        #endregion
    }
    public class MinecartRenderer
    {
        public ManicDiggerGameWindow game;
        [Inject]
        public IGetFileStream d_GetFile { get; set; }
        [Inject]
        public IThe3d d_The3d { get; set; }
        int minecarttexture = -1;
        #region IModelToDraw Members
        public void Draw(Vector3 position, VehicleDirection12 dir, VehicleDirection12 lastdir, double progress)
        {
            if (minecarttexture == -1)
            {
            	minecarttexture = d_The3d.LoadTexture(new System.Drawing.Bitmap(new System.IO.MemoryStream(MyStream.ReadAllBytes(d_GetFile.GetFile("minecart.png")))));
            }
            game.GLPushMatrix();
            Vector3 p = position + new Vector3(0, -0.7f, 0);
            game.GLTranslate(p.X, p.Y, p.Z);
            double currot = vehiclerotation(dir);
            double lastrot = vehiclerotation(lastdir);
            //double rot = lastrot + (currot - lastrot) * progress;
            float rot = (float)AngleInterpolation.InterpolateAngle360(lastrot, currot, progress);
            game.GLRotate(-rot - 90, 0, 1, 0);
            var c = new CharacterRendererMonsterCode();
            c.game = game;
            var cc = c.CuboidNet(8, 8, 8, 0, 0);
            CharacterRendererMonsterCode.CuboidNetNormalize(cc, 32, 16);
            c.DrawCuboid(new Vector3(-0.5f, -0.3f, -0.5f), new Vector3(1, 1, 1), minecarttexture, cc);
            game.GLPopMatrix();
        }
        #endregion
        double vehiclerotation(VehicleDirection12 dir)
        {
            switch (dir)
            {
                case VehicleDirection12.VerticalUp:
                    return 0;
                case VehicleDirection12.DownRightRight:
                case VehicleDirection12.UpLeftUp:
                    return 45;
                case VehicleDirection12.HorizontalRight:
                    return 90;
                case VehicleDirection12.UpRightRight:
                case VehicleDirection12.DownLeftDown:
                    return 90 + 45;
                case VehicleDirection12.VerticalDown:
                    return 180;
                case VehicleDirection12.UpLeftLeft:
                case VehicleDirection12.DownRightDown:
                    return 180 + 45;
                case VehicleDirection12.HorizontalLeft:
                    return 180 + 90;
                case VehicleDirection12.UpRightUp:
                case VehicleDirection12.DownLeftLeft:
                    return 180 + 90 + 45;
                default:
                    throw new Exception();
            }
        }
    }
}

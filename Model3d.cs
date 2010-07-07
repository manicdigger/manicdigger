using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace ManicDigger
{
    public interface ICharacterDrawer
    {
        void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt, int playertexture, AnimationHint animationhint);
    }
    public class CharacterDrawerBlock : ICharacterDrawer
    {
        [Inject]
        public IGetFilePath getfile { get; set; }
        public void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt, int playertexture, AnimationHint animationhint)
        {
            if (animationhint.InVehicle)
            {
                moves = false;
            }
            pos += animationhint.DrawFix;
            RectangleF[] coords;
            float legsheight = 0.9f;
            float armsheight = legsheight + 0.9f;
            if (moves)
            {
                animstate.interp += dt;
            }
            else
            {
                float f = Normalize(animstate.interp);
                if (Math.Abs(f) < 0.05f)
                {
                    animstate.interp = 0;
                }
                else
                {
                    animstate.interp += dt;
                }
            }
            GL.PushMatrix();
            GL.Translate(pos);
            GL.Rotate((-((float)heading / 256)) * 360 - 90, 0, 1, 0);
            GL.Scale(0.7f, 0.7f, 0.7f);
            GL.Translate(0 - 0.3f / 2, -1.57f, 0 - 0.6f / 2);
            GL.Translate(0, UpDown(animstate.interp), 0);
            //torso
            coords = MakeCoords(8, 12, 4, 16, 16);
            MakeTextureCoords(coords, 64, 32);
            DrawCube(new Vector3(0, 0 + legsheight, 0), new Vector3(0.3f, 0.9f, 0.6f), playertexture, coords);
            //head
            GL.PushMatrix();
            GL.Translate(0, armsheight, 0);
            GL.Rotate((((float)pitch / 256)) * 360, 0, 0, 1);
            if (animationhint.leanleft)
            {
                GL.Rotate(30, 0, 1, 0);
            }
            if (animationhint.leanright)
            {
                GL.Rotate(-30, 0, 1, 0);
            }
            GL.Translate(0, -armsheight, 0);
            coords = MakeCoords(8, 8, 8, 0, 0);
            MakeTextureCoords(coords, 64, 32);
            DrawCube(new Vector3(-0.6f / 4, 0.9f + legsheight, 0), new Vector3(0.6f, 0.6f, 0.6f), playertexture, coords);
            GL.PopMatrix();
            //left leg
            GL.PushMatrix();
            GL.Translate(0.3f / 2, legsheight, 0);
            GL.Rotate(LeftLegRotation(animstate.interp), 0, 0, 1);
            GL.Translate(-0.3f / 2, -legsheight, 0);

            coords = MakeCoords(4, 8, 4, 0, 16);
            MakeTextureCoords(coords, 64, 32);
            DrawCube(new Vector3(0, 0, 0), new Vector3(0.3f, 0.9f, 0.3f), playertexture, coords);

            GL.PopMatrix();

            //right leg
            GL.PushMatrix();
            GL.Translate(0.3f / 2, legsheight, 0);
            GL.Rotate(RightLegRotation(animstate.interp), 0, 0, 1);
            GL.Translate(-0.3f / 2, -legsheight, 0);

            DrawCube(new Vector3(0, 0, 0.3f), new Vector3(0.3f, 0.9f, 0.3f), playertexture, coords);

            GL.PopMatrix();
            //left arm
            GL.PushMatrix();
            GL.Translate(0.3f / 2, armsheight, 0);
            GL.Rotate(RightLegRotation(animstate.interp), 0, 0, 1);
            GL.Translate(-0.3f / 2, -armsheight, 0);

            coords = MakeCoords(4, 8, 4, 40, 16);
            MakeTextureCoords(coords, 64, 32);
            DrawCube(new Vector3(0, 0 + legsheight, -0.3f), new Vector3(0.3f, 0.9f, 0.3f), playertexture, coords);

            GL.PopMatrix();
            //right arm
            GL.PushMatrix();
            GL.Translate(0.3f / 2, armsheight, 0);
            GL.Rotate(LeftLegRotation(animstate.interp), 0, 0, 1);
            GL.Translate(-0.3f / 2, -armsheight, 0);

            DrawCube(new Vector3(0, 0 + legsheight, 0.6f), new Vector3(0.3f, 0.9f, 0.3f), playertexture, coords);

            GL.PopMatrix();

            GL.PopMatrix();
        }
        private float Normalize(float p)
        {
            return (float)(p % (Math.PI / 8));
        }
        float UpDown(float time)
        {
            float jumpheight = 0.10f;
            return (float)TriSin(time * 16) * jumpheight + jumpheight / 2;
        }
        float LeftLegRotation(float time)
        {
            return (float)TriSin(time * 8) * 90;
        }
        float RightLegRotation(float time)
        {
            return (float)TriSin(time * 8 + Math.PI) * 90;
        }
        private float TriSin(double t)
        {
            double period = 2 * Math.PI;
            t += Math.PI / 2;
            return (float)Math.Abs(2f * (t / period - Math.Floor(t / period + 0.5f))) * 2 - 1;
        }
        /*
        float LeftLegRotation(float time)
        {
            return (float)Math.Sin(time * 8) * 90;
        }
        float RightLegRotation(float time)
        {
            return (float)Math.Sin(time * 8 + Math.PI) * 90;
        }
        */
        public RectangleF[] MakeCoords(float tsizex, float tsizey, float tsizez, float tstartx, float tstarty)
        {
            RectangleF[] coords = new[]
            {
                new RectangleF(tsizez+tstartx,tsizez+tstarty,tsizex,tsizey),//front
                new RectangleF(2*tsizez+tsizex+tstartx,tsizez+tstarty,tsizex,tsizey),//back
                new RectangleF(0+tstartx,tsizez+tstarty,tsizez,tsizey),//left
                new RectangleF(tsizez+tsizex+tstartx,tsizez+tstarty,tsizez,tsizey),//right
                new RectangleF(tsizez+tstartx,0+tstarty,tsizex,tsizez),//top
                new RectangleF(tsizez+tsizex+tstartx,0+tstarty,tsizex,tsizez),//bottom
            };
            return coords;
        }
        public static void MakeTextureCoords(RectangleF[] coords, float texturewidth, float textureheight)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                coords[i] = new RectangleF((coords[i].X / (float)texturewidth), (coords[i].Y / (float)textureheight),
                    (coords[i].Width / (float)texturewidth), (coords[i].Height / (float)textureheight));
            }
        }
        public void DrawCube(Vector3 pos, Vector3 size, int textureid, RectangleF[] texturecoords)
        {
            //front
            //GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, textureid);
            GL.Disable(EnableCap.CullFace);
            GL.Begin(BeginMode.Quads);
            RectangleF rect;
            //front
            rect = texturecoords[0];
            GL.TexCoord2(rect.X, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y, pos.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Y);
            GL.Vertex3(pos.X, pos.Y + size.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X, rect.Y);
            GL.Vertex3(pos.X, pos.Y + size.Y, pos.Z);
            //back
            rect = texturecoords[1];
            GL.TexCoord2(rect.X, rect.Bottom);
            GL.Vertex3(pos.X + size.X, pos.Y, pos.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Bottom);
            GL.Vertex3(pos.X + size.X, pos.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y + size.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y + size.Y, pos.Z);
            //left
            rect = texturecoords[2];
            GL.TexCoord2(rect.X, rect.Bottom);
            GL.Vertex3(pos.X + size.X, pos.Y, pos.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y, pos.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Y);
            GL.Vertex3(pos.X, pos.Y + size.Y, pos.Z);
            GL.TexCoord2(rect.X, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y + size.Y, pos.Z);
            //right
            rect = texturecoords[3];
            GL.TexCoord2(rect.X + rect.Width, rect.Bottom);
            GL.Vertex3(pos.X + size.X, pos.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X, rect.Y);
            GL.Vertex3(pos.X, pos.Y + size.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y + size.Y, pos.Z + size.Z);
            //top
            rect = texturecoords[4];
            GL.TexCoord2(rect.X, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y + size.Y, pos.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y + size.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y + size.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y + size.Y, pos.Z);
            //bottom
            rect = texturecoords[5];
            GL.TexCoord2(rect.X, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y, pos.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Bottom);
            GL.Vertex3(pos.X, pos.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X + rect.Width, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y, pos.Z + size.Z);
            GL.TexCoord2(rect.X, rect.Y);
            GL.Vertex3(pos.X + size.X, pos.Y, pos.Z);

            GL.End();
            GL.Enable(EnableCap.CullFace);
        }
    }
}

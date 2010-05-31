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
        void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt, int playertexture);
    }
    public class CharacterDrawerBlock : ICharacterDrawer
    {
        [Inject]
        public IGetFilePath getfile { get; set; }
        public void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt, int playertexture)
        {
            RectangleF[] coords;
            float legsheight = 0.9f;
            float armsheight = legsheight + 0.9f;
            if (moves)
            {
                animstate.interp += dt;
            }
            else
            {
                animstate.interp = 0;
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
        RectangleF[] MakeCoords(float tsizex, float tsizey, float tsizez, float tstartx, float tstarty)
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
        private static void MakeTextureCoords(RectangleF[] coords, float texturewidth, float textureheight)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                coords[i] = new RectangleF((coords[i].X / (float)texturewidth), (coords[i].Y / (float)textureheight),
                    (coords[i].Width / (float)texturewidth), (coords[i].Height / (float)textureheight));
            }
        }
        private void DrawCube(Vector3 pos, Vector3 size, int textureid, RectangleF[] texturecoords)
        {
            //front
            GL.Color3(Color.White);
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
    public class CharacterDrawerMd2 : ICharacterDrawer
    {
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IThe3d the3d { get; set; }
        #region ICharacterDrawer Members
        public void DrawCharacter(AnimationState animstate, Vector3 pos, byte heading, byte pitch, bool moves, float dt, int playertexture)
        {
            if (m1 == null)
            {
                m1 = new Md2Engine.Mesh();
                m1.loadMD2(getfile.GetFile("player.md2"));
                m1texture = the3d.LoadTexture(getfile.GetFile("player.png"));
            }
            pos += new Vector3(0.5f, 0, 0.5f);
            string anim = moves ? "run" : "stand";
            Md2Engine.Range tmp = m1.animationPool[m1.findAnim(anim)];
            animstate.frame = Animate(animstate, tmp.getStart(), tmp.getEnd(), animstate.frame, dt);
            GL.PushMatrix();
            GL.Translate(pos);
            GL.Rotate(-90, 1, 0, 0);
            GL.Rotate((-((float)heading / 256)) * 360 + 90, 0, 0, 1);
            GL.Scale(0.05f, 0.05f, 0.05f);
            GL.BindTexture(TextureTarget.Texture2D, m1texture);
            md2renderer.RenderFrameImmediateInterpolated(animstate.frame, animstate.interp, m1, true, tmp.getStart(), tmp.getEnd());
            GL.PopMatrix();
            GL.FrontFace(FrontFaceDirection.Ccw);
        }
        #endregion
        int Animate(AnimationState anim, int start, int end, int frame, float dt)//update the animation parameters
        {
            anim.interp += dt * 5;

            if (anim.interp > 1.0f)
            {
                anim.interp = 0.0f;
                frame++;
            }

            if ((frame < start) || (frame >= end))
                frame = start;

            return frame;
        }
        class OpentkGl : Md2Engine.IOpenGl
        {
            #region IOpenGl Members
            public void GlBegin(Md2Engine.BeginMode mode)
            {
                if (mode == Md2Engine.BeginMode.Triangles)
                {
                    GL.Begin(BeginMode.Triangles);
                }
                else if (mode == Md2Engine.BeginMode.TriangleFan)
                {
                    GL.Begin(BeginMode.TriangleFan);
                }
                else if (mode == Md2Engine.BeginMode.TriangleStrip)
                {
                    GL.Begin(BeginMode.TriangleStrip);
                }
                else
                {
                    throw new Exception();
                }
            }
            public void GlEnd()
            {
                GL.End();
            }
            public void GlFrontFace(Md2Engine.FrontFace mode)
            {
                if (mode == Md2Engine.FrontFace.Cw)
                {
                    GL.FrontFace(FrontFaceDirection.Cw);
                }
                else if (mode == Md2Engine.FrontFace.Ccw)
                {
                    GL.FrontFace(FrontFaceDirection.Ccw);
                }
                else
                {
                    throw new Exception();
                }
            }
            public void GlNormal3f(float x, float y, float z)
            {
                GL.Normal3(x, y, z);
            }
            public void GlTexCoord2f(float x, float y)
            {
                GL.TexCoord2(x, y);
            }
            public void GlVertex3f(float x, float y, float z)
            {
                GL.Vertex3(x, y, z);
            }
            #endregion
        }
        Md2Engine.GlRenderer md2renderer = new Md2Engine.GlRenderer() { gl = new OpentkGl() };
        Md2Engine.Mesh m1;
        int m1texture;
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using CsGL.OpenGL;
using System.Diagnostics;
using Md2Engine;

namespace Md2Viewer
{
    class OGLView : OpenGLControl
    {
        public string model,texture;

        protected CsGL.OpenGL.OpenGLTexture2D Texture; //texture
        public Mesh m1; //the mesh

        protected Boolean wire;
        protected Boolean smooth;   //render mode
        protected Boolean textured;
        protected Boolean light;

        protected float r, g, b;//color
        protected float sca,rotz;//scale and rotate

        protected int n;//current frame
        protected float interp;//interpolation value

        protected string anim; //the current animation range

        public void clearMesh()
        {
            m1.animationPool.Clear();
            m1.framePool.Clear();
            m1.trianglePool.Clear();
            m1.texcoordPool.Clear();
        }

        public float getSca()
        {
            return sca;
        }

        public void setSca(float ns) //set scale factor
        {
            sca = ns;
            updateGLContext();
        }

        public float getRotz()
        {
            return rotz;
        }

        public void setRotz(float nr) //set rotation on Z axis
        {
            rotz = nr;
            updateGLContext();
        }

        public void setAnim(string pa) //set current animation range
        {
            anim = pa;
        }

        public void loadTexture(string path, bool flip) //load a texture
        {
            Bitmap tex = new Bitmap(path);
            if (flip)
            {
                tex.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            Texture = new OpenGLTexture2D(tex);
        }

        public void loadMeshASC(string path) //load an asc mesh
        {
            m1 = new Mesh();
            m1.loadASC(path);
        }

        public void loadMeshMD2(string path) //load a MD2 mesh
        {
            m1 = new Mesh();
            m1.loadMD2(path);
        }

        // SET RENDER MODES and OPTIONS

        public void drawWireframe()
        {
            wire = true;
            updateGLContext();
        }

        public void drawFilled()
        {
            wire = false;
            updateGLContext();
        }

        public void drawSmooth()
        {
            smooth = true;
            updateGLContext();
        }

        public void drawFlat()
        {
            smooth = false;
            updateGLContext();
        }

        public void drawTextured(Boolean dt)
        {
            textured = dt;
            updateGLContext();
        }

        public void drawLights(Boolean dl)
        {
            light = dl;
            updateGLContext();
        }

        public void setColor(float pr, float pg, float pb) //set the ambient color
        {
            r = pr;
            g = pg;
            b = pb;
        }

        public int Animate(int start, int end, int frame)//update the animation parameters
        {
            interp += 0.1f;

            if (interp > 1.0f)
            {
                interp = 0.0f;
                frame++;
            }

            if ((frame < start) || (frame >= end))
                frame = start;

            return frame;
        }

        public void renderAnimation(string anim, Mesh mdl)//render the animation range
        {
            Range tmp = mdl.animationPool[mdl.findAnim(anim)];

            n = Animate(tmp.getStart(), tmp.getEnd(), n);
            renderer.RenderFrameImmediateInterpolated(n, interp, mdl, true, tmp.getStart(), tmp.getEnd());
        }

        GlRenderer renderer = new GlRenderer() { gl = new CsglOpenGl() };

        public void drawAxes()
        {
            //for (int i = -50; i <= 50; i++)
            {
                for (int j = -150; j <= 150; j++)
                {
                    GL.glBegin(GL.GL_LINES);
                        GL.glColor3f(0, 0, 1);
                        GL.glVertex3f(-150, -10, j);
                        GL.glVertex3f(150, -10, j);
                    GL.glEnd();
                    j += 5;
                }

                for (int j = -150; j <= 150; j++)
                {
                    GL.glBegin(GL.GL_LINES);
                    GL.glColor3f(0, 0, 1);
                    GL.glVertex3f(j, -10, -150);
                    GL.glVertex3f(j, -10, 150);
                    GL.glEnd();
                    j += 5;
                }
            }
        }

        public override void glDraw()
        {
            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            GL.glLoadIdentity();
            GL.glTranslatef(0.0f, 0.0f, -7.0f);

            GL.glDisable(GL.GL_TEXTURE_2D);
            drawAxes();
            drawTextured(textured);

            GL.glScalef(sca, sca, sca);
            GL.glRotatef(-80, 1, 0, 0);
            GL.glRotatef(rotz, 0, 0, 1);

            Texture.Bind();

            GL.glColor3f(r, g, b);
            renderAnimation(anim, m1);
        }

        public void updateGLContext()
        {
            if (wire)
            {
                GL.glPolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_LINE);
            }
            else
            {
                GL.glPolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            }

            if (smooth)
            {
                GL.glShadeModel(GL.GL_SMOOTH);
            }
            else
            {
                GL.glShadeModel(GL.GL_FLAT);
            }

            if (textured)
            {
                GL.glEnable(GL.GL_TEXTURE_2D);
            }
            else
            {
                GL.glDisable(GL.GL_TEXTURE_2D);
            }

            if (light)
            {
                GL.glEnable(GL.GL_LIGHTING);
            }
            else
            {
                GL.glDisable(GL.GL_LIGHTING);
            }
        }

        protected override void InitGLContext()
        {
            //loadTexture(Application.StartupPath + "\\data\\textures\\shuttle02.bmp", false);
            //loadMeshASC(Application.StartupPath + "\\data\\models\\shuttle02.asc");
            loadTexture(Application.StartupPath + "\\data\\textures\\model.bmp", true);
            loadMeshMD2(Application.StartupPath + "\\data\\models\\model.md2");

            GL.glEnable(GL.GL_DEPTH_TEST);
            GL.glDepthFunc(GL.GL_LESS);
            GL.glPushAttrib(GL.GL_POLYGON_BIT);
            GL.glEnable(GL.GL_CULL_FACE);
            GL.glCullFace(GL.GL_BACK);

            GL.glClearColor(0.6f, 0.6f, 0.6f, 0.5f);
            GL.glClearDepth(1.0f);
            GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);

            //lights
            float[] light_ambient= { 0.0f, 0.1f, 0.1f, 0.1f };
            float[] light_diffuse= { 1.0f, 1.0f, 1.0f, 0.0f };
            float[] light_specular= { 1.0f, 1.0f, 1.0f, 0.0f };

            float[] light_position= { 50.0f, 10.0f, -10.0f, 5.0f };

            GL.glLightfv(GL.GL_LIGHT1, GL.GL_AMBIENT, light_ambient);
            GL.glLightfv(GL.GL_LIGHT1, GL.GL_DIFFUSE, light_diffuse);
            GL.glLightfv(GL.GL_LIGHT1, GL.GL_SPECULAR, light_specular);
            GL.glLightfv(GL.GL_LIGHT1, GL.GL_POSITION, light_position);

            GL.glEnable(GL.GL_LIGHT1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Size s = Size;
            double aspect_ratio = (double)s.Width / (double)s.Height;

            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glLoadIdentity();
      
            GL.gluPerspective(45.0f, aspect_ratio, 1f, 100.0f);

            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glLoadIdentity();
        }
    }
    public class CsglOpenGl : IOpenGl
    {
        #region IOpenGl Members
        public void GlBegin(BeginMode mode)
        {
            if (mode == BeginMode.Triangles)
            {
                GL.glBegin(GL.GL_TRIANGLES);
            }
            else if (mode == BeginMode.TriangleFan)
            {
                GL.glBegin(GL.GL_TRIANGLE_FAN);
            }
            else if (mode == BeginMode.TriangleStrip)
            {
                GL.glBegin(GL.GL_TRIANGLE_STRIP);
            }
            else
            {
                throw new Exception();
            }
        }
        public void GlEnd()
        {
            GL.glEnd();
        }
        public void GlFrontFace(FrontFace mode)
        {
            if (mode == FrontFace.Cw)
            {
                GL.glFrontFace(GL.GL_CW);
            }
            else if (mode == FrontFace.Ccw)
            {
                GL.glFrontFace(GL.GL_CCW);
            }
            else
            {
                throw new Exception();
            }
        }
        public void GlNormal3f(float x, float y, float z)
        {
            GL.glNormal3f(x, y, z);
        }
        public void GlTexCoord2f(float x, float y)
        {
            GL.glTexCoord2f(x, y);
        }
        public void GlVertex3f(float x, float y, float z)
        {
            GL.glVertex3f(x, y, z);
        }
        #endregion
    }
}

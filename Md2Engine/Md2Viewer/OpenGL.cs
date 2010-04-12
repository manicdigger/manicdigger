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

        public void renderFrame(int n, Mesh mdl, bool strips) //render frame
        {
            if (!strips)
            {
                GL.glFrontFace(GL.GL_CCW);
                int va, vb, vc;
                int ta, tb, tc;

                GL.glBegin(GL.GL_TRIANGLES);
                for (int i = 0; i < mdl.trianglePool.Count; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == 0) 
                        {
                            va = mdl.trianglePool[i].getA();
                            ta = mdl.trianglePool[i].getTA();

                            GL.glTexCoord2f(mdl.texcoordPool[ta].getU(), mdl.texcoordPool[ta].getV());

                            GL.glNormal3f(mdl.framePool[n].normalsPool[va].getX(),
                                          mdl.framePool[n].normalsPool[va].getY(),
                                          mdl.framePool[n].normalsPool[va].getZ());

                            GL.glVertex3f(mdl.framePool[n].vertexPool[va].getX(),
                                          mdl.framePool[n].vertexPool[va].getY(),
                                          mdl.framePool[n].vertexPool[va].getZ());
                        }

                        if (j == 1)
                        {
                            vb = mdl.trianglePool[i].getB();
                            tb = mdl.trianglePool[i].getTB();

                            GL.glTexCoord2f(mdl.texcoordPool[tb].getU(), mdl.texcoordPool[tb].getV());

                            GL.glNormal3f(mdl.framePool[n].normalsPool[vb].getX(),
                                          mdl.framePool[n].normalsPool[vb].getY(),
                                          mdl.framePool[n].normalsPool[vb].getZ());

                            GL.glVertex3f(mdl.framePool[n].vertexPool[vb].getX(),
                                          mdl.framePool[n].vertexPool[vb].getY(),
                                          mdl.framePool[n].vertexPool[vb].getZ());
                        }

                        if (j == 2)
                        {
                            vc = mdl.trianglePool[i].getC();
                            tc = mdl.trianglePool[i].getTC();

                            GL.glTexCoord2f(mdl.texcoordPool[tc].getU(), mdl.texcoordPool[tc].getV());

                            GL.glNormal3f(mdl.framePool[n].normalsPool[vc].getX(),
                                          mdl.framePool[n].normalsPool[vc].getY(),
                                          mdl.framePool[n].normalsPool[vc].getZ());

                            GL.glVertex3f(mdl.framePool[n].vertexPool[vc].getX(),
                                          mdl.framePool[n].vertexPool[vc].getY(),
                                          mdl.framePool[n].vertexPool[vc].getZ());
                        }
                    }
                }
                GL.glEnd();
            }
            else
            {
                GL.glFrontFace(GL.GL_CW);
                Vertex vrt;
                Vector vct;

                for (int i = 0; i < mdl.glCommandPool.Count; i++)
                {
                    if (mdl.glCommandPool[i].getType() == 1)
                    {
                        GL.glBegin(GL.GL_TRIANGLE_STRIP);
                    }
                    if (mdl.glCommandPool[i].getType() == 2)
                    {
                        GL.glBegin(GL.GL_TRIANGLE_FAN);
                    }

                    for (int j = 0; j < mdl.glCommandPool[i].packets.Count; j++)
                    {
                        vrt = mdl.framePool[n].vertexPool[mdl.glCommandPool[i].packets[j].getVertex()];
                        vct = mdl.framePool[n].normalsPool[mdl.glCommandPool[i].packets[j].getVertex()];

                        GL.glTexCoord2f(mdl.glCommandPool[i].packets[j].getU(), mdl.glCommandPool[i].packets[j].getV());
                        GL.glNormal3f(vct.getX(), vct.getY(), vct.getZ());
                        GL.glVertex3f(vrt.getX(), vrt.getY(), vrt.getZ());
                    }

                    GL.glEnd();
                }
            }
        }

        public void renderFrameItp(int n, float interp, Mesh mdl, bool strips) //render the frame interpolated with the next to smooth transition
        {
            if (n < mdl.framePool.Count - 1)
            {
                if (!strips)
                {
                    GL.glFrontFace(GL.GL_CCW);
                    int va, vb, vc;
                    Vertex final = new Vertex();
                    Vector finv = new Vector();
                    int ta, tb, tc;

                    GL.glBegin(GL.GL_TRIANGLES);
                    for (int i = 0; i < mdl.trianglePool.Count; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (j == 0)
                            {
                                va = mdl.trianglePool[i].getA();

                                final.setXYZ(mdl.framePool[n].vertexPool[va].getX() + interp * (mdl.framePool[n + 1].vertexPool[va].getX() - mdl.framePool[n].vertexPool[va].getX()),
                                             mdl.framePool[n].vertexPool[va].getY() + interp * (mdl.framePool[n + 1].vertexPool[va].getY() - mdl.framePool[n].vertexPool[va].getY()),
                                             mdl.framePool[n].vertexPool[va].getZ() + interp * (mdl.framePool[n + 1].vertexPool[va].getZ() - mdl.framePool[n].vertexPool[va].getZ()));

                                finv.setX(mdl.framePool[n].normalsPool[va].getX() + interp * (mdl.framePool[n + 1].normalsPool[va].getX() - mdl.framePool[n].normalsPool[va].getX()));
                                finv.setY(mdl.framePool[n].normalsPool[va].getY() + interp * (mdl.framePool[n + 1].normalsPool[va].getY() - mdl.framePool[n].normalsPool[va].getY()));
                                finv.setZ(mdl.framePool[n].normalsPool[va].getZ() + interp * (mdl.framePool[n + 1].normalsPool[va].getZ() - mdl.framePool[n].normalsPool[va].getZ()));

                                GL.glNormal3f(finv.getX(), finv.getY(), finv.getZ());

                                ta = mdl.trianglePool[i].getTA();

                                GL.glTexCoord2f(mdl.texcoordPool[ta].getU(), mdl.texcoordPool[ta].getV());
                                GL.glVertex3f(final.getX(), final.getY(), final.getZ());
                            }

                            if (j == 1)
                            {
                                vb = mdl.trianglePool[i].getB();

                                final.setXYZ(mdl.framePool[n].vertexPool[vb].getX() + interp * (mdl.framePool[n + 1].vertexPool[vb].getX() - mdl.framePool[n].vertexPool[vb].getX()),
                                             mdl.framePool[n].vertexPool[vb].getY() + interp * (mdl.framePool[n + 1].vertexPool[vb].getY() - mdl.framePool[n].vertexPool[vb].getY()),
                                             mdl.framePool[n].vertexPool[vb].getZ() + interp * (mdl.framePool[n + 1].vertexPool[vb].getZ() - mdl.framePool[n].vertexPool[vb].getZ()));

                                finv.setX(mdl.framePool[n].normalsPool[vb].getX() + interp * (mdl.framePool[n + 1].normalsPool[vb].getX() - mdl.framePool[n].normalsPool[vb].getX()));
                                finv.setY(mdl.framePool[n].normalsPool[vb].getY() + interp * (mdl.framePool[n + 1].normalsPool[vb].getY() - mdl.framePool[n].normalsPool[vb].getY()));
                                finv.setZ(mdl.framePool[n].normalsPool[vb].getZ() + interp * (mdl.framePool[n + 1].normalsPool[vb].getZ() - mdl.framePool[n].normalsPool[vb].getZ()));

                                GL.glNormal3f(finv.getX(), finv.getY(), finv.getZ());

                                tb = mdl.trianglePool[i].getTB();

                                GL.glTexCoord2f(mdl.texcoordPool[tb].getU(), mdl.texcoordPool[tb].getV());
                                GL.glVertex3f(final.getX(), final.getY(), final.getZ());
                            }

                            if (j == 2)
                            {
                                vc = mdl.trianglePool[i].getC();

                                final.setXYZ(mdl.framePool[n].vertexPool[vc].getX() + interp * (mdl.framePool[n + 1].vertexPool[vc].getX() - mdl.framePool[n].vertexPool[vc].getX()),
                                             mdl.framePool[n].vertexPool[vc].getY() + interp * (mdl.framePool[n + 1].vertexPool[vc].getY() - mdl.framePool[n].vertexPool[vc].getY()),
                                             mdl.framePool[n].vertexPool[vc].getZ() + interp * (mdl.framePool[n + 1].vertexPool[vc].getZ() - mdl.framePool[n].vertexPool[vc].getZ()));

                                finv.setX(mdl.framePool[n].normalsPool[vc].getX() + interp * (mdl.framePool[n + 1].normalsPool[vc].getX() - mdl.framePool[n].normalsPool[vc].getX()));
                                finv.setY(mdl.framePool[n].normalsPool[vc].getY() + interp * (mdl.framePool[n + 1].normalsPool[vc].getY() - mdl.framePool[n].normalsPool[vc].getY()));
                                finv.setZ(mdl.framePool[n].normalsPool[vc].getZ() + interp * (mdl.framePool[n + 1].normalsPool[vc].getZ() - mdl.framePool[n].normalsPool[vc].getZ()));

                                GL.glNormal3f(finv.getX(), finv.getY(), finv.getZ());

                                tc = mdl.trianglePool[i].getTC();

                                GL.glTexCoord2f(mdl.texcoordPool[tc].getU(), mdl.texcoordPool[tc].getV());
                                GL.glVertex3f(final.getX(), final.getY(), final.getZ());
                            }
                        }
                    }
                    GL.glEnd();
                }
                else
                {
                    GL.glFrontFace(GL.GL_CW);
                    Vertex vrt;
                    Vector vct;

                    for (int i = 0; i < mdl.glCommandPool.Count; i++)
                    {
                        if (mdl.glCommandPool[i].getType() == 2)
                        {
                            GL.glBegin(GL.GL_TRIANGLE_STRIP);
                        }
                        if (mdl.glCommandPool[i].getType() == 1)
                        {
                            GL.glBegin(GL.GL_TRIANGLE_FAN);
                        }

                        for (int j = 0; j < mdl.glCommandPool[i].packets.Count; j++)
                        {
                            vrt = mdl.framePool[n].vertexPool[mdl.glCommandPool[i].packets[j].getVertex()];
                            vct = mdl.framePool[n].normalsPool[mdl.glCommandPool[i].packets[j].getVertex()];

                            GL.glTexCoord2f(mdl.glCommandPool[i].packets[j].getU(), mdl.glCommandPool[i].packets[j].getV());
                            GL.glNormal3f(vct.getX(), vct.getY(), vct.getZ());
                            GL.glVertex3f(vrt.getX(), vrt.getY(), vrt.getZ());    
                        }

                        GL.glEnd();
                    }
                }
            }
            else
            {
                renderFrame(n, mdl, strips); //if at last frame render it uninterpolated
            }
        }

        public int Animate(int start, int end, int frame)//update the animation parameters
        {
            frame++;
            interp += 0.01f;

            if (interp > 1.0f)
            {
                interp = 0.0f;
            }

            if ((frame < start) || (frame >= end))
                frame = start;

            return frame;
        }

        public void renderAnimation(string anim, Mesh mdl)//render the animation range
        {
            Range tmp = mdl.animationPool[mdl.findAnim(anim)];

            n = Animate(tmp.getStart(), tmp.getEnd(), n);
            renderFrameItp(n, interp, mdl, true);
        }

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
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Md2Engine
{
    public enum BeginMode
    {
        /// <summary>
        /// GL_TRIANGLES
        /// </summary>
        Triangles,
        /// <summary>
        /// GL_TRIANGLE_STRIP
        /// </summary>
        TriangleStrip,
        /// <summary>
        /// GL_TRIANGLE_FAN
        /// </summary>
        TriangleFan,
    }
    public enum FrontFace
    {
        /// <summary>
        /// GL_CW
        /// </summary>
        Cw,
        /// <summary>
        /// GL_CCW
        /// </summary>
        Ccw,
    }
    public interface IOpenGl
    {
        void GlFrontFace(FrontFace mode);
        void GlBegin(BeginMode mode);
        void GlNormal3f(float x, float y, float z);
        void GlTexCoord2f(float x, float y);
        void GlVertex3f(float x, float y, float z);
        void GlEnd();
    }
    public class InjectAttribute : Attribute
    {
    }
    public class GlRenderer
    {
        [Inject]
        public IOpenGl gl { get; set; }
        /// <summary>
        /// Render frame.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="mdl"></param>
        /// <param name="strips"></param>
        public void RenderFrameImmediate(int n, Mesh mdl, bool strips)
        {
            if (!strips)
            {
                gl.GlFrontFace(FrontFace.Ccw);
                int va, vb, vc;
                int ta, tb, tc;

                gl.GlBegin(BeginMode.Triangles);
                for (int i = 0; i < mdl.trianglePool.Count; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == 0)
                        {
                            va = mdl.trianglePool[i].getA();
                            ta = mdl.trianglePool[i].getTA();

                            gl.GlTexCoord2f(mdl.texcoordPool[ta].getU(), mdl.texcoordPool[ta].getV());

                            gl.GlNormal3f(mdl.framePool[n].normalsPool[va].getX(),
                                          mdl.framePool[n].normalsPool[va].getY(),
                                          mdl.framePool[n].normalsPool[va].getZ());

                            gl.GlVertex3f(mdl.framePool[n].vertexPool[va].getX(),
                                          mdl.framePool[n].vertexPool[va].getY(),
                                          mdl.framePool[n].vertexPool[va].getZ());
                        }

                        if (j == 1)
                        {
                            vb = mdl.trianglePool[i].getB();
                            tb = mdl.trianglePool[i].getTB();

                            gl.GlTexCoord2f(mdl.texcoordPool[tb].getU(), mdl.texcoordPool[tb].getV());

                            gl.GlNormal3f(mdl.framePool[n].normalsPool[vb].getX(),
                                          mdl.framePool[n].normalsPool[vb].getY(),
                                          mdl.framePool[n].normalsPool[vb].getZ());

                            gl.GlVertex3f(mdl.framePool[n].vertexPool[vb].getX(),
                                          mdl.framePool[n].vertexPool[vb].getY(),
                                          mdl.framePool[n].vertexPool[vb].getZ());
                        }

                        if (j == 2)
                        {
                            vc = mdl.trianglePool[i].getC();
                            tc = mdl.trianglePool[i].getTC();

                            gl.GlTexCoord2f(mdl.texcoordPool[tc].getU(), mdl.texcoordPool[tc].getV());

                            gl.GlNormal3f(mdl.framePool[n].normalsPool[vc].getX(),
                                          mdl.framePool[n].normalsPool[vc].getY(),
                                          mdl.framePool[n].normalsPool[vc].getZ());

                            gl.GlVertex3f(mdl.framePool[n].vertexPool[vc].getX(),
                                          mdl.framePool[n].vertexPool[vc].getY(),
                                          mdl.framePool[n].vertexPool[vc].getZ());
                        }
                    }
                }
                gl.GlEnd();
            }
            else
            {
                gl.GlFrontFace(FrontFace.Cw);
                Vertex vrt;
                Vector vct;

                for (int i = 0; i < mdl.glCommandPool.Count; i++)
                {
                    if (mdl.glCommandPool[i].getType() == 1)
                    {
                        gl.GlBegin(BeginMode.TriangleStrip);
                    }
                    if (mdl.glCommandPool[i].getType() == 2)
                    {
                        gl.GlBegin(BeginMode.TriangleFan);
                    }

                    for (int j = 0; j < mdl.glCommandPool[i].packets.Count; j++)
                    {
                        vrt = mdl.framePool[n].vertexPool[mdl.glCommandPool[i].packets[j].getVertex()];
                        vct = mdl.framePool[n].normalsPool[mdl.glCommandPool[i].packets[j].getVertex()];

                        gl.GlTexCoord2f(mdl.glCommandPool[i].packets[j].getU(), mdl.glCommandPool[i].packets[j].getV());
                        gl.GlNormal3f(vct.getX(), vct.getY(), vct.getZ());
                        gl.GlVertex3f(vrt.getX(), vrt.getY(), vrt.getZ());
                    }

                    gl.GlEnd();
                }
            }
        }
        /// <summary>
        /// Render the frame interpolated with the next to smooth transition.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="interp"></param>
        /// <param name="mdl"></param>
        /// <param name="strips"></param>
        public void RenderFrameImmediateInterpolated(int n, float interp, Mesh mdl, bool strips, int startframe, int endframe)
        {
            if (n < mdl.framePool.Count - 1)
            {
                if (!strips)
                {
                    gl.GlFrontFace(FrontFace.Ccw);
                    int va, vb, vc;
                    Vertex final = new Vertex();
                    Vector finv = new Vector();
                    int ta, tb, tc;

                    gl.GlBegin(BeginMode.Triangles);
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

                                gl.GlNormal3f(finv.getX(), finv.getY(), finv.getZ());

                                ta = mdl.trianglePool[i].getTA();

                                gl.GlTexCoord2f(mdl.texcoordPool[ta].getU(), mdl.texcoordPool[ta].getV());
                                gl.GlVertex3f(final.getX(), final.getY(), final.getZ());
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

                                gl.GlNormal3f(finv.getX(), finv.getY(), finv.getZ());

                                tb = mdl.trianglePool[i].getTB();

                                gl.GlTexCoord2f(mdl.texcoordPool[tb].getU(), mdl.texcoordPool[tb].getV());
                                gl.GlVertex3f(final.getX(), final.getY(), final.getZ());
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

                                gl.GlNormal3f(finv.getX(), finv.getY(), finv.getZ());

                                tc = mdl.trianglePool[i].getTC();

                                gl.GlTexCoord2f(mdl.texcoordPool[tc].getU(), mdl.texcoordPool[tc].getV());
                                gl.GlVertex3f(final.getX(), final.getY(), final.getZ());
                            }
                        }
                    }
                    gl.GlEnd();
                }
                else
                {
                    gl.GlFrontFace(FrontFace.Cw);
                    Vertex vrt;
                    Vector vct;

                    for (int i = 0; i < mdl.glCommandPool.Count; i++)
                    {
                        if (mdl.glCommandPool[i].getType() == 2)
                        {
                            gl.GlBegin(BeginMode.TriangleStrip);
                        }
                        if (mdl.glCommandPool[i].getType() == 1)
                        {
                            gl.GlBegin(BeginMode.TriangleFan);
                        }

                        for (int j = 0; j < mdl.glCommandPool[i].packets.Count; j++)
                        {
                            vrt = mdl.framePool[n].vertexPool[mdl.glCommandPool[i].packets[j].getVertex()];
                            vct = mdl.framePool[n].normalsPool[mdl.glCommandPool[i].packets[j].getVertex()];

                            //loop animation
                            var vrt2 = mdl.framePool[n + 1].vertexPool[mdl.glCommandPool[i].packets[j].getVertex()];
                            if (n + 1 >= endframe)
                            {
                                vrt2 = mdl.framePool[startframe].vertexPool[mdl.glCommandPool[i].packets[j].getVertex()];
                            }
                            var vct2 = mdl.framePool[n + 1].normalsPool[mdl.glCommandPool[i].packets[j].getVertex()];
                            if (n + 1 >= endframe)
                            {
                                vct2 = mdl.framePool[startframe].normalsPool[mdl.glCommandPool[i].packets[j].getVertex()];
                            }

                            gl.GlTexCoord2f(mdl.glCommandPool[i].packets[j].getU(), mdl.glCommandPool[i].packets[j].getV());

                            gl.GlNormal3f(vct.getX() + interp * (vct2.getX() - vct.getX()),
                                vct.getY() + interp * (vct2.getY() - vct.getY()),
                                vct.getZ() + interp * (vct2.getZ() - vct.getZ()));
                            gl.GlVertex3f(vrt.getX() + interp * (vrt2.getX() - vrt.getX()),
                                vrt.getY() + interp * (vrt2.getY() - vrt.getY()),
                                vrt.getZ() + interp * (vrt2.getZ() - vrt.getZ()));
                        }

                        gl.GlEnd();
                    }
                }
            }
            else
            {
                //renderFrame(n, mdl, strips); //if at last frame render it uninterpolated
            }
        }
    }
}

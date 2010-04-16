using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace ManicDigger
{
    public interface IOpenGl
    {
        void Enable(EnableCap enableCap);
        void Color3(System.Drawing.Color color);
        void BindTexture(TextureTarget glTextureTarget, int p);
        void Begin(BeginMode glBeginMode);
        void End();
        void TexCoord2(float x, float y);
        void Vertex3(int x, float y, int z);
        void Vertex3(float x, float y, float z);
        int GenLists(int nlists);
        void EndList();
        void CallList(int p);
        void NewList(int p, ListMode glListMode);
        void DeleteLists(int lists, int nlists);
        int GenTexture();
        string GetString(StringName stringName);
        void ClearColor(System.Drawing.Color color);
        void DepthMask(bool p);
        void TexParameter(TextureTarget glTextureTarget, TextureParameterName glTextureParameterName, int p);
        void TexImage2D(TextureTarget textureTarget, int p, PixelInternalFormat pixelInternalFormat, int p_4, int p_5, int p_6, PixelFormat pixelFormat, PixelType pixelType, IntPtr intPtr);
        void AlphaFunc(AlphaFunction alphaFunction, float p);
        void CullFace(CullFaceMode glCullFaceMode);
        void Viewport(int p, int p_2, int Width, int Height);
        void MatrixMode(MatrixMode matrixMode);
        void LoadMatrix(ref OpenTK.Matrix4 perpective);
        void Clear(ClearBufferMask p);
        void PushMatrix();
        void LoadIdentity();
        void Translate(int p, float p_2, float p_3);
        void Rotate(float p, OpenTK.Vector3 vector3);
        void PopMatrix();
        void Vertex3(OpenTK.Vector3 vector3);
        void Translate(OpenTK.Vector3 pos);
        void Rotate(int p, int p_2, int p_3, int p_4);
        void Scale(float p, float p_2, float p_3);
        void FrontFace(FrontFaceDirection frontFaceDirection);
        void Rotate(float p, int p_2, int p_3, int p_4);
        void DeleteTexture(int p);
        void Vertex2(float x2, float y1);
        void Disable(EnableCap enableCap);
        void Ortho(int p, int Width, int Height, int p_4, int p_5, int p_6);
        void LineWidth(int p);
    }
    public class OpenGlDummy : IOpenGl
    {
        public void End()
        {
        }
        public void TexCoord2(float x, float y)
        {
        }
        public void Vertex3(int x, float y, int z)
        {
        }
        public void Vertex3(float x, float y, float z)
        {
        }
        public int GenLists(int nlists)
        {
            return 1;
        }
        public void EndList()
        {
        }
        public void CallList(int p)
        {
        }
        public void DeleteLists(int lists, int nlists)
        {
        }
        public int GenTexture()
        {
            return 1;
        }
        public string GetString(StringName stringName)
        {
            return "";
        }
        public void ClearColor(System.Drawing.Color color)
        {
        }
        public void DepthMask(bool p)
        {
        }
        public void Enable(EnableCap enableCap)
        {
        }
        public void Viewport(int p, int p_2, int Width, int Height)
        {
        }
        public void MatrixMode(MatrixMode matrixMode)
        {
        }
        public void LoadMatrix(ref OpenTK.Matrix4 perpective)
        {
        }
        public void Clear(ClearBufferMask p)
        {
        }
        public void PushMatrix()
        {
        }
        public void LoadIdentity()
        {
        }
        public void Translate(int p, float p_2, float p_3)
        {
        }
        public void Rotate(float p, OpenTK.Vector3 vector3)
        {
        }
        public void PopMatrix()
        {
        }
        public void Vertex3(OpenTK.Vector3 vector3)
        {
        }
        public void Translate(OpenTK.Vector3 pos)
        {
        }
        public void Rotate(int p, int p_2, int p_3, int p_4)
        {
        }
        public void Scale(float p, float p_2, float p_3)
        {
        }
        public void FrontFace(FrontFaceDirection frontFaceDirection)
        {
        }
        public void Rotate(float p, int p_2, int p_3, int p_4)
        {
        }
        public void DeleteTexture(int p)
        {
        }
        public void Vertex2(float x2, float y1)
        {
        }
        public void Ortho(int p, int Width, int Height, int p_4, int p_5, int p_6)
        {
        }
        public void LineWidth(int p)
        {
        }
        public void BindTexture(TextureTarget textureTarget, int p)
        {
        }
        public void Begin(BeginMode beginMode)
        {
        }
        public void Disable(EnableCap enableCap)
        {
        }
        public void NewList(int p, ListMode glListMode)
        {
        }
        public void TexParameter(TextureTarget glTextureTarget, TextureParameterName glTextureParameterName, int p)
        {
        }
        public void TexImage2D(TextureTarget textureTarget, int p, PixelInternalFormat pixelInternalFormat, int p_4, int p_5, int p_6, PixelFormat pixelFormat, PixelType pixelType, IntPtr intPtr)
        {
        }
        public void AlphaFunc(AlphaFunction alphaFunction, float p)
        {
        }
        public void CullFace(CullFaceMode glCullFaceMode)
        {
        }
        public void Color3(System.Drawing.Color color)
        {
        }
    }
    public class OpenGlOpenTk : IOpenGl
    {
        public void Color3(System.Drawing.Color color)
        {
            GL.Color3(color);
        }
        public void End()
        {
            GL.End();
        }
        public void TexCoord2(float x, float y)
        {
            GL.TexCoord2(x, y);
        }
        public void Vertex3(int x, float y, int z)
        {
            GL.Vertex3(x, y, z);
        }
        public void Vertex3(float x, float y, float z)
        {
            GL.Vertex3(x, y, z);
        }
        public int GenLists(int nlists)
        {
            return GL.GenLists(nlists);
        }
        public void NewList(int p, ListMode listMode)
        {
            GL.NewList(p, listMode);
        }
        public void EndList()
        {
            GL.EndList();
        }
        public void CallList(int p)
        {
            GL.CallList(p);
        }
        public void DeleteLists(int lists, int nlists)
        {
            GL.DeleteLists(lists, nlists);
        }
        public int GenTexture()
        {
            return GL.GenTexture();
        }
        public string GetString(StringName stringName)
        {
            return GL.GetString(stringName);
        }
        public void ClearColor(System.Drawing.Color color)
        {
            GL.ClearColor(color);
        }
        public void DepthMask(bool p)
        {
            GL.DepthMask(p);
        }
        public void Enable(EnableCap enableCap)
        {
            GL.Enable(enableCap);
        }
        public void TexParameter(TextureTarget glTextureTarget, TextureParameterName glTextureParameterName, int p)
        {
            GL.TexParameter(glTextureTarget, glTextureParameterName, p);
        }
        public void TexImage2D(TextureTarget textureTarget, int p, PixelInternalFormat pixelInternalFormat, int p_4, int p_5, int p_6, PixelFormat pixelFormat, PixelType pixelType, IntPtr intPtr)
        {
            GL.TexImage2D(textureTarget, p, pixelInternalFormat, p_4, p_5, p_6, pixelFormat, pixelType, intPtr);
        }
        public void AlphaFunc(AlphaFunction alphaFunction, float p)
        {
            GL.AlphaFunc(alphaFunction, p);
        }
        public void CullFace(CullFaceMode glCullFaceMode)
        {
            GL.CullFace(glCullFaceMode);
        }
        public void Viewport(int p, int p_2, int Width, int Height)
        {
            GL.Viewport(p, p_2, Width, Height);
        }
        public void MatrixMode(MatrixMode matrixMode)
        {
            GL.MatrixMode(matrixMode);
        }
        public void LoadMatrix(ref OpenTK.Matrix4 perpective)
        {
            GL.LoadMatrix(ref perpective);
        }
        public void Clear(ClearBufferMask p)
        {
            GL.Clear(p);
        }
        public void PushMatrix()
        {
            GL.PushMatrix();
        }
        public void LoadIdentity()
        {
            GL.LoadIdentity();
        }
        public void Translate(int p, float p_2, float p_3)
        {
            GL.Translate(p, p_2, p_3);
        }
        public void Rotate(float p, OpenTK.Vector3 vector3)
        {
            GL.Rotate(p, vector3);
        }
        public void PopMatrix()
        {
            GL.PopMatrix();
        }
        public void Vertex3(OpenTK.Vector3 vector3)
        {
            GL.Vertex3(vector3);
        }
        public void Translate(OpenTK.Vector3 pos)
        {
            GL.Translate(pos);
        }
        public void Rotate(int p, int p_2, int p_3, int p_4)
        {
            GL.Rotate(p, p_2, p_3, p_4);
        }
        public void Scale(float p, float p_2, float p_3)
        {
            GL.Scale(p, p_2, p_3);
        }
        public void FrontFace(FrontFaceDirection frontFaceDirection)
        {
            GL.FrontFace(frontFaceDirection);
        }
        public void Rotate(float p, int p_2, int p_3, int p_4)
        {
            GL.Rotate(p, p_2, p_3, p_4);
        }
        public void DeleteTexture(int p)
        {
            GL.DeleteTexture(p);
        }
        public void Vertex2(float x, float y)
        {
            GL.Vertex2(x, y);
        }
        public void Disable(EnableCap enableCap)
        {
            GL.Disable(enableCap);
        }
        public void Ortho(int left, int right, int bottom, int top, int zNear, int zFar)
        {
            GL.Ortho(left, right, bottom, top, zNear, zFar);
        }
        public void LineWidth(int p)
        {
            GL.LineWidth(p);
        }
        public void BindTexture(TextureTarget textureTarget, int p)
        {
            GL.BindTexture(textureTarget, p);
        }
        public void Begin(BeginMode beginMode)
        {
            GL.Begin(beginMode);
        }
    }
    class Md2EngineOpentkGl : Md2Engine.IOpenGl
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
}
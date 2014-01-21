using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace CitoGl
{
    public class OpenTkWebGlShader : WebGLShader
    {
        public int value;
    }

    public class OpenTkWebGlProgram : WebGLProgram
    {
        public int value;
    }

    public class OpenTkWebGLUniformLocation : WebGLUniformLocation
    {
        public int value;
    }

    public class OpenTkWebGLBuffer : WebGLBuffer
    {
        public int value;
    }

    public class OpenTkWebGLTexture : WebGLTexture
    {
        public int value;
    }

    public class OpenTkHTMLImageElement : HTMLImageElement
    {
        ImageOnLoadHandler h;

        public override ImageOnLoadHandler GetOnLoad()
        {
            return h;
        }

        public override void SetOnLoad(ImageOnLoadHandler handler)
        {
            h = handler;
        }

        string src;

        public override string GetSrc()
        {
            return src;
        }

        public override void SetSrc(string p)
        {
            src = p;
            try
            {
                bitmap = new Bitmap(p);
            }
            catch
            {
            }
            h.OnLoad();
        }

        internal Bitmap bitmap;
    }

    public class OpenTk : Gl
    {
        public override void Start()
        {
            GraphicsMode mode = new GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 24, 0, 2, new OpenTK.Graphics.ColorFormat(32));

            using (MainWindow game = new MainWindow(mode))
            {
                window = game;
                game.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(game_KeyDown);
                game.Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(game_KeyUp);
                game.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(game_KeyPress);
                game.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
                game.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
                game.Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);
                game.Mouse.WheelChanged += new EventHandler<OpenTK.Input.MouseWheelEventArgs>(Mouse_WheelChanged);
                game.gl = this;
                game.Run(60);
            }
        }

        void Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            foreach (MouseEventHandler h in mouseEventHandlers)
            {
                MouseWheelEventArgs args = new MouseWheelEventArgs();
                args.SetDelta(e.Delta);
                args.SetDeltaPrecise(e.DeltaPrecise);
                h.OnMouseWheel(args);
            }
        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (MouseEventHandler h in mouseEventHandlers)
            {
                MouseEventArgs args = new MouseEventArgs();
                args.SetX(e.X);
                args.SetY(e.Y);
                args.SetButton((int)e.Button);
                h.OnMouseDown(args);
            }
        }
        
        void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (MouseEventHandler h in mouseEventHandlers)
            {
                MouseEventArgs args = new MouseEventArgs();
                args.SetX(e.X);
                args.SetY(e.Y);
                args.SetButton((int)e.Button);
                h.OnMouseUp(args);
            }
        }
        
        void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            foreach (MouseEventHandler h in mouseEventHandlers)
            {
                MouseEventArgs args = new MouseEventArgs();
                args.SetX(e.X);
                args.SetY(e.Y);
                h.OnMouseMove(args);
            }
        }

        void game_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            foreach (KeyEventHandler h in keyEventHandlers)
            {
                KeyPressEventArgs args = new KeyPressEventArgs();
                args.SetKeyChar((int)e.KeyChar);
                h.OnKeyPress(args);
            }
        }

        MainWindow window;
        
        void game_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            foreach (KeyEventHandler h in keyEventHandlers)
            {
                KeyEventArgs args = new KeyEventArgs();
                args.SetKeyCode(ToGlKey(e.Key));
                h.OnKeyDown(args);
            }
        }

        void game_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            foreach (KeyEventHandler h in keyEventHandlers)
            {
                KeyEventArgs args = new KeyEventArgs();
                args.SetKeyCode(ToGlKey(e.Key));
                h.OnKeyUp(args);
            }
        }

        int ToGlKey(Key key)
        {
            switch (key)
            {
                case Key.Left:
                    return GlKeys.Left;
                case Key.Up:
                    return GlKeys.Up;
                case Key.Right:
                    return GlKeys.Right;
                case Key.Down:
                    return GlKeys.Down;
                case Key.PageUp:
                    return GlKeys.PageUp;
                case Key.PageDown:
                    return GlKeys.PageDown;
            }
            return (int)key;
        }

        public override int DrawingBufferWidth()
        {
            throw new NotImplementedException();
        }

        public override int DrawingBufferHeight()
        {
            throw new NotImplementedException();
        }

        public override WebGLContextAttributes GetContextAttributes()
        {
            throw new NotImplementedException();
        }

        public override bool IsContextLost()
        {
            throw new NotImplementedException();
        }

        public override string[] GetSupportedExtensions(Int outCount)
        {
            throw new NotImplementedException();
        }

        public override GlObject GetExtension(string name)
        {
            throw new NotImplementedException();
        }

        public override void ActiveTexture(int texture)
        {
            GL.ActiveTexture((TextureUnit)texture);
        }

        public override void AttachShader(WebGLProgram program, WebGLShader shader)
        {
            OpenTkWebGlProgram program_ = (OpenTkWebGlProgram)program;
            OpenTkWebGlShader shader_ = (OpenTkWebGlShader)shader;
            GL.AttachShader(program_.value, shader_.value);
        }

        public override void BindAttribLocation(WebGLProgram program, int index, string name)
        {
            throw new NotImplementedException();
        }

        public override void BindBuffer(int target, WebGLBuffer buffer)
        {
            OpenTkWebGLBuffer buffer_ = (OpenTkWebGLBuffer)buffer;
            GL.BindBuffer((BufferTarget)target, buffer_.value);
        }

        public override void BindFramebuffer(int target, WebGLFramebuffer framebuffer)
        {
            throw new NotImplementedException();
        }

        public override void BindRenderbuffer(int target, WebGLRenderbuffer renderbuffer)
        {
            throw new NotImplementedException();
        }

        public override void BindTexture(int target, WebGLTexture texture)
        {
            OpenTkWebGLTexture texture_ = (OpenTkWebGLTexture)texture;
            int textureValue;
            if (texture_ != null)
            {
                textureValue = texture_.value;
            }
            else
            {
                textureValue = 0;
            }
            GL.BindTexture((TextureTarget)target, textureValue);
        }

        public override void BlendColor(float red, float green, float blue, float alpha)
        {
            throw new NotImplementedException();
        }

        public override void BlendEquation(int mode)
        {
            throw new NotImplementedException();
        }

        public override void BlendEquationSeparate(int modeRGB, int modeAlpha)
        {
            throw new NotImplementedException();
        }

        public override void BlendFunc(int sfactor, int dfactor)
        {
            throw new NotImplementedException();
        }

        public override void BlendFuncSeparate(int srcRGB, int dstRGB, int srcAlpha, int dstAlpha)
        {
            throw new NotImplementedException();
        }

        public override void BufferData1(int target, int size, int usage)
        {
            throw new NotImplementedException();
        }

        public override void BufferData2(int target, GlArrayBufferView data, int usage)
        {
            throw new NotImplementedException();
        }

        public override void BufferData3(int target, GlArrayBuffer data, int usage)
        {
            throw new NotImplementedException();
        }

        public override void BufferDataFloat(int target, float[] data, int usage)
        {
            GL.BufferData((BufferTarget)target, (IntPtr)(data.Length * sizeof(float)), data, (BufferUsageHint)usage);
        }

        public override void BufferDataUshort(int target, int[] data, int usage)
        {
            ushort[] dataUshort = new ushort[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                dataUshort[i] = (ushort)data[i];
            }
            GL.BufferData((BufferTarget)target, (IntPtr)(dataUshort.Length * sizeof(ushort)), dataUshort, (BufferUsageHint)usage);
        }

        public override void BufferSubData1(int target, int offset, GlArrayBufferView data)
        {
            throw new NotImplementedException();
        }

        public override void BufferSubData2(int target, int offset, GlArrayBuffer data)
        {
            throw new NotImplementedException();
        }

        public override int CheckFramebufferStatus(int target)
        {
            throw new NotImplementedException();
        }

        public override void Clear(int mask)
        {
            GL.Clear((ClearBufferMask)mask);
        }

        public override void ClearColor(float red, float green, float blue, float alpha)
        {
            GL.ClearColor(red, green, blue, alpha);
        }

        public override void ClearDepth(float depth)
        {
            throw new NotImplementedException();
        }

        public override void ClearStencil(int s)
        {
            throw new NotImplementedException();
        }

        public override void ColorMask(bool red, bool green, bool blue, bool alpha)
        {
            throw new NotImplementedException();
        }

        public override void CompileShader(WebGLShader shader)
        {
            OpenTkWebGlShader shader_ = (OpenTkWebGlShader)shader;
            GL.CompileShader(shader_.value);
        }

        public override void CompressedTexImage2D(int target, int level, int internalformat, int width, int height, int border, GlArrayBufferView data)
        {
            throw new NotImplementedException();
        }

        public override void CompressedTexSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, GlArrayBufferView data)
        {
            throw new NotImplementedException();
        }

        public override void CopyTexImage2D(int target, int level, int internalformat, int x, int y, int width, int height, int border)
        {
            throw new NotImplementedException();
        }

        public override void CopyTexSubImage2D(int target, int level, int xoffset, int yoffset, int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        public override WebGLBuffer CreateBuffer()
        {
            int[] buffers = new int[1];
            GL.GenBuffers(1, buffers);
            OpenTkWebGLBuffer b = new OpenTkWebGLBuffer();
            b.value = buffers[0];
            return b;
        }

        public override WebGLFramebuffer CreateFramebuffer()
        {
            throw new NotImplementedException();
        }

        public override WebGLProgram CreateProgram()
        {
            OpenTkWebGlProgram program = new OpenTkWebGlProgram();
            program.value = GL.CreateProgram();
            return program;
        }

        public override WebGLRenderbuffer CreateRenderbuffer()
        {
            throw new NotImplementedException();
        }

        public override WebGLShader CreateShader(int type)
        {
            OpenTkWebGlShader shader = new OpenTkWebGlShader();
            shader.value = GL.CreateShader((ShaderType)type);
            return shader;
        }

        public override WebGLTexture CreateTexture()
        {
            OpenTkWebGLTexture t = new OpenTkWebGLTexture();
            t.value = GL.GenTexture();
            return t;
        }

        public override void CullFace(int mode)
        {
            throw new NotImplementedException();
        }

        public override void DeleteBuffer(WebGLBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFramebuffer(WebGLFramebuffer framebuffer)
        {
            throw new NotImplementedException();
        }

        public override void DeleteProgram(WebGLProgram program)
        {
            throw new NotImplementedException();
        }

        public override void DeleteRenderbuffer(WebGLRenderbuffer renderbuffer)
        {
            throw new NotImplementedException();
        }

        public override void DeleteShader(WebGLShader shader)
        {
            throw new NotImplementedException();
        }

        public override void DeleteTexture(WebGLTexture texture)
        {
            throw new NotImplementedException();
        }

        public override void DepthFunc(int func)
        {
            throw new NotImplementedException();
        }

        public override void DepthMask(bool flag)
        {
            throw new NotImplementedException();
        }

        public override void DepthRange(float zNear, float zFar)
        {
            throw new NotImplementedException();
        }

        public override void DetachShader(WebGLProgram program, WebGLShader shader)
        {
            throw new NotImplementedException();
        }

        public override void Disable(int cap)
        {
            throw new NotImplementedException();
        }

        public override void DisableVertexAttribArray(int index)
        {
            throw new NotImplementedException();
        }

        public override void DrawArrays(int mode, int first, int count)
        {
            GL.DrawArrays((BeginMode)mode, first, count);
        }

        public override void DrawElements(int mode, int count, int type, int offset)
        {
            GL.DrawElements((BeginMode)mode, count, (DrawElementsType)type, offset);
        }

        public override void Enable(int cap)
        {
            GL.Enable((EnableCap)cap);
        }

        public override void EnableVertexAttribArray(int index)
        {
            GL.EnableVertexAttribArray(index);
        }

        public override void Finish()
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void FramebufferRenderbuffer(int target, int attachment, int renderbuffertarget, WebGLRenderbuffer renderbuffer)
        {
            throw new NotImplementedException();
        }

        public override void FramebufferTexture2D(int target, int attachment, int textarget, WebGLTexture texture, int level)
        {
            throw new NotImplementedException();
        }

        public override void FrontFace(int mode)
        {
            throw new NotImplementedException();
        }

        public override void GenerateMipmap(int target)
        {
            GL.GenerateMipmap((GenerateMipmapTarget)target);
        }

        public override WebGLActiveInfo GetActiveAttrib(WebGLProgram program, int index)
        {
            throw new NotImplementedException();
        }

        public override WebGLActiveInfo GetActiveUniform(WebGLProgram program, int index)
        {
            throw new NotImplementedException();
        }

        public override WebGLShader[] GetAttachedShaders(WebGLProgram program, Int outCount)
        {
            throw new NotImplementedException();
        }

        public override int GetAttribLocation(WebGLProgram program, string name)
        {
            OpenTkWebGlProgram program_ = (OpenTkWebGlProgram)program;
            return GL.GetAttribLocation(program_.value, name);
        }

        public override GlObject GetBufferParameter(int target, int pname)
        {
            throw new NotImplementedException();
        }

        public override GlObject GetParameter(int pname)
        {
            throw new NotImplementedException();
        }

        public override int GetError()
        {
            throw new NotImplementedException();
        }

        public override GlObject GetFramebufferAttachmentParameter(int target, int attachment, int pname)
        {
            throw new NotImplementedException();
        }

        public override string GetProgramParameter(WebGLProgram program, int pname)
        {
            OpenTkWebGlProgram program_ = (OpenTkWebGlProgram)program;
            string s;
            GL.GetShaderInfoLog(program_.value, out s);
            return s;
        }

        public override string GetProgramInfoLog(WebGLProgram program)
        {
            throw new NotImplementedException();
        }

        public override GlObject GetRenderbufferParameter(int target, int pname)
        {
            throw new NotImplementedException();
        }

        public override GlObject GetShaderParameter(WebGLShader shader, int pname)
        {
            throw new NotImplementedException();
        }

        public override WebGLShaderPrecisionFormat GetShaderPrecisionFormat(int shadertype, int precisiontype)
        {
            throw new NotImplementedException();
        }

        public override string GetShaderInfoLog(WebGLShader shader)
        {
            throw new NotImplementedException();
        }

        public override string GetShaderSource(WebGLShader shader)
        {
            throw new NotImplementedException();
        }

        public override GlObject GetTexParameter(int target, int pname)
        {
            throw new NotImplementedException();
        }

        public override GlObject GetUniform(WebGLProgram program, WebGLUniformLocation location)
        {
            throw new NotImplementedException();
        }

        public override WebGLUniformLocation GetUniformLocation(WebGLProgram program, string name)
        {
            OpenTkWebGlProgram program_ = (OpenTkWebGlProgram)program;
            OpenTkWebGLUniformLocation l = new OpenTkWebGLUniformLocation();
            l.value = GL.GetUniformLocation(program_.value, name);
            return l;
        }

        public override GlObject GetVertexAttrib(int index, int pname)
        {
            throw new NotImplementedException();
        }

        public override int GetVertexAttribOffset(int index, int pname)
        {
            throw new NotImplementedException();
        }

        public override void Hint(int target, int mode)
        {
            throw new NotImplementedException();
        }

        public override bool IsBuffer(WebGLBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override bool IsEnabled(int cap)
        {
            throw new NotImplementedException();
        }

        public override bool IsFramebuffer(WebGLFramebuffer framebuffer)
        {
            throw new NotImplementedException();
        }

        public override bool IsProgram(WebGLProgram program)
        {
            throw new NotImplementedException();
        }

        public override bool IsRenderbuffer(WebGLRenderbuffer renderbuffer)
        {
            throw new NotImplementedException();
        }

        public override bool IsShader(WebGLShader shader)
        {
            throw new NotImplementedException();
        }

        public override bool IsTexture(WebGLTexture texture)
        {
            throw new NotImplementedException();
        }

        public override void LineWidth(float width)
        {
            throw new NotImplementedException();
        }

        public override void LinkProgram(WebGLProgram program)
        {
            OpenTkWebGlProgram program_ = (OpenTkWebGlProgram)program;
            GL.LinkProgram(program_.value);
        }

        public override void PixelStorei(int pname, int param)
        {
        }

        public override void PolygonOffset(float factor, float units)
        {
            throw new NotImplementedException();
        }

        public override void ReadPixels(int x, int y, int width, int height, int format, int type, GlArrayBufferView pixels)
        {
            throw new NotImplementedException();
        }

        public override void RenderbufferStorage(int target, int internalformat, int width, int height)
        {
            throw new NotImplementedException();
        }

        public override void SampleCoverage(float value, bool invert)
        {
            throw new NotImplementedException();
        }

        public override void Scissor(int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        public override void ShaderSource(WebGLShader shader, string source)
        {
            OpenTkWebGlShader shader_ = (OpenTkWebGlShader)shader;
            source = source.Replace("precision mediump float;", "");
            GL.ShaderSource(shader_.value, source);
        }

        public override void StencilFunc(int func, int ref_, int mask)
        {
            throw new NotImplementedException();
        }

        public override void StencilFuncSeparate(int face, int func, int ref_, int mask)
        {
            throw new NotImplementedException();
        }

        public override void StencilMask(int mask)
        {
            throw new NotImplementedException();
        }

        public override void StencilMaskSeparate(int face, int mask)
        {
            throw new NotImplementedException();
        }

        public override void StencilOp(int fail, int zfail, int zpass)
        {
            throw new NotImplementedException();
        }

        public override void StencilOpSeparate(int face, int fail, int zfail, int zpass)
        {
            throw new NotImplementedException();
        }

        public override void TexImage2D(int target, int level, int internalformat,
                        int width, int height, int border, int format,
                        int type, GlArrayBufferView pixels)
        {
            throw new NotImplementedException();
        }

        public override void TexImage2DImageData(int target, int level, int internalformat,
                        int format, int type, ImageData pixels)
        {
            throw new NotImplementedException();
        }

        public override void TexImage2DImage(int target, int level, int internalformat,
                        int format, int type, HTMLImageElement image)
        {
            OpenTkHTMLImageElement image_ = (OpenTkHTMLImageElement)image;
            Bitmap bmp = image_.bitmap;
            if (bmp == null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Image not found: {0}", image.GetSrc()));
                return;
            }

            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D((TextureTarget)target, level, PixelInternalFormat.Rgba,
                bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, (PixelType)type, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);
        }

        public override void TexImage2DCanvas(int target, int level, int internalformat,
                        int format, int type, HTMLCanvasElement canvas)
        {
            throw new NotImplementedException();
        }

        public override void TexImage2DVideo(int target, int level, int internalformat,
                        int format, int type, HTMLVideoElement video)
        {
            throw new NotImplementedException();
        }

        public override void TexParameterf(int target, int pname, float param)
        {
            throw new NotImplementedException();
        }

        public override void TexParameteri(int target, int pname, int param)
        {
            GL.TexParameterI((TextureTarget)target, (TextureParameterName)pname, ref param);
        }

        public override void TexSubImage2D(int target, int level, int xoffset, int yoffset,
                           int width, int height,
                           int format, int type, GlArrayBufferView pixels)
        {
            throw new NotImplementedException();
        }

        public override void TexSubImage2DImageData(int target, int level, int xoffset, int yoffset,
                               int format, int type, ImageData pixels)
        {
            throw new NotImplementedException();
        }

        public override void TexSubImage2DImage(int target, int level, int xoffset, int yoffset,
                           int format, int type, HTMLImageElement image)
        {
            throw new NotImplementedException();
        }

        public override void TexSubImage2DCanvas(int target, int level, int xoffset, int yoffset,
                           int format, int type, HTMLCanvasElement canvas)
        {
            throw new NotImplementedException();
        }

        public override void TexSubImage2DVideo(int target, int level, int xoffset, int yoffset,
                                   int format, int type, HTMLVideoElement video)
        {
            throw new NotImplementedException();
        }

        public override void Uniform1f(WebGLUniformLocation location, float x)
        {
            throw new NotImplementedException();
        }

        public override void Uniform1fv(WebGLUniformLocation location, float[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform1i(WebGLUniformLocation location, int x)
        {
            OpenTkWebGLUniformLocation location_ = (OpenTkWebGLUniformLocation)location;
            GL.Uniform1(location_.value, x);
        }

        public override void Uniform1iv(WebGLUniformLocation location, int[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform2f(WebGLUniformLocation location, float x, float y)
        {
            throw new NotImplementedException();
        }

        public override void Uniform2fv(WebGLUniformLocation location, float[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform2i(WebGLUniformLocation location, int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void Uniform2iv(WebGLUniformLocation location, int[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform3f(WebGLUniformLocation location, float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        public override void Uniform3fv(WebGLUniformLocation location, float[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform3i(WebGLUniformLocation location, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public override void Uniform3iv(WebGLUniformLocation location, int[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform4fv(WebGLUniformLocation location, float[] v)
        {
            throw new NotImplementedException();
        }

        public override void Uniform4i(WebGLUniformLocation location, int x, int y, int z, int w)
        {
            throw new NotImplementedException();
        }

        public override void Uniform4iv(WebGLUniformLocation location, int[] v)
        {
            throw new NotImplementedException();
        }

        public override void UniformMatrix2fv(WebGLUniformLocation location, bool transpose, float[] value)
        {
            throw new NotImplementedException();
        }

        public override void UniformMatrix3fv(WebGLUniformLocation location, bool transpose, float[] value)
        {
            throw new NotImplementedException();
        }

        public override void UniformMatrix4fv(WebGLUniformLocation location, bool transpose, float[] value)
        {
            OpenTkWebGLUniformLocation location_ = (OpenTkWebGLUniformLocation)location;

            Matrix4 m = new Matrix4(value[0], value[1], value[2], value[3], value[4], value[5], value[6], value[7], value[8], value[9], value[10], value[11], value[12], value[13], value[14], value[15]);
            GL.UniformMatrix4(location_.value, transpose, ref m);
        }

        public override void UseProgram(WebGLProgram program)
        {
            OpenTkWebGlProgram program_ = (OpenTkWebGlProgram)program;
            GL.UseProgram(program_.value);
        }

        public override void ValidateProgram(WebGLProgram program)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib1f(int indx, float x)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib1fv(int indx, float[] values)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib2f(int indx, float x, float y)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib2fv(int indx, float[] values)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib3f(int indx, float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib3fv(int indx, float[] values)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib4f(int indx, float x, float y, float z, float w)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttrib4fv(int indx, float[] values)
        {
            throw new NotImplementedException();
        }

        public override void VertexAttribPointer(int indx, int size, int type, bool normalized, int stride, int offset)
        {
            GL.VertexAttribPointer(indx, size, (VertexAttribPointerType)type, normalized, stride, offset);
        }

        public override void Viewport(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }

        public override void AddOnNewFrame(NewFrameHandler handler)
        {
            newFrameHandlers.Add(handler);
        }

        public List<NewFrameHandler> newFrameHandlers = new List<NewFrameHandler>();

        public override HTMLImageElement CreateHTMLImageElement()
        {
            return new OpenTkHTMLImageElement();
        }

        public List<KeyEventHandler> keyEventHandlers = new List<KeyEventHandler>();

        public override void AddOnKeyEvent(KeyEventHandler handler)
        {
            keyEventHandlers.Add(handler);
        }

        public override int GetCanvasWidth()
        {
            return window.Width;
        }

        public override int GetCanvasHeight()
        {
            return window.Height;
        }

        public List<MouseEventHandler> mouseEventHandlers = new List<MouseEventHandler>();
        
        public override void AddOnMouseEvent(MouseEventHandler handler)
        {
            mouseEventHandlers.Add(handler);
        }
        
        public List<TouchEventHandler> touchEventHandlers = new List<TouchEventHandler>();

        public override void AddOnTouchEvent(TouchEventHandler handler)
        {
            touchEventHandlers.Add(handler);
        }

        public override void RequestPointerLock()
        {
            throw new NotImplementedException();
        }

        public override void ExitPointerLock()
        {
            throw new NotImplementedException();
        }

        public override bool IsPointerLockEnabled()
        {
            throw new NotImplementedException();
        }

        public override void RequestFullScreen()
        {
            throw new NotImplementedException();
        }

        public override void ExitFullScreen()
        {
            throw new NotImplementedException();
        }

        public override bool IsFullScreenEnabled()
        {
            throw new NotImplementedException();
        }
    }

    class MainWindow : GameWindow
    {
        public MainWindow(GraphicsMode mode)
            : base(800, 600, mode)
        {
            VSync = VSyncMode.Off;
            WindowState = WindowState.Normal;
        }

        public CitoGl.OpenTk gl;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (Keyboard[Key.Escape])
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            foreach (NewFrameHandler h in gl.newFrameHandlers)
            {
                NewFrameEventArgs args = new NewFrameEventArgs();
                args.SetDt((float)e.Time);
                h.OnNewFrame(args);
            }
            SwapBuffers();
        }
    }
}

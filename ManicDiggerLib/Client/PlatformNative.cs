using System.IO;
using ManicDigger;
using System.Collections.Generic;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Text;
using System.Net;
using OpenTK.Input;
using OpenTK;
using System.Diagnostics;

public class GamePlatformNative : GamePlatform
{
    public GamePlatformNative()
    {
        datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
        start.Start();
    }

    public GameWindowNative window;

    string[] datapaths;
    Dictionary<string, string> cache = new Dictionary<string, string>();
    Dictionary<string, string> remap = new Dictionary<string, string>();
    public override string GetFullFilePath(string filename)
    {
    retry:
        if (remap.ContainsKey(filename))
        {
            filename = remap[filename];
        }
        if (!cache.ContainsKey(filename))
        {
            foreach (string path in datapaths)
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }
                    foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            FileInfo f = new FileInfo(s);
                            cache[f.Name] = s;
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
        }
        string origfilename = filename;
        for (int i = 0; i < 2; i++)
        {
            if (cache.ContainsKey(filename)) { return cache[filename]; }

            string f1 = filename.Replace(".png", ".jpg");
            if (cache.ContainsKey(f1)) { remap[origfilename] = f1; goto retry; }

            string f2 = filename.Replace(".jpg", ".png");
            if (cache.ContainsKey(f2)) { remap[origfilename] = f2; goto retry; }

            string f3 = filename.Replace(".wav", ".ogg");
            if (cache.ContainsKey(f3)) { remap[origfilename] = f3; goto retry; }

            string f4 = filename.Replace(".ogg", ".wav");
            if (cache.ContainsKey(f4)) { remap[origfilename] = f4; goto retry; }

            filename = new FileInfo(filename).Name; //handles GetFile(GetFile(file)) use.
        }

        throw new FileNotFoundException(filename);
    }

    public override int FloatToInt(float value)
    {
        return (int)value;
    }

    public override string[] StringSplit(string value, string separator, IntRef returnLength)
    {
        string[] ret = value.Split(new char[] { separator[0] });
        returnLength.value = ret.Length;
        return ret;
    }

    public override int IntParse(string value)
    {
        return System.Int32.Parse(value);
    }

    public override float FloatParse(string value)
    {
        return System.Single.Parse(value);
    }

    public override float MathSqrt(float value)
    {
        return (float)System.Math.Sqrt(value);
    }

    public override string StringTrim(string value)
    {
        return value.Trim();
    }

    public override string IntToString(int value)
    {
        return value.ToString();
    }

    public override string Timestamp()
    {
        string time = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", System.DateTime.Now);
        return time;
    }

    public override string StringFormat(string format, string arg0)
    {
        return string.Format(format, arg0);
    }

    public override string StringFormat2(string format, string arg0, string arg1)
    {
        return string.Format(format, arg0, arg1);
    }

    public override void ClipboardSetText(string s)
    {
        System.Windows.Forms.Clipboard.SetText(s);
    }
    public override TextTexture CreateTextTexture(string text, float fontSize)
    {
        TextTexture t = new TextTexture();
        t.text = text;
        t.size = fontSize;
        System.Drawing.Bitmap bmp = r.MakeTextTexture(new ManicDigger.Renderers.Text() { fontsize = fontSize, text = text, color = Color.White });
        t.texturewidth = bmp.Width;
        t.textureheight = bmp.Height;
        var size = r.MeasureTextSize(text, fontSize);
        t.textwidth = (int)size.Width;
        t.textheight = (int)size.Height;
        //var texture = gl.CreateTexture();
        //gl.BindTexture(Gl.Texture2d, texture);
        //LoadBitmap(Gl.Texture2d, 0, (int)PixelType.UnsignedByte, bmp);
        //gl.TexParameteri(Gl.Texture2d, Gl.TextureMagFilter, Gl.Linear);
        //gl.TexParameteri(Gl.Texture2d, Gl.TextureMinFilter, Gl.LinearMipmapNearest);
        //gl.GenerateMipmap(Gl.Texture2d);
        //gl.BindTexture(Gl.Texture2d, null);
        TextureNative texture = new TextureNative();
        texture.value = LoadTexture(bmp, true);
        t.texture = texture;
        return t;
    }
    ManicDigger.Renderers.TextRenderer r = new ManicDigger.Renderers.TextRenderer();
    void LoadBitmap(int target, int level, int type, Bitmap bmp2)
    {
        BitmapData bmp_data = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.TexImage2D((TextureTarget)target, level, PixelInternalFormat.Rgba,
            bmp2.Width, bmp2.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, (PixelType)type, bmp_data.Scan0);

        bmp2.UnlockBits(bmp_data);
    }


    public override void TextSize(string text, float fontSize, IntRef outWidth, IntRef outHeight)
    {
    }

    public override void Exit()
    {
        Environment.Exit(0);
    }

    public override int[] StringToCharArray(string s, IntRef length)
    {
        length.value = s.Length;
        int[] charArray = new int[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            charArray[i] = s[i];
        }
        return charArray;
    }

    public override string CharArrayToString(int[] charArray, int length)
    {
        StringBuilder s = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            s.Append((char)charArray[i]);
        }
        return s.ToString();
    }

    public override string PathSavegames()
    {
        return ".";
    }

    public override string[] DirectoryGetFiles(string path, IntRef length)
    {
        string[] files = Directory.GetFiles(path);
        length.value = files.Length;
        return files;
    }

    public override void WebClientDownloadStringAsync(string url, HttpResponseCi response)
    {
        WebClient c = new WebClient();
        c.DownloadStringCompleted += new DownloadStringCompletedEventHandler(c_DownloadStringCompleted);
        c.DownloadStringAsync(new Uri(url), response);
    }

    void c_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        if (e.Error == null)
        {
            ((HttpResponseCi)e.UserState).value = e.Result;
            ((HttpResponseCi)e.UserState).done = true;
        }
    }

    public override string FileName(string fullpath)
    {
        FileInfo info = new FileInfo(fullpath);
        return info.Name.Replace(info.Extension, "");
    }

    public List<NewFrameHandler> newFrameHandlers = new List<NewFrameHandler>();
    public override void AddOnNewFrame(NewFrameHandler handler)
    {
        newFrameHandlers.Add(handler);
    }

    public List<KeyEventHandler> keyEventHandlers = new List<KeyEventHandler>();
    public override void AddOnKeyEvent(KeyEventHandler handler)
    {
        keyEventHandlers.Add(handler);
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

    public override int GetCanvasWidth()
    {
        return window.Width;
    }

    public override int GetCanvasHeight()
    {
        return window.Height;
    }

    public void Start()
    {
        window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(game_KeyDown);
        window.Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(game_KeyUp);
        window.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(game_KeyPress);
        window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
        window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
        window.Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);
        window.Mouse.WheelChanged += new EventHandler<OpenTK.Input.MouseWheelEventArgs>(Mouse_WheelChanged);
        window.RenderFrame += new EventHandler<OpenTK.FrameEventArgs>(window_RenderFrame);
    }

    void window_RenderFrame(object sender, OpenTK.FrameEventArgs e)
    {
        foreach (NewFrameHandler h in newFrameHandlers)
        {
            NewFrameEventArgs args = new NewFrameEventArgs();
            args.SetDt((float)e.Time);
            h.OnNewFrame(args);
        }
        window.SwapBuffers();
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

    int ToGlKey(OpenTK.Input.Key key)
    {
        switch (key)
        {
            case OpenTK.Input.Key.Left:
                return GlKeys.Left;
            case OpenTK.Input.Key.Up:
                return GlKeys.Up;
            case OpenTK.Input.Key.Right:
                return GlKeys.Right;
            case OpenTK.Input.Key.Down:
                return GlKeys.Down;
            case OpenTK.Input.Key.PageUp:
                return GlKeys.PageUp;
            case OpenTK.Input.Key.PageDown:
                return GlKeys.PageDown;
        }
        return (int)key;
    }

    public override void GlViewport(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }

    public override void GlClearColorBufferAndDepthBuffer()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public override void GlDisableDepthTest()
    {
        GL.Disable(EnableCap.DepthTest);
    }

    public override void BindTexture2d(Texture texture)
    {
        TextureNative texture_ = (TextureNative)texture;
        int textureValue;
        if (texture_ != null)
        {
            textureValue = texture_.value;
        }
        else
        {
            textureValue = 0;
        }
        GL.BindTexture(TextureTarget.Texture2D, textureValue);
    }

    public override Model CreateModel(ModelData data)
    {
        int id = GL.GenLists(1);

        GL.NewList(id, ListMode.Compile);

        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.ColorArray);
        GL.EnableClientState(ArrayCap.TextureCoordArray);

        float[] dataXyz = data.getXyz();
        float[] dataUv = data.getUv();
        byte[] dataRgba = data.getRgba();
        float[] xyz = new float[data.GetXyzCount()];
        float[] uv = new float[data.GetUvCount()];
        byte[] rgba = new byte[data.GetRgbaCount()];

        for (int i = 0; i < data.GetXyzCount(); i++)
        {
            xyz[i] = dataXyz[i];
        }
        for (int i = 0; i < data.GetUvCount(); i++)
        {
            uv[i] = dataUv[i];
        }
        if (dataRgba == null)
        {
            for (int i = 0; i < data.GetRgbaCount(); i++)
            {
                rgba[i] = 255;
            }
        }
        else
        {
            for (int i = 0; i < data.GetRgbaCount(); i++)
            {
                rgba[i] = dataRgba[i];
            }
        }
        GL.VertexPointer(3, VertexPointerType.Float, 3 * 4, xyz);
        GL.ColorPointer(4, ColorPointerType.UnsignedByte, 4 * 1, rgba);
        GL.TexCoordPointer(2, TexCoordPointerType.Float, 2 * 4, uv);

        BeginMode beginmode = BeginMode.Triangles;
        if (data.getMode() == DrawModeEnum.Triangles)
        {
            beginmode = BeginMode.Triangles;
            GL.Enable(EnableCap.Texture2D);
        }
        else if (data.getMode() == DrawModeEnum.Lines)
        {
            beginmode = BeginMode.Lines;
            GL.Disable(EnableCap.Texture2D);
        }
        else
        {
            throw new Exception();
        }

        var dataIndices = data.getIndices();
        ushort[] indices = new ushort[data.GetIndicesCount()];
        for (int i = 0; i < data.GetIndicesCount(); i++)
        {
            indices[i] = (ushort)dataIndices[i];
        }

        GL.DrawElements(beginmode, data.GetIndicesCount(), DrawElementsType.UnsignedShort, indices);

        GL.DisableClientState(ArrayCap.VertexArray);
        GL.DisableClientState(ArrayCap.ColorArray);
        GL.DisableClientState(ArrayCap.TextureCoordArray);
        GL.Disable(EnableCap.Texture2D);

        GL.EndList();
        DisplayListModel m = new DisplayListModel();
        m.listId = id;
        return m;
    }

    class DisplayListModel : Model
    {
        public int listId;
    }

    public override void DrawModel(Model model)
    {
        GL.CallList(((DisplayListModel)model).listId);
    }

    public override void InitShaders()
    {
    }

    public override void SetMatrixUniforms(float[] pMatrix, float[] mvMatrix)
    {
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(pMatrix);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(mvMatrix);
    }

    public override void GlClearColorRgbaf(float r, float g, float b, float a)
    {
        GL.ClearColor(r, g, b, a);
    }

    public override void GlEnableDepthTest()
    {
        GL.Enable(EnableCap.DepthTest);
    }

    public override Texture LoadTextureFromFile(string fullPath)
    {
        TextureNative t = new TextureNative();

        using (Bitmap bmp = new Bitmap(fullPath))
        {
            t.value = LoadTexture(bmp, true);
        }

        return t;
    }

    public bool ALLOW_NON_POWER_OF_TWO = false;
    public bool ENABLE_MIPMAPS = true;
    public bool ENABLE_TRANSPARENCY = true;

    //http://www.opentk.com/doc/graphics/textures/loading
    public int LoadTexture(Bitmap bmpArg, bool linearMag)
    {
        Bitmap bmp = bmpArg;
        bool convertedbitmap = false;
        if ((!ALLOW_NON_POWER_OF_TWO) &&
            (!(BitTools.IsPowerOfTwo((uint)bmp.Width) && BitTools.IsPowerOfTwo((uint)bmp.Height))))
        {
            Bitmap bmp2 = new Bitmap((int)BitTools.NextPowerOfTwo((uint)bmp.Width),
                (int)BitTools.NextPowerOfTwo((uint)bmp.Height));
            using (Graphics g = Graphics.FromImage(bmp2))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, 0, 0, bmp2.Width, bmp2.Height);
            }
            convertedbitmap = true;
            bmp = bmp2;
        }
        GL.Enable(EnableCap.Texture2D);
        int id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);
        if (!ENABLE_MIPMAPS)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }
        else
        {
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); //DOES NOT WORK ON ATI GRAPHIC CARDS
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1); //DOES NOT WORK ON ???
            int[] MipMapCount = new int[1];
            GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMaxLevel, out MipMapCount[0]);
            if (MipMapCount[0] == 0)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, linearMag ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 4);
        }
        BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

        bmp.UnlockBits(bmp_data);

        GL.Enable(EnableCap.DepthTest);

        if (ENABLE_TRANSPARENCY)
        {
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
        }


        if (ENABLE_TRANSPARENCY)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new Color4(0, 0, 0, byte.MaxValue));
        }

        if (convertedbitmap)
        {
            bmp.Dispose();
        }
        return id;
    }

    public override string GetLanguageIso6391()
    {
        return "en";
    }

    Stopwatch start = new Stopwatch();

    public override int TimeMillisecondsFromStart()
    {
        return (int)start.ElapsedMilliseconds;
    }
}

public class TextureNative : Texture
{
    public int value;
}


public class GameWindowNative : OpenTK.GameWindow
{
    public GamePlatformNative platform;
    public GameWindowNative(OpenTK.Graphics.GraphicsMode mode)
        : base(1280, 720, mode)
    {
        VSync = OpenTK.VSyncMode.Off;
        WindowState = OpenTK.WindowState.Normal;
    }
}

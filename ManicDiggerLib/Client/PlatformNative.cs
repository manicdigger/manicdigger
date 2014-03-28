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
using ManicDigger.Renderers;
using System.Globalization;
using OpenTK.Audio;
using ManicDigger.ClientNative;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class GamePlatformNative : GamePlatform, IGameExit
{
    public GamePlatformNative()
    {
        System.Threading.ThreadPool.SetMinThreads(32, 32);
        System.Threading.ThreadPool.SetMaxThreads(128, 128);
        audio.d_GameExit = this;
        datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
        start.Start();
    }

    public GameWindow window;

    string[] datapaths;
    Dictionary<string, string> cache = new Dictionary<string, string>();
    Dictionary<string, string> remap = new Dictionary<string, string>();
    public override string GetFullFilePath(string filename, BoolRef found)
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
            if (cache.ContainsKey(filename)) { found.value = true; return cache[filename]; }

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

        found.value = false;
        return null;
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

    public override bool StringEmpty(string data)
    {
        return string.IsNullOrEmpty(data);
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

    public override string StringFormat3(string format, string arg0, string arg1, string arg2)
    {
        return string.Format(format, arg0, arg1, arg2);
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
        System.Drawing.Bitmap bmp = r.MakeTextTexture(new Text_() { fontsize = fontSize, text = text, color = Game.ColorFromArgb(255, 255, 255, 255) });
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
        t.texture = LoadTexture(bmp, true);
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
    Dictionary<TextAndSize, SizeF> textsizes = new Dictionary<TextAndSize, SizeF>();
    public SizeF TextSize(string text, float fontsize)
    {
        SizeF size;
        if (textsizes.TryGetValue(new TextAndSize() { text = text, size = fontsize }, out size))
        {
            return size;
        }
        size = textrenderer.MeasureTextSize(text, fontsize);
        textsizes[new TextAndSize() { text = text, size = fontsize }] = size;
        return size;
    }

    public override void TextSize(string text, float fontSize, IntRef outWidth, IntRef outHeight)
    {
        SizeF size = TextSize(text, fontSize);
        outWidth.value = (int)size.Width;
        outHeight.value = (int)size.Height;
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

    public override string PathCombine(string part1, string part2)
    {
    	return Path.Combine(part1, part2);
    }

    public override string[] DirectoryGetFiles(string path, IntRef length)
    {
        if (!Directory.Exists(path))
        {
            length.value = 0;
            return new string[0];
        }
        string[] files = Directory.GetFiles(path);
        length.value = files.Length;
        return files;
    }
    
    public override string[] FileReadAllLines(string path, IntRef length)
    {
    	string[] lines = File.ReadAllLines(path);
    	length.value = lines.Length;
        return lines;
    }

    public override void WebClientDownloadDataAsync(string url, HttpResponseCi response)
    {
        WebClient c = new WebClient();
        c.DownloadDataCompleted += new DownloadDataCompletedEventHandler(c_DownloadStringCompleted);
        c.DownloadDataAsync(new Uri(url), response);
    }

    void c_DownloadStringCompleted(object sender, DownloadDataCompletedEventArgs e)
    {
        if (e.Error == null)
        {
            ((HttpResponseCi)e.UserState).value = e.Result;
            ((HttpResponseCi)e.UserState).valueLength = e.Result.Length;
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

    public static int ToGlKey(OpenTK.Input.Key key)
    {
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

    public override void BindTexture2d(int texture)
    {
        GL.BindTexture(TextureTarget.Texture2D, texture);
    }

    float[] xyz = new float[65536 * 3];
    float[] uv = new float[65536 * 2];
    byte[] rgba = new byte[65536 * 4];
    ushort[] indices = new ushort[65536];

    public override Model CreateModel(ModelData data)
    {
        int id = GL.GenLists(1);

        GL.NewList(id, ListMode.Compile);

        DrawModelData(data);

        GL.EndList();
        DisplayListModel m = new DisplayListModel();
        m.listId = id;
        return m;
    }

    public override void DrawModelData(ModelData data)
    {
        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.ColorArray);
        GL.EnableClientState(ArrayCap.TextureCoordArray);

        float[] dataXyz = data.getXyz();
        float[] dataUv = data.getUv();
        byte[] dataRgba = data.getRgba();

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

        int[] dataIndices = data.getIndices();
        for (int i = 0; i < data.GetIndicesCount(); i++)
        {
            indices[i] = (ushort)dataIndices[i];
        }

        GL.DrawElements(beginmode, data.GetIndicesCount(), DrawElementsType.UnsignedShort, indices);

        GL.DisableClientState(ArrayCap.VertexArray);
        GL.DisableClientState(ArrayCap.ColorArray);
        GL.DisableClientState(ArrayCap.TextureCoordArray);
        GL.Disable(EnableCap.Texture2D);
    }

    class DisplayListModel : Model
    {
        public int listId;
    }

    public override void DrawModel(Model model)
    {
        GL.CallList(((DisplayListModel)model).listId);
    }

    int[] lists = new int[1024];

    public override void DrawModels(Model[] model, int count)
    {
        if (lists.Length < count)
        {
            lists = new int[count * 2];
        }
        for (int i = 0; i < count; i++)
        {
            lists[i] = ((DisplayListModel)model[i]).listId;
        }
        GL.CallLists(count, ListNameType.Int, lists);
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

    public override int LoadTextureFromFile(string fullPath)
    {
        using (Bitmap bmp = new Bitmap(fullPath))
        {
            return LoadTexture(bmp, false);
        }
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
            (!(BitTools.IsPowerOfTwo(bmp.Width) && BitTools.IsPowerOfTwo(bmp.Height))))
        {
            Bitmap bmp2 = new Bitmap(BitTools.NextPowerOfTwo(bmp.Width),
                BitTools.NextPowerOfTwo(bmp.Height));
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
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    Stopwatch start = new Stopwatch();

    public override int TimeMillisecondsFromStart()
    {
        return (int)start.ElapsedMilliseconds;
    }

    public override void GlDisableCullFace()
    {
        GL.Disable(EnableCap.CullFace);
    }

    public override void GlEnableCullFace()
    {
        GL.Enable(EnableCap.CullFace);
    }

    public override void ThrowException(string message)
    {
        throw new Exception(message);
    }

    public override void DeleteModel(Model model)
    {
        DisplayListModel m = (DisplayListModel)model;
        GL.DeleteLists(m.listId, 1);
    }

    public override void GlEnableTexture2d()
    {
        GL.Enable(EnableCap.Texture2D);
    }

    public override BitmapCi BitmapCreate(int width, int height)
    {
        BitmapCiCs bmp = new BitmapCiCs();
        bmp.bmp = new Bitmap(width, height);
        return bmp;
    }

    public override void BitmapSetPixelsArgb(BitmapCi bmp, int[] pixels)
    {
        BitmapCiCs bmp_ = (BitmapCiCs)bmp;
        int width = bmp_.bmp.Width;
        int height = bmp_.bmp.Height;
        if (IsMono)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int color = pixels[x + y * width];
                    bmp_.bmp.SetPixel(x, y, Color.FromArgb(color));
                }
            }
        }
        else
        {
            FastBitmap fastbmp = new FastBitmap();
            fastbmp.bmp = bmp_.bmp;
            fastbmp.Lock();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    fastbmp.SetPixel(x, y, pixels[x + y * width]);
                }
            }
            fastbmp.Unlock();
        }
    }

    public override BitmapCi BitmapCreateFromPng(byte[] data, int dataLength)
    {
        BitmapCiCs bmp = new BitmapCiCs();
        bmp.bmp = new Bitmap(new MemoryStream(data, 0, dataLength));
        return bmp;
    }

    public bool IsMono = Type.GetType("Mono.Runtime") != null;

    public override void BitmapGetPixelsArgb(BitmapCi bitmap, int[] bmpPixels)
    {
        BitmapCiCs bmp = (BitmapCiCs)bitmap;
        int width = bmp.bmp.Width;
        int height = bmp.bmp.Height;
        if (IsMono)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmpPixels[x + y * width] = bmp.bmp.GetPixel(x, y).ToArgb();
                }
            }
        }
        else
        {
            FastBitmap fastbmp = new FastBitmap();
            fastbmp.bmp = bmp.bmp;
            fastbmp.Lock();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmpPixels[x + y * width] = fastbmp.GetPixel(x, y);
                }
            }
            fastbmp.Unlock();
        }
    }

    public override int LoadTextureFromBitmap(BitmapCi bmp)
    {
        BitmapCiCs bmp_ = (BitmapCiCs)bmp;
        return LoadTexture(bmp_.bmp, false);
    }

    public override void GLLineWidth(int width)
    {
        GL.LineWidth(width);
    }

    public override void GLDisableAlphaTest()
    {
        GL.Disable(EnableCap.AlphaTest);
    }

    public override void GLEnableAlphaTest()
    {
        GL.Enable(EnableCap.AlphaTest);
    }

    public override void GLDeleteTexture(int id)
    {
        GL.DeleteTexture(id);
    }

    ManicDigger.Renderers.TextRenderer textrenderer = new ManicDigger.Renderers.TextRenderer();

    public override BitmapCi CreateTextTexture2(Text_ t)
    {
        Bitmap bmp= textrenderer.MakeTextTexture(t);
        return new BitmapCiCs() { bmp = bmp };
    }

    public override float BitmapGetWidth(BitmapCi bmp)
    {
        BitmapCiCs bmp_ = (BitmapCiCs)bmp;
        return bmp_.bmp.Width;
    }

    public override float BitmapGetHeight(BitmapCi bmp)
    {
        BitmapCiCs bmp_ = (BitmapCiCs)bmp;
        return bmp_.bmp.Height;
    }

    public override void BitmapDelete(BitmapCi bmp)
    {
        BitmapCiCs bmp_ = (BitmapCiCs)bmp;
        bmp_.bmp.Dispose();
    }

    public override bool FloatTryParse(string s, FloatRef ret)
    {
        float f;
        if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
        {
            ret.value = f;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override float MathCos(float a)
    {
        return (float)Math.Cos(a);
    }

    public override float MathSin(float a)
    {
        return (float)Math.Sin(a);
    }

    AudioOpenAl audio = new AudioOpenAl();

    public override void AudioPlay(string path, float x, float y, float z)
    {
        audio.Play(path, new Vector3(x, y, z));
    }
    
    public override void AudioPlayLoop(string path, bool play, bool restart)
    {
        audio.PlayAudioLoop(path, play, restart);
    }

    public override void AudioUpdateListener(float posX, float posY, float posZ, float orientX, float orientY, float orientZ)
    {
        audio.UpdateListener(new Vector3(posX,posY,posZ), new Vector3(orientX, orientY, orientZ));
    }

    public bool exit { get; set; }

    public override void ConsoleWriteLine(string s)
    {
        Console.WriteLine(s);
    }

    public override DummyNetOutgoingMessage CastToDummyNetOutgoingMessage(INetOutgoingMessage message)
    {
        return (DummyNetOutgoingMessage)message;
    }

    public override MonitorObject MonitorCreate()
    {
        return new MonitorObject();
    }

    public override void MonitorEnter(MonitorObject monitorObject)
    {
        System.Threading.Monitor.Enter(monitorObject);
    }

    public override void MonitorExit(MonitorObject monitorObject)
    {
        System.Threading.Monitor.Exit(monitorObject);
    }

    public override bool EnetAvailable()
    {
        return true;
    }

    public override EnetHost EnetCreateHost()
    {
        return new EnetHostNative() { host = new ENet.Host() };
    }

    public override void EnetHostInitializeServer(EnetHost host, int port, int peerLimit)
    {
        EnetHostNative host_ = (EnetHostNative)host;
        host_.host.InitializeServer(port, peerLimit);
    }

    public override bool EnetHostService(EnetHost host, int timeout, EnetEventRef enetEvent)
    {
        EnetHostNative host_ = (EnetHostNative)host;
        ENet.Event e;
        bool ret = host_.host.Service(timeout, out e);
        EnetEventNative ee = new EnetEventNative();
        ee.e = e;
        enetEvent.e = ee;
        return ret;
    }

    public override bool EnetHostCheckEvents(EnetHost host, EnetEventRef event_)
    {
        EnetHostNative host_ = (EnetHostNative)host;
        ENet.Event e;
        bool ret = host_.host.CheckEvents(out e);
        EnetEventNative ee = new EnetEventNative();
        ee.e = e;
        event_.e = ee;
        return ret;
    }

    public override EnetPeer EnetHostConnect(EnetHost host, string hostName, int port, int data, int channelLimit)
    {
        EnetHostNative host_ = (EnetHostNative)host;
        ENet.Peer peer = host_.host.Connect(hostName, port, data, channelLimit);
        EnetPeerNative peer_ = new EnetPeerNative();
        peer_.peer = peer;
        return peer_;
    }

    public override void EnetPeerSend(EnetPeer peer, byte channelID, byte[] data, int dataLength, int flags)
    {
        EnetPeerNative peer_ = (EnetPeerNative)peer;
        peer_.peer.Send(channelID, data, (ENet.PacketFlags)flags);
    }

    public override EnetNetConnection CastToEnetNetConnection(INetConnection connection)
    {
        return (EnetNetConnection)connection;
    }

    public override EnetNetOutgoingMessage CastToEnetNetOutgoingMessage(INetOutgoingMessage msg)
    {
        return (EnetNetOutgoingMessage)msg;
    }

    public override void EnetHostInitialize(EnetHost host, IPEndPointCi address, int peerLimit, int channelLimit, int incomingBandwidth, int outgoingBandwidth)
    {
        if(address!=null)
        {
            throw new Exception();
        }
        EnetHostNative host_ = (EnetHostNative)host;
        host_.host.Initialize(null, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);
    }

    Screenshot screenshot = new Screenshot();

    public override void SaveScreenshot()
    {
        screenshot.d_GameWindow = window;
        screenshot.SaveScreenshot();
    }

    public override BitmapCi GrabScreenshot()
    {
        screenshot.d_GameWindow = window;
        Bitmap bmp = screenshot.GrabScreenshot();
        BitmapCiCs bmp_ = new BitmapCiCs();
        bmp_.bmp = bmp;
        return bmp_;
    }

    public override AviWriterCi AviWriterCreate()
    {
        AviWriterCiCs avi = new AviWriterCiCs();
        return avi;
    }

    public override float FloatModulo(float a, int b)
    {
        return a % b;
    }

    public bool IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;

    public override void SetFreeMouse(bool value)
    {
        if (IsMac)
        {
            window.CursorVisible = value;
            System.Windows.Forms.Cursor.Hide();
        }
    }

    public override UriCi ParseUri(string uri)
    {
        MyUri myuri = new MyUri(uri);

        UriCi ret = new UriCi();
        ret.url = myuri.Url;
        ret.ip = myuri.Ip;
        ret.port = myuri.Port;
        ret.get = new DictionaryStringString();
        foreach (var k in myuri.Get)
        {
            ret.get.Set(k.Key, k.Value);
        }
        return ret;
    }

    public override OptionsCi LoadOptions()
    {
        Options loaded = new Options();
        string path = Path.Combine(gamepathconfig, filename);
        if (!File.Exists(path))
        {
            return null;
        }
        string s = File.ReadAllText(path);
        loaded = (Options)x.Deserialize(new System.IO.StringReader(s));

        OptionsCi ret = new OptionsCi();
        ret.Shadows = loaded.Shadows;
        ret.Font = loaded.Font;
        ret.DrawDistance = loaded.DrawDistance;
        ret.UseServerTextures = loaded.UseServerTextures;
        ret.EnableSound = loaded.EnableSound;
        ret.Framerate = loaded.Framerate;
        ret.Resolution = loaded.Resolution;
        ret.Fullscreen = loaded.Fullscreen;
        ret.Smoothshadows = loaded.Smoothshadows;
        ret.BlockShadowSave = loaded.BlockShadowSave;
        foreach (KeyValuePair<int, int> k in loaded.Keys)
        {
            ret.Keys[k.Key] = k.Value;
        }
        return ret;
    }

    XmlSerializer x = new XmlSerializer(typeof(Options));
    public string gamepathconfig = GameStorePath.GetStorePath();
    string filename = "ClientConfig.txt";

    public override void SaveOptions(OptionsCi options)
    {
        Options save = new Options();
        save.Shadows = options.Shadows;
        save.Font = options.Font;
        save.DrawDistance = options.DrawDistance;
        save.UseServerTextures = options.UseServerTextures;
        save.EnableSound = options.EnableSound;
        save.Framerate = options.Framerate;
        save.Resolution = options.Resolution;
        save.Fullscreen = options.Fullscreen;
        save.Smoothshadows = options.Smoothshadows;
        save.BlockShadowSave = options.BlockShadowSave;
        save.Keys = new SerializableDictionary<int, int>();
        for (int i = 0; i < options.Keys.Length; i++)
        {
            if (options.Keys[i] != 0)
            {
                save.Keys[i] = options.Keys[i];
            }
        }

        string path = Path.Combine(gamepathconfig, filename);
        MemoryStream ms = new MemoryStream();
        x.Serialize(ms, save);
        string xml = Encoding.UTF8.GetString(ms.ToArray());
        File.WriteAllText(path, xml);
    }

    public override bool StringContains(string a, string b)
    {
        return a.Contains(b);
    }

    public override RandomCi RandomCreate()
    {
        return new RandomNative();
    }

    public override void GlClearDepthBuffer()
    {
        GL.Clear(ClearBufferMask.DepthBufferBit);
    }

    public override string PathStorage()
    {
        return GameStorePath.GetStorePath();
    }

    public override string StringReplace(string s, string from, string to)
    {
        return s.Replace(from, to);
    }

    public override PlayerInterpolationState CastToPlayerInterpolationState(InterpolatedObject a)
    {
        return (PlayerInterpolationState)a;
    }

    public override void GlLightModelAmbient(int r, int g, int b)
    {
        float mult = 1f;
        float[] global_ambient = new float[] { (float)r / 255f * mult, (float)g / 255f * mult, (float)b / 255f * mult, 1f };
        GL.LightModel(LightModelParameter.LightModelAmbient, global_ambient);
    }

    public override float MathAcos(float p)
    {
        return (float)Math.Acos(p);
    }

    public override void SetVSync(bool enabled)
    {
        window.VSync = enabled ? VSyncMode.On : VSyncMode.Off;
    }

    public override string GetGameVersion()
    {
        return GameVersion.Version;
    }

    public override void GlEnableFog()
    {
        GL.Enable(EnableCap.Fog);
    }

    public override void GlHintFogHintNicest()
    {
        GL.Hint(HintTarget.FogHint, HintMode.Nicest);
    }

    public override void GlFogFogModeExp2()
    {
        GL.Fog(FogParameter.FogMode, (int)FogMode.Exp2);
    }

    public override void GlFogFogColor(int r, int g, int b, int a)
    {
        float[] fogColor = new[] { (float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255 };
        GL.Fog(FogParameter.FogColor, fogColor);
    }

    public override void GlFogFogDensity(float density)
    {
        GL.Fog(FogParameter.FogDensity, density);
    }
    ICompression compression = new CompressionGzip();
    public override byte[] GzipDecompress(byte[] compressed, int compressedLength)
    {
        byte[] data = new byte[compressedLength];
        for (int i = 0; i < compressedLength; i++)
        {
            data[i] = compressed[i];
        }
        return compression.Decompress(data);
    }
    public bool ENABLE_CHATLOG = true;
    public string gamepathlogs() { return Path.Combine(PathStorage(), "Logs"); }
    private static string MakeValidFileName(string name)
    {
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidReStr = string.Format(@"[{0}]", invalidChars);
        return Regex.Replace(name, invalidReStr, "_");
    }
    public override bool ChatLog(string servername, string p)
    {
        if (!ENABLE_CHATLOG)
        {
            return true;
        }
        if (!Directory.Exists(gamepathlogs()))
        {
            Directory.CreateDirectory(gamepathlogs());
        }
        string filename = Path.Combine(gamepathlogs(), MakeValidFileName(servername) + ".txt");
        try
        {
            File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override float MathTan(float p)
    {
        return (float)Math.Tan(p);
    }

    public override bool IsValidTypingChar(int c_)
    {
        char c = (char)c_;
        return (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)
                    || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
                    && c != '\r' && c != '\t';
    }

    public override bool StringStartsWithIgnoreCase(string a, string b)
    {
        return a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase);
    }

    public override int StringIndexOf(string s, string p)
    {
        return s.IndexOf(p);
    }

    public override void WindowExit()
    {
        window.Exit();
    }

    public override void MessageBoxShowError(string text, string caption)
    {
        System.Windows.Forms.MessageBox.Show(text, caption, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
    }

    public override int ByteArrayLength(byte[] arr)
    {
        return arr.Length;
    }

    public override string StringFromUtf8ByteArray(byte[] value, int valueLength)
    {
        string s = Encoding.UTF8.GetString(value, 0, valueLength);
        return s;
    }

    public override string[] ReadAllLines(string p, IntRef retCount)
    {
        List<string> lines = new List<string>();
        StringReader reader = new StringReader(p);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            lines.Add(line);
        }
        retCount.value = lines.Count;
        return lines.ToArray();
    }

    public override bool ClipboardContainsText()
    {
        return Clipboard.ContainsText();
    }

    public override string ClipboardGetText()
    {
        return Clipboard.GetText();
    }

    public override void SetTitle(string applicationname)
    {
        window.Title = applicationname;
    }

    public override bool Focused()
    {
        return window.Focused;
    }
}

public class RandomNative : RandomCi
{
    public Random rnd = new Random();
    public override float NextFloat()
    {
        return (float)rnd.NextDouble();
    }

    public override int Next()
    {
        return rnd.Next();
    }
}

public class Options
{
    public bool Shadows;
    public int Font;
    public int DrawDistance = 256;
    public bool UseServerTextures = true;
    public bool EnableSound = true;
    public int Framerate = 0;
    public int Resolution = 0;
    public bool Fullscreen = false;
    public bool Smoothshadows = true;
    public float BlockShadowSave = 0.6f;
    public SerializableDictionary<int, int> Keys = new SerializableDictionary<int, int>();
}

public class MyUri
{
    public MyUri(string uri)
    {
        //string url = "md://publichash:123/?user=a&auth=123";
        var a = new Uri(uri);
        Ip = a.Host;
        Port = a.Port;
        Get = ParseGet(uri);
    }
    internal string Url { get; private set; }
    internal string Ip { get; private set; }
    internal int Port { get; private set; }
    internal Dictionary<string, string> Get { get; private set; }
    private static Dictionary<string, string> ParseGet(string url)
    {
        try
        {
            Dictionary<string, string> d;
            d = new Dictionary<string, string>();
            if (url.Contains("?"))
            {
                string url2 = url.Substring(url.IndexOf("?") + 1);
                var ss = url2.Split(new char[] { '&' });
                for (int i = 0; i < ss.Length; i++)
                {
                    var ss2 = ss[i].Split(new char[] { '=' });
                    d[ss2[0]] = ss2[1];
                }
            }
            return d;
        }
        catch
        {
            //throw new FormatException("Invalid address: " + url);
            return null;
        }
    }
}

public class AviWriterCiCs : AviWriterCi
{
    public AviWriterCiCs()
    {
        avi = new AviWriter();
    }

    public AviWriter avi;
    public Bitmap openbmp;

    public override void Open(string filename, int framerate, int width, int height)
    {
        openbmp = avi.Open(filename, (uint)framerate, width, height);
    }

    public override void AddFrame(BitmapCi bitmap)
    {
        var bmp_ = (BitmapCiCs)bitmap;

        using (Graphics g = Graphics.FromImage(openbmp))
        {
            g.DrawImage(bmp_.bmp, 0, 0);
        }
        openbmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

        avi.AddFrame();
    }

    public override void Close()
    {
        avi.Close();
    }
}

public class EnetHostNative : EnetHost
{
    public ENet.Host host;
}

public class EnetEventNative : EnetEvent
{
    public ENet.Event e;
    public override EnetEventType Type()
    {
        return (EnetEventType)e.Type;
    }

    public override EnetPeer Peer()
    {
        EnetPeerNative peer = new EnetPeerNative();
        peer.peer = e.Peer;
        return peer;
    }

    public override EnetPacket Packet()
    {
        EnetPacketNative packet = new EnetPacketNative();
        packet.packet = e.Packet;
        return packet;
    }
}

public class EnetPacketNative : EnetPacket
{
    internal ENet.Packet packet;
    public override int GetBytesCount()
    {
        return packet.GetBytes().Length;
    }

    public override byte[] GetBytes()
    {
        return packet.GetBytes();
    }

    public override void Dispose()
    {
        packet.Dispose();
    }
}

public class EnetPeerNative : EnetPeer
{
    public ENet.Peer peer;
    public override int UserData()
    {
        return peer.UserData.ToInt32();
    }

    public override void SetUserData(int value)
    {
        peer.UserData = new IntPtr(value);
    }

    public override IPEndPointCi GetRemoteAddress()
    {
        return IPEndPointCiDefault.Create(peer.GetRemoteAddress().Address.ToString());
    }
}

public class BitmapCiCs : BitmapCi
{
    public Bitmap bmp;
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

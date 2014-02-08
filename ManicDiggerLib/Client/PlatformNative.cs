using System.IO;
using ManicDigger;
using System.Collections.Generic;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Text;

public class GamePlatformNative : GamePlatform
{
    public GamePlatformNative()
    {
        datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
    }

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
    public override TextTexture CreateTextTexture(Gl gl, string text, float fontSize)
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
        var texture = gl.CreateTexture();
        gl.BindTexture(Gl.Texture2d, texture);
        LoadBitmap(Gl.Texture2d, 0, (int)PixelType.UnsignedByte, bmp);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMagFilter, Gl.Linear);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMinFilter, Gl.LinearMipmapNearest);
        gl.GenerateMipmap(Gl.Texture2d);
        gl.BindTexture(Gl.Texture2d, null);
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
}

using System.IO;
using ManicDigger;
using System.Collections.Generic;

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
}

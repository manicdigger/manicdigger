public class VectorTool
{
    public static void ToVectorInFixedSystem(float dx, float dy, float dz, float orientationx, float orientationy, Vector3Ref output)
    {
        //Don't calculate for nothing ...
        if (dx == 0 && dy == 0 && dz == 0)
        {
            output.X = 0;
            output.Y = 0;
            output.Z = 0;
            return;
        }

        //Convert to Radian : 360° = 2PI
        float xRot = orientationx;//Math.toRadians(orientation.X);
        float yRot = orientationy;//Math.toRadians(orientation.Y);

        //Calculate the formula
        float x = (dx * Platform.Cos(yRot) + dy * Platform.Sin(xRot) * Platform.Sin(yRot) - dz * Platform.Cos(xRot) * Platform.Sin(yRot));
        float y = (dy * Platform.Cos(xRot) + dz * Platform.Sin(xRot));
        float z = (dx * Platform.Sin(yRot) - dy * Platform.Sin(xRot) * Platform.Cos(yRot) + dz * Platform.Cos(xRot) * Platform.Cos(yRot));

        //Return the vector expressed in the global axis system
        output.X = x;
        output.Y = y;
        output.Z = z;
    }
}

public class Unproject
{
    public Unproject()
    {
        finalMatrix = Mat4.Create();
        inp = new float[4];
        out_ = new float[4];
    }
    float[] finalMatrix;
    float[] inp;
    float[] out_;
    public bool UnProject(int winX, int winY, int winZ, float[] model, float[] proj, float[] view, float[] objPos)
    {
        inp[0] = winX;
        inp[1] = winY;
        inp[2] = winZ;
        inp[3] = 1;

        Mat4.Multiply(finalMatrix, proj, model);
        Mat4.Invert(finalMatrix, finalMatrix);

        // Map x and y from window coordinates
        inp[0] = (inp[0] - view[0]) / view[2];
        inp[1] = (inp[1] - view[1]) / view[3];

        // Map to range -1 to 1
        inp[0] = inp[0] * 2 - 1;
        inp[1] = inp[1] * 2 - 1;
        inp[2] = inp[2] * 2 - 1;

        MultMatrixVec(finalMatrix, inp, out_);

        if (out_[3] == 0)
        {
            return false;
        }

        out_[0] /= out_[3];
        out_[1] /= out_[3];
        out_[2] /= out_[3];

        objPos[0] = out_[0];
        objPos[1] = out_[1];
        objPos[2] = out_[2];

        return true;
    }

    void MultMatrixVec(float[] matrix, float[] inp__, float[] out__)
    {
        for (int i = 0; i < 4; i = i + 1)
        {
            out__[i] =
                inp__[0] * matrix[0 * 4 + i] +
                inp__[1] * matrix[1 * 4 + i] +
                inp__[2] * matrix[2 * 4 + i] +
                inp__[3] * matrix[3 * 4 + i];
        }
    }
}

public class RectFRef
{
    internal float x;
    internal float y;
    internal float w;
    internal float h;

    public static RectFRef Create(float x_, float y_, float w_, float h_)
    {
        RectFRef r = new RectFRef();
        r.x = x_;
        r.y = y_;
        r.w = w_;
        r.h = h_;
        return r;
    }

    internal float Left()
    {
        return x;
    }

    internal float Bottom()
    {
        return y + h;
    }

    internal float Top()
    {
        return y;
    }

    internal float Right()
    {
        return x + w;
    }
}

public class InterpolationCi
{
    public static int InterpolateColor(GamePlatform platform, float progress, int[] colors, int colorsLength)
    {
        float one = 1;
        int colora = platform.FloatToInt((colorsLength - 1) * progress);
        if (colora < 0) { colora = 0; }
        if (colora >= colorsLength) { colora = colorsLength - 1; }
        int colorb = colora + 1;
        if (colorb >= colorsLength) { colorb = colorsLength - 1; }
        int a = colors[colora];
        int b = colors[colorb];
        float p = (progress - (one * colora) / (colorsLength - 1)) * (colorsLength - 1);
        int A = platform.FloatToInt(Game.ColorA(a) + (Game.ColorA(b) - Game.ColorA(a)) * p);
        int R = platform.FloatToInt(Game.ColorR(a) + (Game.ColorR(b) - Game.ColorR(a)) * p);
        int G = platform.FloatToInt(Game.ColorG(a) + (Game.ColorG(b) - Game.ColorG(a)) * p);
        int B = platform.FloatToInt(Game.ColorB(a) + (Game.ColorB(b) - Game.ColorB(a)) * p);
        return Game.ColorFromArgb(A, R, G, B);
    }
}

public class BitTools
{
    public static bool IsPowerOfTwo(int x)
    {
        return (
          x == 1 || x == 2 || x == 4 || x == 8 || x == 16 || x == 32 ||
          x == 64 || x == 128 || x == 256 || x == 512 || x == 1024 ||
          x == 2048 || x == 4096 || x == 8192 || x == 16384 ||
          x == 32768 || x == 65536 || x == 131072 || x == 262144 ||
          x == 524288 || x == 1048576 || x == 2097152 ||
          x == 4194304 || x == 8388608 || x == 16777216 ||
          x == 33554432 || x == 67108864 || x == 134217728 ||
          x == 268435456 || x == 536870912 || x == 1073741824 // ||
            //x == 2147483648);
          );
    }
    public static int NextPowerOfTwo(int x)
    {
        x--;
        x |= x >> 1;  // handle  2 bit numbers
        x |= x >> 2;  // handle  4 bit numbers
        x |= x >> 4;  // handle  8 bit numbers
        x |= x >> 8;  // handle 16 bit numbers
        x |= x >> 16; // handle 32 bit numbers
        x++;
        return x;
    }
}


public class StringTools
{
    public static string StringAppend(GamePlatform p, string a, string b)
    {
        IntRef aLength = new IntRef();
        int[] aChars = p.StringToCharArray(a, aLength);
        IntRef bLength = new IntRef();
        int[] bChars = p.StringToCharArray(b, bLength);

        int[] cChars = new int[aLength.value + bLength.value];
        for (int i = 0; i < aLength.value; i++)
        {
            cChars[i] = aChars[i];
        }
        for (int i = 0; i < bLength.value; i++)
        {
            cChars[i + aLength.value] = bChars[i];
        }
        return p.CharArrayToString(cChars, aLength.value + bLength.value);
    }

    public static string StringSubstring(GamePlatform p, string a, int start, int count)
    {
        IntRef aLength = new IntRef();
        int[] aChars = p.StringToCharArray(a, aLength);

        int[] bChars = new int[count];
        for (int i = 0; i < count; i++)
        {
            bChars[i] = aChars[start + i];
        }
        return p.CharArrayToString(bChars, count);
    }

    public static string StringSubstringToEnd(GamePlatform p, string a, int start)
    {
        return StringSubstring(p, a, start, StringLength(p, a) - start);
    }

    public static int StringLength(GamePlatform p, string a)
    {
        IntRef aLength = new IntRef();
        int[] aChars = p.StringToCharArray(a, aLength);
        return aLength.value;
    }

    public static bool StringStartsWith(GamePlatform p, string s, string b)
    {
        return StringSubstring(p, s, 0, StringLength(p, b)) == b;
    }
}

public class MiscCi
{
    public static bool ReadBool(string str)
    {
        if (str == null)
        {
            return false;
        }
        else
        {
            return (str != "0"
                && (str != "false")
                && (str != "False")
                && (str != "FALSE"));
        }
    }
    public static byte[] UshortArrayToByteArray(int[] input, int inputLength)
    {
        int outputLength = inputLength * 2;
        byte[] output = new byte[outputLength];
        for (int i = 0; i < inputLength; i++)
        {
            output[i * 2] = Game.IntToByte(input[i] & 255);
            output[i * 2 + 1] = Game.IntToByte((input[i] >> 8) & 255);
        }
        return output;
    }

    public static float Vec3Length(float x, float y, float z)
    {
        return Platform.Sqrt(x * x + y * y + z * z);
    }
}

public class ConnectData
{
    internal string Username;
    internal string Ip;
    internal int Port;
    internal string Auth;
    internal string ServerPassword;
    internal bool IsServePasswordProtected;
    public static ConnectData FromUri(UriCi uri)
    {
        ConnectData c = new ConnectData();
        c = new ConnectData();
        c.Ip = uri.GetIp();
        c.Port = 25565;
        c.Username = "gamer";
        if (uri.GetPort() != -1)
        {
            c.Port = uri.GetPort();
        }
        if (uri.GetGet().ContainsKey("user"))
        {
            c.Username = uri.GetGet().Get("user");
        }
        if (uri.GetGet().ContainsKey("auth"))
        {
            c.Auth = uri.GetGet().Get("auth");
        }
        if (uri.GetGet().ContainsKey("serverPassword"))
        {
            c.IsServePasswordProtected = MiscCi.ReadBool(uri.GetGet().Get("serverPassword"));
        }
        return c;
    }

    public void SetIp(string value)
    {
        Ip = value;
    }

    public void SetPort(int value)
    {
        Port = value;
    }

    public void SetUsername(string value)
    {
        Username = value;
    }

    public void SetIsServePasswordProtected(bool value)
    {
        IsServePasswordProtected = value;
    }

    public bool GetIsServePasswordProtected()
    {
        return IsServePasswordProtected;
    }

    public void SetServerPassword(string value)
    {
        ServerPassword = value;
    }
}

public class Ping_
{
    public Ping_()
    {
        RoundtripTimeMilliseconds = 0;
        ready = true;
        timeSendMilliseconds = 0;
        timeout = 10;
    }

    int RoundtripTimeMilliseconds;

    bool ready;
    int timeSendMilliseconds;
    int timeout; //in seconds

    public int GetTimeoutValue()
    {
        return timeout;
    }
    public void SetTimeoutValue(int value)
    {
        timeout = value;
    }

    public bool Send(GamePlatform platform)
    {
        if (!ready)
        {
            return false;
        }
        ready = false;
        this.timeSendMilliseconds = platform.TimeMillisecondsFromStart();
        return true;
    }

    public bool Receive(GamePlatform platform)
    {
        if (ready)
        {
            return false;
        }
        this.RoundtripTimeMilliseconds = platform.TimeMillisecondsFromStart() - timeSendMilliseconds;
        ready = true;
        return true;
    }

    public bool Timeout(GamePlatform platform)
    {
        if ((platform.TimeMillisecondsFromStart() - timeSendMilliseconds) / 1000 > this.timeout)
        {
            this.ready = true;
            return true;
        }
        return false;
    }

    internal int RoundtripTimeTotalMilliseconds()
    {
        return RoundtripTimeMilliseconds;
    }
}

public class ConnectedPlayer
{
    internal int id;
    internal string name;
    internal int ping; // in ms
}

public class ServerInformation
{
    public ServerInformation()
    {
        ServerName = "";
        ServerMotd = "";
        connectdata = new ConnectData();
        ServerPing = new Ping_();
    }

    internal string ServerName;
    internal string ServerMotd;
    internal ConnectData connectdata;
    internal Ping_ ServerPing;
}

public class BitmapData_
{
    public static BitmapData_ Create(int width, int height)
    {
        BitmapData_ b = new BitmapData_();
        b.width = width;
        b.height = height;
        b.argb = new int[width * height];
        return b;
    }
    public static BitmapData_ CreateFromBitmap(GamePlatform p, BitmapCi atlas2d_)
    {
        BitmapData_ b = new BitmapData_();
        b.width = p.FloatToInt(p.BitmapGetWidth(atlas2d_));
        b.height = p.FloatToInt(p.BitmapGetHeight(atlas2d_));
        b.argb = new int[b.width * b.height];
        p.BitmapGetPixelsArgb(atlas2d_, b.argb);
        return b;
    }

    internal int[] argb;
    internal int width;
    internal int height;

    public void SetPixel(int x, int y, int color)
    {
        argb[x + y * width] = color;
    }
    public int GetPixel(int x, int y)
    {
        return argb[x + y * width];
    }

    public BitmapCi ToBitmap(GamePlatform p)
    {
        BitmapCi bmp = p.BitmapCreate(width, height);
        p.BitmapSetPixelsArgb(bmp, argb);
        return bmp;
    }
}

public class TextureAtlasConverter
{
    //tiles = 16 means 16 x 16 atlas
    public BitmapCi[] Atlas2dInto1d(GamePlatform p, BitmapCi atlas2d_, int tiles, int atlassizezlimit, IntRef retCount)
    {
        BitmapData_ orig = BitmapData_.CreateFromBitmap(p, atlas2d_);

        int tilesize = orig.width / tiles;

        int atlasescount = MathCi.MaxInt(1, (tiles * tiles * tilesize) / atlassizezlimit);
        BitmapCi[] atlases = new BitmapCi[128];
        int atlasesCount = 0;

        BitmapData_ atlas1d = null;

        for (int i = 0; i < tiles * tiles; i++)
        {
            int x = i % tiles;
            int y = i / tiles;
            int tilesinatlas = (tiles * tiles / atlasescount);
            if (i % tilesinatlas == 0)
            {
                if (atlas1d != null)
                {
                    atlases[atlasesCount++] = atlas1d.ToBitmap(p);
                }
                atlas1d = BitmapData_.Create(tilesize, atlassizezlimit);
            }
            for (int xx = 0; xx < tilesize; xx++)
            {
                for (int yy = 0; yy < tilesize; yy++)
                {
                    int c = orig.GetPixel(x * tilesize + xx, y * tilesize + yy);
                    atlas1d.SetPixel(xx, (i % tilesinatlas) * tilesize + yy, c);
                }
            }
        }
        atlases[atlasesCount++] = atlas1d.ToBitmap(p);
        retCount.value = atlasescount;
        return atlases;
    }
}

public class VecCito3i
{
    public int x;
    public int y;
    public int z;

    public static VecCito3i CitoCtr(int _x, int _y, int _z)
    {
        VecCito3i v = new VecCito3i();
        v.x = _x;
        v.y = _y;
        v.z = _z;

        return v;
    }

    public void Add(int _x, int _y, int _z, VecCito3i result)
    {
        result.x = x + _x;
        result.y = y + _y;
        result.z = z + _z;
    }
}

public class GameVersionHelper
{
    public static bool ServerVersionAtLeast(GamePlatform platform, string serverGameVersion, int year, int month, int day)
    {
        if (serverGameVersion == null)
        {
            return true;
        }
        if (VersionToInt(platform, serverGameVersion) < DateToInt(year, month, day))
        {
            return false;
        }
        return true;
    }

    static bool IsVersionDate(GamePlatform platform, string version)
    {
        IntRef versionCharsCount = new IntRef();
        int[] versionChars = platform.StringToCharArray(version, versionCharsCount);
        if (versionCharsCount.value >= 10)
        {
            if (versionChars[4] == 45 && versionChars[7] == 45) // '-'
            {
                return true;
            }
        }
        return false;
    }

    static int VersionToInt(GamePlatform platform, string version)
    {
        int max = 1000 * 1000 * 1000;
        if (!IsVersionDate(platform, version))
        {
            return max;
        }
        FloatRef year = new FloatRef();
        FloatRef month = new FloatRef();
        FloatRef day = new FloatRef();
        if (platform.FloatTryParse(StringTools.StringSubstring(platform, version, 0, 4), year))
        {
            if (platform.FloatTryParse(StringTools.StringSubstring(platform, version, 5, 2), month))
            {
                if (platform.FloatTryParse(StringTools.StringSubstring(platform, version, 8, 2), day))
                {
                    int year_ = platform.FloatToInt(year.value);
                    int month_ = platform.FloatToInt(month.value);
                    int day_ = platform.FloatToInt(day.value);
                    return year_ * 10000 + month_ * 100 + day_;
                }
            }
        }
        return max;
    }

    static int DateToInt(int year, int month, int day)
    {
        return year * 10000 + month * 100 + day;
    }
}

public class MathCi
{
    public static float MinFloat(float a, float b)
    {
        if (a <= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static float MaxFloat(float a, float b)
    {
        if (a >= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static float AbsFloat(float b)
    {
        if (b >= 0)
        {
            return b;
        }
        else
        {
            return 0 - b;
        }
    }

    public static int Sign(float q)
    {
        if (q < 0)
        {
            return -1;
        }
        else if (q == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    public static int MaxInt(int a, int b)
    {
        if (a >= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static int MinInt(int a, int b)
    {
        if (a <= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static float ClampFloat(float value, float min, float max)
    {
        float result = value;
        if (value > max)
        {
            result = max;
        }
        if (value < min)
        {
            result = min;
        }
        return result;
    }

    public static int ClampInt(int value, int min, int max)
    {
        int result = value;
        if (value > max)
        {
            result = max;
        }
        if (value < min)
        {
            result = min;
        }
        return result;
    }
}

public class Vector3Ref
{
    internal float X;
    internal float Y;
    internal float Z;

    internal float Length()
    {
        return Platform.Sqrt(X * X + Y * Y + Z * Z);
    }

    internal void Normalize()
    {
        float length = Length();
        X = X / length;
        Y = Y / length;
        Z = Z / length;
    }

    internal static Vector3Ref Create(float x, float y, float z)
    {
        Vector3Ref v = new Vector3Ref();
        v.X = x;
        v.Y = y;
        v.Z = z;
        return v;
    }

    public float GetX()
    {
        return X;
    }

    public float GetY()
    {
        return Y;
    }

    public float GetZ()
    {
        return Z;
    }
}

public class Vector3IntRef
{
    internal int X;
    internal int Y;
    internal int Z;

    internal static Vector3IntRef Create(int x, int y, int z)
    {
        Vector3IntRef v = new Vector3IntRef();
        v.X = x;
        v.Y = y;
        v.Z = z;
        return v;
    }
}

public class BoolRef
{
    internal bool value;
    public bool GetValue() { return value; }
    public void SetValue(bool value_) { value = value_; }
}

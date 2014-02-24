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

public class DictionaryStringString
{
    public DictionaryStringString()
    {
        items = new KeyValueStringString[64];
        count = 64;
    }
    internal KeyValueStringString[] items;
    internal int count;

    public void Set(string key, string value)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (items[i].key == key)
            {
                items[i].value = value;
                return;
            }
        }
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                items[i] = new KeyValueStringString();
                items[i].key = key;
                items[i].value = value;
                return;
            }
        }
    }
}

public class KeyValueStringString
{
    internal string key;
    internal string value;
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

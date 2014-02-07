public abstract class GamePlatform
{
    public static GamePlatform Create()
    {
        GamePlatform platform;
#if CITO
#if CS
        native
        {
            platform = new ManicDigger.GamePlatformNative();
        }
#elif JAVA
        native
        {
            platform = new ManicDigger.lib.GamePlatformNative();
        }
#elif JS
        native
        {
            platform = new GamePlatformNative();
        }
#elif C
#endif
#else
        platform = new GamePlatformNative();
#endif
        return platform;
    }

    // 1) find files no matter if they are in Data\Local\ or Data\Public\
    // 2) find files no matter if game is in debugger, or installed
    // 3) find files no matter if they end in .png or .jpg
    // Returns URL in JavaScript
    public abstract string GetFullFilePath(string filename);
    public abstract int FloatToInt(float value);
    public abstract string[] StringSplit(string value, string separator, IntRef returnLength);
    public abstract int IntParse(string value);
    public abstract float FloatParse(string value);
    public abstract float MathSqrt(float value);
    public abstract string StringTrim(string value);
    public abstract string IntToString(int value);
    public abstract string Timestamp();
    public abstract string StringFormat(string format, string arg0);
    public abstract string StringFormat2(string format, string arg0, string arg1);
    public abstract void ClipboardSetText(string s);
}

public class IntRef
{
    internal int value;
}

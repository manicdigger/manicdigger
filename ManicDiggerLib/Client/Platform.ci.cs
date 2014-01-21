public abstract class GamePlatform
{
    public static GamePlatform Create()
    {
        GamePlatform platform;
#if CITO
#if CS
        native
        {
            gl = new ManicDigger.GamePlatformNative();
        }
#elif JAVA
        native
        {
            gl = new ManicDigger.lib.GamePlatformNative();
        }
#elif JS
        native
        {
            gl = new GamePlatformNative();
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
}

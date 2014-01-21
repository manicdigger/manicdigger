public class GamePlatform
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
}

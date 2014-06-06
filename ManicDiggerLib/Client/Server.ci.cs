public class ServerCi
{
    public ServerCi()
    {
        one = 1;
    }
    float one;
    internal INetServer mainSocket0;
    internal INetServer mainSocket1;
}

public class ClientStateOnServer
{
    public const int Connecting = 0;
    public const int LoadingGenerating = 1;
    public const int LoadingSending = 2;
    public const int Playing = 3;
}

public class ServerCi
{
    public ServerCi()
    {
        one = 1;
        mainSocketsCount = 3;
    }
    float one;
    internal NetServer[] mainSockets;
    internal int mainSocketsCount;
}

public class ClientStateOnServer
{
    public const int Connecting = 0;
    public const int LoadingGenerating = 1;
    public const int LoadingSending = 2;
    public const int Playing = 3;
}

public class Script
{
    public virtual void OnCreate(ScriptManager manager) { }
    public virtual void OnUse() { }
}

public abstract class ScriptManager
{
    public abstract void SendMessage(int player, string p);
}

public abstract class NetServer
{
    public abstract void SetPort(int port);
    public abstract void Start();
    public abstract NetIncomingMessage ReadMessage();
}

public abstract class NetClient
{
    public abstract void Start();
    public abstract NetConnection Connect(string ip, int port);
    public abstract NetIncomingMessage ReadMessage();
    public abstract void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method);
}

public abstract class NetConnection
{
    public abstract IPEndPointCi RemoteEndPoint();
    public abstract void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel);
    public abstract void Update();
    public abstract bool EqualsConnection(NetConnection connection);
}
public class NetIncomingMessage
{
    internal NetConnection SenderConnection;
    internal NetworkMessageType Type;
    internal byte[] message;
    internal int messageLength;
}
public enum NetworkMessageType
{
    Data,
    Connect,
    Disconnect
}
public class INetOutgoingMessage
{
    internal byte[] message;
    internal int messageLength;
    public void Write(byte[] source, int sourceCount)
    {
        messageLength = sourceCount;
        message = new byte[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            message[i] = source[i];
        }
    }
}

public abstract class IPEndPointCi
{
    public abstract string AddressToString();
}

public class IPEndPointCiDefault : IPEndPointCi
{
    public static IPEndPointCiDefault Create(string address_)
    {
        IPEndPointCiDefault e = new IPEndPointCiDefault();
        e.address = address_;
        return e;
    }
    internal string address;
    public override string AddressToString()
    {
        return address;
    }
}

public enum MyNetDeliveryMethod
{
    Unknown,// = 0,
    Unreliable,// = 1,
    UnreliableSequenced ,//= 2,
    ReliableUnordered,// = 34,
    ReliableSequenced,// = 35,
    ReliableOrdered// = 67,
}

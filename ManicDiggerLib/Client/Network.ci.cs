public abstract class INetServer
{
    public abstract INetPeerConfiguration Configuration();
    public abstract void Start();
    public abstract void Recycle(INetIncomingMessage msg);
    public abstract INetIncomingMessage ReadMessage();
    public abstract INetOutgoingMessage CreateMessage();
}
public abstract class INetPeerConfiguration
{
    public abstract int GetPort();
    public abstract void SetPort(int value);
}

public abstract class INetClient
{
    public abstract void Start();
    public abstract INetConnection Connect(string ip, int port);
    public abstract INetIncomingMessage ReadMessage();
    public abstract INetOutgoingMessage CreateMessage();
    public abstract void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method);
}

public abstract class INetConnection
{
    public abstract IPEndPointCi RemoteEndPoint();
    public abstract void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel);
    public abstract void Update();
    public abstract bool EqualsConnection(INetConnection connection);
}
public abstract class INetIncomingMessage
{
    public abstract INetConnection SenderConnection();
    public abstract byte[] ReadBytes(int numberOfBytes);
    public abstract int LengthBytes();
    public abstract NetworkMessageType Type();
}
public enum NetworkMessageType
{
    Data,
    Connect,
    Disconnect
}
public abstract class INetOutgoingMessage
{
    public abstract void Write(byte[] source, int sourceCount);
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


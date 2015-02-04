public class DummyNetClient : NetClient
{
    internal GamePlatform platform;
    internal DummyNetwork network;
    public override NetConnection Connect(string ip, int port)
    {
        return new DummyNetConnection();
    }

    public override NetIncomingMessage ReadMessage()
    {
        NetIncomingMessage msg = null;
        platform.MonitorEnter(network.ClientReceiveBufferLock);
        {
            if (network.ClientReceiveBuffer.Count() > 0)
            {
                msg = new NetIncomingMessage();
                ByteArray b = network.ClientReceiveBuffer.Dequeue();
                msg.message = b.data;
                msg.messageLength = b.length;
            }
        }
        platform.MonitorExit(network.ClientReceiveBufferLock);
        return msg;
    }

    public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
    {
        platform.MonitorEnter(network.ServerReceiveBufferLock);
        {
            INetOutgoingMessage msg = message;
            ByteArray b = new ByteArray();
            b.data = msg.message;
            b.length = msg.messageLength;
            network.ServerReceiveBuffer.Enqueue(b);
        }
        platform.MonitorExit(network.ServerReceiveBufferLock);
    }

    public override void Start()
    {
    }

    public void SetNetwork(DummyNetwork network_)
    {
        network = network_;
    }

    public void SetPlatform(GamePlatform gamePlatform)
    {
        platform = gamePlatform;
    }
}
public class DummyNetConnection : NetConnection
{
    internal GamePlatform platform;
    internal DummyNetwork network;
    public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
    {
        platform.MonitorEnter(network.ClientReceiveBufferLock);
        {
            INetOutgoingMessage msg2 = msg;
            ByteArray b = new ByteArray();
            b.data = msg2.message;
            b.length = msg2.messageLength;
            network.ClientReceiveBuffer.Enqueue(b);
        }
        platform.MonitorExit(network.ClientReceiveBufferLock);
    }
    public override IPEndPointCi RemoteEndPoint()
    {
        return new DummyIpEndPoint();
    }
    public override void Update()
    {
    }

    public override bool EqualsConnection(NetConnection connection)
    {
        return true;
    }
}
public class DummyIpEndPoint : IPEndPointCi
{
    public override string AddressToString()
    {
        return "127.0.0.1";
    }
}

public class DummyNetServer : NetServer
{
    public DummyNetServer()
    {
        connectedClient = new DummyNetConnection();
    }
    internal GamePlatform platform;
    internal DummyNetwork network;
    public override void Start()
    {
    }

    DummyNetConnection connectedClient;

    bool receivedAnyMessage;

    public override NetIncomingMessage ReadMessage()
    {
        connectedClient.network = network;
        connectedClient.platform = platform;

        NetIncomingMessage msg = null;
        platform.MonitorEnter(network.ServerReceiveBufferLock);
        {
            if (network.ServerReceiveBuffer.Count() > 0)
            {
                if (!receivedAnyMessage)
                {
                    receivedAnyMessage = true;
                    msg = new NetIncomingMessage();
                    msg.Type = NetworkMessageType.Connect;
                    msg.SenderConnection = connectedClient;
                }
                else
                {
                    msg = new NetIncomingMessage();
                    ByteArray b = network.ServerReceiveBuffer.Dequeue();
                    msg.message = b.data;
                    msg.messageLength = b.length;
                    msg.SenderConnection = connectedClient;
                }
            }
        }
        platform.MonitorExit(network.ServerReceiveBufferLock);
        return msg;
    }

    public void SetNetwork(DummyNetwork dummyNetwork)
    {
        network = dummyNetwork;
    }

    public void SetPlatform(GamePlatform gamePlatform)
    {
        platform = gamePlatform;
    }

    public override void SetPort(int port)
    {
    }
}



public class DummyNetOutgoingMessage : INetOutgoingMessage
{
}

public class DummyNetwork
{
    public DummyNetwork()
    {
        Clear();
    }
    internal QueueByteArray ServerReceiveBuffer;
    internal QueueByteArray ClientReceiveBuffer;
    internal MonitorObject ServerReceiveBufferLock;
    internal MonitorObject ClientReceiveBufferLock;
    public void Start(MonitorObject lock1, MonitorObject lock2)
    {
        ServerReceiveBufferLock = lock1;
        ClientReceiveBufferLock = lock2;
    }

    public void Clear()
    {
        ServerReceiveBuffer = new QueueByteArray();
        ClientReceiveBuffer = new QueueByteArray();
    }
}

public class ByteArray
{
    internal byte[] data;
    internal int length;
}

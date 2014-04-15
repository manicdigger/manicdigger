public class DummyNetClient : INetClient
{
    internal GamePlatform platform;
    internal DummyNetwork network;
    public override INetConnection Connect(string ip, int port)
    {
        return new DummyNetConnection();
    }

    public override INetIncomingMessage ReadMessage()
    {
        DummyNetIncomingmessage msg = null;
        platform.MonitorEnter(network.ClientReceiveBufferLock);
        {
            if (network.ClientReceiveBuffer.Count() > 0)
            {
                msg = new DummyNetIncomingmessage();
                msg.message = network.ClientReceiveBuffer.Dequeue();
            }
        }
        platform.MonitorExit(network.ClientReceiveBufferLock);
        return msg;
    }

    public override INetOutgoingMessage CreateMessage()
    {
        return new DummyNetOutgoingMessage();
    }

    public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
    {
        platform.MonitorEnter(network.ServerReceiveBufferLock);
        {
            DummyNetOutgoingMessage msg = platform.CastToDummyNetOutgoingMessage(message);
            network.ServerReceiveBuffer.Enqueue(msg.data);
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
public class DummyNetConnection : INetConnection
{
    internal GamePlatform platform;
    internal DummyNetwork network;
    public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
    {
        platform.MonitorEnter(network.ClientReceiveBufferLock);
        {
            DummyNetOutgoingMessage msg2 = platform.CastToDummyNetOutgoingMessage(msg);
            network.ClientReceiveBuffer.Enqueue(msg2.data);
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

    public override bool EqualsConnection(INetConnection connection)
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

public class DummyNetIncomingmessage : INetIncomingMessage
{
    internal ByteArray message;
    internal INetConnection senderConnection;
    public override INetConnection SenderConnection() { return senderConnection; }

    public override byte[] ReadBytes(int numberOfBytes)
    {
        //if (numberOfBytes != message.Length)
        {
            //throw new Exception();
        }
        return message.data;
    }

    public override int LengthBytes() { return message.length; }

    internal NetworkMessageType type;
    public override NetworkMessageType Type() { return type; }
}

public class DummyNetServer : INetServer
{
    public DummyNetServer()
    {
        connectedClient = new DummyNetConnection();
        configuration = new DummyNetPeerConfiguration();
    }
    internal GamePlatform platform;
    internal DummyNetwork network;
    public override void Start()
    {
    }

    public override void Recycle(INetIncomingMessage msg)
    {
    }

    DummyNetConnection connectedClient;

    bool receivedAnyMessage;

    public override INetIncomingMessage ReadMessage()
    {
        connectedClient.network = network;
        connectedClient.platform = platform;

        DummyNetIncomingmessage msg = null;
        platform.MonitorEnter(network.ServerReceiveBufferLock);
        {
            if (network.ServerReceiveBuffer.Count() > 0)
            {
                if (!receivedAnyMessage)
                {
                    receivedAnyMessage = true;
                    msg = new DummyNetIncomingmessage();
                    msg.type = NetworkMessageType.Connect;
                    msg.senderConnection = connectedClient;
                }
                else
                {
                    msg = new DummyNetIncomingmessage();
                    msg.message = network.ServerReceiveBuffer.Dequeue();
                    msg.senderConnection = connectedClient;
                }
            }
        }
        platform.MonitorExit(network.ServerReceiveBufferLock);
        return msg;
    }

    DummyNetPeerConfiguration configuration;
    public override INetPeerConfiguration Configuration()
    {
        return configuration;
    }

    public override INetOutgoingMessage CreateMessage()
    {
        return new DummyNetOutgoingMessage();
    }

    public void SetNetwork(DummyNetwork dummyNetwork)
    {
        network = dummyNetwork;
    }

    public void SetPlatform(GamePlatform gamePlatform)
    {
        platform = gamePlatform;
    }
}



public class DummyNetOutgoingMessage : INetOutgoingMessage
{
    internal ByteArray data;
    public override void Write(byte[] source, int sourceCount)
    {
        data = new ByteArray();
        data.data = new byte[sourceCount];
        data.length = sourceCount;
        for (int i = 0; i < sourceCount; i++)
        {
            data.data[i] = source[i];
        }
    }
}

public class DummyNetPeerConfiguration : INetPeerConfiguration
{
    internal int Port;

    public override int GetPort()
    {
        return Port;
    }

    public override void SetPort(int value)
    {
        Port = value;
    }
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

public class QueueByteArray
{
    public QueueByteArray()
    {
        items = new ByteArray[1];
        itemsSize = 1;
        count = 0;
    }
    ByteArray[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal ByteArray Dequeue()
    {
        ByteArray ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(ByteArray p)
    {
        if (count == itemsSize)
        {
            ByteArray[] items2 = new ByteArray[itemsSize * 2];
            for (int i = 0; i < itemsSize; i++)
            {
                items2[i] = items[i];
            }
            itemsSize = itemsSize * 2;
            items = items2;
        }
        items[count++] = p;
    }
}

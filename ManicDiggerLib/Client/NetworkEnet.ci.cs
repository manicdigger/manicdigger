public class EnetNetServer : INetServer
{
    public EnetNetServer()
    {
        configuration = new ENetPeerConfiguration();
        event_ = new EnetEventRef();
        messages = new QueueEnetNetIncomingMessage();
    }
    internal GamePlatform platform;

    public override void Start()
    {
        host = platform.EnetCreateHost();
        platform.EnetHostInitializeServer(host, configuration.Port, 256);
    }

    EnetHost host;

    public override void Recycle(INetIncomingMessage msg)
    {
    }

    EnetEventRef event_;

    public override INetIncomingMessage ReadMessage()
    {
        if (messages.Count() > 0)
        {
            return messages.Dequeue();
        }

        if (platform.EnetHostService(host, 0, event_))
        {
            do
            {
                switch (event_.e.Type())
                {
                    case EnetEventType.Connect:
                        {
                            EnetPeer peer = event_.e.Peer();
                            peer.SetUserData(clientid++);
                            EnetNetConnection senderConnectionConnect = new EnetNetConnection();
                            senderConnectionConnect.platform = platform;
                            senderConnectionConnect.peer = event_.e.Peer();
                            EnetNetIncomingMessage message = new EnetNetIncomingMessage();
                            message.senderconnection = senderConnectionConnect;
                            message.type = NetworkMessageType.Connect;
                            messages.Enqueue(message);
                        }
                        break;
                    case EnetEventType.Receive:
                        {
                            byte[] data = event_.e.Packet().GetBytes();
                            event_.e.Packet().Dispose();
                            EnetNetConnection senderConnectionReceive = new EnetNetConnection();
                            senderConnectionReceive.platform = platform;
                            senderConnectionReceive.peer = event_.e.Peer();
                            EnetNetIncomingMessage message = new EnetNetIncomingMessage();
                            message.senderconnection = senderConnectionReceive;
                            message.message = data;
                            message.type = NetworkMessageType.Data;
                            messages.Enqueue(message);
                        }
                        break;
                    case EnetEventType.Disconnect:
                        {
                            EnetNetConnection senderConnectionDisconnect = new EnetNetConnection();
                            senderConnectionDisconnect.platform = platform;
                            senderConnectionDisconnect.peer = event_.e.Peer();
                            EnetNetIncomingMessage message = new EnetNetIncomingMessage();
                            message.senderconnection = senderConnectionDisconnect;
                            message.type = NetworkMessageType.Disconnect;
                            messages.Enqueue(message);
                        }
                        break;
                }
            }
            while (platform.EnetHostCheckEvents(host, event_));
        }
        if (messages.Count() > 0)
        {
            return messages.Dequeue();
        }
        return null;
    }
    int clientid;
    QueueEnetNetIncomingMessage messages;

    ENetPeerConfiguration configuration;

    public override INetPeerConfiguration Configuration()
    {
        return configuration;
    }

    public override INetOutgoingMessage CreateMessage()
    {
        return new EnetNetOutgoingMessage();
    }
}

public class ENetPeerConfiguration : INetPeerConfiguration
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

public class EnetNetIncomingMessage : INetIncomingMessage
{
    internal EnetNetConnection senderconnection;

    public override INetConnection SenderConnection()
    {
        return senderconnection;
    }

    internal byte[] message;
    internal int messageLength;

    public override byte[] ReadBytes(int numberOfBytes)
    {
        return message;
    }

    public override int LengthBytes()
    {
        return messageLength;
    }

    internal NetworkMessageType type;
    public override NetworkMessageType Type() { return type; }
}

public class EnetNetConnection : INetConnection
{
    internal GamePlatform platform;
    internal EnetPeer peer;
    public override IPEndPointCi RemoteEndPoint()
    {
        return IPEndPointCiDefault.Create(peer.GetRemoteAddress().AddressToString());
    }

    public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
    {
        EnetNetOutgoingMessage msg1 = platform.CastToEnetNetOutgoingMessage(msg);
        platform.EnetPeerSend(peer, 0, msg1.message, msg1.messageLength, EnetPacketFlags.Reliable);
    }

    public override void Update()
    {
    }

    public override bool EqualsConnection(INetConnection connection)
    {
        return peer.UserData() == platform.CastToEnetNetConnection(connection).peer.UserData();
    }
}

public class EnetNetOutgoingMessage : INetOutgoingMessage
{
    internal byte[] message;
    internal int messageLength;
    public override void Write(byte[] source, int sourceCount)
    {
        messageLength = sourceCount;
        message = new byte[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            message[i] = source[i];
        }
    }
}

public class EnetNetClient : INetClient
{
    internal GamePlatform platform;
    public override void Start()
    {
        host = platform.EnetCreateHost();
        platform.EnetHostInitialize(host, null, 1, 0, 0, 0);
        tosend = new QueueEnetNetOutgoingMessage();
        messages = new QueueEnetNetIncomingMessage();
    }
    EnetHost host;
    EnetPeer peer;
    bool connected;
    bool connected2;

    public override INetConnection Connect(string ip, int port)
    {
        peer = platform.EnetHostConnect(host, ip, port, 1234, 200);
        connected = true;
        return null;
    }

    public override INetIncomingMessage ReadMessage()
    {
        if (!connected)
        {
            return null;
        }
        if (messages.Count() > 0)
        {
            return messages.Dequeue();
        }
        if (connected2)
        {
            while (tosend.Count() > 0)
            {
                EnetNetOutgoingMessage msg = tosend.Dequeue();
                DoSendPacket(msg);
            }
        }

        EnetEventRef event_ = new EnetEventRef();
        if (platform.EnetHostService(host, 0, event_))
        {
            do
            {
                switch (event_.e.Type())
                {
                    case EnetEventType.Connect:
                        connected2 = true;
                        break;
                    case EnetEventType.Receive:
                        byte[] data = event_.e.Packet().GetBytes();
                        int dataLength = event_.e.Packet().GetBytesCount();
                        event_.e.Packet().Dispose();
                        EnetNetIncomingMessage msg = new EnetNetIncomingMessage();
                        msg.message = data;
                        msg.messageLength = dataLength;
                        messages.Enqueue(msg);
                        break;
                }
            }
            while (platform.EnetHostCheckEvents(host, event_));
        }
        if (messages.Count() > 0)
        {
            return messages.Dequeue();
        }
        return null;
    }

    void DoSendPacket(EnetNetOutgoingMessage msg)
    {
        EnetNetOutgoingMessage msg1 = msg;
        platform.EnetPeerSend(peer, 0, msg1.message, msg1.messageLength, EnetPacketFlags.Reliable);
    }

    QueueEnetNetIncomingMessage messages;

    public override INetOutgoingMessage CreateMessage()
    {
        return new EnetNetOutgoingMessage();
    }
    QueueEnetNetOutgoingMessage tosend;
    public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
    {
        EnetNetOutgoingMessage msg = platform.CastToEnetNetOutgoingMessage(message);
        if (!connected2)
        {
            tosend.Enqueue(msg);
            return;
        }
        DoSendPacket(msg);
    }

    public void SetPlatform(GamePlatform platform_)
    {
        platform = platform_;
    }
}

public class QueueEnetNetIncomingMessage
{
    public QueueEnetNetIncomingMessage()
    {
        items = new EnetNetIncomingMessage[1];
        itemsSize = 1;
        count = 0;
    }
    EnetNetIncomingMessage[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal EnetNetIncomingMessage Dequeue()
    {
        EnetNetIncomingMessage ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(EnetNetIncomingMessage p)
    {
        if (count == itemsSize)
        {
            EnetNetIncomingMessage[] items2 = new EnetNetIncomingMessage[itemsSize * 2];
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



public class QueueEnetNetOutgoingMessage
{
    public QueueEnetNetOutgoingMessage()
    {
        items = new EnetNetOutgoingMessage[1];
        itemsSize = 1;
        count = 0;
    }
    EnetNetOutgoingMessage[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal EnetNetOutgoingMessage Dequeue()
    {
        EnetNetOutgoingMessage ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(EnetNetOutgoingMessage p)
    {
        if (count == itemsSize)
        {
            EnetNetOutgoingMessage[] items2 = new EnetNetOutgoingMessage[itemsSize * 2];
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


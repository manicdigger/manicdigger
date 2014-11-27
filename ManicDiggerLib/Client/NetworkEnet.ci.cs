public class EnetNetServer : NetServer
{
    public EnetNetServer()
    {
        event_ = new EnetEventRef();
        messages = new QueueNetIncomingMessage();
    }
    internal GamePlatform platform;

    public override void Start()
    {
        host = platform.EnetCreateHost();
        platform.EnetHostInitializeServer(host, Port, 256);
    }

    EnetHost host;

    EnetEventRef event_;

    public override NetIncomingMessage ReadMessage()
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
                            NetIncomingMessage message = new NetIncomingMessage();
                            message.SenderConnection = senderConnectionConnect;
                            message.Type = NetworkMessageType.Connect;
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
                            NetIncomingMessage message = new NetIncomingMessage();
                            message.SenderConnection = senderConnectionReceive;
                            message.message = data;
                            message.Type = NetworkMessageType.Data;
                            messages.Enqueue(message);
                        }
                        break;
                    case EnetEventType.Disconnect:
                        {
                            EnetNetConnection senderConnectionDisconnect = new EnetNetConnection();
                            senderConnectionDisconnect.platform = platform;
                            senderConnectionDisconnect.peer = event_.e.Peer();
                            NetIncomingMessage message = new NetIncomingMessage();
                            message.SenderConnection = senderConnectionDisconnect;
                            message.Type = NetworkMessageType.Disconnect;
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
    QueueNetIncomingMessage messages;
    
    int Port;

    public override void SetPort(int port)
    {
        Port = port;
    }
}

public class EnetNetConnection : NetConnection
{
    internal GamePlatform platform;
    internal EnetPeer peer;
    public override IPEndPointCi RemoteEndPoint()
    {
        return IPEndPointCiDefault.Create(peer.GetRemoteAddress().AddressToString());
    }

    public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
    {
        INetOutgoingMessage msg1 = msg;
        platform.EnetPeerSend(peer, 0, msg1.message, msg1.messageLength, EnetPacketFlags.Reliable);
    }

    public override void Update()
    {
    }

    public override bool EqualsConnection(NetConnection connection)
    {
        return peer.UserData() == platform.CastToEnetNetConnection(connection).peer.UserData();
    }
}

public class EnetNetClient : NetClient
{
    internal GamePlatform platform;
    public override void Start()
    {
        host = platform.EnetCreateHost();
        platform.EnetHostInitialize(host, null, 1, 0, 0, 0);
        tosend = new QueueINetOutgoingMessage();
        messages = new QueueNetIncomingMessage();
    }
    EnetHost host;
    EnetPeer peer;
    bool connected;
    bool connected2;

    public override NetConnection Connect(string ip, int port)
    {
        peer = platform.EnetHostConnect(host, ip, port, 1234, 200);
        connected = true;
        return null;
    }

    public override NetIncomingMessage ReadMessage()
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
                INetOutgoingMessage msg = tosend.Dequeue();
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
                        NetIncomingMessage msg = new NetIncomingMessage();
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

    void DoSendPacket(INetOutgoingMessage msg)
    {
        INetOutgoingMessage msg1 = msg;
        platform.EnetPeerSend(peer, 0, msg1.message, msg1.messageLength, EnetPacketFlags.Reliable);
    }

    QueueNetIncomingMessage messages;

    QueueINetOutgoingMessage tosend;
    public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
    {
        INetOutgoingMessage msg = message;
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

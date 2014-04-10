public class TcpPeerConfiguration : INetPeerConfiguration
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

public class TcpNetIncomingMessage : INetIncomingMessage
{
    public override INetConnection SenderConnection()
    {
        return null;
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

public class TcpNetOutgoingMessage : INetOutgoingMessage
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

public class TcpNetClient : INetClient
{
    public TcpNetClient()
    {
        incoming = new QueueByte();
        data = new byte[dataLength];
        connected = new BoolRef();
    }
    internal GamePlatform platform;
    public override void Start()
    {
        tosend = new QueueTcpNetOutgoingMessage();
    }

    BoolRef connected;

    public override INetConnection Connect(string ip, int port)
    {
        platform.TcpConnect(ip, port, connected);
        return null;
    }

    QueueByte incoming;
    public const int MaxPacketLength = 1024 * 4;

    byte[] data;
    const int dataLength = 1024;
    public override INetIncomingMessage ReadMessage()
    {
        if (connected.value)
        {
            while (tosend.Count() > 0)
            {
                TcpNetOutgoingMessage msg = tosend.Dequeue();
                DoSendPacket(msg);
            }
        }
        TcpNetIncomingMessage message = GetMessage();
        if (message != null)
        {
            return message;
        }

        for (int k = 0; k < 1; k++)
        {
            int received = platform.TcpReceive(data, dataLength);
            if (received <= 0)
            {
                break;
            }
            for (int i = 0; i < received; i++)
            {
                incoming.Enqueue(data[i]);
            }
        }

        message = GetMessage();
        if (message != null)
        {
            return message;
        }

        return null;
    }

    TcpNetIncomingMessage GetMessage()
    {
        if (incoming.count >= 4)
        {
            byte[] length = new byte[4];
            incoming.PeekRange(length, 4);
            int messageLength = ReadInt(length, 0);
            if (incoming.count >= 4 + messageLength)
            {
                incoming.DequeueRange(new byte[4], 4);
                TcpNetIncomingMessage msg = new TcpNetIncomingMessage();
                msg.message = new byte[messageLength];
                msg.messageLength = messageLength;
                incoming.DequeueRange(msg.message, msg.messageLength);
                return msg;
            }
        }
        return null;
    }

    void WriteInt(byte[] writeBuf, int writePos, int n)
    {
        int a = (n >> 24) & 0xFF;
        int b = (n >> 16) & 0xFF;
        int c = (n >> 8) & 0xFF;
        int d = n & 0xFF;
        writeBuf[writePos] = Game.IntToByte(a);
        writeBuf[writePos + 1] = Game.IntToByte(b);
        writeBuf[writePos + 2] = Game.IntToByte(c);
        writeBuf[writePos + 3] = Game.IntToByte(d);
    }

    int ReadInt(byte[] readBuf, int readPos)
    {
        int n = readBuf[readPos] << 24;
        n |= readBuf[readPos + 1] << 16;
        n |= readBuf[readPos + 2] << 8;
        n |= readBuf[readPos + 3];
        return n;
    }

    void DoSendPacket(TcpNetOutgoingMessage msg)
    {
        byte[] packet = new byte[msg.messageLength + 4];
        WriteInt(packet, 0, msg.messageLength);
        for (int i = 0; i < msg.messageLength; i++)
        {
            packet[i + 4] = msg.message[i];
        }
        platform.TcpSend(packet, msg.messageLength + 4);
    }

    public override INetOutgoingMessage CreateMessage()
    {
        return new TcpNetOutgoingMessage();
    }
    QueueTcpNetOutgoingMessage tosend;
    public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
    {
        TcpNetOutgoingMessage msg = platform.CastToTcpNetOutgoingMessage(message);
        if (!connected.value)
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

public class QueueByte
{
    public QueueByte()
    {
        max = 1024 * 1024 * 5;
        items = new byte[max];
    }
    byte[] items;
    internal int start;
    internal int count;
    internal int max;

    public int GetCount()
    {
        return count;
    }

    public void Enqueue(byte value)
    {
        int pos = start + count;
        pos = pos % max;
        count++;
        items[pos] = value;
    }

    public byte Dequeue()
    {
        byte ret = items[start];
        start++;
        start = start % max;
        count--;
        return ret;
    }


    public void DequeueRange(byte[] data, int length)
    {
        for (int i = 0; i < length; i++)
        {
            data[i] = Dequeue();
        }
    }

    internal void PeekRange(byte[] data, int length)
    {
        for (int i = 0; i < length; i++)
        {
            data[i] = items[(start + i) % max];
        }
    }
}

public class QueueTcpNetIncomingMessage
{
    public QueueTcpNetIncomingMessage()
    {
        items = new TcpNetIncomingMessage[1];
        itemsSize = 1;
        count = 0;
    }
    TcpNetIncomingMessage[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal TcpNetIncomingMessage Dequeue()
    {
        TcpNetIncomingMessage ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(TcpNetIncomingMessage p)
    {
        if (count == itemsSize)
        {
            TcpNetIncomingMessage[] items2 = new TcpNetIncomingMessage[itemsSize * 2];
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



public class QueueTcpNetOutgoingMessage
{
    public QueueTcpNetOutgoingMessage()
    {
        items = new TcpNetOutgoingMessage[1];
        itemsSize = 1;
        count = 0;
    }
    TcpNetOutgoingMessage[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal TcpNetOutgoingMessage Dequeue()
    {
        TcpNetOutgoingMessage ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(TcpNetOutgoingMessage p)
    {
        if (count == itemsSize)
        {
            TcpNetOutgoingMessage[] items2 = new TcpNetOutgoingMessage[itemsSize * 2];
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


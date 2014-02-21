using System;
using System.Collections.Generic;
using System.Net;

namespace ManicDigger
{
    public class DummyNetClient : INetClient
    {
        public DummyNetwork network;
        public override INetConnection Connect(string ip, int port)
        {
            return new DummyNetConnection();
        }

        public override INetIncomingMessage ReadMessage()
        {
            lock (network.ClientReceiveBuffer)
            {
                if (network.ClientReceiveBuffer.Count() > 0)
                {
                    var msg = new DummyNetIncomingmessage();
                    msg.message = network.ClientReceiveBuffer.Dequeue();
                    return msg;
                }
                else
                {
                    return null;
                }
            }
        }

        public override INetOutgoingMessage CreateMessage()
        {
            return new DummyNetOutgoingMessage();
        }

        public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            lock (network.ServerReceiveBuffer)
            {
                network.ServerReceiveBuffer.Enqueue(((DummyNetOutgoingMessage)message).data);
            }
        }

        public override void Start()
        {
        }
    }
    public class DummyNetConnection : INetConnection
    {
        public DummyNetwork network;
        public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            lock (network.ClientReceiveBuffer)
            {
                network.ClientReceiveBuffer.Enqueue(((DummyNetOutgoingMessage)msg).data);
            }
        }
        public override IPEndPointCi RemoteEndPoint()
        {
            return new DummyIpEndPoint();
        }
        public override void Update()
        {
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
        public byte[] message;
        internal INetConnection senderConnection;
        public override INetConnection SenderConnection() { return senderConnection; }

        public override byte[] ReadBytes(int numberOfBytes)
        {
            if (numberOfBytes != message.Length)
            {
                throw new Exception();
            }
            return message;
        }

        public override int LengthBytes (){ return message.Length; }

        internal NetworkMessageType type;
        public override NetworkMessageType Type (){ return type; }
    }
    public class DummyNetOutgoingMessage : INetOutgoingMessage
    {
        public byte[] data;
        public override void Write(byte[] source)
        {
            data = new byte[source.Length];
            Array.Copy(source, data, source.Length);
        }
    }
    public class DummyNetServer : INetServer
    {
        public DummyNetwork network;
        public override void Start()
        {
        }

        public override void Recycle(INetIncomingMessage msg)
        {
        }

        INetConnection connectedClient = new DummyNetConnection();

        bool receivedAnyMessage;

        public override INetIncomingMessage ReadMessage()
        {
            ((DummyNetConnection)connectedClient).network = network;
            lock (network.ServerReceiveBuffer)
            {
                if (network.ServerReceiveBuffer.Count() > 0)
                {
                    if (!receivedAnyMessage)
                    {
                        receivedAnyMessage = true;
                        return new DummyNetIncomingmessage() { type = NetworkMessageType.Connect, senderConnection = connectedClient };
                    }
                    return new DummyNetIncomingmessage() { message = network.ServerReceiveBuffer.Dequeue(), senderConnection = connectedClient };
                }
                else
                {
                    return null;
                }
            }
        }

        DummyNetPeerConfiguration configuration = new DummyNetPeerConfiguration();
        public override INetPeerConfiguration Configuration()
        {
            return configuration;
        }

        public override INetOutgoingMessage CreateMessage()
        {
            return new DummyNetOutgoingMessage();
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
        public QueueByteArray ServerReceiveBuffer = new QueueByteArray();
        public QueueByteArray ClientReceiveBuffer = new QueueByteArray();
    }

    public class QueueByteArray
    {
        public QueueByteArray()
        {
            items = new byte[1][];
            itemsSize = 1;
            count = 0;
        }
        byte[][] items;
        int count;
        int itemsSize;

        internal int Count()
        {
            return count;
        }

        internal byte[] Dequeue()
        {
            byte[] ret = items[0];
            for (int i = 0; i < count - 1; i++)
            {
                items[i] = items[i + 1];
            }
            count--;
            return ret;
        }

        internal void Enqueue(byte[] p)
        {
            if (count == itemsSize)
            {
                byte[][] items2 = new byte[itemsSize * 2][];
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
}


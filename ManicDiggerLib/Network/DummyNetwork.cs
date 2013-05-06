using System;
using System.Collections.Generic;
using System.Net;

namespace ManicDigger
{
    public class DummyNetClient : INetClient
    {
        public DummyNetwork network;
        public INetConnection Connect(string ip, int port)
        {
            return new DummyNetConnection();
        }

        public INetIncomingMessage ReadMessage()
        {
            lock (network.ClientReceiveBuffer)
            {
                if (network.ClientReceiveBuffer.Count > 0)
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

        public INetOutgoingMessage CreateMessage()
        {
            return new DummyNetOutgoingMessage();
        }

        public void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            lock (network.ServerReceiveBuffer)
            {
                network.ServerReceiveBuffer.Enqueue(((DummyNetOutgoingMessage)message).data);
            }
        }

        public void Start()
        {
        }
    }
    public class DummyNetConnection : INetConnection
    {
        public DummyNetwork network;
        public void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            lock (network.ClientReceiveBuffer)
            {
                network.ClientReceiveBuffer.Enqueue(((DummyNetOutgoingMessage)msg).data);
            }
        }
        public IPEndPoint RemoteEndPoint
        {
            get { return new IPEndPoint(IPAddress.Loopback, 0); }
        }
        public void Update()
        {
        }
    }
    public class DummyNetIncomingmessage : INetIncomingMessage
    {
        public byte[] message;
        public INetConnection SenderConnection { get; set; }

        public byte[] ReadBytes(int numberOfBytes)
        {
            if (numberOfBytes != message.Length)
            {
                throw new Exception();
            }
            return message;
        }

        public int LengthBytes { get { return message.Length; } }

        public MessageType Type { get; set; }
    }
    public class DummyNetOutgoingMessage : INetOutgoingMessage
    {
        public byte[] data;
        public void Write(byte[] source)
        {
            data = new byte[source.Length];
            Array.Copy(source, data, source.Length);
        }
    }
    public class DummyNetServer : INetServer
    {
        public DummyNetwork network;
        public void Start()
        {
        }

        public void Recycle(INetIncomingMessage msg)
        {
        }

        INetConnection connectedClient = new DummyNetConnection();

        bool receivedAnyMessage;

        public INetIncomingMessage ReadMessage()
        {
            ((DummyNetConnection)connectedClient).network = network;
            lock (network.ServerReceiveBuffer)
            {
                if (network.ServerReceiveBuffer.Count > 0)
                {
                    if (!receivedAnyMessage)
                    {
                        receivedAnyMessage = true;
                        return new DummyNetIncomingmessage() { Type = MessageType.Connect, SenderConnection = connectedClient };
                    }
                    return new DummyNetIncomingmessage() { message = network.ServerReceiveBuffer.Dequeue(), SenderConnection = connectedClient };
                }
                else
                {
                    return null;
                }
            }
        }

        DummyNetPeerConfiguration configuration = new DummyNetPeerConfiguration();
        public INetPeerConfiguration Configuration
        {
            get { return configuration; }
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new DummyNetOutgoingMessage();
        }
    }

    public class DummyNetPeerConfiguration : INetPeerConfiguration
    {
        public int Port { get; set; }
    }

    public class DummyNetwork
    {
        public Queue<byte[]> ServerReceiveBuffer = new Queue<byte[]>();
        public Queue<byte[]> ClientReceiveBuffer = new Queue<byte[]>();
    }
}


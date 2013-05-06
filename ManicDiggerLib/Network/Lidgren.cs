using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using System.Diagnostics;

namespace ManicDigger
{
    public class MyNetServer : INetServer
    {
        public NetServer server;

        public void Start()
        {
            server.Start();
        }

        public void Recycle(INetIncomingMessage msg)
        {
            server.Recycle(((MyNetIncomingMessage)msg).message);
        }

        public INetIncomingMessage ReadMessage()
        {
            NetIncomingMessage msg = server.ReadMessage();
            if (msg == null)
            {
                return null;
            }
            if (msg.MessageType != NetIncomingMessageType.Data)
            {
                return null;
            }
            return new MyNetIncomingMessage() { message = msg };
        }

        public INetPeerConfiguration Configuration
        {
            get { return new MyNetPeerConfiguration() { configuration = server.Configuration}; }
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new MyNetOutgoingMessage() { message = server.CreateMessage() };
        }
    }

    public class MyNetPeerConfiguration : INetPeerConfiguration
    {
        public NetPeerConfiguration configuration;

        public int Port
        {
            get
            {
                return configuration.Port;
            }
            set
            {
                configuration.Port = value;
            }
        }
    }

    public class MyNetClient : INetClient
    {
        public NetClient client;
        public INetConnection Connect(string ip, int port)
        {
            return new MyNetConnection() { netConnection = client.Connect(ip, port) };
        }

        public INetIncomingMessage ReadMessage()
        {
            NetIncomingMessage msg = client.ReadMessage();
            if (msg == null)
            {
                return null;
            }
            if (msg.MessageType != NetIncomingMessageType.Data)
            {
                return null;
            }
            return new MyNetIncomingMessage() { message = msg };
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new MyNetOutgoingMessage() { message = client.CreateMessage() };
        }

        public void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            client.SendMessage(((MyNetOutgoingMessage)message).message, (NetDeliveryMethod)method);
        }

        public void Start()
        {
            client.Start();
        }
    }

    public class MyNetOutgoingMessage : INetOutgoingMessage
    {
        public NetOutgoingMessage message;
        public void Write(byte[] source) { message.Write(source); }
    }

    public class MyNetIncomingMessage : INetIncomingMessage
    {
        public NetIncomingMessage message;
        public INetConnection SenderConnection { get { return new MyNetConnection() {  netConnection = message.SenderConnection }; } }
        public byte[] ReadBytes(int numberOfBytes) { return message.ReadBytes(numberOfBytes); }
        public int LengthBytes { get { return message.LengthBytes; } }
        public MessageType Type { get; set; }
    }
    //Must limit amount of data sent at once because sending too much saturates lidgren-net and crashes client.
    public class MyNetConnection : INetConnection
    {
        Stopwatch s = new Stopwatch();
        public MyNetConnection()
        {
            s.Start();
        }

        public int MaxBytes = 10 * 1000;
        public float PerSeconds = 0.1f;
        public int currentbytes;

        public Queue<INetOutgoingMessage> queued = new Queue<INetOutgoingMessage>();

        public NetConnection netConnection;
        public void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            if (method != MyNetDeliveryMethod.ReliableOrdered || sequenceChannel != 0)
            {
                netConnection.SendMessage(((MyNetOutgoingMessage)msg).message, (NetDeliveryMethod)method, sequenceChannel);
            }
            else
            {
                int len = ((MyNetOutgoingMessage)msg).message.LengthBytes;
                if ((currentbytes == 0 || currentbytes + len <= MaxBytes) && queued.Count == 0)
                {
                    netConnection.SendMessage(((MyNetOutgoingMessage)msg).message, (NetDeliveryMethod)method, sequenceChannel);
                    currentbytes += len;
                }
                else
                {
                    queued.Enqueue(msg);
                }
            }
        }
        public IPEndPoint RemoteEndPoint
        {
            get { return netConnection.RemoteEndPoint; }
        }
        public override bool Equals(object obj)
        {
            if (obj != null && obj is MyNetConnection)
            {
                return netConnection.Equals(((MyNetConnection)obj).netConnection);
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return netConnection.GetHashCode();
        }
        public void Update()
        {
            if (s.Elapsed.TotalSeconds >= PerSeconds)
            {
                //process up to MaxBytes
                while (queued.Count > 0)
                {
                    MyNetOutgoingMessage m = (MyNetOutgoingMessage)queued.Peek();
                    if (currentbytes == 0 || m.message.LengthBytes + currentbytes < MaxBytes)
                    {
                        queued.Dequeue();
                        netConnection.SendMessage(m.message, NetDeliveryMethod.ReliableOrdered, 0);
                        currentbytes += m.message.LengthBytes;
                    }
                    else
                    {
                        break;
                    }
                }
                s.Reset();
                s.Start();
                currentbytes = 0;
            }
        }
    }
}


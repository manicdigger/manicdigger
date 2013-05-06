using System;
using System.Collections.Generic;
using System.Net;

namespace ManicDigger
{
    public class EnetNetServer : INetServer
    {
        public void Start()
        {
            host = new ENet.Host();
            host.InitializeServer(configuration.Port, 256);
        }

        ENet.Host host;

        public void Recycle(INetIncomingMessage msg)
        {
        }

        public INetIncomingMessage ReadMessage()
        {
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }

            ENet.Event event_;
            if (host.Service(0, out event_))
            {
                do
                {
                    switch (event_.Type)
                    {
                        case ENet.EventType.Connect:
                            var peer = event_.Peer;
                            peer.UserData = new IntPtr(clientid++);
                            var senderConnectionConnect = new EnetNetConnection() { peer = event_.Peer };
                            messages.Enqueue(new EnetNetIncomingMessage() { senderconnection = senderConnectionConnect, Type = MessageType.Connect });
                            break;
                        case ENet.EventType.Receive:
                            byte[] data = event_.Packet.GetBytes();
                            event_.Packet.Dispose();
                            var senderConnectionReceive = new EnetNetConnection() { peer = event_.Peer };
                            messages.Enqueue(new EnetNetIncomingMessage() { senderconnection = senderConnectionReceive, message = data, Type = MessageType.Data });
                            break;
                        case ENet.EventType.Disconnect:
                            var senderConnectionDisconnect = new EnetNetConnection() { peer = event_.Peer };
                            messages.Enqueue(new EnetNetIncomingMessage() { senderconnection = senderConnectionDisconnect, Type = MessageType.Disconnect });
                            break;
                    }
                }
                while (host.CheckEvents(out event_));
            }
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            return null;
        }
        int clientid;
        Queue<EnetNetIncomingMessage> messages = new Queue<EnetNetIncomingMessage>();

        private ENetPeerConfiguration configuration = new ENetPeerConfiguration();
        public class ENetPeerConfiguration : INetPeerConfiguration
        {
            public int Port { get; set; }
        }

        public INetPeerConfiguration Configuration
        {
            get { return configuration; }
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new EnetNetOutgoingMessage();
        }
    }

    public class EnetNetIncomingMessage : INetIncomingMessage
    {
        public EnetNetConnection senderconnection;

        public INetConnection SenderConnection
        {
            get { return senderconnection; }
        }

        public byte[] message;

        public byte[] ReadBytes(int numberOfBytes)
        {
            return message;
        }

        public int LengthBytes
        {
            get { return message.Length; }
        }

        public MessageType Type { get; set; }
    }

    public class EnetNetConnection : INetConnection
    {
        public ENet.Peer peer;
        public IPEndPoint RemoteEndPoint
        {
            get { return peer.GetRemoteAddress(); }
        }

        public void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            var msg1 = (EnetNetOutgoingMessage)msg;
            peer.Send(0, msg1.message, ENet.PacketFlags.Reliable);
        }

        public void Update()
        {
        }

        public unsafe override bool Equals(object obj)
        {
            return peer.UserData == ((EnetNetConnection)obj).peer.UserData;
        }

        public unsafe override int GetHashCode()
        {
            return peer.UserData.GetHashCode();
        }
    }

    public class EnetNetOutgoingMessage : INetOutgoingMessage
    {
        public byte[] message;
        public void Write(byte[] source)
        {
            message = (byte[])source.Clone();
        }
    }

    public class EnetNetClient : INetClient
    {
        public void Start()
        {
            host = new ENet.Host();
            host.Initialize(null, 1, 0, 0, 0);
        }
        ENet.Host host;
        ENet.Peer peer;
        bool connected;
        bool connected2;

        public INetConnection Connect(string ip, int port)
        {
            peer = host.Connect(ip, port, 1234, 200);
            connected = true;
            return null;
        }

        public INetIncomingMessage ReadMessage()
        {
            if (!connected)
            {
                return null;
            }
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            if (connected2)
            {
                while (tosend.Count > 0)
                {
                    var msg = tosend.Dequeue();
                    DoSendPacket(msg);
                }
            }

            ENet.Event event_;
            if (host.Service(0, out event_))
            {
                do
                {
                    switch (event_.Type)
                    {
                        case ENet.EventType.Connect:
                            connected2 = true;
                            break;
                        case ENet.EventType.Receive:
                            byte[] data = event_.Packet.GetBytes();
                            event_.Packet.Dispose();
                            messages.Enqueue(new EnetNetIncomingMessage() { message = data });
                            break;
                    }
                }
                while (host.CheckEvents(out event_));
            }
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            return null;
        }

        private void DoSendPacket(EnetNetOutgoingMessage msg)
        {
            var msg1 = (EnetNetOutgoingMessage)msg;
            peer.Send(0, msg1.message, ENet.PacketFlags.Reliable);
        }

        Queue<EnetNetIncomingMessage> messages = new Queue<EnetNetIncomingMessage>();

        public INetOutgoingMessage CreateMessage()
        {
            return new EnetNetOutgoingMessage();
        }
        Queue<EnetNetOutgoingMessage> tosend = new Queue<EnetNetOutgoingMessage>();
        public void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            var msg = (EnetNetOutgoingMessage)message;
            if (!connected2)
            {
                tosend.Enqueue(msg);
                return;
            }
            DoSendPacket(msg);
        }
    }
}
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ManicDigger
{
    public class TcpNetServer : INetServer
    {
        public override void Start()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            System.Net.IPEndPoint iep = new System.Net.IPEndPoint(IPAddress.Any, configuration.Port);
            socket.Bind(iep);
            socket.Listen(10);
        }

        Socket socket;

        public override void Recycle(INetIncomingMessage msg)
        {
        }

        Dictionary<Socket, TcpNetConnection> clients = new Dictionary<Socket, TcpNetConnection>();

        byte[] data = new byte[1024 * 16];
        public override INetIncomingMessage ReadMessage()
        {
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            if (socket.Poll(0, SelectMode.SelectRead)) //Test for new connections
            {
                Socket client1 = socket.Accept();
                IPEndPoint iep1 = (IPEndPoint)client1.RemoteEndPoint;
                clients[client1] = (new TcpNetConnection() { socket = client1 });
            }
            ArrayList copylist = new ArrayList();
            copylist.AddRange(clients.Keys);
            if (copylist.Count != 0)
            {
                Socket.Select(copylist, null, null, 0);
            }
            foreach (Socket s in new List<Socket>(clients.Keys))
            {
                int recv = 0;
                try
                {
                    recv = s.Receive(data);
                }
                catch
                {
                    recv = 0;
                }
                if (recv == 0)
                {
                    try
                    {
                        s.Disconnect(false);
                    }
                    catch
                    {
                    }
                    clients.Remove(s);
                }
                else
                {
                    for (int i = 0; i < recv; i++)
                    {
                        clients[s].received.Add(data[i]);
                    }
                }
            }
            foreach (var k in clients)
            {
                while (k.Value.received.Count > 4)
                {
                    MemoryStream ms = new MemoryStream(k.Value.received.ToArray()); // todo
                    BinaryReader br = new BinaryReader(ms);
                    int length = br.ReadInt32();
                    if (k.Value.received.Count < sizeof(int) + length)
                    {
                        break;
                    }
                    var msg = new TcpNetIncomingMessage();
                    msg.data = br.ReadBytes(length);
                    msg.senderconnection = k.Value;
                    messages.Enqueue(msg);
                    k.Value.received.RemoveRange(0, sizeof(int) + length);
                }
            }
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            return null;
        }
        Queue<INetIncomingMessage> messages = new Queue<INetIncomingMessage>();

        TcpNetPeerConfiguration configuration = new TcpNetPeerConfiguration();

        public override INetPeerConfiguration Configuration()
        {
            return configuration;
        }

        public override INetOutgoingMessage CreateMessage()
        {
            return new TcpNetOutgoingMessage();
        }
    }
    public class TcpNetPeerConfiguration : INetPeerConfiguration
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
    public class TcpNetOutgoingMessage : INetOutgoingMessage
    {
        public byte[] message;
        public override void Write(byte[] source)
        {
            message = (byte[])source.Clone();
        }
    }
    public class TcpNetIncomingMessage : INetIncomingMessage
    {
        public byte[] data;
        public TcpNetConnection senderconnection;
        public override INetConnection SenderConnection() { return senderconnection; }

        public override byte[] ReadBytes(int numberOfBytes)
        {
            return data;
        }

        public override int LengthBytes()
        {
            return data.Length;
        }

        internal NetworkMessageType type;
        public override NetworkMessageType Type() { return type; }
    }
    public class TcpNetClient : INetClient
    {
        public override void Start()
        {
        }

        public override INetConnection Connect(string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Connect(ip, port);
            return null;
        }
        Socket socket;
        List<byte> received = new List<byte>();
        public override INetIncomingMessage ReadMessage()
        {
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            bool again = false;
            for (; ; )
            {
                if (!(socket.Poll(0, SelectMode.SelectRead)))
                {
                    if (!again)
                    {
                        again = true;
                        goto process;
                    }
                    break;
                }
                byte[] data = new byte[1024];
                int recv;
                try
                {
                    recv = socket.Receive(data);
                }
                catch
                {
                    recv = 0;
                }
                if (recv == 0)
                {
                    //disconnected
                    return null;
                }
                for (int i = 0; i < recv; i++)
                {
                    received.Add(data[i]);
                }
            process:
                for (; ; )
                {
                    if (received.Count < sizeof(int))
                    {
                        break;
                    }
                    MemoryStream ms = new MemoryStream(received.ToArray());
                    BinaryReader br = new BinaryReader(ms);
                    int length = br.ReadInt32();
                    if (received.Count < sizeof(int) + length)
                    {
                        break;
                    }
                    TcpNetIncomingMessage msg = new TcpNetIncomingMessage();
                    msg.data = br.ReadBytes(length);
                    messages.Enqueue(msg);
                    received.RemoveRange(0, sizeof(int) + length);
                }
                if (messages.Count > 0)
                {
                    return messages.Dequeue();
                }
                return null;
            }
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
            return null;
        }
        Queue<INetIncomingMessage> messages = new Queue<INetIncomingMessage>();

        public override INetOutgoingMessage CreateMessage()
        {
            return new TcpNetOutgoingMessage();
        }

        public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            var msg = (TcpNetOutgoingMessage)message;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((int)msg.message.Length);
            bw.Write(msg.message);
            socket.BeginSend(ms.ToArray(), 0, (int)ms.Length, SocketFlags.None, EmptyCallback, new object());
        }

        void EmptyCallback(IAsyncResult ar)
        {
        }

        void Update()
        {
        }
    }
    public class TcpNetConnection : INetConnection
    {
        public Socket socket;
        public List<byte> received = new List<byte>();
        public override IPEndPointCi RemoteEndPoint()
        {
            return IPEndPointCiDefault.Create(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
        }

        public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            var msg1 = (TcpNetOutgoingMessage)msg;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((int)msg1.message.Length);
            bw.Write(msg1.message);
            socket.BeginSend(ms.ToArray(), 0, (int)ms.Length, SocketFlags.None, EmptyCallback, new object());
        }
        void EmptyCallback(IAsyncResult ar)
        {
        }

        public override void Update()
        {
        }

        public override int GetHashCode()
        {
            return socket.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var obj1 = (TcpNetConnection)obj;
            return obj1 != null && obj1.socket.Equals(this.socket);
        }
    }
}


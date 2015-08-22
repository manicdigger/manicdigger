using System.Net.Sockets;
using System.Text;
using System;
using System.Collections.Generic;
using System.Net;

public class TcpNetServer : NetServer
{
    public TcpNetServer()
    {
        messages = new Queue<NetIncomingMessage>();
        server = new ServerManager();
    }

    public override void Start()
    {
        server.StartServer(Port);
        server.Connected += new EventHandler<ConnectionEventArgs>(server_Connected);
        server.ReceivedMessage += new EventHandler<MessageEventArgs>(server_ReceivedMessage);
        server.Disconnected += new EventHandler<ConnectionEventArgs>(server_Disconnected);
    }

    void server_Connected(object sender, ConnectionEventArgs e)
    {
        NetIncomingMessage msg = new NetIncomingMessage();
        msg.Type = NetworkMessageType.Connect;
        msg.SenderConnection = new TcpNetConnection() { peer = (TcpConnection)e.ClientId };
        lock (messages)
        {
            messages.Enqueue(msg);
        }
    }

    void server_Disconnected(object sender, ConnectionEventArgs e)
    {
        NetIncomingMessage msg = new NetIncomingMessage();
        msg.Type = NetworkMessageType.Disconnect;
        msg.SenderConnection = new TcpNetConnection() { peer = (TcpConnection)e.ClientId };
        lock (messages)
        {
            messages.Enqueue(msg);
        }
    }

    void server_ReceivedMessage(object sender, MessageEventArgs e)
    {
        NetIncomingMessage msg = new NetIncomingMessage();
        msg.Type = NetworkMessageType.Data;
        msg.message = e.data;
        msg.messageLength = e.data.Length;
        msg.SenderConnection = new TcpNetConnection() { peer = (TcpConnection)e.ClientId };
        lock (messages)
        {
            messages.Enqueue(msg);
        }
    }

    ServerManager server;

    public override NetIncomingMessage ReadMessage()
    {
        lock (messages)
        {
            if (messages.Count > 0)
            {
                return messages.Dequeue();
            }
        }

        return null;
    }
    Queue<NetIncomingMessage> messages;

    int Port;

    public override void SetPort(int port)
    {
        Port = port;
    }
}

public class TcpNetConnection : NetConnection
{
    public TcpConnection peer;

    public override IPEndPointCi RemoteEndPoint()
    {
        return IPEndPointCiDefault.Create(peer.address);
    }

    public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
    {
        INetOutgoingMessage msg1 = (INetOutgoingMessage)msg;
        byte[] data = new byte[msg1.messageLength];
        for (int i = 0; i < msg1.messageLength; i++)
        {
            data[i] = msg1.message[i];
        }
        peer.Send(data);
    }

    public override void Update()
    {
    }

    public override bool EqualsConnection(NetConnection connection)
    {
        return peer.sock == ((TcpNetConnection)connection).peer.sock;
    }
}

public class ServerManager
{
    Socket sock;
    IPAddress addr = IPAddress.Any;
    public void StartServer(int port)
    {
        this.sock = new Socket(
            addr.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        sock.NoDelay = true;
        sock.Bind(new IPEndPoint(this.addr, port));
        this.sock.Listen(10);
        this.sock.BeginAccept(this.OnConnectRequest, sock);
    }

    void OnConnectRequest(IAsyncResult result)
    {
        try
        {
            Socket sock = (Socket)result.AsyncState;

            TcpConnection newConn = new TcpConnection(sock.EndAccept(result));
            newConn.ReceivedMessage += new EventHandler<MessageEventArgs>(newConn_ReceivedMessage);
            newConn.Disconnected += new EventHandler<ConnectionEventArgs>(newConn_Disconnected);
            sock.BeginAccept(this.OnConnectRequest, sock);
        }
        catch
        {
        }
    }

    void newConn_Disconnected(object sender, ConnectionEventArgs e)
    {
        try
        {
            Disconnected(sender, e);
        }
        catch //(Exception ex)
        {
            // Console.WriteLine(ex.ToString());
        }
    }

    void newConn_ReceivedMessage(object sender, MessageEventArgs e)
    {
        try
        {
            if (Connected != null)
            {
                TcpConnection sender_ = (TcpConnection)sender;
                if (!sender_.connected)
                {
                    sender_.connected = true;
                    Connected(this, new ConnectionEventArgs() { ClientId = sender_ });
                }
            }
            ReceivedMessage(sender, e);
        }
        catch //(Exception ex)
        {
            // Console.WriteLine(ex.ToString());
        }
    }

    public event EventHandler<ConnectionEventArgs> Connected;
    public event EventHandler<MessageEventArgs> ReceivedMessage;
    public event EventHandler<ConnectionEventArgs> Disconnected;

    public void Send(object sender, byte[] data)
    {
        try
        {
            ((TcpConnection)sender).Send(data);
        }
        catch
        {
        }
    }
}

public class TcpConnection
{
    public Socket sock;
    public string address;

    public TcpConnection(Socket s)
    {
        this.sock = s;
        address = s.RemoteEndPoint.ToString();
        this.BeginReceive();
    }
    void BeginReceive()
    {
        try
        {
            this.sock.BeginReceive(
                    this.dataRcvBuf, 0,
                    this.dataRcvBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(this.OnBytesReceived),
                    this);
        }
        catch
        {
            InvokeDisconnected();
        }
    }
    public bool connected;
    byte[] dataRcvBuf = new byte[1024 * 8];
    protected void OnBytesReceived(IAsyncResult result)
    {
        try
        {
            int nBytesRec;
            try
            {
                nBytesRec = this.sock.EndReceive(result);
            }
            catch
            {
                try
                {
                    this.sock.Close();
                }
                catch
                {
                }
                InvokeDisconnected();
                return;
            }
            if (nBytesRec <= 0)
            {
                try
                {
                    this.sock.Close();
                }
                catch
                {
                }
                InvokeDisconnected();
                return;
            }

            for (int i = 0; i < nBytesRec; i++)
            {
                receivedBytes.Add(dataRcvBuf[i]);
            }

            //packetize
            while (receivedBytes.Count >= 4)
            {
                byte[] receivedBytesArray = receivedBytes.ToArray();
                int packetLength = ReadInt(receivedBytesArray, 0);
                if (receivedBytes.Count >= 4 + packetLength)
                {
                    //read packet
                    byte[] packet = new byte[packetLength];
                    for (int i = 0; i < packetLength; i++)
                    {
                        packet[i] = receivedBytesArray[4 + i];
                    }
                    receivedBytes.RemoveRange(0, 4 + packetLength);
                    ReceivedMessage.Invoke(this, new MessageEventArgs() { ClientId = this, data = packet });
                }
                else
                {
                    break;
                }
            }

            this.sock.BeginReceive(
                this.dataRcvBuf, 0,
                this.dataRcvBuf.Length,
                SocketFlags.None,
                new AsyncCallback(this.OnBytesReceived),
                this);
        }
        catch
        {
            InvokeDisconnected();
        }
    }

    void InvokeDisconnected()
    {
        if (Disconnected != null)
        {
            if (sock != null)
            {
                sock.Close();
                sock = null;
                if (connected)
                {
                    Disconnected(null, new ConnectionEventArgs() { ClientId = this });
                }
            }
        }
    }

    public void Send(byte[] data)
    {
        try
        {
            int length = data.Length;
            byte[] data2 = new byte[length + 4];
            WriteInt(data2, 0, length);
            for (int i = 0; i < length; i++)
            {
                data2[4 + i] = data[i];
            }
            sock.BeginSend(data2, 0, data2.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
        }
        catch
        {
            InvokeDisconnected();
        }
    }
    void OnSend(IAsyncResult result)
    {
        try
        {
            sock.EndSend(result);
        }
        catch
        {
            InvokeDisconnected();
        }
    }
    List<byte> receivedBytes = new List<byte>();
    public event EventHandler<MessageEventArgs> ReceivedMessage;
    public event EventHandler<ConnectionEventArgs> Disconnected;

    void WriteInt(byte[] writeBuf, int writePos, int n)
    {
        int a = (n >> 24) & 0xFF;
        int b = (n >> 16) & 0xFF;
        int c = (n >> 8) & 0xFF;
        int d = n & 0xFF;
        writeBuf[writePos] = (byte)(a);
        writeBuf[writePos + 1] = (byte)(b);
        writeBuf[writePos + 2] = (byte)(c);
        writeBuf[writePos + 3] = (byte)(d);
    }

    int ReadInt(byte[] readBuf, int readPos)
    {
        int n = readBuf[readPos] << 24;
        n |= readBuf[readPos + 1] << 16;
        n |= readBuf[readPos + 2] << 8;
        n |= readBuf[readPos + 3];
        return n;
    }

    public override string ToString()
    {
        if (address != null)
        {
            return address.ToString();
        }
        return base.ToString();
    }
}

public class ConnectionEventArgs : System.EventArgs
{
    public TcpConnection ClientId;
}

public class MessageEventArgs : System.EventArgs
{
    public TcpConnection ClientId;
    public byte[] data;
}

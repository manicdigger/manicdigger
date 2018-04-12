using ManicDigger.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ManicDigger.Server
{
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
}

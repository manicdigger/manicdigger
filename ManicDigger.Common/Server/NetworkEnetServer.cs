using ManicDigger.Common;

namespace ManicDigger.Server
{
	/// <summary>
	/// Description of NetworkEnetServer.
	/// </summary>
	public class EnetNetServer : NetServer
	{
		EnetHost host;
		EnetEvent event_;
		QueueNetIncomingMessage messages;
		int clientid;
		int Port;

		public EnetNetServer()
		{
			messages = new QueueNetIncomingMessage();
		}

		public override void Start()
		{
			host = new EnetHostNative() { host = new ENet.Host() };
			((EnetHostNative)host).host.InitializeServer(Port, 256);
		}

		public override NetIncomingMessage ReadMessage()
		{
			if (messages.Count() > 0)
			{
				return messages.Dequeue();
			}

			ENet.Event e;
			bool ret = ((EnetHostNative)host).host.Service(0, out e);
			event_ = new EnetEventNative(e);

			if (ret)
			{
				do
				{
					switch (event_.Type())
					{
						case EnetEventType.Connect:
							{
								EnetPeer peer = event_.Peer();
								peer.SetUserData(clientid++);
								EnetNetConnection senderConnectionConnect = new EnetNetConnection();
								senderConnectionConnect.peer = event_.Peer();
								NetIncomingMessage message = new NetIncomingMessage();
								message.SenderConnection = senderConnectionConnect;
								message.Type = NetworkMessageType.Connect;
								messages.Enqueue(message);
							}
							break;
						case EnetEventType.Receive:
							{
								byte[] data = event_.Packet().GetBytes();
								event_.Packet().Dispose();
								EnetNetConnection senderConnectionReceive = new EnetNetConnection();
								senderConnectionReceive.peer = event_.Peer();
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
								senderConnectionDisconnect.peer = event_.Peer();
								NetIncomingMessage message = new NetIncomingMessage();
								message.SenderConnection = senderConnectionDisconnect;
								message.Type = NetworkMessageType.Disconnect;
								messages.Enqueue(message);
							}
							break;
					}

					ret = ((EnetHostNative)host).host.CheckEvents(out e);
					event_ = new EnetEventNative(e);
				}
				while (ret);
			}
			if (messages.Count() > 0)
			{
				return messages.Dequeue();
			}
			return null;
		}

		public override void SetPort(int port)
		{
			Port = port;
		}
	}

}

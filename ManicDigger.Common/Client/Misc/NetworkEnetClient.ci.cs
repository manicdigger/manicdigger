public class EnetNetClient : NetClient
{
	internal GamePlatform platform;
	EnetHost host;
	EnetPeer peer;
	QueueNetIncomingMessage messages;
	QueueINetOutgoingMessage tosend;
	bool connected;
	bool connected2;

	public override void Start()
	{
		host = platform.EnetCreateHost();
		platform.EnetHostInitialize(host, null, 1, 0, 0, 0);
		tosend = new QueueINetOutgoingMessage();
		messages = new QueueNetIncomingMessage();
	}

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

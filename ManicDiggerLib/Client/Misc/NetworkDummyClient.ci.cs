public class DummyNetClient : NetClient
{
	internal GamePlatform platform;
	internal DummyNetwork network;

	public override NetConnection Connect(string ip, int port)
	{
		return new DummyNetConnectionCi();
	}

	public override NetIncomingMessage ReadMessage()
	{
		NetIncomingMessage msg = null;
		platform.MonitorEnter(network.ClientReceiveBufferLock);
		{
			if (network.ClientReceiveBuffer.Count() > 0)
			{
				msg = new NetIncomingMessage();
				ByteArray b = network.ClientReceiveBuffer.Dequeue();
				msg.message = b.data;
				msg.messageLength = b.length;
			}
		}
		platform.MonitorExit(network.ClientReceiveBufferLock);
		return msg;
	}

	public override void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
	{
		platform.MonitorEnter(network.ServerReceiveBufferLock);
		{
			INetOutgoingMessage msg = message;
			ByteArray b = new ByteArray();
			b.data = msg.message;
			b.length = msg.messageLength;
			network.ServerReceiveBuffer.Enqueue(b);
		}
		platform.MonitorExit(network.ServerReceiveBufferLock);
	}

	public override void Start()
	{

	}

	public void SetNetwork(DummyNetwork network_)
	{
		network = network_;
	}

	public void SetPlatform(GamePlatform gamePlatform)
	{
		platform = gamePlatform;
	}
}

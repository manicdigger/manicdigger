/// <summary>
/// Description of NetworkDummyServer.
/// </summary>
public class DummyNetServer : NetServer
{
	public DummyNetServer()
	{
		connectedClient = new DummyNetConnectionCi();
	}
	internal GamePlatform platform;
	internal DummyNetwork network;
	public override void Start()
	{

	}

	DummyNetConnectionCi connectedClient;

	bool receivedAnyMessage;

	public override NetIncomingMessage ReadMessage()
	{
		connectedClient.network = network;
		connectedClient.platform = platform;

		NetIncomingMessage msg = null;
		platform.MonitorEnter(network.ServerReceiveBufferLock);
		{
			if (network.ServerReceiveBuffer.Count() > 0)
			{
				if (!receivedAnyMessage)
				{
					receivedAnyMessage = true;
					msg = new NetIncomingMessage();
					msg.Type = NetworkMessageType.Connect;
					msg.SenderConnection = connectedClient;
				}
				else
				{
					msg = new NetIncomingMessage();
					ByteArray b = network.ServerReceiveBuffer.Dequeue();
					msg.message = b.data;
					msg.messageLength = b.length;
					msg.SenderConnection = connectedClient;
				}
			}
		}
		platform.MonitorExit(network.ServerReceiveBufferLock);
		return msg;
	}

	public void SetNetwork(DummyNetwork dummyNetwork)
	{
		network = dummyNetwork;
	}

	public void SetPlatform(GamePlatform gamePlatform)
	{
		platform = gamePlatform;
	}

	public override void SetPort(int port)
	{

	}
}

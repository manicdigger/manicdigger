public class DummyNetOutgoingMessage : INetOutgoingMessage
{

}

public class DummyNetwork
{
	internal QueueByteArray ServerReceiveBuffer;
	internal QueueByteArray ClientReceiveBuffer;
	internal MonitorObject ServerReceiveBufferLock;
	internal MonitorObject ClientReceiveBufferLock;

	public DummyNetwork()
	{
		Clear();
	}

	public void Start(MonitorObject lock1, MonitorObject lock2)
	{
		ServerReceiveBufferLock = lock1;
		ClientReceiveBufferLock = lock2;
	}

	public void Clear()
	{
		ServerReceiveBuffer = new QueueByteArray();
		ClientReceiveBuffer = new QueueByteArray();
	}
}

public class ByteArray
{
	internal byte[] data;
	internal int length;
}

public class MonitorObject
{

}

public class DummyNetConnectionCi : NetConnection
{
	internal GamePlatform platform;
	internal DummyNetwork network;

	public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
	{
		platform.MonitorEnter(network.ClientReceiveBufferLock);
		{
			INetOutgoingMessage msg2 = msg;
			ByteArray b = new ByteArray();
			b.data = msg2.message;
			b.length = msg2.messageLength;
			network.ClientReceiveBuffer.Enqueue(b);
		}
		platform.MonitorExit(network.ClientReceiveBufferLock);
	}

	public override IPEndPointCi RemoteEndPoint()
	{
		return new DummyIpEndPoint();
	}

	public override void Update()
	{

	}

	public override bool EqualsConnection(NetConnection connection)
	{
		return true;
	}
}
public class DummyIpEndPoint : IPEndPointCi
{
	public override string AddressToString()
	{
		return "127.0.0.1";
	}
}
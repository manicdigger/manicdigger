namespace ManicDigger.Server
{
	/// <summary>
	/// Description of NetworkDummyServer.
	/// </summary>
	public class DummyNetConnection : NetConnection
	{
		internal DummyNetwork network;

		public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
		{
			System.Threading.Monitor.Enter(network.ClientReceiveBufferLock);
			{
				INetOutgoingMessage msg2 = msg;
				ByteArray b = new ByteArray();
				b.data = msg2.message;
				b.length = msg2.messageLength;
				network.ClientReceiveBuffer.Enqueue(b);
			}
			System.Threading.Monitor.Exit(network.ClientReceiveBufferLock);
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
}

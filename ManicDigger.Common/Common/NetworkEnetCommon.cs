using System;

namespace ManicDigger.Common
{
	/// <summary>
	/// Represents an ENet (UDP) connection
	/// </summary>
	public class EnetNetConnection : NetConnection
	{
		internal EnetPeer peer;

		public override IPEndPointCi RemoteEndPoint()
		{
			return IPEndPointCiDefault.Create(peer.GetRemoteAddress().AddressToString());
		}

		// TODO: Use or remove method parameter
		public override void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
		{
			try
			{
				((EnetPeerNative)peer).peer.Send(0, msg.message, (ENet.PacketFlags)EnetPacketFlags.Reliable);
			}
			catch
			{

			}
		}

		public override void Update()
		{

		}

		public override bool EqualsConnection(NetConnection connection)
		{
			return peer.UserData() == ((EnetNetConnection)connection).peer.UserData();
		}
	}

	public class EnetHostNative : EnetHost
	{
		public ENet.Host host;
	}

	public class EnetEventNative : EnetEvent
	{
		public ENet.Event e;

		public EnetEventNative(ENet.Event evt)
		{
			this.e = evt;
		}

		public override EnetEventType Type()
		{
			return (EnetEventType)e.Type;
		}

		public override EnetPeer Peer()
		{
			EnetPeerNative peer = new EnetPeerNative();
			peer.peer = e.Peer;
			return peer;
		}

		public override EnetPacket Packet()
		{
			EnetPacketNative packet = new EnetPacketNative();
			packet.packet = e.Packet;
			return packet;
		}
	}

	public class EnetPacketNative : EnetPacket
	{
		internal ENet.Packet packet;
		public override int GetBytesCount()
		{
			return packet.GetBytes().Length;
		}

		public override byte[] GetBytes()
		{
			return packet.GetBytes();
		}

		public override void Dispose()
		{
			packet.Dispose();
		}
	}

	public class EnetPeerNative : EnetPeer
	{
		public ENet.Peer peer;
		public override int UserData()
		{
			return peer.UserData.ToInt32();
		}

		public override void SetUserData(int value)
		{
			peer.UserData = new IntPtr(value);
		}

		public override IPEndPointCi GetRemoteAddress()
		{
			return IPEndPointCiDefault.Create(peer.GetRemoteAddress().Address.ToString());
		}
	}
}
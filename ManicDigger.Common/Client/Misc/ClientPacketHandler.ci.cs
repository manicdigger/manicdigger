public abstract class ClientPacketHandler
{
	public ClientPacketHandler()
	{
		one = 1;
	}
	internal float one;
	public abstract void Handle(Game game, Packet_Server packet);
}

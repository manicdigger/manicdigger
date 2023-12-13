public class ModSendActiveMaterial : ClientMod
{
	internal int PreviousActiveMaterialBlock;
	public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
	{
		Packet_Item activeitem = game.d_Inventory.RightHand[game.ActiveHudIndex];
		int activeblock = 0;
		if (activeitem != null) { activeblock = activeitem.BlockId; }
		if (activeblock != PreviousActiveMaterialBlock)
		{
			game.SendPacketClient(ClientPackets.ActiveMaterialSlot(game.ActiveHudIndex));
		}
		PreviousActiveMaterialBlock = activeblock;
	}
}

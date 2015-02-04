public class ModReloadAmmo : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.reloadstartMilliseconds != 0
            && (game.one * (game.platform.TimeMillisecondsFromStart() - game.reloadstartMilliseconds) / 1000)
            > game.DeserializeFloat(game.blocktypes[game.reloadblock].ReloadDelayFloat))
        {
            {
                int loaded = game.TotalAmmo[game.reloadblock];
                loaded = MathCi.MinInt(game.blocktypes[game.reloadblock].AmmoMagazine, loaded);
                game.LoadedAmmo[game.reloadblock] = loaded;
                game.reloadstartMilliseconds = 0;
                game.reloadblock = -1;
            }
        }
    }
    public override void OnKeyDown(Game game, KeyEventArgs args)
    {
        if (!(game.guistate == GuiState.Normal && game.GuiTyping == TypingState.None))
        {
            //Do nothing when in dialog or chat
            return;
        }
        int eKey = args.GetKeyCode();
        if (eKey == game.GetKey(GlKeys.R))
        {
            Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
            if (item != null && item.ItemClass == Packet_ItemClassEnum.Block
                && game.blocktypes[item.BlockId].IsPistol
                && game.reloadstartMilliseconds == 0)
            {
                int sound = game.rnd.Next() % game.blocktypes[item.BlockId].Sounds.ReloadCount;
                game.AudioPlay(StringTools.StringAppend(game.platform, game.blocktypes[item.BlockId].Sounds.Reload[sound], ".ogg"));
                game.reloadstartMilliseconds = game.platform.TimeMillisecondsFromStart();
                game.reloadblock = item.BlockId;
                game.SendPacketClient(ClientPackets.Reload());
            }
        }
    }
}

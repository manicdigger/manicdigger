public class ModSetPlayers : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        float one = 1;
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity e = game.entities[i];
            if (e == null) { continue; }
            if (e.player == null) { continue; }
            if (e.push == null)
            {
                e.push = new Packet_ServerExplosion();
            }
            e.push.RangeFloat = game.SerializeFloat(game.PlayerPushDistance);
            e.push.XFloat = game.SerializeFloat(e.player.PositionX);
            e.push.YFloat = game.SerializeFloat(e.player.PositionZ);
            e.push.ZFloat = game.SerializeFloat(e.player.PositionY);

            if ((!e.player.PositionLoaded) ||
                (i == game.LocalPlayerId) || (e.player.Name == "")
                || (e.player.playerDrawInfo == null)
                || (e.player.playerDrawInfo.interpolation == null))
            {
                e.drawName = null;
            }
            else
            {
                if (e.drawName == null)
                {
                    e.drawName = new DrawName();
                }
                e.drawName.TextX = e.player.PositionX;
                e.drawName.TextY = e.player.PositionY + e.player.ModelHeight + one * 8 / 10;
                e.drawName.TextZ = e.player.PositionZ;
                e.drawName.Name = e.player.Name;
                if (e.player.Type == PlayerType.Monster)
                {
                    e.drawName.DrawHealth = true;
                    e.drawName.Health = one * e.player.Health / 20;
                }
            }
        }
    }
}

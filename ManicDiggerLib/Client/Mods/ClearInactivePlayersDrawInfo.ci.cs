public class ModClearInactivePlayersDrawInfo : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        float one = 1;
        int now = game.platform.TimeMillisecondsFromStart();
        for (int i = 0; i < game.entitiesCount; i++)
        {
            if (game.entities[i] == null)
            {
                continue;
            }
            if (game.entities[i].player == null)
            {
                continue;
            }
            int kKey = i;
            Player p = game.entities[i].player;
            if ((one * (now - p.LastUpdateMilliseconds) / 1000) > 2)
            {
                p.playerDrawInfo = null;
                p.PositionLoaded = false;
            }
        }
    }
}

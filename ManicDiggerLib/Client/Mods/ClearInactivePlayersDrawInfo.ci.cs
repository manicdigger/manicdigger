public class ModClearInactivePlayersDrawInfo : ClientMod
{
    const int maxplayers = 64;
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        float one = 1;
        int now = game.platform.TimeMillisecondsFromStart();
        for (int i = 0; i < maxplayers; i++)
        {
            if (game.entities[i] == null)
            {
                continue;
            }
            if (game.entities[i].playerDrawInfo == null)
            {
                continue;
            }
            if (game.entities[i].networkPosition == null)
            {
                continue;
            }
            int kKey = i;
            Entity p = game.entities[i];
            if ((one * (now - p.networkPosition.LastUpdateMilliseconds) / 1000) > 2)
            {
                p.playerDrawInfo = null;
                p.networkPosition.PositionLoaded = false;
            }
        }
    }
}

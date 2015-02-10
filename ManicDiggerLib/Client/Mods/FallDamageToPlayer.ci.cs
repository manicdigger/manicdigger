public class ModFallDamageToPlayer : ClientMod
{
    public ModFallDamageToPlayer()
    {
        one = 1;
        lastPlayerY = -1000;
    }
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        if (game.controls.freemove)
        {
            return;
        }
        if (game.FollowId() == null)
        {
            UpdateFallDamageToPlayer(game, args.GetDt());
        }
    }
    float one;
    int lastfalldamagetimeMilliseconds;
    float lastPlayerY;
    internal void UpdateFallDamageToPlayer(Game game, float dt)
    {
        float movedz;
        if (lastPlayerY == -1000)
        {
            movedz = 0;
        }
        else
        {
            movedz = (game.player.position.y - lastPlayerY) / dt;
        }
        lastPlayerY = game.player.position.y;

        //fallspeed 4 is 10 blocks high
        //fallspeed 5.5 is 20 blocks high
        float fallspeed = movedz / (-game.basemovespeed);

        int posX = game.GetPlayerEyesBlockX();
        int posY = game.GetPlayerEyesBlockY();
        int posZ = game.GetPlayerEyesBlockZ();
        if ((game.blockheight(posX, posY, posZ) < posZ - 8)
            || fallspeed > 3)
        {
            game.AudioPlayLoop("fallloop.wav", fallspeed > 2, true);
        }
        else
        {
            game.AudioPlayLoop("fallloop.wav", false, true);
        }

        //fall damage

        if (game.IsValidPos(posX, posY, posZ - 3))
        {
            int blockBelow = game.GetBlock(posX, posY, posZ - 3);
            if ((blockBelow != 0) && (!game.IsWater(blockBelow)))
            {
                float severity = 0;
                if (fallspeed < 4) { return; }
                else if (fallspeed < (one * 45 / 10)) { severity = (one * 3 / 10); }
                else if (fallspeed < 5) { severity = (one * 5 / 10); }
                else if (fallspeed < (one * 55 / 10)) { severity = (one * 6 / 10); }
                else if (fallspeed < 6) { severity = (one * 8 / 10); }
                else { severity = 1; }
                if ((one * (game.platform.TimeMillisecondsFromStart() - lastfalldamagetimeMilliseconds) / 1000) < 1)
                {
                    return;
                }
                lastfalldamagetimeMilliseconds = game.platform.TimeMillisecondsFromStart();
                game.ApplyDamageToPlayer(game.platform.FloatToInt(severity * game.PlayerStats.MaxHealth), Packet_DeathReasonEnum.FallDamage, 0);	//Maybe give ID of last player touched?
            }
        }
    }
}

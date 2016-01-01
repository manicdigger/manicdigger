public class ModFallDamageToPlayer : ClientMod
{
    public ModFallDamageToPlayer()
    {
        one = 1;
        fallSoundPlaying = false;
    }
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        if (game.controls.GetFreemove() != FreemoveLevelEnum.None)
        {
            if (fallSoundPlaying)
            {
                SetFallSoundActive(game, false);
            }
            return;
        }
        if (game.FollowId() == null)
        {
            UpdateFallDamageToPlayer(game, args.GetDt());
        }
    }
    float one;
    bool fallSoundPlaying;
    int lastfalldamagetimeMilliseconds;
    internal void UpdateFallDamageToPlayer(Game game, float dt)
    {
        float movedz = game.movedz;

        //fallspeed 4 is 10 blocks high
        //fallspeed 5.5 is 20 blocks high
        float fallspeed = movedz / (-game.basemovespeed);

        int posX = game.GetPlayerEyesBlockX();
        int posY = game.GetPlayerEyesBlockY();
        int posZ = game.GetPlayerEyesBlockZ();
        if ((game.blockheight(posX, posY, posZ) < posZ - 8)
            || fallspeed > 3)
        {
            SetFallSoundActive(game, fallspeed > 2);
        }
        else
        {
            SetFallSoundActive(game, false);
        }

        //fall damage

        if (game.map.IsValidPos(posX, posY, posZ - 3))
        {
            int blockBelow = game.map.GetBlock(posX, posY, posZ - 3);
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
    internal void SetFallSoundActive(Game game, bool active)
    {
        game.AudioPlayLoop("fallloop.wav", active, true);
        fallSoundPlaying = active;
    }
}

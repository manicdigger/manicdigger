public class ModTrampoline : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        int blockunderplayer = game.BlockUnderPlayer();

        float one = 1;
        if (blockunderplayer != -1 && blockunderplayer == game.d_Data.BlockIdTrampoline()
            && (!game.player.isplayeronground) && !game.shiftkeydown)
        {
            game.wantsjump = true;
            game.jumpstartacceleration = (20 + one * 666 / 1000) * game.d_Physics.gravity;
        }
    }
}

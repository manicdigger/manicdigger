//slippery walk on ice and when swimming
public class ModSlipperyWalk : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }

        float one = 1;
        int blockunderplayer = game.BlockUnderPlayer();

        if ((blockunderplayer != -1 && game.d_Data.IsSlipperyWalk()[blockunderplayer]) || game.Swimming())
        {
            game.acceleration = new Acceleration();
            {
                game.acceleration.acceleration1 = one * 99 / 100;
                game.acceleration.acceleration2 = one * 2 / 10;
                game.acceleration.acceleration3 = 70;
            }
        }
    }
}

public class ModNoAirControl : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }

        float one = 1;
        if (!game.player.physicsState.isplayeronground)
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

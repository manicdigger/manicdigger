public class ModSendPosition : ClientMod
{
    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        if (game.spawned && ((game.platform.TimeMillisecondsFromStart() - game.lastpositionsentMilliseconds) > 100))
        {
            game.lastpositionsentMilliseconds = game.platform.TimeMillisecondsFromStart();

            game.SendPacketClient(ClientPackets.PositionAndOrientation(game, game.LocalPlayerId,
                game.player.playerposition.X, game.player.playerposition.Y, game.player.playerposition.Z,
                game.player.playerorientation.X, game.player.playerorientation.Y, game.player.playerorientation.Z,
                game.localstance));
        }
    }
}

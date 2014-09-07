public class ModSendPosition : ClientMod
{
    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        if (game.spawned && ((game.platform.TimeMillisecondsFromStart() - game.lastpositionsentMilliseconds) > 100))
        {
            game.lastpositionsentMilliseconds = game.platform.TimeMillisecondsFromStart();

            game.SendPacketClient(ClientPackets.PositionAndOrientation(game, game.LocalPlayerId,
                game.player.position.x, game.player.position.y, game.player.position.z,
                game.player.position.rotx, game.player.position.roty, game.player.position.rotz,
                game.localstance));
        }
    }
}

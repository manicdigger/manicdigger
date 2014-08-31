public class ModInterpolatePositions : ClientMod
{
    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        InterpolatePositions(game, args.GetDt());
    }
    internal void InterpolatePositions(Game game, float dt)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity e = game.entities[i];
            if (e == null) { continue; }
            if (e.player == null) { continue; }
            if (i == game.LocalPlayerId) { continue; }
            if (!e.player.PositionLoaded) { continue; }

            if (e.player.playerDrawInfo == null)
            {
                e.player.playerDrawInfo = new PlayerDrawInfo();
                NetworkInterpolation n = new NetworkInterpolation();
                PlayerInterpolate playerInterpolate = new PlayerInterpolate();
                playerInterpolate.platform = game.platform;
                n.req = playerInterpolate;
                n.DELAYMILLISECONDS = 500;
                n.EXTRAPOLATE = false;
                n.EXTRAPOLATION_TIMEMILLISECONDS = 300;
                e.player.playerDrawInfo.interpolation = n;
            }
            e.player.playerDrawInfo.interpolation.DELAYMILLISECONDS = Game.MaxInt(100, game.ServerInfo.ServerPing.RoundtripTimeTotalMilliseconds());
            Player p = e.player;

            PlayerDrawInfo info = p.playerDrawInfo;
            float networkposX = p.NetworkX;
            float networkposY = p.NetworkY;
            float networkposZ = p.NetworkZ;
            if ((!game.Vec3Equal(networkposX, networkposY, networkposZ,
                            info.lastnetworkposX, info.lastnetworkposY, info.lastnetworkposZ))
                || p.NetworkHeading != info.lastnetworkheading
                || p.NetworkPitch != info.lastnetworkpitch)
            {
                PlayerInterpolationState state = new PlayerInterpolationState();
                state.positionX = networkposX;
                state.positionY = networkposY;
                state.positionZ = networkposZ;
                state.heading = p.NetworkHeading;
                state.pitch = p.NetworkPitch;
                info.interpolation.AddNetworkPacket(state, game.totaltimeMilliseconds);
            }
            PlayerInterpolationState curstate = game.platform.CastToPlayerInterpolationState(info.interpolation.InterpolatedState(game.totaltimeMilliseconds));
            if (curstate == null)
            {
                curstate = new PlayerInterpolationState();
            }
            //do not interpolate player position if player is controlled by game world
            if (game.EnablePlayerUpdatePositionContainsKey(i) && !game.EnablePlayerUpdatePosition(i))
            {
                curstate.positionX = p.NetworkX;
                curstate.positionY = p.NetworkY;
                curstate.positionZ = p.NetworkZ;
            }
            float curposX = curstate.positionX;
            float curposY = curstate.positionY;
            float curposZ = curstate.positionZ;
            info.velocityX = curposX - info.lastcurposX;
            info.velocityY = curposY - info.lastcurposY;
            info.velocityZ = curposZ - info.lastcurposZ;
            p.moves = (!game.Vec3Equal(curposX, curposY, curposZ, info.lastcurposX, info.lastcurposY, info.lastcurposZ));
            info.lastcurposX = curposX;
            info.lastcurposY = curposY;
            info.lastcurposZ = curposZ;
            info.lastnetworkposX = networkposX;
            info.lastnetworkposY = networkposY;
            info.lastnetworkposZ = networkposZ;
            info.lastnetworkheading = p.NetworkHeading;
            info.lastnetworkpitch = p.NetworkPitch;

            p.PositionX = curposX;
            p.PositionY = curposY;
            p.PositionZ = curposZ;
            p.Heading = curstate.heading;
            p.Pitch = curstate.pitch;
        }
    }
}

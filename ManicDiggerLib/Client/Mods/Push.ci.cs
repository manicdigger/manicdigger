public class ModPush : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        game.pushX = 0;
        game.pushY = 0;
        game.pushZ = 0;

        float LocalPlayerPositionX = game.player.position.x;
        float LocalPlayerPositionY = game.player.position.y;
        float LocalPlayerPositionZ = game.player.position.z;
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity entity = game.entities[i];
            if (entity == null) { continue; }
            if (entity.push == null) { continue; }
            //Prevent players that aren't displayed from pushing
            if (entity.networkPosition != null && !entity.networkPosition.PositionLoaded) { continue; }
            float kposX = game.DeserializeFloat(entity.push.XFloat);
            float kposY = game.DeserializeFloat(entity.push.ZFloat);
            float kposZ = game.DeserializeFloat(entity.push.YFloat);
            if (entity.push.IsRelativeToPlayerPosition != 0)
            {
                kposX += LocalPlayerPositionX;
                kposY += LocalPlayerPositionY;
                kposZ += LocalPlayerPositionZ;
            }
            float dist = game.Dist(kposX, kposY, kposZ, LocalPlayerPositionX, LocalPlayerPositionY, LocalPlayerPositionZ);
            if (dist < game.DeserializeFloat(entity.push.RangeFloat))
            {
                float diffX = LocalPlayerPositionX - kposX;
                float diffY = LocalPlayerPositionY - kposY;
                float diffZ = LocalPlayerPositionZ - kposZ;
                game.pushX += diffX;
                game.pushY += diffY;
                game.pushZ += diffZ;
            }
        }
    }
}

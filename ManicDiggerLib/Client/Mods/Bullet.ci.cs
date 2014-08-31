public class ModBullet : ClientMod
{
    public override void OnNewFrameDraw3d(Game game, float dt)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity entity = game.entities[i];
            if (entity == null) { continue; }
            if (entity.bullet == null) { continue; }

            Bullet_ b = entity.bullet;
            if (b.progress < 1)
            {
                b.progress = 1;
            }

            float dirX = b.toX - b.fromX;
            float dirY = b.toY - b.fromY;
            float dirZ = b.toZ - b.fromZ;
            float length = game.Dist(0, 0, 0, dirX, dirY, dirZ);
            dirX /= length;
            dirY /= length;
            dirZ /= length;

            float posX = b.fromX;
            float posY = b.fromY;
            float posZ = b.fromZ;
            posX += dirX * (b.progress + b.speed * dt);
            posY += dirY * (b.progress + b.speed * dt);
            posZ += dirZ * (b.progress + b.speed * dt);
            b.progress += b.speed * dt;

            entity.sprite.positionX = posX;
            entity.sprite.positionY = posY;
            entity.sprite.positionZ = posZ;

            if (b.progress > length)
            {
                game.entities[i] = null;
            }
        }
    }
}

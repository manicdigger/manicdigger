public class ModGrenade : ClientMod
{
    public ModGrenade()
    {
        one = 1;
        projectilegravity = 20;
        bouncespeedmultiply = one * 5 / 10;
        walldistance = one * 3 / 10;
    }
    float one;

    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity entity = game.entities[i];
            if (entity == null) { continue; }
            if (entity.grenade == null) { continue; }
            UpdateGrenade(game, i, args.GetDt());
        }
    }

    internal void UpdateGrenade(Game game, int grenadeEntityId, float dt)
    {
        float LocalPlayerPositionX = game.player.position.x;
        float LocalPlayerPositionY = game.player.position.y;
        float LocalPlayerPositionZ = game.player.position.z;

        Entity grenadeEntity = game.entities[grenadeEntityId];
        Sprite grenadeSprite = grenadeEntity.sprite;
        Grenade_ grenade = grenadeEntity.grenade;

        float oldposX = grenadeEntity.sprite.positionX;
        float oldposY = grenadeSprite.positionY;
        float oldposZ = grenadeSprite.positionZ;
        float newposX = grenadeSprite.positionX + grenade.velocityX * dt;
        float newposY = grenadeSprite.positionY + grenade.velocityY * dt;
        float newposZ = grenadeSprite.positionZ + grenade.velocityZ * dt;
        grenade.velocityY += -projectilegravity * dt;

        Vector3Ref velocity = Vector3Ref.Create(grenade.velocityX, grenade.velocityY, grenade.velocityZ);
        Vector3Ref bouncePosition = GrenadeBounce(game, Vector3Ref.Create(oldposX, oldposY, oldposZ), Vector3Ref.Create(newposX, newposY, newposZ), velocity, dt);
        grenade.velocityX = velocity.X;
        grenade.velocityY = velocity.Y;
        grenade.velocityZ = velocity.Z;
        grenadeSprite.positionX = bouncePosition.X;
        grenadeSprite.positionY = bouncePosition.Y;
        grenadeSprite.positionZ = bouncePosition.Z;
    }
    float projectilegravity;
    float bouncespeedmultiply;
    float walldistance;
    internal Vector3Ref GrenadeBounce(Game game, Vector3Ref oldposition, Vector3Ref newposition, Vector3Ref velocity, float dt)
    {
        bool ismoving = velocity.Length() > 100 * dt;
        float modelheight = walldistance;
        oldposition.Y += walldistance;
        newposition.Y += walldistance;

        //Math.Floor() is needed because casting negative values to integer is not floor.
        Vector3IntRef oldpositioni = Vector3IntRef.Create(game.MathFloor(oldposition.X),
           game.MathFloor(oldposition.Z),
          game.MathFloor(oldposition.Y));
        float playerpositionX = newposition.X;
        float playerpositionY = newposition.Y;
        float playerpositionZ = newposition.Z;
        //left
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z + walldistance;
            bool newempty = game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY))
            && game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY) + 1);
            if (newposition.Z - oldposition.Z > 0)
            {
                if (!newempty)
                {
                    velocity.Z = -velocity.Z;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        game.AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Z = oldposition.Z - newposition.Z;
                }
            }
        }
        //front
        {
            float qnewpositionX = newposition.X + walldistance;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z;
            bool newempty = game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY))
            && game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY) + 1);
            if (newposition.X - oldposition.X > 0)
            {
                if (!newempty)
                {
                    velocity.X = -velocity.X;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        game.AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.X = oldposition.X - newposition.X;
                }
            }
        }
        //top
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y - walldistance;
            float qnewpositionZ = newposition.Z;
            int x = game.MathFloor(qnewpositionX);
            int y = game.MathFloor(qnewpositionZ);
            int z = game.MathFloor(qnewpositionY);
            float a_ = walldistance;
            bool newfull = (!game.IsTileEmptyForPhysics(x, y, z))
                || (qnewpositionX - game.MathFloor(qnewpositionX) <= a_ && (!game.IsTileEmptyForPhysics(x - 1, y, z)) && (game.IsTileEmptyForPhysics(x - 1, y, z + 1)))
                || (qnewpositionX - game.MathFloor(qnewpositionX) >= (1 - a_) && (!game.IsTileEmptyForPhysics(x + 1, y, z)) && (game.IsTileEmptyForPhysics(x + 1, y, z + 1)))
                || (qnewpositionZ - game.MathFloor(qnewpositionZ) <= a_ && (!game.IsTileEmptyForPhysics(x, y - 1, z)) && (game.IsTileEmptyForPhysics(x, y - 1, z + 1)))
                || (qnewpositionZ - game.MathFloor(qnewpositionZ) >= (1 - a_) && (!game.IsTileEmptyForPhysics(x, y + 1, z)) && (game.IsTileEmptyForPhysics(x, y + 1, z + 1)));
            if (newposition.Y - oldposition.Y < 0)
            {
                if (newfull)
                {
                    velocity.Y = -velocity.Y;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        game.AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Y = oldposition.Y - newposition.Y;
                }
            }
        }
        //right
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z - walldistance;
            bool newempty = game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY))
            && game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY) + 1);
            if (newposition.Z - oldposition.Z < 0)
            {
                if (!newempty)
                {
                    velocity.Z = -velocity.Z;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        game.AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Z = oldposition.Z - newposition.Z;
                }
            }
        }
        //back
        {
            float qnewpositionX = newposition.X - walldistance;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z;
            bool newempty = game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY))
            && game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY) + 1);
            if (newposition.X - oldposition.X < 0)
            {
                if (!newempty)
                {
                    velocity.X = -velocity.X;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        game.AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.X = oldposition.X - newposition.X;
                }
            }
        }
        //bottom
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y + modelheight;
            float qnewpositionZ = newposition.Z;
            bool newempty = game.IsTileEmptyForPhysics(game.MathFloor(qnewpositionX), game.MathFloor(qnewpositionZ), game.MathFloor(qnewpositionY));
            if (newposition.Y - oldposition.Y > 0)
            {
                if (!newempty)
                {
                    velocity.Y = -velocity.Y;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        game.AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Y = oldposition.Y - newposition.Y;
                }
            }
        }
        //ok:
        playerpositionY -= walldistance;
        return Vector3Ref.Create(playerpositionX, playerpositionY, playerpositionZ);
    }
}

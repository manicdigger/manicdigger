public class ModExpire : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity entity = game.entities[i];
            if (entity == null) { continue; }
            if (entity.expires == null) { continue; }
            entity.expires.timeLeft -= args.GetDt();
            if (entity.expires.timeLeft <= 0)
            {
                if (entity.grenade != null)
                {
                    GrenadeExplosion(game, i);
                }
                game.entities[i] = null;
            }
        }
    }

    void GrenadeExplosion(Game game, int grenadeEntityId)
    {
        float LocalPlayerPositionX = game.player.position.x;
        float LocalPlayerPositionY = game.player.position.y;
        float LocalPlayerPositionZ = game.player.position.z;

        Entity grenadeEntity = game.entities[grenadeEntityId];
        Sprite grenadeSprite = grenadeEntity.sprite;
        Grenade_ grenade = grenadeEntity.grenade;

        game.AudioPlayAt("grenadeexplosion.ogg", grenadeSprite.positionX, grenadeSprite.positionY, grenadeSprite.positionZ);

        {
            Entity entity = new Entity();

            Sprite spritenew = new Sprite();
            spritenew.image = "ani5.png";
            spritenew.positionX = grenadeSprite.positionX;
            spritenew.positionY = grenadeSprite.positionY + 1;
            spritenew.positionZ = grenadeSprite.positionZ;
            spritenew.size = 200;
            spritenew.animationcount = 4;

            entity.sprite = spritenew;
            entity.expires = Expires.Create(1);
            game.EntityAddLocal(entity);
        }

        {
            Packet_ServerExplosion explosion = new Packet_ServerExplosion();
            explosion.XFloat = game.SerializeFloat(grenadeSprite.positionX);
            explosion.YFloat = game.SerializeFloat(grenadeSprite.positionZ);
            explosion.ZFloat = game.SerializeFloat(grenadeSprite.positionY);
            explosion.RangeFloat = game.blocktypes[grenade.block].ExplosionRangeFloat;
            explosion.IsRelativeToPlayerPosition = 0;
            explosion.TimeFloat = game.blocktypes[grenade.block].ExplosionTimeFloat;

            Entity entity = new Entity();
            entity.push = explosion;
            entity.expires = new Expires();
            entity.expires.timeLeft = game.DeserializeFloat(game.blocktypes[grenade.block].ExplosionTimeFloat);
            game.EntityAddLocal(entity);
        }

        float dist = game.Dist(LocalPlayerPositionX, LocalPlayerPositionY, LocalPlayerPositionZ, grenadeSprite.positionX, grenadeSprite.positionY, grenadeSprite.positionZ);
        float dmg = (1 - dist / game.DeserializeFloat(game.blocktypes[grenade.block].ExplosionRangeFloat)) * game.DeserializeFloat(game.blocktypes[grenade.block].DamageBodyFloat);
        if (dmg > 0)
        {
            game.ApplyDamageToPlayer(game.platform.FloatToInt(dmg), Packet_DeathReasonEnum.Explosion, grenade.sourcePlayer);
        }
    }
}

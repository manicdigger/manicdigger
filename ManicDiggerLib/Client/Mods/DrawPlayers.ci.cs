public class ModDrawPlayers : ClientMod
{
    public ModDrawPlayers()
    {
        one = 1;
    }
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        DrawPlayers(game, deltaTime);
    }

    float one;

    internal void DrawPlayers(Game game, float dt)
    {
        game.totaltimeMilliseconds = game.platform.TimeMillisecondsFromStart();
        for (int i = 0; i < game.entitiesCount; i++)
        {
            if (game.entities[i] == null)
            {
                continue;
            }
            if (game.entities[i].drawModel == null)
            {
                continue;
            }
            Entity p_ = game.entities[i];
            if (i == game.LocalPlayerId && (!game.ENABLE_TPP_VIEW))
            {
                continue;
            }
            if ((p_.networkPosition != null) && (!p_.networkPosition.PositionLoaded))
            {
                continue;
            }
            if (!game.d_FrustumCulling.SphereInFrustum(p_.position.x, p_.position.y, p_.position.z, 3))
            {
                continue;
            }
            int cx = game.platform.FloatToInt(p_.position.x) / Game.chunksize;
            int cy = game.platform.FloatToInt(p_.position.z) / Game.chunksize;
            int cz = game.platform.FloatToInt(p_.position.y) / Game.chunksize;
            if (game.IsValidChunkPos(cx, cy, cz, Game.chunksize))
            {
                if (!game.terrainRenderer.IsChunkRendered(cx, cy, cz))
                {
                    continue;
                }
            }
            float shadow = (one * game.MaybeGetLight(game.platform.FloatToInt(p_.position.x), game.platform.FloatToInt(p_.position.z), game.platform.FloatToInt(p_.position.y))) / Game.maxlight;
            if (p_.playerDrawInfo == null)
            {
                p_.playerDrawInfo = new PlayerDrawInfo();
            }
            p_.playerDrawInfo.anim.light = shadow;
            float FeetPosX = p_.position.x;
            float FeetPosY = p_.position.y;
            float FeetPosZ = p_.position.z;
            AnimationHint animHint = game.entities[i].playerDrawInfo.AnimationHint_;

            float playerspeed_;
            if (i == game.LocalPlayerId)
            {
                if (game.player.playerDrawInfo == null)
                {
                    game.player.playerDrawInfo = new PlayerDrawInfo();
                }
                Vector3Ref playerspeed = Vector3Ref.Create(game.playervelocity.X / 60, game.playervelocity.Y / 60, game.playervelocity.Z / 60);
                float playerspeedf = playerspeed.Length() * (one * 15 / 10);
                game.player.playerDrawInfo.moves = playerspeedf != 0;
                playerspeed_ = playerspeedf;
            }
            else
            {
                playerspeed_ = (game.Length(p_.playerDrawInfo.velocityX, p_.playerDrawInfo.velocityY, p_.playerDrawInfo.velocityZ) / dt) * (one * 4 / 100);
            }

            {
                ICharacterRenderer r = game.GetCharacterRenderer(p_.drawModel.Model_);
                r.SetAnimation("walk");
                r.DrawCharacter(p_.playerDrawInfo.anim, FeetPosX, FeetPosY, FeetPosZ,
                Game.IntToByte(-game.HeadingByte(p_.position.rotx, p_.position.roty, p_.position.rotz) - 256 / 4),
                game.PitchByte(p_.position.rotx, p_.position.roty, p_.position.rotz),
                    p_.playerDrawInfo.moves,
                    dt, game.entities[i].drawModel.CurrentTexture, animHint, playerspeed_);
            }
        }
    }
}

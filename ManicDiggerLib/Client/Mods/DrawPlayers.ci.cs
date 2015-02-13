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
            if (p_.drawModel.CurrentTexture == -1)
            {
                continue;
            }
            int cx = game.platform.FloatToInt(p_.position.x) / Game.chunksize;
            int cy = game.platform.FloatToInt(p_.position.z) / Game.chunksize;
            int cz = game.platform.FloatToInt(p_.position.y) / Game.chunksize;
            if (game.map.IsValidChunkPos(cx, cy, cz))
            {
                if (!game.map.IsChunkRendered(cx, cy, cz))
                {
                    continue;
                }
            }
            float shadow = (one * game.GetLight(game.platform.FloatToInt(p_.position.x), game.platform.FloatToInt(p_.position.z), game.platform.FloatToInt(p_.position.y))) / Game.maxlight;
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
                if (p_.drawModel.renderer == null)
                {
                    p_.drawModel.renderer = new AnimatedModelRenderer();
                    byte[] data = game.GetFile(p_.drawModel.Model_);
                    int dataLength = game.GetFileLength(p_.drawModel.Model_);
                    if (data != null)
                    {
                        string dataString = game.platform.StringFromUtf8ByteArray(data, dataLength);
                        AnimatedModel model = AnimatedModelSerializer.Deserialize(game.platform, dataString);
                        p_.drawModel.renderer.Start(game, model);
                    }
                }
                game.GLPushMatrix();
                game.GLTranslate(FeetPosX, FeetPosY, FeetPosZ);
                //game.GLRotate(PlayerInterpolate.RadToDeg(p_.position.rotx), 1, 0, 0);
                game.GLRotate(PlayerInterpolate.RadToDeg(-p_.position.roty + Game.GetPi()), 0, 1, 0);
                //game.GLRotate(PlayerInterpolate.RadToDeg(p_.position.rotz), 0, 0, 1);
                game.platform.BindTexture2d(game.entities[i].drawModel.CurrentTexture);
                p_.drawModel.renderer.Render(dt, PlayerInterpolate.RadToDeg(p_.position.rotx + Game.GetPi()), true, p_.playerDrawInfo.moves, shadow);
                game.GLPopMatrix();
            }
        }
    }
}

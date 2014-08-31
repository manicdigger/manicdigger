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
            if (game.entities[i].player == null)
            {
                continue;
            }
            Player p_ = game.entities[i].player;
            if (i == game.LocalPlayerId)
            {
                continue;
            }
            if (!p_.PositionLoaded)
            {
                continue;
            }
            if (!game.d_FrustumCulling.SphereInFrustum(p_.PositionX, p_.PositionY, p_.PositionZ, 3))
            {
                continue;
            }
            int cx = game.platform.FloatToInt(p_.PositionX) / Game.chunksize;
            int cy = game.platform.FloatToInt(p_.PositionZ) / Game.chunksize;
            int cz = game.platform.FloatToInt(p_.PositionY) / Game.chunksize;
            if (game.IsValidChunkPos(cx, cy, cz, Game.chunksize))
            {
                if (!game.terrainRenderer.IsChunkRendered(cx, cy, cz))
                {
                    continue;
                }
            }
            float shadow = (one * game.MaybeGetLight(game.platform.FloatToInt(p_.PositionX), game.platform.FloatToInt(p_.PositionZ), game.platform.FloatToInt(p_.PositionY))) / Game.maxlight;
            if (p_.playerDrawInfo == null)
            {
                continue;
            }
            p_.playerDrawInfo.anim.light = shadow;
            float FeetPosX = p_.PositionX;
            float FeetPosY = p_.PositionY;
            float FeetPosZ = p_.PositionZ;
            AnimationHint animHint = game.entities[i].player.AnimationHint_;
            float playerspeed = (game.Length(p_.playerDrawInfo.velocityX, p_.playerDrawInfo.velocityY, p_.playerDrawInfo.velocityZ) / dt) * (one * 4 / 100);
            if (p_.Type == PlayerType.Player)
            {
                ICharacterRenderer r = game.GetCharacterRenderer(p_.Model);
                r.SetAnimation("walk");
                r.DrawCharacter(p_.playerDrawInfo.anim, FeetPosX, FeetPosY, FeetPosZ, Game.IntToByte(-p_.Heading - 256 / 4), p_.Pitch, p_.moves, dt, game.entities[i].player.CurrentTexture, animHint, playerspeed);
                //DrawCharacter(info.anim, FeetPos,
                //    curstate.heading, curstate.pitch, moves, dt, GetPlayerTexture(k.Key), animHint);
            }
            else
            {
                //fix crash on monster spawn
                ICharacterRenderer r = game.GetCharacterRenderer(game.d_DataMonsters.MonsterCode[p_.MonsterType]);
                //var r = MonsterRenderers[d_DataMonsters.MonsterCode[k.Value.MonsterType]];
                r.SetAnimation("walk");
                //curpos += new Vector3(0, -CharacterPhysics.walldistance, 0); //todos
                r.DrawCharacter(p_.playerDrawInfo.anim, p_.PositionX, p_.PositionY, p_.PositionZ,
                    Game.IntToByte(-p_.Heading - 256 / 4), p_.Pitch,
                    p_.moves, dt, game.entities[i].player.CurrentTexture, animHint, playerspeed);
            }
        }
        if (game.ENABLE_TPP_VIEW)
        {
            float LocalPlayerPositionX = game.player.playerposition.X;
            float LocalPlayerPositionY = game.player.playerposition.Y;
            float LocalPlayerPositionZ = game.player.playerposition.Z;
            float LocalPlayerOrientationX = game.player.playerorientation.X;
            float LocalPlayerOrientationY = game.player.playerorientation.Y;
            float LocalPlayerOrientationZ = game.player.playerorientation.Z;
            float velocityX = lastlocalplayerposX - LocalPlayerPositionX;
            float velocityY = lastlocalplayerposY - LocalPlayerPositionY;
            float velocityZ = lastlocalplayerposZ - LocalPlayerPositionZ;
            bool moves = (lastlocalplayerposX != LocalPlayerPositionX
                || lastlocalplayerposY != LocalPlayerPositionY
                || lastlocalplayerposZ != LocalPlayerPositionZ); //bool moves = velocity.Length > 0.08;
            float shadow = (one * game.MaybeGetLight(
                game.platform.FloatToInt(LocalPlayerPositionX),
                game.platform.FloatToInt(LocalPlayerPositionZ),
                game.platform.FloatToInt(LocalPlayerPositionY)))
                / Game.maxlight;
            game.localplayeranim.light = shadow;
            ICharacterRenderer r = game.GetCharacterRenderer(game.entities[game.LocalPlayerId].player.Model);
            r.SetAnimation("walk");
            Vector3Ref playerspeed = Vector3Ref.Create(game.playervelocity.X / 60, game.playervelocity.Y / 60, game.playervelocity.Z / 60);
            float playerspeedf = playerspeed.Length() * (one * 15 / 10);
            r.DrawCharacter
                (game.localplayeranim, LocalPlayerPositionX, LocalPlayerPositionY,
                LocalPlayerPositionZ,
                Game.IntToByte(-game.HeadingByte(LocalPlayerOrientationX, LocalPlayerOrientationY, LocalPlayerOrientationZ) - 256 / 4),
                game.PitchByte(LocalPlayerOrientationX, LocalPlayerOrientationY, LocalPlayerOrientationZ),
                moves, dt, game.entities[game.LocalPlayerId].player.CurrentTexture, game.localplayeranimationhint, playerspeedf);
            lastlocalplayerposX = LocalPlayerPositionX;
            lastlocalplayerposY = LocalPlayerPositionY;
            lastlocalplayerposZ = LocalPlayerPositionZ;
        }
    }
    internal float lastlocalplayerposX;
    internal float lastlocalplayerposY;
    internal float lastlocalplayerposZ;
}

public class ModDrawPlayerNames : ClientMod
{
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity e = game.entities[i];
            if (e == null)
            {
                continue;
            }
            if (e.drawName == null)
            {
                continue;
            }
            if (i == game.LocalPlayerId)
            {
                continue;
            }
            if (e.networkPosition != null && (!e.networkPosition.PositionLoaded))
            {
                continue;
            }
            int kKey = i;
            DrawName p = game.entities[i].drawName;
            if (p.OnlyWhenSelected)
            {
                continue;
            }
            float posX = p.TextX + e.position.x;
            float posY = p.TextY + e.position.y + e.drawModel.ModelHeight + game.one * 7 / 10;
            float posZ = p.TextZ + e.position.z;
            //todo if picking
            if ((game.Dist(game.player.position.x, game.player.position.y, game.player.position.z, posX, posY, posZ) < 20)
                || game.keyboardState[Game.KeyAltLeft] || game.keyboardState[Game.KeyAltRight])
            {
                string name = p.Name;
                {

                    float shadow = (game.one * game.GetLight(game.platform.FloatToInt(posX), game.platform.FloatToInt(posZ), game.platform.FloatToInt(posY))) / Game.maxlight;
                    //do not interpolate player position if player is controlled by game world
                    //if (EnablePlayerUpdatePositionContainsKey(kKey) && !EnablePlayerUpdatePosition(kKey))
                    //{
                    //    posX = p.NetworkX;
                    //    posY = p.NetworkY;
                    //    posZ = p.NetworkZ;
                    //}
                    game.GLPushMatrix();
                    game.GLTranslate(posX, posY, posZ);
                    //if (p.Type == PlayerType.Monster)
                    //{
                    //    GLTranslate(0, 1, 0);
                    //}
                    ModDrawSprites.Billboard(game);
                    float scale = game.one * 2 / 100;
                    game.GLScale(scale, scale, scale);

                    //Color c = Color.FromArgb((int)(shadow * 255), (int)(shadow * 255), (int)(shadow * 255));
                    //Todo: Can't change text color because text has outline anyway.
                    if (p.DrawHealth)
                    {
                        game.Draw2dTexture(game.WhiteTexture(), -26, -11, 52, 12, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
                        game.Draw2dTexture(game.WhiteTexture(), -25, -10, 50 * (game.one * p.Health), 10, null, 0, Game.ColorFromArgb(255, 255, 0, 0), false);
                    }
                    FontCi font = new FontCi();
                    font.family = "Arial";
                    font.size = 14;
                    game.Draw2dText(name, font, -game.TextSizeWidth(name, 14) / 2, 0, IntRef.Create(Game.ColorFromArgb(255, 255, 255, 255)), true);
                    //                        GL.Translate(0, 1, 0);
                    game.GLPopMatrix();
                }
            }
        }
    }
}

public class ModDrawPlayerNames : ClientMod
{
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            if (game.entities[i] == null)
            {
                continue;
            }
            if (game.entities[i].drawName == null)
            {
                continue;
            }
            int kKey = i;
            DrawName p = game.entities[i].drawName;
            //todo if picking
            if ((game.Dist(game.player.playerposition.X, game.player.playerposition.Y, game.player.playerposition.Z, p.TextX, p.TextY, p.TextZ) < 20)
                || game.keyboardState[Game.KeyAltLeft] || game.keyboardState[Game.KeyAltRight])
            {
                string name = p.Name;
                {
                    float posX = p.TextX;
                    float posY = p.TextY;
                    float posZ = p.TextZ;
                    float shadow = (game.one * game.MaybeGetLight(game.platform.FloatToInt(posX), game.platform.FloatToInt(posZ), game.platform.FloatToInt(posY))) / Game.maxlight;
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
                    game.GLRotate(-game.player.playerorientation.Y * 360 / (2 * Game.GetPi()), 0, 1, 0);
                    game.GLRotate(-game.player.playerorientation.X * 360 / (2 * Game.GetPi()), 1, 0, 0);
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

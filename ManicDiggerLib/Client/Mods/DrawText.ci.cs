public class ModDrawText : ClientMod
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
            if (e.drawText == null)
            {
                continue;
            }
            if (e.networkPosition != null && (!e.networkPosition.PositionLoaded))
            {
                continue;
            }
            int kKey = i;
            EntityDrawText p = game.entities[i].drawText;
            float posX = - game.platform.MathSin(e.position.roty) * p.dx + e.position.x;
            float posY = p.dy + e.position.y;
            float posZ = game.platform.MathCos(e.position.roty) * p.dz + e.position.z;
            //todo if picking
            if ((game.Dist(game.player.position.x, game.player.position.y, game.player.position.z, posX, posY, posZ) < 20)
                || game.keyboardState[Game.KeyAltLeft] || game.keyboardState[Game.KeyAltRight])
            {
                string text = p.text;
                {

                    float shadow = (game.one * game.GetLight(game.platform.FloatToInt(posX), game.platform.FloatToInt(posZ), game.platform.FloatToInt(posY))) / Game.maxlight;

                    game.GLPushMatrix();
                    game.GLTranslate(posX, posY, posZ);

                    game.GLRotate(180, 1, 0, 0);
                    game.GLRotate(e.position.roty * 360 / (2 * Game.GetPi()), 0, 1, 0);
                    float scale = game.one * 5 / 1000;
                    game.GLScale(scale, scale, scale);

                    FontCi font = new FontCi();
                    font.family = "Arial";
                    font.size = 14;
                    game.Draw2dText(text, font, -game.TextSizeWidth(text, 14) / 2, 0, IntRef.Create(Game.ColorFromArgb(255, 255, 255, 255)), true);

                    game.GLPopMatrix();
                }
            }
        }
    }
}

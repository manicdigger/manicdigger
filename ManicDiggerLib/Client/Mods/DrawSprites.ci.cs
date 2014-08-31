public class ModDrawSprites : ClientMod
{
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        float one = 1;
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity entity = game.entities[i];
            if (entity == null) { continue; }
            if (entity.sprite == null) { continue; }
            Sprite b = entity.sprite;
            game.GLMatrixModeModelView();
            game.GLPushMatrix();
            game.GLTranslate(b.positionX, b.positionY, b.positionZ);
            game.GLRotate(0 - game.player.playerorientation.Y * 360 / (2 * Game.GetPi()), 0, 1, 0);
            game.GLRotate(0 - game.player.playerorientation.X * 360 / (2 * Game.GetPi()), 1, 0, 0);
            game.GLScale((one * 2 / 100), (one * 2 / 100), (one * 2 / 100));
            game.GLTranslate(0 - b.size / 2, 0 - b.size / 2, 0);
            //d_Draw2d.Draw2dTexture(night ? moontexture : suntexture, 0, 0, ImageSize, ImageSize, null, Color.White);
            IntRef n = null;
            if (b.animationcount > 0)
            {
                float progress = one - (entity.expires.timeLeft / entity.expires.totalTime);
                n = IntRef.Create(game.platform.FloatToInt(progress * (b.animationcount * b.animationcount - 1)));
            }
            game.Draw2dTexture(game.GetTexture(b.image), 0, 0, b.size, b.size, n, b.animationcount, Game.ColorFromArgb(255, 255, 255, 255), true);
            game.GLPopMatrix();
        }
    }
}

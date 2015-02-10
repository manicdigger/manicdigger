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
            Billboard(game);
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
    
    public static void Billboard(Game game)
    {
        float[] m = game.mvMatrix.Peek();
        // http://stackoverflow.com/a/5487981
        // | d 0 0 T.x |
        // | 0 d 0 T.y |
        // | 0 0 d T.z |
        // | 0 0 0   1 |
        float d = game.platform.MathSqrt(m[0] * m[0] + m[1] * m[1] + m[2] * m[2]);

        m[0] = d;
        m[1] = 0;
        m[2] = 0;
        m[3] = 0;

        m[4] = 0;
        m[5] = d;
        m[6] = 0;
        m[7] = 0;

        m[8] = 0;
        m[9] = 0;
        m[10] = d;
        m[11] = 0;

        m[12] = m[12];
        m[13] = m[13];
        m[14] = m[14];
        m[15] = 1;

        Mat4.RotateX(m, m, Game.GetPi());

        game.GLLoadMatrix(m);
    }
}

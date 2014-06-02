public class SunMoonRenderer
{
    public SunMoonRenderer()
    {
        one = 1;
        hour = 6;
        t = 0;
        suntexture = -1;
        moontexture = -1;
        ImageSize = 96;
        day_length_in_seconds = 30;
    }
    internal Game game;
    int hour;
    float one;
    public int GetHour()
    {
        return hour;
    }
    public void SetHour(int value)
    {
        hour = value;
        t = (hour - 6) / (one * 24) * 2 * Game.GetPi();
    }
    float t;
    int suntexture;
    int moontexture;
    internal int ImageSize;
    internal float day_length_in_seconds;
    public void Draw(float dt)
    {
        GamePlatform platform = game.platform;
        game.GLMatrixModeModelView();
        if (suntexture == -1)
        {
            suntexture = game.GetTexture("sun.png");
            moontexture = game.GetTexture("moon.png");
        }
        t += dt * 2 * Game.GetPi() / day_length_in_seconds;
        bool night = (t + 2 * Game.GetPi()) % (2 * Game.GetPi()) > Game.GetPi();
        float tt = t;
        if (night)
        {
            tt = -t;
            //tt -= Math.PI;
        }
        float posX = platform.MathCos(tt) * 20;
        float posY = platform.MathSin(tt) * 20;
        float posZ = platform.MathSin(t) * 20;
        posX += game.player.playerposition.X;
        posY += game.player.playerposition.Y;
        posZ += game.player.playerposition.Z;
        game.GLPushMatrix();
        game.GLTranslate(posX, posY, posZ);
        game.GLRotate(-game.player.playerorientation.Y * 360 / (2 * Game.GetPi()), 0, 1, 0);
        game.GLRotate(-game.player.playerorientation.X * 360 / (2 * Game.GetPi()), 1, 0, 0);
        game.GLScale(one * 2 / 100, one * 2 / 100, one * 2 / 100);
        //GL.Translate(-ImageSize / 2, -ImageSize / 2, 0);
        game.Draw2dTexture(night ? moontexture : suntexture, 0, 0, ImageSize, ImageSize, null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
        game.GLPopMatrix();
    }
}

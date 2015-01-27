public class ModCompass : ClientMod
{
    public ModCompass()
    {
        one = 1;
        compassid = -1;
        needleid = -1;
        compassangle = 0;
        compassvertex = 1;
    }

    public override void OnNewFrameDraw2d(Game game, float dt)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        DrawCompass(game);
    }

    float one;
    int compassid;
    int needleid;
    float compassangle;
    float compassvertex;

    bool CompassInActiveMaterials(Game game)
    {
        for (int i = 0; i < 10; i++)
        {
            if (game.MaterialSlots_(i) == game.d_Data.BlockIdCompass())
            {
                return true;
            }
        }
        return false;
    }

    public void DrawCompass(Game game)
    {
        if (!CompassInActiveMaterials(game)) return;
        if (compassid == -1)
        {
            compassid = game.GetTexture("compass.png");
            needleid = game.GetTexture("compassneedle.png");
        }
        float size = 175;
        float posX = game.Width() - 100;
        float posY = 100;
        float playerorientation = -((game.player.position.roty / (2 * Game.GetPi())) * 360);

        compassvertex += (playerorientation - compassangle) / 50;
        compassvertex *= (one * 9 / 10);
        compassangle += compassvertex;

        // compass
        game.Draw2dTexture(compassid, posX - size / 2, posY - size / 2, size, size, null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);

        // compass needle
        game.GLPushMatrix();
        game.GLTranslate(posX, posY, 0);
        game.GLRotate(compassangle, 0, 0, 90);
        game.GLTranslate(-size / 2, -size / 2, 0);
        game.Draw2dTexture(needleid, 0, 0, size, size, null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
        game.GLPopMatrix();
    }
}

public class ModDrawLinesAroundSelectedBlock : ClientMod
{
    public ModDrawLinesAroundSelectedBlock()
    {
        one = 1;
    }
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        if (game.ENABLE_DRAW2D)
        {
            DrawLinesAroundSelectedBlock(game, game.SelectedBlockPositionX,
              game.SelectedBlockPositionY, game.SelectedBlockPositionZ);
        }
    }
    float one;

    Model wireframeCube;
    public void DrawLinesAroundSelectedBlock(Game game, float x, float y, float z)
    {
        if (x == -1 && y == -1 && z == -1)
        {
            return;
        }

        float pickcubeheight = game.getblockheight(game.platform.FloatToInt(x), game.platform.FloatToInt(z), game.platform.FloatToInt(y));

        float posx = x + one / 2;
        float posy = y + pickcubeheight * one / 2;
        float posz = z + one / 2;

        game.platform.GLLineWidth(2);
        float size = one * 51 / 100;
        game.platform.BindTexture2d(0);

        if (wireframeCube == null)
        {
            ModelData data = WireframeCube.Get();
            wireframeCube = game.platform.CreateModel(data);
        }
        game.GLPushMatrix();
        game.GLTranslate(posx, posy, posz);
        game.GLScale(size, pickcubeheight * size, size);
        game.DrawModel(wireframeCube);
        game.GLPopMatrix();
    }
}

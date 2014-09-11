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
            float size = one * 102 / 100;
            if (game.SelectedEntityId != -1)
            {
                Entity e = game.entities[game.SelectedEntityId];
                if (e == null)
                {
                    return;
                }
                DrawLinesAroundSelectedBlock(game,
                    e.position.x, e.position.y + e.drawModel.ModelHeight / 2, e.position.z,
                    size, size * e.drawModel.ModelHeight, size);
            }
            else
            {
                if (game.SelectedBlockPositionX == -1)
                {
                    return;
                }
                int x = game.SelectedBlockPositionX;
                int y = game.SelectedBlockPositionY;
                int z = game.SelectedBlockPositionZ;
                float pickcubeheight = game.getblockheight(game.platform.FloatToInt(x), game.platform.FloatToInt(z), game.platform.FloatToInt(y));
                float posx = x + one / 2;
                float posy = y + pickcubeheight * one / 2;
                float posz = z + one / 2;
                float scalex = size;
                float scaley = size * pickcubeheight;
                float scalez = size;
                DrawLinesAroundSelectedBlock(game, posx, posy, posz, scalex, scaley, scalez);
            }
        }
    }
    float one;

    Model wireframeCube;
    public void DrawLinesAroundSelectedBlock(Game game, float posx, float posy, float posz, float scalex, float scaley, float scalez)
    {
        game.platform.GLLineWidth(2);
        
        game.platform.BindTexture2d(0);

        if (wireframeCube == null)
        {
            ModelData data = WireframeCube.Get();
            wireframeCube = game.platform.CreateModel(data);
        }
        game.GLPushMatrix();
        game.GLTranslate(posx, posy, posz);
        float half = one / 2;
        game.GLScale(scalex * half, scaley * half, scalez * half);
        game.DrawModel(wireframeCube);
        game.GLPopMatrix();
    }
}

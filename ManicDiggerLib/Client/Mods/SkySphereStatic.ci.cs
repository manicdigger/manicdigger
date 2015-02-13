public class ModSkySphereStatic : ClientMod
{
    public ModSkySphereStatic()
    {
        SkyTexture = -1;
        skyspheretexture = -1;
        skyspherenighttexture = -1;
    }
    internal int SkyTexture;
    Model skymodel;

    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        game.platform.GlDisableFog();
        DrawSkySphere(game);
        game.SetFog();
    }

    internal int skyspheretexture;
    internal int skyspherenighttexture;

    internal void DrawSkySphere(Game game)
    {
        if (skyspheretexture == -1)
        {
            BitmapCi skysphereBmp = game.platform.BitmapCreateFromPng(game.GetFile("skysphere.png"), game.GetFileLength("skysphere.png"));
            BitmapCi skysphereNightBmp = game.platform.BitmapCreateFromPng(game.GetFile("skyspherenight.png"), game.GetFileLength("skyspherenight.png"));
            skyspheretexture = game.platform.LoadTextureFromBitmap(skysphereBmp);
            skyspherenighttexture = game.platform.LoadTextureFromBitmap(skysphereNightBmp);
            game.platform.BitmapDelete(skysphereBmp);
            game.platform.BitmapDelete(skysphereNightBmp);
        }
        int texture = game.SkySphereNight ? skyspherenighttexture : skyspheretexture;
        if (game.shadowssimple) //d_Shadows.GetType() == typeof(ShadowsSimple))
        {
            texture = skyspheretexture;
        }
        SkyTexture = texture;
        Draw(game, game.currentfov());
    }

    public void Draw(Game game, float fov)
    {
        if (SkyTexture == -1)
        {
            game.platform.ThrowException("InvalidOperationException");
        }
        int size = 1000;
        if (skymodel == null)
        {
            skymodel = game.platform.CreateModel(SphereModelData.GetSphereModelData(size, size, 20, 20));
        }
        game.Set3dProjection(size * 2, fov);
        game.GLMatrixModeModelView();
        game.GLPushMatrix();
        game.GLTranslate(game.player.position.x,
            game.player.position.y,
            game.player.position.z);
        game.platform.BindTexture2d(SkyTexture);
        game.DrawModel(skymodel);
        game.GLPopMatrix();
        game.Set3dProjection(game.zfar(), fov);
    }
}

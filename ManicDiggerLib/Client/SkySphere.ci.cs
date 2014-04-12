public class SkySphere
{
    public SkySphere()
    {
        SkyTexture = -1;
    }
    internal Game game;
    internal MeshBatcher d_MeshBatcher;
    internal int SkyTexture;
    //int SkyMeshId = -1;
    Model skymodel;
    public void Draw(float fov)
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
        d_MeshBatcher.BindTexture = false;
        game.GLPushMatrix();
        game.GLTranslate(game.player.playerposition.X,
            game.player.playerposition.Y,
            game.player.playerposition.Z);
        game.platform.BindTexture2d(SkyTexture);
        game.DrawModel(skymodel);
        game.GLPopMatrix();
        game.Set3dProjection(game.zfar(), fov);
    }
}

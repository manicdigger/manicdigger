public class ModDrawTerrain : ClientMod
{
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        game.terrainRenderer.DrawTerrain();
    }
}

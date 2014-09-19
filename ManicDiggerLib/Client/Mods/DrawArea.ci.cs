public class ModDrawArea : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        lines = new DrawWireframeCube();
    }
    
    DrawWireframeCube lines;

    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        if (game.ENABLE_DRAW2D)
        {
            for (int i = 0; i < game.entitiesCount; i++)
            {
                Entity e = game.entities[i];
                if (e == null) { continue; }
                if (e.drawArea == null) { continue; }
                if (!e.drawArea.visible) { continue; }
                int x = e.drawArea.x + e.drawArea.sizex / 2;
                int y = e.drawArea.y + e.drawArea.sizey / 2;
                int z = e.drawArea.z + e.drawArea.sizez / 2;
                float scalex = e.drawArea.sizex;
                float scaley = e.drawArea.sizey;
                float scalez = e.drawArea.sizez;
                lines.DrawWireframeCube_(game, x, y, z, scalex, scaley, scalez);
            }
        }
    }

    public override void OnHitEntity(Game game, OnUseEntityArgs e)
    {
        Entity entity = game.entities[e.entityId];
        if (entity == null) { return; }
        if (entity.drawArea == null) { return; }
        entity.drawArea.visible = !entity.drawArea.visible;
    }
}

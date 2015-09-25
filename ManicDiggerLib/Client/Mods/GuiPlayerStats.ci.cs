public class ModGuiPlayerStats : ClientMod
{
    public override void OnNewFrameDraw2d(Game game, float deltaTime)
    {
        if (game.guistate != GuiState.MapLoading)
        {
            DrawPlayerHealth(game);
            DrawPlayerOxygen(game);
        }
    }

    //Size of Health/Oxygen bar
    const int barSizeX = 120;
    const int barSizeY = 20;
    const int barOffset = 130;
    const int barDistanceToMargin = 40;

    public void DrawPlayerHealth(Game game)
    {
        if (game.PlayerStats != null)
        {
            float progress = game.one * game.PlayerStats.CurrentHealth / game.PlayerStats.MaxHealth;
            int InventoryStartX = game.Width() / 2 - 540 / 2;
            int InventoryStartY = game.Height() - 110;
            int posX = InventoryStartX + 10;
            int posY = InventoryStartY + 10;
            game.Draw2dTexture(game.GetTexture("background.png"), posX, posY - barSizeY, barSizeX, barSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
            game.Draw2dTexture(game.GetTexture("background.png"), posX, posY - barSizeY, (progress) * barSizeX, barSizeY, null, 0, Game.ColorFromArgb(255, 255, 0, 0), false);
        }
        //if (test) { d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), 50, 50, 200, 200, null, Color.Red); }
    }

    public void DrawPlayerOxygen(Game game)
    {
        if (game.PlayerStats != null)
        {
            if (game.PlayerStats.CurrentOxygen < game.PlayerStats.MaxOxygen)
            {
                float progress = game.one * game.PlayerStats.CurrentOxygen / game.PlayerStats.MaxOxygen;
                int InventoryStartX = game.Width() / 2 - 540 / 2;
                int InventoryStartY = game.Height() - 140;
                int posX = InventoryStartX + 10;
                int posY = InventoryStartY + 10;
                game.Draw2dTexture(game.GetTexture("background.png"), posX, posY - barSizeY, barSizeX, barSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
                game.Draw2dTexture(game.GetTexture("background.png"), posX, posY - barSizeY, (progress) * barSizeX, barSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 255), false);
            }
        }
    }
}

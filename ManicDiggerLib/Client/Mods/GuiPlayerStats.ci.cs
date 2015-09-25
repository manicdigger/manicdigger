public class ModGuiPlayerStats : ClientMod
{
    public override void OnNewFrameDraw2d(Game game, float deltaTime)
    {
        healthPosX = game.Width() / 2 - baseSizeX - centerOffset;
        healthPosY = game.Height() - 150;
        oxygenPosX = game.Width() / 2 + centerOffset;
        oxygenPosY = game.Height() - 150;

        if (game.guistate != GuiState.MapLoading)
        {
            DrawPlayerHealth(game);
            DrawPlayerOxygen(game);
        }
    }

    //Sizes of Health/Oxygen bar
    const int baseSizeX = 256;
    const int baseSizeY = 32;
    const int centerOffset = 20;

    int healthPosX;
    int healthPosY;
    int oxygenPosX;
    int oxygenPosY;

    public void DrawPlayerHealth(Game game)
    {
        if (game.PlayerStats != null)
        {
            float progress = game.one * game.PlayerStats.CurrentHealth / game.PlayerStats.MaxHealth;
            game.Draw2dTexture(game.WhiteTexture(), healthPosX, healthPosY, baseSizeX, baseSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
            game.Draw2dTexturePart(game.GetTexture("background.png"), progress, 1, healthPosX, healthPosY, (progress) * baseSizeX, baseSizeY, Game.ColorFromArgb(255, 255, 0, 0), false);
        }
    }

    public void DrawPlayerOxygen(Game game)
    {
        if (game.PlayerStats != null)
        {
            if (game.PlayerStats.CurrentOxygen < game.PlayerStats.MaxOxygen)
            {
                float progress = game.one * game.PlayerStats.CurrentOxygen / game.PlayerStats.MaxOxygen;
                game.Draw2dTexture(game.WhiteTexture(), oxygenPosX, oxygenPosY, baseSizeX, baseSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
                game.Draw2dTexturePart(game.GetTexture("background.png"), progress, 1, oxygenPosX, oxygenPosY, (progress) * baseSizeX, baseSizeY, Game.ColorFromArgb(255, 0, 0, 255), false);
            }
        }
    }
}

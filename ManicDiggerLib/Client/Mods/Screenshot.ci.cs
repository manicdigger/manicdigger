public class ModScreenshot : ClientMod
{
    public override void OnNewFrameDraw2d(Game game, float deltaTime)
    {
        if (screenshotflash > 0)
        {
            DrawScreenshotFlash(game);
            screenshotflash--;
        }
    }

    internal int screenshotflash;

    internal void DrawScreenshotFlash(Game game)
    {
        game.Draw2dTexture(game.WhiteTexture(), 0, 0, game.platform.GetCanvasWidth(), game.platform.GetCanvasHeight(), null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
        string screenshottext = "&0Screenshot";
        IntRef textWidth = new IntRef();
        IntRef textHeight = new IntRef();
        game.platform.TextSize(screenshottext, 50, textWidth, textHeight);
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = 50;
        game.Draw2dText(screenshottext, font, game.xcenter(textWidth.value), game.ycenter(textHeight.value), null, false);
    }

    public override void OnKeyDown(Game game, KeyEventArgs args)
    {
        if (args.GetKeyCode() == game.GetKey(GlKeys.F12))
        {
            game.platform.SaveScreenshot();
            screenshotflash = 5;
            args.SetHandled(true);
        }
    }
}

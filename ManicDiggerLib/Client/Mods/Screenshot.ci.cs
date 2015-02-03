public class ModScreenshot : ClientMod
{
    public ModScreenshot()
    {
        takeScreenshot = false;
        screenshotFlashFramesLeft = 0;
    }

    bool takeScreenshot;
    int screenshotFlashFramesLeft;

    public override void OnNewFrameDraw2d(Game game, float deltaTime)
    {
        if (takeScreenshot)
        {
            takeScreenshot = false;
            // Must be done after rendering, but before SwapBuffers
            game.platform.SaveScreenshot();
            screenshotFlashFramesLeft = 5;
        }
        if (screenshotFlashFramesLeft > 0)
        {
            DrawScreenshotFlash(game);
            screenshotFlashFramesLeft--;
        }
    }

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
            takeScreenshot = true;
            args.SetHandled(true);
        }
    }
}

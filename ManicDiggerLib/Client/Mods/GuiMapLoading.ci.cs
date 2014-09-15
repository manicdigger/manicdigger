public class ModGuiMapLoading : ClientMod
{
    int Width;
    int Height;
    int backgroundW;
    int backgroundH;

    public override void OnNewFrameDraw2d(Game game, float deltaTime)
    {
        if (game.guistate != GuiState.MapLoading)
        {
            return;
        }

        GamePlatform platform = game.platform;
        float one = 1;

        Width = platform.GetCanvasWidth();
        Height = platform.GetCanvasHeight();
        DrawBackground(game);

        string connecting = game.language.Connecting();
        if (game.issingleplayer && (!platform.SinglePlayerServerLoaded()))
        {
            connecting = "Starting game...";
        }
        if (game.maploadingprogress.ProgressStatus != null)
        {
            connecting = game.maploadingprogress.ProgressStatus;
        }

        if (game.invalidVersionDrawMessage != null)
        {
            game.Draw2dText(game.invalidVersionDrawMessage, game.fontMapLoading, game.xcenter(game.TextSizeWidth(game.invalidVersionDrawMessage, 14)), Height / 2 - 50, null, false);
            string connect = "Click to connect";
            game.Draw2dText(connect, game.fontMapLoading, game.xcenter(game.TextSizeWidth(connect, 14)), Height / 2 + 50, null, false);
            return;
        }

        IntRef serverNameWidth = new IntRef();
        IntRef serverNameHeight = new IntRef();
        platform.TextSize(game.ServerInfo.ServerName, 14, serverNameWidth, serverNameHeight);
        game.Draw2dText(game.ServerInfo.ServerName, game.fontMapLoading, game.xcenter(serverNameWidth.value), Height / 2 - 150, null, false);

        if (game.ServerInfo.ServerMotd != null)
        {
            IntRef serverMotdWidth = new IntRef();
            IntRef serverMotdHeight = new IntRef();
            platform.TextSize(game.ServerInfo.ServerMotd, 14, serverMotdWidth, serverMotdHeight);
            game.Draw2dText(game.ServerInfo.ServerMotd, game.fontMapLoading, game.xcenter(serverMotdWidth.value), Height / 2 - 100, null, false);
        }

        IntRef connectingWidth = new IntRef();
        IntRef connectingHeight = new IntRef();
        platform.TextSize(connecting, 14, connectingWidth, connectingHeight);
        game.Draw2dText(connecting, game.fontMapLoading, game.xcenter(connectingWidth.value), Height / 2 - 50, null, false);

        string progress = platform.StringFormat(game.language.ConnectingProgressPercent(), platform.IntToString(game.maploadingprogress.ProgressPercent));
        string progress1 = platform.StringFormat(game.language.ConnectingProgressKilobytes(), platform.IntToString(game.maploadingprogress.ProgressBytes / 1024));

        if (game.maploadingprogress.ProgressPercent > 0)
        {
            IntRef progressWidth = new IntRef();
            IntRef progressHeight = new IntRef();
            platform.TextSize(progress, 14, progressWidth, progressHeight);
            game.Draw2dText(progress, game.fontMapLoading, game.xcenter(progressWidth.value), Height / 2 - 20, null, false);

            IntRef progress1Width = new IntRef();
            IntRef progress1Height = new IntRef();
            platform.TextSize(progress1, 14, progress1Width, progress1Height);
            game.Draw2dText(progress1, game.fontMapLoading, game.xcenter(progress1Width.value), Height / 2 + 10, null, false);

            float progressratio = one * game.maploadingprogress.ProgressPercent / 100;
            int sizex = 400;
            int sizey = 40;
            game.Draw2dTexture(game.WhiteTexture(), game.xcenter(sizex), Height / 2 + 70, sizex, sizey, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
            int red = Game.ColorFromArgb(255, 255, 0, 0);
            int yellow = Game.ColorFromArgb(255, 255, 255, 0);
            int green = Game.ColorFromArgb(255, 0, 255, 0);
            int[] colors = new int[3];
            colors[0] = red;
            colors[1] = yellow;
            colors[2] = green;
            int c = InterpolationCi.InterpolateColor(platform, progressratio, colors, 3);
            game.Draw2dTexture(game.WhiteTexture(), game.xcenter(sizex), Height / 2 + 70, progressratio * sizex, sizey, null, 0, c, false);
        }
    }

    void DrawBackground(Game game)
    {
        backgroundW = 512;
        backgroundH = 512;
        //Background tiling
        int countX = game.platform.FloatToInt(Width / backgroundW) + 1;
        int countY = game.platform.FloatToInt(Height / backgroundH) + 1;
        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                game.Draw2dTexture(game.GetTexture("background.png"), x * backgroundW, y * backgroundH, backgroundW, backgroundH, null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
            }
        }
    }
}

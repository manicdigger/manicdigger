public class ScreenMain : Screen
{
    public ScreenMain()
    {
        singleplayer = new MenuWidget();
        multiplayer = new MenuWidget();
        exit = new MenuWidget();
        widgets[0] = singleplayer;
        widgets[1] = multiplayer;
        widgets[2] = exit;
        queryStringChecked = false;
        cursorLoaded = false;
    }

    MenuWidget singleplayer;
    MenuWidget multiplayer;
    MenuWidget exit;
    internal float windowX;
    internal float windowY;
    bool queryStringChecked;
    bool cursorLoaded;

    public override void Render(float dt)
    {
        windowX = menu.p.GetCanvasWidth();
        windowY = menu.p.GetCanvasHeight();

        float scale = menu.GetScale();

        if (menu.assetsLoadProgress.value != 1)
        {
            string s = menu.p.StringFormat(menu.lang.Get("MainMenu_AssetsLoadProgress"), menu.p.FloatToString(menu.p.FloatToInt(menu.assetsLoadProgress.value * 100)));
            menu.DrawText(s, 20 * scale, windowX / 2, windowY / 2, TextAlign.Center, TextBaseline.Middle);
            return;
        }

        if (!cursorLoaded)
        {
            menu.p.SetWindowCursor(0, 0, 32, 32, menu.GetFile("mousecursor.png"), menu.GetFileLength("mousecursor.png"));
            cursorLoaded = true;
        }

        UseQueryStringIpAndPort(menu);

        menu.DrawBackground();
        menu.Draw2dQuad(menu.GetTexture("logo.png"), windowX / 2 - 1024 * scale / 2, 0, 1024 * scale, 512 * scale);

        int buttonheight = 64;
        int buttonwidth = 256;
        int spacebetween = 5;
        int offsetfromborder = 50;

        singleplayer.text = menu.lang.Get("MainMenu_Singleplayer");
        singleplayer.x = windowX / 2 - (buttonwidth / 2) * scale;
        singleplayer.y = windowY - (3 * (buttonheight * scale + spacebetween)) - offsetfromborder * scale;
        singleplayer.sizex = buttonwidth * scale;
        singleplayer.sizey = buttonheight * scale;

        multiplayer.text = menu.lang.Get("MainMenu_Multiplayer");
        multiplayer.x = windowX / 2 - (buttonwidth / 2) * scale;
        multiplayer.y = windowY - (2 * (buttonheight * scale + spacebetween)) - offsetfromborder * scale;
        multiplayer.sizex = buttonwidth * scale;
        multiplayer.sizey = buttonheight * scale;

        exit.visible = menu.p.ExitAvailable();

        exit.text = menu.lang.Get("MainMenu_Quit");
        exit.x = windowX / 2 - (buttonwidth / 2) * scale;
        exit.y = windowY - (1 * (buttonheight * scale + spacebetween)) - offsetfromborder * scale;
        exit.sizex = buttonwidth * scale;
        exit.sizey = buttonheight * scale;
        DrawWidgets();
    }

    void UseQueryStringIpAndPort(MainMenu menu)
    {
        if (queryStringChecked)
        {
            return;
        }
        queryStringChecked = true;
        string ip = menu.p.QueryStringValue("ip");
        string port = menu.p.QueryStringValue("port");
        int portInt = 25565;
        if (port != null && menu.p.FloatTryParse(port, new FloatRef()))
        {
            portInt = menu.p.IntParse(port);
        }
        if (ip != null)
        {
            menu.StartLogin(null, ip, portInt);
        }
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == singleplayer)
        {
            menu.StartSingleplayer();
        }
        if (w == multiplayer)
        {
            menu.StartMultiplayer();
        }
        if (w == exit)
        {
            menu.Exit();
        }
    }

    public override void OnBackPressed()
    {
        menu.Exit();
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        // debug
        if (e.GetKeyCode() == GlKeys.F5)
        {
            menu.p.SinglePlayerServerDisable();
            menu.StartGame(true, menu.p.PathCombine(menu.p.PathSavegames(), "Default.mdss"), null);
        }
        if (e.GetKeyCode() == GlKeys.F6)
        {
            menu.StartGame(true, menu.p.PathCombine(menu.p.PathSavegames(), "Default.mddbs"), null);
        }
    }
}

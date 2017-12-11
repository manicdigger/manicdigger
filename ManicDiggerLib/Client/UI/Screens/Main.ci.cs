public class ScreenMain : Screen
{
    public ScreenMain()
    {
		logo = new ImageWidget();
		logo.SetTextureName("logo.png");
        singleplayer = new ButtonWidget();
		singleplayer.SetClickable(true);
        multiplayer = new ButtonWidget();
		multiplayer.SetClickable(true);
		exit = new ButtonWidget();
		exit.SetClickable(true);
		AddWidgetNew(logo);
		AddWidgetNew(singleplayer);
		AddWidgetNew(multiplayer);
		AddWidgetNew(exit);
        queryStringChecked = false;
        cursorLoaded = false;
        fontDefault = new FontCi();
        fontDefault.size = 20;
    }

	ImageWidget logo;
    ButtonWidget singleplayer;
	ButtonWidget multiplayer;
	ButtonWidget exit;
    internal float windowX;
    internal float windowY;
    bool queryStringChecked;
    bool cursorLoaded;
    FontCi fontDefault;

    public override void Render(float dt)
    {
        windowX = menu.p.GetCanvasWidth();
        windowY = menu.p.GetCanvasHeight();

        float scale = menu.GetScale();

        if (menu.assetsLoadProgress.value != 1)
        {
            string s = menu.p.StringFormat(menu.lang.Get("MainMenu_AssetsLoadProgress"), menu.p.FloatToString(menu.p.FloatToInt(menu.assetsLoadProgress.value * 100)));
            menu.DrawText(s, fontDefault, windowX / 2, windowY / 2, TextAlign.Center, TextBaseline.Middle);
            return;
        }

        if (!cursorLoaded)
        {
            menu.p.SetWindowCursor(0, 0, 32, 32, menu.GetFile("mousecursor.png"), menu.GetFileLength("mousecursor.png"));
            cursorLoaded = true;
        }

        UseQueryStringIpAndPort(menu);

        menu.DrawBackground();
		//menu.Draw2dQuad(menu.GetTexture("logo.png"), windowX / 2 - 1024 * scale / 2, 0, 1024 * scale, 512 * scale);

		logo.sizex = 1024 * scale;
		logo.sizey = 256 * scale;
		logo.x = windowX / 2 - logo.sizex / 2;
		logo.y = 50 * scale;

		float buttonheight = 64 * scale;
		float buttonwidth = 256 * scale;
		float spacebetween = 5 * scale;
		float offsetfromborder = 50 * scale;

        singleplayer.SetText(menu.lang.Get("MainMenu_Singleplayer"));
        singleplayer.x = windowX / 2 - (buttonwidth / 2);
        singleplayer.y = windowY - (3 * (buttonheight + spacebetween)) - offsetfromborder;
        singleplayer.sizex = buttonwidth;
        singleplayer.sizey = buttonheight;

        multiplayer.SetText(menu.lang.Get("MainMenu_Multiplayer"));
        multiplayer.x = windowX / 2 - (buttonwidth / 2);
        multiplayer.y = windowY - (2 * (buttonheight + spacebetween)) - offsetfromborder;
        multiplayer.sizex = buttonwidth;
        multiplayer.sizey = buttonheight;

        exit.visible = menu.p.ExitAvailable();

        exit.SetText(menu.lang.Get("MainMenu_Quit"));
        exit.x = windowX / 2 - (buttonwidth / 2);
        exit.y = windowY - (1 * (buttonheight + spacebetween)) - offsetfromborder;
        exit.sizex = buttonwidth;
        exit.sizey = buttonheight;
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

    public override void OnButtonA(AbstractMenuWidget w)
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

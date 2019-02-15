public class ScreenMain : MainMenuScreen
{
	public ScreenMain()
	{
		queryStringChecked = false;
		cursorLoaded = false;
		assetsLoaded = false;

		wtxt_loading = new TextWidget();
		wtxt_loading.SetFont(fontDefault);
		wtxt_loading.SetAlignment(TextAlign.Center);
		wtxt_loading.SetBaseline(TextBaseline.Middle);
		AddWidget(wtxt_loading);
		wimg_logo = new ImageWidget();
		wimg_logo.SetTextureName("logo.png");
		AddWidget(wimg_logo);
		wbtn_singleplayer = new ButtonWidget();
		AddWidget(wbtn_singleplayer);
		wbtn_multiplayer = new ButtonWidget();
		AddWidget(wbtn_multiplayer);
		wbtn_exit = new ButtonWidget();
		AddWidget(wbtn_exit);
	}

	TextWidget wtxt_loading;
	ImageWidget wimg_logo;
	ButtonWidget wbtn_singleplayer;
	ButtonWidget wbtn_multiplayer;
	ButtonWidget wbtn_exit;
	internal float windowX;
	internal float windowY;
	bool queryStringChecked;
	bool assetsLoaded;
	bool cursorLoaded;

	public override void LoadTranslations()
	{
		wbtn_singleplayer.SetText(menu.lang.Get("MainMenu_Singleplayer"));
		wbtn_multiplayer.SetText(menu.lang.Get("MainMenu_Multiplayer"));
		wbtn_exit.SetText(menu.lang.Get("MainMenu_Quit"));
	}

	public override void Render(float dt)
	{
		windowX = menu.p.GetCanvasWidth();
		windowY = menu.p.GetCanvasHeight();

		if (!assetsLoaded)
		{
			if (menu.uiRenderer.GetAssetLoadProgress().value != 1)
			{
				string s = menu.p.StringFormat(menu.lang.Get("MainMenu_AssetsLoadProgress"), menu.p.FloatToString(menu.p.FloatToInt(menu.uiRenderer.GetAssetLoadProgress().value * 100)));
				wtxt_loading.SetX(windowX / 2);
				wtxt_loading.SetY(windowY / 2);
				wtxt_loading.SetText(s);
				return;
			}
			assetsLoaded = true;
			wtxt_loading.SetVisible(false);
		}

		if (!cursorLoaded)
		{
			menu.p.SetWindowCursor(0, 0, 32, 32, menu.uiRenderer.GetFile("mousecursor.png"), menu.uiRenderer.GetFileLength("mousecursor.png"));
			cursorLoaded = true;
		}

		if (!queryStringChecked)
		{
			UseQueryStringIpAndPort(menu);
			queryStringChecked = true;
		}

		float scale = menu.uiRenderer.GetScale();
		float buttonheight = 64 * scale;
		float buttonwidth = 256 * scale;
		float spacebetween = 5 * scale;
		float offsetfromborder = 50 * scale;

		wimg_logo.sizex = 1024 * scale;
		wimg_logo.sizey = 256 * scale;
		wimg_logo.x = windowX / 2 - wimg_logo.sizex / 2;
		wimg_logo.y = 50 * scale;

		wbtn_singleplayer.x = windowX / 2 - (buttonwidth / 2);
		wbtn_singleplayer.y = windowY - (3 * (buttonheight + spacebetween)) - offsetfromborder;
		wbtn_singleplayer.sizex = buttonwidth;
		wbtn_singleplayer.sizey = buttonheight;

		wbtn_multiplayer.x = windowX / 2 - (buttonwidth / 2);
		wbtn_multiplayer.y = windowY - (2 * (buttonheight + spacebetween)) - offsetfromborder;
		wbtn_multiplayer.sizex = buttonwidth;
		wbtn_multiplayer.sizey = buttonheight;

		wbtn_exit.visible = menu.p.ExitAvailable();
		wbtn_exit.x = windowX / 2 - (buttonwidth / 2);
		wbtn_exit.y = windowY - (1 * (buttonheight + spacebetween)) - offsetfromborder;
		wbtn_exit.sizex = buttonwidth;
		wbtn_exit.sizey = buttonheight;

		DrawWidgets(dt);
	}

	public override void OnButton(AbstractMenuWidget w)
	{
		if (w == wbtn_singleplayer)
		{
			menu.StartSingleplayer();
		}
		if (w == wbtn_multiplayer)
		{
			menu.StartMultiplayer();
		}
		if (w == wbtn_exit)
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

	void UseQueryStringIpAndPort(MainMenu menu)
	{
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
}

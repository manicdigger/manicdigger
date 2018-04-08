public class ScreenConnectToIp : Screen
{
	public ScreenConnectToIp()
	{
		wbtn_back = new ButtonWidget();
		wbtn_back.SetText("Back");
		AddWidgetNew(wbtn_back);
		wbtn_connect = new ButtonWidget();
		wbtn_connect.SetText("Connect");
		AddWidgetNew(wbtn_connect);
		wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontTitle);
		wtxt_title.SetText("Connect to IP");
		AddWidgetNew(wtxt_title);
		wtxt_ip = new TextWidget();
		wtxt_ip.SetFont(fontDefault);
		wtxt_ip.SetAlignment(TextAlign.Right);
		wtxt_ip.SetBaseline(TextBaseline.Middle);
		wtxt_ip.SetText("IP");
		AddWidgetNew(wtxt_ip);
		wtxt_port = new TextWidget();
		wtxt_port.SetFont(fontDefault);
		wtxt_port.SetAlignment(TextAlign.Right);
		wtxt_port.SetBaseline(TextBaseline.Middle);
		wtxt_port.SetText("Port");
		AddWidgetNew(wtxt_port);
		wtbx_ip = new TextBoxWidget();
		AddWidgetNew(wtbx_ip);
		wtbx_port = new TextBoxWidget();
		AddWidgetNew(wtbx_port);
	}

	ButtonWidget wbtn_back;
	ButtonWidget wbtn_connect;
	TextWidget wtxt_title;
	TextWidget wtxt_ip;
	TextWidget wtxt_port;
	TextBoxWidget wtbx_ip;
	TextBoxWidget wtbx_port;

	bool loaded;

	public override void LoadTranslations()
	{
		wbtn_connect.SetText(menu.lang.Get("MainMenu_ConnectToIpConnect"));
		wtxt_ip.SetText(menu.lang.Get("MainMenu_ConnectToIpIp"));
		wtxt_port.SetText(menu.lang.Get("MainMenu_ConnectToIpPort"));
		wtxt_title.SetText(menu.lang.Get("MainMenu_MultiplayerConnectIP"));
	}

	public override void Render(float dt)
	{
		GamePlatform p = menu.p;

		// load stored values or defaults
		if (!loaded)
		{
			wtbx_ip.SetContent(p, p.GetPreferences().GetString("ConnectToIpIp", "127.0.0.1"));
			wtbx_port.SetContent(p, p.GetPreferences().GetString("ConnectToIpPort", "25565"));
			loaded = true;
		}

		float scale = menu.GetScale();
		float leftx = p.GetCanvasWidth() / 2 - 400 * scale;
		float y = p.GetCanvasHeight() / 2 - 250 * scale;

		wtxt_title.x = leftx;
		wtxt_title.y = y + 50 * scale;

		wtxt_ip.x = leftx;
		wtxt_ip.y = y + 100 * scale;
		wtbx_ip.x = leftx;
		wtbx_ip.y = y + 100 * scale;
		wtbx_ip.sizex = 256 * scale;
		wtbx_ip.sizey = 64 * scale;

		wtxt_port.x = leftx;
		wtxt_port.y = y + 200 * scale;
		wtbx_port.x = leftx;
		wtbx_port.y = y + 200 * scale;
		wtbx_port.sizex = 256 * scale;
		wtbx_port.sizey = 64 * scale;

		wbtn_connect.x = leftx;
		wbtn_connect.y = y + 400 * scale;
		wbtn_connect.sizex = 256 * scale;
		wbtn_connect.sizey = 64 * scale;

		wbtn_back.x = 40 * scale;
		wbtn_back.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		menu.DrawBackground();
		DrawWidgets();
	}

	public override void OnBackPressed()
	{
		menu.StartMultiplayer();
	}

	public override void OnButtonA(AbstractMenuWidget w)
	{
		if (w == wbtn_connect)
		{
			// save user input
			Preferences preferences = menu.p.GetPreferences();
			preferences.SetString("ConnectToIpIp", wtbx_ip.GetContent());
			preferences.SetString("ConnectToIpPort", wtbx_port.GetContent());
			menu.p.SetPreferences(preferences);

			FloatRef ret = new FloatRef();
			if (!Game.StringEquals(wtbx_ip.GetContent(), "")
				&& menu.p.FloatTryParse(wtbx_port.GetContent(), ret))
			{
				menu.StartLogin(null, wtbx_ip.GetContent(), menu.p.IntParse(wtbx_port.GetContent()));
			}
		}
		if (w == wbtn_back)
		{
			OnBackPressed();
		}
	}
}

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
		wtxt_title.SetAlignment(TextAlign.Center);
		wtxt_title.SetText("Connect to IP");
		AddWidgetNew(wtxt_title);
		wtxt_statusMessage = new TextWidget();
		wtxt_statusMessage.SetFont(fontMessage);
		AddWidgetNew(wtxt_statusMessage);
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
	TextWidget wtxt_statusMessage;
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

		float connectAreaWidth = 600;
		float connectAreaHeight = 400;
		float scale = menu.GetScale();
		float leftx = p.GetCanvasWidth() / 2 - (connectAreaWidth / 2) * scale;
		float topy = p.GetCanvasHeight() / 2 - (connectAreaHeight / 2) * scale;

		wtxt_title.x = p.GetCanvasWidth() / 2;
		wtxt_title.y = topy;
		wtxt_statusMessage.x = leftx;
		wtxt_statusMessage.y = topy + 258;

		wtxt_ip.x = leftx - 6 * scale;
		wtxt_ip.y = topy + 82 * scale;
		wtbx_ip.x = leftx;
		wtbx_ip.y = topy + 50 * scale;
		wtbx_ip.sizex = connectAreaWidth * scale;
		wtbx_ip.sizey = 64 * scale;

		wtxt_port.x = leftx - 6 * scale;
		wtxt_port.y = topy + 162 * scale;
		wtbx_port.x = leftx;
		wtbx_port.y = topy + 130 * scale;
		wtbx_port.sizex = connectAreaWidth * scale;
		wtbx_port.sizey = 64 * scale;

		wbtn_connect.x = leftx;
		wbtn_connect.y = topy + 336 * scale;
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
			// check input
			IntRef ret = new IntRef();
			if (Game.StringEquals(wtbx_ip.GetContent(), ""))
			{
				wtxt_statusMessage.SetText("&4Please enter a valid address!");
			}
			else if (!menu.p.IntTryParse(wtbx_port.GetContent(), ret))
			{
				wtxt_statusMessage.SetText("&4Please enter a valid port!");
			}
			else
			{
				// save user input
				Preferences preferences = menu.p.GetPreferences();
				preferences.SetString("ConnectToIpIp", wtbx_ip.GetContent());
				preferences.SetString("ConnectToIpPort", wtbx_port.GetContent());
				menu.p.SetPreferences(preferences);

				// perform login
				menu.StartLogin(null, wtbx_ip.GetContent(), ret.value);
			}
		}
		if (w == wbtn_back)
		{
			OnBackPressed();
		}
	}
}

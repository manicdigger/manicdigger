/// <summary>
/// ScreenConnectToIp shows an input mask for entering server details manually.
/// This enables users to connect to private (unlisted) servers and also allows connections without a valid account.
/// </summary>
public class ScreenConnectToIp : MainMenuScreen
{
	public ScreenConnectToIp()
	{
		wbtn_back = new ButtonWidget();
		AddWidget(wbtn_back);
		wbtn_connect = new ButtonWidget();
		AddWidget(wbtn_connect);
		wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontTitle);
		wtxt_title.SetAlignment(TextAlign.Center);
		AddWidget(wtxt_title);
		wtxt_statusMessage = new TextWidget();
		wtxt_statusMessage.SetFont(fontMessage);
		AddWidget(wtxt_statusMessage);
		wtxt_ip = new TextWidget();
		wtxt_ip.SetFont(fontDefault);
		wtxt_ip.SetAlignment(TextAlign.Right);
		wtxt_ip.SetBaseline(TextBaseline.Middle);
		AddWidget(wtxt_ip);
		wtxt_port = new TextWidget();
		wtxt_port.SetFont(fontDefault);
		wtxt_port.SetAlignment(TextAlign.Right);
		wtxt_port.SetBaseline(TextBaseline.Middle);
		AddWidget(wtxt_port);
		wtbx_ip = new TextBoxWidget();
		AddWidget(wtbx_ip);
		wtbx_port = new TextBoxWidget();
		AddWidget(wtbx_port);

		// tabbing setup
		wtbx_ip.SetNextWidget(wtbx_port);
		wtbx_port.SetNextWidget(wbtn_connect);
		wbtn_connect.SetNextWidget(wbtn_back);
		wbtn_back.SetNextWidget(wtbx_ip);
		wtbx_ip.SetFocused(true);
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
		wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
		wbtn_connect.SetText(menu.lang.Get("MainMenu_ConnectToIpConnect"));
		wtxt_ip.SetText(menu.lang.Get("MainMenu_ConnectToIpIp"));
		wtxt_port.SetText(menu.lang.Get("MainMenu_ConnectToIpPort"));
		wtxt_title.SetText(menu.lang.Get("MainMenu_MultiplayerConnectIP"));
	}

	public override void Render(float dt)
	{
		// load stored values or defaults
		if (!loaded)
		{
			wtbx_ip.SetContent(gamePlatform, gamePlatform.GetPreferences().GetString("ConnectToIpIp", "127.0.0.1"));
			wtbx_port.SetContent(gamePlatform, gamePlatform.GetPreferences().GetString("ConnectToIpPort", "25565"));
			loaded = true;
		}

		float connectAreaWidth = 600;
		float connectAreaHeight = 400;
		float scale = menu.uiRenderer.GetScale();
		float leftx = gamePlatform.GetCanvasWidth() / 2 - (connectAreaWidth / 2) * scale;
		float topy = gamePlatform.GetCanvasHeight() / 2 - (connectAreaHeight / 2) * scale;

		wtxt_title.x = gamePlatform.GetCanvasWidth() / 2;
		wtxt_title.y = topy;
		wtxt_statusMessage.x = leftx;
		wtxt_statusMessage.y = topy + 258 * scale;

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
		wbtn_back.y = gamePlatform.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		DrawWidgets(dt);
	}

	public override void OnBackPressed()
	{
		menu.StartMultiplayer();
	}

	public override void OnButton(AbstractMenuWidget w)
	{
		if (w == wbtn_connect)
		{
			// check input
			IntRef ret = new IntRef();
			if (Game.StringEquals(wtbx_ip.GetContent(), ""))
			{
				wtxt_statusMessage.SetText(menu.lang.Get("MainMenu_ConnectToIpErrorIp"));
			}
			else if (!gamePlatform.IntTryParse(wtbx_port.GetContent(), ret))
			{
				wtxt_statusMessage.SetText(menu.lang.Get("MainMenu_ConnectToIpErrorPort"));
			}
			else
			{
				// save user input
				Preferences preferences = gamePlatform.GetPreferences();
				preferences.SetString("ConnectToIpIp", wtbx_ip.GetContent());
				preferences.SetString("ConnectToIpPort", wtbx_port.GetContent());
				gamePlatform.SetPreferences(preferences);

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

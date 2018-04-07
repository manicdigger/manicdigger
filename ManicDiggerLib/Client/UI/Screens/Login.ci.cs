public class ScreenLogin : Screen
{
	public ScreenLogin()
	{
		wbtn_back = new ButtonWidget();
		wbtn_back.SetText("Back");
		AddWidgetNew(wbtn_back);
		wbtn_login = new ButtonWidget();
		wbtn_login.SetText("Login");
		AddWidgetNew(wbtn_login);
		wbtn_createAccount = new ButtonWidget();
		wbtn_createAccount.SetText("Create Account");
		wbtn_createAccount.SetVisible(false);
		AddWidgetNew(wbtn_createAccount);
		wtbx_username = new TextBoxWidget();
		AddWidgetNew(wtbx_username);
		wtbx_password = new TextBoxWidget();
		wtbx_password.SetInputHidden(true);
		AddWidgetNew(wtbx_password);
		wcbx_rememberPassword = new CheckBoxWidget();
		wcbx_rememberPassword.SetDescription("Remember me");
		AddWidgetNew(wcbx_rememberPassword);
		wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontTitle);
		wtxt_title.SetText("Login");
		AddWidgetNew(wtxt_title);
		wtxt_statusMessage = new TextWidget();
		wtxt_statusMessage.SetFont(fontDefault);
		wtxt_statusMessage.SetAlignment(TextAlign.Center);
		wtxt_statusMessage.SetVisible(false);
		AddWidgetNew(wtxt_statusMessage);
		wtxt_username = new TextWidget();
		wtxt_username.SetFont(fontDefault);
		wtxt_username.SetAlignment(TextAlign.Right);
		wtxt_username.SetBaseline(TextBaseline.Middle);
		wtxt_username.SetText("Username");
		AddWidgetNew(wtxt_username);
		wtxt_password = new TextWidget();
		wtxt_password.SetFont(fontDefault);
		wtxt_password.SetAlignment(TextAlign.Right);
		wtxt_password.SetBaseline(TextBaseline.Middle);
		wtxt_password.SetText("Password");
		AddWidgetNew(wtxt_password);

		fontLoginMessage = new FontCi();
		fontLoginMessage.size = 14;

		title = "Login";

		loginResult = new LoginResultRef();
	}

	ButtonWidget wbtn_back;
	ButtonWidget wbtn_login;
	ButtonWidget wbtn_createAccount;
	TextBoxWidget wtbx_username;
	TextBoxWidget wtbx_password;
	CheckBoxWidget wcbx_rememberPassword;
	TextWidget wtxt_title;
	TextWidget wtxt_statusMessage;
	TextWidget wtxt_username;
	TextWidget wtxt_password;

	FontCi fontLoginMessage;

	bool triedSavedLogin;
	string title;

	public override void LoadTranslations()
	{
		wtxt_title.SetText(menu.lang.Get("MainMenu_Login"));
		wtxt_username.SetText(menu.lang.Get("MainMenu_LoginUsername"));
		wtxt_password.SetText(menu.lang.Get("MainMenu_LoginPassword"));
		wcbx_rememberPassword.SetDescription(menu.lang.Get("MainMenu_LoginRemember"));
		wbtn_login.SetText(menu.lang.Get("MainMenu_Login"));
		wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
		//wbtn_createAccount.SetText(menu.lang.Get("MainMenu_ButtonCreate")); // TODO: Add translation
	}

	public override void Render(float dt)
	{
		if (!triedSavedLogin)
		{
			Preferences preferences = menu.p.GetPreferences();
			wtbx_username.SetContent(menu.p, preferences.GetString("Username", ""));
			string token = preferences.GetString("Password", "");

			loginResultData = new LoginData();
			if (serverHash != null && token != "")
			{
				menu.Login(wtbx_username.GetContent(), wtbx_password.GetContent(), serverHash, token, loginResult, loginResultData);
			}

			triedSavedLogin = true;
		}
		if (loginResultData != null
			&& loginResultData.ServerCorrect
			&& loginResultData.PasswordCorrect)
		{
			if (wcbx_rememberPassword.IsChecked())
			{
				Preferences preferences = menu.p.GetPreferences();
				preferences.SetString("Username", wtbx_username.GetContent());
				if (loginResultData.Token != null && loginResultData.Token != "")
				{
					preferences.SetString("Password", loginResultData.Token);
				}
				menu.p.SetPreferences(preferences);
			}
			menu.ConnectToGame(loginResultData, wtbx_username.GetContent());
		}

		GamePlatform p = menu.p;
		float scale = menu.GetScale();

		menu.DrawBackground();

		float leftx = p.GetCanvasWidth() / 2 - 400 * scale;
		float y = p.GetCanvasHeight() / 2 - 250 * scale;

		string loginResultText = null;
		if (loginResult.value == LoginResult.Failed)
		{
			loginResultText = menu.lang.Get("MainMenu_LoginInvalid");
		}
		if (loginResult.value == LoginResult.Connecting)
		{
			loginResultText = menu.lang.Get("MainMenu_LoginConnecting");
		}
		if (loginResultText != null)
		{
			wtxt_statusMessage.SetText(loginResultText);
		}

		float rightx = p.GetCanvasWidth() / 2 + 150 * scale;

		wtxt_title.x = leftx;
		wtxt_title.y = y - 50 * scale;
		wtxt_statusMessage.x = leftx;
		wtxt_statusMessage.y = y;

		const int textboxHeight = 64;

		float originx = leftx;
		float originy = y + 100;
		wtxt_username.x = originx;
		wtxt_username.y = (originy + textboxHeight / 2) * scale;
		wtbx_username.x = originx;
		wtbx_username.y = originy * scale;
		wtbx_username.sizex = 256 * scale;
		wtbx_username.sizey = textboxHeight * scale;

		originy = y + 200;
		wtxt_password.x = originx;
		wtxt_password.y = (originy + textboxHeight / 2) * scale;
		wtbx_password.x = originx;
		wtbx_password.y = originy * scale;
		wtbx_password.sizex = 256 * scale;
		wtbx_password.sizey = textboxHeight * scale;

		wcbx_rememberPassword.x = leftx;
		wcbx_rememberPassword.y = y + 300 * scale;
		wcbx_rememberPassword.sizex = 256 * scale;
		wcbx_rememberPassword.sizey = 32 * scale;

		wbtn_back.x = 40 * scale;
		wbtn_back.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		wbtn_login.x = leftx;
		wbtn_login.y = y + 400 * scale;
		wbtn_login.sizex = 256 * scale;
		wbtn_login.sizey = 64 * scale;

		wbtn_createAccount.x = rightx;
		wbtn_createAccount.y = y + 400 * scale;
		wbtn_createAccount.sizex = 256 * scale;
		wbtn_createAccount.sizey = 64 * scale;

		DrawWidgets();
	}

	public override void OnBackPressed()
	{
		menu.StartMultiplayer();
	}

	LoginResultRef loginResult;
	LoginData loginResultData;

	public override void OnButtonA(AbstractMenuWidget w)
	{
		if (w == wbtn_login)
		{
			loginResultData = new LoginData();
			if (serverHash != null)
			{
				// Connect to server hash, through main game menu. Do login.
				menu.Login(wtbx_username.GetContent(), wtbx_password.GetContent(), serverHash, "", loginResult, loginResultData);
			}
			else
			{
				// Connect to IP. Don't login

				// Save username
				if (wcbx_rememberPassword.IsChecked())
				{
					Preferences preferences = menu.p.GetPreferences();
					preferences.SetString("Username", wtbx_username.GetContent());
					menu.p.SetPreferences(preferences);
				}

				ConnectData connectdata = new ConnectData();
				connectdata.Ip = serverIp;
				connectdata.Port = serverPort;
				connectdata.Username = wtbx_username.GetContent();
				menu.StartGame(false, null, connectdata);
			}
		}
		if (w == wbtn_createAccount)
		{
			menu.CreateAccount(wtbx_username.GetContent(), wtbx_password.GetContent(), loginResult);
		}
		if (w == wbtn_back)
		{
			OnBackPressed();
		}
	}
	internal string serverHash;
	internal string serverIp;
	internal int serverPort;
}

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
		wtxt_title.SetAlignment(TextAlign.Center);
		wtxt_title.SetText("Login");
		AddWidgetNew(wtxt_title);
		wtxt_statusMessage = new TextWidget();
		wtxt_statusMessage.SetFont(fontMessage);
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

		triedSavedLogin = false;
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

	bool triedSavedLogin;

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
		// first try logging in using stored credentials
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
		// store credentials and connect to server when login was successful
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

		// update login status message
		string loginResultText = null;
		switch (loginResult.value)
		{
			case LoginResult.Connecting:
				loginResultText = menu.lang.Get("MainMenu_LoginConnecting");
				break;
			case LoginResult.Failed:
				loginResultText = menu.lang.Get("MainMenu_LoginInvalid");
				break;
			case LoginResult.Ok:
				loginResultText = "Login successful!";
				break;
			default:
				break;
		}
		wtxt_statusMessage.SetText(loginResultText);

		GamePlatform p = menu.p;
		float scale = menu.GetScale();

		float loginAreaWidth = 600;
		float loginAreaHeight = 400;
		const int textboxHeight = 64;

		float leftx = p.GetCanvasWidth() / 2 - (loginAreaWidth / 2) * scale;
		float rightx = p.GetCanvasWidth() / 2 + 44 * scale;
		float topy = p.GetCanvasHeight() / 2 - (loginAreaHeight / 2) * scale;

		wtxt_title.x = p.GetCanvasWidth() / 2;
		wtxt_title.y = topy;
		wtxt_statusMessage.x = leftx;
		wtxt_statusMessage.y = topy + 258;

		float originy = topy + 50;
		wtxt_username.x = leftx - 6 * scale;
		wtxt_username.y = (originy + textboxHeight / 2) * scale;
		wtbx_username.x = leftx;
		wtbx_username.y = originy * scale;
		wtbx_username.sizex = loginAreaWidth * scale;
		wtbx_username.sizey = textboxHeight * scale;

		originy = topy + 130;
		wtxt_password.x = leftx - 6 * scale;
		wtxt_password.y = (originy + textboxHeight / 2) * scale;
		wtbx_password.x = leftx;
		wtbx_password.y = originy * scale;
		wtbx_password.sizex = loginAreaWidth * scale;
		wtbx_password.sizey = textboxHeight * scale;

		wcbx_rememberPassword.x = leftx;
		wcbx_rememberPassword.y = topy + 210 * scale;
		wcbx_rememberPassword.sizex = loginAreaWidth * scale;
		wcbx_rememberPassword.sizey = 32 * scale;

		wbtn_login.x = leftx;
		wbtn_login.y = topy + 336 * scale;
		wbtn_login.sizex = 256 * scale;
		wbtn_login.sizey = 64 * scale;

		wbtn_createAccount.x = rightx;
		wbtn_createAccount.y = topy + 336 * scale;
		wbtn_createAccount.sizex = 256 * scale;
		wbtn_createAccount.sizey = 64 * scale;

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

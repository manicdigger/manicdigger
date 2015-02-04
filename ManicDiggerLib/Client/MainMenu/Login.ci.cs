public class ScreenLogin : Screen
{
    public ScreenLogin()
    {
        login = new MenuWidget();
        login.text = "Login";
        login.type = WidgetType.Button;
        login.nextWidget = 9;
        loginUsername = new MenuWidget();
        loginUsername.type = WidgetType.Textbox;
        loginUsername.text = "";
        loginUsername.description = "Username";
        loginUsername.nextWidget = 2;
        loginPassword = new MenuWidget();
        loginPassword.type = WidgetType.Textbox;
        loginPassword.text = "";
        loginPassword.description = "Password";
        loginPassword.password = true;
        loginPassword.nextWidget = 3;
        loginRememberMe = new MenuWidget();
        loginRememberMe.text = "Yes";
        loginRememberMe.type = WidgetType.Button;
        loginRememberMe.description = "Remember me";
        loginRememberMe.nextWidget = 0;

        createAccount = new MenuWidget();
        createAccount.text = "Create account";
        createAccount.type = WidgetType.Button;
        createAccountUsername = new MenuWidget();
        createAccountUsername.text = "";
        createAccountUsername.type = WidgetType.Textbox;
        createAccountUsername.description = "Username";
        createAccountPassword = new MenuWidget();
        createAccountPassword.text = "";
        createAccountPassword.type = WidgetType.Textbox;
        createAccountPassword.description = "Password";
        createAccountPassword.password = true;
        createAccountRememberMe = new MenuWidget();
        createAccountRememberMe.text = "Yes";
        createAccountRememberMe.type = WidgetType.Button;
        createAccountRememberMe.description = "Remember me";
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;
        back.nextWidget = 1;

        title = "Login";

        widgets[0] = login;
        widgets[1] = loginUsername;
        widgets[2] = loginPassword;
        widgets[3] = loginRememberMe;
        widgets[4] = createAccount;
        widgets[5] = createAccountUsername;
        widgets[6] = createAccountPassword;
        widgets[7] = createAccountRememberMe;
        widgets[9] = back;

        loginUsername.GetFocus();

        loginResult = new LoginResultRef();
    }

    MenuWidget login;
    MenuWidget loginUsername;
    MenuWidget loginPassword;
    MenuWidget loginRememberMe;

    MenuWidget createAccount;
    MenuWidget createAccountUsername;
    MenuWidget createAccountPassword;
    MenuWidget createAccountRememberMe;

    MenuWidget back;

    bool triedSavedLogin;
    string title;

    public override void LoadTranslations()
    {
        login.text = menu.lang.Get("MainMenu_Login");
        loginUsername.description = menu.lang.Get("MainMenu_LoginUsername");
        loginPassword.description = menu.lang.Get("MainMenu_LoginPassword");
        loginRememberMe.text = menu.lang.Get("MainMenu_ChoiceYes");
        loginRememberMe.description = menu.lang.Get("MainMenu_LoginRemember");
        back.text = menu.lang.Get("MainMenu_ButtonBack");
        title = menu.lang.Get("MainMenu_Login");
    }

    public override void Render(float dt)
    {
        if (!triedSavedLogin)
        {
            Preferences preferences = menu.p.GetPreferences();
            loginUsername.text = preferences.GetString("Username", "");
            loginPassword.text = "";
            string token = preferences.GetString("Password", "");

            loginResultData = new LoginData();
            if (serverHash != null && token != "")
            {
                menu.Login(loginUsername.text, loginPassword.text, serverHash, token, loginResult, loginResultData);
            }

            triedSavedLogin = true;
        }
        if (loginResultData != null
            && loginResultData.ServerCorrect
            && loginResultData.PasswordCorrect)
        {
            if (loginRememberMe.text == menu.lang.Get("MainMenu_ChoiceYes"))
            {
                Preferences preferences = menu.p.GetPreferences();
                preferences.SetString("Username", loginUsername.text);
                if (loginResultData.Token != null && loginResultData.Token != "")
                {
                    preferences.SetString("Password", loginResultData.Token);
                }
                menu.p.SetPreferences(preferences);
            }
            menu.ConnectToGame(loginResultData, loginUsername.text);
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
            menu.DrawText(loginResultText, 14 * scale, leftx, y - 50 * scale, TextAlign.Left, TextBaseline.Top);
        }

        menu.DrawText(title, 14 * scale, leftx, y + 50 * scale, TextAlign.Left, TextBaseline.Top);

        loginUsername.x = leftx;
        loginUsername.y = y + 100 * scale;
        loginUsername.sizex = 256 * scale;
        loginUsername.sizey = 64 * scale;
        loginUsername.fontSize = 14 * scale;

        loginPassword.x = leftx;
        loginPassword.y = y + 200 * scale;
        loginPassword.sizex = 256 * scale;
        loginPassword.sizey = 64 * scale;
        loginPassword.fontSize = 14 * scale;

        loginRememberMe.x = leftx;
        loginRememberMe.y = y + 300 * scale;
        loginRememberMe.sizex = 256 * scale;
        loginRememberMe.sizey = 64 * scale;
        loginRememberMe.fontSize = 14 * scale;

        login.x = leftx;
        login.y = y + 400 * scale;
        login.sizex = 256 * scale;
        login.sizey = 64 * scale;
        login.fontSize = 14 * scale;

        float rightx = p.GetCanvasWidth() / 2 + 150 * scale;

        // menu.DrawText("Create account", 14 * scale, rightx, y + 50 * scale, TextAlign.Left, TextBaseline.Top);

        createAccountUsername.x = rightx;
        createAccountUsername.y = y + 100 * scale;
        createAccountUsername.sizex = 256 * scale;
        createAccountUsername.sizey = 64 * scale;
        createAccountUsername.fontSize = 14 * scale;

        createAccountPassword.x = rightx;
        createAccountPassword.y = y + 200 * scale;
        createAccountPassword.sizex = 256 * scale;
        createAccountPassword.sizey = 64 * scale;
        createAccountPassword.fontSize = 14 * scale;

        createAccountRememberMe.x = rightx;
        createAccountRememberMe.y = y + 300 * scale;
        createAccountRememberMe.sizex = 256 * scale;
        createAccountRememberMe.sizey = 64 * scale;
        createAccountRememberMe.fontSize = 14 * scale;

        createAccount.x = rightx;
        createAccount.y = y + 400 * scale;
        createAccount.sizex = 256 * scale;
        createAccount.sizey = 64 * scale;
        createAccount.fontSize = 14 * scale;

        createAccountUsername.visible = false;
        createAccountPassword.visible = false;
        createAccountRememberMe.visible = false;
        createAccount.visible = false;

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;

        DrawWidgets();
    }

    public override void OnBackPressed()
    {
        menu.StartMultiplayer();
    }

    LoginResultRef loginResult;
    LoginData loginResultData;

    public override void OnButton(MenuWidget w)
    {
        if (w == login)
        {
            loginResultData = new LoginData();
            if (serverHash != null)
            {
                // Connect to server hash, through main game menu. Do login.
                menu.Login(loginUsername.text, loginPassword.text, serverHash, "", loginResult, loginResultData);
            }
            else
            {
                // Connect to IP. Don't login

                // Save username
                if (loginRememberMe.text == menu.lang.Get("MainMenu_ChoiceYes"))
                {
                    Preferences preferences = menu.p.GetPreferences();
                    preferences.SetString("Username", loginUsername.text);
                    menu.p.SetPreferences(preferences);
                }

                ConnectData connectdata = new ConnectData();
                connectdata.Ip = serverIp;
                connectdata.Port = serverPort;
                connectdata.Username = loginUsername.text;
                menu.StartGame(false, null, connectdata);
            }
        }
        if (w == createAccount)
        {
            menu.CreateAccount(createAccountUsername.text, createAccountPassword.text, loginResult);
        }
        if (w == loginRememberMe || w == createAccountRememberMe)
        {
            if (w.text == menu.lang.Get("MainMenu_ChoiceYes"))
            {
                w.text = menu.lang.Get("MainMenu_ChoiceNo");
            }
            else
            {
                w.text = menu.lang.Get("MainMenu_ChoiceYes");
            }
        }
        if (w == back)
        {
            OnBackPressed();
        }
    }
    internal string serverHash;
    internal string serverIp;
    internal int serverPort;
}

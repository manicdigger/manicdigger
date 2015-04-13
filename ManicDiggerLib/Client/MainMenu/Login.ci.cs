public class ScreenLogin : Screen
{
    public ScreenLogin()
    {
        // Create buttons
        login = new MenuWidget(); // Login
        login.text = "Login";
        login.type = WidgetType.Button;
        login.nextWidget = 9;

        loginUsername = new MenuWidget(); // Login Username
        loginUsername.type = WidgetType.Textbox;
        loginUsername.text = "";
        loginUsername.description = "Username";
        loginUsername.nextWidget = 2;

        loginPassword = new MenuWidget(); // Login Password
        loginPassword.type = WidgetType.Textbox;
        loginPassword.text = "";
        loginPassword.description = "Password";
        loginPassword.password = true;
        loginPassword.nextWidget = 3;

        loginRememberMe = new MenuWidget(); // Login Remember Me
        loginRememberMe.text = "Yes";
        loginRememberMe.type = WidgetType.Button;
        loginRememberMe.description = "Remember me";
        loginRememberMe.nextWidget = 0;

        createAccount = new MenuWidget(); // Create Account
        createAccount.text = "Create account";
        createAccount.type = WidgetType.Button;

        createAccountUsername = new MenuWidget(); // Create Account Username
        createAccountUsername.text = "";
        createAccountUsername.type = WidgetType.Textbox;
        createAccountUsername.description = "Username";

        createAccountPassword = new MenuWidget(); // Create Account Password
        createAccountPassword.text = "";
        createAccountPassword.type = WidgetType.Textbox;
        createAccountPassword.description = "Password";
        createAccountPassword.password = true;

        createAccountRememberMe = new MenuWidget(); // Create Account Remember Me
        createAccountRememberMe.text = "Yes";
        createAccountRememberMe.type = WidgetType.Button;
        createAccountRememberMe.description = "Remember me";

        back = new MenuWidget(); // Back
        back.text = "Back";
        back.type = WidgetType.Button;
        back.nextWidget = 1;

        // Add buttons to widget collection
        widgets[0] = login;
        widgets[1] = loginUsername;
        widgets[2] = loginPassword;
        widgets[3] = loginRememberMe;
        widgets[4] = createAccount;
        widgets[5] = createAccountUsername;
        widgets[6] = createAccountPassword;
        widgets[7] = createAccountRememberMe;
        widgets[9] = back;

        // Set screen title
        title = "Login";

        // Focus login username
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
    string title; // Screen title

    int oldWidth; // CanvasWidth from last rendering (frame)
    int oldHeight; // CanvasHeight from last rendering (frame)

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
        // Try logging in with current details (if already logged in)
        if (!triedSavedLogin)
        {
            // Load saved account details
            Preferences preferences = menu.p.GetPreferences();
            loginUsername.text = preferences.GetString("Username", "");
            loginPassword.text = "";
            string token = preferences.GetString("Password", "");

            loginResultData = new LoginData();
            if (token != "") // If there is a "saved password"
            {
                if (connect) // Connect to server
                {
                    if (serverHash != null) // Connect through master server
                    {
                        menu.Login(loginUsername.text, loginPassword.text, serverHash, token, loginResult, loginResultData);
                    }
                    else // Connect to IP
                    {
                        // Connect to IP
                    }
                }
                else // Login only
                {
                    menu.Login(loginUsername.text, loginPassword.text, null, token, loginResult, loginResultData);
                }
            }

            triedSavedLogin = true;
        }

        // If login succeeded
        if (loginResultData != null
            && loginResultData.PasswordCorrect)
        {
            // Remember account details (if "remember me" is checked or user only logged in)
            if (!connect || loginRememberMe.text == menu.lang.Get("MainMenu_ChoiceYes"))
            {
                Preferences preferences = menu.p.GetPreferences();
                preferences.SetString("Username", loginUsername.text);
                if (loginResultData.Token != null && loginResultData.Token != "")
                {
                    preferences.SetString("Password", loginResultData.Token);
                }
                menu.p.SetPreferences(preferences);
            }

            if (connect) // Try connecting to server
            {
                // Connect to server
                if (loginResultData.ServerCorrect)
                {
                    menu.ConnectToGame(loginResultData, loginUsername.text);
                }
                else
                {
                    // SERVER INVALID (error message?)
                }
            }
            else // Login successful
            {
                // Go back (to multiplayer)
                OnBackPressed();
            }
        }

        // Login result
        string loginResultText = null;
        if (loginResult.value == LoginResult.Failed)
        {
            loginResultText = menu.lang.Get("MainMenu_LoginInvalid");
        }
        else if (loginResult.value == LoginResult.Connecting)
        {
            loginResultText = menu.lang.Get("MainMenu_LoginConnecting");
        }

        GamePlatform p = menu.p;

        // Screen measurements
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();
        float rightx = p.GetCanvasWidth() / 2 + 150 * scale;
        float leftx = p.GetCanvasWidth() / 2f - 400f * scale;
        float y = p.GetCanvasHeight() / 2f - 250f * scale;

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            loginUsername.x = leftx; // Login Username
            loginUsername.y = y + 100f * scale;
            loginUsername.sizex = 256f * scale;
            loginUsername.sizey = 64f * scale;
            loginUsername.fontSize = 14f * scale;

            loginPassword.x = leftx; // Login Password
            loginPassword.y = y + 200f * scale;
            loginPassword.sizex = 256f * scale;
            loginPassword.sizey = 64f * scale;
            loginPassword.fontSize = 14f * scale;

            loginRememberMe.x = leftx; // Login Remember me
            loginRememberMe.y = y + 300f * scale;
            loginRememberMe.sizex = 256f * scale;
            loginRememberMe.sizey = 64f * scale;
            loginRememberMe.fontSize = 14f * scale;

            login.x = leftx; // Login
            login.y = y + 400f * scale;
            login.sizex = 256f * scale;
            login.sizey = 64f * scale;
            login.fontSize = 14f * scale;

            // menu.DrawText("Create account", 14 * scale, rightx, y + 50 * scale, TextAlign.Left, TextBaseline.Top);

            createAccountUsername.x = rightx; // Create Account Username
            createAccountUsername.y = y + 100f * scale;
            createAccountUsername.sizex = 256f * scale;
            createAccountUsername.sizey = 64f * scale;
            createAccountUsername.fontSize = 14f * scale;

            createAccountPassword.x = rightx; // Create Account Password
            createAccountPassword.y = y + 200f * scale;
            createAccountPassword.sizex = 256f * scale;
            createAccountPassword.sizey = 64f * scale;
            createAccountPassword.fontSize = 14f * scale;

            createAccountRememberMe.x = rightx; // Create Account Remember Me
            createAccountRememberMe.y = y + 300f * scale;
            createAccountRememberMe.sizex = 256f * scale;
            createAccountRememberMe.sizey = 64f * scale;
            createAccountRememberMe.fontSize = 14f * scale;

            createAccount.x = rightx; // Create Account
            createAccount.y = y + 400f * scale;
            createAccount.sizex = 256f * scale;
            createAccount.sizey = 64f * scale;
            createAccount.fontSize = 14f * scale;

            createAccountUsername.visible = false; // Hide create account widgets
            createAccountPassword.visible = false;
            createAccountRememberMe.visible = false;
            createAccount.visible = false;

            back.x = 40f * scale; // Back
            back.y = p.GetCanvasHeight() - 104f * scale;
            back.sizex = 256f * scale;
            back.sizey = 64f * scale;
            back.fontSize = 14f * scale;
        }

        // Draw background
        menu.DrawBackground();

        // Draw login result
        if (loginResultText != null)
        {
            menu.DrawText(loginResultText, 14f * scale, leftx, y - 50f * scale, TextAlign.Left, TextBaseline.Top);
        }

        // Draw screen title
        menu.DrawText(title, 14f * scale, leftx, y + 50f * scale, TextAlign.Left, TextBaseline.Top);

        // Draw widgets
        DrawWidgets();

        // Update old(Width/Height)
        oldWidth = width;
        oldHeight = height;
    }

    public override void OnBackPressed()
    {
        menu.StartMultiplayer();
    }

    LoginResultRef loginResult;
    LoginData loginResultData;

    public override void OnButton(MenuWidget w)
    {
        if (w == login) // Login
        {
            loginResultData = new LoginData();

            if (connect) // Connect to server
            {
                if (serverHash != null) // Connect with hash. Login
                {
                    // Connect to server hash, through main game menu. Do login.
                    menu.Login(loginUsername.text, loginPassword.text, serverHash, "", loginResult, loginResultData);
                }
                else // Connect to IP. Don't login
                {
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
            else // Login only
            {
                menu.Login(loginUsername.text, loginPassword.text, null, "", loginResult, loginResultData);
            }
        }
        else if (w == createAccount) // Create account
        {
            menu.CreateAccount(createAccountUsername.text, createAccountPassword.text, loginResult); // Create new account (does not work yet)
        }
        else if (w == loginRememberMe || w == createAccountRememberMe) // Remember me
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
        else if (w == back) // Back
        {
            OnBackPressed();
        }
    }

    internal string serverHash;
    internal string serverIp;
    internal int serverPort;
    internal bool connect; // Whether or not user wants to connect to a server (false = just log in)
}

public class ScreenConnectToIp : Screen
{
    public ScreenConnectToIp()
    {
        // Create buttons
        buttonConnect = new MenuWidget(); // Connect
        buttonConnect.text = "Connect";
        buttonConnect.type = WidgetType.Button;
        buttonConnect.nextWidget = 3;

        textboxIp = new MenuWidget(); // IP
        textboxIp.type = WidgetType.Textbox;
        textboxIp.text = "";
        textboxIp.description = "Ip";
        textboxIp.nextWidget = 2;

        textboxPort = new MenuWidget(); // Port
        textboxPort.type = WidgetType.Textbox;
        textboxPort.text = "";
        textboxPort.description = "Port";
        textboxPort.nextWidget = 0;

        back = new MenuWidget(); // Back
        back.text = "Back";
        back.type = WidgetType.Button;
        back.nextWidget = 1;

        // Add buttons to widget collection
        widgets[0] = buttonConnect;
        widgets[1] = textboxIp;
        widgets[2] = textboxPort;
        widgets[3] = back;

        // Set screen title
        title = "Connect to IP";

        // Focus IP textbox
        textboxIp.GetFocus();
    }

    MenuWidget buttonConnect;
    MenuWidget textboxIp;
    MenuWidget textboxPort;

    MenuWidget back;

    bool loaded;
    string title; // Screen title

    int oldWidth; // CanvasWidth from last rendering (frame)
    int oldHeight; // CanvasHeight from last rendering (frame)

    public override void LoadTranslations()
    {
        buttonConnect.text = menu.lang.Get("MainMenu_ConnectToIpConnect");
        textboxIp.description = menu.lang.Get("MainMenu_ConnectToIpIp");
        textboxPort.description = menu.lang.Get("MainMenu_ConnectToIpPort");
        title = menu.lang.Get("MainMenu_MultiplayerConnectIP");
    }

    string preferences_ip;
    string preferences_port;
    public override void Render(float dt)
    {
        if (!loaded)
        {
            preferences_ip = menu.p.GetPreferences().GetString("ConnectToIpIp", "127.0.0.1");
            preferences_port = menu.p.GetPreferences().GetString("ConnectToIpPort", "25565");
            textboxIp.text = preferences_ip;
            textboxPort.text = preferences_port;
            loaded = true;
        }

        if (textboxIp.text != preferences_ip
            || textboxPort.text != preferences_port)
        {
            preferences_ip = textboxIp.text;
            preferences_port = textboxPort.text;
            Preferences preferences = menu.p.GetPreferences();
            preferences.SetString("ConnectToIpIp", preferences_ip);
            preferences.SetString("ConnectToIpPort", preferences_port);
            menu.p.SetPreferences(preferences);
        }

        GamePlatform p = menu.p;

        // Screen measurements
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();
        float leftx = width / 2f - 400f * scale;
        float y = height / 2f - 250f * scale;

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            // IP
            textboxIp.x = leftx;
            textboxIp.y = y + 100f * scale;
            textboxIp.sizex = 256f * scale;
            textboxIp.sizey = 64f * scale;
            textboxIp.fontSize = 14f * scale;

            // Port
            textboxPort.x = leftx;
            textboxPort.y = y + 200f * scale;
            textboxPort.sizex = 256f * scale;
            textboxPort.sizey = 64f * scale;
            textboxPort.fontSize = 14f * scale;
            
            // Connect
            buttonConnect.x = leftx;
            buttonConnect.y = y + 400f * scale;
            buttonConnect.sizex = 256f * scale;
            buttonConnect.sizey = 64f * scale;
            buttonConnect.fontSize = 14f * scale;

            // Back
            back.x = 40f * scale;
            back.y = p.GetCanvasHeight() - 104f * scale;
            back.sizex = 256f * scale;
            back.sizey = 64f * scale;
            back.fontSize = 14f * scale;
        }

        // Draw background
        menu.DrawBackground();

        // Draw login result
        string loginResultText = null;
        if (errorText != null)
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

    string errorText;

    public override void OnBackPressed()
    {
        menu.StartMultiplayer();
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == buttonConnect) // Connect
        {
            FloatRef ret = new FloatRef();
            if (!Game.StringEquals(textboxIp.text, "")
                && menu.p.FloatTryParse(textboxPort.text, ret))
            {
                menu.StartLogin(null, textboxIp.text, menu.p.IntParse(textboxPort.text)); // Connect to server
            }
        }
        else if (w == back) // Back
        {
            OnBackPressed();
        }
    }
}

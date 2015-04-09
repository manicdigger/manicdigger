public class ScreenMain : Screen
{
    public ScreenMain()
    {
        // Create buttons
        singleplayer = new MenuWidget(); // Singleplayer

        multiplayer = new MenuWidget(); // Multiplayer

        exit = new MenuWidget(); // Exit

        // Add buttons to widget collection
        widgets[0] = singleplayer;
        widgets[1] = multiplayer;
        widgets[2] = exit;

        queryStringChecked = false;
    }

    MenuWidget singleplayer;
    MenuWidget multiplayer;
    MenuWidget exit;
    bool queryStringChecked;

    int oldWidth; // CanvasWidth from last rendering (frame)
    int oldHeight; // CanvasHeight from last rendering (frame)

    public override void Render(float dt)
    {
        GamePlatform p = menu.p;

        exit.visible = menu.p.ExitAvailable(); // Check if exit button should be visable
        UseQueryStringIpAndPort(menu);

        // Screen measurements
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();
        float leftx = width / 2f - 128f * scale;
        float y = height / 2f + 0f * scale;

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            // Button 
            float buttonheight = 64f;
            float buttonwidth = 256f;
            float spacebetween = 5f;
            float offsetfromborder = 50f;

            // Singleplayer
            singleplayer.text = menu.lang.Get("MainMenu_Singleplayer");
            singleplayer.x = width / 2f - (buttonwidth / 2f) * scale;
            singleplayer.y = height - (3f * (buttonheight * scale + spacebetween)) - offsetfromborder * scale;
            singleplayer.sizex = buttonwidth * scale;
            singleplayer.sizey = buttonheight * scale;
            singleplayer.fontSize = 14f * scale;

            // Multiplayer
            multiplayer.text = menu.lang.Get("MainMenu_Multiplayer");
            multiplayer.x = width / 2f - (buttonwidth / 2f) * scale;
            multiplayer.y = height - (2f * (buttonheight * scale + spacebetween)) - offsetfromborder * scale;
            multiplayer.sizex = buttonwidth * scale;
            multiplayer.sizey = buttonheight * scale;
            multiplayer.fontSize = 14f * scale;

            // Exit
            exit.text = menu.lang.Get("MainMenu_Quit");
            exit.x = width / 2f - (buttonwidth / 2f) * scale;
            exit.y = height - (1f * (buttonheight * scale + spacebetween)) - offsetfromborder * scale;
            exit.sizex = buttonwidth * scale;
            exit.sizey = buttonheight * scale;
            exit.fontSize = 14f * scale;
        }

        // Draw loading bar (some platforms only)
        if (menu.assetsLoadProgress.value != 1f)
        {
            string s = menu.p.StringFormat(menu.lang.Get("MainMenu_AssetsLoadProgress"), menu.p.FloatToString(menu.p.FloatToInt(menu.assetsLoadProgress.value * 100f)));
            menu.DrawText(s, 20f * scale, width / 2f, height / 2f, TextAlign.Center, TextBaseline.Middle);
            return;
        }

        // Draw background
        menu.DrawBackground();

        // Draw logo
        float logoHeight = 512f * scale;
        menu.Draw2dQuad(menu.GetTexture("logo.png"), width / 2f - 1024f * scale / 2f, (height / 8f) * 3f - logoHeight / 2f, 1024f * scale, logoHeight);

        // Draw widgets
        DrawWidgets();

        // Update old(Width/Height)
        oldWidth = width;
        oldHeight = height;
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
            menu.StartLogin(null, ip, portInt, true);
        }
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == singleplayer) // Singleplayer
        {
            menu.StartSingleplayer();
        }
        else if (w == multiplayer) // Multiplayer
        {
            menu.StartMultiplayer();
        }
        else if (w == exit) // Exit
        {
            menu.Exit();
        }
    }

    public override void OnBackPressed()
    {
        menu.Exit(); // Exit game
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

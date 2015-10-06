public class ModGuiEscapeMenu : ClientMod
{
    public ModGuiEscapeMenu()
    {
        one = 1;
        fonts = new string[4];
        fonts[0] = "Nice";
        fonts[1] = "Simple";
        fonts[2] = "BlackBackground";
        fonts[3] = "Default";
        fontsLength = 4;
        fontValues = new int[4];
        fontValues[0] = 0;
        fontValues[1] = 1;
        fontValues[2] = 2;
        fontValues[3] = 3;
        widgets = new Button[1024];
        keyselectid = -1;
    }
    float one;
    Button buttonMainReturnToGame;
    Button buttonMainOptions;
    Button buttonMainExit;

    int widgetsCount;
    void MainSet()
    {
        Language language = game.language;
        buttonMainReturnToGame = new Button();
        buttonMainReturnToGame.Text = language.ReturnToGame();
        buttonMainOptions = new Button();
        buttonMainOptions.Text = language.Options();
        buttonMainExit = new Button();
        buttonMainExit.Text = language.Exit();

        WidgetsClear();
        AddWidget(buttonMainReturnToGame);
        AddWidget(buttonMainOptions);
        AddWidget(buttonMainExit);
    }

    void MainHandleClick(Button b)
    {
        if (b == buttonMainReturnToGame)
        {
            game.GuiStateBackToGame();
        }
        if (b == buttonMainOptions)
        {
            SetEscapeMenuState(EscapeMenuState.Options);
        }
        if (b == buttonMainExit)
        {
            game.SendLeave(Packet_LeaveReasonEnum.Leave);
            game.ExitToMainMenu_();
        }
    }

    Button optionsGraphics;
    Button optionsKeys;
    Button optionsOther;
    Button optionsReturnToMainMenu;
    void OptionsSet()
    {
        Language language = game.language;
        optionsGraphics = new Button();
        optionsGraphics.Text = language.Graphics();
        optionsKeys = new Button();
        optionsKeys.Text = language.Keys();
        optionsOther = new Button();
        optionsOther.Text = language.Other();
        optionsReturnToMainMenu = new Button();
        optionsReturnToMainMenu.Text = language.ReturnToMainMenu();

        WidgetsClear();
        AddWidget(optionsGraphics);
        AddWidget(optionsKeys);
        AddWidget(optionsOther);
        AddWidget(optionsReturnToMainMenu);
    }

    void OptionsHandleClick(Button b)
    {
        if (b == optionsGraphics)
        {
            SetEscapeMenuState(EscapeMenuState.Graphics);
        }
        if (b == optionsKeys)
        {
            SetEscapeMenuState(EscapeMenuState.Keys);
        }
        if (b == optionsOther)
        {
            SetEscapeMenuState(EscapeMenuState.Other);
        }
        if (b == optionsReturnToMainMenu)
        {
            SaveOptions(); SetEscapeMenuState(EscapeMenuState.Main);
        }
    }

    Button graphicsOptionSmoothShadows;
    Button graphicsOptionDarkenSides;
    Button graphicsViewDistanceOption;
    Button graphicsOptionFramerate;
    Button graphicsOptionResolution;
    Button graphicsOptionFullscreen;
    Button graphicsUseServerTexturesOption;
    Button graphicsFontOption;
    Button graphicsReturnToOptionsMenu;
    void GraphicsSet()
    {
        OptionsCi options = game.options;
        Language language = game.language;
        graphicsOptionSmoothShadows = new Button();
        graphicsOptionSmoothShadows.Text = game.platform.StringFormat(language.OptionSmoothShadows(), options.Smoothshadows ? language.On() : language.Off());
        graphicsOptionDarkenSides = new Button();
        graphicsOptionDarkenSides.Text = game.platform.StringFormat(language.Get("OptionDarkenSides"), options.EnableBlockShadow ? language.On() : language.Off());
        graphicsViewDistanceOption = new Button();
        graphicsViewDistanceOption.Text = game.platform.StringFormat(language.ViewDistanceOption(), game.platform.IntToString(game.platform.FloatToInt(game.d_Config3d.viewdistance)));
        graphicsOptionFramerate = new Button();
        graphicsOptionFramerate.Text = game.platform.StringFormat(language.OptionFramerate(), (VsyncString()));
        graphicsOptionResolution = new Button();
        graphicsOptionResolution.Text = game.platform.StringFormat(language.OptionResolution(), (ResolutionString()));
        graphicsOptionFullscreen = new Button();
        graphicsOptionFullscreen.Text = game.platform.StringFormat(language.OptionFullscreen(), options.Fullscreen ? language.On() : language.Off());
        graphicsUseServerTexturesOption = new Button();
        graphicsUseServerTexturesOption.Text = game.platform.StringFormat(language.UseServerTexturesOption(), (options.UseServerTextures ? language.On() : language.Off()));
        graphicsFontOption = new Button();
        graphicsFontOption.Text = game.platform.StringFormat(language.FontOption(), (FontString()));
        graphicsReturnToOptionsMenu = new Button();
        graphicsReturnToOptionsMenu.Text = language.ReturnToOptionsMenu();

        WidgetsClear();
        AddWidget(graphicsOptionSmoothShadows);
        AddWidget(graphicsOptionDarkenSides);
        AddWidget(graphicsViewDistanceOption);
        AddWidget(graphicsOptionFramerate);
        AddWidget(graphicsOptionResolution);
        AddWidget(graphicsOptionFullscreen);
        AddWidget(graphicsUseServerTexturesOption);
        AddWidget(graphicsFontOption);
        AddWidget(graphicsReturnToOptionsMenu);
    }
    void GraphicsHandleClick(Button b)
    {
        OptionsCi options = game.options;
        if (b == graphicsOptionSmoothShadows)
        {
            options.Smoothshadows = !options.Smoothshadows;
            game.d_TerrainChunkTesselator.EnableSmoothLight = options.Smoothshadows;
            if (options.Smoothshadows)
            {
                options.BlockShadowSave = one * 7 / 10;
                game.d_TerrainChunkTesselator.BlockShadow = options.BlockShadowSave;
            }
            else
            {
                options.BlockShadowSave = one * 6 / 10;
                game.d_TerrainChunkTesselator.BlockShadow = options.BlockShadowSave;
            }
            game.RedrawAllBlocks();
        }
        if (b == graphicsOptionDarkenSides)
        {
            options.EnableBlockShadow = !options.EnableBlockShadow;
            game.d_TerrainChunkTesselator.option_DarkenBlockSides = options.EnableBlockShadow;
            game.RedrawAllBlocks();
        }
        if (b == graphicsViewDistanceOption)
        {
            game.ToggleFog();
        }
        if (b == graphicsOptionFramerate)
        {
            game.ToggleVsync();
        }
        if (b == graphicsOptionResolution)
        {
            ToggleResolution();
        }
        if (b == graphicsOptionFullscreen)
        {
            options.Fullscreen = !options.Fullscreen;
        }
        if (b == graphicsUseServerTexturesOption)
        {
            options.UseServerTextures = !options.UseServerTextures;
        }
        if (b == graphicsFontOption)
        {
            ToggleFont();
        }
        if (b == graphicsReturnToOptionsMenu)
        {
            UseFullscreen(); UseResolution(); SetEscapeMenuState(EscapeMenuState.Options);
        }
    }

    Button otherSoundOption;
    Button otherReturnToOptionsMenu;
    Button otherAutoJumpOption;
    Button otherLanguageSetting;
    void OtherSet()
    {
        Language language = game.language;

        otherSoundOption = new Button();
        otherSoundOption.Text = game.platform.StringFormat(language.SoundOption(), (game.AudioEnabled ? language.On() : language.Off()));
        otherAutoJumpOption = new Button();
        otherAutoJumpOption.Text = game.platform.StringFormat(language.AutoJumpOption(), (game.AutoJumpEnabled ? language.On() : language.Off()));
        otherLanguageSetting = new Button();
        otherLanguageSetting.Text = game.platform.StringFormat(language.ClientLanguageOption(), language.GetUsedLanguage());
        otherReturnToOptionsMenu = new Button();
        otherReturnToOptionsMenu.Text = language.ReturnToOptionsMenu();

        WidgetsClear();
        AddWidget(otherSoundOption);
        AddWidget(otherAutoJumpOption);
        AddWidget(otherLanguageSetting);
        AddWidget(otherReturnToOptionsMenu);
    }

    void OtherHandleClick(Button b)
    {
        if (b == otherSoundOption)
        {
            game.AudioEnabled = !game.AudioEnabled;
        }
        if (b == otherAutoJumpOption)
        {
            game.AutoJumpEnabled = !game.AutoJumpEnabled;
        }
        if (b == otherLanguageSetting)
        {
            //Switch language based on available languages
            game.language.NextLanguage();
        }
        if (b == otherReturnToOptionsMenu)
        {
            SetEscapeMenuState(EscapeMenuState.Options);
        }
    }


    Button[] keyButtons;
    Button keysDefaultKeys;
    Button keysReturnToOptionsMenu;

    const int keyButtonsCount = 1024;
    void KeysSet()
    {
        Language language = game.language;

        keyButtons = new Button[keyButtonsCount];
        for (int i = 0; i < keyButtonsCount; i++)
        {
            keyButtons[i] = null;
        }

        KeyHelp[] helps = keyhelps();
        for (int i = 0; i < keyButtonsCount; i++)
        {
            if (helps[i] == null)
            {
                break;
            }
            int defaultkey = helps[i].DefaultKey;
            int key = defaultkey;
            if (game.options.Keys[defaultkey] != 0)
            {
                key = game.options.Keys[defaultkey];
            }
            keyButtons[i] = new Button();
            keyButtons[i].Text = game.platform.StringFormat2(language.KeyChange(), helps[i].Text, KeyName(key));
            AddWidget(keyButtons[i]);

        }
        keysDefaultKeys = new Button();
        keysDefaultKeys.Text = language.DefaultKeys();
        keysReturnToOptionsMenu = new Button();
        keysReturnToOptionsMenu.Text = language.ReturnToOptionsMenu();
        AddWidget(keysDefaultKeys);
        AddWidget(keysReturnToOptionsMenu);
    }

    void KeysHandleClick(Button b)
    {
        if (keyButtons != null)
        {
            for (int i = 0; i < keyButtonsCount; i++)
            {
                if (keyButtons[i] == b)
                {
                    keyselectid = i;
                }
            }
        }
        if (b == keysDefaultKeys)
        {
            game.options.Keys = new int[256];
        }
        if (b == keysReturnToOptionsMenu)
        {
            SetEscapeMenuState(EscapeMenuState.Options);
        }
    }

    void HandleButtonClick(Button w)
    {
        MainHandleClick(w);
        OptionsHandleClick(w);
        GraphicsHandleClick(w);
        OtherHandleClick(w);
        KeysHandleClick(w);
    }

    void AddWidget(Button b)
    {
        widgets[widgetsCount++] = b;
    }

    void WidgetsClear()
    {
        widgetsCount = 0;
    }

    internal Game game;
    EscapeMenuState escapemenustate;
    void EscapeMenuMouse1()
    {
        for (int i = 0; i < widgetsCount; i++)
        {
            Button w = widgets[i];
            w.selected = RectContains(w.x, w.y, w.width, w.height, game.mouseCurrentX, game.mouseCurrentY);
            if (w.selected && game.mouseleftclick)
            {
                HandleButtonClick(w);
                break;
            }
        }
    }

    bool RectContains(int x, int y, int w, int h, int px, int py)
    {
        return px >= x
            && py >= y
            && px < x + w
            && py < y + h;
    }

    void SetEscapeMenuState(EscapeMenuState state)
    {
        Language language = game.language;
        escapemenustate = state;
        WidgetsClear();
        if (state == EscapeMenuState.Main)
        {
            MainSet();
            MakeSimpleOptions(20, 50);
        }
        else if (state == EscapeMenuState.Options)
        {
            OptionsSet();
            MakeSimpleOptions(20, 50);
        }
        else if (state == EscapeMenuState.Graphics)
        {
            GraphicsSet();
            MakeSimpleOptions(20, 50);
        }
        else if (state == EscapeMenuState.Other)
        {
            OtherSet();
            MakeSimpleOptions(20, 50);
        }
        else if (state == EscapeMenuState.Keys)
        {
            KeysSet();
            int fontsize = 12;
            int textheight = 20;
            MakeSimpleOptions(fontsize, textheight);
        }
    }

    void UseFullscreen()
    {
        if (game.options.Fullscreen)
        {
            if (!changedResolution)
            {
                originalResolutionWidth = game.platform.GetDisplayResolutionDefault().Width;
                originalResolutionHeight = game.platform.GetDisplayResolutionDefault().Height;
                changedResolution = true;
            }
            game.platform.SetWindowState(WindowState.Fullscreen);
            UseResolution();
        }
        else
        {
            game.platform.SetWindowState(WindowState.Normal);
            RestoreResolution();
        }
    }

    string VsyncString()
    {
        if (game.ENABLE_LAG == 0) { return "Vsync"; }
        else if (game.ENABLE_LAG == 1) { return "Unlimited"; }
        else if (game.ENABLE_LAG == 2) { return "Lag"; }
        else return null; //throw new Exception();
    }

    string ResolutionString()
    {
        IntRef resolutionsCount = new IntRef();
        DisplayResolutionCi res = game.platform.GetDisplayResolutions(resolutionsCount)[game.options.Resolution];
        return game.platform.StringFormat4("{0}x{1}, {2}, {3} Hz",
            game.platform.IntToString(res.Width),
            game.platform.IntToString(res.Height),
            game.platform.IntToString(res.BitsPerPixel),
            game.platform.IntToString(game.platform.FloatToInt(res.RefreshRate)));
    }

    void ToggleResolution()
    {
        OptionsCi options = game.options;
        options.Resolution++;

        IntRef resolutionsCount = new IntRef();
        game.platform.GetDisplayResolutions(resolutionsCount);

        if (options.Resolution >= resolutionsCount.value)
        {
            options.Resolution = 0;
        }
    }

    int originalResolutionWidth;
    int originalResolutionHeight;
    bool changedResolution;
    public void RestoreResolution()
    {
        if (changedResolution)
        {
            game.platform.ChangeResolution(originalResolutionWidth, originalResolutionHeight, 32, -1);
            changedResolution = false;
        }
    }
    public void UseResolution()
    {
        OptionsCi options = game.options;
        IntRef resolutionsCount = new IntRef();
        DisplayResolutionCi[] resolutions = game.platform.GetDisplayResolutions(resolutionsCount);

        if (resolutions == null)
        {
            return;
        }

        if (options.Resolution >= resolutionsCount.value)
        {
            options.Resolution = 0;
        }
        DisplayResolutionCi res = resolutions[options.Resolution];
        if (game.platform.GetWindowState() == WindowState.Fullscreen)
        {
            game.platform.ChangeResolution(res.Width, res.Height, res.BitsPerPixel, res.RefreshRate);
            game.platform.SetWindowState(WindowState.Normal);
            game.platform.SetWindowState(WindowState.Fullscreen);
        }
        else
        {
            //d_GlWindow.Width = res.Width;
            //d_GlWindow.Height = res.Height;
        }
    }

    string[] fonts;
    int fontsLength;
    int[] fontValues;

    string FontString()
    {
        return fonts[game.options.Font];
    }
    void ToggleFont()
    {
        OptionsCi options = game.options;
        options.Font++;
        if (options.Font >= fontsLength)
        {
            options.Font = 0;
        }
        game.Font = fontValues[options.Font];
        game.UpdateTextRendererFont();
        for (int i = 0; i < game.cachedTextTexturesMax; i++)
        {
            game.cachedTextTextures[i] = null;
        }
    }

    string KeyName(int key)
    {
        return game.platform.KeyName(key);
    }

    void MakeSimpleOptions(int fontsize, int textheight)
    {
        int starty = game.ycenter(widgetsCount * textheight);
        for (int i = 0; i < widgetsCount; i++)
        {
            string s = widgets[i].Text;
            float sizeWidth = game.TextSizeWidth(s, fontsize);
            float sizeHeight = game.TextSizeHeight(s, fontsize);
            int Width = game.platform.FloatToInt(sizeWidth) + 10;
            int Height = game.platform.FloatToInt(sizeHeight);
            int X = game.xcenter(sizeWidth);
            int Y = starty + textheight * i;
            widgets[i].x = X;
            widgets[i].y = Y;
            widgets[i].width = Width;
            widgets[i].height = Height;
            widgets[i].fontsize = fontsize;
            if (i == keyselectid)
            {
                widgets[i].fontcolor = Game.ColorFromArgb(255, 0, 255, 0);
                widgets[i].fontcolorselected = Game.ColorFromArgb(255, 0, 255, 0);
            }
        }
    }
    bool loaded;
    public override void OnNewFrameDraw2d(Game game_, float deltaTime)
    {
        game = game_;
        if (!loaded)
        {
            loaded = true;
            LoadOptions();
        }
        if (game.escapeMenuRestart)
        {
            game.escapeMenuRestart = false;
            SetEscapeMenuState(EscapeMenuState.Main);
        }
        if (game.guistate != GuiState.EscapeMenu)
        {
            return;
        }
        SetEscapeMenuState(escapemenustate);
        EscapeMenuMouse1();
        for (int i = 0; i < widgetsCount; i++)
        {
            Button w = widgets[i];
            game.Draw2dText1(w.Text, w.x, w.y, w.fontsize, IntRef.Create(w.selected ? w.fontcolorselected : w.fontcolor), false);
        }
    }
    Button[] widgets;
    KeyHelp[] keyhelps()
    {
        int n = 1024;
        KeyHelp[] helps = new KeyHelp[n];
        for (int i = 0; i < n; i++)
        {
            helps[i] = null;
        }
        Language language = game.language;
        int count = 0;
        helps[count++] = KeyHelpCreate(language.KeyMoveFoward(), GlKeys.W);
        helps[count++] = KeyHelpCreate(language.KeyMoveBack(), GlKeys.S);
        helps[count++] = KeyHelpCreate(language.KeyMoveLeft(), GlKeys.A);
        helps[count++] = KeyHelpCreate(language.KeyMoveRight(), GlKeys.D);
        helps[count++] = KeyHelpCreate(language.KeyJump(), GlKeys.Space);
        helps[count++] = KeyHelpCreate(language.KeyShowMaterialSelector(), GlKeys.B);
        helps[count++] = KeyHelpCreate(language.KeySetSpawnPosition(), GlKeys.P);
        helps[count++] = KeyHelpCreate(language.KeyRespawn(), GlKeys.O);
        helps[count++] = KeyHelpCreate(language.KeyReloadWeapon(), GlKeys.R);
        helps[count++] = KeyHelpCreate(language.KeyToggleFogDistance(), GlKeys.F);
        helps[count++] = KeyHelpCreate(game.platform.StringFormat(language.KeyMoveSpeed(), "1"), GlKeys.F1);
        helps[count++] = KeyHelpCreate(game.platform.StringFormat(language.KeyMoveSpeed(), "10"), GlKeys.F2);
        helps[count++] = KeyHelpCreate(language.KeyFreeMove(), GlKeys.F3);
        helps[count++] = KeyHelpCreate(language.KeyThirdPersonCamera(), GlKeys.F5);
        helps[count++] = KeyHelpCreate(language.KeyTextEditor(), GlKeys.F9);
        helps[count++] = KeyHelpCreate(language.KeyFullscreen(), GlKeys.F11);
        helps[count++] = KeyHelpCreate(language.KeyScreenshot(), GlKeys.F12);
        helps[count++] = KeyHelpCreate(language.KeyPlayersList(), GlKeys.Tab);
        helps[count++] = KeyHelpCreate(language.KeyChat(), GlKeys.T);
        helps[count++] = KeyHelpCreate(language.KeyTeamChat(), GlKeys.Y);
        helps[count++] = KeyHelpCreate(language.KeyCraft(), GlKeys.C);
        helps[count++] = KeyHelpCreate(language.KeyBlockInfo(), GlKeys.I);
        helps[count++] = KeyHelpCreate(language.KeyUse(), GlKeys.E);
        helps[count++] = KeyHelpCreate(language.KeyReverseMinecart(), GlKeys.Q);
        return helps;
    }

    KeyHelp KeyHelpCreate(string text, int defaultKey)
    {
        KeyHelp h = new KeyHelp();
        h.Text = text;
        h.DefaultKey = defaultKey;
        return h;
    }


    int keyselectid;
    public override void OnKeyDown(Game game_, KeyEventArgs args)
    {
        int eKey = args.GetKeyCode();
        if (eKey == game.GetKey(GlKeys.Escape))
        {
            if (escapemenustate == EscapeMenuState.Graphics
                || escapemenustate == EscapeMenuState.Keys
                || escapemenustate == EscapeMenuState.Other)
            {
                SetEscapeMenuState(EscapeMenuState.Options);
            }
            else if (escapemenustate == EscapeMenuState.Options)
            {
                SaveOptions();
                SetEscapeMenuState(EscapeMenuState.Main);
            }
            else
            {
                SetEscapeMenuState(EscapeMenuState.Main);
                game.GuiStateBackToGame();
            }
            args.SetHandled(true);
        }
        if (escapemenustate == EscapeMenuState.Keys)
        {
            if (keyselectid != -1)
            {
                game.options.Keys[keyhelps()[keyselectid].DefaultKey] = eKey;
                keyselectid = -1;
                args.SetHandled(true);
            }
        }
        if (eKey == game.GetKey(GlKeys.F11))
        {
            if (game.platform.GetWindowState() == WindowState.Fullscreen)
            {
                game.platform.SetWindowState(WindowState.Normal);
                RestoreResolution();
                SaveOptions();
            }
            else
            {
                game.platform.SetWindowState(WindowState.Fullscreen);
                UseResolution();
                SaveOptions();
            }
            args.SetHandled(true);
        }
    }
    public void LoadOptions()
    {
        OptionsCi o = LoadOptions_();
        if (o == null)
        {
            return;
        }
        game.options = o;
        OptionsCi options = o;

        game.Font = fontValues[options.Font];
        game.UpdateTextRendererFont();
        //game.d_CurrentShadows.ShadowsFull = options.Shadows;
        game.d_Config3d.viewdistance = options.DrawDistance;
        game.AudioEnabled = options.EnableSound;
        game.AutoJumpEnabled = options.EnableAutoJump;
        if (options.ClientLanguage != "")
        {
            game.language.OverrideLanguage = options.ClientLanguage;
        }
        game.d_TerrainChunkTesselator.EnableSmoothLight = options.Smoothshadows;
        game.d_TerrainChunkTesselator.BlockShadow = options.BlockShadowSave;
        game.d_TerrainChunkTesselator.option_DarkenBlockSides = options.EnableBlockShadow;
        game.ENABLE_LAG = options.Framerate;
        UseFullscreen();
        game.UseVsync();
        UseResolution();
    }

    OptionsCi LoadOptions_()
    {
        OptionsCi options = new OptionsCi();
        Preferences preferences = game.platform.GetPreferences();
                
        options.Shadows = preferences.GetBool("Shadows", true);
        options.Font = preferences.GetInt("Font", 0);
        options.DrawDistance = preferences.GetInt("DrawDistance", game.platform.IsFastSystem() ? 128 : 32);
        options.UseServerTextures = preferences.GetBool("UseServerTextures", true);
        options.EnableSound = preferences.GetBool("EnableSound", true);
        options.EnableAutoJump = preferences.GetBool("EnableAutoJump", false);
        options.ClientLanguage = preferences.GetString("ClientLanguage", "");
        options.Framerate = preferences.GetInt("Framerate", 0);
        options.Resolution = preferences.GetInt("Resolution", 0);
        options.Fullscreen = preferences.GetBool("Fullscreen", false);
        options.Smoothshadows = preferences.GetBool("Smoothshadows", true);
        options.BlockShadowSave = one * preferences.GetInt("BlockShadowSave", 70) / 100;
        options.EnableBlockShadow = preferences.GetBool("EnableBlockShadow", true);

        for (int i = 0; i < 256; i++)
        {
            string preferencesKey = StringTools.StringAppend(game.platform, "Key", game.platform.IntToString(i));
            int value = preferences.GetInt(preferencesKey, 0);
            if (value != 0)
            {
                options.Keys[i] = value;
            }
        }

        return options;
    }

    public void SaveOptions()
    {
        OptionsCi options = game.options;

        options.Font = game.Font;
        options.Shadows = true; // game.d_CurrentShadows.ShadowsFull;
        options.DrawDistance = game.platform.FloatToInt(game.d_Config3d.viewdistance);
        options.EnableSound = game.AudioEnabled;
        options.EnableAutoJump = game.AutoJumpEnabled;
        if (game.language.OverrideLanguage != null)
        {
            options.ClientLanguage = game.language.OverrideLanguage;
        }
        options.Framerate = game.ENABLE_LAG;
        options.Fullscreen = game.platform.GetWindowState() == WindowState.Fullscreen;
        options.Smoothshadows = game.d_TerrainChunkTesselator.EnableSmoothLight;
        options.EnableBlockShadow = game.d_TerrainChunkTesselator.option_DarkenBlockSides;

        SaveOptions_(options);
    }

    void SaveOptions_(OptionsCi options)
    {
        Preferences preferences = game.platform.GetPreferences();

        preferences.SetBool("Shadows", options.Shadows);
        preferences.SetInt("Font", options.Font);
        preferences.SetInt("DrawDistance", options.DrawDistance);
        preferences.SetBool("UseServerTextures", options.UseServerTextures);
        preferences.SetBool("EnableSound", options.EnableSound);
        preferences.SetBool("EnableAutoJump", options.EnableAutoJump);
        if (options.ClientLanguage != "")
        {
            preferences.SetString("ClientLanguage", options.ClientLanguage);
        }
        preferences.SetInt("Framerate", options.Framerate);
        preferences.SetInt("Resolution", options.Resolution);
        preferences.SetBool("Fullscreen", options.Fullscreen);
        preferences.SetBool("Smoothshadows", options.Smoothshadows);
        preferences.SetInt("BlockShadowSave", game.platform.FloatToInt(options.BlockShadowSave * 100));
        preferences.SetBool("EnableBlockShadow", options.EnableBlockShadow);

        for (int i = 0; i < 256; i++)
        {
            int value = options.Keys[i];string preferencesKey = StringTools.StringAppend(game.platform, "Key", game.platform.IntToString(i));
            if (value != 0)
            {
                preferences.SetInt(preferencesKey, value);
            }
            else
            {
                preferences.Remove(preferencesKey);
            }
        }

        game.platform.SetPreferences(preferences);
    }
}

public class Button
{
    public Button()
    {
        fontcolor = Game.ColorFromArgb(255, 255, 255, 255);
        fontcolorselected = Game.ColorFromArgb(255, 255, 0, 0);
        fontsize = 20;
    }
    internal int x;
    internal int y;
    internal int width;
    internal int height;
    internal string Text;
    internal bool selected;
    internal int fontsize;
    internal int fontcolor;
    internal int fontcolorselected;
}

public class KeyHelp
{
    internal string Text;
    internal int DefaultKey;
}

public class DisplayResolutionCi
{
    internal int Width;
    internal int Height;
    internal int BitsPerPixel;
    internal float RefreshRate;
    public int GetWidth() { return Width; } public void SetWidth(int value) { Width = value; }
    public int GetHeight() { return Height; } public void SetHeight(int value) { Height = value; }
    public int GetBitsPerPixel() { return BitsPerPixel; } public void SetBitsPerPixel(int value) { BitsPerPixel = value; }
    public float GetRefreshRate() { return RefreshRate; } public void SetRefreshRate(float value) { RefreshRate = value; }
}

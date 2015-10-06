public class MainMenu
{
    public MainMenu()
    {
        one = 1;
        textures = new DictionaryStringInt1024();
        textTextures = new TextTexture[256];
        textTexturesCount = 0;
        screen = new ScreenMain();
        screen.menu = this;
        loginClient = new LoginClientCi();
        assets = new AssetList();
        assetsLoadProgress = new FloatRef();
    }

    internal GamePlatform p;
    internal Language lang;

    internal float one;

    internal AssetList assets;
    internal FloatRef assetsLoadProgress;
    internal TextColorRenderer textColorRenderer;

    public void Start(GamePlatform p_)
    {
        this.p = p_;

        //Initialize translations
        lang = new Language();
        lang.platform = p;
        lang.LoadTranslations();
        p.SetTitle(lang.GameName());

        textColorRenderer = new TextColorRenderer();
        textColorRenderer.platform = p_;
        p_.LoadAssetsAsyc(assets, assetsLoadProgress);

        overlap = 200;
        minspeed = 20;
        rnd = p.RandomCreate();

        xRot = 0;
        xInv = false;
        xSpeed = minspeed + rnd.MaxNext(5);

        yRot = 0;
        yInv = false;
        ySpeed = minspeed + rnd.MaxNext(5);

        z = -5;

        filter = 0;

        mvMatrix = Mat4.Create();
        pMatrix = Mat4.Create();

        currentlyPressedKeys = new bool[256];
        p.AddOnNewFrame(MainMenuNewFrameHandler.Create(this));
        p.AddOnKeyEvent(MainMenuKeyEventHandler.Create(this));
        p.AddOnMouseEvent(MainMenuMouseEventHandler.Create(this));
        p.AddOnTouchEvent(MainMenuTouchEventHandler.Create(this));
    }

    int viewportWidth;
    int viewportHeight;

    float[] mvMatrix;
    float[] pMatrix;

    bool[] currentlyPressedKeys;

    public void HandleKeyDown(KeyEventArgs e)
    {
        currentlyPressedKeys[e.GetKeyCode()] = true;
        screen.OnKeyDown(e);
    }

    public void HandleKeyUp(KeyEventArgs e)
    {
        currentlyPressedKeys[e.GetKeyCode()] = false;
        screen.OnKeyUp(e);
    }

    public void HandleKeyPress(KeyPressEventArgs e)
    {
        if (e.GetKeyChar() == 70 || e.GetKeyChar() == 102) // 'F', 'f'
        {
            filter += 1;
            if (filter == 3)
            {
                filter = 0;
            }
        }
        if (e.GetKeyChar() == 96) // '`'
        {
            screen.OnBackPressed();
        }
        screen.OnKeyPress(e);
    }

    void DrawScene(float dt)
    {
        p.GlViewport(0, 0, viewportWidth, viewportHeight);
        p.GlClearColorBufferAndDepthBuffer();
        p.GlDisableDepthTest();
        p.GlDisableCullFace();
        {
            //Mat4.Perspective(pMatrix, 45, one * viewportWidth / viewportHeight, one / 100, one * 1000);
            //Mat4.Identity_(mvMatrix);
            //Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(0, 0, z));
        }
        {
            Mat4.Identity_(pMatrix);
            Mat4.Ortho(pMatrix, 0, p.GetCanvasWidth(), p.GetCanvasHeight(), 0, 0, 10);
        }

        screen.Render(dt);
    }

    Screen screen;

    internal void DrawButton(string text, float fontSize, float dx, float dy, float dw, float dh, bool pressed)
    {
        Draw2dQuad(pressed ? GetTexture("button_sel.png") : GetTexture("button.png"), dx, dy, dw, dh);
        
        if ((text != null) && (text != ""))
        {
            DrawText(text, fontSize, dx + dw / 2, dy + dh / 2, TextAlign.Center, TextBaseline.Middle);
        }
    }

    internal void DrawText(string text, float fontSize, float x, float y, TextAlign align, TextBaseline baseline)
    {
        TextTexture t = GetTextTexture(text, fontSize);
        int dx = 0;
        int dy = 0;
        if (align == TextAlign.Center)
        {
            dx -= t.textwidth / 2;
        }
        if (align == TextAlign.Right)
        {
            dx -= t.textwidth;
        }
        if (baseline == TextBaseline.Middle)
        {
            dy -= t.textheight / 2;
        }
        if (baseline == TextBaseline.Bottom)
        {
            dy -= t.textheight;
        }
        Draw2dQuad(t.texture, x + dx, y + dy, t.texturewidth, t.textureheight);
    }

    internal void DrawServerButton(string name, string motd, string gamemode, string playercount, float x, float y, float width, float height, string image)
    {
        //Server buttons default to: (screen width - 200) x 64
        Draw2dQuad(GetTexture("serverlist_entry_background.png"), x, y, width, height);
        Draw2dQuad(GetTexture(image), x, y, height, height);

        //       value          size    x position              y position              text alignment      text baseline
        DrawText(name,          14,     x + 70,                 y + 5,                  TextAlign.Left,     TextBaseline.Top);
        DrawText(gamemode,      12,     x + width - 10,         y + height - 5,         TextAlign.Right,    TextBaseline.Bottom);
        DrawText(playercount,   12,     x + width - 10,         y + 5,                  TextAlign.Right,    TextBaseline.Top);
        DrawText(motd,          12,     x + 70,                 y + height - 5,         TextAlign.Left,     TextBaseline.Bottom);
    }

    TextTexture GetTextTexture(string text, float fontSize)
    {
        for (int i = 0; i < textTexturesCount; i++)
        {
            TextTexture t = textTextures[i];
            if (t == null)
            {
                continue;
            }
            if (t.text == text && t.size == fontSize)
            {
                return t;
            }
        }
        TextTexture textTexture = new TextTexture();

        Text_ text_ = new Text_();
        text_.text = text;
        text_.fontsize = fontSize;
        text_.fontfamily = "Arial";
        text_.color = Game.ColorFromArgb(255, 255, 255, 255);
        BitmapCi textBitmap = textColorRenderer.CreateTextTexture(text_);

        int texture = p.LoadTextureFromBitmap(textBitmap);
        
        IntRef textWidth = new IntRef();
        IntRef textHeight = new IntRef();
        p.TextSize(text, fontSize, textWidth, textHeight);

        textTexture.texture = texture;
        textTexture.texturewidth = p.FloatToInt(p.BitmapGetWidth(textBitmap));
        textTexture.textureheight = p.FloatToInt(p.BitmapGetHeight(textBitmap));
        textTexture.text = text;
        textTexture.size = fontSize;
        textTexture.textwidth = textWidth.value;
        textTexture.textheight = textHeight.value;

        p.BitmapDelete(textBitmap);
        
        textTextures[textTexturesCount++] = textTexture;
        return textTexture;
    }

    internal DictionaryStringInt1024 textures;
    internal int GetTexture(string name)
    {
        if (!textures.Contains(name))
        {
            BoolRef found = new BoolRef();
            BitmapCi bmp = p.BitmapCreateFromPng(GetFile(name), GetFileLength(name));
            int texture = p.LoadTextureFromBitmap(bmp);
            textures.Set(name, texture);
            p.BitmapDelete(bmp);
        }
        return textures.Get(name);
    }

    internal byte[] GetFile(string name)
    {
        string pLowercase = p.StringToLower(name);
        for (int i = 0; i < assets.count; i++)
        {
            if (assets.items[i].name == pLowercase)
            {
                return assets.items[i].data;
            }
        }
        return null;
    }

    internal int GetFileLength(string name)
    {
        string pLowercase = p.StringToLower(name);
        for (int i = 0; i < assets.count; i++)
        {
            if (assets.items[i].name == pLowercase)
            {
                return assets.items[i].dataLength;
            }
        }
        return 0;
    }

    Model cubeModel;
    public void Draw2dQuad(int textureid, float dx, float dy, float dw, float dh)
    {
        Mat4.Identity_(mvMatrix);
        Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(dx, dy, 0));
        Mat4.Scale(mvMatrix, mvMatrix, Vec3.FromValues(dw, dh, 0));
        Mat4.Scale(mvMatrix, mvMatrix, Vec3.FromValues(one / 2, one / 2, 0));
        Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(one, one, 0));
        SetMatrixUniforms();
        if (cubeModel == null)
        {
            cubeModel = p.CreateModel(QuadModelData.GetQuadModelData());
        }
        p.BindTexture2d(textureid);
        p.DrawModel(cubeModel);
    }

    void SetMatrixUniforms()
    {
        p.SetMatrixUniformProjection(pMatrix);
        p.SetMatrixUniformModelView(mvMatrix);
    }

    float degToRad(float degrees)
    {
        return degrees * GlMatrixMath.PI() / 180;
    }

    float xRot;
    bool xInv;
    float xSpeed;

    float yRot;
    bool yInv;
    float ySpeed;

    int overlap;
    int minspeed;
    RandomCi rnd;

    float z;

    int filter;

    bool initialized;

    void Animate(float dt)
    {
        float maxDt = 1;
        if (dt > maxDt)
        {
            dt = maxDt;
        }
        if (xInv)
        {
            if (xRot <= -overlap)
            {
                xInv = false;
                xSpeed = minspeed + rnd.MaxNext(5);
            }
            xRot -= xSpeed * dt;
        }
        else
        {
            if (xRot >= overlap)
            {
                xInv = true;
                xSpeed = minspeed + rnd.MaxNext(5);
            }
            xRot += xSpeed * dt;
        }
        if (yInv)
        {
            if (yRot <= -overlap)
            {
                yInv = false;
                ySpeed = minspeed + rnd.MaxNext(5);
            }
            yRot -= ySpeed * dt;
        }
        else
        {
            if (yRot >= overlap)
            {
                yInv = true;
                ySpeed = minspeed + rnd.MaxNext(5);
            }
            yRot += ySpeed * dt;
        }
    }

    public void OnNewFrame(NewFrameEventArgs args)
    {
        if (!initialized)
        {
            initialized = true;
            p.InitShaders();

            p.GlClearColorRgbaf(0, 0, 0, 1);
            p.GlEnableDepthTest();
        }
        viewportWidth = p.GetCanvasWidth();
        viewportHeight = p.GetCanvasHeight();
        DrawScene(args.GetDt());
        Animate(args.GetDt());
        loginClient.Update(p);
    }

    public void HandleMouseDown(MouseEventArgs e)
    {
        mousePressed = true;
        previousMouseX = e.GetX();
        previousMouseY = e.GetY();
        screen.OnMouseDown(e);
    }

    public void HandleMouseUp(MouseEventArgs e)
    {
        mousePressed = false;
        screen.OnMouseUp(e);
    }

    bool mousePressed;

    int previousMouseX;
    int previousMouseY;

    public void HandleMouseMove(MouseEventArgs e)
    {
        float dx = e.GetMovementX();
        float dy = e.GetMovementY();
        previousMouseX = e.GetX();
        previousMouseY = e.GetY();
        if (mousePressed)
        {
            //            ySpeed += dx / 10;
            //            xSpeed += dy / 10;
        }
        screen.OnMouseMove(e);
    }

    public void HandleMouseWheel(MouseWheelEventArgs e)
    {
        z += e.GetDeltaPrecise() / 5;
        screen.OnMouseWheel(e);
    }

    public void HandleTouchStart(TouchEventArgs e)
    {
        touchId = e.GetId();
        previousTouchX = e.GetX();
        previousTouchY = e.GetY();
        screen.OnTouchStart(e);
    }

    int touchId;
    int previousTouchX;
    int previousTouchY;

    public void HandleTouchMove(TouchEventArgs e)
    {
        screen.OnTouchMove(e);
        if (e.GetId() != touchId)
        {
            return;
        }
        float dx = e.GetX() - previousTouchX;
        float dy = e.GetY() - previousTouchY;
        previousTouchX = e.GetX();
        previousTouchY = e.GetY();

        ySpeed += dx / 10;
        xSpeed += dy / 10;
    }

    public void HandleTouchEnd(TouchEventArgs e)
    {
        screen.OnTouchEnd(e);
    }

    TextTexture[] textTextures;
    int textTexturesCount;

    internal void StartSingleplayer()
    {
        screen = new ScreenSingleplayer();
        screen.menu = this;
        screen.LoadTranslations();
    }

    internal void StartLogin(string serverHash, string ip, int port)
    {
        ScreenLogin screenLogin = new ScreenLogin();
        screenLogin.serverHash = serverHash;
        screenLogin.serverIp = ip;
        screenLogin.serverPort = port;
        screen = screenLogin;
        screen.menu = this;
        screen.LoadTranslations();
    }

    internal void StartConnectToIp()
    {
        ScreenConnectToIp screenConnectToIp = new ScreenConnectToIp();
        screen = screenConnectToIp;
        screen.menu = this;
        screen.LoadTranslations();
    }

    internal void Exit()
    {
        p.Exit();
    }

    internal void StartMainMenu()
    {
        screen = new ScreenMain();
        screen.menu = this;
        p.ExitMousePointerLock();
    }

    internal int backgroundW;
    internal int backgroundH;
    internal float windowX;
    internal float windowY;
    internal void DrawBackground()
    {
        backgroundW = 512;
        backgroundH = 512;
        windowX = p.GetCanvasWidth();
        windowY = p.GetCanvasHeight();
        //Background tiling
        int countX = p.FloatToInt((windowX + (2 * overlap)) / backgroundW) + 1;
        int countY = p.FloatToInt((windowY + (2 * overlap)) / backgroundH) + 1;
        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                Draw2dQuad(GetTexture("background.png"), x * backgroundW + xRot - overlap, y * backgroundH + yRot - overlap, backgroundW, backgroundH);
            }
        }
    }

    internal void StartMultiplayer()
    {
        screen = new ScreenMultiplayer();
        screen.menu = this;
        screen.LoadTranslations();
    }

    internal void Login(string user, string password, string serverHash, string token, LoginResultRef loginResult, LoginData loginResultData)
    {
        if (user == "" || (password == "" && token == ""))
        {
            loginResult.value = LoginResult.Failed;
        }
        else
        {
            loginClient.Login(p, user, password, serverHash, token, loginResult, loginResultData);
        }
    }
    LoginClientCi loginClient;

    internal void CreateAccount(string user, string password, LoginResultRef loginResult)
    {
        if (user == "" || password == "")
        {
            loginResult.value = LoginResult.Failed;
        }
        else
        {
            loginResult.value = LoginResult.Ok;
        }
    }

    internal string[] GetSavegames(IntRef length)
    {
        string[] files = p.DirectoryGetFiles(p.PathSavegames(), length);
        string[] savegames = new string[length.value];
        int count = 0;
        for (int i = 0; i < length.value; i++)
        {
            if(StringEndsWith(files[i], ".mddbs"))
            {
                savegames[count++] = files[i];
            }
        }
        length.value = count;
        return savegames;
    }

    public bool StringEndsWith(string s, string value)
    {
        return StringTools.StringSubstring(p, s, StringLength(s) - StringLength(value), StringLength(value)) == value;
    }

    public int StringLength(string a)
    {
        IntRef length = new IntRef();
        p.StringToCharArray(a, length);
        return length.value;
    }

    public string CharToString(int a)
    {
        int[] arr = new int[1];
        arr[0] = a;
        return p.CharArrayToString(arr, 1);
    }

    public string CharRepeat(int c, int length)
    {
        int[] charArray = new int[length];
        for (int i = 0; i < length; i++)
        {
            charArray[i] = c;
        }
        return p.CharArrayToString(charArray, length);
    }
    
    internal void StartNewWorld()
    {
    }

    internal void StartModifyWorld()
    {
    }

    public void StartGame(bool singleplayer, string singleplayerSavePath, ConnectData connectData)
    {
        ScreenGame screenGame = new ScreenGame();
        screenGame.menu = this;
        screenGame.Start(p, singleplayer, singleplayerSavePath, connectData);
        screen = screenGame;
    }

    internal void ConnectToGame(LoginData loginResultData, string username)
    {
        ConnectData connectData = new ConnectData();
        connectData.Ip = loginResultData.ServerAddress;
        connectData.Port = loginResultData.Port;
        connectData.Auth = loginResultData.AuthCode;
        connectData.Username = username;

        StartGame(false, null, connectData);
    }

    public void ConnectToSingleplayer(string filename)
    {
        StartGame(true, filename, null);
    }

    public float GetScale()
    {
        float scale;
        if (p.IsSmallScreen())
        {
            scale = one * p.GetCanvasWidth() / 1280;
        }
        else
        {
            scale = one;
        }
        return scale;
    }
}

public class TextTexture
{
    internal float size;
    internal string text;
    internal int texture;
    internal int texturewidth;
    internal int textureheight;
    internal int textwidth;
    internal int textheight;
}

public class Screen
{
    public Screen()
    {
        WidgetCount = 64;
        widgets = new MenuWidget[WidgetCount];
    }
    internal MainMenu menu;
    public virtual void Render(float dt) { }
    public virtual void OnKeyDown(KeyEventArgs e) { KeyDown(e); }
    public virtual void OnKeyPress(KeyPressEventArgs e) { KeyPress(e); }
    public virtual void OnKeyUp(KeyEventArgs e) {  }
    public virtual void OnTouchStart(TouchEventArgs e) { MouseDown(e.GetX(), e.GetY()); }
    public virtual void OnTouchMove(TouchEventArgs e) { }
    public virtual void OnTouchEnd(TouchEventArgs e) { MouseUp(e.GetX(), e.GetY()); }
    public virtual void OnMouseDown(MouseEventArgs e) { MouseDown(e.GetX(), e.GetY()); }
    public virtual void OnMouseUp(MouseEventArgs e) { MouseUp(e.GetX(), e.GetY()); }
    public virtual void OnMouseMove(MouseEventArgs e) { MouseMove(e); }
    public virtual void OnBackPressed() { }
    public virtual void LoadTranslations() { }

    void KeyDown(KeyEventArgs e)
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w == null)
            {
			    continue;
			}
            if (w.hasKeyboardFocus)
            {
                if (e.GetKeyCode() == GlKeys.Tab || e.GetKeyCode() == GlKeys.Enter)
                {
                    if (w.type == WidgetType.Button && e.GetKeyCode() == GlKeys.Enter)
                    {
                        //Call OnButton when enter is pressed and widget is a button
                        OnButton(w);
                        return;
                    }
                    if (w.nextWidget != -1)
                    {
                        //Just switch focus otherwise
                        w.LoseFocus();
                        widgets[w.nextWidget].GetFocus();
                        return;
                    }
                }
            }
            if (w.type == WidgetType.Textbox)
            {
                if (w.editing)
                {
                    int key = e.GetKeyCode();
                    // pasting text from clipboard
                    if (e.GetCtrlPressed() && key == GlKeys.V)
                    {
                        if (menu.p.ClipboardContainsText())
                        {
                            w.text = StringTools.StringAppend(menu.p, w.text, menu.p.ClipboardGetText());
                        }
                        return;
                    }
                    // deleting characters using backspace
                    if (key == GlKeys.BackSpace)
                    {
                        if (menu.StringLength(w.text) > 0)
                        {
                            w.text = StringTools.StringSubstring(menu.p, w.text, 0, menu.StringLength(w.text) - 1);
                        }
                        return;
                    }
                }
            }
        }
    }

    void KeyPress(KeyPressEventArgs e)
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Textbox)
                {
                    if (w.editing)
                    {
                        if (menu.p.IsValidTypingChar(e.GetKeyChar()))
                        {
                            w.text = StringTools.StringAppend(menu.p, w.text, menu.CharToString(e.GetKeyChar()));
                        }
                    }
                }
            }
        }
    }

    void MouseDown(int x, int y)
    {
        bool editingChange = false;
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Button)
                {
                    w.pressed = pointInRect(x, y, w.x, w.y, w.sizex, w.sizey);
                }
                if (w.type == WidgetType.Textbox)
                {
                    w.pressed = pointInRect(x, y, w.x, w.y, w.sizex, w.sizey);
                    bool wasEditing = w.editing;
                    w.editing = w.pressed;
                    if (w.editing && (!wasEditing))
                    {
                        menu.p.ShowKeyboard(true);
                        editingChange = true;
                    }
                    if ((!w.editing) && wasEditing && (!editingChange))
                    {
                        menu.p.ShowKeyboard(false);
                    }
                }
                if (w.pressed)
                {
                    //Set focus to new element when clicked on
                    AllLoseFocus();
                    w.GetFocus();
                }
            }
        }
    }
    
    void AllLoseFocus()
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                w.LoseFocus();
            }
        }
    }
    
    void MouseUp(int x, int y)
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                w.pressed = false;
            }
        }
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Button)
                {
                    if (pointInRect(x, y, w.x, w.y, w.sizex, w.sizey))
                    {
                        OnButton(w);
                    }
                }
            }
        }
    }

    public virtual void OnButton(MenuWidget w) { }

    void MouseMove(MouseEventArgs e)
    {
        if (e.GetEmulated() && !e.GetForceUsage())
        {
            return;
        }
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                w.hover = pointInRect(e.GetX(), e.GetY(), w.x, w.y, w.sizex, w.sizey);
            }
        }
    }

    bool pointInRect(float x, float y, float rx, float ry, float rw, float rh)
    {
        return x >= rx && y >= ry && x < rx + rw && y < ry + rh;
    }

    public virtual void OnMouseWheel(MouseWheelEventArgs e) { }
    internal int WidgetCount;
    internal MenuWidget[] widgets;
    public void DrawWidgets()
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (!w.visible)
                {
                    continue;
                }
                string text = w.text;
                if (w.selected)
                {
                    text = StringTools.StringAppend(menu.p, "&2", text);
                }
                if (w.type == WidgetType.Button)
                {
                    if (w.buttonStyle == ButtonStyle.Text)
                    {
                        if (w.image != null)
                        {
                            menu.Draw2dQuad(menu.GetTexture(w.image), w.x, w.y, w.sizex, w.sizey);
                        }
                        menu.DrawText(text, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Left, TextBaseline.Middle);
                    }
                    else if (w.buttonStyle == ButtonStyle.Button)
                    {
                        menu.DrawButton(text, w.fontSize, w.x, w.y, w.sizex, w.sizey, (w.hover || w.hasKeyboardFocus));
                        if (w.description != null)
                        {
                            menu.DrawText(w.description, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Right, TextBaseline.Middle);
                        }
                    }
                    else
                    {
                        string[] strings = menu.p.StringSplit(w.text, "\n", new IntRef());
                        if (w.selected)
                        {
                            //Highlight text if selected
                            strings[0] = StringTools.StringAppend(menu.p, "&2", strings[0]);
                            strings[1] = StringTools.StringAppend(menu.p, "&2", strings[1]);
                            strings[2] = StringTools.StringAppend(menu.p, "&2", strings[2]);
                            strings[3] = StringTools.StringAppend(menu.p, "&2", strings[3]);
                        }
                        menu.DrawServerButton(strings[0], strings[1], strings[2], strings[3], w.x, w.y, w.sizex, w.sizey, w.image);
                        if (w.description != null)
                        {
                            //Display a warning sign, when server does not respond to queries
                            menu.Draw2dQuad(menu.GetTexture("serverlist_entry_noresponse.png"), w.x - 38 * menu.GetScale(), w.y, w.sizey / 2, w.sizey / 2);
                        }
                        if (strings[4] != menu.p.GetGameVersion())
                        {
                            //Display an icon if server version differs from client version
                            menu.Draw2dQuad(menu.GetTexture("serverlist_entry_differentversion.png"), w.x - 38 * menu.GetScale(), w.y + w.sizey / 2, w.sizey / 2, w.sizey / 2);
                        }
                    }
                }
                if (w.type == WidgetType.Textbox)
                {
                    if (w.password)
                    {
                        text = menu.CharRepeat(42, menu.StringLength(w.text)); // '*'
                    }
                    if (w.editing)
                    {
                        text = StringTools.StringAppend(menu.p, text, "_");
                    }
                    if (w.buttonStyle == ButtonStyle.Text)
                    {
                        if (w.image != null)
                        {
                            menu.Draw2dQuad(menu.GetTexture(w.image), w.x, w.y, w.sizex, w.sizey);
                        }
                        menu.DrawText(text, w.fontSize, w.x, w.y, TextAlign.Left, TextBaseline.Top);
                    }
                    else
                    {
                        menu.DrawButton(text, w.fontSize, w.x, w.y, w.sizex, w.sizey, (w.hover || w.editing || w.hasKeyboardFocus));
                    }
                    if (w.description != null)
                    {
                        menu.DrawText(w.description, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Right, TextBaseline.Middle);
                    }
                }
            }
        }
    }
}

public enum LoginResult
{
    None,
    Connecting,
    Failed,
    Ok
}

public class LoginResultRef
{
    internal LoginResult value;
}

public class HttpResponseCi
{
    internal bool done;
    internal byte[] value;
    internal int valueLength;

    internal string GetString(GamePlatform platform)
    {
        return platform.StringFromUtf8ByteArray(value, valueLength);
    }

    internal bool error;

    public bool GetDone() { return done; } public void SetDone(bool value_) { done = value_; }
    public byte[] GetValue() { return value; } public void SetValue(byte[] value_) { value = value_; }
    public int GetValueLength() { return valueLength; } public void SetValueLength(int value_) { valueLength = value_; }
    public bool GetError() { return error; } public void SetError(bool value_) { error = value_; }
}

public class ThumbnailResponseCi
{
    internal bool done;
    internal bool error;
    internal string serverMessage;
    internal byte[] data;
    internal int dataLength;
}

public class ServerOnList
{
    internal string hash;
    internal string name;
    internal string motd;
    internal int port;
    internal string ip;
    internal string version;
    internal int users;
    internal int max;
    internal string gamemode;
    internal string players;
    internal bool thumbnailDownloading;
    internal bool thumbnailError;
    internal bool thumbnailFetched;
}

public enum WidgetType
{
    Button,
    Textbox,
    Label
}

public class MenuWidget
{
    public MenuWidget()
    {
        visible = true;
        fontSize = 14;
        nextWidget = -1;
        hasKeyboardFocus = false;
    }
    public void GetFocus()
    {
        hasKeyboardFocus = true;
        if (type == WidgetType.Textbox)
        {
            editing = true;
        }
    }
    public void LoseFocus()
    {
        hasKeyboardFocus = false;
        if (type == WidgetType.Textbox)
        {
            editing = false;
        }
    }
    internal string text;
    internal float x;
    internal float y;
    internal float sizex;
    internal float sizey;
    internal bool pressed;
    internal bool hover;
    internal WidgetType type;
    internal bool editing;
    internal bool visible;
    internal float fontSize;
    internal string description;
    internal bool password;
    internal bool selected;
    internal ButtonStyle buttonStyle;
    internal string image;
    internal int nextWidget;
    internal bool hasKeyboardFocus;
    internal int color;
    internal string id;
    internal bool isbutton;
    internal FontCi font;
}

public enum ButtonStyle
{
    Button,
    Text,
    ServerEntry
}

public class Model
{
}

public class ModelData
{
    internal int verticesCount;
    public int GetVerticesCount() { return verticesCount; }
    public void SetVerticesCount(int value) { verticesCount = value; }
    internal int indicesCount;
    public int GetIndicesCount() { return indicesCount; }
    public void SetIndicesCount(int value) { indicesCount = value; }
    internal float[] xyz;
    public int GetXyzCount() { return verticesCount * 3; }
    internal byte[] rgba;
    public int GetRgbaCount() { return verticesCount * 4; }
    internal float[] uv;
    public int GetUvCount() { return verticesCount * 2; }
    internal int[] indices;
    internal int mode;

    public float[] getXyz() { return xyz; }
    public void setXyz(float[] p) { xyz = p; }
    public byte[] getRgba() { return rgba; }
    public void setRgba(byte[] p) { rgba = p; }
    public float[] getUv() { return uv; }
    public void setUv(float[] p) { uv = p; }
    public int[] getIndices() { return indices; }
    public void setIndices(int[] p) { indices = p; }
    public int getMode() { return mode; }
    public void setMode(int p) { mode = p; }

    internal int verticesMax;
    internal int indicesMax;
}

public class ModelDataTool
{
    public static void AddVertex(ModelData model, float x, float y, float z, float u, float v, int color)
    {
        if (model.verticesCount >= model.verticesMax)
        {
            int xyzCount = model.GetXyzCount();
            float[] xyz = new float[xyzCount * 2];
            for (int i = 0; i < xyzCount; i++)
            {
                xyz[i] = model.xyz[i];
            }

            int uvCount = model.GetUvCount();
            float[] uv = new float[uvCount * 2];
            for (int i = 0; i < uvCount; i++)
            {
                uv[i] = model.uv[i];
            }

            int rgbaCount = model.GetRgbaCount();
            byte[] rgba = new byte[rgbaCount * 2];
            for (int i = 0; i < rgbaCount; i++)
            {
                rgba[i] = model.rgba[i];
            }

            model.xyz = xyz;
            model.uv = uv;
            model.rgba = rgba;
            model.verticesMax = model.verticesMax * 2;
        }
        model.xyz[model.GetXyzCount() + 0] = x;
        model.xyz[model.GetXyzCount() + 1] = y;
        model.xyz[model.GetXyzCount() + 2] = z;
        model.uv[model.GetUvCount() + 0] = u;
        model.uv[model.GetUvCount() + 1] = v;
        model.rgba[model.GetRgbaCount() + 0] = Game.IntToByte(Game.ColorR(color));
        model.rgba[model.GetRgbaCount() + 1] = Game.IntToByte(Game.ColorG(color));
        model.rgba[model.GetRgbaCount() + 2] = Game.IntToByte(Game.ColorB(color));
        model.rgba[model.GetRgbaCount() + 3] = Game.IntToByte(Game.ColorA(color));
        model.verticesCount++;
    }

    internal static void AddIndex(ModelData model, int index)
    {
        if (model.indicesCount >= model.indicesMax)
        {
            int indicesCount = model.indicesCount;
            int[] indices = new int[indicesCount * 2];
            for (int i = 0; i < indicesCount; i++)
            {
                indices[i] = model.indices[i];
            }
            model.indices = indices;
            model.indicesMax = model.indicesMax * 2;
        }
        model.indices[model.indicesCount++] = index;
    }
}

public class DrawModeEnum
{
    public const int Triangles = 0;
    public const int Lines = 1;
}

public class MainMenuNewFrameHandler : NewFrameHandler
{
    public static MainMenuNewFrameHandler Create(MainMenu l)
    {
        MainMenuNewFrameHandler h = new MainMenuNewFrameHandler();
        h.l = l;
        return h;
    }
    MainMenu l;
    public override void OnNewFrame(NewFrameEventArgs args)
    {
        l.OnNewFrame(args);
    }
}

public class MainMenuKeyEventHandler : KeyEventHandler
{
    public static MainMenuKeyEventHandler Create(MainMenu l)
    {
        MainMenuKeyEventHandler h = new MainMenuKeyEventHandler();
        h.l = l;
        return h;
    }
    MainMenu l;
    public override void OnKeyDown(KeyEventArgs e)
    {
        l.HandleKeyDown(e);
    }
    public override void OnKeyUp(KeyEventArgs e)
    {
        l.HandleKeyUp(e);
    }

    public override void OnKeyPress(KeyPressEventArgs e)
    {
        l.HandleKeyPress(e);
    }
}

public class MainMenuMouseEventHandler : MouseEventHandler
{
    public static MainMenuMouseEventHandler Create(MainMenu l)
    {
        MainMenuMouseEventHandler h = new MainMenuMouseEventHandler();
        h.l = l;
        return h;
    }
    MainMenu l;

    public override void OnMouseDown(MouseEventArgs e)
    {
        l.HandleMouseDown(e);
    }

    public override void OnMouseUp(MouseEventArgs e)
    {
        l.HandleMouseUp(e);
    }

    public override void OnMouseMove(MouseEventArgs e)
    {
        l.HandleMouseMove(e);
    }

    public override void OnMouseWheel(MouseWheelEventArgs e)
    {
        l.HandleMouseWheel(e);
    }
}

public class MainMenuTouchEventHandler : TouchEventHandler
{
    public static MainMenuTouchEventHandler Create(MainMenu l)
    {
        MainMenuTouchEventHandler h = new MainMenuTouchEventHandler();
        h.l = l;
        return h;
    }
    MainMenu l;

    public override void OnTouchStart(TouchEventArgs e)
    {
        l.HandleTouchStart(e);
    }

    public override void OnTouchMove(TouchEventArgs e)
    {
        l.HandleTouchMove(e);
    }

    public override void OnTouchEnd(TouchEventArgs e)
    {
        l.HandleTouchEnd(e);
    }
}

public class MainMenu
{
    public MainMenu()
    {
        one = 1;
        textures = new LoadedTexture[256];
        texturesCount = 0;
        textTextures = new TextTexture[256];
        textTexturesCount = 0;
        screen = new ScreenMain();
        screen.menu = this;
    }

    internal GamePlatform p;

    internal float one;

    public void Start(GamePlatform p_)
    {
        this.p = p_;

        xRot = 0;
        xSpeed = 0;

        yRot = 0;
        ySpeed = 0;

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
    }

    public void HandleKeyUp(KeyEventArgs e)
    {
        currentlyPressedKeys[e.GetKeyCode()] = false;
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

    void HandleKeys()
    {
        float elapsed_ = elapsed * 60;
        if (currentlyPressedKeys[33])
        {
            // Page Up
            z -= (one / 20) * elapsed_;
        }
        if (currentlyPressedKeys[34])
        {
            // Page Down
            z += (one / 20) * elapsed_;
        }
        if (currentlyPressedKeys[37])
        {
            // Left cursor key
            ySpeed -= 1 * elapsed_;
        }
        if (currentlyPressedKeys[39])
        {
            // Right cursor key
            ySpeed += 1 * elapsed_;
        }
        if (currentlyPressedKeys[38])
        {
            // Up cursor key
            xSpeed -= 1 * elapsed_;
        }
        if (currentlyPressedKeys[40])
        {
            // Down cursor key
            xSpeed += 1 * elapsed_;
        }
    }

    void DrawScene()
    {
        p.GlViewport(0, 0, viewportWidth, viewportHeight);
        p.GlClearColorBufferAndDepthBuffer();
        p.GlDisableDepthTest();
        {
            //Mat4.Perspective(pMatrix, 45, one * viewportWidth / viewportHeight, one / 100, one * 1000);
            //Mat4.Identity_(mvMatrix);
            //Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(0, 0, z));
        }
        {
            Mat4.Identity_(pMatrix);
            Mat4.Ortho(pMatrix, 0, p.GetCanvasWidth(), p.GetCanvasHeight(), 0, 0, 10);
        }

        screen.Render();
    }

    Screen screen;

    internal void DrawButton(string text, float fontSize, float dx, float dy, float dw, float dh, bool pressed)
    {
        Draw2dQuad(pressed ? GetTexture("button_sel.png") : GetTexture("button.png"), dx, dy, dw, dh);
        
        if (text != "")
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
        TextTexture tnew = p.CreateTextTexture(text, fontSize);
        textTextures[textTexturesCount++] = tnew;
        return tnew;
    }

    internal int GetTexture(string name)
    {
        for (int i = 0; i < texturesCount; i++)
        {
            if (textures[i].name == name)
            {
                return textures[i].texture;
            }
        }
        LoadedTexture t = new LoadedTexture();
        t.name = name;
        t.texture = p.LoadTextureFromFile(p.GetFullFilePath(name));
        textures[texturesCount++] = t;
        return t.texture;
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
        p.SetMatrixUniforms(pMatrix, mvMatrix);
    }

    float degToRad(float degrees)
    {
        return degrees * GlMatrixMath.PI() / 180;
    }

    float xRot;
    float xSpeed;

    float yRot;
    float ySpeed;

    float z;

    int filter;

    bool initialized;

    float elapsed;

    void Animate()
    {
        xRot += xSpeed * elapsed;
        yRot += ySpeed * elapsed;
    }

    public void OnNewFrame(NewFrameEventArgs args)
    {
        elapsed = args.GetDt();
        if (!initialized)
        {
            initialized = true;
            p.InitShaders();

            p.GlClearColorRgbaf(0, 0, 0, 1);
            p.GlEnableDepthTest();
        }
        viewportWidth = p.GetCanvasWidth();
        viewportHeight = p.GetCanvasHeight();
        HandleKeys();
        DrawScene();
        Animate();
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
        float dx = e.GetX() - previousMouseX;
        float dy = e.GetY() - previousMouseY;
        previousMouseX = e.GetX();
        previousMouseY = e.GetY();
        if (mousePressed)
        {
            ySpeed += dx / 10;
            xSpeed += dy / 10;
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
    }

    int touchId;
    int previousTouchX;
    int previousTouchY;

    public void HandleTouchMove(TouchEventArgs e)
    {
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
    }

    LoadedTexture[] textures;
    int texturesCount;
    TextTexture[] textTextures;
    int textTexturesCount;

    internal void StartSingleplayer()
    {
        screen = new ScreenSingleplayer();
        screen.menu = this;
    }

    internal void StartLogin()
    {
        screen = new ScreenLogin();
        screen.menu = this;
    }

    internal void Exit()
    {
        p.Exit();
    }

    internal void StartMainMenu()
    {
        screen = new ScreenMain();
        screen.menu = this;
    }

    internal void DrawBackground()
    {
        float scale = one * p.GetCanvasWidth() / 1280;
        Draw2dQuad(GetTexture("background.png"), 0, 0, 1280 * scale, 1280 * scale);
    }

    internal void StartMultiplayer()
    {
        screen = new ScreenMultiplayer();
        screen.menu = this;
    }

    internal void Login(string user, string password, LoginResultRef loginResult)
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
        return StringSubstring(s, StringLength(s) - StringLength(value), StringLength(value)) == value;
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

    public string StringAppend(string a, string b)
    {
        IntRef aLength = new IntRef();
        int[] aChars = p.StringToCharArray(a, aLength);
        IntRef bLength = new IntRef();
        int[] bChars = p.StringToCharArray(b, bLength);

        int[] cChars = new int[aLength.value + bLength.value];
        for (int i = 0; i < aLength.value; i++)
        {
            cChars[i] = aChars[i];
        }
        for (int i = 0; i < bLength.value; i++)
        {
            cChars[i + aLength.value] = bChars[i];
        }
        return p.CharArrayToString(cChars, aLength.value + bLength.value);
    }

    public string StringSubstring(string a, int start, int count)
    {
        IntRef aLength = new IntRef();
        int[] aChars = p.StringToCharArray(a, aLength);

        int[] bChars = new int[count];
        for (int i = 0; i < count; i++)
        {
            bChars[i] = aChars[start + i];
        }
        return p.CharArrayToString(bChars, count);
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

public class LoadedTexture
{
    internal string name;
    internal int texture;
}

public class Screen
{
    public Screen()
    {
        WidgetCount = 64;
        widgets = new MenuWidget[WidgetCount];
    }
    internal MainMenu menu;
    public virtual void Render() { }
    public virtual void OnKeyDown(KeyEventArgs e) {  }
    public virtual void OnKeyPress(KeyPressEventArgs e) { KeyPress(e); }
    public virtual void OnKeyUp(KeyEventArgs e) { }
    public virtual void OnTouchStart(TouchEventArgs e) { MouseDown(e.GetX(), e.GetY()); }
    public virtual void OnTouchMove(TouchEventArgs e) { }
    public virtual void OnTouchEnd(TouchEventArgs e) { MouseUp(e.GetX(), e.GetY()); }
    public virtual void OnMouseDown(MouseEventArgs e) { MouseDown(e.GetX(), e.GetY()); }
    public virtual void OnMouseUp(MouseEventArgs e) { MouseUp(e.GetX(), e.GetY()); }
    public virtual void OnMouseMove(MouseEventArgs e) { MouseMove(e); }
    public virtual void OnBackPressed() { }
   
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
                        string s = menu.CharToString(e.GetKeyChar());
                        if (e.GetKeyChar() == 8) // backspace
                        {
                            if (menu.StringLength(w.text) > 0)
                            {
                                w.text = menu.StringSubstring(w.text, 0, menu.StringLength(w.text) - 1);
                            }
                            return;
                        }
                        if (e.GetKeyChar() == 9 || e.GetKeyChar() == 13) // tab, enter
                        {
                            return;
                        }
                        w.text = menu.StringAppend(w.text, s);
                    }
                }
            }
        }
    }

    void MouseDown(int x, int y)
    {
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
                    w.editing = w.pressed;
                }
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
                    text = menu.StringAppend("&2", text);
                }
                if (w.type == WidgetType.Button)
                {
                    if (w.buttonStyle == ButtonStyle.Text)
                    {
                        menu.DrawText(text, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Left, TextBaseline.Middle);
                    }
                    else
                    {
                        menu.DrawButton(text, w.fontSize, w.x, w.y, w.sizex, w.sizey, w.pressed);
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
                        text = menu.StringAppend(text, "_");
                    }
                    if (w.buttonStyle == ButtonStyle.Text)
                    {
                        menu.DrawText(text, w.fontSize, w.x, w.y, TextAlign.Left, TextBaseline.Top);
                    }
                    else
                    {
                        menu.DrawButton(text, w.fontSize, w.x, w.y, w.sizex, w.sizey, w.pressed);
                    }
                }
                if (w.description != null)
                {
                    menu.DrawText(w.description, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Right, TextBaseline.Middle);
                }
            }
        }
    }

}


public class ScreenMain : Screen
{
    public ScreenMain()
    {
        singleplayer = new MenuWidget();
        multiplayer = new MenuWidget();
        widgets[0] = singleplayer;
        widgets[1] = multiplayer;
    }
    MenuWidget singleplayer;
    MenuWidget multiplayer;
    public override void Render()
    {
        GamePlatform p = menu.p;

        float scale = menu.one * p.GetCanvasWidth() / 1280;
        float size = menu.one * 80 / 100;
        menu.DrawBackground();
        menu.Draw2dQuad(menu.GetTexture("logo.png"), p.GetCanvasWidth() / 2 - 1280 * scale / 2 * size, 0, 1280 * scale * size, 460 * scale * size);

        singleplayer.text = "Singleplayer";
        singleplayer.x = p.GetCanvasWidth() / 2 - (256 + 100) * scale;
        singleplayer.y = p.GetCanvasHeight() * 7 / 10;
        singleplayer.sizex = 256 * scale;
        singleplayer.sizey = 64 * scale;

        multiplayer.text = "Multiplayer";
        multiplayer.x = p.GetCanvasWidth() / 2 + (100) * scale;
        multiplayer.y = p.GetCanvasHeight() * 7 / 10;
        multiplayer.sizex = 256 * scale;
        multiplayer.sizey = 64 * scale;
        DrawWidgets();
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == singleplayer)
        {
            menu.StartSingleplayer();
        }
        if (w == multiplayer)
        {
            menu.StartLogin();
        }
    }

    public override void OnBackPressed()
    {
        menu.Exit();
    }
}

public class ScreenSingleplayer : Screen
{
    public ScreenSingleplayer()
    {
        play = new MenuWidget();
        play.text = "Play";
        newWorld = new MenuWidget();
        newWorld.text = "New World";
        modify = new MenuWidget();
        modify.text = "Modify";
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;

        widgets[0] = play;
        widgets[1] = newWorld;
        widgets[2] = modify;
        widgets[3] = back;

        worldButtons = new MenuWidget[10];
        for (int i = 0; i < 10; i++)
        {
            worldButtons[i] = new MenuWidget();
            worldButtons[i].visible = false;
            widgets[4 + i] = worldButtons[i];
        }
    }

    MenuWidget newWorld;
    MenuWidget play;
    MenuWidget modify;
    MenuWidget back;

    MenuWidget[] worldButtons;

    string[] savegames;
    int savegamesCount;

    public override void Render()
    {
        GamePlatform p = menu.p;

        float scale = menu.one * p.GetCanvasWidth() / 1280;
        menu.DrawBackground();
        menu.DrawText("Singleplayer", 14 * scale, p.GetCanvasWidth() / 2, 0, TextAlign.Center, TextBaseline.Top);

        float leftx = p.GetCanvasWidth() / 2 - 128 * scale;
        float y = p.GetCanvasHeight() / 2 + 0 * scale;

        play.x = leftx;
        play.y = y + 100 * scale;
        play.sizex = 256 * scale;
        play.sizey = 64 * scale;
        play.fontSize = 14 * scale;

        newWorld.x = leftx;
        newWorld.y = y + 170 * scale;
        newWorld.sizex = 256 * scale;
        newWorld.sizey = 64 * scale;
        newWorld.fontSize = 14 * scale;

        modify.x = leftx;
        modify.y = y + 240 * scale;
        modify.sizex = 256 * scale;
        modify.sizey = 64 * scale;
        modify.fontSize = 14 * scale;

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;
        
        if (savegames == null)
        {
            IntRef savegamesCount_ = new IntRef();
            savegames = menu.GetSavegames(savegamesCount_);
            savegamesCount = savegamesCount_.value;
        }

        for (int i = 0; i < 10; i++)
        {
            worldButtons[i].visible = false;
        }
        for (int i = 0; i < savegamesCount; i++)
        {
            worldButtons[i].visible = true;
            worldButtons[i].text = menu.p.FileName(savegames[i]);
            worldButtons[i].x = leftx;
            worldButtons[i].y = 100 + 100 * scale * i;
            worldButtons[i].sizex = 256 * scale;
            worldButtons[i].sizey = 64 * scale;
            worldButtons[i].fontSize = 14 * scale;
        }

        DrawWidgets();
    }

    public override void OnBackPressed()
    {
        menu.StartMainMenu();
    }

    public override void OnButton(MenuWidget w)
    {
        for (int i = 0; i < 10; i++)
        {
            worldButtons[i].selected = false;
        }
        for (int i = 0; i < 10; i++)
        {
            if (worldButtons[i] == w)
            {
                worldButtons[i].selected = true;
            }
        }

        if (w == newWorld)
        {
            menu.StartNewWorld();
        }

        if (w == play)
        {
        }

        if (w == modify)
        {
            menu.StartModifyWorld();
        }

        if (w == back)
        {
            OnBackPressed();
        }
    }
}

public class ScreenModifyWorld : Screen
{
    public ScreenModifyWorld()
    {
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;

        widgets[0] = back;
    }

    MenuWidget back;

    public override void Render()
    {
        GamePlatform p = menu.p;

        float scale = menu.one * p.GetCanvasWidth() / 1280;
        menu.DrawBackground();
        menu.DrawText("Modify World", 14 * scale, menu.p.GetCanvasWidth() / 2, 0, TextAlign.Center, TextBaseline.Top);

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;

        DrawWidgets();
    }

    public override void OnBackPressed()
    {
        menu.StartSingleplayer();
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == back)
        {
            OnBackPressed();
        }
    }
}


public class ScreenLogin : Screen
{
    public ScreenLogin()
    {
        login = new MenuWidget();
        login.text = "Login";
        login.type = WidgetType.Button;
        loginUsername = new MenuWidget();
        loginUsername.type = WidgetType.Textbox;
        loginUsername.text = "";
        loginUsername.description = "Username";
        loginPassword = new MenuWidget();
        loginPassword.type = WidgetType.Textbox;
        loginPassword.text = "";
        loginPassword.description = "Password";
        loginPassword.password = true;
        loginRememberMe = new MenuWidget();
        loginRememberMe.text = "Yes";
        loginRememberMe.type = WidgetType.Button;
        loginRememberMe.description = "Remember me";

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

        widgets[0] = login;
        widgets[1] = loginUsername;
        widgets[2] = loginPassword;
        widgets[3] = loginRememberMe;
        widgets[4] = createAccount;
        widgets[5] = createAccountUsername;
        widgets[6] = createAccountPassword;
        widgets[7] = createAccountRememberMe;
        widgets[9] = back;

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

    public override void Render()
    {
        if (loginResult.value == LoginResult.Ok)
        {
            menu.StartMultiplayer();
        }

        GamePlatform p = menu.p;
        float scale = menu.one * p.GetCanvasWidth() / 1280;
        menu.DrawBackground();


        float leftx = p.GetCanvasWidth() / 2 - 400 * scale;
        float y = p.GetCanvasHeight() / 2 - 250 * scale;

        if (loginResult.value == LoginResult.Failed)
        {
            menu.DrawText("&4Invalid username or password", 14 * scale, leftx, y - 50 * scale, TextAlign.Left, TextBaseline.Top);
        }

        menu.DrawText("Login", 14 * scale, leftx, y + 50 * scale, TextAlign.Left, TextBaseline.Top);

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

        menu.DrawText("Create account", 14 * scale, rightx, y + 50 * scale, TextAlign.Left, TextBaseline.Top);

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

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;

        DrawWidgets();
    }

    public override void OnBackPressed()
    {
        menu.StartMainMenu();
    }

    LoginResultRef loginResult;

    public override void OnButton(MenuWidget w)
    {
        if (w == login)
        {
            menu.Login(loginUsername.text, loginPassword.text, loginResult);
        }
        if (w == createAccount)
        {
            menu.CreateAccount(createAccountUsername.text, createAccountPassword.text, loginResult);
        }
        if (w == loginRememberMe || w == createAccountRememberMe)
        {
            if (w.text == "Yes")
            {
                w.text = "No";
            }
            else
            {
                w.text = "Yes";
            }
        }
        if (w == back)
        {
            OnBackPressed();
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

public class ScreenMultiplayer : Screen
{
    public ScreenMultiplayer()
    {
        WidgetCount = 64 + serverButtonsCount;
        widgets = new MenuWidget[WidgetCount];
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;
        connect = new MenuWidget();
        connect.text = "Connect";
        connect.type = WidgetType.Button;
        refresh = new MenuWidget();
        refresh.text = "Refresh";
        refresh.type = WidgetType.Button;

        widgets[0] = back;
        widgets[1] = connect;
        widgets[2] = refresh;

        serverListAddress = new HttpResponseCi();
        serverListCsv = new HttpResponseCi();
        serversOnList = new ServerOnList[serversOnListCount];

        serverButtons = new MenuWidget[serverButtonsCount];
        for (int i = 0; i < serverButtonsCount; i++)
        {
            MenuWidget b = new MenuWidget();
            b = new MenuWidget();
            b.text = "Invalid";
            b.type = WidgetType.Button;
            b.visible = false;
            serverButtons[i] = b;
            widgets[3 + i] = b;
        }
    }

    bool loaded;
    HttpResponseCi serverListAddress;
    HttpResponseCi serverListCsv;
    ServerOnList[] serversOnList;
    const int serversOnListCount = 1024;


    public override void Render()
    {
        if (!loaded)
        {
            menu.p.WebClientDownloadStringAsync("http://manicdigger.sourceforge.net/serverlistcsv.txt", serverListAddress);
            loaded = true;
        }
        if (serverListAddress.done)
        {
            serverListAddress.done = false;
            menu.p.WebClientDownloadStringAsync(serverListAddress.value, serverListCsv);
        }
        if (serverListCsv.done)
        {
            serverListCsv.done = false;
            for (int i = 0; i < serversOnListCount; i++)
            {
                serversOnList[i] = null;
            }
            IntRef serversCount = new IntRef();
            string[] servers = menu.p.StringSplit(serverListCsv.value, "\n", serversCount);
            for (int i = 0; i < serversCount.value; i++)
            {
                IntRef ssCount = new IntRef();
                string[] ss = menu.p.StringSplit(servers[i], "\t", ssCount);
                if (ssCount.value < 10)
                {
                    continue;
                }
                ServerOnList s = new ServerOnList();
                s.hash = ss[0];
                s.name = ss[1];
                s.motd = ss[2];
                s.port = menu.p.IntParse(ss[3]);
                s.ip = ss[4];
                s.version = ss[5];
                s.users = menu.p.IntParse(ss[6]);
                s.max = menu.p.IntParse(ss[7]);
                s.gamemode = ss[8];
                s.players = ss[9];
                serversOnList[i] = s;
            }
        }

        GamePlatform p = menu.p;

        float scale = menu.one * p.GetCanvasWidth() / 1280;

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;

        connect.x = p.GetCanvasWidth() / 2 - 200 * scale;
        connect.y = p.GetCanvasHeight() - 104 * scale;
        connect.sizex = 256 * scale;
        connect.sizey = 64 * scale;
        connect.fontSize = 14 * scale;

        refresh.x = p.GetCanvasWidth() / 2 + 200 * scale;
        refresh.y = p.GetCanvasHeight() - 104 * scale;
        refresh.sizex = 256 * scale;
        refresh.sizey = 64 * scale;
        refresh.fontSize = 14 * scale;

        menu.DrawBackground();
        menu.DrawText("Multiplayer", 14 * scale, p.GetCanvasWidth() / 2, 0, TextAlign.Center, TextBaseline.Top);

        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].visible = false;
        }

        for (int i = 0; i < 10; i++)
        {
            ServerOnList s = serversOnList[i];
            if (s == null)
            {
                continue;
            }
            string t = menu.p.StringFormat2("{0}. {1}", menu.p.IntToString(i), s.name);
            t = menu.p.StringFormat2("{0} {1}", t, menu.p.IntToString(s.users));
            t = menu.p.StringFormat2("{0}/{1}", t, menu.p.IntToString(s.max));
            t = menu.p.StringFormat2("{0} {1}", t, s.gamemode);

            serverButtons[i].text = t;
            serverButtons[i].x = 100 * scale;
            serverButtons[i].y = 100 * scale + i * 50 * scale;
            serverButtons[i].sizex = 4 * 256 * scale;
            serverButtons[i].sizey = 64 * scale;
            serverButtons[i].fontSize = 14 * scale;
            serverButtons[i].visible = true;
            serverButtons[i].buttonStyle = ButtonStyle.Text;
        }

        DrawWidgets();
    }

    MenuWidget back;
    MenuWidget connect;
    MenuWidget refresh;
    MenuWidget[] serverButtons;
    const int serverButtonsCount = 1024;

    public override void OnBackPressed()
    {
        menu.StartMainMenu();
    }

    public override void OnButton(MenuWidget w)
    {
        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].selected = false;
            if (serverButtons[i] == w)
            {
                serverButtons[i].selected = true;
            }
        }
        if (w == back)
        {
            OnBackPressed();
        }
        if (w == connect)
        {
        }
        if (w == refresh)
        {
            loaded = false;
        }
    }
}

public class HttpResponseCi
{
    internal bool done;
    internal string value;
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
    }
    internal string text;
    internal float x;
    internal float y;
    internal float sizex;
    internal float sizey;
    internal bool pressed;
    internal WidgetType type;
    internal bool editing;
    internal bool visible;
    internal float fontSize;
    internal string description;
    internal bool password;
    internal bool selected;
    internal ButtonStyle buttonStyle;
}

public enum ButtonStyle
{
    Button,
    Text
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

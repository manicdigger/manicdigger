/// <summary>
/// MainMenu acts as the entry point to the game. It manages different states, screens and forwards user input.
/// </summary>
public class MainMenu
{
	public MainMenu()
	{
		uiRenderer = new UiRenderer();
		loginClient = new LoginClientCi();
		pMatrix = Mat4.Create();
	}

	internal GamePlatform p;
	internal LanguageCi lang;
	internal UiRenderer uiRenderer;
	bool drawBackground;
	AnimatedBackgroundWidget background;

	float[] pMatrix;
	int viewportWidth;
	int viewportHeight;
	MainMenuScreen screen;
	bool initialized;
	LoginClientCi loginClient;

	public void Start(GamePlatform p_)
	{
		this.p = p_;

		// initialize ui renderer
		uiRenderer.Init(p);

		// initialize background
		background = new AnimatedBackgroundWidget();
		background.Init("background.png", 512, 512);

		// initialize translations
		lang = new LanguageCi();
		lang.platform = p;
		lang.LoadTranslations();
		p.SetTitle(lang.GameName());

		StartMainMenu();

		p.AddOnNewFrame(MainMenuNewFrameHandler.Create(this));
		p.AddOnKeyEvent(MainMenuKeyEventHandler.Create(this));
		p.AddOnMouseEvent(MainMenuMouseEventHandler.Create(this));
		p.AddOnTouchEvent(MainMenuTouchEventHandler.Create(this));
	}

	public void HandleKeyDown(KeyEventArgs e)
	{
		screen.OnKeyDown(e);
	}

	public void HandleKeyUp(KeyEventArgs e)
	{
		screen.OnKeyUp(e);
	}

	public void HandleKeyPress(KeyPressEventArgs e)
	{
		screen.OnKeyPress(e);
	}

	void DrawScene(float dt)
	{
		if (uiRenderer.GetAssetLoadProgress().value != 1)
		{
			// skip rendering while assets have not been loaded completely
			// prevents texture loading issues in WebGL
			return;
		}
		// update projection matrix and viewport
		Mat4.Identity_(pMatrix);
		Mat4.Ortho(pMatrix, 0, viewportWidth, viewportHeight, 0, 0, 10);
		p.SetMatrixUniformProjection(pMatrix);
		p.GlViewport(0, 0, viewportWidth, viewportHeight);
		p.GlClearColorBufferAndDepthBuffer();
		p.GlDisableCullFace();

		if (drawBackground) { background.Draw(dt, uiRenderer); }
		screen.Render(dt);
	}

	public void OnNewFrame(NewFrameEventArgs args)
	{
		if (!initialized)
		{
			initialized = true;
			p.InitShaders();
			p.GlClearColorRgbaf(0, 0, 0, 1);
		}
		viewportWidth = p.GetCanvasWidth();
		viewportHeight = p.GetCanvasHeight();
		DrawScene(args.GetDt());
		loginClient.Update(p);
	}

	public void HandleMouseDown(MouseEventArgs e)
	{
		screen.OnMouseDown(e);
	}

	public void HandleMouseUp(MouseEventArgs e)
	{
		screen.OnMouseUp(e);
	}

	public void HandleMouseMove(MouseEventArgs e)
	{
		screen.OnMouseMove(e);
	}

	public void HandleMouseWheel(MouseWheelEventArgs e)
	{
		screen.OnMouseWheel(e);
	}

	public void HandleTouchStart(TouchEventArgs e)
	{
		screen.OnTouchStart(e);
	}

	public void HandleTouchMove(TouchEventArgs e)
	{
		screen.OnTouchMove(e);
	}

	public void HandleTouchEnd(TouchEventArgs e)
	{
		screen.OnTouchEnd(e);
	}

	internal void StartSingleplayer()
	{
		screen = new ScreenSingleplayer();
		screen.Init(this, uiRenderer);
	}

	internal void StartLogin(string serverHash, string ip, int port)
	{
		ScreenLogin screenLogin = new ScreenLogin();
		screenLogin.serverHash = serverHash;
		screenLogin.serverIp = ip;
		screenLogin.serverPort = port;
		screen = screenLogin;
		screen.Init(this, uiRenderer);
	}

	internal void StartConnectToIp()
	{
		ScreenConnectToIp screenConnectToIp = new ScreenConnectToIp();
		screen = screenConnectToIp;
		screen.Init(this, uiRenderer);
	}

	internal void Exit()
	{
		p.Exit();
	}

	internal void StartMainMenu()
	{
		screen = new ScreenMain();
		screen.Init(this, uiRenderer);
		p.ExitMousePointerLock();
		p.SetVSync(true);
		drawBackground = true;
	}

	internal void StartMultiplayer()
	{
		screen = new ScreenMultiplayer();
		screen.Init(this, uiRenderer);
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
			if (StringEndsWith(files[i], ".mddbs"))
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

	internal void StartNewWorld()
	{
	}

	internal void StartModifyWorld()
	{
	}

	public void StartGame(bool singleplayer, string singleplayerSavePath, ConnectData connectData)
	{
		ScreenGame screenGame = new ScreenGame();
		screenGame.Init(this, uiRenderer);
		screenGame.Start(p, singleplayer, singleplayerSavePath, connectData);
		screen = screenGame;
		drawBackground = false;
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
}

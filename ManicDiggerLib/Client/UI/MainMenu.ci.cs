public class MainMenu
{
	public MainMenu()
	{
		one = 1;
		uiRenderer = new UiRenderer();
		loginClient = new LoginClientCi();
		fontMenuHeading = new FontCi();
		fontMenuHeading.size = 20;
		pMatrix = Mat4.Create();
	}

	internal GamePlatform p;
	internal LanguageCi lang;
	internal FontCi fontMenuHeading;
	internal UiRenderer uiRenderer;

	internal float one;
	float[] pMatrix;

	public void Start(GamePlatform p_)
	{
		this.p = p_;

		// initialize ui renderer
		Mat4.Identity_(pMatrix);
		Mat4.Ortho(pMatrix, 0, p.GetCanvasWidth(), p.GetCanvasHeight(), 0, 0, 10);
		uiRenderer.Init(p);

		//Initialize translations
		lang = new LanguageCi();
		lang.platform = p;
		lang.LoadTranslations();
		p.SetTitle(lang.GameName());

		StartMainMenu();

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

		currentlyPressedKeys = new bool[256];
		p.AddOnNewFrame(MainMenuNewFrameHandler.Create(this));
		p.AddOnKeyEvent(MainMenuKeyEventHandler.Create(this));
		p.AddOnMouseEvent(MainMenuMouseEventHandler.Create(this));
		p.AddOnTouchEvent(MainMenuTouchEventHandler.Create(this));
	}

	int viewportWidth;
	int viewportHeight;

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
			//Mat4.Identity_(pMatrix);
			//Mat4.Ortho(pMatrix, 0, p.GetCanvasWidth(), p.GetCanvasHeight(), 0, 0, 10);
		}

		screen.Render(dt);
	}

	Screen screen;

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

	internal void StartSingleplayer()
	{
		screen = new ScreenSingleplayer();
		screen.menu = this;
		screen.uiRenderer = uiRenderer;
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
		screen.uiRenderer = uiRenderer;
		screen.LoadTranslations();
	}

	internal void StartConnectToIp()
	{
		ScreenConnectToIp screenConnectToIp = new ScreenConnectToIp();
		screen = screenConnectToIp;
		screen.menu = this;
		screen.uiRenderer = uiRenderer;
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
		screen.uiRenderer = uiRenderer;
		screen.LoadTranslations();
		p.ExitMousePointerLock();
		p.SetVSync(true);
		p.SetMatrixUniformProjection(pMatrix);
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
				uiRenderer.Draw2dTexture(uiRenderer.GetTexture("background.png"), x * backgroundW + xRot - overlap, y * backgroundH + yRot - overlap, backgroundW, backgroundH, null, 0, Game.ColorFromArgb(255, 255, 255, 255));
			}
		}
	}

	internal void StartMultiplayer()
	{
		screen = new ScreenMultiplayer();
		screen.menu = this;
		screen.uiRenderer = uiRenderer;
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
		screenGame.menu = this;
		screenGame.uiRenderer = uiRenderer;
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
}

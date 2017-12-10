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
		fontMenuHeading = new FontCi();
		fontMenuHeading.size = 20;
	}

	internal GamePlatform p;
	internal LanguageCi lang;
	internal FontCi fontMenuHeading;

	internal float one;

	internal AssetList assets;
	internal FloatRef assetsLoadProgress;
	internal TextColorRenderer textColorRenderer;

	public void Start(GamePlatform p_)
	{
		this.p = p_;

		//Initialize translations
		lang = new LanguageCi();
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

	internal void DrawButton(string text, FontCi font, float dx, float dy, float dw, float dh, bool pressed)
	{
		Draw2dQuad(pressed ? GetTexture("button_sel.png") : GetTexture("button.png"), dx, dy, dw, dh);

		if ((text != null) && (text != ""))
		{
			DrawText(text, font, dx + dw / 2, dy + dh / 2, TextAlign.Center, TextBaseline.Middle);
		}
	}

	internal void DrawText(string text, FontCi font, float x, float y, TextAlign align, TextBaseline baseline)
	{
		TextTexture t = GetTextTexture(text, font);
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

	internal TextTexture GetTextTexture(string text, FontCi font)
	{
		for (int i = 0; i < textTexturesCount; i++)
		{
			TextTexture t = textTextures[i];
			if (t == null)
			{
				continue;
			}
			if (t.text == text
				&& t.font.size == font.size
				&& t.font.family == font.family
				&& t.font.style == font.style)
			{
				return t;
			}
		}
		TextTexture textTexture = new TextTexture();

		Text_ text_ = new Text_();
		text_.text = text;
		text_.font = font;
		text_.color = Game.ColorFromArgb(255, 255, 255, 255);
		BitmapCi textBitmap = textColorRenderer.CreateTextTexture(text_);

		int texture = p.LoadTextureFromBitmap(textBitmap);

		IntRef textWidth = new IntRef();
		IntRef textHeight = new IntRef();
		p.TextSize(text, font, textWidth, textHeight);

		textTexture.texture = texture;
		textTexture.texturewidth = p.FloatToInt(p.BitmapGetWidth(textBitmap));
		textTexture.textureheight = p.FloatToInt(p.BitmapGetHeight(textBitmap));
		textTexture.text = text;
		textTexture.font = font;
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
		p.GLDisableAlphaTest();
		p.DrawModel(cubeModel);
		p.GLEnableAlphaTest();
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
		p.SetVSync(true);
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

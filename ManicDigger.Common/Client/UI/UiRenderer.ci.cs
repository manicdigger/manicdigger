public class UiRenderer
{
	AssetList _assetList;
	DictionaryStringInt1024 _textures;
	FloatRef _assetLoadProgress;
	GamePlatform _p;
	Model _cubeModel;
	TextColorRenderer _textRenderer;
	TextTexture[] _textTextures;

	int textTexturesCount;
	float[] mvMatrix;

	public UiRenderer()
	{
		_textures = new DictionaryStringInt1024();
		_textTextures = new TextTexture[256];
		textTexturesCount = 0;
		_assetList = new AssetList();
		_assetLoadProgress = new FloatRef();
		_textRenderer = new TextColorRenderer();
		mvMatrix = Mat4.Create();
	}

	public void Init(GamePlatform p)
	{
		_p = p;
		_textRenderer.platform = _p;
		_p.LoadAssetsAsyc(_assetList, _assetLoadProgress);
	}

	internal TextTexture GetTextTexture(string text, FontCi font)
	{
		for (int i = 0; i < textTexturesCount; i++)
		{
			TextTexture t = _textTextures[i];
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
		text_.color = ColorCi.FromArgb(255, 255, 255, 255);
		BitmapCi textBitmap = _textRenderer.CreateTextTexture(text_);

		int texture = _p.LoadTextureFromBitmap(textBitmap);

		IntRef textWidth = new IntRef();
		IntRef textHeight = new IntRef();
		_p.TextSize(text, font, textWidth, textHeight);

		textTexture.texture = texture;
		textTexture.texturewidth = _p.FloatToInt(_p.BitmapGetWidth(textBitmap));
		textTexture.textureheight = _p.FloatToInt(_p.BitmapGetHeight(textBitmap));
		textTexture.text = text;
		textTexture.font = font;
		textTexture.textwidth = textWidth.value;
		textTexture.textheight = textHeight.value;

		_p.BitmapDelete(textBitmap);

		_textTextures[textTexturesCount++] = textTexture;
		return textTexture;
	}

	internal int LoadBitmap(BitmapCi data, string name)
	{
		int texture = _p.LoadTextureFromBitmap(data);
		_textures.Set(name, texture);
		return texture;
	}
	internal int GetTexture(string name)
	{
		if (!_textures.Contains(name))
		{
			BoolRef found = new BoolRef();
			BitmapCi bmp = _p.BitmapCreateFromPng(GetFile(name), GetFileLength(name));
			int texture = _p.LoadTextureFromBitmap(bmp);
			_textures.Set(name, texture);
			_p.BitmapDelete(bmp);
		}
		return _textures.Get(name);
	}

	internal byte[] GetFile(string name)
	{
		string pLowercase = _p.StringToLower(name);
		for (int i = 0; i < _assetList.count; i++)
		{
			if (_assetList.items[i].name == pLowercase)
			{
				return _assetList.items[i].data;
			}
		}
		return null;
	}

	internal int GetFileLength(string name)
	{
		string pLowercase = _p.StringToLower(name);
		for (int i = 0; i < _assetList.count; i++)
		{
			if (_assetList.items[i].name == pLowercase)
			{
				return _assetList.items[i].dataLength;
			}
		}
		return 0;
	}

	/// <summary>
	/// Draw a texture to the screen using advanced options such as texture coordinates and color.
	/// This method assumes OpenGL to be in orthographic mode.
	/// </summary>
	/// <param name="textureId">OpenGL texture ID</param>
	/// <param name="dx">Target X screen coordinate</param>
	/// <param name="dy">Target Y screen coordinate</param>
	/// <param name="dw">Target width in pixels</param>
	/// <param name="dh">Target height in pixels</param>
	/// <param name="atlasId">0 indexed ID of the texture to lookup in the atlas. If set to null atlas functionality will be disabled.</param>
	/// <param name="atlasTotal">Total number of textures in the atlas. Has no effect if atlasId is null.</param>
	/// <param name="color">Color to use when drawing the texture.</param>
	public void Draw2dTexture(int textureId, float dx, float dy, float dw, float dh, IntRef atlasId, int atlasTotal, int color)
	{
		_p.GlDisableCullFace();
		_p.GLDisableAlphaTest();
		_p.GlDisableDepthTest();
		if (color == ColorCi.FromArgb(255, 255, 255, 255) && atlasId == null)
		{
			Draw2dTextureSimple(textureId, dx, dy, dw, dh);
		}
		else
		{
			Draw2dTextureInAtlas(textureId, dx, dy, dw, dh, atlasId, atlasTotal, color);
		}
		_p.GlEnableDepthTest();
		_p.GLEnableAlphaTest();
		_p.GlEnableCullFace();
	}

	/// <summary>
	/// Draw a texture to the screen.
	/// This method assumes OpenGL to be in orthographic mode.
	/// </summary>
	/// <param name="textureId">OpenGL texture ID</param>
	/// <param name="dx">Target X screen coordinate</param>
	/// <param name="dy">Target Y screen coordinate</param>
	/// <param name="dw">Target width in pixels</param>
	/// <param name="dh">Target height in pixels</param>
	public void Draw2dTextureSimple(int textureId, float dx, float dy, float dw, float dh)
	{
		if (_cubeModel == null)
		{
			_cubeModel = _p.CreateModel(QuadModelData.GetQuadModelData());
		}
		Mat4.Identity_(mvMatrix);
		Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(dx, dy, 0));
		Mat4.Scale(mvMatrix, mvMatrix, Vec3.FromValues(dw, dh, 0));
		Mat4.Scale(mvMatrix, mvMatrix, Vec3.FromValues(0.5f, 0.5f, 0));
		Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(1, 1, 0));
		SetMatrixUniformModelView();

		_p.BindTexture2d(textureId);
		_p.DrawModel(_cubeModel);
	}

	/// <summary>
	/// Draw a texture to the screen using advanced options such as texture coordinates and color.
	/// This method assumes OpenGL to be in orthographic mode.
	/// </summary>
	/// <param name="textureId">OpenGL texture ID</param>
	/// <param name="dx">Target X screen coordinate</param>
	/// <param name="dy">Target Y screen coordinate</param>
	/// <param name="dw">Target width in pixels</param>
	/// <param name="dh">Target height in pixels</param>
	/// <param name="atlasId">0 indexed ID of the texture to lookup in the atlas. If set to null atlas functionality will be disabled.</param>
	/// <param name="atlasTotal">Total number of textures in the atlas. Has no effect if atlasId is null.</param>
	/// <param name="color">Color to use when drawing the texture.</param>
	public void Draw2dTextureInAtlas(int textureId, float dx, float dy, float dw, float dh, IntRef atlasId, int atlasTotal, int color)
	{
		RectFRef rect = RectFRef.Create(0, 0, 1, 1);
		if (atlasId != null)
		{
			TextureAtlasCi.TextureCoords2d(atlasId.value, atlasTotal, rect);
		}
		Mat4.Identity_(mvMatrix);
		SetMatrixUniformModelView();

		_p.BindTexture2d(textureId);
		ModelData data =
			QuadModelData.GetQuadModelData2(rect.x, rect.y, rect.w, rect.h,
			dx, dy, dw, dh,
			ConvertCi.IntToByte(ColorCi.ExtractR(color)), ConvertCi.IntToByte(ColorCi.ExtractG(color)), ConvertCi.IntToByte(ColorCi.ExtractB(color)), ConvertCi.IntToByte(ColorCi.ExtractA(color)));
		_p.DrawModelData(data);
	}

	public int GetWhiteTextureId()
	{
		if (whitetexture == -1)
		{
			BitmapCi bmp = _p.BitmapCreate(1, 1);
			int[] pixels = new int[1];
			pixels[0] = ColorCi.FromArgb(255, 255, 255, 255);
			_p.BitmapSetPixelsArgb(bmp, pixels);
			whitetexture = _p.LoadTextureFromBitmap(bmp);
			_p.BitmapDelete(bmp);
		}
		return whitetexture;
	}
	int whitetexture;

	void SetMatrixUniformModelView()
	{
		_p.SetMatrixUniformModelView(mvMatrix);
	}

	public float GetScale()
	{
		float scale;
		if (_p.IsSmallScreen())
		{
			scale = 1.0f * _p.GetCanvasWidth() / 1280;
		}
		else
		{
			scale = 1.0f;
		}
		return scale;
	}

	public GamePlatform GetPlatform()
	{
		return _p;
	}
	public AssetList GetAssetList()
	{
		return _assetList;
	}
	public FloatRef GetAssetLoadProgress()
	{
		return _assetLoadProgress;
	}
}

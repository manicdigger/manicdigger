public class ServerButtonWidget : AbstractMenuWidget
{
	string _name;
	string _motd;
	string _gamemode;
	string _playercount;

	string _imagename;
	bool _errorVersion;
	bool _errorConnect;

	TextWidget _textHeading;
	TextWidget _textGamemode;
	TextWidget _textPlayercount;
	TextWidget _textDescription;
	FontCi _fontServerHeading;
	FontCi _fontServerDescription;

	public ServerButtonWidget()
	{
		x = 0;
		y = 0;
		sizex = 0;
		sizey = 0;
		clickable = true;
		focusable = true;

		_name = null;
		_motd = null;
		_gamemode = null;
		_playercount = null;
		_imagename = "serverlist_entry_noimage.png";
		_errorVersion = false;
		_errorConnect = false;

		_fontServerHeading = new FontCi();
		_fontServerHeading.style = 1;
		_fontServerHeading.size = 14;
		_fontServerDescription = new FontCi();

		_textHeading = new TextWidget();
		_textHeading.SetFont(_fontServerHeading);
		_textHeading.SetAlignment(TextAlign.Left);
		_textHeading.SetBaseline(TextBaseline.Top);
		_textGamemode = new TextWidget();
		_textGamemode.SetFont(_fontServerDescription);
		_textGamemode.SetAlignment(TextAlign.Right);
		_textGamemode.SetBaseline(TextBaseline.Bottom);
		_textPlayercount = new TextWidget();
		_textPlayercount.SetFont(_fontServerDescription);
		_textPlayercount.SetAlignment(TextAlign.Right);
		_textPlayercount.SetBaseline(TextBaseline.Top);
		_textDescription = new TextWidget();
		_textDescription.SetFont(_fontServerDescription);
		_textDescription.SetAlignment(TextAlign.Left);
		_textDescription.SetBaseline(TextBaseline.Bottom);
	}
	//public ServerButtonWidget(float dx, float dy, float dw, float dh, string name, string motd, string gamemode, string playercount)
	//{
	//	x = dx;
	//	y = dy;
	//	sizex = dw;
	//	sizey = dh;
	//	_imagename = "serverlist_entry_noimage.png";

	//	_textHeading = new TextWidget(x + 70, y + 5, _name, _fontServerHeading, TextAlign.Left, TextBaseline.Top);
	//	_textGamemode = new TextWidget(x + sizex - 10, y + sizey - 5, _gamemode, _fontServerDescription, TextAlign.Right, TextBaseline.Bottom);
	//	_textPlayercount = new TextWidget(x + sizex - 10, y + 5, _playercount, _fontServerDescription, TextAlign.Right, TextBaseline.Top);
	//	_textDescription = new TextWidget(x + 70, y + sizey - 5, _motd, _fontServerDescription, TextAlign.Left, TextBaseline.Bottom);
	//}

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex <= 0 || sizey <= 0) { return; }

		renderer.Draw2dTexture(renderer.GetTexture("serverlist_entry_background.png"), x, y, sizex, sizey, null, 0, color);
		renderer.Draw2dTexture(renderer.GetTexture(_imagename), x, y, sizey, sizey, null, 0, color);

		// display warnings if server is unreachable or uses a different version
		if (_errorConnect)
		{
			renderer.Draw2dTexture(renderer.GetTexture("serverlist_entry_noresponse.png"), x - 38 * renderer.GetScale(), y, sizey / 2, sizey / 2, null, 0, color);
		}
		if (_errorVersion)
		{
			renderer.Draw2dTexture(renderer.GetTexture("serverlist_entry_differentversion.png"), x - 38 * renderer.GetScale(), y + sizey / 2, sizey / 2, sizey / 2, null, 0, color);
		}

		// highlight text if button is selected
		if (hasKeyboardFocus)
		{
			_textHeading.SetText(StringTools.StringAppend(renderer.GetPlatform(), "&2", _name));
			_textGamemode.SetText(StringTools.StringAppend(renderer.GetPlatform(), "&2", _gamemode));
			_textPlayercount.SetText(StringTools.StringAppend(renderer.GetPlatform(), "&2", _playercount));
			_textDescription.SetText(StringTools.StringAppend(renderer.GetPlatform(), "&2", _motd));
		}
		else
		{
			_textHeading.SetText(_name);
			_textGamemode.SetText(_gamemode);
			_textPlayercount.SetText(_playercount);
			_textDescription.SetText(_motd);
		}

		float scale = renderer.GetScale();
		_textHeading.x = x + 70 * scale;
		_textHeading.y = y + 5 * scale;
		_textHeading.Draw(dt, renderer);
		_textGamemode.x = x + sizex - 10 * scale;
		_textGamemode.y = y + sizey - 5 * scale;
		_textGamemode.Draw(dt, renderer);
		_textPlayercount.x = x + sizex - 10 * scale;
		_textPlayercount.y = y + 5 * scale;
		_textPlayercount.Draw(dt, renderer);
		_textDescription.x = x + 70 * scale;
		_textDescription.y = y + sizey - 5 * scale;
		_textDescription.Draw(dt, renderer);
	}
	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (HasBeenClicked(args))
		{
			hasKeyboardFocus = true;
		}
		else
		{
			hasKeyboardFocus = false;
		}
	}

	public string GetTextHeading()
	{
		return _name;
	}
	public void SetTextHeading(string text)
	{
		_name = text;
	}

	public string GetTextGamemode()
	{
		return _gamemode;
	}
	public void SetTextGamemode(string text)
	{
		_gamemode = text;
	}

	public string GetTextPlayercount()
	{
		return _playercount;
	}
	public void SetTextPlayercount(string text)
	{
		_playercount = text;
	}

	public string GetTextDescription()
	{
		return _motd;
	}
	public void SetTextDescription(string text)
	{
		_motd = text;
	}

	public string GetThumbnail()
	{
		return _imagename;
	}
	public void SetThumbnail(string image)
	{
		if (image == null || image == "")
		{
			_imagename = "serverlist_entry_noimage.png";
		}
		else
		{
			_imagename = image;
		}
	}

	public bool GetErrorConnect()
	{
		return _errorConnect;
	}
	public void SetErrorConnect(bool error)
	{
		_errorConnect = error;
	}

	public bool GetErrorVersion()
	{
		return _errorVersion;
	}
	public void SetErrorVersion(bool error)
	{
		_errorVersion = error;
	}
}

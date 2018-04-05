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
		_imagename = "serverlist_entry_noimage.png";

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

	public override void Draw(MainMenu m)
	{
		if (!visible) { return; }
		m.Draw2dQuad(m.GetTexture("serverlist_entry_background.png"), x, y, sizex, sizey);
		m.Draw2dQuad(m.GetTexture(_imagename), x, y, sizey, sizey);

		// highlight text if button is selected
		if (hasKeyboardFocus)
		{
			_textHeading.SetText(StringTools.StringAppend(m.p, "&2", _name));
			_textGamemode.SetText(StringTools.StringAppend(m.p, "&2", _gamemode));
			_textPlayercount.SetText(StringTools.StringAppend(m.p, "&2", _playercount));
			_textDescription.SetText(StringTools.StringAppend(m.p, "&2", _motd));
		}
		else
		{
			_textHeading.SetText(_name);
			_textGamemode.SetText(_gamemode);
			_textPlayercount.SetText(_playercount);
			_textDescription.SetText(_motd);
		}

		_textHeading.x = x + 70;
		_textHeading.y = y + 5;
		_textHeading.Draw(m);
		_textGamemode.x = x + sizex - 10;
		_textGamemode.y = y + sizey - 5;
		_textGamemode.Draw(m);
		_textPlayercount.x = x + sizex - 10;
		_textPlayercount.y = y + 5;
		_textPlayercount.Draw(m);
		_textDescription.x = x + 70;
		_textDescription.y = y + sizey - 5;
		_textDescription.Draw(m);
	}

	public void SetTextHeading(string text)
	{
		_name = text;
	}
	public void SetTextGamemode(string text)
	{
		_gamemode = text;
	}
	public void SetTextPlayercount(string text)
	{
		_playercount = text;
	}
	public void SetTextDescription(string text)
	{
		_motd = text;
	}
	public void SetServerImage(string image)
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
}

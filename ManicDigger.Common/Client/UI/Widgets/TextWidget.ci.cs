public class TextWidget : AbstractMenuWidget
{
	string _text;
	FontCi _font;
	TextAlign _align;
	TextBaseline _baseline;

	float _offsetX;
	float _offsetY;
	TextTexture _texture;

	public TextWidget()
	{
		x = 0;
		y = 0;
		_offsetX = 0;
		_offsetY = 0;
		_align = TextAlign.Left;
		_baseline = TextBaseline.Top;
	}
	//public TextWidget(float dx, float dy, string text, FontCi font, TextAlign align, TextBaseline baseline)
	//{
	//	x = dx;
	//	y = dy;
	//	_offsetX = 0;
	//	_offsetY = 0;
	//	_font = font;
	//	_align = align;
	//	_baseline = baseline;
	//}

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (_text == null || _font == null) { return; }
		// Only create/lookup new text texture when needed
		if (_texture == null)
		{
			// Create new text texture
			_texture = renderer.GetTextTexture(_text, _font);

			// Calculate baseline and alignment offset
			UpdateOffset_Alignment();
			UpdateOffset_Baseline();

			// Set widget size 
			sizex = _texture.textwidth;
			sizey = _texture.textheight;
		}

		renderer.Draw2dTexture(_texture.texture, x + _offsetX, y + _offsetY, _texture.texturewidth, _texture.textureheight, null, 0, color);
	}

	public TextAlign GetAlignment()
	{
		return _align;
	}
	public void SetAlignment(TextAlign align)
	{
		_align = align;
		UpdateOffset_Alignment();
	}

	public TextBaseline GetBaseline()
	{
		return _baseline;
	}
	public void SetBaseline(TextBaseline baseline)
	{
		_baseline = baseline;
		UpdateOffset_Baseline();
	}

	public FontCi GetFont()
	{
		return _font;
	}
	public void SetFont(FontCi font)
	{
		if (font == null) { return; }
		_font = font;
		_texture = null;
	}

	public void SetX(float dx)
	{
		if (x == dx) { return; }
		x = dx;
		UpdateOffset_Alignment();
	}

	public void SetY(float dy)
	{
		if (y == dy) { return; }
		y = dy;
		UpdateOffset_Baseline();
	}

	public string GetText()
	{
		return _text;
	}
	public void SetText(string text)
	{
		if (text == _text) { return; }
		_text = text;
		_texture = null;
	}

	void UpdateOffset_Alignment()
	{
		if (_texture == null) { return; }
		_offsetX = 0;
		switch (_align)
		{
			case TextAlign.Left:
				break;
			case TextAlign.Center:
				_offsetX -= _texture.textwidth / 2;
				break;
			case TextAlign.Right:
				_offsetX -= _texture.textwidth;
				break;
		}
	}

	void UpdateOffset_Baseline()
	{
		if (_texture == null) { return; }
		_offsetY = 0;
		switch (_baseline)
		{
			case TextBaseline.Top:
				break;
			case TextBaseline.Middle:
				_offsetY -= _texture.textheight / 2;
				break;
			case TextBaseline.Bottom:
				_offsetY -= _texture.textheight;
				break;
		}
	}
}

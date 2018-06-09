public class ButtonWidget : AbstractMenuWidget
{
	TextWidget _text;
	ButtonState _state;
	string _textureNameIdle;
	string _textureNameHover;
	string _textureNamePressed;

	public ButtonWidget()
	{
		_state = ButtonState.Normal;
		_textureNameIdle = "button.png";
		_textureNameHover = "button_sel.png";
		_textureNamePressed = "button_sel.png";
		x = 0;
		y = 0;
		sizex = 0;
		sizey = 0;
		clickable = true;
		focusable = true;
	}
	//public ButtonWidget(float dx, float dy, float dw, float dh, string text, FontCi font)
	//{
	//	_state = ButtonState.Normal;
	//	x = dx;
	//	y = dy;
	//	sizex = dw;
	//	sizey = dh;
	//	if ((text != null) && (text != ""))
	//	{
	//		_text = new TextWidget(x + sizex / 2, y + sizey / 2, text, font, TextAlign.Center, TextBaseline.Middle);
	//	}
	//}

	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (IsCursorInside(args))
		{
			SetState(ButtonState.Pressed);
		}
		else
		{
			SetState(ButtonState.Normal);
		}
	}

	public override void OnMouseUp(GamePlatform p, MouseEventArgs args)
	{
		if (IsCursorInside(args))
		{
			SetState(ButtonState.Hover);
		}
		else
		{
			SetState(ButtonState.Normal);
		}
	}

	public override void OnMouseMove(GamePlatform p, MouseEventArgs args)
	{
		// Check if mouse is inside the button rectangle
		if (IsCursorInside(args))
		{
			if (_state == ButtonState.Normal)
			{
				SetState(ButtonState.Hover);
			}
		}
		else
		{
			SetState(ButtonState.Normal);
		}
	}

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex <= 0 || sizey <= 0) { return; }
		switch (hasKeyboardFocus ? ButtonState.Hover : _state)
		{
			// TODO: Use atlas textures
			case ButtonState.Normal:
				renderer.Draw2dTexture(renderer.GetTexture(_textureNameIdle), x, y, sizex, sizey, null, 0, color);
				break;
			case ButtonState.Hover:
				renderer.Draw2dTexture(renderer.GetTexture(_textureNameHover), x, y, sizex, sizey, null, 0, color);
				break;
			case ButtonState.Pressed:
				renderer.Draw2dTexture(renderer.GetTexture(_textureNamePressed), x, y, sizex, sizey, null, 0, color);
				break;
		}

		if (_text != null)
		{
			_text.SetX(x + sizex / 2);
			_text.SetY(y + sizey / 2);
			_text.Draw(dt, renderer);
		}
	}

	public ButtonState GetState()
	{
		return _state;
	}
	public void SetState(ButtonState state)
	{
		_state = state;
	}

	public void SetText(string text)
	{
		if (text == null || text == "") { return; }
		if (_text == null)
		{
			// Create new text widget if none exists
			FontCi font = new FontCi();
			font.size = 14;
			//_text = new TextWidget(x + sizex / 2, y + sizey / 2, text, font, TextAlign.Center, TextBaseline.Middle);
			_text = new TextWidget();
			_text.SetAlignment(TextAlign.Center);
			_text.SetBaseline(TextBaseline.Middle);
			_text.SetFont(font);
			_text.SetText(text);
		}
		else
		{
			// Change text of existing widget
			_text.SetText(text);
		}
	}

	public void SetTextureNames(string textureIdle, string textureHover, string texturePressed)
	{
		if (textureIdle != null && textureIdle != "")
		{
			_textureNameIdle = textureIdle;
		}
		if (textureHover != null && textureHover != "")
		{
			_textureNameHover = textureHover;
		}
		if (texturePressed != null && texturePressed != "")
		{
			_textureNamePressed = texturePressed;
		}
	}
}

public enum ButtonState
{
	Normal,
	Hover,
	Pressed
}

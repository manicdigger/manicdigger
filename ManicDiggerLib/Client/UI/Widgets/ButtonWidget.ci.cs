public class ButtonWidget : AbstractMenuWidget
{
	TextWidget _text;
	ButtonState _state;

	public ButtonWidget()
	{
		_state = ButtonState.Normal;
		x = 0;
		y = 0;
		sizex = 0;
		sizey = 0;
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
		if (_state != ButtonState.Hover) { return; }
		SetState(ButtonState.Pressed);
	}

	public override void OnMouseUp(GamePlatform p, MouseEventArgs args)
	{
		if (_state != ButtonState.Pressed) { return; }
		SetState(ButtonState.Normal);
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

	public override void Draw(MainMenu m)
	{
		if (!visible) { return; }
		switch (_state)
		{
			// TODO: Use atlas textures
			case ButtonState.Normal:
				m.Draw2dQuad(m.GetTexture("button.png"), x, y, sizex, sizey);
				break;
			case ButtonState.Hover:
				m.Draw2dQuad(m.GetTexture("button_sel.png"), x, y, sizex, sizey);
				break;
			case ButtonState.Pressed:
				m.Draw2dQuad(m.GetTexture("button_sel.png"), x, y, sizex, sizey);
				break;
		}

		if (_text != null)
		{
			_text.Draw(m);
		}
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
}

public enum ButtonState
{
	Normal,
	Hover,
	Pressed
}

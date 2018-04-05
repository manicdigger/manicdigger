public class TextBoxWidget : AbstractMenuWidget
{
	TextWidget _text;
	TextBoxState _state;
	FontCi _inputFont;

	string _placeholderText;
	const string _placeholderColorCode = "&7";
	const string _typingChar = "_";

	bool _hideInput;
	string _textContent;
	string _textDisplay;

	public TextBoxWidget()
	{
		focusable = true;
		_state = TextBoxState.Normal;
		_textContent = "";
		_textDisplay = "";
		x = 0;
		y = 0;
		sizex = 0;
		sizey = 0;

		_inputFont = new FontCi();
		_text = new TextWidget();
		_text.SetFont(_inputFont);
		_text.SetAlignment(TextAlign.Center);
		_text.SetBaseline(TextBaseline.Middle);
		_text.SetText(_placeholderColorCode + "Input");
	}
	//public TextBoxWidget(float dx, float dy, float dw, float dh, string placeholder, FontCi font, bool inputHidden)
	//{
	//	_state = TextBoxState.Normal;
	//	_textContent = "";
	//	_textDisplay = "";
	//	x = dx;
	//	y = dy;
	//	sizex = dw;
	//	sizey = dh;
	//	if ((placeholder != null) && (placeholder != ""))
	//	{
	//		_placeholderText = _placeholderColorCode + placeholder;
	//		_text = new TextWidget(x + sizex / 2, y + sizey / 2, _placeholderText, font, TextAlign.Center, TextBaseline.Middle);
	//	}
	//}

	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (_state != TextBoxState.Hover) { return; }
		SetState(TextBoxState.Editing);
	}

	public override void OnMouseUp(GamePlatform p, MouseEventArgs args)
	{
		if (_state != TextBoxState.Editing) { return; }
		SetState(TextBoxState.Normal);
	}

	public override void OnMouseMove(GamePlatform p, MouseEventArgs args)
	{
		// Check if mouse is inside the textbox rectangle
		if (IsCursorInside(args))
		{
			if (_state == TextBoxState.Normal)
			{
				SetState(TextBoxState.Hover);
			}
		}
		else
		{
			SetState(TextBoxState.Normal);
		}
	}

	public override void OnKeyPress(GamePlatform p, KeyPressEventArgs args)
	{
		if (_state != TextBoxState.Editing)
		{
			if (p.IsValidTypingChar(args.GetKeyChar()))
			{
				SetContent(p, StringTools.StringAppend(p, _textContent, CharToString(p, args.GetKeyChar())));
			}
		}
	}

	public override void OnKeyDown(GamePlatform p, KeyEventArgs args)
	{
		if (_state != TextBoxState.Editing)
		{
			int key = args.GetKeyCode();
			// pasting text from clipboard
			if (args.GetCtrlPressed() && key == GlKeys.V)
			{
				if (p.ClipboardContainsText())
				{
					SetContent(p, StringTools.StringAppend(p, _textContent, p.ClipboardGetText()));
				}
				return;
			}
			// deleting characters using backspace
			if (key == GlKeys.BackSpace)
			{
				if (StringTools.StringLength(p, _textContent) > 0)
				{
					SetContent(p, StringTools.StringSubstring(p, _textContent, 0, StringTools.StringLength(p, _textContent) - 1));
				}
				return;
			}
		}
	}

	public override void SetFocused(bool hasFocus)
	{
		hasKeyboardFocus = hasFocus;
		if (hasFocus)
		{
			SetState(TextBoxState.Editing);
		}
		else
		{
			SetState(TextBoxState.Normal);
		}
	}

	public override void Draw(MainMenu m)
	{
		if (!visible) { return; }
		switch (_state)
		{
			// TODO: Use atlas textures
			case TextBoxState.Normal:
				m.Draw2dQuad(m.GetTexture("button.png"), x, y, sizex, sizey);
				break;
			case TextBoxState.Hover:
				m.Draw2dQuad(m.GetTexture("button_sel.png"), x, y, sizex, sizey);
				break;
			case TextBoxState.Editing:
				m.Draw2dQuad(m.GetTexture("button_sel.png"), x, y, sizex, sizey);
				break;
		}

		if (_text != null)
		{
			if (_state == TextBoxState.Editing)
			{
				_text.SetText(StringTools.StringAppend(m.p, _textDisplay, "_"));
			}
			else
			{
				_text.SetText(_textDisplay);
			}
			_text.Draw(m);
		}
	}

	public void SetState(TextBoxState state)
	{
		_state = state;
	}

	public void SetContent(GamePlatform p, string c)
	{
		if (p == null) { return; }
		if (c == null) { return; }
		if (_text == null) { return; }
		_textContent = c;
		if (_hideInput)
		{
			_textDisplay = CharRepeat(p, 42, StringTools.StringLength(p, _textContent)); // '*'
		}
		else
		{
			_textDisplay = _textContent;
		}
		_text.SetText(_textDisplay);
	}

	string CharToString(GamePlatform p, int a)
	{
		int[] arr = new int[1];
		arr[0] = a;
		return p.CharArrayToString(arr, 1);
	}
	string CharRepeat(GamePlatform p, int c, int length)
	{
		int[] charArray = new int[length];
		for (int i = 0; i < length; i++)
		{
			charArray[i] = c;
		}
		return p.CharArrayToString(charArray, length);
	}
}

public enum TextBoxState
{
	Normal,
	Hover,
	Editing
}

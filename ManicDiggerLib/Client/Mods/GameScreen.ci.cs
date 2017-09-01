public class GameScreen : ClientMod
{
	public GameScreen()
	{
		WidgetCount = 64;
		widgets = new MenuWidget[WidgetCount];
	}
	internal Game game;
	public override void OnKeyPress(Game game_, KeyPressEventArgs args) { KeyPress(args); }
	public override void OnKeyDown(Game game_, KeyEventArgs args) { KeyDown(args); }
	public override void OnTouchStart(Game game_, TouchEventArgs e) { ScreenOnTouchStart(e); }
	public void ScreenOnTouchStart(TouchEventArgs e)
	{
		e.SetHandled(MouseDown(e.GetX(), e.GetY()));
	}
	public override void OnTouchEnd(Game game_, TouchEventArgs e) { ScreenOnTouchEnd(e); }
	public void ScreenOnTouchEnd(TouchEventArgs e)
	{
		MouseUp(e.GetX(), e.GetY());
	}
	public override void OnMouseDown(Game game_, MouseEventArgs args) { MouseDown(args.GetX(), args.GetY()); }
	public override void OnMouseUp(Game game_, MouseEventArgs args) { MouseUp(args.GetX(), args.GetY()); }
	public override void OnMouseMove(Game game_, MouseEventArgs args) { MouseMove(args); }
	public virtual void OnBackPressed() { }

	void KeyPress(KeyPressEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				if (w.type == WidgetType.Textbox)
				{
					if (w.editing)
					{
						if (game.platform.IsValidTypingChar(e.GetKeyChar()))
						{
							w.text = StringTools.StringAppend(game.platform, w.text, CharToString(e.GetKeyChar()));
						}
					}
				}
			}
		}
	}

	void KeyDown(KeyEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				if (w.type == WidgetType.Textbox)
				{
					if (w.editing)
					{
						int key = e.GetKeyCode();
						// pasting text from clipboard
						if (e.GetCtrlPressed() && key == GlKeys.V)
						{
							if (game.platform.ClipboardContainsText())
							{
								w.text = StringTools.StringAppend(game.platform, w.text, game.platform.ClipboardGetText());
							}
							return;
						}
						// deleting characters using backspace
						if (key == GlKeys.BackSpace)
						{
							if (StringTools.StringLength(game.platform, w.text) > 0)
							{
								w.text = StringTools.StringSubstring(game.platform, w.text, 0, StringTools.StringLength(game.platform, w.text) - 1);
							}
							return;
						}
					}
				}
			}
		}
	}

	bool MouseDown(int x, int y)
	{
		bool handled = false;
		bool editingChange = false;
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				if (w.type == WidgetType.Button)
				{
					w.pressed = pointInRect(x, y, screenx + w.x, screeny + w.y, w.sizex, w.sizey);
					if (w.pressed) { handled = true; }
				}
				if (w.type == WidgetType.Textbox)
				{
					w.pressed = pointInRect(x, y, screenx + w.x, screeny + w.y, w.sizex, w.sizey);
					if (w.pressed) { handled = true; }
					bool wasEditing = w.editing;
					w.editing = w.pressed;
					if (w.editing && (!wasEditing))
					{
						game.platform.ShowKeyboard(true);
						editingChange = true;
					}
					if ((!w.editing) && wasEditing && (!editingChange))
					{
						game.platform.ShowKeyboard(false);
					}
				}
			}
		}
		return handled;
	}

	void MouseUp(int x, int y)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				w.pressed = false;
			}
		}
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				if (w.type == WidgetType.Button)
				{
					if (pointInRect(x, y, screenx + w.x, screeny + w.y, w.sizex, w.sizey))
					{
						OnButton(w);
					}
				}
			}
		}
	}

	public virtual void OnButton(MenuWidget w) { }

	void MouseMove(MouseEventArgs e)
	{
		if (e.GetEmulated() && !e.GetForceUsage())
		{
			return;
		}
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				w.hover = pointInRect(e.GetX(), e.GetY(), screenx + w.x, screeny + w.y, w.sizex, w.sizey);
			}
		}
	}

	bool pointInRect(float x, float y, float rx, float ry, float rw, float rh)
	{
		return x >= rx && y >= ry && x < rx + rw && y < ry + rh;
	}

	public virtual void OnMouseWheel(MouseWheelEventArgs e) { }
	internal int WidgetCount;
	internal MenuWidget[] widgets;
	public void DrawWidgets()
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			MenuWidget w = widgets[i];
			if (w != null)
			{
				if (!w.visible)
				{
					continue;
				}
				string text = w.text;
				if (w.selected)
				{
					text = StringTools.StringAppend(game.platform, "&2", text);
				}
				if (w.type == WidgetType.Button)
				{
					if (w.buttonStyle == ButtonStyle.Text)
					{
						//game.Draw2dText1(text, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Left, TextBaseline.Middle);
					}
					else
					{
						if (w.image != null)
						{
							game.Draw2dBitmapFile(w.image, screenx + w.x, screeny + w.y, w.sizex, w.sizey);
						}
						else
						{
							game.Draw2dTexture(game.WhiteTexture(), screenx + w.x, screeny + w.y, w.sizex, w.sizey, null, 0, w.color, false);
						}
						game.Draw2dText(text, w.font, screenx + w.x, screeny + w.y + w.sizey / 2, null, false);
					}
				}
				if (w.type == WidgetType.Textbox)
				{
					if (w.password)
					{
						text = CharRepeat(42, StringTools.StringLength(game.platform, w.text)); // '*'
					}
					if (w.editing)
					{
						text = StringTools.StringAppend(game.platform, text, "_");
					}
					//if (w.buttonStyle == ButtonStyle.Text)
					{
						game.Draw2dText(text, w.font, screenx + w.x, screeny + w.y, null, false);//, TextAlign.Left, TextBaseline.Top);
					}
					//else
					{
						//menu.DrawButton(text, w.fontSize, w.x, w.y, w.sizex, w.sizey, (w.hover || w.editing));
					}
				}
				if (w.type == WidgetType.Label)
				{
					game.Draw2dText(text, w.font, screenx + w.x, screeny + w.y, IntRef.Create(w.color), false);
				}
				if (w.description != null)
				{
					//menu.DrawText(w.description, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Right, TextBaseline.Middle);
				}
			}
		}
	}
	public string CharToString(int a)
	{
		int[] arr = new int[1];
		arr[0] = a;
		return game.platform.CharArrayToString(arr, 1);
	}

	public string CharRepeat(int c, int length)
	{
		int[] charArray = new int[length];
		for (int i = 0; i < length; i++)
		{
			charArray[i] = c;
		}
		return game.platform.CharArrayToString(charArray, length);
	}
	internal int screenx;
	internal int screeny;
}

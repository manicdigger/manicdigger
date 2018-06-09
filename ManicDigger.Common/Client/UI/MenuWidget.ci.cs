public abstract class AbstractMenuWidget
{
	// using "internal" keyword as CiTo does not support "protected"
	internal float x;
	internal float y;
	internal float sizex;
	internal float sizey;
	internal bool visible;
	internal bool clickable;
	internal bool focusable;
	internal bool hasKeyboardFocus;
	internal int color;
	internal int eventKeyChar;
	internal string eventName;
	internal bool eventKeyPressed;
	internal AbstractMenuWidget nextWidget;

	public AbstractMenuWidget()
	{
		x = 0;
		y = 0;
		sizex = 0;
		sizey = 0;
		visible = true;
		clickable = false;
		focusable = false;
		hasKeyboardFocus = false;
		color = -1; // ColorCi.FromArgb(255, 255, 255, 255);
		eventKeyChar = -1;
		eventName = null;
		eventKeyPressed = false;
		nextWidget = null;
	}

	/// --------------------------------------------------
	/// Virtual methods
	/// --------------------------------------------------

	public virtual void OnKeyPress(GamePlatform p, KeyPressEventArgs args)
	{
		DefaultOnKeyPress(p, args);
	}
	public virtual void OnKeyDown(GamePlatform p, KeyEventArgs args)
	{
		DefaultOnKeyDown(p, args);
	}
	public virtual void OnMouseDown(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseUp(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseMove(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseWheel(GamePlatform p, MouseWheelEventArgs args) { }
	public abstract void Draw(float dt, UiRenderer renderer);

	/// --------------------------------------------------
	/// Methods with shared functionality
	/// --------------------------------------------------

	internal void DefaultOnKeyPress(GamePlatform p, KeyPressEventArgs args)
	{
		eventKeyPressed = (eventKeyChar == args.GetKeyChar()) ? true : false;
	}
	internal void DefaultOnKeyDown(GamePlatform p, KeyEventArgs args)
	{
		if (hasKeyboardFocus &&
			args.GetKeyCode() == GlKeys.Tab && !args.GetHandled())
		{
			// activate next widget when tab key is pressed
			// only do this when it has not been already handled
			if (nextWidget != null)
			{
				SetFocused(false);
				nextWidget.SetFocused(true);
				args.SetHandled(true);
			}
		}
	}
	public bool IsCursorInside(MouseEventArgs args)
	{
		return (args.GetX() >= x && args.GetX() <= x + sizex &&
			args.GetY() >= y && args.GetY() <= y + sizey);
	}
	public bool HasBeenClicked(MouseEventArgs args)
	{
		return (visible && clickable && IsCursorInside(args));
	}

	/// --------------------------------------------------
	/// Getter and Setter methods
	/// --------------------------------------------------

	public bool GetFocused() { return hasKeyboardFocus; }
	public void SetFocused(bool hasFocus)
	{
		if (!focusable) { return; }
		hasKeyboardFocus = hasFocus;
	}

	public bool GetVisible() { return visible; }
	public void SetVisible(bool isVisible) { visible = isVisible; }

	public bool GetClickable() { return clickable; }
	public void SetClickable(bool isClickable) { clickable = isClickable; }

	public bool GetFocusable() { return focusable; }
	public void SetFocusable(bool isFocusable)
	{
		focusable = isFocusable;
		if (!focusable && hasKeyboardFocus)
		{
			// Lose focus when property changes while focused
			hasKeyboardFocus = false;
		}
	}

	public void SetX(float newX) { x = newX; }
	public float GetX() { return x; }

	public void SetY(float newY) { y = newY; }
	public float GetY() { return y; }

	public void SetSizeX(float newSizeX) { sizex = newSizeX; }
	public float GetSizeX() { return sizex; }

	public void SetSizeY(float newSizeY) { sizey = newSizeY; }
	public float GetSizeY() { return sizey; }

	public void SetColor(int newColor) { color = newColor; }
	public int GetColor() { return color; }

	public void SetEventKeyChar(int listenChar)
	{
		eventKeyPressed = false;
		eventKeyChar = listenChar;
	}
	public void SetEventName(string clickKey) { eventName = clickKey; }

	public string GetEventName() { return eventName; }
	public bool GetEventKeyPressed() { return eventKeyPressed; }

	public virtual string GetEventResponse() { return null; }

	public void SetNextWidget(AbstractMenuWidget w) { nextWidget = w; }
	public AbstractMenuWidget GetNextWidget() { return nextWidget; }
}

/// --------------------------------------------------
/// Custom datatype definitions
/// --------------------------------------------------

public enum WidgetType
{
	Button,
	Textbox,
	Label
}

public enum ButtonStyle
{
	Button,
	Text,
	ServerEntry
}

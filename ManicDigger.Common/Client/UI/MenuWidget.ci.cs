public abstract class AbstractMenuWidget
{
	internal float x;
	internal float y;
	internal float sizex;
	internal float sizey;
	internal bool visible;
	internal bool clickable;
	internal bool focusable;
	internal bool hasKeyboardFocus;
	internal int color;
	int eventKeyChar;
	string eventName;
	bool eventKeyPressed;

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
		color = ColorCi.FromArgb(255, 255, 255, 255);
		eventKeyChar = -1;
		eventName = null;
		eventKeyPressed = false;
	}
	public virtual void OnKeyPress(GamePlatform p, KeyPressEventArgs args)
	{
		eventKeyPressed = (eventKeyChar == args.GetKeyChar()) ? true : false;
	}
	public virtual void OnKeyDown(GamePlatform p, KeyEventArgs args) { }
	public virtual void OnMouseDown(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseUp(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseMove(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseWheel(GamePlatform p, MouseWheelEventArgs args) { }
	public virtual bool IsCursorInside(MouseEventArgs args)
	{
		return (args.GetX() >= x && args.GetX() <= x + sizex &&
			args.GetY() >= y && args.GetY() <= y + sizey);
	}
	public abstract void Draw(float dt, UiRenderer renderer);

	public virtual bool GetFocused()
	{
		return hasKeyboardFocus;
	}
	public virtual void SetFocused(bool hasFocus)
	{
		if (!focusable) { return; }
		hasKeyboardFocus = hasFocus;
	}

	public virtual bool GetVisible()
	{
		return visible;
	}
	public virtual void SetVisible(bool isVisible)
	{
		visible = isVisible;
	}

	public virtual bool GetClickable()
	{
		return clickable;
	}
	public virtual void SetClickable(bool isClickable)
	{
		clickable = isClickable;
	}

	public virtual bool GetFocusable()
	{
		return focusable;
	}
	public virtual void SetFocusable(bool isFocusable)
	{
		focusable = isFocusable;
		if (!focusable && hasKeyboardFocus)
		{
			// Lose focus when property changes while focused
			hasKeyboardFocus = false;
		}
	}

	public virtual bool HasBeenClicked(MouseEventArgs args)
	{
		return (visible && clickable && IsCursorInside(args));
	}
	public void SetX(float newX)
	{
		x = newX;
	}
	public float GetX()
	{
		return x;
	}
	public void SetY(float newY)
	{
		y = newY;
	}
	public float GetY()
	{
		return y;
	}
	public void SetSizeX(float newSizeX)
	{
		sizex = newSizeX;
	}
	public float GetSizeX()
	{
		return sizex;
	}
	public void SetSizeY(float newSizeY)
	{
		sizey = newSizeY;
	}
	public float GetSizeY()
	{
		return sizey;
	}
	public void SetColor(int newColor)
	{
		color = newColor;
	}
	public int GetColor()
	{
		return color;
	}
	public void SetEventKeyChar(int listenChar)
	{
		eventKeyPressed = false;
		eventKeyChar = listenChar;
	}
	public void SetEventName(string clickKey)
	{
		eventName = clickKey;
	}
	public string GetEventName()
	{
		return eventName;
	}
	public bool GetEventKeyPressed()
	{
		return eventKeyPressed;
	}
	public virtual string GetEventResponse() { return null; }
}

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

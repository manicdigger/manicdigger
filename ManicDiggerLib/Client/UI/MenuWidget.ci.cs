public class MenuWidget
{
	internal float x;
	internal float y;
	internal float sizex;
	internal float sizey;

	public MenuWidget()
	{
		visible = true;
		font = new FontCi();
		font.size = 14;
		nextWidget = -1;
		hasKeyboardFocus = false;
	}
	public virtual void OnKeyPress(GamePlatform p, KeyPressEventArgs args) { }
	public virtual void OnKeyDown(GamePlatform p, KeyEventArgs args) { }
	public virtual void OnMouseDown(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseUp(GamePlatform p, MouseEventArgs args) { }
	public virtual void OnMouseMove(GamePlatform p, MouseEventArgs args) { }
	public virtual bool IsCursorInside(MouseEventArgs args)
	{
		if (args.GetX() >= x && args.GetX() <= x + sizex &&
			args.GetY() >= y && args.GetY() <= y + sizey)
		{
			return true;
		}
		return false;
	}
	public virtual void GetFocus()
	{
		hasKeyboardFocus = true;
		if (type == WidgetType.Textbox)
		{
			editing = true;
		}
	}
	public virtual void LoseFocus()
	{
		hasKeyboardFocus = false;
		if (type == WidgetType.Textbox)
		{
			editing = false;
		}
	}
	internal string text;
	internal bool pressed;
	internal bool hover;
	internal WidgetType type;
	internal bool editing;
	internal bool visible;
	internal string description;
	internal bool password;
	internal bool selected;
	internal ButtonStyle buttonStyle;
	internal string image;
	internal int nextWidget;
	internal bool hasKeyboardFocus;
	internal int color;
	internal string id;
	internal bool isbutton;
	internal FontCi font;
}
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
	internal string id;
	internal int color;

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
		id = null;
		color = Game.ColorFromArgb(255, 255, 255, 255);
	}
	public virtual void OnKeyPress(GamePlatform p, KeyPressEventArgs args) { }
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
	public abstract void Draw(UiRenderer renderer);

	public virtual void SetFocused(bool hasFocus)
	{
		if (!focusable) { return; }
		hasKeyboardFocus = hasFocus;
	}

	public virtual void SetVisible(bool isVisible)
	{
		visible = isVisible;
	}

	public virtual void SetClickable(bool isClickable)
	{
		clickable = isClickable;
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

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

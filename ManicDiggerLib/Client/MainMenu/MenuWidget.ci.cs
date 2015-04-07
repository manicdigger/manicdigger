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
    ServerEntry,
    WorldEntry
}

public class MenuWidget
{
    public MenuWidget()
    {
        // Standard settings for widgets
        visible = true;
        fontSize = 14;
        nextWidget = -1;
        hasKeyboardFocus = false;
        pressable = true; // Can be pressed by default
    }
    public void GetFocus()
    {
        hasKeyboardFocus = true; // Set keyboard focus
        if (type == WidgetType.Textbox)
            editing = true;
    }
    public void LoseFocus()
    {
        hasKeyboardFocus = false; // Remove keyboard focus
        if (type == WidgetType.Textbox)
            editing = false;
    }
    internal string text;
    internal float x; // X-Position
    internal float y; // Y-Position
    internal float sizex; // Width
    internal float sizey; // Height
    internal bool pressed; // If the widget is pressed
    internal bool hover;
    internal WidgetType type; // Type of widget
    internal bool editing; // If the user is editing the widget (typing in a textbox for example)
    internal bool visible; // If the widget is visable (and interactable)
    internal float fontSize; // Size on text font
    internal string description; // Description
    internal bool password;
    internal bool selected;
    internal ButtonStyle buttonStyle;
    internal string image;
    internal int nextWidget;
    internal bool hasKeyboardFocus;
    internal int color;
    internal string id;
    internal bool isButton;
    internal FontCi font;
    internal bool pressable; // If the widget can be pressed (grayed out)

    public virtual void UpdatePosition(int canWidth, int canHeight, float scale)
    {
        // Not in use - might be later
    }
}

// None of the below are actually used (thinking of switching to them, or changing the widget system) //Bubba
public class ButtonWidget : MenuWidget
{
    public ButtonWidget()
    {
        visible = true;
        fontSize = 14;
        nextWidget = -1;
        hasKeyboardFocus = false;
    }
}

public class TextboxWidget : MenuWidget
{
    public TextboxWidget()
    {
        visible = true;
        fontSize = 14;
        nextWidget = -1;
        hasKeyboardFocus = false;
    }
}

public class LabelWidget : MenuWidget
{
    public LabelWidget()
    {
        visible = true;
        fontSize = 14;
        nextWidget = -1;
        hasKeyboardFocus = false;
    }
}

public class ServerEntryWidget : ButtonWidget
{
    internal string name;
    internal string motd;
    internal string gamemode;
    internal string playercount;
    internal int index;

    public ServerEntryWidget()
    {
        visible = true;
        fontSize = 14;
        nextWidget = -1;
        hasKeyboardFocus = false;
    }

    public override void UpdatePosition(int canWidth, int canHeight, float scale)
    {
        x = 100 * scale;
        y = 100 * scale + index * 70 * scale;
        sizex = canWidth - 200 * scale;
        sizey = 64 * scale;
    }
}
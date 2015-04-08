public class ScreenNewWorld : Screen
{
    public ScreenNewWorld()
    {
        // Create buttons
        textboxName = new MenuWidget(); // World Name (textbox)
        textboxName.type = WidgetType.Textbox;
        textboxName.text = "";
        textboxName.description = "World Name";
        textboxName.nextWidget = 1;

        create = new MenuWidget(); // Play
        create.text = "Create";
        create.type = WidgetType.Button;
        create.buttonStyle = ButtonStyle.Button;
        create.visible = true;
        create.pressable = false; // Can not be pressed until save is selected
        create.nextWidget = 2;

        back = new MenuWidget(); // Back
        back.text = "Back";
        back.type = WidgetType.Button;
        back.buttonStyle = ButtonStyle.Button;
        back.visible = true;
        back.nextWidget = 0;

        // Add buttons to widget collection
        widgets[0] = textboxName;
        widgets[1] = create;
        widgets[2] = back;

        // Set screen title
        title = "New World";
    }

    MenuWidget create;
    MenuWidget textboxName;
    MenuWidget back;

    string title; // Screen title

    int oldWidth; // CanvasWidth from last rendering (frame)
    int oldHeight; // CanvasHeight from last rendering (frame)

    public override void Render(float dt)
    {
        GamePlatform p = menu.p;

        //
        if (textboxName.text == "")
        {
            create.pressable = false;
        }
        else
        {
            // Check if world name is valid (todo)


            create.pressable = true;
        }

        // Screen measurements
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();
        float leftx = width / 2f - 400f * scale;
        float y = height / 2f - 250f * scale;

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            // Create button
            create.x = width - 296f * scale;
            create.y = height - 104f * scale;
            create.sizex = 256f * scale;
            create.sizey = 64f * scale;
            create.fontSize = 14f * scale;

            // World Name textbox
            textboxName.x = leftx;
            textboxName.y = y + 200f * scale;
            textboxName.sizex = 256f * scale;
            textboxName.sizey = 64f * scale;
            textboxName.fontSize = 14f * scale;

            // Back button
            back.x = 40f * scale;
            back.y = height - 104f * scale;
            back.sizex = 256f * scale;
            back.sizey = 64f * scale;
            back.fontSize = 14f * scale;
        }

        // Draw background
        menu.DrawBackground();

        // Draw title text
        menu.DrawText(title, 20f * scale, p.GetCanvasWidth() / 2, 10f, TextAlign.Center, TextBaseline.Top);

        // Draw widgets
        DrawWidgets();

        // Update old(Width/Height)
        oldWidth = width;
        oldHeight = height;
    }

    public override void OnBackPressed()
    {
        menu.StartSingleplayer();
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == create)
        {
            if (create.pressable) // If button is pressable
            {
                // Create directory if it doesn't exist
                menu.p.CreateSavegamesDirectory();

                // Decide on savegame extension
                string extension;
                if (menu.p.SinglePlayerServerAvailable()) { extension = "mddbs"; }
                else { extension = "mdss"; }

                // Format path
                string path = menu.p.StringFormat3("{0}\\{1}.{2}", menu.p.PathSavegames(), textboxName.text, extension);

                // Create game
                menu.ConnectToSingleplayer(path);
            }
        }
        else if (w == back)
        {
            OnBackPressed();
        }
    }
}

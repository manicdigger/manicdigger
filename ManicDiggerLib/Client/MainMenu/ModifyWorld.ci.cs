public class ScreenModifyWorld : Screen
{
    public ScreenModifyWorld()
    {
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;

        widgets[0] = back;
    }

    MenuWidget back;

    public override void Render(float dt)
    {
        GamePlatform p = menu.p;

        float scale = menu.GetScale();

        menu.DrawBackground();
        menu.DrawText("Modify World", menu.fontMenuHeading, menu.p.GetCanvasWidth() / 2, 0, TextAlign.Center, TextBaseline.Top);

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;

        DrawWidgets();
    }

    public override void OnBackPressed()
    {
        menu.StartSingleplayer();
    }

    public override void OnButton(MenuWidget w)
    {
        if (w == back)
        {
            OnBackPressed();
        }
    }
}

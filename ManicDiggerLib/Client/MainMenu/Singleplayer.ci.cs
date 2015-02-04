public class ScreenSingleplayer : Screen
{
    public ScreenSingleplayer()
    {
        play = new MenuWidget();
        play.text = "Play";
        newWorld = new MenuWidget();
        newWorld.text = "New World";
        modify = new MenuWidget();
        modify.text = "Modify";
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;
        open = new MenuWidget();
        open.text = "Create or open...";
        open.type = WidgetType.Button;

        title = "Singleplayer";

        widgets[0] = play;
        widgets[1] = newWorld;
        widgets[2] = modify;
        widgets[3] = back;
        widgets[4] = open;

        worldButtons = new MenuWidget[10];
        for (int i = 0; i < 10; i++)
        {
            worldButtons[i] = new MenuWidget();
            worldButtons[i].visible = false;
            widgets[5 + i] = worldButtons[i];
        }
    }

    MenuWidget newWorld;
    MenuWidget play;
    MenuWidget modify;
    MenuWidget back;
    MenuWidget open;

    MenuWidget[] worldButtons;

    string[] savegames;
    int savegamesCount;
    string title;

    public override void LoadTranslations()
    {
        back.text = menu.lang.Get("MainMenu_ButtonBack");
        open.text = menu.lang.Get("MainMenu_SingleplayerButtonCreate");
        title = menu.lang.Get("MainMenu_Singleplayer");
    }

    public override void Render(float dt)
    {
        GamePlatform p = menu.p;

        float scale = menu.GetScale();

        menu.DrawBackground();
        menu.DrawText(title, 20 * scale, p.GetCanvasWidth() / 2, 10, TextAlign.Center, TextBaseline.Top);

        float leftx = p.GetCanvasWidth() / 2 - 128 * scale;
        float y = p.GetCanvasHeight() / 2 + 0 * scale;

        play.x = leftx;
        play.y = y + 100 * scale;
        play.sizex = 256 * scale;
        play.sizey = 64 * scale;
        play.fontSize = 14 * scale;

        newWorld.x = leftx;
        newWorld.y = y + 170 * scale;
        newWorld.sizex = 256 * scale;
        newWorld.sizey = 64 * scale;
        newWorld.fontSize = 14 * scale;

        modify.x = leftx;
        modify.y = y + 240 * scale;
        modify.sizex = 256 * scale;
        modify.sizey = 64 * scale;
        modify.fontSize = 14 * scale;

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;

        open.x = leftx;
        open.y = y + 0 * scale;
        open.sizex = 256 * scale;
        open.sizey = 64 * scale;
        open.fontSize = 14 * scale;

        if (savegames == null)
        {
            IntRef savegamesCount_ = new IntRef();
            savegames = menu.GetSavegames(savegamesCount_);
            savegamesCount = savegamesCount_.value;
        }

        for (int i = 0; i < 10; i++)
        {
            worldButtons[i].visible = false;
        }
        for (int i = 0; i < savegamesCount; i++)
        {
            worldButtons[i].visible = true;
            worldButtons[i].text = menu.p.FileName(savegames[i]);
            worldButtons[i].x = leftx;
            worldButtons[i].y = 100 + 100 * scale * i;
            worldButtons[i].sizex = 256 * scale;
            worldButtons[i].sizey = 64 * scale;
            worldButtons[i].fontSize = 14 * scale;
        }

        open.visible = menu.p.SinglePlayerServerAvailable();
        play.visible = false;
        newWorld.visible = false;
        modify.visible = false;
        for (int i = 0; i < savegamesCount; i++)
        {
            worldButtons[i].visible = false;
        }

        DrawWidgets();

        if (!menu.p.SinglePlayerServerAvailable())
        {
            menu.DrawText("Singleplayer is only available on desktop (Windows, Linux, Mac) version of game.", 16 * scale, menu.p.GetCanvasWidth() / 2, menu.p.GetCanvasHeight() / 2, TextAlign.Center, TextBaseline.Middle);
        }
    }

    public override void OnBackPressed()
    {
        menu.StartMainMenu();
    }

    public override void OnButton(MenuWidget w)
    {
        for (int i = 0; i < 10; i++)
        {
            worldButtons[i].selected = false;
        }
        for (int i = 0; i < 10; i++)
        {
            if (worldButtons[i] == w)
            {
                worldButtons[i].selected = true;
            }
        }

        if (w == newWorld)
        {
            menu.StartNewWorld();
        }

        if (w == play)
        {
        }

        if (w == modify)
        {
            menu.StartModifyWorld();
        }

        if (w == back)
        {
            OnBackPressed();
        }

        if (w == open)
        {
            string extension;
            if (menu.p.SinglePlayerServerAvailable())
            {
                extension = "mddbs";
            }
            else
            {
                extension = "mdss";
            }
            string result = menu.p.FileOpenDialog(extension, "Manic Digger Savegame", menu.p.PathSavegames());
            if (result != null)
            {
                menu.ConnectToSingleplayer(result);
            }
        }
    }
}

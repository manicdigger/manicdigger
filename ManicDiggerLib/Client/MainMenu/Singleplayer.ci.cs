public class ScreenSingleplayer : Screen
{
    public ScreenSingleplayer()
    {
        // Set widget length (because it is extended)
        WidgetCount = 64 + worldButtonsCount;
        widgets = new MenuWidget[WidgetCount];

        // Create buttons
        play = new MenuWidget(); // Play
        play.text = "Play";
        play.type = WidgetType.Button;
        play.buttonStyle = ButtonStyle.Button;
        play.visible = true;
        play.pressable = false; // Can not be pressed until save is selected
        play.nextWidget = 1;

        newWorld = new MenuWidget(); // New World
        newWorld.text = "New World";
        newWorld.type = WidgetType.Button;
        newWorld.buttonStyle = ButtonStyle.Button;
        newWorld.visible = true;
        newWorld.nextWidget = 3;

        back = new MenuWidget(); // Back
        back.text = "Back";
        back.type = WidgetType.Button;
        back.buttonStyle = ButtonStyle.Button;
        back.visible = true;
        back.nextWidget = 0;

        open = new MenuWidget(); // Open
        open.text = "Create or open...";
        open.type = WidgetType.Button;
        open.buttonStyle = ButtonStyle.Button;
        open.visible = true;
        open.nextWidget = 2;

        refresh = new MenuWidget(); // Refresh
        refresh.text = "";
        refresh.type = WidgetType.Button;
        refresh.buttonStyle = ButtonStyle.Text;
        refresh.visible = true;
        refresh.image = "serverlist_refresh.png";

        pageUp = new MenuWidget(); // Page Up
        pageUp.text = "";
        pageUp.type = WidgetType.Button;
        pageUp.buttonStyle = ButtonStyle.Text;
        pageUp.visible = false;
        pageUp.image = "serverlist_nav_down.png";

        pageDown = new MenuWidget(); // Page Down
        pageDown.text = "";
        pageDown.type = WidgetType.Button;
        pageDown.buttonStyle = ButtonStyle.Text;
        pageDown.visible = false;
        pageDown.image = "serverlist_nav_up.png";

        // Add buttons to widget collection
        widgets[0] = play;
        widgets[1] = newWorld;
        widgets[2] = back;
        widgets[3] = open;
        widgets[4] = refresh;
        widgets[5] = pageUp;
        widgets[6] = pageDown;

        // Add world buttons to widget collection (after the more static buttons)
        worldButtons = new MenuWidget[worldButtonsCount];
        for (int i = 0; i < worldButtonsCount; i++)
        {
            // Create saved game button
            MenuWidget b = new MenuWidget();
            worldButtons[i] = new MenuWidget();
            b.text = "Invalid";
            b.type = WidgetType.Button;
            b.buttonStyle = ButtonStyle.WorldEntry;
            b.visible = false;
            b.image = "serverlist_entry_noimage.png";
            b.nextWidget = 0; // Select play when tab is pressed

            // Add widget to collections
            worldButtons[i] = b;
            widgets[7 + i] = b;
        }

        // Set screen title
        title = "Singleplayer";

        // Load savegames
        loadSavegames = true;

        selectedSave = -1; // No save is selected
    }

    MenuWidget newWorld;
    MenuWidget play;
    MenuWidget back;
    MenuWidget open;
    MenuWidget refresh;
    MenuWidget pageUp;
    MenuWidget pageDown;

    MenuWidget[] worldButtons; // Saved games buttons
    const int worldButtonsCount = 1024; // Max amount of saved games buttons

    bool loadSavegames; // If saves should be loaded
    string[] savegames; // Paths to saved games files
    int savegamesCount; // Amount of saved games in collection
    int selectedSave; // Path to the file of the selected save
    int page; // Current saved games page
    int pageCount; // Amount of pages of saved games
    int savesPerPage; // Saved games per page
    string title; // Screen title

    int oldWidth; // CanvasWidth from last rendering (frame)
    int oldHeight; // CanvasHeight from last rendering (frame)

    public override void LoadTranslations()
    {
        back.text = menu.lang.Get("MainMenu_ButtonBack");
        open.text = menu.lang.Get("MainMenu_SingleplayerButtonCreate");
        title = menu.lang.Get("MainMenu_Singleplayer");
    }

    public override void Render(float dt)
    {
        // Load saved games (should only happen once)
        if (loadSavegames)
        {
            // Deselect save
            if (selectedSave >= 0)
            {
                worldButtons[selectedSave].selected = false;
            }
            selectedSave = -1;

            // Load saves
            IntRef savegamesCount_ = new IntRef();
            savegames = menu.GetSavegames(savegamesCount_);
            savegamesCount = savegamesCount_.value;

            play.pressable = false; // User has to select a save again
            loadSavegames = false; // Loading saves done
        }

        GamePlatform p = menu.p;

        // Screen measurements
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();
        float leftx = p.GetCanvasWidth() / 2f - 128f * scale;
        float y = p.GetCanvasHeight() / 2f + 0f * scale;

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            // Amount of entries that fits on the screen
            savesPerPage = menu.p.FloatToInt((height - (2f * 100f * scale)) / 70f * scale);

            // Amount of pages
            if (savesPerPage > 0)
            {
                pageCount = (savegamesCount - 1) / savesPerPage;
                if (pageCount < 0) { pageCount = 0; } // Stop page count from being negative
            }
            else
            {
                pageCount = 0; // Set page count to 0 if it can not be defined
            }

            // Stop the current page from being beyond the last page
            if (page > pageCount)
            {
                page = pageCount;
                UpdateSaveSelection();
            }

            // Play button
            play.x = width - 888f * scale;
            play.y = height - 104f * scale;
            play.sizex = 256f * scale;
            play.sizey = 64f * scale;
            play.fontSize = 14f * scale;

            // New world button
            newWorld.x = width - 592f * scale;
            newWorld.y = height - 104f * scale;
            newWorld.sizex = 256f * scale;
            newWorld.sizey = 64f * scale;
            newWorld.fontSize = 14f * scale;

            // Open button
            open.x = width - 296f * scale;
            open.y = height - 104f * scale;
            open.sizex = 256f * scale;
            open.sizey = 64f * scale;
            open.fontSize = 14f * scale;

            // Back (to menu) button
            back.x = 40f * scale;
            back.y = height - 104f * scale;
            back.sizex = 256f * scale;
            back.sizey = 64f * scale;
            back.fontSize = 14f * scale;

            // Refresh button
            refresh.x = 100f * scale;
            refresh.y = 30f * scale;
            refresh.sizex = 64f * scale;
            refresh.sizey = 64f * scale;

            // Page up button
            pageUp.x = width - 94f * scale;
            pageUp.y = 100f * scale + (savesPerPage - 1) * 70f * scale;
            pageUp.sizex = 64f * scale;
            pageUp.sizey = 64f * scale;

            // Page down button
            pageDown.x = width - 94f * scale;
            pageDown.y = 100f * scale;
            pageDown.sizex = 64f * scale;
            pageDown.sizey = 64f * scale;
        }

        // Show open button (or dont, depending on platform)
        open.visible = menu.p.SinglePlayerServerAvailable();

        // Draw
        menu.DrawBackground(); // Draw background
        menu.DrawText(title, 20f * scale, p.GetCanvasWidth() / 2, 10f, TextAlign.Center, TextBaseline.Top); // Draw title text
        menu.DrawText(p.StringFormat2("{0}/{1}", p.IntToString(page + 1), p.IntToString(pageCount + 1)), 14f * scale,
                      width - 68f * scale, (100f + (savesPerPage * 70f) / 2f) * scale, TextAlign.Center, TextBaseline.Middle); // Draw page number

        // Hide all saved game buttons
        for (int i = 0; i < worldButtonsCount; i++)
        {
            worldButtons[i].visible = false;
        }

        // Draw saved games
        for (int i = 0; i < savesPerPage; i++)
        {
            // Get current saved games index
            int index = i + (savesPerPage * page); // Get index
            if (index > worldButtonsCount) { break; } // If the last entry is reached

            // Don't draw more buttons than there are saves
            if (index >= savegamesCount) { break; }

            // Get saved games file path
            string s = savegames[index];
            if (s == null) { continue; } // Skip if saved game does not exist

            // Set button text
            worldButtons[i].text = menu.p.FileName(savegames[index]);

            worldButtons[i].visible = true; // Make visable
            worldButtons[i].image = "serverlist_entry_noimage.png"; // Placeholder image

            // Update size/position
            //if (resized)
            //{
                worldButtons[i].x = 100f * scale;
                worldButtons[i].y = 100f * scale + i * 70f * scale;
                worldButtons[i].sizex = width - 200f * scale;
                worldButtons[i].sizey = 64f * scale;
            //}
        }

        // Update scrollbars
        UpdateScrollButtons();

        // Draw widgets
        DrawWidgets();

        // Tells certain platforms that they cannot play singleplayer
        if (!menu.p.SinglePlayerServerAvailable())
        {
            menu.DrawText("Singleplayer is only available on desktop (Windows, Linux, Mac) version of game.", 16f * scale, menu.p.GetCanvasWidth() / 2f, menu.p.GetCanvasHeight() / 2f, TextAlign.Center, TextBaseline.Middle);
        }

        // Update old(Width/Height)
        oldWidth = width;
        oldHeight = height;
    }

    public void PageUp_()
    {
        if (pageUp.visible && page < worldButtonsCount / savesPerPage - 1)
        {
            page++;
            UpdateSaveSelection();
        }
    }
    public void PageDown_()
    {
        if (page > 0)
        {
            page--;
            UpdateSaveSelection();
        }
    }
    public void UpdateSaveSelection()
    {
        // Deselect all save buttons
        for (int i = 0; i < worldButtonsCount; i++)
        {
            worldButtons[i].selected = false;
        }

        // Abort if no save is selected
        if (selectedSave == -1) { return; }

        // Abort if no save can not be selected (selected save is in a previous page)
        if (selectedSave - (savesPerPage * page) < 0) { return; }

        // Select selected save
        worldButtons[selectedSave - (savesPerPage * page)].selected = true;
    }
    public void UpdateScrollButtons()
    {
        // Determine if this page is the highest page containing saves
        bool maxpage = false;
        if (savesPerPage <= 0 // Disable page up if there are no saves per page
            || (page + 1) * savesPerPage >= worldButtonsCount
            || (page + 1) * savesPerPage >= savegamesCount)
        {
            maxpage = true;
        }

        // Hide scroll buttons
        if (page == 0) { pageDown.visible = false; }
        else { pageDown.visible = true; }

        if (maxpage) { pageUp.visible = false; }
        else { pageUp.visible = true; }
    }

    public override void OnBackPressed()
    {
        menu.StartMainMenu();
    }

    public override void OnMouseWheel(MouseWheelEventArgs e)
    {
        //menu.p.MessageBoxShowError(menu.p.IntToString(e.GetDelta()), "Delta");
        if (e.GetDelta() < 0) { PageUp_(); } //Mouse wheel turned down
        else if (e.GetDelta() > 0) { PageDown_(); } //Mouse wheel turned up
    }
    public override void OnButton(MenuWidget w)
    {
        // Check what button was clicked
        if (w == newWorld) // New world
        {
            menu.StartNewWorld();
        }
        else if (w == play) // Play
        {
            // Load (and start) saved game
            if (play.pressable) // Check if the button can be pressed
                menu.ConnectToSingleplayer(savegames[selectedSave]);
        }
        else if (w == back) // Back (to main menu)
        {
            OnBackPressed();
        }
        else if (w == pageUp) // Page up button
        {
            PageUp_();
        }
        else if (w == pageDown) // Page down button
        {
            PageDown_();
        }
        else if (w == refresh) // Refresh
        {
            loadSavegames = true; // Reload saved games
        }
        else if (w == open) // Open (with dialog)
        {
            // Decide on savegame extension
            string extension;
            if (menu.p.SinglePlayerServerAvailable()) { extension = "mddbs"; }
            else { extension = "mdss"; }

            // Open a dialog with all saved games
            string path = menu.p.PathSavegames(); // Get path to directory for saved games
            menu.p.CreateSavegamesDirectory(); // Create directory if it doesn't exist
            string result = menu.p.FileOpenDialog(extension, "Manic Digger Savegame", path); // Open dialog for selecting saved game
            if (result != null)
            {
                menu.ConnectToSingleplayer(result); // Load game if file was valid
            }
        }
        else // Saved games
        {
            // Deselect all saves
            int index = -1;
            for (int i = 0; i < worldButtonsCount; i++)
            {
                if (worldButtons[i] == null) { break; } // Stop if end is reached
                if (worldButtons[i] == w) // If button is the clicked button
                {
                    index = i; // Keep index is button is selected button
                }
                else // If button is some other world button
                {
                    worldButtons[i].selected = false; // Deselect button
                }
            }

            selectedSave = index; // Get the clicked saves index
            w.selected = true; // Select clicked save

            play.pressable = true; // Make play button pressable
        }
    }
}

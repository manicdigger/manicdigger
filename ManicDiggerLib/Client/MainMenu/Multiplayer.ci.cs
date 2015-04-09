public class ScreenMultiplayer : Screen
{
    public ScreenMultiplayer()
    {
        // Set widget length (because it is extended)
        WidgetCount = 64 + serverButtonsCount;
        widgets = new MenuWidget[WidgetCount];

        // Create buttons
        back = new MenuWidget(); // Back
        back.text = "Back";
        back.type = WidgetType.Button;
        back.nextWidget = 1;

        connect = new MenuWidget(); // Connect
        connect.text = "Connect";
        connect.type = WidgetType.Button;
        connect.nextWidget = 2;
        connect.pressable = false; // Can not be pressed until server is selected

        connectToIp = new MenuWidget(); // Connect to IP
        connectToIp.text = "Connect to IP";
        connectToIp.type = WidgetType.Button;
        connectToIp.nextWidget = 0;

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

        loggedInName = new MenuWidget(); // Loagged in Name
        loggedInName.text = "";
        loggedInName.type = WidgetType.Button;
        loggedInName.buttonStyle = ButtonStyle.Text;

        logout = new MenuWidget(); // Logout
        logout.text = "Logout";
        logout.type = WidgetType.Button;
        //logout.image = "serverlist_entry_background.png";
        logout.buttonStyle = ButtonStyle.Button;

        login = new MenuWidget(); // Log in
        login.text = "Log in";
        login.type = WidgetType.Button;
        //login.image = "serverlist_entry_background.png";
        login.buttonStyle = ButtonStyle.Button;

        // Add buttons to widget collection
        widgets[0] = back;
        widgets[1] = connect;
        widgets[2] = connectToIp;
        widgets[3] = refresh;
        widgets[4] = pageUp;
        widgets[5] = pageDown;
        widgets[6] = loggedInName;
        widgets[7] = logout;
        widgets[8] = login;

        // Add server buttons to widget collection (after the more static buttons)
        serverButtons = new MenuWidget[serverButtonsCount];
        for (int i = 0; i < serverButtonsCount; i++)
        {
            // Create server button
            MenuWidget b = new MenuWidget();
            b = new MenuWidget();
            b.text = "Invalid";
            b.type = WidgetType.Button;
            b.buttonStyle = ButtonStyle.ServerEntry;
            b.visible = false;
            b.image = "serverlist_entry_noimage.png";
            b.nextWidget = 0; // Select play when tab is pressed

            serverButtons[i] = b;
            widgets[9 + i] = b;
        }

        // Set screen title
        title = "Multiplayer";

        serverListAddress = new HttpResponseCi();
        serverListCsv = new HttpResponseCi();
        serversOnList = new ServerOnList[serversOnListCount];
        thumbResponses = new ThumbnailResponseCi[serversOnListCount];

        loading = true; // Load servers
    }

    bool loaded;
    HttpResponseCi serverListAddress;
    HttpResponseCi serverListCsv;
    ServerOnList[] serversOnList;
    const int serversOnListCount = 1024;
    int serverCount;
    int page;
    int pageCount;
    int serversPerPage;
    string title;
    bool loading;

    int oldWidth; // CanvasWidth from last rendering (frame)
    int oldHeight; // CanvasHeight from last rendering (frame)

    public override void LoadTranslations()
    {
        back.text = menu.lang.Get("MainMenu_ButtonBack");
        connect.text = menu.lang.Get("MainMenu_MultiplayerConnect");
        connectToIp.text = menu.lang.Get("MainMenu_MultiplayerConnectIP");
        title = menu.lang.Get("MainMenu_Multiplayer");
    }

    public override void Render(float dt)
    {
        // Fetch server list
        if (!loaded) // Get server list
        {
            menu.p.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/serverlistcsv.php", serverListAddress);
            loaded = true;
            connect.pressable = false; // User has to select a server again
        }
        if (serverListAddress.done)
        {
            serverListAddress.done = false;
            menu.p.WebClientDownloadDataAsync(serverListAddress.GetString(menu.p), serverListCsv);
        }
        if (serverListCsv.done) // Create server entries
        {
            loading = false;
            serverListCsv.done = false;
            for (int i = 0; i < serversOnListCount; i++)
            {
                serversOnList[i] = null;
                thumbResponses[i] = null;
            }
            IntRef serversCount = new IntRef();
            string[] servers = menu.p.StringSplit(serverListCsv.GetString(menu.p), "\n", serversCount);
            for (int i = 0; i < serversCount.value; i++)
            {
                IntRef ssCount = new IntRef();
                string[] ss = menu.p.StringSplit(servers[i], "\t", ssCount);
                if (ssCount.value < 10)
                {
                    continue;
                }
                ServerOnList s = new ServerOnList();
                s.hash = ss[0];
                s.name = menu.p.DecodeHTMLEntities(ss[1]);
                s.motd = menu.p.DecodeHTMLEntities(ss[2]);
                s.port = menu.p.IntParse(ss[3]);
                s.ip = ss[4];
                s.version = ss[5];
                s.users = menu.p.IntParse(ss[6]);
                s.max = menu.p.IntParse(ss[7]);
                s.gamemode = ss[8];
                s.players = ss[9];
                serversOnList[i] = s;
            }

            serverCount = serversCount.value - 1; // Keep the number of servers
            UpdateServerSelection(); // Select the same server (if it is still reachable)
        }

        GamePlatform p = menu.p;

        // Update thumbnails
        UpdateThumbnails();

        // Logged in name
        if (loggedInName.text == "")
        {
            if (p.GetPreferences().GetString("Password", "") != "")
            {
                loggedInName.text = p.GetPreferences().GetString("Username", "Invalid");
            }
        }

        // Show login our logout
        logout.visible = (loggedInName.text != ""); // If a user is logged in (show logout)
        login.visible = !logout.visible; // If logout hidden (show login)

        // Screen measurements
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            // Amount of entries that fits on the screen
            serversPerPage = menu.p.FloatToInt((height - 2f * 100f * scale) / (70f * scale));

            // Amount of pages
            if (serversPerPage > 0)
            {
                pageCount = (serverCount - 1) / serversPerPage;
                if (pageCount < 0) { pageCount = 0; } // Stop page count from being negative
            }
            else
            {
                pageCount = 0;
            }

            // Stop the current page from being beyond the last page
            if (page > pageCount)
            {
                page = pageCount;
                UpdateServerSelection();
            }

            // Back button
            back.x = 40f * scale;
            back.y = height - 104f * scale;
            back.sizex = 256f * scale;
            back.sizey = 64f * scale;
            back.fontSize = 14f * scale;

            // Connect button
            connect.x = width - 592f * scale;
            connect.y = height - 104f * scale;
            connect.sizex = 256f * scale;
            connect.sizey = 64f * scale;
            connect.fontSize = 14f * scale;

            // Connect to ip button
            connectToIp.x = width - 296f * scale;
            connectToIp.y = height - 104f * scale;
            connectToIp.sizex = 256f * scale;
            connectToIp.sizey = 64f * scale;
            connectToIp.fontSize = 14f * scale;

            // Refresh button
            refresh.x = 100f * scale;
            refresh.y = 30f * scale;
            refresh.sizex = 64f * scale;
            refresh.sizey = 64f * scale;

            // Page up button
            pageUp.x = width - 94f * scale;
            pageUp.y = 100f * scale + (serversPerPage - 1) * 70f * scale;
            pageUp.sizex = 64f * scale;
            pageUp.sizey = 64f * scale;

            // Page down button
            pageDown.x = width - 94f * scale;
            pageDown.y = 100f * scale;
            pageDown.sizex = 64f * scale;
            pageDown.sizey = 64f * scale;

            // Logged in name (label)
            loggedInName.x = width - 228f * scale;
            loggedInName.y = 32f * scale;
            loggedInName.sizex = 128f * scale;
            loggedInName.sizey = 32f * scale;
            loggedInName.fontSize = 12f * scale;
            
            // Logout button
            logout.x = width - 228f * scale;
            logout.y = 62f * scale;
            logout.sizex = 128f * scale;
            logout.sizey = 32f * scale;
            logout.fontSize = 12f * scale;
            
            // Log in button
            login.x = width - 228f * scale;
            login.y = 62f * scale;
            login.sizex = 128f * scale;
            login.sizey = 32f * scale;
            login.fontSize = 12f * scale;
        }

        // Draw
        menu.DrawBackground(); // Draw background
        menu.DrawText(title, 20f * scale, p.GetCanvasWidth() / 2f, 10f, TextAlign.Center, TextBaseline.Top); // Draw title text
        menu.DrawText(p.StringFormat2("{0}/{1}", p.IntToString(page + 1), p.IntToString(pageCount + 1)), 14f * scale,
                      width - 68f * scale, (100f * scale) + (serversPerPage * 70f * scale) / 2f, TextAlign.Center, TextBaseline.Middle); // Draw page number

        // Hide all server entries
        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].visible = false;
        }

        // Draw server entries
        for (int i = 0; i < serversPerPage; i++)
        {
            // Get current server entrys index
            int index = i + (serversPerPage * page); // Get index
            if (index > serversOnListCount) { break; } // If the last entry is reached
            ServerOnList s = serversOnList[index]; // Get entries server data
            if (s == null) { continue; } // Skip if server entry is missing server data

            // Format server data
            string t = menu.p.StringFormat2("{1}", menu.p.IntToString(index), s.name); // Name
            t = menu.p.StringFormat2("{0}\n{1}", t, s.motd); // MOTD
            t = menu.p.StringFormat2("{0}\n{1}", t, s.gamemode); // Game mode
            t = menu.p.StringFormat2("{0}\n{1}", t, menu.p.IntToString(s.users)); // Users online
            t = menu.p.StringFormat2("{0}/{1}", t, menu.p.IntToString(s.max)); // User cap
            t = menu.p.StringFormat2("{0}\n{1}", t, s.version); // Version (for comparing)
            serverButtons[i].text = t; // Replace widget text with formatted text
            serverButtons[i].visible = true; // Make visable

            // Update size/position
            //if (resized)
            //{
                serverButtons[i].x = 100f * scale;
                serverButtons[i].y = 100f * scale + i * 70f * scale;
                serverButtons[i].sizex = width - 200f * scale;
                serverButtons[i].sizey = 64f * scale;
            //}

            if (s.thumbnailError) // If server did not respond to ServerQuery. Maybe not reachable?
            {
                serverButtons[i].description = "Server did not respond to query!";
            }
            else
            {
                serverButtons[i].description = null;
            }

            if (s.thumbnailFetched && !s.thumbnailError) // Thumbnail fetched with error
            {
                serverButtons[i].image = menu.p.StringFormat("serverlist_entry_{0}.png", s.hash);
            }
            else
            {
                serverButtons[i].image = "serverlist_entry_noimage.png";
            }
        }

        // Update scrollbars
        UpdateScrollButtons();

        // Draw widgets
        DrawWidgets();

        // Draw loading text (if loading servers)
        if (loading)
        {
            menu.DrawText(menu.lang.Get("MainMenu_MultiplayerLoading"), 14f * scale, 174f * scale, 50f * scale, TextAlign.Left, TextBaseline.Top);
        }

        // Update old(Width/Height)
        oldWidth = width;
        oldHeight = height;
    }

    ThumbnailResponseCi[] thumbResponses;
    public void UpdateThumbnails()
    {
        for (int i = 0; i < serversOnListCount; i++)
        {
            ServerOnList server = serversOnList[i]; // Get current server entry (int the loop)

            // Server entry is null
            if (server == null) { continue; }

            // Thumbnail already loaded
            if (server.thumbnailFetched) { continue; }

            // Check download state
            if (!server.thumbnailDownloading) // Not started downloading yet
            {
                // Start downloading
                thumbResponses[i] = new ThumbnailResponseCi();
                menu.p.ThumbnailDownloadAsync(server.ip, server.port, thumbResponses[i]);
                server.thumbnailDownloading = true;
            }
            else
            {
                // Download in progress
                if (thumbResponses[i] != null)
                {
                    if (thumbResponses[i].done) // If download is done
                    {
                        //Request completed. load received bitmap
                        BitmapCi bmp = menu.p.BitmapCreateFromPng(thumbResponses[i].data, thumbResponses[i].dataLength); // Get thumbnail
                        if (bmp != null) // Set server entries thumbnail (if there is one)
                        {
                            int texture = menu.p.LoadTextureFromBitmap(bmp);
                            menu.textures.Set(menu.p.StringFormat("serverlist_entry_{0}.png", server.hash), texture);
                            menu.p.BitmapDelete(bmp);
                        }
                        server.thumbnailDownloading = false;
                        server.thumbnailFetched = true;
                    }

                    if (thumbResponses[i].error) // If an error occured (failed download)
                    {
                        server.thumbnailDownloading = false;
                        server.thumbnailError = true;
                        server.thumbnailFetched = true;
                    }
                }
                else // An error occured. Stop trying
                {
                    server.thumbnailDownloading = false;
                    server.thumbnailError = true;
                    server.thumbnailFetched = true;
                }
            }
        }
    }

    public void PageUp_()
    {
        if (pageUp.visible && page < serverButtonsCount / serversPerPage - 1)
        {
            page++;
            UpdateServerSelection();
        }
    }
    public void PageDown_()
    {
        if (page > 0)
        {
            page--;
            UpdateServerSelection();
        }
    }
    public void UpdateServerSelection()
    {
        // Deselect all server buttons
        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].selected = false;
        }

        // Find selected servers index
        int serverIndex = -1;
        for (int i = 0; i < serverCount; i++)
        {
            if (serversOnList[i].hash == selectedServerHash) // If server is selected server
            {
                serverIndex = i; // Keep selected servers index
                break;
            }
        }

        // Abort if no server is selected
        if (serverIndex == -1) { return; }

        // Abort if no server can not be selected (selected server is in a previous page)
        if (serverIndex - (serversPerPage * page) < 0) { return; }

        // Select selected server
        serverButtons[serverIndex - (serversPerPage * page)].selected = true;

        // Make connect button pressable
        connect.pressable = true;
    }
    public void UpdateScrollButtons()
    {
        // Determine if this page is the highest page containing servers
        bool maxpage = false;
        if (serversPerPage <= 0 // Disable page up if there are no servers per page
            || (page + 1) * serversPerPage >= serversOnListCount
            || (page + 1) * serversPerPage >= serverCount)
        {
            maxpage = true;
        }

        // Hide scroll buttons
        if (page == 0) { pageDown.visible = false; }
        else { pageDown.visible = true; }

        if (maxpage) { pageUp.visible = false; }
        else { pageUp.visible = true; }
    }

    MenuWidget back;
    MenuWidget connect;
    MenuWidget connectToIp;
    MenuWidget refresh;
    MenuWidget pageUp;
    MenuWidget pageDown;
    MenuWidget loggedInName;
    MenuWidget logout;
    MenuWidget login;
    MenuWidget[] serverButtons;
    const int serverButtonsCount = 1024;

    public override void OnBackPressed()
    {
        menu.StartMainMenu(); // Go to the main menu
    }
    public override void OnMouseWheel(MouseWheelEventArgs e)
    {
        //menu.p.MessageBoxShowError(menu.p.IntToString(e.GetDelta()), "Delta");
        if (e.GetDelta() < 0) { PageUp_(); } //Mouse wheel turned down
        else if (e.GetDelta() > 0) { PageDown_(); } //Mouse wheel turned up
    }
    string selectedServerHash;
    public override void OnButton(MenuWidget w)
    {
        // Check all server buttons
        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].selected = false; // Deselect server button
            if (serverButtons[i] == w) // If server button is clicked
            {
                // Select server
                serverButtons[i].selected = true;
                if (serversOnList[i + serversPerPage * page] != null)
                    selectedServerHash = serversOnList[i + serversPerPage * page].hash;

                connect.pressable = true; // Make connect button  pressable
            }
        }

        // Check what button was clicked
        if (w == pageUp) // Page up
        {
            PageUp_();
        }
        else if (w == pageDown) // Page down
        {
            PageDown_();
        }
        else if (w == back) // Back
        {
            OnBackPressed();
        }
        else if (w == connect) // Connect
        {
            if (connect.pressable && selectedServerHash != null) // If a server is selected
            {
                menu.StartLogin(selectedServerHash, null, 0, true); // Connect (or log in)
            }
        }
        else if (w == connectToIp) // Connect to ip
        {
            menu.StartConnectToIp();
        }
        else if (w == refresh) // Refresh
        {
            loaded = false; // (Reset this because it is only used while loading)
            loading = true; // Reload servers next frame
        }
        else if (w == logout) // Log out
        {
            Preferences pref = menu.p.GetPreferences();
            pref.Remove("Username");
            pref.Remove("Password");
            menu.p.SetPreferences(pref);
            loggedInName.text = "";
        }
        else if (w == login) // Log in
        {
            menu.StartLogin(null, null, 0, false); // Log in
        }
    }
}

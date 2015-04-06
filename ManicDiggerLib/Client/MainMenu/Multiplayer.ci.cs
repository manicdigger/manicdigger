public class ScreenMultiplayer : Screen
{
    public ScreenMultiplayer()
    {
        // Set widget length (because it is extended)
        WidgetCount = 64 + serverButtonsCount;
        widgets = new MenuWidget[WidgetCount];

        // Create buttons
        back = new MenuWidget();
        back.text = "Back";
        back.type = WidgetType.Button;
        back.nextWidget = 1;

        connect = new MenuWidget();
        connect.text = "Connect";
        connect.type = WidgetType.Button;
        connect.nextWidget = 3;
        connectToIp = new MenuWidget();
        connectToIp.text = "Connect to IP";
        connectToIp.type = WidgetType.Button;
        connectToIp.nextWidget = 2;

        refresh = new MenuWidget();
        refresh.text = "Refresh";
        refresh.type = WidgetType.Button;
        refresh.nextWidget = 0;

        pageUp = new MenuWidget();
        pageUp.text = "";
        pageUp.type = WidgetType.Button;
        pageUp.buttonStyle = ButtonStyle.Text;
        pageUp.visible = false;
        pageUp.image = "serverlist_nav_down.png";

        pageDown = new MenuWidget();
        pageDown.text = "";
        pageDown.type = WidgetType.Button;
        pageDown.buttonStyle = ButtonStyle.Text;
        pageDown.visible = false;
        pageDown.image = "serverlist_nav_up.png";

        loggedInName = new MenuWidget();
        loggedInName.text = "";
        loggedInName.type = WidgetType.Button;
        loggedInName.buttonStyle = ButtonStyle.Text;

        logout = new MenuWidget();
        logout.text = "Logout";
        logout.type = WidgetType.Button;
        //logout.image = "serverlist_entry_background.png";
        logout.buttonStyle = ButtonStyle.Button;

        // Add buttons to widget collection
        widgets[0] = back;
        widgets[1] = connect;
        widgets[2] = refresh;
        widgets[3] = connectToIp;
        widgets[4] = pageUp;
        widgets[5] = pageDown;
        widgets[6] = loggedInName;
        widgets[7] = logout;

        // Add server buttons to widget collection (after the more static buttons)
        serverButtons = new MenuWidget[serverButtonsCount];
        for (int i = 0; i < serverButtonsCount; i++)
        {
            MenuWidget b = new MenuWidget();
            b = new MenuWidget();
            b.text = "Invalid";
            b.type = WidgetType.Button;
            b.buttonStyle = ButtonStyle.ServerEntry;
            b.visible = false;
            b.image = "serverlist_entry_noimage.png";
            serverButtons[i] = b;
            widgets[8 + i] = b;
        }

        // Set screen title
        title = "Multiplayer";

        // 
        page = 0;

        serverListAddress = new HttpResponseCi();
        serverListCsv = new HttpResponseCi();
        serversOnList = new ServerOnList[serversOnListCount];
        thumbResponses = new ThumbnailResponseCi[serversOnListCount];

        loading = true;
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
        refresh.text = menu.lang.Get("MainMenu_MultiplayerRefresh");
        title = menu.lang.Get("MainMenu_Multiplayer");
    }

    public override void Render(float dt)
    {
        // Fetch server list
        if (!loaded) // Get server list
        {
            menu.p.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/serverlistcsv.php", serverListAddress);
            loaded = true;
        }
        if (serverListAddress.done) // Not sure
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
                    continue;
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
        }

        GamePlatform p = menu.p;

        //
        int width = p.GetCanvasWidth();
        int height = p.GetCanvasHeight();
        float scale = menu.GetScale();

        bool resized = (width != oldWidth || height != oldHeight); // If the screen has changed size

        // Update positioning and scale when needed
        if (resized)
        {
            // Amount of entries that fits on the screen
            serversPerPage = menu.p.FloatToInt((height - (2f * 100f * scale)) / 70f * scale);

            // Amount of pages
            if (serversPerPage > 0)
                pageCount = (serverCount - 1) / serversPerPage;
            else
                pageCount = 0;

            // Stop the current page from being beyond the last page
            if (page > pageCount)
                page = pageCount;

            // Back button
            back.x = 40f * scale;
            back.y = height - 104 * scale;
            back.sizex = 256f * scale;
            back.sizey = 64f * scale;
            back.fontSize = 14f * scale;

            // Connect button
            connect.x = width - 888 * scale;
            connect.y = height - 104f * scale;
            connect.sizex = 256f * scale;
            connect.sizey = 64f * scale;
            connect.fontSize = 14f * scale;

            // Connect to ip button
            connectToIp.x = width - 592 * scale;
            connectToIp.y = height - 104f * scale;
            connectToIp.sizex = 256f * scale;
            connectToIp.sizey = 64f * scale;
            connectToIp.fontSize = 14f * scale;

            // Refresh button
            refresh.x = width - 296 * scale;
            refresh.y = height - 104f * scale;
            refresh.sizex = 256f * scale;
            refresh.sizey = 64f * scale;
            refresh.fontSize = 14f * scale;

            // Page up button
            pageUp.x = width - 94 * scale;
            pageUp.y = 100f * scale + (serversPerPage - 1) * 70f * scale;
            pageUp.sizex = 64f * scale;
            pageUp.sizey = 64f * scale;

            // Page down button
            pageDown.x = width - 94 * scale;
            pageDown.y = 100f * scale;
            pageDown.sizex = 64f * scale;
            pageDown.sizey = 64f * scale;

            // Logged in name (label)
            loggedInName.x = width - 228 * scale;
            loggedInName.y = 32f * scale;
            loggedInName.sizex = 128f * scale;
            loggedInName.sizey = 32f * scale;
            loggedInName.fontSize = 12f * scale;
            
            // Logout button
            logout.x = width - 228 * scale;
            logout.y = 62f * scale;
            logout.sizex = 128f * scale;
            logout.sizey = 32f * scale;
            logout.fontSize = 12f * scale;
        }

        // Logged in name
        if (loggedInName.text == "")
            if (p.GetPreferences().GetString("Password", "") != "")
                loggedInName.text = p.GetPreferences().GetString("Username", "Invalid");

        // Show log out button if a user is logged in
        logout.visible = loggedInName.text != "";

        // Draw
        menu.DrawBackground(); // Draw background
        menu.DrawText(title, 20 * scale, p.GetCanvasWidth() / 2, 10, TextAlign.Center, TextBaseline.Top); // Draw title text
        menu.DrawText(p.StringFormat2("{0}/{1}", p.IntToString(page + 1), p.IntToString(pageCount + 1)), 14 * scale,
                      width - 68 * scale, 100 + (height - (2f * 100f * scale)) / 2, TextAlign.Center, TextBaseline.Middle); // Draw page number

        // Draw loading text (if loading servers)
        if (loading)
            menu.DrawText(menu.lang.Get("MainMenu_MultiplayerLoading"), 14 * scale, 100 * scale, 50 * scale, TextAlign.Left, TextBaseline.Top);

        UpdateThumbnails();

        // Hide all server entries
        for (int i = 0; i < serverButtonsCount; i++)
            serverButtons[i].visible = false;

        // Draw server entries
        for (int i = 0; i < serversPerPage; i++)
        {
            // Get current server entrys index
            int index = i + (serversPerPage * page); // Get index
            if (index > serversOnListCount) break; // If the last entry is reached
            ServerOnList s = serversOnList[index]; // Get entries server data
            if (s == null) continue; // Skip if server entry is missing server data

            // Format server data
            string t = menu.p.StringFormat2("{1}", menu.p.IntToString(index), s.name);
            t = menu.p.StringFormat2("{0}\n{1}", t, s.motd);
            t = menu.p.StringFormat2("{0}\n{1}", t, s.gamemode);
            t = menu.p.StringFormat2("{0}\n{1}", t, menu.p.IntToString(s.users));
            t = menu.p.StringFormat2("{0}/{1}", t, menu.p.IntToString(s.max));
            t = menu.p.StringFormat2("{0}\n{1}", t, s.version);
            serverButtons[i].text = t; // Replace widget text with formatted text
            serverButtons[i].visible = true; // Make visable

            // Update size/position
            //if (resized)
            //{
                serverButtons[i].x = 100 * scale;
                serverButtons[i].y = 100 * scale + i * 70 * scale;
                serverButtons[i].sizex = width - 200 * scale;
                serverButtons[i].sizey = 64 * scale;
            //}

            if (s.thumbnailError) // If server did not respond to ServerQuery. Maybe not reachable?
                serverButtons[i].description = "Server did not respond to query!";
            else
                serverButtons[i].description = null;
            
            if (s.thumbnailFetched && !s.thumbnailError) // Thumbnail fetched with error
                serverButtons[i].image = menu.p.StringFormat("serverlist_entry_{0}.png", s.hash);
            else
                serverButtons[i].image = "serverlist_entry_noimage.png";
        }

        // Update scrollbars
        UpdateScrollButtons();

        // Draw widgets
        DrawWidgets();

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
            if (server == null) continue;

            // Thumbnail already loaded
            if (server.thumbnailFetched) continue;

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
            page++;
    }
    public void PageDown_()
    {
        if (page > 0)
            page--;
    }
    public void UpdateScrollButtons()
    {
        // Determine if this page is the highest page containing servers
        bool maxpage = false;
        if (serversPerPage <= 0) // Disable page up if there are no servers per page
            maxpage = true;
        else if ((page + 1) * serversPerPage >= serversOnListCount)
            maxpage = true;
        else if ((page + 1) * serversPerPage >= serverCount)
            maxpage = true;

        // Hide scroll buttons
        if (page == 0)
            pageDown.visible = false;
        else
            pageDown.visible = true;

        if (maxpage)
            pageUp.visible = false;
        else
            pageUp.visible = true;
    }
    MenuWidget back;
    MenuWidget connect;
    MenuWidget connectToIp;
    MenuWidget refresh;
    MenuWidget pageUp;
    MenuWidget pageDown;
    MenuWidget loggedInName;
    MenuWidget logout;
    MenuWidget[] serverButtons;
    const int serverButtonsCount = 1024;

    public override void OnBackPressed()
    {
        menu.StartMainMenu();
    }
    public override void OnMouseWheel(MouseWheelEventArgs e)
    {
        //menu.p.MessageBoxShowError(menu.p.IntToString(e.GetDelta()), "Delta");
        if (e.GetDelta() < 0)
        {
            //Mouse wheel turned down
            PageUp_();
        }
        else if (e.GetDelta() > 0)
        {
            //Mouse wheel turned up
            PageDown_();
        }
    }
    string selectedServerHash;
    public override void OnButton(MenuWidget w)
    {
        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].selected = false;
            if (serverButtons[i] == w)
            {
                serverButtons[i].selected = true;
                if (serversOnList[i + serversPerPage * page] != null)
                    selectedServerHash = serversOnList[i + serversPerPage * page].hash;
            }
        }

        // Check what button was clicked
        if (w == pageUp) // Page up
            PageUp_();
        else if (w == pageDown) // Page down
            PageDown_();
        else if (w == back) // Back
            OnBackPressed();
        else if (w == connect) // Connect
        {
            if (selectedServerHash != null)
                menu.StartLogin(selectedServerHash, null, 0);
        }
        else if (w == connectToIp) // Connect to ip
            menu.StartConnectToIp();
        else if (w == refresh) // Refresh
        {
            loaded = false;
            loading = true;
        }
        else if (w == logout) // Log out
        {
            Preferences pref = menu.p.GetPreferences();
            pref.Remove("Username");
            pref.Remove("Password");
            menu.p.SetPreferences(pref);
            loggedInName.text = "";
        }
    }
}

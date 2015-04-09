public class ScreenMultiplayer : Screen
{
    public ScreenMultiplayer()
    {
        WidgetCount = 64 + serverButtonsCount;
        widgets = new MenuWidget[WidgetCount];
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

        page = 0;
        pageUp = new MenuWidget();
        pageUp.text = "";
        pageUp.type = WidgetType.Button;
        pageUp.buttonStyle = ButtonStyle.Text;
        pageUp.visible = false;
        pageDown = new MenuWidget();
        pageDown.text = "";
        pageDown.type = WidgetType.Button;
        pageDown.buttonStyle = ButtonStyle.Text;
        pageDown.visible = false;

        loggedInName = new MenuWidget();
        loggedInName.text = "";
        loggedInName.type = WidgetType.Button;
        loggedInName.buttonStyle = ButtonStyle.Text;

        logout = new MenuWidget();
        logout.text = "";
        logout.type = WidgetType.Button;
        //logout.image = "serverlist_entry_background.png";
        logout.buttonStyle = ButtonStyle.Button;

        title = "Multiplayer";

        widgets[0] = back;
        widgets[1] = connect;
        widgets[2] = refresh;
        widgets[3] = connectToIp;
        widgets[4] = pageUp;
        widgets[5] = pageDown;
        widgets[6] = loggedInName;
        widgets[7] = logout;

        serverListAddress = new HttpResponseCi();
        serverListCsv = new HttpResponseCi();
        serversOnList = new ServerOnList[serversOnListCount];
        thumbResponses = new ThumbnailResponseCi[serversOnListCount];

        serverButtons = new MenuWidget[serverButtonsCount];
        for (int i = 0; i < serverButtonsCount; i++)
        {
            MenuWidget b = new MenuWidget();
            b = new MenuWidget();
            b.text = "Invalid";
            b.type = WidgetType.Button;
            b.visible = false;
            b.image = "serverlist_entry_noimage.png";
            serverButtons[i] = b;
            widgets[8 + i] = b;
        }
        loading = true;
    }

    bool loaded;
    HttpResponseCi serverListAddress;
    HttpResponseCi serverListCsv;
    ServerOnList[] serversOnList;
    const int serversOnListCount = 1024;
    int page;
    int serversPerPage;
    string title;
    bool loading;

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
        if (!loaded)
        {
            menu.p.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/serverlistcsv.php", serverListAddress);
            loaded = true;
        }
        if (serverListAddress.done)
        {
            serverListAddress.done = false;
            menu.p.WebClientDownloadDataAsync(serverListAddress.GetString(menu.p), serverListCsv);
        }
        if (serverListCsv.done)
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
        }

        GamePlatform p = menu.p;

        float scale = menu.GetScale();

        back.x = 40 * scale;
        back.y = p.GetCanvasHeight() - 104 * scale;
        back.sizex = 256 * scale;
        back.sizey = 64 * scale;
        back.fontSize = 14 * scale;

        connect.x = p.GetCanvasWidth() / 2 - 300 * scale;
        connect.y = p.GetCanvasHeight() - 104 * scale;
        connect.sizex = 256 * scale;
        connect.sizey = 64 * scale;
        connect.fontSize = 14 * scale;

        connectToIp.x = p.GetCanvasWidth() / 2 - 0 * scale;
        connectToIp.y = p.GetCanvasHeight() - 104 * scale;
        connectToIp.sizex = 256 * scale;
        connectToIp.sizey = 64 * scale;
        connectToIp.fontSize = 14 * scale;

        refresh.x = p.GetCanvasWidth() / 2 + 350 * scale;
        refresh.y = p.GetCanvasHeight() - 104 * scale;
        refresh.sizex = 256 * scale;
        refresh.sizey = 64 * scale;
        refresh.fontSize = 14 * scale;

        pageUp.x = p.GetCanvasWidth() - 94 * scale;
        pageUp.y = 100 * scale + (serversPerPage - 1) * 70 * scale;
        pageUp.sizex = 64 * scale;
        pageUp.sizey = 64 * scale;
        pageUp.image = "serverlist_nav_down.png";

        pageDown.x = p.GetCanvasWidth() - 94 * scale;
        pageDown.y = 100 * scale;
        pageDown.sizex = 64 * scale;
        pageDown.sizey = 64 * scale;
        pageDown.image = "serverlist_nav_up.png";

        loggedInName.x = p.GetCanvasWidth() - 228 * scale;
        loggedInName.y = 32 * scale;
        loggedInName.sizex = 128 * scale;
        loggedInName.sizey = 32 * scale;
        loggedInName.fontSize = 12 * scale;
        if (loggedInName.text == "")
        {
            if (p.GetPreferences().GetString("Password", "") != "")
            {
                loggedInName.text = p.GetPreferences().GetString("Username", "Invalid");
            }
        }
        logout.visible = loggedInName.text != "";

        logout.x = p.GetCanvasWidth() - 228 * scale;
        logout.y = 62 * scale;
        logout.sizex = 128 * scale;
        logout.sizey = 32 * scale;
        logout.fontSize = 12 * scale;
        logout.text = "Logout";

        menu.DrawBackground();
        menu.DrawText(title, 20 * scale, p.GetCanvasWidth() / 2, 10, TextAlign.Center, TextBaseline.Top);
        menu.DrawText(p.IntToString(page + 1), 14 * scale, p.GetCanvasWidth() - 68 * scale, p.GetCanvasHeight() / 2, TextAlign.Center, TextBaseline.Middle);

        if (loading)
        {
            menu.DrawText(menu.lang.Get("MainMenu_MultiplayerLoading"), 14 * scale, 100 * scale, 50 * scale, TextAlign.Left, TextBaseline.Top);
        }

        UpdateThumbnails();
        for (int i = 0; i < serverButtonsCount; i++)
        {
            serverButtons[i].visible = false;
        }

        serversPerPage = menu.p.FloatToInt((menu.p.GetCanvasHeight() - (2 * 100 * scale)) / 70 * scale);
        if (serversPerPage <= 0)
        {
            // Do not let this get negative
            serversPerPage = 1;
        }
        for (int i = 0; i < serversPerPage; i++)
        {
            int index = i + (serversPerPage * page);
            if (index > serversOnListCount)
            {
                //Reset to first page
                page = 0;
                index = i + (serversPerPage * page);
            }
            ServerOnList s = serversOnList[index];
            if (s == null)
            {
                continue;
            }
            string t = menu.p.StringFormat2("{1}", menu.p.IntToString(index), s.name);
            t = menu.p.StringFormat2("{0}\n{1}", t, s.motd);
            t = menu.p.StringFormat2("{0}\n{1}", t, s.gamemode);
            t = menu.p.StringFormat2("{0}\n{1}", t, menu.p.IntToString(s.users));
            t = menu.p.StringFormat2("{0}/{1}", t, menu.p.IntToString(s.max));
            t = menu.p.StringFormat2("{0}\n{1}", t, s.version);

            serverButtons[i].text = t;
            serverButtons[i].x = 100 * scale;
            serverButtons[i].y = 100 * scale + i * 70 * scale;
            serverButtons[i].sizex = p.GetCanvasWidth() - 200 * scale;
            serverButtons[i].sizey = 64 * scale;
            serverButtons[i].visible = true;
            serverButtons[i].buttonStyle = ButtonStyle.ServerEntry;
            if (s.thumbnailError)
            {
                //Server did not respond to ServerQuery. Maybe not reachable?
                serverButtons[i].description = "Server did not respond to query!";
            }
            else
            {
                serverButtons[i].description = null;
            }
            if (s.thumbnailFetched && !s.thumbnailError)
            {
                serverButtons[i].image = menu.p.StringFormat("serverlist_entry_{0}.png", s.hash);
            }
            else
            {
                serverButtons[i].image = "serverlist_entry_noimage.png";
            }
        }
        UpdateScrollButtons();
        DrawWidgets();
    }

    ThumbnailResponseCi[] thumbResponses;
    public void UpdateThumbnails()
    {
        for (int i = 0; i < serversOnListCount; i++)
        {
            ServerOnList server = serversOnList[i];
            if (server == null)
            {
                continue;
            }
            if (server.thumbnailFetched)
            {
                //Thumbnail already loaded
                continue;
            }
            if (!server.thumbnailDownloading)
            {
                //Not started downloading yet
                thumbResponses[i] = new ThumbnailResponseCi();
                menu.p.ThumbnailDownloadAsync(server.ip, server.port, thumbResponses[i]);
                server.thumbnailDownloading = true;
            }
            else
            {
                //Download in progress
                if (thumbResponses[i] != null)
                {
                    if (thumbResponses[i].done)
                    {
                        //Request completed. load received bitmap
                        BitmapCi bmp = menu.p.BitmapCreateFromPng(thumbResponses[i].data, thumbResponses[i].dataLength);
                        if (bmp != null)
                        {
                            int texture = menu.p.LoadTextureFromBitmap(bmp);
                            menu.textures.Set(menu.p.StringFormat("serverlist_entry_{0}.png", server.hash), texture);
                            menu.p.BitmapDelete(bmp);
                        }
                        server.thumbnailDownloading = false;
                        server.thumbnailFetched = true;
                    }
                    if (thumbResponses[i].error)
                    {
                        //Error while trying to download thumbnail
                        server.thumbnailDownloading = false;
                        server.thumbnailError = true;
                        server.thumbnailFetched = true;
                    }
                }
                else
                {
                    //An error occured. stop trying
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
        }
    }
    public void PageDown_()
    {
        if (page > 0)
        {
            page--;
        }
    }
    public void UpdateScrollButtons()
    {
        //Determine if this page is the highest page containing servers
        bool maxpage = false;
        if ((page + 1) * serversPerPage >= serversOnListCount)
        {
            maxpage = true;
        }
        else
        {
            if (serversOnList[(page + 1) * serversPerPage] == null)
            {
                maxpage = true;
            }
        }
        //Hide scroll buttons
        if (page == 0)
        {
            pageDown.visible = false;
        }
        else
        {
            pageDown.visible = true;
        }
        if (maxpage)
        {
            pageUp.visible = false;
        }
        else
        {
            pageUp.visible = true;
        }
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
                {
                    selectedServerHash = serversOnList[i + serversPerPage * page].hash;
                }
            }
        }
        if (w == pageUp)
        {
            PageUp_();
        }
        if (w == pageDown)
        {
            PageDown_();
        }
        if (w == back)
        {
            OnBackPressed();
        }
        if (w == connect)
        {
            if (selectedServerHash != null)
            {
                menu.StartLogin(selectedServerHash, null, 0);
            }
        }
        if (w == connectToIp)
        {
            menu.StartConnectToIp();
        }
        if (w == refresh)
        {
            loaded = false;
            loading = true;
        }
        if (w == logout)
        {
            Preferences pref = menu.p.GetPreferences();
            pref.Remove("Username");
            pref.Remove("Password");
            menu.p.SetPreferences(pref);
            loggedInName.text = "";
        }
    }
}

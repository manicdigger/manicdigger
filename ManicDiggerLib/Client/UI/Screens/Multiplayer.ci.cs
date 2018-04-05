public class ScreenMultiplayer : Screen
{
	public ScreenMultiplayer()
	{
		// Button Widgets
		wbtn_back = new ButtonWidget();
		wbtn_back.SetClickable(true);
		wbtn_back.SetText("Back");
		AddWidgetNew(wbtn_back);
		wbtn_connect = new ButtonWidget();
		wbtn_connect.SetClickable(true);
		wbtn_connect.SetText("Connect");
		AddWidgetNew(wbtn_connect);
		wbtn_connectToIp = new ButtonWidget();
		wbtn_connectToIp.SetClickable(true);
		wbtn_connectToIp.SetText("Connect to IP");
		AddWidgetNew(wbtn_connectToIp);
		wbtn_refresh = new ButtonWidget();
		wbtn_refresh.SetClickable(true);
		wbtn_refresh.SetText("Refresh");
		AddWidgetNew(wbtn_refresh);
		wbtn_logout = new ButtonWidget();
		wbtn_logout.SetClickable(true);
		wbtn_logout.SetText("Logout");
		wbtn_logout.SetVisible(false);
		AddWidgetNew(wbtn_logout);
		wbtn_pageUp = new ButtonWidget();
		wbtn_pageUp.SetClickable(true);
		wbtn_pageUp.SetVisible(false);
		AddWidgetNew(wbtn_pageUp);
		wbtn_pageDown = new ButtonWidget();
		wbtn_pageDown.SetClickable(true);
		wbtn_pageDown.SetVisible(false);
		AddWidgetNew(wbtn_pageDown);

		// Text Widgets
		FontCi fontHeading = new FontCi();
		fontHeading.size = 20;
		FontCi fontDefault = new FontCi();

		wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontHeading);
		wtxt_title.SetText("Multiplayer");
		AddWidgetNew(wtxt_title);
		wtxt_loadingText = new TextWidget();
		wtxt_loadingText.SetFont(fontDefault);
		wtxt_loadingText.SetText("Loading...");
		wtxt_loadingText.SetVisible(false);
		AddWidgetNew(wtxt_loadingText);
		wtxt_userName = new TextWidget();
		wtxt_userName.SetFont(fontDefault);
		wtxt_userName.SetVisible(false);
		AddWidgetNew(wtxt_userName);
		wtxt_pageNr = new TextWidget();
		wtxt_pageNr.SetFont(fontDefault);
		AddWidgetNew(wtxt_pageNr);

		currentPage = 0;

		serverListAddress = new HttpResponseCi();
		serverListCsv = new HttpResponseCi();
		serversOnList = new ServerOnList[serversOnListCount];
		thumbResponses = new ThumbnailResponseCi[serversOnListCount];

		serverButtons = new ServerButtonWidget[serversPerPage];
		for (int i = 0; i < serversPerPage; i++)
		{
			ServerButtonWidget b = new ServerButtonWidget();
			b.SetVisible(false);
			serverButtons[i] = b;
			AddWidgetNew(b);
		}
		serverListDownloadInProgress = true;
	}

	ButtonWidget wbtn_back;
	ButtonWidget wbtn_connect;
	ButtonWidget wbtn_connectToIp;
	ButtonWidget wbtn_refresh;
	ButtonWidget wbtn_logout;
	ButtonWidget wbtn_pageUp;
	ButtonWidget wbtn_pageDown;
	TextWidget wtxt_title;
	TextWidget wtxt_loadingText;
	TextWidget wtxt_userName;
	TextWidget wtxt_pageNr;
	ServerButtonWidget[] serverButtons;

	HttpResponseCi serverListAddress;
	HttpResponseCi serverListCsv;
	ServerOnList[] serversOnList;
	ThumbnailResponseCi[] thumbResponses;

	const int serversOnListCount = 1024;
	const int serversPerPage = 8;
	string selectedServerHash;
	int currentPage;
	bool serverListDownloadInProgress;
	bool serverListDownloadStarted;

	public override void LoadTranslations()
	{
		wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
		wbtn_connect.SetText(menu.lang.Get("MainMenu_MultiplayerConnect"));
		wbtn_connectToIp.SetText(menu.lang.Get("MainMenu_MultiplayerConnectIP"));
		wbtn_refresh.SetText(menu.lang.Get("MainMenu_MultiplayerRefresh"));
		wtxt_title.SetText(menu.lang.Get("MainMenu_Multiplayer"));
	}
	public override void Render(float dt)
	{
		GamePlatform p = menu.p;
		float scale = menu.GetScale();

		// setup widgets
		wbtn_back.x = 40 * scale;
		wbtn_back.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		wbtn_connect.x = p.GetCanvasWidth() / 2 - 300 * scale;
		wbtn_connect.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_connect.sizex = 256 * scale;
		wbtn_connect.sizey = 64 * scale;

		wbtn_connectToIp.x = p.GetCanvasWidth() / 2 - 0 * scale;
		wbtn_connectToIp.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_connectToIp.sizex = 256 * scale;
		wbtn_connectToIp.sizey = 64 * scale;

		wbtn_refresh.x = p.GetCanvasWidth() / 2 + 350 * scale;
		wbtn_refresh.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_refresh.sizex = 256 * scale;
		wbtn_refresh.sizey = 64 * scale;

		wbtn_pageUp.x = p.GetCanvasWidth() - 94 * scale;
		wbtn_pageUp.y = 100 * scale + (serversPerPage - 1) * 70 * scale;
		wbtn_pageUp.sizex = 64 * scale;
		wbtn_pageUp.sizey = 64 * scale;
		string texName = "serverlist_nav_down.png";
		wbtn_pageUp.SetTextureNames(texName, texName, texName);

		wbtn_pageDown.x = p.GetCanvasWidth() - 94 * scale;
		wbtn_pageDown.y = 100 * scale;
		wbtn_pageDown.sizex = 64 * scale;
		wbtn_pageDown.sizey = 64 * scale;
		texName = "serverlist_nav_up.png";
		wbtn_pageDown.SetTextureNames(texName, texName, texName);

		wtxt_userName.x = p.GetCanvasWidth() - 228 * scale;
		wtxt_userName.y = 32 * scale;
		wtxt_userName.sizex = 128 * scale;
		wtxt_userName.sizey = 32 * scale;
		if (p.GetPreferences().GetString("Password", "") != "")
		{
			wtxt_userName.SetText(p.GetPreferences().GetString("Username", "Invalid"));
			wtxt_userName.SetVisible(true);
			wbtn_logout.SetVisible(true);
		}

		wbtn_logout.x = p.GetCanvasWidth() - 228 * scale;
		wbtn_logout.y = 62 * scale;
		wbtn_logout.sizex = 128 * scale;
		wbtn_logout.sizey = 32 * scale;

		wtxt_title.x = p.GetCanvasWidth() / 2;
		wtxt_title.y = 10 * scale;
		wtxt_title.SetAlignment(TextAlign.Center);

		wtxt_pageNr.SetText(p.IntToString(currentPage + 1));
		wtxt_pageNr.x = p.GetCanvasWidth() - 68 * scale;
		wtxt_pageNr.y = p.GetCanvasHeight() / 2;
		wtxt_pageNr.SetAlignment(TextAlign.Center);
		wtxt_pageNr.SetBaseline(TextBaseline.Middle);

		wtxt_loadingText.SetVisible(serverListDownloadInProgress);
		wtxt_loadingText.SetText(menu.lang.Get("MainMenu_MultiplayerLoading"));
		wtxt_loadingText.x = 100 * scale;
		wtxt_loadingText.y = 50 * scale;

		// update logic
		UpdateServerList();
		UpdateThumbnails();

		for (int i = 0; i < serversPerPage; i++)
		{
			serverButtons[i].SetVisible(false);
			int index = i + (serversPerPage * currentPage);
			if (index > serversOnListCount)
			{
				// Skip entries if list exceeds server limit
				continue;
			}
			ServerOnList s = serversOnList[index];
			if (s == null)
			{
				// Skip entries when reaching the end of the list
				continue;
			}

			serverButtons[i].SetTextHeading(s.name);
			serverButtons[i].SetTextDescription(s.motd);
			serverButtons[i].SetTextGamemode(s.gamemode);
			serverButtons[i].SetTextPlayercount(menu.p.StringFormat2("{0}/{1}", menu.p.IntToString(s.users), menu.p.IntToString(s.max)));
			serverButtons[i].x = 100 * scale;
			serverButtons[i].y = 100 * scale + i * 70 * scale;
			serverButtons[i].sizex = p.GetCanvasWidth() - 200 * scale;
			serverButtons[i].sizey = 64 * scale;
			serverButtons[i].SetVisible(true);
			serverButtons[i].SetErrorVersion(s.version != menu.p.GetGameVersion());

			if (s.thumbnailFetched && !s.thumbnailError)
			{
				serverButtons[i].SetServerImage(menu.p.StringFormat("serverlist_entry_{0}.png", s.hash));
				serverButtons[i].SetErrorConnect(false);
			}
			else
			{
				serverButtons[i].SetServerImage(null);
				serverButtons[i].SetErrorConnect(true);
			}
		}
		UpdateScrollButtons();

		// draw everything
		menu.DrawBackground();
		DrawWidgets();
	}
	public override void OnButtonA(AbstractMenuWidget w)
	{
		if (w == wbtn_back)
		{
			OnBackPressed();
		}
		if (w == wbtn_connect)
		{
			if (selectedServerHash == null) { return; }
			menu.StartLogin(selectedServerHash, null, 0);
		}
		if (w == wbtn_connectToIp)
		{
			menu.StartConnectToIp();
		}
		if (w == wbtn_logout)
		{
			Preferences pref = menu.p.GetPreferences();
			pref.Remove("Username");
			pref.Remove("Password");
			menu.p.SetPreferences(pref);
			wtxt_userName.SetText("");
		}
		if (w == wbtn_pageDown)
		{
			PageDown_();
		}
		if (w == wbtn_pageUp)
		{
			PageUp_();
		}
		if (w == wbtn_refresh)
		{
			serverListDownloadStarted = false;
			serverListDownloadInProgress = true;
		}

		for (int i = 0; i < serversPerPage; i++)
		{
			if (w == serverButtons[i])
			{
				serverButtons[i].SetFocused(true);
				if (serversOnList[i + serversPerPage * currentPage] != null)
				{
					selectedServerHash = serversOnList[i + serversPerPage * currentPage].hash;
				}
			}
			else
			{
				serverButtons[i].SetFocused(false);
			}
		}
	}
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

	public void UpdateServerList()
	{
		if (!serverListDownloadStarted)
		{
			menu.p.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/serverlistcsv.php", serverListAddress);
			serverListDownloadStarted = true;
		}
		if (serverListAddress.done)
		{
			serverListAddress.done = false;
			menu.p.WebClientDownloadDataAsync(serverListAddress.GetString(menu.p), serverListCsv);
		}
		if (serverListCsv.done)
		{
			serverListDownloadInProgress = false;
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
	}

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
		if (currentPage < serversOnListCount / serversPerPage - 1)
		{
			currentPage++;
		}
	}

	public void PageDown_()
	{
		if (currentPage > 0)
		{
			currentPage--;
		}
	}

	public void UpdateScrollButtons()
	{
		//Determine if this page is the highest page containing servers
		bool maxpage = false;
		if ((currentPage + 1) * serversPerPage >= serversOnListCount)
		{
			maxpage = true;
		}
		else
		{
			if (serversOnList[(currentPage + 1) * serversPerPage] == null)
			{
				maxpage = true;
			}
		}
		//Hide scroll buttons
		if (currentPage == 0)
		{
			wbtn_pageDown.SetVisible(false);
		}
		else
		{
			wbtn_pageDown.SetVisible(true);
		}
		if (maxpage)
		{
			wbtn_pageUp.SetVisible(false);
		}
		else
		{
			wbtn_pageUp.SetVisible(true);
		}
	}
}

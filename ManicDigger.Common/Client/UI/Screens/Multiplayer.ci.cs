/// <summary>
/// ScreenMultiplayer displays a server list and allows choosing a target server to connect to.
/// </summary>
public class ScreenMultiplayer : MainMenuScreen
{
	public ScreenMultiplayer()
	{
		// Button Widgets
		wbtn_back = new ButtonWidget();
		AddWidget(wbtn_back);
		wbtn_connect = new ButtonWidget();
		AddWidget(wbtn_connect);
		wbtn_connectToIp = new ButtonWidget();
		AddWidget(wbtn_connectToIp);
		wbtn_refresh = new ButtonWidget();
		AddWidget(wbtn_refresh);
		wbtn_logout = new ButtonWidget();
		wbtn_logout.SetVisible(false);
		AddWidget(wbtn_logout);

		// Text Widgets
		wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontTitle);
		AddWidget(wtxt_title);
		wtxt_loadingText = new TextWidget();
		wtxt_loadingText.SetFont(fontMessage);
		wtxt_loadingText.SetBaseline(TextBaseline.Middle);
		wtxt_loadingText.SetVisible(false);
		AddWidget(wtxt_loadingText);
		wtxt_userName = new TextWidget();
		wtxt_userName.SetFont(fontMessage);
		wtxt_userName.SetAlignment(TextAlign.Right);
		wtxt_userName.SetBaseline(TextBaseline.Middle);
		wtxt_userName.SetVisible(false);
		AddWidget(wtxt_userName);

		// list widget
		wlst_serverList = new ListWidget();
		AddWidget(wlst_serverList);

		currentPage = 0;

		serverListAddress = new HttpResponseCi();
		serverListCsv = new HttpResponseCi();
		serversOnList = new ServerOnList[serversOnListCount];
		thumbResponses = new ThumbnailResponseCi[serversOnListCount];

		serverListDownloadInProgress = true;
	}

	ButtonWidget wbtn_back;
	ButtonWidget wbtn_connect;
	ButtonWidget wbtn_connectToIp;
	ButtonWidget wbtn_refresh;
	ButtonWidget wbtn_logout;
	TextWidget wtxt_title;
	TextWidget wtxt_loadingText;
	TextWidget wtxt_userName;
	ListWidget wlst_serverList;

	HttpResponseCi serverListAddress;
	HttpResponseCi serverListCsv;
	ServerOnList[] serversOnList;
	ThumbnailResponseCi[] thumbResponses;

	const int serversOnListCount = 1024;
	const int serversPerPage = 8;
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
		wtxt_loadingText.SetText(menu.lang.Get("MainMenu_MultiplayerLoading"));
		wbtn_logout.SetText(menu.lang.Get("MainMenu_MultiplayerLogout"));
	}
	public override void Render(float dt)
	{
		float scale = menu.uiRenderer.GetScale();

		// setup widgets
		wtxt_title.x = gamePlatform.GetCanvasWidth() / 2;
		wtxt_title.y = 10 * scale;
		wtxt_title.SetAlignment(TextAlign.Center);

		wbtn_back.x = 40 * scale;
		wbtn_back.y = gamePlatform.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		wbtn_logout.x = gamePlatform.GetCanvasWidth() - 298 * scale;
		wbtn_logout.y = 62 * scale;
		wbtn_logout.sizex = 128 * scale;
		wbtn_logout.sizey = 32 * scale;

		wtxt_userName.x = wbtn_logout.x - 6 * scale;
		wtxt_userName.y = 78 * scale;
		if (gamePlatform.GetPreferences().GetString("Password", "") != "")
		{
			wtxt_userName.SetText(gamePlatform.GetPreferences().GetString("Username", "Invalid"));
			wtxt_userName.SetVisible(true);
			wbtn_logout.SetVisible(true);
		}

		wbtn_refresh.x = 138 * scale;
		wbtn_refresh.y = 62 * scale;
		wbtn_refresh.sizex = 128 * scale;
		wbtn_refresh.sizey = 32 * scale;

		wtxt_loadingText.SetVisible(serverListDownloadInProgress);
		wtxt_loadingText.x = wbtn_refresh.x + 134 * scale;
		wtxt_loadingText.y = 78 * scale;

		wlst_serverList.x = 100 * scale;
		wlst_serverList.y = 100 * scale;
		wlst_serverList.sizex = gamePlatform.GetCanvasWidth() - 200 * scale;
		wlst_serverList.sizey = gamePlatform.GetCanvasHeight() - 200 * scale;

		wbtn_connect.x = wlst_serverList.x + wlst_serverList.sizex - 326 * scale;
		wbtn_connect.y = gamePlatform.GetCanvasHeight() - 104 * scale;
		wbtn_connect.sizex = 256 * scale;
		wbtn_connect.sizey = 64 * scale;

		wbtn_connectToIp.x = wbtn_connect.x - wbtn_connect.sizex - 6 * scale;
		wbtn_connectToIp.y = gamePlatform.GetCanvasHeight() - 104 * scale;
		wbtn_connectToIp.sizex = 256 * scale;
		wbtn_connectToIp.sizey = 64 * scale;

		// update logic
		UpdateServerList();
		UpdateThumbnails();

		for (int i = 0; i < serversPerPage; i++)
		{
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

			wlst_serverList.GetElement(index).imageStatusBottom = (s.version == gamePlatform.GetGameVersion()) ? null : "";
			if (s.thumbnailFetched && !s.thumbnailError)
			{
				wlst_serverList.GetElement(index).imageMain = gamePlatform.StringFormat("serverlist_entry_{0}.png", s.hash);
			}
			else
			{
				wlst_serverList.GetElement(index).imageStatusTop = null;
				wlst_serverList.GetElement(index).imageMain = null;
			}
		}

		// draw everything
		DrawWidgets(dt);
	}
	public override void OnButton(AbstractMenuWidget w)
	{
		if (w == wbtn_back)
		{
			OnBackPressed();
		}
		if (w == wbtn_connect)
		{
			int selectedIndex = wlst_serverList.GetIndexSelected();
			if (selectedIndex < 0) { return; }
			if (serversOnList[selectedIndex] == null) { return; }
			string selectedServerHash = serversOnList[selectedIndex].hash;
			menu.StartLogin(selectedServerHash, null, 0);
		}
		if (w == wbtn_connectToIp)
		{
			menu.StartConnectToIp();
		}
		if (w == wbtn_logout)
		{
			Preferences pref = gamePlatform.GetPreferences();
			pref.Remove("Username");
			pref.Remove("Password");
			gamePlatform.SetPreferences(pref);
			wtxt_userName.SetText(null);
			wtxt_userName.SetVisible(false);
			wbtn_logout.SetVisible(false);
		}
		if (w == wbtn_refresh)
		{
			serverListDownloadStarted = false;
			serverListDownloadInProgress = true;
		}
	}
	public override void OnBackPressed()
	{
		menu.StartMainMenu();
	}

	public void UpdateServerList()
	{
		if (!serverListDownloadStarted)
		{
			gamePlatform.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/serverlistcsv.php", serverListAddress);
			serverListDownloadStarted = true;
		}
		if (serverListAddress.done)
		{
			serverListAddress.done = false;
			gamePlatform.WebClientDownloadDataAsync(serverListAddress.GetString(gamePlatform), serverListCsv);
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
			string[] servers = gamePlatform.StringSplit(serverListCsv.GetString(gamePlatform), "\n", serversCount);
			wlst_serverList.Clear();
			for (int i = 0; i < serversCount.value; i++)
			{
				IntRef ssCount = new IntRef();
				string[] ss = gamePlatform.StringSplit(servers[i], "\t", ssCount);
				if (ssCount.value < 10)
				{
					continue;
				}
				ServerOnList s = new ServerOnList();
				s.hash = ss[0];
				s.name = gamePlatform.DecodeHTMLEntities(ss[1]);
				s.motd = gamePlatform.DecodeHTMLEntities(ss[2]);
				s.port = gamePlatform.IntParse(ss[3]);
				s.ip = ss[4];
				s.version = ss[5];
				s.users = gamePlatform.IntParse(ss[6]);
				s.max = gamePlatform.IntParse(ss[7]);
				s.gamemode = ss[8];
				s.players = ss[9];
				serversOnList[i] = s;

				ListEntry e = new ListEntry();
				e.textTopLeft = serversOnList[i].name;
				e.textBottomLeft = serversOnList[i].motd;
				e.textTopRight = gamePlatform.StringFormat2("{0}/{1}", gamePlatform.IntToString(serversOnList[i].users), gamePlatform.IntToString(serversOnList[i].max));
				e.textBottomRight = serversOnList[i].gamemode;
				wlst_serverList.AddElement(e);
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
				gamePlatform.ThumbnailDownloadAsync(server.ip, server.port, thumbResponses[i]);
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
						BitmapCi bmp = gamePlatform.BitmapCreateFromPng(thumbResponses[i].data, thumbResponses[i].dataLength);
						if (bmp != null)
						{
							menu.uiRenderer.LoadBitmap(bmp, gamePlatform.StringFormat("serverlist_entry_{0}.png", server.hash));
							gamePlatform.BitmapDelete(bmp);
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
}

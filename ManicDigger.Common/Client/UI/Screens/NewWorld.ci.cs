/// <summary>
/// ScreenSingleplayer shows a minimalistic "Open File" dialog for loading local savegames.
/// TODO: Replace/Enhance this with ScreenModifyWorld to hide unnecessary complexity from the user.
/// </summary>
public class NewWorld : MainMenuScreen
{
	public NewWorld()
	{
        // initialize widgets
        wtbx_name = new LabeledTextBoxWidget();
        AddWidget(wtbx_name);
        //public ? Server_SetupPublic
        // Server_SetupPort
        //  Server_SetupName
        // Server_SetupMOTD
        //Server_SetupWelcomeMessage
        //Server_SetupEnableHTTP
        //Server_SetupMaxClients
        wbtn_serveroptions = new ButtonWidget();
        AddWidget(wbtn_serveroptions);

        wbtn_create = new ButtonWidget();
		AddWidget(wbtn_create);

        wbtn_back = new ButtonWidget();
        AddWidget(wbtn_back);

        wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontTitle);
		AddWidget(wtxt_title);

        wlst_SettingList = new SettingsListWidget();
         setting=new SettingListEntry[7];
        int i = 0;
        setting[i] = new SettingListEntry();
        setting[i]._label = "Public Server";
        setting[i]._setting = "Server_SetupPublic";
        setting[i]._value = "false";
        setting[i]._type = SettingEntryType.String;
        i++;

        setting[i] = new SettingListEntry();
        setting[i]._label = "Server Name";
        setting[i]._setting = "Server_SetupName";
        setting[i]._value = "SNAME";
        setting[i]._type = SettingEntryType.String;
        i++;

        setting[i] = new SettingListEntry();
        setting[i]._label = "Public Server";
        setting[i]._setting = "Server_SetupPublic";
        setting[i]._value = "false";
        setting[i]._type = SettingEntryType.String;
        i++;

        setting[i] = new SettingListEntry();
        setting[i]._label = "MODT";
        setting[i]._value = "Server_SetupMOTD";
        setting[i]._setting = "";
        setting[i]._type = SettingEntryType.String;
        i++;

        setting[i] = new SettingListEntry();
        setting[i]._label = "Server_SetupWelcomeMessage";
        setting[i]._setting = "Server_SetupWelcomeMessage";
        setting[i]._value = "Welcome ";
        setting[i]._type = SettingEntryType.String;
        i++;

        setting[i] = new SettingListEntry();
        setting[i]._label = "Server_SetupEnableHTTP ";
        setting[i]._setting = "Server_SetupEnableHTTP";
        setting[i]._value = "false";
        setting[i]._type = SettingEntryType.Bool;
        i++;

        setting[i] = new SettingListEntry();
        setting[i]._label = "Server_SetupMaxClients ";
        setting[i]._setting = "Server_SetupMaxClients";
        setting[i]._value = "16";
        setting[i]._type = SettingEntryType.Int;

        for (int j=0;j<7;j++)
       wlst_SettingList.AddElement(setting[j]);


        AddWidget(wlst_SettingList);
        serverOptionsActive = true;

            wbtn_serveroptions.SetText("Server Options");
            wtbx_name.visible = false;
            wlst_SettingList.visible = true;


    }
    SettingListEntry [] setting;
    LabeledTextBoxWidget wtbx_name;
    ButtonWidget wbtn_back; 
    SettingsListWidget wlst_SettingList;

    ButtonWidget wbtn_serveroptions;
    ButtonWidget wbtn_create;
    TextWidget wtxt_title;
    bool serverOptionsActive;

    bool loaded;

    public override void LoadTranslations()
	{
		wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
        wbtn_create.SetText("Create");
        wtxt_title.SetText(menu.lang.Get("MainMenu_Singleplayer"));
        wbtn_serveroptions.SetText("Server Options");

    }

	public override void Render(float dt)
	{
        // load stored values or defaults
        if (!loaded)
        {
            wtbx_name.SetLabel("World name");
            wtbx_name.SetContent(gamePlatform,"New World");
            loaded = true;
        }
        float scale = menu.uiRenderer.GetScale();
        float leftx = gamePlatform.GetCanvasWidth() / 2 - 128 * scale;
        float y = gamePlatform.GetCanvasHeight() / 2 + 0 * scale;





        wlst_SettingList.x = 100 * scale;
        wlst_SettingList.y = 100 * scale;
        wlst_SettingList.sizex = gamePlatform.GetCanvasWidth() - 200 * scale;
        wlst_SettingList.sizey = gamePlatform.GetCanvasHeight() - 200 * scale;


        wtbx_name.x = leftx-128*scale;
        wtbx_name.y = y + 100 * scale;
        wtbx_name.sizex = 512 * scale;
        wtbx_name.sizey = 64 * scale;


        wbtn_serveroptions.x = 40 * scale + 256 * scale;
        wbtn_serveroptions.y = gamePlatform.GetCanvasHeight() - 104 * scale;
        wbtn_serveroptions.sizex = 256 * scale;
        wbtn_serveroptions.sizey = 64 * scale;

        wbtn_create.x = 40 * scale + 512 * scale;
        wbtn_create.y = gamePlatform.GetCanvasHeight() - 104 * scale;
        wbtn_create.sizex = 256 * scale;
        wbtn_create.sizey = 64 * scale;

        wbtn_back.x = 40 * scale;
        wbtn_back.y = gamePlatform.GetCanvasHeight() - 104 * scale;
        wbtn_back.sizex = 256 * scale;
        wbtn_back.sizey = 64 * scale;

    

		// TODO: Implement savegame handling in game menu

		DrawWidgets(dt);


	}

	public override void OnBackPressed()
	{
		menu.StartMainMenu();
	}
  
    public override void OnButton(AbstractMenuWidget w)
	{
		

		if (w == wbtn_back)
		{
			OnBackPressed();
		}
        if (w == wbtn_serveroptions)
        {
            serverOptionsActive = !serverOptionsActive;
            if (serverOptionsActive)
            {
                wbtn_serveroptions.SetText("main Options");
                wtbx_name.visible = true;
                wlst_SettingList.visible = false;

            }
            else
            {
                wbtn_serveroptions.SetText("Server Options");
                wtbx_name.visible = false;
                wlst_SettingList.visible = true;

            }
        }
        if (w == wbtn_create)
        {
            string wordname = menu.p.StringFormat2("{0}/{1}.mddbs", menu.p.PathSavegames(), wtbx_name.GetContent());

            //   string temp = string.Format("{0} ({1})",
            //    name,i);
            ServerInitSettings serverInitSettings=new ServerInitSettings();
            serverInitSettings.filename = wordname;
            serverInitSettings.settingsOverride = wlst_SettingList.GetAllElements();
            menu.StartGame(true, serverInitSettings, null);

        }

    }


}

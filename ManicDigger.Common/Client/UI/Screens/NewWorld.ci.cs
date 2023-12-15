
public enum NewWorldPages { 
Default,
ServerSettings,
Mods
}
public class NewWorld : MainMenuScreen
{

    public NewWorld()
    {
        // initialize widgets
        wtbx_name = new LabeledTextBoxWidget();
        AddWidget(wtbx_name);
    
        wbtn_serveroptions = new ButtonWidget();
        AddWidget(wbtn_serveroptions);

        wbtn_serverModOptions = new ButtonWidget();
        AddWidget(wbtn_serverModOptions);

        wbtn_create = new ButtonWidget();
        AddWidget(wbtn_create);

        wbtn_back = new ButtonWidget();
        AddWidget(wbtn_back);

        wtxt_title = new TextWidget();
        wtxt_title.SetFont(fontTitle);
        AddWidget(wtxt_title);

        wlst_modList = new ListWidget();
        AddWidget(wlst_modList);

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
        newWorldPage = NewWorldPages.Default;
 
        wtbx_name.visible = true;

        wlst_SettingList.visible = false;
        wlst_modList.visible = false;
    }



    SettingListEntry [] setting;
    LabeledTextBoxWidget wtbx_name;
    ButtonWidget wbtn_back; 

    SettingsListWidget wlst_SettingList;
    ListWidget wlst_modList;

    ButtonWidget wbtn_serveroptions;
    ButtonWidget wbtn_serverModOptions;
    ButtonWidget wbtn_create;
    TextWidget wtxt_title;
    NewWorldPages newWorldPage;
    bool loaded;



   


    public override void LoadTranslations()
    {
        wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
        wbtn_create.SetText("Create"); //TODO LANG
        wtxt_title.SetText(menu.lang.Get("MainMenu_Singleplayer"));
        wbtn_serveroptions.SetText("Server Options"); //TODO LANG
        wbtn_serverModOptions.SetText("Mod Options"); //TODO LANG
    }

    public override void Render(float dt)
    {
        // load stored values or defaults
        if (!loaded)
        {
            wtbx_name.SetLabel("World name"); //TODO LANG
            wtbx_name.SetContent(gamePlatform,"New World");
            loaded = true;

            IntRef lenght = new IntRef();
            Modinfo[] modinfos = menu.GetModinfo(lenght);
            for (int m = 0; m < lenght.GetValue(); m++)
            {
                ListEntry entry = new ListEntry();
                entry.textTopLeft = modinfos[m].ModName;
                entry.textTopRight = "Active";
                entry.textBottomLeft = modinfos[m].Description;
                entry.textBottomRight = modinfos[m].Category;

                wlst_modList.AddElement(entry);
                
            }
            //wbtn_serveroptions.SetText(menu.p.StringFormat("Server Options{0}",menu.p.IntToString(lenght.GetValue())));


        }
        float scale = menu.uiRenderer.GetScale();
        float leftx = gamePlatform.GetCanvasWidth() / 2 - 128 * scale;
        float y = gamePlatform.GetCanvasHeight() / 2 + 0 * scale;


  
        wlst_SettingList.x = 100 * scale;
        wlst_SettingList.y = 100 * scale;
        wlst_SettingList.sizex = gamePlatform.GetCanvasWidth() - 200 * scale;
        wlst_SettingList.sizey = gamePlatform.GetCanvasHeight() - 200 * scale;


        wlst_modList.x = 100 * scale;
        wlst_modList.y = 100 * scale;
        wlst_modList.sizex = gamePlatform.GetCanvasWidth() - 200 * scale;
        wlst_modList.sizey = gamePlatform.GetCanvasHeight() - 200 * scale;


        wtbx_name.x = leftx-128*scale;
        wtbx_name.y = y + 100 * scale;
        wtbx_name.sizex = 512 * scale;
        wtbx_name.sizey = 64 * scale;


        wbtn_serveroptions.x = 40 * scale + 256 * scale;
        wbtn_serveroptions.y = gamePlatform.GetCanvasHeight() - 104 * scale;
        wbtn_serveroptions.sizex = 256 * scale;
        wbtn_serveroptions.sizey = 64 * scale;

        wbtn_serverModOptions.x = 40 * scale + 512 * scale;
        wbtn_serverModOptions.y = gamePlatform.GetCanvasHeight() - 104 * scale;
        wbtn_serverModOptions.sizex = 256 * scale;
        wbtn_serverModOptions.sizey = 64 * scale;

        wbtn_create.x = 40 * scale + (512+256) * scale;
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

           
        }
        if (w == wbtn_serverModOptions || w == wbtn_serveroptions)
        {
            //this shud be a widget
            switch (newWorldPage)
            {
                case NewWorldPages.Default:
                    newWorldPage = (w == wbtn_serverModOptions) ? NewWorldPages.Mods : NewWorldPages.ServerSettings;
                    break;
                case NewWorldPages.ServerSettings:
                    newWorldPage = (w == wbtn_serverModOptions) ? NewWorldPages.Mods : NewWorldPages.Default;
                    break;
                case NewWorldPages.Mods:
                    newWorldPage = (w == wbtn_serverModOptions) ? NewWorldPages.Default : NewWorldPages.ServerSettings;
                    break;
            }

            wbtn_serverModOptions.SetText("Mod Options"); //TODO LANG
            wbtn_serveroptions.SetText("Server Options"); //TODO LANG

            wtbx_name.visible = false;

            wlst_SettingList.visible = false;
            wlst_modList.visible = false;

            switch (newWorldPage)
            {
                case NewWorldPages.Mods:
                    wbtn_serverModOptions.SetText("World Options");
                    wlst_modList.visible = true;
                    wtxt_title.SetText("Mod settings");//TODO LANG

                    break;
                case NewWorldPages.ServerSettings:
                    wbtn_serveroptions.SetText("World Options"); //TODO LANG
                    wtxt_title.SetText("Server settings"); //TODO LANG

                    wlst_SettingList.visible = true; //TODO LANG
                    break;
                case NewWorldPages.Default:
                    wtxt_title.SetText("World settings"); //TODO LANG

                    wtbx_name.visible = true;
                    break;
            }
        }
        if (w == wbtn_create)
        {
            string name = wtbx_name.GetContent();
            IntRef savegamesCount_=new IntRef();
            string[] savegames = menu.GetSavegames(savegamesCount_);
            //TODO ITS STUPID CODE THERE MUST BE A BETTER WAY
            bool contains;


            for (int j = 2; true; j++)
            {   contains = false;
                for (int k = 0; k < savegamesCount_.value; k++)
                {
                    if (savegames[j] == menu.p.StringFormat2("{0} ({1})",name,menu.p.IntToString(j)))
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains) {
                    name = menu.p.StringFormat2("{0} ({1})", name, menu.p.IntToString(j));
                    break;
                }
            }
        
            
            string wordname = menu.p.StringFormat2("{0}/{1}.mddbs", menu.p.PathSavegames(), name);

            ServerInitSettings serverInitSettings =new ServerInitSettings();
            serverInitSettings.filename = wordname;
            serverInitSettings.settingsOverride = wlst_SettingList.GetAllElements();
            menu.StartGame(true, serverInitSettings, null);

        }

    }


}

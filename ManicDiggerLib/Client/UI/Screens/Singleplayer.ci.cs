public class ScreenSingleplayer : Screen
{
	public ScreenSingleplayer()
	{
		// initialize widgets
		wbtn_newWorld = new ButtonWidget();
		wbtn_newWorld.SetText("New World");
		wbtn_newWorld.SetVisible(false);
		AddWidgetNew(wbtn_newWorld);
		wbtn_playWorld = new ButtonWidget();
		wbtn_playWorld.SetText("Play");
		wbtn_playWorld.SetVisible(false);
		AddWidgetNew(wbtn_playWorld);
		wbtn_modifyWorld = new ButtonWidget();
		wbtn_modifyWorld.SetText("Modify");
		wbtn_modifyWorld.SetVisible(false);
		AddWidgetNew(wbtn_modifyWorld);
		wbtn_back = new ButtonWidget();
		AddWidgetNew(wbtn_back);
		wbtn_openFile = new ButtonWidget();
		AddWidgetNew(wbtn_openFile);
		wtxt_title = new TextWidget();
		wtxt_title.SetFont(fontTitle);
		AddWidgetNew(wtxt_title);
		wtxt_singleplayerUnavailable = new TextWidget();
		wtxt_singleplayerUnavailable.SetFont(fontDefault);
		wtxt_singleplayerUnavailable.SetText("Singleplayer is only available on desktop (Windows, Linux, Mac) version of game.");
		wtxt_singleplayerUnavailable.SetVisible(false);
		AddWidgetNew(wtxt_singleplayerUnavailable);
		wlst_worldList = new ListWidget();
		AddWidgetNew(wlst_worldList);
	}

	ButtonWidget wbtn_newWorld;
	ButtonWidget wbtn_playWorld;
	ButtonWidget wbtn_modifyWorld;
	ButtonWidget wbtn_back;
	ButtonWidget wbtn_openFile;
	TextWidget wtxt_title;
	TextWidget wtxt_singleplayerUnavailable;
	ListWidget wlst_worldList;

	string[] savegames;
	int savegamesCount;

	public override void LoadTranslations()
	{
		wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
		wbtn_openFile.SetText(menu.lang.Get("MainMenu_SingleplayerButtonCreate"));
		wtxt_title.SetText(menu.lang.Get("MainMenu_Singleplayer"));
	}

	public override void Render(float dt)
	{
		GamePlatform p = menu.p;
		float scale = menu.uiRenderer.GetScale();
		float leftx = p.GetCanvasWidth() / 2 - 128 * scale;
		float y = p.GetCanvasHeight() / 2 + 0 * scale;

		wbtn_playWorld.x = leftx;
		wbtn_playWorld.y = y + 100 * scale;
		wbtn_playWorld.sizex = 256 * scale;
		wbtn_playWorld.sizey = 64 * scale;

		wbtn_newWorld.x = leftx;
		wbtn_newWorld.y = y + 170 * scale;
		wbtn_newWorld.sizex = 256 * scale;
		wbtn_newWorld.sizey = 64 * scale;

		wbtn_modifyWorld.x = leftx;
		wbtn_modifyWorld.y = y + 240 * scale;
		wbtn_modifyWorld.sizex = 256 * scale;
		wbtn_modifyWorld.sizey = 64 * scale;

		wbtn_back.x = 40 * scale;
		wbtn_back.y = p.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		wbtn_openFile.x = leftx;
		wbtn_openFile.y = y + 0 * scale;
		wbtn_openFile.sizex = 256 * scale;
		wbtn_openFile.sizey = 64 * scale;

		wtxt_title.x = p.GetCanvasWidth() / 2;
		wtxt_title.y = 10;
		wtxt_title.SetAlignment(TextAlign.Center);

		wlst_worldList.x = leftx;
		wlst_worldList.y = 100;
		wlst_worldList.sizex = 256 * scale;
		wlst_worldList.sizey = 400 * scale;

		// TODO: Implement savegame handling in game menu
		//LoadSavegameList(p);

		DrawWidgets(dt);

		if (!menu.p.SinglePlayerServerAvailable())
		{
			wbtn_openFile.SetVisible(false);
			wtxt_singleplayerUnavailable.SetVisible(true);
			wtxt_singleplayerUnavailable.x = p.GetCanvasWidth() / 2;
			wtxt_singleplayerUnavailable.y = p.GetCanvasHeight() / 2;
			wtxt_singleplayerUnavailable.SetAlignment(TextAlign.Center);
			wtxt_singleplayerUnavailable.SetBaseline(TextBaseline.Middle);
		}
	}

	public override void OnBackPressed()
	{
		menu.StartMainMenu();
	}

	public override void OnButton(AbstractMenuWidget w)
	{
		if (w == wbtn_newWorld)
		{
			menu.StartNewWorld();
		}

		if (w == wbtn_playWorld)
		{

		}

		if (w == wbtn_modifyWorld)
		{
			menu.StartModifyWorld();
		}

		if (w == wbtn_back)
		{
			OnBackPressed();
		}

		if (w == wbtn_openFile)
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

	void LoadSavegameList(GamePlatform p)
	{
		if (savegames == null)
		{
			IntRef savegamesCount_ = new IntRef();
			savegames = menu.GetSavegames(savegamesCount_);
			savegamesCount = savegamesCount_.value;

			for (int i = 0; i < savegamesCount; i++)
			{
				ListEntry e = new ListEntry();
				e.textTopLeft = menu.p.FileName(savegames[i]);
				wlst_worldList.AddElement(e);
			}
		}
	}
}

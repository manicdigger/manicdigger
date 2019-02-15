public class ScreenModifyWorld : MainMenuScreen
{
	public ScreenModifyWorld()
	{
		wbtn_back = new ButtonWidget();
		AddWidget(wbtn_back);
		wtxt_title = new TextWidget();
		wtxt_title.SetAlignment(TextAlign.Center);
		AddWidget(wtxt_title);
	}

	ButtonWidget wbtn_back;
	TextWidget wtxt_title;

	public override void LoadTranslations()
	{
		wbtn_back.SetText(menu.lang.Get("MainMenu_ButtonBack"));
		wtxt_title.SetText("Modify World");
	}

	public override void Render(float dt)
	{
		float scale = menu.uiRenderer.GetScale();

		wbtn_back.x = 40 * scale;
		wbtn_back.y = gamePlatform.GetCanvasHeight() - 104 * scale;
		wbtn_back.sizex = 256 * scale;
		wbtn_back.sizey = 64 * scale;

		wtxt_title.x = gamePlatform.GetCanvasWidth() / 2;
		wtxt_title.y = 10 * scale;

		DrawWidgets(dt);
	}

	public override void OnBackPressed()
	{
		menu.StartSingleplayer();
	}

	public override void OnButton(AbstractMenuWidget w)
	{
		if (w == wbtn_back)
		{
			OnBackPressed();
		}
	}
}

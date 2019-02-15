/// <summary>
/// MainMenuScreen is a specialized widget manager for use with the main game menu.
/// </summary>
public class MainMenuScreen : Screen
{
	public MainMenuScreen()
	{
		fontTitle = new FontCi();
		fontTitle.size = 20;
		fontTitle.style = 1;
		fontMessage = new FontCi();
		fontMessage.style = 3;
	}

	internal MainMenu menu;
	internal FontCi fontTitle;
	internal FontCi fontMessage;

	/// <summary>
	/// Initialize MainMenuScreen with required parameters. Must be manually called after creating a new instance.
	/// </summary>
	/// <param name="m">MainMenu parent object</param>
	/// <param name="r">UiRenderer for text rendering</param>
	public virtual void Init(MainMenu m, UiRenderer r)
	{
		menu = m;
		gamePlatform = m.p;
		uiRenderer = r;
		LoadTranslations();
	}
	/// <summary>
	/// virtual method: load translations from disk
	/// </summary>
	public virtual void LoadTranslations() { }
}

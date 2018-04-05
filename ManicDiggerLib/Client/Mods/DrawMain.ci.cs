public class ModDrawMain : ClientMod
{
	public override void OnReadOnlyMainThread(Game game, float dt)
	{
		game.MainThreadOnRenderFrame(dt);
	}
}

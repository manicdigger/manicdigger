public class ModUpdateMain : ClientMod
{
	// Should use ReadWrite to be correct but that would be too slow
	public override void OnReadOnlyMainThread(Game game, float dt)
	{
		game.Update(dt);
	}
}

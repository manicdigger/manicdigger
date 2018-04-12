public class GameExit
{
	internal bool exit;
	internal bool restart;

	public void SetExit(bool p)
	{
		exit = p;
	}

	public bool GetExit()
	{
		return exit;
	}

	public void SetRestart(bool p)
	{
		restart = p;
	}

	public bool GetRestart()
	{
		return restart;
	}
}

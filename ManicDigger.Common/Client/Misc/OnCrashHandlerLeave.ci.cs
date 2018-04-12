public class OnCrashHandlerLeave : OnCrashHandler
{
	public static OnCrashHandlerLeave Create(Game game)
	{
		OnCrashHandlerLeave oncrash = new OnCrashHandlerLeave();
		oncrash.g = game;
		return oncrash;
	}
	Game g;
	public override void OnCrash()
	{
		g.SendLeave(Packet_LeaveReasonEnum.Crash);
	}
}

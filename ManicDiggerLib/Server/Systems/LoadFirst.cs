using System;

namespace ManicDigger
{
	public class ServerSystemLoadFirst : ServerSystem
	{
		bool loaded;
		public override void Update(Server server, float dt)
		{
			if (!loaded)
			{
				loaded = true;
				LoadFirstEvent(server);
			}
		}

		void LoadFirstEvent(Server server)
		{
			// Add things that need to be done prior to all other systems here.
		}
	}
}

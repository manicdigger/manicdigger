using System;

namespace ManicDigger
{
	public class ServerSystemLoadLast : ServerSystem
	{
		bool loaded;
		public override void Update(Server server, float dt)
		{
			if (!loaded)
			{
				loaded = true;
				LoadLastEvent(server);
			}
		}

		void LoadLastEvent(Server server)
		{
			// Add things that need to be done after all other systems here
		}
	}
}

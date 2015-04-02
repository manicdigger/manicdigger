using System;
using System.Collections.Generic;

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

			// Mod OnLoad needs to be called after all other systems as some functions rely on stuff loaded after ModLoader
			Call_ModOnLoad(server);
		}

		void Call_ModOnLoad(Server server)
		{
			// Create dictionary to hold Mod data if none has been loaded in savegame
			if (server.moddata == null) { server.moddata = new Dictionary<string, byte[]>(); }
			// Execute all methods registered using RegisterOnLoad(). This will fail if they contain errors.
			for (int i = 0; i < server.onload.Count; i++)
			{
				server.onload[i]();
			}
			// Try to execute all methods registered using RegisterOnLoadWorld().
			for (int i = 0; i < server.modEventHandlers.onloadworld.Count; i++)
			{
				try
				{
					server.modEventHandlers.onloadworld[i]();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Mod exception: OnLoadWorld");
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}
	}
}

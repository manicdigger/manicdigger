using System;
using System.Collections.Generic;

namespace ManicDigger.Mods
{
	public class Revert : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("BuildLog");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterPrivilege("revert");
			m.RegisterCommandHelp("revert", "/revert [playername] [number of changes]");
			lines = (List<object[]>)m.GetGlobalDataNotSaved("LogLines");
			m.RegisterOnCommand(OnCommand);
		}
		ModManager m;
		public int MaxRevert = 2000;
		List<object[]> lines = new List<object[]>();
		bool OnCommand(int player, string command, string argument)
		{
			if (command.Equals("revert", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "revert"))
				{
					m.SendMessage(player, m.colorError() + "Insufficient privileges to use revert.");
					return true;
				}
				string targetplayername;
				int n;
				try
				{
					string[] args = argument.Split(' ');
					targetplayername = args[0];
					n = int.Parse(args[1]);
				}
				catch
				{
					m.SendMessage(player, m.colorError() + "Invalid arguments. Type /help to see command's usage.");
					return false;
				}
				if (n > MaxRevert)
				{
					m.SendMessage(player, m.colorError() + string.Format("Can't revert more than {0} block changes", MaxRevert));
				}
				
				int reverted = 0;
				for (int i = lines.Count - 1; i >= 0; i--)
				{
					object[] l = lines[i];
					string lplayername = (string)l[6];
					int lx = ((short)l[1]);
					int ly = ((short)l[2]);
					int lz = ((short)l[3]);
					bool lbuild = (bool)l[5];
					short lblocktype = ((short)l[4]);
					if (lplayername.Equals(targetplayername, StringComparison.InvariantCultureIgnoreCase))
					{
						if (lbuild)
						{
							m.SetBlock(lx, ly, lz, 0);
						}
						else
						{
							m.SetBlock(lx, ly, lz, lblocktype);
						}
						reverted++;
						if (reverted >= n)
						{
							break;
						}
					}
				}
				if (reverted == 0)
				{
					m.SendMessage(player, string.Format(m.colorError() + "Not reverted any block changes by player {0}.", targetplayername));
				}
				else
				{
					m.SendMessageToAll(string.Format("{0} reverted {1} block changes by player {2}", m.GetPlayerName(player), reverted, targetplayername));
					m.LogServerEvent(string.Format("{0} reverts {1} block changes by player {2}", m.GetPlayerName(player), reverted, targetplayername));
				}
				return true;
			}
			return false;
		}
	}
}

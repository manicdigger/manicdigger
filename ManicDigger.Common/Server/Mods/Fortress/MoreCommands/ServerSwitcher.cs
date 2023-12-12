/*
 * ServerSwitcher Mod - Version 1.0
 * Last change: 2014-08-25
 * Author: croxxx
 * 
 * This mod adds a command to switch from one server to another.
 * This could be used for server networks.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace ManicDigger.Mods
{
	public class ServerSwitcher : IMod
	{
		struct Target
		{
			public string IPAdress;
			public int Port;
		}

		public void PreStart(ModManager m) { }
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterPrivilege("switcher_manage");
			m.RegisterOnCommand(Command_Manage);
			m.RegisterOnCommand(Command_Redirect);
			LoadTargets();
		}
		ModManager m;
		Dictionary<string, Target> targetServers = new Dictionary<string, Target>();

		bool CheckIPValid(string strIP)
		{
			IPAddress result = null;
			return (!String.IsNullOrEmpty(strIP) && IPAddress.TryParse(strIP, out result));
		}

		string targetFile = "UserData" + Path.DirectorySeparatorChar + "ServerSwitcher.txt";
		void SaveTargets()
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(targetFile, false, Encoding.UTF8))
				{
					foreach (var target in targetServers)
					{
						sw.WriteLine(string.Format("{0};{1};{2}", target.Key, target.Value.IPAdress, target.Value.Port));
					}
				}
				System.Console.WriteLine("[ServerSwitcher] Targets saved.");
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[ServerSwitcher] ERROR:  " + ex.Message);
			}
		}
		void LoadTargets()
		{
			if (!File.Exists(targetFile))
			{
				//No target file found. skip.
				return;
			}
			try
			{
				using (TextReader tr = new StreamReader(targetFile, Encoding.UTF8))
				{
					string line = tr.ReadLine();
					while (!string.IsNullOrEmpty(line))
					{
						string[] split = line.Split(';');
						Target t = new Target();
						t.IPAdress = split[1];
						t.Port = int.Parse(split[2]);
						targetServers.Add(split[0], t);
						line = tr.ReadLine();
					}
					System.Console.WriteLine("[ServerSwitcher] Loaded {0} targets", targetServers.Count);
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[ServerSwitcher] ERROR:  " + ex.Message);
			}
		}

		bool Command_Manage(int player, string command, string argument)
		{
			if (command.Equals("switcher", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "switcher_manage"))
				{
					m.SendMessage(player, m.colorError() + "You are not allowed to manage target servers.");
					Console.WriteLine("[ServerSwitcher] {0} tried to manage target servers (no permission)");
					return true;
				}
				string[] args;
				string option;
				try
				{
					args = argument.Split(' ');
					option = args[0];
				}
				catch
				{
					m.SendMessage(player, m.colorError() + "Invalid arguments.");
					return true;
				}

				switch (option)
				{
					//switcher add [ip] [port] [target_id]
					case "add":
						//Check number of given arguments
						if (args.Length < 4)
						{
							m.SendMessage(player, m.colorError() + "Invalid arguments for command ADD.");
							break;
						}
						//Check if IP is valid
						if (!CheckIPValid(args[1]))
						{
							m.SendMessage(player, m.colorError() + "Invalid IP");
							break;
						}
						//Check if port is valid
						int port;
						try
						{
							port = int.Parse(args[2]);
							if (port < 0 || port > 65535)
							{
								m.SendMessage(player, m.colorError() + "Invalid Port");
								break;
							}
						}
						catch
						{
							m.SendMessage(player, m.colorError() + "Invalid Port");
							break;
						}
						//Check if target_id already exists
						if (targetServers.ContainsKey(args[3]))
						{
							m.SendMessage(player, m.colorError() + "Target ID already exists");
							break;
						}
						//If all checks are passed, add the new target server to the list
						Target t = new Target();
						t.IPAdress = args[1];
						t.Port = port;
						targetServers.Add(args[3], t);
						m.SendMessage(player, "&2Successfully added new target");
						//Save changes
						SaveTargets();
						break;

					//switcher remove [target_id]
					case "remove":
						//Check number of given arguments
						if (args.Length < 2)
						{
							m.SendMessage(player, m.colorError() + "Invalid arguments for command REMOVE.");
							break;
						}
						//Check if entry with given target id exists
						if (targetServers.ContainsKey(args[1]))
						{
							targetServers.Remove(args[1]);
							m.SendMessage(player, "&2Successfully removed target");
							//Save changes
							SaveTargets();
						}
						else
						{
							m.SendMessage(player, m.colorError() + "Target ID does not exist");
							break;
						}
						break;

					//switcher list
					case "list":
						//Send a list of all targets to the player
						m.SendMessage(player, "List of all targets:");
						foreach (var target in targetServers)
						{
							m.SendMessage(player, "- " + target.Key);
						}
						break;

					default:
						m.SendMessage(player, m.colorError() + "Invalid arguments.");
						break;
				}
				return true;
			}
			return false;
		}

		bool Command_Redirect(int player, string command, string argument)
		{
			if (command.Equals("go", StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (var target in targetServers)
				{
					if (target.Key.Equals(argument, StringComparison.InvariantCultureIgnoreCase))
					{
						Console.WriteLine("[ServerSwitcher] Redirecting {0} to {1}:{2}...", m.GetPlayerName(player), target.Value.IPAdress, target.Value.Port);
						m.SendPlayerRedirect(player, target.Value.IPAdress, target.Value.Port);
						return true;
					}
				}
				m.SendMessage(player, m.colorError() + "Target ID does not exist");
				return true;
			}
			return false;
		}
	}
}

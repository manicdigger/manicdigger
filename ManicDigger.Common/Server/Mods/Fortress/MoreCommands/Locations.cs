/*
 * Locations Mod - Version 1.2
 * Last change: 2013-11-26
 * Author: croxxx
 * 
 * This mod adds locations to your server.
 * Every time a player enters one of the locations a message is displayed
 * Example: You just arrived at <location name>
 * 
 * Following command is added:
 * /location
 * Command options are:
 *    add [startx] [starty] [startz] [endx] [endy] [endz] [name]
 *    delete [name]
 *    list
 *    clear
 *    help
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
	public class Locations : IMod
	{
		//Enter the desired language code here. Currently supported are EN and DE.
		string languageCode = "EN";

		public void PreStart(ModManager m) { }

		public void Start(ModManager m)
		{
			this.m = m;
			m.RegisterPrivilege("manage_locations");
			m.RegisterCommandHelp("manage_locations", "Allows you to add/delete locations");
			m.RegisterTimer(CheckPlayerPosition, (double)1);
			m.RegisterOnCommand(ManageAreas);
			m.RegisterOnLoad(LoadLocations);
			System.Console.WriteLine("[Locations] Loaded Mod Version 1.2");
		}
		ModManager m;
		string chatPrefix = "&8[&6Locations&8] ";
		List<Location> locationList = new List<Location>();
		Dictionary<int, string> PlayerLastSeenIn = new Dictionary<int, string>();

		struct Vector3i
		{
			public Vector3i(int x, int y, int z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
			public int x;
			public int y;
			public int z;
		}
		struct Location
		{
			public Location(Vector3i startpos, Vector3i endpos)
			{
				if (startpos.x < endpos.x)
				{
					start.x = startpos.x;
					end.x = endpos.x;
				}
				else
				{
					start.x = endpos.x;
					end.x = startpos.x;
				}
				if (startpos.y < endpos.y)
				{
					start.y = startpos.y;
					end.y = endpos.y;
				}
				else
				{
					start.y = endpos.y;
					end.y = startpos.y;
				}
				if (startpos.z < endpos.z)
				{
					start.z = startpos.z;
					end.z = endpos.z;
				}
				else
				{
					start.z = endpos.z;
					end.z = startpos.z;
				}
				size.x = (end.x - start.x) + 1;
				size.y = (end.y - start.y) + 1;
				size.z = (end.z - start.z) + 1;
				name = "&4unspecified";
			}
			public Vector3i start;
			public Vector3i end;
			public readonly Vector3i size;
			public string name;
		}

		string GetLocalizedString(string value)
		{
			switch (languageCode)
			{
				#region German translation
				case "DE":
					switch (value)
					{
						case "location_change":
							return "&7Du hast gerade &3{0} &7erreicht.";
						case "invalid_args":
							return "Ungültige Argumente. Schau mal in &7/location help.";
						case "privilege_missing":
							return "Du darfst die Bereiche nicht verwalten.";
						case "add_success":
							return "&aBereich &7{0} &aerfolgreich erstellt.";
						case "add_start":
							return "&7Start-Koordinaten: {0}, {1}, {2}";
						case "add_end":
							return "&7End-Koordinaten: {0}, {1}, {2}";
						case "del_success":
							return "&aBereich &7{0} &agelöscht.";
						case "del_all_success":
							return "&aAlle Bereiche gelöscht.";
						case "list_title":
							return "&7Liste aller Bereiche:";
						case "error_exist":
							return "Bereich {0} existiert nicht.";
						case "error_nolocations":
							return "Es sind keine Bereiche vorhanden.";
						case "help_1":
							return "&7Folgende Kommandos stehen zur Verfügung:";
						case "help_2":
							return "&7/location add [sx] [sy] [sz] [ex] [ey] [ez] [name]";
						case "help_3":
							return "&7/location delete [name]";
						case "help_4":
							return "&7/location list";
						case "help_5":
							return "&7/location clear";
						case "invalid_name":
							return "Der Name darf nicht leer sein!";

						default:
							return string.Format("&4FEHLER: &fString '{0}' existiert nicht.", value);
					}
				#endregion

				#region English translation
				case "EN":
					switch (value)
					{
						case "location_change":
							return "&7You just arrived at &3{0}.";
						case "invalid_args":
							return "Invalid arguments. Try &7/location help.";
						case "privilege_missing":
							return "You are not allowed to manage locations.";
						case "add_success":
							return "&aLocation &7{0} &aadded successfully.";
						case "add_start":
							return "&7Start coordinates: {0}, {1}, {2}";
						case "add_end":
							return "&7End coordinates: {0}, {1}, {2}";
						case "del_success":
							return "&aLocation &7{0} &adeleted.";
						case "del_all_success":
							return "&aAll locations deleted.";
						case "list_title":
							return "&7List of Locations:";
						case "error_exist":
							return "Location {0} does not exist.";
						case "error_nolocations":
							return "There are no locations stored.";
						case "help_1":
							return "&7Following commands are available:";
						case "help_2":
							return "&7/location add [sx] [sy] [sz] [ex] [ey] [ez] [name]";
						case "help_3":
							return "&7/location delete [name]";
						case "help_4":
							return "&7/location list";
						case "help_5":
							return "&7/location clear";
						case "invalid_name":
							return "The location name may not be empty!";

						default:
							return string.Format("&4ERROR: &fString '{0}' does not exist.", value);
					}
				#endregion

				default:
					return string.Format("&4ERROR: &fThe language code {0} is not in the list.", languageCode);
			}
		}

		Vector3i GetPlayerPosition(int PlayerID)
		{
			return new Vector3i((int)m.GetPlayerPositionX(PlayerID), (int)m.GetPlayerPositionY(PlayerID), (int)m.GetPlayerPositionZ(PlayerID));
		}

		void LoadLocations()
		{
			try
			{
				if (!File.Exists("UserData" + Path.DirectorySeparatorChar + "LocationList.txt"))
				{
					SaveLocations();
				}
				System.Console.WriteLine("[Locations] Loading LocationList.txt.");
				using (TextReader tr = new StreamReader("UserData" + Path.DirectorySeparatorChar + "LocationList.txt", System.Text.Encoding.UTF8))
				{
					string name = tr.ReadLine();
					while (!string.IsNullOrEmpty(name))
					{
						string startString = tr.ReadLine();
						string[] startCoords = startString.Split(';');
						int sx = int.Parse(startCoords[0]);
						int sy = int.Parse(startCoords[1]);
						int sz = int.Parse(startCoords[2]);

						string endString = tr.ReadLine();
						string[] endCoords = endString.Split(';');
						int ex = int.Parse(endCoords[0]);
						int ey = int.Parse(endCoords[1]);
						int ez = int.Parse(endCoords[2]);

						Location loc = new Location(new Vector3i(sx, sy, sz), new Vector3i(ex, ey, ez));
						loc.name = name;
						locationList.Add(loc);
						System.Console.WriteLine("[Locations] Loaded location: " + name);

						name = tr.ReadLine();
					}
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[Locations] Error while loading LocationList.txt - Please check or delete.");
				System.Console.WriteLine("ERROR:  " + ex.Message);
				return;
			}
			System.Console.WriteLine("[Locations] Successfully loaded LocationList.txt");
		}

		void SaveLocations()
		{
			try
			{
				if (!File.Exists("UserData" + Path.DirectorySeparatorChar + "LocationList.txt"))
				{
					System.Console.WriteLine("[Locations] LocationList.txt does not exist. Creating new.");
				}
				using (StreamWriter sw = new StreamWriter("UserData" + Path.DirectorySeparatorChar + "LocationList.txt", false, System.Text.Encoding.UTF8))
				{
					for (int i = 0; i < locationList.Count; i++)
					{
						//Write 3 lines for each location: Name, Start, End
						sw.WriteLine(locationList[i].name);
						sw.WriteLine(locationList[i].start.x + ";" + locationList[i].start.y + ";" + locationList[i].start.z);
						sw.WriteLine(locationList[i].end.x + ";" + locationList[i].end.y + ";" + locationList[i].end.z);
					}
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("ERROR:  " + ex.Message);
			}
			System.Console.WriteLine("[Locations] Saved Locations.");
		}

		void CheckPlayerPosition()
		{
			int[] players = m.AllPlayers();
			for (int i = 0; i < players.Length; i++)
			{
				string lastSeen = "";
				try
				{
					//Happens if new player (ID never used before) joins
					lastSeen = PlayerLastSeenIn[players[i]];
				}
				catch
				{
					//Simply set to empty so he gets notified the next tick
					lastSeen = "";
				}
				bool PlayerInAnyZone = false;
				for (int j = 0; j < locationList.Count; j++)
				{
					if (IsInArea(locationList[j], GetPlayerPosition(players[i])))
					{
						//Player is in a defined zone
						PlayerInAnyZone = true;
						if (!lastSeen.Equals(locationList[j].name, StringComparison.InvariantCultureIgnoreCase))
						{
							//Player entered different area
							m.SendMessage(players[i], chatPrefix + string.Format(GetLocalizedString("location_change"), locationList[j].name));
							PlayerLastSeenIn[players[i]] = locationList[j].name;
							return;
						}
					}
				}
				if (!PlayerInAnyZone)
					PlayerLastSeenIn[players[i]] = "";
			}
		}

		void AddArea(Vector3i AreaStart, Vector3i AreaEnd, string AreaName)
		{
			Location newLocation = new Location(AreaStart, AreaEnd);
			newLocation.name = AreaName;
			locationList.Add(newLocation);
			SaveLocations();
			System.Console.WriteLine(string.Format("[Locations] Area {0} added.", AreaName));
		}

		bool DeleteArea(string AreaName)
		{
			for (int i = 0; i < locationList.Count; i++)
			{
				if (AreaName.Equals(locationList[i].name, StringComparison.InvariantCultureIgnoreCase))
				{
					locationList.RemoveAt(i);
					SaveLocations();
					System.Console.WriteLine(string.Format("[Locations] Area {0} deleted.", AreaName));
					return true;
				}
			}
			return false;
		}

		bool IsInArea(Location loc, Vector3i position)
		{
			Vector3i area_start = loc.start;
			Vector3i area_end = loc.end;
			if ((area_start.x <= position.x) && (position.x <= area_end.x))
			{
				if ((area_start.y <= position.y) && (position.y <= area_end.y))
				{
					if ((area_start.z <= position.z) && (position.z <= area_end.z))
					{
						return true;
					}
					return false;
				}
				return false;
			}
			return false;
		}

		bool ManageAreas(int player, string command, string argument)
		{
			if (command.Equals("location", StringComparison.InvariantCultureIgnoreCase))
			{
				string[] args;
				string option = "";
				try
				{
					args = argument.Split(' ');
					option = args[0];
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("invalid_args"));
					return true;
				}

				if (option.Equals("add", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "manage_locations"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("privilege_missing"));
						System.Console.WriteLine(string.Format("[Locations] {0} tried to add a location (no permission)", m.GetPlayerName(player)));
						return true;
					}
					int startx, starty, startz, endx, endy, endz = -1;
					string newName = "";
					try
					{
						//Parse given arguments.
						startx = Convert.ToInt32(args[1]);
						starty = Convert.ToInt32(args[2]);
						startz = Convert.ToInt32(args[3]);
						endx = Convert.ToInt32(args[4]);
						endy = Convert.ToInt32(args[5]);
						endz = Convert.ToInt32(args[6]);
						for (int i = 7; i < args.Length; i++)
						{
							newName = newName + " " + args[i];
						}
						newName = newName.Substring(1);
					}
					catch
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("invalid_args"));
						return true;
					}
					if (string.IsNullOrEmpty(newName) || newName == " " || newName == "  ")
					{
						//Don't allow empty names.
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("invalid_name"));
						return true;
					}
					Vector3i start = new Vector3i(startx, starty, startz);
					Vector3i end = new Vector3i(endx, endy, endz);
					AddArea(start, end, newName);
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("add_success"), newName));
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("add_start"), startx.ToString(), starty.ToString(), startz.ToString()));
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("add_end"), endx.ToString(), endy.ToString(), endz.ToString()));
					return true;
				}
				if (option.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "manage_locations"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("privilege_missing"));
						System.Console.WriteLine(string.Format("[Locations] {0} tried to delete a location (no permission)", m.GetPlayerName(player)));
						return true;
					}
					string delName = "";
					try
					{
						for (int i = 1; i < args.Length; i++)
						{
							delName = delName + " " + args[i];
						}
						delName = delName.Substring(1);
					}
					catch
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("invalid_args"));
						return true;
					}

					if (DeleteArea(delName))
					{
						m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("del_success"), delName));
						return true;
					}
					else
					{
						m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_exist"), delName));
						return true;
					}
				}
				if (option.Equals("list", StringComparison.InvariantCultureIgnoreCase))
				{
					if (locationList.Count > 0)
					{
						m.SendMessage(player, chatPrefix + GetLocalizedString("list_title"));
						m.SendMessage(player, "----------------------------------------");
						foreach (Location loc in locationList)
						{
							m.SendMessage(player, string.Format("- {0}", loc.name));
						}
						return true;
					}
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_nolocations"));
					return true;
				}
				if (option.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "manage_locations"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("privilege_missing"));
						System.Console.WriteLine(string.Format("[Locations] {0} tried to clear the location list (no permission)", m.GetPlayerName(player)));
						return true;
					}
					locationList.Clear();
					SaveLocations();
					m.SendMessage(player, chatPrefix + GetLocalizedString("del_all_success"));
					return true;
				}
				if (option.Equals("help", StringComparison.InvariantCultureIgnoreCase))
				{
					m.SendMessage(player, chatPrefix + GetLocalizedString("help_1"));
					m.SendMessage(player, chatPrefix + GetLocalizedString("help_2"));
					m.SendMessage(player, chatPrefix + GetLocalizedString("help_3"));
					m.SendMessage(player, chatPrefix + GetLocalizedString("help_4"));
					m.SendMessage(player, chatPrefix + GetLocalizedString("help_5"));
					return true;
				}
				m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("invalid_args"));
				return true;
			}
			return false;
		}
	}
}

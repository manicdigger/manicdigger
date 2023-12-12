/*
 * Protection Mod - Version 1.2
 * last change: 2017-09-02
 * Author: croxxx
 * 
 * This mod allows protection of certain areas. No players will be able to build there without permission.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ManicDigger.Mods
{
	public class Protection : IMod
	{
		// Desired language code. Currently supported are EN and DE.
		string languageCode = "EN";
		// Enables or disables admins being able to build in any zone
		bool bAllowAdminIgnore = true;
		// Players of this group will be able to ignore zones
		int iAdminPermissionLevel = 3;
		// Enable/Disable display of the owner when you enter a zone
		bool bDisplayPosition = true;

		public void PreStart(ModManager m) { }

		public void Start(ModManager m)
		{
			this.m = m;

			m.RegisterPrivilege("protect");
			m.RegisterCommandHelp("protect", "Create/Delete protected zones.");
			m.RegisterPrivilege("protect_rank");
			m.RegisterCommandHelp("protect_rank", "Rank people in zones.");
			m.RegisterPrivilege("protect_settings");
			m.RegisterCommandHelp("protect_settings", "Edit Mod settings.");
			m.RegisterOnCommand(OnCommand);
			m.RegisterTimer(CheckPlayerPosition, (double)1);
			m.RegisterOnLoad(OnLoad);
			m.RegisterOnSave(OnSave);

			m.RegisterCheckOnBlockBuild(CheckBlock);
			m.RegisterCheckOnBlockDelete(CheckBlock);
			// m.RegisterCheckOnBlockUse(CheckBlock);

			System.Console.WriteLine("[Protection] Loaded Mod Version 1.2");
		}

		//Internal variables.
		//DO NOT CHANGE!
		ModManager m;
		string chatPrefix = "&8[&cProtection&8] ";
		Dictionary<int, string> PlayerLastSeenIn = new Dictionary<int, string>();
		string sModDir = "UserData" + Path.DirectorySeparatorChar + "Protection";
		List<ProtectedZone> protectedZones = new List<ProtectedZone>();
		Vector3i start_pos;
		string start_setBy = "";
		Vector3i end_pos;
		string end_setBy = "";

		public struct Vector3i
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
		public class ProtectedZone
		{
			public ProtectedZone()
			{
				managers = new List<string>();
				builders = new List<string>();
			}
			public ProtectedZone(string newName, string newOwner, Vector3i newStart, Vector3i newEnd)
			{
				if (newStart.x < newEnd.x)
				{
					start.x = newStart.x;
					end.x = newEnd.x;
				}
				else
				{
					start.x = newEnd.x;
					end.x = newStart.x;
				}
				if (newStart.y < newEnd.y)
				{
					start.y = newStart.y;
					end.y = newEnd.y;
				}
				else
				{
					start.y = newEnd.y;
					end.y = newStart.y;
				}
				if (newStart.z < newEnd.z)
				{
					start.z = newStart.z;
					end.z = newEnd.z;
				}
				else
				{
					start.z = newEnd.z;
					end.z = newStart.z;
				}
				name = newName;
				owner = newOwner;
				managers = new List<string>();
				builders = new List<string>();
			}
			public string name;
			public Vector3i start;
			public Vector3i end;
			public string owner;
			public List<string> managers;
			public List<string> builders;
		}

		string GetLocalizedString(string value)
		{
			switch (languageCode)
			{
				#region German translation
				case "DE":
					switch (value)
					{
						case "error_no_permission":
							return "Du darfst dieses Kommando nicht verwenden!";

						case "error_invalid_args":
							return "Ungültige Argumente! Lies das README für mehr Informationen.";

						case "error_build_noperm":
							return "Du darfst in dieser Zone nicht bauen!";

						case "error_set_invalid_lncode":
							return "Sprachcode ungültig! Unterstützt sind: &6DE/EN";

						case "success_set_lncode":
							return "&7Sprache geändert zu: &6DE";

						case "success_set_adminlvl":
							return "&7Neues Admin Level: &6{0}";

						case "info_set_ignore_off":
							return "&7Admin Ignore: &cAUS";

						case "info_set_ignore_on":
							return "&7Admin Ignore: &aAN";

						case "info_set_display_off":
							return "&7Zonenanzeige: &cAUS";

						case "info_set_display_on":
							return "&7Zonenanzeige: &aAN";

						case "error_rank_invparam":
							return "Ungültige Parameter!";

						case "error_rank_manager_remove_noperm":
							return "Nur der Besitzer oder ein Admin können Manager entfernen!";

						case "error_rank_manager_remove_notfound":
							return "{0} ist kein Manager!";

						case "info_rank_manager_remove_notifytarget":
							return "&6{0} hat dich aus der Gruppe Manager in {1} entfernt.";

						case "info_rank_manager_remove_success":
							return "&a{0} wurde aus der Gruppe Manager entfernt.";

						case "error_rank_manager_add_noperm":
							return "Nur der Besitzer oder ein Admin können Manager hinzufügen!";

						case "info_rank_manager_add_notifytarget":
							return "&6{0} hat dich zum Manager in {1} gemacht.";

						case "info_rank_manager_add_success":
							return "&a{0} ist jetzt ein Manager.";

						case "error_rank_manager_add_alreadymanager":
							return "{0} ist bereits ein Manager!";

						case "error_rank_otherrank":
							return "{0} hat bereits einen anderen Rang!";

						case "error_rank_isowner":
							return "{0} ist der Besitzer dieser Zone!";

						case "error_rank_notonline":
							return "{0} ist nicht online!";

						case "error_rank_notinzone":
							return "Du bist in keiner Zone! Begib dich in die Zone, die du bearbeiten willst.";

						case "error_rank_builder_remove_noperm":
							return "Nur der Besitzer, ein Manager oder Admins können Builder entfernen!";

						case "error_rank_builder_remove_notfound":
							return "{0} ist kein Builder!";

						case "info_rank_builder_remove_notifytarget":
							return "&6{0} hat dich aus der Gruppe Builder in {1} entfernt.";

						case "info_rank_builder_remove_success":
							return "&a{0} wurde aus der Gruppe Builder entfernt.";

						case "error_rank_builder_add_noperm":
							return "Nur der Besitzer, ein Manager oder Admins können Builder hinzufügen!";

						case "info_rank_builder_add_notifytarget":
							return "&6{0} hat dich zum Builder in {1} gemacht.";

						case "info_rank_builder_add_success":
							return "&a{0} ist jetzt ein Builder.";

						case "error_rank_builder_add_alreadybuilder":
							return "{0} ist bereits ein Builder!";

						case "error_rank_owner_change_noperm":
							return "Nur der aktuelle Besitzer oder Admins können den Besitzer ändern!";

						case "info_rank_owner_change_notifytarget":
							return "&6{0} hat dich zum Besitzer von {1} gemacht.";

						case "info_rank_owner_change_success":
							return "&a{0} ist jetzt der Besitzer dieser Zone.";

						case "error_rank_owner_change_alreadyowner":
							return "{0} ist bereits der Besitzer dieser Zone!";

						case "error_protect_delete_nonexistant":
							return "Die Zone '{0}' existiert nicht!";

						case "info_protect_set_success":
							return "&aNeue Zone erfolgreich hinzugefügt: ";

						case "error_protect_set_senotsetbyyou":
							return "Start/Ende nicht von dir gesetzt!";

						case "error_protect_set_senotsetbysame":
							return "Start/Ende nicht vom selben Spieler gesetzt!";

						case "error_protect_set_senotset":
							return "Start/Ende nicht gesetzt!";

						case "error_protect_set_nameexists":
							return "Es gibt bereits eine Zone mit diesem Namen!";

						case "error_protect_emptyname":
							return "Der Zonenname darf nicht leer sein!";

						case "info_protect_delete_success":
							return "&aZone gelöscht: ";

						case "error_protect_delete_notinzone":
							return "Du bist nicht in einer Zone! Bitte gib einen Namen an.";

						case "info_protect_end_success":
							return "&aEndpunkt der Zone gesetzt.";

						case "info_protect_start_success":
							return "&aStartpunkt der Zone gesetzt.";

						case "info_protect_endlist":
							return "&7Ende der Liste";

						case "info_protect_startlist":
							return "&7Liste aller Zonen:";

						case "error_protect_notinanyzone":
							return "Du bist in keiner Zone!";

						default:
							return string.Format("&4FEHLER: &fString '{0}' existiert nicht.", value);
					}
				#endregion

				#region English translation
				case "EN":
					switch (value)
					{
						case "error_no_permission":
							return "You are not allowed to use this command!";

						case "error_invalid_args":
							return "Invalid arguments! Read the README for usage information.";

						case "error_build_noperm":
							return "You are not allowed to build in this zone!";

						case "error_set_invalid_lncode":
							return "Language code is invalid! Supported are: &6DE/EN";

						case "success_set_lncode":
							return "&7Set language to: &6EN";

						case "success_set_adminlvl":
							return "&7New Admin Level: &6{0}";

						case "info_set_ignore_off":
							return "&7Admin Ignore: &cOFF";

						case "info_set_ignore_on":
							return "&7Admin Ignore: &aON";

						case "info_set_display_off":
							return "&7Zone Display: &cOFF";

						case "info_set_display_on":
							return "&7Zone Display: &aON";

						case "error_rank_invparam":
							return "Invalid parameters!";

						case "error_rank_manager_remove_noperm":
							return "Only the current owner or an admin may remove a manager!";

						case "error_rank_manager_remove_notfound":
							return "{0} is not a manager!";

						case "info_rank_manager_remove_notifytarget":
							return "&6{0} removed you from managers in {1}.";

						case "info_rank_manager_remove_success":
							return "&a{0} has been removed from managers.";

						case "error_rank_manager_add_noperm":
							return "Only the current owner or an admin may add a manager!";

						case "info_rank_manager_add_notifytarget":
							return "&6{0} made you manager in: {1}.";

						case "info_rank_manager_add_success":
							return "&a{0} is now a manager.";

						case "error_rank_manager_add_alreadymanager":
							return "{0} already is a manager!";

						case "error_rank_otherrank":
							return "{0} already has another rank!";

						case "error_rank_isowner":
							return "{0} is the owner of this zone!";

						case "error_rank_notonline":
							return "{0} is not online!";

						case "error_rank_notinzone":
							return "You are not in any zone! Enter a zone to edit ranks.";

						case "error_rank_builder_remove_noperm":
							return "Only managers, the owner or an admin may remove builders!";

						case "error_rank_builder_remove_notfound":
							return "{0} is not a builder!";

						case "info_rank_builder_remove_notifytarget":
							return "&6{0} removed you from builders in {1}.";

						case "info_rank_builder_remove_success":
							return "&a{0} has been removed from builders.";

						case "error_rank_builder_add_noperm":
							return "Only managers, the current owner or an admin may add a builder!";

						case "info_rank_builder_add_notifytarget":
							return "&6{0} made you builder in: {1}.";

						case "info_rank_builder_add_success":
							return "&a{0} is now a builder.";

						case "error_rank_builder_add_alreadybuilder":
							return "{0} already is a builder!";

						case "error_rank_owner_change_noperm":
							return "Only the current owner or an admin may change the owner!";

						case "info_rank_owner_change_notifytarget":
							return "&6{0} made you owner of: {1}.";

						case "info_rank_owner_change_success":
							return "&a{0} now owns this zone.";

						case "error_rank_owner_change_alreadyowner":
							return "{0} already owns this zone!";

						case "error_protect_delete_nonexistant":
							return "The zone '{0}' does not exist!";

						case "info_protect_set_success":
							return "&aSuccessfully set new zone: ";

						case "error_protect_set_senotsetbyyou":
							return "Start/End not set by you!";

						case "error_protect_set_senotsetbysame":
							return "Start/End not set by same player!";

						case "error_protect_set_senotset":
							return "Start/End not set!";

						case "error_protect_set_nameexists":
							return "A zone with this name already exists!";

						case "error_protect_emptyname":
							return "Zone name may not be empty!";

						case "info_protect_delete_success":
							return "&aSuccessfully deleted zone: ";

						case "error_protect_delete_notinzone":
							return "You are not in any zone! Please supply a name.";

						case "info_protect_end_success":
							return "&aSuccessfully set zone end point.";

						case "info_protect_start_success":
							return "&aSuccessfully set zone start point.";

						case "info_protect_endlist":
							return "&7End of list";

						case "info_protect_startlist":
							return "&7List of all zones:";

						case "error_protect_notinanyzone":
							return "You are not in any zone!";

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
			//Return the player's position (converted to int)
			return new Vector3i((int)m.GetPlayerPositionX(PlayerID), (int)m.GetPlayerPositionY(PlayerID), (int)m.GetPlayerPositionZ(PlayerID));
		}

		int GetPlayerID(string playerName)
		{
			//Gets the ID of a given player name (not case-sensitive). Returns -1 if player is not found.
			int target = -1;
			int[] allPlayers = m.AllPlayers();
			for (int i = 0; i < allPlayers.Length; i++)
			{
				if (playerName.Equals(m.GetPlayerName(allPlayers[i]), StringComparison.InvariantCultureIgnoreCase))
				{
					target = allPlayers[i];
					break;
				}
			}
			return target;
		}

		string ArrayToString(string[] array)
		{
			string sResult = "";
			for (int i = 0; i < array.Length; i++)
			{
				sResult += (array[i] + " ");
			}
			return sResult.Trim();
		}

		bool IsInAnyZone(int player)
		{
			//Checks if the player is in any zone
			for (int i = 0; i < protectedZones.Count; i++)
			{
				if (IsInZone(protectedZones[i], player))
					return true;
			}
			return false;
		}

		bool IsInZone(ProtectedZone zone, int player)
		{
			//Checks if player is in the given zone
			Vector3i position = GetPlayerPosition(player);  //Get the player's current position
			return IsPositionInZone(zone, position);
		}

		bool IsPositionInZone(ProtectedZone zone, Vector3i position)
		{
			if ((zone.start.x <= position.x) && (position.x <= zone.end.x))
			{
				if ((zone.start.y <= position.y) && (position.y <= zone.end.y))
				{
					if ((zone.start.z <= position.z) && (position.z <= zone.end.z))
					{
						return true;
					}
					return false;
				}
				return false;
			}
			return false;
		}

		bool IsPlayerPermittedToBuild(ProtectedZone zone, int player)
		{
			//Check if player is admin (and feature is enabled).
			if (bAllowAdminIgnore)
			{
				if (m.GetPlayerPermissionLevel(player) >= iAdminPermissionLevel)
				{
					return true;
				}
			}
			string sPlayerName = m.GetPlayerName(player);
			if (zone.owner.Equals(sPlayerName, StringComparison.InvariantCultureIgnoreCase))
			{
				//The owner of a zone should always be able to build in it ;)
				return true;
			}
			//First check for builders as this case is more likely
			for (int i = 0; i < zone.builders.Count; i++)
			{
				if (zone.builders[i].Equals(sPlayerName, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			//Then look inside managers list...
			for (int i = 0; i < zone.managers.Count; i++)
			{
				if (zone.managers[i].Equals(sPlayerName, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			//User in no permitted group. No build privileges.
			return false;
		}

		bool ZoneExists(string name)
		{
			//Checks if a zone with the given name exists
			for (int i = 0; i < protectedZones.Count; i++)
			{
				if (name.Equals(protectedZones[i].name, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}
			return false;
		}

		int GetZoneID(string name)
		{
			//Gets the position of a zone in the list by name. Returns -1 if not found.
			for (int i = 0; i < protectedZones.Count; i++)
			{
				if (name.Equals(protectedZones[i].name, StringComparison.InvariantCultureIgnoreCase))
				{
					//Match, if name is equal. Return index.
					return i;
				}
			}
			//No match. Should never happen, but here for safety.
			return -1;
		}

		int GetRoleID(ProtectedZone zone, int player)
		{
			/* Player role IDs used here:
             * 0 - No role
             * 1 - Builder
             * 2 - Manager
             * 3 - Owner
             */
			string sPlayerName = m.GetPlayerName(player);
			if (zone.owner.Equals(sPlayerName, StringComparison.InvariantCultureIgnoreCase))
				return 3;
			for (int i = 0; i < zone.builders.Count; i++)
			{
				if (zone.builders[i].Equals(sPlayerName, StringComparison.InvariantCultureIgnoreCase))
					return 1;
			}
			for (int i = 0; i < zone.managers.Count; i++)
			{
				if (zone.managers[i].Equals(sPlayerName, StringComparison.InvariantCultureIgnoreCase))
					return 2;
			}
			return 0;
		}

		void DisplayZoneInfo(ProtectedZone zone, int player)
		{
			m.SendMessage(player, chatPrefix + string.Format("&e{0}", zone.name));
			m.SendMessage(player, chatPrefix + string.Format("&4Owner: &7{0}", zone.owner));
			m.SendMessage(player, chatPrefix + string.Format("&3Manager: &7{0}", string.Join(", ", zone.managers.ToArray())));
			m.SendMessage(player, chatPrefix + string.Format("&2Builder: &7{0}", string.Join(", ", zone.builders.ToArray())));
		}

		void CheckPlayerPosition()
		{
			if (bDisplayPosition)
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
					for (int j = 0; j < protectedZones.Count; j++)
					{
						if (IsInZone(protectedZones[j], players[i]))
						{
							//Player is in a defined zone
							PlayerInAnyZone = true;
							if (!lastSeen.Equals(protectedZones[j].name, StringComparison.InvariantCultureIgnoreCase))
							{
								//Player entered different zone
								DisplayZoneInfo(protectedZones[j], players[i]);
								PlayerLastSeenIn[players[i]] = protectedZones[j].name;
								return;
							}
						}
					}
					if (!PlayerInAnyZone)
						PlayerLastSeenIn[players[i]] = "";
				}
			}
		}

		void AddZone(string name, string owner, Vector3i start, Vector3i end)
		{
			//Handles adding of new zones
			ProtectedZone newZone = new ProtectedZone(name, owner, start, end);
			protectedZones.Add(newZone);
			System.Console.WriteLine(string.Format("[Protection] Successfully added zone: {0} - Owner: {1}", name, owner));
		}

		void DeleteZone(string name)
		{
			//Handles deletion of zones
			int iDeleteID = GetZoneID(name);
			if (iDeleteID == -1)
			{
				//Zone not found. Should never happen, but better check twice
				System.Console.WriteLine("[Protection] ERROR in DeleteZone: name not found!");
			}
			else
			{
				//Zone found and may be deleted
				protectedZones.RemoveAt(iDeleteID);
				File.Delete(sModDir + Path.DirectorySeparatorChar + name.ToLower().Replace(' ', '_') + ".zone");
				System.Console.WriteLine("[Protection] Successfully removed zone: " + name);
			}
		}

		void SaveSettings()
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "settings.txt"))
				{
					//Write all temporary settings to a file
					sw.WriteLine(languageCode);
					sw.WriteLine(bAllowAdminIgnore);
					sw.WriteLine(iAdminPermissionLevel);
					sw.WriteLine(bDisplayPosition);
				}
				System.Console.WriteLine("[Protection] Settings saved.");
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[Protection] ERROR:  " + ex.Message);
			}
		}

		void LoadSettings()
		{
			if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "settings.txt"))
			{
				//No settings file found. Create new.
				System.Console.WriteLine("[Protection] 'settings.txt' not found. Creating new.");
				SaveSettings();
				return;
			}
			//Else use existing config
			try
			{
				using (TextReader tr = new StreamReader(sModDir + Path.DirectorySeparatorChar + "settings.txt", Encoding.UTF8))
				{
					languageCode = tr.ReadLine();
					bAllowAdminIgnore = bool.Parse(tr.ReadLine());
					iAdminPermissionLevel = int.Parse(tr.ReadLine());
					bDisplayPosition = bool.Parse(tr.ReadLine());
					System.Console.WriteLine("[Protection] Loaded settings");
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[Protection] ERROR:  " + ex.Message);
			}
		}

		void OnSave()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(ProtectedZone));

			foreach (var zone in protectedZones)
			{
				// Replace all invalid characters wich an underscore
				string fileName = sModDir + Path.DirectorySeparatorChar + string.Join("_", zone.name.Split(Path.GetInvalidFileNameChars())) + ".zone";

				try
				{
					using (TextWriter sw = new StreamWriter(fileName))
					{
						serializer.Serialize(sw, zone);
					}
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("[Protection] ERROR while saving '{0}': {1}", fileName, ex.Message);
				}
			}

			System.Console.WriteLine("[Protection] Saved all zones.");
		}

		void OnLoad()
		{
			if (!Directory.Exists(sModDir))
			{
				//Directory "UserData\Protection" does not exist. Create new.
				System.Console.WriteLine("[Protection] '" + sModDir + "' not found. Creating new.");
				Directory.CreateDirectory(sModDir);
			}

			LoadSettings();
			DirectoryInfo di = new DirectoryInfo(sModDir);
			FileInfo[] files = di.GetFiles("*.zone");
			XmlSerializer serializer = new XmlSerializer(typeof(ProtectedZone));

			foreach (FileInfo fi in files)
			{
				try
				{
					using (FileStream fs = new FileStream(fi.FullName, FileMode.Open))
					{
						// Deserialize object from file
						ProtectedZone loaded = (ProtectedZone)serializer.Deserialize(fs);

						// Add loaded object to the list
						protectedZones.Add(loaded);
						System.Console.WriteLine("[Protection] Loaded zone XML: " + loaded.name);
					}
				}
				catch (Exception e)
				{
					System.Console.WriteLine(e.Message);
					System.Console.WriteLine("[Protection] WARNING: Could not parse zone file as XML. Trying old data format...");
					try
					{
						using (TextReader tr = new StreamReader(fi.FullName, Encoding.UTF8))
						{
							char[] separator = new char[1];
							separator[0] = ';';
							//First, read all data in the file that is needed.
							string name = tr.ReadLine();
							string startString = tr.ReadLine();
							string endString = tr.ReadLine();
							string owner = tr.ReadLine();
							string managerString = tr.ReadLine();
							string builderString = tr.ReadLine();

							//Now process the vectors, as they are needed first
							string[] split; //Array to hold stuff that's being parsed. reused for performance.
							split = startString.Split(separator);
							Vector3i start = new Vector3i(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
							split = endString.Split(separator);
							Vector3i end = new Vector3i(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));

							//Generate the ProtectedZone object we want to add later and already set the owner variable as it needs no additional processing
							ProtectedZone loaded = new ProtectedZone(name, owner, start, end);

							//Parse the manager and builder lists
							split = managerString.Split(separator);
							for (int i = 0; i < split.Length; i++)
							{
								if (!string.IsNullOrEmpty(split[i]))
									loaded.managers.Add(split[i]);
							}
							split = builderString.Split(separator);
							for (int i = 0; i < split.Length; i++)
							{
								if (!string.IsNullOrEmpty(split[i]))
									loaded.builders.Add(split[i]);
							}

							//Finally add the new object to the list
							protectedZones.Add(loaded);
							System.Console.WriteLine("[Protection] Loaded zone: " + loaded.name);
						}
					}
					catch (Exception ex)
					{
						System.Console.WriteLine("[Protection] ERROR: " + ex.Message);
					}
				}
			}
			System.Console.WriteLine("[Protection] Finished loading.");
		}

		bool OnCommand(int player, string command, string argument)
		{
			#region /protect command (/pr)
			if (command.Equals("protect", StringComparison.InvariantCultureIgnoreCase) || command.Equals("pr", StringComparison.InvariantCultureIgnoreCase))
			{
				if (argument.Equals("info", StringComparison.InvariantCultureIgnoreCase))
				{
					for (int i = 0; i < protectedZones.Count; i++)
					{
						if (IsInZone(protectedZones[i], player))
						{
							//Player is in a zone. Display that zone's information.
							DisplayZoneInfo(protectedZones[i], player);
							return true;
						}
					}
					//Player not in any zone (ensured by previous return statement)
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_notinanyzone"));
					System.Console.WriteLine(string.Format("[Protection] {0} tried view zone info (not in any zone)", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("list", StringComparison.InvariantCultureIgnoreCase))
				{
					//Output a list of all saved zones
					m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_startlist"));
					for (int i = 0; i < protectedZones.Count; i++)
					{
						m.SendMessage(player, chatPrefix + "&7- " + protectedZones[i].name);
					}
					m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_endlist"));
					return true;
				}
				//-------------------------------------------------------------------------------------------------------------------------------
				//For all following actions the privilege is required.
				if (!m.PlayerHasPrivilege(player, "protect"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_no_permission"));
					System.Console.WriteLine(string.Format("[Protection] {0} tried to protect an area (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("start", StringComparison.InvariantCultureIgnoreCase))
				{
					start_pos = GetPlayerPosition(player);
					start_setBy = m.GetPlayerName(player);
					m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_start_success"));
					System.Console.WriteLine(string.Format("[Protection] {0} set zone start point.", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("end", StringComparison.InvariantCultureIgnoreCase))
				{
					end_pos = GetPlayerPosition(player);
					end_setBy = m.GetPlayerName(player);
					m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_end_success"));
					System.Console.WriteLine(string.Format("[Protection] {0} set zone end point.", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!IsInAnyZone(player))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_delete_notinzone"));
						System.Console.WriteLine(string.Format("[Protection] {0} tried to delete a zone by position (not in any zone)", m.GetPlayerName(player)));
						return true;
					}
					string toDelete = "";
					for (int i = 0; i < protectedZones.Count; i++)
					{
						if (IsInZone(protectedZones[i], player))
						{
							toDelete = protectedZones[i].name;
							break;
						}
					}
					//Finally delete the zone and notify the player
					DeleteZone(toDelete);
					m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_delete_success") + toDelete);
					return true;
				}
				//Additional argument parsing required for addition/deletion of zones
				string[] args;
				try
				{
					args = argument.Split(' ');
					string[] nameArray = new string[args.Length - 1];
					Array.Copy(args, 1, nameArray, 0, args.Length - 1);
					string name = ArrayToString(nameArray);
					if (args[0].Equals("set", StringComparison.InvariantCultureIgnoreCase))
					{
						//First check if supplied name is empty.
						if (string.IsNullOrEmpty(name))
						{
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_emptyname"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to set a zone (no name given)", m.GetPlayerName(player)));
							return true;
						}
						//Now check if zone with same name already exists
						if (ZoneExists(name))
						{
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_set_nameexists"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to set a zone (already exists)", m.GetPlayerName(player)));
							return true;
						}
						//Check if start/end are correctly set
						if (string.IsNullOrEmpty(start_setBy) || string.IsNullOrEmpty(end_setBy))
						{
							//Start or End point not set. Output error message and abort.
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_set_senotset"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to set a zone (start/end not set)", m.GetPlayerName(player)));
							return true;
						}
						if (!start_setBy.Equals(end_setBy, StringComparison.InvariantCultureIgnoreCase))
						{
							//Start or End point not set by the same player. Output error message and abort.
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_set_senotsetbysame"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to use paste (start/end not set by same player)", m.GetPlayerName(player)));
							return true;
						}
						if ((!start_setBy.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase)) || (!end_setBy.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase)))
						{
							//Start or End point not set by the player who tries to set the zone. Output error message and abort.
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_set_senotsetbyyou"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to teleport (start/end not set by current player)", m.GetPlayerName(player)));
							return true;
						}
						//Finally add the zone and notify the player
						AddZone(name, m.GetPlayerName(player), start_pos, end_pos);
						m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_set_success") + name);
						return true;
					}
					if (args[0].Equals("delete", StringComparison.InvariantCultureIgnoreCase))
					{
						//First check if supplied name is empty.
						if (string.IsNullOrEmpty(name))
						{
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_protect_emptyname"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to delete a zone by name (no name given)", m.GetPlayerName(player)));
							return true;
						}
						if (!ZoneExists(name))
						{
							m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_protect_delete_nonexistant"), name));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to delete a zone by name (does not exist)", m.GetPlayerName(player)));
							return true;
						}
						//Finally delete the zone and notify the player
						DeleteZone(name);
						m.SendMessage(player, chatPrefix + GetLocalizedString("info_protect_delete_success") + name);
						return true;
					}
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
					return true;
				}
				m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
				return true;
			}
			#endregion

			#region /protect_rank command (/pr_rank)
			if (command.Equals("protect_rank", StringComparison.InvariantCultureIgnoreCase) || command.Equals("pr_rank", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!(m.PlayerHasPrivilege(player, "protect_rank") || m.PlayerHasPrivilege(player, "protect"))) //Require 1 of these
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_no_permission"));
					System.Console.WriteLine(string.Format("[Protection] {0} tried to rank someone (no permission)", m.GetPlayerName(player)));
					return true;
				}
				int iZoneID = -1;
				int iRoleID = -1;   //For details see GetRoleID()
				for (int i = 0; i < protectedZones.Count; i++)
				{
					if (IsInZone(protectedZones[i], player))
					{
						iZoneID = i;
						iRoleID = GetRoleID(protectedZones[i], player);
					}
				}
				if ((iZoneID == -1) || (iRoleID == -1))
				{
					//Zone not found or error finding player role ID. Output error and abort.
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_notinzone"));
					System.Console.WriteLine(string.Format("[Protection] {0} tried to rank someone (not in any zone)", m.GetPlayerName(player)));
					return true;
				}
				//Do the ranking stuff, check permissions
				string[] args;
				try
				{
					//Determine what the player actually wants to do
					args = argument.Split(' ');
					if (args.Length < 2)
					{
						//Too few arguments given. Output error message and abort.
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
						System.Console.WriteLine(string.Format("[Protection] {0} tried to rank someone (invalid arguments)", m.GetPlayerName(player)));
						return true;
					}
					#region owner
					if (args[0].Equals("owner", StringComparison.InvariantCultureIgnoreCase))
					{
						//Changing the zone owner is only allowed for the current owner or an admin
						if (iRoleID == 3 || m.GetPlayerPermissionLevel(player) >= iAdminPermissionLevel)
						{
							//Player has permission, check if target is online.
							int targetID = GetPlayerID(args[1]);
							if (targetID == -1)
							{
								//Player not online. Display error and abort.
								m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_notonline"), args[1]));
								System.Console.WriteLine(string.Format("[Protection] {0} tried to change the owner (target offline)", m.GetPlayerName(player)));
								return true;
							}
							else if (GetRoleID(protectedZones[iZoneID], targetID) > 0)
							{
								//Target already has a rank
								if (GetRoleID(protectedZones[iZoneID], targetID) == 3)
								{
									//Target is owner. Owner can't be made owner.
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_owner_change_alreadyowner"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} owner of {2} (target already owner)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
								else
								{
									//Target has another rank. Remove them from any groups they might be in.
									System.Console.WriteLine(string.Format("[Protection] {0} has another rank. Removing...", m.GetPlayerName(targetID)));
									//Check in builders
									for (int i = 0; i < protectedZones[iZoneID].builders.Count; i++)
									{
										if (protectedZones[iZoneID].builders[i].Equals(m.GetPlayerName(targetID), StringComparison.InvariantCultureIgnoreCase))
										{
											//Player found. Delete.
											protectedZones[iZoneID].builders.RemoveAt(i);
											System.Console.WriteLine(string.Format("[Protection] Removed from builders."));
											break;
										}
									}
									//Check in managers
									for (int i = 0; i < protectedZones[iZoneID].managers.Count; i++)
									{
										if (protectedZones[iZoneID].managers[i].Equals(m.GetPlayerName(targetID), StringComparison.InvariantCultureIgnoreCase))
										{
											//Player found. Delete.
											protectedZones[iZoneID].managers.RemoveAt(i);
											System.Console.WriteLine(string.Format("[Protection] Removed from managers."));
											break;
										}
									}
									System.Console.WriteLine(string.Format("[Protection] Done."));
								}
							}
							else
							{
								//Change the owner and notify player and target
								ProtectedZone tempZone = protectedZones[iZoneID];
								tempZone.owner = m.GetPlayerName(targetID);
								protectedZones[iZoneID] = tempZone;
								m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_rank_owner_change_success"), m.GetPlayerName(targetID)));
								m.SendMessage(targetID, chatPrefix + string.Format(GetLocalizedString("info_rank_owner_change_notifytarget"), m.GetPlayerName(player), protectedZones[iZoneID].name));
								System.Console.WriteLine(string.Format("[Protection] {0} made {1} owner of {2}", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
								return true;
							}
						}
						else
						{
							//Player not owner or admin. Display error and abort
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_owner_change_noperm"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to change the owner (no permission)", m.GetPlayerName(player)));
							return true;
						}
					}
					#endregion
					#region builder_add
					if (args[0].Equals("builder_add", StringComparison.InvariantCultureIgnoreCase))
					{
						//Adding builders is allowed for managers, the current owner or an admin
						if (iRoleID >= 2 || m.GetPlayerPermissionLevel(player) >= iAdminPermissionLevel)
						{
							//Player has permission, check if target is online and if they already have a rank.
							int targetID = GetPlayerID(args[1]);
							if (targetID == -1)
							{
								//Player not online. Display error and abort.
								m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_notonline"), args[1]));
								System.Console.WriteLine(string.Format("[Protection] {0} tried to add a builder (target offline)", m.GetPlayerName(player)));
								return true;
							}
							else if (GetRoleID(protectedZones[iZoneID], targetID) > 0)
							{
								//Target already has a rank
								if (GetRoleID(protectedZones[iZoneID], targetID) == 3)
								{
									//Target is owner. Owner can't be part of any other groups.
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_isowner"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} builder in {2} (target is owner)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
								else if (GetRoleID(protectedZones[iZoneID], targetID) == 1)
								{
									//Target already is a builder. Output error message and abort.
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_builder_add_alreadybuilder"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} builder in {2} (target already builder)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
								else
								{
									//Target has another rank
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_otherrank"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} builder in {2} (target has higher rank)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
							}
							else
							{
								//Add the new builder and notify player and target
								protectedZones[iZoneID].builders.Add(m.GetPlayerName(targetID));
								m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_rank_builder_add_success"), m.GetPlayerName(targetID)));
								m.SendMessage(targetID, chatPrefix + string.Format(GetLocalizedString("info_rank_builder_add_notifytarget"), m.GetPlayerName(player), protectedZones[iZoneID].name));
								System.Console.WriteLine(string.Format("[Protection] {0} made {1} builder in {2}", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
								return true;
							}
						}
						else
						{
							//Player not owner, manager or admin. Display error and abort
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_builder_add_noperm"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to add a builder (no permission)", m.GetPlayerName(player)));
							return true;
						}
					}
					#endregion
					#region builder_remove
					if (args[0].Equals("builder_remove", StringComparison.InvariantCultureIgnoreCase))
					{
						//Removing builders is allowed for managers, the current owner or an admin
						if (iRoleID >= 2 || m.GetPlayerPermissionLevel(player) >= iAdminPermissionLevel)
						{
							//Player has permission
							for (int i = 0; i < protectedZones[iZoneID].builders.Count; i++)
							{
								if (protectedZones[iZoneID].builders[i].Equals(args[1], StringComparison.InvariantCultureIgnoreCase))
								{
									//Player found. Delete, display message and notify player
									protectedZones[iZoneID].builders.RemoveAt(i);
									m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_rank_builder_remove_success"), args[1]));
									int targetID = GetPlayerID(args[1]);
									if (targetID != -1)
									{
										//Notifies the target if it's online
										m.SendMessage(targetID, chatPrefix + string.Format(GetLocalizedString("info_rank_builder_remove_notifytarget"), m.GetPlayerName(player), protectedZones[iZoneID].name));
									}
									System.Console.WriteLine(string.Format("[Protection] {0} removed {1} from builders in {2}", m.GetPlayerName(player), args[1], protectedZones[iZoneID].name));
									return true;
								}
							}
							//Player not found in the list. Display error and abort.
							m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_builder_remove_notfound"), args[1]));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to remove {1} from builders (not in list)", m.GetPlayerName(player), args[1]));
							return true;
						}
						else
						{
							//Player not owner or admin. Display error and abort
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_builder_remove_noperm"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to remove a builder (no permission)", m.GetPlayerName(player)));
							return true;
						}
					}
					#endregion
					#region manager_add
					if (args[0].Equals("manager_add", StringComparison.InvariantCultureIgnoreCase))
					{
						//Adding managers is only allowed for the current owner or an admin
						if (iRoleID == 3 || m.GetPlayerPermissionLevel(player) >= iAdminPermissionLevel)
						{
							//Player has permission, check if target is online and if they already have a rank.
							int targetID = GetPlayerID(args[1]);
							if (targetID == -1)
							{
								//Player not online. Display error and abort.
								m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_notonline"), args[1]));
								System.Console.WriteLine(string.Format("[Protection] {0} tried to add a manager (target offline)", m.GetPlayerName(player)));
								return true;
							}
							else if (GetRoleID(protectedZones[iZoneID], targetID) > 0)
							{
								//Target already has a rank
								if (GetRoleID(protectedZones[iZoneID], targetID) == 3)
								{
									//Target is owner. Owner can't be part of any other groups.
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_isowner"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} manager in {2} (target is owner)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
								else if (GetRoleID(protectedZones[iZoneID], targetID) == 2)
								{
									//Target already is a builder. Output error message and abort.
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_manager_add_alreadymanager"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} manager in {2} (target already manager)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
								else
								{
									//Target has another rank
									m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_otherrank"), m.GetPlayerName(targetID)));
									System.Console.WriteLine(string.Format("[Protection] {0} tried to make {1} manager in {2} (target has another rank)", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
									return true;
								}
							}
							else
							{
								//Add the new manager and notify player and target
								protectedZones[iZoneID].managers.Add(m.GetPlayerName(targetID));
								m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_rank_manager_add_success"), m.GetPlayerName(targetID)));
								m.SendMessage(targetID, chatPrefix + string.Format(GetLocalizedString("info_rank_manager_add_notifytarget"), m.GetPlayerName(player), protectedZones[iZoneID].name));
								System.Console.WriteLine(string.Format("[Protection] {0} made {1} manager in {2}", m.GetPlayerName(player), m.GetPlayerName(targetID), protectedZones[iZoneID].name));
								return true;
							}
						}
						else
						{
							//Player not owner or admin. Display error and abort
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_manager_add_noperm"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to add a manager (no permission)", m.GetPlayerName(player)));
							return true;
						}
					}
					#endregion
					#region manager_remove
					if (args[0].Equals("manager_remove", StringComparison.InvariantCultureIgnoreCase))
					{
						//Removing managers is only allowed for the current owner or an admin
						if (iRoleID == 3 || m.GetPlayerPermissionLevel(player) >= iAdminPermissionLevel)
						{
							//Player has permission
							for (int i = 0; i < protectedZones[iZoneID].managers.Count; i++)
							{
								if (protectedZones[iZoneID].managers[i].Equals(args[1], StringComparison.InvariantCultureIgnoreCase))
								{
									//Player found. Delete, display message and notify player
									protectedZones[iZoneID].managers.RemoveAt(i);
									m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_rank_manager_remove_success"), args[1]));
									int targetID = GetPlayerID(args[1]);
									if (targetID != -1)
									{
										//Notifies the target if it's online
										m.SendMessage(targetID, chatPrefix + string.Format(GetLocalizedString("info_rank_manager_remove_notifytarget"), m.GetPlayerName(player), protectedZones[iZoneID].name));
									}
									System.Console.WriteLine(string.Format("[Protection] {0} removed {1} from managers in {2}", m.GetPlayerName(player), args[1], protectedZones[iZoneID].name));
									return true;
								}
							}
							//Player not found in the list. Display error and abort.
							m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("error_rank_manager_remove_notfound"), args[1]));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to remove {1} from managers (not in list)", m.GetPlayerName(player), args[1]));
							return true;
						}
						else
						{
							//Player not owner or admin. Display error and abort
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_manager_remove_noperm"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to remove a manager (no permission)", m.GetPlayerName(player)));
							return true;
						}
					}
					#endregion
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_rank_invparam"));
					System.Console.WriteLine();
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
					return true;
				}
				return true;
			}
			#endregion

			#region /protect_settings command (/pr_set)
			if (command.Equals("protect_settings", StringComparison.InvariantCultureIgnoreCase) || command.Equals("pr_set", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "protect_settings"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_no_permission"));
					System.Console.WriteLine(string.Format("[Protection] {0} tried to change settings (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("display_pos", StringComparison.InvariantCultureIgnoreCase))
				{
					//Toggle display of zones a player enters
					if (bDisplayPosition)
					{
						bDisplayPosition = false;
						m.SendMessage(player, chatPrefix + GetLocalizedString("info_set_display_off"));
						System.Console.WriteLine(string.Format("[Protection] {0} disabled display_pos", m.GetPlayerName(player)));
					}
					else
					{
						bDisplayPosition = true;
						m.SendMessage(player, chatPrefix + GetLocalizedString("info_set_display_on"));
						System.Console.WriteLine(string.Format("[Protection] {0} enabled display_pos", m.GetPlayerName(player)));
					}
					SaveSettings();
					return true;
				}
				if (argument.Equals("admin_ignore", StringComparison.InvariantCultureIgnoreCase))
				{
					//Toggle the ability of admins to ignore zones
					if (bAllowAdminIgnore)
					{
						bAllowAdminIgnore = false;
						m.SendMessage(player, chatPrefix + GetLocalizedString("info_set_ignore_off"));
						System.Console.WriteLine(string.Format("[Protection] {0} disabled admin_ignore", m.GetPlayerName(player)));
					}
					else
					{
						bAllowAdminIgnore = true;
						m.SendMessage(player, chatPrefix + GetLocalizedString("info_set_ignore_on"));
						System.Console.WriteLine(string.Format("[Protection] {0} enabled admin_ignore", m.GetPlayerName(player)));
					}
					SaveSettings();
					return true;
				}
				string[] args;
				try
				{
					args = argument.Split(' ');
					if (args[0].Equals("admin_level", StringComparison.InvariantCultureIgnoreCase) && (args.Length >= 2))
					{
						//Sets the admin level
						iAdminPermissionLevel = int.Parse(args[1]);
						m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("success_set_adminlvl"), iAdminPermissionLevel));
						System.Console.WriteLine(string.Format("[Protection] {0} set admin_level to {1}", m.GetPlayerName(player), iAdminPermissionLevel));
						SaveSettings();
						return true;
					}
					if (args[0].Equals("language_code", StringComparison.InvariantCultureIgnoreCase) && (args.Length >= 2))
					{
						//Change the language code
						if (args[1].Equals("EN", StringComparison.InvariantCultureIgnoreCase))
						{
							languageCode = "EN";
							m.SendMessage(player, chatPrefix + GetLocalizedString("success_set_lncode"));
							System.Console.WriteLine(string.Format("[Protection] {0} set language_code to {1}", m.GetPlayerName(player), languageCode));
						}
						else if (args[1].Equals("DE", StringComparison.InvariantCultureIgnoreCase))
						{
							languageCode = "DE";
							m.SendMessage(player, chatPrefix + GetLocalizedString("success_set_lncode"));
							System.Console.WriteLine(string.Format("[Protection] {0} set language_code to {1}", m.GetPlayerName(player), languageCode));
						}
						else
						{
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_set_invalid_lncode"));
							System.Console.WriteLine(string.Format("[Protection] {0} tried to change the language (invalid code)", m.GetPlayerName(player)));
						}
						SaveSettings();
						return true;
					}
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
					return true;
				}
				m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
				return true;
			}

			#endregion
			return false;
		}

		bool CheckBlock(int player, int x, int y, int z)
		{
			//Loop through all zones to find out if the block is in one
			Vector3i pos = new Vector3i(x, y, z);
			for (int i = 0; i < protectedZones.Count; i++)
			{
				if (IsPositionInZone(protectedZones[i], pos))
				{
					if (!IsPlayerPermittedToBuild(protectedZones[i], player))
					{
						// Player not allowed to build.
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_build_noperm"));
						return false;
					}
				}
			}

			// Player either not in any zone or permitted to build in all zones they are in
			return true;
		}
	}
}

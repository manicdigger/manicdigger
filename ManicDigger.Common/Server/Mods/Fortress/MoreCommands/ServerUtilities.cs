/*
 * ServerUtilities Mod - Version 1.3
 * Last changed: 2015-02-18
 * Author: croxxx
 * 
 * This mod adds a lot of useful commands to your server.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
	public class ServerUtilities : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}

		public void Start(ModManager manager)
		{
			m = manager;
			iRestartInterval = m.GetAutoRestartInterval();

			m.RegisterOnCommand(TeleportToID);
			m.RegisterCommandHelp("tp_id", "Teleports you to the player with the given ID");
			m.RegisterPrivilege("pull");
			m.RegisterOnCommand(PullPlayer);
			m.RegisterCommandHelp("pull", "Teleports the selected player to you");
			m.RegisterOnCommand(PullPlayerID);
			m.RegisterCommandHelp("pull_id", "Teleports the selected player to you (by Player ID)");
			m.RegisterPrivilege("countdown");
			m.RegisterTimer(Countdown_Tick, (double)1);
			m.RegisterOnCommand(Countdown_Start);
			m.RegisterCommandHelp("countdown", "Starts a countdown from a given number (in seconds)");
			m.RegisterOnCommand(DisplayColors);
			m.RegisterCommandHelp("colors", "Displays all colors that can be used in chat");
			m.RegisterPrivilege("copypaste");
			m.RegisterOnCommand(CopyPaste);
			m.RegisterCommandHelp("copypaste", "Allows you to copy/move specified areas of the map to other places");
			m.RegisterPrivilege("vote");
			m.RegisterPrivilege("startvote");
			m.RegisterTimer(Vote_Tick, (double)60);
			m.RegisterOnCommand(Vote);
			m.RegisterCommandHelp("startvote", "Allows you to start votings");
			m.RegisterPrivilege("warn");
			m.RegisterOnCommand(WarnPlayer);
			m.RegisterCommandHelp("warn", "Allows you to warn other players");
			m.RegisterPrivilege("report");
			m.RegisterOnCommand(ReportPlayer);
			m.RegisterOnPlayerJoin(NotifyAboutReports);
			m.RegisterCommandHelp("report", "Allows you to report other players to server staff");
			m.RegisterPrivilege("season");
			m.RegisterOnCommand(ChangeSeason);
			m.RegisterCommandHelp("season", "Allows you to change the current season");
			m.RegisterPrivilege("afk");
			m.RegisterOnCommand(SendAFK);
			m.RegisterCommandHelp("afk", "/afk [minutes] [reason]");
			m.RegisterPrivilege("btk");
			m.RegisterOnCommand(SendBTK);
			m.RegisterCommandHelp("btk", "/btk");
			m.RegisterOnCommand(DisplayUptime);
			m.RegisterTimer(Uptime_Tick, (double)1);
			m.RegisterCommandHelp("uptime", "Displays the time the server has been running");
			m.RegisterPrivilege("me");
			m.RegisterOnCommand(SendMe);
			m.RegisterCommandHelp("me", "/me [message]");
			m.RegisterPrivilege("vanish");
			m.RegisterOnCommand(SetSpectate);
			m.RegisterCommandHelp("vanish", "/vanish - Poof! You're gone.");
			m.RegisterPrivilege("notification");
			m.RegisterOnCommand(ToggleNotifications);
			m.RegisterCommandHelp("notification", "/notification [vanish/warn].");
			m.RegisterPrivilege("language");
			m.RegisterOnCommand(ChangeLanguage);
			m.RegisterCommandHelp("language", "/language [DE/EN].");
			m.RegisterOnCommand(DisplayVersion);
			m.RegisterPrivilege("mute");
			m.RegisterOnCommand(ChangeMuteStatus);
			m.RegisterCommandHelp("mute", "/mute [playername] or /unmute [playername].");
			m.RegisterOnPlayerChat(CheckMute);
			m.RegisterPrivilege("spectate");
			m.RegisterOnCommand(ToggleSpectate);
			m.RegisterCommandHelp("spectate", "Spectate a player. Use /stopspectate to stop.");
			m.RegisterOnCommand(SpawnCommand);
			m.RegisterCommandHelp("spawn", "Respawn at your default spawn.");
			m.RegisterPrivilege("su_reload");
			m.RegisterOnCommand(ReloadConfig);
			m.RegisterCommandHelp("su_reload", "Reloads ServerUtilities configuration.");
			m.RegisterOnCommand(SetNameColor);
			m.RegisterCommandHelp("namecolor", "Changes player name color");
			m.RegisterOnPlayerJoin(AutoColorName);

			//Load mod settings
			LoadSettings();
			System.Console.WriteLine(string.Format("[ServerUtilities] Loaded Mod Version {0}", versionNumber));
			m.LogServerEvent(string.Format("[ServerUtilities] Loaded Mod Version {0}", versionNumber));
		}
		//Enter the desired language code here. Currently supported are EN and DE.
		string languageCode = "EN";
		//Enable or disable all notifications about usage of "pull" command.
		bool AdminNotification_Pull = true;
		//The permission level of the player group which shall be notified. (includes all groups above)
		int AdminNotification_Pull_PlayerGroup = 3; //Default: 3 - Admin rank (in standard configuration)
													//Enable global warnings (Sends all players the message: Player X has been warned by Y for Z)
		bool GlobalNotification_Warn = true;
		//Notify every user having this (or a higher) permission level when a user is reported
		int Report_PlayerGroup = 3; //Default: 3 - Admin rank (in standard configuration)
									//Global notifications when a player vanishes/gets visible
		bool GlobalNotification_Vanish = true;
		//The interval between server restarts (in hours) - dynamically read from server configuration
		int iRestartInterval;

		//Internal variables.
		//DO NOT CHANGE!
		ModManager m;
		string sModDir = "UserData" + Path.DirectorySeparatorChar + "ServerUtilities";
		int UptimeSeconds = 0;
		string versionNumber = "1.3"; //TODO: Update to next release
		string chatPrefix = "&8[&6ServerUtilities&8] ";

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

		string GetLocalizedString(int StringID)
		{
			switch (languageCode)
			{
				#region German translation
				case "DE":
					switch (StringID)
					{
						case 0:
							return "Du darfst dich nicht teleportieren.";
						case 1:
							return "Argument ist keine Zahl.";
						case 2:
							return "Die Zahl ist zu groß für einen Int32.";
						case 3:
							return "&aTeleport erfolgreich.";
						case 4:
							return "Spieler ID {0} existiert nicht.";
						case 5:
							return "Du darfst andere Spieler nicht zu dir teleportieren.";
						case 6:
							return "&7{0} hat dich zu sich teleportiert.";
						case 7:
							return "&7{0} hat {1} zu sich teleportiert.";
						case 8:
							return "Dein Ziel hat einen höheren Rang als du.";
						case 9:
							return "Der Spieler {0} existiert nicht.";
						case 10:
							return "&7Noch &6{0} &7Stunden bis{1}";
						case 11:
							return "&7Noch &6{0} &7Stunde bis{1}";
						case 12:
							return "&7Noch &6{0} &7Minuten bis{1}";
						case 13:
							return "&7Noch &6{0} &7Minute bis{1}";
						case 14:
							return "&7Noch &6{0} &7Sekunden bis{1}";
						case 15:
							return "&7Noch &6{0} &7Sekunde bis{1}";
						case 16:
							return "Du darfst keinen Countdown starten.";
						case 17:
							return "Ungültige Argumente. Hilfe unter /help";
						case 18:
							return "&aCountdown erfolgreich gesetzt.";
						case 19:
							return "Nur ein Countdown kann aktiv sein.";
						case 20:
							return "Du darfst nicht Kopieren/Einfügen.";
						case 21:
							return "&aKopier-Anfang auf {0}, {1}, {2} gesetzt.";
						case 22:
							return "&aKopier-Ende auf {0}, {1}, {2} gesetzt.";
						case 23:
							return "Erlaubt sind 'start' und 'end'";
						case 24:
							return "Abstimmung:&7{0}";
						case 25:
							return "{0} Minuten verbleiben.";
						case 26:
							return "{0} Minute verbleibt.";
						case 27:
							return "&7Abstimmung abgeschlossen.";
						case 28:
							return "Frage:&7{0}";
						case 29:
							return "Gestartet von: &7{0}";
						case 30:
							return "Statistik:";
						case 31:
							return "{0} Stimmen &2DAFÜR&8.";
						case 32:
							return "{0} Stimmen &4DAGEGEN&8.";
						case 33:
							return "&2Abstimmung erfolgreich!";
						case 34:
							return "&6Gleichstand...";
						case 35:
							return "&4Abstimmung fehlgeschlagen!";
						case 36:
							return "Ergebnis: {0}";
						case 37:
							return "Du hast kein Stimmrecht!";
						case 38:
							return "Es läuft derzeit keine Abstimmung.";
						case 39:
							return "Du hast bereits eine Stimme abgegeben!";
						case 40:
							return "&2Vielen Dank, deine Stimme wurde gezählt.";
						case 41:
							return "&7ABSTIMMUNGEN";
						case 42:
							return "&7--------------------------------------------------";
						case 43:
							return "&7Wenn eine Abstimmung läuft, kannst du:";
						case 44:
							return "&7-&6/vote yes &feingeben, um &2DAFÜR &fzu stimmen.";
						case 45:
							return "&7-&6/vote no &feingeben, um &4DAGEGEN &fzu stimmen.";
						case 46:
							return "&7-&6/vote info &feingeben, um den aktuellen Stand zu sehen.";
						case 47:
							return "Du darfst keine Abstimmungen starten.";
						case 48:
							return "Es läuft bereits eine Abstimmung.";
						case 49:
							return "&7Der Server gibt die Abstimmung bekannt.";
						case 50:
							return "&7Das kann bis zu einer Minute dauern.";
						case 51:
							return "&7Abbruch mit &6/vote cancel";
						case 52:
							return "Du darfst keine Abstimmungen abbrechen.";
						case 53:
							return "&7Die aktuelle Abstimung wurde von {0} abgebrochen.";
						case 54:
							return "Du darfst andere Spieler nicht verwarnen.";
						case 55:
							return "{0} &7wurde von &8{1} &4gewarnt&f. &7Grund:{2}";
						case 56:
							return "{0}&7, du wurdest von {1} &4gewarnt. &7Grund:{2}";
						case 57:
							return "Du darfst andere Spieler nicht melden.";
						case 58:
							return "&7{0}: &8{1} &7hat &8{2} &4gemeldet. &7Grund:{3}";
						case 59:
							return "&aDu hast {0} erfolgreich gemeldet. &7Grund:{1}";
						case 60:
							return "Du darfst die aktuelle Jahreszeit nicht verändern.";
						case 61:
							return "Jahreszeit ID {0} ist ungültig.";
						case 62:
							return "Start und Ende des Kopierbereichs nicht von derselben Person festgelegt.";
						case 63:
							return "Start oder Ende des Kopierbereichs nicht von DIR festgelegt.";
						case 64:
							return "Du darfst anderen nicht sagen, dass du AFK bist.";
						case 65:
							return "&7{0} ist für {1} Minuten AFK. Grund:{2}";
						case 66:
							return "&7{0} Tage, {1} Stunden, {2} Minuten, {3} Sekunden.";
						case 67:
							return "&7{0} ist wieder da.";
						case 68:
							return "Du darfst /me nicht verwenden.";
						case 69:
							return "Du darfst dich nicht unsichtbar machen.";
						case 70:
							return "&7{0} ist verschwunden.";
						case 71:
							return "&7{0} ist wieder aufgetaucht.";
						case 72:
							return "&7Du bist verschwunden!";
						case 73:
							return "&7Du bist wieder sichtbar.";
						case 74:
							return "Du darfst die Benachrichtigungsoptionen nicht ändern.";
						case 75:
							return "&7Globale Vanish-Benachrichtigungen: &cAUS";
						case 76:
							return "&7Globale Vanish-Benachrichtigungen: &aAN";
						case 77:
							return "&7Globale Verwarnungen: &cAUS";
						case 78:
							return "&7Globale Verwarnungen: &aAN";
						case 79:
							return "Du darfst die Sprache nicht ändern.";
						case 80:
							return "&7Sprache geändert zu: &eDE - Deutsch";
						case 81:
							return "Du darfst andere Spieler nicht stummschalten.";
						case 82:
							return "&7{0} wurde von {1} stummgeschaltet.";
						case 83:
							return "&7{0}'s Stummschaltung wurde von {1} aufgehoben.";
						case 84:
							return "{0} ist nicht stummgeschaltet.";
						case 85:
							return "Du wurdest stummgeschaltet.";
						case 86:
							return "Du darfst anderen Spielern nicht zuschauen!";
						case 87:
							return "Start oder Ende des Kopierbereichs nicht festgelegt.";
						case 88:
							return "{0} ist bereits stummgeschaltet.";
						case 89:
							return "&7{0} hat den Countdown abgebrochen.";
						case 90:
							return "Es läuft gerade kein Countdown.";
						case 91:
							return "&7Kopiervorgang gestartet. Lag möglich.";
						case 92:
							return "&7Kopiervorgang abgeschlossen.";
						case 93:
							return "&7{0} hat die Jahreszeit geändert.";
						case 94:
							return "Die Sprache {0} ist nicht unterstützt. Siehe /help.";
						case 95:
							return "&aRespawn erfolgreich.";
						case 96:
							return "Du darfst die Konfiguration nicht neu laden!";
						case 97:
							return "&aKonfiguration erfolgreich neu geladen.";
						case 98:
							return "Du darfst deine Namensfarbe nicht ändern!";
						case 99:
							return "&aNamensfarbe erfolgreich geändert.";

						default:
							return string.Format("&4FEHLER: &fString ID {0} existiert nicht.", StringID.ToString());
					}
				#endregion

				#region English translation
				case "EN":
					switch (StringID)
					{
						case 0:
							return "You are not allowed to teleport yourself.";
						case 1:
							return "Argument is not a number.";
						case 2:
							return "The number is too big for an Int32.";
						case 3:
							return "&aTeleport successful.";
						case 4:
							return "Player ID {0} does not exist.";
						case 5:
							return "You are not allowed to pull other players.";
						case 6:
							return "&7{0} pulled you to his position.";
						case 7:
							return "&7{0} pulled {1} to his position.";
						case 8:
							return "Your target has a higher permission level than you.";
						case 9:
							return "Player {0} does not exist.";
						case 10:
							return "&6{0} &7hours left until{1}";
						case 11:
							return "&6{0} &7hour left until{1}";
						case 12:
							return "&6{0} &7minutes left until{1}";
						case 13:
							return "&6{0} &7minute left until{1}";
						case 14:
							return "&6{0} &7seconds left until{1}";
						case 15:
							return "&6{0} &7second left until{1}";
						case 16:
							return "You are not allowed to start a countdown.";
						case 17:
							return "Invalid arguments. Try /help";
						case 18:
							return "&aCountdown successfully set.";
						case 19:
							return "Only one countdown can be active at a time.";
						case 20:
							return "You are not allowed to use copy/paste.";
						case 21:
							return "&aCopy Start set to {0}, {1}, {2}.";
						case 22:
							return "&aCopy End set to {0}, {1}, {2}.";
						case 23:
							return "Use 'start' and 'end' to define an area.";
						case 24:
							return "Current voting:&7{0}";
						case 25:
							return "{0} minutes left.";
						case 26:
							return "{0} minute left.";
						case 27:
							return "&7Voting closed.";
						case 28:
							return "Topic:&7{0}";
						case 29:
							return "Started by: &7{0}";
						case 30:
							return "Statistics:";
						case 31:
							return "{0} votes &2FOR &8the subject.";
						case 32:
							return "{0} votes &4AGAINST &8the subject.";
						case 33:
							return "&2Vote passed!";
						case 34:
							return "&6Votes are even!";
						case 35:
							return "&4Vote failed!";
						case 36:
							return "Result: {0}";
						case 37:
							return "You are not allowed to vote.";
						case 38:
							return "There's currently no vote in progress.";
						case 39:
							return "You have already voted!";
						case 40:
							return "&2Thank You, your vote has been counted.";
						case 41:
							return "&7VOTING";
						case 42:
							return "&7--------------------------------------------------";
						case 43:
							return "&7Once a player has started a vote you can:";
						case 44:
							return "&7-type &6/vote yes &fto vote &2FOR &fthe topic.";
						case 45:
							return "&7-type &6/vote no &fto vote &4AGAINST &fthe topic.";
						case 46:
							return "&7-type &6/vote info &fto view the current statistics.";
						case 47:
							return "You are not allowed to start votings.";
						case 48:
							return "There is already a vote in progress.";
						case 49:
							return "&7Announcement will take a few moments.";
						case 50:
							return "&7This can take up to 1 minute.";
						case 51:
							return "&7Cancel the voting using &6/vote cancel";
						case 52:
							return "You are not allowed to cancel votings.";
						case 53:
							return "&7Current voting was cancelled by {0}.";
						case 54:
							return "You are not allowed to warn other players!";
						case 55:
							return "{0} &7has been &4warned &7by &8{1}. &7Reason:{2}";
						case 56:
							return "{0}&7, you have been &4warned &7by &8{1}. &7Reason:{2}";
						case 57:
							return "You are not allowed to report other players.";
						case 58:
							return "&7{0}: &8{1} &4reported &8{2}. &7Reason:{3}";
						case 59:
							return "&aYou have successfully reported {0}. &7Reason:{1}";
						case 60:
							return "You are not allowed to change the current season!";
						case 61:
							return "Season ID {0} is not a valid season.";
						case 62:
							return "Copy Start and End not set by same person.";
						case 63:
							return "Copy Start or End not set by YOU.";
						case 64:
							return "You are not allowed to notify others that you're AFK.";
						case 65:
							return "&7{0} is AFK for {1} minutes. Reason:{2}";
						case 66:
							return "&7{0} Days, {1} Hours, {2} Minutes, {3} Seconds.";
						case 67:
							return "&7{0} is back.";
						case 68:
							return "You are not allowed to use /me.";
						case 69:
							return "You are not allowed to vanish.";
						case 70:
							return "&7{0} vanished. Poof!";
						case 71:
							return "&7{0} reappeared.";
						case 72:
							return "&7You have vanished. Poof!";
						case 73:
							return "&7You are visible again.";
						case 74:
							return "You are not allowed to change notification options.";
						case 75:
							return "&7Global Vanish Messages: &cOFF";
						case 76:
							return "&7Global Vanish Messages: &aON";
						case 77:
							return "&7Global Warnings: &cOFF";
						case 78:
							return "&7Global Warnings: &aON";
						case 79:
							return "You are not allowed to change the language.";
						case 80:
							return "&7Language changed to: &eEN - English";
						case 81:
							return "You are not allowed to mute other players.";
						case 82:
							return "&7{0} has been muted by {1}.";
						case 83:
							return "&7{0} has been un-muted by {1}.";
						case 84:
							return "{0} isn't muted.";
						case 85:
							return "You have been muted.";
						case 86:
							return "You are not permitted to spectate other players!";
						case 87:
							return "Copy Start or End not set.";
						case 88:
							return "{0} is already muted.";
						case 89:
							return "&7{0} cancelled the current countdown.";
						case 90:
							return "There is no countdown running.";
						case 91:
							return "&7Copy process started. Lag is likely.";
						case 92:
							return "&7Copy process finished.";
						case 93:
							return "&7{0} changed the season.";
						case 94:
							return "The language {0} not supported. See /help.";
						case 95:
							return "&aRespawn successful.";
						case 96:
							return "You are not permitted to reload the configuration.";
						case 97:
							return "&aSuccessfully reloaded configuration.";
						case 98:
							return "You are not allowed to change your name color!";
						case 99:
							return "&aSuccessfully changed your name color.";

						default:
							return string.Format("&4ERROR: &fString ID {0} does not exist.", StringID.ToString());
					}
				#endregion

				default:
					System.Console.WriteLine(string.Format("ERROR: Language code {0} not found!", languageCode));
					return string.Format("&4ERROR: &fThe language code {0} is not in the list.", languageCode);
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
					sw.WriteLine(AdminNotification_Pull);
					sw.WriteLine(AdminNotification_Pull_PlayerGroup);
					sw.WriteLine(GlobalNotification_Warn);
					sw.WriteLine(Report_PlayerGroup);
					sw.WriteLine(GlobalNotification_Vanish);
				}
				System.Console.WriteLine("[ServerUtilities] Settings saved.");
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
			}
		}

		void LoadSettings()
		{
			if (!Directory.Exists(sModDir))
			{
				//Directory doesn't exist. Create new.
				System.Console.WriteLine("[ServerUtilities] Mod directory not found. Creating new.");
				Directory.CreateDirectory(sModDir);
			}
			if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "settings.txt"))
			{
				//No settings file found. Create new.
				System.Console.WriteLine("[ServerUtilities] 'settings.txt' not found. Creating new.");
				SaveSettings();
				return;
			}
			//Else use existing config
			try
			{
				using (TextReader tr = new StreamReader(sModDir + Path.DirectorySeparatorChar + "settings.txt", Encoding.UTF8))
				{
					languageCode = tr.ReadLine();
					AdminNotification_Pull = bool.Parse(tr.ReadLine());
					AdminNotification_Pull_PlayerGroup = int.Parse(tr.ReadLine());
					GlobalNotification_Warn = bool.Parse(tr.ReadLine());
					Report_PlayerGroup = int.Parse(tr.ReadLine());
					GlobalNotification_Vanish = bool.Parse(tr.ReadLine());
					System.Console.WriteLine("[ServerUtilities] Loaded settings.");
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
				System.Console.WriteLine("[ServerUtilities] Saving settings in correct format...");
				SaveSettings();
			}
		}

		/// <summary>
		/// Get a certain player's position by ID
		/// </summary>
		/// <param name="PlayerID">ID of the player</param>
		/// <returns>Vector3i containing the player position</returns>
		Vector3i GetPlayerPosition(int PlayerID)
		{
			return new Vector3i((int)m.GetPlayerPositionX(PlayerID), (int)m.GetPlayerPositionY(PlayerID), (int)m.GetPlayerPositionZ(PlayerID));
		}

		/// <summary>
		/// Gets the ID of a given player name (not case-sensitive).
		/// </summary>
		/// <param name="playerName">player name to find ID for</param>
		/// <returns>ID of the player. -1 if player is not found</returns>
		int GetPlayerID(string playerName)
		{
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

		void NotifyAboutPull(string action)
		{
			if (AdminNotification_Pull)
			{
				int[] playerlist = m.AllPlayers();
				for (int i = 0; i < playerlist.Length; i++)
				{
					if (m.GetPlayerPermissionLevel(playerlist[i]) >= AdminNotification_Pull_PlayerGroup)
					{
						m.SendMessage(playerlist[i], chatPrefix + action);
					}
				}
			}
			System.Console.WriteLine(string.Format("INFO:  {0}", action));
		}

		bool TeleportToID(int player, string command, string argument)
		{
			if (command.Equals("tp_id", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "tp"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(0));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to teleport by ID (no permission)", m.GetPlayerName(player)));
					return true;
				}
				int targetID;
				try
				{
					targetID = Convert.ToInt32(argument);
				}
				catch (FormatException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(1));
					return true;
				}
				catch (OverflowException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(2));
					return true;
				}
				//Check if playerID exists
				int[] playerlist = m.AllPlayers();
				bool ID_in_Playerlist = false;
				for (int i = 0; i < playerlist.Length; i++)
				{
					if (playerlist[i] == targetID)
					{
						ID_in_Playerlist = true;
						break;
					}
				}
				if (ID_in_Playerlist)
				{
					//Do the teleport magic
					m.SetPlayerPosition(player, m.GetPlayerPositionX(targetID), m.GetPlayerPositionY(targetID), m.GetPlayerPositionZ(targetID));
					m.SendMessage(player, chatPrefix + GetLocalizedString(3));
					return true;
				}
				else
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(4), targetID.ToString()));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to teleport by ID (target does not exist)", m.GetPlayerName(player)));
					return true;
				}
			}
			return false;
		}

		bool PullPlayer(int player, string command, string argument)
		{
			if ((command.Equals("pull", StringComparison.InvariantCultureIgnoreCase)) || (command.Equals("p", StringComparison.InvariantCultureIgnoreCase)))
			{
				if (!m.PlayerHasPrivilege(player, "pull"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(5));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to pull someone (no permission)", m.GetPlayerName(player)));
					return true;
				}
				int target = GetPlayerID(argument);
				if (target != -1)
				{
					if (m.GetPlayerPermissionLevel(player) >= m.GetPlayerPermissionLevel(target))
					{
						//Do the teleport magic
						m.SetPlayerPosition(target, m.GetPlayerPositionX(player), m.GetPlayerPositionY(player), m.GetPlayerPositionZ(player));
						m.SendMessage(player, chatPrefix + GetLocalizedString(3));
						m.SendMessage(target, chatPrefix + string.Format(GetLocalizedString(6), m.GetPlayerName(player)));
						//Notify the defined group (abuse prevention)
						NotifyAboutPull(string.Format(GetLocalizedString(7), m.GetPlayerName(player), m.GetPlayerName(target)));
						return true;
					}
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(8));
					return true;
				}
				else
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(9), argument));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to pull someone (target does not exist)", m.GetPlayerName(player)));
					return true;
				}
			}
			return false;
		}

		bool PullPlayerID(int player, string command, string argument)
		{
			if ((command.Equals("pull_id", StringComparison.InvariantCultureIgnoreCase)) || (command.Equals("p_id", StringComparison.InvariantCultureIgnoreCase)))
			{
				if (!m.PlayerHasPrivilege(player, "pull"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(5));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to pull someone by ID (no permission)", m.GetPlayerName(player)));
					return true;
				}
				int targetID;
				try
				{
					targetID = Convert.ToInt32(argument);
				}
				catch (FormatException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(1));
					return true;
				}
				catch (OverflowException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(2));
					return true;
				}
				//Check if playerID exists
				int[] playerlist = m.AllPlayers();
				bool ID_in_Playerlist = false;
				for (int i = 0; i < playerlist.Length; i++)
				{
					if (playerlist[i] == targetID)
					{
						ID_in_Playerlist = true;
						break;
					}
				}
				if (ID_in_Playerlist)
				{
					if (m.GetPlayerPermissionLevel(player) >= m.GetPlayerPermissionLevel(targetID))
					{
						//Do the teleport magic
						m.SetPlayerPosition(targetID, m.GetPlayerPositionX(player), m.GetPlayerPositionY(player), m.GetPlayerPositionZ(player));
						m.SendMessage(player, chatPrefix + GetLocalizedString(3));
						m.SendMessage(targetID, chatPrefix + string.Format(GetLocalizedString(6), m.GetPlayerName(player)));
						//Notify the defined group (abuse prevention)
						NotifyAboutPull(string.Format(GetLocalizedString(7), m.GetPlayerName(player), m.GetPlayerName(targetID)));
						return true;
					}
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(8));
					return true;
				}
				else
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(4), targetID.ToString()));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to pull someone by ID (target does not exist)", m.GetPlayerName(player)));
					return true;
				}
			}
			return false;
		}

		bool CountdownIsActive = false;
		int CountdownCurrent = -1;
		string CountdownReason = "";

		void Countdown_Tick()
		{
			if (CountdownIsActive)
			{
				if (CountdownCurrent > 3600)
				{
					if ((CountdownCurrent % 3600) == 0)
					{
						if ((CountdownCurrent / 3600) == 1)
							m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(11), (int)(CountdownCurrent / 60), CountdownReason));
						else
							m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(10), (int)(CountdownCurrent / 60), CountdownReason));
						CountdownCurrent--;
						return;
					}
					CountdownCurrent--;
					return;
				}
				else if (CountdownCurrent > 60)
				{
					if ((CountdownCurrent % 60) == 0)
					{
						if ((CountdownCurrent / 60) == 1)
							m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(13), (int)(CountdownCurrent / 60), CountdownReason));
						else
							m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(12), (int)(CountdownCurrent / 60), CountdownReason));
						CountdownCurrent--;
						return;
					}
					CountdownCurrent--;
					return;
				}
				else if (CountdownCurrent > 10)
				{
					if ((CountdownCurrent % 10) == 0)
					{
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(14), (int)(CountdownCurrent), CountdownReason));
						CountdownCurrent--;
						return;
					}
					CountdownCurrent--;
					return;
				}
				else if (CountdownCurrent > 0)
				{
					if (CountdownCurrent == 1)
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(15), (int)(CountdownCurrent), CountdownReason));
					else
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(14), (int)(CountdownCurrent), CountdownReason));
					CountdownCurrent--;
					return;
				}
				else
				{
					m.SendMessageToAll(" ");
					m.SendMessageToAll(chatPrefix + string.Format("&7{0}", CountdownReason));
					CountdownIsActive = false;
					CountdownCurrent = -1;
					CountdownReason = "";
				}
			}
		}

		bool Countdown_Start(int player, string command, string argument)
		{
			if ((command.Equals("countdown", StringComparison.InvariantCultureIgnoreCase)) || (command.Equals("count", StringComparison.InvariantCultureIgnoreCase)))
			{
				if (!m.PlayerHasPrivilege(player, "countdown"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(16));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to start a countdown (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("abort", StringComparison.InvariantCultureIgnoreCase))
				{
					if (CountdownIsActive)
					{
						CountdownIsActive = false;
						CountdownCurrent = -1;
						CountdownReason = "";
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(89), m.GetPlayerName(player)));
					}
					else
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(90));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to abort the current countdown (no countdown active)", m.GetPlayerName(player)));
					}
					return true;
				}
				if (!CountdownIsActive)
				{
					string givenReason = "";
					int givenTime;
					try
					{
						string[] args = argument.Split(' ');
						givenTime = int.Parse(args[0]);
						for (int i = 1; i < args.Length; i++)
						{
							givenReason = givenReason + " " + args[i];
						}
					}
					catch
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
						return true;
					}
					if (string.IsNullOrEmpty(givenReason))
					{
						//Invalid (or missing) argument(s)
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
						return true;
					}
					CountdownCurrent = givenTime;
					CountdownReason = givenReason;
					CountdownIsActive = true;

					System.Console.WriteLine(string.Format("[ServerUtilities] {0} started a countdown.", m.GetPlayerName(player)));
					m.SendMessage(player, chatPrefix + GetLocalizedString(18));
					return true;
				}
				else
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(19));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to start a countdown (countdown already running)", m.GetPlayerName(player)));
					return true;
				}
			}
			return false;
		}

		bool DisplayColors(int player, string command, string argument)
		{
			if (command.Equals("colors", StringComparison.InvariantCultureIgnoreCase))
			{
				m.SendMessage(player, chatPrefix + "&00 &11 &22 &33 &44 &55 &66 &77");
				m.SendMessage(player, chatPrefix + "&88 &99 &aa &bb &cc &dd &ee &ff");
				return true;
			}
			return false;
		}

		Vector3i copy_start;
		Vector3i copy_end;
		Vector3i copy_paste;
		string start_set_by = "";
		string end_set_by = "";

		bool CopyPaste(int player, string command, string argument)
		{
			bool copy_deleteSource = false;

			if (command.Equals("copy", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "copypaste"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(20));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use copy (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("start", StringComparison.InvariantCultureIgnoreCase))
				{
					copy_start = GetPlayerPosition(player);
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(21), copy_start.x, copy_start.y, copy_start.z));
					m.SendMessageToAll(chatPrefix + string.Format("&7{0}: Copy Start", m.GetPlayerName(player)));
					start_set_by = m.GetPlayerName(player);
					return true;
				}
				if (argument.Equals("end", StringComparison.InvariantCultureIgnoreCase))
				{
					copy_end = GetPlayerPosition(player);
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(22), copy_end.x, copy_end.y, copy_end.z));
					m.SendMessageToAll(chatPrefix + string.Format("&7{0}: Copy End", m.GetPlayerName(player)));
					end_set_by = m.GetPlayerName(player);
					return true;
				}
				m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(23));
				return true;
			}
			if (command.Equals("paste", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "copypaste"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(20));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use paste (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (argument.Equals("x", StringComparison.InvariantCultureIgnoreCase))
				{
					copy_deleteSource = true;
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} set crop mode option.", m.GetPlayerName(player)));
				}

				if (string.IsNullOrEmpty(start_set_by) || string.IsNullOrEmpty(end_set_by))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(87));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use paste (start/end not set)", m.GetPlayerName(player)));
					return true;
				}
				if (!start_set_by.Equals(end_set_by, StringComparison.InvariantCultureIgnoreCase))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(62));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use paste (start/end not set by same player)", m.GetPlayerName(player)));
					return true;
				}
				if ((!start_set_by.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase)) || (!end_set_by.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase)))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(63));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use paste (start/end not set by this player)", m.GetPlayerName(player)));
					return true;
				}

				System.Console.WriteLine(string.Format("[ServerUtilities] {0} started copy process.", m.GetPlayerName(player)));
				m.SendMessageToAll(chatPrefix + GetLocalizedString(91));

				Vector3i start, end;
				copy_paste = GetPlayerPosition(player);

				if (copy_start.x < copy_end.x)
				{
					start.x = copy_start.x;
					end.x = copy_end.x;
				}
				else
				{
					start.x = copy_end.x;
					end.x = copy_start.x;
				}
				if (copy_start.y < copy_end.y)
				{
					start.y = copy_start.y;
					end.y = copy_end.y;
				}
				else
				{
					start.y = copy_end.y;
					end.y = copy_start.y;
				}
				if (copy_start.z < copy_end.z)
				{
					start.z = copy_start.z;
					end.z = copy_end.z;
				}
				else
				{
					start.z = copy_end.z;
					end.z = copy_start.z;
				}

				if (copy_deleteSource)
				{
					for (int x = start.x; x <= end.x; x++)
					{
						for (int y = start.y; y <= end.y; y++)
						{
							for (int z = start.z; z <= end.z; z++)
							{
								m.SetBlock(copy_paste.x + (x - start.x), copy_paste.y + (y - start.y), copy_paste.z + (z - start.z), m.GetBlock(x, y, z));
								//If crop mode is active set source blocks to "Empty"
								m.SetBlock(x, y, z, 0);
							}
						}
					}
				}
				else
				{
					for (int x = start.x; x <= end.x; x++)
					{
						for (int y = start.y; y <= end.y; y++)
						{
							for (int z = start.z; z <= end.z; z++)
							{
								m.SetBlock(copy_paste.x + (x - start.x), copy_paste.y + (y - start.y), copy_paste.z + (z - start.z), m.GetBlock(x, y, z));
							}
						}
					}
				}
				System.Console.WriteLine("[ServerUtilities] Copying finished");
				m.SendMessageToAll(chatPrefix + GetLocalizedString(92));
				return true;
			}
			return false;
		}

		bool VoteInProgress = false;
		int VoteTime, VoteFor, VoteAgainst = 0;
		string VoteSubject, VoteStartedBy, VoteResult = "";
		List<string> voters = new List<string>();

		void Vote_Tick()
		{
			if (VoteInProgress)
			{
				if (VoteTime > 0)
				{
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(24), VoteSubject));
					if (VoteTime == 1)
					{
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(26), VoteTime.ToString()));
					}
					else
					{
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(25), VoteTime.ToString()));
					}
					VoteTime--;
				}
				else
				{
					m.SendMessageToAll(chatPrefix + GetLocalizedString(27));
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(28), VoteSubject));
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(29), VoteStartedBy));
					m.SendMessageToAll(" ");
					m.SendMessageToAll(chatPrefix + GetLocalizedString(30));
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(31), VoteFor.ToString()));
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(32), VoteAgainst.ToString()));
					if (VoteFor > VoteAgainst)
					{
						VoteResult = GetLocalizedString(33);
					}
					else if (VoteFor == VoteAgainst)
					{
						VoteResult = GetLocalizedString(34);
					}
					else
					{
						VoteResult = GetLocalizedString(35);
					}
					m.SendMessageToAll(" ");
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(36), VoteResult));
					try
					{
						//Save to log file
						if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "VoteLog.txt"))
						{
							File.Create(sModDir + Path.DirectorySeparatorChar + "VoteLog.txt").Close();
							System.Console.WriteLine("[ServerUtilities] VoteLog.txt created!");
						}
						using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "VoteLog.txt", true))
						{
							sw.WriteLine(string.Format("Subject: {0}", VoteSubject));
							sw.WriteLine(string.Format("Started by: {0}", VoteStartedBy));
							sw.WriteLine(string.Format("{0} agreed - {1} disagreed", VoteFor, VoteAgainst));
							sw.WriteLine(string.Format("ENDED: {0}", System.DateTime.UtcNow.ToString()));
							sw.WriteLine("----------");
							System.Console.WriteLine("[ServerUtilities] Vote finished. Result saved to VoteLog.txt");
						}
					}
					catch (Exception ex)
					{
						System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
					}
					//Reset state of the voting system
					VoteSubject = "";
					VoteStartedBy = "";
					VoteResult = "";
					VoteFor = 0;
					VoteAgainst = 0;
					voters.Clear(); //Reset list of people who have voted
					VoteInProgress = false;
				}
			}
		}

		bool Vote(int player, string command, string argument)
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
				m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
				return true;
			}

			if (command.Equals("vote", StringComparison.InvariantCultureIgnoreCase))
			{
				if (option.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "vote"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(37));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to vote (no permission)", m.GetPlayerName(player)));
						return true;
					}
					if (!VoteInProgress)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(38));
						return true;
					}
					foreach (string current in voters)
					{
						if (current.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
						{
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(39));
							return true;
						}
					}
					VoteFor++;
					voters.Add(m.GetPlayerName(player));
					m.SendMessage(player, chatPrefix + GetLocalizedString(40));
					return true;
				}
				if (option.Equals("no", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "vote"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(37));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to vote (no permission)", m.GetPlayerName(player)));
						return true;
					}
					if (!VoteInProgress)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(38));
						return true;
					}
					foreach (string current in voters)
					{
						if (current.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
						{
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(39));
							return true;
						}
					}
					VoteAgainst++;
					voters.Add(m.GetPlayerName(player));
					m.SendMessage(player, chatPrefix + GetLocalizedString(40));
					return true;
				}
				if (option.Equals("help", StringComparison.InvariantCultureIgnoreCase))
				{
					m.SendMessage(player, chatPrefix + GetLocalizedString(41));
					m.SendMessage(player, chatPrefix + GetLocalizedString(42));
					m.SendMessage(player, chatPrefix + GetLocalizedString(43));
					m.SendMessage(player, chatPrefix + GetLocalizedString(44));
					m.SendMessage(player, chatPrefix + GetLocalizedString(45));
					m.SendMessage(player, chatPrefix + GetLocalizedString(46));
					return true;
				}
				if (option.Equals("info", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!VoteInProgress)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(38));
						return true;
					}
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(28), VoteSubject));
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(29), VoteStartedBy));
					m.SendMessage(player, " ");
					m.SendMessage(player, chatPrefix + GetLocalizedString(30));
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(31), VoteFor.ToString()));
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(32), VoteAgainst.ToString()));
					return true;
				}
				if (option.Equals("start", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "startvote"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(47));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to start a vote (no permission)", m.GetPlayerName(player)));
						return true;
					}
					if (VoteInProgress)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(48));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to start a vote (there is already a vote in progress)", m.GetPlayerName(player)));
						return true;
					}
					try
					{
						VoteTime = int.Parse(args[1]);
						for (int i = 2; i < args.Length; i++)
						{
							VoteSubject = VoteSubject + " " + args[i];
						}
					}
					catch
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
						return true;
					}
					VoteInProgress = true;
					VoteStartedBy = m.GetPlayerName(player);
					m.SendMessage(player, chatPrefix + GetLocalizedString(49));
					m.SendMessage(player, chatPrefix + GetLocalizedString(50));
					m.SendMessage(player, chatPrefix + GetLocalizedString(51));
					return true;
				}
				if (option.Equals("cancel", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "startvote"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(52));
						return true;
					}
					if (!VoteInProgress)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(38));
						return true;
					}
					try
					{
						//Save cancellation to log file
						if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "VoteLog.txt"))
						{
							File.Create(sModDir + Path.DirectorySeparatorChar + "VoteLog.txt").Close();
							System.Console.WriteLine("[ServerUtilities] VoteLog.txt created!");
						}
						using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "VoteLog.txt", true))
						{
							sw.WriteLine(string.Format("Subject: {0}", VoteSubject));
							sw.WriteLine(string.Format("Started by: {0}", VoteStartedBy));
							sw.WriteLine(string.Format("{0} agreed - {1} disagreed", VoteFor, VoteAgainst));
							sw.WriteLine(string.Format("CANCELLED: {0} by {1}", System.DateTime.UtcNow.ToString(), m.GetPlayerName(player)));
							sw.WriteLine("----------");
							System.Console.WriteLine("[ServerUtilities] Vote CANCELLED. Result saved to VoteLog.txt");
						}
					}
					catch (Exception ex)
					{
						System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
					}
					//Reset state of the voting system
					VoteInProgress = false;
					VoteSubject = "";
					VoteStartedBy = "";
					VoteResult = "";
					VoteFor = 0;
					VoteAgainst = 0;
					VoteTime = 0;
					voters.Clear(); //Reset list of people who have voted
					m.SendMessageToAll(string.Format(chatPrefix + GetLocalizedString(53), m.GetPlayerName(player)));
					return true;
				}
				m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
				return true;
			}
			return false;
		}

		bool WarnPlayer(int player, string command, string argument)
		{
			if (command.Equals("warn", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "warn"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(54));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to warn someone (no permission)", m.GetPlayerName(player)));
					return true;
				}
				string[] args;
				string warnplayer = "";
				string givenReason = "";
				try
				{
					args = argument.Split(' ');
					warnplayer = args[0];
					for (int i = 1; i < args.Length; i++)
					{
						givenReason = givenReason + " " + args[i];
					}
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
					return true;
				}
				if (string.IsNullOrEmpty(warnplayer) || string.IsNullOrEmpty(givenReason))
				{
					//Invalid (or missing) argument(s)
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
					return true;
				}

				//Check if the specified player exists (is currently online)
				bool playerExists = false;
				int[] allPlayers = m.AllPlayers();
				for (int i = 0; i < allPlayers.Length; i++)
				{
					if (warnplayer.Equals(m.GetPlayerName(allPlayers[i]), StringComparison.InvariantCultureIgnoreCase))
					{
						playerExists = true;
						break;
					}
				}
				if (!playerExists)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(9), warnplayer));
					return true;
				}

				System.Console.WriteLine(string.Format("[ServerUtilities] {0} warned {1}.", m.GetPlayerName(player), warnplayer));

				if (GlobalNotification_Warn)
				{
					m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(55), warnplayer, m.GetPlayerName(player), givenReason));
				}
				else
				{
					m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(56), warnplayer, m.GetPlayerName(player), givenReason));
				}
				try
				{
					//Save warning to log file
					if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "WarnedPlayersLog.txt"))
					{
						File.Create(sModDir + Path.DirectorySeparatorChar + "WarnedPlayersLog.txt").Close();
						System.Console.WriteLine("[ServerUtilities] WarnedPlayersLog.txt created!");
					}
					using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "WarnedPlayersLog.txt", true))
					{
						sw.WriteLine(string.Format("{0}: {1} warned {2}. Reason:{3}", System.DateTime.UtcNow.ToString(), m.GetPlayerName(player), warnplayer, givenReason));
					}
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
				}
				return true;
			}
			return false;
		}

		bool ReportPlayer(int player, string command, string argument)
		{
			if (command.Equals("report", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "report"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(57));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to report someone (no permission)", m.GetPlayerName(player)));
					return true;
				}
				string[] args;
				string rep_player = "";
				string givenReason = "";
				try
				{
					args = argument.Split(' ');
					rep_player = args[0];
					for (int i = 1; i < args.Length; i++)
					{
						givenReason = givenReason + " " + args[i];
					}
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
					return true;
				}
				if (string.IsNullOrEmpty(rep_player) || string.IsNullOrEmpty(givenReason))
				{
					//Invalid (or missing) argument(s)
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
					return true;
				}
				//Check if the specified player exists (is currently online)
				bool playerExists = false;
				int[] allPlayers = m.AllPlayers();
				for (int i = 0; i < allPlayers.Length; i++)
				{
					if (rep_player.Equals(m.GetPlayerName(allPlayers[i]), StringComparison.InvariantCultureIgnoreCase))
					{
						playerExists = true;
						break;
					}
				}
				if (!playerExists)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(9), rep_player));
					return true;
				}
				//Check if at least one member of any group with a permission level equal to/above the one specified in "ReportToGroup" is online
				bool notificationSent = false;
				for (int i = 0; i < allPlayers.Length; i++)
				{
					if (m.GetPlayerPermissionLevel(allPlayers[i]) >= Report_PlayerGroup)
					{
						//Notify members of the specified group that a player has been reported
						m.SendMessage(allPlayers[i], chatPrefix + string.Format(GetLocalizedString(58), System.DateTime.UtcNow.ToString(), m.GetPlayerName(player), rep_player, givenReason));
						notificationSent = true;
					}
				}
				if (!notificationSent)
				{
					try
					{
						//Nobody online --> notify next member of the specified group when he/she joins
						//Save as files to prevent reports getting lost due to restarts
						if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "ReportedPlayers.txt"))
						{
							File.Create(sModDir + Path.DirectorySeparatorChar + "ReportedPlayers.txt").Close();
							System.Console.WriteLine("[ServerUtilities] ReportedPlayers.txt created!");
						}
						using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "ReportedPlayers.txt", true))
						{
							sw.WriteLine(System.DateTime.UtcNow.ToString());    //Time of report
							sw.WriteLine(rep_player);                           //Name of the reported player
							sw.WriteLine(m.GetPlayerName(player));              //Name of the player who reported
							sw.WriteLine(givenReason);                          //Reason for the report
						}
					}
					catch (Exception ex)
					{
						System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
					}
				}
				try
				{
					//Create a log file if it doesn't already exist
					if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "ReportedPlayersLog.txt"))
					{
						File.Create(sModDir + Path.DirectorySeparatorChar + "ReportedPlayersLog.txt").Close();
						System.Console.WriteLine("[ServerUtilities] ReportedPlayersLog.txt created!");
					}
					//Write same stuff into logfile (in a more compact form)
					using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "ReportedPlayersLog.txt", true))
					{
						if (!notificationSent)
						{
							sw.Write("UNSEEN - ");
						}
						sw.WriteLine(string.Format("{0}: {1} reports {2}:{3}", System.DateTime.UtcNow.ToString(), m.GetPlayerName(player), rep_player, givenReason));
					}
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
				}

				System.Console.WriteLine(string.Format("[ServerUtilities] {0} reported {1} (Reason:{2})", m.GetPlayerName(player), rep_player, givenReason));
				m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(59), rep_player, givenReason));
				return true;
			}
			return false;
		}

		void NotifyAboutReports(int player)
		{
			if (m.GetPlayerPermissionLevel(player) >= Report_PlayerGroup)
			{
				//Is member of "ReportToGroup" or higher
				if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "ReportedPlayers.txt"))
				{
					//No reports => No notification necessary
					return;
				}
				DirectoryInfo di = new DirectoryInfo(sModDir + Path.DirectorySeparatorChar + "ReportedPlayers.txt");
				try
				{
					using (TextReader tr = new StreamReader(di.FullName, Encoding.UTF8))
					{
						m.SendMessage(player, GetLocalizedString(42));
						string time = tr.ReadLine();
						string reported = tr.ReadLine();
						string reporter = tr.ReadLine();
						string reason = tr.ReadLine();
						while (!string.IsNullOrEmpty(reason))
						{
							m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(58), time, reporter, reported, reason));
							time = tr.ReadLine();
							reported = tr.ReadLine();
							reporter = tr.ReadLine();
							reason = tr.ReadLine();
						}
						m.SendMessage(player, GetLocalizedString(42));
					}
					//Display name of the admin that has been informed and write it to the log.
					//Create a log file if it doesn't already exist - unlikely at this point, but you never know when users delete stuff...
					if (!File.Exists(sModDir + Path.DirectorySeparatorChar + "ReportedPlayersLog.txt"))
					{
						File.Create(sModDir + Path.DirectorySeparatorChar + "ReportedPlayersLog.txt").Close();
						System.Console.WriteLine("[ServerUtilities] ReportedPlayersLog.txt created!");
					}
					//Write name of informed admin into logfile
					using (StreamWriter sw = new StreamWriter(sModDir + Path.DirectorySeparatorChar + "ReportedPlayersLog.txt", true))
					{
						sw.WriteLine(string.Format("{0} has been informed about all above (prefixed with UNSEEN)", m.GetPlayerName(player)));
					}
					System.Console.WriteLine("[ServerUtilities] Informed " + m.GetPlayerName(player) + " about recent reports.");
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("[ServerUtilities] ERROR:  " + ex.Message);
				}
				//Delete all reports (as an admin has been informed)
				File.Delete(sModDir + Path.DirectorySeparatorChar + "ReportedPlayers.txt");
				System.Console.WriteLine("[ServerUtilities] ReportedPlayers.txt deleted.");
			}
		}

		bool ChangeSeason(int player, string command, string argument)
		{
			if (command.Equals("season", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "season"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(60));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to change the season (no permission)", m.GetPlayerName(player)));
					return true;
				}
				int seasonID = -1;
				try
				{
					seasonID = Convert.ToInt32(argument);
				}
				catch (FormatException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(1));
					return true;
				}
				catch (OverflowException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(2));
					return true;
				}

				//Check if season ID exists
				if ((seasonID < 0) || (seasonID > 3))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(61), seasonID.ToString()));
					return true;
				}

				//The code used to change the season is entirely taken from Default.cs

				SoundSet solidSounds;
				SoundSet snowSounds;
				SoundSet noSound;
				solidSounds = new SoundSet()
				{
					Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
					Break = new string[] { "destruct" },
					Build = new string[] { "build" },
					Clone = new string[] { "clone" },
				};
				snowSounds = new SoundSet()
				{
					Walk = new string[] { "walksnow1", "walksnow2", "walksnow3", "walksnow4" },
					Break = new string[] { "destruct" },
					Build = new string[] { "build" },
					Clone = new string[] { "clone" },
				};
				noSound = new SoundSet();

				// spring
				if (seasonID == 0)
				{
					m.SetBlockType(2, "Grass", new BlockType()
					{
						TextureIdTop = "SpringGrass",
						TextureIdBack = "SpringGrassSide",
						TextureIdFront = "SpringGrassSide",
						TextureIdLeft = "SpringGrassSide",
						TextureIdRight = "SpringGrassSide",
						TextureIdForInventory = "SpringGrassSide",
						TextureIdBottom = "Dirt",
						DrawType = DrawType.Solid,
						WalkableType = WalkableType.Solid,
						Sounds = snowSounds,
						WhenPlayerPlacesGetsConvertedTo = 3,
					});
					m.SetBlockType(18, "OakLeaves", new BlockType()
					{
						AllTextures = "OakLeaves",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
					m.SetBlockType(106, "Apples", new BlockType()
					{
						AllTextures = "Apples",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
				}
				// summer
				if (seasonID == 1)
				{
					m.SetBlockType(2, "Grass", new BlockType()
					{
						TextureIdTop = "Grass",
						TextureIdBack = "GrassSide",
						TextureIdFront = "GrassSide",
						TextureIdLeft = "GrassSide",
						TextureIdRight = "GrassSide",
						TextureIdForInventory = "GrassSide",
						TextureIdBottom = "Dirt",
						DrawType = DrawType.Solid,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
						WhenPlayerPlacesGetsConvertedTo = 3,
					});
					m.SetBlockType(18, "OakLeaves", new BlockType()
					{
						AllTextures = "OakLeaves",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
					m.SetBlockType(106, "Apples", new BlockType()
					{
						AllTextures = "Apples",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
					m.SetBlockType(8, "Water", new BlockType()
					{
						AllTextures = "Water",
						DrawType = DrawType.Fluid,
						WalkableType = WalkableType.Fluid,
						Sounds = noSound,
					});
				}
				// autumn
				if (seasonID == 2)
				{
					m.SetBlockType(2, "Grass", new BlockType()
					{
						TextureIdTop = "AutumnGrass",
						TextureIdBack = "AutumnGrassSide",
						TextureIdFront = "AutumnGrassSide",
						TextureIdLeft = "AutumnGrassSide",
						TextureIdRight = "AutumnGrassSide",
						TextureIdForInventory = "AutumnGrassSide",
						TextureIdBottom = "Dirt",
						DrawType = DrawType.Solid,
						WalkableType = WalkableType.Solid,
						Sounds = snowSounds,
						WhenPlayerPlacesGetsConvertedTo = 3,
					});
					m.SetBlockType(18, "OakLeaves", new BlockType()
					{
						AllTextures = "AutumnLeaves",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
					m.SetBlockType(106, "Apples", new BlockType()
					{
						AllTextures = "AutumnApples",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
				}
				// winter
				if (seasonID == 3)
				{
					m.SetBlockType(2, "Grass", new BlockType()
					{
						TextureIdTop = "WinterGrass",
						TextureIdBack = "WinterGrassSide",
						TextureIdFront = "WinterGrassSide",
						TextureIdLeft = "WinterGrassSide",
						TextureIdRight = "WinterGrassSide",
						TextureIdForInventory = "WinterGrassSide",
						TextureIdBottom = "Dirt",
						DrawType = DrawType.Solid,
						WalkableType = WalkableType.Solid,
						Sounds = snowSounds,
						WhenPlayerPlacesGetsConvertedTo = 3,
					});
					m.SetBlockType(18, "OakLeaves", new BlockType()
					{
						AllTextures = "WinterLeaves",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
					m.SetBlockType(106, "Apples", new BlockType()
					{
						AllTextures = "WinterApples",
						DrawType = DrawType.Transparent,
						WalkableType = WalkableType.Solid,
						Sounds = solidSounds,
					});
					m.SetBlockType(8, "Water", new BlockType()
					{
						AllTextures = "Ice",
						DrawType = DrawType.Solid,
						WalkableType = WalkableType.Solid,
						Sounds = snowSounds,
						IsSlipperyWalk = true,
					});
				}
				m.UpdateBlockTypes();
				m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(93), m.GetPlayerName(player)));
				System.Console.WriteLine(string.Format("[ServerUtilities] {0} changed the season.", m.GetPlayerName(player)));
				return true;
			}
			return false;
		}

		bool SendAFK(int player, string command, string argument)
		{
			if (command.Equals("afk", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "afk"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(64));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to send an AFK message (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (string.IsNullOrEmpty(argument))
				{
					//No arguments given
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
					return true;
				}

				string[] args;
				int time = -1;
				string givenReason = "";
				try
				{
					args = argument.Split(' ');
					time = Convert.ToInt32(args[0]);
					for (int i = 1; i < args.Length; i++)
					{
						givenReason = givenReason + " " + args[i];
					}
				}
				catch (FormatException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(1));
					return true;
				}
				catch (OverflowException)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(2));
					return true;
				}
				if (string.IsNullOrEmpty(givenReason))
				{
					//No AFK reason specified
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
					return true;
				}

				//Notify all players
				m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(65), m.GetPlayerName(player), time.ToString(), givenReason));
				return true;
			}
			return false;
		}

		bool SendBTK(int player, string command, string argument)
		{
			if (command.Equals("btk", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "afk"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(64));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to send a BTK message (no permission)", m.GetPlayerName(player)));
					return true;
				}
				//Notify all players
				m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(67), m.GetPlayerName(player)));
				return true;
			}
			return false;
		}

		void Uptime_Tick()
		{
			//Count uptime in seconds. Better than in 4 different variables
			UptimeSeconds++;
			int iRemainingSeconds = (iRestartInterval * 3600) - UptimeSeconds;
			if (iRemainingSeconds <= (20 * 60) && iRemainingSeconds >= 0)
			{
				//Warn players every 5 minutes
				if (iRemainingSeconds % (5 * 60) == 0)
				{
					m.SendMessageToAll(string.Format(chatPrefix + "&cServer restarts in {0} minutes!", iRemainingSeconds / 60));
				}
				if (iRemainingSeconds <= 60 && (iRemainingSeconds % 20 == 0))
				{
					m.SendMessageToAll(string.Format(chatPrefix + "&cServer restarts in {0} seconds!", iRemainingSeconds));
				}
			}
		}

		bool DisplayUptime(int player, string command, string argument)
		{
			if (command.Equals("uptime", StringComparison.InvariantCultureIgnoreCase))
			{
				//Calculate days, hours, minutes and seconds
				int tempTime = UptimeSeconds;
				int tempDays = (tempTime / 86400);
				tempTime -= (tempDays * 86400);
				int tempHours = (tempTime / 3600);
				tempTime -= (tempHours * 3600);
				int tempMinutes = (tempTime / 60);
				tempTime -= (tempMinutes * 60);
				int tempSeconds = tempTime;
				//Send result to player
				m.SendMessage(player, chatPrefix + string.Format("&7Server Uptime (Restart interval: {0}):", iRestartInterval));
				m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString(66), tempDays, tempHours, tempMinutes, tempSeconds));
				return true;
			}
			return false;
		}

		bool SendMe(int player, string command, string argument)
		{
			if (command.Equals("me", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "me"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(68));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use /me (no permission)", m.GetPlayerName(player)));
					return true;
				}
				m.SendMessageToAll(string.Format("&7{0} {1}", m.GetPlayerName(player), argument));
				return true;
			}
			return false;
		}

		bool SetSpectate(int player, string command, string argument)
		{
			if (command.Equals("vanish", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "vanish"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(69));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to use /vanish (no permission)", m.GetPlayerName(player)));
					return true;
				}
				if (m.IsPlayerSpectator(player))
				{
					//Player already spectator. Make visible again.
					m.SetPlayerSpectator(player, false);
					if (GlobalNotification_Vanish)
					{
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(71), m.GetPlayerName(player)));
					}
					else
					{
						m.SendMessage(player, chatPrefix + GetLocalizedString(73));
					}
				}
				else
				{
					//Player is not a spectator yet. Make invisible.
					m.SetPlayerSpectator(player, true);
					if (GlobalNotification_Vanish)
					{
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(70), m.GetPlayerName(player)));
					}
					else
					{
						m.SendMessage(player, chatPrefix + GetLocalizedString(72));
					}
				}
				return true;
			}
			return false;
		}

		bool ToggleNotifications(int player, string command, string argument)
		{
			if (command.Equals("notification", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "notification"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(74));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to change notification options (no permission)", m.GetPlayerName(player)));
					return true;
				}
				switch (argument)
				{
					case "vanish":
						//If currently enabled, disable notifications
						if (GlobalNotification_Vanish)
						{
							GlobalNotification_Vanish = false;
							m.SendMessage(player, chatPrefix + GetLocalizedString(75));
						}
						else
						{
							GlobalNotification_Vanish = true;
							m.SendMessage(player, chatPrefix + GetLocalizedString(76));
						}
						break;

					case "warn":
						//If currently enabled, disable notifications
						if (GlobalNotification_Warn)
						{
							GlobalNotification_Warn = false;
							m.SendMessage(player, chatPrefix + GetLocalizedString(77));
						}
						else
						{
							GlobalNotification_Warn = true;
							m.SendMessage(player, chatPrefix + GetLocalizedString(78));
						}
						break;

					default:
						//invalid argument supplied - output error message
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(17));
						break;
				}
				SaveSettings();
				return true;
			}
			return false;
		}

		bool ChangeLanguage(int player, string command, string argument)
		{
			if (command.Equals("language", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "language"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(79));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to change the language (no permission)", m.GetPlayerName(player)));
					return true;
				}
				switch (argument)
				{
					case "EN":
					case "en": //English
						languageCode = "EN";
						m.SendMessage(player, chatPrefix + GetLocalizedString(80));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} changed language to {1}", m.GetPlayerName(player), languageCode));
						break;

					case "DE":
					case "de": //Deutsch (German)
						languageCode = "DE";
						m.SendMessage(player, chatPrefix + GetLocalizedString(80));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} changed language to {1}", m.GetPlayerName(player), languageCode));
						break;

					default:
						m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(94), argument));
						break;
				}
				SaveSettings();
				return true;
			}
			return false;
		}

		bool DisplayVersion(int player, string command, string argument)
		{
			if (command.Equals("version", StringComparison.InvariantCultureIgnoreCase))
			{
				m.SendMessage(player, chatPrefix + string.Format("ServerUtilities Mod &b{0} &8by croxxx.", versionNumber));
				return true;
			}
			return false;
		}

		List<string> mutedPlayers = new List<string>();

		bool ChangeMuteStatus(int player, string command, string argument)
		{
			if (command.Equals("mute", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "mute"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(81));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to mute a player (no permission)", m.GetPlayerName(player)));
					return true;
				}
				bool playerIsOnline = false;
				int[] allPlayers = m.AllPlayers();
				for (int i = 0; i < allPlayers.Length; i++)
				{
					if (argument.Equals(m.GetPlayerName(allPlayers[i]), StringComparison.InvariantCultureIgnoreCase))
					{
						playerIsOnline = true;
						//Check if player is already muted (prevents "double-mutes")
						for (int j = 0; j < mutedPlayers.Count; j++)
						{
							if (mutedPlayers[j].Equals(m.GetPlayerName(allPlayers[i]), StringComparison.InvariantCultureIgnoreCase))
							{
								//Player already muted. Display error and abort
								m.SendMessageToAll(chatPrefix + m.colorError() + string.Format(GetLocalizedString(88), mutedPlayers[i]));
								System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to mute a player (already muted)", m.GetPlayerName(player), mutedPlayers[i]));
								return true;
							}
						}
						mutedPlayers.Add(m.GetPlayerName(allPlayers[i]));
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(82), m.GetPlayerName(allPlayers[i]), m.GetPlayerName(player)));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} muted {1}.", m.GetPlayerName(player), m.GetPlayerName(allPlayers[i])));
						break;
					}
				}
				if (!playerIsOnline)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(9), argument));
					return true;
				}
				return true;
			}
			if (command.Equals("unmute", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "mute"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(81));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to unmute a player (no permission)", m.GetPlayerName(player)));
					return true;
				}
				//Unmute player if muted
				for (int i = 0; i < mutedPlayers.Count; i++)
				{
					if (mutedPlayers[i].Equals(argument, StringComparison.InvariantCultureIgnoreCase))
					{
						m.SendMessageToAll(chatPrefix + string.Format(GetLocalizedString(83), mutedPlayers[i], m.GetPlayerName(player)));
						System.Console.WriteLine(string.Format("[ServerUtilities] {0} un-muted {1}.", m.GetPlayerName(player), mutedPlayers[i]));
						mutedPlayers.RemoveAt(i);
						return true;
					}
				}
				//Else output error message
				m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(84), argument));
				return true;
			}
			return false;
		}

		string CheckMute(int player, string message, bool toTeam)
		{
			foreach (string playerName in mutedPlayers)
			{
				if (playerName.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
				{
					m.SendMessage(player, chatPrefix + GetLocalizedString(85));
					return null;
				}
			}
			return message;
		}

		bool ToggleSpectate(int player, string command, string argument)
		{
			if (command.Equals("spectate", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "spectate"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(86));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to spectate someone (no permission)", m.GetPlayerName(player)));
					return true;
				}
				int targetID = GetPlayerID(argument);
				if (targetID != -1)
				{
					//Follow the target if it exists
					m.FollowPlayer(player, targetID, true);
				}
				else
				{
					//Player does not exist. Display error message.
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString(9), argument));
				}
				return true;
			}
			if (command.Equals("stopspectate", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "spectate"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(86));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to stop spectating someone (no permission)", m.GetPlayerName(player)));
					return true;
				}
				//Disable any following
				m.FollowPlayer(player, -1, false);
				return true;
			}
			return false;
		}

		bool SpawnCommand(int player, string command, string argument)
		{
			if (command.Equals("spawn", StringComparison.InvariantCultureIgnoreCase))
			{
				//Get the player's default spawn position and move them there
				float[] pos = m.GetDefaultSpawnPosition(player);
				m.SetPlayerPosition(player, pos[0], pos[1], pos[2]);
				m.SendMessage(player, chatPrefix + GetLocalizedString(95));
				return true;
			}
			return false;
		}

		bool ReloadConfig(int player, string command, string argument)
		{
			if (command.Equals("su_reload", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "su_reload"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(96));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to reload configuration (no permission)", m.GetPlayerName(player)));
					return true;
				}
				//Reload the mod settings and notify on success
				LoadSettings();
				m.SendMessage(player, chatPrefix + GetLocalizedString(97));
				System.Console.WriteLine(string.Format("[ServerUtilities] {0} reloaded the configuration", m.GetPlayerName(player)));
				return true;
			}
			return false;
		}

		bool SetNameColor(int player, string command, string argument)
		{
			if (command.Equals("namecolor", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "namecolor"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString(98));
					System.Console.WriteLine(string.Format("[ServerUtilities] {0} tried to change name color (no permission)", m.GetPlayerName(player)));
					return true;
				}
				//Change name color and notify player
				m.SetPlayerNameColor(player, argument);
				m.SendMessage(player, chatPrefix + GetLocalizedString(99));
				System.Console.WriteLine(string.Format("[ServerUtilities] {0} changed their name color", m.GetPlayerName(player)));
				return true;
			}
			return false;
		}

		void AutoColorName(int player)
		{
			try
			{
				m.SetPlayerNameColor(player, m.GetGroupColor(player));
			}
			catch
			{
				//Do no coloring if above fails
			}
		}
	}
}

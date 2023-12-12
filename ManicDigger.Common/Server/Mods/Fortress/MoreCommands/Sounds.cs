/*
 * Sounds Mod - Version 1.1
 * Last change: 2013-11-17
 * Author: croxxx
 * 
 * This mod adds a command to play custom sounds.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
	public class Sounds : IMod
	{
		public void PreStart(ModManager m) { }

		public void Start(ModManager m)
		{
			this.m = m;
			m.RegisterOnCommand(Sound);
			m.RegisterCommandHelp("play_sounds", "/s [soundname]");
			m.RegisterCommandHelp("reload_soundlist", "/sound reload - Reloads the mod's soundlist.");
			LoadSoundlist();
			System.Console.WriteLine("[Sounds] Loaded Mod Version 1.1");
		}

		//Option if sound is played at source player position or at all players position
		bool playSoundsGlobal = true;

		//Internal variables
		//DO NOT CHANGE!!
		ModManager m;
		List<string> soundlist;
		string chatPrefix = "&8[&6Sounds&8] ";
		bool soundlistLoaded = false;

		void LoadSoundlist()
		{
			if (File.Exists("data/public/sounds/soundlist.txt"))
			{
				soundlist = new List<string>();
				string[] lines = System.IO.File.ReadAllLines("data/public/sounds/soundlist.txt");
				// Store all files listed in soundlist.txt in local List.
				foreach (string line in lines)
				{
					soundlist.Add(line);
				}
				soundlistLoaded = true;
				System.Console.WriteLine("[Sounds] Soundlist loaded.");
			}
			else
			{
				soundlistLoaded = false;
				System.Console.WriteLine("[Sounds] 'soundlist.txt' file does not exist in data/public/sounds.");
			}
		}

		void PlaySoundToAll(string filename)
		{
			int[] playerlist = m.AllPlayers();
			for (int i = 0; i < playerlist.Length; i++)
			{
				int playerID = playerlist[i];
				m.PlaySoundAt((int)m.GetPlayerPositionX(playerID), (int)m.GetPlayerPositionY(playerID), (int)m.GetPlayerPositionZ(playerID), filename);
			}
		}

		bool Sound(int player, string command, string argument)
		{
			if ((command.Equals("sound", StringComparison.InvariantCultureIgnoreCase)) || (command.Equals("s", StringComparison.InvariantCultureIgnoreCase)))
			{
				if (argument.Equals("reload", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!m.PlayerHasPrivilege(player, "reload_soundlist"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + "You are not allowed to reload the soundlist.");
						System.Console.WriteLine(string.Format("[Sounds] {0} tried to reload the soundlist (no permission).", m.GetPlayerName(player)));
						return true;
					}
					LoadSoundlist();
					return true;
				}
				if (!m.PlayerHasPrivilege(player, "play_sounds"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + "You are not allowed to play sounds.");
					System.Console.WriteLine(string.Format("[Sounds] {0} tried to play a sound (no permission).", m.GetPlayerName(player)));
					return true;
				}
				if (soundlistLoaded)
				{
					bool soundexists = false;
					foreach (string sound in soundlist)
					{
						if (argument.Equals(sound, StringComparison.InvariantCultureIgnoreCase))
						{
							soundexists = true;
							if (playSoundsGlobal)
							{
								//May result in sound being played multiple times at once if players are too close to each other (128 blocks?)
								PlaySoundToAll(sound + ".ogg");
							}
							else
							{
								m.PlaySoundAt((int)m.GetPlayerPositionX(player), (int)m.GetPlayerPositionY(player), (int)m.GetPlayerPositionZ(player), sound + ".ogg");
							}

							m.SendMessageToAll(chatPrefix + string.Format("&7{0} played sound {1}.", m.GetPlayerName(player), sound));
							return true;
						}
					}
					if (!soundexists)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + "Sound not found.");
						System.Console.WriteLine(string.Format("[Sounds] {0} tried to play sound {1} (not found).", m.GetPlayerName(player), argument));
						return true;
					}
				}
			}
			if (command.Equals("soundlist", StringComparison.InvariantCultureIgnoreCase))
			{
				m.SendMessage(player, chatPrefix + "List of available sounds:");
				m.SendMessage(player, "&8----------------------------------------");
				foreach (string sound in soundlist)
				{
					m.SendMessage(player, sound);
				}
				m.SendMessage(player, "&8----------------------------------------");
				return true;
			}
			return false;
		}
	}
}

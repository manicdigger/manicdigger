using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ManicDigger.Mods
{
	public class RememberPosition : IMod
	{
		public void PreStart(ModManager m) { }
		
		public void Start(ModManager manager)
		{
			m = manager;
			LoadData();
			m.RegisterOnSave(SaveData);
			m.RegisterOnPlayerJoin(OnJoin);
			m.RegisterOnPlayerLeave(OnLeave);
		}
		
		ModManager m;
		string filename = "UserData" + Path.DirectorySeparatorChar + "StoredPositions.txt";
		public PositionStorage positions;
		
		public void LoadData()
		{
			positions = new PositionStorage();
			
			if (!File.Exists(filename))
			{
				//Nothing to load if file does not exist
				return;
			}
			try
			{
				string[] lines = File.ReadAllLines(filename);
				for (int i = 0; i < lines.Length; i++)
				{
					string[] linesplit = lines[i].Split(';');
					try
					{
						int[] pos = positions.StringToPos(linesplit[1]);
						positions.Store(linesplit[0], pos[0], pos[1], pos[2]);
					}
					catch
					{
						//Skip line when read fails
						Console.WriteLine("[WARNING] Skipping invalid entry on line {0}.", i + 1);
					}
				}
			}
			catch
			{
				Console.WriteLine("[ERROR] StoredPositions.txt could not be read!");
			}
		}
		
		public void SaveData()
		{
			try
			{
				List<string> lines = new List<string>();
				foreach (UserEntry entry in positions.PlayerPositions)
				{
					lines.Add(string.Format("{0};{1}", entry.Name, entry.Position));
				}
				File.WriteAllLines(filename, lines.ToArray());
			}
			catch
			{
				Console.WriteLine("[ERROR] Could not save last player positions");
			}
		}
		
		public void OnJoin(int player)
		{
			string name = m.GetPlayerName(player);
			if (positions.IsStoredAt(name) != -1)
			{
				int[] pos = positions.Get(name);
				if (pos != null)
				{
					m.SetPlayerPosition(player, pos[0], pos[1], pos[2]);
					//Console.WriteLine("[INFO] Position restored: {0}({1},{2},{3})", name, pos[0], pos[1], pos[2]);
				}
			}
		}
		
		public void OnLeave(int player)
		{
			if (m.IsBot(player))
			{
				//Don't store bot positions
				return;
			}
			
			//Do not save position if it is outside the map
			int x = (int)m.GetPlayerPositionX(player);
			int y = (int)m.GetPlayerPositionY(player);
			int z = (int)m.GetPlayerPositionZ(player);
			if (x > 0 && y > 0 && z > 0)
			{
				if (x < m.GetMapSizeX() && y < m.GetMapSizeY() && z < m.GetMapSizeZ())
				{
					positions.Store(m.GetPlayerName(player), x, y, z);
				}
			}
		}
	}
	
	public class PositionStorage
	{
		public List<UserEntry> PlayerPositions { get; set; }
		
		public PositionStorage()
		{
			this.PlayerPositions = new List<UserEntry>();
		}
		
		public int IsStoredAt(string player)
		{
			for (int i = 0; i < PlayerPositions.Count; i++)
			{
				if (player.Equals(PlayerPositions[i].Name, StringComparison.InvariantCultureIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}
		
		public void Store(string player, int x, int y, int z)
		{
			if (IsStoredAt(player) != -1)
			{
				Delete(player);
			}
			UserEntry entry = new UserEntry();
			entry.Name = player;
			entry.Position = PosToString(x, y, z);
			PlayerPositions.Add(entry);
			//Console.WriteLine("[INFO] Position saved: {0}({1})", entry.Name, entry.Position);
		}
		
		public void Delete(string player)
		{
			for (int i = 0; i < PlayerPositions.Count; i++)
			{
				if (player.Equals(PlayerPositions[i].Name, StringComparison.InvariantCultureIgnoreCase))
				{
					PlayerPositions.RemoveAt(i);
					return;
				}
			}
		}
		
		public int[] Get(string player)
		{
			int index = IsStoredAt(player);
			if (index != -1)
			{
				return StringToPos(PlayerPositions[index].Position);
			}
			return null;
		}
		
		public string PosToString(int x, int y, int z)
		{
			return string.Format("{0},{1},{2}", x, y, z);
		}
		
		public int[] StringToPos(string position)
		{
			try
			{
				string[] split = position.Split(',');
				int[] retval = new int[3];
				retval[0] = int.Parse(split[0]);
				retval[1] = int.Parse(split[1]);
				retval[2] = int.Parse(split[2]);
				return retval;
			}
			catch
			{
				Console.WriteLine("[ERROR] Could not convert '{0}' to coordinates", position);
				return null;
			}
		}
	}
	
	public class UserEntry
	{
		public string Name { get; set; }
		public string Position { get; set; }
	}
}

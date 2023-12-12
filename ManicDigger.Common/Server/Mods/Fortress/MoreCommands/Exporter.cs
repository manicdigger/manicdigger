/*
 * Exporter Mod - Version 1.0
 * Last changed: 2014-03-12
 * Author: croxxx
 * 
 * This mod allows you to export certain areas of your map as a file (and import these again).
 * Useful for cross-map copying.
 * Following commands are added:
 * -export start		Sets the start point for the export area
 * -export end			Sets the start point for the export area
 * -export save [name]	Saves the marked area as "name.mdexp"
 * -import	[filename]
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
	public class Exporter : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("Default");
		}

		public void Start(ModManager m)
		{
			this.m = m;

			m.RegisterPrivilege("export");
			m.RegisterOnCommand(OnCommand);
			m.RegisterCommandHelp("export", "Allows you to export/import specified areas of the map");

			if (!Directory.Exists(dirPath))
			{
				System.Console.WriteLine("[Exporter] UserData/ExportedBuildings not found. Creating it.");
				Directory.CreateDirectory(dirPath);
				System.Console.WriteLine("[Exporter] Directory created successfully.");
			}

			m.LogServerEvent(string.Format("[Exporter] Loaded Mod Version {0}", versionNumber));
			System.Console.WriteLine(string.Format("[Exporter] Loaded Mod Version {0}", versionNumber));
		}

		//Internal variables.
		//DO NOT CHANGE!
		ModManager m;
		string dirPath = "UserData" + Path.DirectorySeparatorChar + "ExportedBuildings";
		string chatPrefix = "&8[&6Exporter&8] ";
		string versionNumber = "1.0"; //TODO: Update to next release

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

		Vector3i GetPlayerPosition(int PlayerID)
		{
			return new Vector3i((int)m.GetPlayerPositionX(PlayerID), (int)m.GetPlayerPositionY(PlayerID), (int)m.GetPlayerPositionZ(PlayerID));
		}

		Vector3i export_start;
		Vector3i export_end;
		string start_set_by = "";
		string end_set_by = "";

		bool OnCommand(int player, string command, string argument)
		{
			if (command.Equals("export", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "export"))
				{
					System.Console.WriteLine(string.Format("[Exporter] {0} tried to export (no permission)", m.GetPlayerName(player)));
					m.SendMessage(player, chatPrefix + m.colorError() + "You are not allowed to export areas.");
					return true;
				}
				if (argument.Equals("start", StringComparison.InvariantCultureIgnoreCase))
				{
					export_start = GetPlayerPosition(player);
					m.SendMessage(player, chatPrefix + string.Format("&aExport start point set to: {0}, {1}, {2}", export_start.x, export_start.y, export_start.z));
					start_set_by = m.GetPlayerName(player);
					return true;
				}
				if (argument.Equals("end", StringComparison.InvariantCultureIgnoreCase))
				{
					export_end = GetPlayerPosition(player);
					m.SendMessage(player, chatPrefix + string.Format("&aExport end point set to: {0}, {1}, {2}", export_end.x, export_end.y, export_end.z));
					end_set_by = m.GetPlayerName(player);
					return true;
				}
				string[] args;
				try
				{
					args = argument.Split(' ');
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + "Error while parsing arguments.");
					return true;
				}
				if (args[0].Equals("save", StringComparison.InvariantCultureIgnoreCase))
				{
					if (args.Length < 2)
					{
						m.SendMessage(player, chatPrefix + m.colorError() + "Invalid arguments. Read README for instructions.");
						return true;
					}
					if (!start_set_by.Equals(end_set_by, StringComparison.InvariantCultureIgnoreCase))
					{
						System.Console.WriteLine(string.Format("[Exporter] {0} tried to export (start/end not set by same player)", m.GetPlayerName(player)));
						m.SendMessage(player, chatPrefix + m.colorError() + "Start/End point not set by same player.");
						return true;
					}
					if ((!start_set_by.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase)) || (!end_set_by.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase)))
					{
						System.Console.WriteLine(string.Format("[Exporter] {0} tried to export (start/end not set by this player)", m.GetPlayerName(player)));
						m.SendMessage(player, chatPrefix + m.colorError() + "Start/End point not set by YOU.");
						return true;
					}

					Vector3i start, end;

					if (export_start.x < export_end.x)
					{
						start.x = export_start.x;
						end.x = export_end.x;
					}
					else
					{
						start.x = export_end.x;
						end.x = export_start.x;
					}
					if (export_start.y < export_end.y)
					{
						start.y = export_start.y;
						end.y = export_end.y;
					}
					else
					{
						start.y = export_end.y;
						end.y = export_start.y;
					}
					if (export_start.z < export_end.z)
					{
						start.z = export_start.z;
						end.z = export_end.z;
					}
					else
					{
						start.z = export_end.z;
						end.z = export_start.z;
					}

					int totalBlocks = Math.Abs(((end.x - start.x) + 1) * ((end.y - start.y) + 1) * ((end.z - start.z) + 1));
					m.SendMessage(player, chatPrefix + string.Format("&6Exporting {0} blocks to {1}.mdexp", totalBlocks, args[1]));
					System.Console.WriteLine(string.Format("[Exporter] {0} started export ({1} blocks)", m.GetPlayerName(player), totalBlocks));

					try
					{
						using (BinaryWriter bw = new BinaryWriter(new FileStream(dirPath + Path.DirectorySeparatorChar + args[1] + ".mdexp", FileMode.CreateNew)))
						{
							bw.Write((int)totalBlocks);
							for (int x = start.x; x <= end.x; x++)
							{
								for (int y = start.y; y <= end.y; y++)
								{
									for (int z = start.z; z <= end.z; z++)
									{
										//Write block data in binary form.
										bw.Write((short)(x - start.x));
										bw.Write((short)(y - start.y));
										bw.Write((short)(z - start.z));
										bw.Write(m.GetBlockNameAt(x, y, z));
									}
								}
							}
						}
						System.Console.WriteLine("[Exporter] Export finished.");
						m.SendMessage(player, chatPrefix + "&aExport finished.");
					}
					catch (Exception ex)
					{
						System.Console.WriteLine("[Exporter] ERROR:  " + ex.Message);
						m.SendMessage(player, chatPrefix + m.colorError() + "An error occured. Check console for details.");
					}
					return true;
				}
				m.SendMessage(player, chatPrefix + m.colorError() + "Invalid arguments. Read README for instructions.");
				return true;
			}
			if (command.Equals("import", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "export"))
				{
					System.Console.WriteLine(string.Format("[Exporter] {0} tried to import (no permission)", m.GetPlayerName(player)));
					m.SendMessage(player, chatPrefix + m.colorError() + "You are not allowed to import areas.");
					return true;
				}
				if (!File.Exists(dirPath + Path.DirectorySeparatorChar + argument + ".mdexp"))
				{
					System.Console.WriteLine(string.Format("[Exporter] {0} tried to import (file does not exist)", m.GetPlayerName(player)));
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format("The file {0}.mdexp does not exist.", argument));
					return true;
				}
				System.Console.WriteLine("[Exporter] Import started.");
				m.SendMessage(player, chatPrefix + "&6Import started.");
				Vector3i pos = GetPlayerPosition(player);
				DirectoryInfo di = new DirectoryInfo(dirPath + Path.DirectorySeparatorChar + argument + ".mdexp");
				try
				{
					using (BinaryReader br = new BinaryReader(new FileStream(di.FullName, FileMode.Open)))
					{
						int blockCount = br.ReadInt32();
						System.Console.WriteLine(string.Format("[Exporter] Importing {0} blocks...", blockCount));
						m.SendMessage(player, chatPrefix + string.Format("&7Importing {0} blocks...", blockCount));
						//Reuse variables for better performance/memory usage
						int rx, ry, rz;
						string blockName;
						for (int i = 0; i < blockCount; i++)
						{
							//Read binary block data
							rx = br.ReadInt16();    //relative x position
							ry = br.ReadInt16();    //relative y position
							rz = br.ReadInt16();    //relative z position
							blockName = br.ReadString();    //Block name
							m.SetBlock(pos.x + rx, pos.y + ry, pos.z + rz, m.GetBlockId(blockName));    //Set new block
						}
					}
					System.Console.WriteLine("[Exporter] Import finished.");
					m.SendMessage(player, chatPrefix + "&aImport finished.");
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("[Exporter] ERROR:  " + ex.Message);
					m.SendMessage(player, chatPrefix + m.colorError() + "An error occured. Check console for details.");
					return true;
				}
				return true;
			}
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;

namespace ManicDigger.Mods
{
	public class BuildLog : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterOnBlockBuild(OnBuild);
			m.RegisterOnBlockDelete(OnDelete);
			m.RegisterOnLoad(OnLoad);
			m.RegisterOnSave(OnSave);
			m.SetGlobalDataNotSaved("LogLines", lines);
		}
		ModManager m;
		public int MaxEntries = 50 * 1000;

		//can't pass LogLine object between mods. Store object as an array of fields instead.
		List<object[]> lines = new List<object[]>();

		void OnLoad()
		{
			try
			{
				byte[] b = m.GetGlobalData("BuildLog");
				if (b != null)
				{
					MemoryStream ms = new MemoryStream(b);
					BinaryReader br = new BinaryReader(ms);
					int count = br.ReadInt32();
					for (int i = 0; i < count; i++)
					{
						var l = new object[8];
						l[0] = new DateTime(br.ReadInt64());//timestamp
						l[1] = br.ReadInt16();//x
						l[2] = br.ReadInt16();//y
						l[3] = br.ReadInt16();//z
						l[4] = br.ReadInt16();//blocktype
						l[5] = br.ReadBoolean();//build
						l[6] = br.ReadString();//playername
						l[7] = br.ReadString();//ip
						lines.Add(l);
					}
				}
			}
			catch
			{
				//corrupted
				OnSave();
			}
		}

		void OnSave()
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);
			bw.Write((int)lines.Count);
			for (int i = 0; i < lines.Count; i++)
			{
				object[] l = lines[i];
				bw.Write((long)((DateTime)l[0]).Ticks);//timestamp
				bw.Write((short)l[1]);//x
				bw.Write((short)l[2]);//y
				bw.Write((short)l[3]);//z
				bw.Write((short)l[4]);//blocktype
				bw.Write((bool)l[5]);//build
				bw.Write((string)l[6]);//playername
				bw.Write((string)l[7]);//ip
			}
			m.SetGlobalData("BuildLog", ms.ToArray());
		}

		void OnBuild(int player, int x, int y, int z)
		{
			lines.Add(new object[]
			          {
			          	DateTime.UtcNow,//timestamp
			          	(short)x, //x
			          	(short)y, //y
			          	(short)z, //z
			          	(short)m.GetBlock(x, y, z), //blocktype
			          	true, //build
			          	m.GetPlayerName(player),
			          	m.GetPlayerIp(player), //ip
			          });
			if (lines.Count > MaxEntries)
			{
				lines.RemoveRange(0, 1000);
			}
		}

		void OnDelete(int player, int x, int y, int z, int oldblock)
		{
			lines.Add(new object[]
			          {
			          	DateTime.UtcNow, //timestamp
			          	(short)x, //x
			          	(short)y, //y
			          	(short)z, //z
			          	(short)oldblock, //blocktype
			          	false, //build
			          	m.GetPlayerName(player), //playername
			          	m.GetPlayerIp(player), //ip
			          });
			if (lines.Count > MaxEntries)
			{
				lines.RemoveRange(0, 1000);
			}
		}
	}
}

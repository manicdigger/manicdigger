using System;
using System.Collections.Generic;

namespace ManicDigger.Mods
{
	public class VandalFinder : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
			m.RequireMod("BuildLog");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.SetBlockType("VandalFinder", new BlockType()
			               {
			               	AllTextures = "VandalFinder",
			               	DrawType = DrawType.Solid,
			               	WalkableType = WalkableType.Solid,
			               	IsUsable = true,
			               	IsTool = true,
			               });
			m.AddToCreativeInventory("VandalFinder");
			m.RegisterOnBlockUseWithTool(OnUseWithTool);
			lines = (List<object[]>)m.GetGlobalDataNotSaved("LogLines");
		}
		
		ModManager m;
		List<object[]> lines = new List<object[]>();
		
		void OnUseWithTool(int player, int x, int y, int z, int tool)
		{
			if (m.GetBlockName(tool) == "VandalFinder")
			{
				ShowBlockLog(player, x, y, z);
			}
		}
		
		void ShowBlockLog(int player, int x, int y, int z)
		{
			List<string> messages = new List<string>();
			for (int i = lines.Count - 1; i >= 0; i--)
			{
				object[] l = lines[i];
				int lx = (short)l[1];
				int ly = (short)l[2];
				int lz = (short)l[3];
				DateTime ltimestamp = (DateTime)l[0];
				string lplayername = (string)l[6];
				int lblocktype = (short)l[4];
				bool lbuild = (bool)l[5];
				if (lx == x && ly == y && lz == z)
				{
					messages.Add(string.Format("{0} {1} {2} {3}", ltimestamp.ToString(), lplayername, m.GetBlockName(lblocktype), lbuild ? "build" : "delete"));
					if (messages.Count > 10)
					{
						return;
					}
				}
			}
			messages.Reverse();
			for (int i = 0; i < messages.Count; i++)
			{
				m.SendMessage(player, messages[i]);
			}
			if (messages.Count == 0)
			{
				m.SendMessage(player, "Block was never changed.");
			}
		}
	}
}

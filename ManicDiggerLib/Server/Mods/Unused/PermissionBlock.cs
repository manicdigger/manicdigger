using System;

namespace ManicDigger.Mods
{
	public class PermissionBlock : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		
		public int PermissionLevelsCount = 4;
		public int AreaSize = 64;
		
		public void Start(ModManager manager)
		{
			m = manager;
			
			for (int i = 0; i < PermissionLevelsCount; i++)
			{
				m.SetBlockType(241 + i, "BuildPermission" + i, new BlockType()
				               {
				               	AllTextures = "BuildPermission" + i,
				               	DrawType = DrawType.Solid,
				               	WalkableType = WalkableType.Solid,
				               	IsBuildable = true,
				               });
				m.AddToCreativeInventory("BuildPermission" + i);
			}
			
			m.RegisterOnBlockBuild(OnBuild);
			m.RegisterOnBlockDelete(OnDelete);
		}
		ModManager m;
		
		void OnBuild(int playerid, int x, int y, int z)
		{
			int permissionblock = m.GetBlockId("BuildPermission0");
			
			//can't build any block in column
			for (int zz = 0; zz < m.GetMapSizeZ(); zz++)
			{
				if (zz == z)
				{
					continue;
				}
				for (int i = 0; i < PermissionLevelsCount; i++)
				{
					if (m.GetBlock(x, y, zz) == permissionblock + i)
					{
						m.SetBlock(x, y, z, 0);
						m.SendMessage(playerid, "You can't build in a column that contains permission block.");
						return;
					}
				}
			}
			
			//add area
			for (int i = 0; i < PermissionLevelsCount; i++)
			{
				if (m.GetBlock(x, y, z) == permissionblock + i)
				{
					if (m.GetPlayerPermissionLevel(playerid) <= i)
					{
						m.SendMessage(playerid, "No permission");
						m.SetBlock(x, y, z, 0);
						return;
					}
					m.AddPermissionArea(x - AreaSize, y - AreaSize, 0, x + AreaSize, y + AreaSize, m.GetMapSizeZ(), i);
				}
			}
		}
		
		void OnDelete(int playerid, int x, int y, int z, int oldblock)
		{
			int permissionblock = m.GetBlockId("BuildPermission0");
			
			//remove area
			for (int i = 0; i < PermissionLevelsCount; i++)
			{
				if (oldblock == permissionblock + i)
				{
					if (m.GetPlayerPermissionLevel(playerid) <= i)
					{
						m.SendMessage(playerid, "No permission");
						m.SetBlock(x, y, z, oldblock);
						return;
					}
					m.RemovePermissionArea(x - AreaSize, y - AreaSize, 0, x + AreaSize, y + AreaSize, m.GetMapSizeZ());
				}
			}
		}
	}
}
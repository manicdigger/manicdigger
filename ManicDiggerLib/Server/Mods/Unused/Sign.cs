using System;
using System.Collections.Generic;

namespace ManicDigger.Mods
{
	public class Sign : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.SetBlockType(154, "Sign", new BlockType()
			               {
			               	AllTextures = "Sign",
			               	DrawType = DrawType.Solid,
			               	WalkableType = WalkableType.Solid,
			               	IsUsable = true,
			               	IsTool = true,
			               });
			m.AddToCreativeInventory("Sign");
			m.SetBlockType(155, "PermissionSign", new BlockType()
			               {
			               	AllTextures = "PermissionSign",
			               	DrawType = DrawType.Solid,
			               	WalkableType = WalkableType.Solid,
			               	IsUsable = true,
			               	IsTool = true,
			               });
			m.AddToCreativeInventory("PermissionSign");
		}

		ModManager m;
	}
}

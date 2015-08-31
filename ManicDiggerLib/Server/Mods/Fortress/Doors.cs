using System;

namespace ManicDigger.Mods
{
	public class Doors : IMod
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
			m.RegisterOnBlockUse(OnUse);

			m.SetString("en", "DoorBottomClosed", "Closed Door");
			m.SetString("en", "DoorTopClosed", "Closed Door");
			m.SetString("en", "DoorBottomOpen", "Open Door");
			m.SetString("en", "DoorTopOpen", "Open Door");

			SoundSet sounds = new SoundSet()
			{
				Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
				Break = new string[] { "destruct" },
				Build = new string[] { "build" },
				Clone = new string[] { "clone" },
			};

			m.SetBlockType(126, "DoorBottomClosed", new BlockType()
			               {
			               	AllTextures = "DoorBottom",
			               	DrawType = DrawType.ClosedDoor,
			               	WalkableType = WalkableType.Solid,
			               	Sounds = sounds,
			               	IsUsable = true,
			               });
			m.SetBlockType(127, "DoorTopClosed", new BlockType()
			               {
			               	AllTextures = "DoorTop",
			               	DrawType = DrawType.ClosedDoor,
			               	WalkableType = WalkableType.Solid,
			               	Sounds = sounds,
			               	IsUsable = true,
			               	WhenPlayerPlacesGetsConvertedTo = 126,
			               });
			m.SetBlockType(128, "DoorBottomOpen", new BlockType()
			               {
			               	AllTextures = "DoorBottom",
			               	DrawType = DrawType.OpenDoorLeft,
			               	WalkableType = WalkableType.Empty,
			               	Sounds = sounds,
			               	IsUsable = true,
			               	WhenPlayerPlacesGetsConvertedTo = 126,
			               });
			m.SetBlockType(129, "DoorTopOpen", new BlockType()
			               {
			               	AllTextures = "DoorTop",
			               	DrawType = DrawType.OpenDoorLeft,
			               	WalkableType = WalkableType.Empty,
			               	Sounds = sounds,
			               	IsUsable = true,
			               	WhenPlayerPlacesGetsConvertedTo = 126,
			               });

			m.AddToCreativeInventory("DoorBottomClosed");
			m.AddCraftingRecipe("DoorBottomClosed", 1, "OakWood", 2);
			m.AddCraftingRecipe("DoorBottomClosed", 1, "BirchWood", 2);
			m.AddCraftingRecipe("DoorBottomClosed", 1, "SpruceWood", 2);

			DoorBottomClosed = m.GetBlockId("DoorBottomClosed");
			DoorTopClosed = m.GetBlockId("DoorTopClosed");
			DoorBottomOpen = m.GetBlockId("DoorBottomOpen");
			DoorTopOpen = m.GetBlockId("DoorTopOpen");
		}

		ModManager m;
		int DoorBottomClosed;
		int DoorTopClosed;
		int DoorBottomOpen;
		int DoorTopOpen;

		void OnBuild(int player, int x, int y, int z)
		{
			//Check if placed block is bottom part of door (no need for further checks as player only has this type of block)
			if (m.GetBlock(x, y, z) == DoorBottomClosed)
			{
				//check if block above is valid and empty
				if (m.IsValidPos(x, y, z + 1) && m.GetBlock(x, y, z + 1) == 0)
				{
					m.SetBlock(x, y, z + 1, DoorTopClosed);
				}
				//if not, try to move door down 1 block
				else if (m.IsValidPos(x, y, z - 1) && m.GetBlock(x, y, z - 1) == 0)
				{
					m.SetBlock(x, y, z, DoorTopClosed);
					m.SetBlock(x, y, z - 1, DoorBottomClosed);
				}
				//if this fails, give the player back the block he built (survival mode) and set current block to empty
				else
				{
					m.SetBlock(x, y, z, 0);
					m.GrabBlock(player, DoorBottomClosed);
				}
			}
		}

		void OnDelete(int player, int x, int y, int z, int block)
		{
			if (m.IsValidPos(x, y, z + 1) && (m.GetBlock(x, y, z + 1) == DoorTopClosed || m.GetBlock(x, y, z + 1) == DoorTopOpen))
			{
				m.SetBlock(x, y, z + 1, 0);
			}
			if (m.IsValidPos(x, y, z - 1) && (m.GetBlock(x, y, z - 1) == DoorBottomOpen || m.GetBlock(x, y, z - 1) == DoorBottomClosed))
			{
				m.SetBlock(x, y, z - 1, 0);
			}
		}

		void OnUse(int player, int x, int y, int z)
		{
			//Closed door - bottom part
			if (m.GetBlock(x, y, z) == DoorBottomClosed)
			{
				//check block above
				if (m.GetBlock(x, y, z + 1) == DoorTopClosed)
				{
					//Modify blocks if there is a door counterpart
					m.SetBlock(x, y, z, DoorBottomOpen);
					m.SetBlock(x, y, z + 1, DoorTopOpen);
				}
				else
				{
					//delete used block as it is a leftover
					m.SetBlock(x, y, z, 0);
					m.GrabBlock(player, DoorBottomClosed);
				}
			}

			//Open door - bottom part
			else if (m.GetBlock(x, y, z) == DoorBottomOpen)
			{
				//check block above
				if (m.GetBlock(x, y, z + 1) == DoorTopOpen)
				{
					//Modify blocks if there is a door counterpart
					m.SetBlock(x, y, z, DoorBottomClosed);
					m.SetBlock(x, y, z + 1, DoorTopClosed);
				}
				else
				{
					//delete used block as it is a leftover
					m.SetBlock(x, y, z, 0);
					m.GrabBlock(player, DoorBottomClosed);
				}
			}

			//Closed door - top part
			else if (m.GetBlock(x, y, z) == DoorTopClosed)
			{
				//check block under used one
				if (m.GetBlock(x, y, z - 1) == DoorBottomClosed)
				{
					m.SetBlock(x, y, z, DoorTopOpen);
					m.SetBlock(x, y, z - 1, DoorBottomOpen);
				}
				else
				{
					//delete used block as it is a leftover
					m.SetBlock(x, y, z, 0);
					m.GrabBlock(player, DoorBottomClosed);
				}
			}

			//Open door - top part
			else if (m.GetBlock(x, y, z) == DoorTopOpen)
			{
				//check block under used one
				if (m.GetBlock(x, y, z - 1) == DoorBottomOpen)
				{
					m.SetBlock(x, y, z, DoorTopClosed);
					m.SetBlock(x, y, z - 1, DoorBottomClosed);
				}
				else
				{
					//delete used block as it is a leftover
					m.SetBlock(x, y, z, 0);
					m.GrabBlock(player, DoorBottomClosed);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace ManicDigger.Mods
{
	public class Tnt : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			SoundSet solidSounds = new SoundSet()
			{
				Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
				Break = new string[] { "destruct" },
				Build = new string[] { "build" },
				Clone = new string[] { "clone" },
			};
			m.SetBlockType(46, "TNT", new BlockType()
			               {
			               	TextureIdTop = "TNTTop",
			               	TextureIdBottom = "TNTBottom",
			               	TextureIdBack = "TNT",
			               	TextureIdFront = "TNT",
			               	TextureIdLeft = "TNT",
			               	TextureIdRight = "TNT",
			               	TextureIdForInventory = "TNT",
			               	DrawType = DrawType.Solid,
			               	WalkableType = WalkableType.Solid,
			               	Sounds = solidSounds,
			               	IsUsable = true,
			               });
			tnt = m.GetBlockId("TNT");
			adminium = m.GetBlockId("Adminium");
			m.AddToCreativeInventory("TNT");
			m.AddCraftingRecipe("TNT", 1, "GoldBlock", 1);
			m.RegisterOnBlockUse(UseTnt);
			m.RegisterPrivilege("use_tnt");
			m.RegisterTimer(UpdateTnt, 5);
		}
		ModManager m;
		int tnt;
		int adminium;
		Random rnd = new Random();
		
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
		
		private void UseTnt(int player, int x, int y, int z)
		{
			if (m.GetBlock(x, y, z) == tnt)
			{
				if (!m.PlayerHasPrivilege(player, "use_tnt"))
				{
					m.SendMessage(player, m.colorError() + "Insufficient privileges to use TNT.");
					return;
				}
				if (tntStack.Count < tntMax)
				{
					tntStack.Push(new Vector3i(x, y, z));
				}
			}
		}
		
		private void UpdateTnt()
		{
			int now = 0;
			while (now++ < 3)
			{
				if (tntStack.Count == 0)
				{
					return;
				}
				Vector3i pos = tntStack.Pop();
				int nearestplayer = m.NearestPlayer(pos.x, pos.y, pos.z);
				m.PlaySoundAt(pos.x, pos.y, pos.z, "tnt.wav");
				
				for (int xx = 0; xx < tntRange; xx++)
				{
					for (int yy = 0; yy < tntRange; yy++)
					{
						for (int zz = 0; zz < tntRange; zz++)
						{
							if (sphereEq(xx - (tntRange - 1) / 2, yy - (tntRange - 1) / 2, zz - (tntRange - 1) / 2, tntRange / 2) <= 0)
							{
								Vector3i pos2 = new Vector3i(pos.x + xx - tntRange / 2,
								                             pos.y + yy - tntRange / 2,
								                             pos.z + zz - tntRange / 2);
								if (!m.IsValidPos(pos2.x, pos2.y, pos2.z))
								{
									continue;
								}
								int block = m.GetBlock(pos2.x, pos2.y, pos2.z);
								if (tntStack.Count < tntMax
								    && (pos2.x != pos.x || pos2.y != pos.y || pos2.z != pos.z)
								    && block == tnt)
								{
									tntStack.Push(pos2);
								}
								else
								{
									if ((block != 0)
									    && (block != adminium)
									    && !(m.IsBlockFluid(block)))
									{
										m.SetBlock(pos2.x, pos2.y, pos2.z, 0);
										if (!m.IsCreative())
										{
											// chance to get some of destruced blocks
											if (rnd.NextDouble() < .20f)
											{
												if (nearestplayer != -1)
												{
													m.GrabBlock(nearestplayer, block);
												}
											}
										}
									}
								}
							}
						}
					}
				}
				m.NotifyInventory(nearestplayer);
			}
		}
		private int sphereEq(int x, int y, int z, int r)
		{
			return x * x + y * y + z * z - r * r;
		}
		
		public int tntRange = 10; // sphere diameter
		Stack<Vector3i> tntStack = new Stack<Vector3i>();
		public int tntMax = 10;
	}
}

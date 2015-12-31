using System;

namespace ManicDigger.Mods
{
	public class VegetationGrowth : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			DirtForFarming = m.GetBlockId("DirtForFarming");
			OakSapling = m.GetBlockId("OakSapling");
			BirchSapling = m.GetBlockId("BirchSapling");
			SpruceSapling = m.GetBlockId("SpruceSapling");
			BrownMushroom = m.GetBlockId("BrownMushroom");
			RedMushroom = m.GetBlockId("RedMushroom");
			YellowFlowerDecorations = m.GetBlockId("YellowFlowerDecorations");
			RedRoseDecorations = m.GetBlockId("RedRoseDecorations");
			Dirt = m.GetBlockId("Dirt");
			Grass = m.GetBlockId("Grass");
			Crops1 = m.GetBlockId("Crops1");
			Crops2 = m.GetBlockId("Crops2");
			Crops3 = m.GetBlockId("Crops3");
			Crops4 = m.GetBlockId("Crops4");
			OakLeaves = m.GetBlockId("OakLeaves");
			BirchLeaves = m.GetBlockId("BirchLeaves");
			SpruceLeaves = m.GetBlockId("SpruceLeaves");
			Apples = m.GetBlockId("Apples");
			OakTreeTrunk = m.GetBlockId("OakTreeTrunk");
			BirchTreeTrunk = m.GetBlockId("BirchTreeTrunk");
			SpruceTreeTrunk = m.GetBlockId("SpruceTreeTrunk");

			m.RegisterOnBlockUpdate(BlockTickGrowCropsCycle);
			m.RegisterOnBlockUpdate(BlockTickGrowSapling);
			m.RegisterOnBlockUpdate(BlockTickMushroomDeath);
			m.RegisterOnBlockUpdate(BlockTickFlowerDeath);
			m.RegisterOnBlockUpdate(BlockTickGrowGrassOrMushroomsOnDirt);
			m.RegisterOnBlockUpdate(BlockTickGrassDeathInDarkness);
		}

		ModManager m;
		Random rnd = new Random();
		int DirtForFarming;
		int OakSapling;
		int BirchSapling;
		int SpruceSapling;
		int BrownMushroom;
		int RedMushroom;
		int YellowFlowerDecorations;
		int RedRoseDecorations;
		int Dirt;
		int Grass;
		int Crops1;
		int Crops2;
		int Crops3;
		int Crops4;
		int OakLeaves;
		int BirchLeaves;
		int SpruceLeaves;
		int Apples;
		int OakTreeTrunk;
		int BirchTreeTrunk;
		int SpruceTreeTrunk;

		void BlockTickGrowCropsCycle(int x, int y, int z)
		{
			if (m.GetBlock(x, y, z) == DirtForFarming)
			{
				if (m.IsValidPos(x, y, z + 1))
				{
					int blockabove = m.GetBlock(x, y, z + 1);
					if (blockabove == Crops1) { blockabove = Crops2; }
					else if (blockabove == Crops2) { blockabove = Crops3; }
					else if (blockabove == Crops3) { blockabove = Crops4; }
					else { return; }
					m.SetBlock(x, y, z + 1, blockabove);
				}
			}
		}

		void BlockTickGrowSapling(int x, int y, int z)
		{
			switch (GetTreeType(m.GetBlock(x, y, z)))
			{
				case TreeType.Oak:
					if (IsShadow(x, y, z) || !BlockSupportsSapling(x, y, z - 1))
					{
						return;
					}
					MakeTree(x, y, z, OakTreeTrunk, OakLeaves, true);
					break;

				case TreeType.Birch:
					if (IsShadow(x, y, z) || !BlockSupportsSapling(x, y, z - 1))
					{
						return;
					}
					MakeTree(x, y, z, BirchTreeTrunk, BirchLeaves, false);
					break;

				case TreeType.Spruce:
					if (IsShadow(x, y, z) || !BlockSupportsSapling(x, y, z - 1))
					{
						return;
					}
					MakeTree(x, y, z, SpruceTreeTrunk, SpruceLeaves, false);
					break;
			}
		}

		/// <summary>
		/// Determines the tree type from given block ID.
		/// </summary>
		/// <param name="id_sapling">Block ID to check</param>
		/// <returns>TreeType of the given sapling block. TreeType.None when given id is not a sapling.</returns>
		TreeType GetTreeType(int id_sapling)
		{
			if (id_sapling == OakSapling)
			{
				return TreeType.Oak;
			}
			if (id_sapling == BirchSapling)
			{
				return TreeType.Birch;
			}
			if (id_sapling == SpruceSapling)
			{
				return TreeType.Spruce;
			}
			return TreeType.None;
		}

		/// <summary>
		/// Checks if the block at the given position is a valid block for a tree to grow on
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <returns>true if position is valid and block is either Grass, Dirt or DirtForFarming</returns>
		bool BlockSupportsSapling(int x, int y, int z)
		{
			if (!m.IsValidPos(x, y, z))
			{
				return false;
			}
			int block = m.GetBlock(x, y, z);
			if (!(block == Dirt || block == Grass || block == DirtForFarming))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Places a generic tree at the given position
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="trunkID">Block ID to use for the tree trunk</param>
		/// <param name="leavesID">Block ID to use for the leaves</param>
		void PlaceTree(int x, int y, int z, int trunkID, int leavesID)
		{
			PlaceIfEmpty(x, y, z + 1, trunkID);
			PlaceIfEmpty(x, y, z + 2, trunkID);
			PlaceIfEmpty(x, y, z + 3, trunkID);

			PlaceIfEmpty(x + 1, y, z + 3, leavesID);
			PlaceIfEmpty(x - 1, y, z + 3, leavesID);
			PlaceIfEmpty(x, y + 1, z + 3, leavesID);
			PlaceIfEmpty(x, y - 1, z + 3, leavesID);

			PlaceIfEmpty(x + 1, y + 1, z + 3, leavesID);
			PlaceIfEmpty(x + 1, y - 1, z + 3, leavesID);
			PlaceIfEmpty(x - 1, y + 1, z + 3, leavesID);
			PlaceIfEmpty(x - 1, y - 1, z + 3, leavesID);

			PlaceIfEmpty(x + 1, y, z + 4, leavesID);
			PlaceIfEmpty(x - 1, y, z + 4, leavesID);
			PlaceIfEmpty(x, y + 1, z + 4, leavesID);
			PlaceIfEmpty(x, y - 1, z + 4, leavesID);

			PlaceIfEmpty(x, y, z + 4, OakLeaves);
		}

		/// <summary>
		/// Places the given block if the target block is empty or any sapling
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="blocktype">Block ID to place</param>
		void PlaceIfEmpty(int x, int y, int z, int blocktype)
		{
			int block = m.GetBlock(x, y, z);
			if (m.IsValidPos(x, y, z) && (block == 0 || block == OakSapling || block == BirchSapling || block == SpruceSapling))
			{
				m.SetBlock(x, y, z, blocktype);
			}
		}

		void BlockTickGrowGrassOrMushroomsOnDirt(int x, int y, int z)
		{
			if (m.GetBlock(x, y, z) == Dirt)
			{
				if (m.IsValidPos(x, y, z + 1))
				{
					int roofBlock = m.GetBlock(x, y, z + 1);
					if (m.IsTransparentForLight(roofBlock))
					{
						if (IsShadow(x, y, z) && !reflectedSunnyLight(x, y, z))
						{
							// if 1% chance happens then 1 mushroom will grow up
							if (rnd.NextDouble() < 0.01)
							{
								int tile = rnd.NextDouble() < 0.6 ? RedMushroom : BrownMushroom;
								PlaceIfEmpty(x, y, z + 1, tile);
							}
						}
						else
						{
							m.SetBlock(x, y, z, Grass);
						}
					}
				}
			}
		}

		void BlockTickGrassDeathInDarkness(int x, int y, int z)
		{
			if (m.GetBlock(x, y, z) == Grass)
			{
				if (IsShadow(x, y, z) && !(reflectedSunnyLight(x, y, z) && m.IsTransparentForLight(m.GetBlock(x, y, z + 1))))
				{
					m.SetBlock(x, y, z, Dirt);
				}
			}
		}

		void MakeTree(int cx, int cy, int cz, int id_trunk, int id_leaves, bool isAppleTree)
		{
			int x = cx;
			int y = cy;
			int z = cz;
			int TileIdLeaves = id_leaves;
			int TileIdApples = Apples;
			int TileIdTreeTrunk = id_trunk;
			int treeHeight = rnd.Next(4, 6);
			int xx = 0;
			int yy = 0;
			int dir = 0;

			for (int i = 0; i < treeHeight; i++)
			{
				PlaceIfEmpty(x, y, z + i, TileIdTreeTrunk);
				if (i == treeHeight - 1)
				{
					for (int j = 1; j < 9; j++)
					{
						dir += 45;
						for (int k = 1; k < 2; k++)
						{
							int length = dir % 90 == 0 ? k : (int)(k / 2);
							xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
							yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

							PlaceIfEmpty(x + xx, y + yy, z + i, TileIdTreeTrunk);
							float appleChance = isAppleTree ? 0.45f : 0f;
							int tile;
							tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy, z + i + 1, tile);
							tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx + 1, y + yy, z + i, tile);
							tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx - 1, y + yy, z + i, tile);
							tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy + 1, z + i, tile);
							tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy - 1, z + i, tile);
						}
					}
				}
			}
		}

		// mushrooms will die when they have not shadow or dirt, or 20% chance happens
		void BlockTickMushroomDeath(int x, int y, int z)
		{
			int block = m.GetBlock(x, y, z);
			if (block == BrownMushroom || block == RedMushroom)
			{
				if (rnd.NextDouble() < 0.2) { m.SetBlock(x, y, z, 0); return; }
				if (!IsShadow(x, y, z - 1))
				{
					m.SetBlock(x, y, z, 0);
				}
				else
				{
					if (m.GetBlock(x, y, z - 1) == Dirt) return;
					m.SetBlock(x, y, z, 0);
				}
			}
		}

		// floowers will die when they have no light, dirt or grass or 2% chance happens
		void BlockTickFlowerDeath(int x, int y, int z)
		{
			int block = m.GetBlock(x, y, z);
			if (block == YellowFlowerDecorations || block == RedRoseDecorations)
			{
				if (rnd.NextDouble() < 0.02) { m.SetBlock(x, y, z, 0); return; }
				if (IsShadow(x, y, z - 1))
				{
					m.SetBlock(x, y, z, 0);
				}
				else
				{
					int under = m.GetBlock(x, y, z - 1);
					if ((under == Dirt
					     || under == Grass)) return;
					m.SetBlock(x, y, z, 0);
				}
			}
		}

		bool IsShadow(int x, int y, int z)
		{
			for (int i = 1; i < 10; i++)
			{
				if (m.IsValidPos(x, y, z + i) && !m.IsTransparentForLight(m.GetBlock(x, y, z + i)))
				{
					return true;
				}
			}
			return false;
		}

		// The true if on a cube gets the sunlight reflected from another cubes
		bool reflectedSunnyLight(int x, int y, int z)
		{
			for (int i = x - 2; i <= x + 2; i++)
				for (int j = y - 2; j <= y + 2; j++)
			{
				if (!IsShadow(i, j, z))
				{
					return true;
				}
			}
			return false;
		}

		enum TreeType
		{
			None,
			Oak,
			Birch,
			Spruce
		}
	}
}

using System;

namespace ManicDigger.Mods
{
	public class OreGenerator : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			TileIdStone = m.GetBlockId("Stone");
			TileIdGravel = m.GetBlockId("Gravel");
			TileIdDirt = m.GetBlockId("Dirt");
			TileIdGrass = m.GetBlockId("Grass");
			TileIdGoldOre = m.GetBlockId("GoldOre");
			TileIdIronOre = m.GetBlockId("IronOre");
			TileIdSilverOre = m.GetBlockId("SilverOre");
			TileIdSand = m.GetBlockId("Sand");
			TileIdCoalOre = m.GetBlockId("CoalOre");
			m.RegisterPopulateChunk(PopulateChunk);
		}
		ModManager m;
		Random _rnd = new Random();
		
		int TileIdStone;
		int TileIdGravel;
		int TileIdDirt;
		int TileIdGrass;
		int TileIdGoldOre;
		int TileIdIronOre;
		int TileIdSilverOre;
		int TileIdSand;
		int TileIdCoalOre;
		
		void PopulateChunk(int x, int y, int z)
		{
			x *= m.GetChunkSize();
			y *= m.GetChunkSize();
			z *= m.GetChunkSize();
			
			MakeCaves(x, y, z, m.GetChunkSize(), _rnd, this.EnableCaves, gravellength, goldorelength, ironorelength, coalorelength, dirtlength, silverlength);
		}
		
		public bool EnableCaves = false;
		public int goldorelength = 50;
		public int ironorelength = 50;
		public int coalorelength = 50;
		public int gravellength = 50;
		public int silverlength = 50;
		public int dirtlength = 40;
		
		void MakeCaves(int x, int y, int z, int chunksize, Random rnd,
		               bool enableCaves,
		               int gravelLength,
		               int goldOreLength,
		               int ironOreLength,
		               int coalOreLength,
		               int dirtOreLength,
		               int silverOreLength)
		{
			//find cave start
			double curx = x;
			double cury = y;
			double curz = z;
			for (int i = 0; i < 2; i++)
			{
				curx = x + rnd.Next(chunksize);
				cury = y + rnd.Next(chunksize);
				curz = z + rnd.Next(chunksize);
				if (m.GetBlock((int)curx, (int)cury, (int)curz) == TileIdStone)
				{
					goto ok;
				}
			}
			return;
		ok:
			int blocktype = 0;
			int length = 200;
			if (rnd.NextDouble() < 0.85)
			{
				int oretype = rnd.Next(6);
				if (oretype == 0) { length = gravelLength; }
				if (oretype == 1) { length = goldOreLength; }
				if (oretype == 2) { length = ironOreLength; }
				if (oretype == 3) { length = coalOreLength; }
				if (oretype == 4) { length = dirtOreLength; }
				if (oretype == 5) { length = silverOreLength; }
				
				length = rnd.Next(length);
				blocktype = oretype < 4 ? TileIdGravel + oretype : (oretype > 4 ? TileIdGravel + oretype + 115 : TileIdDirt);
			}
			if (blocktype == 0 && (!enableCaves))
			{
				return;
			}
			int dirx = rnd.NextDouble() < 0.5 ? -1 : 1;
			int dirz = rnd.NextDouble() < 0.5 ? -1 : 1;
			double curspeedx = rnd.NextDouble() * dirx;
			double curspeedy = rnd.NextDouble();
			double curspeedz = rnd.NextDouble() * 0.5 * dirz;
			for (int i = 0; i < length; i++)
			{
				if (rnd.NextDouble() < 0.06)
				{
					curspeedx = rnd.NextDouble() * dirx;
				}
				if (rnd.NextDouble() < 0.06)
				{
					curspeedy = rnd.NextDouble() * dirx;
				}
				if (rnd.NextDouble() < 0.02)
				{
					curspeedz = rnd.NextDouble() * 0.5 * dirz;
				}
				curx += curspeedx;
				cury += curspeedy;
				curz += curspeedz;
				if (!m.IsValidPos((int)curx, (int)cury, (int)curz))
				{
					continue;
				}
				for (int ii = 0; ii < 3; ii++)
				{
					int sizex = rnd.Next(3, 6);
					int sizey = rnd.Next(3, 6);
					int sizez = rnd.Next(2, 3);
					int dx = rnd.Next(-sizex / 2, sizex / 2);
					int dy = rnd.Next(-sizey / 2, sizey / 2);
					int dz = rnd.Next(-sizez / 1, sizez / 1);
					
					int[] allowin = new int[] { TileIdStone };
					double density = blocktype == 0 ? 1 : rnd.NextDouble() * 0.90;
					if (blocktype == 0)
					{
						allowin = new int[] {
							TileIdStone,
							TileIdDirt,
							TileIdGrass,
							TileIdGoldOre,
							TileIdIronOre,
							TileIdCoalOre,
							TileIdSilverOre
						};
					}
					if (blocktype == TileIdGravel)
					{
						density = 1;
						allowin = new int[] {
							TileIdDirt,
							TileIdStone,
							TileIdSand,
							TileIdGoldOre,
							TileIdIronOre,
							TileIdCoalOre,
							TileIdSilverOre
						};
					}
					
					MakeCuboid((int)curx - sizex / 2 + dx, (int)cury - sizey / 2 + dy, (int)curz - sizez / 2 + dz, sizex, sizey, sizez, blocktype, allowin, density, rnd);
				}
			}
		}
		void MakeCuboid(int x, int y, int z, int sizex, int sizey, int sizez, int blocktype, int[] allowin, double chance, Random rnd)
		{
			for (int xx = 0; xx < sizex; xx++)
			{
				for (int yy = 0; yy < sizey; yy++)
				{
					for (int zz = 0; zz < sizez; zz++)
					{
						if (m.IsValidPos(x + xx, y + yy, z + zz))
						{
							if ((z + zz) == 0)
							{
								//Skip bottom layer of map to prevent holes
								continue;
							}
							int t = m.GetBlock(x + xx, y + yy, z + zz);
							if (allowin == null) { goto ok; }
							foreach (int tt in allowin)
							{
								if (tt == t) { goto ok; }
							}
							continue;
						ok:
							if (rnd.NextDouble() < chance)
							{
								m.SetBlock(x + xx, y + yy, z + zz, blocktype);
							}
						}
					}
				}
			}
		}
	}
}

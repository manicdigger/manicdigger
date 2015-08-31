using System;
using LibNoise;
using LibNoise.Modifiers;

namespace ManicDigger.Mods
{
	public class DefaultWorldGenerator : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterWorldGenerator(GetChunk);
			m.RegisterOptionBool("DefaultGenCaves", false);
			m.RegisterOptionBool("DefaultGenLavaCaves", false);

			BLOCK_STONE = m.GetBlockId ("Stone");
			BLOCK_DIRT = m.GetBlockId ("Dirt");
			BLOCK_SAND = m.GetBlockId ("Sand");
			BLOCK_CLAY = m.GetBlockId ("Stone");
			BLOCK_BEDROCK = m.GetBlockId ("Adminium");
			BLOCK_AIR = m.GetBlockId ("Empty");
			BLOCK_SNOW = m.GetBlockId ("Grass");
			BLOCK_ICE = m.GetBlockId ("Water");
			BLOCK_GRASS = m.GetBlockId ("Grass");
			BLOCK_WATER = m.GetBlockId ("Water");
			BLOCK_GRAVEL = m.GetBlockId ("Gravel");
			BLOCK_PUMPKIN = m.GetBlockId ("Hay");
			BLOCK_RED_ROSE = m.GetBlockId ("RedRoseDecorations");
			BLOCK_YELLOW_FLOWER = m.GetBlockId ("YellowFlowerDecorations");
			BLOCK_LAVA = m.GetBlockId ("Lava");
		}

		ModManager m;

		bool started = false;
		public void GetChunk(int x, int y, int z, ushort[] chunk)
		{
			if (!started)
			{
				Init();
				started = true;
			}
			bool addCaves = (bool)m.GetOption("DefaultGenCaves");
			bool addCaveLava = (bool)m.GetOption("DefaultGenLavaCaves");
			int ChunkSize=m.GetChunkSize();
			x *= ChunkSize;
			y *= ChunkSize;
			z *= ChunkSize;
			int chunksize = ChunkSize;
			var noise = new LibNoise.FastNoise();
			noise.Frequency = 0.01;

			for (int xx = 0; xx < chunksize; xx++)
			{
				for (int yy = 0; yy < chunksize; yy++)
				{
					int currentHeight = (byte)((finalTerrain.GetValue((xx + x) / 100.0, 0, (yy + y) / 100.0) * 60) + 64);
					int ymax = currentHeight;
					
					int biome = (int)(BiomeSelect.GetValue((x + xx) / 100.0, 0, (y + yy) / 100.0) * 2); //MD * 2
					int toplayer = BLOCK_DIRT;
					if (biome == 0)
					{
						toplayer = BLOCK_DIRT;
					}
					if (biome == 1)
					{
						toplayer = BLOCK_SAND;
					}
					if (biome == 2)
					{
						toplayer = BLOCK_DIRT;
					}
					if (biome == 3)
					{
						toplayer = BLOCK_DIRT;
					}
					if (biome == 4)
					{
						toplayer = BLOCK_DIRT;
					}
					if (biome == 5)
					{
						toplayer = BLOCK_CLAY;
					}
					
					int stoneHeight = (int)currentHeight - ((64 - (currentHeight % 64)) / 8) + 1;
					
					if (ymax < seaLevel)
					{
						ymax = seaLevel;
					}
					ymax++;
					if (ymax > z + chunksize - 1)
					{
						ymax = z + chunksize - 1;
					}
					for (int bY = z; bY <= ymax; bY++)
					{
						int curBlock = 0;

						// Place bedrock
						if (bY == 0)
						{
							curBlock = BLOCK_BEDROCK;
						}
						else if (bY < currentHeight)
						{
							if (bY < stoneHeight)
							{
								curBlock = BLOCK_STONE;
								// Add caves
								if (addCaves)
								{
									if (caveNoise.GetValue((x + xx) / 4.0, (bY) / 1.5, (y + yy) / 4.0) > cavestreshold)
									{
										if (bY < 10 && addCaveLava)
										{
											curBlock = BLOCK_LAVA;
										}
										else
										{
											curBlock = BLOCK_AIR;
										}
									}
								}
							}
							else
							{
								curBlock = toplayer;
							}
						}
						else if ((currentHeight + 1) == bY && bY > seaLevel && biome == 3)
						{
							curBlock = BLOCK_SNOW;
							continue;
						}
						else if ((currentHeight + 1) == bY && bY > seaLevel + 1)
						{
							if (biome == 1 || biome == 0)
							{
								continue;
							}
							double f = flowers.GetValue(x + xx / 10.0, 0, y + yy / 10.0);
							if (f < -0.999)
							{
								curBlock = BLOCK_RED_ROSE;
							}
							else if (f > 0.999)
							{
								curBlock = BLOCK_YELLOW_FLOWER;
							}
							else if (f < 0.001 && f > -0.001)
							{
								curBlock = BLOCK_PUMPKIN;
							}
						}
						else if (currentHeight == bY)
						{
							if (bY == seaLevel || bY == seaLevel - 1 || bY == seaLevel - 2)
							{
								curBlock = BLOCK_SAND;  // FF
							}
							else if (bY < seaLevel - 1)
							{
								curBlock = BLOCK_GRAVEL;  // FF
							}
							else if (toplayer == BLOCK_DIRT)
							{
								curBlock = BLOCK_GRASS;
							}
							else
							{
								curBlock = toplayer; // FF
							}
						}
						else
						{
							if (bY <= seaLevel)
							{
								curBlock = BLOCK_WATER;  // FF
							}
							else
							{
								curBlock = BLOCK_AIR;  // FF
							}
							if (bY == seaLevel && biome == 3)
							{
								curBlock = BLOCK_ICE;
							}
						}
						chunk[m.Index3d(xx, yy, bY - z, chunksize, chunksize)] = (ushort)curBlock;
					}
				}
			}
		}
		int seaLevel = 62;
		int BLOCK_STONE;
		int BLOCK_DIRT;
		int BLOCK_SAND;
		int BLOCK_CLAY; //stone
		int BLOCK_BEDROCK;
		int BLOCK_AIR;
		int BLOCK_SNOW; //todo
		int BLOCK_ICE; //todo
		int BLOCK_GRASS;
		int BLOCK_WATER;
		int BLOCK_GRAVEL;
		int BLOCK_PUMPKIN; //hay
		int BLOCK_RED_ROSE;
		int BLOCK_YELLOW_FLOWER;
		int BLOCK_LAVA;

		public void Init()
		{
			int Seed = m.GetSeed();

			BiomeBase.Frequency = (0.2);
			BiomeBase.Seed = (Seed - 1);
			BiomeSelect = new ScaleBiasOutput(BiomeBase);
			BiomeSelect.Scale = (2.5);
			BiomeSelect.Bias = (2.5);
			mountainTerrainBase.Seed = (Seed + 1);
			mountainTerrain = new ScaleBiasOutput(mountainTerrainBase);
			mountainTerrain.Scale = (0.5);
			mountainTerrain.Bias = (0.5);
			jaggieEdges = new Select(jaggieControl, terrainType, plain);
			plain.Value = (0.5);
			jaggieEdges.SetBounds(0.5, 1.0);
			jaggieEdges.EdgeFalloff = (0.11);
			jaggieControl.Seed = (Seed + 20);
			baseFlatTerrain.Seed = (Seed);
			baseFlatTerrain.Frequency = (0.2);
			flatTerrain = new ScaleBiasOutput(baseFlatTerrain);
			flatTerrain.Scale = (0.125);
			flatTerrain.Bias = (0.07);
			baseWater.Seed = (Seed - 1);
			water = new ScaleBiasOutput(baseWater);
			water.Scale = (0.3);
			water.Bias = (-0.5);
			terrainType.Seed = (Seed + 2);
			terrainType.Frequency = (0.5);
			terrainType.Persistence = (0.25);
			terrainType2.Seed = (Seed + 7);
			terrainType2.Frequency = (0.5);
			terrainType2.Persistence = (0.25);
			waterTerrain = new Select(terrainType2, water, flatTerrain);
			waterTerrain.EdgeFalloff = (0.1);
			waterTerrain.SetBounds(-0.5, 1.0);
			secondTerrain = new Select(terrainType, mountainTerrain, waterTerrain);
			secondTerrain.EdgeFalloff = (0.3);
			secondTerrain.SetBounds(-0.5, 1.0);
			finalTerrain = new Select(jaggieEdges, secondTerrain, waterTerrain);
			finalTerrain.EdgeFalloff = (0.2);
			finalTerrain.SetBounds(-0.3, 1.0);
			flowers.Seed = (Seed + 10);
			flowers.Frequency = (3);

			// Set up us the Perlin-noise module.
			caveNoise.Seed = (Seed + 22);
			caveNoise.Frequency = (1.0 / cavessize);
			caveNoise.OctaveCount = (4);
		}
		// Heightmap composition
		FastNoise BiomeBase = new FastNoise();
		ScaleBiasOutput BiomeSelect;
		RidgedMultifractal mountainTerrainBase = new RidgedMultifractal();
		ScaleBiasOutput mountainTerrain;
		Billow baseFlatTerrain = new Billow();
		ScaleBiasOutput flatTerrain;
		Billow baseWater = new Billow();
		ScaleBiasOutput water;
		FastNoise terrainType = new FastNoise();
		FastNoise terrainType2 = new FastNoise();
		Select waterTerrain;
		Select finalTerrain;
		Voronoi flowers = new Voronoi();
		Select jaggieEdges;
		Select secondTerrain;
		Constant plain = new Constant(0);
		Billow jaggieControl = new Billow();

		RidgedMultifractal caveNoise = new RidgedMultifractal();

		float cavessize = 15;
		float cavestreshold = 0.6f;
	}
}

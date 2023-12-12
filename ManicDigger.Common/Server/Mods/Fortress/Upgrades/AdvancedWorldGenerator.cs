/*
 * AdvancedWorldGenerator - Version 0.3
 * Author: croxxx
 * Last change: 2015-09-18
 * 
 * This is a more advanced world generator for Manic Digger.
 * It roughly implements biomes and the new vegetation in latest versions.
 * Work is still in progress and the biomes will completely be rewritten.
 */

using System;
using System.Diagnostics;
using LibNoise;
using LibNoise.Modifiers;

/* ScaleBias: The GetValue() method retrieves the output value from the source module,
 *            multiplies it with a scaling factor, adds a bias to it, then outputs the value.
 * 
 */


namespace ManicDigger.Mods
{
	public class AdvancedWorldGenerator : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;

			//Read important settings from configuration
			chunksize = m.GetChunkSize();
			//mapSizeX = m.GetMapSizeX();
			//mapSizeY = m.GetMapSizeY();
			//mapSizeZ = m.GetMapSizeZ();

			//Retrieve map seed for reproducable terrain generation
			int seed = m.GetSeed();
			//Initialize noise functions
			InitializeNoise(seed);
			//Initialize biome selection modules
			InitializeBiomeSelection(seed);

			// TODO: more
			InitializeBiomes(seed);

			//Load block IDs
			BLOCK_AIR = m.GetBlockId("Empty");
			BLOCK_ADMINIUM = m.GetBlockId("Adminium");
			BLOCK_STONE = m.GetBlockId("Stone");
			//BLOCK_LAVA = m.GetBlockId("Lava");
			BLOCK_DIRT = m.GetBlockId("Dirt");
			BLOCK_GRASS = m.GetBlockId("Grass");
			BLOCK_WATER = m.GetBlockId("Water");
			BLOCK_SAND = m.GetBlockId("Sand");
			BLOCK_GRAVEL = m.GetBlockId("Gravel");

			BLOCK_CLAY = m.GetBlockId("Clay");
			BLOCK_REDSAND = m.GetBlockId("RedSand");
			BLOCK_SANDSTONE = m.GetBlockId("Sandstone");
			BLOCK_REDSANDSTONE = m.GetBlockId("RedSandstone");
			BLOCK_CACTUS = m.GetBlockId("Cactus");
			BLOCK_DEADPLANT = m.GetBlockId("DeadPlant");
			BLOCK_GRASSPLANT = manager.GetBlockId("GrassPlant");

			//Register Generator for terrain generation
			m.RegisterWorldGenerator(GetChunk);
			m.RegisterPopulateChunk(PopulateChunk);
			//m.RegisterOnSave(SaveImage);
			m.RegisterOnSave(DisplayTimes);
        //    SaveImage();
		}

		void DisplayTimes()
		{
			if (getChunkCalls != 0)
			{
				Console.WriteLine("{0} calls to GetChunk()", getChunkCalls);
				Console.WriteLine("{0}ms average time", totalTimeElapsed / getChunkCalls);
				m.SendMessageToAll(string.Format("{0}ms generation average", totalTimeElapsed / getChunkCalls));
				totalTimeElapsed = 0;
				getChunkCalls = 0;
			}
			if (populateChunkCalls != 0)
			{
				Console.WriteLine("{0} calls to PopulateChunk()", populateChunkCalls);
				Console.WriteLine("{0}ms average time", totalPopulateTime / populateChunkCalls);
				m.SendMessageToAll(string.Format("{0}ms population average", totalPopulateTime / populateChunkCalls));
				totalPopulateTime = 0;
				populateChunkCalls = 0;
			}
		}

		//Mod variables
		ModManager m;
		int chunksize;
		//int mapSizeX;
		//int mapSizeY;
		//int mapSizeZ;
		double[,,] interpolatednoise;  //Used for interpolationg the 3D perlin noise

		//Block Types
		int BLOCK_AIR;
		int BLOCK_ADMINIUM;
		int BLOCK_STONE;
		//int BLOCK_LAVA;
		int BLOCK_DIRT;
		int BLOCK_GRASS;
		int BLOCK_WATER;
		int BLOCK_SAND;
		int BLOCK_GRAVEL;

		int BLOCK_CLAY;
		int BLOCK_REDSAND;
		int BLOCK_SANDSTONE;
		int BLOCK_REDSANDSTONE;

		//int BLOCK_MARBLE;
		//int BLOCK_GRANITE;
		int BLOCK_CACTUS;
		int BLOCK_DEADPLANT;
		int BLOCK_GRASSPLANT;

		//Variables
		const int heightWaterLevel = 30;
		const int heightVegetationBorder = 90;
		const bool largeBiomes = false;
		const double overhangsTreshold = 0.25;

		//Debug
		int getChunkCalls;
		int populateChunkCalls;
		long totalTimeElapsed;
		long totalPopulateTime;
		Stopwatch watch = new Stopwatch();

		#region Biome ID constants
		//Constant id_none = new Constant(256);
		Constant id_ocean = new Constant(0);
		Constant id_plains = new Constant(1);
		Constant id_desert = new Constant(2);
		Constant id_canyon = new Constant(3);
		Constant id_hills = new Constant(4);
		Constant id_mountains = new Constant(5);
		Constant id_island = new Constant(6);
		Constant id_swamp = new Constant(7);
		Constant id_dunes = new Constant(8);
		Constant id_desertmountains = new Constant(9);
		Constant id_grassymountains = new Constant(10);
		Constant id_shore = new Constant(11);
		#endregion

		#region old selectors
		//Generator functions
		RidgedMultifractal mountainsNoise = new RidgedMultifractal();
		ScaleInput mountainsScaled;
		ScaleBiasOutput mountainsOutput;

		FastBillow hillsNoise = new FastBillow();
		ScaleInput hillsScaled;
		ScaleBiasOutput hillsOutput;

		Billow plainsNoise = new Billow();
		ScaleInput plainsScaled;
		ScaleBiasOutput plainsOutput;

		Billow desertNoise = new Billow();
		ScaleBiasOutput desertOutput;

		Billow canyonNoise = new Billow();
		ScaleBiasOutput canyonScaled;

		FastBillow oceanNoise = new FastBillow();
		ScaleInput oceanScaled;
		ScaleBiasOutput oceanOutput;

		Billow islandNoise = new Billow();
		ScaleInput islandScaled;
		ScaleBiasOutput islandOutput;

		Perlin overhangs = new Perlin();
		FastNoise vegetationNoise = new FastNoise();

		//Selector Noise functions
		FastNoise noise_selectDesertType = new FastNoise();
		ScaleInput noise_selectDesertTypeScaled;
		FastNoise noise_selectCanyon = new FastNoise();
		ScaleInput noise_selectCanyonScaled;
		FastNoise noise_selectIsland = new FastNoise();
		ScaleInput noise_selectIslandScaled;
		FastNoise noise_selectOceanType = new FastNoise();
		ScaleInput noise_selectOceanTypeScaled;
		FastNoise noise_selectPlainDesert = new FastNoise();
		ScaleInput noise_selectPlainDesertScaled;
		FastNoise noise_selectHills = new FastNoise();
		ScaleInput noise_selectHillsScaled;
		FastNoise noise_selectMountains = new FastNoise();
		ScaleInput noise_selectMountainsScaled;

		//Terrain generator selectors
		Select terrainselect_canyon;
		Select terrainselect_desert_type;
		Select terrainselect_island;
		Select terrainselect_ocean_type;
		Select terrainselect_plain_desert;
		Select terrainselect_hills;
		Select terrainselect_mountains;
		Select terrainselect_ocean_land;

		//Biome generator selectors
		Select select_canyon;
		Select select_desert_type;
		Select select_island;
		Select select_ocean_type;
		Select select_plain_desert;
		Select select_hills;
		Select select_mountains;
		Select select_ocean_land;
		#endregion

		// Height and moisture map
		FastNoise noise_height = new FastNoise();
		FastNoise noise_hum = new FastNoise();
		ScaleBiasOutput noise_humidity;

		#region Selectors for biome selection
		Select sel_bio_high0_hum0_1;
		Select sel_bio_high0_hum01_2;
		Select sel_bio_high0_hum012_3;
		Select sel_bio_high0_hum0123_4; // Combined output for height 0

		Select sel_bio_high1_hum0_1;
		Select sel_bio_high1_hum01_2;
		Select sel_bio_high1_hum012_3;
		Select sel_bio_high1_hum0123_4; // Combined output for height 1

		Select sel_bio_high2_hum0_1;
		Select sel_bio_high2_hum01_2;
		Select sel_bio_high2_hum012_3;
		Select sel_bio_high2_hum0123_4; // Combined output for height 2

		Select sel_bio_high3_hum0_1;
		Select sel_bio_high3_hum01_2;
		Select sel_bio_high3_hum012_3;
		Select sel_bio_high3_hum0123_4; // Combined output for height 3

		Select sel_bio_high4_hum0_1;
		Select sel_bio_high4_hum01_2;
		Select sel_bio_high4_hum012_3;
		Select sel_bio_high4_hum0123_4; // Combined output for height 4

		Select sel_bio_high5_hum0_1;
		Select sel_bio_high5_hum01_2;
		Select sel_bio_high5_hum012_3;
		Select sel_bio_high5_hum0123_4; // Combined output for height 5

		Select sel_bio_high0_1;
		Select sel_bio_high01_2;
		Select sel_bio_high012_3;
		Select sel_bio_high0123_4;
		Select sel_bio_high01234_5; // Final output
		#endregion

		#region Selectors for terrain generation
		// TODO
		#endregion

		void GetChunk(int x, int y, int z, ushort[] chunk)
		{
			getChunkCalls++;
			watch.Reset();
			watch.Start();
			//Multiply chunk coordinates with chunk size to get absolute starting coordinates
			x *= chunksize;
			y *= chunksize;
			z *= chunksize;
			interpolatednoise = InterpolateNoise3d(x, y, z, chunksize);

			//Loop for each block in a chunk
			for (int xx = 0; xx < chunksize; xx++)
			{
				for (int yy = 0; yy < chunksize; yy++)
				{
					//Determine height from heightmap
					int currentHeight = (int)(terrainselect_ocean_land.GetValue((x + xx) / 1024.0, 0, (y + yy) / 1024.0) * 32);
					Biome currentBiome = GetBiome(x + xx, y + yy);
					int overhangsHeight = (int)(GetNoise(x + xx, y + yy, 0) * 32) + currentHeight;

					for (int zz = 0; zz < chunksize; zz++)
					{
						int block;
						int globalz = z + zz;
						if (globalz == 0)
						{
							block = BLOCK_ADMINIUM;
						}
						else
						{
							if (globalz <= currentHeight)
							{
								switch (currentBiome)
								{
									case Biome.Ocean:
										if (globalz < currentHeight)
										{
											block = BLOCK_STONE;
										}
										else
										{
											if (globalz > heightWaterLevel)
											{
												block = BLOCK_STONE;
											}
											else
											{
												block = BLOCK_GRAVEL;
											}
										}
										break;
									case Biome.Island:
										if (globalz < currentHeight - 1)
										{
											block = BLOCK_STONE;
										}
										else if (globalz < currentHeight)
										{
											if (currentHeight < heightWaterLevel + 2)
											{
												block = BLOCK_SAND;
											}
											else
											{
												block = BLOCK_DIRT;
											}
										}
										else
										{
											if (currentHeight < heightWaterLevel + 2)
											{
												block = BLOCK_SAND;
											}
											else
											{
												block = BLOCK_GRASS;
											}
										}
										break;
									case Biome.Plains:
										if (globalz < currentHeight - 4)
										{
											block = BLOCK_STONE;
										}
										else if (globalz < currentHeight)
										{
											block = BLOCK_DIRT;
										}
										else
										{
											if (currentHeight < heightWaterLevel + 1)
											{
												block = BLOCK_SAND;
											}
											else
											{
												block = BLOCK_GRASS;
											}
										}
										break;
									case Biome.Mountains:
										if (globalz < currentHeight - 1)
										{
											block = BLOCK_STONE;
										}
										else if (globalz < currentHeight)
										{
											block = BLOCK_DIRT;
										}
										else
										{
											block = BLOCK_GRASS;
										}
										if (interpolatednoise[xx, yy, zz] < -0.3)
										{
											block = BLOCK_AIR;
										}
										break;
									case Biome.Hills:
										if (globalz < currentHeight - 2)
										{
											block = BLOCK_STONE;
										}
										else if (globalz < currentHeight)
										{
											block = BLOCK_DIRT;
										}
										else
										{
											if (currentHeight < heightWaterLevel + 1)
											{
												block = BLOCK_SAND;
											}
											else
											{
												block = BLOCK_GRASS;
											}
										}
										break;
									case Biome.Desert:
										if (globalz < currentHeight - 3)
										{
											block = BLOCK_STONE;
										}
										else
										{
											block = BLOCK_SAND;
										}
										break;
									case Biome.Canyon:
										if (globalz < currentHeight - 2)
										{
											if (globalz == currentHeight - 8 || globalz == currentHeight - 19)
											{
												block = BLOCK_CLAY;
											}
											else if (globalz == currentHeight - 9 || globalz == currentHeight - 21)
											{
												block = BLOCK_SANDSTONE;
											}
											else if (globalz == currentHeight - 20)
											{
												block = BLOCK_SAND;
											}
											else if (globalz > currentHeight - 30)
											{
												block = BLOCK_REDSANDSTONE;
											}
											else
											{
												block = BLOCK_STONE;
											}
										}
										else
										{
											block = BLOCK_REDSAND;
										}
										break;
									//Default just outputs bedrock
									default:
										block = BLOCK_AIR;
										break;
								}
							}
							else
							{
								block = BLOCK_AIR;
								if (globalz <= heightWaterLevel)
								{
									block = BLOCK_WATER;
								}
								if (currentBiome == Biome.Mountains || currentBiome == Biome.Hills)
								{
									if (globalz < overhangsHeight)
									{
										double density = interpolatednoise[xx, yy, zz];
										if (density > overhangsTreshold)
										{
											block = BLOCK_STONE;
										}
									}
								}
								if (currentBiome == Biome.Plains)
								{
									if (globalz == currentHeight + 1)
									{
										double vegetationDensity = vegetationNoise.GetValue((x + xx) / 2.0, (y + yy) / 2.0, 0);
										if (vegetationDensity > 0)
										{
											block = BLOCK_GRASSPLANT;
										}
									}
								}
							}
						}

						//Calculate position inside chunk and store block
						int pos = m.Index3d(xx, yy, zz, chunksize, chunksize);
						chunk[pos] = (ushort)block;
					}
				}
			}
			totalTimeElapsed += watch.ElapsedMilliseconds;
			watch.Stop();
		}

		void InitializeBiomes(int seed)
		{
			// New biome system
			noise_height.Seed = seed;
			noise_hum.Seed = seed * 7;

			noise_humidity = new ScaleBiasOutput(noise_hum);
			noise_humidity.Scale = 10;
			noise_humidity.Bias = 10;

			sel_bio_high0_hum0_1 = new Select(noise_humidity, id_island, id_island);
			sel_bio_high0_hum0_1.SetBounds(2, 6);
			sel_bio_high0_hum01_2 = new Select(noise_humidity, sel_bio_high0_hum0_1, id_island);
			sel_bio_high0_hum01_2.SetBounds(6, 13);
			sel_bio_high0_hum012_3 = new Select(noise_humidity, sel_bio_high0_hum01_2, id_island);
			sel_bio_high0_hum012_3.SetBounds(13, 18);
			sel_bio_high0_hum0123_4 = new Select(noise_humidity, sel_bio_high0_hum012_3, id_island);
			sel_bio_high0_hum0123_4.SetBounds(18, 20);

			sel_bio_high1_hum0_1 = new Select(noise_humidity, id_ocean, id_ocean);
			sel_bio_high1_hum0_1.SetBounds(2, 6);
			sel_bio_high1_hum01_2 = new Select(noise_humidity, sel_bio_high1_hum0_1, id_ocean);
			sel_bio_high1_hum01_2.SetBounds(6, 13);
			sel_bio_high1_hum012_3 = new Select(noise_humidity, sel_bio_high1_hum01_2, id_ocean);
			sel_bio_high1_hum012_3.SetBounds(13, 18);
			sel_bio_high1_hum0123_4 = new Select(noise_humidity, sel_bio_high1_hum012_3, id_ocean);
			sel_bio_high1_hum0123_4.SetBounds(18, 20);

			sel_bio_high2_hum0_1 = new Select(noise_humidity, id_shore, id_shore);
			sel_bio_high2_hum0_1.SetBounds(2, 6);
			sel_bio_high2_hum01_2 = new Select(noise_humidity, sel_bio_high2_hum0_1, id_shore);
			sel_bio_high2_hum01_2.SetBounds(6, 13);
			sel_bio_high2_hum012_3 = new Select(noise_humidity, sel_bio_high2_hum01_2, id_shore);
			sel_bio_high2_hum012_3.SetBounds(13, 18);
			sel_bio_high2_hum0123_4 = new Select(noise_humidity, sel_bio_high2_hum012_3, id_shore);
			sel_bio_high2_hum0123_4.SetBounds(18, 20);

			sel_bio_high3_hum0_1 = new Select(noise_humidity, id_swamp, id_plains);
			sel_bio_high3_hum0_1.SetBounds(2, 6);
			sel_bio_high3_hum01_2 = new Select(noise_humidity, sel_bio_high3_hum0_1, id_plains);
			sel_bio_high3_hum01_2.SetBounds(6, 13);
			sel_bio_high3_hum012_3 = new Select(noise_humidity, sel_bio_high3_hum01_2, id_desert);
			sel_bio_high3_hum012_3.SetBounds(13, 18);
			sel_bio_high3_hum0123_4 = new Select(noise_humidity, sel_bio_high3_hum012_3, id_desert);
			sel_bio_high3_hum0123_4.SetBounds(18, 20);

			sel_bio_high4_hum0_1 = new Select(noise_humidity, id_hills, id_hills);
			sel_bio_high4_hum0_1.SetBounds(2, 6);
			sel_bio_high4_hum01_2 = new Select(noise_humidity, sel_bio_high4_hum0_1, id_hills);
			sel_bio_high4_hum01_2.SetBounds(6, 13);
			sel_bio_high4_hum012_3 = new Select(noise_humidity, sel_bio_high4_hum01_2, id_dunes);
			sel_bio_high4_hum012_3.SetBounds(13, 18);
			sel_bio_high4_hum0123_4 = new Select(noise_humidity, sel_bio_high4_hum012_3, id_dunes);
			sel_bio_high4_hum0123_4.SetBounds(18, 20);

			sel_bio_high5_hum0_1 = new Select(noise_humidity, id_grassymountains, id_grassymountains);
			sel_bio_high5_hum0_1.SetBounds(2, 6);
			sel_bio_high5_hum01_2 = new Select(noise_humidity, sel_bio_high5_hum0_1, id_mountains);
			sel_bio_high5_hum01_2.SetBounds(6, 13);
			sel_bio_high5_hum012_3 = new Select(noise_humidity, sel_bio_high5_hum01_2, id_desertmountains);
			sel_bio_high5_hum012_3.SetBounds(13, 18);
			sel_bio_high5_hum0123_4 = new Select(noise_humidity, sel_bio_high5_hum012_3, id_desertmountains);
			sel_bio_high5_hum0123_4.SetBounds(18, 20);

			sel_bio_high0_1 = new Select(noise_height, sel_bio_high0_hum0123_4, sel_bio_high1_hum0123_4);
			sel_bio_high0_1.SetBounds(2, 5);
			sel_bio_high01_2 = new Select(noise_height, sel_bio_high0_1, sel_bio_high2_hum0123_4);
			sel_bio_high01_2.SetBounds(5, 6);
			sel_bio_high012_3 = new Select(noise_height, sel_bio_high01_2, sel_bio_high3_hum0123_4);
			sel_bio_high012_3.SetBounds(6, 12);
			sel_bio_high0123_4 = new Select(noise_height, sel_bio_high012_3, sel_bio_high4_hum0123_4);
			sel_bio_high0123_4.SetBounds(12, 17);
			sel_bio_high01234_5 = new Select(noise_height, sel_bio_high0123_4, sel_bio_high5_hum0123_4);
			sel_bio_high01234_5.SetBounds(17, 20);
		}

		void InitializeNoise(int Seed)
		{
			//Ocean Noise Generator
			oceanNoise.Seed = Seed;
			oceanScaled = new ScaleInput(oceanNoise, 8, 8, 8);
			oceanScaled.SourceModule = oceanNoise;
			oceanOutput = new ScaleBiasOutput(oceanScaled);
			oceanOutput.Scale = 0.25;
			oceanOutput.Bias = 0.5;

			//Mountain Noise Generator
			mountainsNoise.Seed = Seed;
			mountainsScaled = new ScaleInput(mountainsNoise, 4, 4, 4);
			mountainsScaled.SourceModule = mountainsNoise;
			mountainsOutput = new ScaleBiasOutput(mountainsScaled);
			mountainsOutput.Scale = 1;
			mountainsOutput.Bias = 2.2;

			//Hills Noise Generator
			hillsNoise.Seed = Seed + 5;
			hillsNoise.Persistence = 0.25;
			hillsScaled = new ScaleInput(hillsNoise, 16, 16, 16);
			hillsScaled.SourceModule = hillsNoise;
			hillsOutput = new ScaleBiasOutput(hillsScaled);
			hillsOutput.Scale = 0.25;
			hillsOutput.Bias = 1.25;

			//Plains Noise Generator
			plainsNoise.Persistence = 0.5;
			plainsNoise.Seed = Seed;
			plainsScaled = new ScaleInput(plainsNoise, 2, 2, 2);
			plainsScaled.SourceModule = plainsNoise;
			plainsOutput = new ScaleBiasOutput(plainsScaled);
			plainsOutput.Scale = 0.17;
			plainsOutput.Bias = 1.2;

			//Desert Noise Generator
			desertNoise.Persistence = 0.5;
			desertNoise.Seed = Seed + 10;
			desertOutput = new ScaleBiasOutput(desertNoise);
			desertOutput.Scale = 0.3;
			desertOutput.Bias = 1.3;

			//Canyon Noise Generator
			canyonNoise.Seed = Seed + 20;
			canyonScaled = new ScaleBiasOutput(canyonNoise);
			canyonScaled.Scale = 0.4;
			canyonScaled.Bias = 1.75;

			//Island Noise Generator
			islandNoise.Persistence = 0.5;
			islandNoise.Seed = Seed - 10;
			islandScaled = new ScaleInput(islandNoise, 8, 8, 8);
			islandScaled.SourceModule = islandNoise;
			islandOutput = new ScaleBiasOutput(islandScaled);
			islandOutput.Scale = 0.2;
			islandOutput.Bias = 1.1;

			//Vegetation
			vegetationNoise.Seed = Seed;

			//Overhangs generator
			overhangs.Seed = Seed;
			overhangs.OctaveCount = 1;
			overhangs.Persistence = 0.3;
		}

		void InitializeBiomeSelection(int Seed)
		{
			//Set Selector noise parameters
			noise_selectCanyon.Seed = Seed;
			noise_selectCanyon.OctaveCount = 3;
			noise_selectDesertType.Seed = Seed + 1;
			noise_selectIsland.Seed = Seed + 2;
			noise_selectOceanType.Seed = Seed + 3;
			noise_selectPlainDesert.Seed = Seed - 1;
			noise_selectHills.Seed = Seed - 2;
			noise_selectMountains.Seed = Seed - 3;

			//Setup selector functions
			noise_selectCanyonScaled = new ScaleInput(noise_selectCanyon, 16, 16, 16);
			noise_selectCanyonScaled.SourceModule = noise_selectCanyon;
			noise_selectDesertTypeScaled = new ScaleInput(noise_selectDesertType, 1, 1, 1);
			noise_selectDesertTypeScaled.SourceModule = noise_selectDesertType;
			noise_selectIslandScaled = new ScaleInput(noise_selectIsland, 8, 8, 8);
			noise_selectIslandScaled.SourceModule = noise_selectIsland;
			noise_selectOceanTypeScaled = new ScaleInput(noise_selectOceanType, 1, 1, 1);
			noise_selectOceanTypeScaled.SourceModule = noise_selectOceanType;
			noise_selectPlainDesertScaled = new ScaleInput(noise_selectPlainDesert, 2, 2, 2);
			noise_selectPlainDesertScaled.SourceModule = noise_selectPlainDesert;
			noise_selectHillsScaled = new ScaleInput(noise_selectHills, 2, 2, 2);
			noise_selectHillsScaled.SourceModule = noise_selectHills;
			noise_selectMountainsScaled = new ScaleInput(noise_selectMountains, 2, 2, 2);
			noise_selectMountainsScaled.SourceModule = noise_selectMountains;

			//Biome generation definitions
			select_canyon = new Select(noise_selectCanyonScaled, id_desert, id_canyon);
			select_canyon.SetBounds(0.7, 1);        //ca. 15%
			select_desert_type = new Select(noise_selectDesertTypeScaled, id_desert, select_canyon);
			select_desert_type.SetBounds(0, 2);     //ca. 50%
			select_island = new Select(noise_selectIslandScaled, id_ocean, id_island);
			select_island.SetBounds(0.8, 2);        //ca. 10%
			select_ocean_type = new Select(noise_selectOceanTypeScaled, id_ocean, select_island);
			select_ocean_type.SetBounds(-2, -0.5);  //ca. 25%
			select_plain_desert = new Select(noise_selectPlainDesertScaled, select_desert_type, id_plains);
			select_plain_desert.SetBounds(-0.2, 2); //ca. 60%
			select_hills = new Select(noise_selectHillsScaled, select_plain_desert, id_hills);
			select_hills.SetBounds(0.4, 2);         //ca. 30%
			select_mountains = new Select(noise_selectMountainsScaled, select_hills, id_mountains);
			select_mountains.SetBounds(0.4, 2);     //ca. 30%
			select_ocean_land = new Select(noise_selectOceanTypeScaled, select_ocean_type, select_mountains);
			select_ocean_land.SetBounds(0.2, 2);    //ca. 40% - last step in combination

			//Terrain generation definitions
			terrainselect_canyon = new Select(noise_selectCanyonScaled, desertOutput, canyonScaled);
			terrainselect_canyon.SetBounds(0.7, 1);
			terrainselect_canyon.EdgeFalloff = 0.01;

			terrainselect_desert_type = new Select(noise_selectDesertTypeScaled, desertOutput, terrainselect_canyon);
			terrainselect_desert_type.SetBounds(0, 2);
			terrainselect_desert_type.EdgeFalloff = 0.1;

			terrainselect_island = new Select(noise_selectIslandScaled, oceanOutput, islandOutput);
			terrainselect_island.SetBounds(0.8, 2);
			terrainselect_island.EdgeFalloff = 0.2;

			terrainselect_ocean_type = new Select(noise_selectOceanTypeScaled, oceanOutput, terrainselect_island);
			terrainselect_ocean_type.SetBounds(-2, -0.5);
			terrainselect_ocean_type.EdgeFalloff = 0.05;

			terrainselect_plain_desert = new Select(noise_selectPlainDesertScaled, terrainselect_desert_type, plainsOutput);
			terrainselect_plain_desert.SetBounds(-0.2, 2);
			terrainselect_plain_desert.EdgeFalloff = 0.1;

			terrainselect_hills = new Select(noise_selectHillsScaled, terrainselect_plain_desert, hillsOutput);
			terrainselect_hills.SetBounds(0.4, 2);
			terrainselect_hills.EdgeFalloff = 0.05;

			terrainselect_mountains = new Select(noise_selectMountainsScaled, terrainselect_hills, mountainsOutput);
			terrainselect_mountains.SetBounds(0.4, 2);
			terrainselect_mountains.EdgeFalloff = 0.02;

			terrainselect_ocean_land = new Select(noise_selectOceanTypeScaled, terrainselect_ocean_type, terrainselect_mountains);
			terrainselect_ocean_land.SetBounds(0.2, 2);
			terrainselect_ocean_land.EdgeFalloff = 0.05;
		}



        private void SaveImage()
        {

            using (var tile = new System.Drawing.Bitmap(m.GetMapSizeX(), m.GetMapSizeY()))
            {
                try
                {
                    System.Drawing.Imaging.BitmapData dstData = tile.LockBits(new System.Drawing.Rectangle(0, 0, tile.Width, tile.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    unsafe
                    {
                        System.Drawing.Color c;

                        byte* PtrFirstPixel = (byte*)dstData.Scan0;
                        System.Threading.Tasks.Parallel.For(0, tile.Height, i =>
                        {

                            byte* currentLine = PtrFirstPixel + (i * dstData.Stride);
                            for (int j = 0; j < tile.Width; j++)
                            {
                                switch (GetBiome(i * 88, j * 88))
                                {
                                    case Biome.Ocean:
                                        c = System.Drawing.Color.FromArgb(0, 16, 190);
                                        break;
                                    case Biome.Island:
                                        c = System.Drawing.Color.FromArgb(0, 16, 160);
                                        break;
                                    case Biome.Shore:
                                        c = System.Drawing.Color.FromArgb(21, 35, 190);
                                        break;
                                    case Biome.Desert:
                                        c = System.Drawing.Color.FromArgb(213, 205, 0);
                                        break;
                                    case Biome.Dunes:
                                        c = System.Drawing.Color.FromArgb(213, 255, 0);
                                        break;
                                    case Biome.Canyon:
                                        c = System.Drawing.Color.FromArgb(213, 170, 0);
                                        break;
                                    case Biome.Plains:
                                        c = System.Drawing.Color.FromArgb(39, 173, 46);
                                        break;
                                    case Biome.Swamp:
                                        c = System.Drawing.Color.FromArgb(39, 128, 46);
                                        break;
                                    case Biome.Hills:
                                        c = System.Drawing.Color.FromArgb(39, 210, 46);
                                        break;
                                    case Biome.Mountains:
                                        c = System.Drawing.Color.FromArgb(162, 162, 162);
                                        break;
                                    case Biome.DesertMountains:
                                        c = System.Drawing.Color.FromArgb(162, 162, 128);
                                        break;
                                    case Biome.GrassyMountains:
                                        c = System.Drawing.Color.FromArgb(162, 206, 162);
                                        break;
                                    default:
                                        c = System.Drawing.Color.Pink;
                                        break;
                                }

                                currentLine[0] = c.B; // Blue
                                currentLine[1] = c.G; // Green
                                currentLine[2] = c.R; // Red
                                currentLine[3] = c.A; // Alpha

                                currentLine += 4;

                            }
                        });
                    }

                    tile.UnlockBits(dstData);

                    tile.Save("biomes.png");
                }
                catch (InvalidOperationException e)
                {
                    Console.Write(e);
                }
            }



            
        }

       

		Biome GetBiome(int x, int y)
		{
//	 		return (Biome)((int)(select_ocean_land.GetValue(x / 1024.0, 0, y / 1024.0)));

 //	return (Biome)((int)(sel_bio_high01234_5.GetValue(x / 1024.0, 0, y / 1024.0)));

	 		 		int moist = (int)(noise_humidity.GetValue((x)/2048.0, 0, (y)/2048.0) * 2.25 + 2.75);
 		 		int height = (int)(noise_height.GetValue((x)/1024.0, 0, (y)/1024.0) * 2.75 + 3.5);
	 		 		return DetermineBiome((HeightBase)height, (Humidity)moist);
		}

		enum Biome
		{
			Ocean,              //done
			Plains,             //done
			Desert,             //done
			Canyon,             //done
			Hills,              //
			Mountains,          //done
			Island,             //done
			Swamp,
			Dunes,
			DesertMountains,
			GrassyMountains,
			Shore,
			//Jungle,

			//Volcano,
			//Arctic,
		}
		enum HeightBase
		{
			OceanDeep = 0,
			Ocean = 1,
			OceanShore = 2,
			Low = 3,
			Medium = 4,
			High = 5,
			//Extreme = 6,
		}
		enum Humidity
		{
			ExtremeDry = 0,
			Dry = 1,
			Moderate = 2,
			Wet = 3,
			ExtremeWet = 4,
		}

		/// <summary>
		/// Determines the biome given certain values for height and humidity.
		/// Height is assumed to be proportional to temperature.
		/// Scheme is derived from these:
		/// - http://www.marietta.edu/~biol/biomes/whittaker.jpg
		/// - http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/
		/// </summary>
		/// <param name="height">Height value</param>
		/// <param name="humidity">Humidity value</param>
		/// <returns></returns>
		Biome DetermineBiome(HeightBase height, Humidity humidity)
		{
			switch (height)
			{
				// Ocean
				case HeightBase.OceanDeep:
					return Biome.Island;
				case HeightBase.Ocean:
					return Biome.Ocean;
				case HeightBase.OceanShore:
					return Biome.Shore;

				// Landmass
				case HeightBase.Low:
					switch (humidity)
					{
						case Humidity.ExtremeWet:
							return Biome.Swamp;
						case Humidity.Wet:
						case Humidity.Moderate:
							return Biome.Plains;
						case Humidity.Dry:
						case Humidity.ExtremeDry:
							return Biome.Desert;
					}
					break;
				case HeightBase.Medium:
					switch (humidity)
					{
						case Humidity.ExtremeWet:
						case Humidity.Wet:
						case Humidity.Moderate:
							return Biome.Hills;
						case Humidity.Dry:
						case Humidity.ExtremeDry:
							return Biome.Dunes;
					}
					break;
				case HeightBase.High:
					switch (humidity)
					{
						case Humidity.ExtremeWet:
						case Humidity.Wet:
							return Biome.GrassyMountains;
						case Humidity.Moderate:
							return Biome.Mountains;
						case Humidity.Dry:
						case Humidity.ExtremeDry:
							return Biome.DesertMountains;
					}
					break;
			}
			return Biome.Hills;
		}

		#region 3D Noise helpers
		public double GetNoise(double x, double y, double z)
		{
			return overhangs.GetValue(x / 16.0, y / 16.0, z / 16.0);
		}
		public double[,,] InterpolateNoise3d(double x, double y, double z, int chunksize)
		{
			double[,,] noise = new double[chunksize, chunksize, chunksize];
			const int n = 8;
			for (int xx = 0; xx < chunksize; xx += n)
			{
				for (int yy = 0; yy < chunksize; yy += n)
				{
					for (int zz = 0; zz < chunksize; zz += n)
					{
						double f000 = GetNoise(x + xx, y + yy, z + zz);
						double f100 = GetNoise(x + xx + (n - 1), y + yy, z + zz);
						double f010 = GetNoise(x + xx, y + yy + (n - 1), z + zz);
						double f110 = GetNoise(x + xx + (n - 1), y + yy + (n - 1), z + zz);
						double f001 = GetNoise(x + xx, y + yy, z + zz + (n - 1));
						double f101 = GetNoise(x + xx + (n - 1), y + yy, z + zz + (n - 1));
						double f011 = GetNoise(x + xx, y + yy + (n - 1), z + zz + (n - 1));
						double f111 = GetNoise(x + xx + (n - 1), y + yy + (n - 1), z + zz + (n - 1));
						for (int ix = 0; ix < n; ix++)
						{
							for (int iy = 0; iy < n; iy++)
							{
								for (int iz = 0; iz < n; iz++)
								{
									noise[xx + ix, yy + iy, zz + iz] = Trilinear((double)ix / (n - 1), (double)iy / (n - 1), (double)iz / (n - 1),
																				 f000, f010, f100, f110, f001, f011, f101, f111);
								}
							}
						}
					}
				}
			}
			return noise;
		}
		public static double Trilinear(double x, double y, double z,
									   double f000, double f010, double f100, double f110,
									   double f001, double f011, double f101, double f111)
		{
			double up0 = (f100 - f000) * x + f000;
			double down0 = (f110 - f010) * x + f010;
			double all0 = (down0 - up0) * y + up0;

			double up1 = (f101 - f001) * x + f001;
			double down1 = (f111 - f011) * x + f011;
			double all1 = (down1 - up1) * y + up1;

			return (all1 - all0) * z + all0;
		}
		#endregion

		Random rnd = new Random();
		//This generates grass and other vegetation
		void PopulateChunk(int cx, int cy, int cz)
		{
			populateChunkCalls++;
			watch.Reset();
			watch.Start();

			cx *= chunksize;
			cy *= chunksize;
			cz *= chunksize;

			int vegetationDensity = rnd.Next(50, 300);
			for (int i = 0; i < vegetationDensity; i++)
			{
				int x = cx + rnd.Next(chunksize);
				int y = cy + rnd.Next(chunksize);
				int z = cz + rnd.Next(chunksize);
				if (!m.IsValidPos(x, y, z))
				{
					continue;
				}
				if (!m.IsValidPos(x, y, z + 1) || m.GetBlock(x, y, z + 1) != 0)
				{
					continue;
				}
				int blockAt = m.GetBlock(x, y, z);
				if (blockAt == BLOCK_GRASS)
				{
					//m.SetBlock(x, y, z + 1, BLOCK_GRASSPLANT);
				}
				else if (blockAt == BLOCK_SAND || blockAt == BLOCK_REDSAND)
				{
					switch (rnd.Next(2))
					{
						case 0:
							for (int j = 0; j < rnd.Next(3, 4); j++)
							{
								if (m.IsValidPos(x, y, z + j + 1) && m.GetBlock(x, y, z + j + 1) == 0)
								{
									m.SetBlock(x, y, z + j + 1, BLOCK_CACTUS);
								}
							}
							break;
						case 1:
							m.SetBlock(x, y, z + 1, BLOCK_DEADPLANT);
							break;
					}
				}
			}
			totalPopulateTime += watch.ElapsedMilliseconds;
			watch.Stop();
		}
	}
}

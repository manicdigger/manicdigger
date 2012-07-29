using System;
using System.Collections.Generic;
using System.Text;
using LibNoise;
using LibNoise.Modifiers;

namespace ManicDigger.Mods
{
    public class DefaultWorldGenerator : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }
        public void Start(ModManager m)
        {
            this.m = m;
            Init();
            m.RegisterWorldGenerator(GetChunk);
            m.RegisterOptionBool("DefaultGenCaves", true);
            m.RegisterOptionBool("DefaultGenLavaCaves", true);
        }

        ModManager m;

        public void GetChunk(int x, int y, int z, byte[] chunk)
        {
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
                    /*
                    var v = noise.GetValue(x + xx, y + yy, 0);
                    //if (v < min) { min = v; }
                    int height = (int)(((v + 1) * 0.5) * 64) + 5;
                    //if (height < 0)
                    {
                        //    Console.Beep();
                    }
                    */

                    int currentHeight = (byte)((finalTerrain.GetValue((xx + x) / 100.0, 0, (yy + y) / 100.0) * 60) + 64);
                    int ymax = currentHeight;
                    /*for (int zz = 0; zz < chunksize; zz++)
                    {
                        if (z + zz <= currentHeight) { chunk[xx, yy, zz] = 1; }
                    }*/

                    int biome = (int)(BiomeSelect.GetValue((x + xx) / 100.0, 0, (y + yy) / 100.0) * 2); //MD * 2
                    byte toplayer = BLOCK_DIRT;
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

                    //biomecoun[biome]++;

                    int stoneHeight = (int)currentHeight - ((64 - (currentHeight % 64)) / 8) + 1;
                    int bYbX = ((y << 7) + (x << 11));

                    if (ymax < seaLevel)
                    {
                        ymax = seaLevel;
                    }
                    ymax++;
                    if (ymax > z + chunksize - 1) //md
                    {
                        ymax = z + chunksize - 1;
                    }
                    //for (int bY = 0; bY <= ymax; bY++)
                    for (int bY = z; bY <= ymax; bY++)
                    {
                        //curBlock = &(chunk->blocks[bYbX++]);
                        int curBlock = 0;

                        // Place bedrock
                        if (bY == 0)
                        {
                            curBlock = BLOCK_BEDROCK;
                            continue;
                        }

                        if (bY < currentHeight)
                        {
                            if (bY < stoneHeight)
                            {
                                curBlock = BLOCK_STONE;
                                // Add caves
                                if (addCaves)
                                {
                                    //cave.AddCaves(curBlock, x + xx, bY, y + yy);

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
                        chunk[ModManager.Index3d(xx, yy, bY - z, chunksize, chunksize)] = (byte)curBlock;
                    }
                }
            }
        }
        //int[] biomecoun = new int[10];
        int seaLevel = 62;
        byte BLOCK_STONE = 1;
        byte BLOCK_DIRT = 3;
        byte BLOCK_SAND = 12;
        byte BLOCK_CLAY = 1; //stone
        byte BLOCK_BEDROCK = 7;
        byte BLOCK_AIR = 0;
        byte BLOCK_SNOW = 102; //todo
        byte BLOCK_ICE = 101; //todo
        byte BLOCK_GRASS = 2;
        byte BLOCK_WATER = 8;
        byte BLOCK_GRAVEL = 13;
        byte BLOCK_PUMPKIN = 107; //hay
        byte BLOCK_RED_ROSE = 38;
        byte BLOCK_YELLOW_FLOWER = 37;
        byte BLOCK_LAVA = 11;
        public void Init()
        {
            int Seed = m.GetSeed();
            /*
              cave.init(seed + 7);
  seaLevel = Mineserver::get()->config()->iData("mapgen.sea.level");
  addTrees = Mineserver::get()->config()->bData("mapgen.trees.enabled");
  expandBeaches = Mineserver::get()->config()->bData("mapgen.beaches.expand");
  beachExtent = Mineserver::get()->config()->iData("mapgen.beaches.extent");
  beachHeight = Mineserver::get()->config()->iData("mapgen.beaches.height");

  addOre = Mineserver::get()->config()->bData("mapgen.addore");
  addCaves = Mineserver::get()->config()->bData("mapgen.caves.enabled");
            */

            BiomeBase.Frequency = (0.2);
            BiomeBase.Seed = (Seed - 1);
            BiomeSelect = new ScaleBiasOutput(BiomeBase);
            //BiomeSelect.SourceModule = (BiomeBase);
            BiomeSelect.Scale = (2.5);
            BiomeSelect.Bias = (2.5);
            mountainTerrainBase.Seed = (Seed + 1);
            mountainTerrain = new ScaleBiasOutput(mountainTerrainBase);
            //mountainTerrain.SourceModule = (mountainTerrainBase);
            mountainTerrain.Scale = (0.5);
            mountainTerrain.Bias = (0.5);
            jaggieEdges = new Select(jaggieControl, terrainType, plain);
            //jaggieEdges.SourceModule1 = (terrainType);
            //jaggieEdges.SourceModule2 = (plain);
            plain.Value = (0.5);
            //jaggieEdges.ControlModule = (jaggieControl);
            jaggieEdges.SetBounds(0.5, 1.0);
            jaggieEdges.EdgeFalloff = (0.11);
            jaggieControl.Seed = (Seed + 20);
            baseFlatTerrain.Seed = (Seed);
            baseFlatTerrain.Frequency = (0.2);
            flatTerrain = new ScaleBiasOutput(baseFlatTerrain);
            //flatTerrain.SourceModule = (baseFlatTerrain);
            flatTerrain.Scale = (0.125);
            flatTerrain.Bias = (0.07);
            baseWater.Seed = (Seed - 1);
            water = new ScaleBiasOutput(baseWater);
            //water.SourceModule = (baseWater);
            water.Scale = (0.3);
            water.Bias = (-0.5);
            terrainType.Seed = (Seed + 2);
            terrainType.Frequency = (0.5);
            terrainType.Persistence = (0.25);
            terrainType2.Seed = (Seed + 7);
            terrainType2.Frequency = (0.5);
            terrainType2.Persistence = (0.25);
            waterTerrain = new Select(terrainType2, water, flatTerrain);
            //waterTerrain.SourceModule1 = (water);
            //waterTerrain.SourceModule2 = (flatTerrain);
            //waterTerrain.ControlModule = (terrainType2);
            waterTerrain.EdgeFalloff = (0.1);
            waterTerrain.SetBounds(-0.5, 1.0);
            secondTerrain = new Select(terrainType, mountainTerrain, waterTerrain);
            //secondTerrain.SourceModule2 = (waterTerrain);
            //secondTerrain.SourceModule1 = (mountainTerrain);
            //secondTerrain.ControlModule = (terrainType);
            secondTerrain.EdgeFalloff = (0.3);
            secondTerrain.SetBounds(-0.5, 1.0);
            finalTerrain = new Select(jaggieEdges, secondTerrain, waterTerrain);
            //finalTerrain.SourceModule1 = (secondTerrain);
            //finalTerrain.SourceModule2 = (waterTerrain);
            //finalTerrain.ControlModule = (jaggieEdges);
            finalTerrain.EdgeFalloff = (0.2);
            finalTerrain.SetBounds(-0.3, 1.0);
            flowers.Seed = (Seed + 10);
            flowers.Frequency = (3);
            winterEnabled = false;


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
        bool winterEnabled;

        Random _rnd = new Random();

        RidgedMultifractal caveNoise = new RidgedMultifractal();

        float cavessize = 15;
        float cavestreshold = 0.6f;
    }
}

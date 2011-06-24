#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools
{
    /// <summary>
    /// Provides helper methods that assist with populating chunks with stuff such as caves, trees, flowers etc.
    /// This class cannot be inherited.
    /// </summary>
    public static class PopulationTools
    {
        /// <summary>
        /// Sets a block at the given position.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="blocktype"></param>
        private static void SetBlock(IMapStorage map, int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(map, x, y, z))
            {
                map.SetBlock(x, y, z, blocktype);
            }
        }
        /// <summary>
        /// Sets a block at the given position, but only if it is empty.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="blocktype"></param>
        private static void SetBlockIfEmpty(IMapStorage map, int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(map, x, y, z) && map.GetBlock(x, y, z) == WorldGeneratorTools.TileIdEmpty)
            {
                map.SetBlock(x, y, z, blocktype);
            }
        }

        /// <summary>
        /// Returns whether or not there is a tree at the given location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <param name="seed"></param>
        /// <param name="chunkSize"></param>
        /// <param name="waterLevel"></param>
        /// <param name="treeDensity"></param>
        /// <returns></returns>
        public static bool IsTree(int x, int y, int height, int seed, int chunkSize, int waterLevel, int treeDensity)
        {
            return height >= waterLevel + 3
                && x % 16 >= 2 && x % 16 < chunkSize - 2
                && y % 16 >= 2 && y % 16 < chunkSize - 2
                && (((NoiseTools.Noise(x, y, seed) + 1) / 2) > 1 - treeDensity);
            //&& (((noise((x * 3) + 1000, (y * 3) + 1000) + 1) / 2) > 1 - treedensity);
        }

        /// <summary>
        /// Creates some flowers all over the chunk.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="cz"></param>
        /// <param name="chunksize"></param>
        /// <param name="rnd"></param>
        public static void MakeFlowers(IMapStorage map, int cx, int cy, int cz, int chunksize, Random rnd)
        {
            for (int i = 0; i < 5; i++)
            {
                int x = cx + rnd.Next(chunksize);
                int y = cy + rnd.Next(chunksize);
                int z = cz + rnd.Next(chunksize);
                if (!MapUtil.IsValidPos(map, x, y, z) || map.GetBlock(x, y, z) != WorldGeneratorTools.TileIdGrass)
                {
                    continue;
                }
                int xx; int yy; int zz;
                int tile = rnd.NextDouble() < 0.75 ? WorldGeneratorTools.TileIdYellowFlower : WorldGeneratorTools.TileIdRedFlower;
                int count = rnd.Next(50, 80);
                for (int j = 0; j < count; j++)
                {
                    xx = x + rnd.Next(-6, 6);
                    yy = y + rnd.Next(-6, 6);
                    zz = z + rnd.Next(-2, 2);
                    if (!MapUtil.IsValidPos(map, xx, yy, zz) || map.GetBlock(xx, yy, zz) != WorldGeneratorTools.TileIdGrass)
                    {
                        continue;
                    }

                    // set the block
                    SetBlock(map, x, y, z, tile);
                }
            }
        }

        /// <summary>
        /// Creates some trees all over the chunk.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="chunksize"></param>
        /// <param name="rnd"></param>
        public static void MakeTrees(IMapStorage map, int x, int y, int z, int chunksize, Random rnd)
        {
            if (z != 0) { return; }
            //if (rnd.Next(100) > 30) { return; }
            var foresterArgs = new fCraft.Forester.ForesterArgs();
            if (rnd.Next(100) < 15)
            {
                foresterArgs.SHAPE = fCraft.Forester.TreeShape.Procedural;
            }
            else
            {
                foresterArgs.SHAPE = (fCraft.Forester.TreeShape)rnd.Next(9);
                foresterArgs.HEIGHT = rnd.Next(5, 10);
                foresterArgs.TREECOUNT = 3;
            }
            fCraft.Forester forester = new fCraft.Forester(foresterArgs);
            foresterArgs.inMap = map;
            foresterArgs.outMap = map;
            forester.Generate(x, y, z, chunksize);
        }

        public static void MakeSmallTrees(IMapStorage map, int cx, int cy, int cz, int chunksize, Random rnd, int count)
        {
            int chooseTreeType;
            for (int i = 0; i < count; i++)
            {
                int x = cx + rnd.Next(chunksize);
                int y = cy + rnd.Next(chunksize);
                int z = cz + rnd.Next(chunksize);
                if (!MapUtil.IsValidPos(map, x, y, z) || map.GetBlock(x, y, z) != WorldGeneratorTools.TileIdGrass)
                {
                    continue;
                }
                chooseTreeType = rnd.Next(0, 3);
                switch (chooseTreeType)
                {
                    case 0: MakeTreeType1(map, x, y, z, rnd); break;
                    case 1: MakeTreeType2(map, x, y, z, rnd); break;
                    case 2: MakeTreeType3(map, x, y, z, rnd); break;
                };
            }
        }

        /// <summary>
        /// Creates a tree of type #1 at the given location.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rnd"></param>
        public static void MakeTreeType1(IMapStorage map, int x, int y, int z, Random rnd)
        {
            int treeHeight = rnd.Next(8, 12);
            int xx = 0;
            int yy = 0;
            int dir = 0;

            for (int i = 0; i < treeHeight; i++)
            {
                SetBlock(map, x, y, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                if (i == treeHeight - 4)
                {
                    SetBlock(map, x + 1, y, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                    SetBlock(map, x - 1, y, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                    SetBlock(map, x, y + 1, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                    SetBlock(map, x, y - 1, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                }

                if (i == treeHeight - 3)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 4; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, WorldGeneratorTools.TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, WorldGeneratorTools.TileIdLeaves);
                        }
                    }
                }
                if (i == treeHeight - 1)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 3; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, WorldGeneratorTools.TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, WorldGeneratorTools.TileIdLeaves);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a tree of type #2 at the given location.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rnd"></param>
        public static void MakeTreeType2(IMapStorage map, int x, int y, int z, Random rnd)
        {
            int treeHeight = rnd.Next(4, 6);
            int xx = 0;
            int yy = 0;
            int dir = 0;
            float chanceToAppleTree = 0.1f;
            for (int i = 0; i < treeHeight; i++)
            {
                SetBlock(map, x, y, z + i, WorldGeneratorTools.TileIdTreeTrunk);
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

                            SetBlock(map, x + xx, y + yy, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                            if (chanceToAppleTree < rnd.NextDouble())
                            {
                                SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, WorldGeneratorTools.TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, WorldGeneratorTools.TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, WorldGeneratorTools.TileIdLeaves);
                            }
                            else
                            {
                                float appleChance = 0.4f;
                                int tile;
                                tile = rnd.NextDouble() < appleChance ? WorldGeneratorTools.TileIdApples : WorldGeneratorTools.TileIdLeaves; SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, tile);
                                tile = rnd.NextDouble() < appleChance ? WorldGeneratorTools.TileIdApples : WorldGeneratorTools.TileIdLeaves; SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, tile);
                                tile = rnd.NextDouble() < appleChance ? WorldGeneratorTools.TileIdApples : WorldGeneratorTools.TileIdLeaves; SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, tile);
                                tile = rnd.NextDouble() < appleChance ? WorldGeneratorTools.TileIdApples : WorldGeneratorTools.TileIdLeaves; SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, tile);
                                tile = rnd.NextDouble() < appleChance ? WorldGeneratorTools.TileIdApples : WorldGeneratorTools.TileIdLeaves; SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, tile);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a tree of type #3 at the given location.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rnd"></param>
        public static void MakeTreeType3(IMapStorage map, int x, int y, int z, Random rnd)
        {
            int treeHeight = rnd.Next(6, 9);
            int xx = 0;
            int yy = 0;
            int dir = 0;
            for (int i = 0; i < treeHeight; i++)
            {
                SetBlock(map, x, y, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                if (i % 3 == 0 && i > 3)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 2; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, WorldGeneratorTools.TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, WorldGeneratorTools.TileIdLeaves);
                        }
                    }
                }
                if (i % 3 == 2 && i > 3)
                {
                    dir = 45;
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 3; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, WorldGeneratorTools.TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, WorldGeneratorTools.TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, WorldGeneratorTools.TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, WorldGeneratorTools.TileIdLeaves);
                        }
                    }
                }
                SetBlockIfEmpty(map, x, y, z + treeHeight, WorldGeneratorTools.TileIdLeaves);
            }
        }

        /// <summary>
        /// Creates caves all over the chunk.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="chunksize"></param>
        /// <param name="rnd"></param>
        /// <param name="enableCaves"></param>
        /// <param name="gravelLength"></param>
        /// <param name="goldOreLength"></param>
        /// <param name="ironOreLength"></param>
        /// <param name="coalOreLength"></param>
        /// <param name="dirtOreLength"></param>
        public static void MakeCaves(IMapStorage map, int x, int y, int z, int chunksize, Random rnd,
            bool enableCaves,
            int gravelLength,
            int goldOreLength,
            int ironOreLength,
            int coalOreLength,
            int dirtOreLength)
        {
            //if (rnd.NextDouble() >= 0.6)
            {
                //return;
            }

            //find cave start
            double curx = x;
            double cury = y;
            double curz = z;
            for (int i = 0; i < 2; i++)
            {
                curx = x + rnd.Next(chunksize);
                cury = y + rnd.Next(chunksize);
                curz = z + rnd.Next(chunksize);
                if (map.GetBlock((int)curx, (int)cury, (int)curz) == WorldGeneratorTools.TileIdStone)
                {
                    goto ok;
                }
            }
            return;
        ok:
            int blocktype = WorldGeneratorTools.TileIdEmpty;
            int length = 200;
            if (rnd.NextDouble() < 0.85)
            {
                int oretype = rnd.Next(5);
                if (oretype == 0) { length = gravelLength; }
                if (oretype == 1) { length = goldOreLength; }
                if (oretype == 2) { length = ironOreLength; }
                if (oretype == 3) { length = coalOreLength; }
                if (oretype == 4) { length = dirtOreLength; }

                length = rnd.Next(length);
                blocktype = oretype != 4 ? WorldGeneratorTools.TileIdGravel + oretype : WorldGeneratorTools.TileIdDirt;
            }
            if (blocktype == WorldGeneratorTools.TileIdEmpty && (!enableCaves))
            {
                return;
            }
            //map.SetBlock(x, y, z, WorldGeneratorTools.TileIdLava);
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
                if (!MapUtil.IsValidPos(map, (int)curx, (int)cury, (int)curz))
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

                    int[] allowin = new int[] { WorldGeneratorTools.TileIdStone };
                    double density = blocktype == WorldGeneratorTools.TileIdEmpty ? 1 : rnd.NextDouble() * 0.90;
                    if (blocktype == WorldGeneratorTools.TileIdEmpty)
                    {
                        allowin = new int[] { 
                            WorldGeneratorTools.TileIdStone,
                            WorldGeneratorTools.TileIdDirt,
                            WorldGeneratorTools.TileIdGrass,
                            WorldGeneratorTools.TileIdGoldOre,
                            WorldGeneratorTools.TileIdIronOre,
                            WorldGeneratorTools.TileIdCoalOre
                        };
                    }
                    if (blocktype == WorldGeneratorTools.TileIdGravel)
                    {
                        density = 1;
                        allowin = new int[] {
                            WorldGeneratorTools.TileIdDirt, 
                            WorldGeneratorTools.TileIdStone, 
                            WorldGeneratorTools.TileIdSand, 
                            WorldGeneratorTools.TileIdGoldOre, 
                            WorldGeneratorTools.TileIdIronOre, 
                            WorldGeneratorTools.TileIdCoalOre 
                        };
                    }

                    MakeCuboid(map, (int)curx - sizex / 2 + dx, (int)cury - sizey / 2 + dy, (int)curz - sizez / 2 + dz, sizex, sizey, sizez, blocktype, allowin, density, rnd);
                }
            }
        }

        /// <summary>
        /// Creates a cuboid at the given location and using the given sizes.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="sizex"></param>
        /// <param name="sizey"></param>
        /// <param name="sizez"></param>
        /// <param name="blocktype"></param>
        /// <param name="allowin"></param>
        /// <param name="chance"></param>
        /// <param name="rnd"></param>
        public static void MakeCuboid(IMapStorage map, int x, int y, int z, int sizex, int sizey, int sizez, int blocktype, int[] allowin, double chance, Random rnd)
        {
            for (int xx = 0; xx < sizex; xx++)
            {
                for (int yy = 0; yy < sizey; yy++)
                {
                    for (int zz = 0; zz < sizez; zz++)
                    {
                        if (MapUtil.IsValidPos(map, x + xx, y + yy, z + zz))
                        {
                            int t = map.GetBlock(x + xx, y + yy, z + zz);
                            if (allowin == null) { goto ok; }
                            foreach (int tt in allowin)
                            {
                                if (tt == t) { goto ok; }
                            }
                            continue;
                        ok:
                            if (rnd.NextDouble() < chance)
                            {
                                map.SetBlock(x + xx, y + yy, z + zz, blocktype);
                            }
                        }
                    }
                }
            }
        }
    }
}

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
    }
}

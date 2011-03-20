#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools
{
    /// <summary>
    /// Contains tools for any IWorldGenerator.
    /// </summary>
    public static class WorldGeneratorTools
    {
        // TODO: hard-coded values should be removed, instead use one of the already existing enums!
        public const int TileIdEmpty = 0;
        public const int TileIdGrass = 2;
        public const int TileIdDirt = 3;
        public const int TileIdWater = 8;
        public const int TileIdSand = 12;
        public const int TileIdGravel = 13;
        public const int TileIdTreeTrunk = 17;
        public const int TileIdLeaves = 18;
        public const int TileIdStone = 1;
        public const int TileIdGoldOre = 14;
        public const int TileIdIronOre = 15;
        public const int TileIdCoalOre = 16;
        public const int TileIdLava = 11;
        public const int TileIdYellowFlower = 37;
        public const int TileIdRedFlower = 38;
        public const int TileIdApples = 106;
        public const int TileIdHay = 107;
    }
}

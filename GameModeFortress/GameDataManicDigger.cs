using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using ManicDigger.Collisions;

namespace GameModeFortress
{
    public class GameDataManicDigger
    {
        public static int railstart = (11 * 16);
        public static bool IsRailTile(int tiletype)
        {
            return tiletype >= railstart && tiletype < railstart + 64;
        }
    }
    public enum TileTypeManicDigger
    {
        BrushedMetal = 100,
        ChemicalGreen,
        Salt,
        Roof,
        Camouflage,
        DirtForFarming,
        Apples,
        Hay,
        Crops1,
        Crops2,
        Crops3,
        Crops4,
        CraftingTable,
        Minecart,
        Trampoline,
        FillStart,
        Cuboid,
        FillArea,
        Water0,
        Water1,
        Water2,
        Water3,
        Water4,
        Water5,
        Water6,
        Water7,
    }
}

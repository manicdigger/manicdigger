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
        public static bool IsDoorTile(int tiletype)
        {
            if (tiletype == (int)TileTypeManicDigger.DoorBottomClosed ||
                tiletype == (int)TileTypeManicDigger.DoorTopClosed ||
                tiletype == (int)TileTypeManicDigger.DoorBottomOpen ||
                tiletype == (int)TileTypeManicDigger.DoorTopOpen)
            {
                return true;
            }
            return false;
        }
    }
    // TODO: Move completly to GameData
    public enum TileTypeManicDigger
    {
        // Minecraft tiles
        // http://www.minecraftwiki.net/wiki/Blocks,Items_%26_Data_values
        Empty = 0,
        Stone,
        Grass,
        Dirt,
        Cobblestone,
        Wood,
        Sapling,
        Adminium,
        Water,
        StationaryWater,
        Lava,
        StationaryLava,
        Sand,
        Gravel,
        GoldOre,
        IronOre,
        CoalOre,
        TreeTrunk,
        Leaves,
        Sponge,
        Glass,
        RedCloth,
        OrangeCloth,
        YellowCloth,
        LightGreenCloth,
        GreenCloth,
        AquaGreenCloth,
        CyanCloth,
        BlueCloth,
        PurpleCloth,
        IndigoCloth,
        VioletCloth,
        //dec  hex  Block type  ,
        MagentaCloth,
        PinkCloth,
        BlackCloth,
        GrayCloth,
        WhiteCloth,
        YellowFlowerDecorations,
        RedRoseDecorations,
        RedMushroom,
        BrownMushroom,
        GoldBlock,
        IronBlock,
        DoubleStair,
        Stair,
        Brick,
        TNT,
        Bookcase,
        MossyCobblestone,
        Obsidian,
        Torch,
        FireBlock,
        InfiniteWaterSource,
        InfiniteLavaSource,
        Chest,
        Gear,
        DiamondPre,
        DiamondBlock,
        CraftingTable,
        Crops,
        Soil,
        Furnace,
        BurningFurnace,
        // Manic Digger tiles
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
        //CraftingTable,
        Minecart = 113,
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
        DoorBottomClosed,
        DoorTopClosed,
        DoorBottomOpen,
        DoorTopOpen,
    }
}

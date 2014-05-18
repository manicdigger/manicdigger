using System;
using System.Collections.Generic;

namespace ManicDigger.Mods
{
    public class Default : IMod
    {
        public void PreStart(ModManager m) { }
        public void Start(ModManager manager)
        {
            m = manager;
            m.RenderHint(RenderHint.Fast);
            if (m.IsSinglePlayer())
            {
                m.SetPlayerAreaSize(512);
            }
            else
            {
                m.SetPlayerAreaSize(256);
            }
            solidSounds = new SoundSet()
            {
                Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
                Break = new string[] { "destruct" },
                Build = new string[] { "build" },
                Clone = new string[] { "clone" },
            };
            snowSounds = new SoundSet()
            {
                Walk = new string[] { "walksnow1", "walksnow2", "walksnow3", "walksnow4" },
                Break = new string[] { "destruct" },
                Build = new string[] { "build" },
                Clone = new string[] { "clone" },
            };
            m.SetDefaultSounds(solidSounds);
            noSound = new SoundSet();
            m.SetBlockType(0, "Empty", new BlockType()
                {
                    DrawType = DrawType.Empty,
                    WalkableType = WalkableType.Empty,
                    Sounds = noSound,
                });
            m.SetBlockType(1, "Stone", new BlockType()
                {
                    AllTextures = "Stone",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(2, "Grass", new BlockType()
                {
                    TextureIdTop = "Grass",
                    TextureIdBack = "GrassSide",
                    TextureIdFront = "GrassSide",
                    TextureIdLeft = "GrassSide",
                    TextureIdRight = "GrassSide",
                    TextureIdForInventory = "GrassSide",
                    TextureIdBottom = "Dirt",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    WhenPlayerPlacesGetsConvertedTo = 3,
                });
            m.SetBlockType(3, "Dirt", new BlockType()
                {
                    AllTextures = "Dirt",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(4, "Cobblestone", new BlockType()
                {
                    AllTextures = "Cobblestone",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(5, "Wood", new BlockType()
                {
                    AllTextures = "Wood",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(6, "Sapling", new BlockType()
                {
                    AllTextures = "Sapling",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(7, "Adminium", new BlockType()
                {
                    AllTextures = "Adminium",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(8, "Water", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = noSound,
                });
            m.SetBlockType(9, "StationaryWater", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = noSound,
                });
            m.SetBlockType(10, "Lava", new BlockType()
                {
                    AllTextures = "Lava",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = noSound,
                    LightRadius = 15,
                    DamageToPlayer = 2,
                });
            m.SetBlockType(11, "StationaryLava", new BlockType()
                {
                    AllTextures = "Lava",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = noSound,
                    LightRadius = 15,
                    DamageToPlayer = 2,
                });
            m.SetBlockType(12, "Sand", new BlockType()
                {
                    AllTextures = "Sand",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(13, "Gravel", new BlockType()
                {
                    AllTextures = "Gravel",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(14, "GoldOre", new BlockType()
                {
                    AllTextures = "GoldOre",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(15, "IronOre", new BlockType()
                {
                    AllTextures = "IronOre",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(16, "CoalOre", new BlockType()
                {
                    AllTextures = "CoalOre",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(17, "TreeTrunk", new BlockType()
                {
                    TextureIdTop = "TreeTrunkTopBottom",
                    TextureIdBottom = "TreeTrunkTopBottom",
                    TextureIdBack = "TreeTrunk",
                    TextureIdFront = "TreeTrunk",
                    TextureIdLeft = "TreeTrunk",
                    TextureIdRight = "TreeTrunk",
                    TextureIdForInventory = "TreeTrunk",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(18, "Leaves", new BlockType()
                {
                    AllTextures = "Leaves",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(19, "Sponge", new BlockType()
                {
                    AllTextures = "Sponge",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(20, "Glass", new BlockType()
                {
                    AllTextures = "Glass",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(21, "RedCloth", new BlockType()
                {
                    AllTextures = "RedCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(22, "OrangeCloth", new BlockType()
                {
                    AllTextures = "OrangeCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(23, "YellowCloth", new BlockType()
                {
                    AllTextures = "YellowCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(24, "LightGreenCloth", new BlockType()
                {
                    AllTextures = "LightGreenCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(25, "GreenCloth", new BlockType()
                {
                    AllTextures = "GreenCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(26, "AquaGreenCloth", new BlockType()
                {
                    AllTextures = "AquaGreenCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(27, "CyanCloth", new BlockType()
                {
                    AllTextures = "CyanCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(28, "BlueCloth", new BlockType()
                {
                    AllTextures = "BlueCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(29, "PurpleCloth", new BlockType()
                {
                    AllTextures = "PurpleCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(30, "IndigoCloth", new BlockType()
                {
                    AllTextures = "IndigoCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(31, "VioletCloth", new BlockType()
                {
                    AllTextures = "VioletCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(32, "MagentaCloth", new BlockType()
                {
                    AllTextures = "MagentaCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(33, "PinkCloth", new BlockType()
                {
                    AllTextures = "PinkCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(34, "BlackCloth", new BlockType()
                {
                    AllTextures = "BlackCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(35, "GrayCloth", new BlockType()
                {
                    AllTextures = "GrayCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(36, "WhiteCloth", new BlockType()
                {
                    AllTextures = "WhiteCloth",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(37, "YellowFlowerDecorations", new BlockType()
                {
                    AllTextures = "YellowFlowerDecorations",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(38, "RedRoseDecorations", new BlockType()
                {
                    AllTextures = "RedRoseDecorations",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(39, "RedMushroom", new BlockType()
                {
                    AllTextures = "RedMushroom",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(40, "BrownMushroom", new BlockType()
                {
                    AllTextures = "BrownMushroom",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(41, "GoldBlock", new BlockType()
                {
                    AllTextures = "GoldBlock",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(42, "IronBlock", new BlockType()
                {
                    AllTextures = "IronBlock",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(43, "DoubleStair", new BlockType()
                {
                    TextureIdTop = "Stair",
                    TextureIdBottom = "Stair",
                    TextureIdBack = "DoubleStairSide",
                    TextureIdFront = "DoubleStairSide",
                    TextureIdLeft = "DoubleStairSide",
                    TextureIdRight = "DoubleStairSide",
                    TextureIdForInventory = "DoubleStairSide",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(44, "Stair", new BlockType()
                {
                    TextureIdTop = "Stair",
                    TextureIdBottom = "Stair",
                    TextureIdBack = "StairSide",
                    TextureIdFront = "StairSide",
                    TextureIdLeft = "StairSide",
                    TextureIdRight = "StairSide",
                    TextureIdForInventory = "Stair",
                    DrawType = DrawType.HalfHeight,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(45, "Brick", new BlockType()
                {
                    AllTextures = "Brick",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(47, "Bookcase", new BlockType()
                {
                    TextureIdTop = "Wood",
                    TextureIdBottom = "Wood",
                    TextureIdBack = "Bookcase",
                    TextureIdFront = "Bookcase",
                    TextureIdLeft = "Bookcase",
                    TextureIdRight = "Bookcase",
                    TextureIdForInventory = "Bookcase",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(48, "MossyCobblestone", new BlockType()
                {
                    AllTextures = "MossyCobblestone",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(49, "Obsidian", new BlockType()
                {
                    AllTextures = "Obsidian",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(50, "Torch", new BlockType()
                {
                    TextureIdTop = "TorchTop",
                    TextureIdBottom = "Torch",
                    TextureIdBack = "Torch",
                    TextureIdFront = "Torch",
                    TextureIdLeft = "Torch",
                    TextureIdRight = "Torch",
                    TextureIdForInventory = "Torch",
                    LightRadius = 15,
                    DrawType = DrawType.Torch,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(51, "FireBlock", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(52, "InfiniteWaterSource", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(53, "InfiniteLavaSource", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(54, "Chest", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(55, "Gear", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(56, "DiamondPre", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(57, "DiamondBlock", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(58, "CraftingTable1", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(59, "Crops", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(60, "Soil", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(61, "Furnace", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(62, "BurningFurnace", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(100, "BrushedMetal", new BlockType()
                {
                    AllTextures = "BrushedMetal",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(101, "ChemicalGreen", new BlockType()
                {
                    AllTextures = "ChemicalGreen",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(102, "Salt", new BlockType()
                {
                    AllTextures = "Salt",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(103, "Roof", new BlockType()
                {
                    AllTextures = "Roof",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(104, "Camouflage", new BlockType()
                {
                    AllTextures = "Camouflage",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(105, "DirtForFarming", new BlockType()
                {
                    AllTextures = "DirtForFarming",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(106, "Apples", new BlockType()
                {
                    AllTextures = "Apples",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(107, "Hay", new BlockType()
                {
                    AllTextures = "Hay",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(108, "Crops1", new BlockType()
                {
                    AllTextures = "Crops1",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(109, "Crops2", new BlockType()
                {
                    AllTextures = "Crops2",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(110, "Crops3", new BlockType()
                {
                    AllTextures = "Crops3",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(111, "Crops4", new BlockType()
                {
                    AllTextures = "Crops4",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(112, "CraftingTable", new BlockType()
                {
                    TextureIdTop = "CraftingTableTopBottom",
                    TextureIdBack = "CraftingTableSide",
                    TextureIdFront = "CraftingTableSide",
                    TextureIdLeft = "CraftingTableSide",
                    TextureIdRight = "CraftingTableSide",
                    TextureIdForInventory = "CraftingTableTopBottom",
                    TextureIdBottom = "CraftingTableTopBottom",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(113, "Minecart", new BlockType()
                {
                    AllTextures = "Minecart",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(114, "Trampoline", new BlockType()
                {
                    AllTextures = "Trampoline",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(115, "FillStart", new BlockType()
                {
                    AllTextures = "FillStart",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(116, "Cuboid", new BlockType()
                {
                    AllTextures = "FillEnd",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(117, "FillArea", new BlockType()
                {
                    AllTextures = "FillArea",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(118, "Water0", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(119, "Water1", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(120, "Water2", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(121, "Water3", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(122, "Water4", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(123, "Water5", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(124, "Water6", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(125, "Water7", new BlockType()
                {
                    AllTextures = "Water",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            for (int i = 0; i < 64; i++)
            {
                m.SetBlockType(176 + i, "Rail" + i.ToString(), new BlockType()
                    {
                        TextureIdTop = "Rail" + i.ToString(),
                        TextureIdBottom = "Cobblestone",
                        TextureIdBack = "Cobblestone",
                        TextureIdFront = "Cobblestone",
                        TextureIdLeft = "Cobblestone",
                        TextureIdRight = "Cobblestone",
                        TextureIdForInventory = "Rail" + i.ToString(),
                        DrawType = DrawType.Transparent,
                        WalkableType = WalkableType.Solid,
                        Sounds = solidSounds,
                        Rail = i,
                    });
            }
            m.SetBlockType(130, "GrassTrap", new BlockType()
                {
                    TextureIdTop = "Grass",
                    TextureIdBack = "GrassSide",
                    TextureIdFront = "GrassSide",
                    TextureIdLeft = "GrassSide",
                    TextureIdRight = "GrassSide",
                    TextureIdForInventory = "GrassSide",
                    TextureIdBottom = "Dirt",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(131, "GoldCoin", new BlockType()
                {
                    AllTextures = "GoldCoin",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(132, "GoldBar", new BlockType()
                {
                    TextureIdTop = "GoldBarTopBottom",
                    TextureIdBack = "GoldBarSide",
                    TextureIdFront = "GoldBarSide",
                    TextureIdLeft = "GoldBarSide",
                    TextureIdRight = "GoldBarSide",
                    TextureIdForInventory = "GoldBarInvetory",
                    TextureIdBottom = "GoldBarTopBottom",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    LightRadius = 5,
                });
            m.SetBlockType(133, "SilverOre", new BlockType()
                {
                    AllTextures = "SilverOre",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(135, "SilverCoin", new BlockType()
                {
                    AllTextures = "SilverCoin",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(140, "DirtBrick", new BlockType()
                {
                    AllTextures = "DirtBrick",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(141, "LuxuryRoof", new BlockType()
                {
                    AllTextures = "LuxuryRoof",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(142, "SandBrick", new BlockType()
                {
                    AllTextures = "SandBrick",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(143, "FakeBookcase", new BlockType()
                {
                    TextureIdTop = "Wood",
                    TextureIdBottom = "Wood",
                    TextureIdBack = "Bookcase",
                    TextureIdFront = "Bookcase",
                    TextureIdLeft = "Bookcase",
                    TextureIdRight = "Bookcase",
                    TextureIdForInventory = "Bookcase",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                });
            m.SetBlockType(144, "WoodDesk", new BlockType()
                {
                    TextureIdTop = "WoodBlock",
                    TextureIdBottom = "Empty",
                    TextureIdBack = "GlassDeskSide",
                    TextureIdFront = "GlassDeskSide",
                    TextureIdLeft = "GlassDeskSide",
                    TextureIdRight = "GlassDeskSide",
                    TextureIdForInventory = "WoodBlock",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(145, "GlassDesk", new BlockType()
                {
                    TextureIdTop = "Glass",
                    TextureIdBottom = "Empty",
                    TextureIdBack = "GlassDeskSide",
                    TextureIdFront = "GlassDeskSide",
                    TextureIdLeft = "GlassDeskSide",
                    TextureIdRight = "GlassDeskSide",
                    TextureIdForInventory = "GlassDeskSide",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(146, "Mosaik", new BlockType()
                {
                    AllTextures = "Mosaik",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(147, "Asphalt", new BlockType()
                {
                    AllTextures = "Asphalt",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(148, "Cake", new BlockType()
                {
                    TextureIdTop = "CakeTop",
                    TextureIdBottom = "Gray",
                    TextureIdBack = "CakeSide",
                    TextureIdFront = "CakeSide",
                    TextureIdLeft = "CakeSide",
                    TextureIdRight = "CakeSide",
                    TextureIdForInventory = "CakeTop",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(149, "Fire", new BlockType()
                {
                    AllTextures = "Fire",
                    LightRadius = 15,
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                    DamageToPlayer = 2,
                });
            m.SetBlockType(150, "Fence", new BlockType()
                {
                    AllTextures = "Fence",
                    DrawType = DrawType.Fence,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(151, "Compass", new BlockType()
                {
                    AllTextures = "Compass",
                    TextureIdForInventory = "CompassInventory",
                    DrawType = DrawType.Plant,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(152, "Ladder", new BlockType()
                {
                    AllTextures = "Ladder",
                    DrawType = DrawType.Ladder,
                    WalkableType = WalkableType.Fluid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(153, "EmptyHand", new BlockType()
                {
                    AllTextures = "YellowThing",
                    DrawType = DrawType.Torch,
                    WalkableType = WalkableType.Empty,
                    Sounds = noSound,
                });
            
            m.RegisterTimer(UpdateSeasons, 1);
            
            m.SetGameDayRealHours(1);
            m.SetGameYearRealHours(24);
            
            //Creative inventory
            m.AddToCreativeInventory("Stone");
            m.AddToCreativeInventory("Dirt");
            m.AddToCreativeInventory("Cobblestone");
            m.AddToCreativeInventory("Wood");
            m.AddToCreativeInventory("Sapling");
            //m.AddToCreativeInventory("Adminium");
            m.AddToCreativeInventory("Water");
            m.AddToCreativeInventory("Lava");
            m.AddToCreativeInventory("Sand");
            m.AddToCreativeInventory("Gravel");
            m.AddToCreativeInventory("GoldOre");
            m.AddToCreativeInventory("IronOre");
            m.AddToCreativeInventory("CoalOre");
            m.AddToCreativeInventory("TreeTrunk");
            m.AddToCreativeInventory("Leaves");
            m.AddToCreativeInventory("Sponge");
            m.AddToCreativeInventory("Glass");
            m.AddToCreativeInventory("RedCloth");
            m.AddToCreativeInventory("OrangeCloth");
            m.AddToCreativeInventory("YellowCloth");
            m.AddToCreativeInventory("LightGreenCloth");
            m.AddToCreativeInventory("GreenCloth");
            m.AddToCreativeInventory("AquaGreenCloth");
            m.AddToCreativeInventory("CyanCloth");
            m.AddToCreativeInventory("BlueCloth");
            m.AddToCreativeInventory("PurpleCloth");
            m.AddToCreativeInventory("IndigoCloth");
            m.AddToCreativeInventory("VioletCloth");
            m.AddToCreativeInventory("MagentaCloth");
            m.AddToCreativeInventory("PinkCloth");
            m.AddToCreativeInventory("BlackCloth");
            m.AddToCreativeInventory("GrayCloth");
            m.AddToCreativeInventory("WhiteCloth");
            m.AddToCreativeInventory("YellowFlowerDecorations");
            m.AddToCreativeInventory("RedRoseDecorations");
            m.AddToCreativeInventory("RedMushroom");
            m.AddToCreativeInventory("BrownMushroom");
            m.AddToCreativeInventory("GoldBlock");
            m.AddToCreativeInventory("IronBlock");
            m.AddToCreativeInventory("DoubleStair");
            m.AddToCreativeInventory("Stair");
            m.AddToCreativeInventory("Brick");
            m.AddToCreativeInventory("Bookcase");
            m.AddToCreativeInventory("MossyCobblestone");
            m.AddToCreativeInventory("Obsidian");
            m.AddToCreativeInventory("Torch");
            m.AddToCreativeInventory("BrushedMetal");
            m.AddToCreativeInventory("ChemicalGreen");
            m.AddToCreativeInventory("Salt");
            m.AddToCreativeInventory("Roof");
            m.AddToCreativeInventory("Camouflage");
            m.AddToCreativeInventory("DirtForFarming");
            m.AddToCreativeInventory("Apples");
            m.AddToCreativeInventory("Hay");
            m.AddToCreativeInventory("Crops1");
            m.AddToCreativeInventory("CraftingTable");
            m.AddToCreativeInventory("Trampoline");
            m.AddToCreativeInventory("FillStart");
            m.AddToCreativeInventory("Cuboid");
            m.AddToCreativeInventory("FillArea");
            m.AddToCreativeInventory("GrassTrap");
            m.AddToCreativeInventory("GoldCoin");
            m.AddToCreativeInventory("GoldBar");
            m.AddToCreativeInventory("SilverOre");
            m.AddToCreativeInventory("SilverCoin");
            m.AddToCreativeInventory("DirtBrick");
            m.AddToCreativeInventory("LuxuryRoof");
            m.AddToCreativeInventory("SandBrick");
            m.AddToCreativeInventory("FakeBookcase");
            m.AddToCreativeInventory("WoodDesk");
            m.AddToCreativeInventory("GlassDesk");
            m.AddToCreativeInventory("Mosaik");
            m.AddToCreativeInventory("Asphalt");
            m.AddToCreativeInventory("Cake");
            m.AddToCreativeInventory("Fire");
            m.AddToCreativeInventory("Fence");
            m.AddToCreativeInventory("Compass");
            m.AddToCreativeInventory("Ladder");
            m.AddToCreativeInventory("Rail3");
            m.AddToCreativeInventory("Rail60");
            
            m.AddToStartInventory("Torch", 6);
            m.AddToStartInventory("Crops1", 1);
            m.AddToStartInventory("CraftingTable", 6);
            m.AddToStartInventory("GoldCoin", 2);
            m.AddToStartInventory("GoldBar", 5);
            m.AddToStartInventory("SilverCoin", 1);
            m.AddToStartInventory("Compass", 1);
            
            
            m.AddCraftingRecipe("Cobblestone", 1, "Stone", 2);
            m.AddCraftingRecipe("Stone", 2, "Cobblestone", 1);
            m.AddCraftingRecipe("Wood", 2, "TreeTrunk", 1);
            m.AddCraftingRecipe("Brick", 1, "Stone", 4);
            m.AddCraftingRecipe2("GoldBlock", 1, "CoalOre", 1, "GoldOre", 1);
            m.AddCraftingRecipe2("IronBlock", 1, "CoalOre", 1, "IronOre", 1);
            m.AddCraftingRecipe2("Rail3", 4, "Wood", 1, "IronBlock", 1);
            m.AddCraftingRecipe2("Rail60", 2, "Wood", 1, "IronBlock", 1);
            m.AddCraftingRecipe("CraftingTable", 1, "Wood", 3);
            m.AddCraftingRecipe("Stair", 1, "Stone", 2);
            m.AddCraftingRecipe("DoubleStair", 1, "Stone", 2);
            m.AddCraftingRecipe("Glass", 1, "Sand", 2);
            m.AddCraftingRecipe("RedRoseDecorations", 1, "Leaves", 10);
            m.AddCraftingRecipe("YellowFlowerDecorations", 1, "Leaves", 10);
            m.AddCraftingRecipe("Sapling", 1, "Leaves", 3);
            m.AddCraftingRecipe("RedMushroom", 1, "Dirt", 10);
            m.AddCraftingRecipe("BrownMushroom", 1, "Dirt", 10);
            m.AddCraftingRecipe("RedMushroom", 1, "Grass", 10);
            m.AddCraftingRecipe("BrownMushroom", 1, "Grass", 10);
            m.AddCraftingRecipe("Bookcase", 1, "Wood", 2);
            m.AddCraftingRecipe("MossyCobblestone", 1, "Cobblestone", 1);
            m.AddCraftingRecipe("Cobblestone", 1, "MossyCobblestone", 1);
            m.AddCraftingRecipe("Sponge", 1, "GoldBlock", 1);
            m.AddCraftingRecipe("RedCloth", 1, "GoldBlock", 1);
            m.AddCraftingRecipe("OrangeCloth", 1, "RedCloth", 1);
            m.AddCraftingRecipe("YellowCloth", 1, "OrangeCloth", 1);
            m.AddCraftingRecipe("LightGreenCloth", 1, "YellowCloth", 1);
            m.AddCraftingRecipe("GreenCloth", 1, "LightGreenCloth", 1);
            m.AddCraftingRecipe("AquaGreenCloth", 1, "GreenCloth", 1);
            m.AddCraftingRecipe("CyanCloth", 1, "AquaGreenCloth", 1);
            m.AddCraftingRecipe("BlueCloth", 1, "CyanCloth", 1);
            m.AddCraftingRecipe("PurpleCloth", 1, "BlueCloth", 1);
            m.AddCraftingRecipe("IndigoCloth", 1, "PurpleCloth", 1);
            m.AddCraftingRecipe("VioletCloth", 1, "IndigoCloth", 1);
            m.AddCraftingRecipe("MagentaCloth", 1, "VioletCloth", 1);
            m.AddCraftingRecipe("PinkCloth", 1, "MagentaCloth", 1);
            m.AddCraftingRecipe("BlackCloth", 1, "PinkCloth", 1);
            m.AddCraftingRecipe("GrayCloth", 1, "BlackCloth", 1);
            m.AddCraftingRecipe("WhiteCloth", 1, "GrayCloth", 1);
            m.AddCraftingRecipe("RedCloth", 1, "WhiteCloth", 1);
            m.AddCraftingRecipe("Roof", 1, "Brick", 2);
            m.AddCraftingRecipe("ChemicalGreen", 1, "GoldBlock", 1);
            m.AddCraftingRecipe("Camouflage", 1, "GoldBlock", 1);
            m.AddCraftingRecipe("DirtForFarming", 1, "Dirt", 2);
            m.AddCraftingRecipe("DirtForFarming", 1, "Grass", 2);
            m.AddCraftingRecipe("Crops1", 2, "Crops4", 1);
            m.AddCraftingRecipe2("BrushedMetal", 1, "IronBlock", 1, "CoalOre", 1);
            m.AddCraftingRecipe("Minecart", 1, "BrushedMetal", 5);
            m.AddCraftingRecipe2("Trampoline", 1, "BrushedMetal", 1, "Wood", 1);
            m.AddCraftingRecipe2("Torch", 1, "Wood", 1, "CoalOre", 1);
            m.AddCraftingRecipe2("GrassTrap", 1, "Dirt", 10, "Camouflage", 5);
            m.AddCraftingRecipe2("Sapling", 10, "Apples", 5, "DirtForFarming", 1);
            m.AddCraftingRecipe2("DirtBrick", 1, "Dirt", 2, "Stone", 1);
            m.AddCraftingRecipe("Salt", 1, "Crops4", 2);
            m.AddCraftingRecipe("LuxuryRoof", 1, "Roof", 2);
            m.AddCraftingRecipe2("SandBrick", 1, "Sand", 1, "Stone", 2);
            m.AddCraftingRecipe("Fence", 1, "TreeTrunk", 2);
            m.AddCraftingRecipe2("FakeBookcase", 1, "Bookcase", 1, "Camouflage", 5);
            m.AddCraftingRecipe2("WoodDesk", 1, "Wood", 2, "TreeTrunk", 1);
            m.AddCraftingRecipe2("GlassDesk", 1, "Glass", 2, "TreeTrunk", 1);
            m.AddCraftingRecipe3("Mosaik", 1, "Sand", 2, "Gravel", 1, "Stone", 1);
            m.AddCraftingRecipe2("Asphalt", 1, "CoalOre", 1, "Gravel", 2);
            m.AddCraftingRecipe("Hay", 1, "Crops4", 4);
            m.AddCraftingRecipe2("Cake", 1, "Salt", 2, "Crops4", 4);
            m.AddCraftingRecipe2("Fire", 1, "TreeTrunk", 1, "Torch", 1);
            m.AddCraftingRecipe("SilverCoin", 1, "SilverOre", 1);
            m.AddCraftingRecipe("SilverCoin", 30, "GoldCoin", 1);
            m.AddCraftingRecipe2("GoldCoin", 1, "SilverCoin", 25, "GoldOre", 5);
            m.AddCraftingRecipe("GoldCoin", 30, "GoldBar", 1);
            m.AddCraftingRecipe2("GoldBar", 1, "GoldCoin", 25, "GoldBlock", 5);
            m.AddCraftingRecipe("Ladder", 1, "Wood", 4);
            
            
            m.SetSunLevels(sunLevels);
            m.SetLightLevels(lightLevels);
            
            m.RegisterOnCommand(OnCommandSetModel);
            m.RegisterOnSpecialKey(OnRespawnKey);
            m.RegisterOnSpecialKey(OnSetSpawnKey);
            m.RegisterOnPlayerDeath(OnPlayerDeath);
        }
        
        Dictionary<string, float[]> spawnPositions = new Dictionary<string, float[]>();
        
        void OnSetSpawnKey(int player, SpecialKey key)
        {
            if (key != SpecialKey.SetSpawn)
            {
                return;
            }
            float[] pos = new float[3];
            pos[0] = m.GetPlayerPositionX(player);
            pos[1] = m.GetPlayerPositionY(player);
            pos[2] = m.GetPlayerPositionZ(player);
            spawnPositions[m.GetPlayerName(player)] = pos;
            m.SendMessage(player, "Spawn position set.");
        }
        
        void OnRespawnKey(int player, SpecialKey key)
        {
            if (key != SpecialKey.Respawn)
            {
                return;
            }
            Respawn(player);
            m.SendMessage(player, "Respawn.");
        }
        
        string ColoredPlayername(int player)
        {
            //Returns the player name in group color
            return string.Format("{0}{1}", m.GetGroupColor(player), m.GetPlayerName(player));
        }
        
        void OnPlayerDeath(int player, DeathReason reason, int sourceID)
        {
            Respawn(player);
            string deathMessage = "";
            switch (reason)
            {
                case DeathReason.FallDamage:
                    deathMessage = string.Format("{0} &7was doomed to fall.", ColoredPlayername(player));
                    break;
                case DeathReason.BlockDamage:
                    if (sourceID == m.GetBlockId("Lava"))
                    {
                        deathMessage = string.Format("{0} &7thought they could swim in Lava.", ColoredPlayername(player));
                    }
                    else if (sourceID == m.GetBlockId("Fire"))
                    {
                        deathMessage = string.Format("{0} &7was burned alive.", ColoredPlayername(player));
                    }
                    else
                    {
                        deathMessage = string.Format("{0} &7was killed by {1}.", ColoredPlayername(player), m.GetBlockName(sourceID));
                    }
                    break;
                case DeathReason.Drowning:
                    deathMessage = string.Format("{0} &7tried to breathe under water.", ColoredPlayername(player));
                    break;
                case DeathReason.Explosion:
                    deathMessage = string.Format("{0} &7was blown into pieces by {1}.", ColoredPlayername(player), ColoredPlayername(sourceID));
                    break;
                default:
                    deathMessage = string.Format("{0} &7died.", ColoredPlayername(player));
                    break;
            }
            m.SendMessageToAll(deathMessage);
        }
        
        void Respawn(int player)
        {
            if (!spawnPositions.ContainsKey(m.GetPlayerName(player)))
            {
                float[] pos = m.GetDefaultSpawnPosition(player);
                m.SetPlayerPosition(player, pos[0], pos[1], pos[2]);
            }
            else
            {
                float[] pos = (float[])spawnPositions[m.GetPlayerName(player)];
                m.SetPlayerPosition(player, pos[0], pos[1], pos[2]);
            }
        }
        
        bool OnCommandSetModel(int player, string command, string argument)
        {
            if (command == "setmodel")
            {
                if (!(m.PlayerHasPrivilege(player, "setmodel") || m.IsSinglePlayer()))
                {
                    m.SendMessage(player, "No setmodel privilege.");
                    return true;
                }
                string[] ss = argument.Split(' ');
                string targetplayername = ss[0];
                string modelname = ss[1];
                string texturename = null;
                if (ss.Length >= 3)
                {
                    texturename = ss[2];
                }
                bool found = false;
                foreach (int p in m.AllPlayers())
                {
                    if (m.GetPlayerName(p).Equals(targetplayername, StringComparison.InvariantCultureIgnoreCase))
                    {
                        m.SetPlayerModel(p, modelname, texturename);
                        found = true;
                    }
                }
                if (!found)
                {
                    m.SendMessage(player, string.Format("Player {0} not found", targetplayername));
                }
                return true;
            }
            return false;
        }
        
        /*
        bool red = false;
        void f()
        {
        red = !red;
        m.SetPlayerModel(0, "zombie.txt", red ? "playerred.png" : "playergreen.png");
        }
        */
        
        ModManager m;
        SoundSet solidSounds;
        SoundSet snowSounds;
        SoundSet noSound;
        
        int lastseason;
        void UpdateSeasons()
        {
            int currentSeason = (int)((m.GetCurrentYearTotal() % 1) * 4);
            if (currentSeason != lastseason)
            {
                // spring
                if (currentSeason == 0)
                {
                    m.SetBlockType(2, "Grass", new BlockType()
                        {
                            TextureIdTop = "SpringGrass",
                            TextureIdBack = "SpringGrassSide",
                            TextureIdFront = "SpringGrassSide",
                            TextureIdLeft = "SpringGrassSide",
                            TextureIdRight = "SpringGrassSide",
                            TextureIdForInventory = "SpringGrassSide",
                            TextureIdBottom = "Dirt",
                            DrawType = DrawType.Solid,
                            WalkableType = WalkableType.Solid,
                            Sounds = snowSounds,
                        });
                    m.SetBlockType(18, "Leaves", new BlockType()
                        {
                            AllTextures = "Leaves",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(106, "Apples", new BlockType()
                        {
                            AllTextures = "Apples",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                }
                // summer
                if (currentSeason == 1)
                {
                    m.SetBlockType(2, "Grass", new BlockType()
                        {
                            TextureIdTop = "Grass",
                            TextureIdBack = "GrassSide",
                            TextureIdFront = "GrassSide",
                            TextureIdLeft = "GrassSide",
                            TextureIdRight = "GrassSide",
                            TextureIdForInventory = "GrassSide",
                            TextureIdBottom = "Dirt",
                            DrawType = DrawType.Solid,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(18, "Leaves", new BlockType()
                        {
                            AllTextures = "Leaves",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(106, "Apples", new BlockType()
                        {
                            AllTextures = "Apples",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(8, "Water", new BlockType()
                        {
                            AllTextures = "Water",
                            DrawType = DrawType.Fluid,
                            WalkableType = WalkableType.Fluid,
                            Sounds = noSound,
                        });
                }
                // autumn
                if (currentSeason == 2)
                {
                    m.SetBlockType(2, "Grass", new BlockType()
                        {
                            TextureIdTop = "AutumnGrass",
                            TextureIdBack = "AutumnGrassSide",
                            TextureIdFront = "AutumnGrassSide",
                            TextureIdLeft = "AutumnGrassSide",
                            TextureIdRight = "AutumnGrassSide",
                            TextureIdForInventory = "AutumnGrassSide",
                            TextureIdBottom = "Dirt",
                            DrawType = DrawType.Solid,
                            WalkableType = WalkableType.Solid,
                            Sounds = snowSounds,
                        });
                    m.SetBlockType(18, "Leaves", new BlockType()
                        {
                            AllTextures = "AutumnLeaves",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(106, "Apples", new BlockType()
                        {
                            AllTextures = "AutumnApples",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                }
                // winter
                if (currentSeason == 3)
                {
                    m.SetBlockType(2, "Grass", new BlockType()
                        {
                            TextureIdTop = "WinterGrass",
                            TextureIdBack = "WinterGrassSide",
                            TextureIdFront = "WinterGrassSide",
                            TextureIdLeft = "WinterGrassSide",
                            TextureIdRight = "WinterGrassSide",
                            TextureIdForInventory = "WinterGrassSide",
                            TextureIdBottom = "Dirt",
                            DrawType = DrawType.Solid,
                            WalkableType = WalkableType.Solid,
                            Sounds = snowSounds,
                        });
                    m.SetBlockType(18, "Leaves", new BlockType()
                        {
                            AllTextures = "WinterLeaves",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(106, "Apples", new BlockType()
                        {
                            AllTextures = "WinterApples",
                            DrawType = DrawType.Transparent,
                            WalkableType = WalkableType.Solid,
                            Sounds = solidSounds,
                        });
                    m.SetBlockType(8, "Water", new BlockType()
                        {
                            AllTextures = "Ice",
                            DrawType = DrawType.Solid,
                            WalkableType = WalkableType.Solid,
                            Sounds = snowSounds,
                            IsSlipperyWalk = true,
                        });
                }
                m.UpdateBlockTypes();
                lastseason = currentSeason;
                
                //Readd "lost blocks" to inventory
                m.AddToCreativeInventory("Leaves");
                m.AddToCreativeInventory("Apples");
                m.AddToCreativeInventory("Water");
            }
        }
        
        float[] lightLevels = new float[]
        {
            0f,
            0.0666666667f,
            0.1333333333f,
            0.2f,
            0.2666666667f,
            0.3333333333f,
            0.4f,
            0.4666666667f,
            0.5333333333f,
            0.6f,
            0.6666666667f,
            0.7333333333f,
            0.8f,
            0.8666666667f,
            0.9333333333f,
            1f,
        };
        int[] sunLevels = new int[]
        {
            02,//0 hour
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            03,
            04,
            05,
            06,
            07,//6 hour
            08,
            09,
            10,
            11,
            12,
            13,
            14,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,//12 hour
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            14,
            13,
            12,
            11,
            10,
            09,//18 hour
            08,
            07,
            06,
            05,
            04,
            03,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
        };
    }
}

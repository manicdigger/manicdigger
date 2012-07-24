using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Mods
{
    public class Default : IMod
    {
        public void Start(ModManager m)
        {
            SoundSet solidSounds = new SoundSet()
            {
                Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
                Break = new string[] { "destruct" },
                Build = new string[] { "build" },
                Clone = new string[] { "clone" },
            };
            SoundSet noSound = new SoundSet();
            m.SetBlockType(0, "Empty", new BlockType()
            {
                DrawType = DrawType.Empty,
                WalkableType = WalkableType.Empty,
                Sounds = noSound,
            });
            m.SetBlockType(1, "Stone", new BlockType()
            {
                AllTextures = 1,//"Stone",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(2, "Grass", new BlockType()
            {
                TextureIdTop = 0, //"Grass",
                TextureIdBack = 3,//"GrassSide",
                TextureIdFront = 3, //"GrassSide",
                TextureIdLeft = 3,//"GrassSide",
                TextureIdRight = 3,//"GrassSide",
                TextureIdForInventory = 3,//"GrassSide",
                TextureIdBottom = 2, //"Dirt",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(3, "Dirt", new BlockType()
            {
                AllTextures = 2,//"Dirt",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(4, "Cobblestone", new BlockType()
            {
                AllTextures = 16,//"Cobblestone",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(5, "Wood", new BlockType()
            {
                AllTextures = 4,//"Wood",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(6, "Sapling", new BlockType()
            {
                AllTextures = 15,//"Sapling",
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(7, "Adminium", new BlockType()
            {
                AllTextures = 17,//"Adminium",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(8, "Water", new BlockType()
            {
                AllTextures = 14, //"Water",
                DrawType = DrawType.Fluid,
                WalkableType = WalkableType.Fluid,
                Sounds = noSound,
            });
            m.SetBlockType(9, "StationaryWater", new BlockType()
            {
                AllTextures = 14,//"StationaryWater",
                DrawType = DrawType.Fluid,
                WalkableType = WalkableType.Fluid,
                Sounds = noSound,
            });
            m.SetBlockType(10, "Lava", new BlockType()
            {
                AllTextures = 30,
                DrawType = DrawType.Fluid,
                WalkableType = WalkableType.Fluid,
                Sounds = noSound,
            });
            m.SetBlockType(11, "StationaryLava", new BlockType()
            {
                AllTextures = 30,
                DrawType = DrawType.Fluid,
                WalkableType = WalkableType.Fluid,
                Sounds = noSound,
            });
            m.SetBlockType(12, "Sand", new BlockType()
            {
                AllTextures = 18,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(13, "Gravel", new BlockType()
            {
                AllTextures = 19,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(14, "GoldOre", new BlockType()
            {
                AllTextures = 32,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(15, "IronOre", new BlockType()
            {
                AllTextures = 33,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(16, "CoalOre", new BlockType()
            {
                AllTextures = 34,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(17, "TreeTrunk", new BlockType()
            {
                TextureIdTop = 21,
                TextureIdBottom = 21,
                TextureIdBack = 20,
                TextureIdFront = 20,
                TextureIdLeft = 20,
                TextureIdRight = 20,
                TextureIdForInventory = 20,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(18, "Leaves", new BlockType()
            {
                AllTextures = 22,
                DrawType = DrawType.Transparent,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(19, "Sponge", new BlockType()
            {
                AllTextures = 48,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(20, "Glass", new BlockType()
            {
                AllTextures = 49,
                DrawType = DrawType.Transparent,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(21, "RedCloth", new BlockType()
            {
                AllTextures = 64,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(22, "OrangeCloth", new BlockType()
            {
                AllTextures = 65,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(23, "YellowCloth", new BlockType()
            {
                AllTextures = 66,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(24, "LightGreenCloth", new BlockType()
            {
                AllTextures = 67,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(25, "GreenCloth", new BlockType()
            {
                AllTextures = 68,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(26, "AquaGreenCloth", new BlockType()
            {
                AllTextures = 69,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(27, "CyanCloth", new BlockType()
            {
                AllTextures = 70,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(28, "BlueCloth", new BlockType()
            {
                AllTextures = 71,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(29, "PurpleCloth", new BlockType()
            {
                AllTextures = 72,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(30, "IndigoCloth", new BlockType()
            {
                AllTextures = 73,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(31, "VioletCloth", new BlockType()
            {
                AllTextures = 74,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(32, "MagnetaCloth", new BlockType()
            {
                AllTextures = 75,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(33, "PinkCloth", new BlockType()
            {
                AllTextures = 76,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(34, "BlackCloth", new BlockType()
            {
                AllTextures = 77,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(35, "GrayCloth", new BlockType()
            {
                AllTextures = 78,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(36, "WhiteCloth", new BlockType()
            {
                AllTextures = 79,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(37, "YellowFlowerDecorations", new BlockType()
            {
                AllTextures = 13,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(38, "RedRoseDecorations", new BlockType()
            {
                AllTextures = 12,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(39, "RedMushroom", new BlockType()
            {
                AllTextures = 28,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(40, "BrownMushroom", new BlockType()
            {
                AllTextures = 29,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(41, "GoldBlock", new BlockType()
            {
                AllTextures = 24,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(42, "IronBlock", new BlockType()
            {
                AllTextures = 23,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(43, "DoubleStair", new BlockType()
            {
                AllTextures = 6,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(44, "Stair", new BlockType()
            {
                AllTextures = 6,
                DrawType = DrawType.Transparent,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(45, "Brick", new BlockType()
            {
                AllTextures = 103,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(46, "TNT", new BlockType()
            {
                AllTextures = 9,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(47, "Bookcase", new BlockType()
            {
                TextureIdTop = 4,
                TextureIdBottom = 4,
                TextureIdBack = 35,
                TextureIdFront = 35,
                TextureIdLeft = 35,
                TextureIdRight = 35,
                TextureIdForInventory = 35,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(48, "MossyCobblestone", new BlockType()
            {
                AllTextures = 36,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(49, "Obsidian", new BlockType()
            {
                AllTextures = 37,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(50, "Torch", new BlockType()
            {
                TextureIdTop = 126,
                TextureIdBottom = 125,
                TextureIdBack = 125,
                TextureIdFront = 125,
                TextureIdLeft = 125,
                TextureIdRight = 125,
                TextureIdForInventory = 125,
                LightRadius = 15,
                DrawType = DrawType.Torch,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(51, "FireBlock", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(52, "InfiniteWaterSource", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(53, "InfiniteLavaSource", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(54, "Chest", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(55, "Gear", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(56, "DiamondPre", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(57, "DiamondBlock", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(58, "CraftingTable1", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(59, "Crops", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(60, "Soil", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(61, "Furnace", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(62, "BurningFurnace", new BlockType()
            {
                AllTextures = 0,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(100, "BrushedMetal", new BlockType()
            {
                AllTextures = 80,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(101, "ChemicalGreen", new BlockType()
            {
                AllTextures = 81,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(102, "Salt", new BlockType()
            {
                AllTextures = 82,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(103, "Roof", new BlockType()
            {
                AllTextures = 83,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(104, "Camouflage", new BlockType()
            {
                AllTextures = 84,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(105, "DirtForFarming", new BlockType()
            {
                AllTextures = 85,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(106, "Apples", new BlockType()
            {
                AllTextures = 86,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(107, "Hay", new BlockType()
            {
                AllTextures = 87,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(108, "Crops1", new BlockType()
            {
                AllTextures = 88,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(109, "Crops2", new BlockType()
            {
                AllTextures = 89,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(110, "Crops3", new BlockType()
            {
                AllTextures = 90,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(111, "Crops4", new BlockType()
            {
                AllTextures = 91,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(112, "CraftingTable", new BlockType()
            {
                AllTextures = 112,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(113, "Minecart", new BlockType()
            {
                AllTextures = 113,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(114, "Trampoline", new BlockType()
            {
                AllTextures = 121,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(115, "FillStart", new BlockType()
            {
                AllTextures = 122,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(116, "Cuboid", new BlockType()
            {
                AllTextures = 123,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(117, "FillArea", new BlockType()
            {
                AllTextures = 124,
                DrawType = DrawType.Fluid,
                WalkableType = WalkableType.Fluid,
                Sounds = solidSounds,
            });
            m.SetBlockType(118, "Water0", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(119, "Water1", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(120, "Water2", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(121, "Water3", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(122, "Water4", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(123, "Water5", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(124, "Water6", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(125, "Water7", new BlockType()
            {
                AllTextures = 127,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            for (int i = 0; i < 64; i++)
            {
                m.SetBlockType(176 + i, "Rail" + i.ToString(), new BlockType()
                {
                    TextureIdTop = 176 + i,
                    TextureIdBottom = 16,
                    TextureIdBack = 16,
                    TextureIdFront = 16,
                    TextureIdLeft = 16,
                    TextureIdRight = 16,
                    TextureIdForInventory = 176 + i,
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    Rail = i,
                });
            }
            m.SetBlockType(126, "DoorBottomClosed", new BlockType()
            {
                AllTextures = 130,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(127, "DoorTopClosed", new BlockType()
            {
                AllTextures = 131,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(128, "DoorBottomOpen", new BlockType()
            {
                AllTextures = 130,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(129, "DoorTopOpen", new BlockType()
            {
                AllTextures = 131,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(130, "GrassTrap", new BlockType()
            {
                TextureIdTop = 0, //"Grass",
                TextureIdBack = 3,//"GrassSide",
                TextureIdFront = 3, //"GrassSide",
                TextureIdLeft = 3,//"GrassSide",
                TextureIdRight = 3,//"GrassSide",
                TextureIdForInventory = 3,//"GrassSide",
                TextureIdBottom = 2, //"Dirt",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(131, "GoldCoin", new BlockType()
            {
                AllTextures = 134,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(132, "GoldBar", new BlockType()
            {
                AllTextures = 120,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(133, "SilverOre", new BlockType()
            {
                AllTextures = 50,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(135, "SilverCoin", new BlockType()
            {
                AllTextures = 132,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(140, "DirtBrick", new BlockType()
            {
                AllTextures = 7,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(141, "LuxuryRoof", new BlockType()
            {
                AllTextures = 53,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(142, "SandBrick", new BlockType()
            {
                AllTextures = 54,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(143, "FakeBookcase", new BlockType()
            {
                TextureIdTop = 4,
                TextureIdBottom = 4,
                TextureIdBack = 35,
                TextureIdFront = 35,
                TextureIdLeft = 35,
                TextureIdRight = 35,
                TextureIdForInventory=35,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(144, "WoodDesk", new BlockType()
            {
                TextureIdTop = 10,
                TextureIdBottom = 26,
                TextureIdBack = 11,
                TextureIdFront = 11,
                TextureIdLeft = 11,
                TextureIdRight = 11,
                TextureIdForInventory = 10,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(145, "GlassDesk", new BlockType()
            {
                TextureIdTop = 49,
                TextureIdBottom = 26,
                TextureIdBack = 11,
                TextureIdFront = 11,
                TextureIdLeft = 11,
                TextureIdRight = 11,
                TextureIdForInventory = 11,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(146, "Mosaik", new BlockType()
            {
                AllTextures = 96,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(147, "Asphalt", new BlockType()
            {
                AllTextures = 97,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(148, "Cake", new BlockType()
            {
                TextureIdTop = 144,
                TextureIdBottom = 146,
                TextureIdBack = 145,
                TextureIdFront = 145,
                TextureIdLeft = 145,
                TextureIdRight = 145,
                TextureIdForInventory=144,
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(149, "Fire", new BlockType()
            {
                AllTextures = 31,
                LightRadius = 15,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Empty,
                Sounds = solidSounds,
            });
            m.SetBlockType(150, "Fence", new BlockType()
            {
                AllTextures = 56,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(151, "Compass", new BlockType()
            {
                AllTextures = 147,
                DrawType = DrawType.Plant,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
            });
            m.SetBlockType(152, "Ladder", new BlockType()
            {
                AllTextures = 158,
                DrawType = DrawType.Transparent,
                WalkableType = WalkableType.Fluid,
                Sounds = solidSounds,
            });


            //todo seasons

            //Creative inventory
            m.AddToCreativeInventory("Stone");
            m.AddToCreativeInventory("Dirt");
            m.AddToCreativeInventory("Cobblestone");
            m.AddToCreativeInventory("Wood");
            m.AddToCreativeInventory("Sapling");
            m.AddToCreativeInventory("Adminium");
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
            m.AddToCreativeInventory("MagnetaCloth");
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
            m.AddToCreativeInventory("TNT");
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
            m.AddToCreativeInventory("DoorBottomClosed");
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
            m.AddCraftingRecipe("TNT", 1, "GoldBlock", 1);
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
            m.AddCraftingRecipe("MagnetaCloth", 1, "VioletCloth", 1);
            m.AddCraftingRecipe("PinkCloth", 1, "MagnetaCloth", 1);
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
            m.AddCraftingRecipe("DoorBottomClosed", 1, "Wood", 2);
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

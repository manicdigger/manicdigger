using System;

namespace ManicDigger.Mods
{
    public class DefaultWar : IMod
    {
        public void PreStart(ModManager m)
        {
        }
        public void Start(ModManager manager)
        {
            m = manager;
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
                Shoot = new string[] { },
                ShootEnd = new string[] { "M1GarandGun-SoundBible.com-1519788442", "M1GarandGun-SoundBible.com-15197884422" },
                Reload = new string[] { "shotgun-reload-old_school-RA_The_Sun_God-580332022" },
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
                    DamageToPlayer = 2,
                    LightRadius = 15,
                });
            m.SetBlockType(11, "StationaryLava", new BlockType()
                {
                    AllTextures = "Lava",
                    DrawType = DrawType.Fluid,
                    WalkableType = WalkableType.Fluid,
                    Sounds = noSound,
                    DamageToPlayer = 2,
                    LightRadius = 15,
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
                    LightRadius = 5,
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
            m.SetBlockType(58, "CraftingTable1", new BlockType()
                {
                    AllTextures = "Unknown",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                });
            m.SetBlockType(99, "Hurt", new BlockType()
                {
                    AllTextures = "Hurt",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                    DamageToPlayer = 20,
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
                    DrawType = DrawType.Plant,
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
            m.SetBlockType(154, "Pistol", new BlockType()
                {
                    AllTextures = "Pistol",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    handimage = "pistolhand.png",
                    IsPistol = true,
                    AimRadius = 15,
                    Recoil = 0.04f,
                    Delay = 0.5f,
                    WalkSpeedWhenUsed = 1f,
                    IronSightsEnabled = true,
                    IronSightsMoveSpeed = 1f,
                    IronSightsImage = "pistolhandsights.png",
                    IronSightsAimRadius = 15,
                    IronSightsFov = 0.8f,
                    AmmoMagazine = 12,
                    AmmoTotal = 120,
                    ReloadDelay = 2,
                    ExplosionRange = 0.2f,
                    ExplosionTime = 0.2f,
                    DamageBody = 15,
                    DamageHead = 50,
                });
            m.SetBlockType(155, "SubmachineGun", new BlockType()
                {
                    AllTextures = "SubmachineGun",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    handimage = "submachinegunhand.png",
                    IsPistol = true,
                    AimRadius = 20,
                    Recoil = 0.04f,
                    Delay = 0.1f,
                    WalkSpeedWhenUsed = 1f,
                    IronSightsEnabled = true,
                    IronSightsMoveSpeed = 1f,
                    IronSightsImage = "submachinegunhandsights.png",
                    IronSightsAimRadius = 20,
                    IronSightsFov = 0.8f,
                    AmmoMagazine = 30,
                    AmmoTotal = 120,
                    ReloadDelay = 2,
                    ExplosionRange = 0.2f,
                    ExplosionTime = 0.2f,
                    DamageBody = 15,
                    DamageHead = 40,
                });
            m.SetBlockType(156, "Shotgun", new BlockType()
                {
                    AllTextures = "Shotgun",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    handimage = "shotgunhand.png",
                    IsPistol = true,
                    AimRadius = 50,
                    Recoil = 0.08f,
                    Delay = 1f,
                    BulletsPerShot = 6,
                    WalkSpeedWhenUsed = 1f,
                    IronSightsEnabled = true,
                    IronSightsMoveSpeed = 1f,
                    IronSightsImage = "shotgunhandsights.png",
                    IronSightsAimRadius = 50,
                    IronSightsFov = 0.8f,
                    AmmoMagazine = 30,
                    AmmoTotal = 120,
                    ReloadDelay = 2,
                    ExplosionRange = 0.2f,
                    ExplosionTime = 0.2f,
                    DamageBody = 35,
                    DamageHead = 60,
                });
            m.SetBlockType(157, "Rifle", new BlockType()
                {
                    AllTextures = "Rifle",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = solidSounds,
                    handimage = "riflehand.png",
                    IsPistol = true,
                    AimRadius = 20,
                    Recoil = 0.04f,
                    Delay = 2f,
                    WalkSpeedWhenUsed = 1f,
                    IronSightsEnabled = true,
                    IronSightsMoveSpeed = 0.4f,
                    IronSightsImage = "riflehandsights.png",
                    IronSightsAimRadius = 10,
                    IronSightsFov = 0.5f,
                    AmmoMagazine = 6,
                    AmmoTotal = 48,
                    ReloadDelay = 2,
                    ExplosionRange = 0.2f,
                    ExplosionTime = 0.2f,
                    DamageBody = 35,
                    DamageHead = 100,
                });
            m.SetBlockType(158, "MedicalKit", new BlockType()
                {
                    AllTextures = "MedicalKit",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                    handimage = null,
                    IsPistol = false,
                    WalkSpeedWhenUsed = 1f,
                });
            m.SetBlockType(159, "AmmoPack", new BlockType()
                {
                    TextureIdTop = "AmmoTop",
                    TextureIdBack = "AmmoPack",
                    TextureIdFront = "AmmoPack",
                    TextureIdLeft = "AmmoPack",
                    TextureIdRight = "AmmoPack",
                    TextureIdForInventory = "AmmoPack",
                    TextureIdBottom = "AmmoTop",
                    DrawType = DrawType.Transparent,
                    WalkableType = WalkableType.Empty,
                    Sounds = solidSounds,
                    handimage = null,
                    IsPistol = false,
                    WalkSpeedWhenUsed = 1f,
                });
            SoundSet grenadesounds = new SoundSet();
            grenadesounds.Shoot = new string[] { "grenadestart" };
            grenadesounds.ShootEnd = new string[] {"grenadethrow" };
            grenadesounds.Reload = solidSounds.Reload;
            m.SetBlockType(160, "Grenade", new BlockType()
                {
                    AllTextures = "Grenade",
                    DrawType = DrawType.Solid,
                    WalkableType = WalkableType.Solid,
                    Sounds = grenadesounds,
                    handimage = "grenadehand.png",
                    IsPistol = true,
                    AimRadius = 20,
                    Recoil = 0.04f,
                    Delay = 0.5f,
                    WalkSpeedWhenUsed = 1f,
                    IronSightsEnabled = false,
                    IronSightsMoveSpeed = 0.4f,
                    IronSightsImage = "grenadehand.png",
                    IronSightsAimRadius = 10,
                    IronSightsFov = 0.5f,
                    AmmoMagazine = 6,
                    AmmoTotal = 6,
                    ReloadDelay = 2,
                    ExplosionRange = 10f,
                    ExplosionTime = 1f,
                    ProjectileSpeed = 25f,
                    ProjectileBounce = true,
                    DamageBody = 200,
                    PistolType = PistolType.Grenade,
                });
            
            m.RegisterTimer(UpdateSeasons, 1);
            
            m.SetGameDayRealHours(1);
            m.SetGameYearRealHours(24);
            
            m.SetSunLevels(sunLevels);
            m.SetLightLevels(lightLevels);
        }
        
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
                if (currentSeason == 0)
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
            15,//0 hour
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
            15,
            15,
            15,
            15,
            15,//6 hour
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
            15,
            15,
            15,
            15,
            15,
            15,//18 hour
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
            15,
            15,
            15,
            15,
            15,
            15,
        };
    }
}

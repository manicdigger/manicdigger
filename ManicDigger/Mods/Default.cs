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
                Walk = "walk1 walk2 walk3 walk4",
                Break = "destruct",
                Build = "build",
                Clone = "clone",
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
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
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
                AllTextures = "StationaryWater",
                DrawType = DrawType.Fluid,
                WalkableType = WalkableType.Fluid,
                Sounds = noSound,
            });
           
            //Creative inventory
            m.AddToCreativeInventory("Stone");
            m.AddToCreativeInventory("Dirt");
        }
    }
}

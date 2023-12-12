using System;
using System.Collections.Generic;

namespace ManicDigger.Mods
{
    public class Monsteregg : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("CoreBlocks");
        }
        public void Start(ModManager manager)
        {
            m = manager;
            SoundSet solidSounds = new SoundSet()
            {
                Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
                Break = new string[] { "destruct" },
                Build = new string[] { "build" },
                Clone = new string[] { "clone" },
            };
            m.SetBlockType(46, "Monsteregg", new BlockType()
            {
                TopBottomTextures = "GoldBarTopBottom",
                SideTextures = "GoldBarSide",
                TextureIdForInventory = "GoldBarInvetory",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                Sounds = solidSounds,
                LightRadius = 5,
                IsUsable = true
            });

            megg = m.GetBlockId("Monsteregg");
            m.AddToStartInventory("Monsteregg", 6);
            m.AddToCreativeInventory("Monsteregg");

            m.RegisterOnBlockUse(UseTnt);
    
        }
        ModManager m;
        int megg;



        private void UseTnt(int player, int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == megg)
            {
                m.SpawnMonster(x, y, z);
            }
        }

    
    }
}

using System;

namespace ManicDigger.Mods
{
    public class HealingCake : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("CoreBlocks");
        }
        public void Start(ModManager manager)
        {
        	m = manager;
        	
        	m.RegisterOnBlockUse(OnUse);
        	
            SoundSet solidSounds = new SoundSet()
            {
                Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
                Break = new string[] { "destruct" },
                Build = new string[] { "build" },
                Clone = new string[] { "clone" }
            };
            m.SetBlockType(148, "Cake", new BlockType()
			{
					TextureIdTop = "CakeTop",
					TextureIdBottom = "Gray",
					SideTextures = "CakeSide",
					TextureIdForInventory = "CakeTop",
					DrawType = DrawType.Solid,
					WalkableType = WalkableType.Solid,
					Sounds = solidSounds,
					IsUsable = true,
			});
            	
            m.AddToCreativeInventory("Cake");
            m.AddCraftingRecipe2("Cake", 1, "Salt", 2, "Crops4", 4);
            
            Cake = m.GetBlockId("Cake");
        }
        ModManager m;
        
        int Cake;
        
        void OnUse(int player, int x, int y, int z)
        {
        	if (m.GetBlock(x, y, z) == Cake)
            {
        		int health = m.GetPlayerHealth(player);
            	int maxhealth = m.GetPlayerMaxHealth(player);
            
            
            	health += 30;
            
            
            	if (health > maxhealth)
           		{
            		health = maxhealth;
            	}
            
            	m.SetPlayerHealth(player, health, maxhealth);
            	m.SetBlock(x, y, z, 0);
        	}
        }
    }
}
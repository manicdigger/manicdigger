namespace ManicDigger.Mods
{
    public class CoreTools : IMod
    {
        public void PreStart(ModManager m)
        {
  

        }
        private void AddTool(string name,BlockType type) {
            name = "default_tool_" + name.ToLower().Replace(" ",""); 
            type.AllTextures =name;
            type.DrawType = DrawType.Solid;
            type.WalkableType = WalkableType.Solid;
            m.SetBlockType(name, type);
            m.AddToStartInventory(name, 1);
            m.AddToCreativeInventory(name);
            
        }
        public void Start(ModManager manager)
        {
            m = manager;

            int Axe = m.GetToolType("Axe");
            int Shovel = m.GetToolType("Shovel");
           // int Hoe = m.GetToolType("Hoe");
            int Sword = m.GetToolType("Sword");
            int Pickaxe = m.GetToolType("Pickaxe");
            int Shears = m.GetToolType("Shears");
            
            float baseAxeStrenght=4f;
            float baseShovelStrenght=5f;
            float baseSwordStrenght=3f;
            float basePickaxeStrenght=3f;



            float diamondMod=4f;
            AddTool("Diamond Axe", new BlockType()
            {
                ToolStrenght = baseAxeStrenght * diamondMod,
                HarvestabilityMask = Axe
            });
            AddTool("Diamond Shovel", new BlockType()
            {
                ToolStrenght = baseShovelStrenght * diamondMod,
                HarvestabilityMask = Shovel
            }); 
            AddTool("Diamond Sword", new BlockType()
            {
                ToolStrenght = baseSwordStrenght * diamondMod,
                HarvestabilityMask = Sword
            });
            AddTool("Diamond Pickaxe", new BlockType()
            {
                ToolStrenght = basePickaxeStrenght * diamondMod,
                HarvestabilityMask = Pickaxe
            });

            float goldMod=5f;
            AddTool("Gold Axe", new BlockType()
            {
                ToolStrenght = baseAxeStrenght * goldMod,
                HarvestabilityMask = Axe
            });
            AddTool("Gold Shovel", new BlockType()
            {
                ToolStrenght = baseShovelStrenght * goldMod,
                HarvestabilityMask = Shovel
            }); 
            AddTool("Gold Sword", new BlockType()
            {
                ToolStrenght = baseSwordStrenght * goldMod,
                HarvestabilityMask = Sword
            });
            AddTool("Gold Pickaxe", new BlockType()
            {
                ToolStrenght = basePickaxeStrenght * goldMod,
                HarvestabilityMask = Pickaxe
            });

            float steelMod=3f;
            AddTool("Steel Axe", new BlockType()
            {
                ToolStrenght = baseAxeStrenght * steelMod,
                HarvestabilityMask = Axe
            });
            AddTool("Steel Shovel", new BlockType()
            {
                ToolStrenght = baseShovelStrenght * steelMod,
                HarvestabilityMask = Shovel
            }); 
            AddTool("Steel Sword", new BlockType()
            {
                ToolStrenght = baseSwordStrenght * steelMod,
                HarvestabilityMask = Sword
            });
            AddTool("Steel Pickaxe", new BlockType()
            {
                ToolStrenght = basePickaxeStrenght * steelMod,
                HarvestabilityMask = Pickaxe
            });

            float stoneMod=2f;
            AddTool("Stone Axe", new BlockType()
            {
                ToolStrenght = baseAxeStrenght * stoneMod,
                HarvestabilityMask = Axe
            });
            AddTool("Stone Shovel", new BlockType()
            {
                ToolStrenght = baseShovelStrenght * stoneMod,
                HarvestabilityMask = Shovel
            }); 
            AddTool("Stone Sword", new BlockType()
            {
                ToolStrenght = baseSwordStrenght * stoneMod,
                HarvestabilityMask = Sword
            });
            AddTool("Stone Pickaxe", new BlockType()
            {
                ToolStrenght = basePickaxeStrenght * stoneMod,
                HarvestabilityMask = Pickaxe
            });

            float woodMod=1f;
            AddTool("Wood Axe", new BlockType()
            {
                ToolStrenght = baseAxeStrenght * woodMod,
                HarvestabilityMask = Axe
            });
            AddTool("Wood Shovel", new BlockType()
            {
                ToolStrenght = baseShovelStrenght * woodMod,
                HarvestabilityMask = Shovel
            }); 
            AddTool("Wood Sword", new BlockType()
            {
                ToolStrenght = baseSwordStrenght * woodMod,
                HarvestabilityMask = Sword
            });
            AddTool("Wood Pickaxe", new BlockType()
            {
                ToolStrenght = basePickaxeStrenght * woodMod,
                HarvestabilityMask = Pickaxe
            });

            AddTool("Shears", new BlockType()
            {
                ToolStrenght = 5,
                HarvestabilityMask = Pickaxe
            });
        }
        ModManager m;
    
    }
}

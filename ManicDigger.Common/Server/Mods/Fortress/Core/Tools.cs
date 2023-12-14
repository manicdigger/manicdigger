namespace ManicDigger.Mods
{
    public class Tools : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("CoreBlocks");
        }
        public void Start(ModManager manager)
        {
            m = manager;

            m.SetBlockType( "Pickaxe", new BlockType()
            {

                AllTextures = "Pistol",
                handimage = "pistolhand.png",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                ToolStrenght = 5f,
                
            });


            m.AddToStartInventory("Pickaxe", 1);
        }
        ModManager m;
    
    }
}

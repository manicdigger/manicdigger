namespace ManicDigger.Mods
{
    public class CoreToolsDefinitions : IMod
    {
        public void PreStart(ModManager m)
        {
            //ILLEGAL TODO hack but Its safe mod dosent have anything i need to create
            //guide for myself what is and what isnt worth to puting to own mod
            m.AddTooltype("Axe");
            m.AddTooltype("Pickaxe");
            m.AddTooltype("Shovel");
            m.AddTooltype("Hoe");
            m.AddTooltype("Sword");
            m.AddTooltype("Shears");

        }

        public void Start(ModManager manager)
        {

        }
      
    
    }
}

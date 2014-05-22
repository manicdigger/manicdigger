using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Mods
{
    public class CraftingTable : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }
        public void Start(ModManager manager)
        {
            
        }
    }
}

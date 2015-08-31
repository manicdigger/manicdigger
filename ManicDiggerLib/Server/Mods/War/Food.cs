using System;

namespace ManicDigger.Mods.War
{
	public class Food : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			
			m.RegisterOnBlockUse(OnUse);
			
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

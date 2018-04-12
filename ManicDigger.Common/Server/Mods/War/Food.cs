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
			Apples = m.GetBlockId("Apples");
		}
		ModManager m;
		int Cake,
		 	Apples;
		void OnUse(int player, int x, int y, int z)
		{
			int[] food =
			{
				Cake,
				Apples
			};
			for (int i = 0; i < food.Length; i++)
			{
				if (m.GetBlock(x, y, z) == food[i])
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
}

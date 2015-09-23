using System;
using System.Collections.Generic;

namespace ManicDigger.Mods.War
{
	public class WaterSimple : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterTimer(Update, 1);
			m.RegisterOnBlockBuild(BlockBuild);
			m.RegisterOnBlockDelete(BlockDelete);
			Water = m.GetBlockId("Water");
			Sponge = m.GetBlockId("Sponge");
		}
		int Water;
		int Sponge;
		
		ModManager m;
		
		void BlockBuild(int player, int x, int y, int z)
		{
			BlockChange(player, x, y, z);
		}
		void BlockDelete(int player, int x, int y, int z, int oldblock)
		{
			BlockChange(player, x, y, z);
		}
		
		void Update()
		{
			object enablewater = m.GetGlobalDataNotSaved("enablewater");
			if (enablewater == null || (bool)enablewater == false)
			{
				return;
			}
			if ((DateTime.UtcNow - lastflood).TotalSeconds > 1)
			{
				lastflood = DateTime.UtcNow;
				var curtoflood = new List<Vector3i>(toflood.Keys);
				foreach (var v in curtoflood)
				{
					Flood(v);
					toflood.Remove(v);
				}
			}
		}
		
		int spongerange = 2;
		bool IsSpongeNear(int x, int y, int z)
		{
			for (int xx = x - spongerange; xx <= x + spongerange; xx++)
			{
				for (int yy = y - spongerange; yy <= y + spongerange; yy++)
				{
					for (int zz = z - spongerange; zz <= z + spongerange; zz++)
					{
						if (m.IsValidPos(xx, yy, zz) && m.GetBlock(xx, yy, zz) == Sponge)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		
		public void BlockChange(int player, int x, int y, int z)
		{
			object enablewater=m.GetGlobalDataNotSaved("enablewater");
			if (enablewater == null || (bool)enablewater == false)
			{
				return;
			}
			this.flooded = new Dictionary<Vector3i, Vector3i>();
			//sponge just built.
			if (m.IsValidPos(x, y, z) && m.GetBlock(x, y, z) == Sponge)
			{
				for (int xx = x - spongerange; xx <= x + spongerange; xx++)
				{
					for (int yy = y - spongerange; yy <= y + spongerange; yy++)
					{
						for (int zz = z - spongerange; zz <= z + spongerange; zz++)
						{
							if (m.IsValidPos(xx, yy, zz) && IsWater(m.GetBlock(xx, yy, zz)))
							{
								//tosetempty.Add(new Vector3i(xx, yy, zz));
								m.SetBlock(xx, yy, zz, 0);
							}
						}
					}
				}
			}
			//maybe sponge destroyed. todo faster test.
			for (int xx = x - spongerange; xx <= x + spongerange; xx++)
			{
				for (int yy = y - spongerange; yy <= y + spongerange; yy++)
				{
					for (int zz = z - spongerange; zz <= z + spongerange; zz++)
					{
						if (m.IsValidPos(xx, yy, zz) && m.GetBlock(xx, yy, zz) == 0)
						{
							BlockChangeFlood(xx, yy, zz);
						}
					}
				}
			}
			BlockChangeFlood(x, y, z);
			//var v = new Vector3i(x, y, z);
			//tosetwater.Sort((a, b) => Distance(v, a).CompareTo(Distance(v, b)));
		}
		
		float Distance(Vector3i a, Vector3i b)
		{
			float dx = a.X - b.X;
			float dy = a.Y - b.Y;
			float dz = a.Z - b.Z;
			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}
		
		private bool IsWater(int block)
		{
			return block == Water;
		}
		
		void BlockChangeFlood(int x, int y, int z)
		{
			//water here
			if (m.IsValidPos(x, y, z)
			    && IsWater(m.GetBlock(x, y, z)))
			{
				Flood(new Vector3i(x, y, z));
				return;
			}
			//water around
			foreach (var vv in BlocksAround(new Vector3i(x, y, z)))
			{
				if (m.IsValidPos((int)vv.X, (int)vv.Y, (int)vv.Z) &&
				    IsWater(m.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z)))
				{
					Flood(vv);
					return;
				}
			}
		}
		
		Dictionary<Vector3i, Vector3i> flooded = new Dictionary<Vector3i, Vector3i>();
		//public List<Vector3i> tosetwater = new List<Vector3i>();
		//public List<Vector3i> tosetempty = new List<Vector3i>();
		Dictionary<Vector3i, Vector3i> toflood = new Dictionary<Vector3i, Vector3i>();
		DateTime lastflood;
		
		private void Flood(Vector3i v)
		{
			if (!m.IsValidPos((int)v.X, (int)v.Y, (int)v.Z))
			{
				return;
			}
			if (flooded.ContainsKey(v))
			{
				return;
			}
			flooded.Add(v, v);
			foreach (Vector3i vv in BlocksAround(v))
			{
				if (!m.IsValidPos((int)vv.X, (int)vv.Y, (int)vv.Z))
				{
					continue;
				}
				var type = m.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z);
				if (type == 0 && (!IsSpongeNear((int)vv.X, (int)vv.Y, (int)vv.Z)))
				{
					//tosetwater.Add(vv);
					m.SetBlock(vv.X, vv.Y, vv.Z, Water);
					toflood[vv] = vv;
				}
			}
		}
		
		IEnumerable<Vector3i> BlocksAround(Vector3i pos)
		{
			yield return new Vector3i(pos.X - 1, pos.Y, pos.Z);
			yield return new Vector3i(pos.X + 1, pos.Y, pos.Z);
			yield return new Vector3i(pos.X, pos.Y - 1, pos.Z);
			yield return new Vector3i(pos.X, pos.Y + 1, pos.Z);
			yield return new Vector3i(pos.X, pos.Y, pos.Z - 1);
			//yield return new Vector3i(pos.X, pos.Y, pos.Z + 1); //water does not flow up.
		}
		
		public struct Vector3i
		{
			public Vector3i(int x, int y, int z)
			{
				this.X = x;
				this.Y = y;
				this.Z = z;
			}
			public int X;
			public int Y;
			public int Z;
		}
	}
}

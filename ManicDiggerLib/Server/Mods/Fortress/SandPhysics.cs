using System;

namespace ManicDigger.Mods
{
	public class SandPhysics : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterOnBlockBuild(Build);
			m.RegisterOnBlockDelete(Delete);
		}
		
		ModManager m;
		
		void Build(int player, int x, int y, int z)
		{
			Update(x, y, z);
		}
		
		void Delete(int player, int x, int y, int z, int blockid)
		{
			Update(x, y, z);
		}
		
		void Update(int x, int y, int z)
		{
			if (IsValidDualPos(x, y, z - 1) && (IsSlideDown(x, y, z, m.GetBlockId("Sand")) || IsSlideDown(x, y, z, m.GetBlockId("Gravel"))))
			{
				BlockMoveDown(x, y, z - 1, 0);
				Update(x, y, z - 1);
			}
			else if (IsValidDualPos(x, y, z) && (IsDestroyOfBase(x, y, z, m.GetBlockId("Sand")) || IsDestroyOfBase(x, y, z, m.GetBlockId("Gravel"))))
			{
				BlockMoveDown(x, y, z, GetDepth(x, y, z));
				Update(x, y, z + 1);
			}
		}
		
		int GetDepth(int x, int y, int z)
		{
			int startHeight = z;
			while (m.IsValidPos(x, y, z) && (IsSoftBlock(m.GetBlock(x, y, z))))
			{
				z--;
			}
			
			return (startHeight - z) - 1;
		}
		
		bool IsSoftBlock(int blockType)
		{
			if (blockType == 0)
				return true;
			else if (m.IsBlockFluid(blockType))
				return true;
			else
				return false;
		}
		
		bool IsSlideDown(int x, int y, int z, int blockType)
		{
			return (((IsSoftBlock(m.GetBlock(x, y, z - 1)))) && (m.GetBlock(x, y, z) == blockType));
		}
		
		void BlockMoveDown(int x, int y, int z, int depth)
		{
			m.SetBlock(x, y, z - depth, m.GetBlock(x, y, z + 1));
			m.SetBlock(x, y, z + 1, 0);
		}
		
		bool IsDestroyOfBase(int x, int y, int z, int blockType)
		{
			return (IsSoftBlock((m.GetBlock(x, y, z))) && (m.GetBlock(x, y, z + 1) == blockType));
		}
		
		bool IsValidDualPos(int x, int y, int z)
		{
			return m.IsValidPos(x, y, z) && m.IsValidPos(x, y, z + 1);
		}
	}
}

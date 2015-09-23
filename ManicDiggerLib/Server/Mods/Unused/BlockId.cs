using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace ManicDigger.Mods
{
	public class BlockId : IMod
	{
		private ModManager m;
		public static bool DEBUG = true;
		
		public void PreStart(ModManager m)
		{
			// Add all modfiles here which contain block definitions.
			m.RequireMod("CoreBlocks");
			m.RequireMod("Doors");
			m.RequireMod("Tnt");
			m.RequireMod("PermissionBlock");
			m.RequireMod("VandalFinder");
			
		}
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterOnLoad(OnLoad);
		}
		
		public void OnLoad()
		{
			if (DEBUG) Console.WriteLine("############# BlockID Mod #############");
			
			
			// Assigned blocks (in Default.cs and other mod files).
			Dictionary<int, Block> assignedBlocks = new Dictionary<int, Block>();
			if (DEBUG)
				Console.WriteLine("## Current assigned IDs:");
			
			for (int i = 0; i < m.GetMaxBlockTypes(); i++)
			{
				string s = m.GetBlockName(i);
				if (s != null)
				{
					assignedBlocks.Add(i, new Block {Name = s, Type = m.GetBlockType(i) });
					if (DEBUG)
						Console.WriteLine(i + ": " + s);
				}
				else
				{
					if (DEBUG)
						Console.WriteLine(i + ": not set");
				}
			}
			
			// Expected blocks (blocks which were used before with the savegame).
			Dictionary<int, string> expectedBlocks = LoadBlockIdsFromDatabase();
			if (DEBUG)
			{
				Console.WriteLine("## Expected IDs (from savegame):");
				if (expectedBlocks.Count == 0)
				{
					Console.WriteLine("No expected IDs!");
				}
				
				foreach (var b in expectedBlocks)
				{
					Console.WriteLine(b.Key + ": " + b.Value);
				}
			}
			
			// Reassign IDs.
			Dictionary<int, Block> reassignedBlocks = this.ReassignBlockIds(assignedBlocks, expectedBlocks);
			
			// Set blocktypes again to game with new reassigned IDs.
			for (int i = 0; i < m.GetMaxBlockTypes(); i++)
			{
				if (reassignedBlocks.ContainsKey(i))
				{
					m.SetBlockType(i, reassignedBlocks [i].Name, reassignedBlocks [i].Type);
				}
				else
				{
					m.SetBlockType(i, "", new BlockType() { });
				}
			}
			
			// Finally save new block order to savegame.
			Dictionary<int, string> newBlockIDs = new Dictionary<int, string>();
			if (DEBUG) Console.WriteLine("## New Block IDs (storing to savegame):");
			foreach (var k in reassignedBlocks)
			{
				newBlockIDs.Add(k.Key, k.Value.Name);
				if (DEBUG) Console.WriteLine(k.Key + ": " + k.Value.Name);
			}
			SaveBlockIdsToDatabase(newBlockIDs);
		}
		
		public Dictionary<int, string> LoadBlockIdsFromDatabase()
		{
			Dictionary<int, string> blocks = new Dictionary<int, string>();
			try
			{
				byte[] b = m.GetGlobalData("BlockIDs");
				if (b != null)
				{
					blocks = Serializer.Deserialize<Dictionary<int, string>>(new MemoryStream(b));
					if (DEBUG) Console.WriteLine("Block IDs loaded from savegame.");
				}
				else
				{
					// No BlockIDs saved in savegame.
					if (DEBUG) Console.WriteLine("Block IDs not found in savegame.");
				}
			}
			catch
			{
				throw new Exception("Error at loading blocks!");
			}
			return blocks;
		}
		
		public void SaveBlockIdsToDatabase(Dictionary<int, string> blocks)
		{
			if (blocks != null)
			{
				MemoryStream ms = new MemoryStream();
				Serializer.Serialize(ms, blocks);
				m.SetGlobalData("BlockIDs", ms.ToArray());
				if (DEBUG) Console.WriteLine("Block IDs have been written to savegame");
			}
			else
			{
				if (DEBUG) Console.WriteLine("Block IDs not set");
			}
		}
		
		public Dictionary<int, Block> ReassignBlockIds(Dictionary<int, Block> assignedBlocks, Dictionary<int, string> expectedBlocks)
		{
			Dictionary<int, Block> reassignedBlocks = new Dictionary<int, Block>();
			
			
			List<int> keysToRemove = new List<int>();
			
			// Assign block IDs which exist in assigned blocks and expected blocks (apply expected IDs).
			foreach (var k in assignedBlocks)
			{
				if (expectedBlocks.ContainsValue(k.Value.Name))
				{
					int index = 0;
					foreach (var kk in expectedBlocks)
					{
						if (kk.Value.Equals(k.Value.Name))
						{
							index = kk.Key;
							break;
						}
					}
					expectedBlocks.Remove(index);
					reassignedBlocks.Add(index, new Block { Name = k.Value.Name, Type = k.Value.Type });
					keysToRemove.Add(k.Key);
				}
			}
			// Remove already assigned blocks.
			foreach (int k in keysToRemove)
			{
				assignedBlocks.Remove(k);
			}
			// Add remaining expected blocks (which do not exist in assignedBlocks).
			BlockType unknownBlock = new BlockType()
			{
				AllTextures = "Unknown",
				DrawType = DrawType.Solid,
				WalkableType = WalkableType.Solid,
			};
			if (DEBUG) Console.WriteLine("# Missing block definitions which are expected:");
			foreach (var k in expectedBlocks)
			{
				if (DEBUG) Console.WriteLine(k.Key + ": " + k.Value);
				reassignedBlocks.Add(k.Key, new Block { Name = k.Value, Type = unknownBlock });
			}
			// Add remaining blocks from assignedBlocks.
			foreach (var k in assignedBlocks)
			{
				// generate index
				int i = 0;
				while (reassignedBlocks.ContainsKey(i))
				{
					i++;
				}
				reassignedBlocks.Add(i, new Block { Name = k.Value.Name, Type = k.Value.Type });
			}
			return reassignedBlocks;
		}
		
		public class Block
		{
			public string Name;
			public BlockType Type;
		}
	}
}

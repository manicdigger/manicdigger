using System;
using System.Collections.Generic;

namespace ManicDigger.Server
{
	/// <summary>
	/// Used for operations on different chunk storage variants.
	/// Does some checks when accessing data.
	/// </summary>
	public static class ChunkDb
	{
		public static byte[] GetChunk(IChunkDb db, int x, int y, int z)
		{
			List<byte[]> chunks = new List<byte[]>(db.GetChunks(new Xyz[] { new Xyz() {
					X = x,
					Y = y,
					Z = z
				}
			}));
			if (chunks.Count > 1)
			{
				throw new Exception();
			}
			if (chunks.Count == 0)
			{
				return null;
			}
			return chunks[0];
		}
		public static void SetChunk(IChunkDb db, int x, int y, int z, byte[] c)
		{
			db.SetChunks(new DbChunk[] { new DbChunk() {
					Position = new Xyz() {
						X = x,
						Y = y,
						Z = z
					},
					Chunk = c
				}
			});
		}
		public static void DeleteChunk(IChunkDb db, int x, int y, int z)
		{
			db.DeleteChunks(new Xyz[] { new Xyz() { X = x, Y = y, Z = z } });
		}
		public static void DeleteChunks(IChunkDb db, List<Xyz> chunks)
		{
			db.DeleteChunks(chunks.ToArray());
		}

		public static Dictionary<Xyz, byte[]> GetChunksFromFile(IChunkDb db, List<Xyz> chunksPositions, string filename)
		{
			return db.GetChunksFromFile(chunksPositions.ToArray(), filename);
		}
		public static byte[] GetChunkFromFile(IChunkDb db, int x, int y, int z, string filename)
		{
			Dictionary<Xyz, byte[]> chunks = db.GetChunksFromFile(new Xyz[] { new Xyz() {
					X = x,
					Y = y,
					Z = z
				}
			}, filename);
			if (chunks.Count > 1)
			{
				throw new Exception();
			}
			if (chunks.Count == 0)
			{
				return null;
			}
			return chunks[new Xyz() { X = x, Y = y, Z = z }];
		}
		/*
		public static void SetChunkToFile(IChunkDb db, int x, int y, int z, byte[] c, string filename)
		{
			db.SetChunksToFile(new DbChunk[] { new DbChunk() { Position = new Xyz() { X = x, Y = y, Z = z }, Chunk = c } }, filename);
		}
		*/
	}
}

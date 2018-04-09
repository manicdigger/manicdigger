using ManicDigger.Common;
using System.Collections.Generic;

namespace ManicDigger.Server
{
	/// <summary>
	/// Wrapper to compress data passed on to the actual chunk storage
	/// </summary>
	public class ChunkDbCompressed : IChunkDb
	{
		public IChunkDb d_ChunkDb;
		public ICompression d_Compression;
		#region IChunkDb Members
		public void Open(string filename)
		{
			d_ChunkDb.Open(filename);
		}
		public void Backup(string backupFilename)
		{
			d_ChunkDb.Backup(backupFilename);
		}
		public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions)
		{
			foreach (byte[] b in d_ChunkDb.GetChunks(chunkpositions))
			{
				if (b == null)
				{
					yield return null;
				}
				else
				{
					yield return d_Compression.Decompress(b);
				}
			}
		}

		public Dictionary<Xyz, byte[]> GetChunksFromFile(IEnumerable<Xyz> chunkpositions, string filename)
		{
			Dictionary<Xyz, byte[]> decompressedChunks = new Dictionary<Xyz, byte[]>();
			foreach (var k in d_ChunkDb.GetChunksFromFile(chunkpositions, filename))
			{
				byte[] c;
				if (k.Value == null)
				{
					c = null;
				}
				else
				{
					c = d_Compression.Decompress(k.Value);
				}
				decompressedChunks.Add(k.Key, c);
			}
			return decompressedChunks;
		}

		public void SetChunksToFile(IEnumerable<DbChunk> chunks, string filename)
		{
			List<DbChunk> compressed = new List<DbChunk>();
			foreach (DbChunk c in chunks)
			{
				byte[] b;
				if (c.Chunk == null)
				{
					b = null;
				}
				else
				{
					b = d_Compression.Compress(c.Chunk);
				}
				compressed.Add(new DbChunk() { Position = c.Position, Chunk = b });
			}
			d_ChunkDb.SetChunksToFile(compressed, filename);
		}

		public void DeleteChunks(IEnumerable<Xyz> chunkpositions)
		{
			d_ChunkDb.DeleteChunks(chunkpositions);
		}
		public void SetChunks(IEnumerable<DbChunk> chunks)
		{
			List<DbChunk> compressed = new List<DbChunk>();
			foreach (DbChunk c in chunks)
			{
				byte[] b;
				if (c.Chunk == null)
				{
					b = null;
				}
				else
				{
					b = d_Compression.Compress(c.Chunk);
				}
				compressed.Add(new DbChunk() { Position = c.Position, Chunk = b });
			}
			d_ChunkDb.SetChunks(compressed);
		}
		public byte[] GetGlobalData()
		{
			byte[] globaldata = d_ChunkDb.GetGlobalData();
			if (globaldata == null)
			{
				return null;
			}
			else
			{
				return d_Compression.Decompress(globaldata);
			}
		}
		public void SetGlobalData(byte[] data)
		{
			if (data == null)
			{
				d_ChunkDb.SetGlobalData(null);
			}
			else
			{
				d_ChunkDb.SetGlobalData(d_Compression.Compress(data));
			}
		}

		#endregion

		public bool GetReadOnly()
		{
			return d_ChunkDb.GetReadOnly();
		}
		public void SetReadOnly(bool value)
		{
			d_ChunkDb.SetReadOnly(value);
		}
	}
}

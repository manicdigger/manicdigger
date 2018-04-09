using System.Collections.Generic;

namespace ManicDigger.Server
{
	/// <summary>
	/// Dummy chunk storage
	/// </summary>
	public class ChunkDbDummy : IChunkDb
	{
		string currentFilename = null;
		Dictionary<Xyz, byte[]> chunks = new Dictionary<Xyz, byte[]>();
		public void Open(string filename)
		{
			if (filename != currentFilename)
			{
				currentFilename = filename;
				chunks.Clear();
			}
		}
		public void Backup(string backupFilename)
		{
			// TODO: what to do here?
		}
		public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions)
		{
			foreach (Xyz pos in chunkpositions)
			{
				byte[] c;
				if (chunks.TryGetValue(pos, out c))
				{
					yield return c;
				}
				else
				{
					yield return null;
				}
			}
		}
		public void DeleteChunks(IEnumerable<Xyz> chunkpositions)
		{
			foreach (Xyz pos in chunkpositions)
			{
				chunks[pos] = null;
			}
		}
		public void SetChunks(IEnumerable<DbChunk> chunks)
		{
			foreach (DbChunk c in chunks)
			{
				this.chunks[c.Position] = c.Chunk;
			}
		}
		public Dictionary<Xyz, byte[]> GetChunksFromFile(IEnumerable<Xyz> chunkpositions, string filename)
		{
			// TODO: what to do here?
			return null;
		}
		public void SetChunksToFile(IEnumerable<DbChunk> chunks, string filename)
		{
			// TODO: what to do here?
		}
		byte[] globaldata = null;
		public byte[] GetGlobalData()
		{
			return globaldata;
		}
		public void SetGlobalData(byte[] data)
		{
			globaldata = data;
		}
		bool ReadOnly;
		public bool GetReadOnly()
		{
			return ReadOnly;
		}
		public void SetReadOnly(bool value)
		{
			ReadOnly = value;
		}
	}
}

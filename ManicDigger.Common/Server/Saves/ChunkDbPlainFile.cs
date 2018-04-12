namespace ManicDigger.Server
{
	/// <summary>
	/// Chunk storage using a plain file structure to store data
	/// TODO: This is just a draft. Implement.
	/// </summary>
	/*public class ChunkDbPlainFile : IChunkDb
	{
		private string currentlyOpened;

		public void Open(string filename);
		public void Backup(string backupFilename);
		public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions);
		public void SetChunks(IEnumerable<DbChunk> chunks);
		public void DeleteChunks(IEnumerable<Xyz> chunkpositions);
		public byte[] GetGlobalData();
		public void SetGlobalData(byte[] data);
		public Dictionary<Xyz, byte[]> GetChunksFromFile(IEnumerable<Xyz> chunkpositions, string filename);
		public void SetChunksToFile(IEnumerable<DbChunk> chunks, string filename);
		public bool GetReadOnly();
		public void SetReadOnly(bool value);
	}*/
}

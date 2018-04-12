using System.Collections.Generic;

namespace ManicDigger.Server
{
	/// <summary>
	/// Interface implemented by all variants of chunk storage.
	/// </summary>
	public interface IChunkDb
	{
		void Open(string filename);
		void Backup(string backupFilename);
		IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions);
		void SetChunks(IEnumerable<DbChunk> chunks);
		void DeleteChunks(IEnumerable<Xyz> chunkpositions);
		byte[] GetGlobalData();
		void SetGlobalData(byte[] data);
		Dictionary<Xyz, byte[]> GetChunksFromFile(IEnumerable<Xyz> chunkpositions, string filename);
		void SetChunksToFile(IEnumerable<DbChunk> chunks, string filename);
		bool GetReadOnly();
		void SetReadOnly(bool value);
	}
}

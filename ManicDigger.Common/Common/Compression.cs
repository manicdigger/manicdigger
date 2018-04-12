using System.IO;
using System.IO.Compression;

namespace ManicDigger.Common
{
	/// <summary>
	/// Generic compression interface for bytestreams
	/// </summary>
	public interface ICompression
	{
		byte[] Compress(byte[] data);
		byte[] Decompress(byte[] data);
	}

	/// <summary>
	/// Provides GZip compression/decompression for bytestreams
	/// </summary>
	public class CompressionGzip : ICompression
	{
		public byte[] Compress(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();
			using (GZipStream compress = new GZipStream(output, CompressionMode.Compress))
			{
				byte[] buffer = new byte[4096];
				int numRead;
				while ((numRead = input.Read(buffer, 0, buffer.Length)) != 0)
				{
					compress.Write(buffer, 0, numRead);
				}
			}
			return output.ToArray();
		}
		public byte[] Decompress(byte[] fi)
		{
			MemoryStream ms = new MemoryStream();
			// Get the stream of the source file.
			using (MemoryStream inFile = new MemoryStream(fi))
			{
				using (GZipStream Decompress = new GZipStream(inFile,
															  CompressionMode.Decompress))
				{
					//Copy the decompression stream into the output file.
					byte[] buffer = new byte[4096];
					int numRead;
					while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
					{
						ms.Write(buffer, 0, numRead);
					}
				}
			}
			return ms.ToArray();
		}
		public static byte[] Decompress(FileInfo fi)
		{
			MemoryStream ms = new MemoryStream();
			// Get the stream of the source file.
			using (FileStream inFile = fi.OpenRead())
			{
				// Get original file extension, for example "doc" from report.doc.gz.
				string curFile = fi.FullName;
				string origName = curFile.Remove(curFile.Length - fi.Extension.Length);

				//Create the decompressed file.
				//using (FileStream outFile = File.Create(origName))
				{
					using (GZipStream Decompress = new GZipStream(inFile,
																  CompressionMode.Decompress))
					{
						//Copy the decompression stream into the output file.
						byte[] buffer = new byte[4096];
						int numRead;
						while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
						{
							ms.Write(buffer, 0, numRead);
						}
						//Console.WriteLine("Decompressed: {0}", fi.Name);
					}
				}
			}
			return ms.ToArray();
		}
	}
}

public class InfiniteMapChunked2d
{
	internal Game d_Map;
	public const int chunksize = 16;
	internal int[][] chunks;
	public int GetBlock(int x, int y)
	{
		int[] chunk = GetChunk(x, y);
		return chunk[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)];
	}
	public int[] GetChunk(int x, int y)
	{
		int[] chunk = null;
		int kx = x / chunksize;
		int ky = y / chunksize;
		if (chunks[MapUtilCi.Index2d(kx, ky, d_Map.map.MapSizeX / chunksize)] == null)
		{
			chunk = new int[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
			for (int i = 0; i < chunksize * chunksize; i++)
			{
				chunk[i] = 0;
			}
			chunks[MapUtilCi.Index2d(kx, ky, d_Map.map.MapSizeX / chunksize)] = chunk;
		}
		chunk = chunks[MapUtilCi.Index2d(kx, ky, d_Map.map.MapSizeX / chunksize)];
		return chunk;
	}
	public void SetBlock(int x, int y, int blocktype)
	{
		GetChunk(x, y)[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)] = blocktype;
	}
	public void Restart()
	{
		//chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
		int n = (d_Map.map.MapSizeX / chunksize) * (d_Map.map.MapSizeY / chunksize);
		chunks = new int[n][];//(byte**)Marshal.AllocHGlobal(n * sizeof(IntPtr));
		for (int i = 0; i < n; i++)
		{
			chunks[i] = null;
		}
	}
	public void ClearChunk(int x, int y)
	{
		int px = x / chunksize;
		int py = y / chunksize;
		chunks[MapUtilCi.Index2d(px, py, d_Map.map.MapSizeX / chunksize)] = null;
	}
}

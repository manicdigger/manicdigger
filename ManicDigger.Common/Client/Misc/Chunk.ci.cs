public class Chunk
{
	public Chunk()
	{
		baseLightDirty = true;
	}

	internal byte[] data;
	internal int[] dataInt;
	internal byte[] baseLight;
	internal bool baseLightDirty;
	internal RenderedChunk rendered;

	public int GetBlockInChunk(int pos)
	{
		if (dataInt != null)
		{
			return dataInt[pos];
		}
		else
		{
			return data[pos];
		}
	}

	public void SetBlockInChunk(int pos, int block)
	{
		if (dataInt == null)
		{
			if (block < 255)
			{
				data[pos] = ConvertCi.IntToByte(block);
			}
			else
			{
				int n = Game.chunksize * Game.chunksize * Game.chunksize;
				dataInt = new int[n];
				for (int i = 0; i < n; i++)
				{
					dataInt[i] = data[i];
				}
				data = null;

				dataInt[pos] = block;
			}
		}
		else
		{
			dataInt[pos] = block;
		}
	}

	public bool ChunkHasData()
	{
		return data != null || dataInt != null;
	}
}

public class RenderedChunk
{
	public RenderedChunk()
	{
		dirty = true;
	}
	internal int[] ids;
	internal int idsCount;
	internal bool dirty;
	internal byte[] light;
}

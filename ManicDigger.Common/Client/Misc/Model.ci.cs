public class Model
{
}

public class ModelData
{
	internal int verticesCount;
	public int GetVerticesCount() { return verticesCount; }
	public void SetVerticesCount(int value) { verticesCount = value; }
	internal int indicesCount;
	public int GetIndicesCount() { return indicesCount; }
	public void SetIndicesCount(int value) { indicesCount = value; }
	internal float[] xyz;
	public int GetXyzCount() { return verticesCount * 3; }
	internal byte[] rgba;
	public int GetRgbaCount() { return verticesCount * 4; }
	internal float[] uv;
	public int GetUvCount() { return verticesCount * 2; }
	internal int[] indices;
	internal int mode;

	public float[] getXyz() { return xyz; }
	public void setXyz(float[] p) { xyz = p; }
	public byte[] getRgba() { return rgba; }
	public void setRgba(byte[] p) { rgba = p; }
	public float[] getUv() { return uv; }
	public void setUv(float[] p) { uv = p; }
	public int[] getIndices() { return indices; }
	public void setIndices(int[] p) { indices = p; }
	public int getMode() { return mode; }
	public void setMode(int p) { mode = p; }

	internal int verticesMax;
	internal int indicesMax;
}

public class ModelDataTool
{
	public static void AddVertex(ModelData model, float x, float y, float z, float u, float v, int color)
	{
		if (model.verticesCount >= model.verticesMax)
		{
			int xyzCount = model.GetXyzCount();
			float[] xyz = new float[xyzCount * 2];
			for (int i = 0; i < xyzCount; i++)
			{
				xyz[i] = model.xyz[i];
			}

			int uvCount = model.GetUvCount();
			float[] uv = new float[uvCount * 2];
			for (int i = 0; i < uvCount; i++)
			{
				uv[i] = model.uv[i];
			}

			int rgbaCount = model.GetRgbaCount();
			byte[] rgba = new byte[rgbaCount * 2];
			for (int i = 0; i < rgbaCount; i++)
			{
				rgba[i] = model.rgba[i];
			}

			model.xyz = xyz;
			model.uv = uv;
			model.rgba = rgba;
			model.verticesMax = model.verticesMax * 2;
		}
		model.xyz[model.GetXyzCount() + 0] = x;
		model.xyz[model.GetXyzCount() + 1] = y;
		model.xyz[model.GetXyzCount() + 2] = z;
		model.uv[model.GetUvCount() + 0] = u;
		model.uv[model.GetUvCount() + 1] = v;
		model.rgba[model.GetRgbaCount() + 0] = ConvertCi.IntToByte(ColorCi.ExtractR(color));
		model.rgba[model.GetRgbaCount() + 1] = ConvertCi.IntToByte(ColorCi.ExtractG(color));
		model.rgba[model.GetRgbaCount() + 2] = ConvertCi.IntToByte(ColorCi.ExtractB(color));
		model.rgba[model.GetRgbaCount() + 3] = ConvertCi.IntToByte(ColorCi.ExtractA(color));
		model.verticesCount++;
	}

	internal static void AddIndex(ModelData model, int index)
	{
		if (model.indicesCount >= model.indicesMax)
		{
			int indicesCount = model.indicesCount;
			int[] indices = new int[indicesCount * 2];
			for (int i = 0; i < indicesCount; i++)
			{
				indices[i] = model.indices[i];
			}
			model.indices = indices;
			model.indicesMax = model.indicesMax * 2;
		}
		model.indices[model.indicesCount++] = index;
	}
}

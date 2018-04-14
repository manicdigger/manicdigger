public class ColorCi
{
	public static int FromArgb(int a, int r, int g, int b)
	{
		int iCol = (a << 24) | (r << 16) | (g << 8) | b;
		return iCol;
	}

	public static int ExtractA(int color)
	{
		byte a = ConvertCi.IntToByte(color >> 24);
		return a;
	}

	public static int ExtractR(int color)
	{
		byte r = ConvertCi.IntToByte(color >> 16);
		return r;
	}

	public static int ExtractG(int color)
	{
		byte g = ConvertCi.IntToByte(color >> 8);
		return g;
	}

	public static int ExtractB(int color)
	{
		byte b = ConvertCi.IntToByte(color);
		return b;
	}
}

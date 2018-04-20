public class ConvertCi
{
	public static byte IntToByte(int a)
	{
#if CITO
        return a.LowByte;
#else
		return (byte)a;
#endif
	}
	public static bool StringToBool(string str)
	{
		if (str == null)
		{
			return false;
		}
		else
		{
			return (str != "0"
				&& (str != "false")
				&& (str != "False")
				&& (str != "FALSE"));
		}
	}
	public static byte[] UshortArrayToByteArray(int[] input, int inputLength)
	{
		int outputLength = inputLength * 2;
		byte[] output = new byte[outputLength];
		for (int i = 0; i < inputLength; i++)
		{
			output[i * 2] = ConvertCi.IntToByte(input[i] & 255);
			output[i * 2 + 1] = ConvertCi.IntToByte((input[i] >> 8) & 255);
		}
		return output;
	}
	public static TextAlign IntToTextAlign(int align)
	{
		switch (align)
		{
			case 0:
				return TextAlign.Left;
			case 1:
				return TextAlign.Center;
			case 2:
				return TextAlign.Right;
			default:
				return TextAlign.Left;
		}
	}
	public static TextBaseline IntToTextBaseline(int baseline)
	{
		switch (baseline)
		{
			case 0:
				return TextBaseline.Top;
			case 1:
				return TextBaseline.Middle;
			case 2:
				return TextBaseline.Bottom;
			default:
				return TextBaseline.Top;
		}
	}
}

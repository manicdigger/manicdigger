class ConvertCi
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
}

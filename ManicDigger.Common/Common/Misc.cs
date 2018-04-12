using System;

namespace ManicDigger.Common
{
	/// <summary>
	/// TODO: Rename or remove
	/// </summary>
	public static class Misc
	{
		public static bool ReadBool(string str)
		{
			if (str == null)
			{
				return false;
			}
			else
			{
				return (str != "0" && (!str.Equals(bool.FalseString, StringComparison.InvariantCultureIgnoreCase)));
			}
		}
		public static unsafe byte[] UshortArrayToByteArray(ushort[] a)
		{
			byte[] output = new byte[a.Length * 2];
			fixed (ushort* a1 = a)
			{
				byte* a2 = (byte*)a1;
				for (int i = 0; i < a.Length * 2; i++)
				{
					output[i] = a2[i];
				}
			}
			return output;
		}
	}
}

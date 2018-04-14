public class MathCi
{
	public static float MinFloat(float a, float b)
	{
		if (a <= b)
		{
			return a;
		}
		else
		{
			return b;
		}
	}

	public static float MaxFloat(float a, float b)
	{
		if (a >= b)
		{
			return a;
		}
		else
		{
			return b;
		}
	}

	public static float AbsFloat(float b)
	{
		if (b >= 0)
		{
			return b;
		}
		else
		{
			return 0 - b;
		}
	}

	public static int Sign(float q)
	{
		if (q < 0)
		{
			return -1;
		}
		else if (q == 0)
		{
			return 0;
		}
		else
		{
			return 1;
		}
	}

	public static int MaxInt(int a, int b)
	{
		if (a >= b)
		{
			return a;
		}
		else
		{
			return b;
		}
	}

	public static int MinInt(int a, int b)
	{
		if (a <= b)
		{
			return a;
		}
		else
		{
			return b;
		}
	}

	public static float ClampFloat(float value, float min, float max)
	{
		float result = value;
		if (value > max)
		{
			result = max;
		}
		if (value < min)
		{
			result = min;
		}
		return result;
	}

	public static int ClampInt(int value, int min, int max)
	{
		int result = value;
		if (value > max)
		{
			result = max;
		}
		if (value < min)
		{
			result = min;
		}
		return result;
	}
}

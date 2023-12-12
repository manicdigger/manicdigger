namespace ManicDigger.Mods
{
	/// <summary>
	/// This class contains core settings for the Manic Digger server
	/// </summary>
	public class Core : IMod
	{
		public void PreStart(ModManager m) { }
		public void Start(ModManager m)
		{
			//Render hint to send to clients
			m.RenderHint(RenderHint.Fast);

			//Different serverside view distance if singleplayer
			if (m.IsSinglePlayer())
			{
				m.SetPlayerAreaSize(512);
			}
			else
			{
				m.SetPlayerAreaSize(256);
			}

			//Set up server time
			m.SetGameDayRealHours(1);
			m.SetDaysPerYear(24);

			//Set up day/night cycle
			m.SetSunLevels(sunLevels);
			m.SetLightLevels(lightLevels);
		}

		float[] lightLevels = new float[]
		{
			0.0351843721f,
			0.0439804651f,
			0.0549755814f,
			0.0687194767f,
			0.0858993459f,
			0.1073741824f,
			0.134217728f,
			0.16777216f,
			0.2097152f,
			0.262144f,
			0.32768f,
			0.4096f,
			0.512f,
			0.64f,
			0.8f,
			1f,
		};

		int[] sunLevels = new int[]
		{
			04,//00:00
			04,
			04,
			04,
			04,//01:00
			04,
			04,
			04,
			04,//02:00
			04,
			04,
			04,
			04,//03:00
			04,
			04,
			04,
			04,//04:00
			04,
			04,
			04,
			04,//05:00
			05,
			06,
			07,
			08,//06:00
			09,
			10,
			11,
			12,//07:00
			13,
			14,
			15,
			15,//08:00
			15,
			15,
			15,
			15,//09:00
			15,
			15,
			15,
			15,//10:00
			15,
			15,
			15,
			15,//11:00
			15,
			15,
			15,
			15,//12:00
			15,
			15,
			15,
			15,//13:00
			15,
			15,
			15,
			15,//14:00
			15,
			15,
			15,
			15,//15:00
			15,
			15,
			15,
			15,//16:00
			15,
			15,
			15,
			15,//17:00
			15,
			15,
			15,
			15,//18:00
			15,
			15,
			15,
			15,//19:00
			15,
			15,
			15,
			14,//20:00
			13,
			12,
			11,
			10,//21:00
			09,
			08,
			07,
			06,//22:00
			05,
			04,
			04,
			04,//23:00
			04,
			04,
			04,
		};
	}
}

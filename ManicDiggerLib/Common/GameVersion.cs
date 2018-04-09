using System.IO;

namespace ManicDigger.Common
{
	/// <summary>
	/// Extracts the game version from "version.txt" in the working directory
	/// </summary>
	public static class GameVersion
	{
		static string gameversion;
		public static string Version
		{
			get
			{
				if (gameversion == null)
				{
					gameversion = "unknown";
					if (File.Exists("version.txt"))
					{
						gameversion = File.ReadAllText("version.txt").Trim();
					}
				}
				return gameversion;
			}
		}
	}
}

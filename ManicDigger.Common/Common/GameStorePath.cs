using System;
using System.IO;
using System.Windows.Forms;

namespace ManicDigger.Common
{
    /// <summary>
    /// Description of GameStorePath.
    /// </summary>
    public static class GameStorePath
    {
        public static bool IsMono = Type.GetType("Mono.Runtime") != null;
        private static string  GameName="ERRORNAME";
        public static void SetGameName(string name) {
            Console.WriteLine("Game name CHANGEDDDDDDDDDDDDDDDDD");
            GameName = name;
        }
        public static string GetStorePath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Games/MCIC");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string apppath = Path.GetDirectoryName(Application.ExecutablePath);
            try
            {
                var di = new DirectoryInfo(apppath);
                if (di.Name.Equals("AutoUpdaterTemp", StringComparison.InvariantCultureIgnoreCase))
                {
                    apppath = di.Parent.FullName;
                }
            }
            catch
            {
            }
            string mdfolder = "UserData";
            if (apppath.Contains(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) && !IsMono)
            {
                string mdpath = Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                    mdfolder);
                return mdpath;
            }
            else
            {
                return Path.Combine(apppath, mdfolder);
            }
        }
        public static string gamepathsaves = Path.Combine(GameStorePath.GetStorePath(), "Saves");

        public static string gamepathgameconfig { get { return Path.Combine(gamepathsaves, string.Concat(GameName, "_config")); } } 
        public static string gamepathconfig = Path.Combine(GameStorePath.GetStorePath(), "Configuration");
        public static string gamepathbackup = Path.Combine(GameStorePath.GetStorePath(), "Backup");

        public static bool IsValidName(string s)
        {
            if (s.Length < 1 || s.Length > 32)
            {
                return false;
            }
            for (int i = 0; i < s.Length; i++)
            {
                if (!AllowedNameChars.Contains(s[i].ToString()))
                {
                    return false;
                }
            }
            return true;
        }
        public static string AllowedNameChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_-";
    }
}

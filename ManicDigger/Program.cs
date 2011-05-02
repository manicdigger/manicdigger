using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace ManicDigger
{
    public class InjectAttribute : Attribute
    {
    }
    public interface IGetFilePath
    {
        string GetFile(string p);
    }
    public class GetFilePathDummy : IGetFilePath
    {
        #region IGetFilePath Members
        public string GetFile(string p)
        {
            return p;
        }
        #endregion
    }
    public class GetFilePath : IGetFilePath
    {
        public GetFilePath(IEnumerable<string> datapaths)
        {
            this.DataPaths = new List<string>(datapaths).ToArray();
        }
        public string[] DataPaths;
        Dictionary<string, string> cache = new Dictionary<string, string>();
        public string GetFile(string filename)
        {
            if (!cache.ContainsKey(filename))
            {
				foreach (string path in DataPaths)
				{
					try
					{
						foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
						{
							try
							{
								FileInfo f = new FileInfo(s);
								cache[f.Name] = s;
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
            }
            for (int i = 0; i < 2; i++)
            {
                if (cache.ContainsKey(filename)) { return cache[filename]; }

                string f1 = filename.Replace(".png", ".jpg");
                if (cache.ContainsKey(f1)) { return cache[f1]; }

                string f2 = filename.Replace(".jpg", ".png");
                if (cache.ContainsKey(f2)) { return cache[f2]; }
                filename = new FileInfo(filename).Name; //handles GetFile(GetFile(file)) use.
            }

            throw new FileNotFoundException(filename);
        }
    }
    public class CrashReporter
    {
        public void Start(System.Threading.ThreadStart start)
        {
            if (!Debugger.IsAttached)
            {
                try
                {
                    start();
                }
                catch (Exception e)
                {
                    Crash(e);
                }
            }
            else
            {
                start();
            }
        }
        public static string gamepathcrash = GameStorePath.GetStorePath();
        public static void Crash(Exception e)
        {
            string crashfile = Path.Combine(gamepathcrash, "ManicDiggerCrash.txt");
            File.WriteAllText(crashfile, e.ToString());
            for (int i = 0; i < 5; i++)
            {
                System.Windows.Forms.Cursor.Show();
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }
            System.Windows.Forms.MessageBox.Show(e.ToString());
            Environment.Exit(1);
        }
    }
}
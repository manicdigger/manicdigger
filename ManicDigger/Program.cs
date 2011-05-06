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
    public interface IGetFileStream
    {
        Stream GetFile(string p);
    }
    public class GetFileStreamDummy : IGetFileStream
    {
        #region IGetFilePath Members
        public Stream GetFile(string p)
        {
			throw new FileNotFoundException();
        }
        #endregion
    }
    public class GetFileStream : IGetFileStream
    {
        public GetFileStream(IEnumerable<string> datapaths)
        {
            this.DataPaths = new List<string>(datapaths).ToArray();
        }
        public string[] DataPaths;
        Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();
        public Stream GetFile(string filename)
        {
            if (!cache.ContainsKey(filename))
            {
				foreach (string path in DataPaths)
				{
					try
					{
                        if (!Directory.Exists(path))
                        {
                            continue;
                        }
						foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
						{
							try
							{
								FileInfo f = new FileInfo(s);
								cache[f.Name] = File.ReadAllBytes(s);
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
                if (cache.ContainsKey(filename)) { return new MemoryStream(cache[filename]); }

                string f1 = filename.Replace(".png", ".jpg");
                if (cache.ContainsKey(f1)) { return new MemoryStream(cache[f1]); }

                string f2 = filename.Replace(".jpg", ".png");
                if (cache.ContainsKey(f2)) { return new MemoryStream(cache[f2]); }

                string f3 = filename.Replace(".wav", ".ogg");
                if (cache.ContainsKey(f3)) { return new MemoryStream(cache[f3]); }

                string f4 = filename.Replace(".ogg", ".wav");
                if (cache.ContainsKey(f4)) { return new MemoryStream(cache[f4]); }

                filename = new FileInfo(filename).Name; //handles GetFile(GetFile(file)) use.
            }

            throw new FileNotFoundException(filename);
        }
		public void SetFile(string name, byte[] data)
		{
			cache[name] = data;
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
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
        Dictionary<string, string> remap = new Dictionary<string, string>();
        public Stream GetFile(string filename)
        {
        retry:
            if (remap.ContainsKey(filename))
            {
                filename = remap[filename];
            }
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
            string origfilename = filename;
            for (int i = 0; i < 2; i++)
            {
                if (cache.ContainsKey(filename)) { return new MemoryStream(cache[filename]); }

                string f1 = filename.Replace(".png", ".jpg");
                if (cache.ContainsKey(f1)) { remap[origfilename] = f1; goto retry; }

                string f2 = filename.Replace(".jpg", ".png");
                if (cache.ContainsKey(f2)) { remap[origfilename] = f2; goto retry; }

                string f3 = filename.Replace(".wav", ".ogg");
                if (cache.ContainsKey(f3)) { remap[origfilename] = f3; goto retry; }

                string f4 = filename.Replace(".ogg", ".wav");
                if (cache.ContainsKey(f4)) { remap[origfilename] = f4; goto retry; }

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
        public void Crash(Exception e)
        {
            if (!Directory.Exists(gamepathcrash))
            {
                Directory.CreateDirectory(gamepathcrash);
            }
            string crashfile = Path.Combine(gamepathcrash, "ManicDiggerCrash.txt");
            File.WriteAllText(crashfile, e.ToString());
            Console.WriteLine(e);
            if (OnCrash != null)
            {
                OnCrash(this, new EventArgs());
            }
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    System.Windows.Forms.Cursor.Show();
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            Environment.Exit(1);
        }
        public event EventHandler OnCrash;
    }
}
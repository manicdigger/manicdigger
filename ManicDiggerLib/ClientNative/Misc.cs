using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.XPath;
using System.Xml;
using System.Threading;

namespace ManicDigger.ClientNative
{
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();

                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
    public class CrashReporter
    {
        private static string gamepathcrash = GameStorePath.GetStorePath();
        private static string s_strDefaultFileName = "ManicDiggerCrash.txt";

        private string m_strFileName = "";

        public Action OnCrash;

        /// <summary>
        /// If set to true, the crash will be written to the console.
        /// If set to false, the crash will be displayed in a MessageBox.
        /// </summary>
        private static bool s_blnIsConsole = false;

        /// <summary>
        /// Default filename for crash reports
        /// </summary>
        public static string DefaultFileName
        {
            get { return s_strDefaultFileName; }
            set { s_strDefaultFileName = value; }
        }

        /// <summary>
        /// Writes crashreport to the file specified in DefaultFileName
        /// </summary>
        public CrashReporter() : 
            this(s_strDefaultFileName)
        {
        }

        /// <summary>
        /// Writes crashreport to the given filename
        /// </summary>
        /// <param name="strFileName">Filename for the CrashReport</param>
        public CrashReporter(string strFileName)
        {
            m_strFileName = strFileName;
        }

        /// <summary>
        /// Enable global Exception handlin
        /// </summary>
        /// <param name="blnIsConsole">If set to true, the crash will be written to the console. If set to false, the crash will be displayed in a MessageBox.</param>
        public static void EnableGlobalExceptionHandling(bool blnIsConsole)
        {
            s_blnIsConsole = blnIsConsole;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Called after a unhandled exception occured
        /// </summary>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (s_blnIsConsole)
            {
                //Critical!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("Unhandled Exception occurred");
            }

            Exception ex = e.ExceptionObject as Exception;

            //Create CrashReport for exception
            CrashReporter reporter = new CrashReporter();
            reporter.Crash(ex);
        }

        /// <summary>
        /// Creates a CrashReport if the Action failed
        /// </summary>
        /// <param name="start">Action to execute</param>
        public void Start(ThreadStart start)
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
        

        /// <summary>
        /// Log the exception and exit the application
        /// </summary>
        public void Crash(Exception exCrash)
        {
            StringBuilder strGuiMessage = new StringBuilder();

            strGuiMessage.AppendLine(DateTime.Now.ToString() + "> Critical Error: " + exCrash.Message);

            //Write to Crash.txt
            try
            {
                if (!Directory.Exists(gamepathcrash))
                {
                    Directory.CreateDirectory(gamepathcrash);
                }

                string crashfile = Path.Combine(gamepathcrash, m_strFileName);

                //Open/Create Crash file
                using (FileStream fs = File.Open(crashfile, FileMode.Append))
                {
                    using (StreamWriter logger = new StreamWriter(fs))
                    {
                        Log(DateTime.Now.ToString() + ": Critical error occurred", logger);

                        //Call OnCrash logic
                        CallOnCrash(logger);

                        Exception exToLog = exCrash;

                        //Log the exception and its inner exceptions
                        while (exToLog != null)
                        {
                            Log(exToLog.ToString(), logger);
                            Log("-------------------------------", logger);
                            exToLog = exToLog.InnerException;
                        }
                    }
                }

                //Output Crashreport Location
                strGuiMessage.AppendLine("Crash report created: \"" + crashfile + "\"");
            }
            catch (Exception ex)
            {
                strGuiMessage.AppendLine("Crashreport failed: " + ex.ToString());
            }
            finally
            {
                DisplayInGui(strGuiMessage.ToString());

                if (s_blnIsConsole)
                {
                    //Give user time to read output
                    Console.WriteLine("Ending after critical error!");
                    Console.WriteLine("Press any key to shut down...");
                    Console.ReadLine();
                }

                //Shutdown
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Tries to execute the OnCrash delegate
        /// </summary>
        private void CallOnCrash(TextWriter logger)
        {
            if (OnCrash != null)
            {
                try
                {
                    OnCrash();
                }
                catch (Exception ex)
                {
                    logger.WriteLine("OnCrash() failed: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Write to logger and Console
        /// </summary>
        private void Log(string str, TextWriter logger)
        {
            logger.WriteLine(str);
            Console.WriteLine(str);
        }

        /// <summary>
        /// Displays a text in the gui
        /// </summary>
        private void DisplayInGui(string strTxt)
        {
            try
            {
                //Display error
                if (!s_blnIsConsole)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Cursor.Show();
                        Thread.Sleep(100);
                        Application.DoEvents();
                    }

                    MessageBox.Show(strTxt, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.WriteLine(strTxt);
                }
            }
            catch
            {
                //If this fails, something is really screwed... let's hope the crash report was created
                //Just swallow this exception, to prevent a exception endless loop (UnhandledException -> CrashReport -> UnhandledException)
            }
        }
        
    }

    public static class GameStorePath
    {
        public static bool IsMono = Type.GetType("Mono.Runtime") != null;

        public static string GetStorePath()
        {
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

        public static string gamepathconfig = Path.Combine(GameStorePath.GetStorePath(), "Configuration");
        public static string gamepathsaves = Path.Combine(GameStorePath.GetStorePath(), "Saves");
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
    public interface ICompression
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }
    public class CompressionGzip : ICompression
    {
        public byte[] Compress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (GZipStream compress = new GZipStream(output, CompressionMode.Compress))
            {
                byte[] buffer = new byte[4096];
                int numRead;
                while ((numRead = input.Read(buffer, 0, buffer.Length)) != 0)
                {
                    compress.Write(buffer, 0, numRead);
                }
            }
            return output.ToArray();
        }
        public byte[] Decompress(byte[] fi)
        {
            MemoryStream ms = new MemoryStream();
            // Get the stream of the source file.
            using (MemoryStream inFile = new MemoryStream(fi))
            {
                using (GZipStream Decompress = new GZipStream(inFile,
                        CompressionMode.Decompress))
                {
                    //Copy the decompression stream into the output file.
                    byte[] buffer = new byte[4096];
                    int numRead;
                    while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        ms.Write(buffer, 0, numRead);
                    }
                }
            }
            return ms.ToArray();
        }
        public static byte[] Decompress(FileInfo fi)
        {
            MemoryStream ms = new MemoryStream();
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length - fi.Extension.Length);

                //Create the decompressed file.
                //using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        //Copy the decompression stream into the output file.
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, numRead);
                        }
                        //Console.WriteLine("Decompressed: {0}", fi.Name);
                    }
                }
            }
            return ms.ToArray();
        }
    }
    struct TextAndSize
    {
        public string text;
        public float size;
        public override int GetHashCode()
        {
            if (text == null)
            {
                return 0;
            }
            return text.GetHashCode() ^ size.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is TextAndSize)
            {
                TextAndSize other = (TextAndSize)obj;
                return this.text == other.text && this.size == other.size;
            }
            return base.Equals(obj);
        }
    }
    //Doesn't work on Ubuntu - pointer access crashes.
    public class FastBitmap
    {
        public Bitmap bmp { get; set; }
        BitmapData bmd;
        public void Lock()
        {
            if (bmd != null)
            {
                throw new Exception("Already locked.");
            }
            if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                bmp = new Bitmap(bmp);
            }
            bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
        }
        public int GetPixel(int x, int y)
        {
            if (bmd == null)
            {
                throw new Exception();
            }
            unsafe
            {
                int* row = (int*)((byte*)bmd.Scan0 + (y * bmd.Stride));
                return row[x];
            }
        }
        public void SetPixel(int x, int y, int color)
        {
            if (bmd == null)
            {
                throw new Exception();
            }
            unsafe
            {
                int* row = (int*)((byte*)bmd.Scan0 + (y * bmd.Stride));
                row[x] = color;
            }
        }
        public void Unlock()
        {
            if (bmd == null)
            {
                throw new Exception("Not locked.");
            }
            bmp.UnlockBits(bmd);
            bmd = null;
        }
    }
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
                return (str != "0"
                    && (!str.Equals(bool.FalseString, StringComparison.InvariantCultureIgnoreCase)));
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
    public class XmlTool
    {
        public static string XmlVal(XmlDocument d, string path)
        {
            XPathNavigator navigator = d.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(path);
            foreach (XPathNavigator n in iterator)
            {
                return n.Value;
            }
            return null;
        }
        public static IEnumerable<string> XmlVals(XmlDocument d, string path)
        {
            XPathNavigator navigator = d.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(path);
            foreach (XPathNavigator n in iterator)
            {
                yield return n.Value;
            }
        }
        public static string X(string name, string value)
        {
            return string.Format("<{0}>{1}</{0}>", name, value);
        }
    }
}

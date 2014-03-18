using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

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
}

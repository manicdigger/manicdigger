using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel.Design;
using ProtoBuf;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ManicDigger
{
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
    public static class MyStream
    {
        public static string[] ReadAllLines(Stream s)
        {
            StreamReader sr = new StreamReader(s);
            List<string> lines = new List<string>();
            for (; ; )
            {
                string line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                lines.Add(line);
            }
            return lines.ToArray();
        }
        public static byte[] ReadAllBytes(Stream stream)
        {
            return new BinaryReader(stream).ReadBytes((int)stream.Length);
        }
    }
    public static class MyMath
    {
        public static T Clamp<T>(T value, T min, T max)
            where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        public static int Pow3(int n)
        {
            return n * n * n;
        }
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
    public class CompressionDummy : ICompression
    {
        #region ICompression Members
        public byte[] Compress(byte[] data)
        {
            return Copy(data);
        }
        public byte[] Decompress(byte[] data)
        {
            return Copy(data);
        }
        private static byte[] Copy(byte[] data)
        {
            byte[] copy = new byte[data.Length];
            Array.Copy(data, copy, data.Length);
            return copy;
        }
        #endregion
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
    public interface IFastBitmap
    {
        Bitmap bmp { get; set; }
        void Lock();
        void Unlock();
        int GetPixel(int x, int y);
        void SetPixel(int x, int y, int color);
    }
    public class FastBitmapDummy : IFastBitmap
    {
        #region IFastBitmap Members
        public Bitmap bmp { get; set; }
        public void Lock()
        {
        }
        public void Unlock()
        {
        }
        public int GetPixel(int x, int y)
        {
            return bmp.GetPixel(x, y).ToArgb();
        }
        public void SetPixel(int x, int y, int color)
        {
            bmp.SetPixel(x, y, Color.FromArgb(color));
        }
        #endregion
    }
    //Doesn't work on Ubuntu - pointer access crashes.
    public class FastBitmap : IFastBitmap
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
    public struct Vector2i
    {
        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x;
        public int y;
        public override bool Equals(object obj)
        {
            if (obj is Vector2i)
            {
                Vector2i other = (Vector2i)obj;
                return this.x == other.x && this.y == other.y;
            }
            return base.Equals(obj);
        }
        public static bool operator ==(Vector2i a, Vector2i b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return !(a.x == b.x && a.y == b.y);
        }
        public override int GetHashCode()
        {
            int hash = 23;
            unchecked
            {
                hash = hash * 37 + x;
                hash = hash * 37 + y;
            }
            return hash;
        }
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", x, y);
        }
    }
    public struct Vector3i
    {
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public int x;
        public int y;
        public int z;
        public override bool Equals(object obj)
        {
            if (obj is Vector3i)
            {
                Vector3i other = (Vector3i)obj;
                return this.x == other.x && this.y == other.y && this.z == other.z;
            }
            return base.Equals(obj);
        }
        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return !(a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public override int GetHashCode()
        {
            int hash = 23;
            unchecked
            {
                hash = hash * 37 + x;
                hash = hash * 37 + y;
                hash = hash * 37 + z;
            }
            return hash;
        }
        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", x, y, z);
        }
    }
    public class Timer
    {
        public double INTERVAL { get { return interval; } set { interval = value; } }
        public double MaxDeltaTime { get { return maxDeltaTime; } set { maxDeltaTime = value; } }
        double interval = 1;
        double maxDeltaTime = double.PositiveInfinity;

        double starttime;
        double oldtime;
        public double accumulator;
        public Timer()
        {
            Reset();
        }
        public void Reset()
        {
            starttime = gettime();
        }
        public delegate void Tick();
        public void Update(Tick tick)
        {
            double currenttime = gettime() - starttime;
            double deltaTime = currenttime - oldtime;
            accumulator += deltaTime;
            double dt = INTERVAL;
            if (MaxDeltaTime != double.PositiveInfinity && accumulator > MaxDeltaTime)
            {
                accumulator = MaxDeltaTime;
            }
            while (accumulator >= dt)
            {
                tick();
                accumulator -= dt;
            }
            oldtime = currenttime;
        }
        static double gettime()
        {
            return (double)DateTime.Now.Ticks / (10 * 1000 * 1000);
        }
    }
    public class FastStack<T>
    {
        public void Initialize(int maxCount)
        {
            values = new T[maxCount];
        }
        T[] values;
        public int Count;
        public void Push(T value)
        {
            while (Count >= values.Length)
            {
                Array.Resize(ref values, values.Length * 2);
            }
            values[Count] = value;
            Count++;
        }
        public T Pop()
        {
            Count--;
            return values[Count];
        }
        public void Clear()
        {
            Count = 0;
        }
    }

    public class MyLinq
    {
        public static bool Any<T>(IEnumerable<T> l)
        {
            return l.GetEnumerator().MoveNext();
        }
        public static T First<T>(IEnumerable<T> l)
        {
            var e = l.GetEnumerator();
            e.MoveNext();
            return e.Current;
        }
        public static int Count<T>(IEnumerable<T> l)
        {
            int count = 0;
            foreach (T v in l)
            {
                count++;
            }
            return count;
        }
        public static IEnumerable<T> Take<T>(IEnumerable<T> l, int n)
        {
            int i = 0;
            foreach (var v in l)
            {
                if (i >= n)
                {
                    yield break;
                }
                yield return v;
                i++;
            }
        }
        public static IEnumerable<T> Skip<T>(IEnumerable<T> l, int n)
        {
            var iterator = l.GetEnumerator();
            for (int i = 0; i < n; i++)
            {
                if (iterator.MoveNext() == false)
                    yield break;
            }
            while (iterator.MoveNext())
                yield return iterator.Current;
        }
    }
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
        public static unsafe ushort[] ByteArrayToUshortArray(byte[] a)
        {
            ushort[] output = new ushort[a.Length / 2];
            fixed (byte* a1 = a)
            {
                ushort* a2 = (ushort*)a1;
                for (int i = 0; i < a.Length / 2; i++)
                {
                    output[i] = a2[i];
                }
            }
            return output;
        }
    }
}
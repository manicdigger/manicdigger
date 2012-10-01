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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using Lidgren.Network;

namespace ManicDigger
{
    public static class GameStorePath
    {
        public static bool IsMono = Type.GetType("Mono.Runtime") != null;
     
        public static string GetStorePath()
        {
            string apppath = Path.GetDirectoryName(Application.ExecutablePath);
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

        public static bool IsValidName (string s)
        {
            if (s.Length < 1 || s.Length > 8)
            {
                return false;
            }
            for (int i=0; i<s.Length; i++)
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
    public interface IScreenshot
    {
        void SaveScreenshot();
    }
    public class Screenshot : IScreenshot
    {
        [Inject]
        public GameWindow d_GameWindow;
        public string SavePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public void SaveScreenshot()
        {
            using (Bitmap bmp = GrabScreenshot())
            {
                string time = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
                string filename = Path.Combine(SavePath, time + ".png");
                bmp.Save(filename);
            }
        }
        // Returns a System.Drawing.Bitmap with the contents of the current framebuffer
        public Bitmap GrabScreenshot()
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();

            Bitmap bmp = new Bitmap(d_GameWindow.ClientSize.Width, d_GameWindow.ClientSize.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(d_GameWindow.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, d_GameWindow.ClientSize.Width, d_GameWindow.ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }
    }
    public struct FastColor
    {
        public FastColor(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public FastColor(int A, int R, int G, int B)
        {
            this.A = (byte)A;
            this.R = (byte)R;
            this.G = (byte)G;
            this.B = (byte)B;
        }
        public FastColor(Color c)
        {
            this.A = c.A;
            this.R = c.R;
            this.G = c.G;
            this.B = c.B;
        }
        public byte A;
        public byte R;
        public byte G;
        public byte B;
        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }
    public class BitTools
    {
        public static bool IsPowerOfTwo(uint x)
        {
            return (
              x == 1 || x == 2 || x == 4 || x == 8 || x == 16 || x == 32 ||
              x == 64 || x == 128 || x == 256 || x == 512 || x == 1024 ||
              x == 2048 || x == 4096 || x == 8192 || x == 16384 ||
              x == 32768 || x == 65536 || x == 131072 || x == 262144 ||
              x == 524288 || x == 1048576 || x == 2097152 ||
              x == 4194304 || x == 8388608 || x == 16777216 ||
              x == 33554432 || x == 67108864 || x == 134217728 ||
              x == 268435456 || x == 536870912 || x == 1073741824 ||
              x == 2147483648);
        }
        public static uint NextPowerOfTwo(uint x)
        {
            x--;
            x |= x >> 1;  // handle  2 bit numbers
            x |= x >> 2;  // handle  4 bit numbers
            x |= x >> 4;  // handle  8 bit numbers
            x |= x >> 8;  // handle 16 bit numbers
            x |= x >> 16; // handle 32 bit numbers
            x++;
            return x;
        }
    }
    public static class Interpolation
    {
        public static FastColor InterpolateColor(float progress, params FastColor[] colors)
        {
            int colora = (int)((colors.Length - 1) * progress);
            if (colora < 0) { colora = 0; }
            if (colora >= colors.Length) { colora = colors.Length - 1; }
            int colorb = colora + 1;
            if (colorb >= colors.Length) { colorb = colors.Length - 1; }
            FastColor a = colors[colora];
            FastColor b = colors[colorb];
            float p = (progress - (float)colora / (colors.Length - 1)) * (colors.Length - 1);
            int A = (int)(a.A + (b.A - a.A) * p);
            int R = (int)(a.R + (b.R - a.R) * p);
            int G = (int)(a.G + (b.G - a.G) * p);
            int B = (int)(a.B + (b.B - a.B) * p);
            return new FastColor(A, R, G, B);
        }
    }
    public static class VectorTool
    {
        public static Vector3 ToVectorInFixedSystem(float dx, float dy, float dz, double orientationx, double orientationy)
        {
            //Don't calculate for nothing ...
            if (dx == 0.0f & dy == 0.0f && dz == 0.0f)
                return new Vector3();

            //Convert to Radian : 360° = 2PI
            double xRot = orientationx;//Math.toRadians(orientation.X);
            double yRot = orientationy;//Math.toRadians(orientation.Y);

            //Calculate the formula
            float x = (float)(dx * Math.Cos(yRot) + dy * Math.Sin(xRot) * Math.Sin(yRot) - dz * Math.Cos(xRot) * Math.Sin(yRot));
            float y = (float)(+dy * Math.Cos(xRot) + dz * Math.Sin(xRot));
            float z = (float)(dx * Math.Sin(yRot) - dy * Math.Sin(xRot) * Math.Cos(yRot) + dz * Math.Cos(xRot) * Math.Cos(yRot));

            //Return the vector expressed in the global axis system
            return new Vector3(x, y, z);
        }
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
                throw new Exception();
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
    public class DependencyChecker
    {
        [Inject]
        public Type[] d_InjectAttributes;
        public DependencyChecker()
        {
        }
        public DependencyChecker(params Type[] injectAttributes)
        {
            this.d_InjectAttributes = injectAttributes;
        }
        public void CheckDependencies(params object[] components)
        {
            if (d_InjectAttributes == null || d_InjectAttributes.Length == 0)
            {
                throw new Exception("Inject attributes list is null.");
            }
            foreach (object o in components)
            {
                CheckDependencies1(o);
            }
        }
        private void CheckDependencies1(object o)
        {
            Type type = o.GetType();
            var properties = type.GetProperties();
            var fields = type.GetFields();
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(true);
                foreach (var a in attributes)
                {
                    if (a is InjectAttribute)
                    {
                        if (property.GetValue(o, null) == null)
                        {
                            throw new Exception(string.Format("Dependency {0} of object of type {1} is null.", property.Name, type.Name));
                        }
                    }
                }
            }
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                foreach (var a in attributes)
                {
                    if (a is InjectAttribute)
                    {
                        if (field.GetValue(o) == null)
                        {
                            throw new Exception(string.Format("Dependency {0} of object of type {1} is null.", field.Name, type.Name));
                        }
                    }
                }
            }
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
    public class FastQueue<T>
    {
        public void Initialize(int maxCount)
        {
            this.maxCount = maxCount;
            values = new T[maxCount];
            Count = 0;
            start = 0;
            end = 0;
        }
        int maxCount;
        T[] values;
        public int Count;
        int start;
        int end;
        public void Push(T value)
        {
            /*
            if (Count >= values.Length)
            {
                Array.Resize(ref values, values.Length * 2);
            }
            */
            values[end] = value;
            Count++;
            end++;
            if (end >= maxCount)
            {
                end = 0;
            }
        }
        public T Pop()
        {
            T value = values[start];
            Count--;
            start++;
            if (start >= maxCount)
            {
                start = 0;
            }
            return value;
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
           for(int i=0; i<n; i++) 
           {
               if(iterator.MoveNext()==false)
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
    public static class Extensions
    {
        public static string GetWorkingDirectory(Assembly assembly)
        {
            return Path.GetDirectoryName(assembly.Location);
        }
        public static string GetRelativePath(Assembly assembly, string absolute)
        {
            return absolute.Replace(GetWorkingDirectory(assembly), "");
        }
        public static string GetAbsolutePath(Assembly assembly, string relative)
        {
            return Path.Combine(GetWorkingDirectory(assembly), relative);
        }
    }
    public static class Serializers
    {
        public static void XmlSerialize(Stream stream, object value)
        {
            XmlSerializer serializer = new XmlSerializer(value.GetType());
            serializer.Serialize(stream, value);
        }
        public static void XmlSerialize(string fileName, object value)
        {
            using (Stream s = File.Create(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(value.GetType());
                serializer.Serialize(s, value);
            }
        }
        public static object XmlDeserialize(string fileName, Type type)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(stream);
            }
        }
        public static object XmlDeserialize(Stream stream, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(stream);
        }
    }
    /// <summary>
    /// Provides an application-wide point for services.
    /// </summary>
    public sealed class Container : IServiceContainer
    {
        private static volatile Container _instance;

        public static Container Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Container();
                }
                return _instance;
            }
        }
        private ServiceContainer _serviceContainer = new ServiceContainer();
        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            _serviceContainer.AddService(serviceType, callback, promote);
        }
        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            _serviceContainer.AddService(serviceType, callback);
        }
        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            _serviceContainer.AddService(serviceType, serviceInstance, promote);
        }
        public void AddService(Type serviceType, object serviceInstance)
        {
            _serviceContainer.AddService(serviceType, serviceInstance);
        }
        public void RemoveService(Type serviceType, bool promote)
        {
            _serviceContainer.RemoveService(serviceType, promote);
        }
        public void RemoveService(Type serviceType)
        {
            _serviceContainer.RemoveService(serviceType);
        }
        public object GetService(Type serviceType)
        {
            return _serviceContainer.GetService(serviceType);
        }
    }
    /// <summary>
    /// Caches types in a big library to avoid having multiple classes scan multiple times.
    /// Any class can retrieve types from here to use them in whichever way they want.
    /// </summary>
    public sealed class TypeManager
    {
        /// <summary>
        /// The default capacity amount.
        /// See documentation for further information.
        /// </summary>
        /// <remarks>A greater value means more memory consumption, but increases scanning speed.
        /// A lower value means less memory consumption, but decreases scanning speed.</remarks>
        public const int DefaultAmount = 256;

        private static volatile TypeManager _instance;

        public static TypeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TypeManager();
                    _instance.Initialize(null);
                }
                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }
        public IList<Type> FoundTypes { get; private set; }

        private TypeManager()
        {
            IsInitialized = false;
            FoundTypes = new List<Type>(DefaultAmount);
        }
        private void Initialize(IList<string> assembliesToScan)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException();
            }

            Stopwatch sw = Stopwatch.StartNew();

            // if there are no desired assemblies then we take all assemblies we can find in the working directory
            if (assembliesToScan == null)
            {
                string workingDirectory = Extensions.GetWorkingDirectory(Assembly.GetExecutingAssembly());
                List<string> tmp = new List<string>(2);
                // alright, lets scan all assemblies in the working directory
                tmp.AddRange(Directory.GetFiles(workingDirectory, "*.exe", SearchOption.TopDirectoryOnly));
                tmp.AddRange(Directory.GetFiles(workingDirectory, "*.dll", SearchOption.TopDirectoryOnly));
                assembliesToScan = tmp;
            }

            // load and check each assembly's types
            foreach (string file in assembliesToScan)
            {
                Assembly assembly = null;

                try
                {
                    assembly = Assembly.Load(AssemblyName.GetAssemblyName(file));

                    ScanAssembly(assembly);

                }
                catch (FileLoadException)
                {
                    // this exception can be ignored here
                    continue;
                }
                catch (ReflectionTypeLoadException)
                {
                    // this exception can be ignored here
                    continue;
                }
                catch (TypeLoadException)
                {
                    // this exception can be ignored here
                    continue;
                }
                catch (BadImageFormatException)
                {
                    // this exception can be ignored here
                    continue;
                }
                catch (Exception ex)
                {
                    // other exceptions may be interesting though
                    sw.Stop();
                    throw ex;
                }
            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("Scanned {0} assemblies in {1} milliseconds (collected a total of {2} types).", assembliesToScan.Count, sw.ElapsedMilliseconds, FoundTypes.Count));

            IsInitialized = true;
        }
        private void ScanAssembly(Assembly assembly)
        {
            int amount = 0;

            // process all types (even private ones)
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type t = types[i];

                // add it
                FoundTypes.Add(t);
                amount++;
            }
        }
        public Type[] FindAll(Predicate<Type> predicate)
        {
            List<Type> tmp = new List<Type>(16);

            for (int i = 0; i < FoundTypes.Count; i++)
            {
                Type t = FoundTypes[i];
                if (predicate(t))
                {
                    tmp.Add(t);
                }
            }

            return tmp.ToArray();
        }
        public IList<Type> FindDescendants(Type superclass, bool includeAbstracts)
        {
            List<Type> tmp = new List<Type>(16);

            for (int i = 0; i < FoundTypes.Count; i++)
            {
                Type t = FoundTypes[i];
                if (t.IsSubclassOf(superclass))
                {
                    if (!includeAbstracts && t.IsAbstract)
                    {
                        continue;
                    }
                    tmp.Add(t);
                }
            }

            if (tmp.Count == 0)
            {
                return new Type[0];
            }

            return tmp.ToArray();
        }

        public IList<Type> FindImplementers(Type interfaceType, bool includeAbstracts)
        {
            List<Type> tmp = new List<Type>(16);

            for (int i = 0; i < FoundTypes.Count; i++)
            {
                Type t = FoundTypes[i];

                Type[] interfaces = t.GetInterfaces();
                for (int j = 0; j < interfaces.Length; j++)
                {
                    Type iface = interfaces[j];

                    if (iface == interfaceType)
                    {
                        if (!includeAbstracts && t.IsAbstract)
                        {
                            continue;
                        }

                        tmp.Add(t);
                    }
                }
            }

            return tmp;
        }
        public Type FindByAssemblyQualifiedName(string assemblyQualifiedName)
        {
            for (int i = 0; i < FoundTypes.Count; i++)
            {
                Type t = FoundTypes[i];
                if (t.AssemblyQualifiedName == assemblyQualifiedName)
                {
                    return t;
                }
            }

            return null;
        }
        public Type FindByFullName(string fullName)
        {
            for (int i = 0; i < FoundTypes.Count; i++)
            {
                Type t = FoundTypes[i];

                if (t.FullName == fullName)
                {
                    return t;
                }
            }

            return null;
        }
        public object CreateInstance(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }
        public T CreateInstance<T>(params object[] args)
        {
            return (T)CreateInstance(typeof(T), args);
        }
    }
    public interface INetServer
    {
        void Start();
        void Recycle(INetIncomingMessage msg);
        INetIncomingMessage ReadMessage();
        INetPeerConfiguration Configuration { get; }
        INetOutgoingMessage CreateMessage();
    }
    public interface INetPeerConfiguration
    {
        int Port { get; set; }
    }
    public interface INetClient
    {
        void Start();
        INetConnection Connect(string ip, int port);
        INetIncomingMessage ReadMessage();
        INetOutgoingMessage CreateMessage();
        void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method);
    }
    public interface INetConnection
    {
        IPEndPoint RemoteEndPoint { get; }
        void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel);
    }
    public interface INetIncomingMessage
    {
        INetConnection SenderConnection { get; }
        byte[] ReadBytes(int numberOfBytes);
        int LengthBytes { get; }
    }
    public interface INetOutgoingMessage
    {
        void Write(byte[] source);
    }


    public class MyNetServer : INetServer
    {
        public NetServer server;

        public void Start()
        {
            server.Start();
        }

        public void Recycle(INetIncomingMessage msg)
        {
            server.Recycle(((MyNetIncomingMessage)msg).message);
        }

        public INetIncomingMessage ReadMessage()
        {
            NetIncomingMessage msg = server.ReadMessage();
            if (msg == null)
            {
                return null;
            }
            if (msg.MessageType != NetIncomingMessageType.Data)
            {
                return null;
            }
            return new MyNetIncomingMessage() { message = msg };
        }

        public INetPeerConfiguration Configuration
        {
            get { return new MyNetPeerConfiguration() { configuration = server.Configuration}; }
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new MyNetOutgoingMessage() { message = server.CreateMessage() };
        }
    }

    public class MyNetPeerConfiguration : INetPeerConfiguration
    {
        public NetPeerConfiguration configuration;

        public int Port
        {
            get
            {
                return configuration.Port;
            }
            set
            {
                configuration.Port = value;
            }
        }
    }

    public class MyNetClient : INetClient
    {
        public NetClient client;
        public INetConnection Connect(string ip, int port)
        {
            return new MyNetConnection() { netConnection = client.Connect(ip, port) };
        }

        public INetIncomingMessage ReadMessage()
        {
            NetIncomingMessage msg = client.ReadMessage();
            if (msg == null)
            {
                return null;
            }
            if (msg.MessageType != NetIncomingMessageType.Data)
            {
                return null;
            }
            return new MyNetIncomingMessage() { message = msg };
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new MyNetOutgoingMessage() { message = client.CreateMessage() };
        }

        public void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            client.SendMessage(((MyNetOutgoingMessage)message).message, (NetDeliveryMethod)method);
        }

        public void Start()
        {
            client.Start();
        }
    }

    public enum MyNetDeliveryMethod
    {
        Unknown = 0,
        Unreliable = 1,
        UnreliableSequenced = 2,
        ReliableUnordered = 34,
        ReliableSequenced = 35,
        ReliableOrdered = 67,
    }

    public class MyNetOutgoingMessage : INetOutgoingMessage
    {
        public NetOutgoingMessage message;
        public void Write(byte[] source) { message.Write(source); }
    }

    public class MyNetIncomingMessage : INetIncomingMessage
    {
        public NetIncomingMessage message;
        public INetConnection SenderConnection { get { return new MyNetConnection() {  netConnection = message.SenderConnection }; } }
        public byte[] ReadBytes(int numberOfBytes) { return message.ReadBytes(numberOfBytes); }
        public int LengthBytes { get { return message.LengthBytes; } }
    }

    public class MyNetConnection : INetConnection
    {
        public NetConnection netConnection;
        public void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            netConnection.SendMessage(((MyNetOutgoingMessage)msg).message, (NetDeliveryMethod)method, sequenceChannel);
        }
        public IPEndPoint RemoteEndPoint
        {
            get { return netConnection.RemoteEndPoint; }
        }
        public override bool Equals(object obj)
        {
            if(obj != null && obj is MyNetConnection)
            {
                return netConnection.Equals(((MyNetConnection)obj).netConnection);
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return netConnection.GetHashCode();
        }
    }




    public class DummyNetClient : INetClient
    {
        public DummyNetwork network;
        public INetConnection Connect(string ip, int port)
        {
            return new DummyNetConnection();
        }

        public INetIncomingMessage ReadMessage()
        {
            lock (network.ClientReceiveBuffer)
            {
                if (network.ClientReceiveBuffer.Count > 0)
                {
                    var msg = new DummyNetIncomingmessage();
                    msg.message = network.ClientReceiveBuffer.Dequeue();
                    return msg;
                }
                else
                {
                    return null;
                }
            }
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new DummyNetOutgoingMessage();
        }

        public void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method)
        {
            lock (network.ServerReceiveBuffer)
            {
                network.ServerReceiveBuffer.Enqueue(((DummyNetOutgoingMessage)message).data);
            }
        }

        public void Start()
        {
        }
    }
    public class DummyNetConnection : INetConnection
    {
        public DummyNetwork network;
        public void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel)
        {
            lock (network.ClientReceiveBuffer)
            {
                network.ClientReceiveBuffer.Enqueue(((DummyNetOutgoingMessage)msg).data);
            }
        }
        public IPEndPoint RemoteEndPoint
        {
            get { return new IPEndPoint(0, 0); }
        }
    }
    public class DummyNetIncomingmessage : INetIncomingMessage
    {
        public byte[] message;
        public INetConnection SenderConnection { get; set; }

        public byte[] ReadBytes(int numberOfBytes)
        {
            if (numberOfBytes != message.Length)
            {
                throw new Exception();
            }
            return message;
        }

        public int LengthBytes { get { return message.Length; } }
    }
    public class DummyNetOutgoingMessage : INetOutgoingMessage
    {
        public byte[] data;
        public void Write(byte[] source)
        {
            data = new byte[source.Length];
            Array.Copy(source, data, source.Length);
        }
    }
    public class DummyNetServer : INetServer
    {
        public DummyNetwork network;
        public void Start()
        {
        }

        public void Recycle(INetIncomingMessage msg)
        {
        }

        INetConnection connectedClient = new DummyNetConnection();

        public INetIncomingMessage ReadMessage()
        {
            ((DummyNetConnection)connectedClient).network = network;
            lock (network.ServerReceiveBuffer)
            {
                if (network.ServerReceiveBuffer.Count > 0)
                {
                    return new DummyNetIncomingmessage() { message = network.ServerReceiveBuffer.Dequeue(), SenderConnection = connectedClient };
                }
                else
                {
                    return null;
                }
            }
        }

        DummyNetPeerConfiguration configuration = new DummyNetPeerConfiguration();
        public INetPeerConfiguration Configuration
        {
            get { return configuration; }
        }

        public INetOutgoingMessage CreateMessage()
        {
            return new DummyNetOutgoingMessage();
        }
    }

    public class DummyNetPeerConfiguration : INetPeerConfiguration
    {
        public int Port { get; set; }
    }

    public class DummyNetwork
    {
        public Queue<byte[]> ServerReceiveBuffer = new Queue<byte[]>();
        public Queue<byte[]> ClientReceiveBuffer = new Queue<byte[]>();
    }

    public class MyUri
    {
        public MyUri(string uri)
        {
            //string url = "md://publichash:123/?user=a&auth=123";
            var a = new Uri(uri);
            Ip = a.Host;
            Port = a.Port;
            Get = ParseGet(uri);
        }
        public string Url { get; private set; }
        public string Ip { get; private set; }
        public int Port { get; private set; }
        public Dictionary<string, string> Get { get; private set; }
        private static Dictionary<string, string> ParseGet(string url)
        {
            try
            {
                Dictionary<string, string> d;
                d = new Dictionary<string, string>();
                if (url.Contains("?"))
                {
                    string url2 = url.Substring(url.IndexOf("?") + 1);
                    var ss = url2.Split(new char[] { '&' });
                    for (int i = 0; i < ss.Length; i++)
                    {
                        var ss2 = ss[i].Split(new char[] { '=' });
                        d[ss2[0]] = ss2[1];
                    }
                }
                return d;
            }
            catch
            {
                throw new FormatException("Invalid address: " + url);
            }
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

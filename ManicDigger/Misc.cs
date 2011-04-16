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

namespace ManicDigger
{
    public static class GameStorePath
    {
        public static string GetStorePath()
        {
            string apppath = Path.GetDirectoryName(Application.ExecutablePath);
            string mdfolder = "ManicDiggerUserData";
            if (apppath.Contains(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
            {
                string mdpath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    mdfolder);
                return mdpath;
            }
            else
            {
                //return Path.Combine(apppath, mdfolder);
                return mdfolder;
            }
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
    }
    public class DependencyChecker
    {
        [Inject]
        public Type[] InjectAttributes;
        public DependencyChecker()
        {
        }
        public DependencyChecker(params Type[] injectAttributes)
        {
            this.InjectAttributes = injectAttributes;
        }
        public void CheckDependencies(params object[] components)
        {
            if (InjectAttributes == null || InjectAttributes.Length == 0)
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
        double accumulator;
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
    class FastStack<T>
    {
        public void Initialize(int maxCount)
        {
            values = new T[maxCount];
        }
        T[] values;
        public int Count;
        public void Push(T value)
        {
            if (Count >= values.Length)
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
}

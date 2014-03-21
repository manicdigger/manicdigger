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
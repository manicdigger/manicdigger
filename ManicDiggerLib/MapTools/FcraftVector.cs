using System;

namespace fCraft
{

    public struct Vector3i
    {
        public int x, z, y;

        public double GetLength()
        {
            return Math.Sqrt(x * x + z * z + y * y);
        }

        public static bool operator >(Vector3i a, Vector3i b)
        {
            return a.GetLength() > b.GetLength();
        }
        public static bool operator <(Vector3i a, Vector3i b)
        {
            return a.GetLength() < b.GetLength();
        }
        public static bool operator >=(Vector3i a, Vector3i b)
        {
            return a.GetLength() >= b.GetLength();
        }
        public static bool operator <=(Vector3i a, Vector3i b)
        {
            return a.GetLength() <= b.GetLength();
        }
        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.GetLength() == b.GetLength();
        }
        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return a.GetLength() != b.GetLength();
        }
        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x + b.x, a.z + b.z, a.y + b.y);
        }
        public static Vector3i operator +(Vector3i a, int scalar)
        {
            return new Vector3i(a.x + scalar, a.z + scalar, a.y + scalar);
        }
        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.z - b.z, a.y - b.y);
        }
        public static Vector3i operator -(Vector3i a, int scalar)
        {
            return new Vector3i(a.x - scalar, a.z - scalar, a.y - scalar);
        }
        public static Vector3i operator *(Vector3i a, double scalar)
        {
            return new Vector3i((int)(a.x * scalar), (int)(a.z * scalar), (int)(a.y * scalar));
        }
        public static Vector3i operator /(Vector3i a, double scalar)
        {
            return new Vector3i((int)(a.x / scalar), (int)(a.z / scalar), (int)(a.y / scalar));
        }

        public override int GetHashCode()
        {
            return x + z * 1625 + y * 2642245;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3i)
            {
                return this == (Vector3i)obj;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return y;
                    default: return z;
                }
            }
            set
            {
                switch (i)
                {
                    case 0: x = value; return;
                    case 1: y = value; return;
                    default: z = value; return;
                }
            }
        }

        public Vector3i(int _x, int _y, int _h)
        {
            x = _x;
            z = _y;
            y = _h;
        }
        public Vector3i(Vector3i other)
        {
            x = other.x;
            z = other.z;
            y = other.y;
        }
        public Vector3i(Vector3f other)
        {
            x = (int)other.x;
            z = (int)other.y;
            y = (int)other.h;
        }

        public int GetLargestComponent()
        {
            int maxVal = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
            if (maxVal == Math.Abs(x)) return 0;
            if (maxVal == Math.Abs(y)) return 1;
            return 2;
        }
    }



    public struct Vector3f
    {
        public float x, y, h;

        public float GetLength()
        {
            return (float)Math.Sqrt(x * x + y * y + h * h);
        }

        public static bool operator >(Vector3f a, Vector3f b)
        {
            return a.GetLength() > b.GetLength();
        }
        public static bool operator <(Vector3f a, Vector3f b)
        {
            return a.GetLength() < b.GetLength();
        }
        public static bool operator >=(Vector3f a, Vector3f b)
        {
            return a.GetLength() >= b.GetLength();
        }
        public static bool operator <=(Vector3f a, Vector3f b)
        {
            return a.GetLength() <= b.GetLength();
        }
        public static bool operator ==(Vector3f a, Vector3f b)
        {
            return a.GetLength() == b.GetLength();
        }
        public static bool operator !=(Vector3f a, Vector3f b)
        {
            return a.GetLength() != b.GetLength();
        }
        public static Vector3f operator +(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x + b.x, a.y + b.y, a.h + b.h);
        }
        public static Vector3f operator +(Vector3f a, float scalar)
        {
            return new Vector3f(a.x + scalar, a.y + scalar, a.h + scalar);
        }
        public static Vector3f operator -(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x - b.x, a.y - b.y, a.h - b.h);
        }
        public static Vector3f operator -(Vector3f a, float scalar)
        {
            return new Vector3f(a.x - scalar, a.y - scalar, a.h - scalar);
        }
        public static Vector3f operator *(Vector3f a, float scalar)
        {
            return new Vector3f((float)(a.x * scalar), (float)(a.y * scalar), (float)(a.h * scalar));
        }
        public static Vector3f operator /(Vector3f a, double scalar)
        {
            return new Vector3f((float)(a.x / scalar), (float)(a.y / scalar), (float)(a.h / scalar));
        }

        public static Vector3f operator +(Vector3i a, Vector3f b)
        {
            return new Vector3f(a.x + b.x, a.z + b.y, a.y + b.h);
        }
        public static Vector3f operator +(Vector3f a, Vector3i b)
        {
            return new Vector3f(a.x + b.x, a.y + b.z, a.h + b.y);
        }
        public static Vector3f operator -(Vector3i a, Vector3f b)
        {
            return new Vector3f(a.x - b.x, a.z - b.y, a.y - b.h);
        }
        public static Vector3f operator -(Vector3f a, Vector3i b)
        {
            return new Vector3f(a.x - b.x, a.y - b.z, a.h - b.y);
        }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return h;
                    default: return y;
                }
            }
            set
            {
                switch (i)
                {
                    case 0: x = value; return;
                    case 1: h = value; return;
                    default: y = value; return;
                }
            }
        }

        public Vector3f(float _x, float _y, float _h)
        {
            x = _x;
            y = _y;
            h = _h;
        }
        public Vector3f(Vector3f other)
        {
            x = other.x;
            y = other.y;
            h = other.h;
        }
        public Vector3f(Vector3i other)
        {
            x = other.x;
            y = other.z;
            h = other.y;
        }

        public override int GetHashCode()
        {
            return (int)(x + y * 1625 + h * 2642245);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3f)
            {
                return this == (Vector3f)obj;
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}
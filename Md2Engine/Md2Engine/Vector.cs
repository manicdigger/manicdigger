using System;

namespace Md2Engine
{
    interface IVector
    {
        float getLength();
        float getX();
        float getY();
        float getZ();
        void normalize();
        void setX(float px);
        void setY(float py);
        void setZ(float pz);
    }
    public class Vector : IVector
    {
        private float x, y, z;

        //=======================
        // Vector Constructors
        //=======================

        public Vector()
        {
        }

        public Vector(float px, float py, float pz)
        {
            x = px;
            y = py;
            z = pz;
        }

        public Vector(Vector sp, Vector ep)
        {
            x = ep.getX() - sp.getX();
            y = ep.getY() - sp.getY();
            z = ep.getZ() - sp.getZ();
        }

        //=======================
        // Setters/Getters
        //=======================

        public void setX(float px)
        {
            x = px;
        }

        public float getX()
        {
            return x;
        }

        public void setY(float py)
        {
            y = py;
        }

        public float getY()
        {
            return y;
        }

        public void setZ(float pz)
        {
            z = pz;
        }

        public float getZ()
        {
            return z;
        }

        //=======================
        // Misc functions
        //=======================

        public float getLength() //returns the vecotr's length
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        public void normalize() //normalize the vector => the versor (length = 0)
        {
            float length = getLength();

            x = x / length;
            y = y / length;
            z = z / length;
        }

        public static float scalarProduct(Vector a, Vector b) //scalar product of two vectors (a value)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector dotProduct(Vector a, Vector b) //dot product of two vectors (a vector)
        {
            Vector tmp = new Vector();

            tmp.x=a.y*b.z - a.z*b.y;
            tmp.y=a.z*b.x - a.x*b.z;
            tmp.z=a.x*b.y - a.y*b.x;

            return tmp;
        }
    }
}

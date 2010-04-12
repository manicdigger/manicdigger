namespace Md2Engine
{
    interface IVertex
    {
        int getConnections();
        Vector getNormal();
        float getX();
        float getY();
        float getZ();
        void setConnections(int nc);
        void setNormal(Vector nrm);
        void setX(float px);
        void setXYZ(float px, float py, float pz);
        void setY(float py);
        void setZ(float pz);
    }
    public class Vertex : IVertex
    {
        private float x;
        private float y;
        private float z;

        private Vector normal;
        private int connections;

        public Vertex()
        {
        }

        public Vertex(float px, float py, float pz)
        {
            x = px;
            y = py;
            z = pz;
        }

        //=======================
        // X, Y, Z Setters
        //=======================

        public void setXYZ(float px, float py, float pz)
        {
            x = px;
            y = py;
            z = pz;
        }

        public void setX(float px)
        {
            x = px;
        }

        public void setY(float py)
        {
            y = py;
        }

        public void setZ(float pz)
        {
            z = pz;
        }

        //=======================
        // X, Y, Z Getters
        //=======================

        public float getX()
        {
            return x;
        }

        public float getY()
        {
            return y;
        }

        public float getZ()
        {
            return z;
        }

        //=======================
        // Normal vector Set/Get
        //=======================

        public void setNormal(Vector nrm)
        {
            normal = nrm;
        }

        public Vector getNormal()
        {
            return normal;
        }

        //=======================
        // no of connections Set/Get
        //=======================

        public void setConnections(int nc)
        {
            connections = nc;
        }

        public int getConnections()
        {
            return connections;
        }
    }
}

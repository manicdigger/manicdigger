namespace Md2Engine
{
    interface IGLCommPacket
    {
        float getU();
        float getV();
        int getVertex();
        void setU(float pu);
        void setV(float pv);
        void setVertex(int pvrt);
    }
    public class GLCommPacket : IGLCommPacket
    {
        private float u, v; //tex coordinate
        private int vertex; //vertex pointer

        //CONSTRUCTORS

        public GLCommPacket()
        {
        }

        public GLCommPacket(float pu, float pv, int pvrt)
        {
            u = pu;
            v = pv;
            vertex = pvrt;
        }

        //SETTERS/GETTERS

        public void setU(float pu)
        {
            u = pu;
        }

        public void setV(float pv)
        {
            v = pv;
        }

        public void setVertex(int pvrt)
        {
            vertex = pvrt;
        }

        public float getU()
        {
            return u;
        }

        public float getV()
        {
            return v;
        }

        public int getVertex()
        {
            return vertex;
        }
    }
}

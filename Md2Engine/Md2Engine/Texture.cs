namespace Md2Engine
{
    interface ITexCoordinate
    {
        float getU();
        float getV();
        void setU(float pu);
        void setV(float pv);
    }
    public class TexCoordinate : ITexCoordinate
    {
        private float u;
        private float v;

        //========================
        // Constructors
        //========================

        public TexCoordinate()
        {
        }

        public TexCoordinate(float pu, float pv)
        {
            u = pu;
            v = pv;
        }

        //========================
        // Setters/Getters
        //========================

        public void setU(float pu)
        {
            u = pu;
        }

        public void setV(float pv)
        {
            v = pv;
        }

        public float getU()
        {
            return u;
        }

        public float getV()
        {
            return v;
        }
    }
}

namespace Md2Engine
{
    interface ITriangle
    {
        int getA();
        int getB();
        int getC();
        int getTA();
        int getTB();
        int getTC();
        void setA(int pa);
        void setABC(int pa, int pb, int pc);
        void setB(int pb);
        void setC(int pc);
        void setTA(int pta);
        void setTABC(int pta, int ptb, int ptc);
        void setTB(int ptb);
        void setTC(int ptc);
    }
    public class Triangle : ITriangle
    {
        private int a, b, c; //pointers into the vertexPool for each corner
        private int ta, tb, tc; //pointers into the textCoordPool for each corner

        //============================
        // Constructors
        //============================

        public Triangle()
        {
        }

        public Triangle(int pa, int pb, int pc)
        {
            a = pa;
            b = pb;
            c = pc;
        }

        //============================
        // Setters for A, B, C
        //============================

        public void setA(int pa)
        {
            a = pa;
        }
        public void setB(int pb)
        {
            b = pb;
        }
        public void setC(int pc)
        {
            c = pc;
        }

        public void setABC(int pa, int pb, int pc)
        {
            a = pa;
            b = pb;
            c = pc;
        }

        //============================
        // Getters fo A, B, C
        //============================

        public int getA()
        {
            return a;
        }
        public int getB()
        {
            return b;
        }
        public int getC()
        {
            return c;
        }

        //============================
        // Setters for TA, TB, TC
        //============================

        public void setTA(int pta)
        {
            ta = pta;
        }
        public void setTB(int ptb)
        {
            tb = ptb;
        }
        public void setTC(int ptc)
        {
            tc = ptc;
        }

        public void setTABC(int pta, int ptb, int ptc)
        {
            ta = pta;
            tb = ptb;
            tc = ptc;
        }

        //============================
        // Getters fro TA, TB, TC
        //============================

        public int getTA()
        {
            return ta;
        }
        public int getTB()
        {
            return tb;
        }
        public int getTC()
        {
            return tc;
        }
    }

}

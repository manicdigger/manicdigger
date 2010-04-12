namespace Md2Engine
{
    interface IRange
    {
        int getEnd();
        string getName();
        int getStart();
        void setEnd(int pe);
        void setName(string pn);
        void setStart(int ps);
    }
    public class Range : Md2Engine.IRange //this is a range of frames with a name, comonenet of the animationPool
    {
        private string name;
        private int start;
        private int end;

        //=========================
        // Constructors
        //=========================

        public Range()
        {
        }

        public Range(string pn, int ps, int pe)
        {
            name = pn;
            start = ps;
            end = pe;
        }

        //============================
        // Setters/Getters
        //============================

        public void setName(string pn)
        {
            name = pn;
        }

        public string getName()
        {
            return name;
        }

        public void setStart(int ps)
        {
            start = ps;
        }

        public void setEnd(int pe)
        {
            end = pe;
        }

        public int getStart()
        {
            return start;
        }

        public int getEnd()
        {
            return end;
        }
    }
}

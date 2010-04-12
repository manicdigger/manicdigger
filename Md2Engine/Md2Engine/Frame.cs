using System.Collections.Generic;

namespace Md2Engine
{
    interface IFrame
    {
        string getName();
        void setName(string pn);
    }
    public class Frame : IFrame
    {
        private string name;
        public List<Vertex> vertexPool;
        public List<Vector> normalsPool;

        public Frame() //constructor
        {
            name = "unknown";
            vertexPool = new List<Vertex>();
            normalsPool = new List<Vector>();
        }

        //===============================
        // Set/Get Name
        //===============================
        public void setName(string pn)
        {
            name = pn;
        }

        public string getName()
        {
            return name;
        }
    }
}

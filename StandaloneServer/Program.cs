using System;
using System.Collections.Generic;
using System.Text;
using GameModeFortress;
using System.Threading;

namespace StandaloneServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = FortressModeServerFactory.create(false);
            s.Start();
            while (true)
            {
                s.Process();
                Thread.Sleep(1);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GameModeFortress;
using ManicDigger;

namespace GameClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IInternetGameFactory game = new ManicDiggerProgram2(); 
        }
    }
}

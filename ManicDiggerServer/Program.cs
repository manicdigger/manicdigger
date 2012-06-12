using System;
using System.Diagnostics;
using System.Threading;
using GameModeFortress;
using ManicDigger;
using ManicDigger.MapTools;
using ManicDigger.MapTools.Generators;
using System.IO;

namespace ManicDiggerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new CrashReporter().Start(Main2);
        }
        static void Main2()
        {
            Server server = new Server();
            server.exit = new GameExitDummy();
            server.Public = true;
            server.Start();
            for (; ; )
            {
                server.Process();
                Thread.Sleep(1);
                if (server.exit != null && server.exit.exit) { return; }
            }
        }
    }
}
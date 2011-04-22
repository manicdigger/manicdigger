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
            ServerProgram server = new ServerProgram();
            server.d_Exit = new GameExitDummy();
            ServerProgram.Public = true;
            new CrashReporter().Start(server.Start);
        }
    }
}
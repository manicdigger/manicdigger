using System;
using System.Diagnostics;
using System.Threading;
using GameModeFortress;
using ManicDigger;
using ManicDigger.MapTools;
using ManicDigger.MapTools.Generators;
using System.IO;
using System.Net.Sockets;

namespace ManicDiggerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program(args);
        }
        public Program(string[] args)
        {
            new CrashReporter().Start(Main2);
        }
        void Main2()
        {
            new Thread(AutoRestarter).Start();
        }
        private void Server()
        {
            server = new Server();
            server.exit = new GameExitDummy();
            server.Public = true;
            server.Start();
            for (; ; )
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
                port = server.config.Port;
                server.Process();
                Thread.Sleep(1);
                if (server.exit != null && server.exit.exit) { return; }
            }
        }
        Stopwatch stopwatch = new Stopwatch();
        Thread ServerThread;
        Server server;
        int port;
        void AutoRestarter()
        {
            Restart();
            for (; ; )
            {
                //a) server.Process() too long
                if (stopwatch.Elapsed.Seconds > 15)
                {
                    Restart();
                }
                //b) routine restart
                if (RoutineRestartStopwatch == null)
                {
                    RoutineRestartStopwatch = new Stopwatch();
                    RoutineRestartStopwatch.Start();
                }
                if (RoutineRestartStopwatch.Elapsed.Hours > 6)
                {
                    RoutineRestartStopwatch = new Stopwatch();
                    RoutineRestartStopwatch.Start();
                    Restart();
                }
                //c) admin command /restart
                if (server != null && server.restartserver)
                {
                    server.restartserver = false;
                    Restart();
                }

                Thread.Sleep(1);
            }
        }
        Stopwatch RoutineRestartStopwatch;

        private void Restart()
        {
            if (ServerThread != null)
            {
                ServerThread.Abort();
                while (ServerThread.ThreadState != System.Threading.ThreadState.Stopped)
                {
                    Thread.Sleep(1);
                }
                try
                {
                    server.Dispose();
                }
                catch
                {
                }
            }
            ServerThread = new Thread(Server);
            ServerThread.Start();
            stopwatch = new Stopwatch();
        }
    }
}
using System;
using System.Diagnostics;
using System.Threading;
using GameModeFortress;
using ManicDigger;
using ManicDigger.MapTools;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Net;

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
            ENABLE_REDIRECT_STANDARD_INPUT = IsMono;
            if (args.Length > 0)
            {
                IsAutoRestarter = false;
                parentid = int.Parse(args[0]);
            }
            new CrashReporter().Start(Main2);
        }

        bool ENABLE_REDIRECT_STANDARD_INPUT;
        bool ENABLE_AUTORESTARTER = false;
        int parentid;
        bool IsAutoRestarter = true;
        int autoRestartCycle = 6;

        void Main2()
        {
            if (ENABLE_AUTORESTARTER)
            {
                if (IsAutoRestarter)
                {
                    new Thread(ConsoleOutput).Start();
                    if (ENABLE_REDIRECT_STANDARD_INPUT)
                    {
                        new Thread(ConsoleInput).Start();
                    }
                    ParentAutoRestarter();
                }
                else
                {
                    ChildServer();
                }
            }
            else
            {
                ChildServer();
            }
        }

        private void ChildServer()
        {
            Server server = new Server();
            server.exit = new GameExitDummy();
            server.Public = true;
            server.Start();
            autoRestartCycle = server.config.AutoRestartCycle;
            for (; ; )
            {
                port = server.config.Port;
                server.Process();
                Thread.Sleep(1);
                if (server.exit != null && server.exit.exit) { return; }

                if (!ENABLE_AUTORESTARTER)
                {
                    continue;
                }
                if (parentCheckStopwatch == null)
                {
                    parentCheckStopwatch = new Stopwatch();
                    parentCheckStopwatch.Start();
                }
                if (parentCheckStopwatch.Elapsed.Milliseconds > 100)
                {
                    try
                    {
                        Process.GetProcessById(parentid);
                    }
                    catch
                    {
                        server.Exit();
                        return;
                    }
                    parentCheckStopwatch = new Stopwatch();
                    parentCheckStopwatch.Start();
                }
            }
        }

        Stopwatch parentCheckStopwatch;
        Stopwatch stopwatch = new Stopwatch();
        Process ServerProcess;
        int port;
        
        void ConsoleOutput()
        {
            for (; ; )
            {
                if (ServerProcess != null && !ServerProcess.HasExited)
                {
                    Console.WriteLine(ServerProcess.StandardOutput.ReadLine());
                }
                Thread.Sleep(1);
            }
        }

        void ConsoleInput()
        {
            for (; ; )
            {
                if (ServerProcess != null && !ServerProcess.HasExited)
                {
                    string s = Console.ReadLine();
                    ServerProcess.StandardInput.WriteLine(s);
                }
                Thread.Sleep(1);
            }
        }

        void ParentAutoRestarter()
        {
            Restart();
            for (; ; )
            {
                //a) server process not found
                if (ServerProcess.HasExited)
                {
                    Restart();
                }

                //b) routine restart
                if (RoutineRestartStopwatch == null)
                {
                    RoutineRestartStopwatch = new Stopwatch();
                    RoutineRestartStopwatch.Start();
                }
                if (RoutineRestartStopwatch.Elapsed.Hours > autoRestartCycle)
                {
                    RoutineRestartStopwatch = new Stopwatch();
                    RoutineRestartStopwatch.Start();
                    Restart();
                }

                Thread.Sleep(1);
            }
        }

        Stopwatch RoutineRestartStopwatch;

        public bool IsMono = Type.GetType("Mono.Runtime") != null;

        private void Restart()
        {
            if (ServerProcess != null && (!ServerProcess.HasExited))
            {
                ServerProcess.Kill();
                ServerProcess = null;
            }
            ProcessStartInfo p = new ProcessStartInfo();
            if (!IsMono)
            {
                p.FileName = System.Windows.Forms.Application.ExecutablePath;
                p.Arguments = Process.GetCurrentProcess().Id.ToString();
            }
            else
            {
                p.FileName = "mono";
                p.Arguments = System.Windows.Forms.Application.ExecutablePath + " " + Process.GetCurrentProcess().Id.ToString();
            }
            
            //p.WindowStyle = ProcessWindowStyle.Hidden;
            
            p.RedirectStandardOutput = true;
            if (ENABLE_REDIRECT_STANDARD_INPUT) // fix
            {
                p.RedirectStandardInput = true;
            }
            p.UseShellExecute = false;
            
            ServerProcess = Process.Start(p);
        }

        
    }
}
using System;
using System.Diagnostics;
using System.Threading;
using ManicDigger;
using System.Xml;
using System.IO;
using ManicDigger.ClientNative;

namespace ManicDiggerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Catch unhandled exceptions
            CrashReporter.DefaultFileName = "ManicDiggerServerCrash.txt";
            CrashReporter.EnableGlobalExceptionHandling(true);

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
        bool ENABLE_AUTORESTARTER = !Debugger.IsAttached;
        int parentid;
        bool IsAutoRestarter = true;
        const string lockFileName = "ManicDiggerServer.lck";

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
                    //Start as server process when parent process ID is given
                    ChildServer();
                }
            }
            else
            {
                //Just start as server process if automatic restarting is disabled
                ChildServer();
            }
            //End program (kills Input/Output Redirects)
            Environment.Exit(0);
        }

        void ChildServer()
        {
            try
            {
                //Try to create the lockfile at startup
                File.Create(lockFileName);
            }
            catch
            {
                Console.WriteLine("[SERVER] Lockfile could not be created! Shutdown will not work!");
            }
            Server server = new Server();
            server.exit = new GameExit();
            server.Public = true;
            for (; ; )
            {
                server.Process();
                Thread.Sleep(1);

                //Check if server wants to exit
                if (server.exit != null && server.exit.GetExit())
                {
                    //If so, save data
                    server.Stop();

                    //Check if server wants to be restarted
                    if (!server.exit.GetRestart())
                    {
                        //Delete the lockfile if server wants to be shutdown
                        try
                        {
                            File.Delete(lockFileName);
                        }
                        catch
                        {
                            Console.WriteLine("[SERVER] Lockfile could not be deleted! Server will be restarted!");
                        }
                    }

                    //Finally kill the server process
                    return;
                }

                //Check if parent process is still running
                if (!ENABLE_AUTORESTARTER)
                {
                    //Only do so when automatic restarts are enabled
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
                    parentCheckStopwatch.Reset();
                    parentCheckStopwatch.Start();
                }
            }
        }

        Stopwatch parentCheckStopwatch;
        Stopwatch stopwatch = new Stopwatch();
        Process ServerProcess;
        
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
                //server process not found
                if (ServerProcess.HasExited)
                {
                    // I) Server wants to be shutdown (lockfile has been deleted by child server)
                    if (!File.Exists(lockFileName))
                    {
                        Console.WriteLine("[SERVER] Successful shutdown");
                        return;
                    }

                    // II) Server wants to be restarted or has crashed (lockfile not deleted)
                    Console.WriteLine("[SERVER] Will restart");
                    Restart();
                }
                Thread.Sleep(1);
            }
        }

        public bool IsMono = Type.GetType("Mono.Runtime") != null;

        void Restart()
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
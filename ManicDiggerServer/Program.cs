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
            new Program(args);
        }
        
        public Program(string[] args)
        {
            try
            {
                using (Stream s = new MemoryStream(File.ReadAllBytes(Path.Combine(GameStorePath.gamepathconfig, "ServerConfig.txt"))))
                {
                    StreamReader sr = new StreamReader(s);
                    XmlDocument d = new XmlDocument();
                    d.Load(sr);
                    autoRestartCycle = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AutoRestartCycle"));
                }
            }
            catch
            {
                //ServerConfig cannot be read. Use default value of 6 hours
                autoRestartCycle = 6;
            }
            
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
        int autoRestartCycle = 6;
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
                    Console.WriteLine("AutoRestartCycle: {0}", autoRestartCycle);
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
            server.Start();
            autoRestartCycle = server.config.AutoRestartCycle;
            port = server.config.Port;
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
                    Process.GetCurrentProcess().Kill();
                }

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

                //b) routine restart
                if (RoutineRestartStopwatch == null)
                {
                    RoutineRestartStopwatch = new Stopwatch();
                    RoutineRestartStopwatch.Start();
                }
                if (RoutineRestartStopwatch.Elapsed.Hours >= autoRestartCycle)
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
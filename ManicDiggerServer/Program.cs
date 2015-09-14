using System;
using System.Diagnostics;
using System.Threading;
using ManicDigger;
using System.IO;
using ManicDigger.ClientNative;
using System.Runtime.InteropServices;

namespace ManicDiggerServer
{
	class Program
	{
		static volatile bool exitSystem = false;

		#region Application termination
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(CloseEventHandler handler, bool add);

		private delegate bool CloseEventHandler(CtrlType sig);
		static CloseEventHandler _handler;

		enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		private static bool Handler(CtrlType sig)
		{
			if (IsAutoRestarter)
			{
				Console.WriteLine("[SYSTEM] AutoRestarter: {0}", sig);
				//Autorestarter just needs to be told not to restart
				exitSystem = true;
			}
			else
			{
				Console.WriteLine("[SYSTEM] ChildServer: {0}", sig);
				//Child server needs to shutdown properly
				if (server != null)
				{
					server.Exit();
				}
				Console.WriteLine("[SYSTEM] ChildServer: Exit() called");
			}
			return true;
		}
		#endregion

		static void Main(string[] args)
		{
			//React to close window event, CTRL-C, kill, etc
			_handler += new CloseEventHandler(Handler);
			if (!IsMono)
			{
				SetConsoleCtrlHandler(_handler, true);
			}

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
		static bool IsAutoRestarter = true;
		static Server server;
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
			server = new Server();
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
						//Shutdown the server if parent process could not be found
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
			//Allow this number of restarts in a certain interval. Prevents endless restart-loops
			int restartLimit_Count = 5;
			int restartLimit_Minutes = 10;

			int restartCount = 0;
			Stopwatch restartGuard = new Stopwatch();
			restartGuard.Start();
			Restart();
			for (; ; )
			{
				//server terminated
				if (exitSystem)
				{
					break;
				}
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
					if (restartCount < restartLimit_Count)
					{
						//Consider this a normal number of restarts
						Console.WriteLine("[SERVER] Will restart");
						restartCount++;
						Restart();
					}
					else
					{
						//Stop restarting automatically to prevent extreme load on host system
						Console.WriteLine("[SERVER] Attention! The server tried to restart more often than usual.");
						Console.WriteLine("[SERVER] Please check for errors.");
						Console.WriteLine("[SERVER] Do you want to continue trying? (Y/N)");
						if (string.Equals(Console.ReadLine(), "Y", StringComparison.CurrentCultureIgnoreCase))
						{
							//Continue trying, reset counter
							restartCount = 0;
							Restart();
						}
						else
						{
							//Quit server
							Console.WriteLine("[SERVER] Exiting...");
							return;
						}
					}
				}
				if (restartGuard.Elapsed.TotalMinutes >= restartLimit_Minutes)
				{
					//One interval elapsed, reset restart counter
					restartCount = 0;
					restartGuard.Reset();
					restartGuard.Start();
				}
				Thread.Sleep(1);
			}
		}

		public static bool IsMono = Type.GetType("Mono.Runtime") != null;

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

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using ManicDigger;
using ManicDigger.Renderers;
using System.Text;
using System.Net.Sockets;
using ManicDigger.ClientNative;
using OpenTK.Graphics;
#endregion

public class ManicDiggerProgram
{
	[STAThread]
	public static void Main(string[] args)
	{
		#if !DEBUG
		//Catch unhandled exceptions
		CrashReporter.DefaultFileName = "ManicDiggerClientCrash.txt";
		CrashReporter.EnableGlobalExceptionHandling(false);
		#endif

		new ManicDiggerProgram(args);
	}

	public ManicDiggerProgram(string[] args)
	{
		dummyNetwork = new DummyNetwork();
		dummyNetwork.Start(new MonitorObject(), new MonitorObject());

		#if !DEBUG
		crashreporter = new CrashReporter();
		crashreporter.Start(delegate { Start(args); });
		#else
		Start(args);
		#endif
	}

	CrashReporter crashreporter;

	private void Start(string[] args)
	{
		string appPath = Path.GetDirectoryName(Application.ExecutablePath);
		if (!Debugger.IsAttached)
		{
			System.Environment.CurrentDirectory = appPath;
		}

		MainMenu mainmenu = new MainMenu();
		GamePlatformNative platform = new GamePlatformNative();
		platform.SetExit(exit);
		platform.crashreporter = crashreporter;
		platform.singlePlayerServerDummyNetwork = dummyNetwork;
		this.platform = platform;
		platform.StartSinglePlayerServer = (filename) => { savefilename = filename; new Thread(ServerThreadStart).Start(); };
		GraphicsMode mode = new GraphicsMode(OpenTK.DisplayDevice.Default.BitsPerPixel, 24);
		using (GameWindowNative game = new GameWindowNative(mode))
		{
			game.VSync = OpenTK.VSyncMode.Adaptive;
			platform.window = game;
			game.platform = platform;
			mainmenu.Start(platform);
			ReadArgs(mainmenu, args);
			platform.Start();
			game.Run();
		}
	}

	void ReadArgs(MainMenu mainmenu, string[] args)
	{
		if (args.Length > 0)
		{
			ConnectData connectdata = new ConnectData();
			connectdata = ConnectData.FromUri(new GamePlatformNative().ParseUri(args[0]));

			mainmenu.StartGame(false, null, connectdata);
		}
	}

	DummyNetwork dummyNetwork;
	string savefilename;
	public GameExit exit = new GameExit();
	GamePlatformNative platform;

	public void ServerThreadStart()
	{
		try
		{
			Server server = new Server();
			server.SaveFilenameOverride = savefilename;
			server.exit = exit;
			DummyNetServer netServer = new DummyNetServer();
			netServer.SetPlatform(new GamePlatformNative());
			netServer.SetNetwork(dummyNetwork);
			server.mainSockets = new NetServer[3];
			server.mainSockets[0] = netServer;
			for (; ; )
			{
				server.Process();
				Thread.Sleep(1);
				platform.singlePlayerServerLoaded = true;
				if (exit != null && exit.GetExit()) { server.Stop(); break; }
				if (platform.singlepLayerServerExit)
				{
					// Exit thread and reset shutdown variable
					server.Exit();
					platform.singlepLayerServerExit = false;
				}
			}
			exit.SetExit(false);
		}
		catch (Exception e)
		{
			MessageBox.Show(e.ToString());
		}
	}
}

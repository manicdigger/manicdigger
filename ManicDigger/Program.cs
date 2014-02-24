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
using ManicDigger.Network;
using ManicDigger.Renderers;
using ManicDiggerServer;
using System.Text;
using ManicDigger.Hud;
using System.Net.Sockets;
using Lidgren.Network;
#endregion

namespace GameModeFortress
{
    public class ManicDiggerProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new ManicDiggerProgram(args);
        }
        public ManicDiggerProgram(string[] args)
        {
            crashreporter = new CrashReporter();
            crashreporter.Start(delegate { Start(args); });
        }
        CrashReporter crashreporter;
        private void Start(string[] args)
        {
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            if (!Debugger.IsAttached)
            {
                System.Environment.CurrentDirectory = appPath;
            }
            bool IsSinglePlayer;
            string singleplayerpath;
            ConnectData connectdata = new ConnectData();
            if (args.Length > 0)
            {
                if (args[0].EndsWith(".mdlink", StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlDocument d = new XmlDocument();
                    d.Load(args[0]);
                    string mode = XmlTool.XmlVal(d, "/ManicDiggerLink/GameMode");
                    if (mode != "Fortress")
                    {
                        throw new Exception("Invalid game mode: " + mode);
                    }
                    connectdata.Ip = XmlTool.XmlVal(d, "/ManicDiggerLink/Ip");
                    int port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerLink/Port"));
                    connectdata.Port = port;
                    connectdata.Username = XmlTool.XmlVal(d, "/ManicDiggerLink/User");
                    connectdata.IsServePasswordProtected = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerLink/PasswordProtected"));
                    IsSinglePlayer = false;
                    singleplayerpath = null;
                }
                else
                {
                    connectdata = ConnectData.FromUri(new MyUri(args[0]));
                    IsSinglePlayer = false;
                    singleplayerpath = null;
                }
            }
            else
            {
                try
                {
                    if (File.Exists("cito.txt"))
                    {
                        MainMenu mainmenu = new MainMenu();
                        GamePlatformNative platform = new GamePlatformNative();
                        OpenTK.Graphics.GraphicsMode mode = new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 24, 0, 2, new OpenTK.Graphics.ColorFormat(32));
                        using (GameWindowNative game = new GameWindowNative(mode))
                        {
                            platform.window = game;
                            game.platform = platform;
                            mainmenu.Start(platform);
                            platform.Start();
                            //g.Start();
                            game.Run(60.0);
                        }
                    }
                }
                catch
                {
                }
                //new Thread(ServerThreadStart).Start();
                //p.GameUrl = "127.0.0.1:25570";
                //p.User = "Local";
                Menu form = new Menu();
                Application.Run(form);
                if (form.Chosen == ChosenGameType.None)
                {
                    return;
                }
                IsSinglePlayer = form.Chosen == ChosenGameType.Singleplayer;
                if (IsSinglePlayer)
                {
                    singleplayerpath = form.SinglePlayerSaveGamePath;
                    connectdata.IsServePasswordProtected = false;
                }
                else
                {
                    connectdata = form.MultiplayerConnectData;
                    singleplayerpath = null;
                }
            }
            savefilename = singleplayerpath;

            string serverPassword = "";
            if(connectdata.IsServePasswordProtected)
            {
                PasswordForm passwordForm = new PasswordForm();
                DialogResult dialogResult = passwordForm.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    serverPassword = passwordForm.Password;
                }
                if (dialogResult == DialogResult.Cancel)
                {
                    // TODO: go back to main menu
                    throw new Exception();
                }
            }
            connectdata.ServerPassword = serverPassword;
            StartGameWindowAndConnect(IsSinglePlayer, connectdata, singleplayerpath);
        }

        void StartGameWindowAndConnect(bool issingleplayer, ConnectData connectdata, string singleplayersavepath)
        {
            if (issingleplayer)
            {
                new Thread(ServerThreadStart).Start();
                connectdata.Username = "Local";
            }
        restart:
            ManicDiggerGameWindow w = new ManicDiggerGameWindow();
            w.issingleplayer = issingleplayer;
            this.curw = w;
            if (issingleplayer)
            {
                w.main = new DummyNetClient() { network = this.dummyNetwork };
            }
            else
            {
                var config = new NetPeerConfiguration("ManicDigger");
                //w.main = new MyNetClient() { client = new NetClient(config) };
                //w.main = new TcpNetClient() { };
                w.main = new EnetNetClient();
            }
            var glwindow = new GlWindow(w);
            w.d_GlWindow = glwindow;
            w.d_Exit = exit;
            w.connectdata = connectdata;
            w.crashreporter = crashreporter;
            w.Start();
            w.Run();
            if (w.reconnect)
            {
                goto restart;
            }
            exit.exit = true;
        }
        ManicDiggerGameWindow curw;

        DummyNetwork dummyNetwork = new DummyNetwork();

        string savefilename;
        public IGameExit exit = new GameExitDummy();
        //bool StartedSinglePlayerServer = false;
        public void ServerThreadStart()
        {
            try
            {
                Server server = new Server();
                server.SaveFilenameOverride = savefilename;
                server.exit = exit;
                var socket = new DummyNetServer() { network = dummyNetwork };
                server.d_MainSocket = socket;
                server.Start();
                for (; ; )
                {
                    server.Process();
                    Thread.Sleep(1);
                    while (curw == null)
                    {
                        Thread.Sleep(1);
                    }
                    curw.StartedSinglePlayerServer = true;
                    if (exit != null && exit.exit) { server.SaveAll(); return; }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}

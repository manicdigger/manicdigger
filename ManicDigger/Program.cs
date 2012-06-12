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
using ManicDigger.MapTools;
using ManicDigger.MapTools.Generators;
using ManicDigger.Network;
using ManicDigger.Renderers;
using ManicDiggerServer;
using System.Text;
using ManicDigger.Hud;
using System.Net.Sockets;
#endregion

namespace GameModeFortress
{
    public interface IResetMap
    {
        void Reset(int sizex, int sizey, int sizez);
    }
    public class ManicDiggerProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new ManicDiggerProgram(args);
        }
        public ManicDiggerProgram(string[] args)
        {
            new CrashReporter().Start(delegate { Start(args); });
        }
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
                }
                else
                {
                    connectdata = form.MultiplayerConnectData;
                    singleplayerpath = null;
                }
            }
            savefilename = singleplayerpath;
            StartGameWindowAndConnect(IsSinglePlayer, connectdata, singleplayerpath);
        }

        void StartGameWindowAndConnect(bool issingleplayer, ConnectData connectdata, string singleplayersavepath)
        {
            if (issingleplayer)
            {
                new Thread(ServerThreadStart).Start();
                connectdata.Username = "Local";
            }
            while (issingleplayer && !StartedSinglePlayerServer)
            {
                Thread.Sleep(1);
            }
            ManicDiggerGameWindow w = new ManicDiggerGameWindow();
            if (issingleplayer)
            {
                var socket = new SocketDummy() { network = this.dummyNetwork };
                w.main = socket;
            }
            else
            {
                w.main = new SocketNet(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            }
            var glwindow = new GlWindow(w);
            w.d_GlWindow = glwindow;
            w.d_Exit = exit;
            w.connectdata = connectdata;
            w.Start();
            w.Run();
            exit.exit = true;
        }

        SocketDummyNetwork dummyNetwork = new SocketDummyNetwork();

        string savefilename;
        public IGameExit exit = new GameExitDummy();
        bool StartedSinglePlayerServer = false;
        public void ServerThreadStart()
        {
            try
            {
                Server server = new Server();
                server.SaveFilenameOverride = savefilename;
                server.exit = exit;
                var socket = new SocketDummy(dummyNetwork);
                server.d_MainSocket = socket;
                server.Start();
                for (; ; )
                {
                    server.Process();
                    Thread.Sleep(1);
                    StartedSinglePlayerServer = true;
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

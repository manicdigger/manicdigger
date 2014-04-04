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
using ManicDiggerServer;
using System.Text;
using System.Net.Sockets;
using Lidgren.Network;
using ManicDigger.ClientNative;
using OpenTK.Graphics;
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
            dummyNetwork = new DummyNetwork();
            dummyNetwork.Start(new MonitorObject(), new MonitorObject());
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

            bool MenuResultSinglePlayer = false;
            ConnectData MenuResultMenuConnectData = null;
            string MenuResultSavegamePath = null;

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
                    connectdata.SetIp(XmlTool.XmlVal(d, "/ManicDiggerLink/Ip"));
                    int port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerLink/Port"));
                    connectdata.SetPort(port);
                    connectdata.SetUsername(XmlTool.XmlVal(d, "/ManicDiggerLink/User"));
                    connectdata.SetIsServePasswordProtected(Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerLink/PasswordProtected")));
                    IsSinglePlayer = false;
                    singleplayerpath = null;
                }
                else
                {
                    connectdata = ConnectData.FromUri(new GamePlatformNative().ParseUri(args[0]));
                    IsSinglePlayer = false;
                    singleplayerpath = null;
                }
            }
            else
            {
                MainMenu mainmenu = new MainMenu();
                GamePlatformNative platform = new GamePlatformNative();
                platform.SetExit(exit);
                platform.crashreporter = crashreporter;
                platform.singlePlayerServerDummyNetwork = dummyNetwork;
                this.platform = platform;
                platform.StartSinglePlayerServer = (filename) => { savefilename = filename; new Thread(ServerThreadStart).Start(); };
                GraphicsMode mode = GraphicsMode.Default;
                using (GameWindowNative game = new GameWindowNative(mode))
                {
                    platform.window = game;
                    game.platform = platform;
                    mainmenu.Start(platform);
                    platform.Start();
                    //g.Start();
                    game.Run();
                }
                MenuResultSinglePlayer = mainmenu.GetMenuResultSinglePlayer();
                MenuResultMenuConnectData = mainmenu.GetMenuResultMenuConnectData();
                MenuResultSavegamePath = mainmenu.GetMenuResultSavegamePath();

                if (MenuResultSinglePlayer)
                {
                    if (MenuResultSavegamePath == null)
                    {
                        return;
                    }
                    singleplayerpath = MenuResultSavegamePath;
                    connectdata.SetIsServePasswordProtected(false);
                }
                else
                {
                    if (MenuResultMenuConnectData == null)
                    {
                        return;
                    }
                    connectdata = MenuResultMenuConnectData;
                    singleplayerpath = null;
                }
            }
            savefilename = singleplayerpath;

            string serverPassword = "";
            //if (connectdata.GetIsServePasswordProtected())
            //{
            //    PasswordForm passwordForm = new PasswordForm();
            //    DialogResult dialogResult = passwordForm.ShowDialog();

            //    if (dialogResult == DialogResult.OK)
            //    {
            //        serverPassword = passwordForm.Password;
            //    }
            //    if (dialogResult == DialogResult.Cancel)
            //    {
            //        // TODO: go back to main menu
            //        throw new Exception();
            //    }
            //}
            //connectdata.SetServerPassword(serverPassword);
            //StartGameWindowAndConnect(MenuResultSinglePlayer, connectdata, singleplayerpath);
        }

        DummyNetwork dummyNetwork;

        string savefilename;
        public GameExit exit = new GameExit();
        //bool StartedSinglePlayerServer = false;
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
                server.mainSocket0 = netServer;
                server.Start();
                for (; ; )
                {
                    server.Process();
                    Thread.Sleep(1);
                    //while (curw == null)
                    //{
                    //    Thread.Sleep(1);
                    //}
                    platform.singlePlayerServerLoaded = true;
                    if (exit != null && exit.GetExit()) { server.SaveAll(); return; }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}

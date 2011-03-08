using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;

namespace GameMenu
{
    public class FormJoinMultiplayer : IForm
    {
        public MenuWindow menu;
        public Game game;
        public void Initialize()
        {
            if (game.LoginName == "Local")
            {
                game.LoginName = "gamer";
            }
            InitializeWidgets();
            StartRefreshing();
        }
        private void InitializeWidgets()
        {
            widgets.Clear();
            menu.AddBackground(widgets);
            int[] columnWidths = new int[] { 600, 130, 100, 250, 200, 100 };
            AddListboxRow(new[] { "Name", "Players", "Max", "Ip", "Version", "Names" }, 50, 300, columnWidths, -1);
            if (servers != null)
            {
                for (int i = 0; i < servers.Length; i++)
                {
                    var s = servers[i];
                    AddListboxRow(new string[] { s.Name, s.Users.ToString(), s.Max.ToString(), s.Ip, s.Version, s.Players },
                        50, 300 + (i + 1) * 50, columnWidths, i);
                }
            }
            menu.AddCaption(this, "Multiplayer");
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1050, 50, 400, 128),
                Text = "Playing as: " + (game.IsLoggedIn ? game.LoginName : "~" + game.LoginName),
                Click = delegate { },
                selected = false,
                FontSize = 24,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(100, 150, 300, 128),
                Text = "Refresh",
                Click = delegate { serverlisterror = false; StartRefreshing(); },
                selected = false,
                FontSize = 24,
            });
            refreshingLabel = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(400, 150, 300, 128),
                Text = "Refreshing...",
                Click = delegate { StartRefreshing(); },
                selected = false,
                FontSize = 24,
            };
            widgets.Add(refreshingLabel);
            serverListErrorWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(400, 150, 300, 128),
                Text = "Can't connect to server list.",
                Click = delegate { StartRefreshing(); },
                selected = false,
                FontSize = 24,
                Visible = serverlisterror,
                TextColor = Color.Red,
            };
            widgets.Add(serverListErrorWidget);            
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(1100, 150, 300, 128),
                Text = "Login",
                Click = delegate { menu.FormLogin(); },
                selected = false,
                FontSize = 24,
            });


            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(200, 1000, 400, 128),
                Text = "Connect",
                Click = delegate { game.JoinMultiplayer(servers[selectedServer].Ip, servers[selectedServer].Port); },
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(600, 1000, 400, 128),
                Text = "Connect to IP",
                Click = menu.FormConnectToIp,
            });

            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(1000, 1000, 400, 128),
                Text = "Cancel",
                Click = menu.FormMainMenu,
            });
        }
        Widget refreshingLabel;
        Widget serverListErrorWidget;
        public void StartRefreshing()
        {
            if (!refreshing)
            {
                refreshing = true;
                new Thread(refreshjoin).Start();
            }
        }
        public void Render()
        {
            InitializeWidgets();
            refreshingLabel.Visible = refreshing;
            //if (servers_refresh)
            //{
            //    InitializeWidgets();
            //    servers_refresh = false;
            //}
        }
        bool serverlisterror = false;
        int selectedServer = 0;
        List<Widget> serverlistitems = new List<Widget>();
        private void AddListboxRow(string[] text, int x, int y, int[] columnwidths, int id)
        {
            serverlistitems.Clear();
            for (int i = 0; i < text.Length; i++)
            {
                int id2 = id; //closure
                var b = new Widget()
                {
                    BackgroundImage = null,
                    BackgroundImageSelected = null,
                    Rect = new RectangleF(x, y, 400, 128),
                    Text = text[i],
                    Click = delegate { if (id2 != -1) { selectedServer = id2; } },
                    selected = selectedServer == id,
                    FontSize = 20,
                };
                serverlistitems.Add(b);
                Widgets.Add(b);
                x += columnwidths[i];
            }
        }
        ServerInfo[] servers;
        bool refreshing = false;
        void refreshjoin()
        {
            var newservers = game.GetServers();
            serverlisterror = newservers == null;
            servers = newservers;
            refreshing = false;
            //FormJoinMultiplayer();
        }
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

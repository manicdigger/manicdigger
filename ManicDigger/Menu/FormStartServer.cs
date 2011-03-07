using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameMenu
{
    public partial class MenuWindow
    {
        public void FormStartMultiplayerServer()
        {
            /*
            widgets.Clear();
            AddBackground();
            AddCaption("Start server");
            widgets.Add(new Button()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 200, 400, 90),
                Text = "World: " + (selectedWorld == null ? "none" : game.GetWorlds()[selectedWorld.Value]),
                Click = delegate { afterSelectWorld = FormStartMultiplayerServer; FormSelectWorld(); },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(500, 200, 300, 90),
                Text = "Select",
                Click = delegate { afterSelectWorld = FormStartMultiplayerServer; FormSelectWorld(); },
                FontSize = 20,
            });

            //Connection options
            widgets.Add(new Button()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 300, 400, 90),
                Text = "Server name: ",
                Click = delegate { },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(500, 300, 600, 90),
                Text = typingfield == 0 ? typingbuffer : servername,
                Click = delegate
                {
                    typingfield = 0;
                    typingbuffer = servername;
                    OnFinishedTyping = delegate { servername = typingbuffer; };
                    FormStartMultiplayerServer();
                },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 400, 400, 90),
                Text = "Max players: ",
                Click = delegate { },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(500, 400, 600, 90),
                Text = typingfield == 99 ? typingbuffer : maxplayers.ToString(),
                Click = delegate
                {
                    typingfield = 99;
                    typingbuffer = maxplayers.ToString();
                    OnFinishedTyping = delegate { try { maxplayers = int.Parse(typingbuffer); } catch { maxplayers = 16; } };
                    FormStartMultiplayerServer();
                },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200,500, 400, 90),
                Text = "Password: ",
                Click = delegate {  },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(500, 500, 600, 90),
                Text = typingfield == 1 ? PassString(typingbuffer) : PassString(serverpassword),
                Click = delegate
                {
                    typingfield = 1;
                    typingbuffer = serverpassword;
                    OnFinishedTyping = delegate { serverpassword = typingbuffer; };
                    FormStartMultiplayerServer();
                },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 600, 400, 90),
                Text = "Public: ",
                Click = delegate {  },
                FontSize = 20,
            });
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(500, 600, 200, 90),
                Text = serverpublic ? "Yes" : "No",
                Click = delegate { serverpublic = !serverpublic; FormStartMultiplayerServer(); }, //todo textbox
                FontSize = 20,
            });



            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(1200, 200, 300, 90),
                Text = "Show my IP",
                Click = delegate { },
                FontSize = 20,
            });

            //Bottom buttons
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(200, 1000, 400, 128),
                Text = serverstarted ? "Stop" : "Start",
                //Click = delegate { serverstarted = !serverstarted; FormStartMultiplayerServer(); },
                Click = delegate { typingfield = -1; FormGame(); }
            });
            //widgets.Add(new Button()
            //{
            //    BackgroundImage = "button4.png",
            //    BackgroundImageSelected = "button4_sel.png",
            //    Rect = new RectangleF(600, 1000, 400, 128),
            //    Text = "Play",
            //    Click = delegate { FormGame(); },
            //});
            widgets.Add(new Button()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(1000, 1000, 400, 128),
                Text = "Cancel",
                Click = delegate { typingfield = -1; FormMainMenu(); }
            });
            */
        }
        string servername = "My server";
        int maxplayers = 16;
        bool serverpublic = true;
        string serverpassword = "***";
        bool serverstarted;
    }
}

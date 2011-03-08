using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameMenu
{
    public class FormStartServer : IForm
    {
        public MenuWindow menu;
        public Game game;
        Widget selectedWorldWidget;
        public void Initialize()
        {
            widgets.Clear();
            menu.AddBackground(widgets);
            menu.AddCaption(this, "Start server");
            selectedWorldWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 200, 400, 90),
                Text = "",//Render()
                Click = delegate { menu.FormSelectWorld(menu.FormStartServer); },
                FontSize = 20,
            };
            widgets.Add(selectedWorldWidget);
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 200, 300, 90),
                Text = "Select",
                Click = delegate { menu.FormSelectWorld(delegate { menu.FormStartServer(); }); },
                FontSize = 20,
            });

            //Connection options
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 300, 400, 90),
                Text = "Server name: ",
                Click = delegate { },
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 300, 600, 90),
                IsTextbox = true,
                FontSize = 20,
                Text = "My server",
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 400, 400, 90),
                Text = "Max players: ",
                Click = delegate { },
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 400, 600, 90),
                IsTextbox = true,
                IsNumeric = true,
                FontSize = 20,
                Text = "16",
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 500, 400, 90),
                Text = "Password: ",
                Click = delegate { },
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 500, 600, 90),
                IsTextbox = true,
                IsPassword = true,
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 600, 400, 90),
                Text = "Public: ",
                Click = delegate { },
                FontSize = 20,
            });
            publicWidget = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 600, 200, 90),
                Text = "",//Render()
                Click = delegate { serverpublic = !serverpublic; },
                FontSize = 20,
            };
            widgets.Add(publicWidget);

            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(1200, 200, 300, 90),
                Text = "Show my IP",
                Click = delegate { },
                FontSize = 20,
            });

            //Bottom buttons
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(200, 1000, 400, 128),
                Text = serverstarted ? "Stop" : "Start",
                //Click = delegate { serverstarted = !serverstarted; FormStartMultiplayerServer(); },
                Click = delegate { game.StartAndJoinLocalServer(menu.formSelectWorld.selectedWorld.Value); }
            });
            //widgets.Add(new Button()
            //{
            //    BackgroundImage = "button4.png",
            //    BackgroundImageSelected = "button4_sel.png",
            //    Rect = new RectangleF(600, 1000, 400, 128),
            //    Text = "Play",
            //    Click = delegate { FormGame(); },
            //});
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(1000, 1000, 400, 128),
                Text = "Cancel",
                Click = delegate { menu.FormMainMenu(); }
            });
        }
        public void Render()
        {
            string worldname = (menu.formSelectWorld.selectedWorld == null
                ? "none" : game.GetWorlds()[menu.formSelectWorld.selectedWorld.Value]);
            if (string.IsNullOrEmpty(worldname))
            {
                worldname = "none";
            }
            selectedWorldWidget.Text = "World: " + worldname;

            publicWidget.Text = serverpublic ? "Yes" : "No";
        }
        Widget publicWidget;
        string servername = "My server";
        int maxplayers = 16;
        bool serverpublic = true;
        string serverpassword = "***";
        bool serverstarted;
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

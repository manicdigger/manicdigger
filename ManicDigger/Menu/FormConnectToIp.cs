using System;
using System.Collections.Generic;
using System.Text;
using GameMenu;
using System.Drawing;

namespace ManicDigger.Menu
{
    public class FormConnectToIp : IForm
    {
        public MenuWindow menu;
        public Game game;
        public void Initialize()
        {
            widgets.Clear();
            menu.AddBackground(this.widgets);
            menu.AddCaption(this, "Connect to IP");
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 300, 400, 90),
                Text = "IP: ",
                Click = delegate { },
                FontSize = 20,
            });
            ipTextboxWidget = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 300, 600, 90),
                Text = "127.0.0.1",
                Click = delegate { },
                FontSize = 20,
                IsTextbox = true,
            };
            widgets.Add(ipTextboxWidget);
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 400, 400, 90),
                Text = "Port: ",
                Click = delegate { },
                FontSize = 20,
            });
            var portTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 400, 600, 90),
                Text = "25565",
                Click = delegate { },
                FontSize = 20,
                IsTextbox = true,
                IsNumeric = true,
            };
            widgets.Add(portTextbox);
            invalidHostWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1200, 300, 400, 90),
                Text = "Invalid address.",
                Click = delegate { },
                FontSize = 20,
                TextColor = Color.Red,
            };
            widgets.Add(invalidHostWidget);

            menu.AddOkCancel(this, delegate { game.JoinMultiplayer(ipTextboxWidget.Text, int.Parse(portTextbox.Text)); }, delegate { menu.currentForm = menu.formJoinMultiplayer; });
        }
        Widget ipTextboxWidget;
        Widget invalidHostWidget;
        public void Render()
        {
            invalidHostWidget.Visible = Uri.CheckHostName(ipTextboxWidget.Text) == UriHostNameType.Unknown;
        }
        public int worldId;
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

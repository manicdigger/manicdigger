using System;
using System.Collections.Generic;
using System.Text;
using GameMenu;
using System.Drawing;
using System.Windows.Forms;

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
                Text = "Hash/Url: ",
                Click = delegate { },
                FontSize = 20,
            }); 
            hashTextboxWidget = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 300, 650, 90),
                Text = "",
                Click = delegate { },
                FontSize = 20,
                IsTextbox = true,
            };

            widgets.Add(hashTextboxWidget);
            /*
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 450, 400, 90),
                Text = "Or",
                Click = delegate { },
                FontSize = 20,
            });
            */
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 600, 400, 90),
                Text = "IP: ",
                Click = delegate { },
                FontSize = 20,
            });
            ipTextboxWidget = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 600, 600, 90),
                Text = "",
                Click = delegate { },
                FontSize = 20,
                IsTextbox = true,
            };
            widgets.Add(ipTextboxWidget);
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 700, 400, 90),
                Text = "Port: ",
                Click = delegate { },
                FontSize = 20,
            });
            portTextboxWidget = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 700, 600, 90),
                Text = "25565",
                Click = delegate { },
                FontSize = 20,
                IsTextbox = true,
                IsNumeric = true,
            };
            widgets.Add(portTextboxWidget);
            invalidHashWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1200, 300, 400, 90),
                Text = "Invalid address.",
                Click = delegate { },
                FontSize = 20,
                TextColor = Color.Red,
            };
            widgets.Add(invalidHashWidget);
            invalidHostWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1200, 600, 400, 90),
                Text = "Invalid address.",
                Click = delegate { },
                FontSize = 20,
                TextColor = Color.Red,
            };
            widgets.Add(invalidHostWidget);

            menu.AddOkCancel(this, delegate { Connect(); }, delegate { menu.currentForm = menu.d_FormJoinMultiplayer; });
        }
        void Connect()
        {
            if (hashTextboxWidget.Text != "")
            {
                game.JoinMultiplayer(hashTextboxWidget.Text);
            }
            else
            {
                game.JoinMultiplayer(ipTextboxWidget.Text, int.Parse(portTextboxWidget.Text));
            }
        }
        Widget hashTextboxWidget;
        Widget ipTextboxWidget;
        Widget portTextboxWidget;
        Widget invalidHashWidget;
        Widget invalidHostWidget;
        public string hashPrefix = "server=";
        public int HashLength = 32;
        DateTime lastclipboard;
        public void Render()
        {
            //todo save ip and hash
            if ((DateTime.UtcNow - lastclipboard).TotalSeconds > 1)
            {
                if (IsValidHash(GetHash(Clipboard.GetText())))
                {
                    hashTextboxWidget.Text = Clipboard.GetText();
                }
                lastclipboard = DateTime.UtcNow;
            }
            string hash = hashTextboxWidget.Text;
            try
            {
                bool valid = IsValidHash(GetHash(hash));
                if (valid)
                {
                    //trim url so it fits in textbox.
                    hashTextboxWidget.Text = GetHash(hash);
                }
                invalidHashWidget.Visible = !valid && hash != "";
            }
            catch
            {
                invalidHashWidget.Visible = true;
            }
            invalidHostWidget.Visible = Uri.CheckHostName(ipTextboxWidget.Text) == UriHostNameType.Unknown
                && ipTextboxWidget.Text != "";
        }
        string GetHash(string hash)
        {
            try
            {
                if (hash.Contains(hashPrefix))
                {
                    hash = hash.Substring(hash.IndexOf(hashPrefix) + hashPrefix.Length);
                }
            }
            catch
            {
                return null;
            }
            return hash;
        }
        private bool IsValidHash(string hash)
        {
            if (hash.Length != HashLength)
            {
                return false;
            }
            for (int i = 0; i < hash.Length; i++)
            {
                char c = hash[i];
                if (char.IsDigit(c)) { continue; }
                if (c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f')
                {
                    continue;
                }
                return false;
            }
            return true;
        }
        public int worldId;
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GameMenu;
using System.Drawing;
using System.Windows.Forms;
using ManicDigger.Menu;
using System.IO;

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
                Text = gethashurl(),
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
                Text = getip(),
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
                Text = getport(),
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
		public static string GetStorePath()
        {
            string apppath = Path.GetDirectoryName(Application.ExecutablePath);
            string mdfolder = "ManicDiggerUserData";
            if (apppath.Contains(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
            {
                string mdpath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    mdfolder);
                return mdpath;
            }
            else
            {
                //return Path.Combine(apppath, mdfolder);
                return mdfolder;
            }
        }
        public static string GetIpsaveFilePath()
        {
            string path = GameStorePath.GetStorePath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, "lastip.txt");
        }
		void createipsavefile()
		{
			StringBuilder f = new StringBuilder();
			f.AppendLine("");
			f.AppendLine("");
			f.AppendLine("25565");	// Write port number
			File.WriteAllText(GetIpsaveFilePath(), f.ToString());
		}
		void Saveip()
		{
			// delete history!
			File.Delete(GetIpsaveFilePath());			
			
			StringBuilder b = new StringBuilder();	
			b.AppendLine(hashTextboxWidget.Text);
			b.AppendLine(ipTextboxWidget.Text);
			b.AppendLine(portTextboxWidget.Text);
			File.WriteAllText(GetIpsaveFilePath(), b.ToString());
		}
		public string gethashurl()
		{
			string filename = GetIpsaveFilePath();
			if (File.Exists(filename)) {
				string[] ipsavecontent = File.ReadAllLines(filename);
				return ipsavecontent[0];
			} else {
				createipsavefile();
				return "";
			}
		}
		public string getip()
		{
			string filename = GetIpsaveFilePath();
			if (File.Exists(filename)) {
				string[] ipsavecontent = File.ReadAllLines(filename);
				return ipsavecontent[1];
			} else {
				createipsavefile();
				return "";
			}
		}
		public string getport()
		{
			string filename = GetIpsaveFilePath();
			if (File.Exists(filename)) {
				string[] ipsavecontent = File.ReadAllLines(filename);
				if(ipsavecontent[2] == "") {	// If someone deleted standard port, restore it!
					return "25565";
				} else {
					return ipsavecontent[2];	
				}
			} else {
				createipsavefile();
				return "25565";
			}
		}	
        void Connect()
        {
			Saveip();
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace ManicDigger
{
    public partial class ServerSelector : Form
    {
        public ServerSelector()
        {
            InitializeComponent();
        }
        private void ServerSelector_Load(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri("http://fragmer.net/md/");
            webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(webBrowser1_Navigating);
            webBrowser2.Navigating += new WebBrowserNavigatingEventHandler(webBrowser2_Navigating);
            LoadPassword();
        }
        void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string prefix = "http://fragmer.net/md/play.php?server=";
            if (!e.Url.AbsoluteUri.StartsWith(prefix))
            {
                //e.Cancel = true;
                return;
            }
            SelectedServer = e.Url.AbsoluteUri.Substring(prefix.Length);
            SelectedServerMinecraft = false;
            Cookie = webBrowser1.Document.Cookie;
            Close();
        }
        void webBrowser2_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!e.Url.AbsoluteUri.StartsWith("http://minecraft.net/"))
            {
                return;
            }
            e.Cancel = true;
            SelectedServer = e.Url.AbsoluteUri;
            SelectedServerMinecraft = true;
            SetLoginData(SelectedServer);
            Close();
        }
        public string SelectedServer = null;
        public bool SelectedServerMinecraft = false;
        public string Cookie;
        public string SinglePlayer = null;
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void button2_Click(object sender, EventArgs e)
        {
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
        }
        private void button2_Click_2(object sender, EventArgs e)
        {
            SinglePlayer = "Mine";
            Close();
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }
        private void label2_Click(object sender, EventArgs e)
        {
        }
        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            SinglePlayer = "Fortress";
            Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            LoginClientMinecraft c = new LoginClientMinecraft();
            c.Progress += new EventHandler<ProgressEventArgs>(c_Progress);
            if (textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Enter username and password.");
                return;
            }
            var l = c.ServerList(textBox2.Text, textBox3.Text);
            if (l.Count == 0)
            {
                MessageBox.Show("Problem. (invalid username or password?)");
                return;
            }
            StringBuilder html = new StringBuilder();
            for (int i = 0; i < l.Count; i++)
            {
                var item = new ListViewItem();
                item.Text = l[i].Name;
                item.SubItems.Add(l[i].Players.ToString());
                item.SubItems.Add(l[i].PlayersMax.ToString());
                html.AppendLine(string.Format("<a href=\"{0}\"><b>{1}</b></a> {2}/{3} <br>",
                    l[i].Url, l[i].Name, l[i].Players, l[i].PlayersMax));
            }
            webBrowser2.DocumentText = html.ToString();
            if (checkBox1.Checked)
            {
                RememberPassword(textBox2.Text, textBox3.Text);
            }
        }
        void c_Progress(object sender, ProgressEventArgs e)
        {
            progressBar1.Value = e.ProgressPercent;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Enter username and password.");
                return;
            }
            if (textBox4.Text == "")
            {
                MessageBox.Show("Invalid server address.");
                return;
            }
            SelectedServer = textBox4.Text;
            SelectedServerMinecraft = true;
            SetLoginData(textBox4.Text);
            Close();
        }
        private void SetLoginData(string url)
        {
            LoginClientMinecraft c = new LoginClientMinecraft();
            LoginData logindata = c.Login(textBox2.Text, textBox3.Text, url);
            this.LoginIp = logindata.serveraddress;
            this.LoginPassword = logindata.mppass;
            this.LoginPort = logindata.port.ToString();
            this.LoginUser = textBox2.Text;
        }
        public string LoginIp;
        public string LoginPort;
        public string LoginUser;
        public string LoginPassword;
        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                string filename = GetMinecraftPasswordFilePath();
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            else
            {
                RememberPassword(textBox2.Text, textBox3.Text);
            }
        }
        private void RememberPassword(string user, string password)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(user);
            b.AppendLine(password);
            File.WriteAllText(GetMinecraftPasswordFilePath(), b.ToString());
        }
        void LoadPassword()
        {
            string filename = GetMinecraftPasswordFilePath();
            if (File.Exists(filename))
            {
                string[] lines = File.ReadAllLines(filename);
                textBox2.Text = lines[0];
                textBox3.Text = lines[1];
                checkBox1.Checked = true;
            }
        }
        private static string GetMinecraftPasswordFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MinecraftPassword.txt");
        }
    }
}
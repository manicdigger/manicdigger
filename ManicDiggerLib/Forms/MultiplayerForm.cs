using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.IO;

namespace ManicDigger
{
    public partial class MultiplayerForm : Form
    {
        public MultiplayerForm()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://manicdigger.sourceforge.net/play/");
        }

        private void MultiplayerForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadPassword();
            }
            catch
            {
            }
            listView1.Columns[0].Width = 200;
            listView1.Columns[1].Width = 300;
            listView1.Columns[2].Width = 100;
            try
            {
                servers = GetServers();
                for (int i = 0; i < servers.Length; i++)
                {
                    ListViewItem item = listView1.Items.Add(servers[i].Name);
                    item.SubItems.Add(servers[i].Players);
                    var ver = item.SubItems.Add(servers[i].Version);
                    /*
                    if (ver.Text != GameVersion.Version)
                    {
                        item.ForeColor = Color.Red;
                    }
                    */
                    item.Tag = i;
                }
            }
            catch
            {
                MessageBox.Show("Can't download server list.");
            }
        }
        ServerInfo[] servers;

        public class ServerInfo
        {
            public string Hash;
            public string Name;
            public string Motd;
            public int Port;
            public string Ip;
            public string Version;
            public int Users;
            public int Max;
            public string GameMode;
            public string Players;
        }

        public ServerInfo[] GetServers()
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false; // fixes lighthttpd 417 error in future connections
                WebClient c = new WebClient();
                string xml = c.DownloadString(ServerListAddress);
                XmlDocument d = new XmlDocument();
                d.LoadXml(xml);
                string[] allHash = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Hash")).ToArray();
                string[] allName = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Name")).ToArray();
                string[] allMotd = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/MOTD")).ToArray();
                string[] allPort = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Port")).ToArray();
                string[] allIp = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/IP")).ToArray();
                string[] allVersion = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Version")).ToArray();
                string[] allUsers = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Users")).ToArray();
                string[] allMax = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Max")).ToArray();
                string[] allGameMode = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/GameMode")).ToArray();
                string[] allPlayers = new List<string>(ManicDigger.XmlTool.XmlVals(d, "/ServerList/Server/Players")).ToArray();
                List<ServerInfo> l = new List<ServerInfo>();
                for (int i = 0; i < allHash.Length; i++)
                {
                    ServerInfo info = new ServerInfo();
                    info.Hash = allHash[i];
                    info.Name = allName[i];
                    info.Motd = allMotd[i];
                    info.Port = int.Parse(allPort[i]);
                    info.Ip = allIp[i];
                    info.Version = allVersion[i];
                    info.Users = int.Parse(allUsers[i]);
                    info.Max = int.Parse(allMax[i]);
                    info.GameMode = allGameMode[i];
                    info.Players = allPlayers[i];
                    l.Add(info);
                }
                return l.ToArray();
            }
            catch
            {
                return null;
            }
        }
        public string ServerListAddress = "http://fragmer.net/md/xml.php";

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                MessageBox.Show("Select server on list.");
                return;
            }
            var server = servers[(int)listView1.SelectedItems[0].Tag];
            LoginIp = server.Ip;
            LoginPort = server.Port.ToString();
            LoginData logindata = new LoginClientManicDigger().Login(LoginUser, LoginPassword, server.Hash);
            if (!logindata.PasswordCorrect)
            {
                MessageBox.Show("Invalid username or password");
                return;
            }
            LoginAuthcode = logindata.AuthCode;
            ConnectNow = true;
            Close();
        }
        public bool ConnectNow;
        public string LoginIp;
        public string LoginPort;
        public string LoginUser;
        public string LoginPassword;
        public string LoginAuthcode;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                string filename = GetPasswordFilePath();
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            else
            {
                RememberPassword(textBox1.Text, textBox2.Text);
            }
        }
        private void RememberPassword(string user, string password)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(user);
            b.AppendLine(password);
            File.WriteAllText(GetPasswordFilePath(), b.ToString());
        }
        void LoadPassword()
        {
            string filename = GetPasswordFilePath();
            if (File.Exists(filename))
            {
                string[] lines = File.ReadAllLines(filename);
                textBox1.Text = lines[0];
                textBox2.Text = lines[1];
                checkBox1.Checked = true;
            }
        }
        private static string GetPasswordFilePath()
        {
            string path = GameStorePath.GetStorePath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, "Password.txt");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            RememberPassword(textBox1.Text, textBox2.Text);
            SetLoginData();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            RememberPassword(textBox1.Text, textBox2.Text);
            SetLoginData();
        }

        private void SetLoginData()
        {
            this.LoginUser = textBox1.Text;
            this.LoginPassword = textBox2.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.LoginIp = textBox3.Text;
            if (LoginIp.Trim() == "")
            {
                MessageBox.Show("Invalid server IP.");
                return;
            }
            if (LoginIp.Contains(":"))
            {
                string[] ipport = LoginIp.Split(':');
                this.LoginIp = ipport[0];
                this.LoginPort = ipport[1];
            }
            else
            {
                this.LoginPort = "25565";
            }
            if (servers.Length > 0)
            {
                LoginData logindata = new LoginClientManicDigger().Login(LoginUser, LoginPassword, servers[0].Hash);
                if (!logindata.PasswordCorrect)
                {
                    MessageBox.Show("Invalid username or password");
                }
                else
                {
                    LoginAuthcode = logindata.AuthCode;
                }
            }
            ConnectNow = true;
            Close();
        }
    }
}

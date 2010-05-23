using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

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
            Cookie = webBrowser1.Document.Cookie;
            Close();
        }
        public string SelectedServer = null;
        public string Cookie;
        public string SinglePlayer = null;
        private void button1_Click(object sender, EventArgs e)
        {
            SinglePlayer = "Fortress";
            Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            SinglePlayer = "Mine";
            Close();
        }
    }
}
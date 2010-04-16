using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
            webBrowser1.Url = new Uri("http://list.fragmer.net/");
            webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(webBrowser1_Navigating);
        }
        void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!e.Url.AbsoluteUri.StartsWith("http://minecraft.net/play.jsp?server="))
            {
                e.Cancel = true;
                return;
            }
            Close();
            var p = new ManicDiggerProgram2();
            p.GameUrl = e.Url.AbsoluteUri;
            p.Start();
        }
    }
}
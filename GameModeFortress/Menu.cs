using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ManicDigger;
using System.IO;
using System.Diagnostics;

namespace GameModeFortress
{
    public enum ChosenGameType
    {
        None,
        Singleplayer,
        Multiplayer
    }
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }
        public ChosenGameType Chosen;
        public string SinglePlayerSaveGamePath;
        public ConnectData MultiplayerConnectData;

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dir = System.Environment.CurrentDirectory;
                var result = openFileDialog1.ShowDialog();
                System.Environment.CurrentDirectory=dir;
                if (result == DialogResult.OK)
                {
                    SinglePlayerSaveGamePath = openFileDialog1.FileName;
                    Chosen = ChosenGameType.Singleplayer;
                    Close();
                }
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Chosen = ChosenGameType.Multiplayer;
                MultiplayerConnectData = ConnectData.FromUri(new MyUri(textBox1.Text));
                Close();
            }
            catch
            {
                MessageBox.Show("Invalid sever address.");
            }
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            try
            {
                string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
                foreach (string s in datapaths)
                {
                    try
                    {
                        string ss = Path.Combine(s, "local");
                        string ss2 = Path.Combine(ss, "gui");
                        string ss3 = Path.Combine(ss2, "logo.png");
                        if (File.Exists(ss3))
                        {
                            pictureBox1.Image = new Bitmap(ss3);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://manicdigger.sourceforge.net/play/");
        }
    }
}

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
        string lastserverpath = Path.Combine(GameStorePath.GetStorePath(), "lastserver.txt");

        public ChosenGameType Chosen;
        public string SinglePlayerSaveGamePath;
        public ConnectData MultiplayerConnectData;
        private string button_hover_path;
        private string button_idle_path;
        bool buttonImagesExist = false;
        Bitmap button_hover_image;
        Bitmap button_idle_image;

        MultiplayerForm m = new MultiplayerForm();
        Bitmap bg;

        // Method useless? Maybe remove it.
        /*private void button1_Click(object sender, EventArgs e)
        {
            
            try
            {
                Chosen = ChosenGameType.Multiplayer;
                MultiplayerConnectData = ConnectData.FromUri(new MyUri(textBox1.Text));
                try
                {
                    File.WriteAllText(lastserverpath, textBox1.Text);
                }
                catch
                {
                }
                Close();
            }
            catch
            {
                MessageBox.Show("Invalid sever address.");
            }
            
        }*/

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
                        string ss4 = Path.Combine(ss2, "background.png");
                        button_hover_path = Path.Combine(ss2, "button_sel.png");
                        button_idle_path = Path.Combine(ss2, "button.png");
                        if (File.Exists(ss4))
                        {
                            bg = new Bitmap(ss4);
                            this.BackgroundImage = bg;
                            m.BackgroundImage = bg;
                        }
                        if (File.Exists(ss3))
                        {
                            pictureBox1.Image = new Bitmap(ss3);
                        }
                        // Check if the image files for the buttons exist
                        buttonImagesExist = (File.Exists(button_idle_path) && File.Exists(button_hover_path));
                        if (buttonImagesExist)
                        {
                            button_hover_image = new Bitmap(button_hover_path);
                            button_idle_image = new Bitmap(button_idle_path);
                            button_singleplayer.BackgroundImage = button_idle_image;
                            button_multiplayer.BackgroundImage = button_idle_image;
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
            /*
            try
            {
                if (File.Exists(lastserverpath))
                {
                    textBox1.Text = File.ReadAllText(lastserverpath);
                }
            }
            catch
            {
            }
            */
        }

        private void button_singleplayer_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dir = System.Environment.CurrentDirectory;
                var result = openFileDialog1.ShowDialog();
                System.Environment.CurrentDirectory = dir;
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
        
        private void button_singleplayer_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                button_singleplayer.BackgroundImage = button_hover_image;
            }
            catch
            {
            }
        }

        private void button_singleplayer_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                button_singleplayer.BackgroundImage = button_idle_image;
            }
            catch
            {
            }
        }

        private void button_multiplayer_Click(object sender, EventArgs e)
        {
            m.ShowDialog();
            if (m.ConnectNow)
            {
                Chosen = ChosenGameType.Multiplayer;
                MultiplayerConnectData = new ConnectData();
                MultiplayerConnectData.Auth = m.LoginAuthcode;
                MultiplayerConnectData.Ip = m.LoginIp;
                MultiplayerConnectData.Port = int.Parse(m.LoginPort);
                MultiplayerConnectData.Username = m.LoginUser;
                Close();
            }
        }

        private void button_multiplayer_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                button_multiplayer.BackgroundImage = button_hover_image;
            }
            catch
            {
            }
        }

        private void button_multiplayer_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                button_multiplayer.BackgroundImage = button_idle_image;
            }
            catch
            {
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using ManicDigger;

namespace GameMenu
{
    //todo ClientConfig.xml?
    public class LoginDataFile
    {
        //empty if no login saved.
        public string LoginName = "";
        //empty if guest
        public string Password = "";
        public void Load()
        {
            string filename = GetPasswordFilePath();
            if (File.Exists(filename))
            {
                string[] lines = File.ReadAllLines(filename);
                if (lines.Length > 0)
                {
                    LoginName = lines[0];
                }
                if (lines.Length > 1)
                {
                    Password = lines[1];
                }
            }
        }
        public void Save()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(LoginName);
            b.AppendLine(Password);
            File.WriteAllText(GetPasswordFilePath(), b.ToString());
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
        public void Delete()
        {
            string filename = GetPasswordFilePath();
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }
    public class FormLogin : IForm
    {
        public MenuWindow menu;
        public Game game;
        public LoginDataFile logindatafile;
        public void Initialize()
        {
            widgets.Clear();
            menu.AddBackground(widgets);
            menu.AddCaption(this, "Login");

            //Captions
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(300, 200, 400, 128),
                Text = "Use account",
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1100, 200, 400, 128),
                Text = "Create account",
            });
            
            //TextBox Login

            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(300 - 50, 400, 400, 90),
                Text = "Name: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            userTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(300 + 100, 400, 350, 90),
                Text = game.LoginName,
                FontSize = 20,
                IsTextbox = true,
            };
            widgets.Add(userTextbox);
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(300 - 50, 400 + 100, 400, 90),
                Text = "Pass: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            passwordTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(300 + 100, 400 + 100, 350, 90),
                FontSize = 20,
                IsTextbox = true,
                IsPassword = true,
            };
            widgets.Add(passwordTextbox);
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(300 + 100 + 350, 400 + 100, 350, 90),
                Text = "(leave empty to\n play as a guest)",
                FontSize = 20,
                Visible = true,
                TextColor = Color.Gray,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1050, 450, 400, 128),
                Text = "(click link to open website)",
                FontSize = 20,
                TextColor = Color.Gray,
            });

            //Link Create Account
            string url = "http://fragmer.net/md/";
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1050, 350, 400, 128),
                Text = "&1" + url,
                Click = delegate { System.Diagnostics.Process.Start(url); },
            });

            invalidLoginWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(300, 400 + 300, 350, 90),
                Text = "Invalid username or password.",
                FontSize = 20,
                Visible = false,
                TextColor = Color.Red,
            };
            widgets.Add(invalidLoginWidget);

            //Buttons
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(300, 900, 400, 128),
                Text = "Login",
                Click = delegate
                {
                    if (passwordTextbox.Text == "")
                    {
                        game.LoginGuest(userTextbox.Text);
                        RememberPassword();
                        menu.FormJoinMultiplayer();
                    }
                    bool success = game.LoginAccount(userTextbox.Text, passwordTextbox.Text);
                    if (!success)
                    {
                        invalidLoginWidget.Visible = true;
                        start = DateTime.UtcNow;
                    }
                    else
                    {
                        RememberPassword();
                        menu.FormJoinMultiplayer();
                    }
                },
            });

            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(300 - 50, 600, 400, 90),
                Text = "Remember password: ",
                Click = delegate { },
                FontSize = 20,
            });
            rememberPasswordWidget = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(300 - 50 + 400, 600, 200, 90),
                Text = "",//Render()
                Click = delegate { rememberpassword = !rememberpassword; rememberPassword_CheckedChanged(); },
                FontSize = 20,
            };
            widgets.Add(rememberPasswordWidget);

            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(800, 900, 400, 128),
                Text = "Cancel",
                Click = delegate { menu.typingfield = -1; menu.FormJoinMultiplayer(); },
            });

            LoadPassword();
        }
        Widget invalidLoginWidget;
        Widget userTextbox;
        Widget passwordTextbox;
        Widget rememberPasswordWidget;
        bool rememberpassword;
        DateTime start;
        public void Render()
        {
            if ((DateTime.UtcNow - start).TotalSeconds > 3)
            {
                invalidLoginWidget.Visible = false;
            }
            rememberPasswordWidget.Text = rememberpassword ? "Yes" : "No";
        }
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
        private void rememberPassword_CheckedChanged()
        {
            if (!rememberpassword)
            {
                logindatafile.Delete();
            }
            else
            {
                RememberPassword();
            }
        }
        private void RememberPassword()
        {
            if (rememberpassword)
            {
                RememberPassword(userTextbox.Text, passwordTextbox.Text);
            }
        }
        private void RememberPassword(string user, string password)
        {
            logindatafile.LoginName = user;
            logindatafile.Password = password;
            logindatafile.Save();
        }
        void LoadPassword()
        {
            logindatafile.Load();
            userTextbox.Text = logindatafile.LoginName;
            passwordTextbox.Text = logindatafile.Password;
            rememberpassword = userTextbox.Text != "";
        }
    }
}

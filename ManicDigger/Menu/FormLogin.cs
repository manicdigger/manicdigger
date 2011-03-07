using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameMenu
{
    public class FormLogin : IForm
    {
        public MenuWindow menu;
        public Game game;
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
                Rect = new RectangleF(100, 200, 400, 128),
                Text = "Guest",
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(600, 200, 400, 128),
                Text = "Use account",
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1100, 200, 400, 128),
                Text = "Create account",
            });

            //TextBox Guest

            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(100 - 50, 400, 400, 90),
                Text = "Name: ",
                FontSize = 20,
            });
            Widget guestTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(100 + 100, 400, 350, 90),
                Text = game.LoginName,
                FontSize = 20,
                IsTextbox = true,
            };
            widgets.Add(guestTextbox);
            //widgets[i].OnText = delegate
            //{
            //    game.LoginName = guestTextbox.Text;
            //};
            
            //TextBox Login

            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(600 - 50, 400, 400, 90),
                Text = "Name: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            var loginTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(600 + 100, 400, 350, 90),
                //Text = typingfield == 0 ? typingbuffer : login,
                //Click = delegate
                //{
                //    typingfield = 0;
                //    typingbuffer = login;
                //    OnFinishedTyping = delegate { login = typingbuffer; };
                //    FormLogin();
                //},
                Text = game.LoginName,
                FontSize = 20,
                IsTextbox = true,
            };
            widgets.Add(loginTextbox);
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(600 - 50, 400 + 100, 400, 90),
                Text = "Pass: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            var passwordTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(600 + 100, 400 + 100, 350, 90),
                //Text = typingfield == 1 ? PassString(typingbuffer) : PassString(password),
                //Click = delegate
                //{
                //    typingfield = 1;
                //    typingbuffer = password;
                //    OnFinishedTyping = delegate { password = typingbuffer; };
                //    FormLogin();
                //},
                FontSize = 20,
                IsTextbox = true,
                IsPassword = true,
            };
            widgets.Add(passwordTextbox);

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

            //TextBox Create Account
            /*
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(600 - 50, 400 + 100, 400, 90),
                Text = "Pass: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1100 - 50, 400, 400, 90),
                Text = "Name: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(1100 + 100, 400, 350, 90),
                FontSize = 20,
                IsTextbox = true,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(1100 - 50, 400 + 100, 400, 90),
                Text = "Pass: ",
                //Click = FormJoinMultiplayer,
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(1100 + 100, 400 + 100, 350, 90),
                FontSize = 20,
                IsTextbox = true,
                IsPassword = true,
            });
            */
            invalidLoginWidget = new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(550, 400 + 200, 350, 90),
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
                Rect = new RectangleF(100, 800, 400, 128),
                Text = "Play as guest",
                Click = delegate
                {
                    game.LoginGuest(guestTextbox.Text);
                    menu.FormJoinMultiplayer();
                },
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(600, 800, 400, 128),
                Text = "Login",
                Click = delegate
                {
                    bool success = game.LoginAccount(loginTextbox.Text, passwordTextbox.Text);
                    if (!success)
                    {
                        invalidLoginWidget.Visible = true;
                        start = DateTime.UtcNow;
                    }
                    else
                    {
                        menu.FormJoinMultiplayer();
                    }
                },
            });
            /*
            widgets.Add(new Widget()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(1100, 800, 400, 128),
                Text = "Create account",
                Click = delegate { game.CreateAccountLogin(createlogin, createpassword); menu.typingfield = -1; menu.FormJoinMultiplayer(); },
            });
            */
            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(600, 1000, 400, 128),
                Text = "Cancel",
                Click = delegate { menu.typingfield = -1; menu.FormJoinMultiplayer(); },
            });
        }
        Widget invalidLoginWidget;
        DateTime start;
        public void Render()
        {
            if ((DateTime.UtcNow - start).TotalSeconds > 3)
            {
                invalidLoginWidget.Visible = false;
            }
        }
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
        string guestlogin = "";
        string login = "";
        string password = "";
        string createlogin = "";
        string createpassword = "";
    }
    public partial class MenuWindow
    {
    }
}

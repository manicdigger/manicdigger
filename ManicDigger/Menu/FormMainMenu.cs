using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;

namespace GameMenu
{
    public class FormMainMenu : IForm
    {
        public MenuWindow menu;
        public void Initialize()
        {
            widgets.Clear();
            menu.AddBackground(widgets);
            widgets.Add(new Widget() { BackgroundImage = Path.Combine("gui", "logo.png"), Rect = new RectangleF(menu.ConstWidth / 2 - 780 * 1.5f / 2, 10, 1024 * 1.5f, 512 * 1.5f) });
            string[] text = new string[] { "Single-player", "Join multiplayer", "Start multiplayer server", "Game options", "Exit" };
            ThreadStart[] actions = new ThreadStart[]
                {
                    menu.FormSelectWorld,
                    menu.FormJoinMultiplayer,
                    menu.FormStartMultiplayerServer,
                    menu.FormGameOptions,
                    menu.Exit,
                };
            for (int i = 0; i < 5; i++)
            {
                widgets.Add(new Widget()
                {
                    BackgroundImage = Path.Combine("gui", "button4.png"),
                    BackgroundImageSelected = Path.Combine("gui", "button4_sel.png"),
                    Rect = new RectangleF(menu.ConstWidth / 2 - 600 / 2, 460 + i * 140, 600, 128),
                    Text = text[i],
                    Click = actions[i],
                });
            }
        }
        public void Render()
        {
        }
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

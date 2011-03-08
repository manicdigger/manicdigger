using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace GameMenu
{
    public class FormSelectWorld : IForm
    {
        public MenuWindow menu;
        public Game game;
        public void Initialize()
        {
            widgets.Clear();
            worldbuttons.Clear();
            menu.AddBackground(widgets);
            menu.AddCaption(this, "Select world");
            string[] w = new List<string>(game.GetWorlds()).ToArray();
            for (int i = 0; i < w.Length; i++)
            {
                if (string.IsNullOrEmpty(w[i]))
                {
                    w[i] = "Empty";
                }
            }
            for (int i = 0; i < 8; i++)
            {
                int ii = i;//closure
                var widget = new Widget()
                {
                    BackgroundImage = menu.button4,
                    BackgroundImageSelected = menu.button4sel,
                    Rect = new RectangleF(350 + (i % 2) * 500, 300 + (i / 2) * 150, 400, 128),
                    Text = w[i],
                    Click = delegate { selectedWorld = ii; },
                    selected = selectedWorld == i,
                };
                worldbuttons.Add(widget);
                widgets.Add(widget);
            }

            widgets.Add(new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(650, 900, 300, 90),//500
                Text = "Delete",
                Click = delegate
                {
                    string name = game.GetWorlds()[selectedWorld.Value];
                    if(!string.IsNullOrEmpty(name))
                    {
                        menu.MessageBoxYesNo(string.Format("Are you sure you want to delete world \"{0}\"?", name)
                            , delegate { game.DeleteWorld(selectedWorld.Value); Initialize(); }, delegate { });
                    }
                },
                FontSize = 20,
            });/*
            widgets.Add(new Widget()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(800, 900, 300, 90),
                Text = "Options",
                Click = delegate { },
                FontSize = 20,
            });*/

            menu.AddOkCancel(this, delegate { menu.afterSelectWorld(); }, delegate { menu.FormMainMenu(); });
        }
        List<Widget> worldbuttons = new List<Widget>();
        public void Render()
        {
            if (selectedWorld != null)
            {
                for (int i = 0; i < worldbuttons.Count; i++)
                {
                    worldbuttons[i].selected = i == selectedWorld.Value;
                }
            }
        }
        public int? selectedWorld = 0;
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

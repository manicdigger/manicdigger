using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameMenu
{
    public class FormWorldOptions : IForm
    {
        public MenuWindow menu;
        public Game game;
        public void Initialize()
        {
            widgets.Clear();
            menu.AddBackground(this.widgets);
            menu.AddCaption(this, "World options");
            /*
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 200, 400, 90),
                Text = "World: " + (selectedWorld == null ? "none" : game.GetWorlds()[selectedWorld.Value]),
                Click = delegate { afterSelectWorld = FormStartMultiplayerServer; FormSelectWorld(); },
                FontSize = 20,
            });
            widgets.Add(new Widget()
            {
                BackgroundImage = "button4.png",
                BackgroundImageSelected = "button4_sel.png",
                Rect = new RectangleF(500, 200, 300, 90),
                Text = "Select",
                Click = delegate { afterSelectWorld = FormStartMultiplayerServer; FormSelectWorld(); },
                FontSize = 20,
            });
            */
            widgets.Add(new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(200, 300, 400, 90),
                Text = "World name: ",
                Click = delegate { },
                FontSize = 20,
            });
            var nameTextbox = new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(500, 300, 600, 90),
                Text = game.GetWorlds()[worldId],
                Click = delegate { }, //todo textbox
                FontSize = 20,
                IsTextbox = true,
            };
            widgets.Add(nameTextbox);
            menu.AddOkCancel(this, delegate { game.SetWorldOptions(worldId, nameTextbox.Text); }, delegate { menu.FormSelectWorld(); });
        }
        public void Render()
        {
        }
        public int worldId;
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

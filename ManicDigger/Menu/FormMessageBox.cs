using System;
using System.Collections.Generic;
using System.Text;
using GameMenu;
using System.Threading;
using System.Drawing;

namespace ManicDigger.Menu
{
    public class FormMessageBox : IForm
    {
        public MenuWindow menu;
        public Game game;
        Widget MessageboxYesWidget;
        Widget MessageboxNoWidget;
        Widget MessageboxBackgroundWidget;
        Widget MessageboxLabelWidget;
        public void MessageBoxYesNo(string text, ThreadStart yes, ThreadStart no)
        {
            widgets.Clear();
            menu.AddBackground(widgets);
            MessageboxYesWidget = (new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(400, 600, 400, 128),
                Text = "OK",
                Click = yes,
            });
            MessageboxNoWidget = (new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(800, 600, 400, 128),
                Text = "Cancel",
                Click = no,
            });/*
            MessageboxBackgroundWidget = (new Widget()
            {
                BackgroundImage = menu.button4,
                BackgroundImageSelected = menu.button4sel,
                Rect = new RectangleF(400, 200, 800, 500),
            });*/
            MessageboxLabelWidget = (new Widget()
            {
                BackgroundImage = null,
                BackgroundImageSelected = null,
                Rect = new RectangleF(400, 400, 400, 128),
                Text = text,
            });
            //widgets.Add(MessageboxBackgroundWidget);
            widgets.Add(MessageboxLabelWidget);
            widgets.Add(MessageboxYesWidget);
            widgets.Add(MessageboxNoWidget);
        }
        public bool Visible = false;
        public void Render()
        {
        }
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

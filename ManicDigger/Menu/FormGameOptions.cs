using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using ManicDigger;

namespace GameMenu
{
    public class FormGameOptions : IForm
    {
        public MenuWindow menu;
        public Game game;
        //public Options options;
        public void Initialize()
        {
            widgets.Clear();
            menu.AddBackground(widgets);
            menu.AddCaption(this, "Game options");
            string[] text = new string[] { "Graphics", "Keys", "Other" };
            ThreadStart[] actions = new ThreadStart[]
                {
                    FormGameOptionsGraphics,
                    FormGameOptionsKeys,
                    FormGameOptionsOther,
                };
            for (int i = 0; i < 3; i++)
            {
                widgets.Add(new Widget()
                {
                    BackgroundImage = menu.button4,
                    BackgroundImageSelected = menu.button4sel,
                    Rect = new RectangleF(100 + 500 * i, 200, 400, 128),
                    Text = text[i],
                    Click = actions[i],
                    selected = gameoptionstype == i,
                });
            }
            if (gameoptionstype == 0)// || gameoptionstype == 2)
            {
                string[] text1 = new string[]
                {
                    "Shadows: " + (options.Shadows ? "ON" : "OFF"),
                    "View distance: " + (options.DrawDistance),
                    "Use server textures (restart): " + (options.UseServerTextures ? "ON" : "OFF"),
                    "Font: " + options.Font,
                };
                ThreadStart[] actions1 = new ThreadStart[]
                {
                    delegate{options.Shadows=!options.Shadows;Initialize();},
                    delegate{ToggleFog();Initialize();},
                    delegate{options.UseServerTextures=!options.UseServerTextures;Initialize();},
                    delegate{if(options.Font==0){options.Font=1;}else{options.Font=0;Initialize();}},
                };
                for (int i = 0; i < 3; i++)
                {
                    widgets.Add(new Widget()
                    {
                        BackgroundImage = menu.button4,
                        BackgroundImageSelected = menu.button4sel,
                        Rect = new RectangleF(menu.ConstWidth / 2 - 800 / 2, 460 + i * 140, 800, 128),
                        Text = text1[i],
                        Click = actions1[i],
                    });
                }
            }
            if (gameoptionstype == 1)
            {
                int column = 0;
                int minus = 0;
                for (int i = 0; i < keyhelps.Length; i++)
                {
                    int ii = i; //a copy for closure
                    int defaultkey = keyhelps[i].DefaultKey;
                    int key = defaultkey;
                    if (options.Keys.ContainsKey(defaultkey))
                    {
                        key = options.Keys[defaultkey];
                    }
                    widgets.Add(new Widget()
                    {
                        BackgroundImage = menu.button4,
                        BackgroundImageSelected = menu.button4sel,
                        Rect = new RectangleF(80 + column * 360, 350 + i * 100 - minus, 340, 90),
                        Text = keyhelps[i].Text + ": " + KeyName(key),
                        FontSize = 15,
                        Click = delegate { keyselectid = ii; },
                    });
                    if ((i + 1) % 6 == 0) { column++; minus += 100 * 6; }
                }
            }
            menu.AddOkCancel(this, menu.FormMainMenu, menu.FormMainMenu);
        }
        private string KeyName(int key)
        {
            if (Enum.IsDefined(typeof(OpenTK.Input.Key), key))
            {
                string s = Enum.GetName(typeof(OpenTK.Input.Key), key);
                return s;
            }
            if (Enum.IsDefined(typeof(SpecialKey), key))
            {
                string s = Enum.GetName(typeof(SpecialKey), key);
                return s;
            }
            return key.ToString();
        }
        class KeyHelp
        {
            public string Text;
            public int DefaultKey;
        }
        enum SpecialKey
        {
            MouseLeftClick = 200,
            MouseRightClick = 201,
        }
        KeyHelp[] keyhelps = new KeyHelp[]
        {
            new KeyHelp(){Text="Move foward", DefaultKey=(int)OpenTK.Input.Key.W},
            new KeyHelp(){Text="Move back", DefaultKey=(int)OpenTK.Input.Key.S},
            new KeyHelp(){Text="Move left", DefaultKey=(int)OpenTK.Input.Key.A},
            new KeyHelp(){Text="Move right", DefaultKey=(int)OpenTK.Input.Key.D},
            new KeyHelp(){Text="Jump", DefaultKey=(int)OpenTK.Input.Key.Space},
            //new KeyHelp(){Text="Remove block", DefaultKey=(int)SpecialKey.MouseLeftClick},
            //new KeyHelp(){Text="Place block", DefaultKey=(int)SpecialKey.MouseRightClick},
            new KeyHelp(){Text="Show material selector", DefaultKey=(int)OpenTK.Input.Key.B},
            new KeyHelp(){Text="Set spawn position", DefaultKey=(int)OpenTK.Input.Key.P},
            new KeyHelp(){Text="Respawn", DefaultKey=(int)OpenTK.Input.Key.R},
            new KeyHelp(){Text="Toggle fog distance", DefaultKey=(int)OpenTK.Input.Key.F},
            new KeyHelp(){Text="1x move speed", DefaultKey=(int)OpenTK.Input.Key.F1},
            new KeyHelp(){Text="10x move speed", DefaultKey=(int)OpenTK.Input.Key.F2},
            new KeyHelp(){Text="Free move", DefaultKey=(int)OpenTK.Input.Key.F3},
            new KeyHelp(){Text="Third-person camera", DefaultKey=(int)OpenTK.Input.Key.F5},
            new KeyHelp(){Text="Fullscreen", DefaultKey=(int)OpenTK.Input.Key.F11},
            new KeyHelp(){Text="Screenshot", DefaultKey=(int)OpenTK.Input.Key.F12},
            new KeyHelp(){Text="Players list", DefaultKey=(int)OpenTK.Input.Key.Tab},
            new KeyHelp(){Text="Chat", DefaultKey=(int)OpenTK.Input.Key.Enter},
            new KeyHelp(){Text="Unload blocks", DefaultKey=(int)OpenTK.Input.Key.U},
            new KeyHelp(){Text="Craft", DefaultKey=(int)OpenTK.Input.Key.C},
            new KeyHelp(){Text="Load blocks", DefaultKey=(int)OpenTK.Input.Key.L},
            new KeyHelp(){Text="Enter/leave minecart", DefaultKey=(int)OpenTK.Input.Key.V},
            new KeyHelp(){Text="Reverse minecart", DefaultKey=(int)OpenTK.Input.Key.Q},
            //new KeyHelp(){Text="Swap mouse up-down", BoolId="SwapMouseUpDown"},
        };
        int keyselectid = -1;
        int gameoptionstype = 0;
        void FormGameOptionsGraphics()
        {
            gameoptionstype = 0;
            Initialize();
        }
        void FormGameOptionsKeys()
        {
            gameoptionstype = 1;
            Initialize();
        }
        void FormGameOptionsOther()
        {
            gameoptionstype = 2;
            Initialize();
        }
        public class Options
        {
            public bool Shadows;
            public int Font;
            public int DrawDistance = 256;
            public bool UseServerTextures = true;
            public bool EnableSound = true;
            public SerializableDictionary<int, int> Keys = new SerializableDictionary<int, int>();
        }
        Options options = new Options();
        public int[] drawDistances = { 32, 64, 128, 256, 512 };
        public void ToggleFog()
        {
            for (int i = 0; i < drawDistances.Length; i++)
            {
                if (options.DrawDistance == drawDistances[i])
                {
                    options.DrawDistance = drawDistances[(i + 1) % drawDistances.Length];
                    return;
                }
            }
            options.DrawDistance = drawDistances[0];
        }
        public void Render()
        {
        }
        List<Widget> widgets = new List<Widget>();
        public List<Widget> Widgets { get { return widgets; } set { widgets = value; } }
    }
}

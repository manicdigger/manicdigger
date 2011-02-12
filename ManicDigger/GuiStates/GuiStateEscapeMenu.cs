using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;

namespace ManicDigger
{
    public class Options
    {
        public bool Shadows;
        public int Font;
        public int DrawDistance = 256;
        public bool UseServerTextures = true;
        public SerializableDictionary<int, int> Keys = new SerializableDictionary<int, int>();
    }
    partial class ManicDiggerGameWindow
    {
        private void EscapeMenuStart()
        {
            guistate = GuiState.EscapeMenu;
            menustate = new MenuState();
            FreeMouse = true;
            SetEscapeMenuState(EscapeMenuState.Main);
        }
        enum EscapeMenuState
        {
            Main,
            Options,
            Graphics,
            Keys,
        }
        EscapeMenuState escapemenustate;
        private void EscapeMenuMouse1()
        {
            foreach (var w in new List<Button>(widgets))
            {
                w.selected = w.Rect.Contains(mouse_current);
                if (w.selected && mouseleftclick)
                {
                    w.InvokeOnClick();
                }
            }
        }
        void SetEscapeMenuState(EscapeMenuState state)
        {
            escapemenustate = state;
            widgets.Clear();
            if (state == EscapeMenuState.Main)
            {
                AddButton("Return to game",(a, b) => { GuiStateBackToGame(); });
                AddButton("Options", (a, b) => { SetEscapeMenuState(EscapeMenuState.Options); });
                AddButton("Exit", (a, b) =>
                {
                    exit = true;
                    this.Exit();
                });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Options)
            {
                AddButton("Graphics", (a, b) => { SetEscapeMenuState(EscapeMenuState.Graphics); });
                AddButton("Keys", (a, b) => { SetEscapeMenuState(EscapeMenuState.Keys); });
                AddButton("Return to main menu", (a, b) => { SetEscapeMenuState(EscapeMenuState.Main); });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Graphics)
            {
                AddButton("Shadows: " + (currentshadows.ShadowsFull ? "ON" : "OFF"),
                    (a, b) =>
                    {
                        currentshadows.ShadowsFull = !currentshadows.ShadowsFull;
                        terrain.UpdateAllTiles();
                    });
                AddButton("View distance: " + (terrain.DrawDistance),
                    (a, b) =>
                    {
                        ToggleFog();
                    });
                AddButton("Use server textures (restart): " + (options.UseServerTextures ? "ON" : "OFF"),
                    (a, b) =>
                    {
                        options.UseServerTextures = !options.UseServerTextures;
                    });
                AddButton("Font: " + (textdrawer.NewFont ? "2" : "1"),
                    (a, b) =>
                    {
                        textdrawer.NewFont = !textdrawer.NewFont;
                        cachedTextTextures.Clear();
                    });
                AddButton("Return to options menu", (a, b) => { SetEscapeMenuState(EscapeMenuState.Options); });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Keys)
            {
                int fontsize = 12;
                int textheight = 20;
                for (int i = 0; i < keyhelps.Length; i++)
                {
                    int ii = i; //a copy for closure
                    int defaultkey = keyhelps[i].DefaultKey;
                    int key = defaultkey;
                    if (options.Keys.ContainsKey(defaultkey))
                    {
                        key = options.Keys[defaultkey];
                    }
                    AddButton(keyhelps[i].Text + ": " + KeyName(key), (a, b) => { keyselectid = ii; });
                }
                AddButton("Default keys", (a, b) => { options.Keys.Clear(); });
                AddButton("Return to options menu", (a, b) => { SetEscapeMenuState(EscapeMenuState.Options); });
                MakeSimpleOptions(fontsize, textheight);
            }
            SaveOptions();
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
        void AddButton(string text, EventHandler e)
        {
            Button b = new Button();
            b.Text = text;
            b.OnClick += e;
            widgets.Add(b);
        }
        void MakeSimpleOptions(int fontsize, int textheight)
        {
            int starty = ycenter(widgets.Count * textheight);
            for (int i = 0; i < widgets.Count; i++)
            {
                string s = widgets[i].Text;
                Rectangle rect = new Rectangle();
                SizeF size = TextSize(s, fontsize);
                rect.Width = (int)size.Width + 10;
                rect.Height = (int)size.Height;
                rect.X = xcenter(size.Width);
                rect.Y = starty + textheight * i;
                widgets[i].Rect = rect;
                widgets[i].fontsize = fontsize;
                if (i == keyselectid)
                {
                    widgets[i].fontcolor = Color.Green;
                    widgets[i].fontcolorselected = Color.Green;
                }
            }
        }
        void EscapeMenuDraw()
        {
            SetEscapeMenuState(escapemenustate);
            EscapeMenuMouse1();
            foreach (var w in widgets)
            {
                Draw2dText(w.Text, w.Rect.X, w.Rect.Y, w.fontsize, w.selected ? w.fontcolorselected : w.fontcolor);
            }
        }
        List<Button> widgets = new List<Button>();
        class Button
        {
            public Rectangle Rect;
            public string Text;
            public event EventHandler OnClick;
            public bool selected;
            public int fontsize = 20;
            public Color fontcolor = Color.White;
            public Color fontcolorselected = Color.Red;
            public void InvokeOnClick()
            {
                OnClick(this, new EventArgs());
            }
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
            new KeyHelp(){Text="Noclip", DefaultKey=(int)OpenTK.Input.Key.F4},
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
        private void EscapeMenuKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == GetKey(OpenTK.Input.Key.Escape))
            {
                if (escapemenustate == EscapeMenuState.Graphics || escapemenustate == EscapeMenuState.Keys)
                {
                    SetEscapeMenuState(EscapeMenuState.Options);
                }
                else if (escapemenustate == EscapeMenuState.Options)
                {
                    SetEscapeMenuState(EscapeMenuState.Main);
                }
                else
                {
                    SetEscapeMenuState(EscapeMenuState.Main);
                    GuiStateBackToGame();
                }
            }
            if (escapemenustate == EscapeMenuState.Keys)
            {
                if (keyselectid != -1)
                {
                    options.Keys[keyhelps[keyselectid].DefaultKey] = (int)e.Key;
                    keyselectid = -1;
                }
            }
        }
        Options options = new Options();
        XmlSerializer x = new XmlSerializer(typeof(Options));
        public string gamepathconfig = GameStorePath.GetStorePath();
        string filename = "ClientConfig.xml";
        void LoadOptions()
        {
            string path = Path.Combine(gamepathconfig, filename);
            if (!File.Exists(path))
            {
                return;
            }
            string s = File.ReadAllText(path);
            this.options = (Options)x.Deserialize(new System.IO.StringReader(s));

            textdrawer.NewFont = options.Font != 1;
            currentshadows.ShadowsFull = options.Shadows;
            shadows.ResetShadows();
            terrain.UpdateAllTiles();
            terrain.DrawDistance = options.DrawDistance;
        }
        void SaveOptions()
        {
            options.Font = textdrawer.NewFont ? 0 : 1;
            options.Shadows = currentshadows.ShadowsFull;
            options.DrawDistance = terrain.DrawDistance;
            
            string path = Path.Combine(gamepathconfig, filename);
            MemoryStream ms = new MemoryStream();
            x.Serialize(ms, options);
            string xml = Encoding.UTF8.GetString(ms.ToArray());
            File.WriteAllText(path, xml);
        }
    }
}

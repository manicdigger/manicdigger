using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using OpenTK;
using GameModeFortress;

namespace ManicDigger
{
    public class GuiStateEscapeMenu
    {
        public GuiStateEscapeMenu()
        {
            options = new OptionsCi();
            fonts = new string[4];
            fonts[0] = "Nice";
            fonts[1] = "Simple";
            fonts[2] = "BlackBackground";
            fonts[3] = "Default";
            fontValues = new int[4];
            fontValues[0] = 0;
            fontValues[1] = 1;
            fontValues[2] = 2;
            fontValues[3] = 3;
        }
        public ManicDiggerGameWindow game;
        public void EscapeMenuStart()
        {
            game.guistate = GuiState.EscapeMenu;
            game.menustate = new MenuState();
            game.FreeMouse = true;
            SetEscapeMenuState(EscapeMenuState.Main);
        }
        EscapeMenuState escapemenustate;
        private void EscapeMenuMouse1()
        {
            foreach (var w in new List<Button>(widgets))
            {
                w.selected = w.Rect.Contains(game.mouse_current);
                if (w.selected && game.mouseleftclick)
                {
                    w.InvokeOnClick();
                }
            }
        }
        void SetEscapeMenuState(EscapeMenuState state)
        {
            Language language = game.game.language;
            escapemenustate = state;
            widgets.Clear();
            if (state == EscapeMenuState.Main)
            {
                AddButton(language.ReturnToGame(), (a, b) => { game.GuiStateBackToGame(); });
                AddButton(language.Options(), (a, b) => { SetEscapeMenuState(EscapeMenuState.Options); });
                AddButton(language.Exit(), (a, b) =>
                {
                    RestoreResolution();
                    game.SendLeave(Packet_LeaveReasonEnum.Leave);
                    game.d_Exit.exit = true;
                    game.d_GlWindow.Exit();
                });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Options)
            {
                AddButton(language.Graphics(), (a, b) => { SetEscapeMenuState(EscapeMenuState.Graphics); });
                AddButton(language.Keys(), (a, b) => { SetEscapeMenuState(EscapeMenuState.Keys); });
                AddButton(language.Other(), (a, b) => { SetEscapeMenuState(EscapeMenuState.Other); });
                AddButton(language.ReturnToMainMenu(), (a, b) => { SaveOptions(); SetEscapeMenuState(EscapeMenuState.Main); });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Graphics)
            {
                /*
                AddButton(string.Format(Language.ShadowsOption, (d_CurrentShadows.ShadowsFull ? Language.On : Language.Off)),
                    (a, b) =>
                    {
                        d_CurrentShadows.ShadowsFull = !d_CurrentShadows.ShadowsFull;
                        RedrawAllBlocks();
                    });
                */
                    AddButton(string.Format("Smooth shadows: {0}", options.Smoothshadows ? language.On() : language.Off()),
                    (a, b) =>
                    {
                        options.Smoothshadows = !options.Smoothshadows;
                        game.d_TerrainChunkTesselator.EnableSmoothLight = options.Smoothshadows;
                        if (options.Smoothshadows)
                        {
                            options.BlockShadowSave = 0.7f;
                            game.d_TerrainChunkTesselator.BlockShadow = options.BlockShadowSave;
                        }
                        else
                        {
                            options.BlockShadowSave = 0.6f;
                            game.d_TerrainChunkTesselator.BlockShadow = options.BlockShadowSave;
                        }
                        game.RedrawAllBlocks();
                    });
                AddButton(string.Format(language.ViewDistanceOption(), (game.d_Config3d.viewdistance)),
                    (a, b) =>
                    {
                        game.ToggleFog();
                    });
                AddButton(string.Format("Framerate: {0}", (VsyncString())),
                    (a, b) =>
                    {
                        game.ToggleVsync();
                    });
                AddButton(string.Format("Resolution: {0}", (ResolutionString())),
                    (a, b) =>
                    {
                        ToggleResolution();
                    });
                AddButton(string.Format("Fullscreen: {0}", options.Fullscreen ? language.On() : language.Off()),
                    (a, b) =>
                    {
                        options.Fullscreen = !options.Fullscreen;
                    });
                AddButton(string.Format(language.UseServerTexturesOption(), (options.UseServerTextures ? language.On() : language.Off())),
                    (a, b) =>
                    {
                        options.UseServerTextures = !options.UseServerTextures;
                    });
                AddButton(string.Format(language.FontOption(), (FontString())),
                    (a, b) =>
                    {
                        ToggleFont();
                    });
                AddButton(language.ReturnToOptionsMenu(), (a, b) => { UseFullscreen(); UseResolution(); SetEscapeMenuState(EscapeMenuState.Options); });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Other)
            {
                AddButton(string.Format(language.SoundOption(), (game.AudioEnabled ? language.On() : language.Off())),
                    (a, b) =>
                    {
                        game.AudioEnabled = !game.AudioEnabled;
                    });
                AddButton(language.ReturnToOptionsMenu(), (a, b) => { SetEscapeMenuState(EscapeMenuState.Options); });
                MakeSimpleOptions(20, 50);
            }
            else if (state == EscapeMenuState.Keys)
            {
                int fontsize = 12;
                int textheight = 20;
                KeyHelp[] helps = keyhelps();
                for (int i = 0; i < 1024; i++)
                {
                    if (helps[i] == null)
                    {
                        break;
                    }
                    int ii = i; //a copy for closure
                    int defaultkey = helps[i].DefaultKey;
                    int key = defaultkey;
                    if (options.Keys[defaultkey] != 0)
                    {
                        key = options.Keys[defaultkey];
                    }
                    AddButton(string.Format(language.KeyChange(), helps[i].Text, KeyName(key)), (a, b) => { keyselectid = ii; });
                }
                AddButton(language.DefaultKeys(), (a, b) => { options.Keys = new int[256]; });
                AddButton(language.ReturnToOptionsMenu(), (a, b) => { SetEscapeMenuState(EscapeMenuState.Options); });
                MakeSimpleOptions(fontsize, textheight);
            }
        }

        private void UseFullscreen()
        {
            if (options.Fullscreen)
            {
                game.d_GlWindow.WindowState = WindowState.Fullscreen;
                UseResolution();
            }
            else
            {
                game.d_GlWindow.WindowState = WindowState.Normal;
                RestoreResolution();
            }
        }

        private string VsyncString()
        {
            if (game.ENABLE_LAG == 0) { return "Vsync"; }
            else if (game.ENABLE_LAG == 1) { return "Unlimited"; }
            else if (game.ENABLE_LAG == 2) { return "Lag"; }
            else return null; //throw new Exception();
        }

        private string ResolutionString()
        {
            DisplayResolution res = game.resolutions[options.Resolution];
            return string.Format("{0}x{1}, {2}, {3} Hz", res.Width, res.Height, res.BitsPerPixel, res.RefreshRate);
        }

        private void ToggleResolution()
        {
            options.Resolution++;
            if (options.Resolution >= game.resolutions.Count)
            {
                options.Resolution = 0;
            }
        }
        Size originalResolution;
        bool changedResolution = false;
        public void RestoreResolution()
        {
            if (changedResolution)
            {
                DisplayDevice.Default.ChangeResolution(originalResolution.Width, originalResolution.Height, 32, -1);
            }
        }
        public void UseResolution()
        {
            if (!changedResolution)
            {
                originalResolution = new Size(DisplayDevice.Default.Width, DisplayDevice.Default.Height);
                changedResolution = true;
            }
            if (options.Resolution >= game.resolutions.Count)
            {
                options.Resolution = 0;
            }
            DisplayResolution res = game.resolutions[options.Resolution];
            if (game.d_GlWindow.WindowState == WindowState.Fullscreen)
            {
                DisplayDevice.Default.ChangeResolution(res.Width, res.Height, res.BitsPerPixel, res.RefreshRate);
                game.d_GlWindow.WindowState = WindowState.Normal;
                game.d_GlWindow.WindowState = WindowState.Fullscreen;
            }
            else
            {
                //d_GlWindow.Width = res.Width;
                //d_GlWindow.Height = res.Height;
            }
        }

        string[] fonts;
        int[] fontValues;

        private string FontString()
        {
            return fonts[options.Font];
        }
        private void ToggleFont()
        {
            options.Font++;
            if (options.Font >= fonts.Length)
            {
                options.Font = 0;
            }
            game.Font = (FontType)fontValues[options.Font];
            for (int i = 0; i < game.game.cachedTextTexturesMax; i++)
            {
                game.game.cachedTextTextures[i] = null;
            }
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
            int starty = game.ycenter(widgets.Count * textheight);
            for (int i = 0; i < widgets.Count; i++)
            {
                string s = widgets[i].Text;
                Rectangle rect = new Rectangle();
                SizeF size = game.TextSize_(s, fontsize);
                rect.Width = (int)size.Width + 10;
                rect.Height = (int)size.Height;
                rect.X = game.xcenter(size.Width);
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
        public void EscapeMenuDraw()
        {
            SetEscapeMenuState(escapemenustate);
            EscapeMenuMouse1();
            foreach (var w in widgets)
            {
                game.Draw2dText_(w.Text, w.Rect.X, w.Rect.Y, w.fontsize, w.selected ? w.fontcolorselected : w.fontcolor);
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
        KeyHelp[] keyhelps()
        {
            int n = 1024;
            KeyHelp[] helps = new KeyHelp[n];
            for (int i = 0; i < n; i++)
            {
                helps[i] = null;
            }
            Language language = game.game.language;
            int count = 0;
            helps[count++] = new KeyHelp() { Text = language.KeyMoveFoward(), DefaultKey = (int)OpenTK.Input.Key.W };
            helps[count++] = new KeyHelp() { Text = language.KeyMoveBack(), DefaultKey = (int)OpenTK.Input.Key.S };
            helps[count++] = new KeyHelp() { Text = language.KeyMoveLeft(), DefaultKey = (int)OpenTK.Input.Key.A };
            helps[count++] = new KeyHelp() { Text = language.KeyMoveRight(), DefaultKey = (int)OpenTK.Input.Key.D };
            helps[count++] = new KeyHelp() { Text = language.KeyJump(), DefaultKey = (int)OpenTK.Input.Key.Space };
            //new KeyHelp(){Text="Remove block", DefaultKey=(int)SpecialKey.MouseLeftClick},
            //new KeyHelp(){Text="Place block", DefaultKey=(int)SpecialKey.MouseRightClick},
            helps[count++] = new KeyHelp() { Text = language.KeyShowMaterialSelector(), DefaultKey = (int)OpenTK.Input.Key.B };
            helps[count++] = new KeyHelp() { Text = language.KeySetSpawnPosition(), DefaultKey = (int)OpenTK.Input.Key.P };
            helps[count++] = new KeyHelp() { Text = language.KeyRespawn(), DefaultKey = (int)OpenTK.Input.Key.O };
            helps[count++] = new KeyHelp() { Text = language.KeyReloadWeapon(), DefaultKey = (int)OpenTK.Input.Key.R };
            helps[count++] = new KeyHelp() { Text = language.KeyToggleFogDistance(), DefaultKey = (int)OpenTK.Input.Key.F };
            helps[count++] = new KeyHelp() { Text = string.Format(language.KeyMoveSpeed(), "1"), DefaultKey = (int)OpenTK.Input.Key.F1 };
            helps[count++] = new KeyHelp() { Text = string.Format(language.KeyMoveSpeed(), "10"), DefaultKey = (int)OpenTK.Input.Key.F2 };
            helps[count++] = new KeyHelp() { Text = language.KeyFreeMove(), DefaultKey = (int)OpenTK.Input.Key.F3 };
            helps[count++] = new KeyHelp() { Text = language.KeyThirdPersonCamera(), DefaultKey = (int)OpenTK.Input.Key.F5 };
            helps[count++] = new KeyHelp() { Text = language.KeyTextEditor(), DefaultKey = (int)OpenTK.Input.Key.F9 };
            helps[count++] = new KeyHelp() { Text = language.KeyFullscreen(), DefaultKey = (int)OpenTK.Input.Key.F11 };
            helps[count++] = new KeyHelp() { Text = language.KeyScreenshot(), DefaultKey = (int)OpenTK.Input.Key.F12 };
            helps[count++] = new KeyHelp() { Text = language.KeyPlayersList(), DefaultKey = (int)OpenTK.Input.Key.Tab };
            helps[count++] = new KeyHelp() { Text = language.KeyChat(), DefaultKey = (int)OpenTK.Input.Key.T };
            helps[count++] = new KeyHelp() { Text = language.KeyTeamChat(), DefaultKey = (int)OpenTK.Input.Key.Y };
            //new KeyHelp(){Text="Unload blocks", DefaultKey=(int)OpenTK.Input.Key.U},
            helps[count++] = new KeyHelp() { Text = language.KeyCraft(), DefaultKey = (int)OpenTK.Input.Key.C };
            //new KeyHelp(){Text="Load blocks", DefaultKey=(int)OpenTK.Input.Key.L},
            helps[count++] = new KeyHelp() { Text = language.KeyBlockInfo(), DefaultKey = (int)OpenTK.Input.Key.I };
            helps[count++] = new KeyHelp() { Text = language.KeyUse(), DefaultKey = (int)OpenTK.Input.Key.E };
            helps[count++] = new KeyHelp() { Text = language.KeyReverseMinecart(), DefaultKey = (int)OpenTK.Input.Key.Q };
            //new KeyHelp(){Text="Swap mouse up-down", BoolId="SwapMouseUpDown"},
            return helps;
        }
        int keyselectid = -1;
        public void EscapeMenuKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == game.GetKey(OpenTK.Input.Key.Escape))
            {
                if (escapemenustate == EscapeMenuState.Graphics
                    || escapemenustate == EscapeMenuState.Keys
                    || escapemenustate == EscapeMenuState.Other)
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
                    game.GuiStateBackToGame();
                }
            }
            if (escapemenustate == EscapeMenuState.Keys)
            {
                if (keyselectid != -1)
                {
                    options.Keys[keyhelps()[keyselectid].DefaultKey] = (int)e.Key;
                    keyselectid = -1;
                }
            }
        }
        internal OptionsCi options;
        public void LoadOptions()
        {
            OptionsCi o = game.game.platform.LoadOptions();
            if (o == null)
            {
                return;
            }
            this.options = o;

            game.Font = (FontType)fontValues[options.Font];
            game.d_CurrentShadows.ShadowsFull = options.Shadows;
            //d_Shadows.ResetShadows();
            //d_Terrain.UpdateAllTiles();
            game.d_Config3d.viewdistance = options.DrawDistance;
            game.AudioEnabled = options.EnableSound;
            game.d_TerrainChunkTesselator.EnableSmoothLight = options.Smoothshadows;
            game.d_TerrainChunkTesselator.BlockShadow = options.BlockShadowSave;
            game.ENABLE_LAG = options.Framerate;
            UseFullscreen();
            game.UseVsync();
            UseResolution();
        }
        public void SaveOptions()
        {
            options.Font = (int)game.Font;
            options.Shadows = game.d_CurrentShadows.ShadowsFull;
            options.DrawDistance = (int)game.d_Config3d.viewdistance;
            options.EnableSound = game.AudioEnabled;
            options.Framerate = game.ENABLE_LAG;
            options.Fullscreen = game.d_GlWindow.WindowState == WindowState.Fullscreen;
            options.Smoothshadows = game.d_TerrainChunkTesselator.EnableSmoothLight;

            game.game.platform.SaveOptions(options);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using ManicDigger;
using System.Threading;
using System.Net;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using ManicDigger.Menu;

namespace GameMenu
{
    public interface IForm
    {
        void Render();
        List<Widget> Widgets { get; set; }
    }
    public class Widget
    {
        public string Text = "";
        public RectangleF Rect;
        public string BackgroundImage;
        public string BackgroundImageSelected;
        public System.Threading.ThreadStart Click;
        public float FontSize = 24;
        public bool selected;
        public bool IsTextbox;
        public bool IsNumeric;
        public ThreadStart OnText;
        public bool Visible = true;
        public bool IsPassword = false;
        public Color TextColor = Color.White;

        public bool IsScrollbar = false;
        public int ScrollbarValue = 0;
        public int ScrollbarMax = 0;
        public Color? BackgroundSingleColor;
    }
    public partial class MenuWindow : IMyGameWindow
    {
        [Inject]
        public MainGameWindow d_MainWindow;
        [Inject]
        public IGameExit d_Exit;
        [Inject]
        public The3d d_The3d;
        [Inject]
        public IAudio d_Audio;
        [Inject]
        public IGetFilePath d_GetFile;
        [Inject]
        public FormMainMenu d_FormMainMenu;
        [Inject]
        public FormJoinMultiplayer d_FormJoinMultiplayer;
        [Inject]
        public FormLogin d_FormLogin;
        [Inject]
        public FormSelectWorld d_FormSelectWorld;
        [Inject]
        public FormWorldOptions d_FormWorldOptions;
        [Inject]
        public FormMessageBox d_FormMessageBox;
        [Inject]
        public FormStartServer d_FormStartServer;
        [Inject]
        public FormGameOptions d_FormGameOptions;
        [Inject]
        public FormConnectToIp d_FormConnectToIp;
        [Inject]
        public Game d_Game;
        [Inject]
        public ManicDigger.Renderers.TextRenderer d_TextRenderer;
        public IForm currentForm;
        public int typingfield = -1;
        public ThreadStart OnFinishedTyping;
        public void OnLoad(EventArgs e)
        {
            d_MainWindow.VSync = VSyncMode.On;
            d_MainWindow.WindowState = WindowState.Normal;
            FormMainMenu();
            d_MainWindow.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }
        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                typingfield = -1;
            }
        }
        public void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (typingfield != -1)
            {
                var widget = currentForm.Widgets[typingfield];
                if (e.KeyChar == 8 && widget.Text.Length > 0)//backspace
                {
                    widget.Text = widget.Text.Substring(0, widget.Text.Length - 1);
                }
                else
                {
                    if (e.KeyChar == 8)
                    {
                        return;
                    }
                    if (widget.IsNumeric && !char.IsDigit(e.KeyChar))
                    {
                        return;
                    }
                    if (e.KeyChar == 22)
                    {
                        if (Clipboard.ContainsText())
                        {
                            widget.Text += Clipboard.GetText();
                        }
                        return;
                    }
                    widget.Text += e.KeyChar;
                }
                if (widget.OnText != null)
                {
                    widget.OnText();
                }
            }
        }
        enum MainMenuState
        {
            Main,
            SinglePlayerSelectWorld,
        }
        public void OnResize(EventArgs e)
        {
            ResizeGraphics();
        }
        void ResizeGraphics()
        {
            // Get new window size
            int width = d_MainWindow.Width;
            int height = d_MainWindow.Height;
            float aspect = (float)width / height;

            // Adjust graphics to window size
            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //GLU.Perspective(45.0, aspect, 1.0, 100.0);
            GL.MatrixMode(MatrixMode.Modelview);
        }
        public void OnUpdateFrame(FrameEventArgs e)
        {
            if (d_MainWindow.Keyboard[Key.Escape])
            {
                d_MainWindow.Exit();
            }
        }
        public void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            OrthoMode();
            //background
            UpdateMouse();
            DrawWidgets(currentForm);
            if (d_FormMessageBox.Visible)
            {
                DrawWidgets(d_FormMessageBox);
            }
            else
            {
                currentForm.Render();
            }
            //PerspectiveMode();
            try
            {
                d_MainWindow.SwapBuffers();
            }
            catch { Application.Exit(); } //"failed to swap buffers" crash when exiting program.
        }
        bool mouseleftclick = false;
        bool mouseleftdeclick = false;
        bool wasmouseleft = false;
        bool mouserightclick = false;
        bool mouserightdeclick = false;
        bool wasmouseright = false;
        private void UpdateMouse()
        {
            if (!d_MainWindow.Focused)
            {
                return;
            }
            mouseleftclick = (!wasmouseleft) && Mouse[OpenTK.Input.MouseButton.Left];
            mouserightclick = (!wasmouseright) && Mouse[OpenTK.Input.MouseButton.Right];
            mouseleftdeclick = wasmouseleft && (!Mouse[OpenTK.Input.MouseButton.Left]);
            mouserightdeclick = wasmouseright && (!Mouse[OpenTK.Input.MouseButton.Right]);
            wasmouseleft = Mouse[OpenTK.Input.MouseButton.Left];
            wasmouseright = Mouse[OpenTK.Input.MouseButton.Right];

            if (d_FormMessageBox.Visible)
            {
                UpdateWidgetsMouse(d_FormMessageBox);
            }
            else
            {
                UpdateWidgetsMouse(currentForm);
            }
        }
        private void UpdateWidgetsMouse(IForm form)
        {
            selectedWidget = null;
            float mousex = ((float)Mouse.X / d_MainWindow.Width) * ConstWidth;
            float mousey = ((float)Mouse.Y / d_MainWindow.Height) * ConstHeight;
            for (int i = 0; i < form.Widgets.Count; i++)
            {
                Widget b = form.Widgets[i];
                if (b.Rect.Contains(mousex, mousey))
                {
                    selectedWidget = i;
                }
            }
            if (mouseleftclick && selectedWidget != null)
            {
                var w = form.Widgets[selectedWidget.Value];
                if (w.Click != null)
                {
                    w.Click();
                    d_Audio.Play(d_GetFile.GetFile("destruct.wav"));
                }
                if (w.IsTextbox)
                {
                    typingfield = selectedWidget.Value;
                }
                if (w.IsScrollbar)
                {
                    Widget b = form.Widgets[selectedWidget.Value];
                    float scrollheight=(b.Rect.Height - (40 * 2)) / (b.ScrollbarMax + 1);
                    float scrollpos = b.Rect.Y + ((float)b.ScrollbarValue / (b.ScrollbarMax + 1)) * (b.Rect.Height - 40 * 2) + 40;
                    if (mousey > scrollpos + scrollheight)
                    {
                        b.ScrollbarValue++;
                        if (b.ScrollbarValue > b.ScrollbarMax)
                        {
                            b.ScrollbarValue = b.ScrollbarMax;
                        }
                    }
                    if (mousey < scrollpos)
                    {
                        b.ScrollbarValue--;
                        if (b.ScrollbarValue < 0)
                        {
                            b.ScrollbarValue = 0;
                        }
                    }
                }
            }
        }
        void DrawWidgets(IForm form)
        {
            for (int i = 0; i < form.Widgets.Count; i++)
            {
                Widget b = form.Widgets[i];
                if (!b.Visible)
                {
                    continue;
                }
                if (b.BackgroundSingleColor != null)
                {
                    d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), b.Rect.X, b.Rect.Y, b.Rect.Width, b.Rect.Height,
                        null, b.BackgroundSingleColor.Value);
                }
                if (b.IsScrollbar)
                {
                    d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), b.Rect.X, b.Rect.Y, b.Rect.Width, b.Rect.Height,
                        null, Color.Gray);
                    float scrollpos = b.Rect.Y + ((float)b.ScrollbarValue / (b.ScrollbarMax + 1)) * (b.Rect.Height - 40 * 2) + 40;
                    float scrollheight = (b.Rect.Height - (40 * 2)) / (b.ScrollbarMax + 1);
                    d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), b.Rect.X, scrollpos,
                        b.Rect.Width, scrollheight, null, Color.Black);
                    d_The3d.Draw2dText("^", b.Rect.X, b.Rect.Y, b.FontSize, Color.White);
                    d_The3d.Draw2dText("v", b.Rect.X, b.Rect.Y + b.Rect.Height - 40, b.FontSize, Color.White);
                }
                string img = ((selectedWidget == i || b.selected)
                    && b.BackgroundImageSelected != null)
                    ? b.BackgroundImageSelected : b.BackgroundImage;
                if (img != null)
                {
                    d_The3d.Draw2dBitmapFile(img, b.Rect.X, b.Rect.Y, b.Rect.Width, b.Rect.Height);
                }
                if (b.Text != null)
                {
                    int dx = b.FontSize > 20 ? 49 : 20;
                    string text = b.IsPassword ? PassString(b.Text) : b.Text;
                    if (typingfield == i)
                    {
                        text += "&7|";
                    }
                    d_The3d.Draw2dText(text, b.Rect.X + dx, b.Rect.Y + dx, b.FontSize,
                        (b.BackgroundImage == null && b.selected) ? Color.Red : b.TextColor);
                }
            }
        }
        string PassString(string s)
        {
            string ss = "";
            for (int i = 0; i < s.Length; i++)
            {
                ss += "*";
            }
            return ss;
        }
        public void AddCaption(IForm form, string text)
        {
            form.Widgets.Add(new Widget() { Text = text, FontSize = 48, Rect = new RectangleF(ConstWidth / 2 - 430 * 1.5f / 2, 10, 1024 * 1.5f, 512 * 1.5f) });
        }
        public void AddBackground(List<Widget> widgets)
        {
            //for (int x = 0; x < ConstWidth / 64; x++)
            {
                widgets.Add(new Widget() { BackgroundImage = Path.Combine("gui", "background.png"), Rect = new RectangleF(0, 0, 2048, 2048) });
            }
        }
        public string button4 = Path.Combine("gui", "button4.png");
        public string button4sel = Path.Combine("gui", "button4_sel.png");
        public void AddOkCancel(IForm form, ThreadStart ok, ThreadStart cancel)
        {
            form.Widgets.Add(new Widget()
            {
                BackgroundImage = button4,
                BackgroundImageSelected = button4sel,
                Rect = new RectangleF(350, 1000, 400, 128),
                Text = "OK",
                Click = ok,
            });
            form.Widgets.Add(new Widget()
            {
                BackgroundImage = button4,
                BackgroundImageSelected = button4sel,
                Rect = new RectangleF(850, 1000, 400, 128),
                Text = "Cancel",
                Click = cancel,
            });
        }
        public void MessageBoxYesNo(string text, ThreadStart yes, ThreadStart no)
        {
            d_FormMessageBox.MessageBoxYesNo(text,
                delegate { yes(); d_FormMessageBox.Visible = false; },
                delegate { no(); d_FormMessageBox.Visible = false; });
            d_FormMessageBox.Visible = true;
        }
        List<Widget> optionswidgets = new List<Widget>();

        //List<Button> widgets = new List<Button>();
        int? selectedWidget;
        public int ConstWidth = 1600;
        public int ConstHeight = 1200;
        void OrthoMode()
        {
            //GL.Disable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, ConstWidth, ConstHeight, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
        }
        // Set Up A Perspective View
        void PerspectiveMode()
        {
            // Enter into our projection matrix mode
            GL.MatrixMode(MatrixMode.Projection);
            // Pop off the last matrix pushed on when in projection mode (Get rid of ortho mode)
            GL.PopMatrix();
            // Go back to our model view matrix like normal
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            //GL.LoadIdentity();
            //GL.Enable(EnableCap.DepthTest);
        }
        public void FormMainMenu()
        {
            currentForm = d_FormMainMenu;
        }
        public void FormSelectSinglePlayerWorld()
        {
            afterSelectWorld = delegate
            {
                int id = d_FormSelectWorld.selectedWorld.Value;
                if (string.IsNullOrEmpty(d_Game.GetWorlds()[id]))
                {
                    FormWorldOptions(id);
                    afterWorldOptions = delegate
                    {
                        d_Game.StartSinglePlayer(id);
                    };
                }
                else
                {
                    d_Game.StartSinglePlayer(id);
                }
            };
            currentForm = d_FormSelectWorld;
        }
        public void FormSelectWorld(ThreadStart a)
        {
            afterSelectWorld = delegate
            {
                int id = d_FormSelectWorld.selectedWorld.Value;
                if (string.IsNullOrEmpty(d_Game.GetWorlds()[id]))
                {
                    FormWorldOptions(id);
                    afterWorldOptions = delegate
                    {
                        a();
                    };
                }
                else
                {
                    a();
                }
            };
            currentForm = d_FormSelectWorld;
        }
        public void FormStartServer()
        {
            currentForm = d_FormStartServer;
        }
        private void FormWorldOptions(int id)
        {
            currentForm = d_FormWorldOptions;
            d_FormWorldOptions.worldId = id;
            d_FormWorldOptions.Initialize(); //after worldId set.
        }
        public void FormJoinMultiplayer()
        {
            currentForm = d_FormJoinMultiplayer;
        }
        public void FormLogin()
        {
            currentForm = d_FormLogin;
        }
        public void FormGameOptions()
        {
            currentForm = d_FormGameOptions;
        }
        public void FormConnectToIp()
        {
            currentForm = d_FormConnectToIp;
        }
        public ThreadStart afterSelectWorld = delegate { };
        public ThreadStart afterWorldOptions = delegate { };
        public void OnFocusedChanged(EventArgs e)
        {
        }
        public void OnClosed(EventArgs e)
        {
        }
        public void Exit()
        {
            d_MainWindow.Exit();
            d_Exit.exit = true;
        }
        public OpenTK.Input.KeyboardDevice Keyboard { get { return d_MainWindow.Keyboard; } }
        public OpenTK.Input.MouseDevice Mouse { get { return d_MainWindow.Mouse; } }
    } 
}

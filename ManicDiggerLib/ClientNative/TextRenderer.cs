using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManicDigger.Renderers
{
    public class TextRenderer
    {
        public FontType Font = FontType.Nice;

        public void SetFont(int fontID)
        {
            Font = (FontType)fontID;
        }

        // TODO: Currently broken in mono (Graphics Path).
        private Bitmap defaultFont(Text_ t)
        {
            Font font;
            //outlined font looks smaller
            float oldfontsize = t.fontsize;
            t.fontsize = Math.Max(t.fontsize, 9);
            t.fontsize *= 1.65f;
            try
            {
                font = new Font("Arial", t.fontsize, (FontStyle)t.GetFontStyle());
            }
            catch
            {
                throw new Exception();
            }

            SizeF size;
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    size = g.MeasureString(t.text, font, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                }
            }
            size.Width *= 0.7f;

            SizeF size2 = new SizeF(NextPowerOfTwo((uint)size.Width), NextPowerOfTwo((uint)size.Height));
            if (size2.Width == 0 || size2.Height == 0)
            {
                return new Bitmap(1, 1);
            }
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);
            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                if (size.Width != 0 && size.Height != 0)
                {
                    StringFormat format = StringFormat.GenericTypographic;

                    g2.FillRectangle(new SolidBrush(Color.FromArgb(textalpha, 0, 0, 0)), 0, 0, size.Width, size.Height);
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    Rectangle rect = new Rectangle() { X = 0, Y = 0 };
                    using (GraphicsPath path = GetStringPath(t.text, t.fontsize, rect, font, format))
                    {
                        g2.SmoothingMode = SmoothingMode.AntiAlias;
                        RectangleF off = rect;
                        off.Offset(2, 2);
                        using (GraphicsPath offPath = GetStringPath(t.text, t.fontsize, off, font, format))
                        {
                            Brush b = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
                            g2.FillPath(b, offPath);
                            b.Dispose();
                        }
                        g2.FillPath(new SolidBrush(Color.FromArgb(t.color)), path);
                        g2.DrawPath(Pens.Black, path);
                    }
                }
            }
            return bmp2;
        }

        private Bitmap blackBackgroundFont(Text_ t)
        {
            Font font = new Font("Verdana", t.fontsize, (FontStyle)t.GetFontStyle());
            SizeF size;
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    size = g.MeasureString(t.text, font, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                }
            }

            SizeF size2 = new SizeF(NextPowerOfTwo((uint)size.Width), NextPowerOfTwo((uint)size.Height));
            if (size2.Width == 0 || size2.Height == 0)
            {
                return new Bitmap(1, 1);
            }
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);
            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                if (size.Width != 0 && size.Height != 0)
                {
                    g2.FillRectangle(new SolidBrush(Color.Black), 0, 0, size.Width, size.Height);
                    g2.DrawString(t.text, font, new SolidBrush(Color.FromArgb(t.color)), 0, 0);
                }
            }
            return bmp2;
        }

        private Bitmap simpleFont(Text_ t)
        {
            float fontsize = t.fontsize;
            Font font;
            fontsize = Math.Max(t.fontsize, 9);
            fontsize *= 1.1f;
            try
            {
                font = new Font("Arial", fontsize, (FontStyle)t.GetFontStyle());
            }
            catch
            {
                throw new Exception();
            }

            SizeF size;
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    size = g.MeasureString(t.text, font, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                }
            }

            SizeF size2 = new SizeF(NextPowerOfTwo((uint)size.Width), NextPowerOfTwo((uint)size.Height));
            if (size2.Width == 0 || size2.Height == 0)
            {
                return new Bitmap(1, 1);
            }
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);

            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                if (size.Width != 0 && size.Height != 0)
                {
                    g2.SmoothingMode = SmoothingMode.AntiAlias;
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g2.DrawString(t.text, font, new SolidBrush(Color.FromArgb(t.color)), 0, 0);
                }
            }
            return bmp2;
        }

        private Bitmap niceFont(Text_ t)
        {
            float fontsize = t.fontsize;
            Font font;
            fontsize = Math.Max(fontsize, 9);
            fontsize *= 1.1f;
            try
            {
                font = new Font(t.GetFontFamily(), fontsize, (FontStyle)t.GetFontStyle());
            }
            catch
            {
                throw new Exception();
            }

            SizeF size;
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    size = g.MeasureString(t.text, font, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                }
            }

            SizeF size2 = new SizeF(NextPowerOfTwo((uint)size.Width), NextPowerOfTwo((uint)size.Height));
            if (size2.Width == 0 || size2.Height == 0)
            {
                return new Bitmap(1, 1);
            }
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);
            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                if (size.Width != 0 && size.Height != 0)
                {
                    g2.SmoothingMode = SmoothingMode.AntiAlias;
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    Matrix mx = new Matrix(1f, 0, 0, 1f, 1, 1);
                    g2.Transform = mx;
                    g2.DrawString(t.text, font, new SolidBrush(Color.FromArgb(128, Color.Black)), 0, 0);
                    g2.ResetTransform();

                    g2.DrawString(t.text, font, new SolidBrush(Color.FromArgb(t.color)), 0, 0);
                }
            }
            return bmp2;
        }

        public virtual Bitmap MakeTextTexture(Text_ t)
        {








            switch (this.Font)
            {
                case FontType.Default:
                    return this.defaultFont(t);
                case FontType.BlackBackground:
                    return this.blackBackgroundFont(t);
                case FontType.Simple:
                    return this.simpleFont(t);
                case FontType.Nice:
                    return this.niceFont(t);
                default:
                    return this.defaultFont(t);
            }
        }
        GraphicsPath GetStringPath(string s, float emSize, RectangleF rect, Font font, StringFormat format)
        {
            GraphicsPath path = new GraphicsPath();
            // TODO: Bug in Mono. Returns incomplete list of points / cuts string.
            path.AddString(s, font.FontFamily, (int)font.Style, emSize, rect, format);
            return path;
        }
        int textalpha = 0;
        protected uint NextPowerOfTwo(uint x)
        {
            x--;
            x |= x >> 1;  // handle  2 bit numbers
            x |= x >> 2;  // handle  4 bit numbers
            x |= x >> 4;  // handle  8 bit numbers
            x |= x >> 8;  // handle 16 bit numbers
            x |= x >> 16; // handle 32 bit numbers
            x++;
            return x;
        }


        protected int HexToInt(char c)
        {
            if (c == '0') { return 0; }
            if (c == '1') { return 1; }
            if (c == '2') { return 2; }
            if (c == '3') { return 3; }
            if (c == '4') { return 4; }
            if (c == '5') { return 5; }
            if (c == '6') { return 6; }
            if (c == '7') { return 7; }
            if (c == '8') { return 8; }
            if (c == '9') { return 9; }
            if (c == 'a') { return 10; }
            if (c == 'b') { return 11; }
            if (c == 'c') { return 12; }
            if (c == 'd') { return 13; }
            if (c == 'e') { return 14; }
            if (c == 'f') { return 15; }
            return -1;
        }
        public bool NewFont = true;

        public virtual SizeF MeasureTextSize(string text, float fontsize)
        {
            string text2 = "";
            fontsize = Math.Max(fontsize, 9);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '&')
                {
                    if (i + 1 < text.Length && HexToInt(text[i + 1]) != -1)
                    {
                        //Skip color codes when calculating text length
                        i++;
                    }
                    else
                    {
                        text2 += text[i];
                    }
                }
                else
                {
                    text2 += text[i];
                }
            }
            using (Font font = new Font("Verdana", fontsize))
            {
                using (Bitmap bmp = new Bitmap(1, 1))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        return g.MeasureString(text2, font, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                    }
                }
            }
        }

        public virtual SizeF MeasureTextSize(string text, Font font)
        {
            string text2 = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '&')
                {
                    if (i + 1 < text.Length && HexToInt(text[i + 1]) != -1)
                    {
                        //Skip color codes when calculating text length
                        i++;
                    }
                    else
                    {
                        text2 += text[i];
                    }
                }
                else
                {
                    text2 += text[i];
                }
            }
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    return g.MeasureString(text2, font, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                }
            }
        }
    }
}

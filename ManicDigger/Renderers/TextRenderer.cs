using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManicDigger
{
    public class TextPart
    {
        public Color color;
        public string text;
    }
    public struct Text
    {
        public string text;
        public float fontsize;
        public Color color;
        public override int GetHashCode()
        {
            return ("" + text.GetHashCode() + fontsize.GetHashCode() + color.GetHashCode()).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Text)
            {
                Text other = (Text)obj;
                return other.text.Equals(this.text)
                    && other.fontsize.Equals(this.fontsize)
                    && other.color.Equals(this.color);
            }
            return base.Equals(obj);
        }
    }
    public class TextRenderer
    {
        public Bitmap MakeTextTexture(Text t)
        {
            Font font;
            if (NewFont)
            {
                //outlined font looks smaller
                t.fontsize = Math.Max(t.fontsize, 9);
                t.fontsize *= 1.65f;
                font = new Font("Arial", t.fontsize, FontStyle.Bold);
            }
            else
            {
                font = new Font("Verdana", t.fontsize);
            }
            var parts = DecodeColors(t.text, t.color);
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF>();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        SizeF size = g.MeasureString(parts[i].text, font);
                        if (size.Width == 0 || size.Height == 0)
                        {
                            continue;
                        }
                        if (NewFont)
                        {
                            size.Width *= 0.7f;
                        }
                        totalwidth += size.Width;
                        totalheight = Math.Max(totalheight, size.Height);
                        sizes.Add(size);
                    }
                }
            }
            SizeF size2 = new SizeF(NextPowerOfTwo((uint)totalwidth), NextPowerOfTwo((uint)totalheight));
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);
            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                float currentwidth = 0;
                for (int i = 0; i < parts.Count; i++)
                {
                    SizeF sizei = sizes[i];
                    if (sizei.Width == 0 || sizei.Height == 0)
                    {
                        continue;
                    }
                    if (NewFont)
                    {
                        StringFormat format = StringFormat.GenericTypographic;

                        g2.FillRectangle(new SolidBrush(Color.FromArgb(textalpha, 0, 0, 0)), currentwidth, 0, sizei.Width, sizei.Height);
                        g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        //g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color), currentwidth, 0);
                        Rectangle rect = new Rectangle() { X = (int)currentwidth, Y = 0 };
                        using (GraphicsPath path = GetStringPath(parts[i].text, t.fontsize, rect, font, format))
                        {
                            g2.SmoothingMode = SmoothingMode.AntiAlias;
                            RectangleF off = rect;
                            off.Offset(2, 2);
                            using (GraphicsPath offPath = GetStringPath(parts[i].text, t.fontsize, off, font, format))
                            {
                                Brush b = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
                                g2.FillPath(b, offPath);
                                b.Dispose();
                            }
                            g2.FillPath(new SolidBrush(parts[i].color), path);
                            g2.DrawPath(Pens.Black, path);
                        }
                    }
                    else
                    {
                        g2.FillRectangle(new SolidBrush(Color.Black), currentwidth, 0, sizei.Width, sizei.Height);
                        g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color), currentwidth, 0);
                    }
                    currentwidth += sizei.Width;
                }
            }
            return bmp2;
        }
        GraphicsPath GetStringPath(string s, float emSize, RectangleF rect, Font font, StringFormat format)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddString(s, font.FontFamily, (int)font.Style, emSize, rect, format);
            return path;
        }
        int textalpha = 0;
        private uint NextPowerOfTwo(uint x)
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
        public List<TextPart> DecodeColors(string s, Color defaultcolor)
        {
            List<TextPart> parts = new List<TextPart>();
            int i = 0;
            Color currentcolor = defaultcolor;
            string currenttext = "";
            for (; ; )
            {
                if (i >= s.Length)
                {
                    if (currenttext != "")
                    {
                        parts.Add(new TextPart() { text = currenttext, color = currentcolor });
                    }
                    break;
                }
                if (s[i] == '&')
                {
                    if (i + 1 < s.Length)
                    {
                        int? color = HexToInt(s[i + 1]);
                        if (color != null)
                        {
                            if (currenttext != "")
                            {
                                parts.Add(new TextPart() { text = currenttext, color = currentcolor });
                            }
                            currenttext = "";
                            currentcolor = GetColor(color.Value);
                            i++;
                            goto next;
                        }
                    }
                    else
                    {
                    }
                }
                currenttext += s[i];
            next:
                i++;
            }
            return parts;
        }
        private Color GetColor(int currentcolor)
        {
            switch (currentcolor)
            {
                case 0: { return Color.FromArgb(0, 0, 0); }
                case 1: { return Color.FromArgb(0, 0, 191); }
                case 2: { return Color.FromArgb(0, 191, 0); }
                case 3: { return Color.FromArgb(0, 191, 191); }
                case 4: { return Color.FromArgb(191, 0, 0); }
                case 5: { return Color.FromArgb(191, 0, 191); }
                case 6: { return Color.FromArgb(191, 191, 0); }
                case 7: { return Color.FromArgb(191, 191, 191); }
                case 8: { return Color.FromArgb(40, 40, 40); }
                case 9: { return Color.FromArgb(64, 64, 255); }
                case 10: { return Color.FromArgb(64, 255, 64); }
                case 11: { return Color.FromArgb(64, 255, 255); }
                case 12: { return Color.FromArgb(255, 64, 64); }
                case 13: { return Color.FromArgb(255, 64, 255); }
                case 14: { return Color.FromArgb(255, 255, 64); }
                case 15: { return Color.FromArgb(255, 255, 255); }
                default: throw new Exception();
            }
        }
        int? HexToInt(char c)
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
            return null;
        }
        public bool NewFont = true;
    }
}

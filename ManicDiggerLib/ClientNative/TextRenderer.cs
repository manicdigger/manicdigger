using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManicDigger.Renderers
{
    public class TextPart
    {
        public Color color;
        public string text;
    }

    public class TextRenderer
    {
        public FontType Font = FontType.Nice;

        public virtual Bitmap MakeTextTexture(Text_ t, FontCi font)
        {
            var parts = DecodeColors(t.text, Color.FromArgb(t.color));
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF>();
            using(Bitmap bmp = new Bitmap(1, 1))
            {
                using (Font f = new Font(font.family, font.size, (FontStyle)font.style))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        for (int i = 0; i < parts.Count; i++)
                        {
                            SizeF size = g.MeasureString(parts[i].text, f, new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                            if (size.Width == 0 || size.Height == 0)
                            {
                                continue;
                            }
                            totalwidth += size.Width;
                            totalheight = Math.Max(totalheight, size.Height);
                            sizes.Add(size);
                        }
                    }
                }
            }
            SizeF size2 = new SizeF(NextPowerOfTwo((uint)totalwidth), NextPowerOfTwo((uint)totalheight));
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);
            using(Graphics g2 = Graphics.FromImage(bmp2))
            {
                using (Font f = new Font(font.family, font.size, (FontStyle)font.style))
                {
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    float currentwidth = 0;
                    for (int i = 0; i < parts.Count; i++)
                    {
                        SizeF sizei = sizes[i];
                        if (sizei.Width == 0 || sizei.Height == 0)
                        {
                            continue;
                        }
                        g2.DrawString(parts[i].text, f, new SolidBrush(parts[i].color), currentwidth, 0);
                        currentwidth += sizei.Width;
                    }
                }
            }
            return bmp2;
        }

        // TODO: Currently broken in mono (Graphics Path).
        private Bitmap defaultFont(Text_ t)
        {
            Font font;
            //outlined font looks smaller
            float oldfontsize = t.fontsize;
            t.fontsize = Math.Max (t.fontsize, 9);
            t.fontsize *= 1.65f;
            try
            {
                font = new Font ("Arial", t.fontsize, FontStyle.Bold);
            }
            catch
            {
                throw new Exception();
            }
            var parts = DecodeColors (t.text, Color.FromArgb(t.color));
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF> ();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        SizeF size = g.MeasureString (parts [i].text, font, new PointF(0,0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                        if (size.Width == 0 || size.Height == 0)
                        {
                            continue;
                        }
                        size.Width *= 0.7f;
                        totalwidth += size.Width;
                        totalheight = Math.Max (totalheight, size.Height);
                        sizes.Add (size);
                    }
                }
            }
            SizeF size2 = new SizeF (NextPowerOfTwo ((uint)totalwidth), NextPowerOfTwo ((uint)totalheight));
            Bitmap bmp2 = new Bitmap ((int)size2.Width, (int)size2.Height);
            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                float currentwidth = 0;
                for (int i = 0; i < parts.Count; i++)
                {
                    SizeF sizei = sizes [i];
                    if (sizei.Width == 0 || sizei.Height == 0)
                    {
                        continue;
                    }
                    StringFormat format = StringFormat.GenericTypographic;

                    g2.FillRectangle (new SolidBrush (Color.FromArgb (textalpha, 0, 0, 0)), currentwidth, 0, sizei.Width, sizei.Height);
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    Rectangle rect = new Rectangle () { X = (int)currentwidth, Y = 0 };
                    using (GraphicsPath path = GetStringPath(parts[i].text, t.fontsize, rect, font, format))
                    {
                        g2.SmoothingMode = SmoothingMode.AntiAlias;
                        RectangleF off = rect;
                        off.Offset (2, 2);
                        using (GraphicsPath offPath = GetStringPath(parts[i].text, t.fontsize, off, font, format))
                        {
                            Brush b = new SolidBrush (Color.FromArgb (100, 0, 0, 0));
                            g2.FillPath (b, offPath);
                            b.Dispose ();
                        }
                        g2.FillPath (new SolidBrush (parts [i].color), path);
                        g2.DrawPath (Pens.Black, path);
                    }
                    currentwidth += sizei.Width;
                }
            }
            return bmp2;
        }

		private Bitmap blackBackgroundFont(Text_ t)
        {
            Font font = new Font("Verdana", t.fontsize);
            var parts = DecodeColors(t.text, Color.FromArgb(t.color));
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF>();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        SizeF size = g.MeasureString(parts[i].text, font, new PointF(0,0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                        if (size.Width == 0 || size.Height == 0)
                        {
                            continue;
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
                    g2.FillRectangle(new SolidBrush(Color.Black), currentwidth, 0, sizei.Width, sizei.Height);
                    g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color), currentwidth, 0);
                    currentwidth += sizei.Width;
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
               font = new Font ("Arial", fontsize, FontStyle.Bold);
            }
            catch
            {
                throw new Exception();
            }

            var parts = DecodeColors(t.text, Color.FromArgb(t.color));
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF>();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        SizeF size = g.MeasureString(parts[i].text, font, new PointF(0,0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                        if (size.Width == 0 || size.Height == 0)
                        {
                            continue;
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
                    g2.SmoothingMode = SmoothingMode.AntiAlias;
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color), currentwidth, 0);
                    currentwidth += sizei.Width;
                }
            }
            return bmp2;
        }

        private Bitmap niceFont(Text_ t)
        {
            float fontsize = t.fontsize;
            Font font;
            fontsize = Math.Max (fontsize, 9);
            fontsize *= 1.1f;
            try
            {
               font = new Font ("Arial", fontsize, FontStyle.Bold);
            }
            catch
            {
                throw new Exception();
            }

            var parts = DecodeColors(t.text, Color.FromArgb(t.color));
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF>();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        SizeF size = g.MeasureString(parts[i].text, font, new PointF(0,0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                        if (size.Width == 0 || size.Height == 0)
                        {
                            continue;
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

                    /*
                    RectangleF rect = new RectangleF(currentwidth, 0, sizei.Width, sizei.Height);
                    StringFormat format = StringFormat.GenericTypographic;
                    float emSize = g2.DpiY * font.SizeInPoints / 72;

                    using (GraphicsPath path = GetStringPath(parts[i].text, emSize, rect, font, format))
                    {   
                        g2.SmoothingMode = SmoothingMode.AntiAlias;
                        g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        g2.FillPath(new SolidBrush(parts[i].color), path);
                        g2.DrawPath(new Pen(Color.Black, 1), path);
                    }
                    */

                    g2.SmoothingMode = SmoothingMode.AntiAlias;
                    g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    Matrix mx = new Matrix(1f,0,0,1f,1,1);
                    g2.Transform = mx;
                    g2.DrawString(parts[i].text, font, new SolidBrush( Color.FromArgb(128, Color.Black)), currentwidth, 0);
                    g2.ResetTransform();

                    g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color), currentwidth, 0);
                    currentwidth += sizei.Width;
                }
            }
            return bmp2;
        }

        public virtual Bitmap MakeTextTexture(Text_ t)
        {
            switch(this.Font)
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
        protected Color GetColor(int currentcolor)
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
        protected int? HexToInt(char c)
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

       public virtual SizeF MeasureTextSize(string text, float fontsize)
       {
           fontsize = Math.Max(fontsize, 9);
            using(Font font = new Font("Verdana", fontsize))
            {
                using(Bitmap bmp = new Bitmap(1, 1))
                {
                    using(Graphics g = Graphics.FromImage(bmp))
                    {
                        return g.MeasureString(text, font, new PointF(0,0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                    }
                }
            }
        }

        public virtual SizeF MeasureTextSize(string text, Font font)
        {
            using(Bitmap bmp = new Bitmap(1, 1))
            {
                using(Graphics g = Graphics.FromImage(bmp))
                {
                    return g.MeasureString(text, font, new PointF(0,0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
                }
            }
        }
    }
}

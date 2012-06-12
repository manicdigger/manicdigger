using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManicDigger.Renderers
{

    public class MonospacedTextRenderer : TextRenderer
    {
        const string FONT_FAMILY = "Courier New";
        public override Bitmap MakeTextTexture(Text t)
        {
            Font font;
            font = new Font(FONT_FAMILY, t.fontsize);
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
                    g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color), currentwidth, 0);
                    currentwidth += sizei.Width;
                }
            }
            return bmp2;
        }

        public override SizeF MeasureTextSize(string text, float fontsize)
        {
           using (Font font = new Font(FONT_FAMILY, fontsize))
           {
              using (Bitmap bmp = new Bitmap(1, 1))
              {
                 using (Graphics g = Graphics.FromImage(bmp))
                 {
                    return g.MeasureString(text, font);
                 }
              }
           }
        }
    }
}

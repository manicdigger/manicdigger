using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace ManicDigger.Renderers
{
	public class TextRenderer
	{
		public FontType Font = FontType.Nice;

		public void SetFont(int fontID)
		{
			Font = (FontType)fontID;
		}

		private Bitmap defaultFont(Text_ t)
		{
			Font font;
			try
			{
				font = new Font(t.GetFontFamily(), t.GetFontSize() * 1.25f, (FontStyle)t.GetFontStyle());
			}
			catch
			{
				throw new Exception();
			}

			SizeF size = MeasureTextSize(t.GetText(), font);

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
					using (GraphicsPath path = GetStringPath(t.GetText(), rect, font, format))
					{
						g2.SmoothingMode = SmoothingMode.AntiAlias;
						RectangleF off = rect;
						off.Offset(2, 2);
						using (GraphicsPath offPath = GetStringPath(t.GetText(), off, font, format))
						{
							Brush b = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
							g2.FillPath(b, offPath);
							b.Dispose();
						}
						g2.FillPath(new SolidBrush(Color.FromArgb(t.GetColor())), path);
						g2.DrawPath(Pens.Black, path);
					}
				}
			}
			return bmp2;
		}

		private Bitmap blackBackgroundFont(Text_ t)
		{
			Font font;
			try
			{
				font = new Font(t.GetFontFamily(), t.GetFontSize(), (FontStyle)t.GetFontStyle());
			}
			catch
			{
				throw new Exception();
			}

			SizeF size = MeasureTextSize(t.GetText(), font);
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
					// Draw black background
					g2.FillRectangle(new SolidBrush(Color.Black), 0, 0, size.Width, size.Height);
					// Draw text
					g2.DrawString(t.GetText(), font, new SolidBrush(Color.FromArgb(t.GetColor())), 0, 0, StringFormat.GenericTypographic);
				}
			}
			return bmp2;
		}

		private Bitmap simpleFont(Text_ t)
		{
			Font font;
			try
			{
				font = new Font(t.GetFontFamily(), t.GetFontSize(), (FontStyle)t.GetFontStyle());
			}
			catch
			{
				throw new Exception();
			}

			SizeF size = MeasureTextSize(t.GetText(), font);
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
					// Draw text
					g2.DrawString(t.GetText(), font, new SolidBrush(Color.FromArgb(t.GetColor())), 0, 0, StringFormat.GenericTypographic);
				}
			}
			return bmp2;
		}

		private Bitmap niceFont(Text_ t)
		{
			Font font;
			try
			{
				font = new Font(t.GetFontFamily(), t.GetFontSize(), (FontStyle)t.GetFontStyle());
			}
			catch
			{
				throw new Exception();
			}

			SizeF size = MeasureTextSize(t.GetText(), font);
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
#if DEBUG // Display measured text sizes
					g2.DrawRectangle(new Pen(Color.FromArgb(255, 0, 255, 0)), 0, 0, (int)size.Width, (int)size.Height);
					g2.DrawRectangle(new Pen(Color.FromArgb(255, 255, 255, 0)), 0, 0, (int)size2.Width - 1, (int)size2.Height - 1);
#endif
					// Draw text shadow
					Matrix mx = new Matrix(1f, 0, 0, 1f, 1, 1);
					g2.Transform = mx;
					g2.DrawString(t.GetText(), font, new SolidBrush(Color.FromArgb(128, Color.Black)), 0, 0, StringFormat.GenericTypographic);
					g2.ResetTransform();
					// Draw text
					g2.DrawString(t.GetText(), font, new SolidBrush(Color.FromArgb(t.GetColor())), 0, 0, StringFormat.GenericTypographic);
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

		GraphicsPath GetStringPath(string s, RectangleF rect, Font font, StringFormat format)
		{
			GraphicsPath path = new GraphicsPath();
			path.AddString(s, font.FontFamily, (int)font.Style, font.Size, rect, format);
			return path;
		}
		int textalpha = 0;

		public virtual SizeF MeasureTextSize(string text, FontCi fontci)
		{
			using (Font font = new Font(fontci.GetFontFamily(), fontci.GetFontSize(), (FontStyle)fontci.GetFontStyle()))
			{
				return MeasureTextSize(text, font);
			}
		}

		public virtual SizeF MeasureTextSize(string text, Font font)
		{
			using (Bitmap bmp = new Bitmap(1, 1))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					StringFormat tmpFormat = StringFormat.GenericTypographic;
					tmpFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
					return g.MeasureString(StripColorCodes(text), font, new PointF(0, 0), tmpFormat);
				}
			}
		}

		#region Helpers
		/// <summary>
		/// Strips color codes in format '&x' from a given string.
		/// </summary>
		/// <param name="text">The text to process</param>
		/// <returns>The given text without any color codes</returns>
		string StripColorCodes(string text)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '&')
				{
					if (i + 1 < text.Length && isCharHex(text[i + 1]))
					{
						i++;
					}
					else
					{
						builder.Append(text[i]);
					}
				}
				else
				{
					builder.Append(text[i]);
				}
			}
			return builder.ToString();
		}

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

		protected bool isCharHex(char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
		}
		#endregion
	}
}

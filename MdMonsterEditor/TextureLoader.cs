using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.IO;

namespace ManicDigger
{
	public class TextureLoader
	{
		public Config3d d_Config3d;
		public bool ALLOW_NON_POWER_OF_TWO = false;

		//http://www.opentk.com/doc/graphics/textures/loading
		public int LoadTexture(Bitmap bmpArg)
		{
			Bitmap bmp = bmpArg;
			bool convertedbitmap = false;
			if ((!ALLOW_NON_POWER_OF_TWO) &&
			    (!(BitTools.IsPowerOfTwo(bmp.Width) && BitTools.IsPowerOfTwo(bmp.Height))))
			{
				Bitmap bmp2 = new Bitmap(BitTools.NextPowerOfTwo(bmp.Width),
				                         BitTools.NextPowerOfTwo(bmp.Height));
				using (Graphics g = Graphics.FromImage(bmp2))
				{
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					g.DrawImage(bmp, 0, 0, bmp2.Width, bmp2.Height);
				}
				convertedbitmap = true;
				bmp = bmp2;
			}
			GL.Enable(EnableCap.Texture2D);
			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);
			if (!d_Config3d.GetEnableMipmaps())
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			}
			else
			{
				//GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); //DOES NOT WORK ON ATI GRAPHIC CARDS
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1); //DOES NOT WORK ON ???
				int[] MipMapCount = new int[1];
				GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMaxLevel, out MipMapCount[0]);
				if (MipMapCount[0] == 0)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
				}
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 4);
			}
			BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
			              OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

			bmp.UnlockBits(bmp_data);

			GL.Enable(EnableCap.DepthTest);

			if (d_Config3d.GetEnableTransparency())
			{
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
			}


			if (d_Config3d.GetEnableTransparency())
			{
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
				//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
				//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new Color4(0, 0, 0, byte.MaxValue));
			}

			if (convertedbitmap)
			{
				bmp.Dispose();
			}
			return id;
		}
	}
}

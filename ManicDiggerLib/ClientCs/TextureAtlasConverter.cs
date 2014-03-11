using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ManicDigger
{
    public class TextureAtlasConverter
    {
        public delegate T Factory<T>();
        public Factory<IFastBitmap> d_FastBitmapFactory { get; set; }
        //tiles = 16 means 16 x 16 atlas
        public List<Bitmap> Atlas2dInto1d(Bitmap atlas2d, int tiles, int atlassizezlimit)
        {
            IFastBitmap orig = d_FastBitmapFactory();
            orig.bmp = atlas2d;

            int tilesize = atlas2d.Width / tiles;

            int atlasescount = Math.Max(1, (tiles * tiles * tilesize) / atlassizezlimit);
            List<Bitmap> atlases = new List<Bitmap>();

            orig.Lock();

            //256 x 1
            IFastBitmap atlas1d = null;

            for (int i = 0; i < tiles * tiles; i++)
            {
                int x = i % tiles;
                int y = i / tiles;
                int tilesinatlas = (tiles * tiles / atlasescount);
                if (i % tilesinatlas == 0)
                {
                    if (atlas1d != null)
                    {
                        atlas1d.Unlock();
                        atlases.Add(atlas1d.bmp);
                    }
                    atlas1d = d_FastBitmapFactory();
                    atlas1d.bmp = new Bitmap(tilesize, atlassizezlimit);
                    atlas1d.Lock();
                }
                for (int xx = 0; xx < tilesize; xx++)
                {
                    for (int yy = 0; yy < tilesize; yy++)
                    {
                        int c = orig.GetPixel(x * tilesize + xx, y * tilesize + yy);
                        atlas1d.SetPixel(xx, (i % tilesinatlas) * tilesize + yy, c);
                    }
                }
            }
            atlas1d.Unlock();
            atlases.Add(atlas1d.bmp);
            orig.Unlock();
            return atlases;
        }
        public static void AddTexture(Bitmap atlas, Bitmap bmp, Point pos, Size size)
        {
            if (bmp.Size == new Size(1, 1))
            {
                Bitmap bmp2 = new Bitmap(GlobalVar.MAX_BLOCKTYPES, GlobalVar.MAX_BLOCKTYPES);
                Graphics.FromImage(bmp2).Clear(bmp.GetPixel(0, 0));
                bmp = bmp2;
            }
            bmp = new Bitmap(bmp, size);
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    atlas.SetPixel(pos.X + x, pos.Y + y, bmp.GetPixel(x, y));
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace ManicDigger.Gui
{
    public class HudMaterialSelector
    {
        [Inject]
        public IViewportSize d_ViewportSize;
        [Inject]
        public ManicDiggerGameWindow d_GameWindow;

        public void DrawMaterialSelector()
        {
            int singlesize = 40;
            for (int i = 0; i < 10; i++)
            {
                int x = xcenter(singlesize * 10) + i * singlesize;
                int y = d_ViewportSize.Height - 100;
                d_GameWindow.d_The3d.Draw2dTexture(d_GameWindow.d_TerrainTextures.terrainTexture, x, y, singlesize, singlesize,
                        d_GameWindow.d_Data.TextureIdForInventory[(int)d_GameWindow.MaterialSlots[i]]);

                if (d_GameWindow.ENABLE_FINITEINVENTORY)
                {
                    int amount = d_GameWindow.d_Game.FiniteInventoryAmount((int)d_GameWindow.MaterialSlots[i]);
                    d_GameWindow.d_The3d.Draw2dText("" + amount, x, y, 8, null);
                }
            }
            d_GameWindow.d_The3d.Draw2dBitmapFile(Path.Combine("gui", "activematerial.png"),
                xcenter(singlesize * 10) + d_GameWindow.activematerial * singlesize, d_ViewportSize.Height - 100,
                d_GameWindow.NextPowerOfTwo((uint)singlesize), d_GameWindow.NextPowerOfTwo((uint)singlesize));
            if (d_GameWindow.ENABLE_FINITEINVENTORY)
            {
                int inventoryload = 0;
                foreach (var k in d_GameWindow.FiniteInventory)
                {
                    inventoryload += k.Value;
                }
                float inventoryloadratio = (float)inventoryload / d_GameWindow.d_Game.FiniteInventoryMax;
                d_GameWindow.d_The3d.Draw2dTexture(d_GameWindow.d_The3d.WhiteTexture(), xcenter(100), d_ViewportSize.Height - 120, 100, 10, null, Color.Black);
                Color c;
                if (inventoryloadratio < 0.5)
                {
                    c = Color.Green;
                }
                else if (inventoryloadratio < 0.75)
                {
                    c = Color.Yellow;
                }
                else
                {
                    c = Color.Red;
                }
                d_GameWindow.d_The3d.Draw2dTexture(d_GameWindow.d_The3d.WhiteTexture(), xcenter(100), d_ViewportSize.Height - 120, inventoryloadratio * 100, 10, null, c);
            }
        }

        private int xcenter(float width)
        {
            return (int)(d_ViewportSize.Width / 2 - width / 2);
        }
        private int ycenter(float height)
        {
            return (int)(d_ViewportSize.Height / 2 - height / 2);
        }
    }
}

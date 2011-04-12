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
        public IViewportSize viewportsize;
        [Inject]
        public ManicDiggerGameWindow gameWindow;

        public void DrawMaterialSelector()
        {
            int singlesize = 40;
            for (int i = 0; i < 10; i++)
            {
                int x = xcenter(singlesize * 10) + i * singlesize;
                int y = viewportsize.Height - 100;
                gameWindow.the3d.Draw2dTexture(gameWindow.terrainTextures.terrainTexture, x, y, singlesize, singlesize,
                        gameWindow.data.TextureIdForInventory[(int)gameWindow.MaterialSlots[i]]);

                if (gameWindow.ENABLE_FINITEINVENTORY)
                {
                    int amount = gameWindow.game.FiniteInventoryAmount((int)gameWindow.MaterialSlots[i]);
                    gameWindow.the3d.Draw2dText("" + amount, x, y, 8, null);
                }
            }
            gameWindow.the3d.Draw2dBitmapFile(Path.Combine("gui", "activematerial.png"),
                xcenter(singlesize * 10) + gameWindow.activematerial * singlesize, viewportsize.Height - 100,
                gameWindow.NextPowerOfTwo((uint)singlesize), gameWindow.NextPowerOfTwo((uint)singlesize));
            if (gameWindow.ENABLE_FINITEINVENTORY)
            {
                int inventoryload = 0;
                foreach (var k in gameWindow.FiniteInventory)
                {
                    inventoryload += k.Value;
                }
                float inventoryloadratio = (float)inventoryload / gameWindow.game.FiniteInventoryMax;
                gameWindow.the3d.Draw2dTexture(gameWindow.the3d.WhiteTexture(), xcenter(100), viewportsize.Height - 120, 100, 10, null, Color.Black);
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
                gameWindow.the3d.Draw2dTexture(gameWindow.the3d.WhiteTexture(), xcenter(100), viewportsize.Height - 120, inventoryloadratio * 100, 10, null, c);
            }
        }

        private int xcenter(float width)
        {
            return (int)(viewportsize.Width / 2 - width / 2);
        }
        private int ycenter(float height)
        {
            return (int)(viewportsize.Height / 2 - height / 2);
        }
    }
}

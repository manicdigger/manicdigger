using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace ManicDigger.Gui
{
    public class HudInventory
    {
        [Inject]
        public ManicDiggerGameWindow w;
        [Inject]
        public IViewportSize viewportsize;
        [Inject]
        public IGameData data;

        private int inventoryselectedx;
        private int inventoryselectedy;
        private int inventorysize;
        private int inventorysinglesize = 40;

        public void InventoryMouse(Point mouse, ref bool isLMB)
        {
            int invstartx = xcenter(inventorysinglesize * inventorysize);
            int invstarty = ycenter(inventorysinglesize * inventorysize);
            if (mouse.X > invstartx && mouse.X < invstartx + inventorysinglesize * inventorysize)
            {
                if (mouse.Y > invstarty && mouse.Y < invstarty + inventorysinglesize * inventorysize)
                {
                    inventoryselectedx = (mouse.X - invstartx) / inventorysinglesize;
                    inventoryselectedy = (mouse.Y - invstarty) / inventorysinglesize;
                }
            }
            if (isLMB)
            {
                var sel = InventoryGetSelected();
                if (sel != null)
                {
                    w.MaterialSlots[w.activematerial] = sel.Value;
                    w.GuiStateBackToGame();
                }
                isLMB = false;
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
        public void InventoryKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == w.GetKey(OpenTK.Input.Key.Escape))
            {
                w.GuiStateBackToGame();
            }
            Direction4? dir = null;
            if (e.Key == w.GetKey(OpenTK.Input.Key.Left)) { dir = Direction4.Left; }
            if (e.Key == w.GetKey(OpenTK.Input.Key.Right)) { dir = Direction4.Right; }
            if (e.Key == w.GetKey(OpenTK.Input.Key.Up)) { dir = Direction4.Up; }
            if (e.Key == w.GetKey(OpenTK.Input.Key.Down)) { dir = Direction4.Down; }
            if (dir != null)
            {
                InventorySelectionMove(dir.Value);
            }
            if (e.Key == w.GetKey(OpenTK.Input.Key.Enter) || e.Key == w.GetKey(OpenTK.Input.Key.KeypadEnter))
            {
                var sel = InventoryGetSelected();
                if (sel != null)
                {
                    w.MaterialSlots[w.activematerial] = sel.Value;
                    w.GuiStateBackToGame();
                }
            }
            w.HandleMaterialKeys(e);
        }
        void InventorySelectionMove(Direction4 dir)
        {
            if (dir == Direction4.Left) { inventoryselectedx--; }
            if (dir == Direction4.Right) { inventoryselectedx++; }
            if (dir == Direction4.Up) { inventoryselectedy--; }
            if (dir == Direction4.Down) { inventoryselectedy++; }
            inventoryselectedx = MyMath.Clamp(inventoryselectedx, 0, inventorysize - 1);
            inventoryselectedy = MyMath.Clamp(inventoryselectedy, 0, inventorysize - 1);
        }
        int? InventoryGetSelected()
        {
            int id = inventoryselectedx + (inventoryselectedy * inventorysize);
            if (id >= Buildable.Count)
            {
                return null;
            }
            return Buildable[id];
        }
        List<int> Buildable
        {
            get
            {
                List<int> buildable = new List<int>();
                for (int i = 0; i < 256; i++)
                {
                    if (data.IsValid[(byte)i] && data.IsBuildable[(byte)i])
                    {
                        buildable.Add(i);
                    }
                }
                return buildable;
            }
        }
        public void DrawInventory()
        {
            List<int> buildable = Buildable;
            inventorysize = (int)Math.Ceiling(Math.Sqrt(buildable.Count));

            int x = 0;
            int y = 0;
            for (int ii = 0; ii < buildable.Count; ii++)
            {
                int xx = xcenter(inventorysinglesize * inventorysize) + x * inventorysinglesize;
                int yy = ycenter(inventorysinglesize * inventorysize) + y * inventorysinglesize;
                w.the3d.Draw2dTexture(w.terrain.terrainTexture, xx, yy, inventorysinglesize, inventorysinglesize,
                    data.TextureIdForInventory[buildable[ii]]);

                if (w.ENABLE_FINITEINVENTORY)
                {
                    int amount = w.game.FiniteInventoryAmount(buildable[ii]);
                    w.the3d.Draw2dText("" + amount, xx, yy, 8, null);
                }
                x++;
                if (x >= inventorysize)
                {
                    x = 0;
                    y++;
                }
            }
            if (inventoryselectedx + inventoryselectedy * inventorysize < buildable.Count)
            {
                w.the3d.Draw2dBitmapFile(Path.Combine("gui", "activematerial.png"),
                    xcenter(inventorysinglesize * inventorysize) + inventoryselectedx * inventorysinglesize,
                    ycenter(inventorysinglesize * inventorysize) + inventoryselectedy * inventorysinglesize,
                    w.NextPowerOfTwo((uint)inventorysinglesize), w.NextPowerOfTwo((uint)inventorysinglesize));
            }
            w.DrawMaterialSelector();
        }
    }
}

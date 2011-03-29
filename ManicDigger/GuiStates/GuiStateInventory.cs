using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        void InventoryStart()
        {
            guistate = GuiState.Inventory;
            menustate = new MenuState();
            FreeMouse = true;
        }
        void InventoryMouse()
        {
            int invstartx = xcenter(inventorysinglesize * inventorysize);
            int invstarty = ycenter(inventorysinglesize * inventorysize);
            if (mouse_current.X > invstartx && mouse_current.X < invstartx + inventorysinglesize * inventorysize)
            {
                if (mouse_current.Y > invstarty && mouse_current.Y < invstarty + inventorysinglesize * inventorysize)
                {
                    inventoryselectedx = (mouse_current.X - invstartx) / inventorysinglesize;
                    inventoryselectedy = (mouse_current.Y - invstarty) / inventorysinglesize;
                }
            }
            if (mouseleftclick)
            {
                var sel = InventoryGetSelected();
                if (sel != null)
                {
                    materialSlots[activematerial] = sel.Value;
                    GuiStateBackToGame();
                }
                mouseleftclick = false;
            }
        }
        private void InventoryKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == GetKey(OpenTK.Input.Key.Escape))
            {
                GuiStateBackToGame();
            }
            Direction4? dir = null;
            if (e.Key == GetKey(OpenTK.Input.Key.Left)) { dir = Direction4.Left; }
            if (e.Key == GetKey(OpenTK.Input.Key.Right)) { dir = Direction4.Right; }
            if (e.Key == GetKey(OpenTK.Input.Key.Up)) { dir = Direction4.Up; }
            if (e.Key == GetKey(OpenTK.Input.Key.Down)) { dir = Direction4.Down; }
            if (dir != null)
            {
                InventorySelectionMove(dir.Value);
            }
            if (e.Key == GetKey(OpenTK.Input.Key.Enter) || e.Key == GetKey(OpenTK.Input.Key.KeypadEnter))
            {
                var sel = InventoryGetSelected();
                if (sel != null)
                {
                    materialSlots[activematerial] = sel.Value;
                    GuiStateBackToGame();
                }
            }
            HandleMaterialKeys(e);
        }
        int inventoryselectedx;
        int inventoryselectedy;
        void InventorySelectionMove(Direction4 dir)
        {
            if (dir == Direction4.Left) { inventoryselectedx--; }
            if (dir == Direction4.Right) { inventoryselectedx++; }
            if (dir == Direction4.Up) { inventoryselectedy--; }
            if (dir == Direction4.Down) { inventoryselectedy++; }
            inventoryselectedx = MyMath.Clamp(inventoryselectedx, 0, inventorysize - 1);
            inventoryselectedy = MyMath.Clamp(inventoryselectedy, 0, inventorysize - 1);
        }
        int inventorysize;
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
        int inventorysinglesize = 40;
        void DrawInventory()
        {
            List<int> buildable = Buildable;
            inventorysize = (int)Math.Ceiling(Math.Sqrt(buildable.Count));

            int x = 0;
            int y = 0;
            for (int ii = 0; ii < buildable.Count; ii++)
            {
                int xx = xcenter(inventorysinglesize * inventorysize) + x * inventorysinglesize;
                int yy = ycenter(inventorysinglesize * inventorysize) + y * inventorysinglesize;
                the3d.Draw2dTexture(terrain.terrainTexture, xx, yy, inventorysinglesize, inventorysinglesize,
                    data.TextureIdForInventory[buildable[ii]]);

                if (ENABLE_FINITEINVENTORY)
                {
                    int amount = game.FiniteInventoryAmount(buildable[ii]);
                    the3d.Draw2dText("" + amount, xx, yy, 8, null);
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
                the3d.Draw2dBitmapFile(Path.Combine("gui", "activematerial.png"),
                    xcenter(inventorysinglesize * inventorysize) + inventoryselectedx * inventorysinglesize,
                    ycenter(inventorysinglesize * inventorysize) + inventoryselectedy * inventorysinglesize,
                    NextPowerOfTwo((uint)inventorysinglesize), NextPowerOfTwo((uint)inventorysinglesize));
            }
            DrawMaterialSelector();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using ManicDigger.Renderers;

namespace ManicDigger.Hud
{
    public interface IMouseCurrent
    {
        Point MouseCurrent { get; }
    }

    public interface IActiveMaterial
    {
        int ActiveMaterial { get; set; }
    }

    public class ActiveMaterialDummy : IActiveMaterial
    {
        public int ActiveMaterial { get; set; }
    }

    public interface IViewport3dSelectedBlock
    {
        Vector3i SelectedBlock();
    }
    public class Viewport3dSelectedBlockDummy : IViewport3dSelectedBlock
    {
        public Vector3i SelectedBlock()
        {
            return new Vector3i(-1, -1, -1);
        }
    }

    public class HudInventory
    {
        public IGetFileStream getfile;
        public IMouseCurrent mouse_current;
        public The3d the3d;
        public IViewportSize viewport_size;
        public IGameDataItems dataItems;
        public Inventory inventory;
        public IActiveMaterial ActiveMaterial;
        public InventoryUtil inventoryUtil;
        public IInventoryController controller;
        public IViewport3dSelectedBlock viewport3d;
        public ITerrainTextures terraintextures;

        public int CellDrawSize = 28;

        public Point InventoryStart
        {
            get
            {
                return new Point(viewport_size.Width / 2 - 400 / 2, viewport_size.Height / 2 - 600 / 2);
            }
        }
        public Point CellsStart
        {
            get
            {
                Point p = new Point(33, 309); p.Offset(InventoryStart); return p;
            }
        }
        Point MaterialSelectorStart
        {
            get
            {
                Point p = MaterialSelectorBackgroundStart;
                p.Offset(17, 17);
                return p;
            }
        }
        Point MaterialSelectorBackgroundStart
        {
            get
            {
                return new Point(viewport_size.Width / 2 - 512 / 2, viewport_size.Height - 90);
            }
        }
        Point CellCountInPage = new Point(12, 7);
        Point CellCountTotal = new Point(12, 7 * 3);
        public int ActiveMaterialCellSize = 48;

        public void OnKeyPress(KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                ActiveMaterial.ActiveMaterial = int.Parse("" + e.KeyChar) - 1;
                if (ActiveMaterial.ActiveMaterial == -1) { ActiveMaterial.ActiveMaterial = 9; }
            }
        }
        int ScrollButtonSize { get { return CellDrawSize; } }
        Point ScrollUpButton
        {
            get
            {
                Point p = CellsStart; p.Offset(CellCountInPage.X * CellDrawSize, 0); return p;
            }
        }
        Point ScrollDownButton
        {
            get
            {
                Point p = CellsStart; p.Offset(CellCountInPage.X * CellDrawSize, (CellCountInPage.Y - 1) * CellDrawSize); return p;
            }
        }

        public void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point scaledMouse = mouse_current.MouseCurrent;

            //main inventory
            Point? cellInPage = SelectedCell(scaledMouse);
            //grab from inventory
            if (cellInPage != null)
            {
                controller.InventoryClick(new InventoryPosition()
                {
                    type = InventoryPositionType.MainArea,
                    AreaX = cellInPage.Value.X,
                    AreaY = cellInPage.Value.Y + ScrollLine,
                });
            }
            //drop items on ground
            if (scaledMouse.X < CellsStart.X && scaledMouse.Y < MaterialSelectorStart.Y)
            {
                Vector3i pos = viewport3d.SelectedBlock();
                controller.InventoryClick(new InventoryPosition()
                {
                    type = InventoryPositionType.Ground,
                    GroundPositionX = pos.x,
                    GroundPositionY = pos.y,
                    GroundPositionZ = pos.z,
                });
            }
            //material selector
            if (SelectedMaterialSelectorSlot(scaledMouse) != null)
            {
                int oldActiveMaterial = ActiveMaterial.ActiveMaterial;
                ActiveMaterial.ActiveMaterial = SelectedMaterialSelectorSlot(scaledMouse).Value;
                if (oldActiveMaterial == ActiveMaterial.ActiveMaterial)
                {
                    controller.InventoryClick(new InventoryPosition()
                    {
                        type = InventoryPositionType.MaterialSelector,
                        MaterialId = ActiveMaterial.ActiveMaterial,
                    });
                }
            }
            if (SelectedWearPlace(scaledMouse) != null)
            {
                controller.InventoryClick(new InventoryPosition()
                {
                    type = InventoryPositionType.WearPlace,
                    WearPlace = (int)(SelectedWearPlace(scaledMouse).Value),
                    ActiveMaterial = ActiveMaterial.ActiveMaterial,
                });
            }
            if (scaledMouse.X >= ScrollUpButton.X && scaledMouse.X < ScrollUpButton.X + ScrollButtonSize
                && scaledMouse.Y >= ScrollUpButton.Y && scaledMouse.Y < ScrollUpButton.Y + ScrollButtonSize)
            {
                ScrollUp();
                ScrollingUpTime = DateTime.UtcNow;
            }
            if (scaledMouse.X >= ScrollDownButton.X && scaledMouse.X < ScrollDownButton.X + ScrollButtonSize
                && scaledMouse.Y >= ScrollDownButton.Y && scaledMouse.Y < ScrollDownButton.Y + ScrollButtonSize)
            {
                ScrollDown();
                ScrollingDownTime = DateTime.UtcNow;
            }
        }

        private void ScrollUp()
        {
            ScrollLine--;
            if (ScrollLine < 0) { ScrollLine = 0; }
        }

        private void ScrollDown()
        {
            ScrollLine++;
            int max = CellCountTotal.Y - CellCountInPage.Y;
            if (ScrollLine >= max) { ScrollLine = max; }
        }

        DateTime ScrollingUpTime;
        DateTime ScrollingDownTime;

        public void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScrollingUpTime = new DateTime();
            ScrollingDownTime = new DateTime();
        }

        private int? SelectedMaterialSelectorSlot(Point scaledMouse)
        {
            if (scaledMouse.X >= MaterialSelectorStart.X && scaledMouse.Y >= MaterialSelectorStart.Y
                && scaledMouse.X < MaterialSelectorStart.X + 10 * ActiveMaterialCellSize
                && scaledMouse.Y < MaterialSelectorStart.Y + 10 * ActiveMaterialCellSize)
            {
                return (scaledMouse.X - MaterialSelectorStart.X) / ActiveMaterialCellSize;
            }
            return null;
        }

        private Point? SelectedCell(Point scaledMouse)
        {
            if (scaledMouse.X < CellsStart.X || scaledMouse.Y < CellsStart.Y
                || scaledMouse.X > CellsStart.X + CellCountInPage.X * CellDrawSize
                || scaledMouse.Y > CellsStart.Y + CellCountInPage.Y * CellDrawSize)
            {
                return null;
            }
            Point cell = new Point((scaledMouse.X - CellsStart.X) / CellDrawSize,
                (scaledMouse.Y - CellsStart.Y) / CellDrawSize);
            return cell;
        }

        public int ScrollLine;

        public void Draw()
        {
            if (ScrollingUpTime.Ticks != 0 && (DateTime.UtcNow - ScrollingUpTime).TotalSeconds > 0.25)
            {
                ScrollingUpTime = DateTime.UtcNow;
                ScrollUp();
            }
            if (ScrollingDownTime.Ticks != 0 && (DateTime.UtcNow - ScrollingDownTime).TotalSeconds > 0.25)
            {
                ScrollingDownTime = DateTime.UtcNow;
                ScrollDown();
            }

            Point scaledMouse = mouse_current.MouseCurrent;

            the3d.Draw2dBitmapFile("inventory.png", InventoryStart.X, InventoryStart.Y, 512, 1024);

            //the3d.Draw2dTexture(terrain, 50, 50, 50, 50, 0);
            //the3d.Draw2dBitmapFile("inventory_weapon_shovel.png", 100, 100, 60 * 2, 60 * 4);
            //the3d.Draw2dBitmapFile("inventory_gauntlet_gloves.png", 200, 200, 60 * 2, 60 * 2);
            //main inventory
            foreach (var k in inventory.Items)
            {
                int screeny = k.Key.Y - ScrollLine;
                if (screeny >= 0 && screeny < CellCountInPage.Y)
                {
                    DrawItem(new Point(CellsStart.X + k.Key.X * CellDrawSize, CellsStart.Y + screeny * CellDrawSize), k.Value, null);
                }
            }

            //draw area selection
            if (inventory.DragDropItem != null)
            {
                Point? selectedInPage = SelectedCell(scaledMouse);
                if (selectedInPage != null)
                {
                    int x = (selectedInPage.Value.X) * CellDrawSize + CellsStart.X;
                    int y = (selectedInPage.Value.Y) * CellDrawSize + CellsStart.Y;
                    Point size = dataItems.ItemSize(inventory.DragDropItem);
                    if (selectedInPage.Value.X + size.X <= CellCountInPage.X
                        && selectedInPage.Value.Y + size.Y <= CellCountInPage.Y)
                    {
                        Color c;
                        Point[] itemsAtArea = inventoryUtil.ItemsAtArea(new Point(selectedInPage.Value.X, selectedInPage.Value.Y + ScrollLine), size);
                        if (itemsAtArea == null || itemsAtArea.Length > 1)
                        {
                            c = Color.Red;
                        }
                        else //0 or 1
                        {
                            c = Color.Green;
                        }
                        the3d.Draw2dTexture(the3d.WhiteTexture(), x, y,
                            CellDrawSize * size.X, CellDrawSize * size.Y,
                            null, Color.FromArgb(100, c));
                    }
                }
                WearPlace? selectedWear = SelectedWearPlace(scaledMouse);
                if (selectedWear != null)
                {
                    Point p = Offset(wearPlaceStart[(int)selectedWear], InventoryStart);
                    Point size = wearPlaceCells[(int)selectedWear];

                    Color c;
                    Item itemsAtArea = inventoryUtil.ItemAtWearPlace(selectedWear.Value, ActiveMaterial.ActiveMaterial);
                    if (!dataItems.CanWear(selectedWear.Value, inventory.DragDropItem))
                    {
                        c = Color.Red;
                    }
                    else //0 or 1
                    {
                        c = Color.Green;
                    }
                    the3d.Draw2dTexture(the3d.WhiteTexture(), p.X, p.Y,
                        CellDrawSize * size.X, CellDrawSize * size.Y,
                        null, Color.FromArgb(100, c));
                }
            }

            //material selector
            DrawMaterialSelector();

            //wear
            //DrawItem(Offset(wearPlaceStart[(int)WearPlace.LeftHand], InventoryStart), inventory.LeftHand[ActiveMaterial.ActiveMaterial], null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.RightHand], InventoryStart), inventory.RightHand[ActiveMaterial.ActiveMaterial], null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.MainArmor], InventoryStart), inventory.MainArmor, null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.Boots], InventoryStart), inventory.Boots, null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.Helmet], InventoryStart), inventory.Helmet, null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.Gauntlet], InventoryStart), inventory.Gauntlet, null);

            //info
            if (SelectedCell(scaledMouse) != null)
            {
                Point selected = SelectedCell(scaledMouse).Value;
                selected.Offset(0, ScrollLine);
                Point? itemAtCell = inventoryUtil.ItemAtCell(selected);
                if (itemAtCell != null)
                {
                    Item item = inventory.Items[new ProtoPoint(itemAtCell.Value)];
                    if (item != null)
                    {
                        int x = (selected.X) * CellDrawSize + CellsStart.X;
                        int y = (selected.Y) * CellDrawSize + CellsStart.Y;
                        DrawItemInfo(new Point(scaledMouse.X, scaledMouse.Y), item);
                    }
                }
            }
            if (SelectedWearPlace(scaledMouse) != null)
            {
                WearPlace selected = SelectedWearPlace(scaledMouse).Value;
                Item itemAtWearPlace = inventoryUtil.ItemAtWearPlace(selected, ActiveMaterial.ActiveMaterial);
                if (itemAtWearPlace != null)
                {
                    DrawItemInfo(new Point(scaledMouse.X, scaledMouse.Y), itemAtWearPlace);
                }
            }
            if (SelectedMaterialSelectorSlot(scaledMouse) != null)
            {
                int selected = SelectedMaterialSelectorSlot(scaledMouse).Value;
                Item item = inventory.RightHand[selected];
                if (item != null)
                {
                    DrawItemInfo(new Point(scaledMouse.X, scaledMouse.Y), item);
                }
            }

            if (inventory.DragDropItem != null)
            {
                DrawItem(new Point(scaledMouse.X, scaledMouse.Y), inventory.DragDropItem, null);
            }
        }

        public void DrawMaterialSelector()
        {
            the3d.Draw2dBitmapFile("materials.png", MaterialSelectorBackgroundStart.X, MaterialSelectorBackgroundStart.Y, 1024, 128);
            for (int i = 0; i < 10; i++)
            {
                /*
                if (inventory.LeftHand[i] != null)
                {
                    DrawItem(new Point(MaterialSelectorStart.X + i * ActiveMaterialCellSize, MaterialSelectorStart.Y),
                        inventory.LeftHand[i], new Point(ActiveMaterialCellSize, ActiveMaterialCellSize));
                }
                */
                if (inventory.RightHand[i] != null)
                {
                    DrawItem(new Point(MaterialSelectorStart.X + i * ActiveMaterialCellSize, MaterialSelectorStart.Y),
                        inventory.RightHand[i], new Point(ActiveMaterialCellSize, ActiveMaterialCellSize));
                }
            }
            the3d.Draw2dBitmapFile("activematerial2.png",
                MaterialSelectorStart.X + ActiveMaterialCellSize * ActiveMaterial.ActiveMaterial,
                MaterialSelectorStart.Y, ActiveMaterialCellSize, ActiveMaterialCellSize);
        }

        private WearPlace? SelectedWearPlace(Point scaledMouse)
        {
            for (int i = 0; i < wearPlaceStart.Length; i++)
            {
                Point p = wearPlaceStart[i];
                p.Offset(InventoryStart);
                Point cells = wearPlaceCells[i];
                if (scaledMouse.X >= p.X && scaledMouse.Y >= p.Y
                    && scaledMouse.X < p.X + cells.X * CellDrawSize
                    && scaledMouse.Y < p.Y + cells.Y * CellDrawSize)
                {
                    return (WearPlace)i;
                }
            }
            return null;
        }

        //indexed by enum WearPlace
        Point[] wearPlaceStart = new Point[]
        {
            //new Point(282,85), //LeftHand,
            new Point(55,85), //RightHand,
            new Point(174,106), //MainArmor,
            new Point(278,233), //Boots,
            new Point(174,33), //Helmet,
            new Point(56,231), //Gauntlet,
        };

        //indexed by enum WearPlace
        Point[] wearPlaceCells = new Point[]
        {
            //new Point(2,4), //LeftHand,
            new Point(2,4), //RightHand,
            new Point(2,4), //MainArmor,
            new Point(2,2), //Boots,
            new Point(2,2), //Helmet,
            new Point(2,2), //Gauntlet,
        };

        private void DrawItem(Point screenpos, Item item, Point? drawsize)
        {
            if (item == null)
            {
                return;
            }
            Point size = dataItems.ItemSize(item);
            if (drawsize == null)
            {
                drawsize = new Point(CellDrawSize * size.X, CellDrawSize * size.Y);
            }
            if (item.ItemClass == ItemClass.Block)
            {
                the3d.Draw2dTexture(terraintextures.terrainTexture, screenpos.X, screenpos.Y,
                    drawsize.Value.X, drawsize.Value.Y, dataItems.TextureIdForInventory[item.BlockId]);
                if (item.BlockCount > 1)
                {
                    the3d.Draw2dText(item.BlockCount.ToString(), screenpos.X, screenpos.Y, 8, Color.White);
                }
            }
            else
            {
                the3d.Draw2dBitmapFile(dataItems.ItemGraphics(item), screenpos.X, screenpos.Y,
                    drawsize.Value.X, drawsize.Value.Y);
            }
        }

        public void DrawItemInfo(Point screenpos, Item item)
        {
        	Point size = dataItems.ItemSize(item);
        	float tw = the3d.TextSize(dataItems.ItemInfo(item), 11.5f).Width + 6;
        	float th = the3d.TextSize(dataItems.ItemInfo(item), 11.5f).Height + 4;
        	float w = tw + CellDrawSize * size.X;
        	float h = th < CellDrawSize * size.Y ? CellDrawSize * size.Y + 4 : th;
        	if (screenpos.X < w + 20) { screenpos.X = (int)w + 20; }
            if (screenpos.Y < h + 20) { screenpos.Y = (int)h + 20; }
            if (screenpos.X > viewport_size.Width - (w + 20)) { screenpos.X = viewport_size.Width - ((int)w + 20); }
            if (screenpos.Y > viewport_size.Height - (h + 20)) { screenpos.Y = viewport_size.Height - ((int)h + 20); }
            the3d.Draw2dTexture(the3d.WhiteTexture(), screenpos.X - w, screenpos.Y - h, w, h, null, Color.FromArgb(0, Color.Black));
            the3d.Draw2dTexture(the3d.WhiteTexture(), screenpos.X - w+2, screenpos.Y - h+2, w-4, h-4, null, Color.FromArgb(0, Color.DimGray));
            the3d.Draw2dText(dataItems.ItemInfo(item), screenpos.X - tw + 4, screenpos.Y - h + 2, 10, null);
            DrawItem(new Point(screenpos.X - (int)w + 2, screenpos.Y - (int)h + 2), new Item { BlockId = item.BlockId }, null);
        }
        static Point Offset(Point a, Point b)
        {
            Point p = a;
            p.Offset(b);
            return p;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Input;

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
        Point CellCount = new Point(12, 7);
        public int ActiveMaterialCellSize = 48;

        public void OnKeyPress(KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                ActiveMaterial.ActiveMaterial = int.Parse("" + e.KeyChar) - 1;
                if (ActiveMaterial.ActiveMaterial == -1) { ActiveMaterial.ActiveMaterial = 9; }
            }
        }

        public void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point scaledMouse = mouse_current.MouseCurrent;

            //main inventory
            Point? cell = SelectedCell(scaledMouse);
            //grab from inventory
            if (cell != null)
            {
                controller.InventoryClick(new InventoryPosition()
                {
                    type = InventoryPositionType.MainArea,
                    AreaX = cell.Value.X,
                    AreaY = cell.Value.Y,
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
                || scaledMouse.X > CellsStart.X + CellCount.X * CellDrawSize
                || scaledMouse.Y > CellsStart.Y + CellCount.Y * CellDrawSize)
            {
                return null;
            }
            Point cell = new Point((scaledMouse.X - CellsStart.X) / CellDrawSize,
                (scaledMouse.Y - CellsStart.Y) / CellDrawSize);
            return cell;
        }

        public void OnRenderFrame(FrameEventArgs e)
        {
            Draw();
        }

        int terraintexture = -1;
        int TerrainTexture
        {
            get
            {
                if (terraintexture == -1)
                {
                    terraintexture = the3d.LoadTexture(getfile.GetFile("terrain.png"));
                }
                return terraintexture;
            }
        }
        public void Draw()
        {
            Point scaledMouse = mouse_current.MouseCurrent;

            the3d.Draw2dBitmapFile("inventory.png", InventoryStart.X, InventoryStart.Y, 512, 1024);

            //the3d.Draw2dTexture(terrain, 50, 50, 50, 50, 0);
            //the3d.Draw2dBitmapFile("inventory_weapon_shovel.png", 100, 100, 60 * 2, 60 * 4);
            //the3d.Draw2dBitmapFile("inventory_gauntlet_gloves.png", 200, 200, 60 * 2, 60 * 2);
            //main inventory
            foreach (var k in inventory.Items)
            {
                DrawItem(new Point(CellsStart.X + k.Key.X * CellDrawSize, CellsStart.Y + k.Key.Y * CellDrawSize), k.Value, null);
            }

            //draw area selection
            if (inventory.DragDropItem != null)
            {
                Point? selected = SelectedCell(scaledMouse);
                if (selected != null)
                {
                    int x = (selected.Value.X) * CellDrawSize + CellsStart.X;
                    int y = (selected.Value.Y) * CellDrawSize + CellsStart.Y;
                    Point size = dataItems.ItemSize(inventory.DragDropItem);
                    if (selected.Value.X + size.X <= CellCount.X
                        && selected.Value.Y + size.Y <= CellCount.Y)
                    {
                        Color c;
                        Point[] itemsAtArea = inventoryUtil.ItemsAtArea(selected.Value, size);
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
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.LeftHand], InventoryStart), inventory.LeftHand[ActiveMaterial.ActiveMaterial], null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.RightHand], InventoryStart), inventory.RightHand[ActiveMaterial.ActiveMaterial], null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.MainArmor], InventoryStart), inventory.MainArmor, null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.Boots], InventoryStart), inventory.Boots, null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.Helmet], InventoryStart), inventory.Helmet, null);
            DrawItem(Offset(wearPlaceStart[(int)WearPlace.Gauntlet], InventoryStart), inventory.Gauntlet, null);

            //info
            if (SelectedCell(scaledMouse) != null)
            {
                Point selected = SelectedCell(scaledMouse).Value;
                Point? itemAtCell = inventoryUtil.ItemAtCell(selected);
                if (itemAtCell != null)
                {
                    Item item = inventory.Items[new ProtoPoint(itemAtCell.Value)];
                    if (item != null)
                    {
                        int x = (selected.X) * CellDrawSize + CellsStart.X;
                        int y = (selected.Y) * CellDrawSize + CellsStart.Y;
                        DrawItemInfo(new Point(x, y), item);
                    }
                }
            }
            if (SelectedWearPlace(scaledMouse) != null)
            {
                WearPlace selected = SelectedWearPlace(scaledMouse).Value;
                Item itemAtWearPlace = inventoryUtil.ItemAtWearPlace(selected, ActiveMaterial.ActiveMaterial);
                if (itemAtWearPlace != null)
                {
                    DrawItemInfo(Offset(InventoryStart, wearPlaceStart[(int)selected]), itemAtWearPlace);
                }
            }
            if (SelectedMaterialSelectorSlot(scaledMouse) != null)
            {
                int selected = SelectedMaterialSelectorSlot(scaledMouse).Value;
                Item item = inventory.RightHand[selected];
                if (item != null)
                {
                    DrawItemInfo(new Point(MaterialSelectorStart.X + ActiveMaterialCellSize * selected,
                        MaterialSelectorStart.Y), item);
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
                if (inventory.LeftHand[i] != null)
                {
                    DrawItem(new Point(MaterialSelectorStart.X + i * ActiveMaterialCellSize, MaterialSelectorStart.Y),
                        inventory.LeftHand[i], new Point(ActiveMaterialCellSize, ActiveMaterialCellSize));
                }
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
            new Point(282,85), //LeftHand,
            new Point(55,85), //RightHand,
            new Point(174,106), //MainArmor,
            new Point(278,233), //Boots,
            new Point(174,33), //Helmet,
            new Point(56,231), //Gauntlet,
        };

        //indexed by enum WearPlace
        Point[] wearPlaceCells = new Point[]
        {
            new Point(2,4), //LeftHand,
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
                the3d.Draw2dTexture(TerrainTexture, screenpos.X, screenpos.Y,
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
            if (screenpos.X < 150 + 20) { screenpos.X = 150 + 20; }
            if (screenpos.Y < 100 + 20) { screenpos.Y = 100 + 20; }
            if (screenpos.X > viewport_size.Width - (150 + 20)) { screenpos.X = viewport_size.Width - (150 + 20); }
            if (screenpos.Y > viewport_size.Height - (100 + 20)) { screenpos.Y = viewport_size.Height - (100 + 20); }
            the3d.Draw2dTexture(the3d.WhiteTexture(), screenpos.X - 150, screenpos.Y - 100, 250, 100, null, Color.FromArgb(0, Color.Gray));
            the3d.Draw2dText(dataItems.ItemInfo(item), screenpos.X - 150, screenpos.Y - 100, 10, null);
        }
        static Point Offset(Point a, Point b)
        {
            Point p = a;
            p.Offset(b);
            return p;
        }
    }
}

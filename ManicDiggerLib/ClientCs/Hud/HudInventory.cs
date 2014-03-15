using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using ManicDigger.Renderers;

namespace ManicDigger.Hud
{
    public class HudInventory
    {
        public HudInventory()
        {
            //indexed by enum WearPlace
            wearPlaceStart = new PointRef[5];
            {
                //new Point(282,85), //LeftHand,
                wearPlaceStart[0] = PointRef.Create(34, 100); //RightHand,
                wearPlaceStart[1] = PointRef.Create(74, 100); //MainArmor,
                wearPlaceStart[2] = PointRef.Create(194, 100); //Boots,
                wearPlaceStart[3] = PointRef.Create(114, 100); //Helmet,
                wearPlaceStart[4] = PointRef.Create(154, 100); //Gauntlet,
            };

            //indexed by enum WearPlace
            wearPlaceCells = new PointRef[5];
            {
                //new Point(2,4), //LeftHand,
                wearPlaceCells[0] = PointRef.Create(1, 1); //RightHand,
                wearPlaceCells[1] = PointRef.Create(1, 1); //MainArmor,
                wearPlaceCells[2] = PointRef.Create(1, 1); //Boots,
                wearPlaceCells[3] = PointRef.Create(1, 1); //Helmet,
                wearPlaceCells[4] = PointRef.Create(1, 1); //Gauntlet,
            }
            CellCountInPageX = 12;
            CellCountInPageY = 7;
            CellCountTotalX = 12;
            CellCountTotalY = 7 * 6;
            CellDrawSize = 40;
        }

        public ManicDiggerGameWindow game;
        public Game game1;
        public GameDataItemsClient dataItems;
        public InventoryUtilClient inventoryUtil;
        public IInventoryController controller;

        public int CellDrawSize;

        public int InventoryStartX() { return game1.Width() / 2 - 560 / 2; }
        public int InventoryStartY() { return game1.Height() / 2 - 600 / 2; }
        public int CellsStartX() { return 33 + InventoryStartX(); }
        public int CellsStartY() { return 180 + InventoryStartY(); }
        int MaterialSelectorStartX() { return MaterialSelectorBackgroundStartX() + 17; }
        int MaterialSelectorStartY() { return MaterialSelectorBackgroundStartY() + 17; }
        int MaterialSelectorBackgroundStartX() { return game1.Width() / 2 - 512 / 2; }
        int MaterialSelectorBackgroundStartY() { return game1.Height() - 90; }
        int CellCountInPageX;
        int CellCountInPageY;
        int CellCountTotalX;
        int CellCountTotalY;

        public int ActiveMaterialCellSize = 48;

        public void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                game1.ActiveMaterial = int.Parse("" + e.KeyChar) - 1;
                if (game1.ActiveMaterial == -1) { game1.ActiveMaterial = 9; }
            }
        }
        int ScrollButtonSize { get { return CellDrawSize; } }

        int ScrollUpButtonX() { return CellsStartX() + CellCountInPageX * CellDrawSize; }
        int ScrollUpButtonY() { return CellsStartY(); }

        int ScrollDownButtonX() { return CellsStartX() + CellCountInPageX * CellDrawSize; }
        int ScrollDownButtonY() { return CellsStartY() + (CellCountInPageY - 1) * CellDrawSize; }

        public void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            PointRef scaledMouse = PointRef.Create(game1.mouseCurrentX, game1.mouseCurrentY);

            //main inventory
            PointRef cellInPage = SelectedCell(scaledMouse);
            //grab from inventory
            if (cellInPage != null)
            {
                if (e.Button == MouseButton.Left)
                {
                    Packet_InventoryPosition p = new Packet_InventoryPosition();
                    p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                    p.AreaX = cellInPage.X;
                    p.AreaY = cellInPage.Y + ScrollLine;
                    controller.InventoryClick(p);
                }
                else
                {
                    {
                        Packet_InventoryPosition p = new Packet_InventoryPosition();
                        p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                        p.AreaX = cellInPage.X;
                        p.AreaY = cellInPage.Y + ScrollLine;
                        controller.InventoryClick(p);
                    }
                    {
                        Packet_InventoryPosition p = new Packet_InventoryPosition();
                        p.Type = Packet_InventoryPositionTypeEnum.WearPlace;
                        p.WearPlace = (int)WearPlace_.RightHand;
                        p.ActiveMaterial = game1.ActiveMaterial;
                        controller.InventoryClick(p);
                    }
                    {
                        Packet_InventoryPosition p = new Packet_InventoryPosition();
                        p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                        p.AreaX = cellInPage.X;
                        p.AreaY = cellInPage.Y + ScrollLine;
                        controller.InventoryClick(p);
                    }
                }
            }
            //drop items on ground
            if (scaledMouse.X < CellsStartX() && scaledMouse.Y < MaterialSelectorStartY())
            {
                int posx = game1.SelectedBlockPositionX;
                int posy = game1.SelectedBlockPositionY;
                int posz = game1.SelectedBlockPositionZ;
                Packet_InventoryPosition p = new Packet_InventoryPosition();
                {
                    p.Type = Packet_InventoryPositionTypeEnum.Ground;
                    p.GroundPositionX = posx;
                    p.GroundPositionY = posy;
                    p.GroundPositionZ = posz;
                }
                controller.InventoryClick(p);
            }
            //material selector
            if (SelectedMaterialSelectorSlot(scaledMouse) != null)
            {
                //int oldActiveMaterial = ActiveMaterial.ActiveMaterial;
                game1.ActiveMaterial = SelectedMaterialSelectorSlot(scaledMouse).value;
                //if (oldActiveMaterial == ActiveMaterial.ActiveMaterial)
                {
                    Packet_InventoryPosition p = new Packet_InventoryPosition();
                    p.Type = Packet_InventoryPositionTypeEnum.MaterialSelector;
                    p.MaterialId = game1.ActiveMaterial;
                    controller.InventoryClick(p);
                }
            }
            if (SelectedWearPlace(scaledMouse) != null)
            {
                Packet_InventoryPosition p = new Packet_InventoryPosition();
                p.Type = Packet_InventoryPositionTypeEnum.WearPlace;
                p.WearPlace = (int)(SelectedWearPlace(scaledMouse).value);
                p.ActiveMaterial = game1.ActiveMaterial;
                controller.InventoryClick(p);
            }
            if (scaledMouse.X >= ScrollUpButtonX() && scaledMouse.X < ScrollUpButtonX() + ScrollButtonSize
                && scaledMouse.Y >= ScrollUpButtonY() && scaledMouse.Y < ScrollUpButtonY() + ScrollButtonSize)
            {
                ScrollUp();
                ScrollingUpTimeMilliseconds = game1.platform.TimeMillisecondsFromStart();
            }
            if (scaledMouse.X >= ScrollDownButtonX() && scaledMouse.X < ScrollDownButtonX() + ScrollButtonSize
                && scaledMouse.Y >= ScrollDownButtonY() && scaledMouse.Y < ScrollDownButtonY() + ScrollButtonSize)
            {
                ScrollDown();
                ScrollingDownTimeMilliseconds = game1.platform.TimeMillisecondsFromStart();
            }
        }

        public bool IsMouseOverCells()
        {
            return SelectedCellOrScrollbar(game1.mouseCurrentX, game1.mouseCurrentY);
        }

        public void ScrollUp()
        {
            ScrollLine--;
            if (ScrollLine < 0) { ScrollLine = 0; }
        }

        public void ScrollDown()
        {
            ScrollLine++;
            int max = CellCountTotalY - CellCountInPageY;
            if (ScrollLine >= max) { ScrollLine = max; }
        }

        int ScrollingUpTimeMilliseconds;
        int ScrollingDownTimeMilliseconds;

        public void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScrollingUpTimeMilliseconds = 0;
            ScrollingDownTimeMilliseconds = 0;
        }

        IntRef SelectedMaterialSelectorSlot(PointRef scaledMouse)
        {
            if (scaledMouse.X >= MaterialSelectorStartX() && scaledMouse.Y >= MaterialSelectorStartY()
                && scaledMouse.X < MaterialSelectorStartX() + 10 * ActiveMaterialCellSize
                && scaledMouse.Y < MaterialSelectorStartY() + 10 * ActiveMaterialCellSize)
            {
                return IntRef.Create((scaledMouse.X - MaterialSelectorStartX()) / ActiveMaterialCellSize);
            }
            return null;
        }

        PointRef SelectedCell(PointRef scaledMouse)
        {
            if (scaledMouse.X < CellsStartX() || scaledMouse.Y < CellsStartY()
                || scaledMouse.X > CellsStartX() + CellCountInPageX * CellDrawSize
                || scaledMouse.Y > CellsStartY() + CellCountInPageY * CellDrawSize)
            {
                return null;
            }
            PointRef cell = PointRef.Create((scaledMouse.X - CellsStartX()) / CellDrawSize,
                (scaledMouse.Y - CellsStartY()) / CellDrawSize);
            return cell;
        }

        bool SelectedCellOrScrollbar(int scaledMouseX, int scaledMouseY)
        {
            if (scaledMouseX < CellsStartX() || scaledMouseY < CellsStartY()
                || scaledMouseX > CellsStartX() + (CellCountInPageX + 1) * CellDrawSize
                || scaledMouseY > CellsStartY() + CellCountInPageY * CellDrawSize)
            {
                return false;
            }
            return true;
        }

        public int ScrollLine;

        public void Draw()
        {
            if (ScrollingUpTimeMilliseconds != 0 && (game1.platform.TimeMillisecondsFromStart() - ScrollingUpTimeMilliseconds) > 250)
            {
                ScrollingUpTimeMilliseconds = game1.platform.TimeMillisecondsFromStart();
                ScrollUp();
            }
            if (ScrollingDownTimeMilliseconds != 0 && (game1.platform.TimeMillisecondsFromStart() - ScrollingDownTimeMilliseconds) > 250)
            {
                ScrollingDownTimeMilliseconds = game1.platform.TimeMillisecondsFromStart();
                ScrollDown();
            }

            PointRef scaledMouse = PointRef.Create(game1.mouseCurrentX, game1.mouseCurrentY);

            game.Draw2dBitmapFile("inventory.png", InventoryStartX(), InventoryStartY(), 1024, 1024);

            //the3d.Draw2dTexture(terrain, 50, 50, 50, 50, 0);
            //the3d.Draw2dBitmapFile("inventory_weapon_shovel.png", 100, 100, 60 * 2, 60 * 4);
            //the3d.Draw2dBitmapFile("inventory_gauntlet_gloves.png", 200, 200, 60 * 2, 60 * 2);
            //main inventory
            for (int i = 0; i < game1.d_Inventory.ItemsCount; i++)
            {
                Packet_PositionItem k = game1.d_Inventory.Items[i];
                if (k == null)
                {
                    continue;
                }
                int screeny = k.Y - ScrollLine;
                if (screeny >= 0 && screeny < CellCountInPageY)
                {
                    DrawItem(CellsStartX() + k.X * CellDrawSize, CellsStartY() + screeny * CellDrawSize, k.Value_, 0, 0);
                }
            }

            //draw area selection
            if (game1.d_Inventory.DragDropItem != null)
            {
                PointRef selectedInPage = SelectedCell(scaledMouse);
                if (selectedInPage != null)
                {
                    int x = (selectedInPage.X) * CellDrawSize + CellsStartX();
                    int y = (selectedInPage.Y) * CellDrawSize + CellsStartY();
                    int sizex = dataItems.ItemSizeX(game1.d_Inventory.DragDropItem);
                    int sizey = dataItems.ItemSizeY(game1.d_Inventory.DragDropItem);
                    if (selectedInPage.X + sizex <= CellCountInPageX
                        && selectedInPage.Y + sizey <= CellCountInPageY)
                    {
                        int c;
                        IntRef itemsAtAreaCount = new IntRef();
                        PointRef[] itemsAtArea = inventoryUtil.ItemsAtArea(selectedInPage.X, selectedInPage.Y + ScrollLine, sizex, sizey, itemsAtAreaCount);
                        if (itemsAtArea == null || itemsAtAreaCount.value > 1)
                        {
                            c = Game.ColorFromArgb(100, 255, 0, 0); // red
                        }
                        else //0 or 1
                        {
                            c = Game.ColorFromArgb(100, 0, 255, 0); // green
                        }
                        game1.Draw2dTexture(game1.WhiteTexture(), x, y,
                            CellDrawSize * sizex, CellDrawSize * sizey,
                            null, 0, c, false);
                    }
                }
                IntRef selectedWear = SelectedWearPlace(scaledMouse);
                if (selectedWear != null)
                {
                    Point p = new Point(wearPlaceStart[selectedWear.value].X + InventoryStartX(), wearPlaceStart[selectedWear.value].Y + InventoryStartY());
                    PointRef size = wearPlaceCells[selectedWear.value];

                    int c;
                    Packet_Item itemsAtArea = inventoryUtil.ItemAtWearPlace(selectedWear.value, game1.ActiveMaterial);
                    if (!dataItems.CanWear(selectedWear.value, game1.d_Inventory.DragDropItem))
                    {
                        c = Game.ColorFromArgb(100, 255, 0, 0); // red
                    }
                    else //0 or 1
                    {
                        c = Game.ColorFromArgb(100, 0, 255, 0); // green
                    }
                    game1.Draw2dTexture(game1.WhiteTexture(), p.X, p.Y,
                        CellDrawSize * size.X, CellDrawSize * size.Y,
                        null, 0, c, false);
                }
            }

            //material selector
            DrawMaterialSelector();

            //wear
            //DrawItem(Offset(wearPlaceStart[(int)WearPlace.LeftHand], InventoryStart), inventory.LeftHand[ActiveMaterial.ActiveMaterial], null);
            DrawItem(wearPlaceStart[(int)WearPlace_.RightHand].X + InventoryStartX(), wearPlaceStart[(int)WearPlace_.RightHand].Y + InventoryStartY(), game1.d_Inventory.RightHand[game1.ActiveMaterial], 0, 0);
            DrawItem(wearPlaceStart[(int)WearPlace_.MainArmor].X + InventoryStartX(), wearPlaceStart[(int)WearPlace_.MainArmor].Y + InventoryStartY(), game1.d_Inventory.MainArmor, 0, 0);
            DrawItem(wearPlaceStart[(int)WearPlace_.Boots].X + InventoryStartX(), wearPlaceStart[(int)WearPlace_.Boots].Y + InventoryStartY(), game1.d_Inventory.Boots, 0, 0);
            DrawItem(wearPlaceStart[(int)WearPlace_.Helmet].X + InventoryStartX(), wearPlaceStart[(int)WearPlace_.Helmet].Y + InventoryStartY(), game1.d_Inventory.Helmet, 0, 0);
            DrawItem(wearPlaceStart[(int)WearPlace_.Gauntlet].X + InventoryStartX(), wearPlaceStart[(int)WearPlace_.Gauntlet].Y + InventoryStartY(), game1.d_Inventory.Gauntlet, 0, 0);

            //info
            if (SelectedCell(scaledMouse) != null)
            {
                PointRef selected = SelectedCell(scaledMouse);
                selected.Y += ScrollLine;
                PointRef itemAtCell = inventoryUtil.ItemAtCell(selected);
                if (itemAtCell != null)
                {
                    Packet_Item item = GetItem(game1.d_Inventory, itemAtCell.X, itemAtCell.Y);
                    if (item != null)
                    {
                        int x = (selected.X) * CellDrawSize + CellsStartX();
                        int y = (selected.Y) * CellDrawSize + CellsStartY();
                        DrawItemInfo(scaledMouse.X, scaledMouse.Y, item);
                    }
                }
            }
            if (SelectedWearPlace(scaledMouse) != null)
            {
                int selected = SelectedWearPlace(scaledMouse).value;
                Packet_Item itemAtWearPlace = inventoryUtil.ItemAtWearPlace(selected, game1.ActiveMaterial);
                if (itemAtWearPlace != null)
                {
                    DrawItemInfo(scaledMouse.X, scaledMouse.Y, itemAtWearPlace);
                }
            }
            if (SelectedMaterialSelectorSlot(scaledMouse) != null)
            {
                int selected = SelectedMaterialSelectorSlot(scaledMouse).value;
                Packet_Item item = game1.d_Inventory.RightHand[selected];
                if (item != null)
                {
                    DrawItemInfo(scaledMouse.X, scaledMouse.Y, item);
                }
            }

            if (game1.d_Inventory.DragDropItem != null)
            {
                DrawItem(scaledMouse.X, scaledMouse.Y, game1.d_Inventory.DragDropItem, 0, 0);
            }
        }

        Packet_Item GetItem(Packet_Inventory inventory, int x, int y)
        {
            for (int i = 0; i < inventory.ItemsCount; i++)
            {
                if (inventory.Items[i].X == x && inventory.Items[i].Y == y)
                {
                    return inventory.Items[i].Value_;
                }
            }
            return null;
        }

        public void DrawMaterialSelector()
        {
            game.Draw2dBitmapFile("materials.png", MaterialSelectorBackgroundStartX(), MaterialSelectorBackgroundStartY(), 1024, 128);
            for (int i = 0; i < 10; i++)
            {
                if (game1.d_Inventory.RightHand[i] != null)
                {
                    DrawItem(MaterialSelectorStartX() + i * ActiveMaterialCellSize, MaterialSelectorStartY(),
                        game1.d_Inventory.RightHand[i], ActiveMaterialCellSize, ActiveMaterialCellSize);
                }
            }
            game.Draw2dBitmapFile("activematerial2.png",
                MaterialSelectorStartX() + ActiveMaterialCellSize * game1.ActiveMaterial,
                MaterialSelectorStartY(), ActiveMaterialCellSize, ActiveMaterialCellSize);
        }

        IntRef SelectedWearPlace(PointRef scaledMouse)
        {
            for (int i = 0; i < wearPlaceStart.Length; i++)
            {
                PointRef p = wearPlaceStart[i];
                p.X += InventoryStartX();
                p.Y += InventoryStartY();
                PointRef cells = wearPlaceCells[i];
                if (scaledMouse.X >= p.X && scaledMouse.Y >= p.Y
                    && scaledMouse.X < p.X + cells.X * CellDrawSize
                    && scaledMouse.Y < p.Y + cells.Y * CellDrawSize)
                {
                    return IntRef.Create(i);
                }
            }
            return null;
        }

        //indexed by enum WearPlace
        PointRef[] wearPlaceStart;
        //indexed by enum WearPlace
        PointRef[] wearPlaceCells;

        void DrawItem(int screenposX, int screenposY, Packet_Item item, int drawsizeX, int drawsizeY)
        {
            if (item == null)
            {
                return;
            }
            int sizex = dataItems.ItemSizeX(item);
            int sizey = dataItems.ItemSizeY(item);
            if (drawsizeX == 0 || drawsizeX == -1)
            {
                drawsizeX = CellDrawSize * sizex;
                drawsizeY = CellDrawSize * sizey;
            }
            if (item.ItemClass == Packet_ItemClassEnum.Block)
            {
                if (item.BlockId == 0)
                {
                    return;
                }
                game1.Draw2dTexture(game1.terrainTexture, screenposX, screenposY,
                    drawsizeX, drawsizeY, IntRef.Create(dataItems.TextureIdForInventory()[item.BlockId]), game1.texturesPacked(), Game.ColorFromArgb(255,255,255,255), false);
                if (item.BlockCount > 1)
                {
                    FontCi font = new FontCi();
                    font.size = 8;
                    font.family = "Arial";
                    game1.Draw2dText(item.BlockCount.ToString(), font, screenposX, screenposY, null, false);
                }
            }
            else
            {
                game.Draw2dBitmapFile(dataItems.ItemGraphics(item), screenposX, screenposY,
                    drawsizeX, drawsizeY);
            }
        }

        public void DrawItemInfo(int screenposX, int screenposY, Packet_Item item)
        {
            int sizex = dataItems.ItemSizeX(item);
            int sizey = dataItems.ItemSizeY(item);
            IntRef tw = new IntRef();
            IntRef th = new IntRef();
            float one = 1;
            game1.platform.TextSize(dataItems.ItemInfo(item), 11 + one / 2, tw, th);
            tw.value += 6;
            th.value += 4;
            int w = game1.platform.FloatToInt(tw.value + CellDrawSize * sizex);
            int h = game1.platform.FloatToInt(th.value < CellDrawSize * sizey ? CellDrawSize * sizey + 4 : th.value);
            if (screenposX < w + 20) { screenposX = w + 20; }
            if (screenposY < h + 20) { screenposY = h + 20; }
            if (screenposX > game1.Width() - (w + 20)) { screenposX = game1.Width() - (w + 20); }
            if (screenposY > game1.Height() - (h + 20)) { screenposY = game1.Height() - (h + 20); }
            game1.Draw2dTexture(game1.WhiteTexture(), screenposX - w, screenposY - h, w, h, null,0, Game.ColorFromArgb(255, 0,0,0), false);
            game1.Draw2dTexture(game1.WhiteTexture(), screenposX - w + 2, screenposY - h + 2, w - 4, h - 4, null, 0, Game.ColorFromArgb(255, 105, 105, 105), false);
            FontCi font = new FontCi();
            font.family = "Arial";
            font.size = 10;
            game1.Draw2dText(dataItems.ItemInfo(item), font, screenposX - tw.value + 4, screenposY - h + 2, null, false);
            Packet_Item item2 = new Packet_Item();
            item2.BlockId = item.BlockId;
            DrawItem(screenposX - w + 2, screenposY - h + 2, item2, 0, 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using ManicDigger.Renderers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using ManicDigger.Hud;

namespace ManicDigger.Tests
{
    public class TestInventory : MyGameWindow, IDisposable, IMouseCurrent
    {
        public GameWindow window;
        public IGetFileStream getfile;
        public IViewportSize viewportsize;
        public override void OnLoad(EventArgs e)
        {
            window.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            the3d.d_Config3d = new Config3d();
            the3d.d_GetFile = getfile;
            the3d.d_Terrain = new TerrainTextures();
            the3d.d_TextRenderer = new TextRenderer();
            the3d.d_ViewportSize = viewportsize;

            var dataItems = new GameDataItems();
            var inventory = new Inventory();
            var server = new InventoryServer();
            var inventoryUtil = new InventoryUtil();
            hud = new HudInventory();
            hud.dataItems = dataItems;
            hud.inventory = inventory;
            hud.inventoryUtil = inventoryUtil;
            hud.controller = server;
            hud.viewport_size = viewportsize;
            hud.mouse_current = this;
            hud.the3d = the3d;
            hud.getfile = getfile;
            hud.ActiveMaterial = new ActiveMaterialDummy();
            hud.viewport3d = new ViewportDummy();
            server.d_Items = dataItems;
            server.d_Inventory = inventory;
            server.d_InventoryUtil = inventoryUtil;
            server.d_DropItem = new DropItemDummy();
            inventoryUtil.d_Inventory = inventory;
            inventoryUtil.d_Items = dataItems;

            for (int i = 0; i < 10; i++)
            {
                inventory.Items.Add(new ProtoPoint(i * 1, 0), new Item() { ItemClass = ItemClass.Block, BlockId = i });
                inventory.Items.Add(new ProtoPoint(i * 1, 6), new Item() { ItemClass = ItemClass.Block, BlockId = i });
            }
            inventory.RightHand[0] = new Item() { ItemClass = ItemClass.Weapon, ItemId = "inventory_weapon_hand_axe.png" };
            inventory.Boots = new Item() { ItemClass = ItemClass.Boots, ItemId = "inventory_boots_shoes.png" };
            inventory.Gauntlet = new Item() { ItemClass = ItemClass.Gauntlet, ItemId = "inventory_gauntlet_gloves.png" };
            inventory.Helmet = new Item() { ItemClass = ItemClass.Helmet, ItemId = "inventory_helmet_zischagge.png" };
            inventory.MainArmor = new Item() { ItemClass = ItemClass.MainArmor, ItemId = "inventory_armor_plate_mail.png" };
            inventory.LeftHand[0] = new Item() { ItemClass = ItemClass.Shield, ItemId = "inventory_shield_small_shield.png" };
            inventory.Items.Add(new ProtoPoint(0, 1), new Item() { ItemClass = ItemClass.Weapon, ItemId = "inventory_weapon_shovel.png" });
            inventory.Items.Add(new ProtoPoint(2, 1), new Item() { ItemClass = ItemClass.Weapon, ItemId = "inventory_weapon_knife.png" });
            inventory.Items.Add(new ProtoPoint(3, 1), new Item() { ItemClass = ItemClass.Weapon, ItemId = "inventory_weapon_club.png" });
            inventory.Items.Add(new ProtoPoint(4, 1), new Item() { ItemClass = ItemClass.MainArmor, ItemId = "inventory_armor_plate_mail.png" });
            inventory.Items.Add(new ProtoPoint(6, 1), new Item() { ItemClass = ItemClass.Weapon, ItemId = "inventory_weapon_maul.png" });
            inventory.Items.Add(new ProtoPoint(8, 1), new Item() { ItemClass = ItemClass.Weapon, ItemId = "inventory_sword.png" });
            inventory.Items.Add(new ProtoPoint(9, 1), new Item() { ItemClass = ItemClass.Other, ItemId = "inventory_health_potion.png" });
        }
        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            hud.Mouse_ButtonDown(sender, e);
        }
        The3d the3d = new The3d();
        HudInventory hud;
        public override void OnRenderFrame(FrameEventArgs e)
        {
            mouse_current = System.Windows.Forms.Cursor.Position;
            mouse_current.Offset(-window.X, -window.Y);
            mouse_current.Offset(0, -20);

            the3d.OrthoMode(window.Width, window.Height);
            hud.OnRenderFrame(e);
        }
        public override void OnKeyPress(KeyPressEventArgs e)
        {
            hud.OnKeyPress(e);
        }
        public override void OnResize(EventArgs e)
        {
            //the3d.ResizeGraphics(viewportsize.Width, viewportsize.Height);
            GL.Viewport(0, 0, window.Width, window.Height);
            the3d.Set3dProjection();
        }
        public void Dispose()
        {
        }
        Point mouse_current;
        public Point MouseCurrent
        {
            get { return mouse_current; }
        }
    }
    class GameDataItems : IGameDataItems
    {
        public string ItemGraphics(Item item)
        {
            return item.ItemId; //temp
        }
        //dummy
        public Point ItemSize(Item item)
        {
            if (item.ItemClass == ItemClass.Block) { return new Point(1, 1); }
            if (item.ItemClass == ItemClass.Weapon)
            {
                if (item.ItemId.Contains("shovel")) { return new Point(2, 4); }
                if (item.ItemId.Contains("knife")) { return new Point(1, 2); }
                if (item.ItemId.Contains("club")) { return new Point(1, 3); }
                if (item.ItemId.Contains("arm")) { return new Point(2, 3); }
                if (item.ItemId.Contains("maul")) { return new Point(2, 4); }
                if (item.ItemId.Contains("sword")) { return new Point(1, 4); }
                if (item.ItemId.Contains("axe")) { return new Point(1, 3); }
            }//temp
            if (item.ItemClass == ItemClass.Boots) { return new Point(2, 2); }
            if (item.ItemClass == ItemClass.Gauntlet) { return new Point(2, 2); }
            if (item.ItemClass == ItemClass.Helmet) { return new Point(2, 2); }
            if (item.ItemClass == ItemClass.MainArmor) { return new Point(2, 4); }
            if (item.ItemClass == ItemClass.Shield) { return new Point(2, 2); }
            if (item.ItemClass == ItemClass.Other) { return new Point(1, 1); }
            throw new Exception();
        }
        //dummy
        public string ItemInfo(Item item)
        {
            if (item.ItemClass == ItemClass.Block)
            {
                return BlockName(item.BlockId);
            }
            else
            {
                return item.ItemId;
            }
        }
        //dummy
        public string BlockName(int blockId)
        {
            return "Dirt";
        }
        public Item Stack(Item itemA, Item itemB)
        {
            if (itemA.ItemClass == ItemClass.Block
                && itemB.ItemClass == ItemClass.Block)
            {
                if (itemA.BlockId != itemB.BlockId) { return null; }
                //todo stack size limit
                Item ret = new Item();
                ret.ItemClass = itemA.ItemClass;
                ret.BlockId = itemA.BlockId;
                ret.BlockCount = itemA.BlockCount + itemB.BlockCount;
                return ret;
            }
            else
            {
                return null;
            }
        }
        public bool CanWear(WearPlace selectedWear, Item item)
        {
            if (item == null) { return true; }
            switch (selectedWear)
            {
                case WearPlace.LeftHand: return item.ItemClass == ItemClass.Shield;
                case WearPlace.RightHand:
                    return item.ItemClass == ItemClass.Weapon
                        || item.ItemClass == ItemClass.Block
                        || item.ItemClass == ItemClass.Other;
                case WearPlace.MainArmor: return item.ItemClass == ItemClass.MainArmor;
                case WearPlace.Boots: return item.ItemClass == ItemClass.Boots;
                case WearPlace.Helmet: return item.ItemClass == ItemClass.Helmet;
                case WearPlace.Gauntlet: return item.ItemClass == ItemClass.Gauntlet;
                default: throw new Exception();
            }
        }


        public int[] TextureIdForInventory
        {
            get
            {
                int[] t = new int[256];
                for (int i = 0; i < 256; i++)
                {
                    t[i] = i;
                }
                return t;
            }
        }
    }
    public class DropItemDummy : IDropItem
    {
        public void DropItem(ref Item item, Vector3i pos)
        {
        }
    }
}

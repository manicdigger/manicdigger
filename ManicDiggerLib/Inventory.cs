using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ProtoBuf;
using System.Runtime.Serialization;

namespace ManicDigger
{
    public enum ItemClass
    {
        Block,
        Weapon,
        MainArmor,
        Boots,
        Helmet,
        Gauntlet,
        Shield,
        Other,
    }

    public enum WeaponType
    {
        Hammer,
        OneHandedSword,
        Axe,
        Spear,
        TwoHandedSword,
    }

    [ProtoContract]
    public class Item
    {
        [ProtoMember(1, IsRequired = false)]
        public ItemClass ItemClass;
        [ProtoMember(2, IsRequired = false)]
        public string ItemId;
        [ProtoMember(3, IsRequired = false)]
        public int BlockId;
        [ProtoMember(4, IsRequired = false)]
        public int BlockCount = 1;
    }

    [ProtoContract]
    public class Inventory
    {
        [OnDeserialized()]
        void OnDeserialized()
        {
            LeftHand = new Item[10];
            if (LeftHandProto != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (LeftHandProto.ContainsKey(i))
                    {
                        LeftHand[i] = LeftHandProto[i];
                    }
                }
            }
            RightHand = new Item[10];
            if (RightHandProto != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (RightHandProto.ContainsKey(i))
                    {
                        RightHand[i] = RightHandProto[i];
                    }
                }
            }
        }
        [OnSerializing()]
        void OnSerializing()
        {
            Dictionary<int, Item> d = new Dictionary<int, Item>();
            for (int i = 0; i < 10; i++)
            {
                if (LeftHand[i] != null)
                {
                    d[i] = LeftHand[i];
                }
            }
            LeftHandProto = d;

            d = new Dictionary<int, Item>();
            for (int i = 0; i < 10; i++)
            {
                if (RightHand[i] != null)
                {
                    d[i] = RightHand[i];
                }
            }
            RightHandProto = d;
        }
        //dictionary because protobuf-net can't serialize array of nulls.
        [ProtoMember(1, IsRequired = false)]
        public Dictionary<int, Item> LeftHandProto;
        [ProtoMember(2, IsRequired = false)]
        public Dictionary<int, Item> RightHandProto;
        public Item[] LeftHand = new Item[10];
        public Item[] RightHand = new Item[10];
        [ProtoMember(3, IsRequired = false)]
        public Item MainArmor;
        [ProtoMember(4, IsRequired = false)]
        public Item Boots;
        [ProtoMember(5, IsRequired = false)]
        public Item Helmet;
        [ProtoMember(6, IsRequired = false)]
        public Item Gauntlet;
        [ProtoMember(7, IsRequired = false)]
        public Dictionary<ProtoPoint, Item> Items = new Dictionary<ProtoPoint, Item>();
        [ProtoMember(8, IsRequired = false)]
        public Item DragDropItem;
        public void CopyFrom(Inventory inventory)
        {
            this.LeftHand = inventory.LeftHand;
            this.RightHand = inventory.RightHand;
            this.MainArmor = inventory.MainArmor;
            this.Boots = inventory.Boots;
            this.Helmet = inventory.Helmet;
            this.Gauntlet = inventory.Gauntlet;
            this.Items = inventory.Items;
            this.DragDropItem = inventory.DragDropItem;
        }
        public static Inventory Create()
        {
            Inventory i = new Inventory();
            i.LeftHand = new Item[10];
            i.RightHand = new Item[10];
            return i;
        }
    }

    //separate class because it's used by server and client.
    public class InventoryUtil
    {
        public Inventory d_Inventory;
        public IGameDataItems d_Items;

        public Point CellCount = new Point(12, 7);

        //returns null if area is invalid.
        public Point[] ItemsAtArea(Point p, Point size)
        {
            List<Point> itemsAtArea = new List<Point>();
            for (int xx = 0; xx < size.X; xx++)
            {
                for (int yy = 0; yy < size.Y; yy++)
                {
                    var cell = new Point(p.X + xx, p.Y + yy);
                    if (!IsValidCell(cell))
                    {
                        return null;
                    }
                    if (ItemAtCell(cell) != null)
                    {
                        if (!itemsAtArea.Contains(ItemAtCell(cell).Value))
                        {
                            itemsAtArea.Add(ItemAtCell(cell).Value);
                        }
                    }
                }
            }
            return itemsAtArea.ToArray();
        }

        public bool IsValidCell(Point p)
        {
            return !(p.X < 0 || p.Y < 0 || p.X >= CellCount.X || p.Y >= CellCount.Y);
        }

        public IEnumerable<Point> ItemCells(Point p)
        {
            Item item = d_Inventory.Items[new ProtoPoint(p)];
            for (int x = 0; x < d_Items.ItemSize(item).X; x++)
            {
                for (int y = 0; y < d_Items.ItemSize(item).Y; y++)
                {
                    yield return new Point(p.X + x, p.Y + y);
                }
            }
        }

        public Point? ItemAtCell(Point p)
        {
            foreach (var k in d_Inventory.Items)
            {
                foreach (var pp in ItemCells(k.Key.ToPoint()))
                {
                    if (p == pp) { return k.Key.ToPoint(); }
                }
            }
            return null;
        }

        public Item ItemAtWearPlace(WearPlace wearPlace, int activeMaterial)
        {
            switch (wearPlace)
            {
                case WearPlace.LeftHand: return d_Inventory.LeftHand[activeMaterial];
                case WearPlace.RightHand: return d_Inventory.RightHand[activeMaterial];
                case WearPlace.MainArmor: return d_Inventory.MainArmor;
                case WearPlace.Boots: return d_Inventory.Boots;
                case WearPlace.Helmet: return d_Inventory.Helmet;
                case WearPlace.Gauntlet: return d_Inventory.Gauntlet;
                default: throw new Exception();
            }
        }

        public void SetItemAtWearPlace(WearPlace wearPlace, int activeMaterial, Item item)
        {
            switch (wearPlace)
            {
                case WearPlace.LeftHand: d_Inventory.LeftHand[activeMaterial] = item; break;
                case WearPlace.RightHand: d_Inventory.RightHand[activeMaterial] = item; break;
                case WearPlace.MainArmor: d_Inventory.MainArmor = item; break;
                case WearPlace.Boots: d_Inventory.Boots = item; break;
                case WearPlace.Helmet: d_Inventory.Helmet = item; break;
                case WearPlace.Gauntlet: d_Inventory.Gauntlet = item; break;
                default: throw new Exception();
            }
        }

        public bool GrabItem(Item item, int ActiveMaterial)
        {
            switch (item.ItemClass)
            {
                case ItemClass.Block:
                    if (item.BlockId == SpecialBlockId.Empty)
                    {
                        return true;
                    }
                    //stacking
                    for (int i = 0; i < 10; i++)
                    {
                        if (d_Inventory.RightHand[i] == null)
                        {
                            continue;
                        }
                        Item result = d_Items.Stack(d_Inventory.RightHand[i], item);
                        if (result != null)
                        {
                            d_Inventory.RightHand[i] = result;
                            return true;
                        }
                    }
                    if (d_Inventory.RightHand[ActiveMaterial] == null)
                    {
                        d_Inventory.RightHand[ActiveMaterial] = item;
                        return true;
                    }
                    //current hand
                    if (d_Inventory.RightHand[ActiveMaterial].ItemClass == ItemClass.Block
                        && d_Inventory.RightHand[ActiveMaterial].BlockId == item.BlockId)
                    {
                        d_Inventory.RightHand[ActiveMaterial].BlockCount++;
                        return true;
                    }
                    //any free hand
                    for (int i = 0; i < 10; i++)
                    {
                        if (d_Inventory.RightHand[i] == null)
                        {
                            d_Inventory.RightHand[i] = item;
                            return true;
                        }
                    }
                    //grab to main area - stacking
                    for (int x = 0; x < CellCount.X; x++)
                    {
                        for (int y = 0; y < CellCount.Y; y++)
                        {
                            var p = ItemsAtArea(new Point(x, y), d_Items.ItemSize(item));
                            if (p != null && p.Length == 1)
                            {
                                var stacked = d_Items.Stack(d_Inventory.Items[new ProtoPoint(p[0])], item);
                                if (stacked != null)
                                {
                                    d_Inventory.Items[new ProtoPoint(x, y)] = stacked;
                                    return true;
                                }
                            }
                        }
                    }
                    //grab to main area - adding
                    for (int x = 0; x < CellCount.X; x++)
                    {
                        for (int y = 0; y < CellCount.Y; y++)
                        {
                            var p = ItemsAtArea(new Point(x, y), d_Items.ItemSize(item));
                            if (p != null && p.Length == 0)
                            {
                                d_Inventory.Items[new ProtoPoint(x, y)] = item;
                                return true;
                            }
                        }
                    }
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }

        public int? FreeHand(int ActiveMaterial)
        {
            int? freehand = null;
            if (d_Inventory.RightHand[ActiveMaterial] == null) return ActiveMaterial;
            for (int i = 0; i < d_Inventory.RightHand.Length; i++)
            {
                if (d_Inventory.RightHand[i] == null)
                {
                    freehand = i;
                }
            }
            return freehand;
        }
    }

    public enum WearPlace
    {
        LeftHand,
        RightHand,
        MainArmor,
        Boots,
        Helmet,
        Gauntlet,
    }

    public enum InventoryPositionType
    {
        MainArea,
        Ground,
        MaterialSelector,
        WearPlace,
    }

    [ProtoContract]
    public class InventoryPosition
    {
        [ProtoMember(1, IsRequired = false)]
        public InventoryPositionType type;
        [ProtoMember(2, IsRequired = false)]
        public int AreaX;
        [ProtoMember(3, IsRequired = false)]
        public int AreaY;
        [ProtoMember(4, IsRequired = false)]
        public int MaterialId;

        //WearPlace
        [ProtoMember(5, IsRequired = false)]
        public int WearPlace;
        [ProtoMember(6, IsRequired = false)]
        public int ActiveMaterial;
        [ProtoMember(7, IsRequired = false)]
        public int GroundPositionX;
        [ProtoMember(8, IsRequired = false)]
        public int GroundPositionY;
        [ProtoMember(9, IsRequired = false)]
        public int GroundPositionZ;

        public static InventoryPosition MaterialSelector(int materialId)
        {
            InventoryPosition pos = new InventoryPosition();
            pos.type = InventoryPositionType.MaterialSelector;
            pos.MaterialId = materialId;
            return pos;
        }

        public static InventoryPosition MainArea(Point point)
        {
            InventoryPosition pos = new InventoryPosition();
            pos.type = InventoryPositionType.MainArea;
            pos.AreaX = point.X;
            pos.AreaY = point.Y;
            return pos;
        }
    }

    public interface IGameDataItems
    {
        string ItemInfo(Item item);
        Point ItemSize(Item item);
        /// <summary>
        /// returns null if can't stack.
        /// </summary>
        Item Stack(Item itemA, Item itemB);
        bool CanWear(WearPlace selectedWear, Item item);
        string ItemGraphics(Item item);
        int[] TextureIdForInventory { get; }
    }

    public interface IInventoryController
    {
        void InventoryClick(InventoryPosition pos);
        void WearItem(InventoryPosition from, InventoryPosition to);
        void MoveToInventory(InventoryPosition from);
    }

    public interface IDropItem
    {
        void DropItem(ref Item item, Vector3i pos);
    }

    public class InventoryServer : IInventoryController
    {
        public IGameDataItems d_Items;
        public Inventory d_Inventory;
        public InventoryUtil d_InventoryUtil;
        public IDropItem d_DropItem;

        public void InventoryClick(InventoryPosition pos)
        {
            if (pos.type == InventoryPositionType.MainArea)
            {
                Point? selected = null;
                foreach (var k in d_Inventory.Items)
                {
                    if (pos.AreaX >= k.Key.X && pos.AreaY >= k.Key.Y
                        && pos.AreaX < k.Key.X + d_Items.ItemSize(k.Value).X
                        && pos.AreaY < k.Key.Y + d_Items.ItemSize(k.Value).Y)
                    {
                        selected = k.Key.ToPoint();
                    }
                }
                //drag
                if (selected != null && d_Inventory.DragDropItem == null)
                {
                    d_Inventory.DragDropItem = d_Inventory.Items[new ProtoPoint(selected.Value)];
                    d_Inventory.Items.Remove(new ProtoPoint(selected.Value));
                    SendInventory();
                }
                //drop
                else if (d_Inventory.DragDropItem != null)
                {
                    //make sure there is nothing blocking drop.
                    Point[] itemsAtArea = d_InventoryUtil.ItemsAtArea(new Point(pos.AreaX, pos.AreaY),
                        d_Items.ItemSize(d_Inventory.DragDropItem));
                    if (itemsAtArea == null || itemsAtArea.Length > 1)
                    {
                        //invalid area
                        return;
                    }
                    if (itemsAtArea.Length == 0)
                    {
                        d_Inventory.Items.Add(new ProtoPoint(pos.AreaX, pos.AreaY), d_Inventory.DragDropItem);
                        d_Inventory.DragDropItem = null;
                    }
                    else //1
                    {
                        var swapWith = itemsAtArea[0];
                        //try to stack                        
                        Item stackResult = d_Items.Stack(d_Inventory.Items[new ProtoPoint(swapWith)], d_Inventory.DragDropItem);
                        if (stackResult != null)
                        {
                            d_Inventory.Items[new ProtoPoint(swapWith)] = stackResult;
                            d_Inventory.DragDropItem = null;
                        }
                        else
                        {
                            //try to swap
                            //swap (swapWith, dragdropitem)
                            Item z = d_Inventory.Items[new ProtoPoint(swapWith)];
                            d_Inventory.Items.Remove(new ProtoPoint(swapWith));
                            d_Inventory.Items[new ProtoPoint(pos.AreaX, pos.AreaY)] = d_Inventory.DragDropItem;
                            d_Inventory.DragDropItem = z;
                        }
                    }
                    SendInventory();
                }
            }
            else if (pos.type == InventoryPositionType.Ground)
            {
                /*
                if (d_Inventory.DragDropItem != null)
                {
                    d_DropItem.DropItem(ref d_Inventory.DragDropItem,
                        new Vector3i(pos.GroundPositionX, pos.GroundPositionY, pos.GroundPositionZ));
                    SendInventory();
                }
                */
            }
            else if (pos.type == InventoryPositionType.MaterialSelector)
            {
                if (d_Inventory.DragDropItem == null && d_Inventory.RightHand[pos.MaterialId] != null)
                {
                    d_Inventory.DragDropItem = d_Inventory.RightHand[pos.MaterialId];
                    d_Inventory.RightHand[pos.MaterialId] = null;
                }
                else if (d_Inventory.DragDropItem != null && d_Inventory.RightHand[pos.MaterialId] == null)
                {
                    if(d_Items.CanWear(WearPlace.RightHand, d_Inventory.DragDropItem))
                    {
                        d_Inventory.RightHand[pos.MaterialId] = d_Inventory.DragDropItem;
                        d_Inventory.DragDropItem = null;
                    }
                }
                else if (d_Inventory.DragDropItem != null && d_Inventory.RightHand[pos.MaterialId] != null)
                {
                    if (d_Items.CanWear(WearPlace.RightHand, d_Inventory.DragDropItem))
                    {
                        Item oldHand = d_Inventory.RightHand[pos.MaterialId];
                        d_Inventory.RightHand[pos.MaterialId] = d_Inventory.DragDropItem;
                        d_Inventory.DragDropItem = oldHand;
                    }
                }
                SendInventory();
            }
            else if (pos.type == InventoryPositionType.WearPlace)
            {
                //just swap.
                Item wear = d_InventoryUtil.ItemAtWearPlace((WearPlace)pos.WearPlace, pos.ActiveMaterial);
                if (d_Items.CanWear((WearPlace)pos.WearPlace, d_Inventory.DragDropItem))
                {
                    d_InventoryUtil.SetItemAtWearPlace((WearPlace)pos.WearPlace, pos.ActiveMaterial, d_Inventory.DragDropItem);
                    d_Inventory.DragDropItem = wear;
                }
                SendInventory();
            }
            else
            {
                throw new Exception();
            }
        }
        private void SendInventory()
        {
        }

        public void WearItem(InventoryPosition from, InventoryPosition to)
        {
            //todo
            if (from.type == InventoryPositionType.MainArea
                && to.type == InventoryPositionType.MaterialSelector
                && d_Inventory.RightHand[to.MaterialId] == null
                && d_Items.CanWear(WearPlace.RightHand, d_Inventory.Items[new ProtoPoint(from.AreaX, from.AreaY)]))
            {
                d_Inventory.RightHand[to.MaterialId] = d_Inventory.Items[new ProtoPoint(from.AreaX, from.AreaY)];
                d_Inventory.Items.Remove(new ProtoPoint(from.AreaX, from.AreaY));
            }
        }

        public void MoveToInventory(InventoryPosition from)
        {
            //todo
            if (from.type == InventoryPositionType.MaterialSelector)
            {
                //duplicate code with GrabItem().

                Item item = d_Inventory.RightHand[from.MaterialId];
                //grab to main area - stacking
                for (int x = 0; x < d_InventoryUtil.CellCount.X; x++)
                {
                    for (int y = 0; y < d_InventoryUtil.CellCount.Y; y++)
                    {
                        var p = d_InventoryUtil.ItemsAtArea(new Point(x, y), d_Items.ItemSize(item));
                        if (p != null && p.Length == 1)
                        {
                            var stacked = d_Items.Stack(d_Inventory.Items[new ProtoPoint(p[0])], item);
                            if (stacked != null)
                            {
                                d_Inventory.Items[new ProtoPoint(x, y)] = stacked;
                                d_Inventory.RightHand[from.MaterialId] = null;
                                return;
                            }
                        }
                    }
                }
                //grab to main area - adding
                for (int x = 0; x < d_InventoryUtil.CellCount.X; x++)
                {
                    for (int y = 0; y < d_InventoryUtil.CellCount.Y; y++)
                    {
                        var p = d_InventoryUtil.ItemsAtArea(new Point(x, y), d_Items.ItemSize(item));
                        if (p != null && p.Length == 0)
                        {
                            d_Inventory.Items[new ProtoPoint(x, y)] = item;
                            d_Inventory.RightHand[from.MaterialId] = null;
                            return;
                        }
                    }
                }
            }
        }
    }
    public class GameDataItemsBlocks : IGameDataItems
    {
        public IGameData d_Data;
        public string ItemInfo(Item item)
        {
            if (item.ItemClass == ItemClass.Block) { return d_Data.Name[item.BlockId]; }
            throw new NotImplementedException();
        }

        public Point ItemSize(Item item)
        {
            if (item.ItemClass == ItemClass.Block) { return new Point(1, 1); }
            throw new NotImplementedException();
        }

        public Item Stack(Item itemA, Item itemB)
        {
            if (itemA.ItemClass == ItemClass.Block
                && itemB.ItemClass == ItemClass.Block)
            {
                int railcountA = MyLinq.Count(DirectionUtils.ToRailDirections(d_Data.Rail[itemA.BlockId]));
                int railcountB = MyLinq.Count(DirectionUtils.ToRailDirections(d_Data.Rail[itemB.BlockId]));
                if ((itemA.BlockId != itemB.BlockId) && (!(railcountA > 0 && railcountB > 0)))
                {
                    return null;
                }
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
            if (item == null) { return true; }
            switch (selectedWear)
            {
                case WearPlace.LeftHand: return false;
                case WearPlace.RightHand: return item.ItemClass == ItemClass.Block;
                case WearPlace.MainArmor: return false;
                case WearPlace.Boots: return false;
                case WearPlace.Helmet: return false;
                case WearPlace.Gauntlet: return false;
                default: throw new Exception();
            }
        }

        public string ItemGraphics(Item item)
        {
            throw new NotImplementedException();
        }


        public int[] TextureIdForInventory
        {
            get { return d_Data.TextureIdForInventory; }
        }
    }
}

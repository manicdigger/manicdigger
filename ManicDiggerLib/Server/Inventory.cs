using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ProtoBuf;
using System.Runtime.Serialization;
using ManicDigger;

//separate class because it's used by server and client.
public class InventoryUtil
{
    public InventoryUtil()
    {
        CellCountX = 12;
        CellCountY = 7 * 6;
    }
    public Inventory d_Inventory;
    public IGameDataItems d_Items;

    internal int CellCountX;
    internal int CellCountY;

    //returns null if area is invalid.
    public PointRef[] ItemsAtArea(int pX, int pY, int sizeX, int sizeY, IntRef retCount)
    {
        PointRef[] itemsAtArea = new PointRef[256];
        int itemsAtAreaCount = 0;
        for (int xx = 0; xx < sizeX; xx++)
        {
            for (int yy = 0; yy < sizeY; yy++)
            {
                PointRef cell = PointRef.Create(pX + xx, pY + yy);
                if (!IsValidCell(cell))
                {
                    return null;
                }
                if (ItemAtCell(cell) != null)
                {
                    bool contains = false;
                    for (int i = 0; i < itemsAtAreaCount; i++)
                    {
                        if (itemsAtArea[i] == null)
                        {
                            continue;
                        }
                        if (itemsAtArea[i].X == ItemAtCell(cell).X
                            && itemsAtArea[i].Y == ItemAtCell(cell).Y)
                        {
                            contains = true;
                        }
                    }
                    if (!contains)
                    {
                        itemsAtArea[itemsAtAreaCount++] = ItemAtCell(cell);
                    }
                }
            }
        }
        retCount.value = itemsAtAreaCount;
        return itemsAtArea;
    }

    public bool IsValidCell(PointRef p)
    {
        return !(p.X < 0 || p.Y < 0 || p.X >= CellCountX || p.Y >= CellCountY);
    }

    public IEnumerable<PointRef> ItemCells(PointRef p)
    {
        Item item = d_Inventory.Items[new ProtoPoint(p.X, p.Y)];
        for (int x = 0; x < d_Items.ItemSizeX(item); x++)
        {
            for (int y = 0; y < d_Items.ItemSizeY(item); y++)
            {
                yield return PointRef.Create(p.X + x, p.Y + y);
            }
        }
    }

    public PointRef ItemAtCell(PointRef p)
    {
        foreach (var k in d_Inventory.Items)
        {
            foreach (var pp in ItemCells(PointRef.Create(k.Key.X, k.Key.Y)))
            {
                if (p.X == pp.X && p.Y == pp.Y) { return PointRef.Create(k.Key.X, k.Key.Y); }
            }
        }
        return null;
    }

    public Item ItemAtWearPlace(int wearPlace, int activeMaterial)
    {
        switch (wearPlace)
        {
            //case WearPlace.LeftHand: return d_Inventory.LeftHand[activeMaterial];
            case WearPlace_.RightHand: return d_Inventory.RightHand[activeMaterial];
            case WearPlace_.MainArmor: return d_Inventory.MainArmor;
            case WearPlace_.Boots: return d_Inventory.Boots;
            case WearPlace_.Helmet: return d_Inventory.Helmet;
            case WearPlace_.Gauntlet: return d_Inventory.Gauntlet;
            default: throw new Exception();
        }
    }

    public void SetItemAtWearPlace(int wearPlace, int activeMaterial, Item item)
    {
        switch (wearPlace)
        {
            //case WearPlace.LeftHand: d_Inventory.LeftHand[activeMaterial] = item; break;
            case WearPlace_.RightHand: d_Inventory.RightHand[activeMaterial] = item; break;
            case WearPlace_.MainArmor: d_Inventory.MainArmor = item; break;
            case WearPlace_.Boots: d_Inventory.Boots = item; break;
            case WearPlace_.Helmet: d_Inventory.Helmet = item; break;
            case WearPlace_.Gauntlet: d_Inventory.Gauntlet = item; break;
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
                for (int y = 0; y < CellCountY; y++)
                {
                    for (int x = 0; x < CellCountX; x++)
                    {
                        IntRef pCount = new IntRef();
                        PointRef[] p = ItemsAtArea(x, y, d_Items.ItemSizeX(item), d_Items.ItemSizeY(item), pCount);
                        if (p != null && pCount.value == 1)
                        {
                            var stacked = d_Items.Stack(d_Inventory.Items[new ProtoPoint(p[0].X, p[0].Y)], item);
                            if (stacked != null)
                            {
                                d_Inventory.Items[new ProtoPoint(x, y)] = stacked;
                                return true;
                            }
                        }
                    }
                }
                //grab to main area - adding
                for (int y = 0; y < CellCountY; y++)
                {
                    for (int x = 0; x < CellCountX; x++)
                    {
                        IntRef pCount = new IntRef();
                        PointRef[] p = ItemsAtArea(x, y, d_Items.ItemSizeX(item), d_Items.ItemSizeY(item), pCount);
                        if (p != null && pCount.value == 0)
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
                return freehand;
            }
        }
        return null;
    }
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
    int ItemSizeX(Item item);
    int ItemSizeY(Item item);
    /// <summary>
    /// returns null if can't stack.
    /// </summary>
    Item Stack(Item itemA, Item itemB);
    bool CanWear(int selectedWear, Item item);
    string ItemGraphics(Item item);
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

    public override void InventoryClick(Packet_InventoryPosition pos)
    {
        if (pos.Type == Packet_InventoryPositionTypeEnum.MainArea)
        {
            Point? selected = null;
            foreach (var k in d_Inventory.Items)
            {
                if (pos.AreaX >= k.Key.X && pos.AreaY >= k.Key.Y
                    && pos.AreaX < k.Key.X + d_Items.ItemSizeX(k.Value)
                    && pos.AreaY < k.Key.Y + d_Items.ItemSizeY(k.Value))
                {
                    selected = new Point(k.Key.X, k.Key.Y);
                }
            }
            //drag
            if (selected != null && d_Inventory.DragDropItem == null)
            {
                d_Inventory.DragDropItem = d_Inventory.Items[new ProtoPoint(selected.Value.X, selected.Value.Y)];
                d_Inventory.Items.Remove(new ProtoPoint(selected.Value.X, selected.Value.Y));
                SendInventory();
            }
            //drop
            else if (d_Inventory.DragDropItem != null)
            {
                //make sure there is nothing blocking drop.
                IntRef itemsAtAreaCount = new IntRef();
                PointRef[] itemsAtArea = d_InventoryUtil.ItemsAtArea(pos.AreaX, pos.AreaY,
                    d_Items.ItemSizeX(d_Inventory.DragDropItem), d_Items.ItemSizeY(d_Inventory.DragDropItem), itemsAtAreaCount);
                if (itemsAtArea == null || itemsAtAreaCount.value > 1)
                {
                    //invalid area
                    return;
                }
                if (itemsAtAreaCount.value == 0)
                {
                    d_Inventory.Items.Add(new ProtoPoint(pos.AreaX, pos.AreaY), d_Inventory.DragDropItem);
                    d_Inventory.DragDropItem = null;
                }
                else //1
                {
                    var swapWith = itemsAtArea[0];
                    //try to stack                        
                    Item stackResult = d_Items.Stack(d_Inventory.Items[new ProtoPoint(swapWith.X, swapWith.Y)], d_Inventory.DragDropItem);
                    if (stackResult != null)
                    {
                        d_Inventory.Items[new ProtoPoint(swapWith.X, swapWith.Y)] = stackResult;
                        d_Inventory.DragDropItem = null;
                    }
                    else
                    {
                        //try to swap
                        //swap (swapWith, dragdropitem)
                        Item z = d_Inventory.Items[new ProtoPoint(swapWith.X, swapWith.Y)];
                        d_Inventory.Items.Remove(new ProtoPoint(swapWith.X, swapWith.Y));
                        d_Inventory.Items[new ProtoPoint(pos.AreaX, pos.AreaY)] = d_Inventory.DragDropItem;
                        d_Inventory.DragDropItem = z;
                    }
                }
                SendInventory();
            }
        }
        else if (pos.Type == Packet_InventoryPositionTypeEnum.Ground)
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
        else if (pos.Type == Packet_InventoryPositionTypeEnum.MaterialSelector)
        {
            if (d_Inventory.DragDropItem == null && d_Inventory.RightHand[pos.MaterialId] != null)
            {
                d_Inventory.DragDropItem = d_Inventory.RightHand[pos.MaterialId];
                d_Inventory.RightHand[pos.MaterialId] = null;
            }
            else if (d_Inventory.DragDropItem != null && d_Inventory.RightHand[pos.MaterialId] == null)
            {
                if (d_Items.CanWear(WearPlace_.RightHand, d_Inventory.DragDropItem))
                {
                    d_Inventory.RightHand[pos.MaterialId] = d_Inventory.DragDropItem;
                    d_Inventory.DragDropItem = null;
                }
            }
            else if (d_Inventory.DragDropItem != null && d_Inventory.RightHand[pos.MaterialId] != null)
            {
                if (d_Items.CanWear(WearPlace_.RightHand, d_Inventory.DragDropItem))
                {
                    Item oldHand = d_Inventory.RightHand[pos.MaterialId];
                    d_Inventory.RightHand[pos.MaterialId] = d_Inventory.DragDropItem;
                    d_Inventory.DragDropItem = oldHand;
                }
            }
            SendInventory();
        }
        else if (pos.Type == Packet_InventoryPositionTypeEnum.WearPlace)
        {
            //just swap.
            Item wear = d_InventoryUtil.ItemAtWearPlace(pos.WearPlace, pos.ActiveMaterial);
            if (d_Items.CanWear(pos.WearPlace, d_Inventory.DragDropItem))
            {
                d_InventoryUtil.SetItemAtWearPlace(pos.WearPlace, pos.ActiveMaterial, d_Inventory.DragDropItem);
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

    public override void WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
    {
        //todo
        ProtoPoint originPoint = new ProtoPoint(from.AreaX, from.AreaY);
        if (from.Type == Packet_InventoryPositionTypeEnum.MainArea
            && to.Type == Packet_InventoryPositionTypeEnum.MaterialSelector
            && d_Inventory.RightHand[to.MaterialId] == null
            && d_Inventory.Items.ContainsKey(originPoint)
            && d_Items.CanWear(WearPlace_.RightHand, d_Inventory.Items[originPoint]))
        {
            d_Inventory.RightHand[to.MaterialId] = d_Inventory.Items[originPoint];
            d_Inventory.Items.Remove(originPoint);
        }
    }

    public override void MoveToInventory(Packet_InventoryPosition from)
    {
        //todo
        if (from.Type == Packet_InventoryPositionTypeEnum.MaterialSelector)
        {
            //duplicate code with GrabItem().

            Item item = d_Inventory.RightHand[from.MaterialId];
            if (item == null)
            {
                return;
            }
            //grab to main area - stacking
            for (int x = 0; x < d_InventoryUtil.CellCountX; x++)
            {
                for (int y = 0; y < d_InventoryUtil.CellCountY; y++)
                {
                    IntRef pCount = new IntRef();
                    PointRef[] p = d_InventoryUtil.ItemsAtArea(x, y, d_Items.ItemSizeX(item), d_Items.ItemSizeY(item), pCount);
                    if (p != null && pCount.value == 1)
                    {
                        var stacked = d_Items.Stack(d_Inventory.Items[new ProtoPoint(p[0].X, p[0].Y)], item);
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
            for (int x = 0; x < d_InventoryUtil.CellCountX; x++)
            {
                for (int y = 0; y < d_InventoryUtil.CellCountY; y++)
                {
                    IntRef pCount = new IntRef();
                    PointRef[] p = d_InventoryUtil.ItemsAtArea(x, y, d_Items.ItemSizeX(item), d_Items.ItemSizeY(item), pCount);
                    if (p != null && pCount.value == 0)
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
    public GameData d_Data;

    public int ItemSizeX(Item item)
    {
        if (item.ItemClass == ItemClass.Block) { return 1; }
        throw new NotImplementedException();
    }

    public int ItemSizeY(Item item)
    {
        if (item.ItemClass == ItemClass.Block) { return 1; }
        throw new NotImplementedException();
    }

    public Item Stack(Item itemA, Item itemB)
    {
        if (itemA.ItemClass == ItemClass.Block
            && itemB.ItemClass == ItemClass.Block)
        {
            int railcountA = DirectionUtils.RailDirectionFlagsCount(d_Data.Rail()[itemA.BlockId]);
            int railcountB = DirectionUtils.RailDirectionFlagsCount(d_Data.Rail()[itemB.BlockId]);
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

    public bool CanWear(int selectedWear, Item item)
    {
        if (item == null) { return true; }
        if (item == null) { return true; }
        switch (selectedWear)
        {
            //case WearPlace.LeftHand: return false;
            case WearPlace_.RightHand: return item.ItemClass == ItemClass.Block;
            case WearPlace_.MainArmor: return false;
            case WearPlace_.Boots: return false;
            case WearPlace_.Helmet: return false;
            case WearPlace_.Gauntlet: return false;
            default: throw new Exception();
        }
    }

    public string ItemGraphics(Item item)
    {
        throw new NotImplementedException();
    }
}

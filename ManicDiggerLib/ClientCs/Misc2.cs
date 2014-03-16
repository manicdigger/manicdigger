using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace GameModeFortress
{
    public class CraftingTableTool
    {
        public IMapStorage d_Map;
        public GameData d_Data;
        public List<int> GetOnTable(List<Vector3i> table)
        {
            List<int> ontable = new List<int>();
            foreach (var v in table)
            {
                int t = d_Map.GetBlock(v.x, v.y, v.z + 1);
                ontable.Add(t);
            }
            return ontable;
        }
        public int maxcraftingtablesize = 2000;
        public List<Vector3i> GetTable(Vector3i pos)
        {
            List<Vector3i> l = new List<Vector3i>();
            Queue<Vector3i> todo = new Queue<Vector3i>();
            todo.Enqueue(pos);
            for (; ; )
            {
                if (todo.Count == 0 || l.Count >= maxcraftingtablesize)
                {
                    break;
                }
                var p = todo.Dequeue();
                if (l.Contains(p))
                {
                    continue;
                }
                l.Add(p);
                var a = new Vector3i(p.x + 1, p.y, p.z);
                if (d_Map.GetBlock(a.x, a.y, a.z) == d_Data.BlockIdCraftingTable())
                {
                    todo.Enqueue(a);
                }
                var b = new Vector3i(p.x - 1, p.y, p.z);
                if (d_Map.GetBlock(b.x, b.y, b.z) == d_Data.BlockIdCraftingTable())
                {
                    todo.Enqueue(b);
                }
                var c = new Vector3i(p.x, p.y + 1, p.z);
                if (d_Map.GetBlock(c.x, c.y, c.z) == d_Data.BlockIdCraftingTable())
                {
                    todo.Enqueue(c);
                }
                var d = new Vector3i(p.x, p.y - 1, p.z);
                if (d_Map.GetBlock(d.x, d.y, d.z) == d_Data.BlockIdCraftingTable())
                {
                    todo.Enqueue(d);
                }
            }
            return l;
        }
    }
}

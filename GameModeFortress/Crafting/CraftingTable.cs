using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using ManicDigger;

namespace GameModeFortress
{
    public class CraftingTableTool
    {
        [Inject]
        public IMapStorage map { get; set; }
        public List<int> GetOnTable(List<Vector3i> table)
        {
            List<int> ontable = new List<int>();
            foreach (var v in table)
            {
                int t = map.GetBlock(v.x, v.y, v.z + 1);
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
                if (map.GetBlock(a.x, a.y, a.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(a);
                }
                var b = new Vector3i(p.x - 1, p.y, p.z);
                if (map.GetBlock(b.x, b.y, b.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(b);
                }
                var c = new Vector3i(p.x, p.y + 1, p.z);
                if (map.GetBlock(c.x, c.y, c.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(c);
                }
                var d = new Vector3i(p.x, p.y - 1, p.z);
                if (map.GetBlock(d.x, d.y, d.z) == (int)TileTypeManicDigger.CraftingTable)
                {
                    todo.Enqueue(d);
                }
            }
            return l;
        }
    }
}

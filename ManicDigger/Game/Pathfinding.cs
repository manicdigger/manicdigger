using System;
using System.Collections.Generic;
using System.Text;
using Algorithms;
using System.Runtime.InteropServices;
using System.Collections;
using ManicDigger.FastAStar;
using OpenTK;
using System.Drawing;

namespace ManicDigger
{
    public interface IWalkable
    {
        int? BlockWalkCost(int x, int y, int z);
    }
    public interface IPathfinder3d
    {
        void NewMap();
        void UpdateBlock(int x, int y, int z);
        IEnumerable<Vector3> Pathfind(Vector3 start, Vector3 end);
    }
    public class Pathfinder3d : IPathfinder3d
    {
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public IWalkable walkable { get; set; }
        [Inject]
        public IGameData data { get; set; }
        AStarFastNode[] nodes = new AStarFastNode[100 * 1000];
        Dictionary<Vector3, int> node_id_at_position = new Dictionary<Vector3, int>();
        Dictionary<int, Vector3> position_at_node_id = new Dictionary<int, Vector3>();
        public void NewMap()
        {
            for (int x = 0; x < map.MapSizeX; x++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    for (int z = 0; z < map.MapSizeZ; z++)
                    {
                        UpdateBlock(x, y, z);
                    }
                }
            }
        }
        //update all connections outgoing from this block
        //does not update connections of blocks around going into this block!
        public void UpdateBlock(int x, int y, int z)
        {
            if (map.GetBlock(x, y, z) == data.TileIdEmpty)
            {
                return;
            }
            var v = new Vector3(x, y, z);
            MakeSureThereIsNode(v);
            int nodeid = node_id_at_position[v];
            //can: a) jump one tile higher, equal or down b) (todo) jump across hole (1 tile). c) (todo) swim
            foreach (Point p in PosAround4(new Point(x, y)))
            {
                for (int i = z + 1; i >= 0; i--)
                {
                    int xx = p.X;
                    int yy = p.Y;
                    int zz = i;
                    var vv = new Vector3(xx, yy, zz);
                    if (!MapUtil.IsValidPos(map, xx, yy, zz))
                    {
                        break;
                    }
                    if (map.GetBlock(xx, yy, zz) == data.TileIdEmpty)
                    {
                        continue;
                    }
                    int? cost = walkable.BlockWalkCost(xx, yy, zz);
                    if (cost != null)
                    {
                        MakeSureThereIsNode(vv);
                        var c = new AStarFastNodeConnection();
                        c.DestinationId = (uint)node_id_at_position[vv];
                        c.Length = (ushort)cost.Value;
                        nodes[nodeid].AddConnection(c);
                        break;
                    }
                }
            }
        }
        IEnumerable<Point> PosAround4(Point p)
        {
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X, p.Y - 1);
            yield return new Point(p.X, p.Y + 1);
        }
        int nodescount = 0;
        private Vector3 MakeSureThereIsNode(Vector3 v)
        {
            if (!node_id_at_position.ContainsKey(v))
            {
                nodes[nodescount] = new AStarFastNode();
                nodescount++;
                node_id_at_position[v] = nodescount - 1;
                position_at_node_id[nodescount - 1] = v;
            }
            return v;
        }
        public IEnumerable<Vector3> Pathfind(Vector3 start, Vector3 end)
        {
            this.goal = end;
            if (!node_id_at_position.ContainsKey(start)) { yield break; }
            if (!node_id_at_position.ContainsKey(end)) { yield break; }
            var l = astarfast.Search((uint)node_id_at_position[start], isgoal, score, cango);
            foreach (var v in l)
            {
                yield return position_at_node_id[(int)v.Position];
            }
        }
        Vector3 goal;
        bool isgoal(uint nodeid)
        {
            return node_id_at_position[goal] == nodeid;
        }
        int score(uint nodeid)
        {
            return 1;//todo
        }
        bool cango(uint nodeid)
        {
            return true;
        }
        AStarFast astarfast;
        public Pathfinder3d()
        {
            astarfast = new AStarFast(nodes);
        }
    }
}
namespace ManicDigger.FastAStar
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AStarFastNodeConnection
    {
        public uint DestinationId;
        public ushort Length;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AStarFastNode
    {
        public byte ConnectionCount;
        public AStarFastNodeConnection Connection0;
        public AStarFastNodeConnection Connection1;
        public AStarFastNodeConnection Connection2;
        public AStarFastNodeConnection Connection3;
        public IEnumerable<AStarFastNodeConnection> Connections
        {
            get
            {
                switch (ConnectionCount)
                {
                    case 0:
                        yield break;
                    case 1:
                        yield return Connection0;
                        yield break;
                    case 2:
                        yield return Connection0;
                        yield return Connection1;
                        yield break;
                    case 3:
                        yield return Connection0;
                        yield return Connection1;
                        yield return Connection2;
                        yield break;
                    case 4:
                        yield return Connection0;
                        yield return Connection1;
                        yield return Connection2;
                        yield return Connection3;
                        yield break;
                }
                //if (ConnectionCount > 0) { yield return Connection0; }
                //if (ConnectionCount > 1) { yield return Connection1; }
                //if (ConnectionCount > 2) { yield return Connection2; }
                //if (ConnectionCount > 3) { yield return Connection3; }
            }
        }
        public void AddConnection(AStarFastNodeConnection connection)
        {
            switch (ConnectionCount)
            {
                case 0:
                    Connection0 = connection;
                    break;
                case 1:
                    Connection1 = connection;
                    break;
                case 2:
                    Connection2 = connection;
                    break;
                case 3:
                    Connection3 = connection;
                    break;
            }
            ConnectionCount++;
        }
        public void RemoveAllConnections()
        {
            ConnectionCount = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CalculationGridNode
    {
        public int F;
        public int G;
        public uint Parent;
        public byte Status;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PathFinderNode
    {
        #region Variables Declaration
        public int F;
        public int G;
        public int H;  // f = gone + heuristic
        public uint Position;
        public uint Parent;
        #endregion
    }
    /// <summary>
    /// based on fast astar by CastorTiu
    /// from CodeProject
    /// http://www.codeproject.com/KB/recipes/PathFinder.aspx
    /// </summary>
    public class AStarFast
    {
        class NodeCostComparer : IComparer<uint>
        {
            public NodeCostComparer(CalculationGridNode[] calculation_grid)
            {
                this.calculation_grid = calculation_grid;
            }
            CalculationGridNode[] calculation_grid;
            #region IComparer<AStarNode> Members
            public int Compare(uint a, uint b)
            {
                if (calculation_grid[a].F > calculation_grid[b].F)
                    return 1;
                else if (calculation_grid[a].F < calculation_grid[b].F)
                    return -1;
                return 0;
            }
            #endregion
        }
        BenTools.Data.BinaryPriorityQueue<uint> open;
        List<PathFinderNode> closed = new List<PathFinderNode>();
        byte open_node_value = 1;
        byte closed_node_value = 2;
        int closed_node_counter = 0;
        int h = 0;
        uint location = 0;
        uint new_location = 0;
        int h_estimate = 2;
        bool reopen_closed_nodes = false;//true
        int new_g = 0;
        public double completed_time = 0;
        int search_limit = 2000;
        public AStarFast(AStarFastNode[] nodes)
        {
            this.nodes = nodes;
            this.calculation_grid = new CalculationGridNode[nodes.Length];
            NodeCostComparer comparer = new NodeCostComparer(calculation_grid);
            open = new BenTools.Data.BinaryPriorityQueue<uint>(comparer, 1);
        }
        AStarFastNode[] nodes;
        CalculationGridNode[] calculation_grid;
        public delegate RET Func<A, RET>(A a);
        public List<PathFinderNode> Search(uint start, Func<uint, bool> isGoal, Func<uint, int> score, Func<uint, bool> canGo)
        {
            HighResolutionTime.Start();

            bool found = false;
            bool stop = false;
            bool stopped = false;
            closed_node_counter = 0;
            open_node_value += 2;
            closed_node_value += 2;
            open.Clear();
            closed.Clear();

            location = start;
            calculation_grid[location].G = 0;
            calculation_grid[location].F = h_estimate;
            calculation_grid[location].Parent = start;
            calculation_grid[location].Status = open_node_value;

            uint end = 0;//moje

            open.Push(location);
            while (open.Count > 0)//while q is not empty
            {
                location = open.Pop();
                if (calculation_grid[location].Status == closed_node_value)
                {
                    continue;
                }
                if (isGoal(location))
                {
                    calculation_grid[location].Status = closed_node_value;
                    found = true;
                    end = location;
                    break;
                }
                if (closed_node_counter > search_limit)
                {
                    stopped = true;
                    completed_time = HighResolutionTime.GetTime();
                    return null;
                }
                //Lets calculate each successors
                //for (int i = 0; i < (mDiagonals ? 8 : 4); i++)
                foreach (AStarFastNodeConnection c in nodes[location].Connections)
                {
                    new_location = c.DestinationId;

                    if (new_location < 0 || new_location > nodes.Length - 1)
                    {
                        throw new Exception("invalid node connection");
                    }

                    if (calculation_grid[new_location].Status == closed_node_value && !reopen_closed_nodes)
                        continue;

                    // Unbreakeable?
                    //if (mGrid[mNewLocationX, mNewLocationY] == 0)
                    //    continue;
                    if (!canGo(new_location))
                    {
                        continue;
                    }

                    //if (mHeavyDiagonals && i > 3)
                    //    mNewG = mCalcGrid[mLocation].G + (int)(mGrid[mNewLocationX, mNewLocationY] * 2.41);
                    //else
                    //    mNewG = mCalcGrid[mLocation].G + mGrid[mNewLocationX, mNewLocationY];
                    new_g = calculation_grid[location].G + c.Length;

                    /*
                    if (mPunishChangeDirection)
                    {
                        if ((mNewLocationX - mLocationX) != 0)
                        {
                            if (mHoriz == 0)
                                mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                        }
                        if ((mNewLocationY - mLocationY) != 0)
                        {
                            if (mHoriz != 0)
                                mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                        }
                    }
                    */

                    //Is it open or closed?
                    if (calculation_grid[new_location].Status == open_node_value
                        || calculation_grid[new_location].Status == closed_node_value)
                    {
                        // The current node has less code than the previous? then skip this node
                        if (calculation_grid[new_location].G <= new_g)
                            continue;
                    }

                    calculation_grid[new_location].Parent = location;
                    calculation_grid[new_location].G = new_g;

                    h = h_estimate * score(new_location);
                    /*
                    switch (mFormula)
                    {
                        default:
                        case HeuristicFormula.Manhattan:
                            mH = mHEstimate * (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                            break;
                        case HeuristicFormula.MaxDXDY:
                            mH = mHEstimate * (Math.Max(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y)));
                            break;
                        case HeuristicFormula.DiagonalShortCut:
                            int h_diagonal = Math.Min(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
                            int h_straight = (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                            mH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                            break;
                        case HeuristicFormula.Euclidean:
                            mH = (int)(mHEstimate * Math.Sqrt(Math.Pow((mNewLocationY - end.X), 2) + Math.Pow((mNewLocationY - end.Y), 2)));
                            break;
                        case HeuristicFormula.EuclideanNoSQR:
                            mH = (int)(mHEstimate * (Math.Pow((mNewLocationX - end.X), 2) + Math.Pow((mNewLocationY - end.Y), 2)));
                            break;
                        case HeuristicFormula.Custom1:
                            Point dxy = new Point(Math.Abs(end.X - mNewLocationX), Math.Abs(end.Y - mNewLocationY));
                            int Orthogonal = Math.Abs(dxy.X - dxy.Y);
                            int Diagonal = Math.Abs(((dxy.X + dxy.Y) - Orthogonal) / 2);
                            mH = mHEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Y);
                            break;
                    }
                    */
                    /*
                    if (mTieBreaker)
                    {
                        int dx1 = mLocationX - end.X;
                        int dy1 = mLocationY - end.Y;
                        int dx2 = start.X - end.X;
                        int dy2 = start.Y - end.Y;
                        int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                        mH = (int)(mH + cross * 0.001);
                    }
                    */
                    calculation_grid[new_location].F = new_g + h;

#if DEBUGON
                        if (mDebugProgress && PathFinderDebug != null)
                            PathFinderDebug(mLocationX, mLocationY, mNewLocationX, mNewLocationY, PathFinderNodeType.Open, mCalcGrid[mNewLocation].F, mCalcGrid[mNewLocation].G);
#endif

                    //It is faster if we leave the open node in the priority queue
                    //When it is removed, it will be already closed, it will be ignored automatically
                    //if (tmpGrid[newLocation].Status == 1)
                    //{
                    //    //int removeX   = newLocation & gridXMinus1;
                    //    //int removeY   = newLocation >> gridYLog2;
                    //    mOpen.RemoveLocation(newLocation);
                    //}

                    //if (tmpGrid[newLocation].Status != 1)
                    //{
                    open.Push(new_location);
                    //}
                    calculation_grid[new_location].Status = open_node_value;
                }
                closed_node_counter++;
                calculation_grid[location].Status = closed_node_value;
            }
            completed_time = HighResolutionTime.GetTime();
            if (found)
            {
                closed.Clear();
                uint pos = end;

                CalculationGridNode node_temp = calculation_grid[end];
                PathFinderNode node;
                node.F = node_temp.F;
                node.G = node_temp.G;
                node.H = 0;
                node.Parent = node_temp.Parent;
                node.Position = end;

                while (node.Position != node.Parent)
                {
                    closed.Add(node);
#if DEBUGON
                        if (mDebugFoundPath && PathFinderDebug != null)
                            PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
#endif
                    pos = node.Parent;
                    node_temp = calculation_grid[pos];
                    node.F = node_temp.F;
                    node.G = node_temp.G;
                    node.H = 0;
                    node.Parent = node_temp.Parent;
                    node.Position = pos;
                }

                closed.Add(node);
#if DEBUGON
                    if (mDebugFoundPath && PathFinderDebug != null)
                        PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
#endif

                stopped = true;
                return closed;
            }
            stopped = true;
            return null;
        }
    }
}
namespace BenTools.Data
{
    public interface IPriorityQueue<T>
    {
        int Push(T O);
        T Pop();
        T Peek();
        void Update(int i);
    }
    public class BinaryPriorityQueue<T> : IPriorityQueue<T>
    {
        protected List<T> InnerList = new List<T>();
        protected IComparer<T> Comparer;

        #region contructors
        /*
		public BinaryPriorityQueue() : this(System.Collections.Comparer.Default)
		{}
		public BinaryPriorityQueue(IComparer<T> c)
		{
			Comparer = c;
		}
		public BinaryPriorityQueue(int C) : this(System.Collections.Generic.Comparer<object>.Default,C)
		{}
        */
        public BinaryPriorityQueue(IComparer<T> c, int Capacity)
        {
            Comparer = c;
            InnerList.Capacity = Capacity;
        }

        protected BinaryPriorityQueue(List<T> Core, IComparer<T> Comp, bool Copy)
        {
            if (Copy)
                InnerList = new List<T>(Core);//Core.Clone() as ArrayList;
            else
                InnerList = Core;
            Comparer = Comp;
        }

        #endregion
        protected void SwitchElements(int i, int j)
        {
            T h = InnerList[i];
            InnerList[i] = InnerList[j];
            InnerList[j] = h;
        }

        protected virtual int OnCompare(int i, int j)
        {
            return Comparer.Compare(InnerList[i], InnerList[j]);
        }

        #region public methods
        /// <summary>
        /// Push an object onto the PQ
        /// </summary>
        /// <param name="O">The new object</param>
        /// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
        public int Push(T O)
        {
            int p = InnerList.Count, p2;
            InnerList.Add(O); // E[p] = O
            do
            {
                if (p == 0)
                    break;
                p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                    break;
            } while (true);
            return p;
        }

        /// <summary>
        /// Get the smallest object and remove it.
        /// </summary>
        /// <returns>The smallest object</returns>
        public T Pop()
        {
            T result = InnerList[0];
            int p = 0, p1, p2, pn;
            InnerList[0] = InnerList[InnerList.Count - 1];
            InnerList.RemoveAt(InnerList.Count - 1);
            do
            {
                pn = p;
                p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                    p = p1;
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                    p = p2;

                if (p == pn)
                    break;
                SwitchElements(p, pn);
            } while (true);
            return result;
        }

        /// <summary>
        /// Notify the PQ that the object at position i has changed
        /// and the PQ needs to restore order.
        /// Since you dont have access to any indexes (except by using the
        /// explicit IList.this) you should not call this function without knowing exactly
        /// what you do.
        /// </summary>
        /// <param name="i">The index of the changed object.</param>
        public void Update(int i)
        {
            int p = i, pn;
            int p1, p2;
            do	// aufsteigen
            {
                if (p == 0)
                    break;
                p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                    break;
            } while (true);
            if (p < i)
                return;
            do	   // absteigen
            {
                pn = p;
                p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                    p = p1;
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                    p = p2;

                if (p == pn)
                    break;
                SwitchElements(p, pn);
            } while (true);
        }

        /// <summary>
        /// Get the smallest object without removing it.
        /// </summary>
        /// <returns>The smallest object</returns>
        public T Peek()
        {
            if (InnerList.Count > 0)
                return InnerList[0];
            return default(T);
        }

        public bool Contains(T value)
        {
            return InnerList.Contains(value);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public int Count
        {
            get
            {
                return InnerList.Count;
            }
        }
        IEnumerator GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        /*
        public void CopyTo(Array array, int index)
        {
            InnerList.CopyTo(array,index);
        }

        public object Clone()
        {
            return new BinaryPriorityQueue(InnerList,Comparer,true);	
        }

        public bool IsSynchronized
        {
            get
            {
                return InnerList.IsSynchronized;
            }
        }
        */
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
        #endregion
        #region explicit implementation
        bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        T this[int index]
        {
            get
            {
                return InnerList[index];
            }
            set
            {
                InnerList[index] = value;
                Update(index);
            }
        }

        int Add(T o)
        {
            return Push(o);
        }

        void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void Remove(object value)
        {
            throw new NotSupportedException();
        }

        int IndexOf(object value)
        {
            throw new NotSupportedException();
        }

        bool IsFixedSize
        {
            get
            {
                return false;
            }
        }
        /*
        public static BinaryPriorityQueue<T> Syncronized(BinaryPriorityQueue<T> P)
        {
            return new BinaryPriorityQueue<T>(List<T>.Synchronized(P.InnerList),P.Comparer,false);
        }
        public static BinaryPriorityQueue<T> ReadOnly(BinaryPriorityQueue<T> P)
        {
            return new BinaryPriorityQueue<T>(List<T>.ReadOnly(P.InnerList),P.Comparer,false);
        }
        */
        #endregion
    }
}
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
namespace Algorithms
{
    public static class HighResolutionTime
    {
        #region Win32APIs
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long perfcount);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long freq);
        #endregion

        #region Variables Declaration
        private static long mStartCounter;
        private static long mFrequency;
        #endregion

        #region Constuctors
        static HighResolutionTime()
        {
            QueryPerformanceFrequency(out mFrequency);
        }
        #endregion

        #region Methods
        public static void Start()
        {
            QueryPerformanceCounter(out mStartCounter);
        }

        public static double GetTime()
        {
            long endCounter;
            QueryPerformanceCounter(out endCounter);
            long elapsed = endCounter - mStartCounter;
            return (double)elapsed / mFrequency;
        }
        #endregion
    }
}
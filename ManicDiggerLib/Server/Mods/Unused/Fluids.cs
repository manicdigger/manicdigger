using System;
using System.Collections.Generic;
using System.Text;

/* ********************************************************
 * fluid.cs
 * Version 0.1
 * Date 2013 Jan 13
 * Author: Wilfried Elmenreich
 *
 * mod for Manic Digger
 * This mod makes fluid blocks to move downwards/sideways
 * in a way to level a given pool. Install by copying cs
 * file into Manic Digger\Mods\Fortress
 * ******************************************************** */

namespace ManicDigger.Mods
{
    public class Fluids : IMod
    {
        Random random = new Random();

        struct Vector3i
        {
            public Vector3i(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.searched = false;
            }

            public int x;
            public int y;
            public int z;
            public bool searched;
        }

        Dictionary<int, Vector3i> activeFluids = new Dictionary<int, Vector3i>();
        int Water, Lava;
        public int maxentries = 1000000;
        //private bool warning_issued=false;
        //int[] dx = {-1,1,0,0};
        //int[] dy = {0,0,-1,1};
        int[] dx = {-1,0,1,1, 1, 0,-1,-1};
        int[] dy = { 1,1,1,0,-1,-1,-1, 0};

        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }

        public void Start(ModManager m)
        {
            this.m = m;
            m.RegisterOnBlockUpdate(CheckNeighbors);
            m.RegisterOnBlockBuild(Build);
            m.RegisterTimer(UpdateFluids, 1);
            m.RegisterOnBlockDelete(Delete);
            Water = m.GetBlockId("Water");
            Lava = m.GetBlockId("Lava");
        }

        ModManager m;

        int positionHash(int x, int y, int z)
        {
            return (x * 9973 + y) * 127 + z; //this hash value may overflow, but we don't care
        }

        void Build(int player, int x, int y, int z)
        {
            int b = m.GetBlock(x, y, z);
            if (m.IsBlockFluid(b))
                addActiveFluid(x, y, z);
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            CheckNeighbors(x, y, z);
        }

        void CheckNeighbors(int x, int y, int z)
        {
            for (int xx=x-1; xx<=x+1; xx++)
                for (int yy=y-1; yy<=y+1; yy++)
                    for (int zz=z-1; zz<=z+1; zz++)
                    {
                        Check(xx, yy, zz);
                    }
        }

        void Check(int x, int y, int z)
        {
            if (!m.IsValidPos(x, y, z))
                return;
            int b = m.GetBlock(x, y, z);

            if (m.GetBlockNameAt(x, y, z) == "Cake")
            {
                m.SetBlock(x, y, z, 8); //Water
                b = Water;
            }

            //check if it is a fluid
            if (!m.IsBlockFluid(b))
                return;

            if (z > 0)
            {
                //can it fall down?
                if (m.GetBlock(x, y, z - 1) == 0)
                {
                    addActiveFluid(x, y, z);
                    return;
                }
                //check neighbor cells for a place to drop down

                for (int dd=0; dd< dx.Length; dd++)
                {
                    int xx = x + dx [dd];
                    int yy = y + dy [dd];
                    if (!m.IsValidPos(xx, yy, z))
                        continue;
                    if ((m.GetBlock(xx, yy, z) == 0) && (m.GetBlock(xx, yy, z - 1) == 0))
                    {
                        addActiveFluid(x, y, z);
                        return;
                    }
                }
                //if it is not on top of a water block it will prefer to go to a water block (cohesion)
                if (m.GetBlock(x, y, z - 1) != b)
                    for (int dd=1; dd< dx.Length; dd+=2) //check von Neumann neighbors
                    {
                        int xx = x + dx [dd];
                        int yy = y + dy [dd];
                        if (!m.IsValidPos(xx, yy, z - 1))
                            continue;
                        if (m.GetBlock(xx, yy, z) != 0)
                            continue;
                        int otherblock = m.GetBlock(xx, yy, z - 1);
                        if (otherblock == b)
                        {
                            addActiveFluid(x, y, z);
                            return;
                        }
                    }
                //is it a new hole near a fluid?
                for (int dd=1; dd< dx.Length; dd+=2) //check von Neumann neighbors
                {
                    int xx = x + dx [dd];
                    int yy = y + dy [dd];
                    if (!m.IsValidPos(xx, yy, z - 1))
                        continue;
                    if (m.GetBlock(xx, yy, z) != 0)
                        continue;
                    addActiveFluid(x, y, z);
                    return;
                }
            }
        }

        bool Update(int x, int y, int z)
        {
            int b = m.GetBlock(x, y, z);

            //check if it is a fluid
            if ((b != Water) && (b != Lava))
                return false;

            if (z > 0)
            {
                if (m.GetBlock(x, y, z - 1) == 0)
                {
                    //free fall
                    int targetz = z - 1;
                    if ((b == Water) && (m.GetBlock(x, y, z - 2) == 0))
                        targetz = z - 2;
                    m.SetBlock(x, y, targetz, b);
                    m.SetBlock(x, y, z, 0);
                    removeActiveFluid(x, y, z);
                    addActiveFluid(x, y, targetz);
                    //check environment
                    CheckNeighbors(x, y, z);
                    return true;
                }
                //check neighbor cells for a place to drop down

                int r = random.Next(8);

                for (int d=r; d < r+dx.Length; d++)
                {
                    int dd = d % dx.Length;
                    int xx = x + dx [dd];
                    int yy = y + dy [dd];
                    if (!m.IsValidPos(xx, yy, z))
                        continue;
                    if ((m.GetBlock(xx, yy, z) == 0) && (m.GetBlock(xx, yy, z - 1) == 0))
                    {
                        //Water falling over edge
                        m.SetBlock(xx, yy, z - 1, m.GetBlock(x, y, z));
                        m.SetBlock(x, y, z, 0);
                        removeActiveFluid(x, y, z);
                        addActiveFluid(xx, yy, z - 1);
                        //check environment
                        CheckNeighbors(x, y, z);
                        return true;
                    }
                }

                //if it is not on top of a water block it will prefer to go to a water block (cohesion)
                if (m.GetBlock(x, y, z - 1) != b)
                {
                    r = random.Next(4);
                    for (int d=r; d<r+4; d+=1)
                    {
                        int dd = (1 + 2 * d) % dx.Length;  //check only von Neumann neighbors
                        int xx = x + dx [dd];
                        int yy = y + dy [dd];
                        if (!m.IsValidPos(xx, yy, z - 1))
                            continue;
                        if (m.GetBlock(xx, yy, z) != 0)
                            continue;
                        int otherblock = m.GetBlock(xx, yy, z - 1);
                        if (otherblock == b)
                        {
                            m.SetBlock(xx, yy, z, m.GetBlock(x, y, z));
                            m.SetBlock(x, y, z, 0);
                            removeActiveFluid(x, y, z);
                            addActiveFluid(xx, yy, z - 1);
                            CheckNeighbors(x, y, z);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateFluids()
        {
            var keys = new List<int>(activeFluids.Keys);
            foreach (int key in keys)
            {
                Vector3i p = activeFluids [key];
                if (Update(p.x, p.y, p.z) == false)
                {
                    Vector3i b1, b2;
                    //this fluid does not move anymore
                    //search for lowest freespace
                    if (!lowestFreeSpace(p.x, p.y, p.z, p.z))
                    {
                        //nothing left to do here
                        activeFluids.Remove(key);
                        continue;
                    }
                    b1 = foundBlock;
                    if (!highestFluidBlock(p.x, p.y, p.z, b1.z + 1))
                    {
                        //nothing left to do here
                        activeFluids.Remove(key);
                        continue;
                    }
                    b2 = foundBlock;

                    m.SetBlock(b1.x, b1.y, b1.z, searchMedium);
                    m.SetBlock(b2.x, b2.y, b2.z, 0);
                    CheckNeighbors(b2.x, b2.y, b2.z);
                    addActiveFluid(b1.x, b1.y, b1.z);
                }
            }
        }

        void addActiveFluid(int x, int y, int z)
        {
            int hash = positionHash(x, y, z);
            if (activeFluids.ContainsKey(hash))
                return;
            activeFluids.Add(hash, new Vector3i(x, y, z));
        }

        void removeActiveFluid(int x, int y, int z)
        {
            int hash = positionHash(x, y, z);
            if (activeFluids.ContainsKey(hash))
                activeFluids.Remove(hash);
        }

        Vector3i foundBlock;
        int searchZ;
        int searchMedium, searchTarget, preferedSearchDirection;
        bool found;
        Dictionary<int, Vector3i> visitedBlocks;
        bool endSearchonFound;

        bool lowestFreeSpace(int x, int y, int z, int zrequired)
        {
            if (!m.IsValidPos(x, y, z))
                return false;
            preferedSearchDirection = -1; //down
            int maxRecursionDepth = 10; //default
            searchZ = zrequired;
            found = false;
            endSearchonFound = false;
            searchMedium = m.GetBlock(x, y, z);
            searchTarget = 0;
            if (!m.IsBlockFluid(searchMedium))
                return false;
            if ((searchMedium) == Water)
                maxRecursionDepth = 25;
            visitedBlocks = new Dictionary<int, Vector3i>();
            recursiveSearch(maxRecursionDepth, x, y, z);
            return found;
        }

        bool highestFluidBlock(int x, int y, int z, int zrequired)
        {
            if (!m.IsValidPos(x, y, z))
                return false;
            preferedSearchDirection = 1; //up
            int maxRecursionDepth = 10; //default
            searchZ = zrequired;
            found = false;
            endSearchonFound = true;
            searchMedium = m.GetBlock(x, y, z);
            searchTarget = searchMedium;
            if (!m.IsBlockFluid(searchMedium))
                return false;
            if ((searchMedium) == Water)
                maxRecursionDepth = 25;
            visitedBlocks = new Dictionary<int, Vector3i>();
            recursiveSearch(maxRecursionDepth, x, y, z);
            return found;
        }

        void recursiveSearch(int depth, int x, int y, int z)
        {
            if ((depth == 0) || (found && endSearchonFound))
                return;
            if (!m.IsValidPos(x, y, z))
                return;
            //check if we found the target
            if (m.GetBlock(x, y, z) == searchTarget)
            {
                if (((z - searchZ) * preferedSearchDirection) >= 0)
                {
                    int zz = z;
                    if (searchMedium == searchTarget)
                    {
                        zz = z + preferedSearchDirection;
                        while ((zz>=0) && (zz<m.GetMapSizeZ()) && (m.GetBlock(x,y,zz)==searchMedium))
                            zz = zz + preferedSearchDirection;
                        zz = zz - preferedSearchDirection;
                    }
                    if ((!found) || (((zz - foundBlock.z) * preferedSearchDirection) > 0))
                    {
                        foundBlock = new Vector3i(x, y, zz);
                        found = true;
                        if (endSearchonFound)
                            return;
                    }
                }
            }
            //check if it is the search medium
            if (m.GetBlock(x, y, z) != searchMedium)
                return;
            //check if already visited
            int hash = positionHash(x, y, z);
            if (visitedBlocks.ContainsKey(hash))
                return;
            //mark visited
            visitedBlocks.Add(hash, new Vector3i(x, y, z));
            //search recursive (preferred z,horizontal,less other z direction)
            depth--;
            recursiveSearch(depth, x, y, z + preferedSearchDirection);
            int r = random.Next(4);
            for (int d=r; d<r+4; d+=1)
            {
                int dd = (1 + 2 * d) % dx.Length;  //check only von Neumann neighbors
                int xx = x + dx [dd];
                int yy = y + dy [dd];
                recursiveSearch(depth, xx, yy, z);
            }
            recursiveSearch(depth, x, y, z - preferedSearchDirection);
        }
    }
}

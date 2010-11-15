using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace fCraft
{
    //from fCraft (MIT License): http://fcraft.svn.sourceforge.net/viewvc/fcraft/trunk/fCraft/World/Forester.cs?view=log
    //original: http://www.peripheralarbor.com/minecraft/minecraftscripts.html
    class Forester
    {
        public static int TILETYPE_AIR = 0;
        public static int TILETYPE_STONE = 1;
        public static int TILETYPE_GRASS = 2;
        public static int TILETYPE_WATER = 9;
        public static int TILETYPE_LOG = 17;
        public static int TILETYPE_LEAVES = 18;

        public class ForesterArgs
        {
            public Operation OPERATION = Operation.Replant;
            public int TREECOUNT = 15; // 0 = no limit if op=conserve/replant
            public TreeShape SHAPE = TreeShape.Procedural;
            public int HEIGHT = 25;
            public int HEIGHTVARIATION = 15;
            public bool WOOD = true;
            public float TRUNKTHICKNESS = 1;
            public float TRUNKHEIGHT = .7f;
            public float BRANCHDENSITY = 1;
            public RootMode ROOTS = RootMode.Normal;
            public bool ROOTBUTTRESSES = true;
            public bool FOLIAGE = true;
            public float FOLIAGEDENSITY = 1;
            public bool MAPHEIGHTLIMIT = true;
            public int PLANTON = TILETYPE_GRASS;
            public Random rand = new Random();
            public IMapStorage inMap;
            public IMapStorage outMap;

            public void Validate()
            {
                if (TREECOUNT < 0) TREECOUNT = 0;
                if (HEIGHT < 1) HEIGHT = 1;
                if (HEIGHTVARIATION > HEIGHT) HEIGHTVARIATION = HEIGHT;
                if (TRUNKTHICKNESS < 0) TRUNKTHICKNESS = 0;
                if (TRUNKHEIGHT < 0) TRUNKHEIGHT = 0;
                if (FOLIAGEDENSITY < 0) FOLIAGEDENSITY = 0;
                if (BRANCHDENSITY < 0) BRANCHDENSITY = 0;
            }
        }


        class Tree
        {
            public Vector3i pos;
            public int height = 1;
            public ForesterArgs args;

            public Tree() { }

            public virtual void Prepare() { }

            public virtual void MakeTrunk() { }

            public virtual void MakeFoliage() { }

            public void Copy(Tree other)
            {
                args = other.args;
                pos = other.pos;
                height = other.height;
            }
        }

        class StickTree : Tree
        {
            public override void MakeTrunk()
            {
                for (int i = 0; i < height; i++)
                {
                    args.outMap.SetBlock(pos.x, pos.z, pos.y + i, TILETYPE_LOG);
                }
            }
        }

        class NormalTree : StickTree
        {
            [Inject]
            public IGameData data;
            public override void MakeFoliage()
            {
                int topy = pos[1] + height - 1;
                int start = topy - 2;
                int end = topy + 2;

                for (int y = start; y < end; y++)
                {
                    int rad;
                    if (y > start + 1)
                    {
                        rad = 1;
                    }
                    else
                    {
                        rad = 2;
                    }
                    for (int xoff = -rad; xoff < rad + 1; xoff++)
                    {
                        for (int zoff = -rad; zoff < rad + 1; zoff++)
                        {
                            if (args.rand.NextDouble() > .618 &&
                                Math.Abs(xoff) == Math.Abs(zoff) &&
                                Math.Abs(xoff) == rad)
                            {
                                continue;
                            }
                            args.outMap.SetBlock(pos[0] + xoff, pos[2] + zoff, y, TILETYPE_LEAVES);
                        }
                    }
                }
            }
        }

        class BambooTree : StickTree
        {
            public override void MakeFoliage()
            {
                int start = pos[1];
                int end = start + height + 1;
                for (int y = start; y < end; y++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int xoff = args.rand.Next(0, 2) * 2 - 1;
                        int zoff = args.rand.Next(0, 2) * 2 - 1;
                        args.outMap.SetBlock(pos[0] + xoff, pos[2] + zoff, y, TILETYPE_LEAVES);
                    }
                }
            }
        }

        class PalmTree : StickTree
        {
            public override void MakeFoliage()
            {
                int y = pos[1] + height;
                for (int xoff = -2; xoff < 3; xoff++)
                {
                    for (int zoff = -2; zoff < 3; zoff++)
                    {
                        if (Math.Abs(xoff) == Math.Abs(zoff))
                        {
                            args.outMap.SetBlock(pos[0] + xoff, pos[2] + zoff, y, TILETYPE_LEAVES);
                        }
                    }
                }
            }
        }

        class ProceduralTree : Tree
        {

            public float trunkRadius { get; set; }
            public float branchSlope { get; set; }
            public float trunkHeight { get; set; }
            public float branchDensity { get; set; }
            public float[] foliageShape { get; set; }
            public Vector3i[] foliageCoords { get; set; }


            void CrossSection(Vector3i center, float radius, int diraxis, int matidx)
            {
                int rad = (int)(radius + .618);
                int secidx1 = (diraxis - 1) % 3;
                int secidx2 = (diraxis + 1) % 3;

                Vector3i coord = new Vector3i();

                for (int off1 = -rad; off1 <= rad; off1++)
                {
                    for (int off2 = -rad; off2 <= rad; off2++)
                    {
                        float thisdist = (float)Math.Sqrt(Sqr(Math.Abs(off1) + .5) +
                                                           Sqr(Math.Abs(off2) + .5));
                        if (thisdist > radius) continue;
                        int pri = center[diraxis];
                        int sec1 = center[secidx1] + off1;
                        int sec2 = center[secidx2] + off2;
                        coord[diraxis] = pri;
                        coord[secidx1] = sec1;
                        coord[secidx2] = sec2;
                        args.outMap.SetBlock(coord.x, coord.z, coord.y, matidx);
                    }
                }
            }

            public virtual float ShapeFunc(int y)
            {
                if (args.rand.NextDouble() < 100f / (float)Sqr(height) && y < trunkHeight)
                {
                    return height * .12f;
                }
                else
                {
                    return -1;
                }
            }

            void FoliageCluster(Vector3i center)
            {
                int y = center[1];
                foreach (float i in foliageShape)
                {
                    CrossSection(new Vector3i(center[0], center[2], y), i, 1, TILETYPE_LEAVES);
                    y++;
                }
            }

            bool TaperedLimb(Vector3i start, Vector3i end, float startSize, float endSize)
            {
                Vector3i delta = end - start;
                int primidx = delta.GetLargestComponent();
                int maxdist = delta[primidx];
                if (maxdist == 0) return false;
                int primsign = (maxdist > 0 ? 1 : -1);

                int secidx1 = (primidx - 1) % 3;
                int secidx2 = (primidx + 1) % 3;

                int secdelta1 = delta[secidx1];
                float secfac1 = secdelta1 / (float)delta[primidx];
                int secdelta2 = delta[secidx2];
                float secfac2 = secdelta2 / (float)delta[primidx];

                Vector3i coord = new Vector3i();
                int endoffset = delta[primidx] + primsign;

                for (int primoffset = 0; primoffset < endoffset; primoffset += primsign)
                {
                    int primloc = start[primidx] + primoffset;
                    int secloc1 = (int)(start[secidx1] + primoffset * secfac1);
                    int secloc2 = (int)(start[secidx2] + primoffset * secfac2);
                    coord[primidx] = primloc;
                    coord[secidx1] = secloc1;
                    coord[secidx2] = secloc2;
                    float primdist = (float)Math.Abs(delta[primidx]);
                    float radius = endSize + (startSize - endSize) * Math.Abs(delta[primidx] - primoffset) / (float)primdist;

                    CrossSection(coord, radius, primidx, TILETYPE_LOG);
                }
                return true;
            }

            public override void MakeFoliage()
            {
                foreach (Vector3i coord in foliageCoords)
                {
                    FoliageCluster(coord);
                }
                foreach (Vector3i coord in foliageCoords)
                {
                    args.outMap.SetBlock(coord.x, coord.z, coord.y, TILETYPE_LOG);
                }
            }

            void MakeBranches()
            {
                int topy = pos[1] + (int)(trunkHeight + .5);
                float endrad = trunkRadius * (1 - trunkHeight / (float)height);
                if (endrad < 1) endrad = 1;

                foreach (Vector3i coord in foliageCoords)
                {
                    float dist = (float)Math.Sqrt(Sqr(coord.x - pos.x) + Sqr(coord.z - pos.z));
                    float ydist = coord[1] - pos[1];
                    float value = (branchDensity * 220 * height) / Cub(ydist + dist);

                    if (value < args.rand.NextDouble()) continue;

                    int posy = coord[1];
                    float slope = (float)(branchSlope + (.5 - args.rand.NextDouble()) * .16);

                    float branchy, basesize;
                    if (coord[1] - dist * slope > topy)
                    {
                        float threshold = 1 / (float)height;
                        if (args.rand.NextDouble() < threshold) continue;
                        branchy = topy;
                        basesize = endrad;
                    }
                    else
                    {
                        branchy = posy - dist * slope;
                        basesize = endrad + (trunkRadius - endrad) *
                                   (topy - branchy) / trunkHeight;
                    }

                    float startsize = (float)(basesize * (1 + args.rand.NextDouble()) *
                                              .618 * Math.Pow(dist / (float)height, .618));
                    float rndr = (float)(Math.Sqrt(args.rand.NextDouble()) * basesize * .618);
                    float rndang = (float)(args.rand.NextDouble() * 2 * Math.PI);
                    int rndx = (int)(rndr * Math.Sin(rndang) + .5);
                    int rndz = (int)(rndr * Math.Cos(rndang) + .5);
                    Vector3i startcoord = new Vector3i
                    {
                        x = pos[0] + rndx,
                        z = pos[2] + rndz,
                        y = (int)branchy
                    };
                    if (startsize < 1) startsize = 1;
                    float endsize = 1;
                    TaperedLimb(startcoord, coord, startsize, endsize);
                }
            }

            struct RootBase
            {
                public int x, z;
                public float radius;
            }

            void MakeRoots(RootBase[] rootbases)
            {
                foreach (Vector3i coord in foliageCoords)
                {
                    float dist = (float)Math.Sqrt(Sqr(coord[0] - pos[0]) + Sqr(coord[2] - pos[2]));
                    float ydist = coord[1] - pos[1];
                    float value = (branchDensity * 220 * height) / Cub(ydist + dist);
                    if (value < args.rand.NextDouble()) continue;

                    RootBase rootbase = rootbases[args.rand.Next(0, rootbases.Length)];
                    int rootx = rootbase.x;
                    int rootz = rootbase.z;
                    float rootbaseradius = rootbase.radius;

                    float rndr = (float)(Math.Sqrt(args.rand.NextDouble()) * rootbaseradius * .618);
                    float rndang = (float)(args.rand.NextDouble() * 2 * Math.PI);
                    int rndx = (int)(rndr * Math.Sin(rndang) + .5);
                    int rndz = (int)(rndr * Math.Cos(rndang) + .5);
                    int rndy = (int)(args.rand.NextDouble() * rootbaseradius * .5);
                    Vector3i startcoord = new Vector3i
                    {
                        x = rootx + rndx,
                        z = rootz + rndz,
                        y = pos[1] + rndy
                    };
                    Vector3f offset = new Vector3f(startcoord - coord);

                    if (args.SHAPE == TreeShape.Mangrove)
                    {
                        offset = offset * 1.618f - 1.5f;
                    }

                    Vector3i endcoord = startcoord + new Vector3i(offset);
                    float rootstartsize = (float)(rootbaseradius * .618 * Math.Abs(offset[1]) / (height * .618));

                    if (rootstartsize < 1) rootstartsize = 1;
                    float endsize = 1;

                    if (args.ROOTS == RootMode.ToStone ||
                        args.ROOTS == RootMode.Hanging)
                    {
                        float offlength = offset.GetLength();
                        if (offlength < 1) continue;
                        float rootmid = endsize;
                        Vector3f vec = offset / offlength;

                        int searchIndex = TILETYPE_AIR;
                        if (args.ROOTS == RootMode.ToStone)
                        {
                            searchIndex = TILETYPE_STONE;
                        }
                        else if (args.ROOTS == RootMode.Hanging)
                        {
                            searchIndex = TILETYPE_AIR;
                        }

                        int startdist = (int)(args.rand.NextDouble() * 6 * Math.Sqrt(rootstartsize) + 2.8);
                        Vector3i searchstart = new Vector3i(startcoord + vec * startdist);

                        dist = startdist + DistanceToBlock(args.inMap, new Vector3f(searchstart), vec, searchIndex);

                        if (dist < offlength)
                        {
                            rootmid += (rootstartsize - endsize) * (1 - dist / offlength);
                            endcoord = new Vector3i(startcoord + vec * dist);
                            if (args.ROOTS == RootMode.Hanging)
                            {
                                float remaining_dist = offlength - dist;
                                Vector3i bottomcord = endcoord;
                                bottomcord[1] -= (int)remaining_dist;
                                TaperedLimb(endcoord, bottomcord, rootmid, endsize);
                            }
                        }
                        TaperedLimb(startcoord, endcoord, rootstartsize, rootmid);
                    }
                    else
                    {
                        TaperedLimb(startcoord, endcoord, rootstartsize, endsize);
                    }
                }
            }

            public override void MakeTrunk()
            {
                int starty = pos[1];
                int midy = (int)(pos[1] + trunkHeight * .382);
                int topy = (int)(pos[1] + trunkHeight + .5);

                int x = pos[0];
                int z = pos[2];
                float midrad = trunkRadius * .8f;
                float endrad = trunkRadius * (1 - trunkHeight / (float)height);

                if (endrad < 1) endrad = 1;
                if (midrad < endrad) midrad = endrad;

                float startrad;
                List<RootBase> rootbases = new List<RootBase>();
                if (args.ROOTBUTTRESSES || args.SHAPE == TreeShape.Mangrove)
                {
                    startrad = trunkRadius * .8f;
                    rootbases.Add(new RootBase
                    {
                        x = x,
                        z = z,
                        radius = startrad
                    });
                    float buttress_radius = trunkRadius * .382f;
                    float posradius = trunkRadius;
                    if (args.SHAPE == TreeShape.Mangrove)
                    {
                        posradius *= 2.618f;
                    }
                    int num_of_buttresss = (int)(Math.Sqrt(trunkRadius) + 3.5);
                    for (int i = 0; i < num_of_buttresss; i++)
                    {
                        float rndang = (float)(args.rand.NextDouble() * 2 * Math.PI);
                        float thisposradius = (float)(posradius * (.9 + args.rand.NextDouble() * .2));
                        int thisx = x + (int)(thisposradius * Math.Sin(rndang));
                        int thisz = z + (int)(thisposradius * Math.Cos(rndang));

                        float thisbuttressradius = (float)(buttress_radius * (.618 + args.rand.NextDouble()));
                        if (thisbuttressradius < 1) thisbuttressradius = 1;

                        TaperedLimb(new Vector3i(thisx, thisz, starty), new Vector3i(x, z, midy),
                                     thisbuttressradius, thisbuttressradius);
                        rootbases.Add(new RootBase
                        {
                            x = thisx,
                            z = thisz,
                            radius = thisbuttressradius
                        });
                    }
                }
                else
                {
                    startrad = trunkRadius;
                    rootbases.Add(new RootBase
                    {
                        x = x,
                        z = z,
                        radius = startrad
                    });
                }
                TaperedLimb(new Vector3i(x, z, starty), new Vector3i(x, z, midy), startrad, midrad);
                TaperedLimb(new Vector3i(x, z, midy), new Vector3i(x, z, topy), midrad, endrad);
                MakeBranches();
                if (args.ROOTS != RootMode.None)
                {
                    MakeRoots(rootbases.ToArray());
                }
            }

            public override void Prepare()
            {
                base.Prepare();
                trunkRadius = (float)Math.Sqrt(height * args.TRUNKTHICKNESS);
                if (trunkRadius < 1) trunkRadius = 1;

                trunkHeight = height * .618f;
                branchDensity = (args.BRANCHDENSITY / args.FOLIAGEDENSITY);

                int ystart = pos[1];
                int yend = (int)(pos[1] + height);
                int num_of_clusters_per_y = (int)(1.5 + Sqr(args.FOLIAGEDENSITY * height / 19f));
                if (num_of_clusters_per_y < 1) num_of_clusters_per_y = 1;

                List<Vector3i> _foliageCoords = new List<Vector3i>();
                for (int y = yend - 1; y >= ystart; y--)
                {
                    for (int i = 0; i < num_of_clusters_per_y; i++)
                    {
                        float shapefac = ShapeFunc(y - ystart);
                        if (shapefac < 0) continue;
                        float r = (float)((Math.Sqrt(args.rand.NextDouble()) + .328) * shapefac);
                        float theta = (float)(args.rand.NextDouble() * 2 * Math.PI);
                        int x = (int)(r * Math.Sin(theta)) + pos[0];
                        int z = (int)(r * Math.Cos(theta)) + pos[2];
                        _foliageCoords.Add(new Vector3i(x, z, y));
                    }
                }
                foliageCoords = _foliageCoords.ToArray();
            }
        }

        class RoundTree : ProceduralTree
        {
            public override void Prepare()
            {
                base.Prepare();
                branchSlope = .382f;
                foliageShape = new float[] { 2, 3, 3, 2.5f, 1.6f };
                trunkRadius *= .8f;
                trunkHeight = args.TRUNKHEIGHT * height;
            }

            public override float ShapeFunc(int y)
            {
                float twigs = base.ShapeFunc(y);
                if (twigs >= 0) return twigs;

                if (y < height * (.282 + .1 * Math.Sqrt(args.rand.NextDouble())))
                {
                    return -1;
                }

                float radius = height / 2f;
                float adj = height / 2f - y;
                float dist;
                if (adj == 0)
                {
                    dist = radius;
                }
                else if (Math.Abs(adj) >= radius)
                {
                    dist = 0;
                }
                else
                {
                    dist = (float)Math.Sqrt(radius * radius - adj * adj);
                }
                dist *= .618f;
                return dist;
            }
        }

        class ConeTree : ProceduralTree
        {
            public override void Prepare()
            {
                base.Prepare();
                branchSlope = .15f;
                foliageShape = new float[] { 3, 2.6f, 2, 1 };
                trunkRadius *= .618f;
                trunkHeight = height;
            }

            public override float ShapeFunc(int y)
            {
                float twigs = base.ShapeFunc(y);
                if (twigs >= 0) return twigs;
                if (y < height * (.25 + .05 * Math.Sqrt(args.rand.NextDouble())))
                {
                    return -1;
                }
                float radius = (height - y) * .382f;
                if (radius < 0) radius = 0;
                return radius;
            }
        }

        class RainforestTree : ProceduralTree
        {
            public override void Prepare()
            {
                foliageShape = new float[] { 3.4f, 2.6f };
                base.Prepare();
                branchSlope = 1;
                trunkRadius *= .382f;
                trunkHeight = height * .9f;
            }

            public override float ShapeFunc(int y)
            {
                if (y < height * .8)
                {
                    if (args.HEIGHT < height)
                    {
                        float twigs = base.ShapeFunc(y);
                        if (twigs >= 0 && args.rand.NextDouble() < .05)
                        {
                            return twigs;
                        }
                    }
                    return -1;
                }
                else
                {
                    float width = height * .382f;
                    float topdist = (height - y) / (height * .2f);
                    float dist = (float)(width * (.618 + topdist) * (.618 + args.rand.NextDouble()) * .382);
                    return dist;
                }
            }
        }

        class MangroveTree : RoundTree
        {
            public override void Prepare()
            {
                base.Prepare();
                branchSlope = 1;
                trunkRadius *= .618f;
            }
            public override float ShapeFunc(int y)
            {
                float val = base.ShapeFunc(y);
                if (val < 0) return -1;
                val *= 1.618f;
                return val;
            }
        }

        public enum Operation
        {
            ClearCut,
            Conserve,
            Replant,
            Add
        }

        public enum TreeShape
        {
            Normal,
            Bamboo,
            Palm,
            Stickly,
            Round,
            Cone,
            Procedural,
            Rainforest,
            Mangrove
        }

        public enum RootMode
        {
            Normal,
            ToStone,
            Hanging,
            None
        }

        ForesterArgs args;

        public Forester(ForesterArgs _args)
        {
            args = _args;
            args.Validate();
        }

        public static int DistanceToBlock(IMapStorage map, Vector3f coord, Vector3f vec, int blockType)
        {
            return DistanceToBlock(map, coord, vec, blockType, false);
        }
        public static int DistanceToBlock(IMapStorage map, Vector3f coord, Vector3f vec, int blockType, bool invert)
        {
            coord += .5f;
            int iterations = 0;
            while (MapUtil.IsValidPos(map, (int)coord.x, (int)coord.y, (int)coord.h))
            {
                byte blockAtPos = (byte)map.GetBlock((int)coord.x, (int)coord.y, (int)coord.h);
                if ((blockAtPos == (byte)blockType && !invert) ||
                    (blockAtPos != (byte)blockType && invert))
                {
                    break;
                }
                else
                {
                    coord += vec;
                    iterations++;
                }
            }
            return iterations;
        }

        void FindTrees(List<Tree> treelist, int gx, int gy, int gz, int chunksize)
        {
            return;//TODO Z
            int treeheight = args.HEIGHT;

            //for (int x = 0; x < args.inMap.MapSizeX; x++)
            for (int x = gx; x < gx + chunksize; x++)
            {
                //    for (int z = 0; z < args.inMap.MapSizeY; z++)
                for (int z = gy; z < gy + chunksize; gy++)
                {
                    int y = args.inMap.MapSizeZ - 1;
                    while (true)
                    {
                        int foliagetop = MapUtil.SearchColumn(args.inMap, x, z, TILETYPE_LEAVES, y);
                        if (foliagetop < 0) break;
                        y = foliagetop;
                        Vector3i trunktop = new Vector3i(x, z, y - 1);
                        int height = DistanceToBlock(args.inMap, new Vector3f(trunktop), new Vector3f(0, 0, -1), TILETYPE_LOG, true);
                        if (height == 0)
                        {
                            y--;
                            continue;
                        }
                        y -= height;
                        if (args.HEIGHT > 0)
                        {
                            height = args.rand.Next(treeheight - args.HEIGHTVARIATION,
                                                     treeheight + args.HEIGHTVARIATION + 1);
                        }
                        treelist.Add(new Tree
                        {
                            args = args,
                            pos = new Vector3i(x, z, y),
                            height = height
                        });
                        y--;
                    }
                }
            }
        }

        void PlantTrees(List<Tree> treelist, int gx, int gy, int gz, int chunksize)
        {
            int treeheight = args.HEIGHT;

            int tries = 0;
            while (treelist.Count < args.TREECOUNT)
            {
                if (tries++ > 10) { return; }
                int height = args.rand.Next(treeheight - args.HEIGHTVARIATION,
                                             treeheight + args.HEIGHTVARIATION + 1);

                Vector3i treeLoc = RandomTreeLoc(gx, gy, gz, chunksize, height);
                if (treeLoc.y < 0) continue;
                else treeLoc.y++;
                treelist.Add(new Tree
                {
                    args = args,
                    height = height,
                    pos = treeLoc
                });
            }
        }

        Vector3i RandomTreeLoc(int gx, int gy, int gz, int chunksize, int height)
        {
            int padding = (int)(height / 3f + 1);
            int mindim = Math.Min(args.inMap.MapSizeX, args.inMap.MapSizeY);
            if (padding > mindim / 2.2)
            {
                padding = (int)(mindim / 2.2);
            }
            int x = args.rand.Next(padding + gx, gx + chunksize - padding - 1);
            int z = args.rand.Next(padding + gy, gy + chunksize - padding - 1);
            int y = MapUtil.SearchColumn(args.inMap, x, z, args.PLANTON);
            return new Vector3i(x, z, y);
        }


        void PlantRainForestTrees(List<Tree> treelist,int gx, int gy, int gz, int chunksize)
        {
            int treeheight = args.HEIGHT;

            int existingtreenum = treelist.Count;
            int remainingtrees = args.TREECOUNT - existingtreenum;

            int short_tree_fraction = 6;
            int tries = 0;
            for (int i = 0; i < remainingtrees; )
            {
                if (tries++ > 10) { return; }
                float randomfac = (float)((Math.Sqrt(args.rand.NextDouble()) * 1.618 - .618) * args.HEIGHTVARIATION + .5);

                int height;
                if (i % short_tree_fraction == 0)
                {
                    height = (int)(treeheight + randomfac);
                }
                else
                {
                    height = (int)(treeheight - randomfac);
                }
                Vector3i xyz = RandomTreeLoc(gx, gy, gz, chunksize, height);
                if (xyz.y < 0) continue;

                xyz.y++;

                bool displaced = false;
                foreach (Tree othertree in treelist)
                {
                    Vector3i other_loc = othertree.pos;
                    float otherheight = othertree.height;
                    int tallx = other_loc[0];
                    int tallz = other_loc[2];
                    float dist = (float)Math.Sqrt(Sqr(tallx - xyz.x + .5) + Sqr(tallz - xyz.z + .5));
                    float threshold = (otherheight + height) * .193f;
                    if (dist < threshold)
                    {
                        displaced = true;
                        break;
                    }
                }
                if (displaced) continue;
                treelist.Add(new RainforestTree
                {
                    args = args,
                    pos = xyz,
                    height = height
                });
                i++;
            }
        }

        void PlantMangroves(List<Tree> treelist, int gx, int gy, int gz, int chunksize)
        {
            int treeheight = args.HEIGHT;
            int tries = 0;
            while (treelist.Count < args.TREECOUNT)
            {
                if (tries++ > 10) { return; }
                int height = args.rand.Next(treeheight - args.HEIGHTVARIATION,
                                             treeheight + args.HEIGHTVARIATION + 1);
                int padding = (int)(height / 3f + 1);
                int mindim = Math.Min(args.inMap.MapSizeX, args.inMap.MapSizeY);
                if (padding > mindim / 2.2)
                {
                    padding = (int)(mindim / 2.2);
                }
                int x = args.rand.Next(padding + gx, gx + chunksize - padding - 1);
                int z = args.rand.Next(padding + gy, gy + chunksize - padding - 1);
                int top = args.inMap.MapSizeZ - 1;

                int y = top - DistanceToBlock(args.inMap, new Vector3f(x, z, top), new Vector3f(0, 0, -1), TILETYPE_AIR, true);
                int dist = DistanceToBlock(args.inMap, new Vector3f(x, z, y), new Vector3f(0, 0, -1), TILETYPE_WATER, true);

                if (dist > height * .618 || dist == 0)
                {
                    continue;
                }

                y += (int)Math.Sqrt(height - dist) + 2;
                treelist.Add(new Tree
                {
                    args = args,
                    height = height,
                    pos = new Vector3i(x, z, y)
                });
            }
        }

        void ProcessTrees(List<Tree> treelist)
        {
            TreeShape[] shape_choices;
            switch (args.SHAPE)
            {
                case TreeShape.Stickly:
                    shape_choices = new TreeShape[]{ TreeShape.Normal,
                                                     TreeShape.Bamboo,
                                                     TreeShape.Palm};
                    break;
                case TreeShape.Procedural:
                    shape_choices = new TreeShape[]{ TreeShape.Round,
                                                     TreeShape.Cone };
                    break;
                default:
                    shape_choices = new TreeShape[] { args.SHAPE };
                    break;
            }

            for (int i = 0; i < treelist.Count; i++)
            {
                TreeShape newshape = shape_choices[args.rand.Next(0, shape_choices.Length)];
                Tree newtree;
                switch (newshape)
                {
                    case TreeShape.Normal:
                        newtree = new NormalTree();
                        break;
                    case TreeShape.Bamboo:
                        newtree = new BambooTree();
                        break;
                    case TreeShape.Palm:
                        newtree = new PalmTree();
                        break;
                    case TreeShape.Round:
                        newtree = new RoundTree();
                        break;
                    case TreeShape.Cone:
                        newtree = new ConeTree();
                        break;
                    case TreeShape.Rainforest:
                        newtree = new RainforestTree();
                        break;
                    case TreeShape.Mangrove:
                        newtree = new MangroveTree();
                        break;
                    default:
                        throw new ArgumentException();
                }
                newtree.Copy(treelist[i]);

                if (args.MAPHEIGHTLIMIT)
                {
                    int height = newtree.height;
                    int ybase = newtree.pos[1];
                    int mapheight = args.inMap.MapSizeZ;
                    int foliageheight;
                    if (args.SHAPE == TreeShape.Rainforest)
                    {
                        foliageheight = 2;
                    }
                    else
                    {
                        foliageheight = 4;
                    }
                    if (ybase + height + foliageheight > mapheight)
                    {
                        newtree.height = mapheight - ybase - foliageheight;
                    }
                }

                if (newtree.height < 1) newtree.height = 1;
                newtree.Prepare();
                treelist[i] = newtree;
            }
        }

        public void Generate(int x, int y, int z, int chunksize)
        {
            List<Tree> treelist = new List<Tree>();

            if (args.OPERATION == Operation.Conserve)
            {
                FindTrees(treelist, x, y, z, chunksize);
            }

            if (args.TREECOUNT > 0 && treelist.Count > args.TREECOUNT)
            {
                treelist = new List<Tree>(MyLinq.Take(treelist, args.TREECOUNT));
            }

            if (args.OPERATION == Operation.Replant || args.OPERATION == Operation.Add)
            {
                switch (args.SHAPE)
                {
                    case TreeShape.Rainforest:
                        PlantRainForestTrees(treelist, x, y, z, chunksize);
                        break;
                    case TreeShape.Mangrove:
                        PlantMangroves(treelist, x, y, z, chunksize);
                        break;
                    default:
                        PlantTrees(treelist, x, y, z, chunksize);
                        break;
                }
            }

            if (args.OPERATION != Operation.ClearCut)
            {
                ProcessTrees(treelist);
                if (args.FOLIAGE)
                {
                    foreach (Tree tree in treelist)
                    {
                        tree.MakeFoliage();
                    }
                }
                if (args.WOOD)
                {
                    foreach (Tree tree in treelist)
                    {
                        tree.MakeTrunk();
                    }
                }
            }
        }

        public static float Sqr(float val)
        {
            return val * val;
        }
        public static float Cub(float val)
        {
            return val * val * val;
        }
        public static int Sqr(int val)
        {
            return val * val;
        }
        public static int Cub(int val)
        {
            return val * val * val;
        }
        public static double Sqr(double val)
        {
            return val * val;
        }
        public static double Cub(double val)
        {
            return val * val * val;
        }
    }
}
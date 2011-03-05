using System;
using ManicDigger;
    public interface IWorldGenerator
    {
        byte[, ,] GetChunk(int x, int y, int z, int chunksize);
        void PopulateChunk(IMapStorage map, int x, int y, int z, int chunksize);
    }
    public class WorldGeneratorDummy : IWorldGenerator
    {
        #region IWorldGenerator Members
        public byte[, ,] GetChunk(int x, int y, int z, int chunksize)
        {
            return null;
        }
        #endregion
        #region IWorldGenerator Members
        public void PopulateChunk(IMapStorage map, int x, int y, int z, int chunksize)
        {
        }
        #endregion
    }
    public class WorldGenerator : IWorldGenerator
    {
        public WorldGenerator()
        {
        }
        int seed = 0;
        public void SetSeed(int seed)
        {
            this.seed = seed;
        }
        int waterlevel = 20;
        byte[,] heightcache;
        int chunksize;
        #region IWorldGenerator Members
        public byte[, ,] GetChunk(int x, int y, int z, int chunksize)
        {
            this.chunksize = chunksize;
            heightcache = new byte[chunksize, chunksize];
            x = x * chunksize;
            y = y * chunksize;
            z = z * chunksize;
            byte[, ,] chunk = new byte[chunksize, chunksize, chunksize];
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    heightcache[xx, yy] = GetHeight(x + xx, y + yy);
                }
            }
            // chance of get hay fields
            bool IsHay = rnd.NextDouble() < 0.005 ? false : true;

            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        chunk[xx, yy, zz] = IsHay
                        ? (byte)GetBlock(x + xx, y + yy, z + zz, heightcache[xx, yy], 0)
                        : (byte)GetBlock(x + xx, y + yy, z + zz, heightcache[xx, yy], 1);
                    }
                }
            }
            if (z == 0)
            {
                for (int xx = 0; xx < chunksize; xx++)
                {
                    for (int yy = 0; yy < chunksize; yy++)
                    {
                        chunk[xx, yy, 0] = (byte)TileIdLava;
                    }
                }
            }
            return chunk;
        }
        int TileIdEmpty = 0;
        int TileIdGrass = 2;
        int TileIdDirt = 3;
        int TileIdWater = 8;
        int TileIdSand = 12;
        int TileIdGravel = 13;
        int TileIdTreeTrunk = 17;
        int TileIdLeaves = 18;
        int TileIdStone = 1;
        int TileIdGoldOre = 14;
        int TileIdIronOre = 15;
        int TileIdCoalOre = 16;
        int TileIdLava = 11;
        int TileIdYellowFlower = 37;
        int TileIdRedFlower = 38;
        int TileIdApples = 106;
        int TileIdHay = 107;
        float treedensity = 0.008f;
        // if param 'special' is equal to 1 then hay fields grows
        int GetBlock(int x, int y, int z, int height, int special)
        {
            int spec = special;
            int tile = TileIdStone;

            if (z > waterlevel)
            {
                if (spec == 0)
                {
                    if (z > height) { return TileIdEmpty; }
                    if (z == height) { return TileIdGrass; }
                }
                else
                {
                    if (z > height + 1) { return TileIdEmpty; }
                    if (z == height) { return TileIdHay; }
                    if (z == height + 1) { return TileIdHay; }
                }
                if (z > height - 5) { return TileIdDirt; }
                return TileIdStone;
            }
            else
            {
                if (z > height) { return TileIdWater; }
                if (z == height) { return TileIdSand; }
                return TileIdStone;
            }
        }
        bool IsTree(int x, int y, int height)
        {
            return height >= waterlevel + 3
                && x % 16 >= 2 && x % 16 < chunksize - 2
                && y % 16 >= 2 && y % 16 < chunksize - 2
                && (((noise(x, y) + 1) / 2) > 1 - treedensity);
                //&& (((noise((x * 3) + 1000, (y * 3) + 1000) + 1) / 2) > 1 - treedensity);
        }
        private byte GetHeight(int x, int y)
        {
            x += 30; y -= 30;
            //double p = 0.2 + ((findnoise2(x / 100.0, y / 100.0) + 1.0) / 2) * 0.3;
            double p = 0.5;
            double zoom = 150;
            double getnoise = 0;
            int octaves = 6;
            for (int a = 0; a < octaves - 1; a++)//This loops trough the octaves.
            {
                double frequency = Math.Pow(2, a);//This increases the frequency with every loop of the octave.
                double amplitude = Math.Pow(p, a);//This decreases the amplitude with every loop of the octave.
                getnoise += noise(((double)x) * frequency / zoom, ((double)y) / zoom * frequency) * amplitude;//This uses our perlin noise functions. It calculates all our zoom and frequency and amplitude
            }
            double maxheight = 64;
            int height = (int)(((getnoise + 1) / 2.0) * (maxheight - 5)) + 3;//(int)((getnoise * 128.0) + 128.0);
            if (height > maxheight - 1) { height = (int)maxheight - 1; }
            if (height < 2) { height = 2; }
            return (byte)height;
        }
        #endregion
        //returns number between -1 and 1.
        double FindNoise2(double x, double y)
        {
            int n = (int)x + (int)y * 57;
            return FindNoise1(n);
        }
        private double FindNoise1(int n)
        {
            n += seed;
            n = (n << 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return 1.0 - ((double)nn / 1073741824.0);
        }
        double interpolate(double a, double b, double x)
        {
            double ft = x * 3.1415927;
            double f = (1.0 - Math.Cos(ft)) * 0.5;
            return a * (1.0 - f) + b * f;
        }
        double noise(double x, double y)
        {
            double floorx = (double)((int)x);//This is kinda a cheap way to floor a double integer.
            double floory = (double)((int)y);
            double s, t, u, v;//Integer declaration
            s = FindNoise2(floorx, floory);
            t = FindNoise2(floorx + 1, floory);
            u = FindNoise2(floorx, floory + 1);//Get the surrounding pixels to calculate the transition.
            v = FindNoise2(floorx + 1, floory + 1);
            double int1 = interpolate(s, t, x - floorx);//Interpolate between the values.
            double int2 = interpolate(u, v, x - floorx);//Here we use x-floorx, to get 1st dimension. Don't mind the x-floorx thingie, it's part of the cosine formula.
            return interpolate(int1, int2, y - floory);//Here we use y-floory, to get the 2nd dimension.
        }
        public Random rnd = new Random();
        int goldorelength = 50;
        int ironorelength = 50;
        int coalorelength = 50;
        int gravellength = 50;
        int dirtlength = 40;
        #region IWorldGenerator Members
        public void PopulateChunk(IMapStorage map, int x, int y, int z, int chunksize)
        {
            x *= chunksize;
            y *= chunksize;
            z *= chunksize;
            if (!EnableBigTrees)
            {
                MakeSmallTrees(map, x, y, z, chunksize);
            }
            else
            {
                MakeTrees(map, x, y, z, chunksize);
            }
            MakeCaves(map, x, y, z, chunksize);
        }
        public bool EnableBigTrees = false;
        public bool EnableCaves = false;
        private void MakeCaves(IMapStorage map, int x, int y, int z, int chunksize)
        {
            //if (rnd.NextDouble() >= 0.6)
            {
                //return;
            }

            //find cave start
            double curx = x;
            double cury = y;
            double curz = z;
            for (int i = 0; i < 2; i++)
            {
                curx = x + rnd.Next(chunksize);
                cury = y + rnd.Next(chunksize);
                curz = z + rnd.Next(chunksize);
                if (map.GetBlock((int)curx, (int)cury, (int)curz) == TileIdStone)
                {
                    goto ok;
                }
            }
            return;
        ok:
            int blocktype = TileIdEmpty;
            int length = 200;
            if (rnd.NextDouble() < 0.85)
            {
                int oretype = rnd.Next(5);
                if (oretype == 0) { length = gravellength; }
                if (oretype == 1) { length = goldorelength; }
                if (oretype == 2) { length = ironorelength; }
                if (oretype == 3) { length = coalorelength; }
                if (oretype == 4) { length = dirtlength; }

                length = rnd.Next(length);
                blocktype = oretype != 4 ? TileIdGravel + oretype : TileIdDirt;
            }
            if (blocktype == TileIdEmpty && (!EnableCaves))
            {
                return;
            }
            //map.SetBlock(x, y, z, TileIdLava);
            int dirx = rnd.NextDouble() < 0.5 ? -1 : 1;
            int dirz = rnd.NextDouble() < 0.5 ? -1 : 1;
            double curspeedx = rnd.NextDouble() * dirx;
            double curspeedy = rnd.NextDouble();
            double curspeedz = rnd.NextDouble() * 0.5 * dirz;
            for (int i = 0; i < length; i++)
            {
                if (rnd.NextDouble() < 0.06)
                {
                    curspeedx = rnd.NextDouble() * dirx;
                }
                if (rnd.NextDouble() < 0.06)
                {
                    curspeedy = rnd.NextDouble() * dirx;
                }
                if (rnd.NextDouble() < 0.02)
                {
                    curspeedz = rnd.NextDouble() * 0.5 * dirz;
                }
                curx += curspeedx;
                cury += curspeedy;
                curz += curspeedz;
                if (!MapUtil.IsValidPos(map, (int)curx, (int)cury, (int)curz))
                {
                    continue;
                }
                for (int ii = 0; ii < 3; ii++)
                {
                    int sizex = rnd.Next(3, 6);
                    int sizey = rnd.Next(3, 6);
                    int sizez = rnd.Next(2, 3);
                    int dx = rnd.Next(-sizex / 2, sizex / 2);
                    int dy = rnd.Next(-sizey / 2, sizey / 2);
                    int dz = rnd.Next(-sizez / 1, sizez / 1);

                    int[] allowin = new int[] { TileIdStone };
                    double density = blocktype == TileIdEmpty ? 1 : rnd.NextDouble() * 0.90;
                    if (blocktype == TileIdEmpty)
                    {
                        allowin = new int[] { TileIdStone, TileIdDirt, TileIdGrass, TileIdGoldOre, TileIdIronOre, TileIdCoalOre };
                    }
                    if (blocktype == TileIdGravel)
                    {
                        density = 1;
                        allowin = new int[] { TileIdDirt, TileIdStone, TileIdSand, TileIdGoldOre, TileIdIronOre, TileIdCoalOre };
                    }

                    MakeCuboid(map, (int)curx - sizex / 2 + dx, (int)cury - sizey / 2 + dy, (int)curz - sizez / 2 + dz, sizex, sizey, sizez, blocktype, allowin, density);
                }
            }
        }
        private void MakeSmallTrees(IMapStorage map, int cx, int cy, int cz, int chunksize)
        {
            int chooseTreeType;
            for (int i = 0; i < 30; i++)
            {
                int x = cx + rnd.Next(chunksize);
                int y = cy + rnd.Next(chunksize);
                int z = cz + rnd.Next(chunksize);
                if (!MapUtil.IsValidPos(map, x, y, z) || map.GetBlock(x, y, z) != TileIdGrass)
                {
                    continue;
                }
                chooseTreeType = rnd.Next(0, 3);
                switch (chooseTreeType)
                {
                    case 0: MakeTreeType1(map, x, y, z); break;
                    case 1: MakeTreeType2(map, x, y, z); break;
                    case 2: MakeTreeType3(map, x, y, z); break;
                };
            }
        }
        #region MakeTreeTypes
        private void MakeTreeType1(IMapStorage map, int x, int y, int z)
        {
            int treeHeight = rnd.Next(8, 12);
            int xx = 0;
            int yy = 0;
            int dir = 0;

            for (int i = 0; i < treeHeight; i++)
            {
                SetBlock(map, x, y, z + i, TileIdTreeTrunk);
                if (i == treeHeight - 4)
                {
                    SetBlock(map, x + 1, y, z + i, TileIdTreeTrunk);
                    SetBlock(map, x - 1, y, z + i, TileIdTreeTrunk);
                    SetBlock(map, x, y + 1, z + i, TileIdTreeTrunk);
                    SetBlock(map, x, y - 1, z + i, TileIdTreeTrunk);
                }

                if (i == treeHeight - 3)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 4; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, TileIdLeaves);
                        }
                    }
                }
                if (i == treeHeight - 1)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 3; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, TileIdLeaves);
                        }
                    }
                }
            }
        }
        private void MakeTreeType2(IMapStorage map, int x, int y, int z)
        {
            int treeHeight = rnd.Next(4, 6);
            int xx = 0;
            int yy = 0;
            int dir = 0;
            float chanceToAppleTree = 0.1f;
            for (int i = 0; i < treeHeight; i++)
            {
                SetBlock(map, x, y, z + i, TileIdTreeTrunk);
                if (i == treeHeight - 1)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 2; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, TileIdTreeTrunk);
                            if (chanceToAppleTree < rnd.NextDouble())
                            {
                                SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, TileIdLeaves);
                                SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, TileIdLeaves);
                            }
                            else
                            {
                                float appleChance = 0.4f;
                                int tile;
                                tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, tile);
                                tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, tile);
                                tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, tile);
                                tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, tile);
                                tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, tile);
                            }
                        }
                    }
                }
            }
        }
        private void MakeTreeType3(IMapStorage map, int x, int y, int z)
        {
            int treeHeight = rnd.Next(6, 9);
            int xx = 0;
            int yy = 0;
            int dir = 0;
            for (int i = 0; i < treeHeight; i++)
            {
                SetBlock(map, x, y, z + i, TileIdTreeTrunk);
                if (i % 3 == 0 && i > 3)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 2; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, TileIdLeaves);
                        }
                    }
                }
                if (i % 3 == 2 && i > 3)
                {
                    dir = 45;
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 3; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            SetBlock(map, x + xx, y + yy, z + i, TileIdTreeTrunk);
                            SetBlockIfEmpty(map, x + xx, y + yy, z + i + 1, TileIdLeaves);

                            SetBlockIfEmpty(map, x + xx + 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx - 1, y + yy, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy + 1, z + i, TileIdLeaves);
                            SetBlockIfEmpty(map, x + xx, y + yy - 1, z + i, TileIdLeaves);
                        }
                    }
                }
                SetBlockIfEmpty(map, x, y, z + treeHeight, TileIdLeaves);
            }
        }
        #endregion
        private void MakeFlowers(IMapStorage map, int cx, int cy, int cz, int chunksize)
        {
            for (int i = 0; i < 5; i++)
            {
                int x = cx + rnd.Next(chunksize);
                int y = cy + rnd.Next(chunksize);
                int z = cz + rnd.Next(chunksize);
                if (!MapUtil.IsValidPos(map, x, y, z) || map.GetBlock(x, y, z) != TileIdGrass)
                {
                    continue;
                }
                int xx; int yy; int zz;
                int tile = rnd.NextDouble() < 0.75 ? TileIdYellowFlower : TileIdRedFlower;
                int count = rnd.Next(50, 80);
                for (int j = 0; j < count; j++)
                {
                    xx = x + rnd.Next(-6, 6);
                    yy = y + rnd.Next(-6, 6);
                    zz = z + rnd.Next(-2, 2);
                    if (!MapUtil.IsValidPos(map, xx, yy, zz) || map.GetBlock(xx, yy, zz) != TileIdGrass)
                    {
                        continue;
                    }
                    SetBlock(map, xx, yy, zz + 1, tile);
                }

            }
        }
        private void SetBlock(IMapStorage map, int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(map, x, y, z))
            {
                map.SetBlock(x, y, z, blocktype);
            }
        }
        private void SetBlockIfEmpty(IMapStorage map, int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(map, x, y, z) && map.GetBlock(x, y, z) == TileIdEmpty)
            {
                map.SetBlock(x, y, z, blocktype);
            }
        }
        void MakeCuboid(IMapStorage map, int x, int y, int z, int sizex, int sizey, int sizez, int blocktype, int[] allowin, double chance)
        {
            for (int xx = 0; xx < sizex; xx++)
            {
                for (int yy = 0; yy < sizey; yy++)
                {
                    for (int zz = 0; zz < sizez; zz++)
                    {
                        if (MapUtil.IsValidPos(map, x + xx, y + yy, z + zz))
                        {
                            int t = map.GetBlock(x + xx, y + yy, z + zz);
                            if (allowin == null) { goto ok; }
                            foreach (int tt in allowin)
                            {
                                if (tt == t) { goto ok; }
                            }
                            continue;
                        ok:
                            if (rnd.NextDouble() < chance)
                            {
                                map.SetBlock(x + xx, y + yy, z + zz, blocktype);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        void MakeTrees(IMapStorage map, int x, int y, int z, int chunksize)
        {
            if (z != 0) { return; }
            //if (rnd.Next(100) > 30) { return; }
            var foresterArgs = new fCraft.Forester.ForesterArgs();
            if (rnd.Next(100) < 15)
            {
                foresterArgs.SHAPE = fCraft.Forester.TreeShape.Procedural;
            }
            else
            {
                foresterArgs.SHAPE = (fCraft.Forester.TreeShape)rnd.Next(9);
                foresterArgs.HEIGHT = rnd.Next(5, 10);
                foresterArgs.TREECOUNT = 3;
            }
            fCraft.Forester forester = new fCraft.Forester(foresterArgs);
            foresterArgs.inMap = map;
            foresterArgs.outMap = map;
            forester.Generate(x, y, z, chunksize);
        }
    }
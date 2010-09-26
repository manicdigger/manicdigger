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
            istreecache = new bool[chunksize, chunksize];
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
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        chunk[xx, yy, zz] = (byte)GetBlock(x + xx, y + yy, z + zz, heightcache[xx, yy]);
                    }
                }
            }
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    if (IsTree(x + xx, y + yy, heightcache[xx, yy]))
                    {
                        PlaceTree(chunk, chunksize, xx, yy, heightcache[xx, yy] - z);
                    }
                }
            }
            istreecache = null;
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
        void PlaceTree(byte[, ,] chunk, int chunksize, int xx, int yy, int zz)
        {
            if (zz < 0) { return; }
            if (chunksize - zz < 5) { return; }
            Place(chunk, chunksize, xx, yy, zz + 1, TileIdTreeTrunk);
            Place(chunk, chunksize, xx, yy, zz + 2, TileIdTreeTrunk);
            Place(chunk, chunksize, xx, yy, zz + 3, TileIdTreeTrunk);

            Place(chunk, chunksize, xx + 1, yy, zz + 3, TileIdLeaves);
            Place(chunk, chunksize, xx - 1, yy, zz + 3, TileIdLeaves);
            Place(chunk, chunksize, xx, yy + 1, zz + 3, TileIdLeaves);
            Place(chunk, chunksize, xx, yy - 1, zz + 3, TileIdLeaves);

            Place(chunk, chunksize, xx + 1, yy + 1, zz + 3, TileIdLeaves);
            Place(chunk, chunksize, xx + 1, yy - 1, zz + 3, TileIdLeaves);
            Place(chunk, chunksize, xx - 1, yy + 1, zz + 3, TileIdLeaves);
            Place(chunk, chunksize, xx - 1, yy - 1, zz + 3, TileIdLeaves);

            Place(chunk, chunksize, xx + 1, yy, zz + 4, TileIdLeaves);
            Place(chunk, chunksize, xx - 1, yy, zz + 4, TileIdLeaves);
            Place(chunk, chunksize, xx, yy + 1, zz + 4, TileIdLeaves);
            Place(chunk, chunksize, xx, yy - 1, zz + 4, TileIdLeaves);

            Place(chunk, chunksize, xx, yy, zz + 4, TileIdLeaves);
        }
        void Place(byte[, ,] chunk, int chunksize, int xx, int yy, int zz, int blocktype)
        {
            if (xx < 0 || xx >= chunksize) { return; }
            if (yy < 0 || yy >= chunksize) { return; }
            if (zz < 0 || zz >= chunksize) { return; }
            chunk[xx, yy, zz] = (byte)blocktype;
        }
        bool[,] istreecache;
        int TileIdEmpty = 0;
        int TileIdGrass = 2;
        int TileIdDirt = 3;
        int TileIdWater = 8;
        int TileIdSand = 12;
        int TileIdTreeTrunk = 17;
        int TileIdLeaves = 18;
        int TileIdStone = 1;
        int TileIdGoldOre = 14;
        int TileIdIronOre = 15;
        int TileIdCoalOre = 16;
        int TileIdLava = 11;
        float treedensity = 0.008f;
        int GetBlock(int x, int y, int z, int height)
        {
            if (z > waterlevel)
            {
                if (z > height) { return TileIdEmpty; }
                if (z == height) { return TileIdGrass; }
                if (z > height - 5) { return TileIdDirt; }
                return TileIdStone;
            }
            else
            {
                if (z > height) { return TileIdWater; }
                if (z == height) { return TileIdSand; }
                if (z > height - 5) { return TileIdDirt; }
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
        int goldorelength = 150;
        int ironorelength = 150;
        int coalorelength = 150;
        #region IWorldGenerator Members
        public void PopulateChunk(IMapStorage map, int x, int y, int z, int chunksize)
        {
            x *= chunksize;
            y *= chunksize;
            z *= chunksize;
            //if (rnd.NextDouble() >= 0.6)
            {
                //return;
            }

            //find cave start
            double curx = x;
            double cury = y;
            double curz = z;
            for (int i = 0; i < 10; i++)
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
            if (rnd.NextDouble() < 0.5)
            {
                int oretype = rnd.Next(3);
                if (oretype == 0) { length = goldorelength; }
                if (oretype == 1) { length = ironorelength; }
                if (oretype == 2) { length = coalorelength; }
                length = rnd.Next(length);
                blocktype = TileIdGoldOre + oretype;
            }
            //map.SetBlock(x, y, z, TileIdLava);
            double curspeedx = rnd.Next(-1, 2);
            double curspeedy = rnd.Next(-1, 2);
            double curspeedz = rnd.Next(-1, 2);
            int dir = rnd.NextDouble() < 0.5 ? -1 : 1;
            for (int i = 0; i < length; i++)
            {
                if (rnd.NextDouble() < 0.05)
                {
                    curspeedx = rnd.NextDouble() * dir;
                }
                if (rnd.NextDouble() < 0.05)
                {
                    curspeedy = rnd.NextDouble() * dir;
                }
                if (rnd.NextDouble() < 0.02)
                {
                    curspeedz = rnd.NextDouble() * 0.5 * dir;
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
                    int sizex = rnd.Next(3, 5);
                    int sizey = rnd.Next(3, 5);
                    int sizez = rnd.Next(3, 5);
                    int dx = rnd.Next(-sizex / 1, sizex / 1);
                    int dy = rnd.Next(-sizey / 1, sizey / 1);
                    int dz = rnd.Next(-sizez / 2, sizez / 2);
                    int[] allowin = blocktype == TileIdEmpty ? new int[] { TileIdStone, TileIdDirt, TileIdGrass } : new int[] { TileIdStone };
                    double density = blocktype == TileIdEmpty ? 1 : 0.5;
                    MakeCuboid(map, (int)curx - sizex / 2 + dx, (int)cury - sizey / 2 + dy, (int)curz - sizez / 2 + dz, sizex, sizey, sizez, blocktype, allowin, density);
                }
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
    }
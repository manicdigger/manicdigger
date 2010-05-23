using System;
    public interface IWorldGenerator
    {
        byte[] GetBlocks(int[] pos);
        byte[, ,] GetChunk(int x, int y, int z, int chunksize);
    }
    public class WorldGenerator : IWorldGenerator
    {
        public WorldGenerator()
        {
        }
        int waterlevel = 20;
        byte[,] heightcache;
        #region IWorldGenerator Members
        public byte[, ,] GetChunk(int x, int y, int z, int chunksize)
        {
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
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        chunk[xx, yy, zz] = (byte)GetBlockInside(x + xx, y + yy, z + zz, heightcache[xx, yy]);
                    }
                }
            }
            return chunk;
        }
        public int GetBlock(int x, int y, int z)
        {
            return GetBlockInside(x, y, z, GetHeight(x, y));
        }
        int TileIdEmpty = 0;
        int TileIdGrass = 2;
        int TileIdDirt = 3;
        int TileIdWater = 8;
        int TileIdSand = 12;
        int TileIdTreeTrunk = 17;
        int TileIdLeaves = 18;
        float treedensity = 0.006f;
        int GetBlockInside(int x, int y, int z, int height)
        {
            int tree = Tree(x, y, z, height);
            if (tree != 0)
            {
                return tree;
            }
            if (z > waterlevel)
            {
                if (z > height) { return TileIdEmpty; }
                if (z == height) { return TileIdGrass; }
                return TileIdDirt;
            }
            else
            {
                if (z > height) { return TileIdWater; }
                if (z == height) { return TileIdSand; }
                return TileIdDirt;
            }
        }
        int Tree(int x, int y, int z, int height)
        {
            //trunk
            if (z == height + 1 || z == height + 2 || z == height + 3)
            {
                if (IsTree(x, y, height)) { return TileIdTreeTrunk; }
            }
            if (z - height < 10)
            {
                if (IsLeaves(x, y, z, -1, 0, 3, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 1, 0, 3, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 0, -1, 3, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 0, 1, 3, height)) { return TileIdLeaves; }

                if (IsLeaves(x, y, z, -1, 0, 4, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 1, 0, 4, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 0, -1, 4, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 0, 1, 4, height)) { return TileIdLeaves; }

                if (IsLeaves(x, y, z, -1, -1, 3, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 1, -1, 3, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, -1, 1, 3, height)) { return TileIdLeaves; }
                if (IsLeaves(x, y, z, 1, 1, 3, height)) { return TileIdLeaves; }

                if (IsLeaves(x, y, z, 0, 0, 4, height)) { return TileIdLeaves; }
            }
            return 0;
        }
        bool IsLeaves(int x, int y, int z, int xdiff, int ydiff, int zdiff, int height)
        {
            if (IsTree(x + xdiff, y + ydiff, height))
            {
                int heightnear = GetHeight(x + xdiff, y + ydiff);
                return (heightnear + zdiff == z);
            }
            return false;
        }
        bool IsTree(int x, int y, int height)
        {
            return height >= waterlevel + 3
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
        double findnoise2(double x, double y)
        {
            int n = (int)x + (int)y * 57;
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
            s = findnoise2(floorx, floory);
            t = findnoise2(floorx + 1, floory);
            u = findnoise2(floorx, floory + 1);//Get the surrounding pixels to calculate the transition.
            v = findnoise2(floorx + 1, floory + 1);
            double int1 = interpolate(s, t, x - floorx);//Interpolate between the values.
            double int2 = interpolate(u, v, x - floorx);//Here we use x-floorx, to get 1st dimension. Don't mind the x-floorx thingie, it's part of the cosine formula.
            return interpolate(int1, int2, y - floory);//Here we use y-floory, to get the 2nd dimension.
        }
        #region IWorldGenerator Members
        public byte[] GetBlocks(int[] pos)
        {
            byte[] blocks = new byte[pos.Length / 3];
            for (int i = 0; i < pos.Length / 3; i += 3)
            {
                int x = i;
                int y = i + 1;
                int z = i + 2;
                blocks[i / 3] = (byte)GetBlock(x, y, z);
            }
            return blocks;
        }
        #endregion
    }
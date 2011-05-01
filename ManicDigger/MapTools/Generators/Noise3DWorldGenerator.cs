#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools.Generators
{
    /// <summary>
    /// Provides a chunk generator that generates chunks using the class Manic Digger (up to 2011-Mar-18) chunk generation algorithm.
    /// </summary>
    public class Noise3DWorldGenerator : WorldGeneratorBase
    {
        #region Fields

        private Random _rnd;
        private byte[,] heightcache;
        private double maxnoise = 0.32549166667822577d;
        private double[, ,] interpolatednoise;
        private int goldorelength = 50;
        private int ironorelength = 50;
        private int coalorelength = 50;
        private int gravellength = 50;
        private int dirtlength = 40;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a string representing the display name of this world generator (for GUI purposes only).
        /// </summary>
        public override string DisplayName
        {
            get { return "Manic Digger (Noise 3D)"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Noise3DWorldGenerator class.
        /// </summary>
        public Noise3DWorldGenerator()
            : base()
        {
            _rnd = new Random(DateTime.Now.Millisecond);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the contents of the chunk by its coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override byte[, ,] GetChunk(int x, int y, int z)
        {
            this.waterlevel = WaterLevel;
            heightcache = new byte[this.ChunkSize, this.ChunkSize];
            x = x * this.ChunkSize;
            y = y * this.ChunkSize;
            z = z * this.ChunkSize;
            byte[, ,] chunk = new byte[this.ChunkSize, this.ChunkSize, this.ChunkSize];
            for (int xx = 0; xx < this.ChunkSize; xx++)
            {
                for (int yy = 0; yy < this.ChunkSize; yy++)
                {
                    heightcache[xx, yy] = GetHeight(x + xx, y + yy);
                }
            }
            interpolatednoise = NoiseTools.InterpolateNoise3d(x, y, z, this.ChunkSize);

            // chance of get hay fields
            bool IsHay = _rnd.NextDouble() < 0.005 ? false : true;

            for (int xx = 0; xx < this.ChunkSize; xx++)
            {
                for (int yy = 0; yy < this.ChunkSize; yy++)
                {
                    for (int zz = 0; zz < this.ChunkSize; zz++)
                    {
                        chunk[xx, yy, zz] = IsHay
                        ? (byte)GetBlock(x + xx, y + yy, z + zz, heightcache[xx, yy], 0, xx, yy, zz)
                        : (byte)GetBlock(x + xx, y + yy, z + zz, heightcache[xx, yy], 1, xx, yy, zz);
                    }
                }
            }
            for (int xx = 0; xx < this.ChunkSize; xx++)
            {
                for (int yy = 0; yy < this.ChunkSize; yy++)
                {/*
                    for (int zz = 0; zz < chunksize - 1; zz++)
                    {
                        if (chunk[xx, yy, zz + 1] == TileIdGrass
                            && chunk[xx, yy, zz] == TileIdGrass)
                        {
                            chunk[xx, yy, zz] = (byte)TileIdDirt;
                        }
                    }*/

                    int v = 1;
                    for (int zz = this.ChunkSize - 2; zz >= 0; zz--)
                    {
                        if (chunk[xx, yy, zz] == WorldGeneratorTools.TileIdEmpty) { v = 0; }
                        if (chunk[xx, yy, zz] == WorldGeneratorTools.TileIdGrass)
                        {
                            if (v == 0)
                            {
                            }
                            else if (v < 4)
                            {
                                chunk[xx, yy, zz] = (byte)WorldGeneratorTools.TileIdDirt;
                            }
                            else
                            {
                                chunk[xx, yy, zz] = (byte)WorldGeneratorTools.TileIdStone;
                            }
                            v++;
                        }
                    }

                }
            }
            if (z == 0)
            {
                for (int xx = 0; xx < this.ChunkSize; xx++)
                {
                    for (int yy = 0; yy < this.ChunkSize; yy++)
                    {
                        chunk[xx, yy, 0] = (byte)this.ChunkSize;
                    }
                }
            }
            return chunk;
        }

        /// <summary>
        /// Populates a chunk using the default Manic Digger chunk generation algorithm.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public override void PopulateChunk(IMapStorage map, int x, int y, int z)
        {
            x *= this.ChunkSize;
            y *= this.ChunkSize;
            z *= this.ChunkSize;
            if (!EnableBigTrees)
            {
                PopulationTools.MakeSmallTrees(map, x, y, z, this.ChunkSize, _rnd);
            }
            else
            {
                PopulationTools.MakeTrees(map, x, y, z, this.ChunkSize, _rnd);
            }
            PopulationTools.MakeCaves(map, x, y, z, this.ChunkSize, _rnd, this.EnableCaves, gravellength, goldorelength, ironorelength, coalorelength, dirtlength);
        }

        #endregion

        int waterlevel;

        #region Chunk generation helper methods

        // if param 'special' is equal to 1 then hay fields grows
        private int GetBlock(int x, int y, int z, int height, int special, int xx, int yy, int zz)
        {
            //if (z < 32)
            {
                double d = interpolatednoise[xx, yy, zz];
                //double d = Noise3.noise((double)(x) / 50, (double)(y) / 50, (double)(z) / 50);
                //double d2 = Noise3.noise((double)(x + 1000) / 100, (double)(y + 1000) / 100, (double)(z + 1000) / 100);
                //if ((d2 + maxnoise) > ((double)z / 64) * (maxnoise * 2))
                //{
                //    return TileIdStone;
                //}
                if ((d + maxnoise) > ((double)z / 128) * (maxnoise * 2))
                {
                    return WorldGeneratorTools.TileIdGrass;
                }
                /*
                if (z < height)
                {
                    if (d > -0.25)
                    {
                        return 1;
                    }
                }
                else
                {
                    if (d > 0.25)
                    {
                        return 1;
                    }
                }*/
            }
            return 0;
            int spec = special;
            int tile = WorldGeneratorTools.TileIdStone;

            if (z > waterlevel)
            {
                if (spec == 0)
                {
                    if (z > height) { return WorldGeneratorTools.TileIdEmpty; }
                    if (z == height) { return WorldGeneratorTools.TileIdGrass; }
                }
                else
                {
                    if (z > height + 1) { return WorldGeneratorTools.TileIdEmpty; }
                    if (z == height) { return WorldGeneratorTools.TileIdHay; }
                    if (z == height + 1) { return WorldGeneratorTools.TileIdHay; }
                }
                if (z > height - 5) { return WorldGeneratorTools.TileIdDirt; }
                return WorldGeneratorTools.TileIdStone;
            }
            else
            {
                if (z > height) { return WorldGeneratorTools.TileIdWater; }
                if (z == height) { return WorldGeneratorTools.TileIdSand; }
                return WorldGeneratorTools.TileIdStone;
            }
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
                getnoise += NoiseTools.Noise(((double)x) * frequency / zoom, ((double)y) / zoom * frequency, this.Seed) * amplitude;//This uses our perlin noise functions. It calculates all our zoom and frequency and amplitude
            }
            double maxheight = 64;
            int height = (int)(((getnoise + 1) / 2.0) * (maxheight - 5)) + 3;//(int)((getnoise * 128.0) + 128.0);
            if (height > maxheight - 1) { height = (int)maxheight - 1; }
            if (height < 2) { height = 2; }
            return (byte)height;
        }

        #endregion
    }
}

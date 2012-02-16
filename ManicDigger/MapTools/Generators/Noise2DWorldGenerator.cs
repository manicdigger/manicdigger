#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools.Generators
{
    /// <summary>
    /// Provides a chunk generator that generates chunks using the class Manic Digger (up to 2011-Mar-18) chunk generation algorithm.
    /// </summary>
    public class Noise2DWorldGenerator : WorldGeneratorBase
    {
        #region Fields

        private Random _rnd;
        private byte[,] _heightcache;
        private int goldorelength = 50;
        private int ironorelength = 50;
        private int coalorelength = 50;
        private int gravellength = 50;
        private int silverlength = 50;
        private int dirtlength = 40;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a string representing the display name of this world generator (for GUI purposes only).
        /// </summary>
        public override string DisplayName
        {
            get { return "Classic Manic Digger (Noise 2D)"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DefaultChunkGenerator class.
        /// </summary>
        public Noise2DWorldGenerator()
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
        /// <param name="chunksize"></param>
        /// <returns></returns>
        public override byte[, ,] GetChunk(int x, int y, int z)
        {
            this.waterlevel = this.WaterLevel;
            _heightcache = new byte[this.ChunkSize, this.ChunkSize];
            x = x * this.ChunkSize;
            y = y * this.ChunkSize;
            z = z * this.ChunkSize;
            byte[, ,] chunk = new byte[this.ChunkSize, this.ChunkSize, this.ChunkSize];
            for (int xx = 0; xx < this.ChunkSize; xx++)
            {
                for (int yy = 0; yy < this.ChunkSize; yy++)
                {
                    _heightcache[xx, yy] = GetHeight(x + xx, y + yy);
                }
            }
            // chance of get hay fields
            bool IsHay = _rnd.NextDouble() < 0.005 ? false : true;

            for (int xx = 0; xx < this.ChunkSize; xx++)
            {
                for (int yy = 0; yy < this.ChunkSize; yy++)
                {
                    for (int zz = 0; zz < this.ChunkSize; zz++)
                    {
                        chunk[xx, yy, zz] = IsHay
                        ? (byte)GetBlock(x + xx, y + yy, z + zz, _heightcache[xx, yy], 0)
                        : (byte)GetBlock(x + xx, y + yy, z + zz, _heightcache[xx, yy], 1);
                    }
                }
            }
            if (z == 0)
            {
                for (int xx = 0; xx < this.ChunkSize; xx++)
                {
                    for (int yy = 0; yy < this.ChunkSize; yy++)
                    {
                        chunk[xx, yy, 0] = (byte)WorldGeneratorTools.TileIdLava;
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
                PopulationTools.MakeSmallTrees(map, x, y, z, this.ChunkSize, _rnd, 30);
            }
            else
            {
                PopulationTools.MakeTrees(map, x, y, z, this.ChunkSize, _rnd);
            }
            PopulationTools.MakeCaves(map, x, y, z, this.ChunkSize, _rnd, this.EnableCaves, gravellength, goldorelength, ironorelength, coalorelength, dirtlength, silverlength);
        }

        #endregion

        int waterlevel;

        #region Chunk generation helper methods
    
        /// <summary>
        /// If param 'special' is equal to 1 then hay fields grows
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="height"></param>
        /// <param name="special"></param>
        /// <returns></returns>
        private int GetBlock(int x, int y, int z, int height, int special)
        {
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

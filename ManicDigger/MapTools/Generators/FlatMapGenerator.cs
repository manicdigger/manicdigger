#region Using Statements
using System;

#endregion

namespace ManicDigger.MapTools.Generators
{
    /// <summary>
    /// A simple chunk generator that generates a flat map.
    /// </summary>
    public class FlatMapGenerator : WorldGeneratorBase
    {
        #region Fields

        private byte[,] _heightcache;
        private int waterlevel = 0;
        private int height = 10;
     
        #endregion

        #region Properties

        /// <summary>
        /// Returns a string representing the display name of this world generator (for GUI purposes only).
        /// </summary>
        public override string DisplayName
        {
            get { return "Flatmap without trees"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DefaultChunkGenerator class.
        /// </summary>
        public FlatMapGenerator ()
            : base()
        {
            // nothing to do here...
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
        public override byte[, ,] GetChunk (int x, int y, int z)
        {
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


            for (int xx = 0; xx < this.ChunkSize; xx++)
            {
                for (int yy = 0; yy < this.ChunkSize; yy++)
                {
                    for (int zz = 0; zz < this.ChunkSize; zz++)
                    {
                        chunk[xx, yy, zz] = (byte)GetBlock(x + xx, y + yy, z + zz, _heightcache[xx, yy], 0);
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
        public override void PopulateChunk (IMapStorage map, int x, int y, int z)
        {
            // no trees
        }

        #endregion


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
        private int GetBlock (int x, int y, int z, int height, int special)
        {
            if (z > waterlevel)
            {
                if (z > height)
                {
                    return WorldGeneratorTools.TileIdEmpty;
                }
                if (z == height)
                {
                    return WorldGeneratorTools.TileIdGrass;
                }

                if (z > height - 5)
                {
                    return WorldGeneratorTools.TileIdDirt;
                }
                return WorldGeneratorTools.TileIdStone;
            } else
            {
                if (z > height)
                {
                    return WorldGeneratorTools.TileIdWater;
                }
                if (z == height)
                {
                    return WorldGeneratorTools.TileIdSand;
                }
                return WorldGeneratorTools.TileIdStone;
            }
        }

        private byte GetHeight (int x, int y)
        {
            return (byte)this.height;
        }

        #endregion
    }
}
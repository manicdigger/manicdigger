#region Using Statements
using ManicDigger.Core;
#endregion

namespace ManicDigger.MapTools.Generators
{
    /// <summary>
    /// Provides a convenience implementation of the <see cref="IChunkGenerator"/> interface.
    /// This class is abstract.
    /// </summary>
    public abstract class WorldGeneratorBase : IWorldGenerator
    {
        #region Properties

        /// <summary>
        /// Returns a string representing the display name of this world generator (for GUI purposes only).
        /// </summary>
        public abstract string DisplayName { get; }
        /// <summary>
        /// Gets/sets whether or not to enable the generation of cave systems inside chunks.
        /// </summary>
        public bool EnableCaves
        {
            get
            {
                bool value = (bool)GenerationOptions[GenerationOption.EnableCavesOption, false];
                return value;
            }
            set
            {
                GenerationOptions[GenerationOption.EnableCavesOption] = value;
            }
        }

        /// <summary>
        /// Gets/sets whether or not to enable the generation of big trees.
        /// </summary>
        public bool EnableBigTrees
        {
            get
            {
                bool value = (bool)GenerationOptions[GenerationOption.EnableBigTreesOption, false];
                return value;
            }
            set
            {
                GenerationOptions[GenerationOption.EnableBigTreesOption] = value;
            }
        }

        /// <summary>
        /// Gets/sets the sea level.
        /// </summary>
        public int WaterLevel
        {
            get
            {
                int value = (int)GenerationOptions[GenerationOption.WaterLevelOption, 20];
                return value;
            }
            set
            {
                GenerationOptions[GenerationOption.WaterLevelOption] = value;
            }
        }

        /// <summary>
        /// Gets/sets the density of generated trees.
        /// </summary>
        public float TreeDensity
        {
            get
            {
                float value = (float)GenerationOptions[GenerationOption.TreeDensityOption, 0.008f];
                return value;
            }
            set
            {
                GenerationOptions[GenerationOption.TreeDensityOption] = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ChunkGeneratorBase class.
        /// </summary>
        public WorldGeneratorBase()
        {
            // set default generation options, may be changed later
            GenerationOptions = new SafeDictionary<int, object>();
            GenerationOptions[GenerationOption.EnableBigTreesOption] = false;
            GenerationOptions[GenerationOption.EnableCavesOption] = false;
            GenerationOptions[GenerationOption.WaterLevelOption] = 20;
            GenerationOptions[GenerationOption.TreeDensityOption] = 0.008f;

            // set default chunk size to 32
            // TODO: must be updated when Server.chunksize is changed
            this.ChunkSize = 32;
        }

        #endregion
        
        #region IChunkGenerator Members

        /// <summary>
        /// Gets a dictionary containing options for chunk generation.
        /// </summary>
        public SafeDictionary<int, object> GenerationOptions { get; protected set; }
        /// <summary>
        /// Gets/sets the size of one chunk.
        /// </summary>
        /// <remarks>Affects the way how chunks are populated.</remarks>
        public int ChunkSize { get; set; }
        /// <summary>
        /// Gets the seed that is used by this generator.
        /// </summary>
        public int Seed { get; protected set; }

        /// <summary>
        /// Sets the seed that is used to create chunks.
        /// </summary>
        /// <param name="seed">A long integer representing the seed that is used to create chunks.</param>
        public virtual void SetSeed(int seed)
        {
            Seed = seed;
        }

        /// <summary>
        /// Returns the contents of the chunk by its coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public abstract byte[, ,] GetChunk(int x, int y, int z);
        /// <summary>
        /// Populates the chunk at the given position.
        /// </summary>
        /// <param name="map">The <see cref="IMapStorage"/> instance containing the map.</param>
        /// <param name="x">The x-position of the chunk to populate.</param>
        /// <param name="y">The y-position of the chunk to populate.</param>
        /// <param name="z">The z-position of the chunk to populate.</param>
        public abstract void PopulateChunk(IMapStorage map, int x, int y, int z);

        #endregion
    }
}

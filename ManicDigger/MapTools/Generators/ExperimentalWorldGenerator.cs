#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools.Generators
{
    /// <summary>
    /// Provides a chunk generator that uses an experimental algorithm to create unusual or weird structures.
    /// </summary>
    public class ExperimentalWorldGenerator : WorldGeneratorBase
    {
        #region Fields

        private Noise2DWorldGenerator _default;
        private Random _rnd;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a string representing the display name of this world generator (for GUI purposes only).
        /// </summary>
        public override string DisplayName
        {
            get { return "Experimental"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ExperimentalChunkGenerator class.
        /// </summary>
        public ExperimentalWorldGenerator()
            : base()
        {
            _default = new Noise2DWorldGenerator();
            _rnd = new Random(DateTime.Now.Millisecond);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the seed that is used to create chunks.
        /// </summary>
        /// <param name="seed">A long integer representing the seed that is used to create chunks.</param>
        public override void SetSeed(int seed)
        {
            base.SetSeed(seed);
            _default.SetSeed(seed);
        }

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
            this.UpdateValues();
            return _default.GetChunk(x, y, z);
        }

        /// <summary>
        /// Populates the chunk at the given position.
        /// </summary>
        /// <param name="map">The <see cref="IMapStorage"/> instance containing the map.</param>
        /// <param name="x">The x-position of the chunk to populate.</param>
        /// <param name="y">The y-position of the chunk to populate.</param>
        /// <param name="z">The z-position of the chunk to populate.</param>
        public override void PopulateChunk(IMapStorage map, int x, int y, int z)
        {
            this.UpdateValues();
            _default.PopulateChunk(map, x, y, z);
        }

        private void UpdateValues()
        {
            // update values to the default chunk generator
            _default.GenerationOptions[GenerationOption.EnableBigTreesOption] = this.GenerationOptions[GenerationOption.EnableBigTreesOption];
            _default.GenerationOptions[GenerationOption.EnableCavesOption] = this.GenerationOptions[GenerationOption.EnableCavesOption];
            _default.GenerationOptions[GenerationOption.TreeDensityOption] = this.GenerationOptions[GenerationOption.TreeDensityOption];
            _default.GenerationOptions[GenerationOption.WaterLevelOption] = this.GenerationOptions[GenerationOption.WaterLevelOption];
            _default.ChunkSize = this.ChunkSize;
            // get fresh randomized seed
            this.SetSeed(_rnd.Next());
        }

        #endregion
    }
}

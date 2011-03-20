#region Using Statements
using ManicDigger.Core;
#endregion

namespace ManicDigger.MapTools
{
    /// <summary>
    /// Defines the mechanisms for a type that creates worlds by creating chunks.
    /// </summary>
    public interface IWorldGenerator
    {
        /// <summary>
        /// Returns a string representing the display name of this world generator (for GUI purposes only).
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Gets a dictionary containing options for chunk generation.
        /// </summary>
        SafeDictionary<int, object> GenerationOptions { get; }
        /// <summary>
        /// Gets/sets the size of one chunk.
        /// </summary>
        /// <remarks>Affects the way how chunks are populated.</remarks>
        int ChunkSize { get; set; }
        /// <summary>
        /// Gets the seed that is used by this generator.
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// Sets the seed that is used to create chunks.
        /// </summary>
        /// <param name="seed">A long integer representing the seed that is used to create chunks.</param>
        void SetSeed(int seed);

        /// <summary>
        /// Returns the contents of the chunk by its coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="chunksize"></param>
        /// <returns></returns>
        byte[, ,] GetChunk(int x, int y, int z);
        /// <summary>
        /// Populates the chunk at the given position.
        /// </summary>
        /// <param name="map">The <see cref="IMapStorage"/> instance containing the map.</param>
        /// <param name="x">The x-position of the chunk to populate.</param>
        /// <param name="y">The y-position of the chunk to populate.</param>
        /// <param name="z">The z-position of the chunk to populate.</param>
        /// <param name="chunksize">The chunk's size.</param>
        void PopulateChunk(IMapStorage map, int x, int y, int z);
    }
}

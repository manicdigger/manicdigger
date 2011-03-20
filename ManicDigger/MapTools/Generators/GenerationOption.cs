#region Using Statements
using System;
#endregion

namespace ManicDigger.MapTools.Generators
{
    /// <summary>
    /// Provides access to the basic chunk generation options.
    /// </summary>
    public static class GenerationOption
    {
        /// <summary>
        /// Identifies the "Enable Caves" option, which generates caves inside chunks.
        /// This option value must be of type 'bool'.
        /// </summary>
        public const int EnableCavesOption = 0x0001;
        /// <summary>
        /// Identifies the "Enable Big Trees" option, which generates huge trees.
        /// This option value must be of type 'bool'.
        /// </summary>
        public const int EnableBigTreesOption = 0x0002;
        /// <summary>
        /// Identifies the "Water Level" option, which defines the sea level.
        /// This option value must be of type 'int'.
        /// </summary>
        public const int WaterLevelOption = 0x0003;
        /// <summary>
        /// Identifies the "Tree Density" option, which defines the density of generated trees.
        /// This option value must be of type 'float'.
        /// </summary>
        public const int TreeDensityOption = 0x0004;
    }
}

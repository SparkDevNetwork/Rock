using Rock;

namespace Rock.CodeGeneration.Lava
{
    /// <summary>
    /// Custom lava filters that are added to our Lava engine.
    /// </summary>
    public static class CustomLavaFilters
    {
        /// <summary>
        /// Splits the case so that "EntityType" becomes "Entity Type".
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The string split between capitalized words.</returns>
        public static string SplitCase( string input )
        {
            return input.SplitCase();
        }

        /// <summary>
        /// Converts the string to camel case. Such that "EntityType" becomes "entityType".
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A string that represents the input converted to camel case.</returns>
        public static string CamelCase( string input )
        {
            return input.ToCamelCase();
        }
    }
}

using System.ComponentModel;

namespace Rock.Enums.AI
{
    /// <summary>
    /// The type of name removal to be performed by AI automation (if any).
    /// </summary>
    public enum NameRemoval
    {
        /// <summary>
        /// Do not make any changes in regards to name removal.
        /// </summary>
        NoChanges = 0,

        /// <summary>
        /// Remove only last names (if present).
        /// </summary>
        LastNamesOnly = 1,

        /// <summary>
        /// Remove first and last names (if present).
        /// </summary>
        [Description( "First and Last Names" )]
        FirstAndLastNames = 2
    }
}

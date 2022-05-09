namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Represents a single TypeScript import that will need to be added
    /// to the top of the TypeScript file.
    /// </summary>
    public class TypeScriptImport
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name to use for the default import.
        /// </summary>
        /// <value>The name of the default import or <c>null</c> if default import isn't used.</value>
        public string DefaultImport { get; set; }

        /// <summary>
        /// Gets or sets the named import to use.
        /// </summary>
        /// <value>The named import to use or <c>null</c> if named import isn't used.</value>
        public string NamedImport { get; set; }

        /// <summary>
        /// Gets or sets the source path to import from.
        /// </summary>
        /// <value>The source path to import from.</value>
        public string SourcePath { get; set; }

        #endregion
    }
}

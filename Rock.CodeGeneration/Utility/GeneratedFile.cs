namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Representation of a generated file that is waiting to be written to disk.
    /// </summary>
    public class GeneratedFile
    {
        #region Properties

        /// <summary>
        /// Gets the name of the file without any path information.
        /// </summary>
        /// <value>The name of the file without any path information.</value>
        public string FileName { get; }

        /// <summary>
        /// Gets the path relative to the solution root.
        /// </summary>
        /// <value>The path relative to the solution root.</value>
        public string SolutionRelativePath { get; }

        /// <summary>
        /// Gets the content of the file.
        /// </summary>
        /// <value>The content of the file.</value>
        public string Content { get; }

        /// <summary>
        /// Indicates the state of this file in regards to being saved or not.
        /// </summary>
        /// <value>The save state of the generated file.</value>
        public GeneratedFileSaveState SaveState { get; set; } = GeneratedFileSaveState.NotProcessed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedFile"/> class.
        /// </summary>
        /// <param name="filename">The filename without any path information.</param>
        /// <param name="relativeDirectory">The relative directory to the solution root.</param>
        /// <param name="content">The content to put in the file.</param>
        public GeneratedFile( string filename, string relativeDirectory, string content )
        {
            FileName = filename;
            SolutionRelativePath = $"{relativeDirectory.Trim( '/', '\\' )}\\{filename}";
            Content = content;
        }

        #endregion
    }
}

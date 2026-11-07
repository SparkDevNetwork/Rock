namespace Rock.ViewModels.Blocks.Chat.Disagrees
{
    /// <summary>
    /// A bag with a member its declaration lacks, and a declaration with a member it lacks.
    /// </summary>
    public class DisagreesBag
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the nick name, which the declaration does not have.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets the full name, an expression-bodied member the declaration does not have.
        /// </summary>
        public string FullName => Name;

        /// <summary>
        /// Gets the default name, a static member the declaration does not have.
        /// </summary>
        public static string DefaultName { get; } = "none";
    }
}

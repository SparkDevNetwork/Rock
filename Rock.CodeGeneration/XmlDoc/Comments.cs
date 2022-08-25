namespace Rock.CodeGeneration.XmlDoc
{
    /// <summary>
    /// The comments that were loaded for a given type or member.
    /// </summary>
    public class Comments
    {
        /// <summary>
        /// Gets or sets the summary comment content.
        /// </summary>
        /// <value>The summary comment content.</value>
        public Comment Summary { get; set; }

        /// <summary>
        /// Gets or sets the value comment content.
        /// </summary>
        /// <value>The value comment content.</value>
        public Comment Value { get; set; }

        /// <summary>
        /// Gets or sets the inherit from cref value.
        /// </summary>
        /// <value>The inherit from cref value.</value>
        public string InheritFrom { get; set; }
    }
}

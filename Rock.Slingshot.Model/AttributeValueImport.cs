namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{Value}" )]
    public class AttributeValueImport
    {
        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }
}

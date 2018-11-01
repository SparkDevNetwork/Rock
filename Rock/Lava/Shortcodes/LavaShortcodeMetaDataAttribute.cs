namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// A custom attribute for defining the shortcode documentation that will show
    /// up in the shortcode admin blocks.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class LavaShortcodeMetadataAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the shortcode.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the documentation for the shortcode.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        public string Documentation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameters available to the shortcode.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string Parameters { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the enabled commands inside of the Lava.
        /// </summary>
        /// <value>
        /// The enabled commands.
        /// </value>
        public string EnabledCommands { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaShortcodeMetadataAttribute"/> class.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="description">The description.</param>
        /// <param name="documentation">The documentation.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="enabledCommands">The enabled commands.</param>
        public LavaShortcodeMetadataAttribute( string name, string tagName, string description, string documentation, string parameters, string enabledCommands )
        {
            this.Name = name;
            this.TagName = tagName;
            this.Description = description;
            this.Documentation = documentation;
            this.Parameters = parameters;
            this.EnabledCommands = enabledCommands;
        }
    }
}

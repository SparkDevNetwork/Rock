using System.Collections.Generic;

using Rock.Extension;

namespace Rock.Obsidian
{
    /// <summary>
    /// The base class for a <see cref="Component"/> whose functionality is implemented in Obsidian.
    /// </summary>
    public abstract class ObsidianComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults { get; } = new Dictionary<string, string>
        {
            { "Active", "True" },
            { "Order", "0" }
        };

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive => true;

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order => 0;

        /// <summary>
        /// Gets or sets the path to the Obsidian component's .obs file.
        /// </summary>
        public virtual string ComponentUrl { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianComponent"/> class.
        /// </summary>
        /// <param name="componentFilePath">The path to the Obsidian component's .obs file.</param>
        public ObsidianComponent(string componentFilePath)
        {
            ComponentUrl = componentFilePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianComponent"/> class.
        /// </summary>
        public ObsidianComponent(): base()
        {

        }

        #endregion
    }
}

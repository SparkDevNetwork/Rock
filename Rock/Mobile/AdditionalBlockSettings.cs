namespace Rock.Mobile
{
    /// <summary>
    /// Stores additional block settings for use with Mobile blocks.
    /// </summary>
    public class AdditionalBlockSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether show on tablets.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be shown on tablets; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnTablet { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show on phones.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be shown on phones; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnPhone { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether network is required for this block to display.
        /// </summary>
        /// <value>
        ///   <c>true</c> if network is required for this block to display; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresNetwork { get; set; }

        /// <summary>
        /// Gets or sets the content when there is no network.
        /// </summary>
        /// <value>
        /// The content when there is no network.
        /// </value>
        public string NoNetworkContent { get; set; }
    }
}

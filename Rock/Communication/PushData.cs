using System.Collections.Generic;

namespace Rock.Communication
{
    /// <summary>
    /// This class is used to hold the Rock specific settings in for Rock Push Message.
    /// </summary>
    public class PushData
    {
        /// <summary>
        /// Gets or sets the mobile page identifier.
        /// </summary>
        /// <value>
        /// The mobile page identifier.
        /// </value>
        public int? MobilePageId { get; set; }
        /// <summary>
        /// Gets or sets the mobile page query string.
        /// </summary>
        /// <value>
        /// The mobile page query string.
        /// </value>
        public Dictionary<string, string> MobilePageQueryString { get; set; }
        /// <summary>
        /// Gets or sets the mobile application identifier.
        /// </summary>
        /// <value>
        /// The mobile application identifier.
        /// </value>
        public int? MobileApplicationId { get; set; }
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }
    }
}

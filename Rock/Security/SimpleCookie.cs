using System;

namespace Rock.Security
{
    /// <summary>
    /// Like System.Web.HttpCookie, but with no reference to System Web for Obsidian.
    /// </summary>
    public class SimpleCookie
    {
        /// <summary>
        /// Gets or sets the expires.
        /// </summary>
        /// <value>
        /// The expires.
        /// </value>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }
}

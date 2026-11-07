using System.Collections.Generic;

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// A base box, as BlockBox is: its members reach every box built on it.
    /// </summary>
    public class FixtureBox : Internal.IFixtureBox
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the navigation URLs.
        /// </summary>
        public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the summary, which a box built on this may override.
        /// </summary>
        public virtual string Summary { get; set; }

        /// <summary>
        /// Gets the version. A base class's static member is not read for a class built on it.
        /// </summary>
        public static int Version { get; } = 1;
    }
}

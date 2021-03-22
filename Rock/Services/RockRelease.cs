using System;

namespace Rock.Services
{
    /// <summary>
    /// Represents the bits the Rock system stores regarding a particular release.
    /// </summary>
    [Serializable]
    public class RockRelease
    {
        /// <summary>
        /// Gets or sets the semantic version.
        /// </summary>
        /// <value>
        /// The semantic version.
        /// </value>
        public string SemanticVersion { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the release date.
        /// </summary>
        /// <value>
        /// The release date.
        /// </value>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires early access].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires early access]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresEarlyAccess { get; set; }

        /// <summary>
        /// Gets or sets the requires version.
        /// </summary>
        /// <value>
        /// The requires version.
        /// </value>
        public string RequiresVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is production.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is production; otherwise, <c>false</c>.
        /// </value>
        public bool IsProduction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is beta.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is beta; otherwise, <c>false</c>.
        /// </value>
        public bool IsBeta { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is alpha.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is alpha; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlpha { get; set; }

        /// <summary>
        /// Gets or sets the package URI.
        /// </summary>
        /// <value>
        /// The package URI.
        /// </value>
        public string PackageUri { get; set; }

        /// <summary>
        /// Gets or sets the release notes.
        /// </summary>
        /// <value>
        /// The release notes.
        /// </value>
        public string ReleaseNotes { get; set; }
    }
}

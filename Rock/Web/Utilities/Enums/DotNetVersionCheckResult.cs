namespace Rock.Web.Utilities
{
    /// <summary>
    /// Represents the possible results of a version check.
    /// </summary>
    public enum DotNetVersionCheckResult
    {
        /// <summary>
        /// The version check definitely fails
        /// </summary>
        Fail = 0,

        /// <summary>
        /// This version check definitely passes
        /// </summary>
        Pass = 1,

        /// <summary>
        /// The version check could not determine pass or fail so proceed at own risk.
        /// </summary>
        Unknown = 2
    }
}

using Rock.Attribute;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// A matching person result.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public class MatchingPersonResult
    {
        /// <summary>
        /// The encrypted matching person state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The full name of the matching person.
        /// </summary>
        public string FullName { get; set; }
    }
}

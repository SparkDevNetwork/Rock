using System.Collections.Generic;
using Rock.Attribute;

namespace Rock.Security.Authentication.ExternalRedirectAuthentication
{
    /// <summary>
    /// Represents options to perform external redirect authentication.
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
    public class ExternalRedirectAuthenticationOptions
    {
        /// <summary>
        /// The callback URL for the external auth provider to return to to complete authentication.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the request parameters.
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }
    }
}

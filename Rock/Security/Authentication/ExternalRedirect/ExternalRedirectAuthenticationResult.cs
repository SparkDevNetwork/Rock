using Rock.Attribute;

namespace Rock.Security.Authentication.ExternalRedirectAuthentication
{
    /// <summary>
    /// Represents a result of performing external redirect authentication.
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
    public class ExternalRedirectAuthenticationResult
    {
        /// <summary>
        /// Indicates whether the user is authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the name of the authenticated user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the post-authentication return URL.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}

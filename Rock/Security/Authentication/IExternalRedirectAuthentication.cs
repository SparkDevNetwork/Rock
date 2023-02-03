using System;
using System.Collections.Generic;
using Rock.Attribute;
using Rock.Security.Authentication.ExternalRedirectAuthentication;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Represents an external redirect authentication provider.
    /// </summary>
    /// <remarks>
    ///     Can be used for both OAuth1- and OAuth2-based authentication.
    ///     For OAuth1-based authentication, fetch the request token in the <see cref="GenerateExternalLoginUrl(string, string)"/> method.
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public interface IExternalRedirectAuthentication
    {
        /// <summary>
        /// Authenticates the user using an external redirect authentication provider.
        /// </summary>
        /// <param name="options">The authentication options.</param>
        /// <returns>The external redirect authentication result.</returns>
        ExternalRedirectAuthenticationResult Authenticate( ExternalRedirectAuthenticationOptions options );

        /// <summary>
        /// Generates the log in URL.
        /// </summary>
        /// <param name="externalProviderReturnUrl">The callback URL for external auth provider to return to to complete authentication.</param>
        /// <param name="successfulAuthenticationRedirectUrl">The URL to redirect to after completing authentication.</param>
        /// <returns></returns>
        Uri GenerateExternalLoginUrl( string externalProviderReturnUrl, string successfulAuthenticationRedirectUrl );

        /// <summary>
        /// Determines if authentication should proceed with this authentication provider based on the request parameters.
        /// </summary>
        /// <param name="parameters">The request parameters.</param>
        /// <returns><c>true</c> if authentication should proceed with this authentication provider; otherwise, <c>false</c> is returned.</returns>
        bool IsReturningFromExternalAuthentication( IDictionary<string, string> parameters );
    }
}

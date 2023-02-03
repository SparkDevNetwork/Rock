using Rock.Attribute;
using Rock.Model;
using Rock.Security.Authentication.CredentialAuthentication;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Represents a password-based authentication provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public interface ICredentialAuthentication
    {
        /// <summary>
        /// Authenticates the user based on user name and password.
        /// </summary>
        /// <param name="options">The authentication options.</param>
        /// <returns><c>true</c> if authenticated; otherwise, <c>false</c> is returned.</returns>
        bool Authenticate( CredentialAuthenticationOptions options );

        /// <summary>
        /// Encrypts the <paramref name="password"/>.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encrypted password.</returns>
        string EncryptPassword( string password );
    }
}

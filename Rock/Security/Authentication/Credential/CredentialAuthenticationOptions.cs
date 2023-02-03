using Rock.Attribute;
using Rock.Model;

namespace Rock.Security.Authentication.CredentialAuthentication
{
    /// <summary>
    /// Represents options to perform credential authentication.
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
    public class CredentialAuthenticationOptions
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public UserLogin User { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }
    }
}

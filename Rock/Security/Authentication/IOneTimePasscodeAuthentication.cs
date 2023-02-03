using Rock.Attribute;
using Rock.Data;
using Rock.Security.Authentication.OneTimePasscode;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Represents a one-time passcode authentication provider.
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
    public interface IOneTimePasscodeAuthentication
    {
        /// <summary>
        /// Authenticates the user using a one-time passcode authentication provider.
        /// </summary>
        /// <param name="options">The authentication options.</param>
        /// <returns>The one-time passcode authentication result.</returns>
        OneTimePasscodeAuthenticationResult Authenticate( OneTimePasscodeAuthenticationOptions options );

        /// <summary>
        /// Sends a one time passcode (OTP) via email or SMS.
        /// </summary>
        /// <param name="sendOneTimePasscodeOptions">The OTP options.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>A result containing the encrypted passwordless state, if successful.</returns>
        SendOneTimePasscodeResult SendOneTimePasscode( SendOneTimePasscodeOptions sendOneTimePasscodeOptions, RockContext rockContext );
    }
}

using Rock.Attribute;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// Represents options to perform one-time passcode authentication.
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
    public class OneTimePasscodeAuthenticationOptions
    {
        /// <summary>
        /// The encrypted state that was generated when the OTP was sent.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The encrypted matching person state that was generated when multiple people matched the OTP email/phone number.
        /// </summary>
        public string MatchingPersonValue { get; set; }

        /// <summary>
        /// The one-time passcode to verify.
        /// </summary>
        public string Code { get; set; }
    }
}

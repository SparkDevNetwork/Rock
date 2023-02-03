using System.Collections.Generic;
using Rock.Attribute;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// Represents a result of performing one-time passcode authentication.
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
    public class OneTimePasscodeAuthenticationResult
    {
        /// <summary>
        /// Indicates whether the user is authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; internal set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether account registration is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if account registration is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegistrationRequired { get; set; }

        /// <summary>
        /// Indicates whether person selection is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if person selection is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonSelectionRequired { get; set; }

        /// <summary>
        /// The people matching the email or phone number.
        /// </summary>
        /// <remarks>Only set when multiple matches are found.</remarks>
        public List<MatchingPersonResult> MatchingPeopleResults { get; set; }

        /// <summary>
        /// Gets or sets the registration URL.
        /// </summary>
        public string RegistrationUrl { get; set; }

        /// <summary>
        /// The encrypted state that was generated when the OTP was sent.
        /// </summary>
        public string State { get; set; }
    }
}

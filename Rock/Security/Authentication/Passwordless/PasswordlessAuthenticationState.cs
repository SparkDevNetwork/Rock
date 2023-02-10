using System;
using Rock.Attribute;

namespace Rock.Security.Authentication.Passwordless
{
    /// <summary>
    /// State sent to client during passwordless login
    /// to ensure no tampering takes place.
    /// </summary>
    /// <remarks>
    ///     Should be encrypted when sending.
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public class PasswordlessAuthenticationState
    {
        /// <summary>
        /// The unique identifier for the remote authentication session.
        /// </summary>
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// The email used to start the remote authentication session.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The phone number used to start the remote authentication session.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The one-time passcode.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The one-time passcode issue date.
        /// </summary>
        public DateTime CodeIssueDate { get; set; }

        /// <summary>
        /// The one-time passcode lifetime.
        /// </summary>
        public TimeSpan CodeLifetime { get; set; }
    }
}

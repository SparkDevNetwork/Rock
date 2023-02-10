using Rock.Attribute;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// The result from sending a one-time passcode.
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
    public class SendOneTimePasscodeResult
    {
        /// <summary>
        /// Indicates whether the passwordless login start step was successful.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the passwordless login start step was successful; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// The error message if the passwordless login start step was unsuccessful.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The auto-generated state value that should be sent during the passwordless login verify step.
        /// </summary>
        public string State { get; set; }
    }
}

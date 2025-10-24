using System.Collections.Generic;

namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Represents the options used to verify a proof-of-work captcha challenge.
    /// </summary>
    internal class CaptchaVerifyProofOfWorkOptions
    {
        /// <summary>
        /// Gets or sets the collection of client-calculated challenge solutions.
        /// </summary>
        internal List<int> ChallengeSolutions { get; set; }
        
        /// <summary>
        /// Gets or sets the challenge token provided by the client.
        /// </summary>
        internal string ChallengeToken { get; set; }
    }
}

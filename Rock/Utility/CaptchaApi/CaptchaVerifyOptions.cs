namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Represents the options required to solve a CAPTCHA challenge and obtain a CAPTCHA token.
    /// </summary>
    internal class CaptchaVerifyOptions
    {
        /// <summary>
        /// Gets or sets the options for configuring proof-of-work verification in the captcha process.
        /// </summary>
        internal CaptchaVerifyProofOfWorkOptions PowOptions { get; set; }
    }
}

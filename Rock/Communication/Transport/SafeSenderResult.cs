using System.Net.Mail;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// 
    /// </summary>
    public class SafeSenderResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is unsafe domain.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unsafe domain; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnsafeDomain { get; set; }

        /// <summary>
        /// Gets or sets the safe from address.
        /// </summary>
        /// <value>
        /// The safe from address.
        /// </value>
        public MailAddress SafeFromAddress { get; set; }

        /// <summary>
        /// Gets or sets the reply to address.
        /// </summary>
        /// <value>
        /// The reply to address.
        /// </value>
        public MailAddress ReplyToAddress { get; set; }
    }
}

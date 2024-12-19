using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Message Bag for SMS Conversations
    /// </summary>
    public class MessageBag
    {
        /// <summary>
        /// Gets or sets the message key.
        /// </summary>
        public string MessageKey { get; set; }

        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        public string SMSMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was sent from Rock, vs a mobile device
        /// </summary>
        public bool IsOutbound { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person that sent the message from Rock.
        /// </summary>
        public string OutboundSenderFullName { get; set; }

        /// <summary>
        /// Gets or sets the created date time offset.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a list of Attachment URL's
        /// </summary>
        public List<string> AttachmentUrls { get; set; }
    }
}

using System;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Send Message Bag for SMS Conversations
    /// </summary>
    public class SendMessageBag
    {
        /// <summary>
        /// Gets or sets the person alias IdKey for the Recipient.
        /// </summary>
        public string RecipientPersonAliasIdKey { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Attachment Guid
        /// </summary>
        public Guid? AttachmentGuid { get; set; }
    }
}

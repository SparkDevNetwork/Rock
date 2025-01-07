using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Response Listing Status Bag for SMS Conversations
    /// </summary>
    public class ResponseListingStatusBag
    {
        /// <summary>
        /// Gets or sets the list of conversations.
        /// </summary>
        public List<ConversationBag> Conversations { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string ErrorMessage { get; set; }

    }
}

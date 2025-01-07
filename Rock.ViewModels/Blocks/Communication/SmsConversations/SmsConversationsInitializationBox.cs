using System.Collections.Generic;

using Rock.Enums.Communication;
using Rock.ViewModels.Blocks.Core.Notes;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The SMS Conversations Initialization Box
    /// </summary>
    public class SmsConversationsInitializationBox
    {
        /// <summary>
        /// Gets or sets the list of System Phone Numbers
        /// </summary>
        public List<ListItemBag> SystemPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the list of Conversations
        /// </summary>
        public List<ConversationBag> Conversations { get; set; }

        /// <summary>
        /// Gets or sets the filter used to filter communication messages.
        /// </summary>
        public CommunicationMessageFilter MessageFilter { get; set; }

        /// <summary>
        /// Gets or sets the list of available note types.
        /// </summary>
        public List<NoteTypeBag> NoteTypes { get; set; }

        /// <summary>
        /// Gets or sets the list of available snippets.
        /// </summary>
        public List<SnippetBag> Snippets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "New Message" button is visible.
        /// </summary>
        public bool IsNewMessageButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

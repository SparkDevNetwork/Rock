using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Conversation Bag for SMS Conversations
    /// </summary>
    public class ConversationBag
    {
        /// <summary>
        /// Gets or sets the conversation key. All messages that are part of the
        /// same conversation will share a common ConversationKey.
        /// </summary>
        public string ConversationKey { get; set; }

        /// <summary>
        /// Gets or sets the person alias IdKey for the Recipient.
        /// </summary>
        public string RecipientPersonAliasIdKey { get; set; }

        /// <summary>
        /// Gets or sets the person alias Guid for the Recipient.
        /// </summary>
        public Guid? RecipientPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the person Guid for the Recipient.
        /// </summary>
        public Guid? RecipientPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the person alias Id for the Recipient. This is temporary and will
        /// be removed once the ReminderList Block is converted to Obsidian. This should
        /// only be used when linking to the Webforms Reminder List Block.
        /// </summary>
        public int RecipientPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Recipient's Phone Number.
        /// </summary>
        public string RecipientPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the boolean value determining if the conversation has been read.
        /// </summary>
        public bool IsConversationRead { get; set; }

        /// <summary>
        /// Gets or sets the PhotoUrl of the Recipient.
        /// </summary>
        public string RecipientPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the description of the selected Recipient.
        /// </summary>
        public string RecipientDescription { get; set; }

        /// <summary>
        /// Gets or sets whether the Recipient is a Nameless Person.
        /// </summary>
        public bool? IsRecipientNamelessPerson { get; set; }

        /// <summary>
        /// Gets or sets the full name. This represents the individual outside
        /// of Rock that is being communicated with.
        /// </summary>
        public string RecipientFullName { get; set; }

        /// <summary>
        /// Gets or sets the list of Recipient Responses
        /// </summary>
        public List<MessageBag> Messages { get; set; }

        /// <summary>
        /// Gets or sets the GUID for the entity type associated with the reminder.
        /// </summary>
        public Guid? EntityTypeGuidForReminder { get; set; }

        /// <summary>
        /// Gets or sets the GUID for the entity associated with the reminder.
        /// </summary>
        public Guid? EntityGuidForReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the link to the person is visible.
        /// </summary>
        public bool? IsLinkToPersonVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the view merge request option is visible.
        /// </summary>
        public bool? IsViewMergeRequestVisible { get; set; }
    }
}

using System;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag used to maintain state of recipients.
    /// </summary>
    public class CommunicationEntryRecipientBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the person associated with this recipient.
        /// </summary>
        /// <value>
        /// The unique identifier of the person associated with this recipient.
        /// </value>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of this recipient.
        /// </summary>
        /// <value>
        /// The name of this recipient.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this recipient is deceased.
        /// </summary>
        /// <value>
        /// <c>true</c> if this recipient is deceased; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gets or sets the SMS number for this recipient.
        /// </summary>
        /// <value>
        /// The SMS number for this recipient.
        /// </value>
        public string SmsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS is allowed for this recipient.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SMS is allowed for this recipient; otherwise, <c>false</c>.
        /// </value>
        public bool IsSmsAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether push notifications are allowed for this recipient.
        /// </summary>
        /// <value>
        ///   <c>true</c> if push notifications are allowed for this recipient; otherwise, <c>false</c>.
        /// </value>
        public bool IsPushAllowed { get; set; }

        /// <summary>
        /// Gets or sets email of this recipient.
        /// </summary>
        /// <value>
        /// The email of this recipient.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the photo URL of the recipient.
        /// </summary>
        /// <value>
        /// The photo URL of the recipient.
        /// </value>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether email is allowed for this recipient.
        /// </summary>
        /// <value>
        ///   <c>true</c> if email is allowed for this recipient; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmailAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether bulk email is allowed for this recipient.
        /// </summary>
        /// <value>
        ///   <c>true</c> if bulk email is allowed for this recipient; otherwise, <c>false</c>.
        /// </value>
        public bool IsBulkEmailAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether email is active for this recipient.
        /// </summary>
        /// <value>
        ///   <c>true</c> if email is active for this recipient; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets the email preference for this recipient.
        /// </summary>
        /// <value>
        /// The email preference for this recipient.
        /// </value>
        public string EmailPreference { get; set; }
    }
}

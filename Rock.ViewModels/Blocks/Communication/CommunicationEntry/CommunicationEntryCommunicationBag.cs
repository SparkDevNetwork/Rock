using System;
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the communication information for the Communication Entry block.
    /// </summary>
    public class CommunicationEntryCommunicationBag
    {
        /// <summary>
        /// Gets or sets the communication unique identifier.
        /// </summary>
        /// <value>
        /// The communication unique identifier.
        /// </value>
        public Guid CommunicationGuid { get; set; }

        /// <summary>
        /// Gets or sets the medium entity type unique identifier.
        /// </summary>
        /// <value>
        /// The medium entity type unique identifier.
        /// </value>
        public Guid MediumEntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the communication template unique identifier.
        /// </summary>
        /// <value>
        /// The communication template unique identifier.
        /// </value>
        public Guid? CommunicationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is bulk communication.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is bulk communication; otherwise, <c>false</c>.
        /// </value>
        public bool IsBulkCommunication { get; set; }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        public List<CommunicationEntryRecipientBag> Recipients { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>
        /// The reply to email.
        /// </value>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the cc emails.
        /// </summary>
        /// <value>
        /// The cc emails.
        /// </value>
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        /// <value>
        /// The BCC emails.
        /// </value>
        public string BCCEmails { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        public string MessageMetaData { get; set; }

        /// <summary>
        /// Gets or sets the push title.
        /// </summary>
        /// <value>
        /// The push title.
        /// </value>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the push message.
        /// </summary>
        /// <value>
        /// The push message.
        /// </value>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the push sound.
        /// </summary>
        /// <value>
        /// The push sound.
        /// </value>
        public string PushSound { get; set; }

        /// <summary>
        /// Gets or sets the push data.
        /// </summary>
        /// <value>
        /// The push data.
        /// </value>
        public string PushData { get; set; }

        /// <summary>
        /// Gets or sets the push image binary file identifier.
        /// </summary>
        /// <value>
        /// The push image binary file identifier.
        /// </value>
        public int? PushImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        /// <value>
        /// The push open action.
        /// </value>
        public PushOpenActionType? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        /// <value>
        /// The push open message.
        /// </value>
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the SMS from system phone number identifier.
        /// </summary>
        /// <value>
        /// The SMS from system phone number identifier.
        /// </value>
        public int? SmsFromSystemPhoneNumberId { get; set; }

        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        /// <value>
        /// The SMS message.
        /// </value>
        public string SMSMessage { get; set; }

        /// <summary>
        /// Gets or sets the email attachment binary file ids.
        /// </summary>
        /// <value>
        /// The email attachment binary file ids.
        /// </value>
        public List<int> EmailAttachmentBinaryFileIds { get; set; }

        /// <summary>
        /// Gets or sets the future send date time.
        /// </summary>
        /// <value>
        /// The future send date time.
        /// </value>
        public DateTimeOffset? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the SMS from defined value identifier.
        /// </summary>
        /// <value>
        /// The SMS from defined value identifier.
        /// </value>
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachment binary file ids.
        /// </summary>
        /// <value>
        /// The SMS attachment binary file ids.
        /// </value>
        public List<int> SMSAttachmentBinaryFileIds { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public CommunicationStatus Status { get; set; }
    }
}

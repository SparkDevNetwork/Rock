// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the communication information for the Communication Entry block.
    /// </summary>
    public class CommunicationEntryCommunicationBag
    {
        /// <summary>
        /// Internal for server-side processing only.
        /// </summary>
        internal int CommunicationId { get; set; }

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
        /// Gets or sets from email address.
        /// </summary>
        /// <value>
        /// From email address.
        /// </value>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the reply to email address.
        /// </summary>
        /// <value>
        /// The reply to email address.
        /// </value>
        public string ReplyAddress { get; set; }

        /// <summary>
        /// Gets or sets the cc email addresses.
        /// </summary>
        /// <value>
        /// The cc email addresses.
        /// </value>
        public string CCAddresses { get; set; }

        /// <summary>
        /// Gets or sets the BCC email addresses.
        /// </summary>
        /// <value>
        /// The BCC email addresses.
        /// </value>
        public string BCCAddresses { get; set; }

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
        public CommunicationEntryPushNotificationOptionsBag PushData { get; set; }

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
        /// Gets or sets the push open message structured content JSON.
        /// </summary>
        /// <value>
        /// The push open message structured content JSON.
        /// </value>
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the SMS from system phone number unique identifier.
        /// </summary>
        /// <value>
        /// The SMS from system phone number unique identifier.
        /// </value>
        public Guid? SmsFromSystemPhoneNumberGuid { get; set; }

        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        /// <value>
        /// The SMS message.
        /// </value>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the email attachment binary files.
        /// </summary>
        /// <value>
        /// The email attachment binary files.
        /// </value>
        public List<ListItemBag> EmailAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachment binary files.
        /// </summary>
        /// <value>
        /// The SMS attachment binary files.
        /// </value>
        public List<ListItemBag> SmsAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the future send date time.
        /// </summary>
        /// <value>
        /// The future send date time.
        /// </value>
        public DateTimeOffset? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the additional email addresses to receive the communication.
        /// </summary>
        public List<string> AdditionalEmailAddresses { get; set; }
    }
}

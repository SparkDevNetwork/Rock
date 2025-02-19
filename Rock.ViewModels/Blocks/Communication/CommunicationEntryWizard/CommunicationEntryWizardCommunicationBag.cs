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

using System;
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntryWizard;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing the communication information needed for the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardCommunicationBag
    {
        /// <summary>
        /// Gets or sets the communication identifier.
        /// </summary>
        public int? CommunicationId { get; set; }
        
        /// <summary>
        /// Gets or sets the communication unique identifier.
        /// </summary>
        public Guid CommunicationGuid { get; set; }
        
        /// <summary>
        /// Gets or sets the communication name.
        /// </summary>
        public string CommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the communication type.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is a bulk communication.
        /// </summary>
        public bool IsBulkCommunication { get; set; }

        /// <summary>
        /// Gets or sets the communication subject.
        /// </summary>
        public string Subject { get; set; }
        
        /// <summary>
        /// Gets or sets the email message.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the sender name.
        /// </summary>
        public string FromName { get; set; }
        
        /// <summary>
        /// Gets or sets the sender email address.
        /// </summary>
        public string FromEmail { get; set; }
        
        /// <summary>
        /// Gets or sets the reply-to email address.
        /// </summary>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the future send date time.
        /// </summary>
        /// <value>
        /// The future send date time.
        /// </value>
        public DateTimeOffset? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the individual recipient person alias unique identifiers.
        /// </summary>
        public List<Guid> IndividualRecipientPersonAliasGuids { get; set; }

        /// <summary>
        /// Gets or sets the communication list group unique identifier.
        /// </summary>
        public Guid? CommunicationListGroupGuid { get; set; }
        
        /// <summary>
        /// Gets or sets the segment criterion.
        /// </summary>
        public SegmentCriteria SegmentCriteria { get; set; }
        
        /// <summary>
        /// Gets or sets the segment data view unique identifiers.
        /// </summary>
        public List<Guid> SegmentDataViewGuids { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude duplicate recipient addresses from receiving this communication.
        /// </summary>
        public bool ExcludeDuplicateRecipientAddress { get; set; }

        /// <summary>
        /// Gets or sets the communication template unique identifier.
        /// </summary>
        public Guid? CommunicationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the CC email addresses.
        /// </summary>
        public string CcEmails { get; set; }
        
        /// <summary>
        /// Gets or sets the BCC email addresses.
        /// </summary>
        public string BccEmails { get; set; }
        
        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        public string SmsMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the enable Lava commands.
        /// </summary>
        public string EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets the communication status.
        /// </summary>
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the SMS from system phone number unique identifier.
        /// </summary>
        public Guid? SmsFromSystemPhoneNumberGuid { get; set; }

        /// <summary>
        /// Gets or sets the SMS phone number for testing the communication.
        /// </summary>
        public string TestSmsPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address for testing the communication.
        /// </summary>
        public string TestEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the email attachments.
        /// </summary>
        public List<ListItemBag> EmailAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachments.
        /// </summary>
        public List<ListItemBag> SmsAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the push data.
        /// </summary>
        public CommunicationEntryWizardPushNotificationOptionsBag PushData { get; set; }

        /// <summary>
        /// Gets or sets the push image binary file unique identifier.
        /// </summary>
        public Guid? PushImageBinaryFileGuid { get; set; }

        /// <summary>
        /// Gets or sets the push message.
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the push title.
        /// </summary>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open message JSON.
        /// </summary>
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        public PushOpenAction? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the communication topic defined value.
        /// </summary>
        public ListItemBag CommunicationTopicValue { get; set; }
    }
}
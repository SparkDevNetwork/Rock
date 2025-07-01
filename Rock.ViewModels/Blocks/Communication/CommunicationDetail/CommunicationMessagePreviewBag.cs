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

using Rock.Enums.Blocks.Communication.CommunicationDetail;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information needed to preview a communication's message within the communication detail block.
    /// </summary>
    public class CommunicationMessagePreviewBag
    {
        /// <summary>
        /// Gets or sets the name of the person who created this communication.
        /// </summary>
        public string CreatedByPersonName { get; set; }

        /// <summary>
        /// Gets or sets the datetime this communication was created.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who approved this communication.
        /// </summary>
        public string ApprovedByPersonName { get; set; }

        /// <summary>
        /// Gets or sets the datetime this communication was approved.
        /// </summary>
        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the person this communication is being sent from.
        /// </summary>
        public string FromPersonName { get; set; }

        /// <summary>
        /// Gets or sets the email address this communication is being sent from.
        /// </summary>
        public string FromPersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the email address to which replies to this communication should be sent.
        /// </summary>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the CC email addresses.
        /// </summary>
        public string CcEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC email addresses.
        /// </summary>
        public string BccEmails { get; set; }

        /// <summary>
        /// Gets or sets the subject for this communication.
        /// </summary>
        public string EmailSubject { get; set; }

        /// <summary>
        /// Gets or sets the HTML email message for this communication.
        /// </summary>
        public string EmailMessage { get; set; }

        /// <summary>
        /// Gets or sets the email attachment binary files.
        /// </summary>
        public List<ListItemBag> EmailAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the name of the system phone number this communication is being sent from.
        /// </summary>
        public string FromSystemPhoneNumberName { get; set; }

        /// <summary>
        /// Gets or sets the system phone number this communication is being sent from.
        /// </summary>
        public string FromSystemPhoneNumber { get; set; }

        /// <summary>
        /// Gets the SMS message for this communication.
        /// </summary>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachment binary files.
        /// </summary>
        public List<ListItemBag> SmsAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets the push notification title for this communication.
        /// </summary>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets the push notification message for this communication.
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open action for this communication.
        /// </summary>
        public PushOpenAction? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication list to which this communication should be sent.
        /// </summary>
        public string CommunicationListName { get; set; }

        /// <summary>
        /// Gets or sets the URL from where this communication was created.
        /// </summary>
        public string UrlReferrer { get; set; }
    }
}

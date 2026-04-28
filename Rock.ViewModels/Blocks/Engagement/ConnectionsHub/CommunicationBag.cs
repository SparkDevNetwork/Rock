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

using Rock.Enums.Communication;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to compose and send a communication, such as an email or SMS message, including
    /// message content, sender information, and attachments.
    /// </summary>
    public class CommunicationBag
    {
        /// <summary>
        /// Gets or sets the type of communication.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the associated communication template.
        /// </summary>
        public string CommunicationTemplateIdKey { get; set; }

        /// <summary>
        /// Gets or sets the email address that will appear as the sender in outgoing messages.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the display name of the sender.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the main message content of the communication.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the subject associated with this instance.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the system phone number used as the sender for SMS messages.
        /// </summary>
        public Guid? SmsFromSystemPhoneNumberGuid { get; set; }

        /// <summary>
        /// Gets or sets the text content of the SMS message to be sent.
        /// </summary>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the collection of email attachments associated with the communication.
        /// </summary>
        public List<ListItemBag> EmailAttachments { get; set; }

        /// <summary>
        /// Gets or sets the collection of SMS attachments associated with the communication.
        /// </summary>
        public List<ListItemBag> SmsAttachments { get; set; }

        /// <summary>
        /// Gets or sets the collection of recipients for the communication.
        /// </summary>
        public List<CommunicationRecipientBag> CommunicationRecipients { get; set; }
    }
}

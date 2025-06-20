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

using Rock.Enums.Communication;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing the communication template detail information in the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardCommunicationTemplateDetailBag
    {
        /// <summary>
        /// Gets or sets the communication template unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the email message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the from email.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachment binary files.
        /// </summary>
        public List<ListItemBag> SmsAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the email from name.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the email attachment binary files.
        /// </summary>
        public List<ListItemBag> EmailAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the CC emails.
        /// </summary>
        public string CcEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        public string BccEmails { get; set; }

        /// <summary>
        /// Gets or sets the SMS from system phone number unique identifier.
        /// </summary>
        public Guid? SmsFromSystemPhoneNumberGuid { get; set; }

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
        public PushOpenActionType? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this communication template can be used with the Communication Entry Wizard block.
        /// </summary>
        public bool IsWizardSupported { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the Communication Template.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the image file for the Template Preview Image.
        /// </summary>
        public ListItemBag ImageFile { get; set; }
        
        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a starter template.
        /// </summary>
        public bool IsStarter { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system template.
        /// </summary>
        public bool IsSystem { get; set; }
    }
}
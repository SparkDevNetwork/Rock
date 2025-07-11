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
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail
{
    /// <summary>
    /// Represents the communication template details for the Communication Flow Detail block.
    /// </summary>
    public class CommunicationFlowDetailCommunicationTemplateBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the communication template.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the binary file for the template preview.
        /// </summary>
        public ListItemBag ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the sender.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply-to email address.
        /// </summary>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the system phone number used for SMS sending.
        /// </summary>
        public ListItemBag SmsFromSystemPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the SMS message content.
        /// </summary>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the title of the push notification.
        /// </summary>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message content of the push notification.
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        public PushOpenActionType? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open message structured content JSON.
        /// </summary>
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the push mobile application.
        /// </summary>
        public ListItemBag PushMobileApplication { get; set; }

        /// <summary>
        /// Gets or sets the email attachment binary files.
        /// </summary>
        public List<ListItemBag> EmailAttachmentBinaryFiles { get; set; }

        /// <summary>
        /// Gets or sets the SMS attachment binary file.
        /// </summary>
        public ListItemBag SmsAttachmentBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the query string parameters for the push mobile page.
        /// </summary>
        public Dictionary<string, string> PushMobilePageQueryString { get; set; }

        /// <summary>
        /// Gets or sets the URL for the push notification.
        /// </summary>
        public string PushUrl { get; set; }

        /// <summary>
        /// Gets or sets the mobile page for the push notification.
        /// </summary>
        public PageRouteValueBag PushMobilePage { get; set; }
    }
}

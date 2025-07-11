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
using Rock.Model;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationTemplateDetail
{
    /// <summary>
    /// Represents the initialization box for the Communication Template Detail block.
    /// </summary>
    public class CommunicationTemplateDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the title of the block.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the template.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this template is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this template is a starter template.
        /// </summary>
        public bool IsStarter { get; set; }

        /// <summary>
        /// Gets or sets the description of the template.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the template.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the binary file for the template preview.
        /// </summary>
        public ListItemBag ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the binary file for the template logo.
        /// </summary>
        public ListItemBag LogoBinaryFile { get; set; }

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
        /// Gets or sets the CC email addresses.
        /// </summary>
        public string CcEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC email addresses.
        /// </summary>
        public string BccEmails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CSS inlining is enabled.
        /// </summary>
        public bool IsCssInliningEnabled { get; set; }
        
        /// <summary>
        /// A Dictionary of Key,DefaultValue for Lava MergeFields that can be used when processing Lava in the CommunicationTemplate
        /// By convention, a Key with a 'Color' suffix will indicate that the Value is selected using a ColorPicker. Otherwise,it is just text
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public Dictionary<string, string> LavaFields { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        public List<ListItemBag> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the SMS message content.
        /// </summary>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the selected system phone number.
        /// </summary>
        public ListItemBag SmsFromSystemPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of system phone numbers available for sending SMS messages.
        /// </summary>
        public List<ListItemBag> SmsFromSystemPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether editing is restricted.
        /// </summary>
        public bool IsEditRestricted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether editing is restricted by the system.
        /// </summary>
        public bool IsEditRestrictedSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether editing is restricted for unauthorized users.
        /// </summary>
        public bool IsEditRestrictedUnauthorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the attachments binary file type.
        /// </summary>
        public Guid AttachmentsBinaryFileTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        public PushOpenActionType PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the title of the push notification.
        /// </summary>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message content of the push notification.
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the mobile application for the push notification.
        /// </summary>
        public ListItemBag PushMobileApplication { get; set; }

        /// <summary>
        /// Gets or sets the push notification mobile applications that can be selected.
        /// </summary>
        public List<ListItemBag> PushMobileApplications { get; set; }

        /// <summary>
        /// Gets or sets the JSON content for the push open message.
        /// </summary>
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the mobile page for the push notification.
        /// </summary>
        public PageRouteValueBag PushMobilePage { get; set; }

        /// <summary>
        /// Gets or sets the query string parameters for the push mobile page.
        /// </summary>
        public Dictionary<string, string> PushMobilePageQueryString { get; set; }

        /// <summary>
        /// Gets or sets the URL for the push notification.
        /// </summary>
        public string PushUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the push medium is configured to use the Rock Mobile Push transport.
        /// </summary>
        public bool IsRockMobilePushTransportConfigured { get; set; }
        
        /// <summary>
        /// Gets or sets the communication template version.
        /// </summary>
        public CommunicationTemplateVersion Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a new communication template.
        /// </summary>
        public bool IsNew { get; set; }
    }
}

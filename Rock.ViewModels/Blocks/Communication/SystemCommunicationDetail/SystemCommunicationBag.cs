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

using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationTemplateDetail;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SystemCommunicationDetail
{
    /// <summary>
    /// Represents the system communication data for both initialization and save operations.
    /// </summary>
    public class SystemCommunicationBag
    {
        /// <summary>
        /// Gets or sets the title of the system communication.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this system communication is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the category of the system communication.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the sender.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the recipient email addresses.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the CC email addresses.
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the BCC email addresses.
        /// </summary>
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the email subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the HTML body of the email template.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CSS inlining is enabled.
        /// </summary>
        public bool IsCssInliningEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Lava merge fields with their default values.
        /// Fields with a key ending in "Color" use a color picker editor.
        /// </summary>
        public Dictionary<string, string> LavaFields { get; set; }

        /// <summary>
        /// Gets or sets the selected system phone number for SMS sending.
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
        /// Gets or sets the push notification message content.
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open action type.
        /// </summary>
        public PushOpenAction PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the mobile page for push notification navigation.
        /// </summary>
        public PageRouteValueBag PushMobilePage { get; set; }

        /// <summary>
        /// Gets or sets the query string parameters for the push mobile page.
        /// </summary>
        public Dictionary<string, string> PushMobilePageQueryString { get; set; }

        /// <summary>
        /// Gets or sets the URL for push notification link action.
        /// </summary>
        public string PushUrl { get; set; }

        /// <summary>
        /// Gets or sets the structured content JSON for the push open message.
        /// </summary>
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the rendered HTML for the push open message.
        /// Used for initialization only; not sent back on save.
        /// </summary>
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the mobile application for the push notification.
        /// </summary>
        public ListItemBag PushMobileApplication { get; set; }
    }
}

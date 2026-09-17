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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration
{
    /// <summary>
    /// The church's chat settings as the configuration screen sees them. The signing
    /// key is not here in any form: the browser learns only that one is present, and
    /// the platform-issued values below are shown but never taken back from here.
    /// </summary>
    public class ChatConfigurationBag
    {
        /// <summary>
        /// Gets or sets whether profile details are visible by default.
        /// </summary>
        public bool AreChatProfilesVisible { get; set; }

        /// <summary>
        /// Gets or sets whether anyone may start a direct message by default.
        /// </summary>
        public bool IsOpenDirectMessagingAllowed { get; set; }

        /// <summary>
        /// Gets or sets the youngest age that may use chat, or null for no limit.
        /// </summary>
        public int? MinimumAge { get; set; }

        /// <summary>
        /// Gets or sets the Data View naming who may start a direct message.
        /// </summary>
        public ListItemBag DirectMessageAccessDataView { get; set; }

        /// <summary>
        /// Gets or sets the Data Views whose members carry a badge.
        /// </summary>
        public List<ListItemBag> ChatBadgeDataViews { get; set; }

        /// <summary>
        /// Gets or sets the chat project this church talks to. Issued when chat was
        /// enabled and shown here read only.
        /// </summary>
        public string ProjectUrl { get; set; }

        /// <summary>
        /// Gets or sets the key the browser presents to that project. Public by design,
        /// issued when chat was enabled, shown here read only.
        /// </summary>
        public string PublishableKey { get; set; }

        /// <summary>
        /// Gets or sets this church's id on the chat platform. Read only.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the id of the key pair registered for this church. Read only.
        /// </summary>
        public string Kid { get; set; }

        /// <summary>
        /// Gets or sets whether a signing key is stored. The key itself never leaves
        /// the server, so this is all the screen is told about it.
        /// </summary>
        public bool IsChurchKeyPresent { get; set; }
    }
}

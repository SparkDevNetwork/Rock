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
    /// A bag that contains the chat configuration settings.
    /// </summary>
    public class ChatConfigurationBag
    {
        /// <summary>
        /// Gets or sets the API key for Rock to use when interacting with the external chat application.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API secret for Rock to use when interacting with the external chat application.
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the system default for whether individuals' profiles are visible in the external chat application.
        /// </summary>
        public bool AreChatProfilesVisible { get; set; }

        /// <summary>
        /// Gets or sets the system default for whether individuals can receive direct messages from anybody in the system.
        /// </summary>
        public bool IsOpenDirectMessagingAllowed { get; set; }

        /// <summary>
        /// Gets or sets the list of data views that will be used to populate badges in the external chat application.
        /// </summary>
        public List<ListItemBag> ChatBadgeDataViews { get; set; }
    }
}

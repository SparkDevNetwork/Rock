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

using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;

namespace Rock.ViewModels.Rest.Chat
{
    /// <summary>
    /// What the mobile app is given when chat opens: what the web's chat block is given, and
    /// the first church token, so a cold open pays one call to Rock before the chat platform.
    /// </summary>
    public class ChatMobileSessionBag
    {
        /// <summary>
        /// Gets or sets the outcome of the session gates and, when they passed, the person and
        /// the public platform settings. The photo address is absolute.
        /// </summary>
        public ChatShellSessionBag Session { get; set; }

        /// <summary>
        /// Gets or sets the church's name.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the church's active campuses, in the church's own order.
        /// </summary>
        public List<ChatCampusBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the first church token, or null when the session gates refused the person.
        /// </summary>
        public ChatChurchTokenBag Token { get; set; }
    }
}

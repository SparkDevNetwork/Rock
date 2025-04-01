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
using Rock.Model;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a channel in the external chat system and a <see cref="Group"/> in Rock.
    /// </summary>
    internal class ChatChannel
    {
        /// <summary>
        /// Gets or sets the channel key.
        /// </summary>
        /// <value>
        /// "rock_group_{Group.Id}" if this channel was created by Rock. If the key is not in this format, this means
        /// the channel was created within the external chat system (e.g. direct message channels between 2 chat users).
        /// </value>
        public string Key { get; set; }

        /// <inheritdoc cref="ChatChannelType.Key"/>
        public string ChatChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the "queryable" key, to be used when querying the external chat system's API for an existing
        /// <see cref="ChatChannel"/>.
        /// </summary>
        public string QueryableKey { get; set; }

        /// <summary>
        /// Gets or sets the friendly name to be displayed within the chat UI for this channel.
        /// </summary>
        /// <value>
        /// <see cref="Group.Name"/> if this channel was created by Rock.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc cref="Group.CampusId"/>
        public int? CampusId { get; set; }

        /// <inheritdoc cref="Group.GetIsLeavingChatChannelAllowed"/>
        public bool IsLeavingAllowed { get; set; }

        /// <inheritdoc cref="Group.GetIsChatChannelPublic"/>
        public bool IsPublic { get; set; }

        /// <inheritdoc cref="Group.GetIsChatChannelAlwaysShown"/>
        public bool IsAlwaysShown { get; set; }

        /// <summary>
        /// Gets or sets whether the channel is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

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
    /// Represents the minimum necessary info from a <see cref="Group"/> model needed to synchronize to a
    /// <see cref="ChatChannel"/> in the external chat system.
    /// </summary>
    internal class RockChatGroup
    {
        /// <summary>
        /// Gets or sets the <see cref="GroupType"/> identifier that represents this <see cref="Group"/>.
        /// </summary>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of this <see cref="Group"/>.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatChannelType"/> in the external chat system.
        /// </summary>
        public string ChatChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatChannel"/> in the external chat system.
        /// </summary>
        public string ChatChannelKey { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="Group.ChatChannelKey"/> should be saved in the Rock database.
        /// </summary>
        public bool ShouldSaveChatChannelKeyInRock { get; set; }

        /// <inheritdoc cref="Group.Name"/>
        public string Name { get; set; }

        /// <inheritdoc cref="Group.GetIsLeavingChatChannelAllowed"/>
        public bool IsLeavingAllowed { get; set; }

        /// <inheritdoc cref="Group.GetIsChatChannelPublic"/>/>
        public bool IsPublic { get; set; }

        /// <inheritdoc cref="Group.GetIsChatChannelAlwaysShown"/>/>
        public bool IsAlwaysShown { get; set; }

        /// <inheritdoc cref="Group.GetIsChatEnabled"/>
        public bool IsChatEnabled { get; set; }

        /// <inheritdoc cref="Group.GetIsChatChannelActive"/>
        public bool IsChatChannelActive { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="ChatChannel"/> should be deleted in the external chat system.
        /// </summary>
        public bool ShouldDelete { get; set; }
    }
}

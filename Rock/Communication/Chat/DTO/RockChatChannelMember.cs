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
using Rock.Enums.Communication.Chat;
using Rock.Model;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents the minimum necessary combined info from Rock <see cref="GroupMember"/> and <see cref="GroupTypeRole"/>
    /// models needed to synchronize to a <see cref="ChatChannelMember"/> in the external chat system.
    /// </summary>
    internal class RockChatChannelMember
    {
        /// <summary>
        /// Gets or sets the <see cref="Group"/> identifier that represents this <see cref="GroupMember"/>.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier that represents this <see cref="GroupMember"/>.
        /// </summary>
        public int PersonId { get; set; }

        /// <inheritdoc cref="GroupTypeRole.ChatRole"/>
        public ChatRole ChatRole { get; set; }

        /// <inheritdoc cref="GroupMember.IsChatMuted"/>
        public bool IsChatMuted { get; set; }

        /// <inheritdoc cref="GroupMember.IsChatBanned"/>
        public bool IsChatBanned { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="ChatChannelMember"/> should be deleted in the external chat system.
        /// </summary>
        public bool ShouldDelete { get; set; }
    }
}

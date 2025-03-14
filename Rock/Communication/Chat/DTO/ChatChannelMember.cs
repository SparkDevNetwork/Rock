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
    /// Represents a channel member in the external chat system and a <see cref="GroupMember"/> in Rock.
    /// </summary>
    internal class ChatChannelMember
    {
        /// <inheritdoc cref="ChatUser.Key"/>
        public string ChatUserKey { get; set; }

        /// <summary>
        /// Gets or sets the member's role within the channel.
        /// </summary>
        /// <value>
        /// The <see langword="string"/> representation of a <see cref="ChatRole"/>.
        /// </value>
        public string Role { get; set; }

        /// <inheritdoc cref="GroupMember.IsChatMuted"/>
        public bool IsChatMuted { get; set; }

        /// <inheritdoc cref="GroupMember.IsChatBanned"/>
        public bool IsChatBanned { get; set; }
    }
}

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
using System.Linq;

using Rock.Communication.Chat.DTO;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents a command to synchronize a <see cref="ChatChannel"/>'s <see cref="ChatChannelMember"/>s from Rock to
    /// the external chat system.
    /// </summary>
    internal class SyncChannelMembersToChatCommand
    {
        /// <summary>
        /// The backing field for the <see cref="UserKeysToDelete"/> property.
        /// </summary>
        private readonly List<string> _userKeysToDelete = new List<string>();

        /// <summary>
        /// The backing field for the <see cref="MembersToCreateOrUpdate"/> property.
        /// </summary>
        private readonly List<ChatChannelMember> _membersToCreateOrUpdate = new List<ChatChannelMember>();

        /// <inheritdoc cref="ChatChannelType.Key"/>
        public string ChatChannelTypeKey { get; set; }

        /// <inheritdoc cref="ChatChannel.Key"/>
        public string ChatChannelKey { get; set; }

        /// <summary>
        /// Gets the list of <see cref="ChatUser.Key"/>s for <see cref="ChatChannelMember"/>s that should be deleted
        /// within the external chat system.
        /// </summary>
        public List<string> UserKeysToDelete => _userKeysToDelete;

        /// <summary>
        /// Gets the list of <see cref="ChatChannelMember"/>s that should be created or updated within the external chat
        /// system.
        /// </summary>
        public List<ChatChannelMember> MembersToCreateOrUpdate => _membersToCreateOrUpdate;

        /// <summary>
        /// Gets the list of distinct <see cref="ChatUser.Key"/>s represented within this command.
        /// </summary>
        public List<string> DistinctChatUserKeys =>
            UserKeysToDelete
            .Union( MembersToCreateOrUpdate.Select( m => m.ChatUserKey ) )
            .Distinct()
            .ToList();
    }
}

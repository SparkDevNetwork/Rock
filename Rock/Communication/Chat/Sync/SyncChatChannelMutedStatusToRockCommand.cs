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
using Rock.Communication.Chat.DTO;
using Rock.Enums.Communication.Chat;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents a command to synchronize a chat channel member's muted status to Rock.
    /// </summary>
    internal class SyncChatChannelMutedStatusToRockCommand : ChatToRockSyncCommand
    {
        /// <summary>
        /// Gets or sets the Rock group identifier.
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatChannel"/> in the external chat system.
        /// </summary>
        public string ChatChannelKey { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatUser"/> in the external chat system.
        /// </summary>
        /// <remarks>
        /// Subbing "user" for "person" in the property name, as this can get logged in the UI.
        /// </remarks>
        public string ChatPersonKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PersonAlias"/> identifier.
        /// </summary>
        /// <remarks>
        /// This will be added while processing the command; useful for logging.
        /// </remarks>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier.
        /// </summary>
        /// <remarks>
        /// This will be added while processing the command; useful for logging.
        /// </remarks>
        public int? PersonId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncChatChannelMutedStatusToRockCommand"/> class.
        /// </summary>
        /// <param name="attemptLimit">The maximum number of times to attempt a chat-to-Rock sync command before giving up.</param>
        /// <param name="chatSyncType">The type of synchronization to perform.</param>
        public SyncChatChannelMutedStatusToRockCommand( int attemptLimit, ChatSyncType chatSyncType )
            : base( attemptLimit, chatSyncType )
        {
        }
    }
}

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
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents a command to synchronize a <see cref="Person"/> to a <see cref="ChatUser"/> in the external chat system.
    /// </summary>
    internal class SyncPersonToChatCommand
    {
        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets whether a chat-specific <see cref="PersonAlias"/> should be created and saved to Rock if this
        /// <see cref="Person"/> doesn't already have one.
        /// </summary>
        /// <remarks>
        /// If <see langword="true"/>, this command expects that a new chat-specific <see cref="PersonAlias"/> will be
        /// created and saved to Rock if this <see cref="Person"/> doesn't already have one. If <see langword="false"/>,
        /// and this <see cref="Person"/> doesn't already have a chat-specific <see cref="PersonAlias"/>, synchronization
        /// with the external chat system should NOT take place for this <see cref="Person"/>.
        /// </remarks>
        public bool ShouldEnsureChatAliasExists { get; set; }
    }
}

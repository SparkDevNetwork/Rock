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
using System;

using Rock.Model;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents the mapping between a <see cref="Person"/> and their respective <see cref="ChatUser"/> within the
    /// external chat system.
    /// </summary>
    internal class RockChatUserKey
    {
        /// <summary>
        /// The backing field for the <see cref="ChatUserKey"/> property.
        /// </summary>
        private string _chatUserKey;

        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the chat-specific <see cref="PersonAlias"/> identifier.
        /// </summary>
        public int? ChatPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the chat-specific <see cref="PersonAlias"/> unique identifier.
        /// </summary>
        public Guid? ChatPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets the <see cref="ChatUser.Key"/>.
        /// </summary>
        public string ChatUserKey
        {
            get
            {
                if ( _chatUserKey.IsNullOrWhiteSpace() && this.ChatPersonAliasGuid.HasValue )
                {
                    _chatUserKey = ChatHelper.GetChatUserKey( this.ChatPersonAliasGuid.Value );
                }

                return _chatUserKey;
            }
        }
    }
}

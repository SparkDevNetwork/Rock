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
    /// external chat systemS.
    /// </summary>
    internal class RockChatUserPersonKey
    {
        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets this person's chat-specific <see cref="PersonAlias"/> unique identifier.
        /// </summary>
        public Guid? ChatAliasGuid { get; set; }

        /// <summary>
        /// Gets the <see cref="ChatUser.Key"/> for this person.
        /// </summary>
        public string ChatUserKey => this.ChatAliasGuid.HasValue
            ? ChatHelper.GetChatUserKey( this.ChatAliasGuid.Value )
            : null;
    }
}

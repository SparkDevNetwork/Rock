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

using Rock.Model;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a chat user in the external chat system and a <see cref="PersonAlias"/> in Rock.
    /// </summary>
    internal class ChatUser
    {
        /// <summary>
        /// Gets or sets the chat user key.
        /// </summary>
        /// <value>
        /// "rock_user_{PersonAlias.Id}"
        /// </value>
        public string Key { get; set; }

        /// <inheritdoc cref="Person.FullName"/>
        public string Name { get; set; }

        /// <inheritdoc cref="Person.PhotoUrl"/>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets whether this chat user represents a global admin in the external chat system.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if person belongs to the "APP - Chat Administrators" security role group.
        /// </value>
        public bool IsAdmin { get; set; }

        /// <inheritdoc cref="Person.IsChatProfilePublic"/>
        public bool IsProfileVisible { get; set; }

        /// <inheritdoc cref="Person.IsChatOpenDirectMessageAllowed"/>
        public bool IsOpenDirectMessageAllowed { get; set; }

        /// <summary>
        /// Gets or sets the badges associated with this chat user.
        /// </summary>
        /// <value>
        /// One entry for each chat-related <see cref="DataView"/> for which this person qualifies.
        /// </value>
        public List<ChatBadge> Badges { get; set; }
    }
}

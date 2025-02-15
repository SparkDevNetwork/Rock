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
    /// Represents the minimum necessary combined info from Rock <see cref="Person"/> and <see cref="PersonAlias"/>
    /// models needed to synchronize to a <see cref="ChatUser"/> in the external chat system.
    /// </summary>
    internal class RockChatUserPerson : RockChatUserPersonKey
    {
        /// <inheritdoc cref="Person.NickName"/>
        public string NickName { get; set; }

        /// <inheritdoc cref="Person.LastName"/>
        public string LastName { get; set; }

        /// <inheritdoc cref="Person.SuffixValueId"/>
        public int? SuffixValueId { get; set; }

        /// <inheritdoc cref="Person.RecordTypeValueId"/>
        public int? RecordTypeValueId { get; set; }

        /// <inheritdoc cref="Person.PhotoId"/>
        public int? PhotoId { get; set; }

        /// <inheritdoc cref="Person.BirthYear"/>
        public int? BirthYear { get; set; }

        /// <inheritdoc cref="Person.Gender"/>
        public Gender Gender { get; set; }

        /// <inheritdoc cref="Person.AgeClassification"/>
        public AgeClassification AgeClassification { get; set; }

        /// <inheritdoc cref="Person.IsChatProfilePublic"/>
        public bool? IsChatProfilePublic { get; set; }

        /// <inheritdoc cref="Person.IsChatOpenDirectMessageAllowed"/>
        public bool? IsChatOpenDirectMessageAllowed { get; set; }

        /// <summary>
        /// Gets or sets whether this person is a member of the "APP - Chat Administrators" security role group.
        /// </summary>
        public bool IsChatAdministrator { get; set; }

        /// <inheritdoc cref="ChatUser.Badges"/>
        public List<ChatBadge> Badges { get; set; }

        /// <summary>
        /// Gets or sets whether the underlying chat-specific <see cref="PersonAlias"/> already existed in Rock for
        /// this <see cref="Person"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="true"/>, a chat-specific <see cref="PersonAlias"/> already existed in Rock and was used to
        /// build this <see cref="RockChatUserPerson"/>. If <see langword="false"/>, a new <see cref="PersonAlias"/> was
        /// saved to the Rock database as a part of creating this <see cref="RockChatUserPerson"/>.
        /// </remarks>
        public bool AlreadyExistedInRock { get; set; } = true;
    }
}

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

using Rock.Communication.Chat.DTO;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents a command to send a direct message to <see cref="ChatUser"/>s in the external chat system.
    /// </summary>
    internal class SendChatDirectMessageCommand
    {
        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifiers that represent the <see cref="ChatUser"/>s to whom the
        /// message should be sent.
        /// </summary>
        public List<int> RecipientPersonIds { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier that represents the <see cref="ChatUser"/> sending the message.
        /// </summary>
        public int SenderPersonId { get; set; }

        /// <summary>
        /// Gets or sets the message text to send.
        /// </summary>
        /// <remarks>
        /// Mentions should be in the format of @{personId} (e.g. @123).
        /// </remarks>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="RockChatMessageAttachment"/>s to include with the message.
        /// </summary>
        public List<RockChatMessageAttachment> Attachments { get; set; }
    }
}

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

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a message to be sent using the external chat system.
    /// </summary>
    internal class RockChatMessage
    {
        /// <summary>
        /// Gets or sets the message text to send.
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the attachments to be included.
        /// </summary>
        public List<RockChatMessageAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ChatUser.Key"/>s that are mentioned in the message.
        /// </summary>
        public HashSet<string> MentionedChatUserKeys { get; } = new HashSet<string>();
    }
}

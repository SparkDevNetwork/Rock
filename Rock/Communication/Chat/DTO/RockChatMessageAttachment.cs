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

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a message attachment to be sent using the external chat system.
    /// </summary>
    internal class RockChatMessageAttachment
    {
        /// <summary>
        /// Gets or sets the type of the attachment.
        /// </summary>
        public ChatAttachmentType Type { get; set; }

        /// <summary>
        /// Gets or sets the URL of the attachment.
        /// </summary>
        public string Url { get; set; }
    }
}

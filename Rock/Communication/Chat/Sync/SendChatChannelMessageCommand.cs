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
    /// Represents a command to send a message to a <see cref="ChatChannel"/> in the external chat system.
    /// </summary>
    internal class SendChatChannelMessageCommand
    {
        /// <summary>
        /// Gets or sets the <see cref="ChatChannelType.Key"/> that represents the channel type to which the message
        /// should be sent.
        /// </summary>
        /// <remarks>
        /// If this value and <see cref="ChatChannelKey"/> are both set, the <see cref="GroupId"/> property will be ignored.
        /// </remarks>
        public string ChatChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChatChannel.Key"/> that represents the channel to which the message should be sent.
        /// </summary>
        /// <remarks>
        /// If this value and <see cref="ChatChannelTypeKey"/> are both set, the <see cref="GroupId"/> property will be ignored.
        /// </remarks>
        public string ChatChannelKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Group"/> identifier that represents the <see cref="ChatChannelType"/> and
        /// <see cref="ChatChannel"/> to which the message should be sent.
        /// </summary>
        /// <remarks>
        /// If you already know the <see cref="ChatChannelTypeKey"/> and <see cref="ChatChannelKey"/>, set those instead.
        /// Otherwise, this value can be used to look up those needed keys.
        /// </remarks>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChatUser.Key"/> that represents the <see cref="Person"/> sending the message.
        /// </summary>
        /// <remarks>
        /// If this value is set, the <see cref="SenderPersonId"/> property will be ignored.
        /// </remarks>
        public string SenderChatUserKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier that represents the <see cref="ChatUser"/> sending the message.
        /// </summary>
        /// <remarks>
        /// If you already know the <see cref="SenderChatUserKey"/>, set that instead. Otherwise, this value can be used
        /// too look up that needed key.
        /// </remarks>
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

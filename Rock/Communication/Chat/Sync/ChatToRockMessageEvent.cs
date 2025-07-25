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
using System.Collections.Generic;

using Rock.Communication.Chat.DTO;
using Rock.Enums.Communication.Chat;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents a message event received from the external chat system.
    /// </summary>
    internal class ChatToRockMessageEvent
    {
        /// <summary>
        /// Gets or sets the type of message event that was reported.
        /// </summary>
        public ChatMessageEventType ChatMessageEventType { get; private set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatChannelType"/> in the external chat system, within
        /// which this event took place.
        /// </summary>
        public string ChatChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the Rock <see cref="Model.GroupType"/> that corresponds to the <see cref="ChatChannelType"/> in
        /// the external chat system.
        /// </summary>
        public GroupType ChannelType { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatChannel"/> in the external chat system, within which
        /// this event took place.
        /// </summary>
        public string ChatChannelKey { get; set; }

        /// <summary>
        /// Gets or sets the Rock <see cref="Model.Group"/> that corresponds to the <see cref="ChatChannel"/> in the
        /// external chat system.
        /// </summary>
        public Group Channel { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the <see cref="ChatUser"/> responsible for this message event.
        /// </summary>
        /// <remarks>
        /// Subbing "user" for "person" in the property name, as this can get logged in the UI.
        /// </remarks>
        public string SenderChatPersonKey { get; set; }

        /// <summary>
        /// Gets or sets the Rock <see cref="Model.Person"/> that corresponds to the <see cref="ChatUser"/> in the
        /// external chat system who is responsible for this message event.
        /// </summary>
        public Person SenderPerson { get; set; }

        /// <summary>
        /// Gets or sets the message contained within this event.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Rock datetime this event took place.
        /// </summary>
        public DateTime RockDateTime { get; set; }

        /// <summary>
        /// Gets or sets information about the members of the <see cref="ChatChannel"/> within which this event took place.
        /// </summary>
        public List<RockChatMessageEventChannelMember> EventChannelMembers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatToRockMessageEvent"/> class.
        /// </summary>
        /// <param name="chatMessageEventType">The type of message event that was reported.</param>
        public ChatToRockMessageEvent( ChatMessageEventType chatMessageEventType )
        {
            ChatMessageEventType = chatMessageEventType;
        }
    }
}

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

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a <see cref="ChatChannelMember"/> within the <see cref="ChatChannel"/> where a message event took place.
    /// </summary>
    internal class RockChatMessageEventChannelMember
    {
        /// <summary>
        /// Gets or sets this member's <see cref="ChatUser.Key"/>.
        /// </summary>
        /// <remarks>
        /// Subbing "user" for "person" in the property name, as this can get logged in the UI.
        /// </remarks>
        public string ChatPersonKey { get; set; }

        /// <summary>
        /// Gets or sets this member's "last read at" Rock datetime within the <see cref="ChatChannel"/>.
        /// </summary>
        public DateTime? LastReadAtRockDateTime { get; set; }
    }
}

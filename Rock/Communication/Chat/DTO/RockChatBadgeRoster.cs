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
    /// Represents the current <see cref="DTO.ChatBadge"/> <see cref="Person"/> roster for a given <see cref="ChatUser"/>
    /// synchronization run.
    /// </summary>
    internal class RockChatBadgeRoster
    {
        /// <summary>
        /// Gets or sets the <see cref="DTO.ChatBadge"/> to be assigned.
        /// </summary>
        public ChatBadge ChatBadge { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the <see cref="Person"/>s to who this <see cref="ChatBadge"/> should be assigned.
        /// </summary>
        public HashSet<int> PersonIds { get; set; }
    }
}

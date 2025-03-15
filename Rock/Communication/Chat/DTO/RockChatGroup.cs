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
using Rock.Model;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents the minimum necessary info from a <see cref="Group"/> for identification and labeling purposes within
    /// Rock's chat operations.
    /// </summary>
    internal class RockChatGroup
    {
        /// <summary>
        /// Gets or sets the identifier of the <see cref="Group"/>.
        /// </summary>
        /// <value>
        /// The value from the corresponding <see cref="Group"/> identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="Group"/>.
        /// </summary>
        /// <value>
        /// The value from the corresponding <see cref="Group.Name"/>.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key of the <see cref="ChatChannel"/> within the external chat system.
        /// </summary>
        public string ChatChannelKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the <see cref="ChatChannelType"/> within the external chat system.
        /// </summary>
        public string ChatChannelTypeKey { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="Group"/> is currently chat-enabled.
        /// </summary>
        public bool IsChatEnabled { get; set; }
    }
}

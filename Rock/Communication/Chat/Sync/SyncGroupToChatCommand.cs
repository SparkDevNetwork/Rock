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
using Rock.Communication.Chat.DTO;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents a command to synchronize a <see cref="Group"/> to a <see cref="ChatChannel"/> in the external chat system.
    /// </summary>
    internal class SyncGroupToChatCommand
    {
        /// <summary>
        /// Gets or sets the <see cref="GroupType"/> identifier that represents this <see cref="Group"/>.
        /// </summary>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the identifier that represents this <see cref="Group"/>.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChatChannel.Key"/> that represents this <see cref="Group"/>.
        /// </summary>
        public string ChatChannelKey { get; set; }
    }
}

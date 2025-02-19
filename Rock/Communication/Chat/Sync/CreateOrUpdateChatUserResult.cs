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
    /// Represents the result of creating or updating a <see cref="ChatUser"/> within the external chat system.
    /// </summary>
    internal class CreateOrUpdateChatUserResult
    {
        /// <summary>
        /// Gets or sets the identifier of the <see cref="Person"/> that was created or updated.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChatUser.Key"/> that represents this person in the external chat system.
        /// </summary>
        public string ChatUserKey { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="ChatUser"/> belongs to the `rock_admin` role in the external chat system.
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}

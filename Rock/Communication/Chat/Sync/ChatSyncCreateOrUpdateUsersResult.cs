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

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents the result of creating or updating a list of <see cref="ChatUser"/>s within the external chat system.
    /// </summary>
    internal class ChatSyncCreateOrUpdateUsersResult : ChatSyncResultBase
    {
        /// <summary>
        /// Gets the results of creating or updating the individual <see cref="ChatUser"/>s.
        /// </summary>
        public List<ChatSyncCreateOrUpdateUserResult> UserResults { get; } = new List<ChatSyncCreateOrUpdateUserResult>();
    }
}

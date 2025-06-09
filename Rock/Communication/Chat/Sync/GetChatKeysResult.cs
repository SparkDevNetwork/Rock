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

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents the result of getting identifiers for records that exist in the external chat system.
    /// </summary>
    /// <seealso cref="ChatSyncResultBase"/>
    internal class GetChatKeysResult : ChatSyncResultBase
    {
        /// <summary>
        /// Gets the identifiers for records that exist within the external chat system.
        /// </summary>
        public HashSet<string> Keys { get; } = new HashSet<string>();
    }
}

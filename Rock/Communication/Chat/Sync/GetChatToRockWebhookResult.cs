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
    /// Represents the result of transforming webhook requests from the external chat system into commands and events
    /// Rock knows how to handle.
    /// </summary>
    /// <see cref="ChatSyncResultBase"/>
    internal class GetChatToRockWebhookResult : ChatSyncResultBase
    {
        /// <summary>
        /// Gets the list of <see cref="ChatToRockSyncCommand"/>s received as webhooks from the external chat system.
        /// </summary>
        public List<ChatToRockSyncCommand> SyncCommands { get; } = new List<ChatToRockSyncCommand>();

        /// <summary>
        /// Gets the list of <see cref="ChatToRockMessageEvent"/>s received as webhooks from the external chat system.
        /// </summary>
        public List<ChatToRockMessageEvent> MessageEvents { get; } = new List<ChatToRockMessageEvent>();
    }
}

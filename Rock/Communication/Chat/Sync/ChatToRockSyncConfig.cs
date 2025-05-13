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
namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// A configuration class to fine-tune how Chat-to-Rock synchronization should be completed.
    /// </summary>
    internal class ChatToRockSyncConfig
    {
        /// <summary>
        /// The maximum number of times to attempt a chat-to-Rock sync command before giving up.
        /// </summary>
        public int SyncCommandAttemptLimit { get; set; } = 5;
    }
}

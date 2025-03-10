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
using System.Linq;

using Rock.Communication.Chat;
using Rock.Communication.Chat.Sync;

namespace Rock.Transactions
{
    /// <summary>
    /// Represents a transaction for requeuing a chat-to-Rock sync command.
    /// </summary>
    internal class RequeueChatToRockSyncCommandTransaction : AggregateTransaction<ChatToRockSyncCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequeueChatToRockSyncCommandTransaction"/> class.
        /// </summary>
        /// <param name="syncCommand">The Rock-to-Chat sync command.</param>
        public RequeueChatToRockSyncCommandTransaction( ChatToRockSyncCommand syncCommand )
            : base( syncCommand )
        {
        }

        /// <inheritdoc/>
        protected override void Execute( IList<ChatToRockSyncCommand> syncCommands )
        {
            using ( var chatHelper = new ChatHelper() )
            {
                chatHelper.SyncFromChatToRock( syncCommands.ToList() );
            }
        }
    }
}

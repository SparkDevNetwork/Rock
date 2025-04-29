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
using System.Threading.Tasks;

using Rock.Communication.Chat;
using Rock.Communication.Chat.Sync;

namespace Rock.Transactions
{
    /// <summary>
    /// Represents a transaction for handling a chat webhook request.
    /// </summary>
    internal class HandleChatWebhookRequestTransaction : AggregateAsyncTransaction<ChatWebhookRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandleChatWebhookRequestTransaction"/> class.
        /// </summary>
        /// <param name="webhookRequest">The payload of the webhook request.</param>
        public HandleChatWebhookRequestTransaction( ChatWebhookRequest webhookRequest )
            : base( webhookRequest )
        {
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync( IList<ChatWebhookRequest> webhookRequests )
        {
            using ( var chatHelper = new ChatHelper() )
            {
                await chatHelper.HandleChatWebhookRequestsAsync( webhookRequests.ToList() );
            }
        }
    }
}

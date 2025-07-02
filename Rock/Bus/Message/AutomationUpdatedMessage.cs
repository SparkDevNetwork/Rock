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
using Microsoft.Extensions.Logging;

using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Sent whenever something in the Automation system has changed that should
    /// cause Rock to reload the related trigger or events.
    /// </summary>
    public class AutomationUpdatedMessage : IEventMessage<CacheEventQueue>
    {
        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        /// <value>
        /// The sender rock instance unique identifier.
        /// </value>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// The identifier of the automation trigger that was updated.
        /// </summary>
        public int? AutomationTriggerId { get; set; }

        /// <summary>
        /// The identifier of the automation event that was updated.
        /// </summary>
        public int? AutomationEventId { get; set; }

        /// <summary>
        /// Publishes the AutomationUpdatedMessage message.
        /// </summary>
        public static void Publish( int? automationTriggerId, int? automationEventId )
        {
            var message = new AutomationUpdatedMessage
            {
                AutomationTriggerId = automationTriggerId,
                AutomationEventId = automationEventId
            };

            _ = RockMessageBus.PublishAsync<CacheEventQueue, AutomationUpdatedMessage>( message );

            RockLogger.LoggerFactory.CreateLogger<AutomationUpdatedMessage>()
                .LogDebug( "Published Automation Updated message." );
        }
    }
}

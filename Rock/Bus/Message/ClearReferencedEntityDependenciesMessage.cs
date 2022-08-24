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
using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Sent whenever something happens that should cause the referenced entity cache
    /// to be cleared on AttributeCache.
    /// </summary>
    public class ClearReferencedEntityDependenciesMessage : IEventMessage<CacheEventQueue>
    {
        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        /// <value>
        /// The sender rock instance unique identifier.
        /// </value>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Publishes the ClearReferencedEntityDependenciesMessage message.
        /// </summary>
        public static void Publish()
        {
            /*  08-12-2022 DSH

              In the case of publishing a ClearReferencedEntityDependenciesMessage, we
              don't need to check RockMessageBus.IsRockStarted. The ClearReferencedEntityDependenciesMessage
              publish logic doesn't have a dependency on having Rock fully started.

              Also, we really need to publish these messages regardless of IsRockStarted to
              prevent ClearReferencedEntityDependenciesMessage caches on other servers from getting stale.

              If we later discover that this isn't OK, we'll revisit this decision and make any updates to make it OK again.
           */

            var message = new ClearReferencedEntityDependenciesMessage();

            _ = RockMessageBus.PublishAsync<CacheEventQueue, ClearReferencedEntityDependenciesMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Published Clear Referenced Entity Dependencies message." );
        }
    }
}

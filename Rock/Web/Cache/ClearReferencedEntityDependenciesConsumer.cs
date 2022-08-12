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

using System.Collections.Concurrent;
using System.Reflection;

using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Clear Referenced Entity Dependencies Consumer class.
    /// </summary>
    public sealed class ClearReferencedEntityDependenciesConsumer : RockConsumer<CacheEventQueue, ClearReferencedEntityDependenciesMessage>
    {
        /// <summary>
        /// The cache types
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> _cacheTypeMethodInfoLookup = new ConcurrentDictionary<string, MethodInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCacheConsumer"/> class.
        /// </summary>
        public ClearReferencedEntityDependenciesConsumer()
        {
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( ClearReferencedEntityDependenciesMessage message )
        {
            /* 08-12-2022 DSH

            In the case of consuming a ClearReferencedEntityDependenciesMessage, we don't need to check RockMessageBus.IsRockStarted. The Cache Update
            logic doesn't have a dependency on having Rock fully started.

            Also, we really need to consume these messages regardless of IsRockStarted to prevent the cache from getting stale.

            If we later discover that this isn't OK, we'll revisit this decision and make any updates to make it OK again.
            */

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Consumed Clear Referenced Entity Dependencies message from {message.SenderNodeName} node." );

            AttributeCache.ClearReferencedEntityDependencies();
        }
    }
}

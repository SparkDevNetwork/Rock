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

using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Cache Update Message
    /// </summary>
    public interface ICacheWasUpdatedMessage : IEventMessage<CacheEventQueue>
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        string Key { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        string Region { get; set; }

        /// <summary>
        /// Gets or sets the name of the cache type.
        /// </summary>
        /// <value>
        /// The name of the cache type.
        /// </value>
        string CacheTypeName { get; set; }
    }

    /// <summary>
    /// Cache Update Message
    /// </summary>
    public class CacheWasUpdatedMessage : ICacheWasUpdatedMessage
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the name of the cache type.
        /// </summary>
        /// <value>
        /// The name of the cache type.
        /// </value>
        public string CacheTypeName { get; set; }

        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        /// <value>
        /// The sender rock instance unique identifier.
        /// </value>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Publishes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        public static void Publish<T>( string key = null, string region = null )
        {
            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish cache events until Rock is all the way started
                return;
            }

            var message = new CacheWasUpdatedMessage
            {
                Key = key,
                Region = region,
                CacheTypeName = typeof( T ).AssemblyQualifiedName
            };

            _ = RockMessageBus.PublishAsync<CacheEventQueue, CacheWasUpdatedMessage>( message );
        }
    }
}

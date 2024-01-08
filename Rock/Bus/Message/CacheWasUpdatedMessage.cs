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
        /// Gets or sets the <seealso cref="System.Type.AssemblyQualifiedName"/> type name of the cache type.
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
        /// Gets as debug string.
        /// </summary>
        /// <returns></returns>
        internal string ToDebugString()
        {
            string debugString;

            if ( this.CacheTypeName.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    debugString = $"CacheType: {System.Type.GetType( this.CacheTypeName, false )}. (";
                }
                catch
                {
                    debugString = $"CacheType: {this.CacheTypeName}. (";
                }
            }
            else
            {
                debugString = $"CacheType (";
            }

            if ( this.Key.IsNotNullOrWhiteSpace() )
            {
                debugString += $"Key: {this.Key}";
            }
            else
            {
                debugString += $"Key: null";
            }

            if ( this.Region.IsNotNullOrWhiteSpace() )
            {
                debugString += $", Region: {this.Region}";
            }

            if ( this.SenderNodeName.IsNotNullOrWhiteSpace() )
            {
                debugString += $", SenderNodeName: {this.SenderNodeName}";
            }

            if ( this.SenderNodeName.IsNotNullOrWhiteSpace() )
            {
                debugString += $", IsRockStarted: {RockMessageBus.IsRockStarted}";
            }

            return debugString + ")";
        }

        /// <summary>
        /// Publishes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="region">The region.</param>
        public static void Publish<T>( string key = null, string region = null )
        {
            /*  06-07-2022 MP

            In the case of publishing a CacheWasUpdatedMessage, we don't need to check RockMessageBus.IsRockStarted. The Cache publish
            logic doesn't have a dependency on having Rock fully started.

            Also, we really need to publish these messages regardless of IsRockStarted to prevent caches on other servers from getting stale.

            If we later discover that this isn't OK, we'll revisit this decision and make any updates to make it OK again.

            */

            var message = new CacheWasUpdatedMessage
            {
                Key = key,
                Region = region,
                CacheTypeName = typeof( T )?.AssemblyQualifiedName,
                SenderNodeName = RockMessageBus.NodeName
            };

            _ = RockMessageBus.PublishAsync<CacheEventQueue, CacheWasUpdatedMessage>( message );

            RockLogger.LoggerFactory.CreateLogger<CacheWasUpdatedMessage>()
                .LogDebug( $"Published Cache Update message. {message.ToDebugString()}." );
        }
    }
}

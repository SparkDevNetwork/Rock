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
    /// Cache Update Message
    /// </summary>
    public interface INewItemCacheAddedMessage : IEventMessage<CacheEventQueue>
    {
        /// <summary>
        /// Gets or sets the <seealso cref="System.Type.AssemblyQualifiedName"/> type name of the cache type.
        /// </summary>
        /// <value>
        /// The name of the cache type.
        /// </value>
        string CacheTypeName { get; set; }

        /// <summary>
        /// Gets or sets the idkey
        /// </summary>
        /// <value>
        /// The Id Key of the Item
        /// </value>
        string IdKey { get; set; }
    }

    /// <summary>
    /// Cache Update Message
    /// </summary>
    public class NewItemCacheAddedMessage : INewItemCacheAddedMessage
    {
        /// <summary>
        /// Gets or sets the <seealso cref="System.Type.AssemblyQualifiedName"/> type name of the cache type.
        /// </summary>
        /// <value>
        /// The name of the cache type.
        /// </value>
        public string CacheTypeName { get; set; }

        /// <summary>
        /// Gets or sets the idkey
        /// </summary>
        /// <value>
        /// The Id Key of the Item
        /// </value>
        public string IdKey { get; set; }

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
            string debugString = "(";

            if ( this.CacheTypeName.IsNotNullOrWhiteSpace() )
            {
                debugString += $", CacheTypeName: {this.CacheTypeName}";
            }

            if ( this.IdKey.IsNotNullOrWhiteSpace() )
            {
                debugString += $", IdKey: {this.IdKey}";
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
        /// Publishes the Key which is added to the list of ids in the .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newEntityId">The Id of the new Entity that is being created.</param>
        public static void Publish<T>( string newEntityId = null )
        {
            var message = new NewItemCacheAddedMessage
            {
                CacheTypeName = typeof( T )?.AssemblyQualifiedName,
                IdKey = newEntityId,
                SenderNodeName = RockMessageBus.NodeName
            };

            _ = RockMessageBus.PublishAsync<CacheEventQueue, NewItemCacheAddedMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Published New Item Added message. {message.ToDebugString()}." );
        }
    }
}

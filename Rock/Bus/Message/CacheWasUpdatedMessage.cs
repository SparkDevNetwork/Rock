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
using Rock.Model;
using Rock.Utility.Settings;

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
            var message = new CacheWasUpdatedMessage
            {
                Key = key,
                Region = region,
                CacheTypeName = typeof( T )?.AssemblyQualifiedName,
                SenderNodeName = RockMessageBus.NodeName
            };

            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish cache events until Rock is all the way started
                var logMessage = $"Cache Update message was not published because Rock is not fully started yet. {message.ToDebugString()}.";

                var elapsedSinceProcessStarted = RockDateTime.Now - RockInstanceConfig.ApplicationStartedDateTime;
                if ( elapsedSinceProcessStarted.TotalSeconds > RockMessageBus.MAX_SECONDS_SINCE_STARTTIME_LOG_ERROR )
                {
                    RockLogger.Log.Error( RockLogDomains.Bus, logMessage );
                    ExceptionLogService.LogException( new BusException( logMessage ) );
                }
                else
                {
                    RockLogger.Log.Debug( RockLogDomains.Bus, logMessage );
                }

                return;
            }

            _ = RockMessageBus.PublishAsync<CacheEventQueue, CacheWasUpdatedMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Published Cache Update message. {message.ToDebugString()}." );
        }
    }
}

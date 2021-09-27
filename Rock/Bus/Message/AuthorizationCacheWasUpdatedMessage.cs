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
    /// </summary>
    public interface IAuthorizationCacheWasUpdatedMessage : IEventMessage<CacheEventQueue>
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
    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Rock.Bus.Message.IAuthorizationCacheWasUpdatedMessage" />
    public class AuthorizationCacheWasUpdatedMessage : IAuthorizationCacheWasUpdatedMessage
    {
        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        /// <value>
        /// The sender rock instance unique identifier.
        /// </value>
        public string SenderNodeName { get; set; }

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
        /// Publishes the AuthorizationCacheWasUpdatedMessage message.
        /// </summary>
        public static void Publish( string key )
        {
            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish cache events until Rock is all the way started
                var logMessage = $"Authorization Update message was not published because Rock is not fully started yet.";
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

            var message = new AuthorizationCacheWasUpdatedMessage
            {
                Key = key
            };

            _ = RockMessageBus.PublishAsync<CacheEventQueue, AuthorizationCacheWasUpdatedMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Published Authorization Update message." );
        }
    }
}

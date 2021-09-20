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
using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;
using Rock.Model;
using Rock.Utility.Settings;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Rock Authorization Cache Consumer
    /// </summary>
    public sealed class AuthorizationCacheConsumer : RockConsumer<CacheEventQueue, AuthorizationCacheWasUpdatedMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCacheConsumer"/> class.
        /// </summary>
        public AuthorizationCacheConsumer()
        {
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( AuthorizationCacheWasUpdatedMessage message )
        {
            if ( !RockMessageBus.IsRockStarted )
            {
                var logMessage = $"Authorization Update message was not consumed because Rock is not fully started yet.";
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

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Consumed Authorization Cache Update message." );

            ApplyCacheMessage( message );
        }

        /// <summary>
        /// Applies the Authorization cache message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ApplyCacheMessage( AuthorizationCacheWasUpdatedMessage message )
        {
            CacheWasUpdatedMessage cacheWasUpdatedMessage = new CacheWasUpdatedMessage
            {
                Key = message.Key,
                Region = message.Region,
                SenderNodeName = message.SenderNodeName
            };

            RockCacheManager<object>.Instance.ReceiveRemoveMessage( cacheWasUpdatedMessage );
        }
    }
}

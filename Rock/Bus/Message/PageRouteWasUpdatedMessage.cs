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
using System.Collections.Generic;
using System.Linq;

using Rock.Bus.Queue;
using Rock.Logging;
using Rock.Model;
using Rock.Utility.Settings;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Event bus message class used to indicate that a Page Route was updated. The consumer will rebuild the RouteTable when received.
    /// </summary>
    public class PageRouteWasUpdatedMessage : IEventMessage<PageRouteEventQueue>
    {
        /// <inheritdoc />
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Publishes this instance.
        /// </summary>
        public static void Publish()
        {
            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish events until Rock is all the way started
                var logMessage = $"'Page Route Was Updated' message was not published because Rock is not fully started.";

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

            var message = new PageRouteWasUpdatedMessage();

            _ = RockMessageBus.PublishAsync<PageRouteEventQueue, PageRouteWasUpdatedMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Published 'Page Route Was Updated' message." );
        }
    }
}

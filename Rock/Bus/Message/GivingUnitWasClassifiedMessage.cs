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
using Rock.Configuration;
using Rock.Logging;
using Rock.Model;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Cache Update Message
    /// </summary>
    public class GivingUnitWasClassifiedMessage : IEventMessage<GivingEventQueue>
    {
        /// <summary>
        /// Gets or sets the person ids.
        /// </summary>
        /// <value>
        /// The person ids.
        /// </value>
        public List<int> PersonIds { get; set; }

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
        /// <param name="personIds">The person ids.</param>
        public static void Publish( IEnumerable<int> personIds )
        {
            var list = personIds?.ToList();

            if ( list?.Any() != true )
            {
                // Don't publish if there are no person IDs
                return;
            }

            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish events until Rock is all the way started
                var logMessage = $"'Giving Unit Was Classified' message was not published because Rock is not fully started yet.";

                var elapsedSinceProcessStarted = RockDateTime.Now - RockApp.Current.HostingSettings.ApplicationStartDateTime;

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

            var message = new GivingUnitWasClassifiedMessage
            {
                PersonIds = list
            };

            _ = RockMessageBus.PublishAsync<GivingEventQueue, GivingUnitWasClassifiedMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Published 'Giving Unit Was Classified' message." );
        }
    }
}

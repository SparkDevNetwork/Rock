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
using Rock.Configuration;
using Rock.Logging;
using Rock.Model;

namespace Rock.Web
{
    /// <summary>
    /// Page Route Was Updated Consumer
    /// </summary>
    public sealed class PageRouteWasUpdatedConsumer : RockConsumer<PageRouteEventQueue, PageRouteWasUpdatedMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageRouteWasUpdatedConsumer"/> class.
        /// </summary>
        public PageRouteWasUpdatedConsumer()
        {
        }

        /// <inheritdoc />
        public override void Consume( PageRouteWasUpdatedMessage message )
        {
            // Check if Rock is started
            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish events until Rock is all the way started
                var logMessage = $"'Page Route Was Updated' message was not consumed because Rock is not fully started.";

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

            // Do not reregister the routes is the message was sent from this node as that has already been done.
            if ( RockMessageBus.IsFromSelf( message ) )
            {
                RockLogger.Log.Debug( RockLogDomains.Bus, $"Skipping 'Page Route Was Updated Message' because this node ({message.SenderNodeName}) was the publisher." );
                return;
            }

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Consumed 'Page Route Was Updated Message' on node {RockMessageBus.NodeName}." );
            RockRouteHandler.RemoveRockPageRoutes();
            RockRouteHandler.RegisterRoutes();
        }
    }
}

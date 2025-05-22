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
using Microsoft.Extensions.Logging;

using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Event bus message class used to indicate that the list of restricted countries was updated. The consumer should
    /// reinitialize the list when received.
    /// </summary>
    [RockLoggingCategory]
    public class RestrictedCountriesWereUpdatedMessage : IEventMessage<IpGeolocationEventQueue>
    {
        /// <inheritdoc/>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        public static void Publish()
        {
            /*
                4/17/2025 - JPH

                In the case of publishing a RestrictedCountriesWereUpdatedMessage, we don't need to check
                RockMessageBus.IsRockStarted, as this message's publish and consume logic doesn't have a dependency on
                having Rock fully started.

                Also, we really need to publish these messages regardless of IsRockStarted to prevent restricted
                countries caches on other servers from getting stale.

                If we later discover that this isn't OK, we'll revisit this decision and make any updates to make it OK again.

                Reason: Ensure caches on other servers don't become stale.
            */

            var message = new RestrictedCountriesWereUpdatedMessage();

            _ = RockMessageBus.PublishAsync<IpGeolocationEventQueue, RestrictedCountriesWereUpdatedMessage>( message );

            RockLogger.LoggerFactory.CreateLogger<RestrictedCountriesWereUpdatedMessage>()
                .LogDebug( $"Published 'Restricted Countries Were Updated' message." );
        }
    }
}

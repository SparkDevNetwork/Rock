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

using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Net.Geolocation
{
    /// <summary>
    /// Event bus consumer class to consume <see cref="RestrictedCountriesWereUpdatedMessage"/> messages.
    /// </summary>
    [RockLoggingCategory]
    public sealed class RestrictedCountriesWereUpdatedConsumer : RockConsumer<IpGeolocationEventQueue, RestrictedCountriesWereUpdatedMessage>
    {
        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( RestrictedCountriesWereUpdatedMessage message )
        {
            if ( RockMessageBus.IsFromSelf( message ) )
            {
                Logger.LogDebug( $"Skipping 'Restricted Countries Were Updated' message because this node ({message.SenderNodeName}) was the publisher." );
                return;
            }

            Logger.LogDebug( $"Consumed 'Restricted Countries Were Updated' message on node {RockMessageBus.NodeName}." );

            IpGeoLookup.ReinitializeGloballyRestrictedCountryCodes();
        }
    }
}

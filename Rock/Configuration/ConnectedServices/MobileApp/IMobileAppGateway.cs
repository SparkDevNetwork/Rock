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

using System.Threading;
using System.Threading.Tasks;

using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects;

namespace Rock.Configuration.ConnectedServices.MobileApp
{
    /// <summary>
    /// The calls the Connected Services card makes to enroll the church in the shared
    /// mobile application or take it out again, and the call the daily campus job makes
    /// to keep the church's campuses current.
    /// </summary>
    /// <remarks>
    /// ARGUS-LIVE: This seam exists only so the card can be built and tested before
    /// Spark's gateway answers on the mobile-app service. Once the gateway routes are live,
    /// decide whether the fake stays as a development aid or goes, and if it goes, delete
    /// this interface, <see cref="FakeMobileAppGateway"/> and <see cref="MobileAppGatewayFactory"/>
    /// and call the provider directly the way the Rock IQ card does.
    /// </remarks>
    internal interface IMobileAppGateway
    {
        /// <summary>
        /// <c>true</c> when this gateway never leaves this server. The card shows a notice
        /// so nobody mistakes a fake enrollment for a real directory listing.
        /// </summary>
        bool IsFake { get; }

        /// <summary>
        /// Turns the mobile-app service on or off for the church.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable the service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the call.</returns>
        Task<ConfigurationResult<SetEnabledResponse>> SetEnabledAsync( bool enabled, CancellationToken cancellationToken );

        /// <summary>
        /// Writes the church's directory details, creating the listing on the first call.
        /// </summary>
        /// <param name="request">The church's details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the call, carrying the church code and the poster link.</returns>
        Task<ConfigurationResult<MobileAppConfigurationResponse>> SetConfigurationAsync( MobileAppConfigurationRequest request, CancellationToken cancellationToken );

        /// <summary>
        /// Replaces the church's campus set in the directory. Called by the daily campus job
        /// when the campuses have changed since they were last sent.
        /// </summary>
        /// <param name="request">The church's full campus set.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the call, carrying the directory's campus counts.</returns>
        Task<ConfigurationResult<MobileAppCampusesResponse>> SetCampusesAsync( MobileAppCampusesRequest request, CancellationToken cancellationToken );
    }
}

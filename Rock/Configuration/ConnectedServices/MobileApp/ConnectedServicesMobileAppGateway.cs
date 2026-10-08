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

using System;
using System.Threading;
using System.Threading.Tasks;

using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects;

namespace Rock.Configuration.ConnectedServices.MobileApp
{
    /// <summary>
    /// The real gateway: Spark's API gateway, reached through the Connected
    /// Services provider with the organization's token.
    /// </summary>
    internal class ConnectedServicesMobileAppGateway : IMobileAppGateway
    {
        /// <summary>
        /// The provider that makes the calls.
        /// </summary>
        private readonly ConnectedServicesProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedServicesMobileAppGateway"/> class.
        /// </summary>
        /// <param name="provider">The provider that makes the calls.</param>
        public ConnectedServicesMobileAppGateway( ConnectedServicesProvider provider )
        {
            _provider = provider ?? throw new ArgumentNullException( nameof( provider ) );
        }

        /// <inheritdoc/>
        public bool IsFake => false;

        /// <inheritdoc/>
        public Task<ConfigurationResult<SetEnabledResponse>> SetEnabledAsync( bool enabled, CancellationToken cancellationToken )
        {
            return _provider.SetMobileAppEnabledAsync( enabled, cancellationToken );
        }

        /// <inheritdoc/>
        public Task<ConfigurationResult<MobileAppConfigurationResponse>> SetConfigurationAsync( MobileAppConfigurationRequest request, CancellationToken cancellationToken )
        {
            return _provider.SetMobileAppConfigurationAsync( request, cancellationToken );
        }

        /// <inheritdoc/>
        public Task<ConfigurationResult<MobileAppCampusesResponse>> SetCampusesAsync( MobileAppCampusesRequest request, CancellationToken cancellationToken )
        {
            return _provider.SetMobileAppCampusesAsync( request, cancellationToken );
        }
    }
}

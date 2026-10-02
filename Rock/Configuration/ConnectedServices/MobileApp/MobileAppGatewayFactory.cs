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

using Rock.Enums.Configuration;
using Rock.Web;

namespace Rock.Configuration.ConnectedServices.MobileApp
{
    /// <summary>
    /// Picks the gateway the Connected Services card enrolls through.
    /// </summary>
    internal static class MobileAppGatewayFactory
    {
        /*
            10/2/2026 - CLAUDE

            Spark's gateway does not answer on the mobile-app service yet, and there is
            no Argus sandbox (card impl spec, section 3), so a real enable would either
            fail or put a test church into the live directory. A development machine
            (compilation debug="true") therefore enrolls through a fake that never leaves
            the server. Every other Rock uses the real gateway, so a production church
            sees the gateway's own error until Spark turns the service on.

            Reason: Build and test the card before the gateway and Argus exist.
        */

        // ARGUS-LIVE: Once the gateway answers on the mobile-app service, decide whether a
        // development machine should keep the fake (safer: test churches stay out of the live
        // directory) or reach the real gateway. If the fake goes, delete this factory and use
        // ConnectedServicesMobileAppGateway, or the provider directly, everywhere it is called.

        /// <summary>
        /// Gets the gateway for this Rock.
        /// </summary>
        /// <param name="provider">The Connected Services provider the real gateway calls through.</param>
        /// <returns>The fake gateway on a development machine, otherwise the real one.</returns>
        public static IMobileAppGateway Create( ConnectedServicesProvider provider )
        {
            var isFakeUsed = RockApp.Current.HostingSettings.IsDevelopmentEnvironment
                && RockApp.Current.InitializationSettings.DeploymentEnvironment != DeploymentEnvironment.Demo;

            return Create( provider, isFakeUsed );
        }

        /// <summary>
        /// Gets the real or the fake gateway, as asked.
        /// </summary>
        /// <param name="provider">The Connected Services provider the real gateway calls through.</param>
        /// <param name="isFakeUsed"><c>true</c> for the fake gateway.</param>
        /// <returns>The gateway.</returns>
        internal static IMobileAppGateway Create( ConnectedServicesProvider provider, bool isFakeUsed )
        {
            if ( isFakeUsed )
            {
                return new FakeMobileAppGateway( SystemSettings.GetRockInstanceId() );
            }

            return new ConnectedServicesMobileAppGateway( provider );
        }
    }
}

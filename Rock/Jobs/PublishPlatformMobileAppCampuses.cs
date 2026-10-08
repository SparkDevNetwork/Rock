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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.MobileApp;
using Rock.Mobile;

namespace Rock.Jobs
{
    /// <summary>
    /// Publishes the church's active campuses to the church directory for the shared mobile
    /// application, so a near-me search there can find the church. Only sends when the
    /// campuses the directory would receive have changed since the last send.
    /// </summary>
    /// <remarks>
    /// A church that is not enrolled in the shared mobile application is skipped, so the job
    /// does nothing on most Rock servers.
    /// </remarks>
    [DisplayName( "Publish Platform Mobile App Campuses" )]
    [Description( "Publishes the church's active campuses to the church directory for the shared mobile application when they change. Does nothing unless the church is enrolled in the shared mobile application." )]
    public class PublishPlatformMobileAppCampuses : RockJob
    {
        /*
            10/8/2026 - CLAUDE

            A job rather than a Campus save hook (main spec 5.16). A campus's address lives on
            its Location, so editing the street address never touches the Campus row, and the
            coordinates only arrive later from the LocationServicesVerify job. A job re-reads
            the current state on every run and picks the coordinates up whenever they land,
            and a slow directory never stalls an administrator saving a campus.

            Reason: Campus coordinates arrive by batch, so a batch job is the right trigger.
        */

        /// <inheritdoc/>
        public override void Execute()
        {
            // Start a task that will let us run the async methods in order.
            var task = Task.Run( () => PublishAsync() );

            var result = task.GetAwaiter().GetResult();

            if ( !result.IsSuccess )
            {
                throw new Exception( result.Message );
            }

            UpdateLastStatusMessage( result.Message );
        }

        /// <summary>
        /// Sends the campuses if they have changed.
        /// </summary>
        /// <returns>The result of the run.</returns>
        private async Task<PlatformMobileAppCampusPublishResult> PublishAsync()
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();
            var gateway = MobileAppGatewayFactory.Create( provider );

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                return await PlatformMobileAppCampusPublisher.PublishIfChangedAsync( gateway, provider, rockContext, CancellationToken.None );
            }
        }
    }
}

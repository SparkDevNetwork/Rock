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
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Enums.Mobile;
using Rock.Mobile;
using Rock.Security;
using Rock.ViewModels.Blocks.Mobile.PlatformMobileAppDetail;

namespace Rock.Blocks.Mobile
{
    /// <summary>
    /// The control panel for the platform mobile application: builds, updates and repairs it
    /// from the definition compiled into this Rock, and shows the last run's report.
    /// </summary>
    /// <remarks>
    /// Only the platform half of the control panel lives here so far. The church settings
    /// half (the in-app logo, the colors, the church's picks for each screen) is not yet
    /// specified, and the page this block sits on is still to be decided.
    /// </remarks>
    [DisplayName( "Platform Mobile App Detail" )]
    [Category( "Mobile" )]
    [Description( "Builds, updates and repairs the platform mobile application." )]
    [IconCssClass( "ti ti-device-mobile" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "9E1D5938-6486-48C9-9DCF-C585784FDA9E" )]
    [SystemGuid.BlockTypeGuid( "C51C36B5-A3A6-4552-9C7C-17C8F968DBC9" )]
    internal class PlatformMobileAppDetail : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBag();
        }

        /// <summary>
        /// Gets the builder state and the last run's report for the current person.
        /// </summary>
        /// <returns>The initialization bag.</returns>
        private PlatformMobileAppDetailInitializationBag GetInitializationBag()
        {
            var stamp = PlatformMobileAppBuilder.GetStampVersion( RockContext );
            var lastRun = PlatformMobileAppBuilder.GetLastRunReport( RockContext );

            return new PlatformMobileAppDetailInitializationBag
            {
                State = PlatformMobileAppBuilder.GetState( RockContext ),
                StampVersion = stamp?.ToString(),
                CurrentDefinitionVersion = PlatformMobileAppBuilder.CurrentDefinitionVersion.ToString(),
                IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ),
                LastRun = lastRun != null ? ToBag( lastRun ) : null
            };
        }

        /// <summary>
        /// Converts a run report to the bag sent to the client.
        /// </summary>
        /// <param name="report">The run report.</param>
        /// <returns>The bag.</returns>
        private static PlatformMobileAppRunReportBag ToBag( PlatformMobileAppRunReport report )
        {
            return new PlatformMobileAppRunReportBag
            {
                Mode = report.Mode,
                StartedDateTime = report.StartedDateTime.ToRockDateTimeOffset(),
                CompletedDateTime = report.CompletedDateTime?.ToRockDateTimeOffset(),
                IsSuccess = report.IsSuccess,
                Versions = report.Versions
                    .Select( v => new PlatformMobileAppVersionReportBag
                    {
                        Version = v.Version,
                        Created = v.Created,
                        Updated = v.Updated,
                        Error = v.Error
                    } )
                    .ToList(),
                UnexpectedRecords = report.UnexpectedRecords,
                DeployError = report.DeployError
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Runs the builder: Build, Update or Repair, whichever the current state calls for,
        /// then returns the refreshed state. All three are safe to repeat, so none asks for
        /// confirmation.
        /// </summary>
        /// <returns>The refreshed initialization bag.</returns>
        [BlockAction]
        public async Task<BlockActionResult> Run()
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to build the platform mobile application." );
            }

            if ( PlatformMobileAppBuilder.GetState( RockContext ) == PlatformMobileAppBuildState.AheadOfCode )
            {
                return ActionBadRequest( "This database is ahead of this Rock's builder, so the builder cannot run. Run the Rock version the database came from." );
            }

            try
            {
                await PlatformMobileAppBuilder.RunAsync();
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }

            return ActionOk( GetInitializationBag() );
        }

        #endregion Block Actions
    }
}

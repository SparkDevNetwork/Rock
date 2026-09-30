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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Enums.Mobile;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Mobile.PlatformMobileAppDetail;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Mobile
{
    /// <summary>
    /// The control panel for the platform mobile application: builds, updates and repairs it
    /// from the definition compiled into this Rock, and shows the last run's report.
    /// </summary>
    /// <remarks>
    /// The church settings half holds the in-app logos, the color pair and the content
    /// collection, all written through <see cref="PlatformMobileAppChurchSettings"/>. The
    /// other per-screen picks arrive with their screens, and the page this block sits on is
    /// still to be decided.
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
                LastRun = lastRun != null ? ToBag( lastRun ) : null,
                Settings = stamp != null ? GetSettingsBag() : null,
                ContentCollectionOptions = GetContentCollectionOptions()
            };
        }

        /// <summary>
        /// Gets the church owned settings as they are saved now.
        /// </summary>
        /// <returns>The settings bag.</returns>
        private PlatformMobileAppSettingsBag GetSettingsBag()
        {
            var contentCollectionGuid = PlatformMobileAppChurchSettings.GetBlockValue(
                SystemGuid.PlatformMobileApp.Block.CONTENT_COLLECTION_VIEW.AsGuid(),
                PlatformMobileAppChurchSettings.BlockKey.ContentCollection,
                RockContext ).AsGuidOrNull();

            var site = new SiteService( RockContext ).Get( SystemGuid.Site.PLATFORM_MOBILE_APPLICATION.AsGuid() );
            var branding = site != null ? PlatformMobileAppChurchSettings.GetBranding( site ) : new PlatformMobileAppBranding();

            return new PlatformMobileAppSettingsBag
            {
                ContentCollectionGuid = contentCollectionGuid,
                LightLogo = BinaryFileToListItemBag( branding.LightLogoBinaryFileId ),
                DarkLogo = BinaryFileToListItemBag( branding.DarkLogoBinaryFileId ),
                ColorStrong = branding.ColorStrong,
                ColorSoft = branding.ColorSoft
            };
        }

        /// <summary>
        /// Builds the list item an image uploader shows for a binary file.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier, or <c>null</c>.</param>
        /// <returns>The list item, or <c>null</c> if there is no file.</returns>
        private ListItemBag BinaryFileToListItemBag( int? binaryFileId )
        {
            if ( !binaryFileId.HasValue )
            {
                return null;
            }

            var data = new BinaryFileService( RockContext )
                .GetSelect( binaryFileId.Value, b => new { b.Guid, b.FileName } );

            return data == null
                ? null
                : new ListItemBag { Value = data.Guid.ToString(), Text = data.FileName };
        }

        /// <summary>
        /// Gets the content collections the church can pick for the app's content page, by
        /// name.
        /// </summary>
        /// <returns>The collections as list items.</returns>
        private List<ListItemBag> GetContentCollectionOptions()
        {
            return new ContentCollectionService( RockContext ).Queryable()
                .OrderBy( c => c.Name )
                .ThenBy( c => c.Id )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Marks an uploaded binary file permanent, the way the Styles tab does for its logos.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier, or <c>null</c>.</param>
        private void MarkBinaryFilePermanent( int? binaryFileId )
        {
            if ( !binaryFileId.HasValue )
            {
                return;
            }

            var binaryFile = new BinaryFileService( RockContext ).Get( binaryFileId.Value );

            if ( binaryFile != null )
            {
                binaryFile.IsTemporary = false;
            }
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

        /// <summary>
        /// Saves the church owned settings, then runs the builder once so every change is
        /// applied and the app is deployed a single time.
        /// </summary>
        /// <param name="bag">The settings as edited.</param>
        /// <returns>The refreshed initialization bag.</returns>
        [BlockAction]
        public async Task<BlockActionResult> SaveSettings( PlatformMobileAppSettingsBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No settings were sent." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to change the platform mobile application." );
            }

            var state = PlatformMobileAppBuilder.GetState( RockContext );

            // The choice lives on the platform Site, and a database ahead of the code cannot be run.
            if ( state == PlatformMobileAppBuildState.NotBuilt || state == PlatformMobileAppBuildState.AheadOfCode )
            {
                return ActionBadRequest( "The platform mobile application must be built before its settings can be changed." );
            }

            var contentCollectionGuid = bag.ContentCollectionGuid;
            var isKnownCollection = !contentCollectionGuid.HasValue
                || new ContentCollectionService( RockContext ).Queryable().Any( c => c.Guid == contentCollectionGuid.Value );

            if ( !isKnownCollection )
            {
                return ActionBadRequest( "That content collection no longer exists." );
            }

            if ( bag.ColorStrong.IsNullOrWhiteSpace() || bag.ColorSoft.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Both the strong and the soft color are required." );
            }

            var site = new SiteService( RockContext ).Get( SystemGuid.Site.PLATFORM_MOBILE_APPLICATION.AsGuid() );

            if ( site == null )
            {
                return ActionBadRequest( "The platform mobile application has not been built." );
            }

            var lightLogoId = bag.LightLogo.GetEntityId<BinaryFile>( RockContext );
            var darkLogoId = bag.DarkLogo.GetEntityId<BinaryFile>( RockContext );

            // The control panel writes only through the skip list, which refuses any platform owned setting.
            PlatformMobileAppChurchSettings.SaveBlockValue(
                SystemGuid.PlatformMobileApp.Block.CONTENT_COLLECTION_VIEW.AsGuid(),
                PlatformMobileAppChurchSettings.BlockKey.ContentCollection,
                contentCollectionGuid?.ToString() ?? string.Empty,
                RockContext );

            PlatformMobileAppChurchSettings.SaveBranding( site, new PlatformMobileAppBranding
            {
                LightLogoBinaryFileId = lightLogoId,
                DarkLogoBinaryFileId = darkLogoId,
                ColorStrong = bag.ColorStrong.Trim(),
                ColorSoft = bag.ColorSoft.Trim()
            } );

            // Uploaded images start temporary and are cleaned up unless something keeps them.
            MarkBinaryFilePermanent( lightLogoId );
            MarkBinaryFilePermanent( darkLogoId );

            RockContext.SaveChanges();

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

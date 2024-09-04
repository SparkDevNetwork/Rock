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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ScheduledJobList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of service jobs.
    /// </summary>
    [DisplayName( "Scheduled Job List" )]
    [Category( "Core" )]
    [Description( "Lists all scheduled jobs." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the service job details.",
        Key = AttributeKey.DetailPage )]

    [LinkedPage( "History Page",
        Description = "The page to display group history.",
        Key = AttributeKey.HistoryPage )]

    [Rock.SystemGuid.EntityTypeGuid( "d72e22ca-040d-4de9-b2e0-438ba70ba91a" )]
    [Rock.SystemGuid.BlockTypeGuid( "9b90f2d1-0c7b-4f08-a808-8ba4c9a70a20" )]
    [CustomizedGrid]
    public class ScheduledJobList : RockEntityListBlockType<ServiceJob>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string HistoryPage = "HistoryPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string HistoryPage = "HistoryPage";
        }

        private static class PreferenceKey
        {
            public const string FilterName = "filter-name";
            public const string FilterIsActive = "filter-is-active";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the name of the job(s) to include in the result.
        /// </summary>
        /// <value>
        /// The name filter.
        /// </value>
        protected string FilterName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterName );

        /// <summary>
        /// If true only active jobs are included in the result.
        /// </summary>
        /// <value>
        /// The is active filter.
        /// </value>
        protected string FilterIsActive => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIsActive );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ScheduledJobListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ScheduledJobListOptionsBag GetBoxOptions()
        {
            var options = new ScheduledJobListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ServiceJobId", "((Key))" ),
                [NavigationUrlKey.HistoryPage] = this.GetLinkedPageUrl( AttributeKey.HistoryPage, new Dictionary<string, string>
                {
                    { "ScheduledJobId", "((Key))" }
                })
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ServiceJob> GetListQueryable( RockContext rockContext )
        {
            var jobService = new ServiceJobService( new RockContext() );

            var queryable = jobService.GetAllJobs();

            if ( FilterName.IsNotNullOrWhiteSpace() )
            {
                queryable = queryable.Where( a => a.Name.Contains( FilterName ) );
            }

            if ( FilterIsActive.IsNotNullOrWhiteSpace() )
            {
                var activeStatus = FilterIsActive.AsBoolean();
                queryable = queryable.Where( a => a.IsActive == activeStatus );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<ServiceJob> GetOrderedListQueryable( IQueryable<ServiceJob> queryable, RockContext rockContext )
        {
            return  queryable.OrderByDescending( a => a.LastRunDateTime ).ThenBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ServiceJob> GetGridBuilder()
        {
            return new GridBuilder<ServiceJob>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "lastSuccessfulRun", a => a.LastSuccessfulRunDateTime )
                .AddDateTimeField( "lastRunDateTime", a => a.LastRunDateTime )
                .AddTextField( "lastRunDurationSeconds", a => FormatDuration( a.LastRunDurationSeconds ) )
                .AddTextField( "lastStatus", a => a.LastStatus )
                .AddTextField( "lastStatusMessage", a => a.LastStatusMessage )
                .AddTextField( "lastStatusMessageAsHtml", a => a.LastStatusMessageAsHtml )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isActive", a => a.IsActive )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "guid", a => a.Guid.ToString().ToUpper() );
        }

        /// <summary>
        /// Formats the Last Run Duration Seconds as human friendly text.
        /// </summary>
        /// <param name="lastRunDurationSeconds">The last run duration seconds.</param>
        /// <returns></returns>
        private string FormatDuration( int? lastRunDurationSeconds )
        {
            var text = string.Empty;

            if ( lastRunDurationSeconds.HasValue )
            {
                int durationSeconds = lastRunDurationSeconds.Value;
                TimeSpan duration = TimeSpan.FromSeconds( durationSeconds );

                if ( duration.Days > 0 )
                {
                    text = $"{duration.TotalHours:F2} hours";
                }
                else if ( duration.Hours > 0 )
                {
                    text = $"{duration:%h}h {duration:%m}m {duration:%s}s";
                }
                else if ( duration.Minutes > 0 )
                {
                    text = $"{duration:%m}m {duration:%s}s";
                }
                else
                {
                    text = $"{duration:%s}s";
                }
            }

            return text;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ServiceJobService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{ServiceJob.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {ServiceJob.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Runs the specified job.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [BlockAction]
        public async Task<BlockActionResult> RunNow( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ServiceJobService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity != null )
                {
                    new ProcessRunJobNow.Message { JobId = entity.Id }.Send();
                    // wait a split second for the job to start so that the grid will show the status (if it changed)
                    await Task.Delay( 250 );
                }

                return ActionOk();
            }
        }

        #endregion
    }
}

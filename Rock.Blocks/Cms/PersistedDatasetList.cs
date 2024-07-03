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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PersistedDatasetList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of persisted datasets.
    /// </summary>

    [DisplayName( "Persisted Dataset List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of persisted datasets." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the persisted dataset details.",
        Key = AttributeKey.DetailPage )]

    [DecimalField(
        "Max Preview Size (MB)",
        Key = AttributeKey.MaxPreviewSizeMB,
        Description = "If the JSON data is large, it could cause the browser to timeout.",
        IsRequired = true,
        DefaultDecimalValue = 1,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "dc11e26e-7e4a-4550-af2d-2c9b94beed4e" )]
    [Rock.SystemGuid.BlockTypeGuid( "cfbb4daf-1aeb-4095-8098-e3a82e30fa7e" )]
    [CustomizedGrid]
    public class PersistedDatasetList : RockEntityListBlockType<PersistedDataset>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string MaxPreviewSizeMB = "MaxPreviewSizeMB";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersistedDatasetListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
        private PersistedDatasetListOptionsBag GetBoxOptions()
        {
            var options = new PersistedDatasetListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PersistedDatasetId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersistedDataset> GetListQueryable( RockContext rockContext )
        {
            var persistedDatasetService = new PersistedDatasetService( rockContext );

            // Use AsNoTracking() since these records won't be modified
            var qry = persistedDatasetService.Queryable().AsNoTracking();

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersistedDataset> GetGridBuilder()
        {
            return new GridBuilder<PersistedDataset>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField("id", a => a.Id)
                .AddTextField( "name", a => a.Name )
                .AddField( "lastRefreshDateTime", a => a.LastRefreshDateTime )
                .AddTextField( "accessKey", a => a.AccessKey )
                .AddField( "timeToBuildMS", a => a.TimeToBuildMS.HasValue ? Math.Round( ( double ) a.TimeToBuildMS ).ToString() : "-" )
                .AddField("allowManualRefresh", a => a.AllowManualRefresh)
                .AddTextField( "resultData", a => a.ResultData )
                .AddField( "resultSize", a => a.ResultData != null ? a.ResultData.Length / 1024 : 0 )
                .AddField( "isSystem", a => a.IsSystem );
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult RefreshDataset( string datasetId )
        {
            using ( var rockContext = new RockContext() )
            {
                var persistedDatasetService = new PersistedDatasetService( rockContext );
                var persistedDataset = persistedDatasetService.Get( datasetId );

                if ( persistedDataset == null )
                {
                    return ActionNotFound();
                }

                // Refresh the dataset and save changes
                persistedDataset.UpdateResultData();
                rockContext.SaveChanges();

                // Update the cache
                PersistedDatasetCache.UpdateCachedEntity( persistedDataset.Id, EntityState.Modified );

                return ActionOk();
            }
        }

        [BlockAction]
        public BlockActionResult PreviewDataset( string datasetId )
        {
            using ( var rockContext = new RockContext() )
            {
                var persistedDatasetService = new PersistedDatasetService( rockContext );
                var persistedDataset = persistedDatasetService.GetNoTracking( datasetId );

                if ( persistedDataset == null )
                {
                    return ActionNotFound();
                }

                // Ensure data is refreshed if needed
                if ( persistedDataset.LastRefreshDateTime == null )
                {
                    persistedDataset.UpdateResultData();
                }

                // Get max preview size from block settings (default 1MB)
                var maxPreviewSizeMB = this.GetAttributeValue( AttributeKey.MaxPreviewSizeMB ).AsDecimalOrNull() ?? 1;
                maxPreviewSizeMB = Math.Max( 1, maxPreviewSizeMB );
                var maxPreviewSizeLength = ( int ) ( maxPreviewSizeMB * 1024 * 1024 );

                // Truncate data if it exceeds max size
                var previewData = persistedDataset.ResultData;
                if ( previewData.Length > maxPreviewSizeLength )
                {
                    previewData = previewData.Substring( 0, maxPreviewSizeLength );
                }

                var response = new
                {
                    PreviewData = previewData,
                    TimeToBuildMS = persistedDataset.TimeToBuildMS
                };

                return ActionOk( response );
            }
        }

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
                var entityService = new PersistedDatasetService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{PersistedDataset.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {PersistedDataset.FriendlyTypeName}." );
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

        #endregion
    }
}

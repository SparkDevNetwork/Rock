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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Model.CMS.ContentChannelItem.Options;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelItemList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of content channel items.
    /// </summary>

    [DisplayName( "Content Channel Item List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of content channel items." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware]

    #region Block Attributes

    #region General Settings

    [ContentChannelField(
        "Content Channel",
        Key = AttributeKey.ContentChannel,
        Description = "If set the block will ignore content channel query parameters",
        Category = AttributeCategory.GeneralSettings,
        Order = 0,
        IsRequired = false )]

    [BooleanField(
        "Filter Items For Current User",
        Key = AttributeKey.FilterItemsForCurrentUser,
        Description = "Filters the items by those created by the current logged in user.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.GeneralSettings,
        Order = 1,
        IsRequired = false )]

    [BooleanField(
        "Show Filters",
        Key = AttributeKey.ShowFilters,
        Description = "Allows you to show/hide the grids filters.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 2,
        IsRequired = false )]

    [BooleanField(
        "Show Event Occurrences Column",
        Key = AttributeKey.ShowEventOccurrencesColumn,
        Description = "Determines if the column that lists event occurrences should be shown if any of the items has an event occurrence.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 3,
        IsRequired = false )]

    [BooleanField(
        "Show Total Views Column",
        Key = AttributeKey.ShowTotalViewsColumns,
        Description = "Determines if the total views column should be shown.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 4,
        IsRequired = false )]

    [BooleanField(
        "Show Priority Column",
        Key = AttributeKey.ShowPriorityColumn,
        Description = "Determines if the column that displays priority should be shown for content channels that have Priority enabled.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 5,
        IsRequired = false )]

    [BooleanField(
        "Show Item URL Column",
        Key = AttributeKey.ShowItemUrlColumn,
        Description = "Determines if the item URL column should be shown.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 6,
        IsRequired = false )]

    [BooleanField(
        "Show Linked Media Column",
        Key = AttributeKey.ShowLinkedMediaColumn,
        Description = "Determines if the linked media column should be shown.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 7,
        IsRequired = false )]

    [BooleanField(
        "Show Security Column",
        Key = AttributeKey.ShowSecurityColumn,
        Description = "Determines if the security column should be shown.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.GeneralSettings,
        Order = 8,
        IsRequired = false )]

    #endregion General Settings

    #region Pages

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.Pages,
        Order = 0,
        IsRequired = true )]

    #endregion Pages

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "5597badd-bb0e-4bcd-be1f-5acf230cf428" )]
    [Rock.SystemGuid.BlockTypeGuid( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "93dc73c4-545d-40b9-bfea-1cec04c07eb1" )]
    [CustomizedGrid]
    public class ContentChannelItemList : RockEntityListBlockType<ContentChannelItem>
    {

        private ContentChannel SelectedContentChannel { get; set; }

        #region Keys

        private static class AttributeKey
        {
            // General Settings
            public const string ContentChannel = "ContentChannel";
            public const string FilterItemsForCurrentUser = "FilterItemsForCurrentUser";
            public const string ShowFilters = "ShowFilters";
            public const string ShowEventOccurrencesColumn = "ShowEventOccurrencesColumn";
            public const string ShowTotalViewsColumns = "ShowTotalViewsColumns";
            public const string ShowPriorityColumn = "ShowPriorityColumn";
            public const string ShowItemUrlColumn = "ShowItemUrlColumn";
            public const string ShowLinkedMediaColumn = "ShowLinkedMediaColumn";
            public const string ShowSecurityColumn = "ShowSecurityColumn";

            // Pages
            public const string DetailPage = "DetailPage";
        }

        private static class AttributeCategory
        {
            public const string GeneralSettings = "";
            public const string Pages = "Pages";
        }

        private static class SqlParamKey
        {
            public const string MediumDefinedValueId = "@MediumDefinedValueId";
            public const string ContentChannelId = "@ContentChannelId";
            public const string EntityMetadataKey = "@EntityMetadataKey";
            public const string ContentChannelItemEntityTypeId = "@ContentChannelItemEntityTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string NewItemPage = "NewItemPage";
            public const string LibraryDownloadPage = "LibraryDownloadPage";
            public const string MediaElementPage = "MediaElementPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var contentChannel = GetContentChannel();

            if ( contentChannel == null )
            {
                return null;
            }

            GetContextEntity();

            var box = new ListBlockBox<ContentChannelItemListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = GetIsAddEnabled();
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
        private ContentChannelItemListOptionsBag GetBoxOptions()
        {
            var contentChannel = GetContentChannel();

            if ( contentChannel == null )
            {
                return null;
            }

            var isFiltered = IsFiltered();
            var licenseGuid = contentChannel.ContentLibraryConfiguration?.LicenseTypeValueGuid ?? Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_OPEN.AsGuid();

            var options = new ContentChannelItemListOptionsBag
            {
                ContentItemName = contentChannel.Name,
                IncludeTime = contentChannel.ContentChannelType.IncludeTime,
                IsManuallyOrdered = contentChannel.ItemsManuallyOrdered,
                DateType = contentChannel.ContentChannelType.DateRangeType,
                ContentChannelId = contentChannel.Id,
                ShowFilters = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean(),
                IsContentLibraryEnabled = contentChannel.ContentLibraryConfiguration?.IsEnabled == true,
                LibraryLicenseGuid = licenseGuid,
                LibraryLicenseName = DefinedValueCache.Get( licenseGuid ).Value,

                ShowReorderColumn = !isFiltered && contentChannel.ItemsManuallyOrdered,
                ShowStartDateTimeColumn = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.SingleDate
                    || contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange,
                ShowExpireDateTimeColumn = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange,
                ShowTotalViewsColumns = GetAttributeValue( AttributeKey.ShowTotalViewsColumns ).AsBoolean(),
                ShowPriorityColumn = !contentChannel.ContentChannelType.DisablePriority
                    && GetAttributeValue( AttributeKey.ShowPriorityColumn ).AsBoolean(),
                ShowOccurrencesColumn = GetAttributeValue( AttributeKey.ShowEventOccurrencesColumn ).AsBoolean(),
                ShowStatusColumn = contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus,
                ShowItemUrlColumn = GetAttributeValue( AttributeKey.ShowItemUrlColumn ).AsBoolean()
                    && contentChannel.ItemUrl.IsNotNullOrWhiteSpace(),
                ShowLinkedMediaColumn = GetAttributeValue( AttributeKey.ShowLinkedMediaColumn ).AsBoolean(),
                ShowSecurityColumn = GetAttributeValue( AttributeKey.ShowSecurityColumn ).AsBoolean()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new ContentChannelItem();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var contentChannel = GetContentChannel();

            var libraryDownloadUrl = "";
            var pageCache = PageCache.Get( Rock.SystemGuid.Page.LIBRARY_VIEWER.AsGuid() );
            if ( pageCache != null )
            {
                int routeId = 0;
                {
                    var pageRouteInfo = pageCache.PageRoutes.FirstOrDefault( a => a.Guid == Rock.SystemGuid.PageRoute.LIBRARY_VIEWER.AsGuid() );
                    if ( pageRouteInfo != null )
                    {
                        routeId = pageRouteInfo.Id;
                    }
                }

                libraryDownloadUrl = ( new Rock.Web.PageReference( pageCache.Id, routeId, new Dictionary<string, string>
                {
                    { "ContentChannelIdKey", contentChannel.IdKey }
                }, null ) ).BuildUrl();
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["ContentItemId"] = "((Key))",
                    ["autoEdit"] = "true",
                    ["returnUrl"] = this.GetCurrentPageUrl()
                } ),
                [NavigationUrlKey.NewItemPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["ContentItemId"] = "((Key))",
                    ["ContentChannelId"] = contentChannel.IdKey
                } ),
                [NavigationUrlKey.LibraryDownloadPage] = libraryDownloadUrl,
                [NavigationUrlKey.MediaElementPage] = "/admin/cms/media-accounts/items/((Key))"
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ContentChannelItem> GetListQueryable( RockContext rockContext )
        {
            var contentChannel = GetContentChannel();

            if ( contentChannel == null )
            {
                return Enumerable.Empty<ContentChannelItem>().AsQueryable();
            }

            var query = base.GetListQueryable( rockContext ).Where( i => i.ContentChannelId == contentChannel.Id );

            // Filter by person who created content if context entity is a person
            var contextEntity = GetContextEntity();
            Rock.Model.Person person = null;

            if ( contextEntity != null )
            {
                if ( contextEntity is Rock.Model.Person )
                {
                    person = contextEntity as Rock.Model.Person;
                }
            }

            // Filter by person who created content if context entity is a person
            if ( GetAttributeValue( AttributeKey.FilterItemsForCurrentUser ).AsBoolean() )
            {
                person = GetCurrentPerson();
            }

            if ( person != null )
            {
                query = query.Where( i => i.CreatedByPersonAlias != null && i.CreatedByPersonAlias.PersonId == person.Id );
            }

            return query;
        }

        /// <inheritdoc/>
        protected override IQueryable<ContentChannelItem> GetOrderedListQueryable( IQueryable<ContentChannelItem> queryable, RockContext rockContext )
        {
            var contentChannel = GetContentChannel();

            var query = queryable.OrderBy( i => i.Order );

            if ( contentChannel != null && !contentChannel.ItemsManuallyOrdered )
            {
                query = query.OrderByDescending( p => p.StartDateTime );
            }

            return query;
        }

        /// <summary>
        /// Determine if there are any server side filters being applied.
        /// </summary>
        /// <returns>True if there are any filters being applied on the server side, otherwise false.</returns>
        private bool IsFiltered()
        {
            var contextEntity = GetContextEntity();

            return GetAttributeValue( AttributeKey.FilterItemsForCurrentUser ).AsBoolean()
                || ( contextEntity != null && contextEntity is Rock.Model.Person );
        }

        /// <inheritdoc/>
        protected override List<ContentChannelItem> GetListItems( IQueryable<ContentChannelItem> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();
            return items.Where( cci => cci.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) ).ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<ContentChannelItem> GetGridBuilder()
        {
            var contentChannel = GetContentChannel();

            var itemUrlMergeFields = new Dictionary<string, object>
            {
                ["ContentChannelId"] = contentChannel.Id
            };

            var builder = new GridBuilder<ContentChannelItem>()
                .WithBlock( this )
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "contentChannelId", a => a.ContentChannelId )
                .AddField( "order", a => a.Order )
                .AddTextField( "title", a => a.Title )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "expireDateTime", a => a.ExpireDateTime )
                .AddField( "isScheduled", a => a.StartDateTime > RockDateTime.Now )
                .AddField( "occurrences", a => a.EventItemOccurrences.Any() )
                .AddField( "status", a => a.Status )
                .AddField( "priority", a => a.Priority )
                .AddField( "isContentLibraryOwner", a => a.IsContentLibraryOwner )
                .AddField( "contentLibrarySourceIdentifier", a => a.ContentLibrarySourceIdentifier )
                .AddField( "isDownloadedFromContentLibrary", a => a.IsDownloadedFromContentLibrary )
                .AddField( "isUploadedToContentLibrary", a => a.IsUploadedToContentLibrary )
                .AddField( "contentLibraryLicenseTypeGuid", a => a.ContentLibraryLicenseTypeValueId.HasValue ? DefinedValueCache.Get( a.ContentLibraryLicenseTypeValueId.Value )?.Guid : null )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddField( "last28DaysViewsCount", a => 0 )
                .AddTextField( "itemUrl", a =>
                {
                    if ( contentChannel.ItemUrl.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    itemUrlMergeFields.AddOrReplace( "Id", a.Id );
                    itemUrlMergeFields.AddOrReplace( "Title", a.Title );
                    itemUrlMergeFields.AddOrReplace( "Slug", a.PrimarySlug );

                    return contentChannel.ItemUrl.ResolveMergeFields( itemUrlMergeFields );
                } )
                .AddField( "hasLinkedMediaElements", a => false )
                .AddAttributeFields( GetGridAttributes() );

            return builder;
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;
            var contentChannel = GetContentChannel();

            if ( contentChannel == null )
            {
                return new List<AttributeCache>();
            }

            return AttributeCache.All().AsQueryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn && ( (
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( contentChannel.ContentChannelTypeId.ToString() )
                    ) || (
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( contentChannel.Id.ToString() )
                    ) ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ).ToList();
        }

        private ContentChannel GetContentChannel()
        {
            if ( SelectedContentChannel == null )
            {
                if ( GetAttributeValue( AttributeKey.ContentChannel ).IsNotNullOrWhiteSpace() )
                {
                    SelectedContentChannel = new ContentChannelService( new RockContext() ).Get( GetAttributeValue( AttributeKey.ContentChannel ) );
                }
                else
                {
                    SelectedContentChannel = new ContentChannelService( new RockContext() ).Get( RequestContext.GetPageParameter( "ContentChannelId" ) );
                }
            }

            return SelectedContentChannel;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the content channel item list grid data.
        /// </summary>
        /// <returns>A bag containing the content channel item list grid data.</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetContentChannelItemListGridData()
        {
            var contentChannel = GetContentChannel();
            if ( contentChannel == null )
            {
                return ActionBadRequest( $"Unable to find {ContentChannel.FriendlyTypeName}." );
            }

            /*
                3/18/2026 - JPH

                This `GetGridData` block action diverges from the standard grid data pattern.

                It aggregates data from multiple independent sources:
                    1. Base entities;
                    2. View interaction counts;
                    3. Linked media metadata;

                Each source is retrieved in parallel using separate background tasks (each with its own RockContext).
                The results are then merged into the final grid data bag before returning to the client.

                Reason: Document non-standard parallel data retrieval and aggregation approach.
            */

            var getGridDataTask = Task.Run( () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    return GetGridDataBag( rockContext );
                }
            } );

            var tasks = new List<Task> { getGridDataTask };

            Task<Dictionary<int, InteractionCounts>> getInteractionCountsTask = null;
            Task<Dictionary<int, LinkedMediaMetadata>> getLinkedMediaElementsTask = null;

            var showTotalViewsColumns = GetAttributeValue( AttributeKey.ShowTotalViewsColumns ).AsBoolean();
            if ( showTotalViewsColumns )
            {
                getInteractionCountsTask = Task.Run( () =>
                {
                    var mediumDefinedValueGuid = Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid();

                    using ( var rockContext = new RockContext() )
                    {
                        var sql = $@"
SELECT ic.[EntityId],
    COUNT(*) AS [Last28DaysViewsCount]
FROM [Interaction] i
    INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
    INNER JOIN [InteractionChannel] ich ON ich.[Id] = ic.[InteractionChannelId]
    INNER JOIN [ContentChannelItem] cci ON cci.[Id] = ic.[EntityId]
WHERE ich.[ChannelTypeMediumValueId] = {SqlParamKey.MediumDefinedValueId}
    AND cci.[ContentChannelId] = {SqlParamKey.ContentChannelId}
    AND i.[InteractionDateTime] >= DATEADD(DAY, -27, CAST(GETDATE() AS DATE))
GROUP BY ic.[EntityId];";

                        return rockContext.Database
                            .SqlQuery<InteractionCounts>(
                                sql,
                                new SqlParameter( SqlParamKey.MediumDefinedValueId, DefinedValueCache.GetId( mediumDefinedValueGuid ) ),
                                new SqlParameter( SqlParamKey.ContentChannelId, contentChannel.Id )
                            )
                            .ToDictionary( c => c.EntityId );
                    }
                } );

                tasks.Add( getInteractionCountsTask );
            }

            var showLinkedMediaColumn = GetAttributeValue( AttributeKey.ShowLinkedMediaColumn ).AsBoolean();
            if ( showLinkedMediaColumn )
            {
                getLinkedMediaElementsTask = Task.Run( () =>
                {
                    var contentChannelItemEntityGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid();

                    using ( var rockContext = new RockContext() )
                    {
                        var sql = $@"
SELECT em.[EntityId]
    , em.[Value]
FROM [EntityMetadata] em
    INNER JOIN [ContentChannelItem] cci ON cci.[Id] = em.[EntityId]
WHERE em.[Key] = {SqlParamKey.EntityMetadataKey}
    AND em.[EntityTypeId] = {SqlParamKey.ContentChannelItemEntityTypeId}
    AND cci.[ContentChannelId] = {SqlParamKey.ContentChannelId};";

                        return rockContext.Database
                            .SqlQuery<LinkedMediaMetadata>(
                                sql,
                                new SqlParameter( SqlParamKey.EntityMetadataKey, MetadataKey.MediaElements ),
                                new SqlParameter( SqlParamKey.ContentChannelItemEntityTypeId, EntityTypeCache.GetId( contentChannelItemEntityGuid ) ),
                                new SqlParameter( SqlParamKey.ContentChannelId, contentChannel.Id )
                            )
                            .GroupBy( m => m.EntityId )
                            .ToDictionary(
                                g => g.Key,
                                g => g.First()
                            );
                    }
                } );

                tasks.Add( getLinkedMediaElementsTask );
            }

            await Task.WhenAll( tasks );

            var gridDataBag = getGridDataTask.Result;

            if ( tasks.Count > 1 )
            {
                var interactionCounts = showTotalViewsColumns ? getInteractionCountsTask.Result : null;
                var linkedMediaElements = showLinkedMediaColumn ? getLinkedMediaElementsTask.Result : null;

                foreach ( var row in gridDataBag.Rows )
                {
                    if ( !row.TryGetValue( "idKey", out var idKey ) )
                    {
                        continue;
                    }

                    var id = IdHasher.Instance.GetId( idKey.ToString() );
                    if ( !id.HasValue )
                    {
                        continue;
                    }

                    if ( interactionCounts != null && interactionCounts.TryGetValue( id.Value, out var counts ) )
                    {
                        row["last28DaysViewsCount"] = counts.Last28DaysViewsCount;
                    }

                    if ( linkedMediaElements != null && linkedMediaElements.TryGetValue( id.Value, out var metadata ) )
                    {
                        row["hasLinkedMediaElements"] = metadata.Value.IsNotNullOrWhiteSpace();
                    }
                }
            }

            return ActionOk( gridDataBag );
        }

        /// <summary>
        /// Gets information about linked media elements.
        /// </summary>
        /// <param name="bag">The information needed to get linked media elements.</param>
        /// <returns>A bag containing information about linked media elements.</returns>
        [BlockAction]
        public BlockActionResult GetLinkedMediaElements( GetLinkedMediaElementsRequestBag bag )
        {
            if ( ( bag?.ContentChannelItemIdKey ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest();
            }

            var contentChannelItem = new ContentChannelItemService( RockContext )
                .Get( bag.ContentChannelItemIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( contentChannelItem == null )
            {
                return ActionBadRequest( $"Unable to find {ContentChannelItem.FriendlyTypeName}." );
            }

            if ( !contentChannelItem.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( ContentChannelItem.FriendlyTypeName ) );
            }

            var mediaElementIds = contentChannelItem.GetMetadataValue<List<int>>( MetadataKey.MediaElements, RockContext );
            if ( mediaElementIds?.Any() != true )
            {
                return ActionBadRequest( "Unable to find linked media elements" );
            }

            var mediaElements = new MediaElementService( RockContext )
                .Queryable()
                // This will be a small list of IDs (most often just one), so a SQL `WHERE...IN` should be OK here.
                .Where( me => mediaElementIds.Contains( me.Id ) )
                .Select( me => new LinkedMediaElementBag
                {
                    Id = me.Id,
                    Name = me.Name
                } )
                .ToList();

            mediaElements.ForEach( me => me.TranslateIdToIdKey() );

            return ActionOk( new GetLinkedMediaElementsResponseBag
            {
                LinkedMediaElements = mediaElements
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new ContentChannelItemService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {ContentChannelItem.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Upload a content item to the content library
        /// </summary>
        /// <param name="key">The identifier of the item to be uploaded.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UploadContentLibraryItem( string key )
        {
            try
            {
                var contentChannelItemId = key.AsInteger();
                var contentChannelItemService = new ContentChannelItemService( RockContext );
                contentChannelItemService.UploadToContentLibrary(
                    new ContentLibraryItemUploadOptions
                    {
                        ContentChannelItemId = contentChannelItemId,
                        UploadedByPersonAliasId = GetCurrentPerson().PrimaryAliasId
                    } );
            }
            catch ( AddToContentLibraryException ex )
            {
                Logger.LogError( ex, ex.Message );
                return ActionInternalServerError( ex.Message );
            }

            return ActionOk();
        }

        /// <summary>
        /// Update a content item that has already been uploaded to the content library
        /// </summary>
        /// <param name="key">The identifier of the item to be uploaded.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateContentLibraryItem( string key )
        {
            try
            {
                var contentChannelItemId = key.AsInteger();
                var contentChannelItemService = new ContentChannelItemService( RockContext );
                contentChannelItemService.UploadToContentLibrary(
                    new ContentLibraryItemUploadOptions
                    {
                        ContentChannelItemId = contentChannelItemId,
                        UploadedByPersonAliasId = GetCurrentPerson().PrimaryAliasId
                    }
                );
            }
            catch ( AddToContentLibraryException ex )
            {
                Logger.LogError( ex, ex.Message );
                return ActionInternalServerError( ex.Message );
            }

            return ActionOk();
        }

        /// <summary>
        /// Redownload a content item that has already been downloaded from the content library
        /// </summary>
        /// <param name="key">The identifier of the item to be downloaded.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ReDownloadContentLibraryItem( string key )
        {
            var contentChannelItemId = key.AsInteger();
            var contentChannelItemService = new ContentChannelItemService( RockContext );

            var contentLibraryItemGuid = contentChannelItemService.AsNoFilter().AsNoTracking().Where( i => i.Id == contentChannelItemId ).Select( i => i.ContentLibrarySourceIdentifier ).FirstOrDefault();

            try
            {
                var result = contentChannelItemService.AddFromContentLibrary( new ContentLibraryItemDownloadOptions
                {
                    ContentLibraryItemGuidToDownload = contentLibraryItemGuid.Value,
                    DownloadIntoContentChannelGuid = GetContentChannel().Guid,
                    CurrentPersonPerformingDownload = GetCurrentPerson()
                } );
            }
            catch ( AddFromContentLibraryException ex )
            {
                Logger.LogError( ex, ex.Message );
                return ActionInternalServerError( ex.Message );
            }

            return ActionOk();
        }

        #endregion Block Actions

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent interaction counts.
        /// </summary>
        private class InteractionCounts
        {
            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            public int EntityId { get; set; }

            /// <summary>
            /// Gets or sets the count of view interactions within the last 28 days.
            /// </summary>
            public int Last28DaysViewsCount { get; set; }
        }

        /// <summary>
        /// A POCO to represent linked media metadata.
        /// </summary>
        private class LinkedMediaMetadata
        {
            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            public int EntityId { get; set; }

            /// <summary>
            /// Gets or sets the metadata value that will contain an array of the linked <see cref="MediaElement"/>
            /// identifiers.
            /// </summary>
            public string Value { get; set; }
        }

        #endregion Supporting Classes
    }
}

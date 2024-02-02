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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelItemList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of content channel items.
    /// </summary>

    [DisplayName( "Content Item List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of content channel items." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [ContextAware]

    [LinkedPage(
        "Detail Page",
        Order = 0,
        Key = AttributeKey.DetailPage,
        Category = "Pages" )]

    [BooleanField(
        "Filter Items For Current User",
        Description = "Filters the items by those created by the current logged in user.",
        DefaultBooleanValue = false,
        Order = 1,
        Key = AttributeKey.FilterItemsForCurrentUser )]
    [BooleanField(
        "Show Filters",
        Description = "Allows you to show/hide the grids filters.",
        DefaultBooleanValue = true,
        Order = 2,
        Key = AttributeKey.ShowFilters )]
    [BooleanField(
        "Show Event Occurrences Column",
        Description = "Determines if the column that lists event occurrences should be shown if any of the items has an event occurrence.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.ShowEventOccurrencesColumn )]
    [BooleanField(
        "Show Priority Column",
        Description = "Determines if the column that displays priority should be shown for content channels that have Priority enabled.",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.ShowPriorityColumn )]
    [BooleanField(
        "Show Security Column",
        Description = "Determines if the security column should be shown.",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.ShowSecurityColumn )]
    [BooleanField(
        "Show Expire Column",
        Description = "Determines if the expire column should be shown.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.ShowExpireColumn )]
    [ContentChannelField(
        "Content Channel",
        Description = "If set the block will ignore content channel query parameters",
        IsRequired = false,
        Key = AttributeKey.ContentChannel )]

    [Rock.SystemGuid.EntityTypeGuid( "5597badd-bb0e-4bcd-be1f-5acf230cf428" )]
    [Rock.SystemGuid.BlockTypeGuid( "93dc73c4-545d-40b9-bfea-1cec04c07eb1" )]
    [CustomizedGrid]
    public class ContentChannelItemList : RockEntityListBlockType<ContentChannelItem>
    {

        private ContentChannel SelectedContentChannel { get; set; }

        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string FilterItemsForCurrentUser = "FilterItemsForCurrentUser";
            public const string ShowFilters = "ShowFilters";
            public const string ShowEventOccurrencesColumn = "ShowEventOccurrencesColumn";
            public const string ShowPriorityColumn = "ShowPriorityColumn";
            public const string ShowSecurityColumn = "ShowSecurityColumn";
            public const string ShowExpireColumn = "ShowExpireColumn";
            public const string ContentChannel = "ContentChannel";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string NewItemPage = "NewItemPage";
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

            var options = new ContentChannelItemListOptionsBag
            {
                ContentItemName = contentChannel.Name,
                IncludeTime = contentChannel.ContentChannelType.IncludeTime,
                IsManuallyOrdered = contentChannel.ItemsManuallyOrdered,
                DateType = contentChannel.ContentChannelType.DateRangeType,
                ContentChannelId = contentChannel.Id,
                ShowFilters = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean(),

                ShowReorderColumn = !isFiltered && contentChannel.ItemsManuallyOrdered,
                ShowPriorityColumn = !contentChannel.ContentChannelType.DisablePriority
                    && GetAttributeValue( AttributeKey.ShowPriorityColumn ).AsBoolean(),
                ShowStartDateTimeColumn = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.SingleDate
                    || contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange,
                ShowExpireDateTimeColumn = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange
                    && GetAttributeValue( AttributeKey.ShowExpireColumn ).AsBoolean(),
                ShowStatusColumn = contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus,
                ShowSecurityColumn = GetAttributeValue( AttributeKey.ShowSecurityColumn ).AsBoolean(),
                ShowOccurrencesColumn = GetAttributeValue( AttributeKey.ShowEventOccurrencesColumn ).AsBoolean()
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

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ContentItemId", "((Key))" ),
                [NavigationUrlKey.NewItemPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["ContentItemId"] = "((Key))",
                    ["ContentChannelId"] = contentChannel.Id.ToString()
                } )
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
        protected override GridBuilder<ContentChannelItem> GetGridBuilder()
        {
            var contentChannel = GetContentChannel();

            return new GridBuilder<ContentChannelItem>()
                .WithBlock( this )
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "contentChannelId", a => a.ContentChannelId )
                .AddTextField( "title", a => a.Title )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "expireDateTime", a => a.ExpireDateTime )
                .AddField( "isScheduled", a => a.StartDateTime > RockDateTime.Now )
                .AddField( "occurrences", a => a.EventItemOccurrences.Any() )
                .AddField( "status", a => a.Status )
                .AddField( "priority", a => a.Priority )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
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
                var entityService = new ContentChannelItemService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${ContentChannelItem.FriendlyTypeName}." );
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

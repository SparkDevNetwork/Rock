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
using Rock.ViewModels.Blocks.Event.CalendarContentChannelItemList;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Lists the content channel items associated to a particular calendar item occurrence.
    /// </summary>
    [DisplayName( "Calendar Item Occurrence Content Channel Item List" )]
    [Category( "Event" )]
    [Description( "Lists the content channel items associated to a particular calendar item occurrence." )]
    [IconCssClass( "ti ti-speakerphone" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page that will show the content channel item details.",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "2E5C2304-9E59-4EB5-9305-FC4C209ABA88" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "42D6D8EF-8C6D-4596-8100-7AC73DDADB6C" )]
    [Rock.SystemGuid.BlockTypeGuid( "8418C3B8-5E87-469F-BAE9-E15C32873FBD" )]
    public class CalendarContentChannelItemList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemId = "EventItemId";
            public const string EventItemOccurrenceId = "EventItemOccurrenceId";
            public const string ContentItemId = "ContentItemId";
            public const string ContentChannelId = "ContentChannelId";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string NewItemPage = "NewItemPage";
        }

        #endregion Keys

        #region Fields

        private EventItemOccurrence _eventItemOccurrence;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CalendarContentChannelItemListOptionsBag>
            {
                IsAddEnabled = false,
                IsDeleteEnabled = false,
                ExpectedRowCount = null,
                NavigationUrls = GetBoxNavigationUrls(),
                Options = GetBoxOptions()
            };

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options bag.</returns>
        private CalendarContentChannelItemListOptionsBag GetBoxOptions()
        {
            var options = new CalendarContentChannelItemListOptionsBag
            {
                ContentChannels = new List<CalendarContentChannelItemListContentChannelBag>()
            };

            var occurrence = GetEventItemOccurrence();
            if ( occurrence == null )
            {
                options.IsBlockVisible = false;
                return options;
            }

            var contentChannels = GetAuthorizedContentChannels( occurrence );
            if ( !contentChannels.Any() )
            {
                options.IsBlockVisible = false;
                return options;
            }

            options.IsBlockVisible = true;

            var linkedItems = GetLinkedContentChannelItems( occurrence.Id );
            var itemsByChannelId = linkedItems
                .GroupBy( i => i.ContentChannelId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var channelsWithItems = new HashSet<int>( itemsByChannelId.Keys );

            foreach ( var contentChannel in contentChannels.OrderBy( c => c.Name ) )
            {
                itemsByChannelId.TryGetValue( contentChannel.Id, out var channelItems );
                channelItems = channelItems ?? new List<ContentChannelItem>();

                options.ContentChannels.Add( BuildContentChannelSection( contentChannel, channelItems, channelsWithItems.Contains( contentChannel.Id ) ) );
            }

            return options;
        }

        /// <summary>
        /// Builds a single content channel section bag, including grid definition and data.
        /// </summary>
        /// <param name="contentChannel">The content channel cache entry.</param>
        /// <param name="items">The content channel items linked for this channel.</param>
        /// <param name="hasItems">Whether the channel currently has linked items (controls default expand).</param>
        /// <returns>The section bag.</returns>
        private CalendarContentChannelItemListContentChannelBag BuildContentChannelSection(
            ContentChannelCache contentChannel,
            List<ContentChannelItem> items,
            bool hasItems )
        {
            var contentChannelType = ContentChannelTypeCache.Get( contentChannel.ContentChannelTypeId );
            var canEdit = GetIsChannelEditEnabled( contentChannel );

            var showStartDateTimeColumn = contentChannelType != null
                && contentChannelType.DateRangeType != ContentChannelDateType.NoDates;

            var showExpireDateTimeColumn = contentChannelType != null
                && contentChannelType.DateRangeType == ContentChannelDateType.DateRange;

            var includeTime = contentChannelType?.IncludeTime ?? false;

            var showPriorityColumn = contentChannelType != null
                && !contentChannelType.DisablePriority;

            var showStatusColumn = contentChannel.RequiresApproval
                && contentChannelType != null
                && !contentChannelType.DisableStatus;

            var attributes = GetGridAttributes( contentChannel );
            var builder = GetGridBuilder( attributes );

            return new CalendarContentChannelItemListContentChannelBag
            {
                IdKey = contentChannel.IdKey,
                Name = contentChannel.Name,
                IconCssClass = contentChannel.IconCssClass.IsNotNullOrWhiteSpace()
                    ? contentChannel.IconCssClass
                    : "ti ti-speakerphone",
                IsExpanded = hasItems,
                IsAddEnabled = canEdit,
                IsDeleteEnabled = canEdit,
                ShowStartDateTimeColumn = showStartDateTimeColumn,
                ShowExpireDateTimeColumn = showExpireDateTimeColumn,
                IncludeTime = includeTime,
                ShowPriorityColumn = showPriorityColumn,
                ShowStatusColumn = showStatusColumn,
                GridDefinition = builder.BuildDefinition(),
                GridData = BuildChannelGridData( contentChannel, items )
            };
        }

        /// <summary>
        /// Builds grid row data for the linked content channel items in a single channel.
        /// Loads grid-column attribute values when needed.
        /// </summary>
        /// <param name="contentChannel">The content channel.</param>
        /// <param name="items">The linked items for this channel.</param>
        /// <returns>The grid data bag.</returns>
        private GridDataBag BuildChannelGridData( ContentChannelCache contentChannel, List<ContentChannelItem> items )
        {
            var attributes = GetGridAttributes( contentChannel );

            if ( attributes.Any() && items.Any() )
            {
                var attributeIds = attributes.Select( a => a.Id ).ToList();
                Helper.LoadFilteredAttributes(
                    typeof( ContentChannelItem ),
                    items.Cast<IHasAttributes>().ToList(),
                    RockContext,
                    a => attributeIds.Contains( a.Id ) );
            }

            var orderedItems = items.OrderByDescending( i => i.StartDateTime ).ToList();
            return GetGridBuilder( attributes ).Build( orderedItems );
        }

        /// <summary>
        /// Creates the grid builder for content channel item rows.
        /// </summary>
        /// <param name="attributes">The grid attribute fields to include.</param>
        /// <returns>The grid builder.</returns>
        private GridBuilder<ContentChannelItem> GetGridBuilder( List<AttributeCache> attributes )
        {
            return new GridBuilder<ContentChannelItem>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "expireDateTime", a => a.ExpireDateTime )
                .AddField( "priority", a => a.Priority )
                .AddField( "status", a => a.Status )
                .AddAttributeFields( attributes );
        }

        /// <summary>
        /// Gets the grid-column attributes for a content channel (type- and channel-qualified).
        /// </summary>
        /// <param name="contentChannel">The content channel.</param>
        /// <returns>The ordered grid attributes.</returns>
        private List<AttributeCache> GetGridAttributes( ContentChannelCache contentChannel )
        {
            var entityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ) ).Id;

            return AttributeCache.All()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn &&
                    (
                        (
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( contentChannel.ContentChannelTypeId.ToString() )
                        ) ||
                        (
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( contentChannel.Id.ToString() )
                        )
                    ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the content channels authorized for view that are associated with calendars
        /// containing the event item for the current occurrence.
        /// </summary>
        /// <param name="occurrence">The event item occurrence.</param>
        /// <returns>The authorized content channels.</returns>
        private List<ContentChannelCache> GetAuthorizedContentChannels( EventItemOccurrence occurrence )
        {
            var eventCalendarIds = new EventCalendarItemService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i => i.EventItemId == occurrence.EventItemId )
                .Select( i => i.EventCalendarId )
                .Distinct()
                .ToList();

            if ( !eventCalendarIds.Any() )
            {
                return new List<ContentChannelCache>();
            }

            var contentChannelIds = new EventCalendarContentChannelService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i => eventCalendarIds.Contains( i.EventCalendarId ) )
                .Select( i => i.ContentChannelId )
                .Distinct()
                .ToList();

            var person = RequestContext.CurrentPerson;

            return contentChannelIds
                .Select( id => ContentChannelCache.Get( id ) )
                .Where( c => c != null && c.IsAuthorized( Authorization.VIEW, person ) )
                .ToList();
        }

        /// <summary>
        /// Gets content channel items currently linked to the occurrence.
        /// </summary>
        /// <param name="occurrenceId">The event item occurrence identifier.</param>
        /// <returns>The linked content channel items.</returns>
        private List<ContentChannelItem> GetLinkedContentChannelItems( int occurrenceId )
        {
            return new EventItemOccurrenceChannelItemService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( c => c.EventItemOccurrenceId == occurrenceId )
                .Select( c => c.ContentChannelItem )
                .Where( i => i != null )
                .ToList();
        }

        /// <summary>
        /// Determines whether edit actions are allowed for the given content channel.
        /// </summary>
        /// <param name="contentChannel">The content channel.</param>
        /// <returns><c>true</c> if edit is allowed; otherwise, <c>false</c>.</returns>
        private bool GetIsChannelEditEnabled( ContentChannelCache contentChannel )
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || contentChannel.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether the content channel may be used for Link Existing on this occurrence.
        /// Requires the channel to be associated with a calendar for the event item and VIEW authorization.
        /// </summary>
        /// <param name="occurrence">The event item occurrence.</param>
        /// <param name="contentChannel">The content channel.</param>
        /// <returns><c>true</c> if linking is allowed for this channel; otherwise, <c>false</c>.</returns>
        private bool IsContentChannelAuthorizedForOccurrence( EventItemOccurrence occurrence, ContentChannelCache contentChannel )
        {
            return GetAuthorizedContentChannels( occurrence ).Any( c => c.Id == contentChannel.Id );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var contextParams = GetContextPageParameters();

            var detailParams = new Dictionary<string, string>( contextParams )
            {
                [PageParameterKey.ContentItemId] = "((Key))",
                ["autoEdit"] = "true"
            };

            var newItemParams = new Dictionary<string, string>( contextParams )
            {
                [PageParameterKey.ContentChannelId] = "((Key))"
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, detailParams ),
                [NavigationUrlKey.NewItemPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, newItemParams )
            };
        }

        /// <summary>
        /// Builds the shared navigation page parameters for calendar / item / occurrence context.
        /// </summary>
        /// <returns>The page parameters dictionary.</returns>
        private Dictionary<string, string> GetContextPageParameters()
        {
            var qryParams = new Dictionary<string, string>();
            var allowPredictableIds = !PageCache.Layout.Site.DisablePredictableIds;

            var eventCalendar = new EventCalendarService( RockContext ).Get(
                PageParameter( PageParameterKey.EventCalendarId ),
                allowPredictableIds );

            if ( eventCalendar != null )
            {
                qryParams[PageParameterKey.EventCalendarId] = eventCalendar.IdKey;
            }

            var eventItem = new EventItemService( RockContext ).Get(
                PageParameter( PageParameterKey.EventItemId ),
                allowPredictableIds );

            if ( eventItem != null )
            {
                qryParams[PageParameterKey.EventItemId] = eventItem.IdKey;
            }

            var occurrence = GetEventItemOccurrence();
            if ( occurrence != null )
            {
                qryParams[PageParameterKey.EventItemOccurrenceId] = occurrence.IdKey;
            }

            return qryParams;
        }

        /// <summary>
        /// Gets the event item occurrence from the page parameter.
        /// </summary>
        /// <returns>The event item occurrence, or null if not found.</returns>
        private EventItemOccurrence GetEventItemOccurrence()
        {
            if ( _eventItemOccurrence != null )
            {
                return _eventItemOccurrence;
            }

            var key = PageParameter( PageParameterKey.EventItemOccurrenceId );
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            _eventItemOccurrence = new EventItemOccurrenceService( RockContext ).Get(
                key,
                !PageCache.Layout.Site.DisablePredictableIds );

            return _eventItemOccurrence;
        }

        /// <summary>
        /// Formats a friendly label for an event item occurrence link.
        /// </summary>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns>The friendly label text.</returns>
        private string FormatOccurrenceLinkText( EventItemOccurrence occurrence )
        {
            if ( occurrence == null )
            {
                return string.Empty;
            }

            var eventItemName = occurrence.EventItem?.Name ?? "Event";
            var campusName = occurrence.CampusId.HasValue
                ? CampusCache.Get( occurrence.CampusId.Value )?.Name ?? "Campus"
                : "All Campuses";

            if ( occurrence.Location.IsNotNullOrWhiteSpace() )
            {
                return $"{eventItemName} — {campusName} — {occurrence.Location}";
            }

            return $"{eventItemName} — {campusName}";
        }

        /// <summary>
        /// Formats the link-existing dropdown title for a content channel item.
        /// </summary>
        /// <param name="item">The content channel item.</param>
        /// <param name="contentChannelType">The content channel type.</param>
        /// <returns>The display title.</returns>
        private string FormatLinkableItemTitle( ContentChannelItem item, ContentChannelTypeCache contentChannelType )
        {
            var title = item.Title;

            if ( contentChannelType == null || contentChannelType.DateRangeType == ContentChannelDateType.NoDates )
            {
                return title;
            }

            string startDateText = null;
            string endDateText = null;

            if ( contentChannelType.DateRangeType == ContentChannelDateType.SingleDate )
            {
                startDateText = item.StartDateTime.ToShortDateString();
            }
            else if ( contentChannelType.DateRangeType == ContentChannelDateType.DateRange )
            {
                startDateText = item.StartDateTime.ToShortDateString();
                endDateText = item.ExpireDateTime.HasValue
                    ? item.ExpireDateTime.Value.ToShortDateString()
                    : null;
            }

            if ( endDateText != null )
            {
                return $"{title} ({startDateText} - {endDateText})";
            }

            if ( startDateText != null )
            {
                return $"{title} ({startDateText})";
            }

            return title;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Removes a content channel item from the current occurrence. When the item is still
        /// linked to other occurrences it is unlinked only; otherwise the content channel item
        /// is deleted in a single save (cascade removes this occurrence's linkage).
        /// </summary>
        /// <param name="contentItemKey">The content channel item key.</param>
        /// <param name="contentChannelKey">The content channel key (authorization scope).</param>
        /// <returns>The delete / unlink result.</returns>
        [BlockAction]
        public BlockActionResult Delete( string contentItemKey, string contentChannelKey )
        {
            var occurrence = GetEventItemOccurrence();
            if ( occurrence == null )
            {
                return ActionBadRequest( "Event item occurrence not found." );
            }

            var allowPredictableIds = !PageCache.Layout.Site.DisablePredictableIds;

            var contentChannel = ContentChannelCache.Get( contentChannelKey, allowPredictableIds );
            if ( contentChannel == null )
            {
                return ActionBadRequest( "Content channel not found." );
            }

            if ( !GetIsChannelEditEnabled( contentChannel ) )
            {
                return ActionUnauthorized( "Not authorized to edit content channel items for this channel." );
            }

            var contentItemService = new ContentChannelItemService( RockContext );
            var contentItem = contentItemService.Get( contentItemKey, allowPredictableIds );
            if ( contentItem == null )
            {
                return ActionNotFound( "Content channel item not found." );
            }

            if ( contentItem.ContentChannelId != contentChannel.Id )
            {
                return ActionBadRequest( "Content channel item does not belong to the specified channel." );
            }

            var itemTitle = contentItem.Title;
            var occurrenceChannelItemService = new EventItemOccurrenceChannelItemService( RockContext );

            var linkage = occurrenceChannelItemService.Queryable()
                .FirstOrDefault( l =>
                    l.EventItemOccurrenceId == occurrence.Id &&
                    l.ContentChannelItemId == contentItem.Id );

            /*
                7/13/26 - MSE

                Check other occurrence links before mutating this occurrence's linkage.
                If we unlinked first and then failed to delete the ContentChannelItem
                (e.g. non-cascading associations), the occurrence would be permanently
                unlinked while the client received an error and kept a stale grid row.

                Shared items: unlink this occurrence only (one SaveChanges).
                Sole-linked items: delete the ContentChannelItem while the linkage still
                exists so cascade + CanDelete failures leave the association intact.

                Reason: Avoid partial commits when delete fails after unlink.
            */

            // Materialize with Include before projecting so EventItem is loaded for labels.
            // (EF ignores Include when the query shape is changed by Select.)
            var otherOccurrenceLinks = occurrenceChannelItemService.Queryable()
                .AsNoTracking()
                .Include( l => l.EventItemOccurrence.EventItem )
                .Where( l =>
                    l.ContentChannelItemId == contentItem.Id &&
                    l.EventItemOccurrenceId != occurrence.Id )
                .ToList();

            if ( otherOccurrenceLinks.Any() )
            {
                // Shared item — only remove this occurrence's association.
                if ( linkage == null )
                {
                    return ActionBadRequest( "Content channel item is not linked to this occurrence." );
                }

                occurrenceChannelItemService.Delete( linkage );
                RockContext.SaveChanges();

                var remainingItems = otherOccurrenceLinks
                    .Select( l => l.EventItemOccurrence )
                    .Where( o => o != null )
                    .Select( o => new ListItemBag
                    {
                        Value = o.IdKey,
                        Text = FormatOccurrenceLinkText( o )
                    } )
                    .OrderBy( i => i.Text )
                    .ToList();

                return ActionOk( new CalendarContentChannelItemListDeleteResponseBag
                {
                    WasContentItemDeleted = false,
                    WasUnlinkedOnly = true,
                    ItemTitle = itemTitle,
                    Message = $"\"{itemTitle}\" was removed from this occurrence but was not deleted because it is still linked to other event occurrences.",
                    RemainingOccurrenceLinks = remainingItems
                } );
            }

            // Sole link (or no other links): delete the content item in one save.
            // Cascade on EventItemOccurrenceChannelItem removes this occurrence's linkage.
            if ( linkage == null )
            {
                return ActionBadRequest( "Content channel item is not linked to this occurrence." );
            }

            if ( !contentItemService.CanDelete( contentItem, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            contentItemService.Delete( contentItem );
            RockContext.SaveChanges();

            return ActionOk( new CalendarContentChannelItemListDeleteResponseBag
            {
                WasContentItemDeleted = true,
                WasUnlinkedOnly = false,
                ItemTitle = itemTitle,
                Message = null,
                RemainingOccurrenceLinks = new List<ListItemBag>()
            } );
        }

        /// <summary>
        /// Gets the grid row data for a single content channel section on the current occurrence.
        /// Used to soft-refresh a panel after link or delete without remounting the block.
        /// </summary>
        /// <param name="contentChannelKey">The content channel key.</param>
        /// <returns>The grid data for that channel.</returns>
        [BlockAction]
        public BlockActionResult GetChannelGridData( string contentChannelKey )
        {
            var occurrence = GetEventItemOccurrence();
            if ( occurrence == null )
            {
                return ActionBadRequest( "Event item occurrence not found." );
            }

            var allowPredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var contentChannel = ContentChannelCache.Get( contentChannelKey, allowPredictableIds );
            if ( contentChannel == null )
            {
                return ActionBadRequest( "Content channel not found." );
            }

            if ( !IsContentChannelAuthorizedForOccurrence( occurrence, contentChannel ) )
            {
                return ActionUnauthorized( "Not authorized to view content channel items for this channel." );
            }

            var items = new EventItemOccurrenceChannelItemService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( c =>
                    c.EventItemOccurrenceId == occurrence.Id &&
                    c.ContentChannelItem != null &&
                    c.ContentChannelItem.ContentChannelId == contentChannel.Id )
                .Select( c => c.ContentChannelItem )
                .ToList();

            return ActionOk( BuildChannelGridData( contentChannel, items ) );
        }

        /// <summary>
        /// Gets content channel items that can be linked to the current occurrence for a channel.
        /// </summary>
        /// <param name="contentChannelKey">The content channel key.</param>
        /// <returns>List items for the link-existing dropdown.</returns>
        [BlockAction]
        public BlockActionResult GetLinkableItems( string contentChannelKey )
        {
            var occurrence = GetEventItemOccurrence();
            if ( occurrence == null )
            {
                return ActionBadRequest( "Event item occurrence not found." );
            }

            var allowPredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var contentChannel = ContentChannelCache.Get( contentChannelKey, allowPredictableIds );
            if ( contentChannel == null )
            {
                return ActionBadRequest( "Content channel not found." );
            }

            // Require calendar association + VIEW (same set rendered in the block UI).
            if ( !IsContentChannelAuthorizedForOccurrence( occurrence, contentChannel ) )
            {
                return ActionUnauthorized( "Not authorized to link content channel items for this channel." );
            }

            var contentChannelType = ContentChannelTypeCache.Get( contentChannel.ContentChannelTypeId );
            var now = RockDateTime.Now;

            var items = new ContentChannelItemService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i => i.ContentChannelId == contentChannel.Id )
                .Where( i => !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value >= now )
                .Where( i => !i.EventItemOccurrences.Any( o => o.EventItemOccurrenceId == occurrence.Id ) )
                .OrderBy( i => i.Title )
                .ToList();

            var listItems = items
                .Select( i => new ListItemBag
                {
                    Value = i.IdKey,
                    Text = FormatLinkableItemTitle( i, contentChannelType )
                } )
                .ToList();

            return ActionOk( listItems );
        }

        /// <summary>
        /// Links an existing content channel item to the current event item occurrence.
        /// </summary>
        /// <param name="contentChannelKey">The content channel key.</param>
        /// <param name="contentItemKey">The content channel item key.</param>
        /// <returns>Success or error result.</returns>
        [BlockAction]
        public BlockActionResult LinkExistingItem( string contentChannelKey, string contentItemKey )
        {
            var occurrence = GetEventItemOccurrence();
            if ( occurrence == null )
            {
                return ActionBadRequest( "Event item occurrence not found." );
            }

            var allowPredictableIds = !PageCache.Layout.Site.DisablePredictableIds;

            var contentChannel = ContentChannelCache.Get( contentChannelKey, allowPredictableIds );
            if ( contentChannel == null )
            {
                return ActionBadRequest( "Content channel not found." );
            }

            // Require calendar association + VIEW (same set rendered in the block UI).
            if ( !IsContentChannelAuthorizedForOccurrence( occurrence, contentChannel ) )
            {
                return ActionUnauthorized( "Not authorized to link content channel items for this channel." );
            }

            var contentItem = new ContentChannelItemService( RockContext ).Get( contentItemKey, allowPredictableIds );
            if ( contentItem == null )
            {
                return ActionNotFound( "Content channel item not found." );
            }

            if ( contentItem.ContentChannelId != contentChannel.Id )
            {
                return ActionBadRequest( "Content channel item does not belong to the specified channel." );
            }

            // Match GetLinkableItems: do not allow linking expired items.
            if ( contentItem.ExpireDateTime.HasValue && contentItem.ExpireDateTime.Value < RockDateTime.Now )
            {
                return ActionBadRequest( "That content channel item has expired and cannot be linked." );
            }

            var occurrenceChannelItemService = new EventItemOccurrenceChannelItemService( RockContext );
            var alreadyLinked = occurrenceChannelItemService.Queryable()
                .Any( l =>
                    l.EventItemOccurrenceId == occurrence.Id &&
                    l.ContentChannelItemId == contentItem.Id );

            if ( alreadyLinked )
            {
                return ActionBadRequest( "That content channel item is already linked to this occurrence." );
            }

            occurrenceChannelItemService.Add( new EventItemOccurrenceChannelItem
            {
                ContentChannelItemId = contentItem.Id,
                EventItemOccurrenceId = occurrence.Id
            } );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <remarks>
        /// This block derives from <see cref="RockBlockType"/> rather than
        /// <see cref="RockListBlockType{T}"/>, so it does not inherit the standard
        /// entity-set action. It is provided here so grid actions such as Launch
        /// Workflow and Merge Template can operate on the selected content channel items.
        /// </remarks>
        /// <param name="entitySet">The bag that describes the entity set to create.</param>
        /// <returns>An action result that contains the identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        #endregion Block Actions
    }
}

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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.EventCalendarItemList;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of event calendar items.
    /// </summary>
    [DisplayName( "Calendar Event Item List" )]
    [Category( "Event" )]
    [Description( "Lists all the event items in the given calendar." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the event calendar item details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "ca712211-3076-48bd-9321-2b7cee1d5961" )]
    [Rock.SystemGuid.BlockTypeGuid( "20c68613-f253-4d2f-a465-62afbb01dcd6" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware( typeof( EventCalendar ) )]
    public class EventCalendarItemList : RockEntityListBlockType<EventCalendarItem>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string EventCalendar = "EventCalendarId";
        }

        private static class PreferenceKey
        {
            public const string FilterStartDate = "filter-start-date";
            public const string FilterEndDate = "filter-end-date";
            public const string FilterStatus = "filter-status";
            public const string FilterApprovalStatus = "filter-approval-status";
            public const string FilterCampus = "filter-campus";
            public const string FilterAudience = "filter-audience";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The cached event calendar, should be accessed via the GetEventCalendar method.
        /// </summary>
        private EventCalendar _eventCalendar = null;

        #endregion

        #region Properties

        protected string FilterStartDate => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToEventCalendar( PreferenceKey.FilterStartDate ) );

        protected string FilterEndDate => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToEventCalendar( PreferenceKey.FilterEndDate ) );

        protected string FilterStatus => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToEventCalendar( PreferenceKey.FilterStatus ) );

        protected string FilterApprovalStatus => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToEventCalendar( PreferenceKey.FilterApprovalStatus ) );

        protected List<Guid> FilterCampus => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToEventCalendar( PreferenceKey.FilterCampus ) )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        protected List<Guid> FilterAudience => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToEventCalendar(PreferenceKey.FilterAudience ) )
            .FromJsonOrNull<List<ListItemBag>>()?.Select( l => l.Value.AsGuid() ).ToList() ?? new List<Guid>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<EventCalendarItemListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddDeleteEnabled();
            box.IsDeleteEnabled = GetIsAddDeleteEnabled();
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
        private EventCalendarItemListOptionsBag GetBoxOptions()
        {
            var options = new EventCalendarItemListOptionsBag()
            {
                CampusItems = CampusCache.All().ToListItemBagList(),
                EventCalendarIdKey = GetEventCalendar()?.IdKey
            };
            return options;
        }

        /// <summary>
        /// Determines if the add and delete buttons should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || GetEventCalendar()?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "EventCalendarItemId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<EventCalendarItem> GetListQueryable( RockContext rockContext )
        {
            var eventCalendarId = GetEventCalendar()?.Id ?? 0;

            var qry = new EventCalendarItemService( rockContext )
                .Queryable( "EventCalendar,EventItem.EventItemAudiences,EventItem.EventItemOccurrences.Schedule" )
                .Where( m =>
                    m.EventItem != null &&
                    m.EventCalendarId == eventCalendarId );

            // Filter by Status
            if ( FilterStatus == "Active" )
            {
                qry = qry
                    .Where( m => m.EventItem.IsActive );
            }
            else if ( FilterStatus == "Inactive" )
            {
                qry = qry
                    .Where( m => !m.EventItem.IsActive );
            }

            // Filter by Approval Status
            if ( FilterApprovalStatus == "Approved" )
            {
                qry = qry
                    .Where( m => m.EventItem.IsApproved );
            }
            else if ( FilterApprovalStatus == "Not Approved" )
            {
                qry = qry
                    .Where( m => !m.EventItem.IsApproved );
            }

            // Filter by Campus
            if ( FilterCampus.Any() )
            {
                qry = qry
                    .Where( i =>
                        i.EventItem.EventItemOccurrences
                            .Any( c =>
                                FilterCampus.Contains( c.Campus.Guid ) ) );
            }

            // Filter by Audience
            if ( FilterAudience.Any() )
            {
                qry = qry
                    .Where( i =>
                        i.EventItem.EventItemAudiences
                            .Any( c =>
                                FilterAudience.Contains( c.DefinedValue.Guid ) ) );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<EventCalendarItem> GetOrderedListQueryable( IQueryable<EventCalendarItem> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( i => i.EventItem.Name );
        }

        /// <inheritdoc/>
        protected override List<EventCalendarItem> GetListItems( IQueryable<EventCalendarItem> queryable, RockContext rockContext )
        {
            var eventCalendarItems = base.GetListItems( queryable, rockContext );

            // if a date range was specified, need to get all dates for items and filter based on any that have an occurrence withing the date range
            DateTime? lowerDateRange = FilterStartDate.AsDateTime();
            DateTime? upperDateRange = FilterEndDate.AsDateTime();

            if ( lowerDateRange.HasValue || upperDateRange.HasValue )
            {
                // If only one value was included, default the other to be a years difference
                lowerDateRange = lowerDateRange ?? upperDateRange.Value.AddYears( -1 ).AddDays( 1 );
                upperDateRange = upperDateRange ?? lowerDateRange.Value.AddYears( 1 ).AddDays( -1 );

                // Filter out calendar items with no dates within range
                eventCalendarItems = eventCalendarItems.Where( i => i.EventItem.GetStartTimes( lowerDateRange.Value, upperDateRange.Value.AddDays( 1 ) ).Any() ).ToList();
            }

            return eventCalendarItems;
        }

        /// <inheritdoc/>
        protected override GridBuilder<EventCalendarItem> GetGridBuilder()
        {
            return new GridBuilder<EventCalendarItem>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "date", a => GetNextStartDateTime( a.EventItem ) )
                .AddTextField( "name", a => a.EventItem?.Name )
                .AddField( "occurrences", a => GetOccurrences( a.EventItem ) )
                .AddField( "calendars", a => a.EventItem?.EventCalendarItems?.Select( i => i.EventCalendar.Name ).ToList() )
                .AddField( "audiences", a => a.EventItem?.EventItemAudiences?.Select( i => i.DefinedValue.Value ).ToList() )
                .AddField( "isActive", a => a.EventItem?.IsActive )
                .AddField( "isApproved", a => a.EventItem?.IsApproved )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <inheritdoc/>
        protected override GridDataBag GetGridDataBag( RockContext rockContext )
        {
            // Get the queryable and make sure it is ordered correctly.
            // Marking the query as AsNoTracking interferes with EntityFramework's lazy loading of nested tables,
            // thus in this instance the query is not marked as AsNoTracking.
            var qry = GetListQueryable( rockContext );
            qry = GetOrderedListQueryable( qry, rockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, rockContext );

            if ( !DisableAttributes && typeof( IHasAttributes ).IsAssignableFrom( typeof( EventCalendarItem ) ) )
            {
                var gridAttributes = GetGridAttributes();
                var gridAttributeIds = gridAttributes.Select( a => a.Id ).ToList();

                Helper.LoadFilteredAttributes( items.Cast<IHasAttributes>(), rockContext, a => gridAttributeIds.Contains( a.Id ) );
            }

            return GetGridBuilder().Build( items );
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            var availableAttributes = new List<AttributeCache>();
            var eventCalendar = GetEventCalendar();

            if ( eventCalendar != null )
            {
                int entityTypeId = new EventCalendarItem().TypeId;
                foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "EventCalendarId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( eventCalendar.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    availableAttributes.Add( AttributeCache.Get( attributeModel ) );
                }
            }

            return availableAttributes;
        }

        /// <summary>
        /// Gets the number of EventItemOccurrences.
        /// </summary>
        /// <param name="eventItem">The event item.</param>
        /// <returns></returns>
        private int GetOccurrences( EventItem eventItem )
        {
            if ( FilterCampus.Any() )
            {
                return eventItem.EventItemOccurrences.Count( c => !c.CampusId.HasValue || FilterCampus.Contains( c.Campus.Guid ) );
            }
            else
            {
                return eventItem.EventItemOccurrences.Count;
            }
        }

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private string GetNextStartDateTime( EventItem item )
        {
            DateTime? lowerDateRange = FilterStartDate.AsDateTime();
            DateTime? upperDateRange = FilterEndDate.AsDateTime();
            DateTime? nextStartDate = null;

            if ( lowerDateRange.HasValue || upperDateRange.HasValue )
            {
                lowerDateRange = lowerDateRange ?? upperDateRange.Value.AddYears( -1 ).AddDays( 1 );
                upperDateRange = upperDateRange ?? lowerDateRange.Value.AddYears( 1 ).AddDays( -1 );

                var startDateTimes = item.GetStartTimes( lowerDateRange.Value, upperDateRange.Value.AddDays( 1 ) );

                if ( startDateTimes.Count > 0 )
                {
                    nextStartDate = startDateTimes.Min();
                }
            }
            else
            {
                nextStartDate = item.NextStartDateTime;
            }

            return nextStartDate.HasValue ? nextStartDate.ToShortDateString() : "N/A";
        }

        /// <summary>
        /// Gets the event calendar from the PageParameters or from the block's context entity.
        /// </summary>
        /// <returns><see cref="EventCalendar"/></returns>
        private EventCalendar GetEventCalendar()
        {
            if ( _eventCalendar != null )
            {
                return _eventCalendar;
            }

            var eventCalendarIdParam = PageParameter( PageParameterKey.EventCalendar );
            var eventCalendarId = eventCalendarIdParam.AsIntegerOrNull() ?? Rock.Utility.IdHasher.Instance.GetId( eventCalendarIdParam );

            if ( eventCalendarId.HasValue )
            {
                _eventCalendar = new EventCalendarService( new RockContext() ).Queryable()
                    .Where( g => g.Id == eventCalendarId )
                    .FirstOrDefault();
            }

            return _eventCalendar;
        }

        /// <summary>
        /// Makes the key unique to the current event calendar.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToEventCalendar( string key )
        {
            var eventCalendar = GetEventCalendar();

            if ( eventCalendar != null )
            {
                return $"{eventCalendar.IdKey}-{key}";
            }

            return key;
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
                var entityService = new EventCalendarItemService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{EventCalendarItem.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${EventCalendarItem.FriendlyTypeName}." );
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

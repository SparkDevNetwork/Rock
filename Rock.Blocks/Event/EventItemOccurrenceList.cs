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
using Rock.ViewModels.Blocks.Event.EventItemOccurrenceList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of event item occurrences.
    /// </summary>
    [DisplayName( "Calendar Event Item Occurrence List" )]
    [Category( "Event" )]
    [Description( "Displays the occurrence details for a given calendar event item." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page that will show the event item occurrence details." )]

    [LinkedPage( "Registration Instance Page",
        Key = AttributeKey.RegistrationInstancePage,
        Description = "The page to view registration details",
        IsRequired = true,
        Order = 1 )]

    [LinkedPage( "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "The page for viewing details about a group",
        IsRequired = true,
        Order = 2 )]

    [LinkedPage( "Content Item Detail Page",
        Key = AttributeKey.ContentItemDetailPage,
        Description = "The page for viewing details about a content item",
        IsRequired = true,
        Order = 3 )]

    [Rock.SystemGuid.EntityTypeGuid( "ab765c53-424b-4824-afd6-1174228fd92f" )]
    [Rock.SystemGuid.BlockTypeGuid( "ddc28e7a-e6c0-4081-b4b9-7cd6475e9046" )]
    [CustomizedGrid]
    public class EventItemOccurrenceList : RockEntityListBlockType<EventItemOccurrence>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string ContentItemDetailPage = "ContentItemDetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string EventItemId = "EventItemId";
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemOccurrenceId = "EventItemOccurrenceId";
            public const string CopyFromId = "CopyFromId";
        }

        private static class PreferenceKey
        {
            public const string FilterStartDate = "filter-start-date";
            public const string FilterEndDate = "filter-end-date";
            public const string FilterCampus = "filter-campus";
            public const string FilterContact = "filter-contact";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The cached Event Item, should be accessed via the <see cref="GetEventItem"/> method.
        /// </summary>
        private EventItem _eventItem = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the date from which to start filtering the results.
        /// </summary>
        protected DateTime? FilterStartDate => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterStartDate )
            .AsDateTime();

        /// <summary>
        /// Gets the date to which to filter the results to.
        /// </summary>
        protected DateTime? FilterEndDate => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterEndDate )
            .AsDateTime();

        /// <summary>
        /// The list of campus Guids to filter the results by.
        /// </summary>
        protected List<Guid> FilterCampus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        /// <summary>
        /// Gets the contact by which the results should be filtered.
        /// </summary>
        protected string FilterContact => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterContact );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<EventItemOccurrenceListOptionsBag>();
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
        private EventItemOccurrenceListOptionsBag GetBoxOptions()
        {
            var options = new EventItemOccurrenceListOptionsBag()
            {
                CampusItems = CampusCache.All().ToListItemBagList(),
                ContentItemDetailPageUrl = this.GetLinkedPageUrl( AttributeKey.RegistrationInstancePage ),
                GroupDetailPageUrl = this.GetLinkedPageUrl( AttributeKey.GroupDetailPage ),
                RegistrationInstancePageUrl = this.GetLinkedPageUrl( AttributeKey.RegistrationInstancePage ),
                IsBlockVisible = GetEventItem() != null,
            };

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
            var eventItem = GetEventItem();

            var qryParams = new Dictionary<string, string>
            {
                { PageParameterKey.EventCalendarId, GetEventCalendarIdKey( eventItem ) },
                { PageParameterKey.EventItemId, eventItem?.IdKey },
                { PageParameterKey.EventItemOccurrenceId, "((Key))" },
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<EventItemOccurrence> GetListQueryable( RockContext rockContext )
        {
            var eventItem = GetEventItem();

            if ( eventItem != null )
            {
                var qry = new EventItemOccurrenceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( c => c.EventItemId == eventItem.Id );

                return qry;
            }
            else
            {
                return new List<EventItemOccurrence>().AsQueryable();
            }
        }

        /// <inheritdoc/>
        protected override IQueryable<EventItemOccurrence> GetOrderedListQueryable( IQueryable<EventItemOccurrence> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( e => e.NextStartDateTime );
        }

        /// <inheritdoc/>
        protected override List<EventItemOccurrence> GetListItems( IQueryable<EventItemOccurrence> queryable, RockContext rockContext )
        {
            var eventItemOccurrences = queryable.ToList();

            // if a date range was specified, need to get all dates for items and filter based on any that have an occurrence within the date range.
            if ( FilterStartDate.HasValue || FilterEndDate.HasValue )
            {
                // If only one value was included, default the other to be a years difference
                var lowerDateRange = FilterStartDate ?? FilterEndDate.Value.AddYears( -1 ).AddDays( 1 );
                var upperDateRange = FilterEndDate ?? FilterStartDate.Value.AddYears( 1 ).AddDays( -1 );

                // Filter out calendar items with no dates within range
                eventItemOccurrences = eventItemOccurrences.Where( i => i.GetStartTimes( lowerDateRange, upperDateRange.AddDays( 1 ) ).Any() ).ToList();
            }

            return eventItemOccurrences;
        }

        /// <inheritdoc/>
        protected override GridBuilder<EventItemOccurrence> GetGridBuilder()
        {
            return new GridBuilder<EventItemOccurrence>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "campus", a => a.Campus?.Name ?? "All Campuses" )
                .AddDateTimeField( "date", a => GetNextStartDateTime( a )?.DateTime )
                .AddTextField( "location", a => a.Location )
                .AddField( "registrationInstanceId", a => a.Linkages.Any() ? a.Linkages.First().RegistrationInstance?.IdKey : null )
                .AddTextField( "registration", a => a.Linkages.Any() ? a.Linkages.First().RegistrationInstance?.Name : null )
                .AddField( "groupId", a => a.Linkages.Any() ? a.Linkages.First().Group?.IdKey : null )
                .AddField( "contentItems", a => FormatContentItems( a.ContentChannelItems.Select( e => e.ContentChannelItem ) ) )
                .AddTextField( "group", a => a.Linkages.Any() ? a.Linkages.First().Group?.Name : null )
                .AddPersonField( "contact", a => a.ContactPersonAlias?.Person )
                .AddTextField( "contactPhone", a => a.ContactPhone )
                .AddTextField( "contactEmail", a => a.ContactEmail )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <param name="eventItemOccurrence">The event item occurrence.</param>
        /// <returns></returns>
        private DateTimeOffset? GetNextStartDateTime( EventItemOccurrence eventItemOccurrence )
        {
            DateTime? nextStartDate = null;

            if ( FilterStartDate.HasValue || FilterEndDate.HasValue )
            {
                var lowerDateRange = FilterStartDate ?? FilterEndDate.Value.AddYears( -1 ).AddDays( 1 );
                var upperDateRange = FilterEndDate ?? FilterStartDate.Value.AddYears( 1 ).AddDays( -1 );

                var startDateTimes = eventItemOccurrence.GetStartTimes( lowerDateRange, upperDateRange.AddDays( 1 ) );

                if ( startDateTimes.Count > 0 )
                {
                    nextStartDate = startDateTimes.Min();
                }
            }
            else
            {
                nextStartDate = eventItemOccurrence.NextStartDateTime;
                var lowerDateRange = RockDateTime.Today;
                var upperDateRange = lowerDateRange.AddYears( 1 ).AddDays( -1 );

                var startDateTimes = eventItemOccurrence.GetStartTimes( lowerDateRange, upperDateRange.AddDays( 1 ) );

                if ( startDateTimes.Count > 0 )
                {
                    nextStartDate = startDateTimes.Min();
                }
            }

            return nextStartDate.HasValue ? nextStartDate.Value.ToRockDateTimeOffset() : ( DateTimeOffset? ) null;
        }

        /// <summary>
        /// Formats the Content Items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private string FormatContentItems( IEnumerable<ContentChannelItem> items )
        {
            var qryParams = new Dictionary<string, string> { { "ContentItemId", "" } };

            var itemLinks = new List<string>();
            foreach ( var item in items )
            {
                qryParams["ContentItemId"] = item.Id.ToString();
                itemLinks.Add( string.Format( $"{this.GetLinkedPageUrl( AttributeKey.ContentItemDetailPage, qryParams )}|{item.Title}|{item.ContentChannelType.Name}" ) );
            }
            return itemLinks.AsDelimited( "," );
        }

        /// <summary>
        /// Gets the Event Item from the PageParameters or from the block's context entity.
        /// </summary>
        /// <returns><see cref="EventCalendar"/></returns>
        private EventItem GetEventItem()
        {
            if ( _eventItem != null )
            {
                return _eventItem;
            }

            var eventCalendarId = PageParameter( PageParameterKey.EventItemId );

            if ( eventCalendarId.IsNotNullOrWhiteSpace() )
            {
                _eventItem = new EventItemService( RockContext ).Get( eventCalendarId, !PageCache.Layout.Site.DisablePredictableIds );
            }

            return _eventItem;
        }

        /// <summary>
        /// Gets an appropriate Event Calendar identifier for a given Event Item.  If the calendar id is specified in the page parameter
        /// collection, that value will be used, if not the calendar id from the first Event Calendar Item attached to the Event Item will
        /// be used.
        /// NOTE: This is necessary because this block has been used on pages/routes which do not supply the event calendar id (e.g., the link
        /// from a followed event).
        /// </summary>
        /// <param name="eventItem">The <see cref="EventItem"/>.</param>
        /// <returns></returns>
        private string GetEventCalendarIdKey( EventItem eventItem )
        {
            var calendarId = PageParameter( PageParameterKey.EventCalendarId );
            if ( !string.IsNullOrWhiteSpace( calendarId ) )
            {
                return calendarId;
            }

            var calendarItem = eventItem?.EventCalendarItems?.FirstOrDefault();
            if ( calendarItem != null )
            {
                return calendarItem.EventCalendar.IdKey;
            }

            return string.Empty;
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
            var entityService = new EventItemOccurrenceService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{EventItemOccurrence.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${EventItemOccurrence.FriendlyTypeName}." );
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
        /// Creates a copy of the specified Event Item Occurrence.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            var linkedPageUrl = string.Empty;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DetailPage ) ) )
            {
                var eventItemOccurrence = new EventItemOccurrenceService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                var eventItem = GetEventItem();

                if ( eventItemOccurrence != null )
                {
                    var qryParams = new Dictionary<string, string>
                    {
                        { PageParameterKey.EventCalendarId, GetEventCalendarIdKey( eventItem ) },
                        { PageParameterKey.EventItemId, GetEventItem()?.IdKey },
                        { PageParameterKey.EventItemOccurrenceId, "0" },
                        { PageParameterKey.CopyFromId, eventItemOccurrence.IdKey }
                    };

                    linkedPageUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams );
                }
            }

            return ActionOk( linkedPageUrl );
        }

        #endregion
    }
}

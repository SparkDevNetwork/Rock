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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Event.CalendarNavigation;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a wizard-style trail of the current Event Calendar administration
    /// context (Calendar, Event, Occurrence, Content Item) and lets the user step
    /// back up the page hierarchy.
    /// </summary>
    [DisplayName( "Calendar Navigation" )]
    [Category( "Event" )]
    [Description( "Displays icons to help with calendar administration navigation." )]
    [IconCssClass( "ti ti-border-all" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Navigation )]
    [Rock.SystemGuid.EntityTypeGuid( "2E57FFE8-961C-4073-94FE-B7CC4BD700C0" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "C4B7E2EB-56F3-4052-BEAC-B94EE7335C34" )]
    [Rock.SystemGuid.BlockTypeGuid( "84CC5DAC-238E-48B5-8499-8E97FB289EA9" )]
    public class CalendarNavigation : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemId = "EventItemId";
            public const string EventItemOccurrenceId = "EventItemOccurrenceId";
            public const string ContentItemId = "ContentItemId";
        }

        #endregion

        #region Constants

        // Wizard step levels, top of the hierarchy first. The level number is the
        // step's position in the trail and its depth in the page hierarchy.
        private const int CalendarsLevel = 1;
        private const int CalendarLevel = 2;
        private const int EventLevel = 3;
        private const int OccurrenceLevel = 4;
        private const int ContentItemLevel = 5;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CalendarNavigationOptionsBag
            {
                Items = BuildItems()
            };
        }

        /// <summary>
        /// Builds the ordered list of wizard steps for the current calendar
        /// administration context.
        /// </summary>
        private List<CalendarNavigationItemBag> BuildItems()
        {
            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;
            var currentPerson = RequestContext.CurrentPerson;

            // Resolve each directly-provided page parameter (Id, IdKey, or Guid).
            var eventCalendar = EventCalendarCache.Get( PageParameter( PageParameterKey.EventCalendarId ), allowIntegerIds );
            var contentItem = ContentChannelItemCache.Get( PageParameter( PageParameterKey.ContentItemId ), allowIntegerIds );
            var eventItem = GetEventItem( allowIntegerIds );
            var occurrence = GetOccurrence( allowIntegerIds );

            // The active step is the deepest directly-provided context. This is
            // captured before trail-filling so derived ancestors don't shift it.
            var activeLevel =
                contentItem != null ? ContentItemLevel :
                occurrence != null ? OccurrenceLevel :
                eventItem != null ? EventLevel :
                eventCalendar != null ? CalendarLevel :
                CalendarsLevel;

            // Fill in the ancestor trail so parent steps show their names and can
            // be linked, even when only a deeper parameter was supplied.
            if ( occurrence != null && eventItem == null )
            {
                eventItem = occurrence.EventItem;
            }

            if ( eventItem != null && eventCalendar == null )
            {
                eventCalendar = GetPreferredCalendar( eventItem, currentPerson );
            }

            return new List<CalendarNavigationItemBag>
            {
                BuildItem( CalendarsLevel, "ti ti-fw ti-border-all", "Event Calendars", activeLevel, eventCalendar, eventItem, occurrence ),
                BuildItem( CalendarLevel, "ti ti-fw ti-calendar", eventCalendar?.Name ?? "Calendar", activeLevel, eventCalendar, eventItem, occurrence ),
                BuildItem( EventLevel, "ti ti-fw ti-calendar-check", eventItem?.Name ?? "Event", activeLevel, eventCalendar, eventItem, occurrence ),
                BuildItem( OccurrenceLevel, "ti ti-fw ti-clock", GetOccurrenceLabel( occurrence ), activeLevel, eventCalendar, eventItem, occurrence ),
                BuildItem( ContentItemLevel, "ti ti-fw ti-speakerphone", contentItem?.Title ?? "Content Item", activeLevel, eventCalendar, eventItem, occurrence )
            };
        }

        /// <summary>
        /// Resolves the Event Item from its page parameter, eager-loading the
        /// calendar links used to derive a calendar for the trail.
        /// </summary>
        private EventItem GetEventItem( bool allowIntegerIds )
        {
            var key = PageParameter( PageParameterKey.EventItemId );
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new EventItemService( RockContext )
                .GetQueryableByKey( key, allowIntegerIds )
                .Include( i => i.EventCalendarItems )
                .AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Resolves the Event Item Occurrence from its page parameter, eager-loading
        /// the Event Item and its calendar links so the ancestor trail can be built
        /// without extra round trips.
        /// </summary>
        private EventItemOccurrence GetOccurrence( bool allowIntegerIds )
        {
            var key = PageParameter( PageParameterKey.EventItemOccurrenceId );
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new EventItemOccurrenceService( RockContext )
                .GetQueryableByKey( key, allowIntegerIds )
                .Include( o => o.EventItem.EventCalendarItems )
                .AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Chooses the calendar to represent an event, preferring the first one the
        /// current person can edit and otherwise falling back to the first linked
        /// calendar.
        /// </summary>
        private EventCalendarCache GetPreferredCalendar( EventItem eventItem, Person currentPerson )
        {
            EventCalendarCache preferredCalendar = null;

            foreach ( var calendarItem in eventItem.EventCalendarItems )
            {
                var calendar = EventCalendarCache.Get( calendarItem.EventCalendarId );
                if ( calendar == null )
                {
                    continue;
                }

                preferredCalendar = preferredCalendar ?? calendar;
                if ( calendar.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return calendar;
                }
            }

            return preferredCalendar;
        }

        /// <summary>
        /// Gets the label for the occurrence step: the campus name, "All Campuses"
        /// for a campus-independent occurrence, or the default when none is in scope.
        /// </summary>
        private string GetOccurrenceLabel( EventItemOccurrence occurrence )
        {
            if ( occurrence == null )
            {
                return "Event Occurrence";
            }

            if ( occurrence.CampusId.HasValue )
            {
                return CampusCache.Get( occurrence.CampusId.Value )?.Name ?? "All Campuses";
            }

            return "All Campuses";
        }

        /// <summary>
        /// Builds a single wizard step, computing its state and (for completed,
        /// reachable ancestor steps) its navigation URL.
        /// </summary>
        private CalendarNavigationItemBag BuildItem( int level, string iconCssClass, string label, int activeLevel, EventCalendarCache eventCalendar, EventItem eventItem, EventItemOccurrence occurrence )
        {
            return new CalendarNavigationItemBag
            {
                IconCssClass = iconCssClass,
                Label = label,
                IsActive = level == activeLevel,
                IsComplete = level < activeLevel,
                Url = BuildLevelUrl( level, activeLevel, eventCalendar, eventItem, occurrence )
            };
        }

        /// <summary>
        /// Builds the URL a completed wizard step navigates to, walking up the page
        /// hierarchy and carrying the ancestor context forward. Returns null when the
        /// step is not a reachable ancestor (the active/future steps, the terminal
        /// Content Item step, or a step whose entity is not in scope).
        /// </summary>
        private string BuildLevelUrl( int level, int activeLevel, EventCalendarCache eventCalendar, EventItem eventItem, EventItemOccurrence occurrence )
        {
            // Only completed steps below the terminal Content Item step are navigable.
            if ( level >= activeLevel || level >= ContentItemLevel )
            {
                return null;
            }

            // The entity a step represents must be resolvable to navigate to it.
            if ( ( level == CalendarLevel && eventCalendar == null ) ||
                 ( level == EventLevel && eventItem == null ) ||
                 ( level == OccurrenceLevel && occurrence == null ) )
            {
                return null;
            }

            var targetPage = GetAncestorPage( activeLevel - level );
            if ( targetPage == null )
            {
                return null;
            }

            var queryParams = new Dictionary<string, string>();
            if ( level >= CalendarLevel && eventCalendar != null )
            {
                queryParams[PageParameterKey.EventCalendarId] = eventCalendar.IdKey;
            }

            if ( level >= EventLevel && eventItem != null )
            {
                queryParams[PageParameterKey.EventItemId] = eventItem.IdKey;
            }

            if ( level >= OccurrenceLevel && occurrence != null )
            {
                queryParams[PageParameterKey.EventItemOccurrenceId] = occurrence.IdKey;
            }

            return new PageReference( targetPage.Id )
            {
                Parameters = queryParams
            }.BuildUrl();
        }

        /// <summary>
        /// Walks up the page hierarchy the given number of levels from the current
        /// page. Returns null if the hierarchy is shallower than requested.
        /// </summary>
        private PageCache GetAncestorPage( int levelsUp )
        {
            var page = PageCache;
            for ( var i = 0; i < levelsUp && page != null; i++ )
            {
                page = page.ParentPage;
            }

            return page;
        }

        #endregion
    }
}

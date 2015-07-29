// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Calendar Item List Lava" )]
    [Category( "Event" )]
    [Description( "Renders calendar items using Lava." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "1", order: 0 )]
    [CampusesField( "Campuses", "List of which campuses to show occurences for. This setting will be ignored in the 'Use Campus Context' is enabled.", order: 1 )]
    [BooleanField( "Use Campus Context", "Determine if the campus should be read from the campus context of the page.", order: 2 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 3 )]
    [SlidingDateRangeField( "Date Range", "Optional date range to filter the items on. (defaults to next 1000 days)", false, order: 4 )]
    [IntegerField( "Max Occurrences", "The maximum number of occurrences to show.", false, 100, order: 5 )]

    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Liquid, CodeEditorTheme.Rock, defaultValue: "{% include '~~/Assets/Lava/EventItemList.lava' %}", order: 6 )]
    [BooleanField( "Enable Debug", "Show the lava merge fields.", order: 7 )]
    public partial class EventItemListLava : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the calendar event dates.
        /// </summary>
        /// <value>
        /// The calendar event dates.
        /// </value>
        private List<DateTime> CalendarEventDates { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadContent();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        private void LoadContent()
        {
            var rockContext = new RockContext();
            var eventCalendarGuid = GetAttributeValue( "EventCalendar" ).AsGuid();
            var eventCalendar = new EventCalendarService( rockContext ).Get( eventCalendarGuid );

            if ( eventCalendar == null )
            {
                lMessages.Text = "<div class='alert alert-warning'>No event calendar is configured for this block.</div>";
                lContent.Text = string.Empty;
                return;
            }
            else
            {
                lMessages.Text = string.Empty;
            }
            
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Grab events
            // NOTE: Do not use AsNoTracking() so that things can be lazy loaded if needed
            var qry = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.EventItemAudiences,EventItemSchedules.Schedule" )
                    .Where( m =>
                        m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == eventCalendar.Id ) &&
                        m.EventItem.IsActive );

            // Filter by campus (always include the "All Campuses" events)
            if ( GetAttributeValue( "UseCampusContext" ).AsBoolean() )
            {
                var campusEntityType = EntityTypeCache.Read<Campus>();
                var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                qry = qry.Where( e => e.CampusId == contextCampus.Id || !e.CampusId.HasValue );
            }
            else
            {
                var campusGuidList = GetAttributeValue( "Campuses" ).Split( ',' ).AsGuidList();
                if ( campusGuidList.Any() )
                {
                    qry = qry.Where( e => !e.CampusId.HasValue ||  campusGuidList.Contains( e.Campus.Guid ) );
                }
            }

            // make sure they have a date range
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "DateRange" ) );
            var today = RockDateTime.Today;
            dateRange.Start = dateRange.Start ?? today;
            if ( dateRange.End == null )
            {
                dateRange.End = dateRange.Start.Value.AddDays( 1000 );
            }

            // Get the occurrences 
            var occurrences = qry.ToList();
            var occurrencesWithDates = occurrences
                .Select( o => new EventOccurrenceDate
                {
                    EventItemOccurrence = o,
                    Dates = o.GetStartTimes( dateRange.Start.Value, dateRange.End.Value ).ToList()
                } )
                .Where( d => d.Dates.Any() )
                .ToList();

            CalendarEventDates = new List<DateTime>();

            var eventCampusSummaries = new List<EventOccurrenceSummary>();
            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;
                foreach ( var datetime in occurrenceDates.Dates )
                {
                    CalendarEventDates.Add( datetime.Date );

                    if ( datetime >= dateRange.Start.Value && datetime < dateRange.End.Value )
                    {
                        eventCampusSummaries.Add( new EventOccurrenceSummary
                        {
                            EventItemOccurrence = eventItemOccurrence,
                            Name = eventItemOccurrence.EventItem.Name,
                            DateTime = datetime,
                            Date = datetime.ToShortDateString(),
                            Time = datetime.ToShortTimeString(),
                            Location = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            Description = eventItemOccurrence.EventItem.Description,
                            Summary = eventItemOccurrence.EventItem.Summary,
                            DetailPage = string.IsNullOrWhiteSpace( eventItemOccurrence.EventItem.DetailsUrl ) ? null : eventItemOccurrence.EventItem.DetailsUrl
                        } );
                    }
                }
            }

            eventCampusSummaries = eventCampusSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            // limit results
            int? maxItems = GetAttributeValue( "MaxOccurrences" ).AsIntegerOrNull();
            if ( maxItems.HasValue )
            {
                eventCampusSummaries = eventCampusSummaries.Take( maxItems.Value ).ToList();
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );
            mergeFields.Add( "EventCampusSummaries", eventCampusSummaries );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lContent.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store event item occurrence data for liquid
        /// </summary>
        [DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Time", "Location", "Description", "Summary", "DetailPage" )]
        public class EventOccurrenceSummary
        {
            /// <summary>
            /// Gets or sets the event item occurrence.
            /// </summary>
            /// <value>
            /// The event item occurrence.
            /// </value>
            public EventItemOccurrence EventItemOccurrence { get; set; }

            /// <summary>
            /// Gets or sets the date time.
            /// </summary>
            /// <value>
            /// The date time.
            /// </value>
            public DateTime DateTime { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            /// <value>
            /// The date.
            /// </value>
            public string Date { get; set; }

            /// <summary>
            /// Gets or sets the time.
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public string Time { get; set; }

            /// <summary>
            /// Gets or sets the location.
            /// </summary>
            /// <value>
            /// The location.
            /// </value>
            public string Location { get; set; }

            /// <summary>
            /// Gets or sets the summary.
            /// </summary>
            /// <value>
            /// The summary.
            /// </value>
            public string Summary { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the detail page.
            /// </summary>
            /// <value>
            /// The detail page.
            /// </value>
            public string DetailPage { get; set; }
        }

        /// <summary>
        /// A class to store the event item occurrences dates
        /// </summary>
        public class EventOccurrenceDate
        {
            /// <summary>
            /// Gets or sets the event item occurrence.
            /// </summary>
            /// <value>
            /// The event item occurrence.
            /// </value>
            public EventItemOccurrence EventItemOccurrence { get; set; }

            /// <summary>
            /// Gets or sets the dates.
            /// </summary>
            /// <value>
            /// The dates.
            /// </value>
            public List<DateTime> Dates { get; set; }
        }

        #endregion
    }
}
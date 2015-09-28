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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Popup Calendar" )]
    [Category( "com_centralaz > widgets" )]
    [Description( "Renders a particular calendar using Lava." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "1", order: 0 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 2 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the search results.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_centralaz/Widgets/Lava/PopupCalendar.lava' %}", "", 3)]
    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of day", true, DayOfWeek.Sunday, order: 4 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 5 )]

    public partial class PopupCalendar : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _calendarId = 0;
        private string _calendarName = string.Empty;
        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        #endregion

        #region Properties

        private DateTime? SelectedDate { get; set; }
        private List<DateTime> CalendarEventDates { get; set; }

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SelectedDate = ViewState["SelectedDate"] as DateTime?;
            CalendarEventDates = ViewState["CalendarEventDates"] as List<DateTime>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _firstDayOfWeek = GetAttributeValue( "StartofWeekDay" ).ConvertToEnum<DayOfWeek>();

            var eventCalendar = new EventCalendarService( new RockContext() ).Get( GetAttributeValue( "EventCalendar" ).AsGuid() );
            if ( eventCalendar != null )
            {
                _calendarId = eventCalendar.Id;
                _calendarName = eventCalendar.Name;
            }

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

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {

                BindData();

            }

        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["SelectedDate"] = SelectedDate;
            ViewState["CalendarEventDates"] = CalendarEventDates;

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the calEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void calEventCalendar_SelectionChanged( object sender, EventArgs e )
        {
            ResetCalendarSelection();
            BindData();
            if ( CalendarEventDates.Any( d => d.Date == calEventCalendar.SelectedDate ) )
            {
                pnlPopup.Visible = true;
            }
        }

        /// <summary>
        /// Handles the DayRender event of the calEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DayRenderEventArgs"/> instance containing the event data.</param>
        protected void calEventCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( CalendarEventDates != null && CalendarEventDates.Any( d => d.Date.Equals( day.Date ) ) )
            {
                e.Cell.AddCssClass( "calendar-hasevent" );
            }
        }

        /// <summary>
        /// Handles the VisibleMonthChanged event of the calEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MonthChangedEventArgs"/> instance containing the event data.</param>
        protected void calEventCalendar_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            calEventCalendar.SelectedDate = e.NewDate;
            ResetCalendarSelection();
            BindData();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads and displays the event item occurrences
        /// </summary>
        private void BindData()
        {
            var rockContext = new RockContext();
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Grab events
            var qry = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.EventItemAudiences,Schedule" )
                    .Where( m =>
                        m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == _calendarId ) &&
                        m.EventItem.IsActive );

            // Filter by campus
            var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Read( "Rock.Model.Campus" ) ) as Campus;
            if ( contextCampus != null )
            {
                qry = qry
                    .Where( c =>
                        !c.CampusId.HasValue ||    // All
                        contextCampus.Id == c.CampusId.Value );
            }

            // Get the beginning and end dates
            var today = RockDateTime.Today;
            var filterStart = SelectedDate.HasValue ? SelectedDate.Value : today;
            var monthStart = new DateTime( filterStart.Year, filterStart.Month, 1 );
            var rangeStart = monthStart.AddMonths( -1 );
            var rangeEnd = monthStart.AddMonths( 2 );
            var beginDate = SelectedDate.HasValue ? SelectedDate.Value : rangeStart;
            var endDate = SelectedDate.HasValue ? SelectedDate.Value : rangeEnd;

            endDate = endDate.AddDays( 1 ).AddMilliseconds( -1 );

            // Get the occurrences 
            var occurrences = qry.ToList();
            var occurrencesWithDates = occurrences
                .Select( o => new EventOccurrenceDate
                {
                    EventItemOccurrence = o,
                    Dates = o.GetStartTimes( rangeStart, rangeEnd ).ToList()
                } )
                .Where( d => d.Dates.Any() )
                .ToList();

            CalendarEventDates = new List<DateTime>();

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();
            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;
                foreach ( var datetime in occurrenceDates.Dates )
                {
                    CalendarEventDates.Add( datetime.Date );

                    if ( datetime >= beginDate && datetime < endDate )
                    {
                        eventOccurrenceSummaries.Add( new EventOccurrenceSummary
                        {
                            EventItemOccurrence = eventItemOccurrence,
                            Name = eventItemOccurrence.EventItem.Name,
                            DateTime = datetime,
                            Date = datetime.ToShortDateString(),
                            Time = datetime.ToShortTimeString(),
                            Location = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            Description = eventItemOccurrence.EventItem.Description,
                            Summary = eventItemOccurrence.EventItem.Summary,
                            DetailPage = String.IsNullOrWhiteSpace( eventItemOccurrence.EventItem.DetailsUrl ) ? null : eventItemOccurrence.EventItem.DetailsUrl
                        } );
                    }
                }
            }

            eventOccurrenceSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );
            mergeFields.Add( "EventItemOccurrences", eventOccurrenceSummaries );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

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

        /// <summary>
        /// Resets the calendar selection. The control is configured for day selection, but selection will be changed to the week or month if that is the viewmode being used
        /// </summary>
        private void ResetCalendarSelection()
        {
            // Even though selection will be a single date due to calendar's selection mode, set the appropriate days
            var selectedDate = calEventCalendar.SelectedDate;
            SelectedDate = selectedDate;

            // Reset the selection
            calEventCalendar.SelectedDates.SelectRange( SelectedDate.Value, SelectedDate.Value );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store event item occurrence data for liquid
        /// </summary>
        [DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Time", "Location", "Description", "Summary", "DetailPage" )]
        public class EventOccurrenceSummary
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }
            public DateTime DateTime { get; set; }
            public String Name { get; set; }
            public String Date { get; set; }
            public String Time { get; set; }
            public String Location { get; set; }
            public String Summary { get; set; }
            public String Description { get; set; }
            public String DetailPage { get; set; }
        }

        /// <summary>
        /// A class to store the event item occurrences dates
        /// </summary>
        public class EventOccurrenceDate
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }
            public List<DateTime> Dates { get; set; }
        }

        #endregion

        protected void mdDetails_SaveClick( object sender, EventArgs e )
        {

        }
    }
}
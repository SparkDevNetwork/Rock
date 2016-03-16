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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;

namespace RockWeb.Plugins.com_centralaz.Calendar
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Simple Event List" )]
    [Category( "com_centralaz > Calendar" )]
    [Description( "Provides a simple list of events for crawlers to read." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "1" )]
    [LinkedPage( "Details Page", "Detail page for events" )]
    [IntegerField( "Number of Months", "The number of months to look ahead to find future events.", true, 4 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Themes/Stark/Assets/Lava/Calendar.lava' %}", "" )]

    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "" )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar name.", false )]

    public partial class SimpleEventList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _calendarId = 0;
        private string _calendarName = string.Empty;

        #endregion

        #region Base ControlMethods


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            if ( !Page.IsPostBack )
            {
                pnlDetails.Visible = true;
                BindData();
            }

        }

        protected override void OnPreRender( EventArgs e )
        {
            if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() && !string.IsNullOrWhiteSpace( _calendarName ) )
            {
                string pageTitle = _calendarName.EndsWith( "Calendar", StringComparison.OrdinalIgnoreCase ) ? _calendarName : string.Format( "{0} Calendar", _calendarName );
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            }

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
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

            // Get the beginning and end dates
            var rangeStart = RockDateTime.Today;
            var rangeEnd = rangeStart.AddMonths( GetAttributeValue( "NumberofMonths" ).AsInteger() );

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

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();
            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;
                foreach ( var datetime in occurrenceDates.Dates )
                {
                    if ( datetime >= rangeStart && datetime < rangeEnd )
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

    }
}
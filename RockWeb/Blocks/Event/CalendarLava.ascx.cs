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

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Calendar Lava" )]
    [Category( "Event" )]
    [Description( "Renders a particular calendar using Lava." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Themes/Stark/Assets/Lava/ExternalCalendar.lava' %}", "", 2 )]
    [BooleanField( "Show Campus Filter", "Determines whether the campus filters are shown", false )]
    [BooleanField( "Show Category Filter", "Determines whether the campus filters are shown", false )]
    [BooleanField( "Show Date Range Filter", "Determines whether the campus filters are shown", false )]
    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true )]
    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of day", true, DayOfWeek.Monday )]
    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month", true, "Week" )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar name.", false )]
    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "1" )]
    [LinkedPage( "Details Page", "Detail page for events" )]
    public partial class CalendarLava : Rock.Web.UI.RockBlock
    {
        #region Properties
        protected List<DateTime> _eventDates = null;

        /// <summary>
        /// Gets or sets the accounts that are available for user to add to the list.
        /// </summary>
        protected String CurrentViewMode
        {
            get
            {
                var CurrentViewMode = ViewState["CurrentViewMode"] as String;
                if ( String.IsNullOrWhiteSpace( CurrentViewMode ) )
                {
                    CurrentViewMode = GetAttributeValue( "DefaultViewOption" );
                }

                return CurrentViewMode;
            }

            set
            {
                ViewState["CurrentViewMode"] = value;
            }
        }

        protected DateTime? SelectedDate
        {
            get
            {
                var SelectedDate = ViewState["SelectedDate"] as DateTime?;
                if ( SelectedDate == null )
                {
                    SelectedDate = DateTime.Today;
                }

                return SelectedDate;
            }

            set
            {
                ViewState["SelectedDate"] = value;
            }
        }
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
            nbConfiguration.Visible = false;
            calEventCalendar.SelectedDate = SelectedDate.Value;
            calEventCalendar.FirstDayOfWeek = (FirstDayOfWeek)GetAttributeValue( "StartofWeekDay" ).AsInteger();
            if ( !Page.IsPostBack )
            {
                CheckValidConfiguration();
                LoadDropDowns();
                DisplayCalendarItemList();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            nbConfiguration.Visible = false;
            CheckValidConfiguration();
            DisplayCalendarItemList();
        }

        #endregion

        protected void calEventCalendar_SelectionChanged( object sender, EventArgs e )
        {
            SelectedDate = calEventCalendar.SelectedDate;
            drpDateRange.UpperValue = null;
            drpDateRange.LowerValue = null;
            DisplayCalendarItemList();
        }

        protected void calEventCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( day == Rock.RockDateTime.Today )
            {
                e.Cell.AddCssClass( "calendar-today" );
            }
            else if ( day == calEventCalendar.SelectedDate )
            {
                e.Cell.AddCssClass( "calendar-selecteditem" );
            }
            if ( _eventDates == null )
            {
                GrabData();
            }
            if ( _eventDates.Any( d => d.Date.Equals( day.Date ) ) )
            {
                e.Cell.Style.Add( "font-weight", "bold" );
            }
            if ( CurrentViewMode == "Week" )
            {
                var weekStartDay = (DayOfWeek)GetAttributeValue( "StartofWeekDay" ).AsInteger();
                if ( day.StartOfWeek( weekStartDay ) == calEventCalendar.SelectedDate.StartOfWeek( weekStartDay ) )
                {
                    e.Cell.AddCssClass( "calendar-selecteditem" );
                }
            }
            if ( CurrentViewMode == "Month" )
            {
                if ( day.Month == calEventCalendar.SelectedDate.Month )
                {
                    e.Cell.AddCssClass( "calendar-selecteditem" );
                }
            }

        }

        protected void calEventCalendar_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            if ( e.NewDate.Year > e.PreviousDate.Year )
            {
                calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddMonths( 1 );
            }
            else if ( e.NewDate.Year < e.PreviousDate.Year )
            {
                calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddMonths( -1 );
            }
            else if ( e.NewDate.Month > e.PreviousDate.Month )
            {
                try
                {
                    calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddMonths( 1 );
                }
                catch
                {
                    try
                    {
                        calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddDays( -1 ).AddMonths( 1 );
                    }
                    catch
                    {
                        try
                        {
                            calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddDays( -2 ).AddMonths( 1 );
                        }
                        catch
                        {
                            calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddDays( -3 ).AddMonths( 1 );
                        }
                    }
                }
            }
            else
            {
                try
                {
                    calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddMonths( -1 );
                }
                catch
                {
                    try
                    {
                        calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddDays( -1 ).AddMonths( -1 );
                    }
                    catch
                    {
                        try
                        {
                            calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddDays( -2 ).AddMonths( -1 );
                        }
                        catch
                        {
                            calEventCalendar.SelectedDate = calEventCalendar.SelectedDate.AddDays( -3 ).AddMonths( -1 );
                        }
                    }
                }
            }
            SelectedDate = calEventCalendar.SelectedDate;
            drpDateRange.UpperValue = null;
            drpDateRange.LowerValue = null;
            DisplayCalendarItemList();
        }

        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            DisplayCalendarItemList();
        }

        protected void cblCategory_SelectedIndexChanged( object sender, EventArgs e )
        {
            DisplayCalendarItemList();
        }

        protected void btnDay_Click( object sender, EventArgs e )
        {
            CurrentViewMode = "Day";
            DisplayCalendarItemList();
        }

        protected void btnWeek_Click( object sender, EventArgs e )
        {
            CurrentViewMode = "Week";
            DisplayCalendarItemList();
        }

        protected void btnMonth_Click( object sender, EventArgs e )
        {
            CurrentViewMode = "Month";
            DisplayCalendarItemList();
        }

        #region Internal Methods


        /// <summary>
        /// Checks if the default view mode is one that is enabled.
        /// </summary>
        private void CheckValidConfiguration()
        {
            if ( !GetAttributeValue( string.Format( "Show{0}View", GetAttributeValue( "DefaultViewOption" ) ) ).AsBoolean() )
            {
                nbConfiguration.Text = "The default view option is one that is not enabled.";
                nbConfiguration.Visible = true;
            }
        }

        private void LoadDropDowns()
        {
            cblCategory.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
        }

        private void DisplayCalendarItemList()
        {
            pnlFilters.Visible = true;
            pnlList.CssClass = "col-md-9";
            cblCampus.Visible = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
            cblCategory.Visible = GetAttributeValue( "ShowCategoryFilter" ).AsBoolean();
            drpDateRange.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            btnDay.Visible = GetAttributeValue( "ShowDayView" ).AsBoolean();
            btnWeek.Visible = GetAttributeValue( "ShowWeekView" ).AsBoolean();
            btnMonth.Visible = GetAttributeValue( "ShowMonthView" ).AsBoolean();
            if ( !GetAttributeValue( "ShowSmallCalendar" ).AsBoolean() )
            {
                pnlCalendar.Visible = false;
                if ( !cblCampus.Visible && !cblCategory.Visible && !drpDateRange.Visible )
                {
                    pnlFilters.Visible = false;
                    pnlList.CssClass = "col-md-12";
                }
            }
            else
            {
                pnlCalendar.Visible = true;
            }

            //This hides all buttons if only one view is enabled. Logic is based off of http://stackoverflow.com/questions/5343772, and was verified by truth table
            if ( ( btnDay.Visible != btnWeek.Visible ) != btnMonth.Visible )
            {
                btnDay.Visible = false;
                btnWeek.Visible = false;
                btnMonth.Visible = false;
            }
            var events = GrabData();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Events", events );
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "TimeFrame", CurrentViewMode );
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );


            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() )
            {
                string pageTitle = "Calendar";
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            }

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        private List<EventSummary> GrabData()
        {
            // get package id
            int eventCalendarId = -1;
            var rockContext = new RockContext();
            try
            {
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "EventCalendar" ) ) )
                {
                    eventCalendarId = new EventCalendarService( rockContext ).Get( GetAttributeValue( "EventCalendar" ).AsGuid() ).Id;
                }
            }
            catch
            {
                nbConfiguration.Text = "No event calendar selected";
                nbConfiguration.Visible = true;
            }


            EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( new RockContext() );

            // Grab events
            var qry = eventCalendarItemService
                    .Queryable( "EventCalendar,EventItem.EventItemAudiences,EventItem.EventItemCampuses.EventItemSchedules.Schedule" )
                    .AsNoTracking()
                    .Where( m =>
                        m.EventCalendarId == eventCalendarId &&
                        m.EventItem.IsActive );

            // Filter by campus
            List<int> campusIds = cblCampus.SelectedValuesAsInt;
            if ( campusIds.Any() )
            {
                qry = qry
                    .Where( e =>
                        e.EventItem.EventItemCampuses
                            .Any( c =>
                                !c.CampusId.HasValue ||
                                campusIds.Contains( c.CampusId.Value ) ) );
            }

            //Filter by Audience
            List<int> categories = cblCategory.SelectedValuesAsInt;
            if ( categories.Any() )
            {
                qry = qry.Where( i => i.EventItem.EventItemAudiences
                    .Any( c => categories.Contains( c.DefinedValueId ) ) );
            }

            // Query the DB now
            var items = qry.ToList();

            //Daterange 
            DateTime beginDateTime = drpDateRange.LowerValue ?? calEventCalendar.SelectedDate.AddMonths( -1 );
            DateTime endDateTime = drpDateRange.UpperValue ?? calEventCalendar.SelectedDate.AddMonths( 1 );

            var itemsWithDates = items
                .Select( i => new
                {
                    Item = i,
                    Dates = i.EventItem.GetStartTimes( beginDateTime, endDateTime )
                } )
                .Where( i => i.Dates.Any() )
                .ToList();

            if ( _eventDates == null )
            {
                _eventDates = new List<DateTime>();
            }
            foreach ( var itemWithDates in itemsWithDates )
            {
                foreach ( var datetime in itemWithDates.Dates )
                {
                    _eventDates.Add( datetime.Date );
                }
            }
            _eventDates = _eventDates.Distinct().ToList();

            //Calendar filter
            if ( CurrentViewMode == "Day" )
            {
                itemsWithDates = itemsWithDates
                    .Where( i => i.Dates
                        .Any( d => d.Date.Equals( calEventCalendar.SelectedDate ) ) )
                    .ToList();
            }

            if ( CurrentViewMode == "Week" )
            {
                itemsWithDates = itemsWithDates
                    .Where( i => i.Dates
                        .Any( d => d.Date.StartOfWeek( (DayOfWeek)GetAttributeValue( "StartofWeekDay" ).AsInteger() ).Equals( calEventCalendar.SelectedDate.StartOfWeek( (DayOfWeek)GetAttributeValue( "StartofWeekDay" ).AsInteger() ) ) ) )
                    .ToList();
            }
            if ( CurrentViewMode == "Month" )
            {
                itemsWithDates = itemsWithDates
                    .Where( i => i.Dates
                        .Any( d =>
                            d.Date.Year == calEventCalendar.SelectedDate.Year &&
                            d.Date.Month == calEventCalendar.SelectedDate.Month ) )
                    .ToList();
            }

            var eventSummaries = new List<EventSummary>();
            foreach ( var itemWithDates in itemsWithDates )
            {
                var eventItem = itemWithDates.Item.EventItem;

                foreach ( var datetime in itemWithDates.Dates )
                {
                    eventSummaries.Add( new EventSummary
                    {
                        Event = eventItem,
                        DateTime = datetime,
                        Date = datetime.ToShortDateString(),
                        Time = datetime.ToShortTimeString(),
                        Location = eventItem.EventItemCampuses
                            .ToList()
                            .Select( c => c.Campus != null ? c.Campus.Name : "All Campuses" )
                            .ToList()
                            .AsDelimited( "<br>" ),
                        Description = eventItem.Description,
                        DetailPage = String.IsNullOrWhiteSpace( eventItem.DetailsUrl ) ? null : eventItem.DetailsUrl
                    } );
                }
            }

            var events = eventSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Event.Name )
                .ToList();
            return events;
        }

        [DotLiquid.LiquidType( "Event", "DateTime", "Date", "Time", "Location", "Description", "DetailPage" )]
        public class EventSummary
        {
            public EventItem Event { get; set; }
            public DateTime DateTime { get; set; }
            public String Date { get; set; }
            public String Time { get; set; }
            public String Location { get; set; }
            public String Description { get; set; }
            public String DetailPage { get; set; }
        }

        #endregion

    }
}
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Ical.Net.DataTypes;
using Rock.Lava;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Calendar Lava" )]
    [Category( "Event" )]
    [Description( "Renders a particular calendar using Lava." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "8A444668-19AF-4417-9C74-09F842572974", order: 0 )]
    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month,Year,All", true, "Week", order: 1 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 2 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 3 )]

    [CampusesField( name: "Campuses", description: "Select campuses to display calendar events for. No selection will show all.", required: false, defaultCampusGuids: "", category: "", order: 4, key: "Campuses" )]
    [CustomRadioListField( "Campus Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 5 )]

    [CustomRadioListField( "Audience Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "CategoryFilterDisplayMode", order: 6 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Filter Audiences", "Determines which audiences should be displayed in the filter.", false, true, key: "FilterCategories", order: 7 )]
    [BooleanField( "Show Date Range Filter", "Determines whether the date range filters are shown", false, order: 8 )]

    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true, order: 9 )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false, order: 10 )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true, order: 11 )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true, order: 12 )]
    [BooleanField( "Show Year View", "Determines whether the year view option is shown", false, order: 13 )]
    [BooleanField( "Show All View", "Determines whether the all view option is shown (Limited to 2 years)", false, order: 14 )]

    [BooleanField( "Enable Campus Context", "If the page has a campus context its value will be used as a filter", order: 15 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/Calendar.lava' %}", "", 16 )]

    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of a week.", true, DayOfWeek.Sunday, order: 17 )]

    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar name.", false, order: 18 )]

    [TextField( "Campus Parameter Name", "The page parameter name that contains the id of the campus entity.", false, "CampusId", order: 19 )]
    [TextField( "Category Parameter Name", "The page parameter name that contains the id of the category entity.", false, "CategoryId", order: 20 )]
    [TextField( "Date Parameter Name", "The page parameter name that contains the selected date.", false, "Date", order: 21 )]

    public partial class CalendarLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _calendarId = 0;
        private string _calendarName = string.Empty;

        /// <summary>
        /// NOTE: this is Sunday vs RockDateTime.FirstDayOfWeek since it is used to show the selected week/month in the Calendar control
        /// </summary>
        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        protected bool CampusPanelOpen { get; set; }

        protected bool CampusPanelClosed { get; set; }

        protected bool CategoryPanelOpen { get; set; }

        protected bool CategoryPanelClosed { get; set; }

        #endregion

        #region Properties

        private string ViewMode { get; set; }

        private DateTime? SelectedDate { get; set; }

        private DateTime? FilterStartDate { get; set; }

        private DateTime? FilterEndDate { get; set; }

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

            ViewMode = ViewState["ViewMode"] as string;
            SelectedDate = ViewState["SelectedDate"] as DateTime?;
            FilterStartDate = ViewState["FilterStartDate"] as DateTime?;
            FilterEndDate = ViewState["FilterEndDate"] as DateTime?;
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

            CampusPanelOpen = GetAttributeValue( "CampusFilterDisplayMode" ) == "3";
            CampusPanelClosed = GetAttributeValue( "CampusFilterDisplayMode" ) == "4";
            CategoryPanelOpen = !string.IsNullOrWhiteSpace( GetAttributeValue( "FilterCategories" ) ) && GetAttributeValue( "CategoryFilterDisplayMode" ) == "3";
            CategoryPanelClosed = !string.IsNullOrWhiteSpace( GetAttributeValue( "FilterCategories" ) ) && GetAttributeValue( "CategoryFilterDisplayMode" ) == "4";

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
                if ( SetFilterControls() )
                {
                    SelectedDate = DateTime.Now.Date;
                    pnlDetails.Visible = true;
                    BindData();
                }
                else
                {
                    pnlDetails.Visible = false;
                }
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
            ViewState["ViewMode"] = ViewMode;
            ViewState["SelectedDate"] = SelectedDate;
            ViewState["FilterStartDate"] = FilterStartDate;
            ViewState["FilterEndDate"] = FilterEndDate;
            ViewState["CalendarEventDates"] = CalendarEventDates;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() && !string.IsNullOrWhiteSpace( _calendarName ) )
            {
                string pageTitle = _calendarName.EndsWith( "Calendar", StringComparison.OrdinalIgnoreCase ) ? _calendarName : string.Format( "{0} Calendar", _calendarName );
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            }

            btnDay.CssClass = "btn btn-default" + ( ViewMode == "Day" ? " active" : string.Empty );
            btnWeek.CssClass = "btn btn-default" + ( ViewMode == "Week" ? " active" : string.Empty );
            btnMonth.CssClass = "btn btn-default" + ( ViewMode == "Month" ? " active" : string.Empty );
            btnYear.CssClass = "btn btn-default" + ( ViewMode == "Year" ? " active" : string.Empty );
            btnAll.CssClass = "btn btn-default" + ( ViewMode == "All" ? " active" : string.Empty );

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( SetFilterControls() )
            {
                pnlDetails.Visible = true;
                SetFilterControls();
                BindData();
            }
            else
            {
                pnlDetails.Visible = false;
            }
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
            SelectedDate = calEventCalendar.SelectedDate;
            ResetCalendarSelection();
            BindData();
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
            SelectedDate = e.NewDate;
            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCategory_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindData();
        }

        protected void lbDateRangeRefresh_Click( object sender, EventArgs e )
        {
            FilterStartDate = drpDateRange.LowerValue;
            FilterEndDate = drpDateRange.UpperValue;
            BindData();
        }

        /// <summary>
        /// Handles the Click event of the btnWeek control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewMode_Click( object sender, EventArgs e )
        {
            var btnViewMode = sender as BootstrapButton;
            if ( btnViewMode != null )
            {
                ViewMode = btnViewMode.Text;
                ResetCalendarSelection();
                BindData();

                if ( cblCampus.Items.Count == 1 )
                {
                    CampusPanelClosed = false;
                    CampusPanelOpen = false;
                    rcwCampus.Visible = false;
                }
            }
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
                        m.EventItem.IsActive &&
                        m.EventItem.IsApproved );

            // Filter by campus
            var campusGuidList = GetAttributeValue( "Campuses" ).Split( ',' ).AsGuidList();
            var campusIdList = CampusCache.All().Where( c => campusGuidList.Contains( c.Guid ) ).Select( c => c.Id );
            var selectedCampusIdList = cblCampus.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();

            if ( selectedCampusIdList.Any() )
            {
                // No value gets them all, otherwise get the ones selected
                // Block level campus filtering has already been performed on cblCampus, so no need to do it again here
                // If CampusId is null, then the event is an 'All Campuses' event, so include those
                qry = qry.Where( c => !c.CampusId.HasValue || selectedCampusIdList.Contains( c.CampusId.Value ) );
            }
            else if ( campusIdList.Any() )
            {
                // If no campus filter is selected then check the block filtering
                // If CampusId is null, then the event is an 'All Campuses' event, so include those
                qry = qry.Where( c => !c.CampusId.HasValue || campusIdList.Contains( c.CampusId.Value ) );
            }

            // Filter by Category
            List<int> categories = cblCategory.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            if ( categories.Any() )
            {
                qry = qry.Where( i => i.EventItem.EventItemAudiences.Any( c => categories.Contains( c.DefinedValueId ) ) );
            }

            // Get the beginning and end dates
            var today = RockDateTime.Today;
            var filterStart = FilterStartDate.HasValue ? FilterStartDate.Value : today;
            var monthStart = new DateTime( filterStart.Year, filterStart.Month, 1 );
            var rangeStart = monthStart.AddMonths( -1 );
            var rangeEnd = monthStart.AddMonths( 2 );
            var beginDate = FilterStartDate.HasValue ? FilterStartDate.Value : rangeStart;
            var endDate = FilterEndDate.HasValue ? FilterEndDate.Value : rangeEnd;

            endDate = endDate.AddDays( 1 ).AddMilliseconds( -1 );

            // Get the occurrences
            var occurrences = qry.ToList();
            var occurrencesWithDates = occurrences
                .Select( o =>
                {
                    var eventOccurrenceDate = new EventOccurrenceDate
                    {
                        EventItemOccurrence = o

                    };

                    if ( o.Schedule != null )
                    {
                        eventOccurrenceDate.ScheduleOccurrences = o.Schedule.GetICalOccurrences( beginDate, endDate ).ToList();
                    }
                    else
                    {
                        eventOccurrenceDate.ScheduleOccurrences = new List<Occurrence>();
                    }

                    return eventOccurrenceDate;
                } )
                .Where( d => d.ScheduleOccurrences.Any() )
                .ToList();

            CalendarEventDates = new List<DateTime>();

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();
            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;
                foreach ( var scheduleOccurrence in occurrenceDates.ScheduleOccurrences )
                {

                    var datetime = scheduleOccurrence.Period.StartTime.Value;
                    var occurrenceEndTime = scheduleOccurrence.Period.EndTime;
                    if ( occurrenceEndTime != null && occurrenceEndTime.Value.Date > datetime.Date )
                    {
                        var multiDate = datetime;
                        while ( multiDate <= occurrenceEndTime.Date && multiDate <= endDate )
                        {
                            CalendarEventDates.Add( multiDate.Date );
                            multiDate = multiDate.AddDays( 1 );
                        }
                    }
                    else
                    {
                        CalendarEventDates.Add( datetime.Date );
                    }

                    if ( datetime >= beginDate && datetime < endDate )
                    {
                        eventOccurrenceSummaries.Add( new EventOccurrenceSummary
                        {
                            EventItemOccurrence = eventItemOccurrence,
                            Name = eventItemOccurrence.EventItem.Name,
                            DateTime = datetime,
                            Date = datetime.ToShortDateString(),
                            Time = datetime.ToShortTimeString(),
                            EndDate = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortDateString() : null,
                            EndTime = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortTimeString() : null,
                            Campus = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            Location = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            LocationDescription = eventItemOccurrence.Location,
                            Description = eventItemOccurrence.EventItem.Description,
                            Summary = eventItemOccurrence.EventItem.Summary,
                            OccurrenceNote = eventItemOccurrence.Note.SanitizeHtml(),
                            DetailPage = string.IsNullOrWhiteSpace( eventItemOccurrence.EventItem.DetailsUrl ) ? null : eventItemOccurrence.EventItem.DetailsUrl
                        } );
                    }
                }
            }

            var eventSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .GroupBy( e => e.Name )
                .Select( e => e.ToList() )
                .ToList();

            eventOccurrenceSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "TimeFrame", ViewMode );
            mergeFields.Add( "StartDate", FilterStartDate );
            mergeFields.Add( "EndDate", FilterEndDate );
            mergeFields.Add( "DetailsPage", LinkedPageRoute( "DetailsPage" ) );
            mergeFields.Add( "EventItems", eventSummaries );
            mergeFields.Add( "EventItemOccurrences", eventOccurrenceSummaries );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private bool SetFilterControls()
        {
            // Get and verify the calendar id
            if ( _calendarId <= 0 )
            {
                ShowError( "Configuration Error", "The 'Event Calendar' setting has not been set correctly." );
                return false;
            }

            // Get and verify the view mode
            ViewMode = GetAttributeValue( "DefaultViewOption" );
            if ( !GetAttributeValue( string.Format( "Show{0}View", ViewMode ) ).AsBoolean() )
            {
                ShowError( "Configuration Error", string.Format( "The Default View Option setting has been set to '{0}', but the Show {0} View setting has not been enabled.", ViewMode ) );
                return false;
            }

            // Show/Hide calendar control
            pnlCalendar.Visible = GetAttributeValue( "ShowSmallCalendar" ).AsBoolean();

            // Get the first/last dates based on today's date and the viewmode setting
            var today = RockDateTime.Today;
            FilterStartDate = today;
            FilterEndDate = today;
            if ( ViewMode == "Week" )
            {
                FilterStartDate = today.StartOfWeek( _firstDayOfWeek );
                FilterEndDate = today.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                FilterStartDate = new DateTime( today.Year, today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 1 ).AddDays( -1 );
            }
            else if ( ViewMode == "Year" )
            {
                FilterStartDate = new DateTime( today.Year, today.Month, today.Day );
                FilterEndDate = new DateTime( today.Year, 12, 31 );
            }
            else if ( ViewMode == "All" )
            {
                FilterStartDate = new DateTime( today.Year, today.Month, today.Day );
                FilterEndDate = FilterStartDate.Value.AddDays( 730 );
            }

            // Setup small calendar Filter
            calEventCalendar.FirstDayOfWeek = _firstDayOfWeek.ConvertToInt().ToString().ConvertToEnum<FirstDayOfWeek>();
            calEventCalendar.SelectedDates.Clear();
            calEventCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );

            // Setup different dates if QueryString is set on load
            var selectedDate = PageParameter( GetAttributeValue( "DateParameterName" ) ).AsDateTime();
            if ( selectedDate.HasValue )
            {
                if ( selectedDate != null )
                {
                    SelectedDate = selectedDate;
                    ResetCalendarSelection();
                }
            }

            // Setup Campus Filter
            var campusGuidList = GetAttributeValue( "Campuses" ).Split( ',' ).AsGuidList();
            rcwCampus.Visible = GetAttributeValue( "CampusFilterDisplayMode" ).AsInteger() > 1;

            if ( campusGuidList.Any() )
            {
                cblCampus.DataSource = CampusCache.All( false ).Where( c => campusGuidList.Contains( c.Guid ) );
            }
            else
            {
                cblCampus.DataSource = CampusCache.All( false );
            }

            cblCampus.DataBind();

            if ( cblCampus.Items.Count == 1 )
            {
                CampusPanelClosed = false;
                CampusPanelOpen = false;
                rcwCampus.Visible = false;
            }

            // Check for Campus Parameter
            var campusId = PageParameter( GetAttributeValue( "CampusParameterName" ) ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                // Check if there's a campus with this id.
                var campus = CampusCache.Get( campusId.Value );
                if ( campus != null )
                {
                    cblCampus.SetValue( campusId.Value );
                }
            }
            else
            {
                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
                    var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus;
                    if ( contextCampus != null )
                    {
                        cblCampus.SetValue( contextCampus.Id );
                    }
                }
            }

            // Setup Category Filter
            var selectedCategoryGuids = GetAttributeValue( "FilterCategories" ).SplitDelimitedValues( true ).AsGuidList();
            rcwCategory.Visible = selectedCategoryGuids.Any() && GetAttributeValue( "CategoryFilterDisplayMode" ).AsInteger() > 1;
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if ( definedType != null )
            {
                cblCategory.DataSource = definedType.DefinedValues.Where( v => selectedCategoryGuids.Contains( v.Guid ) );
                cblCategory.DataBind();
            }

            var categoryId = PageParameter( GetAttributeValue( "CategoryParameterName" ) ).AsIntegerOrNull();
            if ( categoryId.HasValue )
            {
                if ( definedType.DefinedValues.Where( v => selectedCategoryGuids.Contains( v.Guid ) && v.Id == categoryId.Value ).FirstOrDefault() != null )
                {
                    cblCategory.SetValue( categoryId.Value );
                }
            }

            // Date Range Filter
            drpDateRange.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            lbDateRangeRefresh.Visible = drpDateRange.Visible;
            drpDateRange.LowerValue = FilterStartDate;
            drpDateRange.UpperValue = FilterEndDate;

            // Get the View Modes, and only show them if more than one is visible
            var viewsVisible = new List<bool> {
                GetAttributeValue( "ShowDayView" ).AsBoolean(),
                GetAttributeValue( "ShowWeekView" ).AsBoolean(),
                GetAttributeValue( "ShowMonthView" ).AsBoolean(),
                GetAttributeValue( "ShowYearView" ).AsBoolean(),
                GetAttributeValue( "ShowAllView" ).AsBoolean()
            };
            var howManyVisible = viewsVisible.Where( v => v ).Count();
            btnDay.Visible = howManyVisible > 1 && viewsVisible[0];
            btnWeek.Visible = howManyVisible > 1 && viewsVisible[1];
            btnMonth.Visible = howManyVisible > 1 && viewsVisible[2];
            btnYear.Visible = howManyVisible > 1 && viewsVisible[3];
            btnAll.Visible = howManyVisible > 1 && viewsVisible[4];

            // Set filter visibility
            bool showFilter = pnlCalendar.Visible || rcwCampus.Visible || rcwCategory.Visible || drpDateRange.Visible;
            pnlFilters.Visible = showFilter;
            pnlList.CssClass = showFilter ? "col-md-9" : "col-md-12";

            return true;
        }

        /// <summary>
        /// Resets the calendar selection. The control is configured for day selection, but selection will be changed to the week or month if that is the viewmode being used
        /// </summary>
        private void ResetCalendarSelection()
        {
            // Even though selection will be a single date due to calendar's selection mode, set the appropriate days
            if ( SelectedDate != null )
            {
                calEventCalendar.SelectedDate = SelectedDate.Value;
            }

            var selectedDate = calEventCalendar.SelectedDate;
            calEventCalendar.VisibleDate = calEventCalendar.SelectedDate;
            var today = RockDateTime.Today;
            FilterStartDate = selectedDate;
            FilterEndDate = selectedDate;
            if ( ViewMode == "Week" )
            {
                FilterStartDate = selectedDate.StartOfWeek( _firstDayOfWeek );
                FilterEndDate = selectedDate.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                FilterStartDate = new DateTime( selectedDate.Year, selectedDate.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 1 ).AddDays( -1 );
            }
            else if ( ViewMode == "Year" )
            {
                FilterStartDate = new DateTime( selectedDate.Year, selectedDate.Month, selectedDate.Day );
                FilterEndDate = new DateTime( selectedDate.Year, 12, 31 );
            }
            else if ( ViewMode == "All" )
            {
                FilterStartDate = new DateTime( today.Year, today.Month, today.Day );
                FilterEndDate = FilterStartDate.Value.AddDays( 730 );
            }

            // Reset the selection
            calEventCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );
        }

        private void SetCalendarFilterDates()
        {
            FilterStartDate = calEventCalendar.SelectedDates.Count > 0 ? calEventCalendar.SelectedDates[0] : ( DateTime? ) null;
            FilterEndDate = calEventCalendar.SelectedDates.Count > 0 ? calEventCalendar.SelectedDates[calEventCalendar.SelectedDates.Count - 1] : ( DateTime? ) null;
        }

        /// <summary>
        /// Shows the warning.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Stores event item occurrence data for use in Lava templates.
        /// </summary>
        [DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Time", "EndDate", "EndTime", "Campus", "Location", "LocationDescription", "Description", "Summary", "OccurrenceNote", "DetailPage" )]
        public class EventOccurrenceSummary : LavaDataObject
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }

            public DateTime DateTime { get; set; }

            public string Name { get; set; }

            public string Date { get; set; }

            public string Time { get; set; }

            public string EndDate { get; set; }

            public string EndTime { get; set; }

            public string Campus { get; set; }

            public string Location { get; set; }

            public string LocationDescription { get; set; }

            public string Summary { get; set; }

            public string Description { get; set; }

            public string OccurrenceNote { get; set; }

            public string DetailPage { get; set; }
        }

        /// <summary>
        /// A block-level viewmodel for event item occurrences dates.
        /// </summary>
        public class EventOccurrenceDate
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }

            public List<Occurrence> ScheduleOccurrences { get; set; }
        }

        #endregion
    }
}

// <copyright>
// Copyright by the Central Christian Church
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

using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Web.Cache;
namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    [DisplayName( "Reservation Lava" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Renders a list of reservations in lava." )]

    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month", true, "Week", order: 1 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 2 )]

    [CustomRadioListField( "Campus Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 3 )]
    [CustomRadioListField( "Ministry Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "MinistryFilterDisplayMode", order: 4 )]
    [BooleanField( "Show Date Range Filter", "Determines whether the date range filters are shown", false, order: 6 )]

    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true, order: 7 )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false, order: 8 )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true, order: 9 )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true, order: 10 )]

    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Themes/Stark/Assets/Lava/CalendarGroupedOccurrence.lava' %}", "", 12 )]

    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of a week.", true, DayOfWeek.Sunday, order: 13 )]

    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 14 )]

    public partial class ReservationLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        protected bool CampusPanelOpen { get; set; }
        protected bool CampusPanelClosed { get; set; }
        protected bool MinistryPanelOpen { get; set; }
        protected bool MinistryPanelClosed { get; set; }

        #endregion

        #region Properties

        private String ViewMode { get; set; }
        private DateTime? FilterStartDate { get; set; }
        private DateTime? FilterEndDate { get; set; }
        private List<DateTime> ReservationDates { get; set; }

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ViewMode = ViewState["ViewMode"] as String;
            FilterStartDate = ViewState["FilterStartDate"] as DateTime?;
            FilterEndDate = ViewState["FilterEndDate"] as DateTime?;
            ReservationDates = ViewState["ReservationDates"] as List<DateTime>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _firstDayOfWeek = GetAttributeValue( "StartofWeekDay" ).ConvertToEnum<DayOfWeek>();

            CampusPanelOpen = GetAttributeValue( "CampusFilterDisplayMode" ) == "3";
            CampusPanelClosed = GetAttributeValue( "CampusFilterDisplayMode" ) == "4";
            MinistryPanelOpen = !String.IsNullOrWhiteSpace( GetAttributeValue( "FilterCategories" ) ) && GetAttributeValue( "MinistryFilterDisplayMode" ) == "3";
            MinistryPanelClosed = !String.IsNullOrWhiteSpace( GetAttributeValue( "FilterCategories" ) ) && GetAttributeValue( "MinistryFilterDisplayMode" ) == "4";

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
            ViewState["FilterStartDate"] = FilterStartDate;
            ViewState["FilterEndDate"] = FilterEndDate;
            ViewState["ReservationDates"] = ReservationDates;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            btnDay.CssClass = "btn btn-default" + ( ViewMode == "Day" ? " active" : "" );
            btnWeek.CssClass = "btn btn-default" + ( ViewMode == "Week" ? " active" : "" );
            btnMonth.CssClass = "btn btn-default" + ( ViewMode == "Month" ? " active" : "" );

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
        /// Handles the SelectionChanged event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void calReservationCalendar_SelectionChanged( object sender, EventArgs e )
        {
            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the DayRender event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DayRenderEventArgs"/> instance containing the event data.</param>
        protected void calReservationCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( ReservationDates != null && ReservationDates.Any( d => d.Date.Equals( day.Date ) ) )
            {
                e.Cell.AddCssClass( "calendar-hasevent" );
            }
        }

        /// <summary>
        /// Handles the VisibleMonthChanged event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MonthChangedEventArgs"/> instance containing the event data.</param>
        protected void calReservationCalendar_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            calReservationCalendar.SelectedDate = e.NewDate;
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
        /// Handles the SelectedIndexChanged event of the cblMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblMinistry_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindData();
        }

        protected void lbDateRangeRefresh_Click( object sender, EventArgs e )
        {
            FilterStartDate = drpDateRange.LowerValue;
            FilterEndDate = drpDateRange.UpperValue;
            BindData();
        }

        protected void btnViewMode_Click( object sender, EventArgs e )
        {
            var btnViewMode = sender as BootstrapButton;
            if ( btnViewMode != null )
            {
                ViewMode = btnViewMode.Text;
                ResetCalendarSelection();
                BindData();
            }
        }

        #endregion

        #region Methods

        private void BindData()
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable();

            // Filter by campus
            List<int> campusIds = cblCampus.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            if ( campusIds.Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.CampusId.HasValue ||    // All
                        campusIds.Contains( r.CampusId.Value ) );
            }

            // Filter by Ministry
            List<int> ministryIds = cblMinistry.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            if ( ministryIds.Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.ReservationMinistryId.HasValue ||    // All
                        ministryIds.Contains( r.ReservationMinistryId.Value ) );
            }

            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = FilterStartDate.HasValue ? FilterStartDate.Value : today;
            var filterEndDateTime = FilterEndDate.HasValue ? FilterEndDate.Value : today.AddMonths( 1 );
            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime );

            // Bind to Grid
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                Status = r.Status,
                Locations = r.ReservationLocations.Select( rl => rl.Location.Name ).ToList().AsDelimited( ", " ),
                Resources = r.ReservationResources.Select( rr => rr.Resource.Name + " (" + rr.Quantity + ")" ).ToList().AsDelimited( ", " ),
                CalendarDate = r.EventStartDateTime.ToLongDateString(),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventDateTimeDescription = r.EventTimeDescription,
                ReservationDateTimeDescription = r.ReservationTimeDescription
            } )
            .OrderBy( r => r.EventStartDateTime )
            .GroupBy( r => r.EventStartDateTime.Date )
            .Select( r => r.ToList() )
            .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "TimeFrame", ViewMode );
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );
            mergeFields.Add( "ReservationSummaries", reservationSummaries );
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
        /// Loads the drop downs.
        /// </summary>
        private bool SetFilterControls()
        {
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

            // Setup small calendar Filter
            calReservationCalendar.FirstDayOfWeek = _firstDayOfWeek.ConvertToInt().ToString().ConvertToEnum<FirstDayOfWeek>();
            calReservationCalendar.SelectedDates.Clear();
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );

            // Setup Campus Filter
            rcwCampus.Visible = GetAttributeValue( "CampusFilterDisplayMode" ).AsInteger() > 1;
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
            {
                var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Read( "Rock.Model.Campus" ) ) as Campus;
                if ( contextCampus != null )
                {
                    cblCampus.SetValue( contextCampus.Id );
                }
            }

            // Setup Ministry Filter
            rcwMinistry.Visible = GetAttributeValue( "MinistryFilterDisplayMode" ).AsInteger() > 1;
            cblMinistry.DataSource = ReservationMinistryCache.All();
            cblMinistry.DataBind();

            // Date Range Filter
            drpDateRange.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            lbDateRangeRefresh.Visible = drpDateRange.Visible;
            drpDateRange.LowerValue = FilterStartDate;
            drpDateRange.UpperValue = FilterEndDate;

            // Get the View Modes, and only show them if more than one is visible
            var viewsVisible = new List<bool> {
                GetAttributeValue( "ShowDayView" ).AsBoolean(),
                GetAttributeValue( "ShowWeekView" ).AsBoolean(),
                GetAttributeValue( "ShowMonthView" ).AsBoolean()
            };
            var howManyVisible = viewsVisible.Where( v => v ).Count();
            btnDay.Visible = howManyVisible > 1 && viewsVisible[0];
            btnWeek.Visible = howManyVisible > 1 && viewsVisible[1];
            btnMonth.Visible = howManyVisible > 1 && viewsVisible[2];

            // Set filter visibility
            bool showFilter = ( pnlCalendar.Visible || rcwCampus.Visible || rcwMinistry.Visible || drpDateRange.Visible );
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
            var selectedDate = calReservationCalendar.SelectedDate;
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

            // Reset the selection
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );
        }

        private void SetCalendarFilterDates()
        {
            FilterStartDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[0] : (DateTime?)null;
            FilterEndDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[calReservationCalendar.SelectedDates.Count - 1] : (DateTime?)null;
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
    }
}
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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Roster" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to view people currently checked into a classroom, mark a person as 'present' in the classroom, check them out, Etc." )]

    #region Block Attributes

    [LinkedPage(
        "Person Page",
        Key = AttributeKey.PersonPage,
        Description = "The page used to display a selected person's details.",
        IsRequired = true,
        Order = 1 )]

    [BooleanField(
        "Show All Areas",
        Key = AttributeKey.ShowAllAreas,
        Description = "If enabled, all Check-in Areas will be shown. This setting will be ignored if a specific area is specified in the URL.",
        DefaultBooleanValue = true,
        Order = 2 )]

    [LinkedPage(
        "Area Select Page",
        Key = AttributeKey.AreaSelectPage,
        Description = "If Show All Areas is not enabled, the page to redirect user to if a Check-in Area has not been configured or selected.",
        IsRequired = false,
        Order = 3 )]

    [GroupTypeField(
        "Check-in Area",
        Key = AttributeKey.CheckInAreaGuid,
        Description = "If Show All Areas is not enabled, the Check-in Area for the rooms to be managed by this Block.",
        IsRequired = false,
        GroupTypePurposeValueGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE,
        Order = 4 )]

    [BooleanField(
        "Enable Group Column",
        Key = AttributeKey.EnableGroupColumn,
        Description = "When enabled, a column showing the group(s) the person checked into will be shown.",
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField(
        "Enable Checkout All",
        Key = AttributeKey.EnableCheckoutAll,
        Description = "When enabled, a button will be shown to allow checking out all individuals.",
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField(
        "Enable Staying Button",
        Key = AttributeKey.EnableStayingButton,
        Description = "When enabled, a 'Staying' button will be shown to mark the person as checked into the selected service( shown in modal )",
        DefaultBooleanValue = false,
        Order = 7 )]

    [BooleanField(
        "Enable Not Present Button",
        Key = AttributeKey.EnableNotPresentButton,
        Description = "When enabled, a 'Not Present' button will be shown to mark the person as not being present in the room.",
        DefaultBooleanValue = false,
        Order = 8 )]

    [BooleanField(
        "Enable Mark Present",
        Key = AttributeKey.EnableMarkPresentButton,
        Description = "When enabled, a button will be shown in 'Checked-out' mode allowing the person to be marked present.",
        DefaultBooleanValue = false,
        Order = 9 )]

    [AttributeCategoryField(
        "Check-in Roster Alert Icon Category",
        Description = "The Person Attribute category to get the Alert Icon attributes from",
        Key = AttributeKey.CheckInRosterAlertIconCategory,
        DefaultValue = Rock.SystemGuid.Category.PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON,
        EntityType = typeof( Rock.Model.Person ),
        AllowMultiple = false,
        Order = 10
        )]

    #endregion Block Attributes

    public partial class Roster : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for block attributes.
        /// </summary>
        private class AttributeKey
        {
            public const string PersonPage = "PersonPage";
            public const string ShowAllAreas = "ShowAllAreas";
            public const string AreaSelectPage = "AreaSelectPage";

            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// For example "Weekly Service Check-in".
            /// </summary>
            public const string CheckInAreaGuid = "CheckInAreaGuid";

            public const string EnableGroupColumn = "EnableGroupColumn";
            public const string EnableCheckoutAll = "EnableCheckoutAll";
            public const string EnableStayingButton = "EnableStayingButton";
            public const string EnableNotPresentButton = "EnableNotPresentButton";
            public const string EnableMarkPresentButton = "EnableMarkPresentButton";

            public const string CheckInRosterAlertIconCategory = "CheckInRosterAlertIconCategory";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private class PageParameterKey
        {
            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// For example "Weekly Service Check-in".
            /// </summary>
            public const string Area = "Area";

            public const string LocationId = "LocationId";
            public const string Person = "Person";
        }

        #endregion Page Parameter Keys

        #region ViewState Keys

        /// <summary>
        /// Keys to use for ViewState.
        /// </summary>
        private class ViewStateKey
        {
            public const string CurrentCampusId = "CurrentCampusId";
            public const string CurrentLocationId = "CurrentLocationId";
            public const string CurrentStatusFilter = "CurrentStatusFilter";
        }

        #endregion ViewState Keys

        #region Properties

        /// <summary>
        /// The current campus identifier.
        /// </summary>
        public int CurrentCampusId
        {
            get
            {
                return ViewState[ViewStateKey.CurrentCampusId] as int? ?? 0;
            }

            set
            {
                ViewState[ViewStateKey.CurrentCampusId] = value;
            }
        }

        /// <summary>
        /// The current location identifier.
        /// </summary>
        public int CurrentLocationId
        {
            get
            {
                return ViewState[ViewStateKey.CurrentLocationId] as int? ?? 0;
            }

            set
            {
                ViewState[ViewStateKey.CurrentLocationId] = value;
            }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += Block_BlockUpdated;
            gAttendees.GridRebind += gAttendees_GridRebind;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gAttendees_GridRebind( object sender, GridRebindEventArgs e )
        {
            BuildRoster();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                bgStatus.Items.Clear();
                bgStatus.Items.Add( new ListItem( RosterStatusFilter.All.GetDescription(), RosterStatusFilter.All.ConvertToInt().ToString() ) );
                bgStatus.Items.Add( new ListItem( RosterStatusFilter.CheckedIn.GetDescription(), RosterStatusFilter.CheckedIn.ConvertToInt().ToString() ) );
                bgStatus.Items.Add( new ListItem( RosterStatusFilter.Present.GetDescription(), RosterStatusFilter.Present.ConvertToInt().ToString() ) );
                bgStatus.Items.Add( new ListItem( RosterStatusFilter.CheckedOut.GetDescription(), RosterStatusFilter.CheckedOut.ConvertToInt().ToString() ) );

                BuildRoster();
            }
        }

        #endregion Base Control Methods

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPage();
        }

        /// <summary>
        /// Handles the SelectLocation event of the lpLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lpLocation_SelectLocation( object sender, EventArgs e )
        {
            var locationId = lpLocation.NamedLocation?.Id;
            if ( locationId != null )
            {
                CheckinManagerHelper.SetSelectedLocation( this, lpLocation, locationId, CurrentCampusId );
            }
            else
            {
                CheckinManagerHelper.SetSelectedLocation( this, lpLocation, 0, CurrentCampusId );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            RosterStatusFilter statusFilter = GetStatusFilterValueFromControl();
            CheckinManagerHelper.SaveRosterConfigurationToCookie( statusFilter );
            BuildRoster();
        }

        #endregion Control Events

        #region Roster Grid Related

        /// <summary>
        /// Builds the roster for the selected campus and location.
        /// </summary>
        private void BuildRoster()
        {
            ResetControlVisibility();

            // If ShowAllAreas is false, the CheckinAreaFilter is required.
            if ( this.GetAttributeValue( AttributeKey.ShowAllAreas ).AsBoolean() == false )
            {
                var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );
                if ( checkinAreaFilter == null )
                {
                    if ( NavigateToLinkedPage( AttributeKey.AreaSelectPage ) )
                    {
                        // We are navigating to get the Area Filter which will get the Area cookie.
                        return;
                    }
                    else
                    {
                        ShowWarningMessage( "The 'Area Select Page' Block Attribute must be defined.", true );
                        return;
                    }
                }
            }

            CampusCache campus = GetCampusFromContext();
            if ( campus == null )
            {
                ShowWarningMessage( "Please select a Campus.", true );
                return;
            }

            CurrentCampusId = campus.Id;

            int? locationId = CheckinManagerHelper.GetSelectedLocation( this, campus, lpLocation );
            if ( !locationId.HasValue )
            {
                ShowWarningMessage( "Please select a location", false );
                return;
            }

            CheckinManagerHelper.SetSelectedLocation( this, lpLocation, locationId, CurrentCampusId );
            if ( this.Response.IsRequestBeingRedirected )
            {
                return;
            }

            InitializeSubPageNav( locationId.Value );

            // Check the ButtonGroup for the StatusFilter value.
            RosterStatusFilter statusFilter = GetStatusFilterValueFromControl();
            if ( statusFilter == RosterStatusFilter.Unknown )
            {
                // If not defined on the ButtonGroup, check for a Block user preference.
                statusFilter = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().RosterStatusFilter;

                if ( statusFilter == RosterStatusFilter.Unknown )
                {
                    // If we still don't know the value, set it to 'All'.
                    statusFilter = RosterStatusFilter.All;
                }

                SetStatusFilterControl( statusFilter );
            }

            CurrentLocationId = locationId.Value;

            BindGrid();
        }

        private RosterStatusFilter _dataBoundRosterStatusFilter;

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            RosterAttendee attendee = e.Row.DataItem as RosterAttendee;

            // Desktop only.
            var lPhoto = e.Row.FindControl( "lPhoto" ) as Literal;
            lPhoto.Text = attendee.GetPersonPhotoImageHtmlTag();

            // Mobile only.
            var lMobileIcon = e.Row.FindControl( "lMobileIcon" ) as Literal;
            lMobileIcon.Text = attendee.GetStatusIconHtmlTag( true );

            // Shared between desktop and mobile.
            var lName = e.Row.FindControl( "lName" ) as Literal;
            lName.Text = attendee.GetAttendeeNameHtml();

            // Desktop only.
            var lBadges = e.Row.FindControl( "lBadges" ) as Literal;
            lBadges.Text = string.Format( "<div>{0}</div>", attendee.GetBadgesHtml( _attributesForAlertIcons ) );

            var lGroupNameAndPath = e.Row.FindControl( "lGroupNameAndPath" ) as Literal;
            if ( lGroupNameAndPath != null && lGroupNameAndPath.Visible )
            {
                lGroupNameAndPath.Text = attendee.GetGroupNameAndPathHtml();
            }

            // Mobile only.
            var lMobileTagAndSchedules = e.Row.FindControl( "lMobileTagAndSchedules" ) as Literal;
            lMobileTagAndSchedules.Text = attendee.GetMobileTagAndSchedulesHtml();

            // Desktop only.
            // Show how it has been since they have checked in (and not yet marked present)
            var lElapsedCheckInTime = e.Row.FindControl( "lElapsedCheckInTime" ) as Literal;
            var lElapsedCheckedOutTime = e.Row.FindControl( "lElapsedCheckedOutTime" ) as Literal;
            var lElapsedPresentTime = e.Row.FindControl( "lElapsedPresentTime" ) as Literal;

            lElapsedCheckInTime.Text = string.Format(
                "<span title='{0}'>{1}</span>",
                attendee.CheckInTime.ToShortTimeString(),
                RockFilters.HumanizeTimeSpan( attendee.CheckInTime, RockDateTime.Now, unit: "Second" ) );

            // Show how it has been since they were marked present
            if ( attendee.PresentDateTime.HasValue )
            {
                lElapsedPresentTime.Text = string.Format(
                    "<span title='{0}'>{1}</span>",
                    attendee.PresentDateTime?.ToShortTimeString(),
                    RockFilters.HumanizeTimeSpan( attendee.PresentDateTime.Value, RockDateTime.Now, unit: "Second" ) );
            }

            if ( attendee.CheckOutTime.HasValue )
            {
                var timeSinceCheckout = DateTime.Now - attendee.CheckOutTime.Value;
                // Show how it has been since they have checked out

                lElapsedCheckedOutTime.Text = string.Format(
                    "<span title='{0}'>{1}</span>",
                    attendee.CheckOutTime.Value.ToShortTimeString(),
                    RockFilters.HumanizeTimeSpan( attendee.CheckOutTime.Value, RockDateTime.Now, unit: "Second" ) );
            }

            lElapsedCheckInTime.Visible = _dataBoundRosterStatusFilter == RosterStatusFilter.CheckedIn;
            lElapsedPresentTime.Visible = _dataBoundRosterStatusFilter == RosterStatusFilter.Present;
            lElapsedCheckedOutTime.Visible = _dataBoundRosterStatusFilter == RosterStatusFilter.CheckedOut;

            // Desktop only.
            var lStatusTag = e.Row.FindControl( "lStatusTag" ) as Literal;
            lStatusTag.Text = attendee.GetStatusIconHtmlTag( false );
        }

        /// <summary>
        /// Handles the DataBound event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnCancel_DataBound( object sender, RowEventArgs e )
        {
            var rosterAttendee = e.Row.DataItem as RosterAttendee;
            var btnCancel = sender as LinkButton;

            // Cancel button will be visible in two cases
            // 1) They are on the CheckedIn tab (which would only show attendees that checked-in (but not yet present) in "Enable Presence" rooms
            // 2) They are on the Present Tab in a room that doesn't have Presence Enable
            btnCancel.Visible = ( _dataBoundRosterStatusFilter == RosterStatusFilter.CheckedIn ) || ( rosterAttendee.RoomHasEnablePresence == false && _dataBoundRosterStatusFilter == RosterStatusFilter.Present );
        }

        /// <summary>
        /// Handles the DataBound event of the btnCheckOut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnCheckOut_DataBound( object sender, RowEventArgs e )
        {
            var rosterAttendee = e.Row.DataItem as RosterAttendee;
            var btnCheckoutLinkButton = sender as LinkButton;
            btnCheckoutLinkButton.Visible = rosterAttendee.RoomHasAllowCheckout;
        }

        /// <summary>
        /// Handles the DataBound event of the btnPresent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnPresent_DataBound( object sender, RowEventArgs e )
        {
            var rosterAttendee = e.Row.DataItem as RosterAttendee;
            var btnPresentLinkButton = e.Row.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault( a => a.ID == "btnPresent" );
            btnPresentLinkButton.Visible = rosterAttendee.RoomHasEnablePresence;
        }

        /// <summary>
        /// Handles the DataBound event of the btnNotPresent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnNotPresent_DataBound( object sender, RowEventArgs e )
        {
            var rosterAttendee = e.Row.DataItem as RosterAttendee;
            var btnNotPresentLinkButton = e.Row.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault( a => a.ID == "btnNotPresent" );
            if ( btnNotPresentLinkButton.Visible && !rosterAttendee.RoomHasEnablePresence )
            {
                btnNotPresentLinkButton.Visible = false;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            // the attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more)
            string personGuid = e.RowKeyValues[0].ToString();
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.Person, personGuid }
            };

            // If an Area Guid was passed to the Page, pass it along so it can be passed back.
            string areaGuid = PageParameter( PageParameterKey.Area );
            if ( areaGuid.IsNotNullOrWhiteSpace() )
            {
                queryParams.Add( PageParameterKey.Area, areaGuid );
            }

            if ( !NavigateToLinkedPage( AttributeKey.PersonPage, queryParams ) )
            {
                ShowWarningMessage( "The 'Person Page' Block Attribute must be defined.", true );
            }
        }

        private List<AttributeCache> _attributesForAlertIcons = new List<AttributeCache>();

        /// <summary>
        /// Shows the attendees.
        /// </summary>
        private void BindGrid()
        {
            IList<RosterAttendee> attendees = null;

            // Note, don't wrap this in a Using, we don't want to destroy it just in case there is some lazy loading that will happen (there shouldn't be, but just in case)
            var rockContext = new RockContext();

            attendees = GetAttendees( rockContext );

            var currentStatusFilter = GetStatusFilterValueFromControl();

            ToggleColumnVisibility( attendees, currentStatusFilter );

            var lGroupNameAndPathField = gAttendees.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lGroupNameAndPath" );
            if ( lGroupNameAndPathField != null )
            {
                lGroupNameAndPathField.Visible = this.GetAttributeValue( AttributeKey.EnableGroupColumn ).AsBoolean();
            }

            _dataBoundRosterStatusFilter = currentStatusFilter;

            var checkInRosterAlertIconCategoryGuid = this.GetAttributeValue( AttributeKey.CheckInRosterAlertIconCategory )?.AsGuid();
            if ( checkInRosterAlertIconCategoryGuid.HasValue )
            {
                var categoryId = CategoryCache.GetId( checkInRosterAlertIconCategoryGuid.Value ) ?? 0;
                _attributesForAlertIcons = new AttributeService( rockContext ).GetByCategoryId( categoryId ).ToAttributeCacheList();
            }
            else
            {
                _attributesForAlertIcons = new List<AttributeCache>();
            }


            var scheduleIds = attendees.Select( a => a.ScheduleId ).Distinct().ToList();

            var schedulePositions = new ScheduleService( rockContext ).GetByIds( scheduleIds ).ToList().OrderByOrderAndNextScheduledDateTime().Select( a => a.Id ).ToList();
            List<RosterAttendee> attendeesSorted;
            gAttendees.SortProperty = gAttendees.SortProperty ?? new SortProperty { Property = "NickName,LastName,PersonGuid", Direction = SortDirection.Ascending };
            if ( gAttendees.SortProperty.Property == "ServiceTimesScheduleOrder" )
            {
                bool descending = gAttendees.SortProperty.Direction == SortDirection.Descending;
                attendeesSorted = attendees.OrderBy( a =>
                    {
                        var positionIndex = schedulePositions.IndexOf( a.ScheduleId );
                        if ( descending )
                        {
                            positionIndex = -positionIndex;
                        }

                        return positionIndex;
                    } )
                    .ThenBy( a => a.NickName )
                    .ThenBy( a => a.LastName )
                    .ThenBy( a => a.PersonGuid )
                    .ToList();
            }
            else
            {
                attendeesSorted = attendees.AsQueryable().Sort( gAttendees.SortProperty ).ToList();
            }

            gAttendees.DataSource = attendeesSorted;
            gAttendees.DataBind();
        }

        /// <summary>
        /// Gets the attendees.
        /// </summary>
        private IList<RosterAttendee> GetAttendees( RockContext rockContext )
        {
            var startDateTime = RockDateTime.Today;

            CampusCache campusCache = CampusCache.Get( CurrentCampusId );
            DateTime currentDateTime;
            if ( campusCache != null )
            {
                currentDateTime = campusCache.CurrentDateTime;
            }
            else
            {
                currentDateTime = RockDateTime.Now;
            }

            // Get all Attendance records for the current day and location
            var attendanceQuery = new AttendanceService( rockContext ).Queryable().Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.ScheduleId.HasValue
                && a.Occurrence.LocationId.HasValue
                && a.Occurrence.LocationId == CurrentLocationId
                && a.Occurrence.ScheduleId.HasValue );

            var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );
            List<int> groupTypeIds;

            if ( checkinAreaFilter != null )
            {
                // If there is a checkin area filter, limit to groups within the selected check-in area.
                groupTypeIds = new GroupTypeService( new RockContext() ).GetCheckinAreaDescendants( checkinAreaFilter.Id ).Select( a => a.Id ).ToList();
            }
            else
            {
                groupTypeIds = new GroupTypeService( new RockContext() ).GetAllCheckinAreaPaths().Select( a => a.GroupTypeId ).ToList();
            }

            attendanceQuery = attendanceQuery.Where( a => groupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );

            // Limit to Groups that are configured for the selected location.
            var groupIdsForLocation = new GroupLocationService( rockContext ).Queryable().Where( a => a.LocationId == CurrentLocationId ).Select( a => a.GroupId ).Distinct().ToList();
            attendanceQuery = attendanceQuery.Where( a => groupIdsForLocation.Contains( a.Occurrence.GroupId.Value ) );
            List<RosterAttendeeAttendance> attendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

            var unfilteredAttendanceCheckinAreas = attendanceList.Select( a => a.GroupTypeId ).ToList().Select( a => GroupTypeCache.Get( a ) ).ToArray();

            RemoveUnneededStatusFilters( unfilteredAttendanceCheckinAreas );
            var currentStatusFilter = GetStatusFilterValueFromControl();

            attendanceList = CheckinManagerHelper.FilterByActiveCheckins( currentDateTime, attendanceList );
            attendanceList = attendanceList.Where( a => a.Person != null ).ToList();
            UpdateStatusFilterTabs( attendanceList );

            List<RosterAttendeeAttendance> attendanceListForCurrentStatusFilter;

            if ( !HasPresenceEnabled( unfilteredAttendanceCheckinAreas ) && currentStatusFilter == RosterStatusFilter.Present )
            {
                // Edge case. If there are attendance records with 'PresentDateTime' null (due to pre-v12.3 checkin, or a change in configuration from Enable Presence to Disable Presence), also include CheckedIn if we are filter only for Present.
                attendanceListForCurrentStatusFilter = attendanceList.Where( a => RosterAttendee.AttendanceMeetsRosterStatusFilter( a, RosterStatusFilter.Present ) || RosterAttendee.AttendanceMeetsRosterStatusFilter( a, RosterStatusFilter.CheckedIn ) ).ToList();
            }
            else
            {
                attendanceListForCurrentStatusFilter = attendanceList.Where( a => RosterAttendee.AttendanceMeetsRosterStatusFilter( a, currentStatusFilter ) ).ToList();
            }

            var attendeesForCurrentStatusFilter = RosterAttendee.GetFromAttendanceList( attendanceListForCurrentStatusFilter, checkinAreaFilter );

            return attendeesForCurrentStatusFilter;
        }

        /// <summary>
        /// Set the Text of the ListItems in the StatusFilter Button Group
        /// </summary>
        /// <param name="attendanceQuery">The attendance query.</param>
        /// <param name="currentDateTime">The current date time.</param>
        private void UpdateStatusFilterTabs( IList<RosterAttendeeAttendance> attendances )
        {
            var checkedInCount = attendances.Where( x => RosterAttendee.AttendanceMeetsRosterStatusFilter( x, RosterStatusFilter.CheckedIn ) ).DistinctBy( a => a.PersonId ).Count();
            var presentCount = attendances.Where( x => RosterAttendee.AttendanceMeetsRosterStatusFilter( x, RosterStatusFilter.Present ) ).DistinctBy( a => a.PersonId ).Count();
            var checkedOutCount = attendances.Where( x => RosterAttendee.AttendanceMeetsRosterStatusFilter( x, RosterStatusFilter.CheckedOut ) ).DistinctBy( a => a.PersonId ).Count();
            var allCount = attendances.DistinctBy( a => a.PersonId ).Count();

            UpdateStatusFilterTabText( RosterStatusFilter.All, allCount );
            UpdateStatusFilterTabText( RosterStatusFilter.CheckedIn, checkedInCount );
            UpdateStatusFilterTabText( RosterStatusFilter.Present, presentCount );
            UpdateStatusFilterTabText( RosterStatusFilter.CheckedOut, checkedOutCount );
        }

        /// <summary>
        /// Updates the status filter tab text.
        /// </summary>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        /// <param name="count">The count.</param>
        private void UpdateStatusFilterTabText( RosterStatusFilter rosterStatusFilter, int count )
        {
            var listItem = bgStatus.Items.FindByValue( rosterStatusFilter.ConvertToInt().ToString() );
            if ( listItem != null )
            {
                if ( listItem.Selected )
                {
                    listItem.Text = string.Format( "{0} <span class='badge badge-info'>{1}</span>", rosterStatusFilter.GetDescription(), count );
                }
                else
                {
                    listItem.Text = string.Format( "{0} <span class='badge badge-'>{1}</span>", rosterStatusFilter.GetDescription(), count );
                }
            }
        }

        #endregion Roster Grid Related

        #region Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, RowEventArgs e )
        {
            // The attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more).
            var attendanceIds = GetRowAttendanceIds( e );
            if ( !attendanceIds.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                foreach ( var attendance in attendanceService
                    .Queryable()
                    .Where( a => attendanceIds.Contains( a.Id ) ) )
                {
                    attendanceService.Delete( attendance );
                }

                rockContext.SaveChanges();

                // Reset the cache for this Location so the kiosk will show the correct counts.
                Rock.CheckIn.KioskLocationAttendance.Remove( CurrentLocationId );
            }

            BindGrid();
        }

        /// <summary>
        /// Gets the row attendance ids.
        /// </summary>
        /// <param name="rowEventArgs">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private int[] GetRowAttendanceIds( RowEventArgs rowEventArgs )
        {
            // The attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more).
            var attendanceIds = rowEventArgs.RowKeyValues[1] as int[];
            return attendanceIds;
        }

        /// <summary>
        /// Handles the Click event of the btnPresent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnPresent_Click( object sender, RowEventArgs e )
        {
            var attendanceIds = GetRowAttendanceIds( e );
            if ( !attendanceIds.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var now = RockDateTime.Now;
                var attendanceService = new AttendanceService( rockContext );
                foreach ( var attendee in attendanceService
                    .Queryable()
                    .Where( a => attendanceIds.Contains( a.Id ) ) )
                {
                    attendee.PresentDateTime = now;
                    attendee.PresentByPersonAliasId = CurrentPersonAliasId;

                    // If they were Checked-out, clear the EndDateTime since they have been changed to Present.
                    if ( attendee.EndDateTime.HasValue )
                    {
                        attendee.EndDateTime = null;
                    }
                }

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCheckOut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnCheckOut_Click( object sender, RowEventArgs e )
        {
            // The attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more).
            var attendanceIds = GetRowAttendanceIds( e ).ToList();
            if ( !attendanceIds.Any() )
            {
                return;
            }

            var rockContext = new RockContext();
            var attendanceList = new AttendanceService( rockContext )
                .GetByIds( attendanceIds )
                .Include( a => a.Occurrence )
                .Include( a => a.PersonAlias.Person )
                .ToList();

            var scheduleIds = attendanceList.Select( a => a.Occurrence?.ScheduleId ?? 0 ).Distinct().ToList();
            if ( scheduleIds.Count > 1 )
            {
                hfConfirmCheckoutAttendeeAttendanceIds.Value = attendanceIds.AsDelimited( "," );

                var personName = attendanceList.FirstOrDefault()?.PersonAlias?.Person.FullName;
                lConfirmCheckoutAttendee.Text = $"Which schedules would you like to check {personName} out of?";

                // if there is more than one schedule, prompt for which one to check out of
                var sortedScheduleList = new ScheduleService( rockContext ).GetByIds( scheduleIds ).AsNoTracking()
                    .ToList().OrderByOrderAndNextScheduledDateTime();

                cblSchedulesCheckoutAttendee.Items.Clear();

                foreach ( var schedule in sortedScheduleList )
                {
                    string listItemText;
                    if ( schedule.Name.IsNotNullOrWhiteSpace() )
                    {
                        listItemText = schedule.Name;
                    }
                    else
                    {
                        listItemText = schedule.FriendlyScheduleText;
                    }

                    cblSchedulesCheckoutAttendee.Items.Add( new ListItem( listItemText, schedule.Id.ToString() ) );
                }

                mdConfirmCheckoutAttendee.Show();
            }
            else
            {
                SetAttendancesAsCheckedOut( attendanceList, rockContext );

                BindGrid();
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfirmCheckoutAttendee control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmCheckoutAttendee_SaveClick( object sender, EventArgs e )
        {
            mdConfirmCheckoutAttendee.Hide();
            var rockContext = new RockContext();

            var attendanceIds = hfConfirmCheckoutAttendeeAttendanceIds.Value?.SplitDelimitedValues().AsIntegerList() ?? new List<int>();

            var selectedScheduleIds = cblSchedulesCheckoutAttendee.SelectedValuesAsInt;

            var attendancesToCheckout = new AttendanceService( rockContext )
                .GetByIds( attendanceIds )
                .Where( x => selectedScheduleIds.Contains( x.Occurrence.ScheduleId.Value ) ).ToList();

            SetAttendancesAsCheckedOut( attendancesToCheckout, rockContext );

            BindGrid();
        }

        /// <summary>
        /// Sets the attendances as checked out.
        /// </summary>
        /// <param name="attendanceIds">The attendance ids.</param>
        private void SetAttendancesAsCheckedOut( List<Attendance> attendanceList, RockContext rockContext )
        {
            var now = RockDateTime.Now;

            foreach ( var attendee in attendanceList )
            {
                attendee.EndDateTime = now;
                attendee.CheckedOutByPersonAliasId = CurrentPersonAliasId;
            }

            rockContext.SaveChanges();

            // Reset the cache for this Location so the kiosk will show the correct counts.
            Rock.CheckIn.KioskLocationAttendance.Remove( CurrentLocationId );
        }

        /// <summary>
        /// Handles the Click event of the btnStaying control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnStaying_Click( object sender, RowEventArgs e )
        {
            var attendanceIds = GetRowAttendanceIds( e ).ToList();

            rblScheduleStayingFor.Items.Clear();

            var rockContext = new RockContext();
            var attendanceQuery = new AttendanceService( rockContext ).Queryable();

            var attendanceInfo = new AttendanceService( rockContext ).GetByIds( attendanceIds )
                .Where( a => a.Occurrence.GroupId.HasValue && a.Occurrence.ScheduleId.HasValue && a.Occurrence.LocationId.HasValue )
                .Select( s => new
                {
                    GroupId = s.Occurrence.GroupId.Value,
                    LocationId = s.Occurrence.LocationId.Value,
                    ScheduleId = s.Occurrence.ScheduleId.Value,
                    s.Id,
                    s.StartDateTime
                } )
                .OrderByDescending( a => a.StartDateTime )
                .FirstOrDefault();

            if ( attendanceInfo == null )
            {
                return;
            }

            hfConfirmStayingAttendanceId.Value = attendanceInfo.Id.ToString();

            // Limit Schedules to ones that available to this attendance's GroupId and LocationId.
            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocationScheduleQuery = groupLocationService.Queryable()
                .Where( a => a.GroupId == attendanceInfo.GroupId && a.LocationId == attendanceInfo.LocationId )
                .SelectMany( s => s.Schedules ).Distinct();

            // Also limit to active check-in schedules, and exclude the current schedule.
            var scheduleQry = groupLocationScheduleQuery
                    .Where( a =>
                        a.Id != attendanceInfo.ScheduleId &&
                        a.IsActive
                        && a.CheckInStartOffsetMinutes != null
                        && a.Name != null
                        && a.Name != string.Empty )
                    .ToList();

            var scheduleList = scheduleQry.ToList();

            // Limit to schedules for the current day.
            scheduleList = scheduleList
                .Where( a => a.GetNextCheckInStartTime( RockDateTime.Today ) < RockDateTime.Today.AddDays( 1 ) )
                .ToList();

            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();
            rblScheduleStayingFor.Items.Clear();

            foreach ( var schedule in sortedScheduleList )
            {
                rblScheduleStayingFor.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }

            nbConfirmStayingWarning.Visible = false;
            lConfirmStayingPromptText.Visible = true;
            rblScheduleStayingFor.Visible = true;
            mdConfirmStaying.SaveButtonText = "Check In";

            if ( !sortedScheduleList.Any() )
            {
                nbConfirmStayingWarning.Text = "No other schedules available for this group and location.";
                nbConfirmStayingWarning.Visible = true;
                lConfirmStayingPromptText.Visible = false;

                // Hide the save button and schedule options.
                mdConfirmStaying.SaveButtonText = string.Empty;

                rblScheduleStayingFor.Visible = false;
            }
            else if ( sortedScheduleList.Count == 1 )
            {
                // Only one schedule to pick from so hide the options and set the wording on the prompt.
                var singleSchedule = sortedScheduleList[0];

                lConfirmStayingPromptText.Text = string.Format( "Would you like this person to stay for {0}?", singleSchedule.Name );
                rblScheduleStayingFor.SetValue( singleSchedule.Id );
                rblScheduleStayingFor.Visible = false;
            }
            else
            {
                lConfirmStayingPromptText.Text = "Which schedule would you like this person to stay for:";
                rblScheduleStayingFor.Visible = true;
            }

            mdConfirmStaying.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfirmStaying control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmStaying_SaveClick( object sender, EventArgs e )
        {
            var selectedAttendanceId = hfConfirmStayingAttendanceId.Value.AsInteger();
            mdConfirmStaying.Hide();

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var selectedAttendance = attendanceService.Get( selectedAttendanceId );
            if ( selectedAttendance == null )
            {
                // Just in case...
                return;
            }

            var stayingForScheduleId = rblScheduleStayingFor.SelectedValueAsId();
            var selectedOccurrence = selectedAttendance.Occurrence;
            if ( selectedAttendance.Occurrence == null || stayingForScheduleId == null )
            {
                // Just in case...
                return;
            }

            var occurrenceService = new AttendanceOccurrenceService( rockContext );

            var stayingOccurrence = occurrenceService.GetOrAdd( selectedOccurrence.OccurrenceDate, selectedOccurrence.GroupId, selectedOccurrence.LocationId, stayingForScheduleId.Value );
            if ( stayingOccurrence.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            // Create a new attendance based on the values in the selected attendance, but change ScheduleId to the schedule they are staying for.
            var stayingAttendance = selectedAttendance.Clone( false );

            // Reset fields specific to the new attendance.
            stayingAttendance.Id = 0;
            stayingAttendance.Guid = Guid.NewGuid();
            stayingOccurrence.CreatedDateTime = null;
            stayingOccurrence.ModifiedDateTime = null;
            stayingAttendance.OccurrenceId = stayingOccurrence.Id;
            stayingAttendance.Occurrence = stayingOccurrence;
            stayingOccurrence.CreatedByPersonAliasId = CurrentPersonAliasId;

            // If the selected attendance was their first time, the 2nd one wouldn't be, so mark IsFirstTime to false.
            if ( selectedAttendance.IsFirstTime == true )
            {
                stayingAttendance.IsFirstTime = false;
            }

            /* 2020-12-18 MDP 
                Keep StartDateTime the same as the original StartDateTime, since that is when they checked into the room.
                see https://app.asana.com/0/0/1199643530714803/f
            */

            stayingAttendance.StartDateTime = selectedAttendance.StartDateTime;

            attendanceService.Add( stayingAttendance );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCheckoutAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCheckoutAll_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                // Populate the schedules for the 'Checkout All' to only include the schedules that are currently shown in the roster grid.
                var displayedScheduleIds = GetAttendees( rockContext ).SelectMany( a => a.ScheduleIds ).Distinct().ToList();
                var sortedScheduleList = new ScheduleService( rockContext ).GetByIds( displayedScheduleIds ).AsNoTracking()
                    .ToList().OrderByOrderAndNextScheduledDateTime();

                cblSchedulesCheckoutAll.Items.Clear();

                foreach ( var schedule in sortedScheduleList )
                {
                    string listItemText;
                    if ( schedule.Name.IsNotNullOrWhiteSpace() )
                    {
                        listItemText = schedule.Name;
                    }
                    else
                    {
                        listItemText = schedule.FriendlyScheduleText;
                    }

                    cblSchedulesCheckoutAll.Items.Add( new ListItem( listItemText, schedule.Id.ToString() ) );
                }

                if ( sortedScheduleList.Count == 1 )
                {
                    var singleSchedule = sortedScheduleList[0];

                    // Only one schedule to pick from so hide the options and set the wording on the prompt.
                    lConfirmCheckoutAll.Text = string.Format( "Would you like to checkout all for {0}?", singleSchedule.Name );
                    cblSchedulesCheckoutAll.SetValue( singleSchedule.Id );
                    cblSchedulesCheckoutAll.Visible = false;
                }
                else
                {
                    lConfirmCheckoutAll.Text = "Which schedules would you like to check out all for:";
                    cblSchedulesCheckoutAll.Visible = true;
                }
            }

            mdConfirmCheckoutAll.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfirmCheckoutAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmCheckoutAll_SaveClick( object sender, EventArgs e )
        {
            mdConfirmCheckoutAll.Hide();
            var rockContext = new RockContext();

            var displayedAttendees = GetAttendees( rockContext );

            // Only checkout attendees that are in a room that allows checkout.
            var displayedAttendanceIdsWithAllowCheckout = displayedAttendees.Where( a => a.RoomHasAllowCheckout ).SelectMany( a => a.AttendanceIds ).ToList();

            var selectedScheduleIds = cblSchedulesCheckoutAll.SelectedValuesAsInt;

            var attendancesToCheckout = new AttendanceService( rockContext )
                .GetByIds( displayedAttendanceIdsWithAllowCheckout )
                .Where( x => selectedScheduleIds.Contains( x.Occurrence.ScheduleId.Value ) ).ToList();

            SetAttendancesAsCheckedOut( attendancesToCheckout, rockContext );

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnNotPresent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnNotPresent_Click( object sender, RowEventArgs e )
        {
            var attendanceIds = GetRowAttendanceIds( e );
            if ( !attendanceIds.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var now = RockDateTime.Now;
                var attendanceService = new AttendanceService( rockContext );
                foreach ( var attendee in attendanceService
                    .Queryable()
                    .Where( a => attendanceIds.Contains( a.Id ) ) )
                {
                    // If they are getting changed from Present to NotPresent, clear out PresentDateTimeTime.
                    if ( attendee.PresentDateTime.HasValue )
                    {
                        attendee.PresentDateTime = null;
                        attendee.PresentByPersonAliasId = null;
                    }

                    // If they were Checked-out, clear the EndDateTime since they have been changed to Present.
                    if ( attendee.EndDateTime.HasValue )
                    {
                        attendee.EndDateTime = null;
                    }
                }

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion Control Events

        #region Internal Methods

        /// <summary>
        /// Resets control visibility to default values.
        /// </summary>
        private void ResetControlVisibility()
        {
            nbWarning.Visible = false;
            lpLocation.Visible = true;
            pnlSubPageNav.Visible = true;
            pnlRoster.Visible = true;
        }

        /// <summary>
        /// Gets the campus from the current context.
        /// </summary>
        private CampusCache GetCampusFromContext()
        {
            CampusCache campus = null;

            var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
            if ( campusEntityType != null )
            {
                var campusContext = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( campusContext != null )
                {
                    campus = CampusCache.Get( campusContext.Id );
                }
            }

            return campus;
        }

        /// <summary>
        /// Shows a warning message, and optionally hides the content panels.
        /// </summary>
        /// <param name="warningMessage">The warning message to show.</param>
        /// <param name="hideLocationPicker">Whether to hide the lpLocation control.</param>
        private void ShowWarningMessage( string warningMessage, bool hideLocationPicker )
        {
            nbWarning.Text = warningMessage;
            nbWarning.Visible = true;
            lpLocation.Visible = !hideLocationPicker;
            pnlSubPageNav.Visible = false;
            pnlRoster.Visible = false;
        }

        /// <summary>
        /// Initializes the sub page navigation.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private void InitializeSubPageNav( int locationId )
        {
            RockPage rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                PageCache page = PageCache.Get( rockPage.PageId );
                if ( page != null )
                {
                    pbSubPages.RootPageId = page.ParentPageId ?? 0;
                }
            }

            pbSubPages.QueryStringParametersToAdd = new NameValueCollection
            {
                { PageParameterKey.LocationId, locationId.ToString() }
            };
        }

        /// <summary>
        /// Gets the status filter value from the bgStatus control.
        /// </summary>
        /// <returns></returns>
        private RosterStatusFilter GetStatusFilterValueFromControl()
        {
            RosterStatusFilter statusFilter = bgStatus.SelectedValue.ConvertToEnumOrNull<RosterStatusFilter>() ?? RosterStatusFilter.Unknown;
            return statusFilter;
        }

        /// <summary>
        /// Sets the value of the bgStatus control.
        /// </summary>
        /// <param name="statusFilter">The status filter.</param>
        private void SetStatusFilterControl( RosterStatusFilter statusFilter )
        {
            bgStatus.SelectedValue = statusFilter.ToString( "d" );
        }

        /// <summary>
        /// Returns true of any if the checkin areas support Enable Presence
        /// </summary>
        /// <param name="checkinAreas">The checkin areas.</param>
        /// <returns></returns>
        private bool HasPresenceEnabled( GroupTypeCache[] checkinAreas )
        {
            var checkinConfigurationTypes = checkinAreas.Select( a => a.GetCheckInConfigurationType() );
            return checkinConfigurationTypes.Any( a => a != null && a.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean() );
        }

        /// <summary>
        /// Returns true of any if the checkin areas support Allow Checkout
        /// </summary>
        /// <param name="checkinAreas">The checkin areas.</param>
        /// <returns></returns>
        private bool HasCheckoutEnabled( GroupTypeCache[] checkinAreas )
        {
            var checkinConfigurationTypes = checkinAreas.Select( a => a.GetCheckInConfigurationType() );
            return checkinConfigurationTypes.Any( a => a != null && a.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT ).AsBoolean() );
        }

        /// <summary>
        /// Removes the unneeded status filters based on the whether any of the rooms have EnablePresence and/or AllowCheckout
        /// </summary>
        /// <param name="attendees">The attendees.</param>
        private void RemoveUnneededStatusFilters( GroupTypeCache[] checkinAreas )
        {
            var selectedStatusFilter = GetStatusFilterValueFromControl();

            // Reset the visibility, just in case the control was previously hidden.
            bgStatus.Visible = true;

            var checkinConfigurationTypes = checkinAreas.Select( a => a.GetCheckInConfigurationType() );

            var showPresenceControls = HasPresenceEnabled( checkinAreas );
            var showAllowCheckoutControls = HasCheckoutEnabled( checkinAreas );

            if ( !showAllowCheckoutControls )
            {
                var checkedOutItem = bgStatus.Items.FindByValue( RosterStatusFilter.CheckedOut.ToString( "d" ) );
                if ( checkedOutItem != null )
                {
                    bgStatus.Items.Remove( checkedOutItem );
                }
            }

            if ( !showPresenceControls )
            {
                // If EnablePresence is false, it doesn't make sense to show the 'Checked-in' filter.
                var checkedInItem = bgStatus.Items.FindByValue( RosterStatusFilter.CheckedIn.ToString( "d" ) );
                if ( checkedInItem != null )
                {
                    bgStatus.Items.Remove( checkedInItem );
                }

                // When EnablePresence is false for a given Check-in Area, the [Attendance].[PresentDateTime] value will have already been set upon check-in.
                if ( !showAllowCheckoutControls )
                {
                    // If there aren't any attendees that are in rooms that have EnabledPresence or AllowCheckout,
                    // it doesn't make sense to show the status filters at all.
                    bgStatus.Visible = false;
                    SetStatusFilterControl( RosterStatusFilter.Present );
                }

                if ( selectedStatusFilter == RosterStatusFilter.CheckedIn )
                {
                    // if Presence is NOT enabled, there isn't a CheckedIn status. 
                    SetStatusFilterControl( RosterStatusFilter.Present );
                }
            }
        }

        /// <summary>
        /// Toggles the column visibility within the gAttendees grid based on the current filter
        /// and the AllowCheckout and EnablePresence settings for the rooms in the attendee list
        /// </summary>
        /// <param name="attendees">The attendees.</param>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        private void ToggleColumnVisibility( IEnumerable<RosterAttendee> attendees, RosterStatusFilter rosterStatusFilter )
        {
            bool anyRoomHasAllowCheckout = attendees.Any( a => a.RoomHasAllowCheckout );
            bool anyRoomHasEnablePresence = attendees.Any( a => a.RoomHasEnablePresence );

            // StatusFilter.All:
            var serviceTimesField = gAttendees.ColumnsOfType<RockBoundField>().First( c => c.DataField == "ServiceTimes" );
            var btnCancelField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnCancel" );

            // StatusFilter.Checked-in:
            var lElapsedCheckInTimeField = gAttendees.ColumnsOfType<RockLiteralField>().First( c => c.ID == "lElapsedCheckInTime" );
            var lElapsedPresentTimeField = gAttendees.ColumnsOfType<RockLiteralField>().First( c => c.ID == "lElapsedPresentTime" );
            var lElapsedCheckedOutTimeField = gAttendees.ColumnsOfType<RockLiteralField>().First( c => c.ID == "lElapsedCheckedOutTime" );

            var btnPresentField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnPresent" );

            // StatusFilter.Present:
            var btnCheckOutField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnCheckOut" );
            var btnStayingField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnStaying" );
            var btnNotPresentField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnNotPresent" );
            btnCheckoutAll.Visible = GetAttributeValue( AttributeKey.EnableCheckoutAll ).AsBoolean()
                && anyRoomHasAllowCheckout
                && rosterStatusFilter == RosterStatusFilter.Present;

            serviceTimesField.Visible = true;

            lElapsedCheckInTimeField.Visible = rosterStatusFilter == RosterStatusFilter.CheckedIn;
            lElapsedPresentTimeField.Visible = rosterStatusFilter == RosterStatusFilter.Present;
            lElapsedCheckedOutTimeField.Visible = rosterStatusFilter == RosterStatusFilter.CheckedOut;

            // Only show the CancelField Column if they are on the CheckedIn or Present tab.
            // The actual button's visibility will be determined per row in the btnCancel_OnDatabound event.
            btnCancelField.Visible = rosterStatusFilter == RosterStatusFilter.CheckedIn || rosterStatusFilter == RosterStatusFilter.Present;

            // Only show the PresentField Column if they are on the CheckedIn tab
            // or they are on the CheckedOut tab and the Mark Present Button is enabled.
            // The actual button's visibility will be determined per row in the btnPresent_OnDatabound event.
            if ( rosterStatusFilter == RosterStatusFilter.CheckedIn )
            {
                hfMarkPresentShowConfirmation.Value = false.ToJavaScriptValue();
                btnPresentField.Visible = true;
            }
            else if ( rosterStatusFilter == RosterStatusFilter.CheckedOut && GetAttributeValue( AttributeKey.EnableMarkPresentButton ).AsBoolean() )
            {
                // Show a confirmation if changing a person from CheckedOut to Present.
                hfMarkPresentShowConfirmation.Value = true.ToJavaScriptValue();
                btnPresentField.Visible = true;
            }
            else
            {
                btnPresentField.Visible = false;
            }

            btnStayingField.Visible = rosterStatusFilter == RosterStatusFilter.Present && GetAttributeValue( AttributeKey.EnableStayingButton ).AsBoolean();
            btnNotPresentField.Visible = anyRoomHasEnablePresence &&
                rosterStatusFilter == RosterStatusFilter.Present
                && GetAttributeValue( AttributeKey.EnableNotPresentButton ).AsBoolean();

            if ( anyRoomHasAllowCheckout )
            {
                // Only show these action button's Column if they are on the Present Tab.
                // The actual button's visibility will be determined per row in the btnCheckout_OnDatabound event.
                btnCheckOutField.Visible = rosterStatusFilter == RosterStatusFilter.Present;
            }
            else
            {
                // If none of the rooms of the displayed attendees has AllowCheckout, don't show the column.
                btnCheckOutField.Visible = false;
            }
        }

        #endregion Internal Methods
    }
}
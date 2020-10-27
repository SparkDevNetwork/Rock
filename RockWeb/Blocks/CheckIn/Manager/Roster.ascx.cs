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

        #region Entity Attribute Value Keys

        /// <summary>
        /// Keys to use for entity attribute values.
        /// </summary>
        private class EntityAttributeValueKey
        {
            public const string GroupType_AllowCheckout = "core_checkin_AllowCheckout";
            public const string GroupType_EnablePresence = "core_checkin_EnablePresence";
        }

        #endregion Entity Attribute Value Keys

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

        /// <summary>
        /// If <seealso cref="AttributeKey.ShowAllAreas"/> is set to false, the 'Check-in Configuration' (which is a <see cref="Rock.Model.GroupType" /> Guid) to limit to
        /// For example "Weekly Service Check-in".
        /// </summary>
        public GroupTypeCache GetCheckinAreaFilter()
        {
            // If a Check-in Area query string parameter is defined, it takes precedence.
            Guid? checkinManagerPageParameterCheckinAreaGuid = PageParameter( PageParameterKey.Area ).AsGuidOrNull();
            if ( checkinManagerPageParameterCheckinAreaGuid.HasValue )
            {
                var checkinManagerPageParameterCheckinArea = GroupTypeCache.Get( checkinManagerPageParameterCheckinAreaGuid.Value );

                if ( checkinManagerPageParameterCheckinArea != null )
                {
                    return checkinManagerPageParameterCheckinArea;
                }
            }

            // if ShowAllAreas is enabled, we won't filter by Check-in Area (unless there was a page parameter)
            if ( this.GetAttributeValue( AttributeKey.ShowAllAreas ).AsBoolean() )
            {
                return null;
            }

            // we ShowAllAreas is false, get the area filter from the cookie
            var checkinManagerCookieCheckinAreaGuid = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CheckinAreaGuid;
            if ( checkinManagerCookieCheckinAreaGuid != null )
            {
                var checkinManagerCookieCheckinArea = GroupTypeCache.Get( checkinManagerCookieCheckinAreaGuid.Value );
                if ( checkinManagerCookieCheckinArea != null )
                {
                    return checkinManagerCookieCheckinArea;
                }
            }

            // Next, check the Block AttributeValue.
            var checkinManagerBlockAttributeCheckinAreaGuid = this.GetAttributeValue( AttributeKey.CheckInAreaGuid ).AsGuidOrNull();
            if ( checkinManagerBlockAttributeCheckinAreaGuid.HasValue )
            {
                var checkinManagerBlockAttributeCheckinArea = GroupTypeCache.Get( checkinManagerBlockAttributeCheckinAreaGuid.Value );
                if ( checkinManagerBlockAttributeCheckinArea != null )
                {
                    return checkinManagerBlockAttributeCheckinArea;
                }
            }

            return null;
        }

        /// <summary>
        /// The current status filter.
        /// </summary>
        public RosterStatusFilter CurrentStatusFilter
        {
            get
            {
                RosterStatusFilter statusFilter = ViewState[ViewStateKey.CurrentStatusFilter] as RosterStatusFilter? ?? RosterStatusFilter.Unknown;
                return statusFilter;
            }

            set
            {
                ViewState[ViewStateKey.CurrentStatusFilter] = value;
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
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            BuildRoster();
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
            Location location = lpLocation.Location;
            if ( location != null )
            {
                SetSelectedLocation( location.Id );
            }
            else
            {
                SetSelectedLocation( 0 );
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
        }

        #endregion Control Events

        #region Roster Grid Related

        /// <summary>
        /// Builds the roster for the selected campus and location.
        /// </summary>
        private void BuildRoster()
        {
            ResetControlVisibility();

            // if ShowAllAreas is false, the CheckinAreaFilter is required
            if ( this.GetAttributeValue( AttributeKey.ShowAllAreas ).AsBoolean() == false )
            {
                var checkinAreaFilter = GetCheckinAreaFilter();
                if ( checkinAreaFilter == null )
                {
                    if ( NavigateToLinkedPage( AttributeKey.AreaSelectPage ) )
                    {
                        // we are navigating to get the Area Filter, which will get the Area cookie
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

            // If the Campus selection has changed, we need to reload the LocationItemPicker with the Locations specific to that Campus.
            if ( campus.Id != CurrentCampusId )
            {
                CurrentCampusId = campus.Id;
                lpLocation.NamedPickerRootLocationId = campus.LocationId.GetValueOrDefault();
            }

            // Check the LocationPicker for the Location ID.
            int locationId = lpLocation.Location != null
                ? lpLocation.Location.Id
                : 0;

            if ( locationId <= 0 )
            {
                // If not defined on the LocationPicker, check first for a LocationId Page parameter.
                locationId = PageParameter( PageParameterKey.LocationId ).AsInteger();

                if ( locationId > 0 )
                {
                    // If the Page parameter was set, make sure it's valid for the selected Campus.
                    if ( !IsLocationWithinCampus( locationId ) )
                    {
                        locationId = 0;
                    }
                }

                if ( locationId > 0 )
                {
                    CheckinManagerHelper.SaveCampusLocationConfigurationToCookie( CurrentCampusId, locationId );
                }
                else
                {
                    // If still not defined, check for a Block user preference.
                    locationId = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().LocationIdFromSelectedCampusId.GetValueOrNull( CurrentCampusId ) ?? 0;

                    if ( locationId <= 0 )
                    {
                        ShowWarningMessage( "Please select a Location.", false );
                        return;
                    }
                }

                SetSelectedLocation( locationId );
            }

            InitializeSubPageNav( locationId );

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

            // If the Location or StatusFilter selections have changed, we need to reload the attendees.
            if ( locationId != CurrentLocationId || statusFilter != CurrentStatusFilter )
            {
                CurrentLocationId = locationId;
                CurrentStatusFilter = statusFilter;

                ShowAttendees();
            }
        }

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
            lBadges.Text = string.Format( "<div>{0}</div>", attendee.GetBadgesHtml( false ) );

            // Mobile only.
            var lMobileTagAndSchedules = e.Row.FindControl( "lMobileTagAndSchedules" ) as Literal;
            lMobileTagAndSchedules.Text = attendee.GetMobileTagAndSchedulesHtml();

            // Desktop only.
            // Show how it has been since they have checked in (and not yet marked present)
            var lElapsedCheckInTime = e.Row.FindControl( "lElapsedCheckInTime" ) as Literal;
            lElapsedCheckInTime.Text = RockFilters.HumanizeTimeSpan( attendee.CheckInTime, DateTime.Now, unit: "Second" );

            // Desktop only.
            var lStatusTag = e.Row.FindControl( "lStatusTag" ) as Literal;
            lStatusTag.Text = attendee.GetStatusIconHtmlTag( false );
            lElapsedCheckInTime.Visible = CurrentStatusFilter == RosterStatusFilter.CheckedIn;
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
            btnCancel.Visible = ( CurrentStatusFilter == RosterStatusFilter.CheckedIn ) || ( rosterAttendee.RoomHasEnablePresence == false && CurrentStatusFilter == RosterStatusFilter.Present );
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
            var btnPresentLinkButton = sender as LinkButton;
            btnPresentLinkButton.Visible = rosterAttendee.RoomHasEnablePresence;
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

        /// <summary>
        /// Shows the attendees.
        /// </summary>
        private void ShowAttendees()
        {
            IList<RosterAttendee> attendees = null;

            using ( var rockContext = new RockContext() )
            {
                attendees = GetAttendees( rockContext );
            }

            ToggleColumnVisibility();

            var attendeesSorted = attendees.OrderByDescending( a => a.Status == RosterAttendeeStatus.Present ).ThenByDescending( a => a.CheckInTime ).ThenBy( a => a.PersonGuid ).ToList();

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
                && a.StartDateTime <= currentDateTime
                && a.Occurrence.GroupId.HasValue
                && a.PersonAliasId.HasValue
                && a.Occurrence.LocationId == CurrentLocationId
                && a.Occurrence.ScheduleId.HasValue );

            var checkinAreaFilter = this.GetCheckinAreaFilter();

            if ( checkinAreaFilter != null )
            {
                // if there is a checkin area filter, limit to groups within the selected check-in area
                var checkinAreaGroupTypeIds = new GroupTypeService( new RockContext() ).GetCheckinAreaDescendants( checkinAreaFilter.Id ).Select( a => a.Id ).ToList();
                attendanceQuery = attendanceQuery.Where( a => checkinAreaGroupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );
            }

            /*
                If StatusFilter == All, no further filtering is needed.
                If StatusFilter == Checked-in, only retrieve records that have neither a EndDateTime nor a PresentDateTime value.
                If StatusFilter == Present, only retrieve records that have a PresentDateTime value but don't have a EndDateTime value.
            */

            var unfilteredAttendanceCheckinAreas = attendanceQuery.Select( a => a.Occurrence.Group.GroupTypeId ).ToList().Select( a => GroupTypeCache.Get( a ) ).ToArray();

            RemoveUnneededStatusFilters( unfilteredAttendanceCheckinAreas );

            if ( CurrentStatusFilter == RosterStatusFilter.CheckedIn )
            {
                attendanceQuery = attendanceQuery.Where( a => !a.PresentDateTime.HasValue && !a.EndDateTime.HasValue );
            }
            else if ( CurrentStatusFilter == RosterStatusFilter.Present )
            {
                attendanceQuery = attendanceQuery.Where( a => a.PresentDateTime.HasValue && !a.EndDateTime.HasValue );
            }

            var attendanceList = attendanceQuery
                .Include( a => a.AttendanceCode )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.Occurrence.Group )
                .AsNoTracking()
                .ToList();

            var groupTypeIds = attendanceList.Select( a => a.Occurrence.Group.GroupTypeId ).Distinct().ToList();
            var groupTypes = groupTypeIds.Select( a => GroupTypeCache.Get( a ) );
            var groupTypeIdsWithAllowCheckout = groupTypes
                .Where( gt => gt.GetAttributeValue( EntityAttributeValueKey.GroupType_AllowCheckout ).AsBoolean() )
                .Where( a => a != null )
                .Select( a => a.Id )
                .Distinct();

            attendanceList = attendanceList.Where( a =>
            {
                var allowCheckout = groupTypeIdsWithAllowCheckout.Contains( a.Occurrence.Group.GroupTypeId );
                if ( !allowCheckout )
                {
                    /* 
                        If AllowCheckout is false, remove all Attendees whose schedules are not currently active. Per the 'WasSchedule...ActiveForCheckOut()'
                        method below: "Check-out can happen while check-in is active or until the event ends (start time + duration)." This will help to keep
                        the list of 'Present' attendees cleaned up and accurate, based on the room schedules, since the volunteers have no way to manually mark
                        an Attendee as 'Checked-out'.

                        If, on the other hand, AllowCheckout is true, it will be the volunteers' responsibility to click the [Check-out] button when an
                        Attendee leaves the room, in order to keep the list of 'Present' Attendees in order. This will also allow the volunteers to continue
                        'Checking-out' Attendees in the case that the parents are running late in picking them up.
                    */

                    return a.Occurrence.Schedule.WasScheduleOrCheckInActiveForCheckOut( currentDateTime );
                }
                else
                {
                    return true;
                }
            } ).ToList();

            attendanceList = attendanceList.Where( a => a.PersonAlias != null && a.PersonAlias.Person != null ).ToList();
            var attendees = RosterAttendee.GetFromAttendanceList( attendanceList );

            return attendees;
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
            // the attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more)
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

            ShowAttendees();
        }

        /// <summary>
        /// Gets the row attendance ids.
        /// </summary>
        /// <param name="rowEventArgs">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private int[] GetRowAttendanceIds( RowEventArgs rowEventArgs )
        {
            // the attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more)
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
                }

                rockContext.SaveChanges();
            }

            ShowAttendees();
        }

        /// <summary>
        /// Handles the Click event of the btnCheckOut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnCheckOut_Click( object sender, RowEventArgs e )
        {
            // the attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more)
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
                    attendee.EndDateTime = now;
                    attendee.CheckedOutByPersonAliasId = CurrentPersonAliasId;
                }

                rockContext.SaveChanges();
            }

            ShowAttendees();
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

                campus = CampusCache.Get( campusContext );
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
        /// Determines whether the specified location is within the current campus.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private bool IsLocationWithinCampus( int locationId )
        {
            using ( var rockContext = new RockContext() )
            {
                var locationCampusId = new LocationService( rockContext ).GetCampusIdForLocation( locationId );
                return locationCampusId == CurrentCampusId;
            }
        }

        /// <summary>
        /// Sets the selected location
        /// </summary>
        /// <param name="locationId">The identifier of the location.</param>
        private void SetSelectedLocation( int? locationId )
        {
            if ( locationId.HasValue && locationId > 0 )
            {
                CheckinManagerHelper.SaveCampusLocationConfigurationToCookie( CurrentCampusId, locationId );
                var pageParameterLocationId = this.PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
                if ( !pageParameterLocationId.HasValue || pageParameterLocationId.Value != locationId )
                {
                    var additionalQueryParameters = new Dictionary<string, string>();
                    additionalQueryParameters.Add( PageParameterKey.LocationId, locationId.ToString() );
                    NavigateToCurrentPageReference( additionalQueryParameters );
                    return;
                }

                using ( var rockContext = new RockContext() )
                {
                    Location location = new LocationService( rockContext ).Get( locationId.Value );
                    if ( location != null )
                    {
                        lpLocation.Location = location;
                    }
                }
            }
            else
            {
                lpLocation.Location = null;
                CheckinManagerHelper.SaveCampusLocationConfigurationToCookie( CurrentCampusId, null );
            }
        }

        /// <summary>
        /// Initializes the sub page navigation.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private void InitializeSubPageNav( int locationId)
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
        /// Removes the unneeded status filters based on the whether any of the rooms have EnablePresence and/or AllowCheckout
        /// </summary>
        /// <param name="attendees">The attendees.</param>
        private void RemoveUnneededStatusFilters( GroupTypeCache[] checkinAreas )
        {
            // Reset the visibility, just in case the control was previously hidden.
            bgStatus.Visible = true;

            var checkinConfigurationTypes = checkinAreas.Select( a => a.GetCheckInConfigurationType() );

            var showPresenceControls = checkinConfigurationTypes.Any( a => a != null && a.GetAttributeValue( EntityAttributeValueKey.GroupType_EnablePresence ).AsBoolean() );
            var showAllowCheckoutControls = checkinConfigurationTypes.Any( a => a != null && a.GetAttributeValue( EntityAttributeValueKey.GroupType_AllowCheckout ).AsBoolean() );

            if ( !showPresenceControls )
            {
                // When EnablePresence is false for a given Check-in Area, the [Attendance].[PresentDateTime] value will have already been set upon check-in.
                if ( !showAllowCheckoutControls )
                {
                    // If there aren't any attendees that are in rooms that have EnabledPresence or AllowCheckout,
                    // it doesn't make sense to show the status filters at all.
                    bgStatus.Visible = false;
                    CurrentStatusFilter = RosterStatusFilter.Present;

                    return;
                }

                // If EnablePresence is false, it doesn't make sense to show the 'Checked-in' filter.
                var checkedInItem = bgStatus.Items.FindByValue( RosterStatusFilter.CheckedIn.ToString( "d" ) );
                if ( checkedInItem != null )
                {
                    bgStatus.Items.Remove( checkedInItem );
                }

                if ( CurrentStatusFilter == RosterStatusFilter.CheckedIn )
                {
                    CurrentStatusFilter = RosterStatusFilter.Present;
                    SetStatusFilterControl( CurrentStatusFilter );
                }
            }
        }

        /// <summary>
        /// Toggles the column visibility within the gAttendees grid based on the current filter
        /// </summary>
        private void ToggleColumnVisibility()
        {
            // StatusFilter.All:
            var mobileIconField = gAttendees.ColumnsOfType<RockLiteralField>().First( c => c.ID == "lMobileIcon" );
            var serviceTimesField = gAttendees.ColumnsOfType<RockBoundField>().First( c => c.DataField == "ServiceTimes" );
            var statusTagField = gAttendees.ColumnsOfType<RockLiteralField>().First( c => c.ID == "lStatusTag" );
            var btnCancelField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnCancel" );

            // StatusFilter.Checked-in:
            var lElapsedCheckInTimeField = gAttendees.ColumnsOfType<RockLiteralField>().First( c => c.ID == "lElapsedCheckInTime" );
            var btnPresentField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnPresent" );

            // StatusFilter.Present:
            var btnCheckOutField = gAttendees.ColumnsOfType<LinkButtonField>().First( c => c.ID == "btnCheckOut" );

            mobileIconField.Visible = CurrentStatusFilter == RosterStatusFilter.All;
            serviceTimesField.Visible = CurrentStatusFilter == RosterStatusFilter.All || CurrentStatusFilter == RosterStatusFilter.Present;
            statusTagField.Visible = CurrentStatusFilter == RosterStatusFilter.All;

            lElapsedCheckInTimeField.Visible = CurrentStatusFilter == RosterStatusFilter.CheckedIn;

            // only show the CancelField Column if they are on the CheckedIn or Present tab
            // The actual button's visibility will be determined per row in the btnCancel_OnDatabound event
            btnCancelField.Visible = CurrentStatusFilter == RosterStatusFilter.CheckedIn || CurrentStatusFilter == RosterStatusFilter.Present;

            // only show the PresentField Column if they are on the CheckedIn tab
            // The actual button's visibility will be determined per row in the btnPresent_OnDatabound event
            btnPresentField.Visible = CurrentStatusFilter == RosterStatusFilter.CheckedIn;

            // only show these action button's Column if they are on the Present Tab
            // The actual button's visibility will be determined per row in the btnCheckout_OnDatabound event
            btnCheckOutField.Visible = CurrentStatusFilter == RosterStatusFilter.Present;
        }

        #endregion Internal Methods
    }
}
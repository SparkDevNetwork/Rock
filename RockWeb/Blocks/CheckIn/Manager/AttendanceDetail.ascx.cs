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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Attendance Detail" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block to show details of a person's attendance" )]

    [LinkedPage(
        "Profile Page",
        Description = "The page to go back to after deleting this attendance.",
        Key = AttributeKey.PersonProfilePage,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER,
        IsRequired = false,
        Order = 6
        )]
    public partial class AttendanceDetail : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
        }

        #endregion

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string AttendanceGuid = "Attendance";
            public const string AttendanceId = "AttendanceId";

            /// <summary>
            /// The person Guid
            /// </summary>
            public const string PersonGuid = "Person";

            /// <summary>
            /// The person identifier
            /// </summary>
            public const string PersonId = "PersonId";
        }

        #endregion PageParameterKeys

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

            if ( !Page.IsPostBack )
            {
                ShowDetail( GetAttendanceId() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }


        #region Move Person

        /*  12-07-2021 MDP

        This Move Person code in this #region is nearly identical in both the RockWeb.Blocks.CheckIn.Manager
        EnRoute and AttendanceDetail Blocks. If changes are made to one, make sure to update the other.

        */

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var attendanceId = GetAttendanceId();
            if ( !attendanceId.HasValue )
            {
                return;
            }

            var attendanceInfo = new AttendanceService( new RockContext() ).GetSelect( attendanceId.Value, s => new {
                s.Occurrence,
                s.CampusId
            } );

            var occurrence = attendanceInfo?.Occurrence;
            if ( occurrence == null )
            {
                return;
            }

            var attendanceCampusId = attendanceInfo?.CampusId;

            if ( attendanceCampusId.HasValue )
            {
                // if the selected attendance is at specific campus, set the location picker to that campus's locations
                var campusLocationId = CampusCache.Get( attendanceCampusId.Value )?.LocationId;
                lpMovePersonLocation.RootLocationId = campusLocationId ?? 0;
            }
            else
            {
                lpMovePersonLocation.RootLocationId = 0;
            }

            // Location picker will list all named locations
            lpMovePersonLocation.SetValueFromLocationId( occurrence.LocationId );

            // Groups depend on selected Location
            LoadMovePersonGroups( occurrence.LocationId );

            // Now that Groups is populated, set to selected group
            ddlMovePersonGroup.SetValue( occurrence.GroupId );

            // Now load list of schedules based on Group and Location
            LoadMovePersonSchedules( occurrence.GroupId, occurrence.LocationId );
            ddlMovePersonSchedule.SetValue( occurrence.ScheduleId );

            mdMovePerson.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMovePersonGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedLocationId = lpMovePersonLocation.SelectedValueAsId();
            var selectedGroupId = ddlMovePersonGroup.SelectedValueAsId();
            LoadMovePersonSchedules( selectedGroupId, selectedLocationId );
        }

        /// <summary>
        /// Handles the SelectItem event of the lpMovePersonLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lpMovePersonLocation_SelectItem( object sender, EventArgs e )
        {
            // just in case a location warning is showing, we can hide if they are selecting a different location
            nbMovePersonLocationFull.Visible = false;
            var selectedLocationId = lpMovePersonLocation.SelectedValueAsId();
            LoadMovePersonGroups( selectedLocationId );

            var selectedGroupId = ddlMovePersonGroup.SelectedValueAsId();
            LoadMovePersonSchedules( selectedGroupId, selectedLocationId );

        }

        /// <summary>
        /// Loads the move person schedules based on selected Group and Location
        /// </summary>
        /// <param name="selectedGroupId">The selected group identifier.</param>
        /// <param name="selectedLocationId">The selected location identifier.</param>
        private void LoadMovePersonSchedules( int? selectedGroupId, int? selectedLocationId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var scheduleService = new ScheduleService( rockContext );


            /* 12-7-2021 MDP

                There is special logic for populating the Schedule, Group and Location pickers and it isn't very intuitive.
                Also, this logic could change based on feedback. If it does change, make sure to append this engineering note.
                As of 12-7-2021, this is how it should work: 

              - List All Checkin Locations
              - List Groups associated with selected Location
              - List schedules limited to selected Group and Location and
                  - Named schedule
                  - Active
                  - Not limited to Checkin Time
                  - Not limited to ones that have a CheckInStartOffsetMinutes specified.
                  - Not limited to only current day's schedules
              - If Group is not selected, don't filter schedules by Group
              - If Location is not selected,  don't filter Groups by Location and don't filter Schedules by Location

            */

            var groupLocationQuery = new GroupLocationService( rockContext ).Queryable();
            if ( selectedGroupId.HasValue )
            {
                groupLocationQuery = groupLocationQuery.Where( a => a.GroupId == selectedGroupId.Value );
            }

            if ( selectedLocationId.HasValue )
            {
                groupLocationQuery = groupLocationQuery.Where( a => a.LocationId == selectedLocationId.Value );
            }

            var scheduleQry = scheduleService.Queryable().Where( a =>
                a.IsActive
                && groupLocationQuery.Any( x => x.Schedules.Any( s => s.Id == a.Id ) )
                && a.Name != null
                && a.Name != string.Empty ).ToList();

            var scheduleList = scheduleQry.ToList();


            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();
            ddlMovePersonSchedule.Items.Clear();

            foreach ( var schedule in sortedScheduleList )
            {
                ddlMovePersonSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Updates the list of Groups in the 'Move Person' dialog based on selected Location
        /// </summary>
        private void LoadMovePersonGroups( int? selectedLocationId )
        {
            var currentlySelectedGroupId = ddlMovePersonGroup.SelectedValueAsId();
            ddlMovePersonGroup.Items.Clear();

            var rockContext = new RockContext();

            IQueryable<GroupLocation> availableGroupQuery;

            if ( selectedLocationId.HasValue )
            {
                // If a Location is selected, get all Groups that have a GroupLocation for the selected Location and have any active schedule, but not filtered by selected Schedule
                availableGroupQuery = new GroupLocationService( rockContext ).Queryable().Where( a => a.LocationId == selectedLocationId && a.Schedules.Where( s => s.IsActive ).Any() );
            }
            else
            {
                // If a Location is not selected, get all Groups that have a GroupLocation that has any active schedule
                // Don't filter by selected Schedule and don't filter to a specific location since no location is selected.
                availableGroupQuery = new GroupLocationService( rockContext ).Queryable().Where( a => a.LocationId == selectedLocationId && a.Schedules.Where( s => s.IsActive ).Any() );
            }

            var groupsQuery = new GroupService( rockContext ).Queryable();
            var availableGroupList = groupsQuery
                .Include( a => a.ParentGroup )
                .Where( g => g.IsActive
                    && !g.IsArchived
                    && availableGroupQuery.Any( x => x.GroupId == g.Id ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .AsNoTracking()
                .ToList();

            foreach ( var group in availableGroupList )
            {
                if ( group.ParentGroup != null )
                {
                    ddlMovePersonGroup.Items.Add( new ListItem( $"{group.ParentGroup.Name} > {group.Name}", group.Id.ToString() ) );
                }
                else
                {
                    ddlMovePersonGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                }
            }

            ddlMovePersonGroup.SetValue( currentlySelectedGroupId );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMovePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMovePerson_SaveClick( object sender, EventArgs e )
        {
            var attendanceId = GetAttendanceId();
            if ( attendanceId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            var attendance = attendanceService.Get( attendanceId.Value );
            if ( attendance == null )
            {
                return;
            }

            var selectedOccurrenceDate = attendance.Occurrence.OccurrenceDate;
            var selectedScheduleId = ddlMovePersonSchedule.SelectedValueAsId();
            var selectedLocationId = lpMovePersonLocation.SelectedValueAsId();
            var selectedGroupId = ddlMovePersonGroup.SelectedValueAsId();
            if ( !selectedLocationId.HasValue || !selectedGroupId.HasValue || !selectedScheduleId.HasValue )
            {
                return;
            }

            var location = NamedLocationCache.Get( selectedLocationId.Value );

            var locationFirmRoomThreshold = location?.FirmRoomThreshold;
            if ( locationFirmRoomThreshold.HasValue )
            {
                // The totalAttended is the number of people still checked in (not people who have been checked-out)
                // not counting the current person who may already be checked in,
                // + the person we are trying to move
                var locationCount = attendanceService.GetByDateOnLocationAndSchedule( selectedOccurrenceDate, selectedLocationId.Value, selectedScheduleId.Value )
                                            .Where( a => a.EndDateTime == null && a.PersonAlias.PersonId != attendance.PersonAlias.PersonId ).Count();

                if ( ( locationCount + 1 ) >= locationFirmRoomThreshold.Value )
                {
                    nbMovePersonLocationFull.Text = $"The {location} has reached its hard threshold capacity and cannot be used for check-in.";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }
            }

            var newRoomsOccurrence = attendanceOccurrenceService.GetOrAdd( selectedOccurrenceDate, selectedGroupId, selectedLocationId, selectedScheduleId );
            attendance.OccurrenceId = newRoomsOccurrence.Id;
            rockContext.SaveChanges();

            mdMovePerson.Hide();
            ShowDetail( GetAttendanceId() );
        }

        #endregion Move Person

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new AttendanceService( rockContext );

                var attendanceId = GetAttendanceId();

                if ( attendanceId == null )
                {
                    return;
                }

                var attendance = service.Get( attendanceId.Value );
                if ( attendance == null )
                {
                    return;
                }

                var personGuid = attendance.PersonAlias.Person?.Guid;

                service.Delete( attendance );
                rockContext.SaveChanges();

                var queryParams = new Dictionary<string, string>();

                queryParams.Add( PageParameterKey.PersonGuid, personGuid.ToString() );

                NavigateToLinkedPage( AttributeKey.PersonProfilePage, queryParams );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the attendance id based on what was passed into the page parameters
        /// </summary>
        private int? GetAttendanceId()
        {
            int? attendanceId = PageParameter( PageParameterKey.AttendanceId ).AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                return attendanceId;
            }

            Guid? attendanceGuid = PageParameter( PageParameterKey.AttendanceGuid ).AsGuidOrNull();

            if ( attendanceGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    attendanceId = new AttendanceService( rockContext ).GetId( attendanceGuid.Value );
                }
            }

            return attendanceId;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="attendanceGuid">The attendance unique identifier.</param>
        private void ShowDetail( int? attendanceId )
        {
            if ( !attendanceId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            // Fetch the attendance record and include all the details that we'll be displaying.
            var attendance = attendanceService.Queryable()
                .Where( a => a.Id == attendanceId.Value && a.PersonAliasId.HasValue )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.Occurrence.Group )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.Occurrence.Location )
                .Include( a => a.AttendanceCode )
                .Include( a => a.CheckedOutByPersonAlias.Person )
                .Include( a => a.PresentByPersonAlias.Person )
                .AsNoTracking()
                .FirstOrDefault();

            if ( attendance == null )
            {
                return;
            }

            ShowAttendanceDetails( attendance );

            btnEdit.Enabled = this.IsUserAuthorized( Rock.Security.Authorization.EDIT );
            btnDelete.Enabled = this.IsUserAuthorized( Rock.Security.Authorization.EDIT );
        }

        /// <summary>
        /// Sets the checkin label (Time, Name, PhoneNumber, etc)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="rockLiteral">The rock literal.</param>
        private static void SetCheckinInfoLabel( RockContext rockContext, DateTime? dateTime, int? personAliasId, RockLiteral rockLiteral )
        {
            if ( dateTime == null )
            {
                rockLiteral.Visible = false;
            }

            rockLiteral.Text = dateTime.Value.ToShortDateTimeString();

            if ( !personAliasId.HasValue )
            {
                return;
            }

            var personAliasService = new PersonAliasService( rockContext );
            var person = personAliasService.GetSelect( personAliasId.Value, s => s.Person );

            if ( person == null )
            {
                return;
            }

            var checkedInByPersonName = person.FullName;
            var checkedInByPersonPhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            rockLiteral.Text = string.Format( "{0 } by {1} {2}", dateTime.Value.ToShortDateTimeString(), checkedInByPersonName, checkedInByPersonPhone );
        }

        /// <summary>
        /// Shows the attendance details.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowAttendanceDetails( Attendance attendance )
        {
            var occurrence = attendance.Occurrence;

            btnDelete.Attributes["onclick"] = $"javascript: return Rock.dialogs.confirmDelete(event, 'Check-in for {attendance.PersonAlias}');";

            var groupType = GroupTypeCache.Get( occurrence.Group.GroupTypeId );
            var rockContext = new RockContext();
            var groupPath = new GroupTypeService( rockContext ).GetAllCheckinAreaPaths().FirstOrDefault( a => a.GroupTypeId == occurrence.Group.GroupTypeId );

            lGroupName.Text = string.Format( "{0} > {1}", groupPath, occurrence.Group );
            if ( occurrence.Location != null )
            {
                lLocationName.Text = occurrence.Location.Name;
            }

            if ( attendance.AttendanceCode != null )
            {
                lTag.Text = attendance.AttendanceCode.Code;
            }

            if ( occurrence.Schedule != null )
            {
                lScheduleName.Text = occurrence.Schedule.Name;
            }

            SetCheckinInfoLabel( rockContext, attendance.StartDateTime, attendance.CheckedInByPersonAliasId, lCheckinTime );

            if ( attendance.PresentDateTime.HasValue )
            {
                SetCheckinInfoLabel( rockContext, attendance.PresentDateTime, attendance.PresentByPersonAliasId, lPresentTime );
            }
            else
            {
                lPresentTime.Visible = false;
                pnlPresentDetails.Visible = false;
            }

            if ( attendance.EndDateTime.HasValue )
            {
                lCheckedOutTime.Visible = true;
                SetCheckinInfoLabel( rockContext, attendance.EndDateTime, attendance.CheckedOutByPersonAliasId, lCheckedOutTime );
            }
            else
            {
                lCheckedOutTime.Visible = false;
                pnlCheckedOutDetails.Visible = false;
            }
        }

        #endregion Methods
    }
}
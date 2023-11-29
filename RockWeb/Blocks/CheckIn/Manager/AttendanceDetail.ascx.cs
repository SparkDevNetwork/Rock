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

    [BooleanField(
    "Allow Editing Start and End Times",
        Key = AttributeKey.AllowEditingStartAndEndTimes,
        Description = "This allows editing the start and end datetime to be edited",
        DefaultBooleanValue = false,
        Order = 7 )]

    [Rock.SystemGuid.BlockTypeGuid( "CA59CE67-9313-4B9F-8593-380044E5AE6A" )]
    public partial class AttendanceDetail : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string AllowEditingStartAndEndTimes = "AllowEditingStartAndEndTimes";
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

            pnlCheckInCheckOutEdit.Visible = GetAttributeValue( AttributeKey.AllowEditingStartAndEndTimes ).AsBoolean();

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
            nbMovePersonLocationFull.Visible = false;
            nbMovePersonLocationFull.Text = string.Empty;

            var attendanceId = GetAttendanceId();
            if ( !attendanceId.HasValue )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var attendanceInfo = new AttendanceService( new RockContext() ).GetSelect( attendanceId.Value, s => new { s.Occurrence, s.CampusId } );
            var occurrence = attendanceInfo?.Occurrence;
            if ( occurrence == null )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var attendanceCampusId = attendanceInfo?.CampusId;

            /*
             * 2023-06-22 ETD
             * 
             * In order to make the selection based on the schedule and not the location we need to reverse the normal FK relationship. Instead
             * of starting out with a group and location, we need to start with a schedule, and then load the GroupLocationSchedules.
             */

            Load_ddlMovePersonSchedule( occurrence.ScheduleId, attendanceCampusId );
            Load_ddlMovePersonLocations( occurrence.LocationId, attendanceCampusId );
            Load_ddlMovePersonGroups( occurrence.GroupId );

            mdMovePerson.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMovePersonSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            // retain the selected location if it is available for the selected schedule
            var previouslySelectedLocationId = ddlMovePersonLocation.SelectedValueAsInt();
            Load_ddlMovePersonLocations( previouslySelectedLocationId, null );

            // retain the selected group if it is available for the selected schedule/location
            var previouslySelectedGroupId = ddlMovePersonGroup.SelectedValueAsInt();
            Load_ddlMovePersonGroups( previouslySelectedGroupId );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMovePersonLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            // retain the selected group if it is available for the selected schedule/location
            var previouslySelectedGroupId = ddlMovePersonGroup.SelectedValueAsInt();
            Load_ddlMovePersonGroups( previouslySelectedGroupId );
        }

        /// <summary>
        /// Loads ddlMovePersonSchedule with the available schedules. Filters by campus if provided and will select the provided schedule if it is valid.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        private void Load_ddlMovePersonSchedule( int? scheduleId, int? campusId )
        {
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var activeSchedules = groupLocationService.Queryable().SelectMany( gl => gl.Schedules ).Distinct().ToList();
            var currentDateTime = CampusCache.Get( campusId.Value )?.CurrentDateTime ?? RockDateTime.Now;
            List<Schedule> scheduleList = new List<Schedule>();

            foreach ( var schedule in activeSchedules )
            {
                var isAvailable = schedule.GetCheckInTimes( currentDateTime ).Any( t => t.CheckInStart <= currentDateTime && t.CheckInEnd > currentDateTime );
                if ( isAvailable )
                {
                    scheduleList.Add( schedule );
                }
            }

            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();
            ddlMovePersonSchedule.Items.Clear();

            foreach ( var schedule in sortedScheduleList )
            {
                ddlMovePersonSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }

            if ( sortedScheduleList.Where( s => s.Id == scheduleId ).Any() )
            {
                ddlMovePersonSchedule.SelectedValue = scheduleId.ToString();
            }
        }

        /// <summary>
        /// Loads ddlMovePersonLocations with the available locations based on the selected schedule and will select the provided location if it is valid.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        private void Load_ddlMovePersonLocations( int? locationId, int? campusId )
        {
            int? scheduleId = ddlMovePersonSchedule.SelectedValue.AsIntegerOrNull();
            if ( scheduleId.IsNullOrZero() )
            {
                // Clear the locations and disable selection until a schedule is provided.
                ddlMovePersonLocation.Items.Clear();
                ddlMovePersonLocation.Enabled = false;

                return;
            }

            // Get a list of GroupLocations for the schedule
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocations = groupLocationService.Queryable().Where( gl => gl.Schedules.Any( s => s.Id == scheduleId ) );

            // See if we can get a campus if one was not provided
            if ( campusId.IsNullOrZero() )
            {
                var attendanceId = GetAttendanceId();
                campusId = new AttendanceService( rockContext ).Queryable().Where( a => a.Id == attendanceId.Value ).Select( a => a.CampusId ).FirstOrDefault();
            }

            // Load the Locations filtered by schedule
            var campusLocationId = CampusCache.Get( campusId.Value )?.LocationId;
            var locationService = new LocationService( rockContext );
            List<Location> locations = null;
            if ( campusLocationId != null )
            {
                // Get a list of locations that are children of the campus then JOIN it to the list of groupLocations to filter it.
                locations = locationService
                    .GetAllDescendents( campusLocationId.Value )
                    .Join( groupLocations, l => l.Id, gl => gl.LocationId, ( l, gl ) => l )
                    .Where( l => l.IsActive == true )
                    .Distinct()
                    .OrderBy( l => l.Name )
                    .ToList();
            }
            else
            {
                locations = locationService
                    .Queryable()
                    .Join( groupLocations, l => l.Id, gl => gl.LocationId, ( l, gl ) => l )
                    .Where( l => l.IsActive == true )
                    .Distinct()
                    .OrderBy( l => l.Name )
                    .ToList();
            }

            ddlMovePersonLocation.Enabled = true;
            ddlMovePersonLocation.Items.Clear();

            // If there is more than one option add a blank option. This blank option will be auto selected if there is more than one available location and the previous location is not available for the selected schedule.
            if ( locations.Count > 1 )
            {
                ddlMovePersonLocation.Items.Add( new ListItem( string.Empty, string.Empty ) );
            }

            foreach ( var location in locations )
            {
                ddlMovePersonLocation.Items.Add( new ListItem( location.Name, location.Id.ToString() ) );
            }

            if ( locationId.IsNotNullOrZero() && locations.Where( l => l.Id == locationId ).Any() )
            {
                ddlMovePersonLocation.SelectedValue = locationId.ToString();
            }
        }

        /// <summary>
        /// Loads ddlMovePersonGroups with the available groups based on the selected schedule/location and will selected the provided group if it is valid.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        private void Load_ddlMovePersonGroups( int? groupId )
        {
            int? scheduleId = ddlMovePersonSchedule.SelectedValue.AsIntegerOrNull();
            int? locationId = ddlMovePersonLocation.SelectedValue.AsIntegerOrNull();

            if ( scheduleId.IsNullOrZero() || locationId.IsNullOrZero() )
            {
                // Clear the groups and disable selection until a location is provided.
                ddlMovePersonGroup.Items.Clear();
                ddlMovePersonGroup.Enabled = false;
                return;
            }

            // Get a list of GroupLocations for the schedule
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var groups = groupLocationService
                .Queryable()
                .Where( gl => gl.Schedules.Any( s => s.Id == scheduleId ) && gl.LocationId == locationId && gl.Group.IsActive )
                .Select( gl => gl.Group )
                .Include( a => a.ParentGroup )
                .ToList();

            // Load the Groups filtered by schedule and location
            ddlMovePersonGroup.Enabled = true;
            ddlMovePersonGroup.Items.Clear();

            // If there is more than one group add a blank option. This blank option will be auto selected if there is more than one available group and the previous group is not available for the selected schedule/location.
            if ( groups.Count > 1 )
            {
                ddlMovePersonGroup.Items.Add( new ListItem( string.Empty, string.Empty ) );
            }

            foreach ( var group in groups )
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

            if ( groupId != null && groups.Where( g => g.Id == groupId ).Any() )
            {
                ddlMovePersonGroup.SelectedValue = groupId.ToString();
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMovePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMovePerson_SaveClick( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;
            nbMovePersonLocationFull.Text = string.Empty;

            var attendanceId = GetAttendanceId();
            if ( attendanceId == null )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.Get( attendanceId.Value );
            if ( attendance == null )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var selectedScheduleId = ddlMovePersonSchedule.SelectedValueAsId();
            if ( !selectedScheduleId.HasValue )
            {
                nbMovePersonLocationFull.Text = "Schedule Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var selectedLocationId = ddlMovePersonLocation.SelectedValueAsId();
            if ( !selectedLocationId.HasValue )
            {
                nbMovePersonLocationFull.Text = "Location Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var selectedGroupId = ddlMovePersonGroup.SelectedValueAsId();
            if ( !selectedGroupId.HasValue )
            {
                nbMovePersonLocationFull.Text = "Group Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var selectedOccurrenceDate = attendance.Occurrence.OccurrenceDate;

            var location = NamedLocationCache.Get( selectedLocationId.Value );
            var locationFirmRoomThreshold = location?.FirmRoomThreshold;
            if ( locationFirmRoomThreshold.HasValue )
            {
                // The totalAttended is the number of people still checked in (not people who have been checked-out)
                // not counting the current person who may already be checked in,
                // + the person we are trying to move
                var locationCount = attendanceService
                    .GetByDateOnLocationAndSchedule( selectedOccurrenceDate, selectedLocationId.Value, selectedScheduleId.Value )
                    .Where( a => a.EndDateTime == null && a.PersonAlias.PersonId != attendance.PersonAlias.PersonId )
                    .Count();

                if ( ( locationCount + 1 ) >= locationFirmRoomThreshold.Value )
                {
                    nbMovePersonLocationFull.Text = $"The {location} has reached its hard threshold capacity and cannot be used for check-in.";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }
            }
            attendance.StartDateTime = dtpStart.SelectedDateTime ?? attendance.StartDateTime;
            attendance.EndDateTime = dtpEnd.SelectedDateTime;

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
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

            var historyService = new HistoryService( rockContext );
            var attendanceEntityTypeId = EntityTypeCache.GetId<Rock.Model.Attendance>();
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
            rptHistoryList.DataSource = historyService.Queryable()
                .Where( h => h.EntityId == attendance.PersonAlias.PersonId
                && h.EntityTypeId == personEntityTypeId
                && h.RelatedEntityTypeId == attendanceEntityTypeId
                && h.RelatedEntityId == attendance.Id )
                .ToList()
                .Select( h => new ChangeHistoryData
                {
                    CreatedPersonName = h.CreatedByPersonName,
                    CreatedPersonId = h.CreatedByPersonId.ToStringSafe(),
                    CreatedDateTime = h.CreatedDateTime.ToElapsedString( false, true ) ?? "",
                    Description = h.ToStringSafe()
                } )
                .ToList();

            btnEdit.Enabled = this.IsUserAuthorized( Rock.Security.Authorization.EDIT );
            rptHistoryList.DataBind();
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
            dtpStart.SelectedDateTime = attendance.StartDateTime;

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
                pnlCheckedOutDetails.Visible = true;
                lCheckedOutTime.Visible = true;
                SetCheckinInfoLabel( rockContext, attendance.EndDateTime, attendance.CheckedOutByPersonAliasId, lCheckedOutTime );
                dtpEnd.SelectedDateTime = attendance.EndDateTime;
            }
            else
            {
                lCheckedOutTime.Visible = false;
                pnlCheckedOutDetails.Visible = false;
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// POCO for storing the data to be displayed on the change history tab.
    /// </summary>
    public class ChangeHistoryData
    {
        public string CreatedPersonName;
        public string CreatedPersonId;
        public string CreatedDateTime;
        public string Description;
    }
}
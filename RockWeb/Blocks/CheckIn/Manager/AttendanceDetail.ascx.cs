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

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Block to show details of a person's attendance.
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
        Order = 0 )]

    [BooleanField(
        "Allow Editing Start and End Times",
        Key = AttributeKey.AllowEditingStartAndEndTimes,
        Description = "This allows editing the start and end datetime.",
        DefaultBooleanValue = false,
        Order = 1 )]

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

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string ScheduleListItems = "ScheduleListItems";
            public const string LocationListItems = "LocationListItems";
            public const string GroupListItems = "GroupListItems";
        }

        #endregion ViewState Keys

        #region Fields

        private const string GroupListItemKeyDelimiter = "|";

        private List<ListItemBag> _scheduleListItems = new List<ListItemBag>();
        private Dictionary<string, List<ListItemBag>> _locationListItemsBySchedule = new Dictionary<string, List<ListItemBag>>();
        private Dictionary<string, List<ListItemBag>> _groupListItemsByScheduleAndLocation = new Dictionary<string, List<ListItemBag>>();

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState[ViewStateKey.ScheduleListItems] as string;
            if ( json.IsNullOrWhiteSpace() )
            {
                _scheduleListItems = new List<ListItemBag>();
            }
            else
            {
                _scheduleListItems = JsonConvert.DeserializeObject<List<ListItemBag>>( json ) ?? new List<ListItemBag>();
            }

            json = ViewState[ViewStateKey.LocationListItems] as string;
            if ( json.IsNullOrWhiteSpace() )
            {
                _locationListItemsBySchedule = new Dictionary<string, List<ListItemBag>>();
            }
            else
            {
                _locationListItemsBySchedule = JsonConvert.DeserializeObject<Dictionary<string, List<ListItemBag>>>( json ) ?? new Dictionary<string, List<ListItemBag>>();
            }

            json = ViewState[ViewStateKey.GroupListItems] as string;
            if ( json.IsNullOrWhiteSpace() )
            {
                _groupListItemsByScheduleAndLocation = new Dictionary<string, List<ListItemBag>>();
            }
            else
            {
                _groupListItemsByScheduleAndLocation = JsonConvert.DeserializeObject<Dictionary<string, List<ListItemBag>>>( json ) ?? new Dictionary<string, List<ListItemBag>>();
            }
        }

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

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKey.ScheduleListItems] = JsonConvert.SerializeObject( _scheduleListItems, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.LocationListItems] = JsonConvert.SerializeObject( _locationListItemsBySchedule, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.GroupListItems] = JsonConvert.SerializeObject( _groupListItemsByScheduleAndLocation, Formatting.None, jsonSetting );

            return base.SaveViewState();
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

            using ( var rockContext = new RockContext() )
            {
                var attendance = new AttendanceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( a => a.Occurrence.Group )
                    .FirstOrDefault( a => a.Id == attendanceId.Value );

                if ( attendance?.Occurrence?.Group == null )
                {
                    nbMovePersonLocationFull.Text = "Attendance Not Found";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }

                GetMovePersonOptions( rockContext, attendance );
                LoadMovePersonScheduleDropDown( attendance.Occurrence.ScheduleId );
                LoadMovePersonLocationDropDown( attendance.Occurrence.LocationId );
                LoadMovePersonGroupDropDown( attendance.Occurrence.GroupId );

                mdMovePerson.Show();
            }
        }

        /// <summary>
        /// Gets the available group, location and schedule combinations to which an individual may be moved.
        /// <para>
        /// Values will be added to private fields and subsequently saved to/loaded from ViewState for use
        /// within partial post backs.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attendance">The attendance instance.</param>
        private void GetMovePersonOptions( RockContext rockContext, Attendance attendance )
        {
            var groupLocationSchedules = CheckinManagerHelper.GetGroupLocationSchedulesForPersonMove( rockContext, attendance );
            if ( groupLocationSchedules?.Any() != true )
            {
                return;
            }

            CheckinManagerHelper.GroupAndSortMovePersonOptions(
                groupLocationSchedules,
                out _scheduleListItems,
                out _locationListItemsBySchedule,
                out _groupListItemsByScheduleAndLocation,
                GroupListItemKeyDelimiter
            );
        }

        /// <summary>
        /// Loads the list of schedules in the "Move Person" dialog based on active schedules for the current check-in configuration.
        /// </summary>
        /// <param name="currentScheduleId">The currently-selected schedule ID. This value will be reselected if it's still an option.</param>
        private void LoadMovePersonScheduleDropDown( int? currentScheduleId )
        {
            ddlMovePersonSchedule.Items.Clear();

            if ( _scheduleListItems?.Any() == true )
            {
                foreach ( var scheduleListItem in _scheduleListItems )
                {
                    ddlMovePersonSchedule.Items.Add( new ListItem( scheduleListItem.Text, scheduleListItem.Value ) );
                }

                if ( currentScheduleId.HasValue && _scheduleListItems.Any( s => s.Value == currentScheduleId.Value.ToString() ) )
                {
                    // If the currently-selected value is still an option, auto-select it.
                    ddlMovePersonSchedule.SetValue( currentScheduleId );
                }
                else
                {
                    // If there is only a single possible selection, auto-select that option.
                    if ( _scheduleListItems.Count == 1 )
                    {
                        ddlMovePersonSchedule.SetValue( _scheduleListItems.First().Value );
                        return;
                    }

                    // Otherwise, blank out the selection and force the person to select a value.
                    ddlMovePersonSchedule.Items.Insert( 0, new ListItem( string.Empty, Rock.Constants.None.Text ) );
                }
            }
        }

        /// <summary>
        /// Loads the list of locations in the "Move Person" dialog based on the selected schedule drop down list value.
        /// </summary>
        /// <param name="currentLocationId">The currently-selected location ID. This value will be reselected if it's still an option.</param>
        private void LoadMovePersonLocationDropDown( int? currentLocationId )
        {
            ddlMovePersonLocation.Items.Clear();

            var selectedScheduleId = ddlMovePersonSchedule.SelectedValue;

            List<ListItemBag> locationListItems = null;
            if ( _locationListItemsBySchedule?.TryGetValue( selectedScheduleId, out locationListItems ) == true && locationListItems?.Any() == true )
            {
                foreach ( var locationListItem in locationListItems )
                {
                    ddlMovePersonLocation.Items.Add( new ListItem( locationListItem.Text, locationListItem.Value ) );
                }

                if ( currentLocationId.HasValue && locationListItems.Any( l => l.Value == currentLocationId.Value.ToString() ) )
                {
                    // If the currently-selected value is still an option, auto-select it.
                    ddlMovePersonLocation.SetValue( currentLocationId );
                }
                else
                {
                    // If there is only a single possible selection, auto-select that option.
                    if ( locationListItems.Count == 1 )
                    {
                        ddlMovePersonLocation.SetValue( locationListItems.First().Value );
                        return;
                    }

                    // Otherwise, blank out the selection and force the person to select a value.
                    ddlMovePersonLocation.Items.Insert( 0, new ListItem( string.Empty, Rock.Constants.None.Text ) );
                }
            }
        }

        /// <summary>
        /// Loads the list of groups in the "Move Person" dialog based on the selected schedule and location drop down list values.
        /// </summary>
        /// <param name="currentGroupId">The currently-selected group ID. This value will be reselected if it's still an option.</param>
        private void LoadMovePersonGroupDropDown( int? currentGroupId )
        {
            ddlMovePersonGroup.Items.Clear();

            var groupListItemKey = $"{ddlMovePersonSchedule.SelectedValue}{GroupListItemKeyDelimiter}{ddlMovePersonLocation.SelectedValue}";

            List<ListItemBag> groupListItems = null;
            if ( _groupListItemsByScheduleAndLocation?.TryGetValue( groupListItemKey, out groupListItems ) == true && groupListItems?.Any() == true )
            {
                foreach ( var groupListItem in groupListItems )
                {
                    ddlMovePersonGroup.Items.Add( new ListItem( groupListItem.Text, groupListItem.Value ) );
                }

                if ( currentGroupId.HasValue && groupListItems.Any( g => g.Value == currentGroupId.Value.ToString() ) )
                {
                    // If the currently-selected value is still an option, auto-select it.
                    ddlMovePersonGroup.SetValue( currentGroupId );
                }
                else
                {
                    // If there is only a single possible selection, auto-select that option.
                    if ( groupListItems.Count == 1 )
                    {
                        ddlMovePersonGroup.SetValue( groupListItems.First().Value );
                        return;
                    }

                    // Otherwise, blank out the selection and force the person to select a value.
                    ddlMovePersonGroup.Items.Insert( 0, new ListItem( string.Empty, Rock.Constants.None.Text ) );
                }
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlMovePersonSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;

            LoadMovePersonLocationDropDown( ddlMovePersonLocation.SelectedValueAsId() );
            LoadMovePersonGroupDropDown( ddlMovePersonGroup.SelectedValueAsId() );
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlMovePersonLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;

            LoadMovePersonGroupDropDown( ddlMovePersonGroup.SelectedValueAsId() );
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlMovePersonGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;
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

            nbError.Visible = false;
            nbError.Text = string.Empty;

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

            if ( dtpEnd.SelectedDateTime.HasValue && dtpEnd.SelectedDateTime < attendance.StartDateTime )
            {
                nbError.Visible = true;
                nbError.Text = "Check-out Date/Time should be after the Check-in Date/Time.";
                return;
            }
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
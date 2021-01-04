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
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Group Schedule Communication" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows an individual to create a communication based on group schedule criteria." )]
    public partial class GroupScheduleCommunication : RockBlock
    {
        #region User Preference Keys

        private static class UserPreferenceKey
        {
            public const string UserPreferenceConfigurationJSON = "UserPreferenceConfigurationJSON";
        }

        #endregion User Preference Keys

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

            if ( !Page.IsPostBack )
            {
                PopulateDropDowns();
                ShowDetails();
            }
            else
            {
                HandleCustomPostbackEvents();
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
            // nothing needed for Block Updated since there aren't any block attributes
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroups_SelectItem( object sender, EventArgs e )
        {
            UpdateListsForSelectedGroups();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the lbSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSchedules_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateLocationListFromSelectedSchedules();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbIncludeChildGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIncludeChildGroups_CheckedChanged( object sender, EventArgs e )
        {
            UpdateListsForSelectedGroups();
        }

        /// <summary>
        /// Handles the Click event of the btnCreateCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateCommunication_Click( object sender, EventArgs e )
        {
            // Create communication
            var rockContext = new RockContext();
            var communicationService = new Rock.Model.CommunicationService( rockContext );
            var communication = new Rock.Model.Communication();
            communication.IsBulkCommunication = false;
            communication.Status = CommunicationStatus.Transient;
            communication.SenderPersonAliasId = this.CurrentPersonAliasId;

            if ( this.Request != null && this.Request.Url != null )
            {
                communication.UrlReferrer = this.Request.Url.AbsoluteUri.TrimForMaxLength( communication, "UrlReferrer" );
            }

            communicationService.Add( communication );

            // save communication to get Id
            rockContext.SaveChanges();

            int[] scheduleIds = lbSchedules.SelectedValuesAsInt.ToArray();
            int[] locationIds = cblLocations.SelectedValuesAsInt.ToArray();
            List<int> parentGroupIds = gpGroups.SelectedValuesAsInt().ToList();

            var allGroupIds = new List<int>();
            allGroupIds.AddRange( parentGroupIds );

            if ( cbIncludeChildGroups.Checked )
            {
                var groupService = new GroupService( rockContext );
                foreach ( var groupId in parentGroupIds )
                {
                    // just the first level of child groups, not all decendants
                    var childGroupIds = groupService.Queryable()
                            .Where( a => a.ParentGroupId == groupId && a.GroupType.IsSchedulingEnabled && !a.DisableScheduling )
                            .Select( a => a.Id ).ToList();
                    allGroupIds.AddRange( childGroupIds );
                }
            }

            allGroupIds = allGroupIds.Distinct().ToList();

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

            var sundayDate = ddlWeek.SelectedValue.AsDateTime() ?? RockDateTime.Now.SundayDate();

            var attendanceOccurrenceQuery = attendanceOccurrenceService
                .Queryable()
                .Where( a => a.ScheduleId.HasValue && a.LocationId.HasValue && a.GroupId.HasValue )
                .WhereDeducedIsActive()
                .Where( a => allGroupIds.Contains( a.GroupId.Value ) )
                .Where( a => locationIds.Contains( a.LocationId.Value ) )
                .Where( a => scheduleIds.Contains( a.ScheduleId.Value ) )
                .Where( a => a.SundayDate == sundayDate );

            ScheduledAttendanceItemStatus[] selectedInviteStatus = cblInviteStatus.SelectedValues
                .Select( a => a.ConvertToEnum<ScheduledAttendanceItemStatus>() )
                .ToArray();

            // limit attendees to ones based on the selected invite status
            var scheduledAttendancesForOccurrenceQuery = attendanceOccurrenceQuery
                    .SelectMany( a => a.Attendees )
                    .WhereHasScheduledAttendanceItemStatus( selectedInviteStatus );

            var personIds = scheduledAttendancesForOccurrenceQuery.Select( a => a.PersonAlias.PersonId ).Distinct().ToList();
            if ( !personIds.Any() )
            {
                nbCommunicationWarning.Text = "No people found to send communication to.";
                nbCommunicationWarning.Visible = true;
                return;
            }

            nbCommunicationWarning.Visible = false;

            var personAliasService = new Rock.Model.PersonAliasService( new Rock.Data.RockContext() );

            // Get the primary aliases
            List<Rock.Model.PersonAlias> primaryAliasList = new List<PersonAlias>( personIds.Count );

            // get the data in chunks just in case we have a large list of PersonIds (to avoid a SQL Expression limit error)
            var chunkedPersonIds = personIds.Take( 1000 );
            int skipCount = 0;
            while ( chunkedPersonIds.Any() )
            {
                var chunkedPrimaryAliasList = personAliasService.Queryable()
                    .Where( p => p.PersonId == p.AliasPersonId && chunkedPersonIds.Contains( p.PersonId ) ).AsNoTracking().ToList();
                primaryAliasList.AddRange( chunkedPrimaryAliasList );
                skipCount += 1000;
                chunkedPersonIds = personIds.Skip( skipCount ).Take( 1000 );
            }

            // NOTE: Set CreatedDateTime, ModifiedDateTime, etc manually set we are using BulkInsert
            var currentDateTime = RockDateTime.Now;
            var currentPersonAliasId = this.CurrentPersonAliasId;

            var communicationRecipientList = primaryAliasList.Select( a => new Rock.Model.CommunicationRecipient
            {
                CommunicationId = communication.Id,
                PersonAliasId = a.Id,
                CreatedByPersonAliasId = currentPersonAliasId,
                ModifiedByPersonAliasId = currentPersonAliasId,
                CreatedDateTime = currentDateTime,
                ModifiedDateTime = currentDateTime
            } ).ToList();

            // BulkInsert to quickly insert the CommunicationRecipient records. Note: This is much faster, but will bypass EF and Rock processing.
            var communicationRecipientRockContext = new RockContext();
            communicationRecipientRockContext.BulkInsert( communicationRecipientList );

            var pageRef = this.RockPage.Site.CommunicationPageReference;
            string communicationUrl;
            if ( pageRef.PageId > 0 )
            {
                pageRef.Parameters.AddOrReplace( "CommunicationId", communication.Id.ToString() );
                communicationUrl = pageRef.BuildUrl();
            }
            else
            {
                communicationUrl = "~/Communication/{0}";
            }

            if ( communicationUrl.Contains( "{0}" ) )
            {
                communicationUrl = string.Format( communicationUrl, communication.Id );
            }

            UserPreferenceConfiguration userPreferenceConfiguration = this.GetBlockUserPreference( UserPreferenceKey.UserPreferenceConfigurationJSON ).FromJsonOrNull<UserPreferenceConfiguration>() ?? new UserPreferenceConfiguration();
            userPreferenceConfiguration.GroupIds = gpGroups.SelectedValuesAsInt().ToArray();
            userPreferenceConfiguration.IncludeChildGroups = cbIncludeChildGroups.Checked;
            userPreferenceConfiguration.InviteStatuses = cblInviteStatus.SelectedValues.ToArray();
            userPreferenceConfiguration.ScheduleIds = lbSchedules.SelectedValuesAsInt.ToArray();
            userPreferenceConfiguration.LocationIds = cblLocations.SelectedValuesAsInt.ToArray();
            this.SetBlockUserPreference( UserPreferenceKey.UserPreferenceConfigurationJSON, userPreferenceConfiguration.ToJson() );

            Page.Response.Redirect( communicationUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the custom postback events.
        /// </summary>
        private void HandleCustomPostbackEvents()
        {
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                if ( postbackArgs == "select-all-locations" )
                {
                    var locationItems = cblLocations.Items.OfType<ListItem>().ToList();
                    bool selected = locationItems.All( a => !a.Selected );
                    foreach ( var cbLocation in locationItems )
                    {
                        cbLocation.Selected = selected;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a queryable of the selected groups
        /// </summary>
        /// <returns></returns>
        private IQueryable<Group> GetSelectedGroupsQuery( RockContext rockContext )
        {
            GroupService groupService;
            int[] selectedGroupIds = gpGroups.SelectedValues.AsIntegerList().ToArray();
            bool includeChildGroups = cbIncludeChildGroups.Checked;

            groupService = new GroupService( rockContext );
            var includedGroupIds = ( selectedGroupIds ?? new int[0] ).ToList();
            if ( includeChildGroups )
            {
                foreach ( var selectedGroupId in selectedGroupIds )
                {
                    var childGroupIds = groupService.Queryable().Where( a => a.ParentGroupId == selectedGroupId ).Select( a => a.Id ).ToList();

                    includedGroupIds.AddRange( childGroupIds );
                }
            }

            var groupsQuery = groupService.GetByIds( includedGroupIds.Distinct().ToList() );
            groupsQuery = groupsQuery.HasSchedulingEnabled();

            return groupsQuery;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            UserPreferenceConfiguration userPreferenceConfiguration = this.GetBlockUserPreference( UserPreferenceKey.UserPreferenceConfigurationJSON ).FromJsonOrNull<UserPreferenceConfiguration>();
            userPreferenceConfiguration = userPreferenceConfiguration ?? new UserPreferenceConfiguration
            {
                InviteStatuses = cblInviteStatus.SelectedValues.ToArray()
            };

            gpGroups.SetValues( userPreferenceConfiguration.GroupIds ?? new int[0] );
            cbIncludeChildGroups.Checked = userPreferenceConfiguration.IncludeChildGroups;
            cblInviteStatus.SetValues( userPreferenceConfiguration.InviteStatuses );
            UpdateListsForSelectedGroups();

            lbSchedules.SetValues( userPreferenceConfiguration.ScheduleIds ?? new int[0] );
            UpdateLocationListFromSelectedSchedules();

            cblLocations.SetValues( userPreferenceConfiguration.LocationIds ?? new int[0] );
        }

        /// <summary>
        /// Populates the drop downs.
        /// </summary>
        private void PopulateDropDowns()
        {
            cblInviteStatus.Items.Clear();
            cblInviteStatus.Items.Add( new ListItem( "Accepted", ScheduledAttendanceItemStatus.Confirmed.ToString() ) );
            cblInviteStatus.Items.Add( new ListItem( "Pending", ScheduledAttendanceItemStatus.Pending.ToString() ) );
            cblInviteStatus.Items.Add( new ListItem( "Declined", ScheduledAttendanceItemStatus.Declined.ToString() ) );
            cblInviteStatus.SetValue( ScheduledAttendanceItemStatus.Confirmed.ToString() );

            int numOfWeeks = 6;

            ddlWeek.Items.Clear();

            var sundayDate = RockDateTime.Now.SundayDate();
            int weekNum = 0;
            while ( weekNum < numOfWeeks )
            {
                string weekTitle = string.Format( "Week of {0} to {1}", sundayDate.AddDays( -6 ).ToShortDateString(), sundayDate.ToShortDateString() );
                ddlWeek.Items.Add( new ListItem( weekTitle, sundayDate.ToISO8601DateString() ) );
                weekNum++;
                sundayDate = sundayDate.AddDays( 7 );
            }
        }

        /// <summary>
        /// Updates the lists for selected groups.
        /// </summary>
        private void UpdateListsForSelectedGroups()
        {
            UpdateScheduleList();
            UpdateLocationListFromSelectedSchedules();
        }

        /// <summary>
        /// Updates the list of schedules for the selected groups
        /// </summary>
        private void UpdateScheduleList()
        {
            var rockContext = new RockContext();
            var includedGroupsQuery = GetSelectedGroupsQuery( rockContext );

            if ( !includedGroupsQuery.Any() )
            {
                return;
            }

            var groupSchedulesQuery = includedGroupsQuery.GetGroupSchedulingSchedules();
            var groupSchedulesList = groupSchedulesQuery.AsNoTracking().ToList();

            lbSchedules.Visible = true;
            if ( !groupSchedulesList.Any() )
            {
                lbSchedules.Visible = false;
                return;
            }

            // get any of the currently schedule ids, and reselect them if they still exist
            var selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList();

            lbSchedules.Items.Clear();

            List<Schedule> sortedScheduleList = groupSchedulesList.OrderByOrderAndNextScheduledDateTime();

            foreach ( var schedule in sortedScheduleList )
            {
                var listItem = new ListItem();
                if ( schedule.Name.IsNotNullOrWhiteSpace() )
                {
                    listItem.Text = schedule.Name;
                }
                else
                {
                    listItem.Text = schedule.FriendlyScheduleText;
                }

                listItem.Value = schedule.Id.ToString();
                listItem.Selected = selectedScheduleIds.Contains( schedule.Id );
                lbSchedules.Items.Add( listItem );
            }
        }

        /// <summary>
        /// Updates the location list from selected schedules.
        /// </summary>
        private void UpdateLocationListFromSelectedSchedules()
        {
            int[] selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList().ToArray();

            cblLocations.Visible = true;

            if ( !selectedScheduleIds.Any() )
            {
                cblLocations.Visible = false;
                return;
            }

            var rockContext = new RockContext();
            var includedGroupsQuery = GetSelectedGroupsQuery( rockContext );

            var groupLocationService = new GroupLocationService( rockContext );

            var groupLocationsQuery = includedGroupsQuery.GetGroupSchedulingGroupLocations();

            // narrow down group locations to ones for the selected schedules
            groupLocationsQuery = groupLocationsQuery.Where( a => a.Schedules.Any( s => selectedScheduleIds.Contains( s.Id ) ) );

            var locationList = groupLocationsQuery.Select( a => a.Location )
                .Select( l => new
                {
                    l.Id,
                    l.Name
                } )
                .AsNoTracking()
                .ToList()
                .DistinctBy( a => a.Id )
                .OrderBy( a => a.Name ).ToList();

            // get any of the currently location ids, and reselect them if they still exist
            var selectedLocationIds = cblLocations.SelectedValues.AsIntegerList();
            cblLocations.Items.Clear();

            foreach ( var location in locationList )
            {
                var locationListItem = new ListItem( location.Name, location.Id.ToString() );
                locationListItem.Selected = selectedLocationIds.Contains( location.Id );
                cblLocations.Items.Add( locationListItem );
            }

            if ( !locationList.Any() )
            {
                cblLocations.Visible = false;
                return;
            }
        }

        #endregion

        #region Private Classes

        private class UserPreferenceConfiguration
        {
            public int[] GroupIds { get; set; }

            public bool IncludeChildGroups { get; set; }

            public string[] InviteStatuses { get; set; }

            public int[] ScheduleIds { get; set; }

            public int[] LocationIds { get; set; }
        }

        #endregion
    }
}
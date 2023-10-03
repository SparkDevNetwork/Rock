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
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Group Schedule Roster" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows a person to view and print a roster by defining group schedule criteria." )]

    #region Block Attributes

    [BooleanField(
        "Enable Live Refresh",
        Key = AttributeKey.EnableLiveRefresh,
        Description = "When enabled, the page content will automatically refresh based on the 'Refresh Interval' setting.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 0 )]

    [RangeSlider(
        "Refresh Interval (seconds)",
        Key = AttributeKey.RefreshIntervalSeconds,
        IsRequired = true,
        Description = "The number of seconds to refresh the page. Note that setting this option too low could put a strain on the server if loaded on several clients at once.",
        DefaultIntegerValue = 30,
        MinValue = 10,
        MaxValue = 600,
        Order = 1 )]

    [CodeEditorField(
        "Roster Lava Template",
        Key = AttributeKey.RosterLavaTemplate,
        DefaultValue = AttributeDefault.RosterLavaDefault,
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        Order = 2 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "730F5D9E-A411-48F2-BBDF-51146C510817" )]
    public partial class GroupScheduleRoster : RockBlock
    {
        private static class AttributeDefault
        {
            public const string RosterLavaDefault = @"{% include '~~/Assets/Lava/GroupScheduleRoster.lava' %}";
        }

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string EnableLiveRefresh = "EnableLiveRefresh";
            public const string RefreshIntervalSeconds = "RefreshIntervalSeconds";
            public const string RosterLavaTemplate = "RosterLavaTemplate";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string GroupIds = "GroupIds";
            public const string IncludeChildGroups = "IncludeChildGroups";

            public const string LocationIds = "LocationIds";
            public const string ScheduleIds = "ScheduleIds";

            // by default, the roster only shows for the current date, but this can override that
            public const string OccurrenceDate = "OccurrenceDate";
        }

        #endregion PageParameterKeys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string RosterConfigurationJSON = "RosterConfigurationJSON";
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

            RockPage.AddScriptLink( "~/Scripts/idle-timer.min.js" );

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

            RockPage.AddCSSLink( "~/Themes/Rock/Styles/group-scheduler.css", true );

            if ( !this.IsPostBack )
            {
                UpdateConfigurationFromUrl();
                UpdateLiveRefreshConfiguration( this.GetAttributeValue( AttributeKey.EnableLiveRefresh ).AsBoolean() );

                var preferences = GetBlockPersonPreferences();
                RosterConfiguration rosterConfiguration = preferences.GetValue( UserPreferenceKey.RosterConfigurationJSON )
                .FromJsonOrNull<RosterConfiguration>() ?? new RosterConfiguration();

                if ( !rosterConfiguration.IsConfigured() )
                {
                    ShowConfigurationDialog();
                }
                else
                {
                    PopulateRoster();
                }
            }
            else
            {
                HandleCustomPostbackEvents();
            }
        }

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
        /// Updates the live refresh configuration.
        /// </summary>
        /// <param name="enableLiveRefresh">if set to <c>true</c> [enable live refresh].</param>
        private void UpdateLiveRefreshConfiguration( bool enableLiveRefresh )
        {
            if ( enableLiveRefresh )
            {
                hfRefreshTimerSeconds.Value = this.GetAttributeValue( AttributeKey.RefreshIntervalSeconds );
            }
            else
            {
                hfRefreshTimerSeconds.Value = string.Empty;
            }

            lLiveUpdateEnabled.Visible = enableLiveRefresh;
            lLiveUpdateDisabled.Visible = !enableLiveRefresh;
        }

        /// <summary>
        /// Updates the configuration from URL.
        /// </summary>
        private void UpdateConfigurationFromUrl()
        {
            /* 2020-07-21 MDP 
  If PageParameters are used, we use the same behavior as the various Analytics blocks  which is
    - Set UserPrefs from URL when first loaded, then Edit/Use/Save UserPrefs like usual
*/
            var preferences = GetBlockPersonPreferences();
            RosterConfiguration rosterConfiguration = preferences.GetValue( UserPreferenceKey.RosterConfigurationJSON )
                .FromJsonOrNull<RosterConfiguration>() ?? new RosterConfiguration();

            if ( this.PageParameter( PageParameterKey.GroupIds ).IsNotNullOrWhiteSpace() )
            {
                // if GroupIds is a page parameter, set GroupIds and IncludeChildGroups from the URL
                rosterConfiguration.PickedGroupIds = this.PageParameter( PageParameterKey.GroupIds ).Split( ',' ).AsIntegerList().ToArray();
            }

            if ( this.PageParameter( PageParameterKey.IncludeChildGroups ).IsNotNullOrWhiteSpace() )
            {
                rosterConfiguration.IncludeChildGroups = this.PageParameter( PageParameterKey.IncludeChildGroups ).AsBoolean();
            }

            if ( this.PageParameter( PageParameterKey.LocationIds ).IsNotNullOrWhiteSpace() )
            {
                // if LocationIds is a page parameter, use that as the LocationIds
                rosterConfiguration.LocationIds = this.PageParameter( PageParameterKey.LocationIds ).Split( ',' ).AsIntegerList().ToArray();
            }

            if ( this.PageParameter( PageParameterKey.ScheduleIds ).IsNotNullOrWhiteSpace() )
            {
                // if ScheduleIds is a page parameter, use that as the ScheduleIds
                rosterConfiguration.ScheduleIds = this.PageParameter( PageParameterKey.ScheduleIds ).Split( ',' ).AsIntegerList().ToArray();
            }

            if ( this.PageParameter( PageParameterKey.OccurrenceDate ).IsNotNullOrWhiteSpace() )
            {
                var occurrenceDate = this.PageParameter( PageParameterKey.OccurrenceDate ).AsDateTime();
                dpOccurrenceDate.Enabled = !occurrenceDate.HasValue;
                if ( occurrenceDate.HasValue )
                {
                    // if ScheduleIds is a page parameter, use that as the ScheduleIds
                    rosterConfiguration.OccurrenceDate = occurrenceDate;
                }
            }

            // just in case URL updated any configuration, save it back to user preferences
            preferences.SetValue( UserPreferenceKey.RosterConfigurationJSON, rosterConfiguration.ToJson() );
            preferences.Save();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            PopulateRoster();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            UpdateLiveRefreshConfiguration( this.GetAttributeValue( AttributeKey.EnableLiveRefresh ).AsBoolean() );
            PopulateRoster();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Populates the roster.
        /// In cases where there won't be a postback, we can disable view state. The only time we need viewstate is when the Configuration Dialog
        /// is showing.
        /// </summary>
        private void PopulateRoster( ViewStateMode viewStateMode = ViewStateMode.Disabled )
        {
            var preferences = GetBlockPersonPreferences();
            RosterConfiguration rosterConfiguration = preferences.GetValue( UserPreferenceKey.RosterConfigurationJSON )
                .FromJsonOrNull<RosterConfiguration>() ?? new RosterConfiguration();

            if ( !rosterConfiguration.IsConfigured() )
            {
                return;
            }

            int[] scheduleIds = rosterConfiguration.ScheduleIds;
            int[] locationIds = rosterConfiguration.LocationIds;
            List<int> pickedGroupIds = rosterConfiguration.PickedGroupIds.ToList();

            var allGroupIds = new List<int>();
            allGroupIds.AddRange( pickedGroupIds );

            var rockContext = new RockContext();

            // Only use teh ShowChildGroups option when there is 1 group selected
            if ( rosterConfiguration.IncludeChildGroups && pickedGroupIds.Count == 1 )
            {
                // if there is exactly one groupId we can avoid a 'Contains' (Contains has a small performance impact)
                var parentGroupId = pickedGroupIds[0];
                var groupService = new GroupService( rockContext );

                // just the first level of child groups, not all decendants
                var childGroupIds = groupService.Queryable().Where( a => a.ParentGroupId == parentGroupId ).Select( a => a.Id ).ToList();
                allGroupIds.AddRange( childGroupIds );
            }

            allGroupIds = allGroupIds.Distinct().ToList();

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

            // An OccurrenceDate probably won't be included in the URL, but just in case
            DateTime? occurrenceDate = rosterConfiguration.OccurrenceDate;
            if ( !occurrenceDate.HasValue )
            {
                occurrenceDate = RockDateTime.Today;
            }

            // only show occurrences for the current day
            var attendanceOccurrenceQuery = attendanceOccurrenceService
                .Queryable()
                .Where( a => a.ScheduleId.HasValue && a.LocationId.HasValue && a.GroupId.HasValue )
                .WhereDeducedIsActive()
                .Where( a =>
                    allGroupIds.Contains( a.GroupId.Value )
                    && a.OccurrenceDate == occurrenceDate
                    && scheduleIds.Contains( a.ScheduleId.Value ) );

            // if specific locations are specified, use those, otherwise just show all
            if ( locationIds.Any() )
            {
                attendanceOccurrenceQuery = attendanceOccurrenceQuery.Where( a => locationIds.Contains( a.LocationId.Value ) );
            }

            // limit attendees to ones that schedules (or are checked-in regardless of being scheduled)
            var confirmedAttendancesForOccurrenceQuery = attendanceOccurrenceQuery
                    .SelectMany( a => a.Attendees )
                    .Include( a => a.PersonAlias.Person )
                    .Include( a => a.Occurrence.Group.Members )
                    .WhereScheduledOrCheckedIn();

            var confirmedScheduledIndividualsForOccurrenceId = confirmedAttendancesForOccurrenceQuery
                .AsNoTracking()
                .ToList()
                .GroupBy( a => a.OccurrenceId )
                .ToDictionary(
                    k => k.Key,
                    v => v.Select( a => new ScheduledIndividual
                    {
                        ScheduledAttendanceItemStatus = Attendance.GetScheduledAttendanceItemStatus( a.RSVP, a.ScheduledToAttend ),
                        Person = a.PersonAlias.Person,
                        GroupMember = a.Occurrence.Group.Members.FirstOrDefault( gm => gm.PersonId == a.PersonAlias.PersonId ),
                        GroupMembers = a.Occurrence.Group.Members.Where( gm => gm.PersonId == a.PersonAlias.PersonId ).ToList(),
                        CurrentlyCheckedIn = a.DidAttend == true
                    } )
                    .ToList() );

            List<AttendanceOccurrence> attendanceOccurrenceList = attendanceOccurrenceQuery
                .Include( a => a.Schedule )
                .Include( a => a.Attendees )
                .Include( a => a.Group )
                .Include( a => a.Location )
                .AsNoTracking()
                .ToList()
                .OrderBy( a => a.OccurrenceDate )
                .ThenBy( a => a.Schedule.Order )
                .ThenBy( a => a.Schedule.GetNextStartDateTime( a.OccurrenceDate ) )
                .ThenBy( a => a.Location.Name )
                .ToList();

            var occurrenceRosterInfoList = new List<OccurrenceRosterInfo>();
            foreach ( var attendanceOccurrence in attendanceOccurrenceList )
            {
                var scheduleDate = attendanceOccurrence.Schedule.GetNextStartDateTime( attendanceOccurrence.OccurrenceDate );
                var scheduledIndividuals = confirmedScheduledIndividualsForOccurrenceId.GetValueOrNull( attendanceOccurrence.Id );

                if ( ( scheduleDate == null ) || ( scheduleDate.Value.Date != attendanceOccurrence.OccurrenceDate ) )
                {
                    // scheduleDate can be later than the OccurrenceDate (or null) if there are exclusions that cause the schedule
                    // to not occur on the occurrence date. In this case, don't show the roster unless there are somehow individuals
                    // scheduled for this occurrence.
                    if ( scheduledIndividuals == null || !scheduledIndividuals.Any() )
                    {
                        // no scheduleDate and no scheduled individuals, so continue on to the next attendanceOccurrence
                        continue;
                    }
                }

                var occurrenceRosterInfo = new OccurrenceRosterInfo
                {
                    Group = attendanceOccurrence.Group,
                    Location = attendanceOccurrence.Location,
                    Schedule = attendanceOccurrence.Schedule,
                    ScheduleDate = scheduleDate,
                    ScheduledIndividuals = scheduledIndividuals
                };

                occurrenceRosterInfoList.Add( occurrenceRosterInfo );
            }

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage );
            mergeFields.Add( "OccurrenceList", occurrenceRosterInfoList );
            mergeFields.Add( "DisplayRole", rosterConfiguration.DisplayRole );
            mergeFields.Add( "OccurrenceDate", occurrenceDate );
            var rosterLavaTemplate = this.GetAttributeValue( AttributeKey.RosterLavaTemplate );

            var rosterHtml = rosterLavaTemplate.ResolveMergeFields( mergeFields );

            // by default, let's disable viewstate (except for when the configuration dialog is showing)
            lOccurrenceRosterHTML.ViewStateMode = viewStateMode;
            lOccurrenceRosterHTML.Text = rosterHtml;
        }

        #endregion

        #region Configuration Related

        /// <summary>
        /// Handles the Click event of the btnConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfiguration_Click( object sender, EventArgs e )
        {
            ShowConfigurationDialog();
        }

        /// <summary>
        /// Shows the configuration dialog.
        /// </summary>
        private void ShowConfigurationDialog()
        {
            // even though it'll be in the background, show the roster (so it doesn't look like it disappeared)
            // also, since there will be postbacks, let's enable viewstate when the configuration dialog is showing
            PopulateRoster( ViewStateMode.Enabled );

            // don't do the live refresh when the configuration dialog is showing
            UpdateLiveRefreshConfiguration( false );

            var preferences = GetBlockPersonPreferences();
            RosterConfiguration rosterConfiguration = preferences.GetValue( UserPreferenceKey.RosterConfigurationJSON )
                            .FromJsonOrNull<RosterConfiguration>();

            if ( rosterConfiguration == null )
            {
                rosterConfiguration = new RosterConfiguration();
            }

            gpGroups.SetValues( rosterConfiguration.PickedGroupIds ?? new int[0] );
            cbIncludeChildGroups.Checked = rosterConfiguration.IncludeChildGroups;
            UpdateIncludeChildGroupsVisibility();

            UpdateScheduleList();
            lbSchedules.SetValues( rosterConfiguration.ScheduleIds ?? new int[0] );

            UpdateLocationListFromSelectedSchedules();
            cblLocations.SetValues( rosterConfiguration.LocationIds ?? new int[0] );
            dpOccurrenceDate.SelectedDate = rosterConfiguration.OccurrenceDate;
            cbDisplayRole.Checked = rosterConfiguration.DisplayRole;

            mdRosterConfiguration.Show();
        }

        /// <summary>
        /// Updates the include child groups visibility based on if there is only one Group selected, and if that group has child groups
        /// </summary>
        private void UpdateIncludeChildGroupsVisibility()
        {
            var pickerGroupIds = gpGroups.SelectedIds.ToList();
            bool showChildGroupsCheckbox =
                pickerGroupIds.Count == 1
                && new GroupService( new RockContext() ).Queryable().Where( a => a.ParentGroupId.HasValue && pickerGroupIds.Contains( a.ParentGroupId.Value ) ).Any();
            cbIncludeChildGroups.Visible = showChildGroupsCheckbox;
        }

        /// <summary>
        /// Updates the lists for selected groups.
        /// </summary>
        private void UpdateListsForSelectedGroups()
        {
            UpdateIncludeChildGroupsVisibility();
            UpdateScheduleList();
            UpdateLocationListFromSelectedSchedules();
        }

        /// <summary>
        /// Updates the list of schedules for the selected groups
        /// </summary>
        private void UpdateScheduleList()
        {
            var pickedGroupIds = gpGroups.SelectedIds;

            nbGroupWarning.Visible = false;
            nbLocationsWarning.Visible = false;

            if ( !pickedGroupIds.Any() )
            {
                nbGroupWarning.Text = "Select at least one group.";
                nbGroupWarning.Visible = true;
                return;
            }

            var rockContext = new RockContext();
            var includedGroupsQuery = GetSelectedSchedulingGroupsQuery( rockContext );

            var groupSchedulesQuery = includedGroupsQuery.GetGroupSchedulingSchedules();
            var groupSchedulesList = groupSchedulesQuery.AsNoTracking().ToList();

            lbSchedules.Visible = true;
            if ( !groupSchedulesList.Any() )
            {
                lbSchedules.Visible = false;
                nbGroupWarning.Text = "The selected groups do not have any locations or schedules";
                nbGroupWarning.Visible = true;
                return;
            }

            nbGroupWarning.Visible = false;

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
        /// Returns a queryable of the selected groups that have scheduling enabled
        /// </summary>
        /// <returns></returns>
        private IQueryable<Group> GetSelectedSchedulingGroupsQuery( RockContext rockContext )
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
        /// Updates the location list from selected schedules.
        /// </summary>
        private void UpdateLocationListFromSelectedSchedules()
        {
            int[] selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList().ToArray();

            cblLocations.Visible = true;
            nbLocationsWarning.Visible = false;

            if ( !selectedScheduleIds.Any() )
            {
                cblLocations.Visible = false;
                nbLocationsWarning.Text = "Select at least one schedule to see available locations";
                nbLocationsWarning.Visible = true;
                return;
            }

            var rockContext = new RockContext();
            var includedGroupsQuery = GetSelectedSchedulingGroupsQuery( rockContext );

            var groupLocationService = new GroupLocationService( rockContext );

            var groupLocationsQuery = includedGroupsQuery.GetGroupSchedulingGroupLocations();

            // narrow down group locations to ones for the selected schedules
            groupLocationsQuery = groupLocationsQuery.Where( a => a.Schedules.Any( s => selectedScheduleIds.Contains( s.Id ) ) );

            var locationList = groupLocationsQuery.Select( a => a.Location )
                .AsNoTracking()
                .ToList()
                .DistinctBy( a => a.Id )
                .OrderBy( a => a.ToString( true ) ).ToList();

            // get any of the currently location ids, and reselect them if they still exist
            var selectedLocationIds = cblLocations.SelectedValues.AsIntegerList();
            cblLocations.Items.Clear();

            foreach ( var location in locationList )
            {
                var locationListItem = new ListItem( location.ToString( true ), location.Id.ToString() );
                locationListItem.Selected = selectedLocationIds.Contains( location.Id );
                cblLocations.Items.Add( locationListItem );
            }

            if ( !locationList.Any() )
            {
                cblLocations.Visible = false;
                nbLocationsWarning.Text = "The selected groups do not have any active locations for the selected schedules";
                return;
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdRosterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdRosterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();
            RosterConfiguration rosterConfiguration = preferences.GetValue( UserPreferenceKey.RosterConfigurationJSON )
                .FromJsonOrNull<RosterConfiguration>();

            if ( rosterConfiguration == null )
            {
                rosterConfiguration = new RosterConfiguration();
            }

            rosterConfiguration.PickedGroupIds = gpGroups.SelectedValuesAsInt().ToArray();
            rosterConfiguration.IncludeChildGroups = cbIncludeChildGroups.Checked;
            rosterConfiguration.LocationIds = cblLocations.SelectedValuesAsInt.ToArray();
            rosterConfiguration.ScheduleIds = lbSchedules.SelectedValuesAsInt.ToArray();
            rosterConfiguration.DisplayRole = cbDisplayRole.Checked;
            rosterConfiguration.OccurrenceDate = dpOccurrenceDate.SelectedDate;

            preferences.SetValue( UserPreferenceKey.RosterConfigurationJSON, rosterConfiguration.ToJson() );
            preferences.Save();

            mdRosterConfiguration.Hide();

            UpdateLiveRefreshConfiguration( this.GetAttributeValue( AttributeKey.EnableLiveRefresh ).AsBoolean() );
            PopulateRoster();
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
        /// Handles the CheckedChanged event of the cbIncludeChildGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIncludeChildGroups_CheckedChanged( object sender, EventArgs e )
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

        #endregion

        #region Classes

        public class RosterConfiguration : RockDynamic
        {
            public int[] PickedGroupIds { get; set; }

            // just the first level of child groups (not all descendants)
            public bool IncludeChildGroups { get; set; }

            public int[] LocationIds { get; set; }

            public int[] ScheduleIds { get; set; }

            public bool DisplayRole { get; set; }

            public DateTime? OccurrenceDate { get; set; }

            public bool IsConfigured()
            {
                return PickedGroupIds != null && LocationIds != null && ScheduleIds != null;
            }
        }

        public class ScheduledIndividual : RockDynamic
        {
            public ScheduledAttendanceItemStatus ScheduledAttendanceItemStatus { get; set; }

            public Person Person { get; set; }

            public GroupMember GroupMember { get; set; }

            public List<GroupMember> GroupMembers { get; set; }

            public bool CurrentlyCheckedIn { get; set; }
        }

        private class OccurrenceRosterInfo : RockDynamic
        {
            public Group Group { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public DateTime? ScheduleDate { get; set; }

            public List<ScheduledIndividual> ScheduledIndividuals { get; set; }
        }

        #endregion
    }
}
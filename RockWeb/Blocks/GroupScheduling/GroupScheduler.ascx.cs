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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Group Scheduler" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows group schedules for groups and locations to be managed by a scheduler." )]

    [IntegerField(
        "Number of Weeks To Show",
        Description = "The number of weeks out that can scheduled.",
        IsRequired = true,
        DefaultIntegerValue = 6,
        Order = 0,
        Key = AttributeKey.FutureWeeksToShow )]
    [BooleanField(
        "Disallow Group Selection If Specified",
        Description = "When enabled it will hide the group picker if there is a GroupId in the query string.",
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.DisallowGroupSelectionIfSpecified )]
    [Rock.SystemGuid.BlockTypeGuid( "37D43C21-1A4D-4B13-9555-EF0B7304EB8A" )]
    public partial class GroupScheduler : RockBlock
    {
        /// <summary>
        ///
        /// </summary>
        protected class AttributeKey
        {
            /// <summary>
            /// The future weeks to show
            /// </summary>
            public const string FutureWeeksToShow = "FutureWeeksToShow";

            /// <summary>
            /// The disallow group selection if specified
            /// </summary>
            public const string DisallowGroupSelectionIfSpecified = "DisallowGroupSelectionIfSpecified";
        }

        #region PageParameterKeys

        /// <summary>
        ///
        /// </summary>
        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupIds = "GroupIds";
            public const string ShowChildGroups = "ShowChildGroups";

            public const string EndOfWeekDate = "EndOfWeekDate";

            public const string SelectAllSchedules = "SelectAllSchedules";
            public const string ScheduleId = "ScheduleId";

            // support either LocationId or LocationIds as a parameter
            public const string LocationId = "LocationId";
            public const string LocationIds = "LocationIds";

            public const string ResourceListSourceType = "ResourceListSourceType";
            public const string GroupMemberFilterType = "GroupMemberFilterType";
            public const string AlternateGroupId = "AlternateGroupId";
            public const string DataViewId = "DataViewId";
        }

        #endregion PageParameterKeys

        #region UserPreferenceKeys

        /// <summary>
        /// These end up with the same values as PageParameterKey values, but with different names for clarity
        /// </summary>
        private static class UserPreferenceKey
        {
            // the selected group Id (the active group column)
            public const string SelectedGroupId = PageParameterKey.GroupId;

            // the GroupIds that are selected in the GroupPicker
            public const string PickerGroupIds = PageParameterKey.GroupIds;

            // the value of the ShowChildGroups checkbox
            public const string ShowChildGroups = PageParameterKey.ShowChildGroups;

            public const string SelectedDate = PageParameterKey.EndOfWeekDate;

            public const string SelectAllSchedules = PageParameterKey.SelectAllSchedules;
            public const string SelectedIndividualScheduleId = PageParameterKey.ScheduleId;

            public const string PickedLocationIds = PageParameterKey.LocationIds;

            public const string SelectedResourceListSourceType = PageParameterKey.ResourceListSourceType;
            public const string GroupMemberFilterType = PageParameterKey.GroupMemberFilterType;
            public const string AlternateGroupId = PageParameterKey.AlternateGroupId;
            public const string DataViewId = PageParameterKey.DataViewId;
        }

        #endregion UserPreferenceKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Tell the browsers to not cache the page output.
            // This will help prevent an issue where Firefox sometimes uses the browser cached version
            // of the page when the 'Reload' button is clicked
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/GroupScheduler/groupScheduler.js" );
            RockPage.AddCSSLink( "~/Themes/Rock/Styles/group-scheduler.css", true );

            this.AddConfigurationUpdateTrigger( upnlContent );

            LoadDropDowns();

            btnCopyToClipboard.Visible = true;
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = string.Format(
@"new ClipboardJS('#{0}');
    $('#{0}').tooltip();
",
btnCopyToClipboard.ClientID );

            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );
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
                LoadFilterFromUserPreferencesOrURL();
                ApplyFilter();
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
                // var postbackArgument = 'update-preference|attendanceId:' + attendanceId|groupMemberId: + groupMemberId;
                var postbackArgsParts = postbackArgs.Split( '|' );
                if ( postbackArgsParts.Length == 3 && postbackArgsParts[0] == "update-preference" )
                {
                    var attendanceParameterParts = postbackArgsParts[1].Split( ':' );
                    int? attendanceId = null;

                    if ( attendanceParameterParts.Length == 2 && attendanceParameterParts[0] == "attendanceId" )
                    {
                        attendanceId = attendanceParameterParts[1].AsIntegerOrNull();
                    }

                    var groupMemberParameterParts = postbackArgsParts[2].Split( ':' );
                    int? groupMemberId = null;

                    if ( groupMemberParameterParts.Length == 2 && groupMemberParameterParts[0] == "groupMemberId" )
                    {
                        groupMemberId = groupMemberParameterParts[1].AsIntegerOrNull();
                    }

                    if ( attendanceId.HasValue && groupMemberId.HasValue )
                    {
                        UpdateGroupScheduleAssignmentPreference( attendanceId.Value, groupMemberId.Value );
                    }
                }
            }
        }

        #endregion Base Control Methods

        #region Methods

        private List<DateTime> _listedEndOfWeekDates = null;

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            _listedEndOfWeekDates = new List<DateTime>();
            int numOfWeeks = GetAttributeValue( AttributeKey.FutureWeeksToShow ).AsIntegerOrNull() ?? 6;
            var endOfWeekDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek );

            for ( int i = 0; i < numOfWeeks; i++ )
            {
                _listedEndOfWeekDates.Add( endOfWeekDate );
                endOfWeekDate = endOfWeekDate.AddDays( 7 );
            }

            rptWeekSelector.DataSource = _listedEndOfWeekDates;
            rptWeekSelector.DataBind();
        }

        /// <summary>
        /// Shows the scheduler.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        private void ShowScheduler( bool visible )
        {
            pnlScheduler.Visible = visible;
        }

        /// <summary>
        /// Updates the list of schedules for the listed groups
        /// </summary>
        private void UpdateScheduleList( List<Group> authorizedListedGroups )
        {
            if ( !authorizedListedGroups.Any() )
            {
                ShowScheduler( false );
                return;
            }

            pnlLocationFilter.Visible = false;
            pnlScheduleFilter.Visible = false;
            ShowScheduler( false );

            List<Schedule> groupSchedules = GetGroupSchedules( authorizedListedGroups );

            pnlLocationFilter.Visible = true;
            pnlScheduleFilter.Visible = true;

            ShowScheduler( true );

            // if a schedule is already selected, set it as the selected schedule (if it still exists for this group)
            var selectedScheduleId = hfSelectedScheduleId.Value.AsIntegerOrNull();

            var listedSchedules = groupSchedules.ToList();
            var hasGroupSchedules = listedSchedules.Any();

            // Add the 'All Schedules' option to the top of the schedule selector
            listedSchedules.Insert( 0, null );

            rptScheduleSelector.DataSource = listedSchedules;
            rptScheduleSelector.DataBind();
            lScheduleFilterText.Visible = true;

            if ( !hasGroupSchedules )
            {
                lScheduleFilterText.Visible = false;
            }
            else if ( selectedScheduleId.HasValue && listedSchedules.Any( s => s != null && s.Id == selectedScheduleId.Value ) )
            {
                hfSelectedScheduleId.Value = selectedScheduleId.ToString();
            }
            else
            {
                hfSelectedScheduleId.Value = "all";
            }
        }

        /// <summary>
        /// Gets the group schedules for the listed groups
        /// </summary>
        /// <param name="listedGroups">The listed groups.</param>
        /// <returns></returns>
        private static List<Schedule> GetGroupSchedules( List<Group> listedGroups )
        {
            if ( !listedGroups.Any() )
            {
                return new List<Schedule>();
            }

            RockContext rockContext = new RockContext();
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            var listedGroupIds = listedGroups.Select( a => a.Id ).ToArray();

            var groupLocationsQuery = groupLocationService.Queryable()
                .Where( a => listedGroupIds.Contains( a.GroupId ) );

            var groupSchedules = groupLocationsQuery
                .Where( gl => gl.Location.IsActive )
                .SelectMany( gl => gl.Schedules )
                .Where( s => s.IsActive )
                .AsNoTracking()
                .ToList()
                .DistinctBy( a => a.Guid )
                .ToList();

            groupSchedules = groupSchedules.OrderByOrderAndNextScheduledDateTime();

            return groupSchedules;
        }

        /// <summary>
        /// Gets the currently selected group.
        /// </summary>
        /// <returns></returns>
        private Group GetCurrentlySelectedGroup()
        {
            var groupId = hfSelectedGroupId.Value.AsIntegerOrNull();
            var rockContext = new RockContext();
            Group group = null;
            if ( groupId.HasValue )
            {
                group = new GroupService( rockContext ).GetNoTracking( groupId.Value );
            }

            return group;
        }

        /// <summary>
        /// Gets the authorized listed groups for which the current person has EDIT or SCHEDULE permission
        /// </summary>
        /// <returns></returns>
        private List<Group> GetAuthorizedListedGroups()
        {
            List<Group> listedGroups = GetListedGroups();

            var authorizedGroups = listedGroups.Where( g =>
            {
                var isAuthorized = g.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || g.IsAuthorized( Authorization.SCHEDULE, this.CurrentPerson );
                return isAuthorized;
            } ).ToList();

            var authorizedGroupCount = authorizedGroups.Count();
            var listedGroupCount = listedGroups.Count();

            return authorizedGroups;
        }

        /// <summary>
        /// Gets the listed groups, including ones that might not be authorized
        /// </summary>
        /// <returns></returns>
        private List<Group> GetListedGroups()
        {
            // get the selected listed groups (not including ones determined from IncludeChildGroups)
            var pickedGroupIds = gpPickedGroups.SelectedIds;

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            // if the ShowChildGroups option is enabled, also include those
            List<int> childGroupIds = new List<int>();
            if ( btnShowChildGroups.Attributes["show-child-groups"].AsBoolean() )
            {
                var childGroupIdQuery = groupService.Queryable()
                    .Where( a =>
                        a.IsActive
                        && !a.IsArchived
                        && a.ParentGroupId.HasValue
                        && a.GroupType.IsSchedulingEnabled
                        && !a.DisableScheduling );

                if ( pickedGroupIds.Count() == 1 )
                {
                    // if there is exactly one groupId we can avoid a 'Contains' (Contains has a small performance impact)
                    var pickedGroupId = pickedGroupIds[0];
                    childGroupIdQuery = childGroupIdQuery.Where( a => a.ParentGroupId == pickedGroupId );
                }
                else
                {
                    childGroupIdQuery = childGroupIdQuery.Where( a => pickedGroupIds.Contains( a.ParentGroupId.Value ) );
                }

                childGroupIds = childGroupIdQuery.Select( a => a.Id ).ToList();
            }

            List<int> groupIdsToQuery = pickedGroupIds.Union( childGroupIds ).ToList();

            // Include ParentGroup and GroupLocations so that these don't need to get lazy loaded
            var listedGroups = groupService
                .GetByIds( groupIdsToQuery )
                .Include( a => a.ParentGroup )
                .Include( a => a.GroupLocations )
                .AsNoTracking().ToList();

            return listedGroups;
        }

        /// <summary>
        /// Loads the filter from user preferences or the URL
        /// </summary>
        private void LoadFilterFromUserPreferencesOrURL()
        {
            DateTime selectedEndOfWeekDate =
                this.GetUrlSettingOrBlockUserPreference( PageParameterKey.EndOfWeekDate, UserPreferenceKey.SelectedDate ).AsDateTime()
                ?? RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek );

            if ( _listedEndOfWeekDates != null && _listedEndOfWeekDates.Contains( selectedEndOfWeekDate ) )
            {
                hfWeekSundayDate.Value = selectedEndOfWeekDate.ToISO8601DateString();
            }
            else
            {
                hfWeekSundayDate.Value = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ).ToISO8601DateString();
            }

            int? selectedGroupId = null;
            List<int> pickerGroupIds = new List<int>();
            bool showChildGroups;
            var preferences = GetBlockPersonPreferences();

            if ( this.PageParameter( PageParameterKey.GroupIds ).IsNotNullOrWhiteSpace() || this.PageParameter( PageParameterKey.GroupId ).IsNotNullOrWhiteSpace() )
            {
                // disable the groups picker if groupId(s) are specified in the URL
                var pageParameterGroupIds = ( this.PageParameter( PageParameterKey.GroupIds ) ?? string.Empty ).Split( ',' ).AsIntegerList();
                var pageParameterGroupId = this.PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                if ( pageParameterGroupId.HasValue )
                {
                    /*
                      SK - 11/09/2022
                      This will hide the group picker if there is a GroupId in the query string.
                      If there is a GroupIds query string parm. This will not lock the group selection.
                     */
                    gpPickedGroups.Enabled = !GetAttributeValue( AttributeKey.DisallowGroupSelectionIfSpecified ).AsBoolean() || pageParameterGroupIds.Any();
                    selectedGroupId = pageParameterGroupId.Value;
                    if ( !pageParameterGroupIds.Contains( selectedGroupId.Value ) )
                    {
                        pageParameterGroupIds.Add( selectedGroupId.Value );
                    }
                }

                pickerGroupIds = pageParameterGroupIds;
                btnShowChildGroups.Enabled = false;
                showChildGroups = this.PageParameter( PageParameterKey.ShowChildGroups ).AsBoolean();
            }
            else
            {
                selectedGroupId = preferences.GetValue( UserPreferenceKey.SelectedGroupId ).AsIntegerOrNull();
                showChildGroups = preferences.GetValue( UserPreferenceKey.ShowChildGroups ).AsBoolean();
            }

            var userPreferenceGroupIds = ( preferences.GetValue( UserPreferenceKey.PickerGroupIds ) ?? string.Empty ).Split( ',' ).AsIntegerList();
            if ( pickerGroupIds.Any() )
            {
                var pickerSelectedGroupIds = userPreferenceGroupIds.Where( a => pickerGroupIds.Contains( a ) ).ToList();
                if ( pickerSelectedGroupIds.Any() )
                {
                    pickerGroupIds = pickerSelectedGroupIds;
                }
                else
                {
                    pickerGroupIds = pickerGroupIds.Take( 1 ).ToList();
                }
            }                                       
            else
            {
                pickerGroupIds = userPreferenceGroupIds;
            }

            // if there is a 'GroupIds' parameter/userpreference, that defines what groups are shown.
            // However, only one group can be selected/active at a time
            gpPickedGroups.SetValues( pickerGroupIds );

            // if there is a 'GroupId' parameter/userpreference that will define active/selected group
            if ( selectedGroupId.HasValue )
            {
                // make sure a valid group was specified
                selectedGroupId = new GroupService( new RockContext() ).GetSelect( selectedGroupId.Value, s => ( int? ) s.Id );
            }

            SetStateForShowChildGroupsButton( showChildGroups );

            hfSelectedGroupId.Value = selectedGroupId.ToString();

            var authorizedListedGroups = GetAuthorizedListedGroups();

            UpdateScheduleList( authorizedListedGroups );
            bool selectAllSchedules;
            int? selectedIndividualScheduleId;
            if ( this.PageParameter( PageParameterKey.SelectAllSchedules ).IsNotNullOrWhiteSpace() || this.PageParameter( PageParameterKey.ScheduleId ).IsNotNullOrWhiteSpace() )
            {
                selectAllSchedules = this.PageParameter( PageParameterKey.SelectAllSchedules ).AsBoolean();
                selectedIndividualScheduleId = this.PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();
            }
            else
            {
                selectAllSchedules = preferences.GetValue( UserPreferenceKey.SelectAllSchedules ).AsBoolean();
                selectedIndividualScheduleId = preferences.GetValue( UserPreferenceKey.SelectedIndividualScheduleId ).AsIntegerOrNull();
            }

            if ( selectAllSchedules )
            {
                hfSelectedScheduleId.Value = "all";
            }
            else
            {
                hfSelectedScheduleId.Value = selectedIndividualScheduleId.ToString();
            }

            UpdateLocationList( authorizedListedGroups );

            if ( this.PageParameter( PageParameterKey.LocationId ).IsNotNullOrWhiteSpace() )
            {
                // if the URL has LocationId (singular) use that as the location
                hfPickedLocationIds.Value = this.PageParameter( PageParameterKey.LocationId );
            }
            else
            {
                // other, if the URL has LocationIds (plural), of there is a Locations user preference use that as the locations
                hfPickedLocationIds.Value = this.GetUrlSettingOrBlockUserPreference( PageParameterKey.LocationIds, UserPreferenceKey.PickedLocationIds );
            }

            SchedulerResourceGroupMemberFilterType groupMemberFilterType;
            var resourceListSourceType = this.GetUrlSettingOrBlockUserPreference( PageParameterKey.ResourceListSourceType, UserPreferenceKey.SelectedResourceListSourceType )
                .ConvertToEnumOrNull<GroupSchedulerResourceListSourceType>()
                ?? GroupSchedulerResourceListSourceType.GroupMembers;

            if ( resourceListSourceType == GroupSchedulerResourceListSourceType.GroupMatchingPreference )
            {
                groupMemberFilterType = SchedulerResourceGroupMemberFilterType.ShowMatchingPreference;
            }
            else
            {
                groupMemberFilterType = this.GetUrlSettingOrBlockUserPreference( PageParameterKey.GroupMemberFilterType, UserPreferenceKey.GroupMemberFilterType )
                    .ConvertToEnumOrNull<SchedulerResourceGroupMemberFilterType>()
                    ?? SchedulerResourceGroupMemberFilterType.ShowAllGroupMembers;
            }

            // if PageParameters have a DataViewId or AlternateGroupId, but didn't specify ResourceListSourceType,
            // assume they want use the DataView Source type when there is a DataViewId, or
            // AlternateGroup source type if AlternateGroupId is specified
            if ( this.PageParameter( PageParameterKey.ResourceListSourceType ).IsNullOrWhiteSpace() )
            {
                if ( this.PageParameter( PageParameterKey.DataViewId ).IsNotNullOrWhiteSpace() )
                {
                    resourceListSourceType = GroupSchedulerResourceListSourceType.DataView;
                }
                else if ( this.PageParameter( PageParameterKey.AlternateGroupId ).IsNotNullOrWhiteSpace() )
                {
                    resourceListSourceType = GroupSchedulerResourceListSourceType.AlternateGroup;
                }
            }

            // NOTE: if PageParameters or UserPreferences specify an invalid combination of ResourceSourceType and GroupId,
            // For example, an AlternateGroupId but when a Group has GroupRequirements,
            // ApplyFilter() will correct for it
            SetResourceListSourceType( resourceListSourceType, groupMemberFilterType );

            gpResourceListAlternateGroup.SetValue( this.GetUrlSettingOrBlockUserPreference( PageParameterKey.AlternateGroupId, UserPreferenceKey.AlternateGroupId ).AsIntegerOrNull() );

            var dataViewId = this.GetUrlSettingOrBlockUserPreference( PageParameterKey.DataViewId, UserPreferenceKey.DataViewId ).AsIntegerOrNull();
            if ( dataViewId.HasValue )
            {
                // make sure it is a Person DataView
                var dataView = new DataViewService( new RockContext() ).Get( dataViewId.Value );
                if ( dataView != null && dataView.EntityTypeId == EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON.AsGuid() ) )
                {
                    dvpResourceListDataView.SetValue( dataView );
                }
            }
        }

        /// <summary>
        /// Sets the state of the ShowChildGroups button.
        /// </summary>
        /// <param name="showChildGroups">if set to <c>true</c> [show child groups].</param>
        private void SetStateForShowChildGroupsButton( bool showChildGroups )
        {
            btnShowChildGroups.Attributes["show-child-groups"] = showChildGroups.ToJavaScriptValue();
            if ( showChildGroups )
            {
                btnShowChildGroups.Text = "<i class='fa fa-check-square'></i> Show Child Groups";
            }
            else
            {
                btnShowChildGroups.Text = "<i class='fa fa-square-o'></i> Show Child Groups";
            }
        }

        /// <summary>
        /// Gets the scheduling enabled group ids.
        /// </summary>
        /// <param name="authorizedGroupIds">The authorized group ids.</param>
        /// <returns></returns>
        private static List<int> GetSchedulingEnabledGroupIds( List<int> authorizedGroupIds )
        {
            var schedulingEnabledGroupIds =
             new GroupService( new RockContext() )
                .GetByIds( authorizedGroupIds )
                .Where( a => a.GroupType.IsSchedulingEnabled && !a.DisableScheduling )
                .Select( a => a.Id )
                .ToList();

            return schedulingEnabledGroupIds;
        }

        /// <summary>
        /// Gets the URL setting (if there is one) or block user preference.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetUrlSettingOrBlockUserPreference( string pageParameterKey, string userPreferenceKey )
        {
            string setting = Request.QueryString[pageParameterKey];
            if ( setting != null )
            {
                return setting;
            }

            var preferences = GetBlockPersonPreferences();

            return preferences.GetValue( userPreferenceKey );
        }

        /// <summary>
        /// Saves the user preferences and updates the resource list and locations based on the filter
        /// </summary>
        private void ApplyFilter()
        {
            upnlContent.Update();
            var authorizedListedGroups = this.GetAuthorizedListedGroups();

            OccurrenceDisplayMode occurrenceDisplayMode = GetOccurrenceDisplayMode( authorizedListedGroups );

            pnlSendNowMultiGroupMode.Visible = occurrenceDisplayMode == OccurrenceDisplayMode.MultiGroup;
            btnSendNowSingleGroupMode.Visible = occurrenceDisplayMode == OccurrenceDisplayMode.SingleGroup;
            pnlAutoScheduleMultiGroupMode.Visible = occurrenceDisplayMode == OccurrenceDisplayMode.MultiGroup;
            btnAutoScheduleSingleGroupMode.Visible = occurrenceDisplayMode == OccurrenceDisplayMode.SingleGroup;

            List<int> listedGroupIds = authorizedListedGroups.Select( a => a.Id ).ToList();

            int? selectedGroupId = null;
            var selectedGroup = this.GetCurrentlySelectedGroup();
            if ( selectedGroup != null )
            {
                selectedGroupId = selectedGroup.Id;
            }

            List<int> scheduleIds = GetSelectedScheduleIds( authorizedListedGroups );

            var endOfWeekDate = hfWeekSundayDate.Value.AsDateTime() ?? RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek );

            lWeekFilterText.Text = string.Format( "<i class='fa fa-calendar-alt'></i> Week: {0}", endOfWeekDate.ToShortDateString() );

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.SelectedGroupId, selectedGroupId.ToString() );
            preferences.SetValue( UserPreferenceKey.PickerGroupIds, gpPickedGroups.SelectedIds.ToList().AsDelimited( "," ) );
            preferences.SetValue( UserPreferenceKey.ShowChildGroups, btnShowChildGroups.Attributes["show-child-groups"] );

            preferences.SetValue( UserPreferenceKey.SelectedDate, endOfWeekDate.ToISO8601DateString() );

            preferences.SetValue( UserPreferenceKey.PickedLocationIds, hfPickedLocationIds.Value );
            bool selectAllSchedules = hfSelectedScheduleId.Value.AsIntegerOrNull() == null;
            int? selectedScheduleId = hfSelectedScheduleId.Value.AsIntegerOrNull();
            preferences.SetValue( UserPreferenceKey.SelectAllSchedules, selectAllSchedules.ToString() );
            preferences.SetValue( UserPreferenceKey.SelectedIndividualScheduleId, selectedScheduleId.ToString() );

            var rockContext = new RockContext();

            // only show the Show Child Groups option if
            // the is only one group selected, and that group has child ground
            var pickerGroupIds = gpPickedGroups.SelectedIds.ToList();

            bool showChildGroupsCheckbox = pickerGroupIds.Count == 1 && new GroupService( rockContext ).Queryable().Where( a => a.ParentGroupId.HasValue && pickerGroupIds.Contains( a.ParentGroupId.Value ) ).Any();
            btnShowChildGroups.Visible = showChildGroupsCheckbox;

            Schedule selectedSchedule = null;
            if ( selectedScheduleId.HasValue )
            {
                selectedSchedule = new ScheduleService( rockContext ).GetNoTracking( selectedScheduleId.Value );
            }

            string selectedSchedulesText;

            if ( selectAllSchedules || selectedSchedule == null )
            {
                selectedSchedulesText = "All Schedules";
            }
            else
            {
                if ( selectedSchedule.Name.IsNotNullOrWhiteSpace() )
                {
                    selectedSchedulesText = selectedSchedule.Name;
                }
                else
                {
                    selectedSchedulesText = selectedSchedule.ToFriendlyScheduleText();
                }
            }

            lScheduleFilterText.Text = string.Format( "<i class='fa fa-clock-o'></i>  {0}", selectedSchedulesText );

            var resourceListSourceType = ( GroupSchedulerResourceListSourceType ) hfSchedulerResourceListSourceType.Value.AsInteger();
            var groupMemberFilterType = ( SchedulerResourceGroupMemberFilterType ) hfResourceGroupMemberFilterType.Value.AsInteger();

            List<GroupSchedulerResourceListSourceType> schedulerResourceListSourceTypes = typeof( GroupSchedulerResourceListSourceType ).GetOrderedValues<GroupSchedulerResourceListSourceType>().ToList();

            if ( selectedGroup != null && selectedGroup.SchedulingMustMeetRequirements )
            {
                var sameGroupSourceTypes = new GroupSchedulerResourceListSourceType[]
                {
                    GroupSchedulerResourceListSourceType.GroupMembers,
                    GroupSchedulerResourceListSourceType.GroupMatchingPreference,
                    GroupSchedulerResourceListSourceType.GroupMatchingAssignment
                };

                // if SchedulingMustMeetRequirements
                // -- don't show options for other groups or people
                // -- don't show the option to add an individual person person
                // this is because people from other groups wouldn't meet scheduling requirements (since they aren't in the same group as the Attendance Occurrence)
                schedulerResourceListSourceTypes = sameGroupSourceTypes.ToList();
                pnlAddPerson.Visible = false;
                ppAddPerson.Visible = false;

                if ( !sameGroupSourceTypes.Contains( resourceListSourceType ) )
                {
                    resourceListSourceType = GroupSchedulerResourceListSourceType.GroupMembers;
                    SetResourceListSourceType( resourceListSourceType, groupMemberFilterType );
                }
            }
            else
            {
                if ( selectedGroup == null || !selectedGroup.ParentGroupId.HasValue )
                {
                    schedulerResourceListSourceTypes = schedulerResourceListSourceTypes.Where( a => a != GroupSchedulerResourceListSourceType.ParentGroup ).ToList();
                }

                pnlAddPerson.Visible = true;
                ppAddPerson.Visible = true;
            }

            rptSchedulerResourceListSourceType.DataSource = schedulerResourceListSourceTypes;
            rptSchedulerResourceListSourceType.DataBind();

            preferences.SetValue( UserPreferenceKey.SelectedResourceListSourceType, resourceListSourceType.ToString() );
            preferences.SetValue( UserPreferenceKey.GroupMemberFilterType, groupMemberFilterType.ToString() );
            preferences.SetValue( UserPreferenceKey.AlternateGroupId, gpResourceListAlternateGroup.SelectedValue );
            preferences.SetValue( UserPreferenceKey.DataViewId, dvpResourceListDataView.SelectedValue );

            preferences.Save();

            pnlResourceFilterAlternateGroup.Visible = resourceListSourceType == GroupSchedulerResourceListSourceType.AlternateGroup;
            pnlResourceFilterDataView.Visible = resourceListSourceType == GroupSchedulerResourceListSourceType.DataView;

            var listedLocations = GetListedLocations( authorizedListedGroups, scheduleIds );
            var pickedLocationIds = hfPickedLocationIds.Value.Split( ',' ).AsIntegerList();
            pickedLocationIds = pickedLocationIds.Where( p => listedLocations.Any( a => a.Id == p ) ).ToList();
            List<int> selectedLocationIds;
            if ( pickedLocationIds.Any() )
            {
                selectedLocationIds = pickedLocationIds;
            }
            else
            {
                selectedLocationIds = listedLocations.Select( a => a.Id ).ToList();
            }

            // fix up the list of selectedGroupLocationIds to only include ones that are listed
            selectedLocationIds = selectedLocationIds.Where( a => listedLocations.Any( l => l.Id == a ) ).ToList();

            List<Location> selectedLocations = new LocationService( rockContext )
                .GetByIds( selectedLocationIds )
                .OrderBy( a => a.Name )
                .AsNoTracking()
                .ToList();

            bool selectAllLocations = false;

            string selectedLocationFilterText;

            // if no locations are selected(because 'all' was selected), or if all the listed locations are selected, then all locations will be selected
            var allLocationsSelected = !pickedLocationIds.Any() || listedLocations.All( sl => selectedLocations.Any( ll => ll.Id == sl.Id ) );

            if ( allLocationsSelected )
            {
                selectedLocationFilterText = "All Locations";
                selectedLocationIds = listedLocations.Where( a => a != null ).Select( a => a.Id ).ToList();
                selectAllLocations = true;
            }
            else if ( selectedLocations.Count() == 1 )
            {
                selectedLocationFilterText = selectedLocations.First().ToString( true );
            }
            else
            {
                selectedLocationFilterText = string.Format(
                    "<span title='{0}'>{1} Locations</span>",
                    selectedLocations.Select( a => a.ToString( true ) ).ToList().AsDelimited( ", " ).EncodeHtml(),
                    selectedLocations.Count() );
            }

            lSelectedLocationFilterText.Text = string.Format( "<i class='fa fa-building'></i> {0}", selectedLocationFilterText );

            var locationButtons = rptLocationSelector.ControlsOfTypeRecursive<LinkButton>().ToList();
            var locationService = new LocationService( rockContext );
            foreach ( var locationButton in locationButtons )
            {
                var locationId = locationButton.CommandArgument.AsIntegerOrNull();
                if ( locationId.HasValue )
                {
                    var location = listedLocations.Where( a => a != null ).Where( a => a.Id == locationId.Value ).FirstOrDefault();
                    if ( location != null )
                    {
                        if ( selectedLocationIds.Contains( locationId.Value ) )
                        {
                            locationButton.Text = string.Format( "<i class='fa fa-check'></i> {0}", location.ToString( true ).EncodeHtml() );
                        }
                        else
                        {
                            locationButton.Text = string.Format( " {0}", location.ToString( true ) );
                        }
                    }
                }
                else
                {
                    // all locations button
                    if ( selectAllLocations )
                    {
                        locationButton.Text = "<i class='fa fa-check'></i> All Locations";
                    }
                    else
                    {
                        locationButton.Text = " All Locations";
                    }
                }
            }

            bool filterIsValid = ValidateFilter( authorizedListedGroups, selectedGroup, scheduleIds, listedLocations, selectedLocationIds );

            ShowScheduler( filterIsValid );

            pnlLocationFilter.Visible = authorizedListedGroups.Any();
            pnlScheduleFilter.Visible = authorizedListedGroups.Any();

            if ( filterIsValid )
            {
                InitResourceList( authorizedListedGroups );
                BindAttendanceOccurrences( authorizedListedGroups, selectedLocationIds );
            }

            // Create URL for selected settings
            // Set the pageparameters from UserPreferences since they end up with the same values

            // create a new pageReference using the CurrentPageReference values so that we don't end up modifying CurrentPageReference
            var pageReference = new PageReference( CurrentPageReference );
            var pagePageParameterKeys = typeof( PageParameterKey ).GetFields().Select( a => a.Name ).ToList();

            foreach ( var pagePageParameterKey in pagePageParameterKeys )
            {
                pageReference.Parameters.AddOrReplace( pagePageParameterKey, preferences.GetValue( pagePageParameterKey ) );
            }

            Uri requestUri = new Uri( Request.UrlProxySafe().ToString() );
            var linkUrl = requestUri.GetLeftPart( UriPartial.Authority ) + pageReference.BuildUrl();
            btnCopyToClipboard.Attributes["data-clipboard-text"] = linkUrl;
            btnCopyToClipboard.Disabled = false;
        }

        /// <summary>
        /// Gets the occurrence display mode based on the number of groups that we are showing
        /// </summary>
        /// <param name="authorizedListedGroups">The authorized listed groups.</param>
        /// <returns></returns>
        private static OccurrenceDisplayMode GetOccurrenceDisplayMode( List<Group> authorizedListedGroups )
        {
            OccurrenceDisplayMode occurrenceDisplayMode;
            if ( authorizedListedGroups.Count > 1 )
            {
                occurrenceDisplayMode = OccurrenceDisplayMode.MultiGroup;
            }
            else
            {
                occurrenceDisplayMode = OccurrenceDisplayMode.SingleGroup;
            }

            return occurrenceDisplayMode;
        }

        /// <summary>
        /// Validates the filter and shows filter warnings if the filter is not valid
        /// </summary>
        /// <param name="authorizedListedGroups">The authorized listed groups.</param>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <param name="listedLocations">The listed locations.</param>
        /// <param name="selectedGroupLocationIds">The selected group location ids.</param>
        /// <returns></returns>
        private bool ValidateFilter( List<Group> authorizedListedGroups, Group selectedGroup, List<int> scheduleIds, List<Location> listedLocations, List<int> selectedLocationIds )
        {
            bool filterIsValid = false;

            var pickedGroupCount = gpPickedGroups.SelectedIds.Count();

            var hasMultipleGroupsSelected = authorizedListedGroups.Count() > 1;
            var authorizedGroupsWarning = string.Empty;

            if ( pickedGroupCount > 0 && authorizedListedGroups.Count == 0 )
            {
                if ( hasMultipleGroupsSelected )
                {
                    authorizedGroupsWarning = "You're not authorized to schedule resources for these groups";
                }
                else
                {
                    authorizedGroupsWarning = "You're not authorized to schedule resources for this group";
                }
            }
            else if ( pickedGroupCount > 0 && authorizedListedGroups.Count < pickedGroupCount )
            {
                authorizedGroupsWarning = "You're not authorized to schedule resources for some of these groups";
            }

            nbAuthorizedGroupsWarning.Visible = authorizedGroupsWarning.IsNotNullOrWhiteSpace();
            nbAuthorizedGroupsWarning.Text = authorizedGroupsWarning;

            string filterMessage;
            NotificationBoxType filterNotificationBoxType = NotificationBoxType.Warning;
            if ( pickedGroupCount == 0 )
            {
                filterMessage = "Please select at least one group";
                filterNotificationBoxType = NotificationBoxType.Info;
            }
            else if ( !scheduleIds.Any() )
            {
                // check schedules (before locations) since you can't have locations if there aren't any schedule
                if ( hasMultipleGroupsSelected )
                {
                    filterMessage = "The selected groups do not have any schedules";
                }
                else
                {
                    filterMessage = "The selected group does not have any schedules";
                }
            }
            else if ( !listedLocations.Any() )
            {
                // if there are schedules, but no locations, warn about lack of schedules
                if ( hasMultipleGroupsSelected )
                {
                    filterMessage = "The selected groups do not have any locations";
                }
                else
                {
                    filterMessage = "The selected group does not have any locations";
                }
            }
            else if ( !selectedLocationIds.Any() )
            {
                filterMessage = "Please select at least one location";
                filterNotificationBoxType = NotificationBoxType.Info;
            }
            else
            {
                filterMessage = string.Empty;
                filterIsValid = true;
            }

            nbFilterMessage.Visible = !filterIsValid;
            nbFilterMessage.Text = filterMessage;
            nbFilterMessage.NotificationBoxType = filterNotificationBoxType;
            return filterIsValid;
        }

        /// <summary>
        /// Gets the selected schedule ids.
        /// </summary>
        /// <param name="authorizedListedGroups">The authorized listed groups.</param>
        /// <returns></returns>
        private List<int> GetSelectedScheduleIds( List<Group> authorizedListedGroups )
        {
            var selectedScheduleId = hfSelectedScheduleId.Value.AsIntegerOrNull();
            if ( selectedScheduleId.HasValue )
            {
                var selectedScheduleIds = new List<int>();
                selectedScheduleIds.Add( selectedScheduleId.Value );
                return selectedScheduleIds;
            }

            var scheduleList = GetGroupSchedules( authorizedListedGroups );

            if ( scheduleList != null )
            {
                return scheduleList.Where( a => a != null ).Select( a => a.Id ).ToList();
            }
            else
            {
                return new List<int>();
            }
        }

        /// <summary>
        /// Updates the list of locations for the selected groups
        /// </summary>
        private void UpdateLocationList( List<Group> authorizedListedGroups )
        {
            if ( !authorizedListedGroups.Any() )
            {
                ShowScheduler( false );
                return;
            }

            if ( authorizedListedGroups.Any() )
            {
                ShowScheduler( true );
                List<int> scheduleIds = GetSelectedScheduleIds( authorizedListedGroups );

                var listedLocations = GetListedLocations( authorizedListedGroups, scheduleIds );

                if ( listedLocations.Any() )
                {
                    listedLocations.Insert( 0, null );
                    lSelectedLocationFilterText.Visible = true;
                }
                else
                {
                    lSelectedLocationFilterText.Visible = false;
                }

                rptLocationSelector.DataSource = listedLocations;
                rptLocationSelector.DataBind();
            }
        }

        /// <summary>
        /// Gets the listed locations.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="scheduleIds">The schedule ids.</param>
        /// <returns></returns>
        private List<Location> GetListedLocations( List<Group> listedGroups, List<int> scheduleIds )
        {
            var rockContext = new RockContext();
            var listedGroupIds = listedGroups.Select( a => a.Id ).ToList();

            var groupLocationsQuery = new GroupLocationService( rockContext ).Queryable()
                .Where( gl =>
                    listedGroupIds.Contains( gl.GroupId ) &&
                    gl.Schedules.Any( s => scheduleIds.Contains( s.Id ) ) &&
                    gl.Location.IsActive )
                .OrderBy( a => new { a.Order, a.Location.Name } )
                .AsNoTracking();

            var listedLocations = groupLocationsQuery.Select( a => a.Location ).ToList().DistinctBy( a => a.Id ).ToList();

            return listedLocations;
        }

        /// <summary>
        /// Set the Resource List hidden fields which groupScheduler.js uses to populate the Resource List
        /// </summary>
        private void InitResourceList( List<Group> authorizedListedGroups )
        {
            int groupId = hfSelectedGroupId.Value.AsInteger();
            int? resourceGroupId = null;
            int? resourceDataViewId = null;

            List<int> scheduleIds = GetSelectedScheduleIds( authorizedListedGroups );

            hfResourceAdditionalPersonIds.Value = string.Empty;

            var resourceListSourceType = hfSchedulerResourceListSourceType.Value.ConvertToEnum<GroupSchedulerResourceListSourceType>();
            switch ( resourceListSourceType )
            {
                case GroupSchedulerResourceListSourceType.GroupMembers:
                case GroupSchedulerResourceListSourceType.GroupMatchingPreference:
                case GroupSchedulerResourceListSourceType.GroupMatchingAssignment:
                    {
                        resourceGroupId = groupId;
                        break;
                    }

                case GroupSchedulerResourceListSourceType.AlternateGroup:
                    {
                        resourceGroupId = gpResourceListAlternateGroup.SelectedValue.AsInteger();
                        break;
                    }

                case GroupSchedulerResourceListSourceType.ParentGroup:
                    {
                        var rockContext = new RockContext();
                        resourceGroupId = new GroupService( rockContext ).GetSelect( groupId, s => s.ParentGroupId );
                        break;
                    }

                case GroupSchedulerResourceListSourceType.DataView:
                    {
                        resourceDataViewId = dvpResourceListDataView.SelectedValue.AsInteger();
                        break;
                    }
            }

            hfOccurrenceGroupId.Value = groupId.ToString();
            hfOccurrenceScheduleIds.Value = scheduleIds.AsDelimited( "," );
            hfOccurrenceSundayDate.Value = ( hfWeekSundayDate.Value.AsDateTime() ?? RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ) ).ToISO8601DateString();

            hfResourceGroupId.Value = resourceGroupId.ToString();
            hfResourceDataViewId.Value = resourceDataViewId.ToString();
            hfResourceAdditionalPersonIds.Value = string.Empty;
        }

        /// <summary>
        /// Binds the Attendance Occurrences ( Which shows the Location for the Attendance Occurrence for the selected Group + DateTime + Location ).
        /// groupScheduler.js will populate these with the scheduled resources
        /// </summary>
        private void BindAttendanceOccurrences( List<Group> authorizedListedGroups, List<int> selectedLocationIds )
        {
            var occurrenceDateEndRange = hfOccurrenceSundayDate.Value.AsDateTime().Value.Date;
            var occurrenceDateStartRange = occurrenceDateEndRange.AddDays( -6 );

            // make sure we don't let them schedule dates in the past
            if ( occurrenceDateStartRange <= RockDateTime.Today )
            {
                occurrenceDateStartRange = RockDateTime.Today;
            }

            var scheduleIds = GetSelectedScheduleIds( authorizedListedGroups );

            var occurrenceSchedules = new ScheduleService( new RockContext() ).GetByIds( scheduleIds ).AsNoTracking().ToList();

            if ( !occurrenceSchedules.Any() )
            {
                pnlAutoScheduleMultiGroupMode.Visible = false;
                btnAutoScheduleSingleGroupMode.Visible = false;
                return;
            }

            var scheduleOccurrenceDatesLookupByScheduleId = new Dictionary<int, List<DateTime>>();

            foreach ( var occurrenceSchedule in occurrenceSchedules )
            {
                // get all the occurrences for the selected week for the selected schedule.
                // we only want create occurrences start times for this specific schedule
                // Note that it could be more than once a week if it is a daily scheduled, or it might not be in the selected week if it is every 2 weeks, etc
                var scheduleOccurrenceDates = occurrenceSchedule
                    .GetScheduledStartTimes( occurrenceDateStartRange, occurrenceDateEndRange.AddDays( 1 ) )
                    .Select( a => a.Date )
                    .Distinct()
                    .ToList();

                scheduleOccurrenceDatesLookupByScheduleId.Add( occurrenceSchedule.Id, scheduleOccurrenceDates );
            }

            var groupIds = authorizedListedGroups.Select( a => a.Id ).ToList();

            List<DateTime> occurrenceDatesForAllSchedules = new List<DateTime>();

            // create a lookup of GroupLocations for each GroupId
            // only include GroupLocations that have Schedules (since we can't schedule somebody for a group location that doesn't have any schedules)
            var groupLocationQuery = new GroupLocationService( new RockContext() ).Queryable()
                    .Where( a =>
                        groupIds.Contains( a.GroupId )
                        && selectedLocationIds.Contains( a.LocationId )
                        && a.Schedules.Any() );

            var groupGroupLocationIdsLookupByGroupId = groupLocationQuery
                    .Select( a => new { a.GroupId, GroupLocationId = a.Id } )
                    .ToList()
                    .GroupBy( a => a.GroupId )
                    .ToDictionary( k => k.Key, v => v.Select( s => s.GroupLocationId ).ToList() );

            var groupLocationSchedules = groupLocationQuery.Select( a => new
            {
                GroupLocationId = a.Id,
                GroupId = a.GroupId,
                LocationId = a.LocationId,
                GroupLocationScheduleIds = a.Schedules.Select( s => s.Id ).ToList()
            } ).ToList();

            List<int> selectedGroupLocationIds = new List<int>();

            var groupIdsWithLocations = groupGroupLocationIdsLookupByGroupId.Where( a => a.Value.Any() ).Select( a => a.Key ).ToList();

            foreach ( var groupId in groupIdsWithLocations )
            {
                var groupGroupLocationIds = groupGroupLocationIdsLookupByGroupId.GetValueOrNull( groupId ) ?? new List<int>();

                using ( var missingAttendanceOccurrenceRockContext = new RockContext() )
                {
                    var missingAttendanceOccurrenceOccurrenceService = new AttendanceOccurrenceService( missingAttendanceOccurrenceRockContext );

                    selectedGroupLocationIds.AddRange( groupGroupLocationIds );

                    foreach ( var occurrenceSchedule in occurrenceSchedules )
                    {
                        // get all the occurrences for the selected week for the selected schedule.
                        // we only want create occurrences start times for this specific schedule
                        // Note that it could be more than once a week if it is a daily scheduled, or it might not be in the selected week if it is every 2 weeks, etc
                        var scheduleOccurrenceDates = scheduleOccurrenceDatesLookupByScheduleId.GetValueOrNull( occurrenceSchedule.Id ) ?? new List<DateTime>();

                        // only include groupLocations if the group has this schedule associated with the GroupLocation
                        var scheduleGroupLocationIds = groupLocationSchedules.Where( a => a.GroupLocationScheduleIds.Contains( occurrenceSchedule.Id ) ).Select( a => a.GroupLocationId ).Distinct().ToList();

                        if ( scheduleGroupLocationIds.Any() )
                        {
                            List<AttendanceOccurrence> missingAttendanceOccurrenceListForSchedule =
                                missingAttendanceOccurrenceOccurrenceService.CreateMissingAttendanceOccurrences( scheduleOccurrenceDates, occurrenceSchedule.Id, scheduleGroupLocationIds );

                            if ( missingAttendanceOccurrenceListForSchedule.Any() )
                            {
                                missingAttendanceOccurrenceOccurrenceService.AddRange( missingAttendanceOccurrenceListForSchedule );
                                missingAttendanceOccurrenceRockContext.SaveChanges();
                            }
                        }

                        occurrenceDatesForAllSchedules.AddRange( scheduleOccurrenceDates );
                    }
                }
            }

            var occurrenceDateList = occurrenceDatesForAllSchedules.Select( a => a.Date ).Distinct().ToList();
            var rockContext = new RockContext();

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

            IQueryable<AttendanceOccurrenceService.AttendanceOccurrenceGroupLocationScheduleConfigJoinResult> attendanceOccurrenceGroupLocationScheduleConfigQuery
                = attendanceOccurrenceService.AttendanceOccurrenceGroupLocationScheduleConfigJoinQuery( occurrenceDateList, scheduleIds, selectedGroupLocationIds );

            OccurrenceDisplayMode occurrenceDisplayMode = GetOccurrenceDisplayMode( authorizedListedGroups );

            var attendanceOccurrencesList = attendanceOccurrenceGroupLocationScheduleConfigQuery.AsNoTracking()
                .Select( a => new AttendanceOccurrenceRowItem
                {
                    OccurrenceDisplayMode = occurrenceDisplayMode,
                    LocationName = a.AttendanceOccurrence.Location.Name,
                    GroupLocationOrder = a.GroupLocation.Order,
                    LocationId = a.AttendanceOccurrence.LocationId,
                    Group = a.AttendanceOccurrence.Group,
                    Schedule = a.AttendanceOccurrence.Schedule,
                    OccurrenceDate = a.AttendanceOccurrence.OccurrenceDate,
                    AttendanceOccurrenceId = a.AttendanceOccurrence.Id,
                    HasAttendees = a.AttendanceOccurrence.Attendees.Any(),
                    CapacityInfo = new CapacityInfo
                    {
                        MinimumCapacity = a.GroupLocationScheduleConfig.MinimumCapacity,
                        DesiredCapacity = a.GroupLocationScheduleConfig.DesiredCapacity,
                        MaximumCapacity = a.GroupLocationScheduleConfig.MaximumCapacity
                    }
                } ).ToList();

            var selectedGroupId = hfSelectedGroupId.Value.AsInteger();

            var attendanceOccurrencesOrderedList = attendanceOccurrencesList.OrderBy( a => a.ScheduledDateTime ).ThenBy( a => a.GroupLocationOrder ).ThenBy( a => a.LocationName ).ToList();

            // limit to occurrences where the GroupLocation has the schedule configured
            attendanceOccurrencesOrderedList = attendanceOccurrencesOrderedList
                .Where( a =>
                 {
                     // only include this occurrence if Schedule is configured for this Group Location
                     if ( groupLocationSchedules.Any( x => x.GroupId == a.Group.Id && x.LocationId == a.LocationId && x.GroupLocationScheduleIds.Contains( a.Schedule.Id ) ) )
                     {
                         return true;
                     }
                     else
                     {
                         if ( a.HasAttendees )
                         {
                             // if the schedule isn't configured for this group's location, but it has attendees, show it
                             return true;
                         }
                         else
                         {
                             return false;
                         }
                     }
                 } ).ToList();

            // if there are any people that signed up with no location preference, add the to a special list of "No Location Preference" occurrences to the top of the list
            var unassignedLocationOccurrenceList = attendanceOccurrenceService.Queryable()
                .Where( a => occurrenceDateList.Contains( a.OccurrenceDate )
                    && a.ScheduleId.HasValue
                    && scheduleIds.Contains( a.ScheduleId.Value )
                    && a.GroupId == selectedGroupId
                    && a.LocationId.HasValue == false )
                .Where( a => a.Attendees.Any( x => x.RequestedToAttend == true || x.ScheduledToAttend == true ) )
                .Select( a => new AttendanceOccurrenceRowItem
                {
                    OccurrenceDisplayMode = occurrenceDisplayMode,
                    LocationName = "No Location Preference",
                    GroupLocationOrder = 0,
                    LocationId = null,
                    Group = a.Group,
                    Schedule = a.Schedule,
                    OccurrenceDate = a.OccurrenceDate,
                    AttendanceOccurrenceId = a.Id,
                    CapacityInfo = new CapacityInfo()
                } )
                .ToList()
                .OrderBy( a => a.ScheduledDateTime )
                .ToList();

            attendanceOccurrencesOrderedList.InsertRange( 0, unassignedLocationOccurrenceList );

            // if selected group isn't in the list of occurrences, change the selected group to the first one
            if ( !attendanceOccurrencesOrderedList.Any( a => a.Group != null && a.Group.Id == selectedGroupId ) )
            {
                selectedGroupId = attendanceOccurrencesOrderedList.Where( a => a.Group != null ).Select( a => a.Group.Id ).FirstOrDefault();
                hfSelectedGroupId.Value = selectedGroupId.ToString();
                InitResourceList( authorizedListedGroups );
            }

            List<OccurrenceColumnItem> occurrenceColumnDataList;

            if ( occurrenceDisplayMode == OccurrenceDisplayMode.MultiGroup )
            {
                // sort the occurrenceColumns so the selected Group is in the first column, then order by Group.Order/Name
                occurrenceColumnDataList = attendanceOccurrencesOrderedList
                    .Where( a => a.ScheduledDateTime.HasValue )
                    .GroupBy( a => a.Group.Id )
                    .Select( a =>
                    {
                        // we are grouping by GroupId but we need Group.
                        // since all of these will have the same Group, we can just grab the first one
                        var group = a.Select( s => s.Group ).FirstOrDefault();

                        var item = new OccurrenceColumnItem
                        {
                            OccurrenceDisplayMode = occurrenceDisplayMode,
                            Group = group,
                            Schedule = ( Schedule ) null,
                            OccurrenceDate = ( DateTime? ) null,
                            AttendanceOccurrenceItems = a.ToList()
                        };

                        return item;
                    } )
                    .OrderBy( a => a.Group.Id == selectedGroupId ? 0 : 1 )
                    .ThenBy( a => a.Group.Order ).ThenBy( a => a.Group.Name ).ToList();
            }
            else
            {
                // single group mode. so the group will be the same for all items in occurrenceColumnDataList
                var group = authorizedListedGroups.FirstOrDefault();

                occurrenceColumnDataList = attendanceOccurrencesOrderedList
                    .Where( a => a.ScheduledDateTime.HasValue )
                    .GroupBy( a => new { ScheduleId = a.Schedule.Id, a.OccurrenceDate } )
                    .Select( a =>
                    {
                        // we are grouping by ScheduleId but we need Schedule.
                        // since all of these will have the same Schedule, we can just grab the first one
                        var schedule = a.Select( s => s.Schedule ).FirstOrDefault();

                        var item = new OccurrenceColumnItem
                        {
                            OccurrenceDisplayMode = occurrenceDisplayMode,
                            Group = group,
                            Schedule = schedule,
                            OccurrenceDate = a.Key.OccurrenceDate,
                            AttendanceOccurrenceItems = a.ToList()
                        };

                        return item;
                    } )
                    .OrderBy( a => a.OccurrenceDate )
                    .ThenBy( a => a.Schedule.Order )
                    .ThenBy( a => a.Schedule.GetNextStartDateTime( occurrenceDateStartRange ) )
                    .ToList();
            }

            hfDisplayedOccurrenceIds.Value = attendanceOccurrencesOrderedList.Select( a => a.AttendanceOccurrenceId ).ToList().AsDelimited( "," );
            rptOccurrenceColumns.DataSource = occurrenceColumnDataList;
            rptOccurrenceColumns.DataBind();
        }

        /// <summary>
        ///
        /// </summary>
        private class CapacityInfo
        {
            /// <summary>
            /// Gets or sets the minimum capacity.
            /// </summary>
            /// <value>
            /// The minimum capacity.
            /// </value>
            public int? MinimumCapacity { get; set; }

            /// <summary>
            /// Gets or sets the desired capacity.
            /// </summary>
            /// <value>
            /// The desired capacity.
            /// </value>
            public int? DesiredCapacity { get; set; }

            /// <summary>
            /// Gets or sets the maximum capacity.
            /// </summary>
            /// <value>
            /// The maximum capacity.
            /// </value>
            public int? MaximumCapacity { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public enum OccurrenceDisplayMode
        {
            /// <summary>
            /// Single Group, so each column is an Schedule/Day
            /// </summary>
            SingleGroup,

            /// <summary>
            ///  Multi Group, so each column is a Group
            /// </summary>
            MultiGroup
        }

        [System.Diagnostics.DebuggerDisplay( "{Group} {Schedule} {OccurrenceDate}" )]
        private class OccurrenceColumnItem
        {
            /// <summary>
            /// Gets or sets the occurrence display mode.
            /// </summary>
            /// <value>
            /// The occurrence display mode.
            /// </value>
            public OccurrenceDisplayMode OccurrenceDisplayMode { get; set; }

            /// <summary>
            /// Gets or sets the group (when in <see cref="OccurrenceDisplayMode.MultiGroup"/> )
            /// </summary>
            /// <value>
            /// The group.
            /// </value>
            public Group Group { get; set; }

            /// <summary>
            /// Gets or sets the schedule (when in <see cref="OccurrenceDisplayMode.ScheduleOccurrenceDate"/> )
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the occurrence date (when in <see cref="OccurrenceDisplayMode.ScheduleOccurrenceDate"/> )
            /// </summary>
            /// <value>
            /// The occurrence date.
            /// </value>
            public DateTime? OccurrenceDate { get; set; }

            /// <summary>
            /// Gets the Schedule's scheduled date time the Occurrence Date  (when in <see cref="OccurrenceDisplayMode.ScheduleOccurrenceDate"/> )
            /// </summary>
            /// <value>
            /// The scheduled date time.
            /// </value>
            public DateTime? ScheduledDateTime
            {
                get
                {
                    if ( this.OccurrenceDate.HasValue )
                    {
                        return Schedule.GetNextStartDateTime( this.OccurrenceDate.Value );
                    }

                    return null;
                }
            }

            /// <summary>
            /// Gets or sets the attendance occurrence items for this column
            /// </summary>
            /// <value>
            /// The attendance occurrence items.
            /// </value>
            public List<AttendanceOccurrenceRowItem> AttendanceOccurrenceItems { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        [System.Diagnostics.DebuggerDisplay( "{Schedule} {Group} {OccurrenceDate} {ScheduledDateTime}" )]
        private class AttendanceOccurrenceRowItem
        {
            public OccurrenceDisplayMode OccurrenceDisplayMode { get; set; }

            /// <summary>
            /// Gets or sets the attendance occurrence identifier.
            /// </summary>
            /// <value>
            /// The attendance occurrence identifier.
            /// </value>
            public int AttendanceOccurrenceId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance has attendees.
            /// Use this to determine if this occurrence should be shown, even if the Group doesn't have the Location/Schedule configured
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has attendees; otherwise, <c>false</c>.
            /// </value>
            public bool HasAttendees { get; set; }

            /// <summary>
            /// Gets or sets the capacity information.
            /// </summary>
            /// <value>
            /// The capacity information.
            /// </value>
            public CapacityInfo CapacityInfo { get; set; }

            /// <summary>
            /// Gets or sets the name of the location.
            /// </summary>
            /// <value>
            /// The name of the location.
            /// </value>
            public string LocationName { get; set; }

            /// <summary>
            /// Gets or sets the location identifier.
            /// NOTE: There should only be one that doesn't have a LocationId, and it should only be shown if there are assignments in it
            /// </summary>
            /// <value>
            /// The location identifier.
            /// </value>
            public int? LocationId { get; set; }

            /// <summary>
            /// Gets or sets the schedule.
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets the Schedule's scheduled date time the Occurrence Date
            /// </summary>
            /// <value>
            /// The scheduled date time.
            /// </value>
            public DateTime? ScheduledDateTime
            {
                get
                {
                    return Schedule.GetNextStartDateTime( this.OccurrenceDate );
                }
            }

            public Group Group { get; set; }

            /// <summary>
            /// Gets or sets the occurrence date.
            /// </summary>
            /// <value>
            /// The occurrence date.
            /// </value>
            public DateTime OccurrenceDate { get; set; }

            /// <summary>
            /// Gets or sets the group location order.
            /// </summary>
            /// <value>
            /// The group location order.
            /// </value>
            public int GroupLocationOrder { get; internal set; }
        }

        private class GroupLocationScheduleInfo
        {
            public int GroupLocationId { get; set; }

            public List<int> LocationScheduleIds { get; set; }

            public int LocationId { get; internal set; }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptOccurrenceColumns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptOccurrenceColumns_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var rptAttendanceOccurrences = e.Item.FindControl( "rptAttendanceOccurrences" ) as Repeater;
            var pnlMultiGroupModeColumnHeading = e.Item.FindControl( "pnlMultiGroupModeColumnHeading" ) as Panel;
            var pnlSingleGroupModeColumnHeading = e.Item.FindControl( "pnlSingleGroupModeColumnHeading" ) as Panel;
            var pnlGroupHasSchedulingDisabled = e.Item.FindControl( "pnlGroupHasSchedulingDisabled" ) as Panel;

            OccurrenceColumnItem occurrenceColumnItem = e.Item.DataItem as OccurrenceColumnItem;
            pnlMultiGroupModeColumnHeading.Visible = occurrenceColumnItem.OccurrenceDisplayMode == OccurrenceDisplayMode.MultiGroup;
            pnlSingleGroupModeColumnHeading.Visible = occurrenceColumnItem.OccurrenceDisplayMode == OccurrenceDisplayMode.SingleGroup;
            var columnCssClasses = new List<string>();
            columnCssClasses.Add( "board-column" );
            columnCssClasses.Add( "occurrence-column" );
            columnCssClasses.Add( "js-occurrence-column" );

            bool isSchedulerTargetColumn = false;

            if ( occurrenceColumnItem.OccurrenceDisplayMode == OccurrenceDisplayMode.MultiGroup )
            {
                columnCssClasses.Add( "occurrence-column-group" );
                var groupType = GroupTypeCache.Get( occurrenceColumnItem.Group.GroupTypeId );
                var lMultiGroupModeColumnGroupNameHtml = e.Item.FindControl( "lMultiGroupModeColumnGroupNameHtml" ) as Literal;
                lMultiGroupModeColumnGroupNameHtml.Text = string.Format(
                    "<i class='{0}'></i> {1}",
                    groupType.IconCssClass,
                    occurrenceColumnItem.Group.Name );

                var selectedGroupId = hfSelectedGroupId.Value.AsInteger();

                bool isSelectedColumn = hfSelectedGroupId.Value.AsInteger() == occurrenceColumnItem.Group.Id;

                // when in multi-group mode, only one group (column) can have resources dragged in and out of it
                isSchedulerTargetColumn = isSelectedColumn;

                if ( isSelectedColumn )
                {
                    columnCssClasses.Add( "occurrence-column-selected" );
                }

                var btnMultiGroupModeColumnSelectedGroup = e.Item.FindControl( "btnMultiGroupModeColumnSelectedGroup" ) as LinkButton;
                if ( isSelectedColumn )
                {
                    btnMultiGroupModeColumnSelectedGroup.Text = "<i class='fa fa-check-square'></i>";
                }
                else
                {
                    btnMultiGroupModeColumnSelectedGroup.Text = "<i class='fa fa-square-o'></i>";
                }

                btnMultiGroupModeColumnSelectedGroup.Attributes["data-group-id"] = occurrenceColumnItem.Group.Id.ToString();
            }
            else if ( occurrenceColumnItem.OccurrenceDisplayMode == OccurrenceDisplayMode.SingleGroup )
            {
                columnCssClasses.Add( "occurrence-column-schedule" );

                // when in single-group mode, all columns can have resources dragged in or out of it
                isSchedulerTargetColumn = true;
                columnCssClasses.Add( "js-scheduler-target-column" );

                // Single Group mode, so show column header with Schedule Info
                var lSingleGroupModeColumnHeadingOccurrenceDate = e.Item.FindControl( "lSingleGroupModeColumnHeadingOccurrenceDate" ) as Literal;
                var lSingleGroupModeColumnHeadingOccurrenceScheduleName = e.Item.FindControl( "lSingleGroupModeColumnHeadingOccurrenceScheduleName" ) as Literal;

                // show date in 'Sunday, June 15' format
                lSingleGroupModeColumnHeadingOccurrenceDate.Text = occurrenceColumnItem.ScheduledDateTime.Value.ToString( "dddd, MMMM dd" );

                // show time in '10:30 AM' format
                lSingleGroupModeColumnHeadingOccurrenceScheduleName.Text = occurrenceColumnItem.Schedule?.AbbreviatedName ?? occurrenceColumnItem.ScheduledDateTime.Value.ToString( "h:mm tt" );
            }
            else
            {
                // shouldn't happen
            }

            if ( occurrenceColumnItem.Group.DisableScheduling )
            {
                columnCssClasses.Add( "group-has-scheduling-disabled" );
            }

            var pnlOccurrenceColumn = e.Item.FindControl( "pnlOccurrenceColumn" ) as Panel;
            pnlOccurrenceColumn.CssClass = columnCssClasses.AsDelimited( " " );

            // when in Multi-Group mode, only one group can be scheduled at a time,
            // which means that only this column can have resources dragged in or out of it
            pnlOccurrenceColumn.Attributes["data-is-scheduler-target-column"] = isSchedulerTargetColumn.ToJavaScriptValue();

            if ( occurrenceColumnItem.Group.DisableScheduling == true )
            {
                pnlGroupHasSchedulingDisabled.Visible = true;
                rptAttendanceOccurrences.Visible = false;
            }
            else
            {
                pnlGroupHasSchedulingDisabled.Visible = false;
                rptAttendanceOccurrences.Visible = true;
                rptAttendanceOccurrences.DataSource = occurrenceColumnItem.AttendanceOccurrenceItems;
                rptAttendanceOccurrences.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAttendanceOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAttendanceOccurrences_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var attendanceOccurrenceRowItem = e.Item.DataItem as AttendanceOccurrenceRowItem;
            var attendanceOccurrenceId = attendanceOccurrenceRowItem.AttendanceOccurrenceId;
            var pnlScheduledOccurrence = e.Item.FindControl( "pnlScheduledOccurrence" ) as Panel;
            var pnlStatusLabels = e.Item.FindControl( "pnlStatusLabels" ) as Panel;

            // hide the scheduled occurrence when it is empty if is the one that doesn't have a Location assigned
            bool hasLocation = attendanceOccurrenceRowItem.LocationId.HasValue;
            pnlScheduledOccurrence.Attributes["data-has-location"] = hasLocation.Bit().ToString();

            // hide the status labels if is the one that doesn't have a Location assigned
            pnlStatusLabels.Visible = hasLocation;

            var pnlMultiGroupModePanelHeading = e.Item.FindControl( "pnlMultiGroupModePanelHeading" ) as Panel;
            var lMultiGroupModeLocationTitle = e.Item.FindControl( "lMultiGroupModeLocationTitle" ) as Literal;
            lMultiGroupModeLocationTitle.Text = $"<span class=\"location\">{attendanceOccurrenceRowItem.LocationName}</span>";
            if ( attendanceOccurrenceRowItem.ScheduledDateTime.HasValue )
            {
                var lMultiGroupModeOccurrenceScheduledDate = e.Item.FindControl( "lMultiGroupModeOccurrenceScheduledDate" ) as Literal;
                var lMultiGroupModeOccurrenceScheduleName = e.Item.FindControl( "lMultiGroupModeOccurrenceScheduleName" ) as Literal;

                if ( !attendanceOccurrenceRowItem.LocationId.HasValue )
                {
                    lMultiGroupModeLocationTitle.Text = $"<span class=\"location resource-no-location-preference\">{attendanceOccurrenceRowItem.LocationName}</span>";
                }

                // show date in 'Sunday, June 15' format
                lMultiGroupModeOccurrenceScheduledDate.Text = attendanceOccurrenceRowItem.ScheduledDateTime.Value.ToString( "dddd, MMMM dd" );

                // show schedule name if null show time in '10:30 AM' format
                lMultiGroupModeOccurrenceScheduleName.Text = attendanceOccurrenceRowItem.Schedule?.AbbreviatedName ?? attendanceOccurrenceRowItem.ScheduledDateTime.Value.ToString( "h:mm tt" );
                pnlScheduledOccurrence.Attributes["data-attendanceoccurrence-date"] = attendanceOccurrenceRowItem.ScheduledDateTime.Value.Date.ToISO8601DateString();
            }

            var pnlSingleGroupModePanelHeading = e.Item.FindControl( "pnlSingleGroupModePanelHeading" ) as Panel;
            var lSingleGroupModeLocationTitle = e.Item.FindControl( "lSingleGroupModeLocationTitle" ) as Literal;
            lSingleGroupModeLocationTitle.Text = attendanceOccurrenceRowItem.LocationName;

            if ( attendanceOccurrenceRowItem.OccurrenceDisplayMode == OccurrenceDisplayMode.MultiGroup )
            {
                pnlMultiGroupModePanelHeading.Visible = true;
                pnlSingleGroupModePanelHeading.Visible = false;
            }
            else
            {
                pnlMultiGroupModePanelHeading.Visible = false;
                pnlSingleGroupModePanelHeading.Visible = true;
            }

            pnlScheduledOccurrence.Attributes["data-attendanceoccurrence-id"] = attendanceOccurrenceId.ToString();

            if ( attendanceOccurrenceRowItem.CapacityInfo != null )
            {
                pnlScheduledOccurrence.Attributes["data-minimum-capacity"] = attendanceOccurrenceRowItem.CapacityInfo.MinimumCapacity.ToString();
                pnlScheduledOccurrence.Attributes["data-desired-capacity"] = attendanceOccurrenceRowItem.CapacityInfo.DesiredCapacity.ToString();
                pnlScheduledOccurrence.Attributes["data-maximum-capacity"] = attendanceOccurrenceRowItem.CapacityInfo.MaximumCapacity.ToString();
            }
        }

        #endregion Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the btnShowChildGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowChildGroups_Click( object sender, EventArgs e )
        {
            var showChildGroups = btnShowChildGroups.Attributes["show-child-groups"].AsBoolean();
            SetStateForShowChildGroupsButton( !showChildGroups );
            UpdatePickedGroups();
        }

        /// <summary>
        /// Handles the ValueChanged event of the gpGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpPickedGroups_ValueChanged( object sender, EventArgs e )
        {
            UpdatePickedGroups();
        }

        /// <summary>
        /// Updates the picked groups after the GroupPicker or ShowChildGroups UI controls are changed
        /// </summary>
        private void UpdatePickedGroups()
        {
            // the glListedGroups picker selected the groups that we show in the Scheduler, but only one Group can be the active/selected one
            var pickedGroupIds = gpPickedGroups.SelectedIds.ToList();

            var authorizedListedGroups = GetAuthorizedListedGroups();
            var listedGroupIds = authorizedListedGroups.Select( a => a.Id ).ToList();

            // if there isn't a currently selected group, default to the first one
            var schedulingEnabledGroupIds = GetSchedulingEnabledGroupIds( authorizedListedGroups.Select( a => a.Id ).ToList() );

            var selectedGroupId = hfSelectedGroupId.Value.AsIntegerOrNull();

            if ( selectedGroupId.HasValue && selectedGroupId > 0 )
            {
                // if the currently selectedGroupId is not in the updated listedGroupIds, default the selectedGroupId to the first listed Groupid
                if ( !listedGroupIds.Contains( selectedGroupId.Value ) || !schedulingEnabledGroupIds.Contains( selectedGroupId.Value ) )
                {
                    selectedGroupId = schedulingEnabledGroupIds.FirstOrDefault();
                }
            }
            else
            {
                selectedGroupId = schedulingEnabledGroupIds.FirstOrDefault();
            }

            hfSelectedGroupId.Value = selectedGroupId.ToString();

            UpdateScheduleList( authorizedListedGroups );
            UpdateLocationList( authorizedListedGroups );
            ApplyFilter();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlWeek control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlWeek_SelectedIndexChanged( object sender, EventArgs e )
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblGroupLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblGroupLocations_SelectedIndexChanged( object sender, EventArgs e )
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the Change event of the ResourceListSourceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ResourceListSourceType_Change( object sender, EventArgs e )
        {
            LinkButton btnResourceListSourceType = sender as LinkButton;

            GroupSchedulerResourceListSourceType schedulerResourceListSourceType = ( GroupSchedulerResourceListSourceType ) btnResourceListSourceType.CommandArgument.AsInteger();
            SchedulerResourceGroupMemberFilterType schedulerResourceGroupMemberFilterType = SchedulerResourceGroupMemberFilterType.ShowAllGroupMembers;

            if ( schedulerResourceListSourceType == GroupSchedulerResourceListSourceType.GroupMatchingPreference )
            {
                schedulerResourceGroupMemberFilterType = SchedulerResourceGroupMemberFilterType.ShowMatchingPreference;
            }

            SetResourceListSourceType( schedulerResourceListSourceType, schedulerResourceGroupMemberFilterType );
            ApplyFilter();
        }

        /// <summary>
        /// Sets the hidden field values for GroupSchedulerResourceListSourceType and SchedulerResourceGroupMemberFilterType and updates the text of the filter controls.
        /// </summary>
        /// <param name="schedulerResourceListSourceType">Type of the scheduler resource list source.</param>
        /// <param name="schedulerResourceGroupMemberFilterType">Type of the scheduler resource group member filter.</param>
        private void SetResourceListSourceType( GroupSchedulerResourceListSourceType schedulerResourceListSourceType, SchedulerResourceGroupMemberFilterType schedulerResourceGroupMemberFilterType )
        {
            hfSchedulerResourceListSourceType.Value = schedulerResourceListSourceType.ConvertToInt().ToString();
            hfResourceGroupMemberFilterType.Value = schedulerResourceGroupMemberFilterType.ConvertToInt().ToString();
            lSelectedResourceTypeDropDownText.Text = schedulerResourceListSourceType.GetDescription();
            sfResource.Placeholder = $"Search \"{lSelectedResourceTypeDropDownText.Text.Replace( "Group Members - ", string.Empty )}\"";
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgResourceListSource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgResourceListSource_SelectedIndexChanged( object sender, EventArgs e )
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the ValueChanged event of the gpResourceListAlternateGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void gpResourceListAlternateGroup_ValueChanged( object sender, EventArgs e )
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblGroupMemberFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblGroupMemberFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the ValueChanged event of the dvpResourceListDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpResourceListDataView_ValueChanged( object sender, EventArgs e )
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the Click event of the btnAutoScheduleAllGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAutoScheduleAllGroups_Click( object sender, EventArgs e )
        {
            List<Group> autoScheduleGroups = GetAuthorizedListedGroups();
            AutoSchedule( autoScheduleGroups );
        }

        /// <summary>
        /// Handles the Click event of the btnAutoScheduleSelectedGroup and btnAutoScheduleSingleGroupMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAutoScheduleSelectedGroup_Click( object sender, EventArgs e )
        {
            List<Group> autoScheduleGroups = new List<Group>();
            var currentlySelectedGroup = GetCurrentlySelectedGroup();
            if ( currentlySelectedGroup != null )
            {
                autoScheduleGroups.Add( currentlySelectedGroup );
            }

            AutoSchedule( autoScheduleGroups );
        }

        /// <summary>
        /// Auto-Schedules for the selected groups
        /// </summary>
        /// <param name="groups">The groups.</param>
        protected void AutoSchedule( List<Group> groups )
        {
            var rockContext = new RockContext();

            var displayedAttendanceOccurrenceIdList = hfDisplayedOccurrenceIds.Value.SplitDelimitedValues().AsIntegerList();
            var groupIds = groups.Select( a => a.Id ).ToList();

            var attendanceOccurrenceIdList = new AttendanceOccurrenceService( rockContext )
                .Queryable()
                .Where( a =>
                     a.GroupId.HasValue
                     && displayedAttendanceOccurrenceIdList.Contains( a.Id )
                     && groupIds.Contains( a.GroupId.Value ) )
                .Select( a => a.Id ).ToList();

            var attendanceService = new AttendanceService( rockContext );

            // AutoSchedule the occurrences that are shown
            attendanceService.SchedulePersonsAutomaticallyForAttendanceOccurrences( attendanceOccurrenceIdList, this.CurrentPersonAlias );
            rockContext.SaveChanges();

            upnlContent.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnSendNowAllGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendNowAllGroups_Click( object sender, EventArgs e )
        {
            List<Group> sendToGroups = GetAuthorizedListedGroups();
            SendConfirmations( sendToGroups );
        }

        /// <summary>
        /// Handles the Click event of the btnSendNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendNowSelectedGroup_Click( object sender, EventArgs e )
        {
            List<Group> sendToGroups = new List<Group>();
            var currentlySelectedGroup = GetCurrentlySelectedGroup();
            if ( currentlySelectedGroup != null )
            {
                sendToGroups.Add( currentlySelectedGroup );
            }

            SendConfirmations( sendToGroups );
        }

        /// <summary>
        /// Sends the confirmation emails to the specified groups
        /// </summary>
        /// <param name="groups">The groups.</param>
        protected void SendConfirmations( List<Group> groups )
        {
            upnlContent.Update();
            var rockContext = new RockContext();

            var displayedAttendanceOccurrenceIdList = hfDisplayedOccurrenceIds.Value.SplitDelimitedValues().AsIntegerList();
            var groupIds = groups.Select( a => a.Id ).ToList();

            var attendanceOccurrenceIdList = new AttendanceOccurrenceService( rockContext )
                .Queryable()
                .Where( a =>
                     a.GroupId.HasValue
                     && displayedAttendanceOccurrenceIdList.Contains( a.Id )
                     && groupIds.Contains( a.GroupId.Value ) )
                .Select( a => a.Id )
                .ToList();

            var attendanceService = new AttendanceService( rockContext );
            var sendConfirmationAttendancesQuery = attendanceService.GetPendingAndAutoAcceptScheduledConfirmations()
                .Where( a => attendanceOccurrenceIdList.Contains( a.OccurrenceId ) )
                .Where( a => a.ScheduleConfirmationSent != true );

            var sendMessageResult = attendanceService.SendScheduleConfirmationCommunication( sendConfirmationAttendancesQuery );
            var isSendConfirmationAttendancesFound = sendConfirmationAttendancesQuery.Any();
            rockContext.SaveChanges();

            var summaryMessageBuilder = new StringBuilder();
            var alertType = ModalAlertType.Information;

            if ( sendMessageResult.Errors.Any() )
            {
                alertType = ModalAlertType.Alert;

                var logException = new Exception( "One or more errors occurred when sending confirmations: " + Environment.NewLine + sendMessageResult.Errors.AsDelimited( Environment.NewLine ) );

                ExceptionLogService.LogException( logException );

                summaryMessageBuilder.AppendLine( logException.Message );
            }

            if ( sendMessageResult.Warnings.Any() )
            {
                if ( alertType != ModalAlertType.Alert )
                {
                    alertType = ModalAlertType.Warning;
                }

                var warningMessage = "One or more warnings occurred when sending confirmations: " + Environment.NewLine + sendMessageResult.Warnings.AsDelimited( Environment.NewLine );

                summaryMessageBuilder.AppendLine( warningMessage );
            }

            if ( sendMessageResult.MessagesSent > 0 && isSendConfirmationAttendancesFound )
            {
                summaryMessageBuilder.AppendLine( string.Format( "Successfully sent {0} {1}.", sendMessageResult.MessagesSent, "confirmation".PluralizeIf( sendMessageResult.MessagesSent != 1 ) ) );
            }
            else if ( !isSendConfirmationAttendancesFound )
            {
                summaryMessageBuilder.AppendLine( "Everybody has already been sent a confirmation. No additional confirmations sent." );
            }

            maSendNowResults.Show( summaryMessageBuilder.ToString().ConvertCrLfToHtmlBr(), alertType );
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            var additionPersonIds = hfResourceAdditionalPersonIds.Value.SplitDelimitedValues().AsIntegerList();
            if ( ppAddPerson.PersonId.HasValue )
            {
                additionPersonIds.Add( ppAddPerson.PersonId.Value );
            }

            hfResourceAdditionalPersonIds.Value = additionPersonIds.AsDelimited( "," );

            // clear on the selected person
            ppAddPerson.SetValue( null );

            upnlContent.Update();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSchedulerResourceListSourceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSchedulerResourceListSourceType_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var btnResourceListSourceType = e.Item.FindControl( "btnResourceListSourceType" ) as LinkButton;
            GroupSchedulerResourceListSourceType schedulerResourceListSourceType = ( GroupSchedulerResourceListSourceType ) e.Item.DataItem;
            btnResourceListSourceType.Text = schedulerResourceListSourceType.GetDescription() ?? schedulerResourceListSourceType.ConvertToString( true );
            btnResourceListSourceType.CommandArgument = schedulerResourceListSourceType.ConvertToInt().ToString();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptWeekSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptWeekSelector_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var endOfWeekDate = ( DateTime ) e.Item.DataItem;
            string weekTitle = $"{endOfWeekDate.AddDays( -6 ).ToShortDateString()} to {endOfWeekDate.ToShortDateString()}";

            var btnSelectWeek = e.Item.FindControl( "btnSelectWeek" ) as LinkButton;
            btnSelectWeek.Text = weekTitle;
            btnSelectWeek.CommandArgument = endOfWeekDate.ToISO8601DateString();
        }

        /// <summary>
        /// Handles the Click event of the btnSelectWeek control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelectWeek_Click( object sender, EventArgs e )
        {
            var btnSelectWeek = sender as LinkButton;
            hfWeekSundayDate.Value = btnSelectWeek.CommandArgument;
            ApplyFilter();
        }

        /// <summary>
        /// Handles the Click event of the btnSelectSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelectSchedule_Click( object sender, EventArgs e )
        {
            var btnSelectSchedule = sender as LinkButton;
            hfSelectedScheduleId.Value = btnSelectSchedule.CommandArgument;

            var authorizedListedGroups = GetAuthorizedListedGroups();

            UpdateLocationList( authorizedListedGroups );
            ApplyFilter();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduleSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduleSelector_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            LinkButton btnSelectSchedule = e.Item.FindControl( "btnSelectSchedule" ) as LinkButton;
            var schedule = e.Item.DataItem as Schedule;
            if ( schedule != null )
            {
                btnSelectSchedule.CommandArgument = schedule.Id.ToString();

                if ( schedule.Name.IsNotNullOrWhiteSpace() )
                {
                    btnSelectSchedule.Text = schedule.Name;
                }
                else
                {
                    btnSelectSchedule.Text = schedule.FriendlyScheduleText;
                }
            }
            else
            {
                btnSelectSchedule.CommandArgument = "all";
                btnSelectSchedule.Text = "All Schedules";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelectLocation_Click( object sender, EventArgs e )
        {
            var btnSelectLocation = sender as LinkButton;

            var selectedLocationId = btnSelectLocation.CommandArgument.AsIntegerOrNull();

            if ( selectedLocationId.HasValue )
            {
                var selectedLocationIds = hfPickedLocationIds.Value.Split( ',' ).AsIntegerList();

                // toggle if the selected group location is in the selected group locations
                if ( selectedLocationIds.Contains( selectedLocationId.Value ) )
                {
                    selectedLocationIds.Remove( selectedLocationId.Value );
                }
                else
                {
                    selectedLocationIds.Add( selectedLocationId.Value );
                }

                hfPickedLocationIds.Value = selectedLocationIds.AsDelimited( "," );
            }
            else
            {
                hfPickedLocationIds.Value = "all";
            }

            ApplyFilter();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupLocationSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptLocationSelector_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            LinkButton btnSelectLocation = e.Item.FindControl( "btnSelectLocation" ) as LinkButton;
            Location location = e.Item.DataItem as Location;
            if ( location != null )
            {
                btnSelectLocation.CommandArgument = location.Id.ToString();
                btnSelectLocation.Text = location.ToString( true );
            }
            else
            {
                btnSelectLocation.CommandArgument = "all";
                btnSelectLocation.Text = "All Locations";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMultiGroupModeColumnSelectedGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMultiGroupModeColumnSelectedGroup_Click( object sender, EventArgs e )
        {
            var btnMultiGroupModeColumnSelectedGroup = sender as LinkButton;

            var groupId = btnMultiGroupModeColumnSelectedGroup.Attributes["data-group-id"].AsInteger();
            hfSelectedGroupId.Value = groupId.ToString();
            ApplyFilter();
        }

        /// <summary>
        /// Updates the group schedule assignment preference.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        /// <param name="groupMemberId">The group member identifier.</param>
        protected void UpdateGroupScheduleAssignmentPreference( int attendanceId, int groupMemberId )
        {
            var rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            AttendanceService attendanceService = new AttendanceService( rockContext );
            var groupMemberPerson = groupMemberService.GetSelect( groupMemberId, s => new
            {
                s.Person,
                s.Group
            } );
            var attendanceOccurrence = attendanceService.GetSelect( attendanceId, s => s.Occurrence );

            if ( attendanceOccurrence == null || groupMemberPerson == null )
            {
                return;
            }

            var scheduleId = attendanceOccurrence.ScheduleId;
            var locationId = attendanceOccurrence.LocationId;
            var groupId = attendanceOccurrence.GroupId;

            GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var groupMemberAssignmentQuery = groupMemberAssignmentService.Queryable();

            var groupMemberPersonId = groupMemberPerson.Person.Id;
            var group = groupMemberPerson.Group;

            /* 2020-07-23 MDP
             *  Note that an Attendance record is for a Person, not a GroupMemberId, so GroupMemberId would be whatever GroupMember record was found for this
             *  Person in the Occurrence group.
             *  So, if the person is in the group multiple times, the groupMember record would be first group member record for that person, sorted by GroupTypeRole.Order.
             *  But, they could have preferences for multiple group members records, so lookup by personId instead of GroupMemberId
             */
            var preferencesForGroup = groupMemberAssignmentQuery
                .Where( a => !a.GroupMember.IsArchived && a.GroupMember.GroupId == groupId && a.GroupMember.PersonId == groupMemberPersonId )
                .ToList();

            nbGroupScheduleAssignmentUpdatePreferenceInformation.Text = string.Empty;

            var otherPreferencesForGroup = preferencesForGroup
                    .Where( a => a.ScheduleId != scheduleId && ( !a.LocationId.HasValue || a.LocationId == locationId ) );

            if ( otherPreferencesForGroup.Any() )
            {
                var currentSchedulePreferencesHTMLBuilder = new StringBuilder();
                currentSchedulePreferencesHTMLBuilder.AppendLine( "<span class='control-label'>These other preferences will be removed and replaced.</span>" );
                currentSchedulePreferencesHTMLBuilder.AppendLine( "<ul>" );
                var occurrenceDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ).AddDays( 1 );
                var otherPreferencesSortedBySchedule = otherPreferencesForGroup
                    .OrderBy( a => a.Schedule.Order )
                    .ThenBy( a => a.Schedule.GetNextStartDateTime( occurrenceDate ) )
                    .ThenBy( a => a.Schedule.Name )
                    .ThenBy( a => a.Schedule.Id )
                    .ToList();

                foreach ( var otherPreferenceForGroup in otherPreferencesSortedBySchedule )
                {
                    string locationPreference;
                    if ( otherPreferenceForGroup.Location == null )
                    {
                        locationPreference = "No Location Preference";
                    }
                    else
                    {
                        locationPreference = otherPreferenceForGroup.Location.Name;
                    }

                    currentSchedulePreferencesHTMLBuilder.AppendLine( string.Format( "<li>{0} - {1}</li>", otherPreferenceForGroup.Schedule.Name, locationPreference ) );
                }

                currentSchedulePreferencesHTMLBuilder.AppendLine( "</ul>" );

                nbGroupScheduleAssignmentUpdatePreferenceInformation.Text = currentSchedulePreferencesHTMLBuilder.ToString();
            }
            else
            {
                nbGroupScheduleAssignmentUpdatePreferenceInformation.Text = string.Empty;
            }

            mdGroupScheduleAssignmentPreference.SubTitle = string.Format( "{0}, {1} - {2} ", groupMemberPerson.Person, attendanceOccurrence.Schedule?.Name ?? "No Schedule", attendanceOccurrence.Location?.Name ?? "No Location Preference" );

            nbGroupScheduleAssignmentUpdatePreferenceInformation.Visible = rblGroupScheduleAssignmentUpdateOption.SelectedValue == "UpdatePreference";

            GroupMemberAssignment preferenceForSchedule = preferencesForGroup.FirstOrDefault();

            nbGroupScheduleAssignmentScheduleWarning.Visible = false;

            int? scheduleTemplateId = null;
            DateTime? scheduleStartDate = null;

            if ( preferenceForSchedule != null )
            {
                scheduleTemplateId = preferenceForSchedule.GroupMember.ScheduleTemplateId;
                scheduleStartDate = preferenceForSchedule.GroupMember.ScheduleStartDate;
                if ( preferenceForSchedule.LocationId.HasValue && preferenceForSchedule.LocationId.Value == locationId )
                {
                    nbGroupScheduleAssignmentScheduleWarning.Visible = true;
                    nbGroupScheduleAssignmentScheduleWarning.Text = "This person already has this location as their preference for this schedule.";
                }
                else
                {
                    string locationPreference;
                    if ( preferenceForSchedule.Location == null )
                    {
                        locationPreference = "No Location Preference";
                    }
                    else
                    {
                        locationPreference = preferenceForSchedule.Location.Name;
                    }

                    nbGroupScheduleAssignmentScheduleWarning.Visible = true;
                    nbGroupScheduleAssignmentScheduleWarning.Text = string.Format( "This person currently has {0} as the preference for this schedule.", locationPreference );
                }
            }

            hfGroupScheduleAssignmentGroupMemberId.Value = groupMemberId.ToString();
            hfGroupScheduleAssignmentLocationId.Value = locationId.ToString();
            hfGroupScheduleAssignmentScheduleId.Value = scheduleId.ToString();


            // Templates for all and this group type.
            var groupMemberScheduleTemplateService = new GroupMemberScheduleTemplateService( rockContext );
            var groupMemberScheduleTemplates = groupMemberScheduleTemplateService
                .Queryable()
                .AsNoTracking()
                .Where( x => x.GroupTypeId == null || x.GroupTypeId == group.GroupTypeId )
                .Select( x => new { Value = ( int? ) x.Id, Text = x.Name } )
                .ToList();

            groupMemberScheduleTemplates.Insert( 0, new { Value = ( int? ) null, Text = "No Schedule" } );

            ddlGroupMemberScheduleTemplate.DataSource = groupMemberScheduleTemplates;
            ddlGroupMemberScheduleTemplate.DataValueField = "Value";
            ddlGroupMemberScheduleTemplate.DataTextField = "Text";
            ddlGroupMemberScheduleTemplate.DataBind();

            ddlGroupMemberScheduleTemplate.SelectedValue = scheduleTemplateId == null ? string.Empty : scheduleTemplateId.ToString();

            pnlGroupPreferenceAssignment.Visible = scheduleTemplateId.HasValue;

            dpGroupMemberScheduleTemplateStartDate.SelectedDate = scheduleStartDate;
            if ( dpGroupMemberScheduleTemplateStartDate.SelectedDate == null )
            {
                dpGroupMemberScheduleTemplateStartDate.SelectedDate = RockDateTime.Today;
            }

            mdGroupScheduleAssignmentPreference.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupMemberScheduleTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupMemberScheduleTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            pnlGroupPreferenceAssignment.Visible = ddlGroupMemberScheduleTemplate.SelectedValueAsId().HasValue;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblGroupScheduleAssignmentUpdateOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblGroupScheduleAssignmentUpdateOption_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbGroupScheduleAssignmentUpdatePreferenceInformation.Visible = rblGroupScheduleAssignmentUpdateOption.SelectedValue == "UpdatePreference";
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupScheduleAssignmentPreference control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupScheduleAssignmentPreference_SaveClick( object sender, EventArgs e )
        {
            mdGroupScheduleAssignmentPreference.Hide();
            var groupMemberId = hfGroupScheduleAssignmentGroupMemberId.Value.AsIntegerOrNull();
            var locationId = hfGroupScheduleAssignmentLocationId.Value.AsIntegerOrNull();
            var scheduleId = hfGroupScheduleAssignmentScheduleId.Value.AsIntegerOrNull();

            if ( !locationId.HasValue || !scheduleId.HasValue || !groupMemberId.HasValue )
            {
                // shouldn't happen
                return;
            }

            var rockContext = new RockContext();

            var groupMemberService = new GroupMemberService( rockContext );

            var groupMember = groupMemberService.Get( groupMemberId.Value );

            if ( groupMember == null )
            {
                // shouldn't happen
                return;
            }

            GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var groupMemberAssignmentQuery = groupMemberAssignmentService.Queryable();

            /* 2020-07-23 MDP
             *  Note that an Attendance record is for a Person, not a GroupMemberId, so GroupMemberId would be whatever GroupMember record was found for this
             *  Person in the Occurrence group.
             *  So, f the person is in the group multiple times, the groupMember record would be first group member record for that person, sorted by GroupTypeRole.Order.
             *  But, they could have preferences for multiple group members records, so lookup by personId instead of GroupMemberId
             */
            int groupMemberPersonId = groupMember.PersonId;

            var locationPreferenceForSchedule = groupMemberAssignmentQuery
                .Where( a =>
                    !a.GroupMember.IsArchived
                    && a.GroupMember.PersonId == groupMemberPersonId
                    && a.ScheduleId.HasValue
                    && a.ScheduleId == scheduleId.Value ).FirstOrDefault();

            if ( locationPreferenceForSchedule == null )
            {
                locationPreferenceForSchedule = new GroupMemberAssignment();
                locationPreferenceForSchedule.GroupMemberId = groupMemberId.Value;
                locationPreferenceForSchedule.ScheduleId = scheduleId.Value;
                groupMemberAssignmentService.Add( locationPreferenceForSchedule );
            }

            locationPreferenceForSchedule.LocationId = locationId.Value;
            groupMember.ScheduleStartDate = dpGroupMemberScheduleTemplateStartDate.SelectedDate;
            groupMember.ScheduleTemplateId = ddlGroupMemberScheduleTemplate.SelectedValueAsId();

            /* 2020-07-23 MDP
                 - 'Update Preference' means that the selected Schedule/Location is now their *only* preference for this Group.
                    So, if they have preferences for other schedules for this group, delete them
                    see https://app.asana.com/0/0/1185765604320009/f

                - 'Append to Preference' means that we are just adding (or updating) the location preference for the selected schedule
             */

            if ( rblGroupScheduleAssignmentUpdateOption.SelectedValue == "UpdatePreference" )
            {
                // Remove all other schedule preferences that this person has for this group (see https://app.asana.com/0/0/1185765604320009/f)
                var otherPreferencesForGroup = groupMemberAssignmentQuery
                    .Where( a =>
                        a.GroupMember.PersonId == groupMemberPersonId
                        && a.ScheduleId != scheduleId.Value )
                    .ToList();

                if ( otherPreferencesForGroup.Any() )
                {
                    groupMemberAssignmentService.DeleteRange( otherPreferencesForGroup );
                }
            }

            rockContext.SaveChanges();

            ApplyFilter();

        }

        #endregion Events
    }
}
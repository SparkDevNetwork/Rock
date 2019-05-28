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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    /// A block for a person to use to manage their group scheduling. View schedule, change preferences, and sign-up for available needs
    /// </summary>
    [DisplayName( "Group Schedule Toolbox" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows management of group scheduling for a specific person (worker)." )]

    [ContextAware( typeof( Rock.Model.Person ) )]

    [IntegerField(
        "Number of Future Weeks To Show",
        Description = "The number of weeks into the future to allow users to signup for a schedule.",
        IsRequired = true,
        DefaultValue = "6",
        Order = 0,
        Key = AttributeKeys.FutureWeeksToShow )]

    [CodeEditorField(
        "Signup Instructions",
        Description = "Instructions here will show up on Signup tab. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue =
    @"<div class=""alert alert-info"">
    {%- if IsSchedulesAvailable -%}
        {%- if CurrentPerson.Id != Person.Id -%}
            Sign up to attend a group and location on the given date.
        {%- else -%}
            Sign up {{ Person.FullName }} to attend a group and location on a given date.
        {%- endif -%}
     {%- else -%}
        No sign-ups available.
     {%- endif -%}
</div>",
        Order = 1,
        Key = AttributeKeys.SignupInstructions )]
    public partial class GroupScheduleToolbox : RockBlock
    {
        protected class AttributeKeys
        {
            public const string FutureWeeksToShow = "FutureWeeksToShow";
            public const string SignupInstructions = "SignupInstructions";
        }

        protected const string ALL_GROUPS_STRING = "All Groups";

        private readonly List<GroupScheduleToolboxTab> _tabs = Enum.GetValues( typeof( GroupScheduleToolboxTab ) ).Cast<GroupScheduleToolboxTab>().ToList();

        /// <summary>
        /// Tab menu options
        /// </summary>
        public enum GroupScheduleToolboxTab
        {
            /// <summary>
            /// My Schedule tab
            /// </summary>
            [Description( "My Schedule" )]
            MySchedule = 0,

            /// <summary>
            /// Preferences tab
            /// </summary>
            [Description( "Preferences" )]
            Preferences = 1,

            /// <summary>
            /// Sign-up tab
            /// </summary>
            [Description( "Sign Up" )]
            SignUp = 2
        }

        /// <summary>
        /// Gets or sets the selected person identifier.
        /// </summary>
        /// <value>
        /// The selected person identifier.
        /// </value>
        public int SelectedPersonId
        {
            get
            {
                return hfSelectedPersonId.Value.AsInteger();
            }

            set
            {
                hfSelectedPersonId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the current tab in the ViewState
        /// </summary>
        /// <value>
        /// The current tab.
        /// </value>
        protected GroupScheduleToolboxTab CurrentTab
        {
            get
            {
                if ( ViewState["CurrentTab"] != null )
                {
                    return ( GroupScheduleToolboxTab ) Enum.Parse( typeof( GroupScheduleToolboxTab ), ViewState["CurrentTab"].ToString() );
                }

                return GroupScheduleToolboxTab.MySchedule;
            }

            set
            {
                ViewState["CurrentTab"] = value;
            }
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBlackoutDates.GridRebind += gBlackoutDates_GridRebind;
            gBlackoutDates.Actions.AddClick += gBlackoutDates_AddClick;
            gBlackoutDates.IsDeleteEnabled = true;
            gBlackoutDates.AllowPaging = false;
            gBlackoutDates.AllowSorting = false;
            gBlackoutDates.Actions.ShowAdd = true;
            gBlackoutDates.Actions.ShowExcelExport = false;
            gBlackoutDates.Actions.ShowMergeTemplate = false;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // Setup for being able to copy text to clipboard
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );

            string script = string.Format(
                @"
new ClipboardJS('#{0}');
$('#{0}').tooltip();
",
                btnCopyToClipboard.ClientID );

            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

            this.SetSelectedPerson();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            List<PersonScheduleSignup> availableGroupLocationSchedules = ( this.ViewState["availableGroupLocationSchedulesJSON"] as string ).FromJsonOrNull<List<PersonScheduleSignup>>() ?? new List<PersonScheduleSignup>();

            CreateDynamicSignupControls( availableGroupLocationSchedules );
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
                BindTabs();
                ShowSelectedTab();
            }
        }

        /// <summary>
        /// Sets the selected person.
        /// </summary>
        private void SetSelectedPerson()
        {
            var targetPerson = this.ContextEntity<Person>();
            if ( targetPerson != null )
            {
                lTitle.Text = "Schedule Toolbox - " + targetPerson.FullName;
                this.SelectedPersonId = targetPerson.Id;
            }
            else
            {
                lTitle.Text = "Schedule Toolbox";
                this.SelectedPersonId = this.CurrentPersonId ?? 0;
            }
        }

        #endregion Base Control Methods

        #region Block Events and Methods

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowSelectedTab();
        }

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                CurrentTab = lb.CommandArgument.ConvertToEnum<GroupScheduleToolboxTab>();

                rptTabs.DataSource = _tabs;
                rptTabs.DataBind();
            }

            ShowSelectedTab();
        }

        /// <summary>
        /// Get the class for the tab's selection state. Empty string if not the current tab, "active" if it is the current tab.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( ( GroupScheduleToolboxTab ) property == CurrentTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the tab.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabName( object property )
        {
            return ( ( GroupScheduleToolboxTab ) property ).GetDescription();
        }

        /// <summary>
        /// Shows the panel for the selected tab.
        /// </summary>
        private void ShowSelectedTab()
        {
            switch ( CurrentTab )
            {
                case GroupScheduleToolboxTab.MySchedule:
                    UpdateMySchedulesTab();
                    break;
                case GroupScheduleToolboxTab.Preferences:
                    BindBlackoutDates();
                    BindGroupPreferencesRepeater();
                    break;
                case GroupScheduleToolboxTab.SignUp:
                    List<PersonScheduleSignup> availableGroupLocationSchedules = GetScheduleData();
                    this.ViewState["availableGroupLocationSchedulesJSON"] = availableGroupLocationSchedules.ToJson();
                    phSignUpSchedules.Controls.Clear();
                    CreateDynamicSignupControls( availableGroupLocationSchedules );
                    break;
                default:
                    break;
            }

            pnlMySchedule.Visible = CurrentTab == GroupScheduleToolboxTab.MySchedule;
            pnlPreferences.Visible = CurrentTab == GroupScheduleToolboxTab.Preferences;
            pnlSignup.Visible = CurrentTab == GroupScheduleToolboxTab.SignUp;
        }

        /// <summary>
        /// Binds the tab repeater with the values in the GroupScheduleToolboxTab enum
        /// </summary>
        private void BindTabs()
        {
            rptTabs.DataSource = _tabs;
            rptTabs.DataBind();
        }

        #endregion Block Events and Methods

        #region My Schedule Tab

        /// <summary>
        /// Gets the occurrence details (Date, Group Name, Location)
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns></returns>
        protected string GetOccurrenceDetails( Attendance attendance )
        {
            return string.Format( "{0} - {1} - {2}", attendance.Occurrence.OccurrenceDate.ToShortDateString(), attendance.Occurrence.Group.Name, attendance.Occurrence.Location );
        }

        /// <summary>
        /// Gets the occurrence time.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns></returns>
        protected string GetOccurrenceTime( Attendance attendance )
        {
            return attendance.Occurrence.Schedule.GetCalendarEvent().DTStart.Value.TimeOfDay.ToTimeString();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptUpcomingSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptUpcomingSchedules_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lConfirmedOccurrenceDetails = e.Item.FindControl( "lConfirmedOccurrenceDetails" ) as Literal;
            var lConfirmedOccurrenceTime = e.Item.FindControl( "lConfirmedOccurrenceTime" ) as Literal;
            var btnCancelConfirmAttending = e.Item.FindControl( "btnCancelConfirmAttending" ) as LinkButton;
            var attendance = e.Item.DataItem as Attendance;

            lConfirmedOccurrenceDetails.Text = GetOccurrenceDetails( attendance );
            lConfirmedOccurrenceTime.Text = GetOccurrenceTime( attendance );

            btnCancelConfirmAttending.CommandName = "AttendanceId";
            btnCancelConfirmAttending.CommandArgument = attendance.Id.ToString();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPendingConfirmations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPendingConfirmations_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lPendingOccurrenceDetails = e.Item.FindControl( "lPendingOccurrenceDetails" ) as Literal;
            var lPendingOccurrenceTime = e.Item.FindControl( "lPendingOccurrenceTime" ) as Literal;
            var btnConfirmAttending = e.Item.FindControl( "btnConfirmAttending" ) as LinkButton;
            var btnDeclineAttending = e.Item.FindControl( "btnDeclineAttending" ) as LinkButton;
            var attendance = e.Item.DataItem as Attendance;

            lPendingOccurrenceDetails.Text = GetOccurrenceDetails( attendance );
            lPendingOccurrenceTime.Text = GetOccurrenceTime( attendance );
            btnConfirmAttending.CommandName = "AttendanceId";
            btnConfirmAttending.CommandArgument = attendance.Id.ToString();

            btnDeclineAttending.CommandName = "AttendanceId";
            btnDeclineAttending.CommandArgument = attendance.Id.ToString();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirmAttending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirmAttending_Click( object sender, EventArgs e )
        {
            var btnCancelConfirmAttending = sender as LinkButton;
            int? attendanceId = btnCancelConfirmAttending.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();
                new AttendanceService( rockContext ).ScheduledPersonConfirmCancel( attendanceId.Value );
                rockContext.SaveChanges();
            }

            UpdateMySchedulesTab();
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmAttending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmAttending_Click( object sender, EventArgs e )
        {
            var btnConfirmAttending = sender as LinkButton;
            int? attendanceId = btnConfirmAttending.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();
                new AttendanceService( rockContext ).ScheduledPersonConfirm( attendanceId.Value );
                rockContext.SaveChanges();
            }

            UpdateMySchedulesTab();
        }

        /// <summary>
        /// Handles the Click event of the btnDeclineAttending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeclineAttending_Click( object sender, EventArgs e )
        {
            var btnDeclineAttending = sender as LinkButton;
            int? attendanceId = btnDeclineAttending.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();

                // TODO: Need to provide a way to indicate the reason a pending schedule was declined.
                int? declineReasonValueId = null;

                new AttendanceService( rockContext ).ScheduledPersonDecline( attendanceId.Value, declineReasonValueId );
                rockContext.SaveChanges();
            }

            UpdateMySchedulesTab();
        }

        /// <summary>
        /// Updates my schedules tab.
        /// </summary>
        private void UpdateMySchedulesTab()
        {
            BindPendingConfirmationsGrid();
            BindUpcomingSchedulesGrid();
            nbNoUpcomingSchedules.Visible = pnlPendingConfirmations.Visible == false && pnlUpcomingSchedules.Visible == false;
        }

        /// <summary>
        /// Binds the Upcoming Schedules grid.
        /// </summary>
        private void BindUpcomingSchedulesGrid()
        {
            var currentDateTime = RockDateTime.Now;
            var rockContext = new RockContext();

            var qryConfirmedScheduled = new AttendanceService( rockContext ).GetConfirmedScheduled()
                .Where( a => a.PersonAlias.PersonId == this.SelectedPersonId )
                .Where( a => a.Occurrence.OccurrenceDate >= currentDateTime )
                .OrderBy( a => a.Occurrence.OccurrenceDate );

            var confirmedScheduledList = qryConfirmedScheduled.ToList();

            pnlUpcomingSchedules.Visible = confirmedScheduledList.Any();

            rptUpcomingSchedules.DataSource = confirmedScheduledList;
            rptUpcomingSchedules.DataBind();

            var personAliasService = new PersonAliasService( rockContext );
            var primaryAlias = personAliasService.GetPrimaryAlias( this.SelectedPersonId );
            if ( primaryAlias != null )
            {
                // Set URL in feed button
                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Get();
                btnCopyToClipboard.Attributes["data-clipboard-text"] = string.Format(
                    "{0}GetPersonGroupScheduleFeed.ashx?paguid={1}",
                    globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash(),
                    primaryAlias.Guid );
            }
            btnCopyToClipboard.Disabled = false;
        }

        /// <summary>
        /// Binds the Pending Confirmations grid.
        /// </summary>
        private void BindPendingConfirmationsGrid()
        {
            pnlPendingConfirmations.Visible = false;

            using ( var rockContext = new RockContext() )
            {
                var pendingConfirmations = new AttendanceService( rockContext ).GetPendingScheduledConfirmations()
                    .Where( a => a.PersonAlias.PersonId == this.SelectedPersonId )
                    .OrderBy( a => a.Occurrence.OccurrenceDate )
                    .ToList();

                if ( pendingConfirmations.Any() )
                {
                    pnlPendingConfirmations.Visible = true;
                    rptPendingConfirmations.DataSource = pendingConfirmations;
                    rptPendingConfirmations.DataBind();
                }
            }
        }

        #endregion My Schedule Tab

        #region Preferences Tab

        /// <summary>
        /// Binds the group preferences repeater with a list of groups that the selected person is an active member of and have SchedulingEnabled and have at least one location with a schedule
        /// </summary>
        protected void BindGroupPreferencesRepeater()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );

                // get groups that the selected person is an active member of and have SchedulingEnabled and have at least one location with a schedule
                var groups = groupService
                    .Queryable()
                    .AsNoTracking()
                    .Where( x => x.Members.Any( m => m.PersonId == this.SelectedPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active ) )
                    .Where( x => x.IsActive == true && x.IsArchived == false && x.GroupType.IsSchedulingEnabled == true )
                    .Where( x => x.GroupLocations.Any( gl => gl.Schedules.Any() ) )
                    .OrderBy( x => new { x.Order, x.Name } )
                    .AsNoTracking()
                    .ToList();

                rptGroupPreferences.DataSource = groups;
                rptGroupPreferences.DataBind();

                nbNoScheduledGroups.Visible = groups.Any() == false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSendRemindersDaysOffset control.
        /// Saves the ScheduleReminderEmailOffsetDays for each GroupMember that matches the Group/Person.
        /// In most cases that will be one GroupMember unless the person has multiple roles in the group
        /// (e.g. Leader and Member).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSendRemindersDaysOffset_SelectedIndexChanged( object sender, EventArgs e )
        {
            var repeaterItem = ( ( DropDownList ) sender ).BindingContainer as RepeaterItem;
            var hfGroupId = repeaterItem.FindControl( "hfPreferencesGroupId" ) as HiddenField;
            var groupId = hfGroupId.ValueAsInt();

            int? days = ( ( DropDownList ) sender ).SelectedValueAsInt( true );

            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );

                // if the person is in the group more than once (for example, as a leader and as a member), just get one of the member records, but prefer the record where they have a leader role
                var groupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, this.SelectedPersonId ).OrderBy( a => a.GroupRole.IsLeader ).FirstOrDefault();
                if ( groupMember != null )
                {
                    groupMember.ScheduleReminderEmailOffsetDays = days;
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupMemberScheduleTemplate control.
        /// Saves the ScheduleTemplateId for each GroupMember that matches the Group/Person.
        /// In most cases that will be one GroupMember unless the person has multiple roles in the group
        /// (e.g. Leader and Member)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupMemberScheduleTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            var repeaterItem = ( ( DropDownList ) sender ).BindingContainer as RepeaterItem;
            SaveGroupMemberSchedule( repeaterItem );
        }

        /// <summary>
        /// Saves the group member schedule.
        /// </summary>
        /// <param name="repeaterItem">The repeater item.</param>
        private void SaveGroupMemberSchedule( RepeaterItem repeaterItem )
        {
            // Save the preference. For now this acts as a note to the scheduler and does not effect the list of assignments presented to the user.
            var hfGroupId = repeaterItem.FindControl( "hfPreferencesGroupId" ) as HiddenField;
            var dpGroupMemberScheduleTemplateStartDate = repeaterItem.FindControl( "dpGroupMemberScheduleTemplateStartDate" ) as DatePicker;
            var ddlGroupMemberScheduleTemplate = repeaterItem.FindControl( "ddlGroupMemberScheduleTemplate" ) as DropDownList;
            var groupId = hfGroupId.ValueAsInt();
            int? scheduleTemplateId = ddlGroupMemberScheduleTemplate.SelectedValueAsInt( true );

            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );

                // if the person is in the group more than once (for example, as a leader and as a member), just get one of the member records, but prefer the record where they have a leader role
                var groupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, this.SelectedPersonId ).OrderBy( a => a.GroupRole.IsLeader ).FirstOrDefault();

                if ( groupMember != null )
                {
                    groupMember.ScheduleTemplateId = scheduleTemplateId;

                    // make sure there is a StartDate so the schedule can be based off of something
                    var currentDate = RockDateTime.Now.Date;
                    groupMember.ScheduleStartDate = dpGroupMemberScheduleTemplateStartDate.SelectedDate ?? currentDate;
                    rockContext.SaveChanges();
                }
            }

            var pnlGroupPreferenceAssignment = repeaterItem.FindControl( "pnlGroupPreferenceAssignment" ) as Panel;
            pnlGroupPreferenceAssignment.Visible = scheduleTemplateId.HasValue;
        }

        /// <summary>
        /// Handles the ValueChanged event of the dpGroupMemberScheduleTemplateStartDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dpGroupMemberScheduleTemplateStartDate_ValueChanged( object sender, EventArgs e )
        {
            var repeaterItem = ( ( DatePicker ) sender ).BindingContainer as RepeaterItem;
            SaveGroupMemberSchedule( repeaterItem );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupPreferences_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var group = ( Group ) e.Item.DataItem;
            if ( group == null )
            {
                return;
            }

            var lGroupPreferencesGroupNameHtml = ( Literal ) e.Item.FindControl( "lGroupPreferencesGroupNameHtml" );
            var hfPreferencesGroupId = ( HiddenField ) e.Item.FindControl( "hfPreferencesGroupId" );
            var rptGroupPreferenceAssignments = ( Repeater ) e.Item.FindControl( "rptGroupPreferenceAssignments" );

            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupType != null && groupType.IconCssClass.IsNotNullOrWhiteSpace() )
            {
                lGroupPreferencesGroupNameHtml.Text = string.Format( "<i class='{0}'></i> {1}", groupType.IconCssClass, group.Name );
            }
            else
            {
                lGroupPreferencesGroupNameHtml.Text = group.Name;
            }


            hfPreferencesGroupId.Value = group.Id.ToString();

            rptGroupPreferencesBindDropDowns( group, e );

            // bind repeater rptGroupPreferenceAssignments
            using ( var rockContext = new RockContext() )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var scheduleList = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.GroupId == group.Id )
                    .SelectMany( g => g.Schedules )
                    .Distinct()
                    .ToList();

                List<Schedule> sortedScheduleList = scheduleList.OrderByNextScheduledDateTime();

                rptGroupPreferenceAssignments.DataSource = sortedScheduleList;
                rptGroupPreferenceAssignments.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupPreferenceAssignments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupPreferenceAssignments_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var schedule = e.Item.DataItem as Schedule;
            if ( schedule == null )
            {
                return;
            }

            var hfScheduleId = ( HiddenField ) e.Item.FindControl( "hfScheduleId" );
            hfScheduleId.Value = schedule.Id.ToString();

            var cbGroupPreferenceAssignmentScheduleTime = ( CheckBox ) e.Item.FindControl( "cbGroupPreferenceAssignmentScheduleTime" );

            var repeaterItemGroup = ( ( Repeater ) sender ).BindingContainer as RepeaterItem;
            var hfPreferencesGroupId = ( HiddenField ) repeaterItemGroup.FindControl( "hfPreferencesGroupId" );

            var rockContext = new RockContext();

            // if the person is in the group more than once (for example, as a leader and as a member), just get one of the member records, but prefer the record where they have a leader role
            int? groupMemberId = new GroupMemberService( rockContext )
                .GetByGroupIdAndPersonId( hfPreferencesGroupId.ValueAsInt(), this.SelectedPersonId )
                .AsNoTracking()
                .OrderBy( a => a.GroupRole.IsLeader )
                .Select( gm => ( int? ) gm.Id )
                .FirstOrDefault();

            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            GroupMemberAssignment groupmemberAssignment = groupMemberAssignmentService
                .Queryable()
                .AsNoTracking()
                .Where( x => x.GroupMemberId == groupMemberId )
                .Where( x => x.ScheduleId == schedule.Id )
                .FirstOrDefault();

            cbGroupPreferenceAssignmentScheduleTime.Text = schedule.Name;
            cbGroupPreferenceAssignmentScheduleTime.Checked = groupmemberAssignment != null;

            var ddlGroupPreferenceAssignmentLocation = ( DropDownList ) e.Item.FindControl( "ddlGroupPreferenceAssignmentLocation" );
            var locations = new LocationService( rockContext ).GetByGroupSchedule( schedule.Id, hfPreferencesGroupId.ValueAsInt() )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            ddlGroupPreferenceAssignmentLocation.DataSource = locations;
            ddlGroupPreferenceAssignmentLocation.DataValueField = "Id";
            ddlGroupPreferenceAssignmentLocation.DataTextField = "Name";
            ddlGroupPreferenceAssignmentLocation.DataBind();
            ddlGroupPreferenceAssignmentLocation.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            ddlGroupPreferenceAssignmentLocation.Visible = cbGroupPreferenceAssignmentScheduleTime.Checked;

            if ( groupmemberAssignment != null )
            {
                ddlGroupPreferenceAssignmentLocation.SelectedValue = groupmemberAssignment.LocationId.ToStringSafe();
                ddlGroupPreferenceAssignmentLocation.Items[0].Text = "No Location Preference";
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbGroupPreferenceAssignmentScheduleTime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbGroupPreferenceAssignmentScheduleTime_CheckedChanged( object sender, EventArgs e )
        {
            var scheduleCheckBox = ( CheckBox ) sender;
            var repeaterItemSchedule = scheduleCheckBox.BindingContainer as RepeaterItem;
            var ddlGroupPreferenceAssignmentLocation = ( DropDownList ) repeaterItemSchedule.FindControl( "ddlGroupPreferenceAssignmentLocation" );
            var hfScheduleId = ( HiddenField ) repeaterItemSchedule.FindControl( "hfScheduleId" );

            ddlGroupPreferenceAssignmentLocation.Visible = scheduleCheckBox.Checked;
            ddlGroupPreferenceAssignmentLocation.Items[0].Text = scheduleCheckBox.Checked ? "No Location Preference" : string.Empty;

            var repeaterItemGroup = repeaterItemSchedule.FindFirstParentWhere( p => p is RepeaterItem );
            var hfPreferencesGroupId = ( HiddenField ) repeaterItemGroup.FindControl( "hfPreferencesGroupId" );

            using ( var rockContext = new RockContext() )
            {
                // if the person is in the group more than once (for example, as a leader and as a member), just get one of the member records, but prefer the record where they have a leader role
                var groupId = hfPreferencesGroupId.ValueAsInt();
                var groupMemberId = new GroupMemberService( rockContext ).GetByGroupIdAndPersonId( groupId, this.SelectedPersonId ).OrderBy( a => a.GroupRole.IsLeader ).Select( a => ( int? ) a.Id ).FirstOrDefault();

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

                if ( groupMemberId.HasValue )
                {
                    if ( scheduleCheckBox.Checked )
                    {
                        groupMemberAssignmentService.AddOrUpdate( groupMemberId.Value, hfScheduleId.ValueAsInt() );
                    }
                    else
                    {
                        groupMemberAssignmentService.DeleteByGroupMemberAndSchedule( groupMemberId.Value, hfScheduleId.ValueAsInt() );
                    }
                }

                rockContext.SaveChanges();

            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupPreferenceAssignmentLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupPreferenceAssignmentLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            var locationDropDownList = ( DropDownList ) sender;
            var repeaterItemSchedule = locationDropDownList.BindingContainer as RepeaterItem;
            var hfScheduleId = ( HiddenField ) repeaterItemSchedule.FindControl( "hfScheduleId" );

            var repeaterItemGroup = repeaterItemSchedule.FindFirstParentWhere( p => p is RepeaterItem );
            var hfPreferencesGroupId = ( HiddenField ) repeaterItemGroup.FindControl( "hfPreferencesGroupId" );
            int groupId = hfPreferencesGroupId.ValueAsInt();

            using ( var rockContext = new RockContext() )
            {
                var groupMemberId = new GroupMemberService( rockContext ).GetByGroupIdAndPersonId( groupId, this.SelectedPersonId ).OrderBy( a => a.GroupRole.IsLeader ).Select( a => ( int? ) a.Id ).FirstOrDefault();

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

                if ( groupMemberId.HasValue )
                {
                    groupMemberAssignmentService.AddOrUpdate( groupMemberId.Value, hfScheduleId.ValueAsInt(), locationDropDownList.SelectedValueAsInt() );
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Populates the DropDownLists ddlGroupMemberScheduleTemplate and ddlSendRemindersDaysOffset and
        /// sets the value for the current Person/Group
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupPreferencesBindDropDowns( Group group, RepeaterItemEventArgs e )
        {
            var ddlGroupMemberScheduleTemplate = e.Item.FindControl( "ddlGroupMemberScheduleTemplate" ) as RockDropDownList;
            var dpGroupMemberScheduleTemplateStartDate = e.Item.FindControl( "dpGroupMemberScheduleTemplateStartDate" ) as DatePicker;
            var ddlSendRemindersDaysOffset = e.Item.FindControl( "ddlSendRemindersDaysOffset" ) as RockDropDownList;
            var pnlGroupPreferenceAssignment = e.Item.FindControl( "pnlGroupPreferenceAssignment" ) as Panel;

            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );

                // if the person is in the group more than once (for example, as a leader and as a member), just get one of the member records, but prefer the record where they have a leader role
                var groupMember = groupMemberService.GetByGroupIdAndPersonId( group.Id, this.SelectedPersonId ).OrderBy( a => a.GroupRole.IsLeader ).FirstOrDefault();

                if ( groupMember == null )
                {
                    // shouldn't happen, but just in case
                    return;
                }

                // The items for this are hard coded in the markup, so just set the selected value.
                ddlSendRemindersDaysOffset.SelectedValue = groupMember.ScheduleReminderEmailOffsetDays == null ? string.Empty : groupMember.ScheduleReminderEmailOffsetDays.ToString();

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

                ddlGroupMemberScheduleTemplate.SelectedValue = groupMember.ScheduleTemplateId == null ? string.Empty : groupMember.ScheduleTemplateId.ToString();
                dpGroupMemberScheduleTemplateStartDate.Visible = groupMember.ScheduleTemplateId.HasValue;
                pnlGroupPreferenceAssignment.Visible = groupMember.ScheduleTemplateId.HasValue;

                dpGroupMemberScheduleTemplateStartDate.SelectedDate = groupMember.ScheduleStartDate;
                if ( dpGroupMemberScheduleTemplateStartDate.SelectedDate == null )
                {
                    dpGroupMemberScheduleTemplateStartDate.SelectedDate = RockDateTime.Today;
                }
            }
        }

        #region Preferences Tab Blackout

        /// <summary>
        /// Binds the blackout dates.
        /// </summary>
        protected void BindBlackoutDates()
        {
            gBlackoutDates_BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gBlackoutDates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBlackoutDates_AddClick( object sender, EventArgs e )
        {
            ShowDialog( "mdAddBlackoutDates" );
        }

        /// <summary>
        /// Handles the Click event of the gBlackoutDatesDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBlackoutDatesDelete_Click( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );
                var personScheduleExclusion = personScheduleExclusionService.Get( e.RowKeyId );
                if ( personScheduleExclusion != null )
                {
                    personScheduleExclusionService.Delete( personScheduleExclusion );
                    rockContext.SaveChanges();
                    BindBlackoutDates();
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gBlackoutDates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gBlackoutDates_GridRebind( object sender, GridRebindEventArgs e )
        {
            gBlackoutDates_BindGrid();
        }

        /// <summary>
        /// gs the blackout dates bind grid.
        /// </summary>
        protected void gBlackoutDates_BindGrid()
        {
            var currentDate = DateTime.Now.Date;

            using ( var rockContext = new RockContext() )
            {
                List<int> familyMemberAliasIds = new PersonService( rockContext )
                    .GetFamilyMembers( this.SelectedPersonId, true )
                    .Select( m => m.Person.Aliases.FirstOrDefault( a => a.PersonId == m.PersonId ) )
                    .Select( a => a.Id )
                    .ToList();

                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );
                var personScheduleExclusions = personScheduleExclusionService
                    .Queryable()
                    .AsNoTracking()
                    .Where( e => familyMemberAliasIds.Contains( e.PersonAliasId.Value ) )
                    .Where( e => e.StartDate >= currentDate || e.EndDate >= currentDate )
                    .OrderBy( e => e.StartDate )
                    .ThenBy( e => e.EndDate )
                    .Select( e => new BlackoutDate
                    {
                        ExclusionId = e.Id,
                        PersonAliasId = e.PersonAliasId.Value,
                        StartDate = DbFunctions.TruncateTime( e.StartDate ).Value,
                        EndDate = DbFunctions.TruncateTime( e.EndDate ).Value,
                        FullName = e.PersonAlias.Person.NickName + " " + e.PersonAlias.Person.LastName,
                        GroupName = e.Group.Name
                    } );

                gBlackoutDates.DataSource = personScheduleExclusions.ToList();
                gBlackoutDates.DataBind();
            }
        }

        /// <summary>
        /// POCO to hold blackout dates for the grid.
        /// </summary>
        private class BlackoutDate
        {
            public int ExclusionId { get; set; }

            public int PersonAliasId { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public string DateRange
            {
                get
                {
                    return StartDate.ToString( "M/d/yyyy" ) + " - " + EndDate.ToString( "M/d/yyyy" );
                }
            }

            public string FullName { get; set; }

            public string GroupName
            {
                get
                {
                    return _groupName.IsNullOrWhiteSpace() ? ALL_GROUPS_STRING : _groupName;
                }

                set
                {
                    _groupName = value;
                }
            }

            private string _groupName;
        }

        #region Blackout Dates Modal

        /// <summary>
        /// Loads the group selection ddl for the add blackout dates modal
        /// </summary>
        private void mdAddBlackoutDates_ddlBlackoutGroups_Bind()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var groups = groupMemberService
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.PersonId == this.SelectedPersonId )
                    .Where( g => g.Group.GroupType.IsSchedulingEnabled == true )
                    .Select( g => new { Value = ( int? ) g.GroupId, Text = g.Group.Name } )
                    .ToList();

                groups.Insert( 0, new { Value = ( int? ) null, Text = ALL_GROUPS_STRING } );

                ddlBlackoutGroups.DataSource = groups;
                ddlBlackoutGroups.DataValueField = "Value";
                ddlBlackoutGroups.DataTextField = "Text";
                ddlBlackoutGroups.DataBind();
            }
        }

        /// <summary>
        /// Creates the list of family members that can be assigned a blackout date for the current person
        /// </summary>
        private void mdAddBlackoutDates_cblBlackoutPersons_Bind()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var familyMemberAliasIds = personService
                    .GetFamilyMembers( this.SelectedPersonId )
                    .Select( m => m.Person.Aliases.FirstOrDefault( a => a.PersonId == m.PersonId ) )
                    .Select( a => new { Value = a.Id, Text = a.Person.NickName + " " + a.Person.LastName } )
                    .ToList();

                var selectedPerson = personService.GetNoTracking( this.SelectedPersonId );

                if ( this.SelectedPersonId == this.CurrentPersonId )
                {
                    familyMemberAliasIds.Insert( 0, new { Value = selectedPerson.PrimaryAliasId ?? 0, Text = selectedPerson.FullName + " (you)" } );
                }
                else
                {
                    familyMemberAliasIds.Insert( 0, new { Value = selectedPerson.PrimaryAliasId ?? 0, Text = selectedPerson.FullName } );
                }

                cblBlackoutPersons.DataSource = familyMemberAliasIds;
                cblBlackoutPersons.DataValueField = "Value";
                cblBlackoutPersons.DataTextField = "Text";
                cblBlackoutPersons.DataBind();

                cblBlackoutPersons.Items[0].Selected = true;

                // if there is only one person in the family, don't show the checkbox list since it'll always just be the individual
                cblBlackoutPersons.Visible = cblBlackoutPersons.Items.Count > 1;
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddBlackoutDates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddBlackoutDates_SaveClick( object sender, EventArgs e )
        {
            // parse the date range and add to query
            if ( drpBlackoutDateRange.DelimitedValues.IsNullOrWhiteSpace() )
            {
                // show error
                return;
            }

            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpBlackoutDateRange.DelimitedValues );
            if ( !dateRange.Start.HasValue || !dateRange.End.HasValue )
            {
                // show error
                return;
            }

            int? parentId = null;


            foreach ( ListItem item in cblBlackoutPersons.Items )
            {
                if ( !item.Selected )
                {
                    continue;
                }

                var personScheduleExclusion = new PersonScheduleExclusion
                {
                    PersonAliasId = item.Value.AsInteger(),
                    StartDate = dateRange.Start.Value.Date,
                    EndDate = dateRange.End.Value.Date,
                    GroupId = ddlBlackoutGroups.SelectedValueAsId(),
                    Title = tbBlackoutDateDescription.Text,
                    ParentPersonScheduleExclusionId = parentId
                };

                using ( var rockContext = new RockContext() )
                {
                    new PersonScheduleExclusionService( rockContext ).Add( personScheduleExclusion );
                    rockContext.SaveChanges();

                    if ( parentId == null )
                    {
                        parentId = personScheduleExclusion.Id;
                    }
                }
            }

            HideDialog();
            BindBlackoutDates();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MDADDBLACKOUTDATES":
                    mdAddBlackoutDates.Show();
                    drpBlackoutDateRange.DelimitedValues = string.Empty;
                    tbBlackoutDateDescription.Text = string.Empty;
                    mdAddBlackoutDates_ddlBlackoutGroups_Bind();
                    mdAddBlackoutDates_cblBlackoutPersons_Bind();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MDADDBLACKOUTDATES":
                    mdAddBlackoutDates.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion Blackout Dates Modal

        #endregion Preferences Tab Blackout

        #endregion Preferences Tab

        #region Sign-up Tab

        /// <summary>
        /// Creates the dynamic controls for the sign-up tab.
        /// </summary>
        protected void CreateDynamicSignupControls( List<PersonScheduleSignup> availableGroupLocationSchedules )
        {
            int currentGroupId = -1;
            DateTime currentOccurrenceDate = DateTime.MinValue;
            int currentScheduleId = -1;

            var availableSchedules = availableGroupLocationSchedules
                .GroupBy( s => new { s.GroupId, ScheduleId = s.ScheduleId, s.ScheduledDateTime.Date } )
                .Select( s => s.First() )
                .OrderBy( a => a.GroupOrder )
                .ThenBy( a => a.GroupName )
                .ThenBy( a => a.ScheduledDateTime )
                .ThenBy( a => a.LocationOrder )
                .ThenBy( a => a.LocationName )
                .ToList();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "IsSchedulesAvailable", availableSchedules.Any() );
            mergeFields.Add( "Person", CurrentPerson );
            lSignupMsg.Text = GetAttributeValue( AttributeKeys.SignupInstructions ).ResolveMergeFields( mergeFields );
            
            foreach ( var availableSchedule in availableSchedules )
            {
                if ( availableSchedule.GroupId != currentGroupId )
                {
                    currentGroupId = availableSchedule.GroupId;
                    CreateGroupHeader( availableSchedule.GroupName, availableSchedule.GroupType );
                }

                if ( availableSchedule.ScheduledDateTime.Date != currentOccurrenceDate.Date )
                {
                    if ( currentScheduleId != -1 )
                    {
                        phSignUpSchedules.Controls.Add( new LiteralControl( "</div>" ) );
                    }

                    currentOccurrenceDate = availableSchedule.ScheduledDateTime.Date;
                    CreateDateHeader( currentOccurrenceDate );
                }

                currentScheduleId = availableSchedule.ScheduleId;
                CreateScheduleSignUpRow( availableSchedule, availableGroupLocationSchedules );
            }
        }

        /// <summary>
        /// Creates the group section header for the sign-up tab controls
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        private void CreateGroupHeader( string groupName, GroupTypeCache groupType )
        {
            LiteralControl lc = new LiteralControl( string.Format( "<h3><i class='{0}'></i> {1} Schedules</h3><hr class='margin-t-sm margin-b-sm'>", groupType.IconCssClass, groupName ) );
            phSignUpSchedules.Controls.Add( lc );
        }

        /// <summary>
        /// Creates the date section header for the sign-up tab controls
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        private void CreateDateHeader( DateTime dateTime )
        {
            string date = dateTime.ToShortDateString();
            string dayOfWeek = dateTime.DayOfWeek.ToString();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "<div class='form-control-group'>" );
            sb.AppendLine( string.Format( "<label class='control-label'>{0}&nbsp;({1})</label><br /><br />", date, dayOfWeek ) );
            phSignUpSchedules.Controls.Add( new LiteralControl( sb.ToString() ) );
        }

        /// <summary>
        /// Creates a row for a schedule with a checkbox for the time and a dll to select a location.
        /// </summary>
        /// <param name="personScheduleSignup">The person schedule signup.</param>
        /// <param name="availableGroupLocationSchedules">The available group location schedules.</param>
        private void CreateScheduleSignUpRow( PersonScheduleSignup personScheduleSignup, List<PersonScheduleSignup> availableGroupLocationSchedules )
        {
            var scheduleSignUpRowItem = new HtmlGenericContainer();
            scheduleSignUpRowItem.Attributes.Add( "class", "row" );
            scheduleSignUpRowItem.AddCssClass( "js-person-schedule-signup-row" );
            scheduleSignUpRowItem.AddCssClass( "margin-b-sm" );
            phSignUpSchedules.Controls.Add( scheduleSignUpRowItem );


            var hfGroupId = new HiddenField { ID = "hfGroupId", Value = personScheduleSignup.GroupId.ToString() };
            var hfScheduleId = new HiddenField { ID = "hfScheduleId", Value = personScheduleSignup.ScheduleId.ToString() };
            var hfOccurrenceDate = new HiddenField { ID = "hfOccurrenceDate", Value = personScheduleSignup.ScheduledDateTime.Date.ToISO8601DateString() };
            var hfAttendanceId = new HiddenField { ID = "hfAttendanceId" };
            scheduleSignUpRowItem.Controls.Add( hfGroupId );
            scheduleSignUpRowItem.Controls.Add( hfScheduleId );
            scheduleSignUpRowItem.Controls.Add( hfOccurrenceDate );
            scheduleSignUpRowItem.Controls.Add( hfAttendanceId );

            var pnlCheckboxCol = new Panel();
            pnlCheckboxCol.Attributes.Add( "class", "col-md-4" );

            var cbSignupSchedule = new RockCheckBox();
            cbSignupSchedule.ID = "cbSignupSchedule";
            cbSignupSchedule.Text = personScheduleSignup.ScheduledDateTime.ToString( "hh:mm tt" );
            cbSignupSchedule.ToolTip = personScheduleSignup.ScheduleName;
            cbSignupSchedule.AddCssClass( "js-person-schedule-signup-checkbox" );
            cbSignupSchedule.Checked = false;
            cbSignupSchedule.AutoPostBack = true;
            cbSignupSchedule.CheckedChanged += CbSignupSchedule_CheckedChanged;
            pnlCheckboxCol.Controls.Add( cbSignupSchedule );

            var locations = availableGroupLocationSchedules
                .Where( x => x.GroupId == personScheduleSignup.GroupId )
                .Where( x => x.ScheduleId == personScheduleSignup.ScheduleId )
                .Where( x => x.ScheduledDateTime.Date == personScheduleSignup.ScheduledDateTime.Date )
                .Select( x => new { Name = x.LocationName, Id = x.LocationId } )
                .ToList();

            var ddlSignupLocations = new RockDropDownList();
            ddlSignupLocations.ID = "ddlSignupLocations";
            ddlSignupLocations.Visible = false;

            ddlSignupLocations.AddCssClass( "js-person-schedule-signup-ddl" );
            ddlSignupLocations.Items.Insert( 0, new ListItem( "No Location Preference", string.Empty ) );
            foreach ( var location in locations )
            {
                ddlSignupLocations.Items.Add( new ListItem( location.Name, location.Id.ToString() ) );
            }

            ddlSignupLocations.AutoPostBack = true;
            ddlSignupLocations.SelectedIndexChanged += DdlSignupLocations_SelectedIndexChanged;

            var pnlLocationCol = new Panel();
            pnlLocationCol.Attributes.Add( "class", "col-md-8" );
            pnlLocationCol.Controls.Add( ddlSignupLocations );

            var hlSignUpSaved = new HighlightLabel { ID = "hlSignUpSaved", LabelType = LabelType.Success, Text = "<i class='fa fa-check-square'></i> Saved" };
            hlSignUpSaved.Visible = false;
            pnlLocationCol.Controls.Add( hlSignUpSaved );

            scheduleSignUpRowItem.Controls.Add( pnlCheckboxCol );
            scheduleSignUpRowItem.Controls.Add( pnlLocationCol );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the DdlSignupLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DdlSignupLocations_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateScheduleSignUp( sender as Control );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the CbSignupSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CbSignupSchedule_CheckedChanged( object sender, EventArgs e )
        {
            UpdateScheduleSignUp( sender as Control );
        }

        /// <summary>
        /// Updates the schedule sign up.
        /// </summary>
        /// <param name="senderControl">The sender control.</param>
        private void UpdateScheduleSignUp( Control senderControl )
        {
            HtmlGenericContainer scheduleSignUpContainer = senderControl.FindFirstParentWhere( a => ( a is HtmlGenericContainer ) && ( a as HtmlGenericContainer ).CssClass.Contains( "js-person-schedule-signup-row" ) ) as HtmlGenericContainer;
            if ( scheduleSignUpContainer != null )
            {
                var hfGroupId = scheduleSignUpContainer.FindControl( "hfGroupId" ) as HiddenField;
                var hfScheduleId = scheduleSignUpContainer.FindControl( "hfScheduleId" ) as HiddenField;
                var hfOccurrenceDate = scheduleSignUpContainer.FindControl( "hfOccurrenceDate" ) as HiddenField;
                var hfAttendanceId = scheduleSignUpContainer.FindControl( "hfAttendanceId" ) as HiddenField;
                var ddlSignupLocations = scheduleSignUpContainer.FindControl( "ddlSignupLocations" ) as RockDropDownList;
                var cbSignupSchedule = scheduleSignUpContainer.FindControl( "cbSignupSchedule" ) as RockCheckBox;
                var hlSignUpSaved = scheduleSignUpContainer.FindControl( "hlSignUpSaved" ) as HighlightLabel;
                ddlSignupLocations.Visible = cbSignupSchedule.Checked;

                using ( var rockContext = new RockContext() )
                {
                    var occurrenceDate = hfOccurrenceDate.Value.AsDateTime().Value.Date;
                    var scheduleId = hfScheduleId.Value.AsInteger();
                    var locationId = ddlSignupLocations.SelectedValue.AsIntegerOrNull();
                    var groupId = hfGroupId.Value.AsInteger();
                    var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();
                    AttendanceOccurrence attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).GetOrCreateAttendanceOccurrence( occurrenceDate, scheduleId, locationId, groupId );
                    var attendanceService = new AttendanceService( rockContext );

                    if ( attendanceId.HasValue )
                    {
                        // if there is an attendanceId, this is an attendance that they just signed up for, but they might have either unselected it, or changed the location, so remove it
                        attendanceService.ScheduledPersonRemove( attendanceId.Value );
                    }

                    if ( cbSignupSchedule.Checked )
                    {
                        var attendance = attendanceService.ScheduledPersonAddPending( this.SelectedPersonId, attendanceOccurrence.Id, CurrentPersonAlias );
                        rockContext.SaveChanges();
                        hfAttendanceId.Value = attendance.Id.ToString();
                        attendanceService.ScheduledPersonConfirm( attendance.Id );
                        rockContext.SaveChanges();
                    }

                    hlSignUpSaved.Visible = true;
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets a list of available schedules for the group the current person belongs to.
        /// </summary>
        /// <param name="includeScheduledItems">if set to <c>true</c> [include scheduled items].</param>
        /// <returns></returns>
        protected List<PersonScheduleSignup> GetScheduleData()
        {
            List<PersonScheduleSignup> personScheduleSignups = new List<PersonScheduleSignup>();
            int numOfWeeks = GetAttributeValue( AttributeKeys.FutureWeeksToShow ).AsIntegerOrNull() ?? 6;
            var startDate = DateTime.Now.AddDays( 1 );
            var endDate = DateTime.Now.AddDays( numOfWeeks * 7 );

            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

                var groupLocationService = new GroupLocationService( rockContext );
                var personGroupLocationQry = groupLocationService.Queryable();

                // get GroupLocations that are for Groups that the person is an active member of
                personGroupLocationQry = personGroupLocationQry.Where( a => a.Group.GroupType.IsSchedulingEnabled == true && a.Group.Members.Any( m => m.PersonId == this.SelectedPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                var personGroupLocationList = personGroupLocationQry.ToList();

                foreach ( var personGroupLocation in personGroupLocationList )
                {
                    foreach ( var schedule in personGroupLocation.Schedules )
                    {
                        var startDateTimeList = schedule.GetScheduledStartTimes( startDate, endDate );
                        foreach ( var startDateTime in startDateTimeList )
                        {
                            var occurrenceDate = startDateTime.Date;
                            bool alreadyScheduled = attendanceService.IsScheduled( occurrenceDate, schedule.Id, this.SelectedPersonId );
                            if ( alreadyScheduled )
                            {
                                continue;
                            }

                            if ( personScheduleExclusionService.IsExclusionDate( this.SelectedPersonId, personGroupLocation.GroupId, occurrenceDate ) )
                            {
                                // Don't show dates they have blacked out
                                continue;
                            }

                            // Add to master list personScheduleSignups
                            personScheduleSignups.Add( new PersonScheduleSignup
                            {
                                GroupId = personGroupLocation.Group.Id,
                                GroupOrder = personGroupLocation.Group.Order,
                                GroupName = personGroupLocation.Group.Name,
                                GroupType = GroupTypeCache.Get( personGroupLocation.Group.GroupTypeId ),
                                LocationId = personGroupLocation.Location.Id,
                                LocationName = personGroupLocation.Location.Name,
                                LocationOrder = personGroupLocation.Order,
                                ScheduleId = schedule.Id,
                                ScheduleName = schedule.Name,
                                ScheduledDateTime = startDateTime,
                            } );
                        }
                    }
                }

                return personScheduleSignups;
            }
        }

        protected class PersonScheduleSignup
        {
            public int GroupId { get; set; }
            public int GroupOrder { get; set; }
            public string GroupName { get; set; }
            public GroupTypeCache GroupType { get; set; }
            public int LocationId { get; set; }
            public int ScheduleId { get; set; }
            public DateTime ScheduledDateTime { get; set; }
            public string ScheduleName { get; set; }
            public string LocationName { get; set; }
            public int LocationOrder { get; set; }
        }


        #endregion Sign-up Tab
    }
}
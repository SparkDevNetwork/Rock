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
    /// </summary>
    [DisplayName( "Group Schedule Toolbox" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows management of group scheduling for a specific person (worker)." )]

    [ContextAware( typeof( Rock.Model.Person ) )]

    [IntegerField(
        "Number of Future Weeks To Show",
        Key = AttributeKey.FutureWeeksToShow,
        Description = "The number of weeks into the future to allow users to sign up for a schedule.",
        IsRequired = true,
        DefaultValue = "6",
        Order = 0 )]

    [BooleanField(
        "Enable Sign-up",
        Key = AttributeKey.EnableSignup,
        Description = "Set to false to hide the Sign Up tab.",
        DefaultBooleanValue = true,
        Order = 1 )]

    [CodeEditorField(
        "Sign Up Instructions",
        Key = AttributeKey.SignupInstructions,
        Description = "Instructions here will show up on Sign Up tab. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue =
    @"<div class=""alert alert-info"">
    {%- if IsSchedulesAvailable -%}
        {%- if CurrentPerson.Id == Person.Id -%}
            Sign up to attend a group and location on the given date.
        {%- else -%}
            Sign up {{ Person.FullName }} to attend a group and location on a given date.
        {%- endif -%}
     {%- else -%}
        No sign-ups available.
     {%- endif -%}
</div>",
        Order = 2
         )]

    [LinkedPage( "Decline Reason Page",
        Description = "If the group type has enabled 'RequiresReasonIfDeclineSchedule' then specify the page to provide that reason here.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.SCHEDULE_CONFIRMATION,
        Key = AttributeKey.DeclineReasonPage,
        Order = 3 )]

    [BooleanField( "Scheduler Receive Confirmation Emails",
        Description = "If checked, the scheduler will receive an email response for each confirmation or decline.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.SchedulerReceiveConfirmationEmails )]

    [SystemCommunicationField( "Scheduling Response Email",
        Description = "The system email that will be used for sending responses back to the scheduler.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SCHEDULING_RESPONSE,
        Key = AttributeKey.SchedulingResponseEmail,
        Order = 5 )]

    [Rock.SystemGuid.BlockTypeGuid( "7F9CEA6F-DCE5-4F60-A551-924965289F1D" )]
    public partial class GroupScheduleToolbox : RockBlock
    {
        protected class AttributeKey
        {
            public const string FutureWeeksToShow = "FutureWeeksToShow";
            public const string EnableSignup = "EnableSignup";
            public const string SignupInstructions = "SignupInstructions";
            public const string DeclineReasonPage = "DeclineReasonPage";
            public const string SchedulerReceiveConfirmationEmails = "SchedulerReceiveConfirmationEmails";
            public const string SchedulingResponseEmail = "SchedulingResponseEmail";
        }

        protected const string ALL_GROUPS_STRING = "All Groups";

        protected const string NO_LOCATION_PREFERENCE = "No Location Preference";

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
            else
            {
                var eventTarget = this.Request.Params["__EVENTTARGET"];
                var eventArgument = this.Request.Params["__EVENTARGUMENT"];
                if ( eventArgument.IsNotNullOrWhiteSpace() )
                {
                    var eventArgumentParts = eventArgument.Split( '|' );
                    if ( eventArgumentParts.Length == 2 )
                    {
                        if ( eventArgumentParts[0] == "Add_GroupPreferenceAssignment" )
                        {
                            var groupArgument = eventArgumentParts[1];

                            // groupArgument will be in the format 'GroupId:5'
                            var groupId = groupArgument.Replace( "GroupId:", string.Empty ).AsInteger();
                            AddEditGroupPreferenceAssignment( groupId, null );
                        }
                    }
                }
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
            BindTabs();
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

                BindTabs();
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
            // Make sure the parent panels are visible before adjusting the child objects.
            pnlMySchedule.Visible = CurrentTab == GroupScheduleToolboxTab.MySchedule;
            pnlPreferences.Visible = CurrentTab == GroupScheduleToolboxTab.Preferences;
            pnlSignup.Visible = CurrentTab == GroupScheduleToolboxTab.SignUp;

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
        }

        /// <summary>
        /// Binds the tab repeater with the values in the GroupScheduleToolboxTab enum
        /// </summary>
        private void BindTabs()
        {
            var tabs = Enum.GetValues( typeof( GroupScheduleToolboxTab ) ).Cast<GroupScheduleToolboxTab>().ToList();
            if ( this.GetAttributeValue( AttributeKey.EnableSignup ).AsBoolean() == false )
            {
                tabs = tabs.Where( a => a != GroupScheduleToolboxTab.SignUp ).ToList();
            }

            rptTabs.DataSource = tabs;
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
            if ( attendance.Occurrence.GroupId == null && attendance.Occurrence.LocationId == null )
            {
                return attendance.Occurrence.OccurrenceDate.ToShortDateString();
            }

            if ( attendance.Occurrence.GroupId == null )
            {
                return string.Format( "{0} - {1}", attendance.Occurrence.OccurrenceDate.ToShortDateString(), attendance.Occurrence.Location );
            }

            if ( attendance.Occurrence.LocationId == null )
            {
                return attendance.Occurrence.OccurrenceDate.ToShortDateString();
            }

            return string.Format( "{0} - {1} - {2}", attendance.Occurrence.OccurrenceDate.ToShortDateString(), attendance.Occurrence.Group.Name, attendance.Occurrence.Location );
        }

        /// <summary>
        /// Gets the occurrence schedule's name.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns>The name of the schedule</returns>
        protected string GetOccurrenceScheduleName( Attendance attendance )
        {
            return attendance.Occurrence.Schedule.Name;
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
            var btnCancelConfirmAttend = e.Item.FindControl( "btnCancelConfirmAttend" ) as LinkButton;
            var attendance = e.Item.DataItem as Attendance;

            lConfirmedOccurrenceDetails.Text = GetOccurrenceDetails( attendance );
            lConfirmedOccurrenceTime.Text = GetOccurrenceScheduleName( attendance );

            btnCancelConfirmAttend.CommandName = "AttendanceId";
            btnCancelConfirmAttend.CommandArgument = attendance.Id.ToString();
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
            var btnConfirmAttend = e.Item.FindControl( "btnConfirmAttend" ) as LinkButton;
            var btnDeclineAttend = e.Item.FindControl( "btnDeclineAttend" ) as LinkButton;
            var attendance = e.Item.DataItem as Attendance;

            lPendingOccurrenceDetails.Text = GetOccurrenceDetails( attendance );
            lPendingOccurrenceTime.Text = GetOccurrenceScheduleName( attendance );
            btnConfirmAttend.CommandName = "AttendanceId";
            btnConfirmAttend.CommandArgument = attendance.Id.ToString();

            btnDeclineAttend.CommandName = "AttendanceId";
            btnDeclineAttend.CommandArgument = attendance.Id.ToString();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirmAttend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirmAttend_Click( object sender, EventArgs e )
        {
            /*
                9/25/2023 - JPH

                We are no longer calling this handler when canceling a previously-accepted attendance.
                Instead, we'll call the `btnDeclineAttend_Click` handler, so the attendance will be
                declined rather than putting it back into the "pending" state, which leads to confusion.

                Reason: Group Schedule Toolbox - 'Cancel' Behavior
                (https://app.asana.com/0/1174768427585341/1205304349766829/f)
             */
            var btnCancelConfirmAttend = sender as LinkButton;
            int? attendanceId = btnCancelConfirmAttend.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();
                new AttendanceService( rockContext ).ScheduledPersonConfirmCancel( attendanceId.Value );
                rockContext.SaveChanges();
            }

            UpdateMySchedulesTab();
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmAttend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmAttend_Click( object sender, EventArgs e )
        {
            var btnConfirmAttend = sender as LinkButton;
            int? attendanceId = btnConfirmAttend.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();
                new AttendanceService( rockContext ).ScheduledPersonConfirm( attendanceId.Value );
                rockContext.SaveChanges();
            }

            UpdateMySchedulesTab();
        }

        /// <summary>
        /// Handles the Click event of the btnDeclineAttend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeclineAttend_Click( object sender, EventArgs e )
        {
            var btnDeclineAttend = sender as LinkButton;
            int? attendanceId = btnDeclineAttend.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var requiresDeclineReason = attendanceService.Get( attendanceId.Value ).Occurrence.Group.GroupType.RequiresReasonIfDeclineSchedule;

                if ( requiresDeclineReason )
                {
                    var queryParams = new Dictionary<string, string>
                    {
                        { "attendanceId", attendanceId.Value.ToString() },
                        { "isConfirmed", "false" },
                        { "ReturnUrl", this.RockPage.Guid.ToString() }
                    };

                    NavigateToLinkedPage( AttributeKey.DeclineReasonPage, queryParams );
                }
                else
                {
                    attendanceService.ScheduledPersonDecline( attendanceId.Value, null );
                    rockContext.SaveChanges();
                    try
                    {
                        var schedulingResponseEmailGuid = GetAttributeValue( AttributeKey.SchedulingResponseEmail ).AsGuid();

                        // The scheduler receives email add as a recipient for both Confirmation and Decline
                        if ( GetAttributeValue( AttributeKey.SchedulerReceiveConfirmationEmails ).AsBoolean() )
                        {
                            attendanceService.SendScheduledPersonResponseEmailToScheduler( attendanceId.Value, schedulingResponseEmailGuid );
                        }

                        attendanceService.SendScheduledPersonDeclineEmail( attendanceId.Value, schedulingResponseEmailGuid );
                    }
                    catch ( SystemException ex )
                    {
                        // intentionally ignore exception
                        ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
                    }
                }
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
            var currentDateTime = RockDateTime.Now.Date;
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
                    globalAttributes.GetValue( "PublicApplicationRoot" ),
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
                    .Where( x => x.IsActive == true && x.IsArchived == false
                        && x.GroupType.IsSchedulingEnabled == true
                        && x.DisableScheduling == false
                        && x.DisableScheduleToolboxAccess == false )
                    .Where( x => x.GroupLocations.Any( gl => gl.Schedules.Any() ) )
                    .OrderBy( x => new { x.Order, x.Name } )
                    .AsNoTracking()
                    .ToList();

                rptGroupPreferences.DataSource = groups;
                rptGroupPreferences.DataBind();

                pnlBlackoutDates.Visible = groups.Any();
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
                var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.SelectedPersonId );
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

                var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.SelectedPersonId );

                if ( groupMember != null )
                {
                    groupMember.ScheduleTemplateId = scheduleTemplateId;

                    // make sure there is a StartDate so the schedule can be based off of something
                    var currentDate = RockDateTime.Now.Date;
                    groupMember.ScheduleStartDate = dpGroupMemberScheduleTemplateStartDate.SelectedDate ?? currentDate;
                    rockContext.SaveChanges();
                }
            }

            dpGroupMemberScheduleTemplateStartDate.Visible = scheduleTemplateId.HasValue && scheduleTemplateId > 0;

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
            var gGroupPreferenceAssignments = ( Grid ) e.Item.FindControl( "gGroupPreferenceAssignments" );

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

            // bind grid gGroupPreferenceAssignments
            using ( var rockContext = new RockContext() )
            {
                var groupId = hfPreferencesGroupId.Value.AsInteger();
                int? groupMemberId = null;
                var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.SelectedPersonId );
                if ( groupMember != null )
                {
                    groupMemberId = groupMember.Id;
                }

                var groupLocationService = new GroupLocationService( rockContext );

                var qryGroupLocations = groupLocationService
                    .Queryable()
                    .Where( g => g.GroupId == group.Id );

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                var groupMemberAssignmentQuery = groupMemberAssignmentService
                    .Queryable()
                    .AsNoTracking()
                    .Where( x =>
                        x.GroupMemberId == groupMemberId
                        && (
                            !x.LocationId.HasValue
                            || qryGroupLocations.Any( gl => gl.LocationId == x.LocationId && gl.Schedules.Any( s => s.Id == x.ScheduleId ) )
                        ) );

                // Calculate the Next Start Date Time based on the start of the week so that schedule columns are in the correct order
                var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );

                var groupMemberAssignmentList = groupMemberAssignmentQuery
                    .Include( a => a.Schedule )
                    .Include( a => a.Location )
                    .AsNoTracking()
                    .ToList()
                    .OrderBy( a => a.Schedule.Order )
                    .ThenBy( a => a.Schedule.GetNextStartDateTime( occurrenceDate ) )
                    .ThenBy( a => a.Schedule.Name )
                    .ThenBy( a => a.Schedule.Id )
                    .ThenBy( a => a.LocationId.HasValue ? a.Location.ToString( true ) : string.Empty )
                    .ToList();

                gGroupPreferenceAssignments.DataKeyNames = new string[1] { "Id" };
                gGroupPreferenceAssignments.Actions.ShowAdd = true;

                // use the ClientAddScript option since this grid is created in a repeater and a normal postback won't wire up correctly
                gGroupPreferenceAssignments.Actions.ClientAddScript = string.Format(
                    @"window.location = ""javascript:__doPostBack( '{0}', 'Add_GroupPreferenceAssignment|GroupId:{1}' )""",
                    upnlContent.ClientID,
                    group.Id );

                gGroupPreferenceAssignments.DataSource = groupMemberAssignmentList;
                gGroupPreferenceAssignments.DataBind();
            }
        }

        /// <summary>
        /// Adds the edit group preference assignment.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberAssignmentId">The group member assignment identifier.</param>
        private void AddEditGroupPreferenceAssignment( int groupId, int? groupMemberAssignmentId )
        {
            hfGroupScheduleAssignmentId.Value = groupMemberAssignmentId.ToString();

            // bind repeater rptGroupPreferenceAssignments
            using ( var rockContext = new RockContext() )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var scheduleList = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.GroupId == groupId )
                    .SelectMany( g => g.Schedules )
                    .Distinct()
                    .ToList();

                List<Schedule> sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

                int? selectedScheduleId = null;
                int? selectedLocationId = null;

                GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                int? groupMemberId = null;

                if ( groupMemberAssignmentId.HasValue )
                {
                    GroupMemberAssignment groupMemberAssignment = groupMemberAssignmentService.Get( groupMemberAssignmentId.Value );
                    if ( groupMemberAssignment != null )
                    {
                        groupMemberId = groupMemberAssignment.GroupMemberId;
                        selectedScheduleId = groupMemberAssignment.ScheduleId;
                        selectedLocationId = groupMemberAssignment.LocationId;
                    }
                }

                hfGroupScheduleAssignmentGroupId.Value = groupId.ToString();
                hfGroupScheduleAssignmentId.Value = groupMemberAssignmentId.ToString();

                // get the groupMemberId record for the selectedPerson,Group (if the person is in there twice, prefer the IsLeader role
                if ( !groupMemberId.HasValue )
                {
                    var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.SelectedPersonId );
                    if ( groupMember != null )
                    {
                        groupMemberId = groupMember.Id;
                    }
                }

                if ( !groupMemberId.HasValue )
                {
                    // shouldn't happen
                    return;
                }

                var configuredScheduleIds = groupMemberAssignmentService.Queryable()
                    .Where( a => a.GroupMemberId == groupMemberId.Value && a.ScheduleId.HasValue )
                    .Select( s => s.ScheduleId.Value ).Distinct().ToList();

                // limit to schedules that haven't had a schedule preference set yet
                sortedScheduleList = sortedScheduleList.Where( a =>
                    a.IsActive
                    && a.IsPublic.HasValue
                    && a.IsPublic.Value
                    && ( !configuredScheduleIds.Contains( a.Id )
                    || ( selectedScheduleId.HasValue && a.Id == selectedScheduleId.Value ) ) ).ToList();

                ddlGroupScheduleAssignmentSchedule.Items.Clear();
                ddlGroupScheduleAssignmentSchedule.Items.Add( new ListItem() );
                foreach ( var schedule in sortedScheduleList )
                {
                    var scheduleListItem = new ListItem( schedule.Name, schedule.Id.ToString() );
                    if ( selectedScheduleId.HasValue && selectedScheduleId.Value == schedule.Id )
                    {
                        scheduleListItem.Selected = true;
                    }

                    ddlGroupScheduleAssignmentSchedule.Items.Add( scheduleListItem );
                }

                PopulateGroupScheduleAssignmentLocations( groupId, selectedScheduleId );
                ddlGroupScheduleAssignmentLocation.SetValue( selectedLocationId );

                mdGroupScheduleAssignment.Show();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEditGroupPreferenceAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnEditGroupPreferenceAssignment_Click( object sender, RowEventArgs e )
        {
            int groupMemberAssignmentId = e.RowKeyId;
            var groupMemberAssignmentGroupId = new GroupMemberAssignmentService( new RockContext() ).GetSelect( groupMemberAssignmentId, a => ( int? ) a.GroupMember.GroupId ) ?? 0;
            AddEditGroupPreferenceAssignment( groupMemberAssignmentGroupId, groupMemberAssignmentId );
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteGroupPreferenceAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnDeleteGroupPreferenceAssignment_Click( object sender, RowEventArgs e )
        {
            int groupMemberAssignmentId = e.RowKeyId;
            var rockContext = new RockContext();
            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

            var groupMemberAssignment = groupMemberAssignmentService.Get( groupMemberAssignmentId );

            if ( groupMemberAssignment != null )
            {
                string errorMessage;
                if ( groupMemberAssignmentService.CanDelete( groupMemberAssignment, out errorMessage ) )
                {
                    groupMemberAssignmentService.Delete( groupMemberAssignment );
                    rockContext.SaveChanges();
                }
                else
                {
                    // shouldn't happen
                }
            }

            BindGroupPreferencesRepeater();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupScheduleAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupScheduleAssignment_SaveClick( object sender, EventArgs e )
        {
            var groupMemberAssignmentId = hfGroupScheduleAssignmentId.Value.AsIntegerOrNull();
            var groupId = hfGroupScheduleAssignmentGroupId.Value.AsIntegerOrNull();
            var rockContext = new RockContext();
            GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            GroupMemberAssignment groupMemberAssignment = null;
            GroupMember groupMember = null;
            if ( groupMemberAssignmentId.HasValue )
            {
                groupMemberAssignment = groupMemberAssignmentService.GetInclude( groupMemberAssignmentId.Value, a => a.GroupMember );
            }

            var groupMemberService = new GroupMemberService( rockContext );

            if ( groupMemberAssignment != null )
            {
                groupMember = groupMemberAssignment.GroupMember;
            }
            else
            {
                groupMember = this.GetGroupMemberRecord( rockContext, groupId.Value, this.SelectedPersonId );
            }

            var scheduleId = ddlGroupScheduleAssignmentSchedule.SelectedValue.AsIntegerOrNull();
            var locationId = ddlGroupScheduleAssignmentLocation.SelectedValue.AsIntegerOrNull();

            // schedule is required, but locationId can be null (which means no location specified )
            if ( !scheduleId.HasValue )
            {
                return;
            }

            if ( groupMemberAssignment == null )
            {
                // just in case there is already is groupMemberAssignment, update it
                groupMemberAssignment = groupMemberAssignmentService.GetByGroupMemberScheduleAndLocation( groupMember, scheduleId, locationId );

                if ( groupMemberAssignment == null )
                {
                    groupMemberAssignment = new GroupMemberAssignment();
                    groupMemberAssignmentService.Add( groupMemberAssignment );
                }
            }
            else
            {
                bool alreadyAssigned = groupMemberAssignmentService.GetByGroupMemberScheduleAndLocation( groupMember, scheduleId, locationId ) != null;

                if ( alreadyAssigned )
                {
                    return;
                }
            }

            groupMemberAssignment.GroupMemberId = groupMember.Id;
            groupMemberAssignment.ScheduleId = scheduleId;
            groupMemberAssignment.LocationId = locationId;

            rockContext.SaveChanges();
            BindGroupPreferencesRepeater();
            mdGroupScheduleAssignment.Hide();
        }

        /// <summary>
        /// Populates the group schedule assignment locations.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        private void PopulateGroupScheduleAssignmentLocations( int groupId, int? scheduleId )
        {
            int? selectedLocationId = ddlGroupScheduleAssignmentLocation.SelectedValue.AsIntegerOrNull();
            ddlGroupScheduleAssignmentLocation.Items.Clear();
            ddlGroupScheduleAssignmentLocation.Items.Add( new ListItem( NO_LOCATION_PREFERENCE, NO_LOCATION_PREFERENCE ) );
            if ( scheduleId.HasValue )
            {
                var locations = new LocationService( new RockContext() ).GetByGroupSchedule( scheduleId.Value, groupId )
                    .OrderBy( a => a.Name )
                    .Select( a => new
                    {
                        a.Id,
                        a.Name
                    } ).ToList();

                foreach ( var location in locations )
                {
                    var locationListItem = new ListItem( location.Name, location.Id.ToString() );
                    if ( selectedLocationId.HasValue && location.Id == selectedLocationId.Value )
                    {
                        locationListItem.Selected = true;
                    }

                    ddlGroupScheduleAssignmentLocation.Items.Add( locationListItem );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupScheduleAssignmentSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupScheduleAssignmentSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupId = hfGroupScheduleAssignmentGroupId.Value.AsInteger();
            PopulateGroupScheduleAssignmentLocations( groupId, ddlGroupScheduleAssignmentSchedule.SelectedValue.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gGroupPreferenceAssignments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroupPreferenceAssignments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            GroupMemberAssignment groupMemberAssignment = e.Row.DataItem as GroupMemberAssignment;
            if ( groupMemberAssignment == null )
            {
                return;
            }

            var lScheduleName = e.Row.FindControl( "lScheduleName" ) as Literal;
            var lLocationName = e.Row.FindControl( "lLocationName" ) as Literal;
            lScheduleName.Text = groupMemberAssignment.Schedule.Name;
            if ( groupMemberAssignment.LocationId.HasValue )
            {
                lLocationName.Text = groupMemberAssignment.Location.ToString( true );
            }
            else
            {
                lLocationName.Text = NO_LOCATION_PREFERENCE;
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
            ddlGroupPreferenceAssignmentLocation.Items[0].Text = scheduleCheckBox.Checked ? NO_LOCATION_PREFERENCE : string.Empty;

            var repeaterItemGroup = repeaterItemSchedule.FindFirstParentWhere( p => p is RepeaterItem );
            var hfPreferencesGroupId = ( HiddenField ) repeaterItemGroup.FindControl( "hfPreferencesGroupId" );

            using ( var rockContext = new RockContext() )
            {
                // if the person is in the group more than once (for example, as a leader and as a member), just get one of the member records, but prefer the record where they have a leader role
                var groupId = hfPreferencesGroupId.ValueAsInt();
                var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.SelectedPersonId );

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

                if ( groupMember != null )
                {
                    if ( scheduleCheckBox.Checked )
                    {
                        groupMemberAssignmentService.AddOrUpdate( groupMember.Id, hfScheduleId.ValueAsInt() );
                    }
                    else
                    {
                        groupMemberAssignmentService.DeleteByGroupMemberAndSchedule( groupMember.Id, hfScheduleId.ValueAsInt() );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the GroupMember record for <see cref="SelectedPersonId" /> and specified groupId.
        /// If the person is in there more than once, prefer the IsLeader role.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private GroupMember GetGroupMemberRecord( RockContext rockContext, int groupId, int? personId )
        {
            if ( !personId.HasValue )
            {
                return null;
            }

            var groupMemberQuery = new GroupMemberService( rockContext )
             .GetByGroupIdAndPersonId( groupId, personId.Value );

            var groupMember = groupMemberQuery.OrderBy( a => a.GroupRole.IsLeader ).FirstOrDefault();

            return groupMember;
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
                var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.SelectedPersonId );

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

                if ( groupMember != null )
                {
                    groupMemberAssignmentService.AddOrUpdate( groupMember.Id, hfScheduleId.ValueAsInt(), locationDropDownList.SelectedValueAsInt() );
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
                var groupMember = this.GetGroupMemberRecord( rockContext, group.Id, this.SelectedPersonId );

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
                    var scheduleExclusionChildren = personScheduleExclusionService.Queryable().Where( x => x.ParentPersonScheduleExclusionId == personScheduleExclusion.Id );
                    foreach ( var scheduleExclusionChild in scheduleExclusionChildren )
                    {
                        scheduleExclusionChild.ParentPersonScheduleExclusionId = null;
                    }

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
            var currentDate = RockDateTime.Now.Date;

            using ( var rockContext = new RockContext() )
            {
                var familyMemberAliasIds = new PersonService( rockContext )
                    .GetFamilyMembers( this.SelectedPersonId, true )
                    .SelectMany( m => m.Person.Aliases )
                    .Select( a => a.Id ).ToList();

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
                    .Where( g => g.Group.IsActive == true
                        && g.PersonId == this.SelectedPersonId
                        && g.Group.GroupType.IsSchedulingEnabled == true
                        && g.Group.DisableScheduling == false
                        && g.Group.DisableScheduleToolboxAccess == false )
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

            var selectedPerson = new PersonService( new RockContext() ).GetNoTracking( this.SelectedPersonId );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "IsSchedulesAvailable", availableSchedules.Any() );
            mergeFields.Add( "Person", selectedPerson );
            lSignupMsg.Text = GetAttributeValue( AttributeKey.SignupInstructions ).ResolveMergeFields( mergeFields );

            foreach ( var availableSchedule in availableSchedules )
            {
                if ( availableSchedule.GroupId != currentGroupId )
                {
                    if ( currentGroupId != -1 )
                    {
                        phSignUpSchedules.Controls.Add( new LiteralControl( "</div>" ) );
                    }

                    CreateGroupHeader( availableSchedule.GroupName, availableSchedule.GroupType );
                }

                if ( availableSchedule.ScheduledDateTime.Date != currentOccurrenceDate.Date )
                {
                    if ( currentScheduleId != -1 && availableSchedule.GroupId == currentGroupId )
                    {
                        phSignUpSchedules.Controls.Add( new LiteralControl( "</div>" ) );
                    }

                    currentOccurrenceDate = availableSchedule.ScheduledDateTime.Date;
                    CreateDateHeader( currentOccurrenceDate );
                }

                currentGroupId = availableSchedule.GroupId;
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
            string date = dateTime.ToLongDateString();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "<div class='form-control-group margin-b-lg'>" );
            sb.AppendLine( string.Format( "<div class=\"clearfix\"><label class='control-label'>{0}</label></div>", date ) );
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
            scheduleSignUpRowItem.Attributes.Add( "class", "row d-flex flex-wrap align-items-center" );
            scheduleSignUpRowItem.AddCssClass( "js-person-schedule-signup-row" );
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
            pnlCheckboxCol.Attributes.Add( "class", "col-xs-12 col-sm-5 col-md-4" );

            var cbSignupSchedule = new RockCheckBox();
            cbSignupSchedule.ID = "cbSignupSchedule";
            cbSignupSchedule.DisplayInline = true;
            cbSignupSchedule.Text = personScheduleSignup.ScheduledDateTime.ToShortTimeString();
            cbSignupSchedule.ToolTip = personScheduleSignup.ScheduleName;
            cbSignupSchedule.AddCssClass( "js-person-schedule-signup-checkbox" );
            cbSignupSchedule.Checked = false;
            cbSignupSchedule.AutoPostBack = true;
            cbSignupSchedule.CheckedChanged += CbSignupSchedule_CheckedChanged;
            cbSignupSchedule.Enabled = !personScheduleSignup.MaxScheduled;

            if ( personScheduleSignup.PeopleNeeded > 0 )
            {
                cbSignupSchedule.Text += $" <span class='schedule-signup-people-needed text-muted small'>({personScheduleSignup.PeopleNeeded} {"person".PluralizeIf( personScheduleSignup.PeopleNeeded != 1 )} needed)</span>";
            }
            else if ( personScheduleSignup.MaxScheduled )
            {
                cbSignupSchedule.Text += " <span class='text-muted small'>(filled)</span>";
            }
            
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
            ddlSignupLocations.AddCssClass( "input-sm" );
            ddlSignupLocations.AddCssClass( "my-1" );
            ddlSignupLocations.Items.Insert( 0, new ListItem( NO_LOCATION_PREFERENCE, string.Empty ) );
            foreach ( var location in locations )
            {
                ddlSignupLocations.Items.Add( new ListItem( location.Name, location.Id.ToString() ) );
            }

            ddlSignupLocations.AutoPostBack = true;
            ddlSignupLocations.SelectedIndexChanged += DdlSignupLocations_SelectedIndexChanged;

            var pnlLocationCol = new Panel();
            pnlLocationCol.Attributes.Add( "class", "col-xs-12 col-sm-7 col-md-8 col-lg-6 mb-3 mb-md-0" );
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
                    AttendanceOccurrence attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).GetOrAdd( occurrenceDate, groupId, locationId, scheduleId );
                    var attendanceService = new AttendanceService( rockContext );

                    if ( attendanceId.HasValue )
                    {
                        // if there is an attendanceId, this is an attendance that they just signed up for,
                        // but they might have either unselected it, or changed the location, so remove it
                        var attendance = attendanceService.Get( attendanceId.Value );
                        if ( attendance != null )
                        {
                            attendanceService.Delete( attendance );
                        }
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
            int numOfWeeks = GetAttributeValue( AttributeKey.FutureWeeksToShow ).AsIntegerOrNull() ?? 6;
            var startDate = DateTime.Now.AddDays( 1 ).Date;
            var endDate = DateTime.Now.AddDays( numOfWeeks * 7 );

            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var groupService = new GroupService( rockContext );
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

                var groupLocationService = new GroupLocationService( rockContext );
                var personGroupLocationQry = groupLocationService.Queryable().AsNoTracking();

                // get GroupLocations that are for Groups that the person is an active member of
                personGroupLocationQry = personGroupLocationQry.Where( a => a.Group.IsArchived == false
                    && a.Group.GroupType.IsSchedulingEnabled == true
                    && a.Group.DisableScheduling == false
                    && a.Group.DisableScheduleToolboxAccess == false
                    && a.Group.Members.Any( m => m.PersonId == this.SelectedPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                var personGroupLocationList = personGroupLocationQry.ToList();
                var groupsThatHaveSchedulingRequirements = personGroupLocationQry.Where( a => a.Group.SchedulingMustMeetRequirements ).Select( a => a.Group ).Distinct().ToList();
                var personDoesntMeetSchedulingRequirementGroupIds = new HashSet<int>();

                foreach ( var groupThatHasSchedulingRequirements in groupsThatHaveSchedulingRequirements )
                {
                    var personDoesntMeetSchedulingRequirements = groupService.GroupMembersNotMeetingRequirements( groupThatHasSchedulingRequirements, false, false )
                        .Where( a => a.Key.PersonId == this.SelectedPersonId )
                        .Any();

                    if ( personDoesntMeetSchedulingRequirements )
                    {
                        personDoesntMeetSchedulingRequirementGroupIds.Add( groupThatHasSchedulingRequirements.Id );
                    }
                }

                foreach ( var personGroupLocation in personGroupLocationList )
                {
                    foreach ( var schedule in personGroupLocation.Schedules.Where( a => ( a.IsPublic ?? true ) && a.IsActive ) )
                    {
                        // Find if this has max volunteers here.
                        int maximumCapacitySetting = 0;
                        int desiredCapacitySetting = 0;
                        int minimumCapacitySetting = 0;
                        int desiredOrMinimumNeeded = 0;
                        if ( personGroupLocation.GroupLocationScheduleConfigs.Any() )
                        {
                            var groupConfigs = personGroupLocationList.Where( x => x.GroupId == personGroupLocation.GroupId ).Select( x => x.GroupLocationScheduleConfigs );
                            foreach ( var groupConfig in groupConfigs )
                            {
                                foreach ( var config in groupConfig )
                                {
                                    if ( config.ScheduleId == schedule.Id )
                                    {
                                        maximumCapacitySetting += config.MaximumCapacity ?? 0;
                                        desiredCapacitySetting += config.DesiredCapacity ?? 0;
                                        minimumCapacitySetting += config.MinimumCapacity ?? 0;
                                    }
                                }
                            }

                            desiredOrMinimumNeeded = Math.Max( desiredCapacitySetting, minimumCapacitySetting );
                        }

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

                            if ( personDoesntMeetSchedulingRequirementGroupIds.Contains( personGroupLocation.GroupId ) )
                            {
                                // don't show groups that have scheduling requirements that the person hasn't met
                                continue;
                            }

                            // Get count of scheduled Occurrences with RSVP "Yes" for the group/schedule
                            int currentScheduled = attendanceService
                                .Queryable()
                                .Where( a => a.Occurrence.OccurrenceDate == startDateTime.Date
                                    && a.Occurrence.ScheduleId == schedule.Id
                                    && a.RSVP == RSVP.Yes
                                    && a.Occurrence.GroupId == personGroupLocation.GroupId )
                                .Count();

                            bool maxScheduled = maximumCapacitySetting != 0 && currentScheduled >= maximumCapacitySetting;
                            int peopleNeeded = desiredOrMinimumNeeded != 0 ? desiredOrMinimumNeeded - currentScheduled : 0;

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
                                MaxScheduled = maxScheduled,
                                PeopleNeeded = peopleNeeded < 0 ? 0 : peopleNeeded
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

            public bool MaxScheduled { get; set; }

            public int PeopleNeeded { get; set; }
        }

        #endregion Sign-up Tab
    }
}
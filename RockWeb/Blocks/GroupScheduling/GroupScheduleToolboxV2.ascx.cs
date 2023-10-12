﻿// <copyright>
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
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    /// Allows management of group scheduling for a specific person (worker).
    /// </summary>
    [DisplayName( "Group Schedule Toolbox v2" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows management of group scheduling for a specific person (worker)." )]

    #region Block Attributes

    [ContextAware( typeof( Rock.Model.Person ) )]

    [SlidingDateRangeField(
        "Future Week Date Range",
        Key = AttributeKey.FutureWeekDateRange,
        Description = "The date range of future weeks to allow users to sign up for a schedule. Please note that only future dates will be accepted.",
        IsRequired = true,
        DefaultValue = "Next|6|Week||",
        Order = 0 )]

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
    {% if IsSchedulesAvailable %}
        {% if CurrentPerson.Id == Person.Id %}
            Sign up to attend a group and location on the given date.
        {% else %}
            Sign up {{ Person.FullName }} to attend a group and location on a given date.
        {% endif %}
     {%- else -%}
        No sign-ups available.
     {% endif %}
</div>",
        Order = 1
         )]

    [LinkedPage( "Decline Reason Page",
        Description = "If the group type has enabled 'RequiresReasonIfDeclineSchedule' then specify the page to provide that reason here.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.SCHEDULE_CONFIRMATION,
        Key = AttributeKey.DeclineReasonPage,
        Order = 2 )]

    [BooleanField( "Scheduler Receive Confirmation Emails",
        Description = "If checked, the scheduler will receive an email response for each confirmation or decline.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.SchedulerReceiveConfirmationEmails )]

    [SystemCommunicationField( "Scheduling Response Email",
        Description = "The system email that will be used for sending responses back to the scheduler.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SCHEDULING_RESPONSE,
        Key = AttributeKey.SchedulingResponseEmail,
        Order = 4 )]

    [CodeEditorField(
        "Action Header Lava Template",
        Key = AttributeKey.ActionHeaderLavaTemplate,
        Description = "The content to show between the scheduled items and the action buttons. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = @"<h4>Actions</h4>",
        Order = 5 )]

    [BooleanField( "Enable Additional Time Sign-Up",
        Description = "When enabled, a button will allow the individual to sign up for upcoming schedules for their group.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.EnableAdditionalTimeSignUp )]

    [TextField( "Additional Time Sign-Up Button Text",
        Description = "The text to display for the Additional Time Sign-Up button.",
        DefaultValue = "Sign-Up for Additional Times",
        Order = 7,
        Key = AttributeKey.AdditionalTimeSignUpButtonText )]

    [BooleanField( "Enable Update Schedule Preferences",
        Description = "When enabled, a button will allow the individual to set their group reminder preferences and preferred schedule.",
        DefaultBooleanValue = true,
        Order = 8,
        Key = AttributeKey.EnableUpdateSchedulePreferences )]

    [TextField( "Update Schedule Preferences Button Text",
        Description = "The text to display for the Update Schedule Preferences button.",
        DefaultValue = "Update Schedule Preferences",
        Order = 9,
        Key = AttributeKey.UpdateSchedulePreferencesButtonText )]

    [BooleanField( "Enable Schedule Unavailability",
        Description = "When enabled, a button will allow the individual to specify dates or date ranges when they will be unavailable to serve.",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.EnableScheduleUnavailability )]

    [TextField( "Schedule Unavailability Button Text",
        Description = "The text to display for the Schedule Unavailability button.",
        DefaultValue = "Schedule Unavailability",
        Order = 11,
        Key = AttributeKey.ScheduleUnavailabilityButtonText )]

    [BooleanField( "Override Hide from Toolbox",
        Description = " When enabled this setting will show all schedule enabled groups no matter what their 'Disable Schedule Toolbox Access' setting is set to.",
        DefaultBooleanValue = false,
        Order = 12,
        Key = AttributeKey.OverrideHideFromToolbox )]

    [CodeEditorField(
        "Schedule Unavailability Header",
        Key = AttributeKey.ScheduleUnavailabilityHeader,
        Description = "Header content to put on the Schedule Unavailability panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = HeaderLavaTemplateDefaultValue,
        Order = 13 )]

    [CodeEditorField(
        "Update Schedule Preferences Header",
        Key = AttributeKey.UpdateSchedulePreferencesHeader,
        Description = "Header content to put on the Update Schedule Preferences panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = HeaderLavaTemplateDefaultValue,
        Order = 14 )]

    [CodeEditorField(
        "Sign-up for Additional Times Header",
        Key = AttributeKey.SignupforAdditionalTimesHeader,
        Description = "Header content to put on the Sign-up for Additional Times panel. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = HeaderLavaTemplateDefaultValue,
        Order = 15 )]

    [BooleanField(
        "Require Location for Additional Sign-ups",
        Key = AttributeKey.RequireLocationForAdditionalSignups,
        Description = "When enabled, a location will be required when signing up for additional times.",
        DefaultBooleanValue = false,
        Order = 16 )]

    [CustomDropdownListField(
        "Schedule List Format",
        Key = AttributeKey.ScheduleListFormat,
        ListSource = "1^Schedule Time,2^Schedule Name,3^Schedule Time and Name",
        IsRequired = false,
        DefaultValue = "1",
        Order = 17 )]

    [GroupTypesField(
        "Include Group Types",
        Key = AttributeKey.IncludeGroupTypes,
        Description = "The group types to display in the list. If none are selected, all group types will be included.",
        IsRequired = false,
        Order = 18 )]

    [GroupTypesField(
        "Exclude Group Types",
        Key = AttributeKey.ExcludeGroupTypes,
        Description = "The group types to exclude from the list (only valid if including all groups).",
        IsRequired = false,
        Order = 19 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "18A6DCE3-376C-4A62-B1DD-5E5177C11595" )]
    public partial class GroupScheduleToolboxV2 : RockBlock
    {
        #region Constants

        #region Keys

        protected class AttributeKey
        {
            public const string FutureWeekDateRange = "FutureWeekDateRange";
            public const string SignupInstructions = "SignupInstructions";
            public const string DeclineReasonPage = "DeclineReasonPage";
            public const string SchedulerReceiveConfirmationEmails = "SchedulerReceiveConfirmationEmails";
            public const string SchedulingResponseEmail = "SchedulingResponseEmail";
            public const string ActionHeaderLavaTemplate = "ActionHeaderLavaTemplate";
            public const string EnableAdditionalTimeSignUp = "EnableAdditionalTimeSignUp";
            public const string AdditionalTimeSignUpButtonText = "AdditionalTimeSignUpButtonText";
            public const string EnableUpdateSchedulePreferences = "EnableUpdateSchedulePreferences";
            public const string UpdateSchedulePreferencesButtonText = "UpdateSchedulePreferencesButtonText";
            public const string EnableScheduleUnavailability = "EnableScheduleUnavailability";
            public const string ScheduleUnavailabilityButtonText = "ScheduleUnavailabilityButtonText";
            public const string OverrideHideFromToolbox = "OverrideHideFromToolbox";
            public const string ScheduleUnavailabilityHeader = "ScheduleUnavailabilityHeader";
            public const string UpdateSchedulePreferencesHeader = "UpdateSchedulePreferencesHeader";
            public const string SignupforAdditionalTimesHeader = "SignupforAdditionalTimesHeader";
            public const string RequireLocationForAdditionalSignups = "RequireLocationForAdditionalSignups";
            public const string ScheduleListFormat = "ScheduleListFormat";
            public const string IncludeGroupTypes = "IncludeGroupTypes";
            public const string ExcludeGroupTypes = "ExcludeGroupTypes";
        }

        /// <summary>
        /// Keys used to access values stored in the ViewState obj.
        /// </summary>
        private static class ViewStateKey
        {
            public const string AvailableGroupLocationSchedulesJSON = "AvailableGroupLocationSchedulesJSON";
        }

        #endregion Keys

        protected const string ALL_GROUPS_STRING = "All Groups";
        protected const string NO_LOCATION_PREFERENCE = "No Location Preference";
        protected const string HeaderLavaTemplateDefaultValue = @"
<p>
    <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>
</p>";

        #endregion Constants

        #region Properties

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

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            // wire up page navigate
            RockPage page = Page as RockPage;
            if ( page != null )
            {
                page.PageNavigate += page_PageNavigate;
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            List<GroupScheduleSignup> availableGroupLocationSchedules = ( this.ViewState[ViewStateKey.AvailableGroupLocationSchedulesJSON] as string ).FromJsonOrNull<List<GroupScheduleSignup>>() ?? new List<GroupScheduleSignup>();

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
                BindScheduleRepeater();
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

        /// <summary>
        /// Handles the ItemDataBound event of the rScheduleCards control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rScheduleCards_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lScheduleDate = e.Item.FindControl( "lScheduleDate" ) as Literal;
            var lScheduleCardDetails = e.Item.FindControl( "lScheduleCardDetails" ) as Literal;
            var pnlPending = e.Item.FindControl( "pnlPending" ) as Panel;
            var pnlSideMenu = e.Item.FindControl( "pnlSideMenu" ) as Panel;
            var groupScheduleRowInfo = e.Item.DataItem as GroupScheduleRowInfo;

            var scheduleDate = groupScheduleRowInfo.OccurrenceStartDate.ToShortDateString();
            if ( groupScheduleRowInfo.GroupScheduleType == GroupScheduleType.Unavailable )
            {
                var days = ( groupScheduleRowInfo.OccurrenceEndDate - groupScheduleRowInfo.OccurrenceStartDate ).Days;
                if ( days > 1 )
                {
                    scheduleDate = $"<span class='schedule-date'>{scheduleDate} - {groupScheduleRowInfo.OccurrenceEndDate.ToShortDateString()}</span>";
                }
                else
                {
                    scheduleDate = $"<span class='schedule-date'>{scheduleDate}</span>";
                }
            }
            else
            {
                scheduleDate = $"<span class='schedule-date'>{scheduleDate}</span>";
            }

            lScheduleDate.Text = scheduleDate;
            lScheduleCardDetails.Text = GetOccurrenceDetails( groupScheduleRowInfo );
            if ( groupScheduleRowInfo.GroupScheduleType == GroupScheduleType.Pending )
            {
                pnlPending.Visible = true;
                pnlSideMenu.Visible = false;
                var btnConfirmAttend = e.Item.FindControl( "btnConfirmAttend" ) as LinkButton;
                var btnDeclineAttend = e.Item.FindControl( "btnDeclineAttend" ) as LinkButton;
                btnConfirmAttend.CommandArgument = groupScheduleRowInfo.Id.ToString();
                btnDeclineAttend.CommandArgument = groupScheduleRowInfo.Id.ToString();
            }
            else
            {
                pnlPending.Visible = false;
                pnlSideMenu.Visible = true;
                var btnCancelConfirmAttend = e.Item.FindControl( "btnCancelConfirmAttend" ) as LinkButton;
                var btnDeleteScheduleExclusion = e.Item.FindControl( "btnDeleteScheduleExclusion" ) as LinkButton;
                var hlScheduleType = e.Item.FindControl( "hlScheduleType" ) as HighlightLabel;
                btnCancelConfirmAttend.CommandArgument = groupScheduleRowInfo.Id.ToString();
                btnDeleteScheduleExclusion.CommandArgument = groupScheduleRowInfo.Id.ToString();
                if ( groupScheduleRowInfo.GroupScheduleType == GroupScheduleType.Unavailable )
                {
                    btnDeleteScheduleExclusion.Visible = true;
                    btnCancelConfirmAttend.Visible = false;
                    hlScheduleType.Text = "Unavailable";
                    hlScheduleType.LabelType = LabelType.Default;
                }
                else
                {
                    btnDeleteScheduleExclusion.Visible = false;
                    btnCancelConfirmAttend.Visible = true;
                    hlScheduleType.Text = "Confirmed";
                    hlScheduleType.LabelType = LabelType.Success;
                }
            }
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
                var attendanceService = new AttendanceService( rockContext );
                attendanceService.ScheduledPersonConfirm( attendanceId.Value );
                rockContext.SaveChanges();

                try
                {
                    var schedulingResponseEmailGuid = GetAttributeValue( AttributeKey.SchedulingResponseEmail ).AsGuid();
                    // The scheduler receives email add as a recipient for both Confirmation and Decline
                    if ( GetAttributeValue( AttributeKey.SchedulerReceiveConfirmationEmails ).AsBoolean() )
                    {
                        attendanceService.SendScheduledPersonResponseEmailToScheduler( attendanceId.Value, schedulingResponseEmailGuid );
                    }
                }
                catch ( SystemException ex )
                {
                    // intentionally ignore exception
                    ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
                }
            }

            BindScheduleRepeater();
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

            BindScheduleRepeater();
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

            BindScheduleRepeater();
        }

        /// <summary>
        /// Handles the Click event of the btnScheduleUnavailability control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduleUnavailability_Click( object sender, EventArgs e )
        {
            SetNavigationHistory( pnlUnavailabilitySchedule );
            NavigateToScheduleUnavailability();
        }

        private void NavigateToScheduleUnavailability()
        {
            pnlToolbox.Visible = false;
            pnlSignup.Visible = false;
            pnlPreferences.Visible = false;
            pnlUnavailabilitySchedule.Visible = true;
            drpUnavailabilityDateRange.DelimitedValues = string.Empty;
            tbUnavailabilityDateDescription.Text = string.Empty;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            lUnavailabilityScheduleHeader.Text = GetAttributeValue( AttributeKey.ScheduleUnavailabilityHeader ).ResolveMergeFields( mergeFields );
            BindUnavailabilityGroups();
            BindPersonsForUnavailabilitySchedule();
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
        /// Handles the Click event of the btnUpdateSchedulePreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateSchedulePreferences_Click( object sender, EventArgs e )
        {
            SetNavigationHistory( pnlPreferences );
            NavigateToUpdateSchedulePreferences();
        }

        private void NavigateToUpdateSchedulePreferences()
        {
            pnlToolbox.Visible = false;
            pnlSignup.Visible = false;
            pnlPreferences.Visible = true;
            pnlUnavailabilitySchedule.Visible = false;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            lPreferencesHeader.Text = GetAttributeValue( AttributeKey.UpdateSchedulePreferencesHeader ).ResolveMergeFields( mergeFields );
            BindGroupPreferencesRepeater();
        }

        /// <summary>
        /// Handles the Click event of the btnSignUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSignUp_Click( object sender, EventArgs e )
        {
            SetNavigationHistory( pnlSignup );
            NavigateToSignUp();
        }

        private void NavigateToSignUp()
        {
            pnlToolbox.Visible = false;
            pnlSignup.Visible = true;
            pnlPreferences.Visible = false;
            pnlUnavailabilitySchedule.Visible = false;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            lSignupHeader.Text = GetAttributeValue( AttributeKey.SignupforAdditionalTimesHeader ).ResolveMergeFields( mergeFields );
            LoadSignupFamilyMembersDropDown();

            var selectedSignupPersonId = ddlSignUpSchedulesFamilyMembers.SelectedValueAsId() ?? this.SelectedPersonId;
            UpdateSignupControls( selectedSignupPersonId );
        }

        /// <summary>
        /// Gets the future week date range.
        /// </summary>
        /// <returns>DateRange.</returns>
        private DateRange GetFutureWeekDateRange()
        {
            var currentDay = RockDateTime.Today;
            var futureWeekDateRangeDelimitedValues = this.GetAttributeValue( AttributeKey.FutureWeekDateRange );
            var futureWeekDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( futureWeekDateRangeDelimitedValues );

            // Must start at least 1 day in the future.
            var minimumStartDay = currentDay.AddDays(1);
            if ( !futureWeekDateRange.Start.HasValue || futureWeekDateRange.Start.Value <= minimumStartDay )
            {
                futureWeekDateRange.Start = minimumStartDay;
            }

            // Must include at least this week.
            var minimumEndDay = currentDay.AddDays(7);
            if ( !futureWeekDateRange.End.HasValue || futureWeekDateRange.End.Value <= minimumEndDay )
            {
                futureWeekDateRange.End = minimumEndDay;
            }

            return futureWeekDateRange;
        }

        /// <summary>
        /// Updates the signup controls.
        /// </summary>
        /// <param name="selectedSignupPersonId">The selected signup person identifier.</param>
        private void UpdateSignupControls( int selectedSignupPersonId )
        {
            List<GroupScheduleSignup> availableGroupLocationSchedules = GetScheduleSignupData( selectedSignupPersonId );
            this.ViewState[ViewStateKey.AvailableGroupLocationSchedulesJSON] = availableGroupLocationSchedules.ToJson();
            phSignUpSchedules.Controls.Clear();
            CreateDynamicSignupControls( availableGroupLocationSchedules );
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
            lScheduleName.Text = GetFormattedScheduleForListing( groupMemberAssignment.Schedule.Name, groupMemberAssignment.Schedule.StartTimeOfDay );
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
                    @"javascript:__doPostBack( '{0}', 'Add_GroupPreferenceAssignment|GroupId:{1}' ); return false;",
                    upnlContent.ClientID,
                    group.Id );

                gGroupPreferenceAssignments.DataSource = groupMemberAssignmentList;
                gGroupPreferenceAssignments.DataBind();
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
                groupMember = GetGroupMemberRecord( rockContext, groupId.Value, this.SelectedPersonId );
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
                    mdGroupScheduleAssignment.Hide();
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
        /// Handles the Click event of the btnDeleteScheduleExclusion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnDeleteScheduleExclusion_Click( object sender, EventArgs e )
        {
            var btnDeleteScheduleExclusion = sender as LinkButton;
            var scheduleExclusionId = btnDeleteScheduleExclusion.CommandArgument.AsIntegerOrNull();
            if ( scheduleExclusionId.HasValue )
            {
                var rockContext = new RockContext();
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );
                var personScheduleExclusion = personScheduleExclusionService.Get( scheduleExclusionId.Value );

                if ( personScheduleExclusion != null )
                {
                    var scheduleExclusionChildren = personScheduleExclusionService.Queryable().Where( x => x.ParentPersonScheduleExclusionId == personScheduleExclusion.Id );
                    foreach ( var scheduleExclusionChild in scheduleExclusionChildren )
                    {
                        scheduleExclusionChild.ParentPersonScheduleExclusionId = null;
                    }

                    personScheduleExclusionService.Delete( personScheduleExclusion );
                    rockContext.SaveChanges();
                }
            }

            BindScheduleRepeater();
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Handles the PageNavigate event of the page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        void page_PageNavigate( object sender, HistoryEventArgs e )
        {
            var navigationPanelId = e.State["navigationPanelId"];

            Panel navigationPanel = null;

            if ( navigationPanelId == null )
            {
                navigationPanelId = pnlToolbox.ID;
            }

            if ( navigationPanelId != null )
            {
                navigationPanel = this.FindControl( navigationPanelId ) as Panel;
            }

            if ( navigationPanel != null )
            {
                if ( navigationPanel == pnlToolbox )
                {
                    BindScheduleRepeater();
                }
                else if ( navigationPanel == pnlPreferences )
                {
                    NavigateToUpdateSchedulePreferences();
                }
                else if ( navigationPanel == pnlUnavailabilitySchedule )
                {

                    NavigateToScheduleUnavailability();
                }
                else if ( navigationPanel == pnlSignup )
                {
                    NavigateToSignUp();
                }
                else
                {
                    BindScheduleRepeater();
                }
            }
        }

        /// <summary>
        /// Sets the navigation history.
        /// </summary>
        /// <param name="currentPanel">The panel that should be navigated to when this history point is loaded
        private void SetNavigationHistory( Panel navigateToPanel )
        {
            this.AddHistory( "navigationPanelId", navigateToPanel.ID );
        }

        /// <summary>
        /// Gets the occurrence details (Group Name, Location)
        /// </summary>
        /// <param name="groupScheduleRowInfo">The groupScheduleRowInfo.</param>
        /// <returns></returns>
        private string GetOccurrenceDetails( GroupScheduleRowInfo groupScheduleRowInfo )
        {
            var occurenceDetail = "<span class='schedule-occurrence'>";

            if ( groupScheduleRowInfo.PersonAlias != null && groupScheduleRowInfo.PersonAlias.Person != null && groupScheduleRowInfo.PersonAlias.PersonId != this.SelectedPersonId )
            {
                occurenceDetail = $"{groupScheduleRowInfo.PersonAlias.Person.FullName} - ";
            }

            if ( groupScheduleRowInfo.Group != null )
            {
                occurenceDetail += groupScheduleRowInfo.Group.Name;
            }
            else if ( groupScheduleRowInfo.GroupScheduleType == GroupScheduleType.Unavailable )
            {
                occurenceDetail += ALL_GROUPS_STRING;
            }

            if ( groupScheduleRowInfo.Location != null )
            {
                if ( occurenceDetail.IsNotNullOrWhiteSpace() )
                {
                    occurenceDetail += " - ";
                }

                occurenceDetail += groupScheduleRowInfo.Location.ToString( true );
            }

            occurenceDetail += "</span><span class='schedule-occurrence-schedule'>" + GetOccurrenceScheduleName( groupScheduleRowInfo ) + "</span>";

            return occurenceDetail;
        }

        /// <summary>
        /// Gets the occurrence schedule's name.
        /// </summary>
        /// <param name="groupScheduleRowInfo">The groupScheduleRowInfo.</param>
        /// <returns>The name of the schedule</returns>
        private string GetOccurrenceScheduleName( GroupScheduleRowInfo groupScheduleRowInfo )
        {
            if ( groupScheduleRowInfo.Schedule != null )
            {
                return groupScheduleRowInfo.Schedule.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// Binds the Pending Confirmations grid.
        /// </summary>
        private void BindScheduleRepeater()
        {
            pnlToolbox.Visible = true;
            pnlSignup.Visible = false;
            pnlPreferences.Visible = false;
            pnlUnavailabilitySchedule.Visible = false;
            var rockContext = new RockContext();
            var scheduleList = new AttendanceService( rockContext )
                .GetPendingScheduledConfirmations()
                .Where( a => a.PersonAlias.PersonId == this.SelectedPersonId )
                .Select( a => new GroupScheduleRowInfo
                {
                    Id = a.Id,
                    OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                    Group = a.Occurrence.Group,
                    Schedule = a.Occurrence.Schedule,
                    Location = a.Occurrence.Location,
                    GroupScheduleType = GroupScheduleType.Pending
                } ).ToList();

            var currentDateTime = RockDateTime.Now.Date;
            var confirmedScheduledList = new AttendanceService( rockContext )
                .GetConfirmedScheduled()
                .Where( a => a.Occurrence.OccurrenceDate >= currentDateTime
                        && a.PersonAlias.PersonId == this.SelectedPersonId )
                .Select( a => new GroupScheduleRowInfo
                {
                    Id = a.Id,
                    OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                    Group = a.Occurrence.Group,
                    Schedule = a.Occurrence.Schedule,
                    Location = a.Occurrence.Location,
                    GroupScheduleType = GroupScheduleType.Upcoming
                } );

            scheduleList.AddRange( confirmedScheduledList.ToList() );

            var familyMemberAliasIds = new PersonService( rockContext )
                  .GetFamilyMembers( this.SelectedPersonId, true )
                  .SelectMany( m => m.Person.Aliases )
                  .Select( a => a.Id ).ToList();

            var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );
            var personScheduleExclusions = personScheduleExclusionService
                .Queryable( "PersonAlias.Person" )
                .AsNoTracking()
                .Where( e => familyMemberAliasIds.Contains( e.PersonAliasId.Value )
                       && ( e.StartDate >= currentDateTime || e.EndDate >= currentDateTime ) )
                .OrderBy( e => e.StartDate )
                .ThenBy( e => e.EndDate )
                .Select( e => new GroupScheduleRowInfo
                {
                    Id = e.Id,
                    OccurrenceStartDate = DbFunctions.TruncateTime( e.StartDate ).Value,
                    OccurrenceEndDate = DbFunctions.TruncateTime( e.EndDate ).Value,
                    Group = e.Group,
                    PersonAlias = e.PersonAlias,
                    GroupScheduleType = GroupScheduleType.Unavailable
                } );

            scheduleList.AddRange( personScheduleExclusions.ToList() );

            rScheduleCards.DataSource = scheduleList.OrderBy( a => a.OccurrenceStartDate );
            rScheduleCards.DataBind();

            if ( confirmedScheduledList.Any() )
            {
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

            var enableAdditionalTimeSignUp = GetAttributeValue( AttributeKey.EnableAdditionalTimeSignUp ).AsBoolean();
            var enableScheduleUnavailability = GetAttributeValue( AttributeKey.EnableScheduleUnavailability ).AsBoolean();
            var enableUpdateSchedulePreferences = GetAttributeValue( AttributeKey.EnableUpdateSchedulePreferences ).AsBoolean();
            var actionsEnabled = enableAdditionalTimeSignUp || enableScheduleUnavailability || enableUpdateSchedulePreferences;
            int enabledActionCount = 0;

            btnSignUp.Visible = enableAdditionalTimeSignUp;
            if ( enableAdditionalTimeSignUp )
            {
                enabledActionCount++;
                btnSignUp.Text = GetAttributeValue( AttributeKey.AdditionalTimeSignUpButtonText );
            }

            btnScheduleUnavailability.Visible = enableScheduleUnavailability;
            if ( enableScheduleUnavailability )
            {
                enabledActionCount++;
                btnScheduleUnavailability.Text = GetAttributeValue( AttributeKey.ScheduleUnavailabilityButtonText );
            }

            btnUpdateSchedulePreferences.Visible = enableUpdateSchedulePreferences;
            if ( enableUpdateSchedulePreferences )
            {
                enabledActionCount++;
                btnUpdateSchedulePreferences.Text = GetAttributeValue( AttributeKey.UpdateSchedulePreferencesButtonText );
            }

            lActionHeader.Visible = actionsEnabled;
            if ( actionsEnabled )
            {
                if ( enabledActionCount > 1 )
                {
                    // Show the action header.
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
                    lActionHeader.Text = GetAttributeValue( AttributeKey.ActionHeaderLavaTemplate ).ResolveMergeFields( mergeFields );
                }
                else
                {
                    // If only one action is enabled, start on the panel for that action.
                    if ( enableAdditionalTimeSignUp )
                    {
                        NavigateToSignUp();
                    }
                    else if ( enableScheduleUnavailability )
                    {
                        NavigateToScheduleUnavailability();
                    }
                    else if ( enableUpdateSchedulePreferences )
                    {
                        NavigateToUpdateSchedulePreferences();
                    }
                }
            }
        }

        #region Preferences

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
        /// Binds the group preferences repeater with a list of groups that the selected person is an active member of and have SchedulingEnabled and have at least one location with a schedule
        /// </summary>
        private void BindGroupPreferencesRepeater()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var overrideHideFromToolbox = GetAttributeValue( AttributeKey.OverrideHideFromToolbox ).AsBoolean();
                // get groups that the selected person is an active member of and have SchedulingEnabled and have at least one location with a schedule
                var qry = groupService
                    .Queryable()
                    .AsNoTracking()
                    .Where( x => x.Members.Any( m => m.PersonId == this.SelectedPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active )
                        && x.IsActive == true && x.IsArchived == false
                        && x.GroupType.IsSchedulingEnabled == true
                        && x.DisableScheduling == false
                        && ( overrideHideFromToolbox || x.DisableScheduleToolboxAccess == false )
                        && x.GroupLocations.Any( gl => gl.Schedules.Any() ) );

                List<Guid> includeGroupTypeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
                List<Guid> excludeGroupTypeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();

                if ( includeGroupTypeGuids.Count > 0 )
                {
                    qry = qry.Where( t => includeGroupTypeGuids.Contains( t.GroupType.Guid ) );
                }
                else if ( excludeGroupTypeGuids.Count > 0 )
                {
                    qry = qry.Where( t => !excludeGroupTypeGuids.Contains( t.GroupType.Guid ) );
                }

                var groups = qry.OrderBy( x => new { x.Order, x.Name } ).AsNoTracking().ToList();

                rptGroupPreferences.DataSource = groups;
                rptGroupPreferences.DataBind();

                nbNoScheduledGroups.Visible = groups.Any() == false;
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
                    || ( selectedScheduleId.HasValue
                        && a.Id == selectedScheduleId.Value ) ) )
                 .ToList();

                ddlGroupScheduleAssignmentSchedule.Items.Clear();
                ddlGroupScheduleAssignmentSchedule.Items.Add( new ListItem() );
                foreach ( var schedule in sortedScheduleList )
                {
                    var scheduleName = GetFormattedScheduleForListing( schedule.Name, schedule.StartTimeOfDay );
                    var scheduleListItem = new ListItem( scheduleName, schedule.Id.ToString() );
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
        /// Gets the formatted schedule name used for listing.
        /// </summary>
        /// <param name="scheduleName">The schedule name.</param>
        /// <param name="startTimeOfDay">The start time of day.</param>
        private string GetFormattedScheduleForListing( string scheduleName, TimeSpan startTimeOfDay )
        {
            var formattedScheduleName = string.Empty;
            var scheduleListFormat = GetAttributeValue( AttributeKey.ScheduleListFormat ).AsInteger();
            if ( scheduleListFormat == 1 )
            {
                formattedScheduleName = startTimeOfDay.ToTimeString();
            }
            else if ( scheduleListFormat == 2 )
            {
                formattedScheduleName = scheduleName;
            }
            else
            {
                formattedScheduleName = $"{startTimeOfDay.ToTimeString()} {scheduleName}";
            }

            return formattedScheduleName;
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

        #endregion Preferences

        #region Sign-up Tab

        /// <summary>
        /// Loads the signup family members drop down.
        /// </summary>
        /// <param name="availableGroupLocationSchedules">The available group location schedules.</param>
        private void LoadSignupFamilyMembersDropDown()
        {
            var rockContext = new RockContext();

            var personService = new PersonService( rockContext );

            var overrideHideFromToolbox = GetAttributeValue( AttributeKey.OverrideHideFromToolbox ).AsBoolean();
            var familyMembersQuery = personService.GetFamilyMembers( this.SelectedPersonId, false );

            var groupMemberQuery = new GroupMemberService( rockContext ).Queryable()
                .Where( a => a.Group.GroupType.IsSchedulingEnabled
                    && ( overrideHideFromToolbox || a.Group.DisableScheduleToolboxAccess == false )
                    && a.Group.DisableScheduling == false
                    && a.Group.IsActive
                    && a.Group.IsArchived == false
                    && a.GroupMemberStatus == GroupMemberStatus.Active
                    && a.IsArchived == false );

            var familyMemberPersonsInGroups = familyMembersQuery
                .Where( fm => groupMemberQuery.Any( gm => fm.PersonId == gm.PersonId ) )
                .Select( a => a.Person )
                .AsNoTracking()
                .ToList();

            ddlSignUpSchedulesFamilyMembers.Items.Clear();

            var selectedPerson = personService.GetNoTracking( this.SelectedPersonId );
            ddlSignUpSchedulesFamilyMembers.Items.Add( new ListItem( selectedPerson.FullName, selectedPerson.Id.ToString() ) );

            foreach ( var familyMemberPerson in familyMemberPersonsInGroups )
            {
                ddlSignUpSchedulesFamilyMembers.Items.Add( new ListItem( familyMemberPerson.FullName, familyMemberPerson.Id.ToString() ) );
            }

            // if there is only one person in the family, don't show since it'll always just be the individual
            ddlSignUpSchedulesFamilyMembers.Visible = ddlSignUpSchedulesFamilyMembers.Items.Count > 1;
        }

        /// <summary>
        /// Creates the dynamic controls for the sign-up tab.
        /// </summary>
        private void CreateDynamicSignupControls( List<GroupScheduleSignup> availableGroupLocationSchedules )
        {
            int currentGroupId = -1;
            DateTime currentOccurrenceDate = DateTime.MinValue;
            int currentScheduleId = -1;

            var availableSchedules = availableGroupLocationSchedules
                .GroupBy( s => new { s.GroupId, ScheduleId = s.ScheduleId, s.ScheduledDateTime.Date } )
                .Select( s => s.First() )
                .OrderBy( a => a.GroupName )
                .ThenBy( a => a.GroupId )
                .ThenBy( a => a.ScheduledDateTime )
                .ThenBy( a => a.LocationOrder )
                .ThenBy( a => a.LocationName )
                .ToList();

            var selectedPerson = new PersonService( new RockContext() ).GetNoTracking( this.SelectedPersonId );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "IsSchedulesAvailable", availableSchedules.Any() );
            mergeFields.Add( "Person", selectedPerson );
            lSignupMsg.Text = GetAttributeValue( AttributeKey.SignupInstructions ).ResolveMergeFields( mergeFields );

            bool needsFinalDateFooter = false;

            foreach ( var availableSchedule in availableSchedules )
            {
                if ( availableSchedule.MaxScheduledAcrossAllLocations )
                {
                    // This schedule is filled, so we don't need signup controls for it.
                    continue;
                }

                if ( availableSchedule.GroupId != currentGroupId )
                {
                    if ( currentGroupId != -1 )
                    {
                        phSignUpSchedules.Controls.Add( new LiteralControl( "</div>" ) );
                        needsFinalDateFooter = false;
                    }

                    CreateGroupHeader( availableSchedule.GroupName, availableSchedule.GroupType );
                }

                if ( availableSchedule.ScheduledDateTime.Date != currentOccurrenceDate.Date )
                {
                    if ( currentScheduleId != -1 && availableSchedule.GroupId == currentGroupId )
                    {
                        phSignUpSchedules.Controls.Add( new LiteralControl( "</div>" ) );
                        needsFinalDateFooter = false;
                    }

                    currentOccurrenceDate = availableSchedule.ScheduledDateTime.Date;
                    CreateDateHeader( currentOccurrenceDate );
                    needsFinalDateFooter = true;
                }

                currentGroupId = availableSchedule.GroupId;
                currentScheduleId = availableSchedule.ScheduleId;
                CreateScheduleSignUpRow( availableSchedule, availableGroupLocationSchedules );
            }

            if ( needsFinalDateFooter )
            {
                phSignUpSchedules.Controls.Add( new LiteralControl( "</div>" ) );
                needsFinalDateFooter = false;
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
        /// <param name="groupScheduleSignup">The person schedule signup.</param>
        /// <param name="availableGroupLocationSchedules">The available group location schedules.</param>
        private void CreateScheduleSignUpRow( GroupScheduleSignup groupScheduleSignup, List<GroupScheduleSignup> availableGroupLocationSchedules )
        {
            var scheduleSignUpRowItem = new HtmlGenericContainer();

            // give this a specific ID so that Postback to cbSignupSchedule works consistently
            scheduleSignUpRowItem.ID = $"scheduleSignUpRowItem_{groupScheduleSignup.GroupId}_{groupScheduleSignup.ScheduleId}_{groupScheduleSignup.ScheduledDateTime.Date.ToString( "yyyyMMdd" )}";

            scheduleSignUpRowItem.Attributes.Add( "class", "d-flex flex-column flex-sm-row flex-wrap align-items-sm-center js-person-schedule-signup-row" );
            phSignUpSchedules.Controls.Add( scheduleSignUpRowItem );

            var hfGroupId = new HiddenField { ID = "hfGroupId", Value = groupScheduleSignup.GroupId.ToString() };
            var hfScheduleId = new HiddenField { ID = "hfScheduleId", Value = groupScheduleSignup.ScheduleId.ToString() };
            var hfOccurrenceDate = new HiddenField { ID = "hfOccurrenceDate", Value = groupScheduleSignup.ScheduledDateTime.Date.ToISO8601DateString() };
            var hfAttendanceId = new HiddenField { ID = "hfAttendanceId" };
            scheduleSignUpRowItem.Controls.Add( hfGroupId );
            scheduleSignUpRowItem.Controls.Add( hfScheduleId );
            scheduleSignUpRowItem.Controls.Add( hfOccurrenceDate );
            scheduleSignUpRowItem.Controls.Add( hfAttendanceId );

            var pnlCheckboxCol = new DynamicControlsPanel();
            pnlCheckboxCol.ID = "pnlCheckboxCol";
            pnlCheckboxCol.Attributes.Add( "class", "flex-fill" );

            var cbSignupSchedule = new RockCheckBox();
            cbSignupSchedule.ID = "cbSignupSchedule";
            cbSignupSchedule.DisplayInline = true;
            cbSignupSchedule.Text = GetFormattedScheduleForListing( groupScheduleSignup.ScheduleName, groupScheduleSignup.ScheduledDateTime.TimeOfDay );
            cbSignupSchedule.ToolTip = groupScheduleSignup.ScheduleName;
            cbSignupSchedule.AddCssClass( "js-person-schedule-signup-checkbox" );
            cbSignupSchedule.Checked = false;
            cbSignupSchedule.AutoPostBack = true;
            cbSignupSchedule.CheckedChanged += cbSignupSchedule_CheckedChanged;

            if ( groupScheduleSignup.PeopleNeeded > 0 )
            {
                cbSignupSchedule.Text += $" <span class='schedule-signup-people-needed text-muted small'>({groupScheduleSignup.PeopleNeeded} {"person".PluralizeIf( groupScheduleSignup.PeopleNeeded != 1 )} needed)</span>";
            }

            pnlCheckboxCol.Controls.Add( cbSignupSchedule );

            var locations = new List<GroupScheduleSignupLocation>();

            var availableLocations = availableGroupLocationSchedules
                .Where( x => x.GroupId == groupScheduleSignup.GroupId
                        && x.ScheduleId == groupScheduleSignup.ScheduleId
                        && x.ScheduledDateTime.Date == groupScheduleSignup.ScheduledDateTime.Date )
                .Select( x => x.Locations )
                .ToList();

            foreach ( var availableLocationList in availableLocations )
            {
                foreach ( var availableLocation in availableLocationList )
                {
                    if ( !availableLocation.MaxScheduled )
                    {
                        locations.Add( availableLocation );
                    }
                }
            }

            var ddlSignupLocations = new RockDropDownList();
            ddlSignupLocations.ID = "ddlSignupLocations";
            ddlSignupLocations.Visible = false;

            ddlSignupLocations.AddCssClass( "js-person-schedule-signup-ddl" );
            ddlSignupLocations.AddCssClass( "input-sm" );
            ddlSignupLocations.AddCssClass( "my-1" );

            var requireLocation = GetAttributeValue( AttributeKey.RequireLocationForAdditionalSignups ).AsBoolean();
            if ( !requireLocation && locations.Count > 1 )
            {
                ddlSignupLocations.Items.Insert( 0, new ListItem( NO_LOCATION_PREFERENCE, string.Empty ) );
            }

            foreach ( var location in locations )
            {
                ddlSignupLocations.Items.Add( new ListItem( location.LocationName, location.LocationId.ToString() ) );
            }

            ddlSignupLocations.AutoPostBack = true;
            ddlSignupLocations.SelectedIndexChanged += ddlSignupLocations_SelectedIndexChanged;

            if ( locations.Count == 1 )
            {
                ddlSignupLocations.Visible = false;
            }

            var pnlLocationCol = new Panel();
            pnlLocationCol.Attributes.Add( "class", "flex-fill mb-3 mb-md-0 ml-sm-3" );
            pnlLocationCol.Controls.Add( ddlSignupLocations );

            var hlSignUpSaved = new HighlightLabel { ID = "hlSignUpSaved", LabelType = LabelType.Success, Text = "<i class='fa fa-check-square'></i> Saved" };
            hlSignUpSaved.Visible = false;
            pnlLocationCol.Controls.Add( hlSignUpSaved );

            scheduleSignUpRowItem.Controls.Add( pnlCheckboxCol );
            scheduleSignUpRowItem.Controls.Add( pnlLocationCol );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSignupLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ddlSignupLocations_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateScheduleSignUp( sender as Control );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbSignupSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cbSignupSchedule_CheckedChanged( object sender, EventArgs e )
        {
            UpdateScheduleSignUp( sender as Control );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSignUpSchedulesFamilyMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSignUpSchedulesFamilyMembers_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedSignupPersonId = ddlSignUpSchedulesFamilyMembers.SelectedValueAsId() ?? this.SelectedPersonId;
            UpdateSignupControls( selectedSignupPersonId );
        }

        /// <summary>
        /// Updates the schedule sign up.
        /// </summary>
        /// <param name="senderControl">The sender control.</param>
        private void UpdateScheduleSignUp( Control senderControl )
        {
            var selectedSignupPersonId = ddlSignUpSchedulesFamilyMembers.SelectedValueAsId() ?? this.SelectedPersonId;

            HtmlGenericContainer scheduleSignUpContainer = senderControl.FindFirstParentWhere( a => ( a is HtmlGenericContainer ) && ( a as HtmlGenericContainer ).CssClass.Contains( "js-person-schedule-signup-row" ) ) as HtmlGenericContainer;
            if ( scheduleSignUpContainer == null )
            {
                return;
            }

            var hfGroupId = scheduleSignUpContainer.FindControl( "hfGroupId" ) as HiddenField;
            var hfScheduleId = scheduleSignUpContainer.FindControl( "hfScheduleId" ) as HiddenField;
            var hfOccurrenceDate = scheduleSignUpContainer.FindControl( "hfOccurrenceDate" ) as HiddenField;
            var hfAttendanceId = scheduleSignUpContainer.FindControl( "hfAttendanceId" ) as HiddenField;
            var ddlSignupLocations = scheduleSignUpContainer.FindControl( "ddlSignupLocations" ) as RockDropDownList;
            var cbSignupSchedule = scheduleSignUpContainer.FindControl( "cbSignupSchedule" ) as RockCheckBox;
            var hlSignUpSaved = scheduleSignUpContainer.FindControl( "hlSignUpSaved" ) as HighlightLabel;

            ddlSignupLocations.Visible = cbSignupSchedule.Checked;

            var requireLocation = GetAttributeValue( AttributeKey.RequireLocationForAdditionalSignups ).AsBoolean();
            if ( ( requireLocation && ddlSignupLocations.Items.Count < 2 )
                || ( !requireLocation && ddlSignupLocations.Items.Count < 3 ) )
            {
                ddlSignupLocations.Enabled = false;
                ddlSignupLocations.Visible = false;
            }
            else
            {
                ddlSignupLocations.Enabled = true;
            }

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
                    // If there is an attendanceId, this is an attendance that they just signed up for,
                    // but they might have either unselected it, or changed the location, so remove it.
                    var attendance = attendanceService.Get( attendanceId.Value );
                    if ( attendance != null )
                    {
                        attendanceService.Delete( attendance );
                    }
                }

                if ( cbSignupSchedule.Checked )
                {
                    var attendance = attendanceService.ScheduledPersonAddPending( selectedSignupPersonId, attendanceOccurrence.Id, CurrentPersonAlias );
                    rockContext.SaveChanges();
                    hfAttendanceId.Value = attendance.Id.ToString();
                    attendanceService.ScheduledPersonConfirm( attendance.Id );
                    rockContext.SaveChanges();
                }

                hlSignUpSaved.Visible = true;
                rockContext.SaveChanges();
            }

        }

        /// <summary>
        /// Gets a list of available schedules for the group the selected sign-up person belongs to.
        /// </summary>
        /// <param name="selectedSignupPersonId">The selected signup person identifier.</param>
        /// <returns>List&lt;GroupScheduleSignup&gt;.</returns>
        private List<GroupScheduleSignup> GetScheduleSignupData( int selectedSignupPersonId )
        {
            var futureWeekDateRange = GetFutureWeekDateRange();
            List<GroupScheduleSignup> groupScheduleSignups = new List<GroupScheduleSignup>();
            var startDate = futureWeekDateRange.Start.Value;
            var endDate = futureWeekDateRange.End.Value;

            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var groupService = new GroupService( rockContext );
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

                var groupLocationService = new GroupLocationService( rockContext );
                var personGroupLocationQry = groupLocationService.Queryable().AsNoTracking();
                var overrideHideFromToolbox = GetAttributeValue( AttributeKey.OverrideHideFromToolbox ).AsBoolean();
                // get GroupLocations that are for Groups that the person is an active member of
                personGroupLocationQry = personGroupLocationQry.Where( a => a.Group.IsArchived == false
                    && a.Group.GroupType.IsSchedulingEnabled == true
                    && a.Group.DisableScheduling == false
                    && ( overrideHideFromToolbox || a.Group.DisableScheduleToolboxAccess == false )
                    && a.Group.Members.Any( m => m.PersonId == selectedSignupPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                List<Guid> includeGroupTypeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
                List<Guid> excludeGroupTypeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();

                if ( includeGroupTypeGuids.Count > 0 )
                {
                    personGroupLocationQry = personGroupLocationQry.Where( t => includeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
                }
                else if ( excludeGroupTypeGuids.Count > 0 )
                {
                    personGroupLocationQry = personGroupLocationQry.Where( t => !excludeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
                }

                var personGroupLocationList = personGroupLocationQry.ToList();
                var groupsThatHaveSchedulingRequirements = personGroupLocationQry.Where( a => a.Group.SchedulingMustMeetRequirements ).Select( a => a.Group ).Distinct().ToList();
                var personDoesntMeetSchedulingRequirementGroupIds = new HashSet<int>();

                foreach ( var groupThatHasSchedulingRequirements in groupsThatHaveSchedulingRequirements )
                {
                    var personDoesntMeetSchedulingRequirements = groupService.GroupMembersNotMeetingRequirements( groupThatHasSchedulingRequirements, false, false )
                        .Where( a => a.Key.PersonId == selectedSignupPersonId )
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
                        // Calculate capacities for this location (from the GroupLocationScheduleConfigs).
                        int maximumCapacitySetting = 0;
                        int desiredCapacitySetting = 0;
                        int minimumCapacitySetting = 0;
                        int desiredOrMinimumNeeded = 0;

                        if ( personGroupLocation.GroupLocationScheduleConfigs.Any() )
                        {
                            foreach ( var config in personGroupLocation.GroupLocationScheduleConfigs )
                            {
                                // There should only be one GroupLocationScheduleConfig for this location.
                                if ( config.ScheduleId == schedule.Id )
                                {
                                    maximumCapacitySetting = config.MaximumCapacity ?? 0;
                                    desiredCapacitySetting = config.DesiredCapacity ?? 0;
                                    minimumCapacitySetting = config.MinimumCapacity ?? 0;
                                }
                            }

                            // Use the higher value (between "minimum" and "desired") to calculate "people needed".
                            desiredOrMinimumNeeded = Math.Max( desiredCapacitySetting, minimumCapacitySetting );
                        }

                        var startDateTimeList = schedule.GetScheduledStartTimes( startDate, endDate );
                        foreach ( var startDateTime in startDateTimeList )
                        {
                            var occurrenceDate = startDateTime.Date;
                            bool alreadyScheduled = attendanceService.IsScheduled( occurrenceDate, schedule.Id, selectedSignupPersonId );
                            if ( alreadyScheduled )
                            {
                                continue;
                            }

                            if ( personScheduleExclusionService.IsExclusionDate( selectedSignupPersonId, personGroupLocation.GroupId, occurrenceDate ) )
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
                            var currentlyScheduledQry = attendanceService
                                .Queryable()
                                .Where( a => a.Occurrence.OccurrenceDate == startDateTime.Date
                                    && a.Occurrence.ScheduleId == schedule.Id
                                    && a.RSVP == RSVP.Yes
                                    && a.Occurrence.GroupId == personGroupLocation.GroupId );

                            int currentlyScheduledAtLocation = currentlyScheduledQry
                                .Where( a => a.Occurrence.LocationId == personGroupLocation.Location.Id )
                                .Count();

                            int peopleNeededAtLocation = desiredOrMinimumNeeded != 0 ? desiredOrMinimumNeeded - currentlyScheduledAtLocation : 0;

                            // If this is a new location for an existing group/schedule, find it.
                            var groupScheduleSignup = groupScheduleSignups
                                .Where( x => x.GroupId == personGroupLocation.Group.Id
                                    && x.ScheduleId == schedule.Id
                                    && x.ScheduledDateTime == startDateTime )
                                .FirstOrDefault();

                            if ( groupScheduleSignup == null )
                            {
                                var currentlyScheduledWithoutLocationQry = currentlyScheduledQry.Where( a => !a.Occurrence.LocationId.HasValue );
                                int currentlyScheduledWithoutLocation = currentlyScheduledWithoutLocationQry.Count();

                                // Add to master list groupScheduleSignups
                                groupScheduleSignup = new GroupScheduleSignup
                                {
                                    GroupId = personGroupLocation.Group.Id,
                                    GroupOrder = personGroupLocation.Group.Order,
                                    GroupName = personGroupLocation.Group.Name,
                                    GroupType = GroupTypeCache.Get( personGroupLocation.Group.GroupTypeId ),
                                    ScheduleId = schedule.Id,
                                    ScheduleName = schedule.Name,
                                    ScheduledDateTime = startDateTime,
                                    ScheduledWithoutLocation = currentlyScheduledWithoutLocation
                                };

                                groupScheduleSignups.Add( groupScheduleSignup );
                            }

                            // add the location to this group/schedule.
                            var groupSignupLocation = new GroupScheduleSignupLocation
                            {
                                LocationId = personGroupLocation.Location.Id,
                                LocationName = personGroupLocation.Location.Name,
                                LocationOrder = personGroupLocation.Order,
                                MaximumCapacity = maximumCapacitySetting,
                                ScheduledAtLocation = currentlyScheduledAtLocation,
                                PeopleNeeded = peopleNeededAtLocation < 0 ? 0 : peopleNeededAtLocation
                            };

                            groupScheduleSignup.Locations.Add( groupSignupLocation );
                        }
                    }
                }

                return groupScheduleSignups;
            }
        }

        #endregion Sign-up Tab

        #region UnavailabilitySchedule

        /// <summary>
        /// Loads the group selection ddl for the add unavailability dates modal
        /// </summary>
        private void BindUnavailabilityGroups()
        {
            using ( var rockContext = new RockContext() )
            {
                var overrideHideFromToolbox = GetAttributeValue( AttributeKey.OverrideHideFromToolbox ).AsBoolean();
                var groupMemberService = new GroupMemberService( rockContext );

                var qry = groupMemberService
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.Group.IsActive == true
                        && g.PersonId == this.SelectedPersonId
                        && g.Group.GroupType.IsSchedulingEnabled == true
                        && g.Group.DisableScheduling == false
                        && ( overrideHideFromToolbox || g.Group.DisableScheduleToolboxAccess == false ) );

                List<Guid> includeGroupTypeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
                List<Guid> excludeGroupTypeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();

                if ( includeGroupTypeGuids.Count > 0 )
                {
                    qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
                }
                else if ( excludeGroupTypeGuids.Count > 0 )
                {
                    qry = qry.Where( t => !excludeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
                }

                var groups = qry.Select( g => new { Value = ( int? ) g.GroupId, Text = g.Group.Name } ).ToList();

                groups.Insert( 0, new { Value = ( int? ) null, Text = ALL_GROUPS_STRING } );

                ddlUnavailabilityGroups.DataSource = groups;
                ddlUnavailabilityGroups.DataValueField = "Value";
                ddlUnavailabilityGroups.DataTextField = "Text";
                ddlUnavailabilityGroups.DataBind();
            }
        }

        /// <summary>
        /// Creates the list of family members that can be assigned a unavailability date for the current person
        /// </summary>
        private void BindPersonsForUnavailabilitySchedule()
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
                familyMemberAliasIds.Insert( 0, new { Value = selectedPerson.PrimaryAliasId ?? 0, Text = selectedPerson.FullName } );

                cblUnavailabilityPersons.DataSource = familyMemberAliasIds;
                cblUnavailabilityPersons.DataValueField = "Value";
                cblUnavailabilityPersons.DataTextField = "Text";
                cblUnavailabilityPersons.DataBind();
                cblUnavailabilityPersons.Items[0].Selected = true;

                // if there is only one person in the family, don't show the checkbox list since it'll always just be the individual
                cblUnavailabilityPersons.Visible = cblUnavailabilityPersons.Items.Count > 1;
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the btnUnavailabilityScheduleSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUnavailabilityScheduleSave_Click( object sender, EventArgs e )
        {
            // parse the date range and add to query
            if ( drpUnavailabilityDateRange.DelimitedValues.IsNullOrWhiteSpace() )
            {
                // show error
                return;
            }

            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpUnavailabilityDateRange.DelimitedValues );
            if ( !dateRange.Start.HasValue || !dateRange.End.HasValue )
            {
                // show error
                return;
            }

            int? parentId = null;

            foreach ( ListItem item in cblUnavailabilityPersons.Items )
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
                    GroupId = ddlUnavailabilityGroups.SelectedValueAsId(),
                    Title = tbUnavailabilityDateDescription.Text,
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

            SetNavigationHistory( pnlToolbox );
            BindScheduleRepeater();
        }

        /// <summary>
        /// Handles the Click event of the btnUnavailabilityScheduleCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnUnavailabilityScheduleCancel_Click( object sender, EventArgs e )
        {
            SetNavigationHistory( pnlToolbox );
            BindScheduleRepeater();
        }

        #endregion UnavailabilitySchedule

        #endregion Private Methods

        #region Helper Class

        private class GroupScheduleRowInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date of the Attendance. Only the date is used.
            /// </summary>
            /// <value>
            /// A <see cref="System.DateTime"/> representing the start date and time/check in date and time.
            /// </value>
            public DateTime OccurrenceStartDate { get; set; }

            /// <summary>
            /// Gets or sets the end date of the Attendance. Only the date is used.
            /// </summary>
            /// <value>
            /// A <see cref="System.DateTime"/> representing the end date and time/check in date and time.
            /// </value>
            public DateTime OccurrenceEndDate { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Group"/>.
            /// </summary>
            /// <value>
            /// The <see cref="Rock.Model.Group"/>.
            /// </value>
            public Group Group { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Location"/> where the Person attended.
            /// </summary>
            /// <value>
            /// The <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
            /// </value>
            public Location Location { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Schedule"/>.
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the GroupScheduleType.
            /// </summary>
            /// <value>
            /// The GroupScheduleType.
            /// </value>
            public GroupScheduleType GroupScheduleType { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier of the Person that this exclusion is for
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public PersonAlias PersonAlias { get; set; }

            /// <summary>
            /// Gets or sets the card CSS class.
            /// </summary>
            /// <value>
            /// The card CSS class.
            public string CardCssClass
            {
                get
                {
                    var cardCssClass = string.Empty;
                    switch ( GroupScheduleType )
                    {
                        case GroupScheduleType.Pending:
                            cardCssClass = "schedule-pending";
                            break;
                        case GroupScheduleType.Upcoming:
                            cardCssClass = "schedule-confirmed";
                            break;
                        default:
                        case GroupScheduleType.Unavailable:
                            cardCssClass = "schedule-unavailable";
                            break;
                    }

                    return cardCssClass;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private enum GroupScheduleType
        {
            /// <summary>
            /// Pending
            /// </summary>
            Pending = 0,

            /// <summary>
            /// Upcoming
            /// </summary>
            Upcoming = 1,

            /// <summary>
            /// Unavailable
            /// </summary>
            Unavailable = 2
        }

        /// <summary>
        /// This class represents an instance of a single scheduled time for a particular. The "unique key"
        /// for this object is a combination of the GroupId, the ScheduleId, and the ScheduledDateTime.
        /// </summary>
        private class GroupScheduleSignup
        {
            public int GroupId { get; set; }

            public int GroupOrder { get; set; }

            public string GroupName { get; set; }

            public GroupTypeCache GroupType { get; set; }

            public int ScheduleId { get; set; }

            public DateTime ScheduledDateTime { get; set; }

            public string ScheduleName { get; set; }

            public List<GroupScheduleSignupLocation> Locations { get; set; } = new List<GroupScheduleSignupLocation>();

            public bool MaxScheduledAcrossAllLocations
            {
                get
                {
                    // If any locations are not capped, we can always schedule more.
                    var unlimitedLocations = Locations.Where( l => l.MaximumCapacity == 0 );
                    if ( unlimitedLocations.Any() )
                    {
                        return false;
                    }

                    // Since all locations have a maximum capacity setting, check each location for available capacity
                    // and subtract any scheduled group members who have not specified a location.
                    int totalAvailableCapacity = 0;
                    foreach ( var location in Locations )
                    {
                        if ( location.MaxScheduled )
                        {
                            // If this locations is overbooked, we will ignore it.  This could potentially result in
                            // over-booking this schedule, but only because this location is already overbooked and
                            // we can't assume that people can be rescheduled to another open location.
                            continue;
                        }

                        // Add open capacity to the total.
                        totalAvailableCapacity += ( location.MaximumCapacity - location.ScheduledAtLocation );
                    }

                    if ( ScheduledWithoutLocation >= totalAvailableCapacity )
                    {
                        return true;
                    }

                    return false;
                }
            }

            public int ScheduledWithoutLocation { get; set; }

            public int PeopleNeeded
            {
                get
                {
                    // Calculate people needed for all locations of this group/schedule, and subtract the number of people
                    // who are signed up without a location selected.
                    int peopleNeeded = Locations.Sum( l => l.PeopleNeeded ) - ScheduledWithoutLocation;
                    if ( peopleNeeded < 0 )
                    {
                        return 0;
                    }

                    return peopleNeeded;
                }
            }

            public int LocationOrder
            {
                get
                {
                    return Locations.Min( l => l.LocationOrder );
                }
            }

            public string LocationName
            {
                get
                {
                    var location = Locations.OrderBy( l => l.LocationOrder ).FirstOrDefault();
                    if ( location == null )
                    {
                        return string.Empty;
                    }

                    return location.LocationName;
                }
            }
        }

        /// <summary>
        /// This class represents a specific location within a <see cref="GroupScheduleSignup"/>.  This is
        /// used to keep track of which locations are available and the total counts of group members who
        /// are scheduled across various locations.
        /// </summary>
        public class GroupScheduleSignupLocation
        {
            public int LocationId { get; set; }

            public string LocationName { get; set; }

            public int LocationOrder { get; set; }

            public int MaximumCapacity { get; set; }

            public int ScheduledAtLocation { get; set; }

            public bool MaxScheduled
            {
                get
                {
                    // If there isn't a maximum capacity setting, this location will always allow signups.
                    if ( MaximumCapacity == 0 )
                    {
                        return false;
                    }

                    return ( ScheduledAtLocation >= MaximumCapacity );

                }
            }

            public int PeopleNeeded { get; set; }
        }

        #endregion Helper Class
    }
}
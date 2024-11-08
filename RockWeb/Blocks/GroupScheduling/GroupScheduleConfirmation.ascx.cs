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
    [DisplayName( "Group Schedule Confirmation" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows a person to confirm a schedule RSVP and view pending schedules.  Uses PersonActionIdentifier in 'Person' with action 'ScheduleConfirm' when supplied." )]

    #region Block Attributes

    [CodeEditorField( "Confirm Heading Template",
        Description = "Text to display when a person confirms a schedule RSVP. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = ConfirmHeadingTemplateDefaultValue,
        Order = 1,
        Key = AttributeKey.ConfirmHeadingTemplate )]

    [CodeEditorField( "Decline Heading Template",
        Description = "Heading to display when a person declines a schedule RSVP. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = DeclineHeadingTemplateDefaultValue,
        Order = 2,
        Key = AttributeKey.DeclineHeadingTemplate )]

    [CodeEditorField( "Decline Message Template",
        Description = "Message to display when a person declines a schedule RSVP. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = DeclineMessageTemplateDefaultValue,
        Order = 3,
        Key = AttributeKey.DeclineMessageTemplate )]

    [BooleanField( "Scheduler Receive Confirmation Emails",
        Description = "If checked, the scheduler will receive an email response for each confirmation or decline.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.SchedulerReceiveConfirmationEmails )]

    [BooleanField( "Require Decline Reasons",
        Description = "If checked, a person must choose one of the 'Decline Reasons' to submit their decline status.",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.RequireDeclineReasons )]

    [BooleanField( "Enable Decline Note",
        Description = "If checked, a note will be shown for the person to elaborate on why they cannot attend.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.EnableDeclineNote )]

    [BooleanField( "Require Decline Note",
        Description = "If checked, a custom note response will be required in order to save their decline status.",
        DefaultBooleanValue = false,
        Order = 7,
        Key = AttributeKey.RequireDeclineNote )]

    [TextField( "Decline Note Title",
        Description = "A custom title for the decline elaboration note.",
        IsRequired = false,
        DefaultValue = "Please elaborate on why you cannot attend.",
        Order = 8,
        Key = AttributeKey.DeclineNoteTitle )]

    [SystemCommunicationField( "Scheduling Response Email",
        Description = "The system email that will be used for sending responses back to the scheduler.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SCHEDULING_RESPONSE,
        Order = 9,
        Key = AttributeKey.SchedulingResponseEmail )]

    #endregion Block Attributes

    [ContextAware( typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.BlockTypeGuid( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C" )]
    public partial class GroupScheduleConfirmation : Rock.Web.UI.RockBlock
    {
        #region Keys/Constants

        protected class AttributeKey
        {
            public const string ConfirmHeadingTemplate = "ConfirmHeadingTemplate";
            public const string DeclineHeadingTemplate = "DeclineHeadingTemplate";
            public const string DeclineMessageTemplate = "DeclineMessageTemplate";
            public const string SchedulerReceiveConfirmationEmails = "SchedulerReceiveConfirmationEmails";
            public const string RequireDeclineReasons = "RequireDeclineReasons";
            public const string EnableDeclineNote = "EnableDeclineNote";
            public const string RequireDeclineNote = "RequireDeclineNote";
            public const string DeclineNoteTitle = "DeclineNoteTitle";
            public const string SchedulingResponseEmail = "SchedulingResponseEmail";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        protected class PageParameterKey
        {
            public const string AttendanceId = "AttendanceId";
            public const string AttendanceIds = "AttendanceIds";
            public const string IsConfirmed = "IsConfirmed";
            public const string ReturnUrl = "ReturnUrl";
            public const string Person = "Person";
        }

        protected const string ConfirmHeadingTemplateDefaultValue = @"
<h2 class='margin-t-none'>{{ Person.NickName }}, you're confirmed to serve.</h2>
<p>Thanks for letting us know.  You're confirmed for:</p>

{% capture attendanceIdList %}{% for ScheduledItem in ScheduledItems %}{{ ScheduledItem.Id }}{% unless forloop.last %},{% endunless %}{% endfor %}{% endcapture %}
{% assign lastDate = '' %}

<p>
{% for ScheduledItem in ScheduledItems %}
{% assign currentDate = ScheduledItem.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' %}
  {% if lastDate != currentDate %}
    <b>{{ currentDate }}</b><br />
    {% assign lastDate = currentDate %}
  {% endif %}

  {{ ScheduledItem.Occurrence.Group.Name }}<br />
  {{ ScheduledItem.Location.Name }} {{ScheduledItem.Occurrence.Schedule.Name }}
<i class='text-success fa fa-check-circle'></i><br /><br />
{% endfor %}
</p><p class='margin-b-lg'>Thanks again!<br /></p>";

        protected const string DeclineHeadingTemplateDefaultValue = @"
<h2 class='margin-t-none'>{{ Person.NickName }}, can't make it?</h2>
<p>Thanks for letting us know.  We'll try to schedule another person for:</p>
{% capture attendanceIdList %}{% for ScheduledItem in ScheduledItems %}{{ ScheduledItem.Id }}{% unless forloop.last %},{% endunless %}{% endfor %}{% endcapture %}
{% assign lastDate = '' %}

<p>
{% for ScheduledItem in ScheduledItems %}
{% assign currentDate = ScheduledItem.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' %}
  {% if lastDate != currentDate %}
    <b>{{ currentDate }}</b><br />
    {% assign lastDate = currentDate %}
  {% endif %}

  {{ ScheduledItem.Occurrence.Group.Name }}<br />
  {{ ScheduledItem.Location.Name }} {{ScheduledItem.Occurrence.Schedule.Name }}<br /><br />
{% endfor %}
</p>";

        protected const string DeclineMessageTemplateDefaultValue = @"
<div class='alert alert-success'><strong>Thank You</strong> We'll try to schedule another person for:

{% for ScheduledItem in ScheduledItems %}
  <br />{{ ScheduledItem.Occurrence.Group.Name }}
{% endfor %}
</div>";

        #endregion Keys/Constants

        #region Fields

        private Person _selectedPerson;

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadBlockSettings();
                if ( HasRequiredParameters() )
                {
                    SetSelectedPersonId();
                    if ( _selectedPerson == null )
                    {
                        ShowNotAuthorizedMessage();
                        return;
                    }

                    GetAttendanceByAttendanceIdAndSelectedPersonId();
                }
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSubmitDeclineReason control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSubmitDeclineReason_Click( object sender, EventArgs e )
        {
            if ( this.Page.IsValid )
            {
                SetSelectedPersonId();

                if ( _selectedPerson == null )
                {
                    ShowNotAuthorizedMessage();
                    return;
                }

                UpdateAttendanceDeclineReasonAfterSubmit();

                if ( PageParameter( PageParameterKey.ReturnUrl ).IsNotNullOrWhiteSpace() )
                {
                    NavigateToPage( PageParameter( PageParameterKey.ReturnUrl ).AsGuid(), null );
                }
                else
                {
                    nbSaveDeclineReasonMessage.Visible = true;
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
                new AttendanceService( rockContext ).ScheduledPersonConfirm( attendanceId.Value );
                rockContext.SaveChanges();
            }

            BindPendingConfirmations();
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

                // Use the value selected in the drop down list if it was set.
                int? declineReasonValueId = ddlDeclineReason.SelectedItem.Value.AsIntegerOrNull();

                new AttendanceService( rockContext ).ScheduledPersonDecline( attendanceId.Value, declineReasonValueId );
                rockContext.SaveChanges();
                DetermineRecipientAndSendResponseEmails( new List<int> { attendanceId.Value } );
            }

            BindPendingConfirmations();
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

        #endregion Events

        #region Methods

        /// <summary>
        /// Loads the decline reasons.
        /// </summary>
        private void LoadDeclineReasons()
        {
            var defineTypeGroupScheduleReason = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_SCHEDULE_DECLINE_REASON );
            var definedValues = defineTypeGroupScheduleReason.DefinedValues;

            ddlDeclineReason.DataSource = definedValues;
            ddlDeclineReason.DataBind();
            ddlDeclineReason.Items.Insert( 0, new ListItem() );
        }

        /// <summary>
        /// Binds the pending confirmations.
        /// </summary>
        private void BindPendingConfirmations()
        {
            var selectedPersonId = hfSelectedPersonId.Value.AsIntegerOrNull();

            if ( selectedPersonId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var qryPendingConfirmations = new AttendanceService( rockContext )
                .GetPendingScheduledConfirmations()
                .AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == selectedPersonId )
                .OrderBy( a => a.Occurrence.OccurrenceDate );

            var pendingConfirmations = qryPendingConfirmations.ToList();
            if ( pendingConfirmations.Count > 0 )
            {
                rptPendingConfirmations.DataSource = pendingConfirmations;
                rptPendingConfirmations.DataBind();
                pnlPendingConfirmation.Visible = true;
            }
            else
            {
                pnlPendingConfirmation.Visible = false;
            }
        }

        /// <summary>
        /// Handles the not authorized.
        /// </summary>
        private void ShowNotAuthorizedMessage()
        {
            pnlPendingConfirmation.Visible = false;
            nbError.Visible = true;
        }

        /// <summary>
        /// Shows the decline message after submit.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowDeclineMessageAfterSubmit( List<int> attendanceIds, IDictionary<string, object> mergeFields )
        {
            nbError.Visible = false;
            lResponse.Text = GetAttributeValue( AttributeKey.DeclineMessageTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            lResponse.Visible = true;

            DetermineRecipientAndSendResponseEmails( attendanceIds );
        }

        /// <summary>
        /// Shows the heading by is confirmed.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowHeadingByIsConfirmed( List<Attendance> attendanceList )
        {
            // All attendances in the list should have the same decline reason, note, etc, so using the first one for those values is okay.
            var attendance = attendanceList.FirstOrDefault();
            if ( attendance.Note.IsNotNullOrWhiteSpace() )
            {
                dtbDeclineReasonNote.Text = attendance.Note;
            }

            if ( attendance.DeclineReasonValueId != null )
            {
                ddlDeclineReason.SelectedValue = attendance.DeclineReasonValueId.ToString();
            }

            if ( PageParameter( PageParameterKey.IsConfirmed ).AsBoolean() )
            {
                var mergeFields = MergeFields( attendanceList, attendance?.ScheduledByPersonAlias?.Person );
                ShowConfirmationHeading( mergeFields );
            }
            else
            {
                // we send decline email from submit button
                var mergeFields = MergeFields( attendanceList, attendance.Occurrence.Group?.ScheduleCoordinatorPersonAlias?.Person );
                ShowDeclineHeading( mergeFields );
            }

            lBlockTitle.Text = "Email Confirmation";
        }

        /// <summary>
        /// Shows the confirmation heading.
        /// </summary>
        /// <param name="mergeFields">The merge fields.</param>
        private void ShowConfirmationHeading( IDictionary<string, object> mergeFields )
        {
            lResponse.Text = GetAttributeValue( AttributeKey.ConfirmHeadingTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            lResponse.Visible = true;
            pnlDeclineReason.Visible = false;
            pnlPendingConfirmation.Visible = true;
        }

        /// <summary>
        /// Shows the decline heading.
        /// </summary>
        /// <param name="mergeFields">The merge fields.</param>
        private void ShowDeclineHeading( IDictionary<string, object> mergeFields )
        {
            pnlDeclineReason.Visible = true;
            lResponse.Text = GetAttributeValue( AttributeKey.DeclineHeadingTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            lResponse.Visible = true;
        }

        /// <summary>
        /// Determines whether [has required parameters].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has required parameters]; otherwise, <c>false</c>.
        /// </returns>
        private bool HasRequiredParameters()
        {
            // Must have either AttendanceId or AttendanceIds.
            if ( PageParameter( PageParameterKey.AttendanceId ).IsNullOrWhiteSpace() && PageParameter( PageParameterKey.AttendanceIds ).IsNullOrWhiteSpace() )
            {
                ShowNotAuthorizedMessage();
                return false;
            }

            if ( PageParameter( PageParameterKey.IsConfirmed ).AsBooleanOrNull() == null )
            {
                ShowIsConfirmedMissingParameterMessage();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the is confirmed missing parameter message.
        /// </summary>
        private void ShowIsConfirmedMissingParameterMessage()
        {
            nbError.Title = "Sorry,";
            nbError.NotificationBoxType = NotificationBoxType.Warning;
            nbError.Text = "Something is not right with the link you used to get here.";
            nbError.Visible = true;
        }

        /// <summary>
        /// Loads the block settings.
        /// </summary>
        private void LoadBlockSettings()
        {
            LoadDeclineReasons();

            // Decline reason drop down always visible
            ddlDeclineReason.Visible = true;

            // block setting drives if required
            ddlDeclineReason.Required = GetAttributeValue( AttributeKey.RequireDeclineReasons ).AsBoolean();
            this.btnSubmitDeclineReason.Visible = true;

            // decline Note
            dtbDeclineReasonNote.Label = GetAttributeValue( AttributeKey.DeclineNoteTitle ).ToString();
            dtbDeclineReasonNote.Visible = GetAttributeValue( AttributeKey.EnableDeclineNote ).AsBoolean();
            dtbDeclineReasonNote.Required = GetAttributeValue( AttributeKey.RequireDeclineNote ).AsBoolean();
        }

        /// <summary>
        /// Sets the attendance on load.
        /// </summary>
        private void GetAttendanceByAttendanceIdAndSelectedPersonId()
        {
            using ( var rockContext = new RockContext() )
            {
                // Is a person selected?
                if ( _selectedPerson == null )
                {
                    nbError.Visible = true;
                }
                else if ( CurrentPerson != null && _selectedPerson.Guid != CurrentPerson.Guid )
                {
                    nbError.Visible = true;
                    nbError.Title = "Note:";
                    nbError.NotificationBoxType = NotificationBoxType.Info;
                    nbError.Text = string.Format( "You are setting and viewing the confirmations for {0}.", _selectedPerson.FullName );
                }

                var request = Context.Request;
                var attendanceIds = GetAttendanceIdsFromParameters();
                if ( attendanceIds.Count == 0 )
                {
                    ShowNotAuthorizedMessage();
                    return;
                }

                var attendanceService = new AttendanceService( rockContext );
                var attendanceList = new List<Attendance>();
                var attendanceIdsChangedToRsvpYes = new List<int>();

                // Make sure each attendance is for the selected in person.
                foreach ( var attendanceId in attendanceIds )
                {
                    var attendance = attendanceService.Queryable()
                        .Where( a => a.Id == attendanceId && a.PersonAlias.PersonId == _selectedPerson.Id )
                        .Include( a => a.PersonAlias.Person )
                        .Include( a => a.ScheduledByPersonAlias.Person )
                        .Include( a => a.Occurrence.Group )
                        .FirstOrDefault();

                    if ( attendance == null )
                    {
                        ShowNotAuthorizedMessage();
                        return;
                    }

                    bool statusChangedToYes = false;

                    bool isConfirmedParameter = this.PageParameter( PageParameterKey.IsConfirmed ).AsBoolean();
                    if ( isConfirmedParameter )
                    {
                        if ( !attendance.IsScheduledPersonConfirmed() )
                        {
                            statusChangedToYes = true;
                            attendanceService.ScheduledPersonConfirm( attendance.Id );
                            rockContext.SaveChanges();
                        }
                    }
                    else
                    {
                        if ( !attendance.IsScheduledPersonDeclined() )
                        {
                            attendanceService.ScheduledPersonDecline( attendance.Id, null );
                            rockContext.SaveChanges();
                        }
                    }

                    if ( statusChangedToYes )
                    {
                        rockContext.SaveChanges();

                        // Only send Confirm if the status has changed and change is to Yes
                        if ( attendance.RSVP == RSVP.Yes )
                        {
                            attendanceIdsChangedToRsvpYes.Add( attendanceId );
                        }
                    }

                    attendanceList.Add( attendance );
                }

                DetermineRecipientAndSendResponseEmails( attendanceIdsChangedToRsvpYes );
                ShowHeadingByIsConfirmed( attendanceList );
                BindPendingConfirmations();
            }
        }

        /// <summary>
        /// Updates the attendance decline reason.
        /// </summary>
        private void UpdateAttendanceDeclineReasonAfterSubmit()
        {
            try
            {
                SetSelectedPersonId();
                var attendanceIds = GetAttendanceIdsFromParameters();
                var attendanceList = new List<Attendance>();

                foreach ( var attendanceId in attendanceIds )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var attendance = new AttendanceService( rockContext ).Queryable()
                            .Where( a => a.Id == attendanceId && a.PersonAlias.PersonId == _selectedPerson.Id )
                            .Include( a => a.PersonAlias.Person )
                            .Include( a => a.ScheduledByPersonAlias.Person )
                            .Include( a => a.Occurrence.Group )
                            .FirstOrDefault();

                        if ( attendance != null )
                        {
                            var declineResonId = ddlDeclineReason.SelectedItem.Value.AsInteger();

                            if ( declineResonId == 0 )
                            {
                                // set to blank and not required
                                attendance.DeclineReasonValueId = null;
                            }
                            else
                            {
                                attendance.DeclineReasonValueId = declineResonId;
                            }

                            if ( !dtbDeclineReasonNote.Text.IsNullOrWhiteSpace() )
                            {
                                attendance.Note = dtbDeclineReasonNote.Text;
                            }

                            rockContext.SaveChanges();
                            attendanceList.Add( attendance );
                        }

                    }
                }

                var mergeFields = MergeFields( attendanceList, this.ContextEntity<Person>() );
                ShowDeclineMessageAfterSubmit( attendanceIds, mergeFields );

                pnlDeclineReason.Visible = false;
            }
            catch ( Exception ex )
            {
                // ignore but log
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Sets the person _selectedPerson.
        /// </summary>
        private void SetSelectedPersonId()
        {
            var targetPerson = this.ContextEntity<Person>();
            if ( targetPerson != null )
            {
                _selectedPerson = targetPerson;
            }
            else
            {
                // Use the PersonActionIdentifier if given...
                string personKey = PageParameter( PageParameterKey.Person );
                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    _selectedPerson = new PersonService( new RockContext() ).GetByPersonActionIdentifier( personKey, "ScheduleConfirm" );
                    if ( _selectedPerson != null )
                    {
                        hfSelectedPersonId.Value = _selectedPerson.Id.ToString();
                    }
                }
                else
                {
                    // ...otherwise use the current person.
                    _selectedPerson = this.CurrentPerson;
                    hfSelectedPersonId.Value = this.CurrentPersonId.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the attendance identifiers from parameters.
        /// </summary>
        private List<int> GetAttendanceIdsFromParameters()
        {
            // Prefer AttendanceIds (plural) parameter.
            if ( PageParameter( PageParameterKey.AttendanceIds ).IsNotNullOrWhiteSpace() )
            {
                var attendanceIdList = new List<int>();
                var attendanceIds = PageParameter( PageParameterKey.AttendanceIds );

                foreach ( var attendanceIdValue in attendanceIds.Split( ',' ) )
                {
                    var attendanceId = attendanceIdValue.AsInteger();
                    attendanceIdList.Add( attendanceId );
                }

                return attendanceIdList;
            }

            // Fall back to old (singular) parameter.
            if ( PageParameter( PageParameterKey.AttendanceId ).IsNotNullOrWhiteSpace() )
            {
                var attendanceId = PageParameter( PageParameterKey.AttendanceId ).AsIntegerOrNull();

                if ( attendanceId != null && _selectedPerson != null )
                {
                    return new List<int> { attendanceId.Value };
                }
            }

            return new List<int>();
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
        /// Gets the occurrence details.
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
        /// Determines the recipient and send confirmation email.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        private void DetermineRecipientAndSendResponseEmails( List<int> attendanceIds )
        {
            if ( !attendanceIds.Any() )
            {
                return;
            }

            var attendanceService = new AttendanceService( new RockContext() );
            foreach ( var attendanceId in attendanceIds )
            {
                // Get all the supporting data we'll need to send the email.
                var attendance = attendanceService
                    .GetWithScheduledPersonResponseData()
                    .FirstOrDefault( a => a.Id == attendanceId );

                if ( attendance == null )
                {
                    continue;
                }

                try
                {
                    var schedulingResponseEmailGuid = GetAttributeValue( AttributeKey.SchedulingResponseEmail ).AsGuid();

                    // Send "accept" and "decline" emails to scheduled-by person (defined on the attendance record).
                    var scheduledByPerson = attendance.ScheduledByPersonAlias?.Person;
                    var shouldSendScheduledByPersonEmail = scheduledByPerson != null
                        && GetAttributeValue( AttributeKey.SchedulerReceiveConfirmationEmails ).AsBoolean();

                    if ( shouldSendScheduledByPersonEmail )
                    {
                        AttendanceService.SendScheduledPersonResponseEmail( attendance, schedulingResponseEmailGuid, scheduledByPerson );
                    }

                    // Send emails to group schedule coordinator person based on group/group type configuration.
                    var notificationType = attendance.RSVP == RSVP.No
                        ? ScheduleCoordinatorNotificationType.Decline
                        : ScheduleCoordinatorNotificationType.Accept;
                    var groupScheduleCoordinatorPerson = attendance.Occurrence?.Group?.ScheduleCoordinatorPersonAlias?.Person;
                    var shouldSendCoordinatorPersonEmail = groupScheduleCoordinatorPerson != null
                        && ( !shouldSendScheduledByPersonEmail || scheduledByPerson.Id != groupScheduleCoordinatorPerson.Id ) // Prevent duplicate email.
                        && attendance.Occurrence.Group.ShouldSendScheduleCoordinatorNotificationType( notificationType );

                    if ( shouldSendCoordinatorPersonEmail )
                    {
                        AttendanceService.SendScheduledPersonResponseEmail( attendance, schedulingResponseEmailGuid, groupScheduleCoordinatorPerson );
                    }
                }
                catch ( SystemException ex )
                {
                    ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
                }
            }
        }

        /// <summary>
        /// Populates the merge fields.
        /// </summary>
        /// <param name="attendanceList">The attendance list.</param>
        /// <param name="recipientPerson">The recipient person.</param>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
        private Dictionary<string, object> MergeFields( List<Attendance> attendanceList, Person recipientPerson )
        {
            var attendance = attendanceList.FirstOrDefault(); // use first attendance to get Person and Scheduler.
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this._selectedPerson );
            mergeFields.Add( "ScheduledItems", attendanceList );
            mergeFields.Add( "Person", attendance.PersonAlias.Person );
            mergeFields.Add( "Scheduler", attendance.ScheduledByPersonAlias.Person );

            // This would be Scheduler or Cancellation Person depending on which recipientPerson was specified
            mergeFields.Add( "Recipient", recipientPerson );

            return mergeFields;
        }

        #endregion Methods
    }
}
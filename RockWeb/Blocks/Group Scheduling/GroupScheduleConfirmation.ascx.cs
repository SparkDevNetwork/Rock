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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    [DisplayName( "Group Schedule Confirmation" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows a person to confirm a schedule RSVP and view pending schedules." )]

    [CodeEditorField( "Confirm Heading Template", "Text to display when person confirms a schedule RSVP. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false,
    @"<h2 class='margin-t-none'>You’re confirmed to serve</h2><p>Thanks for letting us know.  You’re confirmed for:</p><p>{{ Group.Name }}<br>{{ ScheduledItem.Location.Name }} {{ScheduledItem.Schedule.Name }}<br></p>
<p>Thanks again!</p>
<p>{{ Group.Scheduler.FullName }}<br>{{ 'Global' | Attribute:'OrganizationName' }}</p>", "", 1, "ConfirmHeadingTemplate" )]

    [CodeEditorField( "Decline Heading Template", "Text to display when person confirms a schedule RSVP. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false,
    @"<h2 class='margin-t-none'>Can’t make it?</h2><p>Thanks for letting us know.  We’ll try to schedule another person for:</p>
<p>{{ Group.Name }}<br>
{{ ScheduledItem.Location.Name }} {{ ScheduledItem.Schedule.Name }}<br></p>", "", 2, "DeclineHeadingTemplate" )]

    [BooleanField( "Scheduler Receive Confirmation Emails", "If checked, the scheduler will receive an email response for each confirmation or decline.", false, "", 3 )]
    [BooleanField( "Require Decline Reasons", "If checked, a person must choose one of the ‘Decline Reasons’ to submit their decline status.", true, "", 4 )]
    [BooleanField( "Enable Decline Note", "If checked, a note will be shown for the person to elaborate on why they cannot attend.", false, "", 5 )]
    [BooleanField( "Require Decline Note", "If checked, a custom note response will be required in order to save their decline status.", false, "", 6 )]

    [TextField( "Decline Note Title", "A custom title for the decline elaboration note.", false, "Please elaborate on why you cannot attend.", "", 7 )]
    [SystemEmailField( "Scheduling Response Email", "The system email that will be used for sending responses back to the scheduler.", false, Rock.SystemGuid.SystemEmail.SCHEDULING_RESPONSE, "", 8, "SchedulingResponseEmail" )]
    [ContextAware( typeof( Rock.Model.Person ) )]

    public partial class GroupScheduleConfirmation : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Person _selectedPerson;

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
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
            }
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

            BindPendingConfirmations();
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

                int? declineReasonValueId = null;

                new AttendanceService( rockContext ).ScheduledPersonDecline( attendanceId.Value, declineReasonValueId );
                rockContext.SaveChanges();
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
        #endregion

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
        private void ShowDeclineMessageAfterSubmit( Attendance attendance )
        {
            lResponse.Visible = false;
            nbError.Title = "Thank you";
            nbError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Success;
            nbError.Text = string.Format( "Thanks for letting us know. We’ll try to schedule another person for: {0}", attendance.Occurrence.Group.Name );
            nbError.Visible = true;

            DetermineRecipientAndSendResponseEmail( attendance );
        }

        /// <summary>
        /// Shows the heading by is confirmed.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowHeadingByIsConfirmed( Attendance attendance )
        {
            var mergeFields = MergeFields( attendance );
            if ( attendance.Note.IsNotNullOrWhiteSpace() )
            {
                dtbDeclineReasonNote.Text = attendance.Note;
            }

            if ( attendance.DeclineReasonValueId != null )
            {
                ddlDeclineReason.SelectedValue = attendance.DeclineReasonValueId.ToString();
            }

            if ( PageParameter( "isConfirmed" ).AsBoolean() )
            {
                ShowConfirmationHeading( mergeFields );
                // we send decline email from submit button
            }
            else
            {
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
            lResponse.Text = GetAttributeValue( "ConfirmHeadingTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
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
            lResponse.Text = GetAttributeValue( "DeclineHeadingTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
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
            if ( PageParameter( "attendanceId" ).IsNullOrWhiteSpace() )
            {
                ShowNotAuthorizedMessage();
                return false;
            }
            if ( PageParameter( "isConfirmed" ).AsBooleanOrNull() == null )
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
            nbError.Title = "Sorry";
            nbError.NotificationBoxType = NotificationBoxType.Warning;
            nbError.Text = "something is not right with the link you used to get here.";
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
            ddlDeclineReason.Required = GetAttributeValue( "RequireDeclineReasons" ).AsBoolean();
            this.btnSubmit.Visible = true;

            // decline Note
            dtbDeclineReasonNote.Label = GetAttributeValue( "DeclineNoteTitle" ).ToString();
            dtbDeclineReasonNote.Visible = GetAttributeValue( "EnableDeclineNote" ).AsBoolean();
            dtbDeclineReasonNote.Required = GetAttributeValue( "RequireDeclineNote" ).AsBoolean();
        }

        /// <summary>
        /// Sets the attendance on load.
        /// </summary>
        private void GetAttendanceByAttendanceIdAndSelectedPersonId()
        {
            using ( var rockContext = new RockContext() )
            {
                // otherwise use the currently logged in person
                if ( CurrentPerson == null )
                {
                    nbError.Visible = true;
                }

                var request = Context.Request;
                var attendanceId = GetAttendanceIdFromParameters();
                if ( attendanceId != null )
                {
                    var attendanceService = new AttendanceService( rockContext );

                    // make sure the attendance is for the currently logged in person
                    var attendance = attendanceService.Queryable().Where( a => a.Id == attendanceId.Value && a.PersonAlias.PersonId == _selectedPerson.Id ).FirstOrDefault();

                    if ( attendance == null )
                    {
                        ShowNotAuthorizedMessage();
                        return;
                    }

                    bool statusChanged = false;

                    bool isConfirmedParameter = this.PageParameter( "IsConfirmed" ).AsBoolean();
                    if ( isConfirmedParameter )
                    {
                        if ( !attendance.IsScheduledPersonConfirmed() )
                        {
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

                    if ( statusChanged )
                    {
                        rockContext.SaveChanges();

                        // Only send Confirm if the status has changed and change is to Yes
                        if ( attendance.RSVP == RSVP.Yes )
                        {
                            DetermineRecipientAndSendResponseEmail( attendance );
                        }
                    }

                    ShowHeadingByIsConfirmed( attendance );
                }
                else
                {
                    ShowNotAuthorizedMessage();
                    return;
                }

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
                var attendanceId = GetAttendanceIdFromParameters();

                if ( attendanceId != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var attendance = new AttendanceService( rockContext ).Queryable().Where( a => a.Id == attendanceId.Value && a.PersonAlias.PersonId == _selectedPerson.Id ).FirstOrDefault();
                        if ( attendance != null )
                        {
                            var declineResonId = ddlDeclineReason.SelectedItem.Value.AsInteger();

                            if ( declineResonId == 0 )
                            {
                                //set to blank and not required
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
                        }

                        ShowDeclineMessageAfterSubmit( attendance );
                    }
                }

                pnlDeclineReason.Visible = false;
            }
            catch ( Exception ex )
            {
                // ignore but log 
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Sets the person _selectedPersonId.
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
                _selectedPerson = this.CurrentPerson;
                hfSelectedPersonId.Value = this.CurrentPersonId.ToString();
            }
        }

        /// <summary>
        /// Gets the attendance identifier from parameters.
        /// </summary>
        /// <returns></returns>
        private int? GetAttendanceIdFromParameters()
        {
            if ( PageParameter( "attendanceId" ).IsNotNullOrWhiteSpace() )
            {
                var attendanceId = PageParameter( "attendanceId" ).AsIntegerOrNull();

                //_targetPerson null if Log out is was called
                if ( attendanceId != null && CurrentPerson != null )
                {
                    return attendanceId;
                }
            }

            return null;
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
        /// Gets the occurrence details.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns></returns>
        protected string GetOccurrenceDetails( Attendance attendance )
        {
            return string.Format( "{0} - {1} - {2}", attendance.Occurrence.OccurrenceDate.ToShortDateString(), attendance.Occurrence.Group.Name, attendance.Occurrence.Location );
        }

        /// <summary>
        /// Determines the recipient and send confirmation email.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void DetermineRecipientAndSendResponseEmail( Attendance attendance )
        {
            List<string> recipientEmailAddresses = new List<string>();

            // if scheduler receives  email add as a recipient
            if ( GetAttributeValue( "SchedulerReceiveConfirmationEmails" ).AsBoolean() )
            {
                recipientEmailAddresses.Add( attendance.ScheduledByPersonAlias.Person.Email );
            }

            // if attendance is decline (no) send email to Schedule Cancellation Person
            if ( attendance.RSVP == RSVP.No )
            {
                recipientEmailAddresses.Add( attendance.Occurrence.Group.ScheduleCancellationPersonAlias.Person.Email );
            }

            SendResponseEmail( attendance, recipientEmailAddresses );
        }

        /// <summary>
        /// Sends the confirmation email.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <param name="recipientEmailAddresses">The recipient email addresses.</param>
        private void SendResponseEmail( Attendance attendance, List<string> recipientEmailAddresses )
        {
            try
            {
                var mergeFields = MergeFields( attendance );
                mergeFields.Add( "ScheduledDate", attendance.Occurrence.Schedule.EffectiveStartDate.ToString() );
                mergeFields.Add( "Person", attendance.PersonAlias.Person );
                mergeFields.Add( "Scheduler", attendance.ScheduledByPersonAlias.Person );

                // Distinct is used so that if the same email address is for both the Scheduler and ScheduleCancellationPersonAlias
                // Only one email will be sent
                foreach ( var recipient in recipientEmailAddresses.Distinct( StringComparer.CurrentCultureIgnoreCase ) )
                {
                    var emailMessage = new RockEmailMessage( GetAttributeValue( "SchedulingResponseEmail" ).AsGuid() );
                    emailMessage.AddRecipient( new RecipientData( recipient, mergeFields ) );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        /// <summary>
        /// Merges the fields.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns></returns>
        private Dictionary<string, object> MergeFields( Attendance attendance )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this._selectedPerson );
            var group = attendance.Occurrence.Group;
            mergeFields.Add( "Group", group );
            mergeFields.Add( "ScheduledItem", attendance );

            return mergeFields;
        }

        #endregion
    }
}
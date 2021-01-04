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
using Newtonsoft.Json;

namespace RockWeb.Blocks.RSVP
{
    /// <summary>
    /// Displays the details of the given RSVP occurrence.
    /// </summary>
    [DisplayName( "RSVP Response" )]
    [Category( "RSVP" )]
    [Description( "Allows invited people to RSVP for one or more Attendance Occurrences." )]

    #region Block Attributes

    [BooleanField( "Display Form When Signed In",
        Key = AttributeKey.DisplayFormWhenSignedIn,
        Description = "If signed in and Display Form When Signed In is disabled, only the accept and decline buttons are shown.",
        DefaultBooleanValue = true,
        Order = 0 )]

    [TextField( "Accept Button Label",
        Key = AttributeKey.AcceptButtonLabel,
        Description = "The label for the Accept button.",
        DefaultValue = "Accept",
        Order = 2 )]

    [TextField( "Decline Button Label",
        Key = AttributeKey.DeclineButtonLabel,
        Description = "The label for the Decline button.",
        DefaultValue = "Decline",
        Order = 3 )]

    [MemoField( "Default Accept Message",
        Key = AttributeKey.DefaultAcceptMessage,
        Description = "The default message displayed when an RSVP is accepted.",
        DefaultValue = "We have received your response. Thanks, and we’ll see you soon!",
        Order = 4 )]

    [MemoField( "Default Decline Message",
        Key = AttributeKey.DefaultDeclineMessage,
        Description = "The default message displayed when an RSVP is declined.",
        DefaultValue = "Sorry to hear you won’t make it, but hopefully we’ll see you again soon!",
        Order = 5 )]

    [DefinedValueField( "Default Decline Reasons",
        Key = AttributeKey.DefaultDeclineReasons,
        Description = "Default Decline Reasons to be displayed.  Setting decline reasons on the Attendance Occurrence will override these.",
        DefaultValue = "",
        Order = 6 )]

    [TextField( "Multigroup Mode RSVP Title",
        Key = AttributeKey.MultigroupModeRSVPTitle,
        Description = "The page title when a user is RSVPing for multiple groups.",
        DefaultValue = "RSVP For Events",
        Order = 8 )]

    [MemoField( "Multigroup Accept Message",
        Key = AttributeKey.MultigroupAcceptMessage,
        Description = "The message displayed when one or more RSVPs are accepted in Multigroup mode.  Will include a list of accepted events with the key \"AcceptedRsvps\".",
        DefaultValue = "",
        Order = 9 )]

    #endregion

    public partial class RSVPResponse : RockBlock
    {
        private static class AttributeKey
        {
            public const string DisplayFormWhenSignedIn = "DisplayFormWhenSignedIn";
            public const string AcceptButtonLabel = "AcceptButtonLabel";
            public const string DeclineButtonLabel = "DeclineButtonLabel";
            public const string DefaultAcceptMessage = "DefaultAcceptMessage";
            public const string DefaultDeclineMessage = "DefaultDeclineMessage";
            public const string DefaultDeclineReasons = "DefaultDeclineReasons";
            public const string MultigroupModeRSVPTitle = "MultigroupModeRSVPTitle";
            public const string MultigroupAcceptMessage = "MultigroupAcceptMessage";
        }

        private static class PageParameterKey
        {
            public const string AttendanceOccurrenceId = "AttendanceOccurrenceId";
            public const string AttendanceOccurrenceIds = "AttendanceOccurrenceIds";
            public const string PersonActionIdentifier = "p";
            public const string IsAccept = "IsAccept";
            public const string AcceptButtonText = "AcceptButtonText";
            public const string AcceptButtonColor = "AcceptButtonColor";
            public const string AcceptButtonFontColor = "AcceptButtonFontColor";
            public const string DeclineButtonText = "DeclineButtonText";
            public const string DeclineButtonColor = "DeclineButtonColor";
            public const string DeclineButtonFontColor = "DeclineButtonFontColor";
        }

        #region Properties

        /// <summary>
        /// Stores data collection for multiple occurrence responses.
        /// </summary>
        private List<OccurrenceDataItem> MultipleOccurrenceDataItems { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
$('input.rsvp-list-input').each(function () {
    var $cbx = $(this)[0];

    if ($cbx.checked) {
        var $header = $(this).closest('header');
        $header.siblings('.panel-body').show();
    }
});

$('input.rsvp-list-input').on('click', function (e) {
    var $cbx = $(this)[0];
    var $header = $(this).closest('header');

    if ($cbx.checked) {
        $header.siblings('.panel-body').slideDown();
        $header.siblings('.panel-body').find('span[id$=rfv]').each(function () {
            document.getElementById($(this).attr('id')).enabled = true;
        });
    } else {
        $header.siblings('.panel-body').slideUp();
        $header.siblings('.panel-body').find('span[id$=rfv]').each(function () {
            document.getElementById($(this).attr('id')).enabled = false;
        });
    }
});

$(document).ready(function () {

    $('.js-rsvp-item').find('span[id$=rfv]').each(function () {
        document.getElementById($(this).attr('id')).enabled = false;
    });

});

";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "DefinedValueChecklistScript", script, true );

            lbAccept_Multiple.Text = GetAttributeValue( AttributeKey.AcceptButtonLabel );
            lbAccept_Single.Text = GetAttributeValue( AttributeKey.AcceptButtonLabel );
            lbDecline_Single.Text = GetAttributeValue( AttributeKey.DeclineButtonLabel );

            SetButtonProperties();

            var person = GetPerson();
            if ( person == null )
            {
                // Invalid person action identifier and/or user is not logged in.
                nbNotAuthorized.Visible = true;
                return;
            }

            if ( !Page.IsPostBack )
            {
                bool isAccept = ( PageParameter( PageParameterKey.IsAccept ) == "1" );
                bool isDecline = ( PageParameter( PageParameterKey.IsAccept ) == "0" );
                var attendanceOccurrenceId = PageParameter( PageParameterKey.AttendanceOccurrenceId ).AsIntegerOrNull();
                var attendanceOccurrenceIdList = GetMultipleOccurrenceIds();

                if ( ( attendanceOccurrenceId == null ) && ( attendanceOccurrenceIdList.Count == 1 ) )
                {
                    // If only one occurrence ID is specified in the list, move it to the individual occurrence ID and treat it as a single RSVP response.
                    attendanceOccurrenceId = attendanceOccurrenceIdList.First();
                }

                if ( attendanceOccurrenceId != null )
                {
                    // Using a single occurrece.
                    if ( isAccept )
                    {
                        if ( GroupHasAttributes() )
                        {
                            // If the group has GroupMember attributes, write the RSVP but show the decision form.
                            WriteEmailAcceptResponse( attendanceOccurrenceId.Value, person );
                            ShowSingleOccurrence_Choice( attendanceOccurrenceId.Value, person );
                        }
                        else
                        {
                            ShowSingleOccurrence_Accept( attendanceOccurrenceId.Value, person );
                        }
                    }
                    else if ( isDecline )
                    {
                        ShowSingleOccurrence_Decline( attendanceOccurrenceId.Value, person );
                    }
                    else
                    {
                        ShowSingleOccurrence_Choice( attendanceOccurrenceId.Value, person );
                    }               
                }
                else
                {
                    if ( attendanceOccurrenceIdList.Any() )
                    {
                        ShowMultipleOccurrence_Choice( attendanceOccurrenceIdList, person );
                    }
                    else
                    {
                        // No occurrence IDs were supplied.
                        Show404();
                    }
                }

            }
            else
            {
                var attendanceOccurrenceId = PageParameter( PageParameterKey.AttendanceOccurrenceId ).AsIntegerOrNull();
                if ( attendanceOccurrenceId != null )
                {
                    BuildAttributeControls();
                }
                var attendanceOccurrenceIdList = GetMultipleOccurrenceIds();
                if ( attendanceOccurrenceIdList.Any() )
                {
                    RebuildMultipleOccurrenceDataItems( attendanceOccurrenceIdList, person );
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };
            ViewState["MultipleOccurrenceDataItems"] = JsonConvert.SerializeObject( MultipleOccurrenceDataItems, Formatting.None, jsonSetting );
            return base.SaveViewState();
        }

        #endregion

        #region Events

        protected void lbAccept_Single_Click( object sender, EventArgs e )
        {
            var person = GetPerson();
            var attendanceOccurrenceId = PageParameter( PageParameterKey.AttendanceOccurrenceId ).AsIntegerOrNull();
            if ( person == null || attendanceOccurrenceId == null )
            {
                // Invalid person action identifier.
                nbNotAuthorized.Visible = true;
                return;
            }

            if ( string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.PersonActionIdentifier ) ) )
            {
                UpdatePersonRecord();
            }

            ShowSingleOccurrence_Accept( attendanceOccurrenceId.Value, person );
        }

        protected void lbDecline_Single_Click( object sender, EventArgs e )
        {
            var person = GetPerson();
            var attendanceOccurrenceId = PageParameter( PageParameterKey.AttendanceOccurrenceId ).AsIntegerOrNull();
            if ( person == null || attendanceOccurrenceId == null )
            {
                // Invalid person action identifier.
                nbNotAuthorized.Visible = true;
                return;
            }

            ShowSingleOccurrence_Decline( attendanceOccurrenceId.Value, person );
        }

        protected void lbAccept_Multiple_Click( object sender, EventArgs e )
        {
            var person = GetPerson();
            var attendanceOccurrenceIdList = GetMultipleOccurrenceIds();
            if ( person == null || !attendanceOccurrenceIdList.Any() )
            {
                // Invalid person action identifier.
                nbNotAuthorized.Visible = true;
                return;
            }

            if ( string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.PersonActionIdentifier ) ) ) 
            {
                UpdatePersonRecord();
            }

            ShowMultipleOccurrence_Accept( attendanceOccurrenceIdList, person );
        }

        protected void lbSaveDeclineReason_Click( object sender, EventArgs e )
        {
            int? declineReason = rrblDeclineReasons.SelectedValueAsInt();
            if ( declineReason.HasValue )
            {
                int occurrenceId = hfDeclineReason_OccurrenceId.Value.AsInteger();
                using ( var rockContext = new RockContext() )
                {
                    var person = GetPerson();
                    var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                    var occurrence = attendanceOccurrenceService.Get( occurrenceId );
                    person = new PersonService( rockContext ).Get( person.Guid );
                    UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.No, null, declineReason.Value, rtbDeclineNote.Text );
                }
                pnlDeclineReasons.Visible = false;
                pnlDeclineReasonConfirmation.Visible = true;
            }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Writes the email accept response when it's necessary to show the choice form.
        /// </summary>
        /// <param name="occurrenceId"></param>
        /// <param name="person"></param>
        private void WriteEmailAcceptResponse( int occurrenceId, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrenceId );
                person = new PersonService( rockContext ).Get( person.Guid );
                UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.Yes );
            }
        }

        /// <summary>
        /// Gets the list of Occurrence IDs from the query string.
        /// </summary>
        private List<int> GetMultipleOccurrenceIds()
        {
            var attendanceOccurrenceIdList = new List<int>();
            string attendanceOccurrenceIds = PageParameter( PageParameterKey.AttendanceOccurrenceIds );
            if ( !string.IsNullOrWhiteSpace( attendanceOccurrenceIds ) )
            {
                try
                {
                    attendanceOccurrenceIdList = attendanceOccurrenceIds.Split( ',' ).Select( int.Parse ).ToList();
                }
                catch
                {
                    /* Ignore failures to convert query string to integer values. */
                }
            }
            return attendanceOccurrenceIdList;
        }

        /// <summary>
        /// Returns a Person record for a PersonActionIdentifier for the action type "RSVP", or the currently logged in person if no PersonActionIdentifier is present.
        /// </summary>
        /// <returns></returns>
        private Person GetPerson()
        {
            string personActionIdentifier = PageParameter( PageParameterKey.PersonActionIdentifier );
            if ( !string.IsNullOrWhiteSpace( personActionIdentifier ) )
            {
                // Get Person record from PersonActionIdentifier.
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    return personService.GetByPersonActionIdentifier( personActionIdentifier, "RSVP" );
                }
            }
            else
            {
                    // Set Person record to logged in person.
                    return CurrentPerson;
            }

        }

        /// <summary>
        /// Display a "not found" message for actions which cannot be performed.
        /// </summary>
        /// <param name="PageTitle">The optional page title to display.</param>
        private void Show404( bool isExpired = false, string PageTitle = "" )
        {
            //Context.Response.StatusCode = 404;
            pnl404.Visible = true;
            pnlForm.Visible = false;
            pnlMultiple_Accept.Visible = false;
            pnlMultiple_Choice.Visible = false;
            pnlSingle_Accept.Visible = false;
            pnlSingle_Choice.Visible = false;
            pnlSingle_Decline.Visible = false;

            if ( isExpired )
            {
                if ( string.IsNullOrWhiteSpace( PageTitle ) )
                {
                    pnlHeading.Visible = false;
                }
                else
                {
                    lHeading.Text = PageTitle;
                }

                nbExpired.Visible = true;
            }
            else
            {
                pnlHeading.Visible = false;
                nbNotFound.Visible = true;
            }
        }

        /// <summary>
        /// Calculates the display title for an <see cref="AttendanceOccurrence"/>.
        /// </summary>
        /// <param name="occurrence">The <see cref="AttendanceOccurrence"/>.</param>
        private string GetOccurrenceTitle( AttendanceOccurrence occurrence )
        {
            bool hasTitle = ( !string.IsNullOrWhiteSpace( occurrence.Name ) );
            bool hasSchedule = ( occurrence.Schedule != null );

            if ( hasSchedule )
            {
                // This block is unnecessary if the event has a name (because the name will take priority over the schedule, anyway), but it
                // has been intentionally left in place to prevent anyone from creating an unintentional bug in the future, as it affects
                // the logic below.
                var calendarEvent = occurrence.Schedule.GetICalEvent();
                if ( calendarEvent == null )
                {
                    hasSchedule = false;
                }
            }

            if ( hasTitle )
            {
                return occurrence.Name;
            }
            else if ( hasSchedule )
            {
                return string.Format(
                    "{0} - {1}, {2}",
                    occurrence.Group.Name,
                    occurrence.OccurrenceDate.ToString( "dddd, MMMM d, yyyy" ),
                    occurrence.Schedule.GetICalEvent().DtStart.Value.TimeOfDay.ToTimeString() );
            }
            else
            {
                return string.Format(
                    "{0} - {1}",
                    occurrence.Group.Name,
                    occurrence.OccurrenceDate.ToString( "dddd, MMMM d, yyyy" ) );
            }
        }

        /// <summary>
        /// Shows the Accept/Decline options to allow RSVP for a single occurrence.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence.</param>
        /// <param name="person">The Person record of the respondent.</param>
        private void ShowSingleOccurrence_Choice( int occurrenceId, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = attendanceOccurrenceService.Get( occurrenceId );

                if ( occurrence == null )
                {
                    Show404();
                    return;
                }
                else if ( occurrence.OccurrenceDate.EndOfDay() < DateTime.Now )
                {
                    // This event has expired.
                    Show404( true, GetOccurrenceTitle( occurrence ) );
                    return;
                }

                lHeading.Text = GetOccurrenceTitle( occurrence );
                pnlSingle_Choice.Visible = true;

                var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupId = occurrence.Group.Id;
                    groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;
                }

                bool displayForm = GetAttributeValue( AttributeKey.DisplayFormWhenSignedIn ).AsBoolean();
                pnlForm.Visible = ( CurrentPersonId == null || displayForm );

                if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.PersonActionIdentifier ) ) )
                {
                    rtbFirstName.Enabled = false;
                    rtbLastName.Enabled = false;
                    rebEmail.Enabled = false;
                }

                rtbFirstName.Text = person.FirstName;
                rtbLastName.Text = person.LastName;
                rebEmail.Text = person.Email;

                groupMember.LoadAttributes();

                // This collection object is created to limit attribute values to those marked "IsPublic".
                var publicAttributes = new GroupMemberPublicAttributeCollection( groupMember );
                if ( publicAttributes.Attributes.Any() )
                {
                    Helper.AddEditControls( publicAttributes, phAttributes, true );
                }
            }
        }

        /// <summary>
        /// Rebuilds the dynamic attribute value controls (for single occurrence mode) after a postback.
        /// </summary>
        private void BuildAttributeControls()
        {
            using ( var rockContext = new RockContext() )
            {
                var person = GetPerson();
                var occurrenceId = PageParameter( PageParameterKey.AttendanceOccurrenceId ).AsInteger();
                var occurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrenceId );
                var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupId = occurrence.Group.Id;
                    groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;
                }
                var publicAttributes = new GroupMemberPublicAttributeCollection( groupMember );
                if ( publicAttributes.Attributes.Any() )
                {
                    Helper.AddEditControls( publicAttributes, phAttributes, false );
                }
            }
        }

        /// <summary>
        /// Tests the group to see if there are any GroupMember attributes.
        /// </summary>
        /// <returns></returns>
        private bool GroupHasAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var person = GetPerson();
                var occurrenceId = PageParameter( PageParameterKey.AttendanceOccurrenceId ).AsInteger();
                var occurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrenceId );
                var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupId = occurrence.Group.Id;
                    groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;
                }

                groupMember.LoadAttributes();
                var publicAttributes = new GroupMemberPublicAttributeCollection( groupMember );
                return publicAttributes.Attributes.Any();
            }
        }

        /// <summary>
        /// Shows the RSVP Accept message for a single occurrence.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence.</param>
        /// <param name="person">The Person record of the respondent.</param>
        private void ShowSingleOccurrence_Accept( int occurrenceId, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = attendanceOccurrenceService.Get( occurrenceId );

                if ( occurrence == null )
                {
                    Show404();
                    return;
                }
                else if ( occurrence.OccurrenceDate.EndOfDay() < DateTime.Now )
                {
                    // This event has expired.
                    Show404( true, GetOccurrenceTitle( occurrence ) );
                    return;
                }

                person = new PersonService( rockContext ).Get( person.Guid );
                UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.Yes, phAttributes );

                // Show Single Occurrence Accept message.
                pnlSingle_Accept.Visible = true;
                pnlSingle_Choice.Visible = false;
                pnlForm.Visible = false;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                if ( !string.IsNullOrEmpty( occurrence.AcceptConfirmationMessage ) )
                {
                    nbAccept.Text = occurrence.AcceptConfirmationMessage.ResolveMergeFields( mergeFields );
                }
                else
                {
                    nbAccept.Text = GetAttributeValue( AttributeKey.DefaultAcceptMessage ).ResolveMergeFields( mergeFields );
                }
            }
        }

        /// <summary>
        /// Creates a new attendance record or updates an existing one if it already exists.
        /// </summary>
        /// <param name="occurrence">The AttendanceOccurrence record.</param>
        /// <param name="person">The Person record.</param>
        /// <param name="rockContext">The RockContext</param>
        /// <param name="rsvpStatus">The Rock.Model.RSVP enum value to set.</param>
        /// <param name="attributePlaceHolder">(Optional) PlaceHolder control that contains the GroupMember attribute values to set.</param>
        /// <param name="declineReasonId">(Optional) The DefinedValue ID of a Decline Reason, if one was selected.  Only used if rsvpStatus is No.</param>
        /// <param name="declineNote">(Optional) An explanation of the reason for declining.  Only used if rsvpStatus is No.</param>
        private void UpdateOrCreateAttendanceRecord( AttendanceOccurrence occurrence, Person person, RockContext rockContext, Rock.Model.RSVP rsvpStatus, PlaceHolder attributePlaceHolder = null, int declineReasonId = 0, string declineNote = "" )
        {
            var attendance = occurrence.Attendees.Where( a => person.Aliases.Contains( a.PersonAlias ) ).FirstOrDefault();
            if ( attendance == null )
            {
                attendance = new Attendance();
                attendance.OccurrenceId = occurrence.Id;
                attendance.PersonAliasId = person.PrimaryAliasId;
                attendance.StartDateTime = occurrence.Schedule != null && occurrence.Schedule.HasSchedule() ? occurrence.OccurrenceDate.Date.Add( occurrence.Schedule.StartTimeOfDay ) : occurrence.OccurrenceDate;
                occurrence.Attendees.Add( attendance );
            }
            attendance.RSVP = rsvpStatus;
            attendance.RSVPDateTime = DateTime.Now;
            if ( rsvpStatus == Rock.Model.RSVP.No )
            {
                if ( declineReasonId != 0 )
                {
                    attendance.DeclineReasonValueId = declineReasonId;
                }
                attendance.Note = declineNote;
            }

            // Note that GroupMember attributes are being set, here.  If this control saves multiple attendance records for a same group (e.g., the same group meets on multiple dates and the user RSVPs to
            // more than one) it will overwrite values.
            if ( ( attributePlaceHolder != null ) && ( rsvpStatus == Rock.Model.RSVP.Yes ) )
            {
                var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupId = occurrence.Group.Id;
                    groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;

                    new GroupMemberService( rockContext ).Add( groupMember );
                    rockContext.SaveChanges();
                }

                groupMember.LoadAttributes();

                Helper.GetEditValues( attributePlaceHolder, groupMember );

                groupMember.SaveAttributeValues();
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Shows the RSVP Decline message for a single occurrence.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence.</param>
        /// <param name="person">The Person record of the respondent.</param>
        private void ShowSingleOccurrence_Decline( int occurrenceId, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = attendanceOccurrenceService.Get( occurrenceId );

                if ( occurrence == null )
                {
                    Show404();
                    return;
                }
                else if ( occurrence.OccurrenceDate.EndOfDay() < DateTime.Now )
                {
                    // This event has expired.
                    Show404( true, GetOccurrenceTitle( occurrence ) );
                    return;
                }

                pnlForm.Visible = false;
                pnlSingle_Choice.Visible = false;

                person = new PersonService( rockContext ).Get( person.Guid );
                UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.No );
                hfDeclineReason_OccurrenceId.Value = occurrenceId.ToString();

                // Show Single Occurrence Decline form.
                pnlSingle_Decline.Visible = true;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                if ( !string.IsNullOrEmpty( occurrence.DeclineConfirmationMessage ) )
                {
                    nbDecline.Text = occurrence.DeclineConfirmationMessage.ResolveMergeFields( mergeFields );
                }
                else
                {
                    nbDecline.Text = GetAttributeValue( AttributeKey.DefaultDeclineMessage ).ResolveMergeFields( mergeFields );
                }

                if ( occurrence.ShowDeclineReasons == true )
                {
                    // Show Decline Reasons.
                    string declineReasons = occurrence.DeclineReasonValueIds;
                    if ( string.IsNullOrWhiteSpace( declineReasons ) )
                    {
                        // Use default decline reasons (block setting).
                        declineReasons = GetAttributeValue( AttributeKey.DefaultDeclineReasons );
                    }

                    var declineReasonValues = GetDeclineReasons( declineReasons );
                    if ( declineReasonValues.Any() )
                    {
                        rrblDeclineReasons.DataSource = declineReasonValues;
                        rrblDeclineReasons.DataBind();
                        pnlDeclineReasons.Visible = true;
                    }
                    else
                    {
                        pnlDeclineReasons.Visible = false;
                    }
                }
            }
        }

        protected class OccurrenceDataItem
        {
            public string Title { get; set; }
            public string OccurrenceId { get; set; }
            public GroupMemberPublicAttributeCollection PublicAttributes { get; set; }
        };

        /// <summary>
        /// Shows the Accept/Decline options to allow RSVP for muiltiple occurrences.
        /// </summary>
        /// <param name="occurrenceIds">The List of IDs of the AttendanceOccurrences.</param>
        /// <param name="person">The Person record of the respondent.</param>
        private void ShowMultipleOccurrence_Choice( List<int> occurrenceIds, Person person )
        {
            lHeading.Text = GetAttributeValue( AttributeKey.MultigroupModeRSVPTitle );

            bool displayForm = GetAttributeValue( AttributeKey.DisplayFormWhenSignedIn ).AsBoolean();
            pnlForm.Visible = ( CurrentPersonId == null || displayForm );

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.PersonActionIdentifier ) ) )
            {
                rtbFirstName.Enabled = false;
                rtbLastName.Enabled = false;
                rebEmail.Enabled = false;
            }

            rtbFirstName.Text = person.FirstName;
            rtbLastName.Text = person.LastName;
            rebEmail.Text = person.Email;

            bool hasValidOccurrences = false;
            bool isExpired = false;

            using ( var rockContext = new RockContext() )
            {
                List<OccurrenceDataItem> repeaterItems = new List<OccurrenceDataItem>();
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                foreach ( int occurrenceId in occurrenceIds )
                {
                    var occurrence = attendanceOccurrenceService.Get( occurrenceId );
                    if ( occurrence.OccurrenceDate.EndOfDay() < DateTime.Now )
                    {
                        // This event has expired.
                        isExpired = true;
                        continue;
                    }

                    // At least one occurrence is valid.
                    hasValidOccurrences = true;

                    var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                    if ( groupMember == null )
                    {
                        //Person is not a member of the group associated with this invitation.
                        groupMember = new GroupMember();
                        groupMember.PersonId = person.Id;
                        groupMember.GroupId = occurrence.Group.Id;
                        groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;
                    }

                    groupMember.LoadAttributes();

                    // This collection object is created to limit attribute values to those marked "IsPublic".
                    var publicAttributes = new GroupMemberPublicAttributeCollection( groupMember );

                    // Add item to collection for data binding.
                    repeaterItems.Add(
                        new OccurrenceDataItem()
                        {
                            Title = GetOccurrenceTitle( occurrence ),
                            OccurrenceId = occurrenceId.ToString(),
                            PublicAttributes = publicAttributes
                        } );
                }

                /// If no valid occurrences were found, display "Not Found" panel.
                if ( !hasValidOccurrences )
                {
                    Show404( isExpired, GetAttributeValue( AttributeKey.MultigroupModeRSVPTitle ) );
                }
                else
                {
                    pnlMultiple_Choice.Visible = true;
                    MultipleOccurrenceDataItems = repeaterItems;
                    BindMultipleOccurrenceRepeater();
                }
            }
        }

        private void RebuildMultipleOccurrenceDataItems( List<int> occurrenceIds, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                List<OccurrenceDataItem> repeaterItems = new List<OccurrenceDataItem>();
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                foreach ( int occurrenceId in occurrenceIds )
                {
                    var occurrence = attendanceOccurrenceService.Get( occurrenceId );
                    if ( occurrence.OccurrenceDate.EndOfDay() < DateTime.Now )
                    {
                        continue;
                    }

                    var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                    if ( groupMember == null )
                    {
                        //Person is not a member of the group associated with this invitation.
                        groupMember = new GroupMember();
                        groupMember.PersonId = person.Id;
                        groupMember.GroupId = occurrence.Group.Id;
                        groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;
                    }

                    groupMember.LoadAttributes();

                    // This collection object is created to limit attribute values to those marked "IsPublic".
                    var publicAttributes = new GroupMemberPublicAttributeCollection( groupMember );

                    // Add item to collection for data binding.
                    repeaterItems.Add(
                        new OccurrenceDataItem()
                        {
                            Title = GetOccurrenceTitle( occurrence ),
                            OccurrenceId = occurrenceId.ToString(),
                            PublicAttributes = publicAttributes
                        } );
                }

                MultipleOccurrenceDataItems = repeaterItems;
                BindMultipleOccurrenceRepeater();
            }
        }

        /// <summary>
        /// Binds the repeater control for multiple occurrences.
        /// </summary>
        private void BindMultipleOccurrenceRepeater()
        {
            rptrValues.DataSource = MultipleOccurrenceDataItems;
            rptrValues.DataBind();
        }

        /// <summary>
        /// Shows the RSVP Accept message for a single occurrence.
        /// </summary>
        /// <param name="occurrenceIds">The List of IDs of the AttendanceOccurrences.</param>
        /// <param name="person">The Person record of the respondent.</param>
        private void ShowMultipleOccurrence_Accept( List<int> occurrenceIds, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                _processedOccurrences = new List<string>();
                bool occurrenceProcessed = false;
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                foreach ( RepeaterItem item in rptrValues.Items )
                {
                    if ( ProcessOccurrence( person, item, rockContext ) )
                    {
                        occurrenceProcessed = true;
                    }
                }

                // If no occurrences were selected, do nothing.
                if ( occurrenceProcessed )
                {
                    // Show Multiple Occurrence Accept message.
                    pnlMultiple_Accept.Visible = true;
                    pnlMultiple_Choice.Visible = false;
                    pnlForm.Visible = false;

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                    mergeFields.Add( "AcceptedRsvps", _processedOccurrences );
                    nbAcceptMultiple.Text = GetAttributeValue( AttributeKey.MultigroupAcceptMessage ).ResolveMergeFields( mergeFields );
                    nbNoOccurrencesSelected.Visible = false;
                }
                else
                {
                    nbNoOccurrencesSelected.Visible = true;
                }
            }
        }

        /// <summary>
        /// Stores a list of occurrences that were accepted, for inclusion in the Lava accept message.
        /// </summary>
        private List<string> _processedOccurrences;

        /// <summary>
        /// Processes a single RSVP Occurrence from data contained in a PanelWidget control.
        /// </summary>
        /// <param name="person">The Person record.</param>
        /// <param name="widget">The PanelWidget control.</param>
        /// <param name="rockContext">The RockContext.</param>
        /// <returns></returns>
        private bool ProcessOccurrence( Person person, RepeaterItem item, RockContext rockContext )
        {
            RockCheckBox rcbAccept = item.FindControl( "rcbAccept" ) as RockCheckBox;
            if ( rcbAccept.Checked )
            {
                HiddenField hfOccurrenceId = item.FindControl( "hfOccurrenceId" ) as HiddenField;
                PlaceHolder phOccurrenceAttributes = item.FindControl( "phOccurrenceAttributes" ) as PlaceHolder;

                int occurrenceId = int.Parse( hfOccurrenceId.Value );
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = attendanceOccurrenceService.Get( occurrenceId );

                person = new PersonService( rockContext ).Get( person.Guid );
                UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.Yes, phOccurrenceAttributes );
                _processedOccurrences.Add( GetOccurrenceTitle( occurrence ) );
            }
            return rcbAccept.Checked;
        }

        /// <summary>
        /// Updates the person record with name and email address fields.  This method is only called when NOT using a PersonActionIdentifier (i.e., the user must be logged in).
        /// </summary>
        private void UpdatePersonRecord()
        {
            using ( var rockContext = new RockContext() )
            {
                CurrentPerson.FirstName = rtbFirstName.Text;
                CurrentPerson.LastName = rtbLastName.Text;
                CurrentPerson.Email = rebEmail.Text;

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Get Decline Reason DefinedValues for a list of IDs.
        /// </summary>
        /// <param name="commaDelimitedDeclineReasons">The IDs of the DevinedValues to retrieve.</param>
        /// <returns></returns>
        protected List<DefinedValue> GetDeclineReasons( string commaDelimitedDeclineReasons )
        {
            List<DefinedValue> values = new List<DefinedValue>();
            List<int> declineReasonIds = new List<int>();
            if ( !string.IsNullOrWhiteSpace( commaDelimitedDeclineReasons ) )
            {
                declineReasonIds = commaDelimitedDeclineReasons.Split( ',' ).Select( int.Parse ).ToList();
            }

            if ( !declineReasonIds.Any() )
            {
                return values;
            }

            using ( var rockContext = new RockContext() )
            {
                var def = new DefinedValueService( rockContext );
                values = def.Queryable()
                    .Where( v => declineReasonIds.Contains( v.Id ) )
                    .AsNoTracking().ToList();
            }

            return values;
        }

        /// <summary>
        /// Sets the button style and text properties to match query string values passed in by the email editor.
        /// </summary>
        private void SetButtonProperties()
        {
            string acceptButtonText = PageParameter( PageParameterKey.AcceptButtonText );
            string acceptButtonColor = PageParameter( PageParameterKey.AcceptButtonColor );
            string acceptButtonFontColor = PageParameter( PageParameterKey.AcceptButtonFontColor );
            string declineButtonText = PageParameter( PageParameterKey.DeclineButtonText );
            string declineButtonColor = PageParameter( PageParameterKey.DeclineButtonColor );
            string declineButtonFontColor = PageParameter( PageParameterKey.DeclineButtonFontColor );

            if ( !string.IsNullOrWhiteSpace( acceptButtonText ) )
            {
                lbAccept_Multiple.Text = acceptButtonText;
                lbAccept_Single.Text = acceptButtonText;
            }

            if ( !string.IsNullOrWhiteSpace( declineButtonText ) )
            {
                lbDecline_Single.Text = declineButtonText;
            }

            string acceptButtonStyle = string.Empty;
            if ( !string.IsNullOrWhiteSpace( acceptButtonColor ) )
            {
                acceptButtonStyle = "background-color: " + acceptButtonColor + ";";
            }
            if ( !string.IsNullOrWhiteSpace( acceptButtonFontColor ) )
            {
                acceptButtonStyle = acceptButtonStyle + "color: " + acceptButtonFontColor + ";";
            }
            if ( !string.IsNullOrWhiteSpace( acceptButtonStyle ) )
            {
                lbAccept_Multiple.CssClass = "btn";
                lbAccept_Multiple.Attributes.Remove( "style" );
                lbAccept_Multiple.Attributes.Add( "style", acceptButtonStyle );
                lbAccept_Single.CssClass = "btn";
                lbAccept_Single.Attributes.Remove( "style" );
                lbAccept_Single.Attributes.Add( "style", acceptButtonStyle );
            }
            else
            {
                lbAccept_Multiple.CssClass = "btn btn-primary";
                lbAccept_Single.CssClass = "btn btn-primary";
            }


            string declineButtonStyle = string.Empty;
            if ( !string.IsNullOrWhiteSpace( declineButtonColor ) )
            {
                declineButtonStyle = "background-color: " + declineButtonColor + ";";
            }
            if ( !string.IsNullOrWhiteSpace( declineButtonFontColor ) )
            {
                declineButtonStyle = declineButtonStyle + "color: " + declineButtonFontColor + ";";
            }
            if ( !string.IsNullOrWhiteSpace( declineButtonStyle ) )
            {
                lbDecline_Single.CssClass = "btn";
                lbDecline_Single.Attributes.Remove( "style" );
                lbDecline_Single.Attributes.Add( "style", declineButtonStyle );
            }
            else
            {
                lbDecline_Single.CssClass = "btn btn-default";
            }
        }

        #endregion

        protected void rptrValues_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var dataItem = e.Item.DataItem as OccurrenceDataItem;
            var phOccurrenceAttributes = e.Item.FindControl( "phOccurrenceAttributes" );
            if ( dataItem.PublicAttributes.Attributes.Any() )
            {
                Helper.AddEditControls( dataItem.PublicAttributes, phOccurrenceAttributes, !Page.IsPostBack );
            }
        }
    }

    #region Helper Classes

    /// <summary>
    /// This class is used to obtain a list of attributes which are marked IsPublic.
    /// </summary>
    public class GroupMemberPublicAttributeCollection : IHasAttributes
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues" /> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the associated attribute value
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, AttributeValueCache> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attribute value defaults.  This property can be used by a subclass to override the parent class's default
        /// value for an attribute
        /// </summary>
        /// <value>
        /// The attribute value defaults.
        /// </value>
        public Dictionary<string, string> AttributeValueDefaults
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                return this.AttributeValues[key].Value;
            }

            if ( this.Attributes != null &&
                this.Attributes.ContainsKey( key ) )
            {
                return this.Attributes[key].DefaultValue;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A list of string values or an empty list if none exist.
        /// </returns>
        public List<string> GetAttributeValues( string key )
        {
            string value = GetAttributeValue( key );
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                return value.SplitDelimitedValues().ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                this.AttributeValues[key].Value = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberPublicAttributeCollection"/> class.
        /// </summary>
        public GroupMemberPublicAttributeCollection( GroupMember groupMember )
        {
            Id = groupMember.Id;
            groupMember.LoadAttributes();
            Attributes = groupMember.Attributes.Where( a => a.Value.IsPublic == true ).ToDictionary( a => a.Key, a => a.Value );
            AttributeValues = groupMember.AttributeValues.Where( a => Attributes.Keys.Contains( a.Value.AttributeKey ) ).ToDictionary( a => a.Key, a => a.Value );
        }

        public GroupMemberPublicAttributeCollection() { }
    }

    public static class DateEndOfDayStaticFunction
    {
        public static DateTime EndOfDay( this DateTime input )
        {
            return input.Date.AddDays( 1 ).AddMilliseconds( -1 );
        }
    }
    #endregion
}
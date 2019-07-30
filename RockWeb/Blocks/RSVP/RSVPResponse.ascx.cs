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

namespace RockWeb.Blocks.RSVP
{
    /// <summary>
    /// Displays the details of the given RSVP occurrence.
    /// </summary>
    [DisplayName( "RSVP Response Block" )]
    [Category( "RSVP" )]
    [Description( "Allowes invited people to RSVP for one or more Attendane Occurrences." )]

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

    [BooleanField( "Enable Multigroup Mode",
        Key = AttributeKey.EnableMultigroupMode,
        Description = "If Multigroup Mode is enabled, this block will allow users to RSVP for multiple groups at once.",
        DefaultBooleanValue = true,
        Order = 7 )]

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

    [MemoField( "Multigroup Decline Message",
        Key = AttributeKey.MultigroupDeclineMessage,
        Description = "This field is not used.",
        DefaultValue = "",
        Order = 10 )]

    #endregion

    public partial class RSVPResponse : RockBlock
    {
        protected static class AttributeKey
        {
            public const string DisplayFormWhenSignedIn = "DisplayFormWhenSignedIn";
            public const string AcceptButtonLabel = "AcceptButtonLabel";
            public const string DeclineButtonLabel = "DeclineButtonLabel";
            public const string DefaultAcceptMessage = "DefaultAcceptMessage";
            public const string DefaultDeclineMessage = "DefaultDeclineMessage";
            public const string DefaultDeclineReasons = "DefaultDeclineReasons";
            public const string EnableMultigroupMode = "MultigroupModePageTitle";
            public const string MultigroupModeRSVPTitle = "MultigroupModeRSVPTitle";
            public const string MultigroupAcceptMessage = "MultigroupAcceptMessage";
            public const string MultigroupDeclineMessage = "MultigroupDeclineMessage";
        }

        protected static class PageParameterKey
        {
            public const string AttendanceOccurrenceId = "AttendanceOccurrenceId";
            public const string AttendanceOccurrenceIds = "AttendanceOccurrenceIds";
            public const string PersonActionIdentifier = "p";
            public const string IsAccept = "IsAccept";
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbAccept_Multiple.Text = GetAttributeValue( AttributeKey.AcceptButtonLabel );
            lbAccept_Single.Text = GetAttributeValue( AttributeKey.AcceptButtonLabel );
            lbDecline_Single.Text = GetAttributeValue( AttributeKey.DeclineButtonLabel );

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
                        ShowSingleOccurrence_Accept( attendanceOccurrenceId.Value, person );
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
                var attendanceOccurrenceIdList = GetMultipleOccurrenceIds();
                if ( attendanceOccurrenceIdList.Any() )
                {
                    // rebuild Multiple Occurrence Placeholders here so that postback values can be read.
                    BuildMultipleOccurrenceControls( attendanceOccurrenceIdList, person );
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

            if ( string.IsNullOrWhiteSpace(PageParameter( PageParameterKey.PersonActionIdentifier ) ) ) 
            {
                UpdatePersonRecord();
            }

            ShowMultipleOccurrence_Accept( attendanceOccurrenceIdList, person );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the list of Occurrence IDs from the query string.
        /// </summary>
        private List<int> GetMultipleOccurrenceIds()
        {
            var attendanceOccurrenceIdList = new List<int>();
            string attendanceOccurrenceIds = PageParameter( PageParameterKey.AttendanceOccurrenceId );
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
            Context.Response.StatusCode = 404;
            pnl404.Visible = true;

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

                if ( occurrence.OccurrenceDate < DateTime.Now )
                {
                    // This event has expired.
                    Show404( true, occurrence.EntityStringValue );
                    return;
                }

                lHeading.Text = occurrence.EntityStringValue;

                var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    throw new Exception( "Person is not a member of the group associated with this invitation." );
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

                // This collection object is created to limit attribute values to those marked "IsPublic".
                var publicAttributes = new GroupMemberPublicAttriuteCollection( groupMember );
                if ( publicAttributes.Attributes.Any() )
                {
                    Helper.AddEditControls( publicAttributes, phAttributes, false, BlockValidationGroup );
                }
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

                if ( occurrence.OccurrenceDate < DateTime.Now )
                {
                    // This event has expired.
                    Show404();
                    return;
                }

                UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.Yes );

                // Show Single Occurrence Accept message.
                pnlSingle_Accept.Visible = true;
                pnlSingle_Choice.Visible = false;

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
            if ( attributePlaceHolder != null )
            {
                var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                if ( groupMember == null )
                {
                    throw new Exception( "Person is not a member of the group associated with this invitation." );
                }
                Helper.GetEditValues( phAttributes, groupMember );
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

                if ( occurrence.OccurrenceDate < DateTime.Now )
                {
                    // This event has expired.
                    Show404();
                    return;
                }

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
                    string declineReasons = occurrence.DeclineReasons;
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
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                foreach ( int occurrenceId in occurrenceIds )
                {
                    var occurrence = attendanceOccurrenceService.Get( occurrenceId );
                    if ( occurrence.OccurrenceDate < DateTime.Now )
                    {
                        // This event has expired.
                        isExpired = true;
                        continue;
                    }

                    var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                    if ( groupMember == null )
                    {
                        //Person is not a member of the group associated with this invitation.
                        continue;
                    }

                    RockCheckBox rcbAccept = new RockCheckBox()
                    {
                        ID = "rcbAccept",
                        Text = occurrence.EntityStringValue
                    };

                    PanelWidget widget = new PanelWidget();
                    widget.ID = "wOccurrence";
                    widget.HeaderControls = new Control[1] { rcbAccept };

                    HiddenField hfOccurrenceId = new HiddenField() { ID = "hfOccurrenceId", Value = occurrenceId.ToString() };
                    widget.Controls.Add( hfOccurrenceId );

                    PlaceHolder phOccurrenceAttributes = new PlaceHolder() { ID = "phOccurrenceAttributes" };

                    // This collection object is created to limit attribute values to those marked "IsPublic".
                    var publicAttributes = new GroupMemberPublicAttriuteCollection( groupMember );
                    if ( publicAttributes.Attributes.Any() )
                    {
                        Helper.AddEditControls( publicAttributes, phOccurrenceAttributes, false, BlockValidationGroup );
                    }

                    widget.Controls.Add( phOccurrenceAttributes );
                    phOccurrences.Controls.Add( widget );
                }

                /// If no valid occurrences were found, display "Not Found" panel.
                if ( !hasValidOccurrences )
                {
                    Show404( isExpired, GetAttributeValue( AttributeKey.MultigroupModeRSVPTitle ) );
                }
            }
        }

        /// <summary>
        /// Builds the dynamic controls for multiple occurrence RSVP.
        /// </summary>
        /// <param name="occurrenceIds">The list of Occurrence IDs to display.</param>
        /// <param name="person">The Person record.</param>
        private void BuildMultipleOccurrenceControls( List<int> occurrenceIds, Person person )
        {
            bool hasValidOccurrences = false;
            bool isExpired = false;
            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                foreach ( int occurrenceId in occurrenceIds )
                {
                    var occurrence = attendanceOccurrenceService.Get( occurrenceId );
                    if ( occurrence.OccurrenceDate < DateTime.Now )
                    {
                        // This event has expired.
                        isExpired = true;
                        continue;
                    }

                    var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
                    if ( groupMember == null )
                    {
                        //Person is not a member of the group associated with this invitation.
                        continue;
                    }

                    RockCheckBox rcbAccept = new RockCheckBox()
                    {
                        ID = "rcbAccept",
                        Text = occurrence.EntityStringValue
                    };

                    PlaceHolder phOccurrenceAttributes = new PlaceHolder() { ID = "phOccurrenceAttributes" };
                    // This collection object is created to limit attribute values to those marked "IsPublic".
                    var publicAttributes = new GroupMemberPublicAttriuteCollection( groupMember );
                    if ( publicAttributes.Attributes.Any() )
                    {
                        Helper.AddEditControls( publicAttributes, phOccurrenceAttributes, false, BlockValidationGroup );
                    }

                    PanelWidget widget = new PanelWidget();
                    widget.ID = "wOccurrence";
                    widget.HeaderControls = new Control[1] { rcbAccept };
                    widget.Controls.Add( phOccurrenceAttributes );
                    phOccurrences.Controls.Add( widget );
                }

                /// If no valid occurrences were found, display "Not Found" panel.
                if ( !hasValidOccurrences )
                {
                    Show404( isExpired, GetAttributeValue( AttributeKey.MultigroupModeRSVPTitle ) );
                }
            }
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
                foreach (Control control in phOccurrences.Controls)
                {
                    PanelWidget widget = control as PanelWidget;
                    if ( widget != null )
                    {
                        occurrenceProcessed = ProcessOccurrence( person, widget, rockContext );
                    }
                }

                // If no occurrences were selected, do nothing.
                if ( occurrenceProcessed )
                {
                    // Show Multiple Occurrence Accept message.
                    pnlMultiple_Accept.Visible = true;
                    pnlMultiple_Choice.Visible = false;

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                    mergeFields.Add( "AcceptedRsvps", _processedOccurrences );
                    nbAcceptMultiple.Text = GetAttributeValue( AttributeKey.MultigroupAcceptMessage ).ResolveMergeFields( mergeFields );
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
        private bool ProcessOccurrence( Person person, PanelWidget widget, RockContext rockContext )
        {
            RockCheckBox rcbAccept = widget.FindControl( "rcbAccept" ) as RockCheckBox;
            if ( rcbAccept.Checked )
            {
                HiddenField hfOccurrenceId = widget.FindControl( "hfOccurrenceId" ) as HiddenField;
                PlaceHolder phOccurrenceAttributes = widget.FindControl("phOccurrenceAttributes") as PlaceHolder;

                int occurrenceId = int.Parse( hfOccurrenceId.Value );
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = attendanceOccurrenceService.Get( occurrenceId );

                UpdateOrCreateAttendanceRecord( occurrence, person, rockContext, Rock.Model.RSVP.Yes, phOccurrenceAttributes );
                _processedOccurrences.Add( occurrence.EntityStringValue );
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
            var declineReasonIds = commaDelimitedDeclineReasons.Split( ',' ).Select( int.Parse ).ToList();
            if ( !declineReasonIds.Any() )
            {
                return values;
            }

            using ( var rockContext = new RockContext() )
            {
                var def = new DefinedValueService( rockContext );
                values = def.Queryable()
                    .Where(v => declineReasonIds.Contains( v.Id ) )
                    .AsNoTracking().ToList();
            }

            return values;
        }

        #endregion

    }

    #region Helper Class

    /// <summary>
    /// This class is used to obtain a list of attributes which are marked IsPublic.
    /// </summary>
    public class GroupMemberPublicAttriuteCollection : IHasAttributes
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
        /// Initializes a new instance of the <see cref="GroupMemberPublicAttriuteCollection"/> class.
        /// </summary>
        public GroupMemberPublicAttriuteCollection( GroupMember groupMember )
        {
            Id = groupMember.Id;
            groupMember.LoadAttributes();
            Attributes = groupMember.Attributes.Where( a => a.Value.IsPublic == true ).ToDictionary( a => a.Key, a => a.Value );
            groupMember.AttributeValues.Where( a => Attributes.Keys.Contains( a.Value.AttributeKey ) ).ToDictionary( a => a.Key, a => a.Value );
            AttributeValues = new Dictionary<string, AttributeValueCache>();
        }
    }

    #endregion
}
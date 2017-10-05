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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Email Preference Entry" )]
    [Category( "Communication" )]
    [Description( "Allows user to set their email preference or unsubscribe from a communication list." )]

    [MemoField( "Unsubscribe from List Text", "Text to display for the 'Unsubscribe me from list' option.", false, "Please unsubscribe me from emails regarding '{{ Communication.ListGroup | Attribute:'PublicName' | Default:Communication.ListGroup.Name }}'", "", 1, null, 3, true )]
    [MemoField( "Emails Allowed Text", "Text to display for the 'Emails Allowed' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, and wish to receive all emails.", "", 2, null, 3, true )]
    [MemoField( "No Mass Emails Text", "Text to display for the 'No Mass Emails' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not wish to receive mass emails (personal emails are fine).", "", 3, null, 3, true )]
    [MemoField( "No Emails Text", "Text to display for the 'No Emails' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not want to receive emails of ANY kind.", "", 4, null, 3, true )]
    [MemoField( "Not Involved Text", "Text to display for the 'Not Involved' option.", false, " I am no longer involved with {{ 'Global' | Attribute:'OrganizationName' }}.", "", 5, null, 3, true )]
    [MemoField( "Success Text", "Text to display after user submits selection.", false, "<h4>Thank You</h4>We have saved your email preference.", "", 6, null, 3, true )]
    [MemoField( "Unsubscribe Success Text", "Text to display after user unsubscribes from a list.", false, "<h4>Thank You</h4>We have saved your removed you from the '{{ Communication.ListGroup | Attribute:'PublicName' | Default:Communication.ListGroup.Name }}' list.", "", 7, null, 3, true )]
    [TextField( "Reasons to Exclude", "A delimited list of the Inactive Reasons to exclude from Reason list", false, "No Activity,Deceased", "", 8)]
    public partial class EmailPreferenceEntry : RockBlock
    {
        #region Fields

        private Person _person = null;
        private Rock.Model.Communication _communication = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            int? communicationId = PageParameter( "CommunicationId" ).AsIntegerOrNull();
            
            var rockContext = new RockContext();
            if ( communicationId.HasValue )
            {
                _communication = new CommunicationService( rockContext ).Get( communicationId.Value );
                mergeFields.Add( "Communication", _communication );
            }

            LoadDropdowns( mergeFields );

            var key = PageParameter( "Person" );
            if ( !string.IsNullOrWhiteSpace( key ) )
            {
                var service = new PersonService( rockContext );
                _person = service.GetByUrlEncodedKey( key );
            }

            if ( _person == null && CurrentPerson != null )
            {
                _person = CurrentPerson;
            }

            nbUnsubscribeSuccessMessage.NotificationBoxType = NotificationBoxType.Success;
            nbUnsubscribeSuccessMessage.Text = GetAttributeValue( "UnsubscribeSuccessText" ).ResolveMergeFields( mergeFields );

            if (_person != null)
            {
                nbEmailPreferenceSuccessMessage.NotificationBoxType = NotificationBoxType.Success;
                nbEmailPreferenceSuccessMessage.Text = GetAttributeValue( "SuccessText" ).ResolveMergeFields( mergeFields );
            }
            else
            {
                nbEmailPreferenceSuccessMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbEmailPreferenceSuccessMessage.Text = "Unfortunately, we're unable to update your email preference, as we're not sure who you are.";
                nbEmailPreferenceSuccessMessage.Visible = true;
                btnSubmit.Visible = false;
            }

            string script = string.Format( @"
    $(""input[id^='{0}'"").click(function () {{
        if ($(this).val() == '3') {{
            $('#{1}').slideDown('fast');
        }} else {{
            $('#{1}').slideUp('fast');
        }}
    }});    
", rblEmailPreference.ClientID, divNotInvolved.ClientID );

            ScriptManager.RegisterStartupScript( rblEmailPreference, rblEmailPreference.GetType(), "toggle-preference" + this.BlockId.ToString(), script, true );
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
                if (_person != null)
                {
                    switch ( _person.EmailPreference )
                    {
                        case EmailPreference.EmailAllowed:
                            {
                                rblEmailPreference.SelectedValue = "0";
                                break;
                            }
                        case EmailPreference.NoMassEmails:
                            {
                                rblEmailPreference.SelectedValue = "1";
                                break;
                            }
                        case EmailPreference.DoNotEmail:
                            {
                                if ( _person.RecordStatusValueId != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id )
                                {
                                    rblEmailPreference.SelectedValue = "2";
                                }
                                else
                                {
                                    rblEmailPreference.SelectedValue = "3";
                                    if ( _person.RecordStatusReasonValueId.HasValue )
                                    {
                                        ddlInactiveReason.SelectedValue = _person.RecordStatusReasonValueId.HasValue.ToString();
                                    }
                                    tbInactiveNote.Text = _person.ReviewReasonNote;
                                }
                                break;
                            }
                    }
                }

                if ( _communication != null && _communication.ListGroupId.HasValue )
                {
                    rblEmailPreference.SelectedValue = "4";
                }
            }

            divNotInvolved.Attributes["Style"] = rblEmailPreference.SelectedValue == "3" ? "display:block" : "display:none";
        }
        
        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            if ( rblEmailPreference.SelectedValue == "4")
            {
                UnsubscribeFromList();
                return;
            }

            if (_person != null)
            {
                var changes = new List<string>();

                var rockContext = new RockContext();
                var service = new PersonService( rockContext );
                var person = service.Get(_person.Id);
                if ( person != null )
                {
                    EmailPreference emailPreference = EmailPreference.EmailAllowed;

                    switch ( rblEmailPreference.SelectedValue )
                    {
                        case "1":
                            {
                                emailPreference = EmailPreference.NoMassEmails;
                                break;
                            }
                        case "2":
                        case "3":
                            {
                                emailPreference = EmailPreference.DoNotEmail;
                                break;
                            }
                    }

                    History.EvaluateChange( changes, "Email Preference", person.EmailPreference, emailPreference );
                    person.EmailPreference = emailPreference;

                    if (rblEmailPreference.SelectedValue == "3")
                    {
                        var newRecordStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
                        if ( newRecordStatus != null )
                        {
                            History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), newRecordStatus.Value );
                            person.RecordStatusValueId = newRecordStatus.Id;
                        }

                        var newInactiveReason = DefinedValueCache.Read( ddlInactiveReason.SelectedValue.AsInteger() );
                        if ( newInactiveReason != null )
                        {
                            History.EvaluateChange( changes, "Record Status Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), newInactiveReason.Value );
                            person.RecordStatusReasonValueId = newInactiveReason.Id;
                        }

                        var newReviewReason = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED );
                        if ( newReviewReason != null )
                        {
                            History.EvaluateChange( changes, "Review Reason", DefinedValueCache.GetName( person.ReviewReasonValueId ), newReviewReason.Value );
                            person.ReviewReasonValueId = newReviewReason.Id;
                        }

                        // If the inactive reason note is the same as the current review reason note, update it also.
                        if ( ( person.InactiveReasonNote ?? string.Empty ) == ( person.ReviewReasonNote ?? string.Empty ) )
                        {
                            History.EvaluateChange( changes, "Inactive Reason Note", person.InactiveReasonNote, tbInactiveNote.Text );
                            person.InactiveReasonNote = tbInactiveNote.Text;
                        }

                        History.EvaluateChange( changes, "Review Reason Note", person.ReviewReasonNote, tbInactiveNote.Text );
                        person.ReviewReasonNote = tbInactiveNote.Text;
                    }
                    else
                    {
                        var newRecordStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
                        if ( newRecordStatus != null )
                        {
                            History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), newRecordStatus.Value );
                            person.RecordStatusValueId = newRecordStatus.Id;
                        }

                        History.EvaluateChange( changes, "Record Status Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), string.Empty );
                        person.RecordStatusReasonValueId = null;
                    }

                    HistoryService.AddChanges(
                        rockContext,
                        typeof( Person ), 
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        person.Id,
                        changes,
                        CurrentPersonAliasId );

                    rockContext.SaveChanges();

                    nbEmailPreferenceSuccessMessage.Visible = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Unsubscribes the person from the List that the communication was sent to
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void UnsubscribeFromList()
        {
            if ( _person != null && _communication != null && _communication.ListGroup != null )
            {
                var rockContext = new RockContext();

                // normally there would be at most 1 group member record for the person, but just in case, mark them all inactive
                var groupMemberRecordsForPerson = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == _communication.ListGroupId && a.PersonId == _person.Id );
                foreach( var groupMember in groupMemberRecordsForPerson)
                {
                    groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                    if ( groupMember.Note.IsNullOrWhiteSpace() )
                    {
                        groupMember.Note = "Unsubscribed";
                    }
                }

                var communicationRecipient = _communication.GetRecipientsQry( rockContext ).Where( a => a.PersonAlias.PersonId == _person.Id ).FirstOrDefault();
                if ( communicationRecipient != null )
                {
                    var interactionService = new InteractionService( rockContext );

                    InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                                        .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(),
                                            _communication.Id, _communication.Subject );
                    rockContext.SaveChanges();


                    var ipAddress = GetClientIpAddress();
                    var userAgent = Request.UserAgent ?? "";

                    UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( userAgent );
                    var clientOs = client.OS.ToString();
                    var clientBrowser = client.UserAgent.ToString();
                    var clientType = InteractionDeviceType.GetClientType( userAgent );

                    interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Unsubscribe", "", communicationRecipient.PersonAliasId, RockDateTime.Now, clientBrowser, clientOs, clientType, userAgent, ipAddress );

                    rockContext.SaveChanges();
                }

                nbUnsubscribeSuccessMessage.Visible = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the dropdowns.
        /// </summary>
        /// <param name="mergeObjects">The merge objects.</param>
        private void LoadDropdowns( Dictionary<string, object> mergeObjects )
        {
            if ( _communication != null && _communication.ListGroupId.HasValue )
            {
                rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "UnsubscribefromListText" ).ResolveMergeFields( mergeObjects ), "4" ) );
            }

            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "EmailsAllowedText" ).ResolveMergeFields( mergeObjects ), "0" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NoMassEmailsText" ).ResolveMergeFields( mergeObjects ), "1" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NoEmailsText" ).ResolveMergeFields( mergeObjects ), "2" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NotInvolvedText" ).ResolveMergeFields( mergeObjects ), "3" ) );

            // NOTE: OnLoad will set the default selection based the communication.ListGroup and/or the person's current email preference

            var excludeReasons = GetAttributeValue( "ReasonstoExclude" ).SplitDelimitedValues( false ).ToList();
            var ds = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).DefinedValues
                .Where( v => !excludeReasons.Contains(v.Value, StringComparer.OrdinalIgnoreCase))
                .Select( v => new
                {
                    Name = v.Value,
                    v.Id
                } );


            ddlInactiveReason.SelectedIndex = -1;
            ddlInactiveReason.DataSource = ds;
            ddlInactiveReason.DataTextField = "Name";
            ddlInactiveReason.DataValueField = "Id";
            ddlInactiveReason.DataBind();
        }
        
        #endregion

    }
}
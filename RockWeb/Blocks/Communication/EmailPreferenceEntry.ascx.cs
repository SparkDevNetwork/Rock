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
    /// User control for editing a system email
    /// </summary>
    [DisplayName( "Email Preference Entry" )]
    [Category( "Communication" )]
    [Description( "Allows user to set their email preference." )]

    [MemoField( "Emails Allowed Text", "Text to display for the 'Emails Allowed' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, and wish to receive all emails.", "", 0, null, 3, true )]
    [MemoField( "No Mass Emails Text", "Text to display for the 'No Mass Emails' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not wish to receive mass emails (personal emails are fine).", "", 1, null, 3, true )]
    [MemoField( "No Emails Text", "Text to display for the 'No Emails' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not want to receive emails of ANY kind.", "", 2, null, 3, true )]
    [MemoField( "Not Involved Text", "Text to display for the 'Not Involved' option.", false, " I am no longer involved with {{ 'Global' | Attribute:'OrganizationName' }}.", "", 3, null, 3, true )]
    [MemoField( "Success Text", "Text to display after user submits selection.", false, "<h4>Thank You</h4>We have saved your email preference.", "", 4, null, 3, true )]
    [TextField( "Reasons to Exclude", "A delimited list of the Inactive Reasons to exclude from Reason list", false, "No Activity,Deceased", "", 5)]
    public partial class EmailPreferenceEntry : RockBlock
    {
        #region Fields

        private Person _person = null;

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
            LoadDropdowns( mergeFields );

            var key = PageParameter( "Person" );
            if ( !string.IsNullOrWhiteSpace( key ) )
            {
                var service = new PersonService( new RockContext() );
                _person = service.GetByUrlEncodedKey( key );
            }

            if ( _person == null && CurrentPerson != null )
            {
                _person = CurrentPerson;
            }

            if (_person != null)
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Success;
                nbMessage.Text = GetAttributeValue( "SuccessText" ).ResolveMergeFields( mergeFields );
            }
            else
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = "Unfortunately, we're unable to update your email preference, as we're not sure who you are.";
                nbMessage.Visible = true;
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

                    nbMessage.Visible = true;
                    return;
                }
            }
        }

        #endregion

        #region Methods

        private void LoadDropdowns( Dictionary<string, object> mergeObjects )
        {
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "EmailsAllowedText" ).ResolveMergeFields( mergeObjects ), "0" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NoMassEmailsText" ).ResolveMergeFields( mergeObjects ), "1" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NoEmailsText" ).ResolveMergeFields( mergeObjects ), "2" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NotInvolvedText" ).ResolveMergeFields( mergeObjects ), "3" ) );
            rblEmailPreference.SelectedIndex = 0;

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
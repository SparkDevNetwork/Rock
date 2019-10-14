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

    [MemoField( "Unsubscribe from Lists Text", "Text to display for the 'Unsubscribe me from the following lists:' option.", false, "Only unsubscribe me from the following lists", "", 0, null, 3, true )]
    [MemoField( "Update Email Address Text", "Text to display for the 'Update Email Address' option.", false, "Update my email address.", "", 1, null, 3, true )]
    [MemoField( "Emails Allowed Text", "Text to display for the 'Emails Allowed' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, and wish to receive all emails.", "", 2, null, 3, true )]
    [MemoField( "No Mass Emails Text", "Text to display for the 'No Mass Emails' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not wish to receive mass emails (personal emails are fine).", "", 3, null, 3, true )]
    [MemoField( "No Emails Text", "Text to display for the 'No Emails' option.", false, "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not want to receive emails of ANY kind.", "", 4, null, 3, true )]
    [MemoField( "Not Involved Text", "Text to display for the 'Not Involved' option.", false, " I am no longer involved with {{ 'Global' | Attribute:'OrganizationName' }}.", "", 5, null, 3, true )]
    [MemoField( "Success Text", "Text to display after user submits selection.", false, "<h4>Thank You</h4>We have saved your email preference.", "", 6, null, 3, true )]
    [CodeEditorField( "Unsubscribe Success Text", "Text to display after user unsubscribes from communication lists.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, UNSUBSCRIBE_SUCCESS_TEXT_DEFAULT_VALUE, order: 7 )]
    [TextField( "Reasons to Exclude", "A delimited list of the Inactive Reasons to exclude from Reason list", false, "No Activity,Deceased", "", 8 )]
    [GroupCategoryField( "Communication List Categories", "Select the categories of the communication lists to display for unsubscribe, or select none to show all that the user is authorized to view.", true, Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, defaultValue: Rock.SystemGuid.Category.GROUPTYPE_COMMUNICATIONLIST_PUBLIC, required: false, order: 9 )]
    [CustomCheckboxListField( "Available Options", "Select the options that should be available to a user when they are updating their email preference.", "Unsubscribe,Update Email Address,Emails Allowed,No Mass Emails,No Emails,Not Involved", true, "Unsubscribe,Update Email Address,Emails Allowed,No Mass Emails,No Emails,Not Involved", Order = 10 )]
    public partial class EmailPreferenceEntry : RockBlock
    {
        #region Attribute Field Constants
        private const string UNSUBSCRIBE_SUCCESS_TEXT_DEFAULT_VALUE = @"<h4>Thank You</h4>
We have unsubscribed you from the following lists:
<ul>
{% for unsubscribedGroup in UnsubscribedGroups %}
  <li>{{ unsubscribedGroup | Attribute:'PublicName' | Default:unsubscribedGroup.Name }}</li>
{% endfor %}
</ul>";

        #endregion Attribute Field Constants

        #region Fields

        private const string UNSUBSCRIBE = "Unsubscribe";
        private const string UPDATE_EMAIL_ADDRESS = "Update Email Address";
        private const string EMAILS_ALLOWED = "Emails Allowed";
        private const string NO_MASS_EMAILS = "No Mass Emails";
        private const string NO_EMAILS = "No Emails";
        private const string NOT_INVOLVED = "Not Involved";

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

            var key = PageParameter( "Person" );
            if ( !string.IsNullOrWhiteSpace( key ) )
            {
                var service = new PersonService( rockContext );
                _person = service.GetByPersonActionIdentifier( key, "Unsubscribe" );
                if ( _person == null )
                {
                    _person = new PersonService( rockContext ).GetByUrlEncodedKey( key );
                }
            }

            if ( _person == null && CurrentPerson != null )
            {
                _person = CurrentPerson;
            }

            if ( _person != null )
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

            LoadDropdowns( mergeFields );
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
                if ( _person != null )
                {
                    if ( _communication != null && _communication.ListGroupId.HasValue && rbUnsubscribe.Visible )
                    {
                        rbUnsubscribe.Checked = true;
                        var selectedListItem = cblUnsubscribeFromLists.Items.FindByValue( _communication.ListGroupId.ToString() ) as ListItem;
                        if ( selectedListItem != null )
                        {
                            selectedListItem.Selected = true;
                        }
                    }
                    else
                    {
                        bool anyOptionChecked = false;
                        switch ( _person.EmailPreference )
                        {
                            case EmailPreference.EmailAllowed:
                                {
                                    if ( rbEmailPreferenceEmailAllowed.Visible )
                                    {
                                        rbEmailPreferenceEmailAllowed.Checked = true;
                                        anyOptionChecked = true;
                                    }
                                    break;
                                }
                            case EmailPreference.NoMassEmails:
                                {
                                    if ( rbEmailPreferenceNoMassEmails.Visible )
                                    {
                                        rbEmailPreferenceNoMassEmails.Checked = true;
                                        anyOptionChecked = true;
                                    }
                                    break;
                                }
                            case EmailPreference.DoNotEmail:
                                {
                                    if ( _person.RecordStatusValueId != DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id )
                                    {
                                        if ( rbEmailPreferenceDoNotEmail.Visible )
                                        {
                                            rbEmailPreferenceDoNotEmail.Checked = true;
                                            anyOptionChecked = true;
                                        }
                                    }
                                    else
                                    {
                                        if ( rbNotInvolved.Visible )
                                        {
                                            rbNotInvolved.Checked = true;
                                            anyOptionChecked = true;
                                        }

                                        if ( _person.RecordStatusReasonValueId.HasValue )
                                        {
                                            ddlInactiveReason.SelectedValue = _person.RecordStatusReasonValueId.HasValue.ToString();
                                        }
                                        tbInactiveNote.Text = _person.ReviewReasonNote;
                                    }
                                    break;
                                }
                        }

                        if ( !anyOptionChecked && rbUpdateEmailAddress.Visible )
                        {
                            rbUpdateEmailAddress.Checked = true;
                        }
                    }
                    tbEmail.Text = _person.Email;
                }
            }



            divNotInvolved.Attributes["Style"] = rbNotInvolved.Checked ? "display:block" : "display:none";
            divUpdateEmail.Attributes["Style"] = rbUpdateEmailAddress.Checked ? "display:block" : "display:none";
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
            nbUnsubscribeSuccessMessage.Visible = false;
            nbEmailPreferenceSuccessMessage.Visible = false;

            if ( rbUnsubscribe.Checked && rbUnsubscribe.Visible )
            {
                UnsubscribeFromLists();
                return;
            }

            if ( rbUpdateEmailAddress.Checked )
            {
                // Though the chance for empty email address is very minimal as client side validation is in place.
                if ( string.IsNullOrEmpty( tbEmail.Text ) )
                {
                    nbEmailPreferenceSuccessMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbEmailPreferenceSuccessMessage.Text = "Email is required.";
                }
                else
                {
                    UpdateEmail();
                }
                return;
            }

            if ( _person != null )
            {
                var rockContext = new RockContext();
                var service = new PersonService( rockContext );
                var person = service.Get( _person.Id );
                if ( person != null )
                {
                    EmailPreference emailPreference = EmailPreference.EmailAllowed;
                    if ( rbEmailPreferenceNoMassEmails.Checked )
                    {
                        emailPreference = EmailPreference.NoMassEmails;
                    }
                    if ( rbEmailPreferenceDoNotEmail.Checked || rbNotInvolved.Checked )
                    {
                        emailPreference = EmailPreference.DoNotEmail;
                    }

                    person.EmailPreference = emailPreference;

                    if ( rbNotInvolved.Checked )
                    {
                        var newRecordStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
                        if ( newRecordStatus != null )
                        {
                            person.RecordStatusValueId = newRecordStatus.Id;
                        }

                        var newInactiveReason = DefinedValueCache.Get( ddlInactiveReason.SelectedValue.AsInteger() );
                        if ( newInactiveReason != null )
                        {
                            person.RecordStatusReasonValueId = newInactiveReason.Id;
                        }

                        var newReviewReason = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED );
                        if ( newReviewReason != null )
                        {
                            person.ReviewReasonValueId = newReviewReason.Id;
                        }

                        // If the inactive reason note is the same as the current review reason note, update it also.
                        if ( ( person.InactiveReasonNote ?? string.Empty ) == ( person.ReviewReasonNote ?? string.Empty ) )
                        {
                            person.InactiveReasonNote = tbInactiveNote.Text;
                        }

                        person.ReviewReasonNote = tbInactiveNote.Text;
                    }
                    else
                    {
                        var newRecordStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
                        if ( newRecordStatus != null )
                        {
                            person.RecordStatusValueId = newRecordStatus.Id;
                        }

                        person.RecordStatusReasonValueId = null;
                    }

                    rockContext.SaveChanges();

                    nbEmailPreferenceSuccessMessage.Visible = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Updates the person email
        /// </summary>
        private void UpdateEmail()
        {
            if ( _person != null )
            {
                var rockContext = new RockContext();
                var service = new PersonService( rockContext );
                var person = service.Get( _person.Id );
                if ( person != null )
                {
                    person.Email = tbEmail.Text;

                    rockContext.SaveChanges();

                    nbEmailPreferenceSuccessMessage.Text = "<h4>Thank You</h4>We have updated your email address.";
                    nbEmailPreferenceSuccessMessage.Visible = true;

                }
            }
        }

        /// <summary>
        /// Unsubscribes the person from any lists that were selected
        /// </summary>
        private void UnsubscribeFromLists()
        {
            if ( _person != null )
            {
                if ( !cblUnsubscribeFromLists.SelectedValuesAsInt.Any() )
                {
                    nbUnsubscribeSuccessMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbUnsubscribeSuccessMessage.Text = "Please select the lists that you want to unsubscribe from.";
                    nbUnsubscribeSuccessMessage.Visible = true;
                    return;
                }

                List<Group> unsubscribedGroups = new List<Group>();
                var rockContext = new RockContext();

                foreach ( var communicationListId in cblUnsubscribeFromLists.SelectedValuesAsInt )
                {
                    // normally there would be at most 1 group member record for the person, but just in case, mark them all inactive
                    var groupMemberRecordsForPerson = new GroupMemberService( rockContext ).Queryable().Include( a => a.Group ).Where( a => a.GroupId == communicationListId && a.PersonId == _person.Id );
                    foreach ( var groupMember in groupMemberRecordsForPerson.ToList() )
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                        if ( groupMember.Note.IsNullOrWhiteSpace() )
                        {
                            groupMember.Note = "Unsubscribed";
                        }

                        unsubscribedGroups.Add( groupMember.Group );

                        rockContext.SaveChanges();
                    }

                    // if they selected the CommunicationList associated with the CommunicationId from the Url, log an 'Unsubscribe' Interaction 
                    if ( _communication != null && _communication.ListGroupId.HasValue && communicationListId == _communication.ListGroupId )
                    {
                        var communicationRecipient = _communication.GetRecipientsQry( rockContext ).Where( a => a.PersonAlias.PersonId == _person.Id ).FirstOrDefault();
                        if ( communicationRecipient != null )
                        {
                            var interactionService = new InteractionService( rockContext );

                            InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                                                .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(), _communication.Id, _communication.Subject );

                            rockContext.SaveChanges();

                            var ipAddress = GetClientIpAddress();
                            var userAgent = Request.UserAgent ?? "";

                            UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( userAgent );
                            var clientOs = client.OS.ToString();
                            var clientBrowser = client.UserAgent.ToString();
                            var clientType = InteractionDeviceType.GetClientType( userAgent );

                            interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Unsubscribe", "", communicationRecipient.PersonAliasId, RockDateTime.Now, clientBrowser, clientOs, clientType, userAgent, ipAddress, null );

                            rockContext.SaveChanges();
                        }
                    }
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                int? communicationId = PageParameter( "CommunicationId" ).AsIntegerOrNull();

                if ( _communication != null )
                {
                    mergeFields.Add( "Communication", _communication );
                }

                mergeFields.Add( "UnsubscribedGroups", unsubscribedGroups );

                nbUnsubscribeSuccessMessage.NotificationBoxType = NotificationBoxType.Success;
                nbUnsubscribeSuccessMessage.Text = GetAttributeValue( "UnsubscribeSuccessText" ).ResolveMergeFields( mergeFields );
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
            var availableOptions = GetAttributeValue( "AvailableOptions" ).SplitDelimitedValues( false );

            rbUnsubscribe.Visible = availableOptions.Contains( UNSUBSCRIBE );
            rbUnsubscribe.Text = GetAttributeValue( "UnsubscribefromListsText" ).ResolveMergeFields( mergeObjects );
            if ( rbUnsubscribe.Visible )
            {
                cblUnsubscribeFromLists.Items.Clear();
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var categoryService = new CategoryService( rockContext );

                int communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;

                var viewableCommunicationLists = new List<Group>();
                if ( _person != null )
                {
                    // Get a list of all the Active CommunicationLists that the person is an active member of
                    var communicationListQry = groupService.Queryable()
                        .Where( a => a.GroupTypeId == communicationListGroupTypeId && a.IsActive && a.Members.Any( m => m.PersonId == _person.Id && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                    var categoryGuids = this.GetAttributeValue( "CommunicationListCategories" ).SplitDelimitedValues().AsGuidList();

                    var communicationLists = communicationListQry.ToList();
                    
                    foreach ( var communicationList in communicationLists )
                    {
                        communicationList.LoadAttributes( rockContext );
                        if ( !categoryGuids.Any() )
                        {
                            // if no categories where specified, only show lists that the person has VIEW auth
                            if ( communicationList.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                            {
                                viewableCommunicationLists.Add( communicationList );
                            }
                        }
                        else
                        {
                            Guid? categoryGuid = communicationList.GetAttributeValue( "Category" ).AsGuidOrNull();
                            if ( categoryGuid.HasValue && categoryGuids.Contains( categoryGuid.Value ) )
                            {
                                viewableCommunicationLists.Add( communicationList );
                            }
                        }
                    }
                }
                viewableCommunicationLists = viewableCommunicationLists.OrderBy( a =>
                {
                    var name = a.GetAttributeValue( "PublicName" );
                    if ( name.IsNullOrWhiteSpace() )
                    {
                        name = a.Name;
                    }

                    return name;
                } ).ToList();

                foreach ( var communicationList in viewableCommunicationLists )
                {
                    var listItem = new ListItem();
                    listItem.Value = communicationList.Id.ToString();
                    listItem.Text = communicationList.GetAttributeValue( "PublicName" );
                    if ( listItem.Text.IsNullOrWhiteSpace() )
                    {
                        listItem.Text = communicationList.Name;
                    }

                    cblUnsubscribeFromLists.Items.Add( listItem );
                }

                // if there are no communication lists, hide the option
                rbUnsubscribe.Visible = viewableCommunicationLists.Any();
            }

            rbEmailPreferenceEmailAllowed.Visible = availableOptions.Contains( EMAILS_ALLOWED );
            rbEmailPreferenceEmailAllowed.Text = GetAttributeValue( "EmailsAllowedText" ).ResolveMergeFields( mergeObjects );

            rbEmailPreferenceNoMassEmails.Visible = availableOptions.Contains( NO_MASS_EMAILS );
            rbEmailPreferenceNoMassEmails.Text = GetAttributeValue( "NoMassEmailsText" ).ResolveMergeFields( mergeObjects );

            rbEmailPreferenceDoNotEmail.Visible = availableOptions.Contains( NO_EMAILS );
            rbEmailPreferenceDoNotEmail.Text = GetAttributeValue( "NoEmailsText" ).ResolveMergeFields( mergeObjects );

            rbNotInvolved.Visible = availableOptions.Contains( NOT_INVOLVED );
            rbNotInvolved.Text = GetAttributeValue( "NotInvolvedText" ).ResolveMergeFields( mergeObjects );

            rbUpdateEmailAddress.Visible = availableOptions.Contains( UPDATE_EMAIL_ADDRESS );
            rbUpdateEmailAddress.Text = GetAttributeValue( "UpdateEmailAddressText" ).ResolveMergeFields( mergeObjects );

            // NOTE: OnLoad will set the default selection based the communication.ListGroup and/or the person's current email preference

            var excludeReasons = GetAttributeValue( "ReasonstoExclude" ).SplitDelimitedValues( false ).ToList();
            var ds = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).DefinedValues
                .Where( v => !excludeReasons.Contains( v.Value, StringComparer.OrdinalIgnoreCase ) )
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
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

    #region Block Attributes

    [MemoField(
        "Unsubscribe from Lists Text",
        Description = "Text to display for the 'Unsubscribe me from the following lists:' option.",
        IsRequired = false,
        DefaultValue = "Only unsubscribe me from the following lists",
        NumberOfRows = 3,
        AllowHtml = true,
        Order = 0,
        Key =  AttributeKey.UnsubscribefromListsText )]
    [MemoField(
        "Update Email Address Text",
        Description = "Text to display for the 'Update Email Address' option.",
        IsRequired = false,
        DefaultValue = "Update my email address.",
        NumberOfRows = 3,
        AllowHtml = true,
        Order = 1,
        Key = AttributeKey.UpdateEmailAddressText )]
    [MemoField(
        "Emails Allowed Text",
        Description = "Text to display for the 'Emails Allowed' option.",
        IsRequired = false,
        DefaultValue = "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, and wish to receive all emails.",
        Order = 2,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.EmailsAllowedText )]
    [MemoField(
        "No Mass Emails Text",
        Description = "Text to display for the 'No Mass Emails' option.",
        IsRequired = false,
        DefaultValue = "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not wish to receive mass emails (personal emails are fine).",
        Order = 3,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.NoMassEmailsText )]
    [MemoField(
        "No Emails Text",
        Description = "Text to display for the 'No Emails' option.",
        IsRequired = false,
        DefaultValue = "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but do not want to receive emails of ANY kind.",
        Order = 4,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.NoEmailsText )]
    [MemoField(
        "Not Involved Text",
        Description = "Text to display for the 'Not Involved' option.",
        IsRequired = false,
        DefaultValue = " I am no longer involved with {{ 'Global' | Attribute:'OrganizationName' }}.",
        Order = 5,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.NotInvolvedText )]
    [WorkflowTypeField(
        "Unsubscribe from List Workflow",
        Description = "The workflow type to launch for person who wants to unsubscribe from one or more Communication Lists. The person will be passed in as the Entity and the communication list Ids will be passed as a comma delimited string to the workflow 'CommunicationListIds' attribute if it exists.",
        AllowMultiple = false,
        IsRequired = false,
        Key = AttributeKey.UnsubscribeWorkflow )]
    [MemoField(
        "Success Text",
        Description = "Text to display after user submits selection.",
        IsRequired = false,
        DefaultValue = "<h4>Thank You</h4>We have saved your email preference.",
        Order = 6,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.SuccessText )]
    [CodeEditorField(
        "Unsubscribe Success Text",
        Description = "Text to display after user unsubscribes from communication lists.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = UNSUBSCRIBE_SUCCESS_TEXT_DEFAULT_VALUE,
        Order = 7,
        Key = AttributeKey.UnsubscribeSuccessText )]
    [TextField(
        "Reasons to Exclude",
        Description = "A delimited list of the Inactive Reasons to exclude from Reason list",
        IsRequired = false,
        DefaultValue = "No Activity,Deceased",
        Order = 8,
        Key = AttributeKey.ReasonstoExclude )]
    [GroupCategoryField(
        "Communication List Categories",
        Description = "Select the categories of the communication lists to display for unsubscribe, or select none to show all that the user is authorized to view.",
        AllowMultiple = true,
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        DefaultValue = Rock.SystemGuid.Category.GROUPTYPE_COMMUNICATIONLIST_PUBLIC,
        IsRequired = false,
        Order = 9,
        Key = AttributeKey.CommunicationListCategories )]
    [CustomCheckboxListField(
        "Available Options",
        Description = "Select the options that should be available to a user when they are updating their email preference.",
        ListSource = "Unsubscribe,Update Email Address,Emails Allowed,No Mass Emails,No Emails,Not Involved",
        IsRequired = true,
        DefaultValue = "Unsubscribe,Update Email Address,Emails Allowed,No Mass Emails,No Emails,Not Involved",
        Key = AttributeKey.AvailableOptions,
        Order = 10 )]

    #endregion Block Attributes
    public partial class EmailPreferenceEntry : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string UnsubscribefromListsText = "UnsubscribefromListsText";
            public const string UpdateEmailAddressText = "UpdateEmailAddressText";
            public const string EmailsAllowedText = "EmailsAllowedText";
            public const string NoMassEmailsText = "NoMassEmailsText";
            public const string NoEmailsText = "NoEmailsText";
            public const string NotInvolvedText = "NotInvolvedText";
            public const string UnsubscribeWorkflow = "UnsubscribeWorkflow";
            public const string SuccessText = "SuccessText";
            public const string UnsubscribeSuccessText = "UnsubscribeSuccessText";
            public const string ReasonstoExclude = "ReasonstoExclude";
            public const string CommunicationListCategories = "CommunicationListCategories";
            public const string AvailableOptions = "AvailableOptions";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string Person = "Person";
        }

        #endregion PageParameterKey

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
            int? communicationId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();

            var rockContext = new RockContext();
            if ( communicationId.HasValue )
            {
                _communication = new CommunicationService( rockContext ).Get( communicationId.Value );
                mergeFields.Add( "Communication", _communication );
            }

            var key = PageParameter( PageParameterKey.Person );
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

            LoadDropdowns( mergeFields );

            if ( _person != null )
            {
                nbEmailPreferenceSuccessMessage.NotificationBoxType = NotificationBoxType.Success;
                nbEmailPreferenceSuccessMessage.Text = GetAttributeValue( AttributeKey.SuccessText ).ResolveMergeFields( mergeFields );
            }
            else
            {
                nbEmailPreferenceSuccessMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbEmailPreferenceSuccessMessage.Text = "Unfortunately, we're unable to update your email preference, as we're not sure who you are.";
                nbEmailPreferenceSuccessMessage.Visible = true;
                btnSubmit.Visible = false;
            }
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
            var rockContext = new RockContext();
            nbUnsubscribeSuccessMessage.Visible = false;
            nbEmailPreferenceSuccessMessage.Visible = false;

            if ( rbUnsubscribe.Checked && rbUnsubscribe.Visible )
            {
                bool unsubscribed = UnsubscribeFromLists();

                if ( unsubscribed )
                {
                    Guid? workflowGuid = GetAttributeValue( AttributeKey.UnsubscribeWorkflow ).AsGuidOrNull();
                    WorkflowTypeCache workflowType = null;
                    var workflowService = new WorkflowService( rockContext );

                    if ( workflowGuid != null )
                    {
                        workflowType = WorkflowTypeCache.Get( workflowGuid.Value );
                    }

                    // Start workflow for this person
                    if ( workflowType != null )
                    {
                        Dictionary<string, string> attributes = new Dictionary<string, string>();
                        if ( cblUnsubscribeFromLists.SelectedValuesAsInt.Any() )
                        {
                            attributes.Add( "CommunicationListIds", cblUnsubscribeFromLists.SelectedValues.ToList().AsDelimited( "," ) );
                            StartWorkflow( workflowService, workflowType, attributes, string.Format( "{0}", _person.FullName ), _person );
                        }
                    }
                }
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
        /// Unsubscribes the person from any lists that were selected.
        /// </summary>
        /// <returns>true if they were actually unsubscribed from something or false otherwise.</returns>
        private bool UnsubscribeFromLists()
        {
            if ( _person == null )
            {
                return false;
            }

            if ( !cblUnsubscribeFromLists.SelectedValuesAsInt.Any() )
            {
                nbUnsubscribeSuccessMessage.NotificationBoxType = NotificationBoxType.Warning;
                nbUnsubscribeSuccessMessage.Text = "Please select the lists that you want to unsubscribe from.";
                nbUnsubscribeSuccessMessage.Visible = true;
                return false;
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
                        var clientBrowser = client.UA.ToString();
                        var clientType = InteractionDeviceType.GetClientType( userAgent );

                        interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Unsubscribe", "", communicationRecipient.PersonAliasId, RockDateTime.Now, clientBrowser, clientOs, clientType, userAgent, ipAddress, null );

                        rockContext.SaveChanges();
                    }
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            int? communicationId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();

            if ( _communication != null )
            {
                mergeFields.Add( "Communication", _communication );
            }

            mergeFields.Add( "UnsubscribedGroups", unsubscribedGroups );

            nbUnsubscribeSuccessMessage.NotificationBoxType = NotificationBoxType.Success;
            nbUnsubscribeSuccessMessage.Text = GetAttributeValue( AttributeKey.UnsubscribeSuccessText ).ResolveMergeFields( mergeFields );
            nbUnsubscribeSuccessMessage.Visible = true;
            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the dropdowns.
        /// </summary>
        /// <param name="mergeObjects">The merge objects.</param>
        private void LoadDropdowns( Dictionary<string, object> mergeObjects )
        {
            var availableOptions = GetAttributeValue( AttributeKey.AvailableOptions ).SplitDelimitedValues( false );

            rbUnsubscribe.Visible = availableOptions.Contains( UNSUBSCRIBE );
            rbUnsubscribe.Text = GetAttributeValue( AttributeKey.UnsubscribefromListsText ).ResolveMergeFields( mergeObjects );
            if ( rbUnsubscribe.Visible )
            {
                cblUnsubscribeFromLists.Items.Clear();
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var categoryService = new CategoryService( rockContext );

                int communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;

                // Get a list of all the Active CommunicationLists that the person is an active member of
                int? personId = _person != null ? (int?)_person.Id:null;
                var communicationListQry = groupService.Queryable()
                    .Where( a => a.GroupTypeId == communicationListGroupTypeId && a.IsActive && a.Members.Any( m => m.PersonId == personId && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                var categoryGuids = this.GetAttributeValue( AttributeKey.CommunicationListCategories ).SplitDelimitedValues().AsGuidList();

                var communicationLists = communicationListQry.ToList();
                var viewableCommunicationLists = new List<Group>();
                foreach ( var communicationList in communicationLists )
                {
                    communicationList.LoadAttributes( rockContext );
                    if ( !categoryGuids.Any() )
                    {
                        // if no categories where specified, only show lists that the person has VIEW auth
                        if ( communicationList.IsAuthorized( Rock.Security.Authorization.VIEW, _person ) )
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
            rbEmailPreferenceEmailAllowed.Text = GetAttributeValue( AttributeKey.EmailsAllowedText ).ResolveMergeFields( mergeObjects );

            rbEmailPreferenceNoMassEmails.Visible = availableOptions.Contains( NO_MASS_EMAILS );
            rbEmailPreferenceNoMassEmails.Text = GetAttributeValue( AttributeKey.NoMassEmailsText ).ResolveMergeFields( mergeObjects );

            rbEmailPreferenceDoNotEmail.Visible = availableOptions.Contains( NO_EMAILS );
            rbEmailPreferenceDoNotEmail.Text = GetAttributeValue( AttributeKey.NoEmailsText ).ResolveMergeFields( mergeObjects );

            rbNotInvolved.Visible = availableOptions.Contains( NOT_INVOLVED );
            rbNotInvolved.Text = GetAttributeValue( AttributeKey.NotInvolvedText ).ResolveMergeFields( mergeObjects );

            rbUpdateEmailAddress.Visible = availableOptions.Contains( UPDATE_EMAIL_ADDRESS );
            rbUpdateEmailAddress.Text = GetAttributeValue( AttributeKey.UpdateEmailAddressText ).ResolveMergeFields( mergeObjects );

            // NOTE: OnLoad will set the default selection based the communication.ListGroup and/or the person's current email preference

            var excludeReasons = GetAttributeValue( AttributeKey.ReasonstoExclude ).SplitDelimitedValues( false ).ToList();
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

        protected void StartWorkflow( WorkflowService workflowService, WorkflowTypeCache workflowType, Dictionary<string, string> attributes, string workflowNameSuffix, Person p )
        {
            // launch workflow if configured
            if (workflowType != null && (workflowType.IsActive ?? true))
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, "UnsubscribeNotice " + workflowNameSuffix );

                // set attributes
                foreach (KeyValuePair<string, string> attribute in attributes)
                {
                    workflow.SetAttributeValue( attribute.Key, attribute.Value );
                }
                // launch workflow
                List<string> workflowErrors;
                workflowService.Process( workflow, p, out workflowErrors );
            }
        }

        #endregion

    }
}
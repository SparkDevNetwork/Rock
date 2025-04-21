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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Allows user to set their email preference or unsubscribe from a communication list.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Email Preference Entry" )]
    [Category( "Communication" )]
    [Description( "Allows user to set their email preference or unsubscribe from a communication list." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [MemoField(
        "Unsubscribe from Lists Text",
        Description = "Text to display for the 'Unsubscribe me from the following lists:' option.",
        IsRequired = false,
        DefaultValue = "Only unsubscribe me from the following lists.",
        NumberOfRows = 3,
        AllowHtml = true,
        Order = 0,
        Key = AttributeKey.UnsubscribefromListsText )]
    [MemoField(
        "Update Email Address Text",
        Description = "Text to display for the 'Update Email Address' option.",
        IsRequired = false,
        DefaultValue = "I'd like to update my email address instead of unsubscribing.",
        NumberOfRows = 3,
        AllowHtml = true,
        Order = 1,
        Key = AttributeKey.UpdateEmailAddressText )]
    [MemoField(
        "Emails Allowed Text",
        Description = "Text to display for the 'Emails Allowed' option.",
        IsRequired = false,
        DefaultValue = "My intention was not to unsubscribe; I would like to continue receiving all communications.",
        Order = 2,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.EmailsAllowedText )]
    [MemoField(
        "No Mass Emails Text",
        Description = "Text to display for the 'No Mass Emails' option.",
        IsRequired = false,
        DefaultValue = "I do not wish to receive any mass emails from {{ 'Global' | Attribute:'OrganizationName' }}.",
        Order = 3,
        NumberOfRows = 3,
        AllowHtml = true,
        Key = AttributeKey.NoMassEmailsText )]
    [MemoField(
        "No Emails Text",
        Description = "Text to display for the 'No Emails' option.",
        IsRequired = false,
        DefaultValue = "I am still involved with {{ 'Global' | Attribute:'OrganizationName' }}, but I prefer not to receive any email communications.",
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
    [BooleanField(
        "Allow Inactivating Family",
        Description = "If the person chooses the 'Not Involved' choice show the option of inactivating the whole family. This will not show if the person is a member of more than one family or is not an adult.",
        DefaultBooleanValue = true,
        Key = AttributeKey.AllowInactivatingFamily,
        Order = 11 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "28265232-B692-4099-9533-4D7646BDA2C1" )]
    [Rock.SystemGuid.BlockTypeGuid( "476FBA19-005C-4FF4-996B-CA1B165E5BC8" )]
    public class EmailPreferenceEntry : RockBlockType
    {
        #region Keys

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
            public const string AllowInactivatingFamily = "AllowInactivatingFamily";

            public const string Category = "Category";
            public const string PublicName = "PublicName";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string Person = "Person";
        }

        #endregion Keys

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

        private Model.Communication _communication = null;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = GetInitializationBox( rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();

                return box;
            }
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private EmailPreferenceEntryInitializationBox GetInitializationBox( RockContext rockContext )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            var box = new EmailPreferenceEntryInitializationBox();
            var communication = GetCommunication( rockContext );
            var ( person, isFromPageParameter ) = GetPerson( rockContext );

            if ( communication != null )
            {
                mergeFields.Add( "Communication", communication );
            }

            box.AllowInactivatingFamily = SetAllowInactivatingFamilyVisibility( person );

            if ( person == null )
            {
                box.EmailPreferenceUpdateAlertType = nameof( NotificationBoxType.Danger ).ToLower();
                box.EmailPreferenceUpdateMessage = "Unfortunately, we're unable to update your email preference, as we're not sure who you are.";
                return box;
            }

            /*
                5/31/2024 - JMH

                Some individuals were being unsubscribed from email automatically
                without clicking an unsubscribe button in their email.       

                Per the spec, https://datatracker.ietf.org/doc/html/rfc8058#section-3.2,
                an email client must send a POST request with the key-value pair, list-unsubscribe=one-click
                in order to perform a one-click unsubscription. It also states that an email client
                "...MUST NOT perform a POST on the HTTPS URI without user consent...".

                If an email client is following the spec, then it isn't likely that
                POST requests are being sent without user consent (clicking an unsubscribe button);
                however, an email client can still send HEAD, GET, OPTIONS or other requests to this URL.
                Since this block didn't differentiate between requests, it would always unsubscribe a person automatically
                regardless of the request type.

                To prevent unintentional unsubscriptions, only unsubscribe a person from email
                automatically if responding to a one-click unsubscribe request. 
            */
            var isPersonAutoUnsubscribed = false;
            if ( isFromPageParameter && IsOneClickUnsubscribeRequest() )
            {
                var service = new PersonService( rockContext );

                if ( communication?.ListGroupId.HasValue == true )
                {
                    // Auto-unsubscribe from a specific communication list.
                    var communicationListsQuery = new GroupService( rockContext )
                        .GetQueryableByKey( communication.ListGroupId.ToString() )
                        .IsCommunicationList();
                    service.UnsubscribeFromEmail( person, communicationListsQuery );
                }
                else
                {
                    // Auto-unsubscribe from all email.
                    service.UnsubscribeFromEmail( person );
                }

                rockContext.SaveChanges();

                isPersonAutoUnsubscribed = true;
            }

            SetEmailPreferenceData( rockContext, box, mergeFields, person, isPersonAutoUnsubscribed );

            return box;
        }

        /// <summary>
        /// Determines if the current request is a one-click unsubscribe request.
        /// </summary>
        /// <returns><see langword="true"/> if the current request is a one-click unsubscribe request; otherwise, <see langword="false"/> is returned.</returns>
        private bool IsOneClickUnsubscribeRequest()
        {
            // See https://datatracker.ietf.org/doc/html/rfc8058#section-3.2 for email client spec
            // and https://datatracker.ietf.org/doc/html/rfc8058#section-8 for example POST requests.
            return this.RequestContext.HttpMethod == "POST"
                && this.RequestContext.Form != null
                && this.RequestContext.Form.Get( "List-Unsubscribe" )?.Equals( "One-Click", StringComparison.OrdinalIgnoreCase ) == true;
        }

        /// <summary>
        /// Sets the email preference data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="box">The box.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="isPersonUnsubscribed">Whether the person is unsubscribed.</param>
        private void SetEmailPreferenceData( RockContext rockContext, EmailPreferenceEntryInitializationBox box, Dictionary<string, object> mergeFields, Person person, bool isPersonUnsubscribed )
        {
            var availableOptions = GetAttributeValue( AttributeKey.AvailableOptions ).SplitDelimitedValues( false );

            var isUnsubscribeVisible = availableOptions.Contains( UNSUBSCRIBE );
            box.UnsubscribeText = isUnsubscribeVisible ? GetAttributeValue( AttributeKey.UnsubscribefromListsText ).ResolveMergeFields( mergeFields ) : null;
            if ( isUnsubscribeVisible )
            {
                var groupService = new GroupService( rockContext );
                int communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;

                // Get a list of all the Active CommunicationLists that the person is an active member of
                int? personId = person != null ? ( int? ) person.Id : null;
                var communicationListQry = groupService.Queryable()
                    .Where( a => a.GroupTypeId == communicationListGroupTypeId && a.IsActive && a.Members.Any( m => m.PersonId == personId && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                var categoryGuids = this.GetAttributeValue( AttributeKey.CommunicationListCategories ).SplitDelimitedValues().AsGuidList();

                var communicationLists = communicationListQry.ToList();
                var viewableCommunicationLists = new List<Rock.Model.Group>();
                foreach ( var communicationList in communicationLists )
                {
                    communicationList.LoadAttributes( rockContext );
                    if ( !categoryGuids.Any() )
                    {
                        // if no categories where specified, only show lists that the person has VIEW auth
                        if ( communicationList.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                        {
                            viewableCommunicationLists.Add( communicationList );
                        }
                    }
                    else
                    {
                        var categoryGuid = communicationList.GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
                        if ( categoryGuid.HasValue && categoryGuids.Contains( categoryGuid.Value ) )
                        {
                            viewableCommunicationLists.Add( communicationList );
                        }
                    }
                }

                viewableCommunicationLists = viewableCommunicationLists.OrderBy( a =>
                {
                    var name = a.GetAttributeValue( AttributeKey.PublicName );
                    if ( name.IsNullOrWhiteSpace() )
                    {
                        name = a.Name;
                    }

                    return name;
                } ).ToList();

                // Call ToListItemBagList( Func<...> ) passing this function for setting the ListItemBag's Text value using
                // the "PublicName" attribute name if it exists.
                box.UnsubscribeFromListOptions = viewableCommunicationLists.ToListItemBagList( e =>
                    {
                        var a = ( ( Rock.Model.Group ) e );
                        var name = a.GetAttributeValue( AttributeKey.PublicName );
                        if ( name.IsNullOrWhiteSpace() )
                        {
                            name = a.Name;
                        }

                        return name;
                    }
                );
            }

            box.EmailsAllowedText = availableOptions.Contains( EMAILS_ALLOWED ) ? GetAttributeValue( AttributeKey.EmailsAllowedText ).ResolveMergeFields( mergeFields ) : null;
            box.NoMassEmailsText = availableOptions.Contains( NO_MASS_EMAILS ) ? GetAttributeValue( AttributeKey.NoMassEmailsText ).ResolveMergeFields( mergeFields ) : null;
            box.NoEmailsText = availableOptions.Contains( NO_EMAILS ) ? GetAttributeValue( AttributeKey.NoEmailsText ).ResolveMergeFields( mergeFields ) : null;
            box.NotInvolvedText = availableOptions.Contains( NOT_INVOLVED ) ? GetAttributeValue( AttributeKey.NotInvolvedText ).ResolveMergeFields( mergeFields ) : null;
            box.UpdateEmailAddressText = availableOptions.Contains( UPDATE_EMAIL_ADDRESS ) ? GetAttributeValue( AttributeKey.UpdateEmailAddressText ).ResolveMergeFields( mergeFields ) : null;

            var excludeReasons = GetAttributeValue( AttributeKey.ReasonstoExclude ).SplitDelimitedValues( false ).ToList();
            box.InActiveReasons = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).DefinedValues
                .Where( v => !excludeReasons.Contains( v.Value, StringComparer.OrdinalIgnoreCase ) ).ToListItemBagList();

            if ( person != null )
            {
                var communication = GetCommunication( rockContext );
                if ( communication != null && communication.ListGroupId.HasValue && isUnsubscribeVisible )
                {
                    box.EmailPreference = UNSUBSCRIBE;
                    box.UnsubscribeFromList = new List<ViewModels.Utility.ListItemBag>() { box.UnsubscribeFromListOptions.Find( l => l.Value == communication.ListGroup.Guid.ToString() ) };
                    box.SuccessfullyUnsubscribedText = isPersonUnsubscribed ? $"You have been successfully unsubscribed from the \"{GetName(communication.ListGroup)}\" communication list. If you would like to be removed from all communications see the options below." : null;
                }

                var anyOptionChecked = false;
                switch ( person.EmailPreference )
                {
                    case EmailPreference.EmailAllowed:
                    {
                        if ( box.EmailsAllowedText.IsNotNullOrWhiteSpace() )
                        {
                            box.EmailPreference = EMAILS_ALLOWED;
                            anyOptionChecked = true;
                        }
                        break;
                    }
                    case EmailPreference.NoMassEmails:
                    {
                        if ( box.NoMassEmailsText.IsNotNullOrWhiteSpace() )
                        {
                            box.EmailPreference = NO_MASS_EMAILS;
                            anyOptionChecked = true;
                        }

                        if ( isPersonUnsubscribed )
                        {
                            // Only update the unsubscribed message if it hasn't already been set.
                            box.SuccessfullyUnsubscribedText = box.SuccessfullyUnsubscribedText ?? $"You have been successfully unsubscribed from all mass communications sent by {GlobalAttributesCache.Value( "OrganizationName" )}.";
                        }
                        break;
                    }
                    case EmailPreference.DoNotEmail:
                    {
                        if ( person.RecordStatusValueId != DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id )
                        {
                            if ( box.NoEmailsText.IsNotNullOrWhiteSpace() )
                            {
                                box.EmailPreference = NO_EMAILS;
                                anyOptionChecked = true;
                            }
                        }
                        else
                        {
                            if ( box.NotInvolvedText.IsNotNullOrWhiteSpace() )
                            {
                                box.EmailPreference = NOT_INVOLVED;
                                anyOptionChecked = true;
                            }

                            if ( person.RecordStatusReasonValueId.HasValue )
                            {
                                box.InActiveReason = person.RecordStatusReasonValue.Guid.ToString();
                            }

                            box.InActiveNote = person.ReviewReasonNote;
                        }

                        if ( isPersonUnsubscribed )
                        {
                            // Only update the unsubscribed message if it hasn't already been set.
                            box.SuccessfullyUnsubscribedText = box.SuccessfullyUnsubscribedText ?? $"You have been successfully unsubscribed from all communications sent by {GlobalAttributesCache.Value( "OrganizationName" )}.";
                        }
                        break;
                    }
                }

                if ( !anyOptionChecked && box.UpdateEmailAddressText.IsNotNullOrWhiteSpace() )
                {
                    box.EmailPreference = UPDATE_EMAIL_ADDRESS;
                }

                box.Email = person.Email;
            }
        }

        /// <summary>
        /// Checks if the AllowInActivateFamily option should be displayed
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private bool SetAllowInactivatingFamilyVisibility( Person person )
        {
            // First check the block setting to see if it should be shown under any conditions
            if ( !GetAttributeValue( AttributeKey.AllowInactivatingFamily ).AsBooleanOrNull() ?? true )
            {
                return false;
            }

            // If there is no person there is nothing to show.
            if ( person == null )
            {
                return false;
            }

            // If the person is a member of more than one family then do not show
            if ( person.GetFamilies().Count() > 1 )
            {
                return false;
            }

            // Only an adult can do this
            if ( person.AgeClassification != AgeClassification.Adult )
            {
                return false;
            }

            // Show the control since the person and block condtions allow it
            return true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Starts any attached workflows
        /// </summary>
        /// <param name="workflowService"></param>
        /// <param name="workflowType"></param>
        /// <param name="attributes"></param>
        /// <param name="workflowNameSuffix"></param>
        /// <param name="p"></param>
        protected void StartWorkflow( WorkflowService workflowService, WorkflowTypeCache workflowType, Dictionary<string, string> attributes, string workflowNameSuffix, Person p )
        {
            // launch workflow if configured
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, "UnsubscribeNotice " + workflowNameSuffix );

                // set attributes
                foreach ( KeyValuePair<string, string> attribute in attributes )
                {
                    workflow.SetAttributeValue( attribute.Key, attribute.Value );
                }

                // launch workflow
                workflowService.Process( workflow, p, out List<string> workflowErrors );
            }
        }

        private EmailPreferenceEntrySaveResponseBag UnsubscribeFromLists( RockContext rockContext, EmailPreferenceEntrySaveRequestBag bag, Person person )
        {
            var responseBag = GetSuccessResponseBag();
            var communication = GetCommunication( rockContext );

            if ( person == null )
            {
                responseBag.AlertType = nameof( NotificationBoxType.Danger ).ToLower();
                responseBag.ErrorMessage = "Unfortunately, we're unable to update your email preference, as we're not sure who you are.";
                return responseBag;
            }

            if ( !bag.UnsubscribeFromList.Any() )
            {
                responseBag.AlertType = nameof( NotificationBoxType.Warning ).ToLower();
                responseBag.ErrorMessage = "Please select the lists that you want to unsubscribe from.";
                return responseBag;
            }
            
            // Make sure the person can receive emails.
            person.EmailPreference = EmailPreference.EmailAllowed;

            var unsubscribedGroups = new List<Rock.Model.Group>();

            foreach ( var communicationListGuid in bag.UnsubscribeFromList.AsGuidList() )
            {
                // normally there would be at most 1 group member record for the person, but just in case, mark them all inactive
                var groupMemberRecordsForPerson = new GroupMemberService( rockContext )
                    .Queryable()
                    .Include( a => a.Group )
                    .Where( a => a.Group.Guid == communicationListGuid && a.PersonId == person.Id )
                    .ToList();

                foreach ( var groupMember in groupMemberRecordsForPerson )
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
                if ( communication != null && communication.ListGroupId.HasValue && communicationListGuid == communication.ListGroup.Guid )
                {
                    var communicationRecipient = communication.GetRecipientsQry( rockContext ).Where( a => a.PersonAlias.PersonId == person.Id ).FirstOrDefault();
                    if ( communicationRecipient != null )
                    {
                        var interactionService = new InteractionService( rockContext );

                        InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                                            .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(), communication.Id, communication.Subject );

                        rockContext.SaveChanges();

                        var ipAddress = RequestContext.ClientInformation.IpAddress;
                        var userAgent = RequestContext.ClientInformation.UserAgent ?? "";

                        UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( userAgent );
                        var clientOs = client.OS.ToString();
                        var clientBrowser = client.UA.ToString();
                        var clientType = InteractionDeviceType.GetClientType( userAgent );

                        interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Unsubscribe", "", communicationRecipient.PersonAliasId, RockDateTime.Now, clientBrowser, clientOs, clientType, userAgent, ipAddress, null );

                        rockContext.SaveChanges();
                    }
                }
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            if ( communication != null )
            {
                mergeFields.Add( "Communication", communication );
            }

            mergeFields.Add( "UnsubscribedGroups", unsubscribedGroups );

            return GetSuccessResponseBag( GetAttributeValue( AttributeKey.UnsubscribeSuccessText ).ResolveMergeFields( mergeFields ) );
        }

        /// <summary>
        /// Creates a new instance of a <see cref="EmailPreferenceEntrySaveResponseBag"/> displaying the succes text
        /// </summary>
        /// <returns></returns>
        private EmailPreferenceEntrySaveResponseBag GetSuccessResponseBag(string successMessage = null)
        {
            return new EmailPreferenceEntrySaveResponseBag() { AlertType = nameof( NotificationBoxType.Success ).ToLower(), SuccessMessage = successMessage ?? GetAttributeValue( AttributeKey.SuccessText ) };
        }

        /// <summary>
        /// Updates the person email
        /// </summary>
        private EmailPreferenceEntrySaveResponseBag UpdateEmail( RockContext rockContext, string email, Person person )
        {
            var responseBag = new EmailPreferenceEntrySaveResponseBag()
            {
                AlertType = nameof( NotificationBoxType.Danger ).ToLower(),
                ErrorMessage = "Unfortunately, we're unable to update your email preference, as we're not sure who you are."
            };

            if ( person != null )
            {
                var service = new PersonService( rockContext );
                var trackedPerson = service.Get( person.Id );

                if ( trackedPerson != null )
                {
                    trackedPerson.Email = email;

                    // Make sure the person can receive emails.
                    trackedPerson.EmailPreference = EmailPreference.EmailAllowed;

                    rockContext.SaveChanges();

                    return GetSuccessResponseBag( "<h4>Thank You</h4>We have updated your email address." );
                }
            }

            return responseBag;
        }

        /// <summary>
        /// Gets the <see cref="Person"/> about to updated their email preference
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private (Person person, bool isFromPageParameter) GetPerson( RockContext rockContext )
        {
            // Return the person associated with the page parameter first,
            // as they likely got to this page from an unsubscribe link to unsubscribe.
            // If there is none, then use the authenticated person,
            // as they likely navigated to this page to modify their preferences.
            Person person = null;
            var key = PageParameter( PageParameterKey.Person );
            if ( !string.IsNullOrWhiteSpace( key ) )
            {
                var service = new PersonService( rockContext );
                person = service.GetByPersonActionIdentifier( key, "Unsubscribe" );
                if ( person == null )
                {
                    person = service.GetByUrlEncodedKey( key );
                }
            }

            if ( person != null )
            {
                return ( person, true );
            }
            else
            {
                return ( GetCurrentPerson(), false );
            }
        }

        /// <summary>
        /// Gets the communcation passed as a query param.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private Rock.Model.Communication GetCommunication( RockContext rockContext )
        {
            int? communicationId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();
            if ( communicationId.HasValue )
            {
                _communication = new CommunicationService( rockContext ).Get( communicationId.Value );
            }

            return _communication;
        }

        /// <summary>
        /// Gets the name of a communication list (group) using the Public Name if it exists.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private string GetName( Rock.Model.Group communicationList )
        {
            if ( communicationList == null )
            {
                return string.Empty;
            }

            communicationList.LoadAttributes();
            var name = communicationList.GetAttributeValue( AttributeKey.PublicName );
            if ( name.IsNullOrWhiteSpace() )
            {
                name = communicationList.Name;
            }

            return name;
        }
        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the email preference.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "Save" )]
        public BlockActionResult Save( EmailPreferenceEntrySaveRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var responseBag = GetSuccessResponseBag();
                var ( person, _ ) = GetPerson( rockContext );

                if ( bag.EmailPreference == UNSUBSCRIBE )
                {
                    responseBag = UnsubscribeFromLists( rockContext, bag, person );

                    if ( responseBag.ErrorMessage.IsNullOrWhiteSpace() )
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
                            if ( bag.UnsubscribeFromList.Any() )
                            {
                                var communicationListGuids = bag.UnsubscribeFromList.AsGuidList();
                                var communicationListIds = new CommunicationService( rockContext )
                                    .Queryable()
                                    .Where( c => communicationListGuids.Contains( c.Guid ) )
                                    .Select( c => c.Id )
                                    .ToList();
                                attributes.Add( "CommunicationListIds", communicationListIds.AsDelimited( "," ) );
                                StartWorkflow( workflowService, workflowType, attributes, string.Format( "{0}", person.FullName ), person );
                            }
                        }
                    }

                    return ActionOk( responseBag );
                }

                if ( bag.EmailPreference == UPDATE_EMAIL_ADDRESS )
                {
                    // Though the chance for empty email address is very minimal as client side validation is in place.
                    if ( string.IsNullOrEmpty( bag.Email ) )
                    {
                        responseBag.AlertType = nameof( NotificationBoxType.Danger ).ToLower();
                        responseBag.ErrorMessage = "Email is required.";
                    }
                    else
                    {
                        responseBag = UpdateEmail( rockContext, bag.Email, person );
                    }

                    return ActionOk( responseBag );
                }

                if ( person != null )
                {
                    var personService = new PersonService( rockContext );
                    var trackedPerson = personService.Get( person.Id );

                    if ( trackedPerson != null )
                    {
                        EmailPreference emailPreference = EmailPreference.EmailAllowed;

                        if ( bag.EmailPreference == NO_MASS_EMAILS )
                        {
                            emailPreference = EmailPreference.NoMassEmails;
                        }

                        if ( bag.EmailPreference == NO_EMAILS || bag.EmailPreference == NOT_INVOLVED )
                        {
                            emailPreference = EmailPreference.DoNotEmail;
                        }

                        trackedPerson.EmailPreference = emailPreference;

                        if ( bag.EmailPreference == NOT_INVOLVED )
                        {
                            var inactiveReasonDefinedValue = DefinedValueCache.Get( bag.InaActiveReason.AsGuid() );
                            var selfInactivatedDefinedValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED );

                            // If the inactive reason note is the same as the current review reason note, update it also.
                            string inactiveReasonNote = ( trackedPerson.InactiveReasonNote ?? string.Empty ) == ( trackedPerson.ReviewReasonNote ?? string.Empty )
                                ? bag.InActiveReasonNote
                                : trackedPerson.InactiveReasonNote;

                            // See if inactivating just one person or the whole family
                            if ( !bag.InActivateFamily )
                            {
                                personService.InactivatePerson( trackedPerson, inactiveReasonDefinedValue, inactiveReasonNote, selfInactivatedDefinedValue, bag.InActiveReasonNote );
                            }
                            else
                            {
                                // Update each person
                                var inactivatePersonList = personService.GetFamilyMembers( trackedPerson.Id, true ).Select( m => m.Person );
                                foreach ( var inactivatePerson in inactivatePersonList )
                                {
                                    personService.InactivatePerson( inactivatePerson, inactiveReasonDefinedValue, inactiveReasonNote, selfInactivatedDefinedValue, bag.InActiveReasonNote );
                                }
                            }
                        }
                        else
                        {
                            var newRecordStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
                            if ( newRecordStatus != null )
                            {
                                trackedPerson.RecordStatusValueId = newRecordStatus.Id;
                            }

                            trackedPerson.RecordStatusReasonValueId = null;
                        }

                        rockContext.SaveChanges();
                    }
                }

                return ActionOk( responseBag );
            }
        }

        #endregion
    }
}
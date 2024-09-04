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

using Rock.Attribute;
using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Allows a person to register for a group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Registration" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows a person to register for a group." )]
    [IconCssClass( "fa fa-user-plus" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [EnumField(
        "Group Member Status",
        Description = "The group member status to use when adding person to the group.",
        EnumSourceType = typeof( GroupMemberStatus ),
        IsRequired = true,
        DefaultEnumValue = ( int ) GroupMemberStatus.Pending,
        Key = AttributeKey.GroupMemberStatus,
        Order = 0 )]

    [GroupTypesField( "Allowed Group Types",
        Description = "Determines which group types are allowed when adding people to a group from this block.",
        IsRequired = false,
        EnhancedSelection = true,
        Key = AttributeKey.GroupTypes,
        Order = 1 )]

    [WorkflowTypeField( "Registration Workflow",
        Description = "An optional workflow to start for each individual being added to the group. The GroupMember will be set as the workflow Entity. The current/primary person will be passed as the workflow initiator.",
        IsRequired = false,
        Key = AttributeKey.RegistrationWorkflow,
        Order = 2 )]

    [EnumField(
        "Family Options",
        Description = "Provides additional inputs to register additional members of the family.",
        IsRequired = true,
        EnumSourceType = typeof( FamilyOption ),
        DefaultEnumValue = ( int ) FamilyOption.None,
        Key = AttributeKey.FamilyOptions,
        Order = 3 )]

    [EnumField(
        "Mobile Phone",
        Description = "Determines if the Mobile Phone field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Key = AttributeKey.MobilePhoneVisibility,
        Order = 4 )]

    [EnumField(
        "Email Address",
        Description = "Determines if the Email field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Key = AttributeKey.EmailAddressVisibility,
        Order = 5 )]

    [GroupField(
        "Group",
        Description = "An optional group to add person to. If omitted, the group's Guid should be passed via the Query String (GroupGuid=).",
        IsRequired = false,
        Key = AttributeKey.Group,
        Order = 6 )]

    [DefinedValueField(
        "Connection Status",
        Description = "The connection status to use for new individuals.",
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Key = AttributeKey.ConnectionStatus,
        Order = 7 )]

    [DefinedValueField(
        "Record Status",
        Description = "The record status to use for new individuals.",
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Key = AttributeKey.RecordStatus,
        Order = 8 )]

    [LinkedPage(
        "Result Page",
        Description = "An optional page to redirect user to after they have been registered for the group. GroupGuid will be passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.ResultPage,
        Order = 9 )]

    [CodeEditorField(
        "Registration Completion Message",
        Description = "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page.",
        IsRequired = false,
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.RegistrationCompletionMessage,
        Order = 10 )]

    [BooleanField(
        "Prevent Overcapacity Registrations",
        Description = "When set to true, user cannot register for groups that are at capacity or whose default GroupTypeRole are at capacity. If only one spot is available, no family members can be registered.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = false,
        Key = AttributeKey.PreventOvercapacityRegistrations,
        Order = 11 )]

    [BooleanField(
        "Autofill Form",
        Description = "If set to false then the form will not load the context of the logged in user.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = true,
        Key = AttributeKey.AutofillForm,
        Order = 12 )]

    [TextField( "Register Button Text",
        Description = "The text to display in the save button.",
        IsRequired = true,
        DefaultValue = "Register",
        Key = AttributeKey.RegisterButtonText,
        Order = 13 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_REGISTRATION_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_REGISTRATION )]
    public class GroupRegistration : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="GroupRegistration"/> block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The group member status attribute key.
            /// </summary>
            public const string GroupMemberStatus = "GroupMemberStatus";

            /// <summary>
            /// The group types attribute key.
            /// </summary>
            public const string GroupTypes = "GroupTypes";

            /// <summary>
            /// The registration workflow attribute key.
            /// </summary>
            public const string RegistrationWorkflow = "RegistrationWorkflow";

            /// <summary>
            /// The family options attribute key.
            /// </summary>
            public const string FamilyOptions = "FamilyOptions";

            /// <summary>
            /// The mobile phone attribute visibility key.
            /// </summary>
            public const string MobilePhoneVisibility = "MobilePhoneVisibility";

            /// <summary>
            /// The email address visibility attribute key.
            /// </summary>
            public const string EmailAddressVisibility = "EmailAddressVisibility";

            /// <summary>
            /// The group attribute key.
            /// </summary>
            public const string Group = "Group";

            /// <summary>
            /// The default connection status attribute key.
            /// </summary>
            public const string ConnectionStatus = "DefaultConnectionStatus";

            /// <summary>
            /// The default record status attribute key.
            /// </summary>
            public const string RecordStatus = "DefaultRecordStatus";

            /// <summary>
            /// The result page attribute key.
            /// </summary>
            public const string ResultPage = "ResultPage";

            /// <summary>
            /// The completed template attribute key.
            /// </summary>
            public const string RegistrationCompletionMessage = "CompletedTemplate";

            /// <summary>
            /// The prevent overcapacity registrations attribute key.
            /// </summary>
            public const string PreventOvercapacityRegistrations = "PreventOvercapacityRegistrations";

            /// <summary>
            /// The autofill form attribute key.
            /// </summary>
            public const string AutofillForm = "AutofillForm";

            /// <summary>
            /// The save button text attribute key.
            /// </summary>
            public const string RegisterButtonText = "SaveButtonText";
        }

        /// <summary>
        /// Gets the <see cref="GroupMemberStatus"/> to use when adding a new group member.
        /// </summary>
        /// <value>
        /// The <see cref="GroupMemberStatus"/> to use when adding a new group member.
        /// </value>
        public GroupMemberStatus GroupMemberStatus => GetAttributeValue( AttributeKey.GroupMemberStatus ).ConvertToEnum<GroupMemberStatus>();

        /// <summary>
        /// Gets the group type unique identifiers that are valid for use by the query string.
        /// </summary>
        /// <value>
        /// The group type unique identifiers that are valid for use by the query string.
        /// </value>
        public List<Guid> GroupTypeGuids => GetAttributeValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the workflow type unique identifier.
        /// </summary>
        /// <value>
        /// The workflow type unique identifier.
        /// </value>
        public Guid? RegistrationWorkflowGuid => GetAttributeValue( AttributeKey.RegistrationWorkflow ).AsGuidOrNull();

        /// <summary>
        /// Gets the value used to determine how family members should be shown.
        /// </summary>
        /// <value>
        /// The value used to determine how family members should be shown.
        /// </value>
        public FamilyOption FamilyOptions => GetAttributeValue( AttributeKey.FamilyOptions ).ConvertToEnum<FamilyOption>( FamilyOption.None );

        /// <summary>
        /// Gets the mobile phone visibility.
        /// </summary>
        /// <value>
        /// The mobile phone visibility.
        /// </value>
        public VisibilityTriState MobilePhoneVisibility => GetAttributeValue( AttributeKey.MobilePhoneVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets the email address visibility.
        /// </summary>
        /// <value>
        /// The email address visibility.
        /// </value>
        public VisibilityTriState EmailAddressVisibility => GetAttributeValue( AttributeKey.EmailAddressVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets the group unique identifier to add people to.
        /// </summary>
        /// <value>
        /// The group unique identifier to add people to.
        /// </value>
        public Guid? GroupGuid => GetAttributeValue( AttributeKey.Group ).AsGuidOrNull();

        /// <summary>
        /// Gets the connection status when creating new records.
        /// </summary>
        /// <value>
        /// The connection status when creating new records.
        /// </value>
        public DefinedValueCache ConnectionStatus => DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );

        /// <summary>
        /// Gets the record status when creating new records.
        /// </summary>
        /// <value>
        /// The record status when creating new records.
        /// </value>
        public DefinedValueCache RecordStatus => DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

        /// <summary>
        /// Gets the result page unique identifier.
        /// </summary>
        /// <value>
        /// The result page unique identifier.
        /// </value>
        public Guid? ResultPageGuid => GetAttributeValue( AttributeKey.ResultPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the lava template to show when the user has been registered.
        /// </summary>
        /// <value>
        /// The lava template to show when the user has been registered.
        /// </value>
        public string RegistrationCompletionMessage => GetAttributeValue( AttributeKey.RegistrationCompletionMessage );

        /// <summary>
        /// Gets a value indicating whether registrations that would cause the
        /// group to go over capacity should be denied.
        /// </summary>
        /// <value>
        ///   <c>true</c> if registrations that would cause the group to go
        ///   over capacity should be denied; otherwise, <c>false</c>.
        /// </value>
        public bool PreventOvercapacityRegistrations => GetAttributeValue( AttributeKey.PreventOvercapacityRegistrations ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to autofill the form.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the form should be auto-filled with the current person's information; otherwise, <c>false</c>.
        /// </value>
        public bool AutofillForm => GetAttributeValue( AttributeKey.AutofillForm ).AsBoolean();

        /// <summary>
        /// Gets the register button text.
        /// </summary>
        /// <value>
        /// The register button text.
        /// </value>
        public string RegisterButtonText => GetAttributeValue( AttributeKey.RegisterButtonText );

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// The keys used in page parameters.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The group that users will be added to.
            /// </summary>
            public const string GroupGuid = "GroupGuid";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 3 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                EmailAddressVisibility,
                MobilePhoneVisibility,
                RegisterButtonText,
                FamilyOptions
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if we need to enforce group capacity rules.
        /// </summary>
        /// <returns><c>true</c> if group capacity should be enforced; otherwise <c>false</c>.</returns>
        private bool ShouldEnforceGroupCapacity()
        {
            return PreventOvercapacityRegistrations && GroupMemberStatus == GroupMemberStatus.Active;
        }

        /// <summary>
        /// Adds the primary person to the group. This is the person that entered
        /// all the information on the screen.
        /// </summary>
        /// <param name="personDetails">The person to be added.</param>
        /// <param name="group">The group to add the person to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="GroupMember"/> instance that identifies the person in the group.</returns>
        private GroupMember AddPrimaryPersonToGroup( PersonDetail personDetails, Group group, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            Person person = null;

            // Need to re-load the person since we are going to modify and we
            // need control of the context the person is in.
            if ( AutofillForm && RequestContext.CurrentPerson != null )
            {
                person = personService.Get( RequestContext.CurrentPerson.Id );
            }

            if ( person == null )
            {
                // If not logged in, try to match to an existing person.
                var matchQuery = new PersonService.PersonMatchQuery( personDetails.FirstName, personDetails.LastName, personDetails.Email, personDetails.MobilePhone );
                person = personService.FindPerson( matchQuery, true );
            }

            // If we have an existing person, update their personal information.
            if ( person != null )
            {
                person.FirstName = personDetails.FirstName;
                person.LastName = personDetails.LastName;

                if ( personDetails.Email.IsNotNullOrWhiteSpace() )
                {
                    person.Email = personDetails.Email;
                }

                if ( PhoneNumber.CleanNumber( personDetails.MobilePhone ).IsNotNullOrWhiteSpace() )
                {
                    int phoneNumberTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                    person.UpdatePhoneNumber( phoneNumberTypeId, "+1", personDetails.MobilePhone, personDetails.IsMessagingEnabled, null, rockContext );
                }
            }
            else
            {
                person = CreateNewPerson( personDetails.FirstName, personDetails.LastName, personDetails.Email, personDetails.MobilePhone, personDetails.IsMessagingEnabled );
                PersonService.SaveNewPerson( person, rockContext, null, false );
            }

            return AddPersonToGroup( person, group, rockContext );
        }

        /// <summary>
        /// Adds the spouse to the group.
        /// </summary>
        /// <param name="primaryPerson">The primary person.</param>
        /// <param name="personDetails">The person to be added.</param>
        /// <param name="group">The group to add the person to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="GroupMember" /> instance that identifies the person in the group.
        /// </returns>
        private GroupMember AddSpouseToGroup( Person primaryPerson, PersonDetail personDetails, Group group, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            var primaryFamilyId = primaryPerson.GetFamily( rockContext ).Id;

            // Attempt to find the existing spouse.
            var spouse = primaryPerson?.GetSpouse(rockContext );

            // If no spouse found, try to match to existing person.
            if ( spouse == null )
            {
                var matchQuery = new PersonService.PersonMatchQuery( personDetails.FirstName, personDetails.LastName, personDetails.Email, personDetails.MobilePhone );
                spouse = personService.FindPerson( matchQuery, true );

                // If the matched person isn't in the same family then don't
                // use this record.
                if ( spouse != null && spouse.GetFamily( rockContext ).Id != primaryFamilyId )
                {
                    spouse = null;
                }
            }

            // If we have an existing spouse, update their personal information.
            if ( spouse != null )
            {
                spouse.FirstName = personDetails.FirstName;
                spouse.LastName = personDetails.LastName;

                if ( personDetails.Email.IsNotNullOrWhiteSpace() )
                {
                    spouse.Email = personDetails.Email;
                }

                if ( PhoneNumber.CleanNumber( personDetails.MobilePhone ).IsNotNullOrWhiteSpace() )
                {
                    int phoneNumberTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                    spouse.UpdatePhoneNumber( phoneNumberTypeId, "+1", personDetails.MobilePhone, personDetails.IsMessagingEnabled, null, rockContext );
                }
            }
            else
            {
                spouse = CreateNewPerson( personDetails.FirstName, personDetails.LastName, personDetails.Email, personDetails.MobilePhone, personDetails.IsMessagingEnabled );

                var groupRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                    .First( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    .Id;

                PersonService.AddPersonToFamily( spouse, true, primaryFamilyId, groupRoleId, rockContext );
            }

            // Update the marital status if they don't have one set.
            var marriedId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id;
            primaryPerson.MaritalStatusValueId = primaryPerson.MaritalStatusValueId ?? marriedId;
            spouse.MaritalStatusValueId = spouse.MaritalStatusValueId ?? marriedId;

            return AddPersonToGroup( spouse, group, rockContext );
        }

        /// <summary>
        /// Adds the family members to the group.
        /// </summary>
        /// <param name="personAliasGuids">The person alias unique identifiers to be added.</param>
        /// <param name="group">The group to add the people to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A list of <see cref="GroupMember"/> objects that were added to the group.</returns>
        private List<GroupMember> AddFamilyMembersToGroup( List<Guid> personAliasGuids, Group group, RockContext rockContext )
        {
            // Abort early if we don't have a person or don't allow family members.
            if ( RequestContext.CurrentPerson == null || FamilyOptions != FamilyOption.FamilyMemberSelection )
            {
                return new List<GroupMember>();
            }

            // If there are no person aliases to add then also abort.
            if ( personAliasGuids == null || personAliasGuids.Count == 0 )
            {
                return new List<GroupMember>();
            }

            // This is all a bit round-about, but it's to make sure
            // they can't just send us arbitrary person alias guids and
            // have us add them to the group.
            var validFamilyMembers = RequestContext.CurrentPerson
                .GetFamilyMembers( false, rockContext )
                .Select( a => a.Person )
                .ToList();

            // Get all the person identifiers the client wants to add.
            var parameterPersonIds = new PersonAliasService( rockContext ).Queryable()
                .Where( a => personAliasGuids.Contains( a.Guid ) )
                .Select( a => a.PersonId )
                .Distinct()
                .ToList();

            // Convert the person Ids back into the full Person objects.
            var familyMembersToAdd = validFamilyMembers
                .Where( a => parameterPersonIds.Contains( a.Id ) )
                .ToList();

            var members = new List<GroupMember>();

            // Loop through each person and add them to the group.
            foreach ( var person in familyMembersToAdd )
            {
                members.Add( AddPersonToGroup( person, group, rockContext ) );
            }

            return members;
        }

        /// <summary>
        /// Adds the person to the group.
        /// </summary>
        /// <param name="person">The person to be added.</param>
        /// <param name="group">The group the person will be added to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="GroupMember"/> record that identifies the person in the group.</returns>
        private GroupMember AddPersonToGroup( Person person, Group group, RockContext rockContext )
        {
            // Find the existing record, otherwise create a new one.
            var member = group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();

            if ( member == null )
            {
                member = new GroupMember { Id = 0 };
                new GroupMemberService( rockContext ).Add( member );

                member.GroupId = group.Id;
                // This is needed to trigger workflows later.
                member.Person = person;
                member.PersonId = person.Id;
                member.DateTimeAdded = RockDateTime.Now;
                member.GroupMemberStatus = GroupMemberStatus;

                member.GroupRoleId = group.GroupType.DefaultGroupRoleId ?? 0;
            }
            else if ( member.GroupMemberStatus == GroupMemberStatus.Inactive )
            {
                member.GroupMemberStatus = GroupMemberStatus;
            }

            return member;
        }

        /// <summary>
        /// Triggers the workflows.
        /// </summary>
        /// <param name="primaryMember">The primary member record that entered all their information.</param>
        /// <param name="secondaryMembers">The secondary members that were also added to the group.</param>
        private void TriggerWorkflows( GroupMember primaryMember, List<GroupMember> secondaryMembers )
        {
            if ( !RegistrationWorkflowGuid.HasValue )
            {
                return;
            }

            var workflowType = WorkflowTypeCache.Get( RegistrationWorkflowGuid.Value );

            if ( workflowType == null )
            {
                return;
            }

            var initatorAliasId = primaryMember.Person.PrimaryAliasId;

            // Launch the workflow for the main person who entered all
            // their information.
            var transaction = new Rock.Transactions.LaunchWorkflowTransaction<GroupMember>( workflowType.Guid, primaryMember.Id )
            {
                InitiatorPersonAliasId = initatorAliasId
            };
            transaction.Enqueue();

            // Launch the workflow for each family member that was also
            // added to the group.
            foreach ( var member in secondaryMembers )
            {
                transaction = new Rock.Transactions.LaunchWorkflowTransaction<GroupMember>( workflowType.Guid, member.Id )
                {
                    InitiatorPersonAliasId = initatorAliasId
                };
                transaction.Enqueue();
            }
        }

        /// <summary>
        /// Creates a new person.
        /// </summary>
        /// <returns>The <see cref="Person"/> object that was created.</returns>
        private Person CreateNewPerson( string firstName, string lastName, string email, string mobilePhone, bool enableMessaging )
        {
            Person person = new Person
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsEmailActive = true,
                EmailPreference = Rock.Model.EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                ConnectionStatusValueId = ConnectionStatus?.Id,
                RecordStatusValueId = RecordStatus?.Id
            };

            if ( mobilePhone.IsNotNullOrWhiteSpace() )
            {
                int phoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                var phoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = phoneNumberTypeId,
                    Number = PhoneNumber.CleanNumber( mobilePhone ),
                    IsMessagingEnabled = enableMessaging
                };

                person.PhoneNumbers.Add( phoneNumber );
            }

            return person;
        }

        /// <summary>
        /// Gets the number of available spots in the group.
        /// </summary>
        /// <param name="group">The group whose capacity needs to be checked.</param>
        /// <param name="groupRoleId">The group role identifier to check, if <c>null</c> then the default group role is checked.</param>
        /// <returns>The number of available spots remaining, or <c>null</c> if capacity is not enforced.</returns>
        private int? GetAvailableSpotsInGroup( Group group, int? groupRoleId )
        {
            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            if ( groupType == null || groupType.GroupCapacityRule == GroupCapacityRule.None )
            {
                return null;
            }

            int capacity = int.MaxValue;

            // Check the overall group capacity.
            if ( group.GroupCapacity.HasValue )
            {
                var activeCount = group.Members
                    .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Count();

                capacity = Math.Max( 0, Math.Min( capacity, group.GroupCapacity.Value - activeCount ) );
            }

            var role = groupType.Roles.FirstOrDefault( r => r.Id == ( groupRoleId ?? groupType.DefaultGroupRoleId ?? 0 ) );
            if ( role != null && role.MaxCount.HasValue )
            {
                var activeCount = group.Members
                    .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active
                        && m.GroupRoleId == role.Id )
                    .Count();

                capacity = Math.Max( 0, Math.Min( capacity, group.GroupCapacity.Value - activeCount ) );
            }

            return capacity < int.MaxValue ? capacity : ( int? ) null;
        }

        /// <summary>
        /// Gets the person details.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>The person details data.</returns>
        private PersonDetail GetPersonDetails( Person person )
        {
            var personDetails = new PersonDetail
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
            };

            var phone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            if ( phone != null )
            {
                personDetails.MobilePhone = phone.NumberFormatted;
                personDetails.IsMessagingEnabled = phone.IsMessagingEnabled;
            }

            return personDetails;
        }

        /// <summary>
        /// Determines whether the registration data is valid.
        /// </summary>
        /// <param name="person">The person details.</param>
        /// <param name="spouse">The spouse details.</param>
        /// <returns>
        ///   <c>true</c> if the registration data is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRegistrationValid( PersonDetail person, PersonDetail spouse )
        {
            var useSpouse = FamilyOptions == FamilyOption.SpouseOptional || FamilyOptions == FamilyOption.SpouseRequired;

            // Basic check, do we have the primary individual?
            if ( person == null || person.FirstName.IsNullOrWhiteSpace() || person.LastName.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Check if email is required but not provided.
            if ( EmailAddressVisibility == VisibilityTriState.Required && person.Email.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Check if phone is required but not provided.
            if ( MobilePhoneVisibility == VisibilityTriState.Required && person.MobilePhone.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( useSpouse )
            {
                var hasSpouse = spouse != null && spouse.FirstName.IsNotNullOrWhiteSpace() && spouse.LastName.IsNotNullOrWhiteSpace();

                // Basic check, is a spouse required but we don't have one?
                if ( FamilyOptions == FamilyOption.SpouseRequired && !hasSpouse )
                {
                    return false;
                }

                // We either have a required spouse, or we don't have the optional spouse.
                if ( hasSpouse )
                {
                    // Check if email is required but not provided.
                    if ( EmailAddressVisibility == VisibilityTriState.Required && spouse.Email.IsNullOrWhiteSpace() )
                    {
                        return false;
                    }

                    // Check if phone is required but not provided.
                    if ( MobilePhoneVisibility == VisibilityTriState.Required && spouse.MobilePhone.IsNullOrWhiteSpace() )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the registration options for the specified group.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier, this will be ignored if the block settings specify a group.</param>
        /// <returns>The registration options result.</returns>
        [BlockAction]
        public BlockActionResult GetRegistrationOptions( Guid? groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( GroupGuid ?? groupGuid ?? Guid.Empty );

                // Verify the group exists.
                if ( group == null )
                {
                    return ActionNotFound();
                }

                var result = new GetRegistrationOptionsResult();

                if ( ShouldEnforceGroupCapacity() )
                {
                    result.AvailableSpots = GetAvailableSpotsInGroup( group, null );
                }

                var currentPerson = AutofillForm ? RequestContext.CurrentPerson : null;

                if ( currentPerson != null )
                {
                    result.Person = GetPersonDetails( currentPerson );

                    if ( FamilyOptions == FamilyOption.SpouseOptional || FamilyOptions == FamilyOption.SpouseRequired )
                    {
                        var spouse = currentPerson.GetSpouse( rockContext );

                        if ( spouse != null )
                        {
                            result.Spouse = GetPersonDetails( spouse );
                        }
                    }
                    else if ( FamilyOptions == FamilyOption.FamilyMemberSelection )
                    {
                        result.FamilyMembers = currentPerson
                            .GetFamilyMembers( false, rockContext )
                            .AsNoTracking()
                            .Include( a => a.Person.Aliases )
                            .ToList()
                            .Select( a => new FamilyMember
                            {
                                Guid = a.Person.PrimaryAlias.Guid,
                                FullName = a.Person.FullName
                            } )
                            .ToList();
                    }
                }
                else
                {
                    result.Person = new PersonDetail();
                }

                return ActionOk( result );
            }
        }

        /// <summary>
        /// Performs the save action to join one or more people to the group.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier, this will be ignored if the block settings specify a group.</param>
        /// <param name="person">The person to be registered.</param>
        /// <param name="spouse">The spouse to be registered.</param>
        /// <param name="familyMembers">The family members to be registered.</param>
        /// <returns>
        /// The response to be displayed to the user.
        /// </returns>
        [BlockAction]
        public BlockActionResult Register( Guid? groupGuid, PersonDetail person, PersonDetail spouse, List<Guid> familyMembers )
        {
            using ( var rockContext = new RockContext() )
            {
                // Really? More than 25 people in your family? Go away.
                if ( familyMembers != null && familyMembers.Count > 25 )
                {
                    return ActionBadRequest();
                }

                var group = new GroupService( rockContext ).Get( GroupGuid ?? groupGuid ?? Guid.Empty );

                // Verify the group exists.
                if ( group == null )
                {
                    return ActionNotFound();
                }

                // Verify the group is valid.
                if ( GroupTypeGuids.Any() && !GroupTypeGuids.Contains( group.GroupType.Guid ) )
                {
                    return ActionBadRequest( "Invalid group type for current configuration." );
                }

                // Verify the data provided about the person and spouse is valid.
                if ( !IsRegistrationValid( person, spouse ) )
                {
                    return ActionBadRequest();
                }

                // Check if we have room for the registrants.
                var useSpouse = FamilyOptions == FamilyOption.SpouseOptional || FamilyOptions == FamilyOption.SpouseRequired;
                var hasSpouse = spouse != null && spouse.FirstName.IsNotNullOrWhiteSpace() && spouse.LastName.IsNotNullOrWhiteSpace();

                // Check if we are going over capacity with these registrations.
                if ( ShouldEnforceGroupCapacity() )
                {
                    var registrationCount = 1 + ( useSpouse && hasSpouse ? 1 : 0 ) + ( familyMembers != null ? familyMembers.Count : 0 );
                    var availableSpots = GetAvailableSpotsInGroup( group, null );

                    if ( availableSpots.HasValue && availableSpots.Value < registrationCount )
                    {
                        return ActionBadRequest( $"There are only {availableSpots.Value} spots available, please reduce the number of people." );
                    }
                }

                GroupMember primaryMember = null;
                var additionalMembers = new List<GroupMember>();

                rockContext.WrapTransaction( () =>
                {
                    primaryMember = AddPrimaryPersonToGroup( person, group, rockContext );

                    if ( useSpouse && hasSpouse )
                    {
                        additionalMembers.Add( AddSpouseToGroup( primaryMember.Person, spouse, group, rockContext ) );
                    }

                    if ( FamilyOptions == FamilyOption.FamilyMemberSelection )
                    {
                        additionalMembers.AddRange( AddFamilyMembersToGroup( familyMembers, group, rockContext ) );
                    }

                    rockContext.SaveChanges();
                } );

                TriggerWorkflows( primaryMember, additionalMembers );

                // Prepare the response to show the user.
                string resultContent = null;
                if ( !ResultPageGuid.HasValue )
                {
                    var mergeFields = RequestContext.GetCommonMergeFields();
                    mergeFields.AddOrReplace( "Group", group );
                    mergeFields.AddOrReplace( "Person", primaryMember.Person );
                    mergeFields.AddOrReplace( "FamilyMembers", additionalMembers.Select( a => a.Person ).ToList() );

                    resultContent = RegistrationCompletionMessage.ResolveMergeFields( mergeFields );
                }

                return ActionOk( new RegisterResult
                {
                    ResultPage = ResultPageGuid,
                    ResultPageQueryParameters = new Dictionary<string, string>
                    {
                        ["GroupGuid"] = group.Guid.ToString()
                    },
                    Content = resultContent
                } );
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The response to the GetRegistrationOptions block action.
        /// </summary>
        public class GetRegistrationOptionsResult
        {
            /// <summary>
            /// Gets or sets the person details.
            /// </summary>
            /// <value>
            /// The person details.
            /// </value>
            public PersonDetail Person { get; set; }

            /// <summary>
            /// Gets or sets the spouse details.
            /// </summary>
            /// <value>
            /// The spouse details.
            /// </value>
            public PersonDetail Spouse { get; set; }

            /// <summary>
            /// Gets or sets the family members to be shown.
            /// </summary>
            /// <value>
            /// The family members to be shown
            /// </value>
            public List<FamilyMember> FamilyMembers { get; set; }

            /// <summary>
            /// Gets or sets the number of available spots.
            /// </summary>
            /// <value>
            /// The number of available spots.
            /// </value>
            public int? AvailableSpots { get; set; }
        }

        /// <summary>
        /// The result from the Register block action.
        /// </summary>
        public class RegisterResult
        {
            /// <summary>
            /// Gets or sets the result page to direct the user to.
            /// </summary>
            /// <value>
            /// The result page to direct the user to.
            /// </value>
            public Guid? ResultPage { get; set; }

            /// <summary>
            /// Gets or sets the query parameters to pass to the result page.
            /// </summary>
            /// <value>
            /// The query parameters to pass to the result page.
            /// </value>
            public Dictionary<string, string> ResultPageQueryParameters { get; set; }

            /// <summary>
            /// Gets or sets the content to display if ResultPage is <c>null</c>.
            /// </summary>
            /// <value>
            /// The content to display if ResultPage is <c>null</c>.
            /// </value>
            public string Content { get; set; }
        }

        /// <summary>
        /// A simplified person record used in the AddToGroup block.
        /// </summary>
        public class PersonDetail
        {
            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the mobile phone.
            /// </summary>
            /// <value>
            /// The mobile phone.
            /// </value>
            public string MobilePhone { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether SMS messaging should be enabled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if SMS messaging should be enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsMessagingEnabled { get; set; }
        }

        /// <summary>
        /// A reference to an existing family member.
        /// </summary>
        public class FamilyMember
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName { get; set; }
        }

        /// <summary>
        /// The way to show family members.
        /// </summary>
        public enum FamilyOption
        {
            /// <summary>
            /// No family options will be shown.
            /// </summary>
            None = 0,

            /// <summary>
            /// The spouse will be shown but be optional.
            /// </summary>
            SpouseOptional = 1,

            /// <summary>
            /// The spouse will be shown and required.
            /// </summary>
            SpouseRequired = 2,

            /// <summary>
            /// A list of family members will be shown.
            /// </summary>
            FamilyMemberSelection = 3
        }

        #endregion
    }
}

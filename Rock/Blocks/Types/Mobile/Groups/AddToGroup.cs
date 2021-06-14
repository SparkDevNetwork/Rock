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
using System.Web.Http;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Groups.AddToGroup;
using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Adds the current person to a group passed by query string parameter.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Add To Group" )]
    [Category( "Mobile > Groups" )]
    [Description( "Adds the current person to a group passed by query string parameter." )]
    [IconCssClass( "fa fa-user-plus" )]

    #region Block Attributes

    [BooleanField( "Show Family Members",
        Description = "If the person is logged in then family members will also be shown as options to join group.",
        DefaultBooleanValue = true,
        Key = AttributeKeys.ShowFamilyMembers,
        Order = 0 )]

    [EnumField( "Mobile Phone",
        Description = "Determines if the Mobile Phone field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Key = AttributeKeys.MobilePhoneVisibility,
        Order = 1 )]

    [EnumField( "Email Address",
        Description = "Determines if the Email field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Key = AttributeKeys.EmailAddressVisibility,
        Order = 2 )]

    [GroupTypesField( "Group Types",
        Description = "Determines which group types are allowed when adding people to a group from this block.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_GENERAL,
        Key = AttributeKeys.GroupTypes,
        Order = 3 )]

    [EnumField( "Add As Status",
        Description = "The member status to add new members to the group with. Group and Role capacities will be checked if set to Active.",
        EnumSourceType = typeof( GroupMemberStatus ),
        DefaultEnumValue = ( int ) GroupMemberStatus.Active,
        Key = AttributeKeys.AddAsStatus,
        Order = 4 )]

    [DefinedValueField( "Default Connection Status",
        Description = "The connection status to use for new individuals.",
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT,
        Key = AttributeKeys.DefaultConnectionStatus,
        Order = 5 )]

    [DefinedValueField( "Default Record Status",
        Description = "The record status to use for new individuals.",
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Key = AttributeKeys.DefaultRecordStatus,
        Order = 6 )]

    [TextField( "Save Button Text",
        Description = "The text to display in the save button.",
        IsRequired = true,
        DefaultValue = "Save",
        Key = AttributeKeys.SaveButtonText,
        Order = 7 )]

    [TextField( "Family Label Text",
        Description = "The label to display above other family members.",
        IsRequired = true,
        DefaultValue = "Who Else Will Be Joining You?",
        Key = AttributeKeys.FamilyLabelText,
        Order = 8 )]

    [CodeEditorField( "Completed Template",
        Description = "The content to display after the user has been added to the group.",
        IsRequired = true,
        DefaultValue = "<Label Text=\"Thank you for your interest. You have been added to {{ Group.Name }}\" />",
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.CompletedTemplate,
        Order = 9 )]

    [WorkflowTypeField( "Workflow Type",
        Description = "The workflow to launch for each group member that was added. The Group Member object will be passed as the Entity. The primary person will be passed as the workflow initiator.",
        Key = AttributeKeys.WorkflowType,
        Order = 10 )]

    #endregion

    public class AddToGroup : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="AddToGroup"/> block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The group types attribute key.
            /// </summary>
            public const string GroupTypes = "GroupTypes";

            /// <summary>
            /// The show family members attribute key.
            /// </summary>
            public const string ShowFamilyMembers = "ShowFamilyMembers";

            /// <summary>
            /// The add as status attribute key.
            /// </summary>
            public const string AddAsStatus = "AddAsStatus";

            /// <summary>
            /// The email address visibility attribute key.
            /// </summary>
            public const string EmailAddressVisibility = "EmailAddressVisibility";

            /// <summary>
            /// The mobile phone attribute visibility key.
            /// </summary>
            public const string MobilePhoneVisibility = "MobilePhoneVisibility";

            /// <summary>
            /// The completed template attribute key.
            /// </summary>
            public const string CompletedTemplate = "CompletedTemplate";

            /// <summary>
            /// The save button text attribute key.
            /// </summary>
            public const string SaveButtonText = "SaveButtonText";

            /// <summary>
            /// The family label text attribute key.
            /// </summary>
            public const string FamilyLabelText = "FamilyLabelText";

            /// <summary>
            /// The workflow type attribute key.
            /// </summary>
            public const string WorkflowType = "WorkflowType";

            /// <summary>
            /// The default connection status attribute key.
            /// </summary>
            public const string DefaultConnectionStatus = "DefaultConnectionStatus";

            /// <summary>
            /// The default record status attribute key.
            /// </summary>
            public const string DefaultRecordStatus = "DefaultRecordStatus";
        }

        /// <summary>
        /// Gets the group type unique identifiers that are valid for use by the query string.
        /// </summary>
        /// <value>
        /// The group type unique identifiers that are valid for use by the query string.
        /// </value>
        public List<Guid> GroupTypeGuids => GetAttributeValue( AttributeKeys.GroupTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets a value indicating whether to show the family members for selection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the family members should be shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowFamilyMembers => GetAttributeValue( AttributeKeys.ShowFamilyMembers ).AsBoolean();

        /// <summary>
        /// Gets the <see cref="GroupMemberStatus"/> to use when adding a new group member.
        /// </summary>
        /// <value>
        /// The <see cref="GroupMemberStatus"/> to use when adding a new group member.
        /// </value>
        public GroupMemberStatus AddAsStatus => GetAttributeValue( AttributeKeys.AddAsStatus ).ConvertToEnum<GroupMemberStatus>();

        /// <summary>
        /// Gets the email address visibility.
        /// </summary>
        /// <value>
        /// The email address visibility.
        /// </value>
        public VisibilityTriState EmailAddressVisibility => GetAttributeValue( AttributeKeys.EmailAddressVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets the mobile phone visibility.
        /// </summary>
        /// <value>
        /// The mobile phone visibility.
        /// </value>
        public VisibilityTriState MobilePhoneVisibility => GetAttributeValue( AttributeKeys.MobilePhoneVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets the XAML template to show when the user has finished.
        /// </summary>
        /// <value>
        /// The XAML template to show when the user has finished.
        /// </value>
        public string CompletedTemplate => GetAttributeValue( AttributeKeys.CompletedTemplate );

        /// <summary>
        /// Gets the save button text.
        /// </summary>
        /// <value>
        /// The save button text.
        /// </value>
        public string SaveButtonText => GetAttributeValue( AttributeKeys.SaveButtonText );

        /// <summary>
        /// Gets the family label text.
        /// </summary>
        /// <value>
        /// The family label text.
        /// </value>
        public string FamilyLabelText => GetAttributeValue( AttributeKeys.FamilyLabelText );

        /// <summary>
        /// Gets the workflow type unique identifier.
        /// </summary>
        /// <value>
        /// The workflow type unique identifier.
        /// </value>
        public Guid? WorkflowTypeGuid => GetAttributeValue( AttributeKeys.WorkflowType ).AsGuidOrNull();

        /// <summary>
        /// Gets the default connection status when creating new records.
        /// </summary>
        /// <value>
        /// The default connection status when creating new records.
        /// </value>
        public DefinedValueCache DefaultConnectionStatus => DefinedValueCache.Get( GetAttributeValue( AttributeKeys.DefaultConnectionStatus ).AsGuid() );

        /// <summary>
        /// Gets the default record status when creating new records.
        /// </summary>
        /// <value>
        /// The default record status when creating new records.
        /// </value>
        public DefinedValueCache DefaultRecordStatus => DefinedValueCache.Get( GetAttributeValue( AttributeKeys.DefaultRecordStatus ).AsGuid() );

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

            /// <summary>
            /// Optional role to use to add the person to. If a role is provided
            /// that is not valid the group type's default role will be used.
            /// </summary>
            public const string GroupRoleGuid = "GroupRoleGuid";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 3;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Groups.AddToGroup";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Groups.AddToGroup.Configuration
            {
                EmailAddressVisibility = EmailAddressVisibility,
                MobilePhoneVisibility = MobilePhoneVisibility,
                SaveButtonText = SaveButtonText,
                FamilyLabelText = FamilyLabelText,
                ShowFamilyMembers = ShowFamilyMembers
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the primary person to the group. This is the person that entered
        /// all the information on the screen.
        /// </summary>
        /// <param name="parameters">The parameters sent by the client.</param>
        /// <param name="group">The group to add the person to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="GroupMember"/> instance that identifies the person in the group.</returns>
        private GroupMember AddPrimaryPersonToGroup( JoinGroupParameters parameters, Group group, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            var person = RequestContext.CurrentPerson;

            // If we have a logged in person, update their personal information.
            if ( person != null )
            {
                person.FirstName = parameters.FirstName;
                person.LastName = parameters.LastName;

                if ( parameters.Email.IsNotNullOrWhiteSpace() )
                {
                    person.Email = parameters.Email;
                }

                if ( PhoneNumber.CleanNumber( parameters.MobilePhone ).IsNotNullOrWhiteSpace() )
                {
                    int phoneNumberTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                    var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                        person.PhoneNumbers.Add( phoneNumber );
                    }

                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( "+1" );
                    phoneNumber.Number = PhoneNumber.CleanNumber( parameters.MobilePhone );
                }
            }

            // If not logged in, try to match to an existing person.
            if ( person == null )
            {
                var matchQuery = new PersonService.PersonMatchQuery( parameters.FirstName, parameters.LastName, parameters.Email, parameters.MobilePhone );
                person = personService.FindPerson( matchQuery, true );
            }

            // If no match, create a new person.
            if ( person == null )
            {
                person = CreateNewPerson( parameters.FirstName, parameters.LastName, parameters.Email, parameters.MobilePhone );
            }

            return AddPersonToGroup( person, group, rockContext );
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
            if ( RequestContext.CurrentPerson == null || !ShowFamilyMembers )
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
            var groupRoleGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupRoleGuid ).AsGuidOrNull();

            // Find the existing record, otherwise create a new one.
            var member = group.Members.Where( gm => gm.PersonId == person.Id ).FirstOrDefault();
            if ( member == null )
            {
                member = new GroupMember { Id = 0 };
                rockContext.GroupMembers.Add( member );

                member.GroupId = group.Id;
                // This is needed to trigger workflows later.
                member.Person = person;
                member.PersonId = person.Id;
                member.DateTimeAdded = RockDateTime.Now;
                member.GroupMemberStatus = AddAsStatus;

                member.GroupRoleId = group.GroupType.DefaultGroupRoleId ?? 0;

                if ( groupRoleGuid.HasValue )
                {
                    var role = group.GroupType.Roles.FirstOrDefault( a => a.Guid == groupRoleGuid.Value );

                    if ( role != null )
                    {
                        member.GroupRoleId = role.Id;
                    }
                }
            }
            else if ( member.GroupMemberStatus == GroupMemberStatus.Inactive )
            {
                member.GroupMemberStatus = AddAsStatus;
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
            if ( !WorkflowTypeGuid.HasValue )
            {
                return;
            }

            var workflowType = WorkflowTypeCache.Get( WorkflowTypeGuid.Value );

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
        private Person CreateNewPerson( string firstName, string lastName, string email, string mobilePhone )
        {
            var rockContext = new RockContext();

            Person person = new Person
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                ConnectionStatusValueId = DefaultConnectionStatus?.Id,
                RecordStatusValueId = DefaultRecordStatus?.Id
            };

            if ( mobilePhone.IsNotNullOrWhiteSpace() )
            {
                int phoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                var phoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = phoneNumberTypeId,
                    Number = PhoneNumber.CleanNumber( mobilePhone )
                };

                person.PhoneNumbers.Add( phoneNumber );
            }

            PersonService.SaveNewPerson( person, rockContext, null, false );

            return person;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the family members of the currently logged in person.
        /// </summary>
        /// <returns>A collection of objects that identify the name and unique identifier of each family member.</returns>
        [BlockAction]
        public BlockActionResult GetFamilyMembers()
        {
            if ( RequestContext.CurrentPerson != null && ShowFamilyMembers )
            {
                using ( var rockContext = new RockContext() )
                {
                    var family = RequestContext.CurrentPerson
                        .GetFamilyMembers( false, rockContext )
                        .AsNoTracking()
                        .Include( a => a.Person.Aliases )
                        .ToList()
                        .Select( a => new
                        {
                            a.Person.PrimaryAlias.Guid,
                            Name = a.Person.FullName
                        } )
                        .ToList();

                    return ActionOk( family );
                }
            }

            return ActionOk( new object[0] );
        }

        /// <summary>
        /// Performs the save action to join one or more people to the group.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The response to be displayed to the user.</returns>
        [BlockAction]
        public BlockActionResult JoinGroup( [FromBody] JoinGroupParameters parameters )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupGuid ).AsGuid();
                var group = new GroupService( rockContext ).Get( groupGuid );

                // Really? More than 100 people in your family? Go away.
                if ( parameters.FamilyMembers != null && parameters.FamilyMembers.Count > 100 )
                {
                    return ActionBadRequest();
                }

                // Verify the group exists.
                if ( group == null )
                {
                    return ActionOk( new JoinGroupResult
                    {
                        Error = "Group not found."
                    } );
                }

                // Verify the group is valid.
                if ( !GroupTypeGuids.Contains( group.GroupType.Guid ) )
                {
                    return ActionOk( new JoinGroupResult
                    {
                        Error = "Invalid group type for current configuration."
                    } );
                }

                var primaryMember = AddPrimaryPersonToGroup( parameters, group, rockContext );
                var additionalMembers = AddFamilyMembersToGroup( parameters.FamilyMembers, group, rockContext );

                // If there are any family members to process, then do so.

                rockContext.SaveChanges();

                TriggerWorkflows( primaryMember, additionalMembers );

                // Prepare the response to show the user.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "Group", group );
                mergeFields.AddOrReplace( "Person", primaryMember.Person );
                mergeFields.AddOrReplace( "FamilyMembers", additionalMembers.Select( a => a.Person ).ToList() );

                return ActionOk( new JoinGroupResult
                {
                    Content = CompletedTemplate.ResolveMergeFields( mergeFields )
                } );
            }
        }

        #endregion
    }
}

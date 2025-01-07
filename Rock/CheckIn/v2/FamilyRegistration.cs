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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Provides family registration services for the check-in kiosk.
    /// </summary>
    internal class FamilyRegistration
    {
        #region Fields

        /// <summary>
        /// The context to use when accessing the database.
        /// </summary>
        private readonly RockContext _rockContext;

        /// <summary>
        /// The current person to use for security checks and other operations
        /// that need to know the person performing the task.
        /// </summary>
        private readonly Person _currentPerson;

        /// <summary>
        /// The check-in configuration template.
        /// </summary>
        private readonly TemplateConfigurationData _template;

        /// <summary>
        /// The identifier of the Group Type Family role for Adults.
        /// </summary>
        private readonly Lazy<int> GroupTypeRoleAdultId;

        /// <summary>
        /// The identifier of the Group Type Family role for Children.
        /// </summary>
        private readonly Lazy<int> GroupTypeRoleChildId;

        /// <summary>
        /// The identifier of the AlternateId person search defined value.
        /// </summary>
        private readonly Lazy<int> PersonSearchAlternateId;

        /// <summary>
        /// The identifier of the Person record type defined value.
        /// </summary>
        private readonly Lazy<int> RecordTypePersonId;

        /// <summary>
        /// The identifier of the Married marital status defined value.
        /// </summary>
        private readonly Lazy<int> MaritalStatusMarriedId;

        /// <summary>
        /// The identifier of the Single marital status defined value.
        /// </summary>
        private readonly Lazy<int> MaritalStatusSingleId;

        /// <summary>
        /// The identifier of the Mobile phone type defined value.
        /// </summary>
        private readonly Lazy<int> PhoneTypeMobileId;

        /// <summary>
        /// The identifier of the Can Check-in known relationship role.
        /// </summary>
        private readonly Lazy<int> GroupTypeRoleCanCheckInId;

        /// <summary>
        /// The default value configured on the Family group type to specify
        /// the default state of SMS enabled on mobile phones.
        /// </summary>
        private readonly Lazy<bool?> GroupTypeDefaultSmsEnabled;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="FamilyRegistration"/> with
        /// the configured parameters.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="currentPerson">The <see cref="Person"/> that is performing the operation.</param>
        /// <param name="template">The check-in template that provides configuration data.</param>
        public FamilyRegistration( RockContext rockContext, Person currentPerson, TemplateConfigurationData template )
        {
            _rockContext = rockContext;
            _currentPerson = currentPerson;
            _template = template;

            GroupTypeRoleAdultId = new Lazy<int>( () => GroupTypeRoleCache
                .Get( SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid(), _rockContext )
                .Id );

            GroupTypeRoleChildId = new Lazy<int>( () => GroupTypeRoleCache
                .Get( SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid(), _rockContext )
                .Id );

            GroupTypeRoleCanCheckInId = new Lazy<int>( () => GroupTypeRoleCache
                .Get( SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid(), _rockContext )
                .Id );

            PersonSearchAlternateId = new Lazy<int>( () => DefinedValueCache
                .Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid(), _rockContext )
                .Id );

            MaritalStatusMarriedId = new Lazy<int>( () => DefinedValueCache
                .Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid(), _rockContext )
                .Id );

            MaritalStatusSingleId = new Lazy<int>( () => DefinedValueCache
                .Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid(), _rockContext )
                .Id );

            RecordTypePersonId = new Lazy<int>( () => DefinedValueCache
                .Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid(), _rockContext )
                .Id );

            PhoneTypeMobileId = new Lazy<int>( () => DefinedValueCache
                .Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), _rockContext )
                .Id );

            GroupTypeDefaultSmsEnabled = new Lazy<bool?>( () => GroupTypeCache
                .GetFamilyGroupType()
                .GetAttributeValue( SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED )
                .AsBooleanOrNull() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the family bag for the specified family group. This contains
        /// the information about the family and people in the family for
        /// a kiosk to edit an existing family.
        /// </summary>
        /// <param name="group">The family <see cref="Group"/> that will be viewed or edited on a kiosk.</param>
        /// <returns>An instance of <see cref="ValidPropertiesBox{TPropertyBag}"/> that wraps the <see cref="RegistrationFamilyBag"/>.</returns>
        public ValidPropertiesBox<RegistrationFamilyBag> GetFamilyBag( Group group )
        {
            var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), _rockContext ).Id;
            var attributeGuids = _template.RequiredAttributeGuidsForFamilies
                .Union( _template.OptionalAttributeGuidsForFamilies )
                .ToList();
            AddressControlBag address = null;

            if ( group.GroupLocations != null )
            {
                var location = group.GroupLocations
                    .Where( l => l.GroupLocationTypeValueId == homeLocationTypeId )
                    .Select( l => l.Location )
                    .FirstOrDefault();

                address = new AddressControlBag
                {
                    City = location?.City,
                    Country = location?.Country,
                    State = location?.State,
                    Locality = location?.County,
                    PostalCode = location?.PostalCode,
                    Street1 = location?.Street1,
                    Street2 = location?.Street2,
                };
            }

            if ( group.Attributes == null )
            {
                group.LoadAttributes( _rockContext );
            }

            group.Members.Select( gm => gm.Person ).LoadAttributes( _rockContext );

            var bag = new RegistrationFamilyBag
            {
                Id = group.Id != 0 ? group.IdKey : null,
                FamilyName = group.Name,
                AttributeValues = group.GetPublicAttributeValuesForEdit(
                    _currentPerson,
                    false,
                    a => attributeGuids.Contains( a.Guid ) ),
                Address = address
            };

            return new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };
        }

        /// <summary>
        /// Gets the person bags for the specified family group. This contains
        /// the direct family members and those allowed via check-in relationship.
        /// </summary>
        /// <param name="group">The family <see cref="Group"/> that will be viewed or edited on a kiosk.</param>
        /// <param name="canCheckInMembers">The <see cref="GroupMember"/> records that represent relationships allowed for check-in.</param>
        /// <returns>An list of <see cref="RegistrationPersonBag"/> objects.</returns>
        public List<ValidPropertiesBox<RegistrationPersonBag>> GetFamilyMemberBags( Group group, List<GroupMember> canCheckInMembers )
        {
            group.Members.Select( gm => gm.Person ).LoadAttributes( _rockContext );

            var personBags = group.Members
                .Select( gm => GetPersonBag( gm.Person, null ) )
                .Select( bag => new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = bag,
                    ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } )
                .ToList();

            if ( canCheckInMembers != null )
            {
                AmendRelatedMemberBags( personBags, canCheckInMembers );
            }

            return personBags;
        }

        /// <summary>
        /// Gets the person bags for the specified related members. This contains
        /// the direct family members and those allowed via check-in relationship.
        /// </summary>
        /// <param name="personBags">The list of <see cref="RegistrationPersonBag"/> objects to amend.</param>
        /// <param name="members">The related <see cref="GroupMember"/> that will be viewed or edited on a kiosk.</param>
        private void AmendRelatedMemberBags( List<ValidPropertiesBox<RegistrationPersonBag>> personBags, List<GroupMember> members )
        {
            members.Select( gm => gm.Person )
                .DistinctBy( p => p.Id )
                .LoadAttributes( _rockContext );

            foreach ( var member in members )
            {
                if ( personBags.Any( p => p.Bag.Id == member.Person.IdKey ) )
                {
                    continue;
                }

                var bag = GetPersonBag( member.Person, member.GroupRoleId );

                personBags.Add( new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = bag,
                    ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } );
            }
        }

        /// <summary>
        /// Creates or updates any family and person records and saves all
        /// changes to the database.
        /// </summary>
        /// <param name="registrationFamily">The details about the family to be saved.</param>
        /// <param name="people">The people to be saved in the family.</param>
        /// <param name="defaultCampusId">The default campus to use when creating new families.</param>
        /// <param name="removedPersonIdKeys">The identifier keys of the people that should be removed from the primary family.</param>
        /// <returns>An instance of <see cref="FamilyRegistrationSaveResult"/> that describes if the operation was successful or not.</returns>
        public FamilyRegistrationSaveResult SaveRegistration( ValidPropertiesBox<RegistrationFamilyBag> registrationFamily, List<ValidPropertiesBox<RegistrationPersonBag>> people, int? defaultCampusId, List<string> removedPersonIdKeys )
        {
            if ( !HasAllRequiredValues( registrationFamily, people, out var errorMessage ) )
            {
                return new FamilyRegistrationSaveResult
                {
                    ErrorMessage = errorMessage
                };
            }

            try
            {
                var saveResult = new FamilyRegistrationSaveResult();

                _rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( _rockContext );
                    Group primaryFamily = null;

                    if ( registrationFamily.Bag.Id.IsNotNullOrWhiteSpace() )
                    {
                        primaryFamily = groupService.Get( registrationFamily.Bag.Id, false );
                    }

                    var registrationPeople = new List<(ValidPropertiesBox<RegistrationPersonBag> RegistrationPerson, Person Person)>();

                    // Loop through all people and add/update as needed.
                    foreach ( var registrationPerson in people )
                    {
                        var person = CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

                        registrationPeople.Add( (registrationPerson, person) );
                    }

                    // Update or create the primary family.
                    if ( primaryFamily == null )
                    {
                        var familyLastName = GetDefaultFamilyLastName( people );

                        primaryFamily = CreatePrimaryFamily( registrationFamily, familyLastName, defaultCampusId, saveResult );
                    }

                    UpdateFamilyAttributeValues( primaryFamily, registrationFamily );
                    EnsurePeopleInPrimaryFamilyAreMembersOfGroup( primaryFamily, registrationPeople );
                    EnsurePeopleNotInPrimaryFamilyHaveAFamily( registrationPeople, defaultCampusId, saveResult );
                    RemoveFamilyMembers( primaryFamily, removedPersonIdKeys, saveResult );

                    saveResult.PrimaryFamily = primaryFamily;
                } );

                saveResult.IsSuccess = true;

                return saveResult;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return new FamilyRegistrationSaveResult
                {
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// This handles any post-save tasks that should be performed. This
        /// method should be called after the SaveRegistration method so that
        /// all standard post-save processing can be performed.
        /// </summary>
        /// <remarks>
        /// In rare cases, such as testing, we don't want these operations to be
        /// performed.
        /// </remarks>
        /// <param name="saveResult">The result of a previous save operation.</param>
        public void ProcessSaveResult( FamilyRegistrationSaveResult saveResult )
        {
            if ( !saveResult.IsSuccess )
            {
                return;
            }

            if ( saveResult.NewFamilyList.Any() )
            {
                var addFamilyWorkflowTypes = _template.AddFamilyWorkflowTypeGuids
                    .Select( guid => WorkflowTypeCache.Get( guid, _rockContext ) )
                    .Where( wt => wt != null )
                    .ToList();

                // Only fire a NewFamily workflow if the Primary family is new.
                // We don't fire workflows for any 'can check-in' families that
                // were created when adding a family friend or removing somebody
                // from the family.
                var newPrimaryFamily = saveResult.NewFamilyList.FirstOrDefault( g => g.Id == saveResult.PrimaryFamily.Id );
                if ( newPrimaryFamily != null )
                {
                    foreach ( var addFamilyWorkflowType in addFamilyWorkflowTypes )
                    {
                        LaunchWorkflowTransaction launchWorkflowTransaction = new LaunchWorkflowTransaction<Group>( addFamilyWorkflowType.Id, newPrimaryFamily.Id );
                        launchWorkflowTransaction.Enqueue();
                    }
                }
            }

            if ( saveResult.NewPersonList.Any() )
            {
                var addPersonWorkflowTypes = _template.AddPersonWorkflowTypeGuids
                    .Select( guid => WorkflowTypeCache.Get( guid, _rockContext ) )
                    .Where( wt => wt != null )
                    .ToList();

                foreach ( var newPerson in saveResult.NewPersonList )
                {
                    foreach ( var addPersonWorkflowType in addPersonWorkflowTypes )
                    {
                        LaunchWorkflowTransaction launchWorkflowTransaction = new LaunchWorkflowTransaction<Person>( addPersonWorkflowType.Id, newPerson.Id );
                        launchWorkflowTransaction.Enqueue();
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the person bag for the specified person. This will contain
        /// all the information about an individual so they can be viewed or
        /// edited on a kiosk.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> to be viewed or edited.</param>
        /// <param name="relationshipToAdultId">The role identifier of the relationship between the adults and this person. Pass <c>null</c> if this person is in the same family.</param>
        /// <returns>An instance of <see cref="RegistrationPersonBag"/>.</returns>
        internal RegistrationPersonBag GetPersonBag( Person person, int? relationshipToAdultId )
        {
            List<Guid> attributeGuids;
            var isAdult = person.AgeClassification == AgeClassification.Adult;

            if ( isAdult )
            {
                attributeGuids = _template.RequiredAttributeGuidsForAdults
                    .Union( _template.OptionalAttributeGuidsForAdults )
                    .ToList();
            }
            else
            {
                attributeGuids = _template.RequiredAttributeGuidsForChildren
                    .Union( _template.OptionalAttributeGuidsForChildren )
                    .ToList();
            }

            return new RegistrationPersonBag
            {
                Id = person.IdKey,
                NickName = person.NickName,
                LastName = person.LastName,
                Suffix = GetDefinedValueBag( person.SuffixValueId ),
                Gender = person.Gender,
                BirthDate = person.BirthDate?.ToRockDateTimeOffset(),
                Email = person.Email,
                Grade = GetGradeBag( person.GradeOffset ),
                PhoneNumber = GetMobilePhoneNumberBag( person ),
                IsAdult = isAdult,
                IsMarried = person.MaritalStatusValueId == MaritalStatusMarriedId.Value,
                AlternateId = person.GetPersonSearchKeys( _rockContext )
                    .Where( sk => sk.SearchTypeValueId == PersonSearchAlternateId.Value )
                    .Select( sk => sk.SearchValue )
                    .FirstOrDefault(),
                Race = GetDefinedValueBag( person.RaceValueId ),
                Ethnicity = GetDefinedValueBag( person.EthnicityValueId ),
                RecordStatus = GetDefinedValueBag( person.RecordStatusValueId ),
                ConnectionStatus = GetDefinedValueBag( person.ConnectionStatusValueId ),
                RelationshipToAdult = GetGroupTypeRoleBag( relationshipToAdultId ),
                AttributeValues = person.GetPublicAttributeValuesForEdit(
                    _currentPerson,
                    false,
                    a => attributeGuids.Contains( a.Guid ) )
            };
        }

        /// <summary>
        /// Gets the bag instance for the <see cref="DefinedValue"/> specified.
        /// </summary>
        /// <param name="groupTypeRoleId">The identifier of the <see cref="DefinedValue"/>.</param>
        /// <returns>An instance of <see cref="ListItemBag"/> that represents the defined value or <c>null</c> if it was not found.</returns>
        private ListItemBag GetDefinedValueBag( int? groupTypeRoleId )
        {
            if ( !groupTypeRoleId.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( groupTypeRoleId.Value, _rockContext )?.ToListItemBag();
        }

        /// <summary>
        /// Gets the bag instance for the <see cref="GroupTypeRole"/> specified.
        /// </summary>
        /// <param name="groupTypeRoleId">The identifier of the <see cref="GroupTypeRole"/>.</param>
        /// <returns>An instance of <see cref="ListItemBag"/> that represents the group type role or <c>null</c> if it was not found.</returns>
        private ListItemBag GetGroupTypeRoleBag( int? groupTypeRoleId )
        {
            if ( !groupTypeRoleId.HasValue )
            {
                return null;
            }

            return GroupTypeRoleCache.Get( groupTypeRoleId.Value, _rockContext )?.ToListItemBag();
        }

        /// <summary>
        /// Gets the identifier of the <see cref="DefinedValue"/> represented
        /// by <paramref name="itemBag"/>.
        /// </summary>
        /// <param name="itemBag">The bag that represents the <see cref="DefinedValue"/> or <c>null</c> if none is selected.</param>
        /// <returns>The identifier of the <see cref="DefinedValue"/> or <c>null</c>.</returns>
        private int? GetDefinedValueId( ListItemBag itemBag )
        {
            var guid = itemBag?.Value?.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( guid.Value, _rockContext )?.Id;
        }

        /// <summary>
        /// Gets the bag instance that can be used to represent the grade
        /// of an individual.
        /// </summary>
        /// <param name="gradeOffset">The value of <see cref="Person.GradeOffset"/>.</param>
        /// <returns>An instance of <see cref="ListItemBag"/> that represents the grade.</returns>
        private ListItemBag GetGradeBag( int? gradeOffset )
        {
            if ( !gradeOffset.HasValue || gradeOffset < 0 )
            {
                return null;
            }

            var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid(), _rockContext );
            var grade = schoolGrades.DefinedValues
                .OrderBy( a => a.Value.AsInteger() )
                .Where( a => a.Value.AsInteger() >= gradeOffset.Value )
                .FirstOrDefault();

            if ( grade == null )
            {
                return null;
            }

            return new ListItemBag
            {
                Value = grade.Value.ToString(),
                Text = grade.Description
            };
        }

        /// <summary>
        /// Gets the phone number bag for the mobile phone number of the person.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose phone number will be retrieved.</param>
        /// <returns>An instance of <see cref="PhoneNumberBoxWithSmsControlBag"/> that represents the phone number or <c>null</c> if no mobile number was found.</returns>
        private PhoneNumberBoxWithSmsControlBag GetMobilePhoneNumberBag( Person person )
        {
            var phoneNumber = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            if ( phoneNumber == null )
            {
                return null;
            }

            return new PhoneNumberBoxWithSmsControlBag
            {
                CountryCode = phoneNumber.CountryCode,
                Number = phoneNumber.Number,
                IsMessagingEnabled = phoneNumber.IsMessagingEnabled
            };
        }

        /// <summary>
        /// Gets the default family last name to use for the specified
        /// family members.
        /// </summary>
        /// <param name="people">The people to be registered.</param>
        /// <returns>The last name of the best matching person or <c>null</c> if there are no people.</returns>
        internal string GetDefaultFamilyLastName( List<ValidPropertiesBox<RegistrationPersonBag>> people )
        {
            // Prefer the last name of any adults over that of children.
            return people
                .OrderByDescending( a => a.Bag.IsAdult )
                .Select( a => a.Bag.LastName )
                .FirstOrDefault();
        }

        /// <summary>
        /// Verifies that the <see cref="RegistrationFamilyBag"/> has all
        /// required properties specified. This includes all properties on
        /// any <see cref="RegistrationPersonBag"/> objects as well.
        /// </summary>
        /// <param name="registrationFamily">The family to be registered.</param>
        /// <param name="people">The people to be registered.</param>
        /// <param name="errorMessage">An error message that describes what field was missing.</param>
        /// <returns><c>true</c> if all required properties are present; otherwise <c>false</c>.</returns>
        internal bool HasAllRequiredValues( ValidPropertiesBox<RegistrationFamilyBag> registrationFamily, List<ValidPropertiesBox<RegistrationPersonBag>> people, out string errorMessage )
        {
            if ( !registrationFamily.IsValidProperty( nameof( registrationFamily.Bag.Id ) ) )
            {
                errorMessage = $"Family is missing required {nameof( registrationFamily.Bag.Id )} property.";
                return false;
            }

            if ( !registrationFamily.IsValidProperty( nameof( registrationFamily.Bag.FamilyName ) ) )
            {
                errorMessage = $"Family is missing required {nameof( registrationFamily.Bag.FamilyName )} property.";
                return false;
            }

            foreach ( var registrationPerson in people )
            {
                if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.Id ) ) )
                {
                    errorMessage = $"Person is missing required {nameof( registrationPerson.Bag.Id )} property.";
                    return false;
                }

                if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.NickName ) ) )
                {
                    errorMessage = $"Person is missing required {nameof( registrationPerson.Bag.NickName )} property.";
                    return false;
                }

                if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.LastName ) ) )
                {
                    errorMessage = $"Person is missing required {nameof( registrationPerson.Bag.LastName )} property.";
                    return false;
                }

                if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.IsAdult ) ) )
                {
                    errorMessage = $"Person is missing required {nameof( registrationPerson.Bag.IsAdult )} property.";
                    return false;
                }

                if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.RelationshipToAdult ) ) )
                {
                    errorMessage = $"Person is missing required {nameof( registrationPerson.Bag.RelationshipToAdult )} property.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Creates the primary family for a registration. This should be called
        /// when a brand new family is being registered.
        /// </summary>
        /// <param name="registrationFamily">The details of the family being registered.</param>
        /// <param name="defaultFamilyLastName">The default last name to use for the family if it wasn't specified.</param>
        /// <param name="defaultCampusId">The campus to use for the new family.</param>
        /// <param name="saveResult">Will be updated with the new <see cref="Group"/> object.</param>
        /// <returns>An new instance of <see cref="Group"/> that will have already been saved to the database.</returns>
        internal Group CreatePrimaryFamily( ValidPropertiesBox<RegistrationFamilyBag> registrationFamily, string defaultFamilyLastName, int? defaultCampusId, FamilyRegistrationSaveResult saveResult )
        {
            // new family and no family found by looking up matching adults, so create a new family
            var family = new Group
            {
                Name = registrationFamily.Bag.FamilyName,
                GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id,
                CampusId = defaultCampusId
            };

            if ( family.Name.IsNullOrWhiteSpace() )
            {
                if ( defaultFamilyLastName.IsNotNullOrWhiteSpace() )
                {
                    family.Name = $"{defaultFamilyLastName} Family";
                }
            }

            if ( family.Name.IsNullOrWhiteSpace() )
            {
                throw new Exception( "Unable to determine family name for new family." );
            }

            new GroupService( _rockContext ).Add( family );
            saveResult.NewFamilyList.Add( family );
            _rockContext.SaveChanges();

            return family;
        }

        /// <summary>
        /// Builds the match query that will be used to try and find any
        /// existing records that should be matched to the person.
        /// </summary>
        /// <param name="registrationPerson">The details of the person being registered.</param>
        /// <returns>An instance of <see cref="PersonService.PersonMatchQuery"/> that can be used to search for a matching person.</returns>
        internal PersonService.PersonMatchQuery GetPersonMatchQuery( ValidPropertiesBox<RegistrationPersonBag> registrationPerson )
        {
            string email = null;
            string phoneNumber = null;
            Gender? gender = null;
            DateTime? birthdate = null;
            int? suffixId = null;

            if ( registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.Suffix ) ) )
            {
                suffixId = registrationPerson.Bag.Suffix.Value.IsNotNullOrWhiteSpace()
                    ? DefinedValueCache.Get( registrationPerson.Bag.Suffix.Value.AsGuid(), _rockContext )?.Id
                    : null;
            }

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Email ),
                () => email = registrationPerson.Bag.Email );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.PhoneNumber ),
                () => phoneNumber = registrationPerson.Bag.PhoneNumber?.Number );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Gender ),
                () => gender = registrationPerson.Bag.Gender );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.BirthDate ),
                () => birthdate = registrationPerson.Bag.BirthDate?.ToOrganizationDateTime() );

            return new PersonService.PersonMatchQuery( registrationPerson.Bag.NickName,
                registrationPerson.Bag.LastName,
                email,
                phoneNumber,
                gender,
                birthdate,
                suffixId );
        }

        /// <summary>
        /// Creates a new person or updates an existing person with the details
        /// from the registration request. If <paramref name="primaryFamily"/>
        /// is <c>null</c> and an existing person is found then it will be
        /// updated to be the family of the existing person.
        /// </summary>
        /// <param name="registrationPerson">The details of the person being registered.</param>
        /// <param name="primaryFamily">The <see cref="Group"/> object that identifies the primary family for this registration.</param>
        /// <param name="saveResult">Will be updated with any new people that were created.</param>
        /// <returns>The <see cref="Person"/> that was either created or updated.</returns>
        internal Person CreateOrUpdatePerson( ValidPropertiesBox<RegistrationPersonBag> registrationPerson, ref Group primaryFamily, FamilyRegistrationSaveResult saveResult )
        {
            var person = GetExistingMatchedOrNewPerson( registrationPerson, saveResult, ref primaryFamily, out var isMatched );

            // NOTE: NickName, LastName, Gender, MaritalStatusValueId should
            // replace existing values if they were provided even if it is a
            // matched person.
            person.NickName = registrationPerson.Bag.NickName;
            person.LastName = registrationPerson.Bag.LastName;

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Gender ),
                () => person.Gender = registrationPerson.Bag.Gender );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.IsMarried ), () =>
            {
                /*
                 * Daniel Hazelbaker - 9/6/2024
                 * 
                 * This logic is straight forward but needs some explanation
                 * for why it is done this way.
                 * 
                 * There is a potential issue here because we have a boolean
                 * to represent the marital status. But in truth we can have
                 * any number of marital status values. For example, if Ted
                 * currently has a marital status of Divorced, we don't want
                 * to switch him to Single if he answered "no" to being
                 * married.
                 * 
                 * So the logic is that if they said "married" then we are
                 * going to mark them as married. But if they said they were
                 * not married, then we only switch them to single if the
                 * current status is "single".
                 */
                if ( registrationPerson.Bag.IsMarried )
                {
                    person.MaritalStatusValueId = MaritalStatusMarriedId.Value;
                }
                else if ( person.MaritalStatusValueId == MaritalStatusMarriedId.Value )
                {
                    person.MaritalStatusValueId = MaritalStatusSingleId.Value;
                }
                else if ( !person.MaritalStatusValueId.HasValue )
                {
                    // If no current value, it's okay to change.
                    person.MaritalStatusValueId = MaritalStatusSingleId.Value;
                }
            } );


            // If the registrationPerson was matched to an existing Person
            // record then don't overwrite existing values with blank values.
            var saveEmptyValues = !isMatched;

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Suffix ), () =>
            {
                var suffixValueId = GetDefinedValueId( registrationPerson.Bag.Suffix );

                if ( suffixValueId.HasValue || saveEmptyValues )
                {
                    person.SuffixValueId = suffixValueId;
                }
            } );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.BirthDate ), () =>
            {
                if ( registrationPerson.Bag.BirthDate.HasValue || saveEmptyValues )
                {
                    person.SetBirthDate( registrationPerson.Bag.BirthDate?.ToOrganizationDateTime() );
                }
            } );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Email ), () =>
            {
                if ( registrationPerson.Bag.Email.IsNotNullOrWhiteSpace() || saveEmptyValues )
                {
                    person.Email = registrationPerson.Bag.Email;
                }
            } );

            registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Grade ), () =>
            {
                var gradeOffset = registrationPerson.Bag.Grade?.Value.AsIntegerOrNull();

                if ( gradeOffset.HasValue || saveEmptyValues )
                {
                    person.GradeOffset = gradeOffset;
                }
            } );

            // If a match was found, then we don't update these values.
            if ( !isMatched )
            {
                registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.RecordStatus ),
                    () => person.RecordStatusValueId = GetDefinedValueId( registrationPerson.Bag.RecordStatus ) );

                registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.ConnectionStatus ),
                    () => person.ConnectionStatusValueId = GetDefinedValueId( registrationPerson.Bag.ConnectionStatus ) );

                registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Ethnicity ),
                    () => person.EthnicityValueId = GetDefinedValueId( registrationPerson.Bag.Ethnicity ) );

                registrationPerson.IfValidProperty( nameof( registrationPerson.Bag.Race ),
                    () => person.RaceValueId = GetDefinedValueId( registrationPerson.Bag.Race ) );
            }

            var isNewPerson = person.Id == 0;

            _rockContext.SaveChanges();

            UpdatePersonAlternateId( person, registrationPerson, isNewPerson );
            UpdatePersonAttributeValues( person, registrationPerson, saveEmptyValues );
            UpdatePersonMobilePhoneNumber( person, registrationPerson, saveEmptyValues );

            _rockContext.SaveChanges();

            return person;
        }

        /// <summary>
        /// If the registration details specify a new person be created then
        /// first we will try to match to an existing person in the database.
        /// If that fails we will then create a new person. If the registration
        /// details specify editing an existing person then that person will
        /// be loaded from the database.
        /// </summary>
        /// <param name="registrationPerson">The details of the person being registered.</param>
        /// <param name="saveResult">This will be updated with any new person records created.</param>
        /// <param name="primaryFamily">The primary family of the registration. If <c>null</c> and an adult is matched then the family of that adult will be used to update this value.</param>
        /// <param name="isMatched">A boolean value indicating whether the person is matched.</param>
        /// <returns>The <see cref="Person"/> record for this registration request.</returns>
        private Person GetExistingMatchedOrNewPerson( ValidPropertiesBox<RegistrationPersonBag> registrationPerson, FamilyRegistrationSaveResult saveResult, ref Group primaryFamily, out bool isMatched )
        {
            var personService = new PersonService( _rockContext );

            if ( registrationPerson.Bag.Id.IsNullOrWhiteSpace() )
            {
                var personQuery = GetPersonMatchQuery( registrationPerson );

                var person = personService.FindPerson( personQuery, true );

                isMatched = person != null;

                // If this is a new family, but we found a matching adult
                // person, use that person's family as the family.
                // NOTE: We are using person.AgeClassification here instead of
                // registrationPerson.Bag.IsAdult because we want to know if
                // the original matched person was an Adult - not what was set
                // in the UI.
                if ( person != null && primaryFamily == null && person.AgeClassification == AgeClassification.Adult )
                {
                    primaryFamily = person.GetFamily( _rockContext );
                }

                if ( person == null )
                {
                    person = new Person
                    {
                        RecordTypeValueId = RecordTypePersonId.Value
                    };

                    personService.Add( person );
                    saveResult.NewPersonList.Add( person );
                }

                return person;
            }
            else
            {
                var person = personService.Get( registrationPerson.Bag.Id, false );

                if ( person == null )
                {
                    throw new Exception( "Person was not found." );
                }

                isMatched = false;

                return person;
            }
        }

        /// <summary>
        /// Updates the person's alternate search key. If a new person is being
        /// created then any existing search key will be replaced. Otherwise a
        /// new search key will be created unless one with the same value already
        /// exists for that person.
        /// </summary>
        /// <param name="person">The person whose search key is to be updated.</param>
        /// <param name="registrationPerson">The details of the person being registered.</param>
        /// <param name="isNewPerson"><c>true</c> if <paramref name="person"/> is a newly created person; otherwise <c>false</c>.</param>
        /// <returns>The <see cref="PersonSearchKey"/> that was created or updated; or <c>null</c> if no update was requested.</returns>
        internal PersonSearchKey UpdatePersonAlternateId( Person person, ValidPropertiesBox<RegistrationPersonBag> registrationPerson, bool isNewPerson )
        {
            if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.AlternateId ) ) )
            {
                return null;
            }

            var alternateId = registrationPerson.Bag.AlternateId;

            if ( alternateId.IsNullOrWhiteSpace() )
            {
                return null;
            }

            PersonSearchKey personAlternateValueIdSearchKey;
            var personSearchKeyService = new PersonSearchKeyService( _rockContext );

            if ( isNewPerson )
            {
                // If we added a new person, a default AlternateId was probably
                // added in the service layer. If a specific Alternate Id was
                // specified, make sure that their SearchKey is updated.
                personAlternateValueIdSearchKey = person.GetPersonSearchKeys( _rockContext )
                    .Where( a => a.SearchTypeValueId == PersonSearchAlternateId.Value )
                    .FirstOrDefault();
            }
            else
            {
                // See if the key already exists and if it doesn't already exist
                // then we will create a new one.
                personAlternateValueIdSearchKey = person.GetPersonSearchKeys( _rockContext )
                    .Where( a => a.SearchTypeValueId == PersonSearchAlternateId.Value && a.SearchValue == alternateId )
                    .FirstOrDefault();
            }

            if ( personAlternateValueIdSearchKey == null )
            {
                personAlternateValueIdSearchKey = new PersonSearchKey
                {
                    PersonAliasId = person.PrimaryAliasId,
                    SearchTypeValueId = PersonSearchAlternateId.Value
                };
                personSearchKeyService.Add( personAlternateValueIdSearchKey );
            }

            if ( personAlternateValueIdSearchKey.SearchValue != alternateId )
            {
                personAlternateValueIdSearchKey.SearchValue = alternateId;
                _rockContext.SaveChanges();
            }

            return personAlternateValueIdSearchKey;
        }

        /// <summary>
        /// Updates the person's mobile phone number to match the values provided
        /// in the registration details. If <paramref name="saveEmptyValues"/> is
        /// <c>false</c> and no phone number was specified then no change will be
        /// made.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose mobile number will be updated.</param>
        /// <param name="registrationPerson">The registration details that specify the new mobile number.</param>
        /// <param name="saveEmptyValues"><c>true</c> if a blank/empty value should be saved to the database.</param>
        internal void UpdatePersonMobilePhoneNumber( Person person, ValidPropertiesBox<RegistrationPersonBag> registrationPerson, bool saveEmptyValues )
        {
            if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.PhoneNumber ) ) )
            {
                return;
            }

            if ( ( registrationPerson.Bag.PhoneNumber?.Number ).IsNullOrWhiteSpace() && !saveEmptyValues )
            {
                return;
            }

            var isMessagingEnabled = registrationPerson.Bag.PhoneNumber?.IsMessagingEnabled;

            if ( !_template.IsSmsButtonVisible )
            {
                isMessagingEnabled = GroupTypeDefaultSmsEnabled.Value;
            }

            person.UpdatePhoneNumber( PhoneTypeMobileId.Value,
                registrationPerson.Bag.PhoneNumber?.CountryCode,
                registrationPerson.Bag.PhoneNumber?.Number,
                isMessagingEnabled,
                false,
                _rockContext );
        }

        /// <summary>
        /// Updates the person's attribute values based on the registration
        /// details and the check-in template configuration. If
        /// <paramref name="saveEmptyValues"/> is <c>false</c> then any blank
        /// attribute values will be ignored.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose attribute values will be updated.</param>
        /// <param name="registrationPerson">The registration details describing the values to be updated.</param>
        /// <param name="saveEmptyValues"><c>true</c> if empty values should be written; otherwise <c>false</c>.</param>
        internal void UpdatePersonAttributeValues( Person person, ValidPropertiesBox<RegistrationPersonBag> registrationPerson, bool saveEmptyValues )
        {
            if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.AttributeValues ) ) )
            {
                return;
            }

            if ( registrationPerson.Bag.AttributeValues == null )
            {
                return;
            }

            List<Guid> attributeGuids;

            if ( registrationPerson.Bag.IsAdult )
            {
                attributeGuids = _template.OptionalAttributeGuidsForAdults
                    .Union( _template.RequiredAttributeGuidsForAdults )
                    .ToList();
            }
            else
            {
                attributeGuids = _template.OptionalAttributeGuidsForChildren
                    .Union( _template.RequiredAttributeGuidsForChildren )
                    .ToList();
            }

            var attributes = AttributeCache.GetMany( attributeGuids, _rockContext );

            if ( person.Attributes == null )
            {
                person.LoadAttributes( _rockContext );
            }

            foreach ( var attributeValue in registrationPerson.Bag.AttributeValues )
            {
                // Make sure we only update attributes that are configured to
                // be available.
                if ( !attributes.Any( a => a.Key == attributeValue.Key ) )
                {
                    continue;
                }

                if ( attributeValue.Value.IsNotNullOrWhiteSpace() || saveEmptyValues )
                {
                    person.SetPublicAttributeValue( attributeValue.Key, attributeValue.Value, _currentPerson, false );
                }
            }

            person.SaveAttributeValues( _rockContext );
        }

        /// <summary>
        /// Updates the family's attribute values based on the registration
        /// details and the check-in template configuration.
        /// </summary>
        /// <param name="family">The <see cref="Group"/> whose attribute values will be updated.</param>
        /// <param name="registrationFamily">The registration details describing the values to be updated.</param>
        internal void UpdateFamilyAttributeValues( Group family, ValidPropertiesBox<RegistrationFamilyBag> registrationFamily )
        {
            if ( !registrationFamily.IsValidProperty( nameof( registrationFamily.Bag.AttributeValues ) ) )
            {
                return;
            }

            if ( registrationFamily.Bag.AttributeValues == null )
            {
                return;
            }

            var attributeGuids = _template.OptionalAttributeGuidsForFamilies
                .Union( _template.RequiredAttributeGuidsForFamilies )
                .ToList();

            var attributes = AttributeCache.GetMany( attributeGuids, _rockContext );

            if ( family.Attributes == null )
            {
                family.LoadAttributes( _rockContext );
            }

            foreach ( var attributeValue in registrationFamily.Bag.AttributeValues )
            {
                // Make sure we only update attributes that are configured to
                // be available.
                if ( !attributes.Any( a => a.Key == attributeValue.Key ) )
                {
                    continue;
                }

                family.SetPublicAttributeValue( attributeValue.Key, attributeValue.Value, _currentPerson, false );
            }

            family.SaveAttributeValues( _rockContext );
        }

        /// <summary>
        /// Ensure that people who have a <see cref="RegistrationPersonBag.RegistrationPersonBag"/>
        /// value that matches one of the "in same family relationships" values
        /// in the check-in configuration template also are members of the
        /// family group.
        /// </summary>
        /// <param name="family">The family group.</param>
        /// <param name="people">The list of all people, this will be further filtered to only those in the same family.</param>
        internal void EnsurePeopleInPrimaryFamilyAreMembersOfGroup( Group family, List<(ValidPropertiesBox<RegistrationPersonBag> RegistrationPerson, Person Person)> people )
        {
            var groupMemberService = new GroupMemberService( _rockContext );
            var familyRelationshipGuids = _template.SameFamilyKnownRelationshipRoleGuids;

            var peopleInPrimaryFamily = people
                .Where( p => p.RegistrationPerson.Bag.RelationshipToAdult == null
                    || familyRelationshipGuids.Contains( p.RegistrationPerson.Bag.RelationshipToAdult.Value.AsGuid() ) );

            // Ensure that every person who is listed in the UI as being in the
            // same family is a member of the family group.
            foreach ( var individual in peopleInPrimaryFamily )
            {
                var currentFamilyMember = family.Members.FirstOrDefault( m => m.PersonId == individual.Person.Id );

                if ( currentFamilyMember == null )
                {
                    currentFamilyMember = new GroupMember
                    {
                        GroupId = family.Id,
                        PersonId = individual.Person.Id,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };

                    if ( individual.RegistrationPerson.Bag.IsAdult )
                    {
                        currentFamilyMember.GroupRoleId = GroupTypeRoleAdultId.Value;
                    }
                    else
                    {
                        currentFamilyMember.GroupRoleId = GroupTypeRoleChildId.Value;
                    }

                    groupMemberService.Add( currentFamilyMember );

                    _rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// <para>
        /// Ensures that people who should not be members of the primary family
        /// each have a family they are a member of. If multiple people are
        /// being created that share a last name then the same new family group
        /// will be used for those people.
        /// </para>
        /// <para>
        /// This also creates any known relationships that are specified in the
        /// registration details based on the <see cref="RegistrationPersonBag.RelationshipToAdult"/>
        /// property value.
        /// </para>
        /// </summary>
        /// <param name="people">The people being registered in the kiosk.</param>
        /// <param name="defaultCampusId">The campus to set on any new families being created.</param>
        /// <param name="saveResult">Will be updated with any new <see cref="Group"/> objects that were created.</param>
        internal void EnsurePeopleNotInPrimaryFamilyHaveAFamily( List<(ValidPropertiesBox<RegistrationPersonBag> RegistrationPerson, Person Person)> people, int? defaultCampusId, FamilyRegistrationSaveResult saveResult )
        {
            var familyRelationshipGuids = _template.SameFamilyKnownRelationshipRoleGuids;
            var groupService = new GroupService( _rockContext );
            var groupMemberService = new GroupMemberService( _rockContext );

            var adultsInPrimaryFamily = people
                .Where( p => p.RegistrationPerson.Bag.IsAdult )
                .Select( p => p.Person )
                .ToList();

            var peopleNotInPrimaryFamily = people
                .Where( p => !familyRelationshipGuids.Contains( ( p.RegistrationPerson.Bag.RelationshipToAdult?.Value ).AsGuid() ) );

            // Make a dictionary of new related families (by lastname) so
            // we can combine any new related children into a family with
            // the same last name.
            var newRelatedFamilies = new Dictionary<string, Group>( StringComparer.OrdinalIgnoreCase );

            // loop thru all people that are NOT part of the same family
            foreach ( var individual in peopleNotInPrimaryFamily )
            {
                if ( !individual.Person.PrimaryFamilyId.HasValue )
                {
                    // The person does not have a family yet, so we need to
                    // assign one. First try any we have already created.
                    var relatedFamily = newRelatedFamilies.GetValueOrNull( individual.Person.LastName );

                    if ( relatedFamily == null )
                    {
                        relatedFamily = new Group
                        {
                            Name = individual.Person.LastName + " Family",
                            GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id,
                            CampusId = defaultCampusId
                        };

                        newRelatedFamilies.Add( individual.Person.LastName, relatedFamily );
                        groupService.Add( relatedFamily );
                        saveResult.NewFamilyList.Add( relatedFamily );

                        _rockContext.SaveChanges();
                    }

                    var familyMember = new GroupMember
                    {
                        GroupId = relatedFamily.Id,
                        PersonId = individual.Person.Id,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };

                    if ( individual.RegistrationPerson.Bag.IsAdult )
                    {
                        familyMember.GroupRoleId = GroupTypeRoleAdultId.Value;
                    }
                    else
                    {
                        familyMember.GroupRoleId = GroupTypeRoleChildId.Value;
                    }

                    groupMemberService.Add( familyMember );

                    _rockContext.SaveChanges();
                }

                var relationshipToAdultGuid = individual.RegistrationPerson.Bag.RelationshipToAdult?.Value.AsGuid();
                var relationshipToAdultId = relationshipToAdultGuid.HasValue
                    ? GroupTypeRoleCache.Get( relationshipToAdultGuid.Value )?.Id
                    : null;

                if ( relationshipToAdultId.HasValue )
                {
                    // Ensure there are known relationships between each adult
                    // in the primary family to this person that isn't in the
                    // primary family.
                    foreach ( var primaryFamilyAdult in adultsInPrimaryFamily )
                    {
                        /*
                         * Daniel Hazelbaker - 9/6/2024
                         * 
                         * This logic was discussed during PO review. The point
                         * of discussion was the fact that we only create the
                         * selected known relationship if the person is NOT in
                         * the same family.
                         * 
                         * There were good arguments on both sides for also
                         * creating the known relationship when the child is in
                         * the same family and also when not. For example, a
                         * known relationship of "Step Child" would be good to
                         * know. But a counter argument is that "Step Child" is
                         * only correct for one parent/adult.
                         * 
                         * For now, we decided to leave the logic as-is to match
                         * the current v1 functionality.
                         */

                        groupMemberService.CreateKnownRelationship( primaryFamilyAdult.Id, individual.Person.Id, relationshipToAdultId.Value );

                        var canCheckIn = _template.CanCheckInKnownRelationshipRoleGuids.Contains( relationshipToAdultGuid.Value );

                        // If this is something other than the CanCheckIn
                        // relationship, but is a relationship that is
                        // considered a can-check-in relationship, then also
                        // ensure we have a can-check-in relationship.
                        if ( canCheckIn && GroupTypeRoleCanCheckInId.Value != relationshipToAdultId.Value )
                        {
                            groupMemberService.CreateKnownRelationship( primaryFamilyAdult.Id, individual.Person.Id, GroupTypeRoleCanCheckInId.Value );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes any people from the family that have been removed in the UI.
        /// If the person is an actual member of the family then they are removed
        /// from the family and placed into a new family (unless they were
        /// already a member of more than 1 family). If the person is only known
        /// by relationship then those relationships are removed.
        /// </summary>
        /// <param name="primaryFamily">The primary family being edited.</param>
        /// <param name="removedPersonIdKeys">The IdKey values of the people to be removed.</param>
        /// <param name="saveResult">Will be updated with any new <see cref="Group"/> objects that were created.</param>
        internal void RemoveFamilyMembers( Group primaryFamily, List<string> removedPersonIdKeys, FamilyRegistrationSaveResult saveResult )
        {
            var groupService = new GroupService( _rockContext );
            var groupMemberService = new GroupMemberService( _rockContext );
            var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), _rockContext );
            var removedPersonIds = removedPersonIdKeys
                .Select( idKey => IdHasher.Instance.GetId( idKey ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            if ( !removedPersonIds.Any() )
            {
                return;
            }

            // Make a list of all (removed) person ids that exist in other families.
            var otherGroupPersonIds = groupMemberService.Queryable()
                .Where( m => removedPersonIds.Contains( m.PersonId )
                    && m.Group.GroupTypeId == familyGroupType.Id
                    && m.GroupId != primaryFamily.Id )
                .Select( m => m.PersonId )
                .ToList();

            var primaryFamilyGroupMembers = groupMemberService.Queryable()
                .Where( m => m.GroupId == primaryFamily.Id
                    && removedPersonIds.Contains( m.PersonId ) )
                .ToList();

            foreach ( var removedPersonId in removedPersonIds )
            {
                // Remove all valid "can check-in" relationships to anybody
                // in the primary family.
                RemoveCanCheckInRelationships( removedPersonId, primaryFamily.Id );

                // If this person is not in the primary family then we are done.
                var primaryFamilyGroupMember = primaryFamilyGroupMembers.FirstOrDefault( m => m.PersonId == removedPersonId );

                if ( primaryFamilyGroupMember == null )
                {
                    continue;
                }

                // This person is in the primary family. If they also are a
                // member of another family group then we can just remove them
                // from this one.
                if ( otherGroupPersonIds.Contains( removedPersonId ) )
                {
                    groupMemberService.Delete( primaryFamilyGroupMember );
                    _rockContext.SaveChanges();

                    return;
                }

                // They are not a member of another family, so we need to create
                // a family for them.
                MovePersonToNewFamily( removedPersonId, primaryFamilyGroupMember, primaryFamily, saveResult );
            }
        }

        /// <summary>
        /// Removes any known "can check-in" relationships between this person
        /// and all members of the specified family.
        /// </summary>
        /// <param name="personId">The person that can no longer be checked in by the family.</param>
        /// <param name="familyId">The family that will have all relationships to <paramref name="personId"/> removed.</param>
        private void RemoveCanCheckInRelationships( int personId, int familyId )
        {
            var groupMemberService = new GroupMemberService( _rockContext );
            var ownerRoleId = GroupTypeRoleCache.Get( SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid(), _rockContext ).Id;
            var canCheckInRoleIds = GroupTypeRoleCache.GetMany( _template.CanCheckInKnownRelationshipRoleGuids.ToList(), _rockContext )
                .Select( r => r.Id )
                .ToList();

            var primaryFamilyMemberPersonIdQry = groupMemberService.Queryable()
                .Where( m => m.GroupId == familyId )
                .Select( m => m.PersonId );

            // Get the Known Relationship group ids for each member of the family.
            var relationshipGroupIdQry = groupMemberService.Queryable()
                .Where( g => g.GroupRoleId == ownerRoleId
                    && primaryFamilyMemberPersonIdQry.Contains( g.PersonId ) )
                .Select( g => g.GroupId );

            // Get all member records this person has in those groups that
            // has a role flagged as "can check-in".
            var relationshipsToDelete = groupMemberService.Queryable()
                .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active
                    && relationshipGroupIdQry.Contains( gm.GroupId )
                    && gm.PersonId == personId
                    && canCheckInRoleIds.Contains( gm.GroupRoleId ) )
                .ToList();

            groupMemberService.DeleteRange( relationshipsToDelete );
            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates a new family and moves the person to this new family.
        /// </summary>
        /// <param name="personId">The identifier of the person to be moved.</param>
        /// <param name="oldFamilyGroupMember">The <see cref="GroupMember"/> that represents this person in <paramref name="oldFamily"/>. It will be updated to point to the new family.</param>
        /// <param name="oldFamily">The old family that this person should be moved out of.</param>
        /// <param name="saveResult">Will be updated with any new <see cref="Group"/> objects that were created.</param>
        private void MovePersonToNewFamily( int personId, GroupMember oldFamilyGroupMember, Group oldFamily, FamilyRegistrationSaveResult saveResult )
        {
            var personService = new PersonService( _rockContext );
            var groupService = new GroupService( _rockContext );
            var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), _rockContext );
            var person = personService.Get( personId );

            var newFamily = new Group
            {
                Name = $"{person.LastName} {familyGroupType.Name}",
                GroupTypeId = familyGroupType.Id,
                CampusId = oldFamily.CampusId
            };

            groupService.Add( newFamily );
            _rockContext.SaveChanges();

            saveResult.NewFamilyList.Add( newFamily );

            // Update the existing GroupMember to have the new group details.
            oldFamilyGroupMember.GroupId = newFamily.Id;
            oldFamilyGroupMember.Group = newFamily;

            // If person's previous giving group was this family, set it
            // to their new family id.
            if ( person.GivingGroupId == oldFamily.Id )
            {
                person.GivingGroupId = newFamily.Id;
            }

            // If this person is 18 or older, create them as an Adult
            // in their new family.
            if ( ( person.Age ?? 0 ) >= 18 )
            {
                oldFamilyGroupMember.GroupRoleId = GroupTypeRoleCache
                    .Get( SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid(), _rockContext )
                    .Id;
            }

            _rockContext.SaveChanges();
        }

        #endregion
    }
}

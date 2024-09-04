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
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData.Crm
{
    /// <summary>
    /// Provides actions to manage data for people.
    /// </summary>
    public class PersonDataManager
    {
        private static Lazy<PersonDataManager> _dataManager = new Lazy<PersonDataManager>();
        public static PersonDataManager Instance => _dataManager.Value;

        private static int? _FamilyGroupRoleAdultId;

        public PersonDataManager()
        {
            if ( _FamilyGroupRoleAdultId == null )
            {
                _FamilyGroupRoleAdultId = GroupTypeCache.GetFamilyGroupType().Roles
                    .First( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    .Id;
            }
        }

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class PersonInfo
        {
            public string ForeignKey { get; set; }
            public string FirstName { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public string PersonGuid { get; set; }
            public string Email { get; set; }
            public string RecordTypeIdentifier { get; set; }
            public string RecordStatusIdentifier { get; set; }
            public string GenderIdentifier { get; set; }
            public string DateOfBirth { get; set; }

            /// <summary>
            /// The identifier of the primary Family Group of which this person is a member.
            /// </summary>
            public string PrimaryFamilyGroupIdentifier { get; set; }
        }

        #region Add Person

        public class ActionResult
        {
            #region Factory methods

            public static ActionResult Success( string message = null )
            {
                var result = new ActionResult
                {
                    IsSuccess = true,
                    Messages = new List<string> { message }
                };
                return result;
            }
            public static ActionResult Failure( string message = null )
            {
                var result = new ActionResult
                {
                    IsSuccess = false,
                    Messages = new List<string> { message }
                };
                return result;
            }

            #endregion

            public bool IsSuccess { get; set; }
            public List<string> Messages { get; set; }
        }

        public class AddNewPersonActionArgs
        {
            public PersonInfo PersonInfo { get; set; } = new PersonInfo();
            public bool ReplaceIfExists { get; set; }
        }

        public class AddPersonToFamilyActionArgs
        {
            public int PersonIdentifier { get; set; }
            public int FamilyIdentifier { get; set; }
            public string FamilyGroupRoleIdentifier { get; set; }
        }

        public class AddNewPersonToFamilyActionArgs
        {
            public PersonInfo PersonInfo { get; set; } = new PersonInfo();

            public string FamilyIdentifier { get; set; }
            public string FamilyGroupRoleIdentifier { get; set; }
        }

        public class AddPersonActionResult : ActionResult
        {
            public int? PersonId;
            public int? FamilyGroupId;

            public static AddPersonActionResult ForSuccess( int personId, int familyGroupId )
            {
                var result = new AddPersonActionResult { IsSuccess = true, PersonId = personId, FamilyGroupId = familyGroupId };
                return result;
            }

            public static AddPersonActionResult ForFailure()
            {
                var result = new AddPersonActionResult { IsSuccess = false };
                return result;
            }
        }

        public AddPersonActionResult AddNewPersonToNewFamily( AddNewPersonActionArgs args )
        {
            var result = AddPersonToFamilyInternal( args.PersonInfo, args.ReplaceIfExists, null, createNewFamily: true, null );
            return result;
        }

        public AddPersonActionResult AddNewPersonToExistingFamily( AddNewPersonToFamilyActionArgs args )
        {
            var familyId = args.FamilyIdentifier.AsIntegerOrNull();
            var familyGroupRoleGuid = args.FamilyGroupRoleIdentifier.AsGuidOrNull();

            var familyGroupRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                .Where( r => r.Guid == familyGroupRoleGuid )
                .Select( r => r.Id )
                .FirstOrDefault();

            var result = AddPersonToFamilyInternal( args.PersonInfo, replaceExistingPerson: false, familyId, createNewFamily: false, familyGroupRoleId );
            return result;
        }

        /// <summary>
        /// Add a new Person.
        /// A Person is a Group with a specific Group Type of "Person".
        /// A Group of any other Group Type can also be nominated as a Person,
        /// but it should be managed using the GroupDataManager.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public AddPersonActionResult AddPersonToFamilyInternal( PersonInfo personInfo, bool replaceExistingPerson, int? familyGroupId, bool createNewFamily, int? familyGroupRoleId )
        {
            var result = new AddPersonActionResult();

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            Rock.Model.Person person = null;
            Rock.Model.Group familyGroup = null;

            rockContext.WrapTransaction( () =>
            {
                var personGuid = personInfo.PersonGuid.AsGuidOrNull();
                if ( personGuid != null )
                {
                    var existingPerson = personService.Queryable().FirstOrDefault( g => g.Guid == personGuid );
                    if ( existingPerson != null )
                    {
                        if ( !replaceExistingPerson )
                        {
                            return;
                        }
                        DeletePerson( personInfo.PersonGuid );
                        rockContext.SaveChanges();
                    }
                }

                person = new Rock.Model.Person();
                person.IsSystem = false;

                UpdatePersonEntityFromPersonInfo( rockContext, person, personInfo );

                // Update the AttributeCache prior to creating the new Person.
                // If not, a deadlock may occur.
                AttributeCache.All( rockContext );

                if ( createNewFamily )
                {
                    familyGroup = PersonService.SaveNewPerson( person, rockContext, null, false );

                    familyGroupId = familyGroup.Id;
                }
                else
                {
                    if ( familyGroupId == null )
                    {
                        throw new Exception( "AddPersonToFamily failed. The FamilyGroupIdentifier for an existing Family Group must be specified." );
                    }

                    if ( familyGroupRoleId == null || familyGroupRoleId == 0 )
                    {
                        familyGroupRoleId = _FamilyGroupRoleAdultId.Value;
                    }

                    PersonService.AddPersonToFamily( person, true, familyGroupId.Value, familyGroupRoleId.Value, rockContext );
                }


                rockContext.SaveChanges();
            } );

            return AddPersonActionResult.ForSuccess( person.Id, familyGroupId.GetValueOrDefault() );
        }

        #endregion

        #region Update Person

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class UpdatePersonActionArgs : PersonInfo
        {
            public string UpdateTargetIdentifier { get; set; }
        }

        /// <summary>
        /// Update an existing Person.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void UpdatePerson( RockContext rockContext, UpdatePersonActionArgs args )
        {
            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var person = personService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                UpdatePersonEntityFromPersonInfo( rockContext, person, args );

                rockContext.SaveChanges();
            } );
        }

        public class UpdateEntityAttributeValueActionArgs : UpdateEntityActionArgsBase<EntityAttributeValueInfo>
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
        public class EntityAttributeValueInfo
        {
            public string AttributeKey { get; set; }

            public string AttributeValue { get; set; }
        }

        public void SetPersonAttribute( UpdateEntityAttributeValueActionArgs args )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

            person.LoadAttributes();
            person.SetAttributeValue( args.Key, args.Value );

            person.SaveAttributeValues( rockContext );
        }

        public class AddPersonAttributeActionArgs
        {
            public Guid? Guid { get; set; }
            public string EntityTypeIdentifier { get; set; }
            public string Key { get; set; }
            public string Name { get; set; }
            public string UpdateTargetIdentifier { get; set; }
            public string FieldTypeIdentifier { get; set; }
        }

        public void AddPersonAttribute( AddPersonAttributeActionArgs args )
        {
            var rockContext = new RockContext();

            var argsAdd = new TestDataHelper.Core.AddEntityAttributeArgs
            {
                Key = args.Key,
                EntityTypeIdentifier = EntityTypeCache.GetId( typeof( Rock.Model.Person ) ).ToString(),
                FieldTypeIdentifier = args.FieldTypeIdentifier,
                Guid = args.Guid,
                Name = args.Name
            };

            TestDataHelper.Core.AddEntityAttribute( argsAdd, rockContext );

            rockContext.SaveChanges();
        }

        
        public class PersonPreviousNameInfo
        {
            public string PreviousLastName { get; set; }
        }

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class UpdatePersonAddPreviousNameActionArgs : UpdateEntityActionArgsBase<PersonPreviousNameInfo>
        {
        }

        /// <summary>
        /// Add a previous Last Name for the specified person.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="args"></param>
        public ActionResult UpdatePersonAddPreviousName( UpdatePersonAddPreviousNameActionArgs args )
        {
            var rockContext = new RockContext();
            return UpdatePersonAddPreviousName( rockContext, args );
        }

        /// <summary>
        /// Add a previous Last Name for the specified person.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="args"></param>
        public ActionResult UpdatePersonAddPreviousName( RockContext rockContext, UpdatePersonAddPreviousNameActionArgs args )
        {
            ActionResult result = null;

            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var person = personService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                var lastName = args.Properties.PreviousLastName;
                var personAliasId = person.PrimaryAliasId.GetValueOrDefault();

                var previousNameService = new PersonPreviousNameService( rockContext );

                var previousNameExists = previousNameService.Queryable()
                    .Any( p => p.PersonAliasId == personAliasId && p.LastName == lastName );

                if ( previousNameExists )
                {
                    result = ActionResult.Success( "Previous Name exists." );
                    return;
                }

                var previousName = new PersonPreviousName()
                {
                    LastName = args.Properties.PreviousLastName,
                    PersonAliasId = person.PrimaryAliasId.GetValueOrDefault()
                };

                previousNameService.Add( previousName );

                rockContext.SaveChanges();

                result = ActionResult.Success( "Previous Name added." );
            } );

            return result;
        }

        private void UpdatePersonEntityFromPersonInfo( RockContext rockContext, Rock.Model.Person person, PersonInfo args )
        {
            // Set Guid.
            if ( args.PersonGuid.IsNotNullOrWhiteSpace() )
            {
                person.Guid = InputParser.ParseToGuidOrThrow( nameof( PersonInfo.PersonGuid ), args.PersonGuid );
            }

            // Set FirstName.
            if ( args.FirstName.IsNotNullOrWhiteSpace() )
            {
                person.FirstName = args.FirstName.Trim().FixCase();
                ;
            }

            // Set NickName.
            if ( args.NickName.IsNotNullOrWhiteSpace() )
            {
                person.NickName = args.NickName.Trim().FixCase();
            }

            // Set LastName.
            if ( args.LastName.IsNotNullOrWhiteSpace() )
            {
                person.LastName = args.LastName.Trim().FixCase();
                ;
            }

            // Set ForeignKey.
            if ( args.ForeignKey.IsNotNullOrWhiteSpace() )
            {
                person.ForeignKey = args.ForeignKey;
            }

            // Set Record Type.
            if ( args.RecordTypeIdentifier.IsNotNullOrWhiteSpace() )
            {
                person.RecordTypeValueId = DefinedValueCache.GetId( args.RecordTypeIdentifier.AsGuid() );
            }

            // Set Record Status.
            if ( args.RecordTypeIdentifier.IsNotNullOrWhiteSpace() )
            {
                person.RecordStatusValueId = DefinedValueCache.GetId( args.RecordStatusIdentifier.AsGuid() );
            }

            // Set Email.
            if ( args.Email.IsNotNullOrWhiteSpace() )
            {
                person.Email = args.Email;

                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
            }

            // Set Gender.
            if ( args.GenderIdentifier.IsNotNullOrWhiteSpace() )
            {
                person.Gender = args.GenderIdentifier.ConvertToEnumOrNull<Gender>( Gender.Unknown ).Value;
            }

            // Set Date of Birth.
            var dateOfBirth = args.DateOfBirth.AsDateTime();
            if ( dateOfBirth.HasValue )
            {
                person.SetBirthDate( dateOfBirth.Value );
            }

            rockContext.SaveChanges();
        }

        #endregion

        #region Delete Person

        /// <summary>
        /// Delete a Person.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool DeletePerson( string identifier )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var person = personService.Get( identifier );

            personService.ExpungePerson( person.Id );

            return true;
        }

        #endregion
    }
}


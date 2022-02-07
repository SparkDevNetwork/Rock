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
using System.Data.Entity.SqlServer;
using System.Linq;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public static partial class PersonExtensionMethods
    {

        /// <summary>
        /// Gets the families sorted by the person's GroupOrder (GroupMember.GroupOrder)
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<Group> GetFamilies( this Person person, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new PersonService( rockContext ).GetFamilies( person != null ? person.Id : 0 );
        }

        /// <summary>
        /// Gets the family for the person. If multiple families the first family based on the GroupOrder value of the GroupMember
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Group GetFamily( this Person person, RockContext rockContext = null )
        {
            // If PrimaryFamily has been calculated, use that. Otherwise, get it from GetFamilies()
            if ( person.PrimaryFamily != null )
            {
                return person.PrimaryFamily;
            }
            else
            {
                return person.GetFamilies( rockContext ).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the home location.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Location GetHomeLocation( this Person person, RockContext rockContext = null )
        {
            Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
            if ( homeAddressGuid.HasValue )
            {
                var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                if ( homeAddressDv != null )
                {
                    foreach ( var family in person.GetFamilies( rockContext ) )
                    {
                        var loc = family.GroupLocations
                            .Where( l =>
                                l.GroupLocationTypeValueId == homeAddressDv.Id &&
                                l.IsMappedLocation )
                            .Select( l => l.Location )
                            .FirstOrDefault();
                        if ( loc != null )
                        {
                            return loc;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the most ideal mailing location from among this person's families
        /// </summary>
        /// <param name="person">The person to find a mailing address for</param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static Location GetMailingLocation( this Person person, RockContext rockContext = null )
        {
            // Get the mailing address from this person's giving group if there is one
            if ( person.GivingGroup != null )
            {
                var mailingLocation = person.GivingGroup.GetBestMailingLocation();
                if ( mailingLocation != null )
                {
                    return mailingLocation;
                }
            }

            return person.GetFamilies( rockContext ).GetBestMailingLocation();
        }

        /// <summary>
        /// Returns the most ideal mailing location from a single family
        /// </summary>
        /// <param name="group">The family to find addresses on</param>
        /// <returns></returns>
        private static Location GetBestMailingLocation( this Group group )
        {
            return GetBestMailingLocation( new List<Group> { group } );
        }

        /// <summary>
        /// Returns the most ideal mailing location from among the selected family groups
        /// </summary>
        /// <param name="groups">The families to find addresses on</param>
        /// <returns></returns>
        private static Location GetBestMailingLocation( this IEnumerable<Group> groups )
        {
            if ( groups.Any() )
            {
                var homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                var workAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuidOrNull();
                if ( homeAddressGuid.HasValue && workAddressGuid.HasValue )
                {
                    var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                    var workAddressDv = DefinedValueCache.Get( workAddressGuid.Value );
                    if ( homeAddressDv != null && workAddressDv != null )
                    {
                        // Get all available mailing locations, prioritizing mapped locations then home locations
                        var mailingLocations = groups.SelectMany( x => x.GroupLocations )
                            .Where( l => l.IsMailingLocation )
                            .Where( l => l.GroupLocationTypeValueId == homeAddressDv.Id || l.GroupLocationTypeValueId == workAddressDv.Id )
                            .OrderBy( l => l.IsMappedLocation ? 0 : 1 )
                            .ThenBy( l => l.GroupLocationTypeValueId == homeAddressDv.Id ? 0 : 1 );

                        return mailingLocations.Select( l => l.Location ).FirstOrDefault();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Updates, adds or removes a PhoneNumber of the given type. (It doesn't save changes to the database ).
        /// </summary>
        public static void UpdatePhoneNumber( this Person person, int numberTypeValueId, string phoneCountryCode, string phoneNumber, bool? isMessagingEnabled, bool? isUnlisted, RockContext rockContext )
        {
            // try to find the phone number based on the typeGuid.
            var phoneObject = person.PhoneNumbers
                .Where( p =>
                    p.NumberTypeValueId.HasValue &&
                    p.NumberTypeValueId.Value == numberTypeValueId )
                .FirstOrDefault();

            // Since only one number can be used for SMS, before anything else, if isMessagingEnabled is true, turn it off on ALL
            // numbers, so we only enable it for this one.
            if ( isMessagingEnabled.HasValue && isMessagingEnabled.Value == true )
            {
                foreach ( PhoneNumber currPhoneNumber in person.PhoneNumbers )
                {
                    currPhoneNumber.IsMessagingEnabled = false;
                }
            }

            // do they currently have this type of number?
            if ( phoneObject != null )
            {
                // if the text field is blank, we'll delete this phone number type from their list.
                if ( string.IsNullOrWhiteSpace( phoneNumber ) )
                {
                    person.PhoneNumbers.Remove( phoneObject );

                    var phoneNumberService = new PhoneNumberService( rockContext );
                    phoneNumberService.Delete( phoneObject );
                }
                else
                {
                    // otherwise update it with the new info
                    phoneObject.CountryCode = PhoneNumber.CleanNumber( phoneCountryCode );
                    phoneObject.Number = PhoneNumber.CleanNumber( phoneNumber );

                    // for an existing number, if they don't provide messaging / unlisted, use the current values.
                    phoneObject.IsMessagingEnabled = isMessagingEnabled ?? phoneObject.IsMessagingEnabled;
                    phoneObject.IsUnlisted = isUnlisted ?? phoneObject.IsUnlisted;
                }
            }
            else if ( !string.IsNullOrWhiteSpace( phoneNumber ) )
            {
                // they don't have a number of this type. If one is being added, we'll add it.
                // (otherwise we'll just do nothing, leaving it as it)

                // create a new phone number and add it to their list.
                phoneObject = new PhoneNumber();
                person.PhoneNumbers.Add( phoneObject );

                var phoneNumberService = new PhoneNumberService( rockContext );
                phoneNumberService.Add( phoneObject );

                // get the typeId for this phone number so we set it correctly
                // var numberType = DefinedValueCache.Get( phoneTypeGuid );
                phoneObject.NumberTypeValueId = numberTypeValueId;

                phoneObject.CountryCode = PhoneNumber.CleanNumber( phoneCountryCode );
                phoneObject.Number = PhoneNumber.CleanNumber( phoneNumber );

                // for a new number, if they don't specify messaging / unlisted, assume no texting and not unlisted.
                phoneObject.IsMessagingEnabled = isMessagingEnabled ?? false;
                phoneObject.IsUnlisted = isUnlisted ?? false;
            }
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Person" /> entities containing the Person's family.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person" /> to retrieve family members for.</param>
        /// <param name="includeSelf">A <see cref="System.Boolean" /> value that is <c>true</c> if the provided person should be returned in the results, otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Model.Person" /> entities representing the provided Person's family.
        /// </returns>
        public static IQueryable<GroupMember> GetFamilyMembers( this Person person, bool includeSelf = false, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetFamilyMembers( person != null ? person.Id : 0, includeSelf );
        }

        /// <summary>
        /// Gets the group members.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> GetGroupMembers( this Person person, int groupTypeId, bool includeSelf = false, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetGroupMembers( groupTypeId, person != null ? person.Id : 0, includeSelf );
        }

        /// <summary>
        /// Gets any previous last names for this person sorted alphabetically by LastName
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IOrderedQueryable<PersonPreviousName> GetPreviousNames( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetPreviousNames( person != null ? person.Id : 0 );
        }

        /// <summary>
        /// Gets any search keys for this person
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<PersonSearchKey> GetPersonSearchKeys( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetPersonSearchKeys( person != null ? person.Id : 0 );
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person" /> entity of the provided Person's spouse.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the spouse of.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.
        /// </returns>
        public static Person GetSpouse( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetSpouse( person );
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person" /> entity of the provided Person's head of household.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the head of household of.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's head of household. If the provided Person's head of household is not found, this value will be null.
        /// </returns>
        public static Person GetHeadOfHousehold( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetHeadOfHousehold( person );
        }

        /// <summary>
        /// Gets the family role (adult or child).
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GroupTypeRole GetFamilyRole( this Person person, RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            return new PersonService( rockContext ).GetFamilyRole( person, rockContext );
        }

        /// <summary>
        /// Gets a Person's spouse with a selector that lets you only fetch the properties that you need
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the spouse of.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.
        /// </returns>
        public static TResult GetSpouse<TResult>( this Person person, System.Linq.Expressions.Expression<Func<GroupMember, TResult>> selector, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetSpouse( person, selector );
        }

        /// <summary>
        /// Gets the businesses.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<Person> GetBusinesses( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetBusinesses( person.Id );
        }

        /// <summary>
        /// limits the PersonQry to people that have an Age that is between MinAge and MaxAge (inclusive)
        /// </summary>
        /// <param name="personQry">The person qry.</param>
        /// <param name="minAge">The minimum age.</param>
        /// <param name="maxAge">The maximum age.</param>
        /// <param name="includePeopleWithNoAge">if set to <c>true</c> [include people with no age].</param>
        /// <returns></returns>
        public static IQueryable<Person> WhereAgeRange( this IQueryable<Person> personQry, int? minAge, int? maxAge, bool includePeopleWithNoAge = true )
        {
            var currentDate = RockDateTime.Today;
            var qryWithAge = personQry.Select(
                      p => new
                      {
                          Person = p,
                          Age = p.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ), currentDate )
                            ? SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) - 1
                            : SqlFunctions.DateDiff( "year", p.BirthDate, currentDate )
                      } );

            if ( includePeopleWithNoAge )
            {
                if ( minAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => !a.Age.HasValue || a.Age >= minAge );
                }

                if ( maxAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => !a.Age.HasValue || a.Age <= maxAge );
                }
            }
            else
            {
                if ( minAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => a.Age.HasValue && a.Age >= minAge );
                }

                if ( maxAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => a.Age.HasValue && a.Age <= maxAge );
                }
            }

            return qryWithAge.Select( a => a.Person );
        }

        /// <summary>
        /// Limits the PersonQry to people that have an Grade Offset that is between MinGradeOffset and MaxGradeOffset (inclusive)
        /// </summary>
        /// <param name="personQry">The person qry.</param>
        /// <param name="minGradeOffset">The minimum grade offset.</param>
        /// <param name="maxGradeOffset">The maximum grade offset.</param>
        /// <param name="includePeopleWithNoGrade">if set to <c>true</c> [include people with no Grade].</param>
        /// <returns></returns>
        public static IQueryable<Person> WhereGradeOffsetRange( this IQueryable<Person> personQry, int? minGradeOffset, int? maxGradeOffset, bool includePeopleWithNoGrade = true )
        {
            var currentGradYear = PersonService.GetCurrentGraduationYear();

            var qryWithGradeOffset = personQry.Select(
                      p => new
                      {
                          Person = p,
                          GradeOffset = p.GraduationYear.HasValue ? p.GraduationYear.Value - currentGradYear : ( int? ) null
                      } );

            if ( includePeopleWithNoGrade )
            {
                if ( minGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => !a.GradeOffset.HasValue || a.GradeOffset >= minGradeOffset );
                }

                if ( maxGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => !a.GradeOffset.HasValue || a.GradeOffset <= maxGradeOffset );
                }
            }
            else
            {
                if ( minGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => a.GradeOffset.HasValue && a.GradeOffset >= minGradeOffset );
                }

                if ( maxGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => a.GradeOffset.HasValue && a.GradeOffset <= maxGradeOffset );
                }
            }

            return qryWithGradeOffset.Select( a => a.Person );
        }

        /// <summary>
        /// Calculates whether the person is in an active merge request.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if the person is part of merge request; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPartOfMergeRequest( this Person person, RockContext rockContext = null )
        {
            return GetMergeRequestQuery( person, rockContext )?.Any() ?? false;
        }

        /// <summary>
        /// Gets the merge request.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntitySet GetMergeRequest( this Person person, RockContext rockContext = null )
        {
            return GetMergeRequestQuery( person, rockContext )?.FirstOrDefault();
        }

        /// <summary>
        /// Gets the merge request query.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<EntitySet> GetMergeRequestQuery( this Person person, RockContext rockContext = null )
        {
            var mergeRequestQry = PersonService
                .GetMergeRequestQuery( rockContext )
                .Where( es => es.Items.Any( esi => esi.EntityId == person.Id ) );

            return mergeRequestQry;
        }

        /// <summary>
        /// Creates the merge request.
        /// </summary>
        /// <param name="namelessPerson">The nameless person.</param>
        /// <param name="masterPerson">The master person.</param>
        /// <returns></returns>
        public static EntitySet CreateMergeRequest( this Person namelessPerson, Person masterPerson )
        {
            var entitySetPurposeGuid = SystemGuid.DefinedValue.ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST.AsGuid();
            var definedValueId = DefinedValueCache.GetId( entitySetPurposeGuid );
            if ( definedValueId == null )
            {
                return null;
            }

            var entitySet = new EntitySet
            {
                EntityTypeId = EntityTypeCache.Get<Person>().Id,
                EntitySetPurposeValueId = definedValueId
            };

            entitySet.Items.Add( new EntitySetItem
            {
                EntityId = namelessPerson.Id
            } );

            entitySet.Items.Add( new EntitySetItem
            {
                EntityId = masterPerson.Id
            } );

            return entitySet;
        }

        /// <summary>
        /// Determines whether this <see cref="Person"/> record is allowed to use <see cref="PersonToken"/>  basd on their account protection profile.
        /// </summary>
        /// <param name="person">The person.</param>
        public static bool IsPersonTokenUsageAllowed( this Person person )
        {
            var rockSecuritySettingsService = new SecuritySettingsService();

            return rockSecuritySettingsService.SecuritySettings.DisableTokensForAccountProtectionProfiles.Contains( person.AccountProtectionProfile ) == false;
        }
    }
}

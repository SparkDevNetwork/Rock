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
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.BulkExport;
using Rock.Data;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.Enums;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.Person"/> entity objects.
    /// </summary>
    public partial class PersonService
    {
        /// <summary>
        /// The cut off (inclusive) score 
        /// </summary>
        private const int MATCH_SCORE_CUTOFF = 35;

        /// <summary>
        /// Gets the Person with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public override Person Get( Guid guid )
        {
            // if a specific person Guid is specified, get the person record even if IsDeceased or IsBusiness
            return base.Queryable().FirstOrDefault( a => a.Guid == guid );
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Person Get( int id )
        {
            // if a specific person Id is specified, get the person record even if IsDeceased or IsBusiness
            return base.Queryable().FirstOrDefault( a => a.Id == id );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities (not including Deceased or Nameless records)
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities.</returns>
        public override IQueryable<Person> Queryable()
        {
            return Queryable( new PersonQueryOptions() );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities. If includeDeceased is <c>false</c>, deceased individuals will be excluded.
        /// </summary>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.Person"/> should be included. If <c>true</c>
        /// deceased individuals will be included, otherwise <c>false</c> and they will be excluded.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.Person"/> entities, with deceased individuals either included or excluded based on the provided value.
        /// </returns>
        public IQueryable<Person> Queryable( bool includeDeceased, bool includeBusinesses = true )
        {
            return Queryable( new PersonQueryOptions() { IncludeDeceased = includeDeceased, IncludeBusinesses = includeBusinesses } );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.Person"/> entities with eager loading of the properties that are included in the includes parameter.
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a comma delimited list of properties that should support eager loading.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities with properties that support eager loading.</returns>
        public override IQueryable<Person> Queryable( string includes )
        {
            return Queryable( includes, new PersonQueryOptions() );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities with eager loading of properties that are included in the includes parameter.
        /// If includeDeceased is <c>false</c>, deceased individuals will be excluded
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.Person"/> should be included. If <c>true</c>
        /// deceased individuals will be included, otherwise <c>false</c> and they will be excluded.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.Person"/> entities with properties that support eager loading, with deceased individuals either included or excluded based on the provided value.
        /// </returns>
        public IQueryable<Person> Queryable( string includes, bool includeDeceased, bool includeBusinesses = true )
        {
            return this.Queryable( includes, new PersonQueryOptions() { IncludeDeceased = includeDeceased, IncludeBusinesses = includeBusinesses } );
        }

        /// <summary>
        /// Options that can be configured when using some of the PersonService.Queryable(..) methods
        /// </summary>
        public class PersonQueryOptions
        {
            /// <summary>
            /// Gets or sets a value indicating whether deceased people should be included (default is false)
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include deceased]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeDeceased { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether regular person records should be included (default is true)
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include persons]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludePersons { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether business person records should be included (default is true)
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include businesses]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeBusinesses { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether Nameless person records should be included (default is false)
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include nameless]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeNameless { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether RestUser person records should be included (default is true)
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include rest users]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeRestUsers { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether [include anonymous visitor].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include anonymous visitor]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeAnonymousVisitor { get; set; } = false;

            /// <summary>
            /// Gets a new query options instance that will include all records
            /// in the database.
            /// </summary>
            /// <returns>A new instance of <see cref="PersonQueryOptions"/>.</returns>
            public static PersonQueryOptions AllRecords()
            {
                return new PersonQueryOptions
                {
                    IncludeDeceased = true,
                    IncludePersons = true,
                    IncludeBusinesses = true,
                    IncludeNameless = true,
                    IncludeRestUsers = true,
                    IncludeAnonymousVisitor = true
                };
            }
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities 
        /// using the options specified the <see cref="PersonQueryOptions"/> (default is to exclude deceased people and nameless person records)
        /// </summary>
        /// <param name="personQueryOptions">The person query options.</param>
        /// <returns></returns>
        public IQueryable<Person> Queryable( PersonQueryOptions personQueryOptions )
        {
            return this.Queryable( null, personQueryOptions );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities with eager loading of properties that are included in the includes parameter.
        /// using the option specified the <see cref="PersonQueryOptions"/> (default is to exclude deceased people, nameless person records and the anonymous visitor. )
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="personQueryOptions">The person query options.</param>
        /// <returns></returns>
        private IQueryable<Person> Queryable( string includes, PersonQueryOptions personQueryOptions )
        {
            var qry = base.Queryable( includes );
            List<int> excludedPersonRecordTypeIds = new List<int>();

            personQueryOptions = personQueryOptions ?? new PersonQueryOptions();

            if ( personQueryOptions.IncludePersons == false )
            {
                int? recordTypePersonId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
                if ( recordTypePersonId.HasValue )
                {
                    excludedPersonRecordTypeIds.Add( recordTypePersonId.Value );
                }
            }

            if ( personQueryOptions.IncludeBusinesses == false )
            {
                int? recordTypeBusinessId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
                if ( recordTypeBusinessId.HasValue )
                {
                    excludedPersonRecordTypeIds.Add( recordTypeBusinessId.Value );
                }
            }

            if ( personQueryOptions.IncludeNameless == false )
            {
                int? recordTypeNamelessId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );
                if ( recordTypeNamelessId.HasValue )
                {
                    excludedPersonRecordTypeIds.Add( recordTypeNamelessId.Value );
                }
            }

            if ( excludedPersonRecordTypeIds.Any() )
            {
                if ( excludedPersonRecordTypeIds.Count == 1 )
                {
                    var excludedPersonRecordTypeId = excludedPersonRecordTypeIds[0];
                    qry = qry.Where( p => p.RecordTypeValueId.HasValue && excludedPersonRecordTypeId != p.RecordTypeValueId.Value );
                }
                else
                {
                    qry = qry.Where( p => p.RecordTypeValueId.HasValue && !excludedPersonRecordTypeIds.Contains( p.RecordTypeValueId.Value ) );
                }
            }

            if ( personQueryOptions.IncludeDeceased == false )
            {
                qry = qry.Where( p => p.IsDeceased == false );
            }

            if ( personQueryOptions.IncludeAnonymousVisitor == false )
            {
                var anonymousVisitorGuid = Rock.SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid();
                qry = qry.Where( p => p.Guid != anonymousVisitorGuid );
            }

            return qry;
        }

        #region Get People

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person" /> entities by email address.
        /// </summary>
        /// <param name="email">A <see cref="System.String" /> representing the email address to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean" /> flag indicating if deceased individuals should be included in the search results, if
        /// <c>true</c> then they will be included, otherwise <c>false</c>. Default value is false.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person" /> entities that match the search criteria.
        /// </returns>
        public IQueryable<Person> GetByEmail( string email, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    p.Email == email || ( email == null && p.Email == null ) );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities that have a matching email address, firstname and lastname.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="email">The email.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public IEnumerable<Person> FindPersons( string firstName, string lastName, string email, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return FindPersons( new PersonMatchQuery( firstName, lastName, email, string.Empty ) );
        }

        /// <summary>
        /// Finds people who are considered to be good matches based on the query provided.
        /// </summary>
        /// <param name="searchParameters">The search parameters.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>A IEnumerable of person, ordered by the likelihood they are a good match for the query.</returns>
        public IEnumerable<Person> FindPersons( PersonMatchQuery searchParameters, bool includeDeceased = false, bool includeBusinesses = false )
        {
            // Because we are search different tables (PhoneNumber, PreviousName, etc.) we do multiple queries, store the results in a dictionary, and then find good matches by scoring the results of the dictionary.
            // The large query we're building is: (email matches AND suffix matches AND DoB loose matches AND gender matches) OR last name matches OR phone number matches OR previous name matches
            // The dictionary is PersonId => PersonMatchResult, a class that stores the items that match and calculates the score

            // Query by last name, suffix, dob, and gender
            var query = Queryable( includeDeceased, includeBusinesses )
                .AsNoTracking()
                .Where( p => p.LastName == searchParameters.LastName );

            // Make sure people with an ignored account protection profile are not considered as possible matches
            // We'll also double-check by checking against PersonSummary.AccountProtectionProfile when determining MeetsMinimumConfidence
            List<AccountProtectionProfile> accountProtectionProfilesForDuplicateDetectionToIgnore = new SecuritySettingsService().SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore;
            query = query.Where( p => !accountProtectionProfilesForDuplicateDetectionToIgnore.Contains( p.AccountProtectionProfile ) );

            if ( searchParameters.SuffixValueId.HasValue )
            {
                query = query.Where( a => a.SuffixValueId == searchParameters.SuffixValueId.Value || a.SuffixValueId == null );
            }

            // Check for a DOB match here ignoring year and we award higher points if the year *does* match later, this allows for two tiers of scoring for birth dates
            if ( searchParameters.BirthDate.HasValue )
            {
                query = query.Where( a => ( a.BirthMonth == searchParameters.BirthDate.Value.Month && a.BirthDay == searchParameters.BirthDate.Value.Day ) || a.BirthMonth == null || a.BirthDay == null );
            }

            if ( searchParameters.Gender.HasValue )
            {
                query = query.Where( a => a.Gender == searchParameters.Gender.Value || a.Gender == Gender.Unknown );
            }

            // Create dictionary
            var foundPeople = query
                .Select( p => new PersonSummary()
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    NickName = p.NickName,
                    Email = p.Email,
                    Gender = p.Gender,
                    BirthDate = p.BirthDate,
                    SuffixValueId = p.SuffixValueId,
                    AccountProtectionProfile = p.AccountProtectionProfile
                } )
                .ToList()
                .ToDictionary(
                    p => p.Id,
                    p =>
                    {
                        var result = new PersonMatchResult( searchParameters, p, accountProtectionProfilesForDuplicateDetectionToIgnore )
                        {
                            LastNameMatched = true
                        };
                        return result;
                    } );

            if ( searchParameters.Email.IsNotNullOrWhiteSpace() )
            {
                var searchTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() ).Id;

                // OR query for email or previous email
                var previousEmailQry = new PersonSearchKeyService( this.Context as RockContext ).Queryable();
                this.Queryable( includeDeceased, includeBusinesses )
                    .AsNoTracking()
                    .Where(
                        p => previousEmailQry.Any( a => a.PersonAlias.PersonId == p.Id
                            && a.SearchValue == searchParameters.Email
                            && a.SearchTypeValueId == searchTypeValueId ) )
                    .Select( p => new PersonSummary()
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        NickName = p.NickName,
                        Email = p.Email,
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        SuffixValueId = p.SuffixValueId,
                        AccountProtectionProfile = p.AccountProtectionProfile
                    } )
                    .ToList()
                    .ForEach( p =>
                    {
                        if ( foundPeople.ContainsKey( p.Id ) )
                        {
                            foundPeople[p.Id].PreviousEmailMatched = true;
                        }
                        else
                        {
                            foundPeople[p.Id] = new PersonMatchResult( searchParameters, p, accountProtectionProfilesForDuplicateDetectionToIgnore )
                            {
                                PreviousEmailMatched = true
                            };
                        }
                    } );
            }

            var rockContext = new RockContext();

            // OR query for previous name matches
            var previousNameService = new PersonPreviousNameService( rockContext );
            previousNameService.Queryable( "PersonAlias.Person" )
                .AsNoTracking()
                .Where( n => n.LastName == searchParameters.LastName )
                .Select( n => new PersonSummary()
                {
                    Id = n.PersonAlias.Person.Id,
                    FirstName = n.PersonAlias.Person.FirstName,
                    LastName = n.PersonAlias.Person.LastName,
                    NickName = n.PersonAlias.Person.NickName,
                    Email = n.PersonAlias.Person.Email,
                    Gender = n.PersonAlias.Person.Gender,
                    BirthDate = n.PersonAlias.Person.BirthDate,
                    SuffixValueId = n.PersonAlias.Person.SuffixValueId,
                    AccountProtectionProfile = n.PersonAlias.Person.AccountProtectionProfile
                } )
                .ToList()
                .ForEach( p =>
                {
                    if ( foundPeople.ContainsKey( p.Id ) )
                    {
                        foundPeople[p.Id].PreviousNameMatched = true;
                    }
                    else
                    {
                        foundPeople[p.Id] = new PersonMatchResult( searchParameters, p, accountProtectionProfilesForDuplicateDetectionToIgnore )
                        {
                            PreviousNameMatched = true
                        };
                    }
                } );

            // OR query for mobile phone numbers
            if ( searchParameters.MobilePhone.IsNotNullOrWhiteSpace() )
            {
                var mobilePhoneTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                var phoneNumberService = new PhoneNumberService( rockContext );
                phoneNumberService.Queryable( "Person" )
                    .AsNoTracking()
                    .Where( n => n.Number == searchParameters.MobilePhone && n.NumberTypeValueId == mobilePhoneTypeId )
                    .Select( n => new PersonSummary()
                    {
                        Id = n.Person.Id,
                        FirstName = n.Person.FirstName,
                        LastName = n.Person.LastName,
                        NickName = n.Person.NickName,
                        Email = n.Person.Email,
                        Gender = n.Person.Gender,
                        BirthDate = n.Person.BirthDate,
                        SuffixValueId = n.Person.SuffixValueId,
                        AccountProtectionProfile = n.Person.AccountProtectionProfile
                    } )
                    .ToList()
                    .ForEach( p =>
                    {
                        if ( foundPeople.ContainsKey( p.Id ) )
                        {
                            foundPeople[p.Id].MobileMatched = true;
                        }
                        else
                        {
                            foundPeople[p.Id] = new PersonMatchResult( searchParameters, p, accountProtectionProfilesForDuplicateDetectionToIgnore )
                            {
                                MobileMatched = true
                            };
                        }
                    } );
            }

            // Find people who have a good confidence score
            var goodMatches = foundPeople.Values
                .Where( match => match.MeetsMinimumConfidence )
                .OrderByDescending( match => match.ConfidenceScore )
                .Select( match => match.PersonId )
                .ToList();

            // The OrderBy ensures that the returned persons are in goodMatches.ConfidenceScore order
            return GetByIds( goodMatches ).ToList().OrderBy( p => goodMatches.IndexOf( p.Id ) ).ToList();
        }

        #region FindPersonClasses

        /// <summary>
        /// Contains the properties that can be searched for when performing a GetBestMatch query
        /// </summary>
        public class PersonMatchQuery
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonMatchQuery"/> class.
            /// </summary>
            /// <param name="firstName">The first name.</param>
            /// <param name="lastName">The last name.</param>
            /// <param name="email">The email.</param>
            /// <param name="mobilePhone">The mobile phone.</param>
            public PersonMatchQuery( string firstName, string lastName, string email, string mobilePhone )
            {
                FirstName = firstName.IsNotNullOrWhiteSpace() ? firstName.Trim() : string.Empty;
                LastName = lastName.IsNotNullOrWhiteSpace() ? lastName.Trim() : string.Empty;
                Email = email.IsNotNullOrWhiteSpace() ? email.Trim() : string.Empty;
                MobilePhone = mobilePhone.IsNotNullOrWhiteSpace() ? PhoneNumber.CleanNumber( mobilePhone ) : string.Empty;
                Gender = null;
                BirthDate = null;
                SuffixValueId = null;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PersonMatchQuery" /> class. Use this constructor when the person may not have a birth year.
            /// </summary>
            /// <param name="firstName">The first name.</param>
            /// <param name="lastName">The last name.</param>
            /// <param name="email">The email.</param>
            /// <param name="mobilePhone">The mobile phone.</param>
            /// <param name="gender">The gender.</param>
            /// <param name="birthMonth">The birth month.</param>
            /// <param name="birthDay">The birth day.</param>
            /// <param name="birthYear">The birth year.</param>
            /// <param name="suffixValueId">The suffix value identifier.</param>
            public PersonMatchQuery( string firstName, string lastName, string email, string mobilePhone, Gender? gender = null, int? birthMonth = null, int? birthDay = null, int? birthYear = null, int? suffixValueId = null )
            {
                FirstName = firstName.IsNotNullOrWhiteSpace() ? firstName.Trim() : string.Empty;
                LastName = lastName.IsNotNullOrWhiteSpace() ? lastName.Trim() : string.Empty;
                Email = email.IsNotNullOrWhiteSpace() ? email.Trim() : string.Empty;
                MobilePhone = mobilePhone.IsNotNullOrWhiteSpace() ? PhoneNumber.CleanNumber( mobilePhone ) : string.Empty;
                Gender = gender;
                BirthDate = birthDay.HasValue && birthMonth.HasValue ? new DateTime( birthYear ?? DateTime.MinValue.Year, birthMonth.Value, birthDay.Value ) : ( DateTime? ) null;
                SuffixValueId = suffixValueId;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PersonMatchQuery"/> class.
            /// </summary>
            /// <param name="firstName">The first name.</param>
            /// <param name="lastName">The last name.</param>
            /// <param name="email">The email.</param>
            /// <param name="mobilePhone">The mobile phone.</param>
            /// <param name="gender">The gender.</param>
            /// <param name="birthDate">The birth date.</param>
            /// <param name="suffixValueId">The suffix value identifier.</param>
            public PersonMatchQuery( string firstName, string lastName, string email, string mobilePhone, Gender? gender = null, DateTime? birthDate = null, int? suffixValueId = null )
            {
                FirstName = firstName.IsNotNullOrWhiteSpace() ? firstName.Trim() : string.Empty;
                LastName = lastName.IsNotNullOrWhiteSpace() ? lastName.Trim() : string.Empty;
                Email = email.IsNotNullOrWhiteSpace() ? email.Trim() : string.Empty;
                MobilePhone = mobilePhone.IsNotNullOrWhiteSpace() ? PhoneNumber.CleanNumber( mobilePhone ) : string.Empty;
                Gender = gender;
                BirthDate = birthDate;
                SuffixValueId = suffixValueId;
            }

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
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public Gender? Gender { get; set; }

            /// <summary>
            /// Gets or sets the birth date.
            /// </summary>
            /// <value>
            /// The birth date.
            /// </value>
            public DateTime? BirthDate { get; set; }

            /// <summary>
            /// Gets or sets the suffix value identifier.
            /// </summary>
            /// <value>
            /// The suffix value identifier.
            /// </value>
            public int? SuffixValueId { get; set; }
        }

        /// <summary>
        /// A class to summarise the components of a Person which matched a PersonMatchQuery and produce a score representing the likelihood this match is the correct match.
        /// </summary>
        private class PersonMatchResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonMatchResult"/> class.
            /// </summary>
            /// <param name="query">The query.</param>
            /// <param name="person">The person.</param>
            [RockObsolete( "1.13" )]
            [Obsolete( "Use the constructor that takes a list of AccountProtectionProfiles" )]
            public PersonMatchResult( PersonMatchQuery query, PersonSummary person )
                : this( query, person, new List<AccountProtectionProfile> { AccountProtectionProfile.Extreme, AccountProtectionProfile.High, AccountProtectionProfile.Medium } )
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PersonMatchResult" /> class.
            /// </summary>
            /// <param name="query">The person match query.</param>
            /// <param name="person">The person summary.</param>
            /// <param name="accountProtectionProfilesForDuplicateDetectionToIgnore">The account protection profiles that should never be considered a match.</param>
            public PersonMatchResult( PersonMatchQuery query, PersonSummary person, List<AccountProtectionProfile> accountProtectionProfilesForDuplicateDetectionToIgnore )
            {
                PersonId = person.Id;
                FirstNameMatched = ( person.FirstName != null && person.FirstName != String.Empty && person.FirstName.Equals( query.FirstName, StringComparison.CurrentCultureIgnoreCase ) ) || ( person.NickName != null && person.NickName != String.Empty && person.NickName.Equals( query.FirstName, StringComparison.CurrentCultureIgnoreCase ) );
                LastNameMatched = person.LastName != null && person.LastName != String.Empty && person.LastName.Equals( query.LastName, StringComparison.CurrentCultureIgnoreCase );
                SuffixMatched = query.SuffixValueId.HasValue && person.SuffixValueId != null && query.SuffixValueId == person.SuffixValueId;
                GenderMatched = query.Gender.HasValue & query.Gender == person.Gender;

                EmailSearchSpecified = query.Email.IsNotNullOrWhiteSpace();
                PrimaryEmailMatched = query.Email.IsNotNullOrWhiteSpace() && person.Email.IsNotNullOrWhiteSpace() && person.Email.Equals( query.Email, StringComparison.CurrentCultureIgnoreCase );

                // Only allow this record as a potentional match if their AccountProtectionProfile allows matching. If
                // accountProtectionProfilesForDuplicateDetectionToIgnore contains the AccountProtectionProfile, then
                // never consider this record as a potentional match
                MatchingDisabledForAccountProtectionProfile = accountProtectionProfilesForDuplicateDetectionToIgnore.Contains( person.AccountProtectionProfile );

                if ( query.BirthDate.HasValue && person.BirthDate.HasValue )
                {
                    BirthDate = query.BirthDate.Value.Month == person.BirthDate.Value.Month && query.BirthDate.Value.Day == person.BirthDate.Value.Day;
                    BirthDateYearMatched = BirthDate && person.BirthDate.Value.Year == query.BirthDate.Value.Year;
                }
            }

            public int PersonId { get; set; }

            public bool FirstNameMatched { get; set; }

            public bool LastNameMatched { get; set; }

            public bool EmailSearchSpecified { get; private set; }

            public bool PrimaryEmailMatched { get; set; }

            public bool PreviousEmailMatched { get; set; }

            public bool MobileMatched { get; set; }

            public bool PreviousNameMatched { get; set; }

            public bool SuffixMatched { get; set; }

            public bool GenderMatched { get; set; }

            public bool BirthDate { get; set; }

            public bool BirthDateYearMatched { get; set; }

            public bool MatchingDisabledForAccountProtectionProfile { get; set; }

            public bool MeetsMinimumConfidence
            {
                get
                {
                    if ( ConfidenceScore < MATCH_SCORE_CUTOFF )
                    {
                        return false;
                    }

                    /* 

                    2021-09-27 - MDP

                    This used to have strict rule on Email matching to help prevent matching people
                    if they have different emails. However, this is been changed so that people with a
                    low AccountProtectionProfile (no Login, for example) can be matched even though
                    the email address doesn't exactly match (as long as the ConfidenceScore is
                    above MATCH_SCORE_CUTOFF). Note that this change also results in Never attempting to match people with
                    higher AccountProtectionProfile, regardless of the ConfidenceScore.

                    2020-11-12 - MDP (outdated, see 2021-09-27 note)
                    If Email is specified and the Matched Person has an email, it MUST match
                    the person's primary email OR one of the person's previous emails.
                    This prevents matching in cases where we should not match.
                    See https://app.asana.com/0/1181881054809083/1199161381220905/f for why this was done

                    */

                    if ( MatchingDisabledForAccountProtectionProfile )
                    {
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// Calculates a score representing the likelihood this match is the correct match. Higher is better.
            /// </summary>
            /// <returns></returns>
            public int ConfidenceScore
            {
                get
                {
                    int total = 0;

                    if ( FirstNameMatched )
                    {
                        total += 15;
                    }

                    if ( LastNameMatched )
                    {
                        total += 15;
                    }

                    if ( PreviousNameMatched && !LastNameMatched )
                    {
                        total += 12;
                    }

                    if ( MobileMatched || PrimaryEmailMatched || PreviousEmailMatched )
                    {
                        total += 15;
                    }

                    if ( BirthDate )
                    {
                        total += 10;
                    }

                    if ( BirthDateYearMatched )
                    {
                        total += 5;
                    }

                    if ( GenderMatched )
                    {
                        total += 3;
                    }

                    if ( SuffixMatched )
                    {
                        total += 10;
                    }

                    return total;
                }
            }
        }

        /// <summary>
        /// Used to avoid bringing a whole Person into memory
        /// </summary>
        private class PersonSummary
        {
            public int Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string NickName { get; set; }

            public string Email { get; set; }

            public Gender Gender { get; set; }

            public DateTime? BirthDate { get; set; }

            public int? SuffixValueId { get; set; }

            public AccountProtectionProfile AccountProtectionProfile { get; set; }
        }

        #endregion

        /// <summary>
        /// Looks for a single exact match based on the critieria provided. If more than one person is found it will return null (consider using FindPersons).
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="email">The email.</param>
        /// <param name="updatePrimaryEmail">if set to <c>true</c> the person's primary email will be updated to the search value if it was found as a person search key (alternate lookup address).</param>
        /// <param name="includeDeceased">if set to <c>true</c> include deceased individuals.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> include businesses records.</param>
        /// <returns></returns>
        public Person FindPerson( string firstName, string lastName, string email, bool updatePrimaryEmail, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return FindPerson( new PersonMatchQuery( firstName, lastName, email, string.Empty ), updatePrimaryEmail, includeDeceased, includeBusinesses );
        }

        /// <summary>
        /// Finds the person.
        /// </summary>
        /// <param name="personMatchQuery">The person match query.</param>
        /// <param name="updatePrimaryEmail">if set to <c>true</c> [update primary email].</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public Person FindPerson( PersonMatchQuery personMatchQuery, bool updatePrimaryEmail, bool includeDeceased = false, bool includeBusinesses = false )
        {
            var matches = this.FindPersons( personMatchQuery, includeDeceased, includeBusinesses ).ToList();

            var match = matches.FirstOrDefault();

            // Check if we care about updating the person's primary email
            if ( updatePrimaryEmail && match != null && personMatchQuery.Email.IsNotNullOrWhiteSpace() )
            {
                return UpdatePrimaryEmail( personMatchQuery.Email, match );
            }

            return match;
        }

        /// <summary>
        /// Updates the primary email address of a person if they were found using an alternate email address
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="match">The person to update.</param>
        /// <returns></returns>
        private Person UpdatePrimaryEmail( string email, Person match )
        {
            // Emails are already the same
            if ( string.Equals( match.Email, email, StringComparison.CurrentCultureIgnoreCase ) )
            {
                return match;
            }

            // The emails don't match and we've been instructed to update them
            using ( var privateContext = new RockContext() )
            {
                var privatePersonService = new PersonService( privateContext );
                var updatePerson = privatePersonService.Get( match.Id );
                updatePerson.Email = email;
                privateContext.SaveChanges();
            }

            // Return a freshly queried person
            return this.Get( match.Id );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities that have a matching email address, firstname and lastname and a record type of business.
        /// </summary>
        /// <param name="businessName">Name of the business.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public IEnumerable<Person> FindBusinesses( string businessName, string email )
        {
            businessName = businessName ?? string.Empty;
            email = email ?? string.Empty;
            var query = Queryable( false, true );
            var definedValueBusinessType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
            if ( definedValueBusinessType != null )
            {
                int recordTypeBusiness = definedValueBusinessType.Id;
                query = query.Where( p => p.RecordTypeValueId == recordTypeBusiness );
            }

            return query
            .Where( p =>
                email != "" && p.Email == email &&
                businessName != "" && p.LastName == businessName )
            .ToList();
        }

        /// <summary>
        /// Gets an <see cref="Rock.Model.Person"/> entity that have a first 12 character of business name and a record type of business with either of email, phone or street1 matches with existing record.
        /// </summary>
        /// <param name="businessName">Name of the business.</param>
        /// <param name="email">The email.</param>
        /// <param name="phone">The phone.</param>
        /// <param name="street1">The street1.</param>
        /// <returns></returns>
        public Person FindBusiness( string businessName, string email, string phone, string street1 )
        {
            businessName = businessName ?? string.Empty;
            email = email ?? string.Empty;
            var query = Queryable( false, true );
            var definedValueBusinessType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
            if ( definedValueBusinessType != null )
            {
                int recordTypeBusiness = definedValueBusinessType.Id;
                query = query.Where( p => p.RecordTypeValueId == recordTypeBusiness );
            }

            /*
               SK - 07/01/2022
               We consider only first 12 character of the given business name to look for any matching existing record.
            */
            businessName = businessName.SubstringSafe( 0, 12 );
            query = query
                .Where( p => businessName != "" && p.LastName.StartsWith( businessName ) );

            var matchedPersons = new List<Person>();
            if ( email.IsNotNullOrWhiteSpace() )
            {
                var emailMatchingQry = query.Where( a => a.Email.Equals( email, StringComparison.CurrentCultureIgnoreCase ) );
                var emailMatchedPerson = emailMatchingQry.FirstOrDefault();
                if ( emailMatchedPerson != null )
                {
                    return emailMatchedPerson;
                }
            }

            if ( phone.IsNotNullOrWhiteSpace() )
            {
                var workPhoneTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ).Id;
                var numericPhone = phone.AsNumeric();
                var phnNumberMatchingQry = query.Where( p => p.PhoneNumbers.Any( n => n.NumberTypeValueId == workPhoneTypeId && n.Number.Contains( numericPhone ) ) );
                var phnNumberMatchedPerson = phnNumberMatchingQry.FirstOrDefault();
                if ( phnNumberMatchedPerson != null )
                {
                    return phnNumberMatchedPerson;
                }
            }

            if ( street1.IsNotNullOrWhiteSpace() )
            {
                var rockContext = this.Context as RockContext;
                var groupMemberService = new GroupMemberService( rockContext );
                int groupTypeIdFamilyOrBusiness = GroupTypeCache.GetFamilyGroupType().Id;

                var personIdAddressQry = groupMemberService.Queryable()
                    .Where( m => m.Group.GroupTypeId == groupTypeIdFamilyOrBusiness )
                    .Where( m => m.Group.GroupLocations.Any( gl => gl.Location.Street1.Contains( street1 ) ) )
                    .Select( a => a.PersonId );

                return query.Where( a => personIdAddressQry.Contains( a.Id ) ).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Adds a contact to a business.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        /// <param name="contactPersonId">The contact person identifier.</param>
        public void AddContactToBusiness( int businessId, int contactPersonId )
        {
            var rockContext = this.Context as RockContext;
            var groupMemberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );

            // Get the relationship roles to use
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            int businessContactRoleId = knownRelationshipGroupType.Roles
                .Where( r =>
                    r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            int businessRoleId = knownRelationshipGroupType.Roles
                .Where( r =>
                    r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            int ownerRoleId = knownRelationshipGroupType.Roles
                .Where( r =>
                    r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            // get the known relationship group of the business contact
            // add the business as a group member of that group using the group role of GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS
            var contactKnownRelationshipGroup = groupMemberService.Queryable()
                .Where( g =>
                    g.GroupRoleId == ownerRoleId &&
                    g.PersonId == contactPersonId )
                .Select( g => g.Group )
                .FirstOrDefault();
            if ( contactKnownRelationshipGroup == null )
            {
                // In some cases person may not yet have a know relationship group type
                contactKnownRelationshipGroup = new Group();
                groupService.Add( contactKnownRelationshipGroup );
                contactKnownRelationshipGroup.Name = "Known Relationship";
                contactKnownRelationshipGroup.GroupTypeId = knownRelationshipGroupType.Id;

                var ownerMember = new GroupMember();
                ownerMember.PersonId = contactPersonId;
                ownerMember.GroupRoleId = ownerRoleId;
                contactKnownRelationshipGroup.Members.Add( ownerMember );
            }

            var businessRoleGroupMember = contactKnownRelationshipGroup.Members
                .Where( a => a.PersonId == businessId && a.GroupRoleId == businessRoleId )
                .FirstOrDefault();
            if ( businessRoleGroupMember == null )
            {
                businessRoleGroupMember = new GroupMember();
                businessRoleGroupMember.PersonId = businessId;
                businessRoleGroupMember.GroupRoleId = businessRoleId;
                contactKnownRelationshipGroup.Members.Add( businessRoleGroupMember );
            }

            // get the known relationship group of the business
            // add the business contact as a group member of that group using the group role of GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT
            var businessKnownRelationshipGroup = groupMemberService.Queryable()
                .Where( g =>
                    g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                    g.PersonId == businessId )
                .Select( g => g.Group )
                .FirstOrDefault();
            if ( businessKnownRelationshipGroup == null )
            {
                // In some cases business may not yet have a known relationship group type
                businessKnownRelationshipGroup = new Group();
                groupService.Add( businessKnownRelationshipGroup );
                businessKnownRelationshipGroup.Name = "Known Relationship";
                businessKnownRelationshipGroup.GroupTypeId = knownRelationshipGroupType.Id;

                var ownerMember = new GroupMember();
                ownerMember.PersonId = businessId;
                ownerMember.GroupRoleId = ownerRoleId;
                businessKnownRelationshipGroup.Members.Add( ownerMember );
            }

            var businessContactRoleGroupMember = businessKnownRelationshipGroup.Members
                .Where( a => a.PersonId == contactPersonId && a.GroupRoleId == businessContactRoleId )
                .FirstOrDefault();
            if ( businessContactRoleGroupMember == null )
            {
                businessContactRoleGroupMember = new GroupMember();
                businessContactRoleGroupMember.PersonId = contactPersonId;
                businessContactRoleGroupMember.GroupRoleId = businessContactRoleId;
                businessKnownRelationshipGroup.Members.Add( businessContactRoleGroupMember );
            }
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities by marital status <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <param name="maritalStatusId">An <see cref="System.Int32"/> representing the Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByMaritalStatusId( int? maritalStatusId, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    ( p.MaritalStatusValueId == maritalStatusId || ( maritalStatusId == null && p.MaritalStatusValueId == null ) ) )
                .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by the Person's Connection Status <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="personConnectionStatusId">A <see cref="System.Int32"/> representing the Id of the Person Connection Status <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByPersonConnectionStatusId( int? personConnectionStatusId, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    ( p.ConnectionStatusValueId == personConnectionStatusId || ( personConnectionStatusId == null && p.ConnectionStatusValueId == null ) ) )
                .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by their Record Status <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <param name="recordStatusId">A <see cref="System.Int32"/> representing the Id of the Record Status <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByRecordStatusId( int? recordStatusId, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    ( p.RecordStatusValueId == recordStatusId || ( recordStatusId == null && p.RecordStatusValueId == null ) ) )
                .ToList();
        }

        /// <summary>
        /// Gets an export of Person Records
        /// </summary>
        /// <param name="page">The page being requested (where first page is 1).</param>
        /// <param name="pageSize">The number of records to provide per page. NOTE: This is limited to the 'API Max Items Per Page' global attribute.</param>
        /// <param name="exportOptions">The export options.</param>
        /// <returns></returns>
        public PeopleExport GetPeopleExport( int page, int pageSize, ExportOptions exportOptions )
        {
            IQueryable<Person> personQry;
            SortProperty sortProperty = exportOptions.SortProperty;

            RockContext rockContext = this.Context as RockContext;

            if ( exportOptions.DataViewId.HasValue )
            {
                personQry = ModelExport.QueryFromDataView<Person>( rockContext, exportOptions.DataViewId.Value );
            }
            else
            {
                personQry = this.Queryable( true, true );
            }

            if ( sortProperty != null )
            {
                personQry = personQry.Sort( sortProperty );
            }

            if ( exportOptions.ModifiedSince.HasValue )
            {
                personQry = personQry.Where( a => a.ModifiedDateTime.HasValue && a.ModifiedDateTime >= exportOptions.ModifiedSince.Value );
            }

            var skip = ( page - 1 ) * pageSize;

            PeopleExport peopleExport = new PeopleExport();
            peopleExport.Page = page;
            peopleExport.PageSize = pageSize;
            peopleExport.TotalCount = personQry.Count();

            var pagedPersonQry = personQry
                .Include( a => a.Aliases )
                .Include( a => a.PhoneNumbers )
                .AsNoTracking()
                .Skip( skip )
                .Take( pageSize );

            var personList = pagedPersonQry.ToList();

            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            Guid homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

            int homeAddressDefinedValueId = DefinedValueCache.Get( homeAddressGuid ).Id;

            Dictionary<int, Location> personIdHomeLocationsLookup = new GroupMemberService( rockContext ).AsNoFilter()
                .Where( m => m.Group.GroupTypeId == familyGroupTypeId && pagedPersonQry.Any( p => p.Id == m.PersonId ) )
                .OrderBy( a => a.PersonId )
                .Select( m => new
                {
                    m.PersonId,
                    GroupOrder = m.GroupOrder ?? int.MaxValue,
                    Location = m.Group.GroupLocations.Where( a => a.GroupLocationTypeValueId == homeAddressDefinedValueId && a.IsMailingLocation ).Select( a => a.Location ).FirstOrDefault()
                } )
                .AsNoTracking()
                .ToList()
                .GroupBy( a => a.PersonId )
                .Select( a => new
                {
                    PersonId = a.Key,
                    Location = a.OrderBy( v => v.GroupOrder ).Select( s => s.Location ).FirstOrDefault()
                } )
                .ToDictionary( k => k.PersonId, v => v.Location );

            var globalAttributes = GlobalAttributesCache.Get();
            string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );

            peopleExport.Persons = personList.Select( p => new PersonExport( p, personIdHomeLocationsLookup, publicAppRoot ) ).ToList();

            AttributesExport.LoadAttributeValues( exportOptions, rockContext, peopleExport.Persons, pagedPersonQry );

            return peopleExport;
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by the RecordStatusReason <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="recordStatusReasonId">A <see cref="System.Int32"/> representing the Id of the RecordStatusReason <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByRecordStatusReasonId( int? recordStatusReasonId, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    ( p.RecordStatusReasonValueId == recordStatusReasonId || ( recordStatusReasonId == null && p.RecordStatusReasonValueId == null ) ) )
                .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by their RecordType <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="recordTypeId">A <see cref="System.Int32"/> representing the Id of the RecordType <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByRecordTypeId( int? recordTypeId, bool includeDeceased = false )
        {
            return Queryable( includeDeceased, true )
                .Where( p =>
                    ( p.RecordTypeValueId == recordTypeId || ( recordTypeId == null && p.RecordTypeValueId == null ) ) )
                .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by their Suffix <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <param name="suffixId">An <see cref="System.Int32"/> representing the Id of Suffix <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetBySuffixId( int? suffixId, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    ( p.SuffixValueId == suffixId || ( suffixId == null && p.SuffixValueId == null ) ) )
                .ToList();
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Person"/> entities by their Title <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="titleId">A <see cref="System.Int32"/> representing the Id of the Title <see cref="Rock.Model.DefinedValue"/>.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByTitleId( int? titleId, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    ( p.TitleValueId == titleId || ( titleId == null && p.TitleValueId == null ) ) )
                .ToList();
        }

        #region Search related

        /// <summary>
        /// Person Search parameters for <see cref="Search(PersonSearchOptions)"/>
        /// </summary>
        public class PersonSearchOptions
        {
            /// <summary>
            /// The Name search term
            /// </summary>
            /// <value>
            /// The name search.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [allow first name only].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [allow first name only]; otherwise, <c>false</c>.
            /// </value>
            public bool AllowFirstNameOnly { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            /// <value>
            /// The address.
            /// </value>
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets the phone.
            /// </summary>
            /// <value>
            /// The phone.
            /// </value>
            public string Phone { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include businesses].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include businesses]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeBusinesses { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include deceased].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include deceased]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeDeceased { get; set; }
        }

        /// <summary>
        /// Returns a Queryable of person doing a partial term search using the search terms provided
        /// </summary>
        /// <param name="personSearchOptions">The person search options.</param>
        /// <returns></returns>
        public IQueryable<Person> Search( PersonSearchOptions personSearchOptions )
        {
            bool sortByFullNameReversed = false;
            IQueryable<Person> personSearchQry = null;
            if ( personSearchOptions.Name.IsNotNullOrWhiteSpace() )
            {
                personSearchQry = this.GetByFullName( personSearchOptions.Name, true, personSearchOptions.IncludeBusinesses, personSearchOptions.AllowFirstNameOnly, out sortByFullNameReversed );
            }
            else
            {
                personSearchQry = this.Queryable( personSearchOptions.IncludeDeceased, personSearchOptions.IncludeBusinesses );
            }

            if ( personSearchOptions.Email.IsNotNullOrWhiteSpace() )
            {
                personSearchQry = personSearchQry.Where( p => p.Email.Contains( personSearchOptions.Email ) ).OrderBy( p => p.Email );
            }

            if ( personSearchOptions.Phone.IsNotNullOrWhiteSpace() )
            {
                string numericPhone = personSearchOptions.Phone.AsNumeric();
                personSearchQry = personSearchQry.Where( p => p.PhoneNumbers.Any( n => n.Number.Contains( numericPhone ) ) );
            }

            if ( personSearchOptions.Address.IsNotNullOrWhiteSpace() )
            {
                // Only search for address on the Primary Family. This is significantly faster than searching for the address in all families that the person might be in.
                personSearchQry = personSearchQry.Where( a => a.PrimaryFamily.GroupLocations.Any( gl => gl.Location.Street1.Contains( personSearchOptions.Address ) ) );
            }

            if ( sortByFullNameReversed )
            {
                personSearchQry = personSearchQry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName );
            }
            else
            {
                personSearchQry = personSearchQry.OrderBy( p => p.NickName ).ThenBy( p => p.LastName );
            }

            return personSearchQry;
        }

        /// <summary>
        /// Gets the full name of the by.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="allowFirstNameOnly">if set to true, a single value in fullName will also search for matching first names.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public IQueryable<Person> GetByFullName( string fullName, bool allowFirstNameOnly, bool includeDeceased = false, bool includeBusinesses = false )
        {
            bool reversed = false;
            return GetByFullName( fullName, includeDeceased, includeBusinesses, allowFirstNameOnly, out reversed );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities by the person's full name.
        /// </summary>
        /// <param name="fullName">A <see cref="System.String"/> representing the full name to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <param name="allowFirstNameOnly">if set to true, a single value in fullName will also search for matching first names.</param>
        /// <param name="reversed">if set to <c>true</c> [reversed].</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IQueryable<Person> GetByFullName( string fullName, bool includeDeceased, bool includeBusinesses, bool allowFirstNameOnly, out bool reversed )
        {
            var firstNames = new List<string>();
            var lastNames = new List<string>();
            string singleName = string.Empty;

            fullName = fullName.Trim();

            var nameParts = fullName.Trim().Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            if ( fullName.Contains( ',' ) )
            {
                reversed = true;

                // only split by comma if there is a comma present (for example if 'Smith Jones, Sally' is the search, last name would be 'Smith Jones')
                nameParts = fullName.Split( ',' ).ToList();
                if ( nameParts.Count >= 1 )
                {
                    lastNames.Add( nameParts[0].Trim() );
                }

                if ( nameParts.Count >= 2 )
                {
                    firstNames.Add( nameParts[1].Trim() );
                }
            }
            else if ( fullName.Contains( ' ' ) )
            {
                reversed = false;

                for ( int i = 1; i < nameParts.Count; i++ )
                {
                    firstNames.Add( nameParts.Take( i ).ToList().AsDelimited( " " ) );
                    lastNames.Add( nameParts.Skip( i ).ToList().AsDelimited( " " ) );
                }
            }
            else
            {
                // no spaces, no commas
                reversed = true;
                singleName = fullName;
            }

            if ( !string.IsNullOrWhiteSpace( singleName ) )
            {
                int? personId = singleName.AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    return Queryable()
                        .Where( p => p.Aliases.Any( a => a.AliasPersonId == personId.Value ) );
                }

                Guid? personGuid = singleName.AsGuidOrNull();
                if ( personGuid.HasValue )
                {
                    return Queryable()
                        .Where( p => p.Aliases.Any( a => a.AliasPersonGuid == personGuid.Value ) );
                }

                var previousNamesQry = new PersonPreviousNameService( this.Context as RockContext ).Queryable().AsNoTracking();

                if ( allowFirstNameOnly )
                {
                    return Queryable( includeDeceased, includeBusinesses )
                        .Where( p =>
                            p.LastName.StartsWith( singleName ) ||
                            p.FirstName.StartsWith( singleName ) ||
                            p.NickName.StartsWith( singleName ) ||
                            previousNamesQry.Any( a => a.PersonAlias.PersonId == p.Id && a.LastName.StartsWith( singleName ) ) );
                }
                else
                {
                    /* Originally written as:
                    // return Queryable( includeDeceased, includeBusinesses )
                    //    .Where( p =>
                    //        p.LastName.StartsWith( singleName ) ||
                    //        previousNamesQry.Any( a => a.PersonAlias.PersonId == p.Id && a.LastName.StartsWith( singleName ) ) );

                    // The LEFT OUTER JOIN version below is faster than the original
                    // because SQL uses a different index for the WHERE EXISTS version
                    // due to the fact that it will require the entire table to be scanned
                    // and return 1 where the criteria match.
                    //
                    // The query below could have been written in fluent syntax, but it's a
                    // bit more complicated like this (they produce the exact same SQL):
                    //
                    // return Queryable( includeDeceased, includeBusinesses )
                    //    .GroupJoin(
                    //            previousNamesQry,
                    //            Person => Person.Id,
                    //            PrevName => PrevName.PersonAlias.PersonId,
                    //            ( x, y ) => new { Person = x, PrevNames = y } )
                    //    .SelectMany(
                    //            x => x.PrevNames.DefaultIfEmpty(),
                    //            ( x, y ) => new { Person = x.Person, PrevName = y } )
                    //    .Where( x => x.Person.LastName.StartsWith( singleName ) || x.PrevName.LastName.StartsWith( singleName ) )
                    //    .Select( x => x.Person );
                    */

                    return from person in Queryable( includeDeceased, includeBusinesses )
                           join personPreviousName in previousNamesQry
                                on person.Id equals personPreviousName.PersonAlias.PersonId into pnQuery
                           from ppn in pnQuery.DefaultIfEmpty()
                           where person.LastName.StartsWith( singleName ) || ppn.LastName.StartsWith( singleName )
                           select person;
                }
            }
            else
            {
                if ( firstNames.Any() && lastNames.Any() )
                {
                    // Find all matching First and LastName, or LastName, but only select the person IDs to reduce
                    // the pressure that the UNION below would otherwise have (SELECT DISTINCT * issue).
                    var qry = GetByFirstLastName( firstNames.Any() ? firstNames[0] : "", lastNames.Any() ? lastNames[0] : "", includeDeceased, includeBusinesses )
                        .Select( p => p.Id );
                    for ( var i = 1; i < firstNames.Count; i++ )
                    {
                        qry = qry.Union( GetByFirstLastName( firstNames[i], lastNames[i], includeDeceased, includeBusinesses ).Select( p => p.Id ) );
                    }

                    // always include a search for just last name using the last two parts of name search
                    if ( nameParts.Count >= 2 )
                    {
                        var lastName = string.Join( " ", nameParts.TakeLast( 2 ) );

                        qry = qry.Union( GetByLastName( lastName, includeDeceased, includeBusinesses ).Select( p => p.Id ) );
                    }

                    // If searching for businesses, search by the full name as well to handle "," in the name
                    if ( includeBusinesses )
                    {
                        qry = qry.Union( GetByLastName( fullName, includeDeceased, includeBusinesses ).Select( p => p.Id ) );
                    }

                    // Lastly, return all people with those person Ids using the base Queryable used
                    // initially by the GetByFirstLastName() call.
                    return Queryable( includeDeceased, includeBusinesses ).Where( p => qry.Contains( p.Id ) );
                }
                else
                {
                    // Blank string was used, return empty list
                    return new List<Person>().AsQueryable();
                }
            }
        }

        /// <summary>
        /// Gets the last name of the by first.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public IQueryable<Person> GetByFirstLastName( string firstName, string lastName, bool includeDeceased, bool includeBusinesses )
        {
            string fullname = !string.IsNullOrWhiteSpace( firstName ) ? firstName + " " + lastName : lastName;

            var previousNamesQry = new PersonPreviousNameService( this.Context as RockContext ).Queryable().AsNoTracking();

            var qry = Queryable( includeDeceased, includeBusinesses );
            if ( includeBusinesses )
            {
                int recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

                // if a we are including businesses, compare fullname against the Business Name (Person.LastName)
                qry = qry.Where( p =>
                    ( p.RecordTypeValueId.HasValue && p.RecordTypeValueId.Value == recordTypeBusinessId && p.LastName.StartsWith( fullname ) )
                    ||
                    ( ( p.LastName.StartsWith( lastName ) || previousNamesQry.Any( a => a.PersonAlias.PersonId == p.Id && a.LastName.StartsWith( lastName ) ) ) &&
                    ( p.FirstName.StartsWith( firstName ) ||
                    p.NickName.StartsWith( firstName ) ) ) );
            }
            else
            {
                qry = qry.Where( p =>
                    ( ( p.LastName.StartsWith( lastName ) || previousNamesQry.Any( a => a.PersonAlias.PersonId == p.Id && a.LastName.StartsWith( lastName ) ) ) &&
                    ( p.FirstName.StartsWith( firstName ) ||
                    p.NickName.StartsWith( firstName ) ) ) );
            }

            return qry;
        }

        /// <summary>
        /// Gets the by full name ordered.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public IQueryable<Person> GetByLastName( string lastName, bool includeDeceased, bool includeBusinesses )
        {
            lastName = lastName.Trim();

            var lastNameQry = Queryable( includeDeceased, includeBusinesses )
                                    .Where( p => p.LastName.StartsWith( lastName ) );

            return lastNameQry;
        }

        /// <summary>
        /// Gets the by full name ordered.
        /// </summary>
        /// <param name="fullName">The full name search term.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <param name="allowFirstNameOnly">if set to true, a single value in fullName will also search for matching first names.</param>
        /// <param name="reversed">if set to <c>true</c> [reversed].</param>
        /// <returns></returns>
        public IOrderedQueryable<Person> GetByFullNameOrdered( string fullName, bool includeDeceased, bool includeBusinesses, bool allowFirstNameOnly, out bool reversed )
        {
            var qry = GetByFullName( fullName, includeDeceased, includeBusinesses, allowFirstNameOnly, out reversed );
            if ( reversed )
            {
                return qry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName );
            }
            else
            {
                return qry.OrderBy( p => p.NickName ).ThenBy( p => p.LastName );
            }
        }

        /// <summary>
        /// Gets the similar sounding names.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="excludeIds">The exclude ids.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public List<string> GetSimilarNames( string fullName, List<int> excludeIds, bool includeDeceased = false, bool includeBusinesses = false )
        {
            var names = fullName.SplitDelimitedValues();

            string firstName = string.Empty;
            string lastName = string.Empty;

            bool reversed = false;

            if ( fullName.Contains( ',' ) )
            {
                reversed = true;
                lastName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                firstName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else if ( fullName.Contains( ' ' ) )
            {
                firstName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                lastName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else
            {
                reversed = true;
                lastName = fullName.Trim();
            }

            return GetSimilarNames( firstName, lastName, reversed, excludeIds, includeDeceased, includeBusinesses );
        }

        /// <summary>
        /// Gets the similar names.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="reversed">if set to <c>true</c> [reversed].</param>
        /// <param name="excludeIds">The exclude ids.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public List<string> GetSimilarNames( string firstName, string lastName, bool reversed, List<int> excludeIds, bool includeDeceased = false, bool includeBusinesses = false )
        {
            var similarNames = new List<string>();

            if ( !string.IsNullOrWhiteSpace( firstName ) && !string.IsNullOrWhiteSpace( lastName ) )
            {
                var metaphones = ( ( RockContext ) this.Context ).Metaphones;

                string ln1 = string.Empty;
                string ln2 = string.Empty;
                Rock.Utility.DoubleMetaphone.doubleMetaphone( lastName, ref ln1, ref ln2 );
                ln1 = ln1 ?? string.Empty;
                ln2 = ln2 ?? string.Empty;

                var lastNames = metaphones
                    .Where( m =>
                        ( ln1 != "" && ( m.Metaphone1 == ln1 || m.Metaphone2 == ln1 ) ) ||
                        ( ln2 != "" && ( m.Metaphone1 == ln2 || m.Metaphone2 == ln2 ) ) )
                    .Select( m => m.Name )
                    .Distinct()
                    .ToList();

                if ( lastNames.Any() )
                {
                    string fn1 = string.Empty;
                    string fn2 = string.Empty;
                    Rock.Utility.DoubleMetaphone.doubleMetaphone( firstName, ref fn1, ref fn2 );
                    fn1 = fn1 ?? string.Empty;
                    fn2 = fn2 ?? string.Empty;

                    var firstNames = metaphones
                        .Where( m =>
                            ( fn1 != "" && ( m.Metaphone1 == fn1 || m.Metaphone2 == fn1 ) ) ||
                            ( fn2 != "" && ( m.Metaphone1 == fn2 || m.Metaphone2 == fn2 ) ) )
                        .Select( m => m.Name )
                        .Distinct()
                        .ToList();

                    if ( firstNames.Any() )
                    {
                        similarNames = Queryable( includeDeceased, includeBusinesses )
                        .Where( p => !excludeIds.Contains( p.Id ) &&
                            lastNames.Contains( p.LastName ) &&
                            ( firstNames.Contains( p.FirstName ) || firstNames.Contains( p.NickName ) ) )
                        .Select( p => ( reversed ? p.LastName + ", " + p.NickName : p.NickName + " " + p.LastName ) )
                        .Distinct()
                        .ToList();
                    }
                }
            }

            return similarNames;
        }

        /// <summary>
        /// Gets an queryable collection of <see cref="Rock.Model.Person"/> entities where their phone number partially matches the provided value.
        /// </summary>
        /// <param name="partialPhoneNumber">A <see cref="System.String"/> containing a partial phone number to match.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An queryable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IQueryable<Person> GetByPhonePartial( string partialPhoneNumber, bool includeDeceased = false, bool includeBusinesses = false )
        {
            string numericPhone = partialPhoneNumber.AsNumeric();

            return Queryable( includeDeceased, includeBusinesses )
                .Where( p =>
                    p.PhoneNumbers.Any( n => n.Number.Contains( numericPhone ) ) );
        }

        #endregion search related

        /// <summary>
        /// Gets the businesses sorted by name
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Person> GetBusinesses( int personId )
        {
            Guid businessGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid();
            Guid ownerGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            var rockContext = ( RockContext ) this.Context;
            return new GroupMemberService( rockContext )
                .Queryable().AsNoTracking()
                .Where( m =>
                        m.GroupRole.Guid.Equals( businessGuid ) &&
                        m.Group.Members.Any( o =>
                            o.PersonId == personId &&
                            o.GroupRole.Guid.Equals( ownerGuid ) ) )
                .Select( m => m.Person )
                .OrderBy( b => b.LastName );
        }

        /// <summary>
        /// Gets the families sorted by the person's GroupOrder (GroupMember.GroupOrder)
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetFamilies( int personId )
        {
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            return new GroupMemberService( ( RockContext ) this.Context ).Queryable( true )
                .Where( m => m.PersonId == personId && m.Group.GroupTypeId == familyGroupTypeId )
                .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                .Select( m => m.Group );
        }

        /// <summary>
        /// Gets the adults.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Person> GetAllAdults()
        {
            int familyGroupTypeId = 0;
            int adultRoleId = 0;

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( familyGroupType != null )
            {
                familyGroupTypeId = familyGroupType.Id;
                adultRoleId = familyGroupType.Roles
                    .Where( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
            }

            var groupMemberService = new GroupMemberService( ( RockContext ) this.Context );
            return groupMemberService
                .Queryable()
                .Where( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.GroupRoleId == adultRoleId )
                .Select( m => m.Person );
        }

        /// <summary>
        /// 
        /// </summary>
        public class PersonFamilyGivingXref
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public string GivingId { get; set; }

            /// <summary>
            /// Gets or sets the family group identifier.
            /// </summary>
            /// <value>
            /// The family group identifier.
            /// </value>
            public int FamilyGroupId { get; set; }

            /// <summary>
            /// Gets or sets the family role identifier.
            /// </summary>
            /// <value>
            /// The family role identifier.
            /// </value>
            public int FamilyRoleId { get; set; }
        }

        /// <summary>
        /// Gets all person family giving xref.
        /// </summary>
        /// <returns></returns>
        public IQueryable<PersonFamilyGivingXref> GetAllPersonFamilyGivingXref()
        {
            int familyGroupTypeId = 0;
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( familyGroupType != null )
            {
                familyGroupTypeId = familyGroupType.Id;
            }

            var groupMemberService = new GroupMemberService( ( RockContext ) this.Context );
            return groupMemberService
                .Queryable()
                .Where( m => m.Group.GroupTypeId == familyGroupTypeId )
                .Select( m => new PersonFamilyGivingXref
                {
                    PersonId = m.PersonId,
                    GivingId = m.Person.GivingId,
                    FamilyGroupId = m.GroupId,
                    FamilyRoleId = m.GroupRoleId
                } );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Person> GetAllChildren()
        {
            int familyGroupTypeId = 0;
            int childRoleId = 0;

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( familyGroupType != null )
            {
                familyGroupTypeId = familyGroupType.Id;
                childRoleId = familyGroupType.Roles
                    .Where( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
            }

            var groupMemberService = new GroupMemberService( ( RockContext ) this.Context );
            return groupMemberService
                .Queryable()
                .Where( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.GroupRoleId == childRoleId )
                .Select( m => m.Person );
        }

        /// <summary>
        /// Gets the head of households.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Person> GetAllHeadOfHouseholds()
        {
            int groupTypeFamilyId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;
            var groupMemberService = new GroupMemberService( ( RockContext ) this.Context );
            return groupMemberService
                .Queryable()
                .Where( m => m.Group.GroupTypeId == groupTypeFamilyId )
                .GroupBy(
                    m => m.GroupId,
                    ( key, g ) => g
                        .OrderBy( m => m.GroupRole.Order )
                        .ThenBy( m => m.Person.Gender )
                        .ThenBy( m => m.Person.BirthYear )
                        .ThenBy( m => m.Person.BirthMonth )
                        .ThenBy( m => m.Person.BirthDay )
                        .FirstOrDefault() )
                .Select( m => m.Person );
        }

        /// <summary>
        /// Gets the giving leaders.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Person> GetAllGivingLeaders()
        {
            return Queryable()
                .Where( p => p.Id == p.GivingLeaderId );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person sorted by the Person's GroupOrder (GroupMember.GroupOrder)
        /// Does not included deceased members.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetFamilyMembers( int personId, bool includeSelf = false )
        {
            return GetFamilyMembers( personId, includeSelf, false );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person sorted by the Person's GroupOrder (GroupMember.GroupOrder)
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person.
        /// </returns>
        public IQueryable<GroupMember> GetFamilyMembers( int personId, bool includeSelf, bool includeDeceased )
        {
            int groupTypeFamilyId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;
            return GetGroupMembers( groupTypeFamilyId, personId, includeSelf, includeDeceased );
        }

        /// <summary>
        /// Gets the group members. Does not include deceased members.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetGroupMembers( int groupTypeId, int personId, bool includeSelf = false )
        {
            return GetGroupMembers( groupTypeId, personId, includeSelf, false );
        }

        /// <summary>
        /// Gets the group members
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetGroupMembers( int groupTypeId, int personId, bool includeSelf, bool includeDeceased )
        {
            var groupMemberService = new GroupMemberService( ( RockContext ) this.Context );

            // construct the linq in a way that will return the group members sorted by the GroupOrder setting of the person
            var groupMembersQry = groupMemberService.Queryable( true )
                .Where( m =>
                    m.PersonId == personId &&
                    m.Group.GroupTypeId == groupTypeId )
                .Select( m => new
                {
                    SortedMembers = m.Group.Members.Select( x => new
                    {
                        GroupMember = x,
                        PersonGroupOrder = m.GroupOrder
                    } )
                } )
                .SelectMany( x => x.SortedMembers )
                .OrderBy( a => a.PersonGroupOrder ?? int.MaxValue )
                .Select( a => a.GroupMember );

            if ( !includeDeceased )
            {
                groupMembersQry = groupMembersQry.Where( m => !m.Person.IsDeceased );
            }

            if ( !includeSelf )
            {
                groupMembersQry = groupMembersQry.Where( m => m.PersonId != personId );
            }

            return groupMembersQry.Include( a => a.Person ).Include( a => a.GroupRole );
        }

        /// <summary>
        /// Gets the family members.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetFamilyMembers( Group family, int personId, bool includeSelf = false )
        {
            return GetFamilyMembers( family, personId, includeSelf, false );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person.
        /// </returns>
        public IQueryable<GroupMember> GetFamilyMembers( Group family, int personId, bool includeSelf, bool includeDeceased )
        {
            return new GroupMemberService( ( RockContext ) this.Context ).Queryable( "GroupRole, Person", true )
                .Where( m => m.GroupId == family.Id )
                .Where( m => includeDeceased || !m.Person.IsDeceased )
                .Where( m => includeSelf || m.PersonId != personId )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue )
                .ThenByDescending( m => m.Person.Gender )
                .Distinct();
        }

        /// <summary>
        /// Special class that holds the result of a GetChildWithParents query
        /// </summary>
        public class ChildWithParents
        {
            /// <summary>
            /// Gets or sets the child.
            /// </summary>
            /// <value>
            /// The child.
            /// </value>
            public Person Child { get; set; }

            /// <summary>
            /// Gets or sets the parents.
            /// </summary>
            /// <value>
            /// The parents.
            /// </value>
            public IEnumerable<Person> Parents { get; set; }
        }

        /// <summary>
        /// Gets a Queryable of Children with their Parents
        /// </summary>
        /// <param name="includeChildrenWithoutParents">if set to <c>true</c> [include children without parents].</param>
        /// <returns></returns>
        public IQueryable<ChildWithParents> GetChildWithParents( bool includeChildrenWithoutParents )
        {
            var groupTypeFamily = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            int adultRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            int childRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
            int groupTypeFamilyId = groupTypeFamily.Id;

            var qryFamilyGroups = new GroupService( this.Context as RockContext ).Queryable().Where( g => g.GroupTypeId == groupTypeFamilyId && g.Members.Any( a => a.GroupRoleId == childRoleId ) )
                .Select( g => new
                {
                    KidsWithAdults = g.Members.Where( a => a.GroupRoleId == childRoleId ).Select( a => new
                    {
                        Child = a.Person,
                        Parents = g.Members.Where( aa => aa.GroupRoleId == adultRoleId ).Select( b => b.Person )
                    } )
                } )
                .SelectMany( x => x.KidsWithAdults.Select( xx => new { xx.Child, xx.Parents } ) );

            var qryKids = this.Queryable();

            var qryChildrenWithParents = qryKids.Join(
                qryFamilyGroups,
                k => k.Id,
                k2 => k2.Child.Id,
                ( k, f ) => new ChildWithParents
                {
                    Child = f.Child,
                    Parents = f.Parents
                } );

            if ( !includeChildrenWithoutParents )
            {
                qryChildrenWithParents = qryChildrenWithParents.Where( a => a.Parents.Any() );
            }

            return qryChildrenWithParents;
        }

        /// <summary>
        /// Special class that holds the result of a ChildWithParent query
        /// </summary>
        public class ChildWithParent
        {
            /// <summary>
            /// Gets or sets the child.
            /// </summary>
            /// <value>
            /// The child.
            /// </value>
            public Person Child { get; set; }

            /// <summary>
            /// Gets or sets the parent.
            /// </summary>
            /// <value>
            /// The parent.
            /// </value>
            public Person Parent { get; set; }
        }

        /// <summary>
        /// Gets a Queryable of Children with their Parents flattened out so each record is a child with a parent (a kid with 2 parents would return two records)
        /// </summary>
        /// <returns></returns>
        public IQueryable<ChildWithParent> GetChildWithParent()
        {
            var qryChildrenWithParents = this.GetChildWithParents( false );

            var qryChildWithParent = qryChildrenWithParents.Select( a => new
            {
                ParentKid = a.Parents.Select( aa => new
                {
                    Parent = aa,
                    Child = a.Child
                } )
            } ).SelectMany( sm => sm.ParentKid ).Select( s => new ChildWithParent
            {
                Child = s.Child,
                Parent = s.Parent
            } );

            return qryChildWithParent;
        }

        /// <summary>
        /// Special class that holds the result of a GetChildWithParents query
        /// </summary>
        public class ParentWithChildren
        {
            /// <summary>
            /// Gets or sets the child.
            /// </summary>
            /// <value>
            /// The child.
            /// </value>
            public Person Parent { get; set; }

            /// <summary>
            /// Gets or sets the parents.
            /// </summary>
            /// <value>
            /// The parents.
            /// </value>
            public IEnumerable<Person> Children { get; set; }
        }

        /// <summary>
        /// Gets a Queryable of Parents with their Children
        /// </summary>
        /// <param name="includeParentsWithoutChildren">if set to <c>true</c> [include parents without children].</param>
        /// <returns></returns>
        public IQueryable<ParentWithChildren> GetParentWithChildren( bool includeParentsWithoutChildren )
        {
            var groupTypeFamily = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            int childRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
            int parentRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            int groupTypeFamilyId = groupTypeFamily.Id;

            var qryFamilyGroups = new GroupService( this.Context as RockContext ).Queryable().Where( g => g.GroupTypeId == groupTypeFamilyId && g.Members.Any( a => a.GroupRoleId == parentRoleId ) )
                .Select( g => new
                {
                    AdultsWithKids = g.Members.Where( a => a.GroupRoleId == parentRoleId ).Select( a => new
                    {
                        Parent = a.Person,
                        Children = g.Members.Where( aa => aa.GroupRoleId == childRoleId ).Select( b => b.Person )
                    } )
                } )
                .SelectMany( x => x.AdultsWithKids.Select( xx => new { xx.Parent, xx.Children } ) );

            var qryAdults = this.Queryable();

            var qryParentsWithChildren = qryAdults.Join(
                qryFamilyGroups,
                k => k.Id,
                k2 => k2.Parent.Id,
                ( k, f ) => new ParentWithChildren
                {
                    Parent = f.Parent,
                    Children = f.Children
                } );

            if ( !includeParentsWithoutChildren )
            {
                qryParentsWithChildren = qryParentsWithChildren.Where( a => a.Children.Any() );
            }

            return qryParentsWithChildren;
        }

        /// <summary>
        /// Special class that holds the result of a ParentWithChild query
        /// </summary>
        public class ParentWithChild
        {
            /// <summary>
            /// Gets or sets the parent.
            /// </summary>
            /// <value>
            /// The parent.
            /// </value>
            public Person Parent { get; set; }

            /// <summary>
            /// Gets or sets the child.
            /// </summary>
            /// <value>
            /// The child.
            /// </value>
            public Person Child { get; set; }
        }

        /// <summary>
        /// Gets a Queryable of Parents with their Children flattened out so each record is a parent with a child (an adult with 2 children would return two records)
        /// </summary>
        /// <returns></returns>
        public IQueryable<ParentWithChild> GetParentWithChild()
        {
            var qryParentsWithChildren = this.GetParentWithChildren( false );

            var qryParentWithChild = qryParentsWithChildren.Select( a => new
            {
                ChildAdult = a.Children.Select( aa => new
                {
                    Child = aa,
                    Parent = a.Parent
                } )
            } ).SelectMany( sm => sm.ChildAdult ).Select( s => new ParentWithChild
            {
                Parent = s.Parent,
                Child = s.Child
            } );

            return qryParentWithChild;
        }

        /// <summary>
        /// Gets the family names.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeMemberNames">if set to <c>true</c> [include member names].</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public List<string> GetFamilyNames( int personId, bool includeMemberNames = true, bool includeSelf = true )
        {
            var familyNames = new List<string>();

            foreach ( var family in GetFamilies( personId ) )
            {
                string familyName = family.Name;

                if ( includeMemberNames )
                {
                    var nickNames = GetFamilyMembers( family, personId, includeSelf )
                        .Select( m => m.Person.NickName ).ToList();
                    if ( nickNames.Any() )
                    {
                        familyName = string.Format( "{0} ({1})", familyName, nickNames.AsDelimited( ", " ) );
                    }
                }

                familyNames.Add( familyName );
            }

            return familyNames;
        }

        /// <summary>
        /// Gets any previous last names for this person sorted alphabetically by LastName
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IOrderedQueryable<PersonPreviousName> GetPreviousNames( int personId )
        {
            return new PersonPreviousNameService( ( RockContext ) this.Context ).Queryable()
                .Where( m => m.PersonAlias.PersonId == personId ).OrderBy( a => a.LastName );
        }

        /// <summary>
        /// Gets any search keys for this person
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<PersonSearchKey> GetPersonSearchKeys( int personId )
        {
            return new PersonSearchKeyService( ( RockContext ) this.Context ).Queryable()
                .Where( m => m.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Splits a full name into a separate first and last name. If only one name is found it defaults to first name.
        /// </summary>
        /// <param name="fullName">The full name</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        public void SplitName( string fullName, out string firstName, out string lastName )
        {
            // Uses logic from IQueryable<Person> GetByFullName
            firstName = string.Empty;
            lastName = string.Empty;

            if ( fullName.Contains( ',' ) )
            {
                // only split by comma if there is a comma present (for example if 'Smith Jones, Sally' is the search, last name would be 'Smith Jones')
                var nameParts = fullName.Split( ',' );
                lastName = nameParts.Length >= 1 ? nameParts[0].Trim() : string.Empty;
                firstName = nameParts.Length >= 2 ? nameParts[1].Trim() : string.Empty;
            }
            else if ( fullName.Trim().Contains( ' ' ) )
            {
                // if no comma, assume the search is in 'firstname lastname' format (note: 'firstname lastname1 lastname2' isn't supported yet)
                var names = fullName.Split( ' ' );
                firstName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                lastName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else
            {
                // no spaces, no commas
                firstName = fullName.Trim();
            }
        }

        /// <summary>
        /// Gets the first group location.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="locationTypeValueId">The location type value id.</param>
        /// <returns></returns>
        public GroupLocation GetFirstLocation( int personId, int locationTypeValueId )
        {
            int groupTypeFamilyId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            return new GroupMemberService( ( RockContext ) this.Context ).Queryable( "GroupLocations.Location", true )
                .Where( m =>
                    m.PersonId == personId &&
                    m.Group.GroupTypeId == groupTypeFamilyId )
                .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                .SelectMany( m => m.Group.GroupLocations )
                .Where( gl => gl.GroupLocationTypeValueId == locationTypeValueId )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets a phone number
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="phoneType">Type of the phone.</param>
        /// <returns></returns>
        public PhoneNumber GetPhoneNumber( Person person, DefinedValueCache phoneType )
        {
            if ( person != null )
            {
                return new PhoneNumberService( ( RockContext ) this.Context ).Queryable()
                    .Where( n => n.PersonId == person.Id && n.NumberTypeValueId == phoneType.Id )
                    .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Get the person associated with the phone number. Filter to any matching phone number, regardless
        /// of type. Then order by those with a matching number and SMS enabled; then further order
        /// by matching number with type == mobile; finally order by person Id to get the oldest
        /// person in the case of duplicate records.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use other GetPersonFromMobilePhoneNumber that has createNamelessPersonIfNotFound parameter", true )]
        public Person GetPersonFromMobilePhoneNumber( string phoneNumber )
        {
            return GetPersonFromMobilePhoneNumber( phoneNumber, false );
        }

        /// <summary>
        /// Get the person associated with the phone number. Filter to any matching phone number, regardless
        /// of type. Then order by those with a matching number and SMS enabled; then further order
        /// by matching number with type == mobile; finally order by person Id to get the oldest
        /// person in the case of duplicate records. If no person is found and <paramref name="createNamelessPersonIfNotFound" /> = true, a
        /// Nameless person record will created which can later be matched to a person
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="createNamelessPersonIfNotFound">if set to <c>true</c> [create nameless person if not found].</param>
        /// <returns></returns>
        public Person GetPersonFromMobilePhoneNumber( string phoneNumber, bool createNamelessPersonIfNotFound )
        {
            var recordTypeValueIdNameless = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );

            int numberTypeMobileValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            // cleanup phone
            phoneNumber = PhoneNumber.CleanNumber( phoneNumber );

            // order so that non-nameless person with an SMS number with messaging enabled are listed first
            // then sort by the oldest person record in case there are multiple people with the same number
            var person = new PhoneNumberService( this.Context as RockContext ).Queryable()
                .Where( pn => pn.FullNumber == phoneNumber )
                .OrderByDescending( pn => pn.IsMessagingEnabled )
                .ThenByDescending( pn => pn.NumberTypeValueId == numberTypeMobileValueId )
                .ThenByDescending( p => p.Person.RecordTypeValueId != recordTypeValueIdNameless )
                .ThenBy( pn => pn.PersonId )
                .Select( a => a.Person )
                .FirstOrDefault();

            if ( createNamelessPersonIfNotFound && person == null )
            {
                using ( var nameLessPersonRockContext = new RockContext() )
                {
                    var smsPhoneNumber = new PhoneNumber();
                    smsPhoneNumber.NumberTypeValueId = numberTypeMobileValueId;
                    smsPhoneNumber.Number = phoneNumber;
                    smsPhoneNumber.IsMessagingEnabled = true;

                    person = new Person();
                    person.RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );
                    person.PhoneNumbers.Add( smsPhoneNumber );
                    new PersonService( nameLessPersonRockContext ).Add( person );
                    nameLessPersonRockContext.SaveChanges();

                    person = this.Get( person.Id );
                }
            }

            return person;
        }

        /// <summary>
        /// Merges the nameless person <see cref="Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS"/> to existing person and saves changes to the database
        /// </summary>
        /// <param name="namelessPerson">The nameless person.</param>
        /// <param name="existingPerson">The existing person.</param>
        public void MergeNamelessPersonToExistingPerson( Person namelessPerson, Person existingPerson )
        {
            int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
            var existingPersonMobilePhoneNumber = existingPerson.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneTypeId );
            var namelessPersonMobilePhoneNumberNumber = namelessPerson.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneTypeId ).Number;

            if ( existingPersonMobilePhoneNumber == null )
            {
                // the person we are linking the phone number to doesn't have a SMS Messaging Number, so add a new one
                existingPersonMobilePhoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = mobilePhoneTypeId,
                    IsMessagingEnabled = true,
                    Number = namelessPersonMobilePhoneNumberNumber
                };

                existingPerson.PhoneNumbers.Add( existingPersonMobilePhoneNumber );
            }
            else
            {
                // A person should only have one Mobile Phone Number, and no more than one phone with Messaging enabled. (Rock enforces that in the Person Profile UI)
                // So, if they already have a Messaging Enabled Mobile Number, change it to the new linked number
                existingPersonMobilePhoneNumber.Number = namelessPersonMobilePhoneNumberNumber;
                existingPersonMobilePhoneNumber.IsMessagingEnabled = true;
            }

            // ensure they only have one SMS Number
            var otherSMSPhones = existingPerson.PhoneNumbers.Where( a => a != existingPersonMobilePhoneNumber && a.IsMessagingEnabled == true ).ToList();
            foreach ( var otherSMSPhone in otherSMSPhones )
            {
                otherSMSPhone.IsMessagingEnabled = false;
            }

            this.Context.SaveChanges();

            DoNamelessPersonToOtherPersonMerge( namelessPerson, existingPerson );
        }

        /// <summary>
        /// Merges the nameless person (see <see cref="Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS" />) to new person (and new Family) and saves changes to the database
        /// </summary>
        /// <param name="namelessPerson">The nameless person.</param>
        /// <param name="newPerson">The new person.</param>
        /// <param name="newPersonGroupRoleId">The new person group role identifier.</param>
        public void MergeNamelessPersonToNewPerson( Person namelessPerson, Person newPerson, int newPersonGroupRoleId )
        {
            int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            newPerson.PhoneNumbers = new List<PhoneNumber>();

            var namelessPersonMobilePhoneNumber = namelessPerson.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneTypeId );

            if ( namelessPersonMobilePhoneNumber != null )
            {
                // the person we are linking the phone number to doesn't have a SMS Messaging Number, so add a new one
                var newPersonMobilePhoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = mobilePhoneTypeId,
                    IsMessagingEnabled = true,
                    Number = namelessPersonMobilePhoneNumber.Number
                };

                newPerson.PhoneNumbers.Add( newPersonMobilePhoneNumber );
            }

            var groupMember = new GroupMember();
            groupMember.GroupRoleId = newPersonGroupRoleId;
            groupMember.Person = newPerson;

            var groupMembers = new List<GroupMember>();
            groupMembers.Add( groupMember );

            var rockContext = this.Context as RockContext;

            Group group = GroupService.SaveNewFamily( rockContext, groupMembers, null, true );

            DoNamelessPersonToOtherPersonMerge( namelessPerson, newPerson );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Does the actual merge of the nameless person (see <see cref="Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS"/>) to a target person.
        /// </summary>
        /// <param name="namelessPerson">The nameless person.</param>
        /// <param name="targetPerson">The target person.</param>
        private void DoNamelessPersonToOtherPersonMerge( Person namelessPerson, Person targetPerson )
        {
            int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            var namelessPersonMobilePhoneNumber = namelessPerson.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneTypeId );
            var phoneNumberService = new PhoneNumberService( this.Context as RockContext );

            if ( namelessPersonMobilePhoneNumber != null )
            {
                phoneNumberService.Delete( namelessPersonMobilePhoneNumber );
            }

            this.Context.SaveChanges();

            // Run merge proc to merge all associated data
            var parms = new Dictionary<string, object>();
            parms.Add( "OldId", namelessPerson.Id );
            parms.Add( "NewId", targetPerson.Id );
            DbService.ExecuteCommand( "spCrm_PersonMerge", CommandType.StoredProcedure, parms );
        }

        #endregion

        #region Get Person        
        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <returns>Person.</returns>
        public Person GetCurrentPerson()
        {
            var currentUser = new UserLoginService( ( RockContext ) this.Context ).GetByUserName( UserLogin.GetCurrentUserName() );
            return currentUser != null ? currentUser.Person : null;
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> by their PersonId
        /// </summary>
        /// <param name="id">The <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search for.</param>
        /// <param name="followMerges">A <see cref="System.Boolean"/> flag indicating that the provided PersonId should be checked against the <see cref="Rock.Model.PersonAlias"/> list.
        /// When <c>true</c> the <see cref="Rock.Model.PersonAlias"/> log will be checked for the PersonId, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> associated with the provided Id, otherwise null.</returns>
        public Person Get( int id, bool followMerges )
        {
            var person = Get( id );
            if ( person != null )
            {
                return person;
            }

            if ( followMerges )
            {
                var personAlias = new PersonAliasService( ( RockContext ) this.Context ).GetByAliasId( id );
                if ( personAlias != null )
                {
                    return personAlias.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> by their Guid.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the <see cref="Rock.Model.Person">Person's</see> Guid identifier.</param>
        /// <param name="followMerges">A <see cref="System.Boolean"/> flag indicating that the provided Guid should be checked against the <see cref="Rock.Model.PersonAlias"/> list.
        /// When <c>true</c> the <see cref="Rock.Model.PersonAlias"/> log will be checked for the Guid, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> associated with the provided Guid, otherwise null.</returns>
        public Person Get( Guid guid, bool followMerges )
        {
            var person = Get( guid );
            if ( person != null )
            {
                return person;
            }

            if ( followMerges )
            {
                var personAlias = new PersonAliasService( ( RockContext ) this.Context ).GetByAliasGuid( guid );
                if ( personAlias != null )
                {
                    return personAlias.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the Person by action identifier
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public Person GetByPersonActionIdentifier( string encryptedKey, string action )
        {
            string key = encryptedKey.Replace( '!', '%' );
            key = System.Web.HttpUtility.UrlDecode( key );
            string concatinatedKeys = Rock.Security.Encryption.DecryptString( key );
            if ( concatinatedKeys.IsNotNullOrWhiteSpace() )
            {
                string[] keyParts = concatinatedKeys.Split( '>' );
                if ( keyParts.Length == 2 )
                {
                    Guid guid = new Guid( keyParts[0] );
                    string actionPart = keyParts[1];

                    Person person = Get( guid );

                    if ( person != null && actionPart.Equals( action, StringComparison.OrdinalIgnoreCase ) )
                    {
                        return person;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Special override of Entity.GetByUrlEncodedKey for Person. Gets the Person by impersonation token (rckipid) and validates it against a Rock.Model.PersonToken
        /// NOTE: You might want to use GetByImpersonationToken instead to prevent a token from being used that was limited to a specific page
        /// The Person.Aliases will be eager-loaded.
        /// </summary>
        /// <param name="encodedKey">The encoded key.</param>
        /// <returns></returns>
        public override Person GetByUrlEncodedKey( string encodedKey )
        {
            return GetByImpersonationToken( encodedKey, false, null );
        }

        /// <summary>
        /// Gets the Person by impersonation token (rckipid) and validates it against a Rock.Model.PersonToken
        /// The Person.Aliases will be eager-loaded.
        /// </summary>
        /// <param name="impersonationToken">The impersonation token.</param>
        /// <param name="incrementUsage">if set to <c>true</c> [increment usage].</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public Person GetByImpersonationToken( string impersonationToken, bool incrementUsage, int? pageId )
        {
            return GetByEncryptedKey( impersonationToken, true, incrementUsage, pageId );
        }

        /// <summary>
        /// Gets the Person by impersonation token but does not validate against token properties (e.g. expiration)
        /// Use this method if needing to get the person from a token that may be expired.
        /// The Person.Aliases will be eager-loaded.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public Person GetByImpersonationToken( string encryptedKey )
        {
            // first, see if it exists as a PersonToken
            using ( var personTokenRockContext = new RockContext() )
            {
                var personToken = new PersonTokenService( personTokenRockContext ).GetByImpersonationToken( encryptedKey );
                if ( personToken != null )
                {
                    if ( personToken.PersonAlias != null )
                    {
                        // refetch using PersonService using rockContext instead of personTokenRockContext which was used to save the changes to personKey
                        return this.GetInclude( personToken.PersonAlias.PersonId, p => p.Aliases );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Special override of Entity.GetByEncryptedKey for Person. Gets the Person by impersonation token (rckipid) and validates it against a Rock.Model.PersonToken
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public override Person GetByEncryptedKey( string encryptedKey )
        {
            return GetByEncryptedKey( encryptedKey, true, true, null );
        }

        /// <summary>
        /// Special override of Entity.GetByEncryptedKey for Person. Gets the Person by impersonation token (rckipid) and validates it against a Rock.Model.PersonToken
        /// </summary>
        /// <param name="encryptedKey">A <see cref="System.String" /> containing an encrypted key value.</param>
        /// <param name="followMerges">if set to <c>true</c> [follow merges]. (only applies if using PersonTokenLegacyFallback)</param>
        /// <param name="incrementUsage">if set to <c>true</c> [increment usage].</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> associated with the provided Key, otherwise null.
        /// </returns>
        public Person GetByEncryptedKey( string encryptedKey, bool followMerges, bool incrementUsage, int? pageId )
        {
            // first, see if it exists as a PersonToken
            using ( var personTokenRockContext = new RockContext() )
            {
                var personToken = new PersonTokenService( personTokenRockContext ).GetByImpersonationToken( encryptedKey );
                if ( personToken != null )
                {
                    if ( incrementUsage )
                    {
                        personToken.TimesUsed++;
                        personToken.LastUsedDateTime = RockDateTime.Now;
                        personTokenRockContext.SaveChanges();
                    }

                    if ( personToken.UsageLimit.HasValue )
                    {
                        if ( personToken.TimesUsed > personToken.UsageLimit.Value )
                        {
                            // over usagelimit, so return null;
                            return null;
                        }
                    }

                    if ( personToken.ExpireDateTime.HasValue )
                    {
                        if ( personToken.ExpireDateTime.Value < RockDateTime.Now )
                        {
                            // expired, so return null
                            return null;
                        }
                    }

                    if ( personToken.PageId.HasValue && pageId.HasValue )
                    {
                        if ( personToken.PageId.Value != pageId.Value )
                        {
                            // personkey was for a specific page and this is not that page, so return null
                            return null;
                        }
                    }

                    if ( personToken.PersonAlias != null )
                    {
                        // re-fetch using PersonService using rockContext instead of personTokenRockContext which was used to save the changes to personKey
                        return this.GetInclude( personToken.PersonAlias.PersonId, p => p.Aliases );
                    }
                }
            }

            bool tokenUseLegacyFallback = GlobalAttributesCache.Get().GetValue( "core.PersonTokenUseLegacyFallback" ).AsBoolean();
            if ( tokenUseLegacyFallback )
            {
                return GetByLegacyEncryptedKey( encryptedKey, followMerges );
            }

            return null;
        }

        /// <summary>
        /// Gets the person from the user login identifier.
        /// </summary>
        /// <param name="userLoginId">The user login identifier.</param>
        /// <returns></returns>
        public Person GetByUserLoginId( int userLoginId )
        {
            UserLogin userLogin = new UserLoginService( new RockContext() ).Get( userLoginId );
            return Get( userLogin.PersonId.Value );
        }

        /// <summary>
        /// Looks up a person using a Pre-V7 PersonToken. 
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <param name="followMerges">if set to <c>true</c> [follow merges].</param>
        /// <returns></returns>
        internal Person GetByLegacyEncryptedKey( string encryptedKey, bool followMerges )
        {
            // it may have been urlencoded, but first try without urldecoding, just in case
            var person = base.GetByEncryptedKey( encryptedKey );
            if ( person == null )
            {
                string key = encryptedKey.Replace( '!', '%' );
                key = System.Web.HttpUtility.UrlDecode( key );
                person = base.GetByEncryptedKey( key );
            }

            if ( person != null )
            {
                if ( !person.IsPersonTokenUsageAllowed() )
                {
                    return null;
                }

                return person;
            }

            // NOTE: we only need the followMerges when using PersonTokenUseLegacyFallback since the PersonToken method would already take care of PersonMerge since it is keyed off of PersonAliasId
            if ( followMerges )
            {
                var personAlias = new PersonAliasService( this.Context as RockContext ).GetByAliasEncryptedKey( encryptedKey );
                if ( personAlias != null )
                {
                    if ( !personAlias.Person.IsPersonTokenUsageAllowed() )
                    {
                        return null;
                    }

                    return personAlias.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the family role (adult or child).
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public GroupTypeRole GetFamilyRole( Person person, RockContext rockContext = null )
        {
            var familyGroupRoles = GroupTypeCache.GetFamilyGroupType().Roles;
            int? groupTypeRoleId = null;
            if ( person.AgeClassification == AgeClassification.Adult )
            {
                groupTypeRoleId = familyGroupRoles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).FirstOrDefault()?.Id;
            }
            else if ( person.AgeClassification == AgeClassification.Child )
            {
                groupTypeRoleId = familyGroupRoles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).FirstOrDefault()?.Id;
            }

            rockContext = rockContext ?? new RockContext();

            if ( groupTypeRoleId.HasValue )
            {
                return new GroupTypeRoleService( rockContext ).Get( groupTypeRoleId.Value );
            }
            else
            {
                // just in case the AgeClassification method didn't work...
                var primaryFamilyId = person.GetFamily( rockContext )?.Id;
                if ( primaryFamilyId.HasValue )
                {
                    rockContext = rockContext ?? new RockContext();

                    return new GroupMemberService( rockContext ).Queryable()
                                            .Where( gm => gm.PersonId == person.Id && gm.GroupId == primaryFamilyId )
                                            .OrderBy( gm => gm.GroupOrder ?? int.MaxValue )
                                            .ThenBy( gm => gm.GroupRole.Order )
                                            .Select( gm => gm.GroupRole )
                                            .FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> entity of the provided Person's spouse.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> entity of the Person to retrieve the spouse of.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.</returns>
        public Person GetSpouse( Person person )
        {
            return GetSpouse( person, a => a.Person );
        }

        /// <summary>
        /// Gets a Person's spouse with a selector that lets you only fetch the properties that you need
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the spouse of.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.
        /// </returns>
        public TResult GetSpouse<TResult>( Person person, System.Linq.Expressions.Expression<Func<GroupMember, TResult>> selector )
        {
            //// Spouse is determined if all these conditions are met
            //// 1) Both Persons are adults in the same family (GroupType = Family, GroupRole = Adult, and in same Group)
            //// 2) Opposite Gender as Person, if Gender of both Persons is known
            //// 3) Both Persons are Married
            int marriedDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

            if ( person.MaritalStatusValueId != marriedDefinedValueId )
            {
                return default( TResult );
            }

            Guid adultGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
            int adultRoleId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Roles.First( a => a.Guid == adultGuid ).Id;

            // Businesses don't have a family role, so check for null before trying to get the Id.
            var familyRole = GetFamilyRole( person );
            if ( person.MaritalStatusValueId != marriedDefinedValueId || familyRole == null || familyRole.Id != adultRoleId )
            {
                return default( TResult );
            }

            var groupOrderQuery = new GroupMemberService( this.Context as RockContext ).Queryable();

            return GetFamilyMembers( person.Id )
                .Where( m => m.GroupRoleId == adultRoleId )

                // In the future, we may need to implement and check a GLOBAL Attribute "BibleStrict" with this logic: 
                .Where( m => m.Person.Gender != person.Gender || m.Person.Gender == Gender.Unknown || person.Gender == Gender.Unknown )
                .Where( m => m.Person.MaritalStatusValueId == marriedDefinedValueId )
                .OrderBy( m => groupOrderQuery.FirstOrDefault( x => x.GroupId == m.GroupId && x.PersonId == person.Id ).GroupOrder ?? int.MaxValue )
                .ThenBy( m => DbFunctions.DiffDays( m.Person.BirthDate ?? new DateTime( 1, 1, 1 ), person.BirthDate ?? new DateTime( 1, 1, 1 ) ) )
                .ThenBy( m => m.PersonId )
                .Select( selector )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> entity of the provided Person's head of household.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> entity of the Person to retrieve the head of household of.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> entity containing the provided Person's head of household. If the provided Person's family head of household is not found, this value will be null.</returns>
        public Person GetHeadOfHousehold( Person person )
        {
            var family = person.GetFamily( this.Context as RockContext );
            if ( family == null )
            {
                return null;
            }

            return GetFamilyMembers( family, person.Id, true )
                .Where( m => !m.Person.IsDeceased )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.Gender )
                .Select( a => a.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> entity of the provided Person's head of household.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> entity of the Person to retrieve the head of household of.</param>
        /// <param name="family">The <see cref="Rock.Model.Group"/> entity of the Group to retrieve the head of household of.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> entity containing the provided Person's head of household. If the provided Person's family head of household is not found, this value will be null.</returns>
        public Person GetHeadOfHousehold( Person person, Group family )
        {
            if ( family == null )
            {
                family = person.GetFamily( this.Context as RockContext );
            }

            if ( family == null )
            {
                return null;
            }

            return GetFamilyMembers( family, person.Id, true )
                .Where( m => !m.Person.IsDeceased )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.Gender )
                .Select( a => a.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the related people.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        /// <param name="roleIds">The role ids.</param>
        /// <returns></returns>
        public IEnumerable<GroupMember> GetRelatedPeople( List<int> personIds, List<int> roleIds )
        {
            var rockContext = ( RockContext ) this.Context;
            var groupMemberService = new GroupMemberService( rockContext );

            Guid groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid();
            Guid ownerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            var knownRelationshipGroups = groupMemberService
                .Queryable().AsNoTracking()
                .Where( m =>
                    m.Group.GroupType.Guid == groupTypeGuid &&
                    m.GroupRole.Guid == ownerRoleGuid &&
                    personIds.Contains( m.PersonId ) )
                .Select( m => m.GroupId );

            var related = groupMemberService
                .Queryable().AsNoTracking()
                .Where( g =>
                    knownRelationshipGroups.Contains( g.GroupId ) &&
                    roleIds.Contains( g.GroupRoleId ) &&
                    !personIds.Contains( g.PersonId ) )
                .ToList();

            return related;
        }

        #endregion

        #region Update Person

        /// <summary>
        /// Inactivates a person and adds additional info to the HistoryChangeList. The Person model already checks for and adds changes to the History table.
        /// Using the HistoryChangeList obj in this method's out param will create duplicate changes in the History table for "Record Status", "Record Status Reason", and "Inactive Reason Note".
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="reasonNote">The reason note.</param>
        /// <param name="historyChangeList">The history change list.</param>
        [RockObsolete( "1.12" )]
        [Obsolete( @"Use one of the InactivatePerson overloads without the HistoryChangeList out param. The Person model takes care of updating the HistoryChangeList.
            Using the HistoryChangeList obj in this method's out param will create duplicate changes in the History table for
            ""Record Status"", ""Record Status Reason"", and ""Inactive Reason Note""." )]
        public void InactivatePerson( Person person, DefinedValueCache reason, string reasonNote, out History.HistoryChangeList historyChangeList )
        {
            historyChangeList = new History.HistoryChangeList();

            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            if ( inactiveStatus != null && reason != null )
            {
                History.EvaluateChange( historyChangeList, "Record Status", person.RecordStatusValue?.Value, inactiveStatus.Value );
                History.EvaluateChange( historyChangeList, "Record Status Reason", person.RecordStatusReasonValue?.Value, reason.Value );
                History.EvaluateChange( historyChangeList, "Inactive Reason Note", person.InactiveReasonNote, reasonNote );

                person.RecordStatusValueId = inactiveStatus.Id;
                person.RecordStatusReasonValueId = reason.Id;
                person.InactiveReasonNote = reasonNote;
            }
        }

        /// <summary>
        /// Inactivates the person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="inactiveReasonDefinedValue">The inactive reason defined value.</param>
        /// <param name="inactiveReasonNote">The inactive reason note.</param>
        public void InactivatePerson( Person person, DefinedValueCache inactiveReasonDefinedValue, string inactiveReasonNote )
        {
            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            if ( inactiveStatus == null || inactiveReasonDefinedValue == null )
            {
                return;
            }

            // History is Checked in Person.PreSaveChanges() and saved in Person.PostSaveChanges().
            person.RecordStatusValueId = inactiveStatus.Id;
            person.RecordStatusReasonValueId = inactiveReasonDefinedValue.Id;
            person.InactiveReasonNote = inactiveReasonNote;
        }

        /// <summary>
        /// Inactivates the person. Use this one to also update ReviewReason and ReviewReasonNote.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="inactiveReasonDefinedValue">The inactive reason defined value.</param>
        /// <param name="inactiveReasonNote">The inactive reason note.</param>
        /// <param name="reviewReasonDefinedValue">The review reason defined value.</param>
        /// <param name="reviewReasonNote">The review reason note.</param>
        public void InactivatePerson( Person person, DefinedValueCache inactiveReasonDefinedValue, string inactiveReasonNote, DefinedValueCache reviewReasonDefinedValue, string reviewReasonNote )
        {
            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            if ( inactiveStatus == null || inactiveReasonDefinedValue == null || reviewReasonDefinedValue == null )
            {
                return;
            }

            // History is Checked in Person.PreSaveChanges() and saved in Person.PostSaveChanges().
            person.RecordStatusValueId = inactiveStatus.Id;
            person.RecordStatusReasonValueId = inactiveReasonDefinedValue.Id;
            person.InactiveReasonNote = inactiveReasonNote;
            person.ReviewReasonValueId = reviewReasonDefinedValue.Id;
            person.ReviewReasonNote = reviewReasonNote;
        }

        /// <summary>
        /// Configures the text to give settings including the person's preferred target financial account and default source saved account.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="contributionFinancialAccountId">The contribution financial account identifier.</param>
        /// <param name="financialPersonSavedAccountId">The financial person saved account identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool ConfigureTextToGive( int personId, int? contributionFinancialAccountId, int? financialPersonSavedAccountId, out string errorMessage )
        {
            errorMessage = string.Empty;

            // Validate the person
            var person = Get( personId );

            if ( person == null )
            {
                errorMessage = "The person ID is not valid";
                return false;
            }

            // Load the person's saved accounts
            var rockContext = Context as RockContext;
            var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var personsSavedAccounts = savedAccountService.Queryable()
                .Include( sa => sa.PersonAlias )
                .Where( sa => sa.PersonAlias.PersonId == personId )
                .ToList();

            // Loop through each saved account. Set default to false unless the args dictate that it is the default
            var foundDefaultAccount = false;

            foreach ( var savedAccount in personsSavedAccounts )
            {
                if ( !foundDefaultAccount && savedAccount.Id == financialPersonSavedAccountId )
                {
                    savedAccount.IsDefault = true;
                    foundDefaultAccount = true;
                }
                else
                {
                    savedAccount.IsDefault = false;
                }
            }

            // If the args specified an account to be default but it was not found, then return an error
            if ( financialPersonSavedAccountId.HasValue && !foundDefaultAccount )
            {
                errorMessage = "The saved account ID is not valid";
                return false;
            }

            // Validate the account if it is being set
            if ( contributionFinancialAccountId.HasValue )
            {
                var accountService = new FinancialAccountService( rockContext );
                var account = accountService.Get( contributionFinancialAccountId.Value );

                if ( account == null )
                {
                    errorMessage = "The financial account ID is not valid";
                    return false;
                }

                if ( !account.IsActive )
                {
                    errorMessage = "The financial account is not active";
                    return false;
                }

                if ( account.IsPublic.HasValue && !account.IsPublic.Value )
                {
                    errorMessage = "The financial account is not public";
                    return false;
                }
            }

            // Set the person's contribution account ID
            person.ContributionFinancialAccountId = contributionFinancialAccountId;

            // Success
            return true;
        }

        /// <summary>
        /// Expunge Person for programmatically merging a given person into the Anonymous Giver record.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public void ExpungePerson( int personId )
        {
            int? anonymousPersonId = null;

            var rockContext = new RockContext();
            try
            {
                rockContext.WrapTransaction( () =>
                {
                    var personService = new PersonService( rockContext );
                    var userLoginService = new UserLoginService( rockContext );
                    var groupService = new GroupService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );
                    var binaryFileService = new BinaryFileService( rockContext );
                    var phoneNumberService = new PhoneNumberService( rockContext );
                    var taggedItemService = new TaggedItemService( rockContext );
                    var personSearchKeyService = new PersonSearchKeyService( rockContext );

                    var anonymousPersonGuid = Guid.Parse( SystemGuid.Person.GIVER_ANONYMOUS );
                    Person anonymousPerson = personService.Get( anonymousPersonGuid );
                    Person expungePerson = personService.Get( personId );
                    if ( anonymousPerson != null && expungePerson != null )
                    {
                        anonymousPersonId = anonymousPerson.Id;

                        // Write a history record about the merge
                        var changes = new History.HistoryChangeList();
                        changes.AddChange( History.HistoryVerb.Merge, History.HistoryChangeType.Record, string.Format( "{0} [ID: {1}]", expungePerson.FullName, expungePerson.Id ) );

                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), anonymousPerson.Id, changes );

                        // Photo Id
                        anonymousPerson.PhotoId = GetSelectedValue( anonymousPerson.PhotoId, expungePerson.PhotoId );
                        anonymousPerson.TitleValueId = GetSelectedValue( anonymousPerson.TitleValueId, expungePerson.TitleValueId );
                        anonymousPerson.FirstName = GetSelectedValue( anonymousPerson.FirstName, expungePerson.FirstName );
                        anonymousPerson.NickName = GetSelectedValue( anonymousPerson.NickName, expungePerson.NickName );
                        anonymousPerson.MiddleName = GetSelectedValue( anonymousPerson.MiddleName, expungePerson.MiddleName );
                        anonymousPerson.LastName = GetSelectedValue( anonymousPerson.LastName, expungePerson.LastName );
                        anonymousPerson.SuffixValueId = GetSelectedValue( anonymousPerson.SuffixValueId, expungePerson.SuffixValueId );
                        anonymousPerson.RecordTypeValueId = GetSelectedValue( anonymousPerson.SuffixValueId, expungePerson.SuffixValueId );
                        anonymousPerson.RecordStatusValueId = GetSelectedValue( anonymousPerson.RecordStatusValueId, expungePerson.RecordStatusValueId );
                        anonymousPerson.RecordStatusReasonValueId = GetSelectedValue( anonymousPerson.RecordStatusReasonValueId, expungePerson.RecordStatusReasonValueId );
                        anonymousPerson.ConnectionStatusValueId = GetSelectedValue( anonymousPerson.ConnectionStatusValueId, expungePerson.ConnectionStatusValueId );
                        anonymousPerson.Gender = anonymousPerson.Gender != Gender.Unknown ? anonymousPerson.Gender : expungePerson.Gender;
                        anonymousPerson.MaritalStatusValueId = GetSelectedValue( anonymousPerson.MaritalStatusValueId, expungePerson.MaritalStatusValueId );
                        if ( !anonymousPerson.BirthDate.HasValue )
                        {
                            anonymousPerson.SetBirthDate( expungePerson.BirthDate );
                        }

                        anonymousPerson.AnniversaryDate = anonymousPerson.AnniversaryDate.HasValue ? anonymousPerson.AnniversaryDate : expungePerson.AnniversaryDate;
                        anonymousPerson.GraduationYear = GetSelectedValue( anonymousPerson.GraduationYear, expungePerson.GraduationYear );
                        anonymousPerson.Email = GetSelectedValue( anonymousPerson.Email, expungePerson.Email );
                        anonymousPerson.EmailNote = GetSelectedValue( anonymousPerson.EmailNote, expungePerson.EmailNote );
                        anonymousPerson.EmailPreference = anonymousPerson.EmailPreference != EmailPreference.EmailAllowed ? anonymousPerson.EmailPreference : expungePerson.EmailPreference;
                        anonymousPerson.InactiveReasonNote = GetSelectedValue( anonymousPerson.InactiveReasonNote, expungePerson.InactiveReasonNote );
                        anonymousPerson.SystemNote = GetSelectedValue( anonymousPerson.SystemNote, expungePerson.SystemNote );
                        anonymousPerson.ContributionFinancialAccountId = GetSelectedValue( anonymousPerson.ContributionFinancialAccountId, expungePerson.ContributionFinancialAccountId );

                        // Update phone numbers
                        var phoneTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues;
                        foreach ( var phoneType in phoneTypes )
                        {
                            var anonymousPersonPhoneNumber = anonymousPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == phoneType.Id ).FirstOrDefault();
                            if ( anonymousPersonPhoneNumber == null )
                            {
                                var expungePersonphoneNumber = expungePerson.PhoneNumbers.Where( p => p.NumberTypeValueId == phoneType.Id ).FirstOrDefault();

                                // New phone doesn't match old
                                if ( expungePersonphoneNumber != null )
                                {
                                    // Old value didn't exist... create new phone record
                                    anonymousPersonPhoneNumber = new PhoneNumber { NumberTypeValueId = phoneType.Id };
                                    anonymousPerson.PhoneNumbers.Add( anonymousPersonPhoneNumber );

                                    // Update phone number
                                    anonymousPersonPhoneNumber.Number = expungePersonphoneNumber.Number;
                                }
                            }
                        }

                        // Save the new record
                        rockContext.SaveChanges();

                        // Update the attributes
                        anonymousPerson.LoadAttributes( rockContext );
                        expungePerson.LoadAttributes( rockContext );

                        foreach ( var attribute in expungePerson.Attributes.OrderBy( a => a.Value.Order ) )
                        {
                            string value = expungePerson.GetAttributeValue( attribute.Key );
                            if ( value.IsNotNullOrWhiteSpace() )
                            {
                                string primaryValue = anonymousPerson.GetAttributeValue( attribute.Key );

                                if ( primaryValue.IsNullOrWhiteSpace() )
                                {
                                    Rock.Attribute.Helper.SaveAttributeValue( anonymousPerson, attribute.Value, value, rockContext );
                                }
                            }
                        }

                        // Update the family attributes
                        var anonymousFamily = anonymousPerson.GetFamily( rockContext );
                        var expungePersonFamily = expungePerson.GetFamily( rockContext );

                        if ( expungePersonFamily != null && expungePersonFamily != null )
                        {
                            anonymousFamily.Name = GetSelectedValue( anonymousFamily.Name, expungePersonFamily.Name );
                            anonymousFamily.CampusId = GetSelectedValue( anonymousFamily.CampusId, expungePersonFamily.CampusId );

                            anonymousFamily.LoadAttributes( rockContext );
                            expungePersonFamily.LoadAttributes( rockContext );

                            foreach ( var attribute in expungePersonFamily.Attributes.OrderBy( a => a.Value.Order ) )
                            {
                                string value = expungePersonFamily.GetAttributeValue( attribute.Key );
                                if ( value.IsNotNullOrWhiteSpace() )
                                {
                                    string primaryValue = anonymousFamily.GetAttributeValue( attribute.Key );

                                    if ( primaryValue.IsNullOrWhiteSpace() )
                                    {
                                        Rock.Attribute.Helper.SaveAttributeValue( anonymousFamily, attribute.Value, value, rockContext );
                                    }
                                }
                            }
                        }

                        rockContext.SaveChanges();

                        // Merge search keys on merge
                        if ( expungePerson.Email.IsNotNullOrWhiteSpace() )
                        {
                            var searchTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() );
                            var personSearchKeys = anonymousPerson.GetPersonSearchKeys( rockContext ).Where( a => a.SearchTypeValueId == searchTypeValue.Id && a.SearchValue == expungePerson.Email ).ToList();
                            if ( !string.IsNullOrEmpty( expungePerson.Email ) && expungePerson.Email != anonymousPerson.Email && !personSearchKeys.Any() )
                            {
                                PersonSearchKey personSearchKey = new PersonSearchKey()
                                {
                                    PersonAliasId = anonymousPerson.PrimaryAliasId.Value,
                                    SearchTypeValueId = searchTypeValue.Id,
                                    SearchValue = expungePerson.Email
                                };
                                personSearchKeyService.Add( personSearchKey );
                                rockContext.SaveChanges();
                            }

                            var mergeSearchKeys = personService.GetPersonSearchKeys( expungePerson.Id ).Where( a => a.SearchTypeValueId == searchTypeValue.Id ).ToList();
                            var duplicateKeys = mergeSearchKeys.Where( a => personSearchKeys.Any( b => b.SearchValue.Equals( a.SearchValue, StringComparison.OrdinalIgnoreCase ) ) );

                            if ( duplicateKeys.Any() )
                            {
                                personSearchKeyService.DeleteRange( duplicateKeys );
                                rockContext.SaveChanges();
                            }
                        }

                        // Delete the merged person's phone numbers (we've already updated the anonymous person's values)
                        foreach ( var phoneNumber in phoneNumberService.GetByPersonId( expungePerson.Id ) )
                        {
                            phoneNumberService.Delete( phoneNumber );
                        }

                        rockContext.SaveChanges();

                        // Delete the merged person's other family member records and the family if they were the only one in the family
                        Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                        foreach ( var familyMember in groupMemberService.Queryable().Where( m => m.PersonId == expungePerson.Id && m.Group.GroupType.Guid == familyGuid ) )
                        {
                            groupMemberService.Delete( familyMember );

                            rockContext.SaveChanges();

                            // Get the family
                            var family = groupService.Queryable( "Members" ).Where( f => f.Id == familyMember.GroupId ).FirstOrDefault();
                            if ( !family.Members.Any() )
                            {
                                // If there are not any other family members, delete the family record.

                                // If theres any people that have this group as a giving group, set it to null (the person being merged should be the only one)
                                foreach ( Person gp in personService.Queryable().Where( g => g.GivingGroupId == family.Id ) )
                                {
                                    gp.GivingGroupId = null;
                                }

                                // save to the database prior to doing groupService.Delete since .Delete quietly might not delete if thinks the Family is used for a GivingGroupId
                                rockContext.SaveChanges();

                                // Delete the family
                                string errorMessage;
                                if ( groupService.CanDelete( family, out errorMessage ) )
                                {
                                    groupService.Delete( family );
                                    rockContext.SaveChanges();
                                }
                            }
                        }

                        // Flush any security roles that the merged person's other records were a part of
                        foreach ( var groupMember in groupMemberService.Queryable().Where( m => m.PersonId == expungePerson.Id ) )
                        {
                            Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
                            if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                            {
                                RoleCache.Remove( group.Id );
                                Rock.Security.Authorization.Clear();
                            }
                        }

                        RemoveAnonymousGiverUserLogins( userLoginService, rockContext );
                    }
                } );

                // Run merge proc to merge all associated data
                var parms = new Dictionary<string, object>();
                parms.Add( "OldId", personId );
                parms.Add( "NewId", anonymousPersonId.Value );
                DbService.ExecuteCommand( "spCrm_PersonMerge", CommandType.StoredProcedure, parms );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Removes any UserLogin records associated with the Anonymous Giver.
        /// </summary>
        private void RemoveAnonymousGiverUserLogins( UserLoginService userLoginService, RockContext rockContext )
        {
            var anonymousGiver = new PersonService( rockContext ).Get( Rock.SystemGuid.Person.GIVER_ANONYMOUS.AsGuid() );

            var logins = userLoginService.Queryable()
                .Where( l => l.PersonId == anonymousGiver.Id );

            userLoginService.DeleteRange( logins );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Removes duplicate and empty number phone numbers from a person.
        /// NOTE: This method deletes the all the Person's phone numbers which have a phone number type not present in the provided
        /// phoneNumberTypeIds list of phone number types.
        /// </summary>
        /// <param name="person">The Person.</param>
        /// <param name="phoneNumberTypeIds">The list of phone number type ids. The phone numbers with a phone number type not in this list would be deleted.</param>
        /// <param name="rockContext">The rock context.</param>
        public void RemoveEmptyAndDuplicatePhoneNumbers( Person person, List<int> phoneNumberTypeIds, RockContext rockContext )
        {
            var phoneNumberService = new PhoneNumberService( rockContext );

            // Remove any empty numbers
            foreach ( var phoneNumber in person.PhoneNumbers
                            .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                            .ToList() )
            {
                person.PhoneNumbers.Remove( phoneNumber );
                phoneNumberService.Delete( phoneNumber );
            }

            // Remove any duplicate numbers
            var isDuplicate = person.PhoneNumbers.GroupBy( pn => pn.Number ).Where( g => g.Count() > 1 ).Any();

            if ( isDuplicate )
            {
                var listOfValidNumbers = person.PhoneNumbers.OrderBy( o => o.NumberTypeValueId ).GroupBy( pn => pn.Number ).Select( y => y.First() ).ToList();
                var removedNumbers = person.PhoneNumbers.Except( listOfValidNumbers ).ToList();
                phoneNumberService.DeleteRange( removedNumbers );
                person.PhoneNumbers = listOfValidNumbers;
            }
        }

        /// <summary>
        /// Gets the selected value.
        /// </summary>
        /// <param name="anonymousPersonValue">Anonymous person value.</param>
        /// <param name="expungePersonValue">Expunge person value.</param>
        /// <returns></returns>
        private T GetSelectedValue<T>( T anonymousPersonValue, T expungePersonValue )
        {
            var type = typeof( T );
            if ( ( type == typeof( string ) && string.IsNullOrWhiteSpace( anonymousPersonValue as string ) ) ||
                 ( ( Nullable.GetUnderlyingType( type ) != null || type.IsClass ) && anonymousPersonValue == null ) )
            {
                return expungePersonValue;
            }

            return anonymousPersonValue;
        }

        #endregion

        #region Person Group Methods

        /// <summary>
        /// Gets the peer network group.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public Group GetPeerNetworkGroup( int personId )
        {
            var peerNetworkGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK.AsGuid() );
            var impliedOwnerRole = peerNetworkGroupType.Roles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid() ).FirstOrDefault();

            var rockContext = this.Context as RockContext;

            var peerNetworkGroup = new GroupMemberService( rockContext ).Queryable()
                                    .Where(
                                        m => m.PersonId == personId
                                        && m.GroupRoleId == impliedOwnerRole.Id
                                        && m.Group.GroupTypeId == peerNetworkGroupType.Id )
                                    .Select( m => m.Group )
                                    .FirstOrDefault();

            // It's possible that a implied group does not exist for this person due to poor migration from a different system or a manual insert of the data
            if ( peerNetworkGroup == null )
            {
                // Create the new peer network group using a new context so as not to save changes in the current one
                using ( var rockContextClean = new RockContext() )
                {
                    var groupServiceClean = new GroupService( rockContextClean );

                    var groupMember = new GroupMember();
                    groupMember.PersonId = personId;
                    groupMember.GroupRoleId = impliedOwnerRole.Id;

                    var peerNetworkGroupClean = new Group();
                    peerNetworkGroupClean.Name = peerNetworkGroupType.Name;
                    peerNetworkGroupClean.GroupTypeId = peerNetworkGroupType.Id;
                    peerNetworkGroupClean.Members.Add( groupMember );

                    groupServiceClean.Add( peerNetworkGroupClean );
                    rockContextClean.SaveChanges();

                    // Get the new peer network group using the original context
                    peerNetworkGroup = new GroupService( rockContext ).Get( peerNetworkGroupClean.Id );
                }
            }

            return peerNetworkGroup;
        }

        #endregion

        /// <summary>
        /// Gets all of the IsMappedLocation points for a given user. Although each family can only have one 
        /// IsMapped point, the person may belong to more than one family
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<DbGeography> GetGeopoints( int personId )
        {
            var rockContext = ( RockContext ) this.Context;
            var groupMemberService = new GroupMemberService( rockContext );

            int groupTypeFamilyId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            // get the geopoints for the family locations for the selected person
            return groupMemberService
                .Queryable( true ).AsNoTracking()
                .Where( m =>
                    m.PersonId == personId &&
                    m.Group.GroupTypeId == groupTypeFamilyId )
                .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                .SelectMany( m => m.Group.GroupLocations )
                .Where( l =>
                    l.IsMappedLocation &&
                    l.Location.GeoPoint != null )
                .Select( l => l.Location.GeoPoint );
        }

        #region Static Methods

        /// <summary>
        /// Updates Person.BirthDate for each person that is not deceased or inactive, and has a non-null <seealso cref="Person.BirthYear"/> greater than 1800
        /// </summary>
        public static void UpdateBirthDateAll( RockContext rockContext = null )
        {
            var inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            // NOTE: if BirthYear is 1800 or earlier, set BirthDate to null so that database Age calculations don't get an exception
            string sql = $@"
                UPDATE Person
                    SET [BirthDate] = (
		                    CASE 
			                    WHEN ([BirthYear] IS NOT NULL AND [BirthYear] > 1800)
				                    THEN TRY_CONVERT([date], (((CONVERT([varchar], [BirthYear]) + '-') + CONVERT([varchar], [BirthMonth])) + '-') + CONVERT([varchar], [BirthDay]), (126))
			                    ELSE NULL
			                    END
		                    )
                    FROM Person
                    WHERE [BirthDate] != (
		                    CASE 
			                    WHEN ([BirthYear] IS NOT NULL AND [BirthYear] > 1800)
				                    THEN TRY_CONVERT([date], (((CONVERT([varchar], [BirthYear]) + '-') + CONVERT([varchar], [BirthMonth])) + '-') + CONVERT([varchar], [BirthDay]), (126))
			                    ELSE NULL
			                    END
		                    )
                    AND IsDeceased = 0
                    AND RecordStatusValueId <> {inactiveStatusId}";

            rockContext = rockContext ?? new RockContext();
            using ( rockContext )
            {
                rockContext.Database.ExecuteSqlCommand( sql );
            }
        }

        /// <summary>
        /// Adds a person alias, known relationship group, implied relationship group, and family for a new person.
        /// Returns the new Family(Group) that was created for the person. The Person and Family are saved to the database.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="savePersonAttributes">if set to <c>true</c> [save person attributes].</param>
        /// <returns>Family Group</returns>
        public static Group SaveNewPerson( Person person, RockContext rockContext, int? campusId = null, bool savePersonAttributes = false )
        {
            // Since business names can have unique casing as a part of their brands (IBM, asana) don't auto
            // correct the casing of business names on add
            var businessRecordTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS ).Id;
            if ( person.RecordTypeValueId != businessRecordTypeValueId )
            {
                person.FirstName = person.FirstName.FixCase();
                person.NickName = person.NickName.FixCase();
                person.MiddleName = person.MiddleName.FixCase();
                person.LastName = person.LastName.FixCase();
            }

            // Create/Save Known Relationship Group
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            if ( knownRelationshipGroupType != null )
            {
                var ownerRole = knownRelationshipGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) );
                if ( ownerRole != null )
                {
                    var groupMember = new GroupMember();
                    groupMember.Person = person;
                    groupMember.GroupRoleId = ownerRole.Id;

                    var group = new Group();
                    group.Name = knownRelationshipGroupType.Name;
                    group.GroupTypeId = knownRelationshipGroupType.Id;
                    group.Members.Add( groupMember );

                    var groupService = new GroupService( rockContext );
                    groupService.Add( group );
                }
            }

            // Create/Save Implied Relationship Group
            var impliedRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK );
            if ( impliedRelationshipGroupType != null )
            {
                var ownerRole = impliedRelationshipGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid() ) );
                if ( ownerRole != null )
                {
                    var groupMember = new GroupMember();
                    groupMember.Person = person;
                    groupMember.GroupRoleId = ownerRole.Id;

                    var group = new Group();
                    group.Name = impliedRelationshipGroupType.Name;
                    group.GroupTypeId = impliedRelationshipGroupType.Id;
                    group.Members.Add( groupMember );

                    var groupService = new GroupService( rockContext );
                    groupService.Add( group );
                }
            }

            // Create/Save family
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( familyGroupType != null )
            {
                var adultRole = familyGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

                var childRole = familyGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) );

                var age = person.Age;

                var familyRole = age.HasValue && age < 18 ? childRole : adultRole;

                if ( familyRole != null )
                {
                    var groupMember = new GroupMember();
                    groupMember.Person = person;
                    groupMember.GroupRoleId = familyRole.Id;

                    var groupMembers = new List<GroupMember>();
                    groupMembers.Add( groupMember );

                    return GroupService.SaveNewFamily( rockContext, groupMembers, campusId, savePersonAttributes );
                }
            }

            // shouldn't happen unless somehow the core FamilyGroupType and Roles are missing
            return null;
        }

        /// <summary>
        /// Adds the person to family and saves changes to the database
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="newPerson">if set to <c>true</c> [new person].</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void AddPersonToFamily( Person person, bool newPerson, int familyId, int groupRoleId, RockContext rockContext )
        {
            AddPersonToGroup( person, newPerson, familyId, groupRoleId, rockContext );
        }

        /// <summary>
        /// Adds the person to group and saves changes to the database
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="newPerson">if set to <c>true</c> [new person].</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <exception cref="System.Exception">
        /// Unable to find group with Id  + groupId.ToString()
        /// or
        /// Person is already in the specified group
        /// or
        /// </exception>
        public static void AddPersonToGroup( Person person, bool newPerson, int groupId, int groupRoleId, RockContext rockContext )
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();

            var demographicChanges = new History.HistoryChangeList();
            var memberChanges = new History.HistoryChangeList();
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( groupId );
            if ( group == null )
            {
                throw new Exception( "Unable to find group with Id " + groupId.ToString() );
            }

            bool isFamilyGroup = group.GroupTypeId == familyGroupType.Id;

            var groupMemberService = new GroupMemberService( rockContext );

            // make sure the person isn't in the group already
            bool alreadyInGroup = groupMemberService.Queryable().Any( a => a.GroupId == groupId && a.PersonId == person.Id );
            if ( alreadyInGroup )
            {
                throw new Exception( "Person is already in the specified group" );
            }

            var groupMember = new GroupMember();
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            groupMember.Person = person;
            groupMember.GroupRoleId = groupRoleId;

            if ( newPerson )
            {
                person.FirstName = person.FirstName.FixCase();
                person.NickName = person.NickName.FixCase();
                person.MiddleName = person.MiddleName.FixCase();
                person.LastName = person.LastName.FixCase();

                // new person that hasn't be saved to database yet
                demographicChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, person.FullName );
                History.EvaluateChange( demographicChanges, "Title", string.Empty, DefinedValueCache.GetName( person.TitleValueId ) );
                History.EvaluateChange( demographicChanges, "First Name", string.Empty, person.FirstName );
                History.EvaluateChange( demographicChanges, "Last Name", string.Empty, person.LastName );
                History.EvaluateChange( demographicChanges, "Suffix", string.Empty, DefinedValueCache.GetName( person.SuffixValueId ) );
                History.EvaluateChange( demographicChanges, "Gender", null, person.Gender );
                History.EvaluateChange( demographicChanges, "Marital Status", string.Empty, DefinedValueCache.GetName( person.MaritalStatusValueId ) );
                History.EvaluateChange( demographicChanges, "Birth Date", null, person.BirthDate );
                History.EvaluateChange( demographicChanges, "Graduation Year", null, person.GraduationYear );
                History.EvaluateChange( demographicChanges, "Connection Status", string.Empty, DefinedValueCache.GetName( person.ConnectionStatusValueId ) );
                History.EvaluateChange( demographicChanges, "Email Active", true.ToString(), person.IsEmailActive.ToString() );
                History.EvaluateChange( demographicChanges, "Record Type", string.Empty, DefinedValueCache.GetName( person.RecordTypeValueId ) );
                if ( person.GivingGroupId.HasValue )
                {
                    person.GivingGroup = person.GivingGroup ?? groupService.Get( person.GivingGroupId.Value );
                    if ( person.GivingGroup != null )
                    {
                        History.EvaluateChange( demographicChanges, "Giving Group", string.Empty, person.GivingGroup.Name );
                    }
                }

                History.EvaluateChange( demographicChanges, "Record Status", string.Empty, DefinedValueCache.GetName( person.RecordStatusValueId ) );
                History.EvaluateChange( demographicChanges, "Record Status Reason", string.Empty, DefinedValueCache.GetName( groupMember.Person.RecordStatusReasonValueId ) );

                groupMember.Person = person;
            }
            else
            {
                // added from other group
                groupMember.Person = person;
            }

            groupMember.GroupId = groupId;
            groupMember.GroupRoleId = groupRoleId;
            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            var role = groupType.Roles.FirstOrDefault( a => a.Id == groupRoleId );

            if ( role != null )
            {
                History.EvaluateChange( memberChanges, "Role", string.Empty, role.Name );
            }
            else
            {
                throw new Exception( string.Format( "Specified groupRoleId ({0}) is not a {1} role ", groupRoleId, group.GroupType.Name ) );
            }

            if ( !groupMember.IsValidGroupMember( rockContext ) )
            {
                throw new GroupMemberValidationException( groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" ) );
            }

            groupMemberService.Add( groupMember );

            rockContext.SaveChanges();

            HistoryService.SaveChanges(
                rockContext,
                typeof( Person ),
                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                groupMember.Person.Id,
                demographicChanges,
                false,
                null,
                rockContext.SourceOfChange );

            if ( isFamilyGroup )
            {
                HistoryService.SaveChanges(
                    rockContext,
                    typeof( Person ),
                    Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                    groupMember.Person.Id,
                    memberChanges,
                    group.Name,
                    typeof( Group ),
                    groupId,
                    false,
                    null,
                    rockContext.SourceOfChange );
            }
        }

        /// <summary>
        /// Removes the type of the person from other groups of.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <exception cref="System.Exception">Group does not exist, or person is not in the specified group</exception>
        public static void RemovePersonFromOtherGroupsOfType( int groupId, int personId, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( groupId );

            // make sure the group exists, and person belong to the specified group before deleting them from other groups
            if ( group == null || !group.Members.Any( m => m.PersonId == personId ) )
            {
                throw new Exception( "Group does not exist, or person is not in the specified group" );
            }

            var memberInOtherFamilies = groupMemberService.Queryable( true )
                .Where( m =>
                    m.PersonId == personId &&
                    m.Group.GroupTypeId == group.GroupTypeId &&
                    m.GroupId != groupId )
                    .Select( a => new
                    {
                        GroupMember = a,
                        a.Group,
                        a.GroupRole,
                        a.GroupId,
                        a.Person
                    } )
                .ToList();

            foreach ( var fm in memberInOtherFamilies )
            {
                // If the person's giving group id was the family they are being removed from, update it to this new family's id
                if ( fm.Person.GivingGroupId == fm.GroupId )
                {
                    var person = fm.Person;

                    var demographicChanges = new History.HistoryChangeList();
                    History.EvaluateChange( demographicChanges, "Giving Group", person.GivingGroup.Name, group.Name );
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        person.Id,
                        demographicChanges,
                        false,
                        null,
                        rockContext.SourceOfChange );

                    person.GivingGroupId = groupId;
                    rockContext.SaveChanges();
                }

                if ( group.GroupTypeId == GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id )
                {
                    var oldMemberChanges = new History.HistoryChangeList();
                    History.EvaluateChange( oldMemberChanges, "Role", fm.GroupRole.Name, string.Empty );
                    History.EvaluateChange( oldMemberChanges, "Family", fm.Group.Name, string.Empty );
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                        fm.Person.Id,
                        oldMemberChanges,
                        fm.Group.Name,
                        typeof( Group ),
                        fm.Group.Id );
                }

                groupMemberService.Delete( fm.GroupMember );
                rockContext.SaveChanges();

                // delete family if it doesn't have anybody in it anymore
                var otherFamily = groupService.Queryable()
                    .Where( g =>
                        g.Id == fm.GroupId &&
                        !g.Members.Any() )
                    .FirstOrDefault();
                if ( otherFamily != null )
                {
                    groupService.Delete( otherFamily );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Removes the person from other families, then deletes the other families if nobody is left in them
        /// </summary>
        /// <param name="familyId">The groupId of the family that they should stay in</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void RemovePersonFromOtherFamilies( int familyId, int personId, RockContext rockContext )
        {
            RemovePersonFromOtherGroupsOfType( familyId, personId, rockContext );
        }

        /// <summary>
        /// Updates the person profile photo.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="photoBytes">The photo bytes.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The new person profile image (built with the Public Application Root), or an empty string if something went wrong.</returns>
        [RockInternal( "1.15" )]
        internal static string UpdatePersonProfilePhoto( Guid personGuid, byte[] photoBytes, string filename, RockContext rockContext = null )
        {
            // If rockContext is null, create a new RockContext object.
            rockContext = rockContext ?? new RockContext();

            // Get the Person object using the unique identifier.
            var person = new PersonService( rockContext ).Get( personGuid );

            // If the Person object is null, return an empty string.
            if ( person == null )
            {
                return string.Empty;
            }

            // If photoBytes is empty or filename is null or whitespace, return an empty string.
            if ( photoBytes.Length == 0 || string.IsNullOrWhiteSpace( filename ) )
            {
                return string.Empty;
            }

            // Create an array of illegal characters for filenames.
            char[] illegalCharacters = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

            // If filename contains any of the illegal characters, return an empty string.
            if ( filename.IndexOfAny( illegalCharacters ) >= 0 )
            {
                return string.Empty;
            }

            // Get the BinaryFileType object for person images.
            BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );

            // Create a new BinaryFile object and add it to the BinaryFileService.
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = new BinaryFile();
            binaryFileService.Add( binaryFile );

            // Set properties for the new BinaryFile object.
            binaryFile.IsTemporary = false;
            binaryFile.BinaryFileTypeId = binaryFileType.Id;
            binaryFile.MimeType = "octet/stream";
            binaryFile.FileSize = photoBytes.Length;
            binaryFile.FileName = filename;
            binaryFile.ContentStream = new MemoryStream( photoBytes );

            // Save changes to the RockContext.
            rockContext.SaveChanges();

            // Store the old photo ID for the person.
            int? oldPhotoId = person.PhotoId;

            // Set the person's photo ID to the ID of the new BinaryFile object.
            person.PhotoId = binaryFile.Id;

            // Save changes to the RockContext.
            rockContext.SaveChanges();

            // If the person had an old photo ID, mark the old BinaryFile as temporary and save changes.
            if ( oldPhotoId.HasValue )
            {
                binaryFile = binaryFileService.Get( oldPhotoId.Value );
                binaryFile.IsTemporary = true;

                rockContext.SaveChanges();
            }

            // Return the URL for the person's new photo, built with the PublicApplicationRoot global attribute.
            return $"{GlobalAttributesCache.Value( "PublicApplicationRoot" )}{person.PhotoUrl}";
        }

        #endregion

        #region User Preferences

        /// <summary>
        /// Gets the prefix for a user preference key that includes the block id so that it specific to the specified block
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use PersonPreferenceCache to access preferences instead." )]
        public static string GetBlockUserPreferenceKeyPrefix( int blockId )
        {
            return $"block-{blockId}-";
        }

        /// <summary>
        /// Saves a <see cref="Rock.Model.Person">Person's</see> user preference setting by key and SavesChanges()
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> who the preference value belongs to.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key (name) of the preference setting.</param>
        /// <param name="value">The value.</param>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use PersonPreferenceCache to access preferences instead." )]
        public static void SaveUserPreference( Person person, string key, string value )
        {
            var anonymousVisitorGuid = new Guid( SystemGuid.Person.ANONYMOUS_VISITOR );
            PersonPreferenceCollection preferences;

            if ( person == null || !person.PrimaryAliasId.HasValue || person.Guid == anonymousVisitorGuid )
            {
                return;
            }

            preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

            preferences.SetValue( key, value );
            preferences.Save();
        }

        /// <summary>
        /// Saves the user preferences.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="preferences">The preferences.</param>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use PersonPreferenceCache to access preferences instead." )]
        public static void SaveUserPreferences( Person person, Dictionary<string, string> preferences )
        {
            var anonymousVisitorGuid = new Guid( SystemGuid.Person.ANONYMOUS_VISITOR );
            PersonPreferenceCollection preferenceCollection;

            if ( person == null || preferences == null || !person.PrimaryAliasId.HasValue || person.Guid == anonymousVisitorGuid )
            {
                return;
            }

            preferenceCollection = PersonPreferenceCache.GetPersonPreferenceCollection( person );

            foreach ( var kvp in preferences )
            {
                preferenceCollection.SetValue( kvp.Key, kvp.Value );
            }

            preferenceCollection.Save();
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> user preference value by preference setting's key.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the preference value for.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key name of the preference setting.</param>
        /// <returns>A list of <see cref="System.String"/> containing the values associated with the user's preference setting.</returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use PersonPreferenceCache to access preferences instead." )]
        public static string GetUserPreference( Person person, string key )
        {
            var anonymousVisitorGuid = new Guid( SystemGuid.Person.ANONYMOUS_VISITOR );
            PersonPreferenceCollection preferences;

            if ( person == null || !person.PrimaryAliasId.HasValue || person.Guid == anonymousVisitorGuid )
            {
                return string.Empty;
            }

            preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

            return preferences.GetValue( key );
        }

        /// <summary>
        /// Deletes a <see cref="Rock.Model.Person">Person's</see> user preference setting by key and SavesChanges()
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> who the preference value belongs to.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key (name) of the preference setting.</param>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use PersonPreferenceCache to access preferences instead." )]
        public static void DeleteUserPreference( Person person, string key )
        {
            var anonymousVisitorGuid = new Guid( SystemGuid.Person.ANONYMOUS_VISITOR );
            PersonPreferenceCollection preferences;

            if ( person == null || !person.PrimaryAliasId.HasValue || person.Guid == anonymousVisitorGuid )
            {
                return;
            }

            preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

            preferences.SetValue( key, string.Empty );
        }

        /// <summary>
        /// Returns all of the user preference settings for a <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the user preference settings for.</param>
        /// <returns>A dictionary containing all of the <see cref="Rock.Model.Person">Person's</see> user preference settings.</returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use PersonPreferenceCache to access preferences instead." )]
        public static Dictionary<string, string> GetUserPreferences( Person person )
        {
            var anonymousVisitorGuid = new Guid( SystemGuid.Person.ANONYMOUS_VISITOR );
            var prefs = new Dictionary<string, string>();
            PersonPreferenceCollection preferences;

            if ( person == null || !person.PrimaryAliasId.HasValue || person.Guid == anonymousVisitorGuid )
            {
                return prefs;
            }

            preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

            foreach ( var key in preferences.GetKeys() )
            {
                prefs.AddOrIgnore( key, preferences.GetValue( key ) );
            }

            return prefs;
        }

        #endregion

        #region Update Calculated Person Details

        /// <summary>
        /// Ensures the person age classification is correct for the specified person
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// If the person age classification was changed
        /// </returns>
        public static bool UpdatePersonAgeClassification( int personId, RockContext rockContext )
        {
            var recordsUpdated = UpdatePersonAgeClassifications( personId, rockContext );
            return recordsUpdated != 0;
        }

        /// <summary>
        /// Ensures the person age classifications are correct for all records in the database
        /// In most cases this will take care of people that have become adults due to an 18th birthday, but will also
        /// fix person records that somehow got marked as Adult but should be marked as Child (maybe incorrect Birthdate was fixed)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The number of records updated
        /// </returns>
        public static int UpdatePersonAgeClassificationAll( RockContext rockContext )
        {
            return UpdatePersonAgeClassifications( null, rockContext );
        }

        /// <summary>
        /// Updates the person age classifications
        /// In most cases this will take care of people that have become adults due to an 18th birthday, but will also
        /// fix person records that somehow got marked as Adult but should be marked as Child (maybe incorrect Birthdate was fixed)
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The number of records updated
        /// </returns>
        private static int UpdatePersonAgeClassifications( int? personId, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            IQueryable<Person> personQuery;

            if ( personId.HasValue )
            {
                personQuery = personService.AsNoFilter().Where( a => a.Id == personId && !a.IsDeceased );
            }
            else
            {
                personQuery = personService.Queryable( includeDeceased: false, includeBusinesses: false );
            }

            // get the min birthdate of people 18 and younger;
            var birthDateEighteen = RockDateTime.Now.AddYears( -18 );
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int familyGroupTypeId = familyGroupType.Id;
            int groupRoleAdultId = familyGroupType.Roles.FirstOrDefault( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            int groupRoleChildId = familyGroupType.Roles.FirstOrDefault( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;

            var familyPersonRoleQuery = new GroupMemberService( rockContext ).Queryable()
                .Where( a => a.Group.GroupTypeId == familyGroupTypeId );

            // Unknown if Birthdate is not present and not in any family
            var unknownBasedOnBirthdateOrFamilyRole = personQuery
                .Where( p => !p.BirthDate.HasValue && !familyPersonRoleQuery.Where( f => f.PersonId == p.Id ).Any() );

            // Adult if Age >= 18 OR has a role of Adult in one or more (ANY) families
            var adultBasedOnBirthdateOrFamilyRole = personQuery
                .Where( p => !unknownBasedOnBirthdateOrFamilyRole.Any( a => a.Id == p.Id )
                    &&
                    ( ( p.Age.HasValue && p.Age.Value >= 18 )
                        || familyPersonRoleQuery.Where( f => f.PersonId == p.Id ).Any( f => f.GroupRoleId == groupRoleAdultId )
                    ) );

            // Child if (not adultBasedOnBirthdateOrFamilyRole) AND (Age < 18 OR child in ALL families)
            var alreadyClassified = unknownBasedOnBirthdateOrFamilyRole.Union( adultBasedOnBirthdateOrFamilyRole );
            var childBasedOnBirthdateOrFamilyRole = personQuery
                .Where( p => !alreadyClassified.Any( a => a.Id == p.Id )
                    &&
                    ( ( p.Age.HasValue && p.Age.Value < 18 )
                        ||
                        familyPersonRoleQuery.Where( f => f.PersonId == p.Id ).All( f => f.GroupRoleId == groupRoleChildId )
                    ) );

            // update records that aren't marked as Adult but now should be
            int updatedAdultCount = rockContext.BulkUpdate(
                adultBasedOnBirthdateOrFamilyRole.Where( a => a.AgeClassification != AgeClassification.Adult && !a.IsLockedAsChild ),
                p => new Person { AgeClassification = AgeClassification.Adult } );

            // update records that aren marked as Adult but now should be child as IsLockedAsChild is marked as true
            int updatedLockedChildCount = rockContext.BulkUpdate(
                adultBasedOnBirthdateOrFamilyRole.Where( a => a.AgeClassification == AgeClassification.Adult && a.IsLockedAsChild ),
                p => new Person { AgeClassification = AgeClassification.Child } );

            // update records that aren't marked as Child but now should be
            int updatedChildCount = rockContext.BulkUpdate(
                childBasedOnBirthdateOrFamilyRole.Where( a => a.AgeClassification != AgeClassification.Child ),
                p => new Person { AgeClassification = AgeClassification.Child } );

            // NOTE: A person can't become 'AgeClassification.Unknown' if they have already bet set to Adult or Child so we don't have to recalculate for new AgeClassification.Unknown
            return updatedAdultCount + updatedAdultCount + updatedLockedChildCount;
        }

        /// <summary>
        /// Ensures the PrimaryFamily is correct for the specified person
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static bool UpdatePrimaryFamily( int personId, RockContext rockContext )
        {
            int recordsUpdated = UpdatePersonsPrimaryFamily( rockContext, personId: personId );
            return recordsUpdated != 0;
        }

        /// <summary>
        /// Ensures the PrimaryFamily is correct for all person records in the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static int UpdatePrimaryFamilyAll( RockContext rockContext )
        {
            return UpdatePersonsPrimaryFamily( rockContext );
        }

        /// <summary>
        /// Ensures the PrimaryFamily and PrimaryCampus are correct for the people in the specified group.
        /// </summary>
        /// <param name="groupId">The group identifier, usually a reference to a Family.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static bool UpdatePrimaryFamilyByGroup( int groupId, RockContext rockContext )
        {
            int recordsUpdated = UpdatePersonsPrimaryFamily( rockContext, groupId: groupId );

            return recordsUpdated != 0;
        }

        /// <summary>
        /// Updates the primary family and campus for the specified person or group members, or for all persons in the database if no parameters are specified.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Specified parameters are ambiguous.</exception>
        private static int UpdatePersonsPrimaryFamily( RockContext rockContext, int? personId = null, int? groupId = null )
        {
            // Verify only one of the optional parameters is specified.
            if ( personId.HasValue
                 && groupId.HasValue )
            {
                throw new ArgumentException( "Specified parameters are ambiguous." );
            }

            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;

            // Use raw 'UPDATE SET FROM' update to quickly ensure that the Primary Family and Campus on each Person record matches the Calculated Primary Family and Campus.
            var sqlUpdateBuilder = new StringBuilder();
            sqlUpdateBuilder.Append( $@"
UPDATE x
SET x.PrimaryFamilyId = x.CalculatedPrimaryFamilyId
    ,x.PrimaryCampusId = x.CalculatedPrimaryCampusId
FROM (
    SELECT p.Id
        ,p.NickName
        ,p.LastName
        ,p.PrimaryFamilyId
        ,p.PrimaryCampusId
        ,pf.CalculatedPrimaryFamilyId
        ,pf.CalculatedPrimaryCampusId
    FROM Person p
    OUTER APPLY (
        SELECT TOP 1
            g.Id [CalculatedPrimaryFamilyId]
            ,g.CampusId [CalculatedPrimaryCampusId]
        FROM GroupMember gm
        JOIN [Group] g ON g.Id = gm.GroupId
        WHERE g.GroupTypeId = {groupTypeIdFamily}
            AND gm.IsArchived = 0
            AND gm.PersonId = p.Id
        ORDER BY gm.GroupOrder
            ,gm.GroupId
        ) pf
    WHERE (
            (ISNULL(p.PrimaryFamilyId, 0) != ISNULL(pf.CalculatedPrimaryFamilyId, 0))
            OR (ISNULL(p.PrimaryCampusId, 0) != ISNULL(pf.CalculatedPrimaryCampusId, 0))
            )" );

            if ( personId.HasValue )
            {
                sqlUpdateBuilder.Append( $" AND ( p.Id = @personId) " );
            }
            else if ( groupId.HasValue )
            {
                sqlUpdateBuilder.Append( $" AND ( p.Id IN ( SELECT p1.Id FROM Person p1 INNER JOIN GroupMember gm2 ON p1.Id = gm2.PersonId WHERE gm2.GroupId = @groupId ) ) " );
            }

            sqlUpdateBuilder.Append( @"    ) x " );

            if ( personId.HasValue )
            {
                var recordsChanged = rockContext.Database.ExecuteSqlCommand( sqlUpdateBuilder.ToString(), new System.Data.SqlClient.SqlParameter( "@personId", personId.Value ) );

                if ( recordsChanged > 0 )
                {
                    // Since PrimaryFamily is populated in straight SQL, we'll need to tell EF what the Person's new PrimaryFamilyId is
                    var affectedPerson = rockContext.People.FirstOrDefault( a => a.Id == personId );
                    if ( affectedPerson != null )
                    {
                        var primaryFamilyId = rockContext.Database.SqlQuery<int?>( $"SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @personId", new System.Data.SqlClient.SqlParameter( "@personId", personId.Value ) ).FirstOrDefault();
                        if ( primaryFamilyId != null && primaryFamilyId != affectedPerson.PrimaryFamilyId )
                        {
                            // since the PrimaryFamily changed, null out PrimaryFamily and set the new PrimaryFamilyId.
                            // This will make sure any queries to this Person for the remainder of the current rockContext will get the updated PrimaryFamilyId
                            affectedPerson.PrimaryFamily = null;
                            affectedPerson.PrimaryFamilyId = primaryFamilyId;
                        }
                    }
                }

                return recordsChanged;
            }
            else if ( groupId.HasValue )
            {
                return rockContext.Database.ExecuteSqlCommand( sqlUpdateBuilder.ToString(), new System.Data.SqlClient.SqlParameter( "@groupId", groupId.Value ) );
            }
            else
            {
                return rockContext.Database.ExecuteSqlCommand( sqlUpdateBuilder.ToString() );
            }
        }

        /// <summary>
        /// Ensures the GivingLeaderId is correct for the specified person
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static bool UpdateGivingLeaderId( int personId, RockContext rockContext )
        {
            int recordsUpdated = UpdatePersonGivingLeaderId( personId, rockContext );
            return recordsUpdated != 0;
        }

        /// <summary>
        /// Ensures the GivingLeaderId is correct for all person records in the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static int UpdateGivingLeaderIdAll( RockContext rockContext )
        {
            return UpdatePersonGivingLeaderId( null, rockContext );
        }

        /// <summary>
        /// Ensures the GivingId is correct for the given Person.Id. Updates via SQL.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void UpdateGivingId( int personId, RockContext rockContext )
        {
            var person = new PersonService( rockContext ).Get( personId );
            var correctGivingId = person.GivingGroupId.HasValue ? $"G{ person.GivingGroupId.Value }" : $"P{ person.Id }";

            // Make sure the GivingId is correct.
            if ( person.GivingId != correctGivingId )
            {
                rockContext.Database.ExecuteSqlCommand( $"UPDATE [Person] SET [GivingId] = '{ correctGivingId }' WHERE [Id] = { personId }" );
            }
        }

        /// <summary>
        /// Ensures the GivingId is correct for all person records in the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static int UpdateGivingIdAll( RockContext rockContext )
        {
            return rockContext.Database.ExecuteSqlCommand( @"
UPDATE Person
SET GivingId = (
		CASE 
			WHEN [GivingGroupId] IS NOT NULL
				THEN 'G' + CONVERT([varchar], [GivingGroupId])
			ELSE 'P' + CONVERT([varchar], [Id])
			END
		)
WHERE GivingId IS NULL OR GivingId != (
		CASE 
			WHEN [GivingGroupId] IS NOT NULL
				THEN 'G' + CONVERT([varchar], [GivingGroupId])
			ELSE 'P' + CONVERT([varchar], [Id])
			END
		)" );
        }

        /// <summary>
        /// Updates the person giving leader identifier for the specified person, or for all persons in the database if personId is null.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static int UpdatePersonGivingLeaderId( int? personId, RockContext rockContext )
        {
            var sqlUpdateBuilder = new StringBuilder();

            /* 
                JME 9/25/2023
                Updated this logic to not filter out deceased, but to sort by IsDeceased (living before deceased).
                So that if both adults are deceased the Giving Leader goes back to being the adult male vs it
                becoming different for both individuals. (Issue #2848).
            */
            sqlUpdateBuilder.Append( @"
UPDATE x
SET x.GivingLeaderId = x.CalculatedGivingLeaderId
FROM (
	SELECT p.Id
		,p.NickName
		,p.LastName
		,p.GivingLeaderId
		,isnull(pf.CalculatedGivingLeaderId, p.Id) CalculatedGivingLeaderId
	FROM Person p
	OUTER APPLY (
		SELECT TOP 1 p2.[Id] CalculatedGivingLeaderId
		FROM [GroupMember] gm
		INNER JOIN [GroupTypeRole] r ON r.[Id] = gm.[GroupRoleId]
		INNER JOIN [Person] p2 ON p2.[Id] = gm.[PersonId]
		WHERE gm.[GroupId] = p.GivingGroupId
			AND p2.[GivingGroupId] = p.GivingGroupId
		ORDER BY
            p2.[IsDeceased]
            , r.[Order]
			,p2.[Gender]
			,p2.[BirthYear]
			,p2.[BirthMonth]
			,p2.[BirthDay]
		) pf
	WHERE (
			p.GivingLeaderId = 0
			OR (p.GivingLeaderId != ISNULL(pf.CalculatedGivingLeaderId, p.Id))
			)" );

            if ( personId.HasValue )
            {
                sqlUpdateBuilder.Append( $" AND ( p.GivingId in (select GivingId from Person where Id = @personId ) ) " );
            }

            sqlUpdateBuilder.Append( @"    ) x " );

            if ( personId.HasValue )
            {
                return rockContext.Database.ExecuteSqlCommand( sqlUpdateBuilder.ToString(), new System.Data.SqlClient.SqlParameter( "@personId", personId.Value ) );
            }
            else
            {
                return rockContext.Database.ExecuteSqlCommand( sqlUpdateBuilder.ToString() );
            }
        }

        /// <summary>
        /// Updates the group salutations for person and <see cref="Data.DbContext.SaveChanges()">saves changes</see> to the database.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// System.Int32.
        /// </returns>
        public static int UpdateGroupSalutations( int personId, RockContext rockContext )
        {
            // Use specified rockContext to person's PrimaryFamilyId because rockContext
            // Might be in a transaction that hasn't been committed yet
            var primaryFamilyId = new PersonService( rockContext ).GetSelect( personId, s => s.PrimaryFamilyId );

            if ( !primaryFamilyId.HasValue )
            {
                // If this is a new person, and the GroupMember record for the Family hasn't been saved to the database this could happen.
                // If so, the GroupMember.PostSaveChanges will call this and that should take care of it
                return 0;
            }

            bool changesMade = GroupService.UpdateGroupSalutations( primaryFamilyId.Value, rockContext );
            if ( changesMade )
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Updates the <see cref="Person.AccountProtectionProfile" /> for all people.
        /// Returns the number of people whose account protection profile was updated.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>System.Int32.</returns>
        public static int UpdateAccountProtectionProfileAll( RockContext rockContext )
        {
            return UpdateAccountProtectionProfile( null, rockContext );
        }

        /// <summary>
        /// Updates the <see cref="Person.AccountProtectionProfile" /> for the specified person.
        /// Returns 1 (number of person records updated) if that person's account protection profile was updated.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>System.Int32.</returns>
        public static int UpdateAccountProtectionProfileForPerson( int personId, RockContext rockContext )
        {
            return UpdateAccountProtectionProfile( personId, rockContext );
        }

        /// <summary>
        /// Updates the <see cref="Person.AccountProtectionProfile" /> for all people, or just the specified person.
        /// Returns the number of people whose account protection profile was updated.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>System.Int32.</returns>
        private static int UpdateAccountProtectionProfile( int? personId, RockContext rockContext )
        {
            int rowsUpdated = 0;
            var personService = new PersonService( rockContext );

            /* Determine people that have logins */
            var personIdsWithLoginsQuery = personService
                .AsNoFilter()
                .Where( p => p.Users.Any() )
                .Select( a => a.Id );

            bool includeDeceased = true;
            bool includeArchived = false;

            /* Determine people in security role groups with ElevatedSecurityLevel.High */
            var groupMemberService = new GroupMemberService( rockContext );
            var personIdsInGroupsWithHighSecurityLevelQuery = groupMemberService
                .Queryable( includeDeceased, includeArchived )
                .IsInSecurityRoleGroupOrSecurityRoleGroupType()
                .Where( gm => gm.Group.IsActive && gm.Group.ElevatedSecurityLevel == ElevatedSecurityLevel.Extreme )
                .Select( gm => gm.PersonId );

            /* Determine people in security role groups with ElevatedSecurityLevel.Low */
            var personIdsInGroupsWithLowSecurityLevelQuery = groupMemberService
                .Queryable( includeDeceased, includeArchived )
                .IsInSecurityRoleGroupOrSecurityRoleGroupType()
                .Where( gm => gm.Group.IsActive && gm.Group.ElevatedSecurityLevel == ElevatedSecurityLevel.High )
                .Select( gm => gm.PersonId );

            /* Determine People with Financial Data */

            // Determine PersonAliasIds that have Financial Saved Accounts
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var personAliasWithFinancialPersonSavedAccountQuery = financialPersonSavedAccountService
                .Queryable()
                .Where( f => f.PersonAliasId.HasValue )
                .Select( f => f.PersonAliasId.Value );

            // Determine PersonAliasIds that have active Financial Scheduled Transactions
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var personAliasIdsWithFinancialScheduledTransactionQuery = financialScheduledTransactionService
                .Queryable()
                .Where( p => p.IsActive )
                .Select( f => f.AuthorizedPersonAliasId );

            /*  2021-10-05 MDP

            If we decide that FinancialTransactions and/or FinancialPersonBankAccount should be factored in,
            this code can be used to include those.

            // Determine PersonAliasIds that have Financial Transactionss
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var peopleWithFinancialTransactionQuery = financialTransactionService
                .Queryable()
                .Where( f => f.AuthorizedPersonAliasId.HasValue )
                .Select( f => f.AuthorizedPersonAliasId.Value );

            // Determine PersonAliasIds that have Financial Bank Accounts
            var financialPersonBankAccountService = new FinancialPersonBankAccountService( rockContext );
            var personAliasWithFinancialPersonBankAccountQuery = financialPersonBankAccountService
                .Queryable()
                .Select( f => f.PersonAliasId );

            // combine to get PersonAliasIds that have any type of financial data
            var personAliasIdsWithFinancialDataQuery =
                personAliasWithFinancialPersonBankAccountQuery
                .Union( personAliasWithFinancialPersonSavedAccountQuery )
                .Union( personAliasIdsWithFinancialScheduledTransactionQuery )
                .Union( peopleWithFinancialTransactionQuery );

             */

            // combine to get PersonAliasIds that have any type of financial data
            var personAliasIdsWithFinancialDataQuery = personAliasWithFinancialPersonSavedAccountQuery.Union( personAliasIdsWithFinancialScheduledTransactionQuery );

            /*
                Rules

                Low
                  - No Risk Items

                Medium
                  - Individual Has Login

                High
                  - one or more of the following -
                    + Active Scheduled Financial Transaction (inactive are not viewable)
                    + Saved Payment Account
                    + in a Security Role Marked w/ Low Elevated Security 

                Extreme
                  - in a Security Role marked w/ High Elevated Security Level 
             */

            // set up query as all person records regardless of Deceased, record type, etc
            var personQuery = personService.AsNoFilter();
            if ( personId.HasValue )
            {
                // if this is for a specific person, just calculate for that one person
                personQuery = personQuery.Where( a => a.Id == personId.Value );
            }

            // update the people that meet the AccountProtectionProfile.Low criteria:
            //  -- No Risk Items
            var personToSetAsAccountProtectionProfileLowQuery = personQuery.Where( p =>
                    !personIdsWithLoginsQuery.Contains( p.Id )
                    && !personIdsInGroupsWithLowSecurityLevelQuery.Contains( p.Id )
                    && !personAliasIdsWithFinancialDataQuery.Any( fdPersonAliasId => p.Aliases.Any( pa => pa.Id == fdPersonAliasId ) )
                    && !personIdsInGroupsWithHighSecurityLevelQuery.Contains( p.Id )
                    && p.AccountProtectionProfile != AccountProtectionProfile.Low );

            rowsUpdated += rockContext.BulkUpdate( personToSetAsAccountProtectionProfileLowQuery, p => new Person { AccountProtectionProfile = AccountProtectionProfile.Low } );

            // update the people that meet the AccountProtectionProfile.Medium criteria:
            //  -- Has login
            //  -- No other Risk items
            var personToSetAsAccountProtectionProfileMediumQuery = personQuery.Where( p =>
                    personIdsWithLoginsQuery.Contains( p.Id )
                    && !personIdsInGroupsWithLowSecurityLevelQuery.Contains( p.Id )
                    && !personAliasIdsWithFinancialDataQuery.Any( fdPersonAliasId => p.Aliases.Any( pa => pa.Id == fdPersonAliasId ) )
                    && !personIdsInGroupsWithHighSecurityLevelQuery.Contains( p.Id )
                    && p.AccountProtectionProfile != AccountProtectionProfile.Medium );

            rowsUpdated += rockContext.BulkUpdate( personToSetAsAccountProtectionProfileMediumQuery, p => new Person { AccountProtectionProfile = AccountProtectionProfile.Medium } );

            // update the people that meet the AccountProtectionProfile.Medium criteria:
            //   -- In a Low Security Role Group or Has Financial Data
            //   -- Not in a High security role group
            var personToSetAsAccountProtectionProfileHighQuery = personQuery.Where( p =>
                    ( personIdsInGroupsWithLowSecurityLevelQuery.Contains( p.Id ) || personAliasIdsWithFinancialDataQuery.Any( fdPersonAliasId => p.Aliases.Any( pa => pa.Id == fdPersonAliasId ) ) )
                    && !personIdsInGroupsWithHighSecurityLevelQuery.Contains( p.Id )
                    && p.AccountProtectionProfile != AccountProtectionProfile.High );

            rowsUpdated += rockContext.BulkUpdate( personToSetAsAccountProtectionProfileHighQuery, p => new Person { AccountProtectionProfile = AccountProtectionProfile.High } );

            // update the people that meet the AccountProtectionProfile.Extreme criteria:
            //   -- In a High security role group
            var personToSetAsAccountProtectionProfileExtremeQuery = personQuery.Where( p =>
                personIdsInGroupsWithHighSecurityLevelQuery.Contains( p.Id )
                && p.AccountProtectionProfile != AccountProtectionProfile.Extreme );

            rowsUpdated += rockContext.BulkUpdate( personToSetAsAccountProtectionProfileExtremeQuery, p => new Person { AccountProtectionProfile = AccountProtectionProfile.Extreme } );

            return rowsUpdated;
        }

        /// <summary>
        /// Sets the PrimaryAliasId for the specified person
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="primaryAliasId">The PrimaryAlias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static int UpdatePrimaryAlias( int personId, int primaryAliasId, RockContext rockContext )
        {
            return rockContext.Database.ExecuteSqlCommand( @"
UPDATE Person
SET PrimaryAliasId = @primaryAliasId
WHERE Id = @personId",
        new System.Data.SqlClient.SqlParameter( "@personId", personId ), new System.Data.SqlClient.SqlParameter( "@primaryAliasId", primaryAliasId ) );
        }

        /// <summary>
        /// Updates the person's group member role (whether Adult/Child) for the specified person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static int UpdateFamilyMemberRoleByAge( int personId, int age, RockContext rockContext )
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();

            int? roleId;
            int result = 0;

            if ( age >= 18 )
            {
                roleId = familyGroupType.Roles?.Find( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )?.Id;
            }
            else
            {
                roleId = familyGroupType.Roles?.Find( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() )?.Id;
            }

            if ( roleId.HasValue )
            {
                result = rockContext.Database.ExecuteSqlCommand( $@"
UPDATE GroupMember
SET GroupRoleId = {roleId}
WHERE PersonId = ${personId}
AND GroupTypeId = ${familyGroupType.Id}
" );
            }

            return result;
        }

        #endregion

        #region Anonymous Visitor

        /// <summary>
        /// Gets the AnonymousVisitorPersonId, and creates it if it doesn't exist.
        /// <seealso cref="GetOrCreateAnonymousVisitorPerson"/>
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetOrCreateAnonymousVisitorPersonId()
        {
            var anonymousVisitorPersonId = GetId( SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid() );
            if ( !anonymousVisitorPersonId.HasValue )
            {
                anonymousVisitorPersonId = GetOrCreateAnonymousVisitorPerson().Id;
            }

            return anonymousVisitorPersonId.Value;
        }

        /// <summary>
        /// Gets or creates the anonymous visitor person.
        /// </summary>
        /// <returns>A <see cref="Person"/> that matches the SystemGuid.Person.ANONYMOUS_VISITOR Guid value.</returns>
        public Person GetOrCreateAnonymousVisitorPerson()
        {
            var anonymousVisitor = Get( SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid() );
            if ( anonymousVisitor == null )
            {
                CreateAnonymousVisitorPerson();
                anonymousVisitor = Get( SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid() );
            }

            return anonymousVisitor;
        }

        /// <summary>
        /// Creates the anonymous visitor person.  Used by GetOrCreateAnonymousVisitorPerson().
        /// </summary>
        private void CreateAnonymousVisitorPerson()
        {
            using ( var anonymousVisitorPersonRockContext = new RockContext() )
            {
                var connectionStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
                var recordStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                var recordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
                var anonymousVisitor = new Person()
                {
                    IsSystem = true,
                    RecordTypeValueId = recordTypeValueId,
                    RecordStatusValueId = recordStatusValueId,
                    ConnectionStatusValueId = connectionStatusValueId,
                    IsDeceased = false,
                    FirstName = "Anonymous",
                    NickName = "Anonymous",
                    LastName = "Visitor",
                    Gender = Gender.Unknown,
                    IsEmailActive = true,
                    Guid = SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid(),
                    EmailPreference = EmailPreference.EmailAllowed,
                    CommunicationPreference = CommunicationType.Email
                };

                new PersonService( anonymousVisitorPersonRockContext ).Add( anonymousVisitor );
                if ( anonymousVisitor != null )
                {
                    PersonService.SaveNewPerson( anonymousVisitor, anonymousVisitorPersonRockContext, null, false );
                }

                anonymousVisitorPersonRockContext.SaveChanges();
            }
        }

        #endregion
        #region Anonymous Giver

        /// <summary>
        /// Gets or creates the anonymous giver person.
        /// </summary>
        /// <returns>A <see cref="Person"/> that matches the SystemGuid.Person.GIVER_ANONYMOUS Guid value.</returns>
        public Person GetOrCreateAnonymousGiverPerson()
        {
            var anonymousGiver = Get( SystemGuid.Person.GIVER_ANONYMOUS.AsGuid() );
            if ( anonymousGiver == null )
            {
                CreateAnonymousGiverPerson();
                anonymousGiver = Get( SystemGuid.Person.GIVER_ANONYMOUS.AsGuid() );
            }

            return anonymousGiver;
        }

        /// <summary>
        /// Creates the anonymous giver person.  Used by GetOrCreateAnonymousGiverPerson().
        /// </summary>
        private void CreateAnonymousGiverPerson()
        {
            using ( var anonymousGiverPersonRockContext = new RockContext() )
            {
                var connectionStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
                var recordStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                var recordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
                var anonymousGiver = new Person()
                {
                    IsSystem = true,
                    RecordTypeValueId = recordTypeValueId,
                    RecordStatusValueId = recordStatusValueId,
                    ConnectionStatusValueId = connectionStatusValueId,
                    IsDeceased = false,
                    FirstName = "Giver",
                    NickName = "Giver",
                    LastName = "Anonymous",
                    Gender = Gender.Unknown,
                    IsEmailActive = true,
                    Guid = SystemGuid.Person.GIVER_ANONYMOUS.AsGuid(),
                    EmailPreference = EmailPreference.EmailAllowed,
                    CommunicationPreference = CommunicationType.Email
                };

                new PersonService( anonymousGiverPersonRockContext ).Add( anonymousGiver );
                if ( anonymousGiver != null )
                {
                    PersonService.SaveNewPerson( anonymousGiver, anonymousGiverPersonRockContext, null, false );
                }

                anonymousGiverPersonRockContext.SaveChanges();
            }
        }

        #endregion Anonymous Giver

        /// <summary>
        /// Gets the merge request query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<EntitySet> GetMergeRequestQuery( RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var entityTypeId = EntityTypeCache.GetId<Person>();
            if ( entityTypeId == null )
            {
                return null;
            }

            var entitySetPurposeGuid = SystemGuid.DefinedValue.ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST.AsGuid();
            var definedValueId = DefinedValueCache.GetId( entitySetPurposeGuid );
            if ( definedValueId == null )
            {
                return null;
            }

            var entitySetService = new EntitySetService( rockContext );
            var expirationDate = RockDateTime.Now;

            var mergeRequestQry = entitySetService
                .Queryable()
                .Where( es => es.EntityTypeId == entityTypeId )
                .Where( es => es.EntitySetPurposeValueId == definedValueId )
                .Where( es => es.ExpireDateTime == null || es.ExpireDateTime > expirationDate );

            return mergeRequestQry;
        }

        #region Configuration Settings

        /// <summary>
        /// Gets the current graduation date base on the GradeTransitionDate GlobalAttribute and current datetime.
        /// For example, if the Grade Transition Date is June 1st and the current date is June 1st or earlier, it will return June 1st of the current year;
        /// otherwise, it will return June 1st of next year
        /// </summary>
        /// <value>
        /// The current graduation date.
        /// </value>
        public static DateTime GetCurrentGraduationDate()
        {
            /*
             * Implemented as a method rather than a property to indicate to the caller that this is a calculated value
             * and should be cached if it is to be used multiple times.
             */
            var graduationDateWithCurrentYear = GlobalAttributesCache.Get().GetValue( "GradeTransitionDate" ).MonthDayStringAsDateTime() ?? new DateTime( RockDateTime.Today.Year, 6, 1 );
            if ( graduationDateWithCurrentYear < RockDateTime.Today )
            {
                // if the graduation date already occurred this year, return next year' graduation date
                return graduationDateWithCurrentYear.AddYears( 1 );
            }

            return graduationDateWithCurrentYear;
        }

        /// <summary>
        /// Gets the current graduation year based on <see cref="GetCurrentGraduationDate"/>
        /// </summary>
        /// <value>
        /// The current graduation year.
        /// </value>
        public static int GetCurrentGraduationYear()
        {
            return GetCurrentGraduationDate().Year;
        }

        #endregion

        /// <summary>
        /// Gets all the foreign keys in the person table in the database
        /// </summary>
        /// <returns></returns>
        public string[] GetForeignKeys()
        {
            return Queryable()
                .Select( person => person.ForeignKey )
                .Where( foreignKey => foreignKey.Trim().Length > 0 )
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Get the Person Entity with the given the Key of the Foreign System and the Person Id in the Foreign System.
        /// </summary>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        /// <param name="foreignSystemPersonId">The foreign system person identifier.</param>
        /// <returns></returns>
        public Person FromForeignSystem( string foreignSystemKey, int foreignSystemPersonId )
        {
            return Queryable()
                .Where( person => person.ForeignKey == foreignSystemKey && person.ForeignId == foreignSystemPersonId )
                .FirstOrDefault();
        }
    }
}

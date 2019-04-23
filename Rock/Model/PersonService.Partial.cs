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
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using Rock;
using Rock.BulkExport;
using Rock.Data;
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
        /// Gets the specified unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public override Person Get( Guid guid )
        {
            // if a specific person Guid is specified, get the person record even if IsDeceased or IsBusiness
            return this.Queryable( true, true ).FirstOrDefault( a => a.Guid == guid );
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Person Get( int id )
        {
            // if a specific person Id is specified, get the person record even if IsDeceased or IsBusiness
            return this.Queryable( true, true ).FirstOrDefault( a => a.Id == id );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities.
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities.</returns>
        public override IQueryable<Person> Queryable()
        {
            return Queryable( false );
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
            return Queryable( null, includeDeceased, includeBusinesses );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.Person"/> entities with eager loading of the properties that are included in the includes parameter.
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a comma delimited list of properties that should support eager loading.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities with properties that support eager loading.</returns>
        public override IQueryable<Person> Queryable( string includes )
        {
            return Queryable( includes, false, true );
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
            var qry = base.Queryable( includes );
            if ( !includeBusinesses )
            {
                var definedValueBusinessType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
                if ( definedValueBusinessType != null )
                {
                    int recordTypeBusiness = definedValueBusinessType.Id;
                    qry = qry.Where( p => p.RecordTypeValueId != recordTypeBusiness );
                }
            }

            if ( !includeDeceased )
            {
                qry = qry.Where( p => p.IsDeceased == false );
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
        /// <param name="firstName">A <see cref="System.String"/> representing the first name to search by.</param>
        /// <param name="lastName">A <see cref="System.String"/> representing the last name to search by.</param>
        /// <param name="email">A <see cref="System.String"/> representing the email address to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in the search results, if
        /// <c>true</c> then they will be included, otherwise <c>false</c>. Default value is false.</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use FindPersons instead.", false )]
        public IEnumerable<Person> GetByMatch( string firstName, string lastName, string email, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return this.FindPersons( firstName, lastName, email, includeDeceased, includeBusinesses );
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
                    Gender = p.Gender,
                    BirthDate = p.BirthDate,
                    SuffixValueId = p.SuffixValueId
                } )
                .ToList()
                .ToDictionary(
                    p => p.Id,
                    p =>
                    {
                        var result = new PersonMatchResult( searchParameters, p )
                        {
                            LastNameMatched = true
                        };
                        return result;
                    }
                );

            if ( searchParameters.Email.IsNotNullOrWhiteSpace() )
            {
                var searchTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() ).Id;

                // OR query for email or previous email
                var previousEmailQry = new PersonSearchKeyService( this.Context as RockContext ).Queryable();
                Queryable( includeDeceased, includeBusinesses )
                    .AsNoTracking()
                    .Where(
                        p => ( p.Email != String.Empty && p.Email != null && p.Email == searchParameters.Email ) ||
                        previousEmailQry.Any( a => a.PersonAlias.PersonId == p.Id && a.SearchValue == searchParameters.Email && a.SearchTypeValueId == searchTypeValueId )
                    )
                    .Select( p => new PersonSummary()
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        NickName = p.NickName,
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        SuffixValueId = p.SuffixValueId
                    } )
                    .ToList()
                    .ForEach( p =>
                    {
                        if ( foundPeople.ContainsKey( p.Id ) )
                        {
                            foundPeople[p.Id].EmailMatched = true;
                        }
                        else
                        {
                            foundPeople[p.Id] = new PersonMatchResult( searchParameters, p )
                            {
                                EmailMatched = true
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
                    Gender = n.PersonAlias.Person.Gender,
                    BirthDate = n.PersonAlias.Person.BirthDate,
                    SuffixValueId = n.PersonAlias.Person.SuffixValueId
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
                        foundPeople[p.Id] = new PersonMatchResult( searchParameters, p )
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
                        Gender = n.Person.Gender,
                        BirthDate = n.Person.BirthDate,
                        SuffixValueId = n.Person.SuffixValueId
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
                            foundPeople[p.Id] = new PersonMatchResult( searchParameters, p )
                            {
                                MobileMatched = true
                            };
                        }
                    } );
            }

            // Find people who have a good confidence score
            var goodMatches = foundPeople.Values
                .Where( match => match.ConfidenceScore >= MATCH_SCORE_CUTOFF )
                .OrderByDescending( match => match.ConfidenceScore );

            return GetByIds( goodMatches.Select( a => a.PersonId ).ToList() );
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
            /// <param name="query">The person match query.</param>
            /// <param name="person">The person summary.</param>
            public PersonMatchResult( PersonMatchQuery query, PersonSummary person )
            {
                PersonId = person.Id;
                FirstNameMatched = ( person.FirstName != null && person.FirstName != String.Empty && person.FirstName.Equals( query.FirstName, StringComparison.CurrentCultureIgnoreCase ) ) || ( person.NickName != null && person.NickName != String.Empty && person.NickName.Equals( query.FirstName, StringComparison.CurrentCultureIgnoreCase ) );
                LastNameMatched = person.LastName != null && person.LastName != String.Empty && person.LastName.Equals( query.LastName, StringComparison.CurrentCultureIgnoreCase );
                SuffixMatched = query.SuffixValueId.HasValue && person.SuffixValueId != null && query.SuffixValueId == person.SuffixValueId;
                GenderMatched = query.Gender.HasValue & query.Gender == person.Gender;

                if ( query.BirthDate.HasValue && person.BirthDate.HasValue )
                {
                    BirthDate = query.BirthDate.Value.Month == person.BirthDate.Value.Month && query.BirthDate.Value.Day == person.BirthDate.Value.Day;
                    BirthDateYearMatched = BirthDate && person.BirthDate.Value.Year == query.BirthDate.Value.Year;
                }
            }

            public int PersonId { get; set; }

            public bool FirstNameMatched { get; set; }

            public bool LastNameMatched { get; set; }

            public bool EmailMatched { get; set; }

            public bool MobileMatched { get; set; }

            public bool PreviousNameMatched { get; set; }

            public bool SuffixMatched { get; set; }

            public bool GenderMatched { get; set; }

            public bool BirthDate { get; set; }

            public bool BirthDateYearMatched { get; set; }


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

                    if ( MobileMatched || EmailMatched )
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

            public Gender Gender { get; set; }

            public DateTime? BirthDate { get; set; }

            public int? SuffixValueId { get; set; }
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
            if ( updatePrimaryEmail && match != null )
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
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities that have a matching email address, firstname and lastname.
        /// </summary>
        /// <param name="businessName">A <see cref="System.String"/> representing the business name to search by.</param>
        /// <param name="email">A <see cref="System.String"/> representing the email address to search by.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use FindBusinesses instead.", false )]
        public IEnumerable<Person> GetBusinessByMatch( string businessName, string email )
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

            var groupMember = new GroupMember();
            groupMember.PersonId = businessId;
            groupMember.GroupRoleId = businessRoleId;
            contactKnownRelationshipGroup.Members.Add( groupMember );

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

            var businessGroupMember = new GroupMember();
            businessGroupMember.PersonId = contactPersonId;
            businessGroupMember.GroupRoleId = businessContactRoleId;
            businessKnownRelationshipGroup.Members.Add( businessGroupMember );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities by martial status <see cref="Rock.Model.DefinedValue"/>
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
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by the the Person's Connection Status <see cref="Rock.Model.DefinedValue"/>.
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
            string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();

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
                var rockContext = this.Context as RockContext;
                var groupMemberService = new GroupMemberService( rockContext );
                int groupTypeIdFamilyOrBusiness = GroupTypeCache.GetFamilyGroupType().Id;

                var personIdAddressQry = groupMemberService.Queryable()
                    .Where( m => m.Group.GroupTypeId == groupTypeIdFamilyOrBusiness )
                    .Where( m => m.Group.GroupLocations.Any( gl => gl.Location.Street1.Contains( personSearchOptions.Address ) ) )
                    .Select( a => a.PersonId );

                personSearchQry = personSearchQry.Where( a => personIdAddressQry.Contains( a.Id ) );
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
                    return Queryable( includeDeceased, includeBusinesses )
                        .Where( p =>
                            p.LastName.StartsWith( singleName ) ||
                            previousNamesQry.Any( a => a.PersonAlias.PersonId == p.Id && a.LastName.StartsWith( singleName ) ) );
                }
            }
            else
            {
                if ( firstNames.Any() && lastNames.Any() )
                {
                    var qry = GetByFirstLastName( firstNames.Any() ? firstNames[0] : "", lastNames.Any() ? lastNames[0] : "", includeDeceased, includeBusinesses );
                    for ( var i = 1; i < firstNames.Count; i++ )
                    {
                        qry = qry.Union( GetByFirstLastName( firstNames[i], lastNames[i], includeDeceased, includeBusinesses ) );
                    }

                    // always include a search for just last name using the last two parts of name search
                    if ( nameParts.Count >= 2 )
                    {
                        var lastName = string.Join( " ", nameParts.TakeLast( 2 ) );

                        qry = qry.Union( GetByLastName( lastName, includeDeceased, includeBusinesses ) );
                    }

                    //
                    // If searching for businesses, search by the full name as well to handle "," in the name
                    //
                    if ( includeBusinesses )
                    {
                        qry = qry.Union( GetByLastName( fullName, includeDeceased, includeBusinesses ) );
                    }

                    return qry;
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
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person.
        /// </returns>
        public IQueryable<GroupMember> GetFamilyMembers( int personId, bool includeSelf = false )
        {
            int groupTypeFamilyId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;
            return GetGroupMembers( groupTypeFamilyId, personId, includeSelf );
        }

        /// <summary>
        /// Gets the group members 
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetGroupMembers( int groupTypeId, int personId, bool includeSelf = false )
        {
            var groupMemberService = new GroupMemberService( ( RockContext ) this.Context );

            // construct the linq in a way that will return the group members sorted by the GroupOrder setting of the person
            var groupMembers = groupMemberService.Queryable( true )
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
                .Select( a => a.GroupMember )
                .Where( m => includeSelf || ( m.PersonId != personId && !m.Person.IsDeceased ) );

            return groupMembers.Include( a => a.Person ).Include( a => a.GroupRole );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupMember" /> entities containing the family members of the provided person.
        /// </returns>
        public IQueryable<GroupMember> GetFamilyMembers( Group family, int personId, bool includeSelf = false )
        {
            return new GroupMemberService( ( RockContext ) this.Context ).Queryable( "GroupRole, Person", true )
                .Where( m =>
                    m.GroupId == family.Id &&
                    ( includeSelf || ( m.PersonId != personId && !m.Person.IsDeceased ) ) )
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
            //Uses logic from IQueryable<Person> GetByFullName
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
        public Person GetPersonFromMobilePhoneNumber( string phoneNumber )
        {
            int numberTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
            return Queryable()
                .Where( p => p.PhoneNumbers.Any( n => ( n.CountryCode + n.Number ) == phoneNumber ) )
                .OrderByDescending( p => p.PhoneNumbers.Any( n => ( n.CountryCode + n.Number ) == phoneNumber && n.IsMessagingEnabled ) )
                .ThenByDescending( p => p.PhoneNumbers.Any( n => ( n.CountryCode + n.Number ) == phoneNumber && n.NumberTypeValueId == numberTypeValueId ) )
                .ThenBy( p => p.Id )
                .FirstOrDefault();
        }

        #endregion

        #region Get Person

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
        /// </summary>
        /// <param name="encodedKey">The encoded key.</param>
        /// <returns></returns>
        public override Person GetByUrlEncodedKey( string encodedKey )
        {
            return GetByImpersonationToken( encodedKey, false, null );
        }

        /// <summary>
        /// Gets the Person by impersonation token (rckipid) and validates it against a Rock.Model.PersonToken
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
                        return this.Get( personToken.PersonAlias.PersonId );
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
        /// Returns a <see cref="Rock.Model.Person" /> by their encrypted key value.
        /// </summary>
        /// <param name="encryptedKey">A <see cref="System.String" /> containing an encrypted key value.</param>
        /// <param name="followMerges">if set to <c>true</c> [follow merges].</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> associated with the provided Key, otherwise null.
        /// </returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use GetByEncryptedKey( string encryptedKey, bool followMerges, int? pageId ) instead", true )]
        public Person GetByEncryptedKey( string encryptedKey, bool followMerges )
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
                        // refetch using PersonService using rockContext instead of personTokenRockContext which was used to save the changes to personKey
                        return this.Get( personToken.PersonAlias.PersonId );
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
                return person;
            }

            // NOTE: we only need the followMerges when using PersonTokenUseLegacyFallback since the PersonToken method would already take care of PersonMerge since it is keyed off of PersonAliasId
            if ( followMerges )
            {
                var personAlias = new PersonAliasService( this.Context as RockContext ).GetByAliasEncryptedKey( encryptedKey );
                if ( personAlias != null )
                {
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
        /// Inactivates a person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="reasonNote">The reason note.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete]
        public List<string> InactivatePerson( Person person, Web.Cache.DefinedValueCache reason, string reasonNote )
        {
            History.HistoryChangeList historyChangeList;

            // since this is an obsolete method now, convert the definedValueCache to a DefinedValueCache
            DefinedValueCache cacheReason = null;
            if ( reason != null )
            {
                cacheReason = DefinedValueCache.Get( reason.Id );
            }

            InactivatePerson( person, cacheReason, reasonNote, out historyChangeList );

            return historyChangeList.Select( a => a.Summary ).ToList();
        }

        /// <summary>
        /// Inactivates a person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="reasonNote">The reason note.</param>
        /// <param name="historyChangeList">The history change list.</param>
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
                                        && m.Group.GroupTypeId == peerNetworkGroupType.Id
                                    )
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
			                    WHEN (
					                    [BirthYear] IS NOT NULL
					                    AND [BirthYear] > 1800
					                    )
				                    THEN TRY_CONVERT([date], (((CONVERT([varchar], [BirthYear]) + '-') + CONVERT([varchar], [BirthMonth])) + '-') + CONVERT([varchar], [BirthDay]), (126))
			                    ELSE NULL
			                    END
		                    )
                    FROM Person
                    WHERE IsDeceased = 0
                    AND RecordStatusValueId <> {inactiveStatusId}";

            rockContext = rockContext ?? new RockContext();
            using ( rockContext )
            {
                rockContext.Database.ExecuteSqlCommand( sql );
            }
        }

        /// <summary>
        /// Adds a person alias, known relationship group, implied relationship group, and family for a new person.
        /// Returns the new Family(Group) that was created for the person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="savePersonAttributes">if set to <c>true</c> [save person attributes].</param>
        /// <returns>Family Group</returns>
        public static Group SaveNewPerson( Person person, RockContext rockContext, int? campusId = null, bool savePersonAttributes = false )
        {
            person.FirstName = person.FirstName.FixCase();
            person.NickName = person.NickName.FixCase();
            person.MiddleName = person.MiddleName.FixCase();
            person.LastName = person.LastName.FixCase();

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
        /// Adds the person to family.
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
        /// Adds the person to group.
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

                if ( group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ) )
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

        #endregion

        #region User Preferences
        /// <summary>
        /// Saves a <see cref="Rock.Model.Person">Person's</see> user preference setting by key and SavesChanges()
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> who the preference value belongs to.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key (name) of the preference setting.</param>
        /// <param name="value">The value.</param>
        public static void SaveUserPreference( Person person, string key, string value )
        {
            int? personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;

            using ( var rockContext = new RockContext() )
            {
                var attributeService = new Model.AttributeService( rockContext );
                var attribute = attributeService.Get( personEntityTypeId, string.Empty, string.Empty, key );

                if ( attribute == null )
                {
                    var fieldTypeService = new Model.FieldTypeService( rockContext );
                    var fieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() );

                    attribute = new Model.Attribute();
                    attribute.IsSystem = false;
                    attribute.EntityTypeId = personEntityTypeId;
                    attribute.EntityTypeQualifierColumn = string.Empty;
                    attribute.EntityTypeQualifierValue = string.Empty;
                    attribute.Key = key;
                    attribute.Name = key;
                    attribute.IconCssClass = string.Empty;
                    attribute.DefaultValue = string.Empty;
                    attribute.IsMultiValue = false;
                    attribute.IsRequired = false;
                    attribute.Description = string.Empty;
                    attribute.FieldTypeId = fieldType.Id;
                    attribute.Order = 0;

                    attributeService.Add( attribute );
                    rockContext.SaveChanges();
                }

                var attributeValueService = new Model.AttributeValueService( rockContext );
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, person.Id );

                if ( string.IsNullOrWhiteSpace( value ) )
                {
                    // Delete existing value if no existing value
                    if ( attributeValue != null )
                    {
                        attributeValueService.Delete( attributeValue );
                    }
                }
                else
                {
                    if ( attributeValue == null )
                    {
                        attributeValue = new Model.AttributeValue();
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.EntityId = person.Id;
                        attributeValueService.Add( attributeValue );
                    }

                    attributeValue.Value = value;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Saves the user preferences.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="preferences">The preferences.</param>
        public static void SaveUserPreferences( Person person, Dictionary<string, string> preferences )
        {
            if ( preferences != null )
            {
                int? personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;

                using ( var rockContext = new RockContext() )
                {
                    var attributeService = new Model.AttributeService( rockContext );
                    var attributes = attributeService
                        .GetByEntityTypeQualifier( personEntityTypeId, string.Empty, string.Empty, true )
                        .Where( a => preferences.Keys.Contains( a.Key ) )
                        .ToList();

                    bool wasUpdated = false;
                    foreach ( var attributeKeyValue in preferences )
                    {
                        var attribute = attributes.FirstOrDefault( a => a.Key == attributeKeyValue.Key );

                        if ( attribute == null )
                        {
                            var fieldTypeService = new Model.FieldTypeService( rockContext );
                            var fieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() );

                            attribute = new Model.Attribute();
                            attribute.IsSystem = false;
                            attribute.EntityTypeId = personEntityTypeId;
                            attribute.EntityTypeQualifierColumn = string.Empty;
                            attribute.EntityTypeQualifierValue = string.Empty;
                            attribute.Key = attributeKeyValue.Key;
                            attribute.Name = attributeKeyValue.Key;
                            attribute.IconCssClass = string.Empty;
                            attribute.DefaultValue = string.Empty;
                            attribute.IsMultiValue = false;
                            attribute.IsRequired = false;
                            attribute.Description = string.Empty;
                            attribute.FieldTypeId = fieldType.Id;
                            attribute.Order = 0;

                            attributeService.Add( attribute );

                            wasUpdated = true;
                        }
                    }

                    if ( wasUpdated )
                    {
                        // Save any new attributes
                        rockContext.SaveChanges();

                        // Requery attributes ( so they all have ids )
                        attributes = attributeService
                            .GetByEntityTypeQualifier( personEntityTypeId, string.Empty, string.Empty, true )
                            .Where( a => preferences.Keys.Contains( a.Key ) )
                            .ToList();
                    }

                    var attributeIds = attributes.Select( a => a.Id ).ToList();

                    var attributeValueService = new Model.AttributeValueService( rockContext );
                    var attributeValues = attributeValueService.Queryable( "Attribute" )
                        .Where( v =>
                            attributeIds.Contains( v.AttributeId ) &&
                            v.EntityId.HasValue &&
                            v.EntityId.Value == person.Id )
                        .ToList();

                    wasUpdated = false;
                    foreach ( var attributeKeyValue in preferences )
                    {
                        if ( string.IsNullOrWhiteSpace( attributeKeyValue.Value ) )
                        {
                            foreach ( var attributeValue in attributeValues
                                .Where( v =>
                                    v.Attribute != null &&
                                    v.Attribute.Key == attributeKeyValue.Key )
                                .ToList() )
                            {
                                attributeValueService.Delete( attributeValue );
                                attributeValues.Remove( attributeValue );
                                wasUpdated = true;
                            }
                        }
                        else
                        {
                            var attributeValue = attributeValues
                                .Where( v =>
                                    v.Attribute != null &&
                                    v.Attribute.Key == attributeKeyValue.Key )
                                .FirstOrDefault();

                            if ( attributeValue == null )
                            {
                                var attribute = attributes
                                    .Where( a => a.Key == attributeKeyValue.Key )
                                    .FirstOrDefault();
                                if ( attribute != null )
                                {
                                    attributeValue = new Model.AttributeValue();
                                    attributeValue.AttributeId = attribute.Id;
                                    attributeValue.EntityId = person.Id;
                                    attributeValueService.Add( attributeValue );
                                }
                            }

                            wasUpdated = wasUpdated || ( attributeValue.Value != attributeKeyValue.Value );
                            attributeValue.Value = attributeKeyValue.Value;
                        }
                    }

                    if ( wasUpdated )
                    {
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> user preference value by preference setting's key.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the preference value for.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key name of the preference setting.</param>
        /// <returns>A list of <see cref="System.String"/> containing the values associated with the user's preference setting.</returns>
        public static string GetUserPreference( Person person, string key )
        {
            int? personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attributeService = new Model.AttributeService( rockContext );
                var attribute = attributeService.Get( personEntityTypeId, string.Empty, string.Empty, key );

                if ( attribute != null )
                {
                    var attributeValueService = new Model.AttributeValueService( rockContext );
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, person.Id );
                    if ( attributeValue != null )
                    {
                        return attributeValue.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Deletes a <see cref="Rock.Model.Person">Person's</see> user preference setting by key and SavesChanges()
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> who the preference value belongs to.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key (name) of the preference setting.</param>
        public static void DeleteUserPreference( Person person, string key )
        {
            int? personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;

            using ( var rockContext = new RockContext() )
            {
                var attributeService = new Model.AttributeService( rockContext );
                var attribute = attributeService.Get( personEntityTypeId, string.Empty, string.Empty, key );

                if ( attribute != null )
                {
                    var attributeValueService = new Model.AttributeValueService( rockContext );
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, person.Id );
                    if ( attributeValue != null )
                    {
                        attributeValueService.Delete( attributeValue );
                    }

                    attributeService.Delete( attribute );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Returns all of the user preference settings for a <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the user preference settings for.</param>
        /// <returns>A dictionary containing all of the <see cref="Rock.Model.Person">Person's</see> user preference settings.</returns>
        public static Dictionary<string, string> GetUserPreferences( Person person )
        {
            int? personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;

            var values = new Dictionary<string, string>();

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                foreach ( var attributeValue in new Model.AttributeValueService( rockContext ).Queryable()
                    .Where( v =>
                        v.Attribute.EntityTypeId == personEntityTypeId &&
                        ( v.Attribute.EntityTypeQualifierColumn == null || v.Attribute.EntityTypeQualifierColumn == string.Empty ) &&
                        ( v.Attribute.EntityTypeQualifierValue == null || v.Attribute.EntityTypeQualifierValue == string.Empty ) &&
                        v.EntityId == person.Id ) )
                {
                    values.AddOrReplace( attributeValue.Attribute.Key, attributeValue.Value );
                }
            }

            return values;
        }

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

            // Adult if Age >= 18 OR has a role of Adult in one or more (ANY) families
            var adultBasedOnBirthdateOrFamilyRole = personQuery
                .Where( p => ( p.BirthDate.HasValue && p.BirthDate.Value <= birthDateEighteen )
                    || familyPersonRoleQuery.Where( f => f.PersonId == p.Id ).Any( f => f.GroupRoleId == groupRoleAdultId ) );

            // Child if (not adultBasedOnBirthdateOrFamilyRole) AND (Age < 18 OR child in ALL families)
            var childBasedOnBirthdateOrFamilyRole = personQuery
                .Where( p => !adultBasedOnBirthdateOrFamilyRole.Any( a => a.Id == p.Id )
                    &&
                    ( ( p.BirthDate.HasValue && p.BirthDate.Value > birthDateEighteen )
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
            int recordsUpdated = UpdatePersonsPrimaryFamily( personId, rockContext );
            return recordsUpdated != 0;
        }

        /// <summary>
        /// Ensures the PrimaryFamily is correct for all person records in the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static int UpdatePrimaryFamilyAll( RockContext rockContext )
        {
            return UpdatePersonsPrimaryFamily( null, rockContext );
        }

        /// <summary>
        /// Updates the person primary family for the specified person, or for all persons in the database if personId is null
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static int UpdatePersonsPrimaryFamily( int? personId, RockContext rockContext )
        {
            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;

            // Use raw 'UPDATE SET FROM' update to quickly ensure that the Primary Family on each Person record matches the Calculated Primary Family
            var sqlUpdateBuilder = new StringBuilder();
            sqlUpdateBuilder.Append( $@"
UPDATE x
SET x.PrimaryFamilyId = x.CalculatedPrimaryFamilyId
FROM (
    SELECT p.Id
        ,p.NickName
        ,p.LastName
        ,p.PrimaryFamilyId
        ,pf.CalculatedPrimaryFamilyId
    FROM Person p
    OUTER APPLY (
        SELECT TOP 1 g.Id [CalculatedPrimaryFamilyId]
        FROM GroupMember gm
        JOIN [Group] g ON g.Id = gm.GroupId
        WHERE g.GroupTypeId = {groupTypeIdFamily}
            AND gm.PersonId = p.Id
        ORDER BY gm.GroupOrder
            ,gm.GroupId
        ) pf
    WHERE (
            p.PrimaryFamilyId IS NULL
            OR (p.PrimaryFamilyId != pf.CalculatedPrimaryFamilyId)
            )" );

            if ( personId.HasValue )
            {
                sqlUpdateBuilder.Append( $" AND ( p.Id = @personId) " );
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
        /// Updates the person giving leader identifier for the specified person, or for all persons in the database if personId is null.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static int UpdatePersonGivingLeaderId( int? personId, RockContext rockContext )
        {
            var sqlUpdateBuilder = new StringBuilder();
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
			AND p2.[IsDeceased] = 0
			AND p2.[GivingGroupId] = p.GivingGroupId
		ORDER BY r.[Order]
			,p2.[Gender]
			,p2.[BirthYear]
			,p2.[BirthMonth]
			,p2.[BirthDay]
		) pf
	WHERE (
			p.GivingLeaderId IS NULL
			OR (p.GivingLeaderId != pf.CalculatedGivingLeaderId)
			)" );

            if ( personId.HasValue )
            {
                sqlUpdateBuilder.Append( $" AND ( p.Id = @personId) " );
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

        #endregion
    }
}

// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.Person"/> entity objects.
    /// </summary>
    public partial class PersonService 
    {
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
            return Queryable( includeDeceased, includeBusinesses );
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
        public IQueryable<Person> Queryable( string includes, bool includeDeceased, bool includeBusinesses = true)
        {
            if ( !includeBusinesses )
            {
                var definedValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
                if ( definedValue != null )
                {
                    return base.Queryable( includes )
                        .Where( p =>
                            p.RecordTypeValueId != definedValue.Id &&
                            ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) );
                }
            }

            return base.Queryable( includes )
                .Where( p =>
                    ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) );
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
        public IEnumerable<Person> GetByMatch( string firstName, string lastName, string email, bool includeDeceased = false, bool includeBusinesses = false )
        {
            return Queryable( includeDeceased, includeBusinesses )
                .Where( p => 
                    p.Email == email && 
                    (p.FirstName == firstName || p.NickName == firstName) && 
                    p.LastName == lastName )
                .ToList();
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
            string firstName = string.Empty;
            string lastName = string.Empty;
            string singleName = string.Empty;

            if ( fullName.Contains( ',' ) )
            {
                reversed = true;
                // only split by comma if there is a comma present (for example if 'Smith Jones, Sally' is the search, last name would be 'Smith Jones')
                var nameParts = fullName.Split( ',' );
                lastName = nameParts.Length >= 1 ? nameParts[0].Trim() : string.Empty;
                firstName = nameParts.Length >= 2 ? nameParts[1].Trim() : string.Empty;
            }
            else if ( fullName.Trim().Contains( ' ' ) )
            {
                reversed = false;
                // if no comma, assume the search is in 'firstname lastname' format (note: 'firstname lastname1 lastname2' isn't supported yet)
                var names = fullName.Split( ' ' );
                firstName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                lastName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else
            {
                // no spaces, no commas
                reversed = true;
                singleName = fullName.Trim();
            }

            if ( !string.IsNullOrWhiteSpace( singleName ) )
            {
                if ( allowFirstNameOnly )
                {
                    return Queryable( includeDeceased, includeBusinesses )
                        .Where( p =>
                            p.LastName.StartsWith( singleName ) ||
                            p.FirstName.StartsWith( singleName ) ||
                            p.NickName.StartsWith( singleName ) );
                }
                else
                {
                    return Queryable( includeDeceased, includeBusinesses )
                        .Where( p =>
                            p.LastName.StartsWith( singleName ) );
                }
            }
            else
            {
                int recordTypeBusinessId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                
                return Queryable( includeDeceased, includeBusinesses )
                    .Where( p =>
                        ( includeBusinesses &&  p.RecordTypeValueId.HasValue && p.RecordTypeValueId.Value == recordTypeBusinessId && p.LastName.StartsWith(fullName) )
                        ||
                        (p.LastName.StartsWith( lastName ) &&
                        ( p.FirstName.StartsWith( firstName ) ||
                        p.NickName.StartsWith( firstName ) )) );
            }
        }

        /// <summary>
        /// Gets the by full name ordered.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <param name="allowFirstNameOnly">if set to true, a single value in fullName will also search for matching first names.</param>
        /// <param name="reversed">if set to <c>true</c> [reversed].</param>
        /// <returns></returns>
        public IOrderedQueryable<Person> GetByFullNameOrdered(string fullName, bool includeDeceased, bool includeBusinesses, bool allowFirstNameOnly, out bool reversed)
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
        /// Gets the similiar sounding names.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="excludeIds">The exclude ids.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <returns></returns>
        public List<string> GetSimiliarNames( string fullName, List<int> excludeIds, bool includeDeceased = false, bool includeBusinesses = false )
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

            var similarNames = new List<string>();

            if ( !string.IsNullOrWhiteSpace( firstName ) && !string.IsNullOrWhiteSpace( lastName ) )
            {
                var metaphones = ((RockContext)this.Context).Metaphones;

                string ln1 = string.Empty;
                string ln2 = string.Empty;
                Rock.Utility.DoubleMetaphone.doubleMetaphone( lastName, ref ln1, ref ln2 );
                ln1 = ln1 ?? "";
                ln2 = ln2 ?? "";

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
                    fn1 = fn1 ?? "";
                    fn2 = fn2 ?? "";

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
                        .Select( p => ( reversed ?
                            p.LastName + ", " + p.NickName + ( p.SuffixValueId.HasValue ? " " + p.SuffixValue.Value : "" ) :
                            p.NickName + " " + p.LastName + ( p.SuffixValueId.HasValue ? " " + p.SuffixValue.Value : "" ) ) )
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

        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetFamilies(int personId)
        {
            Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            return new GroupMemberService((RockContext)this.Context).Queryable()
                .Where( m => m.PersonId == personId && m.Group.GroupType.Guid == familyGuid )
                .Select( m => m.Group )
                .Distinct();
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Person" /> entities containing the family members of the provided person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person" /> entities containing the family members of the provided person.
        /// </returns>
        public IQueryable<GroupMember> GetFamilyMembers( int personId, bool includeSelf = false )
        {
            int groupTypeFamilyId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            return new GroupMemberService( (RockContext)this.Context ).Queryable( "Person" )
                .Where( m => m.PersonId == personId && m.Group.GroupTypeId == groupTypeFamilyId )
                .SelectMany( m => m.Group.Members)
                .Where( fm => includeSelf || fm.PersonId != personId )
                .Distinct();
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Person" /> entities containing the family members of the provided person.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person" /> entities containing the family members of the provided person.
        /// </returns>
        public IQueryable<GroupMember> GetFamilyMembers( Group family, int personId, bool includeSelf = false )
        {
            return new GroupMemberService( (RockContext)this.Context ).Queryable( "GroupRole, Person" )
                .Where( m => m.GroupId == family.Id )
                .Where( m => includeSelf || m.PersonId != personId )
                .OrderBy( m => m.GroupRole.Order)
                .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue)
                .ThenByDescending( m => m.Person.Gender )
                .Distinct();
        }

        /// <summary>
        /// Gets the family names.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="includeMemberNames">if set to <c>true</c> [include member names].</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public List<string> GetFamilyNames( int personId, bool includeMemberNames = true, bool includeSelf = true)
        {
            var familyNames = new List<string>();

            foreach(var family in GetFamilies(personId))
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
        /// Gets the first group location.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="locationTypeValueId">The location type value id.</param>
        /// <returns></returns>
        public GroupLocation GetFirstLocation( int personId, int locationTypeValueId )
        {
            Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            return new GroupMemberService( (RockContext)this.Context ).Queryable( "GroupLocations.Location" )
                .Where( m => m.PersonId == personId && m.Group.GroupType.Guid == familyGuid )
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
        public PhoneNumber GetPhoneNumber(Person person, Rock.Web.Cache.DefinedValueCache phoneType)
        {
            return new PhoneNumberService( (RockContext)this.Context ).Queryable()
                .Where( n => n.PersonId == person.Id && n.NumberTypeValueId == phoneType.Id)
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
            if (person != null)
            {
                return person;
            }

            if (followMerges )
            {
                var personAlias = new PersonAliasService( (RockContext)this.Context ).GetByAliasId( id );
                if (personAlias != null)
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
            if (person != null)
            {
                return person;
            }

            if (followMerges )
            {
                var personAlias = new PersonAliasService( (RockContext)this.Context ).GetByAliasGuid( guid );
                if (personAlias != null)
                {
                    return personAlias.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the by encrypted key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public override Person GetByEncryptedKey( string encryptedKey )
        {
            return GetByEncryptedKey( encryptedKey, true );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person" /> by their encrypted key value.
        /// </summary>
        /// <param name="encryptedKey">A <see cref="System.String" /> containing an encrypted key value.</param>
        /// <param name="followMerges">if set to <c>true</c> [follow merges].</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> associated with the provided Key, otherwise null.
        /// </returns>
        public Person GetByEncryptedKey( string encryptedKey, bool followMerges )
        {
            var person = base.GetByEncryptedKey( encryptedKey );
            if (person != null)
            {
                return person;
            }

            if ( followMerges )
            {
                var personAlias = new PersonAliasService( (RockContext)this.Context ).GetByAliasEncryptedKey( encryptedKey );
                if (personAlias != null)
                {
                    return personAlias.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> entity of the provided Person's spouse.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> entity of the Person to retrieve the spouse of.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.</returns>
        public Person GetSpouse( Person person )
        {
            // Spouse is determined if all these conditions are met
            // 1) Adult in the same family as Person (GroupType = Family, GroupRole = Adult, and in same Group)
            // 2) Opposite Gender as Person
            // 3) Both Persons are Married

            Guid adultGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
            int adultRoleId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Roles.First( a => a.Guid == adultGuid ).Id;
            int marriedDefinedValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

            if ( person.MaritalStatusValueId != marriedDefinedValueId )
            {
                return null;
            }

            return GetFamilyMembers(person.Id)
                .Where( m => m.GroupRoleId == adultRoleId )
                .Where( m => m.Person.Gender != person.Gender )
                .Where( m => m.Person.MaritalStatusValueId == marriedDefinedValueId)
                .Select( m => m.Person )
                .FirstOrDefault();
        }

        #endregion

        /// <summary>
        /// Adds a person alias, known relationship group, implied relationship group, and optionally a family group for
        /// a new person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="savePersonAttributes">if set to <c>true</c> [save person attributes].</param>
        /// <returns>Family Group</returns>
        public static Group SaveNewPerson ( Person person, RockContext rockContext, int? campusId = null, bool savePersonAttributes = false )
        {
            // Create/Save Known Relationship Group
            var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
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
            var impliedRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_IMPLIED_RELATIONSHIPS );
            if ( impliedRelationshipGroupType != null )
            {
                var ownerRole = impliedRelationshipGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_IMPLIED_RELATIONSHIPS_OWNER.AsGuid() ) );
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
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( familyGroupType != null )
            {
                var adultRole = familyGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );
                if ( adultRole != null )
                {
                    var groupMember = new GroupMember();
                    groupMember.Person = person;
                    groupMember.GroupRoleId = adultRole.Id;

                    var groupMembers = new List<GroupMember>();
                    groupMembers.Add( groupMember );

                    return GroupService.SaveNewFamily( rockContext, groupMembers, campusId, savePersonAttributes );
                }
            }

            return null;
        }

        #region User Preferences

        /// <summary>
        /// Saves a <see cref="Rock.Model.Person">Person's</see> user preference setting by key.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> who the preference value belongs to.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key (name) of the preference setting.</param>
        /// <param name="value">The value.</param>
        public static void SaveUserPreference( Person person, string key, string value )
        {
            int? PersonEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( Person.USER_VALUE_ENTITY ).Id;

            using ( var rockContext = new RockContext() )
            {
                var attributeService = new Model.AttributeService( rockContext );
                var attribute = attributeService.Get( PersonEntityTypeId, string.Empty, string.Empty, key );

                if ( attribute == null )
                {
                    var fieldTypeService = new Model.FieldTypeService( rockContext );
                    var fieldType = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT.AsGuid() );

                    attribute = new Model.Attribute();
                    attribute.IsSystem = false;
                    attribute.EntityTypeId = PersonEntityTypeId;
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
        /// Returns a <see cref="Rock.Model.Person"/> user preference value by preference setting's key.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the preference value for.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key name of the preference setting.</param>
        /// <returns>A list of <see cref="System.String"/> containing the values associated with the user's preference setting.</returns>
        public static string GetUserPreference( Person person, string key )
        {
            int? PersonEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( Person.USER_VALUE_ENTITY ).Id;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attributeService = new Model.AttributeService( rockContext );
                var attribute = attributeService.Get( PersonEntityTypeId, string.Empty, string.Empty, key );

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
        /// Returns all of the user preference settings for a <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the user preference settings for.</param>
        /// <returns>A dictionary containing all of the <see cref="Rock.Model.Person">Person's</see> user preference settings.</returns>
        public static Dictionary<string, string> GetUserPreferences( Person person )
        {
            int? PersonEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( Person.USER_VALUE_ENTITY ).Id;

            var values = new Dictionary<string, string>();

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                foreach ( var attributeValue in new Model.AttributeValueService( rockContext ).Queryable()
                    .Where( v =>
                        v.Attribute.EntityTypeId == PersonEntityTypeId &&
                        ( v.Attribute.EntityTypeQualifierColumn == null || v.Attribute.EntityTypeQualifierColumn == string.Empty ) &&
                        ( v.Attribute.EntityTypeQualifierValue == null || v.Attribute.EntityTypeQualifierValue == string.Empty ) &&
                        v.EntityId == person.Id ) )
                {
                    values.Add( attributeValue.Attribute.Key, attributeValue.Value );
                }
            }

            return values;
        }

        #endregion

    }
}

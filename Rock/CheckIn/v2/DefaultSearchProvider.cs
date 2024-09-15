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
using System.Linq;

using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Performs family search logic for the check-in system.
    /// </summary>
    internal class DefaultSearchProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets the check-in session.
        /// </summary>
        /// <value>The check-in session.</value>
        protected CheckInSession Session { get; }

        /// <summary>
        /// Gets the check-in template configuration data.
        /// </summary>
        /// <value>The check-in template configuration data.</value>
        protected TemplateConfigurationData TemplateConfiguration => Session.TemplateConfiguration;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSearchProvider"/> class.
        /// </summary>
        /// <param name="session">The check-in session.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="session"/> is <c>null</c>.</exception>
        public DefaultSearchProvider( CheckInSession session )
        {
            Session = session ?? throw new ArgumentNullException( nameof( session ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a query to perform the basic family search based on the term
        /// and search type.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <returns>A queryable for <see cref="Group"/> objects that match the search criteria.</returns>
        public virtual IQueryable<Group> GetFamilySearchQuery( string searchTerm, FamilySearchMode searchType )
        {
            switch ( searchType )
            {
                case FamilySearchMode.PhoneNumber:
                    if ( TemplateConfiguration.FamilySearchType != FamilySearchMode.PhoneNumber && TemplateConfiguration.FamilySearchType != FamilySearchMode.NameAndPhone )
                    {
                        throw new CheckInMessageException( "Searching by phone number is not allowed by the check-in configuration." );
                    }

                    return SearchForFamiliesByPhoneNumber( searchTerm );

                case FamilySearchMode.Name:
                    if ( TemplateConfiguration.FamilySearchType != FamilySearchMode.Name && TemplateConfiguration.FamilySearchType != FamilySearchMode.NameAndPhone )
                    {
                        throw new CheckInMessageException( "Searching by phone number is not allowed by the check-in configuration." );
                    }

                    return SearchForFamiliesByName( searchTerm );

                case FamilySearchMode.NameAndPhone:
                    if ( TemplateConfiguration.FamilySearchType != FamilySearchMode.NameAndPhone )
                    {
                        throw new CheckInMessageException( "Searching by phone number is not allowed by the check-in configuration." );
                    }

                    return searchTerm.Any( c => char.IsLetter( c ) )
                        ? SearchForFamiliesByName( searchTerm )
                        : SearchForFamiliesByPhoneNumber( searchTerm);

                case FamilySearchMode.ScannedId:
                    return SearchForFamiliesByScannedId( searchTerm );

                case FamilySearchMode.FamilyId:
                    return SearchForFamiliesByFamilyId( searchTerm );

                default:
                    throw new ArgumentOutOfRangeException( nameof( searchType ), "Invalid search type specified." );
            }
        }

        /// <summary>
        /// Gets the sorted family identifier search query. This is used during
        /// the family search process to apply the correct sorting and maximum
        /// result limits to the query.
        /// </summary>
        /// <param name="familyQry">The family query to be sorted and limited..</param>
        /// <param name="sortByCampus">The campus to use when sorting the results.</param>
        /// <returns>A queryable of family group identifiers to be included in the results.</returns>
        public virtual IQueryable<int> GetSortedFamilyIdSearchQuery( IQueryable<Group> familyQry, CampusCache sortByCampus )
        {
            var maxResults = TemplateConfiguration.MaximumNumberOfResults ?? 100;
            IQueryable<int> familyIdQry;

            // Handle sorting of the results. We either sort by campus or just
            // take the results as-is.
            if ( sortByCampus != null )
            {
                familyIdQry = familyQry
                    .Select( g => new
                    {
                        g.Id,
                        g.CampusId
                    } )
                    .Distinct()
                    .OrderByDescending( g => g.CampusId.HasValue && g.CampusId.Value == sortByCampus.Id )
                    .Select( g => g.Id );
            }
            else
            {
                familyIdQry = familyQry.Select( g => g.Id ).Distinct();
            }

            // Limit the results.
            if ( maxResults > 0 )
            {
                familyIdQry = familyIdQry.Take( maxResults );
            }

            return familyIdQry;
        }

        /// <summary>
        /// Gets the family member query that contains all the family members
        /// are valid for check-in and a member of one of the specified families.
        /// </summary>
        /// <param name="familyIdQry">The family identifier query specifying which families to include.</param>
        /// <returns>A queryable of <see cref="GroupMember"/> objects.</returns>
        public virtual IQueryable<GroupMember> GetFamilyMemberSearchQuery( IQueryable<int> familyIdQry )
        {
            var familyMemberQry = GetFamilyGroupMemberQuery()
                .Where( gm => familyIdQry.Contains( gm.GroupId )
                    && !string.IsNullOrEmpty( gm.Person.NickName ) );

            if ( TemplateConfiguration.IsInactivePersonExcluded )
            {
                var inactiveValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), Session.RockContext )?.Id;

                if ( inactiveValueId.HasValue )
                {
                    familyMemberQry = familyMemberQry
                        .Where( gm => gm.Person.RecordStatusValueId != inactiveValueId.Value );
                }
            }

            return familyMemberQry;
        }

        /// <summary>
        /// Gets the family search item bags from the queryable.
        /// </summary>
        /// <param name="familyMemberQry">The family member query.</param>
        /// <returns>A list of <see cref="FamilyBag"/> instances.</returns>
        public virtual List<FamilyBag> GetFamilySearchItemBags( IQueryable<GroupMember> familyMemberQry )
        {
            // Pull just the information we need from the database.
            var familyMembers = familyMemberQry
                .Select( gm => new
                {
                    gm.GroupId,
                    GroupName = gm.Group.Name,
                    gm.Group.CampusId,
                    RoleOrder = gm.GroupRole.Order,
                    gm.Person
                } )
                .ToList();

            familyMembers.Select( fm => fm.Person ).LoadAttributes( Session.RockContext );

            // Convert the raw database data into the bags that are understood
            // by different elements of the check-in system.
            var families = familyMembers
                .GroupBy( fm => fm.GroupId )
                .Select( family =>
                {
                    var firstMember = family.First();

                    return new FamilyBag
                    {
                        Id = IdHasher.Instance.GetHash( firstMember.GroupId ),
                        Name = firstMember.GroupName,
                        CampusId = firstMember.CampusId.HasValue
                            ? IdHasher.Instance.GetHash( firstMember.CampusId.Value )
                            : null,
                        Members = family
                            .OrderBy( member => member.RoleOrder )
                            .ThenBy( member => member.Person.BirthYear )
                            .ThenBy( member => member.Person.BirthMonth )
                            .ThenBy( member => member.Person.BirthDay )
                            .ThenBy( member => member.Person.Gender )
                            .ThenBy( member => member.Person.NickName )
                            .Select( member => new FamilyMemberBag
                            {
                                Person = Session.Director.ConversionProvider.GetPersonBag( member.Person ),
                                FamilyId = IdHasher.Instance.GetHash( member.GroupId ),
                                RoleOrder = member.RoleOrder
                            } )
                            .ToList()
                    };
                } )
                .ToList();

            return families;
        }

        /// <summary>
        /// Find all group members that match the specified family
        /// identifier for check-in. This normally includes immediate family
        /// members as well as people associated to the family with one of
        /// the configured "can check-in" known relationships.
        /// </summary>
        /// <param name="familyId">The family identifier.</param>
        /// <returns>A queryable that can be used to load all the group members associated with the family.</returns>
        public virtual IQueryable<GroupMember> GetGroupMembersForFamilyQuery( string familyId )
        {
            var familyMemberQry = GetImmediateFamilyMembersQuery( familyId );
            var canCheckInFamilyMemberQry = GetCanCheckInFamilyMembersQuery( familyId );

            return familyMemberQry.Union( canCheckInFamilyMemberQry );
        }

        /// <summary>
        /// Find the family member that matches the specified person
        /// identifier for check-in. If the family identifier is specified
        /// then it is used to sort the result so the GroupMember record
        /// associated with that family is the one used. If the family
        /// identifer is not specified or not found then the first family GroupMember
        /// record will be returned.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <returns>A queryable that can be used to load this person from the family.</returns>
        /// <exception cref="Exception">Inactive person record status was not found in the database, please check your installation.</exception>
        /// <exception cref="Exception">Family group type was not found in the database, please check your installation.</exception>
        public virtual IQueryable<GroupMember> GetPersonForFamilyQuery( string personId, string familyId )
        {
            var groupMemberService = new GroupMemberService( Session.RockContext );
            var familyGroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), Session.RockContext )?.Id;
            var personIdNumber = IdHasher.Instance.GetId( personId ) ?? 0;
            var familyIdNumber = IdHasher.Instance.GetId( familyId );

            if ( !familyGroupTypeId.HasValue )
            {
                throw new Exception( "Family group type was not found in the database, please check your installation." );
            }

            var qry = groupMemberService.Queryable()
                .Where( gm => gm.GroupTypeId == familyGroupTypeId.Value
                    && gm.PersonId == personIdNumber );

            if ( TemplateConfiguration.IsInactivePersonExcluded )
            {
                var personRecordStatusInactiveId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), Session.RockContext )?.Id;

                if ( !personRecordStatusInactiveId.HasValue )
                {
                    throw new Exception( "Inactive person record status was not found in the database, please check your installation." );
                }

                qry = qry.Where( m => m.Person.RecordStatusValueId != personRecordStatusInactiveId.Value );
            }

            // Make the specified family the first one in the list.
            if ( familyIdNumber.HasValue )
            {
                qry = qry.OrderByDescending( gm => gm.GroupId == familyIdNumber.Value );
            }
            else
            {
                // Order by the primary family.
                qry = qry.OrderByDescending( gm => gm.Person.PrimaryFamilyId.HasValue
                    && gm.GroupId == gm.Person.PrimaryFamilyId );
            }

            // We only want one result.
            return qry.Take( 1 );
        }

        /// <summary>
        /// Gets a queryable for GroupMembers that is pre-filtered to the
        /// minimum requirements to be considered for check-in.
        /// </summary>
        /// <returns>A queryable of <see cref="GroupMember"/> objects.</returns>
        /// <exception cref="Exception">Family group type was not found in the database, please check your installation.</exception>
        /// <exception cref="Exception">Person record type was not found in the database, please check your installation.</exception>
        protected virtual IQueryable<GroupMember> GetFamilyGroupMemberQuery()
        {
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), Session.RockContext )?.Id;
            var personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid(), Session.RockContext )?.Id;

            if ( !familyGroupTypeId.HasValue )
            {
                throw new Exception( "Family group type was not found in the database, please check your installation." );
            }

            if ( !personRecordTypeId.HasValue )
            {
                throw new Exception( "Person record type was not found in the database, please check your installation." );
            }

            return new GroupMemberService( Session.RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gm => gm.GroupTypeId == familyGroupTypeId.Value
                    && !gm.Person.IsDeceased
                    && gm.Person.RecordTypeValueId == personRecordTypeId.Value );
        }

        /// <summary>
        /// Searches for families by full name, last name first.
        /// </summary>
        /// <param name="searchTerm">The family name to search for.</param>
        /// <returns>A queryable of family <see cref="Group"/> objects.</returns>
        protected virtual IQueryable<Group> SearchForFamiliesByName( string searchTerm )
        {
            var personIdQry = new PersonService( Session.RockContext )
                .GetByFullName( searchTerm, false )
                .AsNoTracking()
                .Select( p => p.Id );

            return GetFamilyGroupMemberQuery()
                .Where( gm => personIdQry.Contains( gm.PersonId ) )
                .Select( gm => gm.Group )
                .Distinct();
        }

        /// <summary>
        /// Searches for families by phone number.
        /// </summary>
        /// <param name="searchTerm">The phone number to search for.</param>
        /// <returns>A queryable of family <see cref="Group"/> objects.</returns>
        protected virtual IQueryable<Group> SearchForFamiliesByPhoneNumber( string searchTerm )
        {
            var personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid(), Session.RockContext )?.Id;
            var numericSearchTerm = searchTerm.AsNumeric();

            if ( TemplateConfiguration.MinimumPhoneNumberLength.HasValue && searchTerm.Length < TemplateConfiguration.MinimumPhoneNumberLength.Value )
            {
                throw new CheckInMessageException( $"Search term must be at least {TemplateConfiguration.MinimumPhoneNumberLength} digits." );
            }

            if ( TemplateConfiguration.MaximumPhoneNumberLength.HasValue && searchTerm.Length > TemplateConfiguration.MaximumPhoneNumberLength.Value )
            {
                throw new CheckInMessageException( $"Search term must be at most {TemplateConfiguration.MaximumPhoneNumberLength} digits." );
            }

            if ( !personRecordTypeId.HasValue )
            {
                throw new Exception( "Person record type was not found in the database, please check your installation." );
            }

            var phoneQry = new PhoneNumberService( Session.RockContext )
                .Queryable()
                .AsNoTracking();

            if ( TemplateConfiguration.PhoneSearchType == Enums.CheckIn.PhoneSearchMode.EndsWith )
            {
                var charSearchTerm = numericSearchTerm.ToCharArray();

                Array.Reverse( charSearchTerm );

                var reversedSearchTerm = new string( charSearchTerm );

                phoneQry = phoneQry
                    .Where( pn => pn.NumberReversed.StartsWith( reversedSearchTerm ) );
            }
            else
            {
                phoneQry = phoneQry
                    .Where( pn => pn.Number.Contains( numericSearchTerm ) );
            }

            var personIdQry = phoneQry.Select( pn => pn.PersonId );

            return GetFamilyGroupMemberQuery()
                .Where( gm => personIdQry.Contains( gm.PersonId ) )
                .Select( gm => gm.Group );
        }

        /// <summary>
        /// Searches for families by a scanned identifier.
        /// </summary>
        /// <param name="searchTerm">The scanned identifier to search for.</param>
        /// <returns>A queryable of family <see cref="Group"/> objects.</returns>
        protected virtual IQueryable<Group> SearchForFamiliesByScannedId( string searchTerm )
        {
            var alternateIdValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid(), Session.RockContext )?.Id;

            if ( !alternateIdValueId.HasValue )
            {
                throw new Exception( "Alternate Id search type value was not found in the database, please check your installation." );
            }

            var personIdQry = new PersonSearchKeyService( Session.RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( psk => psk.SearchTypeValueId == alternateIdValueId.Value
                    && psk.SearchValue == searchTerm )
                .Select( psk => psk.PersonAlias.PersonId );

            return GetFamilyGroupMemberQuery()
                .Where( gm => personIdQry.Contains( gm.PersonId ) )
                .Select( gm => gm.Group )
                .Distinct();
        }

        /// <summary>
        /// Searches for families by one or more family identifiers.
        /// </summary>
        /// <param name="searchTerm">The family identifer to search for as a delimited list of integer identifiers.</param>
        /// <returns>A queryable of family <see cref="Group"/> objects.</returns>
        protected virtual IQueryable<Group> SearchForFamiliesByFamilyId( string searchTerm )
        {
            var searchFamilyIds = searchTerm.SplitDelimitedValues().AsIntegerList();

            return GetFamilyGroupMemberQuery()
                .Where( gm => searchFamilyIds.Contains( gm.GroupId ) )
                .Select( gm => gm.Group )
                .Distinct();
        }

        /// <summary>
        /// Gets a queryable that will return all family members that are
        /// part of the specified family. Only <see cref="GroupMember"/>
        /// records that are part of the <see cref="Group"/> specified by
        /// <paramref name="familyId"/> will be returned.
        /// </summary>
        /// <param name="familyId">The unique identifier of the family.</param>
        /// <returns>A queryable of matching <see cref="GroupMember"/> objects.</returns>
        /// <exception cref="Exception">Inactive person record status was not found in the database, please check your installation.</exception>
        protected virtual IQueryable<GroupMember> GetImmediateFamilyMembersQuery( string familyId )
        {
            var groupMemberService = new GroupMemberService( Session.RockContext );
            var qry = groupMemberService.GetByGroupId( IdHasher.Instance.GetId( familyId ) ?? 0 ).AsNoTracking();

            if ( TemplateConfiguration.IsInactivePersonExcluded )
            {
                var personRecordStatusInactiveId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), Session.RockContext )?.Id;

                if ( !personRecordStatusInactiveId.HasValue )
                {
                    throw new Exception( "Inactive person record status was not found in the database, please check your installation." );
                }

                qry = qry.Where( m => m.Person.RecordStatusValueId != personRecordStatusInactiveId.Value );
            }

            return qry;
        }

        /// <summary>
        /// Gets a queryable that will return any group member records with
        /// a valid relationship to any member of the family. This uses the
        /// allowed can check-in roles defined on the template configuration.
        /// </summary>
        /// <param name="familyId">The family identifier.</param>
        /// <returns>A queryable of matching <see cref="GroupMember"/> objects.</returns>
        /// <exception cref="Exception">Known relationship group type was not found in the database, please check your installation.</exception>
        /// <exception cref="Exception">Inactive person record status was not found in the database, please check your installation.</exception>
        /// <exception cref="Exception">Known relationship owner role was not found in the database, please check your installation.</exception>
        protected virtual IQueryable<GroupMember> GetCanCheckInFamilyMembersQuery( string familyId )
        {
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), Session.RockContext );
            int? personRecordStatusInactiveId = null;

            if ( knownRelationshipGroupType == null )
            {
                throw new Exception( "Known relationship group type was not found in the database, please check your installation." );
            }

            if ( TemplateConfiguration.IsInactivePersonExcluded )
            {
                personRecordStatusInactiveId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), Session.RockContext )?.Id;

                if ( !personRecordStatusInactiveId.HasValue )
                {
                    throw new Exception( "Inactive person record status was not found in the database, please check your installation." );
                }
            }

            var knownRelationshipsOwnerGuid = SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();
            var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == knownRelationshipsOwnerGuid );

            if ( ownerRole == null )
            {
                throw new Exception( "Known relationship owner role was not found in the database, please check your installation." );
            }

            var familyMemberPersonIdQry = GetImmediateFamilyMembersQuery( familyId )
                .Select( fm => fm.PersonId );
            var groupMemberService = new GroupMemberService( Session.RockContext );
            var canCheckInRoleIds = knownRelationshipGroupType.Roles
                .Where( r => TemplateConfiguration.CanCheckInKnownRelationshipRoleGuids.Contains( r.Guid ) )
                .Select( r => r.Id )
                .ToList();

            // Get the Known Relationship group ids for each member of the family.
            var relationshipGroupIdQry = groupMemberService
                .Queryable()
                .AsNoTracking()
                .Where( g => g.GroupRoleId == ownerRole.Id
                    && familyMemberPersonIdQry.Contains( g.PersonId ) )
                .Select( g => g.GroupId );

            // Get anyone in any of those groups that has a role flagged as "can check-in".
            var canCheckInFamilyMemberQry = groupMemberService
                .Queryable()
                .AsNoTracking()
                .Where( gm => relationshipGroupIdQry.Contains( gm.GroupId ) );

            canCheckInFamilyMemberQry = CheckInDirector.WhereContains( canCheckInFamilyMemberQry, canCheckInRoleIds, gm => gm.GroupRoleId );

            // If check-in does not allow inactive people then add that
            // check now.
            if ( TemplateConfiguration.IsInactivePersonExcluded )
            {
                canCheckInFamilyMemberQry = canCheckInFamilyMemberQry
                    .Where( gm => gm.Person.RecordStatusReasonValueId != personRecordStatusInactiveId.Value );
            }

            return canCheckInFamilyMemberQry;
        }

        #endregion
    }
}

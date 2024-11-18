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
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Reporting;
using Rock.Utility;
using Rock.Web.Cache;
using Z.EntityFramework.Plus;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.GroupMember"/> entity objects. 
    /// </summary>
    public partial class GroupMemberService
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override GroupMember Get( int id )
        {
            // NOTE: This used to some something special pre-v8, but that is no longer needed, so just call base
            return base.Get( id );
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <returns></returns>
        public Person GetPerson( int groupMemberId )
        {
            return this.AsNoFilter().Where( m => m.Id == groupMemberId ).Select( a => a.Person ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the model with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public override GroupMember Get( Guid guid )
        {
            // NOTE: This used to some something special pre-v8, but that is no longer needed, so just call base
            return base.Get( guid );
        }

        /// <summary>
        /// Returns a queryable for Archived GroupMembers. NOTE: This includes deceased people.
        /// </summary>
        /// <returns></returns>
        public IQueryable<GroupMember> GetArchived()
        {
            // by default, all GroupMember Queries treat 'IsArchived' as a soft-deleted, and therefore don't include those records unless AsNoFilter is used
            return this.AsNoFilter().Where( a => a.IsArchived == true );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see>, excluding 
        /// deceased and archived group members. NOTE: Call GetArchived() or Queryable( false, true ) if you
        /// want archived members.
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers.</see></returns>
        public override IQueryable<GroupMember> Queryable()
        {
            return Queryable( false );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMember's</see>.
        /// </summary>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased members should be included in the results. If <c>true</c> deceased members will
        /// be included, otherwise <c>false</c>.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember"/></returns>
        public IQueryable<GroupMember> Queryable( bool includeDeceased )
        {
            // never include ArchivedMembers unless explicitly requested
            return Queryable( includeDeceased, false );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMember's</see>.
        /// </summary>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased members should be included in the results. If <c>true</c> deceased members will
        /// be included, otherwise <c>false</c>.</param>
        /// <param name="includeArchived">A <see cref="System.Boolean"/> value indicating if archived members should be included in the results. If <c>true</c> archived members will
        /// be included, otherwise they will not.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember"/></returns>
        public IQueryable<GroupMember> Queryable( bool includeDeceased, bool includeArchived )
        {
            IQueryable<GroupMember> qry;

            if ( includeArchived )
            {
                // by default, all GroupMember Queries treat 'IsArchived' as a soft-deleted, and therefore don't include those records unless AsNoFilter is used
                qry = base.AsNoFilter();
            }
            else
            {
                // never include ArchivedMembers unless explicitly calling GetArchived()
                qry = base.Queryable().Where( a => a.IsArchived == false );
            }

            if ( !includeDeceased )
            {
                qry = qry.Where( g => g.Person.IsDeceased == false );
            }

            return qry;
        }

        /// <summary>
        /// Returns a collection of all <see cref="Rock.Model.GroupMember">GroupMembers</see> with eager loading of properties specified in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns>Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> with specified properties eagerly loaded</returns>
        public override IQueryable<GroupMember> Queryable( string includes )
        {
            return Queryable( includes, false );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.GroupMember">GroupMembers</see> with eager loading of properties specified in includes
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a list of properties to be eagerly loaded.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> with specified properties eagerly loaded.</returns>
        public IQueryable<GroupMember> Queryable( string includes, bool includeDeceased )
        {
            // never include ArchivedMembers unless explicitly requested
            return Queryable( includes, includeDeceased, false );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.GroupMember">GroupMembers</see> with eager loading of properties specified in includes
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a list of properties to be eagerly loaded.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <param name="includeArchived">A <see cref="System.Boolean"/> value indicating if archived members should be included in the results. If <c>true</c> archived members will
        /// be included, otherwise they will not.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> with specified properties eagerly loaded.</returns>
        public IQueryable<GroupMember> Queryable( string includes, bool includeDeceased, bool includeArchived )
        {
            IQueryable<GroupMember> qry;

            if ( includeArchived )
            {
                // by default, all GroupMember Queries treat 'IsArchived' as a soft-deleted, and therefore don't include those records unless AsNoFilter is used
                qry = base.AsNoFilter( includes );
            }
            else
            {
                // never include ArchivedMembers unless explicitly calling GetArchived()
                qry = base.Queryable( includes ).Where( a => a.IsArchived == false );
            }

            if ( !includeDeceased )
            {
                qry = qry.Where( g => g.Person.IsDeceased == false );
            }

            return qry;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who are members of a specific group.
        /// </summary>
        /// <param name="groupId">A <see cref="System.Int32"/> representing the Id of a <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who belong to the specified group.
        /// </returns>
        public IQueryable<GroupMember> GetByGroupId( int groupId, bool includeDeceased = false )
        {
            return Queryable( "Person,GroupRole", includeDeceased )
                .Where( t => t.GroupId == groupId )
                .OrderBy( g => g.GroupRole.Order );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who are members of a specific group.
        /// </summary>
        /// <param name="groupGuid">A <see cref="System.Guid"/> representing the Guid of a <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who belong to the specified group.
        /// </returns>
        public IQueryable<GroupMember> GetByGroupGuid( Guid groupGuid, bool includeDeceased = false )
        {
            return Queryable( "Person,GroupRole,Group", includeDeceased )
                .Where( t => t.Group.Guid == groupGuid )
                .OrderBy( g => g.GroupRole.Order );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> by the Id of the <see cref="Rock.Model.Group"/>, the Id of the <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="groupId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="personId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c>
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who match the criteria.</returns>
        public IQueryable<GroupMember> GetByGroupIdAndPersonId( int groupId, int personId, bool includeDeceased = false )
        {
            return GetByGroupId( groupId, includeDeceased ).Where( g => g.PersonId == personId ); 
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.GroupMember"/> that matches the Id of the <see cref="Rock.Model.Group"/>,
        /// the Id of the <see cref="Rock.Model.Person"/>, and the Id of the <see cref="Rock.Model.GroupTypeRole"/>
        /// </summary>
        /// <param name="groupId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="personId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search by.</param>
        /// <param name="groupRoleId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>The first <see cref="Rock.Model.GroupMember"/> that matches the search criteria. If no results found returns null.</returns>
        /// <remarks>
        /// In theory a maximum of one result should be returned, since there is a unique constraint on GroupId, PersonId, and GroupRoleId.
        /// </remarks>
        public GroupMember GetByGroupIdAndPersonIdAndGroupRoleId( int groupId, int personId, int groupRoleId, bool includeDeceased = false )
        {
            return GetByGroupIdAndPersonId( groupId, personId, includeDeceased ).Where( t => t.GroupRoleId == groupRoleId ).FirstOrDefault();
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.GroupMember"/> that matches the Id of the <see cref="Rock.Model.Group"/>,
        /// the Id of the <see cref="Rock.Model.Person"/>, and the Id of the <see cref="Rock.Model.GroupTypeRole"/>. If a 
        /// GroupMember cannot be found with a matching GroupTypeRole, the first GroupMember that matches the Group Id and 
        /// Person Id will be returned (with a different role id).
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public GroupMember GetByGroupIdAndPersonIdAndPreferredGroupRoleId( int groupId, int personId, int groupRoleId, bool includeDeceased = false )
        {
            var members = GetByGroupIdAndPersonId( groupId, personId, includeDeceased ).ToList();
            return members.Where( t => t.GroupRoleId == groupRoleId ).FirstOrDefault() ?? members.FirstOrDefault();
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> by the Id of the <see cref="Rock.Model.GroupTypeRole"/> that the member belongs to.
        /// </summary>
        /// <param name="groupRoleId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember"/> who are members of the specified <see cref="Rock.Model.GroupTypeRole"/>.</returns>
        public IQueryable<GroupMember> GetByGroupRoleId( int groupRoleId, bool includeDeceased = false )
        {
            return Queryable( "Person", includeDeceased ).Where( t => t.GroupRoleId == groupRoleId );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember"/> entities associated with a <see cref="Rock.Model.Person"/> by the Person's PersonId
        /// </summary>
        /// <param name="personId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember"/> entities associated with the specified <see cref="Rock.Model.Person"/></returns>
        public IQueryable<GroupMember> GetByPersonId( int personId )
        {
            return Queryable( "Person", true ).Where( t => t.PersonId == personId );
        }

        /// <summary>
        /// Gets the active leaders of the group
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetLeaders( int groupId )
        {
            return GetByGroupId( groupId, false )
                .Where( t =>
                    t.GroupMemberStatus == GroupMemberStatus.Active &&
                    t.GroupRole.IsLeader );
        }

        /// <summary>
        /// Gets the active leaders of the group who have active emails.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetLeadersWithActiveEmails( int groupId )
        {
            return GetLeaders( groupId )
                .Where( m => !string.IsNullOrEmpty( m.Person.Email ) )
                .Where( m => m.Person.IsEmailActive );
        }

        /// <summary>
        /// Gets the sorted group member list for person.
        /// Ordered by adult males oldest to youngest, adult females oldest to youngest, and then children oldest to youngest.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="showOnlyPrimaryGroup">if set to <c>true</c> [show only primary group].</param>
        /// <returns>IEnumerable&lt;GroupMember&gt;.</returns>
        public IEnumerable<GroupMember> GetSortedGroupMemberListForPerson( int personId, int groupTypeId, bool showOnlyPrimaryGroup )
        {
            var orderedGroupMemberList = new List<GroupMember>();
            var groupMemberList = new List<GroupMember>();
            var groupIds = new List<int>();

            if ( showOnlyPrimaryGroup )
            {
                groupIds.Add( Queryable( true )
                    .Where( m => m.GroupTypeId == groupTypeId && m.PersonId == personId )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .ToList()
                    .Select( m => m.GroupId )
                    .FirstOrDefault() );
            }
            else
            {
                groupIds = Queryable( true )
                    .Where( m => m.GroupTypeId == groupTypeId && m.PersonId == personId )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .ToList()
                    .Select( m => m.GroupId )
                    .Distinct()
                    .ToList();
            }

            foreach ( var groupId in groupIds )
            {
                var members = Queryable( "GroupRole,Person", true )
                    .Where( m => m.GroupId == groupId && m.PersonId != personId )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Id )
                    .ToList();

                // Add adult males
                orderedGroupMemberList.AddRange( members
                    .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) && m.Person.Gender == Gender.Male )
                    .OrderByDescending( m => m.Person.Age ) );

                // Add adult females
                orderedGroupMemberList.AddRange( members
                    .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) && m.Person.Gender != Gender.Male )
                    .OrderByDescending( m => m.Person.Age ) );

                // Add non-adults
                orderedGroupMemberList.AddRange( members
                    .Where( m => !m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                    .OrderByDescending( m => m.Person.Age ) );
            }

            return orderedGroupMemberList;
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="System.String"/> objects representing the first names of each person in a <see cref="Rock.Model.Group"/> ordered by group role, age, and gender
        /// </summary>
        /// <param name="groupId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/>.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>An enumerable collection of <see cref="System.String"/> objects containing the first names of each person in the group.</returns>
        public IEnumerable<string> GetFirstNames( int groupId, bool includeDeceased = false )
        {
            return GetByGroupId( groupId, includeDeceased )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.BirthYear )
                .ThenBy( m => m.Person.BirthMonth )
                .ThenBy( m => m.Person.BirthDay )
                .ThenBy( m => m.Person.Gender )
                .Select( m => m.Person.NickName )
                .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="System.String" /> objects representing the first names of each person in a <see cref="Rock.Model.Group" /> ordered by group role, age, and gender
        /// </summary>
        /// <param name="groupId">A <see cref="System.Int32" /> representing the Id of the <see cref="Rock.Model.Group" />.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean" /> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c>
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns>
        /// An enumerable collection of <see cref="System.String" /> objects containing the first names of each person in the group.
        /// </returns>
        public IEnumerable<string> GetFirstNames( int groupId, bool includeDeceased, bool includeInactive )
        {
            var dvActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            if ( dvActive != null )
            {
                return GetByGroupId( groupId, includeDeceased )
                    .Where( m => m.Person.RecordStatusReasonValueId == dvActive.Id )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Person.BirthYear )
                    .ThenBy( m => m.Person.BirthMonth )
                    .ThenBy( m => m.Person.BirthDay )
                    .ThenBy( m => m.Person.Gender )
                    .Select( m => m.Person.NickName )
                    .ToList();
            }
            else
            {
                return GetFirstNames( groupId, includeDeceased );
            }
        }

        /// <summary>
        /// Gets a list of <see cref="System.Int32"/> PersonIds who's home address matches the given search value.
        /// </summary>
        /// <param name="partialHomeAddress">a partial address search string</param>
        /// <returns>A queryable list of <see cref="System.Int32"/> PersonIds</returns>
        public IQueryable<int> GetPersonIdsByHomeAddress( string partialHomeAddress )
        {
            Guid groupTypefamilyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            Guid homeAddressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            var homeAddressTypeValueId = DefinedValueCache.Get( homeAddressTypeGuid ).Id;

            return Queryable()
                .Where( m => m.Group.GroupType.Guid == groupTypefamilyGuid )
                .SelectMany( g => g.Group.GroupLocations )
                .Where( gl => gl.GroupLocationTypeValueId == homeAddressTypeValueId &&
                    gl.Location.Street1.Contains( partialHomeAddress ) )
                .SelectMany( gl => gl.Group.Members )
                .Select( gm => gm.PersonId ).Distinct();
        }

        /// <summary>
        /// Gets the inverse relationship.
        /// Returns the <see cref="Rock.Model.GroupMember" /> who has an inverse relationship to the provided <see cref="Rock.Model.GroupMember" />.
        /// </summary>
        /// <param name="groupMember">A <see cref="Rock.Model.GroupMember" /> representing the person to find the inverse relationship for.</param>
        /// <param name="createGroup">A <see cref="System.Boolean" /> flag indicating if a new <see cref="Rock.Model.Group" /> can be created
        /// for the person with the inverse relationship.</param>
        /// <returns>
        /// A <see cref="Rock.Model.GroupMember" /> representing the <see cref="Rock.Model.Person" /> with the inverse relationship.
        /// </returns>
        /// <remarks>
        /// In Rock, examples of inverse relationships include: Parent/Child, Can Check In/Check in By, Sibling/Sibling, Grandparent/Grandchild, etc.
        /// </remarks>
        public GroupMember GetInverseRelationship( GroupMember groupMember, bool createGroup )
        {
            var groupRole = groupMember.GroupRole;
            if ( groupRole == null )
            {
                groupRole = Queryable( true )
                    .Where( m => m.Id == groupMember.Id )
                    .Select( m => m.GroupRole )
                    .FirstOrDefault();
            }

            if ( groupRole != null )
            {
                if ( groupRole.Attributes == null )
                {
                    groupRole.LoadAttributes();
                }

                if ( groupRole.Attributes.ContainsKey( "InverseRelationship" ) )
                {
                    Guid knownRelationShipOwnerRoleGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER );

                    // The 'owner' of the group is determined by built-in KnownRelationshipsOwner role or the role that is marked as IsLeader for the group
                    var ownerInfo = Queryable( true )
                        .Where( m =>
                            m.GroupId == groupMember.GroupId &&
                            ( m.GroupRole.Guid.Equals( knownRelationShipOwnerRoleGuid ) || m.GroupRole.IsLeader ) )
                        .Select( m => new
                        {
                            PersonId = m.PersonId,
                            RoleId = m.GroupRoleId
                        } )
                        .FirstOrDefault();

                    int? ownerPersonId = null;
                    int? ownerRoleId = null;

                    if ( ownerInfo != null )
                    {
                        ownerPersonId = ownerInfo.PersonId;
                        ownerRoleId = ownerInfo.RoleId;
                    }

                    if ( ownerPersonId.HasValue && ownerRoleId.HasValue )
                    {
                        // Find related person's group where the person is the Owner
                        // NOTE: The 'owner' of the group is determined by built-in KnownRelationshipsOwner role or the role that is marked as IsLeader for the group
                        var inverseGroup = Queryable( true )
                            .Where( m =>
                                m.PersonId == groupMember.PersonId &&
                                m.Group.GroupTypeId == groupRole.GroupTypeId &&
                                ( m.GroupRole.Guid.Equals( knownRelationShipOwnerRoleGuid ) || m.GroupRole.IsLeader ) )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        if ( inverseGroup == null && createGroup )
                        {
                            var ownerGroupMember = new GroupMember();
                            ownerGroupMember.PersonId = groupMember.PersonId;
                            ownerGroupMember.GroupRoleId = ownerRoleId.Value;

                            inverseGroup = new Group();
                            inverseGroup.Name = groupRole.GroupType.Name;
                            inverseGroup.GroupTypeId = groupRole.GroupTypeId.Value;
                            inverseGroup.Members.Add( ownerGroupMember );
                        }

                        if ( inverseGroup != null )
                        {
                            Guid inverseRoleGuid = Guid.Empty;
                            if ( Guid.TryParse( groupRole.GetAttributeValue( "InverseRelationship" ), out inverseRoleGuid ) )
                            {
                                var inverseGroupMember = Queryable( true )
                                    .Where( m =>
                                        m.PersonId == ownerPersonId &&
                                        m.GroupId == inverseGroup.Id &&
                                        m.GroupRole.Guid.Equals( inverseRoleGuid ) )
                                    .FirstOrDefault();

                                if ( inverseGroupMember == null )
                                {
                                    var inverseRole = new GroupTypeRoleService( ( RockContext ) Context ).Get( inverseRoleGuid );
                                    if ( inverseRole != null )
                                    {
                                        inverseGroupMember = new GroupMember();
                                        inverseGroupMember.PersonId = ownerPersonId.Value;
                                        inverseGroupMember.Group = inverseGroup;
                                        inverseGroupMember.GroupRoleId = inverseRole.Id;
                                        Add( inverseGroupMember );
                                    }
                                }

                                return inverseGroupMember;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the GroupMember to the Group. If a matching 'Archived' GroupMember is found with same role and person, it'll be recovered instead of adding a new record
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns>
        /// Either a new GroupMember or a restored GroupMember record
        /// </returns>
        public GroupMember AddOrRestoreGroupMember( Group group, int personId, int groupRoleId )
        {
            var rockContext = this.Context as RockContext;
            var groupService = new GroupService( rockContext );
            GroupMember archivedGroupMember;
            if ( groupService.ExistsAsArchived( group, personId, groupRoleId, out archivedGroupMember ) )
            {
                this.Restore( archivedGroupMember );
                return archivedGroupMember;
            }
            else
            {
                var groupMember = new GroupMember { Group = group, GroupId = group.Id, PersonId = personId, GroupRoleId = groupRoleId };
                base.Add( groupMember );
                return groupMember;
            }
        }

        /// <summary>
        /// Set the IsArchived fields to false
        /// HINT: Use <see cref="GroupService.ExistsAsArchived"></see> to get the matching archived group member
        /// </summary>
        /// <param name="archivedGroupMember">The archived group member.</param>
        public void Restore( GroupMember archivedGroupMember )
        {
            archivedGroupMember.IsArchived = false;
            archivedGroupMember.ArchivedByPersonAliasId = null;
            archivedGroupMember.ArchivedDateTime = null;
        }

        /// <summary>
        /// Deletes or Archives (Soft-Deletes) GroupMember record depending on GroupType.EnableGroupHistory and if the GroupMember has history snapshots
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( GroupMember item )
        {
            var rockContext = this.Context as RockContext;
            int? groupTypeId = item.Group?.GroupTypeId;

            if ( !groupTypeId.HasValue )
            {
                groupTypeId = new GroupService( rockContext ).GetSelect( item.GroupId, a => a.GroupTypeId );
            }

            var groupTypeCache = GroupTypeCache.Get( groupTypeId.Value );
            if ( groupTypeCache?.EnableGroupHistory == true )
            {
                var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
                if ( groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == item.Id ) )
                {
                    // if the group's GroupType has GroupHistory enabled, and this group member has group member history snapshots, then we need to Archive instead of Delete
                    this.Archive( item, null, false );
                    return true;
                }
            }

            // As discussed in https://github.com/SparkDevNetwork/Rock/issues/4697, we are going to delete
            // the association from any group member assignments that have a reference to this group member.
            var groupMemberAssignmentService  = new GroupMemberAssignmentService( this.Context as RockContext );
            foreach ( var groupMemberAssignment in groupMemberAssignmentService.Queryable().Where( a => a.GroupMemberId == item.Id ) )
            {
                groupMemberAssignmentService.Delete( groupMemberAssignment );
            }

            return base.Delete( item );
        }

        /// <summary>
        /// <para>Deletes or Archives (Soft-Deletes) GroupMember record depending on GroupType.EnableGroupHistory and if the GroupMember has history snapshots
        /// with an option to null the GroupMemberId from Registrant tables.</para>
        /// <para> Note, if the option to remove the GroupMember from registrant tables is not
        /// exercised and the GroupMember is a registrant the the deletion will result in an exception.</para>
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="removeFromRegistrants">if set to <c>true</c> [remove from registrants].</param>
        public void Delete( GroupMember groupMember, bool removeFromRegistrants )
        {
            if ( removeFromRegistrants )
            {
                RegistrationRegistrantService registrantService = new RegistrationRegistrantService( this.Context as RockContext );
                foreach ( var registrant in registrantService.Queryable().Where( r => r.GroupMemberId == groupMember.Id ) )
                {
                    registrant.GroupMemberId = null;
                }
            }

            this.Delete( groupMember );
        }

        /// <summary>
        /// Creates the known relationship if it doesn't already exist
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedPersonId">The related person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        public void CreateKnownRelationship( int personId, int relatedPersonId, int relationshipRoleId )
        {
            var groupMemberService = this;
            var rockContext = this.Context as RockContext;

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            int? ownerRoleId = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )?.Id;
            var relationshipRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Id == relationshipRoleId );
            if ( ownerRoleId == null )
            {
                throw new Exception( "Unable to find known relationships owner role" );
            }

            if ( relationshipRole == null )
            {
                throw new Exception( "Specified relationshipRoleId is not a known relationships role" );
            }

            int? knownRelationshipGroupId = groupMemberService.Queryable( true )
                .Where( m =>
                    m.PersonId == personId &&
                    m.GroupRoleId == ownerRoleId.Value )
                .Select( m => ( int? ) m.GroupId )
                .FirstOrDefault();

            // Create known relationship group if doesn't exist
            if ( !knownRelationshipGroupId.HasValue )
            {
                var groupMember = new GroupMember();
                groupMember.PersonId = personId;
                groupMember.GroupRoleId = ownerRoleId.Value;

                var knownRelationshipGroup = new Group();
                knownRelationshipGroup.Name = knownRelationshipGroupType.Name;
                knownRelationshipGroup.GroupTypeId = knownRelationshipGroupType.Id;
                knownRelationshipGroup.Members.Add( groupMember );

                new GroupService( rockContext ).Add( knownRelationshipGroup );
                rockContext.SaveChanges();
                knownRelationshipGroupId = knownRelationshipGroup.Id;
            }

            // Add relationships
            var relationshipMember = groupMemberService.Queryable( true )
                .FirstOrDefault( m =>
                    m.GroupId == knownRelationshipGroupId.Value &&
                    m.PersonId == relatedPersonId &&
                    m.GroupRoleId == relationshipRoleId );

            if ( relationshipMember == null )
            {
                relationshipMember = new GroupMember();
                relationshipMember.GroupId = knownRelationshipGroupId.Value;
                relationshipMember.PersonId = relatedPersonId;
                relationshipMember.GroupRoleId = relationshipRoleId;
                groupMemberService.Add( relationshipMember );
                rockContext.SaveChanges();
            }

            var inverseGroupMember = groupMemberService.GetInverseRelationship( relationshipMember, true );
            if ( inverseGroupMember != null && inverseGroupMember.Id <= 0 )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets all group members that have a  known relationship of relationshipRoleId type with personId.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        public IQueryable<GroupMember> GetKnownRelationship( int personId, int relationshipRoleId )
        {
            var groupMemberService = this;
            var rockContext = this.Context as RockContext;

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) );
            var relationshipRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Id == relationshipRoleId );
            if ( ownerRole == null )
            {
                throw new Exception( "Unable to find known relationships owner role" );
            }

            if ( relationshipRole == null )
            {
                throw new Exception( "Specified relationshipRoleId is not a known relationships role" );
            }

            // find the personId's "known relationship" group
            int? knownRelationshipGroupId = groupMemberService.Queryable( true )
                .Where( m =>
                    m.PersonId == personId &&
                    m.GroupRoleId == ownerRole.Id )
                .Select( m => m.GroupId )
                .FirstOrDefault();

            // if there was a known relationship group found
            IQueryable<GroupMember> groupMembers = null;
            if ( knownRelationshipGroupId.HasValue )
            {
                // take everyone that has the specified relationship role.
                groupMembers = groupMemberService.Queryable()
                   .Where( gm =>
                      gm.GroupId == knownRelationshipGroupId &&
                      gm.GroupRoleId == relationshipRoleId );
            }

            return groupMembers;
        }

        /// <summary>
        /// Deletes the known relationship.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedPersonId">The related person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        public void DeleteKnownRelationship( int personId, int relatedPersonId, int relationshipRoleId )
        {
            DeleteKnownRelationships( personId, relatedPersonId, new List<int> { relationshipRoleId } );
        }

        /// <summary>
        /// Deletes the known relationship.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedPersonId">The related person identifier.</param>
        /// <param name="relationshipRoleIds">The relationship role identifiers.</param>
        internal void DeleteKnownRelationships( int personId, int relatedPersonId, List<int> relationshipRoleIds )
        {
            var groupMemberService = this;
            var rockContext = this.Context as RockContext;

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) );
            var validRoleIds = knownRelationshipGroupType.Roles.Select( r => r.Id ).ToList();
            var hasInvalidRole = relationshipRoleIds.Any( roleId => !validRoleIds.Contains( roleId ) );

            if ( ownerRole == null )
            {
                throw new Exception( "Unable to find known relationships owner role" );
            }

            if ( hasInvalidRole )
            {
                throw new Exception( "Specified relationshipRoleId is not a known relationships role" );
            }

            var knownRelationshipGroup = groupMemberService.Queryable()
                .Where( m =>
                    m.PersonId == personId &&
                    m.GroupRole.Guid.Equals( ownerRole.Guid ) )
                .Select( m => m.Group )
                .FirstOrDefault();

            if ( knownRelationshipGroup == null )
            {
                // no group, so it doesn't exist
                return;
            }

            // lookup the relationship to delete
            var relationshipMemberQry = groupMemberService.Queryable( true )
                .Where( m =>
                    m.GroupId == knownRelationshipGroup.Id &&
                    m.PersonId == relatedPersonId );

            if ( relationshipRoleIds.Count == 1 )
            {
                relationshipMemberQry = relationshipMemberQry.Where( m => m.GroupRoleId == relationshipRoleIds[0] );
            }
            else
            {
                relationshipMemberQry = relationshipMemberQry.Where( m => relationshipRoleIds.Contains( m.GroupRoleId ) );
            }

            foreach ( var relationshipMember in relationshipMemberQry )
            {
                var inverseGroupMember = groupMemberService.GetInverseRelationship( relationshipMember, true );
                if ( inverseGroupMember != null )
                {
                    groupMemberService.Delete( inverseGroupMember );
                }

                groupMemberService.Delete( relationshipMember );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Reorders the group member group.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public virtual void ReorderGroupMemberGroup( List<GroupMember> items, int oldIndex, int newIndex )
        {
            GroupMember movedItem = items[oldIndex];
            if ( movedItem != null )
            {
                items.RemoveAt( oldIndex );
                if ( newIndex >= items.Count )
                {
                    items.Add( movedItem );
                }
                else
                {
                    items.Insert( newIndex, movedItem );
                }
            }

            SetGroupMemberGroupOrder( items );
        }

        /// <summary>
        /// Ensures that the GroupMember.GroupOrder is set for the sortedList of GroupMembers,
        /// and returns true if any updates to GroupMember.GroupOrder where made
        /// </summary>
        /// <param name="sortedItems">The sorted items.</param>
        /// <returns></returns>
        public virtual bool SetGroupMemberGroupOrder( List<GroupMember> sortedItems )
        {
            bool changesMade = false;
            int order = 0;
            foreach ( GroupMember item in sortedItems )
            {
                if ( item.GroupOrder != order )
                {
                    item.GroupOrder = order;
                    changesMade = true;
                }

                order++;
            }

            return changesMade;
        }

        /// <summary>
        /// Gets the group placement group members.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public List<GroupPlacementGroupMember> GetGroupPlacementGroupMembers( GetGroupPlacementGroupMembersParameters options, Person currentPerson )
        {
            var rockContext = this.Context as RockContext;

            var groupMemberService = new GroupMemberService( rockContext );
            var groupMemberQuery = groupMemberService.Queryable();

            groupMemberQuery = groupMemberQuery
                .Where( a => a.GroupId == options.GroupId && a.GroupRoleId == options.GroupRoleId && a.GroupMemberStatus != GroupMemberStatus.Inactive );

            var registrationInstanceGroupPlacementBlock = BlockCache.Get( options.BlockId );
            if ( registrationInstanceGroupPlacementBlock != null && currentPerson != null )
            {
                var registrationTemplatePlacement = new RegistrationTemplatePlacementService( rockContext ).Get( options.RegistrationTemplatePlacementId );
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( currentPerson, registrationInstanceGroupPlacementBlock );

                const string groupMemberAttributeFilter_GroupTypeId = "GroupMemberAttributeFilter_GroupTypeId_{0}";
                string userPreferenceKey = string.Format( groupMemberAttributeFilter_GroupTypeId, registrationTemplatePlacement.GroupTypeId );

                var attributeFilters = preferences.GetValue( userPreferenceKey ).FromJsonOrNull<Dictionary<int, string>>() ?? new Dictionary<int, string>();
                var parameterExpression = groupMemberService.ParameterExpression;
                Expression groupMemberWhereExpression = null;
                foreach ( var attributeFilter in attributeFilters )
                {
                    var attribute = AttributeCache.Get( attributeFilter.Key );
                    var attributeFilterValues = attributeFilter.Value.FromJsonOrNull<List<string>>();
                    var entityField = EntityHelper.GetEntityFieldForAttribute( attribute );
                    if ( entityField != null && attributeFilterValues != null )
                    {
                        var attributeWhereExpression = ExpressionHelper.GetAttributeExpression( groupMemberService, parameterExpression, entityField, attributeFilterValues );
                        if ( groupMemberWhereExpression == null )
                        {
                            groupMemberWhereExpression = attributeWhereExpression;
                        }
                        else
                        {
                            groupMemberWhereExpression = Expression.AndAlso( groupMemberWhereExpression, attributeWhereExpression );
                        }
                    }
                }

                if ( groupMemberWhereExpression != null )
                {
                    groupMemberQuery = groupMemberQuery.Where( parameterExpression, groupMemberWhereExpression );
                }
            }

            var groupPlacementRegistrants = new List<GroupPlacementRegistrant>();
            if ( options.ApplyRegistrantFilter )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var getGroupPlacementRegistrantsParameters = new GetGroupPlacementRegistrantsParameters
                {
                    FilterFeeId = options.FilterFeeId,
                    RegistrationTemplatePlacementId = options.RegistrationTemplatePlacementId,
                    BlockId = options.BlockId,
                    DisplayedAttributeIds = options.DisplayedRegistrantAttributeIds,
                    FilterFeeOptionIds = options.FilterFeeOptionIds,
                    RegistrantPersonDataViewFilterId = options.RegistrantPersonDataViewFilterId,
                    RegistrationInstanceId = options.RegistrationInstanceId,
                    RegistrationTemplateId = options.RegistrationTemplateId
                };

                groupPlacementRegistrants = registrantService.GetGroupPlacementRegistrants( getGroupPlacementRegistrantsParameters, currentPerson ).Where( a => a.AlreadyPlacedInGroup ).ToList();
                var personIds = groupPlacementRegistrants.Select( a => a.PersonId ).ToList();
                groupMemberQuery = groupMemberQuery.Where( a => personIds.Contains( a.PersonId ) );
            }
            else if ( options.DisplayRegistrantAttributes )
            {
                /*
                 SK - 10-07-2022
                 Even if Apply Registrant Filter is not checked, we still need to find the registrant with No Filter in order to display the registrant attribute.
                 */
                var registrantService = new RegistrationRegistrantService( rockContext );
                var getGroupPlacementRegistrantsParameters = new GetGroupPlacementRegistrantsParameters
                {
                    RegistrationTemplatePlacementId = options.RegistrationTemplatePlacementId,
                    BlockId = options.BlockId,
                    DisplayedAttributeIds = options.DisplayedRegistrantAttributeIds,
                    RegistrationInstanceId = options.RegistrationInstanceId,
                    RegistrationTemplateId = options.RegistrationTemplateId
                };

                groupPlacementRegistrants = registrantService.GetGroupPlacementRegistrants( getGroupPlacementRegistrantsParameters, currentPerson ).Where( a => a.AlreadyPlacedInGroup ).ToList();
            }

            var groupMemberList = groupMemberQuery.ToList();
            var groupPlacementGroupMemberList = new List<GroupPlacementGroupMember>();
            if ( options.DisplayRegistrantAttributes )
            {
                foreach ( var groupMember in groupMemberList )
                {
                    var registrantAttributes = groupPlacementRegistrants.Where( a => a.PersonId == groupMember.PersonId ).Select( a => a.Attributes ).FirstOrDefault();
                    var registrantAttributeValues = groupPlacementRegistrants.Where( a => a.PersonId == groupMember.PersonId ).Select( a => a.AttributeValues ).FirstOrDefault();
                    groupPlacementGroupMemberList.Add( new GroupPlacementGroupMember( groupMember, registrantAttributes, registrantAttributeValues, options ) );
                }
            }
            else
            {
                groupPlacementGroupMemberList = groupMemberList
                .Select( x => new GroupPlacementGroupMember( x, options ) )
                .ToList();
            }

            return groupPlacementGroupMemberList;
        }

        /// <summary>
        /// Takes an existing queryable of group members, and filters
        /// the data to where the member has not had an attendance within
        /// x number of weeks.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="groupId"></param>
        /// <param name="amtOfWeeks">The amt of weeks.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IQueryable&lt;GroupMember&gt;.</returns>
        [RockInternal( "1.15" )]
        internal static IQueryable<GroupMember> WhereMembersWithNoAttendanceForNumberOfWeeks( IQueryable<GroupMember> members, int groupId, int amtOfWeeks, RockContext rockContext )
        {
            var attendanceOccurenceService = new AttendanceService( rockContext );
            var limitDate = RockDateTime.Now.AddDays( amtOfWeeks * -7 );

            // Pull the attendance occurrences for this group.
            var attendedPersonIds = attendanceOccurenceService
                .Queryable()
                .Where( x => x.Occurrence.OccurrenceDate >= limitDate
                && x.DidAttend == true
                && x.Occurrence.GroupId == groupId )
                .Select( a => a.PersonAlias.PersonId );

            return members.Where( m => !attendedPersonIds.Contains( m.PersonId ) );
        }

        /// <summary>
        /// Takes an existing queryable of group members, and filters
        /// the data to where the member has had their first attendance within
        /// x number of weeks.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="groupId"></param>
        /// <param name="amtOfWeeks">The amt of weeks.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IQueryable&lt;GroupMember&gt;.</returns>
        [RockInternal( "1.15" )]
        internal static IQueryable<GroupMember> WhereMembersWhoFirstAttendedWithinNumberOfWeeks( IQueryable<GroupMember> members, int groupId, int amtOfWeeks, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var attendanceOccurenceService = new AttendanceService( rockContext );
            var limitDate = RockDateTime.Now.AddDays( amtOfWeeks * -7 );

            // Pull all of the previous attendances for this member (basically, all of the attendances BEFORE the amount of weeks
            // we're filtering out).
            var previousAttendancesPersonIds = attendanceOccurenceService
                .Queryable()
                .Where( x => x.Occurrence.OccurrenceDate < limitDate
                && x.DidAttend == true
                && x.Occurrence.GroupId == groupId )
                .Select( a => a.PersonAlias.PersonId );

            // Pull all of our attendances within our cut-off.
            var attendedPersonIds = attendanceOccurenceService
                .Queryable()
                .Where( x => x.Occurrence.OccurrenceDate >= limitDate
                && x.DidAttend == true
                && x.Occurrence.GroupId == groupId )
                .Select( a => a.PersonAlias.PersonId );

            // Filter the data to where the group member has no previous attendance, but has an attendance within x number of weeks.
            return members.Where( m => !previousAttendancesPersonIds.Contains( m.PersonId ) && attendedPersonIds.Contains( m.PersonId ) );
        }

        /// <summary>
        /// Gets the members who attended within number of weeks.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="groupId"></param>
        /// <param name="amtOfWeeks">The amt of weeks.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IQueryable&lt;GroupMember&gt;.</returns>
        [RockInternal( "1.15" )]
        internal static IQueryable<GroupMember> WhereMembersWhoAttendedWithinNumberOfWeeks( IQueryable<GroupMember> members, int groupId, int amtOfWeeks, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var attendanceOccurenceService = new AttendanceService( rockContext );
            var limitDate = RockDateTime.Now.AddDays( amtOfWeeks * -7 );

            // Pull the attendance occurrences for this group.
            var attendedPersonIds = attendanceOccurenceService
                .Queryable()
                .Where( x => x.Occurrence.OccurrenceDate >= limitDate
                && x.DidAttend == true
                && x.Occurrence.GroupId == groupId )
                .Select( a => a.PersonAlias.PersonId );

            return members.Where( m => attendedPersonIds.Contains( m.PersonId ) );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupPlacementGroupMember
    {
        /// <summary>
        /// Gets or sets the registration registrant.
        /// </summary>
        /// <value>
        /// The registration registrant.
        /// </value>
        private RegistrationRegistrant RegistrationRegistrant { get; set; }

        /// <summary>
        /// Gets or sets the group member.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        private GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        private GetGroupPlacementGroupMembersParameters Options { get; set; }

        /// <summary>
        /// Gets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        public int Id => this.GroupMember.Id;

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId => this.GroupMember.Person.Id;

        /// <summary>
        /// Gets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName => this.GroupMember.Person.FullName;

        /// <summary>
        /// Gets the person gender.
        /// </summary>
        /// <value>
        /// The person gender.
        /// </value>
        public Gender PersonGender => this.GroupMember.Person.Gender;


        /// <summary>
        /// Gets the group member attributes.
        /// </summary>
        /// <value>
        /// The group member attributes.
        /// </value>
        public Dictionary<string, AttributeCache> GroupMemberAttributes
        {
            get
            {
                if ( !Options.DisplayedGroupMemberAttributeIds.Any() )
                {
                    // don't spend time loading attributes if there aren't any to be displayed
                    return null;
                }

                if ( GroupMember.AttributeValues == null )
                {
                    GroupMember.LoadAttributes();
                }

                var displayedAttributeValues = GroupMember
                        .Attributes.Where( a => Options.DisplayedGroupMemberAttributeIds.Contains( a.Value.Id ) )
                        .ToDictionary( k => k.Key, v => v.Value );

                return displayedAttributeValues;
            }
        }

        /// <summary>
        /// Gets the displayed group member attribute values.
        /// </summary>
        /// <value>
        /// The displayed group member attribute values.
        /// </value>
        public Dictionary<string, AttributeValueCache> GroupMemberAttributeValues
        {
            get
            {
                if ( !Options.DisplayedGroupMemberAttributeIds.Any() )
                {
                    // don't spend time loading attributes if there aren't any to be displayed
                    return null;
                }

                if ( GroupMember.AttributeValues == null )
                {
                    GroupMember.LoadAttributes();
                }

                var displayedAttributeValues = GroupMember
                    .AttributeValues.Where( a => Options.DisplayedGroupMemberAttributeIds.Contains( a.Value.AttributeId ) )
                    .ToDictionary( k => k.Key, v => v.Value );

                return displayedAttributeValues;
            }
        }

        /// <summary>
        /// Gets the registrant attributes.
        /// </summary>
        /// <value>
        /// The group member attributes.
        /// </value>
        public Dictionary<string, AttributeCache> RegistrantAttributes { get; set; }

        /// <summary>
        /// Gets the displayed group member attribute values.
        /// </summary>
        /// <value>
        /// The displayed group member attribute values.
        /// </value>
        public Dictionary<string, AttributeValueCache> RegistrantAttributeValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPlacementRegistrant" /> class.
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="registrantAttributes">The registrant attributes.</param>
        /// <param name="registrantAttributeValues">The registrant attribute values.</param>
        /// <param name="options">The options.</param>
        public GroupPlacementGroupMember( GroupMember groupMember, Dictionary<string, AttributeCache> registrantAttributes, Dictionary<string, AttributeValueCache> registrantAttributeValues, GetGroupPlacementGroupMembersParameters options )
        {
            this.RegistrantAttributes = registrantAttributes;
            this.RegistrantAttributeValues = registrantAttributeValues;
            this.GroupMember = groupMember;
            this.Options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPlacementRegistrant" /> class.
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="options">The options.</param>
        public GroupPlacementGroupMember( GroupMember groupMember, GetGroupPlacementGroupMembersParameters options )
        {
            this.GroupMember = groupMember;
            this.Options = options;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetGroupPlacementGroupMembersParameters
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group role identifier.
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        public int GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the registration template placement identifier.
        /// </summary>
        /// <value>
        /// The registration template placement identifier.
        /// </value>
        public int RegistrationTemplatePlacementId { get; set; }

        /// <summary>
        /// Gets or sets the displayed attribute ids.
        /// </summary>
        /// <value>
        /// The displayed attribute ids.
        /// </value>
        public int[] DisplayedGroupMemberAttributeIds { get; set; } = new int[0];

        /// <summary>
        /// Gets or sets the block identifier.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [apply registrant filter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [apply registrant filter]; otherwise, <c>false</c>.
        /// </value>
        public bool ApplyRegistrantFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display registrant attributes].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display registrant attributes]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayRegistrantAttributes { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        public int? RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the data view filter identifier.
        /// </summary>
        /// <value>
        /// The data view filter identifier.
        /// </value>
        public int? RegistrantPersonDataViewFilterId { get; set; }

        /// <summary>
        /// Gets or sets the displayed registrant attribute ids.
        /// </summary>
        /// <value>
        /// The displayed registrant attribute ids.
        /// </value>
        public int[] DisplayedRegistrantAttributeIds { get; set; } = new int[0];

        /// <summary>
        /// Gets or sets the filter fee identifier.
        /// </summary>
        /// <value>
        /// The filter fee identifier.
        /// </value>
        public int? FilterFeeId { get; set; }

        /// <summary>
        /// Gets the filter fee option ids.
        /// </summary>
        /// <value>
        /// The filter fee option ids.
        /// </value>
        public int[] FilterFeeOptionIds { get; set; } = new int[0];
    }

    #region Extension Methods

    public static partial class GroupMemberExtensionMethods
    {
        /// <summary>
        /// Group members that are in the specified groups.
        /// </summary>
        /// <param name="groupMembers">The group members.</param>
        /// <param name="groupIds">The group ids.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> InGroupList( this IQueryable<GroupMember> groupMembers, List<int> groupIds )
        {
            return groupMembers.Where( m => groupIds.Contains( m.GroupId ) );
        }

        /// <summary>
        /// Group Members that are active.
        /// </summary>
        /// <param name="groupMembers">The group members.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> AreActive( this IQueryable<GroupMember> groupMembers )
        {
            return groupMembers.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
        }

        /// <summary>
        /// Group members that are leaders.
        /// </summary>
        /// <param name="groupMembers">The group members.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> AreLeaders( this IQueryable<GroupMember> groupMembers )
        {
            return groupMembers.Where( m => m.GroupRole.IsLeader );
        }

        /// <summary>
        /// Returns a queryable of group members that are in Security Role Group based on the either <see cref="Group.IsSecurityRole" />
        /// or if <see cref="Group.GroupTypeId"/> is the Security Role Group Type.
        /// </summary>
        public static IQueryable<GroupMember> IsInSecurityRoleGroupOrSecurityRoleGroupType( this IQueryable<GroupMember> groupMemberQuery )
        {
            var groupTypeIdSecurityRole = GroupTypeCache.GetSecurityRoleGroupType()?.Id ?? 0;
            return groupMemberQuery.Where( g => g.Group.IsSecurityRole || g.Group.GroupTypeId == groupTypeIdSecurityRole );
        }
    }

    #endregion
}
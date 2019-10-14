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
using System.Web;

using Rock.Data;
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
            return Queryable("Person,GroupRole", includeDeceased)
                .Where( t => t.GroupId == groupId)
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
            return GetByGroupId(groupId, includeDeceased).Where( g => g.PersonId == personId );
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
        /// Returns an enumerable collection of <see cref="System.String"/> objects representing the first names of each person in a <see cref="Rock.Model.Group"/> ordered by group role, age, and gender
        /// </summary>
        /// <param name="groupId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/>.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>An enumerable collection of <see cref="System.String"/> objects containing the first names of each person in the group.</returns>
        public IEnumerable<string> GetFirstNames( int groupId, bool includeDeceased = false )
        {
            return GetByGroupId(groupId, includeDeceased)
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
                    gl.Location.Street1.Contains(partialHomeAddress) )
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
                                    var inverseRole = new GroupTypeRoleService( (RockContext)Context ).Get( inverseRoleGuid );
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
        /// Restores the archived GroupMember record
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

            if ( !groupTypeId.HasValue)
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

            return base.Delete( item );
        }

        /// <summary>
        /// Deletes or Archives (Soft-Deletes) GroupMember record depending on GroupType.EnableGroupHistory and if the GroupMember has history snapshots
        /// with an option to null the GroupMemberId from Registrant tables
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="removeFromRegistrants">if set to <c>true</c> [remove from registrants].</param>
        public void Delete( GroupMember groupMember, bool removeFromRegistrants )
        {
            RegistrationRegistrantService registrantService = new RegistrationRegistrantService( this.Context as RockContext );
            foreach ( var registrant in registrantService.Queryable().Where( r => r.GroupMemberId == groupMember.Id ) )
            {
                registrant.GroupMemberId = null;
            }

            this.Delete( groupMember );
        }

        /// <summary>
        /// Archives the specified group member with an option to null the GroupMemberId from Registrant tables
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier (leave null to have Rock figure it out)</param>
        /// <param name="removeFromRegistrants">if set to <c>true</c> [remove from registrants].</param>
        public void Archive( GroupMember groupMember, int? currentPersonAliasId, bool removeFromRegistrants )
        {
            RegistrationRegistrantService registrantService = new RegistrationRegistrantService( this.Context as RockContext );
            foreach ( var registrant in registrantService.Queryable().Where( r => r.GroupMemberId == groupMember.Id ) )
            {
                registrant.GroupMemberId = null;
            }

            if ( !currentPersonAliasId.HasValue )
            {
                if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
                {
                    currentPersonAliasId = ( HttpContext.Current.Items["CurrentPerson"] as Person )?.PrimaryAliasId;
                }
            }

            groupMember.IsArchived = true;
            groupMember.ArchivedByPersonAliasId = currentPersonAliasId;
            groupMember.ArchivedDateTime = RockDateTime.Now;
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
            var relationshipMember = groupMemberService.Queryable(true)
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
           int? knownRelationshipGroupId = groupMemberService.Queryable(true)
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
            var relationshipMember = groupMemberService.Queryable(true)
                .FirstOrDefault( m =>
                    m.GroupId == knownRelationshipGroup.Id &&
                    m.PersonId == relatedPersonId &&
                    m.GroupRoleId == relationshipRoleId );

            if ( relationshipMember != null )
            {
                var inverseGroupMember = groupMemberService.GetInverseRelationship( relationshipMember, true );
                if ( inverseGroupMember != null )
                {
                    groupMemberService.Delete( inverseGroupMember );
                }

                groupMemberService.Delete( relationshipMember );
                rockContext.SaveChanges();
            }
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
    }
}

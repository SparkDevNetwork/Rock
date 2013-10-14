//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.GroupMember"/> entity objects. 
    /// </summary>
    public partial class GroupMemberService 
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see>, excluding 
        /// deceased group members
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
            return base.Repository.AsQueryable()
                .Where( g => includeDeceased || !g.Person.IsDeceased.HasValue || !g.Person.IsDeceased.Value );
        }

        /// <summary>
        /// Returns a collection of all <see cref="Rock.Model.GroupMember">GroupMembers</see> with eager loading of properties specfied in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns>Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> with specified properties eagerly loaded</returns>
        public override IQueryable<GroupMember> Queryable( string includes )
        {
            return Queryable( includes, false );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.GroupMember">GroupMembers</see> with eager loading of properties specfied in includes
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a list of properties to be eagerly loaded.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> with specified properties eagerly loaded.</returns>
        public IQueryable<GroupMember> Queryable( string includes, bool includeDeceased )
        {
            return base.Repository.AsQueryable( includes ).Where( g => 
                includeDeceased || !g.Person.IsDeceased.HasValue || !g.Person.IsDeceased.Value );
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
                .OrderBy( g => g.GroupRole.SortOrder );
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
        /// Returns the first <see cref="Rock.Model.GroupMember"/> that mathces the Id of the <see cref="Rock.Model.Group"/>,
        /// the Id of the <see cref="Rock.Model.Person"/>, and the Id fo the <see cref="Rock.Model.GroupRole"/>
        /// </summary>
        /// <param name="groupId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> to search by.</param>
        /// <param name="personId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search by.</param>
        /// <param name="groupRoleId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupRole"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// </param>
        /// <returns>The first <see cref="Rock.Model.GroupMember"/> that matches the search criteria. If no results found returns null.</returns>
        /// <remarks>
        /// In theory a maximum of one result should be returned, since there is a unique constraint on GroupId, PersonId, and GroupRoleId.
        /// </remarks>
        public GroupMember GetByGroupIdAndPersonIdAndGroupRoleId( int groupId, int personId, int groupRoleId, bool includeDeceased = false )
        {
            return GetByGroupIdAndPersonId( groupId, personId, includeDeceased ).Where( t => t.GroupRoleId == groupRoleId ).FirstOrDefault();
        }


        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> by the Id of the <see cref="Rock.Model.GroupRole"/> that the member belongs to.
        /// </summary>
        /// <param name="groupRoleId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupRole"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.GroupMember">GroupMembers</see> should be included. If <c>true</c> 
        /// deceased group members will be included, if <c>false</c> deceased group members will not be included. This parameter defaults to false.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.GroupMembers"/> who are members of the specified <see cref="Rock.Model.GroupRole"/>.</returns>
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
            return Queryable( "Person", true ).Where(  t => t.PersonId == personId );
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
            return GetByGroupId(groupId, includeDeceased).
                OrderBy( m => m.GroupRole.SortOrder ).
                ThenBy( m => m.Person.BirthYear ).ThenBy( m => m.Person.BirthMonth ).ThenBy( m => m.Person.BirthDay ).
                ThenBy( m => m.Person.Gender ).
                Select( m => m.Person.FirstName ).
                ToList();
        }

        /// <summary>
        /// Gets the inverse relationship.
        /// Returns the <see cref="Rock.Model.GroupMember" /> who has an inverse relationship to the provided <see cref="Rock.Model.GroupMember" />.
        /// </summary>
        /// <param name="groupMember">A <see cref="Rock.Model.GroupMember" /> representing the person to find the inverse relationship for.</param>
        /// <param name="createGroup">A <see cref="System.Boolean"/> flag indicating if a new <see cref="Rock.Model.Group"/> can be created 
        /// for the person with the inverse relationship. </param>
        /// <param name="personId">The Id of the <see cref="Rock.Model.Person"/> who has the inverse relationship.</param>
        /// <returns>
        /// A <see cref="Rock.Model.GroupMember"/> representing the <see cref="Rock.Model.Person"/> with the inverse relationship.
        /// </returns>
        /// <remarks>
        /// In RockChMS, examples of inverse relationships include: Parent/Child, Can Check In/Check in By, Sibling/Sibling, Grandparent/Grandchild, etc.
        /// </remarks>
        public GroupMember GetInverseRelationship( GroupMember groupMember, bool createGroup, int? personId )
        {
            var groupRole = groupMember.GroupRole;
            if ( groupRole == null )
            {
                groupRole = Queryable()
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
                    Guid ownerRoleGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER );

                    var memberInfo = Queryable()
                        .Where( m =>
                            m.GroupId == groupMember.GroupId &&
                            m.GroupRole.Guid.Equals( ownerRoleGuid ) )
                        .Select( m => new 
                        {
                            PersonId = m.PersonId,
                            RoleId = m.GroupRoleId
                        } )
                        .FirstOrDefault();

                    int? ownerPersonId = null;
                    int? ownerRoleId = null;

                    if ( memberInfo != null )
                    {
                        ownerPersonId = memberInfo.PersonId;
                        ownerRoleId = memberInfo.RoleId;
                    }

                    if ( ownerPersonId.HasValue && ownerRoleId.HasValue )
                    {
                        // Find related person's group
                        var inverseGroup = Queryable()
                            .Where( m =>
                                m.PersonId == groupMember.PersonId &&
                                m.Group.GroupTypeId == groupRole.GroupTypeId &&
                                m.GroupRole.Guid.Equals( ownerRoleGuid ) )
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
                                var inverseGroupMember = Queryable()
                                    .Where( m =>
                                        m.PersonId == ownerPersonId &&
                                        m.GroupId == inverseGroup.Id &&
                                        m.GroupRole.Guid.Equals( inverseRoleGuid ) )
                                    .FirstOrDefault();

                                if ( inverseGroupMember == null )
                                {
                                    var inverseRole = new GroupRoleService().Get( inverseRoleGuid );
                                    if ( inverseRole != null )
                                    {
                                        inverseGroupMember = new GroupMember();
                                        inverseGroupMember.PersonId = ownerPersonId.Value;
                                        inverseGroupMember.Group = inverseGroup;
                                        inverseGroupMember.GroupRoleId = inverseRole.Id;
                                        Add( inverseGroupMember, personId );
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

    }
}

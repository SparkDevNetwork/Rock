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
    /// Member POCO Service class
    /// </summary>
    public partial class GroupMemberService 
    {
        /// <summary>
        /// Gets a queryable list of group members
        /// </summary>
        /// <returns></returns>
        public override IQueryable<GroupMember> Queryable()
        {
            return Queryable( false );
        }

        /// <summary>
        /// Gets a queryable list of group members
        /// </summary>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> Queryable( bool includeDeceased )
        {
            return base.Repository.AsQueryable()
                .Where( g => includeDeceased || !g.Person.IsDeceased.HasValue || !g.Person.IsDeceased.Value );
        }

        /// <summary>
        /// Gets a list of all group members with eager loading of properties specfied in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        public override IQueryable<GroupMember> Queryable( string includes )
        {
            return Queryable( includes, false );
        }

        /// <summary>
        /// Gets a list of all group members with eager loading of properties specfied in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> Queryable( string includes, bool includeDeceased )
        {
            return base.Repository.AsQueryable( includes ).Where( g => 
                includeDeceased || !g.Person.IsDeceased.HasValue || !g.Person.IsDeceased.Value );
        }

        /// <summary>
        /// Gets Members by Group Id
        /// </summary>
        /// <param name="groupId">Group Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Member objects.
        /// </returns>
        public IQueryable<GroupMember> GetByGroupId( int groupId, bool includeDeceased = false )
        {
            return Queryable("Person,GroupRole", includeDeceased)
                .Where( t => t.GroupId == groupId)
                .OrderBy( g => g.GroupRole.SortOrder );
        }

        /// <summary>
        /// Gets Mebers by group id and person id.
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<GroupMember> GetByGroupIdAndPersonId( int groupId, int personId, bool includeDeceased = false )
        {
            return GetByGroupId(groupId, includeDeceased).Where( g => g.PersonId == personId );
        }

        /// <summary>
        /// Gets Member by Group Id And Person Id And Group Role Id
        /// </summary>
        /// <param name="groupId">Group Id.</param>
        /// <param name="personId">Person Id.</param>
        /// <param name="groupRoleId">Group Role Id.</param>
        /// <returns>Member object.</returns>
        public GroupMember GetByGroupIdAndPersonIdAndGroupRoleId( int groupId, int personId, int groupRoleId, bool includeDeceased = false )
        {
            return GetByGroupIdAndPersonId( groupId, personId, includeDeceased ).Where( t => t.GroupRoleId == groupRoleId ).FirstOrDefault();
        }

        /// <summary>
        /// Gets Members by Group Role Id
        /// </summary>
        /// <param name="groupRoleId">Group Role Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Member objects.
        /// </returns>
        public IQueryable<GroupMember> GetByGroupRoleId( int groupRoleId, bool includeDeceased = false )
        {
            return Queryable( "Person", includeDeceased ).Where( t => t.GroupRoleId == groupRoleId );
        }
        
        /// <summary>
        /// Gets Members by Person Id
        /// </summary>
        /// <param name="personId">Person Id.</param>
        /// <returns>An enumerable list of Member objects.</returns>
        public IQueryable<GroupMember> GetByPersonId( int personId )
        {
            return Queryable( "Person", true ).Where(  t => t.PersonId == personId );
        }

        /// <summary>
        /// Gets the first names of each person in the group ordered by group role, age, and gender
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IEnumerable<string> GetFirstNames( int groupId, bool includeDeceased = false )
        {
            return GetByGroupId(groupId, includeDeceased).
                OrderBy( m => m.GroupRole.SortOrder ).
                ThenBy( m => m.Person.BirthYear ).ThenBy( m => m.Person.BirthMonth ).ThenBy( m => m.Person.BirthDay ).
                ThenBy( m => m.Person.Gender ).
                Select( m => m.Person.FirstName ).
                ToList();
        }
    }
}

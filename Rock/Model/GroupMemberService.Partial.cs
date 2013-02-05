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
            return base.Repository.AsQueryable().Where( g => 
                includeDeceased || !g.Person.IsDeceased.HasValue || !g.Person.IsDeceased.Value );
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
        public IEnumerable<GroupMember> GetByGroupId( int groupId, bool includeDeceased = false )
        {
            return Repository.Find( t => t.GroupId == groupId &&
                ( includeDeceased || !t.Person.IsDeceased.HasValue || !t.Person.IsDeceased.Value ) );
        }

        /// <summary>
        /// Gets Mebers by group id and person id.
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IEnumerable<GroupMember> GetByGroupIdAndPersonId( int groupId, int personId, bool includeDeceased = false )
        {
            return Repository.Find( g =>
                g.GroupId == groupId &&
                g.PersonId == personId &&
                ( includeDeceased || !g.Person.IsDeceased.HasValue || !g.Person.IsDeceased.Value ) );
        }

        /// <summary>
        /// Gets Member by Group Id And Person Id And Group Role Id
        /// </summary>
        /// <param name="groupId">Group Id.</param>
        /// <param name="personId">Person Id.</param>
        /// <param name="groupRoleId">Group Role Id.</param>
        /// <returns>Member object.</returns>
        public GroupMember GetByGroupIdAndPersonIdAndGroupRoleId( int groupId, int personId, int groupRoleId )
        {
            return Repository.FirstOrDefault( t => t.GroupId == groupId && t.PersonId == personId && t.GroupRoleId == groupRoleId );
        }

        /// <summary>
        /// Gets Members by Group Role Id
        /// </summary>
        /// <param name="groupRoleId">Group Role Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Member objects.
        /// </returns>
        public IEnumerable<GroupMember> GetByGroupRoleId( int groupRoleId, bool includeDeceased = false  )
        {
            return Repository.Find( t => 
                t.GroupRoleId == groupRoleId &&
                ( includeDeceased || !t.Person.IsDeceased.HasValue || !t.Person.IsDeceased.Value ) );
        }
        
        /// <summary>
        /// Gets Members by Person Id
        /// </summary>
        /// <param name="personId">Person Id.</param>
        /// <returns>An enumerable list of Member objects.</returns>
        public IEnumerable<GroupMember> GetByPersonId( int personId )
        {
            return Repository.Find( t => t.PersonId == personId );
        }

        /// <summary>
        /// Gets the first names of each person in the group ordered by group role, age, and gender
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IEnumerable<string> GetFirstNames( int groupId, bool includeDeceased = false )
        {
            return Repository.AsQueryable().
                Where( m => m.GroupId == groupId &&
                    ( includeDeceased || !m.Person.IsDeceased.HasValue || !m.Person.IsDeceased.Value ) ).
                OrderBy( m => m.GroupRole.SortOrder ).
                ThenBy( m => m.Person.BirthYear ).ThenBy( m => m.Person.BirthMonth ).ThenBy( m => m.Person.BirthDay ).
                ThenBy( m => m.Person.Gender ).
                Select( m => m.Person.NickName ?? m.Person.GivenName ).
                ToList();
        }
    }
}

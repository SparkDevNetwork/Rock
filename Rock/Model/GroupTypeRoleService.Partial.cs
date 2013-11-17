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
    /// The data access/service class for <see cref="Rock.Model.GroupTypeRole"/> entity object types.
    /// </summary>
    public partial class GroupTypeRoleService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupTypeRole">GroupRoles</see> by SortOrder.
        /// </summary>
        /// <param name="sortOrder">A <see cref="System.Int32"/> representing the SortOrder to search by. This value can be null.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupTypeRole"/> with a SortOrder that matches the provided value.</returns>
        public IEnumerable<GroupTypeRole> GetBySortOrder( int? sortOrder )
        {
            return Repository.Find( t => ( t.Order == sortOrder || ( sortOrder == null && t.Order == null ) ) );
        }

        /// <summary>
        /// Gets the by group type identifier.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupTypeRole> GetByGroupTypeId (int groupTypeId)
        {
            return Repository.AsQueryable()
                .Where( r => r.GroupTypeId == groupTypeId )
                .OrderBy( r => r.Order );
        }
    }
}

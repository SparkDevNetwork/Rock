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
    /// The data access/service class for <see cref="Rock.Model.GroupRole"/> entity object types.
    /// </summary>
    public partial class GroupRoleService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupRole">GroupRoles</see> by SortOrder.
        /// </summary>
        /// <param name="sortOrder">A <see cref="System.Int32"/> representing the SortOrder to search by. This value can be null.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupRoles"/> with a SortOrder that matches the provided value.</returns>
        public IEnumerable<GroupRole> GetBySortOrder( int? sortOrder )
        {
            return Repository.Find( t => ( t.SortOrder == sortOrder || ( sortOrder == null && t.SortOrder == null ) ) );
        }
    }
}

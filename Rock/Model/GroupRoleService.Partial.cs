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
    /// Group Role POCO Service class
    /// </summary>
    public partial class GroupRoleService : Service<GroupRole, GroupRoleDto>
    {
        /// <summary>
        /// Gets the by sort order.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns></returns>
        public IEnumerable<GroupRole> GetBySortOrder( int? sortOrder )
        {
            return Repository.Find( t => ( t.SortOrder == sortOrder || ( sortOrder == null && t.SortOrder == null ) ) );
        }
    }
}

//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Crm
{
    /// <summary>
    /// Group Role POCO Service class
    /// </summary>
    public partial class GroupRoleService : Service<GroupRole, GroupRoleDto>
    {
        /// <summary>
        /// Gets Group Roles by Order
        /// </summary>
        /// <param name="order">Order.</param>
        /// <returns>An enumerable list of GroupRole objects.</returns>
        public IEnumerable<GroupRole> GetByOrder( int? order )
        {
            return Repository.Find( t => ( t.SortOrder == order || ( order == null && t.SortOrder == null ) ) );
        }
    }
}

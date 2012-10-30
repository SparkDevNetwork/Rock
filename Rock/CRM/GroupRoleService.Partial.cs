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
        /// Gets the by sort order.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns></returns>
        public IEnumerable<GroupRole> GetBySortOrder( int? sortOrder )
        {
            return Repository.Find( t => ( t.SortOrder == sortOrder || ( sortOrder == null && t.SortOrder == null ) ) );
        }

        /// <summary>
        /// Determines whether this instance can delete the specified group role id.
        /// </summary>
        /// <param name="groupRoleId">The group role id.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified group role id; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( int groupRoleId, out string errorMessage )
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            var groupType = groupTypeService.Queryable().FirstOrDefault(a => (a.DefaultGroupRoleId ?? 0) == groupRoleId);
            if ( groupType != null )
            {
                errorMessage = "This group role is assigned as a default group role for a group type.";
                return false;
            }
            else
            {
                errorMessage = string.Empty;
                return true;
            }
        }
    }
}

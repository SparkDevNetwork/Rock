//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Groups
{
	/// <summary>
	/// Group Role POCO Service class
	/// </summary>
    public partial class GroupRoleService : Service<GroupRole, DTO.GroupRole>
    {
		/// <summary>
		/// Gets Group Roles by Order
		/// </summary>
		/// <param name="order">Order.</param>
		/// <returns>An enumerable list of GroupRole objects.</returns>
	    public IEnumerable<GroupRole> GetByOrder( int? order )
        {
            return Repository.Find( t => ( t.Order == order || ( order == null && t.Order == null ) ) );
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<DTO.GroupRole> QueryableDTO()
        {
            throw new System.NotImplementedException();
        }
    }
}

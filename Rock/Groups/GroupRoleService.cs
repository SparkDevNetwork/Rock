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
    public partial class GroupRoleService : Service<GroupRole, GroupRoleDTO>
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
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override GroupRole CreateNew()
        {
            return new GroupRole();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<GroupRoleDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new GroupRoleDTO( m ) );
        }
    }
}

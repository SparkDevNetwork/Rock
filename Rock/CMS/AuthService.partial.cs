//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq;

namespace Rock.CMS
{
	public partial class AuthService
	{
        /// <summary>
        /// Gets the authorizations for the entity and action.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public IQueryable<Rock.CMS.Auth> GetAuths( string entityType, int? entityId, string action)
		{
            return Repository.AsQueryable().
                    Where( A => A.EntityType == entityType && 
                        A.EntityId == entityId && 
                        A.Action == action ).
                    OrderBy( A => A.Order );
		}
	}
}
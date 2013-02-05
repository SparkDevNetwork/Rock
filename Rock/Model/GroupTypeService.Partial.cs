//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Group Type POCO Service class
    /// </summary>
    public partial class GroupTypeService 
    {
        /// <summary>
        /// Gets Group Types by Default Group Role Id
        /// </summary>
        /// <param name="defaultGroupRoleId">Default Group Role Id.</param>
        /// <returns>An enumerable list of GroupType objects.</returns>
        public IEnumerable<GroupType> GetByDefaultGroupRoleId( int? defaultGroupRoleId )
        {
            return Repository.Find( t => ( t.DefaultGroupRoleId == defaultGroupRoleId || ( defaultGroupRoleId == null && t.DefaultGroupRoleId == null ) ) );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public override bool Delete( GroupType item, int? personId )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            item.ChildGroupTypes.Clear();
            this.Save( item, personId );

            return base.Delete( item, personId );
        }
    }
}

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
    /// Group Type POCO Service class
    /// </summary>
    public partial class GroupTypeService : Service<GroupType, GroupTypeDto>
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
        /// Determines whether GroupType is used as child group type in the database
        /// </summary>
        /// <param name="groupTypeId">The group type id.</param>
        /// <returns>
        ///   <c>true</c> if [is child group type] [the specified group type id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsChildGroupType( int groupTypeId )
        {
            RockContext context = new RockContext();
            context.Database.Connection.Open();
            var cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText = string.Format( "select count(*) from crmGroupTypeAssociation where ChildGroupTypeId = {0} ", groupTypeId );
            var result = cmd.ExecuteScalar();
            int? count = result as int?;
            return ( count > 0 );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public override bool Delete( GroupType item, int? personId )
        {
            if ( IsChildGroupType( item.Id ) )
            {
                return false;
            }
            
            RockContext context = new RockContext();
            context.Database.Connection.Open();
            var cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText = string.Format( "delete from crmGroupTypeAssociation where ParentGroupTypeId = {0}", item.Id );
            cmd.ExecuteNonQuery();
            item.ChildGroupTypes.Clear();
            return base.Delete( item, personId );
        }
    }
}

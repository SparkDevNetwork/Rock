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
    /// Group POCO Service class
    /// </summary>
    public partial class GroupService 
    {
        /// <summary>
        /// Gets Groups by Group Type Id
        /// </summary>
        /// <param name="groupTypeId">Group Type Id.</param>
        /// <returns>An enumerable list of Group objects.</returns>
        public IEnumerable<Group> GetByGroupTypeId( int groupTypeId )
        {
            return Repository.Find( t => t.GroupTypeId == groupTypeId );
        }

        /// <summary>
        /// Gets Group by Guid
        /// </summary>
        /// <param name="guid">Guid.</param>
        /// <returns>Group object.</returns>
        public Group GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets Groups by Is Security Role
        /// </summary>
        /// <param name="isSecurityRole">Is Security Role.</param>
        /// <returns>An enumerable list of Group objects.</returns>
        public IEnumerable<Group> GetByIsSecurityRole( bool isSecurityRole )
        {
            return Repository.Find( t => t.IsSecurityRole == isSecurityRole );
        }

        /// <summary>
        /// Gets Groups by Parent Group Id
        /// </summary>
        /// <param name="parentGroupId">Parent Group Id.</param>
        /// <returns>An enumerable list of Group objects.</returns>
        public IEnumerable<Group> GetByParentGroupId( int? parentGroupId )
        {
            return Repository.Find( t => ( t.ParentGroupId == parentGroupId || ( parentGroupId == null && t.ParentGroupId == null ) ) );
        }

        /// <summary>
        /// Gets Groups by Parent Group Id And Name
        /// </summary>
        /// <param name="parentGroupId">Parent Group Id.</param>
        /// <param name="name">Name.</param>
        /// <returns>An enumerable list of Group objects.</returns>
        public IEnumerable<Group> GetByParentGroupIdAndName( int? parentGroupId, string name )
        {
            return Repository.Find( t => ( t.ParentGroupId == parentGroupId || ( parentGroupId == null && t.ParentGroupId == null ) ) && t.Name == name );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public override bool Delete( Group item, int? personId )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item, personId );
        }
    }
}

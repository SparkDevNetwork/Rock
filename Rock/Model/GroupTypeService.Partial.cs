// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.GroupType"/> entity objects. This class extends <see cref="Rock.Data.Service"/>.
    /// </summary>
    public partial class GroupTypeService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType"/> entities by the Id of their <see cref="Rock.Model.GroupTypeRole"/>.
        /// </summary>
        /// <param name="defaultGroupRoleId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupType">GroupTypes</see> that use the provided <see cref="Rock.Model.GroupTypeRole"/> as the 
        /// default GroupRole for their member Groups.</returns>
        public IEnumerable<GroupType> GetByDefaultGroupRoleId( int? defaultGroupRoleId )
        {
            return Repository.Find( t => ( t.DefaultGroupRoleId == defaultGroupRoleId || ( defaultGroupRoleId == null && t.DefaultGroupRoleId == null ) ) );
        }

        /// <summary>
        /// Gets the child group types.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetChildGroupTypes(int groupTypeId)
        {
            return Repository.AsQueryable().Where( t => t.ParentGroupTypes.Select( p => p.Id ).Contains( groupTypeId ) );
        }

        /// <summary>
        /// Gets the parent group types.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetParentGroupTypes( int groupTypeId )
        {
            return Repository.AsQueryable().Where( t => t.ChildGroupTypes.Select( p => p.Id ).Contains( groupTypeId ) );
        }

        /// <summary>
        /// Verifies if the specified <see cref="Rock.Model.GroupType"/> can be deleted, and if so deletes it.
        /// </summary>
        /// <param name="item">The <see cref="Rock.Model.GroupType"/> to delete.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is attempting to delete the
        /// <see cref="Rock.Model.GroupType"/>.</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the <see cref="Rock.Model.GroupType"/> was able to be successfully deleted, otherwise <c>false</c>.</returns>
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

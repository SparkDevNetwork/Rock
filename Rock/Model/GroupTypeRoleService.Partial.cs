// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.GroupTypeRole"/> entity object types.
    /// </summary>
    public partial class GroupTypeRoleService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupTypeRole">GroupRoles</see> by SortOrder.
        /// </summary>
        /// <param name="sortOrder">A <see cref="System.Int32"/> representing the SortOrder to search by. This value can be null.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupTypeRole"/> with a SortOrder that matches the provided value.</returns>
        public IEnumerable<GroupTypeRole> GetBySortOrder( int? sortOrder )
        {
            return Queryable().Where( t => t.Order == sortOrder );
        }

        /// <summary>
        /// Gets the by group type identifier.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupTypeRole> GetByGroupTypeId (int groupTypeId)
        {
            return Queryable()
                .Where( r => r.GroupTypeId == groupTypeId )
                .OrderBy( r => r.Order );
        }
    }
}

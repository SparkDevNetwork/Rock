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

using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Extension methods for GroupLocation
    /// </summary>
    public static class GroupLocationExtensions
    {
        /// <summary>
        /// Where the group is active.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<GroupLocation> WhereHasActiveGroup( this IQueryable<GroupLocation> query )
        {
            return query.Where( gl => gl.Group.IsActive );
        }

        /// <summary>
        /// Where the location is active.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<GroupLocation> WhereHasActiveLocation( this IQueryable<GroupLocation> query )
        {
            return query.Where( gl => gl.Location.IsActive );
        }

        /// <summary>
        /// Where the entities are active (deduced from the group and location both being active).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<GroupLocation> WhereDeducedIsActive( this IQueryable<GroupLocation> query )
        {
            return query
                .WhereHasActiveLocation()
                .WhereHasActiveGroup();
        }
    }
}

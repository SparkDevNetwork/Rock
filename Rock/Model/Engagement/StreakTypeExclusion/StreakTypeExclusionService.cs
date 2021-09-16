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
    /// Service/Data access class for <see cref="StreakTypeExclusion"/> entity objects.
    /// </summary>
    public partial class StreakTypeExclusionService
    {
        /// <summary>
        /// Gets the exclusions for the given streak type id
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <returns></returns>
        public IQueryable<StreakTypeExclusion> GetByStreakTypeId( int streakTypeId )
        {
            return Queryable().Where( soe => soe.StreakTypeId == streakTypeId );
        }
    }
}
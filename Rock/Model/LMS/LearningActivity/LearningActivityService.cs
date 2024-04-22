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
using System;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    public partial class LearningActivityService
    {
        /// <summary>
        /// Gets a list of <see cref="LearningActivity"/>s for matching a specified <paramref name="activityFilterPredicate" />.
        /// Includes the <see cref="LearningActivityCompletion">LearningActivityCompletions</see> for each activity by default.
        /// </summary>
        /// <param name="activityFilterPredicate">The predicate for filtering the activities.</param>
        /// <param name="includeCompletions">Whether the LearningActivityCompletions for each LearningActivity should be included.</param>
        /// <returns>A <c>Queryable</c> of LearningActivity matched by the predicate.</returns>
        public IQueryable<LearningActivity> GetClassLearningPlan( Func<LearningActivity, bool> activityFilterPredicate, bool includeCompletions = true )
        {
            return
                includeCompletions ?
                Queryable()
                    .Include( a => a.LearningActivityCompletions )
                    .Where( activityFilterPredicate )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .AsQueryable() :
                Queryable()
                    .Where( activityFilterPredicate )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .AsQueryable();
        }
    }
}

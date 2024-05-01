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

using Rock.Data;

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

        /// <summary>
        /// Calculates completion statistics for a <see cref="LearningActivity"/>.
        /// </summary>
        /// <param name="learningActivity">The learning activity to get statistics for.</param>
        /// <returns>An object containing completion statistics for the LearningActivity.</returns>
        public LearningActivityCompletionStatistics GetCompletionStatistics( LearningActivity learningActivity )
        {
            return GetCompletionStatistics( learningActivity.Id, learningActivity.Points );
        }

        /// <summary>
        /// Calculates completion statistics for a <see cref="LearningActivity"/>.
        /// </summary>
        /// <param name="learningActivityId">The identifier of the learning activity to get statistics for.</param>
        /// <param name="points">The points possible for the learning activity.</param>
        /// <returns>An object containing completion statistics for the LearningActivity.</returns>
        public LearningActivityCompletionStatistics GetCompletionStatistics( int learningActivityId, int points )
        {
            if ( learningActivityId == 0 )
            {
                return new LearningActivityCompletionStatistics();
            }

            // Get all of the completions records for the activity.
            var activityCompletions = new LearningActivityCompletionService( ( RockContext ) Context )
                .Queryable()
                .Include( a => a.LearningActivity )
                .Include( a => a.LearningActivity.LearningClass )
                .Where( a => a.LearningActivityId == learningActivityId )
                .AsNoTracking()
                .Select( a => new
                {
                    a.IsStudentCompleted,
                    a.PointsEarned,
                    a.LearningActivity.LearningClass.LearningGradingSystemId
                } )
                .ToList();

            // If there weren't any completions there are no statistics to calculate.
            if ( !activityCompletions.Any() )
            {
                return new LearningActivityCompletionStatistics();
            }

            var gradingSystemId = activityCompletions.Select( a => a.LearningGradingSystemId ).FirstOrDefault();
            var complete = ( double ) activityCompletions.Count( a => a.IsStudentCompleted );
            var incomplete = ( double ) activityCompletions.Count( a => !a.IsStudentCompleted );
            var percentComplete = complete / ( complete + incomplete ) * 100;

            // For all point averages only consider activities that have been completed.
            var completedActivities = activityCompletions.Where( a => a.IsStudentCompleted ).ToList();

            // If there aren't any completed activities there's no need for additional calculations
            // return the data we've collected so far.
            if ( !completedActivities.Any() )
            {
                return new LearningActivityCompletionStatistics
                {
                    Complete = complete.ToIntSafe(),
                    Incomplete = incomplete.ToIntSafe(),
                    PercentComplete = percentComplete
                };
            }

            var averagePoints = completedActivities.Average( a => a.PointsEarned );
            var averagePercent = points > 0 ? averagePoints / points * 100 : 0;

            var averageGrade = new LearningGradingSystemScaleService( ( RockContext ) Context )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.LearningGradingSystemId == gradingSystemId )
                .OrderByDescending( a => a.ThresholdPercentage )
                .ThenBy( a => a.Id )
                .FirstOrDefault( a => a.ThresholdPercentage.HasValue && averagePercent >= ( double ) a.ThresholdPercentage.Value );

            return new LearningActivityCompletionStatistics
            {
                Complete = complete.ToIntSafe(),
                Incomplete = incomplete.ToIntSafe(),
                PercentComplete = percentComplete,
                AveragePoints = averagePoints.ToIntSafe(),
                AverageGrade = averageGrade,
                AverageGradePercent = averagePercent
            };
        }
    }
}

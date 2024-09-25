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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.ViewModels.Utility;

namespace Rock.Model
{
    public partial class LearningActivityService
    {
        /// <summary>
        /// Creates a new Activity with Attributes by copying values from the specified activity.
        /// </summary>
        /// <param name="key">The identifer to use for retreiving the Activity to use as a template for the copy.</param>
        /// <returns>
        ///     A new Activity whose properties and <see cref="AttributeValue" />s match the properties and <see cref="AttributeValue" />s
        ///     of the Activity whose <paramref name="key"/> was provided.
        /// </returns>
        public LearningActivity Copy( string key )
        {
            var activity = Get( key );
            var newActivity = activity.CloneWithoutIdentity();
            newActivity.Name += " - Copy";
            this.Add( newActivity );
            activity.LoadAttributes();
            newActivity.LoadAttributes();
            newActivity.CopyAttributesFrom( activity );

            var rockContext = this.Context as RockContext;

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                newActivity.SaveAttributeValues( rockContext );
            } );
            return newActivity;
        }

        /// <summary>
        /// Gets the availability criteria based on the configuration mode.
        /// </summary>
        /// <remarks>
        /// Program ConfigurationMode's that don't have the concept of a <see cref="LearningSemester"/>
        /// should not allow calculations based on the 'ClassStartOffset' (the <see cref="LearningSemester.StartDate"/>)..
        /// </remarks>
        /// <param name="configurationMode">The <see cref="LearningProgram.ConfigurationMode"/> for the parent <see cref="LearningClass"/>.</param>
        /// <returns>The list of <see cref="AvailabilityCriteria"/> options available for the <see cref="LearningActivity"/>.</returns>
        public List<ListItemBag> GetAvailabilityCriteria( ConfigurationMode configurationMode )
        {
            var onDemandExclusions = new []{ AvailabilityCriteria.ClassStartOffset };
            return Enum.GetValues( typeof( AvailabilityCriteria ) )
                .Cast<AvailabilityCriteria>()
                .Where( value => configurationMode != ConfigurationMode.OnDemandLearning || !onDemandExclusions.Contains( value ) )
                .Select( value => new ListItemBag
                {
                    Value = value.ConvertToInt().ToString(),
                    Text = value.GetDescription() ?? value.ToString().SplitCase()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivity">LearningActivities</see> matching the specified <paramref name="classId">LearningClassId</paramref>.
        /// Includes the <see cref="LearningActivityCompletion">LearningActivityCompletions</see> for each activity by default.
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> for which to retreive activities.</param>
        /// <param name="includeCompletions">Whether the LearningActivityCompletions for each LearningActivity should be included.</param>
        /// <returns>A <c>Queryable</c> of LearningActivity for the specified LearningClass identifier.</returns>
        public IQueryable<LearningActivity> GetClassLearningPlan( int classId, bool includeCompletions = true )
        {
            return
                includeCompletions ?
                Queryable()
                    .Include( a => a.LearningActivityCompletions )
                    .Include( a => a.LearningClass )
                    .Include( a => a.LearningClass.LearningSemester )
                    .Include( a => a.LearningClass.LearningSemester.LearningProgram )
                    .Where( a => a.LearningClassId == classId )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id ) :
                Queryable()
                    .Include( a => a.LearningClass )
                    .Include( a => a.LearningClass.LearningSemester )
                    .Include( a => a.LearningClass.LearningSemester.LearningProgram )
                    .Where( a => a.LearningClassId == classId )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id );
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

            // If there are no points treat it as a passing grade (100%).
            var averagePercent = points > 0 ? averagePoints / points * 100 : 100;

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

        /// <summary>
        /// Gets the due date criteria based on the configuration mode.
        /// </summary>
        /// <remarks>
        /// Program ConfigurationMode's that don't have the concept of a <see cref="LearningSemester"/>
        /// should not allow calculations based on the 'ClassStartOffset' (the <see cref="LearningSemester.StartDate"/>)..
        /// </remarks>
        /// <param name="configurationMode">The <see cref="LearningProgram.ConfigurationMode"/> for the parent <see cref="LearningClass"/>.</param>
        /// <returns>The list of <see cref="DueDateCriteria"/> options available for the <see cref="LearningActivity"/>.</returns>
        public List<ListItemBag> GetDueDateCriteria( ConfigurationMode configurationMode )
        {
            var onDemandExclusions = new[] { DueDateCriteria.ClassStartOffset };
            return Enum.GetValues( typeof( DueDateCriteria ) )
                .Cast<DueDateCriteria>()
                .Where( value => configurationMode != ConfigurationMode.OnDemandLearning || !onDemandExclusions.Contains( value ) )
                .Select( value => new ListItemBag
                {
                    Value = value.ConvertToInt().ToString(),
                    Text = value.GetDescription() ?? value.ToString().SplitCase()
                } )
                .ToList();
        }
    }
}

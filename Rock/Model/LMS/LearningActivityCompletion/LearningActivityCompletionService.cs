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
using System.Linq;

namespace Rock.Model
{
    public partial class LearningActivityCompletionService
    {
        /// <summary>
        /// Updates the <see cref="LearningActivityCompletion.SentNotificationCommunicationId"/> property
        /// for the provided <paramref name="activityCompletionIds"/>.
        /// </summary>
        /// <param name="activityCompletionIds">List of <see cref="LearningActivityCompletion"/> identifiers to update.</param>
        /// <param name="communicationId">The communication id to set for the given identifiers.</param>
        public void UpdateSentNotificationCommunicationIdProperty( List<int> activityCompletionIds, int communicationId )
        {
            var activityCompletions = Queryable().Where( c => activityCompletionIds.Contains( c.Id ) );

            Context.BulkUpdate( activityCompletions, a => new LearningActivityCompletion { SentNotificationCommunicationId = communicationId } );
        }

        /// <summary>
        /// Gets a new instance of a <see cref="LearningActivityCompletion"/> whose initialized values
        /// are based on the provided parameters.
        /// </summary>
        /// <remarks>
        ///     Available and Due Date calculations are performed.
        /// </remarks>
        /// <param name="activity">The <see cref="LearningActivity"/> the completion record is for.</param>
        /// <param name="participantId">The identifier of the <see cref="LearningParticipant"/> the completion record is for.</param>
        /// <param name="enrollmentDate">The date the participant enrolled in the <see cref="LearningClass"/>.</param>
        /// <param name="programCommunicationId">The SystemCommunicationId of the <see cref="LearningProgram"/> the completion record is for.</param>
        /// <returns>A new untracked <see cref="LearningActivityCompletion"/>.</returns>
        public static LearningActivityCompletion GetNew( LearningActivity activity, int participantId, DateTime? enrollmentDate, int? programCommunicationId )
        {
            var semesterStartDate = activity.LearningClass.LearningSemester.StartDate;

            return new LearningActivityCompletion
            {
                StudentId = participantId,
                LearningActivityId = activity.Id,
                AvailableDateTime = LearningActivity.CalculateAvailableDate(
                          activity.AvailabilityCriteria,
                          activity.AvailableDateDefault,
                          activity.AvailableDateOffset,
                          semesterStartDate,
                          enrollmentDate ),
                DueDate = LearningActivity.CalculateDueDate(
                    activity.DueDateCriteria,
                    activity.DueDateDefault,
                    activity.DueDateOffset,
                    semesterStartDate,
                    enrollmentDate )
            };
        }

        /// <summary>
        /// Gets a new <see cref="LearningActivityCompletion"/> using default values based on the provided parameters.
        /// </summary>
        /// <param name="activity">The <see cref="LearningActivity"/> this <see cref="LearningActivityCompletion"/> is for.</param>
        /// <param name="student">The <see cref="LearningParticipant"/> this <see cref="LearningActivityCompletion"/> is for.</param>
        /// <returns>A new <see cref="LearningActivityCompletion"/> record with default values.</returns>
        public static LearningActivityCompletion GetNew( LearningActivity activity, LearningParticipant student )
        {
            var enrollmentDate = student?.CreatedDateTime;
            var classStartDate = student.LearningClass?.LearningSemester?.StartDate;

            return new LearningActivityCompletion
            {
                LearningActivity = activity,
                LearningActivityId = activity.Id,
                StudentId = student.Id,
                Student = student,
                AvailableDateTime = LearningActivity.CalculateAvailableDate(
                    activity.AvailabilityCriteria,
                    activity.AvailableDateDefault,
                    activity.AvailableDateOffset,
                    classStartDate,
                    enrollmentDate ),
                DueDate = LearningActivity.CalculateDueDate(
                    activity.DueDateCriteria,
                    activity.DueDateDefault,
                    activity.DueDateOffset,
                    classStartDate,
                    enrollmentDate )
            };
        }
    }
}
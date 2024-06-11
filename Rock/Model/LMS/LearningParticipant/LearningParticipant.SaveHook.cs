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

using Rock.Data;

namespace Rock.Model
{
    public partial class LearningParticipant
    {
        /// <summary>
        /// Save hook implementation for <see cref="LearningParticipant"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        new internal class SaveHook : EntitySaveHook<LearningParticipant>
        {
            /// <summary>
            /// Ensures the Particpiant has the necessary <see cref="LearningActivityCompletion"/> records
            /// for the <see cref="LearningClass"/>.
            /// </summary>
            protected override void PreSave()
            {
                base.PreSave();

                if ( PreSaveState == EntityContextState.Added )
                {
                    var isStudent = !Entity.GroupRole.IsLeader;

                    if ( isStudent )
                    {
                        AddCompletionsToContextForParticipant();
                    }
                }
            }

            /// <summary>
            /// Adds all <see cref="LearningActivityCompletion"/> records for a class to the Context for the LearningParticipant.
            /// </summary>
            private void AddCompletionsToContextForParticipant()
            {
                var activityService = new LearningActivityService( RockContext );

                // Get the activity data and transform it into the completions for the student.
                var completionsToAdd = activityService.GetClassLearningPlan( Entity.LearningClassId, false )
                   .Select( a => new
                   {
                       LearningActivityId = a.Id,
                       EnrollmentDate = a.LearningClass.CreatedDateTime,
                       SemesterStart = a.LearningClass.LearningSemester.StartDate,
                       a.AvailableDateCalculationMethod,
                       a.AvailableDateDefault,
                       a.AvailableDateOffset,
                       a.DueDateCalculationMethod,
                       a.DueDateDefault,
                       a.DueDateOffset,
                       a.SendNotificationCommunication,
                       a.Order,
                       NotificationCommunicationId = a.LearningClass.LearningSemester.LearningProgram.SystemCommunicationId
                   } )
                    .ToList()
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.LearningActivityId )
                    .Select( a => new LearningActivityCompletion
                    {
                        StudentId = Entity.Id,
                        LearningActivityId = a.LearningActivityId,
                        AvailableDateTime = LearningActivity.CalculateAvailableDate(
                            a.AvailableDateCalculationMethod,
                            a.AvailableDateDefault,
                            a.AvailableDateOffset,
                            a.SemesterStart,
                            a.EnrollmentDate
                        ),
                        DueDate = LearningActivity.CalculateDueDate( a.DueDateCalculationMethod, a.DueDateDefault, a.DueDateOffset, a.SemesterStart, a.EnrollmentDate ),
                        NotificationCommunicationId = a.SendNotificationCommunication ? ( int? ) a.NotificationCommunicationId : null
                    } );

                new LearningActivityCompletionService( RockContext ).AddRange( completionsToAdd );
            }
        }
    }
}

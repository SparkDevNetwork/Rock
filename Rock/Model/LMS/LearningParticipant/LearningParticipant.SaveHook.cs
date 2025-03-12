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
        internal new class SaveHook : EntitySaveHook<LearningParticipant>
        {
            /// <summary>
            /// Called after the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                base.PreSave();

                if ( State == EntityContextState.Added )
                {
                    if ( !Entity.LearningProgramCompletionId.HasValue && Entity.LearningProgramCompletion == null )
                    {
                        UseExistingOrCreateLearningProgramCompletion();
                    }
                }
            }

            /// <summary>
            /// Creates the learning program completion record to show that this
            /// participant is enrolled in the program.
            /// </summary>
            private void UseExistingOrCreateLearningProgramCompletion()
            {
                // Attempt to get the learning class.
                var learningClass= Entity.LearningClass
                    ?? new LearningClassService( RockContext ).Get( Entity.LearningClassId );

                if ( learningClass == null )
                {
                    return;
                }

                // Attempt to get the learning course.
                var learningCourse = learningClass.LearningCourse
                    ?? new LearningCourseService( RockContext ).Get( learningClass.LearningCourseId );

                if ( learningCourse == null )
                {
                    return;
                }

                // Attempt to get the learning program.
                var learningProgram = learningCourse.LearningProgram
                    ?? new LearningProgramService( RockContext ).Get( learningCourse.LearningProgramId );

                if ( learningProgram == null || !learningProgram.IsCompletionStatusTracked )
                {
                    return;
                }

                // Attempt to get the person alias of the participant.
                var personAliasId = Entity.Person?.PrimaryAliasId
                    ?? new PersonAliasService( RockContext ).GetPrimaryAliasId( Entity.PersonId );

                if ( !personAliasId.HasValue )
                {
                    return;
                }

                // Look for an existing pending completion record. In other
                // words, if they sign up for two courses at once, we only
                // want to create a single completion record. But if they sign
                // up for two courses, complete them (and the program); and then
                // a new course is added to the program which they sign up for
                // then we want to create a new pending completion record.
                var existingLearningProgramCompletion = new LearningProgramCompletionService( RockContext ).Queryable()
                    .Where( lpc => lpc.PersonAlias.PersonId == Entity.PersonId
                        && lpc.LearningProgramId == learningProgram.Id
                        && lpc.CompletionStatus == Enums.Lms.CompletionStatus.Pending )
                    .FirstOrDefault();

                if ( existingLearningProgramCompletion != null )
                {
                    Entity.LearningProgramCompletionId = existingLearningProgramCompletion.Id;
                    return;
                }

                // We couldn't find an existing pending completion record, so create a new one.
                var learningProgramCompletion = new LearningProgramCompletion
                {
                    LearningProgramId = learningProgram.Id,
                    PersonAliasId = personAliasId.Value,
                    CampusId = learningClass.CampusId,
                    StartDate = RockDateTime.Now,
                    CompletionStatus = Enums.Lms.CompletionStatus.Pending
                };

                Entity.LearningProgramCompletion = learningProgramCompletion;
            }
        }
    }
}

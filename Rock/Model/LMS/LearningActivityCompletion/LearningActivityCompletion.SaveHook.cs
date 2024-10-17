﻿// <copyright>
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
using System.Linq;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    public partial class LearningActivityCompletion
    {
        /// <summary>
        /// Save hook implementation for <see cref="LearningActivityCompletion"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<LearningActivityCompletion>
        {
            private History.HistoryChangeList HistoryChanges { get; set; }

            protected override void PreSave()
            {
                base.PreSave();

                LogChanges();
            }

            /// <summary>
            /// Ensures the Class Grades are updated for the <see cref="LearningParticipant"/>.
            /// </summary>
            protected override void PostSave()
            {
                base.PostSave();

                UpdateClassGrades();

                if ( HistoryChanges?.Any() == true )
                {
                    var caption = $"{Entity.Student.Person.FullName} - {Entity.LearningActivity.Name}";
                    HistoryService.SaveChanges(
                        this.RockContext,
                        typeof( LearningActivityCompletion ),
                        SystemGuid.Category.HISTORY_LEARNING_ACTIVITY_COMPLETION.AsGuid(),
                        this.Entity.Id,
                        this.HistoryChanges,
                        caption,
                        null,
                        null,
                        true,
                        this.Entity.ModifiedByPersonAliasId );
                }
            }

            /// <summary>
            /// Updates class grades for the related participant (if the current participant completion status is Incomplete).
            /// </summary>
            private void UpdateClassGrades()
            {
                // Get the student specific learning plan (this will include completion
                // records for all activities in the class - even if the completions aren't persisted).
                var completionDetails = new LearningParticipantService( RockContext )
                    .GetStudentLearningPlan( Entity.StudentId )
                    .Select( a => new
                    {
                        // Convert to decimal for proper precision when calculating grade percent.
                        Possible = ( decimal ) a.LearningActivity.Points,
                        Earned = ( decimal ) a.PointsEarned,

                        // For determining overall class completion and calculating grade based on (facilitator) completed activities.
                        IsStudentOrFacilitatorCompleted = a.IsStudentCompleted || a.IsFacilitatorCompleted,

                        // For getting list of grade scales available.
                        GradingSystemId = a.LearningActivity.LearningClass.LearningGradingSystemId,

                        // For updating grade and completion status.
                        a.Student,

                        // Some activities (assessment) may not clearly indicate if the Facilitator must
                        // grade the activity before it can be considered complete.
                        // Therefore; we are always evaluating grades until the class is considered over.
                        ClassEndDate = a.LearningActivity.LearningClass.LearningSemester.EndDate
                    } );

                var anyCompletionRecord = completionDetails.FirstOrDefault();
                var participant = anyCompletionRecord.Student;
                var isClassOver = anyCompletionRecord.ClassEndDate.HasValue && anyCompletionRecord.ClassEndDate.Value.IsPast();

                // If the class has ended don't recalculate the grade.
                if ( isClassOver )
                {
                    return;
                }

                var gradingSystemId = completionDetails.FirstOrDefault().GradingSystemId;

                var gradedActivities = completionDetails.Where( a => a.IsStudentOrFacilitatorCompleted ).ToList();
                var possiblePoints = gradedActivities.Sum( a => a.Possible );
                var earnedPoints = gradedActivities.Sum( a => a.Earned );
                var gradePercent = possiblePoints > 0 ? earnedPoints / possiblePoints * 100 : 0;

                var gradeScaleEarned = new LearningGradingSystemScaleService( RockContext ).GetEarnedScale( gradingSystemId, gradePercent );
                var currentGradePassFailStatus = gradeScaleEarned != null && gradeScaleEarned.IsPassing ? Enums.Lms.LearningCompletionStatus.Pass : Enums.Lms.LearningCompletionStatus.Fail;
                var hasIncompleteAssignments = completionDetails.Any( a => !a.IsStudentOrFacilitatorCompleted );

                // Set the LearningParticipant current class grade values.
                participant.LearningGradePercent = gradePercent;
                participant.LearningGradingSystemScaleId = gradeScaleEarned.Id;
                participant.LearningCompletionStatus = hasIncompleteAssignments ? Enums.Lms.LearningCompletionStatus.Incomplete : currentGradePassFailStatus;

                // If all assignments have been graded (by a facilitator) and the completion date time hasn't yet been set then set it.
                if ( !hasIncompleteAssignments && !participant.LearningCompletionDateTime.HasValue )
                {
                    participant.LearningCompletionDateTime = RockDateTime.Now;
                }

                RockContext.SaveChanges();
            }

            /// <summary>
            /// Logs audit record
            /// </summary>
            private void LogChanges()
            {
                HistoryChanges = new History.HistoryChangeList();

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "LearningActivityCompletion" );
                            History.EvaluateChange( HistoryChanges, "StudentId", null, Entity.StudentId );
                            History.EvaluateChange( HistoryChanges, "Student", null, Entity.Student?.Person?.FullName );
                            History.EvaluateChange( HistoryChanges, "CompletedByPersonAliasId", null, Entity.CompletedByPersonAliasId );
                            History.EvaluateChange( HistoryChanges, "CompletedByPersonAlias", null, Entity.CompletedByPersonAlias?.Name );
                            History.EvaluateChange( HistoryChanges, "ActivityComponentCompletionJson", null, Entity.ActivityComponentCompletionJson );
                            History.EvaluateChange( HistoryChanges, "AvailableDateTime", null, Entity.AvailableDateTime );
                            History.EvaluateChange( HistoryChanges, "DueDate", null, Entity.DueDate );
                            History.EvaluateChange( HistoryChanges, "CompletedDateTime", null, Entity.CompletedDateTime );
                            History.EvaluateChange( HistoryChanges, "FacilitatorComment", null, Entity.FacilitatorComment );
                            History.EvaluateChange( HistoryChanges, "StudentComment", null, Entity.StudentComment );
                            History.EvaluateChange( HistoryChanges, "GradedByPersonAliasId", null, Entity.GradedByPersonAliasId );
                            History.EvaluateChange( HistoryChanges, "GradedByPersonAlias", null, Entity.GradedByPersonAlias?.Name );
                            History.EvaluateChange( HistoryChanges, "PointsEarned", null, Entity.PointsEarned );
                            History.EvaluateChange( HistoryChanges, "IsStudentCompleted", null, Entity.IsStudentCompleted );
                            History.EvaluateChange( HistoryChanges, "IsFacilitatorCompleted", null, Entity.IsFacilitatorCompleted );
                            History.EvaluateChange( HistoryChanges, "WasCompletedOnTime", null, Entity.WasCompletedOnTime );
                            History.EvaluateChange( HistoryChanges, "NotificationCommunicationId", null, Entity.NotificationCommunicationId );
                            History.EvaluateChange( HistoryChanges, "NotificationCommunication", null, Entity.NotificationCommunication?.Title );
                            History.EvaluateChange( HistoryChanges, "BinaryFileId", null, Entity.BinaryFileId );
                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "LearningActivityCompletion" );
                            break;
                        }
                    case EntityContextState.Modified:
                        {
                            var originalDueDate = ( DateTime? ) this.Entry.OriginalValues["DueDate"];
                            History.EvaluateChange( HistoryChanges, "DueDate", originalDueDate, Entity.DueDate );

                            var originalPointsEarned = this.Entry.OriginalValues["PointsEarned"].ToIntSafe();
                            History.EvaluateChange( HistoryChanges, "PointsEarned", originalPointsEarned, Entity.PointsEarned );

                            var originalCompletionJson = ( string ) this.Entry.OriginalValues["ActivityComponentCompletionJson"];
                            History.EvaluateChange( HistoryChanges, "ActivityComponentCompletionJson", originalCompletionJson, Entity.ActivityComponentCompletionJson );

                            var originalIsFacilitatorCompleted = this.Entry.OriginalValues["IsFacilitatorCompleted"].ConvertToBooleanOrDefault( false );
                            History.EvaluateChange( HistoryChanges, "IsFacilitatorCompleted", originalIsFacilitatorCompleted, Entity.IsFacilitatorCompleted );

                            if ( this.Entry.OriginalValues.ContainsKey( "GradedByPersonAlias" ) )
                            {
                                var originalGradedByPersonAlias = ( this.Entry.OriginalValues["GradedByPersonAlias"] as PersonAlias )?.Name ?? string.Empty;
                                History.EvaluateChange( HistoryChanges, "GradedByPersonAlias", originalGradedByPersonAlias, Entity.GradedByPersonAlias?.Name ?? string.Empty );
                            }

                            var originalGradedByPersonAliasId = this.Entry.OriginalValues["GradedByPersonAliasId"] as int?;
                            History.EvaluateChange( HistoryChanges, "GradedByPersonAliasId", originalGradedByPersonAliasId, Entity.GradedByPersonAliasId );

                            var originalFacilitatorComment = ( string ) this.Entry.OriginalValues["FacilitatorComment"];
                            History.EvaluateChange( HistoryChanges, "FacilitatorComment", originalFacilitatorComment, Entity.FacilitatorComment );

                            break;
                        }
                }
            }
        }
    }
}

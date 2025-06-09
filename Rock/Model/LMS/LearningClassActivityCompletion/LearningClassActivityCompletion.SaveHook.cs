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

using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LearningClassActivityCompletion
    {
        /// <summary>
        /// Save hook implementation for <see cref="LearningClassActivityCompletion"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<LearningClassActivityCompletion>
        {
            private History.HistoryChangeList HistoryChanges { get; set; }

            /// <summary>
            /// <c>true</c> if the points earned for an already graded activity were changed; otherwise <c>false</c>.
            /// </summary>
            private bool WasRegraded { get; set; }

            /// <summary>
            /// The configured <see cref="WorkflowType" /> to launch
            /// if the <see cref="LearningClassActivity.CompletionWorkflowType"/>
            /// is not null and the activity was just completed.
            /// </summary>
            private WorkflowType ActivityCompletionWorkflowToLaunch { get; set; }

            protected override void PreSave()
            {
                base.PreSave();

                // Ensure the WasCompletedOnTime property stays current
                // when changes are made to the DueDate of the Completion.
                if ( Entity.IsStudentCompleted || Entity.IsFacilitatorCompleted )
                {
                    Entity.WasCompletedOnTime = Entity.DueDate >= RockDateTime.Now;
                }

                SetWasRegraded();

                SetActivityCompletionWorkflowToLaunch();

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
                    var caption = $"{Entity.Student.Person.FullName} - {Entity.LearningClassActivity.Name}";
                    HistoryService.SaveChanges(
                        this.RockContext,
                        typeof( LearningClassActivityCompletion ),
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
            /// Logs audit record
            /// </summary>
            private void LogChanges()
            {
                HistoryChanges = new History.HistoryChangeList();

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "LearningClassActivityCompletion" );
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
                            History.EvaluateChange( HistoryChanges, "SentNotificationCommunicationId", null, Entity.SentNotificationCommunicationId );
                            History.EvaluateChange( HistoryChanges, "SentNotificationCommunication", null, Entity.SentNotificationCommunication?.Subject );
                            History.EvaluateChange( HistoryChanges, "BinaryFileId", null, Entity.BinaryFileId );
                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "LearningClassActivityCompletion" );
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

                            var originalIsStudentCompleted = this.Entry.OriginalValues["IsStudentCompleted"].ConvertToBooleanOrDefault( false );
                            History.EvaluateChange( HistoryChanges, "IsStudentCompleted", originalIsStudentCompleted, Entity.IsStudentCompleted );

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
                        Possible = a.LearningClassActivity.Points,
                        Earned = a.PointsEarned,

                        // For determining overall class completion and calculating grade based on (facilitator) completed activities.
                        IsStudentOrFacilitatorCompleted = a.IsStudentCompleted || a.IsFacilitatorCompleted,

                        // Don't include ungraded items.
                        a.RequiresGrading,

                        // For getting list of grade scales available.
                        GradingSystemId = a.LearningClassActivity.LearningClass.LearningGradingSystemId,

                        // For updating grade and completion status.
                        a.Student,

                        // The course completion workflow type id (if any).
                        CourseCompletionWorkflowTypeId = a.LearningClassActivity.LearningClass.LearningCourse.CompletionWorkflowTypeId,

                        // The learning course Guid (for passing to workflow)
                        LearningCourseGuid = a.LearningClassActivity.LearningClass.LearningCourse.Guid,

                        // Some activities (assessment) may not clearly indicate if the Facilitator must
                        // grade the activity before it can be considered complete.
                        // Therefore; we are always evaluating grades until the class is considered over.
                        ClassEndDate = a.LearningClassActivity.LearningClass.LearningSemester.EndDate,
                    } )
                    .ToList();

                var anyCompletionRecord = completionDetails.FirstOrDefault();

                // If there are no completion records then there's nothing to do.
                // This situation can/will happen if a student is being removed from a class.
                if ( anyCompletionRecord == null )
                {
                    return;
                }

                var participant = anyCompletionRecord.Student;
                var isClassOver = anyCompletionRecord.ClassEndDate.HasValue && anyCompletionRecord.ClassEndDate.Value.IsPast();
                var hasIncompleteAssignments = completionDetails.Any( a => !a.IsStudentOrFacilitatorCompleted );
                var hasUngradedAssignments = completionDetails.Any( a => a.RequiresGrading );

                // If the student completed all activities then mark the completion date (if unmarked).
                if ( !hasIncompleteAssignments && !participant.LearningCompletionDateTime.HasValue )
                {
                    participant.LearningCompletionDateTime = RockDateTime.Now;
                }

                // If the student has ungraded assignments don't send the completion workflow yet
                // (it may be dependent on the final grade.

                // If the class has ended don't recalculate the grade.
                // This ensures that if a facilitator adds an activity
                // after a student has completed all of what was assigned
                // they don't unexpectedly find their class re-opened.
                // The exception being when an activity was re-graded by a facilitator.
                var hasStudentCompletedClass = participant.LearningCompletionStatus != Enums.Lms.LearningCompletionStatus.Incomplete;
                if ( isClassOver || ( hasStudentCompletedClass && !WasRegraded ) )
                {
                    return;
                }

                var gradingSystemId = completionDetails.FirstOrDefault().GradingSystemId;

                var gradedActivities = completionDetails
                    .Where( a => a.IsStudentOrFacilitatorCompleted
                        && !a.RequiresGrading
                        && a.Earned.HasValue )
                    .ToList();
                var possiblePoints = gradedActivities.Sum( a => a.Possible );
                var earnedPoints = gradedActivities.Sum( a => a.Earned.Value );
                var gradePercent = possiblePoints > 0 ? earnedPoints / ( decimal ) possiblePoints * 100 : 0;

                var gradeScaleEarned = new LearningGradingSystemScaleService( RockContext ).GetEarnedScale( gradingSystemId, gradePercent );
                var currentGradePassFailStatus = gradeScaleEarned != null && gradeScaleEarned.IsPassing ? Enums.Lms.LearningCompletionStatus.Pass : Enums.Lms.LearningCompletionStatus.Fail;

                // Set the LearningParticipant current class grade values.
                participant.LearningGradePercent = gradePercent;
                participant.LearningGradingSystemScaleId = gradeScaleEarned.Id;
                participant.LearningCompletionStatus = hasIncompleteAssignments || hasUngradedAssignments ? Enums.Lms.LearningCompletionStatus.Incomplete : currentGradePassFailStatus;

                RockContext.SaveChanges();

                var wasCourseCompleted = participant.LearningCompletionStatus != Enums.Lms.LearningCompletionStatus.Incomplete;
                var currentPersonAliasId = DbContext.GetCurrentPersonAliasId();

                // If it was determined that the activity was completed and should launch a workflow
                // then launch that workflow before the Course completion workflow (if any).
                if ( ActivityCompletionWorkflowToLaunch != null )
                {
                    var workflowAttributes = new Dictionary<string, string>
                    {
                        {"Entity", Entity.ToJson()},
                        {"Student", participant.ToJson()}
                    };

                    participant.LaunchWorkflow( ActivityCompletionWorkflowToLaunch.Id, ActivityCompletionWorkflowToLaunch.Name, workflowAttributes, currentPersonAliasId );
                }

                // Launch the Course Completion workflow (if any).
                if ( wasCourseCompleted && anyCompletionRecord.CourseCompletionWorkflowTypeId.HasValue )
                {
                    var workflowAttributes = new Dictionary<string, string>
                    {
                        {"Student", participant.ToJson()},
                        {"Person", participant.Person.PrimaryAliasGuid.ToStringSafe()},
                        {"LearningCourse", anyCompletionRecord.LearningCourseGuid.ToStringSafe()}
                    };

                    var workflow = WorkflowTypeCache.Get( ( int ) anyCompletionRecord.CourseCompletionWorkflowTypeId );
                    participant.LaunchWorkflow( anyCompletionRecord.CourseCompletionWorkflowTypeId, workflow?.Name, workflowAttributes, currentPersonAliasId );
                }
            }

            /// <summary>
            /// Determines whether the workflow should be launched and sets
            /// the value of the ActivityCompletionWorkflowToLaunch accordingly.
            /// </summary>
            private void SetActivityCompletionWorkflowToLaunch()
            {
                var activityCompletionWorkflow = new LearningClassActivityService( RockContext )
                    .GetSelect( Entity.LearningClassActivityId, a => a.CompletionWorkflowType );

                // If the activity doesn't have a CompletionWorkflow there's no need for further evaluation.
                if ( activityCompletionWorkflow == null )
                {
                    return;
                }

                var originalIsStudentCompleted = ( this.Entry.OriginalValues?["IsStudentCompleted"] as bool? ).GetValueOrDefault();
                var originalIsFacilitatorCompleted = ( this.Entry.OriginalValues?["IsFacilitatorCompleted"] as bool? ).GetValueOrDefault();

                var wasStudentCompleted = !originalIsStudentCompleted && Entity.IsStudentCompleted;
                var wasFacilitatorCompleted = !originalIsFacilitatorCompleted && Entity.IsFacilitatorCompleted;

                // If the either the IsStudentCompleted or IsFacilitatorCompleted value is true
                // and the corresponding previous value was either null or false then
                // launch the activity completion workflow.
                // Note: Only one of these two properties should be set to true
                //  so this should never be launched twice.
                if ( wasStudentCompleted || wasFacilitatorCompleted )
                {
                    ActivityCompletionWorkflowToLaunch = activityCompletionWorkflow;
                }
            }

            /// <summary>
            /// Sets the value of the WasGraded property.
            /// </summary>
            private void SetWasRegraded()
            {
                if ( this.Entry.OriginalValues == null || !this.Entry.OriginalValues.Any() )
                {
                    return;
                }

                var originalGradedByPersonAliasId = this.Entry.OriginalValues["GradedByPersonAliasId"] as int?;
                var originalPointsEarned = this.Entry.OriginalValues["PointsEarned"].ToIntSafe();
                var pointsEarnedChanged = this.Entity.PointsEarned != originalPointsEarned;

                WasRegraded = originalGradedByPersonAliasId.HasValue && originalGradedByPersonAliasId > 0 && pointsEarnedChanged;
            }
        }
    }
}

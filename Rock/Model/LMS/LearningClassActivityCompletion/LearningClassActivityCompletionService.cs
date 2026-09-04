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

using Microsoft.Extensions.Logging;

using Rock.Communication;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Lava;
using Rock.Logging;

namespace Rock.Model
{
    public partial class LearningClassActivityCompletionService
    {
        /// <summary>
        /// Updates the <see cref="LearningClassActivityCompletion.SentNotificationCommunicationId"/> property
        /// for the provided <paramref name="activityCompletionIds"/>.
        /// </summary>
        /// <param name="activityCompletionIds">List of <see cref="LearningClassActivityCompletion"/> identifiers to update.</param>
        /// <param name="communicationId">The communication id to set for the given identifiers.</param>
        public void UpdateSentNotificationCommunicationIdProperty( List<int> activityCompletionIds, int communicationId )
        {
            var activityCompletions = Queryable().Where( c => activityCompletionIds.Contains( c.Id ) );

            Context.BulkUpdate( activityCompletions, a => new LearningClassActivityCompletion { SentNotificationCommunicationId = communicationId } );
        }

        /// <summary>
        /// Gets a new instance of a <see cref="LearningClassActivityCompletion"/> whose initialized values
        /// are based on the provided parameters.
        /// </summary>
        /// <remarks>
        ///     Available and Due Date calculations are performed.
        /// </remarks>
        /// <param name="activity">The <see cref="LearningClassActivity"/> the completion record is for.</param>
        /// <param name="participantId">The identifier of the <see cref="LearningParticipant"/> the completion record is for.</param>
        /// <param name="enrollmentDate">The date the participant enrolled in the <see cref="LearningClass"/>.</param>
        /// <param name="programCommunicationId">The SystemCommunicationId of the <see cref="LearningProgram"/> the completion record is for.</param>
        /// <returns>A new untracked <see cref="LearningClassActivityCompletion"/>.</returns>
        public static LearningClassActivityCompletion GetNew( LearningClassActivity activity, int participantId, DateTime? enrollmentDate, int? programCommunicationId )
        {
            var semesterStartDate = activity.LearningClass.LearningSemester.StartDate;

            return new LearningClassActivityCompletion
            {
                StudentId = participantId,
                LearningClassActivityId = activity.Id,
                AvailableDateTime = LearningClassActivity.CalculateAvailableDate(
                          activity.AvailabilityCriteria,
                          activity.AvailableDateDefault,
                          activity.AvailableDateOffset,
                          semesterStartDate,
                          enrollmentDate ),
                DueDate = LearningClassActivity.CalculateDueDate(
                    activity.DueDateCriteria,
                    activity.DueDateDefault,
                    activity.DueDateOffset,
                    semesterStartDate,
                    enrollmentDate )
            };
        }

        /// <summary>
        /// Gets a new <see cref="LearningClassActivityCompletion"/> using default values based on the provided parameters.
        /// </summary>
        /// <param name="activity">The <see cref="LearningClassActivity"/> this <see cref="LearningClassActivityCompletion"/> is for.</param>
        /// <param name="student">The <see cref="LearningParticipant"/> this <see cref="LearningClassActivityCompletion"/> is for.</param>
        /// <returns>A new <see cref="LearningClassActivityCompletion"/> record with default values.</returns>
        public static LearningClassActivityCompletion GetNew( LearningClassActivity activity, LearningParticipant student )
        {
            var enrollmentDate = student?.CreatedDateTime;
            var classStartDate = student.LearningClass?.LearningSemester?.StartDate;

            return new LearningClassActivityCompletion
            {
                LearningClassActivity = activity,
                LearningClassActivityId = activity.Id,
                StudentId = student.Id,
                Student = student,
                AvailableDateTime = LearningClassActivity.CalculateAvailableDate(
                    activity.AvailabilityCriteria,
                    activity.AvailableDateDefault,
                    activity.AvailableDateOffset,
                    classStartDate,
                    enrollmentDate ),
                DueDate = LearningClassActivity.CalculateDueDate(
                    activity.DueDateCriteria,
                    activity.DueDateDefault,
                    activity.DueDateOffset,
                    classStartDate,
                    enrollmentDate )
            };
        }

        /// <summary>
        /// Assigns a retake for the given <paramref name="completion"/>, returning the activity to a
        /// not-yet-completed state for the student. The completion and any file the student uploaded
        /// are deleted, and the participant's class completion is reset so it re-stamps when the
        /// student genuinely finishes the retake. The caller is responsible for saving changes, which
        /// drives the grade recomputation in the completion's save hook.
        /// </summary>
        /// <param name="completion">The <see cref="LearningClassActivityCompletion"/> to reset for a retake.</param>
        internal void AssignRetake( LearningClassActivityCompletion completion )
        {
            if ( completion == null )
            {
                return;
            }

            var rockContext = ( RockContext ) Context;

            // Delete the uploaded file now rather than leaving it for the binary-file cleanup job.
            if ( completion.BinaryFileId.HasValue )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( completion.BinaryFileId.Value );

                if ( binaryFile != null )
                {
                    binaryFileService.Delete( binaryFile );
                }
            }

            var participant = completion.Student
                ?? new LearningParticipantService( rockContext ).Get( completion.StudentId );

            if ( participant != null )
            {
                // Clear the completion date so it re-stamps at the genuine post-retake completion
                // instead of staying at the pre-retake timestamp, which is never otherwise updated.
                participant.LearningCompletionDateTime = null;

                // Reset to Incomplete so a pending retake can't read as a finished class, and so the
                // save hook's grade recompute runs; it skips recomputation once a class is complete.
                participant.LearningCompletionStatus = LearningCompletionStatus.Incomplete;
            }

            Delete( completion );
        }

        /// <summary>
        /// Builds the "Retake Required" notification for the given completion as a ready-to-send
        /// message, delivered via the student's communication preference (email or SMS).
        /// </summary>
        /// <param name="completion">The <see cref="LearningClassActivityCompletion"/> a retake was assigned for.</param>
        /// <returns>A prepared <see cref="RockMessage"/> to send, or <c>null</c> if one could not be built.</returns>
        internal RockMessage PrepareRetakeRequiredNotification( LearningClassActivityCompletion completion )
        {
            if ( completion == null )
            {
                return null;
            }

            var rockContext = ( RockContext ) Context;

            // Eager-load the graph this notification reads so it does not depend on lazy loading.
            completion = Queryable()
                .Include( c => c.LearningClassActivity.LearningActivity )
                .Include( c => c.LearningClassActivity.LearningClass.LearningCourse )
                .Include( c => c.LearningClassActivity.LearningClass.LearningSemester.LearningProgram )
                .Include( c => c.Student.Person.PhoneNumbers )
                .FirstOrDefault( c => c.Id == completion.Id )
                ?? completion;

            var activity = completion.LearningClassActivity;
            var learningClass = activity?.LearningClass;
            var person = completion.Student?.Person;

            if ( activity == null || person == null )
            {
                return null;
            }

            var systemCommunication = new SystemCommunicationService( rockContext )
                .Get( Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_RETAKE_REQUIRED.AsGuid() );

            if ( systemCommunication == null )
            {
                return null;
            }

            var course = learningClass?.LearningCourse;
            var program = learningClass?.LearningSemester?.LearningProgram;

            var mergeFields = LavaHelper.GetCommonMergeFields( null, person );
            mergeFields.AddOrReplace( "Person", person );
            mergeFields.AddOrReplace( "Activity", new RetakeActivityInfo
            {
                ActivityName = activity.Name,
                LearningClassActivityIdKey = activity.IdKey,
                DueDate = completion.DueDate
            } );
            mergeFields.AddOrReplace( "Class", new RetakeClassInfo { ClassIdKey = learningClass?.IdKey } );
            mergeFields.AddOrReplace( "Course", new RetakeCourseInfo { CourseIdKey = course?.IdKey } );
            mergeFields.AddOrReplace( "Program", new RetakeProgramInfo { ProgramIdKey = program?.IdKey } );

            var mediumType = Communication.DetermineMediumEntityTypeId(
                ( int ) CommunicationType.Email,
                ( int ) CommunicationType.SMS,
                ( int ) CommunicationType.PushNotification,
                systemCommunication,
                person,
                completion.Student.CommunicationPreference,
                person.CommunicationPreference );

            var logger = RockLogger.LoggerFactory.CreateLogger<CommunicationHelper>();
            var createResult = mediumType == ( int ) CommunicationType.SMS
                ? CommunicationHelper.CreateSmsMessage( person, mergeFields, systemCommunication, logger )
                : CommunicationHelper.CreateEmailMessage( person, mergeFields, systemCommunication, logger );

            return createResult.Message;
        }

        #region Lava Merge Objects

        /// <summary>
        /// The activity merge object for the "Retake Required" system communication.
        /// </summary>
        private class RetakeActivityInfo : LavaDataObject
        {
            public string ActivityName { get; set; }

            public string LearningClassActivityIdKey { get; set; }

            public DateTime? DueDate { get; set; }
        }

        /// <summary>
        /// The class merge object for the "Retake Required" system communication.
        /// </summary>
        private class RetakeClassInfo : LavaDataObject
        {
            public string ClassIdKey { get; set; }
        }

        /// <summary>
        /// The course merge object for the "Retake Required" system communication.
        /// </summary>
        private class RetakeCourseInfo : LavaDataObject
        {
            public string CourseIdKey { get; set; }
        }

        /// <summary>
        /// The program merge object for the "Retake Required" system communication.
        /// </summary>
        private class RetakeProgramInfo : LavaDataObject
        {
            public string ProgramIdKey { get; set; }
        }

        #endregion
    }
}
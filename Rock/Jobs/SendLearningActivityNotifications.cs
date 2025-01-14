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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Utility;

namespace Rock.Jobs
{
    /// <summary>
    /// Send Learning Activity Available Notifications
    /// </summary>
    [DisplayName( "Send Learning Activity Notifications" )]
    [Description( "This job will send s single email for each student with newly available activities. The email is based on the configured System Communication template and that template should contain the following merge objects: Courses and ActivityCount." )]

    #region Job Attributes

    [SystemCommunicationField(
        "System Communication",
        Key = AttributeKey.SystemCommunication,
        Description = "The system communication that contains the email template to use for the email.",
        DefaultSystemCommunicationGuid = SystemGuid.SystemCommunication.LEARNING_ACTIVITY_NOTIFICATIONS,
        IsRequired = true,
        Order = 1 )]

    #endregion

    public class SendLearningActivityNotifications : RockJob
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string SystemCommunication = "SystemCommunication";
        }

        #endregion

        private SystemCommunicationService _systemCommunicationService;
        private LearningActivityService _learningActivityService;
        private LearningActivityCompletionService _learningActivityCompletionService;
        private LearningParticipantService _learningParticipantService;

        private IList<string> _warnings;
        private IList<string> _errors;
        private int _notificationsSent;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require s public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendLearningActivityNotifications()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            try
            {
                var jobStartTime = RockDateTime.Now;
                InitializeResultsCounters();

                var systemCommunicationGuid = this.GetAttributeValue( AttributeKey.SystemCommunication ).ToString().AsGuidOrNull();
                if ( systemCommunicationGuid == null )
                {
                    _errors.Add( "The selected system communication is not valid." );
                    return;
                }

                SystemCommunication systemCommunication;

                using ( var rockContext = new RockContext() )
                {
                    InitializeServices( rockContext );

                    // Make sure the selected system communication exists.
                    systemCommunication = _systemCommunicationService.GetNoTracking( systemCommunicationGuid.Value );
                    if ( systemCommunication == null )
                    {
                        _errors.Add( "Unable to retrieve the selected system communication." );
                        return;
                    }

                    var courseDataByPerson = GetAggregateData();

                    if ( !courseDataByPerson.Any() )
                    {
                        Result = "No notifications to send";
                        return;
                    }
                    else
                    {
                        var successulPersonNotifications = SendNotifications( systemCommunication, courseDataByPerson );

                        foreach ( var personActivitiesNotified in successulPersonNotifications )
                        {
                            AddOrUpdateCompletionRecords( personActivitiesNotified, systemCommunication.Id );
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                throw;
            }

            SetJobResultSummary();
        }

        /// <summary>
        /// Adds any <see cref="LearningActivity"/> records for which notifications were sent.
        /// If there are any existing <see cref="LearningActivityCompletion"/> records their
        /// NotificationCommunicationId property will be set to true.
        /// </summary>
        /// <param name="personActivitiesByCourse">The list of activities grouped by course for this person.</param>
        /// <param name="systemCommunicationId">The identifier of the <see cref="SystemCommunication"/> that was used to send the notification.</param>
        private void AddOrUpdateCompletionRecords( PersonActivitiesByCourse personActivitiesByCourse, int systemCommunicationId )
        {
            // Get a list of all activities for simpler querying.
            var activities = personActivitiesByCourse.Courses.SelectMany( c => c.Activities );

            // The participant will differ across classes.
            var participantIds = personActivitiesByCourse.Courses.Select( a => a.LearningParticipantId ).Distinct().ToList();
            var participants = _learningParticipantService.GetByIds( participantIds );

            // Get the list of all ActivityIds that were referenced in the person's email.
            var activityIdsToAdd = activities
                .Where( a => a.LearningActivityCompletionId.ToIntSafe() == 0 )
                .Select( a => a.LearningActivityId )
                .ToList();

            // Now get the distinct LearningActivityCompletion records to update the bit flag for.
            var completionIds = activities
                .Select( a => a.LearningActivityCompletionId.ToIntSafe() )
                .Where( a => a > 0 )
                .Distinct()
                .ToList();
            _learningActivityCompletionService.UpdateNotificationCommunicationProperties( completionIds, systemCommunicationId );

            // Get the activity data and transform it into the completion records for the student.
            var activityCompletionsToAdd = _learningActivityService
               .GetByIds( activityIdsToAdd )
               .ToList()
               .OrderBy( a => a.Order )
               .ThenBy( a => a.Id )
               .Select( a => LearningActivityCompletionService.GetNew(
                   a,
                   participants.FirstOrDefault( p => p.LearningClassId == a.LearningClassId ) ) );

            // Add the new LearningActivityCompletion records to the context.
            _learningActivityCompletionService.AddRange( activityCompletionsToAdd );
        }

        /// <summary>
        /// Initializes the results counters.
        /// </summary>
        private void InitializeResultsCounters()
        {
            _warnings = new List<string>();
            _errors = new List<string>();
            _notificationsSent = 0;
        }

        /// <summary>
        /// Initializes the services.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void InitializeServices( RockContext rockContext )
        {
            _systemCommunicationService = new SystemCommunicationService( rockContext );
            _learningActivityService = new LearningActivityService( rockContext );
            _learningParticipantService = new LearningParticipantService( rockContext );
            _learningActivityCompletionService = new LearningActivityCompletionService( rockContext );
        }

        private List<PersonActivitiesByCourse> GetAggregateData()
        {
            var now = RockDateTime.Now;

            // Get all the activities - because some may require student specific
            // availability criteria calculations we can't determine exclusions
            // until we also have the student data.
            var activities = _learningActivityService.Queryable()
                .AsNoTracking()
                .Include( a => a.LearningClass )
                .Include( a => a.LearningClass.LearningSemester )
                // Either always available semesters (no end date) or end date is in the future.
                .Where( a =>
                        !a.LearningClass.LearningSemester.EndDate.HasValue
                        || a.LearningClass.LearningSemester.EndDate >= now )
                // Either always available semesters (no start date) or start date is in the past.
                .Where( a =>
                        !a.LearningClass.LearningSemester.StartDate.HasValue
                        || a.LearningClass.LearningSemester.StartDate <= now )
                // Is configured to send a communication.
                .Where( a => a.SendNotificationCommunication )
                // Don't send notifications for Always available.
                .Where( a => a.AvailabilityCriteria != AvailabilityCriteria.AlwaysAvailable )
                // Is assigned to a student
                .Where( a => a.AssignTo == AssignTo.Student )
                .Select( a => new
                {
                    a.LearningClassId,
                    ActivityId = a.Id,
                    a.DueDateCriteria,
                    a.DueDateDefault,
                    a.DueDateOffset,
                    a.AvailabilityCriteria,
                    a.AvailableDateDefault,
                    a.AvailableDateOffset,
                    a.Order,
                    ActivityName = a.Name
                } )
                .ToList();

            // Get the distinct LearningClassIds - we'll use these to filter our student list.
            var classIds = activities.Select( a => a.LearningClassId ).ToList().Distinct();

            // Get the students and join them to the class activities.
            // Filter out any activities that aren't available
            // or have already been notified.
            var studentActivities = _learningParticipantService.Queryable()
                .AsNoTracking()
                .Include( s => s.Person )
                .Include( s => s.LearningClass )
                .Include( s => s.LearningClass.LearningCourse )
                .Include( s => s.LearningClass.LearningCourse.LearningProgram )
                .Include( s => s.LearningActivities )
                .AreStudents()
                .Where( s => classIds.Contains( s.LearningClassId ) )
                .Select( s => new
                {
                    s.Person.Email,
                    PersonNickName = s.Person.NickName,
                    PersonLastName = s.Person.LastName,
                    PersonSuffixValueId = s.Person.SuffixValueId,
                    s.PersonId,
                    LearningParticipantId = s.Id,
                    ProgramName = s.LearningClass.LearningCourse.LearningProgram.Name,
                    SemesterStartDate = s.LearningClass.LearningSemester.StartDate,
                    EnrollmentDate = s.CreatedDateTime,
                    s.LearningClass.LearningCourse.CourseCode,
                    s.LearningClass.LearningCourseId,
                    CourseName = s.LearningClass.LearningCourse.Name,
                    s.LearningClassId,
                    CompletionLookups = s.LearningActivities
                        .Select( lac => new { LearningActivityCompletionId = lac.Id, lac.LearningActivityId } )
                        .ToList(),
                    IsCompletedOrAlreadyNotifiedActivityIds = s.LearningActivities
                        .Where( lac =>
                            lac.IsStudentCompleted
                            || lac.NotificationCommunicationId.HasValue )
                        .Select( lac => lac.LearningActivityId )
                        .ToList()
                } )
                .ToList()
                .Join(
                    activities,
                    s => s.LearningClassId,
                    a => a.LearningClassId,
                    ( s, a ) => new { Student = s, ClassActivity = a }
                )
                // Get activities grouped by Participant and Course data.
                .GroupBy( s => new
                {
                    // Group By Person properties
                    s.Student.Email,
                    s.Student.PersonNickName,
                    s.Student.PersonLastName,
                    s.Student.PersonSuffixValueId,
                    s.Student.PersonId
                }, s => new
                {
                    // with values for course and activities data.
                    s.Student.LearningParticipantId,
                    s.Student.ProgramName,
                    s.Student.SemesterStartDate,
                    s.Student.EnrollmentDate,
                    s.Student.CourseCode,
                    s.Student.LearningCourseId,
                    s.Student.CourseName,
                    s.ClassActivity.ActivityId,
                    // If the student has an activity completion record then get it's Id for later updates.
                    s.Student.CompletionLookups.FirstOrDefault( l => l.LearningActivityId == s.ClassActivity.ActivityId )?.LearningActivityCompletionId,
                    IsCompletedOrAlreadyNotified = s.Student.IsCompletedOrAlreadyNotifiedActivityIds.Any( id => id == s.ClassActivity.ActivityId ),
                    DueDate = LearningActivity.CalculateDueDate(
                        s.ClassActivity.DueDateCriteria,
                        s.ClassActivity.DueDateDefault,
                        s.ClassActivity.DueDateOffset,
                        s.Student.SemesterStartDate,
                        s.Student.EnrollmentDate
                    ),
                    AvailableDateTime = LearningActivity.CalculateAvailableDate(
                        s.ClassActivity.AvailabilityCriteria,
                        s.ClassActivity.AvailableDateDefault,
                        s.ClassActivity.AvailableDateOffset,
                        s.Student.SemesterStartDate,
                        s.Student.EnrollmentDate
                    ),
                    s.ClassActivity.Order,
                    s.ClassActivity.ActivityName
                } )
                .ToList()
                .Where( s => s.Key.Email.IsNotNullOrWhiteSpace() )
                // Convert the grouped data to the POCO for simpler comprehension.
                .Select( row => new PersonActivitiesByCourse
                {
                    Email = row.Key.Email,
                    PersonId = row.Key.PersonId,
                    // Include the properties necessary to get the FullName (NickName, LastName and SuffixValueId).
                    PersonNickName = row.Key.PersonNickName,
                    PersonLastName = row.Key.PersonLastName,
                    PersonSuffixValueId = row.Key.PersonSuffixValueId,
                    Courses = row.GroupBy( groupingKey => new
                    {
                        groupingKey.CourseCode,
                        groupingKey.LearningCourseId,
                        groupingKey.CourseName,
                        groupingKey.ProgramName
                    }, groupingValue => new
                    {
                        groupingValue.ActivityId,
                        groupingValue.LearningParticipantId,
                        groupingValue.AvailableDateTime,
                        groupingValue.ActivityName,
                        groupingValue.DueDate,
                        groupingValue.LearningActivityCompletionId,
                        groupingValue.IsCompletedOrAlreadyNotified,
                        groupingValue.Order
                    } )
                    .Select( groupedResult => new ActivitiesByCourse
                    {
                        CourseCode = groupedResult.Key.CourseCode,
                        CourseId = groupedResult.Key.LearningCourseId,
                        CourseName = groupedResult.Key.CourseName,
                        // While technically possible to have multiple LearningParticipantIds per course;
                        // this is unlikely since it means the same person would be enrolled in 2 classes
                        // for the same course. Still we should handle for it by taking the MAX LearningParticipantId.
                        LearningParticipantId = groupedResult.Max( r => r.LearningParticipantId ),
                        ProgramName = groupedResult.Key.ProgramName,
                        // Apply the date and/or completion logic here
                        // to remove any activities that the student has completed.
                        ActivityCount = groupedResult.Count( a =>
                            !a.IsCompletedOrAlreadyNotified
                            && ( !a.AvailableDateTime.HasValue || a.AvailableDateTime <= now ) ),
                        Activities = groupedResult.Where( a =>
                            !a.IsCompletedOrAlreadyNotified
                            && ( !a.AvailableDateTime.HasValue || a.AvailableDateTime <= now ) )
                        .Select( a => new Activity
                        {
                            ActivityName = a.ActivityName,
                            AvailableDate = a.AvailableDateTime,
                            DueDate = a.DueDate,
                            LearningActivityCompletionId = a.LearningActivityCompletionId,
                            LearningActivityId = a.ActivityId,
                            Order = a.Order
                        } ).ToList()
                    } ).ToList()
                } )
                .ToList();

            // Because we applied some filters to the activities
            // we need to look back and remove any course groupings
            // that don't contain any activities.
            // Reason: Prevent empty emails/email sections from being sent.
            return studentActivities.Where( s => s.Courses.Any( c => c.ActivityCount > 0 ) ).ToList();
        }

        /// <summary>
        /// Sends the digest emails for each person and their course activities.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="personsActivitiesByCourse">The list of persons and their activities to notify.</param>
        /// <returns>The list of PersonActivitiesByCourse that were successfully notified.</returns>
        private List<PersonActivitiesByCourse> SendNotifications( SystemCommunication systemCommunication, List<PersonActivitiesByCourse> personsActivitiesByCourse )
        {
            var successfullySentNotifications = new List<PersonActivitiesByCourse>();
            foreach ( var personActivitiesByCourse in personsActivitiesByCourse )
            {
                if ( SendNotificationForPerson( systemCommunication, personActivitiesByCourse ) )
                {
                    successfullySentNotifications.Add( personActivitiesByCourse );
                }
            }

            return successfullySentNotifications;
        }

        /// <summary>
        /// Sends the digest email for the specified person.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="personActivitiesByCourse">The person and their activities to notify.</param>
        /// <returns><c>true</c> if the email was successfully sent; otherwise <c>false</c>.</returns>
        private bool SendNotificationForPerson( SystemCommunication systemCommunication, PersonActivitiesByCourse personActivitiesByCourse )
        {
            var person = new Person
            {
                Id = personActivitiesByCourse.PersonId,
                NickName = personActivitiesByCourse.PersonNickName,
                LastName = personActivitiesByCourse.PersonLastName,
                SuffixValueId = personActivitiesByCourse.PersonSuffixValueId,
                Email = personActivitiesByCourse.Email
            };

            try
            {
                // Add the merge objects to support this notification.
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, person );
                mergeObjects.AddOrReplace( "ActivityCount", personActivitiesByCourse.Courses.Sum( c => c.ActivityCount ) );
                mergeObjects.AddOrReplace( "Courses", personActivitiesByCourse.Courses );

                var recipient = new RockEmailMessageRecipient( person, mergeObjects );
                var message = new RockEmailMessage( systemCommunication );
                message.Subject = systemCommunication.Subject.ResolveMergeFields( mergeObjects );
                message.SetRecipients( new List<RockEmailMessageRecipient> { recipient } );
                message.Send( out List<string> errorMessages );

                if ( !errorMessages.Any() )
                {
                    _notificationsSent++;
                    return true;
                }

                _errors.Add( $"Unable to send Learning Activity Available Notifications to {person.FullName}. '{errorMessages.JoinStrings( " " )}'" );
            }
            catch( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                _errors.Add( $"Unable to send Learning Activity Available Notifications to {person.FullName}. '{ex.Message}'" );
            }

            return false;
        }

        /// <summary>
        /// Sets the Result property based on the notificationsSent, warnings and errors.
        /// </summary>
        private void SetJobResultSummary()
        {
            StringBuilder jobSummaryBuilder = new StringBuilder();
            jobSummaryBuilder.AppendLine( "Summary:" );
            jobSummaryBuilder.AppendLine();

            if ( _notificationsSent > 0 )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {_notificationsSent} {"notification".PluralizeIf( _notificationsSent != 1 )} sent" );
            }

            foreach ( var warning in _warnings )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-warning'></i> {warning}" );
            }

            foreach ( var error in _errors )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-danger'></i> {error}" );
            }

            this.Result = jobSummaryBuilder.ToString();
        }

        /// <summary>
        /// A class to represent the individual activities.
        /// </summary>
        private class Activity : RockDynamic
        {
            /// <summary>
            /// The Id of the <see cref="LearningActivity"/> the notification is for.
            /// </summary>
            public int LearningActivityId { get; set; }

            /// <summary>
            /// The Id of the <see cref="LearningActivityCompletion"/> the notification is for (if any yet).
            /// </summary>
            /// <remarks>
            /// This is used by the job to mark notifications that have been sent.
            /// </remarks>
            public int? LearningActivityCompletionId { get; set; }

            /// <summary>
            /// The name of the assigned activity.
            /// </summary>
            public string ActivityName { get; set; }

            /// <summary>
            /// The date the activity became available.
            /// </summary>
            public DateTime? AvailableDate { get; set; }

            /// <summary>
            /// The date the activity is due.
            /// </summary>
            public DateTime? DueDate { get; set; }

            /// <summary>
            /// The order of the activity.
            /// </summary>
            public int Order { get; set; }
        }

        /// <summary>
        /// A class to represent s Person and all the courses with available activities requiring notification.
        /// </summary>
        private class PersonActivitiesByCourse : RockDynamic
        {
            /// <summary>
            /// The identifier of the <see cref="Person"/>.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// The nickname of the <see cref="Person"/>.
            /// </summary>
            public string PersonNickName { get; set; }

            /// <summary>
            /// The last name of the <see cref="Person"/>.
            /// </summary>
            public string PersonLastName { get; set; }

            /// <summary>
            /// The suffixValuId for the <see cref="Person"/>.
            /// </summary>
            public int? PersonSuffixValueId { get; set; }

            /// <summary>
            /// The email address of the <see cref="Person" />.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// A list of courses with available activities for this <see cref="Person"/>.
            /// </summary>
            public List<ActivitiesByCourse> Courses { get; set; }
        }

        private class ActivitiesByCourse : RockDynamic
        {
            /// <summary>
            /// The activities in the course.
            /// </summary>
            public List<Activity> Activities { get; set; }

            /// <summary>
            /// The identifier of the Person's <see cref="LearningParticipant"/> record specific to the <see cref="LearningClass"/>.
            /// </summary>
            public int LearningParticipantId { get; set; }

            /// <summary>
            /// The learning course code of the activity.
            /// </summary>
            public string CourseCode { get; set; }

            /// <summary>
            /// The identifier of the activity's learning course.
            /// </summary>
            public int CourseId { get; set; }

            /// <summary>
            /// The learning course name of the activity.
            /// </summary>
            public string CourseName { get; set; }

            /// <summary>
            /// The learning program name of the activity.
            /// </summary>
            public string ProgramName { get; set; }

            /// <summary>
            /// The total number of activities newly available for this course and person.
            /// </summary>
            public int ActivityCount { get; set; }
        }
    }
}

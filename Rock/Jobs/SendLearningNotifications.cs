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
using Rock.Cms.StructuredContent;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Lava;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Send Learning Activity Notifications
    /// </summary>
    [DisplayName( "Send Learning Notifications" )]
    [Description( @"This job will send any unsent class announcements as well as an available activity digest emails for all their newly available activities within a learning program.
        The class announcements SystemCommunication is configured by the job setting and contains the Person and Announcement merge fields.
        The Available Activity Notification is configured by the learning program and contains ActivityCount and Courses (a list of CourseInfo) merge fields." )]

    #region Job Attributes

    [SystemCommunicationField(
        "System Communication",
        Key = AttributeKey.SystemCommunication,
        Description = "The system communication that contains the email template to use for the email.",
        DefaultSystemCommunicationGuid = SystemGuid.SystemCommunication.LEARNING_ANNOUNCEMENT_NOTIFICATIONS,
        IsRequired = true,
        Order = 1 )]

    #endregion

    public class SendLearningNotifications : RockJob
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
        private LearningClassAnnouncementService _learningClassAnnouncementService;
        private LearningParticipantService _learningParticipantService;

        private IList<string> _warnings;
        private IList<string> _errors;
        private int _distinctClassAnnouncementsSent;
        private int _distinctAnnouncementMessagesSent;
        private int _notificationsSent;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require s public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendLearningNotifications()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            try
            {
                var jobStartTime = RockDateTime.Now;
                InitializeResultsCounters();

                using ( var rockContext = new RockContext() )
                {
                    InitializeServices( rockContext );

                    SendActivityNotifications( rockContext );

                    SendAnnouncements();

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
        /// Sends the pending learning class announcements.
        /// </summary>
        private void SendAnnouncements()
        {
            var announcementsSystemCommunicationGuid = this.GetAttributeValue( AttributeKey.SystemCommunication ).AsGuidOrNull();
            if ( announcementsSystemCommunicationGuid == null )
            {
                _errors.Add( "The system communication selected for announcements is not valid." );
                return;
            }

            // Make sure the selected system communication exists.
            SystemCommunication announcementsSystemCommunication = _systemCommunicationService.GetNoTracking( announcementsSystemCommunicationGuid.Value );

            if ( announcementsSystemCommunication == null )
            {
                _errors.Add( "Unable to retrieve the selected system communication." );
                return;
            }

            // Get the pending announcements and the potential recipients for those announcements.
            // Get as no tracking because we may be converting the Announcement.Description from a JSON object
            // to an HTML value (for emails). We don't want that change to be saved to the database.
            var pendingAnnouncements = _learningClassAnnouncementService.GetUnsentAnnouncements().AsNoTracking();
            var uniqueClassIds = pendingAnnouncements
                .Select( a => a.LearningClassId )
                .Distinct()
                .ToList();
            var potentialRecipients = _learningParticipantService
                .GetStudentsForClasses( uniqueClassIds )
                .Include( a => a.Person )
                .ToList();

            foreach ( var announcement in pendingAnnouncements )
            {
                int mediumType;
                switch ( announcement.CommunicationMode )
                {
                    case CommunicationMode.Email:
                        mediumType = ( int ) CommunicationType.Email;

                        // The StrucutredContent needs to be converted to HTML before sending.
                        announcement.Description = new StructuredContentHelper( announcement.Description ).Render();
                        break;
                    case CommunicationMode.SMS:
                        mediumType = ( int ) CommunicationType.SMS;
                        break;
                    default:
                        // Go to the next announcement when no communication mode (or an unknown type).
                        continue;
                }

                var recipients = potentialRecipients.Where( a => a.LearningClassId == announcement.LearningClassId );

                // If this is an email announcement then further filter the list
                // of recipients to only those with a non-empty email address.
                if ( announcement.CommunicationMode == CommunicationMode.Email )
                {
                    recipients = recipients.Where( a => a.Person.Email.IsNotNullOrWhiteSpace() );
                }

                var wasSuccessfullySent = false;

                foreach ( var recipient in recipients )
                {
                    var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, recipient.Person );
                    mergeObjects.Add( "Person", recipient.Person );
                    mergeObjects.Add( "Announcement", announcement );

                    var sendResult = CommunicationHelper.SendMessage( recipient.Person, mediumType, announcementsSystemCommunication, mergeObjects );

                    foreach ( var errorMessage in sendResult.Errors )
                    {
                        _errors.Add( errorMessage );
                    }

                    foreach ( var warningMessage in sendResult.Warnings )
                    {
                        _warnings.Add( warningMessage );
                    }

                    _distinctAnnouncementMessagesSent += sendResult.MessagesSent;

                    // If it at least 1 announcement was sent then treat this as success.
                    if ( sendResult.MessagesSent > 0 )
                    {
                        wasSuccessfullySent = true;
                    }
                }

                // If the message was sent to any recipient save the changes to
                // the LearningClassAnnouncement.CommunicationSent property.
                if ( wasSuccessfullySent )
                {
                    _learningClassAnnouncementService.UpdateCommunicationSentProperty( new List<int> { announcement.Id } );
                }
            }

            // Get a count of distinct announcements sent.
            _distinctClassAnnouncementsSent = pendingAnnouncements.Select( a => a.Id ).Distinct().Count();            
        }

        /// <summary>
        /// Sends the pending learning activity notifications and updates the
        /// LearningActivityCompletion records to indicate the SystemCommunicationId
        /// that was used to notify the individual.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> to use for data access.</param>
        private void SendActivityNotifications( RockContext rockContext )
        {
            var courseDataByPerson = GetPersonActivitiesByCourse();

            if ( !courseDataByPerson.Any() )
            {
                Result = "No notifications to send";
            }
            else
            {
                // Get the distinct SystemCommunicationIds before querying the database.
                var distinctSystemCommunicationIds = courseDataByPerson
                    .Select( pc => pc.ProgramSystemCommunicationId )
                    .Distinct()
                    .ToList();

                // Get the SystemCommunications we'll be using and create a Dictionary for lookup.
                var activityAvailableCommunications = _systemCommunicationService
                    .GetByIds( distinctSystemCommunicationIds )
                    .ToDictionary( s => s.Id, s => s );

                var successulPersonNotifications = SendNotifications( activityAvailableCommunications, courseDataByPerson );

                foreach ( var personActivitiesNotified in successulPersonNotifications )
                {
                    AddOrUpdateCompletionRecords( personActivitiesNotified, activityAvailableCommunications );
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Adds any <see cref="LearningActivity"/> records for which notifications were sent.
        /// If there are any existing <see cref="LearningActivityCompletion"/> records their
        /// NotificationCommunicationId property will be set to <c>true</c>.
        /// </summary>
        /// <param name="personActivitiesByCourse">The list of activities grouped by course for this person.</param>
        /// <param name="systemCommunications">The Dictionary of the <see cref="SystemCommunication"/> for all programs with notifications to send.</param>
        private void AddOrUpdateCompletionRecords( PersonProgramActivitiesByCourseInfo personActivitiesByCourse, Dictionary<int, SystemCommunication> systemCommunications )
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

            foreach ( var systemCommunicationId in systemCommunications.Keys )
            {
                // Now get the distinct LearningActivityCompletion records to update the SystemCommunicationId.
                var completionIds = activities
                    .Where( a => a.LearningActivityCompletionId.HasValue && a.LearningActivityCompletionId > 0 && a.SystemCommunicationId == systemCommunicationId )
                    .Select( a => a.LearningActivityCompletionId.ToIntSafe() )
                    .Distinct()
                    .ToList();

                _learningActivityCompletionService.UpdateNotificationCommunicationIdProperty( completionIds, systemCommunicationId );
            }

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
            _distinctAnnouncementMessagesSent = 0;
            _distinctClassAnnouncementsSent = 0;
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
            _learningClassAnnouncementService = new LearningClassAnnouncementService( rockContext );
            _learningParticipantService = new LearningParticipantService( rockContext );
            _learningActivityCompletionService = new LearningActivityCompletionService( rockContext );
        }

        /// <summary>
        /// Get a list of courses with their respective activities grouped by person and program.
        /// </summary>
        /// <returns>A <c>List&lt;PersonProgramActivitiesByCourseInfo&gt;</c> containing the courses and activities with pending notifications grouped by person and program.</returns>
        private List<PersonProgramActivitiesByCourseInfo> GetPersonActivitiesByCourse()
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
                    ProgramSystemCommunicationId = s.LearningClass.LearningCourse.LearningProgram.SystemCommunicationId,
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
                    s.Student.PersonId,
                    s.Student.ProgramName,
                    s.Student.ProgramSystemCommunicationId,
                }, s => new
                {
                    // with values for course and activities data.
                    s.Student.LearningParticipantId,
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
                .Select( row => new PersonProgramActivitiesByCourseInfo
                {
                    Email = row.Key.Email,
                    PersonId = row.Key.PersonId,
                    // Include the properties necessary to get the FullName (NickName, LastName and SuffixValueId).
                    PersonNickName = row.Key.PersonNickName,
                    PersonLastName = row.Key.PersonLastName,
                    PersonSuffixValueId = row.Key.PersonSuffixValueId,
                    ProgramName = row.Key.ProgramName,
                    ProgramSystemCommunicationId = row.Key.ProgramSystemCommunicationId,
                    Courses = row.GroupBy( groupingKey => new
                    {
                        groupingKey.CourseCode,
                        groupingKey.LearningCourseId,
                        groupingKey.CourseName
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
                    .Select( groupedResult => new ActivitiesByCourseInfo
                    {
                        CourseCode = groupedResult.Key.CourseCode,
                        CourseId = groupedResult.Key.LearningCourseId,
                        CourseName = groupedResult.Key.CourseName,
                        // While technically possible to have multiple LearningParticipantIds per course;
                        // this is unlikely since it means the same person would be enrolled in 2 classes
                        // for the same course. Still we should handle for it by taking the MAX LearningParticipantId.
                        LearningParticipantId = groupedResult.Max( r => r.LearningParticipantId ),
                        // Apply the date and/or completion logic here
                        // to remove any activities that the student has completed.
                        ActivityCount = groupedResult.Count( a =>
                            !a.IsCompletedOrAlreadyNotified
                            && ( !a.AvailableDateTime.HasValue || a.AvailableDateTime <= now ) ),
                        Activities = groupedResult.Where( a =>
                            !a.IsCompletedOrAlreadyNotified
                            && ( !a.AvailableDateTime.HasValue || a.AvailableDateTime <= now ) )
                        .Select( a => new ActivityInfo
                        {
                            ActivityName = a.ActivityName,
                            AvailableDate = a.AvailableDateTime,
                            DueDate = a.DueDate,
                            LearningActivityCompletionId = a.LearningActivityCompletionId,
                            LearningActivityId = a.ActivityId,
                            Order = a.Order,
                            SystemCommunicationId = row.Key.ProgramSystemCommunicationId
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
        /// <param name="systemCommunications">The system communication.</param>
        /// <param name="personsActivitiesByCourse">The list of persons and their activities to notify.</param>
        /// <returns>The list of PersonProgramActivitiesByCourseInfo that were successfully notified.</returns>
        private List<PersonProgramActivitiesByCourseInfo> SendNotifications( Dictionary<int, SystemCommunication> systemCommunications, List<PersonProgramActivitiesByCourseInfo> personsActivitiesByCourse )
        {
            var successfullySentNotifications = new List<PersonProgramActivitiesByCourseInfo>();
            foreach ( var personActivitiesByCourse in personsActivitiesByCourse )
            {
                if ( SendNotificationForPerson( systemCommunications, personActivitiesByCourse ) )
                {
                    successfullySentNotifications.Add( personActivitiesByCourse );
                }
            }

            return successfullySentNotifications;
        }

        /// <summary>
        /// Sends the digest email for the specified person.
        /// </summary>
        /// <param name="systemCommunications">The Dictionary of all system communications that are being sent for.</param>
        /// <param name="personProgramActivitiesByCourse">The person and their activities to notify.</param>
        /// <returns><c>true</c> if the email was successfully sent; otherwise <c>false</c>.</returns>
        private bool SendNotificationForPerson( Dictionary<int, SystemCommunication> systemCommunications, PersonProgramActivitiesByCourseInfo personProgramActivitiesByCourse )
        {
            var person = new Person
            {
                Id = personProgramActivitiesByCourse.PersonId,
                NickName = personProgramActivitiesByCourse.PersonNickName,
                LastName = personProgramActivitiesByCourse.PersonLastName,
                SuffixValueId = personProgramActivitiesByCourse.PersonSuffixValueId,
                Email = personProgramActivitiesByCourse.Email
            };

            if ( !systemCommunications.TryGetValue( personProgramActivitiesByCourse.ProgramSystemCommunicationId, out var systemCommunication ) )
            {
                _errors.Add( $"Unable to get the LearningProgram's SystemCommunication for Activity Notifications. SystemCommunicationId: {personProgramActivitiesByCourse.ProgramSystemCommunicationId}" );
            }

            try
            {
                // Add the merge objects to support this notification.
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, person );
                mergeObjects.AddOrReplace( "ActivityCount", personProgramActivitiesByCourse.Courses.Sum( c => c.ActivityCount ) );
                mergeObjects.AddOrReplace( "Courses", personProgramActivitiesByCourse.Courses );

                var sendResult = CommunicationHelper.SendMessage( person, ( int ) CommunicationType.Email, systemCommunication, mergeObjects );

                foreach ( var errorMessage in sendResult.Errors )
                {
                    _errors.Add( errorMessage );
                }

                foreach ( var warningMessage in sendResult.Warnings )
                {
                    _warnings.Add( warningMessage );
                }

                _notificationsSent += sendResult.MessagesSent;
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

            if ( _distinctAnnouncementMessagesSent > 0 )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {_distinctClassAnnouncementsSent} {"announcement".PluralizeIf( _distinctClassAnnouncementsSent != 1 )} sent to {_distinctAnnouncementMessagesSent} {"individual".PluralizeIf( _distinctAnnouncementMessagesSent != 1 )}" );
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
        /// A POCO for a Person and all the courses with available activities requiring notification.
        /// </summary>
        private class PersonProgramActivitiesByCourseInfo : LavaDataDictionary
        {
            /// <summary>
            /// The email address of the <see cref="Person" />.
            /// </summary>
            public string Email { get; set; }

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
            /// The learning program name of the activity.
            /// </summary>
            public string ProgramName { get; set; }

            /// <summary>
            /// The SystemCommunicationId for Activity Available Notifications for the <see cref="LearningProgram"/>.
            /// </summary>
            public int ProgramSystemCommunicationId { get; set; }

            /// <summary>
            /// A list of courses with available activities for this <see cref="Person"/>.
            /// </summary>
            public List<ActivitiesByCourseInfo> Courses { get; set; }
        }

        /// <summary>
        /// A POCO for a course with all of its related activities.
        /// </summary>
        private class ActivitiesByCourseInfo : LavaDataDictionary
        {
            /// <summary>
            /// The activities in the course.
            /// </summary>
            public List<ActivityInfo> Activities { get; set; }

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
            /// The total number of activities newly available for this course and person.
            /// </summary>
            public int ActivityCount { get; set; }
        }

        /// <summary>
        /// A POCO for an individual activity.
        /// </summary>
        private class ActivityInfo : LavaDataDictionary
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

            /// <summary>
            /// The SystemCommunicationId that was used to send the learning notification.
            /// </summary>
            public int SystemCommunicationId { get; set; }
        }
    }
}

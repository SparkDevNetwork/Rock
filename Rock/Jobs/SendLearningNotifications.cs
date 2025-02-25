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

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Send Learning Activity Notifications
    /// </summary>
    [DisplayName( "Send Learning Notifications" )]
    [Description( "This job will send any unsent class announcements as well as an available activity digest emails for all their newly available activities within a learning program. The class announcements SystemCommunication is configured by the job setting and contains the Person and Announcement merge fields. The Available Activity Notification is configured by the learning program and contains ActivityCount and Courses (a list of CourseInfo) merge fields." )]

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

        private List<string> _warnings;
        private List<string> _errors;
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
                InitializeResultsCounters();

                using ( var rockContext = new RockContext() )
                {
                    SendActivityNotifications( rockContext );
                }

                using ( var rockContext = new RockContext() )
                {
                    SendAnnouncements( rockContext );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                _errors.Add( ex.Message );
            }

            SetJobResultSummary();

            // If we had any warnings or errors then make the job complete
            // with a warning state so it stands out for review.
            if ( _warnings.Any() || _errors.Any() )
            {
                throw new RockJobWarningException();
            }
        }

        /// <summary>
        /// Sends the pending learning class announcements.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        private void SendAnnouncements( RockContext rockContext )
        {
            var learningClassAnnouncementService = new LearningClassAnnouncementService( rockContext );

            var announcementsSystemCommunicationGuid = this.GetAttributeValue( AttributeKey.SystemCommunication ).AsGuidOrNull();
            if ( announcementsSystemCommunicationGuid == null )
            {
                _errors.Add( "The system communication selected for announcements is not valid." );
                return;
            }

            // Make sure the selected system communication exists.
            var announcementsSystemCommunication = new SystemCommunicationService( rockContext )
                .GetNoTracking( announcementsSystemCommunicationGuid.Value );

            if ( announcementsSystemCommunication == null )
            {
                _errors.Add( "Unable to retrieve the selected system communication." );
                return;
            }

            // Get the pending announcements and the potential recipients for those announcements.
            // Get as no tracking because we may be converting the Announcement.Description from a JSON object
            // to an HTML value (for emails). We don't want that change to be saved to the database.
            var pendingAnnouncements = learningClassAnnouncementService
                .GetUnsentAnnouncements()
                .AsNoTracking();
            var uniqueClassIds = pendingAnnouncements
                .Select( a => a.LearningClassId )
                .Distinct()
                .ToList();
            var potentialRecipients = new LearningParticipantService( rockContext )
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
                    learningClassAnnouncementService.UpdateCommunicationSentProperty( new List<int> { announcement.Id } );
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
            var programs = new LearningProgramService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( lp => lp.SystemCommunication )
                .Where( lp => lp.IsActive )
                .ToList();

            foreach ( var program in programs )
            {
                var personPrograms = GetActivityNotificationsForProgram( program, rockContext );

                foreach ( var studentProgram in personPrograms )
                {
                    try
                    {
                        var communicationId = SendNotificationForPerson( program.SystemCommunication, studentProgram );

                        if ( communicationId.HasValue )
                        {
                            using ( var updateRockContext = new RockContext() )
                            {
                                AddOrUpdateCompletionRecords( studentProgram, communicationId.Value, updateRockContext );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        _errors.Add( $"Error sending notifications for {studentProgram.Person.FullName}: {ex.Message}" );
                    }
                }
            }
        }

        /// <summary>
        /// Adds any <see cref="LearningActivity"/> records for which notifications were sent.
        /// If there are any existing <see cref="LearningActivityCompletion"/> records their
        /// NotificationCommunicationId property will be set to <c>true</c>.
        /// </summary>
        /// <param name="personActivitiesByCourse">The list of activities grouped by course for this person.</param>
        /// <param name="communicationId">The identifier of the communication that was sent to the person for this program.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        private static void AddOrUpdateCompletionRecords( PersonProgramActivitiesByCourseInfo personActivitiesByCourse, int communicationId, RockContext rockContext )
        {
            var learningActivityCompletionService = new LearningActivityCompletionService( rockContext );

            // Get a list of all activities for simpler querying.
            var activityInfos = personActivitiesByCourse.Courses.SelectMany( c => c.Activities );

            // Get the list of all ActivityIds that were referenced in the person's email.
            var activityIdsToAdd = activityInfos
                .Where( a => !a.LearningActivityCompletionId.HasValue )
                .Select( a => a.LearningActivityId )
                .ToList();

            // Now get the distinct LearningActivityCompletion records to update.
            var completionIdsToUpdate = activityInfos
                .Where( a => a.LearningActivityCompletionId.HasValue )
                .Select( a => a.LearningActivityCompletionId.Value )
                .Distinct()
                .ToList();

            // Update existing records.
            var existingCompletions = learningActivityCompletionService
                .GetByIds( completionIdsToUpdate )
                .ToList();

            foreach ( var existingCompletion in existingCompletions )
            {
                existingCompletion.SentNotificationCommunicationId = communicationId;
            }

            // Load all the activities. These must be loaded with tracking
            // because the call to LearningActivityCompletionService.GetNew() below
            // adds a reference to the activity and if it isn't tracked then EF
            // thinks it needs to be created which causes a conflict.
            var activities = new LearningActivityService( rockContext )
                .Queryable()
                .Where( la => activityIdsToAdd.Contains( la.Id ) )
                .ToList();

            // Get the distinct class identifiers that we need to load participants for.
            var learningClassIds = activities
                .Select( a => a.LearningClassId )
                .Distinct()
                .ToList();

            // The participant will differ across classes so we need to load
            // all related participant records that will be needed. These must
            // beloaded with tracking because the call to
            // LearningActivityCompletionService.GetNew() below adds a reference
            // to the activity and if it isn't tracked then EF thinks it needs to
            // be created which causes a conflict.
            var participants = new LearningParticipantService( rockContext )
                .Queryable()
                .Where( lp => lp.PersonId == personActivitiesByCourse.Person.Id
                    && learningClassIds.Contains( lp.LearningClassId ) )
                .ToList();

            // Get the activity data and transform it into the completion
            // records for the student.
            var activityCompletionsToAdd = activities
               .Select( a =>
               {
                   var participant = participants.FirstOrDefault( p => p.LearningClassId == a.LearningClassId );
                   var activity = LearningActivityCompletionService.GetNew( a, participant );

                   activity.SentNotificationCommunicationId = communicationId;

                   return activity;
               } )
               .ToList();

            // Add the new LearningActivityCompletion records to the context.
            learningActivityCompletionService.AddRange( activityCompletionsToAdd );

            rockContext.SaveChanges();
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
        /// Get all the activity notifications for a single program. These are
        /// grouped by course and then by person.
        /// </summary>
        /// <param name="program">The program to generate notifications for.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of <see cref="PersonProgramActivitiesByCourseInfo"/> objects.</returns>
        private static List<PersonProgramActivitiesByCourseInfo> GetActivityNotificationsForProgram( LearningProgram program, RockContext rockContext )
        {
            var studentRoleGuid = SystemGuid.GroupRole.GROUPROLE_LMS_CLASS_STUDENT.AsGuid();

            var courses = new LearningCourseService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( lc => lc.LearningProgramId == program.Id )
                .ToList();

            var classes = GetActiveClassesForProgram( program.Id, rockContext );

            // Use a lookup for these so that the sub-methods can add new records
            // and also add new activities to existing records across classes.
            var studentProgramLookup = new Dictionary<int, PersonProgramActivitiesByCourseInfo>();

            foreach ( var learningClass in classes )
            {
                // Load all the active students in this class who have an e-mail.
                var students = new LearningParticipantService( rockContext )
                    .Queryable()
                    .Include( lp => lp.Person )
                    .AsNoTracking()
                    .Where( lp => lp.LearningClassId == learningClass.Id
                        && lp.GroupMemberStatus == GroupMemberStatus.Active
                        && lp.GroupRole.Guid == studentRoleGuid
                        && !string.IsNullOrEmpty( lp.Person.Email ) );

                // Load all activities for this class.
                var activities = new LearningActivityService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( la => la.LearningClassId == learningClass.Id )
                    .OrderBy( la => la.Order )
                    .ThenBy( la => la.Id )
                    .ToList();

                // Load all existing completion records for students in this class.
                var completions = new LearningActivityCompletionService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( lac => lac.LearningActivity.LearningClassId == learningClass.Id )
                    .ToList();

                // Loop over each student and build up the notifications that
                // should be sent to them.
                foreach ( var student in students )
                {
                    PopulateStudentProgramNotifications( program, learningClass, activities, student, completions, studentProgramLookup );
                }
            }

            return studentProgramLookup.Values.ToList();
        }

        /// <summary>
        /// Generate all the notification details for the activities in the
        /// specified class for the student. This will update
        /// <paramref name="studentLookup"/> with the new information.
        /// </summary>
        /// <param name="program">The program that is being processed.</param>
        /// <param name="learningClass">The class that is being processed.</param>
        /// <param name="activities">All the activities for the class, including any non-active ones.</param>
        /// <param name="student">The student that we are going to generate notifications for.</param>
        /// <param name="completionsForClass">All activity completions for all students in this class.</param>
        /// <param name="studentLookup">The lookup dictionary that contains the student program notification details.</param>
        private static void PopulateStudentProgramNotifications( LearningProgram program, LearningClass learningClass, List<LearningActivity> activities, LearningParticipant student, List<LearningActivityCompletion> completionsForClass, Dictionary<int, PersonProgramActivitiesByCourseInfo> studentLookup )
        {
            var activityNotifications = GetActivityNotificationsForStudent( learningClass, activities, student, completionsForClass );

            if ( !activityNotifications.Any() )
            {
                return;
            }

            // Look up an existing person program record if we have
            // already queued some notifications from another class,
            // otherwise create a new record.
            if ( !studentLookup.TryGetValue( student.PersonId, out var studentProgram ) )
            {
                studentProgram = new PersonProgramActivitiesByCourseInfo
                {
                    Person = student.Person,
                    ProgramSystemCommunicationId = program.SystemCommunicationId,
                    Courses = new List<ActivitiesByCourseInfo>()
                };

                studentLookup.Add( student.PersonId, studentProgram );
            }

            // If we had any activities that the student should be notified
            // about then add this course (class) to their record.
            studentProgram.Courses.Add( new ActivitiesByCourseInfo
            {
                Activities = activityNotifications,
                ActivityCount = activityNotifications.Count,
                CourseCode = learningClass.LearningCourse.CourseCode,
                CourseId = learningClass.LearningCourseId,
                CourseName = learningClass.LearningCourse.Name,
                LearningParticipantId = student.Id,
                ProgramName = program.Name
            } );
        }

        /// <summary>
        /// Gets all the active classes for the program. This filters out classes
        /// that have been marked inactive as well as classes that have not
        /// started yet or already ended.
        /// </summary>
        /// <param name="programId">The identifier of the program to load classes for.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of <see cref="LearningClass"/> objects for the program.</returns>
        private static List<LearningClass> GetActiveClassesForProgram( int programId, RockContext rockContext )
        {
            var now = RockDateTime.Now;

            return new LearningClassService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( lc => lc.IsActive
                    && lc.LearningCourse.IsActive
                    && lc.LearningCourse.LearningProgramId == programId )
                // Either always available semesters (no end date) or end date is in the future.
                .Where( lc => !lc.LearningSemester.EndDate.HasValue
                    || lc.LearningSemester.EndDate >= now )
                // Either always available semesters (no start date) or start date is in the past.
                .Where( lc => !lc.LearningSemester.StartDate.HasValue
                    || lc.LearningSemester.StartDate <= now )
                .ToList();
        }

        /// <summary>
        /// Gets the activity notification details for a single student in the class.
        /// </summary>
        /// <param name="learningClass">The class that is being processed.</param>
        /// <param name="activities">All activities, including inactive ones, for the class.</param>
        /// <param name="student">The student that is being processed.</param>
        /// <param name="completionsForClass">All existing completions for all students in this class.</param>
        /// <returns>A collection of <see cref="ActivityInfo"/> objects that represent the notifications to be sent.</returns>
        private static List<ActivityInfo> GetActivityNotificationsForStudent( LearningClass learningClass, List<LearningActivity> activities, LearningParticipant student, List<LearningActivityCompletion> completionsForClass )
        {
            var activitiesToSend = new List<ActivityInfo>();

            for ( int i = 0; i < activities.Count; i++ )
            {
                var activity = activities[i];
                var previousActivity = i > 0 ? activities[i - 1] : null;

                if ( !activity.SendNotificationCommunication )
                {
                    continue;
                }

                if ( !IsActivityAvailable( learningClass, activity, previousActivity, student, completionsForClass ) )
                {
                    continue;
                }

                // Make sure we haven't already sent a communication out for
                // this activity.
                var alreadyNotified = completionsForClass.Any( lac => lac.StudentId == student.Id
                    && lac.LearningActivityId == activity.Id
                    && lac.SentNotificationCommunicationId.HasValue );

                if ( alreadyNotified )
                {
                    continue;
                }

                activitiesToSend.Add( new ActivityInfo
                {
                    LearningActivityId = activity.Id,
                    LearningActivityCompletionId = null,
                    ActivityName = activity.Name,
                    AvailableDate = null,
                    DueDate = activity.DueDateCalculated,
                    Order = activity.Order
                } );
            }

            return activitiesToSend;
        }

        /// <summary>
        /// Determines if this activity is available yet based on the
        /// <see cref="LearningActivity.AvailabilityCriteria"/> value.
        /// </summary>
        /// <param name="learningClass">The class this activity belongs to.</param>
        /// <param name="activity">The activity being processed.</param>
        /// <param name="previousActivity">The activity that is configured to be prior to this activity or <c>null</c> if this is the first activity.</param>
        /// <param name="student">The student being processed.</param>
        /// <param name="completionsForClass">All existing completions for all students in this class.</param>
        /// <returns><c>true</c> if this activity is available and should be notified; otherwise <c>false</c>.</returns>
        private static bool IsActivityAvailable( LearningClass learningClass, LearningActivity activity, LearningActivity previousActivity, LearningParticipant student, List<LearningActivityCompletion> completionsForClass )
        {
            if ( activity.AvailabilityCriteria == AvailabilityCriteria.AfterPreviousCompleted )
            {
                var previousCompletion = completionsForClass
                    .Where( lac => lac.StudentId == student.Id
                        && lac.LearningActivityId == previousActivity.Id )
                    .FirstOrDefault();

                if ( previousCompletion == null || !previousCompletion.CompletedDateTime.HasValue )
                {
                    return false;
                }
            }
            else if ( IsDateCriteria( activity.AvailabilityCriteria ) )
            {
                var date = LearningActivity.CalculateAvailableDate( activity.AvailabilityCriteria,
                    activity.AvailableDateDefault,
                    activity.AvailableDateOffset,
                    learningClass.LearningSemester?.StartDate,
                    student.CreatedDateTime );

                if ( date.HasValue && date.Value > RockDateTime.Now )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if this criteria represents a date-based check.
        /// </summary>
        /// <param name="criteria">The criteria enum value.</param>
        /// <returns><c>true</c> if the activity should be checked with <see cref="LearningActivity.CalculateAvailableDate"/>; otherwise <c>false</c>.</returns>
        private static bool IsDateCriteria( AvailabilityCriteria criteria )
        {
            return criteria == AvailabilityCriteria.ClassStartOffset
                || criteria == AvailabilityCriteria.EnrollmentOffset
                || criteria == AvailabilityCriteria.SpecificDate;
        }

        /// <summary>
        /// Sends the digest email for the specified person.
        /// </summary>
        /// <param name="systemCommunication">The details of the communication to be sent.</param>
        /// <param name="personProgramActivitiesByCourse">The person and their activities to notify.</param>
        /// <returns><c>true</c> if the email was successfully sent; otherwise <c>false</c>.</returns>
        private int? SendNotificationForPerson( SystemCommunication systemCommunication, PersonProgramActivitiesByCourseInfo personProgramActivitiesByCourse )
        {
            try
            {
                // Add the merge objects to support this notification.
                var mergeFields = LavaHelper.GetCommonMergeFields( null );
                mergeFields.AddOrReplace( "ActivityCount", personProgramActivitiesByCourse.Courses.Sum( c => c.ActivityCount ) );
                mergeFields.AddOrReplace( "Courses", personProgramActivitiesByCourse.Courses );

                var communicationId = SendCommunication( personProgramActivitiesByCourse.Person, systemCommunication, mergeFields );

                if ( communicationId.HasValue )
                {
                    _notificationsSent++;
                }

                return communicationId;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                _errors.Add( $"Unable to send Learning Activity Available Notifications to {personProgramActivitiesByCourse.Person.FullName}. '{ex.Message}'" );
            }

            return null;
        }

        /// <summary>
        /// Send a single communication to the person.
        /// </summary>
        /// <param name="person">The person that will receive the communication.</param>
        /// <param name="systemCommunication">The <see cref="SystemCommunication"/> that provides the content.</param>
        /// <param name="mergeFields">The merge fields used to prepare the content.</param>
        /// <returns>The identifier of the <see cref="Communication"/> that was sent.</returns>
        private int? SendCommunication( Person person, SystemCommunication systemCommunication, Dictionary<string, object> mergeFields )
        {
            var logger = RockLogger.LoggerFactory.CreateLogger<CommunicationHelper>();
            var createMessageResults = CommunicationHelper.CreateEmailMessage( person, mergeFields, systemCommunication, logger );

            if ( createMessageResults.Message == null )
            {
                _warnings.AddRange( createMessageResults.Warnings );
                return null;
            }

            if ( !( createMessageResults.Message is RockEmailMessage message ) )
            {
                return null;
            }

            message.CreateCommunicationRecordImmediately = true;

            if ( !message.Send( out var errorMessages ) )
            {
                _errors.AddRange( errorMessages );
                return null;
            }

            return message.LastCommunicationId;
        }

        /// <summary>
        /// Sets the Result property based on the notificationsSent, warnings and errors.
        /// </summary>
        private void SetJobResultSummary()
        {
            StringBuilder jobSummaryBuilder = new StringBuilder();
            jobSummaryBuilder.AppendLine( "Summary:" );
            jobSummaryBuilder.AppendLine();

            jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {_notificationsSent} {"notification".PluralizeIf( _notificationsSent != 1 )} sent" );

            jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {_distinctClassAnnouncementsSent} {"announcement".PluralizeIf( _distinctClassAnnouncementsSent != 1 )} sent to {_distinctAnnouncementMessagesSent} {"individual".PluralizeIf( _distinctAnnouncementMessagesSent != 1 )}" );

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
        private class PersonProgramActivitiesByCourseInfo
        {
            public Person Person { get; set; }

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
        private class ActivitiesByCourseInfo : LavaDataObject
        {
            /// <summary>
            /// The learning program name of the activity.
            /// </summary>
            public string ProgramName { get; set; }

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
        private class ActivityInfo : LavaDataObject
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
    }
}

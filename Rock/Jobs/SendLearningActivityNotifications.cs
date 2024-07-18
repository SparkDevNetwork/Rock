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
using Rock.Model;
using Rock.Utility;

namespace Rock.Jobs
{
    /// <summary>
    /// Send Learning Activity Available Notifications
    /// </summary>
    [DisplayName( "Send Learning Activity Notifications" )]
    [Description( "This job will send a single email for each student with newly available activities. The email is based on the configured System Communication template and that template should contain the following merge objects: Courses and ActivityCount." )]

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
        private LearningActivityCompletionService _learningActivityCompletionService;

        private IList<string> _warnings;
        private IList<string> _errors;
        private int _notificationsSent;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
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
                        return;
                    }
                    else
                    {
                        SendNotifications( systemCommunication, courseDataByPerson );
                    }

                    var completionIds = courseDataByPerson.SelectMany( p => p.Courses.SelectMany( c => c.Activities.Select( a => a.Id ) ) ).ToList();
                    _learningActivityCompletionService.UpdateNotificationCommunicationProperties( completionIds, systemCommunication.Id );
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
            _learningActivityCompletionService = new LearningActivityCompletionService( rockContext );
        }

        private List<PersonActivitiesByCourse> GetAggregateData()
        {
            var now = RockDateTime.Now;

            return _learningActivityCompletionService.Queryable()
                .AsNoTracking()
                .Include( a => a.LearningActivity )
                .Include( a => a.LearningActivity.LearningClass )
                .Include( a => a.LearningActivity.LearningClass.LearningCourse )
                .Include( a => a.LearningActivity.LearningClass.LearningCourse.LearningProgram )
                // Is configured to send a communication.
                .Where( a => a.LearningActivity.SendNotificationCommunication )
                // Hasn't already been sent.
                .Where( a => a.NotificationCommunicationId == null )
                // Is currently available.
                .Where(
                    a => a.AvailableDateTime <= now ||
                    Enums.Lms.AvailableDateCalculationMethod.AlwaysAvailable == a.LearningActivity.AvailableDateCalculationMethod )
                // Is assigned to a student
                .Where( a => a.LearningActivity.AssignTo == Enums.Lms.AssignTo.Student )
                // Get activities grouped by Participant and Course data.
                .GroupBy( a => new
                {
                    // Group By Person properties
                    a.Student.Person.Email,
                    PersonNickName = a.Student.Person.NickName,
                    PersonLastName = a.Student.Person.LastName,
                    PersonSuffixValueId = a.Student.Person.SuffixValueId,
                    PersonId = a.Student.PersonId
                }, a => new
                {
                    // with values for course and activities data.
                    ProgramName = a.LearningActivity.LearningClass.LearningCourse.LearningProgram.Name,
                    CourseName = a.LearningActivity.LearningClass.LearningCourse.Name,
                    a.LearningActivity.LearningClass.LearningCourse.CourseCode,
                    a.Id,
                    a.DueDate,
                    a.AvailableDateTime,
                    a.LearningActivity.Order,
                    ActivityName = a.LearningActivity.Name
                } )
                .ToList()
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
                        groupingKey.CourseName,
                        groupingKey.CourseCode,
                        groupingKey.ProgramName
                    }, groupingValue => new
                    {
                        groupingValue.ActivityName,
                        groupingValue.AvailableDateTime,
                        groupingValue.DueDate,
                        groupingValue.Id,
                        groupingValue.Order
                    } ).Select( groupedResult => new ActivitiesByCourse
                    {
                        CourseCode = groupedResult.Key.CourseCode,
                        CourseName = groupedResult.Key.CourseName,
                        ProgramName = groupedResult.Key.ProgramName,
                        ActivityCount = groupedResult.Count(),
                        Activities = groupedResult.Select( a => new Activity
                        {
                            ActivityName = a.ActivityName,
                            AvailableDate = a.AvailableDateTime,
                            DueDate = a.DueDate,
                            Id = a.Id,
                            Order = a.Order
                        } ).ToList()
                    } ).ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Sends the digest emails for each regional group within the attendance summary.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="personsActivitiesByCourse">The list of .</param>
        private void SendNotifications( SystemCommunication systemCommunication, List<PersonActivitiesByCourse> personsActivitiesByCourse )
        {

            foreach ( var personActivitiesByCourse in personsActivitiesByCourse )
            {
                // Send a separate email to each leader within this regional group.
                var person = new Person
                {
                    Id = personActivitiesByCourse.PersonId,
                    NickName = personActivitiesByCourse.PersonNickName,
                    LastName = personActivitiesByCourse.PersonLastName,
                    SuffixValueId = personActivitiesByCourse.PersonSuffixValueId,
                    Email = personActivitiesByCourse.Email
                };

                SendNotificationForPerson( person, personActivitiesByCourse.Courses, systemCommunication );
            }
        }

        private void SendNotificationForPerson( Person person, List<ActivitiesByCourse> activitiesByCourse, SystemCommunication systemCommunication )
        {
            // Add the merge objects to support this notification.
            var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, person );
            mergeObjects.AddOrReplace( "ActivityCount", activitiesByCourse.Sum( c => c.ActivityCount ) );
            mergeObjects.AddOrReplace( "Courses", activitiesByCourse );

            var recipient = new RockEmailMessageRecipient( person, mergeObjects );
            var message = new RockEmailMessage( systemCommunication );
            message.Subject = systemCommunication.Subject.ResolveMergeFields( mergeObjects );
            message.SetRecipients( new List<RockEmailMessageRecipient> { recipient } );
            message.Send( out List<string> errorMessages );

            if ( !errorMessages.Any() )
            {
                _notificationsSent++;
                return;
            }

            _errors.Add( $"Unable to send Learning Activity Available Notifications to '{person.Email}'." );
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
            /// The Id of the <see cref="LearningActivityCompletion"/> the notification is for.
            /// </summary>
            /// <remarks>
            /// This is used by the job to mark notifications that have been sent.
            /// </remarks>
            public int Id { get; set; }

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
        /// A class to represent a Person and all the courses with available activities requiring notification.
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
            /// The learning course code of the activity.
            /// </summary>
            public string CourseCode { get; set; }

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

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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Jobs;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Crm.Prayer
{
    /// <summary>
    /// Integration tests for the "Send Prayer Comments" Job.
    /// </summary>
    [TestClass]
    public class SendPrayerCommentsJobTests : DatabaseTestsBase
    {
        #region Initialization

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            TestDataHelper.AddTestDataSet( TestDataHelper.DataSetIdentifiers.PrayerSampleData );
        }

        #endregion

        #region Configuration

        private DateTime _TestPeriodStartDate = new DateTime( 2019, 1, 1 );

        //protected override void OnValidateTestData( out bool isValid, out string stateMessage )
        //{
        //    try
        //    {
        //        // Verify that the necessary test data exists by retrieving a well-known test record.
        //        var dataContext = GetNewDataContext();

        //        var prayerRequestService = new PrayerRequestService( dataContext );

        //        var testEntryId = prayerRequestService.GetId( TestGuids.PrayerRequestGuid.AllChurchForEmployment.AsGuid() );

        //        if ( testEntryId == null )
        //        {
        //            throw new Exception( "Prayer Request test data is either incomplete or does not exist in this database." );
        //        }

        //        isValid = true;
        //        stateMessage = null;
        //    }
        //    catch ( Exception ex )
        //    {
        //        isValid = false;
        //        stateMessage = ex.Message;
        //    }
        //}

        #endregion

        #region Tests

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsWithInvalidEmailTemplate_FailsWithError()
        {
            var job = GetJobWithDefaultConfiguration();

            // Ensure no system email template is specified.
            job.SystemEmailTemplateGuid = null;

            // Configure for the first week of the test period.
            job.StartDate = _TestPeriodStartDate;
            job.EndDate = _TestPeriodStartDate.AddDays( 6 );

            job.SendNotifications();

            // Verify that the error message is as expected.
            var message = job.MainLog.GetLastMessage( SendPrayerComments.TaskLog.TaskLogMessage.MessageTypeSpecifier.Error );

            Assert.IsNotNull( message, "Expected error not found." );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_DateFilterForWeek1_ReturnsCommentsInWeek1Only()
        {
            var dataContext = new RockContext();

            var job = GetJobWithDefaultConfiguration();

            // Configure for the first week of the test period.
            job.StartDate = _TestPeriodStartDate;
            job.EndDate = _TestPeriodStartDate.AddDays( 6 );

            job.LoadPrayerRequests();

            var requestWeek1 = job.PrayerRequests.FirstOrDefault( x => x.Guid == TestGuids.PrayerRequestGuid.TedDeckerForEmployment.AsGuid() );

            Assert.IsNotNull( requestWeek1, "Expected Prayer Request not found." );

            // Verify that there are no comments that were entered outside the reporting period.
            var requestOther = job.PrayerComments.FirstOrDefault( x => x.CreatedDateTime < job.StartDate || x.CreatedDateTime > job.EndDate );

            Assert.IsNull( requestOther, "Unexpected Prayer Comments found." );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_DateFilterForWeek2_ReturnsWeek1PrayerRequestWithWeek2Comments()
        {
            var dataContext = new RockContext();

            var job = GetJobWithDefaultConfiguration();

            // Configure for the first week of the test period.
            job.StartDate = _TestPeriodStartDate.AddDays( 7 );
            job.EndDate = _TestPeriodStartDate.AddDays( 14 );

            job.LoadPrayerRequests();

            // Verify a Week 1 prayer request is returned.
            var requestWeek1 = job.PrayerRequests.FirstOrDefault( x => x.Guid == TestGuids.PrayerRequestGuid.TedDeckerForEmployment.AsGuid() );

            Assert.IsNotNull( requestWeek1, "Expected Prayer Request not found." );

            // Verify a Week 2 comment is returned.
            var commentWeek2 = job.PrayerComments.FirstOrDefault( x => x.Guid == TestGuids.PrayerRequestCommentGuid.TedDecker12.AsGuid() );

            Assert.IsNotNull( requestWeek1, "Expected Prayer Request not found." );

            // Verify that there are no comments that were entered outside the reporting period.
            var requestOther = job.PrayerComments.FirstOrDefault( x => x.CreatedDateTime < job.StartDate || x.CreatedDateTime > job.EndDate );

            Assert.IsNull( requestOther, "Unexpected Prayer Comments found." );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_PrivateComments_AreExcluded()
        {
            var dataContext = new RockContext();

            var job = GetJobWithDefaultConfiguration();

            // Configure for Week 2 of the test period.
            job.StartDate = _TestPeriodStartDate.AddDays( 7 );
            job.EndDate = _TestPeriodStartDate.AddDays( 14 );

            job.LoadPrayerRequests();

            // Brian Jones has a Prayer Request with 1 public and 1 private comment in week 1.

            // Verify the correct prayer request is returned.
            var requestWeek1 = job.PrayerRequests.FirstOrDefault( x => x.Guid == TestGuids.PrayerRequestGuid.BenJonesForCarFinance.AsGuid() );

            Assert.IsNotNull( requestWeek1, "Expected Prayer Request not found." );

            // Verify the public comment is returned.
            var commentPublic = job.PrayerComments.FirstOrDefault( x => x.Guid == TestGuids.PrayerRequestCommentGuid.BenJonesComment1.AsGuid() );

            Assert.IsNotNull( commentPublic, "Expected Comment not found. Public commment should be included." );

            // Verify the private comment is not returned.
            var commentPrivate = job.PrayerComments.FirstOrDefault( x => x.Guid == TestGuids.PrayerRequestCommentGuid.BenJonesComment2.AsGuid() );

            Assert.IsNull( commentPrivate, "Unexpected Comment found. Private commment should not be included." );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_FilterWithChildCategoriesExcluded_ReturnsRequestsInParentCategoryOnly()
        {
            var dataContext = new RockContext();

            var job = GetJobWithDefaultConfiguration();

            job.CategoryGuidList = new List<Guid> { TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid() };
            job.IncludeChildCategories = false;
            job.CreateCommunicationRecord = false;

            job.LoadPrayerRequests();

            // Verify at least one known prayer request in the parent category is returned.
            var parentCategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid() ).GetValueOrDefault();

            Assert.IsTrue( job.PrayerRequests.Any( x => x.CategoryId == parentCategoryId ), "Expected Prayer Request not found in parent category." );

            // Verify no requests are returned outside of the parent category.
            Assert.IsFalse( job.PrayerRequests.Any( x => x.CategoryId != parentCategoryId ), "Unexpected Prayer Request found." );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_ChildCategoriesIncluded_ReturnsRequestsInParentAndChildCategories()
        {
            var dataContext = new RockContext();

            var job = GetJobWithDefaultConfiguration();

            job.CategoryGuidList = new List<Guid> { TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid() };
            job.IncludeChildCategories = true;
            job.CreateCommunicationRecord = false;

            job.LoadPrayerRequests();

            // Verify at least one known prayer request in the parent category is returned.
            var parentCategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid() ).GetValueOrDefault();

            Assert.IsTrue( job.PrayerRequests.Any( x => x.CategoryId == parentCategoryId ), "Expected Prayer Request not found in parent category." );

            // Verify at least one known prayer request in a child category is returned.
            var employmentCategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestJobOnly.AsGuid() ).GetValueOrDefault();

            Assert.IsTrue( job.PrayerRequests.Any( x => x.CategoryId == employmentCategoryId ), "Expected Prayer Request not found in child category." );

            // Verify no requests are returned outside of the parent and child categories is returned.
            var allCategories = new List<Guid> { TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid(),
                                                 TestGuids.Category.PrayerRequestFinancesOnly.AsGuid(),
                                                 TestGuids.Category.PrayerRequestJobOnly.AsGuid() };

            Assert.IsFalse( job.PrayerRequests.Any( x => !allCategories.Contains( x.Category.Guid ) ), "Unexpected Prayer Request found." );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsWithCreateCommunicationEnabled_CreatesCommunicationEntries()
        {
            var job = GetJobWithDefaultConfiguration();

            // Configure for the first week of the test period.
            job.StartDate = _TestPeriodStartDate;
            job.EndDate = _TestPeriodStartDate.AddDays( 6 );

            job.SendNotifications();

            //TODO:Assert
        }

        /// <summary>
        /// Verify that the lava merge fields are correctly merged into the standard system template.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_CommunicationTemplateValidMergeFields_AreCorrectlyReplacedInTemplate()
        {
            var job = GetJobWithDefaultConfiguration();

            job.CategoryGuidList = new List<Guid> { TestGuids.Category.PrayerRequestFinancesOnly.AsGuid() };
            job.IncludeChildCategories = false;
            job.SystemEmailTemplateGuid = TestGuids.SystemEmailGuid.PrayerCommentsNotification.AsGuid();

            job.LoadPrayerRequests();
            job.PrepareNotifications();
            job.SendNotifications();

            var notification = job.Notifications.FirstOrDefault();

            //TODO:Assert
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsFirstRun_SetsLastNotificationDateToToday()
        {
            var job = GetJobWithDefaultConfiguration();

            // Reset the last notification date to simulate a first run of this Job.
            job.SetLastNotificationDate( null );

            var lastNotificationDate = job.GetLastNotificationDate();

            Assert.IsNull( lastNotificationDate, "Expected Last Notification Date to be null but it has a value." );

            job.CreateCommunicationRecord = true;

            // Configure for the first week of the test period.
            var endDate = _TestPeriodStartDate.AddDays( 6 );

            job.StartDate = _TestPeriodStartDate;
            job.EndDate = endDate;

            job.Execute();

            Assert.AreEqual( endDate, job.GetLastNotificationDate() );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsSecondRun_SetsNextStartTimeToLastEndTime()
        {
            var job = GetJobWithDefaultConfiguration();

            // Configure for the first week of the test period.
            var endDate = _TestPeriodStartDate.AddDays( 6 );

            job.StartDate = _TestPeriodStartDate;
            job.EndDate = endDate;

            job.Execute();

            // Simulate executing the task on 2/1/2019.
            // We should not see any new comments.
            var nextStartDate = job.GetNextStartDate();

            Assert.AreEqual( endDate, nextStartDate.Date );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsForWeek1_ContainsCorrectComments()
        {
            // Simulate executing the task on Day 7 of the test period.
            var job = GetJobWithDefaultConfiguration();

            DateTime nextStartDate = _TestPeriodStartDate;

            job.StartDate = _TestPeriodStartDate;

            var endDate = _TestPeriodStartDate.AddDays( 6 );

            job.EndDate = endDate;

            job.PrepareNotifications();

            var notification = job.Notifications.FirstOrDefault( x => x.GetRecipients().Any( y => y.To == "ted@rocksolidchurchdemo.com" ) );

            // TODO: Verify that we have an update for Ted Decker, with comments from Sarah and Ben.

            job.Execute();

            // Simulate executing the task on for Day 7 - Day 14 of the test period.
            // We should not see any of the Week 1 comments, only Week 2.
            nextStartDate = job.GetNextStartDate();

            job.StartDate = nextStartDate;

            endDate = _TestPeriodStartDate.AddDays( 13 );

            job.EndDate = endDate;

            // TODO: Verify that we have an update for Ted Decker, with comments from Sarah and Ben.
            job.LoadPrayerRequests();
            job.PrepareNotifications();

            job.Execute();

            //TODO:Assert
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsForWeek2_ContainsCorrectComments()
        {
            // Simulate executing the task on Day 7 of the test period.
            var job = GetJobWithDefaultConfiguration();

            // Simulate executing the task on for Day 7 - Day 14 of the test period.
            // We should not see any of the Week 1 comments, only Week 2.
            job.StartDate = job.GetNextStartDate();

            var endDate = _TestPeriodStartDate.AddDays( 13 );

            job.EndDate = endDate;

            job.LoadPrayerRequests();
            job.PrepareNotifications();

            job.Execute();

            // TODO: Assert?
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        /// <remarks>
        /// This test requires a fully-functioning notification service.
        /// </remarks>
        [TestMethod]
        public void SendPrayerComments_SendNotificationsRepeatExecution_DoesNotSendAdditionalNotifications()
        {
            var job = GetJobWithDefaultConfiguration();

            // Reset the last notification date.
            job.SetLastNotificationDate( null );

            // Simulate executing the task on Day 1 of the test period at 4:30pm.
            // The job should deliver notifications for comments on Day 1 prior to 4:30 only.
            // The specified category should only contain a single Prayer Request.
            var jobStartDateRun1 = _TestPeriodStartDate.AddHours( 16 ).AddMinutes( 30 );

            job.StartDate = jobStartDateRun1.AddDays( -1 ).Date;
            job.EndDate = jobStartDateRun1;
            job.CategoryGuidList = new List<Guid> { TestGuids.Category.PrayerRequestFinancesOther.AsGuid() };

            job.Execute();

            Assert.AreEqual( 1, job.PrayerRequests.Count, "Incorrect Prayer Request count." );
            Assert.IsTrue( job.Notifications.Any(), "Notifications expected but not found." );

            // Repeat the execution, using the system setting for the next start date.            
            // There should be no new comments, and therefore no notifications to send.
            job = GetJobWithDefaultConfiguration();

            job.StartDate = job.GetNextStartDate();
            job.EndDate = null;
            job.CategoryGuidList = new List<Guid> { TestGuids.Category.PrayerRequestFinancesOther.AsGuid() };

            // Verify that the start date for this job execution is the same as the end date of the previous execution.
            Assert.AreEqual( jobStartDateRun1, job.StartDate.Value );

            job.Execute();

            Assert.IsFalse( job.Notifications.Any(), "Notifications found but not expected." );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Create a an instance of the Job "Send Prayer Comments" with a default test configuration.
        /// </summary>
        /// <returns></returns>
        private SendPrayerComments GetJobWithDefaultConfiguration()
        {
            var job = new SendPrayerComments();

            job.SystemEmailTemplateGuid = TestGuids.SystemEmailGuid.PrayerCommentsNotification.AsGuid();
            job.CategoryGuidList = null;
            job.SystemSettingsId = Rock.Tests.Shared.RecordTag.PrayerRequestFeature;
            job.CreateCommunicationRecord = true;

            // Disable sending the actual email because we may not have a transport mechanism set up in the test environment.
            // This should be tested as part of the Communications feature instead.
            job.SendEmailNotification = false;

            return job;
        }

        #endregion
    }
}

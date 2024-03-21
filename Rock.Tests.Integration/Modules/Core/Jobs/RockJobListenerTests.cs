using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock.Communication.Transport;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    [TestClass]
    public class RockJobListenerTests : DatabaseTestsBase
    {
        public static ConcurrentStack<string> actualEmails = new ConcurrentStack<string>();
        private int smtpPort;
        private string _originalSmtpPort;
        private string _originalSmtpServer;
        private string TestJobGuidString = "84AE12A7-968B-4D28-AB39-81D36D1F230E";

        [TestInitialize]
        public void TestInitialize()
        {
            RemoveTestJob();

            smtpPort = GetFirstAvailablePort();
            UpdateSmtpPortNumber( smtpPort.ToString() );
            UpdateSmtpServerNumber( "localhost" );
            StartEmailServer();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            RemoveTestJob();
            UpdateSmtpPortNumber( _originalSmtpPort );
            UpdateSmtpServerNumber( _originalSmtpServer );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleExceptionCorrectly()
        {
            var expectedExceptionMessage = $"{Guid.NewGuid()} Rock Job Listener Exception Message";
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.Exception, expectedExceptionMessage );

            RunJob( jobDataMapDictionary );

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( expectedExceptionMessage, actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Exception", actualJob.LastStatus );

            var exceptions = new ExceptionLogService( new RockContext() ).Queryable().Where( els => els.Description == expectedExceptionMessage );
            Assert.That.IsTrue( exceptions.Count() == 1 );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleExceptionNotificationsCorrectly()
        {
            var expectedExceptionMessage = $"{Guid.NewGuid()} Rock Job Listener Exception Message";

            RunJob( GetJobDataMapDictionary( TestResultType.Exception, expectedExceptionMessage ), JobNotificationStatus.All );

            actualEmails.TryPop( out var message );
            Assert.That.Contains( message, expectedExceptionMessage );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Exception, expectedExceptionMessage ), JobNotificationStatus.Error );

            actualEmails.TryPop( out message );
            Assert.That.Contains( message, expectedExceptionMessage );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Exception, expectedExceptionMessage ), JobNotificationStatus.Success );

            Assert.That.IsFalse( actualEmails.TryPop( out message ) );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Exception, expectedExceptionMessage ), JobNotificationStatus.None );

            Assert.That.IsFalse( actualEmails.TryPop( out message ) );
            RemoveTestJob();
        }

        [TestMethod]
        public void RockJobListenerShouldHandleWarningNotificationsCorrectly()
        {
            var expectedExceptionMessage = $"{Guid.NewGuid()} Rock Job Listener Warning Message";

            RunJob( GetJobDataMapDictionary( TestResultType.Warning, expectedExceptionMessage ), JobNotificationStatus.All );

            actualEmails.TryPop( out var message );
            Assert.That.Contains( message, expectedExceptionMessage );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Warning, expectedExceptionMessage ), JobNotificationStatus.Error );

            actualEmails.TryPop( out message );
            Assert.That.Contains( message, expectedExceptionMessage );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Warning, expectedExceptionMessage ), JobNotificationStatus.Success );

            Assert.That.IsFalse( actualEmails.TryPop( out message ) );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Warning, expectedExceptionMessage ), JobNotificationStatus.None );

            Assert.That.IsFalse( actualEmails.TryPop( out message ) );
            RemoveTestJob();
        }

        [TestMethod]
        public void RockJobListenerShouldHandleSuccessNotificationsCorrectly()
        {
            var expectedExceptionMessage = $"{Guid.NewGuid()} Rock Job Listener Success";

            RunJob( GetJobDataMapDictionary( TestResultType.Success, expectedExceptionMessage ), JobNotificationStatus.All );

            actualEmails.TryPop( out var message );
            Assert.That.Contains( message, expectedExceptionMessage );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Success, expectedExceptionMessage ), JobNotificationStatus.Error );

            Assert.That.IsFalse( actualEmails.TryPop( out message ) );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Success, expectedExceptionMessage ), JobNotificationStatus.Success );

            actualEmails.TryPop( out message );
            Assert.That.Contains( message, expectedExceptionMessage );
            RemoveTestJob();

            RunJob( GetJobDataMapDictionary( TestResultType.Success, expectedExceptionMessage ), JobNotificationStatus.None );

            Assert.That.IsFalse( actualEmails.TryPop( out message ) );
            RemoveTestJob();
        }

        [TestMethod]
        public void RockJobListenerShouldHandleMultipleAggregateExceptionCorrectly()
        {
            var expectedExceptionMessage = $"{Guid.NewGuid()} Rock Job Listener Multiple Aggregate Exception Message";
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.MultipleAggregateException, expectedExceptionMessage );

            RunJob( jobDataMapDictionary );

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( $"One or more exceptions occurred. First Exception: {expectedExceptionMessage} 1", actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Exception", actualJob.LastStatus );

            var exceptions = new ExceptionLogService( new RockContext() ).Queryable().Where( els => els.Description.Contains( expectedExceptionMessage ) );
            Assert.That.IsTrue( exceptions.Count() > 1 );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleSingleAggregateExceptionCorrectly()
        {
            var expectedExceptionMessage = $"{Guid.NewGuid()} Rock Job Listener Single Aggregate Exception Message";
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.SingleAggregateException, expectedExceptionMessage );
            RunJob( jobDataMapDictionary );

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( expectedExceptionMessage, actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Exception", actualJob.LastStatus );

            var exceptions = new ExceptionLogService( new RockContext() ).Queryable().Where( els => els.Description == expectedExceptionMessage );
            Assert.That.IsTrue( exceptions.Count() == 1 );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleWarningExceptionCorrectly()
        {
            var expectedResultMessage = $"{Guid.NewGuid()} Rock Job Listener Completed With Warnings";
            var expectedExceptionsCount = new ExceptionLogService( new RockContext() ).Queryable().Count();
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.Warning, expectedResultMessage );

            RunJob( jobDataMapDictionary );

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( expectedResultMessage, actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Warning", actualJob.LastStatus );

            var exceptions = new ExceptionLogService( new RockContext() ).Queryable().Where( els => els.Description == expectedResultMessage );
            Assert.That.IsTrue( exceptions.Count() == 1 );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleWarningWithMultipleAggregateException()
        {
            var expectedResultMessage = $"{Guid.NewGuid()} Rock Job Listener Completed With Warnings";
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.WarningWithMultipleAggregateException, expectedResultMessage );

            RunJob( jobDataMapDictionary );

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( expectedResultMessage, actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Warning", actualJob.LastStatus );

            var exceptions = new ExceptionLogService( new RockContext() ).Queryable().Where( els => els.Description == expectedResultMessage );
            Assert.That.IsTrue( exceptions.Count() == 1 );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleWarningWithSingleAggregateException()
        {
            var expectedResultMessage = $"{Guid.NewGuid()} Rock Job Listener Completed With Warnings";
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.WarningWithSingleAggregateException, expectedResultMessage );

            RunJob( jobDataMapDictionary );

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( expectedResultMessage, actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Warning", actualJob.LastStatus );

            var exceptions = new ExceptionLogService( new RockContext() ).Queryable().Where( els => els.Description == expectedResultMessage );
            Assert.That.IsTrue( exceptions.Count() == 1 );
        }

        [TestMethod]
        public void RockJobListenerShouldHandleSuccessCorrectly()
        {
            var expectedResultMessage = $"{Guid.NewGuid()} Rock Job Listener Success!";
            var expectedExceptionsCount = new ExceptionLogService( new RockContext() ).Queryable().Count();
            var jobDataMapDictionary = GetJobDataMapDictionary( TestResultType.Success, expectedResultMessage );

            RunJob( jobDataMapDictionary );

            var actualExceptionsCount = new ExceptionLogService( new RockContext() ).Queryable().Count();

            var actualJob = GetAddTestJob( jobDataMapDictionary );

            Assert.That.AreEqual( expectedResultMessage, actualJob.LastStatusMessage );
            Assert.That.AreEqual( "Success", actualJob.LastStatus );

            Assert.That.AreEqual( expectedExceptionsCount, actualExceptionsCount );
        }

        public void RunJob( Dictionary<string, string> jobDataMapDictionary, JobNotificationStatus jobNotificationStatus = JobNotificationStatus.None )
        {
            var job = GetAddTestJob( jobDataMapDictionary, jobNotificationStatus );

            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );

                if ( job != null )
                {
                    // create a scheduler specific for the job
                    var scheduleConfig = new System.Collections.Specialized.NameValueCollection();
                    var runNowSchedulerName = ( "RunNow:" + job.Guid.ToString( "N" ) ).Truncate( 40 );
                    scheduleConfig.Add( StdSchedulerFactory.PropertySchedulerInstanceName, runNowSchedulerName );
                    var schedulerFactory = new StdSchedulerFactory( scheduleConfig );
                    var sched = new StdSchedulerFactory( scheduleConfig ).GetScheduler();
                    if ( sched.IsStarted )
                    {
                        // the job is currently running as a RunNow job
                        return;
                    }

                    // Check if another scheduler is running this job
                    try
                    {
                        var otherSchedulers = new Quartz.Impl.StdSchedulerFactory().AllSchedulers
                            .Where( s => s.SchedulerName != runNowSchedulerName );

                        foreach ( var scheduler in otherSchedulers )
                        {
                            if ( scheduler.GetCurrentlyExecutingJobs().Where( j => j.JobDetail.Description == job.Id.ToString() &&
                                j.JobDetail.ConcurrentExectionDisallowed ).Any() )
                            {
                                // A job with that Id is already running and ConcurrentExectionDisallowed is true
                                System.Diagnostics.Debug.WriteLine( RockDateTime.Now.ToString() + $" Scheduler '{scheduler.SchedulerName}' is already executing job Id '{job.Id}' (name: {job.Name})" );
                                return;
                            }
                        }
                    }
                    catch { }

                    // create the quartz job and trigger
                    IJobDetail jobDetail = jobService.BuildQuartzJob( job );

                    var jobTrigger = TriggerBuilder.Create()
                        .WithIdentity( job.Guid.ToString(), job.Name )
                        .StartNow()
                        .Build();

                    // schedule the job
                    sched.ScheduleJob( jobDetail, jobTrigger );

                    // set up the listener to report back from the job when it completes
                    sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                    // start the scheduler
                    sched.Start();

                    // Wait 10secs to give job chance to start
                    System.Threading.Tasks.Task.Delay( new TimeSpan( 0, 0, 10 ) ).Wait();

                    // stop the scheduler when done with job
                    sched.Shutdown( true );
                }
            }
        }

        private int testJobId;

        private Dictionary<string, string> GetJobDataMapDictionary( TestResultType resultType, string expectedExceptionMessage )
        {
            return new Dictionary<string, string> {
                { TestJobAttributeKey.ExecutionResult, resultType.ConvertToInt().ToString() },
                { TestJobAttributeKey.ExecutionMessage, expectedExceptionMessage }
            };
        }

        public ServiceJob GetAddTestJob( Dictionary<string, string> jobDataMapDictionary, JobNotificationStatus jobNotificationStatus = JobNotificationStatus.None )
        {
            ServiceJobService.UpdateAttributesIfNeeded( typeof( RockJobListenerTestJob ) );

            var testJob = new ServiceJob
            {
                IsSystem = true,
                IsActive = true,
                Name = "Test Job",
                Description = "This job is used for testing RockJobListener",
                Class = typeof( RockJobListenerTestJob ).FullName,
                Assembly = typeof(RockJobListenerTestJob).Assembly.FullName,
                CronExpression = "0 0 1 * * ?",
                NotificationStatus = jobNotificationStatus,
                Guid = TestJobGuidString.AsGuid(),
                NotificationEmails = "jobtest@spark.org"
            };

            testJob.LoadAttributes();
            foreach ( var kv in jobDataMapDictionary )
            {
                testJob.SetAttributeValue( kv.Key, kv.Value );
            }

            using ( var rockContext = new RockContext() )
            {
                var serviceJobService = new ServiceJobService( rockContext );

                var job = serviceJobService.Get( testJob.Guid );
                if ( job != null )
                {
                    testJobId = job.Id;
                    return job;
                }

                serviceJobService.Add( testJob );
                rockContext.SaveChanges();
                testJob.SaveAttributeValues();
            }

            testJobId = testJob.Id;
            return testJob;
        }

        public void RemoveTestJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var serviceJobService = new ServiceJobService( rockContext );
                var testJob = serviceJobService.Get( TestJobGuidString.AsGuid() );
                if ( testJob == null )
                {
                    return;
                }
                serviceJobService.Delete( testJob );
                rockContext.SaveChanges();
            }
        }

        private void StartEmailServer()
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName( "localhost" )
                .Port( smtpPort )
                .MessageStore( new SampleMessageStore() )
                .Build();

            var smtpServer = new SmtpServer.SmtpServer( options );
            smtpServer.StartAsync( CancellationToken.None );
        }

        private int GetFirstAvailablePort()
        {
            int port = 25;
            bool isAvailable = false;
            
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners().ToList();

            while ( !isAvailable )
            {
                isAvailable = !tcpConnInfoArray.Any( t => t.Port == port );
                if ( !isAvailable )
                {
                    port++;
                }
            }

            return port;
        }

        public class SampleMessageStore : MessageStore
        {
            public override Task<SmtpResponse> SaveAsync( ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken )
            {
                var textMessage = ( ITextMessage ) transaction.Message;
                var message = MimeKit.MimeMessage.Load( textMessage.Content );

                actualEmails.Push( message.HtmlBody );

                return Task.FromResult( SmtpResponse.Ok );
            }
        }

        private void UpdateSmtpPortNumber(string smtpPort)
        {
            var smtp = new SMTP();

            if ( string.IsNullOrWhiteSpace( _originalSmtpPort ) )
            {
                _originalSmtpPort = smtp.Port.ToString();
            }

            if ( smtp.Port.ToString() != smtpPort )
            {
                smtp.AttributeValues["Port"].Value = smtpPort;

                smtp.SaveAttributeValues();
            }
        }

        private void UpdateSmtpServerNumber( string smtpServer )
        {
            var smtp = new SMTP();

            if ( string.IsNullOrWhiteSpace( _originalSmtpServer ) )
            {
                _originalSmtpServer = smtp.Server;
            }

            if ( smtp.Server != smtpServer )
            {
                smtp.AttributeValues["Server"].Value = smtpServer;

                smtp.SaveAttributeValues();
            }
        }
    }


}

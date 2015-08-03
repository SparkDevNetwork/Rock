// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using Quartz;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Summary description for JobListener
    /// </summary>
    public class RockJobListener : IJobListener
    {
        /// <summary>
        /// Get the name of the <see cref="IJobListener"/>.
        /// </summary>
        public string Name
        {
            get
            {
                return "RockJobListener";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobListener"/> class.
        /// </summary>
        public RockJobListener()
        {
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="IJobDetail"/>
        /// is about to be executed (an associated <see cref="ITrigger"/>
        /// has occurred).
        /// <para>
        /// This method will not be invoked if the execution of the Job was vetoed
        /// by a <see cref="ITriggerListener"/>.
        /// </para>
        /// </summary>
        /// <param name="context"></param>
        /// <seealso cref="JobExecutionVetoed(IJobExecutionContext)"/>
        public void JobToBeExecuted( IJobExecutionContext context )
        {
            StringBuilder message = new StringBuilder();

            // get job type id
            int jobId = context.JobDetail.Description.AsInteger();

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            if (job != null && job.Guid != Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid())
            {
                job.LastStatus = "Running";
                job.LastStatusMessage = "Started at " + RockDateTime.Now.ToString();
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="IJobDetail"/>
        /// was about to be executed (an associated <see cref="ITrigger"/>
        /// has occurred), but a <see cref="ITriggerListener"/> vetoed its
        /// execution.
        /// </summary>
        /// <param name="context"></param>
        /// <seealso cref="JobToBeExecuted(IJobExecutionContext)"/>
        public void JobExecutionVetoed( IJobExecutionContext context )
        {
        }

        /// <summary>
        /// Called by the <see cref="IScheduler"/> after a <see cref="IJobDetail"/>
        /// has been executed, and before the associated <see cref="Quartz.Spi.IOperableTrigger"/>'s
        /// <see cref="Quartz.Spi.IOperableTrigger.Triggered"/> method has been called.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        public void JobWasExecuted( IJobExecutionContext context, JobExecutionException jobException )
        {
            StringBuilder message = new StringBuilder();
            bool sendMessage = false;

            // get job type id
            int jobId = Convert.ToInt16( context.JobDetail.Description );

            // load job
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            // format the message
            message.Append( string.Format( "The job {0} ran for {1} seconds on {2}.  Below is the results:<p>", job.Name, context.JobRunTime.TotalSeconds, context.FireTimeUtc.Value.DateTime.ToLocalTime() ) );

            // if noticiation staus is all set flag to send message
            if ( job.NotificationStatus == JobNotificationStatus.All )
            {
                sendMessage = true;
            }

            // set last run date
            job.LastRunDateTime = RockDateTime.Now; // context.FireTimeUtc.Value.DateTime.ToLocalTime();

            // set run time
            job.LastRunDurationSeconds = Convert.ToInt32( context.JobRunTime.TotalSeconds );

            // set the scheduler name
            job.LastRunSchedulerName = context.Scheduler.SchedulerName;

            // determine if an error occured
            if ( jobException == null )
            {
                job.LastSuccessfulRunDateTime = job.LastRunDateTime;
                job.LastStatus = "Success";
                job.LastStatusMessage = string.Empty;

                message.Append( "Result: Success" );

                // determine if message should be sent
                if ( job.NotificationStatus == JobNotificationStatus.Success )
                {
                    sendMessage = true;
                }
            }
            else
            {
                Exception exceptionToLog = jobException;

                // drill down to the interesting exception
                while ( exceptionToLog is Quartz.SchedulerException && exceptionToLog.InnerException != null )
                {
                    exceptionToLog = exceptionToLog.InnerException;
                }

                // log the exception to the database
                ExceptionLogService.LogException( exceptionToLog, null );

                var summaryException = exceptionToLog;

                // put the exception into the status
                job.LastStatus = "Exception";

                AggregateException aggregateException = summaryException as AggregateException;
                if ( aggregateException != null && aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count == 1 )
                {
                    // if it's an aggregate, but there is only one, convert it to a single exception
                    summaryException = aggregateException.InnerExceptions[0];
                    aggregateException = null;
                }

                if ( aggregateException != null )
                {
                    var firstException = aggregateException.InnerExceptions.First();
                    job.LastStatusMessage = "One or more exceptions occurred. First Exception: " + firstException.Message;
                    summaryException = firstException;
                }
                else
                {
                    job.LastStatusMessage = summaryException.Message;
                }

                message.Append( "Result: Exception<p>Message:<br>" + job.LastStatusMessage );

                if ( summaryException.InnerException != null )
                {
                    job.LastStatusMessage += " (" + summaryException.InnerException.Message + ")";
                    message.Append( "<p>Inner Exception:<br>" + summaryException.InnerException.Message );
                }

                if ( job.NotificationStatus == JobNotificationStatus.Error )
                {
                    sendMessage = true;
                }
            }

            rockContext.SaveChanges();

            // send notification
            if ( sendMessage )
            {
                // TODO: implement email send once it's available 
            }
        }
    }
}
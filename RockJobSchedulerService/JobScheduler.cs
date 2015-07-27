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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Collections.Specialized;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Rock.Jobs;
using Rock.Model;
using Rock.Data;

namespace RockJobSchedulerService
{
    /// <summary>
    /// 
    /// </summary>
    partial class JobScheduler : ServiceBase
    {
        // global Quartz scheduler for jobs
        IScheduler sched = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobScheduler"/> class.
        /// </summary>
        public JobScheduler()
        {
            this.ServiceName = "Rock Job Scheduler Service";
            this.EventLog.Log = "Application";

            InitializeComponent();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart( string[] args )
        {
            StartJobScheduler();
        }

        /// <summary>
        /// Starts the job scheduler.
        /// </summary>
        public void StartJobScheduler()
        {
            if ( !System.IO.File.Exists( "web.connectionstrings.config" ) )
            {
                // Write an eventlog about web.connectionstring.config not found
                this.EventLog.WriteEntry( "Unable to find web.connectionstrings.config", EventLogEntryType.Error );
            }

            ISchedulerFactory sf;

            // create scheduler
            sf = new StdSchedulerFactory();
            sched = sf.GetScheduler();

            var rockContext = new RockContext();

            // get list of active jobs
            ServiceJobService jobService = new ServiceJobService( rockContext );
            List<ServiceJob> activeJobs = null;
            try
            {
                // make sure that we can connect to the database and get the jobs list.  Write a good EventLog message and exit the app if we can't
                this.EventLog.WriteEntry( string.Format( "Connecting to database {0}:{1}", rockContext.Database.Connection.DataSource, rockContext.Database.Connection.Database ), EventLogEntryType.Information );
                rockContext.Database.Connection.Open();
                activeJobs = jobService.GetActiveJobs().ToList();
            }
            catch ( Exception ex )
            {
                this.EventLog.WriteEntry( "Unable load active jobs list. " + ex.Message, EventLogEntryType.Error );
                throw ex;
            }

            foreach ( ServiceJob job in activeJobs )
            {
                const string errorLoadingStatus = "Error Loading Job";
                try
                {
                    IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                    ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                    sched.ScheduleJob( jobDetail, jobTrigger );

                    //// if the last status was an error, but we now loaded successful, clear the error
                    // also, if the last status was 'Running', clear that status because it would have stopped if the app restarted
                    if ( job.LastStatus == errorLoadingStatus || job.LastStatus == "Running" )
                    {
                        job.LastStatusMessage = string.Empty;
                        job.LastStatus = string.Empty;
                        rockContext.SaveChanges();
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );
                         
                     // get path to the services directory
                    string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    path = System.IO.Path.GetDirectoryName( path );
                
                    // create the error message
                    string message = string.Format( "Error loading the job: {0}.\n\n{1}\n\n Job Assembly: {2}, Path: {3}", job.Name, ex.Message, job.Assembly, path );
                    job.LastStatusMessage = message;
                    job.LastStatus = errorLoadingStatus;
                    rockContext.SaveChanges();
                }

                // set up the listener to report back from jobs as they complete
                sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                // start the scheduler
                sched.Start();

            }

            // set up the listener to report back from jobs as they complete
            sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

            // start the scheduler
            sched.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            if ( sched != null )
            {
                sched.Shutdown();
            }
        }
    }
}

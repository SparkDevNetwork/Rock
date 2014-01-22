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

namespace RockJobSchedulerService
{
    partial class JobScheduler : ServiceBase
    {
        // global Quartz scheduler for jobs
        IScheduler sched = null;
        
        public JobScheduler()
        {
            this.ServiceName = "Rock Job Scheduler Service";
            this.EventLog.Log = "Application";
            
            InitializeComponent();
        }

        protected override void OnStart( string[] args )
        {
            ISchedulerFactory sf;

            // create scheduler
            sf = new StdSchedulerFactory();
            sched = sf.GetScheduler();

            // get list of active jobs
            ServiceJobService jobService = new ServiceJobService();
            foreach ( ServiceJob job in jobService.GetActiveJobs().ToList() )
            {
                try
                {

                    IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                    ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                    sched.ScheduleJob( jobDetail, jobTrigger );

                }
                catch ( Exception ex )
                {
                    // get path to the services directory
                    String path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    path = System.IO.Path.GetDirectoryName(path);

                    // create a friendly error message
                    string message = string.Format("Error loading the job: {0}.  Ensure that the correct version of the job's assembly ({1}.dll) in the services directory ({2}) of your server.", job.Name, job.Assembly, path);
                    message = message + "\n\n\n\n" + ex.Message;
                    //throw new JobLoadFailedException( message );
                    job.LastStatusMessage = message;
                    job.LastStatus = "Error Loading Job";

                    jobService.Save( job, null );
                }
            }

            // set up the listener to report back from jobs as they complete
            sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

            // start the scheduler
            sched.Start();

        }

        protected override void OnStop()
        {
            if ( sched != null )
                sched.Shutdown();
        }
    }
}

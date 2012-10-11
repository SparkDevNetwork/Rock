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
using Rock.Util;

namespace RockJobSchedulerService
    
    partial class JobScheduler : ServiceBase
        
        // global Quartz scheduler for jobs
        IScheduler sched = null;
        
        public JobScheduler()
            
            this.ServiceName = "Rock Job Scheduler Service";
            this.EventLog.Log = "Application";
            
            InitializeComponent();
        }

        protected override void OnStart( string[] args )
            
            ISchedulerFactory sf;

            // create scheduler
            sf = new StdSchedulerFactory();
            sched = sf.GetScheduler();

            // get list of active jobs
            JobService jobService = new JobService();
            foreach ( Job job in jobService.GetActiveJobs().ToList() )
                
                try
                    

                    IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                    ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                    sched.ScheduleJob( jobDetail, jobTrigger );

                }
                catch ( Exception ex )
                    
                    // get path to the services directory
                    String path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    path = System.IO.Path.GetDirectoryName(path);

                    // create a friendly error message
                    string message = string.Format("Error loading the job:     0}.  Ensure that the correct version of the job's assembly (    1}.dll) in the services directory (    2}) of your server.", job.Name, job.Assemby, path);
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
            
            if ( sched != null )
                sched.Shutdown();
        }
    }
}

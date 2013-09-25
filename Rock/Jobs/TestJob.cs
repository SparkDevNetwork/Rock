//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using Quartz;
using Rock.Attribute;
using Rock.Web.UI;

namespace Rock.Jobs
{
    
    /// <summary>
    /// Job to keep a heartbeat of the job process so we know when the jobs stop working
    /// </summary>
    /// <author>Jon Edmiston</author>
    /// <author>Spark Development Network</author>

    [TextField( "Domain", "Domain name of your SMTP server", true, "smtp.yourdomain.com")]
    [TextField( "Port", "Port of the email server", true, "25" )]
    public class TestJob : IJob
    {
        
        
        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public TestJob()
        {
        }
        
        /// <summary> 
        /// Job that updates the JobPulse setting with the current date/time.
        /// This will allow us to notify an admin if the jobs stop running.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void  Execute(IJobExecutionContext context)
        {
            
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string key1 = dataMap.GetString( "Domain" );
            string key2 = dataMap.GetString( "EmailServerPort" );
            
            // I don't do much
        }

    }
}
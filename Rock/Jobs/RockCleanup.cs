//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Jobs
{
    
    /// <summary>
    /// Job to keep a heartbeat of the job process so we know when the jobs stop working
    /// </summary>
    /// <author>Jon Edmiston</author>
    /// <author>Spark Development Network</author>
    [IntegerField("Hours to Keep Unconfirmed Accounts", "The number of hours to keep user accounts that have not been confirmed (default is 48 hours.)", false, 48, "General", 0, "HoursKeepUnconfirmedAccounts" )]
    [IntegerField("Days to Keep Exceptions in Log", "The number of days to keep exceptions in the exception log (default is 14 days.)", false, 14, "General", 1,"DaysKeepExceptions" )]
    public class RockCleanup : IJob
    {        
        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RockCleanup()
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
            
            // get the job map
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // delete accounts that have not been confirmed in X hours
            int userExpireHours = Int32.Parse( dataMap.GetString( "HoursKeepUnconfirmedAccounts" ) );
            DateTime userAccountExpireDate = DateTime.Now.Add( new TimeSpan( userExpireHours * -1,0,0 ) );

            var userLoginService = new UserLoginService();

            foreach (var user in userLoginService.Queryable().Where(u => u.IsConfirmed == false && u.CreationDateTime < userAccountExpireDate))
            {
                userLoginService.Delete( user, null );
            }

            userLoginService.Save( null, null );

            // purge exception log
            int exceptionExpireDays = Int32.Parse( dataMap.GetString( "DaysKeepExceptions" ) );
            DateTime exceptionExpireDate = DateTime.Now.Add( new TimeSpan( userExpireHours * -1, 0, 0 ) );

            ExceptionLogService exceptionLogService = new ExceptionLogService();

            foreach ( var exception in exceptionLogService.Queryable().Where( e => e.ExceptionDateTime < exceptionExpireDate ) )
            {
                exceptionLogService.Delete( exception, null );
            }

            exceptionLogService.Save( null, null );
        }

    }
}
#region License
/* 
 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */
#endregion

using System;
using System.Web;
using System.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Quartz;
using Rock.CMS;
using Rock.Core;

namespace Rock.Jobs
{
	
	/// <summary>
	/// Job to keep a heartbeat of the job process so we know when the jobs stop working
	/// </summary>
	/// <author>Jon Edmiston</author>
    /// <author>Spark Development Network</author>

    [Rock.Attribute.Property( 0, "Hours to Keep Unconfirmed Accounts", "HoursKeepUnconfirmedAccounts", "General", "The number of hours to keep user accounts that have not been confirmed (default is 48 hours.)",  false, "48", "Rock", "Rock.FieldTypes.Integer" )]
    [Rock.Attribute.Property( 0, "Days to Keep Exceptions in Log", "DaysKeepExceptions", "General", "The number of days to keep exceptions in the exception log (default is 14 days.)", false, "14", "Rock", "Rock.FieldTypes.Integer" )]
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

            UserRepository userRepository = new UserRepository();

            foreach (var user in userRepository.AsQueryable().Where(u => u.IsConfirmed == false && u.CreationDate < userAccountExpireDate))
            {
                userRepository.Delete( user, null );
            }

            userRepository.Save( null, null );

            // purge exception log
            int exceptionExpireDays = Int32.Parse( dataMap.GetString( "DaysKeepExceptions" ) );
            DateTime exceptionExpireDate = DateTime.Now.Add( new TimeSpan( userExpireHours * -1, 0, 0 ) );

            ExceptionLogRepository exceptionLogRepository = new ExceptionLogRepository();

            foreach ( var exception in exceptionLogRepository.AsQueryable().Where( e => e.ExceptionDate < exceptionExpireDate ) )
            {
                exceptionLogRepository.Delete( exception, null );
            }

            exceptionLogRepository.Save( null, null );
		}

	}
}
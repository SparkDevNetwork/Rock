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
using System.Linq;

using Quartz;

using Rock.Core;

namespace Rock.Jobs
{
	/// <summary>
	/// Job to keep a heartbeat of the job process so we know when the jobs stop working
	/// </summary>
	/// <author>Jon Edmiston</author>
    /// <author>Spark Development Network</author>
    public class JobPulse : IJob
	{
        
        /// <summary> 
		/// Empty constructor for job initilization
		/// <para>
		/// Jobs require a public empty constructor so that the
		/// scheduler can instantiate the class whenever it needs.
		/// </para>
		/// </summary>
		public JobPulse()
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

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                AttributeRepository attribRepository = new AttributeRepository();
                AttributeValueRepository attributeValueRepository = new AttributeValueRepository();

                Rock.Core.Attribute jobPulseAttrib = attribRepository.GetGlobalAttribute( "JobPulse" );
                Rock.Core.AttributeValue jobPulseAttribValue = jobPulseAttrib.AttributeValues.FirstOrDefault();

                // create attribute value if one does not exist
                if ( jobPulseAttribValue == null )
                {
                    jobPulseAttribValue = new AttributeValue();
                    jobPulseAttribValue.AttributeId = jobPulseAttrib.Id;
                    attributeValueRepository.Add( jobPulseAttribValue, null );
                }

                // store todays date and time
                jobPulseAttribValue.Value = DateTime.Now.ToString();

                // save attribute
                attributeValueRepository.Save( jobPulseAttribValue, null );
            }
		}

	}
}
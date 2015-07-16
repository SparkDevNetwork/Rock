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
using System.Web.Compilation;

using Quartz;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ServiceJob"/> entity objects.
    /// </summary>
    public partial class ServiceJobService 
    {
        /// <summary>
        /// Returns a queryable collection of active <see cref="Rock.Model.ServiceJob">Jobs</see>
        /// </summary>
        /// <returns>A queryable collection that contains all active <see cref="Rock.Model.ServiceJob">Jobs</see></returns>
        public IQueryable<ServiceJob> GetActiveJobs()
        {
            return Queryable().Where( t => t.IsActive == true );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJob">Jobs</see>
        /// </summary>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJob"/>Jobs</returns>
        public IQueryable<ServiceJob> GetAllJobs()
        {
            return Queryable();
        }

        /// <summary>
        /// Builds a Quartz Job for a specified <see cref="Rock.Model.ServiceJob">Job</see>
        /// </summary>
        /// <param name="job">The <see cref="Rock.Model.ServiceJob"/> to create a Quarts Job for.</param>
        /// <returns>A object that implements the <see cref="Quartz.IJobDetail"/> interface</returns>
        public IJobDetail BuildQuartzJob( ServiceJob job )
        {
            // build the type object, will depend if the class is in an assembly or the App_Code folder
            Type type = null;
            
            if ( !string.IsNullOrWhiteSpace(job.Assembly) )
            {
                // first, if no assembly is known, look in all the dlls for it
                type = Rock.Reflection.FindType( typeof( Quartz.IJob ), job.Class );

                if (type == null)
                {
                    // if it can't be found in dlls, look in App_Code using BuildManager
                    type = BuildManager.GetType( job.Class, false );
                }
            }
            else
            {
                // if an assembly is specified, load the type from that
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assembly );
                type = Type.GetType( thetype );
            }

            // load up job attributes (parameters) 
            job.LoadAttributes();

            JobDataMap map = new JobDataMap();

            foreach ( var attrib in job.AttributeValues )
            {
                map.Add( attrib.Key, attrib.Value.Value );
            }

            // create the quartz job object
            IJobDetail jobDetail = JobBuilder.Create( type )
            .WithDescription( job.Id.ToString() )
            .WithIdentity( job.Guid.ToString(), job.Name )
            .UsingJobData( map )
            .Build();

            return jobDetail;
        }

        /// <summary>
        /// Builds a Quartz schedule trigger
        /// </summary>
        /// <param name="job">The <see cref="Rock.Model.ServiceJob">Job</see> to create a <see cref="Quartz.ITrigger"/> compatible Trigger.</param>
        /// <returns>A Quartz trigger that implements <see cref="Quartz.ITrigger"/> for the specified job.</returns>
        public ITrigger BuildQuartzTrigger( ServiceJob job )
        {
            // create quartz trigger
            ITrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithIdentity( job.Guid.ToString(), job.Name )
                .WithCronSchedule( job.CronExpression, x => {
                    x.InTimeZone( RockDateTime.OrgTimeZoneInfo );
                    x.WithMisfireHandlingInstructionDoNothing();
                } )
                .StartNow()
                .Build();

            return trigger;
        }
    }
}

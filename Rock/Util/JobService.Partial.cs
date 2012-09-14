//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;

using Quartz;

using Rock.Data;

namespace Rock.Util
{
	/// <summary>
	/// Job POCO Service class
	/// </summary>
    public partial class JobService : Service<Job, JobDto>
    {
        /// <summary>
        /// Gets the active jobs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Job> GetActiveJobs()
        {
            return Repository.Find( t => t.IsActive == true );
        }

        /// <summary>
        /// Builds the quartz job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        public IJobDetail BuildQuartzJob( Job job )
        {
            // build the type object, will depend if the class is in an assembly or the App_Code folder
            Type type = null;
            if ( job.Assemby == string.Empty || job.Assemby == null )
            {
                type = BuildManager.GetType( job.Class, false );
            }
            else
            {
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assemby );
                type = Type.GetType( thetype );
            }

            // create attributes if needed 
            // TODO: next line should be moved to Job creation UI, when it's created
            Rock.Attribute.Helper.UpdateAttributes( type, "Job", "Class", job.Class, null );

            // load up job attributes (parameters) 
            Rock.Attribute.Helper.LoadAttributes( job );

            JobDataMap map = new JobDataMap();

            foreach ( KeyValuePair<string, KeyValuePair<string, List<Rock.Core.AttributeValueDto>>> attrib in job.AttributeValues )
            {
                map.Add( attrib.Key, attrib.Value.Value[0].Value );
            }

            // create the quartz job object
            IJobDetail jobDetail = JobBuilder.Create( type )
            .WithDescription( job.Id.ToString() )
            .WithIdentity( new Guid().ToString(), job.Name )
            .UsingJobData( map )
            .Build();

            return jobDetail;
        }

        /// <summary>
        /// Builds the quartz trigger.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        public ITrigger BuildQuartzTrigger( Job job )
        {
            // create quartz trigger
            ITrigger trigger = ( ICronTrigger )TriggerBuilder.Create()
                .WithIdentity( new Guid().ToString(), job.Name )
                .WithCronSchedule( job.CronExpression )
                .StartNow()
                .Build();

            return trigger;
        }
    }
}

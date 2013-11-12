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

namespace Rock.Model
{
    /// <summary>
    /// Job POCO Service class
    /// </summary>
    public partial class ServiceJobService 
    {
        /// <summary>
        /// Gets the active jobs.
        /// </summary>
        /// <returns></returns>
        public IQueryable<ServiceJob> GetActiveJobs()
        {
            return Repository.AsQueryable().Where( t => t.IsActive == true );
        }

        /// <summary>
        /// Gets all jobs.
        /// </summary>
        /// <returns></returns>
        public IQueryable<ServiceJob> GetAllJobs()
        {
            return Repository.AsQueryable();
        }

        /// <summary>
        /// Builds the quartz job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        public IJobDetail BuildQuartzJob( ServiceJob job )
        {
            // build the type object, will depend if the class is in an assembly or the App_Code folder
            Type type = null;
            if ( job.Assembly == string.Empty || job.Assembly == null )
            {
                type = BuildManager.GetType( job.Class, false );
            }
            else
            {
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assembly );
                type = Type.GetType( thetype );
            }

            // create attributes if needed 
            // TODO: next line should be moved to Job creation UI, when it's created
            int? jobEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( "Rock.Model.ServiceJob" ).Id;
            using ( new UnitOfWorkScope() )
            {
                Rock.Attribute.Helper.UpdateAttributes( type, jobEntityTypeId, "Class", job.Class, null );
            }

            // load up job attributes (parameters) 
            job.LoadAttributes();

            JobDataMap map = new JobDataMap();

            foreach ( KeyValuePair<string, List<Rock.Model.AttributeValue>> attrib in job.AttributeValues )
            {
                map.Add( attrib.Key, attrib.Value[0].Value );
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
        public ITrigger BuildQuartzTrigger( ServiceJob job )
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

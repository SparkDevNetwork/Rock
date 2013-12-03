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
            return Repository.AsQueryable().Where( t => t.IsActive == true );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJob">Jobs</see>
        /// </summary>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJob"/>Jobs</returns>
        public IQueryable<ServiceJob> GetAllJobs()
        {
            return Repository.AsQueryable();
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
            if ( job.Assembly == string.Empty || job.Assembly == null )
            {
                type = BuildManager.GetType( job.Class, false );
            }
            else
            {
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assembly );
                type = Type.GetType( thetype );
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
        /// Builds a Quartz schedule trigger
        /// </summary>
        /// <param name="job">The <see cref="Rock.Model.ServiceJob">Job</see> to create a <see cref="Quartz.ITrigger"/> compatible Trigger.</param>
        /// <returns>A Quartz trigger that implements <see cref="Quartz.ITrigger"/> for the specified job.</returns>
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

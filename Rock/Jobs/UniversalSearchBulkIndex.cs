// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisallowConcurrentExecution]
    public class UniversalSearchBulkIndex : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UniversalSearchBulkIndex()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            Stopwatch stopwatch;

            RockContext rockContext = new RockContext();

            var entityTypes = new EntityTypeService( rockContext )
                                .Queryable().AsNoTracking().ToList();

            var indexableEntityTypes = entityTypes.Where( e => 
                                            e.IsIndexingSupported == true 
                                            && e.IsIndexingEnabled == true )
                                        .ToList();

            StringBuilder results = new StringBuilder();

            foreach(var entityType in indexableEntityTypes )
            {
                Type type = Type.GetType( entityType.AssemblyName );

                if ( type != null )
                {
                    object classInstance = Activator.CreateInstance( type, null );
                    MethodInfo bulkItemsMethod = type.GetMethod( "BulkIndexDocuments" );

                    if ( classInstance != null && bulkItemsMethod != null )
                    {

                        stopwatch = Stopwatch.StartNew();
                        bulkItemsMethod.Invoke( classInstance, null );
                        stopwatch.Stop();

                        results.Append( string.Format(" {0} in {1}s,", entityType.FriendlyName, stopwatch.ElapsedMilliseconds/1000 ) );
                    }
                }
            }

            context.Result = results.ToString().TrimEnd(',');
        }

    }
}

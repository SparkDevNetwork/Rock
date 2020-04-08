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

using Quartz;

using Rock.Attribute;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [ComponentField( "Rock.Pbx.PbxContainer, Rock", "PBX Component", "The PBX type to process.", true, key:"PbxComponent" )]
    [DisallowConcurrentExecution]
    public class PbxCdrDownload : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public PbxCdrDownload()
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

            // get the selected provider
            var componentType = dataMap.GetString( "PbxComponent" );
            var provider = Pbx.PbxContainer.GetComponent( componentType );

            if (provider == null )
            {
                context.Result = "Could not find Component Type";
                return;
            }

            var lastProcessedKey = string.Format( "pbx-cdr-download-{0}", provider.TypeName.ToLower().Replace( ' ', '-' ) );
            var lastProcessedDate = Rock.Web.SystemSettings.GetValue( lastProcessedKey ).AsDateTime();

            if ( !lastProcessedDate.HasValue )
            {
                lastProcessedDate = new DateTime(2000, 1, 1); // if first run use 1/1/2000
            }

            bool downloadSuccessful = false;
            context.Result = provider.DownloadCdr( out downloadSuccessful, lastProcessedDate );

            if ( downloadSuccessful )
            {
                Rock.Web.SystemSettings.SetValue( lastProcessedKey, RockDateTime.Now.ToString() );
            }

        }

    }
}

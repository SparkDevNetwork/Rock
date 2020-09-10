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
using System.ComponentModel;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V10.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Populates the new RelatedDataViewId field with data." )]
    [Description( "This job will populate the new RelatedDataViewId field in the DataView table which was added as part of v11.0. After all the operations are done, this job will delete itself." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Attribute Values, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV110DataMigrationsPopulateRelatedDataViewId : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                UpdateDataViewFiltersWithGuids( rockContext );
                UpdateDataViewFiltersWithIds( rockContext );
            }

            DeleteJob( context.GetJobId() );
        }

        private void UpdateDataViewFiltersWithGuids( RockContext rockContext )
        {
            rockContext.Database.ExecuteSqlCommand( @"
                UPDATE DataViewFilter
                SET RelatedDataViewId = DataView.Id
                FROM DataView
                INNER JOIN (
	                SELECT Id
		                , RelatedDataViewId
		                , CASE WHEN CHARINDEX(',', Selection, 0) > 0 
			                    THEN SUBSTRING(Selection, 0, CHARINDEX(',', Selection, 0)) 
		                    WHEN CHARINDEX('|', Selection, 0) > 0 
			                    THEN SUBSTRING(Selection, 0, CHARINDEX('|', Selection, 0)) 
		                    ELSE Selection 
			                    END AS RelatedDataViewGuid
	                FROM DataViewFilter
	                WHERE Selection LIKE REPLACE('00000000-0000-0000-0000-000000000000', '0', '[0-9a-fA-F]') + '%'
                ) AS rdv ON rdv.RelatedDataViewGuid = DataView.[Guid]
                WHERE DataViewFilter.Id = rdv.Id
            " );
        }

        private void UpdateDataViewFiltersWithIds( RockContext rockContext )
        {
            rockContext.Database.ExecuteSqlCommand( @"
                UPDATE DataViewFilter
                SET RelatedDataViewId = DataView.Id
                FROM DataView
                INNER JOIN (
	                SELECT Id
		                , SUBSTRING(Selection,
			                LEN('{""DataViewId"":') + 1, 
			                CHARINDEX(',', Selection, CHARINDEX('{""DataViewId"":', Selection, 0)) - LEN('{""DataViewId"":') - 1) AS RelatedDataViewId
	                FROM DataViewFilter
	                WHERE Selection LIKE '{""DataViewId"":%'
                ) AS rdv ON rdv.RelatedDataViewId = DataView.Id
                WHERE DataViewFilter.Id = rdv.Id
            " );
        }
        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                    return;
                }
            }
        }

    }
}
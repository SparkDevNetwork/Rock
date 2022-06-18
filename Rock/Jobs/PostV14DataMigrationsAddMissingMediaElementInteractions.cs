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


using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.ComponentModel;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v14 to update missing media element interactions
    /// </summary>
    [Quartz.DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v14.0 - Add missing Media Element interactions." )]
    [Description( "This job will update the interation length of media element interactions. After all the operations are done, this job will delete itself." )]

    [IntegerField(
    "Command Timeout",
    AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 60 * 60 )]
    public class PostV14DataMigrationsAddMissingMediaElementInteractions:  RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute( RockJobContext context )
        {
            // RockJobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                rockContext.Database.ExecuteSqlCommand( @"
WHILE (
    EXISTS (
        SELECT 1
        FROM [Interaction] i
        INNER JOIN [InteractionComponent] AS ico ON ico.[Id] = i.[InteractionComponentId]
        INNER JOIN [InteractionChannel] AS ich ON ich.[Id] = ico.[InteractionChannelId]
        WHERE
            ich.[Guid] = 'D5B9BDAF-6E52-40D5-8E74-4E23973DF159'
            AND i.[InteractionLength] IS NULL
    )
)
BEGIN
    UPDATE TOP (1000) i SET
        i.[InteractionLength] = JSON_VALUE(i.[InteractionData],'$.WatchedPercentage')
    FROM
        [Interaction] AS i
        INNER JOIN [InteractionComponent] AS ico ON ico.[Id] = i.[InteractionComponentId]
        INNER JOIN [InteractionChannel] AS ich ON ich.[Id] = ico.[InteractionChannelId]
    WHERE
        ich.[Guid] = 'D5B9BDAF-6E52-40D5-8E74-4E23973DF159'
        AND i.[InteractionLength] IS NULL
END
" );
            }

            DeleteJob( context.GetJobId() );
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
                }
            }
        }
    }
}

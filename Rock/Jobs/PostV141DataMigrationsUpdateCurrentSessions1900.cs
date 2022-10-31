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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v14.1 to update current sessions
    /// </summary>
    [DisplayName( "Rock Update Helper v14.1 - Update current sessions that might have 1900-01-01 set as the DurationLastCalculatedDateTime." )]
    [Description( "This job will update the current sessions to have the duration of the session as well as the interaction count." )]

    [IntegerField(
    "Command Timeout",
    AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of interactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = AttributeDefault.CommandTimeout )]
    public class PostV141DataMigrationsUpdateCurrentSessions1900 : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        private static class AttributeDefault
        {
            // Set Default to 4 hours, just in case
            public const int CommandTimeout = 14400;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public override void Execute()
        {
            var commandTimeout = this.GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefault.CommandTimeout;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                // Update current sessions that might have 1900-01-01 set as the DurationLastCalculatedDateTime
                rockContext.Database.ExecuteSqlCommand( "update InteractionSession set DurationLastCalculatedDateTime = null where DurationLastCalculatedDateTime = '1900-01-01'" );

                // Now that we cleaned up any possible 1900-01-01's, recalculate DurationSeconds, InteractionCount and DurationLastCalculatedDateTime that
                // hadn't been updated due to the 1900-01-01 issue.
                rockContext.Database.ExecuteSqlCommand( @"
UPDATE xs
SET xs.[DurationSeconds] = sq.[DurationSeconds]
    , xs.[InteractionCount] = sq.[InteractionCount]
    , xs.[DurationLastCalculatedDateTime] = GETDATE() 
FROM [InteractionSession] xs
INNER JOIN (
        SELECT 
            s.[Id] AS [SessionId]
            , COUNT( i.[Id] ) AS [InteractionCount]
            , CASE WHEN COUNT( i.[Id] ) = 1 THEN 60
                ELSE DATEDIFF(s,MIN(i.[InteractionDateTime]), MAX(i.[InteractionDateTime]) ) + 60
            END AS [DurationSeconds]
        FROM [InteractionSession] s
            INNER JOIN [Interaction] i ON i.[InteractionSessionId] = s.[Id]
            INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
            INNER JOIN [InteractionChannel] ich ON ich.[Id] = ic.[InteractionChannelId]
            INNER JOIN [DefinedValue] mdv ON mdv.[Id] = ich.[ChannelTypeMediumValueId] 
        WHERE
            mdv.[Guid] = 'e503e77d-cf35-e09f-41a2-b213184f48e8'
            AND s.[InteractionSessionLocationId] IS NULL
        GROUP BY s.[Id]
) AS sq ON sq.[SessionId] = xs.[Id]
" );
            }

            DeleteJob( this.GetJobId() );
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

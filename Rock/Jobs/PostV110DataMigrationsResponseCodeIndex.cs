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
    [DisplayName( "Does Post-V11 Migration for Communication Recipient ResponseCode Index" )]
    [Description( "This job will update Communication Recipient ResponseCode to have an index on ReponseCode and CreatedDateTime." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with Communication Recipients, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV110DataMigrationsResponseCodeIndex : IJob
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

                // shouldn't happen since core doesn't put anything bigger than '@99999' in here,
                // but just in case there are ResponseCodes longer than 6, remove them
                rockContext.Database.ExecuteSqlCommand( @"UPDATE CommunicationRecipient
SET ResponseCode = NULL
WHERE ResponseCode IS NOT NULL AND len(ResponseCode) > 6" );

                // This could take a several minutes or more on a large database.
                // Any queries on CommunciationRecipient during this time may timeout
                rockContext.Database.ExecuteSqlCommand( "ALTER TABLE CommunicationRecipient ALTER COLUMN ResponseCode NVARCHAR(6)" );

                // Creating an index will take some time too, but but only a couple of minutes
                rockContext.Database.ExecuteSqlCommand( @"IF NOT EXISTS (
			SELECT *
			FROM sys.indexes
			WHERE name = 'IX_ResponseCodeCreatedDateTime'
				AND object_id = OBJECT_ID('CommunicationRecipient')
			)
	BEGIN
		CREATE INDEX IX_ResponseCodeCreatedDateTime ON CommunicationRecipient (ResponseCode, CreatedDateTime)
	END" );
                
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
                    return;
                }
            }
        }

    }
}
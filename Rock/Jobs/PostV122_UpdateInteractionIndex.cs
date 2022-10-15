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
    /// A run once job for V11.0
    /// </summary>
    [DisplayName( "Rock Update Helper v12.2 - Adds PersonalDeviceId to Interaction Index." )]
    [Description( "This job will update the index. After all the operations are done, this job will delete itself." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Interactions, this could take several hours or more.",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout )]

    public class PostV122_UpdateInteractionIndex : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        private static class AttributeDefaultValue
        {
            public const int CommandTimeout = 60 * 60 * 2; // 2 hours
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            /* MDP 07-22-2021

               NOTE: We intentionally are excluding this from DataMigrationStartup and will just wait for it to run at 2am.
               See https://app.asana.com/0/0/1199506067368201/f

             */

            // get the configured timeout, or default if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaultValue.CommandTimeout;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var keys = new [] { nameof( Interaction.InteractionComponentId ), nameof( Interaction.InteractionDateTime ) };
                var tableName = nameof( Interaction );

                // Drop Old Index
                var oldIndexName = MigrationIndexHelper.GenerateIndexName( keys );
                var dropOldIndexSql = MigrationIndexHelper.GenerateDropIndexIfExistsSql( tableName, oldIndexName );
                rockContext.Database.ExecuteSqlCommand( dropOldIndexSql );

                // Create new index and include personaldevice id
                var includes = new[]
                {
                    nameof( Interaction.InteractionTimeToServe ),
                    nameof( Interaction.Operation ),
                    nameof( Interaction.InteractionSessionId ),
                    nameof( Interaction.PersonalDeviceId )
                };
                var createIndexSql = MigrationIndexHelper.GenerateCreateIndexIfNotExistsSql( tableName, keys, includes );
                rockContext.Database.ExecuteSqlCommand( createIndexSql );
            }

            // This is a one time job
            DeleteJob( this.ServiceJobId );
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
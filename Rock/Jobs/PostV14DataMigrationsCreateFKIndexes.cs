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
    /// Run once job for v14 to update current sessions
    /// </summary>
    [DisplayName( "Rock Update Helper v14.0 - Add FK indexes" )]
    [Description( "This job will add FK indexes on RegistrationRegistrant.RegistrationTemplateId, GroupMember.GroupTypeId, and ConnectionRequest.ConnectionTypeId." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV14DataMigrationsCreateFKIndexes : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var migrationHelper = new MigrationHelper( new JobMigration( commandTimeout ) );

            migrationHelper.CreateIndexIfNotExists( "RegistrationRegistrant", new[] { "RegistrationTemplateId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "GroupMember", new[] { "GroupTypeId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "ConnectionRequest", new[] { "ConnectionTypeId" }, new string[0] );

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

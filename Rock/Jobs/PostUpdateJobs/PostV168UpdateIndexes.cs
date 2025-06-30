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
#if REVIEW_WEBFORMS
using DocumentFormat.OpenXml.Wordprocessing;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.ComponentModel;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v16.8 to update indexes for general database performance..
    /// </summary>
    [DisplayName( "Rock Update Helper v16.8 - Update Indexes." )]
    [Description( "This job updates indexes for general database performance." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV168UpdateIndexes : PostUpdateJobs.PostUpdateJob
    {
        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );

            AddFinancialTransactionForeignKeyFinancialGatewayIdIndex( migrationHelper );

            RemovePersonRecordTypeValueIdIndex( migrationHelper );
            AddPersonRecordTypeValueIdLastNameIndex( migrationHelper );

            RemoveGroupParentGroupIdIndex( migrationHelper );
            AddGroupParentGroupIdGroupTypeIdIndex( migrationHelper );

            RecreateInteractionInteractionSessionIdIndex( migrationHelper );

            DeleteJob();
        }

        /// <summary>
        /// Adds a new index for ForeignKey and FinancialGatewayId to the FinancialTransaction table.
        /// </summary>
        /// <param name="migrationHelper">The <see cref="MigrationHelper"/>.</param>
        private void AddFinancialTransactionForeignKeyFinancialGatewayIdIndex( MigrationHelper migrationHelper )
        {
            migrationHelper.DropIndexIfExists( "FinancialTransaction", "IX_ForeignKey_FinancialGatewayId" );
            migrationHelper.CreateIndexIfNotExists( "FinancialTransaction", "IX_ForeignKey_FinancialGatewayId", new[] { "ForeignKey", "FinancialGatewayId" }, Array.Empty<string>() );
        }

        /// <summary>
        /// Removes the RecordTypeValueId index from the Person table.
        /// </summary>
        /// <param name="migrationHelper"></param>
        private void RemovePersonRecordTypeValueIdIndex( MigrationHelper migrationHelper )
        {
            migrationHelper.DropIndexIfExists( "Person", "IX_RecordTypeValueId" );
        }

        /// <summary>
        /// Adds a new index for RecordTypeValueId and LastName to the Person table (includes Guid).
        /// </summary>
        /// <param name="migrationHelper">The <see cref="MigrationHelper"/>.</param>
        private void AddPersonRecordTypeValueIdLastNameIndex( MigrationHelper migrationHelper )
        {
            migrationHelper.DropIndexIfExists( "Person", "IX_RecordTypeValueId" );
            migrationHelper.CreateIndexIfNotExists( "Person", "IX_RecordTypeValueId_LastName", new[] { "RecordTypeValueId", "LastName" }, new[] { "Guid" } );
        }

        /// <summary>
        /// Removes the ParentGroupId index from the Group table.
        /// </summary>
        /// <param name="migrationHelper"></param>
        private void RemoveGroupParentGroupIdIndex( MigrationHelper migrationHelper )
        {
            migrationHelper.DropIndexIfExists( "Group", "IX_ParentGroupId" );
        }

        /// <summary>
        /// Adds a new index for ParentGroupId and GroupTypeId to the Group table (includes IsActive and IsArchived).
        /// </summary>
        /// <param name="migrationHelper">The <see cref="MigrationHelper"/>.</param>
        private void AddGroupParentGroupIdGroupTypeIdIndex( MigrationHelper migrationHelper )
        {
            migrationHelper.DropIndexIfExists( "Group", "IX_ParentGroupId_GroupTypeId" );
            migrationHelper.CreateIndexIfNotExists( "Group", "IX_ParentGroupId_GroupTypeId", new[] { "ParentGroupId", "GroupTypeId" }, new[] { "IsActive", "IsArchived" } );
        }

        /// <summary>
        /// Modifies the InteractionSessionId index on the Interaction table to include the InteractionDateTime field.
        /// </summary>
        /// <param name="migrationHelper">The <see cref="MigrationHelper"/>.</param>
        private void RecreateInteractionInteractionSessionIdIndex( MigrationHelper migrationHelper )
        {
            migrationHelper.DropIndexIfExists( "Interaction", "IX_InteractionSessionId" );
            migrationHelper.CreateIndexIfNotExists( "Interaction", "IX_InteractionSessionId", new[] { "InteractionSessionId" }, new[] { "InteractionDateTime" } );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}

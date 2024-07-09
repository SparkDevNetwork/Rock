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
namespace Rock.Migrations
{

    /// <summary>
    /// EF Rollup migrations for 06/20/2024
    /// </summary>
    public partial class Rollup_20240620 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MoveDocumentTypeBlocksToCoreDomainUp();
            AddRunOnceJobAddCommunicationRecipientTableIndex();
            GroupMemberCleanUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MoveDocumentTypeBlocksToCoreDomainDown();
        }

        #region JPH: Move Document Type Blocks to "Core" Domain

        /// <summary>
        /// JPH: Move document type blocks to core domain up.
        /// </summary>
        private void MoveDocumentTypeBlocksToCoreDomainUp()
        {
            Sql( $@"
UPDATE [BlockType]
SET [Category] = 'Core'
WHERE [Guid] IN (
    '5f3151bf-577d-485b-9ee3-90f3f86f5739'
    , 'fd3eb724-1afa-4507-8850-c3aee170c83b'
);" );
        }

        /// <summary>
        /// JPH: Move document type blocks to core domain down.
        /// </summary>
        private void MoveDocumentTypeBlocksToCoreDomainDown()
        {
            Sql( $@"
UPDATE [BlockType]
SET [Category] = 'CRM'
WHERE [Guid] IN (
    '5f3151bf-577d-485b-9ee3-90f3f86f5739'
    , 'fd3eb724-1afa-4507-8850-c3aee170c83b'
);" );
        }

        #endregion

        #region JPH: Add CommunicationRecipient Index

        /// <summary>
        /// JPH: Migration to Add Run Once Job to add an index to the CommunicationRecipient table.
        /// This goes along with commit https://github.com/SparkDevNetwork/Rock/commit/0b822733d45114a0e7fbe088b4c227bd63ce7af2.
        /// </summary>
        private void AddRunOnceJobAddCommunicationRecipientTableIndex()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v16.6 - Add CommunicationRecipient Index",
                description: "This job will add a new index to the CommunicationRecipient table.",
                jobType: "Rock.Jobs.PostV166AddCommunicationRecipientIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_166_ADD_COMMUNICATION_RECIPIENT_INDEX );
        }

        #endregion

        #region SK: Group Member Clean-up (data migration)

        private void GroupMemberCleanUp()
        {
            // Set GroupMember InactiveDateTime if they are inactive and have no current InactiveDateTime value (and similar with ArchivedDateTime).
            Sql( @"
UPDATE [GroupMember]
SET [InactiveDateTime] = CASE WHEN [ModifiedDateTime] IS NOT NULL THEN [ModifiedDateTime] ELSE GETDATE() END
WHERE [GroupMemberStatus] = 0 AND [InactiveDateTime] IS NULL 

UPDATE [GroupMember]
SET [ArchivedDateTime] = CASE WHEN [ModifiedDateTime] IS NOT NULL THEN [ModifiedDateTime] ELSE GETDATE() END
WHERE [IsArchived] = 1 AND [ArchivedDateTime] IS NULL
" );

            // Set Group InactiveDateTime if it is inactive and has no current InactiveDateTime value (and similar with ArchivedDateTime).
            Sql( @"
UPDATE [Group]
SET [InactiveDateTime] = CASE WHEN [ModifiedDateTime] IS NOT NULL THEN [ModifiedDateTime] ELSE GETDATE() END
WHERE [IsActive] = 0 AND [InactiveDateTime] IS NULL 

UPDATE [Group]
SET [ArchivedDateTime] = CASE WHEN [ModifiedDateTime] IS NOT NULL THEN [ModifiedDateTime] ELSE GETDATE() END
WHERE [IsArchived] = 1 AND [ArchivedDateTime] IS NULL
" );
        }

        #endregion
    }
}

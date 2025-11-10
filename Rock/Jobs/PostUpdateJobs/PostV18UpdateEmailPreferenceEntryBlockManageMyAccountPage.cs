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

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v18.0 to update block settings for recently-chopped Communication blocks.
    /// </summary>
    [DisplayName( "Rock Update Helper v18.0 - Update Block Settings for Recently-Chopped Communication Blocks" )]
    [Description( "This job will update block settings for recently-chopped Communication blocks." )]

    public class PostV18UpdateEmailPreferenceEntryBlockManageMyAccountPage : RockJob
    {
        /// <inheritdoc />
        public override void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobMigration = new JobMigration( rockContext );
                var migrationHelper = new MigrationHelper( jobMigration );

                #region Update Communication List Block Detail Page

                // Find the block instance.
                var communicationListBlockGuid = jobMigration.SqlScalar( $@"
DECLARE @CommunicationHistoryPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.COMMUNICATION_HISTORY}');
DECLARE @CommunicationListBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'c3544f53-8e2d-43d6-b165-8fefc541a4eb');

SELECT TOP 1 [Guid]
FROM [Block]
WHERE [PageId] = @CommunicationHistoryPageId
    AND [BlockTypeId] = @CommunicationListBlockTypeId;" )
                    .ToStringSafe()
                    .AsGuidOrNull();

                // Only perform the update if the block instance was found.
                if ( communicationListBlockGuid.HasValue )
                {
                    // Overwrite the block setting to link to the latest "New Communication" page.
                    migrationHelper.AddBlockAttributeValue(
                        skipIfAlreadyExists: false,
                        blockGuid: communicationListBlockGuid.Value.ToString(),
                        attributeGuid: "5209A318-9C53-43E4-9511-AAC595FC3684",
                        value: "9f7ae226-cc95-4e6a-b333-c0294a2024bc,79c0c1a7-41b6-4b40-954d-660a4b39b8ce"
                    );
                }

                #endregion Update Communication List Block Detail Page

                #region Update Email Preference Entry Block My Account Page

                // Find the block instance.
                var emailPreferenceEntryBlockGuid = jobMigration.SqlScalar( $@"
DECLARE @EmailPreferencePageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.EMAIL_PREFERENCE}');
DECLARE @EmailPreferenceEntryBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'FF88B243-4580-4BE0-97B1-BA9895C3FB8F');

SELECT TOP 1 [Guid]
FROM [Block]
WHERE [PageId] = @EmailPreferencePageId
    AND [BlockTypeId] = @EmailPreferenceEntryBlockTypeId;" )
                    .ToStringSafe()
                    .AsGuidOrNull();

                // Only perform the update if the block instance was found.
                if ( emailPreferenceEntryBlockGuid.HasValue )
                {
                    // Overwrite the block setting to link to the "My Account" page.
                    migrationHelper.AddBlockAttributeValue(
                       skipIfAlreadyExists: false,
                       blockGuid: emailPreferenceEntryBlockGuid.Value.ToString(),
                       attributeGuid: "0AB5FAA0-0C6C-449C-A0C1-5D3189B066D6",
                       value: "c0854f84-2e8b-479c-a3fb-6b47be89b795,ec68edc9-a8a5-4937-b78b-45f9bc278ee1"
                   );
                }

                #endregion Update Email Preference Entry Block My Account Page
            }

            DeleteJob();
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

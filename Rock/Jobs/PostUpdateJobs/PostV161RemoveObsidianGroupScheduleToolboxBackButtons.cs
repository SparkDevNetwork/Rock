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
using Rock.Web.Cache;

using System.ComponentModel;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// Run once job for v16.1 to remove Obsidian Group Schedule Toolbox Back Buttons.
    /// </summary>
    [DisplayName( "Rock Update Helper v16.1 - Remove Obsidian Group Schedule Toolbox Back Buttons" )]
    [Description( "This job removes the Back buttons from 3 Lava block settings within the Obsidian Group Schedule Toolbox block." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV161RemoveObsidianGroupScheduleToolboxBackButtons : PostUpdateJobs.PostUpdateJob
    {
        private const string BLOCK_TYPE_GUID = "6554ADE3-2FC8-482B-BA63-2C3EABC11D32";

        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                RemoveBackButtons( rockContext );
                FlushAttributesFromCache( rockContext );
                DeleteJob( rockContext );
            }
        }

        /// <summary>
        /// Removes the Back buttons from 3 Lava block settings in the database.
        /// </summary>
        private void RemoveBackButtons( RockContext rockContext )
        {
            var jobMigration = new JobMigration( rockContext );
            var migrationHelper = new MigrationHelper( jobMigration );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = migrationHelper.NormalizeColumnCRLF( "Value" );

            string lavaTemplate = @"<p>
    <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>
</p>";

            string newLavaTemplate = string.Empty;

            jobMigration.Sql( $@"
DECLARE @BlockTypeEntityTypeId [int] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block');
DECLARE @ObsidianGrpSchedToolboxBlockTypeId [int] = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{BLOCK_TYPE_GUID}');

UPDATE [Attribute]
SET [DefaultValue] = ''
    , [DefaultPersistedTextValue] = NULL
    , [DefaultPersistedHtmlValue] = NULL
    , [DefaultPersistedCondensedTextValue] = NULL
    , [DefaultPersistedCondensedHtmlValue] = NULL
    , [IsDefaultPersistedValueDirty] = 1
WHERE [Key] IN (
        'ScheduleUnavailabilityHeader'
        , 'UpdateSchedulePreferencesHeader'
        , 'SignupforAdditionalTimesHeader'
    )
    AND [EntityTypeId] = @BlockTypeEntityTypeId
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = @ObsidianGrpSchedToolboxBlockTypeId;

UPDATE [AttributeValue]
SET [Value] = REPLACE({targetColumn} ,'{lavaTemplate}','{newLavaTemplate}')
    , [PersistedTextValue] = NULL
    , [PersistedHtmlValue] = NULL
    , [PersistedCondensedTextValue] = NULL
    , [PersistedCondensedHtmlValue] = NULL
    , [IsPersistedValueDirty] = 1
WHERE [AttributeId] IN
(
    SELECT [Id]
    FROM [Attribute]
    WHERE [Key] IN (
            'ScheduleUnavailabilityHeader'
            , 'UpdateSchedulePreferencesHeader'
            , 'SignupforAdditionalTimesHeader'
        )
        AND [EntityTypeId] = @BlockTypeEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = @ObsidianGrpSchedToolboxBlockTypeId
);"
            );
        }

        /// <summary>
        /// Flushes the block type's attributes from cache.
        /// </summary>
        private void FlushAttributesFromCache( RockContext rockContext )
        {
            var blockTypeId = new AttributeService( rockContext ).GetId( BLOCK_TYPE_GUID.AsGuid() );
            if ( !blockTypeId.HasValue )
            {
                return;
            }

            AttributeCache.FlushAttributesForBlockType( blockTypeId.Value );
        }

        /// <summary>
        ///  Deletes the job.
        /// </summary>
        private void DeleteJob( RockContext rockContext )
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
